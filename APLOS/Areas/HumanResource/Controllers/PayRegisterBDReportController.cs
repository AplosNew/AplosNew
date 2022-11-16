using Aplos.Controllers;
using Aplos.Helpers;
using Aplos.Properties;
using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.HumanResources;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.HumanResources.PayRegisterBDReportService;
using static OTSBD.clsReport;

namespace Aplos.Areas.HumanResource.Controllers
{


    public class PayRegisterBDReportController : BaseController
    {

        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        //private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private string savepath;

        Library.HumanResource.Report.Payroll.clsPayRegister _payRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();


        public PayRegisterBDReportController(
               IEmployeeProfileService employeeProfileService
            , ISqlRepository sqlRepository
            )
        {
            //_payRegisterBDReportService = payRegisterBDReportService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = sqlRepository;
            _payRegisterBDReportService = new Library.HumanResource.Report.Payroll.clsPayRegister();
        }

        #endregion Constructor

        #region -- Pages

        //[Authorize]
        //public ActionResult Aplos()
        //{
        //    return View();
        //}
        //[Authorize]
        public ActionResult PayRegisterNew()
        {
            return View();
        }
        //[Authorize]
        public ActionResult PayRegisterCom()
        {
            return View();
        }
        public ActionResult PayRegisterContractor()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetSalaryprocessIdCbo(string month, string year, string IsCompletedMonth)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payRegisterBDReportService.GetSalaryprocessIdCbo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, month, year, IsCompletedMonth), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetLanguageIdCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeProfileService.GetDefaultCbo(identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPayGroupCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payRegisterBDReportService.GetPayGroupCbo(identity.IsControlAdmin, identity.IsSysAdmin, identity.UserId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetPayRegisterReportBangla(string month, string year, string salaryProcessId, string divisionId, string unitId, string sectionId, string subSectionId, string departmentId, string payGroupId, string employeeCategoryId, string paymentDate, string paymentMode, string languageId, string selPaymentMode, string selEmpCatg, string sqlInStatement, string withStructure, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
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



                var fileName = monthName + "-" + year + "PayRegister" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);

                var workbook = _payRegisterBDReportService.EmployeeSalaryRegister(PayRegisterParam, paymentDate, sqlInStatement, withStructure, isActive, isSeperated, isMaternity);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
                //return View();
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetPayRegisterReportNew(string month, string year, string salaryProcessId, string paymentDate, string printDate, string languageId, string withStructure, string groupBy, Dictionary<string, string> parameters, string sheetBasedOn, bool withAttendance, string paperSize, string reportType, string docGrouping, bool isActive, bool isSeperated, bool isMaternity, bool onlyEarning)
        {
            try
            {

                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.ToInt());
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                PayRegisterParamList PayRegisterParam = new PayRegisterParamList();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = "";
                string fromDate = 1 + "-" + monthName + "-" + year;
                string toDate = daysInMonth + "-" + monthName + "-" + year;
                var workbook = _payRegisterBDReportService.NewEmployeeSalaryRegisterWithStructure(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, year, month, languageId, paymentDate, printDate, fromDate, toDate, groupBy, parameters, salaryProcessId, sheetBasedOn, withAttendance, paperSize, docGrouping, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity, onlyEarning);

                if (reportType.ToUpper() == "EXCEL")
                {
                    string str = "c$er" + DateTime.Now.ToString("dd-MMM-yyyy") + identity.CompanyGroupId + identity.PlantId;
                    //workbook.Worksheets[0].Protect("c$er" + DateTime.Now.ToString("dd-MMM-yyyy") + identity.CompanyGroupId + identity.PlantId, ExcelSheetProtection.All);
                    workbook.Worksheets[0].PageSetup.PrintTitleRows = "$1:$6";
                    fileName = monthName + "-" + year + "PayRegister" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                    string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                    workbook.Version = ExcelVersion.Excel2013;
                    workbook.SaveAs(fullPath);

                }
                if (reportType.ToUpper() == "PDF")
                {
                    workbook.Version = ExcelVersion.Excel2016;
                    workbook.Worksheets[0].PageSetup.PrintTitleRows = "$1:$6";
                    var converter = new ExcelToPdfConverter(workbook);
                    var pdfDoc = converter.Convert();
                    fileName = monthName + "-" + year + "PayRegister" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".pdf";
                    string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                    pdfDoc.Save(fullPathPDF);
                }
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetPayRegisterReportCom(string month, string year, string salaryProcessId, string paymentDate, string printDate, string languageId, string withStructure, string groupBy, Dictionary<string, string> parameters, string sheetBasedOn, bool withAttendance, string paperSize, string reportType, string docGrouping, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {

                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.ToInt());
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                PayRegisterParamList PayRegisterParam = new PayRegisterParamList();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = "";
                string fromDate = 1 + "-" + monthName + "-" + year;
                string toDate = daysInMonth + "-" + monthName + "-" + year;
                var workbook = _payRegisterBDReportService.ComEmployeeSalaryRegisterWithStructure(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, year, month, languageId, paymentDate, printDate, fromDate, toDate, groupBy, parameters, salaryProcessId, sheetBasedOn, withAttendance, paperSize, docGrouping, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity);

                if (reportType.ToUpper() == "EXCEL")
                {
                    string str = "c$er" + DateTime.Now.ToString("dd-MMM-yyyy") + identity.CompanyGroupId + identity.PlantId;
                    //workbook.Worksheets[0].Protect("c$er" + DateTime.Now.ToString("dd-MMM-yyyy") + identity.CompanyGroupId + identity.PlantId, ExcelSheetProtection.All);
                    workbook.Worksheets[0].PageSetup.PrintTitleRows = "$1:$6";
                    fileName = monthName + "-" + year + "PayRegister" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                    string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                    workbook.Version = ExcelVersion.Excel2013;
                    workbook.SaveAs(fullPath);

                }
                if (reportType.ToUpper() == "PDF")
                {
                    workbook.Version = ExcelVersion.Excel2016;
                    workbook.Worksheets[0].PageSetup.PrintTitleRows = "$1:$6";
                    var converter = new ExcelToPdfConverter(workbook);
                    var pdfDoc = converter.Convert();
                    fileName = monthName + "-" + year + "PayRegister" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".pdf";
                    string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                    pdfDoc.Save(fullPathPDF);
                }
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
        }


        [HttpPost, Authorize]
        public ActionResult GetPayRegisterReportContractor(string month, string year, string salaryProcessId, string paymentDate, string printDate, string languageId, string withStructure, string groupBy, Dictionary<string, string> parameters, string sheetBasedOn, bool withAttendance, string paperSize, string reportType, string docGrouping, bool isActive, bool isSeperated, bool isMaternity, bool onlyEarning, string ContracotrId)
        {
            try
            {

                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.ToInt());
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                PayRegisterParamList PayRegisterParam = new PayRegisterParamList();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var fileName = "";
                string fromDate = 1 + "-" + monthName + "-" + year;
                string toDate = daysInMonth + "-" + monthName + "-" + year;
                var workbook = _payRegisterBDReportService.ContractorEmployeeSalaryRegisterWithStructure(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, year, month, languageId, paymentDate, printDate, fromDate, toDate, groupBy, parameters, salaryProcessId, sheetBasedOn, withAttendance, paperSize, docGrouping, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity, onlyEarning, ContracotrId);

                if (reportType.ToUpper() == "EXCEL")
                {
                    string str = "c$er" + DateTime.Now.ToString("dd-MMM-yyyy") + identity.CompanyGroupId + identity.PlantId;
                    //workbook.Worksheets[0].Protect("c$er" + DateTime.Now.ToString("dd-MMM-yyyy") + identity.CompanyGroupId + identity.PlantId, ExcelSheetProtection.All);
                    workbook.Worksheets[0].PageSetup.PrintTitleRows = "$1:$6";
                    fileName = monthName + "-" + year + "PayRegister" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                    string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                    workbook.Version = ExcelVersion.Excel2013;
                    workbook.SaveAs(fullPath);
                }
                if (reportType.ToUpper() == "PDF")
                {
                    workbook.Version = ExcelVersion.Excel2016;
                    workbook.Worksheets[0].PageSetup.PrintTitleRows = "$1:$6";
                    var converter = new ExcelToPdfConverter(workbook);
                    var pdfDoc = converter.Convert();
                    fileName = monthName + "-" + year + "PayRegister" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".pdf";
                    string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                    pdfDoc.Save(fullPathPDF);
                }
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;
            }
        }





        [HttpPost, Authorize]
        public ActionResult GetPaySlip(string month, string year, string languageId, Dictionary<string, string> parameters)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.ToInt());
            var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
            var fileName = "";
            string fromDate = 1 + "-" + monthName + "-" + year;
            string toDate = daysInMonth + "-" + monthName + "-" + year;

            string PdfLocation = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(languageId))
                {
                    throw new Exception("Select Language.");
                }

