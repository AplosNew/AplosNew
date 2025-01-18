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
using System.Web.Script.Serialization;
using Library.HumanResource.Attendance.Manual;
using SetINOUT;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ManualShiftController : BaseController
    {
        //getAttendanceData,SaveSingleEmployee

        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public ManualShiftController(IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion Constructor
        #region -- Pages

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
                        EMP.EmployeeName
,EMP.EmployeeCode,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric
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
    order by EmployeeCodePreFix,EmployeeCodeNumeric
      
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

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost]
        public ActionResult SaveSingleEmployee(List<AttendanceProcessData> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsManulAttendanceUpload mau = new clsManulAttendanceUpload(identity, _sqlRepository);
            RT _rt = mau.Save(data);

            if (_rt.IsError)
            {
                //return Json(new { Error = true, Message = "Error occured", Data = DataToBeSaved }, JsonRequestBehavior.AllowGet);
                return Json(new { Message = _rt.msg, Error = true, Data = _rt.data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Error = false, Message = _rt.msg, Data = _rt.data }, JsonRequestBehavior.AllowGet);
                //return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
        }

        public void GetHRsettinng(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select * from PlantWiseHRMSSetting where PlantID='" + plantid + "' and isnull(ShiftBasedPunchFlag,0)=1";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function


        private void saveData(List<AttendanceProcessData> data)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID objId = new bplib.clsGenID();

                clsStaticInfo objStatic = new clsStaticInfo();

                DataSet dsHRsetting = null;
                GetHRsettinng(identity.PlantId, out dsHRsetting);
                clsSetInOut objsetinout = new clsSetInOut();



                DataSet dsPrevious = null, dsfuture = null, dsDailyShiftAssignment = null, dsFutureShiftAssignment = null;
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].ShiftSystemID != data[i].ShiftSystemIDOriginal)
                    {
                        #region change shift
                        // objId.GenID("SHIFT ASSIGNMENT MANUAL", out FutureSystemID);
                        con = new ConnectionManager.clsConnection();
                        con.BeginTransaction();
                        con.getDataSet(@"SELECT TOP 1 * FROM EmployeeShiftAssign AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.EffectiveDate <= '" + data[i].WorkDate + "'  ORDER BY SA.EffectiveDate DESC", out dsPrevious);
                        con.CommitTransaction();

                        dsfuture = dsPrevious.Clone();//without data
                        DataRow drpre = dsfuture.Tables[0].NewRow();

                        for (int COL = 0; COL < dsPrevious.Tables[0].Columns.Count; COL++)
                            drpre[COL] = dsPrevious.Tables[0].Rows[0][COL];

                        dsfuture.Tables[0].Rows.Add(drpre);
                        //dsfuture.Tables[0].ImportRow(dsPrevious.Tables[0].Rows[0]);//future data saved//need to change PK+DATE

                        //for today
                        string PreviousSystemID = dsPrevious.Tables[0].Rows[0]["SystemID"].ToString();
                        string TodaySystemID = "";
                        dsPrevious.Tables[0].DefaultView.RowFilter = "EffectiveDate=#" + data[i].WorkDate + "#";
                        if (dsPrevious.Tables[0].DefaultView.Count > 0)
                        {



                            DataRow dr = dsPrevious.Tables[0].DefaultView[0].Row;
                            TodaySystemID = dr["SystemID"].ToString();

                            dr.BeginEdit();
                            dr["FixSystemID"] = data[i].ShiftSystemID;

                            dr["RosterSystemID"] = DBNull.Value;
                            dr["IsFix"] = true;
                            dr["IsRoster"] = false;
                            dr["EffectiveDate"] = data[i].WorkDate;
                            dr["RosterStartShiftID"] = DBNull.Value;
                            dr["StartFromDay"] = DBNull.Value;//
                            dr["IsSingleDayShift"] = true;//IsSingleDayShift



                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now;

                            dr.EndEdit();
                        }
                        else
                        {
                            DataRow dr = dsPrevious.Tables[0].NewRow();
                            objId.GenID("SHIFT ASSIGNMENT MANUAL", out TodaySystemID);


                            dr["SystemID"] = "SFTX" + TodaySystemID;
                            dr["EmpSystemID"] = data[i].Id;
                            dr["FixSystemID"] = data[i].ShiftSystemID;
                            dr["RosterSystemID"] = DBNull.Value;
                            dr["IsFix"] = true;
                            dr["IsRoster"] = false;
                            dr["EffectiveDate"] = data[i].WorkDate;
                            dr["RosterStartShiftID"] = DBNull.Value;
                            dr["StartFromDay"] = DBNull.Value;
                            dr["IsSingleDayShift"] = true;


                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now;
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now;

                            dsPrevious.Tables[0].Rows.Add(dr);

                            TodaySystemID = dr["SystemID"].ToString();

                        }



                        con = new ConnectionManager.clsConnection();
                        con.BeginTransaction();
                        con.getDataSet(@"SELECT * FROM EmpDateWiseShiftAssign AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "' ", out dsDailyShiftAssignment);
                        con.CommitTransaction();
                        if (dsDailyShiftAssignment.Tables[0].Rows.Count > 0)
                        {
                            dsDailyShiftAssignment.Tables[0].Rows[0].BeginEdit();

                            dsDailyShiftAssignment.Tables[0].Rows[0]["EmpSftAssiSystemID"] = TodaySystemID;
                            dsDailyShiftAssignment.Tables[0].Rows[0]["ShiftSystemID"] = data[i].ShiftSystemID;

                            dsDailyShiftAssignment.Tables[0].Rows[0].EndEdit();
                        }
                        else
                        {
                            //DataRow dr = dsDailyShiftAssignment.Tables[0].NewRow();



                            //dr["SystemID"] = "SFTX" + TodaySystemID;
                            //dr["EmpSystemID"] = data[i].Id;
                            //dr["FixSystemID"] = data[i].ShiftSystemID;
                            //dr["RosterSystemID"] = DBNull.Value;
                            //dr["IsFix"] = DBNull.Value;
                            //dr["IsRoster"] = DBNull.Value;
                            //dr["EffectiveDate"] = data[i].WorkDate;
                            //dr["RosterStartShiftID"] = DBNull.Value;
                            //dr["StartFromDay"] = DBNull.Value;


                            //dr["UpdatedBy"] = identity.Name;
                            //dr["DateUpdated"] = System.DateTime.Now;
                            //dr["AddedBy"] = identity.Name;
                            //dr["DateAdded"] = System.DateTime.Now;

                            //dsDailyShiftAssignment.Tables[0].Rows.Add(dr);
                        }



                        string FutureSystemID = "";
                        DataSet dsFutureTemp;
                        con = new ConnectionManager.clsConnection();
                        con.BeginTransaction();
                        con.getDataSet(@"SELECT TOP 1 * FROM EmployeeShiftAssign AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.EffectiveDate > '" + data[i].WorkDate + "'  ORDER BY SA.EffectiveDate ASC", out dsFutureTemp);
                        con.CommitTransaction();
                        dsFutureTemp.Tables[0].DefaultView.RowFilter = "EffectiveDate=#" + Convert.ToDateTime(data[i].WorkDate).AddDays(1).ToString("dd-MMM-yyyy") + "#";


                        if (dsFutureTemp.Tables[0].DefaultView.Count == 0 && Convert.ToDateTime(data[i].WorkDate).AddDays(1) < System.DateTime.Now)
                        {
                            string fsystemid = "";
                            objId.GenID("SHIFT ASSIGN NEW", out fsystemid);

                            dsfuture.Tables[0].Rows[0].BeginEdit();

                            dsfuture.Tables[0].Rows[0]["SystemID"] = "SAS" + fsystemid;
                            dsfuture.Tables[0].Rows[0]["EffectiveDate"] = Convert.ToDateTime(data[i].WorkDate).AddDays(1).ToString("dd-MMM-yyyy");

                            dsfuture.Tables[0].Rows[0]["UpdatedBy"] = identity.Name;
                            dsfuture.Tables[0].Rows[0]["DateUpdated"] = System.DateTime.Now;
                            dsfuture.Tables[0].Rows[0]["AddedBy"] = identity.Name;
                            dsfuture.Tables[0].Rows[0]["DateAdded"] = System.DateTime.Now;

                            dsfuture.Tables[0].Rows[0].EndEdit();

                            FutureSystemID = dsfuture.Tables[0].Rows[0]["SystemID"].ToString();


                            con = new ConnectionManager.clsConnection();
                            con.BeginTransaction();
                            con.getDataSet(@"SELECT * FROM EmpDateWiseShiftAssign AS SA WHERE SA.EmpSftAssiSystemID = '" + PreviousSystemID + "' AND sa.WorkDate > '" + data[i].WorkDate + "' ", out dsFutureShiftAssignment);
                            con.CommitTransaction();

                            foreach (DataRow item in dsFutureShiftAssignment.Tables[0].Rows)
                            {
                                item.BeginEdit();

                                item["EmpSftAssiSystemID"] = FutureSystemID;

                                item["UpdatedBy"] = identity.Name;
                                item["DateUpdated"] = System.DateTime.Now;

                                item.EndEdit();
                            }
                        }
                        else
                        {
                            dsfuture = null;
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

                    objStatic.SaveDataSets(dsPrevious, dsfuture, dsDailyShiftAssignment, dsFutureShiftAssignment, dsManualAttendance);







                    try
                    {
                        clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();

                        //objsetinout.SetRawINOUTonShiftAssignment(identity.PlantId, identity.CompanyGroupId, data[i].WorkDate, "'" + data[i].Id + "'");
                        if (dsHRsetting.Tables[0].Rows.Count > 0)
                        {
                            objsetinout.SetRawINOUTonShiftAssignment(identity.PlantId, identity.CompanyGroupId, data[i].WorkDate, "'" + data[i].Id + "'");
                        }

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
                employeeid = " AND emp.SystemId='" + employeeid + @"' ";
            else
            {
                todate = fromdate;
            }
            return @" SELECT convert(bit, 0) AS Active,
                            kk.Id,kk.EmployeeCode,E.UserName as Entity,
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
                        where emp.plantid='" + identity.PlantId + @"'
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, WorkDate) ASC ";


        }
    }
}