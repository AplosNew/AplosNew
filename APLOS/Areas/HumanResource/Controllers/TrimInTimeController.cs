using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Attendance.Manual;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class TrimInTimeController : BaseController
    {
        //getAttendanceData,SaveRandomTime
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public TrimInTimeController(ISqlRepository R)
        {

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
        public ActionResult getAllEmployees(string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TimeSpan ts = Convert.ToDateTime(todate).Subtract(Convert.ToDateTime(fromdate));
            if (Math.Abs(ts.TotalDays) > 31)
                return Json(new { Error = true, Message = "Timespan between from and to date cannot be greater than 31 days" }, JsonRequestBehavior.AllowGet);

            string sql = @"
                        SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            EMP.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            INNER JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
    
                        WHERE emp.PlantId='" + identity.PlantId + @"' AND o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'
      
                    ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }


        [HttpPost]
        public ActionResult getAttendanceData(string employeeid, string shiftsystemid, string fromdate, string todate, int minutes)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = stringAttendanceData(employeeid, shiftsystemid, fromdate, todate, minutes);

            List<AttendanceProcessData> _data = _sqlRepository.GetModelCollection<AttendanceProcessData>(sql);
            DataTable dt = _sqlRepository.GetDataTable(@"SELECT * FROM ShiftDefination AS sd WHERE sd.SystemID='" + shiftsystemid + "'");
            string message = "";
            if (dt.Rows.Count > 0)
            {
                DateTime dtStartTime = Convert.ToDateTime(fromdate);
                dtStartTime = dtStartTime.AddHours(Convert.ToDateTime(dt.Rows[0]["InTime"].ToString()).Hour);
                dtStartTime = dtStartTime.AddMinutes(Convert.ToDateTime(dt.Rows[0]["InTime"].ToString()).Minute);
                dtStartTime = dtStartTime.AddSeconds(Convert.ToDateTime(dt.Rows[0]["InTime"].ToString()).Second);

                DateTime dtStartTimeBefore = dtStartTime.AddMinutes(minutes * -1);



                message = "**Trimming will take place between " + dtStartTimeBefore.ToString("dd-MMM-yyyy hh:mm tt") + " and " + dtStartTime.ToString("dd-MMM-yyyy hh:mm tt");
            }

            return Json(new { data = _data, note = message }, JsonRequestBehavior.AllowGet);

        }


        [HttpPost, Authorize]
        public ActionResult getShift()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT * FROM ShiftDefination AS sd WHERE sd.PlantID='" + identity.PlantId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

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
        [HttpPost, Authorize]
        public ActionResult getShiftDefinition(string systemid, string WorkDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string sql = @"SELECT 
                            sd.SystemID,
                            sd.InTimeStartMargin, sd.IsActive, sd.DefaultShift, sd.SequenceNo, 
                            sd.UserName AS ShiftName,
                            format(kk.ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
                            format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime

						
                            FROM (
                            SELECT 
                            sd.SystemID,
                            DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),'" + WorkDate + @"'))  AS ShiftInTime,
                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),'" + WorkDate + @"'))  AS ShiftOutTime

		
                            FROM ShiftDefination sd
                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '" + WorkDate + @"' BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                            ) AS KK
                            INNER JOIN   ShiftDefination sd ON sd.SystemID=kk.SystemID
                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '" + WorkDate + @"' BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                            WHERE sd.systemid='" + systemid + @"'
                            ORDER BY sd.SequenceNo ASC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        private string stringAttendanceData(string employeeid, string shiftsystemid, string fromdate, string todate, int minutes)
        {

            todate = fromdate;

            return @" SELECT 
                            0 as Active,kk.Id,kk.EmployeeCode,
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


                            KK.IsManualOutTime,

                            format(KK.PunchInTime,'dd-MMM-yyyy hh:mm tt') AS PunchInTime,
                            format(KK.PunchOutTime,'dd-MMM-yyyy hh:mm tt') AS PunchOutTime,

                            KK.DayStatus, KK.OTHr,
                            KK.IsOTComfirm, KK.IsOTEntitled

                             FROM (
								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, 
       
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,
		                            O.IsOTEntitled

		                            FROM EmployeeInformation EMP
		                            INNER JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID
                                    INNER JOIN DayType AS dt ON dt.DayType=o.DayStatus AND dt.[Category] IN ('Late','Present')
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                       
                            WHERE o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"' and o.ShiftSystemID='" + shiftsystemid + @"'
                        ) AS KK
                        LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=kk.ShiftSystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON kk.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
						    LEFT OUTER JOIN EmployeeInformation EMP ON KK.Id=EMP.SystemID
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id		
                        where convert(datetime,kk.InTime)<dateadd(minute," + (minutes * -1).ToString() + @",ShiftInTime)
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, WorkDate) ASC ";



        }

        [HttpPost]
        public ActionResult SaveRandomTime(List<string> employeelist, string shiftsystemid, string fromdate, int minutes)
        {
            try
            {
                if (employeelist == null)
                    throw new Exception("Select at least one employee");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                DataTable dtLock = _sqlRepository.GetDataTable("SELECT * FROM PlantWiseAttendanceLock AS pwal WHERE pwal.LockedDate='" + fromdate + "' AND pwal.PlantId='" + identity.PlantId + "'");
                if (dtLock.Rows.Count > 0)
                {
                    return Json(new { Error = true, Message = "Day locked for effective date" }, JsonRequestBehavior.AllowGet);
                }


                string sql = stringAttendanceData("", shiftsystemid, fromdate, fromdate, minutes);
                DataTable dtOriginalData = _sqlRepository.GetDataTable(sql);

                string ids = "''";
                foreach (string item in employeelist)
                    ids += ",'" + item + "'";


                DataSet dsToSave;

                string sql2 = @"SELECT * FROM AttdnProcessData WHERE EmpSystemID IN (" + ids + @") AND WorkDate='" + fromdate + "'";
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.getDataSet(sql2, out dsToSave);
                connection.CommitTransaction();


                DataSet dsManualAttendance;
                connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.getDataSet(@"SELECT * FROM AttdnManualData AS SA WHERE SA.EmpSystemID IN (" + ids + ") AND sa.WorkDate = '" + fromdate + "'", out dsManualAttendance);
                connection.CommitTransaction();


                Random rndDuration = new Random((int)DateTime.Now.Ticks);
                for (int i = 0; i < dtOriginalData.Rows.Count; i++)
                {
                    dsToSave.Tables[0].DefaultView.RowFilter = "EmpSystemID='" + dtOriginalData.Rows[i]["Id"].ToString() + "'";
                    if (dsToSave.Tables[0].DefaultView.Count == 0)
                        continue;

                    DateTime shiftStartTime = Convert.ToDateTime(dtOriginalData.Rows[i]["ShiftInTime"].ToString());
                    shiftStartTime = shiftStartTime.AddMinutes(((int)rndDuration.Next(0, minutes - 1)) * -1);
                    shiftStartTime = shiftStartTime.AddSeconds(((int)rndDuration.Next(0, 59)) * -1);


                    DataRow dr = dsToSave.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();

                    dr["InTime"] = shiftStartTime;
                    dr["IsManualInTime"] = true;

                    dr.EndEdit();

                    dsManualAttendance.Tables[0].DefaultView.RowFilter = "EmpSystemID='" + dtOriginalData.Rows[i]["Id"].ToString() + "'";
                    if (dsManualAttendance.Tables[0].DefaultView.Count > 0)
                    {

                        DataRow drManual = dsManualAttendance.Tables[0].DefaultView[0].Row;

                        drManual.BeginEdit();


                        drManual["InTime"] = shiftStartTime;

                        drManual["UpdatedBy"] = identity.Name;
                        drManual["DateUpdated"] = System.DateTime.Now;


                        drManual.EndEdit();
                    }
                    else
                    {

                        DataRow drManual = dsManualAttendance.Tables[0].NewRow();

                        drManual["EmpSystemID"] = dtOriginalData.Rows[i]["Id"].ToString();
                        drManual["WorkDate"] = dtOriginalData.Rows[i]["WorkDate"].ToString();
                        drManual["GroupID"] = identity.CompanyGroupId;
                        drManual["PlantID"] = identity.PlantId;

                        drManual["InTime"] = shiftStartTime;


                        drManual["UpdatedBy"] = identity.Name;
                        drManual["DateUpdated"] = System.DateTime.Now;
                        drManual["AddedBy"] = identity.Name;
                        drManual["DateAdded"] = System.DateTime.Now;

                        dsManualAttendance.Tables[0].Rows.Add(drManual);



                    }
                }
                dsToSave.Tables[0].DefaultView.RowFilter = null;
                dsManualAttendance.Tables[0].DefaultView.RowFilter = null;

                SaveDataSets(dsToSave, dsManualAttendance);

                return Json(new { Error = false, Message = "Time updated successfully" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }


        public void SaveDataSets(params System.Data.DataSet[] dsRef)
        {
            clsConnection objCon = null;
            try
            {
                objCon = new clsConnection();
                objCon.BeginTransaction();
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveData(ref dsRef[i]);
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }

        } // End Function
    }
}