                clsReport objRpt = null;

                DataSet dsSlrProc = null;
                DataView dvSlrProc = null;
                DataSet dsHeading = null;
                DataSet dsCmp = null;
                DataSet dsFactory = null;

                objRpt = new clsReport(_sqlRepository);

                ParamList para = new ParamList();

                para.PlantId = identity.PlantId;
                para.CompanyGroupId = identity.CompanyGroupId;
                para.CompanyId = identity.CompanyId;


                para.FromDate = fromDate;//"01-" + bplib.clsWebLib.GetMonthName(ddlMonth.Text) + "-" + ddlYear.Items[ddlYear.SelectedIndex].Text;
                para.ToDate = toDate;//DateTime.DaysInMonth(Convert.ToInt32(ddlYear.Items[ddlYear.SelectedIndex].Text), Convert.ToInt32(ddlMonth.Text)) + "-" + bplib.clsWebLib.GetMonthName(ddlMonth.Text) + "-" + ddlYear.Items[ddlYear.SelectedIndex].Text;//Number of Days in a month
                string sortingParameters = "";
                sortingParameters = objRpt.GetSortingParameters(para.CompanyGroupId, para.CompanyId, para.PlantId, "");

                para.LanguageId = languageId;//ddlLanguageId.SelectedValue.Trim();
                #region DataSet
                _payRegisterBDReportService.GetSalaryInfoSlrProcIDWisePayGrpForReportNewLog(para, sortingParameters, parameters, out dsSlrProc);

                DataSet dsGrade = null;
                objRpt.GetGrade(para.EmployeeId, para.PayGroup, month, year, parameters, out dsGrade);//GetGrade
                //objRpt.GetSalaryInfoSlrProcIDWise(ddlSlrProcID.Text.Trim(), ddlPlant.SelectedValue.Trim(), lblEmpSystemID.Text, ddlStatus.SelectedValue.Trim(), out dsSlrProc);
                dvSlrProc = new DataView();
                dvSlrProc.Table = dsSlrProc.Tables[0];

                objRpt.GetPlantWiseCompany(identity.PlantId, languageId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                string CompanyName = string.Empty;
                string CompanyAddress = string.Empty;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CompanyName = dsCmp.Tables[0].Rows[0]["CompanyNameLocal"].ToString();
                    CompanyAddress = dsCmp.Tables[0].Rows[0]["CompanyAddressLocal"].ToString();
                }
                else
                {
                    CompanyAddress = "";
                    CompanyName = "";
                }




                ReportUtility oReportUtility = new ReportUtility();

                DataTable dtBioDvAC = null;
                DataTable dtBioDvAC1 = null;
                DataTable dtCompanny = null;



                LocalReport localReport = new LocalReport();
                localReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/HumanResource/Payrolls/PayslipReport.rdlc");



                //switch (dsSlrProc.Tables[0].Rows[0]["LanguageName"].ToString())
                //{
                //    case "English":
                //        localReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/HumanResource/Payrolls/PayslipReportEng.rdlc");
                //        break;
                //    case "Bengali":
                //        localReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/HumanResource/Payrolls/PayslipReport.rdlc");
                //        break;


                //    default:
                //        localReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/HumanResource/Payrolls/PayslipReportEng.rdlc");
                //        break;
                //}




                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Name = "PayrollsDataSet";




                reportDataSource.Value = dsSlrProc.Tables[0];
                //reportDataSource.Value = dtBioDvAC1;

                string TotalAmmountInWord = string.Empty;
                double TotalAmmount = 0;
                //if (dtCompanny.Rows.Count > 0)
                //{

                //    CompanyName = dtCompanny.Rows[0]["UserName"].ToString();
                //    CompanyAddress = dtCompanny.Rows[0]["Address1"].ToString();


                //}
                string PayslipName = string.Empty;
                switch (dsSlrProc.Tables[0].Rows[0]["LanguageName"].ToString())
                {
                    case "English":
                        PayslipName = month + "-" + year;
                        break;
                    case "Bengali":
                        PayslipName = bplib.clsWebLib.GetMonthNameBangla(month) + "-" + string.Concat(year);
                        break;


                    default:
                        PayslipName = month + "-" + year;
                        break;
                }

                ReportParameter[] parameter = new ReportParameter[]
                {
                    new ReportParameter("PayslipName", PayslipName),
                    new ReportParameter("CompanyAddress", CompanyAddress),
                    new ReportParameter("CompanyName", CompanyName)

                };
                localReport.SetParameters(parameter);
     
                localReport.DataSources.Add(reportDataSource);

                string ReportType = "pdf";
                String mimeType = string.Empty;
                String encoding = string.Empty;
                String extension = ReportType == "Excel" ? "xlsx" : "pdf";
                //String extension =  "png";
                Warning[] warnings = null;
                string[] streamids = null;
                Byte[] bytes = null;

                bytes = localReport.Render(ReportType, "", out mimeType, out encoding, out extension, out streamids, out warnings);
                fileName = DateTime.Now.ToString("dd-MMM-yyyy") + "_" + identity.Name + "_SalaryPaySlipRdlc.pdf";
                string savepath = ResourcesPathReader.SavePdfDocUrl();
                if (System.IO.File.Exists(savepath + fileName))
                {
                    try
                    {

                        System.IO.File.Delete(savepath + fileName);
                    }
                    catch (Exception ex)
                    {
                        //Do something
                    }
                }



