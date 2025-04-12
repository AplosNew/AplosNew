using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using bplib;
using Library.Service.Helpers;
using System.IO;
using Syncfusion.XlsIO;
using System.Drawing;
using Library.Crosscutting.Security;
using System.Threading;
using ConnectionManager;

namespace Library.HumanResource.NewAttendanceProcess
{
    #region Monthly Attendance Reports 
   
    public class NewAttdnMonthlySummaryService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public NewAttdnMonthlySummaryService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        Color ContrastColor(Color color)
        {
            int d = 0;
            // Counting the perceptive luminance - human eye favors green color... 
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            if (luminance > 0.5)
                d = 0; // bright colors - black font
            else
                d = 255; // dark colors - white font
            return Color.FromArgb(d, d, d);
        }


        public IWorkbook XlsMonthlyAttendanceSummaryReport(string companyId, string plantId, string Month, string Year, string userName, string DayStatus, Dictionary<string, string> empParameters, bool withColor, bool includeCurrentDate, bool withSummary, bool isActive, bool isSeperated, bool isMaternity)
        {
            #region Variable

            clsReport objRpt = null;
            DataSet dsMonthlyAttnSumm = null;
            DataView dvMonthlyAttnSumm = null;
            DataSet dsDaily = null;
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

            DateTime dtFrmDt = DateTime.Now;
            DateTime dtEndDate = DateTime.Now;
            if (!includeCurrentDate)
            {

                dtEndDate = dtEndDate.AddDays(-1);
            }
            DataSet dsSLeave = null;
            DataView dvSLeave = null;

            #endregion Variable

            try
            {


                objRpt = new clsReport(_sqlRepository);

                #region Validation

                string m = clsWebLib.GetMonthName(Month);
                dtFrmDt = Convert.ToDateTime("01-" + m + "-" + Year);
                string monthName = dtFrmDt.ToString("MMMM");
                string month = clsWebLib.GetMonthName(Month);
                DateTime dateForTheMonth = Convert.ToDateTime("01-" + m + "-" + Year);

                if (Convert.ToInt32(DateTime.Now.Month) == Convert.ToInt32(Month))
                {
                    if (Convert.ToInt32(DateTime.Now.Year) == Convert.ToInt32(Year))
                    {

                    }
                    else
                    {
                        if (!includeCurrentDate)
                        {

                            dtEndDate = dtFrmDt.AddMonths(1).AddDays(-2);
                        }
                        else
                        {
                            dtEndDate = dtFrmDt.AddMonths(1).AddDays(-1);

                        }
                    }
                }
                else
                {
                    if (!includeCurrentDate)
                    {

                        dtEndDate = dtFrmDt.AddMonths(1).AddDays(-2);
                    }
                    else
                    {
                        dtEndDate = dtFrmDt.AddMonths(1).AddDays(-1);

                    }
                }


                #endregion Validation

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);
                sheet1 = workbook.Worksheets[0];
                IWorksheet sheet2 = workbook.Worksheets[1];
                sheet1.IsGridLinesVisible = true;
                workbook.Version = ExcelVersion.Excel2016;

                #region Variable

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
                objm.AMonth = Month;
                objm.AYear = Year;
                objm.FDate = dtFrmDt.ToString("dd-MMM-yyyy");
                objm.TDate = dtEndDate.ToString("dd-MMM-yyyy");
                #endregion Variable


                #region DataSet --Detail Attendance Data with Header
                Dictionary<string, List<DataRow>> dicAttendance = new Dictionary<string, List<DataRow>>();
                Dictionary<string, List<DataRow>> dicExtraAbsent = new Dictionary<string, List<DataRow>>();


                GetMonthlyAttnSummaryRptForDetails(objm, empParameters, out dsMonthlyAttnSumm, isActive, isSeperated, isMaternity);
                dvMonthlyAttnSumm = new DataView();
                dvMonthlyAttnSumm.Table = dsMonthlyAttnSumm.Tables[0];

                // Getting the Leave Types List (LeaveCode)
                var str = @"Select Id , UserName from dbo.LeaveType";
                DataTable dtLeaveList = _sqlRepository.GetDataTable(str);
                string[] LIdList = new string[dtLeaveList.Rows.Count];

                GetLeaveData(empParameters,objm, out DataSet LeaveData);

                GetWeekOffDays(empParameters, objm, out DataSet WeekOffData);
                DataView dvWeekOff= new DataView();
                dvWeekOff.Table = WeekOffData.Tables[0];

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
                    dicAttendance = objRpt.GetMonthlyDailyAttendanceDic(_FLAG, plantId, dtFrmDt.ToString("dd-MMM-yyyy"), dtEndDate.ToString("dd-MMM-yyyy"), empParameters, isActive, isSeperated, isMaternity);
                }

                if (dicAttendance.Count == 0)
                {
                    throw new Exception("Data not found.");

                }



                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsentCW(plantId, empParameters, dtFrmDt.Month, dtEndDate.Year, out dsExtraAbsent);
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

                #endregion DataSet

