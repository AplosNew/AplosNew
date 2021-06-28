using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.Enums;

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

        public IEnumerable<object> GetSOItem(string entityid, string workCenterMasterId, string productionLevel, string processId)
        {
            if (productionLevel != "ProductionOrder")
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
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running'	AND POSP.ProcessId = '" + processId + "'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
            else
            {
                string CmdText = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
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
								   WHERE PO.EntityId='" + entityid + "' AND PS.UserName = 'Running'";

                return _sqlRepository.GetDataCollection(CmdText);
            }
        }

        public IEnumerable<object> GetSFGSOItem(string entityid, string workCenterMasterId, string productionLevel, string processId, string status, bool IsFirst)
        {
            if (productionLevel != "ProductionOrder")
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
                                WHERE PS.UserName = 'Running'	AND POSP.ProcessId = '" + processId + "'";

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
                                WHERE PS.UserName = 'Running'	AND POSP.ProcessId = '" + processId + "'";

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
	                                FROM [TRN].[ProductionSummary] PS WHERE PS.FromSFGInventoryId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.FromSFGInventoryId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.FromSFGInventoryId = '" + processId + @"'
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
                                WHERE PS.UserName = 'Running'	";

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
	                                FROM [TRN].[ProductionSummary] PS WHERE PS.FromSFGInventoryId = '" + processId + @"' GROUP BY PS.SalesOrderId,PS.FromSFGInventoryId
	                                ) AS PRS ON PRS.SalesOrderId = SO.Id AND PRS.FromSFGInventoryId = '" + processId + @"'
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
                                WHERE PS.UserName = 'Running'	";

                        return _sqlRepository.GetDataCollection(CmdText);
                    }
                }
            }
            else
            {
                if (IsFirst == true)
                {
                    string CmdText = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,ISNULL(PD.BuyerOrder,'') BuyerOrder,ISNULL(PD.OwnOrder,'') OwnOrder,ISNULL(PD.BuyerItem,'') BuyerItem,ISNULL(PD.OwnItem,'') OwnItem,PD.Description,PD.PONumber,PD.MaterialMaster,PD.Article
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory,mm.UserName MaterialMaster,ISNULL(mma.StandardName, '') Article
								   
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
								   WHERE PS.UserName = 'Running'";

                    return _sqlRepository.GetDataCollection(CmdText);
                }
                else
                {
                    string CmdText = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,ISNULL(PD.BuyerOrder,'') BuyerOrder,ISNULL(PD.OwnOrder,'') OwnOrder,ISNULL(PD.BuyerItem,'') BuyerItem,ISNULL(PD.OwnItem,'') OwnItem,PD.Description,PD.PONumber,PD.MaterialMaster,PD.Article
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory,mm.UserName MaterialMaster,ISNULL(mma.StandardName, '') Article
								   
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
								   WHERE PS.UserName = 'Running'";

                    return _sqlRepository.GetDataCollection(CmdText);
                }
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
                                ,WCM.UserName FromWorkCenterMaster,TWCM.UserName ToWorkCenterMaster,ISNULL(FP.UserName,FSFG.UserName) [From], ISNULL(TP.UserName,TSFG.UserName) [To],P.ToEntityId
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
								 and p.ProductionDate='" + ProductionDate + @"' " + wc + " ";

                    return _sqlRepository.GetDataCollection(_sql, null);

                }
                else
                {
                    string _sql = @"SELECT P.Id,P.ProductionOrderId,FORMAT(P.ProductionDate,'dd-MMM-yyyy') ProductionDate, P.ProductionGrade, P.Quantity, PBP.UserName ProductionBookingPeriod
                                 ,P.ResponsiblePersonId,R.EmployeeName ResponsiblePersonName,P.MentorId, M.EmployeeName MentorName, P.CheckedBy,C.EmployeeName CheckedByName
                                 ,FORMAT (P.InTime, 'dd-MMM-yyyy hh:mm:tt') InTime, FORMAT (P.OutTime, 'dd-MMM-yyyy hh:mm:tt') OutTime,P.ConsumeHour,P.ManPower
                                 ,P.ToWorkCenterMasterId,P.FromSFGInventoryId,P.ToSFGInventoryId,P.ToProcessId,P.Remarks,P.WorkCenterMasterId,P.LotNumber
                                 ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,WCM.UserName FromWorkCenterMaster,TWCM.UserName ToWorkCenterMaster,ISNULL(FP.UserName,FSFG.UserName) [From], ISNULL(TP.UserName,TSFG.UserName) [To],p.SalesOrderId,P.ToEntityId
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
								 and P.ProductionDate='" + ProductionDate + @"'  " + wc + " ";
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
                if (ProductionLevel != "ProductionOrder")
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
                                ,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem,so.Description
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
                else
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
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSFGWIPQty(string EntityId, string processId, string workCenterMasterId, string salesOrderId, string productionOrderId, string status,bool IsCrossAllowed)
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

                if (IsCrossAllowed==false)
                {
                    if (status == "PROCESS")
                    {
                        sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                                (SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToProcessId ProcessId,PS.SalesOrderId
                                FROM trn.ProductionSummary AS ps
                                WHERE ps.ToProcessId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND ps.ToWorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
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
                               WHERE ps.ToSFGInventoryId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"')
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

            if (IsCrossAllowed==false)
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
            if (IsCrossAllowed==false)
            {
                if (status == "PROCESS")
                {
                    sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                            (
                            SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToProcessId ProcessId,PS.SalesOrderId
                            FROM trn.ProductionSummary AS ps
                            WHERE ps.ToProcessId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND ps.ToWorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
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
                               WHERE ps.ToSFGInventoryId='" + processId + @"' AND PS.EntityId='" + EntityId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
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
            }
            else
            {
                if (status == "PROCESS")
                {
                    sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                            (
                            SELECT ps.ProductionOrderId,PS.ToWorkCenterMasterId AS WorkCenterMasterId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.ToProcessId ProcessId,PS.SalesOrderId
                            FROM trn.ProductionSummary AS ps
                            WHERE ps.ToProcessId='" + processId + @"' AND ps.ToWorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
                            union all
                            SELECT ps.ProductionOrderId,PS.WorkCenterMasterId,0 AS InQuantity, Quantity,0 AS KillQuantity,PS.ProcessId,PS.SalesOrderId
                            FROM trn.ProductionSummary AS ps
                            WHERE ps.ProcessId='" + processId + @"' AND ps.WorkCenterMasterId='" + workCenterMasterId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
                            ) AS K ";

                }
                else
                {
                     sql = @"SELECT ISNULL(SUM(InQuantity),0) AS InQuantity,ISNULL(SUM(OutQuantity),0) AS OutQuantity,ISNULL(SUM(KillQuantity),0) AS KillQuantity, WIP=(ISNULL(SUM(InQuantity)-SUM(OutQuantity)-SUM(KillQuantity),0)) FROM
                               (SELECT ps.ProductionOrderId,ps.Quantity AS InQuantity,0 AS OutQuantity,0 AS KillQuantity,PS.FromSFGInventoryId,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.ToSFGInventoryId='" + processId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
                               union all

                               SELECT ps.ProductionOrderId,0 AS InQuantity,case when ps.ProductionGrade='A' THEN Quantity else 0 END AS OutQuantity,0 AS KillQuantity,PS.FromSFGInventoryId ,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.FromSFGInventoryId='" + processId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
                               union all

                               SELECT ps.ProductionOrderId,0 AS InQuantity,0 AS OutQuantity,case when ps.ProductionGrade<>'A' THEN Quantity else 0 END AS KillQuantity,PS.FromSFGInventoryId,PS.SalesOrderId
                               FROM trn.ProductionSummary AS ps
                               WHERE ps.FromSFGInventoryId='" + processId + @"' AND (ISNULL(ps.SalesOrderId,'')='" + salesOrderId + @"' OR ISNULL(ps.ProductionOrderId,'')='" + productionOrderId + @"') and ps.id<>'" + Id + @"'
                               ) AS K ";
                }
            }
            return _sqlRepository.GetData(sql);
        }

        public IEnumerable<object> GetTotalPOQty(string productionOrderId, string processId)
        {
            try
            {
                var sql = @" SELECT CEILING(SUM(ISNULL(PO.PlannedQty,0))) PlannedQty
                        ,ISNULL(CEILING(SUM(PO.PlannedQty) - ISNULL(CEILING(PRS.TotalProductionQty),0)),0) RemainingQty, ISNULL(CEILING(PRS.TotalProductionQty),0)TotalProductionQty
                         FROM trn.ProductionOrder AS PO
                         LEFT JOIN (SELECT SUM(PS.Quantity) TotalProductionQty,PS.ProductionOrderId
	                     FROM [TRN].[ProductionSummary] PS WHERE PS.ProcessId = '" + processId + @"' GROUP BY PS.ProductionOrderId
	                     ) AS PRS ON PRS.ProductionOrderId = PO.Id WHERE PO.Id ='" + productionOrderId + @"' GROUP BY TotalProductionQty";
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
                    var sql = @"SELECT  PlannedQty=CASE WHEN S.Qty=0 THEN CEILING(SUM(PO.PlannedQty)) ELSE S.Qty END
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

        public IEnumerable<object> GetProductionOrderDataList()
        {
            string CmdText = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
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

        public IEnumerable<object> GetEntityProcessSettingData(string EntityId)
        {
            string sql = @"Select ProcessNature,EntityId,IsPackingSKURequired,PackingForm from HKP.EntityProcessTag Where ProcessNature= 'Packing' AND EntityId='" + EntityId + "'";
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
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=A.ProductionOrderId AND EPT.ProcessNature='Packing') 
							  ,IsPackingSKURequired=(Select EPT.IsPackingSKURequired FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=A.ProductionOrderId AND EPT.ProcessNature='Packing') 
                              ,PackingForm=(Select EPT.PackingForm FROM TRN.ProductionOrder PO 
							                    LEFT JOIN HKP.EntityProcessTag EPT ON EPT.EntityId=PO.EntityId WHERE PO.Id=A.ProductionOrderId AND EPT.ProcessNature='Packing')
                            ,B.NoOfQty,C.NoOfLine,TQ=B.NoOfQty*C.NoOfLine, ISNULL(D.Confirmed,0) Confirmed, Balance=C.NoOfLine- ISNULL(D.Confirmed,0),0 RecvQty 
                            FROM [dbo].[PackingContentMaster] A 
							LEFT JOIN (select SUM(Qty) NoOfQty,PackingContentMasterId FROM [dbo].[PackingContentDetail] GROUP BY PackingContentMasterId) B ON B.PackingContentMasterId=A.Id 
							LEFT JOIN (select COUNT(Id) NoOfLine,PackingContentMasterId FROM [dbo].[PackingChild] GROUP BY PackingContentMasterId) C ON C.PackingContentMasterId=A.Id 
                            LEFT JOIN (SELECT Count(Id) Confirmed,PackingContentMasterId FROM [dbo].[PackingChild] WHERE  IsConfirmed=1 GROUP BY PackingContentMasterId) D ON D.PackingContentMasterId=A.Id";
            return _sqlRepository.GetDataCollection(sql);
        }
    }

}


