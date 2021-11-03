using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Syncfusion.DocIO.DLS;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class AttendanceManagementController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public AttendanceManagementController(
              IAttendanceManagementService AttendanceManagementService, IEmployeeProfileService employeeProfileService, ISqlRepository R
            )
        {
            _AttendanceManagementService = AttendanceManagementService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        
        public ActionResult Aplos()
        {
            return View();
        }
        [Authorize]
        public ActionResult LateAttendancePosting()
        {
            return View();
        }
        [Authorize]
        public ActionResult JobCard()
        {
            return View();
        }
        [Authorize]
        public ActionResult ComplianceJobCard()
        {
            return View();
        }
        public ActionResult leaveschecklistreport()
        {
            return View();
        }
        public ActionResult NationalFestival()
        {
            return View();
        }
       
        [Authorize]
        public ActionResult OtFinal()
        {
            return View();
        }

        [Authorize]
        public ActionResult DailyDayStatus()
        {
            return View();
        }
        
        public ActionResult ManualOutTime()
        {
            return View();
        }
        [Authorize]
        public ActionResult RawDataReport()
        {
            return View();
        }
        [Authorize]
        public ActionResult TiffinBillReport()
        {
            return View();
        }
        [Authorize]
        public ActionResult TiffinBillSummaryReports()
        {
            return View();
        }
         
        public ActionResult MaternityLeaveReport()
        {
            return View();
        }
        [Authorize]
        public ActionResult FinalSettlementReport()
        {
            return View();
        }

        public ActionResult ActualOTAndPlan()
        {
            return View();
        }

        public ActionResult TiffinBill()
        {
            return View();
        }
        public ActionResult AttendanceSummaryStatus()
        {
            return View();
        }
        public ActionResult WorkersLateStatus()
        {
            return View();
        }
        #endregion -- Pages

        [HttpGet, Authorize]
        public ActionResult getShift()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT [CheckBoxSelect] = Convert(bit, 'False'),* FROM ShiftDefination AS sd WHERE sd.PlantID='" + identity.PlantId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        #region ShiftReport

        [HttpGet, Authorize]
        public ActionResult GetShiftReport(ReportFormat reportFormat, string employeeId, string fromDate, string toDate,String EmpDoj)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _AttendanceManagementService.GetShiftReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, employeeId, fromDate, toDate, EmpDoj);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "Shift Report";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        #endregion

        #region GetJobCardReport ----individual job card


        [HttpGet]
        public ActionResult GetJobCardReport(ReportFormat reportFormat, string[] employeeId, string fromDate, string toDate, bool chkAdditionInfo)
        {
            try
            {
                string EmpIdLoop = "";
                foreach (string item in employeeId)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "" + item + ""; ;
                    }

                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetJobCardReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, EmpIdLoop, fromDate, toDate, chkAdditionInfo); ;
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName,false);
                    case ReportFormat.PdfView:
                        return RenderReportAsPdf(workbook, reportFileName);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }



        #endregion

        #region Get Compliance Job Card Report  -----real job card
        [HttpGet]
        public ActionResult GetComplianceJobCardReport(ReportFormat reportFormat, string[] employeeId, string fromDate, string toDate, bool chkAdditionInfo)
        {
            try
            {
                string EmpIdLoop = "";
                foreach (string item in employeeId)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "" + item + ""; ;
                    }
                    
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetComplianceJobCardReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, EmpIdLoop, fromDate, toDate, chkAdditionInfo);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:                      
                        PdfDocument document = new PdfDocument();
                        ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
                        settings.TemplateDocument = document;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document = converter1.Convert(settings);
                        }                      
                        document.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
                        return null;

                    case ReportFormat.PdfView:
                        PdfDocument document1 = new PdfDocument();
                        ExcelToPdfConverterSettings settings1 = new ExcelToPdfConverterSettings();
                        settings1.TemplateDocument = document1;
                        for (int i = 0; i < workbook.Worksheets.Count; i++)
                        {
                            ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                            document1 = converter1.Convert(settings1);
                        }
                        document1.Save(reportFileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                        //return RenderReportAsPdf(document1, reportFileName);
                        return RenderReportAsPdf(workbook, reportFileName);
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }





        [HttpGet,Authorize]
        public ActionResult GetComplianceJobCardReportView(string EmpIdLoop, string fromDate, string toDate, bool chkAdditionInfo)
        {
            try
            {
                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetComplianceJobCardReport(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, EmpIdLoop, fromDate, toDate, chkAdditionInfo);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                return RenderReportAsPdf(workbook, reportFileName);
                
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }
        #endregion

        #region National Festival
        [HttpGet, Authorize]
        public ActionResult GetNationalFestivalReport(string CalanderYearId, ReportFormat reportFormat, string EmpSystemId, string fromDate, string toDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetNationalFestivalReport(CalanderYearId,identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, EmpSystemId, fromDate, toDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "National Festival";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        #endregion

        #region Audit Report
        [HttpGet, Authorize]
        public ActionResult GetManualOutTimeDateWiseReport(ReportFormat reportFormat, string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetManualOutTimeDateWiseReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId,identity.PlantName,FromDate,ToDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Attendance Audit Data";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcelx(workbook, reportFileName);


                    default:
                        return RenderReportAsExcelx(workbook, reportFileName);

                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }



        #endregion
        
        #region PreAllocated Report 

        [HttpGet, Authorize]
        public ActionResult GetPreAllocatedReport(ReportFormat reportFormat, string WorkDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetPreAllocatedReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, WorkDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Preallocated OT Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }




        #endregion

        #region Attendance Raw Data Report 

        [HttpGet]
        public ActionResult GetAttendanceRawDataReport(ReportFormat reportFormat, string WorkDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetAttendanceRawDataReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, WorkDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Attendance Raw Data Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }




        #endregion  Attendance Raw Data Report 

        #region Actual OT And Plan Report 

        [HttpGet, Authorize]
        public ActionResult GetActualOTAndPlanReport(ReportFormat reportFormat, string WorkDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetActualOTAndPlanReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, WorkDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Actual OT And Plan";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }




        #endregion  Actual OT And Plan Report 

        #region TIFFINE BILL LATEST Report 

        [HttpGet, Authorize]
        public ActionResult GetTiffinBillFinalReport(ReportFormat reportFormat, string WorkDate,string DailyAllowance,string ReportName)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetTiffinBillFinalReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, WorkDate, DailyAllowance, ReportName);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Tiffin Bill Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }




        #endregion

        #region TIFFINE BILL Summary Report 

        [HttpGet, Authorize]
        public ActionResult GetTiffinBillFinalSummaryReport(ReportFormat reportFormat,string FromDate,string ToDate,string DailyAllowance,string ReportName)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetTiffinBillFinalSummaryReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, FromDate, ToDate, DailyAllowance, ReportName);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Tiffin Bill Summary";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }




        #endregion

        #region Get Leaves CheckList Report
        [HttpGet, Authorize]
        public ActionResult GetleavesChecklistReport(ReportFormat reportFormat, string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetleavesChecklistReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, FromDate, ToDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "leaves Checklist Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);
                    case ReportFormat.PdfView:
                        return RenderReportAsPdf(workbook, reportFileName);
                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        #endregion  Get Leaves CheckList Report

        #region OT Final Report

        [HttpGet, Authorize]
        public JsonResult GetClanderYear()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var data = _employeeProfileService.GetClanderYear(identity.PlantId);
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetOtFinalReport(ReportFormat reportFormat, string year, string month)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = _AttendanceManagementService.GetOtFinalReport2(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.EmployeeId, identity.PlantName, year, month);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "Ot Final";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetEmployeeInformation(string fromDate, string toDate, string criteria)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            JsonResult json = Json(_AttendanceManagementService.GetEmpInfo(identity.CompanyGroupId, identity.PlantId, fromDate, toDate, criteria), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;

            //return Json(_AttendanceManagementService.GetEmpInfo(identity.CompanyGroupId, identity.PlantId, fromDate, toDate, criteria), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetMaternityLeaveInformation(string criteria)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_AttendanceManagementService.GetMaternityEmpInfo(identity.CompanyGroupId, identity.PlantId, criteria), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetFinalSattlementInformation(string criteria)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_AttendanceManagementService.GetFinalSattlementInformation(identity.CompanyGroupId, identity.PlantId, criteria), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetSalaryCertificateInformation(string criteria,string fiscalYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_AttendanceManagementService.GetSalaryCertificateInformation(identity.CompanyGroupId, identity.PlantId, criteria, fiscalYearId), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Final Settlement
        [HttpGet]
        public ActionResult EmployeeSattlementReport(string SystemId, string LanguageId, string UserName)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _AttendanceManagementService.EmployeeSattlementReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, SystemId, LanguageId, UserName);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return View();
        }

//        public void EmployeeSattlementReport(string companyGroupId, string companyId, string plantId, string SystemId, string LanguageId, string UserName)
//        {
//            try
//            {
//                CreateFinalSettlement(companyGroupId, companyId, plantId, SystemId, LanguageId, UserName);
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//        }
//        private void CreateFinalSettlement(string companyGroupId, string companyId, string plantId, string SystemId, string LanguageId, string UserName)
//        {
//            try
//            {
//                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//                ReportUtility oRU = new ReportUtility();
//                string strPath = "";
//                string filepath = "";
//                string File = "Fs" + plantId + UserName + ".docx";
//                strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/File);
//                if (System.IO.File.Exists(strPath) && UserName != "English")
//                {
//                    filepath = strPath;
//                }
//                else
//                {
//                    File = "Fs" + plantId + "English.docx";
//                    filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
//                }

//                FileInfo DocFile = new FileInfo(strPath);
//                if (DocFile.Exists == false)
//                {
//                    throw new Exception("File Not Found");
//                }
//                WordDocument document = new WordDocument(DocFile.FullName);

//                DataTable dtEmpInformation = GetEmpInformationforfinalsettlement(plantId, SystemId, LanguageId, UserName);
//                DataTable dtFinalSettlementData = GetFinalSettlementData(plantId, SystemId, LanguageId, UserName);




//                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
//                Dictionary<string, int> replaced = new Dictionary<string, int>();

//                string value = "";
//                foreach (TextSelection item in allresult)
//                {
//                    string foundText = item.SelectedText;

//                    if (replaced.ContainsKey(foundText) == false)
//                        replaced.Add(foundText, 0);

//                    //for fixed info
//                    string colName = foundText.Trim().Replace("{", "").Replace("}", "");

//                    if (dtEmpInformation.Columns.Contains(colName))
//                    {

//                        ///=====
//                        value = dtEmpInformation.Rows[0][dtEmpInformation.Columns[colName].ColumnName].ToString();

//                        if (bplib.clsWebLib.IsNumeric(value))
//                            replaced[foundText] = document.Replace(foundText, cnDgt(value, UserName), false, true);
//                        else if (bplib.clsWebLib.IsDateOK(value))
//                            replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, UserName), false, true);
//                        else
//                            replaced[foundText] = document.Replace(foundText, value, false, true);
//                    }
//                    if (dtFinalSettlementData.Columns.Contains(colName))
//                    {

//                        ///=====
//                        value = dtFinalSettlementData.Rows[0][dtFinalSettlementData.Columns[colName].ColumnName].ToString();

//                        if (bplib.clsWebLib.IsNumeric(value))
//                            replaced[foundText] = document.Replace(foundText, cnDgt(value, UserName), false, true);
//                        else if (bplib.clsWebLib.IsDateOK(value))
//                            replaced[foundText] = document.Replace(foundText, GetFormatedDate(value, UserName), false, true);
//                        else
//                            replaced[foundText] = document.Replace(foundText, value, false, true);
//                    }
//                }
//                WSection section = document.Sections[0];
//                WTable table1 = (WTable)section.Body.Tables[1];
//                for (int ROW = 0; ROW < dtFinalSettlementData.Rows.Count; ROW++)
//                {
//                    int isReplaced = 0;

//                    #region Retirement

//                    isReplaced = table1.Replace("{" + dtFinalSettlementData.Rows[ROW]["RetirementDayT"].ToString() + "}", cnDgt(dtFinalSettlementData.Rows[ROW]["PolicyDayNo"].ToString(), UserName), false, true);
//                    isReplaced = table1.Replace("{" + dtFinalSettlementData.Rows[ROW]["RetirementRateT"].ToString() + "}", cnDgt(dtFinalSettlementData.Rows[ROW]["SalaryRate"].ToString(), UserName), false, true);
//                    isReplaced = table1.Replace("{" + dtFinalSettlementData.Rows[ROW]["RetirementAmountT"].ToString() + "}", cnDgt(dtFinalSettlementData.Rows[ROW]["SeparationTypeAmount"].ToString(), UserName), false, true);

//                    #endregion Retirement

//                    #region Resignation

//                    isReplaced = table1.Replace("{" + dtFinalSettlementData.Rows[ROW]["ResignationDayT"].ToString() + "}", cnDgt(dtFinalSettlementData.Rows[ROW]["PolicyDayNo"].ToString(), UserName), false, true);
//                    isReplaced = table1.Replace("{" + dtFinalSettlementData.Rows[ROW]["ResignationRateT"].ToString() + "}", cnDgt(dtFinalSettlementData.Rows[ROW]["SalaryRate"].ToString(), UserName), false, true);
//                    isReplaced = table1.Replace("{" + dtFinalSettlementData.Rows[ROW]["ResignationAmountT"].ToString() + "}", cnDgt(dtFinalSettlementData.Rows[ROW]["SeparationTypeAmount"].ToString(), UserName), false, true);

//                    #endregion Resignation



//                    #region Total In Word
//                    //strnumberToString(string numberToConvert)
//                    numberToString.numberToStringBuilder bangla = new numberToString.numberToStringBuilder();
//                    document.Replace("{TotalInWords}", bangla.strnumberToString(dtFinalSettlementData.Rows[ROW]["NetPayAmount"].ToString()), true, true);

//                    // document.Replace("{TotalInWords}", ru.InWord((materialTotal + serviceTotal), dtOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);

//                    #endregion Total In Word
//                }

//                //deduction part
//                //if (dtFinalSettlementDedutionData.Rows.Count > 0)
//                //{
//                //    for (int ROW = 0; ROW < dtFinalSettlementDedutionData.Rows.Count; ROW++)
//                //    {
//                //        table1.Replace("{" + dtFinalSettlementDedutionData.Rows[ROW]["DeductionAmount"].ToString() + "}", cnDgt(dtFinalSettlementDedutionData.Rows[ROW]["Amount"].ToString(), UserName), false, true);
//                //    }
//                //}
//                DataTable dtFinalSettlementDedutionData = null;
//                DataTable dtFinalSettlementEarningData = null;
//                if (dtFinalSettlementData.Rows.Count > 0)
//                {
//                    dtFinalSettlementDedutionData = GetFinalSettlementDeductionData(dtFinalSettlementData.Rows[0]["Id"].ToString());
//                    dtFinalSettlementEarningData = GetFinalSettlementEarningData(dtFinalSettlementData.Rows[0]["Id"].ToString());
//                }

//                GetFinalSettlementHeadWiseData("{EarningPart}", document, dtFinalSettlementDedutionData, LanguageId);
//                GetFinalSettlementHeadWiseData("{DedutionPart}", document, dtFinalSettlementEarningData, LanguageId);

//                foreach (string item in replaced.Keys)
//                {
//                    if (replaced[item] == 0)
//                        document.Replace(item, "", false, true);

//                }

//                string fileNames = string.Empty;
//                if (dtEmpInformation.Rows.Count > 0)
//                {
//                    fileNames = dtEmpInformation.Rows[0]["EmployeeCode"] + "-FinalSettlement.docx";
//                }
//                else
//                {
//                    fileNames = "FinalSettlement.docx";
//                }

//                document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
//                document.Close();
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }

