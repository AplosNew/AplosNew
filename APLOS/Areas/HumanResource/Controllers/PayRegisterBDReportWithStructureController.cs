using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.HumanResources;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using Syncfusion.XlsIO;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.HumanResources.PayRegisterBDReportService;
using static OTSBD.clsReport;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class PayRegisterBDReportWithStructureController : BaseController
    {
        #region Constructor

        Library.HumanResource.Report.Payroll.clsPayRegister _payRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();

        private readonly IEmployeeProfileService _employeeProfileService;




        public PayRegisterBDReportWithStructureController(
              IEmployeeProfileService employeeProfileService

            )
        {
            _payRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();
            _employeeProfileService = employeeProfileService;

        }

        #endregion Constructor

        #region -- Pages

  
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetSalaryprocessIdCbo(string month,string year, string IsCompletedMonth)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payRegisterBDReportService.GetSalaryprocessIdCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, month, year, IsCompletedMonth), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLanguageIdCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeProfileService.GetDefaultCbo(identity.CompanyGroupId,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPayGroupCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payRegisterBDReportService.GetPayGroupCbo(identity.IsControlAdmin,identity.IsSysAdmin,identity.UserId), JsonRequestBehavior.AllowGet);
        }
        
      
       
        [HttpGet, Authorize]
        public ActionResult GetPayRegisterReportBangla(string month, string year, string salaryProcessId, string divisionId, string unitId, string sectionId, string subSectionId, string departmentId, string payGroupId,string employeeCategoryId, string paymentDate,string printDate, string paymentMode,string languageId,string selPaymentMode,string selEmpCatg,string sqlInStatement, bool isActive, bool isSeperated, bool isMaternity)
        {
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.ToInt());
            var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
            PayRegisterParamList PayRegisterParam = new PayRegisterParamList();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            PayRegisterParam.PlantId = identity.PlantId;
            PayRegisterParam.CompanyGroupId = identity.CompanyGroupId;
            PayRegisterParam.CompanyId = identity.CompanyId;
            PayRegisterParam.FromDate = 1 + "-" + monthName + "-" + year;
            PayRegisterParam.ToDate = daysInMonth + "-" + monthName + "-" + year;
            PayRegisterParam.Month = month;
            PayRegisterParam.Year = year;
            PayRegisterParam.SalaryProcessId = salaryProcessId;
            PayRegisterParam.UnitId = unitId;
            PayRegisterParam.DivisionId = divisionId;
            PayRegisterParam.SubSectionId = subSectionId;
            PayRegisterParam.SectionId = sectionId;
            PayRegisterParam.DepartmentId = departmentId;
            PayRegisterParam.PayGroup = payGroupId;
            PayRegisterParam.EmpCategoryId = employeeCategoryId;
            PayRegisterParam.PaymentMode = paymentMode;
            PayRegisterParam.LanguageId = languageId;



            var fileName = monthName + "-" + year + "PayRegister" + DateTime.Now.ToString("yyMMdd") + ".xls";
            //var workbook = _payRegisterBDReportService.EmployeeSalaryRegisterWithStructure(PayRegisterParam,paymentDate, sqlInStatement);
            var workbook = _payRegisterBDReportService.EmployeeSalaryRegisterWithStructureNew(PayRegisterParam,paymentDate, printDate, sqlInStatement,  isActive,  isSeperated,  isMaternity);

            
            workbook.Version = ExcelVersion.Excel97to2003;
            workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
            return null;
        }
        #endregion -- Operations

     
    }
}