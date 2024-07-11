#region Using

using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using static Library.Service.Helpers.ReportUtility;

#endregion Using

namespace Library.Service.HumanResources
{
    public class ManpowerAttendanceSummary : IManpowerAttendanceSummary
    {

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ManpowerAttendanceSummary(

             IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }


        public IWorkbook GetSummaryManpowerAttendanceGroupWiseExcel(string PlantId, string companyId, string workDate, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID)
        {

            try
            {


                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                DataTable dtManPBSummary = GetDataManpowerAttendanceSummarySql(PlantId, companyId, workDate, sUnitID, sDivID, sDepID, sSecID, sSubSecID);
                dtManPBSummary.Columns.Add("sequence1", typeof(int));
                dtManPBSummary.Columns.Add("sequence2", typeof(int));

                for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                {

                    dtManPBSummary.Rows[i]["sequence1"] = (int)clsStaticInfo.dbl(Regex.Match(dtManPBSummary.Rows[i]["Group1"].ToString(), @"\d+").Value);
                    dtManPBSummary.Rows[i]["sequence2"] = (int)clsStaticInfo.dbl(Regex.Match(dtManPBSummary.Rows[i]["Group2"].ToString(), @"\d+").Value);
                }


                dvDaily = new DataView(dtManPBSummary);
                dvDaily.Sort = "sequence1,Group1,sequence2,Group2";
                dvDaily = new DataView(dvDaily.ToTable());
                dtManPBSummary = dvDaily.ToTable();

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cUnit = 0; int cSubSection = 0; int cAttendancGroup = 0; int cOnRollManpower; int cBudgetedManPower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdOthers = 0; var cfdRemarks = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Unit", ExcelHAlign.HAlignCenter); cUnit = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sub Section", ExcelHAlign.HAlignCenter); cSubSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Attendance Group", ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted", 8, ExcelHAlign.HAlignCenter); cBudgetedManPower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 8, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 8, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 8, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 8, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 8, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Others", 8, ExcelHAlign.HAlignCenter); cfdOthers = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarkes", 10, ExcelHAlign.HAlignCenter); cfdRemarks = xlsCol;

                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                if (dtManPBSummary.Rows.Count > 0)
                {
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _grp3 = string.Empty;

                    #endregion
                    var catFRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;
                        if (_grp1 != dtManPBSummary.Rows[i]["Group1"].ToString() && string.IsNullOrEmpty(dtManPBSummary.Rows[i]["Group1"].ToString()) == false)
                        {
                            _grp1 = dtManPBSummary.Rows[i]["Group1"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastMPGroup = _grp1;
                                rowList.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                                sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);
                                //sheet1.Range[xlsRow, cfdRemarks].BorderAround(ExcelLineStyle.Hair);


                                xlsRow++;
                            }
                            #endregion

                            sheet1.Range[xlsRow, cUnit].Text = _grp1;
                            sheet1.Range[xlsRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignTop;



                            _grp2 = dtManPBSummary.Rows[i]["Group2"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            _grp3 = dtManPBSummary.Rows[i]["Group3"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp3);

                            if (catFRow < xlsRow)
                            {
                                catFRow = xlsRow;
                                catGrp2FRow = xlsRow;
                            }
                        }

                        else if (_grp2 != dtManPBSummary.Rows[i]["Group2"].ToString())
                        {
                            _grp2 = dtManPBSummary.Rows[i]["Group2"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cSubSection].Text = _grp2;
                            sheet1.Range[xlsRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cSubSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cSubSection].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp3 = dtManPBSummary.Rows[i]["Group3"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp3);
                            if (catGrp2FRow < xlsRow)
                            {
                                catGrp2FRow = xlsRow;
                            }
                        }
                        else if (_grp3 != dtManPBSummary.Rows[i]["Group3"].ToString())
                        {

                            _grp3 = dtManPBSummary.Rows[i]["Group3"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp3);

                            sheet1.Range[catFRow, cUnit, xlsRow, cUnit].Merge();
                            sheet1.Range[catFRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[catGrp2FRow, cSubSection, xlsRow, cSubSection].Merge();
                            sheet1.Range[catGrp2FRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);

                        }
                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, Convert.ToInt32(dtManPBSummary.Rows[i]["OnRoll"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cBudgetedManPower, Convert.ToInt32(dtManPBSummary.Rows[i]["BudgetedManPower"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, Convert.ToInt32(dtManPBSummary.Rows[i]["TotalPresent"].ToString()));//LegalDesignation
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, Convert.ToInt32(dtManPBSummary.Rows[i]["TotalAbsent"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, Convert.ToInt32(dtManPBSummary.Rows[i]["TotalLate"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, Convert.ToInt32(dtManPBSummary.Rows[i]["TotalLV"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdOthers, Convert.ToInt32(dtManPBSummary.Rows[i]["Others"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdRemarks, "");//


                        xlsRow++;
                    }
                    xlsRow += 1;

                    rowList.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");

                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;
                    xlsRow++;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = GetFormulaGrandTotal(rowList, cBudgetedManPower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdOthers].Formula = GetFormulaGrandTotal(rowList, cfdOthers);


                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);



                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(PlantId, "", out dsCmp);
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
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].Text = FactoryAddress;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook _GetSummaryManpowerAttendanceExcel(string companyGroupId, string companyId, string PlantId, string workDate, bool withLine)
        {
            try
            {
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                DataTable dtManPBSummary = GetDailyAttendanceSummarySqlNew(workDate, withLine, companyGroupId, companyId, PlantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cUnit = 0; int cSubSection, cSection, cEmpCategory = 0; int cAttendancGroup = 0; int cOnRollManpower; int cBudgetedManPower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdOthers = 0; var cfdRemarks = 0; int cDivision = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Category", 8, ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Division", ExcelHAlign.HAlignCenter); cDivision = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Unit", ExcelHAlign.HAlignCenter); cUnit = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Section", ExcelHAlign.HAlignCenter); cSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sub Section", ExcelHAlign.HAlignCenter); cSubSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted", 8, ExcelHAlign.HAlignCenter); cBudgetedManPower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 8, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 8, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 8, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 8, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 8, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Others", 8, ExcelHAlign.HAlignCenter); cfdOthers = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarkes", 10, ExcelHAlign.HAlignCenter); cfdRemarks = xlsCol;

                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                if (dtManPBSummary.Rows.Count > 0)
                {
                    string _cgrp1 = string.Empty;
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _sgrp3 = string.Empty;
                    string _grp3 = string.Empty;
                    string _grp4 = string.Empty;


                    #endregion
                    var catFRow = xlsRow;
                    var catcGrp2FRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catsGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;
                        if (_cgrp1 != dtManPBSummary.Rows[i]["EmpCategory"].ToString() && string.IsNullOrEmpty(dtManPBSummary.Rows[i]["EmpCategory"].ToString()) == false)
                        {
                            _cgrp1 = dtManPBSummary.Rows[i]["EmpCategory"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastMPGroup = _cgrp1;
                                rowList.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                                sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);
                                //sheet1.Range[xlsRow, cfdRemarks].BorderAround(ExcelLineStyle.Hair);


                                xlsRow++;
                            }
                            #endregion

                            sheet1.Range[xlsRow, cEmpCategory].Text = _cgrp1;
                            sheet1.Range[xlsRow, cEmpCategory, xlsRow, cEmpCategory].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cEmpCategory].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp1 = dtManPBSummary.Rows[i]["DivisionName"].ToString();
                            SetCellText(sheet1, xlsRow, cDivision, _grp1);
                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            SetCellText(sheet1, xlsRow, cUnit, _grp2);
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _sgrp3);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                            if (catFRow < xlsRow)
                            {
                                catFRow = xlsRow;
                                catcGrp2FRow = xlsRow;
                                catGrp2FRow = xlsRow;
                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;


                            }
                        }
                        else if (_grp1 != dtManPBSummary.Rows[i]["DivisionName"].ToString())
                        {
                            _grp1 = dtManPBSummary.Rows[i]["DivisionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cDivision].Text = _grp1;
                            sheet1.Range[xlsRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cDivision].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cDivision].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            SetCellText(sheet1, xlsRow, cUnit, _grp2);
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _sgrp3);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);

                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                            if (catGrp2FRow < xlsRow)
                            {
                                catcGrp2FRow = xlsRow;
                                catGrp2FRow = xlsRow;
                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;


                            }
                        }
                        else if (_grp2 != dtManPBSummary.Rows[i]["UnitName"].ToString())
                        {
                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cUnit].Text = _grp2;
                            sheet1.Range[xlsRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignTop;
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _sgrp3);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                            if (catGrp2FRow < xlsRow)
                            {
                                catGrp2FRow = xlsRow;
                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;

                            }
                        }
                        else if (_sgrp3 != dtManPBSummary.Rows[i]["SectionName"].ToString())
                        {
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cSection].Text = _sgrp3;
                            sheet1.Range[xlsRow, cSection, xlsRow, cSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cSection].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);

                            if (catsGrp3FRow < xlsRow)
                            {
                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;
                            }
                        }
                        else if (_grp3 != dtManPBSummary.Rows[i]["SubSectionName"].ToString())
                        {
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cSubSection].Text = _grp3;
                            sheet1.Range[xlsRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cSubSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cSubSection].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                            if (catGrp3FRow < xlsRow)
                            {
                                catGrp3FRow = xlsRow;
                            }
                        }
                        else if (_grp4 != dtManPBSummary.Rows[i]["DesignationName"].ToString())
                        {

                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);


                            sheet1.Range[catFRow, cEmpCategory, xlsRow, cEmpCategory].Merge();
                            sheet1.Range[catFRow, cEmpCategory, xlsRow, cEmpCategory].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[catcGrp2FRow, cDivision, xlsRow, cDivision].Merge();
                            sheet1.Range[catcGrp2FRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].Merge();
                            sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[catsGrp3FRow, cSection, xlsRow, cSection].Merge();
                            sheet1.Range[catsGrp3FRow, cSection, xlsRow, cSection].BorderAround(ExcelLineStyle.Hair);


                            sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].Merge();
                            sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);

                        }

                        oRU.SetTextBorder(ref sheet1, xlsRow, cBudgetedManPower, Convert.ToInt32(dtManPBSummary.Rows[i]["ProposedManpowerBudget"].ToString()));

                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, Convert.ToInt32(dtManPBSummary.Rows[i]["TotalManpower"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_PRESENT"].ToString()));//LegalDesignation
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Absent"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Late"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Leave"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdOthers, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Others"].ToString()));//

                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdRemarks, "");//
                        xlsRow++;
                    }
                    xlsRow += 1;

                    rowList.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");

                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;
                    xlsRow++;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = GetFormulaGrandTotal(rowList, cBudgetedManPower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdOthers].Formula = GetFormulaGrandTotal(rowList, cfdOthers);


                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);



                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(PlantId, "", out dsCmp);
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
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].Text = FactoryAddress;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook _GetSummaryManpowerAttendanceExcelWithLine(string companyGroupId, string companyId, string PlantId, string workDate, bool withLine)
        {
            try
            {
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                DataTable dtManPBSummary = GetDailyAttendanceSummarySqlNew(workDate, withLine, companyGroupId, companyId, PlantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cUnit = 0; int cSubSection, cSection, cEmpCategory = 0; int cLine = 0; int cAttendancGroup = 0; int cOnRollManpower; int cBudgetedManPower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdOthers = 0; var cfdRemarks = 0; int cDivision = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Category", 8, ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Division", ExcelHAlign.HAlignCenter); cDivision = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Unit", ExcelHAlign.HAlignCenter); cUnit = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Section", ExcelHAlign.HAlignCenter); cSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sub Section", ExcelHAlign.HAlignCenter); cSubSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Line", ExcelHAlign.HAlignCenter); cLine = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted", 8, ExcelHAlign.HAlignCenter); cBudgetedManPower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 8, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 8, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 8, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 8, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 8, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Others", 8, ExcelHAlign.HAlignCenter); cfdOthers = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarkes", 10, ExcelHAlign.HAlignCenter); cfdRemarks = xlsCol;
                #endregion
                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                if (dtManPBSummary.Rows.Count > 0)
                {
                    string _cgrp1 = string.Empty;
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _sgrp3 = string.Empty;
                    string _grp3 = string.Empty;
                    string _grp4 = string.Empty;
                    string _grp5 = string.Empty;



                    var catFRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catcGrp2FRow = xlsRow;
                    var catsGrp3FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;
                    var catGrp5FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;
                        if (_cgrp1 != dtManPBSummary.Rows[i]["EmpCategory"].ToString() && string.IsNullOrEmpty(dtManPBSummary.Rows[i]["EmpCategory"].ToString()) == false)
                        {
                            _cgrp1 = dtManPBSummary.Rows[i]["EmpCategory"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastMPGroup = _cgrp1;
                                rowList.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                                sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);
                                //sheet1.Range[xlsRow, cfdRemarks].BorderAround(ExcelLineStyle.Hair);


                                xlsRow++;
                            }
                            #endregion

                            sheet1.Range[xlsRow, cEmpCategory].Text = _cgrp1;
                            sheet1.Range[xlsRow, cEmpCategory, xlsRow, cEmpCategory].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cEmpCategory].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp1 = dtManPBSummary.Rows[i]["DivisionName"].ToString();
                            SetCellText(sheet1, xlsRow, cDivision, _grp1);
                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            SetCellText(sheet1, xlsRow, cUnit, _grp2);
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _sgrp3);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["LineName"].ToString();
                            SetCellText(sheet1, xlsRow, cLine, _grp4);
                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);
                            if (catFRow < xlsRow)
                            {
                                catFRow = xlsRow;
                                catcGrp2FRow = xlsRow;
                                catGrp2FRow = xlsRow;
                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;


                            }
                        }
                        else if (_grp1 != dtManPBSummary.Rows[i]["DivisionName"].ToString())
                        {
                            _grp1 = dtManPBSummary.Rows[i]["DivisionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cDivision].Text = _grp1;
                            sheet1.Range[xlsRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cDivision].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cDivision].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            SetCellText(sheet1, xlsRow, cUnit, _grp2);
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _sgrp3);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["LineName"].ToString();
                            SetCellText(sheet1, xlsRow, cLine, _grp4);
                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);
                            if (catGrp2FRow < xlsRow)
                            {
                                catGrp2FRow = xlsRow;
                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;


                            }
                        }
                        else if (_grp2 != dtManPBSummary.Rows[i]["UnitName"].ToString())
                        {
                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cUnit].Text = _grp2;
                            sheet1.Range[xlsRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _sgrp3);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["LineName"].ToString();
                            SetCellText(sheet1, xlsRow, cLine, _grp4);
                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);
                            if (catGrp2FRow < xlsRow)
                            {
                                catGrp2FRow = xlsRow;
                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;


                            }
                        }
                        else if (_sgrp3 != dtManPBSummary.Rows[i]["SectionName"].ToString())
                        {
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cSection].Text = _sgrp3;
                            sheet1.Range[xlsRow, cSection, xlsRow, cSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cSection].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["LineName"].ToString();
                            SetCellText(sheet1, xlsRow, cLine, _grp4);
                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);

                            if (catsGrp3FRow < xlsRow)
                            {
                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;
                            }
                        }
                        else if (_grp3 != dtManPBSummary.Rows[i]["SubSectionName"].ToString())
                        {
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cSubSection].Text = _grp3;
                            sheet1.Range[xlsRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cSubSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cSubSection].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp4 = dtManPBSummary.Rows[i]["LineName"].ToString();
                            SetCellText(sheet1, xlsRow, cLine, _grp4);
                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);
                            if (catGrp3FRow < xlsRow)
                            {
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;
                            }
                        }
                        else if (_grp4 != dtManPBSummary.Rows[i]["LineName"].ToString())
                        {

                            _grp4 = dtManPBSummary.Rows[i]["LineName"].ToString();

                            sheet1.Range[xlsRow, cLine].Text = _grp4;
                            sheet1.Range[xlsRow, cLine, xlsRow, cLine].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignTop;
                            //sheet1.Range[catFRow, cDivision, xlsRow, cDivision].Merge();
                            //sheet1.Range[catFRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                            //sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].Merge();
                            //sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            //sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].Merge();
                            //sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);
                            if (catGrp4FRow < xlsRow)
                            {
                                catGrp4FRow = xlsRow;
                            }

                        }
                        else if (_grp5 != dtManPBSummary.Rows[i]["DesignationName"].ToString())
                        {

                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);

                            sheet1.Range[catFRow, cEmpCategory, xlsRow, cEmpCategory].Merge();
                            sheet1.Range[catFRow, cEmpCategory, xlsRow, cEmpCategory].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[catcGrp2FRow, cDivision, xlsRow, cDivision].Merge();
                            sheet1.Range[catcGrp2FRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].Merge();
                            sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[catsGrp3FRow, cSection, xlsRow, cSection].Merge();
                            sheet1.Range[catsGrp3FRow, cSection, xlsRow, cSection].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].Merge();
                            sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[catGrp4FRow, cLine, xlsRow, cLine].Merge();
                            sheet1.Range[catGrp4FRow, cLine, xlsRow, cLine].BorderAround(ExcelLineStyle.Hair);


                        }
                        oRU.SetTextBorder(ref sheet1, xlsRow, cBudgetedManPower, Convert.ToInt32(dtManPBSummary.Rows[i]["ProposedManpowerBudget"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cEmpCategory, dtManPBSummary.Rows[i]["EmpCategory"].ToString());
                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, Convert.ToInt32(dtManPBSummary.Rows[i]["TotalManpower"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_PRESENT"].ToString()));//LegalDesignation
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Absent"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Late"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Leave"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdOthers, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Others"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdRemarks, "");//
                        xlsRow++;
                    }
                    xlsRow += 1;

                    rowList.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");

                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;
                    xlsRow++;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = GetFormulaGrandTotal(rowList, cBudgetedManPower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdOthers].Formula = GetFormulaGrandTotal(rowList, cfdOthers);


                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);



                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(PlantId, "", out dsCmp);
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
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].Text = FactoryAddress;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IWorkbook GetSummaryManpowerAttendanceExcelNew(string companyGroupId, string companyId, string workDate, bool withLine, bool withDesignation, string PlantIds, string typeLists, bool WithoutTBS, bool WithoutLA)
        {
            try
            {
                #region Variable
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;
                var startRow = 0;
                #endregion Variable
                //Create dataset
                DataTable dtManPBSummary = GetDailyManpowerAttendanceSummarySql(workDate, withLine, companyGroupId, companyId, PlantIds, typeLists, WithoutTBS, WithoutLA);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "Data";
                sheet = workbook.Worksheets[1];
                sheet.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cUnit = 0; int cSubSection = 0; int cAttendancGroup = 0; int cOnRollManpower; int cBudgetedManPower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdOthers = 0; var cfdRemarks = 0; int cDivision = 0; int cEmpCategory = 0; int cSection = 0; int cDepartment = 0; int cLine = 0;
                #endregion
                #region ColumnHeaders
                //oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Division", ExcelHAlign.HAlignCenter); cDivision = xlsCol; xlsCol++;
                //oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Unit", ExcelHAlign.HAlignCenter); cUnit = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Emp Category", ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Department", ExcelHAlign.HAlignCenter); cDepartment = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Section", ExcelHAlign.HAlignCenter); cSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Sub Section", ExcelHAlign.HAlignCenter); cSubSection = xlsCol; xlsCol++;
                if (withDesignation)
                {
                    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                }
                if (withLine)
                {
                    oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Line", ExcelHAlign.HAlignCenter); cLine = xlsCol; xlsCol++;
                }
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Budgeted", 8, ExcelHAlign.HAlignCenter); cBudgetedManPower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "OnRoll", 8, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Present", 8, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Absent", 8, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Late", 8, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Leave", 8, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Others", 8, ExcelHAlign.HAlignCenter); cfdOthers = xlsCol++;
                oRU.SetHeaderText(ref sheet, xlsRow, xlsCol, "Remarkes", 10, ExcelHAlign.HAlignCenter); cfdRemarks = xlsCol;

                var orgCollist = xlsCol;
                xlsRow++;
                startRow = xlsRow;
                endXlsCol = xlsCol;

                #endregion

                if (dtManPBSummary.Rows.Count > 0)
                {

                    DataRow dr = dtManPBSummary.NewRow();
                    dtManPBSummary.Rows.Add(dr);
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        oRU.SetTextBorder(ref sheet, xlsRow, cEmpCategory, dtManPBSummary.Rows[i]["EmpCategory"].ToString());
                        oRU.SetTextBorder(ref sheet, xlsRow, cDepartment, dtManPBSummary.Rows[i]["Department"].ToString());
                        oRU.SetTextBorder(ref sheet, xlsRow, cSection, dtManPBSummary.Rows[i]["SectionName"].ToString());
                        oRU.SetTextBorder(ref sheet, xlsRow, cSubSection, dtManPBSummary.Rows[i]["SubSectionName"].ToString());
                        if (withDesignation)
                        {
                            oRU.SetTextBorder(ref sheet, xlsRow, cAttendancGroup, dtManPBSummary.Rows[i]["DesignationName"].ToString());// 
                        }
                        if (withLine)
                        {
                            oRU.SetTextBorder(ref sheet, xlsRow, cLine, dtManPBSummary.Rows[i]["LineName"].ToString());//
                        }
                        oRU.SetTextBorder(ref sheet, xlsRow, cOnRollManpower, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["TotalManpower"].ToString()));
                        oRU.SetTextBorder(ref sheet, xlsRow, cBudgetedManPower, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["ProposedManpowerBudget"].ToString()));
                        oRU.SetTextBorder(ref sheet, xlsRow, cFdPresent, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_PRESENT"].ToString()));//LegalDesignation
                        oRU.SetTextBorder(ref sheet, xlsRow, cfdAbsent, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Absent"].ToString()));//
                        oRU.SetTextBorder(ref sheet, xlsRow, cfdLate, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Late"].ToString()));//
                        oRU.SetTextBorder(ref sheet, xlsRow, cfdLeave, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Leave"].ToString()));//
                        oRU.SetTextBorder(ref sheet, xlsRow, cfdOthers, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Others"].ToString()));//
                        oRU.SetTextBorder(ref sheet, xlsRow, cfdRemarks, "");//

                        sheet.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Size = 8f;
                        xlsRow++;

                    }
                    //sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, xlsRow, endXlsCol];
                    IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, xlsRow, endXlsCol]);
                    table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;

                    #region UsedRange Alignment
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.CellStyle.Font.Size = 8;
                    sheet.Range["A1"].CellStyle.Font.Size = 14;
                    sheet.Range["A2"].CellStyle.Font.Size = 10;
                    sheet.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet.IsDisplayZeros = false;
                    //sheet.UsedRange["A8"].FreezePanes();
                    sheet.FirstVisibleColumn = 1;
                    sheet.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(identity.PlantId, "", out dsCmp);
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
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet.Range[xlsRow, 1].Text = FactoryName;
                    sheet.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    //sheet.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].Text = FactoryAddress;
                    //sheet.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].Merge();
                    //sheet.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                    //sheet.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    #endregion
                    xlsRow += 1;
                    sheet.Range[xlsRow, xlsCol].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet.UsedRange["A6"].FreezePanes();
                    sheet.FirstVisibleColumn = 1;
                    sheet.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);



                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetSummaryManpowerAttendanceExcelNew1(string companyGroupId, string companyId, string workDate, bool withLine, bool withDesignation, DataTable dtManPBSummary)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                workbook.Worksheets[1].Name = "Data";
                sheet = workbook.Worksheets[1];

                //DataTable dtManPBSummary = GetDailyManpowerAttendanceSummarySql(workDate, withLine, companyGroupId, companyId, PlantIds, typeLists, WithoutTBS, WithoutLA);

                int ROW = 6; int COL = 1;
                int colDesignation = 0;
                int colLine = 0;
                #region columns
                sheet[ROW, COL].Text = "Emp Category"; sheet[ROW, COL].ColumnWidth = 10; int colEC = COL; COL++;
                sheet[ROW, COL].Text = "Department"; sheet[ROW, COL].ColumnWidth = 16; int colDPT = COL; COL++;
                sheet[ROW, COL].Text = "Section"; sheet[ROW, COL].ColumnWidth = 16; int colSection = COL; COL++;
                sheet[ROW, COL].Text = "Sub Section"; sheet[ROW, COL].ColumnWidth = 16; int colSS= COL; COL++;
                if (withLine)
                {
                    sheet[ROW, COL].Text = "Line"; sheet[ROW, COL].ColumnWidth = 8; colLine = COL; COL++; 
                }
                if (withDesignation)
                {
                    sheet[ROW, COL].Text = "Designation"; sheet[ROW, COL].ColumnWidth = 22;  colDesignation = COL; COL++; 
                }
                sheet[ROW, COL].Text = "Budget Code"; sheet[ROW, COL].ColumnWidth = 16; int colBC= COL; COL++;
                sheet[ROW, COL].Text = "Budgeted"; sheet[ROW, COL].ColumnWidth = 8; int colBudgeted = COL; COL++;
                sheet[ROW, COL].Text = "OnRoll"; sheet[ROW, COL].ColumnWidth = 8; int colOnRoll = COL; COL++;
                sheet[ROW, COL].Text = "Present"; sheet[ROW, COL].ColumnWidth = 8; int colPresent = COL; COL++;
                sheet[ROW, COL].Text = "Absent"; sheet[ROW, COL].ColumnWidth = 8; int colAbsent = COL; COL++;
                sheet[ROW, COL].Text = "Late"; sheet[ROW, COL].ColumnWidth = 8; int colLate = COL; COL++;
                sheet[ROW, COL].Text = "Leave"; sheet[ROW, COL].ColumnWidth = 8; int colLeave = COL; COL++;
                sheet[ROW, COL].Text = "Others"; sheet[ROW, COL].ColumnWidth = 8; int colOthers = COL; COL++;
                sheet[ROW, COL].Text = "Remarks"; sheet[ROW, COL].ColumnWidth = 8; int colRemarks = COL;

                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;

                for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                {
                    sheet[ROW, colEC].Text = dtManPBSummary.Rows[i]["EmpCategory"].ToString();
                    sheet[ROW, colDPT].Text = dtManPBSummary.Rows[i]["Department"].ToString();
                    sheet[ROW, colSection].Text = dtManPBSummary.Rows[i]["SectionName"].ToString();
                    sheet[ROW, colSS].Text = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                    if (withDesignation)
                    {
                        sheet[ROW, colDesignation].Text = dtManPBSummary.Rows[i]["DesignationName"].ToString(); 
                    }
                    if (withLine)
                    {
                        sheet[ROW, colLine].Text = dtManPBSummary.Rows[i]["LineName"].ToString(); 
                    }
                    sheet[ROW, colBC].Text = dtManPBSummary.Rows[i]["BudgetCode"].ToString();

                    sheet[ROW, colBudgeted].Number = Library.Service.Extension.clsStaticInfo.dbl(dtManPBSummary.Rows[i]["Budgeted"].ToString());
                    sheet[ROW, colOnRoll].Number = Library.Service.Extension.clsStaticInfo.dbl(dtManPBSummary.Rows[i]["OnRoll"].ToString());
                    sheet[ROW, colPresent].Number = Library.Service.Extension.clsStaticInfo.dbl(dtManPBSummary.Rows[i]["Present"].ToString());
                    sheet[ROW, colAbsent].Number = Library.Service.Extension.clsStaticInfo.dbl(dtManPBSummary.Rows[i]["Absent"].ToString());
                    sheet[ROW, colLate].Number = Library.Service.Extension.clsStaticInfo.dbl(dtManPBSummary.Rows[i]["Late"].ToString());
                    sheet[ROW, colLeave].Number = Library.Service.Extension.clsStaticInfo.dbl(dtManPBSummary.Rows[i]["Leave"].ToString());
                    sheet[ROW, colOthers].Number = Library.Service.Extension.clsStaticInfo.dbl(dtManPBSummary.Rows[i]["Others"].ToString());
                    sheet[ROW, colRemarks].Text = null;

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;

                }
                IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
                table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy"), identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                //#endregion ******************Report Header******************

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



                #region Pivot

                string fPath = fPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + "ManpowerAttendanceSummaryReport" + identity.UserId + ".xlsx";

                workbook.SaveAs(fPath);
                workbook = application.Workbooks.Open(fPath);
                try { System.IO.File.Delete(fPath); } catch (Exception) { }

                workbook.Worksheets[0].Name = "EmpCategory Wise";

                IWorksheet pivotSheet = workbook.Worksheets[0];
                IPivotCache cache = workbook.PivotCaches.Add(workbook.Worksheets[1][startRow - 1, 1, ROW - 1, endCol]);
                IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet["A6"], cache);

                pivotTable.Fields[colEC - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colDPT - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSection - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colSS - 1].Axis = PivotAxisTypes.Row;
                if (withLine)
                {
                    pivotTable.Fields[colLine - 1].Axis = PivotAxisTypes.Row; 
                }
                if (withDesignation)
                {
                    pivotTable.Fields[colDesignation - 1].Axis = PivotAxisTypes.Row; 
                }

                //pivotTable.Fields[colActualDate - 1].Axis = PivotAxisTypes.Column;

                pivotTable.Fields[colBC - 1].Axis = PivotAxisTypes.Row;
                IPivotField field = pivotTable.Fields[colOnRoll - 1];
                field.NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);

                pivotTable.DataFields.Add(field, "OnRoll", PivotSubtotalTypes.Sum);
                //pivotTable.DataFields.Add(field, "Budgeted", PivotSubtotalTypes.Sum);
                //pivotTable.DataFields.Add(field, "Present", PivotSubtotalTypes.Sum);
                //pivotTable.DataFields.Add(field, "Absent", PivotSubtotalTypes.Sum);
                //pivotTable.DataFields.Add(field, "Late", PivotSubtotalTypes.Sum);
                //pivotTable.DataFields.Add(field, "Leave", PivotSubtotalTypes.Sum);
                //pivotTable.DataFields.Add(field, "Others", PivotSubtotalTypes.Sum);

                pivotTable.Fields[colBudgeted - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colPresent - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colAbsent - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colLate - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colLeave - 1].Axis = PivotAxisTypes.Row;
                pivotTable.Fields[colOthers - 1].Axis = PivotAxisTypes.Row;

                for (int i = 0; i < pivotTable.Fields.Count; i++)
                {
                    if (i == colEC - 1 || i == colDPT - 1 || i == colSection - 1 || i == colSS - 1 || i == colLine - 1 || i == colDesignation - 1 || i == colBudgeted - 1 || i == colPresent - 1
                        || i == colAbsent - 1 || i == colLate - 1 || i == colLeave - 1 || i == colOthers  - 1 || i == colBC - 1)
                        pivotTable.Fields[i].Subtotals = PivotSubtotalTypes.None;
                }

                pivotTable.ShowRowGrand = false;
                pivotTable.ShowDrillIndicators = false;
                pivotTable.Options.RowLayout = PivotTableRowLayout.Tabular;
                pivotTable.Options.NullString = "";
                pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;

                sheet = workbook.Worksheets[0];
                reportUtility.CompanyPlantHeaderNew(ref sheet, 1, "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy"), identity.CompanyId, identity.CompanyName, "");

                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;
                workbook.Worksheets[0].UsedRange["A7"].FreezePanes();


                #endregion Buyer Summary
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ManpowerAttendanceSummaryReport" + ".xlsx");
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

        public IWorkbook GetSummaryManpowerAttendanceExcel(string companyGroupId, string companyId, string workDate, bool withLine, bool withDesignation, string PlantIds, string typeLists, bool WithoutTBS, bool WithoutLA)
        {
            try
            {
                #region Variable
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                DataTable dtManPBSummary = GetDailyAttendanceSummarySql(workDate, withLine, companyGroupId, companyId, PlantIds, typeLists, WithoutTBS, WithoutLA);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cUnit = 0; int cSubSection = 0; int cAttendancGroup = 0; int cOnRollManpower; int cBudgetedManPower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdOthers = 0; var cfdRemarks = 0; int cDivision = 0; int cEmpCategory = 0; int cSection = 0; int cDepartment = 0; int cLine = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Division", ExcelHAlign.HAlignCenter); cDivision = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Unit", ExcelHAlign.HAlignCenter); cUnit = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Emp Category", ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Department", ExcelHAlign.HAlignCenter); cDepartment = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Section", ExcelHAlign.HAlignCenter); cSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sub Section", ExcelHAlign.HAlignCenter); cSubSection = xlsCol; xlsCol++;
                if (withDesignation)
                {
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                }
                if (withLine)
                {
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Line", ExcelHAlign.HAlignCenter); cLine = xlsCol; xlsCol++;
                }
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted", 8, ExcelHAlign.HAlignCenter); cBudgetedManPower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 8, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 8, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 8, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 8, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 8, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Others", 8, ExcelHAlign.HAlignCenter); cfdOthers = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarkes", 10, ExcelHAlign.HAlignCenter); cfdRemarks = xlsCol;

                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                #endregion

                if (dtManPBSummary.Rows.Count > 0)
                {


                    #region New

                    string _cgrp1 = string.Empty;
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _grp3 = string.Empty;
                    string _sgrp3 = string.Empty;
                    string _grp4 = string.Empty;
                    string _grp5 = string.Empty;



                    var catFRow = xlsRow;
                    var catcGrp2FRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catsGrp3FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;
                    var catGrp5FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;

                    Dictionary<string, Combination> dicGroup = new Dictionary<string, Combination>();

                    string strGroupDivisionName = /*strGroupEmpCategory +*/ dtManPBSummary.Rows[0]["DivisionName"].ToString();
                    string strGroupUnitName = strGroupDivisionName + dtManPBSummary.Rows[0]["UnitName"].ToString();
                    string strGroupEmpCategory = strGroupUnitName + dtManPBSummary.Rows[0]["EmpCategory"].ToString();
                    string strGroupDepartment = strGroupEmpCategory + dtManPBSummary.Rows[0]["Department"].ToString();
                    string strGroupSectionName = strGroupDepartment + dtManPBSummary.Rows[0]["SectionName"].ToString();
                    string strGroupSubSectionName = strGroupSectionName + dtManPBSummary.Rows[0]["SubSectionName"].ToString();



                    dicGroup.Add("DivisionName", new Combination { GroupKey = strGroupDivisionName, Row = xlsRow });
                    dicGroup.Add("UnitName", new Combination { GroupKey = strGroupUnitName, Row = xlsRow });
                    dicGroup.Add("EmpCategory", new Combination { GroupKey = strGroupEmpCategory, Row = xlsRow });
                    dicGroup.Add("Department", new Combination { GroupKey = strGroupDepartment, Row = xlsRow });
                    dicGroup.Add("SectionName", new Combination { GroupKey = strGroupSectionName, Row = xlsRow });
                    dicGroup.Add("SubSectionName", new Combination { GroupKey = strGroupSubSectionName, Row = xlsRow });


                    DataRow dr = dtManPBSummary.NewRow();
                    dtManPBSummary.Rows.Add(dr);
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;
                        if (i == 100)
                        {

                        }
                        strGroupDivisionName =/* strGroupEmpCategory +*/ dtManPBSummary.Rows[i]["DivisionName"].ToString();
                        strGroupUnitName = strGroupDivisionName + dtManPBSummary.Rows[i]["UnitName"].ToString();
                        strGroupEmpCategory = strGroupUnitName + dtManPBSummary.Rows[i]["EmpCategory"].ToString();
                        strGroupDepartment = strGroupEmpCategory + dtManPBSummary.Rows[i]["Department"].ToString();
                        strGroupSectionName = strGroupDepartment + dtManPBSummary.Rows[i]["SectionName"].ToString();
                        strGroupSubSectionName = strGroupSectionName + dtManPBSummary.Rows[i]["SubSectionName"].ToString();

                        if (dicGroup["DivisionName"].GroupKey != strGroupDivisionName)
                        {
                            rowList.Add(xlsRow);
                            SetHeadText(sheet1, xlsRow, 6, " Subtotal:");
                            sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;


                            xlsRow++;


                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision, xlsRow - 1, cDivision].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision, xlsRow - 1, cDivision].Merge();


                            dicGroup["DivisionName"].Row = xlsRow;
                            dicGroup["DivisionName"].GroupKey = strGroupDivisionName;
                        }



                        sheet1.Range[xlsRow, cUnit].Text = dtManPBSummary.Rows[i]["UnitName"].ToString();
                        if (dicGroup["UnitName"].GroupKey != strGroupUnitName)
                        {
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit, xlsRow - 1, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit, xlsRow - 1, cUnit].Merge();
                            dicGroup["UnitName"].Row = xlsRow;
                            dicGroup["UnitName"].GroupKey = strGroupUnitName;
                        }
                        sheet1.Range[xlsRow, cEmpCategory].Text = dtManPBSummary.Rows[i]["EmpCategory"].ToString();
                        if (dicGroup["EmpCategory"].GroupKey != strGroupEmpCategory)
                        {
                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory, xlsRow - 1, cEmpCategory].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory, xlsRow - 1, cEmpCategory].Merge();
                            dicGroup["EmpCategory"].Row = xlsRow;
                            dicGroup["EmpCategory"].GroupKey = strGroupEmpCategory;
                        }

                        sheet1.Range[xlsRow, cDepartment].Text = dtManPBSummary.Rows[i]["Department"].ToString();
                        if (dicGroup["Department"].GroupKey != strGroupDepartment)
                        {
                            sheet1.Range[dicGroup["Department"].Row, cDepartment, xlsRow - 1, cDepartment].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["Department"].Row, cDepartment].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["Department"].Row, cDepartment].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["Department"].Row, cDepartment, xlsRow - 1, cDepartment].Merge();
                            dicGroup["Department"].Row = xlsRow;
                            dicGroup["Department"].GroupKey = strGroupDepartment;
                        }

                        sheet1.Range[xlsRow, cSection].Text = dtManPBSummary.Rows[i]["SectionName"].ToString();
                        if (dicGroup["SectionName"].GroupKey != strGroupSectionName)
                        {
                            sheet1.Range[dicGroup["SectionName"].Row, cSection, xlsRow - 1, cSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["SectionName"].Row, cSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["SectionName"].Row, cSection].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["SectionName"].Row, cSection, xlsRow - 1, cSection].Merge();
                            dicGroup["SectionName"].Row = xlsRow;
                            dicGroup["SectionName"].GroupKey = strGroupSectionName;
                        }

                        sheet1.Range[xlsRow, cSubSection].Text = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                        if (dicGroup["SubSectionName"].GroupKey != strGroupSubSectionName)
                        {
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection, xlsRow - 1, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection, xlsRow - 1, cSubSection].Merge();
                            dicGroup["SubSectionName"].Row = xlsRow;
                            dicGroup["SubSectionName"].GroupKey = strGroupSubSectionName;
                        }



                        sheet1.Range[xlsRow, cDivision].Text = dtManPBSummary.Rows[i]["DivisionName"].ToString();

                        #endregion

                        #region Old
                        //    string _grp1 = string.Empty;
                        //string _grp2 = string.Empty;
                        //string _grp3 = string.Empty;
                        //string _grp4 = string.Empty;
                        //string _grp5 = string.Empty;
                        //string _grp6 = string.Empty;


                        //var catFRow = xlsRow;
                        //var catGrp2FRow = xlsRow;
                        //var catGrp3FRow = xlsRow;
                        //var catGrp4FRow = xlsRow;
                        //var catGrp5FRow = xlsRow;
                        //var catGrp6FRow = xlsRow;

                        //ArrayList rowList = new ArrayList();
                        //var lastMPGroup = string.Empty;
                        //for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                        //{
                        //    var catLRow = xlsRow;
                        //    if (_grp1 != dtManPBSummary.Rows[i]["DivisionName"].ToString() && string.IsNullOrEmpty(dtManPBSummary.Rows[i]["DivisionName"].ToString()) == false)
                        //    {
                        //        _grp1 = dtManPBSummary.Rows[i]["DivisionName"].ToString();

                        //        #region Subtotal
                        //        if (catFRow < xlsRow)
                        //        {
                        //            lastMPGroup = _grp1;
                        //            rowList.Add(xlsRow);
                        //            SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                        //            sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                        //            sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                        //            sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                        //            sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                        //            sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                        //            sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                        //            sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                        //            sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";

                        //            sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                        //            sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                        //            sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                        //            sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                        //            sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                        //            sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                        //            sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                        //            sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                        //            sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);
                        //            //sheet1.Range[xlsRow, cfdRemarks].BorderAround(ExcelLineStyle.Hair);


                        //            xlsRow++;
                        //        }
                        //        #endregion

                        //        sheet1.Range[xlsRow, cDivision].Text = _grp1;
                        //        sheet1.Range[xlsRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                        //        sheet1.Range[xlsRow, cDivision].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                        //        sheet1.Range[xlsRow, cDivision].VerticalAlignment = ExcelVAlign.VAlignTop;


                        //        _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cUnit, _grp2);

                        //        _grp5 = dtManPBSummary.Rows[i]["EmpCategory"].ToString();
                        //        SetCellText(sheet1, xlsRow, cEmpCategory, _grp5);

                        //        _grp6 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cSection, _grp6);


                        //        _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                        //        _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);

                        //        if (catFRow < xlsRow)
                        //        {
                        //            catFRow = xlsRow;
                        //            catGrp2FRow = xlsRow;
                        //            catGrp3FRow = xlsRow;
                        //            catGrp5FRow = xlsRow;
                        //            catGrp6FRow = xlsRow;

                        //        }
                        //    }
                        //    else if (_grp2 != dtManPBSummary.Rows[i]["UnitName"].ToString())
                        //    {
                        //        _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                        //        //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                        //        sheet1.Range[xlsRow, cUnit].Text = _grp2;
                        //        sheet1.Range[xlsRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                        //        sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                        //        sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignTop;

                        //        _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                        //        _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                        //        if (catGrp2FRow < xlsRow)
                        //        {
                        //            catGrp2FRow = xlsRow;
                        //            catGrp3FRow = xlsRow;
                        //            catGrp5FRow = xlsRow;
                        //            catGrp6FRow = xlsRow;

                        //        }
                        //    }

                        //    else if (_grp5 != dtManPBSummary.Rows[i]["EmpCategory"].ToString())
                        //    {
                        //        _grp5 = dtManPBSummary.Rows[i]["EmpCategory"].ToString();
                        //        //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                        //        sheet1.Range[xlsRow, cEmpCategory].Text = _grp5;
                        //        sheet1.Range[xlsRow, cEmpCategory, xlsRow, cEmpCategory].BorderAround(ExcelLineStyle.Hair);
                        //        sheet1.Range[xlsRow, cEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                        //        sheet1.Range[xlsRow, cEmpCategory].VerticalAlignment = ExcelVAlign.VAlignTop;


                        //        _grp6 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cSection, _grp6);
                        //        _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                        //        _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                        //        if (catGrp5FRow < xlsRow)
                        //        {
                        //            catGrp5FRow = xlsRow;
                        //            catGrp6FRow = xlsRow;
                        //            catGrp3FRow = xlsRow;
                        //        }
                        //    }

                        //    else if (_grp6 != dtManPBSummary.Rows[i]["SectionName"].ToString())
                        //    {
                        //        _grp6 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                        //        //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                        //        sheet1.Range[xlsRow, cSection].Text = _grp6;
                        //        sheet1.Range[xlsRow, cSection, xlsRow, cSection].BorderAround(ExcelLineStyle.Hair);
                        //        sheet1.Range[xlsRow, cSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                        //        sheet1.Range[xlsRow, cSection].VerticalAlignment = ExcelVAlign.VAlignTop;


                        //        //_grp6 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                        //        //SetCellText(sheet1, xlsRow, cSection, _grp6);
                        //        _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                        //        _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                        //        if (catGrp6FRow < xlsRow)
                        //        {
                        //            //catGrp6FRow = xlsRow;
                        //            catGrp6FRow = xlsRow;
                        //            catGrp3FRow = xlsRow;
                        //        }
                        //    }

                        //    else if (_grp3 != dtManPBSummary.Rows[i]["SubSectionName"].ToString())
                        //    {
                        //        _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                        //        //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                        //        sheet1.Range[xlsRow, cSubSection].Text = _grp3;
                        //        sheet1.Range[xlsRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);
                        //        sheet1.Range[xlsRow, cSubSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                        //        sheet1.Range[xlsRow, cSubSection].VerticalAlignment = ExcelVAlign.VAlignTop;

                        //        _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                        //        if (catGrp3FRow < xlsRow)
                        //        {
                        //            catGrp3FRow = xlsRow;
                        //        }
                        //    }
                        //    else if (_grp4 != dtManPBSummary.Rows[i]["DesignationName"].ToString())
                        //    {

                        //        _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                        //        SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);

                        //        sheet1.Range[catFRow, cDivision, xlsRow, cDivision].Merge();
                        //        sheet1.Range[catFRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                        //        sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].Merge();
                        //        sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                        //        sheet1.Range[catGrp5FRow, cEmpCategory, xlsRow, cEmpCategory].Merge();
                        //        sheet1.Range[catGrp5FRow, cEmpCategory, xlsRow, cEmpCategory].BorderAround(ExcelLineStyle.Hair);
                        //        sheet1.Range[catGrp6FRow, cSection, xlsRow, cSection].Merge();
                        //        sheet1.Range[catGrp6FRow, cSection, xlsRow, cSection].BorderAround(ExcelLineStyle.Hair);
                        //        sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].Merge();
                        //        sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);



                        //    }
                        #endregion

                        if (withDesignation)
                        {
                            oRU.SetTextBorder(ref sheet1, xlsRow, cAttendancGroup, dtManPBSummary.Rows[i]["DesignationName"].ToString());// 
                        }
                        if (withLine)
                        {
                            oRU.SetTextBorder(ref sheet1, xlsRow, cLine, dtManPBSummary.Rows[i]["LineName"].ToString());//
                        }
                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["TotalManpower"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cBudgetedManPower, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["ProposedManpowerBudget"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_PRESENT"].ToString()));//LegalDesignation
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Absent"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Late"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Leave"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdOthers, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Others"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdRemarks, "");//
                        xlsRow++;
                    }
                    xlsRow += 1;

                    //rowList.Add(xlsRow);
                    //SetHeadText(sheet1, xlsRow, 1, " Subtotal:");

                    //sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    //sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);

                    //sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                    //sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);

                    //sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                    //sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);

                    //sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;
                    //xlsRow++;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = GetFormulaGrandTotal(rowList, cBudgetedManPower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdOthers].Formula = GetFormulaGrandTotal(rowList, cfdOthers);


                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);



                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(identity.PlantId, "", out dsCmp);
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
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].Text = FactoryAddress;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook GetSummaryManpowerAttendanceExcelWithLine(string companyGroupId, string companyId, string PlantId, string workDate, bool withLine, string typeLists, bool WithoutTBS, bool WithoutLA)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                DataTable dtManPBSummary = GetDailyAttendanceSummarySql(workDate, withLine, companyGroupId, companyId, PlantId, typeLists, WithoutTBS, WithoutLA);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cUnit = 0; int cSubSection = 0; int cLine = 0; int cAttendancGroup = 0; int cOnRollManpower; int cBudgetedManPower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdOthers = 0; var cfdRemarks = 0; int cDivision = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Division", ExcelHAlign.HAlignCenter); cDivision = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Unit", ExcelHAlign.HAlignCenter); cUnit = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sub Section", ExcelHAlign.HAlignCenter); cSubSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Line", ExcelHAlign.HAlignCenter); cLine = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted", 8, ExcelHAlign.HAlignCenter); cBudgetedManPower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 8, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 8, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 8, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 8, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 8, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Others", 8, ExcelHAlign.HAlignCenter); cfdOthers = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarkes", 10, ExcelHAlign.HAlignCenter); cfdRemarks = xlsCol;

                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                if (dtManPBSummary.Rows.Count > 0)
                {
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _grp3 = string.Empty;
                    string _grp4 = string.Empty;
                    string _grp5 = string.Empty;


                    #endregion
                    var catFRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;
                    var catGrp5FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;
                        if (_grp1 != dtManPBSummary.Rows[i]["DivisionName"].ToString() && string.IsNullOrEmpty(dtManPBSummary.Rows[i]["DivisionName"].ToString()) == false)
                        {
                            _grp1 = dtManPBSummary.Rows[i]["DivisionName"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastMPGroup = _grp1;
                                rowList.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                                sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);
                                //sheet1.Range[xlsRow, cfdRemarks].BorderAround(ExcelLineStyle.Hair);


                                xlsRow++;
                            }
                            #endregion

                            sheet1.Range[xlsRow, cDivision].Text = _grp1;
                            sheet1.Range[xlsRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cDivision].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cDivision].VerticalAlignment = ExcelVAlign.VAlignTop;


                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            SetCellText(sheet1, xlsRow, cUnit, _grp2);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["LineName"].ToString();
                            SetCellText(sheet1, xlsRow, cLine, _grp4);
                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);
                            if (catFRow < xlsRow)
                            {
                                catFRow = xlsRow;
                                catGrp2FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;


                            }
                        }
                        else if (_grp2 != dtManPBSummary.Rows[i]["UnitName"].ToString())
                        {
                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cUnit].Text = _grp2;
                            sheet1.Range[xlsRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["LineName"].ToString();
                            SetCellText(sheet1, xlsRow, cLine, _grp4);
                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);
                            if (catGrp2FRow < xlsRow)
                            {
                                catGrp2FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;


                            }
                        }
                        else if (_grp3 != dtManPBSummary.Rows[i]["SubSectionName"].ToString())
                        {
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cSubSection].Text = _grp3;
                            sheet1.Range[xlsRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cSubSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cSubSection].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp4 = dtManPBSummary.Rows[i]["LineName"].ToString();
                            SetCellText(sheet1, xlsRow, cLine, _grp4);
                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);
                            if (catGrp3FRow < xlsRow)
                            {
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;
                            }
                        }
                        else if (_grp4 != dtManPBSummary.Rows[i]["LineName"].ToString())
                        {

                            _grp4 = dtManPBSummary.Rows[i]["LineName"].ToString();

                            sheet1.Range[xlsRow, cLine].Text = _grp4;
                            sheet1.Range[xlsRow, cLine, xlsRow, cLine].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignTop;
                            //sheet1.Range[catFRow, cDivision, xlsRow, cDivision].Merge();
                            //sheet1.Range[catFRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                            //sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].Merge();
                            //sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            //sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].Merge();
                            //sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);
                            if (catGrp4FRow < xlsRow)
                            {
                                catGrp4FRow = xlsRow;
                            }

                        }
                        else if (_grp5 != dtManPBSummary.Rows[i]["DesignationName"].ToString())
                        {

                            _grp5 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp5);

                            sheet1.Range[catFRow, cDivision, xlsRow, cDivision].Merge();
                            sheet1.Range[catFRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].Merge();
                            sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].Merge();
                            sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[catGrp4FRow, cLine, xlsRow, cLine].Merge();
                            sheet1.Range[catGrp4FRow, cLine, xlsRow, cLine].BorderAround(ExcelLineStyle.Hair);


                        }
                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, Convert.ToInt32(dtManPBSummary.Rows[i]["TotalManpower"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cBudgetedManPower, Convert.ToInt32(dtManPBSummary.Rows[i]["ProposedManpowerBudget"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, Convert.ToInt32(dtManPBSummary.Rows[i]["SUM_PRESENT"].ToString()));//LegalDesignation
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, Convert.ToInt32(dtManPBSummary.Rows[i]["SUM_Absent"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, Convert.ToInt32(dtManPBSummary.Rows[i]["SUM_Late"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, Convert.ToInt32(dtManPBSummary.Rows[i]["SUM_Leave"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdOthers, Convert.ToInt32(dtManPBSummary.Rows[i]["SUM_Others"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdRemarks, "");//
                        xlsRow++;
                    }
                    xlsRow += 1;

                    rowList.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");

                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;
                    xlsRow++;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = GetFormulaGrandTotal(rowList, cBudgetedManPower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdOthers].Formula = GetFormulaGrandTotal(rowList, cfdOthers);


                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);



                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(identity.PlantId, "", out dsCmp);
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
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].Text = FactoryAddress;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IWorkbook GetSummaryManpowerAttendanceExcelNew1(string companyGroupId, string companyId, string PlantId, string workDate, bool withLine)
        {
            try
            {
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                DataTable dtManPBSummary = GetDailyAttendanceSummarySqlNew(workDate, withLine, companyGroupId, companyId, PlantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cUnit, cSubSection, cSection, cEmpCategory, cAttendancGroup, cOnRollManpower, cBudgetedManPower, cFdPresent, cfdAbsent, cfdLeave, cfdLate, cfdOthers = 0; var cfdRemarks = 0; int cDivision = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Category", 8, ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Division", ExcelHAlign.HAlignCenter); cDivision = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Unit", ExcelHAlign.HAlignCenter); cUnit = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Section", ExcelHAlign.HAlignCenter); cSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sub Section", ExcelHAlign.HAlignCenter); cSubSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted", 8, ExcelHAlign.HAlignCenter); cBudgetedManPower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 8, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 8, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 8, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 8, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 8, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Others", 8, ExcelHAlign.HAlignCenter); cfdOthers = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarkes", 10, ExcelHAlign.HAlignCenter); cfdRemarks = xlsCol;
                #endregion
                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                if (dtManPBSummary.Rows.Count > 0)
                {
                    string _grp1 = string.Empty;
                    string _cgrp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _sgrp3 = string.Empty;
                    string _grp3 = string.Empty;
                    string _grp4 = string.Empty;


                    var catFRow = xlsRow;
                    var catcGrp2FRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catsGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;
                        if (_cgrp1 != dtManPBSummary.Rows[i]["EmpCategory"].ToString() && string.IsNullOrEmpty(dtManPBSummary.Rows[i]["EmpCategory"].ToString()) == false)
                        {
                            _cgrp1 = dtManPBSummary.Rows[i]["EmpCategory"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastMPGroup = _cgrp1;
                                rowList.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                                sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);
                                //sheet1.Range[xlsRow, cfdRemarks].BorderAround(ExcelLineStyle.Hair);


                                xlsRow++;
                            }
                            #endregion

                            sheet1.Range[xlsRow, cEmpCategory].Text = _cgrp1;
                            sheet1.Range[xlsRow, cEmpCategory, xlsRow, cEmpCategory].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cEmpCategory].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp1 = dtManPBSummary.Rows[i]["DivisionName"].ToString();
                            SetCellText(sheet1, xlsRow, cDivision, _grp1);
                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            SetCellText(sheet1, xlsRow, cUnit, _grp2);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _sgrp3);
                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                            if (catFRow < xlsRow)
                            {
                                catFRow = xlsRow;
                                catcGrp2FRow = xlsRow;
                                catGrp2FRow = xlsRow;
                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;


                            }
                        }
                        else if (_grp1 != dtManPBSummary.Rows[i]["DivisionName"].ToString())
                        {
                            _grp1 = dtManPBSummary.Rows[i]["DivisionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cDivision].Text = _grp1;
                            sheet1.Range[xlsRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cDivision].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cDivision].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            SetCellText(sheet1, xlsRow, cUnit, _grp2);
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _sgrp3);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);

                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                            if (catGrp2FRow < xlsRow)
                            {
                                //catGrp2FRow = xlsRow;
                                //catsGrp3FRow = xlsRow;
                                //catGrp3FRow = xlsRow;
                                //catGrp4FRow = xlsRow;


                                catGrp2FRow = xlsRow;
                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;

                            }
                        }
                        else if (_grp2 != dtManPBSummary.Rows[i]["UnitName"].ToString())
                        {
                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cUnit].Text = _grp2;
                            sheet1.Range[xlsRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignTop;
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _sgrp3);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                            if (catGrp2FRow < xlsRow)
                            {
                                //catGrp2FRow = xlsRow;
                                //catGrp3FRow = xlsRow;


                                catsGrp3FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;

                            }
                        }

                        else if (_sgrp3 != dtManPBSummary.Rows[i]["SectionName"].ToString())
                        {
                            _sgrp3 = dtManPBSummary.Rows[i]["SectionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cSection].Text = _sgrp3;
                            sheet1.Range[xlsRow, cSection, xlsRow, cSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cSection].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);

                            if (catsGrp3FRow < xlsRow)
                            {
                                //catsGrp3FRow = xlsRow;
                                //catGrp3FRow = xlsRow;
                                //catGrp4FRow = xlsRow;

                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;
                            }
                        }
                        else if (_grp3 != dtManPBSummary.Rows[i]["SubSectionName"].ToString())
                        {
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cSubSection].Text = _grp3;
                            sheet1.Range[xlsRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cSubSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cSubSection].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                            if (catGrp3FRow < xlsRow)
                            {
                                catGrp3FRow = xlsRow;
                            }
                        }
                        else if (_grp4 != dtManPBSummary.Rows[i]["DesignationName"].ToString())
                        {

                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);


                            sheet1.Range[catFRow, cEmpCategory, xlsRow, cEmpCategory].Merge();
                            sheet1.Range[catFRow, cEmpCategory, xlsRow, cEmpCategory].BorderAround(ExcelLineStyle.Hair);


                            sheet1.Range[catcGrp2FRow, cDivision, xlsRow, cDivision].Merge();
                            sheet1.Range[catcGrp2FRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].Merge();
                            sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);


                            sheet1.Range[catsGrp3FRow, cSection, xlsRow, cSection].Merge();
                            sheet1.Range[catsGrp3FRow, cSection, xlsRow, cSection].BorderAround(ExcelLineStyle.Hair);


                            sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].Merge();
                            sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);

                        }

                        oRU.SetTextBorder(ref sheet1, xlsRow, cBudgetedManPower, Convert.ToInt32(dtManPBSummary.Rows[i]["ProposedManpowerBudget"].ToString()));

                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, Convert.ToInt32(dtManPBSummary.Rows[i]["TotalManpower"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_PRESENT"].ToString()));//LegalDesignation
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Absent"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Late"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Leave"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdOthers, Convert.ToDouble(dtManPBSummary.Rows[i]["SUM_Others"].ToString()));//

                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdRemarks, "");//
                        xlsRow++;
                    }
                    xlsRow += 1;

                    rowList.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");

                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;
                    xlsRow++;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = GetFormulaGrandTotal(rowList, cBudgetedManPower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdOthers].Formula = GetFormulaGrandTotal(rowList, cfdOthers);


                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);



                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(PlantId, "", out dsCmp);
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
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].Text = FactoryAddress;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private class Combination
        {
            public string GroupKey { get; set; } = "";
            public int Row { get; set; } = 0;
        }
        public IWorkbook GetSummaryManpowerAttendanceExcelNew(string companyGroupId, string companyId, string PlantId, string workDate, bool withLine)
        {
            try
            {
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                //DataTable dtManPBSummary = GetDailyAttendanceSummarySql(workDate, withLine, companyGroupId, companyId, PlantId);
                DataTable dtManPBSummary = GetDailyAttendanceSummarySqlNew(workDate, withLine, companyGroupId, companyId, PlantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cUnit = 0; int cSubSection, cSection, cEmpCategory = 0; int cAttendancGroup = 0; int cOnRollManpower; int cBudgetedManPower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdOthers = 0; var cfdRemarks = 0; int cDivision = 0;
                #endregion

                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Category", 8, ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Division", ExcelHAlign.HAlignCenter); cDivision = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Unit", ExcelHAlign.HAlignCenter); cUnit = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Section", ExcelHAlign.HAlignCenter); cSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sub Section", ExcelHAlign.HAlignCenter); cSubSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted", 8, ExcelHAlign.HAlignCenter); cBudgetedManPower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 8, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 8, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 8, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 8, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 8, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Others", 8, ExcelHAlign.HAlignCenter); cfdOthers = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarkes", 10, ExcelHAlign.HAlignCenter); cfdRemarks = xlsCol;
                #endregion

                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                if (dtManPBSummary.Rows.Count > 0)
                {
                    string _cgrp1 = string.Empty;
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _grp3 = string.Empty;
                    string _sgrp3 = string.Empty;
                    string _grp4 = string.Empty;
                    string _grp5 = string.Empty;



                    var catFRow = xlsRow;
                    var catcGrp2FRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catsGrp3FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;
                    var catGrp5FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;

                    Dictionary<string, Combination> dicGroup = new Dictionary<string, Combination>();

                    string strGroupEmpCategory = dtManPBSummary.Rows[0]["EmpCategory"].ToString();
                    string strGroupDivisionName = strGroupEmpCategory + dtManPBSummary.Rows[0]["DivisionName"].ToString();
                    string strGroupUnitName = strGroupDivisionName + dtManPBSummary.Rows[0]["UnitName"].ToString();
                    string strGroupSectionName = strGroupUnitName + dtManPBSummary.Rows[0]["SectionName"].ToString();
                    string strGroupSubSectionName = strGroupSectionName + dtManPBSummary.Rows[0]["SubSectionName"].ToString();



                    dicGroup.Add("EmpCategory", new Combination { GroupKey = strGroupEmpCategory, Row = xlsRow });
                    dicGroup.Add("DivisionName", new Combination { GroupKey = strGroupDivisionName, Row = xlsRow });
                    dicGroup.Add("UnitName", new Combination { GroupKey = strGroupUnitName, Row = xlsRow });
                    dicGroup.Add("SectionName", new Combination { GroupKey = strGroupSectionName, Row = xlsRow });
                    dicGroup.Add("SubSectionName", new Combination { GroupKey = strGroupSubSectionName, Row = xlsRow });


                    DataRow dr = dtManPBSummary.NewRow(); dtManPBSummary.Rows.Add(dr);
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;

                        if (i == 140)
                        {

                        }
                        strGroupEmpCategory = dtManPBSummary.Rows[i]["EmpCategory"].ToString();
                        strGroupDivisionName = strGroupEmpCategory + dtManPBSummary.Rows[i]["DivisionName"].ToString();
                        strGroupUnitName = strGroupDivisionName + dtManPBSummary.Rows[i]["UnitName"].ToString();
                        strGroupSectionName = strGroupUnitName + dtManPBSummary.Rows[i]["SectionName"].ToString();
                        strGroupSubSectionName = strGroupSectionName + dtManPBSummary.Rows[i]["SubSectionName"].ToString();

                        if (dicGroup["EmpCategory"].GroupKey != strGroupEmpCategory)
                        {
                            rowList.Add(xlsRow);
                            SetHeadText(sheet1, xlsRow, 6, " Subtotal:");
                            sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;


                            xlsRow++;


                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory, xlsRow - 1, cEmpCategory].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory, xlsRow - 1, cEmpCategory].Merge();


                            dicGroup["EmpCategory"].Row = xlsRow;
                            dicGroup["EmpCategory"].GroupKey = strGroupEmpCategory;
                        }

                        sheet1.Range[xlsRow, cDivision].Text = dtManPBSummary.Rows[i]["DivisionName"].ToString();
                        if (dicGroup["DivisionName"].GroupKey != strGroupDivisionName)
                        {
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision, xlsRow - 1, cDivision].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision, xlsRow - 1, cDivision].Merge();
                            dicGroup["DivisionName"].Row = xlsRow;
                            dicGroup["DivisionName"].GroupKey = strGroupDivisionName;
                        }

                        sheet1.Range[xlsRow, cUnit].Text = dtManPBSummary.Rows[i]["UnitName"].ToString();
                        if (dicGroup["UnitName"].GroupKey != strGroupUnitName)
                        {
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit, xlsRow - 1, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit, xlsRow - 1, cUnit].Merge();
                            dicGroup["UnitName"].Row = xlsRow;
                            dicGroup["UnitName"].GroupKey = strGroupUnitName;
                        }

                        sheet1.Range[xlsRow, cSection].Text = dtManPBSummary.Rows[i]["SectionName"].ToString();
                        if (dicGroup["SectionName"].GroupKey != strGroupSectionName)
                        {
                            sheet1.Range[dicGroup["SectionName"].Row, cSection, xlsRow - 1, cSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["SectionName"].Row, cSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["SectionName"].Row, cSection].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["SectionName"].Row, cSection, xlsRow - 1, cSection].Merge();
                            dicGroup["SectionName"].Row = xlsRow;
                            dicGroup["SectionName"].GroupKey = strGroupSectionName;
                        }

                        sheet1.Range[xlsRow, cSubSection].Text = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                        if (dicGroup["SubSectionName"].GroupKey != strGroupSubSectionName)
                        {
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection, xlsRow - 1, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection, xlsRow - 1, cSubSection].Merge();
                            dicGroup["SubSectionName"].Row = xlsRow;
                            dicGroup["SubSectionName"].GroupKey = strGroupSubSectionName;
                        }



                        sheet1.Range[xlsRow, cEmpCategory].Text = dtManPBSummary.Rows[i]["EmpCategory"].ToString();

                        oRU.SetTextBorder(ref sheet1, xlsRow, cAttendancGroup, dtManPBSummary.Rows[i]["DesignationName"].ToString());//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cBudgetedManPower, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["ProposedManpowerBudget"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["TotalManpower"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_PRESENT"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Absent"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Late"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Leave"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdOthers, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Others"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdRemarks, "");//




                        xlsRow++;
                    }
                    //xlsRow += 1;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = GetFormulaGrandTotal(rowList, cBudgetedManPower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdOthers].Formula = GetFormulaGrandTotal(rowList, cfdOthers);


                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);



                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(PlantId, "", out dsCmp);
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
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }

                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IWorkbook GetSummaryManpowerAttendanceExcelWithLineNew(string companyGroupId, string companyId, string PlantId, string workDate, bool withLine)
        {
            try
            {
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                //DataTable dtManPBSummary = GetDailyAttendanceSummarySql(workDate, withLine, companyGroupId, companyId, PlantId);
                DataTable dtManPBSummary = GetDailyAttendanceSummarySqlNew(workDate, withLine, companyGroupId, companyId, PlantId);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cUnit = 0; int cSubSection, cSection, cEmpCategory = 0; int cLine = 0; int cAttendancGroup = 0; int cOnRollManpower; int cBudgetedManPower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdOthers = 0; var cfdRemarks = 0; int cDivision = 0;
                #endregion

                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Category", 8, ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Division", ExcelHAlign.HAlignCenter); cDivision = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Unit", ExcelHAlign.HAlignCenter); cUnit = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Section", ExcelHAlign.HAlignCenter); cSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sub Section", ExcelHAlign.HAlignCenter); cSubSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Line", ExcelHAlign.HAlignCenter); cLine = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted", 8, ExcelHAlign.HAlignCenter); cBudgetedManPower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 8, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 8, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 8, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 8, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 8, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Others", 8, ExcelHAlign.HAlignCenter); cfdOthers = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarkes", 10, ExcelHAlign.HAlignCenter); cfdRemarks = xlsCol;
                #endregion

                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                if (dtManPBSummary.Rows.Count > 0)
                {
                    string _cgrp1 = string.Empty;
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _grp3 = string.Empty;
                    string _sgrp3 = string.Empty;
                    string _grp4 = string.Empty;
                    string _grp5 = string.Empty;



                    var catFRow = xlsRow;
                    var catcGrp2FRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catsGrp3FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;
                    var catGrp5FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;

                    Dictionary<string, Combination> dicGroup = new Dictionary<string, Combination>();

                    string strGroupEmpCategory = dtManPBSummary.Rows[0]["EmpCategory"].ToString();
                    string strGroupDivisionName = strGroupEmpCategory + dtManPBSummary.Rows[0]["DivisionName"].ToString();
                    string strGroupUnitName = strGroupDivisionName + dtManPBSummary.Rows[0]["UnitName"].ToString();
                    string strGroupSectionName = strGroupUnitName + dtManPBSummary.Rows[0]["SectionName"].ToString();
                    string strGroupSubSectionName = strGroupSectionName + dtManPBSummary.Rows[0]["SubSectionName"].ToString();
                    string strGroupLineName = strGroupSubSectionName + dtManPBSummary.Rows[0]["LineName"].ToString();



                    dicGroup.Add("EmpCategory", new Combination { GroupKey = strGroupEmpCategory, Row = xlsRow });
                    dicGroup.Add("DivisionName", new Combination { GroupKey = strGroupDivisionName, Row = xlsRow });
                    dicGroup.Add("UnitName", new Combination { GroupKey = strGroupUnitName, Row = xlsRow });
                    dicGroup.Add("SectionName", new Combination { GroupKey = strGroupSectionName, Row = xlsRow });
                    dicGroup.Add("SubSectionName", new Combination { GroupKey = strGroupSubSectionName, Row = xlsRow });
                    dicGroup.Add("LineName", new Combination { GroupKey = strGroupLineName, Row = xlsRow });


                    DataRow dr = dtManPBSummary.NewRow(); dtManPBSummary.Rows.Add(dr);
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;


                        strGroupEmpCategory = dtManPBSummary.Rows[i]["EmpCategory"].ToString();
                        strGroupDivisionName = strGroupEmpCategory + dtManPBSummary.Rows[i]["DivisionName"].ToString();
                        strGroupUnitName = strGroupDivisionName + dtManPBSummary.Rows[i]["UnitName"].ToString();
                        strGroupSectionName = strGroupUnitName + dtManPBSummary.Rows[i]["SectionName"].ToString();
                        strGroupSubSectionName = strGroupSectionName + dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                        strGroupLineName = strGroupSubSectionName + dtManPBSummary.Rows[i]["LineName"].ToString();

                        if (dicGroup["EmpCategory"].GroupKey != strGroupEmpCategory)
                        {
                            rowList.Add(xlsRow);
                            SetHeadText(sheet1, xlsRow, 7, " Subtotal:");
                            sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";
                            sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);

                            sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;


                            xlsRow++;


                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory, xlsRow - 1, cEmpCategory].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["EmpCategory"].Row, cEmpCategory, xlsRow - 1, cEmpCategory].Merge();


                            dicGroup["EmpCategory"].Row = xlsRow;
                            dicGroup["EmpCategory"].GroupKey = strGroupEmpCategory;
                        }

                        sheet1.Range[xlsRow, cDivision].Text = dtManPBSummary.Rows[i]["DivisionName"].ToString();
                        if (dicGroup["DivisionName"].GroupKey != strGroupDivisionName)
                        {
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision, xlsRow - 1, cDivision].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["DivisionName"].Row, cDivision, xlsRow - 1, cDivision].Merge();
                            dicGroup["DivisionName"].Row = xlsRow;
                            dicGroup["DivisionName"].GroupKey = strGroupDivisionName;
                        }

                        sheet1.Range[xlsRow, cUnit].Text = dtManPBSummary.Rows[i]["UnitName"].ToString();
                        if (dicGroup["UnitName"].GroupKey != strGroupUnitName)
                        {
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit, xlsRow - 1, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["UnitName"].Row, cUnit, xlsRow - 1, cUnit].Merge();
                            dicGroup["UnitName"].Row = xlsRow;
                            dicGroup["UnitName"].GroupKey = strGroupUnitName;
                        }

                        sheet1.Range[xlsRow, cSection].Text = dtManPBSummary.Rows[i]["SectionName"].ToString();
                        if (dicGroup["SectionName"].GroupKey != strGroupSectionName)
                        {
                            sheet1.Range[dicGroup["SectionName"].Row, cSection, xlsRow - 1, cSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["SectionName"].Row, cSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["SectionName"].Row, cSection].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["SectionName"].Row, cSection, xlsRow - 1, cSection].Merge();
                            dicGroup["SectionName"].Row = xlsRow;
                            dicGroup["SectionName"].GroupKey = strGroupSectionName;
                        }

                        sheet1.Range[xlsRow, cSubSection].Text = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                        if (dicGroup["SubSectionName"].GroupKey != strGroupSubSectionName)
                        {
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection, xlsRow - 1, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["SubSectionName"].Row, cSubSection, xlsRow - 1, cSubSection].Merge();
                            dicGroup["SubSectionName"].Row = xlsRow;
                            dicGroup["SubSectionName"].GroupKey = strGroupSubSectionName;
                        }

                        sheet1.Range[xlsRow, cLine].Text = dtManPBSummary.Rows[i]["LineName"].ToString();
                        if (dicGroup["LineName"].GroupKey != strGroupLineName)
                        {
                            sheet1.Range[dicGroup["LineName"].Row, cLine, xlsRow - 1, cLine].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[dicGroup["LineName"].Row, cLine].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[dicGroup["LineName"].Row, cLine].VerticalAlignment = ExcelVAlign.VAlignTop;
                            sheet1.Range[dicGroup["LineName"].Row, cLine, xlsRow - 1, cLine].Merge();
                            dicGroup["LineName"].Row = xlsRow;
                            dicGroup["LineName"].GroupKey = strGroupLineName;
                        }

                        sheet1.Range[xlsRow, cEmpCategory].Text = dtManPBSummary.Rows[i]["EmpCategory"].ToString();

                        oRU.SetTextBorder(ref sheet1, xlsRow, cAttendancGroup, dtManPBSummary.Rows[i]["DesignationName"].ToString());//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cBudgetedManPower, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["ProposedManpowerBudget"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["TotalManpower"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_PRESENT"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Absent"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Late"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Leave"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdOthers, clsStaticInfo.dbl(dtManPBSummary.Rows[i]["SUM_Others"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdRemarks, "");//




                        xlsRow++;
                    }
                    //xlsRow += 1;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = GetFormulaGrandTotal(rowList, cBudgetedManPower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdOthers].Formula = GetFormulaGrandTotal(rowList, cfdOthers);


                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);



                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(PlantId, "", out dsCmp);
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
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].Text = FactoryAddress;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IWorkbook X_GetSummaryManpowerAttendanceExcel(string companyGroupId, string companyId, string PlantId, string workDate, bool withLine)
        {
            try
            {
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                //clsReport objRpt = null;
                var objRpt = new clsReport();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                DataTable dtManPBSummary = GetDailyAttendanceSummarySql(workDate, withLine, companyGroupId, companyId, PlantId, "", true, true);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cUnit = 0; int cSubSection = 0; int cAttendancGroup = 0; int cOnRollManpower; int cBudgetedManPower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdOthers = 0; var cfdRemarks = 0; int cDivision = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Division", ExcelHAlign.HAlignCenter); cDivision = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Unit", ExcelHAlign.HAlignCenter); cUnit = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sub Section", ExcelHAlign.HAlignCenter); cSubSection = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Budgeted", 8, ExcelHAlign.HAlignCenter); cBudgetedManPower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 8, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 8, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 8, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 8, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 8, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Others", 8, ExcelHAlign.HAlignCenter); cfdOthers = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarkes", 10, ExcelHAlign.HAlignCenter); cfdRemarks = xlsCol;

                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                if (dtManPBSummary.Rows.Count > 0)
                {
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _grp3 = string.Empty;
                    string _grp4 = string.Empty;


                    #endregion
                    var catFRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;
                    for (int i = 0; i < dtManPBSummary.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;
                        if (_grp1 != dtManPBSummary.Rows[i]["DivisionName"].ToString() && string.IsNullOrEmpty(dtManPBSummary.Rows[i]["DivisionName"].ToString()) == false)
                        {
                            _grp1 = dtManPBSummary.Rows[i]["DivisionName"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastMPGroup = _grp1;
                                rowList.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                                sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);
                                //sheet1.Range[xlsRow, cfdRemarks].BorderAround(ExcelLineStyle.Hair);


                                xlsRow++;
                            }
                            #endregion

                            sheet1.Range[xlsRow, cDivision].Text = _grp1;
                            sheet1.Range[xlsRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cDivision].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cDivision].VerticalAlignment = ExcelVAlign.VAlignTop;


                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            SetCellText(sheet1, xlsRow, cUnit, _grp2);
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);

                            if (catFRow < xlsRow)
                            {
                                catFRow = xlsRow;
                                catGrp2FRow = xlsRow;
                                catGrp3FRow = xlsRow;

                            }
                        }
                        else if (_grp2 != dtManPBSummary.Rows[i]["UnitName"].ToString())
                        {
                            _grp2 = dtManPBSummary.Rows[i]["UnitName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cUnit].Text = _grp2;
                            sheet1.Range[xlsRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            SetCellText(sheet1, xlsRow, cSubSection, _grp3);
                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                            if (catGrp2FRow < xlsRow)
                            {
                                catGrp2FRow = xlsRow;
                                catGrp3FRow = xlsRow;

                            }
                        }
                        else if (_grp3 != dtManPBSummary.Rows[i]["SubSectionName"].ToString())
                        {
                            _grp3 = dtManPBSummary.Rows[i]["SubSectionName"].ToString();
                            //SetCellText(sheet1, xlsRow, cSubSection, _grp2);
                            sheet1.Range[xlsRow, cSubSection].Text = _grp3;
                            sheet1.Range[xlsRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, cSubSection].HorizontalAlignment = ExcelHAlign.HAlignJustify;
                            sheet1.Range[xlsRow, cSubSection].VerticalAlignment = ExcelVAlign.VAlignTop;

                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);
                            if (catGrp3FRow < xlsRow)
                            {
                                catGrp3FRow = xlsRow;
                            }
                        }
                        else if (_grp4 != dtManPBSummary.Rows[i]["DesignationName"].ToString())
                        {

                            _grp4 = dtManPBSummary.Rows[i]["DesignationName"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, _grp4);

                            sheet1.Range[catFRow, cDivision, xlsRow, cDivision].Merge();
                            sheet1.Range[catFRow, cDivision, xlsRow, cDivision].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].Merge();
                            sheet1.Range[catGrp2FRow, cUnit, xlsRow, cUnit].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].Merge();
                            sheet1.Range[catGrp3FRow, cSubSection, xlsRow, cSubSection].BorderAround(ExcelLineStyle.Hair);

                        }
                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, Convert.ToInt32(dtManPBSummary.Rows[i]["TotalManpower"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cBudgetedManPower, Convert.ToInt32(dtManPBSummary.Rows[i]["ProposedManpowerBudget"].ToString()));
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, Convert.ToInt32(dtManPBSummary.Rows[i]["SUM_PRESENT"].ToString()));//LegalDesignation
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, Convert.ToInt32(dtManPBSummary.Rows[i]["SUM_Absent"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, Convert.ToInt32(dtManPBSummary.Rows[i]["SUM_Late"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, Convert.ToInt32(dtManPBSummary.Rows[i]["SUM_Leave"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdOthers, Convert.ToInt32(dtManPBSummary.Rows[i]["SUM_Others"].ToString()));//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdRemarks, "");//
                        xlsRow++;
                    }
                    xlsRow += 1;

                    rowList.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");

                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cBudgetedManPower) + catFRow + ":" + oRU.GetColumnNameForXls(cBudgetedManPower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdOthers) + catFRow + ":" + oRU.GetColumnNameForXls(cfdOthers) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;
                    xlsRow++;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cBudgetedManPower].Formula = GetFormulaGrandTotal(rowList, cBudgetedManPower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdOthers].Formula = GetFormulaGrandTotal(rowList, cfdOthers);


                    sheet1.Range[xlsRow, 1, xlsRow, (cBudgetedManPower - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cBudgetedManPower].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdOthers].BorderAround(ExcelLineStyle.Hair);



                    sheet1.Range[xlsRow, cBudgetedManPower, xlsRow, cfdOthers].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion


                    objRpt.SelectedPlantWiseCompany(PlantId, "", out dsCmp);
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
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 1].RowHeight = 30;

                    #region Plant Address


                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["CompanyAddress"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].Text = FactoryAddress;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public DataTable GetDailyAttendanceSummarySql(string WorkDate, bool withLine, string companyGroupId, string companyId, string plantId, string typeList, bool WithoutTBS, bool WithoutLA)
        {
            string strSql = string.Empty;

            try
            {
                string includeTBS = "";
                string includeTBS1 = "";
                string includeLa = "";
                string includeLa1 = "";
                string wc = string.Empty;
                string selectLine = "";
                string MselectLine = "";

                string joiningLineEmp = "";
                string joiningLineEmpAttdn = "";
                string joiningLineBudget = "";
                string joiningAttdnNotProcessed = "";
                string joiningLineShiftNotAssigned = "";


                if (withLine == true)
                {
                    selectLine = ",LineName";
                    MselectLine = ",M.LineName";

                    joiningLineEmp = "AND EmpInfo.LineId = M.LineId";
                    joiningLineEmpAttdn = "AND EmpAttdn.LineId = M.LineId";
                    joiningLineBudget = "and B.LineId = M.LineId";
                    joiningAttdnNotProcessed = " and AttdnNotProcessedToday.LineId = M.LineId";
                    joiningLineShiftNotAssigned = " and ShiftNotAssigned.LineId = M.LineId";

                }
                if (WithoutTBS)
                {
                    includeTBS = " And  ISNULL(Em.EmployeeCurrentStatus,'') <>  'LONG ABSENTEEISM'";
                    includeTBS1 = " And  ISNULL(E.EmployeeCurrentStatus,'') <>  'LONG ABSENTEEISM'";
                }
                if (WithoutLA)
                {
                    includeLa = " And  ISNULL(Em.EmployeeCurrentStatus,'') <>  'TBS'";
                    includeLa1 = " And  ISNULL(E.EmployeeCurrentStatus,'') <>  'TBS'";
                }
                strSql = @"SELECT DivisionName
                            	,UnitName,EDE.EmpCategory,EDE.Department,EDE.SectionName
                            	,SubSectionName
                            	,DesignationName" + selectLine + @" 
                            	,ISNULL(SUM(TotalNumber), 0) ProposedManpowerBudget
                            	,ISNULL(SUM(TotalManpower), 0) TotalManpower
                            	,ISNULL(SUM(SUM_PRESENT), 0) SUM_PRESENT
                            	,ISNULL(SUM(SUM_Leave), 0) SUM_Leave
                            	,ISNULL(SUM(SUM_Absent), 0) SUM_Absent
                            	,ISNULL(SUM(SUM_Late), 0) SUM_Late
                            	,ISNULL(SUM(Others), 0) SUM_Others
                            FROM (
                            	SELECT m.DesignationName
                            		,M.DivisionName
                            		,M.EmpCategory
                            		,M.SectionName
                            		,M.SubSectionName
                            		,M.Department
                            		,M.UnitName" + MselectLine + @" 
                            		,TotalNumber
                            		,EmpAttdn.SUM_PRESENT
                            		,EmpAttdn.SUM_Absent
                            		,EmpAttdn.SUM_Leave
                            		,EmpAttdn.SUM_Late
                            		,EmpInfo.TotalManpower
                            		,ISNULL(ShiftNotAssigned.TotalEmployee, 0) + ISNULL(AttdnNotProcessedToday.TotalEmployee, 0) + ISNULL(EmpAttdn.SUM_Off, 0) Others
                            	FROM
                            		--------------------1 budgetCode from [MST].[ManpowerBudget]--------------------------------------
                            		(
                            		SELECT MB.Code
                            			,MB.Id
                            			,Cg.Id AS CgId
                            			,Cg.UserName AS GroupName
                            			,c.Id AS CompanyId
                            			,c.UserName AS CName
                            			,Division.UserName DivisionName
                            			,Division.Id DivisionId
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName--,Designation.Id
                            			,Designation.Id DesignationId
                            			,Division.Sequence DivisionSequence
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Unit.UserName UnitName
                            			,Unit.Id UnitId
                            			,Unit.Sequence UnitSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            			,Line.Sequence LineSequence
                            			,Section.UserName SectionName
                            			,EC.UserName EmpCategory,Department.UserName Department
                            		FROM [MST].[ManpowerBudget] MB
                            		LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                            		LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                            		LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
                            		LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = Po.SectionId
                            		LEFT JOIN [ORG].[Subsection] ON Subsection.Id = Po.SubsectionId
                            		LEFT JOIN [HKP].Designation Designation ON Designation.Id = PO.DesignationId
									left join [MST].[DesignationMaster] dm on dm.DesignationId=po.DesignationId
                            		left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1
                            		) M
                            	-----------------------2. EmployeeInformation from [dbo].[EmployeeInformation]--------------------------------
                            	LEFT OUTER JOIN (
                            		SELECT COUNT(em.SystemId) TotalManpower
                            			,Em.BudgetCode
                            			,--MB.CompanyGroupId,MB.CompanyId,
                            			Division.UserName DivisionName
                            			,Division.Id DivisionId
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,Division.Sequence DivisionSequence
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Unit.UserName UnitName
                            			,Unit.Id UnitId
                            			,Unit.Sequence UnitSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM [dbo].[EmployeeInformation] EM
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = em.BudgetCode
                            		LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                            		LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                            		LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                            		LEFT JOIN [ORG].[Division] ON Division.Id = E.DivisionId
                            		LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = Po.SectionId
                            		LEFT JOIN [ORG].[Subsection] ON Subsection.Id = Po.SubsectionId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [HKP].Designation Designation ON Designation.Id = PO.DesignationId
                            		LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EM.GivenDesignationId
                            		LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = EM.EmployeeCodeTypeId
                            		WHERE (
                            				EM.DOJ <= '" + WorkDate + @"'
                            				AND (
                            					EM.DOS IS NULL
                            					OR EM.DOS >= '" + WorkDate + @"'
                            					)
                            				)
                            			AND EM.GroupID = '" + companyGroupId + @"'
                            			AND EM.CompanyId = '" + companyId + @"'
                            			AND Plant.Id IN (" + plantId + @")
                            			AND MB.Active = 1 AND Ect.Id IN (" + typeList + @") " + includeTBS + @" " + includeLa + @"
                            		GROUP BY Em.BudgetCode
                            			,Division.UserName
                            			,Division.Id
                            			,SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,Unit.UserName
                            			,Unit.Id
                            			,Unit.Sequence
                            			,Division.Sequence
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,Line.UserName
                            			,Line.Id
                            		) EmpInfo ON m.Id = EmpInfo.BudgetCode
                            		AND EmpInfo.DesignationId = m.DesignationId
                            		AND EmpInfo.DivisionId = m.DivisionId
                            		AND EmpInfo.SubSectionId = M.SubSectionId
                            		AND EmpInfo.UnitId = M.UnitId " + joiningLineEmp + @"
                            	LEFT OUTER JOIN (
                            		SELECT E.BudgetCode
                            			,Division.UserName DivisionName
                            			,Division.Id DivisionId
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,Division.Sequence DivisionSequence
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Unit.UserName UnitName
                            			,Unit.Id UnitId
                            			,Unit.Sequence UnitSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            			,COUNT(E.SystemId) TotalManpower
                            			,SUM(CASE 
                            					WHEN dt.Category IN ('Present')
                            						THEN 1
                            					ELSE 0
                            					END) SUM_PRESENT
                            			,SUM(CASE 
                            					WHEN dt.Category = 'Late'
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Late
                            			,SUM(CASE 
                            					WHEN dt.Category = 'Absent'
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Absent
                            			,SUM(CASE 
                            					WHEN dt.Category IN ('Leave')
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Leave
                            			,SUM(CASE 
                            					WHEN dt.Category IN (
                            							'Holiday'
                            							,'Weekend'
                            							)
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Off
                            		FROM EmployeeInformation e
                            		LEFT JOIN MST.ManpowerBudget MB ON MB.Id = E.BudgetCode
                            		LEFT JOIN AttdnProcessData apd ON e.SystemId = apd.EmpSystemID
                            			AND APD.WorkDate = '" + WorkDate + @"'
                            		LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Division] Division ON Division.Id = ENT.DivisionId
                            		LEFT JOIN [ORG].[SubSection] SubSection ON SubSection.Id = PO.SubSectionId
                            		LEFT JOIN [HKP].[Designation] Designation ON Designation.Id = PO.DesignationId
                            		LEFT JOIN [ORG].[Unit] ON Unit.Id = ENT.UnitId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		JOIN DayType Dt ON Dt.DayType = apd.DayStatus
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = E.EmployeeCodeTypeId
                            		WHERE E.GroupID = '" + companyGroupId + @"'
                            			AND E.CompanyId = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1 AND Ect.Id IN (" + typeList + @") " + includeTBS1 + @" " + includeLa1 + @"
                            		GROUP BY Division.UserName
                            			,Division.Id
                            			,SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,Unit.UserName
                            			,Unit.Id
                            			,Unit.Sequence
                            			,Division.Sequence
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,E.BudgetCode
                            			,Line.UserName
                            			,Line.Id
                            		) EmpAttdn ON m.Id = EmpAttdn.BudgetCode
                            		AND EmpAttdn.DivisionId = M.DivisionId
                            		AND EmpAttdn.SubSectionId = M.SubSectionId
                            		AND EmpAttdn.DesignationId = M.DesignationId
                            		AND EmpAttdn.UnitId = M.UnitId " + joiningLineEmpAttdn + @"
                            	-------------------------3. Manpower Budget Detail from [MST].[ManpowerBudgetDetail]--------------------------------------------------------
                            	LEFT OUTER JOIN (
                            		SELECT MBD.TotalNumber
                            			,MBD.ManpowerBudgetId
                            			,Division.UserName DivisionName
                            			,Division.Id DivisionId
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,Division.Sequence DivisionSequence
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Unit.UserName UnitName
                            			,Unit.Id UnitId
                            			,Unit.Sequence UnitSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM (
                            			SELECT TOP 1
                            			WITH TIES TotalNumber
                            				,ManpowerBudgetId
                            				,EffectiveDate
                            			FROM [MST].[ManpowerBudgetDetail]
                            			WHERE CONVERT(DATE, EffectiveDate) <= CONVERT(DATE, '" + WorkDate + @"')
                            			ORDER BY ROW_NUMBER() OVER (
                            					PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC
                            					)
                            			) MBD
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON Mb.Id = MBD.ManpowerBudgetId
                            		LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                            		LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                            			AND mb.CompanyId = c.Id
                            		LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                            		LEFT JOIN [ORG].[Division] Division ON Division.Id = E.DivisionId
                            		LEFT JOIN [ORG].[SubSection] SubSection ON SubSection.Id = PO.SubSectionId
                            		LEFT JOIN [HKP].[Designation] Designation ON Designation.Id = PO.DesignationId
                            		LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND Plant.Id IN (" + plantId + @")
                            			AND MB.Active = 1
                            			AND TotalNumber > 0
                            		) B ON M.id = b.ManpowerBudgetId
                            		AND B.DivisionId = M.DivisionId
                            		AND B.DesignationId = M.DesignationId
                            		AND B.SubSectionId = M.SubSectionId
                            		AND B.UnitId = M.UnitId " + joiningLineBudget + @"
                            	LEFT JOIN (
                            		SELECT count(E.SystemID) TotalEmployee
                            			,E.BudgetCode
                            			,--MB.CompanyGroupId,MB.CompanyId,
                            			Division.UserName DivisionName
                            			,Division.Id DivisionId
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,Division.Sequence DivisionSequence
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Unit.UserName UnitName
                            			,Unit.Id UnitId
                            			,Unit.Sequence UnitSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM ORG.CompanyGroup CG
                            		LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                            		INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id
                            			AND c.Id = E.CompanyId
                            		INNER JOIN (
                            			--*
                            			SELECT TOP 1
                            			WITH TIES *
                            			FROM EmployeeShiftAssign
                            			WHERE EffectiveDate <= '" + WorkDate + @"'
                            				AND EmpSystemID NOT IN (
                            					--**
                            					SELECT DISTINCT EmpSystemID
                            					FROM AttdnProcessData
                            					WHERE CONVERT(DATE, WorkDate) = CONVERT(DATE, '" + WorkDate + @"')
                            					)
                            			ORDER BY ROW_NUMBER() OVER (
                            					PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC
                            					)
                            			) -- *
                            			ESA ON E.SystemId = ESA.EmpSystemID
                            		LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                            		LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                            		LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                            		LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
                            		LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = ENT.PlantId
                            		LEFT JOIN [ORG].[Division] ON Division.Id = ENT.DivisionId
                            		LEFT JOIN [ORG].[Unit] ON Unit.Id = ENT.UnitId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = POS.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = POS.SectionId
                            		LEFT JOIN [ORG].[SubSection] ON SubSection.Id = POS.SubSectionId
                            		LEFT JOIN [HKP].[Designation] Designation ON Designation.Id = POS.DesignationId
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = E.EmployeeCodeTypeId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1
                            			AND (
                            				E.DOJ <= '" + WorkDate + @"'
                            				AND (
                            					E.DOS IS NULL
                            					OR E.DOS >= '" + WorkDate + @"'
                            					)
                            				) AND Ect.Id IN (" + typeList + @") " + includeTBS1 + @" " + includeLa1 + @"
                            		GROUP BY Division.UserName
                            			,Division.Id
                            			,SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,Division.Sequence
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,E.BudgetCode
                            			,Unit.UserName
                            			,Unit.Id
                            			,Unit.Sequence
                            			,Line.UserName
                            			,Line.Id
                            		) AttdnNotProcessedToday ON AttdnNotProcessedToday.BudgetCode = M.Id
                            		AND AttdnNotProcessedToday.DivisionId = M.DivisionId
                            		AND AttdnNotProcessedToday.DesignationId = M.DesignationId
                            		AND AttdnNotProcessedToday.SubSectionId = M.SubSectionId
                            		AND AttdnNotProcessedToday.UnitId = M.UnitId " + joiningAttdnNotProcessed + @"
                            	LEFT JOIN (
                            		SELECT COUNT(E.SystemId) TotalEmployee
                            			,E.BudgetCode
                            			,--MB.CompanyGroupId,MB.CompanyId,
                            			Division.UserName DivisionName
                            			,Division.Id DivisionId
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,Division.Sequence DivisionSequence
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Unit.UserName UnitName
                            			,Unit.Id UnitId
                            			,Unit.Sequence UnitSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM ORG.CompanyGroup CG
                            		LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                            		LEFT OUTER JOIN (
                            			--*
                            			SELECT *
                            			FROM EmployeeInformation
                            			WHERE SystemId NOT IN (
                            					--**
                            					SELECT DISTINCT EmpSystemID
                            					FROM EmployeeShiftAssign
                            					) --**
                            			) --*
                            			E ON e.GroupID = CG.Id
                            			AND c.Id = E.CompanyId
                            		LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                            		LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                            		LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                            		LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
                            		LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = ENT.PlantId
                            		LEFT JOIN [ORG].[Division] ON Division.Id = ENT.DivisionId
                            		LEFT JOIN [ORG].[Unit] ON Unit.Id = ENT.UnitId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = POS.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = POS.SectionId
                            		LEFT JOIN [ORG].[SubSection] ON SubSection.Id = POS.SubSectionId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [HKP].[Designation] ON Designation.Id = POS.DesignationId
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = E.EmployeeCodeTypeId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1
                            			AND (
                            				E.DOJ <= '" + WorkDate + @"'
                            				AND (
                            					E.DOS IS NULL
                            					OR E.DOS >= '" + WorkDate + @"'
                            					)
                            				) AND Ect.Id IN (" + typeList + @") " + includeTBS1 + @" " + includeLa1 + @"
                            		GROUP BY Division.UserName
                            			,Division.Id
                            			,SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,Division.Sequence
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,E.BudgetCode
                            			,Unit.UserName
                            			,Unit.Id
                            			,Unit.Sequence
                            			,Line.UserName
                            			,Line.Id
                            		) ShiftNotAssigned ON ShiftNotAssigned.BudgetCode = M.Id
                            		AND ShiftNotAssigned.DivisionId = M.DivisionId
                            		AND ShiftNotAssigned.DesignationId = M.DesignationId
                            		AND ShiftNotAssigned.SubSectionId = M.SubSectionId
                            		AND ShiftNotAssigned.UnitId = M.UnitId " + joiningLineShiftNotAssigned + @"
                            	) EDE
                            GROUP BY DesignationName
                            	,DivisionName,EDE.EmpCategory,EDE.Department,EDE.SectionName
                            	,SubSectionName
                            	,UnitName " + selectLine + @"
                            ORDER BY DivisionName
                            	,UnitName,EDE.EmpCategory,EDE.Department,EDE.SectionName
                            	,SubSectionName " + selectLine + @"
                            	,DesignationName";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }

        }//End Function

        public DataTable GetDailyManpowerAttendanceSummarySql(string WorkDate, bool withLine, string companyGroupId, string companyId, string plantId, string typeList, bool WithoutTBS, bool WithoutLA)
        {
            string strSql = string.Empty;

            try
            {
                string includeTBS = "";
                string includeTBS1 = "";
                string includeLa = "";
                string includeLa1 = "";
                string wc = string.Empty;
                string selectLine = "";
                string MselectLine = "";

                string joiningLineEmp = "";
                string joiningLineEmpAttdn = "";
                string joiningLineBudget = "";
                string joiningAttdnNotProcessed = "";
                string joiningLineShiftNotAssigned = "";


                if (withLine == true)
                {
                    selectLine = ",LineName";
                    MselectLine = ",M.LineName";

                    joiningLineEmp = "AND EmpInfo.LineId = M.LineId";
                    joiningLineEmpAttdn = "AND EmpAttdn.LineId = M.LineId";
                    joiningLineBudget = "and B.LineId = M.LineId";
                    joiningAttdnNotProcessed = " and AttdnNotProcessedToday.LineId = M.LineId";
                    joiningLineShiftNotAssigned = " and ShiftNotAssigned.LineId = M.LineId";

                }
                if (WithoutTBS)
                {
                    includeTBS = " And  ISNULL(Em.EmployeeCurrentStatus,'') <>  'LONG ABSENTEEISM'";
                    includeTBS1 = " And  ISNULL(E.EmployeeCurrentStatus,'') <>  'LONG ABSENTEEISM'";
                }
                if (WithoutLA)
                {
                    includeLa = " And  ISNULL(Em.EmployeeCurrentStatus,'') <>  'TBS'";
                    includeLa1 = " And  ISNULL(E.EmployeeCurrentStatus,'') <>  'TBS'";
                }
                strSql = @"SELECT 
                            	EDE.EmpCategory,EDE.Department,EDE.SectionName
                            	,SubSectionName
                            	,DesignationName" + selectLine + @" 
                            	,ISNULL(SUM(TotalNumber), 0) ProposedManpowerBudget
                            	,ISNULL(SUM(TotalManpower), 0) TotalManpower
                            	,ISNULL(SUM(SUM_PRESENT), 0) SUM_PRESENT
                            	,ISNULL(SUM(SUM_Leave), 0) SUM_Leave
                            	,ISNULL(SUM(SUM_Absent), 0) SUM_Absent
                            	,ISNULL(SUM(SUM_Late), 0) SUM_Late
                            	,ISNULL(SUM(Others), 0) SUM_Others
                            FROM (
                            	SELECT m.DesignationName
                            		,M.EmpCategory
                            		,M.SectionName
                            		,M.SubSectionName
                            		,M.Department,M.LineName 
                            		,TotalNumber
                            		,EmpAttdn.SUM_PRESENT
                            		,EmpAttdn.SUM_Absent
                            		,EmpAttdn.SUM_Leave
                            		,EmpAttdn.SUM_Late
                            		,EmpInfo.TotalManpower
                            		,ISNULL(ShiftNotAssigned.TotalEmployee, 0) + ISNULL(AttdnNotProcessedToday.TotalEmployee, 0) + ISNULL(EmpAttdn.SUM_Off, 0) Others
                            	FROM
                            		--------------------1 budgetCode from [MST].[ManpowerBudget]--------------------------------------
                            		(
                            		SELECT MB.Code
                            			,MB.Id
                            			,Cg.Id AS CgId
                            			,Cg.UserName AS GroupName
                            			,c.Id AS CompanyId
                            			,c.UserName AS CName
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName--,Designation.Id
                            			,Designation.Id DesignationId
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            			,Line.Sequence LineSequence
                            			,Section.UserName SectionName
                            			,EC.UserName EmpCategory,Department.UserName Department
                            		FROM [MST].[ManpowerBudget] MB
                            		LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                            		LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                            		LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = Po.SectionId
                            		LEFT JOIN [ORG].[Subsection] ON Subsection.Id = Po.SubsectionId
                            		LEFT JOIN [HKP].Designation Designation ON Designation.Id = PO.DesignationId
									left join [MST].[DesignationMaster] dm on dm.DesignationId=po.DesignationId
                            		left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1
                            		) M
                            	-----------------------2. EmployeeInformation from [dbo].[EmployeeInformation]--------------------------------
                            	LEFT OUTER JOIN (
                            		SELECT COUNT(em.SystemId) TotalManpower
                            			,Em.BudgetCode
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM [dbo].[EmployeeInformation] EM
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = em.BudgetCode
                            		LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                            		LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                            		LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = Po.SectionId
                            		LEFT JOIN [ORG].[Subsection] ON Subsection.Id = Po.SubsectionId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [HKP].Designation Designation ON Designation.Id = PO.DesignationId
                            		LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EM.GivenDesignationId
                            		LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = EM.EmployeeCodeTypeId
                            		WHERE (
                            				EM.DOJ <= '" + WorkDate + @"'
                            				AND (
                            					EM.DOS IS NULL
                            					OR EM.DOS >= '" + WorkDate + @"'
                            					)
                            				)
                            			AND EM.GroupID = '" + companyGroupId + @"'
                            			AND EM.CompanyId = '" + companyId + @"'
                            			AND Plant.Id IN (" + plantId + @")
                            			AND MB.Active = 1 AND Ect.Id IN (" + typeList + @") " + includeTBS + @" " + includeLa + @"
                            		GROUP BY Em.BudgetCode
                            			,SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,Line.UserName
                            			,Line.Id
                            		) EmpInfo ON m.Id = EmpInfo.BudgetCode
                            		AND EmpInfo.DesignationId = m.DesignationId
                            		 " + joiningLineEmp + @"
                            	LEFT OUTER JOIN (
                            		SELECT E.BudgetCode
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId

                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            			,COUNT(E.SystemId) TotalManpower
                            			,SUM(CASE 
                            					WHEN dt.Category IN ('Present')
                            						THEN 1
                            					ELSE 0
                            					END) SUM_PRESENT
                            			,SUM(CASE 
                            					WHEN dt.Category = 'Late'
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Late
                            			,SUM(CASE 
                            					WHEN dt.Category = 'Absent'
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Absent
                            			,SUM(CASE 
                            					WHEN dt.Category IN ('Leave')
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Leave
                            			,SUM(CASE 
                            					WHEN dt.Category IN (
                            							'Holiday'
                            							,'Weekend'
                            							)
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Off
                            		FROM EmployeeInformation e
                            		LEFT JOIN MST.ManpowerBudget MB ON MB.Id = E.BudgetCode
                            		LEFT JOIN AttdnProcessData apd ON e.SystemId = apd.EmpSystemID
                            			AND APD.WorkDate = '" + WorkDate + @"'
                            		LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		LEFT JOIN [ORG].[SubSection] SubSection ON SubSection.Id = PO.SubSectionId
                            		LEFT JOIN [HKP].[Designation] Designation ON Designation.Id = PO.DesignationId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		JOIN DayType Dt ON Dt.DayType = apd.DayStatus
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = E.EmployeeCodeTypeId
                            		WHERE E.GroupID = '" + companyGroupId + @"'
                            			AND E.CompanyId = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1 AND Ect.Id IN (" + typeList + @") " + includeTBS1 + @" " + includeLa1 + @"
                            		GROUP BY SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,E.BudgetCode
                            			,Line.UserName
                            			,Line.Id
                            		) EmpAttdn ON m.Id = EmpAttdn.BudgetCode
                            		AND EmpAttdn.SubSectionId = M.SubSectionId
                            		" + joiningLineEmpAttdn + @"
                            	-------------------------3. Manpower Budget Detail from [MST].[ManpowerBudgetDetail]--------------------------------------------------------
                            	LEFT OUTER JOIN (
                            		SELECT MBD.TotalNumber
                            			,MBD.ManpowerBudgetId
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM (
                            			SELECT TOP 1
                            			WITH TIES TotalNumber
                            				,ManpowerBudgetId
                            				,EffectiveDate
                            			FROM [MST].[ManpowerBudgetDetail]
                            			WHERE CONVERT(DATE, EffectiveDate) <= CONVERT(DATE, '" + WorkDate + @"')
                            			ORDER BY ROW_NUMBER() OVER (
                            					PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC
                            					)
                            			) MBD
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON Mb.Id = MBD.ManpowerBudgetId
                            		LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                            		LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                            			AND mb.CompanyId = c.Id
                            		LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                            		
                            		LEFT JOIN [ORG].[SubSection] SubSection ON SubSection.Id = PO.SubSectionId
                            		LEFT JOIN [HKP].[Designation] Designation ON Designation.Id = PO.DesignationId
                            		
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND Plant.Id IN (" + plantId + @")
                            			AND MB.Active = 1
                            			AND TotalNumber > 0
                            		) B ON M.id = b.ManpowerBudgetId
                            		AND B.DesignationId = M.DesignationId
                            		AND B.SubSectionId = M.SubSectionId " + joiningLineBudget + @"
                            	LEFT JOIN (
                            		SELECT count(E.SystemID) TotalEmployee
                            			,E.BudgetCode
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM ORG.CompanyGroup CG
                            		LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                            		INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id
                            			AND c.Id = E.CompanyId
                            		INNER JOIN (
                            			--*
                            			SELECT TOP 1
                            			WITH TIES *
                            			FROM EmployeeShiftAssign
                            			WHERE EffectiveDate <= '" + WorkDate + @"'
                            				AND EmpSystemID NOT IN (
                            					--**
                            					SELECT DISTINCT EmpSystemID
                            					FROM AttdnProcessData
                            					WHERE CONVERT(DATE, WorkDate) = CONVERT(DATE, '09-Jul-2024')
                            					)
                            			ORDER BY ROW_NUMBER() OVER (
                            					PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC
                            					)
                            			) -- *
                            			ESA ON E.SystemId = ESA.EmpSystemID
                            		LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                            		LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                            		LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                            		LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
                            		LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = ENT.PlantId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = POS.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = POS.SectionId
                            		LEFT JOIN [ORG].[SubSection] ON SubSection.Id = POS.SubSectionId
                            		LEFT JOIN [HKP].[Designation] Designation ON Designation.Id = POS.DesignationId
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = E.EmployeeCodeTypeId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1
                            			AND (
                            				E.DOJ <= '" + WorkDate + @"'
                            				AND (
                            					E.DOS IS NULL
                            					OR E.DOS >= '" + WorkDate + @"'
                            					)
                            				) AND Ect.Id IN (" + typeList + @") " + includeTBS1 + @" " + includeLa1 + @"
                            		GROUP BY SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,E.BudgetCode
                            			,Line.UserName
                            			,Line.Id
                            		) AttdnNotProcessedToday ON AttdnNotProcessedToday.BudgetCode = M.Id
                            		AND AttdnNotProcessedToday.DesignationId = M.DesignationId
                            		AND AttdnNotProcessedToday.SubSectionId = M.SubSectionId
                            		 and AttdnNotProcessedToday.LineId = M.LineId
                            	LEFT JOIN (
                            		SELECT COUNT(E.SystemId) TotalEmployee
                            			,E.BudgetCode
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM ORG.CompanyGroup CG
                            		LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                            		LEFT OUTER JOIN (
                            			--*
                            			SELECT *
                            			FROM EmployeeInformation
                            			WHERE SystemId NOT IN (
                            					--**
                            					SELECT DISTINCT EmpSystemID
                            					FROM EmployeeShiftAssign
                            					) --**
                            			) --*
                            			E ON e.GroupID = CG.Id
                            			AND c.Id = E.CompanyId
                            		LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                            		LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                            		LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                            		LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
                            		LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = ENT.PlantId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = POS.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = POS.SectionId
                            		LEFT JOIN [ORG].[SubSection] ON SubSection.Id = POS.SubSectionId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [HKP].[Designation] ON Designation.Id = POS.DesignationId
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = E.EmployeeCodeTypeId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1
                            			AND (
                            				E.DOJ <= '" + WorkDate + @"'
                            				AND (
                            					E.DOS IS NULL
                            					OR E.DOS >= '" + WorkDate + @"'
                            					)
                            				) AND Ect.Id IN (" + typeList + @") " + includeTBS1 + @" " + includeLa1 + @"
                            		GROUP BY SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,E.BudgetCode
                            			,Line.UserName
                            			,Line.Id
                            		) ShiftNotAssigned ON ShiftNotAssigned.BudgetCode = M.Id
                            		AND ShiftNotAssigned.DesignationId = M.DesignationId
                            		AND ShiftNotAssigned.SubSectionId = M.SubSectionId
                            		" + joiningLineShiftNotAssigned + @"
                            	) EDE
                            GROUP BY DesignationName
                            	,EDE.EmpCategory,EDE.Department,EDE.SectionName
                            	,SubSectionName
                            	" + selectLine + @"
                            ORDER BY EDE.EmpCategory,EDE.Department,EDE.SectionName
                            	,SubSectionName " + selectLine + @"
                            	,DesignationName";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }

        }//End Function

        public IEnumerable<object> GetDailyManpowerAttendanceSummaryData(string companyGroupId, string companyId, string WorkDate, bool withLine, bool withDesignation, string plantId, string typeList, bool WithoutTBS, bool WithoutLA)
        {
            string strSql = string.Empty;

            try
            {
                string includeTBS = "";
                string includeTBS1 = "";
                string includeLa = "";
                string includeLa1 = "";
                string wc = string.Empty;
                string selectLine = "";
                string MselectLine = "";

                string joiningLineEmp = "";
                string joiningLineEmpAttdn = "";
                string joiningLineBudget = "";
                string joiningAttdnNotProcessed = "";
                string joiningLineShiftNotAssigned = "";


                if (withLine == true)
                {
                    selectLine = ",LineName";
                    MselectLine = ",M.LineName";

                    joiningLineEmp = "AND EmpInfo.LineId = M.LineId";
                    joiningLineEmpAttdn = "AND EmpAttdn.LineId = M.LineId";
                    joiningLineBudget = "and B.LineId = M.LineId";
                    joiningAttdnNotProcessed = " and AttdnNotProcessedToday.LineId = M.LineId";
                    joiningLineShiftNotAssigned = " and ShiftNotAssigned.LineId = M.LineId";

                }
                if (WithoutTBS)
                {
                    includeTBS = " And  ISNULL(Em.EmployeeCurrentStatus,'') <>  'LONG ABSENTEEISM'";
                    includeTBS1 = " And  ISNULL(E.EmployeeCurrentStatus,'') <>  'LONG ABSENTEEISM'";
                }
                if (WithoutLA)
                {
                    includeLa = " And  ISNULL(Em.EmployeeCurrentStatus,'') <>  'TBS'";
                    includeLa1 = " And  ISNULL(E.EmployeeCurrentStatus,'') <>  'TBS'";
                }
                strSql = @"SELECT 
                            	EDE.EmpCategory,EDE.Department,EDE.SectionName
                            	,SubSectionName
                            	,DesignationName" + selectLine + @" 
                            	,ISNULL(SUM(TotalNumber), 0) Budgeted
                            	,ISNULL(SUM(TotalManpower), 0) OnRoll
                            	,ISNULL(SUM(SUM_PRESENT), 0) Present
                            	,ISNULL(SUM(SUM_Leave), 0) Leave
                            	,ISNULL(SUM(SUM_Absent), 0) Absent
                            	,ISNULL(SUM(SUM_Late), 0) Late
                            	,ISNULL(SUM(Others), 0) Others
                                ,Code BudgetCode
                            FROM (
                            	SELECT m.DesignationName
                            		,M.EmpCategory
                            		,M.SectionName
                            		,M.SubSectionName
                            		,M.Department,M.LineName 
                            		,TotalNumber
                            		,EmpAttdn.SUM_PRESENT
                            		,EmpAttdn.SUM_Absent
                            		,EmpAttdn.SUM_Leave
                            		,EmpAttdn.SUM_Late
                            		,EmpInfo.TotalManpower
                            		,ISNULL(ShiftNotAssigned.TotalEmployee, 0) + ISNULL(AttdnNotProcessedToday.TotalEmployee, 0) + ISNULL(EmpAttdn.SUM_Off, 0) Others,M.Code
                            	FROM
                            		--------------------1 budgetCode from [MST].[ManpowerBudget]--------------------------------------
                            		(
                            		SELECT MB.Code
                            			,MB.Id
                            			,Cg.Id AS CgId
                            			,Cg.UserName AS GroupName
                            			,c.Id AS CompanyId
                            			,c.UserName AS CName
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName--,Designation.Id
                            			,Designation.Id DesignationId
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            			,Line.Sequence LineSequence
                            			,Section.UserName SectionName
                            			,EC.UserName EmpCategory,Department.UserName Department
                            		FROM [MST].[ManpowerBudget] MB
                            		LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                            		LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                            		LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = Po.SectionId
                            		LEFT JOIN [ORG].[Subsection] ON Subsection.Id = Po.SubsectionId
                            		LEFT JOIN [HKP].Designation Designation ON Designation.Id = PO.DesignationId
									left join [MST].[DesignationMaster] dm on dm.DesignationId=po.DesignationId
                            		left join HKP.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1
                            		) M
                            	-----------------------2. EmployeeInformation from [dbo].[EmployeeInformation]--------------------------------
                            	LEFT OUTER JOIN (
                            		SELECT COUNT(em.SystemId) TotalManpower
                            			,Em.BudgetCode
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM [dbo].[EmployeeInformation] EM
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = em.BudgetCode
                            		LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = em.GroupId
                            		LEFT OUTER JOIN [ORG].[Company] AS C ON C.Id = em.CompanyId
                            		LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = Po.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = Po.SectionId
                            		LEFT JOIN [ORG].[Subsection] ON Subsection.Id = Po.SubsectionId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [HKP].Designation Designation ON Designation.Id = PO.DesignationId
                            		LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = EM.GivenDesignationId
                            		LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = EM.EmployeeCodeTypeId
                            		WHERE (
                            				EM.DOJ <= '" + WorkDate + @"'
                            				AND (
                            					EM.DOS IS NULL
                            					OR EM.DOS >= '" + WorkDate + @"'
                            					)
                            				)
                            			AND EM.GroupID = '" + companyGroupId + @"'
                            			AND EM.CompanyId = '" + companyId + @"'
                            			AND Plant.Id IN (" + plantId + @")
                            			AND MB.Active = 1 AND Ect.Id IN (" + typeList + @") " + includeTBS + @" " + includeLa + @"
                            		GROUP BY Em.BudgetCode
                            			,SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,Line.UserName
                            			,Line.Id
                            		) EmpInfo ON m.Id = EmpInfo.BudgetCode
                            		AND EmpInfo.DesignationId = m.DesignationId
                            		 " + joiningLineEmp + @"
                            	LEFT OUTER JOIN (
                            		SELECT E.BudgetCode
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId

                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            			,COUNT(E.SystemId) TotalManpower
                            			,SUM(CASE 
                            					WHEN dt.Category IN ('Present')
                            						THEN 1
                            					ELSE 0
                            					END) SUM_PRESENT
                            			,SUM(CASE 
                            					WHEN dt.Category = 'Late'
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Late
                            			,SUM(CASE 
                            					WHEN dt.Category = 'Absent'
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Absent
                            			,SUM(CASE 
                            					WHEN dt.Category IN ('Leave')
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Leave
                            			,SUM(CASE 
                            					WHEN dt.Category IN (
                            							'Holiday'
                            							,'Weekend'
                            							)
                            						THEN 1
                            					ELSE 0
                            					END) SUM_Off
                            		FROM EmployeeInformation e
                            		LEFT JOIN MST.ManpowerBudget MB ON MB.Id = E.BudgetCode
                            		LEFT JOIN AttdnProcessData apd ON e.SystemId = apd.EmpSystemID
                            			AND APD.WorkDate = '" + WorkDate + @"'
                            		LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		LEFT JOIN [ORG].[SubSection] SubSection ON SubSection.Id = PO.SubSectionId
                            		LEFT JOIN [HKP].[Designation] Designation ON Designation.Id = PO.DesignationId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		JOIN DayType Dt ON Dt.DayType = apd.DayStatus
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = E.EmployeeCodeTypeId
                            		WHERE E.GroupID = '" + companyGroupId + @"'
                            			AND E.CompanyId = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1 AND Ect.Id IN (" + typeList + @") " + includeTBS1 + @" " + includeLa1 + @"
                            		GROUP BY SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,E.BudgetCode
                            			,Line.UserName
                            			,Line.Id
                            		) EmpAttdn ON m.Id = EmpAttdn.BudgetCode
                            		AND EmpAttdn.SubSectionId = M.SubSectionId
                            		" + joiningLineEmpAttdn + @"
                            	-------------------------3. Manpower Budget Detail from [MST].[ManpowerBudgetDetail]--------------------------------------------------------
                            	LEFT OUTER JOIN (
                            		SELECT MBD.TotalNumber
                            			,MBD.ManpowerBudgetId
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM (
                            			SELECT TOP 1
                            			WITH TIES TotalNumber
                            				,ManpowerBudgetId
                            				,EffectiveDate
                            			FROM [MST].[ManpowerBudgetDetail]
                            			WHERE CONVERT(DATE, EffectiveDate) <= CONVERT(DATE, '" + WorkDate + @"')
                            			ORDER BY ROW_NUMBER() OVER (
                            					PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC
                            					)
                            			) MBD
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON Mb.Id = MBD.ManpowerBudgetId
                            		LEFT OUTER JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                            		LEFT OUTER JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id
                            			AND mb.CompanyId = c.Id
                            		LEFT OUTER JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                            		LEFT OUTER JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                            		
                            		LEFT JOIN [ORG].[SubSection] SubSection ON SubSection.Id = PO.SubSectionId
                            		LEFT JOIN [HKP].[Designation] Designation ON Designation.Id = PO.DesignationId
                            		
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND Plant.Id IN (" + plantId + @")
                            			AND MB.Active = 1
                            			AND TotalNumber > 0
                            		) B ON M.id = b.ManpowerBudgetId
                            		AND B.DesignationId = M.DesignationId
                            		AND B.SubSectionId = M.SubSectionId " + joiningLineBudget + @"
                            	LEFT JOIN (
                            		SELECT count(E.SystemID) TotalEmployee
                            			,E.BudgetCode
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM ORG.CompanyGroup CG
                            		LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                            		INNER JOIN EmployeeInformation E ON E.GroupID = CG.Id
                            			AND c.Id = E.CompanyId
                            		INNER JOIN (
                            			--*
                            			SELECT TOP 1
                            			WITH TIES *
                            			FROM EmployeeShiftAssign
                            			WHERE EffectiveDate <= '" + WorkDate + @"'
                            				AND EmpSystemID NOT IN (
                            					--**
                            					SELECT DISTINCT EmpSystemID
                            					FROM AttdnProcessData
                            					WHERE CONVERT(DATE, WorkDate) = CONVERT(DATE, '09-Jul-2024')
                            					)
                            			ORDER BY ROW_NUMBER() OVER (
                            					PARTITION BY EmpSystemID ORDER BY EffectiveDate DESC
                            					)
                            			) -- *
                            			ESA ON E.SystemId = ESA.EmpSystemID
                            		LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                            		LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                            		LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                            		LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
                            		LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = ENT.PlantId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = POS.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = POS.SectionId
                            		LEFT JOIN [ORG].[SubSection] ON SubSection.Id = POS.SubSectionId
                            		LEFT JOIN [HKP].[Designation] Designation ON Designation.Id = POS.DesignationId
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = E.EmployeeCodeTypeId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1
                            			AND (
                            				E.DOJ <= '" + WorkDate + @"'
                            				AND (
                            					E.DOS IS NULL
                            					OR E.DOS >= '" + WorkDate + @"'
                            					)
                            				) AND Ect.Id IN (" + typeList + @") " + includeTBS1 + @" " + includeLa1 + @"
                            		GROUP BY SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,E.BudgetCode
                            			,Line.UserName
                            			,Line.Id
                            		) AttdnNotProcessedToday ON AttdnNotProcessedToday.BudgetCode = M.Id
                            		AND AttdnNotProcessedToday.DesignationId = M.DesignationId
                            		AND AttdnNotProcessedToday.SubSectionId = M.SubSectionId
                            		 and AttdnNotProcessedToday.LineId = M.LineId
                            	LEFT JOIN (
                            		SELECT COUNT(E.SystemId) TotalEmployee
                            			,E.BudgetCode
                            			,SubSection.UserName SubSectionName
                            			,SubSection.Id SubSectionId
                            			,Designation.UserName DesignationName
                            			,Designation.Id DesignationId
                            			,SubSection.Sequence SubSectionSequence
                            			,Designation.Sequence DesignationSequence
                            			,Line.UserName LineName
                            			,ISNULL(Line.Id, '') LineId
                            		FROM ORG.CompanyGroup CG
                            		LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
                            		LEFT OUTER JOIN (
                            			--*
                            			SELECT *
                            			FROM EmployeeInformation
                            			WHERE SystemId NOT IN (
                            					--**
                            					SELECT DISTINCT EmpSystemID
                            					FROM EmployeeShiftAssign
                            					) --**
                            			) --*
                            			E ON e.GroupID = CG.Id
                            			AND c.Id = E.CompanyId
                            		LEFT JOIN [HKP].Designation GDes ON GDes.Id = E.GivenDesignationId
                            		LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = E.GivenDesignationId
                            		LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                            		LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = E.BudgetCode
                            		LEFT OUTER JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId
                            		LEFT OUTER JOIN ORG.Position AS POS ON POS.Id = MB.PositionId
                            		LEFT JOIN [ORG].[Plant] ON Plant.Id = ENT.PlantId
                            		LEFT JOIN [ORG].[Department] ON Department.Id = POS.DepartmentId
                            		LEFT JOIN [ORG].[Section] ON Section.Id = POS.SectionId
                            		LEFT JOIN [ORG].[SubSection] ON SubSection.Id = POS.SubSectionId
                            		LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                            		LEFT JOIN [HKP].[Designation] ON Designation.Id = POS.DesignationId
                                    LEFT JOIN  EmployeeCodeType ect ON  ect.Id = E.EmployeeCodeTypeId
                            		WHERE Cg.Id = '" + companyGroupId + @"'
                            			AND C.Id = '" + companyId + @"'
                            			AND E.PlantId IN (" + plantId + @")
                            			AND MB.Active = 1
                            			AND (
                            				E.DOJ <= '" + WorkDate + @"'
                            				AND (
                            					E.DOS IS NULL
                            					OR E.DOS >= '" + WorkDate + @"'
                            					)
                            				) AND Ect.Id IN (" + typeList + @") " + includeTBS1 + @" " + includeLa1 + @"
                            		GROUP BY SubSection.UserName
                            			,SubSection.Id
                            			,Designation.UserName
                            			,Designation.Id
                            			,SubSection.Sequence
                            			,Designation.Sequence
                            			,E.BudgetCode
                            			,Line.UserName
                            			,Line.Id
                            		) ShiftNotAssigned ON ShiftNotAssigned.BudgetCode = M.Id
                            		AND ShiftNotAssigned.DesignationId = M.DesignationId
                            		AND ShiftNotAssigned.SubSectionId = M.SubSectionId
                            		" + joiningLineShiftNotAssigned + @"
                            	) EDE
                            GROUP BY DesignationName,Code
                            	,EDE.EmpCategory,EDE.Department,EDE.SectionName
                            	,SubSectionName
                            	" + selectLine + @"
                            ORDER BY EDE.EmpCategory,EDE.Department,EDE.SectionName
                            	,SubSectionName " + selectLine + @"
                            	,DesignationName,Code";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }

        }//End Function

       
        public DataTable GetDailyAttendanceSummarySqlNew(string WorkDate, bool withLine, string companyGroupId, string companyId, string plantId)
        {
            string strSql = string.Empty;
            string selectLine = "";
            try
            {
                if (withLine == true)
                {
                    selectLine = ",A.LineName";
                }
                strSql = @"SELECT SUM(A.TotalManpower)TotalManpower,SUM(A.TotalNumber)ProposedManpowerBudget,A.ManpowerBudgetId,A.DivisionName,A.SectionName,A.SubSectionName
                                ,A.DesignationName,A.UnitName " + selectLine + @" ,A.EmpCategory,A.SUM_PRESENT,A.SUM_Late,A.SUM_Absent,A.SUM_Leave,A.SUM_Others
                                FROM
                                (SELECT ISNULL(ATT.TotalManpower,0)TotalManpower,ISNULL(MBD.TotalNumber,0) TotalNumber,MBD.ManpowerBudgetId
                                ,Division.UserName DivisionName,Section.UserName SectionName, SubSection.UserName SubSectionName,Designation.UserName DesignationName,Unit.UserName UnitName,Line.UserName LineName
                                ,ISNULL(ATT.PresentValue,0)SUM_PRESENT,ISNULL(ATT.LateValue,0)SUM_Late,ISNULL(ATT.AbsentValue,0)SUM_Absent,ISNULL(ATT.LeaveValue,0)SUM_Leave,ISNULL(ATT.HoliDayWeekOff,0)SUM_Others,ISNULL(ATT.EmpCategory,MPC.UserName)EmpCategory
                                FROM
                                (SELECT TOP 1 WITH TIES TotalNumber,ManpowerBudgetId,EffectiveDate
                                FROM [MST].[ManpowerBudgetDetail]
                                WHERE CONVERT(DATE,EffectiveDate) <= CONVERT(DATE,'" + WorkDate + @"')
                                ORDER BY ROW_NUMBER() OVER(PARTITION BY ManpowerBudgetId ORDER BY EffectiveDate DESC)
                                ) MBD
                                LEFT JOIN [MST].[ManpowerBudget] AS MB  on  Mb.Id = MBD.ManpowerBudgetId
                                LEFT JOIN (
		                                SELECT COUNT(e.SystemId) TotalManpower,E.BudgetCode
		                                ,SUM(apd.PresentValue) PresentValue,SUM(apd.LateValue)LateValue,SUM(apd.AbsentValue)AbsentValue,SUM(ISNULL(l.AvailedValue,0)) LeaveValue
		                                ,SUM(ISNULL(WeekOffValue,0)+ISNULL(HoliDayValue,0))HoliDayWeekOff,EmpC.UserName EmpCategory
		                                FROM EmployeeInformation E 
		                                LEFT JOIN AttdnProcessData apd on e.SystemId=apd.EmpSystemID  and APD.WorkDate='" + WorkDate + @"'
		                                LEFT JOIN SalaryProceAttdnData AS PAA on e.SystemId=PAA.EmpSystemID
		                                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=E.LegalDesignationId
		                                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=E.PlantId
		                                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
		                                LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = dm.EmployeeCategoryId
		                                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=E.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
		                                LEFT JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
		                                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id AND l.LeaveTypeId=apd.LTSystemID
		                                WHERE  (E.DOJ<='" + WorkDate + @"' AND (E.DOS is null or E.DOS >= '" + WorkDate + @"'))
		                                GROUP BY E.BudgetCode,EmpC.UserName
                                       ) AS ATT  ON ATT.BudgetCode=MBD.ManpowerBudgetId
                                LEFT JOIN [ORG].[CompanyGroup] AS Cg ON Cg.Id = MB.CompanyGroupId
                                LEFT JOIN [ORG].[Company] AS C ON C.CompanyGroupId = Cg.Id and mb.CompanyId= c.Id
                                LEFT JOIN [ORG].[Entity] AS E ON E.Id = MB.EntityId
                                LEFT JOIN [ORG].[Position] AS PO ON Po.Id = MB.PositionId
                                LEFT JOIN [ORG].[Plant] ON Plant.Id = E.PlantId
                                LEFT JOIN [ORG].[Division] Division on Division.Id=E.DivisionId
                                LEFT JOIN [ORG].[Section] Section on Section.Id=PO.SectionId
                                LEFT JOIN [ORG].[SubSection] SubSection on SubSection.Id=PO.SubSectionId
                                LEFT JOIN [HKP].[Designation] Designation on Designation.Id=PO.DesignationId
								 LEFT JOIN [MST].DesignationMaster DesM ON DesM.DesignationId = Designation.Id
								 LEFT JOIN [HKP].EmployeeCategory MPC ON MPC.Id = DesM.EmployeeCategoryId
                                LEFT JOIN [ORG].[Unit] ON Unit.Id = E.UnitId
                                LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
                                WHERE Cg.Id = '" + companyGroupId + @"' AND C.Id = '" + companyId + @"' AND Plant.Id = '" + plantId + @"' and MB.Active = 1 AND MBD.TotalNumber > 0) A 
                                GROUP BY A.EmpCategory,A.ManpowerBudgetId,A.DivisionName,A.UnitName,A.SectionName,A.SubSectionName,A.DesignationName " + selectLine + @" 
                                ,A.SUM_PRESENT,A.SUM_Late,A.SUM_Absent,A.SUM_Leave,A.SUM_Others";
                return _sqlRepository.GetDataTable(strSql);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public DataTable GetDailyCustomizedGroupWiseSummarySql(string WorkDate, string companyGroupId, string companyId, string plantId, bool isDesignation)
        {
            string strSql = "";
            string wc = "";
            string select = "";
            string grpBy = "";

            try
            {
                if (isDesignation == true)
                {
                    select = @"  SELECT EmployeeCategoryId,EmployeeCategory,UserDefineGroup1,UserDefineGroup2,UserDefineGroup3,UserDefineReportDesignation
						  ,SUM(OnRoleEmployee) OnRoleEmployee
						  ,SUM(TotalPresentEmployee) TotalPresentEmployee
						  ,SUM(TotalAbsentEmployee) TotalAbsentEmployee
						  ,SUM(TotalLateEmployee) TotalLateEmployee
						  ,SUM(TotalLeaveEmployee) TotalLeaveEmployee
						  ,SUM(TotalWeekoffEmployee) TotalWeekoffEmployee
						  ,SUM(totalMaternithyEmployee) totalMaternithyEmployee
                          FROM
                            (";
                    wc = @"WHERE ISNULL(UserDefineGroup4,'') <> ''";
                    grpBy = @") 
				                dd
				        Group by  EmployeeCategoryId,EmployeeCategory,UserDefineGroup1,UserDefineGroup2,UserDefineGroup3,UserDefineReportDesignation";
                }
                else
                {
                    select = @"  SELECT EmployeeCategoryId,EmployeeCategory,UserDefineGroup1
						  ,SUM(OnRoleEmployee) OnRoleEmployee
						  ,SUM(TotalPresentEmployee) TotalPresentEmployee
						  ,SUM(TotalAbsentEmployee) TotalAbsentEmployee
						  ,SUM(TotalLateEmployee) TotalLateEmployee
						  ,SUM(TotalLeaveEmployee) TotalLeaveEmployee
						  ,SUM(TotalWeekoffEmployee) TotalWeekoffEmployee
						  ,SUM(totalMaternithyEmployee) totalMaternithyEmployee
                          FROM
                            (";
                    grpBy = @") 
				                dd
				        Group by  EmployeeCategoryId,EmployeeCategory,UserDefineGroup1";

                }

                strSql = @"DECLARE @date DATETIME='" + WorkDate + @"'
                          ,@plantId varchar(10)='" + plantId + @"'
                        " + select + @"
						
				SELECT 
				 FORMAT(MAIN.Date,'dd-MMM-yyyy') Date,PL.UserName Plant,P.Id PositionId,P.Code PositionCode,EmpC.Id EmployeeCategoryId,EmpC.UserName EmployeeCategory,GDes.Id StandardDeignationId,GDes.UserName StandardDeignation
				,P.UserDefineGroup1,P.UserDefineGroup2,P.UserDefineGroup3,P.UserDefineGroup4,LD.UserName LegalDesignation, LD.UserDefineReportDesignation
				,MAIN.OnRoleEmployee,MAIN.TotalPresentEmployee,MAIN.TotalAbsentEmployee,MAIN.TotalLateEmployee,MAIN.TotalLeaveEmployee,MAIN.TotalWeekoffEmployee,MAIN.TotalMaternithyEmployee
				FROM	
				(SELECT 
						        @date as [Date],ONROLL.PositionId,ONROLL.LegalDesignationId
						        ,ISNULL(ONROLL.TotalEmployee,0) OnRoleEmployee
								,ISNULL(PRESENT.TotalPresentEmployee,0) TotalPresentEmployee
								,ISNULL(TABSENT.TotalAbsentEmployee,0) TotalAbsentEmployee
								,ISNULL(LATE.TotalLateEmployee,0) TotalLateEmployee
								,ISNULL(LEAVE.TotalLeaveEmployee,0) TotalLeaveEmployee
								,ISNULL(WeekOff.TotalWeekoffEmployee,0)TotalWeekoffEmployee		
                                ,isnull (MATERNITY.TotalMaternithyEmployee,0)totalMaternithyEmployee,ONROLL.PlantId
						 FROM 
				  (
					SELECT COUNT(E.SystemId) TotalEmployee,P.Id PositionId,E.LegalDesignationId,E.PlantId
						    	FROM   EmployeeInformation E
                                LEFT JOIN MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
								INNER JOIN ORG.Position P ON P.Id=MB.PositionId
								WHERE
								PlantId = @plantId AND (E.EmployeeStatus != 'Separated' OR ISNULL(E.DOS,'') = '' OR ISNULL(E.DOS,'')>CONVERT(DATE, @date ))
                                AND CONVERT(DATE,E.DOJ) <= CONVERT(DATE,@date)   
								GROUP BY P.Id,E.LegalDesignationId,E.PlantId
			   ) ONROLL  

			  LEFT JOIN (
								SELECT COUNT(E.SystemId) TotalPresentEmployee
                                ,P.Id PositionId,E.LegalDesignationId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN
							(
								SELECT E.SystemId,E.GroupID,E.CompanyId,E.BudgetCode,E.LegalDesignationId,E.PlantId FROM AttdnProcessData  APD  
								LEFT JOIN  EmployeeInformation E ON E.SystemId=APD.EmpSystemID
								LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
								WHERE DT.Category = 'Present' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,@date)
							)

								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
                                Left join MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
                                
                                INNER JOIN ORG.Position P ON P.Id=MB.PositionId
								WHERE E.PlantId = @plantId GROUP BY P.Id,E.LegalDesignationId
						) PRESENT ON ONROLL.PositionId=PRESENT.PositionId AND ONROLL.LegalDesignationId=PRESENT.LegalDesignationId

		 LEFT JOIN (
								SELECT COUNT(E.SystemId) TotalAbsentEmployee
                                ,P.Id PositionId,LD.Id LegalDesignationId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN
							(
								SELECT E.SystemId,E.GroupID,E.CompanyId,E.BudgetCode,E.LegalDesignationId,E.PlantId FROM AttdnProcessData  APD  
								LEFT JOIN  EmployeeInformation E ON E.SystemId=APD.EmpSystemID
								LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
								WHERE DT.Category = 'Absent' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,@date)
							)

								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
                                Left join MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
                                left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
                                INNER JOIN ORG.Position P ON P.Id=MB.PositionId
								WHERE E.PlantId = @plantId GROUP BY P.Id,LD.Id
						) TABSENT ON ONROLL.PositionId=TABSENT.PositionId AND ONROLL.LegalDesignationId=TABSENT.LegalDesignationId
		
		LEFT JOIN (
								SELECT COUNT(E.SystemId) TotalLateEmployee
                                ,P.Id PositionId,LD.Id LegalDesignationId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN
							(
								SELECT E.SystemId,E.GroupID,E.CompanyId,E.BudgetCode,E.LegalDesignationId,E.PlantId FROM AttdnProcessData  APD  
								LEFT JOIN  EmployeeInformation E ON E.SystemId=APD.EmpSystemID
								LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
								WHERE DT.Category = 'Late' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,@date)
							)

								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
                                Left join MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
                                left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
                                INNER JOIN ORG.Position P ON P.Id=MB.PositionId
								WHERE E.PlantId = @plantId GROUP BY P.Id,LD.Id
						) LATE ON ONROLL.PositionId=LATE.PositionId AND ONROLL.LegalDesignationId=LATE.LegalDesignationId

         LEFT JOIN (
								SELECT COUNT(E.SystemId) TotalLeaveEmployee
                                ,P.Id PositionId,LD.Id LegalDesignationId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN
							(
								SELECT E.SystemId,E.GroupID,E.CompanyId,E.BudgetCode,E.LegalDesignationId,E.PlantId FROM AttdnProcessData  APD  
								LEFT JOIN  EmployeeInformation E ON E.SystemId=APD.EmpSystemID
								LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
								WHERE DT.Category = 'Leave' AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,@date)
							)

								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
                                Left join MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
                                left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
                                INNER JOIN ORG.Position P ON P.Id=MB.PositionId
								WHERE E.PlantId = @plantId GROUP BY P.Id,LD.Id
						) LEAVE ON ONROLL.PositionId=LEAVE.PositionId AND ONROLL.LegalDesignationId=LEAVE.LegalDesignationId

		 LEFT JOIN (
								SELECT COUNT(E.SystemId) TotalWeekoffEmployee
                                ,P.Id PositionId,LD.Id LegalDesignationId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN
							(
								SELECT E.SystemId,E.GroupID,E.CompanyId,E.BudgetCode,E.LegalDesignationId,E.PlantId FROM AttdnProcessData  APD  
								LEFT JOIN  EmployeeInformation E ON E.SystemId=APD.EmpSystemID
								LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
								WHERE DT.Category IN('Holiday', 'Weekend') AND  CONVERT(DATE,WorkDate) = CONVERT(DATE,@date)
							)

								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
                                Left join MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
                                left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
                                INNER JOIN ORG.Position P ON P.Id=MB.PositionId
								WHERE E.PlantId = @plantId GROUP BY P.Id,LD.Id
						) Weekoff ON ONROLL.PositionId=Weekoff.PositionId AND ONROLL.LegalDesignationId=Weekoff.LegalDesignationId

		  LEFT JOIN (
								SELECT COUNT(E.SystemId) TotalMaternithyEmployee
                                ,P.Id PositionId,LD.Id LegalDesignationId
									FROM  ORG.CompanyGroup CG
									LEFT OUTER JOIN ORG.Company C ON CG.Id = c.CompanyGroupId
									LEFT OUTER JOIN
							(
								SELECT E.SystemId,E.GroupID,E.CompanyId,E.BudgetCode,E.LegalDesignationId,E.PlantId,lET.LeaveType,apd.WorkDate FROM AttdnProcessData  APD  
								LEFT JOIN  EmployeeInformation E ON E.SystemId=APD.EmpSystemID
								left join (select * from [dbo].[LeaveTransaction]
								where  (@date Between FromDate and ToDate ))LT on LT.EmpSystemID=e.SystemId and APD.LTSystemID=LT.LTSystemID 
								left join [dbo].[LeaveType] lET on  lET.Id = APD.LTSystemId 
							)

								E ON e.GroupID = CG.Id and c.Id = E.CompanyId
                                Left join MST.ManpowerBudget MB ON mb.Id = E.BudgetCode
                                left join [HKP].[LegalDesignation] as Ld on Ld.Id=E.LegalDesignationId
                                INNER JOIN ORG.Position P ON P.Id=MB.PositionId
								WHERE E.PlantId = @plantId and E.LeaveType='Maternity' and E.WorkDate=@date GROUP BY P.Id,LD.Id
						) MATERNITY ON ONROLL.PositionId=MATERNITY.PositionId AND ONROLL.LegalDesignationId=MATERNITY.LegalDesignationId
			) MAIN
			LEFT JOIN  ORG.Position P ON P.Id=MAIN.PositionId
			LEFT JOIN [HKP].[LegalDesignation] as Ld on Ld.Id=MAIN.LegalDesignationId
			LEFT JOIN [MST].DesignationMasterLegalDesignation LDM ON LDM.LegalDesignationId=MAIN.LegalDesignationId
			LEFT JOIN [MST].DesignationMaster DesM ON DesM.Id = LDM.DesignationMasterId
            LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
			LEFT JOIN [HKP].Designation GDes ON GDes.Id = DesM.DesignationId
			LEFT JOIN ORG.Plant PL ON PL.Id=MAIN.PlantId
            " + wc + @"
		    GROUP BY MAIN.PositionId,MAIN.Date,PL.UserName ,EmpC.Id,EmpC.UserName,GDes.Id,GDes.UserName
				,P.UserDefineGroup1,P.UserDefineGroup2,P.UserDefineGroup3,P.UserDefineGroup4,LD.UserName, LD.UserDefineReportDesignation
				,MAIN.OnRoleEmployee,MAIN.TotalPresentEmployee,MAIN.TotalAbsentEmployee
                ,MAIN.TotalLateEmployee,MAIN.TotalLeaveEmployee,MAIN.TotalWeekoffEmployee,MAIN.TotalMaternithyEmployee,P.Id,P.Code " + grpBy + @"";

                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }

        }//End Function

        #region Customized Attendance Summary Report
        public IWorkbook GetCustomizedAttendanceSummaryReport(string companyGroupId, string companyId, string PlantId, string workDate)
        {
            try
            {
                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                IWorksheet sheet2 = null;
                DataTable dtCmp = null;
                string maxUpTime = "";
                string minUpTime = "";

                DataTable dtMinMaxUpTime = _sqlRepository.GetDataTable(@"SELECT FORMAT(MAX(DateAdded),'dd-MMM-yyyy hh:mm:ss') MaxDateAdded, Format(Min(DateAdded),'dd-MMM-yyyy hh:mm:ss') MinDateAdded 
                                                                          FROM AttdnRawData WHERE PDate = '" + workDate + @"'");
                if (dtMinMaxUpTime.Rows.Count > 0)
                {
                    maxUpTime = dtMinMaxUpTime.Rows[0]["MaxDateAdded"].ToString();
                    minUpTime = dtMinMaxUpTime.Rows[0]["MinDateAdded"].ToString();

                }
                //clsReport objRpt = null;
                var objRpt = new clsReport(_sqlRepository);
                string userDefineGroup1 = "UserDefineGroup1";
                string userDefineGroup2 = "UserDefineGroup2";
                string userDefineGroup3 = "UserDefineGroup3";

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;
                int xlsHeaderRow = 1;
                #endregion Variable
                //Create dataset

                DataTable dtCustomizedGroupWiseSummaryReal = GetDailyCustomizedGroupWiseSummarySql(workDate, companyGroupId, companyId, PlantId, true);
                DataView dvCustomizedGroupWiseSummaryReal = dtCustomizedGroupWiseSummaryReal.DefaultView;
                dvCustomizedGroupWiseSummaryReal.Sort = "EmployeeCategory ,UserDefineGroup1, UserDefineGroup2, UserDefineGroup3,UserDefineReportDesignation";
                DataTable dtCustomizedGroupWiseSummary = dvCustomizedGroupWiseSummaryReal.ToTable(); //GetDailyCustomizedGroupWiseSummarySql(workDate, companyGroupId, companyId, PlantId);


                dvCustomizedGroupWiseSummaryReal = GetDailyCustomizedGroupWiseSummarySql(workDate, companyGroupId, companyId, PlantId, false).DefaultView;
                dvCustomizedGroupWiseSummaryReal.Sort = "EmployeeCategory ,UserDefineGroup1";

                DataTable dtCustomizedGroupWiseSummary2 = dvCustomizedGroupWiseSummaryReal.ToTable();

                DataTable dtUserDefinedGrpName = _sqlRepository.GetDataTable(@"SELECT [Id]
                                                                            ,ISNULL([UserDefineGroup1],'') UserDefineGroup1
                                                                            ,ISNULL([UserDefineGroup2],'') UserDefineGroup2
                                                                            ,ISNULL([UserDefineGroup3],'') UserDefineGroup3
                                                                            ,ISNULL([UserDefineGroup4],'') UserDefineGroup4
                                                                            ,ISNULL([UserDefineGroup5],'') UserDefineGroup5
                                                                            ,ISNULL([UserDefineGroup6],'') UserDefineGroup6      
                                                                                FROM [dbo].[PositionGroupingData]");
                if (dtUserDefinedGrpName.Rows.Count > 0)
                {
                    userDefineGroup1 = (dtUserDefinedGrpName.Rows[0]["UserDefineGroup1"].ToString() != "") ? dtUserDefinedGrpName.Rows[0]["UserDefineGroup1"].ToString() : userDefineGroup1;
                    userDefineGroup2 = (dtUserDefinedGrpName.Rows[0]["UserDefineGroup2"].ToString() != "") ? dtUserDefinedGrpName.Rows[0]["UserDefineGroup2"].ToString() : userDefineGroup2;
                    userDefineGroup3 = (dtUserDefinedGrpName.Rows[0]["UserDefineGroup3"].ToString() != "") ? userDefineGroup3 = dtUserDefinedGrpName.Rows[0]["UserDefineGroup3"].ToString() : userDefineGroup3;
                }

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[1];
                sheet2 = workbook.Worksheets[0];

                sheet1.IsGridLinesVisible = false;
                sheet2.IsGridLinesVisible = false;



                string CmpName;
                string FactoryName;
                FactoryName = string.Empty;
                dtCmp = objRpt.SelectedPlantWiseCompanyDT(PlantId);

                var FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                if (dtCmp.Rows.Count > 0)
                {
                    FactoryName = dtCmp.Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }


                #region Plant Address

                var dtFactory = objRpt.SelectedPlantDT(PlantId);

                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                #region sheet 1

                createOTFinalSummaryReportDeptSection(sheet2, dtCustomizedGroupWiseSummary2, FactoryName, CmpName, companyId, FactoryAddress, workDate, userDefineGroup1);
                #endregion

                xlsRow = 4;

                #region ColumnHeaderVariables              
                int cUserDefineGroup1 = 0; int cUserDefineGroup2 = 0; int cUserDefineGroup3 = 0; int cAttendancGroup = 0; int cOnRollManpower; int cBudgetedManPower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdTotalPresent = 0; int cfdWeekOff = 0; var cfdMatrenity = 0; int cEmpCategory = 0;
                #endregion
                #region ColumnHeaders
                sheet1.Range[xlsRow, 1].Text = "First Upload: " + minUpTime + ", Last Upload: " + maxUpTime;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 8;
                xlsRow++;
                sheet1.Range[xlsRow, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].RowHeight = 13;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                xlsRow++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Category", 10, ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, userDefineGroup1, 10, ExcelHAlign.HAlignCenter); cUserDefineGroup1 = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, userDefineGroup2, 10, ExcelHAlign.HAlignCenter); cUserDefineGroup2 = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, userDefineGroup3, 10, ExcelHAlign.HAlignCenter); cUserDefineGroup3 = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation.", 14, ExcelHAlign.HAlignCenter); cAttendancGroup = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 10, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 10, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 10, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Total Present", 10, ExcelHAlign.HAlignCenter); cfdTotalPresent = xlsCol; xlsCol++;

                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 10, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 10, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "WeekOff", 10, ExcelHAlign.HAlignCenter); cfdWeekOff = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Maternity", 10, ExcelHAlign.HAlignCenter); cfdMatrenity = xlsCol++;


                sheet1.Range[xlsRow, cEmpCategory, xlsRow, cfdMatrenity].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                // sheet1.Range[xlsRow, cEmpCategory, xlsRow, cfdMatrenity].CellStyle.Font.Color = ExcelKnownColors.White;

                xlsHeaderRow = xlsRow;
                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                if (dtCustomizedGroupWiseSummary.Rows.Count > 0)
                {
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _grp3 = string.Empty;
                    string _grp4 = string.Empty;
                    string _grp5 = string.Empty;


                    #endregion
                    var catFRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;
                    var catGrp5FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;
                    for (int i = 0; i < dtCustomizedGroupWiseSummary.Rows.Count; i++)
                    {

                        var catLRow = xlsRow;
                        if (_grp1 != dtCustomizedGroupWiseSummary.Rows[i]["EmployeeCategory"].ToString() && string.IsNullOrEmpty(dtCustomizedGroupWiseSummary.Rows[i]["EmployeeCategory"].ToString()) == false)
                        {
                            _grp1 = dtCustomizedGroupWiseSummary.Rows[i]["EmployeeCategory"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastMPGroup = dtCustomizedGroupWiseSummary.Rows[i]["EmployeeCategory"].ToString();

                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].Merge();
                                sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdTotalPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdTotalPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdTotalPresent) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdWeekOff].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdWeekOff) + catFRow + ":" + oRU.GetColumnNameForXls(cfdWeekOff) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdMatrenity].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdMatrenity) + catFRow + ":" + oRU.GetColumnNameForXls(cfdMatrenity) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cFdPresent, xlsRow, cfdWeekOff].CellStyle.Font.Bold = true;

                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdTotalPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdWeekOff].BorderAround(ExcelLineStyle.Hair);

                                xlsRow++;
                            }
                            #endregion


                            SetCellText(sheet1, xlsRow, cEmpCategory, dtCustomizedGroupWiseSummary.Rows[i]["EmployeeCategory"].ToString(), 8);

                            SetCellText(sheet1, xlsRow, cUserDefineGroup1, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup1"].ToString(), 8);
                            SetCellText(sheet1, xlsRow, cUserDefineGroup2, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup2"].ToString(), 8);
                            SetCellText(sheet1, xlsRow, cUserDefineGroup3, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup3"].ToString(), 8);
                            SetCellText(sheet1, xlsRow, cAttendancGroup, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineReportDesignation"].ToString(), 8);
                            if (catFRow < xlsRow)
                            {
                                catFRow = xlsRow;
                                catGrp2FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;
                                catGrp5FRow = xlsRow;
                            }
                        }
                        if (_grp2 != _grp1 + dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup1"].ToString())
                        {
                            _grp2 = _grp1 + dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup1"].ToString();

                            #region Subtotal
                            if (catGrp2FRow < xlsRow)
                            {
                                lastMPGroup = dtCustomizedGroupWiseSummary.Rows[i]["EmployeeCategory"].ToString();

                                SetHeadText(sheet1, xlsRow, 2, " Subtotal:");
                                sheet1.Range[xlsRow, 2, xlsRow, (cOnRollManpower - 1)].Merge();
                                sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catGrp2FRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catGrp2FRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catGrp2FRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catGrp2FRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdTotalPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdTotalPresent) + catGrp2FRow + ":" + oRU.GetColumnNameForXls(cfdTotalPresent) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catGrp2FRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdWeekOff].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdWeekOff) + catGrp2FRow + ":" + oRU.GetColumnNameForXls(cfdWeekOff) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdMatrenity].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdMatrenity) + catGrp2FRow + ":" + oRU.GetColumnNameForXls(cfdMatrenity) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cFdPresent, xlsRow, cfdMatrenity].CellStyle.Font.Bold = true;

                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdTotalPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdWeekOff].BorderAround(ExcelLineStyle.Hair);

                                xlsRow++;
                            }
                            #endregion


                            SetCellText(sheet1, xlsRow, cUserDefineGroup1, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup1"].ToString(), 8);


                            SetCellText(sheet1, xlsRow, cUserDefineGroup2, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup2"].ToString(), 8);
                            SetCellText(sheet1, xlsRow, cUserDefineGroup3, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup3"].ToString(), 8);
                            SetCellText(sheet1, xlsRow, cAttendancGroup, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineReportDesignation"].ToString(), 8);
                            if (catGrp2FRow < xlsRow)
                            {
                                catGrp2FRow = xlsRow;
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;
                                catGrp5FRow = xlsRow;



                            }
                        }
                        if (_grp3 != _grp2 + dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup2"].ToString())
                        {
                            _grp3 = _grp2 + dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup2"].ToString();

                            #region Subtotal
                            if (catGrp3FRow < xlsRow)
                            {
                                lastMPGroup = dtCustomizedGroupWiseSummary.Rows[i]["EmployeeCategory"].ToString();
                                rowList.Add(xlsRow);

                                SetHeadText(sheet1, xlsRow, 3, " Subtotal:");
                                sheet1.Range[xlsRow, 3, xlsRow, (cOnRollManpower - 1)].Merge();
                                sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdTotalPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdTotalPresent) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdTotalPresent) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdWeekOff].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdWeekOff) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdWeekOff) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdMatrenity].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdMatrenity) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdMatrenity) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cFdPresent, xlsRow, cfdMatrenity].CellStyle.Font.Bold = true;

                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdTotalPresent].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdWeekOff].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cfdMatrenity].BorderAround(ExcelLineStyle.Hair);


                                xlsRow++;
                            }
                            #endregion

                            SetCellText(sheet1, xlsRow, cUserDefineGroup2, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup2"].ToString(), 8);
                            SetCellText(sheet1, xlsRow, cUserDefineGroup3, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup3"].ToString(), 8);
                            SetCellText(sheet1, xlsRow, cAttendancGroup, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineReportDesignation"].ToString(), 8);
                            if (catGrp3FRow < xlsRow)
                            {
                                catGrp3FRow = xlsRow;
                                catGrp4FRow = xlsRow;
                                catGrp5FRow = xlsRow;

                            }
                        }
                        if (_grp4 != _grp3 + dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup3"].ToString())
                        {

                            _grp4 = _grp3 + dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup3"].ToString();

                            SetCellText(sheet1, xlsRow, cUserDefineGroup3, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineGroup3"].ToString(), 8);

                            SetCellText(sheet1, xlsRow, cAttendancGroup, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineReportDesignation"].ToString(), 8);
                            if (catGrp4FRow < xlsRow)
                            {
                                catGrp4FRow = xlsRow;
                                catGrp5FRow = xlsRow;

                            }

                        }
                        if (_grp5 != _grp4 + dtCustomizedGroupWiseSummary.Rows[i]["UserDefineReportDesignation"].ToString())
                        {


                            _grp5 = _grp4 + dtCustomizedGroupWiseSummary.Rows[i]["UserDefineReportDesignation"].ToString();
                            SetCellText(sheet1, xlsRow, cAttendancGroup, dtCustomizedGroupWiseSummary.Rows[i]["UserDefineReportDesignation"].ToString(), 8);

                            sheet1.Range[catFRow, cEmpCategory, xlsRow, cEmpCategory].Merge();

                            sheet1.Range[catFRow, cEmpCategory, xlsRow, cEmpCategory].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[catGrp2FRow, cUserDefineGroup1, xlsRow, cUserDefineGroup1].Merge();

                            sheet1.Range[catGrp2FRow, cUserDefineGroup1, xlsRow, cUserDefineGroup1].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[catGrp3FRow, cUserDefineGroup2, xlsRow, cUserDefineGroup2].Merge();
                            sheet1.Range[catGrp3FRow, cUserDefineGroup2, xlsRow, cUserDefineGroup2].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[catGrp4FRow, cUserDefineGroup3, xlsRow, cUserDefineGroup3].Merge();
                            sheet1.Range[catGrp4FRow, cUserDefineGroup3, xlsRow, cUserDefineGroup3].BorderAround(ExcelLineStyle.Hair);


                        }
                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary.Rows[i]["OnRoleEmployee"].ToString()), 8);
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary.Rows[i]["TotalPresentEmployee"].ToString()), 8);//LegalDesignation
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary.Rows[i]["TotalAbsentEmployee"].ToString()), 8);//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary.Rows[i]["TotalLateEmployee"].ToString()), 8);//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdTotalPresent, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary.Rows[i]["TotalLateEmployee"].ToString()) + clsStaticInfo.dbl(dtCustomizedGroupWiseSummary.Rows[i]["TotalPresentEmployee"].ToString()), 8);//

                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary.Rows[i]["TotalLeaveEmployee"].ToString()) - clsStaticInfo.dbl(dtCustomizedGroupWiseSummary.Rows[i]["totalMaternithyEmployee"].ToString()), 8);//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdWeekOff, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary.Rows[i]["TotalWeekoffEmployee"].ToString()), 8);//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdMatrenity, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary.Rows[i]["totalMaternithyEmployee"].ToString()), 8);//

                        xlsRow++;

                    }
                    //xlsRow += 1;
                    rowList.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].Merge();


                    sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);


                    sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdTotalPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdTotalPresent) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdTotalPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdTotalPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdWeekOff].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdWeekOff) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdWeekOff) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdWeekOff].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdMatrenity].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdMatrenity) + catGrp3FRow + ":" + oRU.GetColumnNameForXls(cfdMatrenity) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdMatrenity].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].CellStyle.Font.Bold = true;
                    xlsRow++;

                    //rowList.Add(xlsRow);
                    //SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                    //sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].Merge();


                    //sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (catGrp2FRow - 1) + ")";
                    //sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);


                    //sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                    //sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdTotalPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdTotalPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdTotalPresent) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cfdTotalPresent].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdWeekOff].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdWeekOff) + catFRow + ":" + oRU.GetColumnNameForXls(cfdWeekOff) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cfdWeekOff].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdMatrenity].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdMatrenity) + catFRow + ":" + oRU.GetColumnNameForXls(cfdMatrenity) + (xlsRow - 1) + ")";
                    //sheet1.Range[xlsRow, cfdMatrenity].BorderAround(ExcelLineStyle.Hair);

                    //sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].CellStyle.Font.Bold = true;
                    //xlsRow++;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdTotalPresent].Formula = GetFormulaGrandTotal(rowList, cfdTotalPresent);

                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdWeekOff].Formula = GetFormulaGrandTotal(rowList, cfdWeekOff);
                    sheet1.Range[xlsRow, cfdMatrenity].Formula = GetFormulaGrandTotal(rowList, cfdMatrenity);



                    sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdTotalPresent].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdTotalPresent].BorderAround(ExcelLineStyle.Hair);

                    //sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdWeekOff].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow, cfdMatrenity].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].BorderInside(ExcelLineStyle.Hair);






                    sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = false;
                    //sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    //sheet1.Range[xlsHeaderRow, cOnRollManpower, xlsRow, cfdMatrenity].CellStyle.Font.Size = 8;

                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion
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
                    sheet1.Range[xlsRow, 4].Text = FactoryName;
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 4, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 4].RowHeight = 30;

                    xlsRow += 1;

                    sheet1.Range[xlsRow, 4].Text = FactoryAddress;
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 4, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 4].RowHeight = 30;
                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, 4].Text = "Production Manpower Attendance Summary";
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 4].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 4].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 4, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    //sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    sheet1.Name = "R-2";
                    sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    sheet1.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    #endregion UsedRange Alignment

                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    //sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Min Time :" +minUpTime+ "Max Time :" + maxUpTime + "";
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet1.PageSetup.PrintGridlines = false;
                    sheet1.PageSetup.CenterVertically = false;
                    sheet1.IsDisplayZeros = false;



                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion

        public void createOTFinalSummaryReportDeptSection(IWorksheet sheet1, DataTable dtCustomizedGroupWiseSummary2, string FactoryName, string CmpName, string companyId, string FactoryAddress, string workDate, string userDefineGroup1)
        {
            try
            {

                ReportUtility oRU = new ReportUtility();

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;
                int xlsHeaderRow = 1;

                xlsRow = 4;

                #region ColumnHeaderVariables              
                int cUserDefineGroup1 = 0; int cUserDefineGroup2 = 0; int cUserDefineGroup3 = 0; int cAttendancGroup = 0; int cOnRollManpower; int cFdPresent = 0; int cfdAbsent = 0;
                int cfdLeave = 0; int cfdLate = 0; int cfdWeekOff = 0; int cfdTotalPresent = 0; var cfdMatrenity = 0; int cEmpCategory = 0;
                #endregion
                #region ColumnHeaders
                sheet1.Range[xlsRow, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1].RowHeight = 13;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                xlsRow++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Employee Category", 10, ExcelHAlign.HAlignCenter); cEmpCategory = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, userDefineGroup1, 14, ExcelHAlign.HAlignCenter); cUserDefineGroup1 = xlsCol; xlsCol++;

                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OnRoll", 10, ExcelHAlign.HAlignCenter); cOnRollManpower = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Present", 10, ExcelHAlign.HAlignCenter); cFdPresent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Late", 10, ExcelHAlign.HAlignCenter); cfdLate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Total Present", 10, ExcelHAlign.HAlignCenter); cfdTotalPresent = xlsCol; xlsCol++;

                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Absent", 10, ExcelHAlign.HAlignCenter); cfdAbsent = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Leave", 10, ExcelHAlign.HAlignCenter); cfdLeave = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "WeekOff", 10, ExcelHAlign.HAlignCenter); cfdWeekOff = xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Maternity", 10, ExcelHAlign.HAlignCenter); cfdMatrenity = xlsCol++;

                sheet1.Range[xlsRow, cEmpCategory, xlsRow, cfdMatrenity].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                // sheet1.Range[xlsRow, cEmpCategory, xlsRow, cfdMatrenity].CellStyle.Font.Color = ExcelKnownColors.White;
                var orgCollist = xlsCol;
                xlsRow++;
                xlsHeaderRow = xlsRow;
                endXlsCol = xlsCol;

                if (dtCustomizedGroupWiseSummary2.Rows.Count > 0)
                {
                    string _grp1 = string.Empty;
                    string _grp2 = string.Empty;
                    string _grp3 = string.Empty;
                    string _grp4 = string.Empty;
                    string _grp5 = string.Empty;


                    #endregion
                    var catFRow = xlsRow;
                    var catGrp2FRow = xlsRow;
                    var catGrp3FRow = xlsRow;
                    var catGrp4FRow = xlsRow;
                    var catGrp5FRow = xlsRow;

                    ArrayList rowList = new ArrayList();
                    var lastMPGroup = string.Empty;
                    for (int i = 0; i < dtCustomizedGroupWiseSummary2.Rows.Count; i++)
                    {
                        var catLRow = xlsRow;
                        if (_grp1 != dtCustomizedGroupWiseSummary2.Rows[i]["EmployeeCategory"].ToString() && string.IsNullOrEmpty(dtCustomizedGroupWiseSummary2.Rows[i]["EmployeeCategory"].ToString()) == false)
                        {
                            _grp1 = dtCustomizedGroupWiseSummary2.Rows[i]["EmployeeCategory"].ToString();

                            #region Subtotal
                            if (catFRow < xlsRow)
                            {
                                lastMPGroup = _grp1;
                                rowList.Add(xlsRow);
                                SetHeadText(sheet1, xlsRow, 1, " Subtotal:");
                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].Merge();
                                sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdTotalPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdTotalPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdTotalPresent) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdWeekOff].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdWeekOff) + catFRow + ":" + oRU.GetColumnNameForXls(cfdWeekOff) + (xlsRow - 1) + ")";
                                sheet1.Range[xlsRow, cfdMatrenity].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdMatrenity) + catFRow + ":" + oRU.GetColumnNameForXls(cfdMatrenity) + (xlsRow - 1) + ")";

                                sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].CellStyle.Font.Bold = true;

                                sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].BorderInside(ExcelLineStyle.Hair);
                                xlsRow++;
                            }
                            #endregion



                            SetCellText(sheet1, xlsRow, cEmpCategory, dtCustomizedGroupWiseSummary2.Rows[i]["EmployeeCategory"].ToString(), 8);

                            SetCellText(sheet1, xlsRow, cUserDefineGroup1, dtCustomizedGroupWiseSummary2.Rows[i]["UserDefineGroup1"].ToString(), 8);

                            if (catFRow < xlsRow)
                            {
                                catFRow = xlsRow;
                                catGrp2FRow = xlsRow;

                            }
                        }

                        if (_grp2 != _grp1 + dtCustomizedGroupWiseSummary2.Rows[i]["UserDefineGroup1"].ToString())
                        {
                            _grp2 = _grp1 + dtCustomizedGroupWiseSummary2.Rows[i]["UserDefineGroup1"].ToString();
                            SetCellText(sheet1, xlsRow, cUserDefineGroup1, dtCustomizedGroupWiseSummary2.Rows[i]["UserDefineGroup1"].ToString(), 8);

                            sheet1.Range[catFRow, cEmpCategory, xlsRow, cEmpCategory].Merge();
                            sheet1.Range[catFRow, cEmpCategory, xlsRow, cEmpCategory].BorderAround(ExcelLineStyle.Hair);
                        }
                        oRU.SetTextBorder(ref sheet1, xlsRow, cOnRollManpower, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary2.Rows[i]["OnRoleEmployee"].ToString()), 8);
                        oRU.SetTextBorder(ref sheet1, xlsRow, cFdPresent, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary2.Rows[i]["TotalPresentEmployee"].ToString()), 8);//LegalDesignation
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdAbsent, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary2.Rows[i]["TotalAbsentEmployee"].ToString()), 8);//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLate, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary2.Rows[i]["TotalLateEmployee"].ToString()), 8);//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdTotalPresent, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary2.Rows[i]["TotalPresentEmployee"].ToString()) + clsStaticInfo.dbl(dtCustomizedGroupWiseSummary2.Rows[i]["TotalLateEmployee"].ToString()), 8);//

                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdLeave, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary2.Rows[i]["TotalLeaveEmployee"].ToString()) - clsStaticInfo.dbl(dtCustomizedGroupWiseSummary2.Rows[i]["totalMaternithyEmployee"].ToString()), 8);//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdWeekOff, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary2.Rows[i]["TotalWeekoffEmployee"].ToString()), 8);//
                        oRU.SetTextBorder(ref sheet1, xlsRow, cfdMatrenity, clsStaticInfo.dbl(dtCustomizedGroupWiseSummary2.Rows[i]["totalMaternithyEmployee"].ToString()), 8);//

                        xlsRow++;
                    }
                    //xlsRow += 1;

                    rowList.Add(xlsRow);
                    SetHeadText(sheet1, xlsRow, 1, " Subtotal:");

                    sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].Merge();

                    sheet1.Range[xlsRow, cOnRollManpower].Formula = "=SUM(" + oRU.GetColumnNameForXls(cOnRollManpower) + catFRow + ":" + oRU.GetColumnNameForXls(cOnRollManpower) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cOnRollManpower].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cFdPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cFdPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cFdPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cFdPresent].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cfdAbsent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdAbsent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdAbsent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdAbsent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLate].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLate) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLate) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLate].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdTotalPresent].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdTotalPresent) + catFRow + ":" + oRU.GetColumnNameForXls(cfdTotalPresent) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdTotalPresent].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdLeave].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdLeave) + catFRow + ":" + oRU.GetColumnNameForXls(cfdLeave) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdLeave].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdWeekOff].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdWeekOff) + catFRow + ":" + oRU.GetColumnNameForXls(cfdWeekOff) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdWeekOff].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cfdMatrenity].Formula = "=SUM(" + oRU.GetColumnNameForXls(cfdMatrenity) + catFRow + ":" + oRU.GetColumnNameForXls(cfdMatrenity) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, cfdMatrenity].BorderAround(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].CellStyle.Font.Bold = true;
                    xlsRow++;

                    SetHeadText(sheet1, xlsRow, 1, "Grand Total:");
                    sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].Merge();
                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);
                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);

                    sheet1.Range[xlsRow, cOnRollManpower].Formula = GetFormulaGrandTotal(rowList, cOnRollManpower);

                    sheet1.Range[xlsRow, cFdPresent].Formula = GetFormulaGrandTotal(rowList, cFdPresent);
                    sheet1.Range[xlsRow, cfdAbsent].Formula = GetFormulaGrandTotal(rowList, cfdAbsent);
                    sheet1.Range[xlsRow, cfdLate].Formula = GetFormulaGrandTotal(rowList, cfdLate);
                    sheet1.Range[xlsRow, cfdTotalPresent].Formula = GetFormulaGrandTotal(rowList, cfdTotalPresent);

                    sheet1.Range[xlsRow, cfdLeave].Formula = GetFormulaGrandTotal(rowList, cfdLeave);
                    sheet1.Range[xlsRow, cfdWeekOff].Formula = GetFormulaGrandTotal(rowList, cfdWeekOff);
                    sheet1.Range[xlsRow, cfdMatrenity].Formula = GetFormulaGrandTotal(rowList, cfdMatrenity);


                    sheet1.Range[xlsRow, 1, xlsRow, (cOnRollManpower - 1)].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].BorderInside(ExcelLineStyle.Hair);

                    sheet1.Range[xlsRow, cOnRollManpower, xlsRow, cfdMatrenity].CellStyle.Font.Bold = true;

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    //sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range[xlsHeaderRow, cOnRollManpower, xlsHeaderRow, cfdMatrenity].CellStyle.Font.Size = 10;

                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion



                    xlsRow = 1;
                    xlsCol = 1;
                    string strPath = "";
                    Image companyLogo = null;
                    try
                    {
                        strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        companyLogo = Image.FromFile(strPath);
                    }
                    catch (Exception)
                    {
                    }
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);


                    }

                    sheet1.Range[xlsRow, 3].Text = FactoryName;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 3, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 3].RowHeight = 30;

                    #region Plant Address
                    xlsRow += 1;


                    sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 3, xlsRow, Convert.ToInt32(endXlsCol)].Merge();
                    sheet1.Range[xlsRow, 3].RowHeight = 30;
                    #endregion
                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Manpower Attendance Summary on " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 15;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 24;


                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A6"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    sheet1.Name = "R-1";
                    sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    #endregion UsedRange Alignment

                    //oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet1.PageSetup.PrintGridlines = false;
                    sheet1.PageSetup.CenterVertically = false;
                    sheet1.IsDisplayZeros = false;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
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
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text, double fontSize)
        {
            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Size = fontSize;
        }

        public IWorkbook GetAttendancFromAppSummaryExcel1(string PlantId, string companyId, string workDate, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID)
        {

            try
            {


                #region Variable
                ReportUtility oRU = new ReportUtility();
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                DataView dvDaily = null;
                DataSet dsCmp = null;
                clsReport objRpt = null;

                int xlsRow = 1, xlsCol = 1; int endXlsCol = 1;

                #endregion Variable
                //Create dataset
                DataTable dtAttSummary = GetAttendancFromAppSummarySql1(PlantId, companyId, workDate, sUnitID, sDivID, sDepID, sSecID, sSubSecID);
                dvDaily = new DataView(dtAttSummary);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;


                string CmpName;
                string FactoryName;


                xlsRow = 5;

                #region ColumnHeaderVariables              
                int cSrNo = 0; int cEmployeeCode = 0; int cEmployeeName = 0; int cDepartment = 0; int cDesignation = 0; int cinTime = 0; int cOutTime; int cPDate; int cLatitude = 0; int cLongitude = 0;
                #endregion
                #region ColumnHeaders
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sr.No", ExcelHAlign.HAlignCenter); cSrNo = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeCode", ExcelHAlign.HAlignCenter); cEmployeeCode = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "EmployeeName", ExcelHAlign.HAlignCenter); cEmployeeName = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Department", ExcelHAlign.HAlignCenter); cDepartment = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", ExcelHAlign.HAlignCenter); cDesignation = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "inTime", ExcelHAlign.HAlignCenter); cinTime = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "OutTime", ExcelHAlign.HAlignCenter); cOutTime = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "PDate", ExcelHAlign.HAlignCenter); cPDate = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Latitude", ExcelHAlign.HAlignCenter); cLatitude = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Longitude", ExcelHAlign.HAlignCenter); cLongitude = xlsCol;
                //oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "DOJ", ExcelHAlign.HAlignCenter); cDOJ = xlsCol; xlsCol++;
                //oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Absent Days", ExcelHAlign.HAlignCenter); cAbsentDays = xlsCol; xlsCol++;

                var orgCollist = xlsCol;
                xlsRow++;
                endXlsCol = xlsCol;

                if (dtAttSummary.Rows.Count > 0)
                {

                    #endregion
                    var slCount = 0;
                    for (int i = 0; i < dtAttSummary.Rows.Count; i++)
                    {
                        slCount++;

                        oRU.SetText(ref sheet1, xlsRow, cSrNo, slCount.ToString());
                        oRU.SetText(ref sheet1, xlsRow, cEmployeeCode, dtAttSummary.Rows[i]["EmployeeCode"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cEmployeeName, dtAttSummary.Rows[i]["EmployeeName"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cDepartment, dtAttSummary.Rows[i]["Department"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cDesignation, dtAttSummary.Rows[i]["Designation"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cinTime, dtAttSummary.Rows[i]["inTime"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cOutTime, dtAttSummary.Rows[i]["OutTime"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cPDate, dtAttSummary.Rows[i]["PDate"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cLatitude, dtAttSummary.Rows[i]["Latitude"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, cLongitude, dtAttSummary.Rows[i]["Longitude"].ToString());//LegalDesignation
                                                                                                                  //  oRU.SetText(ref sheet1, xlsRow, cfdAbsent, dtAttSummary.Rows[i]["TotalAbsent"].ToString());//
                                                                                                                  //oRU.SetText(ref sheet1, xlsRow, cGDesig, dtManPBSummary.Rows[i]["GivenDesignation"].ToString());//
                                                                                                                  //oRU.SetText(ref sheet1, xlsRow, cDOJ, dtManPBSummary.Rows[i]["DOJ"].ToString());//
                                                                                                                  //oRU.SetText(ref sheet1, xlsRow, cAbsentDays, dtManPBSummary.Rows[i]["AbsentDays"].ToString());// 
                        xlsRow++;
                    }
                    xlsRow += 1;
                    #region Line Setup
                    //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment


                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    //sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;

                    #endregion

                    //sheet1.Range[11, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    //sheet1.Range[11, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    //sheet1.Range[11, 4, xlsRow, 4].WrapText = true;
                    objRpt = new clsReport();
                    objRpt.SelectedPlantWiseCompany(PlantId, "", out dsCmp);
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
                    //sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                    //sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //xlsRow += 1;
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, 1].Text = FactoryName;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, Convert.ToInt32(endXlsCol / 2)].Merge();
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].Text = FactoryAddress;
                    sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 18;

                    sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, Convert.ToInt32(endXlsCol / 2) + 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Attendanc From App " + Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 20;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 24;

                    //#endregion *****************Report Header*****************
                    #region Freeze Panes
                    sheet1.UsedRange["A5"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 5;
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    //sheet1.UsedRange.CellStyle.Font.Size = 8;
                    //  oRU.CompanyPlantHeader(ref sheet1, endXlsCol, "Joining Information from ",companyId, plantName);
                    //sheet1.Range[oRU.GetColumnNameForXls(1) + 5 + ":" + oRU.GetColumnNameForXls(endXlsCol) + 5].Merge();
                    oRU.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                }



                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private DataTable GetAttendancFromAppSummarySql1(string PlantId, string companyId, string workDate, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID)
        {
            string strSql1;

            try
            {


                strSql1 = @"select e.EmployeeCode,e.EmployeeName,d.UserName as Department,PDate=REPLACE(CONVERT(VARCHAR(11),PDate, 106), ' ', '-') ,inTime=FORMAT( inTime, 'hh.mm tt'),OutTime=FORMAT( OutTime, 'hh.mm tt'),Latitude,Longitude
                      ,isnull(LDs.Username,GDE.Username)Designation from AttdnRawDataFromApp a
                        LEFT JOIN EmployeeInformation e ON a.EmployeeId = e.SystemId
                        LEFT JOIN Org.Department d ON e.DepartmentId = d.Id 
					    LEFT JOIN HKP.Designation GDe ON GDe.Id = e.GivenDesignationId
					    LEFT JOIN HKP.LegalDesignation  LDs ON LDs.Id =e.LegalDesignationId
				       where a.PDate = '" + workDate + @"'";




                return _sqlRepository.GetDataTable(strSql1);
            }
            catch (Exception)
            {

                throw;
            }




        }

        private DataTable GetDataManpowerAttendanceSummarySql(string PlantId, string companyId, string workDate, string sUnitID, string sDivID, string sDepID, string sSecID, string sSubSecID)
        {
            string strSql;
            try
            {


                strSql = @"SELECT Count(SystemId) AS OnRoll
	                                    ,COUNT(APD.EmpSystemID) AS TotalPresent
	                                    ,COUNT(APDA.EmpSystemID) AS TotalAbsent
	                                    ,COUNT(APDLV.EmpSystemID) AS TotalLV
	                                    ,COUNT(APDL.EmpSystemID) AS TotalLate
                                        ,COUNT(APDW.EmpSystemId) + Count(AttdnNotProcessedToday.EmpSystemId) + Count(ShiftNotAssigned.EmpSystemId)  as Others
	                                    ,ISNULL(BudgetedManPower, 0) BudgetedManPower
	                                    ,iSNULL(Group1,'Attendance Group Not Found') Group1,
										ISNULL(Group2,'Attendance Group Not Found') Group2,ISNULL(Group3,'Attendance Group Not Found') Group3
                                   
                                    FROM EmployeeInformation AS EEI
                                    LEFT JOIN EmployeeAttendanceGroup EAG ON EEI.SystemId = EAG.EmployeeId
                                    LEFT JOIN AttendanceGroup AS AG ON AG.Id = EAG.AttendanceGroupId
                                    LEFT JOIN (
	                                    SELECT *
	                                    FROM AttdnProcessData APD
	                                    LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
	                                    WHERE WorkDate = '" + workDate + @"'
		                                    AND DT.Category = 'Present'
	                                    ) APD ON APD.EmpSystemID = EEI.SystemId
                                    LEFT JOIN (
	                                    SELECT *
	                                    FROM AttdnProcessData APD
	                                    LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
	                                    WHERE WorkDate = '" + workDate + @"'
		                                    AND DT.Category = 'Absent'
	                                    ) APDA ON APDA.EmpSystemID = EEI.SystemId
                                    LEFT JOIN (
	                                    SELECT *
	                                    FROM AttdnProcessData APD
	                                    LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
	                                    WHERE WorkDate = '" + workDate + @"'
		                                    AND DT.Category = 'Leave'
	                                    ) APDLV ON APDLV.EmpSystemID = EEI.SystemId
                                    LEFT JOIN (
	                                    SELECT *
	                                    FROM AttdnProcessData APD
	                                    LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
	                                    WHERE WorkDate = '" + workDate + @"'
		                                    AND DT.Category = 'Late'
	                                    ) APDL ON APDL.EmpSystemID = EEI.SystemId
                                    LEFT JOIN (
	                                    SELECT *
	                                    FROM AttdnProcessData APD
	                                    LEFT JOIN DayType DT ON APD.DayStatus = DT.DayType
	                                    WHERE WorkDate = '" + workDate + @"'
		                                    AND DT.Category = 'Weekend'
	                                    ) APDW ON APDW.EmpSystemID = EEI.SystemId
		                            LEFT JOIN
									(
									SELECT EEI.SystemId EmpSystemId FROM EmployeeInformation EEI where SystemId Not In(select EmpSystemID from AttdnProcessData where WorkDate = '" + workDate + @"'  )                
                                 
								) AttdnNotProcessedToday  
									ON AttdnNotProcessedToday.EmpSystemId = EEI.SystemId
									left join
											(
									SELECT EEI.SystemId EmpSystemId FROM EmployeeInformation EEI where SystemId Not In(select distinct EmpSystemID from EmployeeShiftAssign)                
                                 
								) ShiftNotAssigned  
									ON ShiftNotAssigned.EmpSystemId = EEI.SystemId
                                    WHERE (DOJ<='" + workDate + @"' AND (DOS is null or DOS >= '" + workDate + @"')) ";


                if (sUnitID != "ALL" && sUnitID != null && sUnitID != "null" && sUnitID != "undefined")
                {
                    strSql = strSql + @" AND E.UnitId = '" + sUnitID + "'";
                }
                if (sDivID != "ALL" && sDivID != null && sDivID != "null" && sDivID != "undefined")
                {
                    strSql = strSql + @" AND E.DivisionId = '" + sDivID + "'";
                }
                if (sDepID != "ALL" && sDepID != null && sDepID != "null" && sDivID != "undefined")
                {
                    strSql = strSql + @" AND E.DepartmentId = '" + sDepID + "'";
                }
                if (sSecID != "ALL" && sSecID != null && sSecID != "null" && sDivID != "undefined")
                {
                    strSql = strSql + @" AND E.SectionId = '" + sSecID + "'";
                }
                if (sSubSecID != "ALL" && sSubSecID != null && sSubSecID != "null" && sDivID != "undefined")
                {
                    strSql = strSql + @" AND E.SubSectionId = '" + sSubSecID + "'";
                }
                strSql += @"  GROUP BY BudgetedManPower,Group1,Group2,Group3,Sequence
                             ORDER BY Group1,Group2,Sequence,Group3";
                return _sqlRepository.GetDataTable(strSql);
            }
            catch (Exception)
            {
                throw;
            }
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

        public string ExcelDailyDayStatus3(string PlantId, string PrevWorkDate, string companyId, string TextFromDate, string sDepID, string sSecID, string sSubSecID, string sLineID, string dayStatus, string Dep, string Sec, string employeeCategory, string shift, string entity, string designationList)
        {
            #region Variable

            clsReport objDlySts = null;

            DataSet dslocal = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            ReportUtility oru = null;

            #endregion Variable

            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;
                oru = new ReportUtility();
                objDlySts = new clsReport();
                var ob = new clsStaticInfo();
                //cut
                string ddlDept = sDepID;
                string ddlSec = sSecID;
                string ddlSbSec = sSubSecID;
                string ddll = sLineID;

                GetDailyDayStatusS(TextFromDate, PrevWorkDate, PlantId, ddlDept, ddlSec, ddlSbSec, ddll, dayStatus, employeeCategory, shift, entity, designationList, "", out dslocal);

                objDlySts.SelectedPlantWiseCompany(PlantId, out dsCmp);
                objDlySts.SelectedPlant(PlantId, out dsFactory);

                //subsection 
                DataView dvSubSec = new DataView(dslocal.Tables[0]);
                //dvSubSec.RowFilter = "LineID ='' ";
                DataTable dtSubsecList = dvSubSec.ToTable(true, "SubSectionId", "SubSection");
                //line

                DataView dvLine = new DataView(dslocal.Tables[0]);
                dvLine.RowFilter = "LineID <>'' ";
                //dvLine.Sort = "Sequence";
                DataTable dtLine = dvLine.ToTable(true, "LineID", "Line");

                if (dtSubsecList.Rows.Count + dtLine.Rows.Count == 0)
                {
                    throw new Exception("No Data found...!");
                }

                workbook = application.Workbooks.Create(dtSubsecList.Rows.Count + dtLine.Rows.Count);

                //ss

                //for (int i = 0; i < dtSubsecList.Rows.Count; i++)
                //{
                //var SSid = dtSubsecList.Rows[i]["SubSectionId"].ToString();
                //var _SubSection = dtSubsecList.Rows[i]["SubSection"].ToString();
                DataView dvSSEmp = new DataView(dslocal.Tables[0]);
                //dvSSEmp.RowFilter = "SubSectionId='" + SSid + "' ";
                DataTable dtdvSSEmp = dvSSEmp.ToTable();
                var _SubSection = "Day Status";

                sheet1 = workbook.Worksheets[0];
                CreateSheetS(ref sheet1, dtdvSSEmp, dsCmp, dsFactory, oru, _SubSection, _SubSection, "SUBS", TextFromDate, sDepID, sSecID, sSubSecID, "", Dep, Sec, employeeCategory, shift, entity);
                //}
                //line

                //for (int i = 0; i < dtLine.Rows.Count; i++)
                //{
                //    string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
                //    var _LineID = dtLine.Rows[i]["LineID"].ToString();


                //    string _Line = dtLine.Rows[i]["Line"].ToString();

                //    foreach (var RepChar in specialChar)
                //    {
                //        _Line = _Line.Replace(RepChar.ToString(), " ");
                //    }

                //    DataView dvSSEmp = new DataView(dslocal.Tables[0]);
                //    dvSSEmp.RowFilter = "LineID='" + _LineID + "' ";
                //    DataTable dtdvSSEmp = dvSSEmp.ToTable();

                //    sheet1 = workbook.Worksheets[dtSubsecList.Rows.Count + i];
                //    CreateSheet(ref sheet1, dtdvSSEmp, dsCmp, dsFactory, oru, _Line, _Line, "LINE", TextFromDate, sDepID, sSecID, sSubSecID, sLineID, Dep, Sec, employeeCategory, shift, entity);

                //}
                var filePath = "";
                var SheetName = "";
                //return workbook;
                workbook.Version = ExcelVersion.Excel97to2003;
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objDlySts = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }
        }

        public void GetDailyDayStatusS(string WorkDate, string PrevWorkDate, string sPlantID, string DepartmentId, string SectionId, string SubsectionId, string LineId, string dayStatus, string employeeCategory, string shift, string entity, string designationList, string JobLocation, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string secSQL = string.Empty;
            string xxy = string.Empty;
            string XJobLocation = string.Empty;
            clsStaticInfo obs = null;
            string ShiftIds_WC = "";
            try
            {
                if (shift != "ALL" && shift != "''" && shift != "'ALL'")
                {
                    ShiftIds_WC = " and sd.SystemID in (" + shift + ") ";
                }

                if (dayStatus != null)
                {
                    if (dayStatus.ToUpper() != "ALL" && dayStatus != "null" && dayStatus != "" && dayStatus != "''")
                    {
                        xxy = " and dt.Category in (" + dayStatus + ")";
                    }
                }
                XJobLocation += " And J.SystemID in (" + JobLocation + ")";
                obs = new clsStaticInfo();
                strSql = @" select e.SystemId
                                            from EmployeeInformation e
                                            left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
											left join org.Department dep on dep.Id = p.DepartmentId
											left join org.Section s on s.Id = p.SectionId
											left join org.SubSection ss on ss.Id = p.SubSectionId                                       
                                            LEFT JOIN org.Line L ON L.Id = En.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id 
											left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
											left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
											left join HKP.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId
                                            
											where   e.PlantId='" + sPlantID + @"' and e.DOJ <= ( '" + WorkDate + @"') and (e.DOS is null or e.DOS >= '" + WorkDate + @"')";


                if (DepartmentId != "ALL")
                {
                    strSql = strSql + @" AND dep.Id in ( " + DepartmentId + ")";
                }
                if (SectionId != "ALL")
                {
                    strSql = strSql + @" AND s.Id in (" + SectionId + ")";
                }
                if (SubsectionId != "ALL")
                {
                    strSql = strSql + @" AND ss.Id in (" + SubsectionId + ")";
                }

                if (employeeCategory != "ALL")
                {
                    strSql = strSql + @" AND ec.Id in (" + employeeCategory + ")";
                }

                if (entity != "ALL")
                {
                    strSql = strSql + @" AND en.Id in (" + entity + ")";
                }
                if (LineId != "ALL" && LineId != "''")
                {
                    strSql = strSql + @" AND isnull(L.Id,'') in (" + LineId + ")";
                }
                if (designationList != "ALL" && designationList != "''")
                {
                    strSql = strSql + @" AND LG.Id in (" + designationList + ")";
                }

                secSQL = @"SELECT e.SystemId EmpSystemId,e.EmployeeCode,e.FatherName,dt.Category
								,dep.username Department,CONVERT(VARCHAR(5), AD.InTime, 108)iintime
								,iShiftIn = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(5),SD.InTime , 108)
							 ELSE CONVERT(VARCHAR(5), cs.InTime , 108)
						     END
                                , e.EmployeeName,L.Id LineID,L.UserName Line,SS.Id SubSectionId,SS.UserName SubSection
								,sd.UserName ShiftName,AD.IsOTEntitled IsOTEntitledToday
                                , ShiftIn  = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
						     END							
								,ShiftOut = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                , FORMAT(CAST(ap.InTime AS datetime2), N'hh:mm tt') InTime
								,FORMAT(CAST( ap.OutTime AS datetime2), N'hh:mm tt') OutTime
	                            ,  REPLACE(CONVERT(VARCHAR(11), ap.WorkDate, 113), ' ', '-') PDate
	                            , ap.DayStatus TodayStatus
	                            , ap.OTHr ,AD.ToDayDayCategory
                                ,ap.IsOTEntitled IsOTEntitledYesterday, ISNULL(ap.IsOTComfirm,0) IsTodayOTComfirm, ISNULL(AD.IsOTComfirm,0) IsYesterDayOTComfirm
                                    ,ToDayReConfirm = CASE WHEN AD.IsOTComfirm=0 AND AD.FIOTWorkDate IS NOT NULL THEN 1 ELSE 0  END
                                    ,YesterDayReConfirm= CASE WHEN ap.IsOTComfirm=0 AND AD.FIOTWorkDate IS NOT NULL THEN 1 ELSE 0  END
                        , LG.UserName Designation
                         , kk.PrvDayStatus,kk.YesterDayDayCategory
						,kk.YesterdayOTHr,ap.IsManualInTime,ap.IsManualOutTime,hr.OTConsiderOn

                        from EmployeeInformation e

                        left join AttdnProcessData ap on ap.EmpSystemID = e.SystemId
left join DayType dt on dt.DayType = ap.DayStatus
INNER JOIN (SELECT APD.*, FIOT.NormalOTHr, FIOT.WorkDate FIOTWorkDate,dt.Category ToDayDayCategory,Dt.Category
                                            ,SEQ=case when  LTSystemid in (select  id from leavetype where LeaveType='Maternity') then 1
													 when isnull(MaternityStatus,'')<>''  then 1 else 0 end
											--,DS=(select  code from leavetype where LeaveType='Maternity' and id=LTSystemid)
											,DS=case when LTSystemid in (select  id from leavetype where LeaveType='Maternity') then (select  code from leavetype where LeaveType='Maternity' and id=LTSystemid)
											when isnull(MaternityStatus,'')<>'' then MaternityStatus else null end 
                             from dbo.AttdnProcessData APD
							LEFT JOIN FINALOT FIOT on FIOT.EmpSystemID = APD.EmpSystemID AND FIOT.WorkDate=APD.WorkDate
							LEFT JOIN DayType dt on dt.Daytype=APD.DayStatus
							WHERE APD.WorkDate  = '" + WorkDate + @"' 
							) AD ON AD.EmpSystemID = E.SystemID

                        LEFT JOIN dbo.ShiftDefination SD ON ap.ShiftSystemID = SD.SystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS cs ON ap.WorkDate BETWEEN cs.FromDate AND cs.ToDate AND sd.SystemID=cs.ShiftDefinationID
                                            left join mst.ManpowerBudget mp on mp.id = e.BudgetCode
                                            left join org.Entity en on en.id = mp.EntityId
                                            left join ORG.Position p on p.Id = mp.PositionId
                                            left join org.Department dep on dep.Id = p.DepartmentId
                                            left join org.Section s on s.Id = p.SectionId
                                            LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=e.PlantId
                                            left join org.SubSection ss on ss.Id = p.SubSectionId
                                            LEFT JOIN org.Line L ON L.Id = En.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id
                                            LEFT JOIN JobLocation J ON J.SystemID = e.JobLocationID
                                            left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
                                            left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
                                            left join HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                            left join(select yap.DayStatus PrvDayStatus, yap.OTHr YesterdayOTHr, yap.EmpSystemID,ydt.Category YesterDayDayCategory from AttdnProcessData yap
                                                left join DayType ydt on ydt.DayType = yap.DayStatus
                                                where yap.WorkDate = '" + PrevWorkDate + @"') kk on kk.EmpSystemID = e.SystemId

where  ap.WorkDate='" + WorkDate + @"' and e.SystemId in (" + strSql + ")  " + ShiftIds_WC + " " + xxy + " " + XJobLocation + "";

                objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                objCon.BeginTransaction();
                objCon.getDataSet(secSQL, out dsRef);
                objCon.CommitTransaction();
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

        public IWorkbook ExcelDailyDayStatus(string PlantId, string PrevWorkDate, string companyId, string TextFromDate, string sDepID, string sSecID, string sSubSecID, string sLineID, string dayStatus, string Dep, string Sec, string employeeCategory, string shift, string entity)
        {
            #region Variable

            clsReport objDlySts = null;

            DataSet dslocal = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            ReportUtility oru = null;

            #endregion Variable

            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;
                oru = new ReportUtility();
                objDlySts = new clsReport();
                var ob = new clsStaticInfo();
                //cut
                string ddlDept = sDepID;
                string ddlSec = sSecID;
                string ddlSbSec = sSubSecID;
                string ddll = sLineID;

                objDlySts.GetDailyDayStatus(TextFromDate, PrevWorkDate, PlantId, ddlDept, ddlSec, ddlSbSec, ddll, dayStatus, employeeCategory, shift, entity, out dslocal);

                objDlySts.SelectedPlantWiseCompany(PlantId, out dsCmp);
                objDlySts.SelectedPlant(PlantId, out dsFactory);

                //subsection 
                DataView dvSubSec = new DataView(dslocal.Tables[0]);
                dvSubSec.RowFilter = "LineID ='' ";
                DataTable dtSubsecList = dvSubSec.ToTable(true, "SubSectionId", "SubSection");
                //line

                DataView dvLine = new DataView(dslocal.Tables[0]);
                dvLine.RowFilter = "LineID <>'' ";
                dvLine.Sort = "Sequence";
                DataTable dtLine = dvLine.ToTable(true, "LineID", "Line");

                if (dtSubsecList.Rows.Count + dtLine.Rows.Count == 0)
                {
                    throw new Exception("No Data found...!");
                }

                workbook = application.Workbooks.Create(dtSubsecList.Rows.Count + dtLine.Rows.Count);

                //ss

                for (int i = 0; i < dtSubsecList.Rows.Count; i++)
                {
                    var SSid = dtSubsecList.Rows[i]["SubSectionId"].ToString();
                    var _SubSection = dtSubsecList.Rows[i]["SubSection"].ToString();
                    DataView dvSSEmp = new DataView(dslocal.Tables[0]);
                    dvSSEmp.RowFilter = "SubSectionId='" + SSid + "' and LineId = '' ";
                    DataTable dtdvSSEmp = dvSSEmp.ToTable();

                    sheet1 = workbook.Worksheets[i];
                    CreateSheet(ref sheet1, dtdvSSEmp, dsCmp, dsFactory, oru, _SubSection, _SubSection, "SUBS", TextFromDate, sDepID, sSecID, sSubSecID, sLineID, Dep, Sec, employeeCategory, shift, entity);
                }
                //line

                for (int i = 0; i < dtLine.Rows.Count; i++)
                {
                    string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
                    var _LineID = dtLine.Rows[i]["LineID"].ToString();


                    string _Line = dtLine.Rows[i]["Line"].ToString();

                    foreach (var RepChar in specialChar)
                    {
                        _Line = _Line.Replace(RepChar.ToString(), " ");
                    }

                    DataView dvSSEmp = new DataView(dslocal.Tables[0]);
                    dvSSEmp.RowFilter = "LineID='" + _LineID + "' ";
                    DataTable dtdvSSEmp = dvSSEmp.ToTable();

                    sheet1 = workbook.Worksheets[dtSubsecList.Rows.Count + i];
                    CreateSheet(ref sheet1, dtdvSSEmp, dsCmp, dsFactory, oru, _Line, _Line, "LINE", TextFromDate, sDepID, sSecID, sSubSecID, sLineID, Dep, Sec, employeeCategory, shift, entity);

                }
                var filePath = "";
                var SheetName = "";
                return workbook;
                //workbook.Version = ExcelVersion.Excel97to2003;
                //filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                //workbook.SaveAs(filePath);
                //workbook.Close();
                //excelEngine.Dispose();
                //return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objDlySts = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }

        }

        public IWorkbook ExcelDailyDayStatusReport(string PlantId, string PrevWorkDate, string companyId, string TextFromDate)
        {
            #region Variable

            clsReport objDlySts = null;

            DataSet dslocal = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            ReportUtility oru = null;

            #endregion Variable

            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2013;
                oru = new ReportUtility();
                objDlySts = new clsReport();
                var ob = new clsStaticInfo();

                //string ddlDept = sDepID;
                //string ddlSec = sSecID;
                //string ddlSbSec = sSubSecID;
                //string ddll = sLineID;

                objDlySts.GetDailyDayStatusReport(TextFromDate, PrevWorkDate, PlantId, /*ddlDept, ddlSec, ddlSbSec, ddll, dayStatus, employeeCategory, shift, entity,*/ out dslocal);

                objDlySts.SelectedPlantWiseCompany(PlantId, out dsCmp);
                objDlySts.SelectedPlant(PlantId, out dsFactory);

                //subsection 
                DataView dvSubSec = new DataView(dslocal.Tables[0]);
                dvSubSec.RowFilter = "LineID ='' ";
                DataTable dtSubsecList = dvSubSec.ToTable(true, "SubSectionId", "SubSection");
                //line

                DataView dvLine = new DataView(dslocal.Tables[0]);
                dvLine.RowFilter = "LineID <>'' ";
                dvLine.Sort = "Sequence";
                DataTable dtLine = dvLine.ToTable(true, "LineID", "Line");

                if (dtSubsecList.Rows.Count + dtLine.Rows.Count == 0)
                {
                    throw new Exception("No Data found...!");
                }

                workbook = application.Workbooks.Create(dtSubsecList.Rows.Count + dtLine.Rows.Count);

                //ss

                for (int i = 0; i < dtSubsecList.Rows.Count; i++)
                {
                    var SSid = dtSubsecList.Rows[i]["SubSectionId"].ToString();
                    var _SubSection = dtSubsecList.Rows[i]["SubSection"].ToString();
                    DataView dvSSEmp = new DataView(dslocal.Tables[0]);
                    dvSSEmp.RowFilter = "SubSectionId='" + SSid + "' and LineId = '' ";
                    DataTable dtdvSSEmp = dvSSEmp.ToTable();

                    sheet1 = workbook.Worksheets[i];
                    CreateSheetD(ref sheet1, dtdvSSEmp, dsCmp, dsFactory, oru, _SubSection, _SubSection, "SUBS", TextFromDate/*, sDepID, sSecID, sSubSecID, sLineID, Dep, Sec, employeeCategory, shift, entity*/);
                }
                //line

                for (int i = 0; i < dtLine.Rows.Count; i++)
                {
                    string specialChar = @"\|!#$%&/()=?»«@£§€{}.-;'<>_,";
                    var _LineID = dtLine.Rows[i]["LineID"].ToString();


                    string _Line = dtLine.Rows[i]["Line"].ToString();

                    foreach (var RepChar in specialChar)
                    {
                        _Line = _Line.Replace(RepChar.ToString(), " ");
                    }

                    DataView dvSSEmp = new DataView(dslocal.Tables[0]);
                    dvSSEmp.RowFilter = "LineID='" + _LineID + "' ";
                    DataTable dtdvSSEmp = dvSSEmp.ToTable();

                    sheet1 = workbook.Worksheets[dtSubsecList.Rows.Count + i];
                    CreateSheetD(ref sheet1, dtdvSSEmp, dsCmp, dsFactory, oru, _Line, _Line, "LINE", TextFromDate/*, sDepID, sSecID, sSubSecID, sLineID, Dep, Sec, employeeCategory, shift, entity*/);

                }
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objDlySts = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }
        }

        void CreateSheetS(ref IWorksheet sheet1, DataTable dtlocal, DataSet dsCmp, DataSet dsFactory, ReportUtility oru, string SheetName, string SheetHeader, string FLAG, string TextFromDate, string sDepID, string sSecID, string sSubSecID, string sLineID, string Dep, string Sec, string employeecategory, string shift, string entity)
        {
            int xlsRow = 0;
            int xlsCol = 0;
            int endXlsCol = 0;
            string FactoryAddress = string.Empty;
            string sOfficeInTime = string.Empty;
            string sInTime = string.Empty;
            string strLateBy = string.Empty;


            try
            {
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");
                sheet1.Name = SheetName;
                sheet1.IsGridLinesVisible = true;
                xlsRow = 6;
                xlsCol = 2;

                string FactoryName = "";
                string CmpName = "";
                //string FactoryName = "";

                #region ----Depertment, Section, SubSection, Line-----

                //oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Department", 18);
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Size = 15;
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].RowHeight = 20;

                //var dpt = string.Empty;
                //if (sDepID == "null")
                //{
                //    dpt = "ALL";
                //}
                //else
                //{
                //    dpt = Dep;
                //}
                //oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 1, dpt, 25);

                //xlsRow++;

                //oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Section", 10);
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Size = 15;
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].RowHeight = 20;
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //var sec = string.Empty;
                //if (sSecID == "null")
                //{
                //    sec = "ALL";
                //}
                //else
                //{
                //    sec = Sec;
                //}
                //oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 1, sec, 25);

                //xlsRow = 6;

                //oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 5, "SubSection", 10);
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 11].CellStyle.Font.Size = 15;
                //var ssec = string.Empty;
                //if (sSubSecID == "null")
                //{
                //    ssec = "ALL";
                //}
                //else
                //{
                //    ssec = dtlocal.Rows[0]["SubSection"].ToString();
                //}
                //oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 6, ssec, 25);

                //xlsRow++;

                //oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 5, "Line", 5);
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 11].CellStyle.Font.Size = 15;
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 11].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                //var sline = string.Empty;

                ////sline = SheetHeader;

                //oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 6, sline, 25);
                //sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 11].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                #region Line Loop
                //char[] spChar = { '*', '/', '|', '-', '_', '\'' };

                //int HeaderRow = 3;
                //if (FLAG == "SUBS")
                //{
                //    HeaderRow = 8;
                //}
                //else
                //{
                //    HeaderRow = 9;
                //}

                //xlsRow += 1;
                #endregion line loop

                #endregion ------Depertment, Section, SubSection, Line-----

                #region -------Header Column--------

                int cSrl = 0, cEmpDeg = 0, cEmpCode = 0, cEmpName = 0, cDptName = 0, cEmpPre = 0, cEmpL = 0, cEmpA = 0, cEmpLv = 0, cEmpMLv = 0, cEmpOD = 0, cEmpOtr = 0, cEmpYstdyDay = 0,
                    cEmpYstdysts = 0, cEmpYstdyOt = 0, cLateBy = 0, cEmpTodyOt = 0, cInchargeSignature = 0, cEmployeeSignature = 0, cEmpMonth = 0, cEmpWeek = 0,
                    cEmpCategory = 0, cEmpFatherName = 0, cInTime = 0, cOutTime = 0, cshift = 0, cEntity = 0, cShiftIntime = 0, cShiftOuttime = 0;

                xlsCol = 1;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Sr. No", 5);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cSrl = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Emp Code", 19);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpCode = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Employee Name", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpName = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Father Name", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpFatherName = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Department", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cDptName = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Designation", 21);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpDeg = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Shift", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cshift = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Shift InTime", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cShiftIntime = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Shift OutTime", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cShiftOuttime = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Punch In Time", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cInTime = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Punch Out Time", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cOutTime = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Status", 20);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpPre = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Late By", 18);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cLateBy = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Yestarday", 22);
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                oru.SetHeaderTextWB(ref sheet1, xlsRow + 1, xlsCol, "Status", 11);
                cEmpYstdysts = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow + 1, xlsCol, "OT", 6);
                cEmpYstdyOt = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Employee Signature", 30);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmployeeSignature = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Incharge Signature", 30);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cInchargeSignature = xlsCol;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Size = 15f;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;

                #endregion Header Column
                DateTime firstdate = Convert.ToDateTime(TextFromDate);


                var firstDateOfMonth = new DateTime(firstdate.Year, firstdate.Month, 1);
                var lastDateOfMonth = new DateTime(firstdate.Year, firstdate.Month, 1).AddMonths(1).AddDays(-1);



                xlsCol = 1;
                xlsRow += 2;
                int strCount = 0;
                int startDataRow = xlsRow;
                #region ------Data SET------
                for (int i = 0; i < dtlocal.Rows.Count; i++)
                {
                    DataTable dtMonthlyOt = null;
                    dtMonthlyOt = GetMonthlyOT(dtlocal.Rows[i]["EmpSystemId"].ToString(), firstDateOfMonth.ToString("dd-MMM-yyyy"), TextFromDate);
                    var monthLytotalOthr = 0.00;
                    oru.SetText(ref sheet1, xlsRow, cSrl, strCount);

                    DataTable dtWeekoffDate = null;
                    DataTable dtWeeklyOT = null;
                    dtWeekoffDate = GetWeekOffDate(dtlocal.Rows[i]["EmpSystemId"].ToString(), firstDateOfMonth.ToString("dd-MMM-yyyy"), TextFromDate);

                    if (dtWeekoffDate.Rows.Count > 0)
                    {

                        dtWeeklyOT = GetMonthlyOT(dtlocal.Rows[i]["EmpSystemId"].ToString(), Convert.ToDateTime(dtWeekoffDate.Rows[0]["WeekOffDate"]).AddDays(1).ToString("dd-MMM-yyyy"), TextFromDate);

                    }
                    var WeekLytotalOthr = 0.00;
                    strCount++;
                    oru.SetText(ref sheet1, xlsRow, cSrl, strCount);
                    oru.SetText(ref sheet1, xlsRow, cEmpDeg, dtlocal.Rows[i]["Designation"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cEmpCode, dtlocal.Rows[i]["EmployeeCode"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cEmpName, dtlocal.Rows[i]["EmployeeName"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cEmpFatherName, dtlocal.Rows[i]["FatherName"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cDptName, dtlocal.Rows[i]["Department"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cShiftIntime, dtlocal.Rows[i]["ShiftIn"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cShiftOuttime, dtlocal.Rows[i]["ShiftOut"].ToString());

                    //oru.SetText(ref sheet1, xlsRow, cEmpCategory, dtlocal.Rows[i]["EmployeeCategory"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cInTime, dtlocal.Rows[i]["InTime"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cOutTime, dtlocal.Rows[i]["OutTime"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cshift, dtlocal.Rows[i]["ShiftName"].ToString());

                    //oru.SetText(ref sheet1, xlsRow, cEntity, dtlocal.Rows[i]["Entity"].ToString());

                    if (dtlocal.Rows[i]["TodayStatus"].ToString() == "L")
                    {
                        #region Late by min

                        sOfficeInTime = "00:00:00";
                        sInTime = "00:00:00";
                        strLateBy = "00:00:00";

                        if (dtlocal.Rows[i]["iintime"].ToString().Trim() != "")
                        {
                            sInTime = dtlocal.Rows[i]["iintime"].ToString().Trim() + ":00";
                        }
                        strLateBy = "00:00";
                        if (dtlocal.Rows[i]["iShiftIn"].ToString().Trim() != "" && sInTime != "00:00:00")
                        {
                            sOfficeInTime = dtlocal.Rows[i]["iShiftIn"].ToString().Trim() + ":00";
                            strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString();
                        }

                        oru.SetText(ref sheet1, xlsRow, cLateBy, strLateBy);

                        #endregion Late by min
                    }

                    oru.SetText(ref sheet1, xlsRow, cEmpPre, dtlocal.Rows[i]["TodayStatus"].ToString());

                    oru.SetText(ref sheet1, xlsRow, cEmpYstdysts, dtlocal.Rows[i]["PrvDayStatus"].ToString());

                    string yot = string.Empty;//OTConsiderOn
                    if (bplib.clsWebLib.GetBoolData(dtlocal.Rows[i]["IsOTEntitledYesterday"].ToString()) == true)
                    {
                        if (string.IsNullOrEmpty(dtlocal.Rows[i]["YesterdayOTHr"].ToString()))
                        {

                            if (!string.IsNullOrEmpty(dtlocal.Rows[i]["YesterDayDayCategory"].ToString()))
                            {
                                if (dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Present" || dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Late")
                                {
                                    sheet1.Range[xlsRow, cEmpYstdyOt].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                                }
                            }

                        }
                        else
                        {
                            if (bplib.clsWebLib.GetBoolData(dtlocal.Rows[i]["YesterDayReConfirm"].ToString()) == true)
                            {
                                sheet1.Range[xlsRow, cEmpYstdyOt].CellStyle.Interior.ColorIndex = ExcelKnownColors.Orange;
                            }
                        }
                        if (!string.IsNullOrEmpty(dtlocal.Rows[i]["YesterDayDayCategory"].ToString()))
                        {
                            if (dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Present" || dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Late")
                            {
                                oru.GetOT(dtlocal.Rows[i]["OTConsiderOn"].ToString(), dtlocal.Rows[i]["YesterdayOTHr"].ToString(), out yot);
                            }
                        }

                    }
                    sheet1.Range[xlsRow, cEmpYstdyOt].Text = yot;

                    sheet1.Range[xlsRow, cEmpYstdyOt].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow, cEmpYstdyOt].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, cEmpYstdyOt].VerticalAlignment = ExcelVAlign.VAlignTop;
                }

                sheet1.Range[startDataRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startDataRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startDataRow, 1, xlsRow, endXlsCol].CellStyle.Font.Size = 15f;

                sheet1.Range[8, 1, xlsRow, cInchargeSignature].RowHeight = 65;

                //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].RowHeight = 30;

                #endregion ------Data Set------

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.Range["A1"].CellStyle.Font.Size = 18;
                sheet1.Range["A2"].CellStyle.Font.Size = 18;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                //sheet1.UsedRange.BorderAround(ExcelLineStyle.Thin);

                #endregion UsedRange Alignment

                #region ******************Company Name Header******************

                xlsRow = 1;
                xlsCol = 1;

                FactoryName = string.Empty;

                //string FactoryAddress = string.Empty;

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
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 16;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Company Name Header******************

                #region ******************Report Header******************

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Day Status Report" + " (" + SheetHeader + ")";
                sheet1.Range[xlsRow, cSrl, xlsRow, cEmpName].Merge();
                sheet1.Range[xlsRow, cSrl, xlsRow, cEmpName].CellStyle.Font.Size = 15;
                sheet1.Range[xlsRow, 15].Text = "Date:- " + TextFromDate;
                sheet1.Range[xlsRow, 4, xlsRow, 11].Merge();
                sheet1.Range[xlsRow, 15].CellStyle.Font.Size = 15;
                //sheet1.Range[xlsRow, 16].Text = "Previous Date:- " + PrevWorkDate;
                //sheet1.Range[xlsRow, 4, xlsRow, 11].Merge();
                //sheet1.Range[xlsRow, 16].CellStyle.Font.Size = 15;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 13].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                sheet1.Range[1, 1, 6, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[1, 1, 6, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                #endregion ******************Report Header******************

                #region Freeze Panes
                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A10"].FreezePanes();

                #endregion Freeze Panes

                #region Page Setup
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$9";
                sheet1.PageSetup.RightFooter = "&\"Arial Narrow\"&10" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Arial Narrow\"&10" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;



                #endregion Page Setup
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void CreateSheet(ref IWorksheet sheet1, DataTable dtlocal, DataSet dsCmp, DataSet dsFactory, ReportUtility oru, string SheetName, string SheetHeader, string FLAG, string TextFromDate, string sDepID, string sSecID, string sSubSecID, string sLineID, string Dep, string Sec, string employeecategory, string shift, string entity)
        {
            int xlsRow = 0;
            int xlsCol = 0;
            int endXlsCol = 0;
            string FactoryAddress = string.Empty;
            string sOfficeInTime = string.Empty;
            string sInTime = string.Empty;
            string strLateBy = string.Empty;


            try
            {
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");
                sheet1.Name = SheetName;
                sheet1.IsGridLinesVisible = true;
                xlsRow = 6;
                xlsCol = 2;

                string FactoryName = "";
                string CmpName = "";
                //string FactoryName = "";

                #region ----Depertment, Section, SubSection, Line-----


                #endregion ------Depertment, Section, SubSection, Line-----

                #region -------Header Column--------

                int cSrl = 0, cEmpDeg = 0, cEmpCode = 0, cEmpName = 0, cDptName = 0, cEmpPre = 0, cEmpL = 0, cEmpA = 0, cEmpLv = 0, cEmpMLv = 0, cEmpOD = 0, cEmpOtr = 0, cEmpYstdyDay = 0,
                    cEmpYstdysts = 0, cEmpYstdyOt = 0, cLateBy = 0, cEmpTodyOt = 0, cInchargeSignature = 0, cEmployeeSignature = 0, cEmpMonth = 0, cEmpWeek = 0,
                    cEmpCategory = 0, cEmpFatherName = 0, cInTime = 0, cOutTime = 0, cshift = 0, cEntity = 0, cShiftIntime = 0, cShiftOuttime = 0;

                xlsCol = 1;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Sr. No", 5);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cSrl = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Emp Code", 19);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpCode = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Employee Name", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpName = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Father Name", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpFatherName = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Department", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cDptName = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Designation", 21);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpDeg = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Shift", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cshift = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Shift InTime", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cShiftIntime = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Shift OutTime", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cShiftOuttime = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Punch In Time", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cInTime = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Punch Out Time", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cOutTime = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Status", 20);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpPre = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Late By", 18);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cLateBy = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Yestarday", 22);
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                oru.SetHeaderTextWB(ref sheet1, xlsRow + 1, xlsCol, "Status", 11);
                cEmpYstdysts = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow + 1, xlsCol, "OT", 6);
                cEmpYstdyOt = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Employee Signature", 30);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmployeeSignature = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Incharge Signature", 30);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cInchargeSignature = xlsCol;



                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Size = 15f;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;

                #endregion Header Column
                DateTime firstdate = Convert.ToDateTime(TextFromDate);


                var firstDateOfMonth = new DateTime(firstdate.Year, firstdate.Month, 1);
                var lastDateOfMonth = new DateTime(firstdate.Year, firstdate.Month, 1).AddMonths(1).AddDays(-1);



                xlsCol = 1;
                xlsRow += 2;
                int strCount = 0;
                int startDataRow = xlsRow;
                #region ------Data SET------
                for (int i = 0; i < dtlocal.Rows.Count; i++)
                {
                    DataTable dtMonthlyOt = null;
                    dtMonthlyOt = GetMonthlyOT(dtlocal.Rows[i]["EmpSystemId"].ToString(), firstDateOfMonth.ToString("dd-MMM-yyyy"), TextFromDate);
                    var monthLytotalOthr = 0.00;
                    oru.SetText(ref sheet1, xlsRow, cSrl, strCount);

                    DataTable dtWeekoffDate = null;
                    DataTable dtWeeklyOT = null;
                    dtWeekoffDate = GetWeekOffDate(dtlocal.Rows[i]["EmpSystemId"].ToString(), firstDateOfMonth.ToString("dd-MMM-yyyy"), TextFromDate);

                    if (dtWeekoffDate.Rows.Count > 0)
                    {

                        dtWeeklyOT = GetMonthlyOT(dtlocal.Rows[i]["EmpSystemId"].ToString(), Convert.ToDateTime(dtWeekoffDate.Rows[0]["WeekOffDate"]).AddDays(1).ToString("dd-MMM-yyyy"), TextFromDate);

                    }
                    var WeekLytotalOthr = 0.00;
                    strCount++;
                    oru.SetText(ref sheet1, xlsRow, cSrl, strCount);
                    oru.SetText(ref sheet1, xlsRow, cEmpDeg, dtlocal.Rows[i]["Designation"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cEmpCode, dtlocal.Rows[i]["EmployeeCode"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cEmpName, dtlocal.Rows[i]["EmployeeName"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cEmpFatherName, dtlocal.Rows[i]["FatherName"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cDptName, dtlocal.Rows[i]["Department"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cShiftIntime, dtlocal.Rows[i]["ShiftIn"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cShiftOuttime, dtlocal.Rows[i]["ShiftOut"].ToString());

                    //oru.SetText(ref sheet1, xlsRow, cEmpCategory, dtlocal.Rows[i]["EmployeeCategory"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cInTime, dtlocal.Rows[i]["InTime"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cOutTime, dtlocal.Rows[i]["OutTime"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cshift, dtlocal.Rows[i]["ShiftName"].ToString());

                    //oru.SetText(ref sheet1, xlsRow, cEntity, dtlocal.Rows[i]["Entity"].ToString());

                    if (dtlocal.Rows[i]["TodayStatus"].ToString() == "L")
                    {
                        #region Late by min

                        sOfficeInTime = "00:00:00";
                        sInTime = "00:00:00";
                        strLateBy = "00:00:00";

                        if (dtlocal.Rows[i]["iintime"].ToString().Trim() != "")
                        {
                            sInTime = dtlocal.Rows[i]["iintime"].ToString().Trim() + ":00";
                        }
                        strLateBy = "00:00";
                        if (dtlocal.Rows[i]["iShiftIn"].ToString().Trim() != "" && sInTime != "00:00:00")
                        {
                            sOfficeInTime = dtlocal.Rows[i]["iShiftIn"].ToString().Trim() + ":00";
                            strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString();
                        }

                        oru.SetText(ref sheet1, xlsRow, cLateBy, strLateBy);

                        #endregion Late by min
                    }

                    oru.SetText(ref sheet1, xlsRow, cEmpPre, dtlocal.Rows[i]["TodayStatus"].ToString());

                    oru.SetText(ref sheet1, xlsRow, cEmpYstdysts, dtlocal.Rows[i]["PrvDayStatus"].ToString());

                    string yot = string.Empty;//OTConsiderOn
                    if (bplib.clsWebLib.GetBoolData(dtlocal.Rows[i]["IsOTEntitledYesterday"].ToString()) == true)
                    {
                        if (string.IsNullOrEmpty(dtlocal.Rows[i]["YesterdayOTHr"].ToString()))
                        {

                            if (!string.IsNullOrEmpty(dtlocal.Rows[i]["YesterDayDayCategory"].ToString()))
                            {
                                if (dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Present" || dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Late")
                                {
                                    sheet1.Range[xlsRow, cEmpYstdyOt].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                                }
                            }

                        }
                        else
                        {
                            if (bplib.clsWebLib.GetBoolData(dtlocal.Rows[i]["YesterDayReConfirm"].ToString()) == true)
                            {
                                sheet1.Range[xlsRow, cEmpYstdyOt].CellStyle.Interior.ColorIndex = ExcelKnownColors.Orange;
                            }
                        }
                        if (!string.IsNullOrEmpty(dtlocal.Rows[i]["YesterDayDayCategory"].ToString()))
                        {
                            if (dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Present" || dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Late")
                            {
                                oru.GetOT(dtlocal.Rows[i]["OTConsiderOn"].ToString(), dtlocal.Rows[i]["YesterdayOTHr"].ToString(), out yot);
                            }
                        }

                    }


                    sheet1.Range[xlsRow, cEmpYstdyOt].Text = yot;

                    sheet1.Range[xlsRow, cEmpYstdyOt].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow, cEmpYstdyOt].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, cEmpYstdyOt].VerticalAlignment = ExcelVAlign.VAlignTop;
                    xlsRow++;
                }

                sheet1.Range[startDataRow, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startDataRow, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startDataRow, 1, xlsRow - 1, endXlsCol].CellStyle.Font.Size = 15f;

                sheet1.Range[startDataRow, 1, xlsRow, cInchargeSignature].RowHeight = 65;

                //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].RowHeight = 30;

                #endregion ------Data Set------

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.Range["A1"].CellStyle.Font.Size = 18;
                sheet1.Range["A2"].CellStyle.Font.Size = 18;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                //sheet1.UsedRange.BorderAround(ExcelLineStyle.Thin);

                #endregion UsedRange Alignment

                #region ******************Company Name Header******************

                xlsRow = 1;
                xlsCol = 1;

                FactoryName = string.Empty;

                //string FactoryAddress = string.Empty;

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
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 16;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Company Name Header******************

                #region ******************Report Header******************

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Day Status Report" + " (" + SheetHeader + ")";
                sheet1.Range[xlsRow, cSrl, xlsRow, cEmpName].Merge();
                sheet1.Range[xlsRow, cSrl, xlsRow, cEmpName].CellStyle.Font.Size = 15;
                sheet1.Range[xlsRow, 15].Text = "Date:- " + TextFromDate;
                sheet1.Range[xlsRow, 4, xlsRow, 11].Merge();
                sheet1.Range[xlsRow, 15].CellStyle.Font.Size = 15;
                //sheet1.Range[xlsRow, 16].Text = "Previous Date:- " + PrevWorkDate;
                //sheet1.Range[xlsRow, 4, xlsRow, 11].Merge();
                //sheet1.Range[xlsRow, 16].CellStyle.Font.Size = 15;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 13].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                sheet1.Range[1, 1, 6, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[1, 1, 6, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                #endregion ******************Report Header******************

                #region Freeze Panes
                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A10"].FreezePanes();

                #endregion Freeze Panes

                #region Page Setup
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$9";
                sheet1.PageSetup.RightFooter = "&\"Arial Narrow\"&10" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Arial Narrow\"&10" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;



                #endregion Page Setup
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void CreateSheetD(ref IWorksheet sheet1, DataTable dtlocal, DataSet dsCmp, DataSet dsFactory, ReportUtility oru, string SheetName, string SheetHeader, string FLAG, string TextFromDate)
        {
            int xlsRow = 0;
            int xlsCol = 0;
            int endXlsCol = 0;
            string FactoryAddress = string.Empty;


            try
            {
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");
                sheet1.Name = SheetName;
                sheet1.IsGridLinesVisible = true;
                xlsRow = 6;
                xlsCol = 2;

                string FactoryName = "";
                string CmpName = "";
                //string FactoryName = "";

                #region ----Depertment, Section, SubSection, Line-----

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Department", 18);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Size = 15;

                var dpt = string.Empty;
                //if (sDepID == "null")
                //{
                //    dpt = "ALL";
                //}
                //else
                //{
                //    dpt = Dep;
                //}
                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 1, dpt, 25);

                xlsRow++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Section", 10);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Size = 15;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                var sec = string.Empty;
                //if (sSecID == "null")
                //{
                //    sec = "ALL";
                //}
                //else
                //{
                //    sec = Sec;
                //}
                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 1, sec, 25);

                xlsRow = 6;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 5, "SubSection", 10);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 11].CellStyle.Font.Size = 15;
                var ssec = string.Empty;
                //if (sSubSecID == "null")
                //{
                //    ssec = "ALL";
                //}
                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 6, ssec, 25);

                xlsRow++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 5, "Line", 5);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 11].CellStyle.Font.Size = 15;
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 11].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                var sline = string.Empty;

                sline = SheetHeader;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol + 6, sline, 25);
                sheet1.Range[xlsRow, 1, xlsRow, xlsCol + 11].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                #region Line Loop
                char[] spChar = { '*', '/', '|', '-', '_', '\'' };

                int HeaderRow = 3;
                if (FLAG == "SUBS")
                {
                    HeaderRow = 8;
                }
                else
                {
                    HeaderRow = 9;
                }

                xlsRow += 1;
                #endregion line loop

                #endregion ------Depertment, Section, SubSection, Line-----

                #region -------Header Column--------

                int cSrl = 0, cEmpDeg = 0, cEmpCode = 0, cEmpName = 0, cEmpPre = 0, cEmpL = 0, cEmpA = 0, cEmpLv = 0, cEmpMLv = 0, cEmpOD = 0, cEmpOtr = 0, cEmpYstdyDay = 0,
                    cEmpYstdysts = 0, cEmpYstdyOt = 0, cEmpTodyOt = 0, cInchargeSignature = 0, cEmployeeSignature = 0, cEmpMonth = 0, cEmpWeek = 0,
                    cEmpCategory = 0, cEmpFatherName = 0, cInTime = 0, cOutTime = 0, cshift = 0, cEntity = 0;

                xlsCol = 1;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Sr. No", 5);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cSrl = xlsCol; xlsCol++;


                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Emp Code", 19);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpCode = xlsCol; xlsCol++;


                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Employee Name", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpName = xlsCol; xlsCol++;


                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Employee Category", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpCategory = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Father Name", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpFatherName = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Shift", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cshift = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "In Time", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cInTime = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Out Time", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cOutTime = xlsCol; xlsCol++;



                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Entity", 25);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEntity = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Designation", 21);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpDeg = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "P", 3);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpPre = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "L", 3);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpL = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "A", 3);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpA = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Lv", 6);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpLv = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "MLv", 7);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpMLv = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "OD", 5);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpOD = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Other", 9);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpOtr = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Curr Month OT", 18);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpMonth = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Curr Week OT", 20);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpWeek = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Yestarday", 22);
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Merge();
                sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                oru.SetHeaderTextWB(ref sheet1, xlsRow + 1, xlsCol, "Status", 11);
                cEmpYstdysts = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow + 1, xlsCol, "OT", 6);
                cEmpYstdyOt = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Today's OT", 13);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmpTodyOt = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Employee Signature", 30);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cEmployeeSignature = xlsCol; xlsCol++;

                oru.SetHeaderTextWB(ref sheet1, xlsRow, xlsCol, "Incharge Signature", 30);
                sheet1.Range[xlsRow, xlsCol, xlsRow + 1, xlsCol].Merge();
                cInchargeSignature = xlsCol;



                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Size = 15f;
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;

                #endregion Header Column
                DateTime firstdate = Convert.ToDateTime(TextFromDate);


                var firstDateOfMonth = new DateTime(firstdate.Year, firstdate.Month, 1);
                var lastDateOfMonth = new DateTime(firstdate.Year, firstdate.Month, 1).AddMonths(1).AddDays(-1);



                xlsCol = 1;
                xlsRow += 2;
                int strCount = 0;
                int startDataRow = xlsRow;
                #region ------Data SET------
                for (int i = 0; i < dtlocal.Rows.Count; i++)
                {
                    DataTable dtMonthlyOt = null;
                    dtMonthlyOt = GetMonthlyOT(dtlocal.Rows[i]["EmpSystemId"].ToString(), firstDateOfMonth.ToString("dd-MMM-yyyy"), TextFromDate);
                    var monthLytotalOthr = 0.00;
                    oru.SetText(ref sheet1, xlsRow, cSrl, strCount);

                    DataTable dtWeekoffDate = null;
                    DataTable dtWeeklyOT = null;
                    dtWeekoffDate = GetWeekOffDate(dtlocal.Rows[i]["EmpSystemId"].ToString(), firstDateOfMonth.ToString("dd-MMM-yyyy"), TextFromDate);

                    if (dtWeekoffDate.Rows.Count > 0)
                    {

                        dtWeeklyOT = GetMonthlyOT(dtlocal.Rows[i]["EmpSystemId"].ToString(), Convert.ToDateTime(dtWeekoffDate.Rows[0]["WeekOffDate"]).AddDays(1).ToString("dd-MMM-yyyy"), TextFromDate);

                    }
                    var WeekLytotalOthr = 0.00;
                    strCount++;
                    oru.SetText(ref sheet1, xlsRow, cSrl, strCount);
                    oru.SetText(ref sheet1, xlsRow, cEmpDeg, dtlocal.Rows[i]["Designation"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cEmpCode, dtlocal.Rows[i]["EmployeeCode"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cEmpName, dtlocal.Rows[i]["EmployeeName"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cEmpFatherName, dtlocal.Rows[i]["FatherName"].ToString());

                    oru.SetText(ref sheet1, xlsRow, cEmpCategory, dtlocal.Rows[i]["EmployeeCategory"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cInTime, dtlocal.Rows[i]["intime"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cOutTime, dtlocal.Rows[i]["outtime"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cshift, dtlocal.Rows[i]["ShiftName"].ToString());
                    oru.SetText(ref sheet1, xlsRow, cEntity, dtlocal.Rows[i]["Entity"].ToString());



                    if (dtMonthlyOt.Rows.Count > 0)
                    {
                        monthLytotalOthr = Convert.ToDouble(dtMonthlyOt.Rows[0]["TotalOtHrMonth"]);
                    }
                    oru.SetText(ref sheet1, xlsRow, cEmpMonth, monthLytotalOthr);

                    if (dtWeeklyOT.Rows.Count > 0)
                    {
                        WeekLytotalOthr = Convert.ToDouble(dtWeeklyOT.Rows[0]["TotalOtHrMonth"]);
                    }
                    oru.SetText(ref sheet1, xlsRow, cEmpWeek, WeekLytotalOthr);


                    if (dtlocal.Rows[i]["TodayStatus"].ToString() == "P")

                    {
                        oru.SetText(ref sheet1, xlsRow, cEmpPre, 1);

                    }
                    else if (dtlocal.Rows[i]["TodayStatus"].ToString() == "L")
                    {
                        oru.SetText(ref sheet1, xlsRow, cEmpL, 1);
                    }
                    else if (dtlocal.Rows[i]["TodayStatus"].ToString() == "A")
                    {
                        oru.SetText(ref sheet1, xlsRow, cEmpA, 1);

                    }
                    else if (dtlocal.Rows[i]["TodayStatus"].ToString().ToUpper() == "LV")
                    {
                        oru.SetText(ref sheet1, xlsRow, cEmpLv, 1);

                    }
                    else if (dtlocal.Rows[i]["TodayStatus"].ToString().ToUpper() == "MLV")
                    {
                        oru.SetText(ref sheet1, xlsRow, cEmpMLv, 1);

                    }
                    else if (dtlocal.Rows[i]["TodayStatus"].ToString().ToUpper() == "OD")
                    {
                        oru.SetText(ref sheet1, xlsRow, cEmpOD, 1);

                    }
                    else
                    {
                        oru.SetText(ref sheet1, xlsRow, cEmpOtr, dtlocal.Rows[i]["TodayStatus"].ToString());


                    }

                    oru.SetText(ref sheet1, xlsRow, cEmpYstdysts, dtlocal.Rows[i]["PrvDayStatus"].ToString());

                    string yot = string.Empty;//OTConsiderOn
                    if (bplib.clsWebLib.GetBoolData(dtlocal.Rows[i]["IsOTEntitledYesterday"].ToString()) == true)
                    {
                        if (string.IsNullOrEmpty(dtlocal.Rows[i]["YesterdayOTHr"].ToString()))
                        {

                            if (!string.IsNullOrEmpty(dtlocal.Rows[i]["YesterDayDayCategory"].ToString()))
                            {
                                if (dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Present" || dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Late")
                                {
                                    sheet1.Range[xlsRow, cEmpYstdyOt].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                                }
                            }

                        }
                        else
                        {
                            if (bplib.clsWebLib.GetBoolData(dtlocal.Rows[i]["YesterDayReConfirm"].ToString()) == true)
                            {
                                sheet1.Range[xlsRow, cEmpYstdyOt].CellStyle.Interior.ColorIndex = ExcelKnownColors.Orange;
                            }
                        }
                        if (!string.IsNullOrEmpty(dtlocal.Rows[i]["YesterDayDayCategory"].ToString()))
                        {
                            if (dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Present" || dtlocal.Rows[i]["YesterDayDayCategory"].ToString() == "Late")
                            {
                                oru.GetOT(dtlocal.Rows[i]["OTConsiderOn"].ToString(), dtlocal.Rows[i]["YesterdayOTHr"].ToString(), out yot);
                            }
                        }

                    }


                    sheet1.Range[xlsRow, cEmpYstdyOt].Text = yot;

                    sheet1.Range[xlsRow, cEmpYstdyOt].BorderAround(ExcelLineStyle.Thin);
                    sheet1.Range[xlsRow, cEmpYstdyOt].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, cEmpYstdyOt].VerticalAlignment = ExcelVAlign.VAlignTop;

                    string tyot = string.Empty;//OTConsiderOn
                    if (bplib.clsWebLib.GetBoolData(dtlocal.Rows[i]["IsOTEntitledToday"].ToString()) == true)
                    {
                        if (string.IsNullOrEmpty(dtlocal.Rows[i]["OTHr"].ToString()))
                        {
                            if (!string.IsNullOrEmpty(dtlocal.Rows[i]["ToDayDayCategory"].ToString()))
                            {
                                if (dtlocal.Rows[i]["ToDayDayCategory"].ToString() == "Present" || dtlocal.Rows[i]["ToDayDayCategory"].ToString() == "Late")
                                {
                                    sheet1.Range[xlsRow, cEmpTodyOt].CellStyle.Interior.ColorIndex = ExcelKnownColors.Light_yellow;
                                }
                            }

                        }
                        else
                        {
                            if (bplib.clsWebLib.GetBoolData(dtlocal.Rows[i]["ToDayReConfirm"].ToString()) == true)
                            {
                                sheet1.Range[xlsRow, cEmpTodyOt].CellStyle.Interior.ColorIndex = ExcelKnownColors.Orange;
                            }
                        }
                        if (!string.IsNullOrEmpty(dtlocal.Rows[i]["ToDayDayCategory"].ToString()))
                        {
                            if (dtlocal.Rows[i]["ToDayDayCategory"].ToString() == "Present" || dtlocal.Rows[i]["ToDayDayCategory"].ToString() == "Late")
                            {
                                oru.GetOT(dtlocal.Rows[i]["OTConsiderOn"].ToString(), dtlocal.Rows[i]["OTHr"].ToString(), out tyot);
                            }
                        }

                    }
                    sheet1.Range[xlsRow, cEmpTodyOt].Text = tyot;
                    sheet1.Range[xlsRow, cEmpTodyOt].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, cEmpTodyOt].VerticalAlignment = ExcelVAlign.VAlignTop;
                    xlsRow++;
                }

                sheet1.Range[startDataRow, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[startDataRow, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[startDataRow, 1, xlsRow - 1, endXlsCol].CellStyle.Font.Size = 15f;

                sheet1.Range[10, 1, xlsRow, cInchargeSignature].RowHeight = 65;

                //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].RowHeight = 30;

                #endregion ------Data Set------

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.Range["A1"].CellStyle.Font.Size = 18;
                sheet1.Range["A2"].CellStyle.Font.Size = 18;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                //sheet1.UsedRange.BorderAround(ExcelLineStyle.Thin);

                #endregion UsedRange Alignment

                #region ******************Company Name Header******************

                xlsRow = 1;
                xlsCol = 1;

                FactoryName = string.Empty;

                //string FactoryAddress = string.Empty;

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
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 16;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsFactory.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion ******************Company Name Header******************

                #region ******************Report Header******************

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "Daily Day Status Report" + " (" + SheetHeader + ")";
                sheet1.Range[xlsRow, cSrl, xlsRow, cEmpName].Merge();
                sheet1.Range[xlsRow, cSrl, xlsRow, cEmpName].CellStyle.Font.Size = 15;
                sheet1.Range[xlsRow, 15].Text = "Date:- " + TextFromDate;
                sheet1.Range[xlsRow, 4, xlsRow, 11].Merge();
                sheet1.Range[xlsRow, 15].CellStyle.Font.Size = 15;
                //sheet1.Range[xlsRow, 16].Text = "Previous Date:- " + PrevWorkDate;
                //sheet1.Range[xlsRow, 4, xlsRow, 11].Merge();
                //sheet1.Range[xlsRow, 16].CellStyle.Font.Size = 15;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 13].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                sheet1.Range[1, 1, 6, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[1, 1, 6, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                #endregion ******************Report Header******************

                #region Freeze Panes
                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A10"].FreezePanes();

                #endregion Freeze Panes

                #region Page Setup
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$9";
                sheet1.PageSetup.RightFooter = "&\"Arial Narrow\"&10" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Arial Narrow\"&10" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;



                #endregion Page Setup
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<ComboModel> GetSectionCboByDepartment(string deptID)
        {
            var sql = @"SELECT DISTINCT S.Id,S.UserName FROM ORG.Position  P
						  LEFT JOIN ORG.Section S ON S.Id=P.SectionId
						   WHERE P.DepartmentId= '" + deptID + "'";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }
        public IEnumerable<ComboModel> GetSubSectionCboBySection(string secID)
        {
            var sql = @"SELECT distinct SB.Id,SB.UserName FROM ORG.Position P
                        LEFT JOIN ORG.SubSection SB ON SB.Id=P.SubSectionId
                          WHERE p.SectionId='" + secID + "'";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }
        public IEnumerable<ComboModel> GetLineCboBySubSection(string subsecID)
        {
            var sql = @"select distinct L.Id, L.UserName from  mst.ManpowerBudget M
							 left join ORG.Line L ON L.Id=M.LineId
							 Where M.PositionId in (select Id from org.Position where subsectionid in (select Id from org.SubSection where Id='" + subsecID + "'))";
            return _sqlRepository.GetCombo(sql, "Id", "UserName");
        }
        public IEnumerable<ComboModel> GetAttendanceDayStatus()
        {
            var sql = @"select distinct Category from DayType";
            return _sqlRepository.GetCombo(sql, "Category", "Category");
        }

        public DataTable GetMonthlyOT(string EmployeeId, string FirstDate, string LastDate)
        {
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT SUM(NormalOtHr)/60 TotalOtHrMonth FROM FinalOt
                                where EmpSystemId= '" + EmployeeId + @"' AND WorkDate BETWEEN '" + FirstDate + @"' AND '" + LastDate + @"' 
                                GROUP BY 
                                EmpSystemId";
                return _sqlRepository.GetDataTable(strSql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public DataTable GetWeekOffDate(string EmployeeId, string FirstDate, string LastDate)
        {
            string strSql = string.Empty;
            try
            {
                strSql = @"select isnull(Max(WorkDate),0) WeekOffDate
                                   from EmpDateWiseShiftAssign 
	                               where DayType = 'W' and WorkDate < '" + LastDate + @"' and EmpSystemID = '" + EmployeeId + @"'";
                return _sqlRepository.GetDataTable(strSql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }



    }
}
