using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using Syncfusion.XlsIO;
using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ManpowerBudgetDesignationReportController : BaseController
	{
        #region Constructor
        private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly ICompliedShiftService _compliedShiftService;
        private SqlRepository _sqlRepository = new SqlRepository();
        private readonly Library.HumanResource.Report.Employee.BudgetedDesignationReport _mbpReport = new Library.HumanResource.Report.Employee.BudgetedDesignationReport();

        public ManpowerBudgetDesignationReportController(
			  ICompliedShiftService compliedShiftService ,
               IPayRegisterBDReportService payRegisterBDReportService
            )
		{
			_compliedShiftService = compliedShiftService;
            _payRegisterBDReportService = payRegisterBDReportService;

        }

		#endregion Constructor

		#region -- Pages

		
		public ActionResult Aplos()
		{
			return View();
		}

        #endregion -- Pages-

        #region -- Operations	   


        //[HttpGet, Authorize]
        //public ActionResult GetManpowerBudgtDesignationReport(string effectiveDate)
        //{


        //    try
        //    {
        //        ReportUtility oReportUtility = new ReportUtility();

        //    }
        //    catch (Exception ex)
        //    {
        //        //throw ex;

        //    }
        //}


        [HttpGet, Authorize]
        public ActionResult GetManpowerBudgtDesignationReport(string effectiveDate,string companyId,string plantIds)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = "DailyDayStatus" + DateTime.Now.ToString("yyMMdd") + ".xlsx";
                var workbook = _mbpReport.GetBudgetedDesignation(plantIds, companyId, identity.Name);
                workbook.Version = ExcelVersion.Excel2016;
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetBudgetedDesignationDetail(string plantIds,string date)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = "DailyDayStatus" + DateTime.Now.ToString("yyMMdd") + ".xlsx";
                var workbook = _mbpReport.GetBudgetedDesignationDetail(plantIds, identity.CompanyId,  identity.Name,date);
                workbook.Version = ExcelVersion.Excel2016;
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion -- Operations



    }
  
}