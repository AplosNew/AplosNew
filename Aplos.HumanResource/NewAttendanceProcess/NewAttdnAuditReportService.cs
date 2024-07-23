using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;
using Library.Service.Helpers;
using System.IO;
using Syncfusion.XlsIO;
using System.Drawing;
using ConnectionManager;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class NewAttdnAuditReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public NewAttdnAuditReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IWorkbook GetManualOutTimeDateWiseReport(string username, string plantId, string companyId, string companyGroupId, string plantName, string FromDate, string ToDate)
        {
            #region declare
            clsReport objRpt = null;
            DataSetGenerationClass Gen = null;
            ReportUtility oru = new ReportUtility();
            DataSet dsAbsent = null;
            DataSet dsInMissPunch = null;
            DataSet dsOffdayMissingPunch = null;
            DataSet dsOffdayWithPunch = null;
            DataSet dsShiftUnassign = null;
            DataSet dsLeaveWithPunch = null;
            DataSet dsUnApprovedProfile = null;
            DataSet dsProfileNoSalary = null;
            DataSet dsNoSalaryStructureApprove = null;
            DataSet dsWorkDuration = null;
            DataSet dsOtNotConfirmOverstayReport = null;
            DataSet dsLongAbsentisom = null;
            DataSet dsTBS = null;
            DataSet dsMaternityLeave = null;
            DataSet dsOTEntitledWithOutMissing = null;
            DataSet dsOTNotEntitledWithOutMissing = null;
            DataSet dsBankRemarks = null;
            DataSet dsSeparatedAbsent = null;
            DataSet dsAttendanceNotLockPlant = null;
            DataSet dsTotalAbsent = null;
            DataSet dsNotInLegalDesignationMaster = null;
            DataSet dsSalaryNotApproved = null;
            DataSet dsSeparatedEmpWithPunches = null;
            DataSet dsManualInEntry = null;
            DataTable dtManualInEntry = null;
            DataSet dsManualOutEntry = null;
            DataTable dtManualOutEntry = null;
            DataSet dsManualInOutEntry = null;
            DataTable dtManualInOutEntry = null;
            DataSet dsManualDayStatusEntry = null;
            DataTable dtManualDayStatusEntry = null;
            DataTable dtNotInLegalDesignationMaster = null;
            DataTable dtAbsent = null;
            DataTable dtInPunchMissing = null;
            DataTable dtOffdayMissingPunch = null;
            DataTable dtOffdayWithPunch = null;
            DataTable dtShiftUnassign = null;
            DataTable dtLeaveWithPunch = null;
            DataTable dtUnApprovedProfile = null;
            DataTable dtProfileNoSalary = null;
            DataTable dtNoSalaryStructureApprove = null;
            DataTable dtWorkDuration = null;
            DataTable dtOtNotConfirmOverstay = null;
            DataTable dtLongAbsentisom = null;
            DataTable dtTBS = null;
            DataTable dtMaternityLeave = null;
            DataTable dtOTEntitledWithOutMissing = null;
            DataTable dtOTNotEntitledWithOutMissing = null;
            DataTable dtBankRemarks = null;
            DataTable dtSeparatedAbsent = null;
            DataTable dtSeparatedEmpWithPunches = null;
            DataTable dtAttendanceNotLockPlant = null;
            DataTable dtTotalAbsent = null;
            DataTable dtLongAtbsPlantSetting = null;
            DataTable dtSalaryNotApproved = null;
            DataSet dsLongAtbsPlantSetting = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataView dvPayDays = null;
            DataSet dsLocalHRMSSetting = null;
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string FactoryAddress = string.Empty;
            string OTConsiderOn = string.Empty;
            #endregion

            try
            {
                objStatic.GetPlantWiseHRMSSetting(companyGroupId, plantId, out dsLocalHRMSSetting);
                if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
                {
                    OTConsiderOn = dsLocalHRMSSetting.Tables[0].Rows[0]["OTConsiderOn"].ToString().Trim();
                }

                ExcelEngine excelEngine = null;
                excelEngine = new ExcelEngine();
                IApplication application = null;
                application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Excel2016;
                IWorkbook workbook = application.Workbooks.Create(1);

                workbook.Version = ExcelVersion.Excel2016;

                #region Validation

                if (string.IsNullOrEmpty(FromDate) == true || bplib.clsWebLib.IsDateOK(ToDate) == false)
                {
                    Exception ex = new Exception("Please define access Date..! (allowed format is  dd-MMM-yyyy ex: '01-jan-2008')...");
                    throw (ex);
                }

                #endregion Validation

                objRpt = new clsReport();

                Gen = new DataSetGenerationClass();


                dvPayDays = new DataView();

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region DataSet

                try
                {
                    Gen.GetAbsentReports(FromDate, ToDate, plantId, companyId, companyGroupId, out dsAbsent);
                    dtAbsent = dsAbsent.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    Gen.GetInMissingReports(FromDate, ToDate, plantId, companyId, companyGroupId, out dsInMissPunch);
                    dtInPunchMissing = dsInMissPunch.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    Gen.GetLeaveWithPunchReports(FromDate, ToDate, plantId, companyId, companyGroupId, out dsLeaveWithPunch);
                    dtLeaveWithPunch = dsLeaveWithPunch.Tables[0];

                }
                catch (Exception)
                {
                }
                try
                {
                    Gen.GetOTEntitledWithOutMissingReports(FromDate, ToDate, plantId, companyId, companyGroupId, out dsOTEntitledWithOutMissing);
                    dtOTEntitledWithOutMissing = dsOTEntitledWithOutMissing.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    Gen.GetOTNotEntitledWithOutMissingReports(FromDate, ToDate, plantId, companyId, companyGroupId, out dsOTNotEntitledWithOutMissing);
                    dtOTNotEntitledWithOutMissing = dsOTNotEntitledWithOutMissing.Tables[0];

                }
                catch (Exception)
                {
                }
                try
                {
                    objRpt.GetUNApprovedProfile(FromDate, ToDate, plantId, companyId, companyGroupId, out dsUnApprovedProfile);
                    dtUnApprovedProfile = dsUnApprovedProfile.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    objRpt.GetProfileNoSalary(FromDate, ToDate, plantId, companyId, companyGroupId, out dsProfileNoSalary);
                    dtProfileNoSalary = dsProfileNoSalary.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    objRpt.GetNoSalaryStructureApprove(FromDate, ToDate, plantId, companyId, companyGroupId, out dsNoSalaryStructureApprove);
                    dtNoSalaryStructureApprove = dsNoSalaryStructureApprove.Tables[0];

                }
                catch (Exception)
                {
                }
                try
                {
                    Gen.GetWorkDurationSheet(FromDate, ToDate, plantId, companyId, companyGroupId, out dsWorkDuration);
                    dtWorkDuration = dsWorkDuration.Tables[0];

                }
                catch (Exception)
                {
                }
                try
                {
                    Gen.GetOtNotConfirmOverstayReport(FromDate, ToDate, plantId, companyId, companyGroupId, out dsOtNotConfirmOverstayReport);
                    dtOtNotConfirmOverstay = dsOtNotConfirmOverstayReport.Tables[0];

                }
                catch (Exception)
                {
                }
                try
                {
                    Gen.GetLongAbsentism(FromDate, ToDate, plantId, companyId, companyGroupId, out dsLongAbsentisom);
                    dtLongAbsentisom = dsLongAbsentisom.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    objRpt.GetTBS(FromDate, ToDate, plantId, companyId, companyGroupId, out dsTBS);
                    dtTBS = dsTBS.Tables[0];

                }
                catch (Exception)
                {
                }
                try
                {
                    objRpt.GetMaternityLeave(FromDate, ToDate, plantId, companyId, companyGroupId, out dsMaternityLeave);
                    dtMaternityLeave = dsMaternityLeave.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    objRpt.GetBankRemark(FromDate, ToDate, plantId, companyId, companyGroupId, out dsBankRemarks);
                    dtBankRemarks = dsBankRemarks.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    objRpt.GetAttendanceNotLockPlant(FromDate, ToDate, plantId, companyId, companyGroupId, out dsAttendanceNotLockPlant);
                    dtAttendanceNotLockPlant = dsAttendanceNotLockPlant.Tables[0];

                }
                catch (Exception)
                {
                }
                try
                {
                    objRpt.GetTotalAbsent(FromDate, ToDate, plantId, companyId, companyGroupId, out dsTotalAbsent);
                    dtTotalAbsent = dsTotalAbsent.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    objRpt.GetLongAtbsPlantSetting(FromDate, ToDate, plantId, companyId, companyGroupId, out dsLongAtbsPlantSetting);
                    dtLongAtbsPlantSetting = dsLongAtbsPlantSetting.Tables[0];

                }
                catch (Exception)
                {
                }
                try
                {
                    objRpt.GetNotInLegalDesignationMaster(FromDate, ToDate, plantId, companyId, companyGroupId, out dsNotInLegalDesignationMaster);
                    dtNotInLegalDesignationMaster = dsNotInLegalDesignationMaster.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    objRpt.GetSalaryNotApproved(FromDate, ToDate, plantId, companyId, companyGroupId, out dsSalaryNotApproved);
                    dtSalaryNotApproved = dsSalaryNotApproved.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    objRpt.GetSeparatedAbsent(FromDate, ToDate, plantId, companyId, companyGroupId, out dsSeparatedAbsent);
                    dtSeparatedAbsent = dsSeparatedAbsent.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    Gen.GetOffdayMissingPunchReports(FromDate, ToDate, plantId, companyId, companyGroupId, out dsOffdayMissingPunch);
                    dtOffdayMissingPunch = dsOffdayMissingPunch.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    Gen.GetOffdayWithPunchReports(FromDate, ToDate, plantId, companyId, companyGroupId, out dsOffdayWithPunch);
                    dtOffdayWithPunch = dsOffdayWithPunch.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    objRpt.GetShiftNotAssign(FromDate, ToDate, plantId, companyId, companyGroupId, out dsShiftUnassign);
                    dtShiftUnassign = dsShiftUnassign.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    Gen.GetSeparatedEmployeesPunches(FromDate, ToDate, plantId, companyId, out dsSeparatedEmpWithPunches);
                    dtSeparatedEmpWithPunches = dsSeparatedEmpWithPunches.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    Gen.GetManualInEntry(FromDate, ToDate, plantId, companyId, out dsManualInEntry);
                    dtManualInEntry = dsManualInEntry.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    Gen.GetManualOutEntry(FromDate, ToDate, plantId, companyId, out dsManualOutEntry);
                    dtManualOutEntry = dsManualOutEntry.Tables[0];

                }
                catch (Exception)
                {

                }
                try
                {
                    Gen.GetManualInOutEntry(FromDate, ToDate, plantId, companyId, out dsManualInOutEntry);
                    dtManualInOutEntry = dsManualInOutEntry.Tables[0];

                }
                catch (Exception)
                {

                }

                try
                {
                    Gen.GetManualDayStatusEntry(FromDate, ToDate, plantId, companyId, out dsManualDayStatusEntry);
                    dtManualDayStatusEntry = dsManualDayStatusEntry.Tables[0];

                }
                catch (Exception)
                {

                }
                finally
                {

                }


                #endregion DataSet

                objRpt.SelectedPlantWiseCompany(plantId, out dsCmp);
                objRpt.SelectedPlant(plantId, out dsFactory);
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(27);
                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";

                #region variable
                var iFirstAbsentDate = 0;
                var iDateOfSeperation = 0;
                var iAbsentCount = 0;
                var iAbsentDays = 0;
                var iNumberOfAbsentDays = 0;
                var iWorkTimeDifferentHour = 0;
                var iDurationHour = 0;
                var iWorkDurationHour = 0;
                var iLeaveType = 0;
                var iLogic = 0;
                var iReportName = 0;
                var iObjective = 0;
                var LockDatePlant = 0;
                var iShiftOutTime = 0;
                var iShiftInTime = 0;
                var iWorkDate = 0;
                var iEmployeeCode = 0;
                var iDesignation = 0;
                var iDOJ = 0;
                var iDOS = 0;
                var iInTime = 0;
                var iEmployeeName = 0;
                var iTelephoneNo = 0;
                var iOutTime = 0;
                var iRawPunch = 0;
                var iRawInPunch = 0;
                var iRawOutPunch = 0;
                var iManualByWhom = 0;
                var iSection = 0;
                var iSubSection = 0;
                var iEntity = 0;
                var iShiftName = 0;
                var iFinalOt = 0;
                var iOverStay = 0;
                var iProcessedOT = 0;
                var iOTDifference = 0;
                var iLine = 0;
                var iManualDayStatus = 0;
                var WorkDate = 0;
                var iDepartment = 0;
                var iDayStatus = 0;
                var iPresentFromEffectiveDate = 0;
                var iDuration = 0;
                var iWorkDuration = 0;
                var iWorkTimeDifferent = 0;
                var iEmployeeCurrentStatus = 0;
                var iEmployeeBankStatus = 0;
                var iTotalAbsentDays = 0;
                var iEmployeeCurrentStatusEffectiveDate = 0;
                var iPaymentMode = 0;
                var iBankAccountNo = 0;
                var iRemark = 0;
                var iBabyNo = 0;
                var iFollowUpStartDate = 0;
                var iFollowUpEndDate = 0;
                var iMaternityLeaveStartDate = 0;
                var iMaternityLeaveEndDate = 0;
                var iGapeBetweenConsecutiveIssue = 0;
                var iCurrentStatus = 0;
                var iEmployeeStatus = 0;
                var iProcessedDate = 0;
                var iYear = 0;
                var iMonth = 0;
                var isl = 0;
                var SLNo = 1;
                var igoto = 0;
                var iCount = 0;
                var iEmployeeCategory = 0;
                int SheetIndex = 0;
                string[] AttendanceNotLockPlant = GetUnLockDateList(plantId, FromDate, ToDate);
                #endregion

                int cc = 0;
                foreach (var item in AttendanceNotLockPlant)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        cc++;
                    }
                }

                #region Summary of the report 1
                try
                {
                    IWorksheet sheet20 = null;

                    sheet20 = workbook.Worksheets[SheetIndex];

                    #region ------------------Column Header------------------

                    xlsRow = 1;
                    isl = xlsCol;
                    sheet20.Range[xlsRow, isl].Text = "SL";
                    sheet20.Range[xlsRow, isl].ColumnWidth = 7;
                    sheet20.Range[xlsRow, isl].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet20.Range[xlsRow, isl].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iReportName = xlsCol;
                    sheet20.Range[xlsRow, iReportName].Text = "Report Name";
                    sheet20.Range[xlsRow, iReportName].ColumnWidth = 28;
                    sheet20.Range[xlsRow, iReportName].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet20.Range[xlsRow, iReportName].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iCount = xlsCol;
                    sheet20.Range[xlsRow, iCount].Text = "Count";
                    sheet20.Range[xlsRow, iCount].ColumnWidth = 15;
                    sheet20.Range[xlsRow, iCount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet20.Range[xlsRow, iCount].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iLogic = xlsCol;
                    sheet20.Range[xlsRow, iLogic].Text = "Logic";
                    sheet20.Range[xlsRow, iLogic].ColumnWidth = 65;
                    sheet20.Range[xlsRow, iLogic].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet20.Range[xlsRow, iLogic].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    xlsCol += 1;
                    iObjective = xlsCol;
                    sheet20.Range[xlsRow, iObjective].Text = "Objective";
                    sheet20.Range[xlsRow, iObjective].ColumnWidth = 50;
                    sheet20.Range[xlsRow, iObjective].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet20.Range[xlsRow, iObjective].VerticalAlignment = ExcelVAlign.VAlignCenter;


                    sheet20.Range[xlsRow, isl, xlsRow, iCount].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet20.Range[xlsRow, isl, xlsRow, iCount].BorderAround(ExcelLineStyle.Hair);
                    sheet20.Range[xlsRow, isl, xlsRow, iCount].BorderInside(ExcelLineStyle.Hair);
                    sheet20.Range[xlsRow, isl, xlsRow, iCount].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;

                    #region report Header 1 to 7  

                    sheet20.Range[xlsRow, isl].Text = "1";
                    sheet20.Range[xlsRow, iLogic].Text = "Report Summary";
                    sheet20.Range[xlsRow, iReportName].Text = "1-Index";
                    sheet20.Range[xlsRow, iObjective].Text = "Index";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "2";
                    sheet20.Range[xlsRow, iLogic].Text = "Day Status: A, InTime: Missing, OutTime: Missing";
                    sheet20.Range[xlsRow, iReportName].Text = "2-Absent No Punch Time";
                    sheet20.Range[xlsRow, iObjective].Text = "To Find Who Are Really Absent & Not Sincere About Punch";
                    sheet20.Range[xlsRow, iCount].Number = dtAbsent.Rows.Count;

                    IHyperLink linkAbsentNoPunchTime = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkAbsentNoPunchTime.Type = ExcelHyperLinkType.Workbook;
                    linkAbsentNoPunchTime.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkAbsentNoPunchTime.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkAbsentNoPunchTime.Address = "2_Absent_No_Punch_Time!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "3";
                    sheet20.Range[xlsRow, iLogic].Text = "In Punch Missing";
                    sheet20.Range[xlsRow, iReportName].Text = "3-In Missing";
                    sheet20.Range[xlsRow, iObjective].Text = "Not Sincere About Punch Specially Out punch";
                    sheet20.Range[xlsRow, iCount].Number = dtInPunchMissing.Rows.Count;

                    IHyperLink linkAbsentWithPunch = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkAbsentWithPunch.Type = ExcelHyperLinkType.Workbook;
                    linkAbsentWithPunch.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkAbsentWithPunch.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkAbsentWithPunch.Address = "3_In_Missing!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "4";
                    sheet20.Range[xlsRow, iLogic].Text = "Day Status: LV Having Intime or Out time or Both";
                    sheet20.Range[xlsRow, iReportName].Text = "4-Leave With Punch";
                    sheet20.Range[xlsRow, iObjective].Text = "Without Canceling Leave Present";
                    sheet20.Range[xlsRow, iCount].Number = dtLeaveWithPunch.Rows.Count;

                    IHyperLink linkLeaveWithPunch = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkLeaveWithPunch.Type = ExcelHyperLinkType.Workbook;
                    linkLeaveWithPunch.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkLeaveWithPunch.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkLeaveWithPunch.Address = "4_Leave_With_Punch!A1";
                    xlsRow++;

                    
                    sheet20.Range[xlsRow, isl].Text = "5";
                    sheet20.Range[xlsRow, iLogic].Text = "Work Duration is less than Shift FullDay Duration";
                    sheet20.Range[xlsRow, iReportName].Text = "5-Short Duration";
                    sheet20.Range[xlsRow, iObjective].Text = "To Find Who Are Entitle with Short Leave";
                    sheet20.Range[xlsRow, iCount].Number =dtWorkDuration.Rows.Count;

                    IHyperLink linkShortDuration = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkShortDuration.Type = ExcelHyperLinkType.Workbook;
                    linkShortDuration.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkShortDuration.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkShortDuration.Address = "5_Short_Duration!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "6";
                    sheet20.Range[xlsRow, iLogic].Text = "OT Entitled: NO,Day Status: P,InTime: Yes,OutTime: Missing";
                    sheet20.Range[xlsRow, iReportName].Text = "6-OT Applicable And Out Missing";
                    sheet20.Range[xlsRow, iObjective].Text = "Not Sincere About Out punch";
                    sheet20.Range[xlsRow, iCount].Number = dtOTEntitledWithOutMissing.Rows.Count;


                    IHyperLink linkOtApplicableAndOutMissing = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkOtApplicableAndOutMissing.Type = ExcelHyperLinkType.Workbook;
                    linkOtApplicableAndOutMissing.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkOtApplicableAndOutMissing.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkOtApplicableAndOutMissing.Address = "6_OT_Applicable_And_Out_Missing!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "7";
                    sheet20.Range[xlsRow, iLogic].Text = "OT Not Applicable And Out Missing";
                    sheet20.Range[xlsRow, iReportName].Text = "7-OT Not Applicable And Out Missing";
                    sheet20.Range[xlsRow, iObjective].Text = "OT Not Applicable And Out Missing";
                    sheet20.Range[xlsRow, iCount].Number = dtOTNotEntitledWithOutMissing.Rows.Count;

                    IHyperLink linkOtNotApplicableAndOutMis = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkOtNotApplicableAndOutMis.Type = ExcelHyperLinkType.Workbook;
                    linkOtNotApplicableAndOutMis.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkOtNotApplicableAndOutMis.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkOtNotApplicableAndOutMis.Address = "7_OT_Not_Applicable_And_Out_Mis!A1";
                    xlsRow++;

                    #endregion

                    #region Report Header 8 to 16
                    sheet20.Range[xlsRow, isl].Text = "8";
                    sheet20.Range[xlsRow, iLogic].Text = "Profiles Which Are Not Apporved";
                    sheet20.Range[xlsRow, iReportName].Text = "8-Un Approved Profile";
                    sheet20.Range[xlsRow, iObjective].Text = "To Assist in Profile Approve";
                    sheet20.Range[xlsRow, iCount].Number = dtUnApprovedProfile.Rows.Count;

                    IHyperLink linkUnApprovedProfile = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkUnApprovedProfile.Type = ExcelHyperLinkType.Workbook;
                    linkUnApprovedProfile.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkUnApprovedProfile.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkUnApprovedProfile.Address = "8_Un_Approved_Profile!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "9";
                    sheet20.Range[xlsRow, iLogic].Text = "Profile Approve: Yes,Salary Structure: Not Approved";
                    sheet20.Range[xlsRow, iReportName].Text = "9-No Salary Structure";
                    sheet20.Range[xlsRow, iObjective].Text = "To Assist in Given Salary Structure";
                    sheet20.Range[xlsRow, iCount].Number = dtProfileNoSalary.Rows.Count;


                    IHyperLink linkNoSalaryStructure = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkNoSalaryStructure.Type = ExcelHyperLinkType.Workbook;
                    linkNoSalaryStructure.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkNoSalaryStructure.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkNoSalaryStructure.Address = "9_No_Salary_Structure!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "10";
                    sheet20.Range[xlsRow, iLogic].Text = "Profile Approve: Yes, Salary Structure: Not Approved";
                    sheet20.Range[xlsRow, iReportName].Text = "10-Salary Structure Not Approve";
                    sheet20.Range[xlsRow, iObjective].Text = "To Assist in Approve Salary Structure";
                    sheet20.Range[xlsRow, iCount].Number = dtNoSalaryStructureApprove.Rows.Count;

                    IHyperLink linksalaryStructureNotApprove = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linksalaryStructureNotApprove.Type = ExcelHyperLinkType.Workbook;
                    linksalaryStructureNotApprove.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linksalaryStructureNotApprove.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linksalaryStructureNotApprove.Address = "10_Salary_Structure_Not_Approve!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "11";
                    sheet20.Range[xlsRow, iLogic].Text = "OT Not Confirm Or Over Stay";
                    sheet20.Range[xlsRow, iReportName].Text = "11-OT Not Confirm";
                    sheet20.Range[xlsRow, iObjective].Text = "Whose OT Hasn't Confirmed Yet";
                    sheet20.Range[xlsRow, iCount].Number = dtOtNotConfirmOverstay.Rows.Count;

                    IHyperLink linkOtNotConfirm = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkOtNotConfirm.Type = ExcelHyperLinkType.Workbook;
                    linkOtNotConfirm.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkOtNotConfirm.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkOtNotConfirm.Address = "11_OT_Not_Confirm!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "12";
                    sheet20.Range[xlsRow, iLogic].Text = "Abesnt Continously " + dtLongAtbsPlantSetting.Rows[0]["LongTermAbesnteeism"].ToString() + " Days to " + dtLongAtbsPlantSetting.Rows[0]["TBSDays"].ToString() + " Days. Auto/Manual: " + dtLongAtbsPlantSetting.Rows[0]["IsLongAbsenteeismAuto"];
                    sheet20.Range[xlsRow, iReportName].Text = "12-Long Absenteeism";
                    sheet20.Range[xlsRow, iObjective].Text = "To Find Long Absent";
                    sheet20.Range[xlsRow, iCount].Number = dtLongAbsentisom.Rows.Count;

                    IHyperLink linkLongAbsentisom = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkLongAbsentisom.Type = ExcelHyperLinkType.Workbook;
                    linkLongAbsentisom.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkLongAbsentisom.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkLongAbsentisom.Address = "12_Long_Absenteeism!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "13";
                    sheet20.Range[xlsRow, iLogic].Text = "Abesnt Continously More Than " + dtLongAtbsPlantSetting.Rows[0]["TBSDays"].ToString() + " Days. Auto/Manual: " + dtLongAtbsPlantSetting.Rows[0]["IsTBSAuto"].ToString();
                    sheet20.Range[xlsRow, iReportName].Text = "13-TBS";
                    sheet20.Range[xlsRow, iObjective].Text = "To Finds Whose Are Need To Be Separated";
                    sheet20.Range[xlsRow, iCount].Number = dtTBS.Rows.Count;

                    IHyperLink linkTBS = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkTBS.Type = ExcelHyperLinkType.Workbook;
                    linkTBS.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkTBS.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkTBS.Address = "13_TBS!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "14";
                    sheet20.Range[xlsRow, iLogic].Text = "Who have in Maternity Leave";
                    sheet20.Range[xlsRow, iReportName].Text = "14-Maternity Leave";
                    sheet20.Range[xlsRow, iObjective].Text = "To Finds Whose Are in Maternity Leave";
                    sheet20.Range[xlsRow, iCount].Number = dtMaternityLeave.Rows.Count;

                    IHyperLink linkMaternityLeave = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkMaternityLeave.Type = ExcelHyperLinkType.Workbook;
                    linkMaternityLeave.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkMaternityLeave.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkMaternityLeave.Address = "14_Maternity_Leave!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "15";
                    sheet20.Range[xlsRow, iLogic].Text = "Payment Mode: Bank,Bank Account No: Missing or Not Approve";
                    sheet20.Range[xlsRow, iReportName].Text = "15-Bank Remark";
                    sheet20.Range[xlsRow, iObjective].Text = "Bank Account No or Not Approve";
                    sheet20.Range[xlsRow, iCount].Number = dtBankRemarks.Rows.Count;

                    IHyperLink linkBankRemark = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkBankRemark.Type = ExcelHyperLinkType.Workbook;
                    linkBankRemark.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkBankRemark.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkBankRemark.Address = "15_Bank_Remark!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "16";
                    sheet20.Range[xlsRow, iLogic].Text = "How Many Day No Punch Before The Day of Separation";
                    sheet20.Range[xlsRow, iReportName].Text = "16-Separation With Absent";
                    sheet20.Range[xlsRow, iObjective].Text = "To Assist in Salary & Bonus";
                    sheet20.Range[xlsRow, iCount].Number = dtSeparatedAbsent.Rows.Count;

                    IHyperLink linkSeparationWithAbsent = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkSeparationWithAbsent.Type = ExcelHyperLinkType.Workbook;
                    linkSeparationWithAbsent.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkSeparationWithAbsent.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkSeparationWithAbsent.Address = "16_Separation_With_Absent!A1";
                    xlsRow++;

                    #endregion

                    #region Report Header 17 to 24

                    sheet20.Range[xlsRow, isl].Text = "17";
                    sheet20.Range[xlsRow, iLogic].Text = "Whose Attendance Lock Not Done Yet(" + cc + "/" + dtAttendanceNotLockPlant.Rows.Count + ")";
                    sheet20.Range[xlsRow, iReportName].Text = "17-Attendance Not Lock";
                    sheet20.Range[xlsRow, iObjective].Text = "Whose Attendance Lock Need to Done";
                    sheet20.Range[xlsRow, iCount].Number = cc + dtAttendanceNotLockPlant.Rows.Count;

                    IHyperLink linkAttendanceNotLock = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkAttendanceNotLock.Type = ExcelHyperLinkType.Workbook;
                    linkAttendanceNotLock.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkAttendanceNotLock.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkAttendanceNotLock.Address = "17_Attendance_Not_Lock!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "18";
                    sheet20.Range[xlsRow, iLogic].Text = "Legal Designation Not In Designation Master";
                    sheet20.Range[xlsRow, iReportName].Text = "18-NotIn LegalDesignation Master";
                    sheet20.Range[xlsRow, iObjective].Text = "Legal Designation Not In Designation Master";
                    sheet20.Range[xlsRow, iCount].Number = dtNotInLegalDesignationMaster.Rows.Count;

                    IHyperLink linkLegalDesignation = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkLegalDesignation.Type = ExcelHyperLinkType.Workbook;
                    linkLegalDesignation.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkLegalDesignation.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkLegalDesignation.Address = "18_NotIn_Designation_Master!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "19";
                    sheet20.Range[xlsRow, iLogic].Text = "Salary has been processed but not approved";
                    sheet20.Range[xlsRow, iReportName].Text = "19-Salary Not Approved";
                    sheet20.Range[xlsRow, iObjective].Text = "Whose Salary need to be Approved";
                    sheet20.Range[xlsRow, iCount].Number = dtSalaryNotApproved.Rows.Count;

                    IHyperLink linkSalarynotprocessed = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkSalarynotprocessed.Type = ExcelHyperLinkType.Workbook;
                    linkSalarynotprocessed.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkSalarynotprocessed.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkSalarynotprocessed.Address = "19_Salary_Not_Approved!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "20";
                    sheet20.Range[xlsRow, iLogic].Text = "Offday Missing Punch";
                    sheet20.Range[xlsRow, iReportName].Text = "20-Offday Missing Punch";
                    sheet20.Range[xlsRow, iObjective].Text = "Those Who have worked in Offday and Punch is missing";
                    sheet20.Range[xlsRow, iCount].Number = dtOffdayMissingPunch.Rows.Count;

                    IHyperLink linkOffdayMissingpunch = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkOffdayMissingpunch.Type = ExcelHyperLinkType.Workbook;
                    linkOffdayMissingpunch.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkOffdayMissingpunch.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkOffdayMissingpunch.Address = "20_Offday_Missing_Punch!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "21";
                    sheet20.Range[xlsRow, iLogic].Text = "Offday With Punch";
                    sheet20.Range[xlsRow, iReportName].Text = "21-Offday With Punch";
                    sheet20.Range[xlsRow, iObjective].Text = "Those Who have worked in Offday";
                    sheet20.Range[xlsRow, iCount].Number = dtOffdayWithPunch.Rows.Count;

                    IHyperLink linkOffdayWithpunch = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkOffdayWithpunch.Type = ExcelHyperLinkType.Workbook;
                    linkOffdayWithpunch.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkOffdayWithpunch.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkOffdayWithpunch.Address = "21_Offday_With_Punch!A1";
                    xlsRow++;


                    sheet20.Range[xlsRow, isl].Text = "22";
                    sheet20.Range[xlsRow, iLogic].Text = "Shift Not Assign";
                    sheet20.Range[xlsRow, iReportName].Text = "22-Shift Not Assign";
                    sheet20.Range[xlsRow, iObjective].Text = "Those whose Shift is Not Assigned";
                    sheet20.Range[xlsRow, iCount].Number = dtShiftUnassign.Rows.Count;

                    IHyperLink linkLeaveRejectionReflection = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkLeaveRejectionReflection.Type = ExcelHyperLinkType.Workbook;
                    linkLeaveRejectionReflection.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkLeaveRejectionReflection.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkLeaveRejectionReflection.Address = "22_Shift_Not_Assign!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "23";
                    sheet20.Range[xlsRow, iLogic].Text = "InActive Employees With Punches";
                    sheet20.Range[xlsRow, iReportName].Text = "23-InActive Employees Punches";
                    sheet20.Range[xlsRow, iObjective].Text = "RawData Punches of InActive Employees";
                    sheet20.Range[xlsRow, iCount].Number = dtSeparatedEmpWithPunches.Rows.Count;

                    IHyperLink linkSeparatedEmpPunches = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkSeparatedEmpPunches.Type = ExcelHyperLinkType.Workbook;
                    linkSeparatedEmpPunches.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkSeparatedEmpPunches.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkSeparatedEmpPunches.Address = "23_InActive_Emp_Punches!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "24";
                    sheet20.Range[xlsRow, iLogic].Text = "ManualIn Entry";
                    sheet20.Range[xlsRow, iReportName].Text = "24-ManualIn Entries";
                    sheet20.Range[xlsRow, iObjective].Text = "Manual-In Punches of Employees";
                    sheet20.Range[xlsRow, iCount].Number = dtManualInEntry.Rows.Count;

                    IHyperLink linkManualInPunch = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkManualInPunch.Type = ExcelHyperLinkType.Workbook;
                    linkManualInPunch.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkManualInPunch.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkManualInPunch.Address = "24_ManualIn_Entry!A1";
                    xlsRow++;

                    #endregion

                    sheet20.Range[xlsRow, isl].Text = "25";
                    sheet20.Range[xlsRow, iLogic].Text = "ManualOut Entry";
                    sheet20.Range[xlsRow, iReportName].Text = "25-ManualOut Entries";
                    sheet20.Range[xlsRow, iObjective].Text = "Manual-Out Punches of Employees";
                    sheet20.Range[xlsRow, iCount].Number = dtManualOutEntry.Rows.Count;

                    IHyperLink linkManualOutPunch = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkManualOutPunch.Type = ExcelHyperLinkType.Workbook;
                    linkManualOutPunch.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkManualOutPunch.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkManualOutPunch.Address = "25_ManualOut_Entry!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "26";
                    sheet20.Range[xlsRow, iLogic].Text = "ManualDayStatus Entry";
                    sheet20.Range[xlsRow, iReportName].Text = "26-ManualDayStatus Entries";
                    sheet20.Range[xlsRow, iObjective].Text = "Manual DayStatus of Employees";
                    sheet20.Range[xlsRow, iCount].Number = dtManualDayStatusEntry.Rows.Count;

                    IHyperLink linkManualDayStatus = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkManualDayStatus.Type = ExcelHyperLinkType.Workbook;
                    linkManualDayStatus.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkManualDayStatus.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkManualDayStatus.Address = "26_ManualDayStatus_Entry!A1";
                    xlsRow++;

                    sheet20.Range[xlsRow, isl].Text = "27";
                    sheet20.Range[xlsRow, iLogic].Text = "ManualInOut Entry";
                    sheet20.Range[xlsRow, iReportName].Text = "27-ManualInOut Entries";
                    sheet20.Range[xlsRow, iObjective].Text = "Manual-InOut Punches of Employees";
                    sheet20.Range[xlsRow, iCount].Number = dtManualInOutEntry.Rows.Count;

                    IHyperLink linkManualInOutPunch = sheet20.HyperLinks.Add(sheet20.Range[xlsRow, iReportName]);
                    linkManualInOutPunch.Type = ExcelHyperLinkType.Workbook;
                    linkManualInOutPunch.TextToDisplay = sheet20.Range[xlsRow, iReportName].Text;
                    linkManualInOutPunch.ScreenTip = "Go To " + sheet20.Range[xlsRow, iReportName].Text;
                    linkManualInOutPunch.Address = "27_ManualInOut_Entry!A1";
                    xlsRow++;

                    sheet20.Range[2, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet20.Range[2, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet20.Range[2, 1, xlsRow, endXlsCol].WrapText = true;

                    #endregion ------------------Column Header------------------

                    #region Freeze Panes
                    sheet20.IsDisplayZeros = false;
                    sheet20.UsedRange["A2"].FreezePanes();
                    #endregion Freeze Panes

                    #region Page Setup
                    sheet20.PageSetup.TopMargin = 0.5;
                    sheet20.PageSetup.BottomMargin = 0.7;
                    sheet20.PageSetup.PrintTitleRows = "$1:$5";
                    sheet20.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet20.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet20.PageSetup.LeftMargin = 0.5;
                    sheet20.PageSetup.RightMargin = 0.2;
                    sheet20.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet20.PageSetup.FitToPagesTall = 0;
                    sheet20.PageSetup.FitToPagesWide = 1;
                    sheet20.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet20.IsDisplayZeros = false;

                    sheet20.Name = (SheetIndex + 1) + "_Index";

                    #endregion Page Setup

                }
                catch (Exception ex)
                {

                }
                #endregion  Summary of the report
              
                #region  Absent No Punch Time 2
                try
                {
                    IWorksheet sheet3 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    //var iDayStatus = 0;
                    SheetIndex++;
                    sheet3 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet3.Range[5, igoto].Text = "Goto Index";
                    sheet3.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromAbsentNoPunchTime = sheet3.HyperLinks.Add(sheet3.Range[5, igoto]);
                    linkgofromAbsentNoPunchTime.Type = ExcelHyperLinkType.Workbook;
                    linkgofromAbsentNoPunchTime.TextToDisplay = sheet3.Range[5, igoto].Text;
                    linkgofromAbsentNoPunchTime.ScreenTip = "Go To " + sheet3.Range[5, igoto].Text;
                    linkgofromAbsentNoPunchTime.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet3.Range[xlsRow, isl].Text = "SL";
                    sheet3.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet3.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet3.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet3.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet3.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iDayStatus = xlsCol;
                    sheet3.Range[xlsRow, iDayStatus].Text = "Day Status";
                    sheet3.Range[xlsRow, iDayStatus].ColumnWidth = 18;
                    sheet3.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet3.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet3.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet3.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet3.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iTelephoneNo = xlsCol;
                    sheet3.Range[xlsRow, iTelephoneNo].Text = "Telephone No.";
                    sheet3.Range[xlsRow, iTelephoneNo].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet3.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet3.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet3.Range[xlsRow, iDepartment].Text = "Department";
                    sheet3.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet3.Range[xlsRow, iSection].Text = "Section";
                    sheet3.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet3.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet3.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet3.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet3.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet3.Range[xlsRow, iEntity].Text = "Entity";
                    sheet3.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iLine = xlsCol;
                    sheet3.Range[xlsRow, iLine].Text = "Line";
                    sheet3.Range[xlsRow, iLine].ColumnWidth = 15;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet3.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet3.Range[xlsRow, iDOJ].ColumnWidth = 18;


                    xlsCol += 1;
                    iTotalAbsentDays = xlsCol;
                    sheet3.Range[xlsRow, iTotalAbsentDays].Text = "Total Absent Days(This Month)";
                    sheet3.Range[xlsRow, iTotalAbsentDays].ColumnWidth = 25;

                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet3.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet3.Range[xlsRow, iShiftName].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet3.Range[xlsRow, iShiftInTime].Text = "Shift In Time";
                    sheet3.Range[xlsRow, iShiftInTime].ColumnWidth = 10;
                    sheet3.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet3.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet3.Range[xlsRow, iShiftOutTime].Text = "Shift Out Time";
                    sheet3.Range[xlsRow, iShiftOutTime].ColumnWidth = 10;
                    sheet3.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet3.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iInTime = xlsCol;
                    sheet3.Range[xlsRow, iInTime].Text = "In Time";
                    sheet3.Range[xlsRow, iInTime].ColumnWidth = 14;
                    sheet3.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet3.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet3.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet3.Range[xlsRow, iOutTime].Text = "Out Time";
                    sheet3.Range[xlsRow, iOutTime].ColumnWidth = 14;
                    sheet3.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet3.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet3.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet3.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //sheet3.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet3.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet3.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet3.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtAbsent.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtAbsent.Rows.Count; i++)
                        {

                            #region ----------------------Data-----------------------
                            sheet3.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet3.Range[xlsRow, iTotalAbsentDays].Number = clsStaticInfo.dbl(dtAbsent.Rows[i]["TotalAbsent"].ToString());
                            sheet3.Range[xlsRow, iTotalAbsentDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet3.Range[xlsRow, iTotalAbsentDays].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet3.Range[xlsRow, iEmployeeCode].Text = dtAbsent.Rows[i]["EmployeeCode"].ToString();

                            sheet3.Range[xlsRow, iEmployeeName].Text = dtAbsent.Rows[i]["EmployeeName"].ToString();
                            sheet3.Range[xlsRow, iTelephoneNo].Text = dtAbsent.Rows[i]["TelePhnNo"].ToString();

                            sheet3.Range[xlsRow, iLine].Text = dtAbsent.Rows[i]["Line"].ToString();
                            sheet3.Range[xlsRow, iEmployeeCategory].Text = dtAbsent.Rows[i]["EmployeeCategory"].ToString();
                            sheet3.Range[xlsRow, iDayStatus].Text = dtAbsent.Rows[i]["DayStatus"].ToString();
                            sheet3.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet3.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (bplib.clsWebLib.GetBoolData(dtAbsent.Rows[i]["IsManualDayStatus"].ToString().Trim()))
                            {
                                sheet3.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Orange;
                            }

                            sheet3.Range[xlsRow, iDepartment].Text = dtAbsent.Rows[i]["Department"].ToString();

                            sheet3.Range[xlsRow, iDesignation].Text = dtAbsent.Rows[i]["LegalDesignation"].ToString();

                            sheet3.Range[xlsRow, iSection].Text = dtAbsent.Rows[i]["Section"].ToString();

                            sheet3.Range[xlsRow, iSubSection].Text = dtAbsent.Rows[i]["SubSection"].ToString();

                            sheet3.Range[xlsRow, iEntity].Text = dtAbsent.Rows[i]["EntityName"].ToString();

                            sheet3.Range[xlsRow, iWorkDate].Text = dtAbsent.Rows[i]["WorkDate"].ToString();

                            sheet3.Range[xlsRow, iDOJ].Text = dtAbsent.Rows[i]["DOJ"].ToString();

                            sheet3.Range[xlsRow, iShiftName].Text = dtAbsent.Rows[i]["ShiftName"].ToString();

                            sheet3.Range[xlsRow, iShiftInTime].Text = dtAbsent.Rows[i]["ShiftInTime"].ToString();
                            sheet3.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet3.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet3.Range[xlsRow, iShiftOutTime].Text = dtAbsent.Rows[i]["ShiftOutTime"].ToString();
                            sheet3.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet3.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (dtAbsent.Rows[i]["InTime"].ToString() != "")
                            {
                                sheet3.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                sheet3.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtAbsent.Rows[i]["InTime"].ToString());
                                sheet3.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet3.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtAbsent.Rows[i]["IsManualInTime"].ToString().Trim()))
                                {
                                    sheet3.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }

                            if (dtAbsent.Rows[i]["OutTime"].ToString() != "")
                            {
                                sheet3.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                sheet3.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(dtAbsent.Rows[i]["OutTime"].ToString());
                                sheet3.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet3.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtAbsent.Rows[i]["IsManualOutTime"].ToString().Trim()))
                                {
                                    sheet3.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }

                            xlsRow++;
                            SLNo++;

                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet3.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet3.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet3.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                        xlsRow++;
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "Day Status: A";
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "InTime: Blank";
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "OutTime:Blank";
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet3.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;
                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet3.GetColumnWidth(1) + sheet3.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet3.GetRowHeight(1) + sheet3.GetRowHeight(2) + sheet3.GetRowHeight(3) + sheet3.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet3.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet3.Range[xlsRow, 3].Text = CmpName;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet3.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet3.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {

                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet3.Range[xlsRow, 3].Text = FactoryName;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet3.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet3.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet3.Range[xlsRow, 3].CellStyle.Font.Size = 22;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet3.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Absent No Punch Time: " + FromDate + " To Date: " + ToDate;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet3.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet3.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet3.IsDisplayZeros = false;
                    sheet3.UsedRange["A7"].FreezePanes();
                    sheet3.FirstVisibleColumn = 1;
                    sheet3.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet3.UsedRange.WrapText = true;
                    sheet3.UsedRange.CellStyle.Font.Size = 8;
                    sheet3.Range["A1"].CellStyle.Font.Size = 14;
                    sheet3.Range["A2"].CellStyle.Font.Size = 10;
                    sheet3.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet3.PageSetup.TopMargin = 0.5;
                    sheet3.PageSetup.BottomMargin = 0.7;
                    sheet3.PageSetup.PrintTitleRows = "$1:$5";
                    sheet3.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet3.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet3.PageSetup.LeftMargin = 0.5;
                    sheet3.PageSetup.RightMargin = 0.2;
                    sheet3.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet3.PageSetup.FitToPagesTall = 0;
                    sheet3.PageSetup.FitToPagesWide = 1;
                    sheet3.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet3.IsDisplayZeros = false;

                    if (dtAbsent.Rows.Count > 0)
                    {
                        sheet3.Name = (SheetIndex + 1) + "_Absent_No_Punch_Time";
                        sheet3.TabColorRGB = Color.Red;

                    }
                    else
                    {
                        sheet3.Name = (SheetIndex + 1) + "_Absent_No_Punch_Time";
                    }
                    #endregion Page Setup


                }
                catch (Exception ex)
                {
                }
                #endregion   Absent No Punch Time

                #region Absent With Punch 3
                try
                {
                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    IWorksheet sheet4 = null;
                    sheet4 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet4.Range[5, igoto].Text = "Goto Index";
                    sheet4.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromAbsentWithPunch = sheet4.HyperLinks.Add(sheet4.Range[5, igoto]);
                    linkgofromAbsentWithPunch.Type = ExcelHyperLinkType.Workbook;
                    linkgofromAbsentWithPunch.TextToDisplay = sheet4.Range[5, igoto].Text;
                    linkgofromAbsentWithPunch.ScreenTip = "Go To " + sheet4.Range[5, igoto].Text;
                    linkgofromAbsentWithPunch.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet4.Range[xlsRow, isl].Text = "SL";
                    sheet4.Range[xlsRow, isl].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet4.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet4.Range[xlsRow, iEmployeeCode].ColumnWidth = 7;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet4.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet4.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iDayStatus = xlsCol;
                    sheet4.Range[xlsRow, iDayStatus].Text = "Day Status";
                    sheet4.Range[xlsRow, iDayStatus].ColumnWidth = 18;
                    sheet4.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet4.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet4.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;


                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet4.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet4.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iTelephoneNo = xlsCol;
                    sheet4.Range[xlsRow, iTelephoneNo].Text = "Telephone No.";
                    sheet4.Range[xlsRow, iTelephoneNo].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet4.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet4.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet4.Range[xlsRow, iDepartment].Text = "Department";
                    sheet4.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet4.Range[xlsRow, iSection].Text = "Section";
                    sheet4.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet4.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet4.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet4.Range[xlsRow, iEntity].Text = "Entity";
                    sheet4.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet4.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet4.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet4.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet4.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet4.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet4.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet4.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet4.Range[xlsRow, iShiftName].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet4.Range[xlsRow, iShiftInTime].Text = "Shift In Time";
                    sheet4.Range[xlsRow, iShiftInTime].ColumnWidth = 10;
                    sheet4.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet4.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet4.Range[xlsRow, iShiftOutTime].Text = "Shift Out Time";
                    sheet4.Range[xlsRow, iShiftOutTime].ColumnWidth = 10;
                    sheet4.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet4.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iInTime = xlsCol;
                    sheet4.Range[xlsRow, iInTime].Text = "In Time";
                    sheet4.Range[xlsRow, iInTime].ColumnWidth = 14;
                    sheet4.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet4.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet4.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet4.Range[xlsRow, iOutTime].Text = "Out Time";
                    sheet4.Range[xlsRow, iOutTime].ColumnWidth = 14;
                    sheet4.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet4.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet4.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet4.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //sheet4.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.Gray;
                    sheet4.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet4.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet4.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------


                    if (dtInPunchMissing.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtInPunchMissing.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet4.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet4.Range[xlsRow, iEmployeeCode].Text = dtInPunchMissing.Rows[i]["EmployeeCode"].ToString();


                            sheet4.Range[xlsRow, iDepartment].Text = dtInPunchMissing.Rows[i]["Department"].ToString();

                            sheet4.Range[xlsRow, iEmployeeName].Text = dtInPunchMissing.Rows[i]["EmployeeName"].ToString();

                            sheet4.Range[xlsRow, iEmployeeCurrentStatus].Text = dtInPunchMissing.Rows[i]["EmployeeCurrentStatus"].ToString();
                            sheet4.Range[xlsRow, iTelephoneNo].Text = dtInPunchMissing.Rows[i]["TelePhnNo"].ToString();


                            sheet4.Range[xlsRow, iDayStatus].Text = dtInPunchMissing.Rows[i]["DayStatus"].ToString();
                            sheet4.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet4.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (bplib.clsWebLib.GetBoolData(dtInPunchMissing.Rows[i]["IsManualDayStatus"].ToString().Trim()))
                            {
                                sheet4.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Orange;
                            }

                            sheet4.Range[xlsRow, iDesignation].Text = dtInPunchMissing.Rows[i]["LegalDesignation"].ToString();

                            sheet4.Range[xlsRow, iSection].Text = dtInPunchMissing.Rows[i]["Section"].ToString();
                            sheet4.Range[xlsRow, iEmployeeCategory].Text = dtInPunchMissing.Rows[i]["EmployeeCategory"].ToString();
                            sheet4.Range[xlsRow, iSubSection].Text = dtInPunchMissing.Rows[i]["SubSection"].ToString();
                            sheet4.Range[xlsRow, iEntity].Text = dtInPunchMissing.Rows[i]["EntityName"].ToString();
                            sheet4.Range[xlsRow, iWorkDate].Text = dtInPunchMissing.Rows[i]["WorkDate"].ToString();

                            sheet4.Range[xlsRow, iDOJ].Text = dtInPunchMissing.Rows[i]["DOJ"].ToString();

                            sheet4.Range[xlsRow, iShiftName].Text = dtInPunchMissing.Rows[i]["ShiftName"].ToString();

                            sheet4.Range[xlsRow, iShiftInTime].Text = dtInPunchMissing.Rows[i]["ShiftInTime"].ToString();
                            sheet4.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet4.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet4.Range[xlsRow, iShiftOutTime].Text = dtInPunchMissing.Rows[i]["ShiftOutTime"].ToString();
                            sheet4.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet4.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (dtInPunchMissing.Rows[i]["InTime"].ToString() != "")
                            {
                                sheet4.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                sheet4.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtInPunchMissing.Rows[i]["InTime"].ToString());
                                sheet4.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet4.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtInPunchMissing.Rows[i]["IsManualInTime"].ToString().Trim()))
                                {
                                    sheet4.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }

                            if (dtInPunchMissing.Rows[i]["OutTime"].ToString() != "")
                            {
                                sheet4.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                sheet4.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(dtInPunchMissing.Rows[i]["OutTime"].ToString());
                                sheet4.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet4.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtInPunchMissing.Rows[i]["IsManualOutTime"].ToString().Trim()))
                                {
                                    sheet4.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }


                            xlsRow++;
                            SLNo++;

                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet4.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet4.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet4.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup
                        xlsRow++;
                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "In Punch Missing";
                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "";
                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet4.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet4.GetColumnWidth(1) + sheet4.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet4.GetRowHeight(1) + sheet4.GetRowHeight(2) + sheet4.GetRowHeight(3) + sheet4.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet4.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet4.Range[xlsRow, 3].Text = CmpName;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet4.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet4.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet4.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet4.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {

                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet4.Range[xlsRow, 3].Text = FactoryName;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet4.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet4.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet4.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet4.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet4.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet4.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet4.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet4.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-In Punch Missing From: " + FromDate + " To Date: " + ToDate;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet4.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet4.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet4.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet4.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet4.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet4.IsDisplayZeros = false;
                    sheet4.UsedRange["A7"].FreezePanes();
                    sheet4.FirstVisibleColumn = 1;
                    sheet4.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet4.UsedRange.WrapText = true;
                    sheet4.UsedRange.CellStyle.Font.Size = 8;
                    sheet4.Range["A1"].CellStyle.Font.Size = 14;
                    sheet4.Range["A2"].CellStyle.Font.Size = 10;
                    sheet4.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet4.PageSetup.TopMargin = 0.5;
                    sheet4.PageSetup.BottomMargin = 0.7;
                    sheet4.PageSetup.PrintTitleRows = "$1:$5";
                    sheet4.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet4.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet4.PageSetup.LeftMargin = 0.5;
                    sheet4.PageSetup.RightMargin = 0.2;
                    sheet4.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet4.PageSetup.FitToPagesTall = 0;
                    sheet4.PageSetup.FitToPagesWide = 1;
                    sheet4.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet4.IsDisplayZeros = false;

                    if (dtInPunchMissing.Rows.Count > 0)
                    {
                        sheet4.Name = (SheetIndex + 1) + "_In_Missing";
                        sheet4.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet4.Name = (SheetIndex + 1) + "_In_Missing";
                    }

                    #endregion Page Setup
                }
                catch (Exception)
                {

                }

                #endregion Absent With Punch

                #region Leave With Punch 4

                try
                {
                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    IWorksheet sheet5 = null;
                    SheetIndex++;
                    sheet5 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet5.Range[5, igoto].Text = "Goto Index";
                    sheet5.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromLeaveWithPunch = sheet5.HyperLinks.Add(sheet5.Range[5, igoto]);
                    linkgofromLeaveWithPunch.Type = ExcelHyperLinkType.Workbook;
                    linkgofromLeaveWithPunch.TextToDisplay = sheet5.Range[5, igoto].Text;
                    linkgofromLeaveWithPunch.ScreenTip = "Go To " + sheet5.Range[5, igoto].Text;
                    linkgofromLeaveWithPunch.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet5.Range[xlsRow, isl].Text = "SL";
                    sheet5.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet5.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet5.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet5.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet5.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iDayStatus = xlsCol;
                    sheet5.Range[xlsRow, iDayStatus].Text = "Day Status";
                    sheet5.Range[xlsRow, iDayStatus].ColumnWidth = 18;
                    sheet5.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet5.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet5.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet5.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet5.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet5.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet5.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet5.Range[xlsRow, iDepartment].Text = "Department";
                    sheet5.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet5.Range[xlsRow, iSection].Text = "Section";
                    sheet5.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet5.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet5.Range[xlsRow, iSubSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet5.Range[xlsRow, iEntity].Text = "Entity";
                    sheet5.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet5.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet5.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet5.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet5.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet5.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet5.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet5.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet5.Range[xlsRow, iShiftName].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet5.Range[xlsRow, iShiftInTime].Text = "Shift In Time";
                    sheet5.Range[xlsRow, iShiftInTime].ColumnWidth = 10;
                    sheet5.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet5.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet5.Range[xlsRow, iShiftOutTime].Text = "Shift Out Time";
                    sheet5.Range[xlsRow, iShiftOutTime].ColumnWidth = 10;
                    sheet5.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet5.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iInTime = xlsCol;
                    sheet5.Range[xlsRow, iInTime].Text = "In Time";
                    sheet5.Range[xlsRow, iInTime].ColumnWidth = 14;
                    sheet5.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet5.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet5.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet5.Range[xlsRow, iOutTime].Text = "Out Time";
                    sheet5.Range[xlsRow, iOutTime].ColumnWidth = 14;
                    sheet5.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet5.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet5.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iLeaveType = xlsCol;
                    sheet5.Range[xlsRow, iLeaveType].Text = "Leave Type";
                    sheet5.Range[xlsRow, iLeaveType].ColumnWidth = 16;
                    sheet5.Range[xlsRow, iLeaveType].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet5.Range[xlsRow, iLeaveType].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet5.Range[xlsRow, iLeaveType].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet5.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //sheet5.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet5.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet5.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet5.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtLeaveWithPunch.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtLeaveWithPunch.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet5.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet5.Range[xlsRow, iEmployeeCode].Text = dtLeaveWithPunch.Rows[i]["EmployeeCode"].ToString();

                            sheet5.Range[xlsRow, iEmployeeName].Text = dtLeaveWithPunch.Rows[i]["EmployeeName"].ToString();

                            sheet5.Range[xlsRow, iEmployeeCurrentStatus].Text = dtLeaveWithPunch.Rows[i]["EmployeeCurrentStatus"].ToString();

                            sheet5.Range[xlsRow, iDayStatus].Text = dtLeaveWithPunch.Rows[i]["DayStatus"].ToString();
                            sheet5.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet5.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (bplib.clsWebLib.GetBoolData(dtLeaveWithPunch.Rows[i]["IsManualDayStatus"].ToString().Trim()))
                            {
                                sheet5.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Orange;
                            }

                            sheet5.Range[xlsRow, iDesignation].Text = dtLeaveWithPunch.Rows[i]["LegalDesignation"].ToString();

                            sheet5.Range[xlsRow, iDepartment].Text = dtLeaveWithPunch.Rows[i]["Department"].ToString();

                            sheet5.Range[xlsRow, iLeaveType].Text = dtLeaveWithPunch.Rows[i]["Code"].ToString();

                            sheet5.Range[xlsRow, iSection].Text = dtLeaveWithPunch.Rows[i]["Section"].ToString();

                            sheet5.Range[xlsRow, iSubSection].Text = dtLeaveWithPunch.Rows[i]["SubSection"].ToString();

                            sheet5.Range[xlsRow, iEntity].Text = dtLeaveWithPunch.Rows[i]["EntityName"].ToString();

                            sheet5.Range[xlsRow, iWorkDate].Text = dtLeaveWithPunch.Rows[i]["WorkDate"].ToString();

                            sheet5.Range[xlsRow, iDOJ].Text = dtLeaveWithPunch.Rows[i]["DOJ"].ToString();

                            sheet5.Range[xlsRow, iShiftName].Text = dtLeaveWithPunch.Rows[i]["ShiftName"].ToString();

                            sheet5.Range[xlsRow, iEmployeeCategory].Text = dtLeaveWithPunch.Rows[i]["EmployeeCategory"].ToString();
                            sheet5.Range[xlsRow, iShiftInTime].Text = dtLeaveWithPunch.Rows[i]["ShiftInTime"].ToString();
                            sheet5.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet5.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet5.Range[xlsRow, iShiftOutTime].Text = dtLeaveWithPunch.Rows[i]["ShiftOutTime"].ToString();
                            sheet5.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet5.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (dtLeaveWithPunch.Rows[i]["InTime"].ToString() != "")
                            {
                                sheet5.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                sheet5.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtLeaveWithPunch.Rows[i]["InTime"].ToString());
                                sheet5.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet5.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtLeaveWithPunch.Rows[i]["IsManualInTime"].ToString().Trim()))
                                {
                                    sheet5.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }

                            if (dtLeaveWithPunch.Rows[i]["OutTime"].ToString() != "")
                            {
                                sheet5.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                sheet5.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(dtLeaveWithPunch.Rows[i]["OutTime"].ToString());
                                sheet5.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet5.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtLeaveWithPunch.Rows[i]["IsManualOutTime"].ToString().Trim()))
                                {
                                    sheet5.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }

                            }

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet5.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet5.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet5.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                        xlsRow++;
                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "Day Status: A";
                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "InTime:Not Blank";
                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet5.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;
                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet5.GetColumnWidth(1) + sheet5.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet5.GetRowHeight(1) + sheet5.GetRowHeight(2) + sheet5.GetRowHeight(3) + sheet5.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet5.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet5.Range[xlsRow, 3].Text = CmpName;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet5.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet5.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet5.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet5.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet5.Range[xlsRow, 3].Text = FactoryName;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet5.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet5.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet5.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet5.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet5.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet5.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet5.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet5.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Leave With Punch: As On" + ToDate;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet5.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet5.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet5.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet5.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet5.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet5.IsDisplayZeros = false;
                    sheet5.UsedRange["A7"].FreezePanes();
                    sheet5.FirstVisibleColumn = 1;
                    sheet5.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet5.UsedRange.WrapText = true;
                    sheet5.UsedRange.CellStyle.Font.Size = 8;
                    sheet5.Range["A1"].CellStyle.Font.Size = 14;
                    sheet5.Range["A2"].CellStyle.Font.Size = 10;
                    sheet5.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet5.PageSetup.TopMargin = 0.5;
                    sheet5.PageSetup.BottomMargin = 0.7;
                    sheet5.PageSetup.PrintTitleRows = "$1:$5";
                    sheet5.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet5.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet5.PageSetup.LeftMargin = 0.5;
                    sheet5.PageSetup.RightMargin = 0.2;
                    sheet5.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet5.PageSetup.FitToPagesTall = 0;
                    sheet5.PageSetup.FitToPagesWide = 1;
                    sheet5.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet5.IsDisplayZeros = false;

                    if (dtLeaveWithPunch.Rows.Count > 0)
                    {
                        sheet5.Name = (SheetIndex + 1) + "_Leave_With_Punch";
                        sheet5.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet5.Name = (SheetIndex + 1) + "_Leave_With_Punch";
                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {

                }
                #endregion  Absent with InTime
               
                #region  Short Duration 5
                try
                {
                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    IWorksheet sheet10 = null;
                    SheetIndex++;
                    sheet10 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet10.Range[5, igoto].Text = "Goto Index";
                    sheet10.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromShortDuration = sheet10.HyperLinks.Add(sheet10.Range[5, igoto]);
                    linkgofromShortDuration.Type = ExcelHyperLinkType.Workbook;
                    linkgofromShortDuration.TextToDisplay = sheet10.Range[5, igoto].Text;
                    linkgofromShortDuration.ScreenTip = "Go To " + sheet10.Range[5, igoto].Text;
                    linkgofromShortDuration.Address = "1_Index!A1";


                    isl = xlsCol;
                    sheet10.Range[xlsRow, isl].Text = "SL";
                    sheet10.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet10.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet10.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet10.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet10.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet10.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet10.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDayStatus = xlsCol;
                    sheet10.Range[xlsRow, iDayStatus].Text = "Day Status";
                    sheet10.Range[xlsRow, iDayStatus].ColumnWidth = 12;
                    sheet10.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet10.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet10.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet10.Range[xlsRow, iDepartment].Text = "Department";
                    sheet10.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet10.Range[xlsRow, iSection].Text = "Section";
                    sheet10.Range[xlsRow, iSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet10.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet10.Range[xlsRow, iSubSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet10.Range[xlsRow, iEntity].Text = "Entity";
                    sheet10.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet10.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet10.Range[xlsRow, iDOJ].ColumnWidth = 12;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet10.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet10.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet10.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet10.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet10.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet10.Range[xlsRow, iShiftName].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet10.Range[xlsRow, iShiftInTime].Text = "Shift In Time";
                    sheet10.Range[xlsRow, iShiftInTime].ColumnWidth = 10;
                    sheet10.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet10.Range[xlsRow, iShiftOutTime].Text = "Shift Out Time";
                    sheet10.Range[xlsRow, iShiftOutTime].ColumnWidth = 10;
                    sheet10.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iInTime = xlsCol;
                    sheet10.Range[xlsRow, iInTime].Text = "In Time";
                    sheet10.Range[xlsRow, iInTime].ColumnWidth = 14;
                    sheet10.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet10.Range[xlsRow, iOutTime].Text = "Out Time";
                    sheet10.Range[xlsRow, iOutTime].ColumnWidth = 14;
                    sheet10.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iDuration = xlsCol;
                    sheet10.Range[xlsRow, iDuration].Text = "Shift Duration(Min)";
                    sheet10.Range[xlsRow, iDuration].ColumnWidth = 14;
                    sheet10.Range[xlsRow, iDuration].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iDuration].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet10.Range[xlsRow, iDuration].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iWorkDuration = xlsCol;
                    sheet10.Range[xlsRow, iWorkDuration].Text = "Work Duration(Min)";
                    sheet10.Range[xlsRow, iWorkDuration].ColumnWidth = 14;
                    sheet10.Range[xlsRow, iWorkDuration].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iWorkDuration].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet10.Range[xlsRow, iWorkDuration].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iWorkTimeDifferent = xlsCol;
                    sheet10.Range[xlsRow, iWorkTimeDifferent].Text = "Over Stay(Min)";
                    sheet10.Range[xlsRow, iWorkTimeDifferent].ColumnWidth = 14;
                    sheet10.Range[xlsRow, iWorkTimeDifferent].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iWorkTimeDifferent].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet10.Range[xlsRow, iWorkTimeDifferent].CellStyle.Font.Color = ExcelKnownColors.Red;


                    xlsCol += 1;
                    iDurationHour = xlsCol;
                    sheet10.Range[xlsRow, iDurationHour].Text = "Shift Duration(Hour)";
                    sheet10.Range[xlsRow, iDurationHour].ColumnWidth = 14;
                    sheet10.Range[xlsRow, iDurationHour].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iDurationHour].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet10.Range[xlsRow, iDurationHour].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iWorkDurationHour = xlsCol;
                    sheet10.Range[xlsRow, iWorkDurationHour].Text = "Work Duration(Hour)";
                    sheet10.Range[xlsRow, iWorkDurationHour].ColumnWidth = 14;
                    sheet10.Range[xlsRow, iWorkDurationHour].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iWorkDurationHour].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet10.Range[xlsRow, iWorkDurationHour].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iWorkTimeDifferentHour = xlsCol;
                    sheet10.Range[xlsRow, iWorkTimeDifferentHour].Text = "Over Stay(Hour)";
                    sheet10.Range[xlsRow, iWorkTimeDifferentHour].ColumnWidth = 14;
                    sheet10.Range[xlsRow, iWorkTimeDifferentHour].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet10.Range[xlsRow, iWorkTimeDifferentHour].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet10.Range[xlsRow, iWorkTimeDifferentHour].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet10.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //sheet10.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet10.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet10.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet10.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtWorkDuration.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtWorkDuration.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet10.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet10.Range[xlsRow, iEmployeeCode].Text = dtWorkDuration.Rows[i]["EmployeeCode"].ToString();

                            sheet10.Range[xlsRow, iEmployeeName].Text = dtWorkDuration.Rows[i]["EmployeeName"].ToString();

                            sheet10.Range[xlsRow, iDOJ].Text = dtWorkDuration.Rows[i]["DOJ"].ToString();
                            sheet10.Range[xlsRow, iEmployeeCategory].Text = dtWorkDuration.Rows[i]["EmployeeCategory"].ToString();
                            sheet10.Range[xlsRow, iDayStatus].Text = dtWorkDuration.Rows[i]["DayStatus"].ToString();
                            sheet10.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet10.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (bplib.clsWebLib.GetBoolData(dtWorkDuration.Rows[i]["IsManualDayStatus"].ToString().Trim()))
                            {
                                sheet10.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Orange;
                            }

                            sheet10.Range[xlsRow, iDesignation].Text = dtWorkDuration.Rows[i]["LegalDesignation"].ToString();

                            sheet10.Range[xlsRow, iDepartment].Text = dtWorkDuration.Rows[i]["Department"].ToString();

                            sheet10.Range[xlsRow, iSection].Text = dtWorkDuration.Rows[i]["Section"].ToString();

                            sheet10.Range[xlsRow, iEmployeeCurrentStatus].Text = dtWorkDuration.Rows[i]["EmployeeCurrentStatus"].ToString();

                            sheet10.Range[xlsRow, iSubSection].Text = dtWorkDuration.Rows[i]["SubSection"].ToString();
                            sheet10.Range[xlsRow, iEntity].Text = dtWorkDuration.Rows[i]["EntityName"].ToString();

                            sheet10.Range[xlsRow, iShiftName].Text = dtWorkDuration.Rows[i]["ShiftName"].ToString();

                            sheet10.Range[xlsRow, iShiftInTime].Text = dtWorkDuration.Rows[i]["ShiftInTime"].ToString();

                            sheet10.Range[xlsRow, iShiftOutTime].Text = dtWorkDuration.Rows[i]["ShiftOutTime"].ToString();

                            sheet10.Range[xlsRow, iWorkDate].Text = dtWorkDuration.Rows[i]["WorkDate"].ToString();

                            sheet10.Range[xlsRow, iDuration].Text = dtWorkDuration.Rows[i]["ShiftDuration"].ToString();
                            sheet10.Range[xlsRow, iDuration].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet10.Range[xlsRow, iDuration].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet10.Range[xlsRow, iDurationHour].Text = dtWorkDuration.Rows[i]["ShiftDurationHour"].ToString();
                            sheet10.Range[xlsRow, iDurationHour].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet10.Range[xlsRow, iDurationHour].VerticalAlignment = ExcelVAlign.VAlignCenter;


                            if (dtWorkDuration.Rows[i]["InTime"].ToString() != "")
                            {
                                sheet10.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                sheet10.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtWorkDuration.Rows[i]["InTime"].ToString());
                                sheet10.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet10.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtWorkDuration.Rows[i]["IsManualInTime"].ToString().Trim()))
                                {
                                    sheet10.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }

                            if (dtWorkDuration.Rows[i]["OutTime"].ToString() != "")
                            {
                                sheet10.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                sheet10.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(dtWorkDuration.Rows[i]["OutTime"].ToString());
                                sheet10.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet10.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtWorkDuration.Rows[i]["IsManualOutTime"].ToString().Trim()))
                                {
                                    sheet10.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }

                            }

                            sheet10.Range[xlsRow, iWorkDuration].Text = dtWorkDuration.Rows[i]["WorkDuration"].ToString();
                            sheet10.Range[xlsRow, iWorkDuration].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet10.Range[xlsRow, iWorkDuration].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet10.Range[xlsRow, iWorkDurationHour].Text = dtWorkDuration.Rows[i]["WorkDurationHour"].ToString();
                            sheet10.Range[xlsRow, iWorkDurationHour].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet10.Range[xlsRow, iWorkDurationHour].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet10.Range[xlsRow, iWorkTimeDifferent].Text = dtWorkDuration.Rows[i]["WorkTimeDifferent"].ToString();
                            sheet10.Range[xlsRow, iWorkTimeDifferent].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet10.Range[xlsRow, iWorkTimeDifferent].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet10.Range[xlsRow, iWorkTimeDifferentHour].Text = dtWorkDuration.Rows[i]["WorkTimeDifferentHour"].ToString();
                            sheet10.Range[xlsRow, iWorkTimeDifferentHour].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet10.Range[xlsRow, iWorkTimeDifferentHour].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet10.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet10.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet10.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet10.GetColumnWidth(1) + sheet10.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet10.GetRowHeight(1) + sheet10.GetRowHeight(2) + sheet10.GetRowHeight(3) + sheet10.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet10.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet10.Range[xlsRow, 3].Text = CmpName;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet10.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet10.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet10.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet10.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {

                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet10.Range[xlsRow, 3].Text = FactoryName;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet10.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet10.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet10.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet10.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet10.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet10.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet10.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet10.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Short Duration: " + FromDate + " To : " + ToDate;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet10.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet10.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet10.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet10.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet10.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************                    

                    #region Freeze Panes

                    sheet10.IsDisplayZeros = false;
                    sheet10.UsedRange["A7"].FreezePanes();
                    sheet10.FirstVisibleColumn = 1;
                    sheet10.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet10.UsedRange.WrapText = true;
                    sheet10.UsedRange.CellStyle.Font.Size = 8;
                    sheet10.Range["A1"].CellStyle.Font.Size = 14;
                    sheet10.Range["A2"].CellStyle.Font.Size = 10;
                    sheet10.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet10.PageSetup.TopMargin = 0.5;
                    sheet10.PageSetup.BottomMargin = 0.7;
                    sheet10.PageSetup.PrintTitleRows = "$1:$5";
                    sheet10.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet10.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet10.PageSetup.LeftMargin = 0.5;
                    sheet10.PageSetup.RightMargin = 0.2;
                    sheet10.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet10.PageSetup.FitToPagesTall = 0;
                    sheet10.PageSetup.FitToPagesWide = 1;
                    sheet10.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet10.IsDisplayZeros = false;

                    if (dtWorkDuration.Rows.Count > 0)
                    {
                        sheet10.Name = (SheetIndex + 1) + "_Short_Duration";
                        sheet10.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet10.Name = (SheetIndex + 1) + "_Short_Duration";
                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {
                }
                #endregion short duration

                #region  OT Applicable And Out Missing 6

                try
                {
                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    IWorksheet sheet15 = null;
                    SheetIndex++;
                    sheet15 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet15.Range[5, igoto].Text = "Goto Index";
                    sheet15.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromotnotapplicableandoutmissing = sheet15.HyperLinks.Add(sheet15.Range[5, igoto]);
                    linkgofromotnotapplicableandoutmissing.Type = ExcelHyperLinkType.Workbook;
                    linkgofromotnotapplicableandoutmissing.TextToDisplay = sheet15.Range[5, igoto].Text;
                    linkgofromotnotapplicableandoutmissing.ScreenTip = "Go To " + sheet15.Range[5, igoto].Text;
                    linkgofromotnotapplicableandoutmissing.Address = "1_Index!A1";


                    isl = xlsCol;
                    sheet15.Range[xlsRow, isl].Text = "SL";
                    sheet15.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet15.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet15.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet15.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet15.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iDayStatus = xlsCol;
                    sheet15.Range[xlsRow, iDayStatus].Text = "Day Status";
                    sheet15.Range[xlsRow, iDayStatus].ColumnWidth = 18;
                    sheet15.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet15.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet15.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet15.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iTelephoneNo = xlsCol;
                    sheet15.Range[xlsRow, iTelephoneNo].Text = "Telephone No.";
                    sheet15.Range[xlsRow, iTelephoneNo].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet15.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet15.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet15.Range[xlsRow, iDepartment].Text = "Department";
                    sheet15.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet15.Range[xlsRow, iSection].Text = "Section";
                    sheet15.Range[xlsRow, iSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet15.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet15.Range[xlsRow, iSubSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet15.Range[xlsRow, iEntity].Text = "Entity";
                    sheet15.Range[xlsRow, iEntity].ColumnWidth = 18;



                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet15.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet15.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet15.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet15.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet15.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet15.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet15.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet15.Range[xlsRow, iShiftName].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet15.Range[xlsRow, iShiftInTime].Text = "Shift In Time";
                    sheet15.Range[xlsRow, iShiftInTime].ColumnWidth = 10;
                    sheet15.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet15.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet15.Range[xlsRow, iShiftOutTime].Text = "Shift Out Time";
                    sheet15.Range[xlsRow, iShiftOutTime].ColumnWidth = 10;
                    sheet15.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet15.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iInTime = xlsCol;
                    sheet15.Range[xlsRow, iInTime].Text = "In Time";
                    sheet15.Range[xlsRow, iInTime].ColumnWidth = 14;
                    sheet15.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet15.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet15.Range[xlsRow, iOutTime].Text = "Out Time";
                    sheet15.Range[xlsRow, iOutTime].ColumnWidth = 14;
                    sheet15.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet15.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet15.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iRawPunch = xlsCol;
                    sheet15.Range[xlsRow, iRawPunch].Text = "Raw Punch";
                    sheet15.Range[xlsRow, iRawPunch].ColumnWidth = 28;
                    sheet15.Range[xlsRow, iRawPunch].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet15.Range[xlsRow, iRawPunch].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet15.Range[xlsRow, iRawPunch].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet15.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //sheet15.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet15.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet15.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet15.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtOTEntitledWithOutMissing.Rows.Count > 0)
                    {

                        SLNo = 1;
                        for (int i = 0; i < dtOTEntitledWithOutMissing.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------

                            sheet15.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet15.Range[xlsRow, iEmployeeCode].Text = dtOTEntitledWithOutMissing.Rows[i]["EmployeeCode"].ToString();

                            //sheet15.Range[xlsRow, iRawPunch].Text = dtOTEntitledWithOutMissing.Rows[i]["RawPunch"].ToString();


                            sheet15.Range[xlsRow, iEmployeeName].Text = dtOTEntitledWithOutMissing.Rows[i]["EmployeeName"].ToString();
                            sheet15.Range[xlsRow, iTelephoneNo].Text = dtOTEntitledWithOutMissing.Rows[i]["TelePhnNo"].ToString();

                            sheet15.Range[xlsRow, iDayStatus].Text = dtOTEntitledWithOutMissing.Rows[i]["DayStatus"].ToString();
                            sheet15.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet15.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (bplib.clsWebLib.GetBoolData(dtOTEntitledWithOutMissing.Rows[i]["IsManualDayStatus"].ToString().Trim()))
                            {
                                sheet15.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Orange;
                            }

                            sheet15.Range[xlsRow, iDesignation].Text = dtOTEntitledWithOutMissing.Rows[i]["LegalDesignation"].ToString();

                            sheet15.Range[xlsRow, iEmployeeCurrentStatus].Text = dtOTEntitledWithOutMissing.Rows[i]["EmployeeCurrentStatus"].ToString();
                            sheet15.Range[xlsRow, iEmployeeCategory].Text = dtOTEntitledWithOutMissing.Rows[i]["EmployeeCategory"].ToString();
                            sheet15.Range[xlsRow, iDepartment].Text = dtOTEntitledWithOutMissing.Rows[i]["Department"].ToString();

                            sheet15.Range[xlsRow, iSection].Text = dtOTEntitledWithOutMissing.Rows[i]["Section"].ToString();

                            sheet15.Range[xlsRow, iSubSection].Text = dtOTEntitledWithOutMissing.Rows[i]["SubSection"].ToString();
                            sheet15.Range[xlsRow, iEntity].Text = dtOTEntitledWithOutMissing.Rows[i]["EntityName"].ToString();

                            sheet15.Range[xlsRow, iWorkDate].Text = dtOTEntitledWithOutMissing.Rows[i]["WorkDate"].ToString();

                            sheet15.Range[xlsRow, iDOJ].Text = dtOTEntitledWithOutMissing.Rows[i]["DOJ"].ToString();

                            sheet15.Range[xlsRow, iShiftName].Text = dtOTEntitledWithOutMissing.Rows[i]["ShiftName"].ToString();

                            sheet15.Range[xlsRow, iShiftInTime].Text = dtOTEntitledWithOutMissing.Rows[i]["ShiftInTime"].ToString();
                            sheet15.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet15.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet15.Range[xlsRow, iShiftOutTime].Text = dtOTEntitledWithOutMissing.Rows[i]["ShiftOutTime"].ToString();
                            sheet15.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet15.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (dtOTEntitledWithOutMissing.Rows[i]["InTime"].ToString() != "")
                            {
                                sheet15.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                sheet15.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtOTEntitledWithOutMissing.Rows[i]["InTime"].ToString());
                                sheet15.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet15.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtOTEntitledWithOutMissing.Rows[i]["IsManualInTime"].ToString().Trim()))
                                {
                                    sheet15.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }

                            }

                            if (dtOTEntitledWithOutMissing.Rows[i]["OutTime"].ToString() != "")
                            {
                                sheet15.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                sheet15.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(dtOTEntitledWithOutMissing.Rows[i]["OutTime"].ToString());
                                sheet15.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet15.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtOTEntitledWithOutMissing.Rows[i]["IsManualOutTime"].ToString().Trim()))
                                {
                                    sheet15.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet15.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet15.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet15.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                        xlsRow++;
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "OT Applicable: YES ";
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "Day Status: P";
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "In Time: Not Blank";
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "Out Time: Blank";
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet15.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet15.GetColumnWidth(1) + sheet15.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet15.GetRowHeight(1) + sheet15.GetRowHeight(2) + sheet15.GetRowHeight(3) + sheet15.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet15.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet15.Range[xlsRow, 3].Text = CmpName;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet15.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet15.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet15.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet15.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet15.Range[xlsRow, 3].Text = FactoryName;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet15.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet15.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet15.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet15.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet15.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet15.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet15.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet15.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-OT Applicable And Out Missing: " + FromDate + " To Date: " + ToDate;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet15.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet15.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet15.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet15.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet15.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet15.IsDisplayZeros = false;
                    sheet15.UsedRange["A7"].FreezePanes();
                    sheet15.FirstVisibleColumn = 1;
                    sheet15.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet15.UsedRange.WrapText = true;
                    sheet15.UsedRange.CellStyle.Font.Size = 8;
                    sheet15.Range["A1"].CellStyle.Font.Size = 14;
                    sheet15.Range["A2"].CellStyle.Font.Size = 10;
                    sheet15.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet15.PageSetup.TopMargin = 0.5;
                    sheet15.PageSetup.BottomMargin = 0.7;
                    sheet15.PageSetup.PrintTitleRows = "$1:$5";
                    sheet15.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet15.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet15.PageSetup.LeftMargin = 0.5;
                    sheet15.PageSetup.RightMargin = 0.2;
                    sheet15.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet15.PageSetup.FitToPagesTall = 0;
                    sheet15.PageSetup.FitToPagesWide = 1;
                    sheet15.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet15.IsDisplayZeros = false;

                    if (dtOTEntitledWithOutMissing.Rows.Count > 0)
                    {
                        sheet15.Name = (SheetIndex + 1) + "_OT_Applicable_And_Out_Missing";
                        sheet15.TabColorRGB = Color.Red;

                    }
                    else
                    {
                        sheet15.Name = (SheetIndex + 1) + "_OT_Applicable_And_Out_Missing";
                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {

                }
                #endregion  OT Applicable And Out Missing

                #region  OT Not Applicable And Out Missing 7
                try
                {
                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    IWorksheet sheet16 = null;
                    SheetIndex++;
                    sheet16 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------

                    igoto = xlsCol;
                    sheet16.Range[5, igoto].Text = "Goto Index";
                    sheet16.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromotnotapplicable = sheet16.HyperLinks.Add(sheet16.Range[5, igoto]);
                    linkgofromotnotapplicable.Type = ExcelHyperLinkType.Workbook;
                    linkgofromotnotapplicable.TextToDisplay = sheet16.Range[5, igoto].Text;
                    linkgofromotnotapplicable.ScreenTip = "Go To " + sheet16.Range[5, igoto].Text;
                    linkgofromotnotapplicable.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet16.Range[xlsRow, isl].Text = "SL";
                    sheet16.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet16.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet16.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet16.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet16.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iDayStatus = xlsCol;
                    sheet16.Range[xlsRow, iDayStatus].Text = "Day Status";
                    sheet16.Range[xlsRow, iDayStatus].ColumnWidth = 18;
                    sheet16.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet16.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet16.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet16.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iTelephoneNo = xlsCol;
                    sheet16.Range[xlsRow, iTelephoneNo].Text = "Telephone No.";
                    sheet16.Range[xlsRow, iTelephoneNo].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet16.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet16.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet16.Range[xlsRow, iDepartment].Text = "Department";
                    sheet16.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet16.Range[xlsRow, iSection].Text = "Section";
                    sheet16.Range[xlsRow, iSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet16.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet16.Range[xlsRow, iSubSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet16.Range[xlsRow, iEntity].Text = "Entity";
                    sheet16.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet16.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet16.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet16.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet16.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet16.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet16.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet16.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet16.Range[xlsRow, iShiftName].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet16.Range[xlsRow, iShiftInTime].Text = "Shift In Time";
                    sheet16.Range[xlsRow, iShiftInTime].ColumnWidth = 10;
                    sheet16.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet16.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet16.Range[xlsRow, iShiftOutTime].Text = "Shift Out Time";
                    sheet16.Range[xlsRow, iShiftOutTime].ColumnWidth = 10;
                    sheet16.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet16.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iInTime = xlsCol;
                    sheet16.Range[xlsRow, iInTime].Text = "In Time";
                    sheet16.Range[xlsRow, iInTime].ColumnWidth = 14;
                    sheet16.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet16.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet16.Range[xlsRow, iOutTime].Text = "Out Time";
                    sheet16.Range[xlsRow, iOutTime].ColumnWidth = 14;
                    sheet16.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet16.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet16.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet16.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    // sheet16.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet16.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet16.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet16.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtOTNotEntitledWithOutMissing.Rows.Count > 0)
                    {

                        SLNo = 1;
                        for (int i = 0; i < dtOTNotEntitledWithOutMissing.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet16.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet16.Range[xlsRow, iEmployeeCode].Text = dtOTNotEntitledWithOutMissing.Rows[i]["EmployeeCode"].ToString();

                            //sheet16.Range[xlsRow, iRawPunch].Text = dtOTNotEntitledWithOutMissing.Rows[i]["RawPunch"].ToString();


                            sheet16.Range[xlsRow, iEmployeeName].Text = dtOTNotEntitledWithOutMissing.Rows[i]["EmployeeName"].ToString();
                            sheet16.Range[xlsRow, iTelephoneNo].Text = dtOTNotEntitledWithOutMissing.Rows[i]["TelePhnNo"].ToString();

                            sheet16.Range[xlsRow, iDayStatus].Text = dtOTNotEntitledWithOutMissing.Rows[i]["DayStatus"].ToString();
                            sheet16.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet16.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (bplib.clsWebLib.GetBoolData(dtOTNotEntitledWithOutMissing.Rows[i]["IsManualDayStatus"].ToString().Trim()))
                            {
                                sheet16.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Orange;
                            }

                            sheet16.Range[xlsRow, iEmployeeCategory].Text = dtOTNotEntitledWithOutMissing.Rows[i]["EmployeeCategory"].ToString();

                            sheet16.Range[xlsRow, iDesignation].Text = dtOTNotEntitledWithOutMissing.Rows[i]["LegalDesignation"].ToString();

                            sheet16.Range[xlsRow, iDepartment].Text = dtOTNotEntitledWithOutMissing.Rows[i]["Department"].ToString();

                            sheet16.Range[xlsRow, iEmployeeCurrentStatus].Text = dtOTNotEntitledWithOutMissing.Rows[i]["EmployeeCurrentStatus"].ToString();

                            sheet16.Range[xlsRow, iSection].Text = dtOTNotEntitledWithOutMissing.Rows[i]["Section"].ToString();

                            sheet16.Range[xlsRow, iSubSection].Text = dtOTNotEntitledWithOutMissing.Rows[i]["SubSection"].ToString();
                            sheet16.Range[xlsRow, iEntity].Text = dtOTNotEntitledWithOutMissing.Rows[i]["EntityName"].ToString();

                            sheet16.Range[xlsRow, iWorkDate].Text = dtOTNotEntitledWithOutMissing.Rows[i]["WorkDate"].ToString();

                            sheet16.Range[xlsRow, iDOJ].Text = dtOTNotEntitledWithOutMissing.Rows[i]["DOJ"].ToString();

                            sheet16.Range[xlsRow, iShiftName].Text = dtOTNotEntitledWithOutMissing.Rows[i]["ShiftName"].ToString();

                            sheet16.Range[xlsRow, iShiftInTime].Text = dtOTNotEntitledWithOutMissing.Rows[i]["ShiftInTime"].ToString();
                            sheet16.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet16.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet16.Range[xlsRow, iShiftOutTime].Text = dtOTNotEntitledWithOutMissing.Rows[i]["ShiftOutTime"].ToString();
                            sheet16.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet16.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (dtOTNotEntitledWithOutMissing.Rows[i]["InTime"].ToString() != "")
                            {
                                sheet16.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                sheet16.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtOTNotEntitledWithOutMissing.Rows[i]["InTime"].ToString());
                                sheet16.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet16.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (clsWebLib.GetBoolData(dtOTNotEntitledWithOutMissing.Rows[i]["IsManualInTime"].ToString().Trim()))
                                {
                                    sheet16.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }

                            }

                            if (dtOTNotEntitledWithOutMissing.Rows[i]["OutTime"].ToString() != "")
                            {
                                sheet16.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                sheet16.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(dtOTNotEntitledWithOutMissing.Rows[i]["OutTime"].ToString());
                                sheet16.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet16.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtOTNotEntitledWithOutMissing.Rows[i]["IsManualOutTime"].ToString().Trim()))
                                {
                                    sheet16.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet16.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet16.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet16.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                        xlsRow++;
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "OT Applicable: NO ";
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "Day Status: P";
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "In Time: Not Blank";
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "Out Time: Blank";
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet16.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;
                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet16.GetColumnWidth(1) + sheet16.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet16.GetRowHeight(1) + sheet16.GetRowHeight(2) + sheet16.GetRowHeight(3) + sheet16.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet16.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet16.Range[xlsRow, 3].Text = CmpName;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet16.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet16.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet16.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet16.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet16.Range[xlsRow, 3].Text = FactoryName;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet16.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet16.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet16.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet16.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet16.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet16.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet16.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    sheet16.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-OT Not Applicable And Out Missing: " + FromDate + " To Date: " + ToDate;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet16.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet16.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet16.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet16.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet16.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet16.IsDisplayZeros = false;
                    sheet16.UsedRange["A7"].FreezePanes();
                    sheet16.FirstVisibleColumn = 1;
                    sheet16.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet16.UsedRange.WrapText = true;
                    sheet16.UsedRange.CellStyle.Font.Size = 8;
                    sheet16.Range["A1"].CellStyle.Font.Size = 14;
                    sheet16.Range["A2"].CellStyle.Font.Size = 10;
                    sheet16.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet16.PageSetup.TopMargin = 0.5;
                    sheet16.PageSetup.BottomMargin = 0.7;
                    sheet16.PageSetup.PrintTitleRows = "$1:$5";
                    sheet16.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet16.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet16.PageSetup.LeftMargin = 0.5;
                    sheet16.PageSetup.RightMargin = 0.2;
                    sheet16.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet16.PageSetup.FitToPagesTall = 0;
                    sheet16.PageSetup.FitToPagesWide = 1;
                    sheet16.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet16.IsDisplayZeros = false;

                    if (dtOTNotEntitledWithOutMissing.Rows.Count > 0)
                    {
                        sheet16.Name = (SheetIndex + 1) + "_OT_Not_Applicable_And_Out_Mis";
                        sheet16.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet16.Name = (SheetIndex + 1) + "_OT_Not_Applicable_And_Out_Mis";

                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {

                }
                #endregion  OT Not Applicable And Out Missing

                #region  UnAproved Profile 8
                try
                {
                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    IWorksheet sheet7 = null;
                    SheetIndex++;
                    sheet7 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------

                    igoto = xlsCol;
                    sheet7.Range[5, igoto].Text = "Goto Index";
                    sheet7.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromUnAprovedProfile = sheet7.HyperLinks.Add(sheet7.Range[5, igoto]);
                    linkgofromUnAprovedProfile.Type = ExcelHyperLinkType.Workbook;
                    linkgofromUnAprovedProfile.TextToDisplay = sheet7.Range[5, igoto].Text;
                    linkgofromUnAprovedProfile.ScreenTip = "Go To " + sheet7.Range[5, igoto].Text;
                    linkgofromUnAprovedProfile.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet7.Range[xlsRow, isl].Text = "SL";
                    sheet7.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet7.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet7.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet7.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet7.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet7.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet7.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet7.Range[xlsRow, iDepartment].Text = "Department";
                    sheet7.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet7.Range[xlsRow, iSection].Text = "Section";
                    sheet7.Range[xlsRow, iSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet7.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet7.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet7.Range[xlsRow, iEntity].Text = "Entity";
                    sheet7.Range[xlsRow, iEntity].ColumnWidth = 18;


                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet7.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet7.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet7.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet7.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet7.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet7.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iLine = xlsCol;
                    sheet7.Range[xlsRow, iLine].Text = "Line";
                    sheet7.Range[xlsRow, iLine].ColumnWidth = 18;

                    sheet7.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //sheet7.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet7.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet7.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet7.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtUnApprovedProfile.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtUnApprovedProfile.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet7.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet7.Range[xlsRow, iEmployeeCode].Text = dtUnApprovedProfile.Rows[i]["EmployeeCode"].ToString();

                            sheet7.Range[xlsRow, iEmployeeName].Text = dtUnApprovedProfile.Rows[i]["EmployeeName"].ToString();

                            sheet7.Range[xlsRow, iDesignation].Text = dtUnApprovedProfile.Rows[i]["LegalDesignation"].ToString();

                            sheet7.Range[xlsRow, iDepartment].Text = dtUnApprovedProfile.Rows[i]["Department"].ToString();

                            sheet7.Range[xlsRow, iEmployeeCurrentStatus].Text = dtUnApprovedProfile.Rows[i]["EmployeeCurrentStatus"].ToString();

                            sheet7.Range[xlsRow, iSection].Text = dtUnApprovedProfile.Rows[i]["Section"].ToString();

                            sheet7.Range[xlsRow, iSubSection].Text = dtUnApprovedProfile.Rows[i]["SubSection"].ToString();

                            sheet7.Range[xlsRow, iEntity].Text = dtUnApprovedProfile.Rows[i]["EntityName"].ToString();
                            sheet7.Range[xlsRow, iLine].Text = dtUnApprovedProfile.Rows[i]["Line"].ToString();

                            sheet7.Range[xlsRow, iDOJ].Text = dtUnApprovedProfile.Rows[i]["DOJ"].ToString();

                            sheet7.Range[xlsRow, iEmployeeCategory].Text = dtUnApprovedProfile.Rows[i]["EmployeeCategory"].ToString();

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet7.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet7.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet7.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup


                        sheet7.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    }
                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet7.GetColumnWidth(1) + sheet7.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet7.GetRowHeight(1) + sheet7.GetRowHeight(2) + sheet7.GetRowHeight(3) + sheet7.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet7.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet7.Range[xlsRow, 3].Text = CmpName;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet7.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet7.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet7.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet7.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet7.Range[xlsRow, 3].Text = FactoryName;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet7.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet7.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet7.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet7.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet7.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet7.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet7.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet7.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Un Approved Profile: " + FromDate + " To Date: " + ToDate;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet7.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet7.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet7.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet7.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet7.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet7.IsDisplayZeros = false;
                    sheet7.UsedRange["A7"].FreezePanes();
                    sheet7.FirstVisibleColumn = 1;
                    sheet7.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet7.UsedRange.WrapText = true;
                    sheet7.UsedRange.CellStyle.Font.Size = 8;
                    sheet7.Range["A1"].CellStyle.Font.Size = 14;
                    sheet7.Range["A2"].CellStyle.Font.Size = 10;
                    sheet7.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet7.PageSetup.TopMargin = 0.5;
                    sheet7.PageSetup.BottomMargin = 0.7;
                    sheet7.PageSetup.PrintTitleRows = "$1:$5";
                    sheet7.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet7.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet7.PageSetup.LeftMargin = 0.5;
                    sheet7.PageSetup.RightMargin = 0.2;
                    sheet7.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet7.PageSetup.FitToPagesTall = 0;
                    sheet7.PageSetup.FitToPagesWide = 1;
                    sheet7.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet7.IsDisplayZeros = false;

                    if (dtUnApprovedProfile.Rows.Count > 0)
                    {
                        sheet7.Name = (SheetIndex + 1) + "_Un_Approved_Profile";
                        sheet7.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet7.Name = (SheetIndex + 1) + "_Un_Approved_Profile";

                    }
                    #endregion Page Setup


                }
                catch (Exception ex)
                {

                }
                #endregion   UnAproved Profile 

                #region  No Salary Structure  9
                try
                {
                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    IWorksheet sheet8 = null;
                    SheetIndex++;
                    sheet8 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet8.Range[5, igoto].Text = "Goto Index";
                    sheet8.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromNoSalaryStructure = sheet8.HyperLinks.Add(sheet8.Range[5, igoto]);
                    linkgofromNoSalaryStructure.Type = ExcelHyperLinkType.Workbook;
                    linkgofromNoSalaryStructure.TextToDisplay = sheet8.Range[5, igoto].Text;
                    linkgofromNoSalaryStructure.ScreenTip = "Go To " + sheet8.Range[5, igoto].Text;
                    linkgofromNoSalaryStructure.Address = "1_Index!A1";


                    isl = xlsCol;
                    sheet8.Range[xlsRow, isl].Text = "SL";
                    sheet8.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet8.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet8.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet8.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet8.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet8.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet8.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet8.Range[xlsRow, iDepartment].Text = "Department";
                    sheet8.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet8.Range[xlsRow, iSection].Text = "Section";
                    sheet8.Range[xlsRow, iSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet8.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet8.Range[xlsRow, iSubSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet8.Range[xlsRow, iEntity].Text = "Entity";
                    sheet8.Range[xlsRow, iEntity].ColumnWidth = 18;


                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet8.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet8.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet8.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet8.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet8.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet8.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iLine = xlsCol;
                    sheet8.Range[xlsRow, iLine].Text = "Line";
                    sheet8.Range[xlsRow, iLine].ColumnWidth = 18;

                    sheet8.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //sheet8.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet8.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet8.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet8.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    if (dtProfileNoSalary.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtProfileNoSalary.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet8.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet8.Range[xlsRow, iEmployeeCode].Text = dtProfileNoSalary.Rows[i]["EmployeeCode"].ToString();

                            sheet8.Range[xlsRow, iEmployeeName].Text = dtProfileNoSalary.Rows[i]["EmployeeName"].ToString();

                            sheet8.Range[xlsRow, iDesignation].Text = dtProfileNoSalary.Rows[i]["LegalDesignation"].ToString();

                            sheet8.Range[xlsRow, iEmployeeCurrentStatus].Text = dtProfileNoSalary.Rows[i]["EmployeeCurrentStatus"].ToString();

                            sheet8.Range[xlsRow, iDepartment].Text = dtProfileNoSalary.Rows[i]["Department"].ToString();

                            sheet8.Range[xlsRow, iSection].Text = dtProfileNoSalary.Rows[i]["Section"].ToString();

                            sheet8.Range[xlsRow, iSubSection].Text = dtProfileNoSalary.Rows[i]["SubSection"].ToString();

                            sheet8.Range[xlsRow, iEntity].Text = dtProfileNoSalary.Rows[i]["EntityName"].ToString();

                            sheet8.Range[xlsRow, iLine].Text = dtProfileNoSalary.Rows[i]["Line"].ToString();

                            sheet8.Range[xlsRow, iDOJ].Text = dtProfileNoSalary.Rows[i]["DOJ"].ToString();

                            sheet8.Range[xlsRow, iEmployeeCategory].Text = dtProfileNoSalary.Rows[i]["EmployeeCategory"].ToString();

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet8.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet8.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet8.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup


                        sheet8.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                    }
                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet8.GetColumnWidth(1) + sheet8.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet8.GetRowHeight(1) + sheet8.GetRowHeight(2) + sheet8.GetRowHeight(3) + sheet8.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet8.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet8.Range[xlsRow, 3].Text = CmpName;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet8.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet8.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet8.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet8.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet8.Range[xlsRow, 3].Text = FactoryName;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet8.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet8.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet8.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet8.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet8.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet8.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet8.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet8.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-No Salary Structure: " + FromDate + " To Date: " + ToDate;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet8.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet8.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet8.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet8.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet8.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet8.IsDisplayZeros = false;
                    sheet8.UsedRange["A7"].FreezePanes();
                    sheet8.FirstVisibleColumn = 1;
                    sheet8.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet8.UsedRange.WrapText = true;
                    sheet8.UsedRange.CellStyle.Font.Size = 8;
                    sheet8.Range["A1"].CellStyle.Font.Size = 14;
                    sheet8.Range["A2"].CellStyle.Font.Size = 10;
                    sheet8.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet8.PageSetup.TopMargin = 0.5;
                    sheet8.PageSetup.BottomMargin = 0.7;
                    sheet8.PageSetup.PrintTitleRows = "$1:$5";
                    sheet8.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet8.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet8.PageSetup.LeftMargin = 0.5;
                    sheet8.PageSetup.RightMargin = 0.2;
                    sheet8.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet8.PageSetup.FitToPagesTall = 0;
                    sheet8.PageSetup.FitToPagesWide = 1;
                    sheet8.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet8.IsDisplayZeros = false;

                    if (dtProfileNoSalary.Rows.Count > 0)
                    {
                        sheet8.Name = (SheetIndex + 1) + "_No_Salary_Structure";
                        sheet8.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet8.Name = (SheetIndex + 1) + "_No_Salary_Structure";
                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {

                }
                #endregion   No Salary Structure

                #region  Salary Structure Not Approved 10

                try
                {
                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    IWorksheet sheet9 = null;
                    SheetIndex++;
                    sheet9 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet9.Range[5, igoto].Text = "Goto Index";
                    sheet9.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromSalaryStructureNotApproved = sheet9.HyperLinks.Add(sheet9.Range[5, igoto]);
                    linkgofromSalaryStructureNotApproved.Type = ExcelHyperLinkType.Workbook;
                    linkgofromSalaryStructureNotApproved.TextToDisplay = sheet9.Range[5, igoto].Text;
                    linkgofromSalaryStructureNotApproved.ScreenTip = "Go To " + sheet9.Range[5, igoto].Text;
                    linkgofromSalaryStructureNotApproved.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet9.Range[xlsRow, isl].Text = "SL";
                    sheet9.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet9.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet9.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet9.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet9.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet9.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet9.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet9.Range[xlsRow, iDepartment].Text = "Department";
                    sheet9.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet9.Range[xlsRow, iSection].Text = "Section";
                    sheet9.Range[xlsRow, iSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet9.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet9.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet9.Range[xlsRow, iEntity].Text = "Entity";
                    sheet9.Range[xlsRow, iEntity].ColumnWidth = 18;


                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet9.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet9.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet9.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet9.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet9.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet9.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iLine = xlsCol;
                    sheet9.Range[xlsRow, iLine].Text = "Line";
                    sheet9.Range[xlsRow, iLine].ColumnWidth = 18;

                    sheet9.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    // sheet9.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet9.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet9.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet9.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtNoSalaryStructureApprove.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtNoSalaryStructureApprove.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet9.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet9.Range[xlsRow, iEmployeeCode].Text = dtNoSalaryStructureApprove.Rows[i]["EmployeeCode"].ToString();

                            sheet9.Range[xlsRow, iEmployeeName].Text = dtNoSalaryStructureApprove.Rows[i]["EmployeeName"].ToString();

                            sheet9.Range[xlsRow, iDesignation].Text = dtNoSalaryStructureApprove.Rows[i]["LegalDesignation"].ToString();

                            sheet9.Range[xlsRow, iEmployeeCurrentStatus].Text = dtNoSalaryStructureApprove.Rows[i]["EmployeeCurrentStatus"].ToString();

                            sheet9.Range[xlsRow, iDepartment].Text = dtNoSalaryStructureApprove.Rows[i]["Department"].ToString();

                            sheet9.Range[xlsRow, iSection].Text = dtNoSalaryStructureApprove.Rows[i]["Section"].ToString();

                            sheet9.Range[xlsRow, iSubSection].Text = dtNoSalaryStructureApprove.Rows[i]["SubSection"].ToString();

                            sheet9.Range[xlsRow, iEntity].Text = dtNoSalaryStructureApprove.Rows[i]["EntityName"].ToString();

                            sheet9.Range[xlsRow, iLine].Text = dtNoSalaryStructureApprove.Rows[i]["Line"].ToString();

                            sheet9.Range[xlsRow, iDOJ].Text = dtNoSalaryStructureApprove.Rows[i]["DOJ"].ToString();

                            sheet9.Range[xlsRow, iEmployeeCategory].Text = dtNoSalaryStructureApprove.Rows[i]["EmployeeCategory"].ToString();

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet9.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet9.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet9.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                        sheet9.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;

                    }
                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet9.GetColumnWidth(1) + sheet9.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet9.GetRowHeight(1) + sheet9.GetRowHeight(2) + sheet9.GetRowHeight(3) + sheet9.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet9.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }

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
                    sheet9.Range[xlsRow, 3].Text = CmpName;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet9.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet9.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet9.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet9.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet9.Range[xlsRow, 3].Text = FactoryName;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet9.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet9.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet9.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet9.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet9.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet9.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet9.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet9.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Salary Structure Not Approved: As On" + ToDate;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet9.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet9.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet9.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet9.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet9.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet9.IsDisplayZeros = false;
                    sheet9.UsedRange["A7"].FreezePanes();
                    sheet9.FirstVisibleColumn = 1;
                    sheet9.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet9.UsedRange.WrapText = true;
                    sheet9.UsedRange.CellStyle.Font.Size = 8;
                    sheet9.Range["A1"].CellStyle.Font.Size = 14;
                    sheet9.Range["A2"].CellStyle.Font.Size = 10;
                    sheet9.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet9.PageSetup.TopMargin = 0.5;
                    sheet9.PageSetup.BottomMargin = 0.7;
                    sheet9.PageSetup.PrintTitleRows = "$1:$5";
                    sheet9.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet9.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet9.PageSetup.LeftMargin = 0.5;
                    sheet9.PageSetup.RightMargin = 0.2;
                    sheet9.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet9.PageSetup.FitToPagesTall = 0;
                    sheet9.PageSetup.FitToPagesWide = 1;
                    sheet9.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet9.IsDisplayZeros = false;

                    if (dtNoSalaryStructureApprove.Rows.Count > 0)
                    {
                        sheet9.Name = (SheetIndex + 1) + "_Salary_Structure_Not_Approved";
                        sheet9.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet9.Name = (SheetIndex + 1) + "_Salary_Structure_Not_Approved";
                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {

                }
                #endregion   Salary Structure Not Approved

                #region OT NOT CONFIRM OVERSTAY 11

                try
                {
                    IWorksheet sheet11 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet11 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet11.Range[5, igoto].Text = "Goto Index";
                    sheet11.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromOTNotConfirmOverstay = sheet11.HyperLinks.Add(sheet11.Range[5, igoto]);
                    linkgofromOTNotConfirmOverstay.Type = ExcelHyperLinkType.Workbook;
                    linkgofromOTNotConfirmOverstay.TextToDisplay = sheet11.Range[5, igoto].Text;
                    linkgofromOTNotConfirmOverstay.ScreenTip = "Go To " + sheet11.Range[5, igoto].Text;
                    linkgofromOTNotConfirmOverstay.Address = "1_Index!A1";


                    isl = xlsCol;
                    sheet11.Range[xlsRow, isl].Text = "SL";
                    sheet11.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet11.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet11.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet11.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet11.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iDayStatus = xlsCol;
                    sheet11.Range[xlsRow, iDayStatus].Text = "Day Status";
                    sheet11.Range[xlsRow, iDayStatus].ColumnWidth = 10;
                    sheet11.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet11.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet11.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet11.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet11.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet11.Range[xlsRow, iDesignation].ColumnWidth = 25;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet11.Range[xlsRow, iDepartment].Text = "Department";
                    sheet11.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet11.Range[xlsRow, iSection].Text = "Section";
                    sheet11.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet11.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet11.Range[xlsRow, iSubSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet11.Range[xlsRow, iEntity].Text = "Entity";
                    sheet11.Range[xlsRow, iEntity].ColumnWidth = 18;


                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet11.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet11.Range[xlsRow, iDOJ].ColumnWidth = 18;


                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet11.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet11.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet11.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet11.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet11.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet11.Range[xlsRow, iShiftName].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet11.Range[xlsRow, iShiftInTime].Text = "Shift In Time";
                    sheet11.Range[xlsRow, iShiftInTime].ColumnWidth = 10;
                    sheet11.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet11.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet11.Range[xlsRow, iShiftOutTime].Text = "Shift Out Time";
                    sheet11.Range[xlsRow, iShiftOutTime].ColumnWidth = 10;
                    sheet11.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet11.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iInTime = xlsCol;
                    sheet11.Range[xlsRow, iInTime].Text = "In Time";
                    sheet11.Range[xlsRow, iInTime].ColumnWidth = 14;
                    sheet11.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet11.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet11.Range[xlsRow, iOutTime].Text = "Out Time";
                    sheet11.Range[xlsRow, iOutTime].ColumnWidth = 14;
                    sheet11.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet11.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iOverStay = xlsCol;
                    sheet11.Range[xlsRow, iOverStay].Text = "Over Stay";
                    sheet11.Range[xlsRow, iOverStay].ColumnWidth = 10;
                    sheet11.Range[xlsRow, iOverStay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet11.Range[xlsRow, iOverStay].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet11.Range[xlsRow, iOverStay].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iFinalOt = xlsCol;
                    sheet11.Range[xlsRow, iFinalOt].Text = "Final OT";
                    sheet11.Range[xlsRow, iFinalOt].ColumnWidth = 10;
                    sheet11.Range[xlsRow, iFinalOt].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet11.Range[xlsRow, iFinalOt].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet11.Range[xlsRow, iFinalOt].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iOTDifference = xlsCol;
                    sheet11.Range[xlsRow, iOTDifference].Text = "OT Difference";
                    sheet11.Range[xlsRow, iOTDifference].ColumnWidth = 15;
                    sheet11.Range[xlsRow, iOTDifference].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet11.Range[xlsRow, iOTDifference].VerticalAlignment = ExcelVAlign.VAlignCenter;

                 
                    sheet11.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    // sheet11.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet11.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet11.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet11.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtOtNotConfirmOverstay.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtOtNotConfirmOverstay.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            try
                            {
                                sheet11.Range[xlsRow, isl].Text = SLNo.ToString();

                                sheet11.Range[xlsRow, iEmployeeCode].Text = dtOtNotConfirmOverstay.Rows[i]["EmployeeCode"].ToString();

                                sheet11.Range[xlsRow, iEmployeeName].Text = dtOtNotConfirmOverstay.Rows[i]["EmployeeName"].ToString();

                                sheet11.Range[xlsRow, iDepartment].Text = dtOtNotConfirmOverstay.Rows[i]["Department"].ToString();

                                sheet11.Range[xlsRow, iEmployeeCurrentStatus].Text = dtOtNotConfirmOverstay.Rows[i]["EmployeeCurrentStatus"].ToString();

                                sheet11.Range[xlsRow, iDesignation].Text = dtOtNotConfirmOverstay.Rows[i]["LegalDesignation"].ToString();

                                sheet11.Range[xlsRow, iSection].Text = dtOtNotConfirmOverstay.Rows[i]["Section"].ToString();

                             
                                sheet11.Range[xlsRow, iSubSection].Text = dtOtNotConfirmOverstay.Rows[i]["SubSection"].ToString();
                                sheet11.Range[xlsRow, iEntity].Text = dtOtNotConfirmOverstay.Rows[i]["EntityName"].ToString();

                                sheet11.Range[xlsRow, iEmployeeCategory].Text = dtOtNotConfirmOverstay.Rows[i]["EmployeeCategory"].ToString();
                                sheet11.Range[xlsRow, iDayStatus].Text = dtOtNotConfirmOverstay.Rows[i]["DayStatus"].ToString();
                                sheet11.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet11.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtOtNotConfirmOverstay.Rows[i]["IsManualDayStatus"].ToString().Trim()))
                                {
                                    sheet11.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }

                                sheet11.Range[xlsRow, iWorkDate].Text = dtOtNotConfirmOverstay.Rows[i]["WorkDate"].ToString();

                                sheet11.Range[xlsRow, iDOJ].Text = dtOtNotConfirmOverstay.Rows[i]["DOJ"].ToString();

                                sheet11.Range[xlsRow, iShiftName].Text = dtOtNotConfirmOverstay.Rows[i]["ShiftName"].ToString();

                                sheet11.Range[xlsRow, iShiftInTime].Text = dtOtNotConfirmOverstay.Rows[i]["ShiftInTime"].ToString();
                                sheet11.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet11.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                sheet11.Range[xlsRow, iShiftOutTime].Text = dtOtNotConfirmOverstay.Rows[i]["ShiftOutTime"].ToString();
                                sheet11.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet11.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (dtOtNotConfirmOverstay.Rows[i]["InTime"].ToString() != "")
                                {
                                    sheet11.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                    sheet11.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtOtNotConfirmOverstay.Rows[i]["InTime"].ToString());
                                    sheet11.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet11.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    if (bplib.clsWebLib.GetBoolData(dtOtNotConfirmOverstay.Rows[i]["IsManualInTime"].ToString().Trim()))
                                    {
                                        sheet11.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                    }
                                }

                                if (dtOtNotConfirmOverstay.Rows[i]["OutTime"].ToString() != "")
                                {
                                    sheet11.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                    sheet11.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(dtOtNotConfirmOverstay.Rows[i]["OutTime"].ToString());
                                    sheet11.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                    sheet11.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                    if (bplib.clsWebLib.GetBoolData(dtOtNotConfirmOverstay.Rows[i]["IsManualOutTime"].ToString().Trim()))
                                    {
                                        sheet11.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                    }
                                }

                                string yot = string.Empty;//OTConsiderOn
                                if (dtOtNotConfirmOverstay.Rows[i]["OverStay"].ToString() != "")
                                {
                                    oru.GetOT(OTConsiderOn, dtOtNotConfirmOverstay.Rows[i]["OverStay"].ToString().Trim(), out yot);
                                }
                                sheet11.Range[xlsRow, iOverStay].Text = yot;
                                sheet11.Range[xlsRow, iOverStay].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet11.Range[xlsRow, iOverStay].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                string xot = string.Empty;
                                if (dtOtNotConfirmOverstay.Rows[i]["TotalOTHr"].ToString() != "")
                                {
                                    oru.GetOT(OTConsiderOn, dtOtNotConfirmOverstay.Rows[i]["TotalOTHr"].ToString().Trim(), out xot);
                                }
                                sheet11.Range[xlsRow, iFinalOt].Text = xot;
                                sheet11.Range[xlsRow, iFinalOt].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet11.Range[xlsRow, iFinalOt].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                string zot = string.Empty;
                                if (dtOtNotConfirmOverstay.Rows[i]["OTDifference"].ToString() != "")
                                {
                                    oru.GetOT(OTConsiderOn, dtOtNotConfirmOverstay.Rows[i]["OTDifference"].ToString().Trim(), out zot);
                                }

                                sheet11.Range[xlsRow, iOTDifference].Text = zot;
                                sheet11.Range[xlsRow, iOTDifference].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet11.Range[xlsRow, iOTDifference].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                xlsRow++;
                                SLNo++;
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }

                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet11.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet11.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet11.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup


                        xlsRow++;
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "OT Not Confirm";
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "OverStay";
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;

                        xlsRow++;
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Text = "Difference Over Stay And Final Ot";
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].Merge();
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].CellStyle.Font.Bold = true;

                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderInside(ExcelLineStyle.Hair);
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].BorderAround(ExcelLineStyle.Hair);
                        sheet11.Range[xlsRow, iEmployeeCode, xlsRow, iWorkDate].WrapText = true;
                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet11.GetColumnWidth(1) + sheet11.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet11.GetRowHeight(1) + sheet11.GetRowHeight(2) + sheet11.GetRowHeight(3) + sheet11.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet11.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet11.Range[xlsRow, 3].Text = CmpName;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet11.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet11.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet11.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet11.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet11.Range[xlsRow, 3].Text = FactoryName;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet11.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet11.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet11.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet11.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet11.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet11.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet11.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet11.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-OT Not Confirm: " + FromDate + " To Date: " + ToDate;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet11.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet11.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet11.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet11.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet11.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet11.IsDisplayZeros = false;
                    sheet11.UsedRange["A7"].FreezePanes();
                    sheet11.FirstVisibleColumn = 1;
                    sheet11.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet11.UsedRange.WrapText = true;
                    sheet11.UsedRange.CellStyle.Font.Size = 8;
                    sheet11.Range["A1"].CellStyle.Font.Size = 14;
                    sheet11.Range["A2"].CellStyle.Font.Size = 10;
                    sheet11.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet11.PageSetup.TopMargin = 0.5;
                    sheet11.PageSetup.BottomMargin = 0.7;
                    sheet11.PageSetup.PrintTitleRows = "$1:$5";
                    sheet11.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet11.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet11.PageSetup.LeftMargin = 0.5;
                    sheet11.PageSetup.RightMargin = 0.2;
                    sheet11.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet11.PageSetup.FitToPagesTall = 0;
                    sheet11.PageSetup.FitToPagesWide = 1;
                    sheet11.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet11.IsDisplayZeros = false;

                    if (dtOtNotConfirmOverstay.Rows.Count > 0)
                    {
                        sheet11.Name = (SheetIndex + 1) + "_OT_Not_Confirm";
                        sheet11.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet11.Name = (SheetIndex + 1) + "_OT_Not_Confirm";

                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {

                }
                #endregion OT NOT CONFIRM OVERSTAY

                #region Long Absentisom 12

                try
                {
                    IWorksheet sheet12 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet12 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet12.Range[5, igoto].Text = "Goto Index";
                    sheet12.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromLongAbsentisom = sheet12.HyperLinks.Add(sheet12.Range[5, igoto]);
                    linkgofromLongAbsentisom.Type = ExcelHyperLinkType.Workbook;
                    linkgofromLongAbsentisom.TextToDisplay = sheet12.Range[5, igoto].Text;
                    linkgofromLongAbsentisom.ScreenTip = "Go To " + sheet12.Range[5, igoto].Text;
                    linkgofromLongAbsentisom.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet12.Range[xlsRow, isl].Text = "SL";
                    sheet12.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet12.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet12.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet12.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet12.Range[xlsRow, iWorkDate].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet12.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet12.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet12.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet12.Range[xlsRow, iDesignation].ColumnWidth = 25;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet12.Range[xlsRow, iDepartment].Text = "Department";
                    sheet12.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet12.Range[xlsRow, iSection].Text = "Section";
                    sheet12.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet12.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet12.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet12.Range[xlsRow, iEntity].Text = "Entity";
                    sheet12.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iLine = xlsCol;
                    sheet12.Range[xlsRow, iLine].Text = "Line";
                    sheet12.Range[xlsRow, iLine].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet12.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet12.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet12.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet12.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 18;
                    sheet12.Range[xlsRow, iEmployeeCurrentStatus].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet12.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet12.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCurrentStatusEffectiveDate = xlsCol;
                    sheet12.Range[xlsRow, iEmployeeCurrentStatusEffectiveDate].Text = "Employee Current Status Effective Date";
                    sheet12.Range[xlsRow, iEmployeeCurrentStatusEffectiveDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iNumberOfAbsentDays = xlsCol;
                    sheet12.Range[xlsRow, iNumberOfAbsentDays].Text = "Number Of Absent Days";
                    sheet12.Range[xlsRow, iNumberOfAbsentDays].ColumnWidth = 18;
                    sheet12.Range[xlsRow, iNumberOfAbsentDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet12.Range[xlsRow, iNumberOfAbsentDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet12.Range[xlsRow, iNumberOfAbsentDays].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet12.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    // sheet12.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet12.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet12.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet12.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtLongAbsentisom.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtLongAbsentisom.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet12.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet12.Range[xlsRow, iEmployeeCode].Text = dtLongAbsentisom.Rows[i]["EmployeeCode"].ToString();

                            sheet12.Range[xlsRow, iEmployeeName].Text = dtLongAbsentisom.Rows[i]["EmployeeName"].ToString();

                            sheet12.Range[xlsRow, iWorkDate].Text = dtLongAbsentisom.Rows[i]["WorkDate"].ToString();

                            sheet12.Range[xlsRow, iDepartment].Text = dtLongAbsentisom.Rows[i]["Department"].ToString();

                            sheet12.Range[xlsRow, iDesignation].Text = dtLongAbsentisom.Rows[i]["LegalDesignation"].ToString();

                            sheet12.Range[xlsRow, iSection].Text = dtLongAbsentisom.Rows[i]["Section"].ToString();

                            sheet12.Range[xlsRow, iSubSection].Text = dtLongAbsentisom.Rows[i]["SubSection"].ToString();
                            sheet12.Range[xlsRow, iEntity].Text = dtLongAbsentisom.Rows[i]["EntityName"].ToString();

                            sheet12.Range[xlsRow, iLine].Text = dtLongAbsentisom.Rows[i]["Line"].ToString();

                            sheet12.Range[xlsRow, iEmployeeCurrentStatus].Text = dtLongAbsentisom.Rows[i]["EmployeeCurrentStatus"].ToString();

                            sheet12.Range[xlsRow, iEmployeeCurrentStatusEffectiveDate].Text = dtLongAbsentisom.Rows[i]["EmployeeCurrentStatusEffectiveDate"].ToString();

                            sheet12.Range[xlsRow, iDOJ].Text = dtLongAbsentisom.Rows[i]["DOJ"].ToString();
                            sheet12.Range[xlsRow, iEmployeeCategory].Text = dtLongAbsentisom.Rows[i]["EmployeeCategory"].ToString();
                            sheet12.Range[xlsRow, iNumberOfAbsentDays].Text = dtLongAbsentisom.Rows[i]["NumberOfAbsentDays"].ToString();
                            sheet12.Range[xlsRow, iNumberOfAbsentDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet12.Range[xlsRow, iNumberOfAbsentDays].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet12.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet12.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet12.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup
                    }


                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet12.GetColumnWidth(1) + sheet12.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet12.GetRowHeight(1) + sheet12.GetRowHeight(2) + sheet12.GetRowHeight(3) + sheet12.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet12.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet12.Range[xlsRow, 3].Text = CmpName;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet12.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet12.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet12.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet12.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet12.Range[xlsRow, 3].Text = FactoryName;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet12.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet12.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet12.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet12.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet12.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet12.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet12.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet12.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Long Absentisom: " + FromDate + " To Date: " + ToDate;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet12.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet12.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet12.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet12.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet12.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet12.IsDisplayZeros = false;
                    sheet12.UsedRange["A7"].FreezePanes();
                    sheet12.FirstVisibleColumn = 1;
                    sheet12.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet12.UsedRange.WrapText = true;
                    sheet12.UsedRange.CellStyle.Font.Size = 8;
                    sheet12.Range["A1"].CellStyle.Font.Size = 14;
                    sheet12.Range["A2"].CellStyle.Font.Size = 10;
                    sheet12.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet12.PageSetup.TopMargin = 0.5;
                    sheet12.PageSetup.BottomMargin = 0.7;
                    sheet12.PageSetup.PrintTitleRows = "$1:$5";
                    sheet12.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet12.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet12.PageSetup.LeftMargin = 0.5;
                    sheet12.PageSetup.RightMargin = 0.2;
                    sheet12.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet12.PageSetup.FitToPagesTall = 0;
                    sheet12.PageSetup.FitToPagesWide = 1;
                    sheet12.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet12.IsDisplayZeros = false;

                    if (dtLongAbsentisom.Rows.Count > 0)
                    {
                        sheet12.Name = (SheetIndex + 1) + "_Long_Absenteeism";
                        sheet12.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet12.Name = (SheetIndex + 1) + "_Long_Absenteeism";

                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {

                }
                #endregion Long Absentisom

                #region TBS 13
                try
                {
                    IWorksheet sheet13 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet13 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet13.Range[5, igoto].Text = "Goto Index";
                    sheet13.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromTBS = sheet13.HyperLinks.Add(sheet13.Range[5, igoto]);
                    linkgofromTBS.Type = ExcelHyperLinkType.Workbook;
                    linkgofromTBS.TextToDisplay = sheet13.Range[5, igoto].Text;
                    linkgofromTBS.ScreenTip = "Go To " + sheet13.Range[5, igoto].Text;
                    linkgofromTBS.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet13.Range[xlsRow, isl].Text = "SL";
                    sheet13.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet13.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet13.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet13.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet13.Range[xlsRow, iWorkDate].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet13.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet13.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet13.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet13.Range[xlsRow, iDesignation].ColumnWidth = 25;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet13.Range[xlsRow, iDepartment].Text = "Department";
                    sheet13.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet13.Range[xlsRow, iSection].Text = "Section";
                    sheet13.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet13.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet13.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet13.Range[xlsRow, iEntity].Text = "Entity";
                    sheet13.Range[xlsRow, iEntity].ColumnWidth = 18;



                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet13.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet13.Range[xlsRow, iDOJ].ColumnWidth = 16;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet13.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet13.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 18;
                    sheet13.Range[xlsRow, iEmployeeCurrentStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet13.Range[xlsRow, iEmployeeCurrentStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet13.Range[xlsRow, iEmployeeCurrentStatus].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet13.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet13.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCurrentStatusEffectiveDate = xlsCol;
                    sheet13.Range[xlsRow, iEmployeeCurrentStatusEffectiveDate].Text = "Employee Current Status Effective Date";
                    sheet13.Range[xlsRow, iEmployeeCurrentStatusEffectiveDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iDayStatus = xlsCol;
                    sheet13.Range[xlsRow, iDayStatus].Text = "Day Status";
                    sheet13.Range[xlsRow, iDayStatus].ColumnWidth = 18;
                    sheet13.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet13.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet13.Range[xlsRow, iDayStatus].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iPresentFromEffectiveDate = xlsCol;
                    sheet13.Range[xlsRow, iPresentFromEffectiveDate].Text = "Number Of Present Days From EffectiveDate";
                    sheet13.Range[xlsRow, iPresentFromEffectiveDate].ColumnWidth = 25;
                    sheet13.Range[xlsRow, iPresentFromEffectiveDate].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet13.Range[xlsRow, iPresentFromEffectiveDate].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet13.Range[xlsRow, iPresentFromEffectiveDate].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iNumberOfAbsentDays = xlsCol;
                    sheet13.Range[xlsRow, iNumberOfAbsentDays].Text = "Number Of Absent Days";
                    sheet13.Range[xlsRow, iNumberOfAbsentDays].ColumnWidth = 18;
                    sheet13.Range[xlsRow, iNumberOfAbsentDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet13.Range[xlsRow, iNumberOfAbsentDays].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet13.Range[xlsRow, iNumberOfAbsentDays].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet13.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //sheet13.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet13.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet13.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet13.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    if (dtTBS.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtTBS.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet13.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet13.Range[xlsRow, iEmployeeCode].Text = dtTBS.Rows[i]["EmployeeCode"].ToString();

                            sheet13.Range[xlsRow, iEmployeeName].Text = dtTBS.Rows[i]["EmployeeName"].ToString();

                            sheet13.Range[xlsRow, iWorkDate].Text = dtTBS.Rows[i]["WorkDate"].ToString();

                            sheet13.Range[xlsRow, iDepartment].Text = dtTBS.Rows[i]["Department"].ToString();

                            sheet13.Range[xlsRow, iDesignation].Text = dtTBS.Rows[i]["LegalDesignation"].ToString();

                            sheet13.Range[xlsRow, iSection].Text = dtTBS.Rows[i]["Section"].ToString();

                            sheet13.Range[xlsRow, iSubSection].Text = dtTBS.Rows[i]["SubSection"].ToString();
                            sheet13.Range[xlsRow, iEntity].Text = dtTBS.Rows[i]["EntityName"].ToString();

                            sheet13.Range[xlsRow, iEmployeeCategory].Text = dtTBS.Rows[i]["EmployeeCategory"].ToString();
                            sheet13.Range[xlsRow, iDOJ].Text = dtTBS.Rows[i]["DOJ"].ToString();

                            sheet13.Range[xlsRow, iDayStatus].Text = dtTBS.Rows[i]["DayStatus"].ToString();

                            sheet13.Range[xlsRow, iPresentFromEffectiveDate].Number = clsStaticInfo.dbl(dtTBS.Rows[i]["PresentFromEffectiveDate"].ToString());

                            sheet13.Range[xlsRow, iEmployeeCurrentStatus].Text = dtTBS.Rows[i]["EmployeeCurrentStatus"].ToString();
                            sheet13.Range[xlsRow, iEmployeeCurrentStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet13.Range[xlsRow, iEmployeeCurrentStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet13.Range[xlsRow, iEmployeeCurrentStatusEffectiveDate].Text = dtTBS.Rows[i]["EmployeeCurrentStatusEffectiveDate"].ToString();

                            sheet13.Range[xlsRow, iNumberOfAbsentDays].Text = dtTBS.Rows[i]["NumberOfAbsentDays"].ToString();
                            sheet13.Range[xlsRow, iNumberOfAbsentDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet13.Range[xlsRow, iNumberOfAbsentDays].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet13.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet13.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet13.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup
                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet13.GetColumnWidth(1) + sheet13.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet13.GetRowHeight(1) + sheet13.GetRowHeight(2) + sheet13.GetRowHeight(3) + sheet13.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet13.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet13.Range[xlsRow, 3].Text = CmpName;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet13.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet13.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet13.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet13.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet13.Range[xlsRow, 3].Text = FactoryName;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet13.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet13.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet13.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet13.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet13.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet13.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet13.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet13.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-TBS: " + FromDate + " To Date: " + ToDate;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet13.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet13.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet13.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet13.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet13.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet13.IsDisplayZeros = false;
                    sheet13.UsedRange["A7"].FreezePanes();
                    sheet13.FirstVisibleColumn = 1;
                    sheet13.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet13.UsedRange.WrapText = true;
                    sheet13.UsedRange.CellStyle.Font.Size = 8;
                    sheet13.Range["A1"].CellStyle.Font.Size = 14;
                    sheet13.Range["A2"].CellStyle.Font.Size = 10;
                    sheet13.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet13.PageSetup.TopMargin = 0.5;
                    sheet13.PageSetup.BottomMargin = 0.7;
                    sheet13.PageSetup.PrintTitleRows = "$1:$5";
                    sheet13.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet13.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet13.PageSetup.LeftMargin = 0.5;
                    sheet13.PageSetup.RightMargin = 0.2;
                    sheet13.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet13.PageSetup.FitToPagesTall = 0;
                    sheet13.PageSetup.FitToPagesWide = 1;
                    sheet13.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet13.IsDisplayZeros = false;

                    if (dtTBS.Rows.Count > 0)
                    {
                        sheet13.Name = (SheetIndex + 1) + "_TBS";
                        sheet13.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet13.Name = (SheetIndex + 1) + "_TBS";
                    }

                    #endregion Page Setup
                }
                catch (Exception ex)
                {

                }

                #endregion TBS

                #region Maternity Leave 14

                try
                {
                    IWorksheet sheet14 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet14 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet14.Range[5, igoto].Text = "Goto Index";
                    sheet14.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromMaternityLeave = sheet14.HyperLinks.Add(sheet14.Range[5, igoto]);
                    linkgofromMaternityLeave.Type = ExcelHyperLinkType.Workbook;
                    linkgofromMaternityLeave.TextToDisplay = sheet14.Range[5, igoto].Text;
                    linkgofromMaternityLeave.ScreenTip = "Go To " + sheet14.Range[5, igoto].Text;
                    linkgofromMaternityLeave.Address = "1_Index!A1";


                    isl = xlsCol;
                    sheet14.Range[xlsRow, isl].Text = "SL";
                    sheet14.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet14.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet14.Range[xlsRow, iEmployeeCode].ColumnWidth = 9;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet14.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet14.Range[xlsRow, iWorkDate].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet14.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet14.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet14.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet14.Range[xlsRow, iDesignation].ColumnWidth = 25;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet14.Range[xlsRow, iDepartment].Text = "Department";
                    sheet14.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet14.Range[xlsRow, iSection].Text = "Section";
                    sheet14.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet14.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet14.Range[xlsRow, iSubSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet14.Range[xlsRow, iEntity].Text = "Entity";
                    sheet14.Range[xlsRow, iEntity].ColumnWidth = 18;


                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet14.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet14.Range[xlsRow, iDOJ].ColumnWidth = 12;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet14.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet14.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iBabyNo = xlsCol;
                    sheet14.Range[xlsRow, iBabyNo].Text = "Baby No";
                    sheet14.Range[xlsRow, iBabyNo].ColumnWidth = 8;

                    xlsCol += 1;
                    iFollowUpStartDate = xlsCol;
                    sheet14.Range[xlsRow, iFollowUpStartDate].Text = "Follow Up Start Day";
                    sheet14.Range[xlsRow, iFollowUpStartDate].ColumnWidth = 15;

                    xlsCol += 1;
                    iFollowUpEndDate = xlsCol;
                    sheet14.Range[xlsRow, iFollowUpEndDate].Text = "Follow Up End Day";
                    sheet14.Range[xlsRow, iFollowUpEndDate].ColumnWidth = 15;

                    xlsCol += 1;
                    iMaternityLeaveStartDate = xlsCol;
                    sheet14.Range[xlsRow, iMaternityLeaveStartDate].Text = "Leave Start Day";
                    sheet14.Range[xlsRow, iMaternityLeaveStartDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iMaternityLeaveEndDate = xlsCol;
                    sheet14.Range[xlsRow, iMaternityLeaveEndDate].Text = "Leave End Day";
                    sheet14.Range[xlsRow, iMaternityLeaveEndDate].ColumnWidth = 16;

                    xlsCol += 1;
                    iGapeBetweenConsecutiveIssue = xlsCol;
                    sheet14.Range[xlsRow, iGapeBetweenConsecutiveIssue].Text = "Maternity Status";
                    sheet14.Range[xlsRow, iGapeBetweenConsecutiveIssue].ColumnWidth = 16;
                    sheet14.Range[xlsRow, iGapeBetweenConsecutiveIssue].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet14.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    // sheet14.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet14.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet14.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet14.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtMaternityLeave.Rows.Count > 0)
                    {

                        SLNo = 1;
                        for (int i = 0; i < dtMaternityLeave.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet14.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet14.Range[xlsRow, iEmployeeCode].Text = dtMaternityLeave.Rows[i]["EmployeeCode"].ToString();

                            sheet14.Range[xlsRow, iWorkDate].Text = dtMaternityLeave.Rows[i]["WorkDate"].ToString();

                            sheet14.Range[xlsRow, iEmployeeName].Text = dtMaternityLeave.Rows[i]["EmployeeName"].ToString();

                            sheet14.Range[xlsRow, iDepartment].Text = dtMaternityLeave.Rows[i]["Department"].ToString();

                            sheet14.Range[xlsRow, iDesignation].Text = dtMaternityLeave.Rows[i]["LegalDesignation"].ToString();

                            sheet14.Range[xlsRow, iSection].Text = dtMaternityLeave.Rows[i]["Section"].ToString();

                            sheet14.Range[xlsRow, iSubSection].Text = dtMaternityLeave.Rows[i]["SubSection"].ToString();

                            sheet14.Range[xlsRow, iEntity].Text = dtMaternityLeave.Rows[i]["EntityName"].ToString();

                            sheet14.Range[xlsRow, iDOJ].Text = dtMaternityLeave.Rows[i]["DOJ"].ToString();

                            sheet14.Range[xlsRow, iBabyNo].Text = dtMaternityLeave.Rows[i]["ChildNo"].ToString();

                            sheet14.Range[xlsRow, iFollowUpStartDate].Text = dtMaternityLeave.Rows[i]["FollowUpStartDay"].ToString();

                            sheet14.Range[xlsRow, iFollowUpEndDate].Text = dtMaternityLeave.Rows[i]["FollowUpEndDay"].ToString();

                            sheet14.Range[xlsRow, iMaternityLeaveStartDate].Text = dtMaternityLeave.Rows[i]["MaternityLeaveStartDay"].ToString();

                            sheet14.Range[xlsRow, iMaternityLeaveEndDate].Text = dtMaternityLeave.Rows[i]["MaternityLeaveEndDay"].ToString();

                            sheet14.Range[xlsRow, iGapeBetweenConsecutiveIssue].Text = dtMaternityLeave.Rows[i]["MaternityStatus"].ToString();

                            sheet14.Range[xlsRow, iEmployeeCategory].Text = dtMaternityLeave.Rows[i]["EmployeeCategory"].ToString();

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet14.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet14.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet14.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup
                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet14.GetColumnWidth(1) + sheet14.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet14.GetRowHeight(1) + sheet14.GetRowHeight(2) + sheet14.GetRowHeight(3) + sheet14.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet14.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet14.Range[xlsRow, 3].Text = CmpName;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet14.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet14.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet14.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet14.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet14.Range[xlsRow, 3].Text = FactoryName;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet14.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet14.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet14.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet14.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet14.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet14.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet14.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet14.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Maternity Leave: " + FromDate + " To Date: " + ToDate;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet14.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet14.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet14.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet14.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet14.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet14.IsDisplayZeros = false;
                    sheet14.UsedRange["A7"].FreezePanes();
                    sheet14.FirstVisibleColumn = 1;
                    sheet14.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet14.UsedRange.WrapText = true;
                    sheet14.UsedRange.CellStyle.Font.Size = 8;
                    sheet14.Range["A1"].CellStyle.Font.Size = 14;
                    sheet14.Range["A2"].CellStyle.Font.Size = 10;
                    sheet14.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet14.PageSetup.TopMargin = 0.5;
                    sheet14.PageSetup.BottomMargin = 0.7;
                    sheet14.PageSetup.PrintTitleRows = "$1:$5";
                    sheet14.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet14.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet14.PageSetup.LeftMargin = 0.5;
                    sheet14.PageSetup.RightMargin = 0.2;
                    sheet14.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet14.PageSetup.FitToPagesTall = 0;
                    sheet14.PageSetup.FitToPagesWide = 1;
                    sheet14.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet14.IsDisplayZeros = false;

                    if (dtMaternityLeave.Rows.Count > 0)
                    {
                        sheet14.Name = (SheetIndex + 1) + "_Maternity_Leave";
                        sheet14.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet14.Name = (SheetIndex + 1) + "_Maternity_Leave";
                    }

                    #endregion Page Setup
                }
                catch (Exception ex)
                {

                }

                #endregion Maternity Leave

                #region Bank Remarks 15

                try
                {
                    IWorksheet sheet17 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet17 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;
                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet17.Range[5, igoto].Text = "Goto Index";
                    sheet17.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromBankRemarks = sheet17.HyperLinks.Add(sheet17.Range[5, igoto]);
                    linkgofromBankRemarks.Type = ExcelHyperLinkType.Workbook;
                    linkgofromBankRemarks.TextToDisplay = sheet17.Range[5, igoto].Text;
                    linkgofromBankRemarks.ScreenTip = "Go To " + sheet17.Range[5, igoto].Text;
                    linkgofromBankRemarks.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet17.Range[xlsRow, isl].Text = "SL";
                    sheet17.Range[xlsRow, isl].ColumnWidth = 8;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet17.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet17.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet17.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet17.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet17.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet17.Range[xlsRow, iDesignation].ColumnWidth = 25;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet17.Range[xlsRow, iDepartment].Text = "Department";
                    sheet17.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet17.Range[xlsRow, iSection].Text = "Section";
                    sheet17.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet17.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet17.Range[xlsRow, iSubSection].ColumnWidth = 14;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet17.Range[xlsRow, iEntity].Text = "Entity";
                    sheet17.Range[xlsRow, iEntity].ColumnWidth = 14;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet17.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet17.Range[xlsRow, iDOJ].ColumnWidth = 12;

                    xlsCol += 1;
                    iEmployeeBankStatus = xlsCol;
                    sheet17.Range[xlsRow, iEmployeeBankStatus].Text = "Employee Status";
                    sheet17.Range[xlsRow, iEmployeeBankStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet17.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet17.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet17.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet17.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iPaymentMode = xlsCol;
                    sheet17.Range[xlsRow, iPaymentMode].Text = "Payment Mode";
                    sheet17.Range[xlsRow, iPaymentMode].ColumnWidth = 14;

                    xlsCol += 1;
                    iBankAccountNo = xlsCol;
                    sheet17.Range[xlsRow, iBankAccountNo].Text = "Bank Account No";
                    sheet17.Range[xlsRow, iBankAccountNo].ColumnWidth = 18;

                    xlsCol += 1;
                    iRemark = xlsCol;
                    sheet17.Range[xlsRow, iRemark].Text = "Remark";
                    sheet17.Range[xlsRow, iRemark].ColumnWidth = 24;
                    sheet17.Range[xlsRow, iRemark].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet17.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    // sheet17.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet17.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet17.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet17.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    if (dtBankRemarks.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtBankRemarks.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet17.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet17.Range[xlsRow, iEmployeeCode].Text = dtBankRemarks.Rows[i]["EmployeeCode"].ToString();

                            sheet17.Range[xlsRow, iEmployeeName].Text = dtBankRemarks.Rows[i]["EmployeeName"].ToString();

                            sheet17.Range[xlsRow, iDepartment].Text = dtBankRemarks.Rows[i]["Department"].ToString();

                            sheet17.Range[xlsRow, iDesignation].Text = dtBankRemarks.Rows[i]["LegalDesignation"].ToString();

                            sheet17.Range[xlsRow, iSection].Text = dtBankRemarks.Rows[i]["Section"].ToString();

                            sheet17.Range[xlsRow, iSubSection].Text = dtBankRemarks.Rows[i]["SubSection"].ToString();

                            sheet17.Range[xlsRow, iEntity].Text = dtBankRemarks.Rows[i]["EntityName"].ToString();


                            sheet17.Range[xlsRow, iDOJ].Text = dtBankRemarks.Rows[i]["DOJ"].ToString();

                            sheet17.Range[xlsRow, iPaymentMode].Text = dtBankRemarks.Rows[i]["PaymentMode"].ToString();

                            sheet17.Range[xlsRow, iBankAccountNo].Text = dtBankRemarks.Rows[i]["BankAccNo"].ToString();

                            sheet17.Range[xlsRow, iRemark].Text = dtBankRemarks.Rows[i]["Remark"].ToString();

                            sheet17.Range[xlsRow, iEmployeeBankStatus].Text = dtBankRemarks.Rows[i]["EmployeeStatus"].ToString();
                            sheet17.Range[xlsRow, iEmployeeCurrentStatus].Text = dtBankRemarks.Rows[i]["EmployeeCurrentStatus"].ToString();

                            sheet17.Range[xlsRow, iEmployeeCategory].Text = dtBankRemarks.Rows[i]["EmployeeCategory"].ToString();

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet17.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet17.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet17.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                    }
                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet17.GetColumnWidth(1) + sheet17.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet17.GetRowHeight(1) + sheet17.GetRowHeight(2) + sheet17.GetRowHeight(3) + sheet17.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet17.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet17.Range[xlsRow, 3].Text = CmpName;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet17.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet17.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet17.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet17.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet17.Range[xlsRow, 3].Text = FactoryName;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet17.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet17.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet17.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet17.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet17.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet17.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet17.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet17.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Bank Remarks: " + FromDate + " To Date: " + ToDate;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet17.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet17.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet17.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet17.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet17.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet17.IsDisplayZeros = false;
                    sheet17.UsedRange["A7"].FreezePanes();
                    sheet17.FirstVisibleColumn = 1;
                    sheet17.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet17.UsedRange.WrapText = true;
                    sheet17.UsedRange.CellStyle.Font.Size = 8;
                    sheet17.Range["A1"].CellStyle.Font.Size = 14;
                    sheet17.Range["A2"].CellStyle.Font.Size = 10;
                    sheet17.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet17.PageSetup.TopMargin = 0.5;
                    sheet17.PageSetup.BottomMargin = 0.7;
                    sheet17.PageSetup.PrintTitleRows = "$1:$5";
                    sheet17.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet17.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet17.PageSetup.LeftMargin = 0.5;
                    sheet17.PageSetup.RightMargin = 0.2;
                    sheet17.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet17.PageSetup.FitToPagesTall = 0;
                    sheet17.PageSetup.FitToPagesWide = 1;
                    sheet17.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet17.IsDisplayZeros = false;

                    if (dtBankRemarks.Rows.Count > 0)
                    {
                        sheet17.Name = (SheetIndex + 1) + "_Bank_Remark";
                        sheet17.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet17.Name = (SheetIndex + 1) + "_Bank_Remark";
                    }

                    #endregion Page Setup
                }
                catch (Exception ex)
                {

                }

                #endregion Bank Remarks

                #region Separated Absent 16

                try
                {
                    IWorksheet sheet18 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet18 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet18.Range[5, igoto].Text = "Goto Index";
                    sheet18.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromSeparatedAbsent = sheet18.HyperLinks.Add(sheet18.Range[5, igoto]);
                    linkgofromSeparatedAbsent.Type = ExcelHyperLinkType.Workbook;
                    linkgofromSeparatedAbsent.TextToDisplay = sheet18.Range[5, igoto].Text;
                    linkgofromSeparatedAbsent.ScreenTip = "Go To " + sheet18.Range[5, igoto].Text;
                    linkgofromSeparatedAbsent.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet18.Range[xlsRow, isl].Text = "SL";
                    sheet18.Range[xlsRow, isl].ColumnWidth = 6;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet18.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet18.Range[xlsRow, iEmployeeCode].ColumnWidth = 8;

                    xlsCol += 1;
                    iDayStatus = xlsCol;
                    sheet18.Range[xlsRow, iDayStatus].Text = "Day Status";
                    sheet18.Range[xlsRow, iDayStatus].ColumnWidth = 12;
                    sheet18.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet18.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet18.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet18.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet18.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet18.Range[xlsRow, iDesignation].ColumnWidth = 25;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet18.Range[xlsRow, iDepartment].Text = "Department";
                    sheet18.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet18.Range[xlsRow, iSection].Text = "Section";
                    sheet18.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet18.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet18.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet18.Range[xlsRow, iEntity].Text = "Entity";
                    sheet18.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iFirstAbsentDate = xlsCol;
                    sheet18.Range[xlsRow, iFirstAbsentDate].Text = "First Absent Date";
                    sheet18.Range[xlsRow, iFirstAbsentDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iDateOfSeperation = xlsCol;
                    sheet18.Range[xlsRow, iDateOfSeperation].Text = "Date Of Seperation";
                    sheet18.Range[xlsRow, iDateOfSeperation].ColumnWidth = 18;

                    xlsCol += 1;
                    iAbsentCount = xlsCol;
                    sheet18.Range[xlsRow, iAbsentCount].Text = "Absent Count";
                    sheet18.Range[xlsRow, iAbsentCount].ColumnWidth = 18;
                    sheet18.Range[xlsRow, iAbsentCount].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iAbsentDays = xlsCol;
                    sheet18.Range[xlsRow, iAbsentDays].Text = "Absent Days";
                    sheet18.Range[xlsRow, iAbsentDays].ColumnWidth = 18;
                    sheet18.Range[xlsRow, iAbsentDays].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet18.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //sheet18.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet18.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet18.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet18.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------

                    if (dtSeparatedAbsent.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtSeparatedAbsent.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet18.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet18.Range[xlsRow, iEmployeeCode].Text = dtSeparatedAbsent.Rows[i]["EmployeeCode"].ToString();

                            sheet18.Range[xlsRow, iEmployeeName].Text = dtSeparatedAbsent.Rows[i]["EmployeeName"].ToString();

                            sheet18.Range[xlsRow, iDepartment].Text = dtSeparatedAbsent.Rows[i]["Department"].ToString();

                            sheet18.Range[xlsRow, iDateOfSeperation].Text = dtSeparatedAbsent.Rows[i]["DOS"].ToString();

                            sheet18.Range[xlsRow, iFirstAbsentDate].Text = dtSeparatedAbsent.Rows[i]["FirstAbsentDate"].ToString();

                            sheet18.Range[xlsRow, iAbsentCount].Text = dtSeparatedAbsent.Rows[i]["AbsentCount"].ToString();
                            sheet18.Range[xlsRow, iAbsentCount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet18.Range[xlsRow, iAbsentCount].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet18.Range[xlsRow, iDayStatus].Text = dtSeparatedAbsent.Rows[i]["DayStatus"].ToString();
                            sheet18.Range[xlsRow, iDayStatus].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet18.Range[xlsRow, iDayStatus].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet18.Range[xlsRow, iDesignation].Text = dtSeparatedAbsent.Rows[i]["LegalDesignation"].ToString();

                            sheet18.Range[xlsRow, iSection].Text = dtSeparatedAbsent.Rows[i]["Section"].ToString();

                            sheet18.Range[xlsRow, iSubSection].Text = dtSeparatedAbsent.Rows[i]["SubSection"].ToString();
                            sheet18.Range[xlsRow, iEntity].Text = dtSeparatedAbsent.Rows[i]["EntityName"].ToString();

                            sheet18.Range[xlsRow, iAbsentDays].Text = dtSeparatedAbsent.Rows[i]["AbsentDays"].ToString();
                            sheet18.Range[xlsRow, iAbsentDays].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet18.Range[xlsRow, iAbsentDays].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet18.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet18.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet18.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet18.GetColumnWidth(1) + sheet18.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet18.GetRowHeight(1) + sheet18.GetRowHeight(2) + sheet18.GetRowHeight(3) + sheet18.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet18.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet18.Range[xlsRow, 3].Text = CmpName;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet18.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet18.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet18.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet18.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet18.Range[xlsRow, 3].Text = FactoryName;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet18.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet18.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet18.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet18.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet18.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet18.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet18.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet18.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Separation With Absent: " + FromDate + " To Date: " + ToDate;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet18.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet18.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet18.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet18.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet18.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet18.IsDisplayZeros = false;
                    sheet18.UsedRange["A7"].FreezePanes();
                    sheet18.FirstVisibleColumn = 1;
                    sheet18.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet18.UsedRange.WrapText = true;
                    sheet18.UsedRange.CellStyle.Font.Size = 8;
                    sheet18.Range["A1"].CellStyle.Font.Size = 14;
                    sheet18.Range["A2"].CellStyle.Font.Size = 10;
                    sheet18.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet18.PageSetup.TopMargin = 0.5;
                    sheet18.PageSetup.BottomMargin = 0.7;
                    sheet18.PageSetup.PrintTitleRows = "$1:$5";
                    sheet18.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet18.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet18.PageSetup.LeftMargin = 0.5;
                    sheet18.PageSetup.RightMargin = 0.2;
                    sheet18.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet18.PageSetup.FitToPagesTall = 0;
                    sheet18.PageSetup.FitToPagesWide = 1;
                    sheet18.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet18.IsDisplayZeros = false;

                    if (dtSeparatedAbsent.Rows.Count > 0)
                    {
                        sheet18.Name = (SheetIndex + 1) + "_Separation_With_Absent";
                        sheet18.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet18.Name = (SheetIndex + 1) + "_Separation_With_Absent";
                    }

                    #endregion Page Setup
                }
                catch (Exception ex)
                {

                }

                #endregion Separated Absent

                #region Attendance Not Lock 17
                try
                {
                    IWorksheet sheet19 = null;
                    xlsRow = 1;
                    xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet19 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region plantwisedatelock

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet19.Range[5, igoto].Text = "Goto Index";
                    sheet19.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromunlocksheet = sheet19.HyperLinks.Add(sheet19.Range[5, igoto]);
                    linkgofromunlocksheet.Type = ExcelHyperLinkType.Workbook;
                    linkgofromunlocksheet.TextToDisplay = sheet19.Range[5, igoto].Text;
                    linkgofromunlocksheet.ScreenTip = "Go To " + sheet19.Range[5, igoto].Text;
                    linkgofromunlocksheet.Address = "1_Index!A1";

                    xlsRow++;
                    isl = xlsCol;
                    sheet19.Range[xlsRow, isl].Text = "SL";
                    sheet19.Range[xlsRow, isl].ColumnWidth = 15;

                    xlsCol++;
                    LockDatePlant = xlsCol;
                    sheet19.Range[xlsRow, LockDatePlant].Text = "Un-Lock Date";
                    sheet19.Range[xlsRow, LockDatePlant].ColumnWidth = 25;

                    sheet19.Range[xlsRow - 1, 1].Text = "Plant Wise Day Lock";
                    sheet19.Range[xlsRow - 1, 1, xlsRow - 1, LockDatePlant].Merge();

                    sheet19.Range[xlsRow - 1, 1, xlsRow, LockDatePlant].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    //sheet19.Range[xlsRow - 1, 1, xlsRow, LockDatePlant].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet19.Range[xlsRow - 1, 1, xlsRow, LockDatePlant].BorderAround(ExcelLineStyle.Hair);
                    sheet19.Range[xlsRow - 1, 1, xlsRow, LockDatePlant].BorderInside(ExcelLineStyle.Hair);
                    sheet19.Range[xlsRow - 1, 1, xlsRow, LockDatePlant].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------


                    if (AttendanceNotLockPlant.Length > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < AttendanceNotLockPlant.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(AttendanceNotLockPlant[i]))
                            {
                                sheet19.Range[xlsRow, isl].Text = SLNo.ToString();
                                sheet19.Range[xlsRow, LockDatePlant].Text = AttendanceNotLockPlant[i].ToString();
                                xlsRow++;
                                SLNo++;
                            }
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet19.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet19.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet19.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                    }
                    xlsRow++;
                    
                    
                    #region ******************Report Header******************

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet19.Range[xlsRow, xlsCol].Text = CmpName;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet19.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet19.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet19.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet19.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet19.Range[xlsRow, xlsCol].Text = FactoryName;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet19.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet19.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet19.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet19.Range[xlsRow, xlsCol].Text = FactoryAddress;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet19.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 22;
                    sheet19.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet19.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet19.Range[xlsRow, xlsCol].Text = (SheetIndex + 1) + "-Attendance Not Lock: " + FromDate + " To Date: " + ToDate;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet19.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet19.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                    sheet19.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet19.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet19.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet19.IsDisplayZeros = false;
                    sheet19.UsedRange["A7"].FreezePanes();
                    sheet19.FirstVisibleColumn = 1;
                    sheet19.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet19.UsedRange.WrapText = true;
                    sheet19.UsedRange.CellStyle.Font.Size = 8;
                    sheet19.Range["A1"].CellStyle.Font.Size = 14;
                    sheet19.Range["A2"].CellStyle.Font.Size = 10;
                    sheet19.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet19.PageSetup.TopMargin = 0.5;
                    sheet19.PageSetup.BottomMargin = 0.7;
                    sheet19.PageSetup.PrintTitleRows = "$1:$5";
                    sheet19.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet19.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet19.PageSetup.LeftMargin = 0.5;
                    sheet19.PageSetup.RightMargin = 0.2;
                    sheet19.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet19.PageSetup.FitToPagesTall = 0;
                    sheet19.PageSetup.FitToPagesWide = 1;
                    sheet19.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet19.IsDisplayZeros = false;

                    if (dtAttendanceNotLockPlant.Rows.Count > 0)
                    {
                        sheet19.Name = (SheetIndex + 1) + "_Attendance_Not_Lock";
                        sheet19.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet19.Name = (SheetIndex + 1) + "_Attendance_Not_Lock";

                    }

                    #endregion Page Setup
                }
                catch (Exception ex)
                {

                }

                #endregion plantwisedatelock

                #region Legal Designation 18

                try
                {
                    IWorksheet sheet22 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet22 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;
                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet22.Range[5, igoto].Text = "Goto Index";
                    sheet22.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromLegalDesignation = sheet22.HyperLinks.Add(sheet22.Range[5, igoto]);
                    linkgofromLegalDesignation.Type = ExcelHyperLinkType.Workbook;
                    linkgofromLegalDesignation.TextToDisplay = sheet22.Range[5, igoto].Text;
                    linkgofromLegalDesignation.ScreenTip = "Go To " + sheet22.Range[5, igoto].Text;
                    linkgofromLegalDesignation.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet22.Range[xlsRow, isl].Text = "SL";
                    sheet22.Range[xlsRow, isl].ColumnWidth = 8;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet22.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet22.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet22.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet22.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet22.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet22.Range[xlsRow, iDesignation].ColumnWidth = 25;

                    sheet22.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet22.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet22.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet22.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    if (dtNotInLegalDesignationMaster.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtNotInLegalDesignationMaster.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet22.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet22.Range[xlsRow, iEmployeeCode].Text = dtNotInLegalDesignationMaster.Rows[i]["EmployeeCode"].ToString();

                            sheet22.Range[xlsRow, iEmployeeName].Text = dtNotInLegalDesignationMaster.Rows[i]["EmployeeName"].ToString();

                            sheet22.Range[xlsRow, iDesignation].Text = dtNotInLegalDesignationMaster.Rows[i]["LegalDesignation"].ToString();

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet22.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet22.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet22.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                    }
                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet22.GetColumnWidth(1) + sheet22.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet22.GetRowHeight(1) + sheet22.GetRowHeight(2) + sheet22.GetRowHeight(3) + sheet22.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet22.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet22.Range[xlsRow, 3].Text = CmpName;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet22.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet22.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet22.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet22.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet22.Range[xlsRow, 3].Text = FactoryName;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet22.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet22.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet22.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet22.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet22.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet22.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet22.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet22.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-NotIn LegalDesignation Master: " + FromDate + " To Date: " + ToDate;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet22.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet22.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet22.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet22.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet22.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet22.IsDisplayZeros = false;
                    sheet22.UsedRange["A7"].FreezePanes();
                    sheet22.FirstVisibleColumn = 1;
                    sheet22.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet22.UsedRange.WrapText = true;
                    sheet22.UsedRange.CellStyle.Font.Size = 8;
                    sheet22.Range["A1"].CellStyle.Font.Size = 14;
                    sheet22.Range["A2"].CellStyle.Font.Size = 10;
                    sheet22.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet22.PageSetup.TopMargin = 0.5;
                    sheet22.PageSetup.BottomMargin = 0.7;
                    sheet22.PageSetup.PrintTitleRows = "$1:$5";
                    sheet22.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet22.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet22.PageSetup.LeftMargin = 0.5;
                    sheet22.PageSetup.RightMargin = 0.2;
                    sheet22.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet22.PageSetup.FitToPagesTall = 0;
                    sheet22.PageSetup.FitToPagesWide = 1;
                    sheet22.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet22.IsDisplayZeros = false;

                    if (dtBankRemarks.Rows.Count > 0)
                    {
                        sheet22.Name = (SheetIndex + 1) + "_NotIn_Designation_Master";
                        sheet22.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet22.Name = (SheetIndex + 1) + "_NotIn_Designation_Master";
                    }

                    #endregion Page Setup
                }
                catch (Exception ex)
                {

                }

                #endregion Bank Remarks

                #region Salary Not Approved 19

                try
                {
                    IWorksheet sheet23 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet23 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;
                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet23.Range[5, igoto].Text = "Goto Index";
                    sheet23.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromApproved = sheet23.HyperLinks.Add(sheet23.Range[5, igoto]);
                    linkgofromApproved.Type = ExcelHyperLinkType.Workbook;
                    linkgofromApproved.TextToDisplay = sheet23.Range[5, igoto].Text;
                    linkgofromApproved.ScreenTip = "Go To " + sheet23.Range[5, igoto].Text;
                    linkgofromApproved.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet23.Range[xlsRow, isl].Text = "SL";
                    sheet23.Range[xlsRow, isl].ColumnWidth = 8;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet23.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet23.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet23.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet23.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet23.Range[xlsRow, iDepartment].Text = "Department";
                    sheet23.Range[xlsRow, iDepartment].ColumnWidth = 18;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet23.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet23.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet23.Range[xlsRow, iSection].Text = "Section";
                    sheet23.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet23.Range[xlsRow, iSubSection].Text = "Sub Section";
                    sheet23.Range[xlsRow, iSubSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet23.Range[xlsRow, iEntity].Text = "Entity";
                    sheet23.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iCurrentStatus = xlsCol;
                    sheet23.Range[xlsRow, iCurrentStatus].Text = "Employee Current Status";
                    sheet23.Range[xlsRow, iCurrentStatus].ColumnWidth = 22;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet23.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet23.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeStatus = xlsCol;
                    sheet23.Range[xlsRow, iEmployeeStatus].Text = "Employee Status";
                    sheet23.Range[xlsRow, iEmployeeStatus].ColumnWidth = 20;

                    xlsCol += 1;
                    iProcessedDate = xlsCol;
                    sheet23.Range[xlsRow, iProcessedDate].Text = "Processed Date";
                    sheet23.Range[xlsRow, iProcessedDate].ColumnWidth = 20;

                    xlsCol += 1;
                    iMonth = xlsCol;
                    sheet23.Range[xlsRow, iMonth].Text = "Month";
                    sheet23.Range[xlsRow, iMonth].ColumnWidth = 13;

                    xlsCol += 1;
                    iYear = xlsCol;
                    sheet23.Range[xlsRow, iYear].Text = "Year";
                    sheet23.Range[xlsRow, iYear].ColumnWidth = 13;

                    //xlsCol += 1;
                    //iEmployeeStatus = xlsCol;
                    //sheet23.Range[xlsRow, iEmployeeStatus].Text = "Employee Status";
                    //sheet23.Range[xlsRow, iEmployeeStatus].ColumnWidth = 13;

                    sheet23.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet23.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet23.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet23.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    if (dtSalaryNotApproved.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtSalaryNotApproved.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet23.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet23.Range[xlsRow, iEmployeeCode].Text = dtSalaryNotApproved.Rows[i]["EmployeeCode"].ToString();

                            sheet23.Range[xlsRow, iEmployeeName].Text = dtSalaryNotApproved.Rows[i]["EmployeeName"].ToString();

                            sheet23.Range[xlsRow, iDesignation].Text = dtSalaryNotApproved.Rows[i]["LegalDesignation"].ToString();

                            sheet23.Range[xlsRow, iDepartment].Text = dtSalaryNotApproved.Rows[i]["Department"].ToString();

                            sheet23.Range[xlsRow, iSection].Text = dtSalaryNotApproved.Rows[i]["Section"].ToString();

                            sheet23.Range[xlsRow, iSection].Text = dtSalaryNotApproved.Rows[i]["Section"].ToString();

                            sheet23.Range[xlsRow, iSubSection].Text = dtSalaryNotApproved.Rows[i]["SubSection"].ToString();
                            sheet23.Range[xlsRow, iEntity].Text = dtSalaryNotApproved.Rows[i]["EntityName"].ToString();
                            sheet23.Range[xlsRow, iCurrentStatus].Text = dtSalaryNotApproved.Rows[i]["EmployeeCurrentStatus"].ToString();
                            sheet23.Range[xlsRow, iEmployeeCategory].Text = dtSalaryNotApproved.Rows[i]["EmployeeCategory"].ToString();
                            sheet23.Range[xlsRow, iEmployeeStatus].Text = dtSalaryNotApproved.Rows[i]["EmployeeStatus"].ToString();
                            sheet23.Range[xlsRow, iMonth].Text = dtSalaryNotApproved.Rows[i]["MonthNo"].ToString();
                            sheet23.Range[xlsRow, iYear].Text = dtSalaryNotApproved.Rows[i]["YearNo"].ToString();
                            sheet23.Range[xlsRow, iProcessedDate].Text = dtSalaryNotApproved.Rows[i]["ProcessedDate"].ToString();

                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet23.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet23.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet23.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                    }
                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet23.GetColumnWidth(1) + sheet23.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet23.GetRowHeight(1) + sheet23.GetRowHeight(2) + sheet23.GetRowHeight(3) + sheet23.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet23.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet23.Range[xlsRow, 3].Text = CmpName;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet23.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet23.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet23.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet23.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet23.Range[xlsRow, 3].Text = FactoryName;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet23.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet23.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet23.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet23.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet23.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet23.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet23.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet23.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Salary Not Approved";
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet23.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet23.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet23.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet23.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet23.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet23.IsDisplayZeros = false;
                    sheet23.UsedRange["A7"].FreezePanes();
                    sheet23.FirstVisibleColumn = 1;
                    sheet23.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet23.UsedRange.WrapText = true;
                    sheet23.UsedRange.CellStyle.Font.Size = 8;
                    sheet23.Range["A1"].CellStyle.Font.Size = 14;
                    sheet23.Range["A2"].CellStyle.Font.Size = 10;
                    sheet23.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet23.PageSetup.TopMargin = 0.5;
                    sheet23.PageSetup.BottomMargin = 0.7;
                    sheet23.PageSetup.PrintTitleRows = "$1:$5";
                    sheet23.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet23.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet23.PageSetup.LeftMargin = 0.5;
                    sheet23.PageSetup.RightMargin = 0.2;
                    sheet23.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet23.PageSetup.FitToPagesTall = 0;
                    sheet23.PageSetup.FitToPagesWide = 1;
                    sheet23.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet23.IsDisplayZeros = false;

                    if (dtBankRemarks.Rows.Count > 0)
                    {
                        sheet23.Name = (SheetIndex + 1) + "_Salary_Not_Approved";
                        sheet23.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet23.Name = (SheetIndex + 1) + "_Salary_Not_Approved";
                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {

                }
                #endregion Bank Remarks

                #region Offday Week Punch 20

                try
                {
                    IWorksheet sheet24 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet24 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------

                    igoto = xlsCol;
                    sheet24.Range[5, igoto].Text = "Goto Index";
                    sheet24.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromApproved = sheet24.HyperLinks.Add(sheet24.Range[5, igoto]);
                    linkgofromApproved.Type = ExcelHyperLinkType.Workbook;
                    linkgofromApproved.TextToDisplay = sheet24.Range[5, igoto].Text;
                    linkgofromApproved.ScreenTip = "Go To " + sheet24.Range[5, igoto].Text;
                    linkgofromApproved.Address = "1_Index!A1";
                    //----------------------Test--------------//

                    //--------------------End Test------------//
                    isl = xlsCol;
                    sheet24.Range[xlsRow, isl].Text = "SL";
                    sheet24.Range[xlsRow, isl].ColumnWidth = 8;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet24.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet24.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet24.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet24.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet24.Range[xlsRow, iDepartment].Text = "Department";
                    sheet24.Range[xlsRow, iDepartment].ColumnWidth = 18;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet24.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet24.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet24.Range[xlsRow, iSection].Text = "Section";
                    sheet24.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet24.Range[xlsRow, iSubSection].Text = "Sub Section";
                    sheet24.Range[xlsRow, iSubSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet24.Range[xlsRow, iEntity].Text = "Entity";
                    sheet24.Range[xlsRow, iEntity].ColumnWidth = 18;

                    //
                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet24.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet24.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet24.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet24.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet24.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet24.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet24.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet24.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet24.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet24.Range[xlsRow, iShiftName].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet24.Range[xlsRow, iShiftInTime].Text = "Shift In Time";
                    sheet24.Range[xlsRow, iShiftInTime].ColumnWidth = 10;
                    sheet24.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet24.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet24.Range[xlsRow, iShiftOutTime].Text = "Shift Out Time";
                    sheet24.Range[xlsRow, iShiftOutTime].ColumnWidth = 10;
                    sheet24.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet24.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iInTime = xlsCol;
                    sheet24.Range[xlsRow, iInTime].Text = "In Time";
                    sheet24.Range[xlsRow, iInTime].ColumnWidth = 14;
                    sheet24.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet24.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet24.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet24.Range[xlsRow, iOutTime].Text = "Out Time";
                    sheet24.Range[xlsRow, iOutTime].ColumnWidth = 14;
                    sheet24.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet24.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet24.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet24.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet24.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet24.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet24.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    if (dtOffdayMissingPunch.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtOffdayMissingPunch.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet24.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet24.Range[xlsRow, iEmployeeCode].Text = dtOffdayMissingPunch.Rows[i]["EmployeeCode"].ToString();

                            sheet24.Range[xlsRow, iEmployeeName].Text = dtOffdayMissingPunch.Rows[i]["EmployeeName"].ToString();

                            sheet24.Range[xlsRow, iDesignation].Text = dtOffdayMissingPunch.Rows[i]["LegalDesignation"].ToString();

                            sheet24.Range[xlsRow, iDepartment].Text = dtOffdayMissingPunch.Rows[i]["Department"].ToString();

                            sheet24.Range[xlsRow, iSection].Text = dtOffdayMissingPunch.Rows[i]["Section"].ToString();

                            sheet24.Range[xlsRow, iSection].Text = dtOffdayMissingPunch.Rows[i]["Section"].ToString();

                            sheet24.Range[xlsRow, iSubSection].Text = dtOffdayMissingPunch.Rows[i]["SubSection"].ToString();
                            sheet24.Range[xlsRow, iEntity].Text = dtOffdayMissingPunch.Rows[i]["EntityName"].ToString();
                            sheet24.Range[xlsRow, iDOJ].Text = dtOffdayMissingPunch.Rows[i]["DOJ"].ToString();

                            sheet24.Range[xlsRow, iEmployeeCurrentStatus].Text = dtOffdayMissingPunch.Rows[i]["EmployeeCurrentStatus"].ToString();

                            sheet24.Range[xlsRow, iEmployeeCategory].Text = dtOffdayMissingPunch.Rows[i]["EmployeeCategory"].ToString();

                            sheet24.Range[xlsRow, iShiftName].Text = dtOffdayMissingPunch.Rows[i]["ShiftName"].ToString();

                            sheet24.Range[xlsRow, iWorkDate].Text = dtOffdayMissingPunch.Rows[i]["WorkDate"].ToString();

                            sheet24.Range[xlsRow, iShiftInTime].Text = dtOffdayMissingPunch.Rows[i]["ShiftInTime"].ToString();
                            sheet24.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet24.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet24.Range[xlsRow, iShiftOutTime].Text = dtOffdayMissingPunch.Rows[i]["ShiftOutTime"].ToString();
                            sheet24.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet24.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (dtOffdayMissingPunch.Rows[i]["InTime"].ToString() != "")
                            {
                                sheet24.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                sheet24.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtOffdayMissingPunch.Rows[i]["InTime"].ToString());
                                sheet24.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet24.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtOffdayMissingPunch.Rows[i]["IsManualInTime"].ToString().Trim()))
                                {
                                    sheet24.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }

                            if (dtOffdayMissingPunch.Rows[i]["OutTime"].ToString() != "")
                            {
                                sheet24.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                sheet24.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(dtOffdayMissingPunch.Rows[i]["OutTime"].ToString());
                                sheet24.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet24.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtOffdayMissingPunch.Rows[i]["IsManualOutTime"].ToString().Trim()))
                                {
                                    sheet24.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }


                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet24.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet24.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet24.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                    }
                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet24.GetColumnWidth(1) + sheet24.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet24.GetRowHeight(1) + sheet24.GetRowHeight(2) + sheet24.GetRowHeight(3) + sheet24.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet24.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet24.Range[xlsRow, 3].Text = CmpName;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet24.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet24.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet24.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet24.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet24.Range[xlsRow, 3].Text = FactoryName;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet24.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet24.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet24.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet24.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet24.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet24.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet24.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet24.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Offday Missing Punch";
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet24.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet24.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet24.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet24.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet24.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet24.IsDisplayZeros = false;
                    sheet24.UsedRange["A7"].FreezePanes();
                    sheet24.FirstVisibleColumn = 1;
                    sheet24.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet24.UsedRange.WrapText = true;
                    sheet24.UsedRange.CellStyle.Font.Size = 8;
                    sheet24.Range["A1"].CellStyle.Font.Size = 14;
                    sheet24.Range["A2"].CellStyle.Font.Size = 10;
                    sheet24.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet24.PageSetup.TopMargin = 0.5;
                    sheet24.PageSetup.BottomMargin = 0.7;
                    sheet24.PageSetup.PrintTitleRows = "$1:$5";
                    sheet24.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet24.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet24.PageSetup.LeftMargin = 0.5;
                    sheet24.PageSetup.RightMargin = 0.2;
                    sheet24.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet24.PageSetup.FitToPagesTall = 0;
                    sheet24.PageSetup.FitToPagesWide = 1;
                    sheet24.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet24.IsDisplayZeros = false;

                    if (dtBankRemarks.Rows.Count > 0)
                    {
                        sheet24.Name = (SheetIndex + 1) + "_Offday_Missing_Punch";
                        sheet24.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet24.Name = (SheetIndex + 1) + "_Offday_Missing_Punch";
                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                #endregion Offday Week Punch 23

                #region Offday_With_Punch 21

                try
                {
                    IWorksheet sheet25 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet25 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------

                    igoto = xlsCol;
                    sheet25.Range[5, igoto].Text = "Goto Index";
                    sheet25.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromApproved = sheet25.HyperLinks.Add(sheet25.Range[5, igoto]);
                    linkgofromApproved.Type = ExcelHyperLinkType.Workbook;
                    linkgofromApproved.TextToDisplay = sheet25.Range[5, igoto].Text;
                    linkgofromApproved.ScreenTip = "Go To " + sheet25.Range[5, igoto].Text;
                    linkgofromApproved.Address = "1_Index!A1";
                    //----------------------Test--------------//

                    //--------------------End Test------------//
                    isl = xlsCol;
                    sheet25.Range[xlsRow, isl].Text = "SL";
                    sheet25.Range[xlsRow, isl].ColumnWidth = 8;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet25.Range[xlsRow, iEmployeeCode].Text = "Code";
                    sheet25.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet25.Range[xlsRow, iEmployeeName].Text = "Name";
                    sheet25.Range[xlsRow, iEmployeeName].ColumnWidth = 20;


                    xlsCol += 1;
                    iTelephoneNo = xlsCol;
                    sheet25.Range[xlsRow, iTelephoneNo].Text = "Telephone No.";
                    sheet25.Range[xlsRow, iTelephoneNo].ColumnWidth = 20;



                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet25.Range[xlsRow, iDepartment].Text = "Department";
                    sheet25.Range[xlsRow, iDepartment].ColumnWidth = 18;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet25.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet25.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet25.Range[xlsRow, iSection].Text = "Section";
                    sheet25.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet25.Range[xlsRow, iSubSection].Text = "Sub Section";
                    sheet25.Range[xlsRow, iSubSection].ColumnWidth = 18;


                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet25.Range[xlsRow, iEntity].Text = "Entity";
                    sheet25.Range[xlsRow, iEntity].ColumnWidth = 18;

                    //
                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet25.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet25.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCurrentStatus = xlsCol;
                    sheet25.Range[xlsRow, iEmployeeCurrentStatus].Text = "Employee Current Status";
                    sheet25.Range[xlsRow, iEmployeeCurrentStatus].ColumnWidth = 15;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet25.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet25.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iWorkDate = xlsCol;
                    sheet25.Range[xlsRow, iWorkDate].Text = "Work Date";
                    sheet25.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftName = xlsCol;
                    sheet25.Range[xlsRow, iShiftName].Text = "Shift Name";
                    sheet25.Range[xlsRow, iShiftName].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet25.Range[xlsRow, iShiftInTime].Text = "Shift In Time";
                    sheet25.Range[xlsRow, iShiftInTime].ColumnWidth = 10;
                    sheet25.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet25.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet25.Range[xlsRow, iShiftOutTime].Text = "Shift Out Time";
                    sheet25.Range[xlsRow, iShiftOutTime].ColumnWidth = 10;
                    sheet25.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet25.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iInTime = xlsCol;
                    sheet25.Range[xlsRow, iInTime].Text = "In Time";
                    sheet25.Range[xlsRow, iInTime].ColumnWidth = 14;
                    sheet25.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet25.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet25.Range[xlsRow, iOutTime].Text = "Out Time";
                    sheet25.Range[xlsRow, iOutTime].ColumnWidth = 14;
                    sheet25.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet25.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    xlsCol += 1;
                    iProcessedOT = xlsCol;
                    sheet25.Range[xlsRow, iProcessedOT].Text = "ProcessedOT";
                    sheet25.Range[xlsRow, iProcessedOT].ColumnWidth = 14;
                    sheet25.Range[xlsRow, iProcessedOT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet25.Range[xlsRow, iProcessedOT].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, iProcessedOT].CellStyle.Font.Color = ExcelKnownColors.Red;



                    sheet25.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet25.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet25.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet25.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    if (dtOffdayWithPunch.Rows.Count > 0)
                    {
                        SLNo = 1;
                        for (int i = 0; i < dtOffdayWithPunch.Rows.Count; i++)
                        {
                            #region ----------------------Data-----------------------
                            sheet25.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet25.Range[xlsRow, iEmployeeCode].Text = dtOffdayWithPunch.Rows[i]["EmployeeCode"].ToString();

                            sheet25.Range[xlsRow, iEmployeeName].Text = dtOffdayWithPunch.Rows[i]["EmployeeName"].ToString();

                            sheet25.Range[xlsRow, iDesignation].Text = dtOffdayWithPunch.Rows[i]["LegalDesignation"].ToString();

                            sheet25.Range[xlsRow, iDepartment].Text = dtOffdayWithPunch.Rows[i]["Department"].ToString();

                            sheet25.Range[xlsRow, iTelephoneNo].Text = dtOffdayWithPunch.Rows[i]["TelePhnNo"].ToString();

                            sheet25.Range[xlsRow, iSection].Text = dtOffdayWithPunch.Rows[i]["Section"].ToString();

                            sheet25.Range[xlsRow, iWorkDate].Text = dtOffdayWithPunch.Rows[i]["WorkDate"].ToString();

                            sheet25.Range[xlsRow, iSection].Text = dtOffdayWithPunch.Rows[i]["Section"].ToString();

                            sheet25.Range[xlsRow, iSubSection].Text = dtOffdayWithPunch.Rows[i]["SubSection"].ToString();
                            sheet25.Range[xlsRow, iEntity].Text = dtOffdayWithPunch.Rows[i]["EntityName"].ToString();
                            sheet25.Range[xlsRow, iDOJ].Text = dtOffdayWithPunch.Rows[i]["DOJ"].ToString();

                            sheet25.Range[xlsRow, iEmployeeCurrentStatus].Text = dtOffdayWithPunch.Rows[i]["EmployeeCurrentStatus"].ToString();

                            sheet25.Range[xlsRow, iEmployeeCategory].Text = dtOffdayWithPunch.Rows[i]["EmployeeCategory"].ToString();

                            sheet25.Range[xlsRow, iShiftName].Text = dtOffdayWithPunch.Rows[i]["ShiftName"].ToString();

                            sheet25.Range[xlsRow, iShiftInTime].Text = dtOffdayWithPunch.Rows[i]["ShiftInTime"].ToString();
                            sheet25.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet25.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            sheet25.Range[xlsRow, iShiftOutTime].Text = dtOffdayWithPunch.Rows[i]["ShiftOutTime"].ToString();
                            sheet25.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet25.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            string xprocessot = string.Empty;
                            if (dtOffdayWithPunch.Rows[i]["ProcessedOT"].ToString() != "")
                            {
                                oru.GetOT(OTConsiderOn, dtOffdayWithPunch.Rows[i]["ProcessedOT"].ToString().Trim(), out xprocessot);
                            }
                            sheet25.Range[xlsRow, iProcessedOT].Text = xprocessot;
                            sheet25.Range[xlsRow, iProcessedOT].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet25.Range[xlsRow, iProcessedOT].VerticalAlignment = ExcelVAlign.VAlignCenter;

                            if (dtOffdayWithPunch.Rows[i]["InTime"].ToString() != "")
                            {
                                sheet25.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                sheet25.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtOffdayWithPunch.Rows[i]["InTime"].ToString());
                                sheet25.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet25.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtOffdayWithPunch.Rows[i]["IsManualInTime"].ToString().Trim()))
                                {
                                    sheet25.Range[xlsRow, iInTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }

                            if (dtOffdayWithPunch.Rows[i]["OutTime"].ToString() != "")
                            {
                                sheet25.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                sheet25.Range[xlsRow, iOutTime].DateTime = Convert.ToDateTime(dtOffdayWithPunch.Rows[i]["OutTime"].ToString());
                                sheet25.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet25.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                                if (bplib.clsWebLib.GetBoolData(dtOffdayWithPunch.Rows[i]["IsManualOutTime"].ToString().Trim()))
                                {
                                    sheet25.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Orange;
                                }
                            }


                            xlsRow++;
                            SLNo++;
                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet25.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet25.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet25.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup

                    }
                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet25.GetColumnWidth(1) + sheet25.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet25.GetRowHeight(1) + sheet25.GetRowHeight(2) + sheet25.GetRowHeight(3) + sheet25.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet25.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }
                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet25.Range[xlsRow, 3].Text = CmpName;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet25.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet25.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet25.Range[xlsRow, 3].Text = FactoryName;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet25.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet25.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet25.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    sheet25.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet25.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet25.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Offday With Punch";
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet25.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet25.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet25.IsDisplayZeros = false;
                    sheet25.UsedRange["A7"].FreezePanes();
                    sheet25.FirstVisibleColumn = 1;
                    sheet25.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet25.UsedRange.WrapText = true;
                    sheet25.UsedRange.CellStyle.Font.Size = 8;
                    sheet25.Range["A1"].CellStyle.Font.Size = 14;
                    sheet25.Range["A2"].CellStyle.Font.Size = 10;
                    sheet25.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet25.PageSetup.TopMargin = 0.5;
                    sheet25.PageSetup.BottomMargin = 0.7;
                    sheet25.PageSetup.PrintTitleRows = "$1:$5";
                    sheet25.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet25.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet25.PageSetup.LeftMargin = 0.5;
                    sheet25.PageSetup.RightMargin = 0.2;
                    sheet25.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet25.PageSetup.FitToPagesTall = 0;
                    sheet25.PageSetup.FitToPagesWide = 1;
                    sheet25.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet25.IsDisplayZeros = false;

                    if (dtBankRemarks.Rows.Count > 0)
                    {
                        sheet25.Name = (SheetIndex + 1) + "_Offday_With_Punch";
                        sheet25.TabColorRGB = Color.Red;
                    }
                    else
                    {
                        sheet25.Name = (SheetIndex + 1) + "_Offday_With_Punch";
                    }

                    #endregion Page Setup

                }
                catch (Exception ex)
                {

                }
                #endregion Offday_With_Punch 
                               
                #region  Shift NOT Assign 22
                try
                {
                    IWorksheet sheet25 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    //var iDayStatus = 0;
                    SheetIndex++;
                    sheet25 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet25.Range[5, igoto].Text = "Goto Index";
                    sheet25.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkgofromAbsentNoPunchTime = sheet25.HyperLinks.Add(sheet25.Range[5, igoto]);
                    linkgofromAbsentNoPunchTime.Type = ExcelHyperLinkType.Workbook;
                    linkgofromAbsentNoPunchTime.TextToDisplay = sheet25.Range[5, igoto].Text;
                    linkgofromAbsentNoPunchTime.ScreenTip = "Go To " + sheet25.Range[5, igoto].Text;
                    linkgofromAbsentNoPunchTime.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet25.Range[xlsRow, isl].Text = "SL";
                    sheet25.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet25.Range[xlsRow, iEmployeeCode].Text = "Employee Code";
                    sheet25.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet25.Range[xlsRow, iEmployeeName].Text = "Employee Name";
                    sheet25.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet25.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet25.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet25.Range[xlsRow, iDepartment].Text = "Department";
                    sheet25.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet25.Range[xlsRow, iSection].Text = "Section";
                    sheet25.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet25.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet25.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet25.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet25.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet25.Range[xlsRow, iEntity].Text = "Entity";
                    sheet25.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet25.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet25.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iShiftInTime = xlsCol;
                    sheet25.Range[xlsRow, iShiftInTime].Text = "Effective Date";
                    sheet25.Range[xlsRow, iShiftInTime].ColumnWidth = 18;
                    sheet25.Range[xlsRow, iShiftInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet25.Range[xlsRow, iShiftInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iShiftOutTime = xlsCol;
                    sheet25.Range[xlsRow, iShiftOutTime].Text = "CutOff Date";
                    sheet25.Range[xlsRow, iShiftOutTime].ColumnWidth = 18;
                    sheet25.Range[xlsRow, iShiftOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet25.Range[xlsRow, iShiftOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsCol += 1;
                    iOutTime = xlsCol;
                    sheet25.Range[xlsRow, iOutTime].Text = "Flag";
                    sheet25.Range[xlsRow, iOutTime].ColumnWidth = 14;
                    sheet25.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet25.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, iOutTime].CellStyle.Font.Color = ExcelKnownColors.Red;

                    sheet25.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet25.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet25.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet25.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    SLNo = 1;
                    if (dtShiftUnassign.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtShiftUnassign.Rows.Count; i++)
                        {

                            #region ----------------------Data-----------------------
                            sheet25.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet25.Range[xlsRow, iEmployeeCode].Text = dtShiftUnassign.Rows[i]["EmployeeCode"].ToString();

                            sheet25.Range[xlsRow, iEmployeeName].Text = dtShiftUnassign.Rows[i]["EmployeeName"].ToString();

                            sheet25.Range[xlsRow, iEmployeeCategory].Text = dtShiftUnassign.Rows[i]["EmpCategory"].ToString();

                            sheet25.Range[xlsRow, iDepartment].Text = dtShiftUnassign.Rows[i]["Department"].ToString();

                            sheet25.Range[xlsRow, iDesignation].Text = dtShiftUnassign.Rows[i]["Designation"].ToString();

                            sheet25.Range[xlsRow, iSection].Text = dtShiftUnassign.Rows[i]["Section"].ToString();

                            sheet25.Range[xlsRow, iSubSection].Text = dtShiftUnassign.Rows[i]["SubSection"].ToString();

                            sheet25.Range[xlsRow, iEntity].Text = dtShiftUnassign.Rows[i]["Entity"].ToString();

                            sheet25.Range[xlsRow, iDOJ].Text = dtShiftUnassign.Rows[i]["DOJ"].ToString();

                            sheet25.Range[xlsRow, iShiftInTime].Text = dtShiftUnassign.Rows[i]["EffectiveDate"].ToString();

                            //sheet25.Range[xlsRow, iInTime].Text = dtShiftUnassign.Rows[i]["BudgetCode"].ToString();

                            sheet25.Range[xlsRow, iShiftOutTime].Text = dtShiftUnassign.Rows[i]["CutOffDate"].ToString();

                            sheet25.Range[xlsRow, iOutTime].Text = dtShiftUnassign.Rows[i]["flag"].ToString();

                            xlsRow++;
                            SLNo++;

                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet25.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet25.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet25.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup


                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet25.GetColumnWidth(1) + sheet25.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet25.GetRowHeight(1) + sheet25.GetRowHeight(2) + sheet25.GetRowHeight(3) + sheet25.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet25.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet25.Range[xlsRow, 3].Text = CmpName;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet25.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet25.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {

                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet25.Range[xlsRow, 3].Text = FactoryName;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet25.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet25.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet25.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Size = 22;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet25.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet25.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet25.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-Shift Not Assign";
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet25.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet25.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet25.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet25.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet25.IsDisplayZeros = false;
                    sheet25.UsedRange["A7"].FreezePanes();
                    sheet25.FirstVisibleColumn = 1;
                    sheet25.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet25.UsedRange.WrapText = true;
                    sheet25.UsedRange.CellStyle.Font.Size = 8;
                    sheet25.Range["A1"].CellStyle.Font.Size = 14;
                    sheet25.Range["A2"].CellStyle.Font.Size = 10;
                    sheet25.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet25.PageSetup.TopMargin = 0.5;
                    sheet25.PageSetup.BottomMargin = 0.7;
                    sheet25.PageSetup.PrintTitleRows = "$1:$5";
                    sheet25.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet25.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet25.PageSetup.LeftMargin = 0.5;
                    sheet25.PageSetup.RightMargin = 0.2;
                    sheet25.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet25.PageSetup.FitToPagesTall = 0;
                    sheet25.PageSetup.FitToPagesWide = 1;
                    sheet25.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet25.IsDisplayZeros = false;

                    if (dtShiftUnassign.Rows.Count > 0)
                    {
                        sheet25.Name = (SheetIndex + 1) + "_Shift_Not_Assign";
                        sheet25.TabColorRGB = Color.Red;

                    }
                    else
                    {
                        sheet25.Name = (SheetIndex + 1) + "_Shift_Not_Assign";
                    }
                    #endregion Page Setup


                }
                catch (Exception )
                {
                }
                #endregion  

                #region  InActive Employees With RawData Punches 23
                try
                {
                    IWorksheet sheet28 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet28 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet28.Range[5, igoto].Text = "Goto Index";
                    sheet28.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkSeparatedEmpPunches = sheet28.HyperLinks.Add(sheet28.Range[5, igoto]);
                    linkSeparatedEmpPunches.Type = ExcelHyperLinkType.Workbook;
                    linkSeparatedEmpPunches.TextToDisplay = sheet28.Range[5, igoto].Text;
                    linkSeparatedEmpPunches.ScreenTip = "Go To " + sheet28.Range[5, igoto].Text;
                    linkSeparatedEmpPunches.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet28.Range[xlsRow, isl].Text = "SL";
                    sheet28.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet28.Range[xlsRow, iEmployeeCode].Text = "Employee Code";
                    sheet28.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet28.Range[xlsRow, iEmployeeName].Text = "Employee Name";
                    sheet28.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet28.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet28.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet28.Range[xlsRow, iDepartment].Text = "Department";
                    sheet28.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet28.Range[xlsRow, iSection].Text = "Section";
                    sheet28.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet28.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet28.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet28.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet28.Range[xlsRow, iEmployeeCategory].ColumnWidth = 15;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet28.Range[xlsRow, iEntity].Text = "Entity";
                    sheet28.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet28.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet28.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOS = xlsCol;
                    sheet28.Range[xlsRow, iDOS].Text = "DOS";
                    sheet28.Range[xlsRow, iDOS].ColumnWidth = 18;

                    xlsCol += 1;
                    iRawPunch = xlsCol;
                    sheet28.Range[xlsRow, iRawPunch].Text = "PunchTime";
                    sheet28.Range[xlsRow, iRawPunch].ColumnWidth = 18;



                    sheet28.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet28.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet28.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet28.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    SLNo = 1;
                    if (dtSeparatedEmpWithPunches.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtSeparatedEmpWithPunches.Rows.Count; i++)
                        {

                            #region ----------------------Data-----------------------
                            sheet28.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet28.Range[xlsRow, iEmployeeCode].Text = dtSeparatedEmpWithPunches.Rows[i]["EmployeeCode"].ToString();

                            sheet28.Range[xlsRow, iEmployeeName].Text = dtSeparatedEmpWithPunches.Rows[i]["EmployeeName"].ToString();

                            sheet28.Range[xlsRow, iEmployeeCategory].Text = dtSeparatedEmpWithPunches.Rows[i]["EmpCategory"].ToString();

                            sheet28.Range[xlsRow, iDepartment].Text = dtSeparatedEmpWithPunches.Rows[i]["Department"].ToString();

                            sheet28.Range[xlsRow, iDesignation].Text = dtSeparatedEmpWithPunches.Rows[i]["Designation"].ToString();

                            sheet28.Range[xlsRow, iSection].Text = dtSeparatedEmpWithPunches.Rows[i]["Section"].ToString();

                            sheet28.Range[xlsRow, iSubSection].Text = dtSeparatedEmpWithPunches.Rows[i]["SubSection"].ToString();

                            sheet28.Range[xlsRow, iEntity].Text = dtSeparatedEmpWithPunches.Rows[i]["Entity"].ToString();

                            sheet28.Range[xlsRow, iDOJ].Text = dtSeparatedEmpWithPunches.Rows[i]["DOJ"].ToString();
                            sheet28.Range[xlsRow, iDOS].Text = dtSeparatedEmpWithPunches.Rows[i]["DOS"].ToString();

                            sheet28.Range[xlsRow, iRawPunch].Text = dtSeparatedEmpWithPunches.Rows[i]["PunchTime"].ToString();

                            xlsRow++;
                            SLNo++;

                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet28.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet28.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet28.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup


                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet28.GetColumnWidth(1) + sheet28.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet28.GetRowHeight(1) + sheet28.GetRowHeight(2) + sheet28.GetRowHeight(3) + sheet28.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet28.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet28.Range[xlsRow, 3].Text = CmpName;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet28.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet28.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet28.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet28.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {

                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet28.Range[xlsRow, 3].Text = FactoryName;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet28.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet28.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet28.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet28.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet28.Range[xlsRow, 3].CellStyle.Font.Size = 22;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet28.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet28.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    sheet28.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-InActive Employees Punches";
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet28.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet28.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet28.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet28.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet28.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet28.IsDisplayZeros = false;
                    sheet28.UsedRange["A7"].FreezePanes();
                    sheet28.FirstVisibleColumn = 1;
                    sheet28.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet28.UsedRange.WrapText = true;
                    sheet28.UsedRange.CellStyle.Font.Size = 8;
                    sheet28.Range["A1"].CellStyle.Font.Size = 14;
                    sheet28.Range["A2"].CellStyle.Font.Size = 10;
                    sheet28.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet28.PageSetup.TopMargin = 0.5;
                    sheet28.PageSetup.BottomMargin = 0.7;
                    sheet28.PageSetup.PrintTitleRows = "$1:$5";
                    sheet28.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet28.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet28.PageSetup.LeftMargin = 0.5;
                    sheet28.PageSetup.RightMargin = 0.2;
                    sheet28.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet28.PageSetup.FitToPagesTall = 0;
                    sheet28.PageSetup.FitToPagesWide = 1;
                    sheet28.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet28.IsDisplayZeros = false;

                    if (dtSeparatedEmpWithPunches.Rows.Count > 0)
                    {
                        sheet28.Name = (SheetIndex + 1) + "_InActive_Emp_Punches";
                        sheet28.TabColorRGB = Color.Red;

                    }
                    else
                    {
                        sheet28.Name = (SheetIndex + 1) + "_InActive_Emp_Punches";
                    }
                    #endregion Page Setup


                }
                catch (Exception)
                {
                }
                #endregion  InActive Employees With RawData Punches 23

                #region  ManualIn Entries of Employees 24
                try
                {
                    IWorksheet sheet29 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet29 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet29.Range[5, igoto].Text = "Goto Index";
                    sheet29.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkManualInPunch = sheet29.HyperLinks.Add(sheet29.Range[5, igoto]);
                    linkManualInPunch.Type = ExcelHyperLinkType.Workbook;
                    linkManualInPunch.TextToDisplay = sheet29.Range[5, igoto].Text;
                    linkManualInPunch.ScreenTip = "Go To " + sheet29.Range[5, igoto].Text;
                    linkManualInPunch.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet29.Range[xlsRow, isl].Text = "SL";
                    sheet29.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet29.Range[xlsRow, iEmployeeCode].Text = "Employee Code";
                    sheet29.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet29.Range[xlsRow, iEmployeeName].Text = "Employee Name";
                    sheet29.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet29.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet29.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet29.Range[xlsRow, iDepartment].Text = "Department";
                    sheet29.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet29.Range[xlsRow, iSection].Text = "Section";
                    sheet29.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet29.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet29.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet29.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet29.Range[xlsRow, iEmployeeCategory].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet29.Range[xlsRow, iEntity].Text = "Entity";
                    sheet29.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet29.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet29.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iRawPunch = xlsCol;
                    sheet29.Range[xlsRow, iRawPunch].Text = "PunchTime";
                    sheet29.Range[xlsRow, iRawPunch].ColumnWidth = 18;

                    xlsCol += 1;
                    iManualByWhom = xlsCol;
                    sheet29.Range[xlsRow, iManualByWhom].Text = "By Whom";
                    sheet29.Range[xlsRow, iManualByWhom].ColumnWidth = 18;


                    sheet29.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet29.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet29.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet29.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    SLNo = 1;
                    if (dtManualInEntry.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtManualInEntry.Rows.Count; i++)
                        {

                            #region ----------------------Data-----------------------
                            sheet29.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet29.Range[xlsRow, iEmployeeCode].Text = dtManualInEntry.Rows[i]["EmployeeCode"].ToString();

                            sheet29.Range[xlsRow, iEmployeeName].Text = dtManualInEntry.Rows[i]["EmployeeName"].ToString();

                            sheet29.Range[xlsRow, iEmployeeCategory].Text = dtManualInEntry.Rows[i]["EmpCategory"].ToString();

                            sheet29.Range[xlsRow, iDepartment].Text = dtManualInEntry.Rows[i]["Department"].ToString();

                            sheet29.Range[xlsRow, iDesignation].Text = dtManualInEntry.Rows[i]["Designation"].ToString();

                            sheet29.Range[xlsRow, iSection].Text = dtManualInEntry.Rows[i]["Section"].ToString();

                            sheet29.Range[xlsRow, iSubSection].Text = dtManualInEntry.Rows[i]["SubSection"].ToString();

                            sheet29.Range[xlsRow, iEntity].Text = dtManualInEntry.Rows[i]["Entity"].ToString();

                            sheet29.Range[xlsRow, iDOJ].Text = dtManualInEntry.Rows[i]["DOJ"].ToString();

                            sheet29.Range[xlsRow, iRawPunch].Text = dtManualInEntry.Rows[i]["PunchTime"].ToString();
                          
                            sheet29.Range[xlsRow, iManualByWhom].Text = dtManualInEntry.Rows[i]["ManualByWhom"].ToString();

                            xlsRow++;
                            SLNo++;

                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet29.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet29.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet29.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup


                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet29.GetColumnWidth(1) + sheet29.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet29.GetRowHeight(1) + sheet29.GetRowHeight(2) + sheet29.GetRowHeight(3) + sheet29.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet29.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet29.Range[xlsRow, 3].Text = CmpName;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet29.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet29.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet29.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet29.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {

                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet29.Range[xlsRow, 3].Text = FactoryName;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet29.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet29.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet29.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet29.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet29.Range[xlsRow, 3].CellStyle.Font.Size = 22;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet29.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet29.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    sheet29.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-ManualIn Entries";
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet29.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet29.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet29.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet29.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet29.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet29.IsDisplayZeros = false;
                    sheet29.UsedRange["A7"].FreezePanes();
                    sheet29.FirstVisibleColumn = 1;
                    sheet29.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet29.UsedRange.WrapText = true;
                    sheet29.UsedRange.CellStyle.Font.Size = 8;
                    sheet29.Range["A1"].CellStyle.Font.Size = 14;
                    sheet29.Range["A2"].CellStyle.Font.Size = 10;
                    sheet29.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet29.PageSetup.TopMargin = 0.5;
                    sheet29.PageSetup.BottomMargin = 0.7;
                    sheet29.PageSetup.PrintTitleRows = "$1:$5";
                    sheet29.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet29.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet29.PageSetup.LeftMargin = 0.5;
                    sheet29.PageSetup.RightMargin = 0.2;
                    sheet29.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet29.PageSetup.FitToPagesTall = 0;
                    sheet29.PageSetup.FitToPagesWide = 1;
                    sheet29.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet29.IsDisplayZeros = false;

                    if (dtManualInEntry.Rows.Count > 0)
                    {
                        sheet29.Name = (SheetIndex + 1) + "_ManualIn_Entry";
                        sheet29.TabColorRGB = Color.Red;

                    }
                    else
                    {
                        sheet29.Name = (SheetIndex + 1) + "_ManualIn_Entry";
                    }
                    #endregion Page Setup


                }
                catch (Exception)
                {
                }
                #endregion

                #region  ManualOut Entries of Employees 25
                try
                {
                    IWorksheet sheet30 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet30 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet30.Range[5, igoto].Text = "Goto Index";
                    sheet30.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkManualOutPunch = sheet30.HyperLinks.Add(sheet30.Range[5, igoto]);
                    linkManualOutPunch.Type = ExcelHyperLinkType.Workbook;
                    linkManualOutPunch.TextToDisplay = sheet30.Range[5, igoto].Text;
                    linkManualOutPunch.ScreenTip = "Go To " + sheet30.Range[5, igoto].Text;
                    linkManualOutPunch.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet30.Range[xlsRow, isl].Text = "SL";
                    sheet30.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet30.Range[xlsRow, iEmployeeCode].Text = "Employee Code";
                    sheet30.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet30.Range[xlsRow, iEmployeeName].Text = "Employee Name";
                    sheet30.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet30.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet30.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet30.Range[xlsRow, iDepartment].Text = "Department";
                    sheet30.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet30.Range[xlsRow, iSection].Text = "Section";
                    sheet30.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet30.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet30.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet30.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet30.Range[xlsRow, iEmployeeCategory].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet30.Range[xlsRow, iEntity].Text = "Entity";
                    sheet30.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet30.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet30.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iRawPunch = xlsCol;
                    sheet30.Range[xlsRow, iRawPunch].Text = "PunchTime";
                    sheet30.Range[xlsRow, iRawPunch].ColumnWidth = 18;

                    xlsCol += 1;
                    iManualByWhom = xlsCol;
                    sheet30.Range[xlsRow, iManualByWhom].Text = "By Whom";
                    sheet30.Range[xlsRow, iManualByWhom].ColumnWidth = 18;


                    sheet30.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet30.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet30.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet30.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    SLNo = 1;
                    if (dtManualOutEntry.Rows.Count > 0)
                    {
                        #region ----------------------Data-----------------------

                        for (int i = 0; i < dtManualOutEntry.Rows.Count; i++)
                        {

                            sheet30.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet30.Range[xlsRow, iEmployeeCode].Text = dtManualOutEntry.Rows[i]["EmployeeCode"].ToString();

                            sheet30.Range[xlsRow, iEmployeeName].Text = dtManualOutEntry.Rows[i]["EmployeeName"].ToString();

                            sheet30.Range[xlsRow, iEmployeeCategory].Text = dtManualOutEntry.Rows[i]["EmpCategory"].ToString();

                            sheet30.Range[xlsRow, iDepartment].Text = dtManualOutEntry.Rows[i]["Department"].ToString();

                            sheet30.Range[xlsRow, iDesignation].Text = dtManualOutEntry.Rows[i]["Designation"].ToString();

                            sheet30.Range[xlsRow, iSection].Text = dtManualOutEntry.Rows[i]["Section"].ToString();

                            sheet30.Range[xlsRow, iSubSection].Text = dtManualOutEntry.Rows[i]["SubSection"].ToString();

                            sheet30.Range[xlsRow, iEntity].Text = dtManualOutEntry.Rows[i]["Entity"].ToString();

                            sheet30.Range[xlsRow, iDOJ].Text = dtManualOutEntry.Rows[i]["DOJ"].ToString();

                            sheet30.Range[xlsRow, iRawPunch].Text = dtManualOutEntry.Rows[i]["PunchTime"].ToString();

                            sheet30.Range[xlsRow, iManualByWhom].Text = dtManualOutEntry.Rows[i]["ManualByWhom"].ToString();

                            xlsRow++;
                            SLNo++;

                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet30.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet30.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet30.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup


                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet30.GetColumnWidth(1) + sheet30.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet30.GetRowHeight(1) + sheet30.GetRowHeight(2) + sheet30.GetRowHeight(3) + sheet30.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet30.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet30.Range[xlsRow, 3].Text = CmpName;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet30.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet30.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet30.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet30.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {

                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet30.Range[xlsRow, 3].Text = FactoryName;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet30.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet30.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet30.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet30.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet30.Range[xlsRow, 3].CellStyle.Font.Size = 22;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet30.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet30.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    sheet30.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-ManualOut Entries";
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet30.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet30.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet30.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet30.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet30.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet30.IsDisplayZeros = false;
                    sheet30.UsedRange["A7"].FreezePanes();
                    sheet30.FirstVisibleColumn = 1;
                    sheet30.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet30.UsedRange.WrapText = true;
                    sheet30.UsedRange.CellStyle.Font.Size = 8;
                    sheet30.Range["A1"].CellStyle.Font.Size = 14;
                    sheet30.Range["A2"].CellStyle.Font.Size = 10;
                    sheet30.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet30.PageSetup.TopMargin = 0.5;
                    sheet30.PageSetup.BottomMargin = 0.7;
                    sheet30.PageSetup.PrintTitleRows = "$1:$5";
                    sheet30.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet30.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet30.PageSetup.LeftMargin = 0.5;
                    sheet30.PageSetup.RightMargin = 0.2;
                    sheet30.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet30.PageSetup.FitToPagesTall = 0;
                    sheet30.PageSetup.FitToPagesWide = 1;
                    sheet30.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet30.IsDisplayZeros = false;

                    if (dtManualOutEntry.Rows.Count > 0)
                    {
                        sheet30.Name = (SheetIndex + 1) + "_ManualOut_Entry";
                        sheet30.TabColorRGB = Color.Red;

                    }
                    else
                    {
                        sheet30.Name = (SheetIndex + 1) + "_ManualOut_Entry";
                    }
                    #endregion Page Setup


                }
                catch (Exception)
                {
                }
                #endregion

                #region  ManualDayStatus Entries of Employees 26
                try
                {
                    IWorksheet sheet31 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet31 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet31.Range[5, igoto].Text = "Goto Index";
                    sheet31.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkManualDayStatus = sheet31.HyperLinks.Add(sheet31.Range[5, igoto]);
                    linkManualDayStatus.Type = ExcelHyperLinkType.Workbook;
                    linkManualDayStatus.TextToDisplay = sheet31.Range[5, igoto].Text;
                    linkManualDayStatus.ScreenTip = "Go To " + sheet31.Range[5, igoto].Text;
                    linkManualDayStatus.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet31.Range[xlsRow, isl].Text = "SL";
                    sheet31.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet31.Range[xlsRow, iEmployeeCode].Text = "Employee Code";
                    sheet31.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet31.Range[xlsRow, iEmployeeName].Text = "Employee Name";
                    sheet31.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet31.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet31.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet31.Range[xlsRow, iDepartment].Text = "Department";
                    sheet31.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet31.Range[xlsRow, iSection].Text = "Section";
                    sheet31.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet31.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet31.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet31.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet31.Range[xlsRow, iEmployeeCategory].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet31.Range[xlsRow, iEntity].Text = "Entity";
                    sheet31.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet31.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet31.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    WorkDate = xlsCol;
                    sheet31.Range[xlsRow, WorkDate].Text = "Work Date";
                    sheet31.Range[xlsRow, WorkDate].ColumnWidth = 18;

                    xlsCol += 1;
                    iManualDayStatus = xlsCol;
                    sheet31.Range[xlsRow, iManualDayStatus].Text = "ManualDayStaus";
                    sheet31.Range[xlsRow, iManualDayStatus].ColumnWidth = 18;

                    xlsCol += 1;
                    iManualByWhom = xlsCol;
                    sheet31.Range[xlsRow, iManualByWhom].Text = "By Whom";
                    sheet31.Range[xlsRow, iManualByWhom].ColumnWidth = 18;


                    sheet31.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet31.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet31.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet31.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    SLNo = 1;
                    if (dtManualDayStatusEntry.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtManualDayStatusEntry.Rows.Count; i++)
                        {

                            #region ----------------------Data-----------------------
                            sheet31.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet31.Range[xlsRow, iEmployeeCode].Text = dtManualDayStatusEntry.Rows[i]["EmployeeCode"].ToString();

                            sheet31.Range[xlsRow, iEmployeeName].Text = dtManualDayStatusEntry.Rows[i]["EmployeeName"].ToString();

                            sheet31.Range[xlsRow, iEmployeeCategory].Text = dtManualDayStatusEntry.Rows[i]["EmpCategory"].ToString();

                            sheet31.Range[xlsRow, iDepartment].Text = dtManualDayStatusEntry.Rows[i]["Department"].ToString();

                            sheet31.Range[xlsRow, iDesignation].Text = dtManualDayStatusEntry.Rows[i]["Designation"].ToString();

                            sheet31.Range[xlsRow, iSection].Text = dtManualDayStatusEntry.Rows[i]["Section"].ToString();

                            sheet31.Range[xlsRow, iSubSection].Text = dtManualDayStatusEntry.Rows[i]["SubSection"].ToString();

                            sheet31.Range[xlsRow, iEntity].Text = dtManualDayStatusEntry.Rows[i]["Entity"].ToString();

                            sheet31.Range[xlsRow, iDOJ].Text = dtManualDayStatusEntry.Rows[i]["DOJ"].ToString();

                            sheet31.Range[xlsRow, iManualDayStatus].Text = dtManualDayStatusEntry.Rows[i]["ManualDayStatus"].ToString();
                            
                            sheet31.Range[xlsRow, WorkDate].Text = dtManualDayStatusEntry.Rows[i]["WorkDate"].ToString();

                            sheet31.Range[xlsRow, iManualByWhom].Text = dtManualDayStatusEntry.Rows[i]["ManualByWhom"].ToString();

                            xlsRow++;
                            SLNo++;

                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet31.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet31.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet31.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup


                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet31.GetColumnWidth(1) + sheet31.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet31.GetRowHeight(1) + sheet31.GetRowHeight(2) + sheet31.GetRowHeight(3) + sheet31.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet31.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet31.Range[xlsRow, 3].Text = CmpName;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet31.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet31.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet31.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet31.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {

                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet31.Range[xlsRow, 3].Text = FactoryName;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet31.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet31.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet31.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet31.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet31.Range[xlsRow, 3].CellStyle.Font.Size = 22;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet31.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet31.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    sheet31.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-ManualDayStatus Entries";
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet31.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet31.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet31.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet31.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet31.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet31.IsDisplayZeros = false;
                    sheet31.UsedRange["A7"].FreezePanes();
                    sheet31.FirstVisibleColumn = 1;
                    sheet31.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet31.UsedRange.WrapText = true;
                    sheet31.UsedRange.CellStyle.Font.Size = 8;
                    sheet31.Range["A1"].CellStyle.Font.Size = 14;
                    sheet31.Range["A2"].CellStyle.Font.Size = 10;
                    sheet31.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet31.PageSetup.TopMargin = 0.5;
                    sheet31.PageSetup.BottomMargin = 0.7;
                    sheet31.PageSetup.PrintTitleRows = "$1:$5";
                    sheet31.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet31.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet31.PageSetup.LeftMargin = 0.5;
                    sheet31.PageSetup.RightMargin = 0.2;
                    sheet31.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet31.PageSetup.FitToPagesTall = 0;
                    sheet31.PageSetup.FitToPagesWide = 1;
                    sheet31.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet31.IsDisplayZeros = false;

                    if (dtManualOutEntry.Rows.Count > 0)
                    {
                        sheet31.Name = (SheetIndex + 1) + "_ManualDayStatus_Entry";
                        sheet31.TabColorRGB = Color.Red;

                    }
                    else
                    {
                        sheet31.Name = (SheetIndex + 1) + "_ManualDayStatus_Entry";
                    }
                    #endregion Page Setup


                }
                catch (Exception)
                {
                }
                #endregion 

                #region  ManualInOut Entries of Employees 27
                try
                {
                    IWorksheet sheet32 = null;

                    xlsRow = 1; xlsCol = 1;
                    endXlsCol = 1;
                    FactoryName = "";
                    CmpName = "";
                    SheetIndex++;
                    sheet32 = workbook.Worksheets[SheetIndex];
                    xlsRow = 6;

                    #region ------------------Column Header------------------
                    igoto = xlsCol;
                    sheet32.Range[5, igoto].Text = "Goto Index";
                    sheet32.Range[5, igoto].ColumnWidth = 6;
                    IHyperLink linkManualInOutPunch = sheet32.HyperLinks.Add(sheet32.Range[5, igoto]);
                    linkManualInOutPunch.Type = ExcelHyperLinkType.Workbook;
                    linkManualInOutPunch.TextToDisplay = sheet32.Range[5, igoto].Text;
                    linkManualInOutPunch.ScreenTip = "Go To " + sheet32.Range[5, igoto].Text;
                    linkManualInOutPunch.Address = "1_Index!A1";

                    isl = xlsCol;
                    sheet32.Range[xlsRow, isl].Text = "SL";
                    sheet32.Range[xlsRow, isl].ColumnWidth = 7;

                    xlsCol += 1;
                    iEmployeeCode = xlsCol;
                    sheet32.Range[xlsRow, iEmployeeCode].Text = "Employee Code";
                    sheet32.Range[xlsRow, iEmployeeCode].ColumnWidth = 14;

                    xlsCol += 1;
                    iEmployeeName = xlsCol;
                    sheet32.Range[xlsRow, iEmployeeName].Text = "Employee Name";
                    sheet32.Range[xlsRow, iEmployeeName].ColumnWidth = 20;

                    xlsCol += 1;
                    iDesignation = xlsCol;
                    sheet32.Range[xlsRow, iDesignation].Text = "Designation";
                    sheet32.Range[xlsRow, iDesignation].ColumnWidth = 18;

                    xlsCol += 1;
                    iDepartment = xlsCol;
                    sheet32.Range[xlsRow, iDepartment].Text = "Department";
                    sheet32.Range[xlsRow, iDepartment].ColumnWidth = 25;

                    xlsCol += 1;
                    iSection = xlsCol;
                    sheet32.Range[xlsRow, iSection].Text = "Section";
                    sheet32.Range[xlsRow, iSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iSubSection = xlsCol;
                    sheet32.Range[xlsRow, iSubSection].Text = "SubSection";
                    sheet32.Range[xlsRow, iSubSection].ColumnWidth = 18;

                    xlsCol += 1;
                    iEmployeeCategory = xlsCol;
                    sheet32.Range[xlsRow, iEmployeeCategory].Text = "Employee Category";
                    sheet32.Range[xlsRow, iEmployeeCategory].ColumnWidth = 18;

                    xlsCol += 1;
                    iEntity = xlsCol;
                    sheet32.Range[xlsRow, iEntity].Text = "Entity";
                    sheet32.Range[xlsRow, iEntity].ColumnWidth = 18;

                    xlsCol += 1;
                    iDOJ = xlsCol;
                    sheet32.Range[xlsRow, iDOJ].Text = "DOJ";
                    sheet32.Range[xlsRow, iDOJ].ColumnWidth = 18;

                    xlsCol += 1;
                    iRawInPunch = xlsCol;
                    sheet32.Range[xlsRow, iRawInPunch].Text = "Punch InTime";
                    sheet32.Range[xlsRow, iRawInPunch].ColumnWidth = 18;

                    xlsCol += 1;
                    iRawOutPunch = xlsCol;
                    sheet32.Range[xlsRow, iRawOutPunch].Text = "Punch OutTime";
                    sheet32.Range[xlsRow, iRawOutPunch].ColumnWidth = 18;

                    xlsCol += 1;
                    iManualByWhom = xlsCol;
                    sheet32.Range[xlsRow, iManualByWhom].Text = "By Whom";
                    sheet32.Range[xlsRow, iManualByWhom].ColumnWidth = 18;


                    sheet32.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
                    sheet32.Range[xlsRow, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet32.Range[xlsRow, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet32.Range[xlsRow, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;

                    xlsRow++;
                    endXlsCol = xlsCol;
                    #endregion ------------------Column Header------------------
                    SLNo = 1;
                    if (dtManualInOutEntry.Rows.Count > 0)
                    {
                        #region ----------------------Data-----------------------

                        for (int i = 0; i < dtManualInOutEntry.Rows.Count; i++)
                        {

                            sheet32.Range[xlsRow, isl].Text = SLNo.ToString();

                            sheet32.Range[xlsRow, iEmployeeCode].Text = dtManualInOutEntry.Rows[i]["EmployeeCode"].ToString();

                            sheet32.Range[xlsRow, iEmployeeName].Text = dtManualInOutEntry.Rows[i]["EmployeeName"].ToString();

                            sheet32.Range[xlsRow, iEmployeeCategory].Text = dtManualInOutEntry.Rows[i]["EmpCategory"].ToString();

                            sheet32.Range[xlsRow, iDepartment].Text = dtManualInOutEntry.Rows[i]["Department"].ToString();

                            sheet32.Range[xlsRow, iDesignation].Text = dtManualInOutEntry.Rows[i]["Designation"].ToString();

                            sheet32.Range[xlsRow, iSection].Text = dtManualInOutEntry.Rows[i]["Section"].ToString();

                            sheet32.Range[xlsRow, iSubSection].Text = dtManualInOutEntry.Rows[i]["SubSection"].ToString();

                            sheet32.Range[xlsRow, iEntity].Text = dtManualInOutEntry.Rows[i]["Entity"].ToString();

                            sheet32.Range[xlsRow, iDOJ].Text = dtManualInOutEntry.Rows[i]["DOJ"].ToString();

                            sheet32.Range[xlsRow, iRawInPunch].Text = dtManualInOutEntry.Rows[i]["PunchInTime"].ToString();
                            sheet32.Range[xlsRow, iRawOutPunch].Text = dtManualInOutEntry.Rows[i]["PunchOutTime"].ToString();

                            sheet32.Range[xlsRow, iManualByWhom].Text = dtManualInOutEntry.Rows[i]["ManualByWhom"].ToString();

                            xlsRow++;
                            SLNo++;

                        }

                        #endregion ----------------------Data-----------------------

                        #region Line Setup

                        sheet32.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                        sheet32.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                        sheet32.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;

                        #endregion Line Setup


                    }

                    #region ******************Report Header******************
                    try
                    {
                        string strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), companyId + ".jpg");  // IDCardEng.xlsx
                        Image companyLogo = Image.FromFile(strPath);
                        if (companyLogo != null)
                        {
                            double totalWidth = sheet32.GetColumnWidth(1) + sheet32.GetColumnWidth(2);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((sheet32.GetRowHeight(1) + sheet32.GetRowHeight(2) + sheet32.GetRowHeight(3) + sheet32.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = sheet32.Pictures.AddPicture(1, 1, companyLogo);

                        }


                    }
                    catch (Exception)
                    {


                    }

                    xlsRow = 1;
                    xlsCol = 1;

                    FactoryName = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet32.Range[xlsRow, 3].Text = CmpName;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet32.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet32.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet32.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet32.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {

                        FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet32.Range[xlsRow, 3].Text = FactoryName;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet32.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet32.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet32.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    if (dsFactory.Tables[0].Rows.Count > 0)
                    {
                        FactoryAddress = dsFactory.Tables[0].Rows[0]["Address1"].ToString();
                    }
                    else
                    {
                        FactoryAddress = "";
                    }
                    sheet32.Range[xlsRow, 3].Text = FactoryAddress;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet32.Range[xlsRow, 3].CellStyle.Font.Size = 22;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    sheet32.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet32.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    xlsRow += 1;
                    sheet32.Range[xlsRow, 3].Text = (SheetIndex + 1) + "-ManualInOut Entries";
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    sheet32.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    sheet32.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    sheet32.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet32.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet32.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = Color.Snow;

                    #endregion ******************Report Header******************

                    #region Freeze Panes

                    sheet32.IsDisplayZeros = false;
                    sheet32.UsedRange["A7"].FreezePanes();
                    sheet32.FirstVisibleColumn = 1;
                    sheet32.FirstVisibleRow = 6;

                    #endregion Freeze Panes

                    #region UsedRange Alignment

                    sheet32.UsedRange.WrapText = true;
                    sheet32.UsedRange.CellStyle.Font.Size = 8;
                    sheet32.Range["A1"].CellStyle.Font.Size = 14;
                    sheet32.Range["A2"].CellStyle.Font.Size = 10;
                    sheet32.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet32.PageSetup.TopMargin = 0.5;
                    sheet32.PageSetup.BottomMargin = 0.7;
                    sheet32.PageSetup.PrintTitleRows = "$1:$5";
                    sheet32.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet32.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + username + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet32.PageSetup.LeftMargin = 0.5;
                    sheet32.PageSetup.RightMargin = 0.2;
                    sheet32.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                    sheet32.PageSetup.FitToPagesTall = 0;
                    sheet32.PageSetup.FitToPagesWide = 1;
                    sheet32.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet32.IsDisplayZeros = false;

                    if (dtManualOutEntry.Rows.Count > 0)
                    {
                        sheet32.Name = (SheetIndex + 1) + "_ManualInOut_Entry";
                        sheet32.TabColorRGB = Color.Red;

                    }
                    else
                    {
                        sheet32.Name = (SheetIndex + 1) + "_ManualInOut_Entry";
                    }
                    #endregion Page Setup


                }
                catch (Exception ex)
                {
                    
                }
                #endregion

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string[] GetUnLockDateList(string plantId, string FromDate, string ToDate)
        {

            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string fds = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
            try
            {
                string sql = @"SELECT FORMAT([LockedDate],'dd-MMM-yyyy') [LockedDates]
                              FROM [PlantWiseAttendanceLock]
                                where PlantId='" + plantId + @"' and LockedDate between '" + fds + @"' and '" + ToDate + @"' AND IsActive=1 
                            	 order by LockedDate desc";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                string[] result = new string[dsMaster.Tables[0].Rows.Count];

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    result[i] = dsMaster.Tables[0].Rows[i]["LockedDates"].ToString();

                }
                DateTime dtTo = Convert.ToDateTime(ToDate);
                string fd = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
                DateTime dtFrom = Convert.ToDateTime(fd);

                int diffdate = Convert.ToInt32((dtTo - dtFrom).TotalDays.ToString());

                string[] newResult = new string[diffdate + 1];
                for (int i = 0; i < diffdate + 1; i++)
                {
                    string nDate = dtFrom.AddDays(i).ToString("dd-MMM-yyyy");
                    if (result.Contains(nDate))
                        continue;

                    newResult[i] = nDate;
                }
                return newResult;
            }
            catch (Exception ex)
            {

                throw (ex);
            }
        }



    }

    public class DataSetGenerationClass
    {
        private readonly ISqlRepository _sqlRepository;
        public DataSetGenerationClass()
        {
            
        }
        public DataSetGenerationClass(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
           
        }

        public void GetWorkDurationSheet(string FromDate, string ToDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT datediff(minute,KK.intime ,KK.outtime ) WorkDuration,
                            datediff(minute,KK.ShiftInTime ,CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END )	ShiftDuration,
                            E.Code EntityCode,E.UserName EntityUserName
                            ,kk.Id  EmployeeSystemId ,ei.EmployeeCurrentStatus,EI.CellPhnNo TelePhnNo
                            ,EI.EmployeeCode
                        	,EI.EmployeeName
                        	,PMB.Code BudgetCode
                            , LGD.userName LegalDesignation
                            , DSG.UserName Designation
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , E.UserName EntityName
                            , PR.UserName PositionName
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                            , EC.UserName as EmployeeCategory
                            , L.UserName Line  , FORMAT(KK.WorkDate,'ddd') AS DayName, 
                            FORMAT(KK.WorkDate,'dd-MMM-yyyy') AS WorkDate                           
                            ,kk.ShiftName 
                            ,FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                            ,FORMAT(ShiftInTime,'hh:mm tt') AS ShiftInTime,
                     	    FORMAT(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'hh:mm tt') ShiftOutTime,
                            FORMAT(KK.InTime,'dd-MMM-yyyy hh:mm tt') AS  InTime  
							,FORMAT(KK.OutTime,'dd-MMM-yyyy hh:mm tt') AS  OutTime                           
                            , FORMAT(KK.PunchInTime,'dd-MMM-yyyy hh:mm tt') AS PunchInTime
                            , FORMAT(KK.PunchOutTime,'dd-MMM-yyyy hh:mm tt') AS PunchOutTime
                            , KK.DayStatus
                            ,IsManualInTime=CASE WHEN kk.IsManualInTime=1 then 'Yes' else 'No'end
							,IsManualOutTime=CASE WHEN kk.IsManualOutTime=1 then 'Yes' else 'No'end
							,IsManualDayStatus=CASE WHEN kk.IsManualDayStatus=1 then 'Yes' else 'No'end
                            ,KK.ShiftDuration,KK.ShiftFullDayDuration
                            ,KK.Duration
							 AS WorkDuration
                            ,KK.OverStay AS WorkTimeDifferent,
                            convert(int,KK.ShiftDuration/60) as ShiftDurationHour    
                            ,Convert(int,KK.OverStay/60) AS WorkTimeDifferentHour
                           ,Convert(int,KK.Duration/60) AS WorkDurationHour
                            , KK.OTHr OverStay
                            , KK.TotalOTHr ConfirmedOT
                            ,IsOTEntitled= CASE WHEN KK.IsOTEntitled=1 THEN 'Yes' else 'No'END
                             FROM (								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime,
                                    emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,
		                            O.PunchInTime,O.PunchOutTime,O.OVERSTAY,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,
		                            O.IsOTEntitled,O.ShiftFullDayDuration,o.Duration,O.ShiftDuration
									,fo.TotalOTHr ,o.IsManualDayStatus ,emp.BudgetCode,emp.GivenDesignationId
		                            FROM EmployeeInformation EMP
		                            inner join AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
		                            LEFT JOIN FinalOT AS fo  ON EMP.SystemID=fo.EmpSystemID AND fo.WorkDate=o.WorkDate
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID                       
                            WHERE o.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' and o.IsHalfDayLeave <> 1
                        ) AS KK
                        LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=kk.ShiftSystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON kk.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
						LEFT OUTER JOIN EmployeeInformation EI ON KK.Id=EI.SystemID                            
                        LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                        LEFT JOIN HKP.Designation DSG ON PR.DesignationId = DSG.Id
                        LEFT JOIN HKP.Designation DeG ON DeG.Id = EI.GivenDesignationId
                        LEFT JOIN ORG.Department DP ON DP.Id = EI.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                        LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
                        left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                        LEFT JOIN ORG.Section AS Se ON Se.Id = EI.SectionID
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = EI.SubSectionID
                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId							
						where kk.Duration < KK.ShiftFullDayDuration
						      and
							  EI.PlantId='" + plantId + @"' and EI.CompanyId='" + companyId + @"' and EI.GroupID='" + companyGroupId + @"'
                        ORDER BY CONVERT(DATE, WorkDate),kk.EmployeeCode ASC";

               con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 

        private string tableName()
        {
            return @"
                        LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id                        
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                        LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
						LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                        left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                        LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                        ";
        }
        
        private string columnName()
        {
            return @",ei.EmployeeCurrentStatus,EI.CellPhnNo TelePhnNo
                            ,EI.EmployeeCode
                        	,EI.EmployeeName
                        	,PMB.Code BudgetCode
                            , LGD.userName LegalDesignation
                            , DeG.UserName Designation
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , E.UserName EntityName
                            , PR.UserName PositionName
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                            , EC.UserName as EmployeeCategory
                            , L.UserName Line ";

        }
        
        public void GetOTEntitledWithOutMissingReports(string FromDate, string ToDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
                            ";
                strSql += columnName();
                strSql += @"
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                            ,AP.DayStatus
                            ,AP.IsManualDayStatus, AP.IsManualInTime, AP.IsManualOutTime
                            ,AP.OTHr OverStay
                            ,OTF.TotalOTHr
							,(isnull(AP.OTHr,0) - isnull(OTF.TotalOTHr,0)) OTDifference
                        	,AP.InTime InTime
                        	,AP.OutTime OutTime
                             ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent 
                        	,AP.IsOTComfirm
                        	,OTF.NormalOTHr ComfirmedOT
                        	,AP.OTHr CalOT
                        	,ISNULL(AP.PunchInTime, '') PunchInTime
                        	,AP.PunchOutTime PunchOutTime
                            ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent
                            ,EC.UserName as EmployeeCategory
                        
                        FROM AttdnProcessData AP
                        LEFT join (select LogDownLoadNum,PDate,min(PTime)ptime from AttdnRawData where isnull(ptype,'')='' group by LogDownLoadNum,PDate) rd on rd.LogDownLoadNum = ap.EmpSystemID and rd.PDate = ap.WorkDate
                        LEFT JOIN FinalOT OTF ON AP.EmpSystemID = OTF.EmpSystemID
                        	AND AP.WorkDate = OTF.WorkDate
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus
                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                       ";
                strSql += tableName();
                strSql += @"WHERE AP.DAYSTATUS='A' AND
                           AP.InTime IS NOT NULL                        	
                        	And AP.OutTime IS NULL  
                            AND  AP.IsOTEntitled = 1
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'	
                             
                        ORDER BY 
                                EmployeeCodePreFix,EmployeeCodeNumeric
                                    ,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetOTNotEntitledWithOutMissingReports(string FromDate, string ToDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId";
                strSql += columnName();
                strSql += @",SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                            ,AP.DayStatus
                            ,AP.IsManualDayStatus, AP.IsManualInTime, AP.IsManualOutTime
                            ,AP.OTHr OverStay
                            ,OTF.TotalOTHr
							,(isnull(AP.OTHr,0) - isnull(OTF.TotalOTHr,0)) OTDifference
                        	,AP.InTime InTime
                        	,AP.OutTime OutTime
                             ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent 
                        	,AP.IsOTComfirm
                        	,OTF.NormalOTHr ComfirmedOT
                        	,AP.OTHr CalOT
                        	,ISNULL(AP.PunchInTime, '') PunchInTime
                        	,AP.PunchOutTime PunchOutTime
                            ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent
                            ,EC.UserName as EmployeeCategory
                  
                        FROM AttdnProcessData AP
                        LEFT join (select LogDownLoadNum,PDate,min(PTime)ptime from AttdnRawData where isnull(ptype,'')='' group by LogDownLoadNum,PDate) rd on rd.LogDownLoadNum = ap.EmpSystemID and rd.PDate = ap.WorkDate
                        LEFT JOIN FinalOT OTF ON AP.EmpSystemID = OTF.EmpSystemID
                        	AND AP.WorkDate = OTF.WorkDate
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus
                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                       ";
                strSql += tableName();
                strSql += @"WHERE AP.DAYSTATUS='A' AND
                        	AP.InTime IS NOT NULL                        	
                        	And AP.OutTime IS NULL  
                            AND  AP.IsOTEntitled = 0
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'	                           
                        ORDER BY 
                               EmployeeCodePreFix,EmployeeCodeNumeric
                                    ,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetOtNotConfirmOverstayReport(string FromDate, string ToDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            ConnectionManager.clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
                           ";
                strSql += columnName();
                strSql += @"
                            ,AP.DayStatus
                            , AP.IsManualDayStatus, AP.IsManualInTime, AP.IsManualOutTime
                            ,AP.OverStay OverStay
                            ,OTF.TotalOTHr
							,(isnull(AP.ProcessedOT,0) - isnull(OTF.TotalOTHr,0)) OTDifference
                        	,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                        	,AP.InTime
                        	,AP.OutTime
                             ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent 
                        	,AP.IsOTComfirm
                        	,OTF.NormalOTHr ComfirmedOT
                        	,AP.ProcessedOT CalOT
                        	,ISNULL(AP.PunchInTime, '') PunchInTime
                        	,AP.PunchOutTime PunchOutTime
                            ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent
                            
                        FROM AttdnProcessData AP
                        LEFT JOIN FinalOT OTF ON AP.EmpSystemID = OTF.EmpSystemID
                        	AND AP.WorkDate = OTF.WorkDate
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus
                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                        
                      ";
                strSql += tableName();
                strSql += @"
                        WHERE 
                            AP.DayStatus in (select daytype from daytype where category='Present' OR  category='Late')
						    and ap.ManualOT is null 
                        	AND isnull(AP.IsOTEntitled,'0') = 1  
                        	AND isnull(AP.IsOTComfirm,'0') = 0 
                            and ap.ProcessedOT >=0 and ap.OverStay<>0
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'	
                              and ei.DOJ<='" + ToDate + @"' AND (ei.DOS is null OR ei.DOS>= '" + FromDate + @"')
                        ORDER BY AP.WorkDate
                        	,EmployeeCodePreFix,EmployeeCodeNumeric";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetOffdayWithPunchReports(string FromDate, string ToDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId";
                strSql += columnName();
                strSql += @" ,AP.DayStatus
                            ,AP.OTHr OverStay
                            ,OTF.TotalOTHr,AP.ProcessedOT
							,(isnull(AP.OTHr,0) - isnull(OTF.TotalOTHr,0)) OTDifference
                            ,AP.IsManualDayStatus, AP.IsManualInTime, AP.IsManualOutTime
                        	,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                        	, InTime = case when AP.InTime is null then ap.PunchInTime else ap.InTime end 
                        	,OutTime = case when AP.OutTime is null then ap.PunchOutTime else ap.OutTime end
                             ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent 
                        	,AP.IsOTComfirm
                        	,OTF.NormalOTHr ComfirmedOT
                        	,AP.OTHr CalOT
                        	,ISNULL(AP.PunchInTime, '') PunchInTime
                        	,AP.PunchOutTime PunchOutTime
                            ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent
							
                        FROM AttdnProcessData AP
                        
                        LEFT JOIN FinalOT OTF ON AP.EmpSystemID = OTF.EmpSystemID
                        	AND AP.WorkDate = OTF.WorkDate
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus

                    

                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                        ";
                strSql += tableName();
                strSql += @"WHERE  
                                dt.OriginalDayType in ('W','H')  
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'   
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                              
							and (---1
							
							( AP.InTime IS Not NULL or AP.PunchInTime Is Not Null )
                            and 
                            ( AP.OutTime IS Not NULL or AP.PunchOutTime Is Not Null )
							
							)----1                     		
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetOffdayMissingPunchReports(string FromDate, string ToDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId";
                strSql += columnName();
                strSql += @" ,AP.DayStatus
                            ,AP.OTHr OverStay
                            ,OTF.TotalOTHr
							,(isnull(AP.OTHr,0) - isnull(OTF.TotalOTHr,0)) OTDifference
                            ,AP.IsManualDayStatus, AP.IsManualInTime, AP.IsManualOutTime
                        	,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                        	, InTime = case when AP.InTime is null then ap.PunchInTime else ap.InTime end 
                        	,OutTime = case when AP.OutTime is null then ap.PunchOutTime else ap.OutTime end
                             ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent 
                        	,AP.IsOTComfirm
                        	,OTF.NormalOTHr ComfirmedOT
                        	,AP.OTHr CalOT
                        	,ISNULL(AP.PunchInTime, '') PunchInTime
                        	,AP.PunchOutTime PunchOutTime
                            ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent
							
                        FROM AttdnProcessData AP
                        LEFT JOIN FinalOT OTF ON AP.EmpSystemID = OTF.EmpSystemID
                        	AND AP.WorkDate = OTF.WorkDate
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus

                    

                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                        ";
                strSql += tableName();
                strSql += @"WHERE  
                                dt.OriginalDayType in ('W','H')  
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'   
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                              
							and (---1
							( (AP.InTime IS NULL and AP.PunchInTime Is Null)	AND (AP.OutTime IS not NULL or AP.PunchOutTime Is NOT NULL))
							or 
							( (AP.InTime IS Not NULL or AP.PunchInTime Is Not Null)	AND (AP.OutTime IS NULL and AP.PunchOutTime Is NULL))
							--or
							--( AP.InTime IS not NULL	AND AP.OutTime IS not NULL)
							)----1                        		
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
         
        public void GetAbsentReports(string FromDate, string ToDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            string fd = "01-" + Convert.ToDateTime(ToDate).ToString("MMM") + "-" + Convert.ToDateTime(ToDate).ToString("yyyy");
            string endDate = Convert.ToDateTime(fd).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId

                           ";
                strSql += columnName();
                strSql += @" ,AP.OTHr OverStay
                            ,OTF.TotalOTHr
							,(isnull(AP.OTHr,0) - isnull(OTF.TotalOTHr,0)) OTDifference
                        	,PR.UserName PositionName
                            ,AP.DayStatus
                            ,AP.IsManualDayStatus, AP.IsManualInTime, AP.IsManualOutTime
                        	,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	,l.username as Line
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                        	,AP.InTime InTime
                        	,AP.OutTime OutTime
                             ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent 
                        	,AP.IsOTComfirm
                        	,OTF.NormalOTHr ComfirmedOT
                        	,AP.OTHr CalOT
                        	,ISNULL(AP.PunchInTime, '') PunchInTime
                        	,AP.PunchOutTime PunchOutTime
                            ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent
                            ,totalabsent.TotalAbsent
                        FROM AttdnProcessData AP
                        LEFT JOIN FinalOT OTF ON AP.EmpSystemID = OTF.EmpSystemID
                        	AND AP.WorkDate = OTF.WorkDate
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus

                            left join ( SELECT EmpSystemID ,EmployeeCode
								,sum(TotalAbsent)as TotalAbsent 
                               FROM(
								SELECT EmpSystemID, EmployeeCode,DayStatus,   								                        
                                TotalAbsent = CASE WHEN Category = 'Absent' and LTSystemID is null THEN 1
                                WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration<1 THEN (1-LeaveDuration)
                                WHEN Category = 'Absent' and LTSystemID is not null and LeaveDuration=1 THEN 1
                                WHEN Category = 'Half Day' and LTSystemID is null THEN 0.5
                                ELSE 0 END      							                        
                                FROM dbo.AttdnProcessData a
                                left join daytype p on a.DayStatus=p.DayType
                                left join employeeInformation ei on ei.SystemId =a.EmpSystemID 							
                                WHERE  
								 WorkDate between '" + fd + @"' AND '" + ToDate + @"'
								 AND ei.PlantId= '" + plantId + @"' AND EI.CompanyId='" + companyId + @"'
                                ) A  
							 group by EmployeeCode,EmpSystemID) totalabsent on totalabsent.EmpSystemID=ap.EmpSystemID and EI.SystemId=totalabsent.EmpSystemID

                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                       ";
                strSql += tableName();
                strSql += @"WHERE 
                               AP.DayStatus ='A'
                        	And AP.InTime IS NULL
                        	AND AP.OutTime IS NULL
                            and isnull(ei.EmployeeCurrentStatus,'') not in('TBS','LONG ABSENTEEISM')
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                            
                        ORDER BY AP.WorkDate
                        	,EmployeeCodePreFix,EmployeeCodeNumeric";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function
         
        public void GetLeaveWithPunchReports(string FromDate, string ToDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                string fd = "01-" + Convert.ToDateTime(FromDate).ToString("MMM") + "-" + Convert.ToDateTime(FromDate).ToString("yyyy");
                string endDate = Convert.ToDateTime(fd).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
                        	";
                strSql += columnName();
                strSql += @"
                        	,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                        	,AP.InTime InTime
                        	,AP.OutTime OutTime
                             ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent 
                        	,AP.DayStatus,lt.LeaveType,lt.Code
                            ,AP.IsManualDayStatus, AP.IsManualInTime, AP.IsManualOutTime
                            ,AP.OTHr OverStay
                            ,OTF.TotalOTHr
							,(isnull(AP.OTHr,0) - isnull(OTF.TotalOTHr,0)) OTDifference
                        	,AP.IsOTComfirm
                        	,OTF.NormalOTHr ComfirmedOT
                        	,AP.OTHr CalOT
                        	,ISNULL(AP.PunchInTime, '') PunchInTime
                        	,AP.PunchOutTime PunchOutTime
                            ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent
                        FROM AttdnProcessData AP
                        left join [dbo].[LeaveType] lt on lt.Id=AP.LTSystemID
                        LEFT JOIN FinalOT OTF ON AP.EmpSystemID = OTF.EmpSystemID
                        	AND AP.WorkDate = OTF.WorkDate
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus
                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                       ";
                strSql += tableName();
                strSql += @"WHERE 
                              AP.DayStatus in (select daytype from daytype where category='Leave') 
                                and AP.IsHalfDayLeave = 0
                        	AND AP.WorkDate between '" + fd + @"' and  '" + endDate + @"'
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                                
                             and (---1
							( AP.InTime IS NULL	AND AP.OutTime IS not NULL)
							or 
							( AP.InTime IS not NULL	AND AP.OutTime IS NULL)
							or
							( AP.InTime IS not NULL	AND AP.OutTime IS not NULL)
							)----1
                        		
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                            ,AP.WorkDate";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }

        public void GetInMissingReports(string FromDate, string ToDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId";
                strSql += columnName();
                strSql += @" ,AP.DayStatus
                            ,AP.OTHr OverStay
                            ,OTF.TotalOTHr
							,(isnull(AP.OTHr,0) - isnull(OTF.TotalOTHr,0)) OTDifference
                            ,AP.IsManualDayStatus, AP.IsManualInTime, AP.IsManualOutTime
                        	,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                        	,AP.InTime InTime
                        	,AP.OutTime OutTime
                             ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent 
                        	,AP.IsOTComfirm
                        	,OTF.NormalOTHr ComfirmedOT
                        	,AP.OTHr CalOT
                        	,ISNULL(AP.PunchInTime, '') PunchInTime
                        	,AP.PunchOutTime PunchOutTime
                            ,DateDiff(minute, AP.PunchOutTime,AP.OutTime) OutTimeDifferent
		                
                        FROM AttdnProcessData AP
                        LEFT JOIN FinalOT OTF ON AP.EmpSystemID = OTF.EmpSystemID
                        	AND AP.WorkDate = OTF.WorkDate
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus

                    

                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                    LEFT join (select LogDownLoadNum,PDate,min(PTime)ptime from AttdnRawData where isnull(ptype,'')='' group by LogDownLoadNum,PDate) rd on rd.LogDownLoadNum = ap.EmpSystemID and rd.PDate = ap.WorkDate
                        ";
                strSql += tableName();
                strSql += @"WHERE  
                            AP.DAYSTATUS='A' AND
                        	AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'   
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                               --and ISNULL(rd.LogDownLoadNum,'')=''
							and (---1
							( AP.InTime IS NULL	AND AP.OutTime IS not NULL)							
							)----1                        		
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetSeparatedEmployeesPunches(string FromDate, string ToDate, string plantId, string companyId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"select E.SystemId,e.EmployeeStatus
                            ,E.EmployeeCode
                        	,E.EmployeeName
                        	,PMB.Code BudgetCode
                            , LGD.userName LegalDesignation
                            , DeG.UserName Designation
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , Ent.UserName Entity
                            , PR.UserName PositionName
                            , FORMAT(E.DOJ, 'dd-MMM-yyyy') DOJ
                            , EC.UserName as EmpCategory
                            , L.UserName Line,FORMAT(a.PTime,'dd-MMM-yyyy hh:mm:ss tt') as PunchTime
                            ,FORMAT(E.DOS,'dd-MMM-yyyy')DOS
						from employeeinformation e
						left join AttdnRawData a on a.LogDownLoadNum=e.systemId  
						LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity Ent ON PMB.EntityId = Ent.Id                        
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = E.LegalDesignationId
                        LEFT join  [MST].[DesignationMasterLegalDesignation] dmld 
						on dmld.LegalDesignationId=LGD.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
						LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                        left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                        LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId                      
                        where e.employeestatus!='Active' and a.PDate between '" + FromDate+@"' and  '"+ToDate+@"' 
                        and e.PlantId='"+plantId+ "' and a.PTime is not null and e.CompanyId='"+companyId+"' ";              

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetManualInEntry(string FromDate, string ToDate, string plantId, string companyId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT  FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId,EI.CellPhnNo TelePhnNo
                            ,EI.EmployeeCode
                        	,EI.EmployeeName
                        	,PMB.Code BudgetCode
                            , LGD.userName LegalDesignation
                            , DeG.UserName Designation
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , E.UserName Entity
                            , PR.UserName PositionName
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                            , EC.UserName as EmpCategory
                            , L.UserName Line  ,AP.DayStatus,
                             AP.IsManualInTime,FORMAT(ap.ManualInTime,'dd-MMM-yyyy hh:mm:ss tt') 
							 as PunchTime							
							,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END,ap.ManualByWhom
                        FROM AttdnProcessData AP
                        
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId

                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                        
                        LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id                        
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                        LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
						LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                        left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                        LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                        WHERE (ap.ManualInTime is not null and ap.IsManualInTime=1) and 
                              AP.WorkDate between '" + FromDate+@"' and  '"+ToDate+@"'   
                           and ei.PlantId='"+plantId+@"' and ei.CompanyId='"+companyId+@"'	
                       ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetManualOutEntry(string FromDate, string ToDate, string plantId, string companyId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT  FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId,EI.CellPhnNo TelePhnNo
                            ,EI.EmployeeCode
                        	,EI.EmployeeName,ap.ManualByWhom
                        	,PMB.Code BudgetCode
                            , LGD.userName LegalDesignation
                            , DeG.UserName Designation
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , E.UserName Entity
                            , PR.UserName PositionName
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                            , EC.UserName as EmpCategory
                            , L.UserName Line  ,AP.DayStatus,
                             AP.IsManualOutTime,FORMAT(ap.ManualOutTime,'dd-MMM-yyyy hh:mm:ss tt') 
							 as PunchTime							
							,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                        FROM AttdnProcessData AP
                        
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId

                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                        
                        LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id                        
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                        LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
						LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                        left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                        LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                        WHERE (ap.OriginalManualOutTime is not null and ap.IsManualOutTime=1) and 
                              AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'   
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"'	
                       ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetManualInOutEntry(string FromDate, string ToDate, string plantId, string companyId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT  FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId,EI.CellPhnNo TelePhnNo
                            ,EI.EmployeeCode
                        	,EI.EmployeeName,ap.ManualByWhom
                        	,PMB.Code BudgetCode
                            , LGD.userName LegalDesignation
                            , DeG.UserName Designation
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , E.UserName Entity
                            , PR.UserName PositionName
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                            , EC.UserName as EmpCategory
                            , L.UserName Line  ,AP.DayStatus
							 ,AP.IsManualInTime,FORMAT(ap.ManualInTime,'dd-MMM-yyyy hh:mm:ss tt') AS PunchInTime
                             ,AP.IsManualOutTime,FORMAT(ap.ManualOutTime,'dd-MMM-yyyy hh:mm:ss tt') as PunchOutTime							 
							,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                        FROM AttdnProcessData AP
                        
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId

                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                        
                        LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id                        
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                        LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
						LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                        left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                        LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                        WHERE (ap.ManualInTime is not null and ap.IsManualInTime=1) AND (ap.OriginalManualOutTime is not null and ap.IsManualOutTime=1)							  
                            AND  AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'   
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"'	
                       ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetManualDayStatusEntry(string FromDate, string ToDate, string plantId, string companyId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT  FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId,EI.CellPhnNo TelePhnNo
                            ,EI.EmployeeCode,ap.ManualByWhom
                        	,EI.EmployeeName
                        	,PMB.Code BudgetCode
                            , LGD.userName LegalDesignation
                            , DeG.UserName Designation
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , E.UserName Entity
                            , PR.UserName PositionName
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                            , EC.UserName as EmpCategory
                            , L.UserName Line  ,AP.DayStatus,
                             AP.IsManualDayStatus,ap.ManualDayStatus				
							,FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ
                        	,SD.ShiftDefinationName ShiftName
                        	,sd.ShiftType
                        	,ShiftOutTime = CASE 
                        		WHEN cs.OutTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.OutTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                        		END
                        	,ShiftInTime = CASE 
                        		WHEN cs.InTime IS NULL
                        			THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
                        		ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
                        		END
                        FROM AttdnProcessData AP
                        
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId

                        LEFT JOIN (
                        	SELECT m.ShiftDefinationID
                        		,c.ShiftDate
                        		,m.InTime
                        		,m.SystemID
                        		,m.OutTime
                        	FROM [ShiftTimeChgMaster] m
                        	LEFT JOIN [ShiftTimeChgChild] c ON m.SystemID = c.STCMasterSystemID
                        	) CS ON cs.ShiftDefinationID = AP.ShiftSystemID
                        	AND cs.ShiftDate = AP.WorkDate
                        LEFT JOIN [ShiftDefination] sd ON sd.SystemID = AP.ShiftSystemID
                        
                        LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode = PMB.Id
                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                        LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id                        
                        LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EI.LegalDesignationId
                        LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
                        left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
						LEFT JOIN HKP.Designation DeG ON DeG.Id = dm.DesignationId
                        left join HKP.EmployeeCategory EC ON EC.ID=DM.EmployeeCategoryId
                        LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                        LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                        LEFT JOIN ORG.Line AS L ON L.Id= PMB.LineId
                        WHERE (ap.ManualDayStatus is not null and ap.IsManualDayStatus=1) and 
                              AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'   
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"'	
                       ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetLongAbsentism(string FromDate, string ToDate, string plantId, string companyId, string companyGroupId, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT ei.PlantId
                        	";
                strSql += columnName();
                strSql += @"
                            , FORMAT(AP.WorkDate,'dd-MMM-yyy')  WorkDate                         
							,FORMAT(EI.EmployeeCurrentStatusEffectiveDate,'dd-MMM-yyyy') EmployeeCurrentStatusEffectiveDate
                        	--,DATEDIFF(DAY,EI.EmployeeCurrentStatusEffectiveDate,GETDATE()) NumberOfAbsentDays
                            ,L.UserName AS Line,
                            DATEDIFF(DAY,EI.EmployeeCurrentStatusEffectiveDate,GETDATE())-
							(select count(WorkDate) 
							from attdnprocessdata
							LEFT join DayType d on d.DayType=DayStatus
							where PlantId='"+plantId+ @"' and
							workdate between
							EI.EmployeeCurrentStatusEffectiveDate and getdate()
							and DayStatus IN 
										(select distinct DayType from DayType 
										where Category in ('Holiday','Weekend'))
										and EmpSystemID=ei.SystemId )NumberOfAbsentDays										


                        FROM EmployeeInformation EI    
LEFT JOIN AttdnProcessData  AP ON AP.EmpSystemID = EI.SystemId 
						AND AP.RowId= (select top(1)RowId from AttdnProcessData where WorkDate
                        between '" + FromDate + @"' and '" + ToDate + @"' AND EmpSystemID = AP.EmpSystemID Order By WorkDate DESC)";

                strSql += tableName();
                strSql += @"
                        
                        WHERE 
                         ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'
                                 and ei.DOJ<='" + ToDate + @"' AND (ei.DOS is null OR ei.DOS>= '" + FromDate + @"')
                            AND isnull(EI.EmployeeCurrentStatus,'')='LONG ABSENTEEISM' 
                        
                        ORDER BY
                        	EmployeeCodePreFix,EmployeeCodeNumeric,ap.WorkDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

    }

    public class NewAuditReportSummaryService
    {
        SqlRepository _sqlRepository;
        clsConnectionManager ConManager;

        public NewAuditReportSummaryService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new clsConnectionManager();
        }
        public void GetInMissingReports(string FromDate, string plantId, string companyId, string companyGroupId, string ToDate, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId

                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        WHERE  
                                AP.DayStatus ='A'  
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"' 
                           and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'                               
							and (---1
							( AP.InTime IS NULL	AND AP.OutTime IS not NULL)
													
							)----1                        		
                        ORDER BY 
                        	EmployeeCodePreFix,EmployeeCodeNumeric
                               ,AP.WorkDate";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetWorkDurationSheet(string FromDate, string plantId, string companyId, string companyGroupId, string ToDate, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT datediff(minute,KK.intime ,KK.outtime ) WorkDuration,
                            datediff(minute,KK.ShiftInTime ,CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END )	ShiftDuration
                             FROM (								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, 
                                    emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,
		                            O.IsOTEntitled,O.Duration,O.ShiftFullDayDuration
									,fo.TotalOTHr ,o.IsManualDayStatus ,emp.BudgetCode,emp.GivenDesignationId
		                            FROM EmployeeInformation EMP
		                            inner join AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
		                            LEFT JOIN FinalOT AS fo  ON EMP.SystemID=fo.EmpSystemID AND fo.WorkDate=o.WorkDate
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID                       
                            WHERE o.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' and o.IsHalfDayLeave <> 1
                        ) AS KK
						LEFT OUTER JOIN EmployeeInformation EI ON KK.Id=EI.SystemID  
						where kk.Duration < KK.ShiftFullDayDuration	
                              and
							  EI.PlantId='" + plantId + @"' and EI.CompanyId='" + companyId + @"' and EI.GroupID='" + companyGroupId + @"'
                             and DayStatus in (select DayType from DayType where Category in ('Present', 'Late'))
                        ORDER BY CONVERT(DATE, WorkDate),kk.EmployeeCode ASC";

                con.getDataSet(strSql, out dsRef);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function 

        public void GetOtNotConfirmOverstayReport(string FromDate, string plantId, string companyId, string companyGroupId, string ToDate, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId

                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId                        
                        WHERE 
                                AP.DayStatus in (select daytype from daytype where category='Present' OR  category='Late')
                        	AND AP.IsOTEntitled = 1
                        	AND AP.IsOTComfirm = 0 and AP.ManualOT is null 
                            and ap.ProcessedOT >=0 and ap.OverStay<>0
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'	
                              and ei.DOJ<='" + FromDate + @"' AND (ei.DOS is null OR ei.DOS>= '" + ToDate + @"')
                        ORDER BY AP.WorkDate
                        	,EmployeeCodePreFix,EmployeeCodeNumeric";

                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetOTEntitledWithOutMissingReports(string FromDate, string plantId, string companyId, string companyGroupId, string ToDate, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
                        FROM AttdnProcessData AP                        
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus WHERE AP.DayStatus='A'
                            and AP.InTime IS NOT NULL                        	
                        	And AP.OutTime IS NULL  
                            AND  AP.IsOTEntitled = 1
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'	
                             
                        ORDER BY 
                                EmployeeCodePreFix,EmployeeCodeNumeric
                                    ,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

        public void GetOTNotEntitledWithOutMissingReports(string FromDate, string plantId, string companyId, string companyGroupId, string ToDate, out DataSet dsRef)
        {
            clsConnectionManager con = new clsConnectionManager(120);
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT FORMAT(AP.WorkDate, 'dd-MMM-yyyy') WorkDate
                        	,EI.SystemId
                        FROM AttdnProcessData AP
                        LEFT JOIN EmployeeInformation EI ON AP.EmpSystemID = EI.SystemId
                        Left join DayType DT ON DT.DayType = AP.DayStatus WHERE AP.DayStatus='A'
                         	And AP.InTime IS NOT NULL                        	
                        	And AP.OutTime IS NULL  
                            AND  AP.IsOTEntitled = 0
                        	AND AP.WorkDate between '" + FromDate + @"' and  '" + ToDate + @"'
                        	and ei.PlantId='" + plantId + @"' and ei.CompanyId='" + companyId + @"' and ei.GroupID='" + companyGroupId + @"'	                           
                        ORDER BY 
                               EmployeeCodePreFix,EmployeeCodeNumeric
                                    ,AP.WorkDate";
                con.getDataSet(strSql, out dsRef);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                con = null;
            }
        }//End Function

    }

}

