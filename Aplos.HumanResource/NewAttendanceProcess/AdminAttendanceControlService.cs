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
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

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
                            shiftchange.Tables[0].Rows[0]["ManualByWhom"] = identity.Name;
                            shiftchange.Tables[0].Rows[0]["ManualEntryTime"] = DateTime.Now;
                            shiftchange.Tables[0].Rows[0]["ManualFlag"] = true;
                            shiftchange.Tables[0].Rows[0].EndEdit();
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
                                dr["ManualDayStatus"] = data[i].DayStatusNew;
                                dr["DayStatus"] = data[i].DayStatusNew;
                                dr["IsManualDayStatus"] = true;
                                dr["ManualByWhom"] = identity.Name;
                                dr["ManualEntryTime"] = DateTime.Now;
                                dr["ManualFlag"] = true;
                            }

                            dr.EndEdit();
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
                                    if (string.IsNullOrEmpty(data[i].InTime) == false)
                                    {
                                        dr["InTime"] = data[i].InDate + " " + data[i].InTime;
                                        dr["ManualInTime"] = data[i].InDate + " " + data[i].InTime;
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

                                dr["ManualByWhom"] = identity.Name;
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
                            KK.IsOTComfirm, KK.IsOTEntitled,KK.IsManualDayStatus,convert(bit,isnull(KK.IsLock,0)) AS IsLock

                             FROM (
								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, O.IsManualDayStatus,O.IsLock,
       
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,O.DayStatusCode,
		                            O.IsOTEntitled,emp.plantid

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

        public DataTable getCurrentFile( string PlId, string FD, string TD)
        {
            try
            {
                var str = @"select RowId,EmpSystemID,WorkDate,InTime,OutTime,ShiftSystemID,DayStatus 
                            from AttdnProcessData where WorkDate between '" + FD + @"' and '" + TD + @"'
                            AND PlantID='" + PlId + @"'";
                return _sqlRepository.GetDataTable(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void SaveFileList(List<Dictionary<string, object>> data, string PlId, string FD, string TD)
        {
            try
            {

                
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

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.AttdnProcessData";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where WorkDate between '" + FD + @"' and '" + TD + @"'AND PlantID='" + PlId + @"'", out dsMaster, false, "1");

                if (data.Count > 0)
                {
                    for(int i = 0; i < data.Count; i ++)
                    {
                        dsMaster.Tables[0].DefaultView.RowFilter = "RowId='" + data[i]["RowId"].ToString() + "'";
                        dsMaster.Tables[0].DefaultView[0].BeginEdit();
                        if(data[i]["InTime"] != null)
                        {
                            dsMaster.Tables[0].DefaultView[0]["InTime"] = Convert.ToDateTime(data[i]["InTime"].ToString());
                            dsMaster.Tables[0].DefaultView[0]["ManualInTime"] = Convert.ToDateTime(data[i]["InTime"].ToString());
                            dsMaster.Tables[0].DefaultView[0]["IsManualInTime"] = true;
                        }
                        if (data[i]["OutTime"] != null)
                        {
                            dsMaster.Tables[0].DefaultView[0]["OutTime"] = Convert.ToDateTime(data[i]["OutTime"].ToString());
                            dsMaster.Tables[0].DefaultView[0]["ManualOutTime"] = Convert.ToDateTime(data[i]["OutTime"].ToString());
                            dsMaster.Tables[0].DefaultView[0]["IsManualOutTime"] = true;
                        }

                        
                        dsMaster.Tables[0].DefaultView[0]["ShiftSystemID"] = data[i]["ShiftSystemID"].ToString();
                        if(data[i]["DayStatus"]!=null)
                        {
                            dsMaster.Tables[0].DefaultView[0]["DayStatus"] = data[i]["DayStatus"].ToString();
                            dsMaster.Tables[0].DefaultView[0]["ManualDayStatus"] = data[i]["DayStatus"].ToString();
                        }

                        dsMaster.Tables[0].DefaultView[0]["DateUpdated"] = DateTime.Now;
                        dsMaster.Tables[0].DefaultView[0]["UpdatedBy"] = identity.Name;
                        
                        dsMaster.Tables[0].DefaultView[0]["ManualFlag"] = true;
                        dsMaster.Tables[0].DefaultView[0].EndEdit();
                    }


                }
                

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
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
