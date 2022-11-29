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

namespace Aplos.Areas.Machines.Controllers
{
    public class PendingMaintenanceScheduleController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public PendingMaintenanceScheduleController(ISqlRepository R)
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
        public ActionResult LoadMaintenanceStatusDetailsList(string ToDate,string FromDate,string Status)
        {
            string Filter = string.Empty;
            //if (Status == "All")
            //{
            //    Filter = " and Status in (0,1)";
            //}
            //else if (Status == "Completed")
            //{
            //    Filter = " and Status in (1)";
            //}
            //else
            //{
            //    Filter = " and Status in (0)";
            //}
            if (Status == "All")
            {
                Filter = " and (MPD.ActualDate is not null or MPD.ActualDate is null) and MPD.PlannedDate is not null";
            }
            else if (Status == "Completed")
            {
                Filter = " and MPD.ActualDate is not null and MPD.PlannedDate is not null";
            }
            else
            {
                Filter = " and MPD.ActualDate is null and MPD.PlannedDate is not null";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MS.Id,Format(MPD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,MPD.Id as PlannedId,MMA.EntityId,E.UserName Entity,MS.UserName ScheduleName,MM.UserName MachineName,MM.MachineMake Make,
MM.MachineModel Model,MS.ScheduleCode,MS.ResponsiblePersoneBgtCodeId,MB.Code ResponsiblePersonBudgetCode,MMA.AssetId,MA.AssetName,MA.AssetCode,MA.AssetReference,
MMA.WorkCenterMasterId,WC.UserName WorkCenter,MS.ScheduleDays,
 isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end)<GETDATE() then 1 else 0 end OverDue,
 case when (Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end)=GETDATE() then 1 else 0 end DueToday,
 case when (case when (Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end)<GETDATE() then 1 else 0 end) = 0 and (case when (Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end)=GETDATE() then 1 else 0 end)=0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,MS.Remarks,(select D.UserName Department from Org.Department D where D.Id=MS.DepartmentId) as Department,MS.MaintenanceGroup
,EI.EmployeeName as ActionableResponsiblePerson,RP.ResponsiblePersonId
from TRN.Maintenancescheduling MS
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 left join TRN.MachineAssetPlannedDetails MPD ON MPD.AssetId=MMA.Id
 left join TRN.ResponsiblePlannedDetails RP ON RP.PlannedId=MPD.Id and RP.IsActive=1
 left Join EmployeeInformation EI ON EI.SystemId=RP.ResponsiblePersonId
 where MMA.Id is not null 
 and Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)) end between '" + FromDate + "' and '" + ToDate + "' " + Filter + @" order by MPD.PlannedDate";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult LoadPendingMaintenanceSchedule(string ActResponsiblePerson, string todate,string fromdate, string Status)
        {
            string Filter = string.Empty;
            string Responsible = string.Empty;
            if(ActResponsiblePerson==null)
            {
                Responsible = "";
            }
            else
            {
                Responsible = "and (select top 1 ResponsiblePersonId from TRN.ResponsiblePlannedDetails RP where RP.PlannedId=MPD.Id and RP.IsActive=1 and ResponsiblePersonId='" + ActResponsiblePerson + "') = '" + ActResponsiblePerson + "'";
            }
            
            if (Status == "All")
            {
                Filter = " and (MPD.ActualDate is not null or MPD.ActualDate is null) and MPD.PlannedDate is not null";
            }
            else if (Status == "Completed")
            {
                Filter = " and MPD.ActualDate is not null and MPD.PlannedDate is not null";
            }
            else
            {
                Filter = " and MPD.ActualDate is null and MPD.PlannedDate is not null";
            }
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select MS.Id,Format(MPD.PlannedDate,'dd-MMM-yyyy') as PlannedDate,MPD.Id as PlannedId,MMA.EntityId,E.UserName Entity,MS.UserName ScheduleName,MM.UserName MachineName,MM.MachineMake Make,
MM.MachineModel Model,MS.ScheduleCode,MS.ResponsiblePersoneBgtCodeId,MB.Code ResponsiblePersonBudgetCode,MMA.AssetId,MA.AssetName,MA.AssetCode,MA.AssetReference,
MMA.WorkCenterMasterId,WC.UserName WorkCenter,MS.ScheduleDays,MMA.Id as MachineAssetId,
 isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'') as LastMaintenanceDate,
Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy') end CurrentMaintanceDate,
DateDiff(day,GETDATE(),Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end) DueDays,
 case when (Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end)<GETDATE() then 1 else 0 end OverDue,
 case when (Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end)=GETDATE() then 1 else 0 end DueToday,
 case when (case when (Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end)<GETDATE() then 1 else 0 end) = 0 and (case when (Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then format(GETDATE(),'dd-MMM-yyyy') else format((MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)),'dd-MMM-yyyy')end)=GETDATE() then 1 else 0 end)=0 then 1 else 0 end FutureDue,
