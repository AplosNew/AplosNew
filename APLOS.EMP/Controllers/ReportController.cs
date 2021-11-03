#region Using

using Library.Model.External;
using Library.Service.External;
using Syncfusion.XlsIO;
using System.Web.Mvc;
using Library.Core;

#endregion

namespace Aplos.Controllers
{
    public class ReportController : BaseController
    {
        #region Constructor
  private readonly IEmployeeService _es;
        public ReportController(IEmployeeService es)
        {
            _es = es;
        }
        #endregion

        #region Pages
       
        

        public ActionResult Aplos()
        {
            ViewBag.ControllerName = "reportController";
            return View();
        }
        public ActionResult AplosDate()
        {
            ViewBag.ControllerName = "reportController";
            return View();
        }
        public ActionResult AplosException()
        {
            ViewBag.ControllerName = "reportController";
            return View();
        }
        public ActionResult AplosIndividual()
        {
            ViewBag.ControllerName = "reportController";
            return View();
        }
        #endregion

        //public ActionResult GetEmployeeList(GridParameter parameters, string id, string initialpin)
        //{
        //    return Json(_employeeService.QueryEmployeeAccess(parameters, id, initialpin), JsonRequestBehavior.AllowGet);
        //}

        public ActionResult EmployeeInfo(string cg,string un, bool wa,bool nl, bool s, bool ns)
        {
            var param = new ReportParam
            {
                CompanyGroupId = cg,
                EmployeeName = un,
                notloggedin = nl,
                withoutactivity = wa,
                Submitted = s,
                NotSubmitted = ns
            };

            string fileName = "EmployeeInfo " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            IWorkbook workbook = _es.EmployeeInfo(param);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }
        public ActionResult DateWiseActivity(string cg, string un,string fd ,string td)
        {
            var param = new ReportParam
            {
                CompanyGroupId = cg,
                EmployeeName = un
            };

            string fileName = "ActivityInfo " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            IWorkbook workbook = _es.ActivityInfo(param, fd, td);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        public ActionResult Exception(string cg, string un)
        {
            var param = new ReportParam
            {
                CompanyGroupId = cg,
                EmployeeName = un
            };

            string fileName = "Exception Status " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            IWorkbook workbook = _es.ExceptionInfo(param);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        public ActionResult IndividualStatus(string cg, string un,string uid)
        {
            var param = new ReportParam
            {
                CompanyGroupId = cg,
                EmployeeName = un,
                EmployeeId = uid
            };

            string fileName = "Individual Status " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            IWorkbook workbook = _es.IndividualInfo(param);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        [HttpGet, AllowAnonymous]
		public ActionResult GetEmployeeByCompanyGroup(GridParameter parameters, string companyGroupId)
		{
			return Json(_es.GetEmployeeByCompanyGroup(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
		}
	}
}