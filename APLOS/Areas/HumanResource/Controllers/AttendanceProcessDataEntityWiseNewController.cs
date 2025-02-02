#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Linq;
using clsAttendance;
using Library.HumanResource.Attendance.Manual;
using SetINOUT;
using Library.Service.Attendances;
using Library.HumanResource.NewAttendanceProcess;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class AttendanceProcessDataEntityWiseNewController : BaseController
    {


        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public AttendanceProcessDataEntityWiseNewController(IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion Constructor
        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages
        [HttpPost, Authorize]
        public JsonResult GetEntity()
        {
            try
            {

                string sql = @"";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (identity.IsSysAdmin)
                {
                    sql = @"SELECT distinct E.* FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            WHERE E.PlantId='" + identity.PlantId + @"' AND E.[Active]=1 ORDER BY E.Code";
                    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                }

                sql = @"SELECT distinct e2.* FROM [SEC].[UserEntity] E
                        LEFT JOIN org.Entity AS e2 ON e2.Id=e.EntityId
                        LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E2.Id
                        WHERE E.UserId='" + identity.UserId + @"' AND e.PlantId='" + identity.PlantId + "' AND E2.[Active]=1 ORDER BY E2.Code";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


                throw new Exception("No entity configurations was found in the system for the current user");
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost, Authorize]
        public ActionResult getAllEmployees(string fromdate, string todate, string entityids)
        {
            entityids = "'" + entityids + "'";
            entityids = entityids.Replace(",", "','");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TimeSpan ts = Convert.ToDateTime(todate).Subtract(Convert.ToDateTime(fromdate));
            if (Math.Abs(ts.TotalDays) > 31)
                return Json(new { Error = true, Message = "Timespan between from and to date cannot be greater than 31 days" }, JsonRequestBehavior.AllowGet);

            string sql = @"
                        SELECT distinct Emp.SystemID AS Id,E.UserName AS Entity,
                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            INNER JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
    
                        WHERE E.Id in (" + entityids + ") AND emp.PlantId='" + identity.PlantId + @"' AND o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'
      
                    ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost]
        public ActionResult getAttendanceData(string employeeid, string fromdate, string todate, string entityids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = stringAttendanceData(employeeid, fromdate, todate, entityids);


            string shiftSQL = @" SELECT * FROM ShiftDefination AS sd  WHERE sd.IsActive=1 and sd.PlantID='" + identity.PlantId + @"'";


            var jsondata = Json(new { data = _sqlRepository.GetModelCollection<AttendanceProcessNewProcess>(sql), shift = _sqlRepository.GetDataCollection(shiftSQL) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult getShift(string systemid, string WorkDate)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ManualAttndFromAppService mau = new ManualAttndFromAppService(identity, _sqlRepository);

                return Json(mau.GetShiftData(systemid, WorkDate), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        
        [HttpPost, Authorize]
        public ActionResult getAttendance(string empsystemid, string WorkDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string sql = @"SELECT 
                            FORMAT(pdate,'dd-MMM-yyyy') AS PDate,FORMAT(ptime,'hh:mm:ss tt') AS PTime,PType

                             FROM AttdnRawData WHERE LogDownLoadNum='" + empsystemid + @"' AND PDate BETWEEN DATEADD(DAY,-1,'" + WorkDate + @"') AND DATEADD(DAY,1,'" + WorkDate + @"')

                            ORDER BY AttdnRawData.PDate,AttdnRawData.PTime ASC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult SaveSingleEmployee(List<AttendanceProcessNewProcess> data , string Remarks)
        {
            try
            {
                List<AttendanceProcessNewProcess> DataToBeSaved = new List<AttendanceProcessNewProcess>();

                if (data == null)
                    throw new Exception("No new data has been updated");

                for (int i = 0; i < data.Count; i++)
                {
                
                    DataToBeSaved.Add(data[i]);
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                
                DataTable NewShiftStandardTime = getDateWiseShift(DataToBeSaved);
                //validations
                foreach (AttendanceProcessNewProcess item in DataToBeSaved)
                {

                    if (string.IsNullOrEmpty(item.InDate) == false)
                        if (bplib.clsWebLib.IsDateOK(item.InDate) == false)
                            item.ErrorMessage = "Invalid in date";


                    if (string.IsNullOrEmpty(item.OutDate) == false)
                        if (bplib.clsWebLib.IsDateOK(item.OutDate) == false)
                            item.ErrorMessage = "Invalid out date";

                    NewShiftStandardTime.DefaultView.RowFilter = "SystemID='" + item.ShiftSystemID + "' AND WorkDate=#" + item.WorkDate + "#";
                    if (NewShiftStandardTime.DefaultView.Count > 0)
                    {

                        if (item.InTime != null && item.OutTime != null)
                        {
                            if (item.InDate + item.InTime != item.InDateOriginal + item.InTimeOriginal
                                || item.OutDate + item.OutTime != item.OutDateOriginal + item.OutTimeOriginal)
                            {
                                if (Convert.ToDateTime(item.InDate + " " + item.InTime) > Convert.ToDateTime(item.OutDate + " " + item.OutTime))
                                {
                                    item.IsError = true;
                                    item.ErrorMessage = "Out time is earlier than In time";
                                }

                                if (Convert.ToDateTime(item.OutDate + " " + item.OutTime) > DateTime.Now)
                                {
                                    item.IsError = true;
                                    item.ErrorMessage = "Out time is greater than Now";
                                }

                                TimeSpan ts = Convert.ToDateTime(item.OutDate + " " + item.OutTime).Subtract(Convert.ToDateTime(item.InDate + " " + item.InTime));
                                if (Math.Abs(ts.TotalHours) > 24)
                                {
                                    item.IsError = true;
                                    item.ErrorMessage = "Time span cannot be greater than 24 hours between in and out time";
                                }
                            }
                        }

                        item.ShiftHoursWithoutOT = NewShiftStandardTime.DefaultView[0][@"ShiftHoursWithoutOT"].ToString();
                        item.ShiftDuration = NewShiftStandardTime.DefaultView[0][@"ShiftDuration"].ToString();
                        item.ShiftShortDuration = NewShiftStandardTime.DefaultView[0][@"ShiftShortDuration"].ToString();
                        item.ShiftFullDayDuration = NewShiftStandardTime.DefaultView[0][@"ShiftFullDayDuration"].ToString();
                        item.ShiftHalfDayDuration = NewShiftStandardTime.DefaultView[0][@"ShiftHalfDayDuration"].ToString();
                        item.ShiftInTime = NewShiftStandardTime.DefaultView[0][@"ShiftInTime"].ToString();
                        item.ShiftOutTime = NewShiftStandardTime.DefaultView[0][@"ShiftOutTime"].ToString();

                    }

                }

                if (DataToBeSaved.Where(ee => ee.IsError == true).ToList().Count > 0)
                {

                    return Json(new { Error = true, Message = "Error occured", Data = DataToBeSaved }, JsonRequestBehavior.AllowGet);
                }
                //operations
                saveData(DataToBeSaved , Remarks);



                return Json(new { Error = false, Message = "Manual Entry Done Successfully", Data = data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message,
                    Data = data
                }, JsonRequestBehavior.AllowGet);
            }


        }

        private void saveData(List<AttendanceProcessNewProcess> data , string Remarks)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID objId = new bplib.clsGenID();

                clsStaticInfo objStatic = new clsStaticInfo();
                string man = "''";
                NewAttendanceProcessService ap = new NewAttendanceProcessService();

                DataSet dsRem;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter(@"Select * from dbo.ManualEntryRemarks where 1 = 2", out dsRem, false, "1");

                DataSet shiftchange = null;
                for (int i = 0; i < data.Count; i++)
                {
                    con = new ConnectionManager.clsConnection();
                    con.BeginTransaction();
                    con.getDataSet(@"SELECT * FROM AttdnProcessData  WHERE EmpSystemID = '" + data[i].Id + "' AND WorkDate = '" + data[i].WorkDate + "' ", out shiftchange);
                    con.CommitTransaction();

                    int kk = 0;

                    if (data[i].ShiftSystemID != data[i].ShiftSystemIDOriginal)
                    {

                        #region change shift

                        if (shiftchange.Tables[0].Rows.Count > 0)
                        {
                            shiftchange.Tables[0].Rows[0].BeginEdit();
                            shiftchange.Tables[0].Rows[0]["ShiftSystemID"] = data[i].ShiftSystemID;
                            shiftchange.Tables[0].Rows[0]["ManualShiftId"] = data[i].ShiftSystemID;
                            shiftchange.Tables[0].Rows[0]["ShiftDuration"] = data[i].ShiftDuration;
                            shiftchange.Tables[0].Rows[0]["ShiftShortDuration"] = data[i].ShiftShortDuration;
                            shiftchange.Tables[0].Rows[0]["ShiftHoursWithoutOT"] = data[i].ShiftHoursWithoutOT;
                            shiftchange.Tables[0].Rows[0]["ShiftFullDayDuration"] = data[i].ShiftFullDayDuration;
                            shiftchange.Tables[0].Rows[0]["ShiftHalfDayDuration"] = data[i].ShiftHalfDayDuration;
                            shiftchange.Tables[0].Rows[0]["ShiftOutTime"] = data[i].ShiftOutTime;
                            shiftchange.Tables[0].Rows[0]["ShiftInTime"] = data[i].ShiftInTime;
                            shiftchange.Tables[0].Rows[0]["ManualByWhom"] = identity.Name;
                            shiftchange.Tables[0].Rows[0]["ManualEntryTime"] = DateTime.Now;
                            shiftchange.Tables[0].Rows[0]["ManualFlag"] = true;

                            #region OT Columns Nullified
                            shiftchange.Tables[0].Rows[0]["TargetOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["PlanOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AppliedOTLimit"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AllowedOTLimit"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["StandardOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AdditionalOt"] = DBNull.Value;
                            #endregion
                            
                            shiftchange.Tables[0].Rows[0].EndEdit();
                            ap.CheckerFunction(ref man, shiftchange.Tables[0].Rows[0]["RowId"].ToString());
                            kk++;
                        }
                        #endregion change shift
                    }

                    #region In/Out 


                    if (data[i].InDate + data[i].InTime != data[i].InDateOriginal + data[i].InTimeOriginal
                        || data[i].OutDate + data[i].OutTime != data[i].OutDateOriginal + data[i].OutTimeOriginal)
                    {
                        

                        if (data[i].InTime == null && data[i].OutTime == null)
                        {

                          
                        }
                        else
                        {
                            if (shiftchange.Tables[0].Rows.Count > 0)
                            {

                                DataRow dr = shiftchange.Tables[0].Rows[0];

                                dr.BeginEdit();

                                if (data[i].InDate + data[i].InTime != data[i].InDateOriginal + data[i].InTimeOriginal)
                                {
                                    dr["InTime"] = DBNull.Value;
                                    dr["ManualInTime"] = DBNull.Value;
                                    dr["OriginalManualInTime"] = DBNull.Value;
                                    dr["ProcessIntime"] = DBNull.Value;
                                    if (string.IsNullOrEmpty(data[i].InTime) == false)
                                    {
                                        dr["InTime"] = data[i].InDate + " " + data[i].InTime;
                                        dr["ManualInTime"] = data[i].InDate + " " + data[i].InTime;
                                        dr["ProcessIntime"] = data[i].InDate + " " + data[i].InTime;
                                        dr["OriginalManualInTime"] = data[i].InDate + " " + data[i].InTime;
                                        dr["IsManualInTime"] = true;
                                    }
                                }

                                if (data[i].OutDate + data[i].OutTime != data[i].OutDateOriginal + data[i].OutTimeOriginal)
                                {
                                    dr["OutTime"] = DBNull.Value;
                                    dr["ManualOutTime"] = DBNull.Value;
                                    dr["OriginalManualOutTime"] = DBNull.Value;
                                    dr["ProcessOuttime"] = DBNull.Value;
                                    if (string.IsNullOrEmpty(data[i].OutTime) == false)
                                    {
                                        dr["OutTime"] = data[i].OutDate + " " + data[i].OutTime;
                                        dr["ManualOutTime"] = data[i].OutDate + " " + data[i].OutTime;
                                        dr["ProcessOuttime"] = data[i].OutDate + " " + data[i].OutTime;
                                        dr["OriginalManualOutTime"] = data[i].OutDate + " " + data[i].OutTime;
                                        dr["IsManualOutTime"] = true;
                                    }
                                }

                                dr["ManualByWhom"] = identity.Name;
                                dr["ManualEntryTime"] = DateTime.Now;
                                dr["ManualFlag"] = true;
                                dr["OTComfirmBy"] = DBNull.Value;
                                dr["DateOTComfirm"] = DBNull.Value;
                                dr["IsOTComfirm"] = false;

                                #region OT Columns Nullified

                                dr["TargetOT"] = DBNull.Value;
                                dr["PlanOT"] = DBNull.Value;
                                dr["AppliedOTLimit"] = DBNull.Value;
                                dr["AllowedOTLimit"] = DBNull.Value;
                                dr["StandardOT"] = DBNull.Value;
                                dr["AdditionalOt"] = DBNull.Value;

                                #endregion

                                dr.EndEdit();
                                ap.CheckerFunction(ref man, shiftchange.Tables[0].Rows[0]["RowId"].ToString());
                                kk++;
                            }

                        }
                    }
                    #endregion 
                   
                   objStatic.SaveDataSets(shiftchange);

                    string _Id = "";
                    if(kk>0)
                    {

                        DataRow dr = dsRem.Tables[0].NewRow();
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.ManualEntryRemarks", out _Id);
                        dr["Id"] = _Id;
                        dr["RowId"] = shiftchange.Tables[0].Rows[0]["RowId"].ToString();
                        dr["Remarks"] = Remarks;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["Screen"] = "/attendance-process-data-entity-new";
                        dsRem.Tables[0].Rows.Add(dr);

                    }

                }
                clsStaticInfo _infos = new clsStaticInfo();
                _infos.SaveDataSets(dsRem);

                ap.ManualScheduler(identity.PlantId, man);

            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        private DataTable getDateWiseShift(List<AttendanceProcessNewProcess> data)
        {

            string dateString = "";
            for (int i = 0; i < data.Count; i++)
            {
                if (dateString == "")
                    dateString = " select CONVERT(DATETIME,'" + data[i].WorkDate + "') AS WorkDate ";
                else
                    dateString += " UNION select CONVERT(DATETIME,'" + data[i].WorkDate + "') ";

            }

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

           
            string sql = @" SELECT dt.WorkDate,
 
                           sd.SystemID,                            
                            sd.UserName AS ShiftName,
                            format(kk.ShiftInTime,'dd-MMM-yyyy hh:mm:ss tt') AS ShiftInTime,
                            format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime
                            ,kk.ShiftShortDuration,kk.ShiftHalfDayDuration,kk.ShiftHoursWithoutOt,kk.ShiftFullDayDuration,
                            kk.ShiftDuration
                       
                        FROM
                         (" + dateString + @") AS DT
					    LEFT OUTER JOIN
						(
                            SELECT 
                            sd.SystemID,dt.WorkDate,
		                           	DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),dt.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),dt.WorkDate))  AS ShiftOutTime,
                                    isnull(stcm.ShortDuration,sd.ShortDuration) as ShiftShortDuration,
		                            isnull(stcm.HalfDayDuration,sd.HalfDayDuration) as ShiftHalfDayDuration,
						            isnull(stcm.HoursWithoutOT,sd.HoursWithoutOT) as ShiftHoursWithoutOt,
						            isnull(stcm.FullDayDuration,sd.FullDayDuration) as ShiftFullDayDuration,
                                    isnull(stcm.ShiftDuration,sd.ShiftDuration) as ShiftDuration
                            
                             FROM 
                             
                              (" + dateString + @") AS DT
								LEFT OUTER JOIN ShiftDefination sd ON 1=1
								LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON DT.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                            ) AS KK ON dt.WorkDate=kk.WorkDate
                            INNER JOIN   ShiftDefination sd ON sd.SystemID=kk.SystemID
                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON dt.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
							WHERE sd.PlantID='" + identity.PlantId + @"'
                        ORDER BY dt.WorkDate, sd.SequenceNo ASC ";

            return _sqlRepository.GetDataTable(sql);
        }
        
        private string stringAttendanceData(string employeeid, string fromdate, string todate, string entityids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (string.IsNullOrEmpty(entityids) == false)
                entityids = " E.Id IN (" + entityids + ") AND ";
            else
                entityids = "";

            if (string.IsNullOrEmpty(employeeid) == false)
                employeeid = "AND emp.SystemId='" + employeeid + @"'";
            else
            {
                todate = fromdate;
            }
            return @" SELECT 
                            kk.Id,kk.EmployeeCode,E.UserName AS Entity,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,
                            format(KK.WorkDate,'ddd') AS DayName, 
                            format(KK.WorkDate,'dd-MMM-yyyy') AS WorkDate, 

                            KK.ShiftSystemID,kk.ShiftName,KK.ShiftSystemID AS ShiftSystemIDOriginal,
                            format(ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
                     	    format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime,


                            format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDate,format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDateOriginal,
                            format(KK.InTime,'hh:mm tt') AS  InTime, format(KK.InTime,'hh:mm tt') AS  InTimeOriginal, 

                            KK.IsManualInTime, 


						
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDate,
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDateOriginal,
                            format(KK.OutTime,'hh:mm tt') AS  OutTime, format(KK.OutTime,'hh:mm tt') AS  OutTimeOriginal, 


                            KK.IsManualOutTime,KK.DayStatusCode,convert(bit,isnull(KK.IsLock,0)) AS IsLock,

                            format(KK.PunchInTime,'dd-MMM-yyyy hh:mm tt') AS PunchInTime,
                            format(KK.PunchOutTime,'dd-MMM-yyyy hh:mm tt') AS PunchOutTime,

                            KK.DayStatus, KK.OTHr,
                            KK.IsOTComfirm, KK.IsOTEntitled,KK.IsManualDayStatus

                             FROM (
								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime,
       
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,O.DayStatusCode,
		                            O.IsOTEntitled,O.IsManualDayStatus,O.IsLock

		                            FROM EmployeeInformation EMP
		                            LEFT JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
                                    LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                       
                            WHERE " + entityids + " o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'" + employeeid + @"
                        ) AS KK
                        LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=kk.ShiftSystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON kk.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
						    LEFT OUTER JOIN EmployeeInformation EMP ON KK.Id=EMP.SystemID
                             LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id	
                        where emp.plantid='" + identity.PlantId + @"'
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, WorkDate) ASC ";


        }
    }

}