using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Service.Leave
{
   public class clsOffDDutyHours
    {
        ISqlRepository _sqlRepository;
        private DataSet dsRef;

        public clsOffDDutyHours(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        public clsOffDDutyHours()
        {

        }
        void GetDetail(string empid, string leavetransactionid, string userid, string ip,out IEnumerable<LeavePolicyMaster> dList)
        {
            dList = null;
            try
            {
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SetRowValue(ref DataRow dr,string Field,object v)
        {
            try
            {
                if(v is null)
                {
                    dr[Field] = DBNull.Value;
                }
                else
                {
                    dr[Field] = v;
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SetRowValue(ref DataRow dr, object v)
        {
            try
            {
                dr[nameof(v)] = v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Off Duty Hours

        public void SaveDutyHour(OffDutyHourMaster DutyHour)
        {
            DataSet dsMaster = null;

            try
            {
                SaveDutyHourMasters(DutyHour, out dsMaster);

                clsStaticInfo obj = new clsStaticInfo();

                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void SaveEmpWiseFixedOT(EmpWiseFixedOT EmpWiseFOTSetting)
        {
            DataSet dsMaster = null;
            DataSet dsSecountTime = null;

            try
            {
                SaveEmpWiseFixedOT(EmpWiseFOTSetting, out dsMaster ,out dsSecountTime);

                clsStaticInfo obj = new clsStaticInfo();

                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        void SaveEmpWiseFixedOT(EmpWiseFixedOT EmpWiseFOTSetting, out DataSet dsMaster,out DataSet dsSecountTime)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            dsSecountTime = null;

            try
            {
                EmpFixOTSetting(EmpWiseFOTSetting.Id, out dsMaster);
                SaveSecoundTime(EmpWiseFOTSetting.EmpSystemId, out dsSecountTime);

                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "Id='" + EmpWiseFOTSetting.Id + "' ";

               
                if (dvMaster.Count == 0)
                {
                    #region add
                    if (dsSecountTime.Tables[0].Rows.Count == 1)
                    {
                        Exception ex = new Exception("Already Entered..");
                        throw (ex);
                    }

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeWiseFixedOTSetting", out sID);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    EmpWiseFOTSetting.Id = "FOT" + sID;
                    foreach (PropertyInfo prop in EmpWiseFOTSetting.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(EmpWiseFOTSetting, null));
                    }
                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit
                    EmpWiseFOTSetting.UpdatedBy = identity.Name;
                    EmpWiseFOTSetting.UpdatedDate = DateTime.Now;
                    EmpWiseFOTSetting.UpdatedFromIP = identity.IPAddress;
                    
                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in EmpWiseFOTSetting.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(EmpWiseFOTSetting, null));
                    }
                    dr.EndEdit();
                    #endregion
                }
                dvMaster.RowFilter = null;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }



        void SaveDutyHourMasters(OffDutyHourMaster DutyHour, out DataSet dsMaster)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;

            try
            {
                DataSet dsShift = GetShiftCode(DutyHour.EmpSystemId, DutyHour.WorkDate.ToString());
                DataView dvShift = new DataView(dsShift.Tables[0]);

                DutyHour.DurationInHours = GetDuration(dvShift, DutyHour.DurationInMin.ToString());

                clsAttendance.AttendanceProcessAplos obj = new AttendanceProcessAplos();
                obj.LockValidation(identity.PlantId, DutyHour.FromDate.ToString("dd-MMM-yyyy"), DutyHour.ToDate.ToString("dd-MMM-yyyy"), DutyHour.EmpSystemId);
                
                DutyHourMaster(DutyHour.Id, out dsMaster);
                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "Id='" + DutyHour.Id + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add
                   
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HourlyOffDuty", out sID);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    DutyHour.Id = "OH" + sID;
                    foreach (PropertyInfo prop in DutyHour.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(DutyHour, null));
                    }
                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit

                    

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in DutyHour.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(DutyHour, null));
                    }
                    dr.EndEdit();
                    #endregion
                }
                dvMaster.RowFilter = null;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public DataSet GetShiftCode(string EmpSystemID,string WorkDate)
        {
            string wd = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy");
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"  select ES.EmpSystemID,S.UserName,es.ShiftSystemID, S.WorkingHour,s.BreakPeriod,s.IncludeBreakTimeInOT,(CAST( S.WorkingHour AS int)-CAST(s.BreakPeriod AS int)) AS WithOutBreakPriod
                            ,s.IncludeBreakTimeInOT,s.InTime,s.OutTime,s.OutTime
                              ,ShiftOutTime = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                           ,ShiftInTime = Format(s.InTime, 'yyyy-MM-dd') + ' ' + CASE 
			               WHEN cs.InTime IS NULL
			               	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
			               ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			               END
                               from [dbo].[EmpDateWiseShiftAssign] ES
                               left join ShiftDefination s on s.SystemID=es.ShiftSystemID 
							 left join(
                               SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m
                               left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                        ) CS on cs.ShiftDefinationID = es.ShiftSystemID and cs.ShiftDate = ES.WorkDate
                               left join[ShiftDefination] sd on sd.SystemID = es.ShiftSystemID                          
                               WHERE es.EmpSystemID='"+EmpSystemID+"' and es.WorkDate='"+ wd + "' ";
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
            return dsRef;
        }//End Function

        public decimal GetDuration(DataView dvShift, string DurationInMin)
        {
            decimal CalDuration = 0;
            decimal DurationResult = 0;

            try
            {
                string InTime = dvShift[0]["InTime"].ToString();
                string OutTime = dvShift[0]["OutTime"].ToString();
                int BreakPeriod = Convert.ToInt32(dvShift[0]["BreakPeriod"]);
                bool ISIncludeBreakTimeInOT = Convert.ToBoolean(dvShift[0]["IncludeBreakTimeInOT"].ToString());
                DateTime NewOutTime;
                //string _Work_Duration;

                string ppDate = DateTime.Now.ToString("dd-MMM-yyyy");
                string it = ppDate + " " + Convert.ToDateTime(InTime).ToString("HH:mm:ss");
                string ot = ppDate + " " + Convert.ToDateTime(OutTime).ToString("HH:mm:ss");

                ///calculation
                if (Convert.ToDateTime(ot) < Convert.ToDateTime(it))
                {
                    NewOutTime = Convert.ToDateTime(ot).AddDays(1);
                }
                else
                {
                    NewOutTime = Convert.ToDateTime(OutTime);
                }

                TimeSpan tsOT = NewOutTime - Convert.ToDateTime(InTime);
                //_Work_Duration = ((tsOT.Hours * 60) + tsOT.Minutes);
                int _Work_Duration = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);
                int _Work_Duration_WithDeduction = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes) - BreakPeriod;

                if (!string.IsNullOrEmpty(DurationInMin))
                {
                    DurationResult = Convert.ToDecimal(DurationInMin);
                }

                if (ISIncludeBreakTimeInOT == false)
                {
                    CalDuration = DurationResult / Convert.ToDecimal(_Work_Duration_WithDeduction);
                }
                else
                {
                    CalDuration = DurationResult / Convert.ToDecimal(_Work_Duration);
                }
                return CalDuration;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Dictionary<string, object>> GetShiftInfo(string EmpSystemID, string WorkDate)
        {
            try
            {
                string wd = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy");

                var cmdText = @"           select ES.EmpSystemID,S.UserName,es.ShiftSystemID                            
                                    ,Format(AP.InTime,'dd-MMM-yyyy hh:mm tt')InTime,format(AP.OutTime,'dd-MMM-yyyy hh:mm tt')OutTime
                     ,AP.DayStatus                                    
                      from [dbo].[EmpDateWiseShiftAssign] ES
                      left join ShiftDefination s on s.SystemID=es.ShiftSystemID 
			                    LEFT JOIN 	AttdnProcessData AS	AP ON AP.EmpSystemID=ES.EmpSystemID AND ES.WorkDate=AP.WorkDate 						                       
                               WHERE es.EmpSystemID='" + EmpSystemID + "' and es.WorkDate='" + wd + "'  ";

                return _sqlRepository.GetDataCollection(cmdText);

            }
            catch (Exception)
            {
                throw;
            }
        }//end of function
        
        void DutyHourMaster(string Id, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM  HourlyOffDuty where ID='" + Id + @"' ";

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

        void EmpFixOTSetting(string Id, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM  EmployeeWiseFixedOTSetting where ID='" + Id + @"' ";

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

        void SaveSecoundTime(string EmpSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM  EmployeeWiseFixedOTSetting where EmpSystemId='" + EmpSystemId + @"' ";

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


        public void SaveDutyHourWithapproval(OffDutyHourMasterWithApproval DutyHour)
        {
            DataSet dsMaster = null;

            try
            {
                SaveDutyHourMastersWithApproval(DutyHour, out dsMaster);

                clsStaticInfo obj = new clsStaticInfo();

                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        void SaveDutyHourMastersWithApproval(OffDutyHourMasterWithApproval DutyHour, out DataSet dsMaster)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;

            try
            {
                DataSet dsShift = GetShiftCode(DutyHour.EmpSystemId, DutyHour.WorkDate.ToString());
                DataView dvShift = new DataView(dsShift.Tables[0]);

                DutyHour.DurationInHours = GetDuration(dvShift, DutyHour.DurationInMin.ToString());

                clsAttendance.AttendanceProcessAplos obj = new AttendanceProcessAplos();
                obj.LockValidation(identity.PlantId, DutyHour.FromDate.ToString("dd-MMM-yyyy"), DutyHour.ToDate.ToString("dd-MMM-yyyy"), DutyHour.EmpSystemId);

                DutyHourMaster(DutyHour.Id, out dsMaster);
                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "Id='" + DutyHour.Id + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HourlyOffDutyWA", out sID);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    DutyHour.Id = "OHWA" + sID;
                    foreach (PropertyInfo prop in DutyHour.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(DutyHour, null));
                    }
                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit



                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in DutyHour.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(DutyHour, null));
                    }
                    dr.EndEdit();
                    #endregion
                }
                dvMaster.RowFilter = null;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion

    }
    public class OffDutyHourMaster
    {   
         public string Id { get; set; }
         public string EmpSystemId { get; set; }
         public DateTime FromDate { get; set; }
         public DateTime ToDate { get; set; }
         public int DurationInMin { get; set; }
        public string HourlyLeaveReasonId { get; set; }
        public string PlantId { get; set; }

        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }
        public string AddedBy { get; set; }

        [NeverUpdate]
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime WorkDate { get; set; }
        public decimal DurationInHours { get; set; }        
    }
    public class OffDutyHourMasterWithApproval
    {
        public string Id { get; set; }
        public string EmpSystemId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int DurationInMin { get; set; }
        public string HourlyLeaveReasonId { get; set; }
        public string PlantId { get; set; }

        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }
        public string AddedBy { get; set; }

        [NeverUpdate]
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime WorkDate { get; set; }

        public decimal DurationInHours { get; set; }
        public bool IsApprove { get; set; }
        public string ApproveType { get; set; }
       
    }
    public class EmpWiseFixedOT
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }

        //public DateTime? EffectiveDate { get; set; }

        public string EmpSystemId { get; set; }
        //public int MaximumOT { get; set; }
        public decimal MaximumOTLimitPerWeekDay { get; set; }
        public decimal MaximumOTLimitPerHoliDay { get; set; }
        public decimal MaximumOTLimitPerWeekend { get; set; }
        public decimal MaximumOTLimitPerMonth { get; set; }
        //public bool IsExcessAllowed { get; set; }
        public bool IsMinimumOTLimit { get; set; }


        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public string AddedFromIP { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}
