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
    public class NewProcessBackendRowCreationLogic
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public NewProcessBackendRowCreationLogic()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
      
        public string SaveData(List<BackenedDataModel> DataToSave)
        {
            try
            {
                if (DataToSave.Count() == 0)
                    return "Either Data not in Correct Format or Missing....";

                List<BackenedDataModel> items = DataToSave.ToList();

                DataSet dsRef,dsPlant;
                ConnectionManager.DAL.ConManager objConx = new ConnectionManager.DAL.ConManager("1");
                string Sql = @"select * from EmployeeInformation where SystemId='"+items[0].EmpId+"'";
             
                objConx.OpenDataSetThroughAdapter(Sql, out dsPlant, false, "1");               
                var PlantxId = clsWebLib.RetValidLen(dsPlant.Tables[0].Rows[0][@"PlantId"]).ToString();
                var EmpxId = clsWebLib.RetValidLen(dsPlant.Tables[0].Rows[0][@"SystemId"]).ToString();

                string strSql = @"select * from dbo.AttdnProcessData where WorkDate BETWEEN '" + items[0].FromDate + "'" +
                    " AND '" + items[0].ToDate + "' and PlantId='"+PlantxId+"' and EmpSystemId='"+items[0].EmpId+"'";
                objConx.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");

                DateTime frmdate = Convert.ToDateTime(items[0].FromDate);
                DateTime Todat = Convert.ToDateTime(items[0].ToDate);
                DateTime finalfrm= Convert.ToDateTime(items[0].FromDate);
                int days = 0;
                while (finalfrm.AddDays(days) <= Todat)
                {
                    
                    DataSet UnProcessed;
                    
                    UnProcessedEmp(frmdate.AddDays(days).ToString("yyyy-MM-dd"), out UnProcessed, PlantxId, EmpxId);

                    if (UnProcessed.Tables[0].Rows.Count > 0)
                    {
                        var WkDate = UnProcessed.Tables[0].Rows[0][@"WorkDate"].ToString();
                        var GpId = UnProcessed.Tables[0].Rows[0][@"GroupID"].ToString();

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
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

                                // Priority Wise Shift Assignment
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
                        }
                    }
                    days += 1;
                }

                       
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);
                var Counter = dsRef.Tables[0].Rows.Count;
                if (Counter <= 1)
                {
                
                    return Counter.ToString() + " Row Uploaded... ";
                }
                else
                { 
                    return Counter.ToString() + " Rows Uploaded... ";
                        
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
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

        void UnProcessedEmp(string Date, out DataSet ds, string PlantId,string EmpId)
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
                where e.EmpType!='Guest' and e.PlantId='" + PlantId + @"' and e.SystemID='" + EmpId+"' " +
                " and DOJ <= '" + Date + "' AND (E.DOS >= '" + Date + "' OR ISNULL(E.DOS,'') = '' OR E.DOS = '01/01/1901') ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

    }

    public class BackenedDataModel 
    {
        #region Scalar Properties

        public string EmpId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string PlantId { get; set; }       

        #endregion Navigation Properties
    }

}