                FileStream fs = new FileStream(savepath + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                byte[] data = new byte[fs.Length];
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
                
                string path = ResourcesPathReader.GetPdfDocUrl();
     
                string[] x = Library.Service.Helpers.ResourcesPathReader.SavePdfDocUrl().Split('\\');

                string RelativePath = Library.Service.Helpers.ResourcesPathReader.GetROOT_FOLDER_Without_APP_Name() + "/Output/" + fileName;
                return Json(new { FileName = RelativePath, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;

            }
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeInformation(string effectiveDate, string salaryProcessId, string monthNo, string yearNo, bool isActive, bool isSeperated, bool isMaternity)
        {
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNo.ToInt());
            var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(yearNo), Convert.ToInt32(monthNo));//Number of Days in a month

            string fromDate = 1 + "-" + monthName + "-" + yearNo;
            string toDate = daysInMonth + "-" + monthName + "-" + yearNo;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_payRegisterBDReportService.GetEmpInfo(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate, monthNo, yearNo, salaryProcessId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetEmployeeInformationContractor(string effectiveDate, string salaryProcessId, string monthNo, string yearNo, bool isActive, bool isSeperated, bool isMaternity)
        {
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNo.ToInt());
            var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(yearNo), Convert.ToInt32(monthNo));//Number of Days in a month

            string fromDate = 1 + "-" + monthName + "-" + yearNo;
            string toDate = daysInMonth + "-" + monthName + "-" + yearNo;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(_payRegisterBDReportService.GetEmpInfoContractor(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate, monthNo, yearNo, salaryProcessId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId, isActive, isSeperated, isMaternity), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }



        #region SalaryRegisterSorting       
        [HttpPost, Authorize]
        public ActionResult GetPlantWiseSalaryRegisterSortingParameters()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payRegisterBDReportService.GetPlantWiseSalaryRegisterSortingParameters(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpPost, Authorize]
        public JsonResult Create(IEnumerable<PlantWiseSalaryRegisterSortingParameters> PlantWiseSalaryRegisterSortingParameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            foreach (var item in PlantWiseSalaryRegisterSortingParameters)
            {

                item.AddedBy = identity.Name;
                item.UpdatedBy = identity.Name;
                item.AddedFromIP = identity.IPAddress;

                item.UpdatedFromIP = identity.IPAddress;

            }


            _payRegisterBDReportService.InsertORUpdate(PlantWiseSalaryRegisterSortingParameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId);
            return Json(new { PlantWiseSalaryRegisterSortingParameters, Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public ActionResult GetPayRegisterReportConfigList()
        {
            DataTable dtCheckConfig = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM PayRegisterReportConfig where PlantId = '" + identity.PlantId + @"'";
            dtCheckConfig = _sqlRepository.GetDataTable(sql);


            foreach (var item in Enum.GetValues(typeof(PayRegisterCofigEnum)))
            {
                dtCheckConfig.DefaultView.RowFilter = "FieldName='" + item.ToString() + "'";
                if (dtCheckConfig.DefaultView.Count == 0)
                {
                    DataRow dr = dtCheckConfig.NewRow();
                    dr["FieldName"] = item.ToString();
                    dr["Applicable"] = 0;
                    dr["PlantId"] = identity.PlantId;

                    dtCheckConfig.Rows.Add(dr);
                }
            }


            //SavePayRegisterReportConfig(null);
            return Json(CustomJsonResult.DataTableToJson(dtCheckConfig), JsonRequestBehavior.AllowGet);
        }




        [HttpPost, Authorize]
        public ActionResult GetPayRegisterRowPerPage()
        {
            DataTable dtCheckConfig = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM PayRegisterRowPerPage where PlantId = '" + identity.PlantId + @"'";
            dtCheckConfig = _sqlRepository.GetDataTable(sql);


            foreach (var item in Enum.GetValues(typeof(PayRegisterSettingsPerPage)))
            {
                dtCheckConfig.DefaultView.RowFilter = "Setting='" + item.ToString() + "'";
                if (dtCheckConfig.DefaultView.Count == 0)
                {

                    DataRow dr = dtCheckConfig.NewRow();
                    dr["Setting"] = item.ToString();
                    if (item.ToString() == PayRegisterSettingsPerPage.EarningExceptAttendance.ToString())
                    {
                        dr["NumberOfRowsPerPage"] = 9;
                    }
                    if (item.ToString() == PayRegisterSettingsPerPage.StructreAndEarningExceptAttendance.ToString())
                    {
                        dr["NumberOfRowsPerPage"] = 9;
                    }
                    if (item.ToString() == PayRegisterSettingsPerPage.EarningWithAttendance.ToString())
                    {
                        dr["NumberOfRowsPerPage"] = 6;
                    }
                    if (item.ToString() == PayRegisterSettingsPerPage.StructureAndEarningWithAttendance.ToString())
                    {
                        dr["NumberOfRowsPerPage"] = 6;
                    }
                    dr["PlantId"] = identity.PlantId;

                    dtCheckConfig.Rows.Add(dr);
                }
            }


            //SavePayRegisterReportConfig(null);
            return Json(CustomJsonResult.DataTableToJson(dtCheckConfig), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult UpdatePayRegisterReportConfigList(List<Dictionary<string, object>> data)
        {

            try
            {
                SavePayRegisterReportConfig(data);
                return Json(new { Error = false, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        private void SavePayRegisterReportConfig(List<Dictionary<string, object>> data)
        {

            try
            {

                DataTable dtCheckConfig = null;
                //string sql = @"SELECT distinct FieldName FROM PayRegisterReportConfig ";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT *  FROM PayRegisterReportConfig where PlantId = '" + identity.PlantId + @"'", out dsMaster, false, "1");
                foreach (var item in Enum.GetValues(typeof(PayRegisterCofigEnum)))
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "FieldName='" + item.ToString() + "'";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {

                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["FieldName"] = item.ToString();
                        if (data != null)
                        {
                            List<Dictionary<string, object>> curValue = data.Where(dic => dic["FieldName"].ToString() == item.ToString()).ToList();
                            foreach (var val in curValue)
                            {
                                dr["Applicable"] = bplib.clsWebLib.GetBoolData(val["Applicable"].ToString());
                            }
                        }
                        dr["PlantId"] = identity.PlantId;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        if (data != null)
                        {
                            List<Dictionary<string, object>> curValue = data.Where(dic => dic["FieldName"].ToString() == item.ToString()).ToList();
                            foreach (var val in curValue)
                            {
                                dr["Applicable"] = bplib.clsWebLib.GetBoolData(val["Applicable"].ToString());
                            }
                        }
                        dr.EndEdit();

                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult UpdatePayRegisterRowPerPageList(List<Dictionary<string, object>> data)
        {

            try
            {
                SavePayRegisterRowPerPageConfig(data);
                return Json(new { Error = false, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        private void SavePayRegisterRowPerPageConfig(List<Dictionary<string, object>> data)
        {

            try
            {

                DataTable dtCheckConfig = null;
                //string sql = @"SELECT distinct Setting FROM PayRegisterReportConfig ";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT *  FROM PayRegisterRowPerPage where PlantId = '" + identity.PlantId + @"'", out dsMaster, false, "1");
                foreach (var item in Enum.GetValues(typeof(PayRegisterSettingsPerPage)))
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "Setting='" + item.ToString() + "'";
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {

                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Setting"] = item.ToString();
                        if (data != null)
                        {
                            List<Dictionary<string, object>> curValue = data.Where(dic => dic["Setting"].ToString() == item.ToString()).ToList();
                            foreach (var val in curValue)
                            {
                                dr["NumberOfRowsPerPage"] = bplib.clsWebLib.GetNumData(val["NumberOfRowsPerPage"].ToString());
                            }
                        }
                        dr["PlantId"] = identity.PlantId;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        if (data != null)
                        {
                            List<Dictionary<string, object>> curValue = data.Where(dic => dic["Setting"].ToString() == item.ToString()).ToList();
                            foreach (var val in curValue)
                            {
                                dr["NumberOfRowsPerPage"] = bplib.clsWebLib.GetNumData(val["NumberOfRowsPerPage"].ToString());
                            }
                        }
                        dr.EndEdit();

                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult saveSignatoryConfig(List<PayRegisterSignatoryField> PayRegisterSignatoryField)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            DataSet dsSignatoryConfig = new DataSet();

            if (PayRegisterSignatoryField == null)
                PayRegisterSignatoryField = new List<PayRegisterSignatoryField>();

            if (PayRegisterSignatoryField.Count > 5)
                throw new Exception("Signatory can not be more then 5!!!");


            string fieldIds = "''";
            for (int i = 0; i < PayRegisterSignatoryField.Count; i++)
            {
                fieldIds += ",'" + PayRegisterSignatoryField[i].Id + "'";
            }

            string sql = "SELECT * FROM PayRegisterSignatoryField where Id IN (" + fieldIds + ") ";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql, out dsSignatoryConfig, false, "1");


            var duplicatedSeq = from p in PayRegisterSignatoryField
                                group p by p.Sequence.ToString().Trim() into g
                                where g.Count() > 1
                                select g.Key;
            var duplSeqlist = PayRegisterSignatoryField.FindAll(p => duplicatedSeq.Contains(p.Sequence));
            if (duplSeqlist.Count > 1)
                throw new Exception("Sequence can't be duplicate.");

            var duplicatedFieldName = from p in PayRegisterSignatoryField
                                      group p by p.FieldName.ToString().Trim() into g
                                      where g.Count() > 1
                                      select g.Key;
            var duplicatedFieldList = PayRegisterSignatoryField.FindAll(p => duplicatedFieldName.Contains(p.FieldName));
            if (duplicatedFieldList.Count > 1)
                throw new Exception("Sequence can't be duplicate.");

            for (int i = 0; i < PayRegisterSignatoryField.Count; i++)
            {
                if (string.IsNullOrEmpty(PayRegisterSignatoryField[i].Sequence) == true)
                    throw new Exception("Sequence can not be empty.");
                if (string.IsNullOrEmpty(PayRegisterSignatoryField[i].FieldName) == true)
                    throw new Exception("Field can not be empty.");

                dsSignatoryConfig.Tables[0].DefaultView.RowFilter = "Id=" + PayRegisterSignatoryField[i].Id + "";
                if (dsSignatoryConfig.Tables[0].DefaultView.Count == 0)
                {
                    DataRow dr = dsSignatoryConfig.Tables[0].NewRow();

                    dr["PlantId"] = identity.PlantId;
                    dr["Sequence"] = PayRegisterSignatoryField[i].Sequence;
                    dr["FieldName"] = PayRegisterSignatoryField[i].FieldName;

                    dsSignatoryConfig.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsSignatoryConfig.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["PlantId"] = identity.PlantId;
                    dr["Sequence"] = PayRegisterSignatoryField[i].Sequence;
                    dr["FieldName"] = PayRegisterSignatoryField[i].FieldName;

                    dr.EndEdit();
                }
            }
            clsStaticInfo info = new clsStaticInfo();
            info.SaveDataSets(dsSignatoryConfig);

            return Json(new { PayRegisterSignatoryField, Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult GetPayRegisterSignatoryFieldByList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * FROM PayRegisterSignatoryField WHERE PlantId = '" + identity.PlantId + @"' ORDER BY Sequence";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeletePayRegisterSignatoryFieldById(string id)
        {

            ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
            connection.BeginTransaction();

            connection.executeQuery(@"DELETE FROM PayRegisterSignatoryField where Id = '" + id + "' ");
            connection.CommitTransaction();
            return Json(new { Message = AplosMessage.Deleted });
        }







        [HttpPost, Authorize]
        public ActionResult GetPaySlipCompliance(string month, string year, string languageId, Dictionary<string, string> parameters)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.ToInt());
            var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
            var fileName = "";
            string fromDate = 1 + "-" + monthName + "-" + year;
            string toDate = daysInMonth + "-" + monthName + "-" + year;

            string PdfLocation = string.Empty;
            try
            {


                clsReport objRpt = null;

                DataSet dsSlrProc = null;
                DataView dvSlrProc = null;
                DataSet dsHeading = null;
                DataSet dsCmp = null;
                DataSet dsFactory = null;

                objRpt = new clsReport(_sqlRepository);

                ParamList para = new ParamList();

                para.PlantId = identity.PlantId;
                para.CompanyGroupId = identity.CompanyGroupId;
                para.CompanyId = identity.CompanyId;


                para.FromDate = fromDate;//"01-" + bplib.clsWebLib.GetMonthName(ddlMonth.Text) + "-" + ddlYear.Items[ddlYear.SelectedIndex].Text;
                para.ToDate = toDate;//DateTime.DaysInMonth(Convert.ToInt32(ddlYear.Items[ddlYear.SelectedIndex].Text), Convert.ToInt32(ddlMonth.Text)) + "-" + bplib.clsWebLib.GetMonthName(ddlMonth.Text) + "-" + ddlYear.Items[ddlYear.SelectedIndex].Text;//Number of Days in a month
                string sortingParameters = "";
                sortingParameters = objRpt.GetSortingParameters(para.CompanyGroupId, para.CompanyId, para.PlantId, "");

                para.LanguageId = languageId;//ddlLanguageId.SelectedValue.Trim();
                #region DataSet
                //objRpt.GetSalaryInfoSlrProcIDWisePayGrpForReportNewLogCompliance(para, sortingParameters, parameters, out dsSlrProc);
                GetSalaryInfoSlrProcIDWisePayGrpForReportNewLogCompliance(para, sortingParameters, parameters, out dsSlrProc);


                DataSet dsGrade = null;
                objRpt.GetGrade(para.EmployeeId, para.PayGroup, month, year, parameters, out dsGrade);//GetGrade
                //objRpt.GetSalaryInfoSlrProcIDWise(ddlSlrProcID.Text.Trim(), ddlPlant.SelectedValue.Trim(), lblEmpSystemID.Text, ddlStatus.SelectedValue.Trim(), out dsSlrProc);
                dvSlrProc = new DataView();
                dvSlrProc.Table = dsSlrProc.Tables[0];

                objRpt.GetPlantWiseCompany(identity.PlantId, languageId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                string CompanyName = string.Empty;
                string CompanyAddress = string.Empty;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CompanyName = dsCmp.Tables[0].Rows[0]["CompanyNameLocal"].ToString();
                    CompanyAddress = dsCmp.Tables[0].Rows[0]["CompanyAddressLocal"].ToString();
                }
                else
                {
                    CompanyAddress = "";
                    CompanyName = "";
                }




                ReportUtility oReportUtility = new ReportUtility();

                DataTable dtBioDvAC = null;
                DataTable dtBioDvAC1 = null;
                DataTable dtCompanny = null;



                LocalReport localReport = new LocalReport();
                localReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/HumanResource/Payrolls/PayslipReport.rdlc");



                //switch (dsSlrProc.Tables[0].Rows[0]["LanguageName"].ToString())
                //{
                //    case "English":
                //        localReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/HumanResource/Payrolls/PayslipReportEng.rdlc");
                //        break;
                //    case "Bengali":
                //        localReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/HumanResource/Payrolls/PayslipReport.rdlc");
                //        break;


                //    default:
                //        localReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/HumanResource/Payrolls/PayslipReportEng.rdlc");
                //        break;
                //}




                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Name = "PayrollsDataSet";




                reportDataSource.Value = dsSlrProc.Tables[0];
                //reportDataSource.Value = dtBioDvAC1;

                string TotalAmmountInWord = string.Empty;
                double TotalAmmount = 0;
                //if (dtCompanny.Rows.Count > 0)
                //{

                //    CompanyName = dtCompanny.Rows[0]["UserName"].ToString();
                //    CompanyAddress = dtCompanny.Rows[0]["Address1"].ToString();


                //}
                string PayslipName = string.Empty;
                switch (dsSlrProc.Tables[0].Rows[0]["LanguageName"].ToString())
                {
                    case "English":
                        PayslipName = month + "-" + year;
                        break;
                    case "Bengali":
                        PayslipName = bplib.clsWebLib.GetMonthNameBangla(month) + "-" + string.Concat(year);
                        break;


                    default:
                        PayslipName = month + "-" + year;
                        break;
                }

                ReportParameter[] parameter = new ReportParameter[]
                {
                    new ReportParameter("PayslipName", PayslipName),
                    new ReportParameter("CompanyAddress", CompanyAddress),
                    new ReportParameter("CompanyName", CompanyName)

                };
                localReport.SetParameters(parameter);
                //reportDataSource.Value = db.OnlineApplications.Where(x => x.StudentCode == "2019-Three-001").FirstOrDefault();

                localReport.DataSources.Add(reportDataSource);

                string ReportType = "pdf";
                String mimeType = string.Empty;
                String encoding = string.Empty;
                String extension = ReportType == "Excel" ? "xlsx" : "pdf";
                //String extension =  "png";
                Warning[] warnings = null;
                string[] streamids = null;
                Byte[] bytes = null;

                bytes = localReport.Render(ReportType, "", out mimeType, out encoding, out extension, out streamids, out warnings);
                //string PDFPath = new DirectoryInfo(System.Web.HttpContext.Current.Server.MapPath("~/")) + "Reports\\PDF\\";
                fileName = DateTime.Now.ToString("dd-MMM-yyyy") + "_" + identity.Name + "_SalaryPaySlipRdlc.pdf";
                //string fileName = "iDCard" + DateTime.Now.ToFileTime() + ".png";
                //bool IsExitsPDF = System.IO.File.Exists(PDFPath + fileName);
                string savepath = ResourcesPathReader.SavePdfDocUrl();
                //ShowLog(savepath); 

                //fileName = DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy") + "_" + (string)Session["USER"] + "_SalaryPaySlipRdlc.pdf";
                if (System.IO.File.Exists(savepath + fileName))
                {
                    try
                    {

                        System.IO.File.Delete(savepath + fileName);
                    }
                    catch (Exception ex)
                    {
                        //Do something
                    }
                }



                FileStream fs = new FileStream(savepath + fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                byte[] data = new byte[fs.Length];
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
                //var keyname = System.Configuration.ConfigurationManager.AppSettings["APP_NAME"];
                //PdfLocation =   keyname+"/PDF/" + fileName;
                //PdfLocation = "/Reports/PDF/" + fileName;

                //report.Attributes["src"] = PdfLocation;
                //ViewBag.ReportPath = PdfLocation;
                //string path = Server.MapPath("/Reports/PDF/");
                //string fileName = string.Empty;
                //fileName = "EmployeePayslipReport" + DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy")  + ".pdf"; ;

                string path = ResourcesPathReader.GetPdfDocUrl();

                //fileName = DateTime.Now.AddDays(-1).ToString("dd-MMM-yyyy") + "_" + (string)Session["USER"] + "_SalaryPaySlipRdlc.pdf";
                //if (File.Exists(path + fileName))
                //{
                //    try
                //    {
                //        File.Delete(path + fileName);
                //    }
                //    catch (Exception ex)
                //    {
                //        //Do something
                //    }
                //}
                //ShowLog(path.ToString() + "and" + savepath.ToString());
                string[] x = Library.Service.Helpers.ResourcesPathReader.SavePdfDocUrl().Split('\\');

                //string RelativePath = Library.Service.Helpers.ResourcesPathReader.GetROOT_FOLDER_Without_APP_Name() + "/" + x[x.Length - 1] + "/" + fileName;
                string RelativePath = Library.Service.Helpers.ResourcesPathReader.GetROOT_FOLDER_Without_APP_Name() + "/Output/" + fileName;
                //report.Attributes.Add("src", path + fileName);
                return Json(new { FileName = RelativePath, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                // throw ex;

            }
        }

        public void GetSalaryInfoSlrProcIDWisePayGrpForReportNewLogCompliance(ParamList para, string sortingParameters, Dictionary<string, string> parameters, out DataSet dsRef)
        {
            string strSQL;
            string sqlL;
            string salaryProcessSystemId = "";
            ConnectionManager.DAL.ConManager objCon;
            string sql1 = "select * From ComplianceAttendanceSetting where PlantId ='" + para.PlantId + @"' ";
            DataTable dtValidation = _sqlRepository.GetDataTable(sql1);

            string wcBasedOnSetting = "1 = 1 ";
            string OTCase = "";

            if (bplib.clsWebLib.GetBoolData(dtValidation.Rows[0]["IsNoPunchOnWeekOffForOTEntitle"]))
            {
                wcBasedOnSetting += "AND (DT.Category IN ('Present','Late','Half Day') AND DT.OriginalDayType != 'W')";
            }
            else
            {
                OTCase += @"WHEN (Category IN ('Present','Late','Half Day') AND OriginalDayType = 'W') THEN FOT.TotalOTHr";
            }
            if (bplib.clsWebLib.GetBoolData(dtValidation.Rows[0]["IsNoPunchOnHolidayForOTEntitle"]))
            {
                wcBasedOnSetting += @"AND (DT.Category IN ('Present','Late','Half Day') AND DT.OriginalDayType != 'H')";
            }
            else
            {
                OTCase += " WHEN ( Category IN ('Present','Late','Half Day') AND OriginalDayType = 'H') THEN FOT.TotalOTHr";

            }

            if (!string.IsNullOrEmpty(para.LanguageId))
            {

                sqlL = para.LanguageId;
            }
            else
            {
                sqlL = @"F.LanguageId";
            }

            salaryProcessSystemId = @"SystemId IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + para.PlantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + para.FromDate + "') AND YearNo = Year('" + para.FromDate + "')  )";

            try
            {
                strSQL = @"SELECT EmpSlr.EmpInfoSystemID, EmpBasic.EmployeeCode EmployeeCode, EmpBasic.EmployeeName
                                ,isnull(EmpBasic.EmployeeNameLocal,EmpBasic.EmployeeName) EmployeeNameLocal
                                , Replace(Convert(varchar(11),EmpBasic.DOJ,105),' ','-') DOJ
                                , REPLACE(Convert(VARCHAR(11), EmpBasic.DOB, 106), ' ', '-') AS DOB
                                ,REPLACE(Convert(VARCHAR(11), EmpBasic.DOS, 106), ' ', '-') AS DOS,
                                 EmpBasic.EmployeeStatus, EmpSlr.UnitID, EmpSlr.UnitName, EmpSlr.DivisionID
                               ,EmpSlr.DivisionName
                               ,ISNULL(EmpSlr.SectionLocal,EmpSlr.SectionName) SectionLocal,
                                EmpSlr.DepartmentID, EmpSlr.DepartmentName
                                , EmpSlr.SectionID, EmpSlr.SectionName, EmpSlr.SubSectionID,
                                EmpSlr.SubSectionName, EmpSlr.EmployeeCategorySystemID, EmpSlr.EmpCategoryName                               
                                ,LegalDesignationLocal GivenDesignationName
                                ,EmpBasic.GivenDesignationGroup, EmpBasic.PlantName
							    ,EmpBasic.PayrollGroup                               
                                ,GradeCode = ISNULL(EmpSlr.GradeCode,'')+ISNULL(  CASE WHEN  isnull(EmpSlr.lineName,'')<>'' THEN ' ( '+ isnull(EmpSlr.lineName,'')+' ) ' ELSE '' END,'') 
                                ,esic.ESICNo,pf.UANNo,bb.BankAccNo,bb.BankName, '' BankNameFull,ISNULL(MMDSA.TotalLate, 0) TotalLate
								,(ISNULL(MMDSA.TotalPresent, 0) + ISNULL(MMDSA.TotalLate, 0)) PresentDays,PaymentMode,
								ISNULL(MMDSA.AbsentDays, 0) AbsentDays,  ISNULL(MMDSA.TotalHoliDay + MMDSA.TotalWeekOffHoliDay,0) HoliDay, ISNULL(MMDSA.TotalWeekOff, 0) WeekOff
                                ,ISNULL(MMDSA.TotalProcDate, 0) TotalProcDate
								,(ISNULL(MMDSA.TotalLv, 0) + ISNULL(MMDSA.TotalMLv, 0)) LeaveDays,ISNULL(MMDSA.TotalLWP,0) LWP, EmpSlr.SlrProcChdSysID, EmpSlr.SlrProcMstSystemID, EmpSlr.SalaryProcID,
                                EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate, EmpSlr.MonthNo, EmpSlr.YearNo, EmpSlr.PayAbleShSystemID,
                                EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID,  Convert(decimal(18,0),EmpSlr.EntryAmount) EntryAmount,
                                --OTRate=Convert(decimal(18,2),(Convert(decimal(18,2),EmpSlr.EntryAmount)*2)/208),
                                EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount,
                                EmpSlr.DisbusmentCurrencyID
                                , DisbusmentAmount=case when EmpSlr.HeadCategory='OverTime' then CONVERT(DECIMAL(10, 3),EmpSlr.OTRate )*CONVERT(DECIMAL(10, 2), ISNULL(MMDSA.TotalOTHr,0)/60)  else Convert(decimal(18,0),EmpSlr.DisbusmentAmount) end 
                                , EmpSlr.AcltExcDisbSlrHDID, EmpSlr.AcltExcDisbSlrHDAmt,
                                EmpSlr.PlantWiseExchangeCR, EmpSlr.ExchangeRate, EmpSlr.AmtDefinitionCurrencyID, EmpSlr.AmtDefinitionCurrency,
                                EmpSlr.AmtDefinitionCurrencyRate, EmpSlr.IsNetPayEffect
                                ,EmpSlr.SalaryHead,EmpSlr.HeadCategory,EmpSlr.HeadType,EmpSlr.Sequence,EmpSlr.IsCTCComponent,EmpSlr.IsGrossComponent
                                ,isnull(empslr.SalaryHeadBangla,EmpSlr.SalaryHead) SalaryHeadBangla
	                            ,IIF(EmpSlr.IsCTCComponent = 1 AND EmpSlr.IsGrossComponent = 1, 1, 0) as IsGross 
								,IIF(EmpSlr.PartOfNetPay = 1 AND EmpSlr.IsGrossComponent = 0, 1, 0) as IsCTC 
                                , isnull(vpf.VoluntaryPFValue,0) VoluntaryPFValue
                                , isnull(BonusValue.Bonus,0) Bonus
								, BonusValue.SalaryHead B_SalaryHead
								, BonusValue.SalaryHeadID B_SalaryHeadID
								, BonusValue.IsCTCComponent B_IsCTCComponent
								, BonusValue.IsGrossComponent B_IsGrossComponent
								, BonusValue.HeadType B_HeadType								
                                ,VPFhead.HeadCategory xHeadCategory,VPFhead.SalaryHeadID xSalaryHeadID
                                ,VPFhead.SalaryHead xSalaryHead,VPFhead.IsCTCComponent xIsCTCComponent
                                ,VPFhead.IsGrossComponent xIsGrossComponent,VPFhead.HeadType xHeadType
                                ,VPFhead.SalaryRuleMasterSystemID xSalaryRuleMasterSystemID
                                ,MW.Grade,CONVERT(DECIMAL(10, 2), ISNULL(MMDSA.TotalOTHr,0)/60) TotalOTHr
                                , CTC=Convert(decimal(18,0),EmpSlr.CTC) 
                                      + CONVERT(DECIMAL(10, 3),EmpSlr.OTRate )*CONVERT(DECIMAL(10, 2), ISNULL(MMDSA.TotalOTHr,0)/60)   
								      - EmpSlr.OTAmount
                                , Convert(decimal(18,0),EmpSlr.Deduct) Deduct
                                ,Convert(decimal(18,0),EmpSlr.Gross) Gross
,Convert(decimal(18,0),EmpSlr.DisbusmentGross) DisbusmentGross,   BankAccNo , CONVERT(DECIMAL(10, 3),EmpSlr.OTRate )+0.0001 OTRate  
,EmpSlr.IsOTEntitle, ISNULL(EmpSlr.LunchOutHour,0) LunchOutHour
,ISNULL(EmpSlr.GrossLableLocal,EmpSlr.GrossLable) GrossLableLocal,
EmpBasic.NameLabel ,
EmpBasic.IDNoLabel	  ,
EmpBasic.DesignationLabel	  ,
EmpBasic.DivisionLabel  ,
EmpBasic.DOJLabel	  ,
EmpBasic.GradeLabel	  ,
EmpBasic.SalaryLabel		  ,
EmpBasic.TotalLabel	  ,
EmpBasic.SerialNumberLabel  ,
EmpBasic.PersonalInformationLabel	  ,
EmpBasic.PresentInformationLabel	  ,
EmpBasic.WageRangeLabel	  ,
EmpBasic.CreditLabel	  ,
EmpBasic.DeductionsLabel	  ,
EmpBasic.NetPayableLabel	  ,
EmpBasic.OtRateLabel	  ,
EmpBasic.TotalPayableMoneyLabel		  ,
EmpBasic.TotalDeductionLabel	  ,
EmpBasic.NetTotalPayableMoneyLabel	  ,
EmpBasic.BankAccountLabel	  ,
EmpBasic.LaborSignatureLabel	  ,
EmpBasic.SignaturesLabel	  ,
EmpBasic.AuthoritiesLabel	  ,
EmpBasic.PresentDaysLabel	  ,
EmpBasic.WeeklyHolidaysLabel	  ,
EmpBasic.AvailHolydayLabel  ,
EmpBasic.FestivalHolidaysLabel	  ,
EmpBasic.AbsentDaysLabel  ,
EmpBasic.TotalAttendanceLabel	  ,
EmpBasic.OTHoursLabel  ,
EmpBasic.LunchHourLabel	  ,
EmpBasic.OfficeCopyLabel	  ,
EmpBasic.EmployeeCopyLabel	  ,
EmpBasic.DateLabel	  ,
EmpBasic.PaySlipLabel,EmpBasic.LanguageName	,EmpBasic.PayableSalary,EmpSlr.LineId  
,EmpSlr.DesignationSequence,EmpSlr.PlantSequence,EmpSlr.UnitSequence,EmpSlr.DivisionSequence,EmpSlr.SubDivisionSequence,EmpSlr.DepartmentSequence
                                                                  ,EmpSlr.SectionSequence,EmpSlr.SubSectionSequence,EmpSlr.EntitySequence,EmpBasic.EmployeeCodePreFix,EmpBasic.EmployeeCodeNumeric
                            FROM
                                    (
									  SELECT E.SystemID, E.EmployeeCode EmployeeCode, E.EmployeeName,E.EmployeeNameLocal, E.DOJ,E.DOB,E.DOS, E.EmployeeStatus											
											,PG.UserName PayrollGroup,F.UserName PlantName
											
											, E.PlantID,empOTEn.IsOTEntitle
                                            ,egdsgg.GivenDesignationGroup,e.SalaryRuleMasterSystemID,
	---------------------------------------------------------
ISNULL(Namet.Name              ,'Name') NameLabel ,
ISNULL(IDNo.Name 			   ,'ID No')	IDNoLabel	  ,
ISNULL(Designation.Name 	   ,'Designation')	DesignationLabel	  ,
ISNULL(Division.Name 		   ,'Division')		DivisionLabel  ,
ISNULL(DOJ.Name 			   ,'DOJ')	DOJLabel	  ,
ISNULL(Grade.Name 			   ,'Grade') GradeLabel	  ,
ISNULL(Salary.Name 			   ,'Salary') SalaryLabel		  ,
ISNULL(Total.Name 			   ,'Total') TotalLabel	  ,
ISNULL(SerialNumber.Name 	   ,'Serial No') SerialNumberLabel  ,
ISNULL(PersonalInformation.Name,'Personal Information')	PersonalInformationLabel	  ,
ISNULL(PresentInformation.Name ,'Attendance Information')	PresentInformationLabel	  ,
ISNULL(WageRange.Name 		   ,'Salary Distribution')	WageRangeLabel	  ,
ISNULL(Credit.Name 			   ,'Other Earning')	CreditLabel	  ,
ISNULL(Deductions.Name 		   ,'Deductions')	DeductionsLabel	  ,
ISNULL(NetPayable.Name 		   ,'Net Payable')	NetPayableLabel	  ,
ISNULL(OtRate.Name 			   ,'OT Rate')	OtRateLabel	  ,
ISNULL(TotalPayableMoney.Name  ,'Total Payable')TotalPayableMoneyLabel		  ,
ISNULL(TotalDeduction.Name 	   ,'Total Deduction')	TotalDeductionLabel	  ,
ISNULL(NetTotalPayableMoney.Name,'Net Payable') NetTotalPayableMoneyLabel	  ,
ISNULL(BankAccount.Name 	   ,'Bank Account')	BankAccountLabel	  ,
ISNULL(LaborSignature.Name 	   ,'Labor Signature')	LaborSignatureLabel	  ,
ISNULL(Signatures.Name 		   ,'Signatures')	SignaturesLabel	  ,
ISNULL(Authorities.Name 	   ,'Authorities')	AuthoritiesLabel	  ,
CONCAT(ISNULL(PresentDayss.Name 	   ,'Present '),' ', ISNULL(Days.Name,'Days')) AS	PresentDaysLabel ,
ISNULL(WeeklyHolidays.Name 	   ,'Weekly Holidays')	WeeklyHolidaysLabel	  ,
ISNULL(AvailHolyday.Name 	   ,'Availed Leave')		AvailHolydayLabel  ,
ISNULL(FestivalHolidays.Name   ,'Festival Holidays')	FestivalHolidaysLabel	  ,

CONCAT(ISNULL(AbsentDayss.Name 	   ,'Absent '),' ', ISNULL(Days.Name,'Days')) AS	AbsentDaysLabel ,
ISNULL(TotalAttendance.Name    ,'Total Attendance')	TotalAttendanceLabel	  ,
ISNULL(OTHours.Name 		   ,'OT Hours')		OTHoursLabel  ,
ISNULL(LunchHour.Name 		   ,'Absent Hour')	LunchHourLabel	  ,
ISNULL(OfficeCopy.Name 		   ,'Office Copy')	OfficeCopyLabel	  ,
ISNULL(EmployeeCopy.Name 	   ,'Employee Copy')	EmployeeCopyLabel	  ,
ISNULL(Dates.Name 			   ,'Date')	DateLabel	  ,
ISNULL(Days.Name 			   ,'Days')	DaysLabel	  ,
ISNULL(PaySlip.Name 		   ,'Pay Slip')	PaySlipLabel, L.UserName LanguageName,
ISNULL(PayableSalary.Name 		   ,'Payable Salary')	PayableSalary 
,E.EmployeeCodePreFix,E.EmployeeCodeNumeric 
-------------------------------------------------------------------------------  
                                     FROM EmployeeInformation E

                                    LEFT JOIN [ORG].[Plant] F ON F.Id = E.PlantId
                                                LEFT JOIN EmployeeOTEntitle empOTEn on empOTEn.EmpSystemID=E.SystemId
                                                LEFT JOIN SCS.Language L ON L.id = " + sqlL + @"
											LEFT JOIN MST.PayrollGroupMaster PGM ON PGM.EmployeeId = E.SystemId
											LEFT JOIN HKP.PayrollGroup PG ON PG.Id = PGM.PayrollGroupId 
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Name' ) Namet ON Namet.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='IDNo' ) IDNo ON IDNo.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Designation'  ) Designation ON Designation.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Section'	  ) Division ON Division.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='DOJ' ) DOJ ON DOJ.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Grade' ) Grade ON Grade.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Salary') Salary ON Salary.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Total') Total ON Total.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='SerialNumber') SerialNumber ON SerialNumber.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmployeeInformation'	 ) PersonalInformation ON PersonalInformation.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='AttendanceInfo'	  ) PresentInformation ON PresentInformation.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='SalaryDistribution'			  ) WageRange ON WageRange.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='OtherEarning' 				  ) Credit ON Credit.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Deduction'			  ) Deductions ON Deductions.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='NetPayable'			  ) NetPayable ON NetPayable.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='OTRate'				  ) OtRate ON OtRate.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='TotalPayable'	  ) TotalPayableMoney ON TotalPayableMoney.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='TotalDeduction'		  ) TotalDeduction ON TotalDeduction.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='NetPayable'	  ) NetTotalPayableMoney ON NetTotalPayableMoney.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='BankAccountNo'			  ) BankAccount ON BankAccount.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='LaborSignature'		  ) LaborSignature ON LaborSignature.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Signature'			  ) Signatures ON Signatures.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Authority'			  ) Authorities ON Authorities.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='PresentDays'			  ) PresentDays ON PresentDays.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='WeeklyLeaveDays'		  ) WeeklyHolidays ON WeeklyHolidays.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='AvailedLeave'			  ) AvailHolyday ON AvailHolyday.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='FestivalHoliDay'		  ) FestivalHolidays ON FestivalHolidays.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Present'			  ) PresentDayss ON PresentDayss.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Absent'			  ) AbsentDayss ON AbsentDayss.LanguageId=" + sqlL + @"

LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='TotalAttendance'		  ) TotalAttendance ON TotalAttendance.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='OTHours'				  ) OTHours ON OTHours.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='LunchOutHour'			  ) LunchHour ON LunchHour.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='OfficeCopy'			  ) OfficeCopy ON OfficeCopy.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='EmployeeCopy'			  ) EmployeeCopy ON EmployeeCopy.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Date'					  ) Dates ON Dates.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='PaySlip'				  ) PaySlip ON PaySlip.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='Days'				  ) Days ON Days.LanguageId=" + sqlL + @"
LEFT JOIN (SELECT LanguageId,Name FROM HKP.LocalLanguage WHERE LabelName='PayableSalary'				  ) PayableSalary ON PayableSalary.LanguageId=" + sqlL + @"
												
												LEFT JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									            ,dg.UserName GivenDesignationGroup
									            FROM MST.DesignationMaster dm
									            LEFT JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									            ) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									            and egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID

									) EmpBasic
                                    INNER JOIN
											(
											 SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
													CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
													CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect,sh.PartOfNetPay
                                                    ,sh.SalaryHead,sh.HeadCategory,sh.HeadType,isnull(psh.Sequence, 99) Sequence
                                                    --,sh.IsCTCComponent
                                            ,IsCTCComponent=CASE WHEN sh.IsGrossComponent=0 AND sh.PartOfNetPay=1 AND sh.HeadType='E' THEN 1 ELSE 0 END
                                            ,sh.IsGrossComponent,BSH.Name SalaryHeadBangla
                                            ,ctc.CTC
                                            ,OTAmount.OTAmount
                                            ,de.Deduct
                                            ,deg.Gross                                           
                                            ,otstatus.OTRate OTrate
                                            ,otstatus.IsOTEntitled IsOTEntitle
                                            ,LunchOutHour.LunchOutHour 
                                            ,deg.GrossLable
                                            ,deg.GrossLableLocal,deg.DisbusmentGross
                                            ,ISNULL(lineLang.Name,line.UserName) lineName
                                            ,subDV.Id SubdivisionID,subDV.UserName SubDivision
                                            ,isnull(FU.Id,'')UnitID ,isnull(FU.UserName,'')UnitName 
                                            ,isnull(DV.Id,'')DivisionID ,isnull(FU.UserName,'')DivisionName 
                                            ,SecLocLang.Name SectionLocal,S.UserName SectionName
                                            ,ISNULL(S.Id,'') SectionId,ISNULL(SS.Id,'') SubSectionId, ISNULL(SS.UserName,'') SubSectionName
                                            ,ISNULL(DP.Id,'') DepartmentID ,isnull(DP.UserName,'') DepartmentName 
                                            ,ISNULL(EC.Id,'')EmployeeCategorySystemID,ISNULL(EC.UserName,'') EmpCategoryName
                                            ,ISNULL(ISNULL(LocLangLD.Name,''),ISNULL(LD.UserName,'')) LegalDesignationLocal
                                            ,ISNULL(LD.Sequence,0) DesignationSequence
                                            ,ISNULL(Line.Sequence,0) LineSequence,ISNULL(F.Sequence,0) PlantSequence,ISNULL(FU.Sequence,0) UnitSequence,
                                            ISNULL(DV.Sequence,0) DivisionSequence,ISNULL(subDV.Sequence,0) SubDivisionSequence,ISNULL(S.Sequence,0) SectionSequence,ISNULL(SS.Sequence,0) SubSectionSequence
                                            ,ISNULL(DP.Sequence,0) DepartmentSequence,ISNULL(EN.UserName,'') EntitySequence
                                            , ISNULL(LSalGr.Code,'') GradeCode,ISNULL(spld.PaymentMode,'') PaymentMode,ISNULL(line.Id,'') LineId
											 FROM SalaryProcChild SPC
														INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM." + salaryProcessSystemId + @" AND SPC.DisbusmentAmount !=0 AND SPC.DisbusmentAmount IS NOT NULL---new add for null head
 INNER JOin SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId
											     LEFT  JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON MB.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] DP ON DP.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] DV ON DV.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] F ON F.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] S ON S.Id = PO.SectionId
     								LEFT JOIN HKP.LocalLanguage SecLocLang ON SecLocLang.SectionId = S.Id and SecLocLang.LanguageId = F.LanguageId
                                    LEFT JOIN [ORG].[SubSection] SS ON SS.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[SubDivision] subDV ON subDV.Id = PO.SubDivisionId
                                    LEFT JOIN [ORG].[Unit] FU ON FU.Id = EN.UnitId
	                                            LEFT JOIN ORG.line line ON line.Id=MB.LineId
												LEFT JOIN HKP.LocalLanguage lineLang ON lineLang.lineID = MB.lineid and lineLang.LanguageId = F.LanguageId---
												LEFT JOIN HKP.EmployeeCategory EC ON SPLD.EmployeeCategoryId = EC.Id
                                              LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId
                                              LEFT JOIN HKP.LegalDesignation LD ON LD.Id = SPLD.LegalDesignationId
                                            LEFT JOIN hkp.LocalLanguage LocLangLD ON LocLangLD.LegalDesignationId = SPLD.LegalDesignationId and LocLangLD.LanguageId =F.LanguageId


