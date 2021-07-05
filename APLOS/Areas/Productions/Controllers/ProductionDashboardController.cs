#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System;
using OTSBD;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionDashboardController : BaseController
    {
        #region Constructor
        /// <summary>   The CostingTypesService service. </summary>
        private readonly ISqlRepository _sqlRepository;
        public ProductionDashboardController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT CostingType AS [Value], UserName AS [Text] FROM [dbo].[CostingTypes]"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProcessList()
        {
            string sql = new Library.OrderManagement.Production.WIPReport().GetAllProcessAndInventory();
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult ProductionEfficiencyReport(string PlantId, string entityid, string Date)
        {

            try
            {
                Library.Planning.OrderManagement.Bulletin bulletin = new Library.Planning.OrderManagement.Bulletin();
                bulletin.ProductionEfficiencyReport(PlantId, entityid, Date);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpGet, Authorize]
        public ActionResult GetAllCompaniesAndPlants()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.OrderManagement.Production.WIPReport repo = new Library.OrderManagement.Production.WIPReport();
            return Json(new
            {
                Plant = repo.GetAllPlants(),
                Company = repo.GetAllCompanies(),
                PlantId = identity.PlantId,
                CompanyId = identity.CompanyId,
                BaseProcessId = repo.GetType1ProcessId()
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProcessWiseProduction(string PlantId, string EntityId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            string sql = new Library.Planning.PlanningType1.ProductionDashboard().GetProcessWiseProduction(PlantId, EntityId, _date);
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProductionOrderWiseProduction(string PlantId, string EntityId, string ProcessId, DateTime date)
        {
            //.. GetProductionLisPRWiseDashboard
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
            _wipReport.GetProductionLisPRWiseDashboard(PlantId, ProcessId, _date, out DataTable dt);

            if (string.IsNullOrEmpty(EntityId) == false && EntityId != "null")
            {
                dt.DefaultView.RowFilter = "EntityId='" + EntityId + @"'";
                dt = dt.DefaultView.ToTable();
            }

            dt.DefaultView.RowFilter = "isnull(InQuantityToday,0)>0 OR isnull(OutQuantityToday,0)>0 OR isnull(KillQuantityToday,0)>0";
            dt = dt.DefaultView.ToTable();

            return Json(Helpers.CustomJsonResult.DataTableToJson(dt), JsonRequestBehavior.AllowGet);


            //string sql = new Library.Planning.PlanningType1.ProductionDashboard().GetProductionOrderWiseProduction(EntityId, ProcessId, _date);
            //return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetWorkCenterWiseWIP(string PlantId, string EntityId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
            _wipReport.GetProductionListWithoutWCRowProcessWiseDashboard(PlantId, ProcessId, _date, out DataTable dt);

            //DataTable data = new Library.Planning.PlanningType1.ProductionDashboard().GetWorkCenterWiseWIP(PlantId, EntityId, dt);
            DataTable data = new Library.Planning.PlanningType1.ProductionDashboard().GetWorkCenterWiseWIPForGraph(PlantId, EntityId, ProcessId, dt);

            return Json(Helpers.CustomJsonResult.DataTableToJson(data), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetWorkCenterWiseWIPForGraph(string PlantId, string EntityId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
            _wipReport.GetProductionListWithoutWCRowProcessWiseDashboard(PlantId, ProcessId, _date, out DataTable dt);

            DataTable data = new Library.Planning.PlanningType1.ProductionDashboard().GetWorkCenterWiseWIPForGraph(PlantId, EntityId, ProcessId, dt);

            return Json(Helpers.CustomJsonResult.DataTableToJson(data), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetDailyPlanVsProduction(string PlantId, string EntityId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();

            var _DailyPlanVsProduction = _wipReport.GetDailyPlanVsProduction(PlantId, EntityId, _date);
            string _ProcessName = _wipReport.GetType1ProcessName();
            return Json(new { PlanVsProductionWC = _DailyPlanVsProduction, ProcessName = _ProcessName }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetDailyLast30DaysPlanVsProduction(string PlantId, string EntityId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();

            var _DailyLast30DaysPlanVsProduction = _wipReport.GetDailyLast30DaysPlanVsProduction(PlantId, EntityId, _date);
            return Json(new { PlanVsProduction30 = _DailyLast30DaysPlanVsProduction }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetLastDaysPlanVsProductionStatistics(string PlantId, string EntityId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();

            var dayStatistics = _wipReport.GetLastDaysPlanVsProductionStatistics(PlantId, EntityId, _date);
            return Json(dayStatistics, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetProfitability(string PlantId, string EntityId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.Planning.PlanningType1.ProductionDashboard _wipReport = new Library.Planning.PlanningType1.ProductionDashboard();
            string query = _wipReport.GetProfitability(PlantId, EntityId, _date);
            return Json(_sqlRepository.GetDataCollection(query), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProfitabilityReport(string PlantId, string EntityId, DateTime date)
        {
            try
            {
                string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
                Library.Planning.PlanningType1.ProductionDashboard _wipReport = new Library.Planning.PlanningType1.ProductionDashboard();
                _wipReport.ProfitabilityReport(PlantId, EntityId, _date);
                //string query = _wipReport.GetProfitability(PlantId, EntityId, _date);
                return null;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetPRWiseWIP(string PlantId, string EntityId, string ProcessId, string WorkCenterMasterId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            Library.OrderManagement.Production.WIPReport _wipReport = new Library.OrderManagement.Production.WIPReport();
            _wipReport.GetProductionListWithoutWCRowProcessWiseDashboard(PlantId, ProcessId, _date, out DataTable dt);



            DataTable data = new Library.Planning.PlanningType1.ProductionDashboard().GetPRWiseWIP(EntityId, WorkCenterMasterId, dt);

            return Json(Helpers.CustomJsonResult.DataTableToJson(data), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetInWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetInWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOutWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetOutWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetKillWC(string FDUD, string PlantId, string EntityId, string WorkCenterMasterId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetKillWC(FDUD, EntityId, WorkCenterMasterId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetInPO(string FDUD, string ProductionOrderId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetInPO(FDUD, ProductionOrderId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOutPO(string FDUD, string ProductionOrderId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetOutPO(FDUD, ProductionOrderId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetKillPO(string FDUD, string ProductionOrderId, string ProcessId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetKillPO(FDUD, ProductionOrderId, ProcessId, _date), JsonRequestBehavior.AllowGet);
        }



        [HttpGet, Authorize]
        public ActionResult GetInWCPO(string FDUD, string ProductionOrderId, string WorkCenterMasterId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetInWCPO(FDUD, ProductionOrderId, WorkCenterMasterId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetOutWCPO(string FDUD, string ProductionOrderId, string WorkCenterMasterId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetOutWCPO(FDUD, ProductionOrderId, WorkCenterMasterId, _date), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetKillWCPO(string FDUD, string ProductionOrderId, string WorkCenterMasterId, DateTime date)
        {
            string _date = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
            return Json(new Library.Planning.PlanningType1.ProductionDashboard().GetKillWCPO(FDUD, ProductionOrderId, WorkCenterMasterId, _date), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult GetProductionRelay(string PlantId, string EntityId, string ProcessId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(EntityId) || EntityId.ToUpper() == "NULL")
            {
                string sql = @"SELECT convert(bit,case when  PLST.processId='" + ProcessId + @"' then 1 else 0 END) AS IsLastProcess,
convert(bit,0) AS Checked, pss.Id PSSId,ppr.Id PPRId, convert(bit ,isnull(ppr.IsCompleted,0)) AS IsCompleted,P.UserName PreviousProcess,PPR.CompletedBy ClosedBy
,Format(PPR.CompletionEntryDate,'dd-MMM-yyyy') ClosedDate ,Format(PPR.StartDate,'dd-MMM-yyyy') PreviousProcessStartDate
,  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
isnull(CurrentProcessPR.ProductionQtyAtPR,0) ProducedQty
,Variance=case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end-isnull(CurrentProcessPR.ProductionQtyAtPR,0),
  ISNULL(PO.Qty,0) AS POQuantity,ISNULL(SO.PlannedQty,0) AS PlannedQty,ISNULL(SO.OrderQty,0) AS OrderQty,so.Material,
     so.ProductCategory,so.Product,
		ActualQTY=	case when ISNULL(st.Qty,0)>isnull(PO.Qty,0) then st.Qty else po.Qty end,							
                                Format(so.LastShipmentDate,'dd-MMM-yyyy') LastShipmentDate, so.article,CurrentProcessPR.ProductionQtyAtPR
								,Format(CurrentProcessPR.ProductionStartDateAtPR,'dd-MMM-yyyy') ProductionStartDateAtPR,
								Format(PSS.StartDate,'dd-MMM-yyyy') StartDate,Format(PSS.EndDate,'dd-MMM-yyyy') EndDate,Format(st.LSD,'dd-MMM-yyyy') LSD,
								PSS.CompletedBy ,
PreviousProcessPR.ProductionQtyAtPR PreviousProcessQty,
								      MasterOrderId =STUFF((select distinct ','+XMOI.Id from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
                                                    BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                   OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                  			
  
                            FROM [TRN].[ProductionOrder] AS PO
							left outer join ProductionOrderSchedulingParametersType1 st on st.ProductionOrderID=PO.Id
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
							left join trn.ProductionOrderProcessSet PSS ON PSS.ProductionOrderId=PO.Id
							left join trn.ProductionOrderProcessSet PLST ON PLST.ProductionOrderId=PO.Id and PLST.Id=(select top 1 Id from trn.ProductionOrderProcessSet XP where XP.ProductionOrderId=PO.Id order by XP.Sequence DESC)
							left join trn.ProductionOrderProcessSet PPR ON PSS.ProductionOrderId=PO.Id 
							and PPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] desc) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 
							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence<PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process P on p.Id=PPR.ProcessId					

                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDateAtPR,MAX(s.ProductionDate) AS ProductionEndDateAtPR
											FROM  trn.ProductionSummary S 
											where s.ProcessId='" + ProcessId + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS CurrentProcessPR ON  CurrentProcessPR.ProductionOrderId=po.id AND CurrentProcessPR.ProcessId=PSS.ProcessId
							
                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S											
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PreviousProcessPR ON  PreviousProcessPR.ProductionOrderId=po.id AND PreviousProcessPR.ProcessId=PPR.ProcessId
							 
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, PM.UserName AS Product,pc.UserName AS ProductCategory,
                                                     min(so.DeliveryDate) AS FirstShipmentDate,  max(so.DeliveryDate) AS LastShipmentDate,
                                                    sum(so.Qty) AS OrderQty,
                                                    SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty
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
                            WHERE isnull(s.username,'') IN ('RUNNING') 
                            AND ((ISNULL(ppr.Id,'')<>'' AND ISNULL(ppr.StartDate,'')<>'') OR ISNULL(ppr.Id,'')=''  OR ISNULL(ppr.IsCompleted,0)=1)
                            AND  EN.PlantId='" + PlantId + @"' and  PSS.ProcessId = '" + ProcessId + @"' and isnull(pss.IsCompleted,0)=0";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

            }
            else
            {
                string sql = @"SELECT convert(bit,case when  PLST.processId='" + ProcessId + @"' then 1 else 0 END) AS IsLastProcess,
convert(bit,0) AS Checked, pss.Id PSSId,ppr.Id PPRId, convert(bit ,isnull(ppr.IsCompleted,0)) AS IsCompleted,P.UserName PreviousProcess,PPR.CompletedBy ClosedBy
,Format(PPR.CompletionEntryDate,'dd-MMM-yyyy') ClosedDate ,Format(PPR.StartDate,'dd-MMM-yyyy') PreviousProcessStartDate
,  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
isnull(CurrentProcessPR.ProductionQtyAtPR,0) ProducedQty
,Variance=case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end-isnull(CurrentProcessPR.ProductionQtyAtPR,0),
  ISNULL(PO.Qty,0) AS POQuantity,ISNULL(SO.PlannedQty,0) AS PlannedQty,ISNULL(SO.OrderQty,0) AS OrderQty,so.Material,
     so.ProductCategory,so.Product,
		ActualQTY=	case when ISNULL(st.Qty,0)>isnull(PO.Qty,0) then st.Qty else po.Qty end,							
                                Format(so.LastShipmentDate,'dd-MMM-yyyy') LastShipmentDate, so.article,CurrentProcessPR.ProductionQtyAtPR
								,Format(CurrentProcessPR.ProductionStartDateAtPR,'dd-MMM-yyyy') ProductionStartDateAtPR,
								Format(PSS.StartDate,'dd-MMM-yyyy') StartDate,Format(PSS.EndDate,'dd-MMM-yyyy') EndDate,Format(st.LSD,'dd-MMM-yyyy') LSD,
								PSS.CompletedBy ,
PreviousProcessPR.ProductionQtyAtPR PreviousProcessQty,
								      MasterOrderId =STUFF((select distinct ','+XMOI.Id from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
                                                    BuyerRefNo =STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                   OwnRefNo =STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

													StyleNo=STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 
	                                                
                                                    OwnStyleNo=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''), 

                                                    SONo=STUFF((select distinct ','+sox.Id from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    SODesc=STUFF((select distinct ','+sox.[Description] from 
								                                trn.MasterOrderItem XMOI 	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=xmoi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PO.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),

                                                    buyer=STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),


                                                    Customer=STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where PO.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                                  			
  
                            FROM [TRN].[ProductionOrder] AS PO
							left outer join ProductionOrderSchedulingParametersType1 st on st.ProductionOrderID=PO.Id
                            JOIN [ORG].[Entity] AS EN ON PO.EntityId = EN.Id
                            LEFT JOIN [HKP].[ProductionStatus] AS PS ON PO.EntityId = PS.Id
							left join trn.ProductionOrderProcessSet PSS ON PSS.ProductionOrderId=PO.Id
							left join trn.ProductionOrderProcessSet PLST ON PLST.ProductionOrderId=PO.Id and PLST.Id=(select top 1 Id from trn.ProductionOrderProcessSet XP where XP.ProductionOrderId=PO.Id order by XP.Sequence DESC)
							left join trn.ProductionOrderProcessSet PPR ON PSS.ProductionOrderId=PO.Id 
							and PPR.id=(select A.Id from (
							select DENSE_RANK() over (partition by p.ProductionOrderId order by P.[sequence] desc) AS RNK,P.*
							from TRN.ProductionOrderProcessSet P 
							
							where p.ProductionOrderId=PSS.ProductionOrderId AND P.Sequence<PSS.sequence) AS A where a.RNK=1)
							left outer join HKP.Process P on p.Id=PPR.ProcessId					

                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDateAtPR,MAX(s.ProductionDate) AS ProductionEndDateAtPR
											FROM  trn.ProductionSummary S 
											where s.ProcessId='" + ProcessId + @"'
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS CurrentProcessPR ON  CurrentProcessPR.ProductionOrderId=po.id AND CurrentProcessPR.ProcessId=PSS.ProcessId
							
                             LEFT OUTER JOIN (SELECT s.ProductionOrderId,s.ProcessId,SUM(s.Quantity) AS ProductionQtyAtPR
												,MIN(s.ProductionDate) AS ProductionStartDate,MAX(s.ProductionDate) AS ProductionEndDate
											FROM  trn.ProductionSummary S											
											GROUP BY  s.ProductionOrderId,s.ProcessId
							) AS PreviousProcessPR ON  PreviousProcessPR.ProductionOrderId=po.id AND PreviousProcessPR.ProcessId=PPR.ProcessId
							 
                            LEFT OUTER  JOIN (select
                                                    pod.ProductionOrderId,
                                                    mm.userName AS Material,ma.StandardName AS Article, PM.UserName AS Product,pc.UserName AS ProductCategory,
                                                     min(so.DeliveryDate) AS FirstShipmentDate,  max(so.DeliveryDate) AS LastShipmentDate,
                                                    sum(so.Qty) AS OrderQty,
                                                    SUM(CEILING((isnull(SO.qty,0)*(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0))))) AS PlannedQty
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
                            WHERE isnull(s.username,'') IN ('RUNNING') 
                            AND ((ISNULL(ppr.Id,'')<>'' AND ISNULL(ppr.StartDate,'')<>'') OR ISNULL(ppr.Id,'')=''  OR ISNULL(ppr.IsCompleted,0)=1)
                            AND  EN.PlantId='" + PlantId + @"' AND PO.entityid='" + EntityId + @"' and  PSS.ProcessId = '" + ProcessId + @"' and isnull(pss.IsCompleted,0)=0";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

            }

        }

        #endregion
    }
}