//        }
//        public DataTable GetFinalSettlementDeductionData(string EmployeeFinalSettlementId)
//        {
//            try
//            {

//                string sqlx = @" select dh.ShortName+'Amount' AS DeductionAmount,fs.Amount from FinalSettlementDeductionDetails fs
//                                left join [dbo].[FinalSettlementDeductionHead] dh on dh.id=fs.FinalSettlementDeductionHeadId
//                                where fs.EmployeeFinalSettlementId='" + EmployeeFinalSettlementId + @"'";
//                string sql = @" SELECT ISNULL(ll.Name, dh.UserName) AS FinalSettlementHead ,fs.Amount,dh.Category 
//                                FROM FinalSettlementDeductionDetails fs
//                                left join [dbo].[FinalSettlementDeductionHead] dh on dh.id=fs.FinalSettlementDeductionHeadId
//                                left join [HKP].[LocalLanguage] ll on ll.FinalSettlementHeadId=fs.Id

//                                where fs.EmployeeFinalSettlementId='" + EmployeeFinalSettlementId + @"' and  and dh.Category='Deduction'";

//                return _sqlRepository.GetDataTable(sql);
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }
//        public DataTable GetFinalSettlementEarningData(string EmployeeFinalSettlementId)
//        {
//            try
//            {


