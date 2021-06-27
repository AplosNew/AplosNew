#region Using
using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Attendance;
using Library.HumanResource.Payroll.Allowance;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class AuditReportSummeryController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public AuditReportSummeryController(
              ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region ---Audit Report Summery---

        [HttpPost, Authorize]
        public JsonResult AuditReportSummery(string workDate)
        {
            try
            {
                string fileName = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                fileName = DailyStatusCountReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, "Daily Status Count", "", workDate);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string DailyStatusCountReport(string CGId, string CompanyId, string PlantId, string SheetName1, string s1, string workDate)
        {
            #region Variable

            clsReport objRpt = null;
            clsAuditReportSummery obj = null;
            ReportUtility oru = new ReportUtility();
            string yot = string.Empty;//OTConsiderOn
            string tot = string.Empty;//OTConsiderOn

            DataSet dsFactory = null;
            DataSet dsCmp = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1;
            int endXlsCol = 1;
            string FactoryName = "";
            string CmpName = "";
            var report = new ReportUtility();
            var isl = 0;
            var iReportName = 0;
            var iCount = 0;
            var iTotal = 0;

            DataSet dsBioDvAC = null;
            DataSet dsOnlyOt = null;
            DataSet dsAbsent = null;
            DataSet dsPlant = null;
            DataSet dsAbsentWithPunch = null;
            DataSet dsShortDurationAbsent = null;
            DataSet dsLeaveWithPunch = null;
            DataSet dsOTEntitledWithOutMissing = null;
            DataSet dsOTNotEntitledWithOutMissing = null;
            DataSet dsUnApprovedProfile = null;
            DataSet dsProfileNoSalary = null;
            DataSet dsNoSalaryStructureApprove = null;
            DataSet dsWorkDuration = null;
            DataSet dsOtNotConfirmOverstayReport = null;
            DataSet dsLongAbsentisom = null;
            DataSet dsTBS = null;
            DataSet dsMaternityLeave = null;
            DataSet dsBankRemarks = null;
            DataSet dsAttendanceNotLock = null;
            DataSet dsAttendanceNotLockPlant = null;
            DataSet dsTotalAbsent = null;
            DataSet dsLongAtbsPlantSetting = null;
            DataSet dsNotInLegalDesignationMaster = null;
            DataSet dsSalaryNotApproved = null;
            DataSet dsSeparatedAbsent = null;
            DataSet dsOffdayMissingPunch = null;
            DataSet dsOffdayWithPunch = null;
            DataSet dsAbsentWithRawPunch = null;
            DataSet dsShiftUnassign = null;

            DataTable dtBioDvAC = null;
            DataTable dtOnlyOt = null;
            DataTable dtPlant = null;
            DataTable dtAbsent = null;
            DataTable dtAbsentWithPunch = null;
            DataTable dtShortDurationAbsent = null;
            DataTable dtLeaveWithPunch = null;
            DataTable dtOTEntitledWithOutMissing = null;
            DataTable dtOTNotEntitledWithOutMissing = null;
            DataTable dtUnApprovedProfile = null;
            DataTable dtProfileNoSalary = null;
            DataTable dtNoSalaryStructureApprove = null;
            DataTable dtWorkDuration = null;
            DataTable dtOtNotConfirmOverstay = null;
            DataTable dtLongAbsentisom = null;
            DataTable dtTBS = null;
            DataTable dtMaternityLeave = null;
            DataTable dtBankRemarks = null;
            DataTable dtAttendanceNotLock = null;
            DataTable dtAttendanceNotLockPlant = null;
            DataTable dtTotalAbsent = null;
            DataTable dtLongAtbsPlantSetting = null;
            DataTable dtNotInLegalDesignationMaster = null;
            DataTable dtSalaryNotApproved = null;
            DataTable dtSeparatedAbsent = null;
            DataTable dtOffdayMissingPunch = null;
            DataTable dtOffdayWithPunch = null;
            DataTable dtAbsentWithRawPunch = null;
            DataTable dtShiftUnassign = null;

            DataView dvPlant = null;
            #endregion Variable

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string companyId = identity.CompanyId;
                objRpt = new clsReport();
                obj = new clsAuditReportSummery();
                var ob = new clsStaticInfo();

                #region Variable


                ParaAttendanceReport op = new global::ParaAttendanceReport();
                op.PlantId = identity.PlantId;
                op.ADate = workDate;
                #endregion Variable

                if (string.IsNullOrEmpty(workDate.Trim()) == true || bplib.clsWebLib.IsDateOK(workDate.Trim()) == false)
                {
                    Exception ex = new Exception("Please define Attendance Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }

                #region DataSet

                try
                {
                    obj.GetPlant(companyId, CGId, out dsPlant);
                    dtPlant = dsPlant.Tables[0];
                    dvPlant = new DataView();
                    dvPlant.Table = dsPlant.Tables[0];
                }
                catch (Exception ex)
                {

                }

                obj.SelectedPlantWiseCompany(companyId, out dsCmp);
                objRpt.SelectedPlant(PlantId, out dsFactory);
                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 5;
                int intRow = 0;


                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;
                sheet1.Range[xlsRow, isl].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, isl].VerticalAlignment = ExcelVAlign.VAlignCenter;

                xlsCol += 1;
                iReportName = xlsCol;
                sheet1.Range[xlsRow, iReportName].Text = "Report Name";
                sheet1.Range[xlsRow, iReportName].ColumnWidth = 28;
                sheet1.Range[xlsRow, iReportName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iReportName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                Dictionary<string, int> PlantIndex = new Dictionary<string, int>();
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    xlsCol += 1;
                    iCount = xlsCol;
                    sheet1.Range[xlsRow, iCount].Text = dvPlant[i]["PlantName"].ToString();
                    sheet1.Range[xlsRow, iCount].ColumnWidth = 20;
                    sheet1.Range[xlsRow, iCount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, iCount].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    PlantIndex.Add(dvPlant[i]["Id"].ToString(), iCount);
                }

                xlsCol += 1;
                iTotal = xlsCol;
                sheet1.Range[xlsRow, iTotal].Text = "Total";
                sheet1.Range[xlsRow, iTotal].ColumnWidth = 28;
                sheet1.Range[xlsRow, iTotal].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, iTotal].VerticalAlignment = ExcelVAlign.VAlignCenter;

                sheet1.Range[xlsRow, isl, xlsRow, iTotal].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                sheet1.Range[xlsRow, isl, xlsRow, iTotal].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, isl, xlsRow, iTotal].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, isl, xlsRow, iTotal].CellStyle.Font.Bold = true;
                endXlsCol = xlsCol;
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "1";
                sheet1.Range[xlsRow, iReportName].Text = "Manual OUT Time";
                int startCol = iReportName + 1;
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetManualOutTimeForOTDateWiseReport(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsBioDvAC);
                    dtBioDvAC = dsBioDvAC.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtBioDvAC.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal-1) + (xlsRow) + ")";               
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "2";
                sheet1.Range[xlsRow, iReportName].Text = "Modified OT";

                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetModifiedReport(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsOnlyOt);
                    dtOnlyOt = dsOnlyOt.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtOnlyOt.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "3";
                sheet1.Range[xlsRow, iReportName].Text = "Absent No Punch Time";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetAbsentReports(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsAbsent);
                    dtAbsent = dsAbsent.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtAbsent.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "4";
                sheet1.Range[xlsRow, iReportName].Text = "Absent With single Punch";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetAbsentWithPunchReports(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsAbsentWithPunch);
                    dtAbsentWithPunch = dsAbsentWithPunch.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtAbsentWithPunch.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "5";
                sheet1.Range[xlsRow, iReportName].Text = "Leave With Punch";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetLeaveWithPunchReports(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsLeaveWithPunch);
                    dtLeaveWithPunch = dsLeaveWithPunch.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtLeaveWithPunch.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "6";
                sheet1.Range[xlsRow, iReportName].Text = "Short Duration Absent";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetShortDurationAbsentReports(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsShortDurationAbsent);
                    dtShortDurationAbsent = dsShortDurationAbsent.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtShortDurationAbsent.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "7";
                sheet1.Range[xlsRow, iReportName].Text = "Short Duration";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetWorkDurationSheet(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsWorkDuration);
                    dtWorkDuration = dsWorkDuration.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtWorkDuration.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "8";
                sheet1.Range[xlsRow, iReportName].Text = "OT Applicable And Out Missing";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetOTEntitledWithOutMissingReports(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsOTEntitledWithOutMissing);
                    dtOTEntitledWithOutMissing = dsOTEntitledWithOutMissing.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtOTEntitledWithOutMissing.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "9";
                sheet1.Range[xlsRow, iReportName].Text = "OT Not Applicable And Out Mis";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetOTNotEntitledWithOutMissingReports(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsOTNotEntitledWithOutMissing);
                    dtOTNotEntitledWithOutMissing = dsOTNotEntitledWithOutMissing.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtOTNotEntitledWithOutMissing.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "10";
                sheet1.Range[xlsRow, iReportName].Text = "Un Approved Profile";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetUNApprovedProfile(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsUnApprovedProfile);
                    dtUnApprovedProfile = dsUnApprovedProfile.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtUnApprovedProfile.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "11";
                sheet1.Range[xlsRow, iReportName].Text = "No Salary Structure";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetProfileNoSalary(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsProfileNoSalary);
                    dtProfileNoSalary = dsProfileNoSalary.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtProfileNoSalary.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "12";
                sheet1.Range[xlsRow, iReportName].Text = "Salary Structure Not Approve";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetNoSalaryStructureApprove(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsNoSalaryStructureApprove);
                    dtNoSalaryStructureApprove = dsNoSalaryStructureApprove.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtNoSalaryStructureApprove.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "13";
                sheet1.Range[xlsRow, iReportName].Text = "OT Not Confirm";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetOtNotConfirmOverstayReport(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsOtNotConfirmOverstayReport);
                    dtOtNotConfirmOverstay = dsOtNotConfirmOverstayReport.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtOtNotConfirmOverstay.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "14";
                sheet1.Range[xlsRow, iReportName].Text = "Long Absenteeism";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetLongAbsentisom(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsLongAbsentisom);
                    dtLongAbsentisom = dsLongAbsentisom.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtLongAbsentisom.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "15";
                sheet1.Range[xlsRow, iReportName].Text = "TBS";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetTBS(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsTBS);
                    dtTBS = dsTBS.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtTBS.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "16";
                sheet1.Range[xlsRow, iReportName].Text = "Maternity Leave";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetMaternityLeave(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsMaternityLeave);
                    dtMaternityLeave = dsMaternityLeave.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtMaternityLeave.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "17";
                sheet1.Range[xlsRow, iReportName].Text = "Bank Remark";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetBankRemark(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsBankRemarks);
                    dtBankRemarks = dsBankRemarks.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtBankRemarks.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "18";
                sheet1.Range[xlsRow, iReportName].Text = "Separation With Absent";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetSeparatedAbsent(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsSeparatedAbsent);
                    dtSeparatedAbsent = dsSeparatedAbsent.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtSeparatedAbsent.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "19";
                sheet1.Range[xlsRow, iReportName].Text = "Attendance Not Lock";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    string[] AttendanceNotLockPlant = obj.GetUnLockDateList(dvPlant[i]["Id"].ToString(), workDate);

                    int cc = 0;
                    foreach (var item in AttendanceNotLockPlant)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            cc++;
                        }
                    }
                    obj.GetAttendanceNotLockIndividual(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsAttendanceNotLock);
                    dtAttendanceNotLock = dsAttendanceNotLock.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = cc + dtAttendanceNotLock.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "20";
                sheet1.Range[xlsRow, iReportName].Text = "NotIn LegalDesignation Master";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetNotInLegalDesignationMaster(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsNotInLegalDesignationMaster);
                    dtNotInLegalDesignationMaster = dsNotInLegalDesignationMaster.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtNotInLegalDesignationMaster.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "21";
                sheet1.Range[xlsRow, iReportName].Text = "Salary Not Approved";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetSalaryNotApproved(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsSalaryNotApproved);
                    dtSalaryNotApproved = dsSalaryNotApproved.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtSalaryNotApproved.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "22";
                sheet1.Range[xlsRow, iReportName].Text = "Offday Missing Punch";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetOffdayMissingPunchReports(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsOffdayMissingPunch);
                    dtOffdayMissingPunch = dsOffdayMissingPunch.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtOffdayMissingPunch.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "23";
                sheet1.Range[xlsRow, iReportName].Text = "Offday With Punch";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetOffdayWithPunchReports(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsOffdayWithPunch);
                    dtOffdayWithPunch = dsOffdayWithPunch.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtOffdayWithPunch.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "24";
                sheet1.Range[xlsRow, iReportName].Text = "Absent With Wrong Shift";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetAbsentWithRawPunchReports(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsAbsentWithRawPunch);
                    dtAbsentWithRawPunch = dsAbsentWithRawPunch.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtAbsentWithRawPunch.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";
                xlsRow++;

                sheet1.Range[xlsRow, isl].Text = "25";
                sheet1.Range[xlsRow, iReportName].Text = "Shift Not Assign";
                for (int i = 0; i < dvPlant.Count; i++)
                {
                    obj.GetShiftNotAssign(workDate, dvPlant[i]["Id"].ToString(), companyId, CGId, out dsShiftUnassign);
                    dtShiftUnassign = dsShiftUnassign.Tables[0];
                    sheet1.Range[xlsRow, PlantIndex[dvPlant[i]["Id"].ToString()]].Number = dtShiftUnassign.Rows.Count;
                }
                sheet1.Range[xlsRow, iTotal].Formula = "=SUM(" + oru.GetColumnNameForXls(startCol) + xlsRow + ":" + oru.GetColumnNameForXls(iTotal - 1) + (xlsRow) + ")";

                sheet1.Range[2, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[2, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[2, 1, xlsRow, endXlsCol].WrapText = true;
                
                xlsCol = 1;
                xlsRow += 1;
                #endregion ------------------Column Header------------------

                #region Line Setup
                //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                //sheet1.Range[xlsRow - 1, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region UsedRange Alignment
                //sheet1.UsedRange.WrapText = true;
                //sheet1.UsedRange.CellStyle.Font.Size = 8;
                //sheet1.Range["A1"].CellStyle.Font.Size = 14;
                //sheet1.Range["A2"].CellStyle.Font.Size = 10;
                //sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region ******************Report Header******************
                try
                {
                    string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                    Image companyLogo = Image.FromFile(strPath);
                    if (companyLogo != null)
                    {
                        double totalWidth = sheet1.GetColumnWidth(1) + sheet1.GetColumnWidth(2);
                        int totalWidthPixel = (int)(totalWidth * 7);
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
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
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
                sheet1.Range[xlsRow, 3].Text = "Audit Report Summary:- " + workDate;
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
                sheet1.UsedRange["A6"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;
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

                sheet1.Name = "AuditReportSummary";
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Portrait);

                // return workbook;

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

            }
        }

        #endregion
    }
}