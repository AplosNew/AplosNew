using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Library.Service.Helpers.ReportUtility;

namespace Library.HumanResource.Attendance
{
    public class DailyAttandanceSummeryReport
    {
        ISqlRepository _sqlRepository;
        public DailyAttandanceSummeryReport()
        {
            _sqlRepository = new SqlRepository();
        }


        #region Report 

        public IWorkbook XlsDailyAttendanceSummaryRpt(string WorkDate)//XlsDailyAttendanceSummaryRpt()
        {

            #region Variable

            clsReport objRpt = null;

            DataSet dsAttdnSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;

            #endregion Variable

            try
            {
                if (string.IsNullOrEmpty(WorkDate) == true || bplib.clsWebLib.IsDateOK(WorkDate) == false)
                {
                    Exception ex = new Exception("Please define Attendance Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sUnit = "ALL";
                var sDevi = "ALL";
                var sDept = "ALL";
                var sSect = "ALL";
                var sSbSect = "ALL";


                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                para.PlantId = identity.PlantId;
                //para.JobLocationId = JobLocation;


                leavePara.PlantId = identity.PlantId;

                attdnProcessParam.PlantId = identity.PlantId;

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion Variable

                #region DataSet

                //Sql Salary Structure 
                objRpt.GetAttendanceSummarySql(para, WorkDate, sUnit, sDevi, sDept, sSect, sSbSect, out dsAttdnSummary);

                DataTable dtAttdnSummary = dsAttdnSummary.Tables[0];
                if (dsAttdnSummary.Tables[0].Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                OTSBD.clsSalary.clsSalaryReport srB = new OTSBD.clsSalary.clsSalaryReport();


                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                xlsRow = 5;
                xlsCol = 1;

                var colEmpCatg = 0;
                var colDepartment = 0;
                var colSec = 0;
                var ColDesigGrp = 0;

                var colOnRole = 0;
                var colPresent = 0;
                var colAbsent = 0;
                var colLate = 0;
                var colLeave = 0;
                var colWeekOffHoliday = 0;
                var colAbsPer = 0;



                #region------------------Column Header------------------
                SetHeadText("Category", sheet1, xlsRow, ref xlsCol, out colEmpCatg, 9);
                SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out colDepartment, 37);
                SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out colSec, 13);
                SetHeadText("Desig. Group ", sheet1, xlsRow, ref xlsCol, out ColDesigGrp, 11.71);
                SetHeadText("Recruited", sheet1, xlsRow, ref xlsCol, out colOnRole, 9.14);
                SetHeadText("Present", sheet1, xlsRow, ref xlsCol, out colPresent, 7.29);
                SetHeadText("Absent", sheet1, xlsRow, ref xlsCol, out colAbsent, 7);
                SetHeadText("Late", sheet1, xlsRow, ref xlsCol, out colLate, 7);
                SetHeadText("Leave", sheet1, xlsRow, ref xlsCol, out colLeave, 7);
                SetHeadText("W.Off", sheet1, xlsRow, ref xlsCol, out colWeekOffHoliday, 15);
                SetHeadText("Abs%", sheet1, xlsRow, ref xlsCol, out colAbsPer, 15);
                int RowHeaderLimit = xlsRow;
                #endregion------------------Column Header------------------
                endXlsCol = (xlsCol - 1);
                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = identity.CompanyGroupId;
                param.CompanyId = identity.CompanyId;

                string FactoryAddress = string.Empty;

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
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

               
                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Attendance Summary";
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                var strRptDateRange = "";
                strRptDateRange = WorkDate;
                sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                //  var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;



                xlsRow--;
                xlsRow--;
                var startXlsRow = xlsRow;
                if (dtAttdnSummary.Rows.Count > 0)
                {
                    string _empcat = string.Empty;
                    string _department = string.Empty;
                    string _section = string.Empty;
                    string _DesignationGroup = string.Empty;

                    var isFirst = true;
                    var catFRow = xlsRow;
                    ArrayList al = new ArrayList();
                    var lastEmpCat = string.Empty;
                    for (int i = 0; i <= dtAttdnSummary.Rows.Count - 1; i++)
                    {
                        var catLRow = xlsRow;
                        if (_empcat != dtAttdnSummary.Rows[i]["EmpCategory"].ToString() && string.IsNullOrEmpty(dtAttdnSummary.Rows[i]["EmpCategory"].ToString()) == false)
                        {
                            _empcat = dtAttdnSummary.Rows[i]["EmpCategory"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastEmpCat = _empcat;
                                al.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();
                                sheet1.Range[xlsRow, colOnRole].Formula = "=SUM(" + ru.GetColumnNameForXls(colOnRole) + catFRow + ":" + ru.GetColumnNameForXls(colOnRole) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;
                                xlsRow++;
                            }
                            #endregion
                            SetCellText(sheet1, xlsRow, colEmpCatg, _empcat);
                            _department = dtAttdnSummary.Rows[i]["Department"].ToString();
                            SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString();
                            SetCellText(sheet1, xlsRow, colSec, _section);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString();
                            SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);

                            if (catFRow < xlsRow)
                            {

                                catFRow = xlsRow;
                            }
                        }
                        else if (_department != dtAttdnSummary.Rows[i]["Department"].ToString())
                        {
                            _department = dtAttdnSummary.Rows[i]["Department"].ToString(); SetCellText(sheet1, xlsRow, colDepartment, _department);
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        else if (_section != dtAttdnSummary.Rows[i]["Section"].ToString())
                        {
                            _section = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }
                        else if (_DesignationGroup != dtAttdnSummary.Rows[i]["DesignationGroup"].ToString())
                        {
                            _DesignationGroup = dtAttdnSummary.Rows[i]["Section"].ToString(); SetCellText(sheet1, xlsRow, colSec, _section);
                            _DesignationGroup = dtAttdnSummary.Rows[i]["DesignationGroup"].ToString(); SetCellText(sheet1, xlsRow, ColDesigGrp, _DesignationGroup);
                        }


                        SetCellText(sheet1, xlsRow, colOnRole, Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colPresent, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalPresentEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colAbsent, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colLate, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalLateEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colLeave, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalLeaveEmployee"].ToString()));
                        SetCellText(sheet1, xlsRow, colWeekOffHoliday, Convert.ToDouble(dtAttdnSummary.Rows[i]["totalWeekoffEmployee"].ToString()));

                        var ap = Convert.ToDouble(dtAttdnSummary.Rows[i]["totalAbsentEmployee"].ToString()) / Convert.ToDouble(dtAttdnSummary.Rows[i]["OnRoleEmployee"].ToString());
                        SetCellText(sheet1, xlsRow, colAbsPer, Convert.ToDouble(ap * 100));
                        xlsRow++;
                    }//for emp count

                    #region Last subtotal
                    al.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                    sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();
                    sheet1.Range[xlsRow, colOnRole].Formula = "=SUM(" + ru.GetColumnNameForXls(colOnRole) + catFRow + ":" + ru.GetColumnNameForXls(colOnRole) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colPresent].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresent) + catFRow + ":" + ru.GetColumnNameForXls(colPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colAbsent].Formula = "=SUM(" + ru.GetColumnNameForXls(colAbsent) + catFRow + ":" + ru.GetColumnNameForXls(colAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colLate].Formula = "=SUM(" + ru.GetColumnNameForXls(colLate) + catFRow + ":" + ru.GetColumnNameForXls(colLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colLeave].Formula = "=SUM(" + ru.GetColumnNameForXls(colLeave) + catFRow + ":" + ru.GetColumnNameForXls(colLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colWeekOffHoliday].Formula = "=SUM(" + ru.GetColumnNameForXls(colWeekOffHoliday) + catFRow + ":" + ru.GetColumnNameForXls(colWeekOffHoliday) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;
                    xlsRow++;
                    #endregion

                    #region Grand Total
                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (colOnRole - 1)].Merge();


                    sheet1.Range[xlsRow, colOnRole].Formula = GetFormulaGrandTotal(al, colOnRole);
                    sheet1.Range[xlsRow, colPresent].Formula = GetFormulaGrandTotal(al, colPresent);
                    sheet1.Range[xlsRow, colAbsent].Formula = GetFormulaGrandTotal(al, colAbsent);
                    sheet1.Range[xlsRow, colLate].Formula = GetFormulaGrandTotal(al, colLate);
                    sheet1.Range[xlsRow, colLeave].Formula = GetFormulaGrandTotal(al, colLeave);
                    sheet1.Range[xlsRow, colWeekOffHoliday].Formula = GetFormulaGrandTotal(al, colWeekOffHoliday);
                    sheet1.Range[xlsRow, colOnRole, xlsRow, colAbsPer].CellStyle.Font.Bold = true;


                    #endregion

                }

                #endregion ----------------------Data-----------------------
                var endXlsRow = xlsRow;
                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startXlsRow, 1, endXlsRow, endXlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var xx = RowHeaderLimit + 1;
                sheet1.UsedRange["A" + xx].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 10;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "AttendanceSummary";
                #endregion

                workbook.Version = ExcelVersion.Excel2016;
                string strFileName = "AttdnSumOf" + WorkDate.Trim() + ".xls";
                //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, Response, ExcelDownloadType.PromptDialog);

                return workbook;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        private void SetHeadText(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.AliceBlue;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetHeadText(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {
            //if (string.IsNullOrEmpty(Text) == false)
            //{
            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            //}
        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            string NumberFormatString = "#,##0;(#,##0)";            

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = NumberFormatString;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }
        string GetFormulaGrandTotal(ArrayList al, int col)
        {
            string _formula = string.Empty;
            ReportUtility ru = new ReportUtility();
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    if (_formula.Length == 0)
                    {
                        _formula = "=" + ru.GetColumnNameForXls(col) + al[i];
                    }
                    else
                    {
                        _formula += "+" + ru.GetColumnNameForXls(col) + al[i];
                    }
                }
                return _formula;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

    }
}
