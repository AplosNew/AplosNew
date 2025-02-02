using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Attendances.Controllers
{
    public class DailyAttendanceInformationController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMonthlyAttendanceInformation _monthlyAttendanceInformation;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private DataSet dsRef;
        private object workbook;
        private object objRpt;
        private object excelEngine;
        private object application;

        public DailyAttendanceInformationController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository,
            IMonthlyAttendanceInformation monthlyAttendanceInformation
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _monthlyAttendanceInformation = monthlyAttendanceInformation;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages


        #region -- Operations
        [HttpPost, Authorize]
        public ActionResult XlsDepWiseAttnReport(string effectiveDate, Dictionary<string, string> parameters)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var fileName = "DailyAttendanceInformation" + DateTime.Now.ToString("yyMMdd") + identity.Name + ".xls";
                string fullPath = System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName;


                var workbook = GetXlsDailyAttendanceSummaryReport(effectiveDate,parameters);
                workbook.Version = ExcelVersion.Excel97to2003;
                workbook.SaveAs(fullPath);

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        public IWorkbook GetXlsDailyAttendanceSummaryReport(string effectiveDate, Dictionary<string, string> parameters)
        {
            #region Variable

            clsReport objRpt = null;
            clsReport objRptD = null;

            DataSet dsHeading = null;

            DataSet dsAttn = null;
            DataView dvAttn = null;

            DataSet dsCmp = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string sOfficeInTime = "00:00:00";
            string sInTime = "00:00:00";
            var report = new ReportUtility();

            DataSet dsExtraAbsent = null;
            DataView dvExtraAbsent = null;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objRpt = new clsReport();

                var ob = new clsStaticInfo();

                objRpt.GetExtraAbsentForDaily(identity.PlantId, effectiveDate, out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);
                bool IsExtraAbsent = false;
                #region Variable


                ParaAttendanceReport op = new global::ParaAttendanceReport();
                op.PlantId = identity.PlantId;
                op.ADate = effectiveDate;
                #endregion Variable

                if (string.IsNullOrEmpty(effectiveDate.Trim()) == true || bplib.clsWebLib.IsDateOK(effectiveDate.Trim()) == false)
                {
                    Exception ex = new Exception("Please define Attendance Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }

                #region DataSet

                objRpt.GetDailyAttnRptData(op, parameters, out dsAttn);
                dvAttn = new DataView();
                dvAttn.Table = dsAttn.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId.Trim(), out dsCmp);

                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 7;
                    int intRow = 0;

                    string strSubSec = "0";
                    string strSec = "0";
                    string strUnit = "0";
                    int strCount = 0;
                    string strLateBy = "00:00:00";

                    for (int i = 0; i <= dvAttn.Count - 1; i++)
                    {

                        xlsCol = 1;

                        if ((string.Compare(strSubSec.ToUpper(), dvAttn[i]["SubSectionID"].ToString().Trim().ToUpper())) != 0
                            || (string.Compare(strSec.ToUpper(), dvAttn[i]["SectionID"].ToString().Trim().ToUpper())) != 0
                            || (string.Compare(strUnit.ToUpper(), dvAttn[i]["UnitID"].ToString().Trim().ToUpper())) != 0)
                        {
                            xlsRow += intRow;
                            intRow = 1;
                            strCount = 0;

                            sheet1.Range[xlsRow, 1].Text = "Unit :-" + dvAttn[i]["Unit"].ToString();
                            sheet1.Range[xlsRow, 1, xlsRow, 3].Merge();
                            sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow, 1, xlsRow, 3].RowHeight = 21;
                            sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, 4].Text = "Section :-" + dvAttn[i]["Section"].ToString();
                            sheet1.Range[xlsRow, 4, xlsRow, 6].Merge();
                            sheet1.Range[xlsRow, 4].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow, 4, xlsRow, 6].RowHeight = 21;
                            sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow + 1, 1].Text = "Sub Section :-" + dvAttn[i]["SubSection"].ToString();
                            sheet1.Range[xlsRow + 1, 1, xlsRow + 1, 3].Merge();
                            sheet1.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow + 1, 1].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow + 1, 1, xlsRow + 1, 3].RowHeight = 21;
                            sheet1.Range[xlsRow + 1, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsRow += 2;

                            #region ------------------Column Header------------------
                            xlsCol = 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Sl No.";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 4.70;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8.50;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 39;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Shift Name";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Shift InTime";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Shift OutTime";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Least Punch Time";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 11;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "InTime";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "OutTime";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Day Status";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Late By";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Duration";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Short Leave";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Leave Type";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                            endXlsCol = xlsCol;
                            xlsCol = 1;
                            xlsRow += 1;
                            #endregion ------------------Column Header------------------
                        }
                        strSubSec = dvAttn[i]["SubSectionID"].ToString().Trim();//SubSectionID
                        strSec = dvAttn[i]["SectionID"].ToString().Trim();
                        strUnit = dvAttn[i]["UnitID"].ToString().Trim();
                        var sysid = dvAttn[i]["EmpSystemId"].ToString().Trim();


                        if (strSubSec.ToUpper() == "GENERAL")
                        {

                        }
                        #region ----------------------Data-----------------------

                        dvExtraAbsent.RowFilter = "EmpSystemID='" + sysid + "' and WorkingDate='" + op.ADate + "'";
                        if (dvExtraAbsent.Count > 0)
                        {
                            IsExtraAbsent = true;
                        }

                        strCount += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = strCount;
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeCode"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeName"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftName"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftInTimeShow"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftOutTimeShow"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["LeastPunchTime"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["InTimeShow"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualInTime"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["OutTimeShow"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualOutTime"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        xlsCol += 1;

                        //IsManualInTime
                        //IsManualOutTime
                        //IsManualDayStatus

                        if (dvAttn[i]["DayStatus"].ToString().Trim() == "L")
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Blue;
                            sheet1.Range[xlsRow, xlsCol].Text = "P";
                        }
                        else
                        {
                            sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["DayStatus"].ToString().Trim();
                        }
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualDayStatus"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        if (IsExtraAbsent)
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Red;
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                            IsExtraAbsent = false;
                        }

                        xlsCol += 1;

                        if (dvAttn[i]["employeecode"].ToString().Trim() == "1215")
                        {

                        }

                        

                        //  Aplos: Mizanur, 11:47 AM
                        if (dvAttn[i]["DayStatus"].ToString().Trim() == "L")
                        {

                            #region Late by min

                            sInTime = "00:00:00";
                            if (dvAttn[i]["InTime"].ToString().Trim() != "")
                            {
                                sInTime = dvAttn[i]["InTime"].ToString().Trim() + ":00";
                            }
                            else
                            {
                                if (dvAttn[i]["OutTime"].ToString().Trim() != "")
                                {
                                    sInTime = dvAttn[i]["OutTime"].ToString().Trim() + ":00";
                                }
                            }
                            sOfficeInTime = "00:00:00";
                            strLateBy = "00:00";
                            if (dvAttn[i]["ShiftInTime"].ToString().Trim() != "" && sInTime != "00:00:00")
                            {
                                sOfficeInTime = dvAttn[i]["ShiftInTime"].ToString().Trim() + ":00";
                                //sOfficeInTime = dvLocal[i]["ShiftTime"].ToString().Trim();
                                strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString();
                                //strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                            }




                            #endregion Late by min
                        }
                        else
                        {
                            ///absent by how min

                            #region Absent by how much min

                            if (dvAttn[i]["DayStatus"].ToString().Trim() == "A")
                            {
                                sInTime = "00:00:00";
                                if (dvAttn[i]["InTime"].ToString().Trim() != "")
                                {
                                    sInTime = dvAttn[i]["InTime"].ToString().Trim() + ":00";
                                    sOfficeInTime = "00:00:00";
                                    strLateBy = "00:00";
                                    if (dvAttn[i]["ShiftInTime"].ToString().Trim() != "" && sInTime != "00:00:00")
                                    {
                                        sOfficeInTime = dvAttn[i]["ShiftInTime"].ToString().Trim() + ":00";
                                        strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                    }
                                }
                                else
                                {
                                    //if (dvAttn[i]["OutTime"].ToString().Trim() != "")
                                    //{
                                    //    sInTime = dvAttn[i]["OutTime"].ToString().Trim() + ":00";
                                    //}
                                    strLateBy = "";
                                }
                            }
                            else
                            {
                                strLateBy = "";
                            }





                            #endregion Absent by how much min
                        }

                        sheet1.Range[xlsRow, xlsCol].Text = strLateBy;
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Blue;
                        //sheet1.Range[xlsRow, cDOC].CellStyle.Font.Color = ExcelKnownColors.Red;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        string dti = dvAttn[i]["dti"].ToString().Trim();
                        string dto = dvAttn[i]["dto"].ToString().Trim();
                        string _InTimeShow = dvAttn[i]["InTimeShow"].ToString().Trim();
                        string _OutTimeShow = dvAttn[i]["OutTimeShow"].ToString().Trim();

                        sheet1.Range[xlsRow, xlsCol].Text = GetDuration(dti, dto, _InTimeShow, _OutTimeShow);
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvAttn[i]["ShortLeave"].ToString().Trim()));
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["LeaveType"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsRow += 1;




                        #endregion ----------------------Data-----------------------

                        #region Line Setup
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].WrapText = true;
                        #endregion
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
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Daily Attendance Report";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Attendance Date:- " + effectiveDate;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A10"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 9;
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    //sheet1.PageSetup.PrintTitleRows = "$1:$2";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId.Trim() + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                    sheet1.Name = "Daily Attendance Information";
                    #endregion             

                    workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                    return workbook;
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

            }
        }


        [HttpGet, Authorize]
        public ActionResult XlsDepWiseAttnRpt(string effectiveDate, string empParameters /*Dictionary<string, string> empParameters*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Daily Attandance Info" + DateTime.Now.ToString("yyMMdd") + ".xls";
            IWorkbook workbook;
            try
            {
                workbook = XlsDailyAttendanceSummaryReport(effectiveDate, new JavaScriptSerializer().Deserialize<string[]>(empParameters));
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
            workbook.SaveAs(reportFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
            workbook.Close();
            return null;

        }

        [HttpGet, Authorize]
        public ActionResult XlsDepWiseAttnRptView(string effectiveDate, string empParameters /*Dictionary<string, string> empParameters*/)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = "Daily Attandance Info";
            IWorkbook workbook;
            try
            {
                workbook = XlsDailyAttendanceSummaryReport(effectiveDate, new JavaScriptSerializer().Deserialize<string[]>(empParameters));
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
            return RenderReportAsPdf(workbook, reportFileName);


        }


        [HttpPost, Authorize]
        public ActionResult GetEmpInfo(string effectiveDate, string salaryProcessId, bool isActive, bool isSeperated, bool isMaternity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var wcPayrollGroup = "";
            var wcSalaryProcess = "";
            var salaryProcessJoin = "";
            var salaryProcessColumn = "";
            var strDOJ = "";
            string param = "";
            string salaryProcessFlag = "";
            string wcEmpStatus = "";
            wcEmpStatus = " Where (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " Where (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR CurrentMonthEmployeeStatus ='Separated'";
                }

            }
            wcEmpStatus += ")";

            param = "E.GroupID='" + identity.CompanyGroupId + "' AND E.CompanyId='" + identity.CompanyId + "' AND E.PlantId='" + identity.PlantId + "'";

            var cmdText = @"SELECT * fROM (  SELECT   dISTINCT        [CheckBoxSelect] = Convert(bit, 'False'),
                                     isnull(e.SystemId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId     
                                    ,ISNULL(e.EmployeeCurrentStatus,'') EmployeeCurrentStatus	
                                    ,isnull(ld.UserName,'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 
									,ISNULL(EmpC.UserName,'') EmployeeCategory
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(Line.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    , CASE WHEN MONTH(DOS) =  MONTH('" + effectiveDate + @"')  AND YEAR(DOS) = YEAR('" + effectiveDate + @"') then 'Separated' else 'Regular' end CurrentMonthEmployeeStatus
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus
                                    
                                    
									,ISNULL(PG.UserName,'') PayRollGroup
                                    ,e.EmployeeCodePreFix,e.EmployeeCodeNumeric
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(e.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName

                                    FROM EmployeeInformation e                                
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId                  
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
			LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=DesM.DesignationGroupId
            LEFT JOIN [HKP].EmployeeCategory EmpC ON EmpC.Id = DesM.EmployeeCategoryId
                                    LEFT OUTER JOIN hkp.Designation dsg on dsg.id=PO.DesignationId
                                    Left outer join MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId
									Left outer join HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                    " + salaryProcessJoin + @"
                                    Left Join [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
									left join [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									left join [HKP].[Bank] bb on bb.Id = ebi.BankSystemID

                                     WHERE " + param + @" " + strDOJ + @"
                                            " + wcPayrollGroup + @"  " + wcSalaryProcess + @"  
                                                    
                                        AND
									(E.DOS IS NULL OR CONVERT(DATE,E.DOS) >= CONVERT(DATE,'" + effectiveDate + @"')) --and Convert(Date,E.DOJ) < CONVERT(DATE,'30-June-2020')  
                                     ) DD " + wcEmpStatus + @" ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";


            var jsondata = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;

        }

        public IWorkbook XlsDailyAttendanceSummaryReport(string effectiveDate, string[] MasterLCList)
        {
            #region Variable

            clsReport objRpt = null;
            clsReport objRptD = null;

            DataSet dsHeading = null;

            DataSet dsAttn = null;
            DataView dvAttn = null;

            DataSet dsCmp = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            string sOfficeInTime = "00:00:00";
            string sInTime = "00:00:00";
            var report = new ReportUtility();

            DataSet dsExtraAbsent = null;
            DataView dvExtraAbsent = null;

            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objRpt = new clsReport();

                var ob = new clsStaticInfo();

                objRpt.GetExtraAbsentForDaily(identity.PlantId, effectiveDate, out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);
                bool IsExtraAbsent = false;
                #region Variable


                ParaAttendanceReport op = new global::ParaAttendanceReport();
                op.PlantId = identity.PlantId;
                op.ADate = effectiveDate;
                #endregion Variable

                if (string.IsNullOrEmpty(effectiveDate.Trim()) == true || bplib.clsWebLib.IsDateOK(effectiveDate.Trim()) == false)
                {
                    Exception ex = new Exception("Please define Attendance Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }

                #region DataSet

                objRpt.GetDailyAttnRpt(op, MasterLCList, out dsAttn);
                dvAttn = new DataView();
                dvAttn.Table = dsAttn.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId.Trim(), out dsCmp);

                #endregion DataSet

                if (dvAttn.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 7;
                    int intRow = 0;

                    string strSubSec = "0";
                    string strSec = "0";
                    string strUnit = "0";
                    int strCount = 0;
                    string strLateBy = "00:00:00";

                    for (int i = 0; i <= dvAttn.Count - 1; i++)
                    {

                        xlsCol = 1;

                        if ((string.Compare(strSubSec.ToUpper(), dvAttn[i]["SubSectionID"].ToString().Trim().ToUpper())) != 0
                            || (string.Compare(strSec.ToUpper(), dvAttn[i]["SectionID"].ToString().Trim().ToUpper())) != 0
                            || (string.Compare(strUnit.ToUpper(), dvAttn[i]["UnitID"].ToString().Trim().ToUpper())) != 0)
                        {
                            xlsRow += intRow;
                            intRow = 1;
                            strCount = 0;

                            sheet1.Range[xlsRow, 1].Text = "Unit :-" + dvAttn[i]["Unit"].ToString();
                            sheet1.Range[xlsRow, 1, xlsRow, 3].Merge();
                            sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow, 1, xlsRow, 3].RowHeight = 21;
                            sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, 4].Text = "Section :-" + dvAttn[i]["Section"].ToString();
                            sheet1.Range[xlsRow, 4, xlsRow, 6].Merge();
                            sheet1.Range[xlsRow, 4].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow, 4].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow, 4, xlsRow, 6].RowHeight = 21;
                            sheet1.Range[xlsRow, 4].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow + 1, 1].Text = "Sub Section :-" + dvAttn[i]["SubSection"].ToString();
                            sheet1.Range[xlsRow + 1, 1, xlsRow + 1, 3].Merge();
                            sheet1.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;
                            sheet1.Range[xlsRow + 1, 1].CellStyle.Font.Size = 12;
                            sheet1.Range[xlsRow + 1, 1, xlsRow + 1, 3].RowHeight = 21;
                            sheet1.Range[xlsRow + 1, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsRow += 2;

                            #region ------------------Column Header------------------
                            xlsCol = 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Sl No.";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 4.70;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Employee Code";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8.50;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Employee Name";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 39;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Shift Name";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 25;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Shift InTime";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Shift OutTime";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Least Punch Time";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 11;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "InTime";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "OutTime";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Day Status";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 8;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Late By";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Duration";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Short Leave";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsCol += 1;
                            sheet1.Range[xlsRow, xlsCol].Text = "Leave Type";
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 7;
                            sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                            sheet1.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                            endXlsCol = xlsCol;
                            xlsCol = 1;
                            xlsRow += 1;
                            #endregion ------------------Column Header------------------
                        }
                        strSubSec = dvAttn[i]["SubSectionID"].ToString().Trim();//SubSectionID
                        strSec = dvAttn[i]["SectionID"].ToString().Trim();
                        strUnit = dvAttn[i]["UnitID"].ToString().Trim();
                        var sysid = dvAttn[i]["EmpSystemId"].ToString().Trim();


                        if (strSubSec.ToUpper() == "GENERAL")
                        {

                        }
                        #region ----------------------Data-----------------------

                        dvExtraAbsent.RowFilter = "EmpSystemID='" + sysid + "' and WorkingDate='" + op.ADate + "'";
                        if (dvExtraAbsent.Count > 0)
                        {
                            IsExtraAbsent = true;
                        }

                        strCount += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = strCount;
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeCode"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["EmployeeName"].ToString().ToUpper();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftName"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftInTimeShow"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["ShiftOutTimeShow"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["LeastPunchTime"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["InTimeShow"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualInTime"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["OutTimeShow"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualOutTime"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        xlsCol += 1;

                        //IsManualInTime
                        //IsManualOutTime
                        //IsManualDayStatus

                        if (dvAttn[i]["DayStatus"].ToString().Trim() == "L")
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Blue;
                            sheet1.Range[xlsRow, xlsCol].Text = "P";
                        }
                        else
                        {
                            sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["DayStatus"].ToString().Trim();
                        }
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        if (bplib.clsWebLib.GetBoolData(dvAttn[i]["IsManualDayStatus"].ToString().Trim()))
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Orange;
                        }
                        if (IsExtraAbsent)
                        {
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Red;
                            sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                            IsExtraAbsent = false;
                        }

                        xlsCol += 1;

                        if (dvAttn[i]["employeecode"].ToString().Trim() == "1215")
                        {

                        }

                        #region Late by commented
                        ////if (dvAttn[i]["DayStatus"].ToString().Trim() == "L")
                        ////{
                        ////    #region Late by min
                        ////    sInTime = "00:00:00";
                        ////    if (dvAttn[i]["InTime"].ToString().Trim() != "")
                        ////    {
                        ////        sInTime = dvAttn[i]["InTime"].ToString().Trim() + ":00";
                        ////    }
                        ////    else
                        ////    {
                        ////        if (dvAttn[i]["OutTime"].ToString().Trim() != "")
                        ////        {
                        ////            sInTime = dvAttn[i]["OutTime"].ToString().Trim() + ":00";
                        ////        }
                        ////    }
                        ////    sOfficeInTime = "00:00:00";
                        ////    strLateBy = "00:00";
                        ////    if (dvAttn[i]["ShiftTime"].ToString().Trim() != "")
                        ////    {
                        ////        sOfficeInTime = dvAttn[i]["ShiftTime"].ToString().Trim() + ":00";
                        ////        strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                        ////    } 
                        ////    #endregion
                        ////}
                        ////else
                        ////{
                        ////    ///absent by how min
                        ////    #region Absent by how much min
                        ////    if (dvAttn[i]["DayStatus"].ToString().Trim() == "A")
                        ////    {
                        ////        sInTime = "00:00:00";
                        ////        if (dvAttn[i]["InTime"].ToString().Trim() != "")
                        ////        {
                        ////            sInTime = dvAttn[i]["InTime"].ToString().Trim() + ":00";
                        ////            sOfficeInTime = "00:00:00";
                        ////            strLateBy = "00:00";
                        ////            if (dvAttn[i]["ShiftTime"].ToString().Trim() != "")
                        ////            {
                        ////               // var v=Convert.ToDateTime(dvAttn[i]["ShiftTime"].ToString().Trim())
                        ////                sOfficeInTime = dvAttn[i]["ShiftTime"].ToString().Trim() + ":00";
                        ////                strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                        ////            }
                        ////        }
                        ////        else
                        ////        {
                        ////            strLateBy = "";
                        ////        }

                        ////    }
                        ////    else
                        ////    {
                        ////        strLateBy = "";
                        ////    } 
                        ////    #endregion
                        ////}
                        #endregion

                        //  Aplos: Mizanur, 11:47 AM
                        if (dvAttn[i]["DayStatus"].ToString().Trim() == "L")
                        {

                            #region Late by min

                            sInTime = "00:00:00";
                            if (dvAttn[i]["InTime"].ToString().Trim() != "")
                            {
                                sInTime = dvAttn[i]["InTime"].ToString().Trim() + ":00";
                            }
                            else
                            {
                                if (dvAttn[i]["OutTime"].ToString().Trim() != "")
                                {
                                    sInTime = dvAttn[i]["OutTime"].ToString().Trim() + ":00";
                                }
                            }
                            sOfficeInTime = "00:00:00";
                            strLateBy = "00:00";
                            if (dvAttn[i]["ShiftInTime"].ToString().Trim() != "" && sInTime != "00:00:00")
                            {
                                sOfficeInTime = dvAttn[i]["ShiftInTime"].ToString().Trim() + ":00";
                                //sOfficeInTime = dvLocal[i]["ShiftTime"].ToString().Trim();
                                strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString();
                                //strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                            }




                            #endregion Late by min
                        }
                        else
                        {
                            ///absent by how min

                            #region Absent by how much min

                            if (dvAttn[i]["DayStatus"].ToString().Trim() == "A")
                            {
                                sInTime = "00:00:00";
                                if (dvAttn[i]["InTime"].ToString().Trim() != "")
                                {
                                    sInTime = dvAttn[i]["InTime"].ToString().Trim() + ":00";
                                    sOfficeInTime = "00:00:00";
                                    strLateBy = "00:00";
                                    if (dvAttn[i]["ShiftInTime"].ToString().Trim() != "" && sInTime != "00:00:00")
                                    {
                                        sOfficeInTime = dvAttn[i]["ShiftInTime"].ToString().Trim() + ":00";
                                        strLateBy = (Convert.ToDateTime(sInTime) - Convert.ToDateTime(sOfficeInTime)).ToString().Substring(0, 5);
                                    }
                                }
                                else
                                {
                                    //if (dvAttn[i]["OutTime"].ToString().Trim() != "")
                                    //{
                                    //    sInTime = dvAttn[i]["OutTime"].ToString().Trim() + ":00";
                                    //}
                                    strLateBy = "";
                                }
                            }
                            else
                            {
                                strLateBy = "";
                            }





                            #endregion Absent by how much min
                        }

                        sheet1.Range[xlsRow, xlsCol].Text = strLateBy;
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Color = ExcelKnownColors.Blue;
                        //sheet1.Range[xlsRow, cDOC].CellStyle.Font.Color = ExcelKnownColors.Red;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        string dti = dvAttn[i]["dti"].ToString().Trim();
                        string dto = dvAttn[i]["dto"].ToString().Trim();
                        string _InTimeShow = dvAttn[i]["InTimeShow"].ToString().Trim();
                        string _OutTimeShow = dvAttn[i]["OutTimeShow"].ToString().Trim();

                        sheet1.Range[xlsRow, xlsCol].Text = GetDuration(dti, dto, _InTimeShow, _OutTimeShow);
                        sheet1.Range[xlsRow, xlsCol].RowHeight = 13;
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvAttn[i]["ShortLeave"].ToString().Trim()));
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dvAttn[i]["LeaveType"].ToString().Trim();
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsRow += 1;




                        #endregion ----------------------Data-----------------------

                        #region Line Setup
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].WrapText = true;
                        #endregion
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
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Daily Attendance Report";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Attendance Date:- " + effectiveDate;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes
                    sheet1.IsDisplayZeros = false;
                    sheet1.UsedRange["A10"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 9;
                    #endregion

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    //sheet1.PageSetup.PrintTitleRows = "$1:$2";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.UserId.Trim() + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                    sheet1.Name = "Daily Attendance Information";
                    #endregion             

                    workbook.Version = ExcelVersion.Excel97to2003;
                    report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);
                    return workbook;
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

            }
        }
        string GetDuration(string dti, string dto, string intime, string outtime)
        {
            string res = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(intime) == false && string.IsNullOrEmpty(outtime) == false)
                {
                    string vintime = Convert.ToDateTime(intime).ToString("HH:mm:ss");
                    string vouttime = Convert.ToDateTime(outtime).ToString("HH:mm:ss");
                    var x = (Convert.ToDateTime(dto) - (Convert.ToDateTime(dti)));
                    res = x.ToString().Substring(0, 5);
                }
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion -- Operations  
    }
}