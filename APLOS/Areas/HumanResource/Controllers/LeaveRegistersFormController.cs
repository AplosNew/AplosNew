
#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Linq;
using Syncfusion.XlsIO;
using Syncfusion.Pdf;
using Library.Service.Helpers;
using System.IO;
using Library.Data;
using Library.HumanResource.Payroll;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Reflection;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.DocIO.DLS;
using System.Text.RegularExpressions;
using Syncfusion.DocToPDFConverter;
using Syncfusion.DocIO;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class LeaveRegistersFormController : BaseController
    {


        #region Constructor
        private readonly ISqlRepository _sqlRepository;


        public LeaveRegistersFormController(ISqlRepository R)
        {


            _sqlRepository = R;
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
        public ActionResult GetSettings()
        {
            try
            {
                Library.HumanResource.Payroll.PayrollReportsService service = new Library.HumanResource.Payroll.PayrollReportsService();
                service.GetSettingsForForm18(out List<Dictionary<string, object>> salaryHeads, out List<Dictionary<string, object>> LeaveTypes);

                return Json(new { SalaryHeadList = salaryHeads, LeaveTypeList = LeaveTypes, Message = "Data updated successfully", Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true, }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public ActionResult SaveSettings(List<Dictionary<string, object>> salaryHeads, List<Dictionary<string, object>> LeaveTypes)
        {
            try
            {
                Library.HumanResource.Payroll.PayrollReportsService service = new Library.HumanResource.Payroll.PayrollReportsService();
                service.SaveSettingsForForm18(salaryHeads, LeaveTypes);

                return Json(new { Message = "Data updated successfully", Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true, }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet, Authorize]
        public ActionResult FormLeaveRegister(string year, string empId, string reportType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                CreateLeaveRegisterFormInWord(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, year, empId, reportType);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        private void CreateLeaveRegisterFormInWord(string companyGroupId, string companyId, string plantId, string year, string empId, string reportType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string tempId = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";
                string reportTypeName = "";
                PayrollReportsService payrollReportsService = new PayrollReportsService();

                var lang = payrollReportsService.GetLanguage();

                fileName = "LeaveRegisterForm" + plantId + ".docx";
                strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                File = fileName;
                if (!System.IO.File.Exists(strPath))
                {
                    throw new CustomException("File <" + fileName + "> Not Found.");
                }

                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                DataTable dtEmp = payrollReportsService.GetEmployeeBasicInfoById(empId, plantId, lang["Id"].ToString(), tempId);
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";
                foreach (TextSelection item in allresult)
                {
                    string foundText = item.SelectedText;

                    if (replaced.ContainsKey(foundText) == false)
                        replaced.Add(foundText, 0);

                    //for fixed info
                    string colName = foundText.Trim().Replace("{", "").Replace("}", "");
                    if (dtEmp.Columns.Contains(colName))
                    {

                        colName = payrollReportsService.GetBasicInfoInDefaultLng(colName);
                        ///=====
                        value = dtEmp.Rows[0][dtEmp.Columns[colName].ColumnName].ToString();

                        if (bplib.clsWebLib.IsNumeric(value))
                            replaced[foundText] = document.Replace(foundText, payrollReportsService.cnDgt(value, tempId), false, true);
                        else if (bplib.clsWebLib.IsDateOK(value))
                            replaced[foundText] = document.Replace(foundText, payrollReportsService.GetFormatedDate(value, tempId), false, true);
                        else
                            replaced[foundText] = document.Replace(foundText, value, false, true);
                    }

                }

                WSection section = document.Sections[0];
                #region LeaveInformation

                WTable table1 = (WTable)section.Body.Tables[0];
                WTableRow copiedRow3 = table1.Rows[2].Clone();
                WTableRow row3;

                DataTable dtloadLeaveTransactions = payrollReportsService.LeaveSummaryForServiceBookQuery(empId, identity.PlantId, year);
                DataTable dtLoadLeave = payrollReportsService.loadBf(empId, year);

                for (int ROW = 0; ROW < dtloadLeaveTransactions.Rows.Count; ROW++)
                {

                    if (ROW > 0)
                    {
                        row3 = copiedRow3.Clone();
                        table1.Rows.Add(row3);
                    }

                    if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["EarnLeave"].ToString()))
                    {
                        table1.Replace("{EarnLeave}", payrollReportsService.cnDgt(dtloadLeaveTransactions.Rows[ROW]["EarnLeave"].ToString(), tempId), false, true);
                    }

                    if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["CasualLeave"].ToString()))
                    {
                        table1.Replace("{CasualLeave}", payrollReportsService.cnDgt(dtloadLeaveTransactions.Rows[ROW]["CasualLeave"].ToString(), tempId), false, true);
                    }

                    if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["SickLeave"].ToString()))
                    {
                        table1.Replace("{SickLeave}", payrollReportsService.cnDgt(dtloadLeaveTransactions.Rows[ROW]["SickLeave"].ToString(), tempId), false, true);
                    }

                    if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["RejectionReason"].ToString()))
                    {
                        table1.Replace("{RejectionReason}", payrollReportsService.cnDgt(dtloadLeaveTransactions.Rows[ROW]["RejectionReason"].ToString(), tempId), false, true);
                    }

                    if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["ApprovedDate"].ToString()))
                    {
                        table1.Replace("{ApprovedDate}", payrollReportsService.GetFormatedDate(dtloadLeaveTransactions.Rows[ROW]["ApprovedDate"].ToString(), tempId), false, true);
                    }

                    if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["LeaveAmount"].ToString()))
                    {
                        table1.Replace("{LeaveAmount}", payrollReportsService.cnDgt(dtloadLeaveTransactions.Rows[ROW]["LeaveAmount"].ToString(), tempId), false, true);
                    }

                    if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["CumulitiveEarnLeave"].ToString()))
                    {
                        table1.Replace("{CumulitiveEarnLeave}", payrollReportsService.cnDgt(dtloadLeaveTransactions.Rows[ROW]["CumulitiveEarnLeave"].ToString(), tempId), false, true);
                    }

                    if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["CumulitiveCasualLeave"].ToString()))
                    {
                        table1.Replace("{CumulitiveCasualLeave}", payrollReportsService.cnDgt(dtloadLeaveTransactions.Rows[ROW]["CumulitiveCasualLeave"].ToString(), tempId), false, true);
                    }

                    if (!string.IsNullOrEmpty(dtloadLeaveTransactions.Rows[ROW]["CumulitiveSickLeave"].ToString()))
                    {
                        table1.Replace("{CumulitiveSickLeave}", payrollReportsService.cnDgt(dtloadLeaveTransactions.Rows[ROW]["CumulitiveSickLeave"].ToString(), tempId), false, true);
                    }
                }
                #endregion

                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }
           
                document.Protect(ProtectionType.AllowOnlyReading, "password");
                string filename = "Leave Register Form-" + empId + ".docx";
                document.Save(filename, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet, Authorize]
        public ActionResult XFormLeaveRegister(string year, string empId, string reportType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                Library.HumanResource.Payroll.PayrollReportsService service = new Library.HumanResource.Payroll.PayrollReportsService();

                var fileName = "LeaveRegisterForm" + identity.PlantId;
                var workbook = service.LeaveRegisterFormInMSWord(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, year, empId, reportType);

                workbook.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);

                return null;
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        [HttpGet, Authorize]
        public ActionResult GetFormLeaveRegister(string year, string empId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var fileName = "LeaveRegisterForm-" + empId + "" + DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = GetFormLeaveRegisterWorkBook(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empId);


            PdfDocument document = new PdfDocument();
            ExcelToPdfConverterSettings settings = new ExcelToPdfConverterSettings();
            settings.TemplateDocument = document;
            for (int i = 0; i < workbook.Worksheets.Count; i++)
            {
                ExcelToPdfConverter converter1 = new ExcelToPdfConverter(workbook.Worksheets[i]);
                document = converter1.Convert(settings);
            }
            document.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);
            return null;
        }

        public IWorkbook GetFormLeaveRegisterWorkBook(string companyGroupId, string companyId, string plantId, string empId)
        {
            try
            {
                var excelEngine = new ExcelEngine();
                var report = new ReportUtility();
                var workbook = report.GetWorkbook(ref excelEngine, 1);
                var sheet1 = workbook.Worksheets[0];
                workbook = CreateSheetMain_backup(ref sheet1, report, "Appointment Letter", "Appointment Letter", companyGroupId, companyId, plantId, empId);
                return workbook;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
               Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
               ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private IWorkbook CreateSheetMain_backup(ref IWorksheet sheet1, ReportUtility report, string sheetHeader, string sheetName, string companyGroupId, string companyId, string plantId, string empId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                var reportType = "";

                fileName = "LeaveRegisterForm" + plantId + ".xlsx";
                strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), fileName);
                File = fileName;
                if (!System.IO.File.Exists(strPath))
                {
                    throw new CustomException("File <" + fileName + "> Not Found.");
                }
                PayrollReportsService payrollReportsService = new PayrollReportsService();
                var lang = payrollReportsService.GetLanguage();
                DataTable dtEmp = payrollReportsService.GetEmployeeBasicInfoById(empId, plantId, null, null);//lang["Id"].ToString()

                ExcelEngine excelEngine = new ExcelEngine();
                IWorkbook workbook1 = null;


                if (System.IO.File.Exists(strPath))
                {
                    workbook1 = excelEngine.Excel.Workbooks.Open(strPath);

                    workbook1.Worksheets[0].Replace("{CompanyName}", dtEmp.Rows[0]["CompanyName"].ToString());
                    workbook1.Worksheets[0].Replace("{CompanyAddress}", dtEmp.Rows[0]["CompanyAddress"].ToString());
                    workbook1.Worksheets[0].Replace("{EmployeeName}", dtEmp.Rows[0]["EmployeeName"].ToString());
                    workbook1.Worksheets[0].Replace("{EmployeeCode}", dtEmp.Rows[0]["EmployeeCode"].ToString());
                    workbook1.Worksheets[0].Replace("{DesignationName}", dtEmp.Rows[0]["DesignationName"].ToString());
                    workbook1.Worksheets[0].Replace("{DOJ}", dtEmp.Rows[0]["DateOfJoin"].ToString());
                    workbook1.Worksheets[0].Replace("{TodaysDate}", DateTime.Now.ToString("dd-MMM-yyyy"));
                    workbook1.Worksheets[0].Replace("{Department}", dtEmp.Rows[0]["Department"].ToString());

                    workbook1.Version = ExcelVersion.Excel97to2003;
                }

                return workbook1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion -- Operations
    }

}