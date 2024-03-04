using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.Enums;
using Library.Crosscutting.Security;
using System.Threading;
using Library.ViewModel.OrderManagements;
using Library.Service.Systems;
using ConnectionManager;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;

namespace Library.OrderManagement.Production
{
    public class ProductionSummaryData
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        
        public ProductionSummaryData()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
            
        }

        public IEnumerable<object> GetPOCust(string POId)
        {
            try
            {
                var sql = @"select po.Id AS ProductionOrderId,Customer=STUFF((select distinct ','+XP.UserName from
                            trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                            left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                            where PO.Id=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                            from trn.ProductionOrder PO where PO.Id='" + POId + "'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSOCust(string SOId)
        {
            try
            {

                var sql = @"select Customer=STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO
                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
                            left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
                            left outer join[HKP].[Party] Xp on XP.Id = XMO.PartyId
                            where XSO.Id = '" + SOId + "' for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')";


                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPR_SO(string ProdnDate, string EntityId, string ProcessId, string ShiftId, string WkCenterId)
        {
            try
            {
                var sql = @"select ps.Id, ps.SalesOrderId, ps.ProductionOrderId,p.UserName as Plant,E.UserName as Entity,
                                    Pr.UserName as Process,
                                    Wc.UserName as WorkCenter,
                                    csg.Description as ProductionShift from TRN.ProductionSummary ps
                           left join ORG.Plant p on ps.PlantId = p.Id
                           left join ORG.Entity E on ps.EntityId = E.Id
                           left join HKP.Process pr on ps.ProcessId = pr.Id
                           left join SCS.WorkCenterMaster wc on ps.WorkCenterMasterId = wc.Id
                           left join MST.CompliedShiftGrouping csg on ps.ProductionShiftId = csg.Id
                           left join dbo.EmployeeInformation emp on ps.ResponsiblePersonId = emp.SystemId where isnull(ps.ProductionDate, '') = '" + ProdnDate + "' and isnull(ps.EntityId,'') = '" + EntityId + "'and isnull(ps.ProcessId,'')= '" + ProcessId + "'  and isnull(ps.ProductionShiftId,'')= '" + ShiftId + "' and isnull(ps.WorkCenterMasterId,'')= '" + WkCenterId + "'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEntityName(string EntityId, string PlantId)
        {
            try
            {
                var _sql = @"select Id as Value,UserName AS Text FROM ORG.Entity where Id='" + EntityId + "'and PlantId='" + PlantId + "' ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetItemsData(string entityid, string workCenterMasterId, string productionLevel, string processId, string ProductionOrderId)
        {
            if (productionLevel == ProductionBookingLevel.SalesOrder.ToString())
            {
                string CmdText = @"SELECT DISTINCT mo.MasterOrderNo
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.ProcessId = '" + processId + @"'
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running'	AND POSP.ProcessId = '" + processId + "' AND PO.Id='" + ProductionOrderId + "'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
            else if (productionLevel == ProductionBookingLevel.MasterOrderItem.ToString())
            {
                string CmdText = @"SELECT DISTINCT mo.MasterOrderNo,so.MasterOrderItemId
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,BU.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.ProcessId = '" + processId + @"'
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running'	AND POSP.ProcessId = '" + processId + "' AND PO.Id='" + ProductionOrderId + "'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
            else if (productionLevel == ProductionBookingLevel.ProductCode.ToString())
            {
                string CmdText = @"SELECT DISTINCT mo.MasterOrderNo,MOI.Id MasterOrderItemId,MOI.ProductLibraryId,PL.Code ProductCode
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.ProcessId = '" + processId + @"'
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running'	AND POSP.ProcessId = '" + processId + "' AND PO.Id='" + ProductionOrderId + "'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
            else
            {
                string CmdText = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,ISNULL(PD.BuyerOrder,'') BuyerOrder,ISNULL(PD.OwnOrder,'') OwnOrder,ISNULL(PD.BuyerItem,'') BuyerItem,ISNULL(PD.OwnItem,'') OwnItem,PD.Description,PD.PONumber,PD.MaterialMasterId,PD.MaterialMaster,PD.ArticleId,PD.Article
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory
								   ,MM.Id MaterialMasterId,mm.UserName MaterialMaster,MMA.Id ArticleId,ISNULL(mma.StandardName, '') Article
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                   LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PO.EntityId='" + entityid + "' AND PS.UserName = 'Running'  AND PO.Id='" + ProductionOrderId + "'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
        }

        public object GetItemsDataWC(string entityid, string workCenterMasterId, string productionLevel, string processId, string productionOrderId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetProductionOrderData(string entityid, string workCenterMasterId, string productionLevel, string processId, string status)
        {
            //string CmdText = @"SELECT SO.CustomerPOId
            //                     ,CPO.PONumber
            //                     ,mm.Id MaterialMasterId
            //                     ,mm.UserName MaterialMaster
            //                     ,ISNULL(mma.StandardName, '') Article
            //                     ,b.UserName Customer
            //                     ,mo.TotalQty MOQty
            //                     ,ISNULL(u.UserName, '') UOM
            //                     ,moi.ExtraOrderPercentage [ExtraP]
            //                     ,moi.OrderWastagePercentage [WastageP]
            //                     ,ISNULL(mma.Id, '') ArticleId
            //                     ,mmc.CharCount
            //                     ,ISNULL(POD.ProductionOrderId, '') POId
            //                     ,B.UserName Buyer
            //                     ,PM.UserName AS ProductMasterName
            //                     ,CEILING(SO.PlannedQty) PlannedQty
            //                    	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
            //                     ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
            //                        ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
            //                    FROM TRN.ProductionOrderDetail POD
            //                   LEFT JOIN (
            //                     SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
            //                      ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
            //                     FROM trn.SalesOrder AS s
            //                     INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
            //                     GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
            //                     ) so ON POD.SalesOrderId = SO.Id
            //                    LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
            //                    LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
            //                    LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
            //                     FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.ProcessId
            //                     ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.ProcessId = '" + processId + @"'
            //                    LEFT JOIN HKP.Party b ON b.id = mo.PartyId
            //                    LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
            //                    LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
            //                    LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
            //                    LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
            //                     ) mmc ON mmc.MaterialMasterId = mm.id
            //                    LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
            //                    LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
            //                    LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
            //                    LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
            //                     INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
            //                     ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
            //                    LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
            //                    LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
            //                    LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
            //                    LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
            //                    LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
            //                    WHERE PS.UserName = 'Running' AND POSP.ProcessId = '" + processId + "'";
            string wc = string.Empty;
            if (status == "PROCESS")
            {
                wc = "PS.ProcessId = '" + processId + @"'";
            }
            else
            {
                wc = "PS.FromSFGInventoryId = '" + processId + @"'";
            }

            string CmdText = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
								  ,PlannedQty=CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END
                            ,((CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END)-ISNULL(CEILING(PRS.TotalProductionQty),0)) RemainingQty
                            , ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID=PO.Id
								  LEFT JOIN 
								  (    SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId
                                       FROM [TRN].[ProductionSummary] PS WHERE " + wc + @" GROUP BY PS.ProductionOrderId
                                  ) AS PRS ON PRS.ProductionOrderId = PO.Id
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory--,SO.Qty
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PS.UserName = 'Running' Order by PD.Description,PD.BuyerOrder";

            return _sqlRepository.GetDataCollection(CmdText);
        }

        public IEnumerable<object> GetDailyPlanningProductionData(string fromdate, string todate, string entityId, string processId, string shiftId, string wcId, string POId)
        {
            try
            {
                string sql = @"DECLARE @StartDate DATE = '" + fromdate + @"'
                                  , @EndDate DATE = '" + todate + @"'
                                Select A.EntityId,A.Entity,A.ProcessId,A.Process,POPS.Sequence ProcessSequence,A.WorkCenterMasterId,A.WorkCenterMaster,A.ProductPriority,A.Active,A.NoOfWorkStation
                                ,A.SPT StandardProcessTime,A.ShiftId,A.ShiftName,ISNULL(A.ShiftShortName,'') ShiftShortName,A.ProductionHours,DT.[Date],ISNULL(A.ResponsiblePerson,'') ResponsiblePerson,A.ResponsiblePersonCode
                                ,PS.Id SummaryId,PS.MasterOrderItemId,'' StandardProduct
                                ,Customer=ISNULL(STUFF((select distinct ', '+P.UserName from 
			                                                        HKP.Party P
			                                                        JOIN TRN.MasterOrder MO ON MO.PartyId=P.Id
			                                                        JOIN TRN.MasterOrderItem MOI ON MOI.MasterOrderId=MO.Id
			                                                        JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
									                                JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
			                                                        where POD.ProductionOrderId=PS.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
                                ,POArticle=ISNULL(STUFF((select distinct ', '+A.StandardName from 
			                                                        MST.MaterialMasterArticle A
			                                                        JOIN TRN.MasterOrderItem MOI ON MOI.ArticleId=A.Id
			                                                        JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
									                                JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
			                                                        where POD.ProductionOrderId=PS.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
                                ,LineItemArticle=ISNULL(STUFF((select distinct ', '+AR.StandardName from 
			                                                        MST.MaterialMasterArticle AR
			                                                        JOIN TRN.MasterOrderItem MOI ON MOI.ArticleId=AR.Id
			                                                        where PS.MasterOrderItemId=MOI.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                                ,LineItemProductCode=ISNULL(STUFF((select distinct ', '+PL.Code from 
			                                                        dbo.ProductLibrary PL
			                                                        JOIN TRN.MasterOrderItem MOI ON MOI.ProductLibraryId=PL.Id
			                                                        where PS.MasterOrderItemId=MOI.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')
                                ,SONo=ISNULL(STUFF((select distinct ', '+SO.Id from 
			                                                        TRN.SalesOrder SO 
									                                JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
			                                                        where POD.ProductionOrderId=PS.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                                ,PS.ProductionOrderId POId,PS.LotNumber,PS.SalesOrderId,ISNULL(PS.MasterOrderItemId,'.') MasterOrderItemId,ISNULL(PS.QtyWithoutScan,0)QtyWithoutScan,ISNULL(PS.ScanQty,0) QtyWithScan,TotalActualqty=(ISNULL(PS.QtyWithoutScan,0)+ISNULL(PS.ScanQty,0)) 
                                ,ISNULL(D.[Minute],0) DetentionInMinute,FORMAT(SCH.SPT,'N2')  POSPT,0 ArticleSPT
                                --,SPT= CASE WHEN  ArticleSPT IS NULL ArticleSPT=0 THEN SCH.SPT ELSE SCH.SPT =0 THEN A.SPT END
                                ,SPT=FORMAT(CASE WHEN SCH.SPT=0 THEN A.SPT ELSE SCH.SPT END,'N2')
                                ,ISNULL(CPS.NoOfEntry,1)NoOfEntry,AllotedHour=FORMAT(ROUND(A.ProductionHours,3)/ISNULL(CPS.NoOfEntry,1),'N2')
                                ,ShouldBeProduction=FORMAT((60/(CASE WHEN SCH.SPT=0 THEN A.SPT ELSE SCH.SPT END)*A.NoOfWorkStation*(ROUND(A.ProductionHours,3)/ISNULL(CPS.NoOfEntry,1))),'N2')
                                ,TotalAvailableHour=FORMAT(A.NoOfWorkStation*(ROUND(A.ProductionHours,3)/ISNULL(CPS.NoOfEntry,1)),'N2')
                                ,DetentionHour=FORMAT((ISNULL(D.[Minute],0)*A.NoOfWorkStation/60)/(ISNULL(CPS.NoOfEntry,1)),'N2')
                                ,NetAvailableHour=FORMAT((A.NoOfWorkStation*(ROUND(A.ProductionHours,3)/ISNULL(CPS.NoOfEntry,1)))-(ISNULL(D.[Minute],0)*A.NoOfWorkStation/60)/(ISNULL(CPS.NoOfEntry,1)),'N2')
                                ,ProduceHour=FORMAT((ISNULL(PS.QtyWithoutScan,0)+ISNULL(PS.ScanQty,0))*(CASE WHEN SCH.SPT=0 THEN A.SPT ELSE SCH.SPT END)/60,'N2')
                                ,DetentionLoss=FORMAT(ISNULL((60/(CASE WHEN SCH.SPT=0 THEN A.SPT ELSE SCH.SPT END)*(ISNULL(D.[Minute],0)*A.NoOfWorkStation/60)/(ISNULL(CPS.NoOfEntry,1))),0),'N2')
                                ,ProductivityVariance=FORMAT((60/(CASE WHEN SCH.SPT=0 THEN A.SPT ELSE SCH.SPT END)*A.NoOfWorkStation*(ROUND(A.ProductionHours,3)/ISNULL(CPS.NoOfEntry,1)))-
                                (ISNULL(PS.QtyWithoutScan,0)+ISNULL(PS.ScanQty,0)) -(ISNULL(PS.QtyWithoutScan,0)+ISNULL(PS.ScanQty,0)),'N2') 

                                from (
                                Select WCM.EntityId,E.UserName Entity,WCM.ProcessId,P.UserName Process,WCM.Id WorkCenterMasterId, WCM.UserName WorkCenterMaster
                                ,ProductPriority=ISNULL(STUFF((select distinct ', '+PM.UserName from 
			                                                        [SCS].[WorkCenterMasterProductPriority]  XSO
			                                                        JOIN MST.ProductMaster PM ON PM.id=XSO.ProductMasterId
			                                                        where XSO.WorkCenterMasterId=WCM.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'')

                                ,WCM.Active,WCM.NoOfWorkStation,WCM.SPT,SD.SystemID ShiftId,SD.UserName [ShiftName],SD.ShortName [ShiftShortName] ,WCS.ProductionHours
                                ,EI.EmployeeName ResponsiblePerson,EI.EmployeeCode ResponsiblePersonCode

                                from SCS.WorkCenterMaster WCM
                                LEFT JOIN dbo.WorkCenterWiseShift WCS ON WCS.WorkCenterMasterId=WCM.Id
                                LEFT JOIN (Select MAX(StartDate)StartDate,WorkCenterMasterId from  [SCS].[WorkCenterMasterEffectiveDate] Group BY WorkCenterMasterId) WCD ON WCD.WorkCenterMasterId=WCS.WorkCenterMasterId
                                LEFT JOIN dbo.ShiftDefination SD ON SD.SystemID=WCS.ShiftDefinationID
                                LEFT JOIN ORG.Entity E ON E.Id=WCM.EntityId
                                LEFT JOIN HKP.Process P ON P.Id=WCM.ProcessId
                                LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=WCM.ResponsiblePersonId
                                ) A
                                LEFT JOIN(

                                SELECT  format(DATEADD(DAY, nbr - 1, @StartDate),'dd-MMM-yyyy') [Date]
                                FROM    (SELECT ROW_NUMBER() OVER ( ORDER BY c.object_id ) AS nbr
                                          FROM sys.columns c
                                        ) nbrs
                                WHERE nbr - 1 <= DATEDIFF(DAY, @StartDate, @EndDate)
                                ) DT ON 1=1
                                LEFT JOIN trn.ProductionSummary PS ON PS.ProductionShiftId=A.ShiftId AND PS.EntityId=A.EntityId AND PS.WorkCenterMasterId=A.WorkCenterMasterId AND PS.ProcessId=A.ProcessId AND PS.ProductionDate=DT.Date
                                LEFT JOIN(Select COUNT(Id)NoOfEntry,WorkCenterMasterId,ProductionShiftId,ProductionDate from trn.ProductionSummary Group BY WorkCenterMasterId,ProductionShiftId,ProductionDate) CPS ON CPS.WorkCenterMasterId=A.WorkCenterMasterId AND CPS.ProductionShiftId=A.ShiftId AND CPS.ProductionDate=DT.Date
                                --LEFT JOIN [dbo].[MachineMasterTransaction] D ON D.ShiftId=A.ShiftId AND D.EntityId=A.EntityId AND D.WorkCenterId=A.WorkCenterMasterId AND D.ProcessId=A.ProcessId AND D.Date=DT.Date
                                LEFT JOIN [dbo].[MachineMasterTransaction] D ON D.ProductionSummaryId=PS.Id
                                LEFT JOIN [dbo].[ProductionOrderSchedulingParametersType1] SCH ON SCH.ProductionOrderID=PS.ProductionOrderId
								left join trn.ProductionOrderProcessSet POPS on POPS.ProductionOrderId=PS.ProductionOrderId and POPS.ProcessId=A.ProcessId

                where A.EntityId='" + entityId + @"' and A.ProcessId='" + processId + @"' and A.ShiftId='" + shiftId + @"' and A.WorkCenterMasterId='" + wcId + @"'
                    and PS.ProductionOrderId='" + POId + @"'
                order by A.EntityId,A.Process,PS.ProductionOrderId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string DailyPlanningProductionReport(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Daily Planning & Production Report";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Process Sequence";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcessSequence = COL;
                COL++;

                sheet[ROW, COL].Text = "Process";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProcess = COL;
                COL++;

                sheet[ROW, COL].Text = "Work Center";
                sheet[ROW, COL].ColumnWidth = 16;
                int colWorkCenterMaster = COL;
                COL++;

                sheet[ROW, COL].Text = "Active";
                sheet[ROW, COL].ColumnWidth = 16;
                int colActive = COL;
                COL++;

                sheet[ROW, COL].Text = "No Of Work Station";
                sheet[ROW, COL].ColumnWidth = 16;
                int colNoOfWorkStation = COL;
                COL++;

                sheet[ROW, COL].Text = "Shift";
                sheet[ROW, COL].ColumnWidth = 41;
                int colShiftName = COL;
                COL++;

                sheet[ROW, COL].Text = "Production Hours";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductionHours = COL;
                COL++;

                sheet[ROW, COL].Text = "Standard Product";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStandardProduct = COL;
                COL++;
                
                sheet[ROW, COL].Text = "Standard Process Time";
                sheet[ROW, COL].ColumnWidth = 16;
                int colStandardProcessTime = COL;
                COL++;

                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colResponsiblePerson = COL;
                COL++;

                sheet[ROW, COL].Text = "Date";
                sheet[ROW, COL].ColumnWidth = 28;
                int colDate = COL;
                COL++;

                sheet[ROW, COL].Text = "PO No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOId = COL;
                COL++;

                sheet[ROW, COL].Text = "Lot Number";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLotNumber = COL;
                COL++;  

                sheet[ROW, COL].Text = "Master Order Item No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colMasterOrderItemId = COL;
                COL++;

                sheet[ROW, COL].Text = "SO No";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSONo = COL;
                COL++;

                sheet[ROW, COL].Text = "Qty Without Scan";
                sheet[ROW, COL].ColumnWidth = 16;
                int colQtyWithoutScan = COL;
                COL++;

                sheet[ROW, COL].Text = "Qty With Scan";
                sheet[ROW, COL].ColumnWidth = 16;
                int colQtyWithScan = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Actual qty";
                sheet[ROW, COL].ColumnWidth = 16;
                int colTotalActualqty = COL;
                COL++;

                sheet[ROW, COL].Text = "Line Item Product Code";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLineItemProductCode = COL;
                COL++;

                sheet[ROW, COL].Text = "PO Article";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPOArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "POSPT";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOSPT = COL;
                COL++;

                sheet[ROW, COL].Text = "Detention Hour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDetentionHour = COL;
                COL++;

                sheet[ROW, COL].Text = "No Of Entry";
                sheet[ROW, COL].ColumnWidth = 16;
                int colNoOfEntry = COL;
                COL++;

                sheet[ROW, COL].Text = "Alloted Hour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colAllotedHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Available Hour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colTotalAvailableHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Summary No";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSummaryId = COL;
                COL++;

                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;
                COL++;
                sheet[ROW, COL].Text = "Line Item Article";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLineItemArticle = COL;
                COL++;
                sheet[ROW, COL].Text = "Shift Short Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int colShiftShortName = COL;
                COL++;

                sheet[ROW, COL].Text = "Product Priority";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductPriority = COL;
                COL++;
                sheet[ROW, COL].Text = "Detention In Minute";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDetentionInMinute = COL;
                COL++;
                sheet[ROW, COL].Text = "Article SPT";
                sheet[ROW, COL].ColumnWidth = 16;
                int colArticleSPT = COL;
                COL++;

                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSPT = COL;
                COL++;
               

                sheet[ROW, COL].Text = "Should Be Production";
                sheet[ROW, COL].ColumnWidth = 16;
                int colShouldBeProduction = COL;
                COL++;

                sheet[ROW, COL].Text = "Net Available Hour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colNetAvailableHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Produce Hour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProduceHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Detention Loss";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDetentionLoss = COL;
                COL++;

                sheet[ROW, COL].Text = "Productivity Variance";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductivityVariance = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                int LastRow = ROW + (data.Count - 1);

                for (int i = 0; i < data.Count; i++)
                {
                    sheet[ROW, colEntity].Text = data[i]["Entity"].ToString();
                    sheet[ROW, colProcessSequence].Text = data[i]["ProcessSequence"].ToString();
                    sheet[ROW, colProcess].Text = data[i]["Process"].ToString();
                    sheet[ROW, colWorkCenterMaster].Text = data[i]["WorkCenterMaster"].ToString();
                    sheet[ROW, colProductPriority].Text = data[i]["ProductPriority"].ToString();
                    sheet[ROW, colActive].Text = data[i]["Active"].ToString();
                    sheet[ROW, colNoOfWorkStation].Number = clsStaticInfo.dbl(data[i]["NoOfWorkStation"].ToString());
                    sheet[ROW, colStandardProcessTime].Number = clsStaticInfo.dbl(data[i]["StandardProcessTime"].ToString());
                    sheet[ROW, colShiftName].Text = data[i]["ShiftName"].ToString();
                    sheet[ROW, colStandardProduct].Text = data[i]["StandardProduct"].ToString();
                    sheet[ROW, colShiftShortName].Text = data[i]["ShiftShortName"].ToString();
                    sheet[ROW, colProductionHours].Number = clsStaticInfo.dbl(data[i]["ProductionHours"].ToString());
                    sheet[ROW, colDate].Text = data[i]["Date"].ToString();
                    sheet[ROW, colResponsiblePerson].Text = data[i]["ResponsiblePerson"].ToString();
                    sheet[ROW, colSummaryId].Text = data[i]["SummaryId"].ToString();
                    sheet[ROW, colCustomer].Text = data[i]["Customer"].ToString();
                    sheet[ROW, colPOArticle].Text = data[i]["POArticle"].ToString();
                    sheet[ROW, colLineItemArticle].Text = data[i]["LineItemArticle"].ToString();
                    sheet[ROW, colLineItemProductCode].Text = data[i]["LineItemProductCode"].ToString();
                    sheet[ROW, colSONo].Text = data[i]["SONo"].ToString(); 
                    sheet[ROW, colPOId].Text = data[i]["POId"].ToString();
                    sheet[ROW, colLotNumber].Text = data[i]["LotNumber"].ToString();
                    if (data[i]["MasterOrderItemId"] !=null)
                    {
                    sheet[ROW, colMasterOrderItemId].Text = data[i]["MasterOrderItemId"].ToString();
                    }
                    sheet[ROW, colQtyWithoutScan].Number = clsStaticInfo.dbl(data[i]["QtyWithoutScan"].ToString());
                    sheet[ROW, colQtyWithScan].Number = clsStaticInfo.dbl(data[i]["QtyWithScan"].ToString());
                    sheet[ROW, colTotalActualqty].Number = clsStaticInfo.dbl(data[i]["TotalActualqty"].ToString());
                    sheet[ROW, colDetentionInMinute].Number = clsStaticInfo.dbl(data[i]["DetentionInMinute"].ToString());
                    sheet[ROW, colPOSPT].Number = clsStaticInfo.dbl(data[i]["POSPT"].ToString());
                    sheet[ROW, colArticleSPT].Number = clsStaticInfo.dbl(data[i]["ArticleSPT"].ToString());
                    sheet[ROW, colSPT].Number = clsStaticInfo.dbl(data[i]["SPT"].ToString());
                    sheet[ROW, colNoOfEntry].Number = clsStaticInfo.dbl(data[i]["NoOfEntry"].ToString());
                    sheet[ROW, colAllotedHour].Number = clsStaticInfo.dbl(data[i]["AllotedHour"].ToString());
                    sheet[ROW, colShouldBeProduction].Number = clsStaticInfo.dbl(data[i]["ShouldBeProduction"].ToString());
                    sheet[ROW, colTotalAvailableHour].Number = clsStaticInfo.dbl(data[i]["TotalAvailableHour"].ToString());
                    sheet[ROW, colDetentionHour].Number = clsStaticInfo.dbl(data[i]["DetentionHour"].ToString());
                    sheet[ROW, colNetAvailableHour].Number = clsStaticInfo.dbl(data[i]["NetAvailableHour"].ToString());
                    sheet[ROW, colProduceHour].Number = clsStaticInfo.dbl(data[i]["ProduceHour"].ToString());
                    sheet[ROW, colDetentionLoss].Number = clsStaticInfo.dbl(data[i]["DetentionLoss"].ToString());
                    sheet[ROW, colProductivityVariance].Number = clsStaticInfo.dbl(data[i]["ProductivityVariance"].ToString());



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Daily Planning & Production Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetProductionOrderDataList(string entityid, string workCenterMasterId, string productionLevel, string processId,bool ToCloseAllowed)
        {
            string wcpr;
            if (ToCloseAllowed)
            {
                wcpr = @"PS.UserName IN('Running','To Close')";
            }
            else
            {
                wcpr = @"PS.UserName = 'Running'";
            }
            string CmdText = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
								  ,PlannedQty=CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END
                            --,((CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END)-ISNULL(CEILING(PRS.TotalProductionQty),0)) RemainingQty
                             ,ISNULL((CASE WHEN ISNULL(PPS.Qty,0)=0 THEN ISNULL(PQ.Qty,PO.PlannedQty) ELSE PO.PlannedQty*PPS.Qty/100 END)-ISNULL(CEILING(PRS.TotalProductionQty), 0),0) RemainingQty
                            , ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                ,Material=STUFF((select distinct ','+MM.UserName from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						                                         LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                             ,Article=STUFF((select distinct ','+MMA.StandardName  from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						                                         LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
																 LEFT JOIN MST.MaterialMasterArticle AS mma on mma.MaterialMasterId=MM.Id
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                   PRS.LotNumber
                                   --,PRS.ResponsiblePerson
								   FROM TRN.ProductionOrder PO 
                                   LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id AND PPS.ProcessId = '" + processId + @"'
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID=PO.Id
								  LEFT JOIN 
								  (    SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId,PS.LotNumber
                                       --,(select EmployeeName from EmployeeInformation where SystemId=PS.ResponsiblePersonId) as ResponsiblePerson
                                       FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.ProductionOrderId,PS.LotNumber
                                       --,PS.ResponsiblePersonId
                                  ) AS PRS ON PRS.ProductionOrderId = PO.Id
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory--,SO.Qty
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE "+wcpr+" Order by PD.Description,PD.BuyerOrder";

            return _sqlRepository.GetDataCollection(CmdText);
        }

         public IEnumerable<object> GetProductionOrderDataListWC(string entityid, string workCenterMasterId, string productionLevel, string processId, bool ToCloseAllowed)
        {
            string wcpr;
            if (ToCloseAllowed)
            {
                wcpr = @"PS.UserName IN('Running','To Close')";
            }
            else
            {
                wcpr = @"PS.UserName = 'Running'";
            }
            string CmdText = @"SELECT distinct PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
								  								   ,PlannedQty=CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END
                            --,((CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END)-ISNULL(CEILING(PRS.TotalProductionQty),0)) RemainingQty
                             ,ISNULL((CASE WHEN ISNULL(PPS.Qty,0)=0 THEN ISNULL(PQ.Qty,PO.PlannedQty) ELSE PO.PlannedQty*PPS.Qty/100 END)-ISNULL(CEILING(PRS.TotalProductionQty), 0),0) RemainingQty
                            , ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                ,Material=STUFF((select distinct ','+MM.UserName from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						                                         LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                             ,Article=STUFF((select distinct ','+MMA.StandardName  from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						                                         --LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
																 --LEFT JOIN MST.MaterialMasterArticle AS mma on mma.MaterialMasterId=MM.Id
                                                                 LEFT JOIN MST.MaterialMasterArticle AS mma on mma.Id=MOI.ArticleId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                   --PRS.LotNumber
                                   (Case when PO.IsPreDefineLotApplicable = 1 then PLC.UserLotNo else PRS.LotNumber end)  LotNumber,
                                   PO.IsPreDefineLotApplicable,(Case when PO.IsPreDefineLotApplicable = 1 then 'Yes' else 'No' end) LotPrefefined,
                                   isnull(CEILING(PLC.ProcessPlanQty),0) LotProcessPlanQty,PPS.IsProductionVerification ProductionVerification
                                   --,PRS.ResponsiblePerson
								   FROM TRN.ProductionOrder PO 
                                   LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id AND PPS.ProcessId = '" + processId + @"'
                                   LEFT JOIN ProductionOrderLotControl PLC ON PLC.ProductionOrderID = PO.Id AND PLC.ProcessId = '" + processId + @"'
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID=PO.Id
								  LEFT JOIN 
								  (    SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId
                                      ,PS.LotNumber
                                       --,(select EmployeeName from EmployeeInformation where SystemId=PS.ResponsiblePersonId) as ResponsiblePerson
                                       FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.ProductionOrderId,PS.LotNumber
                                       --,PS.ResponsiblePersonId
                                  ) AS PRS ON PRS.ProductionOrderId = PO.Id
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory--,SO.Qty
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								    WHERE " + wcpr + " Order by PD.Description,PD.BuyerOrder";

            return _sqlRepository.GetDataCollection(CmdText);
        }

        public IEnumerable<object> GetProductionOrderList(string entityid, string productionLevel, string processId, bool ToCloseAllowed)
        {
            string wcpr;
            if (ToCloseAllowed)
            {
                wcpr = @"PS.UserName IN('Running','To Close')";
            }
            else
            {
                wcpr = @"PS.UserName = 'Running'";
            }
            string CmdText = @"SELECT distinct PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
								  								   ,PlannedQty=CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END
                            --,((CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END)-ISNULL(CEILING(PRS.TotalProductionQty),0)) RemainingQty
                             ,ISNULL((CASE WHEN ISNULL(PPS.Qty,0)=0 THEN ISNULL(PQ.Qty,PO.PlannedQty) ELSE PO.PlannedQty*PPS.Qty/100 END)-ISNULL(CEILING(PRS.TotalProductionQty), 0),0) RemainingQty
                            , ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                ,Material=STUFF((select distinct ','+MM.UserName from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						                                         LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                             ,Article=STUFF((select distinct ','+MMA.StandardName  from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						                                         --LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
																 --LEFT JOIN MST.MaterialMasterArticle AS mma on mma.MaterialMasterId=MM.Id
                                                                 LEFT JOIN MST.MaterialMasterArticle AS mma on mma.Id=MOI.ArticleId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                   PRS.LotNumber
                                   --,PRS.ResponsiblePerson
								   FROM TRN.ProductionOrder PO 
                                   LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id AND PPS.ProcessId = '" + processId + @"'
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID=PO.Id
								  LEFT JOIN 
								  (    SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId,PS.LotNumber
                                       --,(select EmployeeName from EmployeeInformation where SystemId=PS.ResponsiblePersonId) as ResponsiblePerson
                                       FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.ProductionOrderId,PS.LotNumber
                                       --,PS.ResponsiblePersonId
                                  ) AS PRS ON PRS.ProductionOrderId = PO.Id
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory--,SO.Qty
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								    WHERE " + wcpr + " Order by PD.Description,PD.BuyerOrder";

            return _sqlRepository.GetDataCollection(CmdText);
        }

        public IEnumerable<object> GetQualityProductionOrderList(string entityid, string productionLevel, string processId, bool ToCloseAllowed)
        {
            string wcpr;
            //if (ToCloseAllowed)
            //{
            //    wcpr = @"PS.UserName IN('Running','To Close')";
            //}
            //else
            //{
                wcpr = @"PS.UserName in ('Running','To Close')";
            //}
            string CmdText = @"SELECT distinct PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                ,Material=STUFF((select distinct ','+MM.UserName from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						                                         LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                             ,Article=STUFF((select distinct ','+MMA.StandardName  from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
                                                                 LEFT JOIN MST.MaterialMasterArticle AS mma on mma.Id=MOI.ArticleId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                 
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory--,SO.Qty
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								    WHERE " + wcpr + " and PO.EntityId='"+ entityid + "' Order by PD.Description,PD.BuyerOrder";

            return _sqlRepository.GetDataCollection(CmdText);
        }

        public IEnumerable<object> GetQualityCompletePOList(string IssueId)
        {
            string sql = @"SELECT distinct QC.ProductionOrderId POId,Article=STUFF((select distinct ','+MMA.StandardName  from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
                                                                 LEFT JOIN MST.MaterialMasterArticle AS mma on mma.Id=MOI.ArticleId
                                                                 WHERE QC.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
																 Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where QC.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
 FROM TRN.QualityControl QC
 where QC.IssueId='" + IssueId + "' and ProductionOrderId is not null";

            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetSFGSOItem(string entityid, string workCenterMasterId, string productionLevel, string processId, string status, bool IsFirst, string ProductionOrderId)
        {

            string wc = string.Empty;
            if (status == "PROCESS")
            {
                wc = "PS.ProcessId = '" + processId + @"'";
            }
            else
            {
                wc = "PS.FromSFGInventoryId = '" + processId + @"'";
            }

            if (productionLevel == ProductionBookingLevel.SalesOrder.ToString())
            {
                if (status == "PROCESS")
                {
                    if (IsFirst == true)
                    {
                        string CmdText = @"SELECT DISTINCT mo.MasterOrderNo
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,BU.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,ISNULL(MO.BuyerReferenceNo,'') BuyerOrder,ISNULL(MO.OwnReferenceNo,'') OwnOrder,ISNULL(moi.BuyerReferenceNo,'') BuyerItem,ISNULL(moi.OwnReferenceNo,'') OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE " + wc + @" GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PS.UserName = 'Running' AND PO.Id='" + ProductionOrderId + "'";

                        return _sqlRepository.GetDataCollection(CmdText);
                    }
                    else
                    {
                        string CmdText = @"SELECT DISTINCT mo.MasterOrderNo
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,BU.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,ISNULL(MO.BuyerReferenceNo,'') BuyerOrder,ISNULL(MO.OwnReferenceNo,'') OwnOrder,ISNULL(moi.BuyerReferenceNo,'') BuyerItem,ISNULL(moi.OwnReferenceNo,'') OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE " + wc + @"  GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PS.UserName = 'Running' AND PO.Id='" + ProductionOrderId + "'";

                        return _sqlRepository.GetDataCollection(CmdText);
                    }
                }
                else
                {
                    if (IsFirst == true)
                    {
                        string CmdText = @"SELECT DISTINCT mo.MasterOrderNo
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,BU.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,ISNULL(MO.BuyerReferenceNo,'') BuyerOrder,ISNULL(MO.OwnReferenceNo,'') OwnOrder,ISNULL(moi.BuyerReferenceNo,'') BuyerItem,ISNULL(moi.OwnReferenceNo,'') OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.FromSFGInventoryId
	                                FROM [TRN].[ProductionSummary] PS WHERE " + wc + @" GROUP BY PS.SalesOrderId,PS.FromSFGInventoryId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PS.UserName = 'Running' AND PO.Id='" + ProductionOrderId + "'";

                        return _sqlRepository.GetDataCollection(CmdText);
                    }
                    else
                    {
                        string CmdText = @"SELECT DISTINCT mo.MasterOrderNo
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,BU.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,ISNULL(MO.BuyerReferenceNo,'') BuyerOrder,ISNULL(MO.OwnReferenceNo,'') OwnOrder,ISNULL(moi.BuyerReferenceNo,'') BuyerItem,ISNULL(moi.OwnReferenceNo,'') OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.FromSFGInventoryId
	                                FROM [TRN].[ProductionSummary] PS WHERE " + wc + @" GROUP BY PS.SalesOrderId,PS.FromSFGInventoryId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PS.UserName = 'Running' AND PO.Id='" + ProductionOrderId + "'";

                        return _sqlRepository.GetDataCollection(CmdText);
                    }
                }
            }
            else if (productionLevel == ProductionBookingLevel.MasterOrderItem.ToString())
            {
                string CmdText = @"SELECT DISTINCT mo.MasterOrderNo,so.MasterOrderItemId
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE " + wc + @" GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running'	AND POSP.ProcessId = '" + processId + "' AND PO.Id='" + ProductionOrderId + "'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
            else if (productionLevel == ProductionBookingLevel.ProductCode.ToString())
            {
                string CmdText = @"SELECT DISTINCT mo.MasterOrderNo,MOI.Id MasterOrderItemId,MOI.ProductLibraryId,PL.Code ProductCode
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
	                               	,CEILING(ISNULL(PRS.TotalProductionQty,0)) TotalProductionQty
	                                ,CEILING(ISNULL((SO.PlannedQty - ISNULL(PRS.TotalProductionQty,0)),0)) RemainingQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId,PS.ProcessId
	                                FROM [TRN].[ProductionSummary] PS WHERE " + wc + @" GROUP BY PS.SalesOrderId,PS.ProcessId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id AND WC.WorkCenterMasterId = '" + workCenterMasterId + @"'
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running' AND PO.Id='" + ProductionOrderId + "'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
            else
            {
                string CmdText = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,ISNULL(PD.BuyerOrder,'') BuyerOrder,ISNULL(PD.OwnOrder,'') OwnOrder,ISNULL(PD.BuyerItem,'') BuyerItem,ISNULL(PD.OwnItem,'') OwnItem,PD.Description,PD.PONumber,PD.MaterialMasterId,PD.MaterialMaster,PD.ArticleId,PD.Article
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory
								   ,MM.Id MaterialMasterId,mm.UserName MaterialMaster,MMA.Id ArticleId,ISNULL(mma.StandardName, '') Article
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                   LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PO.EntityId='" + entityid + "' AND PS.UserName = 'Running'  AND PO.Id='" + ProductionOrderId + "'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
        }

        public IEnumerable<object> GetLineItemGridSFG(string EntityId, string ProcessId, string ProductionDate, string ProductionShiftId, string WorkCenterMasterId, string ProductionLevel, string status)
        {
            try
            {
                string wc = string.Empty;


                if (WorkCenterMasterId != "undefined" && WorkCenterMasterId != "null")
                {
                    //if (status == "PROCESS")
                    //{
                    //    wc = @"AND P.WorkCenterMasterId='" + WorkCenterMasterId + @"' and p.ProcessId='" + ProcessId + @"' ";
                    //}
                    //else
                    //{
                    //    wc = @"AND P.ToWorkCenterMasterId='" + WorkCenterMasterId + @"' and p.FromSFGInventoryId='" + ProcessId + @"' ";
                    //}

                }
                if (status == "PROCESS")
                {
                    wc = @"and p.ProcessId='" + ProcessId + @"' ";
                }
                else
                {
                    wc = @"and p.FromSFGInventoryId='" + ProcessId + @"' ";
                }

                if (ProductionLevel != "ProductionOrder")
                {
                    string _sql = @"select p.Id,mo.MasterOrderNo
								,moi.Id MOrderLineNo
								,so.Id SalesOrderId
                                
                                ,PO.PONumber
                                  ,mm.UserName MaterialMaster, mma.StandardName Article
								  ,b.UserName Customer
                                 --,so.ConfirmDate,so.DeliveryDate
                                 ,Replace(CONVERT(VARCHAR(11), so.ConfirmDate, 106), ' ', '-') ConfirmDate
								 ,Replace(CONVERT(VARCHAR(11), so.DeliveryDate, 106), ' ', '-') DeliveryDate
								 ,mo.TotalQty MOQty
								 ,moi.TotalQty MOIQty
                                 ,so.Qty SOQty,p.Quantity ,p.ProductionBookingPeriodId,p.ProductionGrade 
								,u.UserName UOM
								,moi.ExtraOrderPercentage [ExtraP],moi.OrderWastagePercentage [WastageP]
								,mma.Id ArticleId,mm.Id MaterialMasterId,mmc.CharCount, p.PlantID,p.WorkCenterMasterId,EP.ProductionBookingLevel
                                ,PBP.UserName ProductionBookingPeriod,P.ResponsiblePersonId,R.EmployeeName ResponsiblePersonName,P.MentorId, M.EmployeeName MentorName
                                ,FORMAT (P.InTime, 'dd-MMM-yyyy hh:mm:tt') InTime, FORMAT (P.OutTime, 'dd-MMM-yyyy hh:mm:tt') OutTime,P.ConsumeHour,P.ManPower, P.CheckedBy,C.EmployeeName CheckedByName
                                ,P.ToWorkCenterMasterId,P.FromSFGInventoryId,P.ToSFGInventoryId,P.ToProcessId,P.Remarks,P.WorkCenterMasterId,P.LotNumber
                                ,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem,so.Description,P.ProductionOrderId
                                ,WCM.UserName FromWorkCenterMaster,TWCM.UserName ToWorkCenterMaster,ISNULL(FP.UserName,FSFG.UserName) [From], ISNULL(TP.UserName,TSFG.UserName) [To],P.ToEntityId,so.Description
                                 FROM [TRN].[ProductionSummary] p
								 LEFT JOIN trn.SalesOrder so on so.Id=p.SalesOrderId
                                 LEFT JOIN trn.[MasterOrderItem] moi on moi.id=so.MasterOrderItemId
                                 LEFT JOIN trn.MasterOrder mo on mo.id=moi.MasterOrderId
                                 LEFT JOIN hkp.Party b on b.id=mo.PartyId
								 LEFT JOIN scs.UnitOfMeasurement u on u.id=mo.TotalQtyUOMId
                                 LEFT JOIN mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                 LEFT JOIN mst.MaterialMasterArticle mma on mma.id=moi.ArticleId
                                 LEFT JOIN (
											SELECT count(Id) CharCount,MaterialMasterId from [MST].[MaterialMasterCharacteristics] group by  MaterialMasterId
											) mmc on mmc.MaterialMasterId=mm.id
                                 LEFT JOIN [HKP].[EntityProcessTag] EP ON EP.ProcessId=P.ProcessId and EP.EntityId=P.EntityId
                                 LEFT JOIN [HKP].[ProductionBookingPeriod] PBP ON PBP.Id=P.ProductionBookingPeriodId
                                 LEFT JOIN EmployeeInformation R ON P.ResponsiblePersonId=R.SystemId
                                 LEFT JOIN EmployeeInformation M ON P.MentorId=M.SystemId
                                 LEFT JOIN [TRN].[CustomerPO] PO ON PO.Id=SO.CustomerPOId
								 LEFT JOIN SCS.WorkCenterMaster WCM ON WCM.Id=P.WorkCenterMasterId
								 LEFT JOIN SCS.WorkCenterMaster TWCM ON TWCM.Id=P.ToWorkCenterMasterId
                                 LEFT JOIN EmployeeInformation C ON P.CheckedBy=C.SystemId
                                 LEFT JOIN HKP.Process FP ON FP.Id=P.ProcessId
								 LEFT JOIN HKP.Process TP ON TP.Id=P.ToProcessId
								 LEFT JOIN HKP.SFGInventory FSFG ON FSFG.Id=FromSFGInventoryId
								 LEFT JOIN HKP.SFGInventory TSFG ON TSFG.Id=ToSFGInventoryId
                                 WHERE p.EntityId='" + EntityId + @"' 								 
								 and p.ProductionShiftId='" + ProductionShiftId + @"'  
								 and p.ProductionDate='" + ProductionDate + @"' " + wc + " Order BY ISNULL(WCM.UserName,TWCM.UserName)";

                    return _sqlRepository.GetDataCollection(_sql, null);

                }
                else
                {
                    string _sql = @"SELECT P.Id,P.ProductionOrderId,FORMAT(P.ProductionDate,'dd-MMM-yyyy') ProductionDate, P.ProductionGrade, P.Quantity, PBP.UserName ProductionBookingPeriod
                                 ,P.ResponsiblePersonId,R.EmployeeName ResponsiblePersonName,P.MentorId, M.EmployeeName MentorName, P.CheckedBy,C.EmployeeName CheckedByName
                                 ,FORMAT (P.InTime, 'dd-MMM-yyyy hh:mm:tt') InTime, FORMAT (P.OutTime, 'dd-MMM-yyyy hh:mm:tt') OutTime,P.ConsumeHour,P.ManPower
                                 ,P.ToWorkCenterMasterId,P.FromSFGInventoryId,P.ToSFGInventoryId,P.ToProcessId,P.Remarks,P.WorkCenterMasterId,P.LotNumber
                                 ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,WCM.UserName FromWorkCenterMaster,TWCM.UserName ToWorkCenterMaster,ISNULL(FP.UserName,FSFG.UserName) [From], ISNULL(TP.UserName,TSFG.UserName) [To],p.SalesOrderId,P.ToEntityId,PD.Description
                                 FROM TRN.ProductionSummary P
                                 LEFT JOIN HKP.ProductionBookingPeriod PBP ON PBP.Id=P.ProductionBookingPeriodId
                                 LEFT JOIN EmployeeInformation R ON P.ResponsiblePersonId=R.SystemId
                                 LEFT JOIN EmployeeInformation M ON P.MentorId=M.SystemId
                                 LEFT JOIN EmployeeInformation C ON P.CheckedBy=C.SystemId
								 LEFT JOIN SCS.WorkCenterMaster WCM ON WCM.Id=P.WorkCenterMasterId
								 LEFT JOIN SCS.WorkCenterMaster TWCM ON TWCM.Id=P.ToWorkCenterMasterId
                                 LEFT JOIN HKP.Process FP ON FP.Id=P.ProcessId
								 LEFT JOIN HKP.Process TP ON TP.Id=P.ToProcessId
								 LEFT JOIN HKP.SFGInventory FSFG ON FSFG.Id=FromSFGInventoryId
								 LEFT JOIN HKP.SFGInventory TSFG ON TSFG.Id=ToSFGInventoryId
                                 LEFT JOIN TRN.ProductionOrder PO ON PO.Id=P.ProductionOrderId
	                                LEFT JOIN 
								   (select distinct POD.ProductionOrderId
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								     ) PD ON PD.ProductionOrderId=PO.Id
                                 WHERE P.EntityId='" + EntityId + @"' 
								 and P.ProductionShiftId='" + ProductionShiftId + @"'  
								 and P.ProductionDate='" + ProductionDate + @"'  " + wc + " Order BY ISNULL(WCM.UserName,TWCM.UserName)";
                    return _sqlRepository.GetDataCollection(_sql, null);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetLineItemGrid(string EntityId, string ProcessId, string ProductionDate, string ProductionShiftId, string WorkCenterMasterId, string ProductionLevel)
        {
            try
            {
                if (ProductionLevel == ProductionBookingLevel.SalesOrder.ToString())
                {
                    string _sql = @"select p.Id,mo.MasterOrderNo
								,moi.Id MOrderLineNo
								,so.Id SalesOrderId,SO.Description
                                ,PO.PONumber
                                  ,mm.UserName MaterialMaster, mma.StandardName Article
								  ,b.UserName Customer
                                 --,so.ConfirmDate,so.DeliveryDate
                                 ,Replace(CONVERT(VARCHAR(11), so.ConfirmDate, 106), ' ', '-') ConfirmDate
								 ,Replace(CONVERT(VARCHAR(11), so.DeliveryDate, 106), ' ', '-') DeliveryDate
								 ,mo.TotalQty MOQty
								 ,moi.TotalQty MOIQty
                                 ,so.Qty SOQty,p.Quantity ,p.ProductionBookingPeriodId,p.ProductionGrade 
								,u.UserName UOM
								,moi.ExtraOrderPercentage [ExtraP],moi.OrderWastagePercentage [WastageP]
								,mma.Id ArticleId,mm.Id MaterialMasterId,mmc.CharCount, p.PlantID,p.WorkCenterMasterId,EP.ProductionBookingLevel
                                ,PBP.UserName ProductionBookingPeriod,P.ResponsiblePersonId,R.EmployeeName ResponsiblePersonName,P.MentorId, M.EmployeeName MentorName
                                ,FORMAT (P.InTime, 'dd-MMM-yyyy hh:mm:tt') InTime, FORMAT (P.OutTime, 'dd-MMM-yyyy hh:mm:tt') OutTime,P.ConsumeHour,P.ManPower, P.CheckedBy,C.EmployeeName CheckedByName,p.LotNumber
                                ,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem,so.Description,P.ProductionOrderId
                                 FROM [TRN].[ProductionSummary] p
								 LEFT JOIN trn.SalesOrder so on so.Id=p.SalesOrderId
                                 LEFT JOIN trn.[MasterOrderItem] moi on moi.id=so.MasterOrderItemId
                                 LEFT JOIN trn.MasterOrder mo on mo.id=moi.MasterOrderId
                                 LEFT JOIN hkp.Party b on b.id=mo.PartyId
								 LEFT JOIN scs.UnitOfMeasurement u on u.id=mo.TotalQtyUOMId
                                 LEFT JOIN mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                 LEFT JOIN mst.MaterialMasterArticle mma on mma.id=moi.ArticleId
                                 LEFT JOIN (
											SELECT count(Id) CharCount,MaterialMasterId from [MST].[MaterialMasterCharacteristics] group by  MaterialMasterId
											) mmc on mmc.MaterialMasterId=mm.id
                                 LEFT JOIN [HKP].[EntityProcessTag] EP ON EP.ProcessId=P.ProcessId and EP.EntityId=P.EntityId
                                 LEFT JOIN [HKP].[ProductionBookingPeriod] PBP ON PBP.Id=P.ProductionBookingPeriodId
                                 LEFT JOIN EmployeeInformation R ON P.ResponsiblePersonId=R.SystemId
                                 LEFT JOIN EmployeeInformation M ON P.MentorId=M.SystemId
                                 LEFT JOIN [TRN].[CustomerPO] PO ON PO.Id=SO.CustomerPOId
                                 LEFT JOIN EmployeeInformation C ON P.CheckedBy=C.SystemId
                                 WHERE p.EntityId='" + EntityId + @"' 
								 and p.ProcessId='" + ProcessId + @"' 
								 and p.WorkCenterMasterId='" + WorkCenterMasterId + @"' 
								 and p.ProductionShiftId='" + ProductionShiftId + @"'  
								 and p.ProductionDate='" + ProductionDate + @"' ";
                    return _sqlRepository.GetDataCollection(_sql, null);

                }
                else if (ProductionLevel == ProductionBookingLevel.ProductionOrder.ToString())
                {
                    string _sql = @"SELECT PS.Id,PS.ProductionOrderId,FORMAT(PS.ProductionDate,'dd-MMM-yyyy') ProductionDate, PS.ProductionGrade, PS.Quantity, PBP.UserName ProductionBookingPeriod
                                    ,PS.ResponsiblePersonId,R.EmployeeName ResponsiblePersonName,PS.MentorId, M.EmployeeName MentorName, PS.CheckedBy,C.EmployeeName CheckedByName
                                    ,FORMAT (PS.InTime, 'dd-MMM-yyyy hh:mm:tt') InTime, FORMAT (PS.OutTime, 'dd-MMM-yyyy hh:mm:tt') OutTime,PS.ConsumeHour,PS.ManPower,ps.LotNumber
	                                 ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem
                                    FROM TRN.ProductionSummary PS
                                    LEFT JOIN HKP.ProductionBookingPeriod PBP ON PBP.Id=PS.ProductionBookingPeriodId
                                    LEFT JOIN EmployeeInformation R ON PS.ResponsiblePersonId=R.SystemId
                                    LEFT JOIN EmployeeInformation M ON PS.MentorId=M.SystemId
                                    LEFT JOIN EmployeeInformation C ON PS.CheckedBy=C.SystemId
	                                 LEFT JOIN TRN.ProductionOrder PO ON PO.Id=PS.ProductionOrderId
	                                LEFT JOIN 
								   (select distinct POD.ProductionOrderId
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								     ) PD ON PD.ProductionOrderId=PO.Id
                                 WHERE PS.EntityId='" + EntityId + @"' 
								 and PS.ProcessId='" + ProcessId + @"' 
								 and PS.WorkCenterMasterId='" + WorkCenterMasterId + @"' 
								 and PS.ProductionShiftId='" + ProductionShiftId + @"'  
								 and PS.ProductionDate='" + ProductionDate + @"' ";
                    return _sqlRepository.GetDataCollection(_sql, null);
                }
                else if (ProductionLevel == ProductionBookingLevel.MasterOrderItem.ToString())
                {
                    string _sql = @"select p.Id,mo.MasterOrderNo,MOI.Id MasterOrderItemId
								,moi.Id MOrderLineNo
								,so.Id SalesOrderId,SO.Description
                                ,PO.PONumber
                                  ,mm.UserName MaterialMaster, mma.StandardName Article
								  ,b.UserName Customer
                                 --,so.ConfirmDate,so.DeliveryDate
                                 ,Replace(CONVERT(VARCHAR(11), so.ConfirmDate, 106), ' ', '-') ConfirmDate
								 ,Replace(CONVERT(VARCHAR(11), so.DeliveryDate, 106), ' ', '-') DeliveryDate
								 ,mo.TotalQty MOQty
								 ,moi.TotalQty MOIQty
                                 ,so.Qty SOQty,p.Quantity ,p.ProductionBookingPeriodId,p.ProductionGrade 
								,u.UserName UOM
								,moi.ExtraOrderPercentage [ExtraP],moi.OrderWastagePercentage [WastageP]
								,mma.Id ArticleId,mm.Id MaterialMasterId,mmc.CharCount, p.PlantID,p.WorkCenterMasterId,EP.ProductionBookingLevel
                                ,PBP.UserName ProductionBookingPeriod,P.ResponsiblePersonId,R.EmployeeName ResponsiblePersonName,P.MentorId, M.EmployeeName MentorName
                                ,FORMAT (P.InTime, 'dd-MMM-yyyy hh:mm:tt') InTime, FORMAT (P.OutTime, 'dd-MMM-yyyy hh:mm:tt') OutTime,P.ConsumeHour,P.ManPower, P.CheckedBy,C.EmployeeName CheckedByName,p.LotNumber
                                ,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem,so.Description,P.ProductionOrderId
                                 FROM [TRN].[ProductionSummary] p
								 LEFT JOIN trn.SalesOrder so on so.Id=p.SalesOrderId
                                 LEFT JOIN trn.[MasterOrderItem] moi on moi.id=P.MasterOrderItemId
                                 LEFT JOIN trn.MasterOrder mo on mo.id=moi.MasterOrderId
                                 LEFT JOIN hkp.Party b on b.id=mo.PartyId
								 LEFT JOIN scs.UnitOfMeasurement u on u.id=mo.TotalQtyUOMId
                                 LEFT JOIN mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                 LEFT JOIN mst.MaterialMasterArticle mma on mma.id=moi.ArticleId
                                 LEFT JOIN (
											SELECT count(Id) CharCount,MaterialMasterId from [MST].[MaterialMasterCharacteristics] group by  MaterialMasterId
											) mmc on mmc.MaterialMasterId=mm.id
                                 LEFT JOIN [HKP].[EntityProcessTag] EP ON EP.ProcessId=P.ProcessId and EP.EntityId=P.EntityId
                                 LEFT JOIN [HKP].[ProductionBookingPeriod] PBP ON PBP.Id=P.ProductionBookingPeriodId
                                 LEFT JOIN EmployeeInformation R ON P.ResponsiblePersonId=R.SystemId
                                 LEFT JOIN EmployeeInformation M ON P.MentorId=M.SystemId
                                 LEFT JOIN [TRN].[CustomerPO] PO ON PO.Id=SO.CustomerPOId
                                 LEFT JOIN EmployeeInformation C ON P.CheckedBy=C.SystemId
                                 WHERE p.EntityId='" + EntityId + @"' 
								 and p.ProcessId='" + ProcessId + @"' 
								 and p.WorkCenterMasterId='" + WorkCenterMasterId + @"' 
								 and p.ProductionShiftId='" + ProductionShiftId + @"'  
								 and p.ProductionDate='" + ProductionDate + @"' ";
                    return _sqlRepository.GetDataCollection(_sql, null);

                }
                else
                {
                    string _sql = @"select p.Id,mo.MasterOrderNo,PL.Code ProductCode,P.ProductLibraryId
								,moi.Id MOrderLineNo
								,so.Id SalesOrderId,SO.Description
                                ,PO.PONumber
                                  ,mm.UserName MaterialMaster, mma.StandardName Article
								  ,b.UserName Customer
                                 --,so.ConfirmDate,so.DeliveryDate
                                 ,Replace(CONVERT(VARCHAR(11), so.ConfirmDate, 106), ' ', '-') ConfirmDate
								 ,Replace(CONVERT(VARCHAR(11), so.DeliveryDate, 106), ' ', '-') DeliveryDate
								 ,mo.TotalQty MOQty
								 ,moi.TotalQty MOIQty
                                 ,so.Qty SOQty,p.Quantity ,p.ProductionBookingPeriodId,p.ProductionGrade 
								,u.UserName UOM
								,moi.ExtraOrderPercentage [ExtraP],moi.OrderWastagePercentage [WastageP]
								,mma.Id ArticleId,mm.Id MaterialMasterId,mmc.CharCount, p.PlantID,p.WorkCenterMasterId,EP.ProductionBookingLevel
                                ,PBP.UserName ProductionBookingPeriod,P.ResponsiblePersonId,R.EmployeeName ResponsiblePersonName,P.MentorId, M.EmployeeName MentorName
                                ,FORMAT (P.InTime, 'dd-MMM-yyyy hh:mm:tt') InTime, FORMAT (P.OutTime, 'dd-MMM-yyyy hh:mm:tt') OutTime,P.ConsumeHour,P.ManPower, P.CheckedBy,C.EmployeeName CheckedByName,p.LotNumber
                                ,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem,so.Description,P.ProductionOrderId
                                 FROM [TRN].[ProductionSummary] p
								 LEFT JOIN trn.SalesOrder so on so.Id=p.SalesOrderId
                                 LEFT JOIN trn.[MasterOrderItem] moi on moi.id=so.MasterOrderItemId
                                 LEFT JOIN trn.MasterOrder mo on mo.id=moi.MasterOrderId
                                 LEFT JOIN hkp.Party b on b.id=mo.PartyId
								 LEFT JOIN scs.UnitOfMeasurement u on u.id=mo.TotalQtyUOMId
                                 LEFT JOIN mst.MaterialMaster mm on mm.id=p.MaterialMasterId
                                 LEFT JOIN mst.MaterialMasterArticle mma on mma.id=p.ArticleId
                                 LEFT JOIN (
											SELECT count(Id) CharCount,MaterialMasterId from [MST].[MaterialMasterCharacteristics] group by  MaterialMasterId
											) mmc on mmc.MaterialMasterId=mm.id
                                 LEFT JOIN [HKP].[EntityProcessTag] EP ON EP.ProcessId=P.ProcessId and EP.EntityId=P.EntityId
                                 LEFT JOIN [HKP].[ProductionBookingPeriod] PBP ON PBP.Id=P.ProductionBookingPeriodId
                                 LEFT JOIN EmployeeInformation R ON P.ResponsiblePersonId=R.SystemId
                                 LEFT JOIN EmployeeInformation M ON P.MentorId=M.SystemId
                                 LEFT JOIN [TRN].[CustomerPO] PO ON PO.Id=SO.CustomerPOId
                                 LEFT JOIN EmployeeInformation C ON P.CheckedBy=C.SystemId								 
                                LEFT JOIN dbo.ProductLibrary PL ON PL.Id=P.ProductLibraryId
                                 WHERE p.EntityId='" + EntityId + @"' 
								 and p.ProcessId='" + ProcessId + @"' 
								 and p.WorkCenterMasterId='" + WorkCenterMasterId + @"' 
								 and p.ProductionShiftId='" + ProductionShiftId + @"'  
								 and p.ProductionDate='" + ProductionDate + @"' ";
                    return _sqlRepository.GetDataCollection(_sql, null);

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSFGWIPQty(string EntityId, string processId, string workCenterMasterId, string salesOrderId, string productionOrderId, string status, bool IsCrossAllowed)
        {
            try
            {
                string sql = "";
                if (workCenterMasterId == "null" || workCenterMasterId == "undefined")
                {
                    workCenterMasterId = string.Empty;
                }
                if (productionOrderId == "null" || productionOrderId == "undefined")
                {
                    productionOrderId = string.Empty;
                }

                if (IsCrossAllowed == false)
                {
                    if (status == "PROCESS")
                    {
                        sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                                (SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToProcessId ProcessId,PS.SalesOrderId
                                FROM trn.ProductionSummary AS ps
                                WHERE ps.ToProcessId='" + processId + @"' AND PS.ToEntityId='" + EntityId + @"' AND ps.ToWorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                                union all

                                SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity,PS.ProcessId,PS.SalesOrderId
                                FROM trn.ProductionSummary AS ps
                                WHERE ps.ProcessId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND ps.WorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                                union all

                                SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END AS KillQuantity,PS.ProcessId,PS.SalesOrderId
                                FROM trn.ProductionSummary AS ps
                                WHERE ps.ProcessId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND ps.WorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                                ) AS K ";


                    }
                    else
                    {
                        sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                               (SELECT ps.ProductionOrderId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToSFGInventoryId,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.ToSFGInventoryId='" + processId + @"' AND PS.ToEntityId='" + EntityId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                               union all

                               SELECT ps.ProductionOrderId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity,PS.FromSFGInventoryId ,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.FromSFGInventoryId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                               union all

                               SELECT ps.ProductionOrderId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END AS KillQuantity,PS.FromSFGInventoryId,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.FromSFGInventoryId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                               ) AS K ";


                    }
                }
                else
                {
                    if (status == "PROCESS")
                    {
                        sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                                (SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToProcessId ProcessId,PS.SalesOrderId
                                FROM trn.ProductionSummary AS ps
                                WHERE ps.ToProcessId='" + processId + @"' AND ps.ToWorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                                union all

                                SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity,PS.ProcessId,PS.SalesOrderId
                                FROM trn.ProductionSummary AS ps
                                WHERE ps.ProcessId='" + processId + @"' AND ps.WorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                                union all

                                SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END AS KillQuantity,PS.ProcessId,PS.SalesOrderId
                                FROM trn.ProductionSummary AS ps
                                WHERE ps.ProcessId='" + processId + @"' AND ps.WorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                                ) AS K ";


                    }
                    else
                    {
                        sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                               (SELECT ps.ProductionOrderId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToSFGInventoryId,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.ToSFGInventoryId='" + processId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                               union all

                               SELECT ps.ProductionOrderId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity,PS.FromSFGInventoryId ,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.FromSFGInventoryId='" + processId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                               union all

                               SELECT ps.ProductionOrderId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END AS KillQuantity,PS.FromSFGInventoryId,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.FromSFGInventoryId='" + processId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                               ) AS K ";


                    }

                }
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetWIPQtyForValidation(string Id, string EntityId, string processId, string workCenterMasterId, string salesOrderId, string productionOrderId, string status, bool IsCrossAllowed)
        {
            string sql = "";
            if (workCenterMasterId == "null" || workCenterMasterId == "undefined")
            {
                workCenterMasterId = string.Empty;
            }
            if (productionOrderId == "null" || productionOrderId == "undefined")
            {
                productionOrderId = string.Empty;
            }

            if (IsCrossAllowed == false)
            {
                if (status == "PROCESS")
                {
                    sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                            (
                            SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToProcessId ProcessId,PS.SalesOrderId
                            FROM trn.ProductionSummary AS ps
                            WHERE ps.ToProcessId='" + processId + @"' AND PS.EntityId='" + EntityId + @"'  AND ps.ToWorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                            union all
                            SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity, Quantity,0 AS KillQuantity,PS.ProcessId,PS.SalesOrderId
                            FROM trn.ProductionSummary AS ps
                            WHERE ps.ProcessId='" + processId + @"' AND PS.EntityId='" + EntityId + @"'  AND ps.WorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                            and ps.id<>'" + Id + @"'
                            ) AS K ";


                }
                else
                {
                    sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                        (
                        SELECT ps.ProductionOrderId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToSFGInventoryId,PS.SalesOrderId
                        FROM trn.ProductionSummary AS ps
                        WHERE ps.ToSFGInventoryId='" + processId + @"' AND PS.EntityId='" + EntityId + @"'  AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                        union all
                        SELECT ps.ProductionOrderId,0 AS InQuantity, Quantity,0 AS KillQuantity,PS.ToSFGInventoryId,PS.SalesOrderId
                        FROM trn.ProductionSummary AS ps
                        WHERE ps.ToSFGInventoryId='" + processId + @"' AND PS.EntityId='" + EntityId + @"'  AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                            and ps.id<>'" + Id + @"'
                        ) AS K  ";

                }
            }
            else
            {
                if (status == "PROCESS")
                {
                    sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                            (
                            SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToProcessId ProcessId,PS.SalesOrderId
                            FROM trn.ProductionSummary AS ps
                            WHERE ps.ToProcessId='" + processId + @"' AND ps.ToWorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                            union all
                            SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity, Quantity,0 AS KillQuantity,PS.ProcessId,PS.SalesOrderId
                            FROM trn.ProductionSummary AS ps
                            WHERE ps.ProcessId='" + processId + @"' AND ps.WorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                            and ps.id<>'" + Id + @"'
                            ) AS K ";

                }
                else
                {
                    sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                        (
                        SELECT ps.ProductionOrderId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToSFGInventoryId,PS.SalesOrderId
                        FROM trn.ProductionSummary AS ps
                        WHERE ps.ToSFGInventoryId='" + processId + @"'  AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                        union all
                        SELECT ps.ProductionOrderId,0 AS InQuantity, Quantity,0 AS KillQuantity,PS.ToSFGInventoryId,PS.SalesOrderId
                        FROM trn.ProductionSummary AS ps
                        WHERE ps.ToSFGInventoryId='" + processId + @"'  AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
                            and ps.id<>'" + Id + @"'
                        ) AS K  ";

                }
            }
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public Dictionary<string, object> GetWIPQtyValidation(string Id, string EntityId, string processId, string workCenterMasterId, string salesOrderId, string productionOrderId, string status, bool IsCrossAllowed)
        {
            string sql = "";
            if (workCenterMasterId == "null" || workCenterMasterId == "undefined")
            {
                workCenterMasterId = string.Empty;
            }
            if (productionOrderId == "null" || productionOrderId == "undefined")
            {
                productionOrderId = string.Empty;
            }
            if (string.IsNullOrEmpty(salesOrderId))
            {
                salesOrderId = "null";
            }
            //if (IsCrossAllowed == false)
            //{
            if (status == "PROCESS")
            {
                sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                            (
                            SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToProcessId ProcessId,PS.SalesOrderId
                            FROM trn.ProductionSummary AS ps
                            WHERE ps.ToProcessId='" + processId + @"' AND PS.ToEntityId='" + EntityId + @"' AND ps.ToWorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
                            union all
                            SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity, Quantity,0 AS KillQuantity,PS.ProcessId,PS.SalesOrderId
                            FROM trn.ProductionSummary AS ps
                            WHERE ps.ProcessId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND ps.WorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
                            ) AS K ";


            }
            else
            {

                sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                               (SELECT ps.ProductionOrderId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.FromSFGInventoryId,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.ToSFGInventoryId='" + processId + @"' AND PS.ToEntityId='" + EntityId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
                               union all

                               SELECT ps.ProductionOrderId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity,PS.FromSFGInventoryId ,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.FromSFGInventoryId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
                               union all

                               SELECT ps.ProductionOrderId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END AS KillQuantity,PS.FromSFGInventoryId,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.FromSFGInventoryId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
                               ) AS K ";
            }
            //}
            //else
            //{
            //    if (status == "PROCESS")
            //    {
            //        sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
            //                (
            //                SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToProcessId ProcessId,PS.SalesOrderId
            //                FROM trn.ProductionSummary AS ps
            //                WHERE ps.ToProcessId='" + processId + @"' AND ps.ToWorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
            //                union all
            //                SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity, Quantity,0 AS KillQuantity,PS.ProcessId,PS.SalesOrderId
            //                FROM trn.ProductionSummary AS ps
            //                WHERE ps.ProcessId='" + processId + @"' AND ps.WorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
            //                ) AS K ";

            //    }
            //    else
            //    {
            //        sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
            //                   (SELECT ps.ProductionOrderId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.FromSFGInventoryId,PS.SalesOrderId
            //                   FROM trn.ProductionSummary AS ps
            //                   WHERE ps.ToSFGInventoryId='" + processId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
            //                   union all

            //                   SELECT ps.ProductionOrderId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity,PS.FromSFGInventoryId ,PS.SalesOrderId
            //                   FROM trn.ProductionSummary AS ps
            //                   WHERE ps.FromSFGInventoryId='" + processId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
            //                   union all

            //                   SELECT ps.ProductionOrderId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END AS KillQuantity,PS.FromSFGInventoryId,PS.SalesOrderId
            //                   FROM trn.ProductionSummary AS ps
            //                   WHERE ps.FromSFGInventoryId='" + processId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
            //                   ) AS K ";
            //    }
            //}
            return _sqlRepository.GetData(sql);
        }

        public IEnumerable<object> GetTotalPOQty(string productionOrderId, string processId)
        {
            try
            {
                string sql = "";
               //string sql = @"SELECT PlannedQty=CASE WHEN PQ.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE PQ.Qty END
               //            ,(CASE WHEN PQ.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE PQ.Qty END-ISNULL(CEILING(PRS.TotalProductionQty),0)) RemainingQty
               //            , ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
               //            FROM trn.ProductionOrder AS PO
               //            LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID=PO.Id
               //            LEFT JOIN 
               //            (SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId
               //            FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"'  GROUP BY PS.ProductionOrderId
               //            ) AS PRS ON PRS.ProductionOrderId = PO.Id WHERE PO.Id ='" + productionOrderId + @"' GROUP BY TotalProductionQty,PQ.Qty";
               
                //         sql = @"SELECT (PO.PlannedQty*PPS.Qty/100) PlannedQty,ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty,((PO.PlannedQty*PPS.Qty/100) -ISNULL(CEILING(PRS.TotalProductionQty),0))RemainingQty
       //                     FROM trn.ProductionOrder AS PO
       //                     LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID=PO.Id
							//LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID=PO.Id AND PPS.ProcessId='" + processId + @"'
       //                     LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId
       //                     FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.ProductionOrderId
       //                     ) AS PRS ON PRS.ProductionOrderId = PO.Id WHERE PO.Id ='" + productionOrderId + @"'";

                 sql = @"SELECT PlannedQty=ISNULL(CASE WHEN ISNULL(PPS.Qty,0)=0 THEN ISNULL(PQ.Qty,PO.PlannedQty) ELSE PO.PlannedQty*PPS.Qty/100 END,0)
,ISNULL((CASE WHEN ISNULL(PPS.Qty,0)=0 THEN ISNULL(PQ.Qty,PO.PlannedQty) ELSE PO.PlannedQty*PPS.Qty/100 END)-ISNULL(CEILING(PRS.TotalProductionQty), 0),0) RemainingQty
, ISNULL(CEILING(PRS.TotalProductionQty), 0)TotalProductionQty
                             FROM trn.ProductionOrder AS PO
                             LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id AND PPS.ProcessId = '" + processId + @"'
                            LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID = PO.Id
                             LEFT JOIN
                            (SELECT SUM(PS.Quantity) TotalProductionQty, PS.ProductionOrderId
                            FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"'  GROUP BY PS.ProductionOrderId
                            ) AS PRS ON PRS.ProductionOrderId = PO.Id WHERE PO.Id = '" + productionOrderId + @"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPOQty(string productionOrderId, string processId)
        {
            try
            {
                string sql = "";
                sql = @"SELECT PlannedQty=SOP.OrderQty,POQ.POQty,isnull(PQ.Qty,POQ.POQty)/POQ.POQty*SOP.OrderQty*PPS.Qty/100-ISNULL(CEILING(PRS.TotalProductionQty), 0) as RemainingQty
,ISNULL(CEILING(PRS.TotalProductionQty), 0)TotalProductionQty,isnull(PQ.Qty,POQ.POQty) as TotalActualPlannedQty,PPS.Qty TotalProcessPlanPercentage
,isnull(PQ.Qty,POQ.POQty)/POQ.POQty*SOP.OrderQty*PPS.Qty/100 as ProcessPlanQty,
isnull(PQ.Qty, POQ.POQty)/ POQ.POQty * SOP.OrderQty * PPS.Qty / 100 - ISNULL(CEILING(PRS.TotalProductionQty), 0) as CurPOBalProd,isnull(PPP.TotalProductionQty,0)  as POPreviousProdQty,isnull(FPP.FirstProductionQty,0) POFirstProcessProductionQty,PPS.Sequence POProcessSequence
                             FROM trn.ProductionOrder AS PO
                             LEFT JOIN TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderID = PO.Id AND PPS.ProcessId = '" + processId + @"'
                            LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID = PO.Id
							LEFT JOIN (select SUM(PP.Quantity)TotalProductionQty, PP.ProductionOrderId from [TRN].[ProductionSummary] PP where PP.ProcessId = 
(select ProcessId from TRN.ProductionOrderProcessSet B where B.ProductionOrderId=PP.ProductionOrderId  and B.Sequence =
(select top 1 Sequence=Sequence - 1  from TRN.ProductionOrderProcessSet A where A.ProductionOrderId=PP.ProductionOrderId and A.ProcessId='" + processId + @"')) GROUP BY PP.ProductionOrderId
 ) AS PPP ON PPP.ProductionOrderId = PO.Id
LEFT JOIN (select Sum(FP.Quantity) as FirstProductionQty, FP.ProductionOrderId from [TRN].[ProductionSummary] FP where FP.ProcessId = 
(select ProcessId from TRN.ProductionOrderProcessSet B where B.ProductionOrderId=FP.ProductionOrderId  and B.Sequence = 
(select top 1 Sequence from TRN.ProductionOrderProcessSet A where A.ProductionOrderId=FP.ProductionOrderId)) GROUP BY FP.ProductionOrderId 
 ) AS FPP ON FPP.ProductionOrderId = PO.Id
                             LEFT JOIN
                            (SELECT SUM(SO.Qty) OrderQty, PD.ProductionOrderId
                            FROM TRN.SalesOrder SO
							left join TRN.ProductionOrderDetail PD ON PD.SalesOrderId=SO.Id
							where SO.OrderStatusId<>'Cancelled' GROUP BY PD.ProductionOrderId
                            ) AS SOP ON SOP.ProductionOrderId = PO.Id
                            LEFT JOIN
                            (SELECT SUM(SO.Qty) POQty, PD.ProductionOrderId
                            FROM TRN.SalesOrder SO
							left join TRN.ProductionOrderDetail PD ON PD.SalesOrderId=SO.Id
							where SO.OrderStatusId<>'Cancelled' GROUP BY PD.ProductionOrderId
                            ) AS POQ ON POQ.ProductionOrderId = PO.Id
                            LEFT JOIN
                            (SELECT SUM(PS.Quantity) TotalProductionQty, PS.ProductionOrderId
                            FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"'  GROUP BY PS.ProductionOrderId
                            ) AS PRS ON PRS.ProductionOrderId = PO.Id WHERE PO.Id = '" + productionOrderId + @"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEntityProcessOrderTotalQty(string EntityId, string processId, string SalesOrderId, string ProductionOrderId, string status)
        {
            try
            {
                var sql = "";
                if (status == "PROCESS")
                {
                    sql = @"Select SUM(Quantity) EntityProcessOrderQty from TRN.ProductionSummary Where EntityId='" + EntityId + "' AND ProcessId='" + processId + "'   AND (SalesOrderId='" + SalesOrderId + @"' OR ProductionOrderId='" + ProductionOrderId + "')";
                    return _sqlRepository.GetDataCollection(sql, null);
                }
                else
                {
                    sql = @"Select SUM(Quantity) EntityProcessOrderQty from TRN.ProductionSummary Where EntityId='" + EntityId + "' AND  FromSFGInventoryId='" + processId + "'  AND (SalesOrderId='" + SalesOrderId + @"' OR ProductionOrderId='" + ProductionOrderId + "')";
                    return _sqlRepository.GetDataCollection(sql, null);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetProcessParaData(string processId, string masterId, string ProductionOrderId)
        {
            try
            {
                var sql = "";
                if (masterId != "null")
                {
                    sql = @"SELECT A.Id,P.UserName,P.Formula,P.FormulaId,P.EntryState,ValueIN = CASE WHEN P.ValueinDecimal=1 THEN 'Decimal' ELSE 'Percentage' END
,Value=CASE WHEN A.Value IS NOT NULL THEN A.Value ELSE (CASE WHEN P.ValueinDecimal=1 THEN P.DefaultValue ELSE P.DefaultValue/100 END) END
,P.Id ProductionBookingParameterId,P.IsProduction
FROM dbo.ProductionBookingParameter P
LEFT JOIN [dbo].[ProductionSummaryParameterValue] A ON A.ProductionBookingParameterId=P.Id AND ISNULL(A.ProductionSummaryId,'" + masterId + @"')='" + masterId + @"'
WHERE p.ProductionBookingProcessParameterId=(SELECT Id FROM dbo.ProductionBookingProcessParameter WHERE ProcessId='" + processId + "') AND P.Active=1 ORDER BY P.Sequence";
                }
                else
                {
                    sql = @"SELECT A.Id,P.UserName,P.Formula,P.FormulaId,P.EntryState,ValueIN = CASE WHEN P.ValueinDecimal=1 THEN 'Decimal' ELSE 'Percentage' END
,Value=CASE WHEN PD.Value IS NOT NULL THEN PD.Value ELSE (CASE WHEN P.ValueinDecimal=1 THEN P.DefaultValue ELSE P.DefaultValue/100 END) END
,P.Id ProductionBookingParameterId,P.IsProduction
FROM dbo.ProductionBookingParameter P
LEFT JOIN [dbo].[ProductionSummaryParameterValue] A ON A.ProductionBookingParameterId=P.Id AND ISNULL(A.ProductionSummaryId,'null')='null'
LEFT JOIN (SELECT * FROM [dbo].[ProductionSummaryParameterValue] WHERE ProductionSummaryId=(SELECT TOP(1) Id FROM TRN.ProductionSummary WHERE ProductionOrderId='" + ProductionOrderId + @"' AND ProcessId='" + processId + @"' ORDER BY AddedDate DESC))PD ON PD.UserName=P.UserName
WHERE p.ProductionBookingProcessParameterId=(SELECT Id FROM dbo.ProductionBookingProcessParameter WHERE ProcessId='" + processId + @"') AND P.Active=1 ORDER BY P.Sequence";
                }
                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetProcessData(string entityId)
        {
            try
            {
                var sql = "";
                {
                    sql = @"SELECT DISTINCT P.Id AS [Value], P.UserName AS [Text]
							FROM HKP.EntityProcessTag AS EP
                            JOIN HKP.Process AS P ON EP.ProcessId=P.Id 
                            where EP.EntityId in (" + entityId + @") AND P.Active=1";
                }

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetDetentionParaData(string DetentionId, string processId, string masterId)
        {
            try
            {
                //var sql = "";
                // sql = @"select * from DetentionMasterMachineParameter where DetentionMasterId='" + DetentionId + @"' ORDER BY Sequence";

                var sql = "";
                if (masterId != "null")
                {
                    sql = @"SELECT A.Id,P.UserName,P.Formula,P.FormulaId,P.EntryState,ValueIN = CASE WHEN P.ValueinDecimal=1 THEN 'Decimal' ELSE 'Percentage' END
,Value=CASE WHEN A.Value IS NOT NULL THEN A.Value ELSE (CASE WHEN P.ValueinDecimal=1 THEN P.DefaultValue ELSE P.DefaultValue/100 END) END
,P.Id DetentionMasterMachineParameterId,P.IsProduction
FROM dbo.DetentionMasterMachineParameter P
LEFT JOIN [dbo].[DetentionMasterMachineParameterValue] A ON A.DetentionMasterMachineParameterId=P.Id AND ISNULL(A.MachineMasterTransactionId,'" + masterId + @"')='" + masterId + @"'
WHERE p.DetentionMasterId='" + DetentionId + @"' ORDER BY P.Sequence";
                }
                else
                {
                    sql = @"SELECT A.Id,P.UserName,P.Formula,P.FormulaId,P.EntryState,ValueIN = CASE WHEN P.ValueinDecimal=1 THEN 'Decimal' ELSE 'Percentage' END
,Value=CASE WHEN PD.Value IS NOT NULL THEN PD.Value ELSE (CASE WHEN P.ValueinDecimal=1 THEN P.DefaultValue ELSE P.DefaultValue/100 END) END
,P.Id DetentionMasterMachineParameterId,P.IsProduction
FROM dbo.DetentionMasterMachineParameter P
LEFT JOIN [dbo].[DetentionMasterMachineParameterValue] A ON A.DetentionMasterMachineParameterId=P.Id AND ISNULL(A.MachineMasterTransactionId,'null')='null'
LEFT JOIN (SELECT * FROM [dbo].[DetentionMasterMachineParameterValue] WHERE MachineMasterTransactionId=(SELECT TOP(1) Id FROM  MachineMasterTransaction WHERE  ProcessId='" + processId + @"' ORDER BY AddedDate DESC))PD ON PD.UserName=P.UserName
WHERE p.DetentionMasterId='" + DetentionId + @"' ORDER BY P.Sequence";
                }

                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetProcessDetentionData(string processId, string entityId, string productionDate, string shiftId, string workcenter)
        {
            try
            {
                var sql = "";
                sql = @"
SELECT MMT.Id, MMT.EntityId, MMT.DetentionId, MMT.DetentionType, MMT.ProcessId, MMT.DepartmentId, MMT.ShiftId, MMT.ResponsiblePersonId as ResponsiblePersonId, MMT.Remark, MMT.AddedBy, MMT.AddedDate, MMT.AddedFromIP, MMT.UpdatedBy, MMT.UpdatedDate, MMT.UpdatedFromIP
,E.UserName Entity,D.UserName DepartmentName,DM.DetentionUserName Detention,FORMAT(MMT.Date,'dd-MMM-yyyy')[Date],P.UserName Process
										,format(MMT.FromTime,'hh:mm tt') as FromTime,format(MMT.ToTime,'hh:mm tt') as ToTime,MMT.Minute,SD.UserName Shift,
										EI.EmployeeName ResponsiblePerson,EI.EmployeeCode ResponsiblePersonCode,MMT.Remark,MMT.WorkCenterId,WC.UserName as WorkCenter
			                            from MachineMasterTransaction MMT
			                            left join ORG.Entity E on E.Id=MMT.EntityId
										left join ORG.Department D on D.Id=MMT.DepartmentId
										left join DetentionMaster DM on DM.Id=MMT.DetentionId
										left join HKP.Process P on P.Id=MMT.ProcessId
										left join ShiftDefination SD on SD.SystemID=MMT.ShiftId
										left Join SCS.WorkCenterMaster WC on WC.id=MMT.WorkCenterId
										left join EmployeeInformation EI on EI.SystemId=MMT.ResponsiblePersonId
                where MMT.EntityId = '" + entityId + "' and MMT.ProcessId = '" + processId + "'  and MMT.Date = '" + productionDate + "' and MMT.ShiftId = '" + shiftId + "' and MMT.WorkCenterId = '" + workcenter + "'";


                return _sqlRepository.GetDataCollection(sql, null);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ReLoadFormulaWithValue(string strFormulaID, ref DataTable dtValue, out string lblFormulaValue)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;

            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = strFormulaID.Trim();

                lblFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        dvLocal = new DataView();
                        dvLocal.Table = dtValue;

                        dvLocal.RowFilter = "ProductionBookingParameterId = '" + strTemp.Trim() + "'";
                        if (dvLocal.Count > 0)
                        {
                            if (dvLocal[0]["Amount"].ToString().Trim() == "")
                            {
                                strTemp = "0";
                            }
                            else
                            {
                                strTemp = dvLocal[0]["Amount"].ToString().Trim();
                            }
                        }
                    }

                    lblFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End 

        public void ReLoadDetentionFormulaWithValue(string strFormulaID, ref DataTable dtValue, out string lblFormulaValue)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;

            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = strFormulaID.Trim();

                lblFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {
                        dvLocal = new DataView();
                        dvLocal.Table = dtValue;

                        dvLocal.RowFilter = "DetentionMasterMachineParameterId = '" + strTemp.Trim() + "'";
                        if (dvLocal.Count > 0)
                        {
                            if (dvLocal[0]["Amount"].ToString().Trim() == "")
                            {
                                strTemp = "0";
                            }
                            else
                            {
                                strTemp = dvLocal[0]["Amount"].ToString().Trim();
                            }
                        }
                    }

                    lblFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End 
        public IEnumerable<object> GetSFGTotalQty(string salesOrderId, string processId, string status)
        {
            try
            {

                if (status == "PROCESS")
                {
                    var sql = @"SELECT CEILING(SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty
                            ,ISNULL(CEILING((SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) - ISNULL(CEILING(PRS.TotalProductionQty),0))),0) RemainingQty, ISNULL(CEILING(PRS.TotalProductionQty),0) TotalProductionQty
                                FROM trn.SalesOrder AS so
                                INNER JOIN TRN.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId
	                            FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"'  GROUP BY PS.SalesOrderId
	                            ) AS PRS ON PRS.SalesOrderId = SO.Id WHERE so.Id ='" + salesOrderId + "' GROUP BY TotalProductionQty";
                    return _sqlRepository.GetDataCollection(sql, null);
                }
                else
                {
                    var sql = @"SELECT CEILING(SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty
                            ,ISNULL(CEILING((SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) - ISNULL(CEILING(PRS.TotalProductionQty),0))),0) RemainingQty, ISNULL(CEILING(PRS.TotalProductionQty),0) TotalProductionQty
                                FROM trn.SalesOrder AS so
                                INNER JOIN TRN.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId
	                            FROM [TRN].[ProductionSummary] PS WHERE PS.FromSFGInventoryId = '" + processId + @"'  GROUP BY PS.SalesOrderId
	                            ) AS PRS ON PRS.SalesOrderId = SO.Id WHERE so.Id ='" + salesOrderId + "' GROUP BY TotalProductionQty";
                    return _sqlRepository.GetDataCollection(sql, null);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSFGTotalPOQty(string productionOrderId, string processId, string status)
        {
            try
            {
                if (status == "PROCESS")
                {

                    var sql = @"SELECT  PlannedQty=CASE WHEN S.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE S.Qty END
                          ,ISNULL(CEILING((CASE WHEN S.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE S.Qty END) - ISNULL(CEILING(PRS.TotalProductionQty),0)),0) RemainingQty, ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
                         FROM trn.ProductionOrder AS PO
						 LEFT JOIN (Select Qty, ProductionOrderId from productionOrderSchedulingParametersType1  WHERE ProductionOrderId ='" + productionOrderId + @"') S ON S.ProductionOrderId = PO.Id 	
                         LEFT JOIN (
						 SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId
	                     FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.ProductionOrderId						
	                     ) AS PRS ON PRS.ProductionOrderId = PO.Id WHERE PO.Id ='" + productionOrderId + @"' GROUP BY S.Qty,TotalProductionQty";

                    return _sqlRepository.GetDataCollection(sql, null);
                }
                else
                {
                    var sql = @"SELECT PlannedQty=CASE WHEN S.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE S.Qty END
                          ,ISNULL(CEILING((CASE WHEN S.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE S.Qty END) - ISNULL(CEILING(PRS.TotalProductionQty),0)),0) RemainingQty, ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
                         FROM trn.ProductionOrder AS PO
						 LEFT JOIN (Select Qty, ProductionOrderId from productionOrderSchedulingParametersType1  WHERE ProductionOrderId ='" + productionOrderId + @"') S ON S.ProductionOrderId = PO.Id
                         LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId
	                     FROM [TRN].[ProductionSummary] PS WHERE PS.FromSFGInventoryId =  '" + processId + @"' GROUP BY PS.ProductionOrderId
	                     ) AS PRS ON PRS.ProductionOrderId = PO.Id WHERE PO.Id ='" + productionOrderId + @"' GROUP BY S.Qty,TotalProductionQty";

                    return _sqlRepository.GetDataCollection(sql, null);
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> GetTotalSFGPOQty(string Id, string productionOrderId, string processId, string status)
        {
            try
            {
                if (status == "PROCESS")
                {
                    var sql = @"SELECT  PlannedQty=CASE WHEN S.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE S.Qty END
                          ,ISNULL(CEILING((CASE WHEN S.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE S.Qty END) - ISNULL(CEILING(PRS.TotalProductionQty),0)),0) RemainingQty, ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
                         FROM trn.ProductionOrder AS PO
						 LEFT JOIN (Select Qty, ProductionOrderId from productionOrderSchedulingParametersType1  WHERE ProductionOrderId ='" + productionOrderId + @"') S ON S.ProductionOrderId = PO.Id 	
                         LEFT JOIN (
						 SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId
	                     FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' and ps.id<>'" + Id + @"' GROUP BY PS.ProductionOrderId						
	                     ) AS PRS ON PRS.ProductionOrderId = PO.Id WHERE PO.Id ='" + productionOrderId + @"' GROUP BY S.Qty,TotalProductionQty";
                    return _sqlRepository.GetData(sql);
                }
                else
                {
                    var sql = @"SELECT  PlannedQty=CASE WHEN S.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE S.Qty END
                          ,ISNULL(CEILING((CASE WHEN S.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE S.Qty END) - ISNULL(CEILING(PRS.TotalProductionQty),0)),0) RemainingQty, ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
                         FROM trn.ProductionOrder AS PO
						 LEFT JOIN (Select Qty, ProductionOrderId from productionOrderSchedulingParametersType1  WHERE ProductionOrderId ='" + productionOrderId + @"') S ON S.ProductionOrderId = PO.Id
                         LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId
	                     FROM [TRN].[ProductionSummary] PS WHERE PS.FromSFGInventoryId =  '" + processId + @"' and ps.id<>'" + Id + @"' GROUP BY PS.ProductionOrderId
	                     ) AS PRS ON PRS.ProductionOrderId = PO.Id WHERE PO.Id ='" + productionOrderId + @"' GROUP BY S.Qty,TotalProductionQty";
                    return _sqlRepository.GetData(sql);
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> GetTotalSOSFGQty(string Id, string salesOrderId, string processId, string status)
        {
            try
            {

                if (status == "PROCESS")
                {
                    var sql = @"SELECT CEILING(SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty
                            ,ISNULL(CEILING((SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) - ISNULL(CEILING(PRS.TotalProductionQty),0))),0) RemainingQty, ISNULL(CEILING(PRS.TotalProductionQty),0) TotalProductionQty
                                FROM trn.SalesOrder AS so
                                INNER JOIN TRN.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId
	                            FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' and ps.id<>'" + Id + @"' GROUP BY PS.SalesOrderId
	                            ) AS PRS ON PRS.SalesOrderId = SO.Id WHERE so.Id ='" + salesOrderId + "' GROUP BY TotalProductionQty";
                    return _sqlRepository.GetData(sql);
                }
                else
                {
                    var sql = @"SELECT CEILING(SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty
                            ,ISNULL(CEILING((SUM((isnull(so.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) - ISNULL(CEILING(PRS.TotalProductionQty),0))),0) RemainingQty, ISNULL(CEILING(PRS.TotalProductionQty),0) TotalProductionQty
                                FROM trn.SalesOrder AS so
                                INNER JOIN TRN.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.SalesOrderId
	                            FROM [TRN].[ProductionSummary] PS WHERE PS.FromSFGInventoryId = '" + processId + @"'  and ps.id<>'" + Id + @"'  GROUP BY PS.SalesOrderId
	                            ) AS PRS ON PRS.SalesOrderId = SO.Id WHERE so.Id ='" + salesOrderId + "' GROUP BY TotalProductionQty";
                    return _sqlRepository.GetData(sql);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetPOFields(string POId)
        {
            try
            {



                var sql = @"select distinct ms.UserName AS Material,XMO.BuyerReferenceNo,MM.StandardName as Article from 
                                                            trn.SalesOrder XSO 
                                                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                                                            LEFT OUTER JOIN MST.MaterialMasterArticle MM ON MM.ID=XMOI.ArticleId
                                                            left outer join mst.MaterialMaster ms
                                                            on ms.Id=xmoi.MaterialMasterId
                                                            
                                                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                                                            left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
                                                            left outer join trn.ProductionOrder po on po.Id = Xpod.ProductionOrderId
                                                                where PO.Id='" + POId + "'";



                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSOFields(string SOId)
        {
            try
            {
                var sql = @"select distinct ms.UserName as Material,XMO.BuyerReferenceNo,MM.StandardName as Article from  trn.SalesOrder XSO
                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId = Xso.Id
                                                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id = Xso.MasterOrderItemId
                                                            LEFT OUTER JOIN MST.MaterialMasterArticle MM ON MM.ID=XMOI.ArticleId
                                                            left outer join mst.MaterialMaster ms
                                                            on ms.Id=xmoi.MaterialMasterId
                                                            
                                                            left outer join trn.MasterOrder XMO on Xmo.Id = Xmoi.MasterOrderId
                                                            left outer join[HKP].[Party] Xp on XP.Id = XMO.PartyId
                                                            where XSO.Id = '" + SOId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetWk(string plantId, string ProcessId, string EntityId)
        {
            try
            {
                var sql = @"SELECT Id as Value,UserName as Text FROM SCS.WorkCenterMaster WHERE ProcessId='" + ProcessId + "' AND PlantId='" + plantId + "' AND EntityId='" + EntityId + "' Order  by Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetIsProductionHourOpen(string plantId)
        {
            try
            {
                string sql = @"Select IsProductionHourOpen from SCS.PlantConfig Where PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetProductionBookingPeriodCbo()
        {
            string sql = @"Select Id AS [Value], (UserName+'( '+format(StartTime, 'hh:mm tt')+' - '+format(StartTime, 'hh:mm tt')+')') AS [Text] from HKP.ProductionBookingPeriod";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetShiftList(string processId)
        {
            string sql = @"SELECT distinct sd.SystemID [Value],sd.UserName [Text] FROM [dbo].[WorkCenterWiseShift] WCS
                                        LEFT JOIN dbo.ShiftDefination AS sd ON sd.SystemID = WCS.ShiftDefinationID
                                        WHERE WorkCenterMasterId IN(SELECT Id FROM SCS.WorkCenterMaster AS wcm WHERE wcm.ProcessId='" + processId + "')";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetIssueList(string processId)
        {
            string sql = @"SELECT distinct ID.Id [Value],ID.IssueName [Text] FROM [MST].[IssueDetails] ID
 WHERE ID.ProcessId='" + processId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetQualityIssueList(string processId)
        {
            string sql = @"select IssueNameId as Value,(select QMM.UserName from MST.QualityManagementMaster QMM where QMM.Id=IssueNameId) as Text from [MST].[QualityIssueDetails] where ProcessId='" + processId + @"'
union
select IssueId as Value,(select QMM.UserName from MST.QualityManagementMaster QMM where QMM.Id = IssueId) as Text from[MST].[POQualityPlanDetails] where ProcessId = '" + processId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetQualityWorkCenterList(string IssueId, string EntityId, string ProcessId)
        {
            string sql = @"select QMW.WorkCenterMasterId as Value, WCM.UserName as Text from MST.QualityManagementWorkCenter QMW
left join scs.WorkCenterMaster WCM on WCM.Id=QMW.WorkCenterMasterId
where QMW.QMID ='" + IssueId + "' and WCM.EntityId='" + EntityId + "' and WCM.ProcessId='" + ProcessId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPOCompleteIssueList()
        {
            string sql = @"select IssueNameId as Value,(select QMM.UserName from MST.QualityManagementMaster QMM where QMM.Id=IssueNameId) as Text from [MST].[QualityIssueDetails] 
union
select IssueId as Value,(select QMM.UserName from MST.QualityManagementMaster QMM where QMM.Id = IssueId) as Text from[MST].[POQualityPlanDetails]";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPOList(string IssueId)
        {
            string sql = @"SELECT distinct QC.ProductionOrderId [Value],QC.ProductionOrderId [Text] FROM TRN.QualityControl QC
 WHERE QC.IssueId='" + IssueId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPeriodList(string IssueId)
        {
            string sql = @"select distinct WTD.Id [Value],WTD.PeriodName +' (' + format(WTD.FromTime,'hh:mm tt')+' - ' + format(WTD.ToTime,'hh:mm tt')+')' as  [Text] from [MST].[WCProcessTimeDetails] WTD where WTD.IssueId='" + IssueId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetQualityPeriodList(string IssueId)
        {
            string sql = @"select distinct WTD.Id [Value],WTD.PeriodName +' (' + format(WTD.FromTime,'hh:mm tt')+' - ' + format(WTD.ToTime,'hh:mm tt')+')' as  [Text] from [MST].[QualityTimeDetails] WTD where WTD.IssueId='" + IssueId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetIssueType(string IssueId)
        {
            string sql = @"select QID.IssueType as POIssueType from [MST].[QualityIssueDetails] QID where QID.Id='" + IssueId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetQBookingLevel(string ProcessId, string EntityId, string POId)
        {
            string sql = @"select isnull(PPS.ProductionBookingLevel, (select ProductionBookingLevel from hkp.EntityProcessTag where EntityId = '"+ EntityId + "' and ProcessId = '"+ProcessId+ @"')) as BookingLevel
from TRN.ProductionOrderProcessSet PPS
where PPS.ProductionOrderID = '"+ POId + "' AND PPS.ProcessId = '" + ProcessId + @"'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetChkInterval(string IssueId)
        {
            string sql = @"select QID.CheckingInterval as CheckingInterval from [MST].[QualityIssueDetails] QID where QID.Id='" + IssueId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetAllShiftList()
        {
            string sql = @"SELECT distinct sd.SystemID [Value],sd.UserName [Text] FROM [dbo].[WorkCenterWiseShift] WCS
                                        LEFT JOIN dbo.ShiftDefination AS sd ON sd.SystemID = WCS.ShiftDefinationID
                                        WHERE WorkCenterMasterId IN(SELECT Id FROM SCS.WorkCenterMaster)";
            return _sqlRepository.GetDataCollection(sql);
        }
       

        #region Packing Content & Dispatch

        public IEnumerable<object> GetProductionOrderDataList()
        {
            string CmdText = @"SELECT Flag=Convert(bit,0),PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,ISNULL(PD.BuyerOrder,'')BuyerOrder,ISNULL(PD.OwnOrder,'')OwnOrder,ISNULL(PD.BuyerItem,'')BuyerItem,ISNULL(PD.OwnItem,'')OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PS.UserName = 'Running'";

            return _sqlRepository.GetDataCollection(CmdText);
        }

        public IEnumerable<object> GetProductionOrderDataList(string productionStatusId)
        {
            string CmdText = @"SELECT Flag=Convert(bit,0),PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,ISNULL(PD.BuyerOrder,'')BuyerOrder,ISNULL(PD.OwnOrder,'')OwnOrder,ISNULL(PD.BuyerItem,'')BuyerItem,ISNULL(PD.OwnItem,'')OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PS.Id = '" + productionStatusId + "'";

            return _sqlRepository.GetDataCollection(CmdText);
        }

        public IEnumerable<object> GetEntityProcessSettingData(string EntityId)
        {
            string sql = @"Select ProcessNature,EntityId,IsPackingSKURequired,PackingForm from HKP.EntityProcessTag Where ProcessNature= 'Packing' AND EntityId='" + EntityId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetDispatchEntityProcessSettingData(string EntityId)
        {
            string sql = @"Select ProcessNature,EntityId,IsDispatchSKURequired,DispatchForm,DispatchType from HKP.EntityProcessTag Where ProcessNature= 'Dispatch' AND EntityId='" + EntityId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPackingContentDataByPRId(string PRId)
        {
            string sql = @"SELECT A.*,
                                ProcessNature=(Select EPT.ProcessNature FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=A.ProductionOrderId AND EPT.ProcessNature='Packing') 
							  ,IsPackingSKURequired=(Select EPT.IsPackingSKURequired FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=A.ProductionOrderId AND EPT.ProcessNature='Packing') 
                              ,PackingForm=(Select EPT.PackingForm FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=A.ProductionOrderId AND EPT.ProcessNature='Packing')
                            ,B.NoOfQty,C.NoOfLine,TQ=B.NoOfQty*C.NoOfLine
                            FROM [dbo].[PackingContentMaster] A 
							LEFT JOIN (select SUM(Qty) NoOfQty,PackingContentMasterId FROM [dbo].[PackingContentDetail] GROUP BY PackingContentMasterId) B ON B.PackingContentMasterId=A.Id 
							LEFT JOIN (select COUNT(Id) NoOfLine,PackingContentMasterId FROM [dbo].[PackingChild] GROUP BY PackingContentMasterId) C ON C.PackingContentMasterId=A.Id 
							Where ProductionOrderId='" + PRId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPackingContentDataByPRIdWithTran(string PRId)
        {
            string sql = @"SELECT A.*,
                                ProcessNature=(Select EPT.ProcessNature FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=A.ProductionOrderId AND EPT.ProcessNature='Packing') 
							  ,IsPackingSKURequired=(Select EPT.IsPackingSKURequired FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=A.ProductionOrderId AND EPT.ProcessNature='Packing') 
                              ,PackingForm=(Select EPT.PackingForm FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=A.ProductionOrderId AND EPT.ProcessNature='Packing')
                            ,B.NoOfQty,C.NoOfLine,TQ=B.NoOfQty*C.NoOfLine, ISNULL(D.Confirmed,0) Confirmed, Balance=C.NoOfLine- ISNULL(D.Confirmed,0),0 RecvQty 
                            FROM [dbo].[PackingContentMaster] A 
							LEFT JOIN (select SUM(Qty) NoOfQty,PackingContentMasterId FROM [dbo].[PackingContentDetail] GROUP BY PackingContentMasterId) B ON B.PackingContentMasterId=A.Id 
							LEFT JOIN (select COUNT(Id) NoOfLine,PackingContentMasterId FROM [dbo].[PackingChild] GROUP BY PackingContentMasterId) C ON C.PackingContentMasterId=A.Id 
                            LEFT JOIN (SELECT Count(Id) Confirmed,PackingContentMasterId FROM [dbo].[PackingChild] WHERE  IsConfirmed=1 GROUP BY PackingContentMasterId) D ON D.PackingContentMasterId=A.Id 
							Where ProductionOrderId='" + PRId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetAllConfirmedPackingContentData()
        {
            string sql = @"SELECT A.*,
                                ProcessNature=(Select EPT.ProcessNature FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=PPO.ProductionOrderId AND EPT.ProcessNature='Packing') 
							  ,IsPackingSKURequired=(Select EPT.IsPackingSKURequired FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=PPO.ProductionOrderId AND EPT.ProcessNature='Packing') 
                              ,PackingForm=(Select EPT.PackingForm FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=PPO.ProductionOrderId AND EPT.ProcessNature='Packing')
                            ,B.NoOfQty,C.NoOfLine,TQ=B.NoOfQty*C.NoOfLine, ISNULL(D.Confirmed,0) Confirmed, Balance=C.NoOfLine- ISNULL(D.Confirmed,0),0 RecvQty 
                            FROM [dbo].[PackingContentMaster] A 
							LEFT JOIN (select SUM(Qty) NoOfQty,PackingContentMasterId FROM [dbo].[PackingContentDetail] GROUP BY PackingContentMasterId) B ON B.PackingContentMasterId=A.Id 
							LEFT JOIN (select COUNT(Id) NoOfLine,PackingContentMasterId FROM [dbo].[PackingChild] GROUP BY PackingContentMasterId) C ON C.PackingContentMasterId=A.Id 
                            LEFT JOIN (SELECT Count(Id) Confirmed,PackingContentMasterId FROM [dbo].[PackingChild] WHERE  IsConfirmed=1 GROUP BY PackingContentMasterId) D ON D.PackingContentMasterId=A.Id
							LEFT JOIN(Select top(1) * from  dbo.PackingProductionOrder) PPO ON PPO.PackingContentMasterId=A.Id";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetPackingProductionOrderData(string MasterId)
        {
            try
            {
                string sql = @"SELECT PPO.*,PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,ISNULL(PD.BuyerOrder,'')BuyerOrder,ISNULL(PD.OwnOrder,'')OwnOrder,ISNULL(PD.BuyerItem,'')BuyerItem,ISNULL(PD.OwnItem,'')OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
								   FROM   [dbo].PackingProductionOrder PPO 
								   LEFT JOIN  TRN.ProductionOrder PO ON PPO.ProductionOrderId=PO.Id
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory
								   
								   ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								  
								   ) PD ON PD.ProductionOrderId=PO.Id
								   WHERE PS.UserName = 'Running' AND PPO.PackingContentMasterId='" + MasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSOList(string customerId)
        {
            string sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,POD.ProductionOrderId
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo BuyerOrderNo,mo.OwnReferenceNo AS OwnOrderNo
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,CASE WHEN isnull(so.WeekNo,0)=0 THEN  DATEPART(week,so.DeliveryDate) ELSE so.WeekNo END AS DeliveryWeek
	                            , Active = CAST(0 AS BIT),SO.DestinationDescription
								,CN.ContractNo,MLC.LCRef MasterLCNo
                       FROM [TRN].[SalesOrder] AS SO 
                        left outer join [TRN].[ProductionOrderDetail] POD on POD.SalesOrderId=SO.Id 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                       LEFT JOIN dbo.[Contract] AS CN ON CN.Id=MOI.ContractId
                       LEFT JOIN dbo.MasterLC AS MLC ON MLC.Id=CN.MasterLCId
                       Where MO.PartyId='" + customerId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetDispatchDetailSOList(string masterId)
        {
            string sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN,DSO.*,POD.ProductionOrderId
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo BuyerOrderNo,mo.OwnReferenceNo AS OwnOrderNo
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,CASE WHEN isnull(so.WeekNo,0)=0 THEN  DATEPART(week,so.DeliveryDate) ELSE so.WeekNo END AS DeliveryWeek
	                            ,SO.DestinationDescription
								,CN.ContractNo,MLC.LCRef MasterLCNo
                       FROM [TRN].[SalesOrder] AS SO 
					   LEFT JOIN [dbo].[DispatchDetailSO] DSO ON DSO.SalesOrderId=SO.Id
                      LEFT JOIN [dbo].[DispatchDetail] DD ON DD.Id=DSO.DispatchDetailId
                        left outer join [TRN].[ProductionOrderDetail] POD on POD.SalesOrderId=SO.Id 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                       LEFT JOIN dbo.[Contract] AS CN ON CN.Id=MOI.ContractId
                       LEFT JOIN dbo.MasterLC AS MLC ON MLC.Id=CN.MasterLCId
                    Where DD.DispatchMasterId='" + masterId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetDispatchMasterList(string PlantId)
        {
            string sql = @"Select DM.*,P.UserName PartyName,INP.UserName InvoicingPartyPlant,DLP.UserName DeliveryPartyPlant 
                                from [dbo].[DispatchMaster] DM
                                LEFT JOIN HKP.Party P ON P.Id=DM.PartyId
                                LEFT JOIN HKP.PartyPlant INP ON INP.Id=DM.InvoicingPartyPlantId
                                LEFT JOIN HKP.PartyPlant DLP ON DLP.Id=DM.DeliveryPartyPlantId
                                Where DM.PlantId='" + PlantId + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        #endregion PackingContent

        #region QualityProcessBooking
        public IEnumerable<object> GetQualityList()
        {
            try
            {
                string sql = @"Select a.*,P.UserName Process,qp.UserName QualityProcess,csg.[Description],FORMAT(A.ProductionDate,'dd-MMM-yyyy')PD 
FROM dbo.QuaityProcessBooking A
LEFT JOIN hkp.Process AS p ON p.Id = A.ProcessId
LEFT JOIN hkp.QualityProcess AS qp ON qp.Id = A.QualityProcessId 
LEFT JOIN MST.CompliedShiftGrouping AS csg ON csg.Id = A.ProductionShiftId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetQualityProcessCbo(string ProcessId)
        {
            try
            {
                string sql = @"SELECT DISTINCT P.Id, P.UserName FROM dbo.ProductionQualityProcess AS qp	
LEFT JOIN [HKP].[QualityProcess] AS P ON P.Id=qp.ProcessId
LEFT JOIN dbo.ProductionBookingProcessParameter PP ON PP.Id=qp.ProductionBookingProcessParameterId  
WHERE qp.[Active]=1 AND pp.ProcessId='" + ProcessId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetQualityProcessParameterList(string processId, string masterId)
        {
            try
            {
                string sql = @"SELECT CONVERT(bit,0) Active,A.Id,P.UserName,P.Formula,P.FormulaId,P.EntryState,ValueIN = CASE WHEN P.ValueinDecimal=1 THEN 'Decimal' ELSE 'Percentage' END
,Value=CASE WHEN A.Value IS NOT NULL THEN A.Value ELSE (CASE WHEN P.ValueinDecimal=1 THEN P.DefaultValue ELSE P.DefaultValue/100 END) END
,P.Id QualityProcessParameterId,P.GradeLot,P.ParameterGrade
FROM dbo.QualityProcessParameter P
LEFT JOIN [dbo].[QuaityProcessBookingParameterValue] A ON A.QualityProcessParameterId=P.Id AND ISNULL(A.QuaityProcessBookingId,'" + masterId + @"')='" + masterId + @"'
WHERE p.QualityProcessId IN(select Id from dbo.ProductionQualityProcess where ProcessId='" + processId + "')";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetProductionBookingData(string processId, string productionDate, string ProductionShiftId)
        {
            string sql = @"
SELECT CONVERT(bit,0) Flag,PS.Id ProductionSummaryId,PR.Id PrOId,PS.Quantity,WM.Code WorkCenterMaster,PBP.UserName BookingPeriod
,PS.ProductionGrade,PS.LotNumber,MM.UserName MaterialMaster,MMA.StandardName Article,''ProductCode
,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                            ,ProductCode=REPLACE(REPLACE(
										STUFF((select distinct ','+pl.UserName
																from dbo.ProductLibrary AS pl
																INNER JOIN trn.MasterOrderItem XMOI ON XMOI.ProductLibraryId = pl.Id	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=ps.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
FROM TRN.ProductionSummary AS ps
LEFT JOIN TRN.ProductionOrder PR ON PR.Id = ps.ProductionOrderId 
LEFT JOIN SCS.WorkCenterMaster WM ON WM.Id=PS.WorkCenterMasterId
LEFT JOIN hkp.ProductionBookingPeriod PBP ON PBP.Id=ps.ProductionBookingPeriodId
LEFT JOIN MST.MaterialMaster MM ON MM.Id=PS.MaterialMasterId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.MaterialMasterId=MM.Id
WHERE PS.ProcessId='" + processId + @"' AND PS.ProductionDate='" + productionDate + "' AND PS.ProductionShiftId='" + ProductionShiftId + "' AND PS.Id NOT IN(SELECT ProductionSummaryId FROM [dbo].[QuaityBookingProductionSummary])";
            return _sqlRepository.GetDataCollection(sql);
        }

        public void SaveData(Dictionary<string, object> data, List<Dictionary<string, object>> ProdBookedSaveList, IEnumerable<QuaityProcessBookingParameterValue> ParameterList)
        {

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsProdBooked, dsParameter;
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.QuaityProcessBooking WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "QuaityProcessBooking", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    data["AddedBy"] = dsMaster.Tables[0].Rows[0]["AddedBy"].ToString();
                    data["AddedDate"] = dsMaster.Tables[0].Rows[0]["AddedDate"].ToString();
                    data["AddedFromIP"] = dsMaster.Tables[0].Rows[0]["AddedFromIP"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[QuaityBookingProductionSummary] where  QuaityProcessBookingId='" + data["Id"] + "'", out dsProdBooked, false, "1");
                if (ProdBookedSaveList != null)
                {
                    int pbc = 0;
                    foreach (var item in ProdBookedSaveList)
                    {
                        DataView dv = new DataView(dsProdBooked.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = Id + "-" + pbc++;
                            item["QuaityProcessBookingId"] = Id;

                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                    }
                }

                int pac = 0;
                objCon.OpenDataSetThroughAdapter("SELECT * FROM [dbo].[QuaityProcessBookingParameterValue] where  QuaityProcessBookingId='" + data["Id"] + "'", out dsParameter, false, "1");

                if (ParameterList != null)
                {
                    DataTable dtValue = new DataTable();
                    dtValue.TableName = "TempTable";
                    dtValue.Columns.Add("QualityProcessParameterId");
                    dtValue.Columns.Add("Amount");
                    string sFormulaResult = null;

                    DataSet dsOpenHead = Library.Service.Helpers.DataTableExtensions.ToDataSet<QuaityProcessBookingParameterValue>(ParameterList);
                    for (int i = 0; i < dsOpenHead.Tables[0].Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            DataRow dtValueRow = dtValue.NewRow();

                            dtValueRow["QualityProcessParameterId"] = dsOpenHead.Tables[0].Rows[i]["QualityProcessParameterId"].ToString().Trim();
                            dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                            dtValue.Rows.Add(dtValueRow);
                        }
                        else if (i > 0 && string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                        {
                            DataRow dtValueRow = dtValue.NewRow();

                            dtValueRow["QualityProcessParameterId"] = dsOpenHead.Tables[0].Rows[i]["QualityProcessParameterId"].ToString().Trim();
                            dtValueRow["Amount"] = dsOpenHead.Tables[0].Rows[i]["Value"].ToString().Trim();

                            dtValue.Rows.Add(dtValueRow);
                        }

                        if (!string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString()))
                        {
                            ReLoadFormulaWithValue(dsOpenHead.Tables[0].Rows[i]["FormulaId"].ToString(), ref dtValue, out string _formulaValue);
                            sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString("#,##0");

                            DataRow dtValueRow = dtValue.NewRow();

                            dtValueRow["QualityProcessParameterId"] = dsOpenHead.Tables[0].Rows[i]["QualityProcessParameterId"].ToString().Trim();
                            dtValueRow["Amount"] = sFormulaResult;

                            dtValue.Rows.Add(dtValueRow);

                            DataView dv = new DataView(dsOpenHead.Tables[0]);
                            dv.RowFilter = "QualityProcessParameterId='" + dsOpenHead.Tables[0].Rows[i]["QualityProcessParameterId"].ToString() + "'";
                            if (dv.Count > 0)
                            {
                                DataRow drmo = dv[0].Row;

                                drmo.BeginEdit();
                                drmo["Value"] = sFormulaResult;
                                drmo.EndEdit();

                            }
                        }
                    }

                    List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(dsOpenHead.Tables[0]);

                    foreach (var item in NewData)
                    {
                        DataView dv = new DataView(dsParameter.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = Id + "-" + pac++;
                            item["QuaityProcessBookingId"] = Id;

                            AddNewRow(dsParameter.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsProdBooked, dsParameter);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }




            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }


            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        public IEnumerable<object> GetLotNumberCbo(string SalesOrderId, string ProductionOrderId, string ProcessId, string productionLevel)
        {
            try
            {
                var sql = "";
                if (productionLevel != "ProductionOrder")
                {
                    sql = @"SELECT DISTINCT LotNumber [Value],LotNumber [Text] FROM TRN.ProductionSummary Where ISNULL(LotNumber,'')<>'' AND SalesOrderId='" + SalesOrderId + "' AND ProcessId='" + ProcessId + "'";
                }
                else
                {
                    sql = @"SELECT DISTINCT LotNumber [Value],LotNumber [Text] FROM TRN.ProductionSummary Where ISNULL(LotNumber,'')<>'' AND ProductionOrderId='" + ProductionOrderId + "' AND ProcessId='" + ProcessId + "'";
                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public IEnumerable<object> GetBookingLevel(string FromId, string ToId)
        {
            string sql = @"SELECT ProductionBookingLevel FROM MST.SFGMovementEntity WHERE SFGMovementId = 
                            (SELECT Id FROM MST.SFGMovement WHERE ISNULL(FromProcessId,FromSFGInventoryId) = '" + FromId + @"' AND 
                            ISNULL(ToProcessId,ToSFGInventoryId)='" + ToId + "' AND ISNULL(ProductionBookingLevel,'')<>'')";
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetQualityPlan(string POIssueDate, string ResponsiblePersonId)
        {
            string ResponsiblePerson = string.Empty;

            if (ResponsiblePersonId != "null" && ResponsiblePersonId != "undefined")
            {
                ResponsiblePerson = " and QPEmployeeId = '" + ResponsiblePersonId + "'";
            }

            string sql = @"Select (select day(Min(AddedDate)) from TRN.ProductionSummary where ProductionOrderId=PO1.POId and LotNumber = PO1.LotNumber and ProcessId = PO1.ProcessId) - day(getdate()) Days,Format(PO1.Date,'dd-MMM-yyyy') PODate,Format(PO1.QualityPlanDate,'dd-MMM-yyyy') QPDate,PO1.* from (Select distinct QPC.Id,PD.Id QPId,PO.Id POId,PO.EntryLevel,PO.LotNumber,PD.IssueId,QMM.UserName QPIssue,PO.ProcessId,P.UserName Process,PD.Legdays,PD.CriticalLevel,
PD.DependentDate DependentOn,E.UserName Entity,PO.EntityId,
(select top 1 RepeatEntry from TRN.QualityControl where IssueId=QMM.Id and QualityPlanId=QPC.Id and PlanType='POIssue' and RepeatEntry is not null order by AddedDate desc) as RepeatEntry,
PD.Remarks,PO.POStatus,PO.Customer,
convert(Date,case 
when PD.DependentDate='ItemDate' then format(MOI.AddedDate,'dd-MMM-yyyy')
when PD.DependentDate='ExFactoryDate' then format((select top 1 PlanExFactoryDate from TRN.SalesOrder where Id=SO.Id order by PlanExFactoryDate desc),'dd-MMM-yyyy')
when PD.DependentDate='PODate' then PO.POCreationDate
when PD.DependentDate='POStartDate' then PO.POStartDate
when PD.DependentDate='POEndDate' then PO.POEndDate
end)Date, 
convert(Date,case 
when PD.DependentDate='ItemDate' then format(DATEADD(Day, PD.Legdays, MOI.AddedDate),'dd-MMM-yyyy')
when PD.DependentDate='ExFactoryDate' then format(DATEADD(Day, PD.Legdays, (select top 1 PlanExFactoryDate from TRN.SalesOrder where Id=SO.Id order by PlanExFactoryDate desc)),'dd-MMM-yyyy')
when PD.DependentDate='PODate' then format(DATEADD(Day, PD.Legdays, PO.POCreationDate),'dd-MMM-yyyy')
when PD.DependentDate='POStartDate' then format(DATEADD(Day, PD.Legdays, PO.POStartDate),'dd-MMM-yyyy')
when PD.DependentDate='POEndDate' then format(DATEADD(Day, PD.Legdays,PO.POEndDate),'dd-MMM-yyyy')
end) QualityPlanDate,
PO.POStartDate,PO.POEndDate,PO.POCreationDate,
isnull(QPC.QPEmployeeId,PD.ResponsiblePersonId) as QPEmployeeId,
isnull((select EmployeeName from EmployeeInformation where SystemId=QPC.QPEmployeeId),(select EmployeeName from EmployeeInformation where SystemId=PD.ResponsiblePersonId)) as QPEmployee
from (select distinct PO.Id,PS.UserName POStatus, 'PO' EntryLevel,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
reverse(stuff(reverse((select distinct LotNumber + ',' from TRN.ProductionSummary where ProductionOrderId=PO.Id and ProcessId=Prod.ProcessId for xml path(''))),1,1,'')) as LotNumber,
PO.EntityId,Prod.ProcessId,POFirstProdBookDate,POProcessFirstProdBookDate,POLatestProdBookDate,BaseProcPlanStartDate,BaseProcPlanCompletionDate, 
isnull(format(FBPPD.POFirstProdBookDate,'dd-MMM-yyyy'),format(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')) POStartDate,
isnull(format(Type1.BaseProcPlanCompletionDate,'dd-MMM-yyyy'),format(LBPPD.POLatestProdBookDate,'dd-MMM-yyyy')) POEndDate,
format(PO.AddedDate,'dd-MMM-yyyy') POCreationDate
from TRN.ProductionOrder PO
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
left join TRN.ProductionSummary Prod on Prod.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POFirstProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POProcessFirstProdBookDate,ProductionOrderId,ProcessId From TRN.ProductionSummary Group By ProductionOrderId,ProcessId) POPFBD ON POPFBD.ProductionOrderId=PO.Id and Prod.ProcessId=POPFBD.ProcessId
LEFT JOIN (Select SUM(Quantity)ProQty,MAX(ProductionDate)POLatestProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) LBPPD ON LBPPD.ProductionOrderId=PO.Id
LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanCompletionDate,ProductionOrderId From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=PO.Id
where PS.UserName in ('Running','To Close') 
union
select distinct PO.Id,PS.UserName POStatus, 'LOT' EntryLevel,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),Prod.LotNumber,PO.EntityId,
Prod.ProcessId,POFirstProdBookDate,POProcessFirstProdBookDate,POLatestProdBookDate,BaseProcPlanStartDate,BaseProcPlanCompletionDate, 
isnull(format(FBPPD.POFirstProdBookDate,'dd-MMM-yyyy'),format(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')) POStartDate,
isnull(format(Type1.BaseProcPlanCompletionDate,'dd-MMM-yyyy'),format(LBPPD.POLatestProdBookDate,'dd-MMM-yyyy')) POEndDate,
format(PO.AddedDate,'dd-MMM-yyyy') POCreationDate
from TRN.ProductionOrder PO
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
left join TRN.ProductionSummary Prod on Prod.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POFirstProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POProcessFirstProdBookDate,ProductionOrderId,ProcessId From TRN.ProductionSummary Group By ProductionOrderId,ProcessId) POPFBD ON POPFBD.ProductionOrderId=PO.Id and Prod.ProcessId=POPFBD.ProcessId
LEFT JOIN (Select SUM(Quantity)ProQty,MAX(ProductionDate)POLatestProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) LBPPD ON LBPPD.ProductionOrderId=PO.Id
LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanCompletionDate,ProductionOrderId From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=PO.Id
where PS.UserName in ('Running','To Close')) PO
left join MST.POQualityPlanDetails PD on PD.EntryLevel=PO.EntryLevel and PO.ProcessId=PD.ProcessId and PD.IsActive=1
left Join MST.QualityManagementMaster QMM on QMM.Id=PD.IssueId
left join [TRN].[QualityPlanControl] QPC on QPC.QPId=PD.Id and QPC.POId=PO.Id and QPC.LotNumber=PO.LotNumber and QPC.EntryLevel=PO.EntryLevel
left join TRN.ProductionOrderDetail POD on POD.ProductionOrderId=PO.Id
left join TRN.SalesOrder SO on SO.Id=POD.SalesOrderId 
left join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
left join hkp.Process P on P.Id=PO.ProcessId
left join ORG.Entity E on E.Id=PO.EntityId
where PO.ProcessId is not null and E.Id in (select EntityId from MST.QualityManagementEntity where QMID=QMM.Id) 
and QPC.QCID is null 
--or (select top 1 RepeatEntry from TRN.QualityControl where IssueId=QMM.Id and QualityPlanId=QPC.Id and PlanType='POIssue' order by AddedDate desc) is not null
union
Select distinct QPC.Id,PD.Id QPId,PO.Id POId,PO.EntryLevel,PO.LotNumber,PD.IssueId,QMM.UserName QPIssue,PO.ProcessId,P.UserName Process,PD.Legdays,PD.CriticalLevel,
PD.DependentDate DependentOn,E.UserName Entity,PO.EntityId,
(select top 1 RepeatEntry from TRN.QualityControl where IssueId=QMM.Id and QualityPlanId=QPC.Id and PlanType='POIssue' and RepeatEntry is not null order by AddedDate desc) as RepeatEntry,
PD.Remarks,PO.POStatus,PO.Customer,
convert(Date,case 
when PD.DependentDate='ItemDate' then format(MOI.AddedDate,'dd-MMM-yyyy')
when PD.DependentDate='ExFactoryDate' then format((select top 1 PlanExFactoryDate from TRN.SalesOrder where Id=SO.Id order by PlanExFactoryDate desc),'dd-MMM-yyyy')
when PD.DependentDate='PODate' then PO.POCreationDate
when PD.DependentDate='POStartDate' then PO.POStartDate
when PD.DependentDate='POEndDate' then PO.POEndDate
end)Date, 
convert(Date,case 
when PD.DependentDate='ItemDate' then format(DATEADD(Day, PD.Legdays, MOI.AddedDate),'dd-MMM-yyyy')
when PD.DependentDate='ExFactoryDate' then format(DATEADD(Day, PD.Legdays, (select top 1 PlanExFactoryDate from TRN.SalesOrder where Id=SO.Id order by PlanExFactoryDate desc)),'dd-MMM-yyyy')
when PD.DependentDate='PODate' then format(DATEADD(Day, PD.Legdays, PO.POCreationDate),'dd-MMM-yyyy')
when PD.DependentDate='POStartDate' then format(DATEADD(Day, PD.Legdays, PO.POStartDate),'dd-MMM-yyyy')
when PD.DependentDate='POEndDate' then format(DATEADD(Day, PD.Legdays,PO.POEndDate),'dd-MMM-yyyy')
end) QualityPlanDate,
PO.POStartDate,PO.POEndDate,PO.POCreationDate,
isnull(QPC.QPEmployeeId,PD.ResponsiblePersonId) as QPEmployeeId,
isnull((select EmployeeName from EmployeeInformation where SystemId=QPC.QPEmployeeId),(select EmployeeName from EmployeeInformation where SystemId=PD.ResponsiblePersonId)) as QPEmployee
from (select distinct PO.Id,PS.UserName POStatus, 'PO' EntryLevel,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
reverse(stuff(reverse((select distinct LotNumber + ',' from TRN.ProductionSummary where ProductionOrderId=PO.Id and ProcessId=Prod.ProcessId for xml path(''))),1,1,'')) as LotNumber,
PO.EntityId,Prod.ProcessId,POFirstProdBookDate,POProcessFirstProdBookDate,POLatestProdBookDate,BaseProcPlanStartDate,BaseProcPlanCompletionDate, 
isnull(format(FBPPD.POFirstProdBookDate,'dd-MMM-yyyy'),format(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')) POStartDate,
isnull(format(Type1.BaseProcPlanCompletionDate,'dd-MMM-yyyy'),format(LBPPD.POLatestProdBookDate,'dd-MMM-yyyy')) POEndDate,
format(PO.AddedDate,'dd-MMM-yyyy') POCreationDate
from TRN.ProductionOrder PO
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
left join TRN.ProductionSummary Prod on Prod.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POFirstProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POProcessFirstProdBookDate,ProductionOrderId,ProcessId From TRN.ProductionSummary Group By ProductionOrderId,ProcessId) POPFBD ON POPFBD.ProductionOrderId=PO.Id and Prod.ProcessId=POPFBD.ProcessId
LEFT JOIN (Select SUM(Quantity)ProQty,MAX(ProductionDate)POLatestProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) LBPPD ON LBPPD.ProductionOrderId=PO.Id
LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanCompletionDate,ProductionOrderId From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=PO.Id
where PS.UserName in ('Running','To Close') 
union
select distinct PO.Id,PS.UserName POStatus, 'LOT' EntryLevel,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),Prod.LotNumber,PO.EntityId,
Prod.ProcessId,POFirstProdBookDate,POProcessFirstProdBookDate,POLatestProdBookDate,BaseProcPlanStartDate,BaseProcPlanCompletionDate, 
isnull(format(FBPPD.POFirstProdBookDate,'dd-MMM-yyyy'),format(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')) POStartDate,
isnull(format(Type1.BaseProcPlanCompletionDate,'dd-MMM-yyyy'),format(LBPPD.POLatestProdBookDate,'dd-MMM-yyyy')) POEndDate,
format(PO.AddedDate,'dd-MMM-yyyy') POCreationDate
from TRN.ProductionOrder PO
left join hkp.ProductionStatus PS on PS.Id=PO.ProductionStatusId
left join TRN.ProductionSummary Prod on Prod.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POFirstProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=PO.Id
LEFT JOIN (Select SUM(Quantity)ProQty,MIN(ProductionDate)POProcessFirstProdBookDate,ProductionOrderId,ProcessId From TRN.ProductionSummary Group By ProductionOrderId,ProcessId) POPFBD ON POPFBD.ProductionOrderId=PO.Id and Prod.ProcessId=POPFBD.ProcessId
LEFT JOIN (Select SUM(Quantity)ProQty,MAX(ProductionDate)POLatestProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) LBPPD ON LBPPD.ProductionOrderId=PO.Id
LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanCompletionDate,ProductionOrderId From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=PO.Id
where PS.UserName in ('Running','To Close')) PO
left join MST.POQualityPlanDetails PD on PD.EntryLevel=PO.EntryLevel and PD.IsActive=1 and PO.ProcessId=PD.ProcessId
left Join MST.QualityManagementMaster QMM on QMM.Id=PD.IssueId
left join [TRN].[QualityPlanControl] QPC on QPC.QPId=PD.Id and QPC.POId=PO.Id and QPC.LotNumber=PO.LotNumber and QPC.EntryLevel=PO.EntryLevel
left join TRN.ProductionOrderDetail POD on POD.ProductionOrderId=PO.Id
left join TRN.SalesOrder SO on SO.Id=POD.SalesOrderId 
left join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
left join hkp.Process P on P.Id=PO.ProcessId
left join ORG.Entity E on E.Id=PO.EntityId
where PO.ProcessId is null and E.Id in (select EntityId from MST.QualityManagementEntity where QMID=QMM.Id) 
and QPC.QCID is null 
--or (select top 1 RepeatEntry from TRN.QualityControl where IssueId=QMM.Id and QualityPlanId=QPC.Id and PlanType='POIssue' order by AddedDate desc) is not null
) PO1
where PO1.QualityPlanDate < = '" + POIssueDate + "'" + ResponsiblePerson + @" or PO1.QualityPlanDate is null order by 
--PO1.QualityPlanDate ,
(select day(Min(AddedDate)) from TRN.ProductionSummary where ProductionOrderId=PO1.POId and LotNumber = PO1.LotNumber
and ProcessId = PO1.ProcessId) - day(getdate())";
             return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetGeneralIssue(string ResponsiblePersonId)
        {
            string ResponsiblePerson = string.Empty;

            if (ResponsiblePersonId != "null" && ResponsiblePersonId != "undefined")
            {
                ResponsiblePerson = " where QGIEmployeeId = '" + ResponsiblePersonId + "'";
            }
            string sql = @"select  GI.* from (select  QC.Id,QID.IssueNameId,(select top 1 RepeatEntry from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc) as RepeatEntry,
case when (select top 1 RepeatEntry from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc)='Repeat' then format((select top 1 AddedDate from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc),'dd-MMM-yyyy') else
format(DATEADD(hour, QID.CheckingInterval,(select top 1 AddedDate from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc)),'dd-MMM-yyyy') end as QualityIssueDate,
case when (select top 1 RepeatEntry from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc)='Repeat' then format((select top 1 AddedDate from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc),'hh:mm tt') else
format(DATEADD(hour, QID.CheckingInterval, CAST((select top 1 AddedDate from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId and PlanType='GeneralIssue' order by AddedDate desc) AS DATETIME)),'hh:mm tt') end as QualityIssueTime,
E.Id  EntityId,E.UserName Entity,P.Id ProcessId,P.UserName Process,QID.IssueNameId IssueId,QID.Id DefineIssueId,
QMM.UserName QGIssue,
reverse(stuff(reverse((select EI.EmployeeName + ',' from EmployeeInformation EI where EmployeeStatus='Active' and PositionID in (select PositionCodeId from MST.QualityManagementPositionCode where QMID=QID.IssueNameId) for xml path(''))),1,1,'')) as PositionEmployee,
isnull(QC.QGIEmployeeId,QID.ResponsiblePersonId) as QGIEmployeeId,isnull((select EmployeeName from EmployeeInformation where SystemId=QC.QGIEmployeeId),(select EmployeeName from EmployeeInformation where SystemId=QID.ResponsiblePersonId)) as QGIEmployee
from MST.QualityIssueDetails  QID
left join TRN.QualityIssueControl as QC on QC.DefineIssueId=QID.Id and QC.Id = (select top 1 Id from TRN.QualityIssueControl where DefineIssueId=QID.Id order by AddedDate desc) 
left join MST.QualityManagementMaster QMM on QMM.Id=QID.IssueNameId
left join org.Entity E on E.Id=QID.EntityId
left join hkp.Process P on P.Id=QID.ProcessId where QID.IsMandatory=1) GI
" + ResponsiblePerson + @"
order by Convert(Date,GI.QualityIssueDate)
--order by (select top 1 AddedDate + QID.CheckingInterval from TRN.QualityControl where IssueId=QID.IssueNameId and EntityId=QID.EntityId order by AddedDate asc) asc";
                 return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetSFGMovementFromCbo(string entity)
        {
            string sql;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (identity.IsSysAdmin || identity.IsControlAdmin)
            {
                sql = @"SELECT A.* FROM (
                SELECT DISTINCT 'PROCESS' AS Status,  SFGM.FromProcessId AS FromId,  P.UserName, E.ProductionBookingLevel,PIS.Sequence,E.LotNumberCapture,E.LotNumberMandatory,P.IsFirst,P.IsCrossAllowed,E.IsSKU1,E.IsSKU2,E.IsSKU3          
                FROM MST.SFGMovement AS SFGM
                INNER JOIN  HKP.EntityProcessTag E on E.ProcessId=SFGM.FromProcessId AND E.EntityId='" + entity + @"'
                LEFT JOIN [HKP].Process P ON SFGM.FromProcessId = P.Id 
                LEFT JOIN [dbo].[ProcessAndInventorySequence] PIS ON PIS.ProcessId=P.Id
                WHERE ISNULL(SFGM.FromProcessId,'')<>'' 
                UNION ALL
                SELECT DISTINCT 'INVENTORY' AS Status, SFGM.FromSFGInventoryId AS FromId, SFGI.UserName, E.ProductionBookingLevel,PIS.Sequence,E.LotNumberCapture,E.LotNumberMandatory,SFGI.IsFirst,SFGI.IsCrossAllowed,E.IsSKU1,E.IsSKU2,E.IsSKU3        
                FROM MST.SFGMovement AS SFGM 
                INNER JOIN  MST.EntitySFGInventory E ON E.SFGInventoryId=SFGM.FromSFGInventoryId AND E.EntityId='" + entity + @"'
                LEFT JOIN [HKP].[SFGInventory] SFGI ON SFGM.FromSFGInventoryId = SFGI.Id 
                LEFT JOIN [dbo].[ProcessAndInventorySequence] PIS ON PIS.SFGInventoryId =SFGI.Id
                WHERE ISNULL(SFGM.FromSFGInventoryId,'')<>''
                ) A  Order by A.Sequence";
            }
            else
            {
                sql = @"SELECT A.* FROM (
                         SELECT DISTINCT 'PROCESS' AS Status, SFGM.FromProcessId AS FromId, P.UserName, E.ProductionBookingLevel,PIS.Sequence,E.LotNumberCapture,E.LotNumberMandatory,P.IsFirst,P.IsCrossAllowed,E.IsSKU1,E.IsSKU2,E.IsSKU3            
                        FROM MST.SFGMovement AS SFGM
                        INNER JOIN  HKP.EntityProcessTag E on E.ProcessId=SFGM.FromProcessId AND E.EntityId='" + entity + @"'
                        LEFT JOIN [HKP].Process P ON SFGM.FromProcessId = P.Id 
                        LEFT JOIN [dbo].[ProcessAndInventorySequence] PIS ON PIS.ProcessId=P.Id
                        LEFT JOIN SEC.UserProcess U on U.ProcessId= P.Id  AND U.UserId='" + identity.UserId + @"'
						WHERE ISNULL(SFGM.FromProcessId,'')<>'' 
                        UNION ALL
                        SELECT DISTINCT 'INVENTORY' AS Status, SFGM.FromSFGInventoryId AS FromId, SFGI.UserName, E.ProductionBookingLevel,PIS.Sequence,E.LotNumberCapture,E.LotNumberMandatory,SFGI.IsFirst,SFGI.IsCrossAllowed,E.IsSKU1,E.IsSKU2,E.IsSKU3        
                        FROM MST.SFGMovement AS SFGM 
                        INNER JOIN  MST.EntitySFGInventory E ON E.SFGInventoryId=SFGM.FromSFGInventoryId AND E.EntityId='" + entity + @"'
                        LEFT JOIN [HKP].[SFGInventory] SFGI ON SFGM.FromSFGInventoryId = SFGI.Id 
                        LEFT JOIN [dbo].[ProcessAndInventorySequence] PIS ON PIS.SFGInventoryId =SFGI.Id
				        LEFT JOIN SEC.UserSFGInventory U on U.SFGInventoryId=SFGM.FromSFGInventoryId AND U.UserId='" + identity.UserId + @"'
                        WHERE ISNULL(SFGM.FromSFGInventoryId,'')<>''
                        ) A Order by A.Sequence";
            }
            return _sqlRepository.GetDataCollection(sql);
        }

        public IEnumerable<object> GetSFGMovementToCbo(string FromId, string flag, string EntityId)
        {
            string processId = string.Empty;
            string inventoryId = string.Empty;

            if (flag == "PROCESS")
            {
                processId = FromId;
            }
            else
            {
                inventoryId = FromId;
            }

            string sql;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (identity.IsSysAdmin || identity.IsControlAdmin)
            {
                sql = @"SELECT A.* FROM (
                        SELECT DISTINCT  'PROCESS' AS Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToProcessId AS ToId,  P.UserName
                        FROM MST.SFGMovement AS SFGM  
                        INNER JOIN  HKP.EntityProcessTag E on E.ProcessId=SFGM.ToProcessId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].Process P ON SFGM.ToProcessId = P.Id WHERE ISNULL(SFGM.ToProcessId,'')<>''
                        UNION ALL
                        SELECT DISTINCT 'INVENTORY'as Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToSFGInventoryId AS ToId, SFGI.UserName
                        FROM MST.SFGMovement AS SFGM 
                        INNER JOIN  MST.EntitySFGInventory E ON E.SFGInventoryId=SFGM.ToSFGInventoryId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].[SFGInventory] SFGI ON SFGM.ToSFGInventoryId = SFGI.Id WHERE ISNULL(SFGM.ToSFGInventoryId,'')<>''
                        ) A WHERE A.FromProcessId = '" + processId + @"' OR A.FromSFGInventoryId = '" + inventoryId + @"' ";
            }
            else
            {
                sql = @"SELECT A.* FROM (
                        SELECT DISTINCT  'PROCESS' AS Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToProcessId AS ToId,  P.UserName 
                        FROM MST.SFGMovement AS SFGM  
                        INNER JOIN  HKP.EntityProcessTag E on E.ProcessId=SFGM.ToProcessId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].Process p ON SFGM.ToProcessId = P.Id 
                        LEFT JOIN SEC.UserProcess U on U.ProcessId= p.Id AND U.UserId='" + identity.UserId + @"'
                        WHERE ISNULL(SFGM.ToProcessId,'')<>''
                        UNION ALL
                        SELECT DISTINCT 'INVENTORY' AS Status, SFGM.FromProcessId, SFGM.FromSFGInventoryId, SFGM.ToSFGInventoryId AS ToId, SFGI.UserName
                        FROM MST.SFGMovement AS SFGM 
                        INNER JOIN  MST.EntitySFGInventory E ON E.SFGInventoryId=SFGM.ToSFGInventoryId AND E.EntityId='" + EntityId + @"'
                        LEFT JOIN [HKP].[SFGInventory] SFGI ON SFGM.ToSFGInventoryId = SFGI.Id 
                        LEFT JOIN SEC.UserSFGInventory U on U.SFGInventoryId= SFGI.Id  AND U.UserId='" + identity.UserId + @"'
                        WHERE ISNULL(SFGM.ToSFGInventoryId,'')<>''
                        ) A WHERE A.FromProcessId = '" + processId + @"' OR A.FromSFGInventoryId = '" + inventoryId + @"' ";
            }
            return _sqlRepository.GetDataCollection(sql);

        }
        #endregion

        public IEnumerable<object> GetSOData()
        {
            try
            {
                string CmdText = @"SELECT DISTINCT mo.MasterOrderNo
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
	                                ,ISNULL(mma.StandardName, '') Article
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
	                                ,CEILING(SO.PlannedQty) PlannedQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem
                                FROM TRN.ProductionOrderDetail POD
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
							   Where ISNULL(mo.MasterOrderNo,'')<>'' AND so.Id NOT IN(Select SOId From  dbo.BOMSODetail)";

                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetCostingItemData()
        {
            try
            {
                string sql = @"SELECT ci.ShortName,cat.UserName AS CostingCategory, CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END) AS Selected, ci.CostingComponentId,ci.Id as CostingItemId,  ci.UserName,ci.Code,ci.Sequence, ci.StandardName, 
o.CostingMasterTemplateId,
ci.MinimumOfQuantity, ci.POIssueDeadLine,ci.UnitOfMeasurementId,cc.UserName as CostingComponent,cc.Id as CostingComponentId, 
ci.POIssueDeadLine, ci.Wastage,ci.Description
from hkp.CostingItem ci 
left join hkp.CostingComponent cc on cc.Id = ci.CostingComponentId
LEFT OUTER JOIN hkp.CostingCategory AS cat ON cat.Id=ci.CostingCategoryId
LEFT join PreCostingValueLoss o on o.CostingItemId = ci.Id
ORDER BY CONVERT(BIT, CASE WHEN isnull(o.Id,'')<>'' THEN 1 ELSE 0 END), ci.Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> GetFirstSKUCbo()
        {
            try
            {
                string sql = @"select CV.Id,CV.Username from HKP.CharacteristicsValue CV
left join HKP.Characteristics C ON C.Id=CV.CharacteristicsId
Where C.Sequence=1";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public IEnumerable<object> GetSecondSKUCbo()
        {
            try
            {
                string sql = @"select CV.Id,CV.Username from HKP.CharacteristicsValue CV
left join HKP.Characteristics C ON C.Id=CV.CharacteristicsId
Where C.Sequence=2";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public IEnumerable<object> Productionfilters(string productionStatusId, string poId)
        {
            try
            {
                string pssql = "SELECT UserName FROM HKP.ProductionStatus WHERE Id='" + productionStatusId + "'";
                DataTable dsPS = _sqlRepository.GetDataTable(pssql);
                if (dsPS.Rows.Count > 0)
                {
                    if (dsPS.Rows[0]["UserName"].ToString() == "Closed")
                    {
                        if (string.IsNullOrEmpty(poId) || poId == "undefined")
                        {
                            throw new Exception("Production Order is required.");
                        }
                    }
                }

                string wcpoid = string.Empty;
                if (!string.IsNullOrEmpty(poId) && poId != "undefined")
                {
                    wcpoid = "AND PO.Id='" + poId + "'";
                }
                var sql = @"SELECT * FROM ( SELECT  
                                        isnull(e.Id,'') AS EntityId,isnull(e.UserName,'') Entity,
                                        isnull(ps.Id,'') AS ProductionStatusId, isnull(ps.UserName,'') AS ProductionStatus
										,PO.Id ProductionOrderId,PRS.LotNumber
                                      , PRS.ResponsiblePersonId,EI.EmployeeName ResponsiblePerson,PRS.ProductLibraryId, PL.Code ProductCode
                                                   , Buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),																												
		
													 BuyerId=STUFF((select distinct ','+XB.Id from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

																
                                                    CustomerId=STUFF((select distinct ','+XP.Id from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),   
                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                                 

                                        from TRN.ProductionSummary PRS
												left join trn.ProductionOrder PO ON PO.Id=PRS.ProductionOrderId
												left join dbo.EmployeeInformation EI ON EI.SystemId=PRS.ResponsiblePersonId
				                                left outer join org.Entity E on e.Id=PO.EntityID
				                                LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
												LEFT OUTER JOIN dbo.ProductLibrary PL ON PL.Id=PRS.ProductLibraryId
                              WHERE  PS.Id = '" + productionStatusId + @"'" + wcpoid + @") AS KK  ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetPOWiseProductionStatusData()
        {
            try
            {
                string strSql = @"SELECT distinct PP.Id,trke.UserName AS Entity,PP.ProductionOrderID PONo--,POPS.[Sequence] POProcessSequence 
                            ,wcm.UserName AS WorkCenter ,CPL.UserName AS ProductionShift,FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty,
                            isnull(p.UserName,FSFG.UserName) AS Process,p.Sequence StandardProcessSequence,ISNULL(pp.StandardName,ord.Article ) Article                  
                            ,ord.Product,
                            --additional info
			                     buyer=STUFF((select distinct ','+XB.UserName from 
			                            trn.SalesOrder XSO 
			                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            SalesOrderIds=STUFF((select distinct ','+XSO.Id from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                        BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

			
		                                StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
                                        OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            pt1.NoOfWorkStation,sn.ProductionHours  AS PlanHours,
                            ISNULL(ppt.ProductionHours,0) ProductionHours
                            
                            FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,COUNT(*) AS ProductionHours,SUM(ps.Quantity) AS Quantity
                                    FROM trn.ProductionSummary AS ps 
                                  left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
                                  LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
      		                            --WHERE ps.ProductionDate BETWEEN '01-Nov-2022' AND '30-Nov-2022' AND ps.EntityID in ('','14','15') 
      		                            --AND ps.ProcessId=(select XX.ProcessId from trn.ProductionOrderProcessSet AS XX where XX.IsBaseProcess=1 and XX.ProductionOrderID=ps.ProductionOrderId)
                                  GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId
                            ) AS pp
                            LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
                            LEFT JOIN trn.SalesOrder AS so ON so.Id=pp.SalesOrderId
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
                            LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=pp.ProductionOrderId AND sn.ProductionDate=pp.ProductionDate AND sn.WorkCenterMasterId=pp.WorkCenterMasterId AND sn.EntityID=pp.EntityId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=pp.FromSFGInventoryId
                           
                            LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id=pp.ToWorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id=pp.ToSFGInventoryId
                        
                            left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=pp.ProductionOrderId AND ppt.WorkCenterMasterId=PP.WorkCenterMasterId AND  ppt.ProcessId=PP.ProcessId AND ppt.EntityId=pp.EntityId and ppt.ProductionDate=PP.ProductionDate
                            --left outer join ProductionPlanningCalendar AS ppc on ppc.ProcessId=PP.ProcessId AND ppc.EntityId=pp.EntityId and PPC.WorkingDate=PP.ProductionDate
                            left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
							LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
							--LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id
                             left outer join (
                                                        select POD.ProductionOrderId,mm.UserName AS Material,MA.StandardName AS Article,PM.UserName AS Product,PC.UserName AS ProductCategory,
                                                          SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
                                                          SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM
                                                        from trn.ProductionOrderDetail POD 
                                                        left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                                                        left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                        left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                                                        left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
                                                        left JOIN org.Company AS com ON com.Id=mo.CompanyId
                                                        LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN ('','14','15'))
                                                        LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN ('','14','15'))
                                                        LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                                                        left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                                        LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                        left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                        left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                        left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                                        group by mm.UserName,MA.StandardName,PM.UserName,PC.UserName,POD.ProductionOrderId
                                              ) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId";
                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> SalesOrderfilters()
        {
            try
            {
                var sql = @"SELECT SO.Id SOId,OS.UserName OrderStatus,SO.ResponsiblePersonId,EI.EmployeeName ResponsiblePerson
                                        ,P.Id CustomerId,P.UserName Customer                                             

                                        from TRN.SalesOrder SO
		                                left outer join trn.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
		                                left outer join trn.MasterOrder MO on mo.Id=MOI.MasterOrderId
		                                left outer join [HKP].[Party] P on P.Id=MO.PartyId
										left join [HKP].[OrderStatus] OS on OS.Id=SO.OrderStatusId
										left join dbo.EmployeeInformation EI ON EI.SystemId=SO.ResponsiblePersonId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void getSalesOrderDistribution(string date, Dictionary<string, string> parameters, out Dictionary<string, List<DataRow>> dicDistributedSO, out DataTable dt)
        {

            string sql = @"
                                select D.*,MMN.ProductionStartDate,0 AS CummProductionQty,0 AS CummPlanQty,ISNULL(d.ProductionQty,0)+ISNULL(d.PlanQty,0) AS TotalQty,0 AS CummTotalQty  
                                from (SELECT p1.ProductionOrderID,FORMAT(p1.ProductionDate,'dd-MMM-yyyy')AS ProductionDate,0 AS ProductionQty,SUM(p1.Quantity) AS PlanQty
                                                   from ProductionPlanningType1 p1 
                                                 WHERE p1.ProductionDate>='" + date + @"'  AND P1.EntityId in (" + parameters["EntityId"] + @")
                                                 GROUP BY  p1.ProductionDate,p1.ProductionOrderID
                 
                                                 UNION ALL
                 
                                                 SELECT s.ProductionOrderId,FORMAT(s.ProductionDate,'dd-MMM-yyyy') AS ProductionDate,SUM(s.Quantity) AS ProductionQty,0 AS PlanQty
				                                FROM  trn.ProductionSummary S 
					                                WHERE S.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=S.ProductionOrderID) AND  S.EntityId in (" + parameters["EntityId"] + @") AND CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<CONVERT(DATETIME,'" + date + @"')
				                                GROUP BY  s.ProductionOrderId,s.ProductionDate
                                ) AS D 
                                left join (
                                   select ProductionOrderID,FORMAT(MIN(ProductionDate),'dd-MMM-yyyy')  AS ProductionStartDate 
                                    from ( SELECT p1.ProductionOrderID,MIN(p1.ProductionDate) AS ProductionDate
                                                   from ProductionPlanningType1 p1 
                                                 WHERE p1.ProductionDate>='" + date + @"'  AND P1.EntityId in (" + parameters["EntityId"] + @")
                                                 GROUP BY  p1.ProductionOrderID
                 
                                                 UNION ALL
                 
                                                 SELECT s.ProductionOrderId,MIN(s.ProductionDate) AS ProductionDate
				                                FROM  trn.ProductionSummary S 
					                                WHERE S.ProcessID=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=S.ProductionOrderID) AND  S.EntityId in (" + parameters["EntityId"] + @") AND CONVERT(DATETIME, format(s.ProductionDate,'dd-MMM-yyyy'))<CONVERT(DATETIME,'" + date + @"')
				                                GROUP BY  s.ProductionOrderId) AS K group by ProductionOrderID

                                    ) AS MMN ON MMN.ProductionOrderId=D.ProductionOrderId

                                INNER JOIN trn.ProductionOrder AS po ON po.Id=d.ProductionOrderID
                                INNER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                                WHERE PO.Id IN (SELECT DISTINCT p.ProductionOrderId FROM trn.ProductionOrderDetail AS p
                                            JOIN trn.SalesOrder AS so ON so.Id=p.SalesOrderId
                                            WHERE so.OrderStatusId<>'Closed')
                                ORDER BY D.ProductionOrderID,convert(date,D.ProductionDate)

                            ";


            dt = _sqlRepository.GetDataTable(sql);
            dicDistributedSO = new Dictionary<string, List<DataRow>>();
            List<DataRow> row = new List<DataRow>();

            string Id = ""; double CummProductionQty = 0; double CummPlanQty = 0; double CummTotalQty = 0;
            string ProductionEndDate = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (Id != dt.Rows[i]["ProductionOrderID"].ToString())
                {
                    CummProductionQty = 0; CummPlanQty = 0; CummTotalQty = 0;
                    row = new List<DataRow>();
                    dicDistributedSO.Add(dt.Rows[i]["ProductionOrderID"].ToString(), row);

                    ProductionEndDate = dt.Rows[i]["ProductionDate"].ToString();
                }

                dt.Rows[i]["ProductionStartDate"] = ProductionEndDate;

                CummProductionQty += clsStaticInfo.dbl(dt.Rows[i]["ProductionQty"].ToString());
                CummPlanQty += clsStaticInfo.dbl(dt.Rows[i]["PlanQty"].ToString());
                CummTotalQty += clsStaticInfo.dbl(dt.Rows[i]["TotalQty"].ToString());
                ProductionEndDate = dt.Rows[i]["ProductionDate"].ToString();

                dt.Rows[i]["CummProductionQty"] = CummProductionQty;
                dt.Rows[i]["CummPlanQty"] = CummPlanQty;
                dt.Rows[i]["CummTotalQty"] = CummTotalQty;

                row.Add(dt.Rows[i]);

                Id = dt.Rows[i]["ProductionOrderID"].ToString();
            }


        }

        public void GetProductionOrderMaster(Dictionary<string, string> parameters, out DataTable dtOrderMaster)
        {
            try
            {
                string sql = @"Select * from(Select row_number() over (partition by po.Id order by po.Id,A.Date) as Seq
,po.Id POId,sc.ID ScheduleId,PS.UserName POStatus,FORMAT(PO.AddedDate,'dd-MMM-yyyy')POCreationDate ,FORMAT(BASEP.BaseProcProdStartDate,'dd-MMM-yyyy')BaseProcProdStartDate,FORMAT(BASEP.BaseProductionEndDate,'dd-MMM-yyyy')BaseProductionEndDate
,FORMAT(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')BaseProcPlanStartDate,FORMAT(Type1.BaseProcPlanEndDate,'dd-MMM-yyyy')BaseProcPlanEndDate
,POStartDate=FORMAT(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  <  Type1.BaseProcPlanStartDate then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end,'dd-MMM-yyyy')
,POCompletionDate=FORMAT((case when Type1.BaseProcPlanEndDate is null or BASEP.BaseProductionEndDate  > Type1.BaseProcPlanEndDate then BASEP.BaseProductionEndDate else Type1.BaseProcPlanEndDate end ),'dd-MMM-yyyy')
,COUNT(SO.id) NoOfSO
,FORMAT(A.Date,'dd-MMM-yyyy') Date

,PlanningStatus=CASE WHEN FORMAT(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  <  Type1.BaseProcPlanStartDate then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end,'dd-MMM-yyyy') IS NULL 
OR FORMAT((case when Type1.BaseProcPlanEndDate is null or BASEP.BaseProductionEndDate  > Type1.BaseProcPlanEndDate then BASEP.BaseProductionEndDate else Type1.BaseProcPlanEndDate end ),'dd-MMM-yyyy') IS NULL OR SC.Id IS NULL THEN 'Schedule Missing' ELSE 'Schedule' END
,POCompletion= CASE WHEN A.Date<= GETDATE() Then 'Complete' else 'Scheduled' END 
,A.ProdQty,A.PlanQty,AvailableQty= CASE WHEN ISNULL(A.ProdQty,0)>0 THEN A.ProdQty ELSE A.PlanQty END

,CumProdQty=SUM(CASE WHEN ISNULL(A.ProdQty,0)>0 THEN A.ProdQty ELSE A.PlanQty END) OVER(PARTITION BY PO.ID ORDER BY A.Date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)


FROM trn.SalesOrder SO
LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=so.Id
LEFT JOIN(Select MIN(ProductionDate)BaseProcProdStartDate,MAX(ProductionDate)BaseProductionEndDate,A.ProductionOrderId 
FROM TRN.ProductionSummary A
LEFT JOIN HKP.Process B ON B.Id=A.ProcessId
Group By A.ProductionOrderId) BASEP ON BASEP.ProductionOrderId=POD.ProductionOrderId

LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanEndDate,ProductionOrderId 
From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=POD.ProductionOrderId
LEFT JOIN TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN dbo.ProductionOrderSchedulingParametersType1 SC ON Sc.ProductionOrderID=PO.Id
LEFT JOIN(
Select B.* from
(
Select PS.ProductionOrderId POId,PS.ProductionDate Date,SUM(Quantity)ProdQty,0 PlanQty from TRN.ProductionOrder PO
LEFT JOIN TRN.ProductionSummary PS ON PS.ProductionOrderId=PO.Id
left join TRN.ProductionOrderProcessSet A ON A.ProductionOrderId=PS.ProductionOrderId  AND PS.ProcessId=A.ProcessId Where A.IsBaseProcess=1 Group BY PS.ProductionOrderId,PS.ProductionDate
UNION
Select DISTINCT PO.Id POId,T1.ProductionDate Date, 0 ProdQty,SUM(T1.Quantity) PlanQty 
from TRN.ProductionOrder PO
LEFT JOIN dbo.ProductionPlanningType1 T1 ON T1.ProductionOrderID=PO.Id
Group BY PO.Id,T1.ProductionDate
)B Where ISNULL(B.Date,'')<>'' 
)A ON A.POId=PO.Id

Where SO.OrderStatusId NOT IN('Cancelled','Closed') AND SO.ShipmentFromStock=0 and pod.ProductionOrderId<>''
GROUP BY po.Id,BASEP.BaseProcProdStartDate,BASEP.BaseProductionEndDate,Type1.BaseProcPlanStartDate,Type1.BaseProcPlanEndDate
,A.Date,sc.ID,PS.UserName,PO.AddedDate,A.ProdQty,A.PlanQty)x
Where X.POId IN (" + parameters["POId"] + @")";
                dtOrderMaster = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetSOCompletionData(Dictionary<string, string> parameters,out DataTable dt)
        {
            try
            {
                string sql = @"SELECT row_number() over (partition by POD.ProductionOrderId order by POD.ProductionOrderId,SO.DeliveryDate) as Seq,
POD.ProductionOrderId,SO.OrderStatusId SOStatus,m.[Days]
,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy')DeliveryDate,SO.Id SOId,SO.Qty SOQty
,SoCommqty=SUM(SO.Qty) OVER (PARTITION BY POD.ProductionOrderId ORDER BY SO.DeliveryDate ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)
,P.UserName Customer,MOI.BuyerReferenceNo,moi.OwnReferenceNo,moi.Id LineitemId,MMA.StandardName Article,PL.Code ProductCode
,ProductLibraryDetail=STUFF((select distinct ','+MA.Code+'-'+MA.AttributeValue from
												[dbo].ProductLibraryAttribute MA												
												where MA.ProductLibraryId=PL.Id for xml path('') ), 1, 1, '')

,PS.UserName POStatus,FORMAT(SO.PlanExFactoryDate,'dd-MMM-yyyy')ExFactoryDate,FORMAT(SO.CommitmentDate,'dd-MMM-yyyy')CommitmentDate,RP.EmployeeName ResponsiblePerson,E.UserName Entity,CP.PartyType,DiffComEx=CASE  WHEN SO.CommitmentDate IS NULL THEN DATEDIFF(DAY,PlanExFactoryDate,GETDATE()) ELSE DATEDIFF(DAY,SO.CommitmentDate,GETDATE()) END
from trn.SalesOrder SO
left join TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
left join TRN.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet M ON m.ProductionOrderId=POD.ProductionOrderId
AND m.Id=(SELECT TOP 1 ID FROM TRN.ProductionOrderProcessSet EII WHERE EII.ProductionOrderId=POD.ProductionOrderId ORDER BY EII.Sequence DESC)
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
LEFT JOIN TRN.MasterOrder MO ON MO.Id=MOI.MasterOrderId
LEFT JOIN HKP.Party P ON P.Id=MO.PartyId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
LEFT JOIN [dbo].[ProductLibrary] PL ON PL.Id=MOI.ProductLibraryId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN dbo.EmployeeInformation RP ON RP.SystemId=SO.ResponsiblePersonId
LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'
Where  SO.OrderStatusId NOT IN('Cancelled','Closed') AND SO.ShipmentFromStock=0  AND POD.ProductionOrderId<>''
AND PO.Id IN (" + parameters["POId"] + @") AND P.Id IN (" + parameters["PartyId"] + @")  AND SO.ResponsiblePersonId IN (" + parameters["ResponsiblePersonId"] + @") ";

                dt = _sqlRepository.GetDataTable(sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSOCompletionReportFilter()
        {
            try
            {
                string sql = @"SELECT distinct POD.ProductionOrderId POId, SO.ResponsiblePersonId,E.EmployeeName ResponsiblePerson,MO.PartyId,P.UserName Customer,SO.DeliveryDate
FROM  TRN.SalesOrder SO 
left join TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
LEFT JOIN TRN.MasterOrder MO ON MO.Id=MOI.MasterOrderId
LEFT JOIN HKP.Party P ON P.Id = MO.PartyId
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=SO.ResponsiblePersonId
Where  SO.OrderStatusId NOT IN('Cancelled','Closed') AND SO.ShipmentFromStock=0  AND POD.ProductionOrderId<>''";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetBookingLevelByPrOandProcess(string poId,string processId)
        {
            try
            {
                string sql = @"Select ProductionBookingLevel from TRN.ProductionOrderProcessSet PPS Where PPS.ProductionOrderID = '25178' AND PPS.ProcessId = '202037'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {
                string partyId = "AND 1=1";
                if (!string.IsNullOrEmpty(parameters["CustomerId"].ToString()))
                {
                    partyId = "AND XMO.PartyId in(" + parameters["CustomerId"] + @")";
                }

                string sql = @"Select A.* from (SELECT DISTINCT PP.Id PSId,trke.UserName AS Entity,PP.ProductionOrderID PONo,PSEQ.ProcessIndex,isnull(p.UserName, '') AS Process,p.Sequence StandardProcessSequence,POPS.[Sequence] POProcessSequence
		,BaseProcess = CASE WHEN P.IsProductionProcess = 1 THEN 'Yes' ELSE 'No' END,FORMAT(PP.ProductionDate, 'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty,ProcessWisePlanQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)*POPS.Qty,PSEQ.Qty UpToDateProduction,ISNULL(PSEQ.PreQty, 0) PreProUDProd
		,WIP = ISNULL(PSEQ.PreQty-PSEQ.Qty, 0),UptoDateProPercentage = (pp.Quantity / (select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)) / 100,wcm.UserName AS WorkCenter,CPL.UserName AS ProductionShift,ISNULL(pp.StandardName, ord.Article) Article
		,ord.Product,PS.UserName POStatus,FLB.FirstBookDate,FLB.LastBookDate,PFLB.POFirstBookDate,PFLB.POLastBookDate
		,PP.LotNumber,POProcessStatus=CASE WHEN POPS.IsCompleted=1 THEN 'Completed' ELSE 'Not Completed' END
--additional info
		,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pp.ProductionOrderId=Xpod.ProductionOrderId " + partyId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')	
,Buyer=STUFF((select distinct ','+XB.UserName from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
SONos=STUFF((select distinct ','+XSO.Id from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PlannedQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)
,FirstShipmentDate=(Select FORMAT(MIN(SO.DeliveryDate),'dd-MMM-yyyy') from TRN.SalesOrder SO JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=so.Id where pp.ProductionOrderID=Xpod.ProductionOrderId)
,LastShipmentDate=(Select FORMAT(MAX(SO.DeliveryDate),'dd-MMM-yyyy') from TRN.SalesOrder SO JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=so.Id where pp.ProductionOrderID=Xpod.ProductionOrderId)

,BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
wcm.NoOfWorkStation,ProductionHours=(select top(1) Hour from scs.WorkCenterMasterEffectiveDate Where WorkCenterMasterId=wcm.Id Order BY StartDate Desc)
                            
FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,COUNT(*) AS ProductionHours,SUM(ps.Quantity) AS Quantity,PS.ResponsiblePersonId,PS.LotNumber

FROM trn.ProductionSummary AS ps 
left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,PS.ResponsiblePersonId,PS.LotNumber
) AS pp
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS FirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS LastBookDate,ProcessId,ProductionOrderId from TRN.ProductionSummary GROUP BY ProcessId,ProductionOrderId) FLB ON FLB.ProcessId=PP.ProcessId AND FLB.ProductionOrderId=PP.ProductionOrderId
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS POFirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS POLastBookDate,ProductionOrderId from TRN.ProductionSummary GROUP BY ProductionOrderId) PFLB ON PFLB.ProductionOrderId=PP.ProductionOrderId
LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id AND POPS.ProcessId=pp.ProcessId
left outer join (
select POD.ProductionOrderId,MA.StandardName AS Article,PM.UserName AS Product
from trn.ProductionOrderDetail POD 
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
group by MA.StandardName,PM.UserName,POD.ProductionOrderId
) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN(
SELECT T1.*,T2.Qty PreQty FROM
(Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence) ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T1
LEFT JOIN (Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence)+1 ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T2 ON T1.ProcessIndex=T2.ProcessIndex AND  T1.ProductionOrderId=T2.ProductionOrderId
) PSEQ ON PSEQ.ProductionOrderId=PP.ProductionOrderID AND POPS.[Sequence]=PSEQ.Sequence
Where TRKE.Id in(" + parameters["EntityId"] + @")
AND ISNULL(PP.ResponsiblePersonId,'') in(" + parameters["ResponsiblePersonId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @"))A Order BY A.PONo,A.ProcessIndex ";

                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public void GetWCReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {
                string partyId = "AND 1=1";
                if (!string.IsNullOrEmpty(parameters["CustomerId"].ToString()))
                {
                    partyId = "AND XMO.PartyId in(" + parameters["CustomerId"] + @")";
                }
                string sql = @"Select A.Entity,A.Process,A.POProcessSequence,A.StandardProcessSequence,A.PONo,A.ProcessIndex,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.WorkCenter,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article,A.NoOfWorkStation,A.ProductionHours
,A.PlannedQty,SUM(A.ActualQty) ActualQty,A.UpToDateProduction,A.PreProUDProd,A.WIP,A.FirstBookDate,A.LastBookDate,A.POFirstBookDate,A.POLastBookDate,A.FirstShipmentDate,A.LastShipmentDate  
from (SELECT DISTINCT PP.Id PSId,trke.UserName AS Entity,PP.ProductionOrderID PONo,PSEQ.ProcessIndex,isnull(p.UserName, '') AS Process,p.Sequence StandardProcessSequence,POPS.[Sequence] POProcessSequence
		,BaseProcess = CASE WHEN P.IsProductionProcess = 1 THEN 'Yes' ELSE 'No' END,pp.Quantity AS ActualQty,ProcessWisePlanQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)*POPS.Qty,PSEQ.Qty UpToDateProduction,ISNULL(PSEQ.PreQty, 0) PreProUDProd
		,WIP = ISNULL(PSEQ.PreQty-PSEQ.Qty, 0),UptoDateProPercentage = (pp.Quantity / (select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)) / 100,wcm.UserName AS WorkCenter,ISNULL(pp.StandardName, ord.Article) Article
		,ord.Product,PS.UserName POStatus,FLB.FirstBookDate,FLB.LastBookDate,PFLB.POFirstBookDate,PFLB.POLastBookDate
		,PP.LotNumber,POProcessStatus=CASE WHEN POPS.IsCompleted=1 THEN 'Completed' ELSE 'Not Completed' END
--additional info
		,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pp.ProductionOrderId=Xpod.ProductionOrderId " + partyId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')	
,Buyer=STUFF((select distinct ','+XB.UserName from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
SONos=STUFF((select distinct ','+XSO.Id from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
FirstShipmentDate=(Select FORMAT(MIN(SO.DeliveryDate),'dd-MMM-yyyy') from TRN.SalesOrder SO JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=so.Id where pp.ProductionOrderID=Xpod.ProductionOrderId)
,LastShipmentDate=(Select FORMAT(MAX(SO.DeliveryDate),'dd-MMM-yyyy') from TRN.SalesOrder SO JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=so.Id where pp.ProductionOrderID=Xpod.ProductionOrderId)
,PlannedQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId),
BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
wcm.NoOfWorkStation,ProductionHours=(select top(1) Hour from scs.WorkCenterMasterEffectiveDate Where WorkCenterMasterId=wcm.Id Order BY StartDate Desc)
 FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId, ps.ProductionOrderId,ps.WorkCenterMasterId,SUM(ps.Quantity) AS Quantity,PS.ResponsiblePersonId,PS.LotNumber
FROM trn.ProductionSummary AS ps 
left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionOrderId,ps.WorkCenterMasterId,PS.ResponsiblePersonId,PS.LotNumber
) AS pp
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS FirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS LastBookDate,ProcessId,ProductionOrderId from TRN.ProductionSummary GROUP BY ProcessId,ProductionOrderId) FLB ON FLB.ProcessId=PP.ProcessId AND FLB.ProductionOrderId=PP.ProductionOrderId
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS POFirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS POLastBookDate,ProductionOrderId from TRN.ProductionSummary GROUP BY ProductionOrderId) PFLB ON PFLB.ProductionOrderId=PP.ProductionOrderId
LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id AND POPS.ProcessId=pp.ProcessId
left outer join (
select POD.ProductionOrderId,MA.StandardName AS Article,PM.UserName AS Product
from trn.ProductionOrderDetail POD 
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
group by MA.StandardName,PM.UserName,POD.ProductionOrderId
) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN(
SELECT T1.*,T2.Qty PreQty FROM
(Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence) ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T1
LEFT JOIN (Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence)+1 ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T2 ON T1.ProcessIndex=T2.ProcessIndex AND  T1.ProductionOrderId=T2.ProductionOrderId
) PSEQ ON PSEQ.ProductionOrderId=PP.ProductionOrderID AND POPS.[Sequence]=PSEQ.Sequence
Where TRKE.Id in(" + parameters["EntityId"] + @")
AND ISNULL(PP.ResponsiblePersonId,'') in(" + parameters["ResponsiblePersonId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @"))A 
GROUP BY A.PONo,A.ProcessIndex,A.Process,A.UpToDateProduction,A.PreProUDProd,A.WIP,A.POProcessSequence
,A.Entity,A.Process,A.POProcessSequence,A.StandardProcessSequence,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.WorkCenter,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article,A.NoOfWorkStation,A.ProductionHours,A.PlannedQty,A.FirstBookDate,A.LastBookDate,A.POFirstBookDate,A.POLastBookDate,A.FirstShipmentDate,A.LastShipmentDate 
Order BY A.PONo,A.ProcessIndex";


                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        public void GetSummaryReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {
                string partyId = "AND 1=1";
                if (!string.IsNullOrEmpty(parameters["CustomerId"].ToString()))
                {
                    partyId = "AND XMO.PartyId in(" + parameters["CustomerId"] + @")";
                }
                string sql = @"Select A.Entity,A.Process,A.POProcessSequence,A.StandardProcessSequence,A.PONo,A.ProcessIndex,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article
,A.PlannedQty,SUM(A.ActualQty) ActualQty,A.UpToDateProduction,A.PreProUDProd,A.WIP,A.FirstBookDate,A.LastBookDate,A.POFirstBookDate,A.POLastBookDate,A.FirstShipmentDate,A.LastShipmentDate 
from (SELECT DISTINCT PP.Id PSId,trke.UserName AS Entity,PP.ProductionOrderID PONo,PSEQ.ProcessIndex,isnull(p.UserName, '') AS Process,p.Sequence StandardProcessSequence,POPS.[Sequence] POProcessSequence
		,BaseProcess = CASE WHEN P.IsProductionProcess = 1 THEN 'Yes' ELSE 'No' END,pp.Quantity AS ActualQty,ProcessWisePlanQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)*POPS.Qty,PSEQ.Qty UpToDateProduction,ISNULL(PSEQ.PreQty, 0) PreProUDProd
		,WIP = ISNULL(PSEQ.PreQty-PSEQ.Qty, 0),UptoDateProPercentage = (pp.Quantity / (select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)) / 100,ISNULL(pp.StandardName, ord.Article) Article
		,ord.Product,PS.UserName POStatus,FLB.FirstBookDate,FLB.LastBookDate,PFLB.POFirstBookDate,PFLB.POLastBookDate
		,PP.LotNumber,POProcessStatus=CASE WHEN POPS.IsCompleted=1 THEN 'Completed' ELSE 'Not Completed' END
--additional info
		,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pp.ProductionOrderId=Xpod.ProductionOrderId " + partyId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')	
,Buyer=STUFF((select distinct ','+XB.UserName from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
SONos=STUFF((select distinct ','+XSO.Id from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
FirstShipmentDate=(Select FORMAT(MIN(SO.DeliveryDate),'dd-MMM-yyyy') from TRN.SalesOrder SO JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=so.Id where pp.ProductionOrderID=Xpod.ProductionOrderId)
,LastShipmentDate=(Select FORMAT(MAX(SO.DeliveryDate),'dd-MMM-yyyy') from TRN.SalesOrder SO JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=so.Id where pp.ProductionOrderID=Xpod.ProductionOrderId)
,PlannedQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId),
BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionOrderId,SUM(ps.Quantity) AS Quantity,PS.ResponsiblePersonId,PS.LotNumber

FROM trn.ProductionSummary AS ps 
left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId, ps.ProductionOrderId,PS.ResponsiblePersonId,PS.LotNumber
) AS pp
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS FirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS LastBookDate,ProcessId,ProductionOrderId from TRN.ProductionSummary GROUP BY ProcessId,ProductionOrderId) FLB ON FLB.ProcessId=PP.ProcessId AND FLB.ProductionOrderId=PP.ProductionOrderId
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS POFirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS POLastBookDate,ProductionOrderId from TRN.ProductionSummary GROUP BY ProductionOrderId) PFLB ON PFLB.ProductionOrderId=PP.ProductionOrderId
left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
LEFT JOIN trn.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id AND POPS.ProcessId=pp.ProcessId
left outer join (
select POD.ProductionOrderId,MA.StandardName AS Article,PM.UserName AS Product
from trn.ProductionOrderDetail POD 
left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
group by MA.StandardName,PM.UserName,POD.ProductionOrderId
) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN(
SELECT T1.*,T2.Qty PreQty FROM
(Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence) ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T1
LEFT JOIN (Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence)+1 ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T2 ON T1.ProcessIndex=T2.ProcessIndex AND  T1.ProductionOrderId=T2.ProductionOrderId
) PSEQ ON PSEQ.ProductionOrderId=PP.ProductionOrderID AND POPS.[Sequence]=PSEQ.Sequence
Where TRKE.Id in(" + parameters["EntityId"] + @")
AND ISNULL(PP.ResponsiblePersonId,'') in(" + parameters["ResponsiblePersonId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @"))A 
GROUP BY A.PONo,A.ProcessIndex,A.Process,A.UpToDateProduction,A.PreProUDProd,A.WIP,A.POProcessSequence
,A.Entity,A.Process,A.POProcessSequence,A.StandardProcessSequence,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article,A.PlannedQty,A.FirstBookDate,A.LastBookDate,A.POFirstBookDate,A.POLastBookDate,A.FirstShipmentDate,A.LastShipmentDate
Order BY A.PONo,A.ProcessIndex";


                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public void GetAllSummaryReportSQL(Dictionary<string, string> parameters, out DataTable data)
        {
            try
            {
                string partyId = "AND 1=1";
                if (!string.IsNullOrEmpty(parameters["CustomerId"].ToString()))
                {
                    partyId = "AND XMO.PartyId in(" + parameters["CustomerId"] + @")";
                }
                string sql = @"Select A.Entity,A.Process,A.POProcessSequence,A.ProductionProcess,A.StandardProcessSequence,A.PONo,A.ProcessIndex,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article
,A.PlannedQty,SUM(A.ActualQty) ActualQty,A.UpToDateProduction,A.PreProUDProd,A.WIP,A.FirstBookDate,A.LastBookDate,A.POFirstBookDate,A.POLastBookDate,A.FirstShipmentDate,A.LastShipmentDate  
from (SELECT DISTINCT PP.Id PSId,trke.UserName AS Entity,PP.ProductionOrderID PONo,PSEQ.ProcessIndex,isnull(p.UserName, '') AS Process,p.Sequence StandardProcessSequence,POPS.[Sequence] POProcessSequence,pps.UserName ProductionProcess
		,BaseProcess = CASE WHEN P.IsProductionProcess = 1 THEN 'Yes' ELSE 'No' END,pp.Quantity AS ActualQty,ProcessWisePlanQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)*POPS.Qty,PSEQ.Qty UpToDateProduction,ISNULL(PSEQ.PreQty, 0) PreProUDProd
		,WIP = ISNULL(PSEQ.PreQty-PSEQ.Qty, 0),UptoDateProPercentage = (pp.Quantity / (select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId)) / 100,ISNULL(pp.StandardName, ord.Article) Article
		,ord.Product,PS.UserName POStatus,FLB.FirstBookDate,FLB.LastBookDate,PFLB.POFirstBookDate,PFLB.POLastBookDate
		,PP.LotNumber,POProcessStatus=CASE WHEN POPS.IsCompleted=1 THEN 'Completed' ELSE 'Not Completed' END
--additional info
		,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pp.ProductionOrderId=Xpod.ProductionOrderId " + partyId + @" for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),'&amp;','&'), 'amp;', '')	
,Buyer=STUFF((select distinct ','+XB.UserName from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
SONos=STUFF((select distinct ','+XSO.Id from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
FirstShipmentDate=(Select FORMAT(MIN(SO.DeliveryDate),'dd-MMM-yyyy') from TRN.SalesOrder SO JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=so.Id where pp.ProductionOrderID=Xpod.ProductionOrderId)
,LastShipmentDate=(Select FORMAT(MAX(SO.DeliveryDate),'dd-MMM-yyyy') from TRN.SalesOrder SO JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=so.Id where pp.ProductionOrderID=Xpod.ProductionOrderId)
,PlannedQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pp.ProductionOrderID=Xpod.ProductionOrderId),
BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            
FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId, ps.ProductionOrderId,SUM(ps.Quantity) AS Quantity,PS.ResponsiblePersonId,PS.LotNumber

FROM trn.ProductionSummary AS ps 
left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId, ps.ProductionOrderId,PS.ResponsiblePersonId,PS.LotNumber
) AS pp
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS FirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS LastBookDate,ProcessId,ProductionOrderId from TRN.ProductionSummary GROUP BY ProcessId,ProductionOrderId) FLB ON FLB.ProcessId=PP.ProcessId AND FLB.ProductionOrderId=PP.ProductionOrderId
LEFT JOIN (Select FORMAT(MIN(ProductionDate),'dd-MMM-yyyy') AS POFirstBookDate,FORMAT(MAX(ProductionDate),'dd-MMM-yyyy') AS POLastBookDate,ProductionOrderId from TRN.ProductionSummary GROUP BY ProductionOrderId) PFLB ON PFLB.ProductionOrderId=PP.ProductionOrderId
LEFT OUTER JOIN TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
LEFT JOIN TRN.ProductionOrderProcessSet POPS ON POPS.ProductionOrderId=PO.Id
left join HKP.Process PPS on pps.Id=POPS.ProcessId
LEFT OUTER JOIN (
SELECT POD.ProductionOrderId,MA.StandardName AS Article,PM.UserName AS Product
FROM TRN.ProductionOrderDetail POD 
LEFT OUTER JOIN TRN.SalesOrder SO on so.id=pod.SalesOrderId
LEFT OUTER JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
LEFT OUTER JOIN mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
group by MA.StandardName,PM.UserName,POD.ProductionOrderId
) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId
LEFT JOIN HKP.ProductionStatus PS ON PS.Id=PO.ProductionStatusId
LEFT JOIN(
SELECT T1.*,T2.Qty PreQty FROM
(Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence) ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T1
LEFT JOIN (Select A.*,ROW_NUMBER() OVER(partition by A.ProductionOrderId ORDER BY A.Sequence)+1 ProcessIndex
from (select PS.ProductionOrderId,PSQ.Sequence, sum(PS.Quantity)Qty from TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrder P ON P.Id=PS.ProductionOrderId
LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id AND PSQ.ProcessId=PS.ProcessId
LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
LEFT JOIN HKP.Process PRO ON PRO.Id=PS.ProcessId
Where PRS.Id in(" + parameters["ProductionStatusId"] + @") AND ISNULL(PSQ.Sequence,0)<>0
GROUP BY PS.ProductionOrderId,PSQ.Sequence
) A )T2 ON T1.ProcessIndex=T2.ProcessIndex AND  T1.ProductionOrderId=T2.ProductionOrderId
) PSEQ ON PSEQ.ProductionOrderId=PP.ProductionOrderID AND POPS.[Sequence]=PSEQ.Sequence
Where TRKE.Id in(" + parameters["EntityId"] + @")
AND ISNULL(PP.ResponsiblePersonId,'') in(" + parameters["ResponsiblePersonId"] + @")
AND ps.Id in(" + parameters["ProductionStatusId"] + @"))A 
GROUP BY A.PONo,A.ProcessIndex,A.Process,A.UpToDateProduction,A.PreProUDProd,A.WIP,A.POProcessSequence,A.ProductionProcess
,A.Entity,A.Process,A.POProcessSequence,A.StandardProcessSequence,A.BaseProcess,A.POProcessStatus,A.POStatus
,A.Buyer,A.Customer,A.LotNumber,A.OwnOrderNo,A.SONos,A.Product,A.Article,A.PlannedQty,A.FirstBookDate,A.LastBookDate,A.POFirstBookDate,A.POLastBookDate,A.FirstShipmentDate,A.LastShipmentDate 
Order BY A.PONo,A.ProcessIndex";


                data = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public void ProductionOrderParameterReportSQL(string fromDate, string toDate, string EntityId, string ProcessId, string ShiftId, out DataTable dtOrder)
        {
            try
            {
                string psft = "";
                if (!string.IsNullOrEmpty(ShiftId))
                {
                    psft = " AND ProductionShiftId='"+ ShiftId + "'";
                }

                string sql = @"SELECT  PP.Id,PBP.Sequence, trkp.UserName AS Plant,trke.UserName AS Entity,pp.EntityID,pp.WorkCenterMasterId, PP.ProductionOrderID,wcm.UserName AS WorkCenter,FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty,ORD.CM*pp.Quantity AS ActualCM,
                            pt1.SPT AS SAM,pp.ProcessId,isnull(p.UserName,FSFG.UserName) AS Process,isnull(Tp.UserName,TSFG.UserName) AS ToProcess,Twcm.UserName AS ToWorkCenter,ISNULL(pp.UserName,ord.Material) Material,ISNULL(pp.StandardName,ord.Article ) Article              
                            ,ISNULL(PL.Code,'-') ProductCode,ord.Product, ord.ProductCategory,Format(SN.AddedDate,'dd-MMM-yyyy') AS SnapshotDate,
                            sn.Quantity AS PlanQty,ORD.CM*sn.Quantity AS PlanCM,ORD.CM
                             ,CPL.UserName AS ProductionShift,so.Id AS SalesOrderIdBooking,CPL.ShiftDuration ShiftWorkingMin,ISNULL(so.[Description],'-') AS SalesOrderDescBooking,
                             wcm.StandardTimePerDay AS StandardWorkingHours,  wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
                             PP.ProductionHours AS WorkingHours,SN.isBuildUp,
                             pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
                            --additional info
			                     buyer=STUFF((select distinct ','+XB.UserName from 
			                            trn.SalesOrder XSO 
			                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            SalesOrderIds=STUFF((select distinct ','+XSO.Id from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

	                        SalesOrderDesc=STUFF((select distinct ','+XSO.Description from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                   
                                  MasterOrderNo=STUFF((select distinct ','+XMO.Id from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                        BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

			
		                                StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
                                        OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            pt1.NoOfWorkStation,sn.ProductionHours  AS PlanHours,
                            ISNULL(ppt.ProductionHours,0) ProductionHours,
                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0) AS PlanMinutes,
                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*sn.ProductionHours*60) AS PlanEfficiency,

                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0) AS ActualMinutes,
                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*pp.ProductionHours*60) AS ActualEfficiency
							,PSV.UserName Parameter,ParameterValue=CASE WHEN PBP.IsPreviousValueApplicable=1 THEN PSV.Value ELSE 0 END 
                            ,DetentionInMin=(select ISNULL(SUM(MMT.[Minute]),0) from 
			                        dbo.MachineMasterTransaction MMT 
			                        where MMT.ProcessId=pp.ProcessId and MMT.ShiftId=pp.ProductionShiftId and MMT.WorkCenterId=pp.WorkCenterMasterId and MMT.[Date]=pp.ProductionDate)
                            ,0 Utilization,pp.ProductionOrderId PORefNo,pp.AddedBy EntryBy,ISNULL(UOM.Code,'-') UOM,pp.Quantity ProductionQty,ISNULL(pp.Remarks,'-')Remarks

                            FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,COUNT(*) AS ProductionHours,ps.Quantity
									,PS.AddedBy,ps.Remarks,ps.ProductLibraryId
                                    FROM trn.ProductionSummary AS ps 
                                  left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
                                  LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
      		                      WHERE ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND ps.EntityID in (" + EntityId + @")  and ps.ProcessId in (" + ProcessId + @") "+ psft + @"
                                  GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,PS.AddedBy,ps.Remarks,ps.ProductLibraryId,ps.Quantity
                            ) AS pp
							
                            LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
							left join ProductLibrary PL on PL.Id=pp.ProductLibraryId
                            LEFT JOIN trn.SalesOrder AS so ON so.Id=pp.SalesOrderId
							left join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
							left join SCS.UnitOfMeasurement UOM on UOM.Id=MOI.UOMId
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
                            LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=pp.ProductionOrderId AND sn.ProductionDate=pp.ProductionDate AND sn.WorkCenterMasterId=pp.WorkCenterMasterId AND sn.EntityID=pp.EntityId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=pp.FromSFGInventoryId
                            LEFT OUTER JOIN dbo.ProductionSummaryParameterValue AS psv ON psv.ProductionSummaryId=pp.Id
                            LEFT OUTER JOIN [dbo].[ProductionBookingParameter] PBP ON PBP.Id=PSV.ProductionBookingParameterId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id=pp.ToWorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id=pp.ToSFGInventoryId
                        
                            left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=pp.ProductionOrderId AND ppt.WorkCenterMasterId=PP.WorkCenterMasterId AND  ppt.ProcessId=PP.ProcessId AND ppt.EntityId=pp.EntityId and ppt.ProductionDate=PP.ProductionDate
                            left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
							LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
							LEFT OUTER JOIN hkp.Process AS Tp ON Tp.Id=pp.ToProcessId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
                             left outer join (
                                                        select POD.ProductionOrderId,mm.UserName AS Material,MA.StandardName AS Article,PM.UserName AS Product,PC.UserName AS ProductCategory,
                                                          SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.Rate* so.Qty ELSE  so.Rate* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(so.Qty) AS FOB,
                                                          SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE  so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty) AS CM
                                                        from trn.ProductionOrderDetail POD 
                                                        left outer join trn.SalesOrder SO on so.id=pod.SalesOrderId
                                                        left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                        left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                                                        left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
                                                        left JOIN org.Company AS com ON com.Id=mo.CompanyId
                                                       LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + EntityId + @"))
                                                        LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e WHERE e.Id IN (" + EntityId + @"))
                                                        LEFT OUTER JOIN trn.Commitment AS c ON c.Id=mo.CommitmentId
                                                        left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                                                        LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                        left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                        left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                        left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                                        group by mm.UserName,MA.StandardName,PM.UserName,PC.UserName,POD.ProductionOrderId
                                              ) AS ORD on ord.ProductionOrderID=pp.ProductionOrderId
                                            ORDER BY PBP.Sequence";
                dtOrder = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        public void GetProductionSummaryData(string Date, string Entity, string ProcessId, out DataTable dtOrder)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                // string yd = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");

                strSql = @"select PS.Id ProductionSummaryId,wcm.Id,WCM.UserName WorkCenter,PS.ProductionOrderId PONo,PS.LotNumber  ,0 WIP

,ProductionAsOnDate=(select sum(Quantity) from TRN.ProductionSummary 
where ProductionDate between '" + Date + @"' and '" + Date + @"'  and EntityId = '" + Entity + @"' and ProcessId = '" + ProcessId + @"' AND WorkCenterMasterId=PS.WorkCenterMasterId AND ProductionOrderId=PS.ProductionOrderId AND LotNumber=PS.LotNumber)

,Article =STUFF((select distinct ','+MMA.StandardName from 
	trn.SalesOrder XSO 
    JOIN trn.MasterOrderItem AS MOI ON MOI.Id=XSO.MasterOrderItemId
    left join MST.MaterialMasterArticle MMA on MMA.Id=MOI.ArticleId	
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
    WHERE PS.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
,ProductCode =STUFF((select distinct ','+PL.Code from 
	trn.SalesOrder XSO 
    JOIN trn.MasterOrderItem AS MOI ON MOI.Id=XSO.MasterOrderItemId
    left join dbo.ProductLibrary PL on PL.Id=MOI.ProductLibraryId
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
    WHERE PS.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
,Product =STUFF((select distinct ','+PM.UserName from 
	trn.SalesOrder XSO 
    JOIN trn.MasterOrderItem AS MOI ON MOI.Id=XSO.MasterOrderItemId
    left join MST.MaterialMaster MM on MM.Id=MOI.MaterialMasterId
left join TRN.ProductDefinition AS PD ON PD.MaterialMasterId=MM.Id
left join [MST].[ProductMaster] PM on PM.Id=PD.ProductMasterId
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
    WHERE PS.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
,SONo =STUFF((select distinct ','+XSO.Id from 
	trn.SalesOrder XSO 
    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
    WHERE PS.ProductionOrderId=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

from TRN.ProductionSummary PS
left join SCS.WorkCenterMaster WCM on WCM.Id=PS.WorkCenterMasterId AND WCM.Active=1							  
WHERE PS.ProductionDate between '" + Date + @"' and '" + Date + @"' and PS.EntityId = '" + Entity + @"' and PS.ProcessId = '" + ProcessId + @"'";

                dtOrder = _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public Dictionary<string, List<DataRow>> GetProductionParameterData(string Date, string Entity, string ProcessId, out DataTable dtParameter)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicParameter = new Dictionary<string, List<DataRow>>();
            dtParameter = new DataTable("Tmp");
            try
            {
                strSql = @"SELECT PV.ProductionSummaryId,Value = CASE WHEN PB.IsPreviousValueApplicable = 1 THEN PV.Value ELSE 0 END,PV.UserName,PV.ProductionBookingParameterId,PB.Sequence
    FROM [dbo].[ProductionSummaryParameterValue] PV
   LEFT JOIN[dbo].[ProductionBookingParameter] PB ON PB.Id = PV.ProductionBookingParameterId
Where PV.ProductionSummaryId IN(select Id from TRN.ProductionSummary
where ProductionDate between '" + Date + @"' and '" + Date + @"' and EntityId = '" + Entity + @"' and ProcessId = '" + ProcessId + @"') 
Order by PV.ProductionSummaryId,PB.Sequence";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(strSql, out dsRef);

                dtParameter = dsRef.Tables[0].DefaultView.ToTable(true, "ProductionBookingParameterId", "UserName");
                dtParameter = dtParameter.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["ProductionSummaryId"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicParameter.Add(dt.Rows[i]["ProductionSummaryId"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["ProductionSummaryId"].ToString();
                }

                return dicParameter;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        public void ProductionOrderReportSQL(string fromDate, string toDate, string EntityId, string ProcessId, out DataTable dtOrder)
        {
            try
            {
                string wp = "";
                if (string.IsNullOrEmpty(ProcessId) || ProcessId == "''")
                {
                    wp = @"SELECT DISTINCT P.Id AS [Value] FROM HKP.EntityProcessTag AS EP
                            JOIN HKP.Process AS P ON EP.ProcessId = P.Id
                            where EP.EntityId in (" + EntityId + @") AND P.Active = 1";
                }
                else
                {
                    wp = ProcessId;
                }

                //string sql = @"SELECT A.* from (SELECT distinct PP.Id, trkp.UserName AS Plant,trke.UserName AS Entity,pp.EntityID,pp.WorkCenterMasterId, PP.ProductionOrderID,wcm.UserName AS WorkCenter,FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty
                //--,ORD.CM*pp.Quantity AS ActualCM,
                //,(select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
                //							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
                //							where PO.Id=PoD.ProductionOrderId)*pp.Quantity AS ActualCM
                //                            ,pt1.SPT AS SAM,pp.ProcessId,isnull(p.UserName,FSFG.UserName) AS Process,isnull(Tp.UserName,TSFG.UserName) AS ToProcess,Twcm.UserName AS ToWorkCenter
                //							,Material=STUFF((select distinct ','+MA.UserName from
                //											MST.MaterialMaster MA
                //											left join TRN.MasterOrderItem moi on moi.MaterialMasterId=MA.Id
                //											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
                //											INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
                //											where PO.Id=PoD.ProductionOrderId for xml path('') ), 1, 1, '')
                //							,Article=STUFF((select distinct ','+MA.StandardName from
                //											MST.MaterialMasterArticle MA
                //											left join TRN.MasterOrderItem moi on moi.ArticleId=MA.Id
                //											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
                //											INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
                //											where PO.Id=PoD.ProductionOrderId for xml path('') ), 1, 1, '')            
                //                           ,PL.Code ProductCode
                //						    ,Product=STUFF((select distinct ','+PM.UserName from
                //											MST.MaterialMaster mm
                //											left join TRN.MasterOrderItem moi on moi.MaterialMasterId=mm.Id
                //											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
                //											left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                //                                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                //											INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
                //											where PO.Id=PoD.ProductionOrderId for xml path('') ), 1, 1, '')						   
                //							,ProductCategory=STUFF((select distinct ','+pc.UserName from
                //								[HKP].[ProductCategory] PC
                //								left join [MST].[ProductMaster] PM on pc.Id=pm.ProductCategoryId
                //								left join trn.ProductDefinition AS pd ON pd.ProductMasterId=pm.Id
                //								left join mst.MaterialMaster mm on mm.id=pd.MaterialMasterId
                //								left join TRN.MasterOrderItem moi on moi.MaterialMasterId=MM.Id
                //								left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
                //								INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
                //								where PO.Id=PoD.ProductionOrderId for xml path('') ), 1, 1, '')
                //						   ,Format(SN.AddedDate,'dd-MMM-yyyy') AS SnapshotDate
                //                         ,sn.Quantity AS PlanQty,((select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
                //							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
                //							where PO.Id=PoD.ProductionOrderId)*pp.Quantity)*sn.Quantity AS PlanCM
                //,(select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
                //							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
                //							where PO.Id=PoD.ProductionOrderId) CM
                //                             ,CPL.UserName AS ProductionShift,so.Id AS SalesOrderIdBooking,CPL.ShiftDuration ShiftWorkingMin,so.[Description] AS SalesOrderDescBooking,
                //                             wcm.StandardTimePerDay AS StandardWorkingHours,  wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
                //                             PP.ProductionHours AS WorkingHours,SN.isBuildUp,
                //                             pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
                //                            --additional info
                //			                     buyer=STUFF((select distinct ','+XB.UserName from 
                //			                            trn.SalesOrder XSO 
                //			                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //			                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                //			                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                //			                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                //			                            where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                //                            SalesOrderIds=STUFF((select distinct ','+XSO.Id from 
                //			                        trn.SalesOrder XSO 
                //			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //			                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                //	                        SalesOrderDesc=STUFF((select distinct ','+XSO.Description from 
                //			                        trn.SalesOrder XSO 
                //			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //			                        where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                //                                  MasterOrderNo=STUFF((select distinct ','+XMO.Id from 
                //			                                trn.SalesOrder XSO 
                //			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                //			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                //			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                //                                        BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
                //			                                trn.SalesOrder XSO 
                //			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                //			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                //			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                //                                     OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
                //			                                trn.SalesOrder XSO 
                //			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                //			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                //			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                //		                                StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
                //			                                trn.SalesOrder XSO 
                //			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
                //			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                //                                        OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
                //			                                trn.SalesOrder XSO 
                //			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                //			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
                //			                                where pp.ProductionOrderID=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                //                            pt1.NoOfWorkStation,sn.ProductionHours  AS PlanHours,
                //                            ISNULL(ppt.ProductionHours,0) ProductionHours,
                //                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0) AS PlanMinutes,
                //                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*sn.ProductionHours*60) AS PlanEfficiency,

                //                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0) AS ActualMinutes,
                //                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*pp.ProductionHours*60) AS ActualEfficiency
                //							--,PSV.UserName Parameter,psv.[Value] ParameterValue
                //							--,isnull(MMT.[Minute],0) DetentionInMin
                //,0 Utilization,pp.ProductionOrderId PORefNo,pp.AddedBy EntryBy,UOM.Code UOM,pp.Quantity ProductionQty,pp.Remarks

                //                            FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,COUNT(*) AS ProductionHours,ps.Quantity
                //									,PS.AddedBy,ps.Remarks,ps.ProductLibraryId
                //                                    FROM trn.ProductionSummary AS ps 
                //                                  left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
                //                                  LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
                //      		                      WHERE ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND ps.EntityID in (" + EntityId + @")  and ps.ProcessId in (" + wp + @")
                //                                  GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,PS.AddedBy,ps.Remarks,ps.ProductLibraryId,ps.Quantity
                //                            ) AS pp
                //							--left join MachineMasterTransaction MMT on MMT.ProcessId=pp.ProcessId and MMT.ShiftId=pp.ProductionShiftId and MMT.WorkCenterId=pp.WorkCenterMasterId and MMT.[Date]=pp.ProductionDate
                //                            LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
                //							left join ProductLibrary PL on PL.Id=pp.ProductLibraryId
                //                            LEFT JOIN trn.SalesOrder AS so ON so.Id=pp.SalesOrderId
                //							left join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
                //							left join SCS.UnitOfMeasurement UOM on UOM.Id=MOI.UOMId
                //                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
                //                            LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=pp.ProductionOrderId AND sn.ProductionDate=pp.ProductionDate AND sn.WorkCenterMasterId=pp.WorkCenterMasterId AND sn.EntityID=pp.EntityId
                //                            LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
                //                            LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=pp.FromSFGInventoryId
                //                            --LEFT OUTER JOIN dbo.ProductionSummaryParameterValue AS psv ON psv.ProductionSummaryId=pp.Id
                //                            LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id=pp.ToWorkCenterMasterId
                //                            LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id=pp.ToSFGInventoryId

                //                            left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=pp.ProductionOrderId AND ppt.WorkCenterMasterId=PP.WorkCenterMasterId AND  ppt.ProcessId=PP.ProcessId AND ppt.EntityId=pp.EntityId and ppt.ProductionDate=PP.ProductionDate
                //                            left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
                //							LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
                //							LEFT OUTER JOIN hkp.Process AS Tp ON Tp.Id=pp.ToProcessId
                //                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
                //                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId

                //                                          	)A ORDER BY A.ActualDate, A.WorkCenterMasterId, A.ProductionOrderID";


                string sql = @"Select A.* from(
                Select ps.Id,trkp.UserName AS Plant,trke.UserName AS Entity,PS.EntityID,PS.WorkCenterMasterId, PS.ProductionOrderID,wcm.Code AS WorkCenter,FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ActualDate,PS.Quantity AS ActualQty,ORD.CM*PS.Quantity AS ActualCM
                ,pt1.SPT AS SAM,ps.ProcessId,isnull(p.UserName,FSFG.UserName) AS Process,isnull(Tp.UserName,TSFG.UserName) AS ToProcess,Twcm.UserName AS ToWorkCenter,mm.UserName AS Material,MA.StandardName AS Article,PL.Code ProductCode
                ,PM.UserName AS Product,PC.UserName AS ProductCategory,Format(SN.AddedDate,'dd-MMM-yyyy') AS SnapshotDate,
                sn.Quantity AS PlanQty,ORD.CM*sn.Quantity AS PlanCM,ORD.CM,CPL.[Username] AS ProductionShift,so.Id AS SalesOrderIdBooking
                ,wcm.StandardTimePerDay AS StandardWorkingHours, wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
                PS.ProductionHours AS WorkingHours,SN.isBuildUp,
                pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
                --additional info
                buyer=STUFF((select distinct ','+XB.UserName from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                SalesOrderIds=STUFF((select distinct ','+XSO.Id from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                SalesOrderDesc=STUFF((select distinct ','+XSO.Description from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                MasterOrderNo=STUFF((select distinct ','+XMO.Id from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                pt1.NoOfWorkStation,sn.ProductionHours AS PlanHours,
                ISNULL(ppt.ProductionHours,0) ProductionHours,
                ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0) AS PlanMinutes,
                ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*sn.ProductionHours*60) AS PlanEfficiency,

                ISNULL(PS.Quantity,0)*isnull(pt1.SPT,0) AS ActualMinutes,
                ISNULL(PS.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*PS.ProductionHours*60) AS ActualEfficiency 
                ,0 Utilization,ps.ProductionOrderId PORefNo,ps.AddedBy EntryBy,ORD.UOM,ps.Quantity ProductionQty,ps.Remarks,so.[Description] AS SalesOrderDescBooking
                from (
                SELECT ps.Id,ps.ProcessId,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId
                ,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,ps.MasterOrderItemId,ps.ProductLibraryId,ps.AddedBy,ps.Remarks,COUNT(*) AS ProductionHours,SUM(ps.Quantity) AS Quantity
                FROM trn.ProductionSummary AS ps
                Where PS.MasterOrderItemId IS NOT NULL AND ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND ps.EntityID in (" + EntityId + @")  and ps.ProcessId in (" + wp + @")
                GROUP BY ps.Id,ps.ProcessId,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId, ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId
                ,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,ps.MasterOrderItemId,ps.ProductLibraryId,ps.AddedBy,ps.Remarks

                ) AS PS
                left join ProductLibrary PL on PL.Id=pS.ProductLibraryId
                left join TRN.MasterOrderItem MOI ON MOI.Id=PS.MasterOrderItemId
                left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                 LEFT JOIN trn.SalesOrder AS so ON so.Id=ps.SalesOrderId
                LEFT JOIN(Select (SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty))CM,SO.MasterOrderItemId,UOM.Code UOM
                from  trn.SalesOrder AS so
                left join TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
                left join SCS.UnitOfMeasurement UOM on UOM.Id=MOI.UOMId
                left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
                left JOIN org.Company AS com ON com.Id=mo.CompanyId
                LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e)
                LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e)
                Group By SO.MasterOrderItemId,UOM.Code
                ) ORD  ON ORD.MasterOrderItemId=MOI.Id

                left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                LEFT JOIN ShiftDefination CPL ON cpl.SystemId=PS.ProductionShiftId
                LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=PS.ProductionOrderID
                LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=PS.ProductionOrderId AND sn.ProductionDate=PS.ProductionDate AND sn.WorkCenterMasterId=PS.WorkCenterMasterId AND sn.EntityID=PS.EntityId
                LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=PS.WorkCenterMasterId
                LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=PS.FromSFGInventoryId 
                LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id=PS.ToWorkCenterMasterId
                LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id=PS.ToSFGInventoryId 
                left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=PS.ProductionOrderId AND ppt.WorkCenterMasterId=PS.WorkCenterMasterId AND ppt.ProcessId=PS.ProcessId AND ppt.EntityId=PS.EntityId and ppt.ProductionDate=PS.ProductionDate
                left outer join TRN.ProductionOrder PO ON PO.Id=PS.ProductionOrderID
                LEFT OUTER JOIN hkp.Process AS p ON p.Id=PS.ProcessId
                LEFT OUTER JOIN hkp.Process AS Tp ON Tp.Id=PS.ToProcessId
                LEFT OUTER JOIN ORG.Entity AS TRKE ON trke.Id = PS.EntityId
                LEFT OUTER JOIN org.Plant AS TRKP ON trkp.Id = TRKE.PlantId
                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId

                UNION ALL

                Select ps.Id,trkp.UserName AS Plant,trke.UserName AS Entity,PS.EntityID,PS.WorkCenterMasterId, PS.ProductionOrderID,wcm.Code AS WorkCenter,FORMAT(PS.ProductionDate,'dd-MMM-yyyy') AS ActualDate,PS.Quantity AS ActualQty,ORD.CM*PS.Quantity AS ActualCM,pt1.SPT AS SAM
                ,ps.ProcessId,isnull(p.UserName,FSFG.UserName) AS Process,isnull(Tp.UserName,TSFG.UserName) AS ToProcess,Twcm.UserName AS ToWorkCenter
                ,Material=STUFF((select distinct ','+mm.UserName from mst.MaterialMaster mm
                left outer join trn.MasterOrderItem XMOI on Xmoi.MaterialMasterId=mm.Id
                LEFT JOIN trn.SalesOrder XSO ON XSO.MasterOrderItemId=Xmoi.Id
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '') 

                ,Article=STUFF((select distinct ','+mm.StandardName from mst.MaterialMasterArticle mm
                left outer join trn.MasterOrderItem XMOI on Xmoi.ArticleId=mm.Id
                LEFT JOIN trn.SalesOrder XSO ON XSO.MasterOrderItemId=Xmoi.Id
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                ,PL.Code ProductCode


                ,Product=STUFF((select distinct ','+PM.UserName from mst.MaterialMaster mm
                left outer join trn.MasterOrderItem XMOI on Xmoi.ArticleId=mm.Id
                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                LEFT JOIN trn.SalesOrder XSO ON XSO.MasterOrderItemId=Xmoi.Id
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                ,ProductCategory=STUFF((select distinct ','+PC.UserName from mst.MaterialMaster mm
                left outer join trn.MasterOrderItem XMOI on Xmoi.ArticleId=mm.Id
                left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                LEFT JOIN trn.SalesOrder XSO ON XSO.MasterOrderItemId=Xmoi.Id
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                ,Format(SN.AddedDate,'dd-MMM-yyyy') AS SnapshotDate,sn.Quantity AS PlanQty,ORD.CM*sn.Quantity AS PlanCM,ORD.CM

                ,CPL.[Username] AS ProductionShift,so.Id AS SalesOrderIdBooking
                --,so.Id AS SalesOrderIdBooking,so.[Description] AS SalesOrderDescBooking,
                ,wcm.StandardTimePerDay AS StandardWorkingHours, wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
                PS.ProductionHours AS WorkingHours,SN.isBuildUp,
                pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
                --additional info
                buyer=STUFF((select distinct ','+XB.UserName from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                SalesOrderIds=STUFF((select distinct ','+XSO.Id from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                SalesOrderDesc=STUFF((select distinct ','+XSO.Description from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                MasterOrderNo=STUFF((select distinct ','+XMO.Id from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from
                trn.SalesOrder XSO
                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
                where PS.ProductionOrderID=Xpod.ProductionOrderId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                pt1.NoOfWorkStation,sn.ProductionHours AS PlanHours,
                ISNULL(ppt.ProductionHours,0) ProductionHours,
                ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0) AS PlanMinutes,
                ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*sn.ProductionHours*60) AS PlanEfficiency,

                ISNULL(PS.Quantity,0)*isnull(pt1.SPT,0) AS ActualMinutes,
                ISNULL(PS.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*PS.ProductionHours*60) AS ActualEfficiency 
                ,0 Utilization,ps.ProductionOrderId PORefNo,ps.AddedBy EntryBy,ORD.UOM,ps.Quantity ProductionQty,ps.Remarks,so.[Description] AS SalesOrderDescBooking
                from (
                SELECT ps.Id,ps.ProcessId,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId
                ,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,ps.MasterOrderItemId,pS.ProductLibraryId,ps.AddedBy,ps.Remarks,COUNT(*) AS ProductionHours,SUM(ps.Quantity) AS Quantity
                FROM trn.ProductionSummary AS ps
                Where PS.MasterOrderItemId IS NULL AND ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND ps.EntityID in (" + EntityId + @")  and ps.ProcessId in (" + wp + @")
                GROUP BY ps.Id,ps.ProcessId,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId, ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId
                ,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,ps.MasterOrderItemId,pS.ProductLibraryId,ps.AddedBy,ps.Remarks
                ) AS PS
                left join ProductLibrary PL on PL.Id=pS.ProductLibraryId
                LEFT JOIN trn.SalesOrder AS so ON so.Id=ps.SalesOrderId
                LEFT JOIN(
                Select (SUM(CASE WHEN SAME.FromCurrencyId=mo.CurrencyId THEN SO.CM* so.Qty ELSE so.CM* so.Qty * isnull(RT.ExchangeRate,1) *isnull(RER.ExchangeRate,1) END)/SUM(SO.Qty))CM,POD.ProductionOrderId,UOM.Code UOM
                from  trn.SalesOrder AS so
                LEFT JOIN trn.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
                left join TRN.MasterOrderItem MOI ON MOI.Id=SO.MasterOrderItemId
                left join SCS.UnitOfMeasurement UOM on UOM.Id=MOI.UOMId
                left outer join trn.MasterOrder MO on mo.Id=moi.MasterOrderId
                left join MasterOrderExchangeRates RT ON RT.TransactionId=MO.Id
                left JOIN org.Company AS com ON com.Id=mo.CompanyId
                LEFT JOIN ReportExchangeRates AS rer ON rer.FromCurrencyId=COM.BaseCurrencyId AND rer.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e)
                LEFT JOIN ReportExchangeRates AS SAME ON SAME.FromCurrencyId=SAME.ToCurrencyId AND SAME.PlantId=(SELECT top 1 PlantId FROM org.Entity AS e)
                Group By POD.ProductionOrderId,UOM.Code
                ) ORD  ON ORD.ProductionOrderId=PS.ProductionOrderId

                LEFT JOIN ShiftDefination CPL ON cpl.SystemId=PS.ProductionShiftId
                LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=PS.ProductionOrderID
                LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=PS.ProductionOrderId AND sn.ProductionDate=PS.ProductionDate AND sn.WorkCenterMasterId=PS.WorkCenterMasterId AND sn.EntityID=PS.EntityId
                LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=PS.WorkCenterMasterId
                LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=PS.FromSFGInventoryId 
                LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id=PS.ToWorkCenterMasterId
                LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id=PS.ToSFGInventoryId 
                left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=PS.ProductionOrderId AND ppt.WorkCenterMasterId=PS.WorkCenterMasterId AND ppt.ProcessId=PS.ProcessId AND ppt.EntityId=PS.EntityId and ppt.ProductionDate=PS.ProductionDate
                left outer join TRN.ProductionOrder PO ON PO.Id=PS.ProductionOrderID
                LEFT OUTER JOIN hkp.Process AS p ON p.Id=PS.ProcessId
                LEFT OUTER JOIN hkp.Process AS Tp ON Tp.Id=PS.ToProcessId
                LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PS.EntityId
                LEFT OUTER JOIN org.Plant AS TRKP ON trkp.Id = TRKE.PlantId

                ) A ORDER BY A.ActualDate, A.WorkCenterMasterId, A.ProductionOrderID";

                dtOrder = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ItemReportSQL(string fromDate, string toDate, string EntityId, string ProcessId, out DataTable dtOrder)
        {
            try
            {
                string wp = "";
                if (string.IsNullOrEmpty(ProcessId) || ProcessId == "''")
                {
                    wp = @"SELECT DISTINCT P.Id AS [Value] FROM HKP.EntityProcessTag AS EP
                            JOIN HKP.Process AS P ON EP.ProcessId = P.Id
                            where EP.EntityId in (" + EntityId + @") AND P.Active = 1";
                }
                else
                {
                    wp = ProcessId;
                }

                string sql = @"SELECT A.* from (SELECT distinct PP.Id, trkp.UserName AS Plant,trke.UserName AS Entity,pp.EntityID,pp.WorkCenterMasterId, PP.ProductionOrderID,M.Id ItemId,wcm.UserName AS WorkCenter,FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty
--,ORD.CM*pp.Quantity AS ActualCM,
,(select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
							where PO.Id=PoD.ProductionOrderId)*pp.Quantity AS ActualCM
                            ,pt1.SPT AS SAM,pp.ProcessId,isnull(p.UserName,FSFG.UserName) AS Process,isnull(Tp.UserName,TSFG.UserName) AS ToProcess,Twcm.UserName AS ToWorkCenter
							,Material=STUFF((select distinct ','+MA.UserName from
											MST.MaterialMaster MA
											left join TRN.MasterOrderItem moi on moi.MaterialMasterId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											where M.Id=moi.Id for xml path('') ), 1, 1, '')
							,Article=STUFF((select distinct ','+MA.StandardName from
											MST.MaterialMasterArticle MA
											left join TRN.MasterOrderItem moi on moi.ArticleId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											where M.Id=moi.Id for xml path('') ), 1, 1, '')            
                           --,PL.Code ProductCode
                             ,ProductCode=STUFF((select distinct ','+PM.UserName from
											MST.MaterialMaster mm
											left join TRN.MasterOrderItem moi on moi.MaterialMasterId=mm.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
											where moi.Id=M.Id for xml path('') ), 1, 1, '')
						    ,Product=STUFF((select distinct ','+PM.UserName from
											MST.MaterialMaster mm
											left join TRN.MasterOrderItem moi on moi.MaterialMasterId=mm.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
											where M.Id=moi.Id for xml path('') ), 1, 1, '')						   
							,ProductCategory=STUFF((select distinct ','+pc.UserName from
								[HKP].[ProductCategory] PC
								left join [MST].[ProductMaster] PM on pc.Id=pm.ProductCategoryId
								left join trn.ProductDefinition AS pd ON pd.ProductMasterId=pm.Id
								left join mst.MaterialMaster mm on mm.id=pd.MaterialMasterId
								left join TRN.MasterOrderItem moi on moi.MaterialMasterId=MM.Id
								left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
								where M.Id=moi.Id for xml path('') ), 1, 1, '')
						   ,Format(SN.AddedDate,'dd-MMM-yyyy') AS SnapshotDate
                         ,sn.Quantity AS PlanQty,((select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
							where PO.Id=PoD.ProductionOrderId)*pp.Quantity)*sn.Quantity AS PlanCM
,(select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
							where PO.Id=PoD.ProductionOrderId) CM
                             ,CPL.UserName AS ProductionShift,so.Id AS SalesOrderIdBooking,CPL.ShiftDuration ShiftWorkingMin,so.[Description] AS SalesOrderDescBooking,
                             wcm.StandardTimePerDay AS StandardWorkingHours,  wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
                             PP.ProductionHours AS WorkingHours,SN.isBuildUp,
                             pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
                            --additional info
			                     buyer=STUFF((select distinct ','+XB.UserName from 
			                            trn.SalesOrder XSO 
			                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                            where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            SalesOrderIds=STUFF((select distinct ','+XSO.Id from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.MasterOrderItem AS Xpod ON Xpod.Id=Xso.MasterOrderItemId
			                        where M.Id=Xpod.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            Description=STUFF((select distinct ','+XSO.Description from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.MasterOrderItem AS Xpod ON Xpod.Id=Xso.MasterOrderItemId
			                        where M.Id=Xpod.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

	                        SalesOrderDesc=STUFF((select distinct ','+XSO.Description from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                        where M.Id=Xpod.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                   
                                  MasterOrderNo=STUFF((select distinct ','+XMO.Id from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                        BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

			
		                                StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
                                        OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            pt1.NoOfWorkStation,sn.ProductionHours  AS PlanHours,
                            ISNULL(ppt.ProductionHours,0) ProductionHours,
                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0) AS PlanMinutes,
                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*sn.ProductionHours*60) AS PlanEfficiency,

                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0) AS ActualMinutes,
                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*pp.ProductionHours*60) AS ActualEfficiency
							--,PSV.UserName Parameter,psv.[Value] ParameterValue
							--,isnull(MMT.[Minute],0) DetentionInMin
,0 Utilization,pp.ProductionOrderId PORefNo,pp.AddedBy EntryBy,UOM.Code UOM,pp.Quantity ProductionQty,pp.Remarks

                            FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,COUNT(*) AS ProductionHours,ps.Quantity
									,PS.AddedBy,ps.Remarks,ps.ProductLibraryId,ps.MasterOrderItemId
                                    FROM trn.ProductionSummary AS ps 
                                  left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
                                  LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
      		                      WHERE ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND ps.EntityID in (" + EntityId + @")  and ps.ProcessId in (" + wp + @")
                                  GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,PS.AddedBy,ps.Remarks,ps.ProductLibraryId,ps.Quantity,ps.MasterOrderItemId
                            ) AS pp
							--left join MachineMasterTransaction MMT on MMT.ProcessId=pp.ProcessId and MMT.ShiftId=pp.ProductionShiftId and MMT.WorkCenterId=pp.WorkCenterMasterId and MMT.[Date]=pp.ProductionDate
                            LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
							left join ProductLibrary PL on PL.Id=pp.ProductLibraryId
                            LEFT JOIN trn.SalesOrder AS so ON so.Id=pp.SalesOrderId
							left join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
							left join SCS.UnitOfMeasurement UOM on UOM.Id=MOI.UOMId
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
                            LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=pp.ProductionOrderId AND sn.ProductionDate=pp.ProductionDate AND sn.WorkCenterMasterId=pp.WorkCenterMasterId AND sn.EntityID=pp.EntityId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=pp.FromSFGInventoryId
                            --LEFT OUTER JOIN dbo.ProductionSummaryParameterValue AS psv ON psv.ProductionSummaryId=pp.Id
                            LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id=pp.ToWorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id=pp.ToSFGInventoryId
                        
                            left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=pp.ProductionOrderId AND ppt.WorkCenterMasterId=PP.WorkCenterMasterId AND  ppt.ProcessId=PP.ProcessId AND ppt.EntityId=pp.EntityId and ppt.ProductionDate=PP.ProductionDate
                            left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
                            left join TRN.ProductionOrderDetail PD on PD.ProductionOrderId=PO.Id
							left join TRN.SalesOrder S on S.Id=PD.SalesOrderId
							left join TRN.MasterOrderItem M on M.Id=pp.MasterOrderItemId
							LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
							LEFT OUTER JOIN hkp.Process AS Tp ON Tp.Id=pp.ToProcessId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
                             
                                          	)A ORDER BY A.ActualDate, A.WorkCenterMasterId, A.ProductionOrderID,A.ItemID";
                dtOrder = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SOReportSQL(string fromDate, string toDate, string EntityId, string ProcessId, out DataTable dtOrder)
        {
            try
            {
                string wp = "";
                if (string.IsNullOrEmpty(ProcessId) || ProcessId == "''")
                {
                    wp = @"SELECT DISTINCT P.Id AS [Value] FROM HKP.EntityProcessTag AS EP
                            JOIN HKP.Process AS P ON EP.ProcessId = P.Id
                            where EP.EntityId in (" + EntityId + @") AND P.Active = 1";
                }
                else
                {
                    wp = ProcessId;
                }

                string sql = @"SELECT A.* from (SELECT distinct PP.Id, trkp.UserName AS Plant,trke.UserName AS Entity,pp.EntityID,pp.WorkCenterMasterId, PP.ProductionOrderID,M.Id ItemId,wcm.UserName AS WorkCenter,FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty
--,ORD.CM*pp.Quantity AS ActualCM,
,(select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
							where PO.Id=PoD.ProductionOrderId)*pp.Quantity AS ActualCM
                            ,pt1.SPT AS SAM,pp.ProcessId,isnull(p.UserName,FSFG.UserName) AS Process,isnull(Tp.UserName,TSFG.UserName) AS ToProcess,Twcm.UserName AS ToWorkCenter
							,Material=STUFF((select distinct ','+MA.UserName from
											MST.MaterialMaster MA
											left join TRN.MasterOrderItem moi on moi.MaterialMasterId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											where M.Id=moi.Id for xml path('') ), 1, 1, '')
							,Article=STUFF((select distinct ','+MA.StandardName from
											MST.MaterialMasterArticle MA
											left join TRN.MasterOrderItem moi on moi.ArticleId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											where M.Id=moi.Id for xml path('') ), 1, 1, '')            
                           --,PL.Code ProductCode
                             ,ProductCode=STUFF((select distinct ','+PM.UserName from
											MST.MaterialMaster mm
											left join TRN.MasterOrderItem moi on moi.MaterialMasterId=mm.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
											where moi.Id=M.Id for xml path('') ), 1, 1, '')
						    ,Product=STUFF((select distinct ','+PM.UserName from
											MST.MaterialMaster mm
											left join TRN.MasterOrderItem moi on moi.MaterialMasterId=mm.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
											where M.Id=moi.Id for xml path('') ), 1, 1, '')						   
							,ProductCategory=STUFF((select distinct ','+pc.UserName from
								[HKP].[ProductCategory] PC
								left join [MST].[ProductMaster] PM on pc.Id=pm.ProductCategoryId
								left join trn.ProductDefinition AS pd ON pd.ProductMasterId=pm.Id
								left join mst.MaterialMaster mm on mm.id=pd.MaterialMasterId
								left join TRN.MasterOrderItem moi on moi.MaterialMasterId=MM.Id
								left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
								where M.Id=moi.Id for xml path('') ), 1, 1, '')
						   ,Format(SN.AddedDate,'dd-MMM-yyyy') AS SnapshotDate
                         ,sn.Quantity AS PlanQty,((select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
							where PO.Id=PoD.ProductionOrderId)*pp.Quantity)*sn.Quantity AS PlanCM
,(select SUM(xp.Qty*xp.CM)/sum(xp.Qty) 
							from TRN.SalesOrder AS xp INNER JOIN TRN.ProductionOrderDetail POD ON pod.SalesOrderId=xp.id
							where PO.Id=PoD.ProductionOrderId) CM
                             ,CPL.UserName AS ProductionShift,so.Id AS SalesOrderIdBooking,CPL.ShiftDuration ShiftWorkingMin,so.[Description] AS SalesOrderDescBooking,
                             wcm.StandardTimePerDay AS StandardWorkingHours,  wcm.NoOfWorkStation AS StandardWorkStations,wcm.DailyFixedCost,wcm.VariableCost AS VariableCostPerHour,
                             PP.ProductionHours AS WorkingHours,SN.isBuildUp,
                             pt1.TargetPerDay AS LineTargetPerDay,PT1.TargetPerHour AS PlanTargetPerHour,PT1.PlanWorkingHoursPerDay,
                            --additional info
			                     buyer=STUFF((select distinct ','+XB.UserName from 
			                            trn.SalesOrder XSO 
			                            JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                            left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                            left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                            left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                            where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                             S.Id SalesOrderIds,
                            --SalesOrderIds=STUFF((select distinct ','+XSO.Id from 
			                        --trn.SalesOrder XSO 
			                        --JOIN trn.MasterOrderItem AS Xpod ON Xpod.Id=Xso.MasterOrderItemId
			                        --where M.Id=Xpod.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            Description=STUFF((select distinct ','+XSO.Description from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.MasterOrderItem AS Xpod ON Xpod.Id=Xso.MasterOrderItemId
			                        where M.Id=Xpod.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                               
	                        SalesOrderDesc=STUFF((select distinct ','+XSO.Description from 
			                        trn.SalesOrder XSO 
			                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                        where M.Id=Xpod.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
			                   
                                  MasterOrderNo=STUFF((select distinct ','+XMO.Id from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                        BuyerOrderNo=STUFF((select distinct ','+XMO.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                                     OwnOrderNo=STUFF((select distinct ','+XMO.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
			                                left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
			                                where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

			
		                                StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
       
                                        OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
			                                trn.SalesOrder XSO 
			                                JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
			                                left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId                                           
			                                where M.Id=XMOI.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            pt1.NoOfWorkStation,sn.ProductionHours  AS PlanHours,
                            ISNULL(ppt.ProductionHours,0) ProductionHours,
                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0) AS PlanMinutes,
                            ISNULL(sn.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*sn.ProductionHours*60) AS PlanEfficiency,

                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0) AS ActualMinutes,
                            ISNULL(pp.Quantity,0)*isnull(pt1.SPT,0)/(pt1.NoOfWorkStation*pp.ProductionHours*60) AS ActualEfficiency
							--,PSV.UserName Parameter,psv.[Value] ParameterValue
							--,isnull(MMT.[Minute],0) DetentionInMin
,0 Utilization,pp.ProductionOrderId PORefNo,pp.AddedBy EntryBy,UOM.Code UOM,pp.Quantity ProductionQty,pp.Remarks

                            FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,COUNT(*) AS ProductionHours,ps.Quantity
									,PS.AddedBy,ps.Remarks,ps.ProductLibraryId,ps.MasterOrderItemId
                                    FROM trn.ProductionSummary AS ps 
                                  left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
                                  LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
      		                      WHERE ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND ps.EntityID in (" + EntityId + @")  and ps.ProcessId in (" + wp + @")
                                  GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,PS.AddedBy,ps.Remarks,ps.ProductLibraryId,ps.Quantity,ps.MasterOrderItemId
                            ) AS pp
							--left join MachineMasterTransaction MMT on MMT.ProcessId=pp.ProcessId and MMT.ShiftId=pp.ProductionShiftId and MMT.WorkCenterId=pp.WorkCenterMasterId and MMT.[Date]=pp.ProductionDate
                            LEFT JOIN dbo.ShiftDefination CPL ON cpl.SystemId=pp.ProductionShiftId
							left join ProductLibrary PL on PL.Id=pp.ProductLibraryId
                            LEFT JOIN trn.SalesOrder AS so ON so.Id=pp.SalesOrderId
							left join trn.MasterOrderItem MOI on MOI.Id=so.MasterOrderItemId
							left join SCS.UnitOfMeasurement UOM on UOM.Id=MOI.UOMId
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS PT1 ON pt1.ProductionOrderID=pp.ProductionOrderID
                            LEFT OUTER JOIN ProductionPlanningSnapshot2Type1 AS SN ON sn.ProductionOrderID=pp.ProductionOrderId AND sn.ProductionDate=pp.ProductionDate AND sn.WorkCenterMasterId=pp.WorkCenterMasterId AND sn.EntityID=pp.EntityId
                            LEFT OUTER JOIN scs.WorkCenterMaster AS wcm ON wcm.Id=pp.WorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS FSFG ON FSFG.Id=pp.FromSFGInventoryId
                            --LEFT OUTER JOIN dbo.ProductionSummaryParameterValue AS psv ON psv.ProductionSummaryId=pp.Id
                            LEFT OUTER JOIN scs.WorkCenterMaster AS Twcm ON Twcm.Id=pp.ToWorkCenterMasterId
                            LEFT OUTER JOIN hkp.SFGInventory AS TSFG ON TSFG.Id=pp.ToSFGInventoryId
                        
                            left outer join ProductionPlanningType1 AS ppt on ppt.ProductionOrderID=pp.ProductionOrderId AND ppt.WorkCenterMasterId=PP.WorkCenterMasterId AND  ppt.ProcessId=PP.ProcessId AND ppt.EntityId=pp.EntityId and ppt.ProductionDate=PP.ProductionDate
                            left outer join TRN.ProductionOrder PO ON PO.Id=PP.ProductionOrderID
                            left join TRN.ProductionOrderDetail PD on PD.ProductionOrderId=PO.Id
							left join TRN.SalesOrder S on S.Id=pp.SalesOrderId
							left join TRN.MasterOrderItem M on M.Id=pp.MasterOrderItemId
							LEFT OUTER JOIN hkp.Process AS p ON p.Id=pp.ProcessId
							LEFT OUTER JOIN hkp.Process AS Tp ON Tp.Id=pp.ToProcessId
                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PP.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId
                             
                                          	)A ORDER BY A.ActualDate, A.WorkCenterMasterId, A.ProductionOrderID,A.SalesOrderIds";
                dtOrder = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetPOWiseSql(Dictionary<string, string> parameters)
        {
            try
            {
                var str = @"DECLARE @POCreationDate varchar(100)=DATEADD(day,-180,GETDate())
                            SELECT x.ProcessIndex,X.EntityId,X.Entity,X.Customer,X.Article,X.SONo,X.POId PONo,X.POStatusId,X.POStatus,X.AddedBy,X.AddedDate,X.UpdatedBy,X.UpdatedDate,X.SOQty,X.BaseProcPlanPercentage,ISNULL(X.ActualPlanScheduleQty,0)ActualPlanScheduleQty,ISNULL(X.ShouldBeBaseProcessPlannedQty,0)ShouldBeBaseProcessPlannedQty
                            ,X.BaseProcessProduceQty,ISNULL(X.BaseProcessRemainingQty,0)BaseProcessRemainingQty,X.Sequence,X.ProcessId,X.Process,X.PercentQty,ISNULL(X.ProcessPlannedQty,0)ProcessPlannedQty,X.ProcProdQty,X.PreProcProdQty,X.WIP,X.ProcBalanceToProduce,X.RelayProcess,X.IsBaseProcess
                            ,X.ProcessLegDays,X.POFirstDelivery,X.POLastDelivery,ISNULL(X.BaseProcProdStartDate,'-')BaseProcProdStartDate,ISNULL(X.BaseProcLatestProdDate,'-')BaseProcLatestProdDate,X.BaseProcPlanStartDate,X.BaseProcPlanCompletionDate
                            ,ISNULL(X.POStartDate,'-')POStartDate,ISNULL(X.POCompletionDate,'-')POCompletionDate,ISNULL(X.FirstProcessActualBookDate,'-')FirstProcessActualBookDate,ISNULL(X.POFirstProdBookDate,'-')POFirstProdBookDate,ISNULL(X.POLatestProdBookDate,'-')POLatestProdBookDate,ISNULL(X.ShouldBeProcessStartDate,'-')ShouldBeProcessStartDate,ISNULL(X.ShouldBeProcessEndDate,'-')ShouldBeProcessEndDate
                            ,ISNULL(X.ProcessFirstBookDate,'-')ProcessFirstBookDate,ISNULL(X.ProcessLatestBookDate,'-')ProcessLatestBookDate,ISNULL(X.ProcessStartDays,'-')ProcessStartDays,ISNULL(X.ProcessEndDays,0)ProcessEndDays,X.ProcessPlanPercent,X.ProcessStatus,X.FirstProcessWC,X.ProcLossPercent,X.ProcLossQty,X.BaseProcProdPerenct
                            ,ROUND(X.ProcProdPercent*100,0)ProcProdPercent,X.EntryCheck,ROUND(X.ProceessProdQtyVsSOQty*100,0)ProceessProdQtyVsSOQty,ISNULL(X.Remarks,'-') ProcessStatusRemark--,X.ProcessProdBookDate
                            ,POReviewStatus=CASE WHEN CONVERT(datetime,X.ProcessLatestBookDate)< (GETDATE()-2) THEN 'To Review' ELSE X.POStatus END
							,X.FirstProcessProQty,ISNULL(X.RequestedQty,0)RequestedQty,ISNULL(X.IssueQty,0)IssueQty,ISNULL(X.TotalQty,0)TotalQty
                            ,LotNoQty=ISNULL(STUFF((select distinct ', '+xp.LotNumber+'-'+CONVERT(varchar(100),X.ProcProdQty) from
                            TRN.ProductionSummary AS xp
                            where X.POId=xp.ProductionOrderId for xml path('') ), 1, 1, ''),'-')
                            ,ISNULL(X.InputRecoveryPercentage,0)InputRecoveryPercentage,ActualInputPlanPercentage=ISNULL(ROUND((X.FirstProcessProQty/NULLIF(X.ActualPlanScheduleQty,0))*100,0),0)
                            ,LatestProcessProdBookDays=CASE WHEN DATEDIFF(day,X.ProcessLatestBookDate,GETDATE()) IS NULL THEN 'Entry Missing' ELSE CONVERT(Varchar(100),DATEDIFF(day,X.ProcessLatestBookDate,GETDATE())) END
                            ,ProcessReviewStatus=CASE WHEN DATEDIFF(day,X.ProcessLatestBookDate,GETDATE())>2 THEN 'To Review' ELSE  'NA' END,ProcessBalanceProd=ISNULL(X.ProcessPlannedQty-X.ProcProdQty,0)
                            FROM(
                            SELECT 
                            T1.*,ISNULL(T2.ProcProdQty,0) PreProcProdQty,WIP=case when T1.Sequence=1 then 0 else ISNULL(ISNULL(T2.ProcProdQty,0)-ISNULL(T1.ProcProdQty,0),0) end, ProcLossPercent=ISNULL(t2.PercentQty-t1.PercentQty,0)
                            ,ProcLossQty=ISNULL(T2.ProcessPlannedQty-T1.ProcessPlannedQty,0),BaseProcProdPerenct=ISNULL(t2.BaseProcessProduceQty/NULLIF(t2.BaseProcessPlannedQty,0),0)
                            ,ProcProdPercent=ISNULL(T1.ProcProdQty/NULLIF(t1.ProcessPlannedQty,0),0)
                            ,EntryCheck=CASE WHEN T2.ProcProdQty-T1.ProcProdQty<0 THEN 'ToCheck' ELSE '' END
                            ,ProceessProdQtyVsSOQty=COALESCE(T1.ProcProdQty / NULLIF(T1.SOQty ,0), 0)
                            FROM
                            (Select ROW_NUMBER() OVER(partition by A.POId ORDER BY A.Sequence) ProcessIndex,A.*
                            from (select E.Id EntityId,E.UserName Entity,P.Id POId,PRS.Id POStatusId,PRS.UserName POStatus,P.AddedBy,Format(P.AddedDate,'dd-MMM-yyyy')AddedDate,P.UpdatedBy,Format(P.UpdatedDate,'dd-MMM-yyyy')UpdatedDate
                            --,SOQty=P.Qty*PSQ.Qty/100
                            ,SOQty=(select SUM(xp.Qty) from trn.SalesOrder AS xp
                                INNER JOIN TRN.ProductionOrderDetail PD ON pd.SalesOrderId=xp.id
                                where P.Id=PD.ProductionOrderId)
                           
                            ,BaseProcPlanPercentage=(Select Qty from TRN.ProductionOrderProcessSet Where IsBaseProcess=1 AND ProductionOrderId=P.id)
                            ,ActualPlanScheduleQty=PQ.Qty
                            ,(PQ.Qty*(Select Qty from TRN.ProductionOrderProcessSet Where IsBaseProcess=1 AND ProductionOrderId=P.id)/100) ShouldBeBaseProcessPlannedQty
                            ,ISNULL(PS.Quantity,0) BaseProcessProduceQty
                            ,ISNULL(FPSQ.Quantity,0) FirstProcessProQty
                            ,PQ.Qty-ISNULL(PS.Quantity,0) BaseProcessRemainingQty
                            ,PSQ.Sequence,PRO.Id ProcessId,PRO.UserName Process
                            ,PSQ.Qty PercentQty
                            ,ProcessPlannedQty=(CASE WHEN PSQ.IsBaseProcess=1 THEN PQ.Qty ELSE PQ.Qty*PSQ.Qty/100 END)
                            ,ISNULL(PBQ.ProcProdQty,0) ProcProdQty
                            ,ProcBalanceToProduce=ISNULL((CASE WHEN PSQ.IsBaseProcess=1 THEN PQ.Qty ELSE PQ.Qty*PSQ.Qty/100 END)-PBQ.ProcProdQty,0)
                            ,RelayProcess=CASE WHEN PSQ.IsCompleted=1 THEN 'Yes' ELSE 'No' End
                            ,PSQ.IsBaseProcess,PSQ.Remarks
                            ,ProcessLegDays= CASE WHEN PSQ.Symbol='+' THEN CONVERT(varchar(100),PSQ.Days) ELSE ISNULL((PSQ.Symbol+''+CONVERT(varchar(100),PSQ.Days)),0) END
                            ,FORMAT(POD.POFirstDelivery,'dd-MMM-yyyy')POFirstDelivery,FORMAT(POD.POLastDelivery,'dd-MMM-yyyy')POLastDelivery
                            ,FORMAT(BASEP.BaseProcProdStartDate,'dd-MMM-yyyy')BaseProcProdStartDate,FORMAT(BASEP.BaseProcLatestProdDate,'dd-MMM-yyyy')BaseProcLatestProdDate,ISNULL(FORMAT(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy'),'') BaseProcPlanStartDate,ISNULL(FORMAT(Type1.BaseProcPlanCompletionDate,'dd-MMM-yyyy'),'')BaseProcPlanCompletionDate

                            ,POStartDate=FORMAT(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  < Type1.BaseProcPlanStartDate  then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end,'dd-MMM-yyyy')

                            ,POCompletionDate=FORMAT((case when Type1.BaseProcPlanCompletionDate is null or BASEP.BaseProcLatestProdDate  > Type1.BaseProcPlanCompletionDate  then BASEP.BaseProcLatestProdDate else Type1.BaseProcPlanCompletionDate end),'dd-MMM-yyyy')

                            ,FirstProcessActualBookDate=FORMAT(BASEP.BaseProcProdStartDate,'dd-MMM-yyyy')

                            ,FORMAT(FBPPD.POFirstProdBookDate,'dd-MMM-yyyy')POFirstProdBookDate
                            ,FORMAT(FBPPD.POLatestProdBookDate,'dd-MMM-yyyy')POLatestProdBookDate

                            ,ShouldBeProcessStartDate=FORMAT(DATEADD(DAY,PSQ.Days
                            ,(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  < Type1.BaseProcPlanStartDate  then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end)),'dd-MMM-yyyy')

                            ,ShouldBeProcessEndDate=FORMAT(DATEADD(DAY,PSQ.Days
                            ,(case when Type1.BaseProcPlanCompletionDate is null or BASEP.BaseProcLatestProdDate  > Type1.BaseProcPlanCompletionDate  then BASEP.BaseProcLatestProdDate else Type1.BaseProcPlanCompletionDate end)),'dd-MMM-yyyy')

                            ,FORMAT(PBQ.ProcessFirstBookDate,'dd-MMM-yyyy')ProcessFirstBookDate,FORMAT(PBQ.ProcessLatestBookDate,'dd-MMM-yyyy')ProcessLatestBookDate
                            ,ProcessStartDays=DateDiff(Day,PBQ.ProcessFirstBookDate,GETDate())
                            ,ProcessEndDays=DateDiff(Day,PBQ.ProcessLatestBookDate,GETDate())

                            ,ProcessPlanPercent=PSQ.Qty

                            ,ProcessStatus= CASE WHEN PBQ.ProcProdQty>=ISNULL(CASE WHEN ISNULL(PSQ.Qty,0)=0 THEN ISNULL(PQ.Qty,P.PlannedQty) ELSE P.PlannedQty*PSQ.Qty/100 END,0) THEN 'Complete'
					                            WHEN PBQ.ProcProdQty=0 THEN 'To Start' WHEN PBQ.ProcProdQty>0 THEN 'Running' ELSE 'To Check' END
                            ,FirstProcessWC=ISNULL(STUFF((select distinct ','+xw.UserName from
                            ProductionOrderFirstProcessWorkCenter AS xp
                            INNER JOIN scs.WorkCenterMaster AS xw ON xp.WorkCenterMasterId=xw.Id
                            where P.Id=xp.ProductionOrderId for xml path('') ), 1, 1, ''),'')

                            ,InputRecoveryPercentage=STUFF((select distinct ', '+CONVERT(Varchar(100),xp.PlanPercentage) from
                            dbo.MaterialIssueControlMaster AS xp
                            where P.Id=xp.POId for xml path('') ), 1, 1, '')

                            ,Customer=STUFF((select distinct ','+XP.UserName from 
		                                      trn.SalesOrder XSO 
		                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                      left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                      left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
		                                      where P.Id=Xpod.ProductionOrderId	and Xp.Id in(" + parameters["CustomerId"] + @") for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,Article=STUFF((select distinct ','+XMO.StandardName from 
		                                      trn.SalesOrder XSO 
		                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                      left outer join MST.MaterialMasterArticle XMO on Xmo.Id=Xmoi.ArticleId
		                                      where P.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
							,SONo=STUFF((select distinct ','+XSO.Id from 
		                                      trn.SalesOrder XSO 
		                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id		                                      
		                                      where P.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            ,IC.RequestedQty,IC.IssueQty,IC.TotalQty
                            FROM TRN.ProductionOrder P
                            Left JOIN ORG.Entity E ON E.Id=P.EntityId
                            LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id
                            LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID = P.Id
                            LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
                            LEFT JOIN HKP.Process PRO ON PRO.Id=PSQ.ProcessId

                            LEFT JOIN (Select MIN(SO.DeliveryDate) POFirstDelivery,MAX(SO.DeliveryDate) POLastDelivery,PD.ProductionOrderId FROM TRN.SalesOrder SO
                            LEFT JOIN TRN.ProductionOrderDetail PD ON PD.SalesOrderId=SO.Id GROUP BY PD.ProductionOrderId)POD ON POD.ProductionOrderId=P.Id
                            LEFT JOIN(Select MIN(ProductionDate)BaseProcProdStartDate,MAX(ProductionDate)BaseProcLatestProdDate,A.ProductionOrderId From TRN.ProductionSummary A
                            LEFT JOIN HKP.Process B ON B.Id=A.ProcessId
                            Group By A.ProductionOrderId) BASEP ON BASEP.ProductionOrderId=P.Id
                            LEFT JOIN(Select SUM(Quantity)ProQty,MIN(ProductionDate)POFirstProdBookDate,MAX(ProductionDate)POLatestProdBookDate,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=P.Id
                            LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanCompletionDate,ProductionOrderId From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=P.Id

                            LEFT JOIN(Select B.ProductionOrderId,SUM(Quantity)Quantity from TRN.ProductionSummary B
                            left join TRN.ProductionOrderProcessSet A ON A.ProductionOrderId=B.ProductionOrderId AND B.ProcessId=A.ProcessId Where A.IsBaseProcess=1 Group BY B.ProductionOrderId) PS ON P.Id=PS.ProductionOrderId
                            LEFT JOIN (Select SUM(Quantity)ProcProdQty, MIN(ProductionDate)ProcessFirstBookDate,MAX(ProductionDate)ProcessLatestBookDate ,ProductionOrderId,ProcessId from TRN.ProductionSummary Group BY ProductionOrderId,ProcessId) PBQ ON P.Id=PBQ.ProductionOrderId AND PBQ.ProcessId=PRO.Id
                            LEFT JOIN(Select B.ProductionOrderId,SUM(Quantity)Quantity from TRN.ProductionSummary B
                            LEFT JOIN TRN.ProductionOrderProcessSet A ON A.ProductionOrderId=B.ProductionOrderId AND B.ProcessId=A.ProcessId Where A.Sequence=1 Group BY B.ProductionOrderId) FPSQ ON P.Id=FPSQ.ProductionOrderId
                            LEFT JOIN(
							SELECT SUM(ID.RequestedQty) RequestedQty,SUM(ID.IssueQty)IssueQty,SUM(ID.TotalQty)TotalQty,IM.POId FROM dbo.InputConfirmationMaster IM
							LEFT JOIN dbo.InputConfirmationDetail ID ON ID.InputConfirmationMasterId=IM.Id
							Group BY IM.POId
							) IC ON IC.POId=P.Id                           

                            Where P.AddedDate>= @POCreationDate 
                            GROUP BY E.Id,E.UserName,P.Id,PSQ.Sequence,PRO.Id,PRO.UserName,P.PlannedQty,P.Qty,PSQ.Qty,PRS.Id,PRS.UserName,P.AddedBy,P.AddedDate,P.UpdatedBy,P.UpdatedDate,PQ.Qty,PBQ.ProcProdQty,PSQ.IsCompleted,PSQ.IsBaseProcess
                            ,PSQ.Days,PSQ.Symbol,POD.POFirstDelivery,POD.POLastDelivery,BASEP.BaseProcProdStartDate,BASEP.BaseProcLatestProdDate,BASEP.BaseProcLatestProdDate,Type1.BaseProcPlanStartDate
                            ,Type1.BaseProcPlanCompletionDate,Type1.BaseProcPlanStartDate,FBPPD.POFirstProdBookDate,FBPPD.POLatestProdBookDate,PBQ.ProcessFirstBookDate,PBQ.ProcessLatestBookDate,PS.Quantity,PSQ.Remarks,FPSQ.Quantity--,BPP.ProcessPlannedQty 
                            ,IC.RequestedQty,IC.IssueQty,IC.TotalQty
                            ) A )T1
                            LEFT JOIN (Select ROW_NUMBER() OVER(partition by A.POId ORDER BY A.Sequence)+1 ProcessIndex,A.*
                            from (select 

                            (PQ.Qty*(Select Qty from TRN.ProductionOrderProcessSet Where IsBaseProcess=1 AND ProductionOrderId=P.id)/100) BaseProcessPlannedQty
                            ,PS.Quantity BaseProcessProduceQty

                            ,PSQ.Qty PercentQty
                            ,ProcessPlannedQty=(CASE WHEN PSQ.IsBaseProcess=1 THEN PQ.Qty ELSE PQ.Qty*PSQ.Qty/100 END)
                            ,P.Id POId,PSQ.Sequence,PBQ.ProcProdQty

                            from TRN.ProductionOrder P
                            Left JOIN ORG.Entity E ON E.Id=P.EntityId
                            LEFT JOIN TRN.ProductionOrderProcessSet PSQ ON PSQ.ProductionOrderId=P.Id
                            LEFT JOIN ProductionOrderSchedulingParametersType1 PQ ON PQ.ProductionOrderID = P.Id
                            LEFT JOIN HKP.ProductionStatus PRS ON PRS.Id=P.ProductionStatusId
                            LEFT JOIN HKP.Process PRO ON PRO.Id=PSQ.ProcessId
                            LEFT JOIN(Select B.ProductionOrderId,SUM(Quantity)Quantity from TRN.ProductionSummary B
                            left join TRN.ProductionOrderProcessSet A ON A.ProductionOrderId=B.ProductionOrderId  AND B.ProcessId=A.ProcessId Where A.IsBaseProcess=1 Group BY B.ProductionOrderId ) PS ON P.Id=PS.ProductionOrderId
                            LEFT JOIN (Select SUM(Quantity)ProcProdQty,ProductionOrderId,ProcessId from TRN.ProductionSummary Group BY ProductionOrderId,ProcessId) PBQ ON P.Id=PBQ.ProductionOrderId AND PBQ.ProcessId=PRO.Id
                            Where P.AddedDate>= @POCreationDate 
                            GROUP BY P.Id,PSQ.Sequence,P.Qty,PSQ.Qty,PSQ.IsBaseProcess,PQ.Qty,PBQ.ProcProdQty,PS.Quantity 
                            ) A )T2 ON T1.ProcessIndex=T2.ProcessIndex AND  T1.POId=T2.POId

                            )X
                            --where x.poid=23468
                            

		                    Where X.EntityId in(" + parameters["EntityId"] + @")
			                     and X.POId in(" + parameters["ProductionOrderNo"] + @")
			                     and X.POStatusId in(" + parameters["ProductionStatusId"] + @")
			                     and X.ProcessId in(" + parameters["ProcessId"] + @")
                            Order By X.POId,X.ProcessIndex,X.Sequence,X.Process";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> POWisefiltersData()
        {
            try
            {
                var sql = @"SELECT distinct P.Id ProcessId,P.UserName Process,e.Id EntityId,isnull(e.UserName,'') Entity				
                                     ,ps.Id ProductionStatusId, isnull(ps.UserName,'') AS ProductionStatus		
		                             ,PO.Id ProductionOrderNo
                                  ,CustomerId=STUFF((select distinct ','+XP.Id from 
		                                      trn.SalesOrder XSO 
		                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                      left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                      left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
		                                          where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),   
                                     Customer=STUFF((select distinct ','+XP.UserName from 
		                                      trn.SalesOrder XSO 
		                                      JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                      left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                      left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                      left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
		                                          where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                               

                                     from trn.ProductionOrder PO
				                            left join TRN.ProductionOrderprocessset POS on POS.ProductionOrderId=PO.Id
				                            left join trn.ProductionOrderDetail POD ON POD.ProductionOrderId=PO.Id
		                                    left outer join org.Entity E on e.Id=PO.EntityID
		                                    LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
				                            left join HKP.Process P on P.Id=POS.ProcessId";
                                        return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string OnRolePrintReport(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Daily Planning & Production Report";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEmployeeCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEmployeeName = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 16;
                int colEmployeeCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Day Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDayStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "In Status";
                sheet[ROW, COL].ColumnWidth = 16;
                int colInStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "In Time";
                sheet[ROW, COL].ColumnWidth = 16;
                int colInTime = COL;
                COL++;

                sheet[ROW, COL].Text = "Out Time";
                sheet[ROW, COL].ColumnWidth = 16;
                int colOutTime = COL;
                COL++;

                sheet[ROW, COL].Text = "PV Out";
                sheet[ROW, COL].ColumnWidth = 41;
                int colPVOut = COL;
                COL++;

                sheet[ROW, COL].Text = "PV In";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPVIn = COL;
                COL++;

                sheet[ROW, COL].Text = "In Duration";
                sheet[ROW, COL].ColumnWidth = 16;
                int colInDuration = COL;
                COL++;

                sheet[ROW, COL].Text = "Out Duration";
                sheet[ROW, COL].ColumnWidth = 28;
                int colOutDuration = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colDesignation = COL;
                COL++;

                sheet[ROW, COL].Text = "Summary No";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSummaryId = COL;
                COL++;

                sheet[ROW, COL].Text = "Customer";
                sheet[ROW, COL].ColumnWidth = 16;
                int colCustomer = COL;
                COL++;

                sheet[ROW, COL].Text = "PO Article";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPOArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "Line Item Article";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLineItemArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "Line Item Product Code";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colLineItemProductCode = COL;
                COL++;

                sheet[ROW, COL].Text = "SO No";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSONo = COL;
                COL++;

                sheet[ROW, COL].Text = "PO No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOId = COL;
                COL++;

                sheet[ROW, COL].Text = "Lot Number";
                sheet[ROW, COL].ColumnWidth = 16;
                int colLotNumber = COL;
                COL++;

                sheet[ROW, COL].Text = "Master Order Item No";
                sheet[ROW, COL].ColumnWidth = 16;
                int colMasterOrderItemId = COL;
                COL++;

                sheet[ROW, COL].Text = "Qty Without Scan";
                sheet[ROW, COL].ColumnWidth = 16;
                int colQtyWithoutScan = COL;
                COL++;

                sheet[ROW, COL].Text = "Qty With Scan";
                sheet[ROW, COL].ColumnWidth = 16;
                int colQtyWithScan = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Actual qty";
                sheet[ROW, COL].ColumnWidth = 16;
                int colTotalActualqty = COL;
                COL++;

                sheet[ROW, COL].Text = "Detention In Minute";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDetentionInMinute = COL;
                COL++;

                sheet[ROW, COL].Text = "POSPT";
                sheet[ROW, COL].ColumnWidth = 16;
                int colPOSPT = COL;
                COL++;

                sheet[ROW, COL].Text = "Article SPT";
                sheet[ROW, COL].ColumnWidth = 16;
                int colArticleSPT = COL;
                COL++;

                sheet[ROW, COL].Text = "SPT";
                sheet[ROW, COL].ColumnWidth = 16;
                int colSPT = COL;
                COL++;

                sheet[ROW, COL].Text = "No Of Entry";
                sheet[ROW, COL].ColumnWidth = 16;
                int colNoOfEntry = COL;
                COL++;

                sheet[ROW, COL].Text = "Alloted Hour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colAllotedHour = COL;
                COL++;


                sheet[ROW, COL].Text = "Should Be Production";
                sheet[ROW, COL].ColumnWidth = 16;
                int colShouldBeProduction = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Available Hour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colTotalAvailableHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Detention Hour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDetentionHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Net Available Hour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colNetAvailableHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Produce Hour";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProduceHour = COL;
                COL++;

                sheet[ROW, COL].Text = "Detention Loss";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDetentionLoss = COL;
                COL++;

                sheet[ROW, COL].Text = "Productivity Variance";
                sheet[ROW, COL].ColumnWidth = 16;
                int colProductivityVariance = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                int LastRow = ROW + (data.Count - 1);

                for (int i = 0; i < data.Count; i++)
                {
                    sheet[ROW, colEmployeeCode].Text = data[i]["EmployeeCode"].ToString();
                    sheet[ROW, colEmployeeName].Text = data[i]["EmployeeName"].ToString();
                    sheet[ROW, colEmployeeCategory].Text = data[i]["EmployeeCategory"].ToString();
                    sheet[ROW, colDayStatus].Text = data[i]["DayStatus"].ToString();
                    sheet[ROW, colInStatus].Text = data[i]["InStatus"].ToString();
                    //sheet[ROW, colNoOfWorkStation].Number = clsStaticInfo.dbl(data[i]["NoOfWorkStation"].ToString());
                    //sheet[ROW, colStandardProcessTime].Number = clsStaticInfo.dbl(data[i]["StandardProcessTime"].ToString());
                    sheet[ROW, colInTime].Text = data[i]["InTime"].ToString();
                    sheet[ROW, colOutTime].Text = data[i]["OutTime"].ToString();
                    sheet[ROW, colPVOut].Number = clsStaticInfo.dbl(data[i]["PVOut"].ToString());
                    sheet[ROW, colPVIn].Text = data[i]["PVIn"].ToString();
                    sheet[ROW, colInDuration].Text = data[i]["InDuration"].ToString();
                    sheet[ROW, colOutDuration].Text = data[i]["OutDuration"].ToString();
                    sheet[ROW, colDesignation].Text = data[i]["Designation"].ToString();
                    sheet[ROW, colPOArticle].Text = data[i]["POArticle"].ToString();
                    sheet[ROW, colLineItemArticle].Text = data[i]["LineItemArticle"].ToString();
                    sheet[ROW, colLineItemProductCode].Text = data[i]["LineItemProductCode"].ToString();
                    sheet[ROW, colSONo].Text = data[i]["SONo"].ToString();
                    sheet[ROW, colPOId].Text = data[i]["POId"].ToString();
                    sheet[ROW, colLotNumber].Text = data[i]["LotNumber"].ToString();
                    sheet[ROW, colMasterOrderItemId].Text = data[i]["MasterOrderItemId"].ToString();
                    sheet[ROW, colQtyWithoutScan].Number = clsStaticInfo.dbl(data[i]["QtyWithoutScan"].ToString());
                    sheet[ROW, colQtyWithScan].Number = clsStaticInfo.dbl(data[i]["QtyWithScan"].ToString());
                    sheet[ROW, colTotalActualqty].Number = clsStaticInfo.dbl(data[i]["TotalActualqty"].ToString());
                    sheet[ROW, colDetentionInMinute].Number = clsStaticInfo.dbl(data[i]["DetentionInMinute"].ToString());
                    sheet[ROW, colPOSPT].Number = clsStaticInfo.dbl(data[i]["POSPT"].ToString());
                    sheet[ROW, colArticleSPT].Number = clsStaticInfo.dbl(data[i]["ArticleSPT"].ToString());
                    sheet[ROW, colSPT].Number = clsStaticInfo.dbl(data[i]["SPT"].ToString());
                    sheet[ROW, colNoOfEntry].Number = clsStaticInfo.dbl(data[i]["NoOfEntry"].ToString());
                    sheet[ROW, colAllotedHour].Number = clsStaticInfo.dbl(data[i]["AllotedHour"].ToString());
                    sheet[ROW, colShouldBeProduction].Number = clsStaticInfo.dbl(data[i]["ShouldBeProduction"].ToString());
                    sheet[ROW, colTotalAvailableHour].Number = clsStaticInfo.dbl(data[i]["TotalAvailableHour"].ToString());
                    sheet[ROW, colDetentionHour].Number = clsStaticInfo.dbl(data[i]["DetentionHour"].ToString());
                    sheet[ROW, colNetAvailableHour].Number = clsStaticInfo.dbl(data[i]["NetAvailableHour"].ToString());
                    sheet[ROW, colProduceHour].Number = clsStaticInfo.dbl(data[i]["ProduceHour"].ToString());
                    sheet[ROW, colDetentionLoss].Number = clsStaticInfo.dbl(data[i]["DetentionLoss"].ToString());
                    sheet[ROW, colProductivityVariance].Number = clsStaticInfo.dbl(data[i]["ProductivityVariance"].ToString());



                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Daily Planning & Production Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region MasterPlanSetUp

        public decimal GetMasterPlanSetUpSequence()
        {
            try
            {
                DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM [HKP].[MasterPlanSetUp]");
                if (dt.Rows.Count > 0)
                    return (decimal)clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

                return 1;
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        public IEnumerable <object> GetMasterPlanSetUpList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT TOP 100 * FROM (SELECT MPS.*,(select UserName from hkp.Process where Id=MPS.ProcessId) as Process FROM [HKP].[MasterPlanSetUp] MPS) AS TEMP WHERE " + strkey + "";

            return _sqlRepository.GetDataCollection(sql, null);
        }

        public void SaveMasterPlanSetUp(Dictionary<string, object> data, out string masterId)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from [HKP].[MasterPlanSetUp] where UserName='" + data["UserName"] + "'", out DataSet dsMasterPlanSetUpUserNameValidation, false, "1");
                con.OpenDataSetThroughAdapter("SELECT * FROM [HKP].[MasterPlanSetUp] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    if (dsMasterPlanSetUpUserNameValidation.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("User Name Already Exist.");
                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MasterPlanSetUp", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();


                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void MasterPlanSetUpDelete(string id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from [HKP].[MasterPlanSetUp] where Id ='" + id + @"'");
                conC.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion MasterPlanSetUp

        #region Master Plan Details

        public IEnumerable<object> GetMPDProcessList()
        {
            string sql = @"select PS.ProcessId as Value,(select P.UserName from hkp.Process P where P.Id=PS.ProcessId) as Text from [HKP].[MasterPlanSetUp] PS";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetUserNameList(string ProcessId)
        {
            string sql = @"select PS.ProcessId as Value, PS.UserName as Text from [HKP].[MasterPlanSetUp] PS where PS.ProcessId='"+ ProcessId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetMasterPlanList(string ProcessId)
        {
            string sql = @" SELECT * ,(select E.EmployeeName from EmployeeInformation E where E.SystemId=CP.UserId) as UserName,(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=CP.ResponsiblePersonId) as ResponsiblePerson,
                            (select UserName from hkp.Process where id=Cp.ProcessId) as Process,(select UserName from org.entity where id=Cp.EntityId) as Entity FROM [MST].[MasterPlan] CP where CP.ProcessId='" + ProcessId + "' and CP.Id not in (select MasterPlanId from [MST].[MasterPlanChild])";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetMasterPlanDetailsList(string ProcessId, string MasterPlanId)
        {
            string sql = @"select isnull(MOI.ProductionGrouping,'') AS ProductionGrouping,MOI.OwnReferenceNo, isnull(PO.Id,'') AS PONumber,
PS.UserName ProductionStatus,OS.UserName AS OrderStatusName,SO.Id SONo,SO.Qty,isnull((select PlanPercentage from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id),
MO.ExtraOrderPercentage) SOPlanPercentage,isnull((select SOPlanQty from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id),SO.Qty + (MO.ExtraOrderPercentage*SO.Qty / 100)) as SOPlanQty,
(select PlanStatus from MST.MasterPlan where id=(select MasterPlanId from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id)) as MasterPlanStatus,
(case when (select MasterPlanId from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) is null then 0 else 1 end) IsMasterPlan,
(select MasterPlanId from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) as MasterPlanId,
(select Id from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) as Id,
(select Status from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) as Status,
MOI.MaterialMasterId, MM.UserName AS MaterialMasterName, MOI.ArticleId, 
ART.StandardName AS ArticleName,P.UserName AS Customer,MOI.BuyerReferenceNo,MOI.Id LineItemNo,SO.Id AS SalesOrderId,MO.MasterOrderNo,
E.UserName POEntity,PPS.JobWorkApplicable IsJW,PPS.JobWorkType JWType,(Case when PPS.JobWorkType='EntityWithinCompany' then (select UserName from ORG.Entity where Id=PPS.EntityIdWithinCompany) 
when PPS.JobWorkType='EntityWithinGroup' then (select UserName from ORG.Entity where Id=PPS.EntityIdWithinCompany)
when PPS.JobWorkType='Party' then (select UserName from hkp.Party where Id=PPS.PartyId) end ) EntityVendor
from TRN.ProductionOrder PO
left join TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderId=PO.Id
left join TRN.ProductionOrderDetail POD ON POD.ProductionOrderId=PO.Id
left join TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
left join [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
left join [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
left join [HKP].[Party] AS P ON MO.PartyId = P.Id
left join [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
left join [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
left join [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
left join [TRN].[CustomerPO] AS CP ON SO.CustomerPOId = CP.Id
left join [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
left join [ORG].[Entity]  AS E ON E.Id=PO.EntityId
LEFT JOIN [MST].[MasterPlanSODetails] CPD on CPD.SalesOrderId=SO.Id and CPD.MasterPlanId='" + MasterPlanId + @"'
where PPS.ProcessId = '" + ProcessId + @"'  and CPD.MasterPlanId = '" + MasterPlanId + @"'
and PO.ProductionStatusId in (select Id from HKP.ProductionStatus where MasterPlanApplicable=1) and
SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1) and SO.Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + "' and Status=1) ORDER BY MOI.ProductionGrouping,MOI.OwnReferenceNo";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetMPDLineItemList(string MasterPlanId)
        {
            string sql = @"select MOI.ProductionGrouping,OwnReferenceNo,SO.Id SONo,MOI.Id LineItemNo,MOI.MaterialMasterId,
MM.UserName  MaterialMasterName, MOI.ArticleId,
ART.StandardName  ArticleName,MOI.BuyerReferenceNo,MOI.UOMId,UM.UserName UOM,SO.Qty,MOI.Remark,
(select SUM(Qty) from TRN.FirstCharacteristics  where SalesOrderId=SO.Id) as SKU1,
(select SUM(Qty) from TRN.SecondCharacteristics  where SalesOrderId=SO.Id) as SKU2,
(select Sum(Qty) from TRN.SalesOrder where Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + @"' and Status=1)) as LineItemTotalQty,
(select SUM(Qty) from TRN.FirstCharacteristics  where SalesOrderId in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + @"' and Status=1)) as SKU1TotalQty,
(select SUM(Qty) from TRN.SecondCharacteristics  where SalesOrderId in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + @"' and Status=1)) as SKU2TotalQty,
SO.CostingBOQMasterId BOQId,MOI.OrderCostingMasterTemplateId CostingId
from TRN.MasterOrderItem MOI
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
left join [MST].[MaterialMaster]  MM ON MOI.MaterialMasterId = MM.Id 
left join [MST].[MaterialMasterArticle]  ART ON MOI.ArticleId = ART.Id
left join [SCS].UnitOfMeasurement UM  ON UM.Id=MOI.UOMId
where SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1) and SO.Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + "' and Status=1)";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetMPDSKU1List(string MasterPlanId)
        {
            string sql = @"select MOI.ProductionGrouping,OwnReferenceNo,SO.Id SONo,MOI.Id LineItemNo,MOI.MaterialMasterId,
MM.UserName  MaterialMasterName, MOI.ArticleId,
ART.StandardName  ArticleName,MOI.BuyerReferenceNo,MOI.UOMId,UM.UserName UOM,SO.Qty,MOI.Remark,
SO.CostingBOQMasterId BOQId,MOI.OrderCostingMasterTemplateId CostingId,CV.UserName SKU1Name,FC.Qty SKU1Qty,CV.Remarks SKU1Remark
from TRN.MasterOrderItem MOI
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
left join [MST].[MaterialMaster]  MM ON MOI.MaterialMasterId = MM.Id 
left join [MST].[MaterialMasterArticle]  ART ON MOI.ArticleId = ART.Id
left join [SCS].UnitOfMeasurement UM  ON UM.Id=MOI.UOMId
left join TRN.FirstCharacteristics FC ON FC.SalesOrderId=SO.Id
left join HKP.Characteristics C ON C.Id=FC.CharacteristicsId
left Join HKP.CharacteristicsValue CV on CV.Id=FC.CharacteristicsValueId
where SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1) and SO.Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + "' and Status=1)";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetMPDSKU2List(string MasterPlanId)
        {
            string sql = @"select MOI.ProductionGrouping,OwnReferenceNo,SO.Id SONo,MOI.Id LineItemNo,MOI.MaterialMasterId,
MM.UserName  MaterialMasterName, MOI.ArticleId,
ART.StandardName  ArticleName,MOI.BuyerReferenceNo,MOI.UOMId,UM.UserName UOM,SO.Qty,MOI.Remark,
SO.CostingBOQMasterId BOQId,MOI.OrderCostingMasterTemplateId CostingId,(select UserName from HKP.CharacteristicsValue where Id=(select CharacteristicsValueId from TRN.FirstCharacteristics where Id=SC.FirstCharacteristicsId)) SKU1Name,CV.UserName SKU2Name,SC.Qty SKU2Qty,CV.Remarks SKU2Remark
from TRN.MasterOrderItem MOI
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
left join [MST].[MaterialMaster]  MM ON MOI.MaterialMasterId = MM.Id 
left join [MST].[MaterialMasterArticle]  ART ON MOI.ArticleId = ART.Id
left join [SCS].UnitOfMeasurement UM  ON UM.Id=MOI.UOMId
left join TRN.FirstCharacteristics FC ON FC.SalesOrderId=SO.Id
left join TRN.SecondCharacteristics SC ON SC.SalesOrderId=SO.Id and SC.FirstCharacteristicsId=FC.Id
left join HKP.Characteristics C ON C.Id=SC.CharacteristicsId
left Join HKP.CharacteristicsValue CV on CV.Id=SC.CharacteristicsValueId
left Join HKP.CharacteristicsValue FCV on CV.Id=FC.CharacteristicsValueId
where SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1) and SO.Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + "' and Status=1)";
            return _sqlRepository.GetDataCollection(sql, null);
        }
         
        public IEnumerable<object> GetMasterPlanQtyList(string MasterPlanId, string MinQty, string PlanPercentage, bool LineItem, bool SKU1, bool SKU2)
        {
            string sql = "";
            if(LineItem == true && SKU1 == false && SKU2 == false)
            {
                sql = @"select MPQ.Id,'" + MasterPlanId + @"' MasterPlanId,MOI.ProductionGrouping,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,MOI.ArticleId,
ART.StandardName  ArticleName,MOI.UOMId,UM.UserName UOM,Sum(SO.Qty) Qty,isnull(MPQ.MinQty,isnull(" + MinQty + ",0)) MinQty,isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)) PlanPercentage,
(case when (Sum(SO.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0))/100 < isnull(MPQ.MinQty,isnull(" + MinQty + ",0)))  then (Sum(SO.Qty) + isnull(MPQ.MinQty,isnull(" + MinQty + ",0))) else CEILING(((Sum(SO.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)))/100 + Sum(SO.Qty))) end)  MasterPlanQty,
isnull(MPQ.Adjustmentqty,0) AdjustmentQty,(case when (Sum(SO.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)))/100 < isnull(MPQ.MinQty,isnull(" + MinQty + ",0))  then ((Sum(SO.Qty) + isnull(MPQ.MinQty,isnull(" + MinQty + ",0))) - (isnull(MPQ.Adjustmentqty,0))) else (CEILING(((Sum(SO.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)))/100 + Sum(SO.Qty))) - (isnull(MPQ.Adjustmentqty,0))) end) FinalQty,
MPQ.Remarks
from TRN.MasterOrderItem MOI 
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
left join [MST].[MaterialMaster]  MM ON MOI.MaterialMasterId = MM.Id 
left join [MST].[MaterialMasterArticle]  ART ON MOI.ArticleId = ART.Id
left join [SCS].UnitOfMeasurement UM  ON UM.Id=MOI.UOMId
left join [MST].[MasterPlanChild] MPQ ON MPQ.MasterPlanId='" + MasterPlanId + @"' 
where SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1) and SO.Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + "' and Status=1) Group By MOI.Id,MOI.ArticleId,MPQ.Id,MOI.ProductionGrouping,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,ART.StandardName,MOI.UOMId,UM.UserName,MPQ.MinQty,MPQ.PlanPercentage,MPQ.Adjustmentqty,MPQ.Remarks";
            }
            if(LineItem == true && SKU1 == true && SKU2 == false)
            {
                sql = @"select MPQ.Id,'" + MasterPlanId + @"' MasterPlanId,MOI.ProductionGrouping,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,MOI.ArticleId,
ART.StandardName  ArticleName,FC.CharacteristicsValueId as SKU1NameId,(select UserName from HKP.CharacteristicsValue where Id=FC.CharacteristicsValueId) SKU1Name
,MOI.UOMId,UM.UserName UOM,SUM(FC.Qty) Qty,isnull(MPQ.MinQty,isnull(" + MinQty + ",0)) MinQty,isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)) PlanPercentage,
(case when (Sum(FC.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0))/100 < isnull(MPQ.MinQty,isnull(" + MinQty + ",0)))  then (Sum(FC.Qty) + isnull(MPQ.MinQty,isnull(" + MinQty + ",0))) else CEILING(((Sum(FC.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)))/100 + Sum(FC.Qty))) end)  MasterPlanQty,
isnull(MPQ.Adjustmentqty,0) AdjustmentQty,(case when (Sum(FC.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)))/100 < isnull(MPQ.MinQty,isnull(" + MinQty + ",0))  then ((Sum(FC.Qty) + isnull(MPQ.MinQty,isnull(" + MinQty + ",0))) - (isnull(MPQ.Adjustmentqty,0))) else (CEILING(((Sum(FC.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)))/100 + Sum(FC.Qty))) - (isnull(MPQ.Adjustmentqty,0))) end) FinalQty,
MPQ.Remarks
from TRN.MasterOrderItem MOI
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
left join [MST].[MaterialMaster]  MM ON MOI.MaterialMasterId = MM.Id 
left join [MST].[MaterialMasterArticle]  ART ON MOI.ArticleId = ART.Id
left join [SCS].UnitOfMeasurement UM  ON UM.Id=MOI.UOMId
left join TRN.FirstCharacteristics FC ON FC.SalesOrderId=SO.Id
left join [MST].[MasterPlanChild] MPQ ON MPQ.MasterPlanId='" + MasterPlanId + @"' 
where SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1) and SO.Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + "' and Status=1) Group By MOI.Id,MOI.ArticleId,FC.CharacteristicsValueId,MPQ.Id,MOI.ProductionGrouping,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,ART.StandardName,MOI.UOMId,UM.UserName,MPQ.MinQty,MPQ.PlanPercentage,MPQ.Adjustmentqty,MPQ.Remarks";
            }
            if(LineItem == true && SKU1 == true && SKU2 == true)
            { 
             sql = @"select MPQ.Id,'" + MasterPlanId + @"' MasterPlanId,MOI.ProductionGrouping,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,MOI.ArticleId,
ART.StandardName  ArticleName,FC.CharacteristicsValueId as SKU1NameId,SC.CharacteristicsValueId as SKU2NameId,
(select UserName from HKP.CharacteristicsValue where Id=FC.CharacteristicsValueId) SKU1Name,
(select UserName from HKP.CharacteristicsValue where Id=SC.CharacteristicsValueId) as SKU2Name
,MOI.UOMId,UM.UserName UOM,Sum(SC.Qty) Qty,isnull(MPQ.MinQty,isnull(" + MinQty + ",0)) MinQty,isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)) PlanPercentage,
(case when (Sum(SC.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0))/100 < isnull(MPQ.MinQty,isnull(" + MinQty + ",0)))  then (Sum(SC.Qty) + isnull(MPQ.MinQty,isnull(" + MinQty + ",0))) else CEILING(((Sum(SC.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)))/100 + Sum(SC.Qty))) end)  MasterPlanQty,
isnull(MPQ.Adjustmentqty,0) AdjustmentQty,(case when (Sum(SC.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)))/100 < isnull(MPQ.MinQty,isnull(" + MinQty + ",0))  then ((Sum(SC.Qty) + isnull(MPQ.MinQty,isnull(" + MinQty + ",0))) - (isnull(MPQ.Adjustmentqty,0))) else (CEILING(((Sum(SC.Qty) * isnull(MPQ.PlanPercentage,isnull(" + PlanPercentage + @",0)))/100 + Sum(SC.Qty))) - (isnull(MPQ.Adjustmentqty,0))) end) FinalQty,
MPQ.Remarks
from TRN.MasterOrderItem MOI
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
left join [MST].[MaterialMaster]  MM ON MOI.MaterialMasterId = MM.Id 
left join [MST].[MaterialMasterArticle]  ART ON MOI.ArticleId = ART.Id
left join [SCS].UnitOfMeasurement UM  ON UM.Id=MOI.UOMId
left join TRN.FirstCharacteristics FC ON FC.SalesOrderId=SO.Id
left join TRN.SecondCharacteristics SC ON SC.SalesOrderId=SO.Id and SC.FirstCharacteristicsId=FC.Id
left join [MST].[MasterPlanChild] MPQ ON MPQ.MasterPlanId='" + MasterPlanId + @"' 
where SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1) and SO.Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + "' and Status=1)  Group By MOI.Id,MOI.ArticleId,FC.CharacteristicsValueId,SC.CharacteristicsValueId,MPQ.Id,MOI.ProductionGrouping,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,ART.StandardName,MOI.UOMId,UM.UserName,MPQ.MinQty,MPQ.PlanPercentage,MPQ.Adjustmentqty,MPQ.Remarks";
            }
            return _sqlRepository.GetDataCollection(sql, null);
        }

        #endregion Master Plan Details

        #region Cut Plan

        public IEnumerable<object> GetMasterPlanListForCutPlan(string ProcessId)
        {
            string sql = @" SELECT * ,(select E.EmployeeName from EmployeeInformation E where E.SystemId=CP.UserId) as UserName,(select EI.EmployeeName from EmployeeInformation EI where EI.SystemId=CP.ResponsiblePersonId) as ResponsiblePerson,
                            (select UserName from hkp.Process where id=Cp.ProcessId) as Process,(select UserName from org.entity where id=Cp.EntityId) as Entity FROM [MST].[MasterPlan] CP where CP.ProcessId='" + ProcessId + "' and CP.Id in (select MasterPlanId from [MST].[MasterPlanChild])";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanList(string ProcessId, string MasterPlanId)
        {
            string sql = @"select isnull(MOI.ProductionGrouping,'') AS ProductionGrouping,MOI.OwnReferenceNo, isnull(PO.Id,'') AS PONumber,
PS.UserName ProductionStatus,OS.UserName AS OrderStatusName,SO.Id SONo,SO.Qty,isnull((select PlanPercentage from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id),
MO.ExtraOrderPercentage) SOPlanPercentage,isnull((select SOPlanQty from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id),SO.Qty + (MO.ExtraOrderPercentage*SO.Qty / 100)) as SOPlanQty,
(select PlanStatus from MST.MasterPlan where id=(select MasterPlanId from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id)) as MasterPlanStatus,
(case when (select MasterPlanId from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) is null then 0 else 1 end) IsMasterPlan,
(select MasterPlanId from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) as MasterPlanId,
(select Id from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) as Id,
(select Status from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id) as Status,
MOI.MaterialMasterId, MM.UserName AS MaterialMasterName, MOI.ArticleId, 
ART.StandardName AS ArticleName,P.UserName AS Customer,MOI.BuyerReferenceNo,MOI.Id LineItemNo,SO.Id AS SalesOrderId,MO.MasterOrderNo,
E.UserName POEntity,PPS.JobWorkApplicable IsJW,PPS.JobWorkType JWType,(Case when PPS.JobWorkType='EntityWithinCompany' then (select UserName from ORG.Entity where Id=PPS.EntityIdWithinCompany) 
when PPS.JobWorkType='EntityWithinGroup' then (select UserName from ORG.Entity where Id=PPS.EntityIdWithinCompany)
when PPS.JobWorkType='Party' then (select UserName from hkp.Party where Id=PPS.PartyId) end ) EntityVendor
from TRN.ProductionOrder PO
left join TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderId=PO.Id
left join TRN.ProductionOrderDetail POD ON POD.ProductionOrderId=PO.Id
left join TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
left join [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
left join [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
left join [HKP].[Party] AS P ON MO.PartyId = P.Id
left join [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
left join [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
left join [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
left join [TRN].[CustomerPO] AS CP ON SO.CustomerPOId = CP.Id
left join [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
left join [ORG].[Entity]  AS E ON E.Id=PO.EntityId
LEFT JOIN [MST].[MasterPlanSODetails] CPD on CPD.SalesOrderId=SO.Id and CPD.MasterPlanId='" + MasterPlanId + @"'
where PPS.ProcessId = '" + ProcessId + @"'  and CPD.MasterPlanId = '" + MasterPlanId + @"'
and PO.ProductionStatusId in (select Id from HKP.ProductionStatus where MasterPlanApplicable=1)
and SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1)
and SO.Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + "' and Status=1) ORDER BY MOI.ProductionGrouping,MOI.OwnReferenceNo";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanQtyList(string MasterPlanId, bool LineItem, bool SKU1, bool SKU2, string MinQty, string SKU1ColorId)
        {
            string sql = "";
            string MinQtyChk = string.Empty;

            if (MinQty != "0" && MinQty != "undefined")
            {
                MinQtyChk = ""+ MinQty +"";
            }
            else
            {
                MinQtyChk = "(select min(FinalQty) from[MST].[MasterPlanChild] where MasterPlanId = '" + MasterPlanId + @"')";
            }

            if (LineItem == true && SKU1 == false && SKU2 == false)
            {
                sql = @"select null Id,null AllotedHeaderId,MPQ.Id MasterPlanChildId,MasterPlanId,ProductionGrouping,BuyerReferenceNo,OwnReferenceNo,MPQ.ArticleId,
ART.StandardName  ArticleName,MPQ.UOMId,UM.UserName UOM,MPQ.Qty,MPQ.FinalQty,MPQ.AllotedQty,MPQ.FinalQty-MPQ.AllotedQty BalanceQty,
Floor((MPQ.FinalQty-MPQ.AllotedQty) / " + MinQtyChk + @" ) Ratio, 
Floor((MPQ.FinalQty-MPQ.AllotedQty) / " + MinQtyChk + @" ) * " + MinQtyChk + @" CurrentAllotedQty,
MPQ.Remarks,CAST(1 AS BIT) Status
from [MST].[MasterPlanChild] MPQ 
left join [MST].[MaterialMasterArticle]  ART ON MPQ.ArticleId = ART.Id
left join [SCS].UnitOfMeasurement UM  ON UM.Id=MPQ.UOMId
where MPQ.MasterPlanId='" + MasterPlanId + @"'";
            }
            if (LineItem == true && SKU1 == true && SKU2 == false)
            {
                sql = @"select null Id,null AllotedHeaderId,MPQ.Id MasterPlanChildId,MasterPlanId,ProductionGrouping,BuyerReferenceNo,OwnReferenceNo,MPQ.ArticleId,
ART.StandardName  ArticleName,MPQ.SKU1NameId,(select UserName from HKP.CharacteristicsValue where Id=MPQ.SKU1NameId) SKU1Name,
MPQ.UOMId,UM.UserName UOM,MPQ.Qty,MPQ.FinalQty,MPQ.AllotedQty,MPQ.FinalQty-MPQ.AllotedQty BalanceQty,
Floor((MPQ.FinalQty-MPQ.AllotedQty) / " + MinQtyChk + @" ) Ratio,  
Floor((MPQ.FinalQty-MPQ.AllotedQty) / " + MinQtyChk + @" ) * " + MinQtyChk + @" CurrentAllotedQty,
MPQ.Remarks,CAST(1 AS BIT)  Status
from [MST].[MasterPlanChild] MPQ 
left join [MST].[MaterialMasterArticle]  ART ON MPQ.ArticleId = ART.Id
left join [SCS].UnitOfMeasurement UM  ON UM.Id=MPQ.UOMId
where MPQ.MasterPlanId='" + MasterPlanId + @"'";
            }
            if (LineItem == true && SKU1 == true && SKU2 == true)
            {
                sql = @"select null Id,null AllotedHeaderId,MPQ.Id MasterPlanChildId,MasterPlanId,ProductionGrouping,BuyerReferenceNo,OwnReferenceNo,MPQ.ArticleId,
ART.StandardName  ArticleName,MPQ.SKU1NameId,(select UserName from HKP.CharacteristicsValue where Id=MPQ.SKU1NameId) SKU1Name,
MPQ.SKU2NameId,(select UserName from HKP.CharacteristicsValue where Id=MPQ.SKU2NameId) SKU2Name,
MPQ.UOMId,UM.UserName UOM,MPQ.Qty,MPQ.FinalQty,MPQ.AllotedQty,MPQ.FinalQty-MPQ.AllotedQty BalanceQty,
Floor((MPQ.FinalQty-MPQ.AllotedQty) / " + MinQtyChk + @") Ratio, 
Floor((MPQ.FinalQty-MPQ.AllotedQty) / " + MinQtyChk + @") * " + MinQtyChk + @" CurrentAllotedQty,
MPQ.Remarks,CAST(1 AS BIT)  Status
from [MST].[MasterPlanChild] MPQ 
left join [MST].[MaterialMasterArticle]  ART ON MPQ.ArticleId = ART.Id
left join [SCS].UnitOfMeasurement UM  ON UM.Id=MPQ.UOMId
where MPQ.MasterPlanId='" + MasterPlanId + @"' and SKU1NameId='"+ SKU1ColorId + "'";
            }
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetPackingTypeLists()
        {
            string sql = @"select PT.Id as Value,PT.UserName as Text from HKP.PackingType PT";
            return _sqlRepository.GetDataCollection(sql, null);
        }
        public IEnumerable<object> GetSKU1ColorLists(string MasterPlanId)
        {
            string sql = @"select distinct CV.Id as Value,CV.UserName as Text from MST.MasterPlan MP
left join MST.MasterPlanChild MPC ON MPC.MasterPlanId=MP.Id
left join HKP.CharacteristicsValue CV ON CV.Id=MPC.SKU1NameId
where MPC.MasterPlanId='" + MasterPlanId + "' and MPC.Id not in (select MasterPlanChildId from MST.AllotedChild where (select SUM(AllotedQty) from MST.AllotedChild where AllotedHeaderId in (select Id from MST.AllotedHeader where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = MPC.SKU1NameId)) = (Select Sum(FinalQty) from MST.MasterPlanChild where MasterPlanId = '" + MasterPlanId + "' and SKU1NameId = MPC.SKU1NameId))";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        #endregion Cut Plan

        #region Cut Plan Edit

        public IEnumerable<object> GetColorLists(string MasterPlanId)
        {
            string sql = @"select distinct CV.Id as Value,CV.UserName as Text from MST.MasterPlan MP
left join MST.MasterPlanChild MPC ON MPC.MasterPlanId=MP.Id
left join HKP.CharacteristicsValue CV ON CV.Id=MPC.SKU1NameId
where MPC.MasterPlanId='" + MasterPlanId + "' and MPC.Id in (select MasterPlanChildId from MST.AllotedChild where AllotedHeaderId in (select Id from MST.AllotedHeader where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId is not null))";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetMasterPlanList()
        {
            string sql = @"select distinct MasterPlanId as Value,(select PlanName from MST.MasterPlan where Id=MasterPlanId) as Text from MST.AllotedHeader";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanSummary(string MasterPlanId, string ColorId)
        {
            string sql = @"select distinct AH.MasterPlanId,(select UserName from HKP.CharacteristicsValue where Id=AH.SKU1ColorId) Color,AH.SKU1ColorId,(select UserName from HKP.CharacteristicsValue where Id=MPC.SKU2NameId) Size,MPC.SKU2NameId,MPC.FinalQty,MPC.AllotedQty,UserName UserNameR1,NoOfPly NoOfPlyR1,R1.Ratio Ratio1,R1.AllotedQty AllotedQtyR1,MPC.FinalQty-R1.AllotedQty BalanceToAllotedR1
,Ratio2.UserNameR2,Ratio2.NoOfPlyR2,
floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2) Ra2,
floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2 AllotedQtyR2,
(MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2) BalanceToAllotedR2
,Ratio3.UserNameR3,Ratio3.NoOfPlyR3,
floor(((MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2))/Ratio3.NoOfPlyR3) Ra3,
floor(((MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2))/Ratio3.NoOfPlyR3)*Ratio3.NoOfPlyR3 AllotedQtyR3,
(MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2)-floor(((MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2))/Ratio3.NoOfPlyR3)*Ratio3.NoOfPlyR3 BalanceToAllotedR3
,Ratio4.UserNameR4,Ratio4.NoOfPlyR4,
floor(((MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2)-floor(((MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2))/Ratio3.NoOfPlyR3)*Ratio3.NoOfPlyR3)/Ratio4.NoOfPlyR4) Ra4,
floor(((MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2)-floor(((MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2))/Ratio3.NoOfPlyR3)*Ratio3.NoOfPlyR3)/Ratio4.NoOfPlyR4)*Ratio4.NoOfPlyR4 AllotedQtyR4,
(MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2)-floor(((MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2))/Ratio3.NoOfPlyR3)*Ratio3.NoOfPlyR3-floor(((MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2)-floor(((MPC.FinalQty-R1.AllotedQty)-(floor((MPC.FinalQty-R1.AllotedQty)/Ratio2.NoOfPlyR2)*Ratio2.NoOfPlyR2))/Ratio3.NoOfPlyR3)*Ratio3.NoOfPlyR3)/Ratio4.NoOfPlyR4)*Ratio4.NoOfPlyR4 BalanceToAllotedR4
from [MST].[AllotedHeader] AH
left join [MST].[AllotedChild] R1 on R1.AllotedHeaderId=AH.Id
left join [MST].[MasterPlanChild] MPC on MPC.Id=R1.MasterPlanChildId and MPC.MasterPlanId='" + MasterPlanId + "' and MPC.SKU1NameId='" + ColorId + @"'
left join(
select AH.MasterPlanId, AH.SKU1ColorId, MPC.FinalQty, UserName UserNameR2, NoOfPly NoOfPlyR2, R2.Ratio Ra2, R2.AllotedQty AllotedQtyR2, MPC.FinalQty-R2.AllotedQty BalanceToAllotedR2 from[MST].[AllotedHeader] AH
      left join[MST].[AllotedChild] R2 on R2.AllotedHeaderId = AH.Id
left join[MST].[MasterPlanChild] MPC on MPC.Id = R2.MasterPlanChildId and MPC.MasterPlanId = '"+ MasterPlanId +"' and MPC.SKU1NameId = '"+ ColorId + @"'
where AH.MasterPlanId = '"+ MasterPlanId +"' and AH.SKU1ColorId = '"+ ColorId +@"'
and R2.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '"+ MasterPlanId +"' and SKU1ColorId = '"+ ColorId +@"' ORDER BY Id OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY)) Ratio2 on Ratio2.MasterPlanId = AH.MasterPlanId
left join(
select AH.MasterPlanId, AH.SKU1ColorId, MPC.FinalQty, UserName UserNameR3, NoOfPly NoOfPlyR3, R2.Ratio Ra3, R2.AllotedQty AllotedQtyR3, MPC.FinalQty-R2.AllotedQty BalanceToAllotedR3 from[MST].[AllotedHeader] AH
      left join[MST].[AllotedChild] R2 on R2.AllotedHeaderId = AH.Id
left join[MST].[MasterPlanChild] MPC on MPC.Id = R2.MasterPlanChildId and MPC.MasterPlanId = '"+ MasterPlanId +"' and MPC.SKU1NameId = '"+ ColorId +@"'
where AH.MasterPlanId = '"+ MasterPlanId +"' and AH.SKU1ColorId = '"+ ColorId +@"'
and R2.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '"+ MasterPlanId +"' and SKU1ColorId = '"+ ColorId +@"' ORDER BY Id OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY)) Ratio3 on Ratio3.MasterPlanId = AH.MasterPlanId
left join(
select AH.MasterPlanId, AH.SKU1ColorId, MPC.FinalQty, UserName UserNameR4, NoOfPly NoOfPlyR4, R2.Ratio Ra4, R2.AllotedQty AllotedQtyR4, MPC.FinalQty-R2.AllotedQty BalanceToAllotedR4 from[MST].[AllotedHeader] AH
      left join[MST].[AllotedChild] R2 on R2.AllotedHeaderId = AH.Id
left join[MST].[MasterPlanChild] MPC on MPC.Id = R2.MasterPlanChildId and MPC.MasterPlanId = '"+ MasterPlanId +"' and MPC.SKU1NameId = '"+ ColorId +@"'
where AH.MasterPlanId = '"+ MasterPlanId +"' and AH.SKU1ColorId = '"+ ColorId +@"'
and R2.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '"+ MasterPlanId +"' and SKU1ColorId = '"+ ColorId +@"' ORDER BY Id OFFSET 3 ROWS FETCH NEXT 1 ROWS ONLY)) Ratio4 on Ratio4.MasterPlanId = AH.MasterPlanId
where AH.MasterPlanId = '"+ MasterPlanId +"' and AH.SKU1ColorId = '"+ ColorId +@"'
and R1.AllotedHeaderId = (select top 1 Id from[MST].[AllotedHeader] where MasterPlanId = '"+ MasterPlanId +"' and SKU1ColorId = '"+ ColorId +@"')";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetAllotedHeaderCountList(string MasterPlanId, string ColorId)
        {
            string sql = @"select COUNT(Id) HeaderCount from MST.AllotedHeader where MasterPlanId='"+ MasterPlanId + "' and SKU1ColorId='"+ ColorId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        
        public IEnumerable<object> GetCutPlanDetailsR1List(string MasterPlanId, string ColorId)
        {
            string sql = @"select AH.MasterPlanId,AC.AllotedHeaderId R1Id,AC.Id,MPC.Id MasterPlanChildId,CV.UserName Size,MPC.FinalQty,CVC.UserName Color,AH.SKU1ColorId,MPC.SKU2NameId,
AH.UserName UserNameR1,AH.NoOfPly NoOfPlyR1,AH.MarkerId MarkerIdR1,AH.PackingTypeId PackingTypeIdR1,
AC.Ratio Ratio1,AC.AllotedQty AllotedQtyR1
from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
left join [MST].[AllotedChild] AC on AC.MasterPlanChildId=MPC.Id
left join [MST].[AllotedHeader] AH on AH.Id=AC.AllotedHeaderId
left join HKP.CharacteristicsValue CVC on CVC.Id=AH.SKU1ColorId
where MPC.MasterPlanId='" + MasterPlanId + "' and MPC.SKU1NameId='" + ColorId + @"' and AC.AllotedHeaderId = (select top 1 Id from[MST].[AllotedHeader] where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = '" + ColorId + @"') order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanDetailsR2List(string MasterPlanId, string ColorId)
        {
            string sql = @"select AH.MasterPlanId,AC.AllotedHeaderId R2Id,AC.Id,MPC.Id MasterPlanChildId,CV.UserName Size,MPC.FinalQty,CVC.UserName Color,AH.SKU1ColorId,MPC.SKU2NameId,
AH.UserName UserNameR2,AH.NoOfPly NoOfPlyR2,AH.MarkerId MarkerIdR2,AH.PackingTypeId PackingTypeIdR2,
AC.Ratio Ratio2,AC.AllotedQty AllotedQtyR2,MPC.FinalQty-MPC.AllotedQty BalanceToAllotedR1
from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
left join [MST].[AllotedChild] AC on AC.MasterPlanChildId=MPC.Id
left join [MST].[AllotedHeader] AH on AH.Id=AC.AllotedHeaderId
left join HKP.CharacteristicsValue CVC on CVC.Id=AH.SKU1ColorId
where MPC.MasterPlanId = '" + MasterPlanId + "' and MPC.SKU1NameId = '" + ColorId + @"'
and AC.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = '" + ColorId + @"' ORDER BY Id OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY) order by CV.Sequence ";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanDetailsR3List(string MasterPlanId, string ColorId)
        {
            string sql = @"select AH.MasterPlanId,AC.AllotedHeaderId R3Id,AC.Id,MPC.Id MasterPlanChildId,CV.UserName Size,MPC.FinalQty,CVC.UserName Color,AH.SKU1ColorId,MPC.SKU2NameId,
AH.UserName UserNameR3,AH.NoOfPly NoOfPlyR3,AH.MarkerId MarkerIdR3,AH.PackingTypeId PackingTypeIdR3,
AC.Ratio Ratio3,AC.AllotedQty AllotedQtyR3,MPC.FinalQty-MPC.AllotedQty  BalanceToAllotedR2 from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
left join [MST].[AllotedChild] AC on AC.MasterPlanChildId=MPC.Id
left join [MST].[AllotedHeader] AH on AH.Id=AC.AllotedHeaderId
left join HKP.CharacteristicsValue CVC on CVC.Id=AH.SKU1ColorId
where MPC.MasterPlanId = '" + MasterPlanId + "' and MPC.SKU1NameId = '" + ColorId + @"'
and AC.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = '" + ColorId + @"' ORDER BY Id OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY) order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanDetailsR4List(string MasterPlanId, string ColorId)
        {
            string sql = @"select AH.MasterPlanId,AC.AllotedHeaderId R4Id,AC.Id,MPC.Id MasterPlanChildId,CV.UserName Size,MPC.FinalQty,CVC.UserName Color,AH.SKU1ColorId,MPC.SKU2NameId,
AH.UserName UserNameR4,AH.NoOfPly NoOfPlyR4,AH.MarkerId MarkerIdR4,AH.PackingTypeId PackingTypeIdR4,
AC.Ratio Ratio4,AC.AllotedQty AllotedQtyR4,MPC.FinalQty-MPC.AllotedQty BalanceToAllotedR3
 from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
left join [MST].[AllotedChild] AC on AC.MasterPlanChildId=MPC.Id
left join [MST].[AllotedHeader] AH on AH.Id=AC.AllotedHeaderId
left join HKP.CharacteristicsValue CVC on CVC.Id=AH.SKU1ColorId
where MPC.MasterPlanId = '" + MasterPlanId + "' and MPC.SKU1NameId = '" + ColorId + @"'
and AC.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = '" + ColorId + @"' ORDER BY Id OFFSET 3 ROWS FETCH NEXT 1 ROWS ONLY) order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanDetailsR5List(string MasterPlanId, string ColorId)
        {
            string sql = @"select AH.MasterPlanId,AC.AllotedHeaderId R5Id,AC.Id,MPC.Id MasterPlanChildId,CV.UserName Size,MPC.FinalQty,CVC.UserName Color,AH.SKU1ColorId,MPC.SKU2NameId,
AH.UserName UserNameR5,AH.NoOfPly NoOfPlyR5,AH.MarkerId MarkerIdR5,AH.PackingTypeId PackingTypeIdR5,
AC.Ratio Ratio5,AC.AllotedQty AllotedQtyR5,MPC.FinalQty-MPC.AllotedQty BalanceToAllotedR4
 from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
left join [MST].[AllotedChild] AC on AC.MasterPlanChildId=MPC.Id
left join [MST].[AllotedHeader] AH on AH.Id=AC.AllotedHeaderId
left join HKP.CharacteristicsValue CVC on CVC.Id=AH.SKU1ColorId
where MPC.MasterPlanId = '" + MasterPlanId + "' and MPC.SKU1NameId = '" + ColorId + @"'
and AC.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = '" + ColorId + @"' ORDER BY Id OFFSET 4 ROWS FETCH NEXT 1 ROWS ONLY) order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanDetailsR6List(string MasterPlanId, string ColorId)
        {
            string sql = @"select AH.MasterPlanId,AC.AllotedHeaderId R6Id,AC.Id,MPC.Id MasterPlanChildId,CV.UserName Size,MPC.FinalQty,CVC.UserName Color,AH.SKU1ColorId,MPC.SKU2NameId,
AH.UserName UserNameR6,AH.NoOfPly NoOfPlyR6,AH.MarkerId MarkerIdR6,AH.PackingTypeId PackingTypeIdR6,
AC.Ratio Ratio6,AC.AllotedQty AllotedQtyR6,MPC.FinalQty-MPC.AllotedQty BalanceToAllotedR5
 from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
left join [MST].[AllotedChild] AC on AC.MasterPlanChildId=MPC.Id
left join [MST].[AllotedHeader] AH on AH.Id=AC.AllotedHeaderId
left join HKP.CharacteristicsValue CVC on CVC.Id=AH.SKU1ColorId
where MPC.MasterPlanId = '" + MasterPlanId + "' and MPC.SKU1NameId = '" + ColorId + @"'
and AC.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = '" + ColorId + @"' ORDER BY Id OFFSET 5 ROWS FETCH NEXT 1 ROWS ONLY) order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanDetailsR7List(string MasterPlanId, string ColorId)
        {
            string sql = @"select AH.MasterPlanId,AC.AllotedHeaderId R7Id,AC.Id,MPC.Id MasterPlanChildId,CV.UserName Size,MPC.FinalQty,CVC.UserName Color,AH.SKU1ColorId,MPC.SKU2NameId,
AH.UserName UserNameR7,AH.NoOfPly NoOfPlyR7,AH.MarkerId MarkerIdR7,AH.PackingTypeId PackingTypeIdR7,
AC.Ratio Ratio7,AC.AllotedQty AllotedQtyR7,MPC.FinalQty-MPC.AllotedQty BalanceToAllotedR6
 from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
left join [MST].[AllotedChild] AC on AC.MasterPlanChildId=MPC.Id
left join [MST].[AllotedHeader] AH on AH.Id=AC.AllotedHeaderId
left join HKP.CharacteristicsValue CVC on CVC.Id=AH.SKU1ColorId
where MPC.MasterPlanId = '" + MasterPlanId + "' and MPC.SKU1NameId = '" + ColorId + @"'
and AC.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = '" + ColorId + @"' ORDER BY Id OFFSET 6 ROWS FETCH NEXT 1 ROWS ONLY) order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanDetailsR8List(string MasterPlanId, string ColorId)
        {
            string sql = @"select AH.MasterPlanId,AC.AllotedHeaderId R8Id,AC.Id,MPC.Id MasterPlanChildId,CV.UserName Size,MPC.FinalQty,CVC.UserName Color,AH.SKU1ColorId,MPC.SKU2NameId,
AH.UserName UserNameR8,AH.NoOfPly NoOfPlyR8,AH.MarkerId MarkerIdR8,AH.PackingTypeId PackingTypeIdR8,
AC.Ratio Ratio8,AC.AllotedQty AllotedQtyR8,MPC.FinalQty-MPC.AllotedQty BalanceToAllotedR7
 from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
left join [MST].[AllotedChild] AC on AC.MasterPlanChildId=MPC.Id
left join [MST].[AllotedHeader] AH on AH.Id=AC.AllotedHeaderId
left join HKP.CharacteristicsValue CVC on CVC.Id=AH.SKU1ColorId
where MPC.MasterPlanId = '" + MasterPlanId + "' and MPC.SKU1NameId = '" + ColorId + @"'
and AC.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = '" + ColorId + @"' ORDER BY Id OFFSET 7 ROWS FETCH NEXT 1 ROWS ONLY) order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanDetailsR9List(string MasterPlanId, string ColorId)
        {
            string sql = @"select AH.MasterPlanId,AC.AllotedHeaderId R9Id,AC.Id,MPC.Id MasterPlanChildId,CV.UserName Size,MPC.FinalQty,CVC.UserName Color,AH.SKU1ColorId,MPC.SKU2NameId,
AH.UserName UserNameR9,AH.NoOfPly NoOfPlyR9,AH.MarkerId MarkerIdR9,AH.PackingTypeId PackingTypeIdR9,
AC.Ratio Ratio9,AC.AllotedQty AllotedQtyR9,MPC.FinalQty-MPC.AllotedQty BalanceToAllotedR8
 from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
left join [MST].[AllotedChild] AC on AC.MasterPlanChildId=MPC.Id
left join [MST].[AllotedHeader] AH on AH.Id=AC.AllotedHeaderId
left join HKP.CharacteristicsValue CVC on CVC.Id=AH.SKU1ColorId
where MPC.MasterPlanId = '" + MasterPlanId + "' and MPC.SKU1NameId = '" + ColorId + @"'
and AC.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = '" + ColorId + @"' ORDER BY Id OFFSET 8 ROWS FETCH NEXT 1 ROWS ONLY) order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanDetailsR10List(string MasterPlanId, string ColorId)
        {
            string sql = @"select AH.MasterPlanId,AC.AllotedHeaderId R10Id,AC.Id,MPC.Id MasterPlanChildId,CV.UserName Size,MPC.FinalQty,CVC.UserName Color,AH.SKU1ColorId,MPC.SKU2NameId,
AH.UserName UserNameR10,AH.NoOfPly NoOfPlyR10,AH.MarkerId MarkerIdR10,AH.PackingTypeId PackingTypeIdR10,
AC.Ratio Ratio10,AC.AllotedQty AllotedQtyR10,MPC.FinalQty-MPC.AllotedQty BalanceToAllotedR9
 from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
left join [MST].[AllotedChild] AC on AC.MasterPlanChildId=MPC.Id
left join [MST].[AllotedHeader] AH on AH.Id=AC.AllotedHeaderId
left join HKP.CharacteristicsValue CVC on CVC.Id=AH.SKU1ColorId
where MPC.MasterPlanId = '" + MasterPlanId + "' and MPC.SKU1NameId = '" + ColorId + @"'
and AC.AllotedHeaderId = (select Id from[MST].[AllotedHeader] where MasterPlanId = '" + MasterPlanId + "' and SKU1ColorId = '" + ColorId + @"' ORDER BY Id OFFSET 9 ROWS FETCH NEXT 1 ROWS ONLY) order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCutPlanDetailsBalanceList(string MasterPlanId, string ColorId)
        {
            string sql = @"select MPC.Id,MPC.FinalQty,CV.UserName Size,(select Sum(AllotedQty) from  [MST].[AllotedChild] where MasterPlanChildId=MPC.Id) AllotedQty,
MPC.FinalQty-(select Sum(AllotedQty) from  [MST].[AllotedChild] where MasterPlanChildId=MPC.Id) BalanceQty
from [MST].[MasterPlanChild] MPC 
left join HKP.CharacteristicsValue CV on CV.Id=MPC.SKU2NameId
where MPC.MasterPlanId='" + MasterPlanId + "' and MPC.SKU1NameId='" + ColorId + @"' 
order by CV.Sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        #endregion

        #region fabric Roll

        public IEnumerable<object> GetSummaryList(string GRNId, string parameters)
        {
            string GRNRowIdFilter = string.Empty;
            if (parameters != "null")
            {
                GRNRowIdFilter = " and FRC.GRNRowId in (" + parameters + ")";
            }
            string sql = @"select FRC.FabricType,FRC.CutableWidth,FRC.ShrinkageWidthWise,FRC.ShrinkageLengthWise,FRC.Shade,COUNT(FRC.Id) NoOfRoll,Sum(FRC.SupplierQty) Qty,
FRC.CutableWidthGroup,FRC.MarkerGroup,FRC.ShadeGroup,FRC.FabricGroup,FRC.Remarks,FRC.ShrinkageGroup
 from BPDT.FabricRollManagementChild FRC
where FRC.FabricRollManagementMasterId in (select Id from BPDT.FabricRollManagementMaster where GRNId='" + GRNId + @"')" + GRNRowIdFilter + @"
Group By CutableWidth,ShrinkageWidthWise,ShrinkageLengthWise,Shade,
CutableWidthGroup,MarkerGroup,ShadeGroup,FabricGroup,Remarks,ShrinkageGroup,FabricType";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetFilterList(string GRNId)
        {
            string sql = @"select  FRC.GRNRowId,MMA.Id ArticleId,MMA.StandardName Article,MM.Id MaterialId,MM.UserName Material,SUM(FRC.SupplierQty) Qty,Count(FRC.Id) NoOfRoll,Sum(FRC.Status) NoOfPackage from BPDT.FabricRollManagementChild FRC
left join TRN.InventoryReceiveDetail IRD on IRD.Id=FRC.GRNRowId
Left Join TRN.InventoryMaterial IM on IM.Id=IRD.InventoryMaterialId
left join MST.MaterialMasterArticle MMA on MMA.Id=IM.ArticleId
left join MST.MaterialMaster MM on MM.Id=IM.MaterialMasterId
where FRC.FabricRollManagementMasterId in (select Id from BPDT.FabricRollManagementMaster where GRNId='" + GRNId + @"')
Group By FRC.GRNRowId,MMA.StandardName,MMA.Id,MM.Id,MM.UserName";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetCustomerDataList(string HeaderId)
        {
           
            string sql = @"select CAST (CASE WHEN CM.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,CM.Id,CM.HeaderId,P.UserName Customer,MO.Id MasterOrderNo,SO.Id SOId,SO.Id SONo,SO.MasterOrderItemId,MA.StandardName Article,
MOI.BuyerReferenceNo CustomerReferenceNo,MOI.OwnReferenceNo,PL.Code ProductCode,SO.DeliveryDate,CM.Remarks
from TRN.SalesOrder SO
left outer join trn.MasterOrderItem MOI on moi.Id=SO.MasterOrderItemId
left outer join trn.MasterOrder MO on mo.Id=MOI.MasterOrderId
left outer join [HKP].[Party] p on P.Id=MO.PartyId
left outer join MST.MaterialMasterArticle MA  on MA.Id=MOI.ArticleId
left outer join dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
left outer join TRN.CustomerMaterial CM on CM.HeaderId='" + HeaderId + @"' and CM.SOId=SO.Id 
where SO.OrderStatusId='Active'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        #endregion

        #region Lot Wise Quality Report & Lot Wise Quality Report Update


        public IEnumerable<object> GetCustomerList()
        {
            string sql = @"select Distinct PartyId,P.UserName Customer,P.ShortName,P.StandardName from TRN.Sales S
left join HKP.Party P ON P.Id=S.PartyId
where P.Active=1";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetUpdateCustomerList()
        {
            string sql = @"select Distinct CustomerId PartyId,P.UserName Customer,P.ShortName,P.StandardName from TRN.CustomerQualityReportHeader CQH
LEFT JOIN HKP.Party P ON P.Id=CQH.CustomerId
where CQH.CustomerId not in ('null')";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetSummaryCustomerList()
        {
            string sql = @"select Distinct CustomerId PartyId,P.UserName Customer,P.ShortName,P.StandardName from TRN.CustomerQualityReportHeader CQH
LEFT JOIN HKP.Party P ON P.Id=CQH.CustomerId
where CQH.CustomerId not in ('null')";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetInvoiceList(string PartyId)
        {
            string sql = @"select I.Id InvoiceId,P.UserName Party,Format(I.AddedDate,'dd-MMM-yyyy') InvoiceDate,isnull(POD.ProductionOrderId,'') PONo from TRN.Sales I
left join HKP.Party P on P.Id=I.PartyId
LEFT join SalesPacking SP on SP.SalesId=I.Id
LEFT join TRN.Packing PK on PK.PackingId=SP.PackingId
LEFT join TRN.PackingLineItem PLI on PLI.PackingId=PK.PackingId
LEFT join TRN.SalesOrder SO on SO.Id=PLI.SOId
LEFT JOIN TRN.ProductionOrderDetail POD on POD.SalesOrderId=SO.Id
where PartyId='" + PartyId + "' order by I.AddedDate desc";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetUpdateInvoiceList(string PartyId)
        {
            string sql = @"select I.Id InvoiceId,P.UserName Party,Format(I.AddedDate,'dd-MMM-yyyy') InvoiceDate from TRN.CustomerQualityReportHeader CQH
left Join TRN.Sales I on I.Id=CQH.InvoiceId
left join HKP.Party P on P.Id=I.PartyId
where PartyId='" + PartyId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetSummaryInvoiceList(string PartyId)
        {
            string sql = @"select I.Id InvoiceId,P.UserName Party,Format(I.AddedDate,'dd-MMM-yyyy') InvoiceDate from TRN.CustomerQualityReportHeader CQH
left Join TRN.Sales I on I.Id=CQH.InvoiceId
left join HKP.Party P on P.Id=I.PartyId
where PartyId='" + PartyId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetInvoicePOList(string InvoiceId)
        {
            string sql = "";
            if (InvoiceId != "null" && InvoiceId != "undefined")
            {
                sql = @"select distinct POD.ProductionOrderId Value,POD.ProductionOrderId Text from TRN.Sales S 
LEFT join SalesPacking SP on SP.SalesId=S.Id
LEFT join TRN.Packing P on P.PackingId=SP.PackingId
LEFT join TRN.PackingLineItem PLI on PLI.PackingId=P.PackingId
LEFT join TRN.SalesOrder SO on SO.Id=PLI.SOId
LEFT JOIN TRN.ProductionOrderDetail POD on POD.SalesOrderId=SO.Id
LEFT JOIN TRN.ProductionOrder PO on PO.Id=POD.ProductionOrderId
where S.Id='" + InvoiceId + "' and ProductionOrderId is not null";
            }
            else
            {
                sql = @"select distinct Id Value,Id Text from TRN.ProductionOrder";
            }

            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetUpdateInvoicePOList(string InvoiceId)
        {
            string sql = "";
            if (InvoiceId != "null" && InvoiceId != "undefined")
            {
                sql = @"select ProductionOrderId Value,ProductionOrderId Text from TRN.CustomerQualityReportHeader CQH where InvoiceId='"+ InvoiceId + "'";
            }
            else
            {
                sql = @"select distinct ProductionOrderId Value,ProductionOrderId Text from TRN.CustomerQualityReportHeader";
            }

            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetSummaryInvoicePOList(string InvoiceId)
        {
            string sql = "";
            if (InvoiceId != "null" && InvoiceId != "undefined")
            {
                sql = @"select ProductionOrderId Value,ProductionOrderId Text from TRN.CustomerQualityReportHeader CQH where InvoiceId='" + InvoiceId + "'";
            }
            else
            {
                sql = @"select distinct ProductionOrderId Value,ProductionOrderId Text from TRN.CustomerQualityReportHeader";
            }

            return _sqlRepository.GetDataCollection(sql, null);
        }
        public IEnumerable<object> GetLotNumberLists(string POId)
        {
            string sql = "";
            if (POId != "null" && POId != "undefined")
            {
                 sql = @"select P.* from (select Distinct PS.LotNumber,PS.ProductionOrderId PONo,(case when OWC.Grade is null then 'NoGrade' else OWC.Grade end) Status from TRN.ProductionSummary PS
left join MST.OrderWiseQualityComment OWC on OWC.LotNo=PS.LotNumber
left join TRN.ProductionOrder PO on PO.Id=PS.ProductionOrderId
where PS.ProductionOrderId = '" + POId + "' and PS.LotNumber is not null and PS.LotNumber not in (' ') and PS.LotNumber not in (select LotNo from TRN.CustomerQualityReportHeader))P order by Status desc";
            }
            else
            {
                sql = @"select P.* from (select Distinct PS.LotNumber,PS.ProductionOrderId PONo,(case when OWC.Grade is null then 'NoGrade' else OWC.Grade end) Status from TRN.ProductionSummary PS
left join MST.OrderWiseQualityComment OWC on OWC.LotNo=PS.LotNumber
where PS.LotNumber is not null and PS.LotNumber not in (' ') and PS.LotNumber not in (select LotNo from TRN.CustomerQualityReportHeader))P
order by Status desc";
            }
            return _sqlRepository.GetDataCollection(sql, null);
        }
        public IEnumerable<object> GetUpdateLotNumberLists(string POId)
        {
            string sql = "";
            if (POId != "null" && POId != "undefined")
            {
                sql = @"select distinct LotNo LotNumber,ProductionOrderId PONo from TRN.CustomerQualityReportHeader where ProductionOrderId = '" + POId + "'";
            }
            else
            {
                sql = @"select distinct LotNo LotNumber,ProductionOrderId PONo from TRN.CustomerQualityReportHeader";
            }
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetSummaryLotNumberLists(string POId)
        {
            string sql = "";
            if (POId != "null" && POId != "undefined")
            {
                sql = @"select distinct LotNo LotNumber,ProductionOrderId PONo from TRN.CustomerQualityReportHeader where ProductionOrderId = '" + POId + "'";
            }
            else
            {
                sql = @"select distinct LotNo LotNumber,ProductionOrderId PONo from TRN.CustomerQualityReportHeader";
            }
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> LoadLotWiseQualityReport(string POId, string LotNumber, string CustomerId, string InvoiceId)
        {
            string CQRHFilter = string.Empty;
            string LotFilter =string.Empty;
            string CustFilter = string.Empty;
            string InvFilter = string.Empty;
            if (POId != "null" && POId != "undefined")
            {
                LotFilter = " and QC.ProductionOrderId='" + POId + "' and QC.LotNumber='" + LotNumber + "'";
                CQRHFilter = " CQH.ProductionOrderId='" + POId + "' and CQH.LotNo='" + LotNumber + "'";
            }
            else
            {
                LotFilter = " and QC.LotNumber='" + LotNumber + "'";
                CQRHFilter = " CQH.LotNo='" + LotNumber + "'";
            }
            if (CustomerId != "null" && CustomerId != "undefined")
            {
                CustFilter = " and CQH.CustomerId='" + CustomerId + "'";
            }
            if (InvoiceId != "null" && InvoiceId != "undefined")
            {
                InvFilter = " and CQH.InvoiceId='" + InvoiceId + "'";
            }
            string sql = @"select distinct QC.ProductionOrderId,QC.LotNumber LotNo,'" + CustomerId + "' CustomerId,'"+ InvoiceId + @"' InvoiceId,
--CustomerName= isnull((select UserName from hkp.Party where Id='" + CustomerId + @"'),STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO
--JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
--left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
--left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
--left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
--where QC.ProductionOrderId=Xpod.ProductionOrderId    for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')),
CustomerName=(select UserName from hkp.Party where Id='" + CustomerId + @"'),
Article = STUFF((select distinct ',' + MA.StandardName from trn.ProductionOrderDetail Pod
left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join[MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
where Pod.ProductionOrderId = QC.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
(select UserName from MST.QualityManagementMaster where Id=QC.IssueId) as IssueName,QMP.Finalreport,
QMP.Id ParameterId,CQH.Id CQRHeaderId,CQH.UserName,CQH.Remarks,CQH.ByWhomId,
(select EmployeeName from EmployeeInformation where systemId=CQH.ByWhomId) as ByWhom,
(select top 1 Id from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId) as Id,
(select top 1 ParaRemarks from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId) as ParaRemarks,
(select top 1 SpecialRemarks from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId) as SpecialRemarks,
isnull((select top 1 Value from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId),QCD.Value) Value,
PM.UserName Parameter,QMP.UOMId,UM.UserName UOM,
Reverse(stuff(Reverse((select QR.Grade +', ' from MST.QualityRemark QR																			
where QR.PONo=QC.ProductionOrderId and QR.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) QRGrade,
Reverse(stuff(Reverse((select (select EmployeeName from EmployeeInformation where SystemId=QR.ByWhomId) +', ' from MST.QualityRemark QR																			
where QR.PONo=QC.ProductionOrderId and QR.LotNo=QC.LotNumber  for xml PATH(''))),1,2,'')) QRByWhom,
Reverse(stuff(Reverse((select QR.Comment +', ' from MST.QualityRemark QR																			
where QR.PONo=QC.ProductionOrderId and QR.LotNo=QC.LotNumber   for xml PATH(''))),1,2,'')) QRComment,
Reverse(stuff(Reverse((select OWC.Grade +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.PONo=QC.ProductionOrderId and OWC.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) OWGrade,
Reverse(stuff(Reverse((select (select EmployeeName from EmployeeInformation where SystemId=(Select AuthorizedResPersonId from HKP.QualityManagementAuthorizedPerson where Id=OWC.ByWhomId)) +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.PONo=QC.ProductionOrderId and OWC.LotNo=QC.LotNumber  for xml PATH(''))),1,2,'')) OWByWhom,
Reverse(stuff(Reverse((select OWC.Comment +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.PONo=QC.ProductionOrderId and OWC.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) OWComment
from MST.QualityManagementParameterItem QMP
left join TRN.QualityControlDetails QCD on QCD.ItemId=QMP.Id
left join TRN.QualityControl QC on QC.Id=QCD.QCID
left join HKP.ParameterMaster PM on PM.Id=QMP.ParameterId
left join SCS.UnitOfMeasurement UM on UM.Id=QMP.UOMId
left join[TRN].[CustomerQualityReportHeader] CQH on " + CQRHFilter + " " + CustFilter + " " + InvFilter + @"
where CustomerParameter=1 and QCD.GradeId is not null" + LotFilter + "";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> LoadLWQRUpdate(string POId, string LotNumber, string CustomerId, string InvoiceId)
        {
            string CQRHFilter = string.Empty;
            string LotFilter = string.Empty;
            string CustFilter = string.Empty;
            string InvFilter = string.Empty;
            if (POId != "null" && POId != "undefined")
            {
                LotFilter = " and QC.ProductionOrderId='" + POId + "' and QC.LotNumber='" + LotNumber + "'";
                CQRHFilter = " CQH.ProductionOrderId='" + POId + "' and CQH.LotNo='" + LotNumber + "'";
            }
            else
            {
                LotFilter = " and QC.LotNumber='" + LotNumber + "'";
                CQRHFilter = " CQH.LotNo='" + LotNumber + "'";
            }
            if (CustomerId != "null" && CustomerId != "undefined")
            {
                CustFilter = " and CQH.CustomerId='" + CustomerId + "'";
            }
            if (InvoiceId != "null" && InvoiceId != "undefined")
            {
                InvFilter = " and CQH.InvoiceId='" + InvoiceId + "'";
            }
            string sql = @"select P.* from (select distinct QC.ProductionOrderId,QC.LotNumber LotNo,'" + CustomerId + "' CustomerId,'" + InvoiceId + @"' InvoiceId,
CustomerName=(select UserName from hkp.Party where Id='" + CustomerId + @"'),
Article = STUFF((select distinct ',' + MA.StandardName from trn.ProductionOrderDetail Pod
left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join[MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
where Pod.ProductionOrderId = QC.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
(select UserName from MST.QualityManagementMaster where Id=QC.IssueId) as IssueName,
(select top 1 Finalreport from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId) Finalreport,
QMP.Id ParameterId,CQH.Id CQRHeaderId,CQH.UserName,CQH.Remarks,CQH.ByWhomId,
(select EmployeeName from EmployeeInformation where systemId=CQH.ByWhomId) as ByWhom,
(select top 1 Id from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId) as Id,
(select top 1 ParaRemarks from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId) as ParaRemarks,
(select top 1 SpecialRemarks from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId) as SpecialRemarks,
isnull((select top 1 Value from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId),QCD.Value) Value,
PM.UserName Parameter,QMP.UOMId,UM.UserName UOM,
Reverse(stuff(Reverse((select QR.Grade +', ' from MST.QualityRemark QR																			
where QR.PONo=QC.ProductionOrderId and QR.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) QRGrade,
Reverse(stuff(Reverse((select (select EmployeeName from EmployeeInformation where SystemId=QR.ByWhomId) +', ' from MST.QualityRemark QR																			
where QR.PONo=QC.ProductionOrderId and QR.LotNo=QC.LotNumber  for xml PATH(''))),1,2,'')) QRByWhom,
Reverse(stuff(Reverse((select QR.Comment +', ' from MST.QualityRemark QR																			
where QR.PONo=QC.ProductionOrderId and QR.LotNo=QC.LotNumber   for xml PATH(''))),1,2,'')) QRComment,
Reverse(stuff(Reverse((select OWC.Grade +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.PONo=QC.ProductionOrderId and OWC.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) OWGrade,
Reverse(stuff(Reverse((select (select EmployeeName from EmployeeInformation where SystemId=(Select AuthorizedResPersonId from HKP.QualityManagementAuthorizedPerson where Id=OWC.ByWhomId)) +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.PONo=QC.ProductionOrderId and OWC.LotNo=QC.LotNumber  for xml PATH(''))),1,2,'')) OWByWhom,
Reverse(stuff(Reverse((select OWC.Comment +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.PONo=QC.ProductionOrderId and OWC.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) OWComment
from MST.QualityManagementParameterItem QMP
left join TRN.QualityControlDetails QCD on QCD.ItemId=QMP.Id
left join TRN.QualityControl QC on QC.Id=QCD.QCID
left join HKP.ParameterMaster PM on PM.Id=QMP.ParameterId
left join SCS.UnitOfMeasurement UM on UM.Id=QMP.UOMId
left join[TRN].[CustomerQualityReportHeader] CQH on " + CQRHFilter + " " + CustFilter + " " + InvFilter + @"
where CustomerParameter=1 and QCD.GradeId is not null" + LotFilter + ")P order by P.Finalreport desc";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        
        public IEnumerable<object> GetByWhomList()
        {
            string sql = @"select EMP.SystemId, EMP.EmployeeCode, EMP.EmployeeName, FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, EC.UserName EmployeeCategory, DP.UserName Department
                               ,SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation, LDSG.UserName LegalDesignation, UN.UserName as Entity
                                from EmployeeInformation EMP
                                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                                where EMP.EmployeeStatus = 'Active'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        #endregion
    }
}