//                string sql = @" SELECT ISNULL(ll.Name, dh.UserName) AS FinalSettlementHead ,fs.Amount,dh.Category 
//                                FROM FinalSettlementDeductionDetails fs
//                                left join [dbo].[FinalSettlementDeductionHead] dh on dh.id=fs.FinalSettlementDeductionHeadId
//                                left join [HKP].[LocalLanguage] ll on ll.FinalSettlementHeadId=fs.Id

//                                where fs.EmployeeFinalSettlementId='" + EmployeeFinalSettlementId + @"' and  and dh.Category='Earning'";

//                return _sqlRepository.GetDataTable(sql);
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }
//        public void GetFinalSettlementHeadWiseData(string replaceString, WordDocument document, DataTable dsFinalSettlementHeadWiseData, string lng)
//        {
//            //string replaceString = "{employeeTable}";





//            int LasColumnIndex = 6;
//            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();





//            WTable wTable = new WTable(document);
//            wTable.TableFormat.Borders.LineWidth = 1;
//            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
//            wTable.TableFormat.IsAutoResized = true;

//            int ROW = 0; int COL = 0;
//            wTable.ResetCells(1, LasColumnIndex + 1);

//            WTableRow TemplateRow = wTable.Rows[0].Clone();
//            #region column headers
//            document.EnsureMinimal();


