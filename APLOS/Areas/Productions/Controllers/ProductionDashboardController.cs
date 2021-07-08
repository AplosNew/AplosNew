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
using Syncfusion.XlsIO;
using System.IO;
using Library.Service.Helpers;

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
                string sql = @"SELECT convert(bit,case when  PLST.processId='" + ProcessId + @"' then 1 else 0 END) AS IsLastProcess,PSS.Remarks,
convert(bit,0) AS Checked, pss.Id PSSId,ppr.Id PPRId, convert(bit ,isnull(ppr.IsCompleted,0)) AS IsCompleted,P.UserName PreviousProcess,CP.UserName CurrentProcess,PPR.CompletedBy ClosedBy
,Format(PPR.CompletionEntryDate,'dd-MMM-yyyy') ClosedDate ,Format(PPR.StartDate,'dd-MMM-yyyy') PreviousProcessStartDate
,  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
isnull(CurrentProcessPR.ProductionQtyAtPR,0) ProducedQty
,Variance=case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end-isnull(CurrentProcessPR.ProductionQtyAtPR,0),
  ISNULL(PO.Qty,0) AS POQuantity,ISNULL(SO.PlannedQty,0) AS PlannedQty,ISNULL(SO.OrderQty,0) AS OrderQty,so.Material,
     so.ProductCategory,so.Product,
		ActualQTY=	case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end,							
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
							left outer join HKP.Process CP on CP.Id=PSS.ProcessId					

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
                           AND EN.PlantId ='" + PlantId+@"' AND PSS.ProcessId = '" + ProcessId + @"' and isnull(pss.IsCompleted,0)=0 
                            ORDER BY st.LSD";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

            }
            else
            {
                string sql = @"SELECT convert(bit,case when  PLST.processId='" + ProcessId + @"' then 1 else 0 END) AS IsLastProcess,PSS.Remarks,
convert(bit,0) AS Checked, pss.Id PSSId,ppr.Id PPRId, convert(bit ,isnull(ppr.IsCompleted,0)) AS IsCompleted,P.UserName PreviousProcess,CP.UserName CurrentProcess,PPR.CompletedBy ClosedBy
,Format(PPR.CompletionEntryDate,'dd-MMM-yyyy') ClosedDate ,Format(PPR.StartDate,'dd-MMM-yyyy') PreviousProcessStartDate
,  PO.Id,PO.EntityId, PO.Remarks,s.UserName AS ProductionStatus, EN.UserName AS EntityName, PS.UserName AS ProductionStatusName,
isnull(CurrentProcessPR.ProductionQtyAtPR,0) ProducedQty
,Variance=case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end-isnull(CurrentProcessPR.ProductionQtyAtPR,0),
  ISNULL(PO.Qty,0) AS POQuantity,ISNULL(SO.PlannedQty,0) AS PlannedQty,ISNULL(SO.OrderQty,0) AS OrderQty,so.Material,
     so.ProductCategory,so.Product,
		ActualQTY=	case when ISNULL(st.Qty,0)>0 then st.Qty else po.Qty end,							
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
							left outer join HKP.Process CP on CP.Id=PSS.ProcessId				

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
                             AND EN.PlantId ='" + PlantId + @"' AND  PO.entityid='" + EntityId + @"' and  PSS.ProcessId = '" + ProcessId + @"' and isnull(pss.IsCompleted,0)=0 
                            ORDER BY st.LSD";
                return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

            }

        }

        //[HttpGet, Authorize]
        //public ActionResult GetProductionRelayReport(string PlantId, string EntityId, string ProcessId)
        //{
        //    try
        //    {

        //        ProductionRelayReport(PlantId, EntityId, ProcessId);

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}

        //public void ProductionRelayReport(string PlantId, string EntityId, string ProcessId)
        //{
        //    try
        //    {
        //        string sql = GetProductionRelay(PlantId, EntityId, ProcessId).ToString();
        //        ExcelEngine excelEngine = new ExcelEngine();
        //        //Instantiate the Excel application object
        //        IApplication application = excelEngine.Excel;

        //        //Set the default application version
        //        application.DefaultVersion = ExcelVersion.Excel2013;
        //        IWorkbook workbook = application.Workbooks.Create(1);
        //        IWorksheet sheet = workbook.Worksheets[0];

        //        sheet.Name = "Production Relay";

        //        DataTable dtPurchaseLc = _sqlRepository.GetDataTable(sql);

        //        int ROW = 6;
        //        int COL = 1;


        //        sheet[ROW, COL].Text = "Sl No.";
        //        sheet[ROW, COL].ColumnWidth = 6;
        //        int colSlNo = COL;
        //        COL++;

        //        sheet[ROW, COL].Text = "LC No.";

        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colLCNo = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Opening Bank";

        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colOpeningBank = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Opening Date";
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colOpeningDate = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Vendor";
        //        sheet[ROW, COL].ColumnWidth = 20;
        //        int colVendor = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Value";
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
        //        int colValue = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Currency";
        //        sheet[ROW, COL].ColumnWidth = 5;
        //        int colCurrency = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "LCA No";
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colLCANo = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "LC Type";
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colLCType = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Tenure";
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colTenure = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Benificiary Bank";
        //        sheet[ROW, COL].ColumnWidth = 15;
        //        int colBenificiaryBank = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "PO Value";
        //        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colPOValue = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Acceptance Value";
        //        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colAcceptanceValue = COL;

        //        COL++;
        //        sheet[ROW, COL].Text = "GRN Value";
        //        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colGRNValue = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Payment Made";
        //        sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colPaymentMade = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Contract No";

        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colContractNo = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "Customer";
        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colCustomer = COL;
        //        COL++;
        //        sheet[ROW, COL].Text = "LC Id";

        //        sheet[ROW, COL].ColumnWidth = 10;
        //        int colLCId = COL;



        //        int endCol = COL;
        //        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
        //        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
        //        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
        //        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
        //        ROW++;

        //        int StartRow = ROW; //row 20
        //        for (int i = 0; i < dtPurchaseLc.Rows.Count; i++)
        //        {


        //            sheet[ROW, colSlNo].Number = (i + 1);

        //            sheet[ROW, colLCNo].Text = dtPurchaseLc.Rows[i]["LCNo"].ToString();
        //            sheet[ROW, colOpeningBank].Text = dtPurchaseLc.Rows[i]["OpeningBank"].ToString();
        //            sheet[ROW, colOpeningDate].Text = dtPurchaseLc.Rows[i]["OpeningDate"].ToString();
        //            sheet[ROW, colVendor].Text = dtPurchaseLc.Rows[i]["Vendor"].ToString();
        //            sheet[ROW, colValue].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["Value"].ToString());
        //            sheet[ROW, colCurrency].Text = dtPurchaseLc.Rows[i]["Currency"].ToString();
        //            sheet[ROW, colLCANo].Text = dtPurchaseLc.Rows[i]["LCANo"].ToString();
        //            sheet[ROW, colLCType].Text = dtPurchaseLc.Rows[i]["LCType"].ToString();
        //            sheet[ROW, colTenure].Text = dtPurchaseLc.Rows[i]["Tenure"].ToString();
        //            sheet[ROW, colBenificiaryBank].Text = dtPurchaseLc.Rows[i]["BenificiaryBank"].ToString();
        //            sheet[ROW, colPOValue].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["POValue"].ToString());
        //            sheet[ROW, colAcceptanceValue].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["AcceptanceValue"].ToString());
        //            sheet[ROW, colGRNValue].Number = clsStaticInfo.dbl(dtPurchaseLc.Rows[i]["GRNValue"].ToString());
        //            sheet[ROW, colPaymentMade].Text = dtPurchaseLc.Rows[i]["PaymentMade"].ToString();
        //            sheet[ROW, colContractNo].Text = dtPurchaseLc.Rows[i]["ContractNo"].ToString();
        //            sheet[ROW, colCustomer].Text = dtPurchaseLc.Rows[i]["Customer"].ToString();
        //            sheet[ROW, colLCId].Text = dtPurchaseLc.Rows[i]["LCId"].ToString();



        //            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
        //            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

        //            ROW++;

        //        }

        //        sheet.Range[StartRow, colValue, ROW, colValue].NumberFormat = clsStaticInfo.NumberFormat(2);
        //        sheet.Range[StartRow, colPOValue, ROW, colPOValue].NumberFormat = clsStaticInfo.NumberFormat(2);
        //        sheet.Range[StartRow, colAcceptanceValue, ROW, colAcceptanceValue].NumberFormat = clsStaticInfo.NumberFormat(2);
        //        sheet.Range[StartRow, colGRNValue, ROW, colGRNValue].NumberFormat = clsStaticInfo.NumberFormat(2);
        //        sheet.IsGridLinesVisible = false;

        //        sheet.UsedRange.WrapText = true;
        //        sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
        //        sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

        //        sheet["A" + StartRow.ToString()].FreezePanes();

        //        sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
        //        sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;

        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        ReportUtility reportUtility = new ReportUtility();
        //        reportUtility.PlantHeader(ref sheet, endCol, "Purchase LC", identity.PlantId);
        //        reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
        //        sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
        //        sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

        //        string strFileName = "Production Relay.xlsx";
        //        workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
        //        workbook.Close();
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}


        #endregion
    }
}