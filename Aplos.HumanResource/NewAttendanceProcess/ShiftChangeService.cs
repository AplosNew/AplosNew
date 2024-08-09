using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Model.EmployeeServices;

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

        private void PhysicalVerificationFuture(IEnumerable<PhysicalVerifyModel> Future)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.PhysicalVerification";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");               

                string EmpId = "''";
                foreach (PhysicalVerifyModel item in Future)
                {
                    EmpId += ",'" + item.EmpSystemId + "'";
                }

                var items = Future.ToList();

                var sqly = @"select * from dbo.PhysicalVerification where WorkDate='" + items[0].WorkDate + " ' and EmpSystemId IN(" + EmpId + ")";
                con.OpenDataSetThroughAdapter(sqly, out dsMaster, false, "1");


                foreach (PhysicalVerifyModel item in Future)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"EmpSystemId='" + item.EmpSystemId + "' ";

                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        clsGenID genid = new clsGenID();
                        genid.GenID(TableName, out string _Id);                      
                      //  string ExistingIn = clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0][@"InTime"]).ToString();


                        if (item.InOutParam=="In")
                        {
                            dr["Id"] = "PHY" + _Id;
                            dr["EmpSystemID"] = item.EmpSystemId;
                            dr["WorkDate"] = item.WorkDate;
                            dr["InTime"] = DateTime.Now;
                            dr["BudgetCode"] = clsWebLib.RetValidLen(item.BudgetCode);
                            dr["AddedBy"] = item.AddedBy;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = item.AddedFromIP;

                        }
                        else
                        {
                            dr["Id"] = "PHY" + _Id;
                            dr["EmpSystemID"] = item.EmpSystemId;
                            dr["WorkDate"] = item.WorkDate;
                            dr["OutTime"] = DateTime.Now;
                            if (clsWebLib.RetValidLen(item.OThour).ToString() != "")
                            {
                                dr["OThour"] = item.OThour;
                            }
                            else
                            {
                                dr["OThour"] = DBNull.Value;
                            }
                            dr["AddedBy"] = item.AddedBy;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = item.AddedFromIP;

                        }

                        dsMaster.Tables[0].Rows.Add(dr);
                        
                    }
                    else
                    {
                        //string ExistingIn = clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0][@"InTime"]).ToString();

                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                       
                        if (item.InOutParam == "In")
                        {                            
                            dr["EmpSystemID"] = item.EmpSystemId;
                            dr["WorkDate"] = item.WorkDate;
                            dr["InTime"] = DateTime.Now;
                            dr["BudgetCode"] = clsWebLib.RetValidLen(item.BudgetCode);
                            dr["UpdatedBy"] = item.AddedBy;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = item.AddedFromIP;

                        }
                        else
                        {
                            dr["EmpSystemID"] = item.EmpSystemId;
                            dr["WorkDate"] = item.WorkDate;
                            dr["OutTime"] = DateTime.Now;
                            if (clsWebLib.RetValidLen(item.OThour).ToString() != "")
                            {
                                dr["OThour"] = item.OThour;
                            }
                            else
                            {
                                dr["OThour"] = DBNull.Value;
                            }
                            dr["UpdatedBy"] = item.AddedBy;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = item.AddedFromIP;

                        }

                        dr.EndEdit();                        
                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);                

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string BusVerificationFuture(IEnumerable<PhysicalVerifyModel> Future)
        {
            try
            {
                DataSet dsMaster;
                string TableName = "dbo.busverification";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string EmpId = "''";
                foreach (PhysicalVerifyModel item in Future)
                {
                    EmpId += ",'" + item.EmpSystemId + "'";
                }

                var items = Future.ToList();

                var sqly = @"select * from dbo.PhysicalVerification where WorkDate='" + items[0].WorkDate + " ' and EmpSystemId IN(" + EmpId + ")";
                con.OpenDataSetThroughAdapter(sqly, out dsMaster, false, "1");


                foreach (PhysicalVerifyModel item in Future)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"EmpSystemId='" + item.EmpSystemId + "' ";

                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        clsGenID genid = new clsGenID();
                        genid.GenID(TableName, out string _Id);
                        //  string ExistingIn = clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0][@"InTime"]).ToString();


                        if (item.InOutParam == "In")
                        {
                            dr["Id"] = "PHY" + _Id;
                            dr["EmpSystemID"] = item.EmpSystemId;
                            dr["WorkDate"] = item.WorkDate;
                            dr["InTime"] = DateTime.Now;
                            dr["AddedBy"] = item.AddedBy;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = item.AddedFromIP;

                        }
                        else
                        {
                            dr["Id"] = "PHY" + _Id;
                            dr["EmpSystemID"] = item.EmpSystemId;
                            dr["WorkDate"] = item.WorkDate;
                            dr["OutTime"] = DateTime.Now;
                           
                            dr["AddedBy"] = item.AddedBy;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = item.AddedFromIP;

                        }

                        dsMaster.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        //string ExistingIn = clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0][@"InTime"]).ToString();

                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        if (item.InOutParam == "In")
                        {
                            dr["EmpSystemID"] = item.EmpSystemId;
                            dr["WorkDate"] = item.WorkDate;
                            dr["InTime"] = DateTime.Now;
                            dr["UpdatedBy"] = item.AddedBy;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = item.AddedFromIP;

                        }
                        else
                        {
                            dr["EmpSystemID"] = item.EmpSystemId;
                            dr["WorkDate"] = item.WorkDate;
                            dr["OutTime"] = DateTime.Now;
                            dr["UpdatedBy"] = item.AddedBy;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = item.AddedFromIP;

                        }

                        dr.EndEdit();
                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return "true";
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

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                #region Saving in Physical Verification               
                PhysicalVerificationFuture(DataToSave);                
                #endregion

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

                    if (clsWebLib.RetValidLen(item.OThour).ToString() != "")
                    {
                        if (dsMaster.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            clsGenID genid = new clsGenID();
                            genid.GenID(TableName, out string _Id);

                            dr["Id"] = "OT" + _Id;
                            dr["EmpSystemId"] = item.EmpSystemId;
                            dr["Remarks"] = DBNull.Value;
                            if (clsWebLib.RetValidLen(item.OThour).ToString() != "")
                            {
                                dr["OThour"] = item.OThour;
                            }
                            else
                            {
                                dr["OThour"] = DBNull.Value;
                            }
                            dr["AddedBy"] = item.AddedBy;
                            dr["AddedDate"] = DateTime.Now.ToString();
                            dr["WorkDate"] = item.WorkDate;
                            dr["IsConfirmed"] = false;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["EmpSystemId"] = item.EmpSystemId;
                            dr["Remarks"] = DBNull.Value;
                            if (clsWebLib.RetValidLen(item.OThour).ToString() != "")
                            {
                                dr["OThour"] = item.OThour;
                            }
                            else
                            {
                                dr["OThour"] = DBNull.Value;
                            }
                            dr["IsConfirmed"] = false;
                            dr["UpdatedBy"] = item.AddedBy;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["WorkDate"] = item.WorkDate;

                            dr.EndEdit();

                        }
                    }
                  
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return "true";

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
                DataSet dsMaster,dsref,PlantMaster;
                string TableName = "dbo.PhysicalVerification";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (data.Count() == 0)
                    return "";
                int i = 0;
                string EmpId = "''";
                string PlantData = "''";

                foreach (AttendanceProcessNewProcess item in data)
                {
                    EmpId += ",'" + item.EmpSystemID + "'";                   
                }

                string ReturnLockedEmp = "''";
                var items = data.ToList();
                string Date = items[0].WorkDate;
                string newformat = Convert.ToDateTime(Date).ToString("yyyyMMdd");
               
                var sqly = @"select * from dbo.AttdnProcessData where WorkDate='" + items[0].WorkDate + "' and " +
                    "EmpSystemId IN(" + EmpId + ")";
                con.OpenDataSetThroughAdapter(sqly, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    for (int j = 0; j < dsMaster.Tables[0].Rows.Count; j++)
                    {
                        var PlantIdx = clsWebLib.RetValidLen(dsMaster.Tables[0].Rows[j][@"PlantId"]).ToString();
                        CheckerFunction(ref PlantData, PlantIdx);
                    }
                }

                var sqlz = @"select * from dbo.PhysicalVerification where WorkDate='" + items[0].WorkDate + " ' and EmpSystemID IN(" + EmpId + ")";
                con.OpenDataSetThroughAdapter(sqlz, out dsref, false, "1");

                var sqlx = @"select PlantId,ManualInAllowed,ManualOTAllowed,ManualOutAllowed from AttendanceSourceConfig 
                where PlantId In(" + PlantData+")";
                con.OpenDataSetThroughAdapter(sqlx, out PlantMaster, false, "1");


                foreach (AttendanceProcessNewProcess item in data)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + item.EmpSystemID + "'"; 

                    if (dsMaster.Tables[0].DefaultView.Count > 0)
                    {
                        string Lock = clsWebLib.GetBoolData(dsMaster.Tables[0].DefaultView[0][@"IsLock"]).ToString();
                        string OTEntitled = clsWebLib.GetBoolData(dsMaster.Tables[0].DefaultView[0][@"IsOTEntitled"]).ToString();
                        string PlantId= clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0][@"PlantID"]).ToString();
                        
                        if (Lock == "False") 
                        {
                            #region To Save in Physical Verification

                            dsref.Tables[0].DefaultView.RowFilter = @"EmpSystemID='" + item.EmpSystemID + "' ";
                            if (dsref.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = dsref.Tables[0].NewRow();

                                clsGenID genid = new clsGenID();
                                genid.GenID(TableName, out string _Id);


                                if (item.InOutParam == "In")
                                {
                                    dr["Id"] = "PHY" + _Id;
                                    dr["EmpSystemID"] = item.EmpSystemID;
                                    dr["WorkDate"] = item.WorkDate;
                                    dr["InTime"] = DateTime.Now;
                                    dr["BudgetCode"] = clsWebLib.RetValidLen(item.BudgetCode);
                                    dr["AddedBy"] = item.AddedBy;
                                    dr["AddedDate"] = DateTime.Now;
                                    dr["AddedFromIP"] = item.AddedFromIP;
                                    i++;
                                }
                                else
                                {
                                    dr["Id"] = "PHY" + _Id;
                                    dr["EmpSystemID"] = item.EmpSystemID;
                                    dr["WorkDate"] = item.WorkDate;
                                    dr["OutTime"] = DateTime.Now;
                                    if (clsWebLib.RetValidLen(item.ManualOt).ToString() != "")
                                    {
                                        dr["OThour"] = item.ManualOt;
                                    }
                                    dr["AddedBy"] = item.AddedBy;
                                    dr["AddedDate"] = DateTime.Now;
                                    dr["AddedFromIP"] = item.AddedFromIP;
                                    i++;
                                }

                                dsref.Tables[0].Rows.Add(dr);

                            }
                            else
                            {
                                DataRow dr = dsref.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();

                                if (item.InOutParam == "In")
                                {
                                    dr["EmpSystemID"] = item.EmpSystemID;
                                    dr["WorkDate"] = item.WorkDate;
                                    dr["InTime"] = DateTime.Now;
                                    dr["BudgetCode"] = clsWebLib.RetValidLen(item.BudgetCode);
                                    dr["UpdatedBy"] = item.AddedBy;
                                    dr["UpdatedDate"] = DateTime.Now;
                                    dr["UpdatedFromIP"] = item.AddedFromIP;
                                    i++;
                                }
                                else
                                {
                                    dr["EmpSystemID"] = item.EmpSystemID;
                                    dr["WorkDate"] = item.WorkDate;
                                    dr["OutTime"] = DateTime.Now;
                                    if (clsWebLib.RetValidLen(item.ManualOt).ToString() != "")
                                    {
                                        dr["OThour"] = item.ManualOt;
                                    }                                    
                                    dr["UpdatedBy"] = item.AddedBy;
                                    dr["UpdatedDate"] = DateTime.Now;
                                    dr["UpdatedFromIP"] = item.AddedFromIP;
                                    i++;
                                }

                                dr.EndEdit();
                            }

                            #endregion

                            PlantMaster.Tables[0].DefaultView.RowFilter = @"PlantId='" + PlantId + "'";
                            if (PlantMaster.Tables[0].DefaultView.Count > 0)
                            {

                                string ManualInAllowed = clsWebLib.RetValidLen(PlantMaster.Tables[0].DefaultView[0][@"ManualInAllowed"]).ToString();
                                string ManualOutAllowed = clsWebLib.RetValidLen(PlantMaster.Tables[0].DefaultView[0][@"ManualOutAllowed"]).ToString();
                                string ManualOTAllowed = clsWebLib.RetValidLen(PlantMaster.Tables[0].DefaultView[0][@"ManualOTAllowed"]).ToString();

                                DataRow drx = dsMaster.Tables[0].DefaultView[0].Row;
                                if (item.InOutParam == "In")
                                {
                                    if (ManualInAllowed != "" && ManualInAllowed == "True")
                                    {

                                        drx.BeginEdit();
                                        drx["ManualInTime"] = DateTime.Now;
                                        drx["OriginalManualInTime"] = DateTime.Now;
                                        drx["IsManualInTime"] = true; 
                                        drx["ManualByWhom"] = item.AddedBy;
                                        drx["ManualEntryTime"] = DateTime.Now.ToString();
                                        drx["OTComfirmBy"] = DBNull.Value;
                                        drx["DateOTComfirm"] = DBNull.Value;
                                        drx["IsOTComfirm"] = false;
                                        drx.EndEdit();

                                    }
                                }

                                else if (item.InOutParam == "Out")
                                {
                                    if (ManualOutAllowed != "" && ManualOutAllowed == "True")
                                    {

                                        drx.BeginEdit();
                                        drx["ManualOutTime"] = DateTime.Now;
                                        drx["OriginalManualOutTime"] = DateTime.Now;
                                        drx["IsManualOutTime"] = true;
                                        drx["ManualByWhom"] = item.AddedBy;
                                        drx["ManualEntryTime"] = DateTime.Now.ToString();
                                        drx["OTComfirmBy"] = DBNull.Value;
                                        drx["DateOTComfirm"] = DBNull.Value;
                                        drx["IsOTComfirm"] = false;
                                        drx.EndEdit();

                                    }
                                }
                                if (OTEntitled == "True")
                                {

                                    if (clsWebLib.RetValidLen(item.ManualOt).ToString() != "")
                                    {
                                        if (ManualOTAllowed != "" && ManualOTAllowed == "True")
                                        {
                                            drx.BeginEdit();
                                            drx["ManualOt"] = item.ManualOt;                                         
                                            drx.EndEdit();
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            ReturnLockedEmp += "," + item.EmpSystemID + " ";
                        }
                    }
                    else
                    {
                        ReturnLockedEmp += "," + item.EmpSystemID + " ";
                    }

                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMaster,dsref);
                if (data.Count().ToString() == i.ToString())
                {
                    return "true";
                }
                else
                {
                    return i+" Rows Updated Succesfully. Entry of :- "+ ReturnLockedEmp+ " isn't Allowed";
                }
            }
            catch (Exception ex)
            {
                SaveLog("Data:- "+data.Count().ToString()+" "+ ex.ToString(), "App", true);
                return ex.ToString();
            }
        }
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

            clsStaticInfo info = new clsStaticInfo();
            info.SaveDataSets(dsRef);
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
    }
   
    public class MyClass
    {
        public int BackDays { get; set; }
        public int FutureDays { get; set; }
        public string GroupId { get; set; }
    }

    public class PhysicalVerificationReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public PhysicalVerificationReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }


        public IEnumerable<object> GetData(string WkDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = identity.PlantId;
              
                var sql = @"select e.EmployeeCode,e.EmployeeName,pv.EmpSystemID,s.UserName as Section,ss.UserName as SubSection,
                d.UserName as Department,
                o.UserName as Unit,ld.UserName as LegalDesignation,
                format(pv.WorkDate,'dd-MMM-yyyy')as WorkDate,pv.InTime,pv.OutTime,pv.AddedBy from 
                PhysicalVerification pv left join EmployeeInformation e on e.SystemId=pv.EmpSystemID
                left join org.Department d on d.Id=e.DepartmentId
                left join org.Section s on s.Id=e.SectionId
                left join org.SubSection ss on ss.Id=e.SubSectionId
                left join org.Unit o on o.Id=e.UnitId
                left join hkp.LegalDesignation ld on ld.Id=e.LegalDesignationId
                where WorkDate='"+WkDate+@"'
                and e.PlantId='"+plantId+"'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetReportData(string WkDate,string EmpId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = identity.PlantId;
                
                var sql = @"select e.EmployeeCode,e.EmployeeName,pv.EmpSystemID,s.UserName as Section,ss.UserName as SubSection,
                d.UserName as Department,
                o.UserName as Unit,ld.UserName as LegalDesignation,
                format(pv.WorkDate,'dd-MMM-yyyy')as WorkDate,pv.InTime,pv.OutTime,pv.AddedBy from 
                PhysicalVerification pv left join EmployeeInformation e on e.SystemId=pv.EmpSystemID
                left join org.Department d on d.Id=e.DepartmentId
                left join org.Section s on s.Id=e.SectionId
                left join org.SubSection ss on ss.Id=e.SubSectionId
                left join org.Unit o on o.Id=e.UnitId
                left join hkp.LegalDesignation ld on ld.Id=e.LegalDesignationId
                where WorkDate='" + WkDate + @"'
                and e.PlantId='" + plantId + "' and isnull(e.SystemId, '') IN(" + EmpId + @")"; 

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }

    public class EmployeeFeedbackService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public EmployeeFeedbackService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        
        public IEnumerable<object> GetReasoningMaster()
        {
            try
            {
                var sql = @"select Id as Value,UserName as Text from HKP.AbsentismReasoningMaster";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Create(IEnumerable<EmployeeFeedBackModel> DataToSave)
        {

            try
            {
                DataSet dsMaster;
                string TableName = "dbo.Employeefeedback";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";

                List<EmployeeFeedBackModel> items = DataToSave.ToList();


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1=2", out dsMaster, false, "1");

                string _Id = "";

                foreach (EmployeeFeedBackModel item in DataToSave)
                {
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        clsGenID genid = new clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "EF" + _Id;
                        dr["EmpSystemId"] = item.EmpSystemId;
                        dr["Date"] = item.Date;
                        dr["ReasoningId"] = item.ReasoningId;
                        dr["Action"] = item.Action;
                        dr["Remarks"] = item.Remarks;                  
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                return MasterId;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
}