//            WCharacterFormat FontBold = new WCharacterFormat(document);
//            FontBold.Bold = true;

//            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("S.NO.");
//            range.ApplyCharacterFormat(FontBold);
//            int colSlNo = COL; COL++;
//            wTable.Rows[ROW].Cells[colSlNo].Width = 40;

//            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Appraisal Date");
//            range.ApplyCharacterFormat(FontBold);
//            int colAppraisalDate = COL; COL++;
//            wTable.Rows[ROW].Cells[colAppraisalDate].Width = 80;


//            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Previous Gross ");
//            //range.ApplyCharacterFormat(FontBold);
//            //int colPreviousGross = COL; COL++;
//            //wTable.Rows[ROW].Cells[colPreviousGross].Width = 80;


//            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Previous Designation");
//            //range.ApplyCharacterFormat(FontBold);
//            //int colPreviousDesignation = COL; COL++;
//            //wTable.Rows[ROW].Cells[colPreviousDesignation].Width = 40;


//            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("New Gross");
//            //range.ApplyCharacterFormat(FontBold);
//            //int colNewGross = COL; COL++;
//            //wTable.Rows[ROW].Cells[colNewGross].Width = 40;

//            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("New Designation");
//            //range.ApplyCharacterFormat(FontBold);
//            //int colNewDesignation = COL; COL++;
//            //wTable.Rows[ROW].Cells[colNewDesignation].Width = 40;


