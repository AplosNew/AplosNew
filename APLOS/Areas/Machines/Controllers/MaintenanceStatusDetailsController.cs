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
    public class MaintenanceStatusDetailsController : Controller
    {
        #region Constructor


        private readonly IAttendanceManagementService _AttendanceManagementService;
        ResudeceStatusReportService rsr = new ResudeceStatusReportService();
        private readonly ISqlRepository _sqlRepository;

        public MaintenanceStatusDetailsController(IAttendanceManagementService AttendanceManagementService, ISqlRepository R)
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
        public JsonResult GetFromDateList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select top 1 format(MPD.ActualDate+MS.ScheduleDays,'dd-MMM-yyyy') FromDate from  [TRN].[MachineAssetPlannedDetails] MPD
 left join TRN.MaintenanceMachineAsset MMA ON MMA.Id=MPD.AssetId
 left join TRN.Maintenancescheduling MS ON MMA.MaintenanceSchedulingId=MS.Id
where MPD.ActualDate is not null  
order by MPD.ActualDate asc";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetActionablePersonList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @" select distinct EI.EmployeeName as Text,RP.ResponsiblePersonId as Value
 from [TRN].[MachineAssetPlannedDetails] MPD
 left join TRN.ResponsiblePlannedDetails RP ON RP.PlannedId=MPD.Id
 left join EmployeeInformation EI ON EI.SystemId=RP.ResponsiblePersonId
 where (ActualDate is not null or ActualDate is null) and RP.ResponsiblePersonId is not null and RP.IsActive=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public ActionResult LoadMaintenanceStatusDetailsList(string ToDate, string FromDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MS.Id,E.UserName Entity,MS.UserName ScheduleName,MS.AdvancePlanningDays,MA.MachineMasterId,MM.UserName MachineName,MM.MachineMake Make,
MM.MachineModel Model,MS.ScheduleCode,MB.Code ResponsiblePersonBudgetCode,MMA.Id as AssetId,MA.AssetName,MA.AssetCode,MA.AssetReference,
WC.UserName WorkCenter,MS.ScheduleDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,MS.Remarks,(select D.UserName Department from Org.Department D where D.Id=MS.DepartmentId) as Department,MS.MaintenanceGroup,'Status Details' as SD
 ,Format(MPD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,MPD.Id as PlannedId
from TRN.Maintenancescheduling MS
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 left join TRN.MachineAssetPlannedDetails MPD ON MPD.Id=(select top 1 Id from [TRN].[MachineAssetPlannedDetails] MAPD where MAPD.AssetId=MMA.Id order by MAPD.ActualDate desc)
 where MS.IsActive=1 and MMA.Id is not null 
 and  Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (MPD.ActualDate) end between '" + FromDate + "' and '" + ToDate + @"' 
 and (select count(Id) from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id and APD.PlannedDate is not null and APD.ActualDate is null) = 0 
 order by Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id ORDER BY APD.Id DESC),'')= '' then GETDATE() else (MS.ScheduleDays + (select top 1 ActualDate from[TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id ORDER BY APD.Id DESC)) end";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMaintenanceStatusSummaryList(string ToDate, string FromDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select X.Id,X.EntityId,X.Entity,X.ScheduleName,X.MachineId,X.MachineName,X.Make,X.Model,X.ScheduleCode,X.ResponsiblePersonBudgetCode,
count(X.NoOfAsset) as NoOfAsset,sum(X.OverDue) as OverDue,sum(X.DueToday) as DueToday,sum(X.FutureDue) as FutureDue,X.Remarks,X.PlanStatus,X.Department,X.MaintenanceGroup from (
select MS.Id,E.Id as EntityId,E.UserName Entity,MS.UserName ScheduleName,MM.UserName MachineName,MM.Id MachineId,MM.MachineMake Make,
MM.MachineModel Model,MS.ScheduleCode,MB.Code ResponsiblePersonBudgetCode,count(MMA.Id) NoOfAsset,
MS.ScheduleDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.Remarks,(select count(MPD.Id) from [TRN].[MachineAssetPlannedDetails] MPD where MPD.PlannedDate is null) as PlanStatus,
(select D.UserName Department from Org.Department D where D.Id=MS.DepartmentId) as Department,MS.MaintenanceGroup
 from TRN.Maintenancescheduling MS
 --left Join MST.MachineMaster MM ON MM.id=MS.MachineMasterId
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 left join TRN.MachineAssetPlannedDetails MPD ON MPD.Id=(select top 1 Id from [TRN].[MachineAssetPlannedDetails] MAPD where MAPD.AssetId=MMA.Id order by MAPD.ActualDate desc)
 where MS.IsActive=1 and MMA.Id is not null and Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.Id=MPD.Id
 ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + "' and (select count(Id) from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id and APD.PlannedDate is not null and APD.ActualDate is null) = 0 group by MS.Id,MM.Id,MMA.Id,MPD.Id,E.Id,E.UserName,MS.UserName,MM.UserName,MM.MachineMake,MM.MachineModel,MS.ScheduleCode,MB.Code,MS.LastMaintenanceDate,MS.ScheduleDays,MS.Remarks,MS.DepartmentId,MS.MaintenanceGroup) X group by NoOfAsset,Id,MachineId,EntityId,Entity,ScheduleName,MachineName,Make,Model,ScheduleCode,ResponsiblePersonBudgetCode,Remarks,PlanStatus,Department,MaintenanceGroup";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMaintenanceStatusPlannedList(string ToDate, string FromDate, string MaintenanceId, string MachineId, string EntityId, string Value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,'' Id,(select top 1 Id from TRN.MachineAssetPlannedDetails where AssetId=MMA.Id order by PlannedDate desc) as PlannedId,MS.Id as MaintenanceSchedulingId,MMA.Id as AssetId,MA.AssetName,MA.AssetCode,MA.AssetReference,
WC.UserName WorkCenter,MS.ScheduleDays,MS.AdvancePlanningDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] MPD where MPD.Id=APD.Id
 ORDER BY MPD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,
--Format(APD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,
'' as PlanDate,
(CASE WHEN APD.ActualDate IS NULL THEN 0 ELSE 1 END) as [Status],
'' as ActualDate,
APD.Remarks
 from TRN.Maintenancescheduling MS
 --left Join MST.MachineMaster MM ON MM.id=MS.MachineMasterId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left Join [TRN].[MachineAssetPlannedDetails] APD ON APD.AssetId=MMA.Id 
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 where MS.IsActive=1 and MMA.Id is not null and MA.MachineMasterId='" + MachineId + "' and MA.EntityId='" + EntityId + "' and MS.Id='" + MaintenanceId + "' and Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id ORDER BY APD.Id DESC),'')= '' then GETDATE() else (MS.ScheduleDays + (select top 1 ActualDate from[TRN].[MachineAssetPlannedDetails] APD where APD.AssetId = MMA.Id ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + "' and 1='" + Value + "'  and (select count(Id) from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId = MMA.Id and APD.PlannedDate is not null and APD.ActualDate is null) = 0";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMaintenanceStatusPlannedListDetails(string ToDate, string FromDate, string MaintenanceId, string MachineId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,APD.Id,(select top 1 Id from TRN.MachineAssetPlannedDetails where AssetId=MMA.Id order by PlannedDate desc) as PlannedId,MS.Id as MaintenanceSchedulingId,MMA.Id as AssetId,MA.AssetName,MA.AssetCode,MA.AssetReference,
WC.UserName WorkCenter,MS.ScheduleDays,MS.AdvancePlanningDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] MPD where MPD.Id=APD.Id
 ORDER BY MPD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,'' as PlannedDate,
--Format(APD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,
(CASE WHEN APD.ActualDate IS NULL THEN 0 ELSE 1 END) as [Status],'' as ActualDate,
--Format(APD.ActualDate,'dd-MMM-yyyy') as ActualDate,
APD.Remarks
 from TRN.Maintenancescheduling MS
 --left Join MST.MachineMaster MM ON MM.id=MS.MachineMasterId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left Join [TRN].[MachineAssetPlannedDetails] APD ON APD.AssetId=MMA.Id and APD.ActualDate is null
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 where MS.IsActive=1 and MMA.Id is not null and MA.MachineMasterId='" + MachineId + "' and MS.Id='" + MaintenanceId + "' and Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id ORDER BY APD.Id DESC),'')= '' then GETDATE() else (MS.ScheduleDays + (select top 1 ActualDate from[TRN].[MachineAssetPlannedDetails] APD where APD.AssetId = MMA.Id ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMaintenanceStatusPlannedListGetDetails(string ToDate, string FromDate, string MaintenanceId, string MachineId, string AssetId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,'' Id,(select top 1 Id from TRN.MachineAssetPlannedDetails where AssetId=MMA.Id order by PlannedDate desc) as PlannedId,MS.Id as MaintenanceSchedulingId,MMA.Id as AssetId,MA.AssetName,MA.AssetCode,MA.AssetReference,
WC.UserName WorkCenter,MS.ScheduleDays,MS.AdvancePlanningDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] MPD where MPD.Id=APD.Id
 ORDER BY MPD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((APD.ActualDate),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+APD.ActualDate),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,'' as PlannedDate,
--Format(APD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,
(CASE WHEN APD.ActualDate IS NULL THEN 0 ELSE 1 END) as [Status],'' as ActualDate,
--Format(APD.ActualDate,'dd-MMM-yyyy') as ActualDate,
APD.Remarks
 from TRN.Maintenancescheduling MS
 --left Join MST.MachineMaster MM ON MM.id=MS.MachineMasterId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left Join [TRN].[MachineAssetPlannedDetails] APD ON APD.AssetId=MMA.Id and APD.Id=(select top 1 Id from [TRN].[MachineAssetPlannedDetails] MAPD where MAPD.AssetId=MMA.Id order by MAPD.ActualDate desc)
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 where MS.IsActive=1 and MMA.Id is not null and MA.MachineMasterId='" + MachineId + "' and MS.Id='" + MaintenanceId + "' and Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id ORDER BY APD.Id DESC),'')= '' then GETDATE() else (MS.ScheduleDays + (select top 1 ActualDate from[TRN].[MachineAssetPlannedDetails] APD where APD.AssetId = MMA.Id ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + "' and MMA.Id='" + AssetId + "' and (select count(Id) from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id and APD.PlannedDate is not null and APD.ActualDate is null) = 0";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMaintenanceStatusPlannedListGetPlanDetails(string ToDate, string FromDate, string MaintenanceId, string MachineId, string AssetId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,APD.Id,(select top 1 Id from TRN.MachineAssetPlannedDetails where AssetId=MMA.Id order by PlannedDate desc) as PlannedId,MS.Id as MaintenanceSchedulingId,MMA.Id as AssetId,MA.AssetName,MA.AssetCode,MA.AssetReference,
WC.UserName WorkCenter,MS.ScheduleDays,MS.AdvancePlanningDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] MPD where MPD.Id=APD.Id
 ORDER BY MPD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,'' as PlannedDate,
--Format(APD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,
(CASE WHEN APD.ActualDate IS NULL THEN 0 ELSE 1 END) as [Status],'' as ActualDate,
--Format(APD.ActualDate,'dd-MMM-yyyy') as ActualDate,
APD.Remarks
 from TRN.Maintenancescheduling MS
 --left Join MST.MachineMaster MM ON MM.id=MS.MachineMasterId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left Join [TRN].[MachineAssetPlannedDetails] APD ON APD.AssetId=MMA.Id 
--and APD.Id=(select top 1 Id from [TRN].[MachineAssetPlannedDetails] MAPD where MAPD.AssetId=MMA.Id order by MAPD.ActualDate desc)
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 where MS.IsActive=1 and MMA.Id is not null and MA.MachineMasterId='" + MachineId + "' and MS.Id='" + MaintenanceId + "' and Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id ORDER BY APD.Id DESC),'')= '' then GETDATE() else (MS.ScheduleDays + (select top 1 ActualDate from[TRN].[MachineAssetPlannedDetails] APD where APD.AssetId = MMA.Id ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + "' and APD.Id=(select top 1 Id from [TRN].[MachineAssetPlannedDetails] PD where PD.AssetId='" + AssetId + "' order by PD.PlannedDate desc)";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpGet]
        public ActionResult LoadMaintenancePendingdScheduleList(string ToDate, string FromDate, string MaintenanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,APD.Id,MS.Id as MaintenanceSchedulingId,MMA.Id as AssetId,MA.AssetName,MA.AssetCode,
WC.UserName WorkCenter,MS.ScheduleDays,isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] where Id='" + MaintenanceId + @"'
 ORDER BY Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where Id='" + MaintenanceId + @"'
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where Id='" + MaintenanceId + @"'
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))<0 then 1 else 0 end OverDue,
  case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))=0 then 1 else 0 end DueToday,
 case when (DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end))>0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,Format(APD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,isnull(APD.[Status],1) as [Status],APD.Remarks,
Format(APD.FromDate,'dd-MMM-yyyy') as FromDate,Format(APD.ActualDate,'dd-MMM-yyyy') as ActualDate,format(APD.FromTime,'hh:mm tt') as FromTime,format(APD.ToTime,'hh:mm tt') as	ToTime,APD.Minute as [Minute],APD.ActualRemark,
APD.FileName,'id' as test
 from TRN.Maintenancescheduling MS
 --left Join MST.MachineMaster MM ON MM.id=MS.MachineMasterId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left Join [TRN].[MachineAssetPlannedDetails] APD ON APD.AssetId=MMA.Id
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 where MS.IsActive=1 and MMA.Id is not null and APD.Id='" + MaintenanceId + "' and Case when isnull((SELECT TOP 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id and APD.Id='" + MaintenanceId + "' ORDER BY APD.Id DESC),'')= '' then GETDATE() else (APD.ActualDate) end between '" + FromDate + "' and '" + ToDate + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadReponsiblePersonList(string Id, string MaintenanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select 
RPD.IsActive,RPD.Id,RPD.PlanMinutes,RPD.ActualMinutes,
EI.SystemId as ResponsiblePersonId,EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection,'No' EmployeeFlag  
from TRN.Maintenancescheduling MS
LEFT JOIN TRN.MaintenancePersonBudgetCode PBC ON PBC.MaintenanceSchedulingId=MS.Id
LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=PBC.PersonBudgetCodeId
LEFT JOIN dbo.EmployeeInformation AS EI ON EI.BudgetCode=PBC.PersonBudgetCodeId
LEFT JOIN [TRN].[ResponsiblePlannedDetails] RPD ON RPD.ResponsiblePersonId=EI.SystemId and RPD.IsActive=1 and RPD.PlannedId='" + Id + @"'
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
LEFT OUTER JOIN org.Position P ON P.Id=mb.PositionID
LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
LEFT JOIN ORG.Department AS DEP ON DEP.Id=p.DepartmentId
LEFT OUTER JOIN ORG.Section S ON S.Id=p.SectionId
LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=p.SubSectionId
WHERE EI.EmployeeStatus='Active' and MS.Id='" + MaintenanceId + @"' 
union 
select RPD.IsActive,RPD.Id,RPD.PlanMinutes,RPD.ActualMinutes,
EI.SystemId as ResponsiblePersonId,EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS[LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode, P.Code PCode, S.UserName as Section,SS.UserName as SubSection,'Yes' EmployeeFlag 
from TRN.TeamDefinitionEmployee TDE
LEFT JOIN TRN.MaintenanceTeamDefinition MTD ON MTD.TeamDefinitionId = TDE.TeamDefinitionId
LEFT JOIN dbo.EmployeeInformation AS EI ON EI.SystemId = TDE.EmployeeId
LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = EI.BudgetCode
LEFT JOIN ORG.Entity AS EN ON EN.Id = MB.EntityId
LEFT OUTER JOIN org.Position P ON P.Id = mb.PositionID
LEFT JOIN [TRN].[ResponsiblePlannedDetails] RPD ON RPD.ResponsiblePersonId = EI.SystemId and RPD.IsActive = 1 and RPD.PlannedId='" + Id + @"'
LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id = EI.LegalDesignationId
LEFT JOIN ORG.Department AS DEP ON DEP.Id = p.DepartmentId
LEFT OUTER JOIN ORG.Section S ON S.Id = p.SectionId
LEFT OUTER JOIN ORG.SubSection SS ON SS.Id = p.SubSectionId WHERE EI.EmployeeStatus='Active' and MTD.MaintenanceSchedulingId='" + MaintenanceId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult createPlanned(List<Dictionary<string, object>> DataList)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[MachineAssetPlannedDetails]";
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
                                objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and AssetId='" + item["AssetId"] + "'", out dsProdBooked, false, "1");
                                objCon.OpenDataSetThroughAdapter("select * from " + TableName + " where AssetId='" + item["AssetId"] + "'", out DataSet dsMaintenancePlanAssetValidation, false, "1");
                                DataView dv = new DataView(dsProdBooked.Tables[0]);

                                if (dv.Count == 0)
                                {
                                    //if (dsMaintenancePlanAssetValidation.Tables[0].Rows.Count > 0)
                                    //{
                                    //    throw new Exception("This Machine Asset is already plan");
                                    //}
                                    //else
                                    //{
                                        bplib.clsGenID genid = new bplib.clsGenID();
                                        genid.GenID(TableName, out _Id);
                                        item["Id"] = "APD" + _Id;
                                        AddNewRow(dsProdBooked.Tables[0], item);
                                    //}

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
            string TableName = "[TRN].[ResponsiblePlannedDetails]";
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
                                item["Id"] = "RPD" + _Id;
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
        public ActionResult GetMaintenanceJobCardPlanReportView(string PlannedId)
        {
            try
            {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    IWorkbook workbook = _AttendanceManagementService.GetMaintenanceJobCardReports(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, PlannedId);
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
        public ActionResult GetMaintenanceJobCardReportView(string PlannedId)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {
                //    objCon = new ConnectionManager.DAL.ConManager("1");
                //    objCon.OpenDataSetThroughAdapter("select * from [TRN].[ResponsiblePlannedDetails] where PlannedId='" + PlannedId + "'", out DataSet dsResponsibleValidation, false, "1");
                //    if (dsResponsibleValidation.Tables[0].Rows.Count > 0)
                //    {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _AttendanceManagementService.GetMaintenanceJobCardReports(identity.Name, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, PlannedId);
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Job Card Report";
                return RenderReportAsPdf(workbook, reportFileName);
                //}
                //else
                //{
                //    throw new CustomException("Please Add Actionable Person and Proceed!");
                //}

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


        [Authorize, HttpPost]
        public ActionResult XlsMaintenanceStatusSummary(string todate, string fromDate)
        {
            try
            {
                var workbook = MaintenanceStatusSummaryReport(todate, fromDate);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "MaintenanceStatusSummary.xlsx";
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
        private IWorkbook MaintenanceStatusSummaryReport(string todate, string fromDate)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = rsr.MaintenanceStatusSummaryReport(todate, fromDate);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Maintenance Status Summary Report";

            int ROW = 1;
            int endCol = 1;
            int COL = 1;

            int COLHeader = 0;

            report.SetHeaderText(ref sheet, ROW, COLHeader + 6, "Maintenance Status Summary Report :", 20, ExcelHAlign.HAlignCenter);
            sheet.Range[ROW, COLHeader + 6, ROW, COLHeader + 7].Merge();
            ROW++;
            #region Grid Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 12, ExcelHAlign.HAlignCenter);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Schedule Name", 12, ExcelHAlign.HAlignCenter);
            int ColScheduleName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Machine Name", 12, ExcelHAlign.HAlignCenter);
            int ColMachineName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Make", 12, ExcelHAlign.HAlignCenter);
            int ColMake = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Model", 12, ExcelHAlign.HAlignCenter);
            int ColModel = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Schedule Code", 15, ExcelHAlign.HAlignCenter);
            int ColScheduleCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person BudgetCode", 15, ExcelHAlign.HAlignCenter);
            int ColResponsiblePersonBudgetCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "No Of Asset", 12, ExcelHAlign.HAlignCenter);
            int ColNoOfAsset = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Over Due", 12, ExcelHAlign.HAlignCenter);
            int ColOverDue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Due Today", 12, ExcelHAlign.HAlignCenter);
            int ColDueToday = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Future Due", 12, ExcelHAlign.HAlignCenter);
            int ColFutureDue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plan Status", 12, ExcelHAlign.HAlignCenter);
            int ColPlanStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 12, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 12, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Maintenance Group", 12, ExcelHAlign.HAlignCenter);
            int ColMaintenanceGroup = COL;

            ROW++;
            endCol = COL;
            #endregion Headers

            string MaintenanceEntity = "";
            string MaintenanceScheduleName = "";

            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            int MaintenanceEntityRow = 0;
            int MaintenanceScheduleNameRow = 0;


            double[] arr = new double[4];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                if (MaintenanceEntity != data.Rows[i]["Entity"].ToString())
                {
                    MaintenanceEntity = data.Rows[i]["Entity"].ToString();

                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();

                    if (i != 0 && MaintenanceEntityRow != (ROW - 1))
                    {
                        sheet.Range[MaintenanceEntityRow, ColEntity, ROW - 1, ColEntity].Merge();
                        sheet.Range[MaintenanceEntityRow, ColEntity, ROW - 1, ColEntity].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    MaintenanceEntityRow = ROW;
                }

                if (MaintenanceScheduleName != data.Rows[i]["ScheduleName"].ToString())
                {
                    MaintenanceScheduleName = data.Rows[i]["ScheduleName"].ToString();
                    sheet[ROW, ColScheduleName].Text = data.Rows[i]["ScheduleName"].ToString();

                    if (i != 0 && MaintenanceScheduleNameRow != (ROW - 1))
                    {
                        sheet.Range[MaintenanceScheduleNameRow, ColScheduleName, ROW - 1, ColScheduleName].Merge();
                        sheet.Range[MaintenanceScheduleNameRow, ColScheduleName, ROW - 1, ColScheduleName].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    MaintenanceScheduleNameRow = ROW;
                }

                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColScheduleName].Text = data.Rows[i]["ScheduleName"].ToString();

                sheet[ROW, ColMachineName].Text = data.Rows[i]["MachineName"].ToString();
                sheet[ROW, ColMake].Text = data.Rows[i]["Make"].ToString();
                sheet[ROW, ColModel].Text = data.Rows[i]["Model"].ToString();
                sheet[ROW, ColScheduleCode].Text = data.Rows[i]["ScheduleCode"].ToString();

                sheet[ROW, ColResponsiblePersonBudgetCode].Number = clsStaticInfo.dbl(data.Rows[i]["ResponsiblePersonBudgetCode"].ToString());
                sheet[ROW, ColNoOfAsset].Number = clsStaticInfo.dbl(data.Rows[i]["NoOfAsset"].ToString());
                sheet[ROW, ColOverDue].Number = clsStaticInfo.dbl(data.Rows[i]["OverDue"].ToString());
                sheet[ROW, ColDueToday].Number = clsStaticInfo.dbl(data.Rows[i]["DueToday"].ToString());
                sheet[ROW, ColFutureDue].Number = clsStaticInfo.dbl(data.Rows[i]["FutureDue"].ToString());
                sheet[ROW, ColPlanStatus].Number = clsStaticInfo.dbl(data.Rows[i]["PlanStatus"].ToString());
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColMaintenanceGroup].Text = data.Rows[i]["MaintenanceGroup"].ToString();


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


        [Authorize, HttpPost]
        public ActionResult XlsMaintenanceStatusDetails(string todate, string fromDate)
        {
            try
            {
                var workbook = MaintenanceStatusDetailsReport(todate, fromDate);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "MaintenanceStatusDetails.xlsx";
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
        private IWorkbook MaintenanceStatusDetailsReport(string todate, string fromDate)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;


            var data = rsr.MaintenanceStatusDetailsReport(todate, fromDate);


            var sheet = workbook.Worksheets[0];


            #region sheet1
            sheet.Name = "Maintenance Status Details Report";



            int ROW = 1;
            int endCol = 1;
            int COL = 1;
            int COLHeader = 0;

            report.SetHeaderText(ref sheet, ROW, COLHeader + 6, "Maintenance Status Details Report :", 15, ExcelHAlign.HAlignCenter);
            sheet.Range[ROW, COLHeader + 6, ROW, COLHeader + 7].Merge();
            ROW++;

            #region Grid Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Entity", 12, ExcelHAlign.HAlignCenter);
            int ColEntity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Schedule Name", 12, ExcelHAlign.HAlignCenter);
            int ColScheduleName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Machine Name", 12, ExcelHAlign.HAlignCenter);
            int ColMachineName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Make", 12, ExcelHAlign.HAlignCenter);
            int ColMake = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Model", 12, ExcelHAlign.HAlignCenter);
            int ColModel = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Schedule Code", 15, ExcelHAlign.HAlignCenter);
            int ColScheduleCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person BudgetCode", 15, ExcelHAlign.HAlignCenter);
            int ColResponsiblePersonBudgetCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Asset Name", 12, ExcelHAlign.HAlignCenter);
            int ColAssetName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Asset Code", 12, ExcelHAlign.HAlignCenter);
            int ColAssetCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Work Center", 12, ExcelHAlign.HAlignCenter);
            int ColWorkCenter = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Schedule Days", 12, ExcelHAlign.HAlignCenter);
            int ColScheduleDays = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Planned Date", 12, ExcelHAlign.HAlignCenter);
            int ColPlannedDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "LM.Date", 12, ExcelHAlign.HAlignCenter);
            int ColLastMaintenanceDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "CM.Date", 12, ExcelHAlign.HAlignCenter);
            int ColCurrentMaintanceDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Due Days", 12, ExcelHAlign.HAlignCenter);
            int ColDueDays = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Over Due", 12, ExcelHAlign.HAlignCenter);
            int ColOverDue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Due Today", 12, ExcelHAlign.HAlignCenter);
            int ColDueToday = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Future Due", 12, ExcelHAlign.HAlignCenter);
            int ColFutureDue = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Standard Schedule Minutes", 12, ExcelHAlign.HAlignCenter);
            int ColStandardScheduleMinutes = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 12, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Department", 12, ExcelHAlign.HAlignCenter);
            int ColDepartment = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Maintenance Group", 12, ExcelHAlign.HAlignCenter);
            int ColMaintenanceGroup = COL;

            ROW++;
            endCol = COL;
            #endregion Headers

            string MaintenanceEntity = "";
            string MaintenanceScheduleName = "";

            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            int MaintenanceEntityRow = 0;
            int MaintenanceScheduleNameRow = 0;


            double[] arr = new double[4];

            for (int i = 0; i < data.Rows.Count; i++)
            {
                if (MaintenanceEntity != data.Rows[i]["Entity"].ToString())
                {
                    MaintenanceEntity = data.Rows[i]["Entity"].ToString();

                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();

                    if (i != 0 && MaintenanceEntityRow != (ROW - 1))
                    {
                        sheet.Range[MaintenanceEntityRow, ColEntity, ROW - 1, ColEntity].Merge();
                        sheet.Range[MaintenanceEntityRow, ColEntity, ROW - 1, ColEntity].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    MaintenanceEntityRow = ROW;
                }

                if (MaintenanceScheduleName != data.Rows[i]["ScheduleName"].ToString())
                {
                    MaintenanceScheduleName = data.Rows[i]["ScheduleName"].ToString();
                    sheet[ROW, ColScheduleName].Text = data.Rows[i]["ScheduleName"].ToString();

                    if (i != 0 && MaintenanceScheduleNameRow != (ROW - 1))
                    {
                        sheet.Range[MaintenanceScheduleNameRow, ColScheduleName, ROW - 1, ColScheduleName].Merge();
                        sheet.Range[MaintenanceScheduleNameRow, ColScheduleName, ROW - 1, ColScheduleName].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    }
                    MaintenanceScheduleNameRow = ROW;
                }

                sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                sheet[ROW, ColScheduleName].Text = data.Rows[i]["ScheduleName"].ToString();

                sheet[ROW, ColMachineName].Text = data.Rows[i]["MachineName"].ToString();
                sheet[ROW, ColMake].Text = data.Rows[i]["Make"].ToString();
                sheet[ROW, ColModel].Text = data.Rows[i]["Model"].ToString();
                sheet[ROW, ColScheduleCode].Text = data.Rows[i]["ScheduleCode"].ToString();

                sheet[ROW, ColResponsiblePersonBudgetCode].Number = clsStaticInfo.dbl(data.Rows[i]["ResponsiblePersonBudgetCode"].ToString());
                sheet[ROW, ColAssetName].Text = data.Rows[i]["AssetName"].ToString();
                sheet[ROW, ColAssetCode].Text = data.Rows[i]["AssetCode"].ToString();
                sheet[ROW, ColWorkCenter].Text = data.Rows[i]["WorkCenter"].ToString();
                sheet[ROW, ColScheduleDays].Number = clsStaticInfo.dbl(data.Rows[i]["ScheduleDays"].ToString());
                sheet[ROW, ColPlannedDate].Text = data.Rows[i]["PlannedDate"].ToString();
                sheet[ROW, ColLastMaintenanceDate].Text = data.Rows[i]["LastMaintenanceDate"].ToString();
                sheet[ROW, ColCurrentMaintanceDate].Text = data.Rows[i]["CurrentMaintanceDate"].ToString();
                sheet[ROW, ColDueDays].Number = clsStaticInfo.dbl(data.Rows[i]["DueDays"].ToString());
                sheet[ROW, ColOverDue].Number = clsStaticInfo.dbl(data.Rows[i]["OverDue"].ToString());
                sheet[ROW, ColDueToday].Number = clsStaticInfo.dbl(data.Rows[i]["DueToday"].ToString());
                sheet[ROW, ColFutureDue].Number = clsStaticInfo.dbl(data.Rows[i]["FutureDue"].ToString());
                sheet[ROW, ColStandardScheduleMinutes].Number = clsStaticInfo.dbl(data.Rows[i]["StandardScheduleMinutes"].ToString());
                sheet[ROW, ColRemarks].Text = data.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColDepartment].Text = data.Rows[i]["Department"].ToString();
                sheet[ROW, ColMaintenanceGroup].Text = data.Rows[i]["MaintenanceGroup"].ToString();


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
        #endregion -- Operations
    }
}