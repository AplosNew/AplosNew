using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.HumanResources;
using Syncfusion.XlsIO;

namespace Aplos.Areas.Machines.Controllers
{
    public class TeamPlanReportController : Controller
    {
        #region Constructor


        private readonly IAttendanceManagementService _AttendanceManagementService;
        ResudeceStatusReportService rsr = new ResudeceStatusReportService();
        private readonly ISqlRepository _sqlRepository;

        public TeamPlanReportController(IAttendanceManagementService AttendanceManagementService, ISqlRepository R)
        {
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetTeamNameList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from TRN.TeamDefinition where Active=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEntityList(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select E.Id as Value,E.UserName as Text from TRN.TeamEntity TE
left join ORG.Entity E ON E.Id=TE.EntityId
where TE.TeamDefinitionId='"+TeamId+"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTeamCategoryList(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select TC.Id as Value,TC.UserName as Text from TRN.TeamDefinitionCategory TDC
left join hkp.TeamCategory TC ON TC.Id=TDC.TeamCategoryId
where TDC.TeamDefinitionId='" + TeamId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetBudgetCodeList(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select MB.Id as Value,MB.Code as Text from TRN.TeamBudgetCode TB
left join MST.ManpowerBudget MB ON MB.Id=TB.BudgetCodeId
where TB.TeamDefinitionId='" + TeamId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeList(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select EI.SystemId as Value,EI.EmployeeName as Text from TRN.TeamDefinitionEmployee TDE
left join EmployeeInformation EI ON EI.SystemId=TDE.EmployeeId
where TDE.TeamDefinitionId='" + TeamId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetActivityCategoryList(string EmpId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select EAC.Id as Value,EAC.UserName as Text from TRN.TeamDefinitionEmployee TDE
left join hkp.EmployeeActivityCategory EAC ON EAC.Id=TDE.EmployeeActiviyCategory
where EmployeeId='" + EmpId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult XlsTeamPlanReport(string todate, string fromDate, string teamName, string employeeId)
        {
            try
            {
                var workbook = TeamPlanReport(todate, fromDate, teamName, employeeId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "TeamPlanReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [Authorize, HttpPost]
        private IWorkbook TeamPlanReport(string todate, string fromDate, string teamName, string employeeId)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = rsr.TeamPlanReport(todate, fromDate, teamName, employeeId);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Team Plan Report";

            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            int COLHeader = 0;

            report.SetHeaderText(ref sheet, ROW, COLHeader + 6, "Team Plan Report :", 20, ExcelHAlign.HAlignCenter);
            sheet.Range[ROW, COLHeader + 6, ROW, COLHeader + 7].Merge();
            ROW++;
            #region Grid Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Team Name", 12, ExcelHAlign.HAlignCenter);
            int ColTeamName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 12, ExcelHAlign.HAlignCenter);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 12, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Section", 12, ExcelHAlign.HAlignCenter);
            int ColSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Sub Section", 12, ExcelHAlign.HAlignCenter);
            int ColSubSection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 15, ExcelHAlign.HAlignCenter);
            int ColEmployeeCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Designation", 15, ExcelHAlign.HAlignCenter);
            int ColDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Legal Designation", 12, ExcelHAlign.HAlignCenter);
            int ColLegalDesignation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Shift", 12, ExcelHAlign.HAlignCenter);
            int ColShift = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsibility Level", 12, ExcelHAlign.HAlignCenter);
            int ColResponsibilityLevel = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EmpId", 12, ExcelHAlign.HAlignCenter);
            int ColEmpId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Name", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DOJ", 12, ExcelHAlign.HAlignCenter);
            int ColDOJ = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Current Status", 12, ExcelHAlign.HAlignCenter);
            int ColEmployeeCurrentStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Week Off", 12, ExcelHAlign.HAlignCenter);
            int ColWeekOff = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Date", 12, ExcelHAlign.HAlignCenter);
            int ColDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Day Status", 12, ExcelHAlign.HAlignCenter);
            int ColDayStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plan Hours", 12, ExcelHAlign.HAlignCenter);
            int ColPlanHours = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Available Hours", 12, ExcelHAlign.HAlignCenter);
            int ColAvailableHours = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Alloted Hours", 12, ExcelHAlign.HAlignCenter);
            int ColAllotedHours = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Actual Hours", 12, ExcelHAlign.HAlignCenter);
            int ColActualHours = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 12, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Team Category", 12, ExcelHAlign.HAlignCenter);
            int ColTeamCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "EA Category", 12, ExcelHAlign.HAlignCenter);
            int ColEACategory = COL;

            ROW++;
            endCol = COL;
            #endregion Headers

            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {
                sheet[ROW, ColTeamName].Text = data.Rows[i]["TeamName"].ToString();
                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColSection].Text = data.Rows[i]["Section"].ToString();
                sheet[ROW, ColSubSection].Text = data.Rows[i]["SubSection"].ToString();
                sheet[ROW, ColEmployeeCategory].Text = data.Rows[i]["EmployeeCategory"].ToString();
                sheet[ROW, ColDesignation].Text = data.Rows[i]["Designation"].ToString();
                sheet[ROW, ColLegalDesignation].Text = data.Rows[i]["LegalDesignation"].ToString();
                sheet[ROW, ColShift].Text = data.Rows[i]["Shift"].ToString();
                sheet[ROW, ColResponsibilityLevel].Text = data.Rows[i]["ResponsibilityLevel"].ToString();
                sheet[ROW, ColEmpId].Text = data.Rows[i]["EmpId"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColDOJ].Text = data.Rows[i]["DOJ"].ToString();
                sheet[ROW, ColEmployeeStatus].Text = data.Rows[i]["EmployeeStatus"].ToString();
                sheet[ROW, ColEmployeeCurrentStatus].Text = data.Rows[i]["EmployeeCurrentStatus"].ToString();
                sheet[ROW, ColWeekOff].Text = data.Rows[i]["WeekOff"].ToString();
                sheet[ROW, ColDate].Text = data.Rows[i]["Date"].ToString();
                sheet[ROW, ColDayStatus].Text = data.Rows[i]["DayStatus"].ToString();
                sheet[ROW, ColPlanHours].Number = clsStaticInfo.dbl(data.Rows[i]["PlanHours"].ToString());
                sheet[ROW, ColAvailableHours].Number = clsStaticInfo.dbl(data.Rows[i]["AvailableHours"].ToString());
                sheet[ROW, ColAllotedHours].Number = clsStaticInfo.dbl(data.Rows[i]["AllotedHours"].ToString());
                sheet[ROW, ColActualHours].Number = clsStaticInfo.dbl(data.Rows[i]["ActualHours"].ToString());
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColEACategory].Text = data.Rows[i]["EACategory"].ToString();