//            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Increment Amount");
//            //range.ApplyCharacterFormat(FontBold);
//            //int colIncrementAmount = COL; COL++;
//            //wTable.Rows[ROW].Cells[colIncrementAmount].Width = 60;






//            #endregion column headers
//            double totalValue = 0;
//            int startRow = ROW + 1;
//            int slno = 0;
//            for (int i = 0; i < dsFinalSettlementHeadWiseData.Rows.Count; i++)
//            {
//                //slno++;
//                ROW++;
//                wTable.AddRow();
//                WTableRow TROW = wTable.LastRow;

//                // WTableRow TROW = wTable.Rows[1].Clone();
//                for (int CE = 0; CE < TROW.Cells.Count; CE++)
//                {
//                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
//                    {
//                        item.Text = "";
//                    }
//                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
//                }
//                //TROW.Cells[colSLId].AddParagraph().AppendText(sl.ToString());
//                //TROW.Cells[colSLId].Width = 30;
//                TROW.Cells[colSlNo].AddParagraph().AppendText(dsFinalSettlementHeadWiseData.Rows[i]["FinalSettlementHead"].ToString());
//                TROW.Cells[colAppraisalDate].AddParagraph().AppendText(cnDgt(dsFinalSettlementHeadWiseData.Rows[i]["FinalSettlementHead"].ToString(), lng));
//                //TROW.Cells[colPreviousGross].AddParagraph().AppendText(clsStdLib.dbl(dsFinalSettlementHeadWiseData.Rows[i]["PreviousGross"].ToString()).ToString("#,##0.00"));


