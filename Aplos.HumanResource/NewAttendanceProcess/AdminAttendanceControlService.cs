using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.HumanResource.Attendance.Manual;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Service.Attendances;
using SetINOUT;
using Library.Core;
using bplib;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class AdminAttendanceControlService
    {

        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        public AdminAttendanceControlService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        
        public IEnumerable<object> GetShiftData(string ShiftId, string Date)
        {
            try
            {
                var sql = @"SELECT sd.SystemID,sd.UserName AS ShiftName,
                            format(kk.ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
                            format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime,
							kk.ShiftShortDuration,kk.ShiftHalfDayDuration,kk.ShiftHoursWithoutOt,kk.ShiftFullDayDuration,
                            kk.ShiftDuration
						
                            FROM (
                            SELECT 
                            sd.SystemID,
                            DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),'"+Date+@"'))  AS ShiftInTime,
                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),'"+Date+ @"'))  AS ShiftOutTime
							,isnull(stcm.ShortDuration,sd.ShortDuration) as ShiftShortDuration
		                    ,isnull(stcm.HalfDayDuration,sd.HalfDayDuration) as ShiftHalfDayDuration
							,isnull(stcm.HoursWithoutOT,sd.HoursWithoutOT) as ShiftHoursWithoutOt,
							isnull(stcm.FullDayDuration,sd.FullDayDuration) as ShiftFullDayDuration,
                            isnull(stcm.ShiftDuration,sd.ShiftDuration) as ShiftDuration
                            
                            FROM ShiftDefination sd
                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '" + Date+@"' 
							BETWEEN stcm.FromDate AND stcm.ToDate AND 
							sd.SystemID=stcm.ShiftDefinationID
                            ) AS KK
                            INNER JOIN   ShiftDefination sd ON sd.SystemID=kk.SystemID
                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '"+Date+@"'
							BETWEEN stcm.FromDate AND stcm.ToDate AND 
							sd.SystemID=stcm.ShiftDefinationID
                            WHERE sd.systemid='"+ShiftId+@"'
                            ORDER BY sd.SequenceNo ASC";
                return _sqlRepository.GetDataCollection(sql, null);
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
                            isnull(stcm.ShortDuration,sd.ShortDuration) as ShiftShortDuration
		                    ,isnull(stcm.HalfDayDuration,sd.HalfDayDuration) as ShiftHalfDayDuration
							,isnull(stcm.HoursWithoutOT,sd.HoursWithoutOT) as ShiftHoursWithoutOt,
							isnull(stcm.FullDayDuration,sd.FullDayDuration) as ShiftFullDayDuration,
                            isnull(stcm.ShiftDuration,sd.ShiftDuration) as ShiftDuration
                            
                             FROM 
                             
                              (" + dateString + @") AS DT
								LEFT OUTER JOIN ShiftDefination sd ON 1=1
								LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON DT.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                            ) AS KK ON dt.WorkDate=kk.WorkDate
                            INNER JOIN   ShiftDefination sd ON sd.SystemID=kk.SystemID
                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON dt.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
							WHERE sd.PlantID='" + data[0].PlantID + @"'
                        ORDER BY dt.WorkDate, sd.SequenceNo ASC ";

            return _sqlRepository.GetDataTable(sql);
        }

        public RTx Savex(List<AttendanceProcessNewProcess> data , string Remarks)
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

                DataTable NewShiftStandardTime = getDateWiseShift(DataToBeSaved);
                //validations
                string inDates = "";
                string inEmployeeIds = "";

                foreach (AttendanceProcessNewProcess item in DataToBeSaved)
                {

                    if (inDates == "")
                    {
                        inDates = "'" + item.WorkDate + "'";
                    }
                    else
                    {
                        inDates += ",'" + item.WorkDate + "'";
                    }

                    if (inEmployeeIds == "")
                    {
                        inEmployeeIds = "'" + item.Id + "'";
                    }
                    else
                    {
                        inEmployeeIds += ",'" + item.Id + "'";
                    }
                }

                    
                 foreach (AttendanceProcessNewProcess item in DataToBeSaved)
                 {

                    
                    if (inDates != "")
                    {
                        DataTable dtLock = _sqlRepository.GetDataTable("SELECT * FROM PlantWiseAttendanceLock AS pwal WHERE  isActive=1 AND pwal.LockedDate IN (" + inDates + ") AND pwal.PlantId='" +data[0].PlantID + "'");
                        if (dtLock.Rows.Count > 0)
                        {
                            for (int i = 0; i < dtLock.Rows.Count; i++)
                            {
                                var k = DataToBeSaved.Where(ee => ee.WorkDate.ToUpper() == Convert.ToDateTime(dtLock.Rows[i]["LockedDate"].ToString()).ToString("dd-MMM-yyyy").ToUpper());
                                foreach (var itemx in k)
                                {
                                    dtLock.DefaultView.RowFilter = "LockedDate='" + itemx.WorkDate + "'";
                                    if (dtLock.DefaultView.Count > 0)
                                    {
                                        itemx.IsError = true;
                                        itemx.ErrorMessage = "Day locked";
                                    }
                                }
                            }
                        }
                    }


                        if (string.IsNullOrEmpty(item.InDate) == false)
                        if (clsWebLib.IsDateOK(item.InDate) == false)
                            item.ErrorMessage = "Invalid in date";


                    if (string.IsNullOrEmpty(item.OutDate) == false)
                        if (clsWebLib.IsDateOK(item.OutDate) == false)
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

                    if (item.DayStatus != item.DayStatusNew)
                    {
                        string TodaySandwich = clsWebLib.RetValidLen(item.SandwichFlag).ToString();
                        string PastSandwich = clsWebLib.RetValidLen(item.PrevDayFlag).ToString();
                        string FutureSandwich = clsWebLib.RetValidLen(item.FutureDayFlag).ToString();

                        if (TodaySandwich == "1" && PastSandwich == "2" && FutureSandwich == "2")
                        {
                            item.IsError = true;
                            item.ErrorMessage = "It is a Sandwich Case Please check ...";
                        }
                    }


                 }

                if (DataToBeSaved.Where(ee => ee.IsError == true).ToList().Count > 0)
                {
                    return new RTx { data = DataToBeSaved, IsError = true, msg = "Error occured" };
                }
               
                saveDatax(DataToBeSaved , Remarks);


                return new RTx { data = data, IsError = false, msg = "Manual Entry Done Successfully" };
             
            }
            catch (Exception ex)
            {
                return new RTx { data = data, IsError = true, msg = ex.Message };
                
            }

        }
        
        private void saveDatax(List<AttendanceProcessNewProcess> data , string Remarks)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                clsGenID objId = new clsGenID();

                //New
                string RowsEdits = "''";
                NewAttendanceProcessService ap = new NewAttendanceProcessService();

                DataSet dsRem;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter(@"Select * from dbo.ManualEntryRemarks where 1 = 2", out dsRem, false, "1");
                // new End
                DataSet shiftchange = null ;
                
                for (int i = 0; i < data.Count; i++)
                {
                    con = new ConnectionManager.clsConnection();
                    con.BeginTransaction();
                    con.getDataSet(@"SELECT * FROM AttdnProcessData  WHERE EmpSystemID = '" + data[i].Id + "' AND WorkDate = '" + data[i].WorkDate + "' ", out shiftchange);
                    con.CommitTransaction();

                    //Manual Entry Flag
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
                            shiftchange.Tables[0].Rows[0]["LockedDate"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["LockedBy"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["IsLock"] = false;
                            shiftchange.Tables[0].Rows[0]["OTComfirmBy"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["DateOTComfirm"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["IsOTComfirm"] = false;

                            #region OT Nullified Columns
                            shiftchange.Tables[0].Rows[0]["TargetOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["PlanOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AppliedOTLimit"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AllowedOTLimit"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["StandardOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AdditionalOt"] = DBNull.Value;
                            #endregion

                            shiftchange.Tables[0].Rows[0].EndEdit();
                            //New
                            kk++;
                            ap.CheckerFunction(ref RowsEdits, shiftchange.Tables[0].Rows[0]["RowId"].ToString());
                        }
                        #endregion change shift
                    }

                    #region Day Status

                    if (data[i].DayStatusNew != data[i].DayStatus)
                    {
                        if (shiftchange.Tables[0].Rows.Count > 0)
                        {

                            DataRow dr = shiftchange.Tables[0].Rows[0];
                            dr.BeginEdit();

                            if (string.IsNullOrEmpty(data[i].DayStatusNew) == false)
                            {
                                if (dr["SandwichFlag"].ToString() == "2")
                                {
                                    dr["SandwichFlag"] = 0;
                                    dr["SandwichStatus"] = DBNull.Value;
                                }

                                dr["ManualDayStatus"] = data[i].DayStatusNew;
                                dr["DayStatus"] = data[i].DayStatusNew;
                                dr["IsManualDayStatus"] = true;
                                dr["ManualByWhom"] = identity.Name;
                                dr["ManualEntryTime"] = DateTime.Now;
                                dr["ManualFlag"] = true;
                                dr["IsLock"] = false;
                                dr["LockedBy"] = DBNull.Value;
                                dr["LockedDate"] = DBNull.Value;
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
                            }

                            dr.EndEdit();
                            //New
                            ap.CheckerFunction(ref RowsEdits, dr["RowId"].ToString());
                            kk++;
                        }
                    }

                    #endregion

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
                                dr["IsLock"] = false;
                                dr["LockedBy"] = DBNull.Value;
                                dr["LockedDate"] = DBNull.Value; 
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
                                //New
                                ap.CheckerFunction(ref RowsEdits, dr["RowId"].ToString());
                                kk++;
                            }
                        }
                            
                        
                    }
                    #endregion

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(shiftchange);

                    string _Id = null;

                    if (kk>0)
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
                        dr["Screen"] = "/admin-attdn-control";
                        dsRem.Tables[0].Rows.Add(dr);
                    }

                }

                //New
                clsStaticInfo _infos = new clsStaticInfo();
                _infos.SaveDataSets(dsRem);

                ap.ManualScheduler(data[0].PlantID, RowsEdits);
                
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> GetDayStatus(string PlantId)
        {
            try
            {              
                var sql = @"select distinct DayType,dt.Id from DayTypeWithValues dt 
                left join DayStatusHeader dh on dh.Id=dt.HeaderId
                left join DayStatusPlantChild dc on dc.HeaderId=dh.Id
                where dt.ManualStatusAllowed=1 and dc.PlantId='" + PlantId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string stringAttendanceData(string employeeid, string fromdate, string todate, string PlantId)
        {

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


                            KK.IsManualOutTime,KK.DayStatusCode,KK.DayStatus AS DayStatusNew,

                            format(KK.PunchInTime,'dd-MMM-yyyy hh:mm tt') AS PunchInTime,
                            format(KK.PunchOutTime,'dd-MMM-yyyy hh:mm tt') AS PunchOutTime,

                            KK.DayStatus, KK.OTHr,KK.plantid AS PlantID,
                            KK.IsOTComfirm, KK.IsOTEntitled,KK.IsManualDayStatus,convert(bit,isnull(KK.IsLock,0)) AS IsLock,

                           (
				            select SandwichFlag from AttdnProcessData where WorkDate=DATEADD(day,-1,kk.WorkDate) 
				            and EmpSystemID=kk.EmpSystemID
				            and PlantID='" + PlantId + @"'
				            )PrevDayFlag,KK.SandwichFlag,
				            (
				            select SandwichFlag from AttdnProcessData where WorkDate=DATEADD(day,+1,kk.WorkDate) 
				            and EmpSystemID=kk.EmpSystemID
				            and PlantID='" + PlantId + @"'
				            )FutureDayFlag

                             FROM (
								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, O.IsManualDayStatus,O.IsLock,
       
		                            O.PunchInTime,O.PunchOutTime,o.EmpSystemID,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,O.DayStatusCode,
		                            O.IsOTEntitled,emp.plantid,o.SandwichFlag

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
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON EMP.DepartmentId=DEPT.Id	
                        where emp.plantid='" + PlantId + @"'
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, WorkDate) ASC ";

        }

        public DataTable getCurrentFile( string PlId, string FD, string TD, string Emps)
        {
            try
            {
                string EmpSel = "";
                if (Emps == "''")
                {
                    EmpSel = "";
                }
                else
                {
                    EmpSel = "and EmpSystemID in (" + Emps + ")";
                }

                var str = @"select EMP.EmployeeName,APD.RowId,APD.EmpSystemID,APD.WorkDate,APD.InTime,APD.OutTime,APD.ShiftSystemID,APD.DayStatus,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection,LN.UserName Line
                            from AttdnProcessData APD
							LEFT OUTER JOIN EmployeeInformation EMP ON APD.EmpSystemid=EMP.SystemID
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT JOIN ORG.Line LN ON LN.Id=EMP.LineId
                            LEFT JOIN ORG.Department DEPT ON EMP.DepartmentId=DEPT.Id
							where APD.WorkDate between '" + FD + @"' and '" + TD + @"'
                            AND APD.PlantID='" + PlId + @"' "+EmpSel+"";
                return _sqlRepository.GetDataTable(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void SaveFileList(List<Dictionary<string, object>> data, string PlId, string FD, string TD, string Emps , string Remarks)
        {
            try
            {

                #region PlantLock Check

                DataSet PlantLock;
                PlantLockCheck(FD , TD , out PlantLock, PlId);
                string pl = "";
                if(PlantLock.Tables[0].Rows.Count>0)
                {
                    for(var i = 0; i< PlantLock.Tables[0].Rows.Count; i++ )
                    {
                        pl = pl + " " + PlantLock.Tables[0].Rows[i]["LockedDate"].ToString() + ", "; 
                    }

                    throw new Exception("The Plant is Locked for - " + pl);
                }

                #endregion

                string EmpSel = "";
                if(Emps == "''")
                {
                    EmpSel = "";
                }
                else
                {
                    EmpSel = "and ap.EmpSystemID in (" + Emps + ")";
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = DateTime.Now.ToString();
                string TableName = "dbo.AttdnProcessData ap";

                //Getting the DayStatuses
                var sttr = @"Select DayType from dbo.DayType";
                DataTable DSTable = _sqlRepository.GetDataTable(sttr);
                string DayTypesList = "";
                for(int i = 0; i< DSTable.Rows.Count;i++)
                {
                    DayTypesList = DayTypesList + " " + DSTable.Rows[i]["DayType"].ToString();
                }

                DataSet dsRem;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter(@"Select * from dbo.ManualEntryRemarks where 1 = 2", out dsRem, false, "1");



                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(@"select  * , 
                            (
                            select SandwichFlag from AttdnProcessData where WorkDate = DATEADD(day, -1, ap.WorkDate)
                            and EmpSystemID = ap.EmpSystemID
                            and PlantID = ap.PlantID
                            )PrevDayFlag,
                            (
                            select SandwichFlag from AttdnProcessData where WorkDate = DATEADD(day, +1, ap.WorkDate)
                            and EmpSystemID = ap.EmpSystemID
                            and PlantID = ap.PlantID
                            )FutureDayFlag from " + TableName + " where ap.WorkDate between '" + FD + @"' and '" + TD + @"'AND ap.PlantID='" + PlId + @"' "+EmpSel+"", out dsMaster, false, "1");


                int KI = 0; 
                int KO = 0; 
                if (data.Count > 0)
                {
                    for(int i = 0; i < data.Count; i ++)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "RowId='" + data[i]["RowId"].ToString() + "'";
                        int j = dsMaster.Tables[0].DefaultView.Count;
                        KI = 0; KO = 0;
                        if (dsMaster.Tables[0].DefaultView.Count > 0)
                        {
                            dsMaster.Tables[0].DefaultView[0].BeginEdit();

                            if (clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0]["InTime"]).ToString() != clsWebLib.RetValidLen(data[i]["InTime"]).ToString())
                            {
                                if (clsWebLib.RetValidLen(data[i]["InTime"]).ToString() != "")
                                {
                                    //if(Convert.ToDateTime(dsMaster.Tables[0].DefaultView[0]["InTime"].ToString()) != Convert.ToDateTime(data[i]["InTime"].ToString()))
                                    //{


                                    //if (bplib.clsWebLib.IsDateOK(data[i]["InTime"].ToString()) == false)
                                    //    throw new Exception("Invalid in date - "+ i );

                                    dsMaster.Tables[0].DefaultView[0]["InTime"] = Convert.ToDateTime(data[i]["InTime"].ToString());
                                    dsMaster.Tables[0].DefaultView[0]["ManualInTime"] = Convert.ToDateTime(data[i]["InTime"].ToString());
                                    dsMaster.Tables[0].DefaultView[0]["IsManualInTime"] = true;
                                    dsMaster.Tables[0].DefaultView[0]["OriginalManualInTime"] = Convert.ToDateTime(data[i]["InTime"].ToString());
                                    dsMaster.Tables[0].DefaultView[0]["ProcessIntime"] = Convert.ToDateTime(data[i]["InTime"].ToString());

                                    KI = 1;

                                    //}

                                }
                                else
                                {
                                    dsMaster.Tables[0].DefaultView[0]["InTime"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["ManualInTime"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["OriginalManualInTime"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["IsManualInTime"] = true;
                                    dsMaster.Tables[0].DefaultView[0]["ProcessIntime"] = DBNull.Value;

                                }
                                // Fixed Values in Both If/Else Blocks
                                dsMaster.Tables[0].DefaultView[0]["ManualEntryTime"] = DateTime.Now;
                                dsMaster.Tables[0].DefaultView[0]["ManualByWhom"] = identity.Name;
                                dsMaster.Tables[0].DefaultView[0]["ManualFlag"] = true;
                                dsMaster.Tables[0].DefaultView[0]["isLock"] = false;
                                dsMaster.Tables[0].DefaultView[0]["LockedBy"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["LockedDate"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["OTComfirmBy"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["DateOTComfirm"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["IsOTComfirm"] = false;
                               
                                #region OT Nullified Columns
                                dsMaster.Tables[0].DefaultView[0]["StandardOT"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["AdditionalOt"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["AllowedOTLimit"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["AppliedOTLimit"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["PlanOT"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["TargetOT"] = DBNull.Value;
                                #endregion

                            }
                            else
                            {
                                if (clsWebLib.RetValidLen(data[i]["InTime"]).ToString() != "")
                                {
                                    KI = 1;
                                }
                            }

                            if (clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0]["OutTime"]).ToString() != clsWebLib.RetValidLen(data[i]["OutTime"]).ToString())
                            {
                                if (clsWebLib.RetValidLen(data[i]["OutTime"]).ToString() != "")
                                {
                                    //if (Convert.ToDateTime(dsMaster.Tables[0].DefaultView[0]["OutTime"].ToString()) != Convert.ToDateTime(data[i]["OutTime"].ToString()))
                                    //{

                                    //if (bplib.clsWebLib.IsDateOK(data[i]["OutTime"].ToString()) == false)
                                    //    throw new Exception("Invalid Out Time - " + i);

                                    dsMaster.Tables[0].DefaultView[0]["OutTime"] = Convert.ToDateTime(data[i]["OutTime"].ToString());
                                    dsMaster.Tables[0].DefaultView[0]["ManualOutTime"] = Convert.ToDateTime(data[i]["OutTime"].ToString());
                                    dsMaster.Tables[0].DefaultView[0]["IsManualOutTime"] = true;
                                    dsMaster.Tables[0].DefaultView[0]["OriginalManualOutTime"] = Convert.ToDateTime(data[i]["OutTime"].ToString());
                                    dsMaster.Tables[0].DefaultView[0]["ProcessOuttime"] = Convert.ToDateTime(data[i]["OutTime"].ToString());

                                    KO = 1;
                                    //}
                                }
                                else
                                {
                                    dsMaster.Tables[0].DefaultView[0]["OutTime"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["ManualOutTime"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["IsManualOutTime"] = true;
                                    dsMaster.Tables[0].DefaultView[0]["OriginalManualOutTime"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["ProcessOuttime"] = DBNull.Value;

                                }

                                // Fixed Values in both Sections
                                dsMaster.Tables[0].DefaultView[0]["ManualEntryTime"] = DateTime.Now;
                                dsMaster.Tables[0].DefaultView[0]["ManualByWhom"] = identity.Name;
                                dsMaster.Tables[0].DefaultView[0]["ManualFlag"] = true;
                                dsMaster.Tables[0].DefaultView[0]["isLock"] = false;
                                dsMaster.Tables[0].DefaultView[0]["LockedBy"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["LockedDate"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["OTComfirmBy"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["DateOTComfirm"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["IsOTComfirm"] = false;

                                #region OT Nullified Columns
                                dsMaster.Tables[0].DefaultView[0]["StandardOT"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["AdditionalOt"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["AllowedOTLimit"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["AppliedOTLimit"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["PlanOT"] = DBNull.Value;
                                dsMaster.Tables[0].DefaultView[0]["TargetOT"] = DBNull.Value;
                                #endregion


                            }
                            else
                            {
                                if (clsWebLib.RetValidLen(data[i]["OutTime"]).ToString() != "")
                                {
                                    KO = 1;
                                }
                            }


                            if (clsWebLib.RetValidLen(data[i]["ShiftSystemID"]).ToString() != "")
                            {
                                if (data[i]["ShiftSystemID"].ToString() != dsMaster.Tables[0].DefaultView[0]["ShiftSystemID"].ToString())
                                {
                                    dsMaster.Tables[0].DefaultView[0]["ManualEntryTime"] = DateTime.Now;
                                    dsMaster.Tables[0].DefaultView[0]["ManualByWhom"] = identity.Name;
                                    dsMaster.Tables[0].DefaultView[0]["ManualFlag"] = true;
                                    dsMaster.Tables[0].DefaultView[0]["isLock"] = false;
                                    dsMaster.Tables[0].DefaultView[0]["LockedBy"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["LockedDate"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["OTComfirmBy"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["DateOTComfirm"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["IsOTComfirm"] = false;


                                    #region OT Nullified Columns
                                    dsMaster.Tables[0].DefaultView[0]["StandardOT"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["AdditionalOt"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["AllowedOTLimit"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["AppliedOTLimit"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["PlanOT"] = DBNull.Value;
                                    dsMaster.Tables[0].DefaultView[0]["TargetOT"] = DBNull.Value;
                                    #endregion

                                    dsMaster.Tables[0].DefaultView[0]["ShiftSystemID"] = data[i]["ShiftSystemID"].ToString();

                                }
                            }


                            if (clsWebLib.RetValidLen(data[i]["DayStatus"]).ToString() != "")
                            {
                                if (dsMaster.Tables[0].DefaultView[0]["ManualDayStatus"].ToString() != data[i]["DayStatus"].ToString())
                                {
                                    if (DayTypesList.Contains(data[i]["DayStatus"].ToString()))
                                    {

                                        string TodaySandwich = clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0]["SandwichFlag"]).ToString();
                                        string PastSandwich = clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0]["PrevDayFlag"]).ToString();
                                        string FutureSandwich = clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0]["FutureDayFlag"]).ToString();

                                        if (TodaySandwich == "1" && PastSandwich == "2" && FutureSandwich == "2")
                                        {
                                            throw new Exception("It is a Sandwich Case Please check ... - " + dsMaster.Tables[0].DefaultView[0]["WorkDate"].ToString());
                                        }

                                        if (dsMaster.Tables[0].DefaultView[0]["SandwichFlag"].ToString() == "2")
                                        {
                                            dsMaster.Tables[0].DefaultView[0]["SandwichFlag"] = 0;
                                            dsMaster.Tables[0].DefaultView[0]["SandwichStatus"] = DBNull.Value;
                                        }

                                        dsMaster.Tables[0].DefaultView[0]["ManualEntryTime"] = DateTime.Now;
                                        dsMaster.Tables[0].DefaultView[0]["ManualByWhom"] = identity.Name;
                                        dsMaster.Tables[0].DefaultView[0]["ManualFlag"] = true;
                                        dsMaster.Tables[0].DefaultView[0]["isLock"] = false;
                                        dsMaster.Tables[0].DefaultView[0]["LockedBy"] = DBNull.Value;
                                        dsMaster.Tables[0].DefaultView[0]["LockedDate"] = DBNull.Value;
                                        dsMaster.Tables[0].DefaultView[0]["ManualDayStatus"] = data[i]["DayStatus"].ToString();
                                        dsMaster.Tables[0].DefaultView[0]["isManualDayStatus"] = true;
                                        dsMaster.Tables[0].DefaultView[0]["OTComfirmBy"] = DBNull.Value;
                                        dsMaster.Tables[0].DefaultView[0]["DateOTComfirm"] = DBNull.Value;
                                        dsMaster.Tables[0].DefaultView[0]["IsOTComfirm"] = false;

                                        #region OT Nullified Columns
                                        dsMaster.Tables[0].DefaultView[0]["StandardOT"] = DBNull.Value;
                                        dsMaster.Tables[0].DefaultView[0]["AdditionalOt"] = DBNull.Value;
                                        dsMaster.Tables[0].DefaultView[0]["AllowedOTLimit"] = DBNull.Value;
                                        dsMaster.Tables[0].DefaultView[0]["AppliedOTLimit"] = DBNull.Value;
                                        dsMaster.Tables[0].DefaultView[0]["PlanOT"] = DBNull.Value;
                                        dsMaster.Tables[0].DefaultView[0]["TargetOT"] = DBNull.Value;
                                        #endregion

                                    }
                                    else
                                    {
                                        throw new Exception("Day Status is not Present!!");
                                    }

                                }

                            }

                            //Checking of the Timing
                            if (KO == 1 && KI == 1)
                            {
                                if (Convert.ToDateTime(dsMaster.Tables[0].DefaultView[0]["InTime"].ToString()) > Convert.ToDateTime(dsMaster.Tables[0].DefaultView[0]["OutTime"].ToString()))
                                {
                                    throw new Exception("Out time is earlier than In time for RowId '" + data[i]["RowId"].ToString() + "'");
                                }

                                if (Convert.ToDateTime(dsMaster.Tables[0].DefaultView[0]["OutTime"].ToString()) > DateTime.Now)
                                {
                                    throw new Exception("Out time is greater than Now for RowId '" + data[i]["RowId"].ToString() + "'");
                                }

                                TimeSpan ts = Convert.ToDateTime(dsMaster.Tables[0].DefaultView[0]["InTime"].ToString()).Subtract(Convert.ToDateTime(dsMaster.Tables[0].DefaultView[0]["OutTime"].ToString()));
                                if (Math.Abs(ts.TotalHours) > 24)
                                {
                                    throw new Exception("Time span cannot be greater than 24 hours between in and out time - " + i);
                                }
                            }

                            dsMaster.Tables[0].DefaultView[0].EndEdit();

                            string _Id = null;

                            if (dsMaster.Tables[0].DefaultView[0]["ManualFlag"].ToString() == "True")
                            {
                                DataRow dr = dsRem.Tables[0].NewRow();
                                bplib.clsGenID genid = new bplib.clsGenID();
                                genid.GenID("dbo.ManualEntryRemarks", out _Id);
                                dr["Id"] = _Id;
                                dr["RowId"] = dsMaster.Tables[0].DefaultView[0]["RowId"].ToString();
                                dr["Remarks"] = Remarks;
                                dr["AddedDate"] = DateTime.Now;
                                dr["AddedBy"] = identity.Name;
                                dr["AddedFromIP"] = identity.IPAddress;
                                dr["Screen"] = "/admin-attdn-control";
                                dsRem.Tables[0].Rows.Add(dr);
                            }
                        }
                        else
                        {
                            throw new Exception("RowId is not Present / the Date Range is not set correctly!");
                        }
                    }


                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsRem);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getEmployees(string plantId)
        {
            try
            {
                var str = @"select EmployeeCode , SystemId , EmployeeName from dbo.EmployeeInformation where PlantId='"+plantId+"'";
                DataTable dt = _sqlRepository.GetDataTable(str);

                dt.Columns.Add("checked", typeof(bool));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["checked"] = false;
                }

                return Service.Helpers.DataTableExtensions.DataTableToJson(dt);
                
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void PlantLockCheck(string FDate,string TDate, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string From = Convert.ToDateTime(FDate).ToString("dd-MMM-yyyy");
                string To = Convert.ToDateTime(TDate).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId='" + Plant + @"'
                and LockedDate between '" + From + "' and '"+To+"' and IsActive='1'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
}