                if (dvMonthlyAttnSumm.Count > 0)
                {
                  
                    xlsRow = 6;

                    #region StyleSheet


                    IStyle baseStyle = workbook.Styles.Add("BaseStyle");
                    baseStyle.Font.Color = ExcelKnownColors.Black;
                    baseStyle.Color = Color.White;
                    baseStyle.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
                    baseStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Hair;
                    baseStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Hair;
                    baseStyle.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Hair;

                 
                    DataTable dtdaytype=_sqlRepository.GetDataTable("SELECT * FROM DayType");

                    int row = 1;
                    sheet2.Name = "Legends";
                    sheet2[row, 1].Text = "Day Type";
                    sheet2[row, 2].Text = "Description"; sheet1[row, 2].ColumnWidth = 20;
                    sheet2[row, 3].Text = "Category";
                    sheet2[row, 4].Text = "Color";
                    sheet2.Range[row, 1, row, 4].CellStyle.Font.Bold = true;
                    row++;

                    Dictionary<string, IStyle> daylegends = new Dictionary<string, IStyle>();
                    for(int i=0; i<dtdaytype.Rows.Count; i++)
                    {
                        string backgroundcolor = dtdaytype.Rows[i]["ColorCode"].ToString();
                        if(string.IsNullOrEmpty(backgroundcolor))
                        {
                            backgroundcolor = "#FFFFFF";
                        }
                       
                        Color forcolor = ColorTranslator.FromHtml(backgroundcolor);
                        forcolor = ContrastColor(forcolor);
                        
                        IStyle DayTypeStyle = workbook.Styles.Add("Style"+dtdaytype.Rows[i]["DayType"].ToString());
                        DayTypeStyle.Font.RGBColor = forcolor;

                        DayTypeStyle.Color = ColorTranslator.FromHtml(backgroundcolor);

                        daylegends.Add(dtdaytype.Rows[i]["DayType"].ToString(), DayTypeStyle);

                        sheet2[row, 1].Text = dtdaytype.Rows[i]["DayType"].ToString();
                        sheet2[row, 2].Text = dtdaytype.Rows[i]["Description"].ToString();
                        sheet2[row, 3].Text = dtdaytype.Rows[i]["Category"].ToString();
                        sheet2[row, 4].CellStyle = DayTypeStyle;
                        row++;

                    }
                
                    #endregion.


                    #region Variables

                    int strCount = 0;

                    int LeaveStartCol = 0; // LeaveCode
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
                    int iWorkingCount = 0;
                    int cPayDays = 0;
                    int iTtlHD = 0;
                    int iTtlWO = 0;
                    int iTtlPst = 0;
                    int ionlyP = 0;
                    int iTtlAbs = 0;
                    int iTtlLte = 0;                   
                    int iExtraAbs = 0;
                    int iWeekOffDays = 0;
                    int iLateIn = 0;
                    int iEarlyOut = 0;
                    int iGender = 0;
                    int iEmpCategory = 0;
                    int iPlant = 0;
                    #endregion

                    #region ------------------Column Header------------------

                    #region ------------------Details Header-----------------


                    #region Employee Values

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
                    iPlant = xlsCol;
                    sheet1.Range[xlsRow, iPlant].Text = "Plant";
                    sheet1.Range[xlsRow, iPlant].ColumnWidth = 22;
                    sheet1.Range[xlsRow, iPlant].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iPlant].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iPlant, xlsRow + 1, iPlant].Merge();

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
                    int iContractor = xlsCol;
                    sheet1.Range[xlsRow, iContractor].Text = "Contractor";
                    sheet1.Range[xlsRow, iContractor].ColumnWidth = 15;
                    sheet1.Range[xlsRow, iContractor].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iContractor].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iContractor, xlsRow + 1, iContractor].Merge();


                    xlsCol += 1;
                    iDesig = xlsCol;
                    sheet1.Range[xlsRow, iDesig].Text = "Designation";
                    sheet1.Range[xlsRow, iDesig].ColumnWidth = 18;
                    sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignCenter;

                    sheet1.Range[xlsRow, iDesig, xlsRow + 1, iDesig].Merge();


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


                    #endregion

                    #region Summary Region

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
                        iWorkingCount = xlsCol;
                        sheet1.Range[xlsRow - 1, iWorkingCount].Text = "Working Days";
                        sheet1.Range[xlsRow - 1, iWorkingCount].ColumnWidth = 9;
                        sheet1.Range[xlsRow - 1, iWorkingCount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iWorkingCount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iWorkingCount, xlsRow, iWorkingCount].Merge();


                        xlsCol += 1;
                        cPayDays = xlsCol;
                        sheet1.Range[xlsRow - 1, cPayDays].Text = "Pay Days";
                        sheet1.Range[xlsRow - 1, cPayDays].ColumnWidth = 6;
                        sheet1.Range[xlsRow - 1, cPayDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, cPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, cPayDays, xlsRow, cPayDays].Merge();

                        xlsCol += 1;
                        ionlyP = xlsCol;
                        sheet1.Range[xlsRow - 1, ionlyP].Text = "Total Present";
                        sheet1.Range[xlsRow - 1, ionlyP].ColumnWidth = 10;
                        sheet1.Range[xlsRow - 1, ionlyP].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, ionlyP].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, ionlyP, xlsRow, ionlyP].Merge();

                        xlsCol += 1;
                        iTtlLte = xlsCol;
                        sheet1.Range[xlsRow - 1, iTtlLte].Text = "Total Late";
                        sheet1.Range[xlsRow - 1, iTtlLte].ColumnWidth = 6;
                        sheet1.Range[xlsRow - 1, iTtlLte].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlLte].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlLte, xlsRow, iTtlLte].Merge();

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



                        LeaveStartCol = xlsCol + 1;

                        // The Dynamic Columns for the Leave (LeaveCode)
                        for(int i=0;i<dtLeaveList.Rows.Count;i++)
                        {
                            xlsCol += 1;
                            
                            sheet1.Range[xlsRow - 1, xlsCol].Text = dtLeaveList.Rows[i]["UserName"].ToString();
                            sheet1.Range[xlsRow - 1, xlsCol].ColumnWidth = 7.20;
                            sheet1.Range[xlsRow - 1, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow - 1, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow - 1, xlsCol, xlsRow, xlsCol].Merge();

                            LIdList[i] = dtLeaveList.Rows[i]["Id"].ToString();
                        }

                        xlsCol += 1;
                        iWeekOffDays= xlsCol;
                        sheet1.Range[xlsRow - 1, iWeekOffDays].Text = "WeekOff Days";
                        sheet1.Range[xlsRow - 1, iWeekOffDays].ColumnWidth = 7.20;
                        sheet1.Range[xlsRow - 1, iWeekOffDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iWeekOffDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iWeekOffDays, xlsRow, iWeekOffDays].Merge();



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

                    #endregion

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

                    string attdnStatus = "";
                    string _day_status = "";
                    #region Attendance Data 
                    for (int i = 0; i <= dvMonthlyAttnSumm.Count - 1; i++)
                    {

                        xlsCol = 1;

                        #region ----------------------Data-----------------------

                        #region Employee Data
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
                        sheet1.Range[xlsRow, iPlant].Text = dvMonthlyAttnSumm[i]["PlantName"].ToString().Trim();
                        sheet1.Range[xlsRow, iContractor].Text = dvMonthlyAttnSumm[i]["Contractor"].ToString().Trim();

                        sheet1.Range[xlsRow, iDesig].Text = dvMonthlyAttnSumm[i]["LegalDG"].ToString().Trim();
                        string _m = clsWebLib.GetMonthName(Month);
                        dtFrmDt = Convert.ToDateTime("01-" + _m + "-" + Year);
                        xlsCol = iDesig;
                        string ecode = dvMonthlyAttnSumm[i]["EmployeeCode"].ToString().Trim();
                        string _SystemId = dvMonthlyAttnSumm[i]["EmployeePK"].ToString().Trim();

                        #endregion

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
                                        //var _day_
                                        if (_FLAG.ToUpper() == "DAYSTATUS")
                                        {                                                
                                            attdnStatus = item["DayStatus"].ToString();                                          
                                        }
                                        else if (_FLAG.ToUpper() == "ALLSTATUS")
                                        {
                                            if (item["DayCategory"].ToString().ToUpper() == "Leave".ToUpper())
                                            {
                                                attdnStatus = item["DayStatus"].ToString();

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


                                        if (item["MANUALStatus"].ToString().ToUpper() == "MANUAL")
                                        {
                                            IsManual = true;

                                        }
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    try
                                    {
                                        if (withColor == true)
                                        {

                                            sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = daylegends[item["DayStatus"].ToString()];
                                        }
                                    }
                                    catch (Exception)
                                    { 
                                    
                                    }

                                    
                                }
                            }
                            
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }
                        #endregion
                                               
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

                       
                            sheet1.Range[xlsRow, iWorkingCount].Number = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalActualDays"].ToString().Trim()));
                            sheet1.Range[xlsRow, iWorkingCount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iWorkingCount].VerticalAlignment = ExcelVAlign.VAlignCenter;


                            double _pay_days = 0.00;
                            double ExtraAbsentWeekOFF = 0.00;
                            double ExtraAbsentHoliday = 0.00;
                            if (dicExtraAbsentWeekOFF.ContainsKey(_SystemId))
                            {
                                ExtraAbsentWeekOFF = clsStaticInfo.dbl(dicExtraAbsentWeekOFF[_SystemId]);
                            }
                            if (dicExtraAbsentHoliday.ContainsKey(_SystemId))
                            {

                                ExtraAbsentHoliday = clsStaticInfo.dbl(dicExtraAbsentHoliday[_SystemId]);
                            }

                            _pay_days = clsStaticInfo.dbl(dvMonthlyAttnSumm[i]["TotalPayDay"].ToString());

                            sheet1.Range[xlsRow, cPayDays].Text = _pay_days.ToString();
                            sheet1.Range[xlsRow, cPayDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, cPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlHD].Number = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalHoliDay"].ToString().Trim()));
                            sheet1.Range[xlsRow, iTtlHD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlHD].VerticalAlignment = ExcelVAlign.VAlignCenter;


                            sheet1.Range[xlsRow, iTtlWO].Number = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalWeekOff"].ToString().Trim()));
                            sheet1.Range[xlsRow, iTtlWO].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlWO].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            double _pre = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalPresent"].ToString().Trim()));
                            double _Late = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLate"].ToString().Trim()));

                            double TPresentAndLate = _pre + _Late;
                            sheet1.Range[xlsRow, iTtlPst].Number = TPresentAndLate;
                            sheet1.Range[xlsRow, iTtlPst].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlPst].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, ionlyP].Number = _pre;
                            sheet1.Range[xlsRow, ionlyP].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, ionlyP].VerticalAlignment = ExcelVAlign.VAlignCenter;


                            sheet1.Range[xlsRow, iTtlAbs].Number = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalAbsent"].ToString().Trim()));
                            sheet1.Range[xlsRow, iTtlAbs].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlAbs].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlLte].Number = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLate"].ToString().Trim()));
                            sheet1.Range[xlsRow, iTtlLte].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlLte].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iExtraAbs].Number = Convert.ToDouble(_ExtraAbsent);
                            sheet1.Range[xlsRow, iExtraAbs].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iExtraAbs].VerticalAlignment = ExcelVAlign.VAlignCenter;


                            dvWeekOff.RowFilter= "EmpSystemID='" + _SystemId + "' ";
                            if (dvWeekOff.Count > 0)
                            {
                                sheet1.Range[xlsRow, iWeekOffDays].Text = dvWeekOff[0]["WeekOffDays"].ToString();
                                sheet1.Range[xlsRow, iWeekOffDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iWeekOffDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }

                            sheet1.Range[xlsRow, iLateIn].Number = lateIn;
                            sheet1.Range[xlsRow, iLateIn].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iLateIn].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iEarlyOut].Number = earlyOut;
                            sheet1.Range[xlsRow, iEarlyOut].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iEarlyOut].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            // Leave Code for dynamic Amounts For All Dynamic Leaves (LeaveCode)
                            int tempLv = LeaveStartCol;
                            for(int j = 0; j< dtLeaveList.Rows.Count;j++)
                            {
                                LeaveData.Tables[0].DefaultView.RowFilter = @"EmpSystemID ='" + _SystemId + "' and LTSystemID='" + LIdList[j].ToString() + "'";

                                if(LeaveData.Tables[0].DefaultView.Count>0)
                                {
                                    sheet1.Range[xlsRow, tempLv].Number = Math.Abs(Convert.ToDouble(clsWebLib.GetNumData(LeaveData.Tables[0].DefaultView[0]["LeaveValue"].ToString())));
                                    sheet1.Range[xlsRow, tempLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, tempLv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }
                                else
                                {
                                    sheet1.Range[xlsRow, tempLv].Number = 0.0;
                                    sheet1.Range[xlsRow, tempLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, tempLv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }

                                tempLv++;
                            }
                        }
                      
                        xlsRow += 1;

                        #endregion ----------------------Data-----------------------


                    }
                    #endregion

                    #region Line Setup
                    try
                    {
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
                        sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
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

                    // start color indication  
                    //if (withColor == true)
                    //{
                    //    sheet1.Range[xlsRow, endXlsCol - 4, xlsRow, endXlsCol - 1].Merge();
                    //    sheet1.Range[xlsRow, endXlsCol - 4].Text = "Color Indication";
                    //    sheet1.Range[xlsRow, endXlsCol - 4].CellStyle.Font.Bold = true;
                    //    sheet1.Range[xlsRow, endXlsCol - 4].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                    //    sheet1.Range[xlsRow, endXlsCol - 4].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //    sheet1.Range[xlsRow, endXlsCol - 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //    sheet1.Range[xlsRow + 1, endXlsCol - 4].Text = "Present";
                    //    sheet1.Range[xlsRow + 1, endXlsCol - 3].CellStyle.Interior.Color = System.Drawing.Color.Green;
                    //    sheet1.Range[xlsRow + 1, endXlsCol - 4].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //    sheet1.Range[xlsRow + 1, endXlsCol - 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //    sheet1.Range[xlsRow + 1, endXlsCol - 2].Text = "Absent";
                    //    sheet1.Range[xlsRow + 1, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Red;
                    //    sheet1.Range[xlsRow + 1, endXlsCol - 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //    sheet1.Range[xlsRow + 1, endXlsCol - 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //    sheet1.Range[xlsRow + 2, endXlsCol - 4].Text = "Leave";
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 3].CellStyle.Interior.Color = System.Drawing.Color.Yellow;
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 4].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //    sheet1.Range[xlsRow + 2, endXlsCol - 2].Text = "Half Day Leave";
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 2].WrapText = true;
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 2].CellStyle.Font.Size = 8;
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 1].CellStyle.Font.Color = ExcelKnownColors.Yellow;
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //    sheet1.Range[xlsRow + 2, endXlsCol - 1].Text = "Yellow Font";
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 1].WrapText = true;
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 1].CellStyle.Font.Size = 8;
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //    sheet1.Range[xlsRow + 2, endXlsCol - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    //    sheet1.Range[xlsRow + 3, endXlsCol - 2].Text = "Late";
                    //    sheet1.Range[xlsRow + 3, endXlsCol - 1].CellStyle.Interior.Color = System.Drawing.Color.Blue;

                    //    sheet1.Range[xlsRow + 3, endXlsCol - 4].Text = "Out T Miss:";
                    //    sheet1.Range[xlsRow + 3, endXlsCol - 4].WrapText = true;
                    //    sheet1.Range[xlsRow + 3, endXlsCol - 4].CellStyle.Font.Size = 8;
                    //    sheet1.Range[xlsRow + 3, endXlsCol - 3].CellStyle.Interior.Color = System.Drawing.Color.Violet;

                    //    sheet1.Range[xlsRow + 4, endXlsCol - 4].Text = "Manual Attdn:";
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 4].WrapText = true;
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 4].CellStyle.Font.Size = 8;
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 3].CellStyle.Interior.Color = System.Drawing.Color.Orange;

                    //    sheet1.Range[xlsRow + 4, endXlsCol - 2].Text = "Short Leave";
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 2].WrapText = true;
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 2].CellStyle.Font.Size = 8;
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 2].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 2].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //    sheet1.Range[xlsRow + 4, endXlsCol - 1].Text = "Maganta Font";
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 1].WrapText = true;
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 1].CellStyle.Font.Size = 8;
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 1].CellStyle.Font.Color = ExcelKnownColors.Magenta;
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //    sheet1.Range[xlsRow + 4, endXlsCol - 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //    sheet1.Range[xlsRow, endXlsCol - 5, xlsRow + 4, endXlsCol - 1].BorderAround(ExcelLineStyle.Hair);
                    //}

                    // END color indication

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
                    sheet1.Range[xlsRow, xlsCol].Text = "Year : " + Year + " and Month : " + dateForTheMonth.ToString("MMMM");
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
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
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
        public Dictionary<string, string> GetExtraAbsentWeekOFF(string PlantId, string month, string year)
        {
            Dictionary<string, string> dicExtraAbsentWeekOFF = new Dictionary<string, string>();
            try
            {
                string sql = @"select Count(Id) ExtraAbsentWeekOFF,EmpSystemID from [SCS].[WeeklyAbsentismAssignment] WAA
                                            INNER JOIN EmployeeInformation E ON E.SystemId = WAA.EmpSystemId
                            where --E.PlantId= '" + PlantId + @"' and 
                                Month(WorkingDate) = " + month + @" and Year(WorkingDate) = " + year + @"
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
                            WHERE --E.PlantId= '" + PlantId + @"' and 
                                Month(WorkDate) = " + month + @" and Year(WorkDate) = " + year + @"
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

        public void GetMonthlyAttnSummaryRptForDetails(ParaMontlyAttendance objm, Dictionary<string, string> empParameters, out DataSet dsRef, bool isActive, bool isSeperated, bool isMaternity)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                string wcEmpStatus = " AND (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " AND (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR (select CASE WHEN MONTH(e.DOS) = MONTH('01-Sep-2021')  AND YEAR(e.DOS) = YEAR('01-Sep-2021') then 'Separated' else 'Regular' end)= 'Regular'";

                        //wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Regular'";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR (select CASE WHEN MONTH(e.DOS) = MONTH('01-Sep-2021')  AND YEAR(e.DOS) = YEAR('01-Sep-2021') then 'Separated' else 'Regular' end)= 'Separated'";
                       // wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Separated'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR (select CASE WHEN MONTH(e.DOS) = MONTH('01-Sep-2021')  AND YEAR(e.DOS) = YEAR('01-Sep-2021') then 'Separated' else 'Regular' end)= 'MLV_PRE'";
                    }
                }
                wcEmpStatus += ")";

                strSql = @"select dd.*,e.EmployeeCode,e.GenderID,E.EmployeeCode,                
                ISNULL(E.EmployeeCodeNumeric,0) EmployeeCodeNumeric
									,ISNULL(E.EmployeeCodePreFix,0) EmployeeCodePreFix,E.EmployeeName,
									REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ, 
									REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOS, E.EmpType,
									  ISNULL( Ld.UserName, '') LegalDG, Unit.UserName Unit,
										   Division.UserName Division, Department.UserName Department,
                                             ISNULL(EmpC.UserName,'') EmployeeCategory,cdata.UserName as Contractor,											
											 Section.UserName Section, SubSection.UserName SubSection,Line.UserName Line,
									Month(dd.FromDate)MonthNo,YEAR(dd.FromDate)YearNo,Plant.UserName PlantName from 
            (select p.EmpSystemID as EmployeePK,REPLACE(CONVERT(VARCHAR(11), MIN(p.WorkDate), 113), ' ', '-') FromDate,   
               REPLACE(CONVERT(VARCHAR(11), MAX(p.WorkDate), 113), ' ', '-') ToDate,
                COUNT(p.WorkDate) TotalProcDate,
                isnull(SUM(P.PresentValue),'0')TotalPresent,isnull(SUM(p.LateValue),'0')TotalLate,isnull(SUM(p.AbsentValue),'0')TotalAbsent
                ,isnull(SUM(p.LvValue),'0')TotalLv,isnull(SUM(p.CompAssignLvValue),'0')TotalCompAssignLv,
                isnull(SUM(p.WeekOffValue),'0')TotalWeekOff,isnull(SUM(p.HoliDayValue),'0')TotalHoliDay,isnull(SUM(p.WeekOffHoliDayValue),'0')TotalWeekOffHoliDay
               ,isnull(SUM(p.PayDayValue),'0')TotalPayDay,isnull(SUM(p.ActualWorkingDayValue),'0')TotalActualDays
			            from AttdnProcessData p
                        where isnull(p.DayStatus,'')!='' and WorkDate between '" + objm.FDate+@"'
						 and '"+objm.TDate+ @"' group BY EmpSystemID) as dd
						 join EmployeeInformation  e on e.SystemId=dd.EmployeePK	
                  LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
                                    left join hkp.party cdata on cdata.id=e.VendorId
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
                             where
							  (E.DOS IS NULL  OR E.DOS >= '" + objm.FDate+@"') 
									AND E.DOJ <= '"+objm.TDate+"'";

               
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

                var sql = strSql + wcEmpStatus;

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, false, "", "1");
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

        public void GetLeaveData(Dictionary<string, string> empParameters, ParaMontlyAttendance objm, out DataSet ds)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string empStr = "";
                if (empParameters.Count > 0)
                {
                    if (empParameters.Keys.ElementAt(0) != "")
                    {
                        empStr = @" AND E.SystemId IN(" + empParameters["EmpSystemId"] + ")";
                    }
                }

                var sql = @"select p.EmpSystemID,p.LTSystemID,p.LeaveStatus,Sum(p.LvValue) as LeaveValue
                from LeaveType l join AttdnProcessData p  
                on l.Id=p.LTSystemID left join EmployeeInformation e on e.SystemId=p.EmpSystemID
                where isnull(DayStatus,'')!='' and isnull(LeaveStatus,'')!='' 
                "+empStr+@" 
                --and (E.DOS IS NULL  OR E.DOS >= '"+objm.FDate+@"') 
				--AND E.DOJ <= '"+objm.TDate+@"' and 
                AND WorkDate between '"+objm.FDate+@"' and '"+objm.TDate+@"'
                group by p.EmpSystemID,p.LeaveStatus,p.LTSystemID";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void GetWeekOffDays(Dictionary<string, string> empParameters, ParaMontlyAttendance objm, out DataSet ds)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string empStr = "";
                if (empParameters.Count > 0)
                {
                    if (empParameters.Keys.ElementAt(0) != "")
                    {
                        empStr = @" AND EmpSystemID IN(" + empParameters["EmpSystemId"] + ")";
                    }
                }

                string Month= Convert.ToDateTime(objm.FDate).Month.ToString();
                string Year= Convert.ToDateTime(objm.FDate).Year.ToString();
                objCon = new ConnectionManager.DAL.ConManager("1");
                var sql = @"SELECT EmpSystemID,
                WeekOffDays = STUFF((
                SELECT '-' + format(WorkDate,'dd') as FF
                FROM AttdnProcessData ap
                WHERE ap.EmpSystemID = p.EmpSystemID and ap.WeekOffValue = '1' 
                and OtMonth='" + Month + "' and OtYear='" + Year + "'"+empStr+ @"
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '')
                FROM AttdnProcessData p
                where p.WeekOffValue='1' and OtMonth='" + Month+"' and OtYear='"+Year+"' "+empStr+@"
                group by EmpSystemID";

               // objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sql, out ds);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }

    public class NewAttdnMonthlyDateRangeSummaryService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public NewAttdnMonthlyDateRangeSummaryService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public void GetLeaveData(Dictionary<string, string> empParameters, out DataSet ds , string FD , string TD)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string empStr = "";
                if (empParameters.Count > 0)
                {
                    if (empParameters.Keys.ElementAt(0) != "")
                    {
                        empStr = @" AND E.SystemId IN(" + empParameters["EmpSystemId"] + ")";
                    }
                }

                var sql = @"select p.EmpSystemID,p.LTSystemID,p.LeaveStatus,Sum(p.LvValue) as LeaveValue
                from LeaveType l join AttdnProcessData p  
                on l.Id=p.LTSystemID left join EmployeeInformation e on e.SystemId=p.EmpSystemID
                where isnull(DayStatus,'')!='' and isnull(LeaveStatus,'')!='' 
                "+ empStr +@"
                and (E.DOS IS NULL  OR E.DOS >= '" + FD + @"') 
				AND E.DOJ <= '" + TD + @"' and 
                WorkDate between '" + FD + @"' and '" + TD + @"'
                group by p.EmpSystemID,p.LeaveStatus,p.LTSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        public IWorkbook XlsMonthlyAttendanceSummaryReportDateRange(string companyId, string plantId, string FromDate, string ToDate, string userName, string DayStatus, Dictionary<string, string> empParameters, bool withColor, bool includeCurrentDate, bool withSummary, bool isActive, bool isSeperated, bool isMaternity)
        {
            #region Variable

            clsReport objRpt = null;
            clsSalaryProc objRptSal = null;

            DataSet dsMonthlyAttnSumm = null;
            DataView dvMonthlyAttnSumm = null;
            DataSet dsDaily = null;
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
           
            DataSet dsSLeave = null;
            DataView dvSLeave = null;

            #endregion Variable

            excelEngine = new ExcelEngine();
            application = excelEngine.Excel;

            workbook = application.Workbooks.Create(2);
            sheet1 = workbook.Worksheets[0];
            IWorksheet sheet2 = workbook.Worksheets[1];
            sheet1.IsGridLinesVisible = true;
            workbook.Version = ExcelVersion.Excel2016;

            try
            {


                objRpt = new clsReport(_sqlRepository);
                objRptSal = new clsSalaryProc();


                #region Variable
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
               // DataSet dsAttdnSumm = null;
                Dictionary<string, List<DataRow>> dicAttendance = new Dictionary<string, List<DataRow>>();

                GetMonthlyAttnSummaryRptForDetailsDateRange(objm, empParameters, out dsMonthlyAttnSumm, isActive, isSeperated, isMaternity);
                dvMonthlyAttnSumm = new DataView();
                dvMonthlyAttnSumm.Table = dsMonthlyAttnSumm.Tables[0];

                // Getting the Leave Types List (LeaveCode)
                var str = @"Select Id , UserName from dbo.LeaveType";
                DataTable dtLeaveList = _sqlRepository.GetDataTable(str);
                string[] LIdList = new string[dtLeaveList.Rows.Count];

                GetLeaveData(empParameters, out DataSet LeaveData , FromDate , ToDate);

                GetWeekOffDays(empParameters, objm, out DataSet WeekOffData);
                DataView dvWeekOff = new DataView();
                dvWeekOff.Table = WeekOffData.Tables[0];

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
                    dicAttendance = objRpt.GetMonthlyDailyAttendanceDic(_FLAG, plantId, dtFrmDt.ToString("dd-MMM-yyyy"), dtEndDate.ToString("dd-MMM-yyyy"), empParameters, isActive, isSeperated, isMaternity);
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
                   
                    xlsRow = 6;

                    #region StyleSheet

                    IStyle baseStyle = workbook.Styles.Add("BaseStyle");
                    baseStyle.Font.Color = ExcelKnownColors.Black;
                    baseStyle.Color = Color.White;
                    baseStyle.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Hair;
                    baseStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Hair;
                    baseStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Hair;
                    baseStyle.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Hair;

                    DataTable dtdaytype = _sqlRepository.GetDataTable("SELECT * FROM DayType");

                    int row = 1;
                    sheet2.Name = "Legends";
                    sheet2[row, 1].Text = "Day Type";
                    sheet2[row, 2].Text = "Description"; sheet1[row, 2].ColumnWidth = 20;
                    sheet2[row, 3].Text = "Category";
                    sheet2[row, 4].Text = "Color";
                    sheet2.Range[row, 1, row, 4].CellStyle.Font.Bold = true;
                    row++;

                    Dictionary<string, IStyle> daylegends = new Dictionary<string, IStyle>();
                    for (int i = 0; i < dtdaytype.Rows.Count; i++)
                    {
                        string backgroundcolor = dtdaytype.Rows[i]["ColorCode"].ToString();
                        if (string.IsNullOrEmpty(backgroundcolor))
                        {
                            backgroundcolor = "#FFFFFF";
                        }

                        Color forcolor = ColorTranslator.FromHtml(backgroundcolor);
                        forcolor = ContrastColor(forcolor);

                        IStyle DayTypeStyle = workbook.Styles.Add("Style" + dtdaytype.Rows[i]["DayType"].ToString());
                        DayTypeStyle.Font.RGBColor = forcolor;

                        DayTypeStyle.Color = ColorTranslator.FromHtml(backgroundcolor);

                        daylegends.Add(dtdaytype.Rows[i]["DayType"].ToString(), DayTypeStyle);

                        sheet2[row, 1].Text = dtdaytype.Rows[i]["DayType"].ToString();
                        sheet2[row, 2].Text = dtdaytype.Rows[i]["Description"].ToString();
                        sheet2[row, 3].Text = dtdaytype.Rows[i]["Category"].ToString();
                        sheet2[row, 4].CellStyle = DayTypeStyle;
                        row++;

                    }



                    #endregion.


                    #region Variables

                    int strCount = 0;

                    int LeaveStartCol = 0; // LeaveCode

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
                    int iWorkingCount = 0;
                    int cPayDays = 0;
                    int iTtlHD = 0;
                    int iTtlWO = 0;
                    int iTtlPst = 0;
                    int iTtlAbs = 0;
                    int iTtlLte = 0;                  
                    int ionlyP = 0;
                    int iExtraAbs = 0;
                    int iLateIn = 0;
                    int iEarlyOut = 0;
                    int iGender = 0;
                    int iEmpCategory = 0;
                    int iWeekOffDays = 0;
                    #endregion

                    #region ------------------Column Header------------------

                    #region ------------------Details Header-----------------

                    xlsRow += 1;

                    #region EmployeeInfo
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
                    int iContractor = xlsCol;
                    sheet1.Range[xlsRow, iContractor].Text = "Contractor";
                    sheet1.Range[xlsRow, iContractor].ColumnWidth = 15;
                    sheet1.Range[xlsRow, iContractor].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iContractor].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iContractor, xlsRow + 1, iContractor].Merge();


                    xlsCol += 1;
                    iDesig = xlsCol;
                    sheet1.Range[xlsRow, iDesig].Text = "Designation";
                    sheet1.Range[xlsRow, iDesig].ColumnWidth = 18;
                    sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    //sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iDesig, xlsRow + 1, iDesig].Merge();

                    #endregion

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

                    #region Summary Header

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
                        iWorkingCount = xlsCol;
                        sheet1.Range[xlsRow - 1, iWorkingCount].Text = "Working Days";
                        sheet1.Range[xlsRow - 1, iWorkingCount].ColumnWidth = 9;
                        sheet1.Range[xlsRow - 1, iWorkingCount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iWorkingCount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iWorkingCount, xlsRow, iWorkingCount].Merge();


                        xlsCol += 1;
                        cPayDays = xlsCol;
                        sheet1.Range[xlsRow - 1, cPayDays].Text = "Pay Days";
                        sheet1.Range[xlsRow - 1, cPayDays].ColumnWidth = 6;
                        sheet1.Range[xlsRow - 1, cPayDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, cPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, cPayDays, xlsRow, cPayDays].Merge();

                        xlsCol += 1;
                        ionlyP = xlsCol;
                        sheet1.Range[xlsRow - 1, ionlyP].Text = "Total Present";
                        sheet1.Range[xlsRow - 1, ionlyP].ColumnWidth = 10;
                        sheet1.Range[xlsRow - 1, ionlyP].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, ionlyP].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, ionlyP, xlsRow, ionlyP].Merge();


                        xlsCol += 1;
                        iTtlLte = xlsCol;
                        sheet1.Range[xlsRow - 1, iTtlLte].Text = "Total Late";
                        sheet1.Range[xlsRow - 1, iTtlLte].ColumnWidth = 6;
                        sheet1.Range[xlsRow - 1, iTtlLte].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlLte].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTtlLte, xlsRow, iTtlLte].Merge();


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



                      
                        LeaveStartCol = xlsCol + 1;

                        // The Dynamic Columns for the Leave (LeaveCode)
                        for (int i = 0; i < dtLeaveList.Rows.Count; i++)
                        {
                            xlsCol += 1;

                            sheet1.Range[xlsRow - 1, xlsCol].Text = dtLeaveList.Rows[i]["UserName"].ToString();
                            sheet1.Range[xlsRow - 1, xlsCol].ColumnWidth = 7.20;
                            sheet1.Range[xlsRow - 1, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow - 1, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            sheet1.Range[xlsRow - 1, xlsCol, xlsRow, xlsCol].Merge();

                            LIdList[i] = dtLeaveList.Rows[i]["Id"].ToString();
                        }

                        xlsCol += 1;
                        iWeekOffDays = xlsCol;
                        sheet1.Range[xlsRow - 1, iWeekOffDays].Text = "WeekOff Days";
                        sheet1.Range[xlsRow - 1, iWeekOffDays].ColumnWidth = 7.20;
                        sheet1.Range[xlsRow - 1, iWeekOffDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iWeekOffDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iWeekOffDays, xlsRow, iWeekOffDays].Merge();


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

                    #endregion

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
                        sheet1.Range[xlsRow, iContractor].Text = dvMonthlyAttnSumm[i]["Contractor"].ToString().Trim();

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
                                            
                                            attdnStatus = item["DayStatus"].ToString();
                                            
                                        }
                                        else if (_FLAG.ToUpper() == "ALLSTATUS")
                                        {
                                            if (item["DayCategory"].ToString().ToUpper() == "Leave".ToUpper())
                                            {
                                                attdnStatus = item["DayStatus"].ToString();

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
                                    catch (Exception )
                                    {

                                    }

                                    if (withColor == true)
                                    {

                                        try
                                        {
                                            sheet1[xlsRow, StartDayCol + (int)clsStaticInfo.dbl(item["D"].ToString())].CellStyle = daylegends[item["DayStatus"].ToString()];
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

                                sheet1.Range[xlsRow, iWorkingCount].Number = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalActualDays"].ToString().Trim()));
                                sheet1.Range[xlsRow, iWorkingCount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iWorkingCount].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                double _pay_days = 0.00;

                                _pay_days = clsStaticInfo.dbl(dvMonthlyAttnSumm[i]["TotalPayDay"].ToString());

                                sheet1.Range[xlsRow, cPayDays].Text = _pay_days.ToString();
                                sheet1.Range[xlsRow, cPayDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, cPayDays].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iTtlHD].Number = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalHoliDay"].ToString().Trim()));
                                sheet1.Range[xlsRow, iTtlHD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlHD].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                sheet1.Range[xlsRow, iTtlWO].Number = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalWeekOff"].ToString().Trim()));
                                sheet1.Range[xlsRow, iTtlWO].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlWO].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                double _pre = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalPresent"].ToString().Trim()));
                                double _Late = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLate"].ToString().Trim()));


                                sheet1.Range[xlsRow, ionlyP].Number = _pre;
                                sheet1.Range[xlsRow, ionlyP].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, ionlyP].VerticalAlignment = ExcelVAlign.VAlignCenter;


                                double TPresentAndLate = _pre + _Late;
                                sheet1.Range[xlsRow, iTtlPst].Number = TPresentAndLate;
                                sheet1.Range[xlsRow, iTtlPst].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlPst].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iTtlAbs].Number = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalAbsent"].ToString().Trim()));
                                sheet1.Range[xlsRow, iTtlAbs].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlAbs].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iTtlLte].Number = Convert.ToDouble(clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLate"].ToString().Trim()));
                                sheet1.Range[xlsRow, iTtlLte].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlLte].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iExtraAbs].Number = Convert.ToDouble(_ExtraAbsent);
                                sheet1.Range[xlsRow, iExtraAbs].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iExtraAbs].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                dvWeekOff.RowFilter = "EmpSystemID='" + _SystemId + "' ";
                                if (dvWeekOff.Count > 0)
                                {
                                    sheet1.Range[xlsRow, iWeekOffDays].Text = dvWeekOff[0]["WeekOffDays"].ToString();
                                    sheet1.Range[xlsRow, iWeekOffDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet1.Range[xlsRow, iWeekOffDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }


                                sheet1.Range[xlsRow, iLateIn].Number = lateIn;
                                sheet1.Range[xlsRow, iLateIn].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iLateIn].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, iEarlyOut].Number = earlyOut;
                                sheet1.Range[xlsRow, iEarlyOut].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iEarlyOut].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                // Leave Code for dynamic Amounts For All Dynamic Leaves (LeaveCode)
                                int tempLv = LeaveStartCol;
                                for (int j = 0; j < dtLeaveList.Rows.Count; j++)
                                {
                                    LeaveData.Tables[0].DefaultView.RowFilter = @"EmpSystemID ='" + _SystemId + "' and LTSystemID='" + LIdList[j].ToString() + "'";

                                    if (LeaveData.Tables[0].DefaultView.Count > 0)
                                    {
                                        sheet1.Range[xlsRow, tempLv].Number = Math.Abs(Convert.ToDouble(clsWebLib.GetNumData(LeaveData.Tables[0].DefaultView[0]["LeaveValue"].ToString())));
                                        sheet1.Range[xlsRow, tempLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, tempLv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                    else
                                    {
                                        sheet1.Range[xlsRow, tempLv].Number = 0.0;
                                        sheet1.Range[xlsRow, tempLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                        sheet1.Range[xlsRow, tempLv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }

                                    //sheet1.Range[xlsRow, tempLv].Text = LIdList[j].ToString();
                                    //sheet1.Range[xlsRow, tempLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    //sheet1.Range[xlsRow, tempLv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    tempLv++;
                                }
                            }
                          

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
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
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

                strSql = @"select dd.*,e.EmployeeCode,e.GenderID,E.EmployeeCode,                
                ISNULL(E.EmployeeCodeNumeric,0) EmployeeCodeNumeric
									,ISNULL(E.EmployeeCodePreFix,0) EmployeeCodePreFix,E.EmployeeName,
									REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ, 
									REPLACE(CONVERT(VARCHAR(11), E.DOS, 113), ' ', '-') DOS, E.EmpType,
									  ISNULL( Ld.UserName, '') LegalDG, Unit.UserName Unit,
										   Division.UserName Division, Department.UserName Department,
                                             ISNULL(EmpC.UserName,'') EmployeeCategory,cdata.UserName as Contractor,									
											 Section.UserName Section, SubSection.UserName SubSection,Line.UserName Line,
									Month(dd.FromDate)MonthNo,YEAR(dd.FromDate)YearNo,Plant.UserName PlantName from 
            (select p.EmpSystemID as EmployeePK,REPLACE(CONVERT(VARCHAR(11), MIN(p.WorkDate), 113), ' ', '-') FromDate,   
               REPLACE(CONVERT(VARCHAR(11), MAX(p.WorkDate), 113), ' ', '-') ToDate,
                COUNT(p.WorkDate) TotalProcDate,
                isnull(SUM(P.PresentValue),'0')TotalPresent,isnull(SUM(p.LateValue),'0')TotalLate,isnull(SUM(p.AbsentValue),'0')TotalAbsent
                ,isnull(SUM(p.LvValue),'0')TotalLv,isnull(SUM(p.CompAssignLvValue),'0')TotalCompAssignLv,
                isnull(SUM(p.WeekOffValue),'0')TotalWeekOff,isnull(SUM(p.HoliDayValue),'0')TotalHoliDay,isnull(SUM(p.WeekOffHoliDayValue),'0')TotalWeekOffHoliDay
               ,isnull(SUM(p.PayDayValue),'0')TotalPayDay,isnull(SUM(p.ActualWorkingDayValue),'0')TotalActualDays
			            from AttdnProcessData p
                        where isnull(p.DayStatus,'')!='' and WorkDate between '" + objm.FDate + @"'
						 and '" + objm.TDate + @"' group BY EmpSystemID) as dd
						 join EmployeeInformation  e on e.SystemId=dd.EmployeePK	
                  LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
                                    left join hkp.party cdata on cdata.id=e.VendorId									
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
                             where
							  (E.DOS IS NULL  OR E.DOS >= '" + objm.FDate + @"') 
									AND E.DOJ <= '" + objm.TDate + "'";


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

        Color ContrastColor(Color color)
        {
            int d = 0;
            // Counting the perceptive luminance - human eye favors green color... 
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
            if (luminance > 0.5)
                d = 0; // bright colors - black font
            else
                d = 255; // dark colors - white font
            return Color.FromArgb(d, d, d);
        }

        public void GetWeekOffDays(Dictionary<string, string> empParameters, ParaMontlyAttendance objm, out DataSet ds)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string empStr = "";
                if (empParameters.Count > 0)
                {
                    if (empParameters.Keys.ElementAt(0) != "")
                    {
                        empStr = @" AND EmpSystemID IN(" + empParameters["EmpSystemId"] + ")";
                    }
                }

                string Month = Convert.ToDateTime(objm.FDate).Month.ToString();
                string Year = Convert.ToDateTime(objm.FDate).Year.ToString();
                objCon = new ConnectionManager.DAL.ConManager("1");
                
                var sql = @"SELECT EmpSystemID,
                WeekOffDays = STUFF((
                SELECT '-' + format(WorkDate,'dd') as FF
                FROM AttdnProcessData ap
                WHERE ap.EmpSystemID = p.EmpSystemID and ap.WeekOffValue = '1' 
                and OtMonth='" + Month + "' and OtYear='" + Year + "'" + empStr + @"
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 1, '')
                FROM AttdnProcessData p
                where p.WeekOffValue='1' and OtMonth='" + Month + "' and OtYear='" + Year + "' " + empStr + @"
                group by EmpSystemID";
                
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }

    #endregion

    public class AttdnBonusMasterService
    {

        ISqlRepository _sqlRepository;
        public AttdnBonusMasterService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region Add/Edit Section
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        #endregion

        #region PlantChild Functions
      
        public IEnumerable<object> getChildData(string MasterId)
        {
            try
            {
                var sql = @"Select * from dbo.attdnbonusplantchild where HeaderId ='" + MasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> saveChild(Dictionary<string, object> Child)
        {
            try
            {
                string TableName = "dbo.attdnbonusplantchild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where PlantId ='" + Child["PlantId"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);

                    Child["Id"] = "PC"+_Id;
                    AddNewRow(dsMaster.Tables[0], Child);
                }
                else
                {
                    throw new Exception("Already same Combination is Present!");
                }

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Child;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
      
        public string DeleteChild(string id)
        {
            try
            {
                string TableName = "dbo.attdnbonusplantchild";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();
                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        #endregion

        #region Header Functions
        public double GetSequence()
        {
            string TableName = "dbo.AttdnBonusHeader";
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        public IEnumerable<object> getMaster()
        {
            try
            {
                var str = @"Select * from dbo.AttdnBonusHeader";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> getHeader()
        {
            try
            {
                var str = @"Select * from dbo.AttdnBonusHeader";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public Dictionary<string, object> saveHeader(Dictionary<string, object> Header)
        {
            try
            {
                string TableName = "dbo.AttdnBonusHeader";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id<>'" + Header["Id"] + "' and UserName='" + Header["UserName"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id<>'" + Header["Id"] + "' and StandardName='" + Header["StandardName"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same StandardName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Header["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Header["Id"] = "BH"+_Id;
                    AddNewRow(dsMaster.Tables[0], Header);
                }
                else
                {
                    _Id = Header["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Header);
                }

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Header;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public double GetSequenceHeader()
        {
            string TableName = "dbo.AttdnBonusHeader";
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        #endregion

        #region Rules Functions
        public IEnumerable<object> getRulesList(string Id)
        {
            try
            {
                var str = @"Select * from dbo.AttdnBonusRuleChild where HeaderId ='" + Id + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> SaveRuleMaster(Dictionary<string, object> Header)
        {
            try
            {
                string TableName = "dbo.AttdnBonusRuleChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where HeaderId='" + Header["HeaderId"] + "' and UserName='" + Header["UserName"] + "' and Id<>'" + Header["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where HeaderId='" + Header["HeaderId"] + "' and StandardName='" + Header["StandardName"] + "' and Id<>'" + Header["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same StandardName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Header["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Header["Id"] ="RC"+ _Id;
                    AddNewRow(dsMaster.Tables[0], Header);
                }
                else
                {
                    _Id = Header["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Header);
                }

                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Header;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region Bonus Query Code Commented
        //        select dd.* , ar.Amount, ar.Id
        //from AttdnBonusRuleChild ar
        //left join
        //(select distinct p.EmpSystemID as EmpId,
        //Sum(dtv.AttnBonusAbsent) AbsentCount, sum(dtv.AttnBonusLate) LateCount,
        //sum(dtv.AttnBonusLeave) LeaveCount, dc.AttdnBonusHeaderId as HeaderId, p.PlantID
        //from AttdnProcessData p join
        //EmployeeInformation e on e.SystemId= p.EmpSystemID
        //left join DayStatusHeader dh on dh.Id= p.DayStatusHeaderId
        //left join DayTypeWithValues dtv on dtv.DayType= p.DayStatus
        //left join mst.DesignationMasterLegalDesignation ddm on
        //ddm.LegalDesignationId = e.LegalDesignationId
        //left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
        //left join scs.DesignationMasterConfiguration dc on dc.DesignationMasterId= dm.Id
        //and dc.PlantId= e.PlantId
        //left join attdnbonusheader ah on ah.Id= dc.AttdnBonusHeaderId
        //where p.otmonth= '11' and p.otyear= '2021' and p.PlantId= '202016' AND AH.Active= 1
        //group by p.EmpSystemID, dc.AttdnBonusHeaderId, p.PlantID)
        //as dd on ar.HeaderId=dd.HeaderId and
        //dd.AbsentCount = ar.AbsentValue
        //and dd.LateCount = ar.LateValue and dd.LeaveCount=ar.LeaveValue
        //where dd.HeaderId is not null
        #endregion

    }
}

