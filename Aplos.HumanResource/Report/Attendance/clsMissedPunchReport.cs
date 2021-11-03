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
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;

namespace Library.HumanResource.Report.Attendance
{
    public class clsMissedPunchReport
    {
        public string MissedPunchReport(out ExcelEngine excelEngine, string workDate, string sDepID, string sSecID, string sSubSecID, string sLineID, bool chkIntime, bool chkoutTime, string ShiftId, string JobLocation, string designationList, string enttyList, string empCategoryList, bool WithFatherName)
        {
            ReportUtility oRU = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            excelEngine = null;
            try
            {
                oRU = new ReportUtility();

                workbook = oRU.GetWorkbook(ref excelEngine, 1);
                sheet1 = workbook.Worksheets[0];
                MisMatchInOutPunch(ref sheet1, workDate, sDepID, sSecID, sSubSecID, sLineID, chkIntime, chkoutTime, ShiftId, JobLocation, designationList, enttyList, empCategoryList, WithFatherName);
                workbook.Version = ExcelVersion.Excel2013;

                var filePath = "";
                var SheetName = "";
                workbook.Version = ExcelVersion.Excel97to2003;
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void MisMatchInOutPunch(ref IWorksheet sheet1, string workDate, string sDepID, string sSecID, string sSubSecID, string sLineID, bool chkIntime, bool chkoutTime, string ShiftId, string JobLocation, string designationList, string enttyList, string empCategoryList, bool WithFatherName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            #region Variable

            clsReport objRpt = null;
            DataSet dsAttn = null;
            DataView dvAttn = null;
            DataSet dsCmp = null;
            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            DataSet dsFactory = null;
            #endregion Variable

            try
            {
                objRpt = new clsReport();

                var ob = new clsStaticInfo();

                #region Variable

                string sUnit = enttyList;
                string sDevi = "ALL";
                string sDept = sDepID;
                string sSect = sSecID;
                string sSbSe = sSubSecID;
                string sLine = sLineID;
                string JLoc = JobLocation;
                string sEmpC = empCategoryList;
                string sDeGr = "ALL";
                string sDesi = designationList;

                #endregion Variable

                #region Validation
                if (string.IsNullOrEmpty(workDate) == true || bplib.clsWebLib.IsDateOK(workDate) == false)
                {
                    Exception ex = new Exception("Please define Attendance Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2020')...");
                    throw (ex);
                }
                if (chkIntime == false && chkoutTime == false)
                {
                    Exception ex = new Exception("You must tick a check-box");
                    throw (ex);
                }
                #endregion Validation

                #region DataSet

                string wc = "";
                if (chkIntime)
                {
                    wc = "  (AP.InTime IS NULL AND AP.OutTime IS NOT NULL)";
                }
                if (chkoutTime)
                {
                    if (!string.IsNullOrEmpty(wc))
                    {
                        wc += " OR (AP.OutTime IS NULL AND AP.InTime IS NOT NULL)";
                    }
                    else
                    {
                        wc = " (AP.OutTime IS NULL AND AP.InTime IS NOT NULL)";
                    }
                }

                if (wc.Length > 0)
                {
                    wc = " AND (" + wc + ")";
                }
                GetMisMatchPunchRpt(identity.PlantId, workDate, sUnit, JLoc, sDevi, sDept, sSect, sSbSe, sLine, sDeGr, sDesi, sEmpC, wc, ShiftId, out dsAttn);                
                dvAttn = new DataView();
                dvAttn.Table = dsAttn.Tables[0];
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                objRpt.SelectedPlant(identity.PlantId, out dsFactory);
                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 7;
                    int intRow = 0;
                    string strSubSec = "0";
                    int strCount = 0;
                    var colSL = 0;
                    var ColEmployeeCode = 0;
                    var ColEmployeeName = 0;
                    var ColFatherName = 0;
                    var ColDesignation = 0;
                    var ColSection = 0;
                    var ColSubSection = 0;
                    var ColLine = 0;
                    var ColShiftName = 0;
                    var ColShiftInTime = 0;
                    var ColPunchInTime = 0;
                    var ColInTime = 0;
                    var ColOutTime = 0;
                    var ColDayStatus = 0;
                    var ColWorkerSignature = 0;
                    var ColInchargeSignature = 0;

                    #region ------------------Column Header------------------

                    xlsCol = 1;
                    colSL = xlsCol;
                    sheet1.Range[xlsRow, colSL].Text = "Sl No.";
                    sheet1.Range[xlsRow, colSL].ColumnWidth = 4.70;
                    sheet1.Range[xlsRow, colSL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, colSL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColEmployeeCode = xlsCol;
                    sheet1.Range[xlsRow, ColEmployeeCode].Text = "Employee Code";
                    sheet1.Range[xlsRow, ColEmployeeCode].ColumnWidth = 8.50;
                    sheet1.Range[xlsRow, ColEmployeeCode].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColEmployeeCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColEmployeeName = xlsCol;
                    sheet1.Range[xlsRow, ColEmployeeName].Text = "Employee Name";
                    sheet1.Range[xlsRow, ColEmployeeName].ColumnWidth = 25;
                    sheet1.Range[xlsRow, ColEmployeeName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColEmployeeName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (WithFatherName == true)
                    {
                        xlsCol += 1;
                        ColFatherName = xlsCol;
                        sheet1.Range[xlsRow, ColFatherName].Text = "Father Name";
                        sheet1.Range[xlsRow, ColFatherName].ColumnWidth = 25;
                        sheet1.Range[xlsRow, ColFatherName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, ColFatherName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }

                    xlsCol += 1;
                    ColDesignation = xlsCol;
                    sheet1.Range[xlsRow, ColDesignation].Text = "Designation";
                    sheet1.Range[xlsRow, ColDesignation].ColumnWidth = 30;
                    sheet1.Range[xlsRow, ColDesignation].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColDesignation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColSection = xlsCol;
                    sheet1.Range[xlsRow, ColSection].Text = "Section";
                    sheet1.Range[xlsRow, ColSection].ColumnWidth = 20;
                    sheet1.Range[xlsRow, ColSection].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColSection].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColSubSection = xlsCol;
                    sheet1.Range[xlsRow, ColSubSection].Text = "Sub-Section";
                    sheet1.Range[xlsRow, ColSubSection].ColumnWidth = 20;
                    sheet1.Range[xlsRow, ColSubSection].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColLine = xlsCol;
                    sheet1.Range[xlsRow, ColLine].Text = "Line";
                    sheet1.Range[xlsRow, ColLine].ColumnWidth = 20;
                    sheet1.Range[xlsRow, ColLine].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColLine].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    xlsCol += 1;
                    ColShiftName = xlsCol;
                    sheet1.Range[xlsRow, ColShiftName].Text = "Shift Name";
                    sheet1.Range[xlsRow, ColShiftName].ColumnWidth = 25;
                    sheet1.Range[xlsRow, ColShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColShiftInTime = xlsCol;
                    sheet1.Range[xlsRow, ColShiftInTime].Text = "Shift InTime";
                    sheet1.Range[xlsRow, ColShiftInTime].ColumnWidth = 7;
                    sheet1.Range[xlsRow, ColShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColPunchInTime = xlsCol;
                    sheet1.Range[xlsRow, ColPunchInTime].Text = "First Punch In Time";
                    sheet1.Range[xlsRow, ColPunchInTime].ColumnWidth = 10;
                    sheet1.Range[xlsRow, ColPunchInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColPunchInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColInTime = xlsCol;
                    sheet1.Range[xlsRow, ColInTime].Text = "InTime";
                    sheet1.Range[xlsRow, ColInTime].ColumnWidth = 7;
                    sheet1.Range[xlsRow, ColInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColOutTime = xlsCol;
                    sheet1.Range[xlsRow, ColOutTime].Text = "OutTime";
                    sheet1.Range[xlsRow, ColOutTime].ColumnWidth = 7;
                    sheet1.Range[xlsRow, ColOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColDayStatus = xlsCol;
                    sheet1.Range[xlsRow, ColDayStatus].Text = "Day Status";
                    sheet1.Range[xlsRow, ColDayStatus].ColumnWidth = 8;
                    sheet1.Range[xlsRow, ColDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColWorkerSignature = xlsCol;
                    sheet1.Range[xlsRow, ColWorkerSignature].Text = "Worker Signature";
                    sheet1.Range[xlsRow, ColWorkerSignature].ColumnWidth = 20;
                    //sheet1.Range[xlsRow, ColWorkerSignature].RowHeight = 30;
                    sheet1.Range[xlsRow, ColWorkerSignature].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColWorkerSignature].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    ColInchargeSignature = xlsCol;
                    sheet1.Range[xlsRow, ColInchargeSignature].Text = "InCharge Signature";
                    sheet1.Range[xlsRow, ColInchargeSignature].ColumnWidth = 20;
                    //sheet1.Range[xlsRow, ColInchargeSignature].RowHeight = 30;
                    sheet1.Range[xlsRow, ColInchargeSignature].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, ColInchargeSignature].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;

                    xlsCol = 1;
                    xlsRow += 1;

                    #endregion ------------------Column Header------------------

                    for (int i = 0; i <= dsAttn.Tables[0].Rows.Count - 1; i++)
                    {
                        //if (xlsRow==30)
                        //{

                        //}
                        //}
                        strSubSec = dvAttn[i]["SubSection"].ToString().Trim();

                        #region ----------------------Data-----------------------

                        strCount += 1;
                        sheet1.Range[xlsRow, colSL].Number = strCount;
                        sheet1.Range[xlsRow, colSL].RowHeight = 13;
                        sheet1.Range[xlsRow, colSL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, colSL].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColEmployeeCode].Text = dvAttn[i]["EmployeeCode"].ToString().Trim();
                        sheet1.Range[xlsRow, ColEmployeeCode].RowHeight = 13;
                        sheet1.Range[xlsRow, ColEmployeeCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColEmployeeCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColEmployeeName].Text = dvAttn[i]["EmployeeName"].ToString().Trim();
                        sheet1.Range[xlsRow, ColEmployeeName].RowHeight = 13;
                        sheet1.Range[xlsRow, ColEmployeeName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColEmployeeName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        if (WithFatherName == true)
                        {
                            xlsCol += 1;
                            sheet1.Range[xlsRow, ColFatherName].Text = dvAttn[i]["FatherName"].ToString().Trim();
                            sheet1.Range[xlsRow, ColFatherName].RowHeight = 13;
                            sheet1.Range[xlsRow, ColFatherName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            sheet1.Range[xlsRow, ColFatherName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColDesignation].Text = dvAttn[i]["Designation"].ToString().Trim();
                        sheet1.Range[xlsRow, ColDesignation].RowHeight = 13;
                        sheet1.Range[xlsRow, ColDesignation].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColDesignation].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColSection].Text = dvAttn[i]["Section"].ToString().Trim();
                        sheet1.Range[xlsRow, ColSection].RowHeight = 13;
                        sheet1.Range[xlsRow, ColSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSection].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColSubSection].Text = dvAttn[i]["SubSection"].ToString().Trim();
                        sheet1.Range[xlsRow, ColSubSection].RowHeight = 13;
                        sheet1.Range[xlsRow, ColSubSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColLine].Text = dvAttn[i]["Line"].ToString().Trim();
                        sheet1.Range[xlsRow, ColLine].RowHeight = 13;
                        sheet1.Range[xlsRow, ColLine].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColLine].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColShiftName].Text = dvAttn[i]["ShiftName"].ToString().Trim();
                        sheet1.Range[xlsRow, ColShiftName].RowHeight = 13;
                        sheet1.Range[xlsRow, ColShiftName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, ColShiftName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColShiftInTime].Text = dvAttn[i]["ShiftIn"].ToString().Trim();
                        sheet1.Range[xlsRow, ColShiftInTime].RowHeight = 13;
                        sheet1.Range[xlsRow, ColShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, ColShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColPunchInTime].Text = dvAttn[i]["LeastPunchTime"].ToString().Trim();
                        sheet1.Range[xlsRow, ColPunchInTime].RowHeight = 13;
                        sheet1.Range[xlsRow, ColPunchInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, ColPunchInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColInTime].Text = dvAttn[i]["InTimeShow"].ToString().Trim();
                        sheet1.Range[xlsRow, ColInTime].RowHeight = 13;
                        sheet1.Range[xlsRow, ColInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, ColInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, ColOutTime].Text = dvAttn[i]["OutTimeShow"].ToString().Trim();
                        sheet1.Range[xlsRow, ColOutTime].RowHeight = 13;
                        sheet1.Range[xlsRow, ColOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, ColOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;

                        if (dvAttn[i]["DayStatus"].ToString().Trim() == "L")
                        {
                            sheet1.Range[xlsRow, ColDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;
                            sheet1.Range[xlsRow, ColDayStatus].Text = "P";
                        }
                        else
                        {
                            sheet1.Range[xlsRow, ColDayStatus].Text = dvAttn[i]["DayStatus"].ToString().Trim();
                        }
                        sheet1.Range[xlsRow, ColDayStatus].RowHeight = 13;
                        sheet1.Range[xlsRow, ColDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, ColDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, endXlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, endXlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsRow += 1;

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].RowHeight = 30;
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup
                    }

                    #region UsedRange Alignment

                    sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region ******************Report Header******************

                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), identity.CompanyId + ".jpg");  // IDCardEng.xlsx
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
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 26;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Missed Punch Report";
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, 3].Text = "Attendance Date:- " + workDate;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 9;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A8"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 7;

                    #endregion Freeze Panes

                    #region Page Setup


                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$1:$5";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                    sheet1.Name = "Missed Punch Information";

                    #endregion Page Setup
                }
                else
                {
                    throw new Exception("No Data found...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
            }
        }

        public void GetMisMatchPunchRpt(string sPlantID, string WDate, string sUnitID, string JLoc, string sDivID, string sDepID, string sSecID, string sSubSecID, string sLineID, string sDesigGrpID, string sDesigID, string sEmpCatID, string wc, string ShiftId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string secSQL = string.Empty;
            //string xxy = string.Empty;
            string XJobLocation = string.Empty;
            clsStaticInfo obs = null;
            string ShiftIds_WC = "";
            try
            {
                if (ShiftId != "ALL" && ShiftId != "''" && ShiftId != "'ALL'")
                {
                    ShiftIds_WC = " and sd.SystemID in (" + ShiftId + ") ";
                }
                obs = new clsStaticInfo();
                strSql = @" select e.SystemId
                                            from EmployeeInformation e
                                            left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
											left join org.Department dep on dep.Id = p.DepartmentId
											left join org.Section s on s.Id = p.SectionId
											left join org.SubSection ss on ss.Id = p.SubSectionId                                       
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id 
											left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
											left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
											left join HKP.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId
                                            
											where   e.PlantId='" + sPlantID + @"' and e.DOJ <= ( '" + WDate + @"') and (e.DOS is null or e.DOS >= '" + WDate + @"')  ";

                if (sDepID != "ALL")
                {
                    strSql = strSql + @" AND dep.Id in ( " + sDepID + ")";
                }
                if (sSecID != "ALL")
                {
                    strSql = strSql + @" AND s.Id in (" + sSecID + ")";
                }
                if (sSubSecID != "ALL")
                {
                    strSql = strSql + @" AND ss.Id in (" + sSubSecID + ")";
                }

                if (sEmpCatID != "ALL")
                {
                    strSql = strSql + @" AND ec.Id in (" + sEmpCatID + ")";
                }

                if (sUnitID != "ALL")
                {
                    strSql = strSql + @" AND en.Id in (" + sUnitID + ")";
                }
                if (sLineID != "ALL" && sLineID != "''")
                {
                    strSql = strSql + @" AND isnull(L.Id,'') in (" + sLineID + ")";
                }
                if (sDesigID != "ALL" && sDesigID != "''")
                {
                    strSql = strSql + @" AND LG.Id in (" + sDesigID + ")";
                }
                if (JLoc != "ALL" && JLoc != "''")
                {
                    strSql = strSql + @"And J.SystemID in (" + JLoc + ")";
                }
                secSQL = @"SELECT e.SystemId EmpSystemId,e.EmployeeCode, e.EmployeeName,e.FatherName,dep.username Department
                                    ,L.UserName Line,SS.UserName SubSection,s.UserName Section
								,sd.UserName ShiftName
                                , ShiftIn  = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
						     END
	                            , ap.DayStatus
                                    
                                    
                        , LG.UserName Designation,CONVERT(VARCHAR(15),CAST(LIT.ptime AS TIME),100)  +' ('+ ARD.PType+')' LeastPunchTime
						,CONVERT(varchar(15),CAST(AP.InTime AS TIME),100) InTimeShow,CONVERT(varchar(15),CAST(AP.OutTime AS TIME),100) OutTimeShow

                        from EmployeeInformation e
 
                        left join AttdnProcessData ap on ap.EmpSystemID = e.SystemId
                            LEFT JOIN(SELECT LogDownLoadNum
												,MIN(ptime) ptime
												FROM AttdnRawData
												WHERE pdate='" + WDate + @"' AND PType='IN'--and LogDownLoadNum='1800004'
												GROUP BY LogDownLoadNum
												) LIT ON LIT.LogDownLoadNum=E.SystemId
                        LEFT JOIN AttdnRawData ARD ON ARD.LogDownLoadNum=LIT.LogDownLoadNum  AND ARD.PTime=LIT.ptime

                        left join DayType dt on dt.DayType = ap.DayStatus                        
                        LEFT JOIN dbo.ShiftDefination SD ON ap.ShiftSystemID = SD.SystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS cs ON ap.WorkDate BETWEEN cs.FromDate AND cs.ToDate AND sd.SystemID=cs.ShiftDefinationID
                                            left join mst.ManpowerBudget mp on mp.id = e.BudgetCode
                                            left join org.Entity en on en.id = mp.EntityId
                                            left join ORG.Position p on p.Id = mp.PositionId
                                            left join org.Department dep on dep.Id = p.DepartmentId
                                            left join org.Section s on s.Id = p.SectionId
                                            LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=e.PlantId
                                            left join org.SubSection ss on ss.Id = p.SubSectionId
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id
                                            LEFT JOIN JobLocation J ON J.SystemID = e.JobLocationID
                                            left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
                                            left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
                                            left join HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                            where  ap.WorkDate='" + WDate + @"' " + wc + @" and e.SystemId in (" + strSql + ")  " + ShiftIds_WC + "  order by e.EmployeeCode   ";

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

        //private void GetMisMatchPunchRpt(string sPlantID, string WDate,  string sUnitID, string JLoc, string sDivID, string sDepID, string sSecID, string sSubSecID, string sLineID, string sDesigGrpID, string sDesigID, string sEmpCatID, string wc,string ShiftId, out DataSet dsRef)
        //{
        //    ConnectionManager.DAL.ConManager objCon;
        //    string strSql = string.Empty;
        //    clsStaticInfo obs = null;
        //    var _wj = string.Empty;

        //    if (JLoc.ToUpper() != "ALL")
        //    {
        //        _wj = "  AND E.JobLocationID in (" + JLoc + @")";
        //    }
        //    else
        //    {
        //        _wj = "";
        //    }
        //    try
        //    {
        //        obs = new clsStaticInfo();
        //        strSql = @"SELECT EmployeeCode
        //                        ,EmployeeCodeNumeric
        //                     , EmployeeName                                                      
        //                     , DOJ
        //                     , DesignationGroupID
        //                     , UnitID
        //                     , DivisionID
        //                     , DepartmentID
        //                     , SectionID
        //                     , SubSectionID
        //                     , LineID
        //                     , EmpCategoryID
        //                        , dti,dto
        //                     , PDate
        //                     , DayStatus
        //                     , InTime
        //                        ,ShiftName
        //                        ,ShiftInTimeShow
        //                        ,InTimeShow
        //                        ,OutTimeShow
        //                        --,CONVERT(VARCHAR(5),dateadd(MINUTE,-LeastInTime, ShiftInTime), 108) LeastEntryTime
        //                        ,LeastPunchTime
        //                     , InDeviceID
        //                     , OutTime
        //                     , OutDeviceID
        //                     , OTHr
        //                        ,LDesignation
        //                     , ShiftTime = CASE
        //                      WHEN ShiftChangeInTime IS NULL
        //                       THEN ShiftInTime
        //                      ELSE ShiftChangeInTime
        //                      END
        //                     , PlantID
        //                        " + EntityAlias() + @"
        //           FROM
        //                    (
        //                      SELECT E.EmployeeCode,E.EmployeeCodeNumeric
        //                         , isnull(E.FirstName,'') +' ' +isnull(E.MiddleName,'')+' ' +isnull(E.LastName,'') EmployeeName
        //                         , REPLACE(CONVERT(VARCHAR(11), E.DOJ, 113), ' ', '-') DOJ
        //                         , E.DesignationGroupID
        //                         , E.UnitID
        //                         , E.DivisionID
        //                         , E.DepartmentID
        //                         , E.SectionID
        //                         , E.SubSectionID
        //                         , E.LineID
        //                            ,AD.InTime dti,AD.OutTime dto
        //                         , REPLACE(CONVERT(VARCHAR(11), AD.WorkDate, 113), ' ', '-') PDate
        //                         , E.EmployeeCategorySystemID EmpCategoryID
        //                         , AD.DayStatus
        //                            ,CONVERT(VARCHAR(15),CAST(LIT.ptime AS TIME),100)  +' ('+ ARD.PType+')' LeastPunchTime
        //                         , CONVERT(VARCHAR(5), AD.InTime, 108) InTime
        //                            ,CONVERT(varchar(15),CAST(AD.InTime AS TIME),100) InTimeShow
        //                         , ARIN.DeviceID InDeviceID
        //                         , CONVERT(VARCHAR(5), AD.OutTime, 108) OutTime
        //                            ,CONVERT(varchar(15),CAST(AD.OutTime AS TIME),100) OutTimeShow
        //                         , AROUT.DeviceID OutDeviceID
        //                         , AD.OTHr
        //                         , CONVERT(VARCHAR(5), SFCG.InTime, 108) ShiftChangeInTime
        //                            --,sd.InTimeStartMargin LeastInTime
        //                         , CONVERT(VARCHAR(5), SD.InTime, 108) ShiftInTime
        //                            ,CONVERT(varchar(15),CAST(SD.InTime AS TIME),100) ShiftInTimeShow
        //                            , SD.ShiftDefinationName ShiftName
        //                         , AD.PlantID
        //                            ,E.GivenDesignationId  
        //                            ,LD.UserName LDesignation
        //                            " + EntityColumns() + @"

        //                        FROM dbo.EmployeeInformation E
        //               INNER JOIN dbo.AttdnProcessData AD ON E.SystemID = AD.EmpSystemID
        //               LEFT JOIN (SELECT * FROM dbo.ShiftTimeChgMaster WHERE '" + WDate + @"' BETWEEN FromDate AND ToDate) AS SFCG
        //								                ON AD.ShiftSystemID = SFCG.ShiftDefinationID
        //               LEFT JOIN dbo.ShiftDefination SD ON AD.ShiftSystemID = SD.SystemID
        //               LEFT JOIN dbo.AttdnRawData ARIN ON AD.InTimeRowID = ARIN.RowID
        //               LEFT JOIN dbo.AttdnRawData AROUT ON AD.OutTimeRowID = AROUT.RowID
        //                                    LEFT JOIN
        //				(
        //				SELECT LogDownLoadNum
        //				,MIN(ptime) ptime
        //				FROM AttdnRawData
        //				WHERE pdate='" + WDate + @"' AND PType='IN'--and LogDownLoadNum='1800004'
        //				GROUP BY LogDownLoadNum
        //				) LIT ON LIT.LogDownLoadNum=E.SystemId
        //                                    LEFT JOIN AttdnRawData ARD ON ARD.LogDownLoadNum=LIT.LogDownLoadNum  AND ARD.PTime=LIT.ptime
        //               " + EntityTables() + @"

        //               WHERE AD.WorkDate  = '" + WDate + @"' AND (ISNULL(E.EmployeeCurrentStatus,'') != 'TBS' or (ISNULL(E.EmployeeCurrentStatus,'') = 'TBS' AND EmployeeCurrentStatusEffectiveDate > '" + WDate + @"'))
        //                                AND E.EmployeeStatus='Active' " + _wj + @"
        //                                  " + wc + @"
        //                        --AND AD.DayStatus <>'W'
        //                    ) A  WHERE PlantID  = '" + sPlantID + @"'";

        //        if (sUnitID != "ALL")
        //        {
        //            strSql = strSql + @" AND UnitID in (" + sUnitID + ")";
        //        }
        //        if (sDivID != "ALL")
        //        {
        //            strSql = strSql + @" AND DivisionID  in (" + sDivID + ")";
        //        }
        //        if (sDepID != "ALL")
        //        {
        //            strSql = strSql + @" AND DepartmentID  in (" + sDepID + ")";
        //        }
        //        if (sSecID != "ALL")
        //        {
        //            strSql = strSql + @" AND SectionID  in (" + sSecID + ")";
        //        }
        //        if (sSubSecID != "ALL")
        //        {
        //            strSql = strSql + @" AND SubSectionID  in (" + sSubSecID + ")";
        //        }
        //        if (sLineID != "ALL")
        //        {
        //            strSql = strSql + @" AND LineID in (" + sLineID + ")";
        //        }

        //        if (sDesigGrpID != "ALL")
        //        {
        //            strSql = strSql + @" AND DesignationGroupID  in (" + sDesigGrpID + ")";
        //        }
        //        if (sDesigID != "ALL")
        //        {
        //            strSql = strSql + @" AND GivenDesignationId  in (" + sDesigID + ")";
        //        }
        //        if (sEmpCatID != "ALL")
        //        {
        //            strSql = strSql + @" AND EmpCategoryID  in (" + sEmpCatID + ")";
        //        }

        //        strSql = strSql + @"
        //                GROUP BY  EmployeeCode
        //                        ,EmployeeCodeNumeric
        //                     , EmployeeName
        //                     , DOJ
        //                     , DesignationGroupID
        //                     , UnitID
        //                     , DivisionID
        //                     , DepartmentID
        //                     , SectionID
        //                     , SubSectionID
        //                     , LineID
        //                     , EmpCategoryID
        //                        ,dti,dto
        //                     , PDate
        //                     , DayStatus
        //                     , InTime
        //                        ,LeastPunchTime
        //                     , InDeviceID
        //                     , OutTime
        //                     , OutDeviceID
        //                     , OTHr
        //                     , ShiftChangeInTime
        //                        ,ShiftName
        //                        ,ShiftInTimeShow
        //                        ,InTimeShow
        //                        ,OutTimeShow
        //                     , ShiftInTime
        //                     , PlantID
        //                        ,LDesignation
        //                        " + EntityAlias() + @"
        //                ORDER BY EmployeeCodeNumeric";

        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function


        public string EntityTables()
        {
            return @"                       LEFT JOIN org.Unit U ON E.UnitID = U.Id
                                            LEFT JOIN org.Division Dv ON E.DivisionID = Dv.Id
                                            LEFT JOIN org.SubDivision SubDv ON E.SubdivisionID = SubDv.Id
                                            LEFT JOIN org.Department Dp ON E.DepartmentID = Dp.Id
                                            LEFT JOIN org.Section S ON E.SectionID = S.Id
                                            LEFT JOIN org.SubSection SB ON E.SubSectionID = SB.Id
                                            LEFT JOIN org.Line L ON E.LineID = L.Id
                                            LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                                            LEFT JOIN hkp.Designation D ON E.DesignationSystemID = D.Id
                                            LEFT JOIN hkp.Designation GVD ON E.GivenDesignationId = GVD.Id
                                            LEFT JOIN HKP.LegalDesignation LD on LD.Id=E.LegalDesignationId
                                            LEFT JOIN
                                            --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
                                            (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
											LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
											)EC ON EC.DesignationId=E.GivenDesignationId";
        }
        public string EntityAlias()
        {
            return @"   , DesignationGroup
	                    , Designation
                        , GivenDesignation
		                , EmpCategory
		                , Line
		                , SubSection
		                , Section
		                , Department
		                , Division
		                , Unit ";
        }

        public string EntityColumns()
        {
            return @" 	, DG.UserName DesignationGroup
		                , D.UserName Designation
		                , GVD.UserName GivenDesignation
		                , L.UserName Line
		                , U.UserName Unit                       
		                , Dv.UserName Division
                        , SubDv.UserName SubDivision
		                , Dp.UserName Department
		                , S.UserName Section
		                , SB.UserName SubSection
		                , EC.UserName AS EmpCategory ";
        }

    }
}
