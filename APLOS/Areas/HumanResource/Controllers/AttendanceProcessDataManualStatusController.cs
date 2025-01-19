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
//using clsAttendance;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class AttendanceProcessDataManualStatusController : BaseController
    {


        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public AttendanceProcessDataManualStatusController(IUnitOfWork U, ISqlRepository R)
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

        [HttpPost]
        public ActionResult getAllEmployees(string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TimeSpan ts = Convert.ToDateTime(todate).Subtract(Convert.ToDateTime(fromdate));
            if (Math.Abs(ts.TotalDays) > 31)
                return Json(new { Error = true, Message = "Timespan between from and to date cannot be greater than 31 days" }, JsonRequestBehavior.AllowGet);

            string sql = @"
                        SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName
,EMP.EmployeeCode,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
,EMP.EmpPicPath,
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
                        WHERE emp.PlantId='" + identity.PlantId + @"' AND o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'
      	ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
                    ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpPost, Authorize]
        public ActionResult getAttendanceData(string employeeid, string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = stringAttendanceData(employeeid, fromdate, todate);


            string shiftSQL = @" SELECT * FROM ShiftDefination AS sd WHERE sd.PlantID='" + identity.PlantId + @"'";


            var jsondata = Json(new { data = _sqlRepository.GetModelCollection<AttendanceProcessData>(sql), shift = _sqlRepository.GetDataCollection(shiftSQL) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpPost, Authorize]
        public ActionResult getShift(string systemid, string WorkDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            //     string sql = @"
            //                SELECT 
            //                     sd.SystemID,
            //                     sd.InTimeStartMargin, sd.IsActive, sd.DefaultShift, sd.SequenceNo, 
            //                     sd.UserName AS ShiftName,
            //                     format(kk.ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
            //                     format(DATEADD(minute,CASE WHEN sd.IsGapInclude=0 THEN ISNULL((sd.WorkingHour+sd.BreakPeriod), sd.WorkingHour+sd.BreakPeriod) ELSE ISNULL((sd.WorkingHour+sd.BreakPeriod),'" + WorkDate + @"') END,kk.ShiftInTime),'dd-MMM-yyyy hh:mm tt') AS ShiftOutTime

            //                      FROM (
            //                     SELECT 
            //                     sd.SystemID,
            //                      	DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),'" + WorkDate + @"'))  AS ShiftInTime

            //                      FROM ShiftDefination sd
            //                      LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '" + WorkDate + @"' BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
            //                     ) AS KK
            //                     INNER JOIN   ShiftDefination sd ON sd.SystemID=kk.SystemID
            //                     LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '" + WorkDate + @"' BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
            //WHERE sd.systemid='" + systemid + @"'
            //                     ORDER BY sd.SequenceNo ASC ";

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
        public ActionResult SaveSingleEmployee(List<AttendanceProcessData> data)
        {
            try
            {
                List<AttendanceProcessData> DataToBeSaved = new List<AttendanceProcessData>();

                if (data == null)
                    throw new Exception("No new data has been updated");

                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].DayStatusNew != data[i].DayStatus)
                    {
                        DataToBeSaved.Add(data[i]);
                    }
                }



                string inDates = "";
                string inEmployeeIds = "";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                try
                {
                    //string inDates = "";
                    //string inEmployeeIds = "";
                    foreach (AttendanceProcessData item in DataToBeSaved)
                    {
                        if (inDates == "")
                            inDates = "'" + item.WorkDate + "'";
                        else
                            inDates += ",'" + item.WorkDate + "'";

                        if (inEmployeeIds == "")
                            inEmployeeIds = "'" + item.Id + "'";
                        else
                            inEmployeeIds += ",'" + item.Id + "'";
                    }

                    if (inDates != "")
                    {
                        DataTable dtLock = _sqlRepository.GetDataTable("SELECT * FROM PlantWiseAttendanceLock AS pwal WHERE  isActive=1 AND pwal.LockedDate IN (" + inDates + ") AND pwal.PlantId='" + identity.PlantId + "'");
                        DataTable dtLockEmployee = _sqlRepository.GetDataTable("SELECT * FROM ExceptionEmployeeAttendanceUnlock WHERE EmpSystemId IN (" + inEmployeeIds + @")");
                        for (int i = 0; i < dtLock.Rows.Count; i++)
                        {
                            var k = DataToBeSaved.Where(ee => ee.WorkDate.ToUpper() == Convert.ToDateTime(dtLock.Rows[i]["LockedDate"].ToString()).ToString("dd-MMM-yyyy").ToUpper());
                            foreach (var item in k)
                            {
                                dtLockEmployee.DefaultView.RowFilter = "EmpSystemId='" + item.Id + "' AND WorkDate=#" + item.WorkDate + "#";
                                if (dtLockEmployee.DefaultView.Count == 0)
                                {
                                    item.IsError = true;
                                    item.ErrorMessage = "Day locked";
                                }
                            }
                        }

                        if (DataToBeSaved.Where(ee => ee.IsError == true).ToList().Count > 0)
                        {

                            return Json(new { Error = true, Message = "Error occured", Data = DataToBeSaved }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception)
                {


                }


                #region Leave validation
                string _sql_leave = @"select e.EmployeeCode,format(d.workdate,'dd-MMM-yyyy')wd from LeaveTransactionDetails d
                                                left join LeaveTransaction t on t.SystemID=d.LvTrnsSystemID
                                                inner join EmployeeInformation e on e.systemid=t.EmpSystemID where 
                                                t.EmpSystemID in (" + inEmployeeIds + @") and 
                                                d.WorkDate in (" + inDates + @") and d.LeaveDuration>=1";
                DataTable dtLeave = _sqlRepository.GetDataTable(_sql_leave);
                if(dtLeave.Rows.Count>0)
                {
                    string msg = string.Empty;
                    foreach (DataRow item in dtLeave.Rows)
                    {
                        if (msg == "")
                            msg = "'" + item["EmployeeCode"].ToString() + "' on ("+ item["wd"].ToString() + @")";
                        else
                            msg += ", '" + item["EmployeeCode"].ToString() + "' on (" + item["wd"].ToString() + @")";
                    }

                    throw new Exception("Leave  entry for the following employees must be deleted...");
                }
                #endregion



                if (DataToBeSaved.Where(ee => ee.IsError == true).ToList().Count > 0)
                {

                    return Json(new { Error = true, Message = "Error occured", Data = DataToBeSaved }, JsonRequestBehavior.AllowGet);
                }
                //operations
                saveData(DataToBeSaved);



                return Json(new { Error = false, Message = "Time updated successfully", Data = data }, JsonRequestBehavior.AllowGet);

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

        private void saveData(List<AttendanceProcessData> data)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID objId = new bplib.clsGenID();

                clsStaticInfo objStatic = new clsStaticInfo();

                for (int i = 0; i < data.Count; i++)
                {


                    #region manual Attendance

                    DataSet dsManualAttendance = null;

                    if (data[i].DayStatus != data[i].DayStatusNew)
                    {
                        con = new ConnectionManager.clsConnection();
                        con.BeginTransaction();
                        con.getDataSet(@"SELECT * FROM AttdnManualData AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "'", out dsManualAttendance);
                        con.CommitTransaction();


                        if (dsManualAttendance.Tables[0].Rows.Count > 0)
                        {

                            DataRow dr = dsManualAttendance.Tables[0].Rows[0];

                            dr.BeginEdit();


                            dr["DayStatus"] = DBNull.Value;
                            if (string.IsNullOrEmpty(data[i].DayStatusNew) == false)
                                dr["DayStatus"] = data[i].DayStatusNew;


                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now;


                            dr.EndEdit();
                        }
                        else
                        {

                            DataRow dr = dsManualAttendance.Tables[0].NewRow();

                            dr["EmpSystemID"] = data[i].Id;
                            dr["WorkDate"] = data[i].WorkDate;
                            dr["GroupID"] = identity.CompanyGroupId;
                            //dr["PlantID"] = identity.PlantId;

                            dr["DayStatus"] = DBNull.Value;
                            if (string.IsNullOrEmpty(data[i].DayStatusNew) == false)
                                dr["DayStatus"] = data[i].DayStatusNew;



                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now;
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now;

                            dsManualAttendance.Tables[0].Rows.Add(dr);




                        }
                    }
                    #endregion manual Attendance

                    if (dsManualAttendance != null)
                    {
                        if (dsManualAttendance.Tables[0].DefaultView.Count > 0)
                        {
                            if (string.IsNullOrEmpty(dsManualAttendance.Tables[0].DefaultView[0]["DayStatus"].ToString()) == true
                                && string.IsNullOrEmpty(dsManualAttendance.Tables[0].DefaultView[0]["InTime"].ToString()) == true
                                 && string.IsNullOrEmpty(dsManualAttendance.Tables[0].DefaultView[0]["OutTime"].ToString()) == true)
                            {
                                dsManualAttendance.Tables[0].DefaultView[0].Delete();
                            }
                        }
                    }

                    objStatic.SaveDataSets(dsManualAttendance);

                    try
                    {
                        AttendanceProcessAplos obj = new AttendanceProcessAplos();
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                        ReturnType r = obj.SaveTotal(identity.PlantId, data[i].WorkDate, "'" + data[i].Id + "'", false);//laila



                    }
                    catch (Exception ex)
                    {

                        throw new Exception("Error occured while processing attendance " + ex.Message);

                    }



                }
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }



        private DataTable getDateWiseShift(List<AttendanceProcessData> data)
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

            //      string sql = @" SELECT dt.WorkDate,

            //                     sd.SystemID,
            //                      sd.InTimeStartMargin, sd.IsActive, sd.DefaultShift, sd.SequenceNo, 
            //                      sd.UserName AS ShiftName,
            //                      format(kk.ShiftInTime,'dd-MMM-yyyy hh:mm:ss tt') AS ShiftInTime,
            //                      format(DATEADD(minute,CASE WHEN sd.IsGapInclude=0 THEN ISNULL((sd.WorkingHour+sd.BreakPeriod), sd.WorkingHour+sd.BreakPeriod) ELSE ISNULL((sd.WorkingHour+sd.BreakPeriod),'16-May-2019') END,kk.ShiftInTime),'dd-MMM-yyyy hh:mm tt') AS ShiftOutTime

            //                   FROM
            //                   (" + dateString + @") AS DT
            //   LEFT OUTER JOIN
            //(
            //                      SELECT 
            //                      sd.SystemID,dt.WorkDate,
            //                       	DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),dt.WorkDate))  AS ShiftInTime

            //                       FROM 

            //                        (" + dateString + @") AS DT
            //		LEFT OUTER JOIN ShiftDefination sd ON 1=1
            //		LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON DT.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
            //                      ) AS KK ON dt.WorkDate=kk.WorkDate
            //                      INNER JOIN   ShiftDefination sd ON sd.SystemID=kk.SystemID
            //                      LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON dt.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
            //	WHERE sd.PlantID='" + identity.PlantId + @"'
            //                  ORDER BY dt.WorkDate, sd.SequenceNo ASC ";


            string sql = @" SELECT dt.WorkDate,
 
                           sd.SystemID,
                            sd.InTimeStartMargin, sd.IsActive, sd.DefaultShift, sd.SequenceNo, 
                            sd.UserName AS ShiftName,
                            format(kk.ShiftInTime,'dd-MMM-yyyy hh:mm:ss tt') AS ShiftInTime,
                            format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime

                         FROM
                         (" + dateString + @") AS DT
					    LEFT OUTER JOIN
						(
                            SELECT 
                            sd.SystemID,dt.WorkDate,
		                           	DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),dt.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),dt.WorkDate))  AS ShiftOutTime
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
        private string stringAttendanceData(string employeeid, string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            if (string.IsNullOrEmpty(employeeid) == false)
                employeeid = "AND emp.SystemId='" + employeeid + @"'";
            else
            {
                todate = fromdate;
            }
            return @" SELECT 
                          kk.IsOD, kk.AttendanceRestDetailId, kk.LTSystemID,   kk.Id,kk.EmployeeCode,E.UserName as Entity,
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

                            KK.DayStatus,KK.DayStatus AS DayStatusNew, KK.OTHr,
                            KK.IsOTComfirm, KK.IsOTEntitled,KK.IsManualDayStatus

                             FROM (
								
		                            SELECT O.IsOD,o.AttendanceRestDetailId,o.LTSystemID, Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, O.IsManualDayStatus,
       
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,
		                            O.IsOTEntitled

		                            FROM EmployeeInformation EMP
		                            LEFT JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                       
                            WHERE o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'" + employeeid + @"
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
                        WHERE EMP.PlantID='" + identity.PlantId + @"'
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, WorkDate) ASC ";




        }
    }

}