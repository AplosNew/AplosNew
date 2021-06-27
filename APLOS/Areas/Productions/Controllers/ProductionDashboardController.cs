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
        #endregion
    }
}