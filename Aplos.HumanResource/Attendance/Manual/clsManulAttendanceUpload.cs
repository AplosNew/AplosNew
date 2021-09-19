using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Attendances;
using Library.Service.Helpers;
using OTSBD;
using SetINOUT;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;

namespace Library.HumanResource.Attendance.Manual
{
    public class RT
    {
        public bool IsError = false;
        public List<AttendanceProcessData> data = null;
        public string msg = string.Empty;
    }
    public class clsManulAttendanceUpload//:Controller
    {
        CustomIdentity _identity = null;
        ISqlRepository _sqlRepository = null;
        public clsManulAttendanceUpload(CustomIdentity _identity, ISqlRepository _sqlRepository)
        {
            this._identity = _identity;
            this._sqlRepository = _sqlRepository;
        }
        public RT Save(List<AttendanceProcessData> data)
        {
            try
            {
                List<AttendanceProcessData> DataToBeSaved = new List<AttendanceProcessData>();

                if (data == null)
                    throw new Exception("No new data has been updated");

                for (int i = 0; i < data.Count; i++)
                {
                    //    if (
                    //        data[i].ShiftSystemID != data[i].ShiftSystemIDOriginal
                    //        || Convert.ToDateTime(data[i].InDate + " " + data[i].InTime) != Convert.ToDateTime(data[i].InDateOriginal + " " + data[i].InTimeOriginal)
                    //        || Convert.ToDateTime(data[i].OutDate + " " + data[i].OutTime) != Convert.ToDateTime(data[i].OutDateOriginal + " " + data[i].OutTimeOriginal)
                    //        )
                    //    {
                    DataToBeSaved.Add(data[i]);

                    //    }
                }





                var identity = _identity;// (CustomIdentity)Thread.CurrentPrincipal.Identity;
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
                        DataTable dtLock = _sqlRepository.GetDataTable("SELECT * FROM PlantWiseAttendanceLock AS pwal WHERE isActive=1 AND pwal.LockedDate IN (" + inDates + ") AND pwal.PlantId='" + identity.PlantId + "'");
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
                            return new RT { data = DataToBeSaved, IsError = true, msg = "Error occured" };
                            // return Json(new { Error = true, Message = "Error occured", Data = DataToBeSaved }, JsonRequestBehavior.AllowGet);
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
                    return new RT { data = DataToBeSaved, IsError = true, msg = "Error occured" };
                    //return Json(new { Error = true, Message = "Error occured", Data = DataToBeSaved }, JsonRequestBehavior.AllowGet);
                }
                //operations
                saveData(DataToBeSaved);


                return new RT { data = data, IsError = false, msg = "Time updated successfully" };
                //return Json(new { Error = false, Message = "Time updated successfully", Data = data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return new RT { data = data, IsError = true, msg = ex.Message };
                //return Json(new
                //{
                //    Error = true,
                //    Message = ex.Message,
                //    Data = data
                //}, JsonRequestBehavior.AllowGet);
            }

        }
        void _setValue(AttendanceProcessData data, ref DataRow dr)
        {
            try
            {
                //if (data.IsManualDayStatus==false)
                //{
                //    dr["DayStatus"] = data.DayStatus;
                //}
                if (data.IsManualDayStatus == false)
                {
                    if (data.InDate != null && data.InTime != null)
                    {
                        if (data.InDate != data.InDateOriginal || data.InTime != data.InTimeOriginal)
                        {
                            dr["InTime"] = data.InDate + " " + data.InTime;
                        }
                    }
                    else
                    {
                        dr["InTime"] = DBNull.Value;
                    }

                    if (data.OutDate != null && data.OutTime != null)
                    {
                        if (data.OutDate != data.OutDateOriginal || data.OutTime != data.OutTimeOriginal)
                        {
                            dr["OutTime"] = data.OutDate + " " + data.OutTime;
                        }
                    }
                    else
                    {
                        dr["OutTime"] = DBNull.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void xsaveData(List<AttendanceProcessData> data)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID objId = new bplib.clsGenID();
                for (int i = 0; i < data.Count; i++)
                {


                    DataSet dsManualAttendance = null;
                    //DataSet dsManualAttendanceFromApp = null;
                    DataSet dsDateWise = null;


                    con = new ConnectionManager.clsConnection();
                    con.BeginTransaction();
                    con.getDataSet(@"SELECT * FROM AttdnManualData AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "'", out dsManualAttendance);
                    //con.getDataSet(@"SELECT * FROM AttndManualDataFromApp AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "'", out dsManualAttendanceFromApp);
                    con.getDataSet(@"SELECT * FROM EmpDateWiseShiftAssign AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "'", out dsDateWise);
                    con.CommitTransaction();

                    #region Emp Date wise shift
                    var CurrentDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    if (data[i].WorkDate == CurrentDate)
                    {
                        if (dsDateWise.Tables[0].Rows.Count > 0)
                        {
                            DataRow dx = dsDateWise.Tables[0].Rows[0];
                            dx.BeginEdit();
                            dx["ManualShiftId"] = data[i].ShiftSystemID;
                            dx["ShiftSystemID"] = data[i].ShiftSystemID;
                            dx["UpdatedBy"] = identity.Name;
                            dx["DateUpdated"] = System.DateTime.Now;
                            dx.EndEdit();
                        }
                        else
                        {
                            DataRow dx = dsDateWise.Tables[0].NewRow();
                            dx["ManualShiftId"] = data[i].ShiftSystemID;
                            dx["ShiftSystemID"] = data[i].ShiftSystemID;
                            dx["UpdatedBy"] = identity.Name;
                            dx["DateUpdated"] = System.DateTime.Now;
                            dsDateWise.Tables[0].Rows.Add(dx);
                        }
                    }
                    #endregion

                    #region manual Attendance 
                    if (dsManualAttendance.Tables[0].Rows.Count > 0)
                    {
                        DataRow dr = dsManualAttendance.Tables[0].Rows[0];
                        dr.BeginEdit();
                        _setValue(data[i], ref dr);
                        dr["UpdatedBy"] = identity.Name;
                        dr["DateUpdated"] = System.DateTime.Now;
                        dr.EndEdit();
                    }
                    else
                    {
                        DataRow dr = dsManualAttendance.Tables[0].NewRow();
                        dr["EmpSystemID"] = data[i].Id;
                        if (data[i].WorkDate == null)
                        {
                            dr["WorkDate"] = data[i].InDate;
                        }
                        else
                        {
                            dr["WorkDate"] = data[i].WorkDate;
                        }
                        dr["GroupID"] = identity.CompanyGroupId;
                        _setValue(data[i], ref dr);
                        dr["UpdatedBy"] = identity.Name;
                        dr["DateUpdated"] = System.DateTime.Now;
                        dr["AddedBy"] = identity.Name;
                        dr["DateAdded"] = System.DateTime.Now;
                        dsManualAttendance.Tables[0].Rows.Add(dr);
                    }
                    #endregion manual Attendance 

                    SaveDataSets(dsManualAttendance, dsDateWise);

                    try
                    {

                        //if (dsHRsetting.Tables[0].Rows.Count > 0)
                        //{
                        //    DateTime FromDateR = Convert.ToDateTime(ss.EffectiveDate);
                        //    DateTime ToDateR = DateTime.Now;
                        //    while (FromDateR <= ToDateR)
                        //    {

                        //        objSetInOut.SetRawINOUTonShiftAssignment(identity.PlantId, identity.CompanyGroupId, FromDateR.ToString("dd-MMM-yyyy"), ss.EmpSystemIDs);
                        //        FromDateR = FromDateR.AddDays(1);
                        //    }
                        //}//hr
                        DataSet dsHRsetting = null;
                        clsShiftInfo objStatic = new clsShiftInfo(_sqlRepository);
                        clsSetInOut objSetInOut = new clsSetInOut();
                        objStatic.GetHRsettinng(identity.PlantId, out dsHRsetting);
                        if (dsHRsetting.Tables[0].Rows.Count > 0)
                        {
                            objSetInOut.SetRawINOUTonShiftAssignment(identity.PlantId, identity.CompanyGroupId, data[i].WorkDate, "'" + data[i].Id + "'");
                        }


                        clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
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
        private void saveData(List<AttendanceProcessData> data)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var identity = _identity;
                bplib.clsGenID objId = new bplib.clsGenID();

                clsShiftInfo objStatic = new clsShiftInfo(_sqlRepository);
                clsSetInOut objSetInOut = new clsSetInOut();
                DataSet dsHRsetting = null;
                objStatic.GetHRsettinng(identity.PlantId, out dsHRsetting);



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
                        //    dr["StartFromDay"] = DBNull.Value;//
                        //    dr["IsSingleDayShift"] = true;//IsSingleDayShift



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
                        //    dr["IsSingleDayShift"] = true;


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
                                //dr["PlantID"] = identity.PlantId;

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

                    //SaveDataSets(dsPrevious, dsfuture, dsDailyShiftAssignment, dsFutureShiftAssignment, dsManualAttendance);
                    SaveDataSets(dsDailyShiftAssignment, dsManualAttendance);

                    try
                    {

                        //if (dsHRsetting.Tables[0].Rows.Count > 0)
                        //{
                        //    DateTime FromDateR = Convert.ToDateTime(ss.EffectiveDate);
                        //    DateTime ToDateR = DateTime.Now;
                        //    while (FromDateR <= ToDateR)
                        //    {

                        //        objSetInOut.SetRawINOUTonShiftAssignment(identity.PlantId, identity.CompanyGroupId, FromDateR.ToString("dd-MMM-yyyy"), ss.EmpSystemIDs);
                        //        FromDateR = FromDateR.AddDays(1);
                        //    }
                        //}//hr

                        if (dsHRsetting.Tables[0].Rows.Count > 0)
                        {
                            objSetInOut.SetRawINOUTonShiftAssignment(identity.PlantId, identity.CompanyGroupId, data[i].WorkDate, "'" + data[i].Id + "'");
                        }


                        clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
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
        public void SaveDataSets(params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                        if (dsRef[i].Tables.Count > 0)
                            objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                    i++;
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exp)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        DataTable getDateWiseShift(List<AttendanceProcessData> data)
        {
            string dateString = "";
            for (int i = 0; i < data.Count; i++)
            {
                if (dateString == "")
                    dateString = " select CONVERT(DATETIME,'" + data[i].WorkDate + "') AS WorkDate ";
                else
                    dateString += " UNION select CONVERT(DATETIME,'" + data[i].WorkDate + "') ";

            }
            var identity = _identity;
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
        DataTable _getEmpInfo(string empids, string fromDate, string toDate)
        {
            string sql = @"select e.SystemId Id,  format(a.WorkDate,'ddd') [DayName],a.AttendanceRestDetailId
                                    ,a.DayStatus,a.DayStatus DayStatusNew
                                    ,format(a.InTime,'HH:mm')InTime
                                    ,format(a.InTime,'HH:mm')InTimeOriginal
                                    ,format(a.InTime,'dd-MMM-yyyy') InDate
                                    ,format(a.InTime,'dd-MMM-yyyy') InDateOriginal
                                    ,format(a.OutTime,'HH:mm')OutTime
                                    ,format(a.OutTime,'HH:mm')OutTimeOriginal
                                    ,format(a.OutTime,'dd-MMM-yyyy') OutDate
                                    ,format(a.OutTime,'dd-MMM-yyyy') OutDateOriginal
                                    ,sd.SystemID ShiftSystemID
                                    ,sd.SystemID ShiftSystemIDOriginal
                                    ,a.LTSystemID,'0' ModelState
                                    ,a.LTSystemID LTSystemIDOriginal
                                    ,'' Department,'' Designation,'' Entity,'' ErrorMessage,convert(bit, 0) IsError
                                    ,e.EmployeeCode,e.EmployeeName
                                    ,a.IsManualDayStatus,a.IsManualInTime,a.IsManualOutTime
                                    ,a.IsOD,a.IsOTComfirm,a.IsOTEntitled
                                     ,trim(str(a.OTHr)) OTHr
,format(a.PunchInTime,'dd-MMM-yyyy') PunchInTime
									 ,format(a.PunchOutTime,'dd-MMM-yyyy') PunchOutTime
                                    ,format(sd.InTime,'HH:mm')ShiftInTime
									  ,format(sd.OutTime,'HH:mm')ShiftOutTime

,sd.ShiftDefinationName ShiftName

                                    ,format(a.WorkDate,'dd-MMM-yyyy') WorkDate
                                    ,'' Section,'' SubSection
                                from AttdnProcessData a
                                left join EmployeeInformation e on a.EmpSystemID=e.SystemId
                                left join ShiftDefination sd on sd.SystemID=a.ShiftSystemID
                                where EmpSystemID in
                                (
                                " + empids + @"
                                ) and a.WorkDate between '" + fromDate + @"' and '" + toDate + @"' ";
            return _sqlRepository.GetDataTable(sql);
        }
        void _createEmpIds(List<AttendanceManualData> fromUI, out string empids)
        {
            empids = "''";
            try
            {
                foreach (var item in fromUI)
                {
                    empids += ",'" + item.EmpSystemId + "'";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _mergeList(List<AttendanceManualData> fromUI, List<apd> fromDB, out List<AttendanceProcessData> finalList)
        {
            finalList = new List<AttendanceProcessData>();
            try
            {
                foreach (var ui_data in fromUI)
                {
                    var db_data = fromDB.Where(r => r.Id == ui_data.EmpSystemId && r.WorkDate == ui_data.WorkDate).FirstOrDefault();
                    if (db_data != null)
                    {
                        AttendanceProcessData newObj = new AttendanceProcessData();
                        newObj.AttendanceRestDetailId = db_data.AttendanceRestDetailId;

                        #region set value
                        newObj.DayName = db_data.DayName;
                        newObj.DayStatus = db_data.DayStatus;

                        newObj.DayStatusNew = db_data.DayStatusNew;
                        newObj.Department = db_data.Department;
                        newObj.Designation = db_data.Designation;

                        newObj.EmployeeCode = db_data.EmployeeCode;
                        newObj.EmployeeName = db_data.EmployeeName;
                        newObj.Entity = db_data.Entity;

                        newObj.ErrorMessage = db_data.ErrorMessage;
                        newObj.Id = db_data.Id;
                        newObj.InDate = db_data.InDate;

                        newObj.InDateOriginal = db_data.InDateOriginal;
                        newObj.InTime = db_data.InTime;
                        newObj.InTimeOriginal = db_data.InTimeOriginal;

                        newObj.IsError = db_data.IsError;
                        newObj.IsManualDayStatus = db_data.IsManualDayStatus;
                        newObj.IsManualInTime = db_data.IsManualInTime;

                        newObj.IsManualOutTime = db_data.IsManualOutTime;
                        newObj.IsOD = db_data.IsOD;
                        newObj.IsOTComfirm = db_data.IsOTComfirm;

                        newObj.IsOTEntitled = db_data.IsOTEntitled;
                        newObj.LTSystemID = db_data.LTSystemID;
                        newObj.LTSystemIDOriginal = db_data.LTSystemIDOriginal;

                        newObj.OTHr = db_data.OTHr;
                        newObj.OutDate = db_data.OutDate;
                        newObj.OutDateOriginal = db_data.OutDateOriginal;

                        newObj.OutTime = db_data.OutTime;
                        newObj.OutTimeOriginal = db_data.OutTimeOriginal;
                        newObj.PunchInTime = db_data.PunchInTime;

                        newObj.PunchOutTime = db_data.PunchOutTime;
                        newObj.Section = db_data.Section;
                        newObj.ShiftInTime = db_data.ShiftInTime;

                        newObj.ShiftName = db_data.ShiftName;
                        newObj.ShiftOutTime = db_data.ShiftOutTime;
                        newObj.ShiftSystemID = db_data.ShiftSystemID;

                        newObj.ShiftSystemIDOriginal = db_data.ShiftSystemIDOriginal;
                        newObj.SubSection = db_data.SubSection;
                        newObj.WorkDate = db_data.WorkDate;
                        #endregion


                        if (string.IsNullOrEmpty(ui_data.InDate) == false && string.IsNullOrEmpty(ui_data.InTime) == false)
                        {
                            newObj.InDate = ui_data.InDate;
                            newObj.InDateOriginal = null;
                            newObj.InTime = ui_data.InTime;
                            newObj.InTimeOriginal = null;
                            newObj.IsManualInTime = true;
                        }
                        if (string.IsNullOrEmpty(ui_data.OutDate) == false && string.IsNullOrEmpty(ui_data.OutTime) == false)
                        {
                            newObj.OutDate = ui_data.OutDate;
                            newObj.OutDateOriginal = null;
                            newObj.OutTime = ui_data.OutTime;
                            newObj.OutTimeOriginal = null;
                            newObj.IsManualOutTime = true;
                        }
                        newObj.LTSystemIDOriginal = null;
                        newObj.ShiftSystemID = GetPK(ui_data.ShiftSystemID);
                        newObj.ShiftSystemIDOriginal = null;

                        finalList.Add(newObj);
                    }//null
                }//foreach
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void CreateFinalList(List<AttendanceManualData> fromUI, string fromDate, string toDate, out List<AttendanceProcessData> finalList)
        {
            List<apd> _listDB = null;
            string empids = string.Empty;

            try
            {
                finalList = new List<AttendanceProcessData>();
                _createEmpIds(fromUI, out empids);
                DataTable dt_db = _getEmpInfo(empids, fromDate, toDate);
                if (dt_db.Rows.Count > 0)
                {
                    _listDB = dt_db.ToList<apd>();
                }
                _mergeList(fromUI, _listDB, out finalList);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetShift(string PlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT UserName+'_#'+id UserName FROM scs.District where active=1 order by UserName";
                strSQL = @"select ShiftDefinationDescription +'_#'+ SystemId UserName from ShiftDefination where IsActive=1  and PlantID='" + PlantID + "' order by ShiftDefinationDescription";
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
        public void CreateSource(DataSet ds, int Col, string Header, ref IWorksheet sheetSource)
        {
            try
            {
                ReportUtility ru = new ReportUtility();
                ru.SetText(ref sheetSource, 1, Col, Header);
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    var un = ds.Tables[0].Rows[i]["UserName"].ToString();
                    int k = i + 2;
                    ru.SetText(ref sheetSource, k, Col, un);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IWorkbook GetSampleFile(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string date)
        {
            #region declare
            clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            #endregion
            try
            {
                ReportUtility ru = new ReportUtility();
                DataSet dsShift = null;
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;

                objRpt = new clsReport();
                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                GetShift(PlantId, out dsShift);

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(2);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                var iEmployeeCode = 0;
                var iShiftName = 0;
                var iWorkDate = 0;
                var iShiftId = 0;
                var iDayStatus = 0;
                var isl = 0;
                var iInTime = 0;
                var iInDate = 0;
                var iOutTime = 0;
                var iOutDate = 0;
                var iShiftInTime = 0;
                var iShiftOutTime = 0;

                DataTable data = GetBulletinTemplateData(PlantId, CompanyGroupId, date);

                #region Lunch Out
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                xlsRow = 1;
                IWorksheet sheetSource = null;
                sheetSource = workbook.Worksheets[1];

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "EmpSystemId";
                sheet1.Range[xlsRow, isl].ColumnWidth = 18;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "EmployeeCode";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 18;

                xlsCol += 1;
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "WorkDate";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 18;

                xlsCol += 1;
                iDayStatus = xlsCol;
                sheet1.Range[xlsRow, iDayStatus].Text = "DayStatus";
                sheet1.Range[xlsRow, iDayStatus].ColumnWidth = 18;

                xlsCol += 1;
                iShiftId = xlsCol;
                sheet1.Range[xlsRow, iShiftId].Text = "ShiftSystemID";
                sheet1.Range[xlsRow, iShiftId].ColumnWidth = 18;
                CreateSource(dsShift, 1, "Shift", ref sheetSource); int ShiftCol = 1;

                //ru.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ShiftSystemId");
                ru.SetList(ref sheet1, xlsRow, data.Rows.Count, xlsCol, sheetSource, ShiftCol, dsShift.Tables[0].Rows.Count);
                //int ShiftSystemIdCol = xlsCol; xlsCol += 1;


                xlsCol += 1;
                iShiftName = xlsCol;
                sheet1.Range[xlsRow, iShiftName].Text = "ShiftName";
                sheet1.Range[xlsRow, iShiftName].ColumnWidth = 36;

                xlsCol += 1;
                iShiftInTime = xlsCol;
                sheet1.Range[xlsRow, iShiftInTime].Text = "ShiftInTime";
                sheet1.Range[xlsRow, iShiftInTime].ColumnWidth = 20;

                xlsCol += 1;
                iShiftOutTime = xlsCol;
                sheet1.Range[xlsRow, iShiftOutTime].Text = "ShiftOutTime";
                sheet1.Range[xlsRow, iShiftOutTime].ColumnWidth = 20;

                xlsCol += 1;
                iInDate = xlsCol;
                sheet1.Range[xlsRow, iInDate].Text = "InDate";
                sheet1.Range[xlsRow, iInDate].ColumnWidth = 20;

                xlsCol += 1;
                iInTime = xlsCol;
                sheet1.Range[xlsRow, iInTime].Text = "InTime";
                sheet1.Range[xlsRow, iInTime].ColumnWidth = 20;

                xlsCol += 1;
                iOutDate = xlsCol;
                sheet1.Range[xlsRow, iOutDate].Text = "OutDate";
                sheet1.Range[xlsRow, iOutDate].ColumnWidth = 20;

                xlsCol += 1;
                iOutTime = xlsCol;
                sheet1.Range[xlsRow, iOutTime].Text = "OutTime";
                sheet1.Range[xlsRow, iOutTime].ColumnWidth = 20;



                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------

                #region data in column



                int ROW = 1;
                int endCol = 1;
                int COL = 1;
                var startRow = 0;

                int RowIndex = ROW;
                startRow = ROW;
                ROW++;
                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet1[ROW, isl].Text = data.Rows[i]["SystemId"].ToString();
                    sheet1[ROW, iEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                    sheet1[ROW, iWorkDate].Text = data.Rows[i]["WorkDate"].ToString();
                    sheet1[ROW, iDayStatus].Text = data.Rows[i]["DayStatus"].ToString();
                    sheet1[ROW, iShiftId].Text = data.Rows[i]["ShiftId"].ToString();
                    sheet1[ROW, iShiftName].Text = data.Rows[i]["ShiftName"].ToString();
                    sheet1[ROW, iShiftInTime].Text = data.Rows[i]["ShiftInTime"].ToString();
                    sheet1[ROW, iShiftOutTime].Text = data.Rows[i]["ShiftOutTime"].ToString();

                    //sheet1[ROW, iInTime].Text = "";
                    sheet1.Range[ROW, iInDate].NumberFormat = "@";
                    sheet1.Range[ROW, iInDate].Text = "";
                    sheet1.Range[ROW, iInTime].NumberFormat = "HH:mm";
                    sheet1.Range[ROW, iOutDate].NumberFormat = "@";
                    sheet1.Range[ROW, iOutDate].Text = "";
                    sheet1.Range[ROW, iOutTime].NumberFormat = "HH:mm";
                    ROW++;
                }

                #endregion

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Sheet1";
                #endregion Page Setup


                #endregion  Lunch Out

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string GetPK(string colvalue)
        {
            string r = string.Empty;
            string token = "_#";
            try
            {
                //var k = colvalue;
                if (colvalue != null)
                {
                    var _index = colvalue.IndexOf(token);
                    if (_index != -1)
                    {
                        r = colvalue.Substring(_index + token.Length).Trim().Replace("\n", "").Replace("\r", "");
                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private DataTable GetBulletinTemplateData(string plantId, string GroupId, string data)
        {
            try
            {
                // DateTime dtt = Convert.ToDateTime(data).AddMonths(1).AddDays(-1);
                string sql = @"select * from (select e.SystemId,e.EmployeeCode, 
                                                format(a.WorkDate,'dd-MMM-yyyy')WorkDate,a.DayStatus,
                                                s.ShiftDefinationDescription +'_#'+ s.SystemId ShiftId,s.ShiftDefinationDescription ShiftName,
                                                FORMAT(CAST(s.InTime AS datetime2), N'hh:mm tt')ShiftInTime,
												FORMAT(CAST(s.OutTime AS datetime2), N'hh:mm tt')ShiftOutTime
                                                from AttdnProcessData a
                                                left join EmployeeInformation e on e.SystemId = a.EmpSystemID
									            left join ShiftDefination s on s.SystemID = a.ShiftSystemID 
                                                where a.PlantID ='" + plantId + @"' and a.GroupID = '" + GroupId + @"') kk where 
kk.workdate = '" + data + @"'
";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class AttendanceProcessData : BaseModel
    {
        public string Id { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Section { get; set; } = "";
        public string SubSection { get; set; } = "";
        public string Department { get; set; } = "";
        public string Designation { get; set; } = "";
        public string Entity { get; set; } = "";
        public string LTSystemID { get; set; } = "";
        public string LTSystemIDOriginal { get; set; } = "";

        public bool IsOD { get; set; } = false;
        public string AttendanceRestDetailId { get; set; } = "";


        public string DayName { get; set; } = "";
        public string WorkDate { get; set; } = "";
        public string ShiftSystemID { get; set; } = "";
        public string ShiftSystemIDOriginal { get; set; } = "";
        public string ShiftName { get; set; } = "";
        public string ShiftInTime { get; set; } = "";
        public string ShiftOutTime { get; set; } = "";
        public string InDate { get; set; } = "";
        public string InTime { get; set; } = "";
        public string InDateOriginal { get; set; } = "";
        public string InTimeOriginal { get; set; } = "";
        public bool IsManualInTime { get; set; } = false;
        public string OutDate { get; set; } = "";
        public string OutTime { get; set; } = "";
        public string OutDateOriginal { get; set; } = "";
        public string OutTimeOriginal { get; set; } = "";
        public bool IsManualOutTime { get; set; } = false;
        public string PunchInTime { get; set; } = "";
        public string PunchOutTime { get; set; } = "";
        public string DayStatus { get; set; } = "";
        public string DayStatusNew { get; set; } = "";
        public bool IsManualDayStatus { get; set; } = false;
        public string OTHr { get; set; } = "";
        public bool IsOTComfirm { get; set; } = false;
        public bool IsOTEntitled { get; set; } = false;
        public bool IsError { get; set; } = false;
        public string ErrorMessage { get; set; } = "";
        public bool IsPunchMissing { get; set; } = false;
    }

    public class apd
    {
        public string Id { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Section { get; set; } = "";
        public string SubSection { get; set; } = "";
        public string Department { get; set; } = "";
        public string Designation { get; set; } = "";
        public string Entity { get; set; } = "";
        public string LTSystemID { get; set; } = "";
        public string LTSystemIDOriginal { get; set; } = "";

        public bool IsOD { get; set; } = false;
        public string AttendanceRestDetailId { get; set; } = "";


        public string DayName { get; set; } = "";
        public string WorkDate { get; set; } = "";
        public string ShiftSystemID { get; set; } = "";
        public string ShiftSystemIDOriginal { get; set; } = "";
        public string ShiftName { get; set; } = "";
        public string ShiftInTime { get; set; } = "";
        public string ShiftOutTime { get; set; } = "";
        public string InDate { get; set; } = "";
        public string InTime { get; set; } = "";
        public string InDateOriginal { get; set; } = "";
        public string InTimeOriginal { get; set; } = "";
        public bool IsManualInTime { get; set; } = false;
        public string OutDate { get; set; } = "";
        public string OutTime { get; set; } = "";
        public string OutDateOriginal { get; set; } = "";
        public string OutTimeOriginal { get; set; } = "";
        public bool IsManualOutTime { get; set; } = false;
        public string PunchInTime { get; set; } = "";
        public string PunchOutTime { get; set; } = "";
        public string DayStatus { get; set; } = "";
        public string DayStatusNew { get; set; } = "";
        public bool IsManualDayStatus { get; set; } = false;
        public string OTHr { get; set; } = "";
        public bool IsOTComfirm { get; set; } = false;
        public bool IsOTEntitled { get; set; } = false;
        public bool IsError { get; set; } = false;
        public string ErrorMessage { get; set; } = "";
    }

    public class AttendanceManualData
    {
        public string EmpSystemId { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string WorkDate { get; set; } = "";
        public string DayStatus { get; set; } = "";
        public string ShiftSystemID { get; set; } = "";
        public string ShiftName { get; set; } = "";
        public string ShiftInTime { get; set; } = "";
        public string ShiftOutTime { get; set; } = "";
        public string InDate { get; set; } = "";
        public string InTime { get; set; } = "";
        public string OutDate { get; set; } = "";
        public string OutTime { get; set; } = "";
    }

}
