using Library.Model.External;
using Library.Service.External;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class DashBoardController : Controller
    {
        #region Constructor
        private readonly IChartService _chartService;
        public DashBoardController(IChartService chartService)
        {
            _chartService = chartService;
        }
		#endregion

		public ActionResult Index()
		{
			ViewBag.ControllerName = "ChartController";
			return View();
		}
		//public ActionResult Aplos()
		//{
		//	ViewBag.ControllerName = "ChartController";
		//	return View();
		//}

		[HttpPost]
        public ActionResult GetDetailsJList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            return Json(_chartService.GetDetailList(ChartColumnList, seq, cgid), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetGroupWiseColumnJList(string companyGroupId)
        {
            return Json(_chartService.GetGroupWiseColumnList(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetGroupWiseCompanyList(string companyGroupId)
        {
			
            return Json(_chartService.GetGroupWiseCList(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        #region Modal Controllers
        [HttpPost]
        public ActionResult ModalNotLoggedInEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            return Json(_chartService.NotLoggedInEmployeeList(ChartColumnList, seq, cgid), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ModalSubmittedEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            return Json(_chartService.SubmittedEmployeeList(ChartColumnList, seq, cgid), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ModalNotSubmittedEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            return Json(_chartService.NotSubmittedEmployeeList(ChartColumnList, seq, cgid), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult sModalNotLoggedInEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            return Json(_chartService.StNotLoggedInEmployeeList(ChartColumnList, seq, cgid), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult sModalSubmittedEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            return Json(_chartService.StSubmittedEmployeeList(ChartColumnList, seq, cgid), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult sModalNotSubmittedEmployeeList(IEnumerable<ChartColumnList> ChartColumnList, int seq, string cgid)
        {
            return Json(_chartService.StNotSubmittedEmployeeList(ChartColumnList, seq, cgid), JsonRequestBehavior.AllowGet);
        }
        //totalActivity
        [HttpPost]
        public ActionResult JtotalActivity(string cgid)
        {
            return Json(_chartService.TotalActivity(cgid), JsonRequestBehavior.AllowGet);
        }
        //FirstLoggedIn
        [HttpPost]
        public ActionResult JFirstLoggedIn(string cgid)
        {
            return Json(_chartService.FirstLoggedIn(cgid), JsonRequestBehavior.AllowGet);
        }
        //DayWiseSubmit
        [HttpPost]
        public ActionResult JDayWiseSubmit(string cgid)
        {
            return Json(_chartService.DayWiseSubmit(cgid), JsonRequestBehavior.AllowGet);
        }
        //totalDocument
        [HttpPost]
        public ActionResult JtotalDocument(string cgid)
        {
            return Json(_chartService.TotalDocument(cgid), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}
