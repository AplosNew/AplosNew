using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance
{
    public class clsMonthlyAttandanceSummery
    {
        ISqlRepository _sqlRepository;
        public clsMonthlyAttandanceSummery()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetYear(string PlantID)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT ID, YearNo FROM YearlyCalendar WHERE PlantID = '" + PlantID + @"'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IWorkbook XlsMonthlyAttendanceSummaryRpt(string Year, string Month)//XlsMonthlyAttendanceSummaryRpt()
        {
            #region Variable

            clsReport objRpt = null;

            DataSet dsHeading = null;

            DataSet dsAttn = null;
            DataView dvAttn = null;

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

            //dtFrmDt = Convert.ToDateTime(this.ddlMonthNo.Text.Trim() + "/" + "01/" + this.ddlYearNo.SelectedItem.Text.Trim());
            //string m = bplib.clsWebLib.GetMonthName(ddlMonthNo.Text);
            //dtFrmDt = Convert.ToDateTime("01-" + m + "-" + this.ddlYearNo.SelectedItem.Text.Trim());
            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objRpt = new clsReport();
                #region Validation

                if (Month.Trim() == null)
                {
                    Exception ex = new Exception("Please select Month !!");
                    throw ex;
                }

                if (Year.Trim() == "ALL")
                {
                    Exception ex = new Exception("Ooops.....Select Year !!!");
                    throw (ex);
                }

                string m = bplib.clsWebLib.GetMonthName(Month);
                dtFrmDt = Convert.ToDateTime("01-" + m + "-" + Year);

                if (Convert.ToInt32(DateTime.Now.Month) != Convert.ToInt32(Month))
                {
                    dtEndDate = dtFrmDt.AddMonths(1).AddDays(-1);
                }
                if (string.IsNullOrEmpty(Month) == true)
                {                    
                    Exception ex = new Exception("Please select month No...!");
                    throw (ex);
                }

                if (string.IsNullOrEmpty(Year.Trim()) == true)
                {                    
                    Exception ex = new Exception("Please select year No...!");
                    throw (ex);
                }

                #endregion Validation

                #region Variable

                string sUnit = "ALL";
                string sDevi = "ALL";
                string sDept = "ALL";
                string sSect = "ALL";
                string sSbSe = "ALL";
                string sLine = "ALL";
                //string sSbSeStr = this.ddlSubSecStruc.SelectedValue.ToString().Trim();
                string sEmpC = "ALL";
                string sDeGr = "ALL";
                string sDesi = "ALL";

                #endregion Variable

                #region DataSet

                objRpt.GetMonthlyAttnSummaryRpt(identity.PlantId, Convert.ToInt16(Month), Convert.ToInt16(Year), sUnit, sDevi, sDept, sSect, sSbSe, sLine, sEmpC, sDeGr, sDesi, out dsAttn);
                //objRpt.GetMonthlyAttnSummaryRpt(this.ddlPlant.SelectedValue.ToString().Trim(), Convert.ToInt16(this.ddlMonthNo.Text.Trim()), Convert.ToInt16(this.ddlYearNo.SelectedItem.Text.Trim()), sUnit, sDevi, sDept, sSect, sSbSe, sLine, sSbSeStr, sEmpC, sDeGr, sDesi, out dsAttn);
                dvAttn = new DataView();
                dvAttn.Table = dsAttn.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 5;
                    int intRow = 0;

                    #region Variables

                    string x = "";
                    string strSubSec = "0";
                    int strCount = 0;

                    int iSrNo = 0;
                    int iEmpCode = 0;
                    int iEmpName = 0;
                    int iDOJ = 0;
                    int iDesig = 0;
                    int iFDate = 0;
                    int iTDate = 0;
                    int iTtlAPD = 0;
                    int iTtlHD = 0;
                    int iTtlWO = 0;
                    int iTtlWOHD = 0;
                    int iTtlPst = 0;
                    int iTtlAbs = 0;
                    int iTtlLte = 0;
                    int iTtlLv = 0;
                    int iTtlMLv = 0;
                    int iTtlOTHr = 0;
                    int iTtlNorOTHr = 0;
                    int iTtlEOTHr = 0;

                    #endregion
                    if (dvAttn.Count > 0)
                    {
                        for (int i = 0; i <= dvAttn.Count - 1; i++)
                        {
                            xlsCol = 1;
                            if ((string.Compare(strSubSec.ToUpper(), dvAttn[i]["SubSection"].ToString().Trim().ToUpper())) != 0)
                            {
                                xlsRow += intRow;
                                intRow = 2;
                                strCount = 0;

                                #region ------------------Column Header------------------

                                #region --------------------Top Header--------------------

                                sheet1.Range[xlsRow, 1].Text = "Unit :-" + dvAttn[i]["Unit"].ToString();
                                sheet1.Range[xlsRow, 1, xlsRow, 4].Merge();
                                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 12;
                                sheet1.Range[xlsRow, 1, xlsRow, 4].RowHeight = 21;
                                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, 5].Text = "Division :-" + dvAttn[i]["Division"].ToString();
                                sheet1.Range[xlsRow, 5, xlsRow, 8].Merge();
                                sheet1.Range[xlsRow, 5].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, 5].CellStyle.Font.Size = 12;
                                sheet1.Range[xlsRow, 5, xlsRow, 8].RowHeight = 21;
                                sheet1.Range[xlsRow, 5].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, 9].Text = "Department :-" + dvAttn[i]["Department"].ToString();
                                sheet1.Range[xlsRow, 9, xlsRow, 12].Merge();
                                sheet1.Range[xlsRow, 9].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, 9].CellStyle.Font.Size = 12;
                                sheet1.Range[xlsRow, 9, xlsRow, 12].RowHeight = 21;
                                sheet1.Range[xlsRow, 9].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, 13].Text = "Section :-" + dvAttn[i]["Section"].ToString();
                                sheet1.Range[xlsRow, 13, xlsRow, 16].Merge();
                                sheet1.Range[xlsRow, 13].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, 13].CellStyle.Font.Size = 12;
                                sheet1.Range[xlsRow, 13, xlsRow, 16].RowHeight = 21;
                                sheet1.Range[xlsRow, 13].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet1.Range[xlsRow, 17].Text = "Sub Section :-" + dvAttn[i]["SubSection"].ToString();
                                sheet1.Range[xlsRow, 17, xlsRow, 19].Merge();
                                sheet1.Range[xlsRow, 17].CellStyle.Font.Bold = true;
                                sheet1.Range[xlsRow, 17].CellStyle.Font.Size = 12;
                                sheet1.Range[xlsRow, 17, xlsRow, 19].RowHeight = 21;
                                sheet1.Range[xlsRow, 17].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                #endregion --------------------Top Header--------------------

                                #region ------------------Details Header-----------------

                                xlsRow += 1;

                                xlsCol = 1;
                                iSrNo = xlsCol;
                                sheet1.Range[xlsRow, iSrNo].Text = "Sl No.";
                                sheet1.Range[xlsRow, iSrNo].ColumnWidth = 4.70;
                                sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iEmpCode = xlsCol;
                                sheet1.Range[xlsRow, iEmpCode].Text = "Employee Code";
                                sheet1.Range[xlsRow, iEmpCode].ColumnWidth = 8.50;
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
                                iDesig = xlsCol;
                                sheet1.Range[xlsRow, iDesig].Text = "Designation";
                                sheet1.Range[xlsRow, iDesig].ColumnWidth = 15;
                                sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iFDate = xlsCol;
                                sheet1.Range[xlsRow, iFDate].Text = "From Date";
                                sheet1.Range[xlsRow, iFDate].ColumnWidth = 12;
                                sheet1.Range[xlsRow, iFDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iFDate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTDate = xlsCol;
                                sheet1.Range[xlsRow, iTDate].Text = "To Date";
                                sheet1.Range[xlsRow, iTDate].ColumnWidth = 12;
                                sheet1.Range[xlsRow, iTDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTDate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlAPD = xlsCol;
                                sheet1.Range[xlsRow, iTtlAPD].Text = "Total Attendance Processed Day";
                                sheet1.Range[xlsRow, iTtlAPD].ColumnWidth = 14;
                                sheet1.Range[xlsRow, iTtlAPD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlAPD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlHD = xlsCol;
                                sheet1.Range[xlsRow, iTtlHD].Text = "Total HoliDay";
                                sheet1.Range[xlsRow, iTtlHD].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlHD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlHD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlWO = xlsCol;
                                sheet1.Range[xlsRow, iTtlWO].Text = "Total WeekOff";
                                sheet1.Range[xlsRow, iTtlWO].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlWO].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlWO].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlWOHD = xlsCol;
                                sheet1.Range[xlsRow, iTtlWOHD].Text = "TotalWeekOffHoliDay";
                                sheet1.Range[xlsRow, iTtlWOHD].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlWOHD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlWOHD].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlPst = xlsCol;
                                sheet1.Range[xlsRow, iTtlPst].Text = "Total Present";
                                sheet1.Range[xlsRow, iTtlPst].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlPst].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlPst].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlAbs = xlsCol;
                                sheet1.Range[xlsRow, iTtlAbs].Text = "Total Absent";
                                sheet1.Range[xlsRow, iTtlAbs].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlAbs].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlAbs].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlLte = xlsCol;
                                sheet1.Range[xlsRow, iTtlLte].Text = "Total Late";
                                sheet1.Range[xlsRow, iTtlLte].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlLte].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlLte].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlLv = xlsCol;
                                sheet1.Range[xlsRow, iTtlLv].Text = "Total Leave";
                                sheet1.Range[xlsRow, iTtlLv].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlLv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlMLv = xlsCol;
                                sheet1.Range[xlsRow, iTtlMLv].Text = "Total M Leave";
                                sheet1.Range[xlsRow, iTtlMLv].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlMLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlMLv].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlOTHr = xlsCol;
                                sheet1.Range[xlsRow, iTtlOTHr].Text = "Total OT Hour";
                                sheet1.Range[xlsRow, iTtlOTHr].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlOTHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlNorOTHr = xlsCol;
                                sheet1.Range[xlsRow, iTtlNorOTHr].Text = "Total Normal OT Hour";
                                sheet1.Range[xlsRow, iTtlNorOTHr].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlNorOTHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlNorOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                xlsCol += 1;
                                iTtlEOTHr = xlsCol;
                                sheet1.Range[xlsRow, iTtlEOTHr].Text = "Total Extra OT Hour";
                                sheet1.Range[xlsRow, iTtlEOTHr].ColumnWidth = 10;
                                sheet1.Range[xlsRow, iTtlEOTHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iTtlEOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                #endregion ------------------Details Header-------------------------

                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                                sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                                endXlsCol = xlsCol;
                                xlsCol = 1;
                                xlsRow += 1;

                                #endregion ------------------Column Header------------------
                            }
                            strSubSec = dvAttn[i]["SubSection"].ToString().Trim();

                            #region ----------------------Data-----------------------

                            strCount += 1;
                            sheet1.Range[xlsRow, iSrNo].Number = strCount;
                            sheet1.Range[xlsRow, iSrNo].RowHeight = 13;
                            sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iEmpCode].Text = dvAttn[i]["EmployeeCode"].ToString().Trim();
                            sheet1.Range[xlsRow, iEmpCode].RowHeight = 13;
                            sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iEmpName].Text = dvAttn[i]["EmployeeName"].ToString().Trim();
                            sheet1.Range[xlsRow, iEmpName].RowHeight = 13;
                            sheet1.Range[xlsRow, iEmpName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, iEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iDOJ].Text = dvAttn[i]["DOJ"].ToString().Trim();
                            sheet1.Range[xlsRow, iDOJ].RowHeight = 13;
                            sheet1.Range[xlsRow, iDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, iDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iDesig].Text = dvAttn[i]["Designation"].ToString().Trim();
                            sheet1.Range[xlsRow, iDesig].RowHeight = 13;
                            sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iFDate].Text = dvAttn[i]["FromDate"].ToString().Trim();
                            sheet1.Range[xlsRow, iFDate].RowHeight = 13;
                            sheet1.Range[xlsRow, iFDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iFDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTDate].Text = dvAttn[i]["ToDate"].ToString().Trim();
                            sheet1.Range[xlsRow, iTDate].RowHeight = 13;
                            sheet1.Range[xlsRow, iTDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTDate].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlAPD].Text = dvAttn[i]["TotalProcDate"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlAPD].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlAPD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlAPD].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlHD].Text = dvAttn[i]["TotalHoliDay"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlHD].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlHD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlHD].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlWO].Text = dvAttn[i]["TotalWeekOff"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlWO].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlWO].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlWO].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlWOHD].Text = dvAttn[i]["TotalWeekOffHoliDay"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlWOHD].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlWOHD].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlWOHD].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlPst].Text = dvAttn[i]["TotalPresent"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlPst].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlPst].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlPst].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlAbs].Text = dvAttn[i]["TotalAbsent"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlAbs].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlAbs].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlAbs].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlLte].Text = dvAttn[i]["TotalLate"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlLte].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlLte].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlLte].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlLv].Text = dvAttn[i]["TotalLv"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlLv].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlLv].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlMLv].Text = dvAttn[i]["TotalMLv"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlMLv].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlMLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlMLv].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlOTHr].Text = dvAttn[i]["TotalOTHr"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlOTHr].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlOTHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlNorOTHr].Text = dvAttn[i]["TotalNormalOTHr"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlNorOTHr].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlNorOTHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlNorOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, iTtlEOTHr].Text = dvAttn[i]["TotalExtraOTHr"].ToString().Trim();
                            sheet1.Range[xlsRow, iTtlEOTHr].RowHeight = 13;
                            sheet1.Range[xlsRow, iTtlEOTHr].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlEOTHr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsRow += 1;

                            #endregion ----------------------Data-----------------------

                            #region Line Setup
                            sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].WrapText = true;
                            #endregion
                        }
                    }
                    else
                    {
                        throw new Exception ("No Data Found....");
                    }


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
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
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
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 26;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Monthly Attendance Summary";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Year No:- " + Year + " and Month:- " + Convert.ToDateTime(dtFrmDt).ToString("MMMM");
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
                    sheet1.UsedRange["A7"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$1:$5";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                    sheet1.Name = "Monthly Attendance Summary";
                    #endregion

                    workbook.Version = ExcelVersion.Excel2016;
                    string strFileName = "MonthlyAttandanceSummeryReport" + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xls";
                    //workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, Response, ExcelDownloadType.PromptDialog);

                }
                    return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objStatic = null;
                //objRpt = null;
                //dsHeading = null;

                //excelEngine = null;
                //application = null;
                //workbook = null;
            }
        }
    }
}
