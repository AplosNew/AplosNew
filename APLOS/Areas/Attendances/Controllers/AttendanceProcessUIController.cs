using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using Library.HumanResource.Report.Attendance;
namespace Aplos.Areas.Attendances.Controllers
{
    public class AttendanceProcessUIController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMonthlyAttendanceInformation _monthlyAttendanceInformation;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private DataSet dsRef;
        private object workbook;
        private object objRpt;
        private object excelEngine;
        private object application;

        public AttendanceProcessUIController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository,
            IMonthlyAttendanceInformation monthlyAttendanceInformation
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _monthlyAttendanceInformation = monthlyAttendanceInformation;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult MonthlyAttendanceInformation()
        {
            return View();
        }
        public ActionResult MonthlyAttendanceInformationDateRange()
        {
            return View();
        }
        public ActionResult MonthlyAttendanceInformationALLStatus()
        {
            return View();
        }

        public ActionResult OTFinalInformation()
        {
            return View();
        }
        #endregion -- Pages


        #region -- Operations
        [HttpGet, Authorize]
        public ActionResult XlsDepWiseAttnRptView(string Month, string Year, string DayStatus, bool withColor, string[] empParameters, bool includeCurrentDate, bool isActive, bool isSeperated, bool isMaternity)
        {

            //var paramValues = HttpContext.Current.Request.Params.GetValues("listOfIds");
            Dictionary<string, string> empParameters1 = new Dictionary<string, string>();
            if (empParameters.Length > 0)
            {
                if (!string.IsNullOrEmpty(empParameters[0].ToString()))
                {
                    empParameters1.Add("EmpSystemId", empParameters[0].ToString());
                }

            }


            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //var fileName = "MonthlyAttdnInfo" + DateTime.Now.ToString("yyMMdd") + ".xls";
                //string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                var workbook = _monthlyAttendanceInformation.XlsMonthlyAttendanceSummaryReport(identity.CompanyId, identity.PlantId, Month, Year, identity.Name, DayStatus, empParameters1, withColor, includeCurrentDate, false, isActive, isSeperated, isMaternity);



                return RenderReportAsPdf(workbook, "MonthlyAttdnInfo");

                //return RedirectToAction("GetData", "AttendanceProcessUI", workbook);
            }