//            }
//            //WSection section = document.Sections[0];

//        }

//        public DataTable GetEmpInformationforfinalsettlement(string plantId, string SystemId, string LanguageId, string UserName)
//        {
//            try
//            {

//                string sql = @"select isnull(e.EmployeeNameLocal,e.EmployeeName) as EmployeeName,ISNULL(ll.Name, LDN.UserName)AS Designation , ISNULL(e.EmployeeCode,e.EmployeeCode) EmployeeCode,
//ISNULL(lls.Name,Se.UserName)AS Section
//,FORMAT(e.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(e.DOS,'dd-MMM-yyyy')DOS,FORMAT(e.DOS,'MMMM-yyyy') DOSMonth
//                                from EmployeeInformation e
//	LEFT JOIN  hkp.LegalDesignation LDN ON LDN.Id=E.LegalDesignationId
//								LEFT JOIN MST.ManpowerBudget PMB ON e.BudgetCode = PMB.Id
//                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
//                        LEFT JOIN ORG.Entity ET ON PMB.EntityId = ET.Id
//						LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
//                                left join [HKP].[LocalLanguage] ll on ll.LegalDesignationId=e.LegalDesignationId
//                                left join [HKP].[LocalLanguage] lls on lls.SectionId=e.SectionId
//                                left join [ORG].[Plant] p on p.Id=e.PlantId
//                                where e.SystemId='" + SystemId + @"'and p.Id='" + plantId + @"' ";
//                return _sqlRepository.GetDataTable(sql);
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }

//        public DataTable GetFinalSettlementData(string plantId, string SystemId, string LanguageId, string UserName)
//        {
//            try
//            {

//                string sql = @"Select efs.Id, FORMAT(efs.FinalSettlementDate,'dd-MMM-yyy') FinalSettlementDate
//							,efs.SalaryRate
//							,efs.OTRate
//                            ,convert(int,ROUND(efs.[TotalDeductionAmount],0)) TotalDeductionAmount
//                            ,convert(int,ROUND(efs.LvEncashmentAmount,0)) LvEncashmentAmount
//							,convert(int,ROUND(efs.EarningAmount,0)) EarningAmount
//							----,convert(int,ROUND(efs.DeductionAmount,0)) DeductionAmount
//							,convert(int,ROUND(efs.GratuityAmount,0)) GratuityAmount
//							,efs.[LastMonthAbsentDay]
//                            ,efs.OTRate OTRateA
//							,convert(int,ROUND(efs.[TotalPayableAmount],0)) TotalPayableAmount
//							,convert(int,ROUND(efs.[NetPayAmount],0)) NetPayAmount
//							,efs.[LastMonthOTHour]
//							,efs.[LastMonthOTAmount]
//                            ---,efs.[StampAmount]
//							,efs.[LastMonthAbsenteeismAmount]
//							,efs.[LvEncashmentDayNo]
//							,convert(int,ROUND(efs.[LastMonthProcDay],0)) LastMonthProcDay
//							,convert(int,ROUND(efs.[LastMonthGrossAmount],0)) LastMonthGrossAmount

//							,SY.UserName+'Day' AS RetirementDayT
//							,SY.UserName+'Rate' AS RetirementRateT
//							,SY.UserName+'Amount' AS RetirementAmountT

//                            ,SY.UserName+'Day' AS ResignationDayT
//							,SY.UserName+'Rate' AS ResignationRateT
//							,SY.UserName+'Amount' AS ResignationAmountT

//							---,efs.PolicyDayNo
//                            ,CONVERT(INT, ISNULL(efs.PolicyYearNo,0)*ISNULL(efs.PolicyDayNo,0)) PolicyDayNo
//                            ,SY.UserName AS SeprationName
//                            ,convert(int,ROUND(efs.TenureDayNo,0)) TenureDayNo
//							,convert(int,ROUND(efs.SeparationTypeAmount,0)) SeparationTypeAmount
//							,convert(int,ROUND(efs.GrossAmount,0)) GrossAmount
//							,convert(int,ROUND(efs.BasicAmount,0)) BasicAmount
//							,convert(int,efs.[TenureYearNo]) TenureYearNo
//							,convert(int,efs.[TenureMonthNo]) TenureMonthNo
//							,convert(int,efs.TenureDayNo) TenureDayNoA
//							,convert(int,ROUND(efs.LastMonthNetPayAmount,0)) LastMonthNetPayAmount
//							,efs.LvEncashmentRateAmount

