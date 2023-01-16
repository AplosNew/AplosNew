using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll;
using Library.HumanResource.Payroll.Report;
using Library.HumanResource.Report.OT;
using Library.Model.HumanResources;
using Library.Security.Core;
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
        private readonly ISqlRepository _sqlRepository;


        public PayrollReportsController(
IEmployeeProfileService employeeProfileService, ISqlRepository sqlRepository

            )
        {
            _payrollReportsService = new PayrollReportsService();
            _employeeProfileService = employeeProfileService;
            _sqlRepository = sqlRepository;
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
        public ActionResult ArrearVsPayroll()
        {
            return View();
        }
        public ActionResult ArrearVsPayrollTotal()
        {
            return View();
        }

        public ActionResult SalaryStructureReportPlantWise()
        {
            return View();
        }
        public ActionResult SalaryIntegrationWithThirdParty()
        {
            return View();
        }


        public ActionResult SalaryStructureAndProcessedReport()
        {
            return View();
        }

        public ActionResult SalaryStructureAndProcessedReportNew()
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

        public ActionResult SalaryProcessedReportNew()
        {
            return View();
        }

        public ActionResult SalaryNotDisbursed()
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
        public ActionResult GetEmployeeSalaryStructureWithProceesdNew(string month, string year, string PlantId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "EmployeeSalaryStructure" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;

                var workbook = _payrollReportsService.GetEmployeeSalaryStructureWithProcessedNew(identity.CompanyGroupId, identity.CompanyId, PlantId, identity.UserId, month, year, payRollGroup, parameters, isActive, isSeperated, isMaternity);
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


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, identity.IsSysAdmin, identity.IsControlAdmin, false,"");
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
        public ActionResult GetEmployeeSalaryProcessedReportSalLogWiseRpt(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReportSalaryLogWiseRpt(out int xlsRow, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false,identity.Name);
                //workbook.Version = ExcelVersion.Excel97to2003;
                //workbook.SaveAs(fullPath);
                //return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

                return Json(new { FullPath = workbook, FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
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
        [HttpPost, Authorize]
        public ActionResult GetEmployeeArrearAndSalaryProcessedReportSalLogWise(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReportSalaryLogWise(out int xlsRow, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false);
                workbook = _payrollReportsService.GetEmployeeArrearSalaryProcessedReportSalaryLogWise(workbook.Worksheets[0], xlsRow, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false);


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
        public ActionResult GetEmployeeArrearTotalAndSalaryProcessedReportSalLogWise(string ArrearBatchNo, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                //  var workbook =  _payrollReportsService.GetEmployeeSalaryProcessedReportSalaryLogWise(out int xlsRow, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false);
                var workbook = _payrollReportsService.GetEmployeeArrearTotalSalaryProcessedReportSalaryLogWise(ArrearBatchNo, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, false);


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
        public ActionResult GetBsrSalarySummaryReport(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity,string PlantId,string typeList)
        {
            try
            {
                string typeLists = string.Empty;
                parameters = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (!string.IsNullOrEmpty(typeList))
                {
                typeLists = "'" + typeList.Replace(",", "','") + "'";//replaced with ""
                }
                else
                {
                    throw new Exception("Please select the Employee Code Type");
                }
                var fileName = month + "-" + year + "SalarySummary" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReport(identity.CompanyGroupId, identity.CompanyId, PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, identity.IsSysAdmin, identity.IsControlAdmin, isMaternity, true, typeLists);
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
        public ActionResult GetSalarySheetExtraOTCTCReport(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, string PlantId , string TypeId)
        {
            try
            {
                string typeId = string.Empty;
                if (!string.IsNullOrEmpty(TypeId))
                {
                    typeId = "'" + TypeId.Replace(",", "','") + "'";//replaced with ""
                }
                else
                {
                    throw new Exception("Please Select the Employee Code Type");
                }
                // parameters = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                WeekOFFandHolidayOT clsWeekOFFOTReport = new WeekOFFandHolidayOT();

                var fileName = bplib.clsWebLib.GetMonthName(month) + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = clsWeekOFFOTReport.GetSalarySheetExtraOTCTCReportWithType(identity.CompanyGroupId, identity.CompanyId, PlantId, identity.UserId, month, year, typeId ,salaryProcessId, payRollGroup, parameters, isActive, isSeperated, identity.IsSysAdmin, identity.IsControlAdmin, isMaternity, false);
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
        public ActionResult GetSalarySheetExtraOTCTCReportOriginal(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, string PlantId)
        {
            try
            {
                // parameters = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                WeekOFFandHolidayOTOriginal clsWeekOFFOTReport = new WeekOFFandHolidayOTOriginal();

                var fileName = bplib.clsWebLib.GetMonthName(month) + "-" + year + "SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = clsWeekOFFOTReport.GetSalarySheetExtraOTCTCReport(identity.CompanyGroupId, identity.CompanyId, PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, identity.IsSysAdmin, identity.IsControlAdmin, isMaternity, false);
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
        public ActionResult GetEmpInfoSalaryPorcessedWithType(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string PlantId , string TypeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string Plant = string.Empty;
            string typeId = string.Empty;
            if (!string.IsNullOrEmpty(PlantId))
            {
                Plant = "'" + PlantId.Replace(",", "','") + "'";//replaced with ""
            }
            else
            {
                Plant = "'" + identity.PlantId + "'";
            }

            if (!string.IsNullOrEmpty(TypeId))
            {
                typeId = "'" + TypeId.Replace(",", "','") + "'";//replaced with ""
            }
            else
            {
                throw new Exception("Please Select the Employee Code Type");
            }
            var jsondata = Json(_payrollReportsService.GetEmpInfoSalaryPorcessedWithType(identity.CompanyGroupId, Plant, effectiveDate, salaryProcessId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity , typeId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpPost, Authorize]
        public ActionResult GetEmpInfoSalaryPorcessed(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string Plant = string.Empty;
            string typeId = string.Empty;
            if (!string.IsNullOrEmpty(PlantId))
            {
                Plant = "'" + PlantId.Replace(",", "','") + "'";//replaced with ""
            }
            else
            {
                Plant = "'" + identity.PlantId + "'";
            }

            var jsondata = Json(_payrollReportsService.GetEmpInfoSalaryPorcessed(identity.CompanyGroupId, Plant, effectiveDate, salaryProcessId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetEmpInfoSalaryFromArrearPorcessed(string ArrearProcessBatchId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_payrollReportsService.GetEmpInfoArrearPorcessedAll(ArrearProcessBatchId), JsonRequestBehavior.AllowGet);
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
        public ActionResult GetEmpInfoYearlySalaryPorcessedbyFromYear(string ToYear, string ToMonth, bool isActive, bool isSeperated, bool isMaternity, string FromYear, string FromMonth)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            Library.HumanResource.Report.Payroll.PayrollReports prr = new Library.HumanResource.Report.Payroll.PayrollReports();
            var jsondata = Json(prr.GetEmpInfoYearlySalaryPorcessedFromYear(identity.CompanyGroupId, identity.PlantId, ToYear, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity,ToMonth,FromYear,FromMonth), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public ActionResult GetAllArrearProcessInfo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT distinct apm.ArrearProcessBatchId,CONCAT( apm.[Description], ' From ',
                    FORMAT(apm.ArrearProcessFromDate,'dd-MMM-yyyy') , ' To ',
                    FORMAT(apm.ArrearProcessToDate,'dd-MMM-yyyy') , ' Processed By ',apm.AddedBy)  ArrearDesc
                    FROM ArrearProcMaster AS apm";
            SqlRepository sqlRepository = new SqlRepository();

            var jsondata = Json(sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }



        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryProcessedReportYearlySalary(string FromYear, string FromMonth, string ToYear, string ToMonth, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool withGoodWork)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = FromYear+ FromMonth + " - "+ ToYear + ToMonth + "- SalarySheet" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                Library.HumanResource.Report.Payroll.PayrollReports prr = new Library.HumanResource.Report.Payroll.PayrollReports();

                var workbook = prr.GetEmployeeSalaryProcessedReportSalaryYearlyWise(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId,  parameters, isActive, isSeperated, isMaternity, withGoodWork, FromYear, FromMonth, ToYear, ToMonth);
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

        [HttpGet, Authorize]
        public JsonResult GetPlantList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var str = @"select Id PlantId,UserName PlantName  from ORG.PLANT where CompanyId='" + identity.CompanyId + "'";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult SalaryIntegrationWithThirdpartyXls(string plantId, string month, string year,string TypeId)
        {
            try
            {
                string typeId = string.Empty;

                plantId = "'" + plantId.Replace(",", "','") + "'";
                if (!string.IsNullOrEmpty(TypeId))
                {
                    typeId = "'" + TypeId.Replace(",", "','") + "'";//replaced with ""
                }
                else
                {
                    throw new Exception("Please Select the Employee Code Type");
                }


                ExcelEngine excelEngine = new ExcelEngine();

                Library.HumanResource.Payroll.PayrollReportsService service = new Library.HumanResource.Payroll.PayrollReportsService();
                IWorkbook workbook = service.SalaryIntegrationWithThirdparty(plantId, year, month, typeId, excelEngine);

                string strFileName = "SalaryIntegrationWithThirdparty.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();

            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return null;
        }
        #endregion -- Operations


        // Written By Nitesh 2023-01-04
        #region Salary Not Disbursed
        [HttpPost, Authorize]
        public ActionResult GetEmployeeSalaryNotDisbursedProcessedReportSalLogWiseNew(string empstatus)
        {
            try
            {

                string fileName = "";
                fileName = GetEmployeeSalaryNotDisbursedReport(empstatus, "ContractTransactionSummaryReport");

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

               
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        // Written By Nitesh 2023-01-04
        #region Salary Not Disbursed
       
        public string GetEmployeeSalaryNotDisbursedReport(string empstatus, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            try
            {

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Salary Not Disbursed";
                sheet = workbook.Worksheets[0];
                DataTable data;
                GetNewEmployeeInfoDetailSalaryNotDisbursedLogWise(empstatus, out data);

                int ROW = 6; int COL = 1;

                #region Columns
             
                sheet[ROW, COL].Text = "Month Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColMonthName = COL;
                COL++;

                sheet[ROW, COL].Text = "Year";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColYear = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEC = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColEN = COL;
                COL++;

                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDOJ = COL;
                COL++;

                sheet[ROW, COL].Text = "DOS";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDOS = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEcg = COL;
                COL++;

                sheet[ROW, COL].Text = "Department";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDep = COL;
                COL++;

                sheet[ROW, COL].Text = "Section";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColSec = COL;
                COL++;

                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColSS = COL;
                COL++;

                sheet[ROW, COL].Text = "Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDesg = COL;
                COL++;

                sheet[ROW, COL].Text = "Payment Mode";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPM = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBank = COL;
                COL++;

                sheet[ROW, COL].Text = "Bank Account No";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBAN = COL;
                COL++;

                sheet[ROW, COL].Text = "IFSC Code";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColIFSC = COL;
                COL++;

                sheet[ROW, COL].Text = "Net Payable";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColNetPay = COL;
                //COL++;
                // COL++;
                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {

                    sheet[ROW, ColMonthName].Text = data.Rows[i]["MonthName"].ToString();
                    sheet[ROW, ColYear].Number = clsStaticInfo.dbl(data.Rows[i]["YearNo"].ToString());
                    sheet[ROW, ColEC].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet[ROW, ColEN].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColDOJ].DateTime = Convert.ToDateTime(data.Rows[i]["DOJ"].ToString());
                    sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                    sheet[ROW, ColEcg].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColDep].Text = data.Rows[i]["Department"].ToString();
                    sheet[ROW, ColSec].Text = data.Rows[i]["Section"].ToString();
                    sheet[ROW, ColSS].Text = data.Rows[i]["SubSection"].ToString();
                    sheet[ROW, ColDesg].Text = data.Rows[i]["Designation"].ToString();
                    sheet[ROW, ColPM].Text = data.Rows[i]["PaymentMode"].ToString();
                    sheet[ROW, ColBank].Text = data.Rows[i]["BankName"].ToString();
                    sheet[ROW, ColBAN].Text = data.Rows[i]["BankAccNo"].ToString();
                    sheet[ROW, ColIFSC].Text = data.Rows[i]["IFSCCode"].ToString();
                    sheet[ROW, ColNetPay].Number = clsStaticInfo.dbl(data.Rows[i]["NetPayable"].ToString());


                    ROW++;
                }



                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Salary Not Disbursed Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public void GetNewEmployeeInfoDetailSalaryNotDisbursedLogWise(string empstatus, out DataTable data)
        {
            string strSQL;
            var sqlCondition = "";
            try
            {
                
                if (empstatus == "Active" || empstatus == "Separated")
                {
                    sqlCondition = "SH.HeadCategory = 'Net Payable' and SL.IsDisbursed = 0 and SPC.DisbusmentAmount > 0 and EI.EmployeeStatus = '" + empstatus + "' order by EI.EmployeeCode DESC";
                }
               
                else
                {
                    sqlCondition = "SH.HeadCategory = 'Net Payable' and SL.IsDisbursed = 0 and SPC.DisbusmentAmount > 0 order by EI.EmployeeCode DESC";
                }
                strSQL = @"select 
[MonthName]=CASE WHEN SPM.MonthNo=1 THEN 'Jan'   
              WHEN  SPM.MonthNo=2 THEN 'Feb'
			  WHEN  SPM.MonthNo=3 THEN 'Mar'
			  WHEN  SPM.MonthNo=4 THEN 'Apr'
			  WHEN  SPM.MonthNo=5 THEN 'May'
			  WHEN  SPM.MonthNo=6 THEN 'Jun'
			  WHEN  SPM.MonthNo=7 THEN 'Jul'
			  WHEN  SPM.MonthNo=8 THEN 'Aug'
			  WHEN  SPM.MonthNo=9 THEN 'Sep'
			  WHEN  SPM.MonthNo=10 THEN 'Oct'
			  WHEN  SPM.MonthNo=11 THEN 'Nov'
			  WHEN  SPM.MonthNo=12 THEN 'Dec' ELSE '' END, SPM.YearNo
,EI.EmployeeCode, EI.EmployeeName, FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS, EC.UserName EmployeeCategory, DP.UserName Department, S.UserName Section, SS.UserName SubSection, D.UserName Designation, EI.PaymentMode, B.UserName BankName
                        ,EBI.IFSCCode, EBI.BankAccNo, SPC.DisbusmentAmount NetPayable
                        from SalaryProcChild SPC
                        LEFT JOIN SalaryProcMaster SPM ON SPM.SystemID = SPC.SlrProcMstSystemID 
                        LEFT JOIN SalaryLock SL ON SL.EmpSystemId = SPC.EmpInfoSystemID AND SL.YearNo = SPM.YearNo AND SPM.MonthNo = SL.MonthNo
                        LEFT JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId = SPC.SlrProcMstSystemID and SPLD.EmpSystemId = SPC.EmpInfoSystemID
                        LEFT JOIN SalaryHead SH on SH.SalaryHeadID = SPC.SalaryHeadID
                        LEFT JOIN MST.ManpowerBudget MPB on MPB.Id = SPLD.BudgetCode
                        LEFT JOIN EmployeeInformation EI on EI.SystemId = SPC.EmpInfoSystemID
						LEFT JOIN EmployeeBankInfo EBI ON EBI.EmpSystemID = EI.SystemId
                        LEFT JOIN HKP.Bank B on B.Id = EBI.BankSystemID
                        LEFT JOIN ORG.Position POS on POS.Id = MPB.PositionId
                        LEFT JOIN ORG.Department DP on DP.Id = POS.DepartmentId
                        LEFT JOIN ORG.Section S on S.Id = POS.SectionId
                        LEFT JOIN ORG.SubSection SS on SS.ID = POS.SubSectionId
                        LEFT JOIN HKP.Designation D on D.Id = SPLD.DesignationId
                        LEFT JOIN HKP.LegalDesignation LD on LD.Id = SPLD.LegalDesignationId
                        LEFT JOIN MST.DesignationMasterLegalDesignation DMLD on DMLD.LegalDesignationId = SPLD.LegalDesignationId
                        LEFT JOIN MST.DesignationMaster DM ON DM.Id = DMLD.DesignationMasterId
                        LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                        LEFT JOIN [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID

                        where " + sqlCondition + "";

              
                    data = _sqlRepository.GetDataTable(strSQL);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }//End Function
        #endregion
        #endregion Salary Not Disbursed

    }
}