            catch (Exception ex)
            {
                throw ex;
            }

        }





        [HttpPost, Authorize]
        public ActionResult XXlsDepWiseAttnRpt(string Month, string Year, string DayStatus, Dictionary<string, string> empParameters, bool withColor, bool includeCurrentDate, bool withSummary, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "MonthlyAttdnInfo" + DateTime.Now.ToString("yyMMdd") + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                var workbook = _monthlyAttendanceInformation.XlsMonthlyAttendanceSummaryReport(identity.CompanyId, identity.PlantId, Month, Year, identity.Name, DayStatus, empParameters, withColor, includeCurrentDate, withSummary, isActive, isSeperated, isMaternity);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);
                return Json(new { FileName = fullPath, Error = false }, JsonRequestBehavior.AllowGet);
                //workbook.SaveAs(fileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                //workbook.Close();
                

            }

            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost, Authorize]
        public ActionResult XlsDepWiseAttnRpt(string Month, string Year, string DayStatus, Dictionary<string, string> empParameters, bool withColor, bool includeCurrentDate, bool withSummary, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "MonthlyAttdnInfo" + DateTime.Now.ToString("yyMMdd")+identity.UserId + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                var workbook = _monthlyAttendanceInformation.XlsMonthlyAttendanceSummaryReports(identity.CompanyId, identity.PlantId, Month, Year, identity.Name, DayStatus, empParameters, withColor, includeCurrentDate, withSummary, isActive, isSeperated, isMaternity);

                return Json(new { FullPath = workbook, FileName= fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }

            catch (Exception ex)
            {
                throw ex;
            }

        }


        [HttpPost, Authorize]
        public ActionResult XlsDepWiseAttnRptDateRange(string FromDate, string ToDate, string DayStatus, string employeeStatus, Dictionary<string, string> empParameters, bool withColor, bool includeCurrentDate, bool withSummary, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsMonthlyAttendanceInformation clsMonthlyAttendanceInformation = new clsMonthlyAttendanceInformation();
                var fileName = "MonthlyAttdnInfo" + DateTime.Now.ToString("yyMMdd") + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;
                var workbook = clsMonthlyAttendanceInformation.XlsMonthlyAttendanceSummaryReportDateRange(identity.CompanyId, identity.PlantId, FromDate, ToDate, identity.Name, DayStatus, empParameters, withColor, includeCurrentDate, withSummary, isActive, isSeperated, isMaternity);

                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }

            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost, Authorize]
        public ActionResult XlsOTFinalReport(string Month, string Year)
        {
            #region Variable
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsReport objRpt = null;
            DataSet dsOT = null;
            DataTable dtOT = null;
            DataView dvOT = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            string FactoryName = "";
            string CmpName = "";
            string companyId = identity.CompanyId;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            DateTime dtFrmDt = DateTime.Now;
            DateTime dtEndDate = DateTime.Now;
            #endregion Variable

            try
            {
                objRpt = new clsReport();
                ReportUtility oru = new ReportUtility();

                #region Validation
                string m = bplib.clsWebLib.GetMonthName(Month);
                dtFrmDt = Convert.ToDateTime("01-" + m + "-" + Year);
                if (Convert.ToInt32(DateTime.Now.Month) != Convert.ToInt32(Month))
                {
                    dtEndDate = dtFrmDt.AddMonths(1).AddDays(-1);
                }
                #endregion Validation

                #region Variable
                string sUnit = "ALL";
                string sDevi = "ALL";
                string sDept = "ALL";
                string sSect = "ALL";
                string sSbSe = "ALL";
                string sLine = "ALL";
                string sEmpC = "ALL";
                string sDeGr = "ALL";
                string sDesi = "ALL";
                var otStartCol = 0;
                var otEndCol = 0;
                #endregion Variable

                #region DataSet

                objRpt.GetOTFinalRpt(identity.PlantId, dtFrmDt.ToString("dd-MMM-yyyy"), dtEndDate.ToString("dd-MMM-yyyy"), sUnit, sDevi, sDept, sSect, sSbSe, sLine, sEmpC, sDeGr, sDesi, out dsOT);
                dtOT = dsOT.Tables[0];
                dvOT = new DataView();
                dvOT.Table = dsOT.Tables[0];
                var ListOT = dsOT.Tables[0].ToList<OTReport>();
                DataView dvEmp = new DataView(dsOT.Tables[0]);
                DataTable dtEmp = dvEmp.ToTable(true, "EmployeeCode", "EmployeeName", "DOJ", "Unit", "Department", "Section", "Designation", "GivenDesignation", "LegalDG");
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                object chequeAmount;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 6;
                #region Variables
                int strCount = 0;
                int iSrNo = 0;
                int iEmpCode = 0;
                int iEmpName = 0;
                int iDOJ = 0;
                int iUnit = 0;
                int iDepart = 0;
                int iSec = 0;
                int iDesig = 0;
                int iTotal = 0;
                int iLine = 0;

                #endregion Variables

                #region ------------------Column Header------------------

                #region ------------------Details Header-----------------

                xlsRow += 1;

                xlsCol = 1;
                iSrNo = xlsCol;
                sheet1.Range[xlsRow, iSrNo].Text = "Sl No.";
                sheet1.Range[xlsRow, iSrNo].ColumnWidth = 6;
                sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iEmpCode = xlsCol;
                sheet1.Range[xlsRow, iEmpCode].Text = "Employee Code";
                sheet1.Range[xlsRow, iEmpCode].ColumnWidth = 10;
                sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iEmpName = xlsCol;
                sheet1.Range[xlsRow, iEmpName].Text = "Employee Name";
                sheet1.Range[xlsRow, iEmpName].ColumnWidth = 22;
                sheet1.Range[xlsRow, iEmpName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 9.20;
                sheet1.Range[xlsRow, iDOJ].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iUnit = xlsCol;
                sheet1.Range[xlsRow, iUnit].Text = "Unit";
                sheet1.Range[xlsRow, iUnit].ColumnWidth = 9;
                sheet1.Range[xlsRow, iUnit].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iDepart = xlsCol;
                sheet1.Range[xlsRow, iDepart].Text = "Department";
                sheet1.Range[xlsRow, iDepart].ColumnWidth = 15;
                sheet1.Range[xlsRow, iDepart].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iDepart].VerticalAlignment = ExcelVAlign.VAlignCenter;
                xlsCol += 1;
                iSec = xlsCol;
                sheet1.Range[xlsRow, iSec].Text = "Section";
                sheet1.Range[xlsRow, iSec].ColumnWidth = 15;
                sheet1.Range[xlsRow, iSec].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsCol += 1;
                iDesig = xlsCol;
                sheet1.Range[xlsRow, iDesig].Text = "Designation";
                sheet1.Range[xlsRow, iDesig].ColumnWidth = 15;
                sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsCol = iDesig;
                while (dtFrmDt <= dtEndDate)
                {
                    xlsCol += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = dtFrmDt.ToString("dd");
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 5;
                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    dtFrmDt = dtFrmDt.AddDays(1);
                }
                xlsCol += 1;
                iTotal = xlsCol;
                sheet1.Range[xlsRow, iTotal].Text = "Total";
                sheet1.Range[xlsRow, iTotal].ColumnWidth = 15;
                sheet1.Range[xlsRow, iTotal].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iTotal].VerticalAlignment = ExcelVAlign.VAlignCenter;

                #endregion ------------------Details Header-----------------

                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                endXlsCol = xlsCol;
                xlsCol = 1;
                xlsRow += 1;

                #endregion ------------------Column Header------------------

                for (int i = 0; i <= dtEmp.Rows.Count - 1; i++)
                {
                    xlsCol = 1;

                    #region ----------------------Data-----------------------
                    chequeAmount = dsOT.Tables[0].Compute(@"Sum(TotalOTHr)", "EmployeeCode ='" + dtEmp.Rows[i]["EmployeeCode"].ToString().Trim() + "'");
                    strCount += 1;
                    sheet1.Range[xlsRow, iSrNo].Number = strCount;
                    sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iEmpCode].Text = dtEmp.Rows[i]["EmployeeCode"].ToString().Trim();
                    sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iEmpName].Text = dtEmp.Rows[i]["EmployeeName"].ToString().Trim();
                    sheet1.Range[xlsRow, iEmpName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iDOJ].Text = dtEmp.Rows[i]["DOJ"].ToString().Trim();
                    sheet1.Range[xlsRow, iDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iUnit].Text = dtEmp.Rows[i]["Unit"].ToString().Trim();
                    sheet1.Range[xlsRow, iUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iDepart].Text = dtEmp.Rows[i]["Department"].ToString().Trim();
                    sheet1.Range[xlsRow, iDepart].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iDepart].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, iSec].Text = dtEmp.Rows[i]["Section"].ToString().Trim();
                    sheet1.Range[xlsRow, iSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iSec].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    sheet1.Range[xlsRow, iDesig].Text = dtEmp.Rows[i]["LegalDG"].ToString().Trim();
                    sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    dtFrmDt = Convert.ToDateTime(Month + "/" + "01/" + Year);
                    string _m = bplib.clsWebLib.GetMonthName(Month);
                    dtFrmDt = Convert.ToDateTime("01-" + _m + "-" + Year);
                    xlsCol = iDesig;
                    otStartCol = iDesig + 1;
                    while (dtFrmDt <= dtEndDate)
                    {
                        xlsCol += 1;
                        string ecode = dtEmp.Rows[i]["EmployeeCode"].ToString().Trim();
                        var _ddd = Convert.ToDateTime(dtFrmDt.ToString("dd-MMM-yyyy"));
                        var _data = ListOT.Where(r => r.EmployeeCode == ecode && r.workdate == _ddd).FirstOrDefault();
                        if (_data != null)
                        {
                            string yot = string.Empty;//OTConsiderOn
                            oru.GetOT(dsOT.Tables[0].Rows[0]["OTConsiderOn"].ToString(), _data.TotalOTHr.ToString(), out yot);
                            if (string.IsNullOrEmpty(dsOT.Tables[0].Rows[0]["OTConsiderOn"].ToString()))
                            {

                                sheet1.Range[xlsRow, xlsCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Red;
                            }
                            sheet1.Range[xlsRow, xlsCol].Text = yot.ToString();
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        dtFrmDt = dtFrmDt.AddDays(1);
                    }
                    otEndCol = xlsCol;
                    string jot = string.Empty;//OTConsiderOn
                    oru.GetOT(dsOT.Tables[0].Rows[0]["OTConsiderOn"].ToString(), chequeAmount.ToString(), out jot);
                    var tt = jot;

                    sheet1.Range[xlsRow, otEndCol + 1].Text = jot;
                    sheet1.Range[xlsRow, otEndCol + 1].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                    sheet1.Range[xlsRow, otEndCol + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    xlsRow += 1;

                    #endregion ----------------------Data-----------------------

                    #region Line Setup

                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].WrapText = true;

                    #endregion Line Setup
                }

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region ******************Report Header******************
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                    Image companyLogo = Image.FromFile(strPath);
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);


                    }


                }
                catch (Exception)
                {


                }

                xlsRow = 1;
                xlsCol = 1;

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 26;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "OTFinal Information";
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Report Ref  No.";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 9;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Year No:- " + Year + " and Month No:- " + Month;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 9;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A8"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region Page Setup

                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "OTFinal Information";

                #endregion Page Setup

                workbook.Version = ExcelVersion.Excel97to2003;
                var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "OTFinalInformation.xls";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
            }
        }
        List<SwapColumn> GetColDisplayName(DataSet dslocal)
        {
            List<SwapColumn> list = null;
            try
            {
                list = new List<SwapColumn>();
                for (int i = 0; i < dslocal.Tables[0].Columns.Count; i++)
                {
                    var c = dslocal.Tables[0].Columns[i].ColumnName;
                    if (c.ToUpper() != "EMPLOYEEPK")
                    {
                        string _date = Convert.ToDateTime(c).ToString("dd-MMM-yyyy");
                        string _day = Convert.ToDateTime(c).ToString("dd");
                        SwapColumn ob = new SwapColumn();
                        ob.DisplayMember = _date;
                        ob.ValueMember = _day;
                        list.Add(ob);
                    }//if
                }
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeInformation(string EffectiveDate, string criteria)
        {
            string sql = string.Empty;
            try
            {
                EffectiveDate = DateTime.Now.ToString();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsShiftInfo ob = new clsShiftInfo(_sqlRepository);
                var data = ob.GetEmpInfo(identity.CompanyGroupId, identity.PlantId, EffectiveDate, criteria);

                return Json(new { EmpInfo = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Process(string pFromDate, string pToDate, Dictionary<string, string> EmpList, bool CheckBox)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                string EmpIdLoop = "";
                if (EmpList.Count > 0)
                {
                    if (EmpList.Keys.ElementAt(0) != "")
                    {
                        EmpIdLoop += EmpList["EmpSystemId"];
                    }
                }
                //foreach (string item in EmpList)
                //{
                //    if (EmpIdLoop == "")
                //    {
                //        EmpIdLoop = "'" + item + "'"; ;
                //    }
                //    else
                //    {
                //        EmpIdLoop += ",'" + item + "'";

                //    }
                //}

                DateTime FromDateV = Convert.ToDateTime(pFromDate);
                DateTime ToDateV = Convert.ToDateTime(pToDate);
                while (FromDateV <= ToDateV)
                {
                    if (EmpIdLoop.Length > 0)
                    {
                        obj.LockValidation(identity.PlantId, FromDateV.ToString("dd-MMM-yyyy"), ToDateV.ToString("dd-MMM-yyyy"), EmpIdLoop);
                    }

                    FromDateV = FromDateV.AddDays(1);
                }



                if (CheckBox == true)
                {
                    string sql = @"update EmpDateWiseShiftAssign set ToReprocess='Yes' where WorkDate between '" + pFromDate + "' and '" + pToDate + "' and EmpSystemID in (" + EmpIdLoop + @")";
                    ExecuteRawSQL(sql);

                    string sql2 = @"delete FROM AttdnProcessData  where WorkDate between '" + pFromDate + "' and '" + pToDate + "' and EmpSystemID in (" + EmpIdLoop + @")";
                    ExecuteRawSQL(sql2);
                }

                #region Attendance process

                DateTime FromDate = Convert.ToDateTime(pFromDate);
                DateTime ToDate = Convert.ToDateTime(pToDate);
                while (FromDate <= ToDate)
                {
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    ReturnType r = obj.SaveTotal(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), EmpIdLoop, false);//laila                 
                    FromDate = FromDate.AddDays(1);
                }






            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion
            return Json(new { Message = "Process completed!!!" }, JsonRequestBehavior.AllowGet);
        }

        public void ExecuteRawSQL(string sql1)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(sql1, true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End Function

        [HttpGet, Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT Id,YearNo FROM YearlyCalendar WHERE PlantId='" + identity.PlantId + "'  ORDER BY YearNo DESC ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        class OTReport
        {
            public decimal TotalOTHr { get; set; }
            public string EmployeeCode { get; set; }
            public DateTime workdate { get; set; }

        }

        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity,string PlantId)
        {
            string Plant = string.Empty;
            
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (PlantId == "" || PlantId == null)
            {
                Plant = identity.PlantId;
            }
            else
            {
                Plant = "'" + PlantId.Replace(",", "','") + "'";//replaced with ""
            }

            var month = Convert.ToDateTime(effectiveDate).AddMonths(1);
            var Ld = month.AddDays(-1);
            var wcPayrollGroup = "";
            var wcSalaryProcess = "";
            var salaryProcessJoin = "";
            var salaryProcessColumn = "";
            var strDOJ = "";
            string param = "";
            string salaryProcessFlag = "";
            string wcEmpStatus = "";
            wcEmpStatus = " Where (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Separated'";
                }

            }
            wcEmpStatus += ")";

            param = "E.GroupID='" + identity.CompanyGroupId + "' AND E.CompanyId='" + identity.CompanyId + "' AND E.PlantId in (" + Plant + ")";

            var cmdText = @"SELECT * fROM (  SELECT   dISTINCT        [CheckBoxSelect] = Convert(bit, 'False'),
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId     
                                    ,ISNULL(e.EmployeeCurrentStatus,'') EmployeeCurrentStatus	
                                    ,isnull(ld.UserName,'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(Line.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Regular' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    
                                    
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                    FROM EmployeeInformation e
                                  
                                    
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                          
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
									AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    LEFT JOIN [ORG].[Line] ON Line.Id = mpb.LineId
LEFT OUTER JOIN HKP.Designation edsg on edsg.id=PO.DesignationID
                                    
                                    
						LEFT JOIN [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
			LEFT JOIN [MST].DesignationMasterLegalDesignation LDM ON LDM.LegalDesignationId=E.LegalDesignationId
			LEFT JOIN [MST].DesignationMaster DesM ON DesM.Id = LDM.DesignationMasterId
LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=DesM.DesignationGroupId
            LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    " + salaryProcessJoin + @"
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = ebi.BankSystemID

                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"  " + wcSalaryProcess + @"  
                                                    
                                        AND
									(E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + effectiveDate + @"')
                                    AND e.DOJ <= '" + Ld.ToString("dd-MMM-yyyy") + @"'
                                    ) 
                                     ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";


            var jsondata = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

      

        [HttpPost, Authorize]
        public ActionResult GetEmpInfoDateRang(string fromDate, string toDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //var month = Convert.ToDateTime(toDate).AddMonths(1);
            //var Ld = month.AddDays(-1);
            var wcPayrollGroup = "";
            var wcSalaryProcess = "";
            var salaryProcessJoin = "";
            var salaryProcessColumn = "";
            var strDOJ = "";
            string param = "";
            string salaryProcessFlag = "";
            string wcEmpStatus = "";
            wcEmpStatus = " Where (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Separated'";
                }

            }
            wcEmpStatus += ")";

            param = "E.GroupID='" + identity.CompanyGroupId + "' AND E.CompanyId='" + identity.CompanyId + "' AND E.PlantId='" + identity.PlantId + "'";

            var cmdText = @"SELECT * fROM (  SELECT   dISTINCT        [CheckBoxSelect] = Convert(bit, 'False'),
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId     
                                    ,ISNULL(e.EmployeeCurrentStatus,'') EmployeeCurrentStatus	
                                    ,isnull(ld.UserName,'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(Line.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + fromDate + @"')  AND YEAR(DOS) = YEAR('" + fromDate + @"') then 'Separated' else 'Regular' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    
                                    
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                    FROM EmployeeInformation e
                                  
                                    
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    LEFT JOIN [ORG].[Line] ON Line.Id = mpb.LineId

                                    
						LEFT JOIN [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
			LEFT JOIN [MST].DesignationMasterLegalDesignation LDM ON LDM.LegalDesignationId=E.LegalDesignationId
			LEFT JOIN [MST].DesignationMaster DesM ON DesM.Id = LDM.DesignationMasterId
            LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    " + salaryProcessJoin + @"
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = ebi.BankSystemID

                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"  " + wcSalaryProcess + @"  
                                                    
                                        AND
									(E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + fromDate + @"')
                                    and e.DOJ <= '" + toDate + @"'
                                    ) 
                                     ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";


            var jsondata = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
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

        #endregion -- Operations  
    }

    public class EarlyOutStatus
    {
        public int NOP { get; set; }
        public int VLD { get; set; }
        public int LO { get; set; }
        public int NOO { get; set; }
        public int MO { get; set; }
    }
}