//                            From [dbo].[EmployeeFinalSettlement] efs 
//	                        LEFT JOIN [HKP].[SeparationType] SY ON SY.Id=efs.SeparationTypeId
//                            LEFT JOIN EmployeeInformation E ON E.SystemId=efs.EmpSystemID
//                            LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
//                            where EmpSystemId='" + SystemId + @"'and ep.Id='" + plantId + @"'";

//                string xsql = @"select FORMAT(efs.FinalSettlementDate,'dd-MMM-yyy') FinalSettlementDate,efs.SalaryRate,efs.OTRate,efs.[TotalDeductionAmount]
//                            ,efs.LvEncashmentAmount,efs.OthersAmount,efs.DeductionAmount,efs.GratuityAmount,efs.[LastMonthAbsentDay]
//                            ,efs.OTRate OTRateA,efs.[TotalPayableAmount],efs.[NetPayAmount],efs.[LastMonthOTHour],efs.[LastMonthOTAmount]
//                            ,efs.[StampAmount],efs.[LastMonthAbsenteeismAmount],efs.[LvEncashmentDayNo],efs.[LastMonthProcDay],efs.[LastMonthGrossAmount]

//							,SY.UserName+'Day' AS RetirementDayT
//							,SY.UserName+'Rate' AS RetirementRateT
//							,SY.UserName+'Amount' AS RetirementAmountT

//                            ,SY.UserName+'Day' AS ResignationDayT
//							,SY.UserName+'Rate' AS ResignationRateT
//							,SY.UserName+'Amount' AS ResignationAmountT

//							---,efs.PolicyDayNo
//                            ,CONVERT(INT, ISNULL(efs.PolicyYearNo,0)*ISNULL(efs.PolicyDayNo,0)) PolicyDayNo
//                            ,SY.UserName AS SeprationName
//                            ,efs.TenureDayNo,efs.SeparationTypeAmount,efs.GrossAmount,efs.BasicAmount
//							,convert(int,efs.[TenureYearNo]) TenureYearNo
//							,convert(int,efs.[TenureMonthNo]) TenureMonthNo
//							,convert(int,efs.TenureDayNo) TenureDayNoA

//		,efs.LastMonthNetPayAmount,efs.LvEncashmentRateAmount

//                            From [dbo].[EmployeeFinalSettlement] efs 
//	                        LEFT JOIN [HKP].[SeparationType] SY ON SY.Id=efs.SeparationTypeId
//                            LEFT JOIN EmployeeInformation E ON E.SystemId=efs.EmpSystemID
//                            LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
//                            where EmpSystemId='" + SystemId + @"'and ep.Id='" + plantId + @"'";
//                return _sqlRepository.GetDataTable(sql);
//            }
//            catch (Exception)
//            {
//                throw;
//            }
//        }
//        public string cnDgt(string input, string lng)
//        {
//            if (lng == "Bengali")
//            {
//                return input.Replace('0', '০')
//                    .Replace('1', '১')
//                    .Replace('2', '২')
//                    .Replace('3', '৩')
//                    .Replace('4', '৪')
//                    .Replace('5', '৫')
//                    .Replace('6', '৬')
//                    .Replace('7', '৭')
//                    .Replace('8', '৮')
//                    .Replace('9', '৯');
//            }
//            else if (lng == "Hindi")
//            {
//                return input.Replace('0', '०')
//                    .Replace('1', '१')
//                    .Replace('2', '२')
//                    .Replace('3', '३')
//                    .Replace('4', '४')
//                    .Replace('5', '५')
//                    .Replace('6', '६')
//                    .Replace('7', '७')
//                    .Replace('8', '८')
//                    .Replace('9', '९');
//            }
//            else if (lng == "English")
//            {
//                return input.Replace('0', '0')
//                    .Replace('1', '1')
//                    .Replace('2', '2')
//                    .Replace('3', '3')
//                    .Replace('4', '4')
//                    .Replace('5', '5')
//                    .Replace('6', '6')
//                    .Replace('7', '7')
//                    .Replace('8', '8')
//                    .Replace('9', '9');
//            }
//            return input;
//        }

