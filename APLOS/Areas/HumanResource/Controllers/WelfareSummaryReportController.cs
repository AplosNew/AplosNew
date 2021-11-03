using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.HumanResources;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class WelfareSummaryReportController : BaseController
    {
        #region Constructor

        //private readonly IWelfareSummaryReportService _welfareSummaryReportService;

        public WelfareSummaryReportController(IWelfareSummaryReportService welfareSummaryReportService)
        {
           // _welfareSummaryReportService = welfareSummaryReportService;
        }

        #endregion Constructor

        #region -- Pages

        
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations
        [HttpPost,Authorize]
        public ActionResult XlsEmployeeWalfareSummary(string year)
        {
            #region Variable

            clsReport objRpt = null;
            int slCount = 0;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            DataSet dsWalfareSummary = null;
            DataTable dtWalfareSummary = null;

            //DataSet dsEmpAttdn = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
     
            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            #endregion Variable

            try
            {
                ru = new ReportUtility();

                objRpt = new clsReport();

                #region Variable
                ParamList para = new ParamList();
                ParamList leavePara = new ParamList();
                ParamList attdnProcessParam = new ParamList();

                var FactoryName = "";
                var CmpName = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                para.PlantId = identity.PlantId;

                 
                #endregion Variable
                var oRU = new ReportUtility();

                var colSr = 0;
                var colTotal = 0;
                var colEmployerShare = 0;
                var colMonth = 0;
                var colEmployeeShare = 0;

                #region DataSet

                objRpt.GetWelfareSummary(identity.CompanyGroupId,identity.CompanyId, para.PlantId, year, out dsWalfareSummary);
                dtWalfareSummary = dsWalfareSummary.Tables[0];

                objRpt.SelectedPlantWiseCompany(para.PlantId, out dsCmp);

                objRpt.SelectedPlant(para.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 5;
                xlsCol = 1;
                #region------------------Column Header------------------

                SetHeaderValue("S.No.", sheet1, xlsRow, ref xlsCol, out colSr, 6);
                SetHeaderValue("Month", sheet1, xlsRow, ref xlsCol, out colMonth, 10);
                SetHeaderValue("Employee Share", sheet1, xlsRow, ref xlsCol, out colEmployeeShare, 15);
                SetHeaderValue("Employer Share", sheet1, xlsRow, ref xlsCol, out colEmployerShare, 15);
                SetHeaderValue("Total", sheet1, xlsRow, ref xlsCol, out colTotal, 12);
                endXlsCol = colTotal;
                #endregion------------------Column Header------------------
                var fPanRow = xlsRow + 1;//Freeze pan starting rows

                #region Data to Excel Column
                xlsRow++;
                var formulaStartRow = xlsRow;
                for (int mi = 1; mi <= 12; mi++)
                {
                    slCount++;
                    ru.SetSLText(ref sheet1, xlsRow, colSr, slCount);
                    ru.SetText(ref sheet1, xlsRow, colMonth, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mi));
                    for (int i = 0; i < dtWalfareSummary.Rows.Count; i++)
                    {
                        if (mi == Convert.ToInt32(dtWalfareSummary.Rows[i]["MonthNo"]))
                        {
                            if (dtWalfareSummary.Rows[i]["SalaryHead"].ToString() == "Welfare (Employee contribution)")
                            {

                                ru.SetText(ref sheet1, xlsRow, colEmployeeShare, Convert.ToInt32(dtWalfareSummary.Rows[i]["EntryAmount"]));
                            }
                            if (dtWalfareSummary.Rows[i]["SalaryHead"].ToString() == "Welfare (Employer contribution)")
                            {

                                ru.SetText(ref sheet1, xlsRow, colEmployerShare, Convert.ToInt32(dtWalfareSummary.Rows[i]["EntryAmount"]));
                            }

                            var formulaText = "=SUM(" + ru.GetColumnNameForXls(colEmployeeShare) + xlsRow + "+" + ru.GetColumnNameForXls(colEmployerShare) + xlsRow + ")";

                            ru.SetColFormula(ref sheet1, xlsRow, colTotal, formulaText, false);
                            sheet1.Range[xlsRow, colTotal].HorizontalAlignment = ExcelHAlign.HAlignRight;

                        }

                    }

                    xlsRow++;
                }
                var summationRowLimit = xlsRow - 1;
                sheet1.Range[xlsRow, colMonth].Text = "Total";
                sheet1.Range[xlsRow, colMonth].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colMonth].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet1.Range[xlsRow, colEmployeeShare].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colEmployeeShare].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colEmployeeShare].Formula = "=SUM(" + ru.GetColumnNameForXls(colEmployeeShare) + formulaStartRow + ":" + ru.GetColumnNameForXls(colEmployeeShare) + (summationRowLimit) + ")";
                sheet1.Range[xlsRow, colEmployeeShare].NumberFormat = ru.NumberFormatInt();


                sheet1.Range[xlsRow, colEmployerShare].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colEmployerShare].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colEmployerShare].Formula = "=SUM(" + ru.GetColumnNameForXls(colEmployerShare) + formulaStartRow + ":" + ru.GetColumnNameForXls(colEmployerShare) + (summationRowLimit) + ")";
                sheet1.Range[xlsRow, colEmployerShare].NumberFormat = ru.NumberFormatInt();

                sheet1.Range[xlsRow, colTotal].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colTotal].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colTotal].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotal) + formulaStartRow + ":" + ru.GetColumnNameForXls(colTotal) + (summationRowLimit) + ")";
                sheet1.Range[xlsRow, colTotal].NumberFormat = ru.NumberFormatInt();

                
                #endregion

                #region ******************Report Header******************

                objRpt.SelectedPlantWiseCompany(para.PlantId, out dsCmp);
                xlsRow = 1;
                xlsCol = 1;
                FactoryName = string.Empty;
                var FactoryAddress = string.Empty;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Welfare summary Report of " + year;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Report Header******************

                #region Freeze Panes
                sheet1.UsedRange["A" + fPanRow].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                //sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "WelfareSummary";
                #endregion

                workbook.Version = ExcelVersion.Excel2016;
                string strFileName = DateTime.Now.ToString("yyMMdd") + " " + "WelfareSummary.xlsx";

                //var fileName = month + "-" + year + "SalarySummary" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xlsx";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName;


               // var workbook = _payrollReportsService.GetEmployeeSalaryProcessedReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, month, year, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, identity.IsSysAdmin, identity.IsControlAdmin, isMaternity, true);
                workbook.Version = ExcelVersion.Excel2013;
                workbook.SaveAs(fullPath);
                //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, Response, ExcelDownloadType.PromptDialog);

                workbook.Close();
                excelEngine.Dispose();
                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
           
        }//End Function

        private void SetHeaderValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            ColIndex = xlsCol;
            xlsCol += 1;
        }


        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = 4;
            ColIndex = xlsCol;
        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol)
        {

            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = 4;
        }


        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            ColIndex = xlsCol;
            xlsCol += 1;
        }



        #endregion -- Operations
    }
}