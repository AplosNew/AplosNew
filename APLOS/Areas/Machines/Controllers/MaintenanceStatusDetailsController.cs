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

namespace Aplos.Areas.Machines.Controllers
{
    public class MaintenanceStatusDetailsController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public MaintenanceStatusDetailsController(ISqlRepository R)
        {
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
        public ActionResult LoadMaintenanceStatusDetailsList(string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MS.Id,E.UserName Entity,MS.UserName ScheduleName,MM.UserName MachineName,MM.MachineMake Make,
MM.MachineModel Model,MS.ScheduleCode,MB.Code ResponsiblePersonBudgetCode,MA.AssetName,MA.AssetCode,
WC.UserName WorkCenter,MS.ScheduleDays,MS.LastMaintenanceDate,
Case when isnull(MS.LastMaintenanceDate,'')='' then convert(varchar(20),GETDATE(),103) else convert(varchar(20),(MS.ScheduleDays+GETDATE()),103) end CurrentMaintanceDate,
Case when isnull(MS.LastMaintenanceDate,'')='' then DATEDIFF(day, GETDATE(), GETDATE()) else DATEDIFF(day, GETDATE(), (MS.ScheduleDays+GETDATE())) end DueDays,
Case when isnull(MS.LastMaintenanceDate,'')='' then Case when GETDATE()<GETDATE() then 1 else 0 end else Case when(MS.ScheduleDays+GETDATE())<GETDATE() then 1 else 0 end end OverDue,
Case when isnull(MS.LastMaintenanceDate,'')='' then Case when GETDATE()=GETDATE() then 1 else 0 end else Case when(MS.ScheduleDays+GETDATE())=GETDATE() then 1 else 0 end end DueToday,
Case when isnull(MS.LastMaintenanceDate,'')='' then Case when (GETDATE()>GETDATE() and GETDATE()!=GETDATE()) then 1 else 0 end else Case when ((MS.ScheduleDays+GETDATE())>GETDATE() and (MS.ScheduleDays+GETDATE())!=GETDATE()) then 1 else 0 end end FutureDue,
MS.StandardScheduleMinutes,MS.Remarks
 from TRN.Maintenancescheduling MS
 left Join MST.MachineMaster MM ON MM.id=MS.MachineMasterId
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 where MMA.Id is not null 
 and (Case when isnull(MS.LastMaintenanceDate,'')='' then convert(varchar(20),GETDATE(),106) else convert(varchar(20),(MS.ScheduleDays+GETDATE()),106) end)=(select replace('"+ ToDate + "','-',' '))";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadMaintenanceStatusSummaryList(string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MS.Id,E.UserName Entity,MS.UserName ScheduleName,MM.UserName MachineName,MM.MachineMake Make,
MM.MachineModel Model,MS.ScheduleCode,MB.Code ResponsiblePersonBudgetCode,
count(MMA.Id) NoOfAsset,
sum(Case when isnull(MS.LastMaintenanceDate,'')='' then Case when GETDATE()<GETDATE() then 1 else 0 end else Case when(MS.ScheduleDays+GETDATE())<GETDATE() then 1 else 0 end end) OverDue,
sum(Case when isnull(MS.LastMaintenanceDate,'')='' then Case when GETDATE()=GETDATE() then 1 else 0 end else Case when(MS.ScheduleDays+GETDATE())=GETDATE() then 1 else 0 end end) DueToday,
sum(Case when isnull(MS.LastMaintenanceDate,'')='' then Case when (GETDATE()>GETDATE() and GETDATE()!=GETDATE()) then 1 else 0 end else Case when ((MS.ScheduleDays+GETDATE())>GETDATE() and (MS.ScheduleDays+GETDATE())!=GETDATE()) then 1 else 0 end end) FutureDue,
MS.Remarks
 from TRN.Maintenancescheduling MS
 left Join MST.MachineMaster MM ON MM.id=MS.MachineMasterId
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 where MMA.Id is not null 
 and (Case when isnull(MS.LastMaintenanceDate,'')='' then convert(varchar(20),GETDATE(),106) else convert(varchar(20),(MS.ScheduleDays+GETDATE()),106) end)=(select replace('" + ToDate + "','-',' ')) group by MS.Id,E.UserName,MS.UserName,MM.UserName,MM.MachineMake,MM.MachineModel,MS.ScheduleCode,MB.Code,MS.LastMaintenanceDate,MS.ScheduleDays,MS.Remarks"; 
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}