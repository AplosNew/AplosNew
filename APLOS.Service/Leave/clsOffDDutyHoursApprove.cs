using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Biometrics;
using Library.Service.Biometrics;
using Library.Service.Enums;
using Library.Service.HumanResources;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.Service.Leave
{
    public class clsOffDDutyHoursApprove
    {
        private readonly ILeaveTransectionService _leaveTransactionService;
        IMaternityLeavePolicyService LeavePolicyService;
        private DataSet dsRef;
        private readonly ISqlRepository _sqlRepository;
        public clsOffDDutyHoursApprove(ISqlRepository sqlRepository, ILeaveTransectionService leaveTransactionService)
        {
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;

        }
        public clsOffDDutyHoursApprove()
        {
        }
        void GetDetail(string empid, string leavetransactionid, string userid, string ip, out IEnumerable<LeavePolicyMaster> dList)
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
        void SetRowValue(ref DataRow dr, string Field, object v)
        {
            try
            {
                if (v is null)
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

        public void SaveDutyHour(List<OffDutyHourMasterApprove> OffDutyApprove)
        {
            DataSet dsMaster = null;
            try
            {
                SaveDutyHourMasters(OffDutyApprove, out dsMaster);
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void SaveDutyHourMasters(List<OffDutyHourMasterApprove> OffDutyApprove, out DataSet dsMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
                DutyHourMaster(out dsMaster);
                for (int i = 0; i < OffDutyApprove.Count; i++)
                {
                    DataView dvMaster = new DataView(dsMaster.Tables[0]);
                    dvMaster.RowFilter = "Id='" + OffDutyApprove[i].Id + "' ";
                    #region add
                    if (dvMaster.Count > 0)
                    {
                        DataRow dr = dvMaster[0].Row;
                        dr.BeginEdit();
                        dr["IsApprove"] = true;
                        dr["ApproveType"] = OffDutyApprove[i].ApproveType;
                        dr["DurationInHours"] = OffDutyApprove[i].DurationInHours;
                        dr.EndEdit();
                    }
                    dvMaster.RowFilter = null;
                    #endregion
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        void DutyHourMaster(out System.Data.DataSet dsRef)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM  HourlyOffDuty where  PlantId='" + identity.PlantId + "' ";
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


        public void SaveSingleEmployee(List<OffDutyHourMasterApprove> data)
        {
            //string EmpIdLoop = "";
            //foreach (var item in data)
            //{
            //    string wd = Convert.ToDateTime(item.WorkDate).ToString("dd-MMM-yyyy");
            //    if (EmpIdLoop == "")
            //    {
            //        EmpIdLoop = " (EmpSystemID= '" + item.EmpSystemId + "' and WorkDate = '" + wd + "')";

            //    }
            //    else
            //    {
            //        EmpIdLoop += " OR (EmpSystemID= '" + item.EmpSystemId + "' and WorkDate = '" + wd + "')";
            //    }
            //    //(EmpSystemID='1800086' AND WorkDate='5-Jan-20')
            //}

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            try
            {

                //DataSet dsShift = GetShiftCode(EmpIdLoop);
                //DataView dvShift = new DataView(dsShift.Tables[0]);

          

                if (data == null || data.Count == 0)
                    throw new Exception("Nothing to save");

                clsStaticInfo objStatic = new clsStaticInfo();

                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].ApproveType == "Leave")
                    {
                        //dvShift.RowFilter = "EmpSystemID=" + data[i].EmpSystemId;
                        //if (dvShift.Count == 0)
                        //{
                        //    continue;
                        //}                      
                        
                        DataSet dsLeave = null, dsAttendanceProcessData = null;
                        con = new ConnectionManager.DAL.ConManager("1");
                        con.OpenDataSetThroughAdapter("SELECT * FROM LeaveTransaction AS lt where 1=2", out dsLeave, false, "1");
                        con.OpenDataSetThroughAdapter("SELECT * FROM AttdnProcessData AS lt where EmpSystemID='" + data[i].Id + "' AND WorkDate='" + data[0].WorkDate + "'", out dsAttendanceProcessData, false, "1");
                        string _systemid = "";
                        bplib.clsGenID _id = new bplib.clsGenID();
                        _id.GenIDYearly(DateTime.Now.ToShortDateString(), "LEAVE APPLICATION", out _systemid);
                        DataRow dr = dsLeave.Tables[0].NewRow();
                        dr["SystemID"] = "LT" + _systemid;
                        dr["EmpSystemID"] = data[i].EmpSystemId;
                        dr["LTSystemID"] = data[i].EmploymentType;
                        dr["GroupID"] = identity.CompanyGroupId;
                        dr["PlantID"] = identity.PlantId;
                        dr["FromDate"] = data[i].FromDate;
                        dr["ToDate"] = data[i].ToDate;
                        //dr["LeaveDays"] = GetDuration(dvShift, data[i].Duration.ToString());
                        dr["LeaveDays"] = data[i].DurationInHours;
                        dr["LeaveDayType"] = "Hourly Off Duty";
                        dr["LvReason"] = GetLeaveReason(data[i].EmpSystemId, data[i].HourlyLeaveReasonId,data[i].WorkDate.ToString());
                        dr["AppliedDate"] = DateTime.Now.ToString("dd-MMM-yyyy");
                        dr["LeaveStatus"] = "Approved";
                        dr["UpdatedBy"] = identity.Name;
                        dr["DateUpdated"] = System.DateTime.Now;
                        dr["AddedBy"] = identity.Name;
                        dr["DateAdded"] = System.DateTime.Now;
                        dr["IsApproved"] = true;
                        dr["ApprovedBy"] = identity.Name;
                        dr["ApprovedDate"] = System.DateTime.Now;
                        dr["CompanyId"] = identity.CompanyId;

                        dsLeave.Tables[0].Rows.Add(dr);
                        DataSet dsLeaveDetails;
                        con.OpenDataSetThroughAdapter("SELECT * FROM LeaveTransactionDetails AS lt where 1=2", out dsLeaveDetails, false, "1");
                        con.OpenDataSetThroughAdapter("SELECT * FROM AttdnProcessData AS lt where EmpSystemID='" + data[i].Id + "' AND WorkDate='" + data[i].WorkDate + "'", out dsAttendanceProcessData, false, "1");

                        string _childsystemid = "";
                        _id = new bplib.clsGenID();
                        _id.GenIDYearly(DateTime.Now.ToShortDateString(), "LEAVE APPLICATION CHILD", out _childsystemid);
                        dr = dsLeaveDetails.Tables[0].NewRow();
                        dr["SystemID"] = "LT" + _childsystemid;
                        dr["LvTrnsSystemID"] = "LT" + _systemid;
                        dr["WorkDate"] = data[i].WorkDate.ToString("dd-MMM-yyyy");
                        dr["DayType"] = "NW";
                        dr["LeaveStatus"] = "LV";
                        dr["IsAvailed"] = false;
                        dr["LeaveDuration"] = data[i].DurationInHours;
                        dr["IsFirstHalf"] = false;
                        //dr["LeaveDuration"] = data[i].LeaveDays;
                        dr["IsFirstHalf"] = false;
                        dr["UpdatedBy"] = identity.Name;
                        dr["DateUpdated"] = System.DateTime.Now;
                        dr["AddedBy"] = identity.Name;
                        dr["DateAdded"] = System.DateTime.Now;
                        dr["IsAvailed"] = true;
                        dsLeaveDetails.Tables[0].Rows.Add(dr);
                        objStatic.SaveDataSets(dsLeave, dsLeaveDetails);

                        #region Attendance process
                        clsAttendance.AttendanceProcessAplos obj = new AttendanceProcessAplos();
                        DateTime ed = data[i].WorkDate;
                        obj.SaveTotal(identity.PlantId, ed.ToString("dd-MMM-yyyy"), data[i].EmpSystemId, false, true);//Main Function for attendace Process
                        #endregion
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        //public decimal GetDuration(DataView dvShift,string Duration)
        //{
        //    decimal CalDuration = 0;
        //    decimal DurationResult = 0;

        //    try
        //    {
        //        string InTime = dvShift[0]["InTime"].ToString();
        //        string OutTime = dvShift[0]["OutTime"].ToString();
        //        int BreakPeriod =Convert.ToInt32( dvShift[0]["BreakPeriod"]);
        //        bool ISIncludeBreakTimeInOT =Convert.ToBoolean( dvShift[0]["IncludeBreakTimeInOT"].ToString());
        //        DateTime NewOutTime;
        //        //string _Work_Duration;

        //        string ppDate =DateTime.Now.ToString("dd-MMM-yyyy");
        //        string it = ppDate + " " + Convert.ToDateTime(InTime).ToString("HH:mm:ss");
        //        string ot = ppDate + " " + Convert.ToDateTime(OutTime).ToString("HH:mm:ss");

        //        ///calculation
        //        if (Convert.ToDateTime(ot) < Convert.ToDateTime(it))
        //        {
        //            NewOutTime = Convert.ToDateTime(ot).AddDays(1);
        //        }
        //        else
        //        {
        //            NewOutTime = Convert.ToDateTime(OutTime);
        //        }

        //        TimeSpan tsOT = NewOutTime - Convert.ToDateTime(InTime);
        //        //_Work_Duration = ((tsOT.Hours * 60) + tsOT.Minutes);
        //        int _Work_Duration = (((tsOT.Days* 60) * 24) +(tsOT.Hours * 60) + tsOT.Minutes);
        //        int _Work_Duration_WithDeduction = (((tsOT.Days* 60) * 24) +(tsOT.Hours * 60) + tsOT.Minutes)- BreakPeriod;

        //        if (!string.IsNullOrEmpty(Duration))
        //        {
        //            DurationResult = Convert.ToDecimal(Duration);
        //        }

        //        if (ISIncludeBreakTimeInOT == false)
        //        {
        //            CalDuration = DurationResult / Convert.ToDecimal(_Work_Duration_WithDeduction);
        //        }
        //        else
        //        {
        //            CalDuration = DurationResult / Convert.ToDecimal(_Work_Duration);
        //        }
        //        return CalDuration;

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
    
        private DataSet GetShiftCode(string EmpIdLoop)
        {
            //string wd = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy");
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"    select ES.EmpSystemID,S.UserName,es.ShiftSystemID, S.WorkingHour,s.BreakPeriod,s.IncludeBreakTimeInOT,(CAST( S.WorkingHour AS int)-CAST(s.BreakPeriod AS int)) AS WithOutBreakPriod
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
                               WHERE " + EmpIdLoop + " ";
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

        private string GetLeaveReason(string EmpSystemId ,string HourlyLeaveReasonId, string WorkDate)
        {
            string result = string.Empty;
            string wd = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy");
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" select hl.UserName
                             from  HourlyOffDuty h
                            left join [HKP].[HourlyLeaveReason] hl on hl.Id=h.HourlyLeaveReasonId
                            where h.EmpSystemId='"+ EmpSystemId + "' AND h.HourlyLeaveReasonId='"+HourlyLeaveReasonId+ "'  AND h.WorkDate='"+ wd + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count>0)
                {
                    result = dsRef.Tables[0].Rows[0]["UserName"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
            return result;
        }//End Function


        #endregion

    }

    public class OffDutyHourMasterApprove
    {
        public string Id { get; set; }
        public string EmpSystemId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int DurationInMin { get; set; }
        public string PlantId { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string ApproveType { get; set; }
        public string HourlyLeaveReasonId { get; set; }
        public DateTime WorkDate { get; set; }
        public string EmploymentType { get; set; }
        public decimal DurationInHours { get; set; }

    }
}
