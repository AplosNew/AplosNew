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

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class AttendanceProcessDataEntityWiseController : BaseController
    {


        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public AttendanceProcessDataEntityWiseController(IUnitOfWork U, ISqlRepository R)
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


            string shiftSQL = @" SELECT * FROM ShiftDefination AS sd WHERE sd.IsActive=1 and sd.PlantID='" + identity.PlantId + @"'";


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
                    //if (
                    //    data[i].ShiftSystemID != data[i].ShiftSystemIDOriginal
                    //    || Convert.ToDateTime(data[i].InDate + " " + data[i].InTime) != Convert.ToDateTime(data[i].InDateOriginal + " " + data[i].InTimeOriginal)
                    //    || Convert.ToDateTime(data[i].OutDate + " " + data[i].OutTime) != Convert.ToDateTime(data[i].OutDateOriginal + " " + data[i].OutTimeOriginal)
                    //    )
                    //{
                    DataToBeSaved.Add(data[i]);

                    //}
                }





                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                try
                {
                    string inDates = "";
                    string inEmployeeIds = "";
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




                DataTable NewShiftStandardTime = getDateWiseShift(DataToBeSaved);
                //validations
                foreach (AttendanceProcessData item in DataToBeSaved)
                {

                    //if (string.IsNullOrEmpty(item.InTime) == true && string.IsNullOrEmpty(item.OutTime) == true)
                    //    continue;

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

                                TimeSpan ts = Convert.ToDateTime(item.OutDate + " " + item.OutTime).Subtract(Convert.ToDateTime(item.InDate + " " + item.InTime));
                                if (Math.Abs(ts.TotalHours) > 24)
                                {
                                    item.IsError = true;
                                    item.ErrorMessage = "Time span cannot be greater than 24 hours between in and out time";
                                }
                            }
                        }
                        if (item.InTime != null)
                        {
                            if (item.InDate + item.InTime != item.InDateOriginal + item.InTimeOriginal)
                            {
                                if (Convert.ToDateTime(item.InDate + " " + item.InTime) < Convert.ToDateTime(NewShiftStandardTime.DefaultView[0]["ShiftInTime"].ToString())
                               .AddHours(-8))
                                {
                                    item.IsError = true;
                                    item.ErrorMessage = "In time is too early";
                                }
                                if (Convert.ToDateTime(item.InDate + " " + item.InTime) > Convert.ToDateTime(NewShiftStandardTime.DefaultView[0]["ShiftOutTime"].ToString()))
                                {
                                    item.IsError = true;
                                    item.ErrorMessage = "In time is after shift end time";
                                }
                            }
                        }
                        if (item.OutTime != null)
                        {
                            if (item.OutDate + item.OutTime != item.OutDateOriginal + item.OutTimeOriginal)
                            {
                                if (Convert.ToDateTime(item.OutDate + " " + item.OutTime) > Convert.ToDateTime(NewShiftStandardTime.DefaultView[0]["ShiftOutTime"].ToString())
                         .AddHours(16))
                                {
                                    item.IsError = true;
                                    item.ErrorMessage = "Out time is too late";
                                }
                            }
                        }
                        //if (Convert.ToDateTime(item.InDate + " " + item.InTime) < Convert.ToDateTime(NewShiftStandardTime.DefaultView[0]["ShiftInTime"].ToString())
                        //    .AddMinutes(clsStaticInfo.dbl(NewShiftStandardTime.DefaultView[0]["InTimeStartMargin"].ToString()) * -1))
                        //{
                        //    item.IsError = true;
                        //    item.ErrorMessage = "In time is too early";
                        //}



                        //if (Convert.ToDateTime(item.InDate + " " + item.InTime) < Convert.ToDateTime(NewShiftStandardTime.DefaultView[0]["ShiftInTime"].ToString()) && Convert.ToDateTime(item.OutDate + " " + item.OutTime) < Convert.ToDateTime(NewShiftStandardTime.DefaultView[0]["ShiftInTime"].ToString()))
                        //{
                        //    item.IsError = true;
                        //    item.ErrorMessage = "Both In and Out time is before shift start time";
                        //}







                    }

                }

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

                clsShiftInfo objStatic1 = new clsShiftInfo(_sqlRepository);
                clsSetInOut objSetInOut = new clsSetInOut();
                DataSet dsHRsetting = null;
                objStatic1.GetHRsettinng(identity.PlantId, out dsHRsetting);


                DataSet dsPrevious = null, dsfuture = null, dsDailyShiftAssignment = null, dsFutureShiftAssignment = null;
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].ShiftSystemID != data[i].ShiftSystemIDOriginal)
                    {
                        #region change shift
                        //// objId.GenID("SHIFT ASSIGNMENT MANUAL", out FutureSystemID);
                        //con = new ConnectionManager.clsConnection();
                        //con.BeginTransaction();
                        //con.getDataSet(@"SELECT TOP 1 * FROM EmployeeShiftAssign AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.EffectiveDate <= '" + data[i].WorkDate + "'  ORDER BY SA.EffectiveDate DESC", out dsPrevious);
                        //con.CommitTransaction();

                        //dsfuture = dsPrevious.Clone();//without data
                        //DataRow drpre = dsfuture.Tables[0].NewRow();

                        //for (int COL = 0; COL < dsPrevious.Tables[0].Columns.Count; COL++)
                        //    drpre[COL] = dsPrevious.Tables[0].Rows[0][COL];

                        //dsfuture.Tables[0].Rows.Add(drpre);
                        ////dsfuture.Tables[0].ImportRow(dsPrevious.Tables[0].Rows[0]);//future data saved//need to change PK+DATE

                        ////for today
                        //string PreviousSystemID = dsPrevious.Tables[0].Rows[0]["SystemID"].ToString();
                        //string TodaySystemID = "";
                        //dsPrevious.Tables[0].DefaultView.RowFilter = "EffectiveDate=#" + data[i].WorkDate + "#";
                        //if (dsPrevious.Tables[0].DefaultView.Count > 0)
                        //{



                        //    DataRow dr = dsPrevious.Tables[0].DefaultView[0].Row;
                        //    TodaySystemID = dr["SystemID"].ToString();

                        //    dr.BeginEdit();
                        //    dr["FixSystemID"] = data[i].ShiftSystemID;

                        //    dr["RosterSystemID"] = DBNull.Value;
                        //    dr["IsFix"] = true;
                        //    dr["IsRoster"] = false;
                        //    dr["EffectiveDate"] = data[i].WorkDate;
                        //    dr["RosterStartShiftID"] = DBNull.Value;
                        //    dr["StartFromDay"] = DBNull.Value;



                        //    dr["UpdatedBy"] = identity.Name;
                        //    dr["DateUpdated"] = System.DateTime.Now;

                        //    dr.EndEdit();
                        //}
                        //else
                        //{
                        //    DataRow dr = dsPrevious.Tables[0].NewRow();
                        //    objId.GenID("SHIFT ASSIGNMENT MANUAL", out TodaySystemID);


                        //    dr["SystemID"] = "SFTX" + TodaySystemID;
                        //    dr["EmpSystemID"] = data[i].Id;
                        //    dr["FixSystemID"] = data[i].ShiftSystemID;
                        //    dr["RosterSystemID"] = DBNull.Value;
                        //    dr["IsFix"] = true;
                        //    dr["IsRoster"] = false;
                        //    dr["EffectiveDate"] = data[i].WorkDate;
                        //    dr["RosterStartShiftID"] = DBNull.Value;
                        //    dr["StartFromDay"] = DBNull.Value;


                        //    dr["UpdatedBy"] = identity.Name;
                        //    dr["DateUpdated"] = System.DateTime.Now;
                        //    dr["AddedBy"] = identity.Name;
                        //    dr["DateAdded"] = System.DateTime.Now;

                        //    dsPrevious.Tables[0].Rows.Add(dr);

                        //    TodaySystemID = dr["SystemID"].ToString();

                        //}



                        //con = new ConnectionManager.clsConnection();
                        //con.BeginTransaction();
                        //con.getDataSet(@"SELECT * FROM EmpDateWiseShiftAssign AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "' ", out dsDailyShiftAssignment);
                        //con.CommitTransaction();
                        //if (dsDailyShiftAssignment.Tables[0].Rows.Count > 0)
                        //{
                        //    dsDailyShiftAssignment.Tables[0].Rows[0].BeginEdit();

                        //    dsDailyShiftAssignment.Tables[0].Rows[0]["EmpSftAssiSystemID"] = TodaySystemID;
                        //    dsDailyShiftAssignment.Tables[0].Rows[0]["ShiftSystemID"] = data[i].ShiftSystemID;

                        //    dsDailyShiftAssignment.Tables[0].Rows[0].EndEdit();
                        //}
                        //else
                        //{
                        //    //DataRow dr = dsDailyShiftAssignment.Tables[0].NewRow();



                        //    //dr["SystemID"] = "SFTX" + TodaySystemID;
                        //    //dr["EmpSystemID"] = data[i].Id;
                        //    //dr["FixSystemID"] = data[i].ShiftSystemID;
                        //    //dr["RosterSystemID"] = DBNull.Value;
                        //    //dr["IsFix"] = DBNull.Value;
                        //    //dr["IsRoster"] = DBNull.Value;
                        //    //dr["EffectiveDate"] = data[i].WorkDate;
                        //    //dr["RosterStartShiftID"] = DBNull.Value;
                        //    //dr["StartFromDay"] = DBNull.Value;


                        //    //dr["UpdatedBy"] = identity.Name;
                        //    //dr["DateUpdated"] = System.DateTime.Now;
                        //    //dr["AddedBy"] = identity.Name;
                        //    //dr["DateAdded"] = System.DateTime.Now;

                        //    //dsDailyShiftAssignment.Tables[0].Rows.Add(dr);
                        //}



                        //string FutureSystemID = "";
                        //DataSet dsFutureTemp;
                        //con = new ConnectionManager.clsConnection();
                        //con.BeginTransaction();
                        //con.getDataSet(@"SELECT TOP 1 * FROM EmployeeShiftAssign AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.EffectiveDate > '" + data[i].WorkDate + "'  ORDER BY SA.EffectiveDate ASC", out dsFutureTemp);
                        //con.CommitTransaction();
                        //dsFutureTemp.Tables[0].DefaultView.RowFilter = "EffectiveDate=#" + Convert.ToDateTime(data[i].WorkDate).AddDays(1).ToString("dd-MMM-yyyy") + "#";


                        //if (dsFutureTemp.Tables[0].DefaultView.Count == 0 && Convert.ToDateTime(data[i].WorkDate).AddDays(1) < System.DateTime.Now)
                        //{
                        //    string fsystemid = "";
                        //    objId.GenID("SHIFT ASSIGN NEW", out fsystemid);

                        //    dsfuture.Tables[0].Rows[0].BeginEdit();

                        //    dsfuture.Tables[0].Rows[0]["SystemID"] = "SAS" + fsystemid;
                        //    dsfuture.Tables[0].Rows[0]["EffectiveDate"] = Convert.ToDateTime(data[i].WorkDate).AddDays(1).ToString("dd-MMM-yyyy");

                        //    dsfuture.Tables[0].Rows[0]["UpdatedBy"] = identity.Name;
                        //    dsfuture.Tables[0].Rows[0]["DateUpdated"] = System.DateTime.Now;
                        //    dsfuture.Tables[0].Rows[0]["AddedBy"] = identity.Name;
                        //    dsfuture.Tables[0].Rows[0]["DateAdded"] = System.DateTime.Now;

                        //    dsfuture.Tables[0].Rows[0].EndEdit();

                        //    FutureSystemID = dsfuture.Tables[0].Rows[0]["SystemID"].ToString();


                        //    con = new ConnectionManager.clsConnection();
                        //    con.BeginTransaction();
                        //    con.getDataSet(@"SELECT * FROM EmpDateWiseShiftAssign AS SA WHERE SA.EmpSftAssiSystemID = '" + PreviousSystemID + "' AND sa.WorkDate > '" + data[i].WorkDate + "' ", out dsFutureShiftAssignment);
                        //    con.CommitTransaction();

                        //    foreach (DataRow item in dsFutureShiftAssignment.Tables[0].Rows)
                        //    {
                        //        item.BeginEdit();

                        //        item["EmpSftAssiSystemID"] = FutureSystemID;

                        //        item["UpdatedBy"] = identity.Name;
                        //        item["DateUpdated"] = System.DateTime.Now;

                        //        item.EndEdit();
                        //    }
                        //}
                        //else
                        //{
                        //    dsfuture = null;
                        //}



                        #endregion change shift

                        #region change shift




                        con = new ConnectionManager.clsConnection();
                        con.BeginTransaction();
                        con.getDataSet(@"SELECT * FROM EmpDateWiseShiftAssign AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "' ", out dsDailyShiftAssignment);
                        con.CommitTransaction();
                        if (dsDailyShiftAssignment.Tables[0].Rows.Count > 0)
                        {
                            dsDailyShiftAssignment.Tables[0].Rows[0].BeginEdit();

                            //dsDailyShiftAssignment.Tables[0].Rows[0]["EmpSftAssiSystemID"] = TodaySystemID;
                            dsDailyShiftAssignment.Tables[0].Rows[0]["ShiftSystemID"] = data[i].ShiftSystemID;
                            dsDailyShiftAssignment.Tables[0].Rows[0]["ManualShiftId"] = data[i].ShiftSystemID;
                            dsDailyShiftAssignment.Tables[0].Rows[0]["UpdatedBy"] = identity.Name;
                            dsDailyShiftAssignment.Tables[0].Rows[0]["DateUpdated"] = DateTime.Now;
                            dsDailyShiftAssignment.Tables[0].Rows[0].EndEdit();
                        }








                        #endregion change shift

                    }

                    #region manual Attendance

                    DataSet dsManualAttendance = null;

                    if (data[i].InDate + data[i].InTime != data[i].InDateOriginal + data[i].InTimeOriginal
                        || data[i].OutDate + data[i].OutTime != data[i].OutDateOriginal + data[i].OutTimeOriginal)
                    {
                        con = new ConnectionManager.clsConnection();
                        con.BeginTransaction();
                        con.getDataSet(@"SELECT * FROM AttdnManualData AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "'", out dsManualAttendance);
                        con.CommitTransaction();

                        if (data[i].InTime == null && data[i].OutTime == null)
                        {

                            if (dsManualAttendance.Tables[0].Rows.Count > 0)
                            {
                                if (string.IsNullOrEmpty(dsManualAttendance.Tables[0].Rows[0]["DayStatus"].ToString()) == true)
                                {
                                    dsManualAttendance.Tables[0].Rows[0].Delete();
                                }
                            }
                        }
                        else
                        {
                            if (dsManualAttendance.Tables[0].Rows.Count > 0)
                            {

                                DataRow dr = dsManualAttendance.Tables[0].Rows[0];

                                dr.BeginEdit();





                                if (data[i].InDate + data[i].InTime != data[i].InDateOriginal + data[i].InTimeOriginal)
                                {
                                    dr["InTime"] = DBNull.Value;
                                    if (string.IsNullOrEmpty(data[i].InTime) == false)
                                        dr["InTime"] = data[i].InDate + " " + data[i].InTime;
                                }

                                if (data[i].OutDate + data[i].OutTime != data[i].OutDateOriginal + data[i].OutTimeOriginal)
                                {
                                    dr["OutTime"] = DBNull.Value;
                                    if (string.IsNullOrEmpty(data[i].OutTime) == false)
                                        dr["OutTime"] = data[i].OutDate + " " + data[i].OutTime;
                                }

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
                                dr["PlantID"] = identity.PlantId;

                                if (data[i].InDate + data[i].InTime != data[i].InDateOriginal + data[i].InTimeOriginal)
                                {
                                    dr["InTime"] = DBNull.Value;
                                    if (string.IsNullOrEmpty(data[i].InTime) == false)
                                        dr["InTime"] = data[i].InDate + " " + data[i].InTime;
                                }

                                if (data[i].OutDate + data[i].OutTime != data[i].OutDateOriginal + data[i].OutTimeOriginal)
                                {
                                    dr["OutTime"] = DBNull.Value;
                                    if (string.IsNullOrEmpty(data[i].OutTime) == false)
                                        dr["OutTime"] = data[i].OutDate + " " + data[i].OutTime;
                                }


                                dr["UpdatedBy"] = identity.Name;
                                dr["DateUpdated"] = System.DateTime.Now;
                                dr["AddedBy"] = identity.Name;
                                dr["DateAdded"] = System.DateTime.Now;

                                dsManualAttendance.Tables[0].Rows.Add(dr);



                            }
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

                    //objStatic.SaveDataSets(dsPrevious, dsfuture, dsDailyShiftAssignment, dsFutureShiftAssignment, dsManualAttendance);
                    objStatic.SaveDataSets(dsDailyShiftAssignment, dsManualAttendance);


                    try
                    {


                        if (dsHRsetting.Tables[0].Rows.Count > 0)
                        {
                            objSetInOut.SetRawINOUTonShiftAssignment(identity.PlantId, identity.CompanyGroupId, data[i].WorkDate, "'" + data[i].Id + "'");
                        }
                        clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                        ReturnType r = obj.SaveTotal(identity.PlantId, data[i].WorkDate, "'" + data[i].Id + "'", false);//laila


                        //AttendanceEarlyOut objEarlyOut = new AttendanceEarlyOut();
                        //objEarlyOut.Execute(identity.CompanyGroupId, identity.PlantId, data[i].WorkDate, "'" + data[i].Id + "'", true);


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
                            CONVERT(BIT,CASE WHEN (ISNULL(KK.InTime,'')<>'' OR ISNULL(KK.OutTime,'')<>'' ) AND (ISNULL(KK.InTime,'')='' OR ISNULL(KK.OutTime,'')='') THEN 1 ELSE 0 END) AS IsPunchMissing,
                       
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
                            KK.IsOTComfirm, KK.IsOTEntitled,KK.IsManualDayStatus

                             FROM (
								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime,
       
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,
		                            O.IsOTEntitled,O.IsManualDayStatus

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


            //    return @"
            //               SELECT 
            //                kk.Id,kk.EmployeeCode,
            //                format(KK.WorkDate,'ddd') AS DayName, 
            //                format(KK.WorkDate,'dd-MMM-yyyy') AS WorkDate, 

            //                KK.ShiftSystemID,kk.ShiftName,KK.ShiftSystemID AS ShiftSystemIDOriginal,
            //                format(ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
            //             	format(DATEADD(minute,CASE WHEN sd.IsGapInclude=0 THEN ISNULL((sd.WorkingHour+sd.BreakPeriod), sd.WorkingHour+sd.BreakPeriod) ELSE ISNULL((sd.WorkingHour+sd.BreakPeriod),sd.WorkingHour) END,kk.ShiftInTime),'dd-MMM-yyyy hh:mm tt') AS ShiftOutTime,


            //                format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDate,format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDateOriginal,
            //                format(KK.InTime,'hh:mm tt') AS  InTime, format(KK.InTime,'hh:mm tt') AS  InTimeOriginal, 

            //                KK.IsManualInTime, 


            //                format(isnull(KK.OutTime,format(DATEADD(minute,CASE WHEN sd.IsGapInclude=0 THEN ISNULL((sd.WorkingHour+sd.BreakPeriod), sd.WorkingHour+sd.BreakPeriod) ELSE ISNULL((sd.WorkingHour+sd.BreakPeriod),sd.WorkingHour) END,kk.ShiftInTime),'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDate,
            //                format(isnull(KK.OutTime,format(DATEADD(minute,CASE WHEN sd.IsGapInclude=0 THEN ISNULL((sd.WorkingHour+sd.BreakPeriod), sd.WorkingHour+sd.BreakPeriod) ELSE ISNULL((sd.WorkingHour+sd.BreakPeriod),sd.WorkingHour) END,kk.ShiftInTime),'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDateOriginal,
            //                format(KK.OutTime,'hh:mm tt') AS  OutTime, format(KK.OutTime,'hh:mm tt') AS  OutTimeOriginal, 


            //                KK.IsManualOutTime,

            //                format(KK.PunchInTime,'dd-MMM-yyyy hh:mm tt') AS PunchInTime,
            //                format(KK.PunchOutTime,'dd-MMM-yyyy hh:mm tt') AS PunchOutTime,

            //                KK.DayStatus, KK.OTHr,
            //                KK.IsOTComfirm, KK.IsOTEntitled

            //                 FROM (

            //                  SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
            //DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
            //                  O.InTime, O.IsManualInTime,
            //                  O.OutTime, O.IsManualOutTime, 

            //                  O.PunchInTime,O.PunchOutTime,
            //                  O.DayStatus, O.OTHr, O.IsOTComfirm,
            //                  O.IsOTEntitled

            //                  FROM EmployeeInformation EMP
            //                  LEFT JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
            //                  LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
            //                  LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID


            //                WHERE o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'" + employeeid + @"
            //                ) AS KK
            //                LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=kk.ShiftSystemID
            //                LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON kk.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID

            //                ORDER BY kk.EmployeeCode,CONVERT(DATE, WorkDate) ASC ";

        }
    }

}