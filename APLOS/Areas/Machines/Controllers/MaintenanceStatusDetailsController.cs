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

        [Authorize, HttpGet]
        public ActionResult LoadMaintenanceStatusPlannedList(string ToDate,string MaintenanceId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select CAST (CASE WHEN APD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,APD.Id,MS.Id as MaintenanceSchedulingId,MMA.Id as AssetId,MA.AssetName,MA.AssetCode,
WC.UserName WorkCenter,MS.ScheduleDays,MS.LastMaintenanceDate,
Case when isnull(MS.LastMaintenanceDate,'')='' then convert(varchar(20),GETDATE(),103) else convert(varchar(20),(MS.ScheduleDays+GETDATE()),103) end CurrentMaintanceDate,
Case when isnull(MS.LastMaintenanceDate,'')='' then DATEDIFF(day, GETDATE(), GETDATE()) else DATEDIFF(day, GETDATE(), (MS.ScheduleDays+GETDATE())) end DueDays,
Case when isnull(MS.LastMaintenanceDate,'')='' then Case when GETDATE()<GETDATE() then 1 else 0 end else Case when(MS.ScheduleDays+GETDATE())<GETDATE() then 1 else 0 end end OverDue,
Case when isnull(MS.LastMaintenanceDate,'')='' then Case when GETDATE()=GETDATE() then 1 else 0 end else Case when(MS.ScheduleDays+GETDATE())=GETDATE() then 1 else 0 end end DueToday,
Case when isnull(MS.LastMaintenanceDate,'')='' then Case when (GETDATE()>GETDATE() and GETDATE()!=GETDATE()) then 1 else 0 end else Case when ((MS.ScheduleDays+GETDATE())>GETDATE() and (MS.ScheduleDays+GETDATE())!=GETDATE()) then 1 else 0 end end FutureDue,
MS.StandardScheduleMinutes,APD.PlannedDate,isnull(APD.[Status],1) as [Status],APD.ActualDate,APD.Remarks
 from TRN.Maintenancescheduling MS
 left Join MST.MachineMaster MM ON MM.id=MS.MachineMasterId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id
 left Join [TRN].[MachineAssetPlannedDetails] APD ON APD.AssetId=MMA.Id
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 where MMA.Id is not null and MS.Id='" + MaintenanceId + "' and (Case when isnull(MS.LastMaintenanceDate,'')='' then convert(varchar(20),GETDATE(),106) else convert(varchar(20),(MS.ScheduleDays+GETDATE()),106) end)=(select replace('" + ToDate + "','-',' '))";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult LoadReponsiblePersonList(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"SELECT CAST (CASE WHEN RPD.Id IS NULL THEN 0 ELSE 1 END AS bit) Flag,RPD.Id,EI.SystemId as ResponsiblePersonId,EI.SystemId as SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName as EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [LegalDesignation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType,MB.Code MBCode,P.Code PCode,S.UserName as Section,SS.UserName as SubSection
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN [TRN].[ResponsiblePlannedDetails] RPD ON RPD.ResponsiblePersonId=EI.SystemId and RPD.PlannedId='"+Id+ @"'
                            LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=EI.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
							LEFT OUTER JOIN org.Position P ON P.Id=ei.PositionID
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            LEFT OUTER JOIN ORG.Section S ON S.Id=EI.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=EI.SubSectionId
                            WHERE EI.EmployeeStatus='Active'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
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
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "APD" + _Id;
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
                }
                return Json(new { Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [Authorize, HttpPost]
        public ActionResult createResponsible(List<Dictionary<string, object>> DataList,string PId)
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
                }
                return Json(new { Message = AplosMessage.Insert });

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
        #endregion -- Operations
    }
}