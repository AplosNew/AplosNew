using bplib;
using ConnectionManager;
using Library.Data.Sql;
using Library.General.Setups;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using context = System.Web.HttpContext;

namespace Library.HumanResource.NewAttendanceProcess {

    public class NewAttendanceProcessService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public NewAttendanceProcessService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }

        #region Shift Process
        public void ShiftProcess(string Date, string PlantValue,string UserId=null)
        {
            ProcessLock _lock = new ProcessLock(UserId, ProcessLockId.AttendanceProcess, "", 60);
            _lock.LockProcess();
            string EmpId = "";
            try
            {

                DataSet PlantLock;
                PlantLockCheck(Date, out PlantLock, PlantValue);
                if (PlantLock.Tables[0].Rows.Count > 0)
                {

                }
                else
                {

                    #region AssignedShift Process           
                    DataSet UnProcessed;
                    UnProcessedEmp(Date, out UnProcessed, PlantValue); //DataSet of Employees For Row Creation
                    if (UnProcessed.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = UnProcessed.Tables[0].Rows[0][@"WorkDate"].ToString();
                        var GpId = UnProcessed.Tables[0].Rows[0][@"GroupID"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter("select * from AttdnProcessData where WorkDate='" + WkDate + "'and PlantID='" + PlantValue + "'", out DataSet dsRef, false, false, "", "1");
                        
                        for (int i = 0; i < UnProcessed.Tables[0].Rows.Count; i++)
                        {
                             EmpId = UnProcessed.Tables[0].Rows[i][@"SystemId"].ToString();
                            string PlantId = UnProcessed.Tables[0].Rows[i][@"PlantId"].ToString();
                            string RowId = UnProcessed.Tables[0].Rows[i][@"RowId"].ToString();
                            string ManualShift = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ManualShift"]).ToString();
                            string ManualShiftDurn = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ManualDuration"]).ToString();
                            string ManualShiftIn = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ManualShiftIn"]).ToString();
                            string ManualShiftOut = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ManualShiftOut"]).ToString();
                            string ManualInTime = UnProcessed.Tables[0].Rows[i][@"ManualInTime"].ToString();
                            string ManualOutTime = UnProcessed.Tables[0].Rows[i][@"ManualOutTime"].ToString();
                            string ManualDayStatus = UnProcessed.Tables[0].Rows[i][@"ManualDayStatus"].ToString();
                            string IsManualInTime = UnProcessed.Tables[0].Rows[i][@"IsManualInTime"].ToString();
                            string IsManualOutTime = UnProcessed.Tables[0].Rows[i][@"IsManualOutTime"].ToString();
                            string IsManualDayStatus = UnProcessed.Tables[0].Rows[i][@"IsManualDayStatus"].ToString();
                            ShiftTime(ref ManualShiftIn, ref ManualShiftOut, WkDate);

                            string BudgetShift = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"BudgetedShift"]).ToString();
                            string BudgetShiftDurn = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"BudgetDuration"]).ToString();
                            string BudgetShiftIn = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"BudgetShiftIn"]).ToString();
                            string BudgetShiftOut = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"BudgetShiftOut"]).ToString();
                            ShiftTime(ref BudgetShiftIn, ref BudgetShiftOut, WkDate);

                            var ProfileShift = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ProfileShift"]).ToString();
                            var ProfileShiftDurn = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ProfileDuration"]).ToString();
                            var ProfileShiftIn = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ProfileShiftIn"]).ToString();
                            var ProfileShiftOut = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ProfileShiftOut"]).ToString();
                            ShiftTime(ref ProfileShiftIn, ref ProfileShiftOut, WkDate);

                            var RosterShift = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"RosterShift"]).ToString();
                            var RosterShiftDurn = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"RosterDuration"]).ToString();
                            var RosterShiftIn = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"RosterShiftIn"]).ToString();
                            var RosterShiftOut = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"RosterShiftOut"]).ToString();
                            var BudgetId = UnProcessed.Tables[0].Rows[i][@"BudgetId"].ToString();
                            var Deployment = UnProcessed.Tables[0].Rows[i][@"Deployment"].ToString();
                            var BudgetedManpower = UnProcessed.Tables[0].Rows[i][@"BudgetedManpower"].ToString();
                            var RosterId = UnProcessed.Tables[0].Rows[i][@"RosterId"].ToString();
                            var GivenDesignationId = UnProcessed.Tables[0].Rows[i][@"GivenDesignationId"].ToString();
                            ShiftTime(ref RosterShiftIn, ref RosterShiftOut, WkDate);

                            var PlantInPunchStartTime = UnProcessed.Tables[0].Rows[i][@"PlantInPunchStartTime"].ToString();
                            PlantInTime(ref PlantInPunchStartTime, WkDate);

                            var FullDayDuration = UnProcessed.Tables[0].Rows[i][@"FullDayDuration"].ToString();
                            var HalfDayDuration = UnProcessed.Tables[0].Rows[i][@"HalfDayDuration"].ToString();
                            var ShortDuration = UnProcessed.Tables[0].Rows[i][@"ShortDuration"].ToString();
                            var HoursWithoutOT = UnProcessed.Tables[0].Rows[i][@"HoursWithoutOT"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";


                            if (dsRef.Tables[0].DefaultView.Count == 0 && Convert.ToBoolean(UnProcessed.Tables[0].Rows[i]["TobeAdded"].ToString()) == true)
                            {
                                DataRow dr = dsRef.Tables[0].NewRow();
                                dr["EmpSystemID"] = EmpId;
                                dr["RowId"] = RowId;
                                dr["WorkDate"] = WkDate;
                                dr["GroupID"] = GpId;
                                dr["PlantID"] = PlantId;

                                dr["ManualShiftID"] = clsWebLib.RetValidLen(ManualShift);
                                dr["RosterShiftID"] = clsWebLib.RetValidLen(RosterShift);
                                dr["ProfileShiftID"] = clsWebLib.RetValidLen(ProfileShift);
                                dr["BudgetedShiftID"] = clsWebLib.RetValidLen(BudgetShift);
                                dr["BudgetId"] = clsWebLib.RetValidLen(BudgetId);
                                dr["RosterId"] = clsWebLib.RetValidLen(RosterId);
                                dr["GivenDesignationId"] = clsWebLib.RetValidLen(GivenDesignationId);
                                dr["PlantInPunchStartTime"] = clsWebLib.RetValidLen(PlantInPunchStartTime);
                                dr["Deployment"] = clsWebLib.RetValidLen(Deployment);
                                dr["BudgetedManpower"] = clsWebLib.RetValidLen(BudgetedManpower);

                                #region ManualData Entry
                                if (clsWebLib.RetValidLen(ManualInTime).ToString() != "")
                                {
                                    dr["ManualInTime"] = clsWebLib.RetValidLen(ManualInTime);
                                    dr["IsManualInTime"] = clsWebLib.GetBoolData(IsManualInTime);
                                    dr["OriginalManualInTime"] = clsWebLib.RetValidLen(ManualInTime);
                                }
                                if (clsWebLib.RetValidLen(ManualOutTime).ToString() != "")
                                {
                                    dr["ManualOutTime"] = clsWebLib.RetValidLen(ManualOutTime);
                                    dr["IsManualOutTime"] = clsWebLib.GetBoolData(IsManualOutTime);
                                    dr["OriginalManualOutTime"] = clsWebLib.RetValidLen(ManualOutTime);
                                }
                                if (clsWebLib.RetValidLen(ManualDayStatus).ToString() != "")
                                {
                                    dr["ManualDayStatus"] = clsWebLib.RetValidLen(ManualDayStatus);
                                    dr["IsManualDayStatus"] = clsWebLib.GetBoolData(IsManualDayStatus);
                                }
                                #endregion

                                // Priority Wise Shift Assignment
                                #region AssignedShift Data
                                if (ManualShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = ManualShift;
                                    dr["ShiftDuration"] = ManualShiftDurn;
                                    dr["ShiftInTime"] = ManualShiftIn;
                                    dr["ShiftOutTime"] = ManualShiftOut;
                                }
                                else if (ProfileShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = ProfileShift;
                                    dr["ShiftDuration"] = ProfileShiftDurn;
                                    dr["ShiftInTime"] = ProfileShiftIn;
                                    dr["ShiftOutTime"] = ProfileShiftOut;

                                }
                                else if (RosterShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = RosterShift;
                                    dr["ShiftDuration"] = RosterShiftDurn;
                                    dr["ShiftInTime"] = RosterShiftIn;
                                    dr["ShiftOutTime"] = RosterShiftOut;

                                }

                                else if (BudgetShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = BudgetShift;
                                    dr["ShiftDuration"] = BudgetShiftDurn;
                                    dr["ShiftInTime"] = BudgetShiftIn;
                                    dr["ShiftOutTime"] = BudgetShiftOut;

                                }
                                #endregion

                                dr["ShiftHalfDayDuration"] = clsWebLib.RetValidLen(HalfDayDuration);
                                dr["ShiftShortDuration"] = clsWebLib.RetValidLen(ShortDuration);
                                dr["ShiftFullDayDuration"] = clsWebLib.RetValidLen(FullDayDuration);
                                dr["ShiftHoursWithoutOT"] = clsWebLib.RetValidLen(HoursWithoutOT);


                                #region  Not Nullable Columns default values

                                dr["WrongShift"] = 0;
                                dr["OTHr"] = "0";
                                dr["ProcessedOT"] = "0";
                                dr["CalculatedOT"] = 0;
                                dr["IsOTComfirm"] = 0;
                                dr["IsLock"] = 0;
                                dr["IsOTEntitled"] = 0;
                                dr["IsLWP"] = 0;
                                dr["IsOD"] = 0;
                                dr["IsHalfDayLeave"] = 0;
                                dr["OTIntime"] = "0";
                                dr["OTOuttime"] = "0";
                                dr["LeaveDuration"] = "0";
                                dr["ToReprocess"] = "No";
                                dr["AddedBy"] = "Schedule";
                                dr["DateAdded"] = Convert.ToDateTime(DateTime.Now);

                                #endregion

                                dsRef.Tables[0].Rows.Add(dr);

                            }
                            else
                            {
                                
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["ManualShiftID"] = clsWebLib.RetValidLen(ManualShift);
                                dr["RosterShiftID"] = clsWebLib.RetValidLen(RosterShift);
                                dr["ProfileShiftID"] = clsWebLib.RetValidLen(ProfileShift);
                                dr["BudgetedShiftID"] = clsWebLib.RetValidLen(BudgetShift);
                                if (string.IsNullOrEmpty(dr["BudgetId"].ToString()))
                                {
                                    dr["BudgetId"] = clsWebLib.RetValidLen(BudgetId); 
                                }
                                dr["Deployment"] = clsWebLib.RetValidLen(Deployment);
                                dr["BudgetedManpower"] = clsWebLib.RetValidLen(BudgetedManpower);
                                dr["RosterId"] = clsWebLib.RetValidLen(RosterId);
                                dr["PlantInPunchStartTime"] = clsWebLib.RetValidLen(PlantInPunchStartTime);
                                if (string.IsNullOrEmpty(dr["GivenDesignationId"].ToString()))
                                {
                                    dr["GivenDesignationId"] = clsWebLib.RetValidLen(GivenDesignationId); 
                                }
                                #region ManualData Entry
                                if (clsWebLib.RetValidLen(ManualInTime).ToString() != "")
                                {
                                    dr["ManualInTime"] = clsWebLib.RetValidLen(ManualInTime);
                                    dr["IsManualInTime"] = clsWebLib.GetBoolData(IsManualInTime);
                                    dr["OriginalManualInTime"] = clsWebLib.RetValidLen(ManualInTime);
                                }
                                if (clsWebLib.RetValidLen(ManualOutTime).ToString() != "")
                                {
                                    dr["ManualOutTime"] = clsWebLib.RetValidLen(ManualOutTime);
                                    dr["IsManualOutTime"] = clsWebLib.GetBoolData(IsManualOutTime);
                                    dr["OriginalManualOutTime"] = clsWebLib.RetValidLen(ManualOutTime);
                                }
                                if (clsWebLib.RetValidLen(ManualDayStatus).ToString() != "")
                                {
                                    dr["ManualDayStatus"] = clsWebLib.RetValidLen(ManualDayStatus);
                                    dr["IsManualDayStatus"] = clsWebLib.GetBoolData(IsManualDayStatus);
                                }
                                #endregion

                                #region AssignedShift Data
                                if (ManualShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = ManualShift;
                                    dr["ShiftDuration"] = ManualShiftDurn;
                                    dr["ShiftInTime"] = ManualShiftIn;
                                    dr["ShiftOutTime"] = ManualShiftOut;
                                }
                                else if (ProfileShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = ProfileShift;
                                    dr["ShiftDuration"] = ProfileShiftDurn;
                                    dr["ShiftInTime"] = ProfileShiftIn;
                                    dr["ShiftOutTime"] = ProfileShiftOut;

                                }
                                else if (RosterShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = RosterShift;
                                    dr["ShiftDuration"] = RosterShiftDurn;
                                    dr["ShiftInTime"] = RosterShiftIn;
                                    dr["ShiftOutTime"] = RosterShiftOut;

                                }

                                else if (BudgetShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = BudgetShift;
                                    dr["ShiftDuration"] = BudgetShiftDurn;
                                    dr["ShiftInTime"] = BudgetShiftIn;
                                    dr["ShiftOutTime"] = BudgetShiftOut;

                                }
                                #endregion

                                dr["ShiftHalfDayDuration"] = clsWebLib.RetValidLen(HalfDayDuration);
                                dr["ShiftShortDuration"] = clsWebLib.RetValidLen(ShortDuration);
                                dr["ShiftFullDayDuration"] = clsWebLib.RetValidLen(FullDayDuration);
                                dr["ShiftHoursWithoutOT"] = clsWebLib.RetValidLen(HoursWithoutOT);

                                dr.EndEdit();

                            }

                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region Shift Not Assigned Employee
                    DataSet ShiftNotAssigned;
                    TopShift(out ShiftNotAssigned, PlantValue); // Getting Top Shift of Plant
                    if (ShiftNotAssigned.Tables[0].Rows.Count > 0)
                    {
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");

                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + Date + "' and isnull(ShiftSystemID,'')='' and PlantID ='" + PlantValue + "' ";
                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        var ShiftDurn = clsWebLib.RetValidLen(ShiftNotAssigned.Tables[0].Rows[0][@"ShiftDuration"]).ToString();
                        var ShiftId = clsWebLib.RetValidLen(ShiftNotAssigned.Tables[0].Rows[0][@"SystemID"]).ToString();
                        var ShiftIn = clsWebLib.RetValidLen(ShiftNotAssigned.Tables[0].Rows[0][@"InTime"]).ToString();
                        var ShiftOut = clsWebLib.RetValidLen(ShiftNotAssigned.Tables[0].Rows[0][@"OutTime"]).ToString();
                        var FullDayDuration = clsWebLib.RetValidLen(ShiftNotAssigned.Tables[0].Rows[0][@"FullDayDuration"]).ToString();
                        var HalfDayDuration = clsWebLib.RetValidLen(ShiftNotAssigned.Tables[0].Rows[0][@"HalfDayDuration"]).ToString();
                        var ShortDuration = clsWebLib.RetValidLen(ShiftNotAssigned.Tables[0].Rows[0][@"ShortDuration"]).ToString();
                        var HoursWithoutOT = clsWebLib.RetValidLen(ShiftNotAssigned.Tables[0].Rows[0][@"HoursWithoutOT"]).ToString();
                        ShiftTime(ref ShiftIn, ref ShiftOut, Date);

                        string EmpSet = "''";
                        // Setting default Shift of Employees Whom Shift Not Assigned
                        if (dsRef.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                            {

                                EmpSet += ",'" + dsRef.Tables[0].Rows[i][@"RowId"].ToString() + "'";

                            }
                        }

                        var sql = @"update AttdnProcessData set ShiftSystemID='" + ShiftId + @"',ShiftDuration='" + ShiftDurn + @"',ShiftInTime='" + ShiftIn + @"',
                                           ShiftOutTime='" + ShiftOut + @"',ShiftHalfDayDuration='" + HalfDayDuration + @"',ShiftShortDuration='" + ShortDuration + @"',
                                           ShiftFullDayDuration='" + FullDayDuration + @"',ShiftHoursWithoutOT='" + HoursWithoutOT + @"' where RowId 
                                           IN(" + EmpSet + ")";

                        ConnectionManager.DAL.ConManager objCone = null;
                        objCone = new ConnectionManager.DAL.ConManager("1");
                        objCone.OpenConnection("1");
                        objCone.BeginTransaction();

                        objCone.ExecuteNonQueryWrapper(sql, true, "1");
                        objCone.CommitTransaction();




                    }
                    #endregion

                    #region Ramadan Shift Flagging
                    DataSet RamadanShift;
                    ChangedShift(Date, out RamadanShift, PlantValue); // Building Dataset for Ramadan Shift Days
                    if (RamadanShift.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = RamadanShift.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID ='" + PlantValue + "' ";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < RamadanShift.Tables[0].Rows.Count; i++)
                        {
                             EmpId = RamadanShift.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                            var ShiftDurn = clsWebLib.RetValidLen(RamadanShift.Tables[0].Rows[i][@"ShiftDuration"]).ToString();
                            var ShiftIn = clsWebLib.RetValidLen(RamadanShift.Tables[0].Rows[i][@"InTime"]).ToString();
                            var ShiftOut = clsWebLib.RetValidLen(RamadanShift.Tables[0].Rows[i][@"OutTime"]).ToString();
                            var FullDayDuration = clsWebLib.RetValidLen(RamadanShift.Tables[0].Rows[i][@"FullDayDuration"]).ToString();
                            var HalfDayDuration = clsWebLib.RetValidLen(RamadanShift.Tables[0].Rows[i][@"HalfDayDuration"]).ToString();
                            var ShortDuration = clsWebLib.RetValidLen(RamadanShift.Tables[0].Rows[i][@"ShortDuration"]).ToString();
                            var HoursWithoutOT = clsWebLib.RetValidLen(RamadanShift.Tables[0].Rows[i][@"HoursWithoutOT"]).ToString();
                            ShiftTime(ref ShiftIn, ref ShiftOut, WorkDate);

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";

                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                // Updating Exisiting Shift Localized Data with Ramadan Shift Info
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["ShiftDuration"] = ShiftDurn;
                                dr["ShiftInTime"] = Convert.ToDateTime(ShiftIn);
                                dr["ShiftOutTime"] = Convert.ToDateTime(ShiftOut);
                                dr["ShiftHalfDayDuration"] = HalfDayDuration;
                                dr["ShiftShortDuration"] = ShortDuration;
                                dr["ShiftFullDayDuration"] = FullDayDuration;
                                dr["ShiftHoursWithoutOT"] = HoursWithoutOT;
                                dr["UpdatedBy"] = "Schedule";
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);

                                dr.EndEdit();
                            }


                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region HolidayData Flagging
                    DataSet Holiday;
                    HolidayData(Date, out Holiday, PlantValue);
                    if (Holiday.Tables[0].Rows.Count > 0)
                    {
                        // Updating Holiday Staus of Entire Plant If Holiday Exists
                        string WorkDate = Holiday.Tables[0].Rows[0][@"WorkDate"].ToString();


                        var sql = @"Update AttdnProcessData Set HolidayStatus='H' 
                                      where PlantID='" + PlantValue + "' and WorkDate='" + WorkDate + "'";

                        ConnectionManager.DAL.ConManager objCone = null;
                        objCone = new ConnectionManager.DAL.ConManager("1");
                        objCone.OpenConnection("1");
                        objCone.BeginTransaction();

                        objCone.ExecuteNonQueryWrapper(sql, true, "1");
                        objCone.CommitTransaction();


                    }

                    #endregion

                    #region LeaveData Flagging
                    DataSet Leavedata;
                    LeaveData(Date, out Leavedata, PlantValue); // Building Leave DataSet of Employees 
                    if (Leavedata.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = Leavedata.Tables[0].Rows[0][@"WorkDate"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID ='" + PlantValue + "' ";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < Leavedata.Tables[0].Rows.Count; i++)
                        {
                            string RowId = Leavedata.Tables[0].Rows[i][@"RowId"].ToString();
                            string LTSystemID = Leavedata.Tables[0].Rows[i][@"LTSystemID"].ToString();
                            decimal LeaveDuration = Convert.ToDecimal(Leavedata.Tables[0].Rows[i][@"LeaveDuration"].ToString());
                            string LeaveStatus = Leavedata.Tables[0].Rows[i][@"Code"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";

                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                // Updations in APD Table
                                dr["LeaveDuration"] = LeaveDuration;
                                dr["LTSystemID"] = clsWebLib.RetValidLen(LTSystemID);
                                dr["LeaveStatus"] = clsWebLib.RetValidLen(LeaveStatus);
                                dr["UpdatedBy"] = "Schedule";
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);
                        // IsAvail Flag Update Logic
                        #region Update in LeaveTransactionDetail
                        LeaveAvailUpdate(Date, PlantValue);
                        #endregion
                    }
                    #endregion

                    #region Maternity LeaveData Flagging
                    DataSet MaternityLeavedata;
                    MaternityLeaveData(Date, out MaternityLeavedata, PlantValue); // Building Maternity Leave DataSet of Employees 
                    if (MaternityLeavedata.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = MaternityLeavedata.Tables[0].Rows[0][@"WorkDate"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID ='" + PlantValue + "' ";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < MaternityLeavedata.Tables[0].Rows.Count; i++)
                        {
                            string RowId = MaternityLeavedata.Tables[0].Rows[i][@"RowId"].ToString();
                            string LTSystemID = MaternityLeavedata.Tables[0].Rows[i][@"LTSystemID"].ToString();
                            decimal LeaveDuration = Convert.ToDecimal(MaternityLeavedata.Tables[0].Rows[i][@"LeaveDuration"].ToString());
                            string LeaveStatus = MaternityLeavedata.Tables[0].Rows[i][@"Code"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";

                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                // Updations in APD Table
                                dr["LeaveDuration"] = LeaveDuration;
                                dr["LTSystemID"] = clsWebLib.RetValidLen(LTSystemID);
                                dr["LeaveStatus"] = clsWebLib.RetValidLen(LeaveStatus);
                                dr["UpdatedBy"] = "Schedule";
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region WeekOff Flagging
                    DataSet dsRosterWeekOff;
                    DataSet IndividualWeekOff;
                    DataSet CompanyWeekOff;
                    DataSet dsRefApd;
                    CompanyWeekOffData(Date, out CompanyWeekOff, PlantValue);
                    IndividualWeekOffData(Date, out IndividualWeekOff, PlantValue);
                    RosterWeekOffData(Date, out dsRosterWeekOff, PlantValue);
                    ConnectionManager.DAL.ConManager objConR = new ConnectionManager.DAL.ConManager("1");
                    if (dsRosterWeekOff.Tables[0].Rows.Count > 0)
                    {
                       

                        // Employee Week Off DataSet Generation
                        var sqlx = @"select * from AttdnProcessData 
                                   WHERE WorkDate='" + Date + @"'
                                    AND isnull(EmpSystemID,'') IN (SELECT isnull(ei.SystemId,'') 
                                    FROM EmployeeInformation AS ei WHERE  ei.PlantId='" + PlantValue + @"') ";

                        objConR.OpenDataSetThroughAdapter(sqlx, out dsRefApd, false, false, "", "1");
                        string newformat = Convert.ToDateTime(Date).ToString("yyyyMMdd");

                        for (int r = 0; r < dsRefApd.Tables[0].Rows.Count; r++)
                        {
                            EmpId = dsRefApd.Tables[0].Rows[r][@"EmpSystemId"].ToString();

                            DataView dv = new DataView(dsRosterWeekOff.Tables[0]);
                            dv.RowFilter = "EmpSystemId = '" + EmpId + "'";
                            string DayType = null;
                            if (dv.Count > 0)
                            {
                                DayType = clsWebLib.RetValidLen(dv[0]["DayType"]).ToString();
                            }
                            else
                            {
                                DayType = string.Empty; // or default value
                            }

                            if (EmpId == "25254653")
                            {

                            }
                            if (!string.IsNullOrEmpty(DayType))
                            {
                                dsRefApd.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                if (dsRefApd.Tables[0].DefaultView.Count > 0)
                                {
                                    // Week Off Updation in APD Level
                                    if (DayType.ToString() != "")
                                    {
                                        DataRow dr = dsRefApd.Tables[0].DefaultView[0].Row;
                                        dr.BeginEdit();
                                        dr["UpdatedBy"] = "Schedule";
                                        dr["WeeklyStatus"] = DayType;
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                    }
                                }
                            }
                            else if(IndividualWeekOff.Tables[0].Rows.Count > 0)
                            {
                                DataView dvi = new DataView(IndividualWeekOff.Tables[0]);
                                dvi.RowFilter = "SystemId = '"+ EmpId + "'";
                                if (dvi.Count > 0)
                                {
                                    DayType = clsWebLib.RetValidLen(dvi[0]["DayType"]).ToString();
                                }
                                else
                                {
                                    DayType = string.Empty; // or default value
                                }

                                        dsRefApd.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                        if (dsRefApd.Tables[0].DefaultView.Count > 0)
                                        {
                                            // Week Off Updation in APD Level
                                            if (DayType.ToString() != "")
                                            {
                                                DataRow dr = dsRefApd.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["UpdatedBy"] = "Schedule";
                                                dr["WeeklyStatus"] = DayType;
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                            }
                                        }
                                   
                                    SaveDataSets(dsRefApd);
                            }
                            else  
                            {
                                if (CompanyWeekOff.Tables[0].Rows.Count > 0)
                                {

                                    for (int c = 0; c < CompanyWeekOff.Tables[0].Rows.Count; c++)
                                    {
                                        // Company WeekOff Employees Weekly Status Updation to W 
                                        string PlantId = CompanyWeekOff.Tables[0].Rows[c][@"PlantId"].ToString();
                                        string WkDate = CompanyWeekOff.Tables[0].Rows[c][@"WkDate"].ToString();

                                        var sql = @"Update AttdnProcessData Set WeeklyStatus='W'  
                                           WHERE WorkDate='" + WkDate + "'AND isnull(EmpSystemID,'') IN" +
                                        " (SELECT isnull(ei.SystemId,'')   FROM EmployeeInformation AS " +
                                        "ei WHERE  ei.PlantId ='" + PlantId + "' AND ei.DOJ <= '" + Date + "' AND (ei.DOS >= '" + Date + "' OR ISNULL(ei.DOS,'') = '' OR ei.DOS = '01/01/1901')" +
                                        "and  ISNULL(EmpSystemID,'') not in (select distinct ISNULL(EmpSystemID,'') " +
                                        "from EmployeeWeeklyOff where EffectiveDate<='" + WkDate + "'))";


                                        ConnectionManager.DAL.ConManager objCone = null;
                                        objCone = new ConnectionManager.DAL.ConManager("1");
                                        objCone.OpenConnection("1");
                                        objCone.BeginTransaction();

                                        objCone.ExecuteNonQueryWrapper(sql, true, "1");
                                        objCone.CommitTransaction();

                                    }
                                }
                                else
                                {
                                    // Company WeekOff Employees Weekly Status Updation to NW 

                                    var sql = @"Update AttdnProcessData Set WeeklyStatus='NW'  
                                          WHERE WorkDate='" + Date + @"' AND isnull(EmpSystemID,'') IN" +
                                       " (SELECT isnull(ei.SystemId,'')   FROM EmployeeInformation AS " +
                                       "ei WHERE  ei.PlantId='" + PlantValue + "'  and ei.DOJ <= '" + Date + "' AND (ei.DOS >= '" + Date + "' OR ISNULL(ei.DOS,'') = '' OR ei.DOS = '01/01/1901')" +
                                       "and  ISNULL(EmpSystemID,'') not in (select distinct ISNULL(EmpSystemID,'') " +
                                       "from EmployeeWeeklyOff where EffectiveDate<='" + Date + "'))";


                                    ConnectionManager.DAL.ConManager objCone = null;
                                    objCone = new ConnectionManager.DAL.ConManager("1");
                                    objCone.OpenConnection("1");
                                    objCone.BeginTransaction();

                                    objCone.ExecuteNonQueryWrapper(sql, true, "1");
                                    objCone.CommitTransaction();
                                }
                            }
                            
                            
                        }
                        SaveDataSets(dsRefApd);
                    }

                    #endregion

                  
                    
                    
 

                    #region Compensatory Logic
                    DataSet OriginalDateComp;
                    OriginalDateData(Date, out OriginalDateComp, PlantValue);
                    if (OriginalDateComp.Tables[0].Rows.Count > 0)
                    {
                        // Getting DayCode from Screen and flagging it 
                        string OCompensatory = "", DayCode = "";
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");

                        string WkDate = OriginalDateComp.Tables[0].Rows[0][@"WkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                        var sqlx = @"SELECT * FROM AttdnProcessData where 
                                   WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < OriginalDateComp.Tables[0].Rows.Count; i++)
                        {
                            string Plant = OriginalDateComp.Tables[0].Rows[i][@"PlantId"].ToString();
                            string ForEntirePlant = clsWebLib.GetBoolData(OriginalDateComp.Tables[0].Rows[i][@"ForEntirePlant"]).ToString();
                            DayCode = clsWebLib.RetValidLen(OriginalDateComp.Tables[0].Rows[i][@"DayCode"]).ToString();
                             EmpId = clsWebLib.RetValidLen(OriginalDateComp.Tables[0].Rows[i][@"EmpSystemId"]).ToString();

                            if (ForEntirePlant == "True")
                            {
                                OCompensatory = "1";

                            }
                            else
                            {
                                if (DayCode != "")
                                {
                                    dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                    if (dsRef.Tables[0].DefaultView.Count > 0)
                                    {
                                        DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                        dr.BeginEdit();
                                        dr["UpdatedBy"] = "Schedule";
                                        dr["ManualDayStatus"] = DayCode;
                                        dr["IsManualDayStatus"] = 1;
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                    }
                                }
                            }
                        }
                        SaveDataSets(dsRef);

                        #region Entire Plant Flagging Exceptional Case
                        if (OCompensatory == "1")
                        {
                            if (DayCode != "")
                            {
                                // If Entire Plant on Compensatory
                                var sql = @"Update AttdnProcessData Set IsManualDayStatus=1,ManualDayStatus='" + DayCode + @"'    
                                             WHERE WorkDate='" + WkDate + "' AND " +
                              "isnull(EmpSystemID,'') IN" +
                              " (SELECT isnull(ei.SystemId,'')   FROM EmployeeInformation AS " +
                              "  ei WHERE  ei.PlantId ='" + PlantValue + "' AND ei.DOJ <= '" + Date + "' AND (ei.DOS >= '" + Date + "' OR ISNULL(ei.DOS,'') = '' OR ei.DOS = '01/01/1901'))";


                                ConnectionManager.DAL.ConManager objCone = null;
                                objCone = new ConnectionManager.DAL.ConManager("1");
                                objCone.OpenConnection("1");
                                objCone.BeginTransaction();

                                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                                objCone.CommitTransaction();
                            }

                        }

                        #endregion

                    }
                    #endregion

                    #region OTEligibleData Flagging
                    DataSet OTElgbEmp;
                    OTEligibleEmp(Date, out OTElgbEmp, PlantValue); // OT Eligible DataSet Generation
                    if (OTElgbEmp.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = OTElgbEmp.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < OTElgbEmp.Tables[0].Rows.Count; i++)
                        {
                             EmpId = OTElgbEmp.Tables[0].Rows[i][@"EmpId"].ToString();
                            string IsOTEntitled = OTElgbEmp.Tables[0].Rows[i][@"IsOTEntitled"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                // Updation in APD Table for OT Entitled Employees
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["IsOTEntitled"] = clsWebLib.GetBoolData(IsOTEntitled);
                                dr["UpdatedBy"] = "Schedule";
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region OnDuty Data Flagging
                    DataSet OnDuty;
                    OnDutyData(Date, out OnDuty, PlantValue);
                    if (OnDuty.Tables[0].Rows.Count > 0) // On Duty Employees Flagging in Manual DayStatus
                    {
                        string WorkDate = OnDuty.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < OnDuty.Tables[0].Rows.Count; i++)
                        {
                             EmpId = OnDuty.Tables[0].Rows[i][@"EmpSystemId"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["IsOD"] = 1;
                                dr["IsManualDayStatus"] = true;
                                dr["ManualDayStatus"] = "OD";
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region OnRest Data Flagging
                    DataSet OnRest;
                    OnRestData(Date, out OnRest, PlantValue);
                    if (OnRest.Tables[0].Rows.Count > 0)
                    {
                        // On Rest Employees Flagging in Manual DayStatus
                        string WorkDate = OnRest.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < OnRest.Tables[0].Rows.Count; i++)
                        {
                             EmpId = OnRest.Tables[0].Rows[i][@"EmpSystemId"].ToString();
                            string RestId = clsWebLib.RetValidLen(OnRest.Tables[0].Rows[i][@"RestId"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["AttendanceRestDetailId"] = RestId;
                                dr["ManualDayStatus"] = "RST";
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region OT Month Year Week Localization
                    DataSet OTWeek;
                    OTWeekLocalizationData(Date, out OTWeek);
                    if (OTWeek.Tables[0].Rows.Count > 0) // OT Week Month Year DataSet Generation
                    {
                        // Data From Week Defination
                        var Month = clsWebLib.RetValidLen(OTWeek.Tables[0].Rows[0][@"Month"]).ToString();
                        var Year = clsWebLib.RetValidLen(OTWeek.Tables[0].Rows[0][@"Year"]).ToString();
                        var NoOfDaysInMonth = clsWebLib.RetValidLen(OTWeek.Tables[0].Rows[0][@"NoOfDaysInMonth"]).ToString();
                        var Pattern28 = clsWebLib.RetValidLen(OTWeek.Tables[0].Rows[0][@"Pattern28"]).ToString();
                        var Pattern29 = clsWebLib.RetValidLen(OTWeek.Tables[0].Rows[0][@"Pattern29"]).ToString();
                        var Pattern30 = clsWebLib.RetValidLen(OTWeek.Tables[0].Rows[0][@"Pattern30"]).ToString();
                        var Pattern31 = clsWebLib.RetValidLen(OTWeek.Tables[0].Rows[0][@"Pattern31"]).ToString();

                        var sql = @"";
                        // WeekNo Depending on the No Of Days In Month
                        if (NoOfDaysInMonth == "28" && Pattern28 != "")
                        {
                            sql = "update AttdnProcessData set otmonth = '" + Month + "', OTYear = '" + Year + "', OTWeek = '" + Pattern28 + "' " +
                                "where WorkDate = '" + Date + "' and PlantID = '" + PlantValue + "'";
                        }
                        else if (NoOfDaysInMonth == "29" && Pattern29 != "")
                        {
                            sql = "update AttdnProcessData set otmonth = '" + Month + "', OTYear = '" + Year + "', OTWeek = '" + Pattern29 + "' " +
                               "where WorkDate = '" + Date + "' and PlantID = '" + PlantValue + "'";
                        }
                        else if (NoOfDaysInMonth == "30" && Pattern30 != "")
                        {
                            sql = "update AttdnProcessData set otmonth = '" + Month + "', OTYear = '" + Year + "', OTWeek = '" + Pattern30 + "' " +
                               "where WorkDate = '" + Date + "' and PlantID = '" + PlantValue + "'";
                        }
                        else if (NoOfDaysInMonth == "31" && Pattern31 != "")
                        {
                            sql = "update AttdnProcessData set otmonth = '" + Month + "', OTYear = '" + Year + "', OTWeek = '" + Pattern31 + "' " +
                               "where WorkDate = '" + Date + "' and PlantID = '" + PlantValue + "'";
                        }

                        if (sql != "")
                        {
                            #region Update Entire Plant Rows
                            OTUpdateinAPD(sql);
                            #endregion
                        }

                    }
                    #endregion

                    #region DayStatusHeader & LeavePolicy MasterId Localization
                    DataSet HeaderPolicy;
                    LocalizingHeaderValue(Date, out HeaderPolicy, PlantValue);
                    // DataSet Generation from Employee Category

                    if (HeaderPolicy.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = HeaderPolicy.Tables[0].Rows[0][@"WorkDate"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < HeaderPolicy.Tables[0].Rows.Count; i++)
                        {
                             EmpId = clsWebLib.RetValidLen(HeaderPolicy.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                            string HeaderId = clsWebLib.RetValidLen(HeaderPolicy.Tables[0].Rows[i][@"HeaderId"]).ToString();
                            string LeavePolicyId = clsWebLib.RetValidLen(HeaderPolicy.Tables[0].Rows[i][@"LeavePolicyMasterId"]).ToString();

                            if (HeaderId != "")
                            {
                                // HeaderId & LeavePolicy MasterId Localizing in APD
                                dsRef.Tables[0].DefaultView.RowFilter = @"EmpSystemID='" + EmpId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {
                                    string HeaderMaster = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"DayStatusHeaderId"]).ToString();
                                    if (HeaderMaster == "")
                                    {
                                        DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                        dr.BeginEdit();
                                        // Data Found using Plant & Employee Category
                                        dr["DayStatusHeaderId"] = HeaderId;
                                        if (LeavePolicyId != "")
                                        {
                                            dr["LeavePolicyMasterId"] = LeavePolicyId;
                                        }
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                    }
                                }
                            }
                        }
                        SaveDataSets(dsRef);
                    }
                    #endregion

                    #region IsTbs & IsLA Localizing
                    
                    DataSet TBS_LA_Data;
                    TBS_LA_Localizing(Date, out TBS_LA_Data, PlantValue);
                   
                    if (TBS_LA_Data.Tables[0].Rows.Count > 0)
                    {
                      
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + Date + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        DateTime Now = Convert.ToDateTime(DateTime.Now.ToString("yyyyy-MMM-dd"));

                        if (Now <= Convert.ToDateTime(Date))
                        {

                            for (int i = 0; i < TBS_LA_Data.Tables[0].Rows.Count; i++)
                            {
                                 EmpId = clsWebLib.RetValidLen(TBS_LA_Data.Tables[0].Rows[i][@"SystemId"]).ToString();
                                string IsLa = clsWebLib.GetBoolData(TBS_LA_Data.Tables[0].Rows[i][@"IsLA"]).ToString();
                                string IsTBS = clsWebLib.GetBoolData(TBS_LA_Data.Tables[0].Rows[i][@"IsTBS"]).ToString();

                                dsRef.Tables[0].DefaultView.RowFilter = @"EmpSystemID='" + EmpId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {
                                    string LA = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"IsLongAbsentism"]).ToString();
                                    string TBS = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"IsTBS"]).ToString();
                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();
                                    dr["IsLongAbsentism"] = IsLa;
                                    dr["IsTBS"] = IsTBS;
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();
                                }
                            }
                            SaveDataSets(dsRef);
                        }
                    }

                    #endregion

                    #region WorkDate Wise Budget Summary

                    DataSet BudgetSummary_Data;
                    WorkDate_Wise_BudgetSaving(Date, out BudgetSummary_Data, PlantValue);

                    if (BudgetSummary_Data.Tables[0].Rows.Count > 0)
                    {

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnWorkdateWiseBudget where WorkDate='" + Date + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < BudgetSummary_Data.Tables[0].Rows.Count; i++)
                        {
                            string BudgetId = clsWebLib.RetValidLen(BudgetSummary_Data.Tables[0].Rows[i][@"BudgetId"]).ToString();
                            string Total = clsWebLib.RetValidLen(BudgetSummary_Data.Tables[0].Rows[i][@"Total"]).ToString();
                            string Deployment = clsWebLib.RetValidLen(BudgetSummary_Data.Tables[0].Rows[i][@"Deployment"]).ToString();


                            dsRef.Tables[0].DefaultView.RowFilter = @"BudgetId='" + BudgetId + "' AND WorkDate =#" + Date + "#";
                            if (dsRef.Tables[0].DefaultView.Count == 0)
                            {
                                // Row Creation in AttdnWorkdateWiseBudget
                                DataRow dr = dsRef.Tables[0].NewRow();
                                clsGenID genid = new clsGenID();
                                genid.GenID("AttdnWorkdateWiseBudget", out string _Id);

                                dr["Id"] = "AB" + _Id;
                                dr["PlantId"] = PlantValue;
                                dr["BudgetId"] = BudgetId;
                                dr["WorkDate"] = Date;
                                dr["TotalNumber"] = Total;
                                dr["Deployment"] = Deployment;
                                dr["AddedBy"] = "Schedule";
                                dr["AddedDate"] = Convert.ToDateTime(DateTime.Now);
                                dr["AddedFromIP"] = "1";
                                dsRef.Tables[0].Rows.Add(dr);

                            }
                        }
                        SaveDataSets(dsRef);
                    }


                    #endregion
                    
                }
                _lock.UnlockProcess();
            }
            catch (Exception ex)
            {
                EmpId.ToString();
                _lock.UnlockProcess();
                throw ex;
            }
        }
        #endregion

        #region ShiftProcess SourceData
     
        void UnProcessedEmp(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string newformat = Convert.ToDateTime(Date).ToString("yyyyMMdd");

                var sql = @"select TobeAdded=case When isnull(p.EmpSystemID,'') ='' then 'true' 
                else 'false' end ,e.SystemId,'"+Date+@"' as WorkDate,
                convert(varchar(30),'"+newformat+ @"' )+convert(varchar(30), e.SystemId)RowId,
				e.PlantId,e.GroupID,
                isnull(m.ShiftSystemId,p.ManualShiftID) 
                as ManualShift,ISNULL(sd.InTime,p.ShiftInTime) as ManualShiftIn,
				isnull(sd.OutTime,p.ShiftOutTime) as ManualShiftOut,isnull(sd.ShiftDuration,p.ShiftDuration)
				as ManualDuration,
                sdmaster.SystemID as ProfileShift,sdmaster.InTime as ProfileShiftIn,sdmaster.OutTime as ProfileShiftOut,
                sdmaster.ShiftDuration as ProfileDuration,
                mb.ShiftDefinationId as BudgetedShift,sdy.InTime as BudgetShiftIn,sdy.OutTime as BudgetShiftOut,
                sdy.ShiftDuration as BudgetDuration,rp.ShiftDefinationID as RosterShift,sdz.InTime as RosterShiftIn,
                sdz.OutTime as RosterShiftOut,sdz.ShiftDuration as RosterDuration,m.InTime as ManualInTime,m.OutTime as ManualOutTime,
                m.DayStatus as ManualDayStatus,IsManualDayStatus=case When isnull(m.DayStatus,'') ='' then 'false' 
                else 'true' end,IsManualInTime=case When isnull(m.InTime,'') ='' then 'false' 
                else 'true' end,IsManualOutTime=case When isnull(m.OutTime,'') ='' then 'false' 
                else 'true' end,mb.Id as BudgetId,mb.Deployment,MBD.TotalNumber BudgetedManpower,rh.Id as RosterId,e.GivenDesignationId,Op.InPunchStartTime as PlantInPunchStartTime, 
                FullDayDuration=case when isnull(p.ManualShiftID,'')!='' then
				isnull(isnull(isnull(sd.FullDayDuration,p.ShiftFullDayDuration),
				sdmaster.FullDayDuration),isnull(sdz.FullDayDuration,sdy.FullDayDuration)) 
				else 
				isnull(isnull(sd.FullDayDuration,sdmaster.FullDayDuration),
                isnull(sdz.FullDayDuration,sdy.FullDayDuration))end,				
				HalfDayDuration=case when isnull(p.ManualShiftID,'')!='' then
				isnull(isnull(isnull(sd.HalfDayDuration,p.ShiftHalfDayDuration),sdmaster.HalfDayDuration),
                isnull(sdz.HalfDayDuration,sdy.HalfDayDuration))
				else
				isnull(isnull(sd.HalfDayDuration,sdmaster.HalfDayDuration),
                isnull(sdz.HalfDayDuration,sdy.HalfDayDuration))
				end,
				ShortDuration= case when isnull(p.ManualShiftID,'')!='' then 
				isnull(isnull(isnull(sd.ShortDuration,p.ShiftShortDuration),sdmaster.ShortDuration),
                isnull(sdz.ShortDuration,sdy.ShortDuration)) else
				isnull(isnull(sd.ShortDuration,sdmaster.ShortDuration),
                isnull(sdz.ShortDuration,sdy.ShortDuration)) end,
				HoursWithoutOT=case when isnull(p.ManualShiftID,'')!='' then 
				isnull(isnull(isnull(sd.HoursWithoutOT,p.ShiftHoursWithoutOT),sdmaster.HoursWithoutOT),
                isnull(sdz.HoursWithoutOT,sdy.HoursWithoutOT)) 
				else
				isnull(isnull(sd.HoursWithoutOT,sdmaster.HoursWithoutOT),
                isnull(sdz.HoursWithoutOT,sdy.HoursWithoutOT))end
		        from EmployeeInformation e 
                left outer join AttndManualDataFromApp m on e.SystemId=m.EmpSystemID and
				m.WorkDate='" + Date+@"'
                left join ShiftDefination sd on sd.SystemID=m.ShiftSystemId
                left join AttdnProcessData p on p.EmpSystemID=e.SystemId and p.WorkDate='"+Date+ @"'
                left join mst.ManpowerBudget mb on mb.Id=e.BudgetCode
                LEFT JOIN MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId=mb.Id 
				AND MBD.Id=(Select top(1) Id From MST.ManpowerBudgetDetail Where ManpowerBudgetId=mb.Id Order By EffectiveDate DESC)
                left join ShiftDefination sdy on sdy.SystemID=mb.ShiftDefinationId
                left join dbo.RosterBudget rb on rb.BudgetId=mb.Id 
                left join RosterPatternHeader rh on rh.Id=rb.RosterId
                left join dbo.RosterPatternProcess rp on rp.RPHeaderId=rh.Id and rp.WorkDate='" + Date+@"'
                left join ShiftDefination sdz on sdz.SystemID=rp.ShiftDefinationID
                left join org.Plant pl on pl.Id=e.PlantId
                left join OutPunchConfigurationHeader Op on OP.PlantId=pl.Id
				left join (select distinct es.EmpSystemId,(Select top 1 ShiftId
				from dbo.EmployeeProfileShift
				where EmpSystemId = es.EmpSystemId and EffectiveDate <= '"+Date+@"'
				order by EffectiveDate desc
				) as ShiftId 
				from dbo.EmployeeProfileShift es
				where EffectiveDate <= '"+Date+@"') as Tablex on Tablex.EmpSystemId=e.SystemId
				left join ShiftDefination sdmaster on sdmaster.SystemID=Tablex.ShiftId
                where e.EmpType!='Guest' and e.PlantId='"+PlantId+@"' 
and	E.DOJ <= '"+Date+@"' AND (E.DOS >= '"+Date+ @"' OR ISNULL(E.DOS,'') = '' OR E.DOS = '01/01/1901')
";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void OTEligibleEmp(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sql = @"select distinct e.SystemId as EmpId,dc.IsOTEntitled,
				Format(p.WorkDate,'yyyy-MMM-dd')WorkDate
                from AttdnProcessData p join
                EmployeeInformation e on e.SystemId=p.EmpSystemID    
				left join mst.DesignationMasterLegalDesignation ddm on 
                ddm.LegalDesignationId = e.LegalDesignationId
                left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
				left join scs.DesignationMasterConfiguration dc on dc.DesignationMasterId=dm.Id
                and dc.PlantId=e.PlantId
                where p.WorkDate='" + Date + @"' and 
				e.PlantId='" + PlantId + @"' 
                and E.DOJ <= '" + Date + @"' 
				AND (E.DOS >= '" + Date + @"' OR ISNULL(E.DOS,'') = '' 
				OR E.DOS = '01/01/1901') and dc.IsOTEntitled=1 
				and e.ExcludeOT=0";   
                                
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void LeaveData(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct FORMAT(D.WorkDate,'yyyy-MMM-dd')WorkDate,
                    LT.PlantID,LT.EmpSystemID,
                    Format(D.WorkDate,'yyyyMMdd')+LT.EmpSystemID AS RowId, 
                    D.LeaveDuration,lt.LTSystemID,
                    Case when D.LeaveDuration = '0.5' then  ('HD' + LTP.Code) else	LTP.Code end Code
                    from LeaveTransactionDetails D 
                    LEFT JOIN LeaveTransaction LT ON LT.SystemID=D.LvTrnsSystemID
					left join LeaveType ltp on ltp.Id=LT.LTSystemID
                    WHERE LT.PlantID = '" + PlantId + @"' AND D.WorkDate='" + Date + @"'
                    and LT.IsApproved=1";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void MaternityLeaveData(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct FORMAT(D.WorkDate,'yyyy-MMM-dd')WorkDate,
                    LT.PlantID,LT.EmpSystemID,
                    Format(D.WorkDate,'yyyyMMdd')+LT.EmpSystemID AS RowId, 
                    D.LeaveDuration,lt.LTSystemID,(LTP.Code+'WOB') AS Code
                    from LeaveTransactionDetails D 
                    LEFT JOIN LeaveTransaction LT ON LT.SystemID=D.LvTrnsSystemID
					left join LeaveType ltp on ltp.Id=LT.LTSystemID
					LEFT JOIN [MST].[MaternityLeavePolicy] MP ON MP.Id=LT.MaternityLeavePolicyId
                    WHERE LT.PlantID = '"+PlantId+@"' AND ISNULL(MP.IsNoBenefit,'')='1'
					AND D.WorkDate='"+Date+@"' 
                    and LT.IsApproved=1";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void IndividualWeekOffData(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sql = @"Select dd.*,
                (Select wcc.DayType from
                dbo.WeekOffChild wcc where wcc.WOSequence =dd.DayDiff 
                and wcc.WOHeaderId = dd.WeekOffHeaderId) 
                as DayType
                from
                (Select e.SystemId,
                
                (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                where EmpSystemId = e.SystemId and ex.EffectiveDate<='" + Date + @"'
                order by ex.EffectiveDate desc) WeekOffHeaderId,

                (DATEDIFF(DAY, (Select top 1 ed.EffectiveDate from
                dbo.WeekOffHeader h 
                left join dbo.WeekOffEffectiveDate ed on ed.WOHeaderId = h.Id               
                where ed.EffectiveDate <= '" + Date + @"' and ed.WOHeaderId =  
				(Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                where EmpSystemId = e.SystemId and ex.EffectiveDate<='" + Date + @"'
                order by ex.EffectiveDate desc)
                order by ed.EffectiveDate desc) , '" + Date + @"') % 
                (Select max(WOSequence) from WeekOffHeader h 
                left join WeekOffChild wc on wc.WOHeaderId=h.Id 
                where h.Id =  
				(Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                where EmpSystemId = e.SystemId and ex.EffectiveDate<='" + Date + @"'
                order by ex.EffectiveDate desc)
				)
				)+1 as DayDiff
                from 
                EmployeeInformation e
                left join EmployeeWeeklyOff ex on e.SystemId=ex.EmpSystemId
                where e.SystemId in( select empsystemid from EmployeeWeeklyOff)
                and e.PlantId='" + Plant + @"'
                group by e.SystemId
                ) as dd	";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void CompanyWeekOffData(string Date, out DataSet ds, string plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct odd.DayName,od.PlantId,Format(odd.OffDayDate,'yyyy-MMM-dd')WkDate
				from scs.OffDayMaster od 
				left join scs.OffDayDetail odd on odd.OffDayMasterId=od.Id
				where od.OffDayType='W' 
				and od.PlantId='" + plant + @"'
				and odd.OffDayDate='" + Date + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        
        public void RosterWeekOffData(string Date, out DataSet ds, string plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select ei.EmployeeName,apd.EmpSystemId,rpc.RPHeaderId,ei.BudgetCode,rpc.ShiftDefinitionID,rpc.Days31,apd.Daystatus,ws.Code DayType,apd.*
				from  AttdnProcessData apd 
				left join dbo.EmployeeInformation ei on ei.SystemId=apd.EmpSystemId
				left join dbo.RosterBudget rb on rb.budgetId=ei.BudgetCode
				left join RosterPatternChild rpc on rpc.RPHeaderId=rb.RosterId  and rpc.Days31 in (SELECT day('" + Date + @"'))
                left join dbo.RosterEffectiveDate red on red.RPHeaderId=rpc.RPHeaderId
				and red.Id=(select top(1) id from dbo.RosterEffectiveDate where Id=red.Id order by Effectivedate desc)
				left join hkp.WeeklyStatus ws on ws.Id=rpc.WeeklyStatusId
				where apd.workdate='"+ Date + "' and apd.PlantId='"+ plant + @"' and isnull(EmpSystemID,'') IN (
									SELECT isnull(ei.SystemId,'') 
                                    FROM EmployeeInformation AS ei WHERE  ei.PlantId='" + plant + @"'
                                   AND  ei.DOJ <= '" + Date + @"' 
                                   AND (ei.DOS >= '" + Date + @"' OR ISNULL(ei.DOS,'') = '' OR ei.DOS = '01/01/1901'))  ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void OriginalDateData(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                // Getting DayCode and applying the Logic

                var sql = @"select Format(co.OriginalDate,'yyyy-MMM-dd')WkDate,
                co.CompensatoryDateTreatmentType as Type,co.PlantId,co.DayCode,
				co.ForEntirePlant,coel.EmpSystemId
				from mst.CompensatoryOff AS co 
				left join mst.CompensatoryOffEmpList AS coel on coel.CompensatoryOffId=co.Id
				where co.plantId='" + Plant + @"'
				and co.OriginalDate='" + Date + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void LeaveAvailUpdate(string Date, string Plant)
        {
            try
            {
                var sql = @"update LeaveTransactionDetails set IsAvailed=1
					from LeaveTransactionDetails d 	left join leavetransaction lt 
					on LT.SystemID=D.LvTrnsSystemID
					where D.WorkDate='" + Date + "' and PlantID='" + Plant + "' and IsApproved=1";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void OnDutyData(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct e.EmpSystemId,FORMAT(d.Workdate,'yyyy-MMM-dd')Workdate,
                e.PlantId 
                from EmployeeOnDutyDetails d
                left join EmployeeOnDuty e on e.Id=d.OnDutyId
                where Workdate='" + Date + @"' AND IsApproved=1 and IsAvailed=1 
                and e.PlantId='" + PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void OnRestData(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select d.Id as RestId,d.PlantId,d.EmpSystemId,
                FORMAT(r.AttendanceRestDate,'yyyy-MMM-dd')WorkDate
                from AttendanceRest r
                left join AttendanceRestDetail d on r.Id=d.AttendanceRestId
                where AttendanceRestDate='" + Date + "' and d.PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void HolidayData(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select om.PlantId,om.OffDayType,Format(od.OffDayDate,'yyyy-MMM-dd') as WorkDate
                from SCS.OffDayMaster om left join scs.OffDayDetail od
                on om.Id=od.OffDayMasterId where od.OffDayDate='" + Date + @"'
                and om.PlantId='" + PlantId + "' and om.OffDayType='H'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ChangedShift(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select p.EmpSystemID,p.ShiftSystemID,Format(p.WorkDate,'yyyy-MMM-dd')WorkDate,s.InTime,s.OutTime,S.ShiftDuration,
                s.FullDayDuration,s.HalfDayDuration,s.ShortDuration,s.HoursWithoutOT
                from AttdnProcessData p left join ShiftTimeChgMaster s
                on p.ShiftSystemID=s.ShiftDefinationID
                left join ShiftTimeChgChild sc on sc.STCMasterSystemID=s.SystemID
                where WorkDate='" + Date + "' and sc.ShiftDate='" + Date + "' and sc.PlantID='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void TopShift(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select top 1 SystemID,ShiftDuration,ShortDuration,
                HalfDayDuration,HoursWithoutOT,FullDayDuration,InTime,
                OutTime
                from ShiftDefination where PlantID='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }        
        void ShiftTime(ref string InTime, ref string OutTime, string WorkDate)
        {

            if (string.IsNullOrEmpty(InTime) || string.IsNullOrEmpty(OutTime))
            {
                return;
            }
            InTime = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(InTime).ToString("hh:mm:ss tt");
            OutTime = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(OutTime).ToString("hh:mm:ss tt");

            if (Convert.ToDateTime(OutTime).Hour < Convert.ToDateTime(InTime).Hour)
            {
                OutTime = Convert.ToDateTime(OutTime).AddDays(1).ToString("dd-MMM-yyyy hh:mm:ss tt");
            }

        }
        void PlantInTime(ref string PlantInPunchStartTime, string WorkDate)
        {

            if (string.IsNullOrEmpty(PlantInPunchStartTime))
            {
                return;
            }
            PlantInPunchStartTime = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(PlantInPunchStartTime).ToString("hh:mm:ss tt");

        }
        public void OTWeekLocalizationData(string Date, out DataSet ds)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sql = @"select month('"+Date+ @"')as Month,Year('" + Date + @"')as Year,Day('" + Date + @"')as Day,
                DAY(DATEADD(DD,-1,DATEADD(MM,DATEDIFF(MM,-1,'" + Date + @"'),0)))NoOfDaysInMonth,
                W.[Days28] AS Pattern28,W.[Days29] as Pattern29,W.[Days30] as Pattern30,W.[Days31] as Pattern31
                from WeekDefination W where DayNo=Day('" + Date + @"')";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void OTUpdateinAPD(string sql)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void LocalizingHeaderValue(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select p.EmpSystemID,dh.Id as HeaderId,dxc.LeavePolicyMasterId,
		        format(p.WorkDate,'yyyy-MMM-dd')WorkDate from AttdnProcessData p
                join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                left join mst.DesignationMasterLegalDesignation ddm on ddm.LegalDesignationId = 
		        ei.LegalDesignationId
				left join mst.DesignationMaster 
				dm on dm.Id = ddm.DesignationMasterId
				left join scs.DesignationMasterConfiguration dxc on dxc.DesignationMasterId=dm.Id
				and dxc.PlantId=ei.PlantId
				left join DayStatusPlantChild 
				dc on dc.EmpTypeId=dm.EmployeeCategoryId
				and dc.PlantId=ei.PlantId
				left join DayStatusHeader dh on dh.Id=dc.headerId
     			where WorkDate='"+Date+"' and ei.PlantId='"+Plant+"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void TBS_LA_Localizing(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select SystemId,EmployeeCurrentStatus,
                IsLA=case when EmployeeCurrentStatus='LONG ABSENTEEISM' then 1 else 0 end,
                IsTBS=case when EmployeeCurrentStatus='TBS' then 1 else 0 end
                from EmployeeInformation e where EmployeeCurrentStatus 
                in('TBS','LONG ABSENTEEISM')
                and e.EmpType!='Guest' and e.PlantId='"+PlantId+@"' and
                E.DOJ <= '"+Date+@"' AND (E.DOS >= '"+Date+@"' 
                OR ISNULL(E.DOS,'') = '' OR E.DOS = '01/01/1901')
                order by EmployeeCurrentStatus";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void WorkDate_Wise_BudgetSaving(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"Select distinct format(apd.WorkDate ,'dd-MMM-yyyy')
                as WorkDate ,
                mb.Id as BudgetId ,isnull(mb.Deployment,0) as Deployment ,
                Total=isnull((Select top 1 TotalNumber from mst.ManpowerBudgetDetail
                where ManpowerBudgetId = mb.Id
                order by EffectiveDate desc),'0')
                from dbo.AttdnProcessData apd
                left join mst.ManpowerBudget mb on mb.Id = apd.BudgetId
                where PlantID='"+PlantId+@"'
                and apd.WorkDate = '"+Date+"' and mb.Id is not null";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        #region Attendance Process
        public void AttndProcess(string Date, string PlantValue,string UserId=null)
        {

            ProcessLock _lock = new ProcessLock(UserId, ProcessLockId.AttendanceProcess, "", 60);
            _lock.LockProcess();
            try
            {
                Date = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");
                string PreviousDay = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");

                DataSet ValidationData;
                Validation(out ValidationData, PlantValue);
                if (ValidationData.Tables[0].Rows.Count > 0)
                {
                    // Plant Lock Checking of Previous Day
                    DataSet PlantLock;
                    PlantLockCheck(PreviousDay, out PlantLock, PlantValue);
                    if (PlantLock.Tables[0].Rows.Count > 0)
                    {

                    }
                    else
                    {

                        #region Getting MissFlagged InPunch of the PrevDay
                        DataSet MissFlaggedIn;
                        ConfirmedPrevMissIn(PreviousDay, out MissFlaggedIn, PlantValue);
                        #endregion

                        #region Process FlaggedIn Data
                        if (MissFlaggedIn.Tables[0].Rows.Count > 0)
                        {

                            // Previous Day Missed In Flagged(Double Device) Punches
                            // (Due to Some Machine Issues or RawData Late Coming)
                            string MainRowId = "''";

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + PreviousDay + "' and isnull(PunchInTime,'')='' and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");
                            string newformat = Convert.ToDateTime(PreviousDay).ToString("yyyyMMdd");

                            // Last In of Day Allowed Checking (From OutpunchConfiguration)
                            #region InLimit Validation Check
                            DataSet InlimitVal;
                            InLimitValidation(out InlimitVal, PlantValue);
                            string InEntryLimit = clsWebLib.RetValidLen(InlimitVal.Tables[0].Rows[0][@"InEntryLimit"]).ToString();
                            if (InEntryLimit != "")
                            {
                                InEntryLimit = Convert.ToDateTime(PreviousDay).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(InEntryLimit).ToString("HH:mm:ss");
                            }
                            #endregion

                            for (int i = 0; i < MissFlaggedIn.Tables[0].Rows.Count; i++)
                            {
                                string EmpId = clsWebLib.RetValidLen(MissFlaggedIn.Tables[0].Rows[i][@"EmpId"]).ToString();
                                string MinTimeRow = clsWebLib.RetValidLen(MissFlaggedIn.Tables[0].Rows[i][@"MinTime"]).ToString();
                                string InPunchLimit = clsWebLib.RetValidLen(MissFlaggedIn.Tables[0].Rows[i][@"InPunchLimit"]).ToString();
                                string OutPunchLimit = clsWebLib.RetValidLen(MissFlaggedIn.Tables[0].Rows[i][@"OutPunchLimit"]).ToString();
                                DateTime MinTime = new DateTime();

                                string RowId = "";
                                if (MinTimeRow != "")
                                {
                                    // Retrieving RowId of RawData    
                                    string formatString = "yyyyMMddHHmmss";
                                    string sample = MinTimeRow.Split('.')[0].ToString();
                                    MinTime = DateTime.ParseExact(sample, formatString, null);
                                    RowId = MinTimeRow.Split('.')[1].ToString();
                                }

                                PunchTimeVal(ref InPunchLimit, ref OutPunchLimit, PreviousDay);

                                if (MinTimeRow.ToString() != "" && RowId != ""
                                    && InPunchLimit.ToString() != "")
                                {
                                    if (MinTime <= Convert.ToDateTime(InEntryLimit))
                                    {
                                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' and PlantInPunchStartTime<='" + MinTime + "'";
                                        if (dsRef.Tables[0].DefaultView.Count > 0)
                                        {
                                            string ExistingIn = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"PunchInTime"]).ToString();
                                            if (ExistingIn == "")
                                            {
                                                // Once InPunch Added can't be Updated
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchInTime"] = Convert.ToDateTime(MinTime);
                                                dr["OutPunchLimit"] = Convert.ToDateTime(OutPunchLimit);
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                MainRowId += ",'" + RowId + "'";
                                            }
                                        }
                                    }
                                }
                            }

                            SaveDataSets(dsRef);

                            #region RawData Table Processing
                            ProcessFlag(MainRowId); // Setting Processed Flag ->1
                            #endregion
                        }
                        #endregion

                        #region Getting MissFlagless InPunch of the PrevDay
                        DataSet MissFlaglessIn;
                        ConfirmedPrevFlaglessMissIn(PreviousDay, out MissFlaglessIn, PlantValue);
                        #endregion

                        #region Process FlagLess InData
                        if (MissFlaglessIn.Tables[0].Rows.Count > 0)
                        {
                            // Previous Day Missed In Flagless(Single Device) Punches
                            // (Due to Some Machine Issues or RawData Late Coming)

                            string MainRowId = "''";

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + PreviousDay + "' and isnull(PunchInTime,'')='' and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");
                            string newformat = Convert.ToDateTime(PreviousDay).ToString("yyyyMMdd");

                            // Last In of Day Allowed Checking (From OutpunchConfiguration)

                            #region InLimit Validation Check
                            DataSet InlimitVal;
                            InLimitValidation(out InlimitVal, PlantValue);
                            string InEntryLimit = clsWebLib.RetValidLen(InlimitVal.Tables[0].Rows[0][@"InEntryLimit"]).ToString();
                            if (InEntryLimit != "")
                            {
                                InEntryLimit = Convert.ToDateTime(PreviousDay).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(InEntryLimit).ToString("HH:mm:ss");
                            }
                            #endregion

                            for (int i = 0; i < MissFlaglessIn.Tables[0].Rows.Count; i++)
                            {
                                string EmpId = clsWebLib.RetValidLen(MissFlaglessIn.Tables[0].Rows[i][@"EmpId"]).ToString();
                                string MinTimeRow = clsWebLib.RetValidLen(MissFlaglessIn.Tables[0].Rows[i][@"MinTime"]).ToString();
                                string InPunchLimit = clsWebLib.RetValidLen(MissFlaglessIn.Tables[0].Rows[i][@"InPunchLimit"]).ToString();
                                string OutPunchLimit = clsWebLib.RetValidLen(MissFlaglessIn.Tables[0].Rows[i][@"OutPunchLimit"]).ToString();
                                DateTime MinTime = new DateTime();

                                string RowId = "";
                                if (MinTimeRow != "")
                                {
                                    // Retrieving RowId of RawData    
                                    string formatString = "yyyyMMddHHmmss";
                                    string sample = MinTimeRow.Split('.')[0].ToString();
                                    MinTime = DateTime.ParseExact(sample, formatString, null);
                                    RowId = MinTimeRow.Split('.')[1].ToString();
                                }

                                PunchTimeVal(ref InPunchLimit, ref OutPunchLimit, PreviousDay);

                                if (MinTimeRow.ToString() != "" && RowId != ""
                                    && InPunchLimit.ToString() != "")
                                {
                                    if (MinTime <= Convert.ToDateTime(InEntryLimit))
                                    {
                                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' and PlantInPunchStartTime<='" + MinTime + "'";
                                        if (dsRef.Tables[0].DefaultView.Count > 0)
                                        {
                                            string ExistingIn = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"PunchInTime"]).ToString();
                                            if (ExistingIn == "")
                                            {
                                                // Once InPunch Added can't be Updated
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchInTime"] = Convert.ToDateTime(MinTime);
                                                dr["OutPunchLimit"] = Convert.ToDateTime(OutPunchLimit);
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                MainRowId += ",'" + RowId + "'";
                                            }
                                        }
                                    }
                                }
                            }

                            SaveDataSets(dsRef);

                            #region RawData Table Processing
                            ProcessFlag(MainRowId); // Setting Processed Flag ->1
                            #endregion
                        }
                        #endregion

                        #region Getting Missing Out of PrevDay FlagData
                        DataSet OutwithFlag;
                        ConfirmedOutFlagPrevDay(PreviousDay, out OutwithFlag, PlantValue);
                        #endregion

                        #region Process OutTime of Flagged Data
                        if (OutwithFlag.Tables[0].Rows.Count > 0)
                        {
                            // Previous Day In Exist but Out Missing (Flagged Punches Dealing)
                            string MainRowId = "''";
                            var WkDate = OutwithFlag.Tables[0].Rows[0][@"WorkDate"].ToString();
                            string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");


                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' " +
                                "and PlantID='" + PlantValue + "' and isnull(PunchInTime,'')!=''";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                            for (int i = 0; i < OutwithFlag.Tables[0].Rows.Count; i++)
                            {
                                DateTime OutPunch = new DateTime();
                                string EmpId = clsWebLib.RetValidLen(OutwithFlag.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                                string OutPunchRow = clsWebLib.RetValidLen(OutwithFlag.Tables[0].Rows[i][@"MaxOut"]).ToString();
                                string OutPunchLimit = clsWebLib.RetValidLen(OutwithFlag.Tables[0].Rows[i][@"OutPunchLimit"]).ToString();
                                if (EmpId== "24241568")
                                {

                                }
                                string RowId = "";
                                if (OutPunchRow != "")
                                {
                                    // Retrieving RowId of RawData    
                                    string formatString = "yyyyMMddHHmmss";
                                    string sample = OutPunchRow.Split('.')[0].ToString();
                                    OutPunch = DateTime.ParseExact(sample, formatString, null);
                                    RowId = OutPunchRow.Split('.')[1].ToString();
                                }

                                dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {
                                    string ExistingIn = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"PunchInTime"]).ToString();
                                    string ExistingOut = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"PunchOutTime"]).ToString();

                                    if (OutPunchLimit.ToString() != "" && RowId != ""
                                        && OutPunchRow.ToString() != "")
                                    {

                                        if (Convert.ToDateTime(OutPunch) <= Convert.ToDateTime(OutPunchLimit))
                                        {
                                            // Out Limit Validation Check 
                                            // Out Should be greater than In & Less than OutPunchLimit
                                            if (ExistingOut == "" && OutPunch > Convert.ToDateTime(ExistingIn))
                                            {
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchOutTime"] = Convert.ToDateTime(OutPunch);
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                MainRowId += ",'" + RowId + "'";
                                            }

                                            else if (ExistingOut != "" && OutPunch > Convert.ToDateTime(ExistingOut) && OutPunch > Convert.ToDateTime(ExistingIn))
                                            {
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchOutTime"] = Convert.ToDateTime(OutPunch);
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                MainRowId += ",'" + RowId + "'";
                                            }

                                        }
                                    }
                                }
                            }
                            SaveDataSets(dsRef);

                            #region RawData Table Processing
                            ProcessFlag(MainRowId); // Setting Processed Flag ->1
                            #endregion
                        }
                        #endregion

                        #region Getting Missing Out of Prev Day FlaglessData
                        DataSet OutFlagless;
                        ConfirmedOutFlaglessPrevDay(PreviousDay, out OutFlagless, PlantValue);
                        #endregion

                        #region Process OutTime of Flagless Data
                        if (OutFlagless.Tables[0].Rows.Count > 0)
                        {
                            // Previous Day In Exist but Out Missing (Flagless Punches Dealing)
                            string MainRowId = "''";
                            var WkDate = OutFlagless.Tables[0].Rows[0][@"WorkDate"].ToString();
                            string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' " +
                                "and PlantID='" + PlantValue + "' and isnull(PunchInTime,'')!=''";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                            for (int i = 0; i < OutFlagless.Tables[0].Rows.Count; i++)
                            {
                                DateTime OutPunch = new DateTime();
                                string EmpId = clsWebLib.RetValidLen(OutFlagless.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                                string OutPunchRow = clsWebLib.RetValidLen(OutFlagless.Tables[0].Rows[i][@"MaxOut"]).ToString();
                                string OutPunchLimit = clsWebLib.RetValidLen(OutFlagless.Tables[0].Rows[i][@"OutPunchLimit"]).ToString();

                                string RowId = "";
                                if (OutPunchRow != "")
                                {
                                    // Retrieving RowId of RawData    
                                    string formatString = "yyyyMMddHHmmss";
                                    string sample = OutPunchRow.Split('.')[0].ToString();
                                    OutPunch = DateTime.ParseExact(sample, formatString, null);
                                    RowId = OutPunchRow.Split('.')[1].ToString();
                                }

                                dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {
                                    string ExistingIn = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"PunchInTime"]).ToString();
                                    string ExistingOut = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"PunchOutTime"]).ToString();

                                    if (OutPunchLimit.ToString() != "" && RowId != ""
                                        && OutPunchRow.ToString() != "")
                                    {

                                        if (Convert.ToDateTime(OutPunch) <= Convert.ToDateTime(OutPunchLimit))
                                        {
                                            // Out Limit Validation Check 
                                            // Out Should be greater than In & Less than OutPunchLimit
                                            if (ExistingOut == "" && OutPunch > Convert.ToDateTime(ExistingIn))
                                            {
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchOutTime"] = Convert.ToDateTime(OutPunch);
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                MainRowId += ",'" + RowId + "'";
                                            }

                                            else if (ExistingOut != "" && OutPunch > Convert.ToDateTime(ExistingOut) && OutPunch > Convert.ToDateTime(ExistingIn))
                                            {
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchOutTime"] = Convert.ToDateTime(OutPunch);
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                MainRowId += ",'" + RowId + "'";
                                            }

                                        }
                                    }
                                }
                            }
                            SaveDataSets(dsRef);

                            #region RawData Table Processing
                            ProcessFlag(MainRowId); // Setting Processed Flag ->1
                            #endregion
                        }
                        #endregion

                        #region Getting Flagged OutPunch of the Interval
                        DataSet ConfirmOutFlag;
                        FlagDataOutCalculate(PreviousDay, out ConfirmOutFlag, PlantValue);
                        #endregion

                        #region Process Orphan Out Punches of Previous WorkDate
                        if (ConfirmOutFlag.Tables[0].Rows.Count > 0)
                        {
                            string MainRowId = "''";

                            // Double Device Orphan Punches
                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + PreviousDay + "' and isnull(PunchInTime,'')=''" +
                                "and isnull(PunchOutTime,'')='' and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");
                            string newformat = Convert.ToDateTime(PreviousDay).ToString("yyyyMMdd");

                            #region InLimit Validation Check
                            DataSet PlantVal;
                            PlantStartValidation(out PlantVal, PlantValue);
                            string PlantInLimit = clsWebLib.RetValidLen(PlantVal.Tables[0].Rows[0][@"plantStart"]).ToString();
                            string PlantOutLimit = "";
                            if (PlantInLimit != "")
                            {
                                // Plant Start & End Time of Next Day
                                PlantInLimit = Convert.ToDateTime(PreviousDay).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(PlantInLimit).ToString("HH:mm:ss");
                                PlantOutLimit = Convert.ToDateTime(PreviousDay).AddDays(1).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(PlantInLimit).ToString("HH:mm:ss");

                            }
                            #endregion

                            if (DateTime.Now > Convert.ToDateTime(PlantOutLimit))
                            {

                                for (int i = 0; i < ConfirmOutFlag.Tables[0].Rows.Count; i++)
                                {
                                    DateTime MaxTime = new DateTime();
                                    string EmpId = clsWebLib.RetValidLen(ConfirmOutFlag.Tables[0].Rows[i][@"EmpId"]).ToString();
                                    string MaxTimeRow = clsWebLib.RetValidLen(ConfirmOutFlag.Tables[0].Rows[i][@"MaxTime"]).ToString();

                                    string RowId = "";
                                    if (MaxTimeRow != "")
                                    {
                                        // Retrieving RowId of RawData    
                                        string formatString = "yyyyMMddHHmmss";
                                        string sample = MaxTimeRow.Split('.')[0].ToString();
                                        MaxTime = DateTime.ParseExact(sample, formatString, null);
                                        RowId = MaxTimeRow.Split('.')[1].ToString();
                                    }


                                    if (MaxTimeRow.ToString() != "" && RowId != "")
                                    {

                                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "'";
                                        if (dsRef.Tables[0].DefaultView.Count > 0)
                                        {
                                            string ExistingOut = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"PunchOutTime"]).ToString();

                                            if (ExistingOut == "")
                                            {
                                                // Punch Should be in Plant Start time and Plant Out Next Day
                                                if (Convert.ToDateTime(PlantInLimit) <= MaxTime && MaxTime <= Convert.ToDateTime(PlantOutLimit))
                                                {
                                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                    dr.BeginEdit();
                                                    dr["PunchOutTime"] = Convert.ToDateTime(MaxTime);
                                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                    dr.EndEdit();
                                                    MainRowId += ",'" + RowId + "'";
                                                }
                                            }

                                        }
                                    }
                                }

                                SaveDataSets(dsRef);

                                #region RawData Table Processing
                                ProcessFlag(MainRowId);  // Setting Processed Flag ->1
                                #endregion
                            }
                        }
                        #endregion

                        #region Final PrevDay In/Out                  
                        FinalInOut(PreviousDay, PlantValue); // Final In Out Stamping on the Basis of Manual & Punch
                        #endregion

                        #region Exception Final PrevDay In/Out  (Wrong Entry Handling)                   
                        ExceptionFinalInOut(PreviousDay, PlantValue);
                        // Doing Final In Out Null if Invalid Data Entered from Manual
                        #endregion

                        #region Process Final PrevDay In/Out                  
                        ProcessFinalInOut(PreviousDay, PlantValue); // Final In Out Stamping on the Basis of Original Manual & Punch
                        #endregion

                        #region Exception Process Final PrevDay In/Out  (Wrong Entry Handling)                   
                        ExceptionProcessFinalInOut(PreviousDay, PlantValue);
                        // Doing Process Final In Out Null if Invalid Data Entered from OriginalManual
                        #endregion

                        #region In Status Logic Previous Day
                        DataSet InStatusPrev;
                        InStatusCalculate(PreviousDay, out InStatusPrev, PlantValue);
                        if (InStatusPrev.Tables[0].Rows.Count > 0)
                        {
                            // In Status on the Basis of FinalIn
                            var WkDate = InStatusPrev.Tables[0].Rows[0][@"WorkDate"].ToString();
                            string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                            for (int i = 0; i < InStatusPrev.Tables[0].Rows.Count; i++)
                            {
                                // Logic on the basis of Shift Early & Late Margin
                                string EmpId = clsWebLib.RetValidLen(InStatusPrev.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                                string ProcessIntime = clsWebLib.RetValidLen(InStatusPrev.Tables[0].Rows[i][@"ProcessIntime"]).ToString();
                                string ShiftInTime = clsWebLib.RetValidLen(InStatusPrev.Tables[0].Rows[i][@"ShiftInTime"]).ToString();
                                double ShiftEarlyInMargin = Convert.ToDouble(clsWebLib.RetValidLen(InStatusPrev.Tables[0].Rows[i][@"ShiftEarlyInMargin"]).ToString());
                                double ShiftLateInMargin = Convert.ToDouble(clsWebLib.RetValidLen(InStatusPrev.Tables[0].Rows[i][@"ShiftLateInMargin"]).ToString());

                                dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {

                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();
                                    if (ProcessIntime != "" && ShiftInTime != "")
                                    {
                                        // Intime + Margin < ShiftInTime :- EarlyIn
                                        if (Convert.ToDateTime(ProcessIntime).AddMinutes(ShiftEarlyInMargin) < Convert.ToDateTime(ShiftInTime))
                                        {
                                            dr["InStatus"] = "EI";
                                        }
                                        // Intime - Margin > ShiftInTime :- LateIn
                                        else if (Convert.ToDateTime(ProcessIntime).AddMinutes(-ShiftLateInMargin) > Convert.ToDateTime(ShiftInTime))
                                        {
                                            dr["InStatus"] = "LI";
                                        }

                                        else
                                        {
                                            dr["InStatus"] = "IN"; // On Time
                                        }
                                    }
                                    else
                                    {
                                        // If FinalIn Not Present
                                        if (ShiftInTime != "")
                                        {
                                            if (DateTime.Now > Convert.ToDateTime(ShiftInTime))
                                            {
                                                dr["InStatus"] = "IM"; // In Missing
                                            }
                                            else if (DateTime.Now < Convert.ToDateTime(ShiftInTime))
                                            {
                                                dr["InStatus"] = "O"; //Other
                                            }
                                        }
                                    }
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();
                                }
                            }
                            SaveDataSets(dsRef);

                        }
                        #endregion


                    }

                    // Plant Lock Checking of Today
                    DataSet PlantLockToday;
                    PlantLockCheck(Date, out PlantLockToday, PlantValue);
                    if (PlantLockToday.Tables[0].Rows.Count > 0)
                    {

                    }
                    else
                    {
                        #region Getting flagged InPunch of the Day
                        DataSet FlaggedIn;
                        ConfirmedInFlagForDay(Date, out FlaggedIn, PlantValue);
                        #endregion

                        #region Process FlaggedIn Data
                        if (FlaggedIn.Tables[0].Rows.Count > 0)
                        {
                            // Today Flagged(Double Device) Punches
                            string MainRowId = "''";

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + Date + "' and isnull(PunchInTime,'')='' and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");
                            string newformat = Convert.ToDateTime(Date).ToString("yyyyMMdd");

                            #region InLimit Validation Check
                            DataSet InlimitVal;
                            InLimitValidation(out InlimitVal, PlantValue);
                            string InEntryLimit = clsWebLib.RetValidLen(InlimitVal.Tables[0].Rows[0][@"InEntryLimit"]).ToString();
                            if (InEntryLimit != "")
                            {
                                // Last In of Day Allowed Checking (From OutpunchConfiguration)
                                InEntryLimit = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(InEntryLimit).ToString("HH:mm:ss");
                            }
                            #endregion

                            for (int i = 0; i < FlaggedIn.Tables[0].Rows.Count; i++)
                            {
                                string EmpId = clsWebLib.RetValidLen(FlaggedIn.Tables[0].Rows[i][@"EmpId"]).ToString();
                                if (EmpId == "22221041")
                                {
                                    // do nothing
                                }

                                string MinTimeRow = clsWebLib.RetValidLen(FlaggedIn.Tables[0].Rows[i][@"MinTime"]).ToString();
                                string InPunchLimit = clsWebLib.RetValidLen(FlaggedIn.Tables[0].Rows[i][@"InPunchLimit"]).ToString();
                                string OutPunchLimit = clsWebLib.RetValidLen(FlaggedIn.Tables[0].Rows[i][@"OutPunchLimit"]).ToString();
                                DateTime MinTime = new DateTime();

                                string RowId = "";
                                if (MinTimeRow != "")
                                {
                                    // Retrieving RowId of RawData    
                                    string formatString = "yyyyMMddHHmmss";
                                    string sample = MinTimeRow.Split('.')[0].ToString();
                                    MinTime = DateTime.ParseExact(sample, formatString, null);
                                    RowId = MinTimeRow.Split('.')[1].ToString();
                                }

                                PunchTimeVal(ref InPunchLimit, ref OutPunchLimit, Date);

                                if (MinTimeRow.ToString() != "" && RowId != ""
                                    && InPunchLimit.ToString() != "")
                                {
                                    if (MinTime <= Convert.ToDateTime(InEntryLimit))
                                    {
                                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' and PlantInPunchStartTime<='" + MinTime + "'";
                                        if (dsRef.Tables[0].DefaultView.Count > 0)
                                        {
                                            string ExistingIn = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"PunchInTime"]).ToString();
                                            if (ExistingIn == "")
                                            {
                                                // Once InPunch Added can't be Updated
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchInTime"] = Convert.ToDateTime(MinTime);
                                                dr["OutPunchLimit"] = Convert.ToDateTime(OutPunchLimit);
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                MainRowId += ",'" + RowId + "'";
                                            }
                                        }
                                    }
                                }
                            }

                            SaveDataSets(dsRef);

                            #region RawData Table Processing
                            ProcessFlag(MainRowId); // Setting Processed Flag ->1
                            #endregion
                        }
                        #endregion

                        #region Getting flaggless InPunch of the Day
                        DataSet FlagglessIn;
                        FlaglessInForDay(Date, out FlagglessIn, PlantValue);
                        #endregion

                        #region Process FlagglessIn Data
                        if (FlagglessIn.Tables[0].Rows.Count > 0)
                        {

                            // Today Flagless(Single Device) Punches
                            string MainRowId = "''";

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + Date + "'and isnull(PunchInTime,'')='' and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");
                            string newformat = Convert.ToDateTime(Date).ToString("yyyyMMdd");


                            #region InLimit Validation Check
                            DataSet InlimitVal;
                            InLimitValidation(out InlimitVal, PlantValue);
                            string InEntryLimit = clsWebLib.RetValidLen(InlimitVal.Tables[0].Rows[0][@"InEntryLimit"]).ToString();
                            if (InEntryLimit != "")
                            {
                                // Last In of Day Allowed Checking (From OutpunchConfiguration)
                                InEntryLimit = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(InEntryLimit).ToString("HH:mm:ss");
                            }
                            #endregion

                            for (int i = 0; i < FlagglessIn.Tables[0].Rows.Count; i++)
                            {
                                string EmpId = clsWebLib.RetValidLen(FlagglessIn.Tables[0].Rows[i][@"EmpId"]).ToString();
                                if (EmpId == "2200009")
                                {
                                    // do nothing
                                }
                                string MinTimeRow = clsWebLib.RetValidLen(FlagglessIn.Tables[0].Rows[i][@"MinTime"]).ToString();
                                string InPunchLimit = clsWebLib.RetValidLen(FlagglessIn.Tables[0].Rows[i][@"InPunchLimit"]).ToString();
                                string OutPunchLimit = clsWebLib.RetValidLen(FlagglessIn.Tables[0].Rows[i][@"OutPunchLimit"]).ToString();
                                DateTime MinTime = new DateTime();

                                string RowId = "";
                                if (MinTimeRow != "")
                                {
                                    // Retrieving RowId of RawData   
                                    string formatString = "yyyyMMddHHmmss";
                                    string sample = MinTimeRow.Split('.')[0].ToString();
                                    MinTime = DateTime.ParseExact(sample, formatString, null);
                                    RowId = MinTimeRow.Split('.')[1].ToString();
                                }

                                PunchTimeVal(ref InPunchLimit, ref OutPunchLimit, Date);

                                if (MinTimeRow.ToString() != "" && RowId != ""
                                    && InPunchLimit.ToString() != "")
                                {
                                    if (MinTime <= Convert.ToDateTime(InEntryLimit))
                                    {
                                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' and PlantInPunchStartTime<='" + MinTime + "'";
                                        if (dsRef.Tables[0].DefaultView.Count > 0)
                                        {
                                            string ExistingIn = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"PunchInTime"]).ToString();
                                            if (ExistingIn == "")
                                            {
                                                // Once InPunch Added can't be Updated
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchInTime"] = Convert.ToDateTime(MinTime);
                                                dr["OutPunchLimit"] = Convert.ToDateTime(OutPunchLimit);
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                MainRowId += ",'" + RowId + "'";
                                            }
                                        }
                                    }
                                }
                            }

                            SaveDataSets(dsRef);

                            #region RawData Table Processing
                            ProcessFlag(MainRowId); // Setting Processed Flag ->1
                            #endregion
                        }
                        #endregion
                       
                        #region Final Day In/Out    
                        FinalInOut(Date, PlantValue); // Final In Out Stamping on the Basis of Manual & Punch
                        #endregion

                        #region Exception Final Day In/Out  (Wrong Entry Handling)                
                        ExceptionFinalInOut(Date, PlantValue);
                        // Doing Final In Out Null if Invalid Data Entered from Manual
                        #endregion

                        #region Process Final Day In/Out    
                        ProcessFinalInOut(Date, PlantValue); // Final In Out Stamping on the Basis of OriginalManual & Punch
                        #endregion

                        #region Exception Process Final Day In/Out  (Wrong Entry Handling)                
                        ExceptionProcessFinalInOut(Date, PlantValue);
                        // Doing Process Final In Out Null if Invalid Data Entered from OriginalManual
                        #endregion

                        #region In Status Logic
                        DataSet InStatus;
                        InStatusCalculate(Date, out InStatus, PlantValue);
                        if (InStatus.Tables[0].Rows.Count > 0)
                        {
                            // In Status on the Basis of FinalIn
                            var WkDate = InStatus.Tables[0].Rows[0][@"WorkDate"].ToString();
                            string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                            for (int i = 0; i < InStatus.Tables[0].Rows.Count; i++)
                            {
                                
                                // Logic on the basis of Shift Early & Late Margin
                                string EmpId = clsWebLib.RetValidLen(InStatus.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                                if (EmpId == "2200009")
                                {
                                    // do nothing
                                }

                                string ProcessIntime = clsWebLib.RetValidLen(InStatus.Tables[0].Rows[i][@"ProcessIntime"]).ToString();
                                string ShiftInTime = clsWebLib.RetValidLen(InStatus.Tables[0].Rows[i][@"ShiftInTime"]).ToString();
                                double ShiftEarlyInMargin = Convert.ToDouble(clsWebLib.RetValidLen(InStatus.Tables[0].Rows[i][@"ShiftEarlyInMargin"]).ToString());
                                double ShiftLateInMargin = Convert.ToDouble(clsWebLib.RetValidLen(InStatus.Tables[0].Rows[i][@"ShiftLateInMargin"]).ToString());

                                dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {

                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();
                                    if (ProcessIntime != "" && ShiftInTime != "")
                                    {
                                        // Intime + Margin < ShiftInTime :- EarlyIn
                                        if (Convert.ToDateTime(ProcessIntime).AddMinutes(ShiftEarlyInMargin) < Convert.ToDateTime(ShiftInTime))
                                        {
                                            dr["InStatus"] = "EI";
                                        }
                                        // Intime - Margin > ShiftInTime :- LateIn
                                        else if (Convert.ToDateTime(ProcessIntime).AddMinutes(-ShiftLateInMargin) > Convert.ToDateTime(ShiftInTime))
                                        {
                                            dr["InStatus"] = "LI";
                                        }

                                        else
                                        {
                                            dr["InStatus"] = "IN"; // On Time
                                        }
                                    }
                                    else
                                    {
                                        // If FinalIn Not Present
                                        if (ShiftInTime != "")
                                        {
                                            if (DateTime.Now > Convert.ToDateTime(ShiftInTime))
                                            {
                                                dr["InStatus"] = "IM"; // In Missing
                                            }
                                            else if (DateTime.Now < Convert.ToDateTime(ShiftInTime))
                                            {
                                                dr["InStatus"] = "O"; //Other
                                            }
                                        }
                                    }
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();
                                }
                            }
                            SaveDataSets(dsRef);

                        }
                        #endregion


                    }
                }
                _lock.UnlockProcess();
            }
            catch (Exception ex)
            {
                _lock.UnlockProcess();
                throw ex;
            }

        }
        #endregion

        #region Validations   

        void PunchTimeVal(ref string InPunchLimit, ref string OutPunchLimit, string WorkDate)
        {

            if (string.IsNullOrEmpty(InPunchLimit) || string.IsNullOrEmpty(OutPunchLimit))
            {
                return;
            }
            InPunchLimit = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(InPunchLimit).ToString("HH:mm:ss");
            OutPunchLimit = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(OutPunchLimit).ToString("HH:mm:ss");

            if (Convert.ToDateTime(OutPunchLimit) < Convert.ToDateTime(InPunchLimit))
            {
                OutPunchLimit = Convert.ToDateTime(OutPunchLimit).AddDays(1).ToString("dd-MMM-yyyy HH:mm:ss");
            }

        }
        public void Validation(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select oh.InPunchStartTime,oc.OutPunchLimit from 
                OutPunchConfigurationHeader oh 
                left join OutPunchConfigurationChild oc on oc.MasterId=oh.Id
                where PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void InLimitValidation(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select max(oc.InPunchLimit) as InEntryLimit from 
                OutPunchConfigurationChild oc left 
                join outpunchconfigurationheader oh on oh.Id=oc.MasterId
                where PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PlantStartValidation(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct oh.InPunchStartTime as plantStart from               
				outpunchconfigurationheader oh
                where PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void GetPlant(string CompanyGpId, out DataSet ds)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sql = @"SELECT CompanyGroupId, Id as PlantValue FROM ORG.Plant WHERE CompanyGroupId = 
               '" + CompanyGpId + "' AND  Active = 1 AND Archive = 0 ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void GetCompanyGp(out DataSet ds)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sql = @"select distinct Id as CGId from org.CompanyGroup";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }


        #endregion

        #region Attendance Process Source Data
        public void ConfirmedPrevMissIn(string PrevDay, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sqlx = @"SELECT * FROM ( select a.LogDownLoadNum as EmpId,               
                (
                select  min(convert (varchar(30), format(b.PTime,'yyyyMMddHHmmss'))+'.'+convert(varchar(30),b.RowID))
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='IN' and  b.PlantId='" + Plant + @"'  
                and convert(date,b.PTime)='" + PrevDay + @"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum
                ) as MinTime,

                (
                select top 1 Format(oc.InPunchLimit,'yyyy-MMM-dd HH:mm:ss')  
                from
                dbo.OutPunchConfigurationHeader oh
                left join dbo.OutPunchConfigurationChild oc on oc.MasterId = oh.Id
                left join org.Plant p on p.Id = oh.PlantId
                where CAST(oc.InPunchLimit AS TIME) >= Cast ((select  min(b.PTime)
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='IN' and  b.PlantId='" + Plant + @"'  
                and convert(date,b.PTime)='" + PrevDay + @"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum  )
                AS TIME) 
                and p.Id='" + Plant + @"'
                ) as InPunchLimit,
                (
                select top 1 Format(oc.OutPunchLimit,'yyyy-MMM-dd HH:mm:ss')
                from
                dbo.OutPunchConfigurationHeader oh
                left join dbo.OutPunchConfigurationChild oc on oc.MasterId = oh.Id
                left join org.Plant p on p.Id = oh.PlantId
                where CAST(oc.InPunchLimit AS TIME) >= Cast ((select  min(b.PTime)
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='IN' and  b.PlantId='" + Plant + @"'  
                and convert(date,b.PTime)='" + PrevDay + @"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum  ) AS TIME) and 
				p.Id='" + Plant + @"'
                ) as OutPunchLimit
                from
                AttdnRawData a 
                left join AttdnProcessData p on p.EmpSystemID=a.LogDownLoadNum
                where isnull(p.PunchInTime,'')='' and isnull(p.PunchOutTime,'')='' 
                and a.PlantId='" + Plant + @"' and convert(date,a.PTime)='" + PrevDay + @"'
                and (a.PType='IN') and a.ProcessedFlag!=1               
                GROUP BY 
                a.LogDownLoadNum,a.PDate,a.PlantID
				) AS dd where dd.MinTime!=''";
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sqlx, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void ConfirmedPrevFlaglessMissIn(string PrevDay, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sqlx = @"SELECT * FROM ( select a.LogDownLoadNum as EmpId,
                (
                select  min(convert (varchar(30), format(b.PTime,'yyyyMMddHHmmss'))+'.'+convert(varchar(30),b.RowID))
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='' and  b.PlantId='" + Plant + @"'  
                and convert(date,b.PTime)='" + PrevDay + @"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum
                ) as MinTime,

                (
                select top 1 Format(oc.InPunchLimit,'yyyy-MMM-dd HH:mm:ss')  
                from
                dbo.OutPunchConfigurationHeader oh
                left join dbo.OutPunchConfigurationChild oc on oc.MasterId = oh.Id
                left join org.Plant p on p.Id = oh.PlantId
                where CAST(oc.InPunchLimit AS TIME) >= Cast ((select  min(B.PTIME)
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='' and  b.PlantId='" + Plant + @"'  
                and convert(date,b.PTime)='" + PrevDay + @"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum)
               
                AS TIME) 
                and p.Id='" + Plant + @"' 
                ) as InPunchLimit,
                (
                select top 1 Format(oc.OutPunchLimit,'yyyy-MMM-dd HH:mm:ss')
                from
                dbo.OutPunchConfigurationHeader oh
                left join dbo.OutPunchConfigurationChild oc on oc.MasterId = oh.Id
                left join org.Plant p on p.Id = oh.PlantId
                where CAST(oc.InPunchLimit AS TIME) >= Cast ((select  min(B.PTIME)
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='' and  b.PlantId='" + Plant + @"'  
                and convert(date,b.PTime)='" + PrevDay + @"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum) AS TIME) and p.Id='" + Plant + @"'
                ) as OutPunchLimit
                from
                AttdnRawData a 
                left join AttdnProcessData p on p.EmpSystemID=a.LogDownLoadNum				
                where isnull(p.PunchInTime,'')='' and isnull(p.PunchOutTime,'')=''
                and a.PlantId='" + Plant + @"' and 
                convert(date,a.PTime)='" + PrevDay + @"'   
                and isnull(a.PType,'')='' and a.ProcessedFlag!=1 
                GROUP BY 
                a.LogDownLoadNum,a.PDate,a.PlantID
				) AS dd where dd.MinTime!=''";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sqlx, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void ConfirmedInFlagForDay(string Date, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sqlx = @"select a.LogDownLoadNum as EmpId,               
                (
                select  min(convert (varchar(30), format(b.PTime,'yyyyMMddHHmmss'))+'.'+convert(varchar(30),b.RowID))
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='IN' and  b.PlantId='"+Plant+@"'  
                and convert(date,b.PTime)='"+Date+@"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum
                ) as MinTime,

                (
                select top 1 Format(oc.InPunchLimit,'yyyy-MMM-dd HH:mm:ss')  
                from
                dbo.OutPunchConfigurationHeader oh
                left join dbo.OutPunchConfigurationChild oc on oc.MasterId = oh.Id
                left join org.Plant p on p.Id = oh.PlantId
                where CAST(oc.InPunchLimit AS TIME) >= Cast ((select  min(b.PTime)
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='IN' and  b.PlantId='"+Plant+@"'  
                and convert(date,b.PTime)='"+Date+@"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum  )
                AS TIME) 
                and p.Id='"+Plant+@"'
                ) as InPunchLimit,
                (
                select top 1 Format(oc.OutPunchLimit,'yyyy-MMM-dd HH:mm:ss')
                from
                dbo.OutPunchConfigurationHeader oh
                left join dbo.OutPunchConfigurationChild oc on oc.MasterId = oh.Id
                left join org.Plant p on p.Id = oh.PlantId
                where CAST(oc.InPunchLimit AS TIME) >= Cast ((select  min(b.PTime)
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='IN' and  b.PlantId='"+Plant+@"'  
                and convert(date,b.PTime)='"+Date+@"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum  ) AS TIME) and 
				p.Id='"+Plant+@"'
                ) as OutPunchLimit
                from
                AttdnRawData a 
                where a.PlantId='"+Plant+@"' and convert(date,a.PTime)='"+Date+@"'
                and a.PType='IN' and a.ProcessedFlag!=1               
                GROUP BY 
                a.LogDownLoadNum,a.PDate,a.PlantID";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sqlx, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void FlaglessInForDay(string Date, out DataSet ds, string Plant)
        {
           // ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sqlx = @"select a.LogDownLoadNum as EmpId,
                (
                select  min(convert (varchar(30), format(b.PTime,'yyyyMMddHHmmss'))+'.'+convert(varchar(30),b.RowID))
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='' and  b.PlantId='"+Plant+@"'  
                and convert(date,b.PTime)='"+Date+@"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum
                ) as MinTime,

                (
                select top 1 Format(oc.InPunchLimit,'yyyy-MMM-dd HH:mm:ss ')  
                from
                dbo.OutPunchConfigurationHeader oh
                left join dbo.OutPunchConfigurationChild oc on oc.MasterId = oh.Id
                left join org.Plant p on p.Id = oh.PlantId
                where CAST(oc.InPunchLimit AS TIME) >= Cast ((select  min(B.PTIME)
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='' and  b.PlantId='"+Plant+@"'  
                and convert(date,b.PTime)='"+Date+@"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum)
               
                AS TIME) 
                and p.Id='"+Plant+@"' 
                ) as InPunchLimit,
                (
                select top 1 Format(oc.OutPunchLimit,'yyyy-MMM-dd HH:mm:ss')
                from
                dbo.OutPunchConfigurationHeader oh
                left join dbo.OutPunchConfigurationChild oc on oc.MasterId = oh.Id
                left join org.Plant p on p.Id = oh.PlantId
                where CAST(oc.InPunchLimit AS TIME) >= Cast ((select  min(B.PTIME)
                from AttdnRawData b left join OutPunchConfigurationHeader oh on oh.PlantId=b.PlantID
                where b.LogDownLoadNum=a.LogDownLoadNum and b.ProcessedFlag!=1
                and isnull(b.PType,'')='' and  b.PlantId='"+Plant+@"'  
                and convert(date,b.PTime)='"+Date+@"'   
                and CAST(oh.InPunchStartTime AS TIME) <= Cast ((b.ptime) AS TIME)
                group by b.LogDownLoadNum) AS TIME) and p.Id='"+Plant+@"'
                ) as OutPunchLimit
                from
                AttdnRawData a 
                where a.PlantId='"+Plant+@"' and 
                convert(date,a.PTime)='"+Date+@"'   
                and isnull(a.PType,'')='' and a.ProcessedFlag!=1 and 
                isnull(a.PType,'')!='IN' AND isnull(a.PType,'')!='OUT' 
                GROUP BY 
                a.LogDownLoadNum,a.PDate,a.PlantID";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sqlx, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void ConfirmedOutFlagPrevDay(string PreviousDate, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sqlx = @"select * from (
                select distinct EmpSystemID ,(Format(WorkDate,'yyyy-MMM-dd'))WorkDate,
                PunchInTime,OutPunchLimit,
                (select Max(convert (varchar(30), format(PTime,'yyyyMMddHHmmss'))+'.'+convert(varchar(30),RowID)) from AttdnRawData b where b.LogDownLoadNum=p.EmpSystemID and
                b.PTime between p.PunchInTime and p.OutPunchLimit and b.ProcessedFlag!=1
                AND isnull(b.PType,'')='OUT'
                )as MaxOut
                from AttdnProcessData p 
                left join AttdnRawData a on a.PlantID=p.PlantID
                where p.PlantID='" + Plant + @"' and a.ProcessedFlag!=1 and ISNULL(PType,'')!='IN'  
                and WorkDate ='" + PreviousDate + @"' and isnull(PunchInTime,'')!='' and  isnull(OutPunchLimit,'')!='' 
                and getdate()>=OutPunchLimit) 
                as dd where isnull(dd.MaxOut,'')!=''";
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sqlx, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void ConfirmedOutFlaglessPrevDay(string PreviousDate, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sqlx = @"select * from (select distinct EmpSystemID ,(Format(WorkDate,'yyyy-MMM-dd'))WorkDate,
                PunchInTime,OutPunchLimit,
                (select Max(convert (varchar(30), format(PTime,'yyyyMMddHHmmss'))+'.'+convert(varchar(30),RowID)
                )
                from AttdnRawData b where b.LogDownLoadNum=p.EmpSystemID and
                b.PTime between p.PunchInTime and p.OutPunchLimit and b.ProcessedFlag!=1
                AND isnull(b.PType,'')='' 
                )as MaxOut
                from AttdnProcessData p 
                left join AttdnRawData a on a.PlantID=p.PlantID
                where p.PlantID='" + Plant + @"' and a.ProcessedFlag!=1 
                and WorkDate ='" + PreviousDate + @"' and  isnull(PunchInTime,'')!='' and  isnull(OutPunchLimit,'')!='' 
                and getdate()>=OutPunchLimit) 
                as dd where isnull(dd.MaxOut,'')!='' ";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sqlx, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void FinalInOut(string Date, string Plant)
        {
            try
            {
                var sql = @"update AttdnProcessData set InTime=ISNULL(ManualInTime,PunchInTime),OutTime=
				 ISNULL(ManualOutTime,PunchOutTime),UpdatedBy='Schedule',DateUpdated=GETDATE()
				 WHERE WorkDate='" + Date + @"' 
				 and PlantID='" + Plant + "'";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ProcessFinalInOut(string Date, string Plant)
        {
            try
            {
                var sql = @"update AttdnProcessData set ProcessIntime=ISNULL(OriginalManualInTime,PunchInTime),ProcessOuttime=
				 ISNULL(OriginalManualOutTime,PunchOutTime),UpdatedBy='Schedule',DateUpdated=GETDATE()
				 WHERE WorkDate='" + Date + @"' 
				 and PlantID='" + Plant + "'";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ExceptionFinalInOut(string Date, string Plant)
        {
            try
            {
                var sql = @"update AttdnProcessData	set Intime=null,OutTime=null				 
				 from AttdnProcessData 
				 WHERE WorkDate='"+Date+@"' 
				 and PlantID='"+Plant+@"' and 
				 ISNULL(ManualInTime,PunchInTime)> ISNULL(ManualOutTime,PunchOutTime)";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ExceptionProcessFinalInOut(string Date, string Plant)
        {
            try
            {
                var sql = @"update AttdnProcessData	set ProcessIntime=null,ProcessOuttime=null				 
				 from AttdnProcessData 
				 WHERE WorkDate='" + Date + @"' 
				 and PlantID='" + Plant + @"' and 
				 ISNULL(OriginalManualInTime,PunchInTime) > ISNULL(OriginalManualOutTime,PunchOutTime)";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ProcessFlag(string MainRowId)
        {
            try
            {
                var sql = @"update AttdnRawData set ProcessedFlag=1,UpdatedBy='Schedule',DateUpdated=GetDate()
                where RowID IN(" + MainRowId + @")";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void FlagDataOutCalculate(string PrevDay, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try  
            {   
                string pretime = Convert.ToDateTime(PrevDay).ToString("yyyy-MMM-dd") + " " + "6:30:00";
                string nexttime = Convert.ToDateTime(PrevDay).AddDays(1).ToString("yyyy-MMM-dd") + " " + "6:30:00";

                var sqlx = @"select distinct * from (select a.LogDownLoadNum as EmpId,
                (select max(convert(varchar(30), format(b.PTime,'yyyyMMddHHmmss')) + '.'+
				convert(varchar(30),b.RowID)
				) 
                 from AttdnRawData b 
                where b.LogDownLoadNum=a.LogDownLoadNum and isnull(b.PType,'')='OUT' 
				and PlantId='" + Plant + @"' 
                and b.PTime between '" + pretime + @"' and '" + nexttime + @"'
			    group by b.LogDownLoadNum)as MaxTime
                from
                AttdnRawData a 
				left join AttdnProcessData p on p.EmpSystemID=a.LogDownLoadNum
                where a.PlantId='" + Plant + @"' and p.WorkDate='" + PrevDay + @"'
				and a.PTime between '" + pretime + @"' and '" + nexttime + @"'
				and isnull(p.PunchInTime,'')=''
				and isnull(p.PunchOutTime,'')=''
                and  a.ProcessedFlag!=1
				and isnull(a.PType,'')!='' and isnull(a.PType,'')!='IN'
                GROUP BY 
                a.LogDownLoadNum,a.PDate,a.PlantID) as dd where 
				dd.MaxTime!=''";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sqlx, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void InStatusCalculate(string Date, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct Format(WorkDate,'yyyy-MMM-dd')WorkDate,EmpSystemID,
               Format(ap.ProcessIntime,'yyyy-MMM-dd HH:mm:ss')ProcessIntime,				
				Format(ap.ShiftInTime,'yyyy-MMM-dd HH:mm:ss')ShiftInTime,  
                sd.ShiftEarlyInMargin,sd.ShiftLateInMargin                
                from Attdnprocessdata  ap
                left join ShiftDefination sd on sd.SystemID=ap.ShiftSystemID
                where workdate='" + Date + @"' and ap.PlantID='" + Plant + @"'";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sql, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        #endregion

        #region DayStatus Source Data
        public void DayStatusReprocessing(string PreDay, string Plant)
        {

            try
            {
                var sql = @"update AttdnProcessData set Duration=null,earlyin=null,latein=null,LateOut=null,
                earlyout=null,OverStay=null,UnderStay=null,DurationStatus=null,EarlyLateIn=null,EarlyLateOut=null,
                SandwichFlag=NULL,DayTypeOtApplicable=null,SandwichStatus=null,ProcessFinalDayStatus=null,DayStatus=null,
                DayStatusCode=null,ProcessDayStatus=null,ProcessedOT=0,DayTypeGoodWorkApplicable=null,IsLock=0,LockedBy=null,
                LockedDate=null ,IsOTComfirm=0,OTComfirmBy=null,DateOTComfirm=null,StandardOT=null,PlanOT=null,AppliedOTLimit=null,
                AllowedOTLimit=null,TargetOT=null,AdditionalOT=null,CalculatedOT=0,SandwichReprocess=0
                where PlantID='" + Plant+"' and WorkDate='"+PreDay+"'";

                ConnectionManager.clsConnection objCone = new ConnectionManager.clsConnection();

                //ConnectionManager.DAL.ConManager objCone = null;
                //objCone = new ConnectionManager.DAL.ConManager("1");
                //objCone.OpenConnection("1");
                objCone.BeginTransaction();
                objCone.executeQuery(sql);
                //objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void OutRestoring(string PreDay, string Plant)
        {

            try
            {
                var sql = @"update AttdnProcessData set OutTime=isnull(ProcessOuttime,OutTime),
                ManualOutTime=isnull(OriginalManualOutTime,ManualOutTime)
                where WorkDate='"+PreDay+"' and PlantID='"+Plant+@"'
                and IsOTEntitled='1'
                and IsOTComfirm=0";

                ConnectionManager.clsConnection objCone = new ConnectionManager.clsConnection();
                //ConnectionManager.DAL.ConManager objCone = null;
                //objCone = new ConnectionManager.DAL.ConManager("1");
                //objCone.OpenConnection("1");
                objCone.BeginTransaction();
                objCone.executeQuery(sql);
                //objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PrevDayDuration(string PreDay, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select * from (select Format(WorkDate,'yyyy-MMM-dd')WorkDate,EmpSystemID,
                datediff(minute,ap.ProcessIntime,ap.ProcessOuttime) 
                as CalDuration, Format(ap.ProcessIntime,'yyyy-MMM-dd HH:mm:ss')ProcessIntime,
				Format(ap.ProcessOuttime,'yyyy-MMM-dd HH:mm:ss')ProcessOuttime,
				Format(ap.ShiftInTime,'yyyy-MMM-dd HH:mm:ss')ShiftInTime, 
                Format(ap.ShiftOutTime,'yyyy-MMM-dd HH:mm:ss')ShiftOutTime, 
                sd.ShiftEarlyInMargin,sd.ShiftEarlyOutMargin,sd.ShiftLateInMargin,
                sd.ShiftLateOutMargin,ap.RowId
                from Attdnprocessdata  ap
                left join ShiftDefination sd on sd.SystemID=ap.ShiftSystemID
                where workdate='" + PreDay + @"' and ap.PlantID='" + Plant + @"' 
                and isnull(ap.ProcessIntime,'')!='' and isnull(ap.ProcessOuttime,'')!='') as dd
				where dd.CalDuration>=0";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sql, out ds);
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        public void PlantLockCheck(string Date, out DataSet ds, string Plant)
        {
           // ConnectionManager.DAL.ConManager objCon;
            try
            {
                string Today = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId='" + Plant + @"'
                and LockedDate='" + Today + "' and IsActive='1'";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sql, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void OverUnderStayPrevDay(string PreDay, out DataSet ds, string Plant)
        {

            try
            {
                //ConnectionManager.DAL.ConManager objCon;

                var sql = @"select ap.EmpSystemID,Format(ap.WorkDate,'yyyy-MMM-dd')WorkDate,
                ap.Duration,ap.ShiftSystemID,ap.RowId,
                OverUnderStay=case when ap.DayTypeOtApplicable=2 then
				ap.Duration else
				(ap.Duration-isnull(ap.ShiftHoursWithoutOT,'0'))
				end
                from attdnprocessdata ap
                where WorkDate='" + PreDay + "' and Duration >0 and ap.PlantID='" + Plant + "'";


                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sql, out ds);
            }

            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PrevDurationStatusCal(string PreviousDay, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select p.EmpSystemID,Format(p.WorkDate,'yyyy-MMM-dd')WorkDate,p.Duration
                ,p.ShiftHalfDayDuration,p.RowId,p.ShiftFullDayDuration,p.ProcessIntime,p.ProcessOuttime,
                p.ShiftShortDuration from AttdnProcessData p 
                where WorkDate='" + PreviousDay + @"' 
                and p.PlantID='" + Plant + "'";
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sql, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PrevDayStatusCodeData(string PreDay, string Plant)
        {

            try
            {
                var sql = @"UPDATE AttdnProcessData Set DayStatusCode=(ISNULL(HolidayStatus,'')+	
											ISNULL(WeeklyStatus,'')+ISNULL(DurationStatus,'')+
								ISNULL(EarlyLateIn,'')+ISNULL(EarlyLateOut,'')
								+ISNULL(LeaveStatus,'')),DateUpdated=GETDATE() 
                        WHERE PlantID='" + Plant + @"'
								AND WorkDate='" + PreDay + "'";

                ConnectionManager.clsConnection objCone = new ConnectionManager.clsConnection();
                //ConnectionManager.DAL.ConManager objCone = null;
                //objCone = new ConnectionManager.DAL.ConManager("1");
                //objCone.OpenConnection("1");
                objCone.BeginTransaction();
                objCone.executeQuery(sql);
                // objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PrevProcessDayStatusUpdate(string PreDay, string Plant)
        {
            try
            {
                var sql = @"update AttdnProcessData set ProcessDayStatus=x.DayType from
                    (select distinct p.EmpSystemID,p.rowid as RowIdx,dt.DayType,
                        format(p.WorkDate,'yyyy-MMM-dd')WorkDate from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                     	                    left join DayStatusHeader dh on dh.Id=p.DayStatusHeaderId
									        left join DayStatus ds on ds.headerId=dh.Id
											left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId
									        where WorkDate='"+PreDay+@"' and ds.Code=p.DayStatusCode
									        and ei.PlantId='"+Plant+@"') as x
			            where x.RowIdx=Rowid";

                //ConnectionManager.DAL.ConManager objCone = null;
                //objCone = new ConnectionManager.DAL.ConManager("1");
                //objCone.OpenConnection("1");
                ConnectionManager.clsConnection objCone = new ConnectionManager.clsConnection();
                objCone.BeginTransaction();

                objCone.executeQuery(sql);
                //objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PreProcessFinalDayStatus(string PreDay, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {
                string newformat = Convert.ToDateTime(PreDay).ToString("yyyyMMdd");

                var sql = @"select distinct p.EmpSystemID,Result=dt.DayType,format(p.WorkDate,'yyyy-MMM-dd')WorkDate, 
                dt.SandwichStatusFlag,p.RowId                 
				from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                     	left join DayStatusHeader dh on dh.Id=p.DayStatusHeaderId
						left join DayStatus ds on ds.headerId=dh.Id
						left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId									       
						where WorkDate='" + PreDay+ @"' 
						and dt.DayType=ISNULL(p.ManualDayStatus,p.ProcessDayStatus)
						and ei.PlantId='" + Plant+"'";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sql, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PrePayrollDayStatus(string PreDay, string Plant)
        {

            try
            {
                var sql = @"UPDATE 	AttdnProcessData Set DayStatus= ISNULL(ISNULL(ManualDayStatus,SandwichStatus),ProcessDayStatus)
				,UpdatedBy='Schedule',DateUpdated=GETDATE()
								WHERE PlantID='" + Plant + @"'
								AND WorkDate='" + PreDay + "'";

                //ConnectionManager.DAL.ConManager objCone = null;
                //objCone = new ConnectionManager.DAL.ConManager("1");
                //objCone.OpenConnection("1");
                ConnectionManager.clsConnection objCone = new ConnectionManager.clsConnection();
                objCone.BeginTransaction();
                objCone.executeQuery(sql);
                //objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void PrevDayOTCalculation(string PreDay, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            { 
                // 1 :- On OverStay 2:- On Duration 3:- On (OverStay-EarlyIn)
                var sql = @"select distinct p.EmpSystemID,p.RowId,
                format(p.WorkDate,'yyyy-MMM-dd')WorkDate,Result=
                case when p.DayTypeOTApplicable='1' then 
                (select distinct ot.AllotedOT from OTPerMinutePolicy ot
                where ot.PlantId=p.PlantID and ot.OverstayOrEarlyOut=p.OverStay) 
                when p.DayTypeOTApplicable='2' then (select distinct ot.OffDayAllotedOT 
				from OTPerMinutePolicy ot
                where ot.PlantId=p.PlantID and ot.OverstayOrEarlyOut=p.Duration)
				when p.DayTypeOTApplicable='3' then (select distinct ot.AllotedOT 
				from OTPerMinutePolicy ot
                where ot.PlantId=p.PlantID and ot.OverstayOrEarlyOut=p.OverStay-p.EarlyIn) 
				end
                from AttdnProcessData p
                left join org.Plant pl on pl.Id=p.PlantID
                left join OTPerMinutePolicy ot on ot.PlantId=pl.Id
                        where WorkDate='" + PreDay + @"' and p.IsOTEntitled='1'
						and p.DayTypeOTApplicable != 0 and p.Duration>0
						and p.PlantId='" + Plant + "'";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sql, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void SandwichLogic(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct EmpSystemID,SandwichFlag,ProcessFinalDayStatus,
                Format(WorkDate,'yyyy-MMM-dd')WorkDate from AttdnProcessData 
                where WorkDate='" + Date + "' and PlantID='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ManualOT(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct EmpSystemId,OThour,Format(WorkDate,'yyyy-MMM-dd')WorkDate
                    from dbo.otfromapp ot 
                    LEFT JOIN EmployeeInformation E ON E.SystemId=ot.EmpSystemId
                    where e.PlantId='" + Plant + "' and ot.WorkDate='" + Date + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void UpdateZeroProcessedOTEmp(string Date,string Plant)
        {
            try
            {
                var sql = @"UPDATE AttdnProcessData SET IsOTComfirm=1,OTComfirmBy='AutoConfirmation',
                DateOTComfirm=GETDATE()
                where (ProcessedOT=0 AND OverStay IS NULL) and isnull(DayStatus,'')!=''
                and WorkDate='"+Date+@"' and IsOTComfirm=0 AND IsOTEntitled=1
                and PlantID='"+Plant+"'";

                //ConnectionManager.DAL.ConManager objCone = null;
                //objCone = new ConnectionManager.DAL.ConManager("1");
                //objCone.OpenConnection("1");
                ConnectionManager.clsConnection objCone = new ConnectionManager.clsConnection();
                objCone.BeginTransaction();

                objCone.executeQuery(sql);
                //objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PreProcessPayrollDayStatusData(string PreDay, out DataSet ds, string Plant)
        {
            //ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.EmpSystemID,Result=dt.DayType, dt.AutoLock,format(p.WorkDate,'yyyy-MMM-dd')WorkDate, 
                dt.SandwichStatusFlag,dt.OTApplicable,dt.GoodWorkApplicable,p.RowId,				
				isnull(dt.PresentValuePD,'0')PresentValue,isnull(dt.LateValueLV,'0')LateValue,isnull(dt.AbsentValueAB,'0')AbsentValue,
				isnull(dt.LeaveValueLP,'0')LvValue,isnull(dt.CompAssignLv,'0')CompAssignLvValue,
                isnull(dt.WeeklyOffWO,'0')WeekOffValue,isnull(dt.HolidayH,'0')HoliDayValue,isnull(dt.WeekOffHoliDayWOH,'0')WeekOffHoliDayValue,
				isnull(dt.TotalWorkingDay,'0')WorkingDay,
				isnull(dt.ActualWorkingDay,'0')ActualWorkingDay,isnull(dt.PayDay,'0')TotalPayDay,isnull(dt.NonPayDay,'0')TotalNonPayDay                 
				from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                     	left join DayStatusHeader dh on dh.Id=p.DayStatusHeaderId
						left join DayStatus ds on ds.headerId=dh.Id
						left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId									       
						where WorkDate='" + PreDay + @"' 
						and dt.DayType=p.DayStatus
						and ei.PlantId='" + Plant + "'";

                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
                ConnectionManager.clsConnectionManager con = new clsConnectionManager(3600);
                con.getDataSet(sql, out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        #endregion

        #region OT Confirmation Process SourceData
        public void AutoConfirmedDataSet(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.RowId from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                                            left join DayStatusHeader dh on dh.Id=p.DayStatusHeaderId
									        left join DayStatus ds on ds.headerId=dh.Id
											left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId
									        where WorkDate='" + Date+@"' and	dt.DayType=p.DayStatus 
											and IsOTEntitled=1 and isOTConfirmationAuto=1
											and DayTypeOtApplicable=0
									        and ei.PlantId='"+PlantId+"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                 throw (ex);
            }

        }
        public void ConfirmOTFlag(string MainRowId)
        {
            try
            {
                var sql = @"update AttdnProcessData set IsOTComfirm=1,
                OTComfirmBy='AutoConfirmation',DateOTComfirm=GETDATE() where rowid in("+MainRowId+@")";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion
       
        #region DayStatus Process
        public void DayStatus(string Date, string PlantValue,string UserId=null)
        {

            ProcessLock _lock = new ProcessLock(UserId, ProcessLockId.AttendanceProcess, "", 60);
            _lock.LockProcess();
            try
            {

                Date = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");
                string PreviousDay = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");
                string SandwichPrevDay = Convert.ToDateTime(Date).AddDays(-2).ToString("dd-MMM-yyyy");
                
                DataSet PlantLock; // Previous Day Plant Lock Checking
                PlantLockCheck(PreviousDay, out PlantLock, PlantValue);
                if (PlantLock.Tables[0].Rows.Count > 0)
                {

                }
                else
                {

                    #region Previous Day Status Reprocessing               
                    //DayStatusReprocessing(PreviousDay, PlantValue); //Making Localized Columns Null
                    #endregion

                    SaveLog("Nullified Columns Logic Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region Previous Day Out Restoring               
                    OutRestoring(PreviousDay, PlantValue); //Restoring OutTime
                    #endregion

                    SaveLog("Outime restored Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region Previous Day Duration EarlyIn Late EarlyOut OverStay
                    DataSet PrevDurn;
                    PrevDayDuration(PreviousDay, out PrevDurn, PlantValue);
                    if (PrevDurn.Tables[0].Rows.Count > 0)
                    {
                        // Dataset Generated for Duration EarlyIn EarlyOut Calculation
                        string WorkDate = PrevDurn.Tables[0].Rows[0][@"WorkDate"].ToString();
                      
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "'and isnull(ProcessIntime,'')!='' and isnull(ProcessOuttime,'')!='' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < PrevDurn.Tables[0].Rows.Count; i++)
                        {
                            string RowId = PrevDurn.Tables[0].Rows[i][@"RowId"].ToString();
                            string ProcessInTime = clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ProcessIntime"]).ToString();
                            string ProcessOutTime = clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ProcessOuttime"]).ToString();
                            string ShiftOutTime = clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftOutTime"]).ToString();
                            string ShiftInTime = clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftInTime"]).ToString();
                            string CalDuration = clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"CalDuration"]).ToString();
                            double ShiftEarlyInMargin = Convert.ToDouble(clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftEarlyInMargin"]).ToString());
                            double ShiftLateInMargin = Convert.ToDouble(clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftLateInMargin"]).ToString());
                            double ShiftEarlyOutMargin = Convert.ToDouble(clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftEarlyOutMargin"]).ToString());
                            double ShiftLateOutMargin = Convert.ToDouble(clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftLateOutMargin"]).ToString());

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                // Updation in AttdnProcessData 
                                dr["Duration"] = CalDuration;
                                dr["EarlyLateIn"] = DBNull.Value;
                                dr["EarlyLateOut"] = DBNull.Value;
                                if (ShiftInTime != "")
                                {
                                    // If Intime + EarlyMargin < ShiftInTime :- EarlyIn
                                    if (Convert.ToDateTime(ProcessInTime).AddMinutes(ShiftEarlyInMargin) < Convert.ToDateTime(ShiftInTime))
                                    {
                                        TimeSpan ts = Convert.ToDateTime(ShiftInTime).Subtract(Convert.ToDateTime(ProcessInTime));
                                        dr["EarlyIn"] = ts.TotalMinutes;
                                        dr["EarlyLateIn"] = "EI";
                                    }
                                    else
                                    {
                                        dr["EarlyIn"] = 0;

                                    }
                                    // If Intime - LateMargin > ShiftInTime :- LateIn
                                    if (Convert.ToDateTime(ProcessInTime).AddMinutes(-ShiftLateInMargin) > Convert.ToDateTime(ShiftInTime))
                                    {
                                        TimeSpan ts = Convert.ToDateTime(ProcessInTime).Subtract(Convert.ToDateTime(ShiftInTime));
                                        dr["LateIn"] = ts.TotalMinutes;
                                        dr["EarlyLateIn"] = "LI";
                                    }
                                    else
                                    {
                                        dr["LateIn"] = 0;

                                    }
                                }
                                if (ShiftOutTime != "")
                                {
                                    // If OutTime + EarlyMargin < ShiftOutTime :- EarlyOut
                                    if (Convert.ToDateTime(ProcessOutTime).AddMinutes(ShiftEarlyOutMargin) < Convert.ToDateTime(ShiftOutTime))
                                    {

                                        TimeSpan ts = Convert.ToDateTime(ShiftOutTime).Subtract(Convert.ToDateTime(ProcessOutTime));
                                        dr["EarlyOut"] = ts.TotalMinutes;
                                        dr["EarlyLateOut"] = "EO";
                                    }
                                    else
                                    {
                                        dr["EarlyOut"] = 0;

                                    }
                                    // If OutTime - LateMargin < ShiftOutTime :- 0
                                    if (Convert.ToDateTime(ProcessOutTime).AddMinutes(-ShiftLateOutMargin) < Convert.ToDateTime(ShiftOutTime))
                                    {
                                        dr["LateOut"] = 0;

                                    }
                                    // LateOut
                                    else
                                    {
                                        TimeSpan ts = Convert.ToDateTime(ProcessOutTime).Subtract(Convert.ToDateTime(ShiftOutTime));
                                        dr["LateOut"] = ts.TotalMinutes;
                                        dr["EarlyLateOut"] = "LO";
                                    }
                                }
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }

                    #endregion

                    SaveLog("Duration Logic Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region Previous Day DurationStatus Flagging
                    DataSet PrevDurationStat;
                    PrevDurationStatusCal(PreviousDay, out PrevDurationStat, PlantValue);
                    if (PrevDurationStat.Tables[0].Rows.Count > 0)
                    {
                        // Duration Staus on the Basis of Duration of Work of Employee
                        string WorkDate = PrevDurationStat.Tables[0].Rows[0][@"WorkDate"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < PrevDurationStat.Tables[0].Rows.Count; i++)
                        {
                            string RowId = PrevDurationStat.Tables[0].Rows[i][@"RowId"].ToString();
                            string ShortDuration = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"ShiftShortDuration"]).ToString();
                            string FullDayDuration = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"ShiftFullDayDuration"]).ToString();
                            string HalfDayDuration = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"ShiftHalfDayDuration"]).ToString();
                            string Duration = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"Duration"]).ToString();
                            string In = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"ProcessIntime"]).ToString();
                            string Out = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"ProcessOutTime"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                // In & Out Both Present
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                if (Duration.ToString() != "" &&
                                    FullDayDuration.ToString() != ""
                                    && ShortDuration.ToString() != ""
                                    && HalfDayDuration.ToString() != "")
                                {
                                    if (Convert.ToDouble(Duration) >= Convert.ToDouble(FullDayDuration))
                                    {
                                        dr["DurationStatus"] = "FD"; // Full Day
                                    }
                                    else if (Convert.ToDouble(Duration) >= Convert.ToDouble(HalfDayDuration))
                                    {
                                        dr["DurationStatus"] = "HD"; // Half Day
                                    }
                                    else if (Convert.ToDouble(Duration) >= Convert.ToDouble(ShortDuration))
                                    {
                                        dr["DurationStatus"] = "SD"; // Short Day
                                    }
                                    else if (Convert.ToDouble(Duration) < Convert.ToDouble(ShortDuration))
                                    {
                                        dr["DurationStatus"] = "A"; // Absent
                                    }
                                }
                                else
                                {
                                    // Missing In : Out
                                    if (In.ToString() == "" &&
                                         Out.ToString() == "")
                                    {
                                        dr["DurationStatus"] = "NP"; // No Punch
                                    }
                                    else if (In.ToString() == "" &&
                                        Out.ToString() != "")
                                    {
                                        dr["DurationStatus"] = "IM"; //In Miss
                                    }
                                    else if (In.ToString() != "" &&
                                        Out.ToString() == "")
                                    {
                                        dr["DurationStatus"] = "OM"; //Out Miss
                                    }

                                }


                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();

                            }
                        }
                        SaveDataSets(dsRef);

                    }

                    #endregion

                    SaveLog("Duration Status Logic Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region Previous Day Status Code              
                    PrevDayStatusCodeData(PreviousDay, PlantValue); // DayStausCode Text Join 
                                                                    //HolidayStatus + WeeklyStatus + DurationStatus + EarlyLateIn + EarlyLateOut + LeaveStatus
                    #endregion

                    SaveLog("DayStatus Code Logic Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region Prev User Day Status 

                    PrevProcessDayStatusUpdate(PreviousDay, PlantValue);

                    #endregion

                    SaveLog("User DayStatus Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region Prev Process FinalDayStatus 
                    DataSet PrevFinalDayStat; // Process DayStatus & Manual DayStatus Comparison
                    PreProcessFinalDayStatus(PreviousDay, out PrevFinalDayStat, PlantValue);
                    if (PrevFinalDayStat.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = PrevFinalDayStat.Tables[0].Rows[0][@"WorkDate"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < PrevFinalDayStat.Tables[0].Rows.Count; i++)
                        {
                            // Localizing Processed FinalDayStatus 

                            string RowId = clsWebLib.RetValidLen(PrevFinalDayStat.Tables[0].Rows[i][@"RowId"]).ToString();
                            string Result = clsWebLib.RetValidLen(PrevFinalDayStat.Tables[0].Rows[i][@"Result"]).ToString();
                            string SandwichFlag = clsWebLib.RetValidLen(PrevFinalDayStat.Tables[0].Rows[i][@"SandwichStatusFlag"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                // Updations in APD Table 
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                dr["ProcessFinalDayStatus"] = Result;
                                dr["SandwichFlag"] = SandwichFlag;
                                if (SandwichFlag != "0")
                                {
                                    dr["SandwichReprocess"] = true;
                                }
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    SaveLog("Process FinalDayStatus Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region Sandwich Saving  
                    DataSet SandwichSavingData;
                    SandwichLogic(SandwichPrevDay, out SandwichSavingData, PlantValue);
                    if (SandwichSavingData.Tables[0].Rows.Count > 0)
                    {

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + PreviousDay + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");
                        string newformat = Convert.ToDateTime(PreviousDay).ToString("yyyyMMdd");


                        for (int i = 0; i < SandwichSavingData.Tables[0].Rows.Count; i++)
                        {

                            string EmpId = clsWebLib.RetValidLen(SandwichSavingData.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                            if (EmpId == "2200009")
                            {
                                // do nothing
                            }
                            string PrevDaySandwich = clsWebLib.RetValidLen(SandwichSavingData.Tables[0].Rows[i][@"SandwichFlag"]).ToString();

                            // Updation in AttdnProcessData
                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                string ToDaySandwich = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"SandwichFlag"]).ToString();

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                if (PrevDaySandwich == "0" && ToDaySandwich == "2")
                                {
                                    dr["SandwichFlag"] = "0"; //Today Change                                    
                                    dr["SandwichReprocess"] = false;

                                }

                                else if (PrevDaySandwich == "1" && ToDaySandwich == "2")
                                {
                                    dr["SandwichFlag"] = "2"; //Today Change
                                }

                                else if (PrevDaySandwich == "0" && ToDaySandwich == "3")
                                {
                                    dr["SandwichFlag"] = "0"; //Today Change
                                    dr["SandwichReprocess"] = false;
                                }

                                else if (PrevDaySandwich == "0" && ToDaySandwich == "4")
                                {
                                    dr["SandwichFlag"] = "0"; //Today Change
                                    dr["SandwichReprocess"] = false;
                                }

                                else if (PrevDaySandwich == "1" && ToDaySandwich == "3")
                                {
                                    dr["SandwichFlag"] = "3"; //Today Change
                                }
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }

                        }

                        SaveDataSets(dsRef); // Saving Main DataSet                  

                    }
                    #endregion

                    SaveLog("Sandwich Data Entry Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region Previous Payroll DayStatus 
                    PrePayrollDayStatus(PreviousDay, PlantValue); // On the Priority Check of Sandwich and ProcessFinalDayStatus 
                    #endregion                

                    #region Prev Process Payroll DayStatus 
                    DataSet PrevPayrollDayStat;
                    PreProcessPayrollDayStatusData(PreviousDay, out PrevPayrollDayStat, PlantValue);
                    if (PrevPayrollDayStat.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = PrevPayrollDayStat.Tables[0].Rows[0][@"WorkDate"].ToString();
                     
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < PrevPayrollDayStat.Tables[0].Rows.Count; i++)
                        {
                            // Localizing Diff Flags on the Basis of Processed FinalDayStatus 

                            string RowId = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"RowId"]).ToString();
                            string OtApplicable = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"OTApplicable"]).ToString();
                            string Goodwork = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"GoodWorkApplicable"]).ToString();
                            string AutoLock = clsWebLib.GetBoolData(PrevPayrollDayStat.Tables[0].Rows[i][@"AutoLock"]).ToString();

                            #region For Using them to get the Summary
                            string TotalPresent = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"PresentValue"]).ToString();
                            string TotalLate = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"LateValue"]).ToString();
                            string TotalAbsent = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"AbsentValue"]).ToString();
                            string TotalLv = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"LvValue"]).ToString();
                            string TotalCompAssignLv = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"CompAssignLvValue"]).ToString();
                            string TotalWeekOff = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"WeekOffValue"]).ToString();
                            string TotalHoliDay = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"HoliDayValue"]).ToString();
                            string TotalWeekOffHoliDay = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"WeekOffHoliDayValue"]).ToString();
                            string TotalPayDay = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"TotalPayDay"]).ToString();
                            string TotalNonPayDay = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"TotalNonPayDay"]).ToString();
                            string TotalWorkingDay = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"WorkingDay"]).ToString();
                            string ActualWorkingDay = clsWebLib.RetValidLen(PrevPayrollDayStat.Tables[0].Rows[i][@"ActualWorkingDay"]).ToString();

                            #endregion

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                // Updations in APD Table 
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                #region Null Flagging

                                dr["PresentValue"] = DBNull.Value;
                                dr["LateValue"] = DBNull.Value;
                                dr["AbsentValue"] = DBNull.Value;
                                dr["LvValue"] = DBNull.Value;
                                dr["CompAssignLvValue"] = DBNull.Value;
                                dr["WeekOffValue"] = DBNull.Value;
                                dr["HoliDayValue"] = DBNull.Value;
                                dr["WeekOffHoliDayValue"] = DBNull.Value;
                                dr["PayDayValue"] = DBNull.Value;
                                dr["NonPayDayValue"] = DBNull.Value;
                                dr["WorkingDayValue"] = DBNull.Value;
                                dr["ActualWorkingDayValue"] = DBNull.Value;

                                #endregion

                                dr["DayTypeOTApplicable"] = OtApplicable;
                                dr["DayTypeGoodWorkApplicable"] = Goodwork;
                                if (AutoLock == "True")
                                {
                                    // Individual Lock
                                    dr["IsLock"] = true;
                                    dr["LockedDate"] = DateTime.Now;
                                    dr["LockedBy"] = "AutoLock";
                                }
                                dr["PresentValue"] = TotalPresent;
                                dr["LateValue"] = TotalLate;
                                dr["AbsentValue"] = TotalAbsent;
                                dr["LvValue"] = TotalLv;
                                dr["CompAssignLvValue"] = TotalCompAssignLv;
                                dr["WeekOffValue"] = TotalWeekOff;
                                dr["HoliDayValue"] = TotalHoliDay;
                                dr["WeekOffHoliDayValue"] = TotalWeekOffHoliDay;
                                dr["PayDayValue"] = TotalPayDay;
                                dr["NonPayDayValue"] = TotalNonPayDay;
                                dr["WorkingDayValue"] = TotalWorkingDay;
                                dr["ActualWorkingDayValue"] = ActualWorkingDay;
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    SaveLog("Process Payroll DayStatus Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region PrevDay OverStay UnderStay 
                    DataSet PrevDayOT;
                    OverUnderStayPrevDay(PreviousDay, out PrevDayOT, PlantValue);
                    if (PrevDayOT.Tables[0].Rows.Count > 0)
                    {
                        // OverStay underStay DataSet Generation using (Duration - ShiftHoursWithoutOT)
                        string WorkDate = PrevDayOT.Tables[0].Rows[0][@"WorkDate"].ToString();
                   
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "'and Duration >0 and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < PrevDayOT.Tables[0].Rows.Count; i++)
                        {
                            string RowId = PrevDayOT.Tables[0].Rows[i][@"RowId"].ToString();
                            double OverUnderStay = Convert.ToDouble(clsWebLib.RetValidLen(PrevDayOT.Tables[0].Rows[i][@"OverUnderStay"]).ToString());

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                if (OverUnderStay > 0)
                                {
                                    // Extra Work
                                    dr["OverStay"] = OverUnderStay;
                                    dr["UnderStay"] = 0;
                                }
                                else if (OverUnderStay == 0)
                                {
                                    dr["OverStay"] = 0;
                                    dr["UnderStay"] = 0;
                                }
                                else
                                {
                                    // Less Work 
                                    dr["OverStay"] = 0;
                                    dr["UnderStay"] = OverUnderStay;
                                }

                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }

                    #endregion

                    SaveLog("OverStay Logic Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region Prev DayOT Calculation 
                    DataSet PrevOTCalculate;
                    PrevDayOTCalculation(PreviousDay, out PrevOTCalculate, PlantValue);
                    if (PrevOTCalculate.Tables[0].Rows.Count > 0)
                    {
                        // OverTime DataSet Using OT Per Minute Policy
                        var WkDate = PrevOTCalculate.Tables[0].Rows[0][@"WorkDate"].ToString();
                    
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where IsOTEntitled='1' and WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";
                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        var sqly = @"select * from PlantWiseHRMSSetting where PlantID='" + PlantValue + "'";
                        objCon.OpenDataSetThroughAdapter(sqly, out DataSet OTMode, false, false, "", "1");

                        // Settings of Modes from PlantWiseHRMSSetting
                        string OTModeValue = clsWebLib.RetValidLen(OTMode.Tables[0].Rows[0][@"ResultendOT"]).ToString();

                        // 0 means Punched Based, 1 means Manual, 2 means Mixed

                        for (int i = 0; i < PrevOTCalculate.Tables[0].Rows.Count; i++)
                        {

                            string RowId = clsWebLib.RetValidLen(PrevOTCalculate.Tables[0].Rows[i][@"RowId"]).ToString();
                            string Result = clsWebLib.RetValidLen(PrevOTCalculate.Tables[0].Rows[i][@"Result"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                string PastManualOT = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"ManualOt"]).ToString();
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;

                                if (OTModeValue == "0")
                                {
                                    // Punched Based
                                    if (Result != "")
                                    {
                                        if (Convert.ToDouble(Result) > 0)
                                        {
                                            dr.BeginEdit();
                                            dr["ProcessedOT"] = Result;
                                            dr["CalculatedOT"] = Result; // For Visiblity
                                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                            dr.EndEdit();
                                        }
                                    }

                                }
                                else if (OTModeValue == "1")
                                {
                                    if (Result != "")
                                    {
                                        // Manual Mode
                                        if (PastManualOT != "")
                                        {
                                            double SmallerValue = Math.Min(Convert.ToDouble(PastManualOT), Convert.ToDouble(Result));
                                            dr.BeginEdit();
                                            dr["ProcessedOT"] = SmallerValue;
                                            dr["CalculatedOT"] = Result;  // For Visiblity
                                            dr.EndEdit();                                            
                                        }
                                    }
                                }
                                else
                                {
                                    // Mixed Mode
                                    if (Result != "")
                                    {
                                        if (PastManualOT != "")
                                        {
                                            if (Convert.ToDouble(PastManualOT) >= 0)
                                            {
                                                if (Convert.ToDouble(PastManualOT) < Convert.ToDouble(Result))
                                                {
                                                    // If Manual is less than Processed
                                                    dr.BeginEdit();
                                                    dr["ProcessedOT"] = PastManualOT;
                                                    dr["CalculatedOT"] = Result;  // For Visiblity
                                                    dr.EndEdit();
                                                }
                                                else
                                                {
                                                    // Otherwise Processed
                                                    dr.BeginEdit();
                                                    dr["ProcessedOT"] = Result;
                                                    dr["CalculatedOT"] = Result;  // For Visiblity
                                                    dr.EndEdit();
                                                }
                                            }
                                            else
                                            {
                                                // Otherwise Processed
                                                dr.BeginEdit();
                                                dr["ProcessedOT"] = Result;
                                                dr["CalculatedOT"] = Result;  // For Visiblity
                                                dr.EndEdit();
                                            }
                                        }
                                        else
                                        {
                                            // Otherwise Processed
                                            dr.BeginEdit();
                                            dr["ProcessedOT"] = Result;
                                            dr["CalculatedOT"] = Result;  // For Visiblity
                                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                            dr.EndEdit();
                                        }
                                    }
                                }
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region Future ManualOT
                    DataSet PrevManualOT;
                    ManualOT(PreviousDay, out PrevManualOT, PlantValue);
                    if (PrevManualOT.Tables[0].Rows.Count > 0)
                    {
                        // OTFromApp OT Of Future
                        var WkDate = PrevManualOT.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where IsOTEntitled='1' and WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        // Settings of Modes from PlantWiseHRMSSetting
                        var sqly = @"select * from PlantWiseHRMSSetting where PlantID='" + PlantValue + "'";
                        objCon.OpenDataSetThroughAdapter(sqly, out DataSet OTMode, false, false, "", "1");

                        string OTModeValue = clsWebLib.RetValidLen(OTMode.Tables[0].Rows[0][@"ResultendOT"]).ToString();
                        // 0 means Punched Based 1 means Manual 2 means Mixed

                        for (int i = 0; i < PrevManualOT.Tables[0].Rows.Count; i++)
                        {

                            string EmpId = clsWebLib.RetValidLen(PrevManualOT.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                            string ManualOT = clsWebLib.RetValidLen(PrevManualOT.Tables[0].Rows[i][@"OThour"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                string AutoOT = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"OTHr"]).ToString();
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;

                                if (OTModeValue == "1")
                                {
                                    // Manual Mode
                                    if (ManualOT != "")
                                    {
                                        if (Convert.ToDouble(ManualOT) >= 0)
                                        {
                                            dr.BeginEdit();
                                            dr["ProcessedOT"] = ManualOT;
                                            dr["ManualOt"] = ManualOT;
                                            dr.EndEdit();
                                        }
                                    }
                                }

                                else if (OTModeValue == "2")
                                {
                                    // Mixed Mode
                                    if (ManualOT != "")
                                    {
                                        if (Convert.ToDouble(ManualOT) >= 0)
                                        {
                                            if (Convert.ToDouble(ManualOT) < Convert.ToDouble(AutoOT))
                                            {
                                                // if Manual is less than Processed
                                                dr.BeginEdit();
                                                dr["ProcessedOT"] = ManualOT;
                                                dr["ManualOt"] = ManualOT;
                                                dr.EndEdit();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    SaveLog("Processed OT Calculation Ran Successfully for " + PreviousDay + " ...", PlantValue, false);
                  
                    #region OTEntitled But OT Not Applicable Employees
                    DataSet OTNotApplicable;
                    AutoConfirmedDataSet(PreviousDay, out OTNotApplicable, PlantValue);
                    if (OTNotApplicable.Tables[0].Rows.Count > 0)
                    {
                        string RowMaster = "''";
                        for (int i = 0; i < OTNotApplicable.Tables[0].Rows.Count; i++)
                        {
                            string RowId = clsWebLib.RetValidLen(OTNotApplicable.Tables[0].Rows[i][@"RowId"]).ToString();
                            RowMaster += ",'" + RowId + "'";
                        }
                        if (RowMaster != "''")
                        { 
                            ConfirmOTFlag(RowMaster);
                        }
                    }

                    #endregion

                    SaveLog("OT Not Applicable Auto Confirm Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                    #region OT Entitled Employees whose ProcessedOT is 0

                    // Confirming the OT of Employees Whose Processed OT is 0
                    UpdateZeroProcessedOTEmp(PreviousDay, PlantValue);

                    #endregion

                    SaveLog("0 Processed OT Auto Confirm Ran Successfully for " + PreviousDay + " ...", PlantValue, false);

                }
                _lock.UnlockProcess();
            }
            catch (Exception ex)
            {
                _lock.UnlockProcess();
                throw ex;
            }

        }
        #endregion

        #region ManualScheduler Source Data
        public void ManualInStatusCalculate(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select Format(WorkDate,'yyyy-MMM-dd')WorkDate,EmpSystemID,
                Format(ap.ProcessIntime,'yyyy-MMM-dd HH:mm:ss')ProcessIntime,				
				Format(ap.ShiftInTime,'yyyy-MMM-dd HH:mm:ss')ShiftInTime,  
                sd.ShiftEarlyInMargin,sd.ShiftLateInMargin,ap.RowId                
                from Attdnprocessdata  ap
                left join ShiftDefination sd on sd.SystemID=ap.ShiftSystemID
                where ManualFlag=1
				and ap.PlantID='" + Plant+ "' order by ap.WorkDate asc";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }  
        public void ManualDuration(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select * from (select Format(WorkDate,'yyyy-MMM-dd')WorkDate,EmpSystemID,
                datediff(minute,ap.ProcessInTime,ap.ProcessOutTime) 
                as CalDuration, Format(ap.ProcessInTime,'yyyy-MMM-dd HH:mm:ss')ProcessInTime,
				Format(ap.ProcessOutTime,'yyyy-MMM-dd HH:mm:ss')ProcessOutTime,
				Format(ap.ShiftInTime,'yyyy-MMM-dd HH:mm:ss')ShiftInTime, 
                Format(ap.ShiftOutTime,'yyyy-MMM-dd HH:mm:ss')ShiftOutTime, 
                sd.ShiftEarlyInMargin,sd.ShiftEarlyOutMargin,sd.ShiftLateInMargin,
                sd.ShiftLateOutMargin,ap.RowId
                from Attdnprocessdata  ap
                left join ShiftDefination sd on sd.SystemID=ap.ShiftSystemID
                where ap.ManualFlag=1 and ap.PlantID='" + Plant + @"' and isnull(ap.ProcessInTime,'')!='' 
				and isnull(ap.ProcessOutTime,'')!='') as dd
				where dd.CalDuration>=0 order by WorkDate asc";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ManualOverUnderStayData(out DataSet ds, string Plant)
        {

            try
            {
                ConnectionManager.DAL.ConManager objCon;

                var sql = @"select ap.EmpSystemID,Format(ap.WorkDate,'yyyy-MMM-dd')WorkDate,
                ap.Duration,ap.ShiftSystemID,ap.RowId,
                OverUnderStay=case when ap.DayTypeOtApplicable=2 then
				ap.Duration else
				(ap.Duration-isnull(ap.ShiftHoursWithoutOT,'0'))
				end
                from attdnprocessdata ap
                where Duration >0 and ap.PlantID='" + Plant + @"'
				AND ManualFlag=1 order by WorkDate asc";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }

            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ManualDurationStatusCal(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select p.EmpSystemID,Format(p.WorkDate,'yyyy-MMM-dd')WorkDate,p.Duration
                ,p.ShiftHalfDayDuration,p.ShiftFullDayDuration,p.ProcessInTime,p.ProcessOutTime,
                p.ShiftShortDuration,p.RowId from AttdnProcessData p 
                where ManualFlag=1 
                and p.PlantID='" + Plant + "' order by WorkDate asc";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ManualDayStatusCodeData(string Plant , string empMaster)
        {

            try
            {
                var sql = "";
                string empMaster1 = (clsWebLib.RetValidLen(empMaster).ToString());
                if (empMaster1 == "")
                {
                    sql = @"UPDATE AttdnProcessData Set DayStatusCode=(ISNULL(HolidayStatus,'')+	
											ISNULL(WeeklyStatus,'')+ISNULL(DurationStatus,'')+
								ISNULL(EarlyLateIn,'')+ISNULL(EarlyLateOut,'')
								+ISNULL(LeaveStatus,'')),DateUpdated=GETDATE() 
                        WHERE PlantID='" + Plant + @"'
								AND ManualFlag=1 and IsLock=0";
                }
                else
                {
                    sql = @"UPDATE AttdnProcessData Set DayStatusCode=(ISNULL(HolidayStatus,'')+	
											ISNULL(WeeklyStatus,'')+ISNULL(DurationStatus,'')+
								ISNULL(EarlyLateIn,'')+ISNULL(EarlyLateOut,'')
								+ISNULL(LeaveStatus,'')),DateUpdated=GETDATE() 
                        WHERE PlantID='" + Plant + @"' and RowId in("+empMaster+@")
								AND ManualFlag=1 and IsLock=0";
                }
                

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ManualDayStatus(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.EmpSystemID,p.DayStatusCode,dt.DayType,
                        format(p.WorkDate,'yyyy-MMM-dd')WorkDate,p.RowId from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
   					                    left join DayStatusHeader dh on dh.Id=p.DayStatusHeaderId
									        left join DayStatus ds on ds.headerId=dh.Id
											left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId
									        where ManualFlag=1 and ds.Code=p.DayStatusCode
									        and ei.PlantId='" + Plant + "' order by workdate asc";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ManualFinalDayStatus(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.EmpSystemID,Result=dt.DayType,dt.SandwichStatusFlag,
				format(p.WorkDate,'yyyy-MMM-dd')WorkDate,p.RowId				                
	            from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                        left join DayStatusHeader dh on dh.Id=p.DayStatusHeaderId
						left join DayStatus ds on ds.headerId=dh.Id
						left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId									       
						where ManualFlag=1 						
						and dt.DayType=ISNULL(p.ManualDayStatus,p.ProcessDayStatus)
						and ei.PlantId='" + Plant+ "' order by WorkDate asc";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PayrollDayStatus(string Plant , string empMaster)
        {

            try
            {
                var sql = "";
                string empMaster1 = (clsWebLib.RetValidLen(empMaster).ToString());
                if (empMaster1 == "")
                {
                    sql = @"UPDATE 	AttdnProcessData Set DayStatus=ISNULL(ISNULL(ManualDayStatus,SandwichStatus),ProcessDayStatus)
				,UpdatedBy='Schedule',DateUpdated=GETDATE()
								WHERE PlantID='" + Plant + @"'
								AND ManualFlag=1";
                }
                else
                {
                    sql = @"UPDATE 	AttdnProcessData Set DayStatus=ISNULL(ISNULL(ManualDayStatus,SandwichStatus),ProcessDayStatus) 
				,UpdatedBy='Schedule',DateUpdated=GETDATE()
								WHERE PlantID='" + Plant + @"' and RowId in ("+empMaster+@")
								AND ManualFlag=1";
                }

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void ProcessedOTCalculation(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.EmpSystemID,p.RowId,
                format(p.WorkDate,'yyyy-MMM-dd')WorkDate,Result=
                case when p.DayTypeOTApplicable='1' then 
                (select distinct ot.AllotedOT from OTPerMinutePolicy ot
                where ot.PlantId=p.PlantID and ot.OverstayOrEarlyOut=p.OverStay) 
                when p.DayTypeOTApplicable='2' then (select distinct ot.OffDayAllotedOT 
				from OTPerMinutePolicy ot
                where ot.PlantId=p.PlantID and ot.OverstayOrEarlyOut=p.Duration)
				when p.DayTypeOTApplicable='3' then (select distinct ot.AllotedOT 
				from OTPerMinutePolicy ot
                where ot.PlantId=p.PlantID and ot.OverstayOrEarlyOut=p.OverStay-p.EarlyIn) 
				end
                from AttdnProcessData p
                left join org.Plant pl on pl.Id=p.PlantID
                left join OTPerMinutePolicy ot on ot.PlantId=pl.Id
                        where ManualFlag=1 and p.IsOTEntitled='1'
						and p.DayTypeOTApplicable != 0 and p.Duration>0
						and p.PlantId='" + Plant + "' order by WorkDate asc";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ProcessManualFlag(string MainFlagId)
        {
            try
            {
                var sql = @"update AttdnProcessData set ManualFlag=0,DateUpdated=GetDate()
                where RowID IN(" + MainFlagId + @")";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void CheckerFunction(ref string ManualFlagRowId, string Value)
        {
            if (ManualFlagRowId.Contains(Value))
            {
                return;
            }
            else
            {
                ManualFlagRowId += ",'" + Value + "'";
            }
        }
        public void ManualReprocessing(string Plant, string empMaster)
        {

            try
            {               
                string empMaster1 = (clsWebLib.RetValidLen(empMaster).ToString());
                if (empMaster1 != "")
                {                   
                    var sql = @"update AttdnProcessData set Duration=null,earlyin=null,latein=null,LateOut=null,
                    earlyout=null,OverStay=null,UnderStay=null,DurationStatus=null,EarlyLateIn=null,EarlyLateOut=null,
                    DayStatusCode=null,ProcessDayStatus=null,ProcessedOT=0,IsLock=0,ProcessFinalDayStatus=null,DayStatus=null,
                    LockedBy=null,StandardOT=null,PlanOT=null,AppliedOTLimit=null,
                    AllowedOTLimit=null,TargetOT=null,AdditionalOT=null,SandwichReprocess=0,
                    LockedDate=null,IsOTComfirm=0,OTComfirmBy=null,DateOTComfirm=null ,CalculatedOT=0
                    where PlantID='" + Plant+@"'
                    and ManualFlag=1 and RowId IN(" + empMaster + @")";
                  
                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sql, true, "1");
                    objCone.CommitTransaction();
                }
                else
                {
                    var sql = @"update AttdnProcessData set Duration=null,earlyin=null,latein=null,LateOut=null,
                    earlyout=null,OverStay=null,UnderStay=null,DurationStatus=null,EarlyLateIn=null,EarlyLateOut=null,
                    DayStatusCode=null,ProcessDayStatus=null,ProcessedOT=0,IsLock=0,ProcessFinalDayStatus=null,DayStatus=null,
                    LockedBy=null,IsOTComfirm=0,OTComfirmBy=null,DateOTComfirm=null,CalculatedOT=0,
                    LockedDate=null,StandardOT=null,PlanOT=null,AppliedOTLimit=null,
                    AllowedOTLimit=null,TargetOT=null,AdditionalOT=null
                    where PlantID='" + Plant + @"'
                    and ManualFlag=1";

                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sql, true, "1");
                    objCone.CommitTransaction();
                }
               
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ManualOutRestored(string Plant, string empMaster)
        {

            try
            {
                string empMaster1 = (clsWebLib.RetValidLen(empMaster).ToString());
                if (empMaster1 != "")
                {               
                    var sqlx = @"update AttdnProcessData set OutTime=isnull(ProcessOuttime,OutTime),
                    ManualOutTime=isnull(OriginalManualOutTime,ManualOutTime)
                    where ManualFlag=1 and PlantID='"+Plant+ @"'
                    and IsOTEntitled='1'
                    and IsOTComfirm=0 and RowId IN(" + empMaster + @")";

                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sqlx, true, "1");
                    objCone.CommitTransaction();
                }
                else
                {                 
                    var sqlx = @"update AttdnProcessData set OutTime=isnull(ProcessOuttime,OutTime),
                    ManualOutTime=isnull(OriginalManualOutTime,ManualOutTime)
                    where ManualFlag=1 and PlantID='" + Plant + @"'
                    and IsOTEntitled='1'
                    and IsOTComfirm=0";

                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sqlx, true, "1");
                    objCone.CommitTransaction();
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ManualPayrollDayStatus(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.EmpSystemID,Result=dt.DayType,
                dt.OTApplicable,dt.AutoLock,dt.GoodWorkApplicable,
				format(p.WorkDate,'yyyy-MMM-dd')WorkDate,p.RowId,
				isnull(dt.PresentValuePD,'0')PresentValue,isnull(dt.LateValueLV,'0')LateValue,isnull(dt.AbsentValueAB,'0')AbsentValue,
				isnull(dt.LeaveValueLP,'0')LvValue,isnull(dt.CompAssignLv,'0')CompAssignLvValue,
                isnull(dt.WeeklyOffWO,'0')WeekOffValue,isnull(dt.HolidayH,'0')HoliDayValue,isnull(dt.WeekOffHoliDayWOH,'0')WeekOffHoliDayValue,
				isnull(dt.TotalWorkingDay,'0')WorkingDay,
				isnull(dt.ActualWorkingDay,'0')ActualWorkingDay,isnull(dt.PayDay,'0')TotalPayDay,isnull(dt.NonPayDay,'0')TotalNonPayDay                
	            from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                        left join DayStatusHeader dh on dh.Id=p.DayStatusHeaderId
						left join DayStatus ds on ds.headerId=dh.Id
						left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId									       
						where ManualFlag=1 						
						and dt.DayType=p.DayStatus
						and ei.PlantId='" + Plant + "' order by WorkDate asc";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion

        #region Manual Scheduler
        public void ManualScheduler(string PlantValue , string manualempidfromscreens=null)
        {
            try
            {
                string ManualFlagRowId = "''";

                string empMaster = clsWebLib.RetValidLen(manualempidfromscreens).ToString();
                string empList = manualempidfromscreens;

                #region Manual Day Status Nullifying Localized Values              
                //ManualReprocessing(PlantValue, empList); // Reprocessing Manual Employees called from Screen
                #endregion
                
                SaveLog("Manual Nullified Columns Logic Ran Successfully ...", PlantValue, false);

                #region Manual Day Status Nullifying Localized Values              
                ManualOutRestored(PlantValue, empList); // Reprocessing OutTime of Employees
                #endregion

                SaveLog("Manual Reprocessing OutTime Ran Successfully ...", PlantValue, false);

                #region Manual In Status Logic
                DataSet ManualInStatus;
                ManualInStatusCalculate(out ManualInStatus, PlantValue);
                if (ManualInStatus.Tables[0].Rows.Count > 0)
                {
                    // In Status on the Basis of FinalIn
                    var WkDate = ManualInStatus.Tables[0].Rows[0][@"WorkDate"].ToString();
                    var sqlx = "";
                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");

                    if (empMaster == "")
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "'";
                    }
                    else
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and PlantID='" + PlantValue + "' and ManualFlag=1 and RowId in(" + empList + ")";
                    }
                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                    for (int i = 0; i < ManualInStatus.Tables[0].Rows.Count; i++)
                    {
                        // Logic on the basis of Shift Early & Late Margin
                        string RowId = clsWebLib.RetValidLen(ManualInStatus.Tables[0].Rows[i][@"RowId"]).ToString();
                        string ProcessIntime = clsWebLib.RetValidLen(ManualInStatus.Tables[0].Rows[i][@"ProcessIntime"]).ToString();
                        string ShiftInTime = clsWebLib.RetValidLen(ManualInStatus.Tables[0].Rows[i][@"ShiftInTime"]).ToString();
                        double ShiftEarlyInMargin = Convert.ToDouble(clsWebLib.RetValidLen(ManualInStatus.Tables[0].Rows[i][@"ShiftEarlyInMargin"]).ToString());
                        double ShiftLateInMargin = Convert.ToDouble(clsWebLib.RetValidLen(ManualInStatus.Tables[0].Rows[i][@"ShiftLateInMargin"]).ToString());

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {

                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            if (ProcessIntime != "" && ShiftInTime != "")
                            {
                                // Intime + Margin < ShiftInTime :- EarlyIn
                                if (Convert.ToDateTime(ProcessIntime).AddMinutes(ShiftEarlyInMargin) < Convert.ToDateTime(ShiftInTime))
                                {
                                    dr["InStatus"] = "EI";
                                }
                                // Intime - Margin > ShiftInTime :- LateIn
                                else if (Convert.ToDateTime(ProcessIntime).AddMinutes(-ShiftLateInMargin) > Convert.ToDateTime(ShiftInTime))
                                {
                                    dr["InStatus"] = "LI";
                                }

                                else
                                {
                                    dr["InStatus"] = "IN"; // On Time
                                }
                            }
                            else
                            {
                                // If FinalIn Not Present
                                if (ShiftInTime != "")
                                {
                                    if (DateTime.Now > Convert.ToDateTime(ShiftInTime))
                                    {
                                        dr["InStatus"] = "IM"; // In Missing
                                    }
                                    else if (DateTime.Now < Convert.ToDateTime(ShiftInTime))
                                    {
                                        dr["InStatus"] = "O"; //Other
                                    }
                                }
                            }
                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                        }
                    }
                    SaveDataSets(dsRef);

                }
                #endregion
              
                SaveLog("Manual InStatus Logic Ran Successfully ...", PlantValue, false);

                #region Manual Day Duration  
                DataSet ManualDurn;
                ManualDuration(out ManualDurn, PlantValue);
                if (ManualDurn.Tables[0].Rows.Count > 0)
                {
                    // Dataset Generated for Duration EarlyIn EarlyOut Calculation
                    var sqlx = "";
                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    if (empMaster == "")
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and isnull(ProcessInTime,'')!='' and isnull(ProcessOutTime,'')!='' and PlantID='" + PlantValue + "' and ManualFlag=1";
                    }
                    else
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and isnull(ProcessInTime,'')!='' and isnull(ProcessOutTime,'')!='' and PlantID='" + PlantValue + "' and ManualFlag=1 and RowId in(" + empList + ")";
                    }

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    for (int i = 0; i < ManualDurn.Tables[0].Rows.Count; i++)
                    {
                        string RowId = ManualDurn.Tables[0].Rows[i][@"RowId"].ToString();
                        string WorkDate = ManualDurn.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");
                        string ProcessInTime = clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ProcessInTime"]).ToString();
                        string ProcessOutTime = clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ProcessOutTime"]).ToString();
                        string ShiftOutTime = clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftOutTime"]).ToString();
                        string ShiftInTime = clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftInTime"]).ToString();
                        string CalDuration = clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"CalDuration"]).ToString();
                        double ShiftEarlyInMargin = Convert.ToDouble(clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftEarlyInMargin"]).ToString());
                        double ShiftLateInMargin = Convert.ToDouble(clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftLateInMargin"]).ToString());
                        double ShiftEarlyOutMargin = Convert.ToDouble(clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftEarlyOutMargin"]).ToString());
                        double ShiftLateOutMargin = Convert.ToDouble(clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftLateOutMargin"]).ToString());

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {

                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            // Updation in AttdnProcessData 
                            dr["Duration"] = CalDuration;
                            dr["EarlyLateIn"] = DBNull.Value;
                            dr["EarlyLateOut"] = DBNull.Value;

                            // If Intime + EarlyMargin < ShiftInTime :- EarlyIn
                            if (Convert.ToDateTime(ProcessInTime).AddMinutes(ShiftEarlyInMargin) < Convert.ToDateTime(ShiftInTime))
                            {
                                TimeSpan ts = Convert.ToDateTime(ShiftInTime).Subtract(Convert.ToDateTime(ProcessInTime));
                                dr["EarlyIn"] = ts.TotalMinutes;
                                dr["EarlyLateIn"] = "EI";
                            }
                            else
                            {
                                dr["EarlyIn"] = 0;

                            }

                            // If Intime - LateMargin > ShiftInTime :- LateIn
                            if (Convert.ToDateTime(ProcessInTime).AddMinutes(-ShiftLateInMargin) > Convert.ToDateTime(ShiftInTime))
                            {
                                TimeSpan ts = Convert.ToDateTime(ProcessInTime).Subtract(Convert.ToDateTime(ShiftInTime));
                                dr["LateIn"] = ts.TotalMinutes;
                                dr["EarlyLateIn"] = "LI";
                            }
                            else
                            {
                                dr["LateIn"] = 0;

                            }

                            // If OutTime + EarlyMargin < ShiftOutTime :- EarlyOut
                            if (Convert.ToDateTime(ProcessOutTime).AddMinutes(ShiftEarlyOutMargin) < Convert.ToDateTime(ShiftOutTime))
                            {

                                TimeSpan ts = Convert.ToDateTime(ShiftOutTime).Subtract(Convert.ToDateTime(ProcessOutTime));
                                dr["EarlyOut"] = ts.TotalMinutes;
                                dr["EarlyLateOut"] = "EO";
                            }
                            else
                            {
                                dr["EarlyOut"] = 0;

                            }

                            // If OutTime - LateMargin < ShiftOutTime :- 0
                            if (Convert.ToDateTime(ProcessOutTime).AddMinutes(-ShiftLateOutMargin) < Convert.ToDateTime(ShiftOutTime))
                            {
                                dr["LateOut"] = 0;

                            }
                            else
                            {
                                TimeSpan ts = Convert.ToDateTime(ProcessOutTime).Subtract(Convert.ToDateTime(ShiftOutTime));
                                dr["LateOut"] = ts.TotalMinutes;
                                dr["EarlyLateOut"] = "LO";
                            }

                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                            CheckerFunction(ref ManualFlagRowId, RowId);
                        }
                    }
                    SaveDataSets(dsRef);

                }

                #endregion

                SaveLog("Manual Duration Logic Ran Successfully ...", PlantValue, false);

                #region Manual DurationStatus Flagging
                DataSet ManualDurationStat;
                ManualDurationStatusCal(out ManualDurationStat, PlantValue);
                if (ManualDurationStat.Tables[0].Rows.Count > 0)
                {
                    // Duration Staus on the Basis of Duration of Work of Employee

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = "";
                    if (empMaster == "")
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "'";
                    }
                    else
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "' and RowId in (" + empList + ")";
                    }

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    for (int i = 0; i < ManualDurationStat.Tables[0].Rows.Count; i++)
                    {
                        string WorkDate = ManualDurationStat.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string RowId = ManualDurationStat.Tables[0].Rows[i][@"RowId"].ToString();
                        string ShortDuration = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"ShiftShortDuration"]).ToString();
                        string FullDayDuration = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"ShiftFullDayDuration"]).ToString();
                        string HalfDayDuration = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"ShiftHalfDayDuration"]).ToString();
                        string Duration = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"Duration"]).ToString();
                        string In = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"ProcessInTime"]).ToString();
                        string Out = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"ProcessOutTime"]).ToString();

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            // In & Out Both Present
                            if (Duration.ToString() != "" &&
                                FullDayDuration.ToString() != ""
                                && ShortDuration.ToString() != ""
                                && HalfDayDuration.ToString() != "")
                            {
                                if (Convert.ToDouble(Duration) >= Convert.ToDouble(FullDayDuration))
                                {
                                    dr["DurationStatus"] = "FD";  // Full Day
                                }
                                else if (Convert.ToDouble(Duration) >= Convert.ToDouble(HalfDayDuration))
                                {
                                    dr["DurationStatus"] = "HD";  // Half Day
                                }
                                else if (Convert.ToDouble(Duration) >= Convert.ToDouble(ShortDuration))
                                {
                                    dr["DurationStatus"] = "SD";  // Short Day
                                }
                                else if (Convert.ToDouble(Duration) < Convert.ToDouble(ShortDuration))
                                {
                                    dr["DurationStatus"] = "A";  // Absent
                                }
                            }
                            else
                            {

                                // Missing In : Out
                                if (In.ToString() == "" &&
                                     Out.ToString() == "")
                                {
                                    dr["DurationStatus"] = "NP"; // No Punch
                                }
                                else if (In.ToString() == "" &&
                                    Out.ToString() != "")
                                {
                                    dr["DurationStatus"] = "IM"; //In Miss
                                }
                                else if (In.ToString() != "" &&
                                    Out.ToString() == "")
                                {
                                    dr["DurationStatus"] = "OM"; //Out Miss
                                }

                            }


                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                            CheckerFunction(ref ManualFlagRowId, RowId);
                        }
                    }
                    SaveDataSets(dsRef);

                }

                #endregion

                SaveLog("Manual DurationStatus Logic Ran Successfully ...", PlantValue, false);

                #region Manual Day Status Code              
                ManualDayStatusCodeData(PlantValue, empList);
                // DayStausCode Text Join 
                //HolidayStatus + WeeklyStatus + DurationStatus + EarlyLateIn + EarlyLateOut + LeaveStatus
                #endregion

                SaveLog("Manual DayStatusCode Logic Ran Successfully ...", PlantValue, false);

                #region Manual User Day Status 
                DataSet ManualUserDayStat;
                ManualDayStatus(out ManualUserDayStat, PlantValue);
                if (ManualUserDayStat.Tables[0].Rows.Count > 0)
                {
                    // ProcessDayStatus Generation from DayStausCode using DaytypeWith Values
                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = "";
                    if (empMaster == "")
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "'";
                    }
                    else
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "' and RowId in (" + empList + ")";
                    }

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                    for (int i = 0; i < ManualUserDayStat.Tables[0].Rows.Count; i++)
                    {
                        var WkDate = ManualUserDayStat.Tables[0].Rows[i][@"WorkDate"].ToString();
                  
                        string RowId = clsWebLib.RetValidLen(ManualUserDayStat.Tables[0].Rows[i][@"RowId"]).ToString();
                        string DayStatus = clsWebLib.RetValidLen(ManualUserDayStat.Tables[0].Rows[i][@"DayType"]).ToString();

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            // Updation in AttdnProcessData
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["ProcessDayStatus"] = DayStatus;
                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                            CheckerFunction(ref ManualFlagRowId, RowId);
                        }
                    }
                    SaveDataSets(dsRef);

                }
                #endregion

                SaveLog("Manual UserDayStatus Logic Ran Successfully ...", PlantValue, false);

                #region ProcessFinalDayStatus 
                DataSet ManualFinalDayStat;  // Process DayStatus & Manual DayStatus Comparison
                ManualFinalDayStatus(out ManualFinalDayStat, PlantValue);
                if (ManualFinalDayStat.Tables[0].Rows.Count > 0)
                {

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = "";
                    if (empMaster == "")
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "'";
                    }
                    else
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "' and RowId in (" + empList + ")";
                    }

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                    for (int i = 0; i < ManualFinalDayStat.Tables[0].Rows.Count; i++)
                    {
                        // Localizing Diff Flags on the Basis of Processed FinalDayStatus 

                        var WkDate = ManualFinalDayStat.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string RowId = clsWebLib.RetValidLen(ManualFinalDayStat.Tables[0].Rows[i][@"RowId"]).ToString();
                        string Result = clsWebLib.RetValidLen(ManualFinalDayStat.Tables[0].Rows[i][@"Result"]).ToString();
                        string SandwichFlag = clsWebLib.RetValidLen(ManualFinalDayStat.Tables[0].Rows[i][@"SandwichStatusFlag"]).ToString();
                      
                   
                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            // Updations in APD Table 
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();                       
                            dr["ProcessFinalDayStatus"] = Result;
                            dr["SandwichFlag"] = SandwichFlag;
                            if (SandwichFlag != "0")
                            {
                                dr["SandwichReprocess"] = true;
                            }
                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                            CheckerFunction(ref ManualFlagRowId, RowId);
                        }
                    }
                    SaveDataSets(dsRef);

                }
                #endregion

                SaveLog("Manual ProcessFinalDayStatus Logic Ran Successfully ...", PlantValue, false);
            
                #region Payroll DayStatus 
                PayrollDayStatus(PlantValue, empList); // On the Priority Check of Sandwich and ProcessFinalDayStatus 
                #endregion

                #region Process PayrollDayStatus 
                DataSet ManualPayrollDayStat;  
                ManualPayrollDayStatus(out ManualPayrollDayStat, PlantValue);
                if (ManualPayrollDayStat.Tables[0].Rows.Count > 0)
                {

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = "";
                    if (empMaster == "")
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "'";
                    }
                    else
                    {
                        sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "' and RowId in (" + empList + ")";
                    }

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                    for (int i = 0; i < ManualPayrollDayStat.Tables[0].Rows.Count; i++)
                    {
                        // Localizing Diff Flags on the Basis of Processed FinalDayStatus 

                        var WkDate = ManualPayrollDayStat.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string RowId = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"RowId"]).ToString();
                        string OtApplicable = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"OTApplicable"]).ToString();
                        string Goodwork = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"GoodWorkApplicable"]).ToString();
                        string AutoLock = clsWebLib.GetBoolData(ManualPayrollDayStat.Tables[0].Rows[i][@"AutoLock"]).ToString();

                        #region For Using them to get the Summary
                        string TotalPresent = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"PresentValue"]).ToString();
                        string TotalLate = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"LateValue"]).ToString();
                        string TotalAbsent = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"AbsentValue"]).ToString();
                        string TotalLv = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"LvValue"]).ToString();
                        string TotalCompAssignLv = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"CompAssignLvValue"]).ToString();
                        string TotalWeekOff = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"WeekOffValue"]).ToString();
                        string TotalHoliDay = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"HoliDayValue"]).ToString();
                        string TotalWeekOffHoliDay = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"WeekOffHoliDayValue"]).ToString();
                        string TotalPayDay = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"TotalPayDay"]).ToString();
                        string TotalNonPayDay = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"TotalNonPayDay"]).ToString();
                        string TotalWorkingDay = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"WorkingDay"]).ToString();
                        string ActualWorkingDay = clsWebLib.RetValidLen(ManualPayrollDayStat.Tables[0].Rows[i][@"ActualWorkingDay"]).ToString();

                        #endregion



                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            // Updations in APD Table 
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            #region Null Flagging

                            dr["PresentValue"] = DBNull.Value;
                            dr["LateValue"] = DBNull.Value;
                            dr["AbsentValue"] = DBNull.Value;
                            dr["LvValue"] = DBNull.Value;
                            dr["CompAssignLvValue"] = DBNull.Value;
                            dr["WeekOffValue"] = DBNull.Value;
                            dr["HoliDayValue"] = DBNull.Value;
                            dr["WeekOffHoliDayValue"] = DBNull.Value;
                            dr["PayDayValue"] = DBNull.Value;
                            dr["NonPayDayValue"] = DBNull.Value;
                            dr["WorkingDayValue"] = DBNull.Value;
                            dr["ActualWorkingDayValue"] = DBNull.Value;

                            #endregion

                            dr["DayTypeOTApplicable"] = OtApplicable;
                            dr["DayTypeGoodWorkApplicable"] = Goodwork;
                            if (AutoLock == "True")
                            {
                                // Individual Lock
                                dr["IsLock"] = true;
                                dr["LockedDate"] = DateTime.Now;
                                dr["LockedBy"] = "AutoLock";
                            }

                            dr["PresentValue"] = TotalPresent;
                            dr["LateValue"] = TotalLate;
                            dr["AbsentValue"] = TotalAbsent;
                            dr["LvValue"] = TotalLv;
                            dr["CompAssignLvValue"] = TotalCompAssignLv;
                            dr["WeekOffValue"] = TotalWeekOff;
                            dr["HoliDayValue"] = TotalHoliDay;
                            dr["WeekOffHoliDayValue"] = TotalWeekOffHoliDay;
                            dr["PayDayValue"] = TotalPayDay;
                            dr["NonPayDayValue"] = TotalNonPayDay;
                            dr["WorkingDayValue"] = TotalWorkingDay;
                            dr["ActualWorkingDayValue"] = ActualWorkingDay;

                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                            CheckerFunction(ref ManualFlagRowId, RowId);
                        }
                    }
                    SaveDataSets(dsRef);

                }
                #endregion

                SaveLog("Manual Payroll DayStatus Logic Ran Successfully ...", PlantValue, false);

                #region Manual OverStay UnderStay 
                DataSet ManualOverUnderStay;
                ManualOverUnderStayData(out ManualOverUnderStay, PlantValue);
                if (ManualOverUnderStay.Tables[0].Rows.Count > 0)
                {
                    // OverStay underStay DataSet Generation using (Duration - ShiftHoursWithoutOT)
                    var sqlx = "";
                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    if (empMaster == "")
                    {
                        sqlx = @"select * from AttdnProcessData where ManualFlag=1 and Duration >0 and PlantID='" + PlantValue + "'";
                    }
                    else
                    {
                        sqlx = @"select * from AttdnProcessData where ManualFlag=1 and Duration >0 and PlantID='" + PlantValue + "' and RowId in (" + empList + ")";
                    }


                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    for (int i = 0; i < ManualOverUnderStay.Tables[0].Rows.Count; i++)
                    {
                        string WorkDate = ManualOverUnderStay.Tables[0].Rows[i][@"WorkDate"].ToString();
                     
                        string RowId = ManualOverUnderStay.Tables[0].Rows[i][@"RowId"].ToString();
                        double OverUnderStay = Convert.ToDouble(clsWebLib.RetValidLen(ManualOverUnderStay.Tables[0].Rows[i][@"OverUnderStay"]).ToString());

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {

                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            if (OverUnderStay > 0)
                            {
                                // Extra Work
                                dr["OverStay"] = OverUnderStay;
                                dr["UnderStay"] = 0;
                            }
                            else if (OverUnderStay == 0)
                            {
                                dr["OverStay"] = 0;
                                dr["UnderStay"] = 0;
                            }
                            else
                            {

                                // Less Work
                                dr["OverStay"] = 0;
                                dr["UnderStay"] = OverUnderStay;
                            }

                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                            CheckerFunction(ref ManualFlagRowId, RowId);
                        }
                    }
                    SaveDataSets(dsRef);

                }

                #endregion

                SaveLog("Manual OverStay Logic Ran Successfully ...", PlantValue, false);

                #region Manual OT Calculation 
                DataSet ProcessOTCalculate;
                ProcessedOTCalculation(out ProcessOTCalculate, PlantValue);
                if (ProcessOTCalculate.Tables[0].Rows.Count > 0)
                {
                    // OverTime DataSet Using OT Per Minute Policy
                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = "";
                    if(empMaster == "")
                    {
                        sqlx = @"select * from AttdnProcessData where ManualFlag=1 and IsOTEntitled='1' and PlantID='" + PlantValue + "'";
                    }
                    else
                    {
                        sqlx = @"select * from AttdnProcessData where ManualFlag=1 and IsOTEntitled='1' and PlantID='" + PlantValue + "' and RowId in ("+ empList + ")";
                    }

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    // Settings of Modes from PlantWiseHRMSSetting
                    var sqly = @"select * from PlantWiseHRMSSetting where PlantID='" + PlantValue + "'";
                    objCon.OpenDataSetThroughAdapter(sqly, out DataSet OTMode, false, false, "", "1");

                    string OTModeValue = clsWebLib.RetValidLen(OTMode.Tables[0].Rows[0][@"ResultendOT"]).ToString();
                    // 0 means Punched Based 1 means Manual 2 means Mixed

                    for (int i = 0; i < ProcessOTCalculate.Tables[0].Rows.Count; i++)
                    {
                        var WkDate = ProcessOTCalculate.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string RowId = clsWebLib.RetValidLen(ProcessOTCalculate.Tables[0].Rows[i][@"RowId"]).ToString();
                        string Result = clsWebLib.RetValidLen(ProcessOTCalculate.Tables[0].Rows[i][@"Result"]).ToString();

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            string PastManualOT = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"ManualOt"]).ToString();
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;

                            if (OTModeValue == "0")
                            {
                                // Punched Based
                                if (Result != "")
                                {
                                    if (Convert.ToDouble(Result) > 0)
                                    {
                                        dr.BeginEdit();
                                        dr["ProcessedOT"] = Result;
                                        dr["CalculatedOT"] = Result;  // For Visiblity
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                        CheckerFunction(ref ManualFlagRowId, RowId);
                                    }
                                }

                            }
                            else if (OTModeValue == "1")
                            {
                                // Manual Mode
                                if (Result != "")
                                {
                                    if (PastManualOT != "")
                                    {
                                        double SmallerValue = Math.Min(Convert.ToDouble(PastManualOT), Convert.ToDouble(Result));
                                        dr.BeginEdit();
                                        dr["ProcessedOT"] = SmallerValue;
                                        dr["CalculatedOT"] = Result;  // For Visiblity
                                        dr.EndEdit();
                                        CheckerFunction(ref ManualFlagRowId, RowId);
                                                                    
                                    }
                                }
                            }

                            else
                            {
                                // Mixed Mode
                                if (Result != "")
                                {
                                    if (PastManualOT != "")
                                    {
                                        if (Convert.ToDouble(PastManualOT) >= 0)
                                        {
                                            // If Manual is less than Processed
                                            if (Convert.ToDouble(PastManualOT) < Convert.ToDouble(Result))
                                            {
                                                dr.BeginEdit();
                                                dr["ProcessedOT"] = PastManualOT;
                                                dr["CalculatedOT"] = Result;  // For Visiblity
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                CheckerFunction(ref ManualFlagRowId, RowId);
                                            }
                                            else
                                            {
                                                // Otherwise Processed
                                                dr.BeginEdit();
                                                dr["ProcessedOT"] = Result;
                                                dr["CalculatedOT"] = Result;  // For Visiblity
                                                dr.EndEdit();
                                            }
                                        }
                                        else
                                        {
                                            // Otherwise Processed
                                            dr.BeginEdit();
                                            dr["ProcessedOT"] = Result;
                                            dr["CalculatedOT"] = Result;
                                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                            dr.EndEdit();
                                            CheckerFunction(ref ManualFlagRowId, RowId);
                                        }

                                    }
                                    else
                                    {
                                        // Otherwise Processed
                                        dr.BeginEdit();
                                        dr["ProcessedOT"] = Result;
                                        dr["CalculatedOT"] = Result;
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                        CheckerFunction(ref ManualFlagRowId, RowId);
                                    }

                                }
                            }
                        }
                    }
                    SaveDataSets(dsRef);

                }
                #endregion

                SaveLog("Manual Processed OT Logic Ran Successfully ...", PlantValue, false);
                               
                #region Set Manual Flag ->0              
                ProcessManualFlag(ManualFlagRowId); // Set ManualFlag to 0
                #endregion

                SaveLog("Set Manual Flag to 0 Ran Successfully ...", PlantValue, false);

            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }
        #endregion

        #region Roster Process

        public void RosterProcess(string PlantId, string Date, string UserId = null)
        {

            ProcessLock _lock = new ProcessLock(UserId, ProcessLockId.RosterProcess, "", 60);
            _lock.LockProcess();
            try
            {
                DataSet PlantLock;
                PlantLockCheck(Date, out PlantLock, PlantId);
                if (PlantLock.Tables[0].Rows.Count > 0)
                {

                }
                else
                {                    

                    var sql2 = @"Select * from dbo.RosterPatternHeader where PlantId = '" + PlantId + "'";
                    DataTable RosterTable = new DataTable();
                    RosterTable = _sqlRepository.GetDataTable(sql2);

                    //Dictionary and DataSet Initialization
                    DataSet ds;
                    ConnectionManager.DAL.ConManager cona = new ConnectionManager.DAL.ConManager("1");
                    cona.OpenDataSetThroughAdapter("select * from RosterPatternProcess where 1 = 2", out ds, false, "1");

                    if (RosterTable.Rows.Count > 0)
                    {
                        //Loop to go through all the Rosters in a Plant
                        for (int j = 0; j < RosterTable.Rows.Count; j++)
                        {
                            DateTime ddt = Convert.ToDateTime(Date);
                            string DaysCol = "Days" + DateTime.DaysInMonth(ddt.Year, ddt.Month).ToString();

                            //Getting all the Shifts Child 
                            var sql3 = @"Select *, " + DaysCol + " as ShiftSequence from dbo.RosterPatternChild where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' order by Days31";
                            DataTable ShiftsTable = new DataTable();
                            ShiftsTable = _sqlRepository.GetDataTable(sql3);

                            //Getting the Max Sequence through the Use of Dynamic Months
                            var maxS = @"Select top 1 * from dbo.RosterPatternChild where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' order by " + DaysCol + @" desc";
                            DataTable MaxSTable = new DataTable();
                            MaxSTable = _sqlRepository.GetDataTable(maxS);


                            if (MaxSTable.Rows.Count == 0)
                            {
                                continue;
                            }
                            else
                            {
                                int maxSeq = int.Parse(MaxSTable.Rows[0][DaysCol].ToString());
                                string _Id = "";
                                //Get the top Nearest Effective Date
                                DateTime Today = ddt;
                                String noww = ddt.ToString("dd-MMM-yyyy");
                                var sql4 = @"Select top 1 ed.*, rp.PlantId from dbo.RosterEffectiveDate ed
                                                left join dbo.RosterPatternHeader rp on rp.Id = ed.RPHeaderId
                                                 where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' and EffectiveDate <= '" + noww + "' order by EffectiveDate desc";

                                DataTable EffectiveDateTable = new DataTable();
                                EffectiveDateTable = _sqlRepository.GetDataTable(sql4);

                                //Getting all the rows from the Process table
                                var sql5 = @"Select * from dbo.RosterPatternProcess where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' and PlantId = '" + PlantId + "' and WorkDate='" + noww + "'";
                                DataTable ProcessTable = new DataTable();
                                ProcessTable = _sqlRepository.GetDataTable(sql5);
                                int counts = ProcessTable.Rows.Count;

                                if (counts == 0)
                                {
                                    

                                    Dictionary<string, object> dict = InitializeMyDictionary();

                                    // Conditions...
                                    int DateDifference = -1;
                                    if (EffectiveDateTable.Rows.Count > 0)
                                    {
                                        DateTime EffecDate = Convert.ToDateTime(EffectiveDateTable.Rows[0]["EffectiveDate"].ToString());
                                        DateDifference = (int)(Today - EffecDate).Days;
                                    }

                                    if (DateDifference == 0)// If today is an Effective Date
                                    {
                                        bplib.clsGenID genid = new bplib.clsGenID();
                                        genid.GenID("dbo.RosterPatternProcess", out _Id);
                                        dict["Id"] = "RP" + _Id;
                                        dict["RPHeaderId"] = RosterTable.Rows[j]["Id"].ToString();
                                        dict["PlantId"] = PlantId;
                                        dict["WorkDate"] = Convert.ToDateTime(Today);
                                        dict["ShiftDefinationID"] = ShiftsTable.Rows[0]["ShiftDefinitionID"].ToString();
                                        dict["ShiftSequence"] = ShiftsTable.Rows[0]["ShiftSequence"].ToString();
                                        Add(ds.Tables[0], dict);
                                    }
                                    else
                                    {
                                        //Check for the nearest Previous Date;
                                        if (EffectiveDateTable.Rows.Count > 0)
                                        {
                                            DateTime EffecDates = Convert.ToDateTime(EffectiveDateTable.Rows[0]["EffectiveDate"].ToString());
                                            double DayDiffs = (Today - EffecDates).Days;
                                            int Seq = (int)(DayDiffs % maxSeq); // The Sequence of Shift to be inserted Today

                                            bplib.clsGenID genid = new bplib.clsGenID();
                                            genid.GenID("dbo.RosterPatternProcess", out _Id);
                                            dict["Id"] = "RP" + _Id;
                                            dict["RPHeaderId"] = RosterTable.Rows[j]["Id"].ToString();
                                            dict["PlantId"] = PlantId;
                                            dict["WorkDate"] = Convert.ToDateTime(Today);
                                            dict["ShiftDefinationID"] = ShiftsTable.Rows[Seq]["ShiftDefinitionID"].ToString();
                                            dict["ShiftSequence"] = ShiftsTable.Rows[Seq]["ShiftSequence"].ToString();
                                            //We will make the Row and insert into the Table.
                                            Add(ds.Tables[0], dict);
                                        }
                                        else // In case there are no previous date Either, it will be an Exceptional Case.
                                        {
                                            continue;
                                        }

                                    }
                                    //
                                }

                            }

                        }
                    }
                    SaveDataSets(ds);
                }
                _lock.UnlockProcess();

            }
            catch (Exception e)
            {
                _lock.UnlockProcess();
                throw e;
            }
        }
        #endregion

        #region PastDOJ Source Data

        public void PastDOJ(out DataSet ds, string Plant,string WkDate,string EmpMaster)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                // DataSet For Row Creation of PAST DOJ Employees
                // It Will compare ShiftTime Change Master & Shift Defination

                string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                var sql = @"select TobeAdded=case When isnull(p.EmpSystemID,'') ='' then 'true' 
			    else 'false' end , e.SystemId,'"+WkDate+ @"' as WorkDate,Month('" + WkDate + @"') as Month,
				Year('" + WkDate + @"') as Year,
                convert(varchar(30),'" + newformat+ @"' )+convert(varchar(30), e.SystemId)RowId,e.PlantId,
				e.GroupID,
                mb.ShiftDefinationId as BudgetedShift,isnull(stcm.InTime,sdy.InTime) as BudgetShiftIn,
				ISNULL(stcm.OutTime,sdy.OutTime) as BudgetShiftOut,
                ISNULL(stcm.ShiftDuration,sdy.ShiftDuration) as ShiftDuration,
				mb.Id as BudgetId,Op.InPunchStartTime as PlantInPunchStartTime, 
                FullDayDuration=ISNULL(stcm.FullDayDuration,sdy.FullDayDuration),HalfDayDuration=
				isnull(stcm.HalfDayDuration,sdy.HalfDayDuration),
				ShortDuration=ISNULL(stcm.ShortDuration,sdy.ShortDuration),
				HoursWithoutOT=ISNULL(stcm.HoursWithoutOT,sdy.HoursWithoutOT),
                HolidayStatus=isnull((select om.OffDayType
                from SCS.OffDayMaster om left join scs.OffDayDetail od
                on om.Id=od.OffDayMasterId where od.OffDayDate='"+WkDate+@"'
                and om.PlantId='"+Plant+ @"' and om.OffDayType='H'),'false'),
                WeekOfftype=isnull((SELECT WOHeaderId FROM EmployeeWeeklyOff ex
				left join EmployeeInformation emp on emp.SystemId=ex.EmpSystemId
				where  
				emp.DOJ <= '"+WkDate+@"' AND (emp.DOS >= '"+WkDate+ @"' OR 
				ISNULL(emp.DOS,'') = '' 
				OR emp.DOS = '01/01/1901') and emp.SystemId=e.SystemId),'CompanyWeekOff'),
				WeeklyStatus=isnull((select od.OffDayType
				from scs.OffDayMaster od 
				left join scs.OffDayDetail odd on odd.OffDayMasterId=od.Id
				where od.OffDayType='W' 
				and od.PlantId='" + Plant+"' and odd.OffDayDate='"+WkDate+ @"'),'NW'),
                dh.Id as HeaderId,dxc.LeavePolicyMasterId
                from EmployeeInformation e 
                left join mst.DesignationMasterLegalDesignation ddm on ddm.LegalDesignationId = 
		        e.LegalDesignationId
				left join mst.DesignationMaster 
				dm on dm.Id = ddm.DesignationMasterId
				left join scs.DesignationMasterConfiguration dxc on dxc.DesignationMasterId=dm.Id
				and dxc.PlantId=e.PlantId
				left join DayStatusPlantChild 
				dc on dc.EmpTypeId=dm.EmployeeCategoryId
				and dc.PlantId=e.PlantId
				left join DayStatusHeader dh on dh.Id=dc.headerId                
                left join mst.ManpowerBudget mb on mb.Id=e.BudgetCode
                left join ShiftDefination sdy on sdy.SystemID=mb.ShiftDefinationId				  
				LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '" + WkDate+@"' 
							BETWEEN stcm.FromDate AND stcm.ToDate AND 
							sdy.SystemID=stcm.ShiftDefinationID                            
                left join org.Plant pl on pl.Id=e.PlantId
                left join OutPunchConfigurationHeader Op on OP.PlantId=pl.Id
				left join AttdnProcessData p on p.EmpSystemID=e.SystemId 
				and p.WorkDate='"+WkDate+@"'              
                where e.EmpType!='Guest' and e.PlantId='"+Plant+@"' and e.SystemID In("+EmpMaster+")" +
                "and DOJ <= '"+WkDate+@"' AND (E.DOS >= '"+WkDate+@"' OR ISNULL(E.DOS,'') = '' 
				OR E.DOS = '01/01/1901') ";

                // Finds HolidayStatus,BudgetCode as well as Weekly Status if Company WeekOff
                // HeaderId and Month,Year as well
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            { 
                throw (ex);
            }
        }
        public void MissingRowsDOJ(out DataSet ds, string Plant,string Date)
        {
            // This DataSet to find all the Entries that are done Today 
            // Whose DOJ is less than Today
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"SELECT e.SystemId as EmpId,FORMAT(e.DOJ,'yyyy-MM-dd') as DOJ,e.GroupID, e.PlantId,
                     FORMAT(e.DateAdded,'yyyy-MM-dd') as ToDate,
				     e.DateAdded as EntryTime
                     FROM EmployeeInformation E
                     WHERE CONVERT(DATE,'"+Date+@"'
                     )=CONVERT(date,E.DateAdded) 
				     and DOJ<= CONVERT(DATE,'"+Date+"') and e.PlantId='"+Plant+@"'
					 order by doj asc";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void IndividualWeekOffDataSet(out DataSet ds, string FromDate, string ToDate, string EmpMaster)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                // It finds all the Weekoff Values of Range of Dates from DOJ To Today's Date 
                // That have Week Off other than Company Week Off ....
                var sql = @"select dd.* from (Select jj.* ,  (Select wcc.DayType from
                                                 
												    dbo.WeekOffChild wcc where wcc.WOSequence =jj.Seq 
                                                    and wcc.WOHeaderId = jj.WeekOffHeaderId) 
                                                    as DayType , ap.RowId , (Case when 
													ap.RowId = jj.MyRowId then 1 else 0 end) as Checks
                                        from
                                                    (Select ap.WorkDate, ap.EmpSystemID, format(ap.WorkDate,'yyyyMMdd')+ap.EmpSystemID as MyRowId,
                                                    (Select distinct
                                                    (DATEDIFF(DAY, (Select top 1 ed.EffectiveDate from
                                                     dbo.WeekOffHeader h 
                                                    left join dbo.WeekOffEffectiveDate ed on ed.WOHeaderId = h.Id
                                                    where ed.EffectiveDate <= ap.WorkDate and ed.WOHeaderId =  
                                        (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                                                    where EmpSystemId = e.SystemId and ex.EffectiveDate<=ap.WorkDate
                                                    order by ex.EffectiveDate desc)
                                                    order by ed.EffectiveDate desc) , ap.WorkDate) % 
                                                    (Select max(WOSequence) from WeekOffHeader h 
                                                    left join WeekOffChild wc on wc.WOHeaderId=h.Id 
                                                    where h.Id =  
                                        (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                                                    where EmpSystemId = e.SystemId and ex.EffectiveDate<=ap.WorkDate
                                                    order by ex.EffectiveDate desc)
                                        )
                                        )+1 as DayDiff
                                                    from 
                                                    EmployeeInformation e
                                                    left join EmployeeWeeklyOff ex on e.SystemId=ex.EmpSystemId
                                                    where e.PlantId=ap.PlantID and e.SystemId = ap.EmpSystemID) as Seq,

                                                    (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                                                    where EmpSystemId = ap.EmpSystemID and ex.EffectiveDate<=ap.WorkDate
                                                    order by ex.EffectiveDate desc) WeekOffHeaderId 
                                        from AttdnProcessData ap 

                                        where ap.EmpSystemID In("+EmpMaster+@") and WorkDate 
										between '"+FromDate+@"' and '"+ToDate+@"'
                                        )as jj
                                        left join AttdnProcessData ap on
										ap.WorkDate = jj.WorkDate and 
										ap.EmpSystemID In("+EmpMaster+@") and ap.WorkDate 
										between '"+FromDate+@"' and '"+ToDate+@"'
										)as dd where dd.Checks=1 and isnull(dd.DayType,'')!=''";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void OTEligibleEmpDOJ(string FromDate,string ToDate, out DataSet ds, string PlantId, string empMaster)
        {
            string EmpData = clsWebLib.RetValidLen(empMaster).ToString();
            string strkey = "1=1";
            if (EmpData != "")
            {
                strkey = "e.SystemId in(" + empMaster + @")";
            }

            ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sql = @"select distinct e.SystemId as EmpId,dc.IsOTEntitled,
				Format(p.WorkDate,'yyyy-MMM-dd')WorkDate,(Format(p.WorkDate,'yyyyMMdd')+e.SystemId)
				as RowId
                from AttdnProcessData p join
                EmployeeInformation e on e.SystemId=p.EmpSystemID    
				left join mst.DesignationMasterLegalDesignation ddm on 
                ddm.LegalDesignationId = e.LegalDesignationId
                left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
				left join scs.DesignationMasterConfiguration dc on dc.DesignationMasterId=dm.Id
                and dc.PlantId=e.PlantId
                where p.WorkDate between '"+FromDate+@"' and '"+ToDate+@"' and
				e.PlantId='" + PlantId + @"' 
                and E.DOJ <= '" +ToDate  + @"' 
				AND (E.DOS >= '" + ToDate + @"' OR ISNULL(E.DOS,'') = '' 
				OR E.DOS = '01/01/1901')and dc.IsOTEntitled=1 and " + strkey + @"
				and e.SystemId not in (select final.EmpSystemId from 
				(select distinct o.empsystemId,
				(select top 1 Exclude from NonEligibleOT m where 
				m.EmpSystemId=o.EmpSystemId order by EffectiveDate desc)as x 
				from NonEligibleOT o) final where final.x=1)";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void OTWeekDOJ(string FromDate, string ToDate, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct Format(WorkDate,'dd-MMM-yyyy')WorkDate,
				OTWeek from AttdnProcessData where PlantID='"+PlantId+@"'
				and WorkDate between '"+FromDate+"' and '"+ToDate+@"'
				and OTWeek is not null";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        #endregion

        #region PastDOJ Row Creation Process
        public void PastDOJProcess(string Date, string PlantValue,string UserId=null)
        {

            ProcessLock _lock = new ProcessLock(UserId, ProcessLockId.DOJProcess, "", 60);
            _lock.LockProcess();
            try
            {

                DataSet PlantLock;
                PlantLockCheck(Date, out PlantLock, PlantValue);
                if (PlantLock.Tables[0].Rows.Count > 0)
                {
                    return;
                }
                else
                {             

                    string EmpMaster = "''",CreatedEmpIds="''";
                 
                    #region Previous DOJ Row Creation Logic
                    DataSet MissingDOJ,IndividualWeekOfDOJ;
                    MissingRowsDOJ(out MissingDOJ, PlantValue,Date);// Finds All Entries in Today's Date with Past DOJ
                    if (MissingDOJ.Tables[0].Rows.Count > 0)
                    {
                        // Dataset Generated for New Entries Having Past DOJ
                        string StartDate = clsWebLib.RetValidLen(MissingDOJ.Tables[0].Rows[0][@"DOJ"]).ToString();
                        string ToDate = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");

                        for (int i = 0; i < MissingDOJ.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = MissingDOJ.Tables[0].Rows[i][@"EmpId"].ToString();
                            CheckerFunction(ref EmpMaster, EmpId); // loop in and Adding distinct Employees
                        }


                        if (StartDate != "")
                        {
                            #region RowCreation Logic
                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            objCon.OpenDataSetThroughAdapter("select * from AttdnProcessData where WorkDate between '" + StartDate + "' and '" + ToDate + "' and PlantID = '" + PlantValue + "' and EmpSystemID in (" + EmpMaster + ")", out DataSet dsRef, false, false, "", "1");

                            DateTime frmdate = Convert.ToDateTime(StartDate);
                            DateTime Todate = Convert.ToDateTime(ToDate);
                            int days = 0;

                            while (frmdate.AddDays(days) <= Todate)
                            {
                                string CurrentDate = Convert.ToString(Convert.ToDateTime(frmdate).AddDays(days));
                                DataSet RowCreationData; // Iterate b/w DOJ and Today's Date
                                PastDOJ(out RowCreationData, PlantValue, CurrentDate, EmpMaster);
                                if (RowCreationData.Tables[0].Rows.Count > 0)
                                {
                                    string EmpWkDate = RowCreationData.Tables[0].Rows[0][@"WorkDate"].ToString();

                                    for (int i = 0; i < RowCreationData.Tables[0].Rows.Count; i++)
                                    {
                                        string EmpId = RowCreationData.Tables[0].Rows[i][@"SystemId"].ToString();
                                        var GpId = RowCreationData.Tables[0].Rows[0][@"GroupID"].ToString();
                                        string PlantId = RowCreationData.Tables[0].Rows[i][@"PlantId"].ToString();
                                        string RowId = RowCreationData.Tables[0].Rows[i][@"RowId"].ToString();
                                        string HoliDay = RowCreationData.Tables[0].Rows[i][@"HolidayStatus"].ToString();
                                        string WeekOfftype = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"WeekOfftype"]).ToString();
                                        string WeeklyStatus = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"WeeklyStatus"]).ToString();

                                        // Set Budgeted Shift as Default Shift  
                                        string BudgetShift = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"BudgetedShift"]).ToString();
                                        string BudgetShiftDurn = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"ShiftDuration"]).ToString();
                                        string BudgetShiftIn = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"BudgetShiftIn"]).ToString();
                                        string BudgetShiftOut = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"BudgetShiftOut"]).ToString();
                                        ShiftTime(ref BudgetShiftIn, ref BudgetShiftOut, EmpWkDate);

                                        var BudgetId = RowCreationData.Tables[0].Rows[i][@"BudgetId"].ToString();
                                        var FullDayDuration = RowCreationData.Tables[0].Rows[i][@"FullDayDuration"].ToString();
                                        var HalfDayDuration = RowCreationData.Tables[0].Rows[i][@"HalfDayDuration"].ToString();
                                        var ShortDuration = RowCreationData.Tables[0].Rows[i][@"ShortDuration"].ToString();
                                        var HoursWithoutOT = RowCreationData.Tables[0].Rows[i][@"HoursWithoutOT"].ToString();

                                        string HeaderId = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"HeaderId"]).ToString();
                                        string LeavePolicyId = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"LeavePolicyMasterId"]).ToString();
                                        var Month = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"Month"]).ToString();
                                        var Year = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"Year"]).ToString();

                                        var PlantInPunchStartTime = RowCreationData.Tables[0].Rows[i][@"PlantInPunchStartTime"].ToString();
                                        PlantInTime(ref PlantInPunchStartTime, EmpWkDate);

                                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";

                                        if (dsRef.Tables[0].DefaultView.Count == 0 && Convert.ToBoolean(RowCreationData.Tables[0].Rows[i]["TobeAdded"].ToString()) == true)
                                        {
                                            DataRow dr = dsRef.Tables[0].NewRow();
                                            dr["EmpSystemID"] = EmpId;
                                            dr["RowId"] = RowId;
                                            dr["WorkDate"] = EmpWkDate; // Localizing Default Values
                                            dr["GroupID"] = GpId;
                                            dr["PlantID"] = PlantId;
                                            dr["OTMonth"] = Month;
                                            dr["OTYear"] = Year;

                                            dr["BudgetId"] = clsWebLib.RetValidLen(BudgetId);
                                            dr["PlantInPunchStartTime"] = clsWebLib.RetValidLen(PlantInPunchStartTime);
                                            dr["ManualFlag"] = true;

                                            if (BudgetShift.ToString() != "")
                                            {
                                                // Assigned Shift
                                                dr["ShiftSystemID"] = BudgetShift;
                                                dr["ShiftInTime"] = BudgetShiftIn;
                                                dr["ShiftOutTime"] = BudgetShiftOut;
                                                dr["BudgetedShiftID"] = BudgetShift;

                                                // Duration Columns
                                                dr["ShiftDuration"] = BudgetShiftDurn;
                                                dr["ShiftHalfDayDuration"] = clsWebLib.RetValidLen(HalfDayDuration);
                                                dr["ShiftShortDuration"] = clsWebLib.RetValidLen(ShortDuration);
                                                dr["ShiftFullDayDuration"] = clsWebLib.RetValidLen(FullDayDuration);
                                                dr["ShiftHoursWithoutOT"] = clsWebLib.RetValidLen(HoursWithoutOT);
                                            }

                                            #region  Not Nullable Columns default values

                                            dr["WrongShift"] = 0;
                                            dr["OTHr"] = "0";
                                            dr["ProcessedOT"] = "0";
                                            dr["CalculatedOT"] = 0;
                                            dr["IsOTComfirm"] = 0;
                                            dr["IsLock"] = 0;
                                            dr["IsOTEntitled"] = 0;
                                            dr["IsLWP"] = 0;
                                            dr["IsOD"] = 0;
                                            dr["IsHalfDayLeave"] = 0;
                                            dr["OTIntime"] = "0";
                                            dr["OTOuttime"] = "0";
                                            dr["LeaveDuration"] = "0";
                                            dr["ToReprocess"] = "No";
                                            dr["AddedBy"] = "DOJProcess";
                                            dr["DateAdded"] = Convert.ToDateTime(DateTime.Now);

                                            #endregion

                                            #region HeaderId Localized
                                            dr["DayStatusHeaderId"] = HeaderId;
                                            if (LeavePolicyId != "")
                                            {
                                                dr["LeavePolicyMasterId"] = LeavePolicyId;
                                            }
                                            #endregion

                                            if (HoliDay != "false")
                                            {
                                                dr["HolidayStatus"] = "H";
                                            }
                                            if (WeekOfftype == "CompanyWeekOff")
                                            {
                                                // Setting WeekOff Using Company WeekOff Setting
                                                dr["WeeklyStatus"] = WeeklyStatus;
                                            }

                                            dsRef.Tables[0].Rows.Add(dr);

                                            CheckerFunction(ref CreatedEmpIds, RowId); // loop in and Adding distinct RowIds
                                        }

                                    }
                                }

                                days += 1; // Increment Day Counter
                            }

                            SaveDataSets(dsRef); // Rows Saved

                            #endregion

                            #region Individual WeekOff Setting
                            // New Entry of Employees and Fetching from Range
                            IndividualWeekOffDataSet(out IndividualWeekOfDOJ, StartDate, ToDate, EmpMaster);
                            if (IndividualWeekOfDOJ.Tables[0].Rows.Count > 0)
                            {
                                ConnectionManager.DAL.ConManager conx = new ConnectionManager.DAL.ConManager("1");
                                conx.OpenDataSetThroughAdapter("select * from AttdnProcessData where RowId in (" + CreatedEmpIds + ")", out DataSet dsMaster, false, false, "", "1");

                                for (int i = 0; i < IndividualWeekOfDOJ.Tables[0].Rows.Count; i++)
                                {
                                    string RowId = IndividualWeekOfDOJ.Tables[0].Rows[i][@"RowId"].ToString();
                                    string DayType = clsWebLib.RetValidLen(IndividualWeekOfDOJ.Tables[0].Rows[i][@"DayType"]).ToString();

                                    dsMaster.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";

                                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                                    {
                                        // Calculated DayType and Setting Their Weekoffs in APD
                                        DataRow drx = dsMaster.Tables[0].DefaultView[0].Row;
                                        drx.BeginEdit();
                                        drx["WeeklyStatus"] = DayType;
                                        drx["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        drx["UpdatedBy"] = "DOJProcess";
                                        drx.EndEdit();
                                    }
                                }
                                SaveDataSets(dsMaster);
                            }

                            #endregion

                            #region OTEligibleData Flagging
                            DataSet OTElgbEmp;
                            OTEligibleEmpDOJ(StartDate, ToDate, out OTElgbEmp, PlantValue, EmpMaster); // OT Eligible DataSet Generation
                            if (OTElgbEmp.Tables[0].Rows.Count > 0)
                            {
                                // Chosen From & ToDate to get RowIds 
                                ConnectionManager.DAL.ConManager newcon = new ConnectionManager.DAL.ConManager("1");
                                newcon.OpenDataSetThroughAdapter("select * from AttdnProcessData where RowId in (" + CreatedEmpIds + ")", out DataSet dsOt, false, false, "", "1");

                                for (int i = 0; i < OTElgbEmp.Tables[0].Rows.Count; i++)
                                {
                                    string RowId = OTElgbEmp.Tables[0].Rows[i][@"RowId"].ToString();
                                    string IsOTEntitled = OTElgbEmp.Tables[0].Rows[i][@"IsOTEntitled"].ToString();

                                    // Only RowIds that exist will come
                                    dsOt.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                                    if (dsOt.Tables[0].DefaultView.Count > 0)
                                    {
                                        // Updation in APD Table for OT Entitled Employees
                                        DataRow dr = dsOt.Tables[0].DefaultView[0].Row;
                                        dr.BeginEdit();

                                        dr["IsOTEntitled"] = clsWebLib.GetBoolData(IsOTEntitled);
                                        dr["UpdatedBy"] = "Schedule";
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                    }
                                }
                                SaveDataSets(dsOt);
                            }
                            #endregion

                            #region OTWeek Localization
                            if (StartDate != "")
                            {
                                string strSql = string.Empty;
                                DataSet dsWeekData;                               
                                OTWeekDOJ(StartDate, ToDate, out dsWeekData, PlantValue);
                                if(dsWeekData.Tables[0].Rows.Count>0)
                                {
                                    for (int i = 0; i < dsWeekData.Tables[0].Rows.Count; i++)
                                    {
                                        string Datex = clsWebLib.RetValidLen(dsWeekData.Tables[0].Rows[i]["WorkDate"]).ToString();
                                        string Week = clsWebLib.RetValidLen(dsWeekData.Tables[0].Rows[i]["OTWeek"]).ToString();

                                        if (Datex != "" && Week !="")
                                        {
                                            if (strSql.Length == 0)
                                            {
                                                strSql = @" update AttdnProcessData set OTWeek='"+Week+"' where WorkDate='"+Datex+"' and " +
                                                    "PlantId='"+PlantValue+"' and RowId in (" + CreatedEmpIds + ") ;";
                                            }
                                            else
                                            {
                                                strSql += Environment.NewLine + @" update AttdnProcessData set OTWeek='" + Week + "' where WorkDate='" + Datex + "' and " +
                                                    "PlantId='" + PlantValue + "' and RowId in (" + CreatedEmpIds + ") ;";
                                            }
                                        }
                                    }
                                    if (strSql.Length > 0)
                                    {
                                        UpdateStatus(strSql); // OTWeek Updation
                                    }
                                }
                            }
                            #endregion

                        }


                    }
                    #endregion
                }
                _lock.UnlockProcess();
            }
            catch(Exception ex)
            {
                _lock.UnlockProcess();
                throw ex;
            }
        }

        #endregion

        #region TBS LA Process

        #region LA
        
        private void EmployeeAutoStatusChange_LA(string plantid, string Date)
        {
            try
            {
                DataSet HRSettingLA = null;
                DataSet LAEmployees = null;

                GetHRSettingForAutoLA(plantid, out HRSettingLA);
            
                if (HRSettingLA.Tables[0].Rows.Count > 0)//LA
                {
                    string maxDays = GetNumData(HRSettingLA.Tables[0].Rows[0]["LongTermAbesnteeism"].ToString());
                    if (Convert.ToInt32(maxDays) > 0)
                    {
                        Get_tobe_LA(plantid, Date, maxDays, out LAEmployees);
                     
                        if (LAEmployees.Tables[0].Rows.Count > 0)
                        {
                            UpdateEmpStatusLA(plantid, LAEmployees); //update these emps as Long Absentism
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void UpdateEmpStatusLA(string PlantId, DataSet dsLA)
        {
            string strSql = string.Empty;
            try
            {

                for (int i = 0; i < dsLA.Tables[0].Rows.Count; i++)
                {
                    string _empid = dsLA.Tables[0].Rows[i]["Id"].ToString();

                    string v = clsWebLib.RetValidLen(dsLA.Tables[0].Rows[i]["FirstAbsentDate"]).ToString();
                    if (v != "")
                    {

                       string adate = Convert.ToDateTime(v).ToString("dd-MMM-yyyy");

                        if (strSql.Length == 0)
                        {
                            strSql = @"update EmployeeInformation set EmployeeCurrentStatus='LONG ABSENTEEISM',EmployeeCurrentStatusEffectiveDate='" + adate + "' where plantid='" + PlantId + "'  and systemid ='" + _empid + "';";
                        }
                        else
                        {
                            strSql += Environment.NewLine + @"update EmployeeInformation set EmployeeCurrentStatus='LONG ABSENTEEISM',EmployeeCurrentStatusEffectiveDate='" + adate + "' where plantid='" + PlantId + "'  and systemid ='" + _empid + "';";
                        }
                    }


                }
                if (strSql.Length > 0)
                {
                    UpdateStatus(strSql);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }          
        }
        private void Get_tobe_LA(string PlantId, string adate, string maxDays, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string strSql = @"SELECT 0 AS Active, e.SystemId AS Id, e.EmployeeCode,E.EmployeeName,
                D.DayStatus,COUNT(d.AbsentValue) AS AbsentCount,ab.AbsentDays,
                Format(ab.FirstAbsentDate,'dd-MMM-yyyy')FirstAbsentDate
                                FROM (
		                                SELECT p.EmpSystemID, p.WorkDate,
										p.AbsentValue,p.daystatus,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER 
										BY P.WorkDate DESC) AS SEQ
		                                FROM AttdnProcessData AS P	                               
                                        join EmployeeInformation x on p.EmpSystemID=x.SystemId
		                                WHERE x.PlantID='"+PlantId+@"' and 										
										 p.DayStatus NOT IN 
										 (select distinct DayType from DayType where Category in 
										('Holiday','Weekend')) 
										
	                                ) AS D

                                INNER JOIN (select * from EmployeeInformation) AS E ON e.SystemId=d.EmpSystemID 
                                LEFT OUTER JOIN (select K.EmpSystemID,COUNT(*)AbsentDays,MIN(k.WorkDate) AS FirstAbsentDate
                                  from (SELECT *,RANK() OVER(PARTITION BY EmpSystemID,dayStatustemp ORDER BY EmpSystemID,seq) 
								  AS SQ FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,
										
										CASE WHEN daystatus IN
										(select distinct DayType from DayType where Category in
										('Holiday','Weekend')) THEN 'A' 
										ELSE daystatus END AS dayStatustemp,
		                                
										dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM (select * from AttdnProcessData where WorkDate<= '" + adate+@"' 
										and PlantID='"+PlantId+@"')  AS P 
		                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=p.EmpSystemID
                                     
									 where p.DayStatus NOT IN (select distinct DayType from DayType where 
										Category in ('Holiday','Weekend')) 
										AND ei.EmployeeStatus='Active' AND isnull(ei.EmployeeCurrentStatus,'')=''
                                ) AS K WHERE K.dayStatustemp='A') AS K 
                                WHERE K.SEQ=K.SQ
                                GROUP BY K.EmpSystemID
                                HAVING COUNT(*)>='"+maxDays+@"') AS AB ON ab.EmpSystemID=E.SystemId


                                WHERE  e.EmployeeStatus='Active' AND isnull(e.EmployeeCurrentStatus,'')='' 
								AND D.SEQ<='"+maxDays+@"' AND D.AbsentValue='1'
								AND E.PlantId='"+PlantId+@"'
                                GROUP BY e.SystemId,ab.AbsentDays, e.EmployeeCode,E.EmployeeName,
								D.AbsentValue,d.DayStatus,
                                ab.FirstAbsentDate
                                HAVING COUNT(d.AbsentValue)>='"+maxDays+"' ORDER BY AB.AbsentDays DESC";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
        private void GetHRSettingForAutoLA(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var strSql = @"select LongTermAbesnteeism from PlantWiseHRMSSetting where PlantId='" + PlantId + "' and IsLongAbsenteeismAuto=1";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }           
        }

        #endregion

        #region TBS

        private void EmployeeAutoStatusChange_TBS(string plantid, string adate)
        {
            try
            {
                DataSet HRSetting_TBS = null;
                DataSet TBSDataSet = null;

                GetHRSettingForAutoTBS(plantid, out HRSetting_TBS);

                if (HRSetting_TBS.Tables[0].Rows.Count > 0)
                {
                    string maxDays = GetNumData(HRSetting_TBS.Tables[0].Rows[0]["TBSDays"].ToString());
                    if (Convert.ToInt32(maxDays) > 0)
                    {
                        Get_tobe_TBS(plantid, adate, maxDays, out TBSDataSet);
                        if (TBSDataSet.Tables[0].Rows.Count > 0)
                        {
                            UpdateEmpStatusTBS(plantid,TBSDataSet); //update these Employees as TBS
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void GetHRSettingForAutoTBS(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"select TBSDays from PlantWiseHRMSSetting where PlantId='" + PlantId + "' and IsTBSAuto=1";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }          
        }
        private void UpdateEmpStatusTBS(string PlantId, DataSet dsLA)
        {
            string strSql = string.Empty;
            try
            {

                for (int i = 0; i < dsLA.Tables[0].Rows.Count; i++)
                {
                    string _empid = dsLA.Tables[0].Rows[i]["Id"].ToString();
                    string v = clsWebLib.RetValidLen(dsLA.Tables[0].Rows[i]["FirstAbsentDate"]).ToString();
                    if (v != "")
                    {
                        string adate = Convert.ToDateTime(v).ToString("dd-MMM-yyyy");

                        if (strSql.Length == 0)
                        {
                            strSql = @" update EmployeeInformation set EmployeeCurrentStatus='TBS',EmployeeCurrentStatusEffectiveDate='" + adate + "' where plantid='" + PlantId + "'  and systemid ='" + _empid + "';";
                        }
                        else
                        {
                            strSql += Environment.NewLine + @" update EmployeeInformation set EmployeeCurrentStatus='TBS',EmployeeCurrentStatusEffectiveDate='" + adate + "' where plantid='" + PlantId + "'  and systemid ='" + _empid + "';";
                        }
                    }
                }
                if (strSql.Length > 0)
                {
                    UpdateStatus(strSql);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
           
        }
        private void Get_tobe_TBS(string PlantId, string adate, string maxDays, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string strSql = @"SELECT 0 AS Active, e.SystemId AS Id, e.EmployeeCode,                               
                                D.DayStatus,ab.AbsentDays,Format(ab.FirstAbsentDate,'dd-MMM-yyyy')
								FirstAbsentDate
                                FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,p.AbsentValue,
		                                dense_rank() OVER 
										(PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM AttdnProcessData AS P
										left join EmployeeInformation x on x.SystemId=p.EmpSystemID
										where x.plantid='"+PlantId+@"' and
		                                 p.DayStatus NOT IN 
										(select distinct DayType from DayType 
										where Category in ('Holiday','Weekend')) 
										and p.WorkDate<='"+adate+@"'
	                                ) AS D
                                INNER JOIN (select * from EmployeeInformation) AS E ON e.SystemId=d.EmpSystemID 
                            
                                LEFT OUTER JOIN (select K.EmpSystemID,COUNT(*)AbsentDays,MIN(k.WorkDate) AS FirstAbsentDate
                                  from (SELECT *,RANK() OVER(PARTITION BY EmpSystemID,dayStatustemp ORDER BY EmpSystemID,seq) AS SQ FROM (
		                                SELECT p.EmpSystemID, p.WorkDate, p.DayStatus,CASE WHEN daystatus IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) THEN 'A' ELSE daystatus END AS dayStatustemp,
		                                dense_rank() OVER (PARTITION BY p.EmpSystemID ORDER BY P.WorkDate DESC) AS SEQ
		                                FROM (select * from AttdnProcessData where
										WorkDate<= '"+adate+"' and PlantID='"+PlantId+@"')  AS P 
		                                INNER JOIN EmployeeInformation AS ei ON ei.SystemId=p.EmpSystemID
                                        where p.DayStatus NOT IN (select distinct DayType from DayType where Category in ('Holiday','Weekend')) AND ei.EmployeeStatus='Active' AND (isnull(ei.EmployeeCurrentStatus,'')=''
                                or isnull(ei.EmployeeCurrentStatus,'')='LONG ABSENTEEISM') 
                                ) AS K WHERE K.dayStatustemp='A') AS K 
                                WHERE K.SEQ=K.SQ
                                GROUP BY K.EmpSystemID
                                HAVING COUNT(*)>='"+maxDays+@"') AS AB ON ab.EmpSystemID=E.SystemId


                                WHERE  e.EmployeeStatus='Active' AND 
								(isnull(e.EmployeeCurrentStatus,'')='' or
								isnull(e.EmployeeCurrentStatus,'')='LONG ABSENTEEISM') 
								AND D.SEQ<='"+maxDays+@"' AND D.AbsentValue='1'  AND E.PlantId='"+PlantId+@"'
                                GROUP BY e.SystemId,ab.AbsentDays, e.EmployeeCode,D.AbsentValue,d.DayStatus
                                ,ab.FirstAbsentDate
                                HAVING COUNT(d.AbsentValue)>='"+maxDays+"' ORDER BY AB.AbsentDays DESC";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        #region Reverse LA

        private void UpdateEmpStatus_Reverse(string PlantId, DataSet dsLA)
        {
            string _empids = string.Empty;
            try
            {

                for (int i = 0; i < dsLA.Tables[0].Rows.Count; i++)
                {
                    string _empid = dsLA.Tables[0].Rows[i]["systemid"].ToString();
                    if (_empids.Length == 0)
                    {
                        _empids = "'" + _empid + "'";
                    }
                    else
                    {
                        _empids += ",'" + _empid + "'";
                    }
                }

                if (_empids.Length == 0)
                {
                    _empids = " ";
                }
                else
                {
                    _empids = " and systemid in (" + _empids + ")";
                }
                string strSql = @"update EmployeeInformation set EmployeeCurrentStatus=null,EmployeeCurrentStatusEffectiveDate=null where plantid='" + PlantId + "' " + _empids + "";
                UpdateStatus(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }           
        } 
        private void EmployeeAutoStatusChange_LA_Reverse(string plantid, string Todate)
        {
            try
            {
                DataSet HRSetting = null;
                DataSet DsActive = null;

                GetHRSettingForAutoLA(plantid, out HRSetting);
             
                if (HRSetting.Tables[0].Rows.Count > 0)
                {
                    string maxDays = GetNumData(HRSetting.Tables[0].Rows[0]["LongTermAbesnteeism"].ToString());
                    if (Convert.ToInt32(maxDays) > 0)
                    {
                        string FromDate = Convert.ToDateTime(Todate).AddDays(-Convert.ToInt32(maxDays)).ToString("dd-MMM-yyyy");
                        Get_tobe_Active_from_LA(plantid, FromDate, Todate, out DsActive);
                        if (DsActive.Tables[0].Rows.Count > 0)
                        {
                            UpdateEmpStatus_Reverse(plantid, DsActive);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Get_tobe_Active_from_LA(string PlantId, string fdate, string tdate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;       
            try
            {
                var sql = @"select systemid,EmployeeStatus,EmployeeCurrentStatus,
                            EmployeeCurrentStatusEffectiveDate  from EmployeeInformation
                                where EmployeeStatus='Active' and systemid in
                                (
                                select EmpSystemID from AttdnProcessData p 
								left join EmployeeInformation e on p.EmpSystemID=e.SystemId
                                where EmployeeCurrentStatus='LONG ABSENTEEISM' and 
								e.PlantId='"+PlantId+@"'
								and WorkDate between '"+fdate+@"'  and '"+tdate+@"' and
								(p.PresentValue='1' or p.LateValue='1' or p.LvValue='1' or 
								p.PresentValue='0.5')   
								and EmployeeCurrentStatusEffectiveDate<='"+tdate+"')";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }

        #endregion

        #region Supporting Functions

        private static string GetNumData(string strNumber)
        {
            double d;
            strNumber = strNumber.Replace(",", "");
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0"; }
            else if (Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return strNumber;
            }
            else
            {
                return "0";
            }
        }        
        public void UpdateStatus(string sql)
        {
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(sql, true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }

        #endregion

        public void TBS_LA_Process(string Date, string PlantValue)
        {
            try
            {
                // Change to Long Absentism
                EmployeeAutoStatusChange_LA(PlantValue, Date);

                // Change to TBS
                EmployeeAutoStatusChange_TBS(PlantValue, Date);

                // Reverse LA
                EmployeeAutoStatusChange_LA_Reverse(PlantValue, Date);

            }
            catch (Exception ex)
            {
                throw ex;
            }       
        }

        #endregion

        #region Leave Process
        public void LeaveProcess(string Date, string PlantValue, string UserId = null)
        {

            ProcessLock _lock = new ProcessLock(UserId, ProcessLockId.AttendanceProcess, "", 60);
            _lock.LockProcess();
            try
            {
                Date = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");
                string PreviousDay = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");
               
                DataSet PlantLock; // Previous Day Plant Lock Checking
                PlantLockCheck(PreviousDay, out PlantLock, PlantValue);
                if (PlantLock.Tables[0].Rows.Count > 0)
                {

                }
                else
                {
                    #region Year and From To Date Finding
                   
                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    DataSet YearFinding;
                    string YearId="";
                    FindLeaveYear(PreviousDay, out YearFinding, PlantValue);
                    if (YearFinding.Tables[0].Rows.Count > 0)
                    {
                        YearId = YearFinding.Tables[0].Rows[0][@"Id"].ToString();
                    }

                    DataTable DateTbl;
                    var str = @"select FromDate,ToDate from LeaveYearDefination where id='" + YearId + "'";
                    DateTbl = _sqlRepository.GetDataTable(str);
                    string From = "", To = "";
                    if (DateTbl.Rows.Count > 0)
                    {
                        From = DateTbl.Rows[0]["FromDate"].ToString();
                        To = DateTbl.Rows[0]["ToDate"].ToString();
                    }

                    #endregion

                    #region Saving Logic 

                    DataSet dsRef, dsSource;
                    var sqlx = @"select * from AnnualLeaveDataCurrent where PlantId='"+PlantValue+"'and LeaveYearId='"+YearId+"'";
                    objCon.OpenDataSetThroughAdapter(sqlx, out dsRef, false, false, "", "1");

                    LeaveSourceDataGeneration(From, To, out dsSource, PlantValue, YearId);
                    if(dsSource.Tables[0].Rows.Count>0)
                    {
                        for (int i = 0; i < dsSource.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = clsWebLib.RetValidLen(dsSource.Tables[0].Rows[i][@"EmpId"]).ToString();
                            string LvYearId = clsWebLib.RetValidLen(dsSource.Tables[0].Rows[i][@"LeaveYearId"]).ToString();
                            string LvTypeId = clsWebLib.RetValidLen(dsSource.Tables[0].Rows[i][@"LeaveTypeId"]).ToString();
                            decimal Availed = Convert.ToDecimal(clsWebLib.RetValidLen(dsSource.Tables[0].Rows[i][@"Availed"]).ToString());
                            decimal Earned = Convert.ToDecimal(clsWebLib.RetValidLen(dsSource.Tables[0].Rows[i][@"Earned"]).ToString());

                            dsRef.Tables[0].DefaultView.RowFilter = @"EmployeeId='" + EmpId + "' AND LeaveTypeId='"+LvTypeId+ "' AND LeaveYearId='"+ LvYearId+"'";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                dr["Availed"] = Availed;
                                dr["Earned"] = Earned;
                                dr["UpdatedFromIp"] = "1";
                                dr["UpdatedBy"] = "Schedule";
                                dr["UpdatedDate"] = DateTime.Now.ToString();
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);
                    }

                    #endregion

                }
                _lock.UnlockProcess();

            }
            catch (Exception ex)
            {
                _lock.UnlockProcess();
                throw ex;
            }
        }
        #endregion

        #region Leave Process Source Data

        public void FindLeaveYear(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select ld.* from LeaveYearDefination ld left 
                join LeaveYearDefinationPlantChild pc on
                pc.LeaveYearDefinationId=ld.Id where 
                FromDate<='" + Date+"' and '"+Date+ "'<=ToDate and pc.PlantId='"+Plant+"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        public void LeaveSourceDataGeneration(string From,string To, out DataSet ds, string Plant,string YearId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select dd.*,case when lpd.EncashWorkingDaysQty > 0 
                then dd.EarnDays/lpd.EncashWorkingDaysQty else 
				0 END as Earned	 
			    from (select e.SystemId as EmpId,e.EmployeeCode,ld.Id as 
                LeaveYearId,ld.UserName as LeaveYear,p.UserName as Plant,
                lt.UserName as LeaveType,lt.Id as LeaveTypeId,lt.Code,
                isnull(Masterx.EarnDays,'0')+ isnull(md.Earned,'0')EarnDays,
                Availed= (isnull(Info.AvailedLeave,'0')+isnull(md.Availed,'0')),
			    Info.EmpTypeId,Info.LeavePolicyMasterId
                from LeaveYearDefination ld 
                left join LeaveYearDefinationPlantChild pc on 
				pc.LeaveYearDefinationId=ld.Id and pc.PlantId='" + Plant+@"'
                left join org.Plant p on p.Id=pc.PlantId
				left join org.Company c on c.Id=p.CompanyId
                left join org.CompanyGroup cg on cg.Id=c.CompanyGroupId
                left join LeaveType lt on lt.CompanyGroupId=cg.Id 
                left join EmployeeInformation e on e.PlantId=p.Id
                left join ManualLeaveData md on md.EmployeeId=e.SystemId
				and md.LeaveYearId=ld.Id and 
				md.LeaveTypeId=lt.Id and md.PlantId='"+Plant+@"'
                left join AnnualLeaveDataCurrent ac on ac.EmployeeId=e.SystemId
				and ac.LeaveYearId=ld.Id and ac.LeaveTypeId=lt.Id and ac.PlantId='"+Plant+ @"'
				left join
				(
				select a.EmpSystemID,SUM(a.LvValue)AvailedLeave,A.DayStatus,a.PlantID,dc.EmpTypeId,
                dxc.LeavePolicyMasterId				
				from AttdnProcessData a left join EmployeeInformation ei on a.EmpSystemID=ei.SystemId
				left join mst.DesignationMasterLegalDesignation ddm on ddm.LegalDesignationId = 
		        ei.LegalDesignationId
				left join mst.DesignationMaster 
				dm on dm.Id = ddm.DesignationMasterId
				left join scs.DesignationMasterConfiguration dxc on dxc.DesignationMasterId=dm.Id
				and dxc.PlantId=ei.PlantId
				left join DayStatusPlantChild 
				dc on dc.EmpTypeId=dm.EmployeeCategoryId
				and dc.PlantId=ei.PlantId
				left join DayStatusHeader dh on dh.Id=dc.headerId
				left join DayTypeWithValues dt on dt.HeaderId=dh.Id
				and dt.DayType=a.DayStatus				
				where dt.HeaderId is not null and 
				a.LvValue<>0 and ei.EmployeeStatus='Active'
				and 
				a.workdate between '" + From+@"' and '"+To+@"'
				and ei.PlantId='"+Plant+ @"'
				group by A.EmpSystemID,a.DayStatus,a.PlantID,dc.EmpTypeId,
                dxc.LeavePolicyMasterId ) as Info
				on Info.EmpSystemID=e.SystemId and Info.PlantID=e.PlantId 
				and Info.DayStatus=lt.Code
                left join (SELECT EmpSystemID,SUM(l.EarnValue)EarnDays,T.Id as LeaveId,ei.PlantId
                FROM  EmployeeInformation AS ei 
                JOIN AttdnProcessData AS apd   ON apd.EmpSystemID=ei.SystemId
                LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId
                AND dmc.PlantId=ei.PlantId
                LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                left JOIN DayTypeWithValues AS ds ON ds.DayType=apd.DayStatus AND ds.HeaderId=pc.HeaderId
                LEFT JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 
                JOIN LeaveType T ON t.Id=L.LeaveTypeId
                where apd.workdate between '" + From+"' and '"+To+@"'
                and EI.PlantID='"+Plant+@"' and t.LeaveType='Earn'
                group by EmpSystemID,t.Id,ei.plantid
                ) as Masterx on Masterx.EmpSystemID=e.SystemId 
				and e.PlantId=Masterx.PlantId and
                Masterx.LeaveId=lt.Id          
                where p.Id='"+Plant+"' and ld.Id='"+YearId+ @"' and			
                e.EmployeeStatus='Active' ) as dd
                left join LeavePolicyDetail lpd on lpd.LPMSystemID=dd.LeavePolicyMasterId
				and lpd.LTSystemID=dd.LeaveTypeId	
                order by dd.EmpId,dd.LeaveTypeId";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }

        #endregion

        #region Save Function

        public static void SaveLog(string Message, string UserName, bool isError = false)
        {
            if (Message.Length > 2000)
                Message = Message.Substring(0, 2000);

            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter("select * from SchedulerLog where 1=2", out DataSet dsRef, false, false, "", "1");

            DataRow dr = dsRef.Tables[0].NewRow();
            dr["ScheduleMessage"] = Message;
            dr["UserName"] = UserName;
            dr["isError"] = isError;
            dr["AddedDate"] = DateTime.Now.ToString();
            dsRef.Tables[0].Rows.Add(dr);


            SaveDataSets(dsRef);
        }
        private static void SaveDataSets(params DataSet[] dsRef)
        {
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
                catch (Exception)
                {
                    
                }
                throw ex;
            }
            finally
            {
                objCon = null;
            }
        }
        private static Dictionary<string, object> InitializeMyDictionary()
        {
            Dictionary<string, object> ds = new Dictionary<string, object>
        {
            { "Id", "" },
            { "PlantId", "" },
            { "RPHeaderId", "" },
            { "WorkDate", "" },
            { "ShiftDefinationID", "" },
            { "ShiftSequence", "" },
        };
            return ds;
        }
        public void Add(DataTable dt, Dictionary<string, object> sourceData)
        {
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
            dr["AddedBy"] = "RosterProcess";
            dr["DateAdded"] = DateTime.Now.ToString(); 
            dr["AddedFromIP"] = "1";
            dr["UpdatedBy"] = "RosterProcess";
            dr["DateUpdated"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = "1";

            dt.Rows.Add(dr);
        }

        #endregion 

        #region GroupWise Calling Functions
        public void ShiftProcessGroupWise(string Date, string GroupId)
        {
            DataSet PlantList;
            GetPlant(GroupId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {

                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        ShiftProcess(Date, PlantValue,"Scheduler");
                    }
                    catch (Exception ex)
                    {
                        CommonLogFunction(ex, CatchPlant, "ShiftProcess");
                    }
                }
            }
        }

        public void AttendanceProcessGroupWise(string Date, string GroupId)
        {
            DataSet PlantList;
            GetPlant(GroupId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {

                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        AttndProcess(Date, PlantValue,"Scheduler");
                    }
                    catch (Exception ex)
                    {
                        CommonLogFunction(ex, CatchPlant, "AttdnProcess");
                    }
                }
            }
        }

        public void DayStatusProcessGroupWise(string Date, string GroupId)
        {
            DataSet PlantList;
            GetPlant(GroupId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {

                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                    try
                    {
                        CatchPlant = PlantValue;
                        DayStatus(Date, PlantValue,"Scheduler");
                    }
                    catch (Exception ex)
                    {
                        CommonLogFunction(ex, CatchPlant, "DayStatusProcess");
                    }
                    try                        
                    {
                        CatchPlant = PlantValue;
                        ManualScheduler(PlantValue);
                    }
                    catch (Exception ex)
                    {
                        CommonLogFunction(ex, CatchPlant, "ManualProcess");
                    }
                    
                }
            }
        }

        public void DOJProcessGroupWise(string Date, string GroupId)
        {
            // Log Check
            SaveLog("Group Call", "DOJProcess", false);

            DataSet PlantList;
            GetPlant(GroupId, out PlantList);           
          
            if (PlantList.Tables[0].Rows.Count > 0)
            {

                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        PastDOJProcess(Date, PlantValue,"Scheduler");
                    }
                    catch (Exception ex)
                    {
                        CommonLogFunction(ex, CatchPlant, "DOJProcess");
                    }

                }
            }
        }

        public void RosterProcessGroupWise(string Date, string GroupId)
        {
            DataSet PlantList;
            GetPlant(GroupId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {

                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString(); 
                        CatchPlant = PlantValue;
                        RosterProcess(PlantValue, Date,"Scheduler");
                    }
                    catch (Exception ex)
                    {
                        CommonLogFunction(ex, CatchPlant, "RosterProcess");                       
                    }
                }
            }
        }

        public void TBS_LA_ProcessGroupWise(string Date, string GroupId)
        {
            // Log Check
            SaveLog("Group Call", "TBS_LA_Process", false);

            DataSet PlantList;
            GetPlant(GroupId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {

                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        TBS_LA_Process(Date, PlantValue);
                    }
                    catch (Exception ex)
                    {
                        CommonLogFunction(ex, CatchPlant, "TBS_LA_Process");
                    }

                }
            }
        }

        #endregion

        public void CommonLogFunction(Exception ex, string CatchPlant,string Process)
        {
            string error = "Plant:- " + CatchPlant + " Exception :-" +ex.ToString();
            SaveLog(error, Process, true);
       
        } 

    }

    public static class ExceptionLogging
    {

        private static String ErrorlineNo, Errormsg, extype, exurl, ErrorLocation;

        public static void SendErrorToText(Exception ex)
        {
            var line = Environment.NewLine + Environment.NewLine;

            ErrorlineNo = ex.StackTrace.Substring(ex.StackTrace.Length - 7, 7);
            Errormsg = ex.GetType().Name.ToString();
            extype = ex.GetType().ToString();
            exurl = context.Current.Request.Url.ToString();
            ErrorLocation = ex.Message.ToString();

            try
            {
                string filepath = context.Current.Server.MapPath("~/ExceptionDetailsFile/");  //Text File Path

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);

                }
                filepath = filepath + DateTime.Today.ToString("dd-MM-yy") + ".txt";   //Text File Name
                if (!File.Exists(filepath))
                {


                    File.Create(filepath).Dispose();

                }
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    string error = "Log Written Date:" + " " + DateTime.Now.ToString() + line + "Error Line No :" + " " + ErrorlineNo + line + "Error Message:" + " " + Errormsg + line + "Exception Type:" + " " + extype + line + "Error Location :" + " " + ErrorLocation + line + " Error Page Url:" + " " + exurl + line;
                    sw.WriteLine("-----------Exception Details on " + " " + DateTime.Now.ToString() + "-----------------");
                    sw.WriteLine("-------------------------------------------------------------------------------------");
                    sw.WriteLine(line);
                    sw.WriteLine(error);
                    sw.WriteLine("--------------------------------*End*------------------------------------------");
                    sw.WriteLine(line);
                    sw.Flush();
                    sw.Close();

                }

            }
            catch (Exception e)
            {
                e.ToString();

            }
        }

    }

}
  