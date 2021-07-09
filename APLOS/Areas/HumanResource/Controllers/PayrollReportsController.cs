using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.HumanResource.Payroll;
using Library.HumanResource.Payroll.Report;
using Library.HumanResource.Report.OT;
using Library.Model.HumanResources;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
//using static Library.Service.HumanResources.PayrollReportsService;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class PayrollReportsController : BaseController
    {
        #region Constructor

        private readonly PayrollReportsService _payrollReportsService;
        private readonly IEmployeeProfileService _employeeProfileService;




        public PayrollReportsController(
IEmployeeProfileService employeeProfileService

            )
        {
            _payrollReportsService = new PayrollReportsService();
            _employeeProfileService = employeeProfileService;

        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult SalaryStructureDaily()
        {
            return View();
        }

        public ActionResult SalaryStructureReportPlantWise()
        {
            return View();
        }


        public ActionResult SalaryStructureAndProcessedReport()
        {
            return View();
        }


        public ActionResult SeparatedEmployeeSalaryStructure()
        {
            return View();
        }


        public ActionResult SalaryProcessedReport()
        {
            return View();
        }
        public ActionResult SalarySheetBudgetaryOT()
        {
            return View();
        }

        public ActionResult SalaryProcessedReportCompliance()
        {
            return View();
        }

        public ActionResult SalarySummaryReport()
        {
            return View();
        }

        public ActionResult SalaryProcessedReportExtraOTCTC()
        {
            return View();
        }
        public ActionResult YearlySalaryProcessedReport()
        {
            return View();
        }

        //Salary Sheet Company Wise
        public ActionResult SalaryProcessedReportExtraOTCTCCompany()
        {
            return View();
        }


        #endregion -- Pages

        #region -- Operations
        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryStructure(string effectiveDate, string payRollGroup, Dictionary<string, string> parameters)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "EmployeeSalaryStructure" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                var workbook = _payrollReportsService.GetEmployeeSalaryStructure(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name, effectiveDate, payRollGroup, parameters);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);


                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryStructureDaily(string effectiveDate, string payRollGroup, Dictionary<string, string> parameters, string payDays)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "EmployeeSalaryStructure" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                Library.HumanResource.Report.Payroll.PayrollReports prr = new Library.HumanResource.Report.Payroll.PayrollReports();

                var workbook = prr.GetEmployeeSalaryStructureDaily(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name, effectiveDate, payRollGroup, parameters, payDays);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);


                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryStructurePlantWise(string effectiveDate, string plantList, Dictionary<string, string> parameters)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "EmployeeSalaryStructure" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                var workbook = _payrollReportsService.GetEmployeeSalaryStructurePlantWise(identity.CompanyGroupId, identity.CompanyId, plantList, identity.Name, effectiveDate, parameters);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);


                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetSeparatedEmployeeStructure(string effectiveDate, string FromDate, string ToDate, string payRollGroup, Dictionary<string, string> parameters)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "EmployeeSalaryStructure" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                var workbook = _payrollReportsService.GetSeparatedEmployeeStructure(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, effectiveDate, FromDate, ToDate, payRollGroup, parameters);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);


                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryStructureWithProceesd(string month, string year, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "EmployeeSalaryStructure" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                var workbook = _payrollReportsService.GetEmployeeSalaryStructureWithProcessed(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, payRollGroup, parameters, isActive, isSeperated, isMaternity);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);


                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryProcessedReport(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, identity.IsSysAdmin, identity.IsControlAdmin, false);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryProcessedReportSalLogWise(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReportSalaryLogWise(out int xlsRow, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpPost, Authorize]
        public ActionResult GetEmployeeArrearAndSalaryProcessedReportSalLogWise(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReportSalaryLogWise(out int xlsRow, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false);
               // workbook = _payrollReportsService.GetEmployeeArrearSalaryProcessedReportSalaryLogWise(workbook.Worksheets[0], xlsRow, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false);


                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryProcessedReportSalLogWiseDirectInDirectSalaryPayable(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool IsDirectInDirect)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReportSalLogWiseDirectInDirectSalaryPayable(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false, IsDirectInDirect);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryProcessedOTQtyAmountReport(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, double budgetedOT)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "OT" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedOTQtyAmountReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false, budgetedOT);
                workbook.Version = ExcelVersion.Excel2016;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }



        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryProcessedReportSalLogWiseCompliance(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReportSalaryLogWiseCompliance(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetBsrSalarySummaryReport(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                parameters = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySummary" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, identity.IsSysAdmin, identity.IsControlAdmin, isMaternity, true);
                workbook.Version = ExcelVersion.Excel2013;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpPost, Authorize]
        public ActionResult GetSalarySheetExtraOTCTCReport(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                parameters = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                WeekOFFandHolidayOT clsWeekOFFOTReport = new WeekOFFandHolidayOT();

                var fileName = bplib.clsWebLib.GetMonthName(month) + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = clsWeekOFFOTReport.GetSalarySheetExtraOTCTCReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, identity.IsSysAdmin, identity.IsControlAdmin, isMaternity, false);
                workbook.Version = ExcelVersion.Excel2013;
                workbook.SaveAs(fullPath);

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
            var jsondata = Json(_payrollReportsService.GetEmpInfo(identity.CompanyGroupId, identity.PlantId, effectiveDate, salaryProcessId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetEmpInfoDaily(string effectiveDate, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string attdnStatusCatg = "'Present','Late','Leave'";
            Library.HumanResource.Report.Payroll.PayrollReports prr = new Library.HumanResource.Report.Payroll.PayrollReports();

            var jsondata = Json(prr.GetEmpInfoDaily(identity.CompanyGroupId, identity.PlantId, effectiveDate, attdnStatusCatg, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpPost, Authorize]
        public ActionResult GetEmpInfoSalaryPorcessed(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_payrollReportsService.GetEmpInfoSalaryPorcessed(identity.CompanyGroupId, identity.PlantId, effectiveDate, salaryProcessId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetSeparatedEmpInfo(string effectiveDate, string FromDate, string ToDate, string salaryProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payrollReportsService.GetSeparatedEmpInfo(identity.CompanyGroupId, identity.PlantId, effectiveDate, FromDate, ToDate, salaryProcessId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPayRollGroupCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payrollReportsService.GetPayRollGroupCbo(identity.IsSysAdmin, identity.IsControlAdmin, identity.PlantId, identity.UserId), JsonRequestBehavior.AllowGet);
        }


        //Salary Sheet Company Wise Operations
        [HttpPost, Authorize]
        public ActionResult GetSalarySheetExtraOTCTCReportCompany(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                //parameters = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                WeekOFFandHolidayOT clsWeekOFFOTReport = new WeekOFFandHolidayOT();

                var fileName = bplib.clsWebLib.GetMonthName(month) + "-" + year + "SalarySheetCompanyWise" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                SalarySheetCompanyService ss = new SalarySheetCompanyService();
                var workbook = ss.GetSalarySheetExtraOTCTCReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, identity.IsSysAdmin, identity.IsControlAdmin, isMaternity, false);
                workbook.Version = ExcelVersion.Excel2013;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }



        //Salary Sheet CompanyWise
        [HttpPost, Authorize]
        public ActionResult GetEmpInfoSalaryPorcessedCompany(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            //Sayanto Change Company Wise
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            SalarySheetCompanyService ss = new SalarySheetCompanyService();
            var jsondata = Json(ss.GetEmpInfoSalaryPorcessedCompany(identity.CompanyGroupId, identity.CompanyId, effectiveDate, salaryProcessId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        //End Company Wise Operations


        #region MyRegion

        [HttpPost, Authorize]
        public ActionResult GetEmpInfoYearlySalaryPorcessed(string taxYearId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.HumanResource.Report.Payroll.PayrollReports prr = new Library.HumanResource.Report.Payroll.PayrollReports();
            var jsondata = Json(prr.GetEmpInfoYearlySalaryPorcessed(identity.CompanyGroupId, identity.PlantId, taxYearId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryProcessedReportYearlySalary(string taxYearId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool withGoodWork)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = taxYearId + "- SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                Library.HumanResource.Report.Payroll.PayrollReports prr = new Library.HumanResource.Report.Payroll.PayrollReports();

                var workbook = prr.GetEmployeeSalaryProcessedReportSalaryYearly(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, taxYearId, parameters, isActive, isSeperated, isMaternity, withGoodWork);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }


        #endregion

        #endregion -- Operations


    }
}