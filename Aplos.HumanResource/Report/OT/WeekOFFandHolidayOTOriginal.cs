using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using System.Drawing;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Library.Service.Payrolls.OT;
using Library.Core;
using ConnectionManager;
using static Library.Service.Helpers.ReportUtility;
using Library.Model.Enums;
using Library.Crosscutting.Security;
using System.Threading;

namespace Library.HumanResource.Report.OT
{
    public class WeekOFFandHolidayOTOriginal
    {
        public string NumberFormatTwoDecimal = "#,##0.00;(#,##0.00)";
        SqlRepository _sqlRepository = null;

        public WeekOFFandHolidayOTOriginal()
        {
            _sqlRepository = new SqlRepository();
        }
        public IWorkbook GetWeekOFFExtraOT(string Name, string CompanyGroupId, string CompanyId, string PlantId, string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = "";
            string _currencyId = "";
            Dictionary<string, double> dicNW = null;
            Dictionary<string, double> dicW = null;
            Dictionary<string, double> dicH = null;

            #endregion
            try
            {
                Dictionary<string, DataRow> dicHourlyOTNW = new Dictionary<string, DataRow>();

                DataTable dtEmpHourlyOt = getHourlyOT(PlantId, Month, Year, parameters);

                dicHourlyOTNW = GetDictionaryHourotmonthReportwithoutWeekendHoliday(Year, Month, PlantId, CompanyId, CompanyGroupId, parameters, isActive, isSeperated);
                if (dtEmpHourlyOt.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data Found....");
                    throw (ex);
                }

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                Dictionary<string, DataRow> dicHourlyOTW = new Dictionary<string, DataRow>();
                dicHourlyOTW = GetDictionaryHourOTMonthReportWithWeekendORHoliday(Year, Month, PlantId, CompanyId, CompanyGroupId, parameters, isActive, isSeperated, "Weekend");



                string FirstDayOfTheMonth = "01-" + bplib.clsWebLib.GetMonthName(Month) + "-" + Year;
                string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                DataSet dsCurrency = null;

                Dictionary<string, List<DataRow>> dicSalStructure = LoadSalaryStructure(PlantId, FirstDayOfTheMonth, LastDayOfTheMonth);
                Dictionary<string, DataRow> dicOTpolicy = LoadOverTimePolicy(PlantId, FirstDayOfTheMonth, LastDayOfTheMonth);

                clsSalaryInfo objSal = new clsSalaryInfo();
                objSal.GetLocalCurrency(CompanyGroupId, PlantId, out dsCurrency);
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }
                GenerateDic(dicOTpolicy, dicSalStructure, _currencyId, out dicNW, out dicW, out dicH);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                var iName = 0;
                var iEmployeeCode = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iBasic = 0;
                var iWeekDayOTHr = 0;
                var iAmount = 0;
                var iLine = 0;
                var totalAmount = 0.00;
                var iDOJ = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var totalEntryAmount = 0.00;
                var totalNWOTAmount = 0.00;
                var totalWOTAmount = 0.00;



                var isl = 0;
                var SLNo = 1;


                #region Hourly Ot

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "Emp Name";
                sheet1.Range[xlsRow, iName].ColumnWidth = 25;

                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;
                xlsCol += 1;
                int iDOS = xlsCol;
                sheet1.Range[xlsRow, iDOS].Text = "DOS";
                sheet1.Range[xlsRow, iDOS].ColumnWidth = 20;
                xlsCol += 1;
                iDepartment = xlsCol;
                sheet1.Range[xlsRow, iDepartment].Text = "Department";
                sheet1.Range[xlsRow, iDepartment].ColumnWidth = 25;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                sheet1.Range[xlsRow, iDesignation].ColumnWidth = 25;
                xlsCol += 1;
                int iPayrollGroup = xlsCol;
                sheet1.Range[xlsRow, iPayrollGroup].Text = "PayrollGroup";
                sheet1.Range[xlsRow, iPayrollGroup].ColumnWidth = 25;

                xlsCol += 1;
                int iCostCenter = xlsCol;
                sheet1.Range[xlsRow, iCostCenter].Text = "Cost Center";
                sheet1.Range[xlsRow, iCostCenter].ColumnWidth = 25;
                xlsCol += 1;
                int iEmployeeCategory = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                sheet1.Range[xlsRow, iEmployeeCategory].ColumnWidth = 25;

                xlsCol += 1;
                iSection = xlsCol;
                sheet1.Range[xlsRow, iSection].Text = "Section";
                sheet1.Range[xlsRow, iSection].ColumnWidth = 14;

                xlsCol += 1;
                iSubSection = xlsCol;
                sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                sheet1.Range[xlsRow, iSubSection].ColumnWidth = 16;

                xlsCol += 1;
                iLine = xlsCol;
                sheet1.Range[xlsRow, iLine].Text = "Line";
                sheet1.Range[xlsRow, iLine].ColumnWidth = 12;

                xlsCol += 1;
                iWeekDayOTHr = xlsCol;
                sheet1.Range[xlsRow, iWeekDayOTHr].Text = "Week Day(OT Hrs)";
                sheet1.Range[xlsRow, iWeekDayOTHr].ColumnWidth = 15;

                xlsCol += 1;
                int iTotalWkOffHr = xlsCol;
                sheet1.Range[xlsRow, iTotalWkOffHr].Text = "WeekOff(OT Hrs)";
                sheet1.Range[xlsRow, iTotalWkOffHr].ColumnWidth = 15;

                xlsCol += 1;
                iBasic = xlsCol;
                sheet1.Range[xlsRow, iBasic].Text = "Fixed Gross";
                sheet1.Range[xlsRow, iBasic].ColumnWidth = 15;


                xlsCol += 1;
                iAmount = xlsCol;
                sheet1.Range[xlsRow, iAmount].Text = "Amount";
                sheet1.Range[xlsRow, iAmount].ColumnWidth = 15;

                xlsCol += 1;
                int iBankAccNo = xlsCol;
                sheet1.Range[xlsRow, iBankAccNo].Text = "Bank Acc No";
                sheet1.Range[xlsRow, iBankAccNo].ColumnWidth = 25;

                xlsCol += 1;
                int iBankName = xlsCol;
                sheet1.Range[xlsRow, iBankName].Text = "Bank Name";
                sheet1.Range[xlsRow, iBankName].ColumnWidth = 25;
                xlsCol += 1;
                int iIFSCNO = xlsCol;
                sheet1.Range[xlsRow, iIFSCNO].Text = "IFSC";
                sheet1.Range[xlsRow, iIFSCNO].ColumnWidth = 25;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                xlsRow++;

                #endregion ------------------Column Header---------------

                #region ----------------------Data-----------------------  
                string yot = string.Empty;
                string nwotFormated = string.Empty;

                double FOT = 0.00;
                double NWOT = 0.00;
                double entryAmount = 0.00;
                string EmpSystemid = "";
                int firstLoopRow = xlsRow;
                for (int i = 0; i < dtEmpHourlyOt.Rows.Count; i++)
                {
                    FOT = 0.00;
                    NWOT = 0.00;
                    nwotFormated = "";
                    yot = "";
                    entryAmount = 0.00;
                    EmpSystemid = dtEmpHourlyOt.Rows[i]["EmpSystemId"].ToString();
                    if (EmpSystemid == "207091")
                    {

                    }
                    //OTConsiderOn = dtrOTNW["OTConsiderOn"].ToString();

                    if (dicHourlyOTNW.ContainsKey(EmpSystemid))
                    {

                        DataRow dtrOTNW = dicHourlyOTNW[EmpSystemid];

                        NWOT = clsStaticInfo.dbl(dtrOTNW["Duration"].ToString());

                        oru.GetOT(OTConsiderOn, dtrOTNW["Duration"].ToString(), out nwotFormated);

                    }

                    if (dicHourlyOTW.ContainsKey(EmpSystemid))
                    {
                        DataRow dtrOTW = dicHourlyOTW[EmpSystemid];

                        FOT = clsStaticInfo.dbl(dtrOTW["Duration"].ToString());

                        oru.GetOT(OTConsiderOn, dtrOTW["Duration"].ToString(), out yot);

                    }
                    if (FOT == 0.00 && NWOT == 0.00)
                    {
                        continue;
                    }
                    sheet1.Range[xlsRow, iTotalWkOffHr].Text = yot;
                    sheet1.Range[xlsRow, iTotalWkOffHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, iTotalWkOffHr].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (dicSalStructure.ContainsKey(EmpSystemid))
                    {
                        List<DataRow> drSalaryHeadCollection = dicSalStructure[EmpSystemid].Where(x => x["HeadCategory"].ToString().ToUpper() == "GROSS").ToList();


                        entryAmount = clsStaticInfo.dbl(drSalaryHeadCollection[0]["Amount"].ToString());
                    }




                    //oru.GetOT(dtrOTNW["OTConsiderOn"].ToString(), dtrOTNW["Duration"].ToString(), out yot);

                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, iName].Text = dtEmpHourlyOt.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtEmpHourlyOt.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iDOJ].DateTime = Convert.ToDateTime(dtEmpHourlyOt.Rows[i]["DOJ"].ToString());
                    sheet1.Range[xlsRow, iDOJ].NumberFormat = "dd-MMM-yyyy";
                    if (!String.IsNullOrEmpty(dtEmpHourlyOt.Rows[i]["DOS"].ToString()))
                    {
                        sheet1.Range[xlsRow, iDOS].DateTime = Convert.ToDateTime(dtEmpHourlyOt.Rows[i]["DOS"].ToString());
                        sheet1.Range[xlsRow, iDOS].NumberFormat = "dd-MMM-yyyy";
                    }