MS.StandardScheduleMinutes,MS.Remarks,(select D.UserName Department from Org.Department D where D.Id=MS.DepartmentId) as Department,MS.MaintenanceGroup,MPD.FileName,'Pid' as test,
  Reverse(stuff(Reverse((select EmployeeName+',' from EmployeeInformation where SystemId in (select ResponsiblePersonId from TRN.ResponsiblePlannedDetails AP where AP.PlannedId=MPD.Id and AP.IsActive=1) for xml path(''))),1,1,'')) ActionableResponsiblePerson
 from TRN.Maintenancescheduling MS
 left join MST.ManpowerBudget MB ON MB.id=MS.ResponsiblePersoneBgtCodeId
 left join TRN.MaintenanceMachineAsset MMA ON MMA.MaintenanceSchedulingId=MS.Id and MMA.IsActive=1
 left join MachineMasterAsset MA ON MA.Id=MMA.AssetId
 left join MST.MachineMaster MM  ON MM.Id=MA.MachineMasterId
 left join ORG.Entity E ON E.Id=MMA.EntityId
 left join SCS.WorkCenterMaster WC ON WC.Id=MMA.WorkCenterMasterId
 left join TRN.MachineAssetPlannedDetails MPD ON MPD.AssetId=MMA.Id
 --left join TRN.ResponsiblePlannedDetails RP ON RP.PlannedId=MPD.Id and RP.IsActive=1
 --left Join EmployeeInformation EI ON EI.SystemId=RP.ResponsiblePersonId
 where 
 Case when isnull((SELECT TOP 1 format(ActualDate,'dd-MMM-yyyy') from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC),'')='' then GETDATE() else (MS.ScheduleDays+(select top 1 ActualDate from [TRN].[MachineAssetPlannedDetails] APD where APD.AssetId=MMA.Id
 ORDER BY APD.Id DESC)) end between '" + fromdate + "' and '" + todate + "' " + Filter + @" " + Responsible + @"  order by MPD.PlannedDate";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
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
                        objCon.OpenDataSetThroughAdapter("select * from [TRN].[MachineAssetPlannedDetails] where ActualDate is not null and Id='" + item["Id"] + "'", out DataSet dsMachineAssetPlannedDetailsValidation, false, "1");
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            DateTime FromDt = Convert.ToDateTime(item["FromDate"]);
                            DateTime ToDt = Convert.ToDateTime(item["ActualDate"]);
                            TimeSpan t = ToDt.Subtract(FromDt);
                            int N = t.Days;
                            DateTime date1 = Convert.ToDateTime(item["FromTime"]);
                            DateTime date2 = Convert.ToDateTime(item["ToTime"]);
                            DateTime NextDayDate = date2.AddDays(N);
                            TimeSpan ts = date2 - date1;
                            TimeSpan Nd = NextDayDate - date1;
                            int minutes = (int)Nd.TotalMinutes;

                            if (minutes >= 720 || minutes < 0)
                            {
                                item["ToTime"] = NextDayDate;
                                item["Minute"] = Nd.TotalMinutes;
                            }
                            else
                            {
                                item["ToTime"] = date2;
                                item["Minute"] = ts.TotalMinutes;
                            }

                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "APD" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                           
                        }
                        else
                        {
                            DateTime ActualDate = Convert.ToDateTime(item["ActualDate"]);
                            DateTime LastDayDate = DateTime.Today.AddDays(-1);
                            if (dsMachineAssetPlannedDetailsValidation.Tables[0].Rows.Count > 0)
                            {
                                if (ActualDate == DateTime.Today || ActualDate == LastDayDate)
                                {
                                    DataRow drpb = dv[0].Row;
                                    DateTime FromDt = Convert.ToDateTime(item["FromDate"]);
                                    DateTime ToDt = Convert.ToDateTime(item["ActualDate"]);
                                    TimeSpan t = ToDt.Subtract(FromDt);
                                    int N = t.Days;
                                    DateTime date1 = Convert.ToDateTime(item["FromTime"]);
                                    DateTime date2 = Convert.ToDateTime(item["ToTime"]);
                                    DateTime NextDayDate = date2.AddDays(N);
                                    TimeSpan ts = date2 - date1;
                                    TimeSpan Nd = NextDayDate - date1;
                                    int minutes = (int)Nd.TotalMinutes;

                                    if (minutes >= 720 || minutes < 0)
                                    {
                                        item["ToTime"] = NextDayDate;
                                        item["Minute"] = Nd.TotalMinutes;
                                    }
                                    else
                                    {
                                        item["ToTime"] = date2;
                                        item["Minute"] = ts.TotalMinutes;
                                    }
                                    EditRow(drpb, item);
                                }
                                else
                                {
                                    throw new CustomException("Actual date should be today's date or yesterday's date only!");
                                }
                            }
                            else
                            {
                                if(ActualDate > DateTime.Today)
                                { 
                                throw new Exception("Actual date cannot be greater than today's date!");
                                }
                                else
                                {
                                    DataRow drpb = dv[0].Row;
                                    DateTime FromDt = Convert.ToDateTime(item["FromDate"]);
                                    DateTime ToDt = Convert.ToDateTime(item["ActualDate"]);
                                    TimeSpan t = ToDt.Subtract(FromDt);
                                    int N = t.Days;
                                    DateTime date1 = Convert.ToDateTime(item["FromTime"]);
                                    DateTime date2 = Convert.ToDateTime(item["ToTime"]);
                                    DateTime NextDayDate = date2.AddDays(N);
                                    TimeSpan ts = date2 - date1;
                                    TimeSpan Nd = NextDayDate - date1;
                                    int minutes = (int)Nd.TotalMinutes;

                                    if (minutes >= 720 || minutes < 0)
                                    {
                                        item["ToTime"] = NextDayDate;
                                        item["Minute"] = Nd.TotalMinutes;
                                    }
                                    else
                                    {
                                        item["ToTime"] = date2;
                                        item["Minute"] = ts.TotalMinutes;
                                    }
                                    EditRow(drpb, item);
                                }
                            }
                            
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

        [HttpPost, Authorize]
        public ActionResult SaveDefault(IEnumerable<System.Web.HttpPostedFileBase> UploadDefault, string UploadDefault_data)
        {
            try
            {
                UploadDefault_data = UploadDefault_data.Replace("\"", "");
                if (string.IsNullOrEmpty(UploadDefault_data))
                    throw new Exception("Save the order first");

                foreach (var file in UploadDefault)
                {

                    var fileName = Path.GetFileName(UploadDefault_data + new FileInfo(file.FileName).Extension);
                    var fileN = file.FileName;
                    var destinationPath = Path.Combine(ResourcesPathReader.GetMSADocumentPath(), fileName);

                    var directory = ResourcesPathReader.GetMSADocumentPath();
                    var path = Path.Combine(directory);

                    if (System.IO.Directory.Exists(ResourcesPathReader.GetMSADocumentPath()) == false)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(ResourcesPathReader.GetMSADocumentPath());
                        }
                        catch (Exception)
                        {

                        }
                    }


                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    string sql = "SELECT * FROM [TRN].[MachineAssetPlannedDetails] WHERE Id='" + UploadDefault_data + "'";
                    DataSet dsLocal = null;
                    connection.BeginTransaction();
                    connection.getDataSet(sql, out dsLocal);
                    connection.CommitTransaction();
                    var FN = dsLocal.Tables[0].Rows[0]["FileName"].ToString();
                    if (fileN != FN)
                        if (System.IO.File.Exists(path + UploadDefault_data + Path.GetExtension(FN)))
                            System.IO.File.Delete(path + UploadDefault_data + Path.GetExtension(FN));

                    if (dsLocal.Tables[0].Rows.Count > 0)
                    {
                        dsLocal.Tables[0].Rows[0].BeginEdit();

                        dsLocal.Tables[0].Rows[0]["FileName"] = fileN;

                        dsLocal.Tables[0].Rows[0].EndEdit();

                        file.SaveAs(destinationPath);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsLocal);



                    }
                }
                return Content("");
            }
            catch (Exception ex)
            {
                HttpResponse Response = System.Web.HttpContext.Current.Response;
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.Status = "204 No Content";
                Response.StatusDescription = ex.Message;
                Response.End();

                return Content("");
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetFileInfo(string Id)
        {
            try
            {
                return Json(_sqlRepository.GetDataCollection("select * from [TRN].[MachineAssetPlannedDetails]  where Id='" + Id + "'"), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion -- Operations
    }
}