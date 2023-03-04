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

        public IEnumerable<object> GetProductionOrderDataList(string entityid, string workCenterMasterId, string productionLevel, string processId)
        {
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
								   WHERE PS.UserName = 'Running' Order by PD.Description,PD.BuyerOrder";

            return _sqlRepository.GetDataCollection(CmdText);
        }

         public IEnumerable<object> GetProductionOrderDataListWC(string entityid, string workCenterMasterId, string productionLevel, string processId)
        {
            string CmdText = @"SELECT distinct PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
								  -- ,PlannedQty=CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END
         --                   --,((CASE WHEN PQ.Qty=0 THEN PO.PlannedQty ELSE PO.PlannedQty END)-ISNULL(CEILING(PRS.TotalProductionQty),0)) RemainingQty
         --                    ,ISNULL((CASE WHEN ISNULL(PPS.Qty,0)=0 THEN ISNULL(PQ.Qty,PO.PlannedQty) ELSE PO.PlannedQty*PPS.Qty/100 END)-ISNULL(CEILING(PRS.TotalProductionQty), 0),0) RemainingQty
         --                   , ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
									--,SONo=STUFF((select distinct ','+XSO.Id from 
         --                                                        trn.SalesOrder XSO 
         --                                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						   --                                      LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
         --                                                        WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
         --                       ,Material=STUFF((select distinct ','+MM.UserName from 
         --                                                        trn.SalesOrder XSO 
         --                                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						   --                                      LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						   --                                      LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
         --                                                        WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
         --                    ,Article=STUFF((select distinct ','+MMA.StandardName  from 
         --                                                        trn.SalesOrder XSO 
         --                                                        JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						   --                                      LEFT JOIN trn.MasterOrderItem moi ON moi.Id = XSO.MasterOrderItemId
						   --                                      LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
									--							 LEFT JOIN MST.MaterialMasterArticle AS mma on mma.MaterialMasterId=MM.Id
         --                                                        WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
         --                          PRS.LotNumber
         --                          --,PRS.ResponsiblePerson
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
								   WHERE PS.UserName = 'Running' and E.Id='" + entityid + @"' Order by PD.Description,PD.BuyerOrder";

            return _sqlRepository.GetDataCollection(CmdText);
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
WHERE p.ProductionBookingProcessParameterId=(SELECT Id FROM dbo.ProductionBookingProcessParameter WHERE ProcessId='" + processId + "') ORDER BY P.Sequence";
                }
                else
                {
                    sql = @"SELECT A.Id,P.UserName,P.Formula,P.FormulaId,P.EntryState,ValueIN = CASE WHEN P.ValueinDecimal=1 THEN 'Decimal' ELSE 'Percentage' END
,Value=CASE WHEN PD.Value IS NOT NULL THEN PD.Value ELSE (CASE WHEN P.ValueinDecimal=1 THEN P.DefaultValue ELSE P.DefaultValue/100 END) END
,P.Id ProductionBookingParameterId,P.IsProduction
FROM dbo.ProductionBookingParameter P
LEFT JOIN [dbo].[ProductionSummaryParameterValue] A ON A.ProductionBookingParameterId=P.Id AND ISNULL(A.ProductionSummaryId,'null')='null'
LEFT JOIN (SELECT * FROM [dbo].[ProductionSummaryParameterValue] WHERE ProductionSummaryId=(SELECT TOP(1) Id FROM TRN.ProductionSummary WHERE ProductionOrderId='" + ProductionOrderId + @"' AND ProcessId='" + processId + @"' ORDER BY AddedDate DESC))PD ON PD.UserName=P.UserName
WHERE p.ProductionBookingProcessParameterId=(SELECT Id FROM dbo.ProductionBookingProcessParameter WHERE ProcessId='" + processId + @"') ORDER BY P.Sequence";
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

        public void getOrderMaster(Dictionary<string, string> parameters, out DataTable dtOrderMaster)
        {

            string sql = @"SELECT trkp.UserName AS Plant,trke.UserName AS Entity,trke.Id as EntityId,so.Id AS SalesOrderId, b.UserName AS Buyer,ei.EmployeeName AS ResponsiblePerson,mo.MasterOrderNo,mm.UserName AS Material,
                           OC.UserName AS OrderCategory,os.UserName AS OrderStatus,   MA.StandardName AS Article,                   
                            pc.UserName AS ProductCategory,  pm.UserName AS Product,moi.BuyerReferenceNo,MOI.OwnReferenceNo,
                            mo.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo OwnOrderNo,SO.Description AS SODesc,
                            mm.Id AS MaterialRowId,pod.ProductionOrderId,CASE WHEN isnull(sed.ID,0)<>0 THEN 'YES' ELSE 'NO' END AS isProductionScheduled,
                            so.DeliveryDate,so.CommitmentDate,so.Qty AS SOQty, cp.PONumber,format(cp.PODate,'dd-MMM-yyyy') AS PODate,ps.UserName AS ProductionStatus,
                            CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS PlannedQty,0 AS CummPlannedQty,
                           
                            --CEILING((so.Qty*(1+(moi.ExtraOrderPercentage/100)))*(1+(moi.OrderWastagePercentage/100))) AS PlannedQty,0 AS CummPlannedQty,
                            PO.Qty AS PRQty,case when isnull(SED.Qty,0)=0 THEN PO.PlannedQty ELSE  SED.Qty END AS PRActualPlannedQty,
                            PO.PlannedQty AS PRPlannedQty,P.UserName AS Customer,ISNULL(PL.Code,'-') ProductCode
							,ProductAttribute=ISNULL(STUFF((select distinct '/'+PLA.UserName+'-'+PLA.AttributeValue from
[dbo].[ProductLibraryAttribute] PLA
left join[dbo].[ProductLibrary] MA ON MA.Id=PLA.ProductLibraryId
where MA.Id=MOI.ProductLibraryId for xml path('') ), 1, 1, ''),'-')

							,FORMAT(PO.AddedDate,'dd-MMM-yyyy')POCreationDate ,FORMAT(BASEP.BaseProcProdStartDate,'dd-MMM-yyyy')BaseProcProdStartDate,FORMAT(BASEP.BaseProductionEndDate,'dd-MMM-yyyy')BaseProductionEndDate
,FORMAT(Type1.BaseProcPlanStartDate,'dd-MMM-yyyy')BaseProcPlanStartDate,FORMAT(Type1.BaseProcPlanEndDate,'dd-MMM-yyyy')BaseProcPlanEndDate
,POStartDate=FORMAT(case when Type1.BaseProcPlanStartDate is null or BASEP.BaseProcProdStartDate  <  Type1.BaseProcPlanStartDate then BASEP.BaseProcProdStartDate else Type1.BaseProcPlanStartDate end,'dd-MMM-yyyy')
,POCompletionDate=FORMAT((case when Type1.BaseProcPlanEndDate is null or BASEP.BaseProductionEndDate  > Type1.BaseProcPlanEndDate then BASEP.BaseProductionEndDate else Type1.BaseProcPlanEndDate end ),'dd-MMM-yyyy')
,FORMAT(SO.PlanExFactoryDate,'dd-MMM-yyyy')PlanExFactoryDate,ISNULL(FBPPD.POProduceQty,0)POProduceQty,RemainingQty=ISNULL(case when isnull(SED.Qty,0)=0 THEN PO.PlannedQty ELSE  SED.Qty END-FBPPD.POProduceQty,0)

                             FROM trn.SalesOrder SO
							LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=so.Id 
                            left outer join trn.MasterOrderItem MOI on moi.Id=SO.MasterOrderItemId
                            left outer join dbo.ProductLibrary PL ON PL.Id=MOI.ProductLibraryId
							left outer join trn.MasterOrder MO ON MO.Id=MOI.MasterOrderId
                            LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS SED ON sed.ProductionOrderID=pod.ProductionOrderId
                            LEFT OUTER JOIN trn.ProductionOrder AS po ON po.Id=pod.ProductionOrderId
                            LEFT OUTER JOIN hkp.ProductionStatus AS ps ON ps.Id=po.ProductionStatusId
                            LEFT OUTER JOIN trn.CustomerPO AS cp ON cp.Id=so.CustomerPOId                        

							LEFT JOIN(Select MIN(ProductionDate)BaseProcProdStartDate,MAX(ProductionDate)BaseProductionEndDate,A.ProductionOrderId 
FROM TRN.ProductionSummary A
LEFT JOIN HKP.Process B ON B.Id=A.ProcessId
Group By A.ProductionOrderId) BASEP ON BASEP.ProductionOrderId=POD.ProductionOrderId

LEFT JOIN(Select MIN(ProductionDate)BaseProcPlanStartDate,MAX(ProductionDate)BaseProcPlanEndDate,ProductionOrderId 
From ProductionPlanningType1 Group By ProductionOrderId) Type1 ON Type1.ProductionOrderId=POD.ProductionOrderId

LEFT JOIN(Select SUM(Quantity)POProduceQty ,ProductionOrderId From TRN.ProductionSummary Group By ProductionOrderId) FBPPD ON FBPPD.ProductionOrderId=PO.Id

                            LEFT OUTER JOIN ORg.Entity AS TRKE ON trke.Id = PO.EntityId
                            LEFT OUTER JOIN org.Plant AS TRKP ON  trkp.Id = TRKE.PlantId

                            left outer join mst.MaterialMaster mm on mm.id=moi.MaterialMasterId
                            LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                            left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                            left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                            left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                           
                            left outer join [HKP].[Party] p on P.Id=MO.PartyId
                            left outer join [HKP].[PartyPlant] PPI on ppi.id=mo.InvoicingPartyPlantId
                            left outer join [HKP].[PartyPlant] PPD on ppd.id=mo.DeliveryPartyPlantId
                            left outer join [HKP].[Buyer] B on b.id=mo.BuyerId
                            left outer join [HKP].[BuyerBrand] BB on bb.id=mo.BuyerBrandId
                            left outer join [HKP].[BuyerDivision] BD on bd.id=mo.BuyerBrandId
                            left outer join [HKP].[OrderCategory] OC on oc.id=mo.OrderCategoryId
                            left outer join [HKP].[OrderStatus] OS on OS.id=mo.OrderStatusId
                            left outer join mst.Destination DEST on dest.Id=so.DestinationId
                            left outer join [TRN].[CustomerPO] CPO ON CPO.Id=so.CustomerPOId
                            left outer join [MST].[ShipMode] SMO on SMO.Id=so.ShipmentModeId
                            left outer join hkp.Season S on s.id=mo.SeasonId
                            left outer join EmployeeInformation EI on ei.SystemId= MO.ResponsiblePersonId

                            AND MO.EntityId in(" + parameters["EntityId"] + @")
                            AND SO.OrderStatusId in(" + parameters["OrderStatusId"] + @")
                            AND MO.ResponsiblePersonId in(" + parameters["ResponsiblePersonId"] + @")
                            AND p.Id in(" + parameters["PartyId"] + @")
            ORDER BY trkp.UserName,trke.UserName,trke.Id, pod.ProductionOrderId,so.DeliveryDate,SO.ID";

            dtOrderMaster = _sqlRepository.GetDataTable(sql);


        }

        public IEnumerable<object> GetSOCompletionReportFilter()
        {
            try
            {
                string sql = @"SELECT distinct MO.ResponsiblePersonId,E.EmployeeName ResponsiblePerson,MO.EntityId,EN.UserName Entity,MO.PartyId,P.UserName Customer,SO.OrderStatusId FROM trn.MasterOrder MO
LEFT JOIN ORG.Entity EN ON EN.Id=MO.EntityId
LEFT JOIN HKP.Party P ON P.Id = MO.PartyId
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=MO.ResponsiblePersonId
LEFT JOIN TRN.MasterOrderItem MOI ON MOI.MasterOrderId=MO.Id
LEFT JOIN TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id";
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
							,isnull(MMT.[Minute],0) DetentionInMin,0 Utilization,pp.ProductionOrderId PORefNo,pp.AddedBy EntryBy,ISNULL(UOM.Code,'-') UOM,pp.Quantity ProductionQty,ISNULL(pp.Remarks,'-')Remarks

                            FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,COUNT(*) AS ProductionHours,ps.Quantity
									,PS.AddedBy,ps.Remarks,ps.ProductLibraryId
                                    FROM trn.ProductionSummary AS ps 
                                  left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
                                  LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
      		                      WHERE ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND ps.EntityID in (" + EntityId + @")  and ps.ProcessId in (" + ProcessId + @") "+ psft + @"
                                  GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,PS.AddedBy,ps.Remarks,ps.ProductLibraryId,ps.Quantity
                            ) AS pp
							left join MachineMasterTransaction MMT on MMT.ProcessId=pp.ProcessId and MMT.ShiftId=pp.ProductionShiftId and MMT.WorkCenterId=pp.WorkCenterMasterId and MMT.[Date]=pp.ProductionDate
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

                string sql = @"SELECT A.* from (SELECT distinct PP.Id, trkp.UserName AS Plant,trke.UserName AS Entity,pp.EntityID,pp.WorkCenterMasterId, PP.ProductionOrderID,wcm.UserName AS WorkCenter,FORMAT(PP.ProductionDate,'dd-MMM-yyyy') AS ActualDate,pp.Quantity AS ActualQty,ORD.CM*pp.Quantity AS ActualCM,
                            pt1.SPT AS SAM,pp.ProcessId,isnull(p.UserName,FSFG.UserName) AS Process,isnull(Tp.UserName,TSFG.UserName) AS ToProcess,Twcm.UserName AS ToWorkCenter,ISNULL(pp.UserName,ord.Material) Material,ISNULL(pp.StandardName,ord.Article ) Article              
                            ,PL.Code ProductCode,ord.Product, ord.ProductCategory,Format(SN.AddedDate,'dd-MMM-yyyy') AS SnapshotDate,
                            sn.Quantity AS PlanQty,ORD.CM*sn.Quantity AS PlanCM,ORD.CM
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
							--,PSV.UserName Parameter,psv.[Value] ParameterValue
							,isnull(MMT.[Minute],0) DetentionInMin,0 Utilization,pp.ProductionOrderId PORefNo,pp.AddedBy EntryBy,UOM.Code UOM,pp.Quantity ProductionQty,pp.Remarks

                            FROM (SELECT  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId,  ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,COUNT(*) AS ProductionHours,ps.Quantity
									,PS.AddedBy,ps.Remarks,ps.ProductLibraryId
                                    FROM trn.ProductionSummary AS ps 
                                  left outer join mst.MaterialMaster mm on mm.id=ps.MaterialMasterId
                                  LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=ps.ArticleId
      		                      WHERE ps.ProductionDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"' AND ps.EntityID in (" + EntityId + @")  and ps.ProcessId in (" + wp + @")
                                  GROUP BY  ps.Id,ps.ProcessId,mm.UserName,ma.StandardName,ps.FromSFGInventoryId,ps.ToProcessId,ps.ToSFGInventoryId,  ps.EntityId,ps.SalesOrderId,ps.ProductionShiftId, ps.ProductionOrderId,ps.ProductionDate,ps.WorkCenterMasterId,ps.ToWorkCenterMasterId,PS.AddedBy,ps.Remarks,ps.ProductLibraryId,ps.Quantity
                            ) AS pp
							left join MachineMasterTransaction MMT on MMT.ProcessId=pp.ProcessId and MMT.ShiftId=pp.ProductionShiftId and MMT.WorkCenterId=pp.WorkCenterMasterId and MMT.[Date]=pp.ProductionDate
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
                                          	)A ORDER BY A.ActualDate, A.WorkCenterMasterId, A.ProductionOrderID";
                dtOrder = _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

    }


}


