using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using System.Drawing;
using System.Collections.Specialized;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class NewJobCardReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public NewJobCardReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IWorkbook GetComplianceJobCardReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string EmpIdLoop, string fromDate, string toDate, bool chkAdditionInfo)
        {

            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsBioDvAC = null;
            DataTable dtBioDvAC = null;
            DataView dvBioDvAC = null;
            DataView dvSummary = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsMonthlySummary = null;

            //DataSet dsPayDays = null;
            DataTable dtMonthlySummary = null;
            DataView dvPayDays = null;

            StringCollection sEmpCodeColl = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            var workbook = oru.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            IWorksheet sheet1 = null;

            DataSet dsWeeklyAbsnt = null;
            DataTable dtWeeklyAbsnt = null;
            DataView dvWeeklyAbsnt = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string sOfficeInTime = "00:00";
            string sInTime = "00:00";
            string freezeRow = "";
            try
            {
                #region Validation
                if (string.IsNullOrEmpty(fromDate) == true || clsWebLib.IsDateOK(fromDate) == false)
                {

                    Exception ex = new Exception("Please define access From Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }
                if (string.IsNullOrEmpty(fromDate) == true || clsWebLib.IsDateOK(fromDate) == false)
                {

                    Exception ex = new Exception("Please define access To Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }
                DateTime dtFrmDate = clsWebLib.DateData_DBToApp(fromDate, clsWebLib.DB_DATE_FORMAT);
                DateTime dtToDate = clsWebLib.DateData_DBToApp(fromDate, clsWebLib.DB_DATE_FORMAT);
                TimeSpan tsFromToDate = dtToDate - dtFrmDate;
                int daysFromTo = tsFromToDate.Days;
                if (daysFromTo < 0)
                {
                    Exception ex = new Exception("Please check the access From Date, cannot more than access To Date...");
                    throw (ex);
                }

                #endregion Validation

                objRpt = new clsReport();
                dvPayDays = new DataView();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");//

                #region DataSet

                GetEmpJobCardInfoWithInDateTimes(EmpIdLoop, fromDate, toDate, plantId, out dsBioDvAC);
                dtBioDvAC = dsBioDvAC.Tables[0];

                GetEmpJobCardMonthlySummary(EmpIdLoop, fromDate, toDate, out dsMonthlySummary);
                dtMonthlySummary = dsMonthlySummary.Tables[0];

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                DataView dvExtraAbsentDate = null;
                objRpt.GetExtraAbsentCount(fromDate, toDate, plantId, out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);
                dvExtraAbsentDate = new DataView(dsExtraAbsent.Tables[0]);

                ParaMontlyAttendance objm = new ParaMontlyAttendance();
                dvWeeklyAbsnt = new DataView();
                objRpt.GetWeeklyAbsentismAssignment(plantId, EmpIdLoop, fromDate, toDate, out dsWeeklyAbsnt);
                dtWeeklyAbsnt = dsWeeklyAbsnt.Tables[0];
                dvWeeklyAbsnt.Table = dtWeeklyAbsnt;
                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
                objRpt.SelectedPlant(plantId, out dsFactory);
                #endregion DataSet

                if (dsBioDvAC.Tables[0].Rows.Count > 0)
                {
                    sEmpCodeColl = new StringCollection();
                    for (int i = 0; i <= dsBioDvAC.Tables[0].Rows.Count - 1; i++)
                    {
                        if (sEmpCodeColl.Contains(dsBioDvAC.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim()) == false)
                        {
                            sEmpCodeColl.Add(dsBioDvAC.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim());
                            //sEmpCodeColl.Add(dsBioDvAC.Tables[0].Rows[i][",A1,"].ToString().Trim()); EmpSystemID


                        }
                    }
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(sEmpCodeColl.Count);
                    for (int Ec = 0; Ec < sEmpCodeColl.Count; Ec++)
                    {
                        dvBioDvAC = new DataView();
                        dvBioDvAC.Table = dtBioDvAC;

                        dvSummary = new DataView();
                        dvSummary.Table = dtMonthlySummary;

                        dvBioDvAC.RowFilter = "EmployeeCode = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";
                        dvSummary.RowFilter = "EmployeeCode = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";
                        dvExtraAbsent.RowFilter = "EmployeeCode = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";

                        if (dvBioDvAC.Count > 0)
                        {
                            sheet1 = workbook.Worksheets[Ec];
                            sheet1.IsGridLinesVisible = true;
                            xlsRow = 6;
                            string strEmpCode = "";
                            int iDate = 0;
                            int iShiftIntime = 0;
                            int iInTime = 0;
                            int iOutTime = 0;
                            int iTotalOT = 0;
                            int iDayStatus = 0;
                            int iODD = 0;
                            int iLvShortName = 0;
                            string strLateBy = "00:00:00";
                            int iLateBy = 0;
                            int iShiftName = 0;
                            int iShiftOuttime = 0;
                            var iDay = 0;
                            // var iOverStay = 0;

                            string employeeName = "";
                            object chequeAmount;
                            object OverStay;
                            object totalPresentDays;
                            object totalAbsentDays;
                            object totalLateDays;
                            object totalLeaveDays;
                            object totalWeekOFFDays;
                            object totalHolidays;
                            object totalODD;
                            object totalDays;
                            object totalLWPDays;
                            object totalHalfDays;
                            object totalHalfDaysLeave;
                            object totalLeaveAbsentDays;
                            object totalAbsentLeaveDays;
                            object totalExtraAbsent;

                            chequeAmount = dvBioDvAC.ToTable().Compute(@"Sum(FinalOT)", "");
                            OverStay = dvBioDvAC.ToTable().Compute(@"Sum(OverStay)", "");
                            totalPresentDays = dvSummary.ToTable().Compute(@"Sum(TotalPresent)", null);
                            totalHalfDays = 0;
                            totalHalfDaysLeave = 0;
                            totalLeaveAbsentDays = 0;
                            totalAbsentLeaveDays = 0;
                            totalAbsentDays = dvSummary.ToTable().Compute(@"SUM(TotalAbsent)", null);
                            totalExtraAbsent = dvExtraAbsent.ToTable().Compute(@"Count(WorkingDate)", null);
                            totalLateDays = dvSummary.ToTable().Compute(@"SUM(TotalLate)", null);
                            totalLeaveDays = dvSummary.ToTable().Compute(@"SUM(TotalLv)", null);
                            totalWeekOFFDays = dvSummary.ToTable().Compute(@"SUM(TotalWeekOff)", null);
                            totalHolidays = dvSummary.ToTable().Compute(@"SUM(TotalHoliDay)", null);
                            totalODD = dvBioDvAC.ToTable().Compute(@"SUM(DurationInMin)", null);
                            totalDays = dvSummary.ToTable().Compute(@"COUNT(DayValue)", null);
                            totalLWPDays = dvSummary.ToTable().Compute(@"SUM(TotalLWP)", null);

                            for (int i = 0; i < dvBioDvAC.Count; i++)
                            {
                                if ((string.Compare(strEmpCode.ToUpper(), dvBioDvAC[i]["EmployeeCode"].ToString().Trim().ToUpper())) != 0)
                                {
                                    #region ------------------Column Header------------------
                                    employeeName = dvBioDvAC[i]["EmployeeName"].ToString().Trim();
                                    xlsCol = 1;
                                    xlsRow = 5;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Emp Code";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeCode"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Text = "Emp Name";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeName"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "DOJ";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["DOJ"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Designation";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["LegalDesignation"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Grade";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["GradeCode"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Current Status";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["EmployeeStatus"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    xlsCol = 5;
                                    xlsRow = 5;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Unit";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Unit"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Division";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Division"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Department";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Department"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Section";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Section"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "SubSection";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["SubSection"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 3].Merge();
                                    xlsRow += 1;
                                    xlsRow = 5;
                                    xlsCol = 9;

                                    #region Total
                                    //-----Total------
                                    sheet1.Range[1, 10].Text = "Job Card Summary";
                                    sheet1.Range[1, 10, 1, 10 + 2].Merge();

                                    sheet1.Range[1, 10, 1, 9 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[1, 10, 1, 9 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[1, 10, 1, 9 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[1, 10, 1, 9 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //1 += 1;

                                    sheet1.Range[2, 10].Text = "Present Days";
                                    sheet1.Range[2, 10, 2, 10 + 1].Merge();

                                    sheet1.Range[2, 10 + 2].Text = (Convert.ToDouble(totalPresentDays) + (Convert.ToDouble(totalHalfDays) * 0.5)).ToString();
                                    sheet1.Range[2, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[2, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[2, 10, 2, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[2, 10, 2, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[2, 10, 2, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;

                                    sheet1.Range[3, 10].Text = "Leave Days / LWP";
                                    sheet1.Range[3, 10, 3, 10 + 1].Merge();

                                    sheet1.Range[3, 10 + 2].Text = (Convert.ToDouble(totalLeaveDays) + (Convert.ToDouble(totalHalfDaysLeave) * 0.5) + (Convert.ToDouble(totalAbsentLeaveDays) * 0.5)).ToString() + " / " + totalLWPDays;
                                    sheet1.Range[3, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[3, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[3, 10, 3, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[3, 10, 3, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[3, 10, 3, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;
                                    sheet1.Range[4, 10].Text = "Absent Days/Extra Ab";
                                    sheet1.Range[4, 10, 4, 10 + 1].Merge();

                                    sheet1.Range[4, 10 + 2].Text = (Convert.ToDouble(totalAbsentDays) + (Convert.ToDouble(totalLeaveAbsentDays) * 0.5) - (Convert.ToDouble(totalAbsentLeaveDays) * 0.5)).ToString() + " / " + totalExtraAbsent;
                                    sheet1.Range[4, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[4, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[4, 10, 4, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[4, 10, 4, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[4, 10, 4, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    ///xlsRow += 1;
                                    sheet1.Range[5, 10].Text = "Total Weekoffs";
                                    sheet1.Range[5, 10, 5, 10 + 1].Merge();

                                    sheet1.Range[5, 10 + 2].Text = totalWeekOFFDays.ToString();
                                    sheet1.Range[5, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[5, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[5, 10, 5, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[5, 10, 5, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[5, 10, 5, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;
                                    sheet1.Range[6, 10].Text = "Late";
                                    sheet1.Range[6, 10, 6, 10 + 1].Merge();

                                    sheet1.Range[6, 10 + 2].Text = totalLateDays.ToString();
                                    sheet1.Range[6, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[6, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[6, 10, 6, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[6, 10, 6, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[6, 10, 6, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    //xlsRow += 1;
                                    sheet1.Range[7, 10].Text = "Holidays";
                                    sheet1.Range[7, 10, 7, 10 + 1].Merge();

                                    sheet1.Range[7, 10 + 2].Text = totalHolidays.ToString();
                                    sheet1.Range[7, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[7, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[7, 10, 7, 10 + 2].CellStyle.Font.Bold = true;
                                    sheet1.Range[7, 10, 7, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[7, 10, 7, 10 + 2].BorderAround(ExcelLineStyle.Hair);

                                    if (chkAdditionInfo == true)
                                    {
                                        //xlsRow += 1;
                                        sheet1.Range[8, 10].Text = "ODD(Hours):";
                                        sheet1.Range[8, 10, 8, 10 + 1].Merge();


                                        string zot = string.Empty;
                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[i]["OTConsiderOn"].ToString(), totalODD.ToString(), out zot);
                                        sheet1.Range[8, 10 + 2].Text = zot;

                                        sheet1.Range[8, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                        sheet1.Range[8, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[8, 10, 8, 10 + 2].CellStyle.Font.Bold = true;
                                        sheet1.Range[8, 10, 8, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        sheet1.Range[8, 10, 8, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    }

                                    if (chkAdditionInfo == true)
                                    {
                                        //xlsRow += 1;
                                        sheet1.Range[9, 10].Text = "Total OT Hour";
                                        sheet1.Range[9, 10, 9, 10 + 1].Merge();

                                        string TotalOt = string.Empty;//OTConsiderOn
                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), chequeAmount.ToString(), out TotalOt);

                                        sheet1.Range[9, 10 + 2].Text = TotalOt;

                                        sheet1.Range[9, 10].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                        sheet1.Range[9, 10 + 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[9, 10, 9, 10 + 2].CellStyle.Font.Bold = true;
                                        sheet1.Range[9, 10, 9, 10 + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        sheet1.Range[9, 10, 9, 10 + 2].BorderAround(ExcelLineStyle.Hair);
                                    }
                                    //----End Total--- 
                                    #endregion


                                    xlsRow = 11;
                                    xlsCol = 1;
                                    iDate = xlsCol;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, iDate].Text = "Date";
                                    sheet1.Range[xlsRow, iDate].ColumnWidth = 11;
                                    sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iDay = xlsCol;
                                    sheet1.Range[xlsRow, iDay].Text = "Day";
                                    sheet1.Range[xlsRow, iDay].ColumnWidth = 7;
                                    sheet1.Range[xlsRow, iDay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDay].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    iShiftName = xlsCol;
                                    sheet1.Range[xlsRow, iShiftName].Text = "Shift Name";
                                    sheet1.Range[xlsRow, iShiftName].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iShiftIntime = xlsCol;
                                    sheet1.Range[xlsRow, iShiftIntime].Text = "Shift InTime";
                                    sheet1.Range[xlsRow, iShiftIntime].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iShiftOuttime = xlsCol;
                                    sheet1.Range[xlsRow, iShiftOuttime].Text = "Shift OutTime";
                                    sheet1.Range[xlsRow, iShiftOuttime].ColumnWidth = 9;
                                    sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iInTime = xlsCol;
                                    sheet1.Range[xlsRow, iInTime].Text = "InTime";
                                    sheet1.Range[xlsRow, iInTime].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iOutTime = xlsCol;
                                    sheet1.Range[xlsRow, iOutTime].Text = "OutTime";
                                    sheet1.Range[xlsRow, iOutTime].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iDayStatus = xlsCol;
                                    sheet1.Range[xlsRow, iDayStatus].Text = "Day Status";
                                    sheet1.Range[xlsRow, iDayStatus].ColumnWidth = 6.5;
                                    sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    iLateBy = xlsCol;
                                    sheet1.Range[xlsRow, iLateBy].Text = "Late By";
                                    sheet1.Range[xlsRow, iLateBy].ColumnWidth = 9.5;
                                    sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    iLvShortName = xlsCol;
                                    sheet1.Range[xlsRow, iLvShortName].Text = "LV";
                                    sheet1.Range[xlsRow, iLvShortName].ColumnWidth = 9;
                                    sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    if (chkAdditionInfo == true)
                                    {
                                        xlsCol += 1;
                                        iODD = xlsCol;
                                        sheet1.Range[xlsRow, iODD].Text = "ODD";
                                        sheet1.Range[xlsRow, iODD].ColumnWidth = 9;
                                        sheet1.Range[xlsRow, iODD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iODD].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    }

                                    if (chkAdditionInfo == true)
                                    {
                                        //xlsCol += 1;
                                        //iOverStay = xlsCol;
                                        //sheet1.Range[xlsRow, iOverStay].Text = "Over Stay";
                                        //sheet1.Range[xlsRow, iOverStay].ColumnWidth = 9;
                                        //sheet1.Range[xlsRow, iOverStay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        //sheet1.Range[xlsRow, iOverStay].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                        xlsCol += 1;
                                        iTotalOT = xlsCol;
                                        sheet1.Range[xlsRow, iTotalOT].Text = "Final OT Hour";
                                        sheet1.Range[xlsRow, iTotalOT].ColumnWidth = 11.5;
                                        sheet1.Range[xlsRow, iTotalOT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iTotalOT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                    ///sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    endXlsCol = xlsCol;

                                    freezeRow = xlsRow.ToString();
                                    #endregion ------------------Column Header------------------
                                }
                                strEmpCode = dvBioDvAC[i]["EmployeeCode"].ToString().Trim();

                                #region ----------------------Data-----------------------

                                xlsRow += 1;
                                sheet1.Range[xlsRow, iDate].Text = dvBioDvAC[i]["PDate"].ToString();
                                sheet1.Range[xlsRow, iDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iShiftName].Text = dvBioDvAC[i]["ShiftName"].ToString();
                                sheet1.Range[xlsRow, iShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iShiftIntime].Text = dvBioDvAC[i]["ShiftInTimeShow"].ToString();
                                sheet1.Range[xlsRow, iShiftIntime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftIntime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iShiftOuttime].Text = dvBioDvAC[i]["ShiftOutTime"].ToString();
                                sheet1.Range[xlsRow, iShiftOuttime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iShiftOuttime].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                if (chkAdditionInfo == true)
                                {
                                    if (!string.IsNullOrEmpty(dvBioDvAC[i]["DurationInMin"].ToString()))
                                    {
                                        string yot = string.Empty;
                                        oru.GetOT(dsBioDvAC.Tables[0].Rows[i]["OTConsiderOn"].ToString(), dsBioDvAC.Tables[0].Rows[i]["DurationInMin"].ToString(), out yot);

                                        sheet1.Range[xlsRow, iODD].Text = yot;
                                        sheet1.Range[xlsRow, iODD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iODD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }

                                }

                                sheet1.Range[xlsRow, iDay].Text = dvBioDvAC[i]["PDay"].ToString().Substring(0, 3);
                                sheet1.Range[xlsRow, iDay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iDay].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                if (dvBioDvAC[i]["InTimeShow"].ToString() != "")
                                {
                                    sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                    sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dvBioDvAC[i]["InTimeShow"].ToString());
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                    sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dvBioDvAC[i]["InTimeShow"].ToString());
                                    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                }

                                if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsManualInTime"].ToString().Trim()))
                                {
                                    sheet1.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Grey_80_percent;
                                }

                                if (dvBioDvAC[i]["OutTimeShow"].ToString() != "")
                                {
                                    ///=============================
                                    ///1.if OT not applicable and
                                    ///2. out time > slab based outtime
                                    if (Convert.ToBoolean(dvBioDvAC[i]["IsOTEntitled"].ToString()))
                                    {
                                        sheet1.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                        sheet1.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(dvBioDvAC[i]["OutTimeShow"].ToString());
                                        sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                    else
                                    {
                                        string FinalOutTime = string.Empty;
                                        //_getPunchTime(dvBioDvAC[i].Row, out FinalOutTime);

                                        //if (Convert.ToDateTime(FinalOutTime) > Convert.ToDateTime(dvBioDvAC[i]["punchTime"].ToString()))
                                        //{
                                        FinalOutTime = dvBioDvAC[i]["OutTimeShow"].ToString();
                                        //}

                                        sheet1.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                        sheet1.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(FinalOutTime);
                                        sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }

                                }

                                if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsManualOutTime"].ToString().Trim()))
                                {
                                    sheet1.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Grey_80_percent;
                                }


                                sheet1.Range[xlsRow, iDayStatus].Text = dvBioDvAC[i]["DayStatus"].ToString().Trim();
                                sheet1.Range[xlsRow, iDayStatus].RowHeight = 13;
                                sheet1.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                var Date = dvBioDvAC[i]["PDate"].ToString().Trim();
                                var EmpCode = dvBioDvAC[i]["EmployeeCode"].ToString().Trim();

                                dvWeeklyAbsnt.RowFilter = "EmployeeCode = '" + EmpCode + "' AND  WorkingDate = '" + Date + "'";


                                if (dvWeeklyAbsnt.Count > 0)
                                {
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;
                                }

                                #region Extra Absent Colore

                                dvExtraAbsentDate.RowFilter = "EmployeeCode = '" + EmpCode + "' AND  WorkingDate = '" + Date + "'";

                                bool IsExtraAbsent = false;
                                if (dvExtraAbsentDate.Count > 0)
                                {
                                    IsExtraAbsent = true;
                                }
                                if (IsExtraAbsent)
                                {
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;
                                    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Bold = true;

                                }
                                #endregion

                                #region Unnecessary Validation Commented By Dhruv

                                //if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsManualDayStatus"].ToString().Trim()))
                                //{
                                //    sheet1.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Grey_80_percent;

                                //    sheet1.Range[xlsRow, iOutTime].Text = "";
                                //    sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                //    sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                //    sheet1.Range[xlsRow, iInTime].Text = "";
                                //    sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                //    sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                //    sheet1.Range[xlsRow, iLateBy].Text = "";
                                //    sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                //    sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                //}

                                #endregion

                                if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "L")
                                {
                                    #region Late by min                                  
                                    sInTime = "00:00:00";
                                    if (dvBioDvAC[i]["InTimelate"].ToString().Trim() != "")
                                    {
                                        sInTime = dvBioDvAC[i]["InTimelate"].ToString().Trim() + ":00";
                                    }
                                    else
                                    {
                                        if (dvBioDvAC[i]["OutTimelate"].ToString().Trim() != "")
                                        {
                                            sInTime = dvBioDvAC[i]["OutTimelate"].ToString().Trim() + ":00";
                                        }
                                    }
                                    sOfficeInTime = "00:00:00";
                                    strLateBy = "00:00";
                                    if (dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim() != "" && sInTime != "00:00:00")
                                    {
                                        sOfficeInTime = dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim();
                                        strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                    }

                                    #endregion Late by min
                                }
                                else
                                {
                                    ///absent by how min

                                    #region Absent by how much min

                                    if (dvBioDvAC[i]["DayStatus"].ToString().Trim() == "A")
                                    {
                                        sInTime = "00:00:00";
                                        if (dvBioDvAC[i]["InTimelate"].ToString().Trim() != "")
                                        {
                                            sInTime = dvBioDvAC[i]["InTimelate"].ToString().Trim() + ":00";
                                            sOfficeInTime = "00:00:00";
                                            strLateBy = "00:00";
                                            if (dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim() != "" && sInTime != "00:00:00")
                                            {
                                                sOfficeInTime = dvBioDvAC[i]["ShiftInTimeLate"].ToString().Trim();
                                                strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                            }
                                        }
                                        else
                                        {
                                            strLateBy = "";
                                        }
                                    }
                                    else
                                    {
                                        strLateBy = "";
                                    }

                                    #endregion Absent by how much min
                                }

                                //paid days

                                DateTime _ddd = Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString());

                                string dti = dvBioDvAC[i]["dti"].ToString().Trim();
                                string dto = dvBioDvAC[i]["dto"].ToString().Trim();
                                string _InTimeShow = dvBioDvAC[i]["InTimeShow"].ToString().Trim();
                                string _OutTimeShow = dvBioDvAC[i]["OutTimeShow"].ToString().Trim();

                                sheet1.Range[xlsRow, iLateBy].Text = strLateBy;
                                sheet1.Range[xlsRow, iLateBy].CellStyle.Font.Color = ExcelKnownColors.Blue;
                                sheet1.Range[xlsRow, iLateBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iLateBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (!string.IsNullOrEmpty(dvBioDvAC[i]["Code"].ToString()))
                                {
                                    sheet1.Range[xlsRow, iLvShortName].Text = dvBioDvAC[i]["Code"].ToString() + "(" + dvBioDvAC[i]["LeaveDuration"].ToString() + ")";
                                    sheet1.Range[xlsRow, iLvShortName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iLvShortName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }

                                var sl = dvBioDvAC[i]["ShortLeave"].ToString();
                                if (sl == "0")
                                {
                                    sl = null;
                                }

                                if (chkAdditionInfo == true)
                                {
                                    string yot = string.Empty;//OTConsiderOn
                                    string overstay = string.Empty;
                                    if (bplib.clsWebLib.GetBoolData(dvBioDvAC[i]["IsOTEntitled"].ToString()) == true)
                                    {
                                        if (string.IsNullOrEmpty(dvBioDvAC[i]["FinalOT"].ToString()))
                                        {
                                            if (!string.IsNullOrEmpty(dvBioDvAC[i]["DayCategory"].ToString()))
                                            {

                                            }

                                        }
                                        else
                                        {

                                        }
                                        if (dvBioDvAC[i]["OutTimeShow"].ToString() != "")
                                        {
                                            if (!string.IsNullOrEmpty(dvBioDvAC[i]["DayCategory"].ToString()))
                                            {
                                                if (dvBioDvAC[i]["DayCategory"].ToString() == "Present" || dvBioDvAC[i]["DayCategory"].ToString() == "Late")
                                                {
                                                    oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), dvBioDvAC[i]["FinalOT"].ToString(), out yot);
                                                    oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), dvBioDvAC[i]["OverStay"].ToString(), out overstay);

                                                }
                                            }
                                        }


                                    }

                                    //sheet1.Range[xlsRow, iOverStay].Text = overstay;

                                    //sheet1.Range[xlsRow, iOverStay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    //sheet1.Range[xlsRow, iOverStay].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, iTotalOT].Text = yot;

                                    sheet1.Range[xlsRow, iTotalOT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iTotalOT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }

                                #endregion ----------------------Data-----------------------

                                #region Line Setup

                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                                #endregion Line Setup
                            }


                            if (chkAdditionInfo == true)
                            {
                                xlsRow++;
                                //string overstay = string.Empty;
                                //oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), OverStay.ToString(), out overstay);
                                //string tost = overstay;
                                //sheet1.Range[xlsRow, iOverStay].Text = overstay;

                                //sheet1.Range[xlsRow, iOverStay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                //sheet1.Range[xlsRow, iOverStay].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                string TotalOt = string.Empty;//OTConsiderOn
                                oru.GetOT(dsBioDvAC.Tables[0].Rows[0]["OTConsiderOn"].ToString(), chequeAmount.ToString(), out TotalOt);
                                string tt = TotalOt;
                                sheet1.Range[xlsRow, iTotalOT].Text = TotalOt;

                                sheet1.Range[xlsRow, iTotalOT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTotalOT].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iTotalOT - 2].Text = "Total ";

                                sheet1.Range[xlsRow, iTotalOT - 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTotalOT - 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                sheet1.Range[xlsRow, iTotalOT - 2, xlsRow, iTotalOT].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, iTotalOT - 2, xlsRow, iTotalOT].BorderInside(ExcelLineStyle.Hair);

                            }
                            xlsRow += 3;

                            xlsRow += 5;
                            sheet1.Range[xlsRow, iDate].Text = employeeName;
                            sheet1.Range[xlsRow, iDate, xlsRow, iShiftName].Merge();// = "Signature";
                            sheet1.Range[xlsRow, iDate, xlsRow, iShiftName].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, iDate, xlsRow, iShiftName].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thick;

                            sheet1.Range[xlsRow, iDate, xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iDate, xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.IsDisplayZeros = false;

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
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 17;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 20;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                            sheet1.Range[xlsRow, 3].Text = FactoryName;
                            //sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 25;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 15;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            xlsRow += 1;
                            sheet1.Range[xlsRow, 3].Text = "Employee Job Card Information From Date: " + fromDate + " To Date: " + toDate;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].Merge();
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].RowHeight = 20;
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, iLateBy].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            #endregion ******************Report Header******************

                            #region Freeze Panes
                            if (chkAdditionInfo == true)
                            {
                                sheet1.IsDisplayZeros = false;
                                sheet1.UsedRange["A13"].FreezePanes();
                            }
                            else
                            {
                                sheet1.IsDisplayZeros = false;
                                sheet1.UsedRange["A13"].FreezePanes();
                            }

                            #endregion Freeze Panes

                            #region UsedRange Alignment

                            sheet1.UsedRange.WrapText = true;
                            sheet1.UsedRange.CellStyle.Font.Size = 8;
                            sheet1.Range["A1"].CellStyle.Font.Size = 14;
                            sheet1.Range["A2"].CellStyle.Font.Size = 10;
                            sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                            #endregion UsedRange Alignment

                            #region Page Setup
                            sheet1.PageSetup.TopMargin = 0.5;
                            sheet1.PageSetup.BottomMargin = 0.7;
                            sheet1.PageSetup.PrintTitleRows = "$1:$11";
                            sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                            sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                            sheet1.PageSetup.LeftMargin = 0.5;
                            sheet1.PageSetup.RightMargin = 0.2;
                            sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                            sheet1.PageSetup.FitToPagesTall = 0;
                            sheet1.PageSetup.FitToPagesWide = 1;
                            sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                            sheet1.IsDisplayZeros = false;

                            sheet1.Name = sEmpCodeColl[Ec].ToString().Trim();

                            #endregion Page Setup

                        }

                    }

                    return workbook;
                }
                else
                {
                    Exception ex = new Exception("No data found...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsBioDvAC = null;
                dvBioDvAC = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }


        }


        //order wise report function

        public IWorkbook GetOrderWiseParameterJobCardReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string IssueId, string ProductionOrderId, string LotNumber, string EntityId, string QualityStatus, string Date)
        {

            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsBioDvAC = null;
            DataSet dsPO = null;
            DataTable dtBioDvAC = null;
            DataView dvBioDvAC = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            StringCollection sEmpCodeColl = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            var workbook = oru.GetWorkbook(ref excelEngine, 2);
            workbook.Version = ExcelVersion.Excel2013;
            IWorksheet sheet = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string freezeRow = "";
            try
            {
                #region DataSet
                objRpt = new clsReport();
                GetOrderWiseParameterJobCardReport(fromDate, toDate, IssueId, ProductionOrderId, LotNumber, EntityId, QualityStatus, Date, plantId, out dsBioDvAC);
                dtBioDvAC = dsBioDvAC.Tables[0];

                GetProductionDatabyPOId(ProductionOrderId, out dsPO);

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
                objRpt.SelectedPlant(plantId, out dsFactory);
                #endregion DataSet

                if (dsBioDvAC.Tables[0].Rows.Count > 0)
                {
                    sEmpCodeColl = new StringCollection();
                    for (int i = 0; i <= dsBioDvAC.Tables[0].Rows.Count - 1; i++)
                    {
                        if (sEmpCodeColl.Contains(dsBioDvAC.Tables[0].Rows[i]["PONo"].ToString().Trim()) == false)
                        {
                            sEmpCodeColl.Add(dsBioDvAC.Tables[0].Rows[i]["PONo"].ToString().Trim());

                        }
                    }
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(2);
                    for (int Ec = 0; Ec < sEmpCodeColl.Count; Ec++)
                    {
                        dvBioDvAC = new DataView();
                        dvBioDvAC.Table = dtBioDvAC;
                        dvBioDvAC.RowFilter = "PONo = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";

                        if (dvBioDvAC.Count > 0)
                        {
                            sheet1 = workbook.Worksheets[Ec];
                            sheet1.IsGridLinesVisible = true;
                            xlsRow = 5;
                            string strEmpCode = "";
                            int QPIssueName = 0;
                            int QPProcess = 0;
                            //int QPPSNo = 0;
                            int QPPName = 0;
                            int QPUOM = 0;
                            int QPValue = 0;
                            int QPMinMaxReq = 0;
                            int QPMinMaxStd = 0;
                            int QPGradeName = 0;
                            int QPActionTeBeTaken = 0;
                            int QPResponsiblePerson = 0;
                            int QPActionTaken = 0;
                            int QPActionBy = 0;
                            int QPParameterRemarks = 0;
                            int QPDate = 0;
                            int QPTime = 0;
                            int QPConfirmRemarks = 0;
                            int QPQAURemarks = 0;
                            int QPReason = 0;

                            for (int i = 0; i < dvBioDvAC.Count; i++)
                            {
                                if ((string.Compare(strEmpCode.ToUpper(), dvBioDvAC[i]["PONo"].ToString().Trim().ToUpper())) != 0)
                                {
                                    #region ------------------Column Header------------------

                                    xlsCol = 1;
                                    xlsRow = 3;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Quality Status";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["QualityStatus"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Text = "Date";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Date"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "MO Line ItemNo";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["MOLineItemNo"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "PO Status";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["POStatus"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Lot No";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["LotNumber"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();
                                    xlsRow += 1;
                                    xlsCol = 4;
                                    xlsRow = 3;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Customer";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Customer"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 5].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "PO No";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["PONo"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 5].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Article";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Article"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 5].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Grade - Added By";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Grade"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 5].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Comment";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["CommentDetails"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 4].Merge();

                                    xlsRow = 8;
                                    xlsCol = 1;
                                    QPIssueName = xlsCol;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, QPIssueName].Text = "IssueName";
                                    sheet1.Range[xlsRow, QPIssueName].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, QPIssueName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPIssueName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPProcess = xlsCol;
                                    sheet1.Range[xlsRow, QPProcess].Text = "Process";
                                    sheet1.Range[xlsRow, QPProcess].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    QPPName = xlsCol;
                                    sheet1.Range[xlsRow, QPPName].Text = "PName";
                                    sheet1.Range[xlsRow, QPPName].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPPName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPPName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    //xlsCol += 1;
                                    //QPPSNo = xlsCol;
                                    //sheet1.Range[xlsRow, QPPSNo].Text = "PSNo";
                                    //sheet1.Range[xlsRow, QPPSNo].ColumnWidth = 8;
                                    //sheet1.Range[xlsRow, QPPSNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    //sheet1.Range[xlsRow, QPPSNo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPDate = xlsCol;
                                    sheet1.Range[xlsRow, QPDate].Text = "Date";
                                    sheet1.Range[xlsRow, QPDate].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPTime = xlsCol;
                                    sheet1.Range[xlsRow, QPTime].Text = "Time";
                                    sheet1.Range[xlsRow, QPTime].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPUOM = xlsCol;
                                    sheet1.Range[xlsRow, QPUOM].Text = "UOM";
                                    sheet1.Range[xlsRow, QPUOM].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, QPUOM].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPUOM].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPValue = xlsCol;
                                    sheet1.Range[xlsRow, QPValue].Text = "Value";
                                    sheet1.Range[xlsRow, QPValue].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, QPValue].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPValue].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPMinMaxReq = xlsCol;
                                    sheet1.Range[xlsRow, QPMinMaxReq].Text = "Min/MaxReq";
                                    sheet1.Range[xlsRow, QPMinMaxReq].ColumnWidth = 10;
                                    sheet1.Range[xlsRow, QPMinMaxReq].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPMinMaxReq].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPMinMaxStd = xlsCol;
                                    sheet1.Range[xlsRow, QPMinMaxStd].Text = "Min/MaxStd";
                                    sheet1.Range[xlsRow, QPMinMaxStd].ColumnWidth = 10;
                                    sheet1.Range[xlsRow, QPMinMaxStd].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPMinMaxStd].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPGradeName = xlsCol;
                                    sheet1.Range[xlsRow, QPGradeName].Text = "GradeName";
                                    sheet1.Range[xlsRow, QPGradeName].ColumnWidth = 10;
                                    sheet1.Range[xlsRow, QPGradeName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPGradeName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPActionTeBeTaken = xlsCol;
                                    sheet1.Range[xlsRow, QPActionTeBeTaken].Text = "ActionTeBeTaken";
                                    sheet1.Range[xlsRow, QPActionTeBeTaken].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, QPActionTeBeTaken].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPActionTeBeTaken].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPResponsiblePerson = xlsCol;
                                    sheet1.Range[xlsRow, QPResponsiblePerson].Text = "ResponsiblePerson";
                                    sheet1.Range[xlsRow, QPResponsiblePerson].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPResponsiblePerson].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPResponsiblePerson].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPActionTaken = xlsCol;
                                    sheet1.Range[xlsRow, QPActionTaken].Text = "ActionTaken";
                                    sheet1.Range[xlsRow, QPActionTaken].ColumnWidth = 10;
                                    sheet1.Range[xlsRow, QPActionTaken].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPActionTaken].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPActionBy = xlsCol;
                                    sheet1.Range[xlsRow, QPActionBy].Text = "ActionBy";
                                    sheet1.Range[xlsRow, QPActionBy].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPActionBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPActionBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPParameterRemarks = xlsCol;
                                    sheet1.Range[xlsRow, QPParameterRemarks].Text = "ParameterRemarks";
                                    sheet1.Range[xlsRow, QPParameterRemarks].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPParameterRemarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPParameterRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPConfirmRemarks = xlsCol;
                                    sheet1.Range[xlsRow, QPConfirmRemarks].Text = "ConfirmRemarks";
                                    sheet1.Range[xlsRow, QPConfirmRemarks].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPConfirmRemarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPConfirmRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPQAURemarks = xlsCol;
                                    sheet1.Range[xlsRow, QPQAURemarks].Text = "QAURemarks";
                                    sheet1.Range[xlsRow, QPQAURemarks].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPQAURemarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPQAURemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPReason = xlsCol;
                                    sheet1.Range[xlsRow, QPReason].Text = "Reason";
                                    sheet1.Range[xlsRow, QPReason].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPReason].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPReason].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    endXlsCol = xlsCol;

                                    freezeRow = xlsRow.ToString();
                                    #endregion ------------------Column Header------------------
                                }
                                strEmpCode = dvBioDvAC[i]["PONo"].ToString().Trim();

                                #region ----------------------Data-----------------------

                                xlsRow += 1;
                                sheet1.Range[xlsRow, QPIssueName].Text = dvBioDvAC[i]["IssueName"].ToString();
                                sheet1.Range[xlsRow, QPIssueName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPIssueName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPProcess].Text = dvBioDvAC[i]["Process"].ToString();
                                sheet1.Range[xlsRow, QPProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                //sheet1.Range[xlsRow, QPPSNo].Text = dvBioDvAC[i]["ParameterSequence"].ToString();
                                //sheet1.Range[xlsRow, QPPSNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                //sheet1.Range[xlsRow, QPPSNo].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                sheet1.Range[xlsRow, QPPName].Text = dvBioDvAC[i]["ParameterName"].ToString();
                                sheet1.Range[xlsRow, QPPName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPPName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPDate].Text = dvBioDvAC[i]["QCDDate"].ToString();
                                sheet1.Range[xlsRow, QPDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPTime].Text = dvBioDvAC[i]["QCDTime"].ToString();
                                sheet1.Range[xlsRow, QPTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPUOM].Text = dvBioDvAC[i]["UOM"].ToString();
                                sheet1.Range[xlsRow, QPUOM].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPUOM].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPValue].Text = dvBioDvAC[i]["Value"].ToString();
                                sheet1.Range[xlsRow, QPValue].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPValue].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPMinMaxReq].Text = dvBioDvAC[i]["MinMaxRequirement"].ToString();
                                sheet1.Range[xlsRow, QPMinMaxReq].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPMinMaxReq].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPMinMaxStd].Text = dvBioDvAC[i]["MinMaxStandard"].ToString();
                                sheet1.Range[xlsRow, QPMinMaxStd].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPMinMaxStd].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPGradeName].Text = dvBioDvAC[i]["GradeName"].ToString();
                                sheet1.Range[xlsRow, QPGradeName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPGradeName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPActionTeBeTaken].Text = dvBioDvAC[i]["ActionToBeTakenName"].ToString();
                                sheet1.Range[xlsRow, QPActionTeBeTaken].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPActionTeBeTaken].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPResponsiblePerson].Text = dvBioDvAC[i]["ResponsiblePerson"].ToString();
                                sheet1.Range[xlsRow, QPResponsiblePerson].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPResponsiblePerson].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPActionTaken].Text = dvBioDvAC[i]["ActionTaken"].ToString();
                                sheet1.Range[xlsRow, QPActionTaken].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPActionTaken].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPActionBy].Text = dvBioDvAC[i]["ActionBy"].ToString();
                                sheet1.Range[xlsRow, QPActionBy].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPActionBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPParameterRemarks].Text = dvBioDvAC[i]["ParameterRemark"].ToString();
                                sheet1.Range[xlsRow, QPParameterRemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPParameterRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPConfirmRemarks].Text = dvBioDvAC[i]["ConfirmRemarks"].ToString();
                                sheet1.Range[xlsRow, QPConfirmRemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPConfirmRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPQAURemarks].Text = dvBioDvAC[i]["QAURemarks"].ToString();
                                sheet1.Range[xlsRow, QPQAURemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPQAURemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPReason].Text = dvBioDvAC[i]["ReasonName"].ToString();
                                sheet1.Range[xlsRow, QPReason].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPReason].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                #endregion ----------------------Data-----------------------

                                #region Line Setup

                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                                #endregion Line Setup
                            }

                            xlsRow += 3;

                            xlsRow += 5;


                            sheet1.IsDisplayZeros = false;

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
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 17;
                            sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                            #endregion ******************Report Header******************

                            #region UsedRange Alignment

                            sheet1.UsedRange.WrapText = true;
                            sheet1.UsedRange.CellStyle.Font.Size = 8;
                            sheet1.Range["A1"].CellStyle.Font.Size = 14;
                            sheet1.Range["A2"].CellStyle.Font.Size = 10;
                            sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                            #endregion UsedRange Alignment

                            #region Page Setup
                            sheet1.PageSetup.TopMargin = 0.5;
                            sheet1.PageSetup.BottomMargin = 0.7;
                            sheet1.PageSetup.PrintTitleRows = "$1:$11";
                            sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                            sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                            sheet1.PageSetup.LeftMargin = 0.5;
                            sheet1.PageSetup.RightMargin = 0.2;
                            sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                            sheet1.PageSetup.FitToPagesTall = 0;
                            sheet1.PageSetup.FitToPagesWide = 1;
                            sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                            sheet1.IsDisplayZeros = false;

                            sheet1.Name = sEmpCodeColl[Ec].ToString().Trim();

                            #endregion Page Setup

                        }

                    }

                    #region PO & Lot wise Qty
                    sheet = workbook.Worksheets[1];
                    int ROW = 1; int COL = 1;

                    #region ColumnsHeader

                    sheet[ROW, COL].Text = "Process Set Seq"; sheet[ROW, COL].ColumnWidth = 12; int colPSS = COL; COL++;
                    sheet[ROW, COL].Text = "Process"; sheet[ROW, COL].ColumnWidth = 11; int colP = COL; COL++;
                    sheet[ROW, COL].Text = "IsBase Process"; sheet[ROW, COL].ColumnWidth = 12; int colBP = COL; COL++;
                    sheet[ROW, COL].Text = "Production Date"; sheet[ROW, COL].ColumnWidth = 12; int colPD = COL; COL++;
                    sheet[ROW, COL].Text = "Work Center"; sheet[ROW, COL].ColumnWidth = 14; int colWC = COL; COL++;
                    sheet[ROW, COL].Text = "Qty"; sheet[ROW, COL].ColumnWidth = 10; int colQty = COL; COL++;
                    sheet[ROW, COL].Text = "Responsible Person"; sheet[ROW, COL].ColumnWidth = 20; int colRP = COL; COL++;
                    sheet[ROW, COL].Text = "Remark"; sheet[ROW, COL].ColumnWidth = 30; int colRemark = COL;
                    sheet[ROW, COL].Text = "PO No"; sheet[ROW, COL].ColumnWidth = 10; int colPONo = COL; COL++;
                    sheet[ROW, COL].Text = "Lot No"; sheet[ROW, COL].ColumnWidth = 10; int colLN = COL;

                    int endCol = COL;
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.White;
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                    #endregion columns

                    ROW++;
                    int startRow = ROW;

                    #region DataPlot
                    for (int i = 0; i < dsPO.Tables[0].Rows.Count; i++)
                    {
                        sheet[ROW, colPSS].Text = dsPO.Tables[0].Rows[i]["ProcessSetSeq"].ToString();
                        sheet[ROW, colP].Text = dsPO.Tables[0].Rows[i]["Process"].ToString();
                        sheet[ROW, colBP].Text = dsPO.Tables[0].Rows[i]["IsBaseProcess"].ToString();
                        sheet[ROW, colPD].Text = dsPO.Tables[0].Rows[i]["ProductionDate"].ToString();
                        sheet[ROW, colWC].Text = dsPO.Tables[0].Rows[i]["WorkCenterMaster"].ToString();
                        sheet[ROW, colQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dsPO.Tables[0].Rows[i]["Quantity"].ToString());
                        sheet.Range[ROW, colQty].VerticalAlignment = ExcelVAlign.VAlignTop;
                        sheet.Range[ROW, colQty].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet[ROW, colRP].Text = dsPO.Tables[0].Rows[i]["ResponsiblePerson"].ToString();
                        sheet[ROW, colRemark].Text = dsPO.Tables[0].Rows[i]["Remarks"].ToString();
                        sheet[ROW, colPONo].Text = dsPO.Tables[0].Rows[i]["PONo"].ToString();
                        sheet[ROW, colLN].Text = dsPO.Tables[0].Rows[i]["LotNo"].ToString();


                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                        ROW++;
                    }
                    #endregion
                    int edCRow = ROW;

                    #region ReportHeader

                    sheet.AutoFilters.FilterRange = sheet.Range[colPSS, 1, 1, endCol];
                    //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[colPSS, 1, ROW, endCol]);
                    ////Apply custom table style
                    //ITableStyles tableStyles = workbook.TableStyles;
                    //ITableStyle tableStyle = tableStyles.Add("Table Style 1");
                    //ITableStyleElements tableStyleElements = tableStyle.TableStyleElements;
                    //ITableStyleElement tableStyleElement = tableStyleElements.Add(ExcelTableStyleElementType.SecondColumnStripe);
                    //tableStyleElement.BackColorRGB = Color.FromArgb(217, 225, 242);

                    //ITableStyleElement tableStyleElement1 = tableStyleElements.Add(ExcelTableStyleElementType.FirstColumn);
                    //tableStyleElement1.FontColorRGB = Color.FromArgb(128, 128, 128);

                    //ITableStyleElement tableStyleElement2 = tableStyleElements.Add(ExcelTableStyleElementType.HeaderRow);
                    //tableStyleElement2.FontColor = ExcelKnownColors.White;
                    //tableStyleElement2.BackColorRGB = Color.FromArgb(0, 112, 192);


                    //table.TableStyleName = tableStyle.Name;

                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.UsedRange.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    sheet["A" + startRow.ToString()].FreezePanes();

                    ReportUtility reportUtility = new ReportUtility();
                    //reportUtility.PlantHeader(ref sheet, endCol, "Production Data Report", plantId);
                    reportUtility.PageSetup(ref sheet, 1, ExcelPageOrientation.Landscape);
                    sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[1, 1, 1, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.IsGridLinesVisible = false;
                    sheet.Name = "ProductionData";
                    sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                    sheet.PageSetup.TopMargin = 0.2;
                    sheet.PageSetup.BottomMargin = 0.8;
                    sheet.PageSetup.LeftMargin = 0.2;
                    sheet.PageSetup.RightMargin = 0.2;
                    sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet.PageSetup.FitToPagesTall = 0;
                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet.PageSetup.CenterHorizontally = true;
                    #endregion

                    #endregion


                    return workbook;
                }
                else
                {
                    Exception ex = new Exception("No data found...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsBioDvAC = null;
                dsPO = null;
                dvBioDvAC = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }


        }


        // Daily Quality Status report function
        public IWorkbook GetDailyQualityStatusParameterJobCardReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string fromDate, string toDate, string IssueId, string ProductionOrderId, string LotNumber, string EntityId, string QualityStatus, string Date)
        {

            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsBioDvAC, dsPO = null;
            DataTable dtBioDvAC = null;
            DataView dvBioDvAC = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            StringCollection sEmpCodeColl = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            var workbook = oru.GetWorkbook(ref excelEngine, 2);
            workbook.Version = ExcelVersion.Excel2013;
            IWorksheet sheet1 = null;
            IWorksheet sheet = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string freezeRow = "";
            try
            {
                #region DataSet
                objRpt = new clsReport();
                GetDailyQualityStatusParameterJobCardReport(fromDate, toDate, IssueId, ProductionOrderId, LotNumber, EntityId, QualityStatus, Date, plantId, out dsBioDvAC);
                dtBioDvAC = dsBioDvAC.Tables[0];

                GetProductionDatabyPOId(ProductionOrderId, out dsPO);

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
                objRpt.SelectedPlant(plantId, out dsFactory);
                #endregion DataSet

                if (dsBioDvAC.Tables[0].Rows.Count > 0)
                {
                    sEmpCodeColl = new StringCollection();
                    for (int i = 0; i <= dsBioDvAC.Tables[0].Rows.Count - 1; i++)
                    {
                        if (sEmpCodeColl.Contains(dsBioDvAC.Tables[0].Rows[i]["PONo"].ToString().Trim()) == false)
                        {
                            sEmpCodeColl.Add(dsBioDvAC.Tables[0].Rows[i]["PONo"].ToString().Trim());

                        }
                    }
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(2);
                    for (int Ec = 0; Ec < sEmpCodeColl.Count; Ec++)
                    {
                        dvBioDvAC = new DataView();
                        dvBioDvAC.Table = dtBioDvAC;
                        dvBioDvAC.RowFilter = "PONo = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";

                        if (dvBioDvAC.Count > 0)
                        {
                            sheet1 = workbook.Worksheets[Ec];
                            sheet1.IsGridLinesVisible = true;
                            xlsRow = 3;
                            string strEmpCode = "";
                            int QPIssueName = 0;
                            int QPProcess = 0;
                            int QPPName = 0;
                            int QPUOM = 0;
                            int QPValue = 0;
                            int QPMinMaxReq = 0;
                            int QPMinMaxStd = 0;
                            int QPGradeName = 0;
                            int QPActionTeBeTaken = 0;
                            int QPResponsiblePerson = 0;
                            int QPActionTaken = 0;
                            int QPActionBy = 0;
                            int QPParameterRemarks = 0;
                            int QPDate = 0;
                            int QPTime = 0;
                            int QPConfirmRemarks = 0;
                            int QPQAURemarks = 0;
                            int QPReason = 0;

                            for (int i = 0; i < dvBioDvAC.Count; i++)
                            {
                                if ((string.Compare(strEmpCode.ToUpper(), dvBioDvAC[i]["PONo"].ToString().Trim().ToUpper())) != 0)
                                {
                                    #region ------------------Column Header------------------

                                    xlsCol = 1;
                                    xlsRow = 3;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Quality Status";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["QualityStatus"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 1].Text = "Date";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Date"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "MO Line ItemNo";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["MOLineItemNo"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "PO Status";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["POStatus"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Lot No";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["LotNumber"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "QI.ByWhom";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["ByWhom"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();


                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 2].Merge();
                                    xlsRow += 1;
                                    xlsCol = 4;
                                    xlsRow = 3;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Customer";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Customer"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 5].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "PO No";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["PONo"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 5].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "Article";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Article"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 5].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "QI.Grade - Added By";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["Grade"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 5].Merge();
                                    xlsRow += 1;

                                    sheet1.Range[xlsRow, xlsCol].Text = "QI.Comment";
                                    sheet1.Range[xlsRow, xlsCol + 1].Text = ": " + dvBioDvAC[i]["CommentDetails"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol + 1, xlsRow, xlsCol + 4].Merge();

                                    xlsRow = 9;
                                    xlsCol = 1;
                                    QPIssueName = xlsCol;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, QPIssueName].Text = "IssueName";
                                    sheet1.Range[xlsRow, QPIssueName].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, QPIssueName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPIssueName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPProcess = xlsCol;
                                    sheet1.Range[xlsRow, QPProcess].Text = "Process";
                                    sheet1.Range[xlsRow, QPProcess].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    QPPName = xlsCol;
                                    sheet1.Range[xlsRow, QPPName].Text = "PName";
                                    sheet1.Range[xlsRow, QPPName].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPPName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPPName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPDate = xlsCol;
                                    sheet1.Range[xlsRow, QPDate].Text = "Date";
                                    sheet1.Range[xlsRow, QPDate].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPTime = xlsCol;
                                    sheet1.Range[xlsRow, QPTime].Text = "Time";
                                    sheet1.Range[xlsRow, QPTime].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPUOM = xlsCol;
                                    sheet1.Range[xlsRow, QPUOM].Text = "UOM";
                                    sheet1.Range[xlsRow, QPUOM].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, QPUOM].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPUOM].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPValue = xlsCol;
                                    sheet1.Range[xlsRow, QPValue].Text = "Value";
                                    sheet1.Range[xlsRow, QPValue].ColumnWidth = 8;
                                    sheet1.Range[xlsRow, QPValue].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPValue].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPMinMaxReq = xlsCol;
                                    sheet1.Range[xlsRow, QPMinMaxReq].Text = "Min/MaxReq";
                                    sheet1.Range[xlsRow, QPMinMaxReq].ColumnWidth = 10;
                                    sheet1.Range[xlsRow, QPMinMaxReq].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPMinMaxReq].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPMinMaxStd = xlsCol;
                                    sheet1.Range[xlsRow, QPMinMaxStd].Text = "Min/MaxStd";
                                    sheet1.Range[xlsRow, QPMinMaxStd].ColumnWidth = 10;
                                    sheet1.Range[xlsRow, QPMinMaxStd].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPMinMaxStd].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPGradeName = xlsCol;
                                    sheet1.Range[xlsRow, QPGradeName].Text = "GradeName";
                                    sheet1.Range[xlsRow, QPGradeName].ColumnWidth = 10;
                                    sheet1.Range[xlsRow, QPGradeName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPGradeName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPActionTeBeTaken = xlsCol;
                                    sheet1.Range[xlsRow, QPActionTeBeTaken].Text = "ActionTeBeTaken";
                                    sheet1.Range[xlsRow, QPActionTeBeTaken].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, QPActionTeBeTaken].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPActionTeBeTaken].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPResponsiblePerson = xlsCol;
                                    sheet1.Range[xlsRow, QPResponsiblePerson].Text = "ResponsiblePerson";
                                    sheet1.Range[xlsRow, QPResponsiblePerson].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, QPResponsiblePerson].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPResponsiblePerson].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPActionTaken = xlsCol;
                                    sheet1.Range[xlsRow, QPActionTaken].Text = "ActionTaken";
                                    sheet1.Range[xlsRow, QPActionTaken].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, QPActionTaken].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPActionTaken].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPActionBy = xlsCol;
                                    sheet1.Range[xlsRow, QPActionBy].Text = "ActionBy";
                                    sheet1.Range[xlsRow, QPActionBy].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, QPActionBy].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPActionBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPParameterRemarks = xlsCol;
                                    sheet1.Range[xlsRow, QPParameterRemarks].Text = "ParameterRemarks";
                                    sheet1.Range[xlsRow, QPParameterRemarks].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, QPParameterRemarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPParameterRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPConfirmRemarks = xlsCol;
                                    sheet1.Range[xlsRow, QPConfirmRemarks].Text = "ConfirmRemarks";
                                    sheet1.Range[xlsRow, QPConfirmRemarks].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, QPConfirmRemarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPConfirmRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPQAURemarks = xlsCol;
                                    sheet1.Range[xlsRow, QPQAURemarks].Text = "QAURemarks";
                                    sheet1.Range[xlsRow, QPQAURemarks].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, QPQAURemarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPQAURemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    QPReason = xlsCol;
                                    sheet1.Range[xlsRow, QPReason].Text = "Reason";
                                    sheet1.Range[xlsRow, QPReason].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, QPReason].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, QPReason].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    endXlsCol = xlsCol;

                                    freezeRow = xlsRow.ToString();
                                    #endregion ------------------Column Header------------------
                                }
                                strEmpCode = dvBioDvAC[i]["PONo"].ToString().Trim();

                                #region ----------------------Data-----------------------

                                xlsRow += 1;
                                sheet1.Range[xlsRow, QPIssueName].Text = dvBioDvAC[i]["IssueName"].ToString();
                                sheet1.Range[xlsRow, QPIssueName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPIssueName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPProcess].Text = dvBioDvAC[i]["Process"].ToString();
                                sheet1.Range[xlsRow, QPProcess].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPProcess].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPPName].Text = dvBioDvAC[i]["ParameterName"].ToString();
                                sheet1.Range[xlsRow, QPPName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPPName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPDate].Text = dvBioDvAC[i]["QCDDate"].ToString();
                                sheet1.Range[xlsRow, QPDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPTime].Text = dvBioDvAC[i]["QCDTime"].ToString();
                                sheet1.Range[xlsRow, QPTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPUOM].Text = dvBioDvAC[i]["UOM"].ToString();
                                sheet1.Range[xlsRow, QPUOM].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPUOM].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPValue].Text = dvBioDvAC[i]["Value"].ToString();
                                sheet1.Range[xlsRow, QPValue].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPValue].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPMinMaxReq].Text = dvBioDvAC[i]["MinMaxRequirement"].ToString();
                                sheet1.Range[xlsRow, QPMinMaxReq].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPMinMaxReq].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPMinMaxStd].Text = dvBioDvAC[i]["MinMaxStandard"].ToString();
                                sheet1.Range[xlsRow, QPMinMaxStd].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPMinMaxStd].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPGradeName].Text = dvBioDvAC[i]["GradeName"].ToString();
                                sheet1.Range[xlsRow, QPGradeName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPGradeName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPActionTeBeTaken].Text = dvBioDvAC[i]["ActionToBeTakenName"].ToString();
                                sheet1.Range[xlsRow, QPActionTeBeTaken].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPActionTeBeTaken].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPResponsiblePerson].Text = dvBioDvAC[i]["ResponsiblePerson"].ToString();
                                sheet1.Range[xlsRow, QPResponsiblePerson].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPResponsiblePerson].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPActionTaken].Text = dvBioDvAC[i]["ActionTaken"].ToString();
                                sheet1.Range[xlsRow, QPActionTaken].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, QPActionTaken].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPActionBy].Text = dvBioDvAC[i]["ActionBy"].ToString();
                                sheet1.Range[xlsRow, QPActionBy].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPActionBy].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPParameterRemarks].Text = dvBioDvAC[i]["ParameterRemark"].ToString();
                                sheet1.Range[xlsRow, QPParameterRemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPParameterRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPConfirmRemarks].Text = dvBioDvAC[i]["ConfirmRemarks"].ToString();
                                sheet1.Range[xlsRow, QPConfirmRemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPConfirmRemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPQAURemarks].Text = dvBioDvAC[i]["QAURemarks"].ToString();
                                sheet1.Range[xlsRow, QPQAURemarks].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPQAURemarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, QPReason].Text = dvBioDvAC[i]["ReasonName"].ToString();
                                sheet1.Range[xlsRow, QPReason].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, QPReason].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                #endregion ----------------------Data-----------------------

                                #region Line Setup

                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                                #endregion Line Setup
                            }

                            xlsRow += 3;

                            xlsRow += 5;

                            sheet1.IsDisplayZeros = false;

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
                            sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 17;
                            sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                            sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            #endregion ******************Report Header******************

                            #region UsedRange Alignment

                            sheet1.UsedRange.WrapText = true;
                            sheet1.UsedRange.CellStyle.Font.Size = 8;
                            sheet1.Range["A1"].CellStyle.Font.Size = 14;
                            sheet1.Range["A2"].CellStyle.Font.Size = 10;
                            sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                            #endregion UsedRange Alignment

                            #region Page Setup
                            sheet1.PageSetup.TopMargin = 0.5;
                            sheet1.PageSetup.BottomMargin = 0.7;
                            sheet1.PageSetup.PrintTitleRows = "$1:$11";
                            sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                            sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                            sheet1.PageSetup.LeftMargin = 0.5;
                            sheet1.PageSetup.RightMargin = 0.2;
                            sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                            sheet1.PageSetup.FitToPagesTall = 0;
                            sheet1.PageSetup.FitToPagesWide = 1;
                            sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                            sheet1.IsDisplayZeros = false;

                            sheet1.Name = sEmpCodeColl[Ec].ToString().Trim();

                            #endregion Page Setup

                        }

                    }

                    #region PO & Lot wise Qty
                    sheet = workbook.Worksheets[1];
                    int ROW = 1; int COL = 1;

                    #region ColumnsHeader

                    sheet[ROW, COL].Text = "Process Set Seq"; sheet[ROW, COL].ColumnWidth = 12; int colPSS = COL; COL++;
                    sheet[ROW, COL].Text = "Process"; sheet[ROW, COL].ColumnWidth = 11; int colP = COL; COL++;
                    sheet[ROW, COL].Text = "IsBase Process"; sheet[ROW, COL].ColumnWidth = 12; int colBP = COL; COL++;
                    sheet[ROW, COL].Text = "Production Date"; sheet[ROW, COL].ColumnWidth = 12; int colPD = COL; COL++;
                    sheet[ROW, COL].Text = "Work Center"; sheet[ROW, COL].ColumnWidth = 14; int colWC = COL; COL++;
                    sheet[ROW, COL].Text = "Qty"; sheet[ROW, COL].ColumnWidth = 10; int colQty = COL; COL++;
                    sheet[ROW, COL].Text = "Responsible Person"; sheet[ROW, COL].ColumnWidth = 20; int colRP = COL; COL++;
                    sheet[ROW, COL].Text = "Remark"; sheet[ROW, COL].ColumnWidth = 30; int colRemark = COL;
                    sheet[ROW, COL].Text = "PO No"; sheet[ROW, COL].ColumnWidth = 10; int colPONo = COL; COL++;
                    sheet[ROW, COL].Text = "Lot No"; sheet[ROW, COL].ColumnWidth = 10; int colLN = COL;

                    int endCol = COL;
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.White;
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.Black;
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                    #endregion columns

                    ROW++;
                    int startRow = ROW;

                    #region DataPlot
                    for (int i = 0; i < dsPO.Tables[0].Rows.Count; i++)
                    {
                        sheet[ROW, colPSS].Text = dsPO.Tables[0].Rows[i]["ProcessSetSeq"].ToString();
                        sheet[ROW, colP].Text = dsPO.Tables[0].Rows[i]["Process"].ToString();
                        sheet[ROW, colBP].Text = dsPO.Tables[0].Rows[i]["IsBaseProcess"].ToString();
                        sheet[ROW, colPD].Text = dsPO.Tables[0].Rows[i]["ProductionDate"].ToString();
                        sheet[ROW, colWC].Text = dsPO.Tables[0].Rows[i]["WorkCenterMaster"].ToString();
                        sheet[ROW, colQty].Number = Library.Service.Extension.clsStaticInfo.dbl(dsPO.Tables[0].Rows[i]["Quantity"].ToString());
                        sheet.Range[ROW, colQty].VerticalAlignment = ExcelVAlign.VAlignTop;
                        sheet.Range[ROW, colQty].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet[ROW, colRP].Text = dsPO.Tables[0].Rows[i]["ResponsiblePerson"].ToString();
                        sheet[ROW, colRemark].Text = dsPO.Tables[0].Rows[i]["Remarks"].ToString();
                        sheet[ROW, colPONo].Text = dsPO.Tables[0].Rows[i]["PONo"].ToString();
                        sheet[ROW, colLN].Text = dsPO.Tables[0].Rows[i]["LotNo"].ToString();


                        sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                        sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                        ROW++;
                    }
                    #endregion
                    int edCRow = ROW;

                    #region ReportHeader

                    sheet.AutoFilters.FilterRange = sheet.Range[colPSS, 1, 1, endCol];
                    //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[colPSS, 1, ROW, endCol]);
                    ////Apply custom table style
                    //ITableStyles tableStyles = workbook.TableStyles;
                    //ITableStyle tableStyle = tableStyles.Add("Table Style 1");
                    //ITableStyleElements tableStyleElements = tableStyle.TableStyleElements;
                    //ITableStyleElement tableStyleElement = tableStyleElements.Add(ExcelTableStyleElementType.SecondColumnStripe);
                    //tableStyleElement.BackColorRGB = Color.FromArgb(217, 225, 242);

                    //ITableStyleElement tableStyleElement1 = tableStyleElements.Add(ExcelTableStyleElementType.FirstColumn);
                    //tableStyleElement1.FontColorRGB = Color.FromArgb(128, 128, 128);

                    //ITableStyleElement tableStyleElement2 = tableStyleElements.Add(ExcelTableStyleElementType.HeaderRow);
                    //tableStyleElement2.FontColor = ExcelKnownColors.White;
                    //tableStyleElement2.BackColorRGB = Color.FromArgb(0, 112, 192);


                    //table.TableStyleName = tableStyle.Name;

                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.UsedRange.HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    sheet["A" + startRow.ToString()].FreezePanes();

                    ReportUtility reportUtility = new ReportUtility();
                    //reportUtility.PlantHeader(ref sheet, endCol, "Production Data Report", plantId);
                    reportUtility.PageSetup(ref sheet, 1, ExcelPageOrientation.Landscape);
                    sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[1, 1, 1, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    sheet.UsedRange.WrapText = true;
                    sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.IsGridLinesVisible = false;
                    sheet.Name = "ProductionData";
                    sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


                    sheet.PageSetup.TopMargin = 0.2;
                    sheet.PageSetup.BottomMargin = 0.8;
                    sheet.PageSetup.LeftMargin = 0.2;
                    sheet.PageSetup.RightMargin = 0.2;
                    sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet.PageSetup.FitToPagesTall = 0;
                    sheet.PageSetup.FitToPagesWide = 1;
                    sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet.PageSetup.CenterHorizontally = true;
                    #endregion

                    #endregion

                    return workbook;
                }
                else
                {
                    Exception ex = new Exception("No data found...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsBioDvAC = null;
                dvBioDvAC = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }


        }

        // Lot Wise Quality Report Function

        public IWorkbook GetCustomerQualityLotWiseJobCardReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string CustomerId, string InvoiceId, string ProductionOrderId, string LotNumber)
        {

            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsBioDvAC = null;
            DataSet dsPO = null;
            DataTable dtBioDvAC = null;
            DataView dvBioDvAC = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            StringCollection sEmpCodeColl = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            var workbook = oru.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            IWorksheet sheet = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string freezeRow = "";
            try
            {
                #region DataSet
                objRpt = new clsReport();
                GetCustomerLotWiseQualityJobCardReport(CustomerId, InvoiceId, ProductionOrderId, LotNumber, plantId, out dsBioDvAC);
                dtBioDvAC = dsBioDvAC.Tables[0];

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
                objRpt.SelectedPlant(plantId, out dsFactory);
                #endregion DataSet

                if (dsBioDvAC.Tables[0].Rows.Count > 0)
                {
                    sEmpCodeColl = new StringCollection();
                    for (int i = 0; i <= dsBioDvAC.Tables[0].Rows.Count - 1; i++)
                    {
                        if (sEmpCodeColl.Contains(dsBioDvAC.Tables[0].Rows[i]["ProductionOrderId"].ToString().Trim()) == false)
                        {
                            sEmpCodeColl.Add(dsBioDvAC.Tables[0].Rows[i]["ProductionOrderId"].ToString().Trim());

                        }
                    }
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(1);
                    for (int Ec = 0; Ec < sEmpCodeColl.Count-1; Ec++)
                    {
                        dvBioDvAC = new DataView();
                        dvBioDvAC.Table = dtBioDvAC;
                        dvBioDvAC.RowFilter = "ProductionOrderId = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";

                        if (dvBioDvAC.Count > 0)
                        {
                            sheet1 = workbook.Worksheets[Ec];
                            sheet1.IsGridLinesVisible = true;
                            xlsRow = 5;
                            string strEmpCode = "";
                            int Parameter = 0;
                            int UOM = 0;
                            int Value = 0;
                            int Remarks = 0;

                            for (int i = 0; i < dvBioDvAC.Count; i++)
                            {
                                if ((string.Compare(strEmpCode.ToUpper(), dvBioDvAC[i]["ProductionOrderId"].ToString().Trim().ToUpper())) != 0)
                                {
                                    #region ------------------Column Header------------------

                                    xlsCol = 1;
                                    xlsRow = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Lot Wise Quality Report";
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].Merge();


                                    xlsCol = 1;
                                    xlsRow = 3;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Date".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    xlsRow = 3;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Date"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Article".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Article"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "PO No".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["ProductionOrderId"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "LotNumber".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["LotNo"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Product Code".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["ProductCode"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Detail".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Detail"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Final Remarks".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Remarks"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();



                                    xlsRow += 1;
                                    xlsCol = 1;
                                    Parameter = xlsCol;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, Parameter].Text = "Parameter Name";
                                    sheet1.Range[xlsRow, Parameter].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, Parameter].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, Parameter].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    UOM = xlsCol;
                                    sheet1.Range[xlsRow, UOM].Text = "UOM";
                                    sheet1.Range[xlsRow, UOM].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, UOM].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, UOM].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    Value = xlsCol;
                                    sheet1.Range[xlsRow, Value].Text = "Value";
                                    sheet1.Range[xlsRow, Value].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, Value].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, Value].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    Remarks = xlsCol;
                                    sheet1.Range[xlsRow, Remarks].Text = "Remarks";
                                    sheet1.Range[xlsRow, Remarks].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, Remarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, Remarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    endXlsCol = xlsCol;

                                    freezeRow = xlsRow.ToString();
                                    #endregion ------------------Column Header------------------
                                }
                                strEmpCode = dvBioDvAC[i]["ProductionOrderId"].ToString().Trim();

                                #region ----------------------Data-----------------------

                                xlsRow += 1;
                                sheet1.Range[xlsRow, Parameter].Text = dvBioDvAC[i]["Parameter"].ToString();
                                sheet1.Range[xlsRow, Parameter].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, Parameter].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, UOM].Text = dvBioDvAC[i]["UOM"].ToString();
                                sheet1.Range[xlsRow, UOM].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, UOM].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, Value].Text = dvBioDvAC[i]["Value"].ToString();
                                sheet1.Range[xlsRow, Value].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, Value].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, Remarks].Text = dvBioDvAC[i]["ParaRemarks"].ToString();
                                sheet1.Range[xlsRow, Remarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, Remarks].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                #endregion ----------------------Data-----------------------

                                #region Line Setup

                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                                #endregion Line Setup
                            }

                            xlsRow += 3;

                            xlsRow += 5;


                            sheet1.IsDisplayZeros = false;

                            #region ******************Report Header******************
                           
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
                            sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].Merge();
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                            #endregion ******************Report Header******************

                            #region UsedRange Alignment

                            sheet1.UsedRange.WrapText = true;
                            sheet1.UsedRange.CellStyle.Font.Size = 8;
                            sheet1.Range["A1"].CellStyle.Font.Size = 10;
                            sheet1.Range["A2"].CellStyle.Font.Size = 10;
                            sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                            #endregion UsedRange Alignment

                            #region Page Setup
                            sheet1.PageSetup.TopMargin = 0.5;
                            sheet1.PageSetup.BottomMargin = 0.7;
                            sheet1.PageSetup.PrintTitleRows = "$1:$11";
                            sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                            sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                            sheet1.PageSetup.LeftMargin = 0.5;
                            sheet1.PageSetup.RightMargin = 0.2;
                            sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                            sheet1.PageSetup.FitToPagesTall = 0;
                            sheet1.PageSetup.FitToPagesWide = 1;
                            sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                            sheet1.IsDisplayZeros = false;

                            sheet1.Name = sEmpCodeColl[Ec].ToString().Trim();

                            #endregion Page Setup

                        }

                    }
                    return workbook;
                }
                else
                {
                    Exception ex = new Exception("No data found...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsBioDvAC = null;
                dsPO = null;
                dvBioDvAC = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }


        }

        public IWorkbook GetCustomerQualityLotWiseUpdateJobCardReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string CustomerId, string InvoiceId, string ProductionOrderId, string LotNumber)
        {

            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsBioDvAC = null;
            DataSet dsPO = null;
            DataTable dtBioDvAC = null;
            DataView dvBioDvAC = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            StringCollection sEmpCodeColl = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            var workbook = oru.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            IWorksheet sheet = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string freezeRow = "";
            try
            {
                #region DataSet
                objRpt = new clsReport();
                GetCustomerQualityLotWiseUpdateJobCardReport(CustomerId, InvoiceId, ProductionOrderId, LotNumber, plantId, out dsBioDvAC);
                dtBioDvAC = dsBioDvAC.Tables[0];

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
                objRpt.SelectedPlant(plantId, out dsFactory);
                #endregion DataSet

                if (dsBioDvAC.Tables[0].Rows.Count > 0)
                {
                    sEmpCodeColl = new StringCollection();
                    for (int i = 0; i <= dsBioDvAC.Tables[0].Rows.Count - 1; i++)
                    {
                        if (sEmpCodeColl.Contains(dsBioDvAC.Tables[0].Rows[i]["ProductionOrderId"].ToString().Trim()) == false)
                        {
                            sEmpCodeColl.Add(dsBioDvAC.Tables[0].Rows[i]["ProductionOrderId"].ToString().Trim());

                        }
                    }
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(1);
                    for (int Ec = 0; Ec < sEmpCodeColl.Count; Ec++)
                    {
                        dvBioDvAC = new DataView();
                        dvBioDvAC.Table = dtBioDvAC;
                        dvBioDvAC.RowFilter = "ProductionOrderId = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";

                        if (dvBioDvAC.Count > 0)
                        {
                            sheet1 = workbook.Worksheets[Ec];
                            sheet1.IsGridLinesVisible = true;
                            xlsRow = 5;
                            string strEmpCode = "";
                            int Parameter = 0;
                            int UOM = 0;
                            int Value = 0;
                            int Remarks = 0;

                            for (int i = 0; i < dvBioDvAC.Count; i++)
                            {
                                if ((string.Compare(strEmpCode.ToUpper(), dvBioDvAC[i]["ProductionOrderId"].ToString().Trim().ToUpper())) != 0)
                                {
                                    #region ------------------Column Header------------------

                                    xlsCol = 1;
                                    xlsRow = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Lot Wise Quality Report";
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].Merge();


                                    xlsCol = 1;
                                    xlsRow = 3;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Date".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    xlsRow = 3;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Date"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Article".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Article"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "PO No".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["ProductionOrderId"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "LotNumber".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["LotNo"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Product Code".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["ProductCode"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Detail".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Detail"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Final Remarks".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Remarks"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();



                                    xlsRow += 1;
                                    xlsCol = 1;
                                    Parameter = xlsCol;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, Parameter].Text = "Parameter Name";
                                    sheet1.Range[xlsRow, Parameter].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, Parameter].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, Parameter].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    UOM = xlsCol;
                                    sheet1.Range[xlsRow, UOM].Text = "UOM";
                                    sheet1.Range[xlsRow, UOM].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, UOM].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, UOM].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    Value = xlsCol;
                                    sheet1.Range[xlsRow, Value].Text = "Value";
                                    sheet1.Range[xlsRow, Value].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, Value].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, Value].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    Remarks = xlsCol;
                                    sheet1.Range[xlsRow, Remarks].Text = "Remarks";
                                    sheet1.Range[xlsRow, Remarks].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, Remarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, Remarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    endXlsCol = xlsCol;

                                    freezeRow = xlsRow.ToString();
                                    #endregion ------------------Column Header------------------
                                }
                                strEmpCode = dvBioDvAC[i]["ProductionOrderId"].ToString().Trim();

                                #region ----------------------Data-----------------------

                                xlsRow += 1;
                                sheet1.Range[xlsRow, Parameter].Text = dvBioDvAC[i]["Parameter"].ToString();
                                sheet1.Range[xlsRow, Parameter].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, Parameter].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, UOM].Text = dvBioDvAC[i]["UOM"].ToString();
                                sheet1.Range[xlsRow, UOM].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, UOM].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, Value].Text = dvBioDvAC[i]["Value"].ToString();
                                sheet1.Range[xlsRow, Value].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, Value].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, Remarks].Text = dvBioDvAC[i]["ParaRemarks"].ToString();
                                sheet1.Range[xlsRow, Remarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, Remarks].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                #endregion ----------------------Data-----------------------

                                #region Line Setup

                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                                #endregion Line Setup
                            }

                            xlsRow += 3;

                            xlsRow += 5;


                            sheet1.IsDisplayZeros = false;

                            #region ******************Report Header******************
                          
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
                            sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].Merge();
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            #endregion ******************Report Header******************

                            #region UsedRange Alignment

                            sheet1.UsedRange.WrapText = true;
                            sheet1.UsedRange.CellStyle.Font.Size = 8;
                            sheet1.Range["A1"].CellStyle.Font.Size = 10;
                            sheet1.Range["A2"].CellStyle.Font.Size = 10;
                            sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                            #endregion UsedRange Alignment

                            #region Page Setup
                            sheet1.PageSetup.TopMargin = 0.5;
                            sheet1.PageSetup.BottomMargin = 0.7;
                            sheet1.PageSetup.PrintTitleRows = "$1:$11";
                            sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                            sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                            sheet1.PageSetup.LeftMargin = 0.5;
                            sheet1.PageSetup.RightMargin = 0.2;
                            sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                            sheet1.PageSetup.FitToPagesTall = 0;
                            sheet1.PageSetup.FitToPagesWide = 1;
                            sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                            sheet1.IsDisplayZeros = false;

                            sheet1.Name = sEmpCodeColl[Ec].ToString().Trim();

                            #endregion Page Setup

                        }

                    }
                    return workbook;
                }
                else
                {
                    Exception ex = new Exception("No data found...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsBioDvAC = null;
                dsPO = null;
                dvBioDvAC = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }


        }

        public IWorkbook GetCustomerLWQSummaryJobCardReport(string username, string companyGroupId, string companyId, string plantId, string plantName, string CustomerId, string InvoiceId, string ProductionOrderId, string LotNumber)
        {

            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsBioDvAC = null;
            DataSet dsPO = null;
            DataTable dtBioDvAC = null;
            DataView dvBioDvAC = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            StringCollection sEmpCodeColl = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            var workbook = oru.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            IWorksheet sheet = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string freezeRow = "";
            try
            {
                #region DataSet
                objRpt = new clsReport();
                GetCustomerLWQSummaryJobCardReport(CustomerId, InvoiceId, ProductionOrderId, LotNumber, plantId, out dsBioDvAC);
                dtBioDvAC = dsBioDvAC.Tables[0];

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
                objRpt.SelectedPlant(plantId, out dsFactory);
                #endregion DataSet

                if (dsBioDvAC.Tables[0].Rows.Count > 0)
                {
                    sEmpCodeColl = new StringCollection();
                    for (int i = 0; i <= dsBioDvAC.Tables[0].Rows.Count - 1; i++)
                    {
                        if (sEmpCodeColl.Contains(dsBioDvAC.Tables[0].Rows[i]["ProductionOrderId"].ToString().Trim()) == false)
                        {
                            sEmpCodeColl.Add(dsBioDvAC.Tables[0].Rows[i]["ProductionOrderId"].ToString().Trim());

                        }
                    }
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;
                    workbook = application.Workbooks.Create(1);
                    for (int Ec = 0; Ec < sEmpCodeColl.Count; Ec++)
                    {
                        dvBioDvAC = new DataView();
                        dvBioDvAC.Table = dtBioDvAC;
                        dvBioDvAC.RowFilter = "ProductionOrderId = '" + sEmpCodeColl[Ec].ToString().Trim() + "'";

                        if (dvBioDvAC.Count > 0)
                        {
                            sheet1 = workbook.Worksheets[Ec];
                            sheet1.IsGridLinesVisible = true;
                            xlsRow = 5;
                            string strEmpCode = "";
                            int Parameter = 0;
                            int UOM = 0;
                            int Value = 0;
                            int Remarks = 0;

                            for (int i = 0; i < dvBioDvAC.Count; i++)
                            {
                                if ((string.Compare(strEmpCode.ToUpper(), dvBioDvAC[i]["ProductionOrderId"].ToString().Trim().ToUpper())) != 0)
                                {
                                    #region ------------------Column Header------------------

                                    xlsCol = 1;
                                    xlsRow = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Lot Wise Quality Report";
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].Merge();


                                    xlsCol = 1;
                                    xlsRow = 3;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Date".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    xlsRow = 3;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Date"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Article".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Article"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "PO No".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["ProductionOrderId"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "LotNumber".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["LotNo"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Product Code".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["ProductCode"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Detail".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Detail"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();

                                    xlsCol = 1;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, xlsCol].Text = "Final Remarks".ToUpper();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol].Merge();
                                    xlsCol = 2;
                                    sheet1.Range[xlsRow, xlsCol].Text = dvBioDvAC[i]["Remarks"].ToString().Trim();
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 2].Merge();



                                    xlsRow += 1;
                                    xlsCol = 1;
                                    Parameter = xlsCol;
                                    xlsRow += 1;
                                    sheet1.Range[xlsRow, Parameter].Text = "Parameter Name";
                                    sheet1.Range[xlsRow, Parameter].ColumnWidth = 20;
                                    sheet1.Range[xlsRow, Parameter].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, Parameter].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    UOM = xlsCol;
                                    sheet1.Range[xlsRow, UOM].Text = "UOM";
                                    sheet1.Range[xlsRow, UOM].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, UOM].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, UOM].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                    xlsCol += 1;
                                    Value = xlsCol;
                                    sheet1.Range[xlsRow, Value].Text = "Value";
                                    sheet1.Range[xlsRow, Value].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, Value].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, Value].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    xlsCol += 1;
                                    Remarks = xlsCol;
                                    sheet1.Range[xlsRow, Remarks].Text = "Remarks";
                                    sheet1.Range[xlsRow, Remarks].ColumnWidth = 15;
                                    sheet1.Range[xlsRow, Remarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, Remarks].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                                    endXlsCol = xlsCol;

                                    freezeRow = xlsRow.ToString();
                                    #endregion ------------------Column Header------------------
                                }
                                strEmpCode = dvBioDvAC[i]["ProductionOrderId"].ToString().Trim();

                                #region ----------------------Data-----------------------

                                xlsRow += 1;
                                sheet1.Range[xlsRow, Parameter].Text = dvBioDvAC[i]["Parameter"].ToString();
                                sheet1.Range[xlsRow, Parameter].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, Parameter].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, UOM].Text = dvBioDvAC[i]["UOM"].ToString();
                                sheet1.Range[xlsRow, UOM].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, UOM].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, Value].Text = dvBioDvAC[i]["Value"].ToString();
                                sheet1.Range[xlsRow, Value].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, Value].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, Remarks].Text = dvBioDvAC[i]["ParaRemarks"].ToString();
                                sheet1.Range[xlsRow, Remarks].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, Remarks].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                #endregion ----------------------Data-----------------------

                                #region Line Setup

                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].WrapText = true;

                                #endregion Line Setup
                            }

                            xlsRow += 3;

                            xlsRow += 5;


                            sheet1.IsDisplayZeros = false;

                            #region ******************Report Header******************
                           
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
                            sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].Merge();
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow, xlsCol, xlsRow, xlsCol + 3].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                            #endregion ******************Report Header******************

                            #region UsedRange Alignment

                            sheet1.UsedRange.WrapText = true;
                            sheet1.UsedRange.CellStyle.Font.Size = 8;
                            sheet1.Range["A1"].CellStyle.Font.Size = 10;
                            sheet1.Range["A2"].CellStyle.Font.Size = 10;
                            sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                            #endregion UsedRange Alignment

                            #region Page Setup
                            sheet1.PageSetup.TopMargin = 0.5;
                            sheet1.PageSetup.BottomMargin = 0.7;
                            sheet1.PageSetup.PrintTitleRows = "$1:$11";
                            sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                            sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                            sheet1.PageSetup.LeftMargin = 0.5;
                            sheet1.PageSetup.RightMargin = 0.2;
                            sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                            sheet1.PageSetup.FitToPagesTall = 0;
                            sheet1.PageSetup.FitToPagesWide = 1;
                            sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                            sheet1.IsDisplayZeros = false;

                            sheet1.Name = sEmpCodeColl[Ec].ToString().Trim();

                            #endregion Page Setup

                        }

                    }
                    return workbook;
                }
                else
                {
                    Exception ex = new Exception("No data found...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsBioDvAC = null;
                dsPO = null;
                dvBioDvAC = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }


        }

        public void GetEmpJobCardMonthlySummary(string EmpIdLoop, string FromDate, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @" SELECT EmpSystemID,EmployeeCode, WorkDate ,EmployeeCode,isnull(a.PresentValue,0)TotalPresent,
                ISNULL(a.LvValue, 0) TotalLv,ISNULL(a.HoliDayValue,0)TotalHoliDay,
                ISNULL(a.WeekOffValue, 0)TotalWeekOff
                                ,ISNULL(a.LWPValue, 0) TotalLWP,ISNULL(TotalMLv, 0) TotalMLv,
								isnull(a.AbsentValue,0)TotalAbsent,ISNULL(a.LateValue,0)TotalLate
                                , DayValue = ISNULL(a.PresentValue, 0) +
								ISNULL(a.LateValue, 0) + ISNULL(a.LvValue, 0) +
								ISNULL(TotalMLv, 0) + ISNULL(a.WeekOffValue, 0)
                                + ISNULL(TotalCompAssignLv, 0) + ISNULL(a.HoliDayValue, 0) + 
								ISNULL(TotalWeekOffHoliDay, 0)
								,Category,DayStatus
                                FROM(SELECT EmpSystemID, WorkDate, EmployeeCode,
								Category,DayStatus,PresentValue,AbsentValue,LateValue,                          
			                              
                                TotalMLv = 0,
								TotalCompAssignLv = 0,                             
                                TotalWeekOffHoliDay = 0,
                                OTHr,LvValue,WeekOffValue,HoliDayValue,
                                LWPValue=Case When(DayStatus='LWP')
								then 1 else 0 end
                                FROM dbo.AttdnProcessData a
                                left join daytype p on a.DayStatus=p.DayType
                                left join employeeInformation ei on ei.SystemId =a.EmpSystemID
                                WHERE  ei.SystemId in(" + EmpIdLoop + @")
                                and WorkDate between '" + FromDate + @"' AND '" + ToDate + @"'
                                ) A  ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void GetEmpJobCardInfoWithInDateTimes(string EmpIdLoop, string FromDate, string ToDate, string plantId, out DataSet dsRef)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT A.EmployeeCode,A.EmployeeCodeNumeric
                            	,A.EmployeeName,A.OutTime punchTime,A.firstSlab
                                ,A.EmployeeStatus
                            	,A.DOJ
                            	,A.GivenDesignation
                                ,A.LegalDesignation
                            	,A.Unit
                            	,A.Division
                            	,A.Department
                            	,A.Section
                            	,A.SubSection
                            	,REPLACE(CONVERT(VARCHAR(11), A.PDate, 113), ' ', '-') PDate
                                ,PDay
                            	,DayStatus
                                
                                ,A.IsHalfDayLeave
                            	,A.InTime
                                ,ShiftInTimeShow
								 ,ShiftInTime
                            	,A.InDeviceID
                            	,A.OutTime
                            	,A.OutDeviceID
                            	,A.IsManual
                            	,OverStay
                                ,A.TotalOTHr FinalOT
                            	,A.LvShortName
                            	,A.Code
                            	,A.LvDescrip
                            	,A.LeaveType
                                ,dti,dto
                                ,InTimeShow
                                ,OutTimeShow
                                ,A.OTConsiderOn
                                ,ShiftTime = CASE WHEN ShiftChangeInTime IS NULL THEN ShiftInTime ELSE ShiftChangeInTime END
                                ,ShiftInTimeC = CASE WHEN ShiftChangeInTime IS NULL THEN ShiftInTimecc ELSE ShiftChangeInTime END
                                ,ShiftName
								,ShiftType
							    ,ShiftOutTime
                                ,A.IsManualDayStatus,A.IsManualInTime,A.IsManualOutTime, A.ShortLeave,A.IsOTEntitled,A.IsOTComfirm,A.pdate  WorkDate,
                                ReConfirm = CASE  WHEN A.IsOTComfirm=0 AND A.WorkDate IS NOT NULL  THEN 1   ELSE 0  END,A.DayCategory
                                ,A.InTimelate,A.OutTimelate
                                ,A.ShiftInTimeLate
                                ,A.GradeCode
	                            ,A.LeaveDuration                               
								,A.DurationInMin

	                                ,A.EO 
									,A.LIN
									,A.LO
                                    ,A.Line
                            FROM(
                                SELECT E.EmployeeCode,e.EmployeeCodeNumeric,g.firstSlab
                                    , E.EmployeeName
                                    ,E.EmployeeStatus
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
                                    , REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOS
                                    , D.UserName GivenDesignation
                                    , U.UserName Unit
                                    , Dv.UserName Division
                                    , Dp.UserName Department
                                    , S.UserName Section
                                    ,ar.IsHalfDayLeave
                                    , SB.UserName SubSection
                                    ,datename(dw,AR.WorkDate) as PDay
                                    , AR.WorkDate PDate
                                    --, AR.DayStatus
                                    , LSalGr.Code GradeCode
                                    , HR.OTConsiderOn
                                    , AR.InTime InTime
                                    --, AR.InTime InTimeShow
                                   	,l.UserName as Line
                                    ,OverStay = case when hr.NoPunchOnHoliday = 1 and dt.OriginalDayType = 'H' then 0.00
									when hr.NoPunchOnWeekoff = 1 and dt.OriginalDayType = 'W' then 0.00 else AR.ProcessedOT end
                                    --,DayStatus = case when hr.NoPunchOnHoliday = 1 and dt.OriginalDayType = 'H' then 'H' 
									--when hr.NoPunchOnWeekoff = 1 and dt.OriginalDayType = 'W' then 'W'
									--else AR.DayStatus end
                                    ,AR.DayStatus
									,InTimeShow = case when hr.NoPunchOnHoliday = 1 and dt.OriginalDayType = 'H' then null
									 when hr.NoPunchOnWeekoff = 1 and dt.OriginalDayType = 'W' then null else AR.InTime end
									,OutTimeShow = case when hr.NoPunchOnHoliday = 1 and dt.OriginalDayType = 'H' then Null
								 when hr.NoPunchOnWeekoff = 1 and dt.OriginalDayType = 'W' then Null	else AR.OutTime end
                            ,ShiftInTimeLate=CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),108)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 108)
						     END
                                    , CONVERT(VARCHAR(5), AR.InTime, 108) InTimelate
                             ,ShiftInTimeShow = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
						     END
                                    , ARIN.DeviceID InDeviceID
                                    , AR.OutTime OutTime
                                    --, AR.OutTime OutTimeShow
                                    , CONVERT(VARCHAR(5), AR.OutTime, 108) OutTimelate
                                    , AROUT.DeviceID OutDeviceID
                                    , AR.IsManualInTime IsManual
                                    ,ar.StandardOT as TotalOTHr
                                    , LT.UserName LvShortName
                                    , LT.Description LvDescrip
                                    , LT.LeaveType
                                    , LT.Code
                                    , Isnull(LG.UserName, '') LegalDesignation
                                    , AR.InTime dti, AR.OutTime dto
                                    , CONVERT(VARCHAR(5), cs.InTime, 108) ShiftChangeInTime
                                    , SD.ShiftDefinationName ShiftName
									,sd.ShiftType
                                    ,LEAVE.LeaveDuration	                            
									,HODD.DurationInMin

		                            ,EO.OffDuration AS EO
									,EIN.OffDuration AS LIN
									,LO= Case when LO.InfoType='LUNCHOUT' THEN 'YES' ELSE 'NO' END

						   ,ShiftOutTime = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                     ,ShiftInTime = Format(AR.InTime, 'yyyy-MM-dd') + ' ' + CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END

                                    ,ShiftInTimecc = Format(AR.WorkDate, 'yyyy-MM-dd') + ' ' + CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END

                                    , AR.IsManualDayStatus, AR.IsManualInTime, AR.IsManualOutTime, ar.CountedShortLeave ShortLeave,AR.IsOTEntitled,AR.IsOTComfirm,AR.WorkDate,dt.Category DayCategory
                                FROM dbo.EmployeeInformation E

                                    LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id

                                INNER JOIN dbo.AttdnProcessData AR ON E.SystemID = AR.EmpSystemID
	                           LEFT JOIN (select LET.SystemID,LTD.LeaveDuration,LTD.WorkDate,LET.EmpSystemID from  LeaveTransaction LET 
										    left join LeaveTransactionDetails LTD ON LTD.LvTrnsSystemID=LET.SystemID	
                                        where ltd.WorkDate Between '" + FromDate + @"' and '" + ToDate + @"'
								         ) LEAVE ON LEAVE.EmpSystemID=E.SystemId and LEAVE.WorkDate= AR.WorkDate

                                left join (select EmpSystemID,WorkDate,SUM(DurationInMin)AS DurationInMin
		                    From  [dbo].[HourlyOffDuty] 
	                        WHERE  ApproveType='Deducation' AND WorkDate Between '" + FromDate + @"' and '" + ToDate + @"'
		                    Group BY  EmpSystemID,WorkDate)as HODD on HODD.EmpSystemID=E.SystemId and HODD.WorkDate=AR.WorkDate

                                LEFT JOIN(SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + FromDate + @"' BETWEEN FromDate AND ToDate) AS SFCG
                                ON AR.ShiftSystemID = SFCG.ShiftDefinationID
                                LEFT JOIN dbo.AttdnRawData ARIN ON AR.InTimeRowID = ARIN.RowID
                                LEFT JOIN dbo.AttdnRawData AROUT ON AR.OutTimeRowID = AROUT.RowID
                                LEFT JOIN dbo.LeaveType LT ON AR.LTSystemID = LT.Id
                                LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                                LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                                LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id

                                  LEFT JOIN ORG.Section S ON PO.SectionID = S.Id
                                LEFT JOIN ORG.SubSection SB ON PO.SubSectionID = SB.Id
								left join org.Line l on l.Id=mpb.LineId

                                LEFT JOIN HKP.LegalDesignation LG ON E.LegalDesignationId = LG.Id
                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LG.Id and LSGD.PlantId='" + plantId + @"' and LSGD.LegalSalaryGradeId is not null
                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId
                                left join(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m
                                left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = AR.ShiftSystemID and cs.ShiftDate = ar.WorkDate
                                left join[ShiftDefination] sd on sd.SystemID = AR.ShiftSystemID
                                LEFT JOIN HKP.Designation D ON E.GivenDesignationId = D.Id
                                LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=E.PlantId
                                LEFT JOIN DayType dt on dt.Daytype=AR.DayStatus
                            
							left join OTSlabDefineGeneral g on 
							'" + ToDate + @"' between g.FromDate and g.ToDate 
							and g.PlantID=ar.PlantID 
							and g.DayType=dt.OriginalDayType

                                left join AttendanceInfoExtra LO on LO.EmpSystemId=e.SystemId and LO.WorkDate=ar.WorkDate and LO.InfoType='LUNCHOUT'
								left join AttendanceInfoExtra EO on EO.EmpSystemId=e.SystemId and EO.WorkDate=ar.WorkDate and EO.InfoType='EARLYOUT'
								left join AttendanceInfoExtra EIN on EIN.EmpSystemId=e.SystemId and EIN.WorkDate=ar.WorkDate and EIN.InfoType='EARLYIN'

                                WHERE E.SystemID in (" + EmpIdLoop + @")
                                    AND AR.WorkDate BETWEEN '" + FromDate + @"'
                                        AND '" + ToDate + @"' AND (EmployeeStatus = 'Active' OR COnvert(date,DOS) >= Convert(Date,'" + FromDate + @"'))
                                ) A
                            
                            ORDER BY A.EmployeeCode
                            	,A.PDate
                                ";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
                objCon.CommitTransaction();
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

        public void GetOrderWiseParameterJobCardReport(string FromDate, string ToDate, string IssueId, string ProductionOrderId, string LotNumber, string EntityId, string QualityStatus, string Date, string plantId, out DataSet dsRef)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"select distinct '" + QualityStatus + "' QualityStatus, '" + Date + @"' Date, QCData.QCDate,M.MOLineItemNo,M.POStatus,M.ProductionOrderId PONo,isnull(QCData.LotNumber,M.LotNumber) LotNumber,M.Article,M.Customer,M.PartyNature, 
M.IssueId,M.IssueName,M.ParameterSequence,M.ParameterId,M.ParameterName,M.UOM,QCData.Value,QCData.GradeName,QCData.ParameterRemark,QCData.ActionToBeTakenName,QCData.ResponsiblePerson,QCData.PassValue,QCData.FailValue,QCData.RejectValue,QCData.ToClose,QCData.ToConfirm,
QCData.HeaderId,QCData.ChildId,QCData.QCDDate,QCData.QCDTime,(Case When (QCData.Value is null or QCData.Value = '0') then 1 else 0 end) EntryMissing,M.Process,M.Entity,M.EntityId,Reverse(stuff(Reverse((select OWC.Grade +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.MOLineItemNo=M.MOLineItemNo and OWC.PONo=M.ProductionOrderId and OWC.LotNo=M.LotNumber for xml PATH(''))),1,2,'')) Grade,
Reverse(stuff(Reverse((select format(OWC.AddedDate,'dd-MMM-yyyy') + '-' + OWC.Comment +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.MOLineItemNo=M.MOLineItemNo and OWC.PONo=M.ProductionOrderId and OWC.LotNo=M.LotNumber for xml PATH(''))),1,2,'')) CommentDetails,
Reverse(stuff(Reverse((select isnull(RD.MinRequirement,'') + '/' + isnull(RD.MaxRequirement,'') +', ' from TRN.UCPRequirementDetails RD																			
where RD.ParameterId=QCData.ChildId for xml PATH(''))),1,2,'')) MinMaxRequirement,
Reverse(stuff(Reverse((select isnull(SD.MinStandard,'') + '/' + isnull(SD.MaxStandard,'') +', ' from TRN.UCPMaxMinStandardDetails SD																			
where SD.ParameterId=QCData.ChildId for xml PATH(''))),1,2,'')) MinMaxStandard,
QCData.ActionTaken,QCData.ActionBy,QCData.ConfirmRemarks,QCData.QAURemarks,QCData.ReasonName from (Select  P.*,CP.IssueName,CP.IssueId,CP.ParameterId,CP.ParameterName,CP.UOM,CP.ParameterSequence,CP.Process from (select Distinct PS.ProductionOrderId,PS.LotNumber,E.UserName Entity,PS.EntityId,
MOLineItemNo= STUFF((select distinct ','+ XMOI.Id from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
where PS.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),  
Article=STUFF((select distinct ','+MA.StandardName from
											MST.MaterialMasterArticle MA
											left join TRN.MasterOrderItem moi on moi.ArticleId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=xp.Id
											where PS.ProductionOrderId=Xpod.ProductionOrderId for xml path('') ), 1, 1, '')
,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PS.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PartyNature= STUFF((select distinct ','+XP.PartyNature from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PS.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
1 PlanSet,PST.UserName POStatus,PS.ProcessId
from TRN.ProductionSummary PS
left join trn.ProductionOrder PO on PO.Id=PS.ProductionOrderId
left join hkp.ProductionStatus PST on PST.Id=PO.ProductionStatusId
left join org.Entity E on E.Id=PS.EntityId
where PS.AddedDate between '" + FromDate + "' and '" + ToDate + @"') P
inner Join (select QMM.UserName IssueName,QMP.QMID IssueId,QMP.Id ParameterId,PM.UserName ParameterName,QMP.SNO ParameterSequence,UOM.UserName UOM,1 as PlanSet,PR.UserName Process,QMP.ProcessId
 from MST.QualityManagementParameterItem QMP
 left join MST.QualityManagementMaster QMM on QMM.Id=QMP.QMID
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId
 left join hkp.Process PR on  PR.Id=QMP.ProcessId
 where CustomerParameter = 1) CP on CP.PlanSet=P.PlanSet) M
 left join (select QC.IssueId,QMM.UserName IssueName,QCD.ItemId ParameterId,PM.UserName ParameterName,QC.LotNumber,QC.ProductionOrderId,
 QCD.Value,QGD.GradeName,QCD.Remarks ParameterRemark,QAT.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,
 format(QC.AddedDate,'dd-MMM-yyyy') QCDate,format(QCD.AddedDate,'dd-MMM-yyyy') QCDDate,format(QCD.AddedDate,'hh:mm-tt') QCDTime,QC.Id HeaderId,QCD.Id ChildId,QGD.IsPassValue PassValue,QGD.IsFailValue FailValue,QGD.IsRejectValue RejectValue,
 QAU.ActionTaken,QAE.EmployeeName ActionBy,QAU.Remarks QAURemarks,isnull(QAU.ReasonName,(select UserName from [HKP].[QualityManagementReasonMaster] where Id=(select ReasonId from [MST].[QualityManagementParameterReason] where Id=QAU.ReasonId))) ReasonName,QAU.ConfirmRemarks,
 (case when (QGD.IsFailValue <> 0 and QCD.Status not in ('Close','Complete')) then 1 else 0 end) ToClose,
 (case when (QGD.IsFailValue <> 0 and QCD.Status not in ('Complete')) then 1 else 0 end) ToConfirm,QC.ProcessId,QCD.Status ParameterStatus
 from TRN.QualityControlDetails QCD
 left join TRN.QualityControl QC on QC.Id=QCD.QCId
 left join MST.QualityManagementMaster QMM on QMM.Id=QC.IssueId
 left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
 left join MST.QualityActionToBeTakenDetails QAT on QAT.Id=QCD.ActionToBeTaken
 left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
 left join TRN.QualityActionTakenUpdate QAU on QAU.ParameterId=QCD.Id
 left join EmployeeInformation QAE on QAE.SystemId=QAU.ActionById
 where QCD.ItemId in (select Id from MST.QualityManagementParameterItem where CustomerParameter = 1)) QCData on 
QCData.IssueId=M.IssueId and 
QCData.ParameterId=M.ParameterId
 and QCData.ProductionOrderId=M.ProductionOrderId
and QCData.LotNumber=M.LotNumber
where M.ProductionOrderId='" + ProductionOrderId + "' and M.LotNumber='" + LotNumber + "' and M.EntityId='" + EntityId + "' order by M.ParameterSequence,QCData.QCDDate,QCData.QCDTime";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
                objCon.CommitTransaction();
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

        public void GetProductionDatabyPOId(string POId, out DataSet dsPO)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string sql = @"SELECT ps.Id,PS.Quantity,PS.ProductionOrderId PONo,FORMAT(PS.ProductionDate,'dd-MMM-yyyy')ProductionDate,P.UserName Process,POS.Sequence ProcessSetSeq,PS.LotNumber LotNo
,WC.Code WorkCenterMaster,EI.EmployeeName ResponsiblePerson,PS.Remarks,IsBaseProcess=CASE WHEN POS.IsBaseProcess=1 THEN 'Yes' ELSE 'No' END
 FROM TRN.ProductionSummary PS
LEFT JOIN TRN.ProductionOrderProcessSet POS ON POS.ProductionOrderId=PS.ProductionOrderId AND POS.ProcessId=PS.ProcessId
LEFT JOIN HKP.Process P ON P.Id=PS.ProcessId
LEFT JOIN SCS.WorkCenterMaster WC ON WC.Id=PS.WorkCenterMasterId AND WC.ProcessId=PS.ProcessId
LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=PS.ResponsiblePersonId
where PS.ProductionOrderId='" + POId + "' Order By POS.Sequence,PS.ProductionDate";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(sql, out dsPO);
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Daily Quality Status function
        public void GetDailyQualityStatusParameterJobCardReport(string FromDate, string ToDate, string IssueId, string ProductionOrderId, string LotNumber, string EntityId, string QualityStatus, string Date, string plantId, out DataSet dsRef)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"select distinct '" + QualityStatus + "' QualityStatus, '" + Date + @"' Date,QCData.QCDate,PELP.PONo,isnull(QCData.LotNumber,PELP.LotNumber) LotNumber,PELP.Entity,PELP.EntityId,PELP.PartyNature,
MOLineItemNo = STUFF((select distinct ','+ XMOI.Id from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
where PELP.PONo=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
POStatus=(select UserName from hkp.ProductionStatus where Id=(select ProductionStatusId from TRN.ProductionOrder where Id=PELP.PONo)),
Article=STUFF((select distinct ','+MA.StandardName from
											MST.MaterialMasterArticle MA
											left join TRN.MasterOrderItem moi on moi.ArticleId=MA.Id
											left join trn.SalesOrder AS xp on xp.MasterOrderItemId=moi.Id
											JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=xp.Id
											where PELP.PONo=Xpod.ProductionOrderId for xml path('') ), 1, 1, '')
,Customer= STUFF((select distinct ','+XP.UserName from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where PELP.PONo=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
PELP.IssueId,PELP.Issue IssueName,PELP.Process,PELP.ParameterId,PELP.Parameter ParameterName,PELP.UOM,QCData.Value,QCData.GradeName,QCData.ParameterRemark,QCData.ActionToBeTakenName,QCData.ResponsiblePerson,QCData.PassValue,QCData.FailValue,QCData.RejectValue,QCData.FailGrade,QCData.ToClose,QCData.ToConfirm,
QCData.HeaderId,QCData.ChildId,QCData.QCDDate,QCData.QCDTime, (Case When (QCData.Value is null or QCData.Value = '0') then 1 else 0 end) EntryMissing,
Reverse(stuff(Reverse((select QR.Grade +', ' from MST.QualityRemark QR																			
where QR.PONo=PELP.PONo and QR.LotNo=QCData.LotNumber and QR.EntityId=PELP.EntityId for xml PATH(''))),1,2,'')) Grade,
Reverse(stuff(Reverse((select (select EmployeeName from EmployeeInformation where SystemId=QR.ByWhomId) +', ' from MST.QualityRemark QR																			
where QR.PONo=PELP.PONo and QR.LotNo=QCData.LotNumber and QR.EntityId=PELP.EntityId for xml PATH(''))),1,2,'')) ByWhom,
Reverse(stuff(Reverse((select format(QR.AddedDate,'dd-MMM-yyyy') + '-' + QR.Comment +', ' from MST.QualityRemark QR																			
where QR.PONo=PELP.PONo and QR.LotNo=QCData.LotNumber  and QR.EntityId=PELP.EntityId for xml PATH(''))),1,2,'')) CommentDetails,
Reverse(stuff(Reverse((select isnull(RD.MinRequirement,'') + '/' + isnull(RD.MaxRequirement,'') +', ' from TRN.UCPRequirementDetails RD																			
where RD.ParameterId=QCData.ChildId for xml PATH(''))),1,2,'')) MinMaxRequirement,
Reverse(stuff(Reverse((select isnull(SD.MinStandard,'') + '/' + isnull(SD.MaxStandard,'') +', ' from TRN.UCPMaxMinStandardDetails SD																			
where SD.ParameterId=QCData.ChildId for xml PATH(''))),1,2,'')) MinMaxStandard,
QCData.ActionTaken,QCData.ActionBy,QCData.ConfirmRemarks,QCData.QAURemarks,QCData.ReasonName
from (select  Z.PONo,Z.LotNumber,Z.EntryLevel,Z.ApplicableLot,Z.Entity,Z.EntityId,Z.PartyNature,ELP.Process,ELP.Issue,ELP.IssueId,ELP.Parameter,ELP.ParameterId,ELP.UOM from (select P.PONo,P.LotNumber,
(case when len(P.LotNumber) > 0 then 'LOT' else 'PO' end) EntryLevel,
(case when len(P.LotNumber) > 0 then P.LotNumber else P.PONo end) ApplicableLot,
PartyNature= STUFF((select distinct ','+XP.PartyNature from trn.SalesOrder XSO 
JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
where P.PONo=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
P.EntityId,E.UserName Entity
from  (select distinct PS.ProductionOrderId  PONo,null LotNumber,EntityId
from TRN.ProductionSummary PS
where PS.AddedDate between '" + FromDate + "' and '" + ToDate + @"'
union
select distinct PS.ProductionOrderId  PONo,PS.LotNumber,EntityId 
from TRN.ProductionSummary PS
where PS.AddedDate between '" + FromDate + "' and '" + ToDate + @"')P
left join org.Entity E on E.Id=P.EntityId
)Z
left join (select EntryLevel,P.UserName Process,QMM.UserName Issue,IssueId,QMP.Id ParameterId,PM.UserName Parameter,UOM.UserName UOM from MST.POQualityPlanDetails POD
left join HKP.Process P on P.Id=POD.ProcessId
left join MST.QualityManagementMaster QMM on QMM.Id=POD.IssueId
left join MST.QualityManagementParameterItem QMP on QMP.QMID=POD.IssueId and QMP.ReportApplicable=1
left join HKP.ParameterMaster PM on PM.Id=QMP.ParameterId
left join SCS.UnitOfMeasurement UOM on UOM.Id=QMP.UOMId) ELP on ELP.EntryLevel=Z.EntryLevel)PELP
left join (select PQD.EntryLevel,QC.IssueId,QMM.UserName IssueName,QCD.ItemId ParameterId,PM.UserName ParameterName,QC.LotNumber,QC.ProductionOrderId,
 QCD.Value,QGD.GradeName,QCD.Remarks ParameterRemark,QAT.ActionToBeTakenName,EI.EmployeeName ResponsiblePerson,
 format(QC.AddedDate,'dd-MMM-yyyy') QCDate,format(QCD.AddedDate,'dd-MMM-yyyy') QCDDate,format(QCD.AddedDate,'hh:mm-tt') QCDTime,QC.Id HeaderId,QCD.Id ChildId,QGD.IsPassValue PassValue,QGD.IsFailValue FailValue,QGD.IsRejectValue RejectValue,
 QAU.ActionTaken,QAE.EmployeeName ActionBy,QAU.Remarks QAURemarks,isnull(QAU.ReasonName,(select UserName from [HKP].[QualityManagementReasonMaster] where Id=(select ReasonId from [MST].[QualityManagementParameterReason] where Id=QAU.ReasonId))) ReasonName,QAU.ConfirmRemarks,
(case when QCD.GradeId is null then 1 end) FailGrade,
 (case when (QGD.IsFailValue <> 0 and QCD.Status not in ('Close','Complete')) then 1 else 0 end) ToClose,
 (case when (QGD.IsFailValue <> 0 and QCD.Status not in ('Complete')) then 1 else 0 end) ToConfirm,QC.ProcessId,
 (case when PQD.EntryLevel='PO' then QC.ProductionOrderId else QC.LotNumber end) ApplicableLot
 from TRN.QualityControlDetails QCD
 left join TRN.QualityControl QC on QC.Id=QCD.QCId
  left join MST.POQualityPlanDetails PQD on PQD.IssueId=QC.IssueId
 left join MST.QualityManagementMaster QMM on QMM.Id=PQD.IssueId
 left join MST.QualityManagementParameterItem QMP on QMP.Id=QCD.ItemId
 left join Hkp.ParameterMaster PM on PM.Id=QMP.ParameterId
 left join MST.QualityGradeDetails QGD on QGD.Id=QCD.GradeId
 left join MST.QualityActionToBeTakenDetails QAT on QAT.Id=QCD.ActionToBeTaken
 left join EmployeeInformation EI on EI.SystemId=QCD.ResponsiblePersonId
 left join TRN.QualityActionTakenUpdate QAU on QAU.ParameterId=QCD.Id
 left join EmployeeInformation QAE on QAE.SystemId=QAU.ActionById) QCData on 
 QCData.IssueId=PELP.IssueId and 
QCData.ParameterId=PELP.ParameterId
 and QCData.ProductionOrderId=PELP.PONo
and QCData.ApplicableLot=PELP.ApplicableLot
and QCData.EntryLevel=PELP.EntryLevel
where PELP.PONo='" + ProductionOrderId + "' and QCData.LotNumber='" + LotNumber + "' and PELP.EntityId='" + EntityId + "' order by QCData.QCDDate,QCData.QCDTime";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
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
        }

        // Lot Wise Quality Report function

        public void GetCustomerLotWiseQualityJobCardReport(string CustomerId, string InvoiceId, string ProductionOrderId, string LotNumber, string plantId, out DataSet dsRef)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string LotFilter = string.Empty;
                string CustFilter = string.Empty;
                string InvFilter = string.Empty;
                if (ProductionOrderId != "null" && ProductionOrderId != "undefined")
                {
                    LotFilter = " and QC.ProductionOrderId='" + ProductionOrderId + "' and QC.LotNumber='" + LotNumber + "'";
                }
                else
                {
                    LotFilter = " and QC.LotNumber='" + LotNumber + "'";
                }
                if (CustomerId != null)
                {
                    CustFilter = " and CQH.CustomerId='" + CustomerId + "'";
                }
                if (InvoiceId != null)
                {
                    InvFilter = " and CQH.InvoiceId='" + InvoiceId + "'";
                }
                strSql = @"select format(getdate(),'dd-MMM-yyyy') Date,QC.ProductionOrderId,QC.LotNumber LotNo,'" + CustomerId + "' CustomerId,'" + InvoiceId + @"' InvoiceId,
CustomerName=(select UserName from hkp.Party where Id='" + CustomerId + @"'),
Article = STUFF((select distinct ',' + MA.StandardName from trn.ProductionOrderDetail Pod
left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join[MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
where Pod.ProductionOrderId = QC.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
ProductCode=STUFF((select distinct ',' + PL.Code from trn.ProductionOrderDetail Pod
left outer join trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join ProductLibrary PL on PL.Id=MOI.ProductLibraryId
where Pod.ProductionOrderId = QC.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
Detail=STUFF((select distinct ',' + PAL.AttributeValue from trn.ProductionOrderDetail Pod
left outer join trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join ProductLibrary PL on PL.Id=MOI.ProductLibraryId
left outer join ProductLibraryAttribute PAL on PAL.ProductLibraryId=PL.Id
where Pod.ProductionOrderId = QC.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
QMP.Id ParameterId,CQH.Id CQRHeaderId,CQH.UserName,CQH.Remarks,CQH.ByWhomId,
(select EmployeeName from EmployeeInformation where systemId=CQH.ByWhomId) as ByWhom,
(select top 1 Id from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId) as Id,
(select top 1 ParaRemarks from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId) as ParaRemarks,
isnull((select top 1 Value from [TRN].[CustomerQualityReportDetails] where CQRHeaderId=CQH.Id and ParameterId=QMP.Id and UOMId=QMP.UOMId),QCD.Value) Value,
PM.UserName Parameter,QMP.UOMId,UM.UserName UOM,
Reverse(stuff(Reverse((select QR.Grade +', ' from MST.QualityRemark QR																			
where QR.PONo=QC.ProductionOrderId and QR.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) QRGrade,
Reverse(stuff(Reverse((select (select EmployeeName from EmployeeInformation where SystemId=QR.ByWhomId) +', ' from MST.QualityRemark QR																			
where QR.PONo=QC.ProductionOrderId and QR.LotNo=QC.LotNumber  for xml PATH(''))),1,2,'')) QRByWhom,
Reverse(stuff(Reverse((select QR.Comment +', ' from MST.QualityRemark QR																			
where QR.PONo=QC.ProductionOrderId and QR.LotNo=QC.LotNumber   for xml PATH(''))),1,2,'')) QRComment,
Reverse(stuff(Reverse((select OWC.Grade +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.PONo=QC.ProductionOrderId and OWC.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) OWGrade,
Reverse(stuff(Reverse((select (select EmployeeName from EmployeeInformation where SystemId=(Select AuthorizedResPersonId from HKP.QualityManagementAuthorizedPerson where Id=OWC.ByWhomId)) +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.PONo=QC.ProductionOrderId and OWC.LotNo=QC.LotNumber  for xml PATH(''))),1,2,'')) OWByWhom,
Reverse(stuff(Reverse((select OWC.Comment +', ' from MST.OrderWiseQualityComment OWC																			
where OWC.PONo=QC.ProductionOrderId and OWC.LotNo=QC.LotNumber for xml PATH(''))),1,2,'')) OWComment
from MST.QualityManagementParameterItem QMP
left join TRN.QualityControlDetails QCD on QCD.ItemId=QMP.Id
left join TRN.QualityControl QC on QC.Id=QCD.QCID
left join HKP.ParameterMaster PM on PM.Id=QMP.ParameterId
left join [MST].[QualityManagementCPSequence] CPS on CPS.ParameterId=PM.Id and CPS.QMPId=CQD.ParameterId
left join SCS.UnitOfMeasurement UM on UM.Id=QMP.UOMId
left join[TRN].[CustomerQualityReportHeader] CQH on CQH.ProductionOrderId='" + ProductionOrderId + "' and CQH.LotNo='" + LotNumber + "' " + CustFilter + " " + InvFilter + @"
where CustomerParameter=1 and Finalreport=1 and QCD.GradeId is not null" + LotFilter + " order by CPS.Sequence";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
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
        }

        public void GetCustomerQualityLotWiseUpdateJobCardReport(string CustomerId, string InvoiceId, string ProductionOrderId, string LotNumber, string plantId, out DataSet dsRef)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string POFilter = string.Empty;
                string LotFilter = string.Empty;
                string CustFilter = string.Empty;
                string InvFilter = string.Empty;
                if (ProductionOrderId != "null" && ProductionOrderId != "undefined")
                {

                    POFilter = "and CQH.ProductionOrderId='" + ProductionOrderId + "'";
                }
                if (LotNumber != "null" && LotNumber != "undefined")
                {
                    LotFilter = "and CQH.LotNo='" + LotNumber + "'";
                }
                if (CustomerId != "null" && CustomerId != "undefined")
                {
                    CustFilter = " and CQH.CustomerId='" + CustomerId + "'";
                }
                if (InvoiceId != "null" && InvoiceId != "undefined")
                {
                    InvFilter = " and CQH.InvoiceId='" + InvoiceId + "'";
                }
                strSql = @"select format(getdate(),'dd-MMM-yyyy') Date,CQH.ProductionOrderId,CQH.LotNo,CQH.CustomerId,CQH.InvoiceId,
CustomerName=(select UserName from hkp.Party where Id=CQH.CustomerId),
Article = STUFF((select distinct ',' + MA.StandardName from trn.ProductionOrderDetail Pod
left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join[MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
where Pod.ProductionOrderId = CQH.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
ProductCode=STUFF((select distinct ',' + PL.Code from trn.ProductionOrderDetail Pod
left outer join trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join ProductLibrary PL on PL.Id=MOI.ProductLibraryId
where Pod.ProductionOrderId = CQH.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
Detail=STUFF((select distinct ',' + PAL.AttributeValue from trn.ProductionOrderDetail Pod
left outer join trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join ProductLibrary PL on PL.Id=MOI.ProductLibraryId
left outer join ProductLibraryAttribute PAL on PAL.ProductLibraryId=PL.Id
where Pod.ProductionOrderId = CQH.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
CQD.ParameterId,CQH.Id CQRHeaderId,CQH.UserName,CQH.Remarks,CQH.ByWhomId,
(select EmployeeName from EmployeeInformation where systemId=CQH.ByWhomId) as ByWhom,
CQD.Id,CQD.ParaRemarks,CQD.Value,PM.UserName Parameter,CQD.UOMId,UM.UserName UOM
from TRN.CustomerQualityReportHeader CQH
left Join (Select * from TRN.CustomerQualityReportDetails Where FinalReport=1) CQD on CQD.CQRHeaderId=CQH.Id 
left join HKP.ParameterMaster PM on PM.Id=(select ParameterId from MST.QualityManagementParameterItem where id=CQD.ParameterId)
left join [MST].[QualityManagementCPSequence] CPS on CPS.ParameterId=PM.Id and CPS.QMPId=CQD.ParameterId
left join SCS.UnitOfMeasurement UM on UM.Id=CQD.UOMId
where 1=1 " + POFilter + " " + LotFilter + "" + CustFilter + " " + InvFilter + " order by CPS.Sequence";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
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
        }

        public void GetCustomerLWQSummaryJobCardReport(string CustomerId, string InvoiceId, string ProductionOrderId, string LotNumber, string plantId, out DataSet dsRef)
        {

            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                string POFilter = string.Empty;
                string LotFilter = string.Empty;
                string CustFilter = string.Empty;
                string InvFilter = string.Empty;
                if (ProductionOrderId != "null" && ProductionOrderId != "undefined")
                {

                    POFilter = "and CQH.ProductionOrderId='" + ProductionOrderId + "'";
                }
                if (LotNumber != "null" && LotNumber != "undefined")
                {
                    LotFilter = "and CQH.LotNo='" + LotNumber + "'";
                }
                if (CustomerId != "null" && CustomerId != "undefined")
                {
                    CustFilter = " and CQH.CustomerId='" + CustomerId + "'";
                }
                if (InvoiceId != "null" && InvoiceId != "undefined")
                {
                    InvFilter = " and CQH.InvoiceId='" + InvoiceId + "'";
                }
                strSql = @"select format(getdate(),'dd-MMM-yyyy') Date,CQH.ProductionOrderId,CQH.LotNo,CQH.CustomerId,CQH.InvoiceId,
CustomerName=(select UserName from hkp.Party where Id=CQH.CustomerId),
Article = STUFF((select distinct ',' + MA.StandardName from trn.ProductionOrderDetail Pod
left outer JOIN trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join[MST].[MaterialMasterArticle] MA ON ma.Id = moi.ArticleId
where Pod.ProductionOrderId = CQH.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
ProductCode=STUFF((select distinct ',' + PL.Code from trn.ProductionOrderDetail Pod
left outer join trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join ProductLibrary PL on PL.Id=MOI.ProductLibraryId
where Pod.ProductionOrderId = CQH.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
Detail=STUFF((select distinct ',' + PAL.AttributeValue from trn.ProductionOrderDetail Pod
left outer join trn.SalesOrder sO ON pod.SalesOrderId = so.Id
left outer join trn.MasterOrderItem MOI on moi.Id = so.MasterOrderItemId
left outer join ProductLibrary PL on PL.Id=MOI.ProductLibraryId
left outer join ProductLibraryAttribute PAL on PAL.ProductLibraryId=PL.Id
where Pod.ProductionOrderId = CQH.ProductionOrderId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
CQD.ParameterId,CQH.Id CQRHeaderId,CQH.UserName,CQH.Remarks,CQH.ByWhomId,
(select EmployeeName from EmployeeInformation where systemId=CQH.ByWhomId) as ByWhom,
CQD.Id,CQD.ParaRemarks,CQD.Value,PM.UserName Parameter,CQD.UOMId,UM.UserName UOM
from TRN.CustomerQualityReportHeader CQH
left Join TRN.CustomerQualityReportDetails CQD on CQD.CQRHeaderId=CQH.Id
left join HKP.ParameterMaster PM on PM.Id=(select ParameterId from MST.QualityManagementParameterItem where id=CQD.ParameterId)
left join [MST].[QualityManagementCPSequence] CPS on CPS.ParameterId=PM.Id and CPS.QMPId=CQD.ParameterId
left join SCS.UnitOfMeasurement UM on UM.Id=CQD.UOMId
where 1=1 " + POFilter + " " + LotFilter + "" + CustFilter + " " + InvFilter + " order by CPS.Sequence";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.getDataSet(strSql, out dsRef);
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
        }
    }

}

