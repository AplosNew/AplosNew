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

        public RTx Savex(List<AttendanceProcessNewProcess> data)
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
               
                saveDatax(DataToBeSaved);


                return new RTx { data = data, IsError = false, msg = "Manual Entry Done Successfully" };
             
            }
            catch (Exception ex)
            {
                return new RTx { data = data, IsError = true, msg = ex.Message };
                
            }

        }
        private void saveDatax(List<AttendanceProcessNewProcess> data)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
               
                bplib.clsGenID objId = new bplib.clsGenID();

               
                DataSet shiftchange = null ;
                
                for (int i = 0; i < data.Count; i++)
                {
                    con = new ConnectionManager.clsConnection();
                    con.BeginTransaction();
                    con.getDataSet(@"SELECT * FROM AttdnProcessData  WHERE EmpSystemID = '" + data[i].Id + "' AND WorkDate = '" + data[i].WorkDate + "' ", out shiftchange);
                    con.CommitTransaction();

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
                            shiftchange.Tables[0].Rows[0]["ManualByWhom"] = "SuperUser";
                            shiftchange.Tables[0].Rows[0]["ManualEntryTime"] = DateTime.Now;
                            shiftchange.Tables[0].Rows[0]["ManualFlag"] = true;
                            shiftchange.Tables[0].Rows[0].EndEdit();
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
                                    if (string.IsNullOrEmpty(data[i].InTime) == false)
                                    {
                                        dr["InTime"] = data[i].InDate + " " + data[i].InTime;
                                        dr["ManualInTime"]= data[i].InDate + " " + data[i].InTime;
                                        dr["IsManualInTime"] = true;
                                    }
                                }

                                if (data[i].OutDate + data[i].OutTime != data[i].OutDateOriginal + data[i].OutTimeOriginal)
                                {
                                    dr["OutTime"] = DBNull.Value;
                                    dr["ManualOutTime"] = DBNull.Value;
                                    if (string.IsNullOrEmpty(data[i].OutTime) == false)
                                    {
                                        dr["OutTime"] = data[i].OutDate + " " + data[i].OutTime;
                                        dr["ManualOutTime"] = data[i].OutDate + " " + data[i].OutTime;
                                        dr["IsManualOutTime"] = true;
                                    }
                                }

                                dr["ManualByWhom"] = "Superuser";
                                dr["ManualEntryTime"] = DateTime.Now;
                                dr["ManualFlag"] = true;
                                
                                dr.EndEdit();
                            }
                            
                        }
                    }
                    #endregion

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(shiftchange);

                }


            }
            catch (Exception ex)
            {
                throw ex;

            }

        }


    }

    
}
