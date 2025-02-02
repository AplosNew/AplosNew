using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using static Library.Service.Extension.Mail.HumanResourceMailService;

namespace Library.HumanResource.Report.Attendance
{
    public class clsMonthlyAttendanceInformation
    {
        ISqlRepository _sqlRepository;
        public clsMonthlyAttendanceInformation()
        {
            _sqlRepository = new SqlRepository();
        }
        public IWorkbook XlsMonthlyAttendanceSummaryReportDateRange(string companyId, string plantId, string FromDate, string ToDate, string userName, string DayStatus, Dictionary<string, string> empParameters, bool withColor, bool includeCurrentDate, bool withSummary, bool isActive, bool isSeperated, bool isMaternity)
        {
            #region Variable

            clsReport objRpt = null;
            clsSalaryProc objRptSal = null;

            DataSet dsMonthlyAttnSumm = null;
            DataView dvMonthlyAttnSumm = null;
            DataSet dsDaily = null;
            DataTable dtDaily = null;
            DataView dvDaily = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            string FactoryName = "";
            string CmpName = "";

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;

            DateTime dtFrmDt = Convert.ToDateTime(FromDate);
            DateTime dtEndDate = Convert.ToDateTime(ToDate);

            if (dtFrmDt.ToString("MM") != dtEndDate.ToString("MM"))
            {
                throw new Exception("Date must be in same Month");
            }
            if (dtFrmDt.ToString("yyyy") != dtEndDate.ToString("yyyy"))
            {
                throw new Exception("Date must be in same Year");
            }
            //if (!includeCurrentDate)
            //{

            //    dtEndDate = dtEndDate.AddDays(-1);
            //}
            DataSet dsSLeave = null;
            DataView dvSLeave = null;

            #endregion Variable

            try
            {


                objRpt = new clsReport(_sqlRepository);
                objRptSal = new clsSalaryProc();


                #region Variable
                //Library.HumanResource.Report.Attendance.clsMonthlyAttendanceInformation clsMonthlyAttendanceNew = new Library.HumanResource.Report.Attendance.clsMonthlyAttendanceInformation();
                ParaMontlyAttendance objm = new global::ParaMontlyAttendance();

                objm.UnitId = "ALL";
                objm.DivisionId = "ALL";
                objm.DepartmentId = "ALL";
                objm.SectionId = "ALL";
                objm.SubsectionId = "ALL";
                objm.LineId = "ALL";
                objm.EmpCat = "ALL";
                objm.DesignationGroupId = "ALL";
                objm.DesignationId = "ALL";
                objm.JoblocationName = "ALL";

                objm.PlantId = plantId;
                //objm.AMonth = Month;
                //objm.AYear = Year;
                objm.FDate = dtFrmDt.ToString("dd-MMM-yyyy");
                objm.TDate = dtEndDate.ToString("dd-MMM-yyyy");
                #endregion Variable


                #region DataSet --Detail Attendance Data with Header
                string sEmpSystemID = "''";
                if (empParameters.Count > 0)
                {
                    if (empParameters.Keys.ElementAt(0) != "")
                    {
                        sEmpSystemID += @"'" + empParameters["EmpSystemId"] + "'";
                    }
                }
                DataSet dsAttdnSumm = null;
                //objRptSal.GetAttdnDataForMonthlyProcPlantAndEmpWise(sEmpSystemID,objm.FDate,objm.TDate,plantId, out dsAttdnSumm);
                Dictionary<string, List<DataRow>> dicAttendance = new Dictionary<string, List<DataRow>>();

                GetMonthlyAttnSummaryRptForDetailsDateRange(objm, empParameters, out dsMonthlyAttnSumm, isActive, isSeperated, isMaternity);
                dvMonthlyAttnSumm = new DataView();
                dvMonthlyAttnSumm.Table = dsMonthlyAttnSumm.Tables[0];

                string _FLAG = "DAYSTATUS";

                if (DayStatus == "DAYSTATUS")
                {
                    _FLAG = "DAYSTATUS";
                }
                else if (DayStatus == "INTIME")
                {
                    _FLAG = "INTIME";
                }
                else if (DayStatus == "OUTTIME")
                {
                    _FLAG = "OUTTIME";
                }
                else if (DayStatus == "INRAW")
                {
                    _FLAG = "INRAW";
                }
                else if (DayStatus == "OUTRAW")
                {
                    _FLAG = "OUTRAW";
                }
                else if (DayStatus == "ALLSTATUS")
                {
                    _FLAG = "ALLSTATUS";
                }
                else
                {
                    _FLAG = "DAYSTATUS";
                }

                if (_FLAG == "INRAW" || _FLAG == "OUTRAW")
                {
                    objRpt.GetMonthlyIntimeOutTimeRaw(_FLAG, empParameters, objm, out dsDaily);
                }
                else
                {
                    //GetMonthlyDailyAttendanceDic(string IsDayStatus, string plantId, string fromDate, string toDate, Dictionary<string, string> parameters, bool isActive, bool isSeperated, bool isMaternity)
                    dicAttendance = objRpt.GetMonthlyDailyAttendanceDic(_FLAG, plantId, dtFrmDt.ToString("dd-MMM-yyyy"), dtEndDate.ToString("dd-MMM-yyyy"), empParameters, isActive, isSeperated, isMaternity);
                    //objRpt.GegMonthlyDaily(_FLAG, empParameters, objm, out dsDaily, isActive,  isSeperated,  isMaternity);
                }

                if (dicAttendance.Count == 0)
                {
                    throw new Exception("Data not found.");

                }



                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(plantId, empParameters, dtFrmDt.Month, dtEndDate.Year, out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                DataSet dsAttdnInfoExtra = null;
                DataTable dtAttdnInfoExtra = null;
                objRpt.GetAttendanceInfoExtra(plantId, dtFrmDt.ToString("dd-MMM-yyyy"), dtEndDate.ToString("dd-MMM-yyyy"), out dsAttdnInfoExtra);
                dtAttdnInfoExtra = dsAttdnInfoExtra.Tables[0];
                int earlyOut = 0;
                int lateIn = 0;

                Dictionary<string, string> dicExtraAbsentWeekOFF = GetExtraAbsentWeekOFF(plantId, dtEndDate.Month.ToString(), dtEndDate.Year.ToString());
                Dictionary<string, string> dicExtraAbsentHoliday = GetExtraAbsentHoliday(plantId, dtEndDate.Month.ToString(), dtEndDate.Year.ToString());


                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);

                objRpt.SelectedPlant(plantId, out dsFactory);

                dsSLeave = new DataSet();
                objRpt.GetShortLeave(objm, empParameters, out dsSLeave);
                dvSLeave = new DataView(dsSLeave.Tables[0]);

                #endregion DataSet

                if (dvMonthlyAttnSumm.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;
                    workbook.Version = ExcelVersion.Excel97to2003;
                    xlsRow = 6;

                    #region StyleSheet

                    IStyle baseStyle = workbook.Styles.Add("BaseStyle");
                    baseStyle.Font.Color = ExcelKnownColors.Black;
                    baseStyle.Color = System.Drawing.Color.White;
                    baseStyle.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
                    baseStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Hair;
                    baseStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Hair;
                    baseStyle.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Hair;

                    IStyle absentStyle = workbook.Styles.Add("AbsentStyle");
                    //absentStyle = baseStyle;
                    absentStyle.Font.Color = ExcelKnownColors.White;
                    absentStyle.Color = System.Drawing.Color.Red;

                    IStyle presentStyle = workbook.Styles.Add("PresentStyle");
                    //presentStyle = baseStyle;
                    presentStyle.Font.Color = ExcelKnownColors.White;
                    presentStyle.Color = System.Drawing.Color.Green;

                    IStyle noOUTtimeStyle = workbook.Styles.Add("NoOUTtimeStyle");
                    //noOUTtimeStyle = baseStyle;
                    noOUTtimeStyle.Font.Color = ExcelKnownColors.White;
                    noOUTtimeStyle.Color = System.Drawing.Color.Violet;

                    IStyle lateStyle = workbook.Styles.Add("LateStyle");
                    //lateStyle = baseStyle;
                    lateStyle.Font.Color = ExcelKnownColors.White;
                    lateStyle.Color = System.Drawing.Color.Blue;


                    IStyle leaveStyle = workbook.Styles.Add("LeaveStyle");
                    //leaveStyle = baseStyle;
                    leaveStyle.Font.Color = ExcelKnownColors.Black;
                    leaveStyle.Color = System.Drawing.Color.Yellow;


                    IStyle isManualandNotLeaveStyle = workbook.Styles.Add("IsManualandNotLeaveStyle");
                    //isManualandNotLeaveStyle = baseStyle;
                    isManualandNotLeaveStyle.Font.Color = ExcelKnownColors.White;
                    isManualandNotLeaveStyle.Color = System.Drawing.Color.Orange;



                    IStyle isHalfLeaveStyle = workbook.Styles.Add("IsHalfLeaveStyle");
                    //isHalfLeaveStyle = baseStyle;
                    isHalfLeaveStyle.Font.Color = ExcelKnownColors.Yellow;
                    isHalfLeaveStyle.Font.Bold = true;

                    IStyle isExtraAbsentStyle = workbook.Styles.Add("IsExtraAbsentStyle");
                    //isExtraAbsentStyle = baseStyle;
                    isExtraAbsentStyle.Font.Color = ExcelKnownColors.Red;
                    isExtraAbsentStyle.Font.Bold = true;


                    IStyle isShortLeaveStyle = workbook.Styles.Add("IsShortLeaveStyle");
                    ////isShortLeaveStyle = baseStyle;
                    isShortLeaveStyle.Font.Color = ExcelKnownColors.Magenta;
                    isShortLeaveStyle.Font.Bold = true;



                    #endregion.


                    #region Variables

                    int strCount = 0;

                    int iSrNo = 0;
                    int iEmpCode = 0;
                    int iEmpName = 0;
                    int iDOJ = 0;
                    int iDOS = 0;
                    int iUnit = 0;
                    int iDepart = 0;
                    int iSec = 0;
                    int iSubSection = 0;
                    int iDesig = 0;
                    int iTtlAPD = 0;
                    int cPayDays = 0;
                    int iTtlHD = 0;
                    int iTtlWO = 0;
                    int iTtlPst = 0;
                    int iTtlAbs = 0;
                    int iTtlLte = 0;
                    int iTtlLv = 0;
                    int iTtlLWP = 0;
                    int iTsl = 0;
                    int iTtlMLv = 0;
                    int iExtraAbs = 0;
                    int iLateIn = 0;
                    int iEarlyOut = 0;
                    int iGender = 0;
                    int iEmpCategory = 0;
                    #endregion

                    #region ------------------Column Header------------------

                    #region ------------------Details Header-----------------

                    xlsRow += 1;

                    xlsCol = 1;
                    iSrNo = xlsCol;
                    sheet1.Range[xlsRow, iSrNo].Text = "Sl No.";
                    sheet1.Range[xlsRow, iSrNo].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iSrNo, xlsRow + 1, iSrNo].Merge();

                    xlsCol += 1;
                    iEmpCode = xlsCol;
                    sheet1.Range[xlsRow, iEmpCode].Text = "Employee Code";
                    sheet1.Range[xlsRow, iEmpCode].ColumnWidth = 8.50;
                    sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iEmpCode, xlsRow + 1, iEmpCode].Merge();

                    xlsCol += 1;
                    iEmpName = xlsCol;
                    sheet1.Range[xlsRow, iEmpName].Text = "Employee Name";
                    sheet1.Range[xlsRow, iEmpName].ColumnWidth = 22;
                    sheet1.Range[xlsRow, iEmpName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iEmpName, xlsRow + 1, iEmpName].Merge();

                    xlsCol += 1;
                    iGender = xlsCol;
                    sheet1.Range[xlsRow, iGender].Text = "Gender";
                    sheet1.Range[xlsRow, iGender].ColumnWidth = 22;
                    sheet1.Range[xlsRow, iGender].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iGender].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iGender, xlsRow + 1, iGender].Merge();

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet1.Range[xlsRow, iDOJ].ColumnWidth = 9.20;
                    sheet1.Range[xlsRow, iDOJ].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iDOJ, xlsRow + 1, iDOJ].Merge();

                    xlsCol += 1;
                    iDOS = xlsCol;
                    sheet1.Range[xlsRow, iDOS].Text = "DOS";
                    sheet1.Range[xlsRow, iDOS].ColumnWidth = 9.20;
                    sheet1.Range[xlsRow, iDOS].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iDOS, xlsRow + 1, iDOS].Merge();


                    xlsCol += 1;
                    iEmpCategory = xlsCol;
                    sheet1.Range[xlsRow, iEmpCategory].Text = "Employee Category";
                    sheet1.Range[xlsRow, iEmpCategory].ColumnWidth = 22;
                    sheet1.Range[xlsRow, iEmpCategory].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iEmpCategory].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iEmpCategory, xlsRow + 1, iEmpCategory].Merge();

                    xlsCol += 1;
                    iUnit = xlsCol;
                    sheet1.Range[xlsRow, iUnit].Text = "Unit";
                    sheet1.Range[xlsRow, iUnit].ColumnWidth = 9;
                    sheet1.Range[xlsRow, iUnit].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iUnit, xlsRow + 1, iUnit].Merge();

                    xlsCol += 1;
                    iDepart = xlsCol;
                    sheet1.Range[xlsRow, iDepart].Text = "Department";
                    sheet1.Range[xlsRow, iDepart].ColumnWidth = 15;
                    sheet1.Range[xlsRow, iDepart].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iDepart].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iDepart, xlsRow + 1, iDepart].Merge();

                    xlsCol += 1;
                    iSec = xlsCol;
                    sheet1.Range[xlsRow, iSec].Text = "Section";
                    sheet1.Range[xlsRow, iSec].ColumnWidth = 15;
                    sheet1.Range[xlsRow, iSec].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSec].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iSec, xlsRow + 1, iSec].Merge();

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet1.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet1.Range[xlsRow, iSubSection].ColumnWidth = 15;
                    sheet1.Range[xlsRow, iSubSection].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iSubSection, xlsRow + 1, iSubSection].Merge();

                    xlsCol += 1;
                    int iLine = xlsCol;
                    sheet1.Range[xlsRow, iLine].Text = "Line";
                    sheet1.Range[xlsRow, iLine].ColumnWidth = 15;
                    sheet1.Range[xlsRow, iLine].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iLine].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iLine, xlsRow + 1, iLine].Merge();

                    xlsCol += 1;
                    iDesig = xlsCol;
                    sheet1.Range[xlsRow, iDesig].Text = "Designation";
                    sheet1.Range[xlsRow, iDesig].ColumnWidth = 15;
                    sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iDesig, xlsRow + 1, iDesig].Merge();

                    //List<SwapColumn> _list2 = GetColDisplayName(dsDaily);
                    xlsCol = iDesig;
                    int StartDayCol = xlsCol;
                    while (dtFrmDt <= dtEndDate)
                    {
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dtFrmDt.ToString("dd");
                        //xlsRow++;
                        sheet1.Range[xlsRow + 1, xlsCol].Text = dtFrmDt.ToString("ddd");
                        if (_FLAG.ToUpper() == "ALLSTATUS")
                        {
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 10;

                        }
                        else
                        {
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 5;
                        }
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 1, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                        dtFrmDt = dtFrmDt.AddDays(1);

                    }
                    xlsRow++;


                    if (withSummary)
                    {
                        xlsCol += 1;
                        iTtlAPD = xlsCol;
                        sheet1.Range[xlsRow - 1, iTtlAPD].Text = "Total Days";
                        sheet1.Range[xlsRow - 1, iTtlAPD].ColumnWidth = 6;
                        sheet1.Range[xlsRow - 1, iTtlAPD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlAPD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlAPD, xlsRow, iTtlAPD].Merge();

                        xlsCol += 1;
                        cPayDays = xlsCol;
                        sheet1.Range[xlsRow - 1, cPayDays].Text = "Pay Days";
                        sheet1.Range[xlsRow - 1, cPayDays].ColumnWidth = 6;
                        sheet1.Range[xlsRow - 1, cPayDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, cPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, cPayDays, xlsRow, cPayDays].Merge();

                        xlsCol += 1;
                        iTtlHD = xlsCol;
                        sheet1.Range[xlsRow, iTtlHD].Text = "Total HoliDay";
                        sheet1.Range[xlsRow, iTtlHD].ColumnWidth = 7.20;
                        sheet1.Range[xlsRow, iTtlHD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iTtlHD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlHD, xlsRow, iTtlHD].Merge();

                        xlsCol += 1;
                        iTtlWO = xlsCol;
                        sheet1.Range[xlsRow - 1, iTtlWO].Text = "Total WeekOff";
                        sheet1.Range[xlsRow - 1, iTtlWO].ColumnWidth = 7.20;
                        sheet1.Range[xlsRow - 1, iTtlWO].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlWO].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlWO, xlsRow, iTtlWO].Merge();

                        xlsCol += 1;
                        iTtlPst = xlsCol;
                        sheet1.Range[xlsRow - 1, iTtlPst].Text = "Total Present (Late included)";
                        sheet1.Range[xlsRow - 1, iTtlPst].ColumnWidth = 10;
                        sheet1.Range[xlsRow - 1, iTtlPst].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlPst].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlPst, xlsRow, iTtlPst].Merge();

                        xlsCol += 1;
                        iTtlAbs = xlsCol;
                        sheet1.Range[xlsRow - 1, iTtlAbs].Text = "Total Absent";
                        sheet1.Range[xlsRow - 1, iTtlAbs].ColumnWidth = 6;
                        sheet1.Range[xlsRow - 1, iTtlAbs].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlAbs].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlAbs, xlsRow, iTtlAbs].Merge();

                        xlsCol += 1;
                        iTtlLte = xlsCol;
                        sheet1.Range[xlsRow - 1, iTtlLte].Text = "Total Late";
                        sheet1.Range[xlsRow - 1, iTtlLte].ColumnWidth = 6;
                        sheet1.Range[xlsRow - 1, iTtlLte].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlLte].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlLte, xlsRow, iTtlLte].Merge();

                        xlsCol += 1;
                        iTtlLv = xlsCol;
                        sheet1.Range[xlsRow - 1, iTtlLv].Text = "Leave";
                        sheet1.Range[xlsRow - 1, iTtlLv].ColumnWidth = 7.20;
                        sheet1.Range[xlsRow - 1, iTtlLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlLv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlLv, xlsRow, iTtlLv].Merge();


                        xlsCol += 1;

                        iTtlMLv = xlsCol;
                        sheet1.Range[xlsRow - 1, iTtlMLv].Text = "Maternity Leave";
                        sheet1.Range[xlsRow - 1, iTtlMLv].ColumnWidth = 15;
                        sheet1.Range[xlsRow - 1, iTtlMLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlMLv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlMLv, xlsRow, iTtlMLv].Merge();

                        xlsCol += 1;
                        iTtlLWP = xlsCol;
                        sheet1.Range[xlsRow - 1, iTtlLWP].Text = "LWP";
                        sheet1.Range[xlsRow - 1, iTtlLWP].ColumnWidth = 7.20;
                        sheet1.Range[xlsRow - 1, iTtlLWP].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlLWP].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlLWP, xlsRow, iTtlLWP].Merge();

                        xlsCol += 1;
                        iExtraAbs = xlsCol;
                        sheet1.Range[xlsRow - 1, iExtraAbs].Text = "Extra Absent";
                        sheet1.Range[xlsRow - 1, iExtraAbs].ColumnWidth = 7.20;
                        sheet1.Range[xlsRow - 1, iExtraAbs].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iExtraAbs].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iExtraAbs, xlsRow, iExtraAbs].Merge();

                        xlsCol += 1;
                        iLateIn = xlsCol;
                        sheet1.Range[xlsRow - 1, iLateIn].Text = "Late In";
                        sheet1.Range[xlsRow - 1, iLateIn].ColumnWidth = 9;
                        sheet1.Range[xlsRow - 1, iLateIn].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iLateIn].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iLateIn, xlsRow, iLateIn].Merge();
                        xlsCol += 1;
                        iEarlyOut = xlsCol;
                        sheet1.Range[xlsRow - 1, iEarlyOut].Text = "Early Out";
                        sheet1.Range[xlsRow - 1, iEarlyOut].ColumnWidth = 9;
                        sheet1.Range[xlsRow - 1, iEarlyOut].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iEarlyOut].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iEarlyOut, xlsRow, iEarlyOut].Merge();
                    }

                    //}

                    #endregion ------------------Details Header-------------------------

                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    xlsCol = 1;
                    xlsRow += 1;
                    int _StartRow = xlsRow;
                    #endregion ------------------Column Header------------------

                    //dvDaily.Table = dtDaily;
                    string attdnStatus = "";
                    string _day_status = "";
                    //#region Attendance Data 
                    for (int i = 0; i <= dvMonthlyAttnSumm.Count - 1; i++)
                    {

                        xlsCol = 1;

                        #region ----------------------Data-----------------------
                        strCount += 1;
                        sheet1.Range[xlsRow, iSrNo].Number = strCount;
                        sheet1.Range[xlsRow, iEmpCode].Text = dvMonthlyAttnSumm[i]["EmployeeCode"].ToString().Trim();
                        sheet1.Range[xlsRow, iEmpName].Text = dvMonthlyAttnSumm[i]["EmployeeName"].ToString().ToUpper();
                        sheet1.Range[xlsRow, iGender].Text = dvMonthlyAttnSumm[i]["GenderID"].ToString().ToUpper();
                        sheet1.Range[xlsRow, iDOJ].Text = dvMonthlyAttnSumm[i]["DOJ"].ToString().Trim();
                        sheet1.Range[xlsRow, iDOS].Text = dvMonthlyAttnSumm[i]["DOS"].ToString().Trim();
                        sheet1.Range[xlsRow, iUnit].Text = dvMonthlyAttnSumm[i]["Unit"].ToString().Trim();
                        sheet1.Range[xlsRow, iEmpCategory].Text = dvMonthlyAttnSumm[i]["EmployeeCategory"].ToString().Trim();
                        sheet1.Range[xlsRow, iDepart].Text = dvMonthlyAttnSumm[i]["Department"].ToString().Trim();
                        sheet1.Range[xlsRow, iSec].Text = dvMonthlyAttnSumm[i]["Section"].ToString().Trim();
                        sheet1.Range[xlsRow, iSubSection].Text = dvMonthlyAttnSumm[i]["SubSection"].ToString().Trim();
                        sheet1.Range[xlsRow, iLine].Text = dvMonthlyAttnSumm[i]["Line"].ToString().Trim();

                        sheet1.Range[xlsRow, iDesig].Text = dvMonthlyAttnSumm[i]["LegalDG"].ToString().Trim();

                        dtFrmDt = Convert.ToDateTime(objm.FDate);
                        xlsCol = iDesig;
                        string ecode = dvMonthlyAttnSumm[i]["EmployeeCode"].ToString().Trim();
                        string _SystemId = dvMonthlyAttnSumm[i]["EmployeePK"].ToString().Trim();

                        #region Attendance Data Plotting
                        try
                        {
                            if (dicAttendance.ContainsKey(_SystemId))
                            {


                                List<DataRow> drData = dicAttendance[_SystemId];

                                foreach (DataRow item in drData)
                                {
                                    bool HasOUTtime = true;
                                    bool IsHalfLeave = false;
                                    bool IsManual = false;
                                    bool IsExtraAbsent = false;
                                    bool IsShortLeave = false;
                                    try
                                    {
                                        attdnStatus = "";
                                        _day_status = "";
                                        _day_status = item["DayStatus"].ToString();
                                        if (_FLAG.ToUpper() == "DAYSTATUS")
                                        {
                                            if (item["DayCategory"].ToString().ToUpper() == "Leave".ToUpper())
                                            {
                                                attdnStatus = item["LeaveCode"].ToString();
                                            }
                                            else
                                            {
                                                attdnStatus = item["DayStatus"].ToString();
                                            }
                                        }
                                        else if (_FLAG.ToUpper() == "ALLSTATUS")
                                        {
                                            if (item["DayCategory"].ToString().ToUpper() == "Leave".ToUpper())
                                            {
                                                attdnStatus = item["LeaveCode"].ToString();

                                            }
                                            else
                                            {
                                                attdnStatus = item["DayStatus"].ToString() + Environment.NewLine + item["ShiftName"].ToString()
                                                              + Environment.NewLine + item["InTime"].ToString() + Environment.NewLine + item["OutTime"].ToString();
                                            }

                                        }


                                        sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].Text = attdnStatus;

                                        sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Font.FontName = "Arial Narrow";
                                        sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle.Font.Size = 17;

                                        sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].BorderAround(ExcelLineStyle.Hair);
                                        //sheet1.Range[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString()), xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].BorderInside(ExcelLineStyle.Hair);
                                        ////dvSLeave.RowFilter = "EmployeeSystemID='" + _SystemId + "' and PDate='" + item["PDate"].ToString() + "'";
                                        ////if (dvSLeave.Count > 0)
                                        ////{
                                        ////    IsShortLeave = true;
                                        ////}
                                        if (clsStaticInfo.dbl(item["LeaveDuration"].ToString()) == 0.5)
                                        {

                                            IsHalfLeave = true;
                                        }
                                        //dvSLeave.RowFilter = "EmployeeSystemID='" + _SystemId + "' and PDate='" + item["PDate"].ToString() + "'";
                                        if (clsStaticInfo.dbl(item["CountedShortLeave"].ToString()) > 0)
                                        {
                                            IsShortLeave = true;
                                        }

                                        dvExtraAbsent.RowFilter = "EmpSystemID='" + _SystemId + "' and WorkingDate='" + item["PDate"].ToString() + "'";
                                        if (dvExtraAbsent.Count > 0)
                                        {
                                            IsExtraAbsent = true;
                                        }
                                        if (string.IsNullOrEmpty(item["OutTime"].ToString()))
                                        {
                                            HasOUTtime = false;
                                        }


                                        ///manual
                                        if (item["MANUALStatus"].ToString().ToUpper() == "MANUAL")
                                        {
                                            IsManual = true;

                                        }



                                    }
                                    catch (Exception ex)
                                    {



                                    }

                                    if (withColor == true)
                                    {

                                        try
                                        {
                                            if (!HasOUTtime && (item["DayCategory"].ToString().ToUpper() != "WEEKEND" || item["DayCategory"].ToString().ToUpper() != "HOLIDAY"))
                                            {
                                                sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = noOUTtimeStyle;

                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Violet;
                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.White;
                                            }
                                            if (_day_status == "P")
                                            {
                                                sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = presentStyle;

                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Green;
                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.White;
                                            }
                                            if (_day_status == "A")
                                            {
                                                sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = absentStyle;

                                            }
                                            if (_day_status == "L" || _day_status == "LVL" || _day_status == "WL" || _day_status == "HL")
                                            {
                                                sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = lateStyle;

                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Blue;
                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.White;
                                            }
                                            if (_day_status.Contains("LV"))
                                            {
                                                sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = leaveStyle;


                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Yellow;
                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.Black;
                                            }

                                            if (IsManual && !_day_status.Contains("LV"))
                                            {

                                                sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = isManualandNotLeaveStyle;

                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Orange;
                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.White;
                                            }

                                            if (IsHalfLeave)
                                            {

                                                sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = isHalfLeaveStyle;

                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.Yellow;
                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Font.Bold = true;
                                            }

                                            if (IsExtraAbsent)
                                            {

                                                sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = isExtraAbsentStyle;

                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.Red;
                                                //sheet1.Range[xlsRow, _col_index].CellStyle.Font.Bold = true;
                                            }
                                            if (IsShortLeave)
                                            {
                                                if (_day_status == "P")
                                                {
                                                    isShortLeaveStyle.Color = System.Drawing.Color.Green;
                                                }
                                                if (_day_status == "L" || _day_status == "LVL" || _day_status == "WL" || _day_status == "HL")
                                                {
                                                    isShortLeaveStyle.Color = System.Drawing.Color.Blue;
                                                }
                                                if (IsManual && !_day_status.Contains("LV"))
                                                {
                                                    isShortLeaveStyle.Color = System.Drawing.Color.Orange;

                                                }

                                                sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = isShortLeaveStyle;

                                            }
                                        }
                                        catch (Exception)
                                        {


                                        }
                                    }
                                }//if count
                                dtFrmDt = dtFrmDt.AddDays(1);
                            }//date

                            //if (chkAdditionInfo.Checked == true)
                            //{
                            if (withSummary)
                            {
                                earlyOut = dtAttdnInfoExtra.Select("InfoType = 'EARLYOUT' AND EmpSystemId = '" + _SystemId + "'").Length;

                                lateIn = dtAttdnInfoExtra.Select("InfoType = 'LATEIN' AND EmpSystemId = '" + _SystemId + "'").Length;
                                decimal _ExtraAbsent = 0;
                                dvExtraAbsent.RowFilter = "EmpSystemID='" + _SystemId + "' ";
                                _ExtraAbsent = dvExtraAbsent.Count;


                                ReportUtility ru = new ReportUtility();
                                sheet1.Range[xlsRow, iTtlAPD].Text = dvMonthlyAttnSumm[i]["TotalProcDate"].ToString().Trim();
                                sheet1.Range[xlsRow, iTtlAPD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlAPD].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                var DaysInaMonth = bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalProcDate"].ToString().Trim());
                                var TotalAbsent = bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalAbsent"].ToString().Trim());
                                var TotalLWP = bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLWP"].ToString().Trim());
                                //var DaysInaMonth = _ExtraAbsent;

                                double _pay_days = 0.00;

                                if (!String.IsNullOrEmpty(dvMonthlyAttnSumm[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                                {
                                    if (dvMonthlyAttnSumm[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                                    {
                                        _pay_days = clsStaticInfo.dbl(dvMonthlyAttnSumm[i]["TotalProcDate"].ToString()) - (Convert.ToDouble(TotalAbsent) + Convert.ToDouble(TotalLWP) + Convert.ToDouble(_ExtraAbsent)) - clsStaticInfo.dbl(dvMonthlyAttnSumm[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dvMonthlyAttnSumm[i]["TotalWeekOff"].ToString());
                                    }
                                    if (dvMonthlyAttnSumm[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                                    {
                                        _pay_days = clsStaticInfo.dbl(dvMonthlyAttnSumm[i]["TotalProcDate"].ToString()) - (Convert.ToDouble(TotalAbsent) + Convert.ToDouble(TotalLWP) + Convert.ToDouble(_ExtraAbsent)) - clsStaticInfo.dbl(dvMonthlyAttnSumm[i]["TotalWeekOff"].ToString());
                                    }
                                }
                                else
                                {
                                    _pay_days = clsStaticInfo.dbl(dvMonthlyAttnSumm[i]["TotalProcDate"].ToString()) - (Convert.ToDouble(TotalAbsent) + Convert.ToDouble(TotalLWP) + Convert.ToDouble(_ExtraAbsent));
                                }



                                //_pay_days = Convert.ToDouble(DaysInaMonth) - (Convert.ToDouble(TotalAbsent) + Convert.ToDouble(TotalLWP) + Convert.ToDouble(_ExtraAbsent));

                                sheet1.Range[xlsRow, cPayDays].Text = _pay_days.ToString();
                                sheet1.Range[xlsRow, cPayDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, cPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iTtlHD].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalHoliDay"].ToString().Trim()));
                                sheet1.Range[xlsRow, iTtlHD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlHD].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                sheet1.Range[xlsRow, iTtlWO].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalWeekOff"].ToString().Trim()));
                                sheet1.Range[xlsRow, iTtlWO].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlWO].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                double _pre = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalPresent"].ToString().Trim()));
                                double _Late = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLate"].ToString().Trim()));

                                double TPresentAndLate = _pre + _Late;
                                sheet1.Range[xlsRow, iTtlPst].Number = TPresentAndLate;
                                sheet1.Range[xlsRow, iTtlPst].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlPst].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iTtlAbs].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalAbsent"].ToString().Trim()));
                                sheet1.Range[xlsRow, iTtlAbs].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlAbs].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iTtlLte].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLate"].ToString().Trim()));
                                sheet1.Range[xlsRow, iTtlLte].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlLte].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iTtlLWP].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLWP"].ToString().Trim()));
                                sheet1.Range[xlsRow, iTtlLWP].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlLWP].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iExtraAbs].Number = Convert.ToDouble(_ExtraAbsent);
                                sheet1.Range[xlsRow, iExtraAbs].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iExtraAbs].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iTtlLv].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLv"].ToString().Trim())) - Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalMLv"].ToString().Trim()));
                                sheet1.Range[xlsRow, iTtlLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlLv].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iTtlMLv].Number = System.Math.Abs(Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalMLv"].ToString().Trim())));
                                sheet1.Range[xlsRow, iTtlMLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlMLv].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                sheet1.Range[xlsRow, iLateIn].Number = lateIn;
                                sheet1.Range[xlsRow, iLateIn].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iLateIn].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iEarlyOut].Number = earlyOut;
                                sheet1.Range[xlsRow, iEarlyOut].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iEarlyOut].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }
                            //var sl = dvMonthlyAttnSumm[i]["ShortLeave"].ToString().Trim();
                            //if (sl == "0")
                            //{
                            //    sl = null;
                            //}
                            //sheet1.Range[xlsRow, iTsl].Text = sl;
                            //sheet1.Range[xlsRow, iTsl].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            //sheet1.Range[xlsRow, iTsl].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            //}

                            xlsRow += 1;

                            #endregion ----------------------Data-----------------------


                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    #endregion

                    #region Line Setup
                    try
                    {
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
                    }
                    catch (Exception)
                    {


                    }
                    #endregion

                    #region UsedRange Alignment
                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region ******************Report Header******************
                    xlsRow = 1;
                    xlsCol = 1;
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
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].RowHeight = 30;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet1.Range[xlsRow, 1].CellStyle.Rotation

                    // start color indication  by Mirza
                    if (withColor == true)
                    {
                        sheet1.Range[xlsRow, endXlsCol - 4, xlsRow, endXlsCol - 1].Merge();
                        sheet1.Range[xlsRow, endXlsCol - 4].Text = "Color Indication";
                        sheet1.Range[xlsRow, endXlsCol - 4].CellStyle.Font.Bold = true;
                        sheet1.Range[xlsRow, endXlsCol - 4].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                        sheet1.Range[xlsRow, endXlsCol - 4].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, endXlsCol - 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow + 1, endXlsCol - 4].Text = "Present";
                        sheet1.Range[xlsRow + 1, endXlsCol - 3].CellStyle.Interior.Color = System.Drawing.Color.Green;
                        sheet1.Range[xlsRow + 1, endXlsCol - 4].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 1, endXlsCol - 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow + 1, endXlsCol - 2].Text = "Absent";
                        sheet1.Range[xlsRow + 1, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Red;
                        sheet1.Range[xlsRow + 1, endXlsCol - 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 1, endXlsCol - 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow + 2, endXlsCol - 4].Text = "Leave";
                        sheet1.Range[xlsRow + 2, endXlsCol - 3].CellStyle.Interior.Color = System.Drawing.Color.Yellow;
                        sheet1.Range[xlsRow + 2, endXlsCol - 4].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 2, endXlsCol - 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow + 2, endXlsCol - 2].Text = "Half Day Leave";
                        sheet1.Range[xlsRow + 2, endXlsCol - 2].WrapText = true;
                        sheet1.Range[xlsRow + 2, endXlsCol - 2].CellStyle.Font.Size = 8;
                        sheet1.Range[xlsRow + 2, endXlsCol - 1].CellStyle.Font.Color = ExcelKnownColors.Yellow;
                        sheet1.Range[xlsRow + 2, endXlsCol - 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 2, endXlsCol - 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow + 2, endXlsCol - 1].Text = "Yellow Font";
                        sheet1.Range[xlsRow + 2, endXlsCol - 1].WrapText = true;
                        sheet1.Range[xlsRow + 2, endXlsCol - 1].CellStyle.Font.Size = 8;
                        sheet1.Range[xlsRow + 2, endXlsCol - 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 2, endXlsCol - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;



                        sheet1.Range[xlsRow + 3, endXlsCol - 2].Text = "Late";
                        sheet1.Range[xlsRow + 3, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Blue;

                        sheet1.Range[xlsRow + 3, endXlsCol - 4].Text = "Out T Miss:";
                        sheet1.Range[xlsRow + 3, endXlsCol - 4].WrapText = true;
                        sheet1.Range[xlsRow + 3, endXlsCol - 4].CellStyle.Font.Size = 8;
                        sheet1.Range[xlsRow + 3, endXlsCol - 3].CellStyle.Interior.Color = System.Drawing.Color.Violet;

                        sheet1.Range[xlsRow + 4, endXlsCol - 4].Text = "Manual Attdn:";
                        sheet1.Range[xlsRow + 4, endXlsCol - 4].WrapText = true;
                        sheet1.Range[xlsRow + 4, endXlsCol - 4].CellStyle.Font.Size = 8;
                        sheet1.Range[xlsRow + 4, endXlsCol - 3].CellStyle.Interior.Color = System.Drawing.Color.Orange;

                        sheet1.Range[xlsRow + 4, endXlsCol - 2].Text = "Short Leave";
                        sheet1.Range[xlsRow + 4, endXlsCol - 2].WrapText = true;
                        sheet1.Range[xlsRow + 4, endXlsCol - 2].CellStyle.Font.Size = 8;
                        sheet1.Range[xlsRow + 4, endXlsCol - 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 4, endXlsCol - 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow + 4, endXlsCol - 1].Text = "Maganta Font";
                        sheet1.Range[xlsRow + 4, endXlsCol - 1].WrapText = true;
                        sheet1.Range[xlsRow + 4, endXlsCol - 1].CellStyle.Font.Size = 8;
                        sheet1.Range[xlsRow + 4, endXlsCol - 1].CellStyle.Font.Color = ExcelKnownColors.Magenta;
                        sheet1.Range[xlsRow + 4, endXlsCol - 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow + 4, endXlsCol - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, endXlsCol - 5, xlsRow + 4, endXlsCol - 1].BorderAround(ExcelLineStyle.Hair);
                    }

                    // END color indication  by Mirza

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
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].Merge();
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].RowHeight = 26;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    string _sheetHeaderName = "Monthly Attendance Information(Day Status)";
                    string _sheetHeaderName1 = "Monthly Attendance Information(Intime Attendance Data)";
                    string _sheetHeaderName2 = "Monthly Attendance Information(OutTime Attendance Data)";
                    string _sheetHeaderName3 = "Monthly Attendance Information(Intime Raw Data)";
                    string _sheetHeaderName4 = "Monthly Attendance Information(OutTime Raw Data)";

                    if (DayStatus == "DAYSTATUS")
                    {
                        sheet1.Range[xlsRow, 3].Text = _sheetHeaderName;
                    }
                    else if (DayStatus == "INTIME")
                    {
                        sheet1.Range[xlsRow, 3].Text = _sheetHeaderName1;
                    }
                    else if (DayStatus == "3")
                    {
                        sheet1.Range[xlsRow, 3].Text = _sheetHeaderName2;
                    }

                    else if (DayStatus == "INRAW")
                    {
                        sheet1.Range[xlsRow, 3].Text = _sheetHeaderName3;
                    }
                    else if (DayStatus == "OUTRAW")
                    {
                        sheet1.Range[xlsRow, 3].Text = _sheetHeaderName4;
                    }
                    else
                    {
                        sheet1.Range[xlsRow, 3].Text = _sheetHeaderName;
                    }
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Report Ref No.";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].Merge();
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 9;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "From : " + FromDate + " To : " + ToDate;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].Merge();
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 9;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    sheet1.UsedRange["A9"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$1:$5";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + userName + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet1.IsDisplayZeros = false;

                    sheet1.Name = "MAR";
                    #endregion


                    //workbook.Version = ExcelVersion.Excel97to2003;
                    //var strFileName = DateTime.Now.ToString("yyMMdd") + " " + "MonthlyAttendanceInformation.xls";
                    //string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                    //workbook.SaveAs(fullPath);
                    //return Json(new { FileName = strFileName, FullPath = fullPath, Error = false }, JsonRequestBehavior.AllowGet);
                    // return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);

                }
                return workbook;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
            }
        }


        public void GetMonthlyAttnSummaryRptForDetailsDateRange(ParaMontlyAttendance objm, Dictionary<string, string> empParameters, out DataSet dsRef, bool isActive, bool isSeperated, bool isMaternity)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;


            try
            {

                strSql = @"SELECT A.* FROM
                                    (SELECT E.SystemId EmployeePK,E.GenderID,E.EmployeeCode, E.EmployeeCodeNumeric,E.EmployeeName, REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ, REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOS, E.EmpType
                                           ,ISNULL( Ld.UserName, '') LegalDG, egdsg.UserName GivenDesignation, Unit.UserName Unit, Division.UserName Division, Department.UserName Department,
                                             ISNULL(EmpC.UserName,'') EmployeeCategory,ISNULL(EmpC.WorkingDaysInAMonth,'') WorkingDaysInAMonth,Section.UserName Section, SubSection.UserName SubSection, Line.UserName Line, REPLACE(CONVERT(VARCHAR(11), ADM.FromDate, 113), ' ', '-') FromDate,
                                            REPLACE(CONVERT(VARCHAR(11), ADM.ToDate, 113), ' ', '-') ToDate, ADM.MonthNo, ADM.YearNo,
                                            ADM.TotalProcDate, ADM.TotalPresent, ADM.TotalLate, ADM.TotalAbsent, ADM.TotalLv, ADM.TotalLWP, ADM.TotalMLv, ADM.TotalOTHr,
                                            ADM.TotalNormalOTHr, ADM.TotalExtraOTHr, ADM.TotalHoliDay, isnull(ADM.TotalWeekOffHoliDay,0) + isnull(ADM.TotalWeekOff,0) TotalWeekOff, ADM.TotalWeekOffHoliDay,SLeave.ShortLeave
                                    FROM dbo.EmployeeInformation E
                                                INNER JOIN 
                                    
                                    (SELECT EmpSystemID,MonthNo = MONTH('" + objm.FDate + @"') ,YearNo = YEAR('" + objm.FDate + @"'), MIN(WorkDate) FromDate, MAX(WorkDate) ToDate, 
                                    COUNT(WorkDate) TotalProcDate, 
		                            SUM(ISNULL(CAST(TotalPresent As decimal(18, 2)), '0.00')) TotalPresent, 
                                    SUM(ISNULL(CAST(TotalLate As decimal(18, 2)), '0.00')) TotalLate, 
		                            SUM(ISNULL(CAST(TotalAbsent As decimal(18, 2)), '0.00')) TotalAbsent, 
                                    SUM(ISNULL(CAST(TotalLv As decimal(18, 2)), '0.00')) TotalLv, 
		                            SUM(ISNULL(TotalMLv,0)) TotalMLv, 
                                    SUM(ISNULL(TotalWeekOff,0)) TotalWeekOff, 
                                    SUM(ISNULL(TotalCompAssignLv,0))  TotalCompAssignLv,
		                            SUM(ISNULL(TotalHoliDay,0)) TotalHoliDay, 
                                    SUM(ISNULL(TotalWeekOffHoliDay,0)) TotalWeekOffHoliDay,
                                    SUM(ISNULL(CAST(OTHr As decimal(18, 2)), '0.00')) TotalOTHr, 
                                    0.00 TotalNormalOTHr, 
                                    0.00 TotalExtraOTHr, 
                                    SUM(ISNULL(TotalLWP, 0)) TotalLWP   
                            FROM (SELECT EmpSystemID, WorkDate, 
			                              TotalPresent = CASE WHEN Category = 'Present' and LTSystemID is null THEN 1 
                                                       WHEN Category = 'Present' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                                       WHEN Category = 'Half Day' and LTSystemID is not null  THEN (1-LeaveDuration)
                                                       WHEN Category = 'Half Day' and LTSystemID is null  THEN 0.5
                                                       ELSE 0 END,

                                                        --LWP and LWOP both r considered          
			                            TotalLate = CASE WHEN Category = 'Late' and LTSystemID is null THEN 1
                                                        WHEN Category = 'Late' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                                        WHEN Category = 'Late' and LTSystemID is not null and LeaveDuration=1 THEN 1
                                                        ELSE 0 END,

			                            TotalAbsent = CASE WHEN Category = 'Absent' and LTSystemID is null THEN 1
                                                        WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                                        WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration=1 THEN 1
                                                        WHEN Category = 'Half Day' and LTSystemID is null  THEN 0.5
                                                        ELSE 0 END,

			                            TotalLv = CASE WHEN LTSystemID is not null  and Category<>'Leave' and LeaveDuration<1 and IsLWP=0 THEN LeaveDuration
                                                          WHEN LTSystemID is not null  and Category='Leave' and IsLWP=0 THEN LeaveDuration
                                                           ELSE 0 END,

                                        TotalLWP = CASE WHEN LTSystemID is not null  and Category<>'Leave' and LeaveDuration<1 and IsLWP=1 THEN LeaveDuration
                                                        WHEN LTSystemID is not null  and Category='Leave' and IsLWP=1 THEN LeaveDuration                                                        
                                                        ELSE 0 END,

			                            TotalMLv = CASE WHEN LTSystemID is not null  and Category='Leave' and isnull(MaternityStatus,'')='MLV' THEN 1
                                                           ELSE 0 END,
                                        TotalCompAssignLv = 0,
			                            TotalWeekOff = CASE WHEN Category = 'Weekend' THEN 1
                                                        ELSE 0 END,

			                            TotalHoliDay = CASE WHEN Category = 'Holiday' THEN 1
                                                       ELSE 0 END,

                                        TotalWeekOffHoliDay = 0,
                                        OTHr
	                             FROM dbo.AttdnProcessData left join daytype p on AttdnProcessData.DayStatus=p.DayType
                                WHERE WorkDate BETWEEN '" + objm.FDate + @"'
                                    AND '" + objm.TDate + @"'  
                                    -- and PlantId = '" + objm.PlantId + @"'
                                    ) A
                            GROUP BY EmpSystemID) ADM
                        
                                     ON E.SystemID = ADM.EmpSystemID
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    

                                    LEFT OUTER JOIN (select dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									,dg.UserName GivenDesignationGroup--,srm.SalaryRuleName
									FROM mst.DesignationMaster dm
									LEFT OUTER JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
		                          
									) egdsgg on egdsgg.DesignationId=e.GivenDesignationId
							

									--AND egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
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
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    
                                    LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = ebi.BankSystemID
                                             LEFT JOIN (
												select EmpSystemID,sum(CountedShortLeave) ShortLeave,DATEPART(year,workdate) _year,DATEPART(month,workdate) _month
												 from AttdnProcessData
													--where PlantID='" + objm.PlantId + @"' 
													group by EmpSystemID,DATEPART(year,workdate),DATEPART(month,workdate)
													
												) SLeave on adm.MonthNo=SLeave._month and adm.YearNo=SLeave._year and e.SystemId=SLeave.EmpSystemID

                                    WHERE E.PlantID = '" + objm.PlantId + @"' AND ADM.MonthNo = Month( '" + objm.TDate + @"') AND ADM.YearNo = Year('" + objm.TDate + @"')
                                    --AND (ISNULL(E.EmployeeCurrentStatus,'') != 'TBS' or (ISNULL(E.EmployeeCurrentStatus,'') = 'TBS' AND EmployeeCurrentStatusEffectiveDate >='" + objm.FDate + @"'))
                                    AND (DOS IS NULL  OR DOS >= '" + objm.FDate + @"') AND E.DOJ <= '" + objm.TDate + @"'";

                if (objm.UnitId != "ALL")
                {
                    strSql = strSql + @" AND E.UnitID = '" + objm.UnitId + "'";
                }
                if (objm.DivisionId != "ALL")
                {
                    strSql = strSql + @" AND E.DivisionID = '" + objm.DivisionId + "'";
                }
                if (objm.DepartmentId != "ALL")
                {
                    strSql = strSql + @" AND E.DepartmentID = '" + objm.DepartmentId + "'";
                }
                if (objm.SectionId != "ALL")
                {
                    strSql = strSql + @" AND E.SectionID = '" + objm.SectionId + "'";
                }
                if (objm.SubsectionId != "ALL")
                {
                    strSql = strSql + @" AND E.SubSectionID = '" + objm.SubsectionId + "'";
                }
                if (objm.LineId != "ALL")
                {
                    strSql = strSql + @" AND E.LineID = '" + objm.LineId + "'";
                }

                if (objm.EmpCat != "ALL")
                {
                    strSql = strSql + @" AND E.EmployeeCategorySystemID = '" + objm.EmpCat + "'";
                }
                if (objm.DesignationGroupId != "ALL")
                {
                    strSql = strSql + @" AND E.DesignationGroupID = '" + objm.DesignationGroupId + "'";
                }
                if (objm.JoblocationName.ToUpper() != "ALL")
                {
                    strSql = strSql + @" AND E.JobLocationID = '" + objm.JoblocationName + "'";
                }
                if (objm.DesignationId != "ALL")
                {
                    strSql = strSql + @" AND E.DesignationSystemID = '" + objm.DesignationId + "'";
                }
                try
                {
                    if (empParameters.Count > 0)
                    {
                        if (empParameters.Keys.ElementAt(0) != "")
                        {
                            strSql += @" AND E.SystemId IN(" + empParameters["EmpSystemId"] + ")";
                        }
                    }
                }
                catch (Exception)
                {
                }


                strSql = strSql + @") A
                        GROUP BY A.EmployeeCode, A.GenderID,A.EmployeeCodeNumeric,A.EmployeeName, A.DOJ,A.DOS, A.EmpType,  A.GivenDesignation, A.LegalDG, A.Unit, A.Division, A.Department,
		                            A.Section, A.SubSection, A.Line, A.FromDate, A.ToDate, A.MonthNo, A.YearNo,
                                    A.TotalProcDate, A.TotalPresent, A.TotalLate, A.TotalAbsent, A.TotalLv,A.TotalLWP, A.TotalMLv, A.TotalOTHr,
                                    A.TotalNormalOTHr, A.EmployeeCategory,A.TotalExtraOTHr, A.TotalHoliDay, A.TotalWeekOff, A.TotalWeekOffHoliDay,A.EmployeePK,A.ShortLeave,A.WorkingDaysInAMonth
                        ORDER BY  A.EmployeeCodeNumeric";

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

        public Dictionary<string, string> GetExtraAbsentWeekOFF(string PlantId, string month, string year)
        {
            Dictionary<string, string> dicExtraAbsentWeekOFF = new Dictionary<string, string>();
            try
            {
                string sql = @"select Count(Id) ExtraAbsentWeekOFF,EmpSystemID from [SCS].[WeeklyAbsentismAssignment] WAA
                                            INNER JOIN EmployeeInformation E ON E.SystemId = WAA.EmpSystemId
                            where E.PlantId= '" + PlantId + @"' and Month(WorkingDate) = " + month + @" and Year(WorkingDate) = " + year + @"
                            GROUP BY EmpSystemID";
                DataTable dt = _sqlRepository.GetDataTable(sql);
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicExtraAbsentWeekOFF.Add(dt.Rows[i]["EmpSystemID"].ToString(), dt.Rows[i]["ExtraAbsentWeekOFF"].ToString());
                }
                return dicExtraAbsentWeekOFF;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public Dictionary<string, string> GetExtraAbsentHoliday(string PlantId, string month, string year)
        {
            Dictionary<string, string> dicExtraAbsentHoliday = new Dictionary<string, string>();
            try
            {
                string sql = @"SELECT COUNT(Id) ExtraAbsentHoliday,EmpSystemID  from [trn].[HolidayAbsentismAssignment] HAA
                                            INNER JOIN EmployeeInformation E ON E.SystemId = HAA.EmpSystemId
                            WHERE E.PlantId= '" + PlantId + @"' and Month(WorkDate) = " + month + @" and Year(WorkDate) = " + year + @"
                                GROUP BY EmpSystemID";
                DataTable dt = _sqlRepository.GetDataTable(sql);
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dicExtraAbsentHoliday.Add(dt.Rows[i]["EmpSystemID"].ToString(), dt.Rows[i]["ExtraAbsentHoliday"].ToString());
                }
                return dicExtraAbsentHoliday;
            }
            catch (Exception ex)
            {

                throw ex;
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

    }
}
