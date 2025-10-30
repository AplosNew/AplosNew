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
using Library.HumanResource.NewAttendanceProcess;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class ManualAttndFromAppService
    {
        
        CustomIdentity _identity = null;
        ISqlRepository _sqlRepository = null;
        public ManualAttndFromAppService(CustomIdentity _identity, ISqlRepository _sqlRepository)
        {
            this._identity = _identity;
            this._sqlRepository = _sqlRepository;
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

        public IEnumerable<object> GetDayStatus(string EmpType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select distinct DayType,dt.Id from DayTypeWithValues dt 
                left join DayStatusHeader dh on dh.Id=dt.HeaderId
                left join DayStatusPlantChild dc on dc.HeaderId=dh.Id
                where dt.ManualStatusAllowed=1 and dc.EmpTypeId='"+EmpType+"' and dc.PlantId='"+identity.PlantId+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public RTx Save(List<AttendanceProcessNewProcess> data , string Remarks)
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

                var identity = _identity;
                
                  

                DataTable NewShiftStandardTime = getDateWiseShift(DataToBeSaved);
                //validations
                foreach (AttendanceProcessNewProcess item in DataToBeSaved)
                {

                    NewShiftStandardTime.DefaultView.RowFilter = "SystemID='" + item.ShiftSystemID + "' AND WorkDate=#" + item.WorkDate + "#";
                    if (NewShiftStandardTime.DefaultView.Count > 0)
                    {
                        item.ShiftHoursWithoutOT= NewShiftStandardTime.DefaultView[0][@"ShiftHoursWithoutOT"].ToString();
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
                    return new RTx { data = DataToBeSaved, IsError = true, msg = "Error occured" };
                }
                
                saveData(DataToBeSaved, Remarks);

                return new RTx { data = data, IsError = false, msg = "Manual Shift Updated Successfully" };
               
            }
            catch (Exception ex)
            {
                return new RTx { data = data, IsError = true, msg = ex.Message };
            }

        }

        private void saveData(List<AttendanceProcessNewProcess> data, string Remarks)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var identity = _identity;
                bplib.clsGenID objId = new bplib.clsGenID();

                string man = "''";
                NewAttendanceProcessService ap = new NewAttendanceProcessService();

                DataSet dsRem;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter(@"Select * from dbo.ManualEntryRemarks where 1 = 2", out dsRem, false, "1");




                DataSet shiftchange = null;
                for (int i = 0; i < data.Count; i++)
                {
                    int kk = 0;
                    if (data[i].ShiftSystemID != data[i].ShiftSystemIDOriginal)
                    {
                        #region change shift

                        con = new ConnectionManager.clsConnection();
                        con.BeginTransaction();
                        con.getDataSet(@"SELECT * FROM AttdnProcessData  WHERE EmpSystemID = '" + data[i].Id + "' AND WorkDate = '" + data[i].WorkDate + "' ", out shiftchange);
                        con.CommitTransaction();
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
                            shiftchange.Tables[0].Rows[0]["OTComfirmBy"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["DateOTComfirm"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["IsOTComfirm"] = false;
                            shiftchange.Tables[0].Rows[0]["TargetOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["PlanOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AppliedOTLimit"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AllowedOTLimit"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["StandardOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AdditionalOt"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0].EndEdit();
                            ap.CheckerFunction(ref man, shiftchange.Tables[0].Rows[0]["RowId"].ToString());
                            kk = 1;
                        }
                        #endregion change shift

                    }
                    SaveDataSets(shiftchange);

                    string _Id = "";
                    if(kk == 1)
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
                        dr["Screen"] = "/manual-shift-new";
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
							WHERE sd.PlantID='" + identity.PlantId + @"'
                        ORDER BY dt.WorkDate, sd.SequenceNo ASC ";

            return _sqlRepository.GetDataTable(sql);
        }

        // For In/Out Entry Screen
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

                var identity = _identity;                

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
                var identity = _identity;
                bplib.clsGenID objId = new bplib.clsGenID();

                string man = "''";
                NewAttendanceProcessService ap = new NewAttendanceProcessService();
                DataSet dsRem;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter(@"Select * from dbo.ManualEntryRemarks where 1 = 2", out dsRem, false, "1");


                DataSet shiftchange = null ;
                
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
                            shiftchange.Tables[0].Rows[0]["TargetOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["PlanOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AppliedOTLimit"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AllowedOTLimit"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["StandardOT"] = DBNull.Value;
                            shiftchange.Tables[0].Rows[0]["AdditionalOt"] = DBNull.Value;

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
                                        dr["ManualInTime"]= data[i].InDate + " " + data[i].InTime;
                                        dr["ProcessIntime"] = data[i].InDate + " " + data[i].InTime;
                                        dr["OriginalManualInTime"] = data[i].InDate + " " + data[i].InTime;
                                        dr["IsManualInTime"] = true;
                                    }
                                }

                                if (data[i].OutDate + data[i].OutTime != data[i].OutDateOriginal + data[i].OutTimeOriginal)
                                {
                                    dr["OutTime"] = DBNull.Value;
                                    dr["ProcessOuttime"] = DBNull.Value;
                                    dr["ManualOutTime"] = DBNull.Value;
                                    dr["OriginalManualOutTime"] = DBNull.Value;
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

                    
                    SaveDataSets(shiftchange);
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
                        dr["Screen"] = "/attendance-process-data-new";
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


    }

    public class AttendanceProcessNewProcess : BaseModel
    {
        public string Id { get; set; } = "";
        public string InOutParam { get; set; } 
        public string ManualOt { get; set; } = "";
        public string RowId { get; set; } = "";
        public string AddedBy { get; set; } = "";
        public string DayStatusChange { get; set; } = "";
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
        public bool IsLock { get; set; } = false;
        public string AttendanceRestDetailId { get; set; } = "";
        public string EmpSystemID { get; set; } = "";
        public string DayName { get; set; } = "";
        public string DayStatusCode { get; set; } = "";
        public string WorkDate { get; set; } = "";
        public string ShiftSystemID { get; set; } = "";
        public string ShiftSystemIDOriginal { get; set; } = "";
        public string ShiftName { get; set; } = "";
        public string ShiftInTime { get; set; } = "";
        public string ShiftOutTime { get; set; } = "";
        public string ShiftDuration { get; set; }
        public string ShiftHalfDayDuration { get; set; }
        public string ShiftFullDayDuration { get; set; }
        public string ShiftHoursWithoutOT { get; set; }
        public string ShiftShortDuration { get; set; }
        public string EmployeeCategoryId { get; set; }        
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
        public string PlantID { get; set; } = "";
        public bool IsOTEntitled { get; set; } = false;
        public bool IsError { get; set; } = false;
        public string ErrorMessage { get; set; } = "";
        public string AddedFromIP { get; set; }
        public string BudgetCode { get; set; }
        public string FutureDayFlag { get; set; }
        public string PrevDayFlag { get; set; }
        public string SandwichFlag { get; set; }
        public string ShiftId { get; set; }
        public string Lineno { get; set; }
        public string SupervisorId { get; set; }
    }

    public class RTx
    {
        public bool IsError = false;
        public List<AttendanceProcessNewProcess> data = null;
        public string msg = string.Empty;
    }
}
