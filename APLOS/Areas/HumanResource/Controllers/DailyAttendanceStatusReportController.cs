using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll;
using Library.HumanResource.Payroll.Report;
using Library.HumanResource.Report.OT;
using Library.Model.HumanResources;
using Library.Security.Core;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Microsoft.Reporting.WebForms;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
namespace Aplos.Areas.HumanResource.Controllers
{
    public class DailyAttendanceStatusReportController : Controller
    {
        private readonly SqlRepository _sqlRepository;
        public DailyAttendanceStatusReportController()
        {
            _sqlRepository = new SqlRepository();
        }
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
        public JsonResult GetShift()
        {
            try
            {
                var sql = @"select SystemID Value, UserName Text from ShiftDefination";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [AllowAnonymous]
        public JsonResult GetEmployeeCategory()
        {
            try
            {
                var sql = @"select Id Value, UserName Text from HKP.EmployeeCategory";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public JsonResult GetTeamLeader()            
        {
            try
            {
                var sql = @"select EMP.SystemId EmpSystemId, EMP.EmployeeCode, EMP.EmployeeName, FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, EC.UserName EmployeeCategory, DP.UserName Department
                                ,SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation, LDSG.UserName LegalDesignation, UN.UserName as Entity 
                                from TRN.TeamDefinition TD
                                left join EmployeeInformation EMP on EMP.SystemId = TD.TeamLeaderId
                                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                                where EMP.EmployeeStatus = 'Active'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public JsonResult GetFavoriteListByUser()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select DAF.Id, DAF.FavoriteName, FORMAT(DAF.FromDate, 'dd-MMM-yyyy') FromDate, FORMAT(DAF.ToDate, 'dd-MMM-yyyy') ToDate, DAF.InStatus, DAF.EmployeeStatus ,U.FullName [User], DAF.FavoriteFilteruserId UserId, DAF.ShiftDefinationId
, DAF.EmployeecategoryId,  DAF.ResponsiblePersonId
from[TRN].[DailyAttendanceFavoriteFilter] DAF
left join [SEC].[User] U on U.Id = DAF.FavoriteFilteruserId
left join ShiftDefination SD ON SD.SystemID = DAF.ShiftDefinationId
left join HKP.EmployeeCategory EC on EC.Id = DAF.EmployeecategoryId
--left join EmployeeInformation EI on EI.SystemId = DAF.TeamLeaderId
left join EmployeeInformation EI2 on EI2.SystemId = DAF.ResponsiblePersonId
where DAF.FavoriteFilteruserId = '" + identity .UserId+ "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [AllowAnonymous]
        public JsonResult GetFavoriteFilterByUser()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select DAF.Id Value, DAF.FavoriteName Text from [TRN].[DailyAttendanceFavoriteFilter] DAF";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult GetFavouriteFilter(string filterId)
        {
            var sql = @"select DAF.Id, FORMAT(DAF.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(DAF.FromDate, 'dd-MMM-yyyy')FromDate, DAF.InStatus,  DAF.EmployeecategoryId, DAF.ShiftDefinationId, DAF.ResponsiblePersonId, DAF.FavoriteFilteruserId, DAF.FavoriteName, EI.EmployeeName, DAF.DayStatus, DAF.EmployeeStatus
from [TRN].[DailyAttendanceFavoriteFilter] DAF  
left join EmployeeInformation EI on EI.SystemId = DAF.ResponsiblePersonId where DAF.Id = '" + filterId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize , AllowAnonymous]
        public JsonResult RemoveFavoriteFilter(string id)
        {
            var sql = @"delete from [TRN].[DailyAttendanceFavoriteFilter] where Id = '"+id+"'";

            return Json(new { Data = _sqlRepository.GetDataCollection(sql), Message = AplosMessage.Deleted });
            //Json(_sqlRepository.GetDataCollection(sql), AplosMessage.Deleted, JsonRequestBehavior.AllowGet);
            
        }

        [Authorize]
        public JsonResult GetDailyAttendanceStatus(string instatus, string fromdate, string todate, string employeecategory,  string responsibleperson, string shift, string employeestatus, string daystatus)
        {
            try
            {
                
                var sqlCondition = "";
                var condition2 = "";


                if(instatus != "null" && employeecategory == "null" && shift == "null"  && responsibleperson == "null" && daystatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "'";
                }
                if (employeecategory != "null" && instatus == "null" && shift == "null"  && responsibleperson == "null" && daystatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "EC.Id = '" + employeecategory + "'";
                }
                if (shift != "null" && employeecategory == "null" && instatus == "null"  && responsibleperson == "null" && daystatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "MBGT.ShiftDefinationId = '" + shift + "'";
                }
                
                if (responsibleperson != "null"  && shift == "null" && employeecategory == "null" && instatus == "null" && daystatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "EI2.SystemId = '" + responsibleperson + "'";
                }

                if (daystatus != "null" && responsibleperson == "null"  && shift == "null" && employeecategory == "null" && instatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.DayStatus = '" + daystatus + "'";
                }

                if (instatus != "null" && shift != "null"  && responsibleperson == "null"  && daystatus == "null" && employeestatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and MBGT.ShiftDefinationId = '" + shift + "'";
                }

                if (employeestatus != "null" && instatus == "null" && employeecategory == "null" && shift == "null"  && responsibleperson == "null" && daystatus == "null")
                {
                    sqlCondition = "EMP.EmployeeCurrentStatus = '" + employeestatus + "'";
                }

                if (instatus != "null" && shift != "null" &&  daystatus != "null" && responsibleperson == "null"  && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "'";
                }

                if (instatus != "null" && shift != "null" && daystatus != "null" && daystatus != "undefined" && employeecategory != "null" && responsibleperson == "null"  && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EC.Id = '" + employeecategory + "'";
                }

                if (instatus != "null" && shift != "null" && daystatus != "null"  && employeecategory != "null" && employeestatus != "null" && responsibleperson == "null" )
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EC.Id = '" + employeecategory + "' and EMP.EmployeeCurrentStatus = '" + employeestatus + "'";
                }

                if (instatus != "null" && daystatus != "null" && responsibleperson == "null"  && shift == "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "'";
                }

                if (instatus != "null" && employeecategory != "null" && responsibleperson == "null"  && shift == "null" && daystatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and EC.Id = '" + employeecategory + "'";
                }

                if (instatus != "null" && employeecategory != "null" && shift != "null" && responsibleperson == "null"  && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and EC.Id = '" + employeecategory + "' and MBGT.ShiftDefinationId = '" + shift + "'";
                }

                // Filter with ResponsiblePerson
                #region ResponsiblePerson
                if (instatus != "null" &&  responsibleperson != "null"  && daystatus == "null" && employeestatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and EI2.SystemId = '" + responsibleperson + "'";
                }
                if (shift != "null" && responsibleperson != "null"  && daystatus == "null" && employeestatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "MBGT.ShiftDefinationId = '" + shift + "' and EI2.SystemId = '" + responsibleperson + "'";
                }
                if (instatus != "null" && shift != "null" && responsibleperson != "null"  && daystatus == "null" && employeestatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EI2.SystemId = '" + responsibleperson + "'";
                }
                if (employeestatus != "null" && responsibleperson != "null" && instatus == "null" && employeecategory == "null" && shift == "null"  && daystatus == "null")
                {
                    sqlCondition = "EMP.EmployeeCurrentStatus = '" + employeestatus + "' and EI2.SystemId = '" + responsibleperson + "'";
                }


                if (instatus != "null" && shift != "null" && daystatus != "null" && responsibleperson != "null"  && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EI2.SystemId = '" + responsibleperson + "'";
                }

                if (instatus != "null" && shift != "null" && daystatus != "null" && daystatus != "undefined" && employeecategory != "null" && responsibleperson != "null"  && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EC.Id = '" + employeecategory + "' and EI2.SystemId = '" + responsibleperson + "'";
                }

                if (instatus != "null" && shift != "null" && daystatus != "null" && employeecategory != "null" && employeestatus != "null" && responsibleperson != "null" )
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EC.Id = '" + employeecategory + "' and EMP.EmployeeCurrentStatus = '" + employeestatus + "' and EI2.SystemId = '" + responsibleperson + "'";
                }

                if (instatus != "null" && daystatus != "null" && responsibleperson != "null"  && shift == "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and EI2.SystemId = '" + responsibleperson + "'";
                }

                if (instatus != "null" && employeecategory != "null" && responsibleperson != "null"  && shift == "null" && daystatus == "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and EC.Id = '" + employeecategory + "' and EI2.SystemId = '" + responsibleperson + "'";
                }

                if (instatus != "null" && employeecategory != "null" && shift != "null" && responsibleperson != "null"  && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and EC.Id = '" + employeecategory + "' and MBGT.ShiftDefinationId = '" + shift + "' and EI2.SystemId = '" + responsibleperson + "'";
                }


                #endregion ResponsiblePerson

                // Shift & Day Status
                if (shift != "null" && daystatus != "null" && employeecategory == "null" && responsibleperson == "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "'";
                }
                if (shift != "null" && daystatus != "null" && employeecategory != "null" && responsibleperson != "null" && employeestatus == "null")
                {
                    sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EC.Id = '" + employeecategory + "' and EI2.SystemId = '" + responsibleperson + "'";
                }

                if (sqlCondition == "")
                {
                    condition2 = "where APD.WorkDate between '" + fromdate + "' and '" + todate + "' and EMP.EmployeeStatus = 'Active'";
                }
                else
                {
                    condition2 = "where APD.WorkDate between '" + fromdate + "' and '" + todate + "' and EMP.EmployeeStatus = 'Active' and " + sqlCondition + @"";
                }

                var sql = @"Select ROW_NUMBER() OVER(ORDER BY APD.WorkDate DESC) SrlNo, UN.UserName Entity, D.UserName Division, DP.UserName Department, SC.UserName Section, SBC.UserName SubSection, POS.Activity, DM.UserName Designation, LDSG.UserName GivenDesignation
, ST.UserName [Shift], MBGT.Code BudgetCode, EMP.EmployeeCode, EMP.EmployeeName, EMP.CellPhnNo, S.UserName [State], EMP.DOJ, EC.UserName EmployeeCategory , APD.DayStatus, APD.InStatus, FORMAT(APD.InTime, 'hh:mm tt')InTime,  FORMAT(APD.OutTime, 'hh:mm tt')OutTime, APD.LateIn,  EMP.EmployeeStatus
,EI2.EmployeeName ResponsiblePerson, EFB.Action Feedback, FORMAT(EFB.AddedDate, 'dd-MMM-yyyy') FeedbackDate, ARM.UserName FeedbackRason, EFB.AddedBy FeedbackBy, RM.ResidenceNumber, RAE.isOccupied,  R.UserName TransportRoute
,MBGT2.Code ROBudgetCode,APD.WorkDate , PV.UpdatedBy
,ApprovedStatus = case when PV.UpdatedBy is not null then 'Approved' else 'Not Approved' end
from AttdnProcessData APD
left join EmployeeInformation EMP on EMP.SystemId = APD.EmpSystemID
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
left join  MST.ManpowerBudget MBGT2 on MBGT2.Id = MBGT.ROBudgetCode
LEFT JOIN EmployeeInformation EI2 on EI2.SystemId = MBGT.ResponsiblePerson
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
LEFT JOIN ORG.Division D on D.Id = POS.DivisionId

left join dbo.ShiftDefination ST on ST.SystemID = MBGT.ShiftDefinationId
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
left join hkp.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
left join EmployeeFeedback EFB on EFB.EmpSystemId = EMP.SystemId and EFB.Date between '" + fromdate + "' and '" + todate + @"'
left join [HKP].[AbsentismReasoningMaster] ARM on ARM.Id = EFB.ReasoningId
left join EmployeeInformation EI on EI.SystemId = EFB.EmpSystemId
LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId and RAE.isOccupied = 1
LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
left join EmployeeTransportAllocation ETA on ETA.EmployeeSystemId = EMP.SystemId and ETA.AssignStatus = 1
left join HKP.Stoppage SPG on SPG.Id = ETA.StoppageId
left join MST.RouteStoppage RSG on RSG.StoppageId = SPG.Id
left join MST.Route R on R.Id = RSG.RouteId
left join SCS.[State] S on S.Id = EMP.ParmStateId
left join (select distinct WorkDate, EmpSystemID, UpdatedBy  from PhysicalVerification)PV on PV.EmpSystemID = EMP.SystemId and PV.WorkDate = APD.WorkDate
--LEFT join TRN.TeamDefinition TD on TD.TeamLeaderId = EMP.SystemId
--LEFT JOIN EmployeeInformation TDEmp on TDEmp.SystemId = TD.TeamLeaderId
" + condition2+ " order by APD.WorkDate DESC";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #region Report

        [HttpPost, Authorize]
        public ActionResult GetDailyAttendanceStatusXls(string instatus, string fromdate, string todate, string employeecategory, string responsibleperson, string shift, string employeestatus, string daystatus, string SheetName)
        {
            try
            {

                string fileName = "";
                fileName = DailyAttendanceStatusReport(instatus, fromdate, todate, employeecategory,  responsibleperson, shift, employeestatus, daystatus, "DailyAttendanceStatusSummaryReport");

                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }



        public string DailyAttendanceStatusReport(string instatus, string fromdate, string todate ,string employeecategory, string responsibleperson, string shift, string employeestatus, string daystatus, string SheetName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            try
            {

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Daily Attendance Status Summary Report";
                sheet = workbook.Worksheets[0];
                DataTable data;
                DailyAttdnStatusReportQry(instatus, fromdate, todate ,employeecategory,  responsibleperson, shift, employeestatus, daystatus, out data);

                int ROW = 6; int COL = 1;

                #region Columns
                //sheet[ROW, COL].Text = "SrlNo";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColSrlNo = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Work Date";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColWorkDate = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Entity";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColEntity = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Division";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColDivision = COL;
                //COL++;

                sheet[ROW, COL].Text = "Employee Code";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEmpCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee Name";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEmployeeName = COL;
                COL++;

                //sheet[ROW, COL].Text = "Shift";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColShift = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Department";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColDepartment = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Section";
                //sheet[ROW, COL].ColumnWidth = 16;
                //int ColSection = COL;
                //COL++;

                sheet[ROW, COL].Text = "Sub Section";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColSubSec = COL;
                COL++;

                //sheet[ROW, COL].Text = "Activity";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColActivity = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Designation";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColDesignation = COL;
                //COL++;

                sheet[ROW, COL].Text = "Given Designation";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColGivenDesignation = COL;
                COL++;

                

                sheet[ROW, COL].Text = "Employee Category";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEmployeeCategory = COL;
                COL++;

                //sheet[ROW, COL].Text = "Budget Code";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColBudgetCode = COL;
                //COL++;

                //sheet[ROW, COL].Text = "State";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColState = COL;
                //COL++;

                sheet[ROW, COL].Text = "Mobile No";
                sheet[ROW, COL].ColumnWidth = 15;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColMobileNo = COL;
                COL++;

                sheet[ROW, COL].Text = "DOJ";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDOJ = COL;
                COL++;

                //sheet[ROW, COL].Text = "DOS";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColDOS = COL;
                //COL++;

                sheet[ROW, COL].Text = "Day Status";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDaySts = COL;
                COL++;

                sheet[ROW, COL].Text = "In Time";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColInTime = COL;
                COL++;

                sheet[ROW, COL].Text = "In Status";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColInStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "LateIn Time";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColLateIn = COL;
                COL++;

                //sheet[ROW, COL].Text = "InActive";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColInActive = COL;
                //COL++;

                sheet[ROW, COL].Text = "Employee Current Status";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEmpStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Responsible Person";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColResPerson = COL;
                COL++;

                //sheet[ROW, COL].Text = "Team Leader";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColTeamLeader = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Feedback";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColFeedback = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Feedback Date";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColFeedbackDate = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Reason";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColReason = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Feedback By";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColFeedbackBy = COL;
                //COL++;

                // Replace Residence Status with Residdence No
                sheet[ROW, COL].Text = "Residence Number";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColResidenceNo = COL;
                COL++;

                //sheet[ROW, COL].Text = "Present Area";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColPresentArea = COL;
                //COL++;

                //sheet[ROW, COL].Text = "Transport Status";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColTransportStatus = COL;
                //COL++;

                // Replace Stoppage with Transport Root
                sheet[ROW, COL].Text = "Transport Route";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColTransportRoute = COL;
                COL++;

                sheet[ROW, COL].Text = "Leave Status";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColVerifStatus = COL;
                COL++;

                // Add PO, RO1
                //sheet[ROW, COL].Text = "RO";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColRO = COL;
                // COL++;

                sheet[ROW, COL].Text = "RO Budget Code";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColROBudgetCode = COL;
                COL++;

                sheet[ROW, COL].Text = "PO Budget Code";
                sheet[ROW, COL].ColumnWidth = 16;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPOBudgetCode = COL;

                #endregion Columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                double[] arr = new double[3];
                for (int i = 0; i < data.Rows.Count; i++)
                {

                    //sheet[ROW, ColSrlNo].Number = clsStaticInfo.dbl(data.Rows[i]["SrlNo"].ToString());
                    //sheet[ROW, ColWorkDate].DateTime = Convert.ToDateTime(data.Rows[i]["WorkDate"].ToString());
                    //sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                    //sheet[ROW, ColDivision].Text = data.Rows[i]["Division"].ToString();
                    //sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                    //sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                    //sheet[ROW, ColSection].DateTime = Convert.ToDateTime(data.Rows[i]["DOJ"].ToString());
                    sheet[ROW, ColSubSec].Text = data.Rows[i]["SubSection"].ToString();
                    //sheet[ROW, ColActivity].Text = data.Rows[i]["Activity"].ToString();
                    //sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                    //sheet[ROW, ColShift].Text = data.Rows[i]["Shift"].ToString();
                    //sheet[ROW, ColBudgetCode].Number = clsStaticInfo.dbl(data.Rows[i]["BudgetCode"].ToString());
                    sheet[ROW, ColEmpCode].Number = clsStaticInfo.dbl(data.Rows[i]["EmployeeCode"].ToString());
                    sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColMobileNo].Number = clsStaticInfo.dbl(data.Rows[i]["CellPhnNo"].ToString());
                    sheet[ROW, ColDOJ].DateTime = Convert.ToDateTime(data.Rows[i]["DOJ"].ToString());
                    
                    //sheet[ROW, ColDOS].Text = data.Rows[i]["DOS"].ToString();
                    sheet[ROW, ColInTime].Text = data.Rows[i]["InTime"].ToString();
                    sheet[ROW, ColDaySts].Text = data.Rows[i]["DayStatus"].ToString();
                    sheet[ROW, ColInStatus].Text = data.Rows[i]["InStatus"].ToString();
                    sheet[ROW, ColLateIn].Number = clsStaticInfo.dbl(data.Rows[i]["LateIn"].ToString());
                    //sheet[ROW, ColInActive].Text = data.Rows[i]["InActive"].ToString();                   
                    sheet[ROW, ColEmpStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                    sheet[ROW, ColResPerson].Text = data.Rows[i]["ResponsiblePerson"].ToString();
                    //sheet[ROW, ColTeamLeader].Text = data.Rows[i]["TeamLeader"].ToString();
                    //sheet[ROW, ColFeedback].Text = data.Rows[i]["Feedback"].ToString();
                    //sheet[ROW, ColFeedbackDate].Text = data.Rows[i]["FeedbackDate"].ToString();
                    //sheet[ROW, ColReason].Text = data.Rows[i]["FeedbackRason"].ToString();
                    //sheet[ROW, ColFeedbackBy].Text = data.Rows[i]["FeedbackBy"].ToString();
                    sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                    sheet[ROW, ColGivenDesignation].Text = data.Rows[i]["GivenDesignation"].ToString();                    
                    //sheet[ROW, ColResidenceStatus].Text = data.Rows[i]["isOccupied"].ToString();                  
                    //sheet[ROW, ColPresentArea].Text = data.Rows[i]["PresentArea"].ToString();                  
                    sheet[ROW, ColVerifStatus].Text = data.Rows[i]["ApprovedStatus"].ToString();
                    //sheet[ROW, ColVerifiedBy].Text = data.Rows[i]["UpdatedBy"].ToString();
                    sheet[ROW, ColROBudgetCode].Number = clsStaticInfo.dbl(data.Rows[i]["ROBudgetCode"].ToString());
                    //sheet[ROW, ColTransportStatus].Text = data.Rows[i]["AssignStatus"].ToString();
                    sheet[ROW, ColTransportRoute].Text = data.Rows[i]["TransportRoute"].ToString();
                    sheet[ROW, ColResidenceNo].Text = data.Rows[i]["ResidenceNumber"].ToString();
                    //sheet[ROW, ColState].Text = data.Rows[i]["State"].ToString();


                    ROW++;
                }



                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Daily Attendance Status Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = false;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = true;
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;


                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public void DailyAttdnStatusReportQry(string instatus, string fromdate, string todate ,string employeecategory,  string responsibleperson, string shift, string employeestatus, string daystatus,  out DataTable data)
        {
            string strSQL;
            var sqlCondition = "";
            var condition2 = "";

            #region commented
            //if ((employeecategory != null || employeecategory != "null") && (teamleaderid != null || teamleaderid != "null") && (responsibleperson != null || responsibleperson != "null") && (shift != null || shift != "null"))
            //{
            //    sqlCondition = "EC.Id = " + employeecategory + " and TD.TeamLeaderId = " + teamleaderid + " and EI2.EmployeeName = " + responsibleperson + " and ST.Id = " + shift + "";
            //}

            //if (employeecategory != null && teamleaderid != null && responsibleperson != null)
            //{
            //    sqlCondition = "EC.Id = " + employeecategory + " and TD.TeamLeaderId = " + teamleaderid + " and EI2.EmployeeName = " + responsibleperson + "";
            //}

            //if (employeecategory != null && teamleaderid != null)
            //{
            //    sqlCondition = "EC.Id = " + employeecategory + " and TD.TeamLeaderId = " + teamleaderid + "";
            //}


            //if ((employeecategory != null || employeecategory != "null") && (shift != null || shift != "null"))
            //{
            //    sqlCondition = "EC.Id = " + employeecategory + "  and MBGT.ShiftDefinationId = '" + shift + "'";
            //}
            //if (employeecategory != null)
            //{
            //    sqlCondition = "EC.Id = " + employeecategory + "";
            //}
            #endregion commented

            if (instatus != null && employeecategory == null && shift == null  && responsibleperson == null && daystatus == null && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "'";
            }
            if (daystatus != null  && shift == null && employeecategory == null && instatus == null && employeestatus != null)
            {
                sqlCondition = "EI2.SystemId = '" + responsibleperson + "'";
            }
            if (employeecategory != null && instatus == null && shift == null  && responsibleperson == null && daystatus == null && employeestatus == null)
            {
                sqlCondition = "EC.Id = '" + employeecategory + "'";
            }
            if (shift != null && employeecategory == null && instatus == null  && responsibleperson == null && daystatus == null && employeestatus == null)
            {
                sqlCondition = "MBGT.ShiftDefinationId = '" + shift + "'";
            }
            
            if (responsibleperson != null  && shift == null && employeecategory == null && instatus == null && daystatus == null && employeestatus == null)
            {
                sqlCondition = "EI2.SystemId = '" + responsibleperson + "'";
            }
            if (employeestatus != null && instatus == null && employeecategory == null && shift == null  && responsibleperson == null && daystatus == null && employeestatus == null)
            {
                sqlCondition = "EMP.EmployeeCurrentStatus = '" + employeestatus + "'";
            }
            if (instatus != null && daystatus != null && responsibleperson == null  && shift == null && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "'";
            }
            if (instatus != null && shift != null && responsibleperson == null  && daystatus == null && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and MBGT.ShiftDefinationId = '" + shift + "'";
            }

            if (instatus != null && shift != null && daystatus != null && responsibleperson == null  && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "'";
            }

            if (instatus != null && shift != null && daystatus != null && employeecategory != null && responsibleperson == null  && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EC.Id = '" + employeecategory + "'";
            }

            if (instatus != null && shift != null && daystatus != null &&  employeecategory != null && employeestatus != null && responsibleperson == null )
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EC.Id = '" + employeecategory + "' and EMP.EmployeeCurrentStatus = '" + employeestatus + "'";
            }

            if (instatus != null && employeecategory != null && responsibleperson == null  && shift == null && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and EC.Id = '" + employeecategory + "'";
            }

            if (instatus != null && employeecategory != null && shift != null && responsibleperson == null  && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and EC.Id = '" + employeecategory + "' and MBGT.ShiftDefinationId = '" + shift + "'";
            }

            // Filter with ResponsiblePerson
            #region ResponsiblePerson
            if (instatus != null && responsibleperson != null  && daystatus == null && employeestatus == null && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and EI2.SystemId = '" + responsibleperson + "'";
            }
            if (shift != null && responsibleperson != null  && daystatus == null && employeestatus == null && employeestatus == null)
            {
                sqlCondition = "MBGT.ShiftDefinationId = '" + shift + "' and EI2.SystemId = '" + responsibleperson + "'";
            }
            if (instatus != null && shift != null && responsibleperson != null  && daystatus == null && employeestatus == null && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EI2.SystemId = '" + responsibleperson + "'";
            }
            if (employeestatus != null && responsibleperson != null && instatus == null && employeecategory == null && shift == null  && daystatus == null)
            {
                sqlCondition = "EMP.EmployeeCurrentStatus = '" + employeestatus + "' and EI2.SystemId = '" + responsibleperson + "'";
            }


            if (instatus != null && shift != null && daystatus != null && responsibleperson != null  && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EI2.SystemId = '" + responsibleperson + "'";
            }

            if (instatus != null && shift != null && daystatus != null && daystatus != "undefined" && employeecategory != null && responsibleperson != null  && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EC.Id = '" + employeecategory + "' and EI2.SystemId = '" + responsibleperson + "'";
            }

            if (instatus != null && shift != null && daystatus != null && employeecategory != null && employeestatus != null && responsibleperson != null )
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EC.Id = '" + employeecategory + "' and EMP.EmployeeCurrentStatus = '" + employeestatus + "' and EI2.SystemId = '" + responsibleperson + "'";
            }

            if (instatus != null && daystatus != null && responsibleperson != null  && shift == null && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and EI2.SystemId = '" + responsibleperson + "'";
            }

            if (instatus != null && employeecategory != null && responsibleperson != null  && shift == null && daystatus == null && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and EC.Id = '" + employeecategory + "' and EI2.SystemId = '" + responsibleperson + "'";
            }

            if (instatus != null && employeecategory != null && shift != null && responsibleperson != null  && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and EC.Id = '" + employeecategory + "' and MBGT.ShiftDefinationId = '" + shift + "' and EI2.SystemId = '" + responsibleperson + "'";
            }

            // Shift & DayStatus
            if (shift != null && daystatus != null && employeecategory == null && responsibleperson == null && employeestatus == null)
            {
                sqlCondition = "APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "'";
            }
            if (shift != null && daystatus != null && employeecategory != null && responsibleperson != null && employeestatus == null)
            {
                sqlCondition = "APD.InStatus = '" + instatus + "' and APD.DayStatus = '" + daystatus + "' and MBGT.ShiftDefinationId = '" + shift + "' and EC.Id = '" + employeecategory + "' and EI2.SystemId = '" + responsibleperson + "'";
            }


            #endregion ResponsiblePerson

            if (sqlCondition == "")
            {
                condition2 = "where APD.WorkDate between '" + fromdate + "' and '" + todate + "' and EMP.EmployeeStatus = 'Active'";
            }
            else
            {
                condition2 = "where APD.WorkDate between '" + fromdate + "' and '" + todate + "' and EMP.EmployeeStatus = 'Active' and " + sqlCondition + @"";
            }

            try
            {

                strSQL = @"Select EMP.EmployeeCode, EMP.EmployeeName, SBC.UserName SubSection, LDSG.UserName GivenDesignation, EC.UserName EmployeeCategory, FORMAT(EMP.DOJ, 'dd-MMM-yyyy')DOJ, EMP.CellPhnNo, APD.DayStatus, APD.InStatus
,FORMAT(APD.InTime, 'hh:mm tt')InTime, EMP.EmployeeStatus, EI2.EmployeeName ResponsiblePerson, RM.ResidenceNumber, R.UserName TransportRoute
,APD.LateIn
,ApprovedStatus = case when PV.UpdatedBy is not null then 'Approved' else 'Not Approved' end
,MBGT2.Code ROBudgetCode --, RO.EmployeeName
from AttdnProcessData APD
left join EmployeeInformation EMP on EMP.SystemId = APD.EmpSystemID
LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
left join  MST.ManpowerBudget MBGT2 on MBGT2.Id = MBGT.ROBudgetCode
--left join (select distinct   EmployeeName, BudgetCode from EmployeeInformation ) RO on RO.BudgetCode = MBGT2.Id 
LEFT JOIN EmployeeInformation EI2 on EI2.SystemId = MBGT.ResponsiblePerson
LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
LEFT JOIN ORG.Division D on D.Id = POS.DivisionId

left join dbo.ShiftDefination ST on ST.SystemID = MBGT.ShiftDefinationId
left join ORG.Entity UN on UN.Id = MBGT.EntityId
left join ORG.Department DP on DP.ID = POS.DepartmentId
left join ORG.Section SC on SC.Id = POS.SectionId
left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
left join hkp.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
left join EmployeeFeedback EFB on EFB.EmpSystemId = EMP.SystemId and EFB.Date between '21-Feb-2023' and '22-Feb-2023'
left join [HKP].[AbsentismReasoningMaster] ARM on ARM.Id = EFB.ReasoningId
left join EmployeeInformation EI on EI.SystemId = EFB.EmpSystemId
LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId and RAE.isOccupied = 1
LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
left join EmployeeTransportAllocation ETA on ETA.EmployeeSystemId = EMP.SystemId and ETA.AssignStatus = 1
left join HKP.Stoppage SPG on SPG.Id = ETA.StoppageId
left join MST.RouteStoppage RSG on RSG.StoppageId = SPG.Id
left join MST.Route R on R.Id = RSG.RouteId
left join SCS.[State] S on S.Id = EMP.ParmStateId
left join (select distinct WorkDate, EmpSystemID, UpdatedBy  from PhysicalVerification)PV on PV.EmpSystemID = EMP.SystemId and PV.WorkDate = APD.WorkDate
LEFT join TRN.TeamDefinition TD on TD.TeamLeaderId = EMP.SystemId
LEFT JOIN EmployeeInformation TDEmp on TDEmp.SystemId = TD.TeamLeaderId
" + condition2 + " order by APD.WorkDate DESC";

                data = _sqlRepository.GetDataTable(strSQL);



            }
            catch (Exception ex)
            {
                throw ex;
            }

        }//End Function
        #endregion

        #region Save
        [HttpPost, Authorize, AllowAnonymous]
        public JsonResult Save(Dictionary<string, object> datas, string employeeId)
        {
            try
            {
                string TableNameHead = "[TRN].[DailyAttendanceFavoriteFilter]";

                DataSet dsMaster, dsEmpId;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where FavoriteName='" + datas["FavoriteName"] + "' AND  Id='" + datas["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Favorite Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + datas["Id"] + "'", out dsMaster, false, "1");


                string _Id = "";

                #region  HEAD
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);
                    datas["Id"] = _Id;
                    if (!string.IsNullOrEmpty(identity.EmployeeId))
                    {                        
                        datas["FavoriteFilterEmployeeId"] = employeeId;
                    }
                    else
                    {
                        datas["FavoriteFilterUserId"] = identity.UserId; // Upanel
                    }
                   
                    AddNewRow(dsMaster.Tables[0], datas);
                }
                else
                {
                    _Id = datas["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], datas);
                }
                #endregion  HEAD
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Data = datas, Message = AplosMessage.Insert }) ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region CREATE AND EDIT DEFAULT COLUMN
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
            dr["AddedDate"] = System.DateTime.Now.ToString();
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
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion CREATE AND EDIT DEFAULT COLUMN
        #endregion Save
    }
}