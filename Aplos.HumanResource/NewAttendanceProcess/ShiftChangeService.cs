using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class ShiftChangeService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public ShiftChangeService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        public IEnumerable<object> GetShiftData(string Plant, string Date)
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
                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),'"+Date+@"'))  AS ShiftOutTime
							,isnull(stcm.ShortDuration,sd.ShortDuration) as ShiftShortDuration
		                    ,isnull(stcm.HalfDayDuration,sd.HalfDayDuration) as ShiftHalfDayDuration
							,isnull(stcm.HoursWithoutOT,sd.HoursWithoutOT) as ShiftHoursWithoutOt,
							isnull(stcm.FullDayDuration,sd.FullDayDuration) as ShiftFullDayDuration,
                            isnull(stcm.ShiftDuration,sd.ShiftDuration) as ShiftDuration
                            
                            FROM ShiftDefination sd
                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '"+Date+@"'
							BETWEEN stcm.FromDate AND stcm.ToDate AND 
							sd.SystemID=stcm.ShiftDefinationID
                            ) AS KK
                            INNER JOIN   ShiftDefination sd ON sd.SystemID=kk.SystemID
                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '"+Date+@"'
							BETWEEN stcm.FromDate AND stcm.ToDate AND 
							sd.SystemID=stcm.ShiftDefinationID
                            WHERE sd.PlantID='"+Plant+@"'
                            ORDER BY sd.SequenceNo ASC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetExistingShiftData(string EmpId, string Date)
        {
            try
            {
                var sql = @"select distinct p.RowId,p.ShiftSystemID as ShiftId,p.InTime,
                p.OutTime,d.UserName as Shift,p.ShiftInTime,p.ShiftOutTime,p.IsLock
				from dbo.AttdnProcessData p
                left join dbo.ShiftDefination d on d.SystemID=p.ShiftSystemID				 
				where  EmpSystemID='" + EmpId+"' and WorkDate='"+Date+"'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SaveData(List<AttdnManualData> DataToSave)
        {
            try
            {
                List<AttdnManualData> items = DataToSave.ToList();

                DataSet dsRef;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from dbo.AttndManualDataFromApp where EmpSystemID='" + items[0].EmpSystemID + "' and WorkDate='" + items[0].WorkDate + "'";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                int i = 0;
                if (dsRef.Tables[0].Rows.Count == 0)
                {

                    DataRow dr = dsRef.Tables[0].NewRow();
                    i = 1;
                    dr["GroupID"] = items[0].GroupID;
                    dr["EmpSystemID"] = items[0].EmpSystemID;
                    dr["WorkDate"] = items[0].WorkDate;
                    dr["DayStatus"] = DBNull.Value;
                    dr["ShiftSystemId"] = items[0].ShiftSystemId;

                    if (items[0].InTime != null)
                    {
                        dr["InTime"] = items[0].InTime;
                    }
                    else
                    {
                        dr["InTime"] = DBNull.Value;
                    }
                    if (items[0].OutTime != null)
                    {
                        dr["OutTime"] = items[0].OutTime;
                    }
                    else
                    {
                        dr["OutTime"] = DBNull.Value;
                    }

                    dr["AddedBy"] = items[0].AddedBy;
                    dr["DateAdded"] = DateTime.Now.ToString();


                    dsRef.Tables[0].Rows.Add(dr);

                }
                else
                {
                    i = 1;
                    DataRow dr = dsRef.Tables[0].Rows[0];
                    dr.BeginEdit();

                    dr["DayStatus"] = DBNull.Value;
                    dr["ShiftSystemId"] = items[0].ShiftSystemId;

                    if (items[0].InTime != null)
                    {
                        dr["InTime"] = items[0].InTime;
                    }
                    else
                    {
                        dr["InTime"] = DBNull.Value;
                    }
                    if (items[0].OutTime != null)
                    {
                        dr["OutTime"] = items[0].OutTime;
                    }
                    else
                    {
                        dr["OutTime"] = DBNull.Value;
                    }


                    dr["UpdatedBy"] = items[0].AddedBy;
                    dr["DateUpdated"] = DateTime.Now.ToString();

                    dr.EndEdit();

                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);
                if (i == 1)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        
        public string Save(List<AttendanceProcessNewProcess> data)
        {
            try
            {
                DataSet shiftchange;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from dbo.AttdnProcessData where RowId='" + data[0].RowId + "'" ;
                objCon.OpenDataSetThroughAdapter(strSql, out shiftchange, false, "1");
                int i = 0;

                if (shiftchange.Tables[0].Rows.Count > 0)
                {
                        shiftchange.Tables[0].Rows[0].BeginEdit();
                        shiftchange.Tables[0].Rows[0]["ShiftSystemID"] = data[0].ShiftSystemID;
                        shiftchange.Tables[0].Rows[0]["ManualShiftId"] = data[0].ShiftSystemID;
                        shiftchange.Tables[0].Rows[0]["ShiftDuration"] = data[0].ShiftDuration;
                        shiftchange.Tables[0].Rows[0]["ShiftShortDuration"] = data[0].ShiftShortDuration;
                        shiftchange.Tables[0].Rows[0]["ShiftHoursWithoutOT"] = data[0].ShiftHoursWithoutOT;
                        shiftchange.Tables[0].Rows[0]["ShiftFullDayDuration"] = data[0].ShiftFullDayDuration;
                        shiftchange.Tables[0].Rows[0]["ShiftHalfDayDuration"] = data[0].ShiftHalfDayDuration;
                        shiftchange.Tables[0].Rows[0]["ShiftOutTime"] = data[0].ShiftOutTime;
                        shiftchange.Tables[0].Rows[0]["ShiftInTime"] = data[0].ShiftInTime;
                        shiftchange.Tables[0].Rows[0]["ManualByWhom"] = data[0].AddedBy;
                        shiftchange.Tables[0].Rows[0]["ManualEntryTime"] = DateTime.Now;
                        shiftchange.Tables[0].Rows[0]["ManualFlag"] = true;
                        shiftchange.Tables[0].Rows[0].EndEdit();
                        i = 1;
                    
                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(shiftchange);
                if (i == 1)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }

    public class ManualOTFromAppService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public ManualOTFromAppService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public void BuildDataSet(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"select BackDays,FutureDays,GroupId from dbo.Otupdateconfiguration"; 

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        public string GetConfigurationDays()
        {
            try
            {
                MyClass master = new MyClass();
                DataSet dsref = null;
                BuildDataSet(out dsref);
                DataTable dtEmpInfo = dsref.Tables[0];
                master.BackDays =Convert.ToInt32(clsWebLib.RetValidLen(dtEmpInfo.Rows[0]["BackDays"]).ToString());
                master.FutureDays = Convert.ToInt32(clsWebLib.RetValidLen(dtEmpInfo.Rows[0]["FutureDays"]).ToString());
                master.GroupId = clsWebLib.RetValidLen(dtEmpInfo.Rows[0]["GroupId"]).ToString();
                return JsonConvert.SerializeObject(master, Formatting.Indented);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Create(IEnumerable<PhysicalVerifyModel> DataToSave)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.OTfromApp";

                int i = 0;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                string EmpId = "''";
                foreach (PhysicalVerifyModel item in DataToSave)
                {
                    EmpId += ",'" + item.EmpSystemId + "'";                   
                }

                var items = DataToSave.ToList();

                var sqly = @"select * from dbo.OTfromApp where WorkDate='"+items[0].WorkDate+" ' and EmpSystemId IN("+EmpId+")";
                con.OpenDataSetThroughAdapter(sqly, out dsMaster, false, "1");


                foreach (PhysicalVerifyModel item in DataToSave)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"EmpSystemId='" + item.EmpSystemId + "' ";

                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        clsGenID genid = new clsGenID();
                        genid.GenID(TableName, out string _Id);

                        dr["Id"] = "OT" + _Id;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["Remarks"] = DBNull.Value;
                        dr["OThour"] = item.OThour;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["WorkDate"] = item.WorkDate;
                        dr["IsConfirmed"] = false;

                        dsMaster.Tables[0].Rows.Add(dr);
                        i++;
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["Remarks"] = DBNull.Value;
                        dr["OThour"] = item.OThour;
                        dr["IsConfirmed"] = false;
                        dr["UpdatedBy"] = item.AddedBy;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["WorkDate"] = item.WorkDate;

                        dr.EndEdit();
                        i++;
                    }
                  
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return i.ToString();

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public string Save(List<AttendanceProcessNewProcess> data)
        {
            try
            {
                DataSet dsMaster;
                int i = 0;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (data.Count() == 0)
                    return "";

                string EmpId = "''";
                foreach (AttendanceProcessNewProcess item in data)
                {
                    EmpId += ",'" + item.EmpSystemID + "'";
                }

                string ReturnLockedEmp = "''";
                var items = data.ToList();
                string Date = items[0].WorkDate;
                string newformat = Convert.ToDateTime(Date).ToString("yyyyMMdd");
                var sqly = @"select * from dbo.AttdnProcessData where WorkDate='" + items[0].WorkDate + " ' and EmpSystemId IN(" + EmpId + ")";
                con.OpenDataSetThroughAdapter(sqly, out dsMaster, false, "1");

                foreach (AttendanceProcessNewProcess item in data)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + item.EmpSystemID + "'"; 

                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                    {
                        string Lock = clsWebLib.GetBoolData(dsMaster.Tables[0].DefaultView[0][@"IsLock"]).ToString();
                        string OTEntitled = clsWebLib.GetBoolData(dsMaster.Tables[0].DefaultView[0][@"IsOTEntitled"]).ToString();

                        if (Lock == "False" && OTEntitled=="True")
                        {

                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["ManualOt"] = item.ManualOt;
                            dr["ManualByWhom"] = item.AddedBy;
                            dr["ManualEntryTime"] = DateTime.Now.ToString();
                            dr["ManualFlag"] = true;

                            dr.EndEdit();
                            i++;
                        }
                        else
                        {
                            ReturnLockedEmp += ",'" + item.EmpSystemID + "'";
                        }
                    }

                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster);
                if (data.Count().ToString() == i.ToString())
                {
                    return i.ToString();
                }
                else
                {
                    return "OT Entry of :-"+ ReturnLockedEmp+ " isn't Allowed";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }

    public class MyClass
    {
        public int BackDays { get; set; }
        public int FutureDays { get; set; }
        public string GroupId { get; set; }
    }

}

