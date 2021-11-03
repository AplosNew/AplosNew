using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.HumanResources;
using Library.Model.Setups;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;
using static Library.Service.HumanResources.PayRegisterBDReportService;
using static Library.Service.Enums.SalaryHeadEnum;
using Library.Service.Enums;
using Syncfusion.ExcelToPdfConverter;
using System.Collections.Specialized;
using System.Text;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class MonthlyAttendanceInformationReportController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;



        public MonthlyAttendanceInformationReportController(
              ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages     

        [HttpGet, Authorize]
        public ActionResult XlsMonthlyAttendanceInformation(string year, string month, string rbStatus,bool chkAdditionInfo, string parameterString)
        {
            #region Variable
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            clsReport objRpt = null;

            DataSet dsHeading = null;

            DataSet dsMonthlyAttnSumm = null;
            DataView dvMonthlyAttnSumm = null;

            //DataSet dsMonthlyAttnInfo = null;
            DataSet dsDaily = null;
            DataTable dtDaily = null;
            DataView dvDaily = null;

            DataSet dsDailyRaw = null;
            DataTable dtDailyRaw = null;
            DataView dvDailyRaw = null;

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

            DataSet dsSLeave = null;
            DataView dvSLeave = null;
            DataTable dtSLeave = null;


            #endregion Variable

            try
            {
                objRpt = new clsReport();

                #region Validation

             
                string m = bplib.clsWebLib.GetMonthName(month);
                dtFrmDt = Convert.ToDateTime("01-" + m + "-" + year);
                //dtFrmDt = Convert.ToDateTime(this.ddlMonthNo.Text.Trim() + "/" + "01/" + this.ddlYearNo.SelectedItem.Text.Trim());
                string monthName = dtFrmDt.ToString("MMMM");
                DateTime dateForTheMonth = Convert.ToDateTime("01-" + m + "-" + year);

                if (Convert.ToInt32(DateTime.Now.Month) != Convert.ToInt32(month))
                {
                    dtEndDate = dtFrmDt.AddMonths(1).AddDays(-1);
                }           

                #endregion Validation

                #region Variable

                ParaMontlyAttendance objm = new global::ParaMontlyAttendance();





                objm.AMonth = month;
                objm.AYear =year;
                objm.FDate = dtFrmDt.ToString("dd-MMM-yyyy");
                objm.TDate = dtEndDate.ToString("dd-MMM-yyyy");

                #endregion Variable


                #region DataSet --Detail Attendance Data with Header

                objRpt.GetMonthlyAttnSummaryRptForDetails(objm, out dsMonthlyAttnSumm);
                dvMonthlyAttnSumm = new DataView();
                dvMonthlyAttnSumm.Table = dsMonthlyAttnSumm.Tables[0];

                string _FLAG = "DAYSTATUS";

                if (rbStatus == "DAYSTATUS")
                {
                    _FLAG = "DAYSTATUS";
                }
                else if (rbStatus == "INTIME")
                {
                    _FLAG = "INTIME";
                }
                else if (rbStatus== "OUTTIME")
                {
                    _FLAG = "OUTTIME";
                }
                else if (rbStatus == "INRAW")
                {
                    _FLAG = "INRAW";
                }
                else if (rbStatus == "OUTRAW")
                {
                    _FLAG = "OUTRAW";
                }
                else if (rbStatus == "ALLSTATUS")
                {
                    _FLAG = "ALLSTATUS";
                }
                else
                {
                    _FLAG = "DAYSTATUS";
                }

                if (_FLAG == "INRAW" || _FLAG == "OUTRAW")
                {
                    objRpt.GetMonthlyIntimeOutTimeRaw(_FLAG, objm, out dsDaily);
                }
                else
                {
                    objRpt.GegMonthlyDaily(_FLAG, objm, out dsDaily);
                }

                if (dsDaily.Tables.Count > 0)
                {
                    if (dsDaily.Tables[0].Rows.Count > 0)
                    {
                        dtDaily = dsDaily.Tables[0];
                    }
                }
                else
                {
                    throw new Exception("Data not found.");
                }
                dvDaily = new DataView();

                DataSet dsOUTTime = null;
                DataView dvOUTTime = null;
                objRpt.GegMonthlyDaily("OUTTIME", objm, out dsOUTTime);
                dvOUTTime = new DataView(dsOUTTime.Tables[0]);

                DataSet dsManual = null;
                DataView dvManual = null;
                objRpt.GegMonthlyDaily("MANUAL", objm, out dsManual);
                dvManual = new DataView(dsManual.Tables[0]);

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(identity.PlantId, dtFrmDt.Month, dtEndDate.Year, out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                DataSet dsLeaveHalf = null;
                DataView dvLeaveHalf = null;
                objRpt.GetHalfLeaveInfo(identity.PlantId, dtFrmDt.ToString("dd-MMM-yyyy"), dtEndDate.ToString("dd-MMM-yyyy"), out dsLeaveHalf);
                dvLeaveHalf = new DataView(dsLeaveHalf.Tables[0]);

                DataSet dsDayType = null;
                DataView dvDayType = null;
                objRpt.GetDayType(out dsDayType);
                dvDayType = new DataView(dsDayType.Tables[0]);


                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                dsSLeave = new DataSet();
                objRpt.GetShortLeave(objm, out dsSLeave);
                dvSLeave = new DataView(dsSLeave.Tables[0]);

                #endregion DataSet

                if (dvMonthlyAttnSumm.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;

                    xlsRow = 5;

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
                    int iExtraAbs = 0;
                    int iTsl = 0;

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
                    iDesig = xlsCol;
                    sheet1.Range[xlsRow, iDesig].Text = "Designation";
                    sheet1.Range[xlsRow, iDesig].ColumnWidth = 15;
                    sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, iDesig, xlsRow + 1, iDesig].Merge();

                    List<SwapColumn> _list2 = GetColDisplayName(dsDaily);
                    xlsCol = iDesig;
                    while (dtFrmDt <= dtEndDate)
                    {
                        xlsCol += 1;
                        sheet1.Range[xlsRow, xlsCol].Text = dtFrmDt.ToString("dd");
                        //xlsRow++;
                        sheet1.Range[xlsRow + 1, xlsCol].Text = dtFrmDt.ToString("ddd");

                        if (rbStatus.ToUpper() == "DAYSTATUS")
                        {
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 2.5;
                        }
                        else
                        {
                            sheet1.Range[xlsRow, xlsCol].ColumnWidth = 5;
                        }
                        sheet1.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        var ob = _list2.Find(r => r.ValueMember == dtFrmDt.ToString("dd"));
                        if (ob != null)
                        {
                            ob.ColIndex = xlsCol;
                        }//if
                        dtFrmDt = dtFrmDt.AddDays(1);
                    }
                    xlsRow++;
                    if (chkAdditionInfo == true)
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
                        iTsl = xlsCol;
                        sheet1.Range[xlsRow - 1, iTsl].Text = "Short Leave";
                        sheet1.Range[xlsRow - 1, iTsl].ColumnWidth = 7.20;
                        sheet1.Range[xlsRow - 1, iTsl].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow - 1, iTsl].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet1.Range[xlsRow - 1, iTsl, xlsRow, iTsl].Merge();

                    }

                    #endregion ------------------Details Header-------------------------

                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    endXlsCol = xlsCol;
                    xlsCol = 1;
                    xlsRow += 1;
                    int _StartRow = xlsRow;
                    #endregion ------------------Column Header------------------

                    dvDaily.Table = dtDaily;



                    #region Attendance Summary 
                    for (int i = 0; i <= dvMonthlyAttnSumm.Count - 1; i++)
                    {
                        xlsCol = 1;

                        #region ----------------------Data-----------------------

                        strCount += 1;
                        sheet1.Range[xlsRow, iSrNo].Number = strCount;
                        sheet1.Range[xlsRow, iSrNo].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                        sheet1.Range[xlsRow, iSrNo].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iEmpCode].Text = dvMonthlyAttnSumm[i]["EmployeeCode"].ToString().Trim();
                        sheet1.Range[xlsRow, iEmpCode].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, iEmpCode].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iEmpName].Text = dvMonthlyAttnSumm[i]["EmployeeName"].ToString().ToUpper();
                        sheet1.Range[xlsRow, iEmpName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, iEmpName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iDOJ].Text = dvMonthlyAttnSumm[i]["DOJ"].ToString().Trim();
                        sheet1.Range[xlsRow, iDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, iDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iDOS].Text = dvMonthlyAttnSumm[i]["DOS"].ToString().Trim();
                        sheet1.Range[xlsRow, iDOS].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, iDOS].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iUnit].Text = dvMonthlyAttnSumm[i]["Unit"].ToString().Trim();
                        sheet1.Range[xlsRow, iUnit].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, iUnit].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iDepart].Text = dvMonthlyAttnSumm[i]["Department"].ToString().Trim();
                        sheet1.Range[xlsRow, iDepart].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, iDepart].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        sheet1.Range[xlsRow, iSec].Text = dvMonthlyAttnSumm[i]["Section"].ToString().Trim();
                        sheet1.Range[xlsRow, iSec].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, iSec].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        sheet1.Range[xlsRow, iSubSection].Text = dvMonthlyAttnSumm[i]["SubSection"].ToString().Trim();
                        sheet1.Range[xlsRow, iSubSection].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, iSubSection].VerticalAlignment = ExcelVAlign.VAlignCenter;


                        sheet1.Range[xlsRow, iDesig].Text = dvMonthlyAttnSumm[i]["LegalDG"].ToString().Trim();
                        sheet1.Range[xlsRow, iDesig].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, iDesig].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        string _m = bplib.clsWebLib.GetMonthName(month);
                        dtFrmDt = Convert.ToDateTime("01-" + _m + "-" + year);
                        xlsCol = iDesig;
                        string ecode = dvMonthlyAttnSumm[i]["EmployeeCode"].ToString().Trim();
                        string _SystemId = dvMonthlyAttnSumm[i]["EmployeePK"].ToString().Trim();
                        if (_SystemId == "1800815")
                        {

                        }

                        //int _CountLateNominal= 0;
                        while (dtFrmDt <= dtEndDate)
                        {

                            xlsCol += 1;
                            var sc = _list2.Find(r => r.ValueMember == dtFrmDt.ToString("dd"));
                            //list.Find(x => x.Id == IdToFind);
                            //_Date_from_head = sc.DisplayMember + "-" + ddlMonthNo.Items[ddlMonthNo.SelectedIndex].Text + "-" + ddlYearNo.Items[ddlYearNo.SelectedIndex].Text;
                            #region OUTTime
                            dvOUTTime.RowFilter = "EmployeePK = '" + _SystemId + "' ";
                            dvManual.RowFilter = "EmployeePK = '" + _SystemId + "' ";

                            #endregion

                            dvDaily.RowFilter = "EmployeePK = '" + _SystemId + "' ";

                            if (dvDaily.Count > 0)
                            {
                                bool HasOUTtime = true;
                                bool IsHalfLeave = false;
                                bool IsManual = false;
                                bool IsExtraAbsent = false;
                                bool IsShortLeave = false;
                                var _day_status = "";
                                var _day_status_modified = "";

                                var _col_index = xlsCol;
                                if (sc != null)
                                {
                                    _day_status = dvDaily[0][sc.DisplayMember].ToString().Trim().Replace(",", Environment.NewLine);
                                    _day_status_modified = _day_status;
                                    if (_day_status.Contains("LV"))
                                    {
                                        _day_status_modified = _day_status.Remove(0, 3);
                                    }
                                    _col_index = sc.ColIndex;

                                    if (_FLAG == "ALLSTATUS")
                                    {
                                        sheet1.Range[xlsRow, _col_index].Text = _day_status;
                                        sheet1.Range[xlsRow, _col_index].RowHeight = 52;

                                    }
                                    dvSLeave.RowFilter = "EmployeeSystemID='" + _SystemId + "' and PDate='" + sc.DisplayMember + "'";
                                    if (dvSLeave.Count > 0)
                                    {
                                        IsShortLeave = true;
                                    }


                                    dvExtraAbsent.RowFilter = "EmpSystemID='" + _SystemId + "' and WorkingDate='" + sc.DisplayMember + "'";
                                    if (dvExtraAbsent.Count > 0)
                                    {
                                        IsExtraAbsent = true;
                                    }

                                    dvLeaveHalf.RowFilter = "EmpSystemID='" + _SystemId + "' and WorkDate='" + sc.DisplayMember + "'";
                                    if (dvLeaveHalf.Count > 0)
                                    {
                                        IsHalfLeave = true;
                                    }

                                    if (dvOUTTime.Count > 0)
                                    {
                                        string strdaystat = "";
                                        int funn = strdaystat.IndexOf(",");
                                        if (funn != -1)
                                        {
                                            //string[] _split = _day_status.Split(',');
                                            _day_status = _day_status.Substring(0, funn);
                                        }
                                        string s = _day_status;
                                        if (_FLAG == "ALLSTATUS" && !string.IsNullOrEmpty(_day_status))
                                        {
                                            try
                                            {
                                                //s = s.Substring(0, s.IndexOf("\r"));
                                                s = s.Split('\r')[0];
                                            }
                                            catch (Exception ex)
                                            {

                                                throw ex;
                                            }
                                        }
                                        dvDayType.RowFilter = "DayType='" + s + "'";
                                        if (dvDayType.Count > 0)
                                        {
                                            HasOUTtime = !string.IsNullOrEmpty(dvOUTTime[0][sc.DisplayMember].ToString().Trim());
                                        }
                                    }

                                    ///manual
                                    if (dvManual.Count > 0)
                                    {
                                        if (!string.IsNullOrEmpty(dvManual[0][sc.DisplayMember].ToString().Trim()))
                                        {
                                            IsManual = true;
                                        }
                                    }
                                }


                                if (_FLAG == "INTIME" || _FLAG == "OUTTIME")
                                {
                                    if (string.IsNullOrEmpty(_day_status))
                                    {
                                        sheet1.Range[xlsRow, _col_index].Text = _day_status;
                                    }
                                    else
                                    {
                                        IRange range = sheet1.Range[xlsRow, _col_index];
                                        range.Value = Convert.ToDateTime(_day_status).ToString("HH:mm");
                                        range.NumberFormat = "HH:mm";
                                        var v = range.DisplayText;

                                        //sheet1.Range[xlsRow, _col_index].DisplayText = Convert.ToDateTime(_day_status).ToString("HH:mm");
                                    }

                                    sheet1.Range[xlsRow, _col_index].NumberFormat = "HH:mm";
                                }

                                else
                                {
                                    try
                                    {
                                        #region Replacing L with P 

                                        #endregion
                                        if (_day_status_modified.Length > 0)
                                        {

                                            if (_day_status_modified[0].ToString() == "L")
                                            {
                                                StringBuilder sb = new StringBuilder(_day_status_modified);
                                                sb[0] = 'P'; // index starts at 0!
                                                _day_status_modified = sb.ToString();

                                            }

                                        }

                                        sheet1.Range[xlsRow, _col_index].Text = (_day_status_modified);
                                    }
                                    catch (Exception)
                                    {

                                        throw;
                                    }

                                }

                                #region  ----   DayStatus---
                                string vv = string.Empty;
                                if (sc != null)
                                {
                                    vv = dvDaily[0][sc.DisplayMember].ToString().Trim();
                                }

                                int fn = vv.IndexOf(",");
                                if (fn != -1)
                                {
                                    _day_status = vv.Substring(0, fn);
                                }
                                #endregion ----DayStaus---

                                sheet1.Range[xlsRow, _col_index].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                                sheet1.Range[xlsRow, _col_index].VerticalAlignment = ExcelVAlign.VAlignTop;
                                sheet1.Range[xlsRow, _col_index].ColumnWidth = 6;


                                if (!HasOUTtime)
                                {
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Violet;
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.White;
                                }
                                else if (_day_status == "P")
                                {
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Green;
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.White;
                                }
                                else if (_day_status == "A")
                                {
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Red;
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.White;
                                }
                                else if (_day_status == "L" || _day_status == "LVL" || _day_status == "WL" || _day_status == "HL")
                                {
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Blue;
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.White;
                                }
                                else if (_day_status.Contains("LV"))
                                {

                                    sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Yellow;
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.Black;
                                }

                                if (IsManual && !_day_status.Contains("LV"))
                                {
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Interior.Color = System.Drawing.Color.Orange;
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.White;
                                }

                                if (IsHalfLeave)
                                {
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.Yellow;
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Bold = true;
                                }

                                if (IsExtraAbsent)
                                {
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.Red;
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Bold = true;
                                }
                                if (IsShortLeave)
                                {
                                    sheet1.Range[xlsRow, _col_index].CellStyle.Font.Color = ExcelKnownColors.Magenta;
                                }
                            }//if count
                            dtFrmDt = dtFrmDt.AddDays(1);
                        }//date

                        if (chkAdditionInfo == true)
                        {
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

                            double _pay_days = Convert.ToDouble(DaysInaMonth) - (Convert.ToDouble(TotalAbsent) + Convert.ToDouble(TotalLWP) + Convert.ToDouble(_ExtraAbsent));

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

                            sheet1.Range[xlsRow, iTtlLv].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvMonthlyAttnSumm[i]["TotalLv"].ToString().Trim()));
                            sheet1.Range[xlsRow, iTtlLv].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTtlLv].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            var sl = dvMonthlyAttnSumm[i]["ShortLeave"].ToString().Trim();
                            if (sl == "0")
                            {
                                sl = null;
                            }
                            sheet1.Range[xlsRow, iTsl].Text = sl;
                            sheet1.Range[xlsRow, iTsl].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iTsl].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }

                        xlsRow += 1;

                        #endregion ----------------------Data-----------------------


                    }
                    #endregion

                    #region Line Setup
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[_StartRow, 1, xlsRow - 1, endXlsCol].WrapText = true;
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
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].RowHeight = 30;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    //sheet1.Range[xlsRow, 1].CellStyle.Rotation

                    // start color indication  by Mirza
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
                    sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.Snow;

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
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].Merge();
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].RowHeight = 26;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    string _sheetHeaderName = "Monthly Attendance Information(Day Status)";
                    string _sheetHeaderName1 = "Monthly Attendance Information(Intime Attendance Data)";
                    string _sheetHeaderName2 = "Monthly Attendance Information(OutTime Attendance Data)";
                    string _sheetHeaderName3 = "Monthly Attendance Information(Intime Raw Data)";
                    string _sheetHeaderName4 = "Monthly Attendance Information(OutTime Raw Data)";

                    if (rbStatus == "DAYSTATUS")
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = _sheetHeaderName;
                    }
                    else if (rbStatus == "INTIME")
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = _sheetHeaderName1;
                    }
                    else if (rbStatus == "OUTTIME")
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = _sheetHeaderName2;
                    }

                    else if (rbStatus == "INRAW")
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = _sheetHeaderName3;
                    }
                    else if (rbStatus == "OUTRAW")
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = _sheetHeaderName4;
                    }
                    else
                    {
                        sheet1.Range[xlsRow, xlsCol].Text = _sheetHeaderName;
                    }
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].Merge();
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1].CellStyle.Font.Size = 11;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol - 5].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Year No:- " + year + " and Month:- " + dateForTheMonth.ToString("MMMM");
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
                    sheet1.UsedRange["A8"].FreezePanes();
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
                    sheet1.IsDisplayZeros = false;

                    sheet1.Name = "Monthly Attendance Information";
                    #endregion

                    workbook.Version = ExcelVersion.Excel2013;
                    string strFileName = "MonthlyAttendanceInformation::"+month+"-"+year + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xlsx";
                    workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                    workbook.Close();
                    excelEngine.Dispose();
                }
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //objStatic = null;

                objRpt = null;
                dsHeading = null;

                excelEngine = null;
                application = null;
                workbook = null;
            }

        }//End Function

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