--ctc starts
INNER join (select SlrProcMstSystemID,EmpInfoSystemID,SUM(DisbusmentAmount) CTC 
FROM SalaryProcChild 
left join SalaryHead h on h.SalaryHeadID=SalaryProcChild.SalaryHeadID
---where h.IsCTCComponent=1 and IsNetPayEffect=1 
where h.HeadCategory='TOTAL GROSS' 
group by SlrProcMstSystemID,EmpInfoSystemID
) ctc on ctc.SlrProcMstSystemID=SPM.SystemID and ctc.EmpInfoSystemID=spc.EmpInfoSystemID
--ctc ends
--OT Amount starts
INNER join (select SlrProcMstSystemID,EmpInfoSystemID,SUM(DisbusmentAmount) OTAmount 
FROM SalaryProcChild 
left join SalaryHead h on h.SalaryHeadID=SalaryProcChild.SalaryHeadID
where h.HeadCategory='OverTime' 
group by SlrProcMstSystemID,EmpInfoSystemID
) OTAmount on OTAmount.SlrProcMstSystemID=SPM.SystemID and OTAmount.EmpInfoSystemID=spc.EmpInfoSystemID
--OT Amount ends
--deduct starts
INNER join (select SlrProcMstSystemID,EmpInfoSystemID,SUM(DisbusmentAmount) Deduct 
FROM SalaryProcChild 
left join SalaryHead h on h.SalaryHeadID=SalaryProcChild.SalaryHeadID
where h.HeadType='D' --and IsNetPayEffect=1
group by SlrProcMstSystemID,EmpInfoSystemID
) de on de.SlrProcMstSystemID=spm.SystemID and de.EmpInfoSystemID=spc.EmpInfoSystemID
--deduct ends
--Gross starts
INNER join (select SlrProcMstSystemID,EmpInfoSystemID,EntryAmount Gross,DisbusmentAmount DisbusmentGross,h.SalaryHead GrossLable
			,BSH.Name GrossLableLocal
FROM SalaryProcChild 
left join SalaryHead h on h.SalaryHeadID=SalaryProcChild.SalaryHeadID
LEFT JOIN org.Plant F ON SalaryProcChild.PlantID = F.Id----add
		LEFT JOIN (
		SELECT *
		FROM HKP.LocalLanguage
		WHERE SalaryHeadId IS NOT NULL
		) AS BSH ON BSH.SalaryHeadId = h.SalaryHeadID and BSH.LanguageId=" + sqlL + @"
WHERE h.HeadCategory='GROSS' 
) deg on deg.SlrProcMstSystemID=spm.SystemID and deg.EmpInfoSystemID=spc.EmpInfoSystemID
--gross ends
--OT Rate starts
	LEFT JOIN (
		SELECT SlrProcMstSystemID
			,EmpInfoSystemID
			,Convert(DECIMAL(18, 2), (Convert(DECIMAL(18, 2),ISNULL(EntryAmount, 0) ) * 2) / 208) OTRate
		FROM SalaryProcChild
		LEFT JOIN SalaryHead h ON h.SalaryHeadID = SalaryProcChild.SalaryHeadID
		WHERE h.HeadCategory = 'basic'
		) deot ON deot.SlrProcMstSystemID = spm.SystemID
		AND deot.EmpInfoSystemID = spc.EmpInfoSystemID
	--OT Rate ends
LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
LEFT JOIN SalaryProceAttdnData otstatus on otstatus.EmpSystemID=spc.EmpInfoSystemID and otstatus.SlrProcMstSystemID=spm.SystemID
LEFT JOIN (SELECT EmpSystemId,SUM(LunchOutHour) LunchOutHour FROM LunchOutHour WHERE WorkDate Between '" + para.FromDate + @"' and '" + para.ToDate + @"' GROUP BY EmpSystemId ) LunchOutHour on LunchOutHour.EmpSystemId=spc.EmpInfoSystemID 
LEFT JOIN org.Plant FP ON spc.PlantID = FP.Id----add
LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL) AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID and BSH.LanguageId=" + sqlL + @" --BanglaSalaryHead
														LEFT JOIN (select * from [MST].[PlantSalaryHeadSequence] where PlantId='" + para.PlantId + @"' ) psh
																		on psh.SalaryHeadId=spc.SalaryHeadID
														LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id
														LEFT JOIN (
																   SELECT * FROM ExchangerateDateWiseForHR
																   WHERE FromDate IN (
																					  SELECT MAX(FromDate) FromDate FROM ExchangerateDateWiseForHR
																						WHERE FromDate <= (
																										   SELECT MAX(FromDate) FromDate FROM SalaryProcMaster
																											WHERE  " + salaryProcessSystemId + @"
                                                                                                           )
                                                                                    )
																  ) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode
																							AND SPC.PlantID = Exr.PlantID
														LEFT JOIN scs.Currency CRE ON EXR.FromCurrencyCode = CRE.Id
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID
                                left join (
										
												  select EmpSystemId,MAX(EffectiveDate) EffectiveDate, VoluntaryPFValue from [PFEmployeeVoluntaryValue] 
												  Where EffectiveDate<='" + para.FromDate + @"'
												  Group By EmpSystemId,EffectiveDate,VoluntaryPFValue
										) VPF on EmpBasic.SystemId=vpf.EmpSystemId
										left join(
										SELECT SH.HeadCategory ,SH.SalaryHeadID,SH.SalaryHead,SH.IsCTCComponent
										,SH.IsGrossComponent,SH.HeadType,sr.SalaryRuleMasterSystemID
											  FROM [dbo].[SalaryRulePF] SR
											  left join SalaryHead SH ON SH.SalaryHeadID=SR.SalaryHeadID
											  where SH.HeadCategory='PF Voluntary'
										) VPFhead on VPFhead.SalaryRuleMasterSystemID=EmpBasic.SalaryRuleMasterSystemID
                                        left join
										(
										select m.EmpSystemID,m.MonthNo,d.Value Bonus 
												--,SH.HeadCategory 
                                                ,SH.SalaryHeadID,SH.SalaryHead,SH.IsCTCComponent
												,SH.IsGrossComponent,SH.HeadType
												from [BonusPolicyMonthlyRetainEmpWiseCalculation] m
												left join [BonusPolicyMonthlyRetainDistributionPmt] d on m.id=d.BnsPlyMntRetainID
												left join SalaryHead SH ON SH.SalaryHeadID=d.SalaryHeadID
												where m.MonthNo=DATEPART(Month,'" + para.FromDate + @"')  AND m.YearNo=DATEPART(YEAR,'" + para.FromDate + @"')  
                                                --and d.SalaryHeadID=(select SalaryHeadID from SalaryHead where HeadCategory='Festival Bonus')
										) BonusValue on EmpBasic.SystemId=BonusValue.EmpSystemID
                                    LEFT JOIN
		                                    (
											 SELECT MMDSA.EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,
													TotalAbsent AbsentDays, TotalLv,TotalLWP, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
													TotalWeekOffHoliDay, ISNULL(OT.TotalOTHr,0) TotalOTHr, TotalNormalOTHr, TotalExtraOTHr,SlrProcMstSystemID
				                              FROM SalaryProceAttdnData MMDSA                                                
											  LEFT JOIN 
											(SELECT SUM(TOT) TotalOTHr,EmpSystemID,PlantID FROM (
                                            SELECT 
                                            CASE " + OTCase + @"
                            WHEN FOT.TotalOTHr > CAS.MaxOTPerDay then CAS.MaxOTPerDay                                             
                                                else FOT.TotalOTHr end TOT
                                            ,FOT.EmpSystemID,FOT.WorkDate,FOT.PlantID,FOT.TotalOTHr
                                             FROM FinalOT FOT LEFT JOIN 
                                            ComplianceAttendanceSetting CAS ON CAS.CompanyGroupId = FOT.GroupID  AND CAS.PlantID = '" + para.PlantId + @"'
	                                        LEFT JOIN AttdnProcessData APD  ON APD.WorkDate = FOT.WorkDate and apd.EmpSystemID = FOT.EmpSystemID
											LEFT JOIN DayType DT  ON DT.DayType = APD.DayStatus 
                                            WHERE " + wcBasedOnSetting + @"
                                            ) dd
                                            WHERE WorkDate BETWEEN '" + para.FromDate + @"' and '" + para.ToDate + @"' and PlantID = '" + para.PlantId + @"'
                                            GROUP BY EmpSystemID,PlantID ) OT ON OT.EmpSystemID = MMDSA.EmpSystemID
                                            WHERE MMDSA.MonthNo = MONTH('" + para.ToDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + para.ToDate + @"')

											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID and   EmpSlr.SlrProcMstSystemID=MMDSA.SlrProcMstSystemID
                                                      
                                    LEFT JOIN 
                                           		(
                                           		 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
                                           			FROM EmployeeInformation E   
                                           					LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
                                           					LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                               AND E.PlantId = gd.PlantId
                                           					LEFT JOIN (
                                           								SELECT MAX(EffectiveDate)EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
                                           									FROM MST.LegalSalaryStructure 
                                           									WHERE EffectiveDate <= GETDATE()
                                           								GROUP BY LegalSalaryGradeId, EmployeeLocationId 
                                           							  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
                                           					LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                           AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                           AND SS.EffectiveDate = S.EffectiveDate
                                           					LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 																							
                                           					left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId
                                           			GROUP BY E.SystemId,LSG.UserName 
                                           		) MW ON MW.SystemId = EmpBasic.SystemId

                                                left join (
                                                select bb.UserName BankName,b.BankAccNo,b.EmpSystemID from [dbo].[EmployeeBankInfo] b
                                                left join hkp.BankBranch bb on b.BankBranchId=bb.Id
                                                ) BB ON BB.EmpSystemID = EmpBasic.SystemId

                                                left join
												( select ed.DocNumber UANNo,ED.EmpSystemID from 
												EmployeeDocument ED 
												inner JOIN (select * from HKP.ComplianceDocument where ProfileType = 'PF') CD ON CD.Id = ED.ComplianceDocumentId 
												) pf on EmpBasic.SystemId = pf.EmpSystemID

												left join
												( select ed.DocNumber ESICNo,ED.EmpSystemID from 
												EmployeeDocument ED 
												INNER JOIN (SELECT * FROM HKP.ComplianceDocument where ProfileType = 'ESIC') CD ON CD.Id = ED.ComplianceDocumentId 
												) ESIC on EmpBasic.SystemId = ESIC.EmpSystemID";

                if (parameters.Count > 0)
                {
                    if (parameters.Keys.ElementAt(0) != "")
                    {
                        strSQL += @" Where EmpBasic.SystemId IN(" + parameters["EmpSystemId"] + ")";
                    }
                }

                strSQL = strSQL + @" " + sortingParameters + "";
                ConnectionManager.clsConnectionManager objConss = new clsConnectionManager(3600);
                objConss.BeginTransaction();
                objConss.getDataSet(strSQL, out dsRef);
                objConss.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        #endregion -- Operations
    }
    public class PayRegisterSignatoryField
    {
        public int Id { get; set; }
        public string PlantId { get; set; }
        public string Sequence { get; set; }
        public string FieldName { get; set; }
    }
}