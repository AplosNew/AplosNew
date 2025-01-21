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
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System.Collections.Specialized;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using Library.Service.Helpers;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using Aplos.Areas.Commercial.Controllers;
using Library.Service.Systems;
#endregion

namespace Aplos.Areas.Materials.Controllers
{
    public class RawMaterialPlanningController : BaseController
    {
        #region -- Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IIssueRequestService _issueRequestService;
        private readonly IPKGeneratorService _pkGeneratorService;
        public RawMaterialPlanningController(ISqlRepository R, IIssueRequestService issueRequestService, IPKGeneratorService pkGeneratorService)
        {
            _sqlRepository = R;
            _issueRequestService = issueRequestService;
            _pkGeneratorService = pkGeneratorService;
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

        [Authorize, HttpGet]
        public ActionResult GetEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult LoadPlanDetails(string POID)
        {
            try
            {
                string sql = @"SELECT *,Format(PlanDate,'dd-MMM-yyyy') as FormatPlanDate,(Select EmployeeName from EmployeeInformation where SystemId=PlanById) as PlanBy 
from RawMaterialPlanningMaster where POId='"+ POID + "'";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpGet]
        public ActionResult LoadPlanEditData(string PlanId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT *,Format(PlanDate,'dd-MMM-yyyy') as FormatPlanDate,(Select EmployeeName from EmployeeInformation where SystemId=PlanById) as PlanBy 
from RawMaterialPlanningMaster where Id='" + PlanId + "'";
            return Json(new { plan = _sqlRepository.GetDataCollection(sql, null) }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetConsumptionLevelList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from DefineEnum where Category='Material Planning'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


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
LEFT JOIN HKP.MaterialStorage MS ON MS.Id=M.MaterialStorageId";
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
        public ActionResult GetSavedDetailDataToApprove(string masterId)
        {
            try
            {
                string sql = @"SELECT ROW_NUMBER() OVER(ORDER BY D.Id) SrNo,D.*,D.CostingItemId,I.UserName Item,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId
,M.UserName MaterialMaster,um.Code as UoM, um.Id as UoMId from dbo.MaterialIssueControlDetail D 
INNER JOIN HKP.CostingItem I on i.Id=D.CostingItemId
left join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
LEFT JOIN MST.MaterialMaster M ON M.Id=D.MaterialMasterId
LEFT JOIN MST.MaterialMasterArticle A ON A.Id=D.ArticleId
WHERE D.MaterialIssueControlMasterId='" + masterId + "'";
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
                ,D.Id RawMaterialPlanningDetailId,D.RawMaterialPlanningMasterId,D.CostingItemId,D.NetConsumptionPerUnit,D.ValueLoss,D.GrossConsumption,D.TotalConsumption,D.AdditionReduction
				,D.PlanConsumption,D.Rate,D.TotaPlanlAmount,D.ArticleId,D.MaterialMasterId,D.StockRate,D.ActualIssueAmount,D.Remarks,D.AddedBy,D.AddedDate,D.AddedFromIP
				,I.UserName Item,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId
                ,M.UserName MaterialMaster,um.Code as UoM, um.Id as UoMId, BaseUoMFactor=case when M.BaseUOMId=i.UnitOfMeasurementId then 1 else 1 end
                ,B.UserName BudgetName,ACT.UserName ActivityName,BM.Id BudgetMasterId,BM.GLGeneralInfoId,ACT.Id ExpenseActivityId,M.MaterialGroupMasterId
                ,'' CostCenterName,''CostCenterId,'' Id,'' RequestedQty
                FROM dbo.RawMaterialPlanningDetail D 
                INNER JOIN HKP.CostingItem I on i.Id=D.CostingItemId
                left join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
                LEFT JOIN MST.MaterialMaster M ON M.Id=D.MaterialMasterId
                LEFT JOIN MST.MaterialMasterArticle A ON A.Id=D.ArticleId
                LEFT JOIN HKP.MaterialGroupGL MGGL ON MGGL.Id=M.MaterialGroupMasterId
                LEFT JOIN MST.BudgetMaster BM ON BM.Id=MGGL.InventoryBudgetMasterId
                LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                LEFT JOIN HKP.Activity ACT ON ACT.Id=MGGL.InventoryActivityId
                WHERE D.RawMaterialPlanningMasterId='" + masterId + "'";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetList(Dictionary<string, string> parameters, string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";


            string sql = @"select distinct * from (SELECT  PO.Id,s.UserName AS ProductionStatus,so.SONo,so.BuyerRefNo,ISNULL(PO.Qty,0) AS POQuantity,
                        so.DeliveryDate,so.ProductCode,so.OwnRefNo,so.Customer,PO.AddedDate,PO.EntityId,E.UserName as Entity,'' RMPlanQty,'' RMBalToPlan,'' SQQtyChk,isnull((select top 1 PlanStatus from RawMaterialPlanningMaster where POId=PO.Id order by PO.Id desc),'OnHold') as PlanStatus 
                            FROM [TRN].[ProductionOrder] AS PO                            
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, PM.UserName AS Product,PM.Code as ProductCode,pc.UserName AS ProductCategory,
                                                     sum(so.Qty) AS Qty,SO.PlanExFactoryDate,SO.CommitmentDate,SO.MasterOrderItemId,
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

													DeliveryDate=STUFF((select distinct ','+FORMAT(sox.DeliveryDate,'dd-MMM-yyyy') from 
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
,SOQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pod.ProductionOrderID=Xpod.ProductionOrderId)

                                                      from 
 
                                                     trn.SalesOrder SO 
                                                      JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                                    left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                    left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                    left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
													LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                    group by pod.ProductionOrderId,mm.userName,ma.StandardName,PM.UserName,PM.Code,pc.UserName ,so.DeliveryDate,SO.PlanExFactoryDate,SO.CommitmentDate,SO.MasterOrderItemId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
							Left Outer Join ORG.Entity E ON E.Id=PO.entityid
                            LEFT JOIN RawMaterialPlanningMaster RMP ON RMP.POId=PO.Id
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=RMP.PlanById
                            WHERE (PO.entityid IN(" + parameters["EntityId"] + @") or PO.entityid is null)
            AND (so.Customer IN(" + parameters["Customer"] + @") or  so.Customer is null)
            AND (so.ProductCode IN(" + parameters["ProductCode"] + @") or  so.ProductCode is null)
            AND (so.OwnRefNo IN(" + parameters["OwnRefNo"] + @") or  so.OwnRefNo is null)
            AND (so.BuyerRefNo IN(" + parameters["BuyerRefNo"] + @") or  so.BuyerRefNo is null)  AND S.UserName='Running') AS TEMP WHERE " + strkey + " ORDER BY AddedDate Desc";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetFilterList()
        {
            
            string sql = @"select distinct * from (SELECT  PO.Id,s.UserName AS ProductionStatus,so.SONo,so.BuyerRefNo,ISNULL(PO.Qty,0) AS POQuantity,
                        so.DeliveryDate,so.ProductCode,so.OwnRefNo,so.Customer,PO.AddedDate,PO.EntityId,E.UserName as Entity 
                            FROM [TRN].[ProductionOrder] AS PO                            
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, PM.UserName AS Product,PM.Code as ProductCode,pc.UserName AS ProductCategory,
                                                     sum(so.Qty) AS Qty,SO.PlanExFactoryDate,SO.CommitmentDate,SO.MasterOrderItemId,
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

													DeliveryDate=STUFF((select distinct ','+FORMAT(sox.DeliveryDate,'dd-MMM-yyyy') from 
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
,SOQty=(select SUM((isnull(XSO.qty, 0) * (1 + (isnull(moi.ExtraOrderPercentage, 0) / 100))) * (100 / (100 - isnull(moi.OrderWastagePercentage, 0)))) from 
trn.SalesOrder XSO 
join TRN.MasterOrderItem moi on moi.id=xso.MasterOrderItemId
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
where pod.ProductionOrderID=Xpod.ProductionOrderId)

                                                      from 
 
                                                     trn.SalesOrder SO 
                                                      JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                                    left outer join trn.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                                    left outer join mst.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                                    left outer join trn.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
                                                    left outer join [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                                    left outer join [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
													LEFT OUTER JOIN [MST].[MaterialMasterArticle] MA ON ma.Id=moi.ArticleId
                                                    group by pod.ProductionOrderId,mm.userName,ma.StandardName,PM.UserName,PM.Code,pc.UserName ,so.DeliveryDate,SO.PlanExFactoryDate,SO.CommitmentDate,SO.MasterOrderItemId) AS SO ON so.ProductionOrderId=po.Id
                            LEFT OUTER JOIN hkp.ProductionStatus AS S ON s.Id=po.ProductionStatusId
                            Left Outer Join ORG.Entity E ON E.Id=PO.entityid
            WHERE S.UserName='Running') AS TEMP  ORDER BY AddedDate Desc";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSOItemList(string ProductionOrderId)
        {
            string CmdText = @"SELECT DISTINCT CAST (CASE WHEN RMPD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,RMPD.Remarks,RMP.Id as PlanId,format(RMP.PlanDate,'dd-MMM-yyyy') as PlanDate,mo.MasterOrderNo,moi.Id LineItemId,PM.Id ProductMasterId,RMPD.Id
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
                                    ,format(SO.DeliveryDate,'dd-MMM-yyyy') as DeliveryDate
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
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description,s.DeliveryDate
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description,s.DeliveryDate
	                                ) so ON POD.SalesOrderId = SO.Id
                                left Join [RawMaterialPlanningDetail] RMPD ON RMPD.SOId=so.Id 
                                LEFT JOIN RawMaterialPlanningMaster RMP ON RMP.Id=RMPD.RawMaterialPlanningMasterId
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

                                WHERE PS.UserName='Running' AND PO.Id='" + ProductionOrderId + "' and so.Id not in (select SOId from RawMaterialPlanningDetail)";


            return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSOPlanWiseList(string PlanId)
        {
            string CmdText = @"SELECT DISTINCT CAST (CASE WHEN RMPD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,RMPD.Remarks,RMP.Id as PlanId,format(RMP.PlanDate,'dd-MMM-yyyy') as PlanDate,mo.MasterOrderNo,moi.Id LineItemId,PM.Id ProductMasterId,RMPD.Id
	                                ,ISNULL(so.Id,'') SOId
	                                ,SO.CustomerPOId
                                    ,format(SO.DeliveryDate,'dd-MMM-yyyy') as DeliveryDate
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
		                                ,s.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description,s.DeliveryDate
	                                FROM trn.SalesOrder AS s
	                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id = s.MasterOrderItemId
	                                GROUP BY S.Id,s.MasterOrderItemId,s.CustomerPOId,s.Description,s.DeliveryDate
	                                ) so ON POD.SalesOrderId = SO.Id
                                left Join [RawMaterialPlanningDetail] RMPD ON RMPD.SOId=so.Id 
                                LEFT JOIN RawMaterialPlanningMaster RMP ON RMP.Id=RMPD.RawMaterialPlanningMasterId
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

                                WHERE PS.UserName='Running' and RMP.Id='"+ PlanId + "'";


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
            DataSet dsMaster, dsChild, dsSOChild, dsIdChild;
            string _Id = string.Empty;
            try
            {

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.RawMaterialPlanningMaster WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("RawMaterialPlanningMaster", out _Id);

                    data["Id"] = "R" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();



                #region RawMaterialPlanningDetail 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.RawMaterialPlanningDetail where  RawMaterialPlanningMasterId='" + _Id + "'", out dsSOChild, false, "1");
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
                            item["RawMaterialPlanningMasterId"] = _Id;
                            item["PlannedQty"] = item["PlannedQty"];

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
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MaterialPlanningDetails where  RawMaterialPlanningMasterId='" + _Id + "'", out dsChild, false, "1");
                objCon.OpenDataSetThroughAdapter("SELECT Count(Id)Idc FROM dbo.MaterialPlanningDetails where  RawMaterialPlanningMasterId='" + _Id + "'", out dsIdChild, false, "1");
                int ccount = Convert.ToInt32(dsIdChild.Tables[0].Rows[0]["Idc"].ToString());
                if (dataList != null)
                {
                    foreach (var item in dataList)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            ccount++;

                            string id = _pkGeneratorService.MakePK(_Id, ccount, 2);
                            item["Id"] = id;
                            item["RawMaterialPlanningMasterId"] = _Id;
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
        public JsonResult CreateRMPlan(Dictionary<string, object> model,List<Dictionary<string, object>> soList, List<Dictionary<string, object>> dataList, List<IssueRequestViewModel> dataLists)
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
            string CmdText = @"SELECT ROW_NUMBER() OVER(ORDER BY Q.Sequence) SrNo,CAST (CASE WHEN MPD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,MPD.Remarks,MPD.Id,MPD.ConsumptionLevel,RMP.Id as PlanId,Q.Id as CostingId,I.Id CostingItemId,I.UserName Item,um.Code as UoM, um.Id as UoMId,Q.Consumption NetConsumptionPerUnit,Q.ValueLoss,Q.GrossConsumption,
 TotalConsumption=Q.GrossConsumption*SO.Qty,SO.Qty, 0 AdditionReduction,0 PlanConsumption
 ,Q.Rate,0 TotaPlanlAmount,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId,M.UserName MaterialMaster,ISNULL(SR.StockRate,0)StockRate,0 ActualIssueAmount, NULL Remarks,IM.Id InventoryMaterialId
 FROM OrderProcurementCostingDirectMaterial AS Q  
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
left join [SCS].[UnitOfMeasurement] um on um.Id = i.UnitOfMeasurementId
left join TRN.MasterOrderItem MOI ON MOI.OrderCostingMasterTemplateId=Q.OrderCostingMasterTemplateId
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
left join MST.MaterialMasterArticle A ON A.Id=MOI.ArticleId
left join MST.MaterialMaster M ON M.Id=MOI.MaterialMasterId
left Join [MaterialPlanningDetails] MPD ON MPD.CostingId=Q.Id 
LEFT JOIN RawMaterialPlanningMaster RMP ON RMP.Id=MPD.RawMaterialPlanningMasterId
LEFT JOIN [TRN].[InventoryMaterial] IM ON IM.MaterialMasterId=Q.MaterialMasterId AND IM.ArticleId=Q.ArticleId
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
            string CmdText = @"SELECT ROW_NUMBER() OVER(ORDER BY Q.Sequence) SrNo,CAST (CASE WHEN MPD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,MPD.Remarks,MPD.Id,MPD.ConsumptionLevel,RMP.Id as PlanId,Q.Id as QBOQId,I.Id CostingItemId,I.UserName Item,U.Code UoM,Q.UoMId,Q.NetConsumptionPerUnit,Q.ValueLossPercentage ValueLoss,Q.GrossConsumption,SO.Qty
,TotalConsumption=Q.GrossConsumption*SO.Qty, 0 AdditionReduction,0 PlanConsumption,Q.MaterialCostPerUnit Rate,0 TotaPlanlAmount,A.StandardName QBOQArticle,A.Id ArticleId,M.Id MaterialMasterId
,M.UserName MaterialMaster,ISNULL(SR.StockRate,0)StockRate,0 ActualIssueAmount, NULL Remarks,IM.Id InventoryMaterialId
FROM [dbo].[QuickBOQ] Q
INNER JOIN HKP.CostingItem I on i.Id=Q.CostingItemId
inner join[HKP].[CostingComponent] CC ON CC.Id=I.CostingComponentId AND CC.CostingSegment='DirectMaterial'
left join SCS.UnitOfMeasurement U on U.Id=Q.UoMId
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=Q.MasterOrderItemId
left join MST.MaterialMasterArticle A ON A.Id=Q.ArticleId
left join MST.MaterialMaster M ON M.Id=Q.MaterialMasterId
left Join [MaterialPlanningDetails] MPD ON MPD.QBOQId=Q.Id 
LEFT JOIN RawMaterialPlanningMaster RMP ON RMP.Id=MPD.RawMaterialPlanningMasterId
LEFT JOIN [TRN].[InventoryMaterial] IM ON IM.MaterialMasterId=Q.MaterialMasterId AND IM.ArticleId=Q.ArticleId
LEFT JOIN (
Select StockRate= SUM(IRD.MaterialTranRate)/COUNT(IRD.Id),IM.MaterialMasterId,IM.ArticleId 
from TRN.InventoryReceiveDetail IRD 
JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
 AND (IRD.BaseQty-IRD.BaseIssueQty)>0
GROUP BY IM.MaterialMasterId,IM.ArticleId ) SR ON SR.MaterialMasterId=M.Id AND SR.ArticleId=A.Id
Where SO.Id " + soId + "";
            return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPlanQtyList(string soId, string PlanId)
        {
            string CmdText = @"Select RawMaterialPlanningMasterId as PlanId,MOI.Id as LineItemId,TotalQty as LineItemQty,FC.ValueFreeText as SKU1,SC.ValueFreeText as SKU2,TotalQty+isnull(FC.Qty,0)+isnull(SC.Qty,0) as SumAmount,100 as PlanPercentage,(((TotalQty+isnull(FC.Qty,0)+isnull(SC.Qty,0))*100)/100) as PlanQty,'' Remarks from TRN.MasterOrderItem MOI
left join TRN.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
left join RawMaterialPlanningDetail MPD ON MPD.SOId=SO.Id
left join TRN.FirstCharacteristics FC ON FC.SalesOrderId=SO.Id
left join TRN.SecondCharacteristics SC ON SC.SalesOrderId=SO.Id
Where SO.Id " + soId + " and MPD.RawMaterialPlanningMasterId = '"+ PlanId + "'";
            return Json(_sqlRepository.GetDataCollection(CmdText, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult IssueRequestReport(string mId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IssueRequestReport(identity.PlantId, mId);
            return null;
        }

        public void IssueRequestReport(string plantId, string mId)
        {
            _ = new ReportUtility();
            string issueId = "";
            string fileName = "IssueRequestReport" + plantId + ".docx";
            string strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
            string File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            //makeDictionary();
            ////A opens input document.
            WordDocument document = new WordDocument(File, FormatType.Docx);
            //Gets the paragraph at index 1
            try
            {
                var sqlissue = @"Select distinct IssueRequestMasterId from [TRN].[IssueRequest] Where MaterialIssueControlDetailId IN(SELECT Id FROM dbo.MaterialIssueControlDetail Where MaterialIssueControlMasterId='" + mId + "')";
                DataTable dtIssue = _sqlRepository.GetDataTable(sqlissue);
                if (dtIssue.Rows.Count > 0)
                {
                    issueId = dtIssue.Rows[0]["IssueRequestMasterId"].ToString();
                }
                else
                {
                    throw new CustomException("No issue slip found.");
                }

                WSection section = document.Sections[0];

                DataTable dtOrderMaster;
                dtOrderMaster = loadIssueRequestMaster(issueId);


                Dictionary<string, string> columns = new Dictionary<string, string>();


                //document.Replace("{Remarks}", dtOrderMaster.Rows[0]["Remarks"].ToString(), false, false);
                //document.Replace("{PreparedBy}", dtOrderMaster.Rows[0]["PreparedBy"].ToString(), false, false);
                document.Replace("{CheckedByName}", dtOrderMaster.Rows[0]["CheckedByName"].ToString(), false, false);
                document.Replace("{AuthorizedByName}", dtOrderMaster.Rows[0]["AuthorizedByName"].ToString(), false, false);
                document.Replace("{EmployeeName}", dtOrderMaster.Rows[0]["ReceivedBy"].ToString(), false, false);




                foreach (DataColumn item in dtOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var dsServiceItems = loadIssueRequestDetail(issueId);
                var materialTotal = makeIssueDetailsTable(document, dsServiceItems, issueId);//Material Details 
                var serviceTotal = 0.00;
                //if (dsServiceItems.Rows.Count > 0)
                //{
                //    serviceTotal = makeOrderServiceTable(document, dsServiceItems, issueId);//Service Details 
                //    document.Replace("{ServiceDetails}", "Service Details", true, true);
                //}
                //{TotalInWords}
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2") + " " + dtOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{TotalInWords}", ru.InWord((materialTotal + serviceTotal), dtOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();

                StringCollection strColDistinct = new StringCollection();

                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                        continue;

                    strColDistinct.Add(strReplace[i].ToUpper());             //For Same Name Use
                    string text = strReplace[i].ToUpper();

                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        ReplaceInfo[text] = document.Replace(text, dtOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }
                document.Replace("{IssueId}", dtOrderMaster.Rows[0]["IssueId"].ToString(), false, false);
                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);


                //removing any unused place holder
                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "", false, false);

                }

                ////Creates an instance of the DocToPDFConverter
                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);

                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects
                document.Close();
                var filename = "IssueRequestReport-" + plantId + "-" + issueId;
                //Saves the PDF file 
                pdfDocument.Save(filename + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);

                document.Close();


            }
            catch (Exception ex)
            {
                throw ex;

            }
            document.Close();
        }

        public DataTable loadIssueRequestMaster(string issueId)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT 
                             EmIU.EmployeeName ReceivedBy
						,IsR.Id IssueId
						,REPLACE(CONVERT(VARCHAR(11),IsR.AddedDate, 113), ' ', '-') IssueDate
						,CheckedByName=CASE WHEN IsR.CheckedByStatus='Checked' Then eI.EmployeeName else '' END 
						,AuthorizedByName=CASE When IsR.AuthorizedByStatus='Approved'then eI1.EmployeeName else '' END
						--,AddedBy=CASE When IsR.CheckedByStatus='ForChecked' OR IsR.CheckedByStatus='Hold' OR IsR.CheckedByStatus='Reject' OR IsR.CheckedByStatus='Checked'then eI3.EmployeeName else ''  END 
						,PurOrCheckedStatus= CASE when IsR.CheckedByStatus='ForChecked' Then 'To be checked'
						when IsR.CheckedByStatus='Hold' Then 'Hold'
						when IsR.CheckedByStatus='Reject' Then 'Reject'
						when IsR.CheckedByStatus='Checked' Then 'Checked'
						else ''
						END
						,PurOrApprovedStatus= CASE 
						when IsR.AuthorizedByStatus='Reject' Then 'Reject For Approved'
						when IsR.AuthorizedByStatus='Hold' Then 'Hold For Approved'
						when IsR.AuthorizedByStatus='For Approval' Then 'To be Approval'
						when IsR.AuthorizedByStatus='Approved' Then 'Approved'
						else ''
						END
						,p.UserName ProcessName,po.SalesOrderId,FGColor=isnull(po.FGColor1,'')+','+isnull(po.FGColor2,'')+','+isnull(po.FGColor3,'')
						,po.ProductionOrder,PreparedBy.EmployeeName AddedBy
						FROM TRN.IssueRequestMaster As IsR
						                LEFT JOIN dbo.EmployeeInformation eI ON eI.SystemId=IsR.CheckedBy
                                         LEFT JOIN dbo.EmployeeInformation eI1 ON eI1.SystemId=IsR.AuthorizedBy
							             LEFT JOIN dbo.EmployeeInformation eI3 ON eI3.SystemId=IsR.AddedBy
										 Left Join EmployeeInformation EmIU on EmIU.SystemId=IsR.UpdatedBy
										  Left Join EmployeeInformation PreparedBy on PreparedBy.SystemId=IsR.Preparedby
										 left join trn.IssueRequestMasterProcessMap IRPmap On IRPmap.IssueRequestMasterId=IsR.Id
										 left join hkp.Process p On p.Id=IRPmap.ProcessId
										 LEFT JOIN(
							SELECT distinct PDAMAP.IssueRequestMasterId
								,SalesOrderId=STUFF((select distinct ','+xPDAMAP.SalesOrderId from
								trn.IssueRequestMaster xpo
								INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,FGColor1=STUFF((select distinct ','+FCV.UserName from
								trn.IssueRequestMaster xpo
								INNER JOin trn.IssueRequestSKUMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON xPDAMAP.FirstCharacteristicsValueId=FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON xPDAMAP.SecondCharacteristicsValueId=SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON xPDAMAP.ThirdCharacteristicsValueId=TCV.Id
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

							   ,FGColor2=STUFF((select distinct ','+SCV.UserName from
								trn.IssueRequestMaster xpo
								INNER JOin trn.IssueRequestSKUMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON xPDAMAP.FirstCharacteristicsValueId=FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON xPDAMAP.SecondCharacteristicsValueId=SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON xPDAMAP.ThirdCharacteristicsValueId=TCV.Id
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,FGColor3=STUFF((select distinct ','+TCV.UserName from
								trn.IssueRequestMaster xpo
								INNER JOin trn.IssueRequestSKUMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
								LEFT JOIN HKP.CharacteristicsValue AS FCV ON xPDAMAP.FirstCharacteristicsValueId=FCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS SCV ON xPDAMAP.SecondCharacteristicsValueId=SCV.Id
								LEFT JOIN HKP.CharacteristicsValue AS TCV ON xPDAMAP.ThirdCharacteristicsValueId=TCV.Id
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ProductionOrder=STUFF((select distinct ','+PrOrderDetail.ProductionOrderId from
								trn.IssueRequestMaster xpo
								INNER JOin trn.IssueRequestMasterSalesOrderMap xPDAMAP on xpo.Id=xPDAMAP.IssueRequestMasterId
								left join trn.ProductionOrderDetail PrOrderDetail ON PrOrderDetail.SalesOrderId=xPDAMAP.SalesOrderId
								
								where xPDAMAP.IssueRequestMasterId=PDAMAP.IssueRequestMasterId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								from  trn.IssueRequestMasterSalesOrderMap PDAMAP 							 
							  group by  PDAMAP.IssueRequestMasterId
							)PO ON PO.IssueRequestMasterId = IsR.Id
                      WHERE IsR.Id ='" + issueId + "'";

                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public DataTable loadIssueRequestDetail(string issueId)
        {
            string strSQL1;
            try
            {
                strSQL1 = @"DECLARE @Own VARCHAR(10)='Own',@Other VARCHAR(10)='Other';
                            Select 
                            ROW_NUMBER() OVER(ORDER BY IR.Id ASC) AS SiNo
                            ,IR.Id
                            ,CC.UserName AS CostCenterName
                            ,B.UserName ActivityName 
                            ,mm.UserName MaterialMasterName
                            ,ART.StandardName
                            ,ART.Code
                            ,IR.RequisitionId
                            ,IR.RequisitionDetailId
                            ,TUoM.UserName AS UOM
                            ,IR.RequestedQty
                            ,IR.RejectedQty
                            ,IR.RequestedQty+IR.RejectedQty AS Total

                            ,En.Username As EntityName
                            ,MRM.EntityId
                            --,Bu.Code
                            --,Bu.UserName
                            ,Us.FullName AddedBy
                            ,MRM.Id RequisitionNo
                            ,MRD.ArticleId
                            ,Dp.UserName DepartmentName
                            ,MGM.UserName MaterialMasterGroupName

                            ,MT.UserName MaterialType
                            ,MRD.FirstCharacteristicsId
                            ,FC.UserName AS FirstCharacteristics
                            ,MRD.FirstCharacteristicsValueId
                            ,FCV.UserName AS FirstCharacteristicsValue
                            ,MRD.SecondCharacteristicsId
                            ,SC.UserName AS SecondCharacteristics
                            ,MRD.SecondCharacteristicsValueId
                            ,SCV.UserName AS SecondCharacteristicsValue
                            ,MRD.ThirdCharacteristicsId
                            ,TC.UserName AS ThirdCharacteristics
                            ,MRD.ThirdCharacteristicsValueId
                            ,TCV.UserName AS ThirdCharacteristicsValue

                            ,IR.CostCenterId
                            ,IR.ExpenseActivityId
                            ,IR.BudgetMasterId
                            ,IR.GLGeneralInfoId 
                            ,OwnOtherQty=CASE
								WHEN IR.CostCenterId=IR.ExpenseActivityId THEN @Own						
								ELSE @Other
							
							END
,isnull(IGL1.UserName,'') AS CGL									
							,isnull(B1.UserName,'') AS CBUdget
							,isnull(IA1.UserName,'') AS GLBudgetActivity
                            --,IR.AddedBy
							--,CC.UserName AS CostCenterName
                            from trn.IssueRequest IR
                            left Join [TRN].[MaterialRequsitionDetails] As MRD on MRD.Id=IR.REquisitionDetailId
                            Left Join [TRN].[MaterialRequsitionMaster] As MRM On MRD.MaterialReqqusitionMasterId=MRM.Id
                            Left Join [ORG].[CostCenter] CC On CC.Id=IR.CostCenterId
                            Left Join hkp.Budget B On B.Id=IR.ExpenseActivityId

                            Left Join [ORG].[Entity] As En On MRM.EntityId=En.Id
                            Left Join [HKP].[Budget] As Bu On Bu.Id=MRD.ActivityId
                            Left JOIN MST.MaterialMaster AS MM ON IR.MaterialMasterId = MM.Id
                            LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
                            LEFT JOIN MST.MaterialMasterArticle AS ART ON IR.ArticleId = ART.Id
                            LEFT JOIN HKP.Characteristics AS FC ON IR.FirstCharacteristicsId = FC.Id
                            LEFT JOIN HKP.Characteristics AS SC ON IR.SecondCharacteristicsId = SC.Id
                            LEFT JOIN HKP.Characteristics AS TC ON IR.ThirdCharacteristicsId = TC.Id
                            LEFT JOIN HKP.CharacteristicsValue AS FCV ON IR.FirstCharacteristicsValueId = FCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS SCV ON IR.SecondCharacteristicsValueId = SCV.Id
                            LEFT JOIN HKP.CharacteristicsValue AS TCV ON IR.ThirdCharacteristicsValueId = TCV.Id
                             LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IR.TransactionUoMId = TUoM.Id
                            LEFT JOIN [SEC].[User] As Us On MRM.AddedBy=Us.UserId
                            LEFT JOIN dbo.EmployeeInformation As Em On Us.EmployeeId=Em.SystemId
                            LEFT JOIN [ORG].[Department] AS Dp On Dp.Id=Em.DepartmentId
                            LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                            LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IR.GLGeneralInfoId 
									LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IR.BudgetMasterId
									LEFT JOIN HKP.Activity IA1 ON IA1.Id=IR.ExpenseActivityId
									Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId

                      where IR.IssueRequestMasterId='" + issueId + "'";

                return _sqlRepository.GetDataTable(strSQL1);

            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }

        public double makeIssueDetailsTable(WordDocument document, DataTable dsOrderMaster, string issueId)
        {
            string replaceString = "{IssueSlipDetails}";

            DataTable dsOrderItems, dsTax;

            //dsOrderItems = loadOrderMasterItems(grnId);
            //dsTax = loadOrderMasterTax(grnId);

            int LasColumnIndex = 10;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();
            //DataView dv = new DataView(dsTax.DefaultView.ToTable(true, "TaxCode"));

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);
            //if (dv.Count > 0)
            //{
            //    for (int i = 0; i < dv.Count; i++)
            //    {
            //        LasColumnIndex++;
            //        dicTaxes.Add(dv[i]["TaxCode"].ToString(), LasColumnIndex);
            //        LasColumnIndex++;
            //    }
            //}


            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();
            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SL");
            range.ApplyCharacterFormat(FontBold);
            int colSLNo = COL; COL++;
            wTable.Rows[ROW].Cells[colSLNo].Width = 50;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("IssueId");
            range.ApplyCharacterFormat(FontBold);
            int colIssueId = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Cost Center Name");
            range.ApplyCharacterFormat(FontBold);
            int CostCenterNameId = COL; COL++;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Expe.Activity Code ");
            range.ApplyCharacterFormat(FontBold);
            int colActivityCode = COL; COL++;




            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Material");
            range.ApplyCharacterFormat(FontBold);
            int colItemName = COL; COL++;
            wTable.Rows[ROW].Cells[colItemName].Width = 120;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticleCode = COL; COL++;
            wTable.Rows[ROW].Cells[colArticleCode].Width = 120;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU1");
            range.ApplyCharacterFormat(FontBold);
            int colSku1 = COL; COL++;
            wTable.Rows[ROW].Cells[colSku1].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU2");
            range.ApplyCharacterFormat(FontBold);
            int colSku2 = COL; COL++;
            wTable.Rows[ROW].Cells[colSku2].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SKU3");
            range.ApplyCharacterFormat(FontBold);
            int colSku3 = COL; COL++;
            wTable.Rows[ROW].Cells[colSku3].Width = 70;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UOM");
            range.ApplyCharacterFormat(FontBold);
            int colUOM = COL; COL++;
            wTable.Rows[ROW].Cells[colUOM].Width = 70;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Total Qty");
            range.ApplyCharacterFormat(FontBold);
            int colValidQty = COL; //COL++;

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    //TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }


                TROW.Cells[colSLNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SiNo"].ToString());
                TROW.Cells[colIssueId].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Id"].ToString());
                TROW.Cells[CostCenterNameId].AddParagraph().AppendText(dsOrderMaster.Rows[i]["CostCenterName"].ToString());
                TROW.Cells[colActivityCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["GLBudgetActivity"].ToString());
                TROW.Cells[colItemName].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialMasterName"].ToString());
                TROW.Cells[colArticleCode].AddParagraph().AppendText(dsOrderMaster.Rows[i]["StandardName"].ToString());
                TROW.Cells[colSku1].AddParagraph().AppendText(dsOrderMaster.Rows[i]["FirstCharacteristicsValue"].ToString());

                TROW.Cells[colSku2].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SecondCharacteristicsValue"].ToString());

                TROW.Cells[colSku3].AddParagraph().AppendText(dsOrderMaster.Rows[i]["ThirdCharacteristicsValue"].ToString());


                TROW.Cells[colUOM].AddParagraph().AppendText(dsOrderMaster.Rows[i]["UOM"].ToString());
                TROW.Cells[colValidQty].AddParagraph().AppendText(dsOrderMaster.Rows[i]["RequestedQty"].ToString());


                totalValue += clsStdLib.dbl(dsOrderMaster.Rows[i]["Total"].ToString());

                //if (dv.Count > 0)
                //{
                //    DataView dvtax = new DataView(dsTax.DefaultView.ToTable());
                //    //double totalTax = 0;

                //    for (int T = 0; T < dv.Count; T++)
                //    {
                //        dvtax.RowFilter = "TaxCode='" + dv[T]["TaxCode"].ToString() + "' AND InventoryReceiveDetailId ='" + dsOrderMaster.Rows[i]["InventoryReceiveDetailId"].ToString() + "'";
                //        if (dvtax.Count > 0)
                //        {
                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()]].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["Percentage"].ToString()).ToString("F2"));

                //            TROW.Cells[dicTaxes[dv[T]["TaxCode"].ToString()] + 1].AddParagraph().AppendText(Convert.ToDouble(dvtax[0]["TaxAmount"].ToString()).ToString("F2"));

                //        }
                //    }

                //}
                //ROW++;
            }


            #region Total
            int TotalRow = ROW + 1;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);


            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                if (C == colSLNo || C == CostCenterNameId || C == colIssueId || C == colActivityCode || C == colItemName || C == colArticleCode || C == colSku1 || C == colSku2 || C == colSku3 || C == colUOM)
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("F2")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total


            ROW++;
            #region Sub Total
            //int SubTotalRow = ROW;
            //int SubTotalColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[SubTotalColumn].AddParagraph().AppendText("Sub Total");

            //double total = clsStdLib.dbl(dsOrderMaster.Compute("SUM(TrnAmount)", "").ToString());
            //- clsStdLib.dbl(dsOrderItems.Tables[0].Compute("SUM(Discount)", "").ToString())
            //+ clsStdLib.dbl(dsTax.Compute("SUM(TaxAmount)", "").ToString());

            //_TROW.Cells[SubTotalColumn + 1].AddParagraph().AppendText(total.ToString("F2"));

            #endregion Total


            ROW++;
            #region Total Payable
            //int TotalPayableRow = ROW;
            //int TotalPayableColumn = 0;//_TROW.Cells.Count - 5;
            //wTable.AddRow();
            //_TROW = wTable.LastRow;

            //_TROW.Cells[TotalPayableColumn].AddParagraph().AppendText("Total Amount Payable");
            //_TROW.Cells[TotalPayableColumn + 1].AddParagraph().AppendText("Need To Discuss");

            #endregion Total Payable

            ROW++;

            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            for (int R = 0; R < wTable.Rows.Count; R++)
            {
                WTableRow TROW = wTable.Rows[R];
                TROW.Cells[0].Width = 30;
                //if (dv.Count < 3)
                //    TROW.Cells[0].Width = 120 + ((3 - dv.Count) * 40);//for each tax group missing, adjust width with 0 cell

                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.ApplyStyle("MyStyle");
                    }
                }
            }

            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);
            double total = 0.00;
            return total;
        }


        #endregion
    }
}