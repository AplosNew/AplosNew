using bplib;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public void ShiftProcess(string Date, string PlantValue)
        {
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
                    UnProcessedEmp(Date, out UnProcessed, PlantValue);
                    if (UnProcessed.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = UnProcessed.Tables[0].Rows[0][@"WorkDate"].ToString();
                        var GpId = UnProcessed.Tables[0].Rows[0][@"GroupID"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter("select * from AttdnProcessData where WorkDate='" + WkDate + "'and PlantID='" + PlantValue + "'", out DataSet dsRef, false, false, "", "1");

                        objCon.OpenDataSetThroughAdapter("select * from LeaveEarned where WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'", out DataSet dsEarnedLeave, false, false, "", "1");

                        for (int i = 0; i < UnProcessed.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = UnProcessed.Tables[0].Rows[i][@"SystemId"].ToString();
                            string PlantId = UnProcessed.Tables[0].Rows[i][@"PlantId"].ToString();
                            string RowId = UnProcessed.Tables[0].Rows[i][@"RowId"].ToString();
                            string ManualShift = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ManualShift"]).ToString();
                            string ManualShiftDurn = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ManualDuration"]).ToString();
                            string ManualShiftIn = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ManualShiftIn"]).ToString();
                            string ManualShiftOut = clsWebLib.RetValidLen(UnProcessed.Tables[0].Rows[i][@"ManualShiftOut"]).ToString();
                            string ManualInTime = UnProcessed.Tables[0].Rows[i][@"ManualInTime"].ToString();
                            string ManualOuTime = UnProcessed.Tables[0].Rows[i][@"ManualOutTime"].ToString();
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
                            var RosterId = UnProcessed.Tables[0].Rows[i][@"RosterId"].ToString();
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
                                dr["PlantInPunchStartTime"] = clsWebLib.RetValidLen(PlantInPunchStartTime);

                                #region ManualData Entry

                                dr["ManualInTime"] = clsWebLib.RetValidLen(ManualInTime);
                                dr["ManualOutTime"] = clsWebLib.RetValidLen(ManualOuTime);
                                dr["ManualDayStatus"] = clsWebLib.RetValidLen(ManualDayStatus);
                                dr["IsManualInTime"] = clsWebLib.GetBoolData(IsManualInTime);
                                dr["IsManualOutTime"] = clsWebLib.GetBoolData(IsManualOutTime);
                                dr["IsManualDayStatus"] = clsWebLib.GetBoolData(IsManualDayStatus);

                                #endregion

                                #region AssignedShift Data
                                if (ManualShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = ManualShift;
                                    dr["ShiftDuration"] = ManualShiftDurn;
                                    dr["ShiftInTime"] = ManualShiftIn;
                                    dr["ShiftOutTime"] = ManualShiftOut;
                                }
                                else if (RosterShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = RosterShift;
                                    dr["ShiftDuration"] = RosterShiftDurn;
                                    dr["ShiftInTime"] = RosterShiftIn;
                                    dr["ShiftOutTime"] = RosterShiftOut;

                                }
                                else if (ProfileShift.ToString() != "")
                                {
                                    dr["ShiftSystemID"] = ProfileShift;
                                    dr["ShiftDuration"] = ProfileShiftDurn;
                                    dr["ShiftInTime"] = ProfileShiftIn;
                                    dr["ShiftOutTime"] = ProfileShiftOut;

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
                                dr["ManualOt"] = "0";
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

                            dsEarnedLeave.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                            if (dsEarnedLeave.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow drx = dsEarnedLeave.Tables[0].NewRow();
                                drx["EmpSystemID"] = EmpId;
                                drx["RowId"] = RowId;
                                drx["WorkDate"] = WkDate;
                                drx["GroupID"] = GpId;
                                drx["DayStatus"] = DBNull.Value;
                                drx["EarnedPriviledgeLeave"] = 0;
                                drx["EarnedCasualLeave"] = 0;
                                drx["PlantID"] = PlantId;
                                drx["AddedBy"] = "Schedule";
                                drx["DateAdded"] = Convert.ToDateTime(DateTime.Now);
                                dsEarnedLeave.Tables[0].Rows.Add(drx);
                            }
                        }
                        SaveDataSets(dsRef, dsEarnedLeave);

                    }
                    #endregion

                    #region Shift Not Assigned Employee
                    DataSet ShiftNotAssigned;
                    TopShift(out ShiftNotAssigned, PlantValue);
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
                        if (dsRef.Tables[0].Rows.Count > 0)
                        {
                            for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                            {

                                EmpSet += ",'" + dsRef.Tables[0].Rows[i][@"RowId"].ToString() + "'";

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

                    }
                    #endregion

                    #region Ramadan Shift Flagging
                    DataSet RamadanShift;
                    ChangedShift(Date, out RamadanShift, PlantValue);
                    if (RamadanShift.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = RamadanShift.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID ='" + PlantValue + "' ";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < RamadanShift.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = RamadanShift.Tables[0].Rows[i][@"EmpSystemID"].ToString();
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
                    LeaveData(Date, out Leavedata, PlantValue);
                    if (Leavedata.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = Leavedata.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID ='" + PlantValue + "' ";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < Leavedata.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = Leavedata.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                            string LTSystemID = Leavedata.Tables[0].Rows[i][@"LTSystemID"].ToString();
                            decimal LeaveDuration = Convert.ToDecimal(Leavedata.Tables[0].Rows[i][@"LeaveDuration"].ToString());
                            string LeaveStatus = Leavedata.Tables[0].Rows[i][@"Code"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";

                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["LeaveDuration"] = LeaveDuration;
                                dr["LTSystemID"] = clsWebLib.RetValidLen(LTSystemID);
                                dr["LeaveStatus"] = clsWebLib.RetValidLen(LeaveStatus);
                                dr["UpdatedBy"] = "Schedule";
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                        #region Update in LeaveTransactionDetail
                        LeaveAvailUpdate(Date, PlantValue);
                        #endregion
                    }
                    #endregion

                    #region CompanyWeekOff Flagging
                    DataSet CompanyWeekOff;
                    CompanyWeekOffData(Date, out CompanyWeekOff, PlantValue);
                    if (CompanyWeekOff.Tables[0].Rows.Count > 0)
                    {

                        for (int i = 0; i < CompanyWeekOff.Tables[0].Rows.Count; i++)
                        {

                            string PlantId = CompanyWeekOff.Tables[0].Rows[i][@"PlantId"].ToString();
                            string WkDate = CompanyWeekOff.Tables[0].Rows[i][@"WkDate"].ToString();

                            var sql = @"Update AttdnProcessData Set WeeklyStatus='W'  
                            WHERE WorkDate='" + WkDate + "'AND isnull(EmpSystemID,'') IN" +
                            " (SELECT isnull(ei.SystemId,'')   FROM EmployeeInformation AS " +
                            "ei WHERE  ei.PlantId ='" + PlantId + "' AND ei.EmployeeStatus='Active'" +
                            "and  ISNULL(EmpSystemID,'') not in (select distinct ISNULL(EmpSystemID,'') " +
                            "from EmployeeWeeklyOff))";


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

                        var sql = @"Update AttdnProcessData Set WeeklyStatus='NW'  
                           WHERE WorkDate='" + Date + @"' AND isnull(EmpSystemID,'') IN" +
                           " (SELECT isnull(ei.SystemId,'')   FROM EmployeeInformation AS " +
                           "ei WHERE  ei.PlantId='" + PlantValue + "' AND ei.EmployeeStatus='Active'" +
                           "and  ISNULL(EmpSystemID,'') not in (select distinct ISNULL(EmpSystemID,'') " +
                           "from EmployeeWeeklyOff))";


                        ConnectionManager.DAL.ConManager objCone = null;
                        objCone = new ConnectionManager.DAL.ConManager("1");
                        objCone.OpenConnection("1");
                        objCone.BeginTransaction();

                        objCone.ExecuteNonQueryWrapper(sql, true, "1");
                        objCone.CommitTransaction();
                    }
                    #endregion

                    #region IndividualWeekOff Flagging
                    DataSet IndividualWeekOff;
                    IndividualWeekOffData(Date, out IndividualWeekOff, PlantValue);
                    if (IndividualWeekOff.Tables[0].Rows.Count > 0)
                    {
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");


                        var sqlx = @"select * from AttdnProcessData 
                    WHERE WorkDate='" + Date + @"'
                     AND isnull(EmpSystemID,'') IN (SELECT isnull(ei.SystemId,'') 
                     FROM EmployeeInformation AS ei WHERE  ei.PlantId='" + PlantValue + @"'
                    AND ei.EmployeeStatus='Active'
					and  ISNULL(EmpSystemID,'') in (select distinct ISNULL(EmpSystemID,'') 
                    from EmployeeWeeklyOff))";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");
                        string newformat = Convert.ToDateTime(Date).ToString("yyyyMMdd");

                        for (int i = 0; i < IndividualWeekOff.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = IndividualWeekOff.Tables[0].Rows[i][@"SystemId"].ToString();
                            string DayType = clsWebLib.RetValidLen(IndividualWeekOff.Tables[0].Rows[i][@"DayType"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                if (DayType.ToString() != "")
                                {
                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();
                                    dr["UpdatedBy"] = "Schedule";
                                    dr["WeeklyStatus"] = DayType;
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();
                                }
                            }
                        }
                        SaveDataSets(dsRef);
                    }
                    #endregion

                    #region Original Date Compensatory Check
                    DataSet OriginalDateComp;
                    OriginalDateData(Date, out OriginalDateComp, PlantValue);
                    if (OriginalDateComp.Tables[0].Rows.Count > 0)
                    {
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");

                        string WkDate = OriginalDateComp.Tables[0].Rows[0][@"WkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                        var sqlx = @"SELECT * FROM AttdnProcessData where (WeeklyStatus='W' or HolidayStatus='H') and 
				                WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < OriginalDateComp.Tables[0].Rows.Count; i++)
                        {
                            string Plant = OriginalDateComp.Tables[0].Rows[i][@"PlantId"].ToString();
                            string ForEntirePlant = clsWebLib.GetBoolData(OriginalDateComp.Tables[0].Rows[i][@"ForEntirePlant"]).ToString();
                            string Type = clsWebLib.RetValidLen(OriginalDateComp.Tables[0].Rows[i][@"Type"]).ToString();
                            string EmpId = clsWebLib.RetValidLen(OriginalDateComp.Tables[0].Rows[i][@"EmpSystemId"]).ToString();

                            if (ForEntirePlant == "1")
                            {
                                if (Type == "W")
                                {

                                    var sql = @"Update AttdnProcessData Set WeeklyStatus='WW'    
                              WHERE WorkDate='" + WkDate + "' AND WeeklyStatus='W' AND " +
                                      "isnull(EmpSystemID,'') IN" +
                                      " (SELECT isnull(ei.SystemId,'')   FROM EmployeeInformation AS " +
                                      "  ei WHERE  ei.PlantId ='" + Plant + "' AND ei.EmployeeStatus='Active')";


                                    ConnectionManager.DAL.ConManager objCone = null;
                                    objCone = new ConnectionManager.DAL.ConManager("1");
                                    objCone.OpenConnection("1");
                                    objCone.BeginTransaction();

                                    objCone.ExecuteNonQueryWrapper(sql, true, "1");
                                    objCone.CommitTransaction();

                                }
                                if (Type == "H")
                                {
                                    var sql = @"Update AttdnProcessData Set HolidayStatus='NH'  
                                          WHERE WorkDate='" + WkDate + "' AND HolidayStatus='H' AND " +
                                      "isnull(EmpSystemID,'') IN" +
                                      " (SELECT isnull(ei.SystemId,'')   FROM EmployeeInformation AS " +
                                      "  ei WHERE  ei.PlantId ='" + Plant + "' AND ei.EmployeeStatus='Active')";


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
                                if (Type == "H")
                                {
                                    dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                    if (dsRef.Tables[0].DefaultView.Count > 0)
                                    {
                                        DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                        dr.BeginEdit();
                                        dr["UpdatedBy"] = "Schedule";
                                        dr["HolidayStatus"] = "NH";
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                    }
                                }
                                if (Type == "W")
                                {
                                    dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                    if (dsRef.Tables[0].DefaultView.Count > 0)
                                    {
                                        DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                        dr.BeginEdit();
                                        dr["UpdatedBy"] = "Schedule";
                                        dr["WeeklyStatus"] = "WW";
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                    }
                                }
                            }
                        }
                        SaveDataSets(dsRef);
                    }
                    #endregion

                    #region Compensatory Date Compensatory Check
                    DataSet CompensatoryDateComp;
                    CompensatoryData(Date, out CompensatoryDateComp, PlantValue);
                    if (CompensatoryDateComp.Tables[0].Rows.Count > 0)
                    {
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");

                        string WkDate = CompensatoryDateComp.Tables[0].Rows[0][@"WkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                        var sqlx = @"SELECT * FROM AttdnProcessData where (ISNULL(WeeklyStatus,'')!='W' and 
                ISNULL(HolidayStatus,'')!='H')	and WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < CompensatoryDateComp.Tables[0].Rows.Count; i++)
                        {
                            string Plant = CompensatoryDateComp.Tables[0].Rows[i][@"PlantId"].ToString();
                            string ForEntirePlant = clsWebLib.GetBoolData(CompensatoryDateComp.Tables[0].Rows[i][@"ForEntirePlant"]).ToString();
                            string Type = clsWebLib.RetValidLen(CompensatoryDateComp.Tables[0].Rows[i][@"Type"]).ToString();
                            string EmpId = clsWebLib.RetValidLen(CompensatoryDateComp.Tables[0].Rows[i][@"EmpSystemId"]).ToString();

                            if (ForEntirePlant == "1")
                            {
                                if (Type == "W")
                                {

                                    var sql = @"Update AttdnProcessData Set ManualDayStatus='CW',IsManualDayStatus=1   
                              WHERE WorkDate='" + WkDate + "' AND WeeklyStatus!='W' AND " +
                                      "isnull(EmpSystemID,'') IN" +
                                      " (SELECT isnull(ei.SystemId,'')   FROM EmployeeInformation AS " +
                                      "  ei WHERE  ei.PlantId ='" + Plant + "' AND ei.EmployeeStatus='Active')";


                                    ConnectionManager.DAL.ConManager objCone = null;
                                    objCone = new ConnectionManager.DAL.ConManager("1");
                                    objCone.OpenConnection("1");
                                    objCone.BeginTransaction();

                                    objCone.ExecuteNonQueryWrapper(sql, true, "1");
                                    objCone.CommitTransaction();

                                }
                                if (Type == "H")
                                {
                                    var sql = @"Update AttdnProcessData Set ManualDayStatus='AH',IsManualDayStatus=1  
                              WHERE WorkDate='" + WkDate + "' AND HolidayStatus!='H' AND " +
                                      "isnull(EmpSystemID,'') IN" +
                                      " (SELECT isnull(ei.SystemId,'')   FROM EmployeeInformation AS " +
                                      "  ei WHERE  ei.PlantId ='" + Plant + "' AND ei.EmployeeStatus='Active')";


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
                                if (Type == "H")
                                {
                                    dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                    if (dsRef.Tables[0].DefaultView.Count > 0)
                                    {
                                        DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                        dr.BeginEdit();
                                        dr["UpdatedBy"] = "Schedule";
                                        dr["ManualDayStatus"] = "AH";
                                        dr["IsManualDayStatus"] = 1;
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                    }
                                }
                                if (Type == "W")
                                {
                                    dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                    if (dsRef.Tables[0].DefaultView.Count > 0)
                                    {
                                        DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                        dr.BeginEdit();
                                        dr["UpdatedBy"] = "Schedule";
                                        dr["IsManualDayStatus"] = 1;
                                        dr["ManualDayStatus"] = "CW";
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                    }
                                }
                            }
                        }
                        SaveDataSets(dsRef);
                    }
                    #endregion

                    #region OTEligibleData Flagging
                    DataSet OTElgbEmp;
                    OTEligibleEmp(Date, out OTElgbEmp, PlantValue);
                    if (OTElgbEmp.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = OTElgbEmp.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < OTElgbEmp.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = OTElgbEmp.Tables[0].Rows[i][@"EmpId"].ToString();
                            string IsOTEntitled = OTElgbEmp.Tables[0].Rows[i][@"IsOTEntitled"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
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
                    if (OnDuty.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = OnDuty.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < OnDuty.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = OnDuty.Tables[0].Rows[i][@"EmpSystemId"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["IsOD"] = 1;
                                dr["ManualDayStatus"] = "OD";
                                dr["UpdatedBy"] = "Schedule";
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
                        string WorkDate = OnRest.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < OnRest.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = OnRest.Tables[0].Rows[i][@"EmpSystemId"].ToString();
                            string RestId = clsWebLib.RetValidLen(OnRest.Tables[0].Rows[i][@"RestId"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["AttendanceRestDetailId"] = RestId;
                                dr["ManualDayStatus"] = "RST";
                                dr["UpdatedBy"] = "Schedule";
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region OTDayLimit Process Row Creation
                    DataSet OTDayLimit;
                    OTDayLimitRowCreation(Date, out OTDayLimit, PlantValue);
                    if (OTDayLimit.Tables[0].Rows.Count > 0)
                   {
                        var WkDate = OTDayLimit.Tables[0].Rows[0][@"WorkDate"].ToString();
                        var GpId = OTDayLimit.Tables[0].Rows[0][@"GroupID"].ToString();
                        var PlantId = OTDayLimit.Tables[0].Rows[0][@"PlantID"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter("select * from OTProcessDayLimit where WorkDate='" + WkDate + "'and PlantID='" + PlantId + "'", out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < OTDayLimit.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = OTDayLimit.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                            string RowId = OTDayLimit.Tables[0].Rows[i][@"RowId"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow drx = dsRef.Tables[0].NewRow();
                                drx["EmpSystemID"] = EmpId;
                                drx["RowId"] = RowId;
                                drx["WorkDate"] = WkDate;
                                drx["GroupID"] = GpId; 
                                drx["PlantID"] = PlantId;
                                drx["DayType"] = DBNull.Value;
                                drx["PlannedOT"] = 0;
                                drx["FixedOT"] = 0; 
                                drx["LimitSettingOT"] = 0; 
                                drx["SlabOT"] = 0;                                
                                drx["AddedBy"] = "Schedule";
                                drx["DateAdded"] = Convert.ToDateTime(DateTime.Now);
                                dsRef.Tables[0].Rows.Add(drx);
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion
                    
                }
            }
            catch (Exception ex)
            {
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
else 'false' end ,e.SystemId,'" + Date + @"' as WorkDate,
convert(varchar(30),'" + newformat + @"' )+convert(varchar(30), e.SystemId)RowId,e.PlantId,e.GroupID,
m.ShiftSystemId 
as ManualShift,sd.InTime as ManualShiftIn,sd.OutTime as ManualShiftOut,sd.ShiftDuration as ManualDuration,
e.ProfileShiftId as ProfileShift,sdx.InTime as ProfileShiftIn,sdx.OutTime as ProfileShiftOut,
sdx.ShiftDuration as ProfileDuration,
mb.ShiftDefinationId as BudgetedShift,sdy.InTime as BudgetShiftIn,sdy.OutTime as BudgetShiftOut,
sdy.ShiftDuration as BudgetDuration,rp.ShiftDefinationID as RosterShift,sdz.InTime as RosterShiftIn,
sdz.OutTime as RosterShiftOut,sdz.ShiftDuration as RosterDuration,m.InTime as ManualInTime,m.OutTime as ManualOutTime,
m.DayStatus as ManualDayStatus,IsManualDayStatus=case When isnull(m.DayStatus,'') ='' then 'false' 
else 'true' end,IsManualInTime=case When isnull(m.InTime,'') ='' then 'false' 
else 'true' end,IsManualOutTime=case When isnull(m.OutTime,'') ='' then 'false' 
else 'true' end,mb.Id as BudgetId,rh.Id as RosterId,Op.InPunchStartTime as PlantInPunchStartTime, 
FullDayDuration=isnull(isnull(sd.FullDayDuration,sdz.FullDayDuration),
isnull(sdx.FullDayDuration,sdy.FullDayDuration)),HalfDayDuration=isnull(isnull(sd.HalfDayDuration,sdz.HalfDayDuration),
isnull(sdx.HalfDayDuration,sdy.HalfDayDuration)),ShortDuration=isnull(isnull(sd.ShortDuration,sdz.ShortDuration),
isnull(sdx.ShortDuration,sdy.ShortDuration)),HoursWithoutOT=isnull(isnull(sd.HoursWithoutOT,sdz.HoursWithoutOT),
isnull(sdx.HoursWithoutOT,sdy.HoursWithoutOT))
from EmployeeInformation e 
left join ShiftDefination sdx on sdx.SystemID=e.ProfileShiftId
left outer join AttndManualDataFromApp m on e.SystemId=m.EmpSystemID and m.WorkDate='" + Date + @"'
left join ShiftDefination sd on sd.SystemID=m.ShiftSystemId
left join AttdnProcessData p on p.EmpSystemID=e.SystemId and p.WorkDate='" + Date + @"'
left join mst.ManpowerBudget mb on mb.Id=e.BudgetCode
left join ShiftDefination sdy on sdy.SystemID=mb.ShiftDefinationId
left join dbo.RosterBudget rb on rb.BudgetId=mb.Id 
left join RosterPatternHeader rh on rh.Id=rb.RosterId
left join dbo.RosterPatternProcess rp on rp.RPHeaderId=rh.Id and rp.WorkDate='" + Date + @"'
left join ShiftDefination sdz on sdz.SystemID=rp.ShiftDefinationID
left join org.Plant pl on pl.Id=e.PlantId
left join OutPunchConfigurationHeader Op on OP.PlantId=pl.Id
where e.EmployeeStatus='Active' and e.EmpType!='Guest' and e.PlantId='" + PlantId + @"'";

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
                 where p.WorkDate='" + Date+@"' and 
				 e.PlantId='"+PlantId+@"' 
                and e.EmployeeStatus='Active'
                and dc.IsOTEntitled=1
				 and e.SystemId not in
                (select final.EmpSystemId from (
                select distinct o.empsystemId,(select top 1 Exclude from NonEligibleOT m
                where m.EmpSystemId=o.EmpSystemId
                order by EffectiveDate desc)as x
                from NonEligibleOT o) final where final.x=1)";

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
                    D.LeaveDuration,lt.LTSystemID,LTP.Code
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
        public void OriginalDateData(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select Format(co.OriginalDate,'yyyy-MMM-dd')WkDate,
                co.CompensatoryDateTreatmentType as Type,co.PlantId,
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
        public void CompensatoryData(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select Format(co.CompensatoryDate,'yyyy-MMM-dd')WkDate,
				co.CompensatoryDateTreatmentType as Type,co.PlantId,
				co.ForEntirePlant,coel.EmpSystemId
				from mst.CompensatoryOff AS co 
				left join mst.CompensatoryOffEmpList AS coel on coel.CompensatoryOffId=co.Id
				where co.plantId='" + Plant + @"'
				and co.CompensatoryDate='" + Date + "'";
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

        #endregion

        #region Attendance Process
        public void AttndProcess(string Date, string PlantValue)
        {
            try
            {
                Date = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");
                string PreviousDay = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");

                DataSet PlantLock;
                PlantbothdaysCheck(Date, out PlantLock, PlantValue);
                if (PlantLock.Tables[0].Rows.Count > 0)
                {

                }
                else
                {

                    DataSet ValidationData;
                    Validation(out ValidationData, PlantValue);
                    if (ValidationData.Tables[0].Rows.Count > 0)
                    {
                        #region Getting MissFlagged InPunch of the PrevDay
                        DataSet MissFlaggedIn;
                        ConfirmedPrevMissIn(PreviousDay, out MissFlaggedIn, PlantValue);
                        #endregion

                        #region Process FlaggedIn Data
                        if (MissFlaggedIn.Tables[0].Rows.Count > 0)
                        {
                            string MainRowId = "''";

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + PreviousDay + "' and isnull(PunchInTime,'')='' and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");
                            string newformat = Convert.ToDateTime(PreviousDay).ToString("yyyyMMdd");


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
                            ProcessFlag(MainRowId);
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
                            string MainRowId = "''";

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + PreviousDay + "' and isnull(PunchInTime,'')='' and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");
                            string newformat = Convert.ToDateTime(PreviousDay).ToString("yyyyMMdd");


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
                            ProcessFlag(MainRowId);
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

                                string RowId = "";
                                if (OutPunchRow != "")
                                {
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
                                        {  // Out Limit Validation Check
                                            if (ExistingOut == "" && OutPunch > Convert.ToDateTime(ExistingIn))
                                            {
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchOutTime"] = Convert.ToDateTime(OutPunch);
                                                dr["UpdatedBy"] = "Schedule";
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                MainRowId += ",'" + RowId + "'";
                                            }

                                            else if (ExistingOut != "" && OutPunch > Convert.ToDateTime(ExistingOut) && OutPunch > Convert.ToDateTime(ExistingIn))
                                            {
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchOutTime"] = Convert.ToDateTime(OutPunch);
                                                dr["UpdatedBy"] = "Schedule";
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
                            ProcessFlag(MainRowId);
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
                                            if (ExistingOut == "" && OutPunch > Convert.ToDateTime(ExistingIn))
                                            {
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchOutTime"] = Convert.ToDateTime(OutPunch);
                                                dr["UpdatedBy"] = "Schedule";
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                MainRowId += ",'" + RowId + "'";
                                            }

                                            else if (ExistingOut != "" && OutPunch > Convert.ToDateTime(ExistingOut) && OutPunch > Convert.ToDateTime(ExistingIn))
                                            {
                                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                dr.BeginEdit();
                                                dr["PunchOutTime"] = Convert.ToDateTime(OutPunch);
                                                dr["UpdatedBy"] = "Schedule";
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
                            ProcessFlag(MainRowId);
                            #endregion
                        }
                        #endregion

                        #region App Prev Attnd
                        DataSet PrevDayApp;
                        PrevAppData(PreviousDay, out PrevDayApp, PlantValue);
                        if (PrevDayApp.Tables[0].Rows.Count > 0)
                        {

                            var WkDate = PrevDayApp.Tables[0].Rows[0][@"WkDate"].ToString();
                            string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' " +
                                "and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                            for (int i = 0; i < PrevDayApp.Tables[0].Rows.Count; i++)
                            {
                                string EmpId = clsWebLib.RetValidLen(PrevDayApp.Tables[0].Rows[i][@"EmpId"]).ToString();
                                string In = clsWebLib.RetValidLen(PrevDayApp.Tables[0].Rows[i][@"ManualIn"]).ToString();
                                string Out = clsWebLib.RetValidLen(PrevDayApp.Tables[0].Rows[i][@"ManualOut"]).ToString();

                                PunchTimeVal(ref In, ref Out, WkDate);

                                dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {
                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();

                                    if (Out.ToString() != "")
                                    {
                                        dr["IsManualOutTime"] = 1;
                                        dr["ManualOutTime"] = Convert.ToDateTime(Out);
                                    }
                                    if (In.ToString() != "")
                                    {
                                        dr["ManualInTime"] = Convert.ToDateTime(In);
                                        dr["IsManualInTime"] = 1;
                                    }

                                    dr["UpdatedBy"] = "Schedule";
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();

                                }
                            }
                            SaveDataSets(dsRef);
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
                                                if (Convert.ToDateTime(PlantInLimit) <= MaxTime && MaxTime <= Convert.ToDateTime(PlantOutLimit))
                                                {
                                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                                    dr.BeginEdit();
                                                    dr["PunchOutTime"] = Convert.ToDateTime(MaxTime);
                                                    dr["UpdatedBy"] = "Schedule";
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
                                ProcessFlag(MainRowId);
                                #endregion
                            }
                        }
                        #endregion

                        #region Final PrevDay In/Out                  
                        FinalInOut(PreviousDay, PlantValue);
                        #endregion

                        #region Exception Final PrevDay In/Out  (Wrong Entry Handling)                   
                        ExceptionFinalInOut(PreviousDay, PlantValue);
                        #endregion

                        #region Getting flagged InPunch of the Day
                        DataSet FlaggedIn;
                        ConfirmedInFlagForDay(Date, out FlaggedIn, PlantValue);
                        #endregion

                        #region Process FlaggedIn Data
                        if (FlaggedIn.Tables[0].Rows.Count > 0)
                        {
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
                                InEntryLimit = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(InEntryLimit).ToString("HH:mm:ss");
                            }
                            #endregion

                            for (int i = 0; i < FlaggedIn.Tables[0].Rows.Count; i++)
                            {
                                string EmpId = clsWebLib.RetValidLen(FlaggedIn.Tables[0].Rows[i][@"EmpId"]).ToString();
                                string MinTimeRow = clsWebLib.RetValidLen(FlaggedIn.Tables[0].Rows[i][@"MinTime"]).ToString();
                                string InPunchLimit = clsWebLib.RetValidLen(FlaggedIn.Tables[0].Rows[i][@"InPunchLimit"]).ToString();
                                string OutPunchLimit = clsWebLib.RetValidLen(FlaggedIn.Tables[0].Rows[i][@"OutPunchLimit"]).ToString();
                                DateTime MinTime = new DateTime();

                                string RowId = "";
                                if (MinTimeRow != "")
                                {
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
                            ProcessFlag(MainRowId);
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
                                InEntryLimit = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(InEntryLimit).ToString("HH:mm:ss");
                            }
                            #endregion

                            for (int i = 0; i < FlagglessIn.Tables[0].Rows.Count; i++)
                            {
                                string EmpId = clsWebLib.RetValidLen(FlagglessIn.Tables[0].Rows[i][@"EmpId"]).ToString();
                                string MinTimeRow = clsWebLib.RetValidLen(FlagglessIn.Tables[0].Rows[i][@"MinTime"]).ToString();
                                string InPunchLimit = clsWebLib.RetValidLen(FlagglessIn.Tables[0].Rows[i][@"InPunchLimit"]).ToString();
                                string OutPunchLimit = clsWebLib.RetValidLen(FlagglessIn.Tables[0].Rows[i][@"OutPunchLimit"]).ToString();
                                DateTime MinTime = new DateTime();

                                string RowId = "";
                                if (MinTimeRow != "")
                                {
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
                            ProcessFlag(MainRowId);
                            #endregion
                        }
                        #endregion

                        #region App Today Attnd
                        DataSet TodayApp;
                        TodayAppData(Date, out TodayApp, PlantValue);
                        if (TodayApp.Tables[0].Rows.Count > 0)
                        {

                            var WkDate = TodayApp.Tables[0].Rows[0][@"WkDate"].ToString();
                            string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' " +
                                "and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                            for (int i = 0; i < TodayApp.Tables[0].Rows.Count; i++)
                            {
                                string EmpId = TodayApp.Tables[0].Rows[i][@"EmpId"].ToString();
                                string In = clsWebLib.RetValidLen(TodayApp.Tables[0].Rows[i][@"ManualIn"]).ToString();
                                string Out = clsWebLib.RetValidLen(TodayApp.Tables[0].Rows[i][@"ManualOut"]).ToString();

                                PunchTimeVal(ref In, ref Out, WkDate);

                                dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {
                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();

                                    if (Out.ToString() != "")
                                    {
                                        dr["IsManualOutTime"] = 1;
                                        dr["ManualOutTime"] = Convert.ToDateTime(Out);
                                    }
                                    if (In.ToString() != "")
                                    {
                                        dr["ManualInTime"] = Convert.ToDateTime(In);
                                        dr["IsManualInTime"] = 1;
                                    }

                                    dr["UpdatedBy"] = "Schedule";
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();

                                }
                            }
                            SaveDataSets(dsRef);
                        }
                        #endregion

                        #region Final Day In/Out    
                        FinalInOut(Date, PlantValue);
                        #endregion

                        #region Exception Final Day In/Out  (Wrong Entry Handling)                
                        ExceptionFinalInOut(Date, PlantValue);
                        #endregion

                        #region In Status Logic
                        DataSet InStatus;
                        InStatusCalculate(Date, out InStatus, PlantValue);
                        if (InStatus.Tables[0].Rows.Count > 0)
                        {
                            var WkDate = InStatus.Tables[0].Rows[0][@"WorkDate"].ToString();
                            string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                            for (int i = 0; i < InStatus.Tables[0].Rows.Count; i++)
                            {

                                string EmpId = clsWebLib.RetValidLen(InStatus.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                                string InTime = clsWebLib.RetValidLen(InStatus.Tables[0].Rows[i][@"InTime"]).ToString();
                                string ShiftInTime = clsWebLib.RetValidLen(InStatus.Tables[0].Rows[i][@"ShiftInTime"]).ToString();
                                double ShiftEarlyInMargin = Convert.ToDouble(clsWebLib.RetValidLen(InStatus.Tables[0].Rows[i][@"ShiftEarlyInMargin"]).ToString());
                                double ShiftLateInMargin = Convert.ToDouble(clsWebLib.RetValidLen(InStatus.Tables[0].Rows[i][@"ShiftLateInMargin"]).ToString());

                                dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {

                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();
                                    if (InTime != "" && ShiftInTime != "")
                                    {
                                        if (Convert.ToDateTime(InTime).AddMinutes(ShiftEarlyInMargin) < Convert.ToDateTime(ShiftInTime))
                                        {
                                            dr["InStatus"] = "EI"; //Early In
                                        }

                                        else if (Convert.ToDateTime(InTime).AddMinutes(-ShiftLateInMargin) > Convert.ToDateTime(ShiftInTime))
                                        {
                                            dr["InStatus"] = "LI"; //Late In
                                        }

                                        else
                                        {
                                            dr["InStatus"] = "IN"; // On Time
                                        }
                                    }
                                    else
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
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();
                                }
                            }
                            SaveDataSets(dsRef);

                        }
                        #endregion


                    }
                }
            }
            catch (Exception ex)
            {
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
        public void PlantbothdaysCheck(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string Today = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");
                string PreviousDay = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId='" + Plant + @"'
                and LockedDate between '" + PreviousDay + "' and '" + Today + "' and IsActive='1' ";

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
               '" + CompanyGpId + "' AND  Active = 1 AND Archive = 0";

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
            ConnectionManager.DAL.ConManager objCon;
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
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void ConfirmedPrevFlaglessMissIn(string PrevDay, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
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

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void ConfirmedInFlagForDay(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
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

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void FlaglessInForDay(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
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
                
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void ConfirmedOutFlagPrevDay(string PreviousDate, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
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
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void ConfirmedOutFlaglessPrevDay(string PreviousDate, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
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

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void PrevAppData(string PreDay, out DataSet ds, string Plant)
        {

            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"SELECT EmployeeId as EmpId,FORMAT(PDate,'yyyy-MMM-dd')WkDate,
                InTime as ManualIn,
                OutTime as ManualOut FROM 
                AttdnRawDataFromApp where PDate='" + PreDay + @"'
                and PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void TodayAppData(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"SELECT EmployeeId as EmpId,FORMAT(PDate,'yyyy-MMM-dd')WkDate,
                InTime as ManualIn,
                OutTime as ManualOut FROM 
                AttdnRawDataFromApp where PDate='" + Date + @"'
                and PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
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
				 and PlantID='" + Plant + "' and OutTime>Intime";

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
            ConnectionManager.DAL.ConManager objCon;
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

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlx, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void InStatusCalculate(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct Format(WorkDate,'yyyy-MMM-dd')WorkDate,EmpSystemID,
               Format(ap.InTime,'yyyy-MMM-dd HH:mm:ss')InTime,				
				Format(ap.ShiftInTime,'yyyy-MMM-dd HH:mm:ss')ShiftInTime,  
                sd.ShiftEarlyInMargin,sd.ShiftLateInMargin                
                from Attdnprocessdata  ap
                left join ShiftDefination sd on sd.SystemID=ap.ShiftSystemID
                where workdate='" + Date + @"' and ap.PlantID='" + Plant + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        #endregion

        #region DayStatus Source Data

        public void TodayDuration(string Today, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select * from (select Format(WorkDate,'yyyy-MMM-dd')WorkDate,EmpSystemID,
                datediff(minute,ap.InTime,ap.OutTime) 
                as CalDuration, Format(ap.InTime,'yyyy-MMM-dd HH:mm:ss')InTime,
				Format(ap.OutTime,'yyyy-MMM-dd HH:mm:ss')OutTime,
				Format(ap.ShiftInTime,'yyyy-MMM-dd HH:mm:ss')ShiftInTime, 
                Format(ap.ShiftOutTime,'yyyy-MMM-dd HH:mm:ss')ShiftOutTime, 
                sd.ShiftEarlyInMargin,sd.ShiftEarlyOutMargin,sd.ShiftLateInMargin,
                sd.ShiftLateOutMargin
                from Attdnprocessdata  ap
                left join ShiftDefination sd on sd.SystemID=ap.ShiftSystemID
                where workdate='" + Today + @"' and ap.PlantID='" + Plant + @"' 
                and isnull(ap.InTime,'')!='' and isnull(ap.OutTime,'')!='') as dd
				where dd.CalDuration>0";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        public void PrevDayDuration(string PreDay, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select * from (select Format(WorkDate,'yyyy-MMM-dd')WorkDate,EmpSystemID,
                datediff(minute,ap.InTime,ap.OutTime) 
                as CalDuration, Format(ap.InTime,'yyyy-MMM-dd HH:mm:ss')InTime,
				Format(ap.OutTime,'yyyy-MMM-dd HH:mm:ss')OutTime,
				Format(ap.ShiftInTime,'yyyy-MMM-dd HH:mm:ss')ShiftInTime, 
                Format(ap.ShiftOutTime,'yyyy-MMM-dd HH:mm:ss')ShiftOutTime, 
                sd.ShiftEarlyInMargin,sd.ShiftEarlyOutMargin,sd.ShiftLateInMargin,
                sd.ShiftLateOutMargin
                from Attdnprocessdata  ap
                left join ShiftDefination sd on sd.SystemID=ap.ShiftSystemID
                where workdate='" + PreDay + @"' and ap.PlantID='" + Plant + @"' 
                and isnull(ap.InTime,'')!='' and isnull(ap.OutTime,'')!='') as dd
				where dd.CalDuration>0";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
        public void PlantLockCheck(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string Today = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId='" + Plant + @"'
                and LockedDate='" + Today + "' and IsActive='1'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
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
                ConnectionManager.DAL.ConManager objCon;

                var sql = @"select ap.EmpSystemID,Format(ap.WorkDate,'yyyy-MMM-dd')WorkDate,
                ap.Duration,ap.ShiftSystemID,
                (ap.Duration-isnull(ap.ShiftHoursWithoutOT,'0'))OverUnderStay
                from attdnprocessdata ap
                where WorkDate='" + PreDay + "' and Duration >0 and ap.PlantID='" + Plant + "'";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }

            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void OverUnderStaySameDay(string SameDay, out DataSet ds, string Plant)
        {

            try
            {
                ConnectionManager.DAL.ConManager objCon;

                var sql = @"select ap.EmpSystemID,Format(ap.WorkDate,'yyyy-MMM-dd')WorkDate,
                ap.Duration,ap.ShiftSystemID,
                (ap.Duration-isnull(ap.ShiftHoursWithoutOT,'0'))OverUnderStay
                from attdnprocessdata ap
                where WorkDate='" + SameDay + "' and Duration >0 and ap.PlantID='" + Plant + "'";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }

            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PrevDurationStatusCal(string PreviousDay, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select p.EmpSystemID,Format(p.WorkDate,'yyyy-MMM-dd')WorkDate,p.Duration
                ,p.ShiftHalfDayDuration,p.ShiftFullDayDuration,p.InTime,p.OutTime,
                p.ShiftShortDuration from AttdnProcessData p 
                where WorkDate='" + PreviousDay + @"' 
                and p.PlantID='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
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
        public void TodayStatusCodeData(string Today, string Plant)
        {

            try
            {
                var sql = @"UPDATE AttdnProcessData Set DayStatusCode=(ISNULL(HolidayStatus,'')+	
											ISNULL(WeeklyStatus,'')+ISNULL(DurationStatus,'')+
								ISNULL(EarlyLateIn,'')+ISNULL(EarlyLateOut,'')
								+ISNULL(LeaveStatus,'')),DateUpdated=GETDATE() 
                        WHERE PlantID='"+Plant+@"'
								AND WorkDate='"+Today+@"' and isnull(intime,'')!=''
								and ISNULL(outtime,'')!=''";

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
        public void PrevDayStatus(string PreDay, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.EmpSystemID,p.DayStatusCode,dt.DayType,
                        format(p.WorkDate,'yyyy-MMM-dd')WorkDate from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                                  left join mst.DesignationMasterLegalDesignation ddm on 
                        ddm.LegalDesignationId = ei.LegalDesignationId
                                            left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
									        left join DayStatusPlantChild dc on dc.EmpTypeId=dm.EmployeeCategoryId
											and dc.PlantId=ei.PlantId
						                    left join DayStatusHeader dh on dh.Id=dc.headerId
									        left join DayStatus ds on ds.headerId=dh.Id
											left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId
									        where WorkDate='" + PreDay + @"' and ds.Code=p.DayStatusCode
									        and ei.PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PreProcessFinalDayStatus(string PreDay, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.EmpSystemID,Result=dt.DayType, dt.AutoLock,
                dt.SandwichStatusFlag,dt.OTApplicable,dt.GoodWorkApplicable,format(p.WorkDate,'yyyy-MMM-dd')WorkDate from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                        left join mst.DesignationMasterLegalDesignation ddm on 
                        ddm.LegalDesignationId = ei.LegalDesignationId
                        left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
						left join DayStatusPlantChild dc on dc.EmpTypeId=dm.EmployeeCategoryId
						and dc.PlantId=ei.PlantId
						left join DayStatusHeader dh on dh.Id=dc.headerId
						left join DayStatus ds on ds.headerId=dh.Id
						left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId									       
						where WorkDate='" + PreDay + @"' 
						and dt.DayType=ISNULL(p.ManualDayStatus,p.ProcessDayStatus)
						and ei.PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
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
                var sql = @"UPDATE 	AttdnProcessData Set DayStatus= isnull(Sandwichstatus,ProcessFinalDayStatus)
				,UpdatedBy='Schedule',DateUpdated=GETDATE()
								WHERE PlantID='" + Plant + @"'
								AND WorkDate='" + PreDay + "'";

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
        public void PrevDayOTCalculation(string PreDay, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.EmpSystemID,
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

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
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
        public void MonthlySummarySource(string Date, out DataSet ds)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sql = @"select dd.*,Month(dd.FromDate)Month,YEAR(dd.FromDate)Year from (select distinct p.EmpSystemID,MIN(p.WorkDate) FromDate,
                MAX(p.WorkDate) ToDate,(select PlantID
                from EmployeeInformation where SystemId=p.EmpSystemID)PlantId,(select GroupID
                from EmployeeInformation where SystemId=p.EmpSystemID)GroupID,
                COUNT(p.WorkDate) TotalProcDate,
                SUM(dt.PresentValuePD)TotalPresent,SUM(dt.LateValueLV)TotalLate,SUM(dt.AbsentValueAB)TotalAbsent,
                SUM(dt.LeaveValueLP)TotalLv,SUM(dt.MaternityLeaveValueMLV)TotalMlv,SUM(dt.CompAssignLv)TotalCompAssignLv,
                SUM(dt.WeeklyOffWO)TotalWeekOff,SUM(dt.HolidayH)TotalHoliDay,SUM(dt.WeekOffHoliDayWOH)TotalWeekOffHoliDay,
                SUM(ISNULL(p.OTHr, 0)) TotalOTHr,SUM(dt.LeaveValueLWP)TotalLWP,SUM(dt.CasualLeaveValueCV)TotalCasualLeave,
                SUM(dt.PriviledgeLeavePL)TotalPriviledgeLeave,SUM(dt.MedicalLeaveValueMV)TotalMedicalLeave,SUM(dt.TotalWorkingDay)TotalWorkingDay,
				SUM(dt.ActualWorkingDay)ActualWorkingDay,SUM(dt.PayDay)TotalPayDay,SUM(dt.NonPayDay)TotalNonPayDay
                        from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                        left join mst.DesignationMasterLegalDesignation ddm on
                        ddm.LegalDesignationId = ei.LegalDesignationId
                        left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
                        left join DayStatusPlantChild dc on dc.EmpTypeId=dm.EmployeeCategoryId
                        and dc.PlantId=ei.PlantId
                        left join DayStatusHeader dh on dh.Id=dc.headerId
                        left join DayTypeWithValues dt on dt.HeaderId=dh.Id                                          
                        where dt.DayType=p.DayStatus AND  isnull(p.DayStatus,'')!='' and		
                        MONTH(WorkDate) = MONTH('"+Date+@"') AND 
						YEAR(WorkDate) = YEAR('"+Date+@"')                       					
                        GROUP BY EmpSystemID) as dd";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void EarnedLeaveCalculation(string PreDay, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.RowId,dt.DayType, dt.EarnedPL,dt.EarnedCL,
                        format(p.WorkDate,'yyyy-MMM-dd')WorkDate from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                        left join mst.DesignationMasterLegalDesignation ddm on 
                        ddm.LegalDesignationId = ei.LegalDesignationId
                        left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
						left join DayStatusPlantChild dc on dc.EmpTypeId=dm.EmployeeCategoryId
						and dc.PlantId=ei.PlantId
						left join DayStatusHeader dh on dh.Id=dc.headerId
						left join DayStatus ds on ds.headerId=dh.Id
						left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId									       
						where WorkDate='"+PreDay+ @"' 
						and dt.DayType=p.DayStatus and (dt.EarnedCL>0 or dt.EarnedPL>0)
						and ei.PlantId='" + Plant+"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void TodayDurationStatusCal(string Today, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select p.EmpSystemID,Format(p.WorkDate,'yyyy-MMM-dd')WorkDate,p.Duration
                ,p.ShiftHalfDayDuration,p.ShiftFullDayDuration,p.InTime,p.OutTime,
                p.ShiftShortDuration from AttdnProcessData p 
                where WorkDate='"+Today+@"' 
                and p.PlantID='"+Plant+"' and ISNULL(intime,'')!='' and ISNULL(outtime,'')!=''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        #region OT DayLimit Process SourceData
        public void OTDayLimitRowCreation(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sql = @"select RowId,EmpSystemID,WorkDate,PlantID,GroupID from AttdnProcessData where IsOTEntitled='1' 
                and WorkDate='" + Date + "' and PlantID='" + PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void DayTypeforOTProcess(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select dd.* from (select RowId,PlantID,'"+Date+@"' as WorkDate,
                DayType=ISNULL(HolidayStatus,WeeklyStatus)
                from AttdnProcessData where WorkDate='"+Date+@"' and IsOTEntitled='1'
                and PlantID='"+Plant+"' and isnull(daystatus,'')!='') as dd where isnull(dd.DayType,'')!=''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PreallocatedOTSource(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select EmpSystemID,WorkDate,PreallocatedOTHr*60 as 
                PreAllocatedOTMinutes,PlantID
                from [dbo].[PreallocatedOT] where WorkDate='" + Date + @"'
                and PlantID='" + PlantId + "' and ISNULL(ExtendTheDayLimit,'')! =''";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void FixedOTSettingSource(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"Select EmpSystemId,'" + Date + @"' AS WorkDate,PlantId,
                MaximumOTLimitPerWeekend*60 as WeekOffOT,MaximumOTLimitPerHoliDay*60 AS HolidayOT,
                MaximumOTLimitPerWeekDay*60 AS NormalDayOT
                from EmployeeWiseFixedOTSetting 
                WHERE PlantId='" + PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void SlabOTSource(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select O.RowId,o.PlantID,o.EmpSystemID,
                Format(o.WorkDate,'yyyy-MMM-dd')WorkDate,s.firstSlab*60 as firstSlab
                from OTProcessDayLimit o 
                left join org.Plant p on o.PlantID=p.Id left join
                OTSlabDefineGeneral s on s.PlantID=p.Id and s.DayType=o.DayType
                where p.Id='" + PlantId+"' and o.WorkDate='"+Date+"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void WeekLimitOTSource(string Date, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"DECLARE @MyDate DATETIME = '" + Date + @"';
                declare @WeekNo varchar(10) = '';
                Set @WeekNo = (SELECT DATEDIFF(WEEK, DATEADD(MONTH, DATEDIFF(MONTH, 0, @MyDate), 0), @MyDate) + 1); 

               
                IF @WeekNo='1'
                begin
                select o.RowId,o.PlantID,'"+Date+ @"' as WorkDate,MaxHolidayOTLimitParDay as HolidayOT,
                MaxWeekOffOTLimitParDay as WeekOffOT,MaxOTLimitParDay as NormalDayOT from OTProcessDayLimit O 
                left join org.Plant p 
                on p.Id=o.PlantID left join 
                OTLimitSetting ol on ol.PlantID=p.Id
                where o.PlantID='" + PlantId+ @"' AND ol.UserName='OT Time Setting (W-1)'
                end

                Else IF @WeekNo='2'
                begin
                select o.RowId,o.PlantID,'" + Date + @"' as WorkDate,MaxHolidayOTLimitParDay as HolidayOT,
                MaxWeekOffOTLimitParDay as WeekOffOT,MaxOTLimitParDay as NormalDayOT from OTProcessDayLimit O 
                left join org.Plant p 
                on p.Id=o.PlantID left join 
                OTLimitSetting ol on ol.PlantID=p.Id
                where o.PlantID='" + PlantId + @"' AND ol.UserName='OT Time Setting (W-2)'
                end

                Else IF @WeekNo='3'
                begin
                select o.RowId,o.PlantID,'" + Date + @"' as WorkDate,MaxHolidayOTLimitParDay as HolidayOT,
                MaxWeekOffOTLimitParDay as WeekOffOT,MaxOTLimitParDay as NormalDayOT from OTProcessDayLimit O 
                left join org.Plant p 
                on p.Id=o.PlantID left join 
                OTLimitSetting ol on ol.PlantID=p.Id
                where o.PlantID='" + PlantId + @"' AND ol.UserName='OT Time Setting (W-3)'
                end

                else
                begin
                select o.RowId,o.PlantID,'" + Date + @"' as WorkDate,MaxHolidayOTLimitParDay as HolidayOT,
                MaxWeekOffOTLimitParDay as WeekOffOT,MaxOTLimitParDay as NormalDayOT from OTProcessDayLimit O 
                left join org.Plant p 
                on p.Id=o.PlantID left join 
                OTLimitSetting ol on ol.PlantID=p.Id
                where o.PlantID='" + PlantId + @"' AND ol.UserName='OT Time Setting (W-4)'
                end";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        #endregion

        #region DayStatus Process
        public void DayStatus(string Date, string PlantValue)
        {
            try
            {

                Date = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");
                string PreviousDay = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");
                string SandwichPrevDay = Convert.ToDateTime(Date).AddDays(-2).ToString("dd-MMM-yyyy");

                DataSet PlantLock;
                PlantLockCheck(PreviousDay, out PlantLock, PlantValue);
                if (PlantLock.Tables[0].Rows.Count > 0)
                {

                }
                else
                {
                   
                    #region Previous Day Duration EarlyIn Late EarlyOut OverStay
                    DataSet PrevDurn;
                    PrevDayDuration(PreviousDay, out PrevDurn, PlantValue);
                    if (PrevDurn.Tables[0].Rows.Count > 0)
                    { 
                        string WorkDate = PrevDurn.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "'and isnull(InTime,'')!='' and isnull(OutTime,'')!='' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < PrevDurn.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = PrevDurn.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                            string ProcessInTime = clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"InTime"]).ToString();
                            string ProcessOutTime = clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"OutTime"]).ToString();
                            string ShiftOutTime = clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftOutTime"]).ToString();
                            string ShiftInTime = clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftInTime"]).ToString();
                            string CalDuration = clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"CalDuration"]).ToString();
                            double ShiftEarlyInMargin = Convert.ToDouble(clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftEarlyInMargin"]).ToString());
                            double ShiftLateInMargin = Convert.ToDouble(clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftLateInMargin"]).ToString());
                            double ShiftEarlyOutMargin = Convert.ToDouble(clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftEarlyOutMargin"]).ToString());
                            double ShiftLateOutMargin = Convert.ToDouble(clsWebLib.RetValidLen(PrevDurn.Tables[0].Rows[i][@"ShiftLateOutMargin"]).ToString());

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["Duration"] = CalDuration;
                                dr["EarlyLateIn"] = DBNull.Value;
                                dr["EarlyLateOut"] = DBNull.Value;
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
                            }
                        }
                        SaveDataSets(dsRef);

                    }

                    #endregion

                    #region PrevDay OverStay UnderStay 
                    DataSet PrevDayOT;
                    OverUnderStayPrevDay(PreviousDay, out PrevDayOT, PlantValue);
                    if (PrevDayOT.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = PrevDayOT.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "'and Duration >0 and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < PrevDayOT.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = PrevDayOT.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                            double OverUnderStay = Convert.ToDouble(clsWebLib.RetValidLen(PrevDayOT.Tables[0].Rows[i][@"OverUnderStay"]).ToString());

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                if (OverUnderStay > 0)
                                {
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

                    #region Previous Day DurationStatus Flagging
                    DataSet PrevDurationStat;
                    PrevDurationStatusCal(PreviousDay, out PrevDurationStat, PlantValue);
                    if (PrevDurationStat.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = PrevDurationStat.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < PrevDurationStat.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = PrevDurationStat.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                            string ShortDuration = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"ShiftShortDuration"]).ToString();
                            string FullDayDuration = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"ShiftFullDayDuration"]).ToString();
                            string HalfDayDuration = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"ShiftHalfDayDuration"]).ToString();
                            string Duration = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"Duration"]).ToString();
                            string In = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"InTime"]).ToString();
                            string Out = clsWebLib.RetValidLen(PrevDurationStat.Tables[0].Rows[i][@"OutTime"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                if (Duration.ToString() != "" &&
                                    FullDayDuration.ToString() != ""
                                    && ShortDuration.ToString() != ""
                                    && HalfDayDuration.ToString() != "")
                                {
                                    if (Convert.ToDouble(Duration) >= Convert.ToDouble(FullDayDuration))
                                    {
                                        dr["DurationStatus"] = "FD";
                                    }
                                    else if (Convert.ToDouble(Duration) >= Convert.ToDouble(HalfDayDuration))
                                    {
                                        dr["DurationStatus"] = "HD";
                                    }
                                    else if (Convert.ToDouble(Duration) >= Convert.ToDouble(ShortDuration))
                                    {
                                        dr["DurationStatus"] = "SD";
                                    }
                                    else if (Convert.ToDouble(Duration) < Convert.ToDouble(ShortDuration))
                                    {
                                        dr["DurationStatus"] = "A";
                                    }
                                }
                                else
                                {
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

                    #region Previous Day Status Code              
                    PrevDayStatusCodeData(PreviousDay, PlantValue);
                    #endregion

                    #region Prev User Day Status 
                    DataSet PrevUserDayStat;
                    PrevDayStatus(PreviousDay, out PrevUserDayStat, PlantValue);
                    if (PrevUserDayStat.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = PrevUserDayStat.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < PrevUserDayStat.Tables[0].Rows.Count; i++)
                        {

                            string EmpId = clsWebLib.RetValidLen(PrevUserDayStat.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                            string DayStatus = clsWebLib.RetValidLen(PrevUserDayStat.Tables[0].Rows[i][@"DayType"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                dr["ProcessDayStatus"] = DayStatus;
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region Prev Process FinalDayStatus 
                    DataSet PrevFinalDayStat; // Process DayStatus & Manual DayStatus Comparison
                    PreProcessFinalDayStatus(PreviousDay, out PrevFinalDayStat, PlantValue);
                    if (PrevFinalDayStat.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = PrevFinalDayStat.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < PrevFinalDayStat.Tables[0].Rows.Count; i++)
                        {

                            string EmpId = clsWebLib.RetValidLen(PrevFinalDayStat.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                            string Result = clsWebLib.RetValidLen(PrevFinalDayStat.Tables[0].Rows[i][@"Result"]).ToString();
                            string SandwichFlag = clsWebLib.RetValidLen(PrevFinalDayStat.Tables[0].Rows[i][@"SandwichStatusFlag"]).ToString();
                            string OtApplicable = clsWebLib.RetValidLen(PrevFinalDayStat.Tables[0].Rows[i][@"OTApplicable"]).ToString();
                            string Goodwork = clsWebLib.RetValidLen(PrevFinalDayStat.Tables[0].Rows[i][@"GoodWorkApplicable"]).ToString();
                            string AutoLock = clsWebLib.GetBoolData(PrevFinalDayStat.Tables[0].Rows[i][@"AutoLock"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                dr["ProcessFinalDayStatus"] = Result;
                                dr["SandwichFlag"] = SandwichFlag;
                                dr["DayTypeOTApplicable"] = OtApplicable;
                                dr["DayTypeGoodWorkApplicable"] = Goodwork;
                                if (AutoLock == "True")
                                {
                                    dr["IsLock"] = true;
                                    dr["LockedDate"] = DateTime.Now;
                                    dr["LockedBy"] = "AutoLock";
                                }
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region Sandwich Logic 
                    DataSet SandwichData;
                    SandwichLogic(SandwichPrevDay, out SandwichData, PlantValue);
                    if (SandwichData.Tables[0].Rows.Count > 0)
                    {

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + PreviousDay + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");
                        string newformat = Convert.ToDateTime(PreviousDay).ToString("yyyyMMdd");


                        for (int i = 0; i < SandwichData.Tables[0].Rows.Count; i++)
                        {

                            string EmpId = clsWebLib.RetValidLen(SandwichData.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                            string PrevDaySandwich = clsWebLib.RetValidLen(SandwichData.Tables[0].Rows[i][@"SandwichFlag"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                string ToDaySandwich = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"SandwichFlag"]).ToString();
                                string FinalStatus = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"ProcessFinalDayStatus"]).ToString();

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                if (PrevDaySandwich == "0" && ToDaySandwich == "2")
                                {
                                    dr["SandwichFlag"] = "0"; //Today
                                }

                                else if (PrevDaySandwich == "1" && ToDaySandwich == "2")
                                {
                                    dr["SandwichFlag"] = "2"; //Today
                                }
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();


                                if (PrevDaySandwich == "2")
                                {
                                    if (ToDaySandwich == "1")
                                    {
                                        if (FinalStatus != "")
                                        {
                                            var sql = @"update AttdnProcessData set UpdatedBy='Sandwich', DayStatus='" + FinalStatus + "',Sandwichstatus='" + FinalStatus + "'" +
                                                "where WorkDate <= '" + SandwichPrevDay + "' and EmpSystemID='" + EmpId + "' " +
                                                "and SandwichFlag!=1 and SandwichFlag!=0";

                                            ConnectionManager.DAL.ConManager objCone = null;
                                            objCone = new ConnectionManager.DAL.ConManager("1");
                                            objCone.OpenConnection("1");
                                            objCone.BeginTransaction();

                                            objCone.ExecuteNonQueryWrapper(sql, true, "1");
                                            objCone.CommitTransaction();
                                        }
                                    }
                                    else if (ToDaySandwich == "0")
                                    {
                                        var sql = @"update AttdnProcessData set UpdatedBy='Sandwich',SandwichFlag='0' 
                                        where WorkDate <= '" + SandwichPrevDay + "' and EmpSystemID='" + EmpId + "' " +
                                        "and SandwichFlag!=1 and SandwichFlag!=0";

                                        ConnectionManager.DAL.ConManager objCone = null;
                                        objCone = new ConnectionManager.DAL.ConManager("1");
                                        objCone.OpenConnection("1");
                                        objCone.BeginTransaction();

                                        objCone.ExecuteNonQueryWrapper(sql, true, "1");
                                        objCone.CommitTransaction();
                                    }
                                }

                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region Previous Payroll DayStatus 
                    PrePayrollDayStatus(PreviousDay, PlantValue);
                    #endregion       
                  
                    #region Prev DayOT Calculation 
                    DataSet PrevOTCalculate;
                    PrevDayOTCalculation(PreviousDay, out PrevOTCalculate, PlantValue);
                    if (PrevOTCalculate.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = PrevOTCalculate.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where IsOTEntitled='1' and WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";
                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        var sqly = @"select * from PlantWiseHRMSSetting where PlantID='"+PlantValue+"'";
                        objCon.OpenDataSetThroughAdapter(sqly, out DataSet OTMode, false, false, "", "1");

                        string OTModeValue = clsWebLib.RetValidLen(OTMode.Tables[0].Rows[0][@"ResultendOT"]).ToString();
                        // 0 means Punched Based 1 means Manual 2 means Mixed


                        for (int i = 0; i < PrevOTCalculate.Tables[0].Rows.Count; i++)
                        {

                            string EmpId = clsWebLib.RetValidLen(PrevOTCalculate.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                            string Result = clsWebLib.RetValidLen(PrevOTCalculate.Tables[0].Rows[i][@"Result"]).ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                string PastManualOT = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"ManualOt"]).ToString();
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;

                                if (OTModeValue == "0")
                                {
                                    if (Result != "")
                                    {
                                        if (Convert.ToDouble(Result) > 0)
                                        {
                                            dr.BeginEdit();
                                            dr["ProcessedOT"] = Result;
                                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                            dr.EndEdit();
                                        }
                                    }

                                }
                                else if (OTModeValue == "1")
                                {
                                    if (PastManualOT != "")
                                    {
                                        if (Convert.ToDouble(PastManualOT) > 0)
                                        {
                                            dr.BeginEdit();
                                            dr["ProcessedOT"] = PastManualOT;
                                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                            dr.EndEdit();
                                        }
                                    }
                                }
                                else
                                {
                                    if (Result != "")
                                    {
                                        if (PastManualOT != "")
                                        {
                                            if (Convert.ToDouble(PastManualOT) > 0)
                                            {
                                                if (Convert.ToDouble(PastManualOT) < Convert.ToDouble(Result))
                                                {
                                                    dr.BeginEdit();
                                                    dr["ProcessedOT"] = PastManualOT;
                                                    dr.EndEdit();
                                                }
                                            }
                                            else
                                            {
                                                dr.BeginEdit();
                                                dr["ProcessedOT"] = Result;
                                                dr.EndEdit();
                                            }                                            
                                        }
                                        else
                                        {
                                            dr.BeginEdit();
                                            dr["ProcessedOT"] = Result;
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

                    #region PrevDay EarnedLeave Processing 
                    DataSet PrevEarnedLeave;
                    EarnedLeaveCalculation(PreviousDay, out PrevEarnedLeave, PlantValue);
                    if (PrevEarnedLeave.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = PrevEarnedLeave.Tables[0].Rows[0][@"WorkDate"].ToString();
                        
                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from LeaveEarned where WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                        for (int i = 0; i < PrevEarnedLeave.Tables[0].Rows.Count; i++)
                        {

                            string RowId = clsWebLib.RetValidLen(PrevEarnedLeave.Tables[0].Rows[i][@"RowId"]).ToString();
                            string DayType = clsWebLib.RetValidLen(PrevEarnedLeave.Tables[0].Rows[i][@"DayType"]).ToString();
                            string EarnedPL = clsWebLib.RetValidLen(PrevEarnedLeave.Tables[0].Rows[i][@"EarnedPL"]).ToString();
                            string EarnedCL = clsWebLib.RetValidLen(PrevEarnedLeave.Tables[0].Rows[i][@"EarnedCL"]).ToString();
                           
                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId+ "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                dr["DayStatus"] = DayType;
                                if (EarnedCL != "")
                                {
                                    dr["EarnedCasualLeave"] = EarnedCL;
                                }
                                if (EarnedPL != "")
                                {
                                    dr["EarnedPriviledgeLeave"] = EarnedPL;
                                }
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr["UpdatedBy"] = "DayStatusProcess";
                                dr.EndEdit();
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
                        var WkDate = PrevManualOT.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where IsOTEntitled='1' and WorkDate='" + WkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


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
                                    if (ManualOT != "")
                                    {
                                        if (Convert.ToDouble(ManualOT) > 0)
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
                                    if (ManualOT != "")
                                    {
                                        if (Convert.ToDouble(ManualOT) > 0)
                                        {
                                            if (Convert.ToDouble(ManualOT) < Convert.ToDouble(AutoOT))
                                            {

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

                    #region Today Duration EarlyIn Late EarlyOut OverStay
                    DataSet TodayDurn;
                    TodayDuration(Date, out TodayDurn, PlantValue);
                    if (TodayDurn.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = TodayDurn.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "'and isnull(InTime,'')!='' and isnull(OutTime,'')!='' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < TodayDurn.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = clsWebLib.RetValidLen(TodayDurn.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                            string ProcessInTime = clsWebLib.RetValidLen(TodayDurn.Tables[0].Rows[i][@"InTime"]).ToString();
                            string ProcessOutTime = clsWebLib.RetValidLen(TodayDurn.Tables[0].Rows[i][@"OutTime"]).ToString();
                            string ShiftOutTime = clsWebLib.RetValidLen(TodayDurn.Tables[0].Rows[i][@"ShiftOutTime"]).ToString();
                            string ShiftInTime = clsWebLib.RetValidLen(TodayDurn.Tables[0].Rows[i][@"ShiftInTime"]).ToString();
                            string CalDuration = clsWebLib.RetValidLen(TodayDurn.Tables[0].Rows[i][@"CalDuration"]).ToString();
                            double ShiftEarlyInMargin = Convert.ToDouble(clsWebLib.RetValidLen(TodayDurn.Tables[0].Rows[i][@"ShiftEarlyInMargin"]).ToString());
                            double ShiftLateInMargin = Convert.ToDouble(clsWebLib.RetValidLen(TodayDurn.Tables[0].Rows[i][@"ShiftLateInMargin"]).ToString());
                            double ShiftEarlyOutMargin = Convert.ToDouble(clsWebLib.RetValidLen(TodayDurn.Tables[0].Rows[i][@"ShiftEarlyOutMargin"]).ToString());
                            double ShiftLateOutMargin = Convert.ToDouble(clsWebLib.RetValidLen(TodayDurn.Tables[0].Rows[i][@"ShiftLateOutMargin"]).ToString());

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                dr["Duration"] = CalDuration;
                                dr["EarlyLateIn"] = DBNull.Value;
                                dr["EarlyLateOut"] = DBNull.Value;
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
                            }
                        }
                        SaveDataSets(dsRef);

                    }

                    #endregion

                    #region SameDay OverStay UnderStay 
                    DataSet SameDayOverStay;
                    OverUnderStaySameDay(Date, out SameDayOverStay, PlantValue);
                    if (SameDayOverStay.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = SameDayOverStay.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "'and Duration >0 and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < SameDayOverStay.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = SameDayOverStay.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                            double OverUnderStay = Convert.ToDouble(clsWebLib.RetValidLen(SameDayOverStay.Tables[0].Rows[i][@"OverUnderStay"]).ToString());

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {

                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                if (OverUnderStay > 0)
                                {
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

                    #region Today DurationStatus Flagging
                    DataSet TodayDurationStat;
                    TodayDurationStatusCal(Date, out TodayDurationStat, PlantValue);
                    if (TodayDurationStat.Tables[0].Rows.Count > 0)
                    {
                        string WorkDate = TodayDurationStat.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where WorkDate='" + WorkDate + "' and PlantID='" + PlantValue + "'";

                        objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < TodayDurationStat.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = TodayDurationStat.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                            string ShortDuration = clsWebLib.RetValidLen(TodayDurationStat.Tables[0].Rows[i][@"ShiftShortDuration"]).ToString();
                            string FullDayDuration = clsWebLib.RetValidLen(TodayDurationStat.Tables[0].Rows[i][@"ShiftFullDayDuration"]).ToString();
                            string HalfDayDuration = clsWebLib.RetValidLen(TodayDurationStat.Tables[0].Rows[i][@"ShiftHalfDayDuration"]).ToString();
                            string Duration = clsWebLib.RetValidLen(TodayDurationStat.Tables[0].Rows[i][@"Duration"]).ToString();
                            
                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                if (Duration.ToString() != "" &&
                                    FullDayDuration.ToString() != ""
                                    && ShortDuration.ToString() != ""
                                    && HalfDayDuration.ToString() != "")
                                {
                                    if (Convert.ToDouble(Duration) >= Convert.ToDouble(FullDayDuration))
                                    {
                                        dr["DurationStatus"] = "FD";
                                    }
                                    else if (Convert.ToDouble(Duration) >= Convert.ToDouble(HalfDayDuration))
                                    {
                                        dr["DurationStatus"] = "HD";
                                    }
                                    else if (Convert.ToDouble(Duration) >= Convert.ToDouble(ShortDuration))
                                    {
                                        dr["DurationStatus"] = "SD";
                                    }
                                    else if (Convert.ToDouble(Duration) < Convert.ToDouble(ShortDuration))
                                    {
                                        dr["DurationStatus"] = "A";
                                    }
                                }                                

                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();

                            }
                        }
                        SaveDataSets(dsRef);

                    }

                    #endregion
                    
                    #region Today Status Code              
                    TodayStatusCodeData(Date, PlantValue);
                    #endregion
                    
                    #region DayLimitProcess 

                    #region DayType Updation

                    DataSet DaytypeLimitOT;
                    DayTypeforOTProcess(PreviousDay, out DaytypeLimitOT, PlantValue);
                    if (DaytypeLimitOT.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = DaytypeLimitOT.Tables[0].Rows[0][@"WorkDate"].ToString();
                        var PlantId = DaytypeLimitOT.Tables[0].Rows[0][@"PlantID"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter("select * from OTProcessDayLimit where WorkDate='" + WkDate + "'and PlantID='" + PlantId + "'", out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < DaytypeLimitOT.Tables[0].Rows.Count; i++)
                        {
                            string RowId = DaytypeLimitOT.Tables[0].Rows[i][@"RowId"].ToString();
                            string DayType = DaytypeLimitOT.Tables[0].Rows[i][@"DayType"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                dr["DayType"] = DayType;
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr["UpdatedBy"] = "Schedule";
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region Planned OT Flagging 
                    DataSet PreallocatedOT;
                    PreallocatedOTSource(PreviousDay, out PreallocatedOT, PlantValue);
                    if (PreallocatedOT.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = PreallocatedOT.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");
                        var PlantId = PreallocatedOT.Tables[0].Rows[0][@"PlantID"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter("select * from OTProcessDayLimit where WorkDate='" + WkDate + "'and PlantID='" + PlantId + "'and isnull(DayType,'')!=''", out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < PreallocatedOT.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = PreallocatedOT.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                            string OTMinutes = PreallocatedOT.Tables[0].Rows[i][@"PreAllocatedOTMinutes"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                dr["PlannedOT"] = OTMinutes;
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region EmployeeWise FixedOTSetting 
                    DataSet FixedOTSetting;
                    FixedOTSettingSource(PreviousDay,out FixedOTSetting, PlantValue);
                    if (FixedOTSetting.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = FixedOTSetting.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");
                        var PlantId = FixedOTSetting.Tables[0].Rows[0][@"PlantId"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter("select * from OTProcessDayLimit where WorkDate='" + WkDate + "'and PlantID='" + PlantId + "' and isnull(DayType,'')!=''", out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < FixedOTSetting.Tables[0].Rows.Count; i++)
                        {
                            string EmpId = FixedOTSetting.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                            string WeekOffOT = FixedOTSetting.Tables[0].Rows[i][@"WeekOffOT"].ToString();
                            string NormalDayOT = FixedOTSetting.Tables[0].Rows[i][@"NormalDayOT"].ToString();
                            string HolidayOT = FixedOTSetting.Tables[0].Rows[i][@"HolidayOT"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                string DayType = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"DayType"]).ToString();
                                if (DayType != "")
                                {
                                    dr.BeginEdit();
                                    if(DayType=="H")
                                    {
                                        dr["FixedOT"] = HolidayOT;
                                    }
                                    else if(DayType=="W")
                                    {
                                        dr["FixedOT"] = WeekOffOT;
                                    }
                                    else
                                    {
                                        dr["FixedOT"] = NormalDayOT;
                                    }
                                   
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();
                                }
                            }
                                
                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region WeeklyOT Entry
                    DataSet WeekOTSource;
                    WeekLimitOTSource(PreviousDay, out WeekOTSource, PlantValue);
                    if (WeekOTSource.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = WeekOTSource.Tables[0].Rows[0][@"WorkDate"].ToString();
                        var PlantId = WeekOTSource.Tables[0].Rows[0][@"PlantID"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter("select * from OTProcessDayLimit where WorkDate='" + WkDate + "'and PlantID='" + PlantId + "' and isnull(DayType,'')!=''", out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < WeekOTSource.Tables[0].Rows.Count; i++)
                        {
                            string RowId = WeekOTSource.Tables[0].Rows[i][@"RowId"].ToString();
                            string WeekOffOT = WeekOTSource.Tables[0].Rows[i][@"WeekOffOT"].ToString();
                            string NormalDayOT = WeekOTSource.Tables[0].Rows[i][@"NormalDayOT"].ToString();
                            string HolidayOT = WeekOTSource.Tables[0].Rows[i][@"HolidayOT"].ToString();

                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                string DayType = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"DayType"]).ToString();
                                if (DayType != "")
                                {
                                    dr.BeginEdit();
                                    if (DayType == "H")
                                    {
                                        dr["LimitSettingOT"] = HolidayOT;
                                    }
                                    else if (DayType == "W")
                                    {
                                        dr["LimitSettingOT"] = WeekOffOT;
                                    }
                                    else
                                    {
                                        dr["LimitSettingOT"] = NormalDayOT;
                                    }

                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();
                                }
                            }

                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion

                    #region SlabOT Entry
                    DataSet SlabOT;
                    SlabOTSource(PreviousDay, out SlabOT, PlantValue);
                    if (SlabOT.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = SlabOT.Tables[0].Rows[0][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");
                        var PlantId = SlabOT.Tables[0].Rows[0][@"PlantId"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter("select * from OTProcessDayLimit where WorkDate='" + WkDate + "'and PlantID='" + PlantId + "' and isnull(DayType,'')!=''", out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < SlabOT.Tables[0].Rows.Count; i++)
                        {
                            string RowId = SlabOT.Tables[0].Rows[i][@"RowId"].ToString();
                            string firstSlab = SlabOT.Tables[0].Rows[i][@"firstSlab"].ToString();
                         
                            dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId +"' ";
                            if (dsRef.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                string DayType = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"DayType"]).ToString();
                                if (DayType != "")
                                {
                                    dr.BeginEdit();                                    
                                    dr["SlabOT"] = firstSlab;                    
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();
                                }
                            }

                        }
                        SaveDataSets(dsRef);

                    }
                    #endregion
                    
                    #endregion


                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion

        #region ManualScheduler Source Data

        public void ManualInOut(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select EmpsystemId,Format(WorkDate,'yyyy-MMM-dd')WorkDate,
				 InTime=ISNULL(ManualInTime,PunchInTime),OutTime=
				 ISNULL(ManualOutTime,PunchOutTime) from  AttdnProcessData
				 WHERE ManualFlag=1 and PlantID='" + Plant + "'";

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
                datediff(minute,ap.InTime,ap.OutTime) 
                as CalDuration, Format(ap.InTime,'yyyy-MMM-dd HH:mm:ss')InTime,
				Format(ap.OutTime,'yyyy-MMM-dd HH:mm:ss')OutTime,
				Format(ap.ShiftInTime,'yyyy-MMM-dd HH:mm:ss')ShiftInTime, 
                Format(ap.ShiftOutTime,'yyyy-MMM-dd HH:mm:ss')ShiftOutTime, 
                sd.ShiftEarlyInMargin,sd.ShiftEarlyOutMargin,sd.ShiftLateInMargin,
                sd.ShiftLateOutMargin
                from Attdnprocessdata  ap
                left join ShiftDefination sd on sd.SystemID=ap.ShiftSystemID
                where ap.ManualFlag=1 and ap.PlantID='" + Plant + @"' and isnull(ap.InTime,'')!='' 
				and isnull(ap.OutTime,'')!='') as dd
				where dd.CalDuration>0";

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
                ap.Duration,ap.ShiftSystemID,
                (ap.Duration-isnull(ap.ShiftHoursWithoutOT,'0'))OverUnderStay
                from attdnprocessdata ap
                where Duration >0 and ap.PlantID='" + Plant + @"'
				AND ManualFlag=1";

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
                ,p.ShiftHalfDayDuration,p.ShiftFullDayDuration,p.InTime,p.OutTime,
                p.ShiftShortDuration from AttdnProcessData p 
                where ManualFlag=1 
                and p.PlantID='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ManualDayStatusCodeData(string Plant)
        {

            try
            {
                var sql = @"UPDATE AttdnProcessData Set DayStatusCode=(ISNULL(HolidayStatus,'')+	
											ISNULL(WeeklyStatus,'')+ISNULL(DurationStatus,'')+
								ISNULL(EarlyLateIn,'')+ISNULL(EarlyLateOut,'')
								+ISNULL(LeaveStatus,'')),DateUpdated=GETDATE() 
                        WHERE PlantID='" + Plant + @"'
								AND ManualFlag=1 and IsLock=0";

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
                        format(p.WorkDate,'yyyy-MMM-dd')WorkDate from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                                  left join mst.DesignationMasterLegalDesignation ddm on 
                        ddm.LegalDesignationId = ei.LegalDesignationId
                                            left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
									        left join DayStatusPlantChild dc on dc.EmpTypeId=dm.EmployeeCategoryId
											and dc.PlantId=ei.PlantId
						                    left join DayStatusHeader dh on dh.Id=dc.headerId
									        left join DayStatus ds on ds.headerId=dh.Id
											left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId
									        where ManualFlag=1 and ds.Code=p.DayStatusCode
									        and ei.PlantId='" + Plant + "'";
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
                var sql = @"select distinct p.EmpSystemID,Result=dt.DayType,p.ManualDayStatus,p.ProcessDayStatus, 
                dt.SandwichStatusFlag,dt.OTApplicable,dt.AutoLock,dt.GoodWorkApplicable,format(p.WorkDate,'yyyy-MMM-dd')WorkDate 
                from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                        left join mst.DesignationMasterLegalDesignation ddm on 
                        ddm.LegalDesignationId = ei.LegalDesignationId
                        left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
						left join DayStatusPlantChild dc on dc.EmpTypeId=dm.EmployeeCategoryId
						and dc.PlantId=ei.PlantId
						left join DayStatusHeader dh on dh.Id=dc.headerId
						left join DayStatus ds on ds.headerId=dh.Id
						left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId									       
						where ManualFlag=1
						and dt.DayType=ISNULL(p.ManualDayStatus,p.ProcessDayStatus)
						and ei.PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ManualsandwichLogic(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.EmpSystemID,p.SandwichFlag as TodayFlag,p.ProcessFinalDayStatus as TodayStatus,
                Format(WorkDate,'yyyy-MMM-dd')WorkDate,
				(
				select SandwichFlag from AttdnProcessData where WorkDate=DATEADD(day,-1,p.WorkDate) 
				and EmpSystemID=p.EmpSystemID
				and PlantID='" + Plant + @"'
				)PrevDayFlag,
				(
				select Format(WorkDate,'yyyy-MMM-dd')WorkDate from AttdnProcessData 
				where WorkDate=DATEADD(day,-1,p.WorkDate) 
				and EmpSystemID=p.EmpSystemID
				and PlantID='" + Plant + @"'
				)PrevWorkDate
				from AttdnProcessData p
                where ManualFlag=1 and PlantID='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void PayrollDayStatus(string Plant)
        {

            try
            {
                var sql = @"UPDATE 	AttdnProcessData Set DayStatus= isnull(Sandwichstatus,ProcessFinalDayStatus)
				,UpdatedBy='Schedule',DateUpdated=GETDATE()
								WHERE PlantID='" + Plant + @"'
								AND ManualFlag=1";

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
                var sql = @"select distinct p.EmpSystemID,
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
						and p.PlantId='" + Plant + "'";

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
        public void ManualEarnedLeave(out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct p.RowId,dt.DayType, dt.EarnedPL,dt.EarnedCL,
                        format(p.WorkDate,'yyyy-MMM-dd')WorkDate from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                        left join mst.DesignationMasterLegalDesignation ddm on 
                        ddm.LegalDesignationId = ei.LegalDesignationId
                        left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
						left join DayStatusPlantChild dc on dc.EmpTypeId=dm.EmployeeCategoryId
						and dc.PlantId=ei.PlantId
						left join DayStatusHeader dh on dh.Id=dc.headerId
						left join DayStatus ds on ds.headerId=dh.Id
						left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId									       
						where p.ManualFlag=1 
						and dt.DayType=p.DayStatus and (dt.EarnedCL>0 or dt.EarnedPL>0)
						and ei.PlantId='" + Plant + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
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
        #endregion

        #region Manual Scheduler
        public void ManualScheduler(string PlantValue)
        {
            try
            {
                string ManualFlagRowId = "''";

                #region Manual Day Duration  
                DataSet ManualDurn;
                ManualDuration(out ManualDurn, PlantValue);
                if (ManualDurn.Tables[0].Rows.Count > 0)
                {

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where IsLock=0 and isnull(InTime,'')!='' and isnull(OutTime,'')!='' and PlantID='" + PlantValue + "' and ManualFlag=1";

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    for (int i = 0; i < ManualDurn.Tables[0].Rows.Count; i++)
                    {
                        string EmpId = ManualDurn.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                        string WorkDate = ManualDurn.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");
                        string ProcessInTime = clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"InTime"]).ToString();
                        string ProcessOutTime = clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"OutTime"]).ToString();
                        string ShiftOutTime = clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftOutTime"]).ToString();
                        string ShiftInTime = clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftInTime"]).ToString();
                        string CalDuration = clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"CalDuration"]).ToString();
                        double ShiftEarlyInMargin = Convert.ToDouble(clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftEarlyInMargin"]).ToString());
                        double ShiftLateInMargin = Convert.ToDouble(clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftLateInMargin"]).ToString());
                        double ShiftEarlyOutMargin = Convert.ToDouble(clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftEarlyOutMargin"]).ToString());
                        double ShiftLateOutMargin = Convert.ToDouble(clsWebLib.RetValidLen(ManualDurn.Tables[0].Rows[i][@"ShiftLateOutMargin"]).ToString());

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {

                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            dr["Duration"] = CalDuration;
                            dr["EarlyLateIn"] = DBNull.Value;
                            dr["EarlyLateOut"] = DBNull.Value;
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
                            CheckerFunction(ref ManualFlagRowId, newformat + EmpId);
                        }
                    }
                    SaveDataSets(dsRef);

                }

                #endregion

                #region Manual OverStay UnderStay 
                DataSet ManualOverUnderStay;
                ManualOverUnderStayData(out ManualOverUnderStay, PlantValue);
                if (ManualOverUnderStay.Tables[0].Rows.Count > 0)
                {

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where  IsLock=0 and ManualFlag=1 and Duration >0 and PlantID='" + PlantValue + "'";

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    for (int i = 0; i < ManualOverUnderStay.Tables[0].Rows.Count; i++)
                    {
                        string WorkDate = ManualOverUnderStay.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");

                        string EmpId = ManualOverUnderStay.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                        double OverUnderStay = Convert.ToDouble(clsWebLib.RetValidLen(ManualOverUnderStay.Tables[0].Rows[i][@"OverUnderStay"]).ToString());

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {

                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            if (OverUnderStay > 0)
                            {
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
                                dr["OverStay"] = 0;
                                dr["UnderStay"] = OverUnderStay;
                            }

                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                            CheckerFunction(ref ManualFlagRowId, newformat + EmpId);
                        }
                    }
                    SaveDataSets(dsRef);

                }

                #endregion

                #region Manual DurationStatus Flagging
                DataSet ManualDurationStat;
                ManualDurationStatusCal(out ManualDurationStat, PlantValue);
                if (ManualDurationStat.Tables[0].Rows.Count > 0)
                {

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "'";

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    for (int i = 0; i < ManualDurationStat.Tables[0].Rows.Count; i++)
                    {
                        string WorkDate = ManualDurationStat.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WorkDate).ToString("yyyyMMdd");
                        string EmpId = ManualDurationStat.Tables[0].Rows[i][@"EmpSystemID"].ToString();
                        string ShortDuration = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"ShiftShortDuration"]).ToString();
                        string FullDayDuration = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"ShiftFullDayDuration"]).ToString();
                        string HalfDayDuration = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"ShiftHalfDayDuration"]).ToString();
                        string Duration = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"Duration"]).ToString();
                        string In = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"InTime"]).ToString();
                        string Out = clsWebLib.RetValidLen(ManualDurationStat.Tables[0].Rows[i][@"OutTime"]).ToString();

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            if (Duration.ToString() != "" &&
                                FullDayDuration.ToString() != ""
                                && ShortDuration.ToString() != ""
                                && HalfDayDuration.ToString() != "")
                            {
                                if (Convert.ToDouble(Duration) >= Convert.ToDouble(FullDayDuration))
                                {
                                    dr["DurationStatus"] = "FD";
                                }
                                else if (Convert.ToDouble(Duration) >= Convert.ToDouble(HalfDayDuration))
                                {
                                    dr["DurationStatus"] = "HD";
                                }
                                else if (Convert.ToDouble(Duration) >= Convert.ToDouble(ShortDuration))
                                {
                                    dr["DurationStatus"] = "SD";
                                }
                                else if (Convert.ToDouble(Duration) < Convert.ToDouble(ShortDuration))
                                {
                                    dr["DurationStatus"] = "A";
                                }
                            }
                            else
                            {
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
                            CheckerFunction(ref ManualFlagRowId, newformat + EmpId);
                        }
                    }
                    SaveDataSets(dsRef);

                }

                #endregion

                #region Manual Day Status Code              
                ManualDayStatusCodeData(PlantValue);
                #endregion

                #region Manual User Day Status 
                DataSet ManualUserDayStat;
                ManualDayStatus(out ManualUserDayStat, PlantValue);
                if (ManualUserDayStat.Tables[0].Rows.Count > 0)
                {

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "'";

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                    for (int i = 0; i < ManualUserDayStat.Tables[0].Rows.Count; i++)
                    {
                        var WkDate = ManualUserDayStat.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                        string EmpId = clsWebLib.RetValidLen(ManualUserDayStat.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                        string DayStatus = clsWebLib.RetValidLen(ManualUserDayStat.Tables[0].Rows[i][@"DayType"]).ToString();

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {

                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["ProcessDayStatus"] = DayStatus;
                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                            CheckerFunction(ref ManualFlagRowId, newformat + EmpId);
                        }
                    }
                    SaveDataSets(dsRef);

                }
                #endregion

                #region ProcessFinalDayStatus 
                DataSet ManualFinalDayStat;
                ManualFinalDayStatus(out ManualFinalDayStat, PlantValue);
                if (ManualFinalDayStat.Tables[0].Rows.Count > 0)
                {

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where IsLock=0 and ManualFlag=1 and PlantID='" + PlantValue + "'";

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");


                    for (int i = 0; i < ManualFinalDayStat.Tables[0].Rows.Count; i++)
                    {
                        var WkDate = ManualFinalDayStat.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");
                        string EmpId = clsWebLib.RetValidLen(ManualFinalDayStat.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                        string Result = clsWebLib.RetValidLen(ManualFinalDayStat.Tables[0].Rows[i][@"Result"]).ToString();
                        string SandwichFlag = clsWebLib.RetValidLen(ManualFinalDayStat.Tables[0].Rows[i][@"SandwichStatusFlag"]).ToString();
                        string OtApplicable = clsWebLib.RetValidLen(ManualFinalDayStat.Tables[0].Rows[i][@"OTApplicable"]).ToString();
                        string Goodwork = clsWebLib.RetValidLen(ManualFinalDayStat.Tables[0].Rows[i][@"GoodWorkApplicable"]).ToString();
                        string AutoLock = clsWebLib.GetBoolData(ManualFinalDayStat.Tables[0].Rows[i][@"AutoLock"]).ToString();

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {

                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["ProcessFinalDayStatus"] = Result;
                            dr["SandwichFlag"] = SandwichFlag;
                            dr["DayTypeOTApplicable"] = OtApplicable;
                            dr["DayTypeGoodWorkApplicable"] = Goodwork;
                            if (AutoLock == "True")
                            {
                                dr["IsLock"] = true;
                                dr["LockedDate"] = DateTime.Now;
                                dr["LockedBy"] = "AutoLock";
                            }
                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                            CheckerFunction(ref ManualFlagRowId, newformat + EmpId);
                        }
                    }
                    SaveDataSets(dsRef);

                }
                #endregion

                #region Sandwich Logic 
                DataSet ManualSandwichData;
                ManualsandwichLogic(out ManualSandwichData, PlantValue);
                if (ManualSandwichData.Tables[0].Rows.Count > 0)
                {

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where ManualFlag=1 and PlantID='" + PlantValue + "'";

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    for (int i = 0; i < ManualSandwichData.Tables[0].Rows.Count; i++)
                    {
                        var WkDate = ManualSandwichData.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");
                        string EmpId = clsWebLib.RetValidLen(ManualSandwichData.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                        string PrevDaySandwich = clsWebLib.RetValidLen(ManualSandwichData.Tables[0].Rows[i][@"PrevDayFlag"]).ToString();
                        var PrevWkDate = clsWebLib.RetValidLen(ManualSandwichData.Tables[0].Rows[i][@"PrevWorkDate"]).ToString();

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            string TodaySandwich = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"SandwichFlag"]).ToString();
                            string FinalStatus = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"ProcessFinalDayStatus"]).ToString();

                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            if (PrevDaySandwich == "0" && TodaySandwich == "2")
                            {
                                dr["SandwichFlag"] = "0"; //Today
                            }

                            else if (PrevDaySandwich == "1" && TodaySandwich == "2")
                            {
                                dr["SandwichFlag"] = "2"; //Today
                            }
                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                            dr.EndEdit();
                            CheckerFunction(ref ManualFlagRowId, newformat + EmpId);

                            if (PrevDaySandwich == "2")
                            {
                                if (TodaySandwich == "1" && PrevWkDate != "")
                                {
                                    if (FinalStatus != "")
                                    {
                                        var sql = @"update AttdnProcessData set UpdatedBy='Sandwich', DayStatus='" + FinalStatus + "',Sandwichstatus='" + FinalStatus + "'" +
                                            "where WorkDate <= '" + PrevWkDate + "' and EmpSystemID='" + EmpId + "' " +
                                            "and SandwichFlag!=1 and SandwichFlag!=0";

                                        ConnectionManager.DAL.ConManager objCone = null;
                                        objCone = new ConnectionManager.DAL.ConManager("1");
                                        objCone.OpenConnection("1");
                                        objCone.BeginTransaction();

                                        objCone.ExecuteNonQueryWrapper(sql, true, "1");
                                        objCone.CommitTransaction();
                                    }
                                }
                                else if (TodaySandwich == "0" && PrevWkDate != "")
                                {
                                    var sql = @"update AttdnProcessData set UpdatedBy='Sandwich',SandwichFlag='0' 
                                    where WorkDate <= '" + PrevWkDate + "' and EmpSystemID='" + EmpId + "' " +
                                    "and SandwichFlag!=1 and SandwichFlag!=0";

                                    ConnectionManager.DAL.ConManager objCone = null;
                                    objCone = new ConnectionManager.DAL.ConManager("1");
                                    objCone.OpenConnection("1");
                                    objCone.BeginTransaction();

                                    objCone.ExecuteNonQueryWrapper(sql, true, "1");
                                    objCone.CommitTransaction();
                                }
                            }

                        }
                    }
                    SaveDataSets(dsRef);

                }
                #endregion

                #region Payroll DayStatus 
                PayrollDayStatus(PlantValue);
                #endregion

                #region OT Calculation 
                DataSet ProcessOTCalculate;
                ProcessedOTCalculation(out ProcessOTCalculate, PlantValue);
                if (ProcessOTCalculate.Tables[0].Rows.Count > 0)
                {
                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where ManualFlag=1 and IsOTEntitled='1' and PlantID='" + PlantValue + "'";

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    var sqly = @"select * from PlantWiseHRMSSetting where PlantID='" + PlantValue + "'";
                    objCon.OpenDataSetThroughAdapter(sqly, out DataSet OTMode, false, false, "", "1");

                    string OTModeValue = clsWebLib.RetValidLen(OTMode.Tables[0].Rows[0][@"ResultendOT"]).ToString();
                    // 0 means Punched Based 1 means Manual 2 means Mixed

                    for (int i = 0; i < ProcessOTCalculate.Tables[0].Rows.Count; i++)
                    {
                        var WkDate = ProcessOTCalculate.Tables[0].Rows[i][@"WorkDate"].ToString();
                        string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");
                        string EmpId = clsWebLib.RetValidLen(ProcessOTCalculate.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                        string Result = clsWebLib.RetValidLen(ProcessOTCalculate.Tables[0].Rows[i][@"Result"]).ToString();

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + EmpId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            string PastManualOT = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"ManualOt"]).ToString();
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;

                            if (OTModeValue == "0")
                            {
                                if (Result != "")
                                {
                                    if (Convert.ToDouble(Result) > 0)
                                    {
                                        dr.BeginEdit();
                                        dr["ProcessedOT"] = Result;
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                        CheckerFunction(ref ManualFlagRowId, newformat + EmpId);
                                    }
                                }

                            }
                            else if (OTModeValue == "1")
                            {
                                if (PastManualOT != "")
                                {
                                    if (Convert.ToDouble(PastManualOT) > 0)
                                    {
                                        dr.BeginEdit();
                                        dr["ProcessedOT"] = PastManualOT;
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                        CheckerFunction(ref ManualFlagRowId, newformat + EmpId);
                                    }
                                }
                            }

                            else 
                            {
                                if (Result != "")
                                {
                                    if (PastManualOT != "")
                                    {
                                        if (Convert.ToDouble(PastManualOT) > 0)
                                        {
                                            if (Convert.ToDouble(PastManualOT) < Convert.ToDouble(Result))
                                            {
                                                dr.BeginEdit();
                                                dr["ProcessedOT"] = PastManualOT;
                                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                                dr.EndEdit();
                                                CheckerFunction(ref ManualFlagRowId, newformat + EmpId);
                                            }
                                        }
                                        else
                                        {
                                            dr.BeginEdit();
                                            dr["ProcessedOT"] = Result;
                                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                            dr.EndEdit();
                                            CheckerFunction(ref ManualFlagRowId, newformat + EmpId);
                                        }
                                        
                                    }
                                    else
                                    {
                                        dr.BeginEdit();
                                        dr["ProcessedOT"] = Result;
                                        dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                        dr.EndEdit();
                                        CheckerFunction(ref ManualFlagRowId, newformat + EmpId);
                                    }

                                }
                            }
                        }
                    }
                    SaveDataSets(dsRef);

                }
                #endregion

                #region Manual EarnedLeave Processing 
                DataSet ManualEarnedLeaveData;
                ManualEarnedLeave(out ManualEarnedLeaveData, PlantValue);
                if (ManualEarnedLeaveData.Tables[0].Rows.Count > 0)
                {

                    string RowIdDataSet = "''";
                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where ManualFlag=1 and PlantID='" + PlantValue + "'";
                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet RowIdSet, false, false, "", "1");
                    
                    if(RowIdSet.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < RowIdSet.Tables[0].Rows.Count; i++)
                        {
                            RowIdDataSet += ",'" + RowIdSet.Tables[0].Rows[i][@"RowId"].ToString() + "'";
                        }
                        var sql = @"select * from LeaveEarned where RowId In("+RowIdDataSet+") ";

                        objCon.OpenDataSetThroughAdapter(sql, out DataSet dsRef, false, false, "", "1");

                        for (int i = 0; i < ManualEarnedLeaveData.Tables[0].Rows.Count; i++)                            
                        {
                                string RowId = clsWebLib.RetValidLen(ManualEarnedLeaveData.Tables[0].Rows[i][@"RowId"]).ToString();
                                string DayType = clsWebLib.RetValidLen(ManualEarnedLeaveData.Tables[0].Rows[i][@"DayType"]).ToString();
                                string EarnedPL = clsWebLib.RetValidLen(ManualEarnedLeaveData.Tables[0].Rows[i][@"EarnedPL"]).ToString();
                                string EarnedCL = clsWebLib.RetValidLen(ManualEarnedLeaveData.Tables[0].Rows[i][@"EarnedCL"]).ToString();

                                dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {

                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();
                                    dr["DayStatus"] = DayType;
                                    if (EarnedCL != "")
                                    {
                                        dr["EarnedCasualLeave"] = EarnedCL;
                                    }
                                    if (EarnedPL != "")
                                    {
                                        dr["EarnedPriviledgeLeave"] = EarnedPL;
                                    }
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr["UpdatedBy"] = "ManualProcess";
                                    dr.EndEdit();
                                }                            
                        }
                           
                        SaveDataSets(dsRef);
                        
                    }

                }
                #endregion

                #region Set Manual Flag ->0              
                ProcessManualFlag(ManualFlagRowId);
                #endregion


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region MonthlyData Summary Process           

        public void MonthlySummary(string Date)
        {
            try
            {
                string Day = Convert.ToDateTime(Date).AddDays(-1).ToString("dd-MMM-yyyy");
                DataSet MonthlyData;
                MonthlySummarySource(Day, out MonthlyData);
                if (MonthlyData.Tables[0].Rows.Count > 0)
                {
                    var Year = MonthlyData.Tables[0].Rows[0][@"Year"].ToString();
                    var GpId = MonthlyData.Tables[0].Rows[0][@"GroupID"].ToString();
                    var Month = MonthlyData.Tables[0].Rows[0][@"Month"].ToString();

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter("select * from AttdnDataMonthlySummary where YearNo='" + Year + "' and MonthNo='" + Month + "' and GroupID='" + GpId + "'", out DataSet dsRef, false, false, "", "1");


                    for (int i = 0; i < MonthlyData.Tables[0].Rows.Count; i++)
                    {
                        string EmpId = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"EmpSystemID"]).ToString();
                        string PlantId = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"PlantId"]).ToString();
                        string FromDate = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"FromDate"]).ToString();
                        string ToDate = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"ToDate"]).ToString();
                        string TotalProcDate = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalProcDate"]).ToString();
                        string TotalPresent = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalPresent"]).ToString();
                        string TotalLate = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalLate"]).ToString();
                        string TotalAbsent = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalAbsent"]).ToString();
                        string TotalLv = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalLv"]).ToString();
                        string TotalMlv = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalMlv"]).ToString();
                        string TotalCompAssignLv = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalCompAssignLv"]).ToString();
                        string TotalWeekOff = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalWeekOff"]).ToString();
                        string TotalHoliDay = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalHoliDay"]).ToString();
                        string TotalWeekOffHoliDay = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalWeekOffHoliDay"]).ToString();
                        string TotalOTHr = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalOTHr"]).ToString();
                        string TotalLWP = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalLWP"]).ToString();
                        string TotalCasualLeave = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalCasualLeave"]).ToString();
                        string TotalPriviledgeLeave = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalPriviledgeLeave"]).ToString();
                        string TotalMedicalLeave = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalMedicalLeave"]).ToString();
                        string TotalPayDay = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalPayDay"]).ToString();
                        string TotalNonPayDay = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalNonPayDay"]).ToString();
                        string TotalWorkingDay = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"TotalWorkingDay"]).ToString();
                        string ActualWorkingDay = clsWebLib.RetValidLen(MonthlyData.Tables[0].Rows[i][@"ActualWorkingDay"]).ToString();

                        dsRef.Tables[0].DefaultView.RowFilter = @"EmpSystemID='" + EmpId + "' ";


                        if (dsRef.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = dsRef.Tables[0].NewRow();
                            dr["EmpSystemID"] = EmpId;
                            dr["FromDate"] = FromDate;
                            dr["ToDate"] = ToDate;
                            dr["YearNo"] = Year;
                            dr["MonthNo"] = Month;
                            dr["GroupID"] = GpId;
                            dr["PlantID"] = PlantId;
                            dr["TotalProcDate"] = TotalProcDate;
                            dr["TotalPresent"] = TotalPresent;
                            dr["TotalLate"] = TotalLate;
                            dr["TotalAbsent"] = TotalAbsent;
                            dr["TotalLv"] = TotalLv;
                            dr["TotalMlv"] = TotalMlv;
                            dr["TotalCompAssignLv"] = TotalCompAssignLv;
                            dr["TotalWeekOff"] = TotalWeekOff;
                            dr["TotalWeekOffHoliDay"] = TotalWeekOffHoliDay;
                            dr["TotalHoliDay"] = TotalHoliDay;
                            dr["TotalOTHr"] = TotalOTHr;
                            dr["TotalLWP"] = TotalLWP;
                            dr["TotalCasualLeave"] = TotalCasualLeave;
                            dr["TotalPriviledgeLeave"] = TotalPriviledgeLeave;
                            dr["TotalMedicalLeave"] = TotalMedicalLeave;
                            dr["TotalPayDay"] = TotalPayDay;
                            dr["TotalNonPayDay"] = TotalNonPayDay;
                            dr["TotalWorkingDay"] = TotalWorkingDay;
                            dr["ActualWorkingDay"] = ActualWorkingDay;
                            dr["TotalNormalOTHr"] = 0;
                            dr["TotalExtraOTHr"] = 0;
                            dr["IsDisbusted"] = false;
                            dr["AddedBy"] = "Schedule";
                            dr["DateAdded"] = Convert.ToDateTime(DateTime.Now);

                            dsRef.Tables[0].Rows.Add(dr);

                        }
                        else
                        {

                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["FromDate"] = FromDate;
                            dr["ToDate"] = ToDate;
                            dr["PlantID"] = PlantId;
                            dr["TotalProcDate"] = TotalProcDate;
                            dr["TotalPresent"] = TotalPresent;
                            dr["TotalLate"] = TotalLate;
                            dr["TotalAbsent"] = TotalAbsent;
                            dr["TotalLv"] = TotalLv;
                            dr["TotalMlv"] = TotalMlv;
                            dr["TotalCompAssignLv"] = TotalCompAssignLv;
                            dr["TotalWeekOff"] = TotalWeekOff;
                            dr["TotalWeekOffHoliDay"] = TotalWeekOffHoliDay;
                            dr["TotalHoliDay"] = TotalHoliDay;
                            dr["TotalOTHr"] = TotalOTHr;
                            dr["TotalNonPayDay"] = TotalNonPayDay;
                            dr["TotalLWP"] = TotalLWP;
                            dr["TotalWorkingDay"] = TotalWorkingDay;
                            dr["TotalPayDay"] = TotalPayDay;
                            dr["TotalCasualLeave"] = TotalCasualLeave;
                            dr["TotalPriviledgeLeave"] = TotalPriviledgeLeave; 
                            dr["ActualWorkingDay"] = ActualWorkingDay;
                            dr["TotalMedicalLeave"] = TotalMedicalLeave;
                            dr["TotalNormalOTHr"] = 0;
                            dr["TotalExtraOTHr"] = 0;
                            dr["IsDisbusted"] = false;
                            dr["UpdatedBy"] = "Schedule";
                            dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);

                            dr.EndEdit();

                        }

                    }

                    SaveDataSets(dsRef);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }



        }

        #endregion

        #region Roster Process

        public void RosterProcess(string PlantId, string Date)
        {
            try
            {


                var sql2 = @"Select * from dbo.RosterPatternHeader where PlantId = '" + PlantId + "'";
                DataTable RosterTable = new DataTable();
                RosterTable = _sqlRepository.GetDataTable(sql2);
                if (RosterTable.Rows.Count > 0)
                {
                    //Loop to go through all the Rosters in a Plant
                    for (int j = 0; j < RosterTable.Rows.Count; j++)
                    {
                        DateTime ddt = Convert.ToDateTime(Date);
                        string DaysCol = "Days" + DateTime.DaysInMonth(ddt.Year, ddt.Month).ToString();

                        //Getting all the Shifts Child 
                        var sql3 = @"Select *, " + DaysCol + " as ShiftSequence from dbo.RosterPatternChild where RPHeaderId = '" + RosterTable.Rows[j]["Id"].ToString() + "' order by ShiftSequence";
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
                                //Dictionary and DataSet Initialization
                                DataSet ds;
                                ConnectionManager.DAL.ConManager cona = new ConnectionManager.DAL.ConManager("1");
                                cona.OpenDataSetThroughAdapter("select * from RosterPatternProcess where 1 = 2", out ds, false, "1");

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


                                SaveDataSets(ds);
                            }

                        }

                    }
                }

            }
            catch (Exception e)
            {
                throw e;
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
                catch (Exception exp)
                {
                    throw exp;
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
                        ShiftProcess(Date, PlantValue);
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
                        AttndProcess(Date, PlantValue);
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
                        DayStatus(Date, PlantValue);
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
                        RosterProcess(PlantValue, Date);
                    }
                    catch (Exception ex)
                    {
                        CommonLogFunction(ex, CatchPlant, "RosterProcess");                       
                    }
                }
            }
        }
       
        #endregion

        public void CommonLogFunction(Exception ex, string CatchPlant,string Process)
        {

            string ErrorlineNo, Errormsg, extype, ErrorLocation;

            ErrorlineNo = ex.StackTrace.ToString();
            Errormsg = ex.GetType().Name.ToString();
            extype = ex.GetType().ToString();
            ErrorLocation = ex.Message.ToString();
            string error = "Plant:- " + CatchPlant + " Error Line No :" + " " + ErrorlineNo + " Error Message:" + " " + Errormsg + "Exception Type:" + " " + extype + "Error Location :" + " " + ErrorLocation;
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
