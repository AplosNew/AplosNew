using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.HumanResource.Payroll;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class PaySlipsNewController : BaseController
    {
        #region Constructor

        private readonly PayrollReportsService _payrollReportsService;
        private readonly IEmployeeProfileService _employeeProfileService;

        Library.HumanResource.Report.Payroll.clsPayRegister _payRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();



        public PaySlipsNewController(IEmployeeProfileService employeeProfileService
            )
        {
            _payrollReportsService = new PayrollReportsService();
            _employeeProfileService = employeeProfileService;
            _payRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();

        }

        #endregion Constructor

        #region -- Pages
       
        public ActionResult PaySlipsNew()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpPost, Authorize]
        public ActionResult GetEmployeePaySlip(string month, string year, string salaryProcessId, Dictionary<string, string> parameters, string languageId, bool isActive, bool isSeperated, bool isMaternity, bool IsIncludingZeroHeads, bool singleEmployee, string reportFormat)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "PaySlip" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                //GetEmployeePaySlipWithBal
                var workbook = _payrollReportsService.GetEmployeePaySlipNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, parameters, languageId, isActive, isSeperated, isMaternity, IsIncludingZeroHeads, singleEmployee);

                workbook.Version = ExcelVersion.Excel2016;
                //workbook.SaveAs(fullPath);
                if (reportFormat=="Pdf")
                {
                    var converter = new ExcelToPdfConverter(workbook);
                    ExcelToPdfConverterSettings _settings = new ExcelToPdfConverterSettings();
                    _settings.AutoDetectComplexScript = true;
                    var pdfDoc = converter.Convert(_settings);

                    fileName = month + "-" + year + "PaySlip" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".pdf";
                    string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                    pdfDoc.Save(fullPathPDF);
                }
                else
                {
                    workbook.SaveAs(fullPath);
                    workbook.Close();
                }
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetEmployeePaySlipContractor(string month, string year, string salaryProcessId, Dictionary<string, string> parameters, string languageId, string contractorId, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "PaySlipCotractor" + month + year + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                var workbook = _payRegisterBDReportService.GetEmployeePaySlipContractor(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, parameters, languageId, contractorId, isActive, isSeperated, isMaternity);

                workbook.Version = ExcelVersion.Excel2016;
                //workbook.SaveAs(fullPath);
                var converter = new ExcelToPdfConverter(workbook);
                var pdfDoc = converter.Convert();
                fileName = month + "-" + year + "PaySlip" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".pdf";
                string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                pdfDoc.Save(fullPathPDF);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payrollReportsService.GetEmpInfoSalaryPorcessed(identity.CompanyGroupId, identity.PlantId, effectiveDate, salaryProcessId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPayRollGroupCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payrollReportsService.GetPayRollGroupCbo(identity.IsSysAdmin, identity.IsControlAdmin, identity.PlantId, identity.UserId), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}