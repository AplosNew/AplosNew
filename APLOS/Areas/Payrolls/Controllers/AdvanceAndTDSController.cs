using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;
using static Library.Service.HumanResources.PayRegisterBDReportService;
using static OTSBD.clsSalary.clsSalaryReport;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class AdvanceAndTDSController : BaseController
    {
        #region Constructor

        private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        private string monthName;

        public AdvanceAndTDSController(
              IPayRegisterBDReportService payRegisterBDReportService, IEmployeeProfileService employeeProfileService,
              ISqlRepository sqlRepository
            )
        {
            _payRegisterBDReportService = payRegisterBDReportService;
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
        public ActionResult GetAdvanceAndTDSReports(string month, string year, bool isActive, bool isSummary)
        {
            #region Variable

            clsReport objRpt = null;

            DataSet dsSlrProc = null;
            DataView dvSlrProc = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            clsStaticInfo objs = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";

            #endregion Variable

            try
            {
                objRpt = new clsReport();
                objs = new OTSBD.clsStaticInfo();
                #region Variable

                ParamList para = new ParamList();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                para.PlantId = identity.PlantId;
                para.EmployeeId = identity.EmployeeId;

                para.FromDate = "01-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var ToDate = daysInMonth + "-" + monthName + "-" + year;
                para.ToDate = ToDate;
                para.CompanyGroupId = identity.CompanyGroupId;
                para.CompanyId = identity.CompanyId;
                #endregion Variable
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");
                #region DataSet
                GetAdvanceAndTDS(para, out dsSlrProc);//Sql Query
                dvSlrProc = new DataView();
                dvSlrProc.Table = dsSlrProc.Tables[0];
                DataView dvEmp = new DataView();
                DataView dvEmpAdvTvs = new DataView();
                dvEmp.Table = dsSlrProc.Tables[0];
                DataTable dtEmployees = dvEmp.ToTable(true, "SystemId", "EmployeeName", "EmployeeCode", "TaxAmount", "AdvanceAmount", "GrossAmount", "DocNumber");

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 5;
                xlsCol = 1;

                int ColSr = 0;
                int ColIDNo = 0;
                int ColName = 0;
                int ColTDS = 0;
                int ColAD = 0;
                int ColL = 0;
                int ColLN = 0;
                int ColB = 0;
                int ColMB = 0;
                int colTotal = 0;
                int totalAmount = 0;
                int totalAdvAmount = 0;
                int totalGrossAmount = 0;
                int ColDoc = 0;
                int ColGROSS = 0;
                int ColPdDy = xlsCol;
                int ColAbDy = xlsCol;
                int ColHlDy = xlsCol;
                int ColWkOf = xlsCol;
                int ColLv = xlsCol;

                SetHeadText("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr, 6);
                SetHeadText("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 8);
                SetHeadText("Name", sheet1, xlsRow, ref xlsCol, out ColName, 22);
                SetHeadText("PAN No.", sheet1, xlsRow, ref xlsCol, out ColDoc, 22);
                SetHeadText("GROSS", sheet1, xlsRow, ref xlsCol, out ColGROSS, 17);
                SetHeadText("TDS", sheet1, xlsRow, ref xlsCol, out ColTDS, 17);
                SetHeadText("ADVANCE", sheet1, xlsRow, ref xlsCol, out ColAD, 17);

                if (isActive == true)
                {

                    SetHeadText("LOAN", sheet1, xlsRow, ref xlsCol, out ColL, 17);
                    SetHeadText("LOAN.INT", sheet1, xlsRow, ref xlsCol, out ColLN, 17);
                    SetHeadText("BUS", sheet1, xlsRow, ref xlsCol, out ColB, 17);
                    SetHeadText("MOBILE", sheet1, xlsRow, ref xlsCol, out ColMB, 17);
                }
                if (isActive == false)
                {
                    using (var dvEmpInfo = new DataView(dtEmployees)
                    {
                        RowFilter = "AdvanceAmount >0 OR  TaxAmount > 0",

                    })
                    {
                        dtEmployees = dvEmpInfo.ToTable();
                    }
                }

                endXlsCol = xlsCol;
                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region *****************Report Header*****************
                xlsRow = 1;
                xlsCol = 1;
                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    //CmpName = dsCmp.Tables[0].Rows[0]["UserName"].ToString();
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                sheet1.Range[xlsRow, xlsCol].Text = "DEDUCTION STATEMENT FOR THE MONTH OF : " + bplib.clsWebLib.GetMonthName(month) + ", " + year + " ";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion *****************Report Header*****************

                #region ----------------------Data-----------------------

                int SrNo = 0;
                string x = "";
                ReportUtility oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                xlsRow--;
                var formulaStartRow = xlsRow;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region empinfo col Data

                    sheet1.Range[xlsRow, ColSr].Number = (1 + SrNo);
                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    SetCellText(sheet1, xlsRow, ColIDNo, dtEmployees.Rows[i]["EmployeeCode"].ToString());
                    SetCellText(sheet1, xlsRow, ColName, dtEmployees.Rows[i]["EmployeeName"].ToString());
                    SetCellText(sheet1, xlsRow, ColDoc, dtEmployees.Rows[i]["DocNumber"].ToString());

                    var tax = Convert.ToDouble(dtEmployees.Rows[i]["TaxAmount"].ToString());
                    SetCellText(sheet1, xlsRow, ColTDS, tax);
                    sheet1.Range[xlsRow, ColTDS].NumberFormat = oRU.NumberFormatDecimalZero();

                    totalAmount = ColTDS;

                    var adv = Convert.ToDouble(dtEmployees.Rows[i]["AdvanceAmount"].ToString());
                    SetCellText(sheet1, xlsRow, ColAD, adv);
                    sheet1.Range[xlsRow, ColAD].NumberFormat = oRU.NumberFormatDecimalZero();

                    totalAdvAmount = ColAD;

                    var gross = Convert.ToDouble(dtEmployees.Rows[i]["GrossAmount"].ToString());
                    SetCellText(sheet1, xlsRow, ColGROSS, gross);
                    sheet1.Range[xlsRow, ColGROSS].NumberFormat = oRU.NumberFormatDecimalZero();

                    totalGrossAmount = ColGROSS;
                    if (isActive == true)
                    {
                        SetCellText(sheet1, xlsRow, ColL, "");
                        SetCellText(sheet1, xlsRow, ColLN, "");
                        SetCellText(sheet1, xlsRow, ColB, "");
                        SetCellText(sheet1, xlsRow, ColMB, "");
                    }

                    SrNo += 1;
                    #endregion
                    x = dtEmployees.Rows[i]["SystemId"].ToString().Trim().ToUpper();

                    xlsRow++;
                }

                if (dtEmployees.Rows.Count > 0)
                {
                    var summationRowLimit = xlsRow - 1;
                    colTotal = 3;
                    sheet1.Range[xlsRow, colTotal].Text = "Total";
                    sheet1.Range[xlsRow, colTotal].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, colTotal].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    sheet1.Range[xlsRow, totalAmount].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, totalAmount].Formula = "=SUM(" + oRU.GetColumnNameForXls(totalAmount) + formulaStartRow + ":" + oRU.GetColumnNameForXls(totalAmount) + (summationRowLimit) + ")";
                    sheet1.Range[xlsRow, totalAmount].NumberFormat = oRU.NumberFormatDecimalZero();

                    sheet1.Range[xlsRow, totalAdvAmount].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, totalAdvAmount].Formula = "=SUM(" + oRU.GetColumnNameForXls(totalAdvAmount) + formulaStartRow + ":" + oRU.GetColumnNameForXls(totalAdvAmount) + (summationRowLimit) + ")";
                    sheet1.Range[xlsRow, totalAdvAmount].NumberFormat = oRU.NumberFormatDecimalTwo();


                    sheet1.Range[xlsRow, totalGrossAmount].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, totalGrossAmount].Formula = "=SUM(" + oRU.GetColumnNameForXls(totalGrossAmount) + formulaStartRow + ":" + oRU.GetColumnNameForXls(totalGrossAmount) + (summationRowLimit) + ")";
                    sheet1.Range[xlsRow, totalGrossAmount].NumberFormat = oRU.NumberFormatDecimalTwo();

                    #endregion ----------------------Data-----------------------

                    #region Line Setup
                    if (RowIndex >= (xlsRow - 1))
                    {
                        xlsRow = RowIndex + 2;
                    }
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    #endregion
                }

                #region Freeze Panes
                sheet1.UsedRange["A6"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.Range["A3"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                //sheet1.PageSetup.PrintTitleRows = "$1:$7";
                sheet1.PageSetup.PrintTitleRows = "$A$5:$IV$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + (string)Session["USER"] + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Advance&TDS";
                #endregion

                workbook.Version = ExcelVersion.Excel2013;
                string fileName = monthName + "-" + year + "Advance And TDS" + DateTime.Now.ToString("yyMMdd") + ".xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetAdvanceAndTDSReportsSummary(string fromDate, string toDate, bool isSummary)
        {
            #region Variable

            clsReport objRpt = null;

            DataSet dsSlrProc = null;
            DataView dvSlrProc = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            clsStaticInfo objs = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";

            #endregion Variable

            try
            {
                objRpt = new clsReport();
                objs = new OTSBD.clsStaticInfo();
                #region Variable

                ParamList para = new ParamList();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                para.PlantId = identity.PlantId;
                para.EmployeeId = identity.EmployeeId;

                para.FromDate = fromDate;

                var ToDate = toDate;
                para.ToDate = ToDate;
                para.CompanyGroupId = identity.CompanyGroupId;
                para.CompanyId = identity.CompanyId;
                #endregion Variable
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");
                #region DataSet
                GetAdvanceAndTDSSummary(para, out dsSlrProc);
                dvSlrProc = new DataView();
                dvSlrProc.Table = dsSlrProc.Tables[0];
                DataView dvEmp = new DataView();
                DataView dvEmpAdvTvs = new DataView();
                dvEmp.Table = dsSlrProc.Tables[0];
                DataTable dtEmployees = dvEmp.ToTable(true, "EmpInfoSystemID", "EmployeeName", "EmployeeCode", "HeadCategoryT", "HeadCategoryA", "HeadCategoryG", "TaxAmount", "AdvanceAmount", "GrossAmount", "DocNumber");

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 5;
                xlsCol = 1;

                int ColSr = 0;
                int ColIDNo = 0;
                int ColName = 0;
                int ColTDS = 0;
                int ColAD = 0;
                int ColL = 0;
                int ColLN = 0;
                int ColB = 0;
                int ColMB = 0;
                int colTotal = 0;
                int totalAmount = 0;
                int totalAdvAmount = 0;
                int totalGrossAmount = 0;
                int ColDoc = 0;
                int ColGROSS = 0;
                int ColPdDy = xlsCol;
                int ColAbDy = xlsCol;
                int ColHlDy = xlsCol;
                int ColWkOf = xlsCol;
                int ColLv = xlsCol;

                SetHeadText("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr, 6);
                SetHeadText("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 8);
                SetHeadText("Name", sheet1, xlsRow, ref xlsCol, out ColName, 22);
                SetHeadText("PAN No.", sheet1, xlsRow, ref xlsCol, out ColDoc, 22);
                SetHeadText("GROSS", sheet1, xlsRow, ref xlsCol, out ColGROSS, 17);
                SetHeadText("TDS", sheet1, xlsRow, ref xlsCol, out ColTDS, 17);
                SetHeadText("ADVANCE", sheet1, xlsRow, ref xlsCol, out ColAD, 17);

                if (isSummary == true)
                {

                    SetHeadText("LOAN", sheet1, xlsRow, ref xlsCol, out ColL, 17);
                    SetHeadText("LOAN.INT", sheet1, xlsRow, ref xlsCol, out ColLN, 17);
                    SetHeadText("BUS", sheet1, xlsRow, ref xlsCol, out ColB, 17);
                    SetHeadText("MOBILE", sheet1, xlsRow, ref xlsCol, out ColMB, 17);
                }
                if (isSummary == false)
                {
                    using (var dvEmpInfo = new DataView(dtEmployees)
                    {
                        RowFilter = "AdvanceAmount >0 OR  TaxAmount > 0",

                    })
                    {
                        dtEmployees = dvEmpInfo.ToTable();
                    }
                }

                endXlsCol = xlsCol;
                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region *****************Report Header*****************
                xlsRow = 1;
                xlsCol = 1;
                string FactoryAddress = string.Empty;

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    //CmpName = dsCmp.Tables[0].Rows[0]["UserName"].ToString();
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                sheet1.Range[xlsRow, xlsCol].Text = "DEDUCTION STATEMENT From " + Convert.ToDateTime(fromDate).ToString("MMMM") + ", " + Convert.ToDateTime(fromDate).ToString("yyyy") + " To " + Convert.ToDateTime(toDate).ToString("MMMM") + ", " + Convert.ToDateTime(toDate).ToString("yyyy");//FOR THE MONTH OF : " + bplib.clsWebLib.GetMonthName(month) + ", " + year + " ";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion *****************Report Header*****************

                #region ----------------------Data-----------------------

                int SrNo = 0;
                string x = "";
                ReportUtility oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;
                xlsRow--;
                var formulaStartRow = xlsRow;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    #region empinfo col Data

                    sheet1.Range[xlsRow, ColSr].Number = (1 + SrNo);
                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    SetCellText(sheet1, xlsRow, ColIDNo, dtEmployees.Rows[i]["EmployeeCode"].ToString());
                    SetCellText(sheet1, xlsRow, ColName, dtEmployees.Rows[i]["EmployeeName"].ToString());

                    if (dtEmployees.Rows[i]["HeadCategoryT"].ToString().ToUpper() == "TAX")
                    {
                        SetCellText(sheet1, xlsRow, ColDoc, dtEmployees.Rows[i]["DocNumber"].ToString());
                    }

                    if (dtEmployees.Rows[i]["HeadCategoryT"].ToString().ToUpper() == "TAX")
                    {
                        var tax = Convert.ToDouble(dtEmployees.Rows[i]["TaxAmount"].ToString());

                        SetCellText(sheet1, xlsRow, ColTDS, tax);
                        sheet1.Range[xlsRow, ColTDS].NumberFormat = oRU.NumberFormatDecimalZero();
                    }
                    totalAmount = ColTDS;
                    if (dtEmployees.Rows[i]["HeadCategoryA"].ToString().ToUpper() == "ADVANCE")
                    {
                        var adv = Convert.ToDouble(dtEmployees.Rows[i]["AdvanceAmount"].ToString());
                        SetCellText(sheet1, xlsRow, ColAD, adv);
                        sheet1.Range[xlsRow, ColAD].NumberFormat = oRU.NumberFormatDecimalZero();

                    }
                    totalAdvAmount = ColAD;

                    if (dtEmployees.Rows[i]["HeadCategoryG"].ToString().ToUpper() == "GROSS")
                    {
                        var gross = Convert.ToDouble(dtEmployees.Rows[i]["GrossAmount"].ToString());
                        SetCellText(sheet1, xlsRow, ColGROSS, gross);
                        sheet1.Range[xlsRow, ColGROSS].NumberFormat = oRU.NumberFormatDecimalZero();

                    }
                    totalGrossAmount = ColGROSS;
                    if (isSummary == true)
                    {
                        SetCellText(sheet1, xlsRow, ColL, "");
                        SetCellText(sheet1, xlsRow, ColLN, "");
                        SetCellText(sheet1, xlsRow, ColB, "");
                        SetCellText(sheet1, xlsRow, ColMB, "");
                    }


                    SrNo += 1;
                    #endregion
                    x = dtEmployees.Rows[i]["EmpInfoSystemID"].ToString().Trim().ToUpper();

                    xlsRow++;
                }


                if (dtEmployees.Rows.Count > 0)
                {
                    var summationRowLimit = xlsRow - 1;
                    colTotal = 3;
                    sheet1.Range[xlsRow, colTotal].Text = "Total";
                    sheet1.Range[xlsRow, colTotal].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, colTotal].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                    sheet1.Range[xlsRow, totalAmount].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, totalAmount].Formula = "=SUM(" + oRU.GetColumnNameForXls(totalAmount) + formulaStartRow + ":" + oRU.GetColumnNameForXls(totalAmount) + (summationRowLimit) + ")";
                    sheet1.Range[xlsRow, totalAmount].NumberFormat = oRU.NumberFormatDecimalZero();

                    sheet1.Range[xlsRow, totalAdvAmount].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, totalAdvAmount].Formula = "=SUM(" + oRU.GetColumnNameForXls(totalAdvAmount) + formulaStartRow + ":" + oRU.GetColumnNameForXls(totalAdvAmount) + (summationRowLimit) + ")";
                    sheet1.Range[xlsRow, totalAdvAmount].NumberFormat = oRU.NumberFormatDecimalTwo();


                    sheet1.Range[xlsRow, totalGrossAmount].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, totalGrossAmount].Formula = "=SUM(" + oRU.GetColumnNameForXls(totalGrossAmount) + formulaStartRow + ":" + oRU.GetColumnNameForXls(totalGrossAmount) + (summationRowLimit) + ")";
                    sheet1.Range[xlsRow, totalGrossAmount].NumberFormat = oRU.NumberFormatDecimalTwo();

                    #endregion ----------------------Data-----------------------

                    #region Line Setup
                    if (RowIndex >= (xlsRow - 1))
                    {
                        xlsRow = RowIndex + 2;
                    }
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    #endregion
                }

                #region Freeze Panes
                sheet1.UsedRange["A6"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 8;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.Range["A3"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                //sheet1.PageSetup.PrintTitleRows = "$1:$7";
                sheet1.PageSetup.PrintTitleRows = "$A$5:$IV$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + (string)Session["USER"] + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Advance&TDS";
                #endregion

                workbook.Version = ExcelVersion.Excel2013;
                string fileName = monthName + "-" + "Advance And TDSSummary" + DateTime.Now.ToString("yyMMdd") + ".xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        public string getMonthYearWithoutAnd(string fromDate, string toDate, string monthNo, string yearNo)
        {
            var r = "";
            var _fDate = Convert.ToDateTime(fromDate);
            var _tDate = Convert.ToDateTime(toDate);
            while (_fDate < _tDate)
            {
                if (r.Length == 0)
                {
                    r = " (" + monthNo + " =" + _fDate.Year + " AND " + yearNo + " =" + _fDate.Month + ")";
                }
                else
                {
                    r += " OR (" + monthNo + " =" + _fDate.Year + " AND " + yearNo + " =" + _fDate.Month + ")";

                }
                _fDate = _fDate.AddMonths(1);

            }
            if (r.Length > 0)
            {
                r = " (" + r + ")";
            }

            return r;
        }


        #endregion -- Operations
        private void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = 10;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {
            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        }

        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            string NumberFormatString = "#,##0;(#,##0)";

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

        }
        public string NumberFormatIntWithComma()
        {
            return "#,#,#0;";
        }

        public void GetAdvanceAndTDS(ParamList para, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string strSQL = string.Empty;
                var EmpStatus = "";
                if (para.EmpStatus != "ALL")
                {
                    EmpStatus = @"AND E.EmployeeStatus='" + para.EmpStatus + "'";
                }
                string strSqlSal = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + para.PlantId + @"' GROUP BY SlrProcMstSystemID)
                                         AND FromDate BETWEEN '" + para.FromDate + @"' and '" + para.ToDate + @"'";
                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSqlSal);
                string salaryProcessID;
                salaryProcessID = "''";

                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }

                if (!string.IsNullOrEmpty(para.EmployeeId))
                {
                }

                strSQL = @"SELECT distinct EMP.SystemId,isnull(Advance.TaxAmount,0) TaxAmount 
                                ,ISNULL(advance.Advance,0) AdvanceAmount
                                ,ISNULL(Advance.Gross,0) GrossAmount 
                                ,EMP.EmployeeCode,Emp.EmployeeCodeNumeric,Emp.EmployeeCodePreFix,EMP.EmployeeName,DC.DocNumber
                                ,''Loan,''LoanInt,''Bus,''Mobile
                                FROM  EmployeeInformation EMP
                            LEFT JOIN
                            (
                             select EmpInfoSystemID,SUM(TaxAmount)TaxAmount, SUM(Gross)Gross,SUM(Advance)Advance from (select SlrProcMstSystemID,ct.EmpInfoSystemID,(ct.DisbusmentAmount)*(-1) AS TaxAmount,0 AS Gross,0 AS Advance from 
                            SalaryProcChild CT 
                            join SalaryHead HT ON HT.SalaryHeadID=CT.SalaryHeadID and SlrProcMstSystemID IN (" + salaryProcessID + @") and ct.PlantID = '" + para.PlantId + @"'
                            where HT.HeadCategory='Tax' 
							union all
							 select SlrProcMstSystemID,ct.EmpInfoSystemID, 0 AS TaxAmount,(ct.DisbusmentAmount) AS Gross,0 AS Advance from 
                            SalaryProcChild CT 
                            join SalaryHead HT ON HT.SalaryHeadID=CT.SalaryHeadID and SlrProcMstSystemID IN (" + salaryProcessID + @") and ct.PlantID = '" + para.PlantId + @"'
                            where HT.HeadCategory='Gross' 
							union all
							 select SlrProcMstSystemID,ct.EmpInfoSystemID,0 AS TaxAmount,0 AS Gross,(ct.DisbusmentAmount)*(-1) AS Advance from 
                            SalaryProcChild CT 
                            join SalaryHead HT ON HT.SalaryHeadID=CT.SalaryHeadID and SlrProcMstSystemID IN (" + salaryProcessID + @") and ct.PlantID = '" + para.PlantId + @"'
                            where HT.HeadCategory='Advance' ) AS K group by EmpInfoSystemID
                            
                            ) AS Advance on Advance.EmpInfoSystemID=EMP.SystemId
                            LEFT JOIN (
                            SELECT ED.EmpSystemID,ED.DocNumber from EmployeeDocument ED
                            LEFT JOIN HKP.ComplianceDocument CD ON CD.Id=ED.ComplianceDocumentId Where CD.ProfileType='TIN') DC ON DC.EmpSystemID=EMP.SystemId";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void GetAdvanceAndTDSSummary(ParamList para, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var EmpStatus = "";
                if (para.EmpStatus != "ALL")
                {
                    EmpStatus = @"AND E.EmployeeStatus='" + para.EmpStatus + "'";
                }

                var EmpSysId = "";
                var EmpSysIds = "";
                if (!string.IsNullOrEmpty(para.EmployeeId))
                {
                }
                strSQL = @"SELECT SUM(TaxAmount) TaxAmount ,HeadCategoryT ,SUM(AdvanceAmount) AdvanceAmount, HeadCategoryA,SUM(GrossAmount) GrossAmount ,HeadCategoryG
                         ,EmpInfoSystemID  ,EmployeeCode,EmployeeName,DocNumber,EmployeeCodePreFix ,EmployeeCodeNumeric 
                     FROM (
                            SELECT distinct s.MonthNo, c.EmpInfoSystemID,isnull(tax.TaxAmount,0) TaxAmount ,tax.HeadCategory HeadCategoryT,isnull(advance.AdvanceAmount,0) AdvanceAmount,Advance.HeadCategory HeadCategoryA,isnull(gross.GrossAmount,0) GrossAmount ,Gross.HeadCategory HeadCategoryG
                            ,EMP.EmployeeCode,EMP.EmployeeCodePreFix ,EMP.EmployeeCodeNumeric  ,EMP.EmployeeName,DC.DocNumber
                            FROM SalaryProcMaster S
                            inner join SalaryProcChild C on s.SystemID=c.SlrProcMstSystemID
                            --inner join SalaryProcessLogSummary LS on ls.SalaryProcessId=s.SystemID
                            inner join EmployeeInformation EMP on EMP.SystemId=C.EmpInfoSystemID
                            LEFT JOIN
                            (
                            select SlrProcMstSystemID,ct.EmpInfoSystemID,sum(ct.DisbusmentAmount)*(-1) AS TaxAmount,HT.HeadCategory from 
                            SalaryProcChild CT 
                            join SalaryHead HT ON HT.SalaryHeadID=CT.SalaryHeadID 
                            where HT.HeadCategory='Tax' 
                            group by SlrProcMstSystemID,ct.EmpInfoSystemID,HT.HeadCategory
                            
                            ) AS TAX on tax.SlrProcMstSystemID=s.SystemID and Tax.EmpInfoSystemID=c.EmpInfoSystemID
                            
                            LEFT JOIN
                            (
                            select SlrProcMstSystemID,ct.EmpInfoSystemID,sum(ct.DisbusmentAmount) AS GrossAmount,HT.HeadCategory from 
                            SalaryProcChild CT 
                            join SalaryHead HT ON HT.SalaryHeadID=CT.SalaryHeadID 
                            where HT.HeadCategory='Gross' 
                            group by SlrProcMstSystemID,ct.EmpInfoSystemID,HT.HeadCategory
                            
                            ) AS Gross on Gross.SlrProcMstSystemID=s.SystemID and Gross.EmpInfoSystemID=c.EmpInfoSystemID
                            
                            
                            LEFT JOIN
                            (
                            select SlrProcMstSystemID,ct.EmpInfoSystemID,sum(ct.DisbusmentAmount)*(-1) AS AdvanceAmount,HT.HeadCategory from 
                            SalaryProcChild CT 
                            join SalaryHead HT ON HT.SalaryHeadID=CT.SalaryHeadID 
                            where  HT.HeadCategory='Advance' 
                            group by SlrProcMstSystemID,ct.EmpInfoSystemID,HT.HeadCategory
                            
                            ) AS Advance on Advance.SlrProcMstSystemID=s.SystemID and Advance.EmpInfoSystemID=c.EmpInfoSystemID
                            LEFT JOIN (
                            SELECT ED.EmpSystemID,ED.DocNumber from EmployeeDocument ED
                            LEFT JOIN HKP.ComplianceDocument CD ON CD.Id=ED.ComplianceDocumentId Where CD.ProfileType='TIN') DC ON DC.EmpSystemID=C.EmpInfoSystemID
                            WHERE " + getMonthYearWithoutAnd(para.FromDate, para.ToDate, "s.YearNo", "S.MonthNo") + @"--S.FromDate between '" + para.FromDate + @"' and '" + para.ToDate + @"'
                            and C.PlantID = '" + para.PlantId + @"' AND ISNULL(c.SystemID,'')<>'') A
	                        GROUP BY HeadCategoryT , HeadCategoryA,HeadCategoryA
                                ,EmpInfoSystemID  ,EmployeeCode,EmployeeName,DocNumber,EmployeeCodePreFix ,EmployeeCodeNumeric,HeadCategoryG
							
                            ORDER BY  A.EmployeeCodePreFix ,A.EmployeeCodeNumeric ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}