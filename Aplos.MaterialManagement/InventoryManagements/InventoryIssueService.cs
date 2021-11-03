using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
#region Using
using Syncfusion.DocIO.DLS;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Collections.Specialized;
using System.Linq;
using Syncfusion.XlsIO;
using Library.Service.Helpers;

#endregion Using

namespace Library.MaterialManagement.InventoryManagements
{
    public class InventoryIssueService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public InventoryIssueService() 
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor



        public IEnumerable<object> GetEntityWiseConsumption(string EntityId) 
        {
            try
            {
                var sql = @"select distinct a.ConsumptionBooking,a.EntityId,b.UserName from [dbo].[EntityConfig] a
                            left join [ORG].[Entity] b ON b.Id=a.EntityId
                            where a.EntityId='"+ EntityId + "'";               
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

		public IEnumerable<object> GetSalesOrderInfobyIssueSlipId(string IssueSlipId)  
		{
			try
			{
				var sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN
	                            ,POD.Id, POD.ProductionOrderId, MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,MO.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
                                , LSD = REPLACE(CONVERT(CHAR(11), SO.LSD, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description
	                            , Flag = CAST(0 AS BIT),null Active
                       FROM 
                       [TRN].[ProductionOrderDetail] AS POD
                       JOIN [TRN].[SalesOrder] AS SO ON pod.SalesOrderId=so.Id
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                       left  join trn.IssueRequestMasterSalesOrderMap map on map.SalesOrderId=SO.Id
                       --WHERE SO.Id In('212160101','212160102','212160103') 
					   --AND POD.ProductionOrderId = '21139' 
					   WHERE map.IssueRequestMasterId='"+ IssueSlipId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
			}
		}

		public IEnumerable<object> GetProductionOrderBYSalesOrder(string ProductionOrderId) 
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{//top 100
				var sql = @"select  * from ( SELECT 
                    case when PO.PlantId='"+ identity .PlantId+ @"' AND PO.PlantId=EN.PlantId then 'OWN' else 
                     case when PO.PlantId='" + identity.PlantId + @"' and EN.PlantId<>PO.PlantId then 'OUT' ELSE
                    case when PO.PlantId<>'" + identity.PlantId + @"' AND EN.PlantId='" + identity.PlantId + @"' THEN 'IN' ELSE '' END END END AS Owner,
                    PO.*,isnull(po.Remarks,'') AS ProductionRemarks,isnull(s.UserName,'') AS ProductionStatus, isnull(EN.UserName,'') AS EntityName,                                                                    
                    isnull(PS.UserName,'') AS ProductionStatusName,SO.*
                                FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,PM.UserName AS Product,pc.UserName AS ProductCategory,PM.Id ProductMasterId,
                                                    -- Min(LSD) AS LSD,max(CommitmentDate) AS CommitmentDate ,
                                                    sum(so.Qty) AS SOQuantity, Format(Min(so.DeliveryDate),'dd-MMM-yyyy') DeliveryDate,
                                                    MasterOrderId=STUFF((select distinct ','+XMOI.MasterOrderId from 
								                            trn.MasterOrderItem XMOI 	 
								                            INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                            INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                            where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
											 
					                                BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                                     
                                            from 
 
 
                                                     trn.SalesOrder SO 
                                                      JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                                    left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                    left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                    left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                                                    group by pod.ProductionOrderId,mm.userName,PM.UserName,pc.UserName,PM.Id) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE PO.PlantId='" + identity.PlantId + @"' OR EN.PlantId='" + identity.PlantId + @"') AS TEMP WHERE Id ='" + ProductionOrderId+@"' ORDER BY UpdatedDate DESC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
			}
		}

		public IEnumerable<object> GetIssueWiseSKU(string IssueId)
		{ 
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = @"select --Concat('-',ISNULL(SKUMAP.FirstCharacteristicsValueId,''),'-',ISNULL(SKUMAP.SecondCharacteristicsValueId,''),'-',ISNULL(SKUMAP.ThirdCharacteristicsValueId,'')) SOMATART
					--,MOI.MaterialMasterId
					MM.UserName MaterialName
					,Article.Id ArticleId
					,Article.StandardName ArticleName 
					,SKUMAP.FirstCharacteristicsValueId  FirstCharacteristicsValueId
					,IsNULL(V1.UserName, '') AS FirstCharacteristicsValue
					,FC.Id FirstCharacteristicsId

					,IsNULL(v2.UserName, '') AS SecondCharacteristicsValue
					,SKUMAP.SecondCharacteristicsValueId SecondCharacteristicsValueId
					,SC.Id SecondCharacteristicsId

					,IsNULL(v3.UserName, '') AS ThirdCharacteristicsValue
					,SKUMAP.ThirdCharacteristicsValueId ThirdCharacteristicsValueId
					,TC.Id ThirdCharacteristicsId
					,null Active
					,SKUMAP.SalesOrderId
					,SKUMAP.OrderQty	
					,SKUMAP.PlanOrderQty
					,SKUMAP.Destination
					,SKUMAP.PONumber
					,SKUMAP.PODate
					,SKUMAP.RequisitionForQty RequisitionForQty 
				FROM trn.IssueRequestSKUMap SKUMAP
				LEFT OUTER JOIN[HKP].[CharacteristicsValue] V1 ON v1.Id = SKUMAP.FirstCharacteristicsValueId
				LEFT OUTER JOIN[HKP].[CharacteristicsValue] V2 ON v2.Id = SKUMAP.SecondCharacteristicsValueId
				LEFT OUTER JOIN[HKP].[CharacteristicsValue] V3 ON v3.Id = SKUMAP.ThirdCharacteristicsValueId
				LEFT JOIN HKP.Characteristics AS FC ON FC.Id = V1.CharacteristicsId
				LEFT JOIN HKP.Characteristics AS SC ON SC.Id = V2.CharacteristicsId
				LEFT JOIN HKP.Characteristics AS TC ON TC.Id = V3.CharacteristicsId
				LEFT  JOIN MST.MaterialMaster MM ON MM.Id=SKUMAP.MaterialMasterId
				LEFT JOIN mst.MaterialMasterArticle Article ON Article.Id=SKUMAP.ArticleId
				--LEFT JOIN  trn.IssueRequestMasterSalesOrderMap IssueRequestMasterSalesOrderMap ON IssueRequestMasterSalesOrderMap.IssueRequestMasterId=SKUMAP.IssueRequestMasterId
				--LEFT JOIN trn.SalesOrder SO ON SO.Id=IssueRequestMasterSalesOrderMap.SalesOrderId
				--LEFT JOIN [MST].[Destination] AS D ON D.Id=SO.DestinationId
				--LEFT JOIN [TRN].[CustomerPO] AS CPO ON CPO.Id=SO.CustomerPOId
				where SKUMAP.IssueRequestMasterId='" + IssueId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
			}
		}

	}
}