//        public string GetFormatedDate(string date, string lng)
//        {
//            var formateDate = string.Empty;
//            var day = cnDgt(date.Substring(0, 2), lng);
//            var mon = ChangeMonth(date.Substring(3, 3), lng);
//            var year = cnDgt(date.Substring(7, 4), lng);
//            return formateDate = day + "-" + mon + "-" + year;
//        }
//        public string ChangeMonth(string input, string lng)
//        {
//            if (lng == "Bengali")
//            {
//                return input
//                    .Replace("Jan", "জানুয়ারি")
//                    .Replace("Feb", "ফেব্রুয়ারি")
//                    .Replace("Mar", "মার্চ")
//                    .Replace("Apr", "এপ্রিল")
//                    .Replace("May", "মে")
//                    .Replace("Jun", "জুন")
//                    .Replace("Jul", "জুলাই")
//                    .Replace("Aug", "আগস্ট")
//                    .Replace("Sep", "সেপ্টেম্বর")
//                    .Replace("Oct", "অক্টোবর")
//                    .Replace("Nov", "নভেম্বর")
//                    .Replace("Dec", "ডিসেম্বর");
//            }
//            else if (lng == "Hindi")
//            {
//                return input
//                    .Replace("Jan", "जनवरी")
//                    .Replace("Feb", "फरवरी")
//                    .Replace("Mar", "मार्च")
//                    .Replace("Apr", "अप्रैल")
//                    .Replace("May", "मई")
//                    .Replace("Jun", "जून")
//                    .Replace("Jul", "जुलाई")
//                    .Replace("Aug", "अगस्त")
//                    .Replace("Sep", "सितम्बर")
//                    .Replace("Oct", "अक्तूबर")
//                    .Replace("Nov", "नवम्बर")
//                    .Replace("Dec", "दिसम्बर");
//            }
//            return input;
//        }
        #endregion

        #region tiffin bill Report



        [HttpGet, Authorize]
        public ActionResult GetTifineReport( string fromDate, string toDate, string ShiftId, string Hr, string Min)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = _AttendanceManagementService.GetTiffinBillReport( identity.PlantId,   fromDate,  toDate,  ShiftId,  Hr,  Min);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "Tiffin Bill";
            //switch (reportFormat)
            //{
            //    case ReportFormat.Pdf:
            //        return RenderReportAsPdf(workbook, reportFileName);

            //    case ReportFormat.Excel:
            //        return RenderReportAsExcel(workbook, reportFileName);

            //    default:
            //        return RenderReportAsExcel(workbook, reportFileName);
            //}
            return RenderReportAsExcel(workbook, reportFileName);

        }


        #endregion

        #region Late Attendance Posting

        [HttpGet]
        public ActionResult GetLateAttendancePostingReport(ReportFormat reportFormat, string EffectiveDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetLateAttendancePostingReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, EffectiveDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Late Attendance Posting Report";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }




        #endregion

        #region Get Attendance Summary Status Report
        [HttpGet,Authorize]
        public ActionResult GetAttendanceSummaryStatusReport(ReportFormat reportFormat, string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetAttendanceSummaryStatusReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, FromDate, ToDate);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Previous Day Absent";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        #endregion  Get Attendance Summary Status Report

        #region Worker Late Status
        [HttpGet]
        public ActionResult GetWorkerLateStatusReport(ReportFormat reportFormat, string WorkDate,string EntityId,string EntityUserName)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetWorkerLateStatusReport(identity.Name, identity.PlantId, identity.CompanyId, identity.CompanyGroupId, identity.PlantName, WorkDate, EntityId, EntityUserName);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Workers Late Status";
                switch (reportFormat)
                {
                    case ReportFormat.Pdf:
                        return RenderReportAsPdf(workbook, reportFileName, false);

                    case ReportFormat.Excel:
                        return RenderReportAsExcel(workbook, reportFileName);

                    default:
                        return RenderReportAsExcel(workbook, reportFileName);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        # endregion WORKER Late Status

        [HttpGet, Authorize]
        public JsonResult GetDailyAllowanceCbo()
        {
            var sql = @"select  UserName,Id  from [HKP].[AllowanceDaily] where Active=1";
            return Json(_sqlRepository.GetCombo(sql, "Id", "UserName"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult EntityCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,UserName from [ORG].[Entity] where PlantId='"+identity.PlantId+@"' and CompanyId='"+identity.CompanyId+@"' and CompanyGroupId='"+identity.CompanyGroupId+@"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);

        }

    }
}