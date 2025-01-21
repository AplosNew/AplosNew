using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Aplos.Controllers;
using Library.Service.Employees;
using Library.Model.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using Syncfusion.DocIO.DLS;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Library.HumanResource.NewAttendanceProcess;
using Library.Data;

namespace Aplos.Areas.Machines.Controllers
{
    public class SkillManagementDetailsController : Controller
    {
        #region Constructor


        private readonly IAttendanceManagementService _AttendanceManagementService;
        ResudeceStatusReportService rsr = new ResudeceStatusReportService();
        private readonly ISqlRepository _sqlRepository;

        public SkillManagementDetailsController(IAttendanceManagementService AttendanceManagementService, ISqlRepository R)
        {
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetPerformancePointsList(string PerformanceGroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,PerformancePoints as Text from TRN.SkillManagementLevel where PerformanceGroup='" + PerformanceGroup + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFromDateList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select top 1 format(MPD.ActualDate+MS.ScheduleDays,'dd-MMM-yyyy') FromDate from  [TRN].[EmployeePlannedDetails] MPD
 left join TRN.SkillManagementPositionCode SPC ON SPC.Id=MPD.PositionCodeId
 left join TRN.SkillManagement MS ON SPC.SMID=MS.Id
where MPD.ActualDate is not null  
order by MPD.ActualDate asc";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetActionablePersonList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @" select distinct EI.EmployeeName as Text,RP.ResponsiblePersonId as Value
 from [TRN].[EmployeePlannedDetails] MPD
 left join TRN.SkillResponsiblePlannedDetails RP ON RP.PlannedId=MPD.Id
 left join EmployeeInformation EI ON EI.SystemId=RP.ResponsiblePersonId
 where (ActualDate is not null or ActualDate is null) and RP.ResponsiblePersonId is not null and RP.IsActive=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadSkillManagementStatusDetailsList(string ToDate, string FromDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select SM.Id,E.UserName Entity,SME.Id as EntityId,SPC.Id as PositionId,P.UserName as Process,(select P.UserName from HKP.Process PR where PR.Id=SP.ProcessId) as SubProcess,SM.UserName ScheduleName,SM.AdvancePlanningDays,SM.ScheduleCode,MB.Code ResponsiblePersonBudgetCode,EI.SystemId as EmployeeId,EI.EmployeeName,
P.Code as PositionCode,DIV.UserName Division,DEP.UserName EmpDepartment,S.UserName Section,SS.UserName SubSection,EB.Code as BudgetCode,
P.Activity,DEG.UserName Designation,SM.ScheduleDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
SM.StandardScheduleMinutes,SM.Remarks,(select D.UserName Department from Org.Department D where D.Id=SM.DepartmentId) as Department,SM.TrainingGroup,'Status Details' as SD
 ,Format(EPD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,EPD.Id as PlannedId
from TRN.SkillManagement SM
left join HKP.Process PRO ON PRO.Id=SM.ProcessId
left join HKP.SubProcess SP ON SP.Id=SM.SubProcessId
left join MST.ManpowerBudget MB ON MB.id=SM.ResponsiblePersoneBgtCodeId
left join TRN.SkillManagementEntity SME ON SME.SMID=SM.Id
left Join Org.Entity E ON E.Id=SME.EntityId
left join [TRN].[SkillManagementPositionCode] SPC ON SPC.SMID=SM.Id
left join EmployeeInformation EI ON EI.EmployeeStatus='Active' --and EI.PositionID=SPC.PositionCodeId
left join MST.ManpowerBudget EB ON EB.Id=EI.BudgetCode
left Join ORG.Position P ON P.Id=EB.PositionID
left join org.Division DIV ON DIV.Id=P.DivisionId
left join Org.Department DEP ON DEP.Id=p.DepartmentId
left join Org.Section S ON S.Id=p.SectionId
left join Org.SubSection SS ON SS.Id=p.SubSectionId
left join HKP.Designation DEG ON DEG.Id=EI.GivenDesignationId
left join TRN.EmployeePlannedDetails EPD ON EPD.Id=(select top 1 Id from [TRN].EmployeePlannedDetails ED where ED.EmployeeId=EI.SystemId and ED.PositionCodeId=SPC.Id and ED.EntityId=SME.Id order by ED.ActualDate desc)
where SM.IsActive=1 and EI.SystemId is not null 
 and  Case when isnull((SELECT TOP 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + @"' 
 and (select count(Id) from [TRN].[EmployeePlannedDetails] APD where APD.EmployeeId=EI.SystemId and APD.PositionCodeId=SPC.Id and APD.EntityId=SME.Id and
 APD.PlannedDate is not null and APD.ActualDate is null) = 0 order by Case when 
 isnull((SELECT TOP 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id ORDER BY APD.Id DESC),'')= '' then GETDATE() 
 else (SM.ScheduleDays + (select top 1 ActualDate from[TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id ORDER BY APD.Id DESC)) end";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSkillManagementStatusSummaryList(string ToDate, string FromDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select X.Id,X.EntityId,X.Entity,X.PositionId,X.SEntityId,X.Process,X.SubProcess,X.ScheduleName,X.ScheduleCode,X.ResponsiblePersonBudgetCode,
count(X.NoOfEmployee) as NoOfEmployee,sum(X.OverDue) as OverDue,sum(X.DueToday) as DueToday,sum(X.FutureDue) as FutureDue,X.Remarks,X.PlanStatus,X.Department,X.TrainingGroup from (
select SM.Id,SME.Id as EntityId,E.UserName Entity,SPC.Id as PositionId,SME.Id as SEntityId,P.UserName as Process,(select P.UserName from HKP.Process PR where PR.Id=SP.ProcessId) as SubProcess,SM.UserName ScheduleName,SM.ScheduleCode,MB.Code ResponsiblePersonBudgetCode,count(EI.SystemId) NoOfEmployee,
SM.ScheduleDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
SM.Remarks,(select count(EPD.Id) from [TRN].[EmployeePlannedDetails] EPD where EPD.PlannedDate is null) as PlanStatus,
(select D.UserName Department from Org.Department D where D.Id=SM.DepartmentId) as Department,SM.TrainingGroup
 from TRN.SkillManagement SM
left join HKP.Process P ON P.Id=SM.ProcessId
left join HKP.SubProcess SP ON SP.Id=SM.SubProcessId
left join MST.ManpowerBudget MB ON MB.id=SM.ResponsiblePersoneBgtCodeId
left join TRN.SkillManagementEntity SME ON SME.SMID=SM.Id
left Join Org.Entity E ON E.Id=SME.EntityId
left join [TRN].[SkillManagementPositionCode] SPC ON SPC.SMID=SM.Id
--left join MST.ManpowerBudget B ON B.PositionId=SPC.PositionCodeId
left join EmployeeInformation EI ON EI.EmployeeStatus='Active' and EI.PositionID=SPC.PositionCodeId
left join TRN.EmployeePlannedDetails EPD ON EPD.Id=(select top 1 Id from [TRN].EmployeePlannedDetails ED where ED.EmployeeId=EI.SystemId and ED.PositionCodeId=SPC.Id order by ED.ActualDate desc)
 where SM.IsActive=1 and EI.SystemId is not null and Case when isnull((SELECT TOP 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (SM.ScheduleDays+(select top 1 ActualDate from [TRN].[EmployeePlannedDetails] APD where APD.Id=EPD.Id
 ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + @"' and (select count(Id) from [TRN].[EmployeePlannedDetails] APD
 where APD.EmployeeId=EI.SystemId and APD.PositionCodeId=SPC.Id and APD.EntityId=SME.Id and APD.PlannedDate is not null and APD.ActualDate is null) = 0 
 group by SM.Id,EI.SystemId,EPD.Id,E.Id,E.UserName,SPC.Id,SME.Id,P.Id,P.UserName,SP.ProcessId,SM.UserName,SM.ScheduleCode,
 MB.Code,SM.LastMaintenanceDate,SM.ScheduleDays,SM.Remarks,SM.DepartmentId,SM.TrainingGroup) X group by NoOfEmployee,Id,EntityId,Entity,PositionId,SEntityId,Process,SubProcess,ScheduleName,ScheduleCode,ResponsiblePersonBudgetCode,Remarks,PlanStatus,Department,TrainingGroup";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSkillManagementStatusPlannedList(string ToDate, string FromDate, string SMID, string PositionCodeId, string EntityId, string Value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,'' Id,(select top 1 Id from TRN.EmployeePlannedDetails where PositionCodeId=SPC.Id order by PlannedDate desc) as PlannedId,SM.Id as SMId,SPC.Id as PositionCodeId,SPE.Id as EntityId,EI.SystemId as EmployeeId,EI.EmployeeName,
P.Code as PositionCode,DIV.UserName Division,DEP.UserName EmpDepartment,S.UserName Section,SS.UserName SubSection,EB.Code as BudgetCode,P.Activity,DEG.UserName Designation,SM.ScheduleDays,SM.AdvancePlanningDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails MPD where MPD.Id=APD.Id
 ORDER BY MPD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
SM.StandardScheduleMinutes,
''  as PlannedDate,
(CASE WHEN APD.ActualDate IS NULL THEN 0 ELSE 1 END) as [Status],
'' as ActualDate,
APD.Remarks as Remark
 from TRN.SkillManagement SM
 left join TRN.SkillManagementEntity SPE ON SPE.SMID=SM.Id
 left join TRN.SkillManagementPositionCode SPC ON SPC.SMID=SM.Id
 left join EmployeeInformation EI ON EI.EmployeeStatus='Active' --and EI.PositionID=SPC.PositionCodeId 
 left Join TRN.EmployeePlannedDetails APD ON APD.PositionCodeId=SPC.Id and APD.Id=(select top 1 Id from TRN.EmployeePlannedDetails MAPD where MAPD.PositionCodeId=SPC.Id and MAPD.EntityId=SPE.Id and MAPD.EmployeeId=EI.SystemId order by MAPD.ActualDate desc)
  LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
 left Join Org.Entity E ON E.Id=SPE.EntityId
 left Join ORG.Position P ON P.Id=mb.PositionID
 left join org.Division DIV ON DIV.Id=p.DivisionId
 left join Org.Department DEP ON DEP.Id=p.DepartmentId
 left join Org.Section S ON S.Id=p.SectionId
 left join Org.SubSection SS ON SS.Id=p.SubSectionId
 left join MST.ManpowerBudget EB ON EB.Id=EI.BudgetCode
 left join HKP.Designation DEG ON DEG.Id=EI.GivenDesignationId
 where SM.IsActive=1 and EI.SystemId is not null and SPC.Id='" + PositionCodeId + "' and SPE.Id='" + EntityId + "' and SM.Id = '" + SMID + @"'  and 1 = '" + Value + @"' and APD.Id is null
 and (select count(Id) from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id and APD.PlannedDate is null
 and APD.ActualDate is null and APD.EntityId=SPE.Id) = 0";
            //and Case when isnull((SELECT TOP 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id ORDER BY APD.Id DESC),'')= '' then GETDATE() else (SM.ScheduleDays +
            //  (select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id ORDER BY APD.Id DESC)) end
            //  between '" + FromDate + "' and '" + ToDate + "'
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSkillManagementStatusPlannedListDetails(string ToDate, string FromDate, string SMId, string EntityId, string PositionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,APD.Id,(select top 1 Id from TRN.EmployeePlannedDetails where PositionCodeId=SPC.Id and EmployeeId=EI.SystemId and EntityId=SPE.Id order by PlannedDate desc) as PlannedId,SM.Id as SMId,SPC.Id as PositionCodeId,SPE.Id as EntityId,EI.SystemId as EmployeeId,EI.EmployeeName,
P.Code as PositionCode,DIV.UserName Division,DEP.UserName EmpDepartment,S.UserName Section,SS.UserName SubSection,EB.Code as BudgetCode,P.Activity,DEG.UserName Designation,
SM.ScheduleDays,SM.AdvancePlanningDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails MPD where MPD.Id=APD.Id
 ORDER BY MPD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
SM.StandardScheduleMinutes,'' as PlannedDate,
(CASE WHEN APD.ActualDate IS NULL THEN 0 ELSE 1 END) as [Status],'' as ActualDate,
APD.Remarks
 from TRN.SkillManagement SM
 left join TRN.SkillManagementEntity SPE ON SPE.SMID=SM.Id
 left join TRN.SkillManagementPositionCode SPC ON SPC.SMID=SM.Id
 left Join TRN.EmployeePlannedDetails APD ON APD.PositionCodeId=SPC.Id
 left join EmployeeInformation EI ON EI.EmployeeStatus='Active' --and EI.PositionID=SPC.PositionCodeId 
 left join MST.ManpowerBudget EB ON EB.Id=EI.BudgetCode
 left Join Org.Entity E ON E.Id=SPE.EntityId
 left Join ORG.Position P ON P.Id=eb.PositionID
 left join org.Division DIV ON DIV.Id=p.DivisionId
 left join Org.Department DEP ON DEP.Id=p.DepartmentId
 left join Org.Section S ON S.Id=p.SectionId
 left join Org.SubSection SS ON SS.Id=p.SubSectionId
 left join HKP.Designation DEG ON DEG.Id=EI.GivenDesignationId
 where SM.IsActive=1 and SPC.Id is not null and SPE.Id='" + EntityId + "' and SM.Id='" + SMId + @"' 
 and Case when isnull((SELECT TOP 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id ORDER BY APD.Id DESC),'')= ''
 then GETDATE() else (SM.ScheduleDays + (select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id
 ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + @"' and APD.Id = (select top 1 Id from TRN.EmployeePlannedDetails PD
 where PD.PositionCodeId = '" + PositionId + "' and PD.EmployeeId=EI.SystemId and PD.EntityId=SPE.Id order by PD.PlannedDate desc)";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadSkillManagementStatusPlannedListGetDetails(string ToDate, string FromDate, string SMId, string EntityId, string PositionId, string EmployeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,'' Id,(select top 1 Id from TRN.EmployeePlannedDetails where PositionCodeId=SPC.Id order by PlannedDate desc) as PlannedId,EI.SystemId as EmployeeId,EI.EmployeeName,
P.Code as PositionCode,DIV.UserName Division,DEP.UserName EmpDepartment,S.UserName Section,SS.UserName SubSection,EB.Code as BudgetCode,P.Activity,DEG.UserName Designation,
SM.Id as SMId,SPC.Id as PositionCodeId,E.UserName Entity,SPE.Id as EntityId,SM.ScheduleDays,SM.AdvancePlanningDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails MPD where MPD.Id=APD.Id
 ORDER BY MPD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
SM.StandardScheduleMinutes,'' as PlanDate,
(CASE WHEN APD.ActualDate IS NULL THEN 0 ELSE 1 END) as [Status],'' as ActualDate,
APD.Remarks
 from TRN.SkillManagement SM
 left join TRN.SkillManagementEntity SPE ON SPE.SMID=SM.Id
 left join TRN.SkillManagementPositionCode SPC ON SPC.SMID=SM.Id
 left Join TRN.EmployeePlannedDetails APD ON APD.PositionCodeId=SPC.Id and APD.Id=(select top 1 Id from TRN.EmployeePlannedDetails MAPD where MAPD.PositionCodeId=SPC.Id and MAPD.EmployeeId='" + EmployeeId + @"' and MAPD.EntityId=SPE.Id order by MAPD.ActualDate desc)
 left join EmployeeInformation EI ON EI.EmployeeStatus='Active' --and EI.PositionID=SPC.PositionCodeId
and EI.SystemId='" + EmployeeId + @"'
 left join MST.ManpowerBudget EB ON EB.Id=EI.BudgetCode
 left Join Org.Entity E ON E.Id=SPE.EntityId
 left Join ORG.Position P ON P.Id=eb.PositionID
 left join org.Division DIV ON DIV.Id=p.DivisionId
 left join Org.Department DEP ON DEP.Id=p.DepartmentId
 left join Org.Section S ON S.Id=p.SectionId
 left join Org.SubSection SS ON SS.Id=p.SubSectionId
 left join HKP.Designation DEG ON DEG.Id=EI.GivenDesignationId
 where SM.IsActive=1 and SPC.Id is not null and SPE.Id='" + EntityId + "' and SM.Id='" + SMId + @"' 
 and SPC.Id = '" + PositionId + @"' and EI.SystemId = '" + EmployeeId + @"'
 and(select count(Id) from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id and APD.PlannedDate is null
 and APD.ActualDate is null and APD.EntityId=SPE.Id) = 0";
 //           and Case when isnull((SELECT TOP 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id ORDER BY APD.Id DESC),'')= ''
 //then GETDATE() else (SM.ScheduleDays + (select top 1 ActualDate from TRN.EmployeePlannedDetails APD where
 //APD.PositionCodeId = SPC.Id ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + "'
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadSkillManagementStatusPlannedListGetPlanDetails(string ToDate, string FromDate, string SMId, string EntityId, string PositionId, string EmployeeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,APD.Id,(select top 1 Id from TRN.EmployeePlannedDetails where PositionCodeId=SPC.Id and EmployeeId=EI.SystemId and EntityId=SPE.Id order by PlannedDate desc) as PlannedId,SM.Id as SMId,SPC.Id as PositionCodeId,SPE.Id as EntityId,EI.SystemId as EmployeeId,EI.EmployeeName,
P.Code as PositionCode,DIV.UserName Division,DEP.UserName EmpDepartment,S.UserName Section,SS.UserName SubSection,EB.Code as BudgetCode,P.Activity,DEG.UserName Designation,
SM.ScheduleDays,SM.AdvancePlanningDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails MPD where MPD.Id=APD.Id
 ORDER BY MPD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
SM.StandardScheduleMinutes,'' as PlannedDate,
(CASE WHEN APD.ActualDate IS NULL THEN 0 ELSE 1 END) as [Status],'' as ActualDate,
APD.Remarks
 from TRN.SkillManagement SM
 left join TRN.SkillManagementEntity SPE ON SPE.SMID=SM.Id
 left join TRN.SkillManagementPositionCode SPC ON SPC.SMID=SM.Id
 left Join TRN.EmployeePlannedDetails APD ON APD.PositionCodeId=SPC.Id
 left join EmployeeInformation EI ON EI.EmployeeStatus='Active' 
--and EI.PositionID=SPC.PositionCodeId 
and EI.SystemId='" + EmployeeId + @"'
 left join MST.ManpowerBudget EB ON EB.Id=EI.BudgetCode
 left Join Org.Entity E ON E.Id=SPE.EntityId
 left Join ORG.Position P ON P.Id=EB.PositionID
 left join org.Division DIV ON DIV.Id=p.DivisionId
 left join Org.Department DEP ON DEP.Id=p.DepartmentId
 left join Org.Section S ON S.Id=p.SectionId
 left join Org.SubSection SS ON SS.Id=p.SubSectionId
 left join HKP.Designation DEG ON DEG.Id=EI.GivenDesignationId
 where SM.IsActive=1 and SPC.Id is not null and SPE.Id='" + EntityId + "' and SM.Id='" + SMId + @"'
and APD.Id = (select top 1 Id from TRN.EmployeePlannedDetails PD
 where PD.PositionCodeId = '" +PositionId+"' and PD.EmployeeId = '" + EmployeeId + "' and PD.EntityId='" + EntityId + "' order by PD.PlannedDate desc)";
 //           and Case when isnull((SELECT TOP 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id ORDER BY APD.Id DESC),'')= ''
 //then GETDATE() else (SM.ScheduleDays + (select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id
 //ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + @"'
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult LoadSkillManagementPendingdScheduleList(string ToDate, string FromDate, string MaintenanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,APD.Id,SM.Id as SMId,SPC.Id as PositionCodeId,EI.SystemId as EmployeeId,EI.EmployeeName,
P.Code as PositionCode,DIV.UserName Division,DEP.UserName EmpDepartment,S.UserName Section,SS.UserName SubSection,EB.Code as BudgetCode,P.Activity,DEG.UserName Designation,SM.ScheduleDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails where Id='" + MaintenanceId + @"'
 ORDER BY Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where Id='" + MaintenanceId + @"'
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where Id='" + MaintenanceId + @"'
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((SM.ScheduleDays+(select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId=SPC.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
SM.StandardScheduleMinutes,Format(APD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,isnull(APD.[Status],1) as [Status],APD.Remarks,
Format(APD.FromDate,'dd-MMM-yyyy') as FromDate,Format(APD.ActualDate,'dd-MMM-yyyy') as ActualDate,format(APD.FromTime,'hh:mm tt') as FromTime,format(APD.ToTime,'hh:mm tt') as	ToTime,APD.Minute as [Minute],APD.ActualRemark,
APD.FileName,'id' as test,APD.Grade,APD.GradeRemark
 from TRN.SkillManagement SM
 --left join TRN.SkillManagementEntity SPE ON SPE.SMID=SM.Id
 left join TRN.SkillManagementPositionCode SPC ON SPC.SMID=SM.Id
 left Join TRN.EmployeePlannedDetails APD ON APD.PositionCodeId=SPC.Id
 left join EmployeeInformation EI ON EI.EmployeeStatus='Active' 
--and EI.PositionID=SPC.PositionCodeId 
and EI.SystemId=APD.EmployeeId
 left join MST.ManpowerBudget EB ON EB.Id=EI.BudgetCode
 --left Join Org.Entity E ON E.Id=SPE.EntityId
 left Join ORG.Position P ON P.Id=EB.PositionID
 left join org.Division DIV ON DIV.Id=p.DivisionId
 left join Org.Department DEP ON DEP.Id=p.DepartmentId
 left join Org.Section S ON S.Id=p.SectionId
 left join Org.SubSection SS ON SS.Id=p.SubSectionId
 left join HKP.Designation DEG ON DEG.Id=EI.GivenDesignationId
 where SM.IsActive=1 and SPC.Id is not null and APD.Id='" + MaintenanceId + @"'";
            //and Case when isnull((SELECT TOP 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id ORDER BY APD.Id DESC),'')= ''
            //then GETDATE() else (SM.ScheduleDays + (select top 1 ActualDate from TRN.EmployeePlannedDetails APD where APD.PositionCodeId = SPC.Id
            //ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + "'
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadReponsiblePersonList(string Id, string SMId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select 
RPD.IsActive,RPD.Id,RPD.PlanMinutes,RPD.ActualMinutes,
EI.SystemId as ResponsiblePersonId,EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection,'No' EmployeeFlag  
from TRN.SkillManagement SM
LEFT JOIN TRN.SkillManagementPersonBudgetCode PBC ON PBC.SMID=SM.Id
LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=PBC.PersonBudgetCodeId
LEFT JOIN dbo.EmployeeInformation AS EI ON EI.BudgetCode=PBC.PersonBudgetCodeId
LEFT JOIN [TRN].[SkillResponsiblePlannedDetails] RPD ON RPD.ResponsiblePersonId=EI.SystemId and RPD.IsActive=1 and RPD.PlannedId='" + Id + @"'
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
LEFT OUTER JOIN org.Position P ON P.Id=MB.PositionID
LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
WHERE EI.EmployeeStatus='Active' and SM.Id='" + SMId + @"' 
union 
select RPD.IsActive,RPD.Id,RPD.PlanMinutes,RPD.ActualMinutes,
EI.SystemId as ResponsiblePersonId,EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS[LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode, P.Code PCode, S.UserName as Section,SS.UserName as SubSection,'Yes' EmployeeFlag 
from TRN.TeamDefinitionEmployee TDE
LEFT JOIN TRN.SkillManagementTeamDefinition MTD ON MTD.TeamDefinitionId = TDE.TeamDefinitionId
LEFT JOIN dbo.EmployeeInformation AS EI ON EI.SystemId = TDE.EmployeeId
LEFT JOIN [TRN].[SkillResponsiblePlannedDetails] RPD ON RPD.ResponsiblePersonId = EI.SystemId and RPD.IsActive = 1 and RPD.PlannedId='" + Id + @"'
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id = EI.LegalDesignationId
LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = EI.BudgetCode
LEFT JOIN ORG.Entity AS EN ON EN.Id = MB.EntityId
LEFT OUTER JOIN org.Position P ON P.Id = mb.PositionID
LEFT JOIN ORG.Department AS DEP ON DEP.Id = p.DepartmentId
LEFT OUTER JOIN ORG.Section S ON S.Id = p.SectionId
LEFT OUTER JOIN ORG.SubSection SS ON SS.Id = EI.SubSectionId WHERE EI.EmployeeStatus='Active' and MTD.SMID='" + SMId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadItemPerformanceList(string Id, string SMId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"Select IPD.IsActive,IPD.Id,SMI.Id as ItemId,SMI.ItemName,SMI.PerformanceGroupId,isnull(IPD.PerformancePoints,'') as PerformancePoints,isnull(IPD.PerformanceComments,'') as PerformanceComments,SMI.Remarks,EPD.Id as PlannedId,null as PerformancePointsList,SMI.MaximumPoints,SMI.MinimumPoints from TRN.SkillManagementItem SMI
--left join TRN.SkillManagementLevel SML ON SML.Id=SMI.PerformanceGroupId
left join TRN.SkillManagement SM ON SM.Id=SMI.SMID
left join TRN.SkillManagementPositionCode SPC ON SPC.SMID=SM.Id
left join TRN.EmployeePlannedDetails EPD ON EPD.PositionCodeId=SPC.Id 
LEFT JOIN [TRN].[SkillItemPerformanceDetails] IPD ON  IPD.ItemId=SMI.Id and IPD.PlannedId='" + Id + @"'
where EPD.Id='"+ Id + "' order by SMI.Id"; 

 return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createPlanned(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "TRN.EmployeePlannedDetails";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        if (!string.IsNullOrEmpty(item["PlannedDate"].ToString()))
                        {
                            int PlanningDays = Convert.ToInt32(item["AdvancePlanningDays"]);
                            DateTime PlanDate = Convert.ToDateTime(item["PlannedDate"]);
                            DateTime NextDayDate = DateTime.Now.AddDays(PlanningDays);

                            if (PlanDate <= NextDayDate)
                            {
                                objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and PositionCodeId='" + item["PositionCodeId"] + "' and EmployeeId = '" + item["EmployeeId"] + "'", out dsProdBooked, false, "1");
                               
                                DataView dv = new DataView(dsProdBooked.Tables[0]);

                                if (dv.Count == 0)
                                {
                                        bplib.clsGenID genid = new bplib.clsGenID();
                                        genid.GenID(TableName, out _Id);
                                        item["Id"] = "EPD" + _Id;
                                        AddNewRow(dsProdBooked.Tables[0], item);
                                }
                                else
                                {
                                    DataRow drpb = dv[0].Row;
                                    EditRow(drpb, item);
                                }
                                clsStaticInfo obj = new clsStaticInfo();
                                obj.SaveDataSets(dsProdBooked);
                            }
                            else
                            {
                                throw new CustomException("Plan Date Should Not Exceed More than " + PlanningDays + " Days of Today's Date!");
                            }
                        }
                        else
                        {
                            throw new CustomException("Please Enter Plan Date and Proceed!");
                        }

                    }
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult createResponsible(List<Dictionary<string, object>> DataList, string PId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[SkillResponsiblePlannedDetails]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (DataList != null)
                {
                    foreach (var item in DataList)
                    {
                        if (item["PlanMinutes"].IsNotNull() && Convert.ToInt32(item["PlanMinutes"]) != 0)
                        {
                            objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                            DataView dv = new DataView(dsProdBooked.Tables[0]);

                            if (dv.Count == 0)
                            {
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID(TableName, out _Id);
                                item["Id"] = "SPD" + _Id;
                                item["PlannedId"] = PId;
                                AddNewRow(dsProdBooked.Tables[0], item);
                            }
                            else
                            {
                                item["PlannedId"] = PId;
                                DataRow drpb = dv[0].Row;
                                EditRow(drpb, item);
                            }

                            clsStaticInfo obj = new clsStaticInfo();
                            obj.SaveDataSets(dsProdBooked);
                        }
                        else
                        {
                            throw new CustomException("Please enter Plan Minutes greater than 0 and proceed!");
                        }
                    }
                    return Json(new { Message = AplosMessage.Insert });
                }
                else
                {
                    throw new CustomException("Please select atleast one actionable person and proceed!");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
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

        [HttpGet, Authorize]
        public ActionResult GetSkillManagementJobCardPlanReportView(string PlannedId)
        {
            try
            {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    IWorkbook workbook = _AttendanceManagementService.GetSkillManagementJobCardReports(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, PlannedId);
                    var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                    return RenderReportAsPdf(workbook, reportFileName);
               
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetSkillManagementJobCardReportView(string PlannedId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = _AttendanceManagementService.GetSkillManagementJobCardReports(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, PlannedId);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
            return RenderReportAsPdf(workbook, reportFileName);
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        public ActionResult RenderReportAsPdf(IWorkbook workbook, string fileName, bool isOpen = true)
        {
            try
            {
                using (var converter = new ExcelToPdfConverter(workbook))
                {
                    var pdfDocument = new PdfDocument();
                    ExcelToPdfConverterSettings _settings = new ExcelToPdfConverterSettings();
                    _settings.AutoDetectComplexScript = true;
                    _settings.EmbedFonts = true;
                    _settings.LayoutOptions = LayoutOptions.FitAllColumnsOnOnePage;

                    pdfDocument = converter.Convert(_settings);

                    if (isOpen == true)
                        pdfDocument.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Open);
                    else
                        pdfDocument.Save(fileName + ".pdf", HttpContext.ApplicationInstance.Response, HttpReadType.Save);

                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion -- Operations
    }
}