using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
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
    public class FinalDeductionReportController : BaseController
    {
        #region Constructor

        private readonly PayrollReportsService _payrollReportsService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;


        public FinalDeductionReportController(
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
        public ActionResult GetEmployeeSalaryStructureWithProceesdNew(string month, string year,string PlantId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
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
        public ActionResult GetEmployeeSalaryProcessedReportSalLogWiseNew(string month, string year,  Dictionary<string, string> parameters, string format)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "FinalDeduction" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetFinalDeductionReportCompanyWise(out int xlsRow, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, parameters, format, false);
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
        public JsonResult GetEmployeeList(string month, string year,string PlantId,string format)
        {
            var str = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (format == "1")
            {
                 str = @"SELECT  DISTINCT E.SystemID EmpSystemId,[isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'), isnull(E.VendorId,'') as Vendor 
									, isnull(p.UserName,'') as Contractor, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus 
											
											  ,F.Id PlantID, F.UserName PlantName, 
											FU.UserName UnitName,  DV.UserName DivisionName,  DP.UserName DepartmentName,
											 S.UserName SectionName
											 , SS.UserName SubSectionName,
											EC.UserName EmpCategoryName,
                                         Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation
											,LS.UserName Grade
                                     FROM EmployeeInformation E
                                     left join hkp.Party p on p.Id = E.VendorId
									 			LEFT JOIN ORG.Plant F ON F.Id= E.PlantId
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON E.LegalDesignationId = LDS.Id
LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LDS.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                        left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                  JOIN [SCS].[LegalSalaryGrade] LS ON LS.Id = LSGD.LegalSalaryGradeId and ls.PlantId=lsgd.PlantId
												
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id
												
Where E.PlantId IN( " + PlantId + @"  ) AND E.SystemId IN (												 
												 SELECT ES.EmployeeId FROM EmpServiceData AS ES
												 JOIN EmpServiceCategory EC on EC.Id=ES.EmployeeServiceCategoryId
JOIN EmpServiceType AS ET ON ET.id=EC.EmpServiceTypeId
JOIN SalaryHead AS SH ON SH.SalaryHeadID=ET.SalaryHeadID
WHERE  MONTH(ES.Date)='" + month + @"' AND Year(ES.Date)='" + year + @"') ";
            }
            else
            {
                 str = @"SELECT  DISTINCT E.SystemID EmpSystemId,[isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'), isnull(E.VendorId,'') as Vendor 
									, isnull(p.UserName,'') as Contractor, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus 

											  ,F.Id PlantID, F.UserName PlantName, 
											FU.UserName UnitName,  DV.UserName DivisionName,  DP.UserName DepartmentName,
											 S.UserName SectionName
											 , SS.UserName SubSectionName,
											EC.UserName EmpCategoryName,
                                         Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation
											,LS.UserName Grade
                                     FROM EmployeeInformation E
                                     left join hkp.Party p on p.Id = E.VendorId
									 			LEFT JOIN ORG.Plant F ON F.Id= E.PlantId
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON E.LegalDesignationId = LDS.Id
LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LDS.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                        left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = E.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                  JOIN [SCS].[LegalSalaryGrade] LS ON LS.Id = LSGD.LegalSalaryGradeId and ls.PlantId=lsgd.PlantId
												
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id
												
Where E.PlantId IN( " + PlantId + @"  ) AND E.SystemId IN (												 
												 SELECT m.EmpInfoSystemID FROM MonthWiseExtraSalaryAmtMaster AS m
JOIN MonthWiseExtraSalaryAmtChild AS C ON c.MWESAMasterSystemID=m.SystemID
JOIN SalaryHead AS sh ON sh.SalaryHeadID=c.SalaryHeadID
WHERE m.MonthNo='" + month + @"' AND m.YearNo='" + year + @"') ";
            }

            var jsondata = Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public JsonResult GetPlantList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var str = @"select Id PlantId,UserName PlantName  from ORG.PLANT where CompanyId='" + identity.CompanyId + "'";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
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
        public ActionResult GetBsrSalarySummaryReport(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                parameters = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = month + "-" + year + "SalarySummary" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, identity.IsSysAdmin, identity.IsControlAdmin, isMaternity, true,"");
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
        public ActionResult GetSalarySheetExtraOTCTCReport(string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, string PlantId)
        {
            try
            {
                // parameters = null;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                WeekOFFandHolidayOT clsWeekOFFOTReport = new WeekOFFandHolidayOT();

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
        public ActionResult GetEmpInfoSalaryPorcessed(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity, string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string Plant = string.Empty;
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