                ROW++;

            }

            ROW++;


            sheet.Range[ROW, ColEntity, ROW, endCol].CellStyle.Font.Bold = true;
            endRow = ROW - 1;
            endRow = ROW - 1;
            #endregion sheet1

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, startRow, endCol];

            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        [Authorize, HttpGet]
        public ActionResult LoadTeamPlanReportList(string ToDate, string FromDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select TD.UserName TeamName,E.UserName Entity,
DEP.UserName AS Department,S.UserName as Section,SS.UserName as SubSection,EC.UserName EmployeeCategory,DEG.UserName as Designation,
LD.UserName as LegalDesignation,SD.UserName as Shift,TDE.ResponsibilityLevel,EI.SystemId as EmpId,EI.EmployeeName,format(EI.DOJ,'dd-MMM-yyyy') as DOJ,
EI.EmployeeStatus,EI.EmployeeCurrentStatus, 
(select top 1 DATENAME(WEEKDAY, AD.WorkDate) from AttdnProcessData AD where AD.EmpSystemID=TDE.EmployeeId and AD.DayStatus='W') as  WeekOff,
format(APD.WorkDate,'dd-MMM-yyyy') as Date,APD.DayStatus,TDE.PlanHours,isnull(FLOOR(APD.Duration/60),0) AvailableHours,
isnull(Alloted.AllotedHours,0) as AllotedHours,
isnull(Actual.ActualHours,0) as ActualHours,
TDE.Remarks,
TC.UserName as TeamCategory,
EAC.UserName EACategory
from TRN.TeamDefinition TD
left join TRN.TeamEntity TE ON TE.TeamDefinitionId=TD.Id 
left join ORG.Entity E ON E.Id=TE.EntityId
left join TRN.TeamDefinitionEmployee TDE ON TDE.TeamDefinitionId=TD.Id
left join hkp.EmployeeActivityCategory EAC ON EAC.Id=TDE.EmployeeActiviyCategory
left join EmployeeInformation EI ON EI.SystemId=TDE.EmployeeId  
left join [MST].[ManpowerBudget]  MB ON MB.Id=EI.BudgetCode
left join ORG.Position P on P.Id = MB.PositionId
left join ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
left join HKP.Designation DEG ON DEG.Id=EI.DesignationSystemID
left join ORG.Section S ON S.Id=P.SectionId
left join ORG.SubSection SS ON SS.Id=P.SubSectionId
left join TRN.TeamDefinitionCategory TDC ON TDC.TeamDefinitionId=TD.Id
left join hkp.TeamCategory TC ON TC.Id=TDC.TeamCategoryId
left join HKP.LegalDesignation LD ON LD.Id=EI.LegalDesignationId
left join ShiftDefination SD ON SD.SystemID=MB.ShiftDefinationId
left join MST.DesignationMaster DM on DM.DesignationId = P.DesignationId
left join HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
left join AttdnProcessData APD ON APD.EmpSystemID=TDE.EmployeeId
left join (select  (sum(isnull(PlanMinutes,0))/60) AllotedHours,ResponsiblePersonId,mapd.PlannedDate from TRN.ResponsiblePlannedDetails rpd 
left join (select Id,PlannedDate,ActualDate from TRN.MachineAssetPlannedDetails )mapd on mapd.Id=rpd.PlannedId
group by rpd.ResponsiblePersonId,mapd.PlannedDate) Alloted ON Alloted.ResponsiblePersonId=TDE.EmployeeId and Alloted.PlannedDate=APD.WorkDate
left join (select  (sum(isnull(ActualMinutes,0))/60) ActualHours,ResponsiblePersonId,mapd.PlannedDate from TRN.ResponsiblePlannedDetails rpd 
left join (select Id,PlannedDate,ActualDate from TRN.MachineAssetPlannedDetails )mapd on mapd.Id=rpd.PlannedId  
group by rpd.ResponsiblePersonId,mapd.PlannedDate) Actual ON Actual.ResponsiblePersonId=TDE.EmployeeId and Actual.PlannedDate=APD.WorkDate 
where EI.EmployeeStatus='Active' and APD.WorkDate between '" + FromDate + "' and '" + ToDate + "'";
            //return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            var jsondata = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        #endregion -- Operations
    }
}