#region using
using Aplos.Controllers;
using Library.Model.Materials;
using Aplos.Properties;
using Library.Data;
using Library.Service.Materials;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System.Collections.Generic;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Library.Security.Core;
using Library.MaterialManagement.Inventory;
using Library.Model.Inventory;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using Newtonsoft.Json;
using Library.MaterialManagement.Products;
using Library.Model.Products;
#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class MaterialIssueControlController : BaseController
    {
        #region -- Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IIssueRequestService _issueRequestService;
        public MaterialIssueControlController(ISqlRepository R, IIssueRequestService issueRequestService)
        {
            _sqlRepository = R;
            _issueRequestService = issueRequestService;
        }
        #endregion

        #region Pages

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult Approval()
        {
            return View();
        }
        public ActionResult Issue()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult EntityList()
        {
            string sql = @"";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (identity.IsSysAdmin)
            {
                sql = @"SELECT distinct E.Id Value,E.PlantId,P.UserName AS PlantName,e.Code,e.UserName AS Text
                        FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                            LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                            WHERE  e.Id IN (
                        SELECT ept.EntityId FROM hkp.EntityProcessTag AS ept WHERE ept.ProcessId IN (SELECT pt.BaseProcessId FROM PlanningTypes AS pt)) AND E.[Active]=1 AND e.CompanyId='" + identity.CompanyId + @"'
                        ORDER BY e.Code";

                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }

            sql = @"SELECT  distinct E2.Id Value,e2.PlantId,P.UserName AS PlantName,e2.Code,e2.UserName AS Text  FROM [SEC].[UserEntity] E
                        LEFT JOIN org.Entity AS e2 ON e2.Id=e.EntityId
                        LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E2.Id
                        LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                        LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                        WHERE E.UserId='" + identity.UserId + @"' AND e2.Id IN (
                        SELECT ept.EntityId FROM hkp.EntityProcessTag AS ept WHERE ept.ProcessId IN (SELECT pt.BaseProcessId FROM PlanningTypes AS pt)) AND E2.[Active]=1 ORDER BY E2.Code";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedUnApprovedData()
        {
            try
            {
                string sql = @"SELECT M.*,E.EmployeeName ByWhom,EN.UserName Entity,MS.UserName MaterialStorage FROM [dbo].[MaterialIssueControlMaster] M
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=M.ByWhomId
                            LEFT JOIN ORG.Entity EN ON EN.Id=M.EntityId
                            LEFT JOIN HKP.MaterialStorage MS ON MS.Id=M.MaterialStorageId";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetApprovedData()
        {
            try
            {
                string sql = @"SELECT M.*,E.EmployeeName ByWhom,EN.UserName Entity,MS.UserName MaterialStorage FROM [dbo].[MaterialIssueControlMaster] M
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=M.ByWhomId
LEFT JOIN ORG.Entity EN ON EN.Id=M.EntityId
LEFT JOIN HKP.MaterialStorage MS ON MS.Id=M.MaterialStorageId Where M.IsApproved=1";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedSODetailData(string masterId)
        {
            try
            {
                string sql = @"SELECT DISTINCT mo.MasterOrderNo,moi.Id LineItemId,MIS.Id
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
									,mma.Id ArticleId
	                                ,ISNULL(mma.StandardName, '') SOArticle
									,b.Id CustomerId
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
                                    ,CBI.CostingBOQMasterId
                                    ,moi.OrderCostingMasterTemplateId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
									,PC.UserName AS ProductCategory
	                                ,CEILING(SO.PlannedQty) PlannedQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem

									,DMC.[Value] ItemMaterialCost,SDMC.[SOValue] SOMaterialCost,CMC.TotalGrossAmount CostingMaterialCost

									,ISNULL(QBOQ.BOQMaterialCost,0) BOQMaterialCost,SOTotalMaterailCost=CEILING(SO.PlannedQty)*SDMC.[SOValue]

									,CostingTotalMaterialCost=CMC.TotalGrossAmount*CEILING(SO.PlannedQty),BOQTotalCost=ISNULL(QBOQ.BOQMaterialCost,0)*CEILING(SO.PlannedQty)

									,TotalVarianceCostingVsSO=(CEILING(SO.PlannedQty)*SDMC.[SOValue])-(CMC.TotalGrossAmount*CEILING(SO.PlannedQty))

									,TotalVarianceCostingVsBOQ=ISNULL(QBOQ.BOQMaterialCost,0)*CEILING(SO.PlannedQty)-CMC.TotalGrossAmount*CEILING(SO.PlannedQty)
                                    ,MIS.PlanRate,MIS.PlantCost,MIS.TotalSOCostVsTotalPlanCost
                                FROM [dbo].[MaterialIssueControlSODetail] MIS 
								 LEFT JOIN TRN.ProductionOrderDetail POD ON POD.SalesOrderId=MIS.SOId
                               LEFT JOIN (
	                                SELECT SUM((isnull(qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) AS PlannedQty
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description
	                                ) so ON POD.SalesOrderId = SO.Id
                                LEFT JOIN TRN.[MasterOrderItem] moi ON moi.id = so.MasterOrderItemId
                                LEFT JOIN TRN.MasterOrder mo ON mo.id = moi.MasterOrderId
                                LEFT JOIN [dbo].[CostingBOQItems] CBI ON CBI.SalesOrderId=SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
								left join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
								LEFT JOIN(SELECT distinct MI.Id,MC.UserName,MC.[Value] 
														FROM  dbo.MasterOrderItemCostingRate MC 
														LEFT JOIN dbo.OrderLineCostingItem OLC ON OLC.Id=MC.OrderLineCostingItemId
														LEFT JOIN TRN.MasterOrderItem MI ON MI.Id=MC.MasterOrderItemId
														WHERE OLC.SOItemName='DirectMaterialCost') DMC ON DMC.Id=MOI.Id
								LEFT JOIN(SELECT distinct MC.SalesOrderId,MC.UserName,MC.[SOValue] 
														FROM  dbo.SOCostingConfirmation MC 
														LEFT JOIN dbo.OrderLineCostingItem OLC ON OLC.Id=MC.OrderLineCostingItemId
														WHERE OLC.SOItemName='DirectMaterialCost') SDMC ON SDMC.SalesOrderId=so.Id
LEFT JOIN(SELECT pc.OrderCostingMasterTemplateId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc  
INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
GROUP BY PC.OrderCostingMasterTemplateId) CMC ON CMC.OrderCostingMasterTemplateId=MOI.OrderCostingMasterTemplateId

LEFT JOIN (SELECT SUM((Q.MaterialCostPerUnit*Q.GrossConsumption))BOQMaterialCost,Q.MasterOrderItemId FROM [dbo].[QuickBOQ] Q
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial' GROUP BY Q.MasterOrderItemId) QBOQ ON QBOQ.MasterOrderItemId=moi.Id

                                WHERE MIS.MaterialIssueControlMasterId='" + masterId + "'";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedDetailData(string masterId)
        {
            try
            {
                string sql = @"SELECT ROW_NUMBER() OVER(ORDER BY D.Id) SrNo
                ,D.Id MaterialIssueControlDetailId,D.MaterialIssueControlMasterId,D.CostingItemId,D.NetConsumptionPerUnit,D.ValueLoss,D.GrossConsumption,D.TotalConsumption,D.AdditionReduction
				,D.PlanConsumption,D.Rate,D.TotaPlanlAmount,ISNULL(IR.IssueQty,0) IssueQty,D.ArticleId,D.MaterialMasterId,D.StockRate,D.ActualIssueAmount,D.Remarks,D.AddedBy,D.AddedDate,D.AddedFromIP
				,I.UserName Item,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId
                ,M.UserName MaterialMaster,um.Code as UoM, um.Id as UoMId, BaseUoMFactor=case when M.BaseUOMId=i.UnitOfMeasurementId then 1 else 1 end
                ,B.UserName BudgetName,ACT.UserName ActivityName,BM.Id BudgetMasterId,BM.GLGeneralInfoId,ACT.Id ExpenseActivityId,M.MaterialGroupMasterId
                ,'' CostCenterName,''CostCenterId,'' Id,'' RequestedQty
                FROM dbo.MaterialIssueControlDetail D 
                INNER JOIN HKP.CostingItem I on i.Id=D.CostingItemId
                left join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
                LEFT JOIN MST.MaterialMaster M ON M.Id=D.MaterialMasterId
                LEFT JOIN MST.MaterialMasterArticle A ON A.Id=D.ArticleId
                LEFT JOIN HKP.MaterialGroupGL MGGL ON MGGL.Id=M.MaterialGroupMasterId
                LEFT JOIN MST.BudgetMaster BM ON BM.Id=MGGL.InventoryBudgetMasterId
                LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                LEFT JOIN HKP.Activity ACT ON ACT.Id=MGGL.InventoryActivityId
				LEFT JOIN (SELECT MaterialIssueControlDetailId, SUM(ISNULL(RequestedQty,0)) IssueQty,TransactionUoMId 
							FROM TRN.IssueRequest GROUP BY MaterialIssueControlDetailId,TransactionUoMId)IR ON IR.MaterialIssueControlDetailId=D.Id
                WHERE D.MaterialIssueControlMasterId='" + masterId + "'";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string entityid, string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            string sql = @"select * from (
                        SELECT  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName,
                        ISNULL(PO.Qty,0) AS POQuantity,ISNULL(PO.PlannedQty,0) AS SOQuantity,ISNULL(SO.Qty,0) AS SavedQuantity,t1.Color,FORMAT(t1.LSD,'dd-MMM-yyyy') AS LSD,FORMAT(t1.CommitmentDate,'dd-MMM-yyyy') AS CommitmentDate,t1.TargetPerDay
                                ,T1.ProductionPriority ,so.Material, so.Product,t1.Qty,
                                so.ProductCategory, so.FirstShipmentDate,
                                so.LastShipmentDate, so.buyer, so.BuyerRefNo,
                                so.OwnRefNo, so.StyleNo, so.OwnStyleNo, so.SONo,
                                so.SODesc,So.MasterOrderId,
                                so.Customer,so.article,PRODPR.ProductionQtyAtPR 
                                   ,ISNULL(CASE WHEN ISNULL(T1.Qty,0)>0 THEN T1.Qty ELSE PO.PlannedQty END,0)-(ISNULL(PRODPR.ProductionQtyAtPR,0)-ISNULL(PRDQ.ProductionBookedQty,0)) AS ToBePlanQty                                  			
								,SOCount=(Select  Count (S.Id) From TRN.SalesOrder S 
								INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=s.Id
								Where pod.ProductionOrderId=po.Id)
								,ItemCount=(Select  Count (M.Id) From TRN.MasterOrderItem M
								INNER JOIN TRN.SalesOrder S ON S.MasterOrderItemId=M.Id
								INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=s.Id
								Where pod.ProductionOrderId=po.Id)
                            FROM [TRN].[ProductionOrder] AS PO
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN ProductionOrderSchedulingParametersType1 t1 ON t1.ProductionOrderID=po.Id

                             LEFT OUTER JOIN (
												SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR,MIN(s.ProductionDate) AS ProductionStartDateAtPR
											FROM  trn.ProductionSummary S 
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PRODPR ON  PRODPR.ProductionOrderId=po.id AND PRODPR.ProcessId=(select ProcessId from trn.ProductionOrderProcessSet where IsBaseProcess=1 and ProductionOrderID=po.Id)
							 left outer join (SELECT pod.ProductionOrderId,
                                sum(isnull(so.ProductionBookedQty,0)) ProductionBookedQty
                                 FROM trn.SalesOrder AS so
                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id

                                GROUP BY pod.ProductionOrderId
                            ) AS PRDQ ON PRDQ.ProductionOrderId=T1.ProductionOrderId
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, PM.UserName AS Product,pc.UserName AS ProductCategory,
                                                     min(so.DeliveryDate) AS FirstShipmentDate,  max(so.DeliveryDate) AS LastShipmentDate,
                                                    sum(so.Qty) AS Qty,
                                                    MasterOrderId =STUFF((select distinct ','+XMOI.Id from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
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
													LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                    group by pod.ProductionOrderId,mm.userName,ma.StandardName,PM.UserName,pc.UserName) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            WHERE PO.entityid='" + entityid + @"' AND S.UserName<>'Closed' AND PO.Id NOT IN(Select POId from dbo.MaterialIssueControlMaster)) AS TEMP WHERE " + strkey + " ORDER BY ProductionPriority";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSOItemList(string entityid, string ProductionOrderId)
        {
            string CmdText = @"SELECT DISTINCT mo.MasterOrderNo,moi.Id LineItemId,PM.Id
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
	                                ,CPO.PONumber
	                                ,mm.Id MaterialMasterId
	                                ,mm.UserName MaterialMaster
									,mma.Id ArticleId
	                                ,ISNULL(mma.StandardName, '') SOArticle
									,b.Id CustomerId
	                                ,b.UserName Customer
	                                ,mo.TotalQty MOQty
	                                ,ISNULL(u.UserName, '') UOM
	                                ,moi.ExtraOrderPercentage [ExtraP]
	                                ,moi.OrderWastagePercentage [WastageP]
	                                ,ISNULL(mma.Id, '') ArticleId
	                                ,mmc.CharCount
	                                ,ISNULL(POD.ProductionOrderId, '') POId
                                    ,CBI.CostingBOQMasterId
                                    ,moi.OrderCostingMasterTemplateId
	                                ,B.UserName Buyer
	                                ,PM.UserName AS ProductMasterName
									,PC.UserName AS ProductCategory
	                                ,CEILING(SO.PlannedQty) PlannedQty
                                    ,SO.Description,MO.BuyerReferenceNo BuyerOrder,MO.OwnReferenceNo OwnOrder,moi.BuyerReferenceNo BuyerItem,moi.OwnReferenceNo OwnItem

									,(DMC.[Value]*R.ExchangeRate) ItemMaterialCost,(SDMC.[SOValue]*R.ExchangeRate) SOMaterialCost,CMC.TotalGrossAmount CostingMaterialCost

									,ISNULL(QBOQ.BOQMaterialCost,0) BOQMaterialCost,SOTotalMaterailCost=CEILING(SO.PlannedQty)*SDMC.[SOValue]

									,CostingTotalMaterialCost=CMC.TotalGrossAmount*CEILING(SO.PlannedQty),BOQTotalCost=ISNULL(QBOQ.BOQMaterialCost,0)*CEILING(SO.PlannedQty)

									,TotalVarianceCostingVsSO=(CEILING(SO.PlannedQty)*SDMC.[SOValue])-(CMC.TotalGrossAmount*CEILING(SO.PlannedQty))

									,TotalVarianceCostingVsBOQ=ISNULL(QBOQ.BOQMaterialCost,0)*CEILING(SO.PlannedQty)-CMC.TotalGrossAmount*CEILING(SO.PlannedQty)
                                    ,0 PlanRate,0 PlantCost,0 TotalSOCostVsTotalPlanCost
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
                                LEFT JOIN [dbo].[MasterOrderExchangeRates] R ON R.TransactionId=mo.Id
                                LEFT JOIN [dbo].[CostingBOQItems] CBI ON CBI.SalesOrderId=SO.Id
                                LEFT JOIN HKP.Party b ON b.id = mo.PartyId
                                LEFT JOIN SCS.UnitOfMeasurement u ON u.id = mo.TotalQtyUOMId
                                LEFT JOIN MST.MaterialMaster mm ON mm.id = moi.MaterialMasterId
                                LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
                                LEFT JOIN (SELECT COUNT(Id) CharCount, MaterialMasterId	FROM [MST].[MaterialMasterCharacteristics] GROUP BY MaterialMasterId
	                                ) mmc ON mmc.MaterialMasterId = mm.id
                                LEFT JOIN HKP.Buyer BU ON BU.Id = mo.BuyerId
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId = MM.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
								left join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
                                LEFT JOIN (SELECT PS.UserName, PO.Id ProductionOrderId FROM [HKP].[ProductionStatus] PS
	                                INNER JOIN TRN.ProductionOrder PO ON PO.ProductionStatusId = PS.Id
	                                ) OS ON OS.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN TRN.ProductionOrder PO ON PO.Id = POD.ProductionOrderId
                                LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id = PO.ProductionStatusId
                                LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = POD.ProductionOrderId
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
								LEFT JOIN(SELECT distinct MI.Id,MC.UserName,MC.[Value] 
														FROM  dbo.MasterOrderItemCostingRate MC 
														LEFT JOIN dbo.OrderLineCostingItem OLC ON OLC.Id=MC.OrderLineCostingItemId
														LEFT JOIN TRN.MasterOrderItem MI ON MI.Id=MC.MasterOrderItemId
														WHERE OLC.SOItemName='DirectMaterialCost') DMC ON DMC.Id=MOI.Id
								LEFT JOIN(SELECT distinct MC.SalesOrderId,MC.UserName,MC.[SOValue] 
														FROM  dbo.SOCostingConfirmation MC 
														LEFT JOIN dbo.OrderLineCostingItem OLC ON OLC.Id=MC.OrderLineCostingItemId
														WHERE OLC.SOItemName='DirectMaterialCost') SDMC ON SDMC.SalesOrderId=so.Id
LEFT JOIN(SELECT pc.OrderCostingMasterTemplateId,SUM(pc.GrossAmount)AS TotalGrossAmount FROM OrderProcurementCostingDirectMaterial AS pc  
INNER JOIN HKP.CostingItem I on i.Id=PC.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
GROUP BY PC.OrderCostingMasterTemplateId) CMC ON CMC.OrderCostingMasterTemplateId=MOI.OrderCostingMasterTemplateId

LEFT JOIN (SELECT SUM((Q.MaterialCostPerUnit*Q.GrossConsumption))BOQMaterialCost,Q.MasterOrderItemId FROM [dbo].[QuickBOQ] Q
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial' GROUP BY Q.MasterOrderItemId) QBOQ ON QBOQ.MasterOrderItemId=moi.Id

                                WHERE PO.EntityId = '" + entityid + @"' AND PS.UserName<>'Closed' AND PO.Id='" + ProductionOrderId + "'";


            return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMOIItemList(string entityid, string ProductionOrderId)
        {
            string CmdText = @"SELECT DISTINCT mo.MasterOrderNo,so.MasterOrderItemId
	                                ,ISNULL(so.Id,'') SOId
									,moi.OrderCostingMasterTemplateId
									,CBI.CostingBOQMasterId
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
                                LEFT JOIN [dbo].[CostingBOQItems] CBI ON CBI.SalesOrderId=SO.Id
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
                                LEFT JOIN [SCS].[WorkCenterMasterProductPriority] WC ON WC.ProductMasterId = PM.Id
                                LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = SO.CustomerPOId
                                WHERE PO.EntityId = '" + entityid + @"'	AND PS.UserName = 'Running' AND PO.Id='" + ProductionOrderId + "'";
            return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> model, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList)
        {
            try
            {
                SaveData(model, soList, dataList);
                return Json(new { Data = model, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        private void SaveData(Dictionary<string, object> data, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild, dsSOChild;
            string _Id = string.Empty;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlMaster WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaterialIssueControlMaster", out _Id);

                    data["Id"] = "M" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #region MaterialIssueControlSODetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlSODetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsSOChild, false, "1");
                int socount = 0;
                if (soList != null)
                {
                    foreach (var item in soList)
                    {
                        socount++;
                        DataView dv = new DataView(dsSOChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id + "-" + socount;
                            item["MaterialIssueControlMasterId"] = _Id;
                            item["SOQty"] = item["PlannedQty"];

                            AddNewRow(dsSOChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                #region MaterialIssueControlDetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlDetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsChild, false, "1");
                int ccount = 0;
                if (dataList != null)
                {
                    foreach (var item in dataList)
                    {
                        ccount++;
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id + "-" + ccount;
                            item["MaterialIssueControlMasterId"] = _Id;
                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsSOChild, dsChild);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult CreateApprove(Dictionary<string, object> model, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList)
        {
            try
            {
                SaveApproveData(model, soList, dataList);
                return Json(new { Data = model, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        private void SaveApproveData(Dictionary<string, object> data, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild, dsSOChild;
            string _Id = string.Empty;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlMaster WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaterialIssueControlMaster", out _Id);

                    data["Id"] = "M" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["IsApproved"] = true;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #region MaterialIssueControlSODetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlSODetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsSOChild, false, "1");
                int socount = 0;
                if (soList != null)
                {
                    foreach (var item in soList)
                    {
                        socount++;
                        DataView dv = new DataView(dsSOChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id + "-" + socount;
                            item["MaterialIssueControlMasterId"] = _Id;
                            item["SOQty"] = item["PlannedQty"];

                            AddNewRow(dsSOChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                #region MaterialIssueControlDetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlDetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsChild, false, "1");
                int ccount = 0;
                if (dataList != null)
                {
                    foreach (var item in dataList)
                    {
                        ccount++;
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";


                        if (dv.Count == 0)
                        {
                            item["Id"] = _Id + "-" + ccount;
                            item["MaterialIssueControlMasterId"] = _Id;
                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsSOChild, dsChild);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult CreateIssue(Dictionary<string, object> model, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList, List<IssueRequestViewModel> dataLists)
        {
            try
            {
                IssueRequestMaster inventoryIssue = new IssueRequestMaster();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
                inventoryIssue.CompanyId = identity.CompanyId;
                inventoryIssue.PlantId = identity.PlantId;
                inventoryIssue.CheckedBy = model["CheckedBy"].ToString(); 
                inventoryIssue.CompanyGroupId = identity.CompanyGroupId;
                inventoryIssue.CompanyId = identity.CompanyId;
                inventoryIssue.PlantId = identity.PlantId;
                inventoryIssue.Orderspecific = "No";
                inventoryIssue.IssueSlipType = "InventorySlip";
                inventoryIssue.Preparedby = model["ByWhomId"].ToString();
                inventoryIssue.ProductionOrderId = model["POId"].ToString();

                SaveIssueData(model, soList, dataList);
                List<IssueRequestViewModel> entityDetailVM = dataLists;
                List<IssueRequestViewModel> entityGroupDataVM = dataLists;
                List<IssueRequestViewModel> SOListSelectedNewDetailVM = null;
                List<IssueRequestViewModel> MaterialColorListNewDetailVM = null;
                
                _issueRequestService.InsertOrUpdateGraphIssueSlipCreate(inventoryIssue, entityDetailVM, entityGroupDataVM, inventoryIssue.IssueSlipType, null, null, SOListSelectedNewDetailVM, MaterialColorListNewDetailVM, null);
                return Json(new { Data = model, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        //[Authorize, HttpGet]
        //public JsonResult GetInventoryIssueByProductionOrder(string productionOrderId)
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    var jsondata = Json(_inventoryIssueService.GetInventoryIssueByProductionOrder(identity.PlantId, productionOrderId), JsonRequestBehavior.AllowGet);
        //    jsondata.MaxJsonLength = int.MaxValue;
        //    return jsondata;
        //}

        private void SaveIssueData(Dictionary<string, object> data, List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild, dsSOChild;
            string _Id = string.Empty;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlMaster WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MaterialIssueControlMaster", out _Id);

                    data["Id"] = "M" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["IsApproved"] = true;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                //#region MaterialIssueControlSODetail 
                //objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlSODetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsSOChild, false, "1");
                //int socount = 0;
                //if (soList != null)
                //{
                //    foreach (var item in soList)
                //    {
                //        socount++;
                //        DataView dv = new DataView(dsSOChild.Tables[0]);
                //        dv.RowFilter = "Id='" + item["Id"] + "'";


                //        if (dv.Count == 0)
                //        {
                //            item["Id"] = _Id + "-" + socount;
                //            item["MaterialIssueControlMasterId"] = _Id;
                //            item["SOQty"] = item["PlannedQty"];

                //            AddNewRow(dsSOChild.Tables[0], item);
                //        }
                //        else
                //        {
                //            DataRow drmo = dv[0].Row;
                //            EditRow(drmo, item);
                //        }
                //    }
                //}

                //#endregion

                //#region MaterialIssueControlDetail 
                //objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialIssueControlDetail where  MaterialIssueControlMasterId='" + _Id + "'", out dsChild, false, "1");
                //int ccount = 0;
                //if (dataList != null)
                //{
                //    foreach (var item in dataList)
                //    {
                //        ccount++;
                //        DataView dv = new DataView(dsChild.Tables[0]);
                //        dv.RowFilter = "Id='" + item["Id"] + "'";


                //        if (dv.Count == 0)
                //        {
                //            item["Id"] = _Id + "-" + ccount;
                //            item["MaterialIssueControlMasterId"] = _Id;
                //            AddNewRow(dsChild.Tables[0], item);
                //        }
                //        else
                //        {
                //            DataRow drmo = dv[0].Row;
                //            EditRow(drmo, item);
                //        }
                //    }
                //}

                //#endregion
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                //_fgzoneService.Archive(id);
                return null;// Json(new { Sequence = _fgzoneService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpGet, Authorize]
        public ActionResult GetCostingDataList(string soId)
        {
            string CmdText = @"SELECT ROW_NUMBER() OVER(ORDER BY Q.Sequence) SrNo,NULL Id,I.Id CostingItemId,I.UserName Item,um.Code as UoM, um.Id as UoMId,Q.Consumption NetConsumptionPerUnit,Q.ValueLoss,Q.GrossConsumption,
 TotalConsumption=Q.GrossConsumption*SO.Qty,SO.Qty, 0 AdditionReduction,0 PlanConsumption
 ,Q.Rate,0 TotaPlanlAmount,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId,M.UserName MaterialMaster,ISNULL(SR.StockRate,0)StockRate,0 ActualIssueAmount, NULL Remarks
 FROM OrderProcurementCostingDirectMaterial AS Q  
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
left join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
left join TRN.MasterOrderItem MOI ON MOI.OrderCostingMasterTemplateId=Q.OrderCostingMasterTemplateId
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
left join MST.MaterialMasterArticle A ON A.Id=MOI.ArticleId
left join MST.MaterialMaster M ON M.Id=MOI.MaterialMasterId
LEFT JOIN (
Select StockRate= SUM(IRD.MaterialTranRate)/COUNT(IRD.Id),IM.MaterialMasterId,IM.ArticleId 
from TRN.InventoryReceiveDetail IRD 
JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
 AND (IRD.BaseQty-IRD.BaseIssueQty)>0
GROUP BY IM.MaterialMasterId,IM.ArticleId )SR ON SR.MaterialMasterId=M.Id AND SR.ArticleId=A.Id
Where SO.Id " + soId + "";
            return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetQBOQDataList(string soId)
        {
            string CmdText = @"SELECT ROW_NUMBER() OVER(ORDER BY Q.Sequence) SrNo,NULL Id,I.Id CostingItemId,I.UserName Item,U.Code UoM,Q.UoMId,Q.NetConsumptionPerUnit,Q.ValueLossPercentage ValueLoss,Q.GrossConsumption,SO.Qty
,TotalConsumption=Q.GrossConsumption*SO.Qty, 0 AdditionReduction,0 PlanConsumption,Q.MaterialCostPerUnit Rate,0 TotaPlanlAmount,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId
,M.UserName MaterialMaster,ISNULL(SR.StockRate,0)StockRate,0 ActualIssueAmount, NULL Remarks
FROM [dbo].[QuickBOQ] Q
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
left join SCS.UnitOfMeasurement U on U.Id=Q.UoMId
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=Q.MasterOrderItemId
left join MST.MaterialMasterArticle A ON A.Id=Q.ArticleId
left join MST.MaterialMaster M ON M.Id=Q.MaterialMasterId
LEFT JOIN (
Select StockRate= SUM(IRD.MaterialTranRate)/COUNT(IRD.Id),IM.MaterialMasterId,IM.ArticleId 
from TRN.InventoryReceiveDetail IRD 
JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
 AND (IRD.BaseQty-IRD.BaseIssueQty)>0
GROUP BY IM.MaterialMasterId,IM.ArticleId ) SR ON SR.MaterialMasterId=M.Id AND SR.ArticleId=A.Id
Where SO.Id " + soId + "";
            return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}