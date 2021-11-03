using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class WelFareReportsController : BaseController
    {
        #region Constructor

        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;
        public WelFareReportsController(
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

        #endregion -- Pages

        #region Get MANUAL Out Report FOR DateWise
        [HttpPost, Authorize]
        public ActionResult GetWelFareReport(string FromDate, string ToDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsReport objRpt = null;
            int slCount = 0;

            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsMonth = null;


            DataSet dsEmpWelfare = null;
            DataTable dtEmpWelfare = null;
            DataTable dtMonthInfo = null;

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

                para.PlantId = identity.PlantId;
         
                #endregion Variable
                var oRU = new ReportUtility();

                var colSr = 0;
                var colEmpCode = 0;
                var colEmpName = 0;
                var colTotalAmount = 0;
                var colDOS = 0;
                var colWelFareLabel = 0;
                var colWelFareIndividualTotal = 0;
                var colStart = 0;
                var colEmpFatherName = 0;

                var empCode = string.Empty;
                var _total_head_count = 0;
                List<FiscalYearMonthSequence> list = null;

                #region DataSet
                int fromYear = Convert.ToDateTime(FromDate).Year;
                int toYear = Convert.ToDateTime(ToDate).Year;

                if (toYear != fromYear)
                {
                    throw new Exception("Dates must be in same year");
                    //ShowMessage("Dates must be in same year");
                }
                if (Convert.ToDateTime(FromDate) > Convert.ToDateTime(ToDate))
                {
                    throw new Exception("Dates must be in same year");

                }
                else
                {
                    objRpt.GetFiscalMonthListSql(FromDate, ToDate, out dsMonth);

                    objRpt.GetMonthWiseEmpWelFareInfo(FromDate, ToDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, dsMonth.Tables[0], out dsEmpWelfare);
                    dtEmpWelfare = dsEmpWelfare.Tables[0];
                    dtMonthInfo = dsMonth.Tables[0];

                    objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                    objRpt.SelectedPlant(identity.PlantId, out dsFactory);

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
                    SetHeaderValue("EmpCode", sheet1, xlsRow, ref xlsCol, out colEmpCode, 9);
                    SetHeaderValue("Name", sheet1, xlsRow, ref xlsCol, out colEmpName, 25);
                    SetHeaderValue("Father Name", sheet1, xlsRow, ref xlsCol, out colEmpFatherName, 25);
                    SetHeaderValue("Contribution", sheet1, xlsRow, ref xlsCol, out colWelFareLabel, 22.14);
                    colStart = colWelFareLabel;
                    CreateDynamicMonthHead(dtMonthInfo, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref colStart, out list);
                    //xlsCol--;
                    SetHeaderValue("Total", sheet1, xlsRow, ref xlsCol, out colWelFareIndividualTotal, 12);
                    // SetHeaderValue("Employer's Share", sheet1, xlsRow, ref xlsCol, out colWFareEERS, 12);
                    SetHeaderValue("Amount Total", sheet1, xlsRow, ref xlsCol, out colTotalAmount, 12);
                    endXlsCol = colTotalAmount;
                    var fPanRow = xlsRow + 1;//Freeze pan starting rows
                    #endregion------------------Column Header------------------

                    #region ******************Report Header******************

                    var totalEarningAmountEEC = 0.00;
                    var totalEarningAmountEER = 0.00;
                    DataView view = new DataView(dtEmpWelfare);
                    DataTable dtEmpInfo = view.ToTable(true, "EmpSystemId", "EmployeeCode", "EmployeeName", "FatherName");

                    xlsRow++;
                    for (int dti = 0; dti < dtEmpInfo.Rows.Count; dti++)
                    {
                        totalEarningAmountEEC = 0.00;
                        totalEarningAmountEER = 0.00;

                        empCode = dtEmpInfo.Rows[dti]["EmployeeCode"].ToString();
                        slCount++;
                        sheet1.Range[xlsRow, colSr].Text = slCount.ToString();
                        sheet1.Range[xlsRow, colSr, xlsRow + 1, colSr].Merge();
                        sheet1.Range[xlsRow, colSr, xlsRow + 1, colSr].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, colSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colSr].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        sheet1.Range[xlsRow, colEmpCode].Text = empCode;
                        sheet1.Range[xlsRow, colEmpCode, xlsRow + 1, colEmpCode].Merge();
                        sheet1.Range[xlsRow, colEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colEmpCode, xlsRow + 1, colEmpCode].BorderAround(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow, colEmpName].Text = dtEmpInfo.Rows[dti]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, colEmpName, xlsRow + 1, colEmpName].Merge();
                        sheet1.Range[xlsRow, colEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colEmpName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpName, xlsRow + 1, colEmpName].BorderAround(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow, colEmpFatherName].Text = dtEmpInfo.Rows[dti]["FatherName"].ToString();
                        sheet1.Range[xlsRow, colEmpFatherName, xlsRow + 1, colEmpFatherName].Merge();
                        sheet1.Range[xlsRow, colEmpFatherName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colEmpFatherName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpFatherName].BorderAround(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow, colWelFareLabel].Text = "Employee's contribution->";
                        sheet1.Range[xlsRow, colWelFareLabel].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colWelFareLabel].BorderAround(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow + 1, colWelFareLabel].Text = "Employer's contribution->";
                        sheet1.Range[xlsRow + 1, colWelFareLabel].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow + 1, colWelFareLabel].BorderAround(ExcelLineStyle.Hair);


                        for (int ci = 0; ci < list.Count; ci++)
                        {
                            var ob = list[ci];
                            var welfareEEC = "Welfare (Employee contribution)";
                            var welfareEER = "Welfare (Employer contribution)";

                            DataView dvWelfareEEC = new DataView(dtEmpWelfare);
                            dvWelfareEEC.RowFilter = "EmployeeCode='" + empCode + "' AND SalaryHead = '" + welfareEEC + "'  AND MonthNo = '" + ob.MonthNo + "'  AND YearNo = '" + ob.MonthYear + "'";

                            DataView dvWelfareEER = new DataView(dtEmpWelfare);
                            dvWelfareEER.RowFilter = "EmployeeCode='" + empCode + "' AND SalaryHead = '" + welfareEER + "'  AND MonthNo = '" + ob.MonthNo + "'  AND YearNo = '" + ob.MonthYear + "'";

                            var earningWalfareAmountEEC = string.Empty;
                            var earningWalfareAmountEER = string.Empty;

                            if (dvWelfareEEC.Count > 0)
                            {
                                earningWalfareAmountEEC = (Convert.ToDouble(dvWelfareEEC[0]["DisbusmentAmount"]) * -1).ToString();
                                totalEarningAmountEEC += Convert.ToDouble(earningWalfareAmountEEC);

                                sheet1.Range[xlsRow, ob.XLColIndex].Number = Math.Round(Convert.ToDouble(earningWalfareAmountEEC), 2);// + Environment.NewLine + totalPayDay;
                                sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, ob.XLColIndex].BorderAround(ExcelLineStyle.Hair);
                            }
                            else
                            {
                                sheet1.Range[xlsRow, ob.XLColIndex].Text = "";// + Environment.NewLine + totalPayDay;
                                sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, ob.XLColIndex].BorderAround(ExcelLineStyle.Hair);
                            }
                            if (dvWelfareEER.Count > 0)
                            {
                                earningWalfareAmountEER = dvWelfareEER[0]["DisbusmentAmount"].ToString();
                                totalEarningAmountEER += Convert.ToDouble(earningWalfareAmountEER);

                                sheet1.Range[xlsRow + 1, ob.XLColIndex].Number = Math.Round(Convert.ToDouble(earningWalfareAmountEER), 2);// + Environment.NewLine + totalPayDay;
                                sheet1.Range[xlsRow + 1, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow + 1, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow + 1, ob.XLColIndex].BorderAround(ExcelLineStyle.Hair);
                            }
                            else
                            {
                                sheet1.Range[xlsRow + 1, ob.XLColIndex].Text = "";// + Environment.NewLine + totalPayDay;
                                sheet1.Range[xlsRow + 1, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow + 1, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow + 1, ob.XLColIndex].BorderAround(ExcelLineStyle.Hair);
                            }
                        }
                        //Total Amount
                        sheet1.Range[xlsRow, colWelFareIndividualTotal].Text = totalEarningAmountEEC.ToString();
                        sheet1.Range[xlsRow, colWelFareIndividualTotal].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colWelFareIndividualTotal].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colWelFareIndividualTotal].BorderAround(ExcelLineStyle.Hair);

                        sheet1.Range[xlsRow + 1, colWelFareIndividualTotal].Text = totalEarningAmountEER.ToString();
                        sheet1.Range[xlsRow + 1, colWelFareIndividualTotal].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow + 1, colWelFareIndividualTotal].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 1, colWelFareIndividualTotal].BorderAround(ExcelLineStyle.Hair);

                        var totalCalcFormula = "=SUM(" + ru.GetColumnNameForXls(colWelFareIndividualTotal) + xlsRow + "+" + ru.GetColumnNameForXls(colWelFareIndividualTotal) + (xlsRow + 1) + ")";

                        sheet1.Range[xlsRow, colTotalAmount].Formula = totalCalcFormula;
                        sheet1.Range[xlsRow, colTotalAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow, colTotalAmount, xlsRow + 1, colTotalAmount].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow, colTotalAmount, xlsRow + 1, colTotalAmount].Merge();
                        xlsRow += 2;
                    }


                    objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
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
                    sheet1.Range[xlsRow, xlsCol].Text = "Welfare Return Report :" + Convert.ToDateTime(FromDate).ToString("MMMM") + "," + Convert.ToDateTime(FromDate).Year.ToString() + " TO " + Convert.ToDateTime(ToDate).ToString("MMMM") + "," + Convert.ToDateTime(ToDate).Year.ToString();
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
                    sheet1.PageSetup.PrintTitleRows = "$A$5:$IV$5";

                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                    sheet1.Name = "WalfareReturnReport" + para.SalaryProcessId;
                    #endregion

                    workbook.Version = ExcelVersion.Excel97to2003;
                    var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "WelfareReturnReport.xls";
                    string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                    workbook.SaveAs(fullPath);

                    //workbook.Close();
                    //excelEngine.Dispose();
                   return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
                }
            }

            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

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
        private void CreateDynamicMonthHead(DataTable dtMonthList, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColStart, out List<FiscalYearMonthSequence> list)
        {
            try
            {
                list = new List<FiscalYearMonthSequence>();
                _total_head_count = 0;

                int countGross = 0;
                string grossFormula = "";
                string deductionFormula = "";
                for (int ci = 0; ci < dtMonthList.Rows.Count; ci++)
                {
                    _total_head_count++;
                    countGross++;
                    sheet1.Range[xlsRow, ColStart + countGross].Text = dtMonthList.Rows[ci]["MonthName"].ToString().Substring(0, 3) + "," + dtMonthList.Rows[ci]["MonthYear"].ToString().Substring(2, 2);
                    sheet1.Range[xlsRow, ColStart + countGross].ColumnWidth = 8;
                    sheet1.Range[xlsRow, ColStart + countGross].CellStyle.Font.Bold = true;
                    //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    sheet1.Range[xlsRow, ColStart + countGross].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, ColStart + countGross].BorderAround(ExcelLineStyle.Thin);

                    FiscalYearMonthSequence fiscalYearMonthSequence = new FiscalYearMonthSequence();
                    fiscalYearMonthSequence.MonthName = dtMonthList.Rows[ci]["MonthName"].ToString();
                    fiscalYearMonthSequence.MonthNo = dtMonthList.Rows[ci]["MonthNumber"].ToString();
                    fiscalYearMonthSequence.LastDayOfMonth = dtMonthList.Rows[ci]["LastDayOfMonth"].ToString();
                    fiscalYearMonthSequence.MonthYear = dtMonthList.Rows[ci]["MonthYear"].ToString();
                    fiscalYearMonthSequence.XLColIndex = ColStart + countGross;

                    list.Add(fiscalYearMonthSequence);
                    xlsCol += 1;
                }//for         
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