                    sheet1.Range[xlsRow, iDesignation].Text = dtEmpHourlyOt.Rows[i]["Designation"].ToString();
                    sheet1.Range[xlsRow, iDepartment].Text = dtEmpHourlyOt.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, iSection].Text = dtEmpHourlyOt.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, iSubSection].Text = dtEmpHourlyOt.Rows[i]["SubSection"].ToString();
                    sheet1.Range[xlsRow, iLine].Text = dtEmpHourlyOt.Rows[i]["Line"].ToString();

                    sheet1.Range[xlsRow, iPayrollGroup].Text = dtEmpHourlyOt.Rows[i]["PayRollGroup"].ToString();
                    sheet1.Range[xlsRow, iBankAccNo].Text = dtEmpHourlyOt.Rows[i]["BankAccNo"].ToString();
                    sheet1.Range[xlsRow, iBankName].Text = dtEmpHourlyOt.Rows[i]["BankName"].ToString();
                    sheet1.Range[xlsRow, iIFSCNO].Text = dtEmpHourlyOt.Rows[i]["IFSCCode"].ToString();
                    sheet1.Range[xlsRow, iEmployeeCategory].Text = dtEmpHourlyOt.Rows[i]["EmployeeCategory"].ToString();

                    sheet1.Range[xlsRow, iWeekDayOTHr].Text = nwotFormated;
                    sheet1.Range[xlsRow, iWeekDayOTHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, iWeekDayOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    totalEntryAmount += NWOT + FOT;
                    totalNWOTAmount += NWOT;
                    totalWOTAmount += FOT;


                    sheet1.Range[xlsRow, iBasic].Number = clsStaticInfo.dbl(entryAmount);
                    sheet1.Range[xlsRow, iBasic].NumberFormat = NumberFormatTwoDecimal;


                    if (dicW.ContainsKey(EmpSystemid))
                    {
                        FOT = (FOT / 60) * dicW[EmpSystemid];
                    }
                    if (dicNW.ContainsKey(EmpSystemid))
                    {
                        sheet1.Range[xlsRow, iAmount].Number = clsStaticInfo.dbl(dicNW[EmpSystemid]) * (NWOT / 60) + FOT;

                        sheet1.Range[xlsRow, iAmount].NumberFormat = NumberFormatTwoDecimal;
                        totalAmount += clsStaticInfo.dbl(dicNW[EmpSystemid]) * (NWOT / 60) + (FOT / 60);
                    }

                    xlsRow++;
                    SLNo++;
                }


                string Tot = string.Empty;
                oru.GetOT(OTConsiderOn, totalNWOTAmount.ToString(), out Tot);
                sheet1.Range[xlsRow, iLine].Text = "Total";
                sheet1.Range[xlsRow, iLine + 1].Text = Tot;
                oru.GetOT(OTConsiderOn, totalWOTAmount.ToString(), out Tot);
                sheet1.Range[xlsRow, iLine + 2].Text = Tot;

                sheet1.Range[xlsRow, iLine + 4].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(iAmount) + firstLoopRow + ":" + clsStaticInfo.GetxlsCol(iAmount) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, iLine + 4].NumberFormat = NumberFormatTwoDecimal;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************

                xlsRow = 1;
                xlsCol = 1;

                string reportTitle = "Week day ExtraOt and WeekOff OT For " + bplib.clsWebLib.GetMonthName(Month) + @", " + Year + @"";
                ReporHeader(CompanyId, PlantId, xlsRow, xlsCol, endXlsCol, reportTitle, sheet1);
                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Hourly OT Monthly";
                #endregion Page Setup

                #endregion  Attendance Summary Status

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IWorkbook GetholidayExtraOT(string Name, string CompanyGroupId, string CompanyId, string PlantId, string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsHourlyOffDutyTag = null;
            DataSet dsHourlyOffDutyWithoutWeekend = null;

            DataTable dtHourlyOffDutyTag = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataView dvOT = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;

            // DataSet dsOTPolicy = null;
            DataSet dsSStructure = null;
            string _currencyId = string.Empty;
            Dictionary<string, double> dicNW = null;
            Dictionary<string, double> dicW = null;
            Dictionary<string, double> dicH = null;

            #endregion
            try
            {

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);
                objRpt.SelectedPlant(PlantId, out dsFactory);
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                string Plant = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            Plant += parameters["PlantId"];
                        }
                    }
                }
                catch (Exception)
                {

                }

                Dictionary<string, string> dicStrSal = new Dictionary<string, string>();

                Dictionary<string, DataRow> dicHourlyOTH = new Dictionary<string, DataRow>();

                dicHourlyOTH = GetDictionaryHourOTMonthReportWithWeekendORHolidaySaad(Year, Month, CompanyId, CompanyGroupId, parameters, isActive, isSeperated, "Holiday");


                if (dicHourlyOTH.Count == 0)
                {
                    Exception ex = new Exception("No Data Found....");
                    throw (ex);
                }


                string FirstDayOfTheMonth = "01-" + Month + "-" + Year;
                string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                DataSet dsCurrency = null;


                Dictionary<string, List<DataRow>> dicSalStructure = LoadSalaryStructure(Plant, FirstDayOfTheMonth, LastDayOfTheMonth);
                Dictionary<string, DataRow> dicOTpolicy = LoadOverTimePolicy(Plant, FirstDayOfTheMonth, LastDayOfTheMonth);

                clsSalaryInfo objSal = new clsSalaryInfo();
                objSal.GetLocalCurrency(CompanyGroupId, Plant, out dsCurrency);
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }
                GenerateDic(dicOTpolicy, dicSalStructure, _currencyId, out dicNW, out dicW, out dicH);



                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                var iName = 0;
                var iEmployeeCode = 0;
                //var iDepartment = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iBasic = 0;
                var iTotalHr = 0;
                var iRate = 0;
                var iAmount = 0;
                var iLine = 0;
                var totalAmount = 0.00;
                var iDOJ = 0;
                var iPlant = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var itotal = 0;
                var totalEntryAmount = 0.00;
                var iGender = 0;
                var isl = 0;
                var SLNo = 1;


                #region Hourly Ot

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "Emp Name";
                sheet1.Range[xlsRow, iName].ColumnWidth = 25;

                xlsCol += 1;
                iPlant = xlsCol;
                sheet1.Range[xlsRow, iPlant].Text = "Plant";
                sheet1.Range[xlsRow, iPlant].ColumnWidth = 20;

                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;
                xlsCol += 1;
                int iDOS = xlsCol;
                sheet1.Range[xlsRow, iDOS].Text = "DOS";
                sheet1.Range[xlsRow, iDOS].ColumnWidth = 20;
                xlsCol += 1;
                iDepartment = xlsCol;
                sheet1.Range[xlsRow, iDepartment].Text = "Department";
                sheet1.Range[xlsRow, iDepartment].ColumnWidth = 25;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                sheet1.Range[xlsRow, iDesignation].ColumnWidth = 25;
                xlsCol += 1;
                int iPayrollGroup = xlsCol;
                sheet1.Range[xlsRow, iPayrollGroup].Text = "PayrollGroup";
                sheet1.Range[xlsRow, iPayrollGroup].ColumnWidth = 25;

                xlsCol += 1;
                int iCostCenter = xlsCol;
                sheet1.Range[xlsRow, iCostCenter].Text = "Cost Center";
                sheet1.Range[xlsRow, iCostCenter].ColumnWidth = 25;
                xlsCol += 1;
                int iEmployeeCategory = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                sheet1.Range[xlsRow, iEmployeeCategory].ColumnWidth = 25;

                //xlsCol += 1;
                //iGender = xlsCol;
                //sheet1.Range[xlsRow, iGender].Text = "Gender";
                //sheet1.Range[xlsRow, iGender].ColumnWidth = 15;

                xlsCol += 1;
                iSection = xlsCol;
                sheet1.Range[xlsRow, iSection].Text = "Section";
                sheet1.Range[xlsRow, iSection].ColumnWidth = 14;

                xlsCol += 1;
                iSubSection = xlsCol;
                sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                sheet1.Range[xlsRow, iSubSection].ColumnWidth = 16;

                xlsCol += 1;
                iLine = xlsCol;
                sheet1.Range[xlsRow, iLine].Text = "Line";
                sheet1.Range[xlsRow, iLine].ColumnWidth = 12;

                xlsCol += 1;
                iTotalHr = xlsCol;
                sheet1.Range[xlsRow, iTotalHr].Text = "Holiday(OT Hrs)";
                sheet1.Range[xlsRow, iTotalHr].ColumnWidth = 15;

                //xlsCol += 1;
                //int iTotalWkOffHr = xlsCol;
                //sheet1.Range[xlsRow, iTotalWkOffHr].Text = "Total WeekOff(Hrs)";
                //sheet1.Range[xlsRow, iTotalWkOffHr].ColumnWidth = 15;

                xlsCol += 1;
                iBasic = xlsCol;
                sheet1.Range[xlsRow, iBasic].Text = "Fixed Gross";
                sheet1.Range[xlsRow, iBasic].ColumnWidth = 15;

                //xlsCol += 1;
                //iRate = xlsCol;
                //sheet1.Range[xlsRow, iRate].Text = "Rate";
                //sheet1.Range[xlsRow, iRate].ColumnWidth = 15;

                xlsCol += 1;
                iAmount = xlsCol;
                sheet1.Range[xlsRow, iAmount].Text = "Amount";
                sheet1.Range[xlsRow, iAmount].ColumnWidth = 15;

                xlsCol += 1;
                int iBankAccNo = xlsCol;
                sheet1.Range[xlsRow, iBankAccNo].Text = "Bank Acc No";
                sheet1.Range[xlsRow, iBankAccNo].ColumnWidth = 25;

                xlsCol += 1;
                int iBankName = xlsCol;
                sheet1.Range[xlsRow, iBankName].Text = "Bank Name";
                sheet1.Range[xlsRow, iBankName].ColumnWidth = 25;
                xlsCol += 1;
                int iIFSCNO = xlsCol;
                sheet1.Range[xlsRow, iIFSCNO].Text = "IFSC";
                sheet1.Range[xlsRow, iIFSCNO].ColumnWidth = 25;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                xlsRow++;

                #endregion ------------------Column Header------------------
                #region ----------------------Data-----------------------  
                string yot = "";
                string EmpSystemid = "";
                double entryAmount = 0.00;


                double FOT = 0.00;
                foreach (var item in dicHourlyOTH)
                {
                    FOT = 0.00;
                    entryAmount = 0.00;
                    EmpSystemid = item.Key;
                    DataRow dtrOTH = item.Value;
                    OTConsiderOn = dtrOTH["OTConsiderOn"].ToString();

                    if (FOT == 0.00 && clsStaticInfo.dbl(dtrOTH["Duration"].ToString()) == 0.00)
                    {
                        continue;
                    }



                    if (dicSalStructure.ContainsKey(EmpSystemid))
                    {
                        List<DataRow> drSalaryHeadCollection = dicSalStructure[EmpSystemid].Where(x => x["HeadCategory"].ToString().ToUpper() == "GROSS").ToList();


                        entryAmount = clsStaticInfo.dbl(drSalaryHeadCollection[0]["Amount"].ToString());
                    }





                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, iName].Text = dtrOTH["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtrOTH["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iPlant].Text = dtrOTH["PlantName"].ToString();
                    sheet1.Range[xlsRow, iDOJ].DateTime = Convert.ToDateTime(dtrOTH["DOJ"].ToString());
                    sheet1.Range[xlsRow, iDOJ].NumberFormat = "dd-MMM-yyyy";
                    if (!String.IsNullOrEmpty(dtrOTH["DOS"].ToString()))
                    {
                        sheet1.Range[xlsRow, iDOS].DateTime = Convert.ToDateTime(dtrOTH["DOS"].ToString());
                        sheet1.Range[xlsRow, iDOS].NumberFormat = "dd-MMM-yyyy";

                    }

                    sheet1.Range[xlsRow, iDesignation].Text = dtrOTH["Designation"].ToString();
                    sheet1.Range[xlsRow, iDepartment].Text = dtrOTH["Department"].ToString();
                    sheet1.Range[xlsRow, iSection].Text = dtrOTH["Section"].ToString();
                    sheet1.Range[xlsRow, iSubSection].Text = dtrOTH["SubSection"].ToString();
                    sheet1.Range[xlsRow, iLine].Text = dtrOTH["Line"].ToString();

                    sheet1.Range[xlsRow, iPayrollGroup].Text = dtrOTH["PayRollGroup"].ToString();
                    sheet1.Range[xlsRow, iBankAccNo].Text = dtrOTH["BankAccNo"].ToString();
                    sheet1.Range[xlsRow, iBankName].Text = dtrOTH["BankName"].ToString();
                    sheet1.Range[xlsRow, iIFSCNO].Text = dtrOTH["IFSCCode"].ToString();
                    sheet1.Range[xlsRow, iEmployeeCategory].Text = dtrOTH["EmployeeCategory"].ToString();

                    oru.GetOT(dtrOTH["OTConsiderOn"].ToString(), dtrOTH["Duration"].ToString(), out yot);

                    sheet1.Range[xlsRow, iTotalHr].Text = yot;
                    sheet1.Range[xlsRow, iTotalHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, iTotalHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    totalEntryAmount += clsStaticInfo.dbl(dtrOTH["Duration"].ToString()) + FOT;

                    sheet1.Range[xlsRow, iBasic].Number = clsStaticInfo.dbl(entryAmount);
                    sheet1.Range[xlsRow, iBasic].NumberFormat = NumberFormatTwoDecimal;


                    //if (dicW.ContainsKey(EmpSystemid))
                    //{
                    //    FOT = (FOT / 60) * dicW[EmpSystemid];
                    //}
                    if (dicH.ContainsKey(EmpSystemid))
                    {
                        sheet1.Range[xlsRow, iAmount].Number = clsStaticInfo.dbl(dicH[EmpSystemid]) * (clsStaticInfo.dbl(dtrOTH["DurationH"].ToString())) + FOT;

                        sheet1.Range[xlsRow, iAmount].NumberFormat = NumberFormatTwoDecimal;
                        totalAmount += clsStaticInfo.dbl(dicH[EmpSystemid]) * (clsStaticInfo.dbl(dtrOTH["DurationH"].ToString()) + (FOT / 60));

                    }

                    xlsRow++;
                    SLNo++;
                }

                string Zot = string.Empty;
                oru.GetOT(OTConsiderOn, totalEntryAmount.ToString(), out Zot);

                sheet1.Range[xlsRow, iLine].Text = "Total";
                sheet1.Range[xlsRow, iLine + 1].Text = Zot;

                sheet1.Range[xlsRow, iLine + 4].Number = totalAmount;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                #endregion ----------------------Data-----------------------


                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;

                string reportTitle = "Week day ExtraOt and WeekOff OT For " + bplib.clsWebLib.GetMonthName(Month) + @", " + Year + @"";
                ReporHeader(CompanyId, PlantId, xlsRow, xlsCol, endXlsCol, reportTitle, sheet1);

                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Hourly OT Monthly";
                #endregion Page Setup

                #endregion  Attendance Summary Status

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IWorkbook GetSalarySheetExtraOTCTCReport(string companyGroupId, string companyId, string plantId, string userId, string month, string year, string salaryProcessId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, bool sa, bool ca, bool isTopSheet)
        {
            #region Variable
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsReport objRpt = null;
            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsEmpLoyeeInfo = null;
            DataTable dtEmployees = null;

            DataView dvSlrSheet = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;
            var FactoryName = string.Empty;
            var CmpName = string.Empty;
            var done1 = "Ok";
            var done2 = "Ok";
            var done3 = "Ok";
            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            int colCtc = 0;
            int endGenericColumn = 0;
            #endregion Variable

            try
            {
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
                var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
                var fdateOfMonth = "1" + "-" + monthName + "-" + year;
                string strPath = "";
                Image companyLogo = null;
                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();

                string FromDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), 1).ToString("dd-MMM-yyyy");
                string ToDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), daysInMonth).ToString("dd-MMM-yyyy");

                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                var para = new ParamList();
                var leavePara = new ParamList();
                var attdnProcessParam = new ParamList();

                #endregion Variable

                #region DataSet

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, parameters, month.ToInt(), year.ToInt(), out dsExtraAbsent);

                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);


                #region GWR Extra OT (Weekend WeekOFF & Holiday)
                Dictionary<string, double> dicNW = null;
                Dictionary<string, double> dicW = null;
                Dictionary<string, double> dicH = null;
                DataSet dsCurrency = null;

                Dictionary<string, DataRow> dicHourlyOTNW = new Dictionary<string, DataRow>();
                Dictionary<string, DataRow> dicHourlyOTW = new Dictionary<string, DataRow>();
                Dictionary<string, DataRow> dicHourlyOTH = new Dictionary<string, DataRow>();

                dicHourlyOTNW = GetDictionaryHourotmonthReportwithoutWeekendHolidayExtraOTCTC(year, month, plantId, companyId, companyGroupId, parameters, isActive, isSeperated);
                dicHourlyOTW = GetDictionaryHourOTMonthReportWithWeekendORHolidayPOSTCTC(year, month, plantId, companyId, companyGroupId, parameters, isActive, isSeperated, "Weekend");
                dicHourlyOTH = GetDictionaryHourOTMonthReportWithWeekendORHolidayPOSTCTC(year, month, plantId, companyId, companyGroupId, parameters, isActive, isSeperated, "Holiday");


                //otc.LoadSalaryStructure(plantId, fdateOfMonth, ldateOfMonth, out dsSStructureOT);
                //otc.LoadOverTimePolicy(plantId, fdateOfMonth, ldateOfMonth, out dsOTPolicy);



                Dictionary<string, List<DataRow>> dicSalStructure = LoadSalaryStructure(plantId, fdateOfMonth, ldateOfMonth);
                Dictionary<string, DataRow> dicOTpolicy = LoadOverTimePolicy(plantId, fdateOfMonth, ldateOfMonth);

                clsSalaryInfo objSal = new clsSalaryInfo();
                objSal.GetLocalCurrency(companyGroupId, plantId, out dsCurrency);
                string _currencyId = "";
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }
                GenerateDic(dicOTpolicy, dicSalStructure, _currencyId, out dicNW, out dicW, out dicH);

                #endregion


                //Sql Salary Structure 
                List<SalarySheetReportUD> listdsSlrStr = new List<SalarySheetReportUD>();

                //Sql Salary Process 
                DataTable dtSalaryHeadSheet;
                List<SalarySheetReportUD> listdsSlrProc = new List<SalarySheetReportUD>();
                GetEmployeeInfoDetailSalaryLogWise(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo);//Sql Query For Salary  Data
                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetail(companyGroupId, companyId, plantId, fdateOfMonth, ldateOfMonth, salaryProcessId, payRollGroup, parameters, out dtSalaryHeadSheet);

                if (dicEmpSalry.First().Value[0].Table.Rows.Count > 0)
                {
                    listdsSlrProc = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    listdsSlrStr = dicEmpSalry.First().Value[0].Table.ToList<SalarySheetReportUD>();
                    dtEmployees = dsEmpLoyeeInfo.Tables[0];//dicEmpSalry.First().Value[0].Table;
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                dvSlrSheet = new DataView();

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                clsSalaryProcessAplosR r = new clsSalaryProcessAplosR();
                r.GetSundayMondayCount(FromDate, ToDate, out Dictionary<string, int> DicWeekOffCount);
                r.GetWeekOffAll(identity.PlantId, ToDate, out DataSet dsEmployeeWiseWeekoff);
                Dictionary<string, string> dicEmployeeWiseWeekoff = new Dictionary<string, string>();
                for (int i = 0; i < dsEmployeeWiseWeekoff.Tables[0].Rows.Count; i++)
                    if (dicEmployeeWiseWeekoff.ContainsKey(dsEmployeeWiseWeekoff.Tables[0].Rows[i]["EmpSystemID"].ToString()) == false)
                        dicEmployeeWiseWeekoff.Add(dsEmployeeWiseWeekoff.Tables[0].Rows[i]["EmpSystemID"].ToString(), dsEmployeeWiseWeekoff.Tables[0].Rows[i]["offday"].ToString());

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                if (isTopSheet == true)
                {
                    workbook = application.Workbooks.Create(5);
                }
                else
                {
                    workbook = application.Workbooks.Create(1);
                }

                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 6;
                xlsCol = 1;

                #region Column Variables
                int ColSr = 0, ColIDNo = 0, ColName = 0, ColDOJ = 0, ColDOS = 0, ColPlantName = 0, cDept = 0, cSec = 0, cSubSec = 0, cLine = 0, cPayrollGroup = 0, cJobLocation = 0, cGender = 0,
                    cGrade = 0, ColGVDG = 0, ColGrs = 0, colPayDays = 0, ColPdDy = 0, ColLate = 0, ColAbDy = 0, ColHlDy = 0, ColWkOf = 0, ColLv = 0, ColMLv = 0, colBank = 0, colBankAccountNo = 0
                   , ColLWP = 0, cDMP = 0, ColExtraAbsent = 0, colEmpCurrentStat = 0, colEmpStatus = 0, cPaymentMode = 0, cUnit = 0, ColTotalOTHR = 0, colDirectManpowerCost = 0, colBasic = 0, colGross = 0, colCTC = 0;
                int npstruct = 0;

                #endregion

                //1
                SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 12);
                SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 17);
                SetCellValue("Plant Name", sheet1, xlsRow, ref xlsCol, out ColPlantName, 17);
                SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 12);
                SetCellValue("EmployeeCurrentStatus", sheet1, xlsRow, ref xlsCol, out colEmpCurrentStat, 12);
                SetCellValue("EmployeeSatatus", sheet1, xlsRow, ref xlsCol, out colEmpStatus, 12);
                SetCellValue("Gender", sheet1, xlsRow, ref xlsCol, out cGender, 12);
                SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 25);
                SetCellValue("Employee Category", sheet1, xlsRow, ref xlsCol, out int colEmpCategory, 25);
                SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);
                SetCellValue("Section", sheet1, xlsRow, ref xlsCol, out cSec, 25);
                SetCellValue("SubSection", sheet1, xlsRow, ref xlsCol, out cSubSec, 25);
                SetCellValue("Unit", sheet1, xlsRow, ref xlsCol, out cUnit, 25);
                SetCellValue("Line", sheet1, xlsRow, ref xlsCol, out cLine, 25);
                SetCellValue("JobLocation", sheet1, xlsRow, ref xlsCol, out cJobLocation, 25);
                SetCellValue("Payroll group", sheet1, xlsRow, ref xlsCol, out cPayrollGroup, 25);
                SetCellValue("Payment mode", sheet1, xlsRow, ref xlsCol, out cPaymentMode, 25);
                SetCellValue("Bank", sheet1, xlsRow, ref xlsCol, out colBank, 25);
                SetCellValue("Bank Acc No.", sheet1, xlsRow, ref xlsCol, out colBankAccountNo, 25);
                SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out cGrade, 25);
                SetCellValue("Direct Manpower Cost", sheet1, xlsRow, ref xlsCol, out colDirectManpowerCost, 25);

                SetCellValue("Working Days", sheet1, xlsRow, ref xlsCol, out int colWorkingDays, 9);
                SetCellValue("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 5);
                SetCellValue("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 9);
                SetCellValue("Late", sheet1, xlsRow, ref xlsCol, out ColLate, 9);
                SetCellValue("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 9);
                SetCellValue("LWP", sheet1, xlsRow, ref xlsCol, out ColLWP, 9);
                SetCellValue("Extra Absent", sheet1, xlsRow, ref xlsCol, out ColExtraAbsent, 9);
                SetCellValue("Holiday", sheet1, xlsRow, ref xlsCol, out ColHlDy, 9);
                SetCellValue("WeekOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 9);
                SetCellValue("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 11);
                SetCellValue("Maternity Leave", sheet1, xlsRow, ref xlsCol, out ColMLv, 20);
                SetCellValue("Structured Basic", sheet1, xlsRow, ref xlsCol, out colBasic, 11);
                SetCellValue("Structured Gross", sheet1, xlsRow, ref xlsCol, out colGross, 11);
                SetCellValue("Structured CTC", sheet1, xlsRow, ref xlsCol, out colCTC, 11);
                SetCellValue("Total Ot Hr", sheet1, xlsRow, ref xlsCol, out ColTotalOTHR, 11);
                endGenericColumn = xlsCol;

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColTotalOTHR].Merge();
                //xlsCol += 1;
                ColGrs = ColTotalOTHR;
                // 9

                int _count_earning_head = 0;
                int _count_earning_ctchead = 0;
                int _count_deducting_head = 0;
                int _total_head_count = 0;
                int _count_earning_notionalhead = 0;
                double totalBankPayDisbusmentAmount = 0.00;
                double totalCashPayDisbusmentAmount = 0.00;

                Dictionary<string, SalaryHeadSequence> shtList = null;

                CreateDynamicSHead(dtSalaryHeadSheet, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out _count_earning_notionalhead, out shtList);




                List<SalaryHeadSequence> salList = new List<SalaryHeadSequence>();
                salList.AddRange(shtList.Values);

                xlsCol--;

                //Header Col
                if (_count_earning_ctchead > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning head";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                }

                var ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction head";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                npstruct = 0;
                //int endxlsCol = 0;
                if (shtList.Count > 0)
                {
                    xlsCol++;
                    //npstruct = ColGrs + shtList.Count + 1;
                    npstruct = ds + _count_deducting_head;
                    //sheet1.Range[xlsRow + 1, npstruct].Text = "Net Payable";
                }
                endXlsCol = npstruct + _count_earning_notionalhead - 1;

                int colBankPaymentPercentage = 0;
                int colCashPaymentPercentage = 0;

                DataTable dtbankCash = _sqlRepository.GetDataTable("SELECT * FROM EmployeeWiseBankCashAmount WHERE PlantId in (" + plantId + ") AND MonthNo = '" + month + @"' AND YearNo  ='" + year + @"'");


                if (dtbankCash.Rows.Count > 0)
                {
                    xlsCol++;

                    colBankPaymentPercentage = npstruct + 1;
                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage].Text = "Bank";
                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage].ColumnWidth = 10;
                    sheet1.Range[xlsRow + 1, colBankPaymentPercentage].CellStyle.Font.Size = 8;
                    xlsCol++;
                    colCashPaymentPercentage = colBankPaymentPercentage + 1;
                    sheet1.Range[xlsRow + 1, colCashPaymentPercentage].Text = "Cash";
                    sheet1.Range[xlsRow + 1, colCashPaymentPercentage].ColumnWidth = 10;
                    sheet1.Range[xlsRow + 1, colCashPaymentPercentage].CellStyle.Font.Size = 8;

                    endXlsCol = colCashPaymentPercentage;
                }

                endXlsCol++;
                int colGWRDailyExtraOTweekoffOT = endXlsCol;
                sheet1.Range[xlsRow + 1, colGWRDailyExtraOTweekoffOT].Text = "GWR (Daily Extra OT & week off OT)";
                sheet1.Range[xlsRow + 1, colGWRDailyExtraOTweekoffOT].ColumnWidth = 10;
                sheet1.Range[xlsRow + 1, colGWRDailyExtraOTweekoffOT].CellStyle.Font.Size = 8;
                endXlsCol++;
                int colHolidayOT = endXlsCol;
                sheet1.Range[xlsRow + 1, colHolidayOT].Text = "Holiday OT";
                sheet1.Range[xlsRow + 1, colHolidayOT].ColumnWidth = 10;
                sheet1.Range[xlsRow + 1, colHolidayOT].CellStyle.Font.Size = 8;
                endXlsCol++;
                int colExtraOT = endXlsCol;
                sheet1.Range[xlsRow + 1, colExtraOT].Text = "Week Day(OT Hrs)";
                sheet1.Range[xlsRow + 1, colExtraOT].ColumnWidth = 10;
                sheet1.Range[xlsRow + 1, colExtraOT].CellStyle.Font.Size = 8;
                endXlsCol++;
                int colWeekOff = endXlsCol;
                sheet1.Range[xlsRow + 1, colWeekOff].Text = "WeekOff(OT Hrs)";
                sheet1.Range[xlsRow + 1, colWeekOff].ColumnWidth = 10;
                sheet1.Range[xlsRow + 1, colWeekOff].CellStyle.Font.Size = 8;
                endXlsCol++;
                int colTotalCTC = endXlsCol;
                sheet1.Range[xlsRow + 1, colTotalCTC].Text = "Total CTC";
                sheet1.Range[xlsRow + 1, colTotalCTC].ColumnWidth = 10;
                sheet1.Range[xlsRow + 1, colTotalCTC].CellStyle.Font.Size = 8;

                xlsCol++;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No.";
                sheet1.Range[xlsRow - 1, 1].ColumnWidth = 14;
                sheet1.Range[xlsRow - 1, 1, xlsRow - 1, 3].Merge();
                sheet1.Range[xlsRow, 1, xlsRow + 1, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow - 1, 1, xlsRow + 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //endXlsCol = endxlsCol;


                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.CompanyId = companyId;

                string FactoryAddress = string.Empty;
                try
                {

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
                catch (Exception ex)
                {
                }


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
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------
                var SrNo = 0;
                var x = "";

                var oRU = new ReportUtility();

                xlsRow = RowIndex;

                xlsRow--;

                double FOT = 0.00;
                double HolidayOT = 0.00;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    Double ExtraOT = 0.00;
                    Double ExtraOTH = 0.00;
                    Double ExtraOTW = 0.00;

                    #region EmpInfo
                    try
                    {
                        SrNo += 1;
                        x = dtEmployees.Rows[i]["EmpSystemID"].ToString().Trim();

                        FOT = 0.00;
                        HolidayOT = 0.00;

                        if (dicHourlyOTW.ContainsKey(x))
                        {
                            FOT = clsStaticInfo.dbl(dicHourlyOTW[x]["Duration"].ToString());
                            ExtraOTW = clsStaticInfo.dbl(dicHourlyOTW[x]["DurationH"].ToString());
                        }
                        if (dicHourlyOTH.ContainsKey(x))
                        {
                            HolidayOT = clsStaticInfo.dbl(dicHourlyOTH[x]["Duration"].ToString());
                            if (dicH.ContainsKey(x))
                            {
                                ExtraOTH = clsStaticInfo.dbl(dicHourlyOTH[x]["DurationH"].ToString());
                                sheet1.Range[xlsRow, colHolidayOT].Number = clsStaticInfo.dbl(dicH[x]) * (clsStaticInfo.dbl(dicHourlyOTH[x]["DurationH"].ToString()));

                                sheet1.Range[xlsRow, colHolidayOT].NumberFormat = NumberFormatTwoDecimal;
                            }
                        }

                        if (dicW.ContainsKey(x))
                        {
                            FOT = (FOT / 60) * dicW[x];
                        }
                        double TotalOTAmount = FOT;
                        if (dicNW.ContainsKey(x))
                        {
                            if (dicHourlyOTNW.ContainsKey(x))
                            {

                                ExtraOTH = clsStaticInfo.dbl(dicHourlyOTNW[x]["DurationH"].ToString());
                                TotalOTAmount += clsStaticInfo.dbl(dicNW[x]) * (clsStaticInfo.dbl(dicHourlyOTNW[x]["DurationH"].ToString()));

                            }
                        }
                        sheet1.Range[xlsRow, colGWRDailyExtraOTweekoffOT].Number = TotalOTAmount;
                        sheet1.Range[xlsRow, colGWRDailyExtraOTweekoffOT].NumberFormat = NumberFormatTwoDecimal;



                        //10
                        sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //3
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PlantName"].ToString()) == false)
                            sheet1.Range[xlsRow, ColPlantName].Text = dtEmployees.Rows[i]["PlantName"].ToString();
                        sheet1.Range[xlsRow, ColPlantName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColPlantName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //4
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                            sheet1.Range[xlsRow, ColDOS].Text = dtEmployees.Rows[i]["DOS"].ToString();
                        sheet1.Range[xlsRow, ColDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpCurrentStat].Text = dtEmployees.Rows[i]["EmployeeCurrentStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpCurrentStat].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCurrentStat].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeStatus"].ToString()) == false)
                            sheet1.Range[xlsRow, colEmpStatus].Text = dtEmployees.Rows[i]["EmployeeStatus"].ToString();
                        sheet1.Range[xlsRow, colEmpStatus].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LegalDesignation"].ToString()) == false)
                            sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["LegalDesignation"].ToString();
                        sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmpCategoryName"].ToString()) == false)// EmployeeCategory Need to Make Correct
                            sheet1.Range[xlsRow, colEmpCategory].Text = dtEmployees.Rows[i]["EmpCategoryName"].ToString();
                        sheet1.Range[xlsRow, colEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        //4.2
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DepartmentName"].ToString()) == false)
                            sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["DepartmentName"].ToString();
                        sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSec].Text = dtEmployees.Rows[i]["SectionName"].ToString();
                        sheet1.Range[xlsRow, cSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSec].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SubSectionName"].ToString()) == false)
                            sheet1.Range[xlsRow, cSubSec].Text = dtEmployees.Rows[i]["SubSectionName"].ToString();
                        sheet1.Range[xlsRow, cSubSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cSubSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["UnitName"].ToString()) == false)
                            sheet1.Range[xlsRow, cUnit].Text = dtEmployees.Rows[i]["UnitName"].ToString();
                        sheet1.Range[xlsRow, cUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PaymentMode"].ToString()) == false)
                            sheet1.Range[xlsRow, cPaymentMode].Text = dtEmployees.Rows[i]["PaymentMode"].ToString();
                        sheet1.Range[xlsRow, cPaymentMode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPaymentMode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["JobLocation"].ToString()) == false)
                            sheet1.Range[xlsRow, cJobLocation].Text = dtEmployees.Rows[i]["JobLocation"].ToString();
                        sheet1.Range[xlsRow, cJobLocation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cJobLocation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["LineName"].ToString()) == false)
                            sheet1.Range[xlsRow, cLine].Text = dtEmployees.Rows[i]["LineName"].ToString();
                        sheet1.Range[xlsRow, cLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["PayRollGroup"].ToString()) == false)
                            sheet1.Range[xlsRow, cPayrollGroup].Text = dtEmployees.Rows[i]["PayRollGroup"].ToString();
                        sheet1.Range[xlsRow, cPayrollGroup].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cPayrollGroup].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        //5
                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GradeCode"].ToString()) == false)
                            sheet1.Range[xlsRow, cGrade].Text = dtEmployees.Rows[i]["GradeCode"].ToString();
                        sheet1.Range[xlsRow, cGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankName"].ToString()) == false)
                            sheet1.Range[xlsRow, colBank].Text = dtEmployees.Rows[i]["BankName"].ToString();
                        sheet1.Range[xlsRow, colBank].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBank].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["BankAccNo"].ToString()) == false)
                            sheet1.Range[xlsRow, colBankAccountNo].Text = dtEmployees.Rows[i]["BankAccNo"].ToString();
                        sheet1.Range[xlsRow, colBankAccountNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colBankAccountNo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DirectManpowerCost"].ToString()) == false)
                            sheet1.Range[xlsRow, colDirectManpowerCost].Text = dtEmployees.Rows[i]["DirectManpowerCost"].ToString();
                        sheet1.Range[xlsRow, colDirectManpowerCost].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, colDirectManpowerCost].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Gender"].ToString()) == false)
                            sheet1.Range[xlsRow, cGender].Text = dtEmployees.Rows[i]["Gender"].ToString();
                        sheet1.Range[xlsRow, cGender].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, cGender].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        #endregion
                        #region Attendance Data
                        double _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpSystemID"].ToString() + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        var payDays = 0.00;// clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        if (!String.IsNullOrEmpty(dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                        {
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());

                            }
                            if (dtEmployees.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                            {
                                payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString());
                            }
                        }
                        else
                        {
                            payDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString());
                        }

                        SetCellTextAttdn(sheet1, xlsRow, colPayDays, payDays);
                        SetCellTextAttdn(sheet1, xlsRow, ColPdDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalPresent"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLate, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLate"].ToString()));
                        SetCellTextNumber(sheet1, xlsRow, ColAbDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLWP, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLWP"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColExtraAbsent, _ExtraAbsent);
                        SetCellTextAttdn(sheet1, xlsRow, ColHlDy, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalHoliDay"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColWkOf, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalWeekOff"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColMLv, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalMLv"].ToString()));
                        SetCellTextAttdn(sheet1, xlsRow, ColTotalOTHR, clsStaticInfo.dbl(dtEmployees.Rows[i]["TotalOTHr"].ToString()) / 60);

                        double WorkingDays = clsStaticInfo.dbl(dtEmployees.Rows[i]["WorkDays"].ToString());
                        if (Convert.ToDateTime(dtEmployees.Rows[i]["DOJ"].ToString()) > Convert.ToDateTime(FromDate))
                        {

                            if (dicEmployeeWiseWeekoff.ContainsKey(dtEmployees.Rows[i]["EmpSystemId"].ToString()))
                            {
                                string _dayName = dicEmployeeWiseWeekoff[dtEmployees.Rows[i]["EmpSystemId"].ToString()];
                                foreach (var WeekOffs in DicWeekOffCount)
                                {
                                    if (WeekOffs.Key.ToUpper() == _dayName.ToUpper())
                                    {
                                        WorkingDays = daysInMonth - WeekOffs.Value;
                                        break;
                                    }
                                }
                            }

                        }

                        SetCellTextAttdn(sheet1, xlsRow, colWorkingDays, WorkingDays);
                        //ExtraOT = ExtraOTH + ExtraOTW;
                        SetCellTextAttdn(sheet1, xlsRow, colExtraOT, ExtraOTH);
                        SetCellTextAttdn(sheet1, xlsRow, colWeekOff, ExtraOTW);

                        //}
                        #endregion

                        #region ------------------------------------Salary Sheet----------------------------------
                        if (dicEmpSalry.ContainsKey(dtEmployees.Rows[i]["EmpSystemID"].ToString()))
                        {
                            List<DataRow> drSalaryHeadCollection = dicEmpSalry[dtEmployees.Rows[i]["EmpSystemID"].ToString()];
                            if (drSalaryHeadCollection.Count > 0)
                            {
                                //for (int ix = 0; ix < listdsSlrStr.Count; ix++)
                                //{
                                //    if (listdsSlrStr[ix].HeadCategory == "Basic" )
                                //    {
                                //        sheet1.Range[xlsRow, colBasic].Number = Convert.ToDouble(listdsSlrStr[ix].EntryAmount);
                                //        sheet1.Range[xlsRow, colBasic].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                //        sheet1.Range[xlsRow, colBasic].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                //    }
                                //    if (listdsSlrStr[ix].HeadCategory == "CTC" )
                                //    {
                                //        sheet1.Range[xlsRow, colCTC].Number = Convert.ToDouble(listdsSlrStr[ix].EntryAmount);
                                //        sheet1.Range[xlsRow, colCTC].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                //        sheet1.Range[xlsRow, colCTC].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                //    }
                                //    if (listdsSlrStr[ix].HeadCategory == "GROSS" )
                                //    {
                                //        sheet1.Range[xlsRow, colGross].Number = Convert.ToDouble(listdsSlrStr[ix].EntryAmount);
                                //        sheet1.Range[xlsRow, colGross].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                //        sheet1.Range[xlsRow, colGross].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                //    }

                                //}
                                for (int CI = 0; CI < drSalaryHeadCollection.Count; CI++)
                                {

                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "BASIC")
                                    {
                                        sheet1.Range[xlsRow, colBasic].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["EntryAmount"].ToString());
                                        sheet1.Range[xlsRow, colBasic].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                        sheet1.Range[xlsRow, colBasic].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    }
                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "CTC")
                                    {
                                        sheet1.Range[xlsRow, colCTC].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["EntryAmount"].ToString());
                                        sheet1.Range[xlsRow, colCTC].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                        sheet1.Range[xlsRow, colCTC].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    }
                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "GROSS")
                                    {
                                        sheet1.Range[xlsRow, colGross].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["EntryAmount"].ToString());
                                        sheet1.Range[xlsRow, colGross].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                        sheet1.Range[xlsRow, colGross].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    }

                                    if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "NET PAYABLE")
                                    {
                                        SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                        if (xx != null)
                                        {
                                            sheet1.Range[xlsRow, xx.XLColIndex].Number = Convert.ToDouble(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            continue;
                                        }
                                    }

                                    try
                                    {
                                        SalaryHeadSequence xx = shtList[drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()];// shtList.Where(ee => ee.SalaryHeadId == drSalaryHeadCollection[CI]["SalaryHeadId"].ToString()).ToList();
                                        if (xx != null)
                                        {
                                            if (drSalaryHeadCollection[CI]["HeadCategory"].ToString().ToUpper() == "CTC")
                                            {
                                                colCtc = xx.XLColIndex;
                                            }
                                            if (drSalaryHeadCollection[CI]["HeadType"].ToString() == "D")
                                            {
                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString()) * (-1);
                                            }

                                            else
                                            {

                                                sheet1.Range[xlsRow, xx.XLColIndex].Number = clsStaticInfo.dbl(drSalaryHeadCollection[CI]["DisbusmentAmount"].ToString());
                                            }

                                            sheet1.Range[xlsRow, xx.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                            sheet1.Range[xlsRow, xx.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                            sheet1.Range[xlsRow, xx.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        throw ex;
                                    }
                                    if (dtbankCash.Rows.Count > 0)
                                    {
                                        dtbankCash.DefaultView.RowFilter = "EmpSystemId = '" + dtEmployees.Rows[i]["EmpsystemId"].ToString() + @"'";

                                        if (dtbankCash.DefaultView.Count > 0)
                                        {
                                            sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].Number = Convert.ToDouble(dtbankCash.DefaultView[0]["BankAmount"].ToString());
                                            sheet1.Range[xlsRow, colBankPaymentPercentage, xlsRow, colBankPaymentPercentage].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[CI]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[CI]["DecimalNo"].ToString()), "");
                                            sheet1.Range[xlsRow, colBankPaymentPercentage].CellStyle.Font.Size = 34;

                                            sheet1.Range[xlsRow, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].Number = Convert.ToDouble(dtbankCash.DefaultView[0]["CashAmount"].ToString());
                                            sheet1.Range[xlsRow, colCashPaymentPercentage, xlsRow, colCashPaymentPercentage].NumberFormat = ru.GetDecimalFormatlocalNetPay(Convert.ToBoolean(drSalaryHeadCollection[CI]["IntegerInDisb"].ToString()), Convert.ToInt32(drSalaryHeadCollection[CI]["DecimalNo"].ToString()), "");
                                            sheet1.Range[xlsRow, colCashPaymentPercentage].CellStyle.Font.Size = 34;

                                            totalBankPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["BankAmount"].ToString());
                                            totalCashPayDisbusmentAmount += clsStaticInfo.dbl(dtbankCash.DefaultView[0]["CashAmount"].ToString());
                                        }

                                    }

                                }

                            }
                        }

                        sheet1.Range[xlsRow, colTotalCTC].Formula = clsStaticInfo.GetxlsCol(colGWRDailyExtraOTweekoffOT) + xlsRow + "+" + clsStaticInfo.GetxlsCol(colHolidayOT) + xlsRow + "+" + clsStaticInfo.GetxlsCol(colCtc) + xlsRow;

                        sheet1.Range[xlsRow, colTotalCTC].NumberFormat = NumberFormatTwoDecimal;

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }


                    #endregion

                    xlsRow++;
                }//for emp count
                int sheetEndXlsRow = xlsRow - 1;
                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                var freezePan = RowIndex - 1;
                sheet1.UsedRange["A" + freezePan].FreezePanes();
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
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "EmpSalaryInfo";
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";

                #endregion

                workbook.Version = ExcelVersion.Excel2016;

                if (isTopSheet == true)
                {
                    #region Salary Summary
                    string filePath = HostingEnvironment.MapPath("~/") + "TempSalaeySummary.xlsx";
                    workbook.SaveAs(filePath);
                    workbook = application.Workbooks.Open(filePath);

                    IWorksheet worksheet = workbook.Worksheets[0];
                    worksheet.Move(4);

                    #region PivotSheet 1 EmployeeStatus, PaymentMode, Department   
                    IWorksheet pivotSheet = workbook.Worksheets[0];
                    pivotSheet.Name = "Summary 1";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet.GetColumnWidth(1) + pivotSheet.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet.GetRowHeight(1) + pivotSheet.GetRowHeight(2) + pivotSheet.GetRowHeight(3) + pivotSheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    // pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;
                    int tableColRange = 0;
                    if (dtbankCash.Rows.Count > 0)
                    {
                        tableColRange = colCashPaymentPercentage;
                    }
                    else
                    {
                        tableColRange = npstruct;
                    }

                    IRange iRange = worksheet["A7:" + clsStaticInfo.GetxlsCol(tableColRange) + (sheetEndXlsRow)];
                    IPivotCache cache2 = workbook.PivotCaches.Add(iRange);
                    IPivotCache cache = workbook.PivotCaches.Add(iRange);


                    #region Second Pivot table
                    pivotSheet.Range[xlsRow + 2, 1].Text = "EmployeeStatus, PaymentMode, Department Wise Salary Summary";
                    pivotSheet.Range[xlsRow + 2, 1, xlsRow + 2, 5].Merge();
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;

                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                    pivotTable2.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cPaymentMode - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[cSec - 1].Axis = PivotAxisTypes.Row;


                    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable2_1.Options.ShowDrillIndicators = false;

                    pivotTable2_1.DisplayFieldCaptions = true;

                    //Add data field
                    IPivotField field2 = pivotTable2_1.Fields[ColSr - 1];
                    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                    int pivotColumnCount = 0;
                    IPivotField fieldGross = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                            }
                            try
                            {
                                if (ob.HeadType == "D")
                                {
                                    pivotColumnCount++;
                                    fieldGross = pivotTable2_1.Fields[ob.XLColIndex - 1];
                                    pivotTable2_1.DataFields.Add(fieldGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                        }
                    }
                    try
                    {
                        fieldGross = null;
                        pivotColumnCount++;
                        fieldGross = pivotTable2_1.Fields[npstruct - 1];
                        pivotTable2_1.DataFields.Add(fieldGross, "Net Payable", PivotSubtotalTypes.Sum);
                        fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                        if (dtbankCash.Rows.Count > 0)
                        {
                            fieldGross = null;
                            pivotColumnCount++;
                            fieldGross = pivotTable2_1.Fields[colBankPaymentPercentage - 1];
                            pivotTable2_1.DataFields.Add(fieldGross, "Employee", PivotSubtotalTypes.Count);
                            fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                            pivotColumnCount++;
                            fieldGross = pivotTable2_1.Fields[colBankPaymentPercentage - 1];
                            pivotTable2_1.DataFields.Add(fieldGross, "Bank", PivotSubtotalTypes.Sum);
                            fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                            pivotColumnCount++;
                            fieldGross = pivotTable2_1.Fields[colCashPaymentPercentage - 1];
                            pivotTable2_1.DataFields.Add(fieldGross, "Cash", PivotSubtotalTypes.Sum);
                            fieldGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                        }

                    }
                    catch (Exception)
                    {

                    }

                    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    int totalColumns = pivotTable2_1.RowFields.Count + pivotColumnCount;

                    int lastCloumn = totalColumns + 2;

                    #endregion


                    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet.IsGridLinesVisible = false;
                    pivotSheet.IsDisplayZeros = false;

                    pivotSheet.UsedRange.WrapText = false;
                    pivotSheet.PageSetup.TopMargin = 0.5;
                    pivotSheet.PageSetup.BottomMargin = 0.7;
                    pivotSheet.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    pivotSheet.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    pivotSheet.PageSetup.LeftMargin = 0.5;
                    pivotSheet.PageSetup.RightMargin = 0.2;
                    pivotSheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    pivotSheet.PageSetup.FitToPagesTall = 0;
                    pivotSheet.PageSetup.FitToPagesWide = 1;
                    pivotSheet.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;

                    #endregion


                    #region PivotSheet 2 Employee Category  No 
                    IWorksheet pivotSheet2EmpC = workbook.Worksheets[1];
                    pivotSheet2EmpC.Name = "Summary 2";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet2EmpC.GetColumnWidth(1) + pivotSheet2EmpC.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet2EmpC.GetRowHeight(1) + pivotSheet2EmpC.GetRowHeight(2) + pivotSheet2EmpC.GetRowHeight(3) + pivotSheet2EmpC.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet2EmpC.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet2EmpC.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet2EmpC.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;

                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet2EmpC.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet2EmpC.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet2EmpC.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //pivotSheet2EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet2EmpC.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet2EmpC.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet2EmpC.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                    #region Second Pivot table

                    lastCloumn = 1;
                    pivotSheet2EmpC.Range[xlsRow + 2, lastCloumn].Text = "Employee Category Wise Salary Summary";
                    pivotSheet2EmpC.Range[xlsRow + 2, lastCloumn].CellStyle.Font.Size = 12;
                    pivotSheet2EmpC.Range[xlsRow + 2, lastCloumn, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet2EmpC.Range[xlsRow + 2, lastCloumn].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable = pivotSheet2EmpC.PivotTables.Add("PivotTable1", pivotSheet2EmpC["A8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[cSec - 1].Axis = PivotAxisTypes.Row;


                    IPivotTable pivotTable1 = pivotSheet2EmpC.PivotTables["PivotTable1"];
                    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable1.Options.ShowDrillIndicators = false;

                    pivotTable1.DisplayFieldCaptions = true;
                    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    //Add data field
                    IPivotField field = pivotTable.Fields[ColSr - 1];
                    pivotTable.DataFields.Add(field, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot2ColumnCount = 0;
                    IPivotField fieldGross2 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross2 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                    pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross2 = pivotTable.Fields[ob.XLColIndex - 1];
                                pivotTable.DataFields.Add(fieldGross2, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross2 = null;
                    pivot2ColumnCount++;
                    fieldGross2 = pivotTable.Fields[npstruct - 1];
                    pivotTable.DataFields.Add(fieldGross2, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    if (dtbankCash.Rows.Count > 0)
                    {
                        fieldGross2 = null;
                        pivotColumnCount++;
                        fieldGross2 = pivotTable.Fields[colBankPaymentPercentage - 1];
                        pivotTable.DataFields.Add(fieldGross2, "Employee Bank", PivotSubtotalTypes.Count);
                        fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                        pivotColumnCount++;
                        fieldGross2 = pivotTable.Fields[colBankPaymentPercentage - 1];
                        pivotTable.DataFields.Add(fieldGross2, "Bank", PivotSubtotalTypes.Sum);
                        fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                        pivotColumnCount++;
                        fieldGross2 = pivotTable.Fields[colCashPaymentPercentage - 1];
                        pivotTable.DataFields.Add(fieldGross2, "Cash", PivotSubtotalTypes.Sum);
                        fieldGross2.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    }




                    pivotTable.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;
                    totalColumns = 0;
                    totalColumns = pivotTable.RowFields.Count + pivotColumnCount;
                    //lastCloumn = 0;
                    //lastCloumn = totalColumns + 2;

                    #endregion

                    pivotSheet2EmpC.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet2EmpC.IsGridLinesVisible = false;
                    pivotSheet2EmpC.IsDisplayZeros = false;

                    pivotSheet2EmpC.UsedRange.WrapText = false;
                    pivotSheet2EmpC.PageSetup.TopMargin = 0.5;
                    pivotSheet2EmpC.PageSetup.BottomMargin = 0.7;
                    pivotSheet2EmpC.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    pivotSheet2EmpC.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    pivotSheet2EmpC.PageSetup.LeftMargin = 0.5;
                    pivotSheet2EmpC.PageSetup.RightMargin = 0.2;
                    pivotSheet2EmpC.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    pivotSheet2EmpC.PageSetup.FitToPagesTall = 0;
                    pivotSheet2EmpC.PageSetup.FitToPagesWide = 1;
                    pivotSheet2EmpC.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;
                    #endregion


                    #region PivotSheet 3 EmployeeStatus ,Employee Category and Department
                    IWorksheet pivotSheet3EmpC = workbook.Worksheets[2];
                    pivotSheet3EmpC.Name = "Summary 3";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet3EmpC.GetColumnWidth(1) + pivotSheet3EmpC.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet3EmpC.GetRowHeight(1) + pivotSheet3EmpC.GetRowHeight(2) + pivotSheet3EmpC.GetRowHeight(3) + pivotSheet3EmpC.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet3EmpC.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet3EmpC.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet3EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet3EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet3EmpC.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet3EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet3EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet3EmpC.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet3EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet3EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;
                    pivotSheet3EmpC.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet3EmpC.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet3EmpC.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet3EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet3EmpC.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    #endregion

                    pivotSheet3EmpC.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet3EmpC.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet3EmpC.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                    #region Second Pivot table

                    totalColumns += pivotTable.RowFields.Count + pivot2ColumnCount;

                    lastCloumn = 1;

                    pivotSheet3EmpC.Range[xlsRow + 2, lastCloumn].Text = "EmployeeStatus ,Employee Category and Department Wise  Salary Summary";
                    pivotSheet3EmpC.Range[xlsRow + 2, lastCloumn].CellStyle.Font.Size = 12;
                    pivotSheet3EmpC.Range[xlsRow + 2, lastCloumn, xlsRow + 2, lastCloumn + 5].Merge();
                    pivotSheet3EmpC.Range[xlsRow + 2, lastCloumn].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable3 = pivotSheet3EmpC.PivotTables.Add("PivotTable13", pivotSheet3EmpC["A8"], cache);

                    pivotTable3.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[cDept - 1].Axis = PivotAxisTypes.Row;
                    pivotTable3.Fields[colEmpCategory - 1].Axis = PivotAxisTypes.Row;

                    IPivotTable pivotTable13_1 = pivotSheet3EmpC.PivotTables["PivotTable13"];
                    pivotTable13_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable13_1.Options.ShowDrillIndicators = false;

                    pivotTable13_1.DisplayFieldCaptions = true;
                    pivotTable13_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;

                    IPivotField fields3 = pivotTable13_1.Fields[ColSr - 1];
                    pivotTable13_1.DataFields.Add(fields3, "Total Employee", PivotSubtotalTypes.Count);

                    int pivot3ColumnCount = 0;
                    IPivotField fieldGross3 = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldGross3 = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivot3ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }

                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivot2ColumnCount++;
                                    fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                    pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            if (ob.HeadType == "D")
                            {
                                pivot2ColumnCount++;
                                fieldGross3 = pivotTable13_1.Fields[ob.XLColIndex - 1];
                                pivotTable13_1.DataFields.Add(fieldGross3, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                            }

                        }
                    }
                    fieldGross3 = null;
                    pivot2ColumnCount++;
                    fieldGross3 = pivotTable13_1.Fields[npstruct - 1];
                    pivotTable13_1.DataFields.Add(fieldGross3, "Net Payable", PivotSubtotalTypes.Sum);
                    fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    if (dtbankCash.Rows.Count > 0)
                    {
                        fieldGross3 = null;
                        pivotColumnCount++;
                        fieldGross3 = pivotTable13_1.Fields[colBankPaymentPercentage - 1];
                        pivotTable13_1.DataFields.Add(fieldGross3, "Employee Bank", PivotSubtotalTypes.Count);
                        fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                        pivotColumnCount++;
                        fieldGross3 = pivotTable13_1.Fields[colBankPaymentPercentage - 1];
                        pivotTable13_1.DataFields.Add(fieldGross3, "Bank", PivotSubtotalTypes.Sum);
                        fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                        pivotColumnCount++;
                        fieldGross3 = pivotTable13_1.Fields[colCashPaymentPercentage - 1];
                        pivotTable13_1.DataFields.Add(fieldGross3, "Cash", PivotSubtotalTypes.Sum);
                        fieldGross3.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                    }
                    #endregion

                    pivotSheet3EmpC.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet3EmpC.IsGridLinesVisible = false;
                    pivotSheet3EmpC.IsDisplayZeros = false;

                    pivotSheet3EmpC.UsedRange.WrapText = false;
                    pivotSheet3EmpC.PageSetup.TopMargin = 0.5;
                    pivotSheet3EmpC.PageSetup.BottomMargin = 0.7;
                    pivotSheet3EmpC.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    pivotSheet3EmpC.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    pivotSheet3EmpC.PageSetup.LeftMargin = 0.5;
                    pivotSheet3EmpC.PageSetup.RightMargin = 0.2;
                    pivotSheet3EmpC.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    pivotSheet3EmpC.PageSetup.FitToPagesTall = 0;
                    pivotSheet3EmpC.PageSetup.FitToPagesWide = 1;
                    pivotSheet3EmpC.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;
                    #endregion




                    #region PivotSheet 4 bank Sheet No 4
                    IWorksheet pivotSheet2 = workbook.Worksheets[3];
                    pivotSheet2.Name = "Bank Summary";

                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet2.GetColumnWidth(1) + pivotSheet2.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet2.GetRowHeight(1) + pivotSheet2.GetRowHeight(2) + pivotSheet2.GetRowHeight(3) + pivotSheet2.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet2.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet2.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet2.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;
                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet2.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;

                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;
                    pivotSheet2.Range[xlsRow, 3].Text = "Salary Summary for the month of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                    pivotSheet2EmpC.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignCenterAcrossSelection;

                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    #endregion

                    pivotSheet2.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet2.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet2.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;
                    //tableColRange = 0;
                    //if (dtbankCash.Rows.Count > 0)
                    //{
                    //    tableColRange = colCashPaymentPercentage;
                    //}
                    //else
                    //{
                    //    tableColRange = npstruct;
                    //}

                    ///IRange iRange = worksheet["A7:" + clsStaticInfo.GetxlsCol(tableColRange) + (sheetEndXlsRow)];
                    //IPivotCache cache2 = workbook.PivotCaches.Add(iRange);
                    //IPivotCache cache = workbook.PivotCaches.Add(iRange);


                    #region Second Pivot table
                    pivotSheet2.Range[xlsRow + 2, 1].Text = "EmployeeStatus, PaymentMode, Bank Wise Salary Summary";
                    pivotSheet2.Range[xlsRow + 2, 1, xlsRow + 2, 5].Merge();
                    pivotSheet2.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;

                    pivotSheet2.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTableBank = pivotSheet2.PivotTables.Add("PivotTableBank", pivotSheet2["A8"], cache);

                    pivotTableBank.Fields[colEmpStatus - 1].Axis = PivotAxisTypes.Row;
                    pivotTableBank.Fields[cPaymentMode - 1].Axis = PivotAxisTypes.Row;
                    pivotTableBank.Fields[colBank - 1].Axis = PivotAxisTypes.Row;
                    //pivotTableBank.Fields[cSec - 1].Axis = PivotAxisTypes.Row;


                    IPivotTable pivotTableBank_1 = pivotSheet2.PivotTables["PivotTableBank"];
                    pivotTableBank_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTableBank_1.Options.ShowDrillIndicators = false;

                    pivotTableBank_1.DisplayFieldCaptions = true;

                    //Add data field
                    IPivotField fieldbank = pivotTableBank_1.Fields[ColSr - 1];
                    pivotTableBank_1.DataFields.Add(fieldbank, "Total Employees", PivotSubtotalTypes.Count);
                    pivotColumnCount = 0;
                    IPivotField fieldBankGross = null;
                    for (int i = 0; i < salList.Count; i++)
                    {
                        var ob = salList[i];
                        fieldBankGross = null;
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.HeadType == "E")
                            {
                                if (ob.SalaryHead.ToUpper() == "GROSS")
                                {
                                    pivotColumnCount++;
                                    fieldBankGross = pivotTableBank_1.Fields[ob.XLColIndex - 1];
                                    pivotTableBank_1.DataFields.Add(fieldBankGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (!ob.IsGrossComponent && ob.IsNetPayEffect)
                                {
                                    pivotColumnCount++;
                                    fieldBankGross = pivotTableBank_1.Fields[ob.XLColIndex - 1];
                                    pivotTableBank_1.DataFields.Add(fieldBankGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                                if (ob.SalaryHead.ToUpper() == "CTC")
                                {
                                    pivotColumnCount++;
                                    fieldBankGross = pivotTableBank_1.Fields[ob.XLColIndex - 1];
                                    pivotTableBank_1.DataFields.Add(fieldBankGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");
                                }
                            }
                            try
                            {
                                if (ob.HeadType == "D")
                                {
                                    pivotColumnCount++;
                                    fieldBankGross = pivotTableBank_1.Fields[ob.XLColIndex - 1];
                                    pivotTableBank_1.DataFields.Add(fieldBankGross, ob.SalaryHead, PivotSubtotalTypes.Sum);
                                    fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(ob.DecimalNo, "");

                                }
                            }
                            catch (Exception ex)
                            {

                                //throw ex;
                            }

                        }
                    }
                    try
                    {
                        fieldBankGross = null;
                        pivotColumnCount++;
                        fieldBankGross = pivotTableBank_1.Fields[npstruct - 1];
                        pivotTableBank_1.DataFields.Add(fieldBankGross, "Net Payable", PivotSubtotalTypes.Sum);
                        fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                        if (dtbankCash.Rows.Count > 0)
                        {
                            fieldBankGross = null;
                            pivotColumnCount++;
                            fieldBankGross = pivotTableBank_1.Fields[colBankPaymentPercentage - 1];
                            pivotTableBank_1.DataFields.Add(fieldBankGross, "Employee", PivotSubtotalTypes.Count);
                            fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                            pivotColumnCount++;
                            fieldBankGross = pivotTableBank_1.Fields[colBankPaymentPercentage - 1];
                            pivotTableBank_1.DataFields.Add(fieldBankGross, "Bank", PivotSubtotalTypes.Sum);
                            fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");
                            pivotColumnCount++;
                            fieldBankGross = pivotTableBank_1.Fields[colCashPaymentPercentage - 1];
                            pivotTableBank_1.DataFields.Add(fieldBankGross, "Cash", PivotSubtotalTypes.Sum);
                            fieldBankGross.NumberFormat = ru.GetDecimalFormatlocal(0, "");

                        }

                    }
                    catch (Exception)
                    {

                    }

                    pivotTableBank_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleLight1;
                    totalColumns = 0;
                    totalColumns = pivotTableBank_1.RowFields.Count + pivotColumnCount;
                    //lastCloumn = 0;
                    //lastCloumn = totalColumns + 2;

                    #endregion

                    pivotSheet2.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet2.IsGridLinesVisible = false;
                    pivotSheet2.IsDisplayZeros = false;

                    pivotSheet2.UsedRange.WrapText = false;
                    pivotSheet2.PageSetup.TopMargin = 0.5;
                    pivotSheet2.PageSetup.BottomMargin = 0.7;
                    pivotSheet2.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    pivotSheet2.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    pivotSheet2.PageSetup.LeftMargin = 0.5;
                    pivotSheet2.PageSetup.RightMargin = 0.2;
                    pivotSheet2.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    pivotSheet2.PageSetup.FitToPagesTall = 0;
                    pivotSheet2.PageSetup.FitToPagesWide = 1;
                    pivotSheet2.PageSetup.PaperSize = ExcelPaperSize.PaperLegal;
                    #endregion
                    #endregion

                    workbook.ActiveSheetIndex = 0;
                }

                return workbook;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objRpt = null;
                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }

        public Dictionary<string, object> GetFinalOTForforSpecincDayType(string DayCategory, string year, string month, string plantId, Dictionary<string, string> parameters)
        {
            try
            {
                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and fot.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                string strSql = @" SELECT Sum(NormalOTHr) NormalOTHr,FOT.EmpSystemID FROM FinalOT FOT 
									 LEFT JOIN AttdnProcessData ap on  FOT.EmpSystemId=ap.EmpSystemID and FOT.WorkDate=ap.WorkDate
									 left join DayType DT ON DT.DayType = ap.DayStatus 								  
									
									WHERE MONTH(fot.WorkDate) = " + month + @" AND YEAR(fot.WorkDate) = " + year + @"   and fot.PlantID ='" + plantId + @"' and dt.Category = '" + DayCategory + @"' " + wcEmpSystemId + @"
									GROUP BY FOT.EmpSystemID";

                return _sqlRepository.GetData(strSql, null);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        public IEnumerable<object> GetEmpInfoDateRange(string companyGroupId, string plantId, string month, string year, bool sa, bool ca, string userId, bool isActive, bool isSeperated, bool isMaternity)
        {
            try
            {
                var wcPayrollGroup = "";

                var salaryProcessColumn = "";
                var strDOJ = "";
                string salaryProcessFlag = "";
                string wcEmpStatus = " Where (1=0 ";
                //string salaryProcessID = "";

                if (sa == true || ca == true)
                {
                    wcPayrollGroup = @"";
                }
                else
                {
                    string inPayrollGroup = "";
                    DataTable dtPayRollGrpEmpId = _sqlRepository.GetDataTable("SELECT employeeid FROM MST.PayrollGroupMaster WHERE PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"') AND PlantID = '" + plantId + @"'");
                    DataTable dtNotPayRollGrpEmpId = _sqlRepository.GetDataTable(@"SELECT SystemId FROM EmployeeInformation E 
                    WHERE SystemId NOT IN (SELECT employeeid from MST.PayrollGroupMaster where PlantID = '" + plantId + @"')  AND E.PlantID = '" + plantId + @"'");

                    if (dtPayRollGrpEmpId.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtPayRollGrpEmpId.Rows.Count; i++)
                        {
                            inPayrollGroup += ",'" + dtPayRollGrpEmpId.Rows[i]["employeeid"].ToString() + "'";
                        }
                        if (dtNotPayRollGrpEmpId.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtNotPayRollGrpEmpId.Rows.Count; i++)
                            {
                                inPayrollGroup += ",'" + dtNotPayRollGrpEmpId.Rows[i]["SystemId"].ToString() + "'";
                            }
                        }
                        wcPayrollGroup = @"AND E.SystemId  IN (" + inPayrollGroup + @")";
                    }
                    else
                    {
                        wcPayrollGroup = @"";
                    }

                    //wcPayrollGroup = @"AND E.SystemId  IN (SELECT employeeid from MST.PayrollGroupMaster where PayrollGroupId IN (SELECT PayrollGroupId FROM SEC.UserPayrollGroup where UserId = '" + userId + @"'))";
                }

                wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }





                wcEmpStatus += ")";

                var cListOId = string.Empty; var cList = string.Empty; ; var cListId = string.Empty; var Join = string.Empty;
                var param = string.Empty;
                if (!string.IsNullOrEmpty(companyGroupId) && !string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "' AND spld.PlantId='" + plantId + "'";
                else if (!string.IsNullOrEmpty(companyGroupId) && string.IsNullOrEmpty(plantId))
                    param = "E.GroupID='" + companyGroupId + "'";

                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'),[isToBeSelect] = Convert(bit, 'False'),* FROM (  SELECT   dISTINCT   
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ld.UserName,'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(eL.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  " + month + @"  AND YEAR(DOS) =" + year + @" then 'Separated' else 'Active' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    " + salaryProcessFlag + @"
                                    " + salaryProcessColumn + @"
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(SPLD.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                     FROM EmployeeInformation e
                                
								    --LEFT OUTER JOIN SalaryProcChild SPC ON SPC.EmpInfoSystemID = E.SystemId AND SPC.PlantID = '" + plantId + @"'
                                   
									  JOIN SalaryProcessLogDetail SPLD ON 
									 --SPLD.SalaryProcessId  IN( '','M-2020337','M-2020338'  )    
									  SPLD.SalaryProcessId=SPM.SlrProcMstSystemID
									 AND SPM.EmpInfoSystemID = SPLD.EmpSystemId 

                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=SPLD.LegalDesignationId
                                   
                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=SPLD.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    
                                    LEFT OUTER JOIN ORG.Line eL on eL.id=mpb.LineId

                                    LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = SPLD.EmployeeCategoryId
			                                       
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    
								    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
									left join [HKP].[BankBranch] bbranch on bbranch.Id = SPLD.BankBranchId
   
                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"                                
                                     ) DD " + wcEmpStatus + @" ORDER BY ISNULL(EmployeeCodePreFix,''),ISNULL(EmployeeCodeNumeric,0)";
                return _sqlRepository.GetDataCollection(cmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }





        void GenerateRate(DataTable DTAllowPolicy, DataSet dsSalaryStruc, string _currencyId, out Dictionary<string, double> dicNW)
        {
            double nwRate = 0;
            dicNW = null;

            try
            {
                DataTable dtemp = new DataView(DTAllowPolicy).ToTable(true, "SystemID");
                dicNW = new Dictionary<string, double>();
                for (int i = 0; i < dtemp.Rows.Count; i++)
                {
                    string _empid = dtemp.Rows[i]["SystemID"].ToString();
                    GetFormulaAllRate(DTAllowPolicy, dsSalaryStruc, _currencyId, _empid, out nwRate);
                    dicNW.Add(_empid, nwRate);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetFormulaAllRate(DataTable dsPolicy, DataSet dsSalaryStruc, string _currencyId, string empid, out double nwRate)
        {
            nwRate = 0;
            string AllDesignation = string.Empty;
            string IsFixed = string.Empty;
            string Rate = string.Empty;
            string FormulaDes = string.Empty;
            string IsFixedFromRate = string.Empty;
            string ratear = string.Empty;
            string FormulaDesIDFromRate = string.Empty;
            try
            {
                DataView dv = new DataView(dsPolicy);
                dv.RowFilter = "SystemID='" + empid + "'";
                if (dv.Count > 0)
                {
                    //"IsAllDesignation", "IsFixed", "Rate", "FormulaDesID", "IsFixedFromRate", "ratear", "FormulaDesIDFromRate", "SystemId"
                    AllDesignation = dv[0]["IsAllDesignation"].ToString();
                    IsFixed = dv[0]["IsFixed"].ToString();
                    Rate = dv[0]["Rate"].ToString();
                    FormulaDes = dv[0]["FormulaDesID"].ToString();
                    IsFixedFromRate = dv[0]["IsFixedFromRate"].ToString();
                    ratear = dv[0]["ratear"].ToString();
                    FormulaDesIDFromRate = dv[0]["FormulaDesIDFromRate"].ToString();
                    string EmployeeCode = dv[0]["EmployeeCode"].ToString();

                    string formula = string.Empty;
                    if (Convert.ToBoolean(AllDesignation.ToString()) == true)
                    {
                        if (Convert.ToBoolean(IsFixed) == true)
                        {
                            nwRate = clsStaticInfo.dbl(Rate);
                        }
                        else
                        {
                            formula = FormulaDes;
                        }
                    }

                    else
                    {
                        if (Convert.ToBoolean(IsFixedFromRate.ToString()) == true)
                        {
                            nwRate = clsStaticInfo.dbl(ratear);
                        }
                        else
                        {
                            formula = FormulaDesIDFromRate;
                        }
                    }

                    if (Convert.ToBoolean(IsFixed) == false && Convert.ToBoolean(IsFixedFromRate) == false)
                    {
                        if (string.IsNullOrEmpty(formula))
                        {
                            throw new Exception("Employee " + EmployeeCode + " has no Rate Formaula in allowance setting ...");
                        }


                        DataView dvss = new DataView(dsSalaryStruc.Tables[0]);
                        dvss.RowFilter = "EmpInfoSystemID='" + empid + "'";
                        if (dvss.Count > 0)
                        {
                            string FormulaValue = string.Empty;
                            DataTable dtValue = dvss.ToTable();
                            DataTable dtSalaryHead = dvss.ToTable(true, "SalaryHeadID", "SalaryHead");

                            GetFormulValue(formula, ref dtValue, _currencyId, out nwRate, ref dtSalaryHead);

                        }
                        else
                        {
                            throw new Exception("Employee " + EmployeeCode + " has no Salary sturcture ...");
                        }
                    }


                }//if dv

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void GenerateDic(Dictionary<string, DataRow> dsPolicy, Dictionary<string, List<DataRow>> dsSalaryStruc, string _currencyId, out Dictionary<string, double> dicNW, out Dictionary<string, double> dicW, out Dictionary<string, double> dicH)
        {
            double nwRate = 0;
            double wRate = 0;
            double hRate = 0;
            dicNW = null;
            dicW = null;
            dicH = null;
            try
            {

                //DataTable dtemp = new DataView(dsSalaryStruc.Tables[0]).ToTable(true, "EmpInfoSystemID");
                //DataTable dtemp = dicSalaryStruc[empid].CopyToDataTable();// .ToDataSet().Tables[0];// dvss.ToTable();
                //DataTable dtemp = dsSalaryStruc[empid].CopyToDataTable().DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead");// dvss.ToTable(true, "SalaryHeadID", "SalaryHead");

                dicNW = new Dictionary<string, double>();
                dicW = new Dictionary<string, double>();
                dicH = new Dictionary<string, double>();

                foreach (var item in dsSalaryStruc)
                {

                    string _empid = item.Key;//dtemp.Rows[i]["EmpInfoSystemID"].ToString();

                    GetFormula(dsPolicy, dsSalaryStruc, _currencyId, _empid, out nwRate, out wRate, out hRate);
                    dicNW.Add(_empid, nwRate);
                    dicW.Add(_empid, wRate);
                    dicH.Add(_empid, hRate);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetFormula(Dictionary<string, DataRow> dicPolicy, Dictionary<string, List<DataRow>> dicSalaryStruc, string _currencyId, string empid, out double nwRate, out double wRate, out double hRate)
        {
            nwRate = 0;
            wRate = 0;
            hRate = 0;
            //out string FormulaDesIDN, out string FormulaDesIDW, out string FormulaDesIDH
            string FormulaDesIDN = string.Empty;
            string FormulaDesIDW = string.Empty;
            string FormulaDesIDH = string.Empty;
            try
            {
                //DataView dv = new DataView(dsPolicy.Tables[0]);
                //dv.RowFilter = "systemid='" + empid + "'";
                if (dicPolicy.ContainsKey(empid))
                {
                    DataRow dtr = dicPolicy[empid];

                    FormulaDesIDN = dtr["FormulaDesIDN"].ToString();
                    FormulaDesIDW = dtr["FormulaDesIDW"].ToString();
                    FormulaDesIDH = dtr["FormulaDesIDH"].ToString();
                    string EmployeeCode = dtr["EmployeeCode"].ToString();

                    if (string.IsNullOrEmpty(FormulaDesIDN))
                    {
                        throw new Exception("Employee " + EmployeeCode + " has no OT policy with her/his designation ...");
                    }


                    //DataView dvss = new DataView(dsSalaryStruc.Tables[0]);
                    //dvss.RowFilter = "EmpInfoSystemID='" + empid + "'";
                    if (dicSalaryStruc.ContainsKey(empid))
                    {


                        string FormulaValue = string.Empty;
                        DataTable dtValue = dicSalaryStruc[empid].CopyToDataTable();// .ToDataSet().Tables[0];// dvss.ToTable();
                        DataTable dtSalaryHead = dicSalaryStruc[empid].CopyToDataTable().DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead");// dvss.ToTable(true, "SalaryHeadID", "SalaryHead");

                        dtValue.TableName = "Temp";
                        dtSalaryHead.TableName = "Temp";

                        GetFormulValue(FormulaDesIDH, ref dtValue, _currencyId, out hRate, ref dtSalaryHead);

                        GetFormulValue(FormulaDesIDW, ref dtValue, _currencyId, out wRate, ref dtSalaryHead);

                        GetFormulValue(FormulaDesIDN, ref dtValue, _currencyId, out nwRate, ref dtSalaryHead);

                    }//if
                }//if dv

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetFormulValue(string FormulaDesIDN, ref DataTable dtValue, string _currencyId, out double nwRate, ref DataTable dtSalaryHead)
        {
            string FormulaValue = string.Empty;
            nwRate = 0;
            try
            {
                clsSalaryUtility su = new clsSalaryUtility();
                su.ReLoadFormulaWithValue(FormulaDesIDN, ref dtValue, _currencyId, "1", out FormulaValue, ref dtSalaryHead);
                string sFormulaResult = clsSalaryStructureAplos.Evaluate(FormulaValue).ToString();
                if (sFormulaResult == "NaN")
                {
                    sFormulaResult = "0.00";
                    //throw new Exception("Salary Head is not orderly tagged in Salary Rule");
                }

                //get formula wise value
                var vv = Convert.ToDouble(sFormulaResult).ToString("00.00");
                nwRate = Convert.ToDouble(vv);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void GetHourotmonthReportwithoutWeekendHoliday(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, out DataSet dsRef)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,sb.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                      ,sum(ho.Duration)as Duration,sum(CAST(ho.Duration AS decimal)/60)as DurationH
                                    ,ad.IsAllDesignation--1
                                    ,ISNULL(ad.IsFixed,0)as IsFixed---1--rate--0-farmula
                                    ,ISNULL(ad.Rate,0) as Rate
                                    ,ad.FormulaDesID
                                    ,ISNULL(dar.IsFixed,0)as IsFixedFromRate--1--rate--0--farmula
                                    ,ISNULL(dar.rate,0)as ratear
                                    ,dar.FormulaDesID FormulaDesIDFromRate
		                            ,ISNULL(bb.UserName,'') BankName
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory
                                      FROM HourlyOT  HO 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
                                      LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                      LEFT JOIN [ORG].[Section] s on s.Id=p.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=p.SubSectionId
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=p.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=mb.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=HO.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ho.PlantId
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId							
                                      LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ad.PlantId AND dar.DesignationId=ei.GivenDesignationId
                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @"
                                   AND DT.Category NOT IN('Weekend','Holiday')  " + wcDos + @" AND ei.plantid='" + plantId + @"' " + wcEmpSystemId + @" 
                                    --AND ad.Catagory='HourlyOffDuty' 
                                    --AND ad.Active=1
                                    group by  EmployeeName
									,EmployeeCode
                                    ,ei.SystemId
									,DOJ
									,s.UserName
									,sb.UserName
									,lg.UserName
									,d.UserName
									,ei.GenderID
									,HO.EmpSystemId
									,l.UserName
									,hr.OTConsiderOn
                                    --,EntryAmount
                                    ,ad.IsAllDesignation
                                    ,ad.IsFixed
                                    ,ad.FormulaDesID
                                    ,dar.IsFixed
                                    ,dar.FormulaDesID
                                    ,ad.Rate
                                    ,dar.rate
	                                ,ei.DOS	,bb.UserName
									,PG.UserName
                                    ,ebi.IFSCCode
									,ebi.BankAccNo
                                    ,ec.UserName
                                   ORDER BY ei.EmployeeCode
                                    ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);
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

        public void GetHourOTMonthReportWithWeekendORHoliday(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, string DayCategory, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,sb.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                      ,sum(ho.Duration) AS Duration,SUM(CAST(ho.Duration AS decimal)/60) AS DurationH

                                    ,AD.IsAllDesignation--1
                                    ,ISNULL(ad.IsFixed,0) AS IsFixed---1--rate--0-farmula
                                    ,ISNULL(ad.Rate,0) AS Rate
                                    ,AD.FormulaDesID
                                    ,ISNULL(dar.IsFixed,0) AS IsFixedFromRate--1--rate--0--farmula
                                    ,ISNULL(dar.rate,0) AS ratear
                                    ,dar.FormulaDesID FormulaDesIDFromRate
		                            ,ISNULL(bb.UserName,'') BankName
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory
                                      FROM HourlyOT  HO 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
                                      LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                      LEFT JOIN [ORG].[Section] s on s.Id=p.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=p.SubSectionId
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=p.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=mb.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=HO.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ho.PlantId
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
									
                                      LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ad.PlantId AND dar.DesignationId=ei.GivenDesignationId

                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @" AND DT.Category IN ('" + DayCategory + @"')  " + wcDos + @" AND ei.plantid='" + plantId + @"' " + wcEmpSystemId + @" 
                                        --AND ad.Catagory='HourlyOffDuty' AND ad.Active=1
                                    GROUP BY  EmployeeName,EmployeeCode,ei.SystemId,DOJ,s.UserName,sb.UserName,lg.UserName
									,d.UserName,ei.GenderID,HO.EmpSystemId,l.UserName,hr.OTConsiderOn --,EntryAmount
                                    ,ad.IsAllDesignation
                                    ,ad.IsFixed
                                    ,ad.FormulaDesID
                                    ,dar.IsFixed
                                    ,dar.FormulaDesID
                                    ,ad.Rate
                                    ,dar.rate
	                                ,ei.DOS	,bb.UserName
									,PG.UserName
                                    ,ebi.IFSCCode
									,ebi.BankAccNo
                                    ,ec.UserName
                                   ORDER BY ei.EmployeeCode
                                    ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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


        private void CreateDynamicSHead(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out int _count_earning_notionalhead, out Dictionary<string, SalaryHeadSequence> list)
        {
            try
            {
                list = new Dictionary<string, SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_deducting_head = 0;
                _count_earning_ctchead = 0;
                _count_earning_notionalhead = 0;
                int countGrossPostion = 0;
                string deductionFormula = "";

                xlsCol += 1;
                countGrossPostion++;

                int countCTCPosition = countGrossPostion;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {

                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper() && bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["PartOfNetPay"].ToString().ToUpper()))
                        {
                            _total_head_count++;


                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Size = 10;
                            //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.ShrinkToFit = true;

                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
                            salaryHeadSequence.IsNetPayEffect = Convert.ToBoolean(dtSalaryHead.Rows[ci]["PartOfNetPay"]);
                            salaryHeadSequence.IsGrossComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsGrossComponent"]);
                            salaryHeadSequence.IsCTCComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsCTCComponent"]);

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCPosition;

                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);
                            countCTCPosition++;
                        }


                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;


                _count_earning_ctchead = countCTCPosition - 1;

                int countDeductionPosition = countCTCPosition - 1;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        //{
                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
                        {
                            _total_head_count++;
                            countDeductionPosition++;

                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 10;
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = "Arial Narrow";
                            //sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
                            if (deductionFormula.Length == 0)
                            {
                                deductionFormula += salaryHeadSequence.XLColIndex.ToString();
                            }
                            else
                            {
                                deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                            }

                            //countDeductionPosition++;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;

                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();


                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

                            _count_deducting_head++;
                        }
                        //}//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for
                int countCTCNOTNETPosition = countDeductionPosition;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "Net Payable".ToUpper())
                    {

                        _total_head_count++;
                        countDeductionPosition++;

                        sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                        sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Size = 10;
                        sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.FontName = "Arial Narrow";
                        //sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition, xlsRow + 1, ColGrs + countDeductionPosition + 1].Merge();
                        sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.ShrinkToFit = true;


                        if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                        {
                            sheet1.Range[xlsRow + 1, ColGrs + countDeductionPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                        }
                        xlsCol += 2;
                        SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                        salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;
                        if (deductionFormula.Length == 0)
                        {
                            deductionFormula += salaryHeadSequence.XLColIndex.ToString();
                        }
                        else
                        {
                            deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                        }

                        //countDeductionPosition++;

                        salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                        salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                        salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                        salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                        salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();

                        salaryHeadSequence.Sequence = ci;
                        salaryHeadSequence.XLColIndex = ColGrs + countDeductionPosition;

                        salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();


                        list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

                        _count_deducting_head++;

                        //}//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for
                countCTCNOTNETPosition = countDeductionPosition;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {

                        if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "Net Payable".ToUpper() && !bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["PartOfNetPay"].ToString().ToUpper()))
                        {
                            _total_head_count++;
                            countCTCNOTNETPosition++;

                            sheet1.Range[xlsRow + 1, ColGrs + countCTCNOTNETPosition].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();

                            sheet1.Range[xlsRow + 1, ColGrs + countCTCNOTNETPosition].CellStyle.Font.FontName = "Arial Narrow";
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCNOTNETPosition].CellStyle.Font.Size = 10;
                            //sheet1.Range[xlsRow + 1, ColGrs + countCTCPosition, xlsRow + 1, ColGrs + countCTCPosition + 1].Merge();
                            sheet1.Range[xlsRow + 1, ColGrs + countCTCNOTNETPosition].CellStyle.ShrinkToFit = true;

                            if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                            {
                                sheet1.Range[xlsRow + 1, ColGrs + countCTCNOTNETPosition].CellStyle.Font.Color = ExcelKnownColors.Red;
                            }
                            xlsCol += 2;
                            SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCNOTNETPosition;

                            salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                            salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                            salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                            salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                            salaryHeadSequence.HeadType = dtSalaryHead.Rows[ci]["HeadType"].ToString();
                            salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
                            salaryHeadSequence.IsNetPayEffect = Convert.ToBoolean(dtSalaryHead.Rows[ci]["PartOfNetPay"]);
                            salaryHeadSequence.IsGrossComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsGrossComponent"]);
                            salaryHeadSequence.IsCTCComponent = Convert.ToBoolean(dtSalaryHead.Rows[ci]["IsCTCComponent"]);

                            salaryHeadSequence.Sequence = ci;
                            salaryHeadSequence.XLColIndex = ColGrs + countCTCNOTNETPosition;

                            list.Add(dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString(), salaryHeadSequence);

                            _count_earning_notionalhead++;

                        }


                    }//SalaryHead 
                    #endregion
                }//for
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Dictionary<string, List<DataRow>> GetEmployeeSalaryInfoDetail(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, out DataTable distinctSalaryHead)
        {
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            distinctSalaryHead = new DataTable("Tmp");
            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID in (" + plantId + @") GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + @"') AND YearNo = Year('" + fromDate + @"')";
            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);

            string salaryProcessID = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessID += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }


            try
            {
                strSQL = @"SELECT EmpSlr.*,PSH.Sequence,ISNULL(crc.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(CRC.IntegerInDisb,1) IntegerInDisb,ISNULL(CRC.DecimalNo,0) DecimalNo FROM(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
                                                    SPC.EmpInfoSystemID EmpSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, ISNULL(SH.IsCTCComponent,0) IsCTCComponent, ISNULL(SH.IsGrossComponent,0) IsGrossComponent
                                                    , sh.SalaryHead, sh.HeadCategory, sh.HeadType, ISNULL(SH.PartOfNetPay,0) PartOfNetPay

                                     FROM SalaryProcChild SPC

                                        left JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID



                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID= spc.SalaryHeadID


                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id

                                                        LEFT JOIN (
                                                                   SELECT* FROM ExchangerateDateWiseForHR

                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster


                                                                                                            WHERE SystemID IN(" + salaryProcessID + @")
																  )) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode

                                                                                            AND SPC.PlantID = Exr.PlantID

                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

                                                        where isnull(SPC.SlrProcMstSystemID,'')  IN(" + salaryProcessID + @")) EmpSlr--ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID

                                            Inner join EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpSystemID

                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
                                        LEFT JOIN(SELECT* FROM dbo.SalaryHead) PSH
                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND EEI.CompanyId = '" + companyId + @"'";

                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"AND EmpSlr.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";

                        }
                    }
                }
                catch (Exception)
                {
                }
                strSQL += "ORDER BY EmpSlr.EmpSystemID";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);

                distinctSalaryHead = dsRef.Tables[0].DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo", "PartOfNetPay", "IsCTCComponent", "IsGrossComponent");
                distinctSalaryHead.DefaultView.Sort = "Sequence";
                distinctSalaryHead = distinctSalaryHead.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                        {
                            _data = new List<DataRow>();
                            dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                        }
                        _data.Add(dt.Rows[i]);

                        empId = dt.Rows[i]["EmpSystemID"].ToString();
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }

                return dicBonus;


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//End Function


        public Dictionary<string, DataRow> GetDictionaryHourotmonthReportwithoutWeekendHoliday(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsRef = null;
            Dictionary<string, DataRow> dicHourlyOt = new Dictionary<string, DataRow>();
            string strSql = string.Empty;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,sb.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                      ,sum(ho.Duration)as Duration,sum(CAST(ho.Duration AS decimal)/60)as DurationH
                                    ,ad.IsAllDesignation--1
                                    ,ISNULL(ad.IsFixed,0)as IsFixed---1--rate--0-farmula
                                    ,ISNULL(ad.Rate,0) as Rate
                                    ,ad.FormulaDesID
                                    ,ISNULL(dar.IsFixed,0)as IsFixedFromRate--1--rate--0--farmula
                                    ,ISNULL(dar.rate,0)as ratear
                                    ,dar.FormulaDesID FormulaDesIDFromRate
		                            ,ISNULL(bb.UserName,'') BankName
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory
                                      FROM HourlyOT  HO 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
                                      LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                      LEFT JOIN [ORG].[Section] s on s.Id=p.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=p.SubSectionId
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=p.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=mb.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=HO.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ho.PlantId
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId							
                                      LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ad.PlantId AND dar.DesignationId=ei.GivenDesignationId
                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @"
                                   AND DT.Category NOT IN('Weekend','Holiday')  
                                    " + wcDos + @" AND ei.plantid in (" + plantId + @") " + wcEmpSystemId + @"                             
                                    GROUP BY  EmployeeName,EmployeeCode,ei.SystemId,DOJ
									,s.UserName,sb.UserName,lg.UserName,d.UserName,ei.GenderID,HO.EmpSystemId,l.UserName,hr.OTConsiderOn ,ad.IsAllDesignation 
                                    ,ad.IsFixed,ad.FormulaDesID,dar.IsFixed,dar.FormulaDesID,ad.Rate ,dar.rate,ei.DOS	,bb.UserName,PG.UserName
                                    ,ebi.IFSCCode,ebi.BankAccNo ,ec.UserName
                                   ORDER BY ei.EmployeeCode
                                    ";

              
                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicHourlyOt.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicHourlyOt;
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
        public Dictionary<string, DataRow> GetDictionaryHourotmonthReportwithoutWeekendHolidayExtraOTCTC(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsRef = null;
            Dictionary<string, DataRow> dicHourlyOt = new Dictionary<string, DataRow>();
            string strSql = string.Empty;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT ei.SystemId,ei.EmployeeCode,sum(ho.Duration)as Duration,sum(CAST(ho.Duration AS decimal)/60)as DurationH
                                      FROM HourlyOT  HO 
                                       LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                     LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
                                       left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=EI.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=EI.PlantId
							            LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = EI.PlantId AND dar.DesignationId=DM.DesignationId
                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @"
                                   AND DT.Category NOT IN('Weekend','Holiday')  
                                    " + wcDos + @" AND ei.plantid in (" + plantId + @") " + wcEmpSystemId + @"                             
                                    GROUP BY ei.SystemId,ei.EmployeeCode
                                   ORDER BY ei.EmployeeCode
                                    ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicHourlyOt.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicHourlyOt;
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



        public DataTable getHourlyOT(string plantId, string monthNo, string yearNo, Dictionary<string, string> parameters)
        {
            DataSet dsRef = null;
            try
            {


                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and EI.SystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                string strSql = "";
                strSql = @"SELECT  ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,SS.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                   ,ISNULL(bb.UserName,'') BankName
                                    --,ad.IsAllDesignation--1
                                    --,ISNULL(ad.IsFixed,0)as IsFixed---1--rate--0-farmula
                                    --,ISNULL(ad.Rate,0) as Rate
                                    --,ad.FormulaDesID
                            
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory
                                      FROM EmployeeInformation ei   
                                      INNER JOIN (Select distinct EmpsystemId From HourlyOT HO where  HO.Duration !=0  and Month(HO.WorkDate) = " + monthNo + @" and Year(HO.WorkDate) = " + yearNo + @" ) HO ON ei.SystemId = HO.EmpSystemId
									  LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = ei.BudgetCode
								      LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                      LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
												
												LEFT JOIN [ORG].[Line] L ON L.Id = MB.LineId							
                                		
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department d ON PO.DepartmentID = d.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId                                 
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=ei.PlantId                                    
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId	
									WHERE  EI.plantid='" + plantId + @"' " + wcEmpSystemId + @"";
                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                return dsRef.Tables[0];

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public Dictionary<string, DataRow> GetDictionaryHourOTMonthReportWithWeekendORHoliday(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, string DayCategory)
        {
            ConnectionManager.DAL.ConManager objCon;
            Dictionary<string, DataRow> dicOTPolicy = new Dictionary<string, DataRow>();
            string strSql = string.Empty;
            DataSet dsRef = null;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,sb.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                      ,sum(ho.Duration) AS Duration,SUM(CAST(ho.Duration AS decimal)/60) AS DurationH

                                    ,AD.IsAllDesignation--1
                                    ,ISNULL(ad.IsFixed,0) AS IsFixed---1--rate--0-farmula
                                    ,ISNULL(ad.Rate,0) AS Rate
                                    ,AD.FormulaDesID
                                    ,ISNULL(dar.IsFixed,0) AS IsFixedFromRate--1--rate--0--farmula
                                    ,ISNULL(dar.rate,0) AS ratear
                                    ,dar.FormulaDesID FormulaDesIDFromRate
		                            ,ISNULL(bb.UserName,'') BankName
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory
                                      FROM HourlyOT  HO 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
                                      LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                      LEFT JOIN [ORG].[Section] s on s.Id=p.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=p.SubSectionId
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=p.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=mb.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=HO.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ho.PlantId
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
									
                                      LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ad.PlantId AND dar.DesignationId=ei.GivenDesignationId

                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @" AND DT.Category IN ('" + DayCategory + @"')  " + wcDos + @" AND ei.plantid in (" + plantId + @") " + wcEmpSystemId + @" 
                                        --AND ad.Catagory='HourlyOffDuty' AND ad.Active=1
                                    GROUP BY  EmployeeName,EmployeeCode,ei.SystemId,DOJ,s.UserName,sb.UserName,lg.UserName
									,d.UserName,ei.GenderID,HO.EmpSystemId,l.UserName,hr.OTConsiderOn --,EntryAmount
                                    ,ad.IsAllDesignation
                                    ,ad.IsFixed
                                    ,ad.FormulaDesID
                                    ,dar.IsFixed
                                    ,dar.FormulaDesID
                                    ,ad.Rate
                                    ,dar.rate
	                                ,ei.DOS	,bb.UserName
									,PG.UserName
                                    ,ebi.IFSCCode
									,ebi.BankAccNo
                                    ,ec.UserName
                                   ORDER BY ei.EmployeeCode
                                    ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicOTPolicy.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicOTPolicy;
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public Dictionary<string, DataRow> GetDictionaryHourOTMonthReportWithWeekendORHolidayPOSTCTC(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, string DayCategory)
        {
            ConnectionManager.DAL.ConManager objCon;
            Dictionary<string, DataRow> dicOTPolicy = new Dictionary<string, DataRow>();
            string strSql = string.Empty;
            DataSet dsRef = null;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {



                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT ei.SystemId,ei.EmployeeCode,sum(ho.Duration)as Duration,sum(CAST(ho.Duration AS decimal)/60)as DurationH
                                      FROM HourlyOT  HO 
                                       LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                     LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
                                       left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=ei.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ei.PlantId
							            LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ei.PlantId AND dar.DesignationId=dm.DesignationId

                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @" AND DT.Category IN ('" + DayCategory + @"')  " + wcDos + @" AND ei.plantid in (" + plantId + @") " + wcEmpSystemId + @" 
                                        --AND ad.Catagory='HourlyOffDuty' AND ad.Active=1
                                    GROUP BY  EI.SystemId,EI.EmployeeCode
                                   ORDER BY ei.EmployeeCode
                                    ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicOTPolicy.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicOTPolicy;
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public Dictionary<string, DataRow> GetDictionaryHourOTMonthReportWithWeekendORHolidaySaad(string YearNo, string MonthNo, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, string DayCategory)
        {
            ConnectionManager.DAL.ConManager objCon;
            Dictionary<string, DataRow> dicOTPolicy = new Dictionary<string, DataRow>();
            string strSql = string.Empty;
            DataSet dsRef = null;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                string Plant = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                            Plant += @"AND ei.plantid in (" + parameters["PlantId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT Plant.UserName PlantName,ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,sb.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                      ,sum(ho.Duration) AS Duration,SUM(CAST(ho.Duration AS decimal)/60) AS DurationH

                                    ,AD.IsAllDesignation--1
                                    ,ISNULL(ad.IsFixed,0) AS IsFixed---1--rate--0-farmula
                                    ,ISNULL(ad.Rate,0) AS Rate
                                    ,AD.FormulaDesID
                                    ,ISNULL(dar.IsFixed,0) AS IsFixedFromRate--1--rate--0--farmula
                                    ,ISNULL(dar.rate,0) AS ratear
                                    ,dar.FormulaDesID FormulaDesIDFromRate
		                            ,ISNULL(bb.UserName,'') BankName
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory
                                      FROM HourlyOT  HO 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
                                    LEFT JOIN ORG.Plant  on Plant.Id=ei.PlantId
                                      LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                      LEFT JOIN [ORG].[Section] s on s.Id=p.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=p.SubSectionId
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=p.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=mb.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=HO.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ho.PlantId
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
									
                                      LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ad.PlantId AND dar.DesignationId=ei.GivenDesignationId

                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @" AND DT.Category IN ('" + DayCategory + @"')  " + wcDos + @" " + Plant + @" " + wcEmpSystemId + @" 
                                        --AND ad.Catagory='HourlyOffDuty' AND ad.Active=1
                                    GROUP BY  EmployeeName,EmployeeCode,ei.SystemId,DOJ,s.UserName,sb.UserName,lg.UserName
									,d.UserName,ei.GenderID,HO.EmpSystemId,l.UserName,hr.OTConsiderOn --,EntryAmount
                                    ,ad.IsAllDesignation,Plant.UserName
                                    ,ad.IsFixed
                                    ,ad.FormulaDesID
                                    ,dar.IsFixed
                                    ,dar.FormulaDesID
                                    ,ad.Rate
                                    ,dar.rate
	                                ,ei.DOS	,bb.UserName
									,PG.UserName
                                    ,ebi.IFSCCode
									,ebi.BankAccNo
                                    ,ec.UserName
                                   ORDER BY ei.EmployeeCode
                                    ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicOTPolicy.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicOTPolicy;
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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



        public void GetEmployeeInfoDetailSalaryLogWise(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string salaryProcessSystemId, string payRollGroup, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            string salaryProcessId = "";
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";


            if (!string.IsNullOrEmpty(salaryProcessSystemId) && salaryProcessSystemId != "undefined" && salaryProcessSystemId != "null")
            {
                wcSalaryProcessSystemIdStr = "SystemID IN ('" + salaryProcessSystemId + @"')";
            }
            else
            {
                wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID in (" + plantId + @") GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";


                string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID in (" + plantId + @") GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')";

                DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
                salaryProcessId = "''";
                for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
                {
                    salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
                }
            }
            string wcEmpStatus = " AND (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='MLV_PRE'";

                }
            }

            wcEmpStatus += ")";

            try
            {
                strSQL = @"SELECT EmpBasic.*,MMDSA.*,ISNULL(MW.Grade,'') Grade,ISNULL(MW.SalaryHeadValue,0) MinimumWage
                            FROM
                                    (
									SELECT DISTINCT E.SystemID EmpSystemId,AP.WorkDays,ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric,E.GroupID CompanyGroupId,E.CompanyId, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus EmployeeStatusReal,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID,  F.Id PlantID, F.UserName PlantName, 
											FU.UserName UnitName,  DV.UserName DivisionName,  DP.UserName DepartmentName,
											 S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,EC.WorkingDaysInAMonth--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,egdsgg.GivenDesignationGroup,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
											,ISNULL(Line.UserName,'') LineName
											,ISNULL(E.GenderID,'') Gender
                                            ,ISNULL(LSalGr.Code,'') GradeCode
											,ISNULL(PG.UserName,'') PayRollGroup
                                    , CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(SPLD.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(spld.BankAccNo,'') BankAccNo
                                    ,ISNULL(spld.IFSCCode,'') IFSCCode
                                    ,CASE WHEN ISNULL(PO.IsDirect,0) = 0 THEN 'No' ELSE 'Yes' END IsDirect
                                    ,CASE WHEN ISNULL(PO.DirectManpowerCost,0) = 0 THEN 'No' ELSE 'Yes' END DirectManpowerCost

                                     FROM EmployeeInformation E
                                    JOIN (
                                    SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
                                    FROM SalaryProcChild c
                                    JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
                                    WHERE SlrProcMstSystemID in (SELECT systemid FROM SalaryProcMaster WHERE MonthNo= MONTH('" + fromDate + @"') AND YearNo=YEAR('" + toDate + @"'))
                                    AND PlantID in (" + plantId + @")
                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN(" + salaryProcessId + @") AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '202022' 
                         
									 			LEFT JOIN ORG.Plant F ON SPLD.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON SPLD.LegalDesignationId = LDS.Id
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

												LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
												  LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
                                        left join (select EmpSystemID,count(*) AS WorkDays from AttdnProcessData 
                                        where WorkDate between '" + fromDate + @"' and '" + toDate + @"' and DayStatus NOT IN(select DayType from DayType where Category='Weekend')
                                        group by EmpSystemID) AS AP ON AP.EmpSystemID=e.SystemId
									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and SPLD.PlantId = LSalGr.PlantId
												
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id

												LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
            --                                    (
            --                                    SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												--LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												--)EC ON EC.DesignationId=E.GivenDesignationId
												[HKP].[EmployeeCategory] EC ON EC.Id = SPLD.EmployeeCategoryId
												LEFT JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									            ,dg.UserName GivenDesignationGroup
									            FROM MST.DesignationMaster dm
									            LEFT JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									            ) egdsgg ON egdsgg.DesignationId=e.GivenDesignationId
									            AND egdsgg.EmployeeCategoryId=SPLD.EmployeeCategoryId

                                      --Where SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      --WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        --WHERE PlantID in (" + plantId + @") GROUP BY SlrProcMstSystemID)
                                        --AND MonthNo =   MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')   )   
									) EmpBasic
                                   LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + fromDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = EmpBasic.EmpSystemId
                                    INNER JOIN
		                                    (
													SELECT EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
										,ISNULL(TotalLv,0) TotalLv
										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) +  ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
										  FROM SalaryProceAttdnData MMDSA where MMDSA.MonthNo = MONTH('" + fromDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + fromDate + @"') AND MMDSA.PlantID in (" + plantId + @")
											) MMDSA ON EmpBasic.EmpSystemID = MMDSA.EmpSystemID 
                                            WHERE EmpBasic.CompanyGroupId = '" + companyGroupId + @"'  AND EmpBasic.PlantId in (" + plantId + @") " + wcEmpStatus + @"";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            strSQL += @"and EmpBasic.EmpSystemId IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSQL += @"Order by EmpBasic.EmployeeCodePreFix,EmpBasic.EmployeeCodeNumeric ";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);


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

        private void FormatText(ref IWorksheet sheet1, ref IRichTextString rtf, string NewText, double FontSize)
        {
            IFont font = sheet1.Workbook.CreateFont();
            font.Color = ExcelKnownColors.Black;
            font.Size = FontSize;

            int oldPos = 0;
            if (rtf.Text.Length > 0)
                oldPos = rtf.Text.Length - 1;

            rtf.Append(NewText, font);
            rtf.SetFont(oldPos, (oldPos + NewText.Length) - 1, font);
        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = 7;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        void GetEarningDays(ref double EarningDays, string pvalue)
        {
            try
            {
                var vl = (string.IsNullOrEmpty(pvalue) ? "0" : pvalue);
                EarningDays += Convert.ToDouble(vl);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetCellTextNumber(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            //string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }
        private void SetCellValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 10;

            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetCellValueBangla(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width, string printFont, int rotationDegree)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = printFont;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Rotation = rotationDegree;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 24;

            ColIndex = xlsCol;
            xlsCol += 1;
        }

        private void SetCellValueRotate(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow + 1, xlsCol].Text = text;
            sheet.Range[xlsRow + 1, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.FontName = "Arial Narrow";
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Font.Size = 24;
            sheet.Range[xlsRow + 1, xlsCol].CellStyle.Rotation = 90;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        private void SetCellTextAttdn(IWorksheet sheet, int xlsRow, int xlsCol, double Value)
        {
            //string NumberFormatString = "#,##0;(#,##0)";
            //if (string.IsNullOrEmpty(Value.to) == false)
            //{
            // if (dvSlrProc[i]["SalaryHeadID"].ToString() == "SHD2017-1" & string.IsNullOrEmpty(dvSlrProc[i]["SalaryHeadID"].ToString()) == false)
            // ColBasSlr += Convert.ToDecimal(dvSlrProc[i]["DisbusmentAmount"].ToString());

            sheet.Range[xlsRow, xlsCol].Number = Value;
            sheet.Range[xlsRow, xlsCol].NumberFormat = clsStaticInfo.NumberFormat(2);
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //}
        }

        class SalarySheetReportUD
        {
            public string EmpSystemID { get; set; }
            public string SalaryHeadID { get; set; }
            public string HeadCategory { get; set; }
            public decimal DisbusmentAmount { get; set; } = 0;
            public decimal EntryAmount { get; set; } = 0;
        }

        public void ReporHeader(string companyId, string PlantId, int xlsRow, int xlsCol, int endXlsCol, string reportTitle, IWorksheet sheet1)
        {
            try
            {
                DataSet dsCmp = null;
                DataSet dsFactory = null;
                string strPath = "";
                string CmpName = "";
                Image companyLogo = null;
                string FactoryName = "";
                clsReport objRpt = new clsReport();
                objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);
                objRpt.SelectedPlant(PlantId, out dsFactory);
                string companyLogoName = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'").Rows[0]["Image"].ToString();
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                xlsRow = 1;
                xlsCol = 1;

                string FactoryAddress = string.Empty;
                try
                {

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
                catch (Exception ex)
                {
                }


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
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = reportTitle;//"Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            }
            catch (Exception)
            {

                throw;
            }
        }


        public Dictionary<string, List<DataRow>> LoadSalaryStructure(string sPlantID, string sFromDate, string sToDate)
        {
            System.Data.DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                            select m.*,d.EntryAmount Amount,d.DefineAmount,d.SalaryHeadID,d.EntryCurrencyID,h.SalaryHead,h.HeadCategory,h.HeadType 
                            ,LD.UserName OldLegalDesignation, LG.Code OldGradeCode from
                                (---m
                            select max(ed) ed,EmpInfoSystemID from
                            (
                            select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid in (" + sPlantID + @") group by EmpInfoSystemID
												                            union 
                            select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid in (" + sPlantID + @")  group by EmpInfoSystemID
                            ) x 
                            group by EmpInfoSystemID
                            ) ---m
                            mx
                            left join (
                            select SystemID,EmpInfoSystemID,EffectiveDate  from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid in (" + sPlantID + @")
                            union
                            select SystemID,EmpInfoSystemID,EffectiveDate  from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid in (" + sPlantID + @")
                            )
                             m on m.EmpInfoSystemID=mx.EmpInfoSystemID and m.EffectiveDate=mx.ed
                            left join (
                            select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoDefine
                            union
                            select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoBack
                            )
                            d on d.SalaryID=m.SystemID
                            left join SalaryHead h on h.SalaryHeadID=d.SalaryHeadID
                            LEFT JOIN IncrementHistory IH on IH.ToSalaryId=d.SalaryID
                            
                            LEFT JOIN Hkp.LegalDesignation LD ON LD.Id = ih. FromLegalDesignationId
                            LEFT JOIN MST.LegalSalaryGradeDesignation LGD ON LGD.LegalDesignationId = ih.FromLegalDesignationId AND LGD.PlantId in (" + sPlantID + @")
                            
                            LEFT JOIN scs.LegalSalaryGrade LG ON LG.Id = LGD.LegalSalaryGradeId
left join EmployeeInformation e on e.SystemId =m.EmpInfoSystemID
                                where (e.DOJ<='" + sToDate + @"') and (e.DOS is null or e.DOS>='" + sFromDate + @"') and e.PlantId in (" + sPlantID + @")
                            ORDER BY m.EmpInfoSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");


                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpInfoSystemID"].ToString();
                }

                return dicBonus;
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
        public Dictionary<string, DataRow> LoadOverTimePolicy(string sPlantID, string sFromDate, string sToDate)
        {
            string strSQL;
            System.Data.DataSet dsRef = null;
            Dictionary<string, DataRow> dicOTPolicy = new Dictionary<string, DataRow>();
            ConnectionManager.DAL.ConManager objCon;//
            try
            {
                strSQL = @"
                                        SELECT distinct e.SystemId,E.EmployeeCode,e.GivenDesignationId,dc.IsOTEntitled
											,onw.FormulaDesID FormulaDesIDN,onw.IsFixed IsFixedN,onw.IsFormula IsFormulaN,onw.FixedValue FixedValueN
											,ow.FormulaDesID FormulaDesIDW,ow.IsFixed IsFixedW,ow.IsFormula IsFormulaW,ow.FixedValue FixedValueW
											,oh.FormulaDesID FormulaDesIDH,oh.IsFixed IsFixedH,oh.IsFormula IsFormulaH,oh.FixedValue FixedValueH


                                    FROM dbo.EmployeeInformation E                                                

												left join mst.DesignationMaster dml on dml.DesignationId=e.GivenDesignationId
												inner join (select DesignationMasterId,OverTimePmtPolicyMasterID,IsOTEntitled,PlantId
                                                            from scs.DesignationMasterConfiguration where PlantId in (" + sPlantID + @") and IsOTEntitled=1) dc 
                                                            on dc.DesignationMasterId=dml.Id and e.PlantId = dc.PlantId
												left join OverTimePmtPolicyMaster otpm on otpm.ID=dc.OverTimePmtPolicyMasterID and otpm.PlantID in (" + sPlantID + @")
												left join OverTimePmtPolicyDetails oH on oh.OverTimePmtPolicyID=otpm.ID and oh.OverTimeDayType='Holiday'
												left join OverTimePmtPolicyDetails oW on ow.OverTimePmtPolicyID=otpm.ID and ow.OverTimeDayType='Week Off'
												left join OverTimePmtPolicyDetails oNW on oNW.OverTimePmtPolicyID=otpm.ID and onw.OverTimeDayType='Working Day'

												where (e.DOJ<='" + sToDate + @"') and (dos is null or e.DOS>='" + sFromDate + @"') and e.PlantId in (" + sPlantID + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicOTPolicy.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicOTPolicy;
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

        //Sayanto Addition For Consolidated Weekends/WeekOff Good Work Report
        public IWorkbook GetWeekOFFExtraOTCon(string Name, string CompanyGroupId, string CompanyId, string PlantId, string Month, string Year, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = "";
            string _currencyId = "";
            Dictionary<string, double> dicNW = null;
            Dictionary<string, double> dicW = null;
            Dictionary<string, double> dicH = null;

            #endregion
            try
            {
                Dictionary<string, DataRow> dicHourlyOTNW = new Dictionary<string, DataRow>();

                DataTable dtEmpHourlyOt = getHourlyOTCon(PlantId, Month, Year, parameters);

                dicHourlyOTNW = GetDictionaryHourotmonthReportwithoutWeekendHolidayCon(Year, Month, PlantId, CompanyId, CompanyGroupId, parameters, isActive, isSeperated);
                if (dtEmpHourlyOt.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data Found....");
                    throw (ex);
                }

                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                Dictionary<string, DataRow> dicHourlyOTW = new Dictionary<string, DataRow>();
                dicHourlyOTW = GetDictionaryHourOTMonthReportWithWeekendORHolidayCon(Year, Month, PlantId, CompanyId, CompanyGroupId, parameters, isActive, isSeperated, "Weekend");



                string FirstDayOfTheMonth = "01-" + bplib.clsWebLib.GetMonthName(Month) + "-" + Year;
                string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                DataSet dsCurrency = null;

                Dictionary<string, List<DataRow>> dicSalStructure = LoadSalaryStructureCon(PlantId, FirstDayOfTheMonth, LastDayOfTheMonth);
                Dictionary<string, DataRow> dicOTpolicy = LoadOverTimePolicyCon(PlantId, FirstDayOfTheMonth, LastDayOfTheMonth);

                clsSalaryInfo objSal = new clsSalaryInfo();

                GetLocalCurrencyCon(CompanyGroupId, CompanyId, out dsCurrency);
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }
                GenerateDic(dicOTpolicy, dicSalStructure, _currencyId, out dicNW, out dicW, out dicH);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;

                var iName = 0;
                var iEmployeeCode = 0;
                var iPlantName = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iBasic = 0;
                var iWeekDayOTHr = 0;
                var iAmount = 0;
                var iLine = 0;
                var totalAmount = 0.00;
                var iDOJ = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var totalEntryAmount = 0.00;
                var totalNWOTAmount = 0.00;
                var totalWOTAmount = 0.00;



                var isl = 0;
                var SLNo = 1;


                #region Hourly Ot

                IWorksheet sheet1 = null;

                sheet1 = workbook.Worksheets[0];
                xlsRow = 6;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "Emp Name";
                sheet1.Range[xlsRow, iName].ColumnWidth = 25;

                xlsCol += 1;
                iPlantName = xlsCol;
                sheet1.Range[xlsRow, iPlantName].Text = "Plant Name";
                sheet1.Range[xlsRow, iPlantName].ColumnWidth = 25;

                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;
                xlsCol += 1;
                int iDOS = xlsCol;
                sheet1.Range[xlsRow, iDOS].Text = "DOS";
                sheet1.Range[xlsRow, iDOS].ColumnWidth = 20;
                xlsCol += 1;
                iDepartment = xlsCol;
                sheet1.Range[xlsRow, iDepartment].Text = "Department";
                sheet1.Range[xlsRow, iDepartment].ColumnWidth = 25;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                sheet1.Range[xlsRow, iDesignation].ColumnWidth = 25;
                xlsCol += 1;
                int iPayrollGroup = xlsCol;
                sheet1.Range[xlsRow, iPayrollGroup].Text = "PayrollGroup";
                sheet1.Range[xlsRow, iPayrollGroup].ColumnWidth = 25;

                xlsCol += 1;
                int iCostCenter = xlsCol;
                sheet1.Range[xlsRow, iCostCenter].Text = "Cost Center";
                sheet1.Range[xlsRow, iCostCenter].ColumnWidth = 25;
                xlsCol += 1;
                int iEmployeeCategory = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                sheet1.Range[xlsRow, iEmployeeCategory].ColumnWidth = 25;

                xlsCol += 1;
                iSection = xlsCol;
                sheet1.Range[xlsRow, iSection].Text = "Section";
                sheet1.Range[xlsRow, iSection].ColumnWidth = 14;

                xlsCol += 1;
                iSubSection = xlsCol;
                sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                sheet1.Range[xlsRow, iSubSection].ColumnWidth = 16;

                xlsCol += 1;
                iLine = xlsCol;
                sheet1.Range[xlsRow, iLine].Text = "Line";
                sheet1.Range[xlsRow, iLine].ColumnWidth = 12;

                xlsCol += 1;
                iWeekDayOTHr = xlsCol;
                sheet1.Range[xlsRow, iWeekDayOTHr].Text = "Week Day(OT Hrs)";
                sheet1.Range[xlsRow, iWeekDayOTHr].ColumnWidth = 15;

                xlsCol += 1;
                int iTotalWkOffHr = xlsCol;
                sheet1.Range[xlsRow, iTotalWkOffHr].Text = "WeekOff(OT Hrs)";
                sheet1.Range[xlsRow, iTotalWkOffHr].ColumnWidth = 15;

                xlsCol += 1;
                iBasic = xlsCol;
                sheet1.Range[xlsRow, iBasic].Text = "Fixed Gross";
                sheet1.Range[xlsRow, iBasic].ColumnWidth = 15;


                xlsCol += 1;
                iAmount = xlsCol;
                sheet1.Range[xlsRow, iAmount].Text = "Amount";
                sheet1.Range[xlsRow, iAmount].ColumnWidth = 15;

                xlsCol += 1;
                int iBankAccNo = xlsCol;
                sheet1.Range[xlsRow, iBankAccNo].Text = "Bank Acc No";
                sheet1.Range[xlsRow, iBankAccNo].ColumnWidth = 25;

                xlsCol += 1;
                int iBankName = xlsCol;
                sheet1.Range[xlsRow, iBankName].Text = "Bank Name";
                sheet1.Range[xlsRow, iBankName].ColumnWidth = 25;
                xlsCol += 1;
                int iIFSCNO = xlsCol;
                sheet1.Range[xlsRow, iIFSCNO].Text = "IFSC";
                sheet1.Range[xlsRow, iIFSCNO].ColumnWidth = 25;

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;

                xlsRow++;

                #endregion ------------------Column Header---------------

                #region ----------------------Data-----------------------  
                string yot = string.Empty;
                string nwotFormated = string.Empty;

                double FOT = 0.00;
                double NWOT = 0.00;
                double entryAmount = 0.00;
                string EmpSystemid = "";
                int firstLoopRow = xlsRow;
                for (int i = 0; i < dtEmpHourlyOt.Rows.Count; i++)
                {
                    FOT = 0.00;
                    NWOT = 0.00;
                    nwotFormated = "";
                    yot = "";
                    entryAmount = 0.00;
                    EmpSystemid = dtEmpHourlyOt.Rows[i]["EmpSystemId"].ToString();
                    if (EmpSystemid == "207091")
                    {

                    }
                    //OTConsiderOn = dtrOTNW["OTConsiderOn"].ToString();

                    if (dicHourlyOTNW.ContainsKey(EmpSystemid))
                    {

                        DataRow dtrOTNW = dicHourlyOTNW[EmpSystemid];

                        NWOT = clsStaticInfo.dbl(dtrOTNW["Duration"].ToString());

                        oru.GetOT(OTConsiderOn, dtrOTNW["Duration"].ToString(), out nwotFormated);

                    }

                    if (dicHourlyOTW.ContainsKey(EmpSystemid))
                    {
                        DataRow dtrOTW = dicHourlyOTW[EmpSystemid];

                        FOT = clsStaticInfo.dbl(dtrOTW["Duration"].ToString());

                        oru.GetOT(OTConsiderOn, dtrOTW["Duration"].ToString(), out yot);

                    }
                    if (FOT == 0.00 && NWOT == 0.00)
                    {
                        continue;
                    }
                    sheet1.Range[xlsRow, iTotalWkOffHr].Text = yot;
                    sheet1.Range[xlsRow, iTotalWkOffHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, iTotalWkOffHr].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    if (dicSalStructure.ContainsKey(EmpSystemid))
                    {
                        List<DataRow> drSalaryHeadCollection = dicSalStructure[EmpSystemid].Where(x => x["HeadCategory"].ToString().ToUpper() == "GROSS").ToList();


                        entryAmount = clsStaticInfo.dbl(drSalaryHeadCollection[0]["Amount"].ToString());
                    }




                    //oru.GetOT(dtrOTNW["OTConsiderOn"].ToString(), dtrOTNW["Duration"].ToString(), out yot);

                    sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                    sheet1.Range[xlsRow, iName].Text = dtEmpHourlyOt.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, iEmployeeCode].Text = dtEmpHourlyOt.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, iPlantName].Text = dtEmpHourlyOt.Rows[i]["Plant"].ToString();
                    sheet1.Range[xlsRow, iDOJ].DateTime = Convert.ToDateTime(dtEmpHourlyOt.Rows[i]["DOJ"].ToString());
                    sheet1.Range[xlsRow, iDOJ].NumberFormat = "dd-MMM-yyyy";
                    if (!String.IsNullOrEmpty(dtEmpHourlyOt.Rows[i]["DOS"].ToString()))
                    {
                        sheet1.Range[xlsRow, iDOS].DateTime = Convert.ToDateTime(dtEmpHourlyOt.Rows[i]["DOS"].ToString());
                        sheet1.Range[xlsRow, iDOS].NumberFormat = "dd-MMM-yyyy";
                    }

                    sheet1.Range[xlsRow, iDesignation].Text = dtEmpHourlyOt.Rows[i]["Designation"].ToString();
                    sheet1.Range[xlsRow, iDepartment].Text = dtEmpHourlyOt.Rows[i]["Department"].ToString();
                    sheet1.Range[xlsRow, iSection].Text = dtEmpHourlyOt.Rows[i]["Section"].ToString();
                    sheet1.Range[xlsRow, iSubSection].Text = dtEmpHourlyOt.Rows[i]["SubSection"].ToString();
                    sheet1.Range[xlsRow, iLine].Text = dtEmpHourlyOt.Rows[i]["Line"].ToString();

                    sheet1.Range[xlsRow, iPayrollGroup].Text = dtEmpHourlyOt.Rows[i]["PayRollGroup"].ToString();
                    sheet1.Range[xlsRow, iBankAccNo].Text = dtEmpHourlyOt.Rows[i]["BankAccNo"].ToString();
                    sheet1.Range[xlsRow, iBankName].Text = dtEmpHourlyOt.Rows[i]["BankName"].ToString();
                    sheet1.Range[xlsRow, iIFSCNO].Text = dtEmpHourlyOt.Rows[i]["IFSCCode"].ToString();
                    sheet1.Range[xlsRow, iEmployeeCategory].Text = dtEmpHourlyOt.Rows[i]["EmployeeCategory"].ToString();

                    sheet1.Range[xlsRow, iWeekDayOTHr].Text = nwotFormated;
                    sheet1.Range[xlsRow, iWeekDayOTHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, iWeekDayOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    totalEntryAmount += NWOT + FOT;
                    totalNWOTAmount += NWOT;
                    totalWOTAmount += FOT;


                    sheet1.Range[xlsRow, iBasic].Number = clsStaticInfo.dbl(entryAmount);
                    sheet1.Range[xlsRow, iBasic].NumberFormat = NumberFormatTwoDecimal;


                    if (dicW.ContainsKey(EmpSystemid))
                    {
                        FOT = (FOT / 60) * dicW[EmpSystemid];
                    }
                    if (dicNW.ContainsKey(EmpSystemid))
                    {
                        sheet1.Range[xlsRow, iAmount].Number = clsStaticInfo.dbl(dicNW[EmpSystemid]) * (NWOT / 60) + FOT;

                        sheet1.Range[xlsRow, iAmount].NumberFormat = NumberFormatTwoDecimal;
                        totalAmount += clsStaticInfo.dbl(dicNW[EmpSystemid]) * (NWOT / 60) + (FOT / 60);
                    }

                    xlsRow++;
                    SLNo++;
                }


                string Tot = string.Empty;
                oru.GetOT(OTConsiderOn, totalNWOTAmount.ToString(), out Tot);
                sheet1.Range[xlsRow, iLine].Text = "Total";
                sheet1.Range[xlsRow, iLine + 1].Text = Tot;
                oru.GetOT(OTConsiderOn, totalWOTAmount.ToString(), out Tot);
                sheet1.Range[xlsRow, iLine + 2].Text = Tot;

                sheet1.Range[xlsRow, iLine + 4].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(iAmount) + firstLoopRow + ":" + clsStaticInfo.GetxlsCol(iAmount) + (xlsRow - 1) + ")";
                sheet1.Range[xlsRow, iLine + 4].NumberFormat = NumberFormatTwoDecimal;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, iLine, xlsRow, iLine + 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************

                xlsRow = 1;
                xlsCol = 1;


                string reportTitle = "Week day ExtraOt and WeekOff OT For " + bplib.clsWebLib.GetMonthName(Month) + @", " + Year + @"";
                ReporHeaderCon(CompanyId, PlantId, xlsRow, xlsCol, endXlsCol, reportTitle, sheet1);
                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Hourly OT Monthly";
                #endregion Page Setup

                #endregion  Attendance Summary Status

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable getHourlyOTCon(string plantId, string monthNo, string yearNo, Dictionary<string, string> parameters)
        {
            DataSet dsRef = null;
            try
            {


                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and EI.SystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                string strSql = "";
                strSql = @"SELECT  ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,SS.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                   ,ISNULL(bb.UserName,'') BankName
                                    --,ad.IsAllDesignation--1
                                    --,ISNULL(ad.IsFixed,0)as IsFixed---1--rate--0-farmula
                                    --,ISNULL(ad.Rate,0) as Rate
                                    --,ad.FormulaDesID
                            
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory, p.UserName as Plant
                                      FROM EmployeeInformation ei   
                                      INNER JOIN (Select distinct EmpsystemId From HourlyOT HO where  HO.Duration !=0  and Month(HO.WorkDate) = " + monthNo + @" and Year(HO.WorkDate) = " + yearNo + @" ) HO ON ei.SystemId = HO.EmpSystemId
									  LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = ei.BudgetCode
								      LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                      LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId
												
												LEFT JOIN [ORG].[Line] L ON L.Id = MB.LineId							
                                		
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department d ON PO.DepartmentID = d.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId                                 
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=ei.PlantId                                    
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                        left join org.Plant p on p.Id = ei.PlantId
									WHERE  EI.plantid in (" + plantId + @") " + wcEmpSystemId + @"";
                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                return dsRef.Tables[0];

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public Dictionary<string, DataRow> GetDictionaryHourotmonthReportwithoutWeekendHolidayCon(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated)
        {
            //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsRef = null;
            Dictionary<string, DataRow> dicHourlyOt = new Dictionary<string, DataRow>();
            string strSql = string.Empty;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,sb.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                      ,sum(ho.Duration)as Duration,sum(CAST(ho.Duration AS decimal)/60)as DurationH
                                    ,ad.IsAllDesignation--1
                                    ,ISNULL(ad.IsFixed,0)as IsFixed---1--rate--0-farmula
                                    ,ISNULL(ad.Rate,0) as Rate
                                    ,ad.FormulaDesID
                                    ,ISNULL(dar.IsFixed,0)as IsFixedFromRate--1--rate--0--farmula
                                    ,ISNULL(dar.rate,0)as ratear
                                    ,dar.FormulaDesID FormulaDesIDFromRate
		                            ,ISNULL(bb.UserName,'') BankName
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory , p.UserName as Plant
                                      FROM HourlyOT  HO 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
                                      LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                      LEFT JOIN [ORG].[Section] s on s.Id=p.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=p.SubSectionId
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=p.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=mb.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=ei.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ei.PlantId
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                        left join org.Plant p on p.Id = ei.PlantId
                                      LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ei.PlantId AND dar.DesignationId=dm.DesignationId
                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @"
                                   AND DT.Category NOT IN('Weekend','Holiday')  
                                    " + wcDos + @" AND ei.plantid in (" + plantId + @") " + wcEmpSystemId + @"                             
                                    GROUP BY  EmployeeName,EmployeeCode,ei.SystemId,DOJ
									,s.UserName,sb.UserName,lg.UserName,d.UserName,ei.GenderID,HO.EmpSystemId,l.UserName,hr.OTConsiderOn ,ad.IsAllDesignation 
                                    ,ad.IsFixed,ad.FormulaDesID,dar.IsFixed,dar.FormulaDesID,ad.Rate ,dar.rate,ei.DOS	,bb.UserName,PG.UserName
                                    ,ebi.IFSCCode,ebi.BankAccNo ,ec.UserName,p.UserName
                                   ORDER BY ei.EmployeeCode
                                    ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicHourlyOt.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicHourlyOt;
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

        public Dictionary<string, DataRow> GetDictionaryHourOTMonthReportWithWeekendORHolidayCon(string YearNo, string MonthNo, string plantId, string companyId, string companyGroupId, Dictionary<string, string> parameters, bool isActive, bool isSeperated, string DayCategory)
        {
            ConnectionManager.DAL.ConManager objCon;
            Dictionary<string, DataRow> dicOTPolicy = new Dictionary<string, DataRow>();
            string strSql = string.Empty;
            DataSet dsRef = null;
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                string wcDos = "AND (1=0";

                if (isActive == true && isSeperated == true)
                {
                    wcDos = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') = ''";
                    }
                    if (isSeperated == true)
                    {
                        wcDos += " OR ISNULL(ei.DOS,'') <> ''";
                    }
                }

                wcDos += ")";

                string wcEmpSystemId = "";
                try
                {
                    if (parameters.Count > 0)
                    {
                        if (parameters.Keys.ElementAt(0) != "")
                        {
                            wcEmpSystemId += @"and HO.EmpSystemID IN(" + parameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {

                }

                strSql = @"SELECT ei.SystemId,ei.EmployeeName,ei.EmployeeCode,format(ei.DOJ,'dd-MMM-yyyy') DOJ,format(ei.DOS,'dd-MMM-yyyy') DOS,s.UserName as Section,sb.UserName as SubSection,lg.UserName Designation
                                ,d.UserName Department,ei.GenderID,HO.EmpSystemId,l.UserName as Line,hr.OTConsiderOn--,YY.EntryAmount
                                      ,sum(ho.Duration) AS Duration,SUM(CAST(ho.Duration AS decimal)/60) AS DurationH

                                    ,AD.IsAllDesignation--1
                                    ,ISNULL(ad.IsFixed,0) AS IsFixed---1--rate--0-farmula
                                    ,ISNULL(ad.Rate,0) AS Rate
                                    ,AD.FormulaDesID
                                    ,ISNULL(dar.IsFixed,0) AS IsFixedFromRate--1--rate--0--farmula
                                    ,ISNULL(dar.rate,0) AS ratear
                                    ,dar.FormulaDesID FormulaDesIDFromRate
		                            ,ISNULL(bb.UserName,'') BankName
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,ISNULL(ebi.IFSCCode,'') IFSCCode
									,ISNULL(ebi.BankAccNo,'') BankAccNo
                                    ,ISNULL(ec.UserName,'') EmployeeCategory
                                      FROM HourlyOT  HO 
                                      LEFT JOIN EmployeeInformation ei on ei.SystemId=HO.EmpSystemId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position P ON MB.PositionId=P.Id
                                      LEFT JOIN AttdnProcessData ap on  ho.EmpSystemId=ap.EmpSystemID and HO.WorkDate=ap.WorkDate
                                        LEFT JOIN DayType  DT on  DT.DayType = ap.DayStatus
                                      LEFT JOIN [ORG].[Section] s on s.Id=p.SectionId
                                      LEFT JOIN [ORG].[SubSection] sb on sb.Id=p.SubSectionId
                                        left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=ei.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                        left join hkp.LegalDesignation LG on LG.Id = ei.LegalDesignationId
                                      LEFT JOIN [ORG].[Department] d on d.Id=p.DepartmentId
                                      LEFT JOIN [ORG].[Line] l on l.Id=mb.LineId
                                      LEFT JOIN PlantWiseHRMSSetting hr on hr.PlantID=ei.PlantId   
                                      LEFT JOIN hkp.AllowanceDaily ad on ad.PlantID=ei.PlantId
                                      LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=ei.SystemId
									  LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
									  LEFT JOIN [HKP].[BankBranch] bbranch on bbranch.Id = ebi.BankBranchId
									  LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = ei.SystemId
                                        LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
									
                                      LEFT JOIN DailyAllowanceRate dar on dar.DailyAllowanceId=ad.id AND dar.PlantId = ei.PlantId AND dar.DesignationId=dm.DesignationId

                                    WHERE Month(HO.WorkDate) = " + MonthNo + @" and Year(HO.WorkDate) = " + YearNo + @" AND DT.Category IN ('" + DayCategory + @"')  " + wcDos + @" AND ei.plantid in (" + plantId + @") " + wcEmpSystemId + @" 
                                        --AND ad.Catagory='HourlyOffDuty' AND ad.Active=1
                                    GROUP BY  EmployeeName,EmployeeCode,ei.SystemId,DOJ,s.UserName,sb.UserName,lg.UserName
									,d.UserName,ei.GenderID,HO.EmpSystemId,l.UserName,hr.OTConsiderOn --,EntryAmount
                                    ,ad.IsAllDesignation
                                    ,ad.IsFixed
                                    ,ad.FormulaDesID
                                    ,dar.IsFixed
                                    ,dar.FormulaDesID
                                    ,ad.Rate
                                    ,dar.rate
	                                ,ei.DOS	,bb.UserName
									,PG.UserName
                                    ,ebi.IFSCCode
									,ebi.BankAccNo
                                    ,ec.UserName
                                   ORDER BY ei.EmployeeCode
                                    ";

                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.getDataSet(strSql, out dsRef);

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicOTPolicy.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicOTPolicy;
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public Dictionary<string, List<DataRow>> LoadSalaryStructureCon(string sPlantID, string sFromDate, string sToDate)
        {
            System.Data.DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                            select m.*,d.EntryAmount Amount,d.DefineAmount,d.SalaryHeadID,d.EntryCurrencyID,h.SalaryHead,h.HeadCategory,h.HeadType 
                            ,LD.UserName OldLegalDesignation, LG.Code OldGradeCode from
                                (---m
                            select max(ed) ed,EmpInfoSystemID from
                            (
                            select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid in (" + sPlantID + @") group by EmpInfoSystemID
												                            union 
                            select EmpInfoSystemID,max(EffectiveDate) ed from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid in (" + sPlantID + @")  group by EmpInfoSystemID
                            ) x 
                            group by EmpInfoSystemID
                            ) ---m
                            mx
                            left join (
                            select SystemID,EmpInfoSystemID,EffectiveDate  from SalaryInfoDefineMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid in (" + sPlantID + @")
                            union
                            select SystemID,EmpInfoSystemID,EffectiveDate  from SalaryInfoBackMaster where IsApproved=1 and EffectiveDate<='" + sToDate + @"' and plantid in (" + sPlantID + @")
                            )
                             m on m.EmpInfoSystemID=mx.EmpInfoSystemID and m.EffectiveDate=mx.ed
                            left join (
                            select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoDefine
                            union
                            select systemid,EntryAmount,DefineAmount,SalaryID,SalaryHeadID,EntryCurrencyID from SalaryInfoBack
                            )
                            d on d.SalaryID=m.SystemID
                            left join SalaryHead h on h.SalaryHeadID=d.SalaryHeadID
                            LEFT JOIN IncrementHistory IH on IH.ToSalaryId=d.SalaryID
                            left join EmployeeInformation e on e.SystemId =m.EmpInfoSystemID
                            LEFT JOIN Hkp.LegalDesignation LD ON LD.Id = ih. FromLegalDesignationId
                            LEFT JOIN MST.LegalSalaryGradeDesignation LGD ON LGD.LegalDesignationId = ih.FromLegalDesignationId AND LGD.PlantId=e.PlantId AND LGD.PlantId in (" + sPlantID + @")
                            
                            LEFT JOIN scs.LegalSalaryGrade LG ON LG.Id = LGD.LegalSalaryGradeId

                                where (e.DOJ<='" + sToDate + @"') and (e.DOS is null or e.DOS>='" + sFromDate + @"') and e.PlantId in (" + sPlantID + @")
                            ORDER BY m.EmpInfoSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");


                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpInfoSystemID"].ToString();
                }

                return dicBonus;
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

        public Dictionary<string, DataRow> LoadOverTimePolicyCon(string sPlantID, string sFromDate, string sToDate)
        {
            string strSQL;
            System.Data.DataSet dsRef = null;
            Dictionary<string, DataRow> dicOTPolicy = new Dictionary<string, DataRow>();
            ConnectionManager.DAL.ConManager objCon;//
            try
            {
                strSQL = @"
                                        SELECT distinct e.SystemId,E.EmployeeCode,e.GivenDesignationId,dc.IsOTEntitled
											,onw.FormulaDesID FormulaDesIDN,onw.IsFixed IsFixedN,onw.IsFormula IsFormulaN,onw.FixedValue FixedValueN
											,ow.FormulaDesID FormulaDesIDW,ow.IsFixed IsFixedW,ow.IsFormula IsFormulaW,ow.FixedValue FixedValueW
											,oh.FormulaDesID FormulaDesIDH,oh.IsFixed IsFixedH,oh.IsFormula IsFormulaH,oh.FixedValue FixedValueH


                                    FROM dbo.EmployeeInformation E                                                

												left join mst.DesignationMaster dml on dml.DesignationId=e.GivenDesignationId
												inner join (select DesignationMasterId,OverTimePmtPolicyMasterID,IsOTEntitled ,PlantId
                                                            from scs.DesignationMasterConfiguration where PlantId in (" + sPlantID + @") and IsOTEntitled=1) dc 
                                                            on dc.DesignationMasterId=dml.Id and e.PlantId = dc.PlantId
												left join OverTimePmtPolicyMaster otpm on otpm.ID=dc.OverTimePmtPolicyMasterID and otpm.PlantID=E.PlantId  and otpm.PlantID in (" + sPlantID + @")
												left join OverTimePmtPolicyDetails oH on oh.OverTimePmtPolicyID=otpm.ID and oh.OverTimeDayType='Holiday'
												left join OverTimePmtPolicyDetails oW on ow.OverTimePmtPolicyID=otpm.ID and ow.OverTimeDayType='Week Off'
												left join OverTimePmtPolicyDetails oNW on oNW.OverTimePmtPolicyID=otpm.ID and onw.OverTimeDayType='Working Day'

												where (e.DOJ<='" + sToDate + @"') and (dos is null or e.DOS>='" + sFromDate + @"') and e.PlantId in (" + sPlantID + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                DataTable dt = dsRef.Tables[0];


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicOTPolicy.Add(dt.Rows[i]["SystemId"].ToString(), dt.Rows[i]);
                }

                return dicOTPolicy;
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

        public void GetLocalCurrencyCon(string sGroupID, string CompId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT C.Id AS LocalCurrency, C.[Name] AS Currency 		
                //          FROM scs.Currency C		
                //           INNER JOIN [SCS].[CurrencyTransaction] CA ON C.id = CA.CurrencyId		
                //          WHERE  CA.CompanyID IN (SELECT DISTINCT CompanyID 		
                //             FROM org.Plant		
                //             WHERE ID = '" + sPlantID + @"')		
                //          ORDER BY C.[Description]";
                strSQL = @"SELECT C.Id AS LocalCurrency, C.[Name] AS Currency 		
		                        FROM scs.Currency C		
			                        INNER JOIN [ORG].[Company] CA ON C.id = CA.BaseCurrencyId	
									where CA.Id = '" + CompId + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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

        public void ReporHeaderCon(string companyId, string PlantId, int xlsRow, int xlsCol, int endXlsCol, string reportTitle, IWorksheet sheet1)
        {
            try
            {
                DataSet dsCmp = null;
                DataSet dsFactory = null;
                string strPath = "";
                string CmpName = "";
                Image companyLogo = null;
                string FactoryName = "";
                clsReport objRpt = new clsReport();
                //objRpt.SelectedPlantWiseCompany(PlantId, out dsCmp);
                //objRpt.SelectedPlant(PlantId, out dsFactory);
                DataTable tt = _sqlRepository.GetDataTable(@"select * from ORG.Company where Id = '" + companyId + @"'");
                string companyLogoName = tt.Rows[0]["Image"].ToString();
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyLogoName);  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                xlsRow = 1;
                xlsCol = 1;

                string FactoryAddress = string.Empty;
                try
                {

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
                catch (Exception ex)
                {
                }


                //if (dsCmp.Tables[0].Rows.Count > 0)
                //{
                //    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                //}
                //else
                //{
                //    CmpName = "";
                //}
                sheet1.Range[xlsRow, 3].Text = tt.Rows[0]["UserName"].ToString();
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = reportTitle;//"Salary Sheet For The Month Of " + Convert.ToDateTime(fdateOfMonth).ToString("MMMM") + "," + Convert.ToDateTime(fdateOfMonth).ToString("yyyy");
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 14;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
