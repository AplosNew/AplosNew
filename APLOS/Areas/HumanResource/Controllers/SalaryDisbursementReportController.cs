using Aplos.Controllers;
using Aplos.Properties;
using bplib;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll;
using Library.HumanResource.Payroll.SalaryProcess;
using Library.Model.HumanResources;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;


namespace Aplos.Areas.HumanResource.Controllers
{
    public class SalaryDisbursementReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly PayrollReportsService _payrollReportsService;
        public SalaryDisbursementReportController(
            ISqlRepository sqlRepository
            )
        {
            _payrollReportsService = new PayrollReportsService();
            _sqlRepository = sqlRepository;

        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations
        [HttpPost,Authorize]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            JsonResult json = Json((new clsSalaryLock().GetEmpInfoForSalaryDisbursement((CustomIdentity)Thread.CurrentPrincipal.Identity, effectiveDate, salaryProcessId, isActive, isSeperated, isMaternity)), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryProcessedReportSalLogWiseNew(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReportSalaryLogWiseNew(out int xlsRow, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        #endregion -- Operations
    }
}