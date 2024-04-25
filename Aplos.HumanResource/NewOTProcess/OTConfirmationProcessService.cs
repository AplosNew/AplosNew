using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using bplib;
using Newtonsoft.Json;
using System.Collections.Specialized;
using Library.HumanResource.NewAttendanceProcess;

namespace Library.HumanResource.NewOTProcess

{
    public class OTConfirmationProcessService
    {

        ISqlRepository _sqlRepository;
        public OTConfirmationProcessService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region Filters DataSet
        public IEnumerable<object> getDayTypes()
        {
            try
            {
                var str = @"Select * from dbo.DayType";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetWorkDateRange(string Year,string Month,string Week)
        {
            try
            {
                var str = @"select Format(min(workdate),'dd-MMM-yyyy')FromDate,
                Format(max(workdate),'dd-MMM-yyyy')ToDate 
                from AttdnProcessData where OTMonth='"+Month+"' and OTYear='"+Year+"' and otweek='"+Week+"'";
                return _sqlRepository.GetDataCollection(str);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public object getFilters()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var str = @"Select p.Id as PlantId , p.UserName as Plant, e.Id as EntityId , e.UserName as Entity
                            from Org.Entity e 
                            left join org.Plant p on p.Id = e.PlantId where p.CompanyId = '"+identity.CompanyId+@"'
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getGridData(string Week, string FromDate, string ToDate, Dictionary<string, string> Parameters)
        {
            try
            {
                

                var str = @"select a.EmpSystemID,a.RowId,e.EmployeeCode,a.DayStatus,format(a.WorkDate ,'dd-MMM-yyyy') as WorkDate,e.PlantId,p.UserName as Plant,
                            a.InTime,a.OutTime,a.ProcessOutTime,a.IsManualOutTime,a.ProcessedOT,isnull((a.ProcessedOT*dt.OTMultiplingFactor),'0') as TargetOT,
                            isnull(PreallocatedOTHr*60,'0') as PlanOT,isnull(dt.DayLimit,'0')DayLimit,a.IsOTComfirm,
                            isnull(a.StandardOT,'0')StandardOT,isnull(a.AppliedOTLimit,'0')AppliedOTLimit,
                            isnull(a.AllowedOTLimit,'0')AllowedOTLimit,isnull(a.AdditionalOT,'0')AdditionalOT,dt.ApplicableWM,isnull(dt.MonthlyLimit,'0')MonthlyLimit,
                            --- Week Data
                            WeekLimit= case when a.OTWeek='1' then (select dt.Week1Limit)
                            when a.OTWeek='2' then (select dt.Week2Limit)
                            when a.OTWeek='2' then (select dt.Week2Limit)
                            when a.OTWeek='3' then (select dt.Week3Limit)
                            when a.OTWeek='4' then (select dt.Week4Limit) end,
                            a.OTYear,a.OTMonth,a.OTWeek,a.ManualOutTime,
                            d.UserName as Department,s.UserName as Section,ss.UserName AS SubSection,l.UserName as Designation 
                            from AttdnProcessData a left join employeeinformation e on a.EmpSystemID=e.SystemId
                            left join org.Plant p on p.Id=e.PlantId     
                            left join mst.ManpowerBudget mb on mb.id=e.BudgetCode
							left join org.Entity ent on ent.id=mb.EntityId                            
                            left join DayStatusHeader dh on dh.Id=a.DayStatusHeaderId
                            left join DayTypeWithValues dt on dt.HeaderId=dh.Id
                            left join org.Section s on s.Id=e.SectionId
                            left join ORG.SubSection ss on ss.Id=e.SubSectionId
                            left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId
                            left join org.Department d on d.Id=e.DepartmentId
                            left join PreallocatedOT pot on (pot.PlantID=e.PlantId and pot.WorkDate between '" + FromDate+@"'
                            and '"+ToDate+@"') and ISNULL(ExtendTheDayLimit,'')! =''
                            where  a.IsOTEntitled=1
                            and dt.DayType=a.DayStatus 
                            and OTWeek="+Week+@"
                            and a.WorkDate between '"+FromDate+@"' and '"+ToDate+@"'
                            and p.Id in ("+ Parameters["PlantId"] + ")  and ent.Id in (" + Parameters["EntityId"] + ") order by WorkDate asc";

                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region Main Saving Function
        public void ProcessData(string Data, string OTWeek,string SelectedOT)
        {
            try
            {                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string CGId = identity.CompanyGroupId;
                List<Dictionary<string, object>> _objects = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Data);
                var StringDates = new List<DateTime>();
                StringCollection StrDistinctEmployee = new StringCollection();

                #region To Find Week Max & Min Date

                string WorkDatesMaster = "''";

                foreach (Dictionary<string, object> AllWorkDates in _objects)
                {
                    if (AllWorkDates.ContainsKey("WorkDate"))
                    {

                        string value = AllWorkDates["WorkDate"].ToString();
                        string Param = "";
                        DistinctFunction(ref WorkDatesMaster, value, out Param);
                        if (Param == "1")
                        {
                            StringDates.Add(Convert.ToDateTime(value));
                        }
                    }
                }
                string WeekMaxDate = StringDates.Max(date => date).ToString("dd-MMM-yyyy");
                string WeekMinDate = StringDates.Min(date => date).ToString("dd-MMM-yyyy");

                #endregion

                #region List to DataTable

                DataTable MainTable = ToDataTable(_objects);

                #endregion

                #region Monthly Confirmed OT

                DataSet MonthData;
                MonthlyOTData(out MonthData, WeekMinDate,OTWeek);

                #endregion

                #region PlantWise Iteration & Saving

                NewAttendanceProcessService repo = new NewAttendanceProcessService();                
                DataSet PlantList;
                repo.GetPlant(CGId, out PlantList);

                for (int k = 0; k < PlantList.Tables[0].Rows.Count; k++)
                {
                  
                    var PlantValue = PlantList.Tables[0].Rows[k][@"PlantValue"].ToString();
                    
                    MainTable.DefaultView.RowFilter = @"PlantId="+PlantValue;
                    if (MainTable.DefaultView.Count > 0)
                    {

                        DataTable Table = MainTable.Select("PlantId = '" + PlantValue + "'").CopyToDataTable();

                        SaveLog("OT Confirmation Process Start ....", PlantValue, false);

                        #region DataTable Traversing

                        if (Table.Rows.Count > 0)
                        {
                            for (int i = 0; i < Table.Rows.Count; i++)
                            {

                                decimal MonthlyConfirmedOT = 0;
                                #region Distinct Employees

                                string EmpId = Table.Rows[i][@"EmpSystemId"].ToString();

                                if (StrDistinctEmployee.Contains(EmpId))
                                {
                                    continue;
                                }

                                StrDistinctEmployee.Add(EmpId);

                                #endregion

                                #region Calculations Area

                                DateTime MaxDate = StringDates.Max(date => date);
                                DateTime MinDate = StringDates.Min(date => date);


                                if (MonthData.Tables[0].Rows.Count > 0)
                                {
                                    MonthData.Tables[0].DefaultView.RowFilter = @"EmpId='" + EmpId + "' ";
                                    if (MonthData.Tables[0].DefaultView.Count > 0)
                                    {
                                        MonthlyConfirmedOT = Convert.ToDecimal(MonthData.Tables[0].DefaultView[0][@"MonthlyConfirmedOT"].ToString());
                                    }
                                }

                                while (MinDate <= MaxDate)
                                {
                                    string ApplicablePattern = clsWebLib.RetValidLen(Table.Rows[i]["ApplicableWM"]).ToString();
                                    string FormatDate = MinDate.ToString("dd-MMM-yyyy");
                                    decimal WeekStandardOTMaster = 0;
                                    decimal MonthStandardOTMaster = 0;

                                    #region Confirmed OT Find Out

                                    if (ApplicablePattern == "W")
                                    {
                                        Table.DefaultView.RowFilter = @"EmpSystemID='" + EmpId + "' AND IsOTComfirm=true AND WorkDate <>#" + FormatDate + "# " +
                                        "AND WorkDate >= #" + WeekMinDate + "# and WorkDate<= #" + WeekMaxDate + "# ";

                                        if (Table.DefaultView.Count > 0)
                                        {
                                            for (int j = 0; j < Table.DefaultView.Count; j++)
                                            {
                                                string ApplicableWM = Table.DefaultView[j][@"ApplicableWM"].ToString();
                                                if (ApplicableWM == "W")
                                                {
                                                    // Sum Up the Week Confirmed StandardOT                                    
                                                    WeekStandardOTMaster += Convert.ToDecimal(Table.DefaultView[j][@"StandardOT"].ToString());
                                                }
                                            }
                                        }
                                    }

                                    else if (ApplicablePattern == "M")
                                    {
                                        MonthStandardOTMaster += MonthlyConfirmedOT;

                                        Table.DefaultView.RowFilter = @"EmpSystemID='" + EmpId + "' AND IsOTComfirm=true AND WorkDate <>#" + FormatDate + "# " +
                                        "AND WorkDate >= #" + WeekMinDate + "# and WorkDate<= #" + WeekMaxDate + "# ";
                                        if (Table.DefaultView.Count > 0)
                                        {
                                            for (int j = 0; j < Table.DefaultView.Count; j++)
                                            {
                                                // Sum Up the Month Confirmed StandardOT
                                                MonthStandardOTMaster += Convert.ToDecimal(Table.DefaultView[j][@"StandardOT"].ToString());
                                            }
                                        }

                                    }

                                    #endregion

                                    Table.DefaultView.RowFilter = @"EmpSystemID='" + EmpId + "'AND IsOTComfirm=false AND WorkDate =#" + FormatDate + "# ";
                                    if (Table.DefaultView.Count > 0)
                                    {

                                        DataRow dr = Table.DefaultView[0].Row;
                                        dr.BeginEdit();

                                        #region Variables 

                                        decimal TargetOT = Convert.ToDecimal(Table.DefaultView[0][@"TargetOT"].ToString());
                                        decimal DayLimit = Convert.ToDecimal(Table.DefaultView[0][@"DayLimit"].ToString());
                                        decimal WeekLimit = Convert.ToDecimal(Table.DefaultView[0][@"WeekLimit"].ToString());
                                        decimal PlanOT = Convert.ToDecimal(Table.DefaultView[0][@"PlanOT"].ToString());
                                        decimal MonthlyLimit = Convert.ToDecimal(Table.DefaultView[0][@"MonthlyLimit"].ToString());
                                        string ProcessOutTime = clsWebLib.RetValidLen(Table.DefaultView[0][@"ProcessOutTime"]).ToString();

                                        #endregion

                                        #region AllowedOT Limit  

                                        string WkDateApplicableWM = Table.DefaultView[0][@"ApplicableWM"].ToString();
                                        if (DayLimit == 0) // Say If DayType is W Or H 
                                        {
                                            // Entire Target OT In Additional OT && Standard OT ->0               
                                            dr["AllowedOTLimit"] = 0;
                                        }
                                        else if (WkDateApplicableWM == "W")
                                        {
                                            // Min of Balance Limit of Week & DailyLimit
                                            decimal SmallerValue = Math.Min(WeekLimit - WeekStandardOTMaster, DayLimit);
                                            if (SmallerValue > 0)
                                            {
                                                dr["AllowedOTLimit"] = SmallerValue;
                                            }
                                            else
                                            {
                                                dr["AllowedOTLimit"] = 0;
                                            }

                                        }
                                        else if (WkDateApplicableWM == "M")
                                        {
                                            // Min of Balance Limit of Month & DailyLimit                                
                                            decimal SmallerValue = Math.Min(MonthlyLimit - MonthStandardOTMaster, DayLimit);
                                            if (SmallerValue > 0)
                                            {
                                                dr["AllowedOTLimit"] = SmallerValue;
                                            }
                                            else
                                            {
                                                dr["AllowedOTLimit"] = 0;
                                            }
                                        }

                                        #endregion

                                        #region AppliedOTLimit Calculation

                                        if (PlanOT > 0)
                                        {
                                            dr["AppliedOTLimit"] = PlanOT;
                                        }
                                        else
                                        {
                                            dr["AppliedOTLimit"] = Convert.ToDecimal(Table.DefaultView[0][@"AllowedOTLimit"].ToString());
                                        }

                                        #endregion

                                        #region Standard OT

                                        if (SelectedOT == "1")
                                        {
                                            dr["StandardOT"] = Math.Min(Convert.ToDecimal(Table.DefaultView[0][@"AppliedOTLimit"].ToString()), TargetOT);
                                        }
                                        else if (SelectedOT == "2")
                                        {
                                            dr["StandardOT"] = TargetOT;
                                        }

                                        #endregion

                                        #region Extra OT

                                        decimal StdOT = Convert.ToDecimal(Table.DefaultView[0][@"StandardOT"].ToString());
                                        decimal ExtraOT = Convert.ToDecimal(TargetOT - StdOT);
                                        if (ExtraOT >= 0)
                                        {
                                            dr["AdditionalOT"] = ExtraOT;
                                        }
                                        #endregion

                                        #region OT Confirm

                                        dr["IsOTComfirm"] = true;

                                        #endregion

                                        #region Outime Adjust

                                        if (ProcessOutTime != "")
                                        {
                                            decimal ProcessOT = Convert.ToDecimal(Table.DefaultView[0][@"ProcessedOT"].ToString());
                                            decimal ReducedMinutes = 0;
                                            if (ProcessOT > 0)
                                            {
                                                ReducedMinutes = Convert.ToDecimal(ProcessOT - StdOT);

                                                DateTime NewOutTime = Convert.ToDateTime(ProcessOutTime).AddMinutes(Convert.ToDouble(ReducedMinutes) * -1);
                                                string NewOut = NewOutTime.ToString("dd-MMM-yyyy hh:mm:ss tt");

                                                dr["IsManualOutTime"] = true;
                                                dr["OutTime"] = NewOut;
                                            }
                                        }

                                        #endregion

                                        dr.EndEdit();

                                    }

                                    MinDate = MinDate.AddDays(1);
                                }

                                #endregion

                            }
                        }
                        #endregion

                        SaveLog("OT Confirmation Calculations Done ....", PlantValue, false);

                        #region Save Data in APD

                        if (Table.Rows.Count > 0)
                        {
                            string FinalMin = StringDates.Min(date => date).ToString("dd-MMM-yyyy");
                            string FinalMax = StringDates.Max(date => date).ToString("dd-MMM-yyyy");
                            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                            var sqlx = @"select * from AttdnProcessData where PlantId='" + PlantValue + "' and IsOTComfirm=0 and WorkDate between '" + FinalMin + "' and '" + FinalMax + "'";

                            objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                            for (int i = 0; i < Table.Rows.Count; i++)
                            {
                                // Manipulated DataSet Variables
                                string RowId = Table.Rows[i][@"RowId"].ToString();
                                decimal TargetOT = Convert.ToDecimal(Table.Rows[i][@"TargetOT"].ToString());
                                decimal StdOT = Convert.ToDecimal(Table.Rows[i][@"StandardOT"].ToString());
                                decimal PlanOT = Convert.ToDecimal(Table.Rows[i][@"PlanOT"].ToString());
                                decimal AdditionalOT = Convert.ToDecimal(Table.Rows[i][@"AdditionalOT"].ToString());
                                decimal AppliedOTLimit = Convert.ToDecimal(Table.Rows[i][@"AppliedOTLimit"].ToString());
                                decimal AllowedOTLimit = Convert.ToDecimal(Table.Rows[i][@"AllowedOTLimit"].ToString());
                                string NewOut = clsWebLib.RetValidLen(Table.Rows[i][@"OutTime"]).ToString();
                                string IsManual = clsWebLib.GetBoolData(Table.Rows[i][@"IsManualOutTime"]).ToString();

                                dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                                if (dsRef.Tables[0].DefaultView.Count > 0)
                                {
                                    DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();

                                    #region Legal & Extra OT Columns

                                    dr["TargetOT"] = TargetOT;
                                    dr["PlanOT"] = PlanOT;
                                    dr["AppliedOTLimit"] = AppliedOTLimit;
                                    dr["AllowedOTLimit"] = AllowedOTLimit;
                                    dr["StandardOT"] = StdOT;
                                    dr["AdditionalOt"] = AdditionalOT;

                                    #endregion

                                    if (IsManual == "True" && NewOut!="")
                                    {
                                        dr["OutTime"] = NewOut;
                                        dr["ManualOutTime"] = NewOut; // New Out Based on OT Split 
                                        dr["IsManualOutTime"] = IsManual;
                                    }

                                    dr["IsOTComfirm"] = true;
                                    dr["OTComfirmBy"] = identity.Name;
                                    dr["DateOTComfirm"] = Convert.ToDateTime(DateTime.Now);
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);

                                    dr.EndEdit();

                                }

                            }

                            clsStaticInfo info = new clsStaticInfo();
                            info.SaveDataSets(dsRef);
                          
                        }

                        #endregion
                       
                        SaveLog("OT Confirmed ....", PlantValue, false);
                    }       
                }

                #endregion

            }
            catch (Exception ex)
            {
                SaveLog(ex.Message,"Process Crashed", true);
                throw ex;
            }
        }
        #endregion

        #region Supporting Functions
        public void DistinctFunction(ref string WorkDatesMaster, string Value, out string Param)
        {
            if (WorkDatesMaster.Contains(Value))
            {
                Param = "0";
                return;
            }
            else
            {
                Param = "1";
                WorkDatesMaster += ",'" + Value + "'";
            }
        }

        static DataTable ToDataTable(List<Dictionary<string, object>> list)
        {
            DataTable result = new DataTable();
            if (list.Count == 0)
                return result;

            result.Columns.AddRange(
                list.First().Select(r => new DataColumn(r.Key)).ToArray()
            );

            list.ForEach(r => result.Rows.Add(r.Select(c => c.Value).Cast<object>().ToArray()));

            return result;
        }
    
        public void CommonLogFunction(Exception ex)
        {
            string Message = "";
            if (ex.ToString().Length > 2000)
            {
               Message = ex.ToString().Substring(0, 2000);
            }
            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter("select * from SchedulerLog where 1=2", out DataSet dsRef, false, false, "", "1");

            DataRow dr = dsRef.Tables[0].NewRow();
            dr["ScheduleMessage"] = Message;
            dr["UserName"] = "OTConfirmation";
            dr["isError"] = true;
            dr["AddedDate"] = DateTime.Now.ToString();
            dsRef.Tables[0].Rows.Add(dr);

            clsStaticInfo info = new clsStaticInfo();
            info.SaveDataSets(dsRef);

        }

        public void MonthlyOTData(out DataSet ds,string Date,string OTWeek)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select EmpSystemID as EmpId,Isnull(Sum(StandardOT),'0')MonthlyConfirmedOT
                from AttdnProcessData where OTMonth=Month('" + Date+@"') and OTYear=Year('"+Date+@"')
                and ISNULL(daystatus,'')!='' AND IsOTComfirm=1 and otweek<>'"+OTWeek+@"'
                group by EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
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

        #endregion

        #region Report Tab
        public IEnumerable<object> getReportData(string Week, string FromDate, string ToDate, string OTConfirmationValue, string OTLimit, string Process, string ProcessValue, string DayStatus
 , string DSApp, Dictionary<string, string> Parameters)
        {
            try
            {
                string OTConfirm = "";
                if (clsWebLib.RetValidLen(OTConfirmationValue).ToString() != "" && clsWebLib.RetValidLen(OTConfirmationValue).ToString() != "2")
                {
                    OTConfirm = "and IsOTComfirm = " + OTConfirmationValue;
                }

                string isDayStatus = "";
                if (clsWebLib.RetValidLen(DSApp).ToString() != "" && clsWebLib.RetValidLen(DSApp).ToString() != "2")
                {
                    isDayStatus = "and isLock =" + DSApp;
                }

                string ProcessFil = "";
                if (clsWebLib.RetValidLen(Process).ToString() != "" && clsWebLib.RetValidLen(ProcessValue).ToString() == "")
                {
                    throw new Exception("Please Enter The Process Filter Value!!");
                }

                if (clsWebLib.RetValidLen(Process).ToString() == "" && clsWebLib.RetValidLen(ProcessValue).ToString() != "")
                {
                    throw new Exception("Please Enter The Process Filter Selection!!");
                }

                if (clsWebLib.RetValidLen(Process).ToString() != "" && clsWebLib.RetValidLen(ProcessValue).ToString() != "")
                {
                    ProcessFil = " and " + Process + ProcessValue;
                }

                string DaySt = "";
                if (clsWebLib.RetValidLen(DayStatus).ToString() != "")
                {
                    DaySt = "and a.DayStatus = '" + DayStatus + "'";
                }

                var str = @"select a.EmpSystemID,e.EmployeeCode,a.DayStatus,format(a.WorkDate ,'dd-MMM-yyyy') as WorkDate,e.PlantId,p.UserName as Plant,
                            a.InTime,a.OutTime,a.ProcessedOT,isnull((a.ProcessedOT*dt.OTMultiplingFactor),'0') as TargetOT,
                            isnull(PreallocatedOTHr*60,'0') as PlanOT,isnull(dt.DayLimit,'0')DayLimit,a.IsOTComfirm,
                            isnull(a.StandardOT,'0')StandardOT,isnull(a.AppliedOTLimit,'0')AppliedOTLimit,
                            isnull(a.AllowedOTLimit,'0')AllowedOTLimit,isnull(a.AdditionalOT,'0')AdditionalOT,dt.ApplicableWM,isnull(dt.MonthlyLimit,'0')MonthlyLimit,
                            --- Week Data
                            WeekLimit= case when a.OTWeek='1' then (select dt.Week1Limit)
                            when a.OTWeek='2' then (select dt.Week2Limit)
                            when a.OTWeek='3' then (select dt.Week3Limit)
                            when a.OTWeek='4' then (select dt.Week4Limit) end,
                            a.OTYear,a.OTMonth,a.OTWeek,a.ManualOutTime,
                            d.UserName as Department,s.UserName as Section,ss.UserName AS SubSection,l.UserName as Designation 
                            from AttdnProcessData a left join employeeinformation e on a.EmpSystemID=e.SystemId
                            left join org.Plant p on p.Id=e.PlantId
                            left join mst.ManpowerBudget mb on mb.id=e.BudgetCode
							left join org.Entity ent on ent.id=mb.EntityId                            
                            left join DayStatusHeader dh on dh.Id=a.DayStatusHeaderId
                            left join DayTypeWithValues dt on dt.HeaderId=dh.Id
                            left join org.Section s on s.Id=e.SectionId
                            left join ORG.SubSection ss on ss.Id=e.SubSectionId
                            left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId
                            left join org.Department d on d.Id=e.DepartmentId
                            left join PreallocatedOT pot on (pot.PlantID=e.PlantId and pot.WorkDate between '" + FromDate + @"'
                            and '" + ToDate + @"') and ISNULL(ExtendTheDayLimit,'')! =''
                            where  a.IsOTEntitled=1
                            and dt.DayType=a.DayStatus 
                            " + OTConfirm + @" " + isDayStatus + @"
                            " + ProcessFil + @" " + DaySt + @"
                            and OTWeek=" + Week + @"
                            and a.WorkDate between '" + FromDate + @"' and '" + ToDate + @"'
                            and p.Id in (" + Parameters["PlantId"] + ") and ent.Id in ("+ Parameters["EntityId"]+") order by WorkDate asc";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getReportDownload(string Week, string FromDate, string ToDate, string OTConfirmationValue, string OTLimit, string Process, string ProcessValue, string DayStatus
 , string DSApp, Dictionary<string, string> Parameters)
        {
            try
            {
                string OTConfirm = "";
                if (clsWebLib.RetValidLen(OTConfirmationValue).ToString() != "" && clsWebLib.RetValidLen(OTConfirmationValue).ToString() != "2")
                {
                    OTConfirm = "and IsOTComfirm = " + OTConfirmationValue;
                }

                string isDayStatus = "";
                if (clsWebLib.RetValidLen(DSApp).ToString() != "" && clsWebLib.RetValidLen(DSApp).ToString() != "2")
                {
                    isDayStatus = "and isLock =" + DSApp;
                }

                string ProcessFil = "";
                if (clsWebLib.RetValidLen(Process).ToString() != "" && clsWebLib.RetValidLen(ProcessValue).ToString() == "")
                {
                    throw new Exception("Please Enter The Process Filter Value!!");
                }

                if (clsWebLib.RetValidLen(Process).ToString() == "" && clsWebLib.RetValidLen(ProcessValue).ToString() != "")
                {
                    throw new Exception("Please Enter The Process Filter Selection!!");
                }

                if (clsWebLib.RetValidLen(Process).ToString() != "" && clsWebLib.RetValidLen(ProcessValue).ToString() != "")
                {
                    ProcessFil = " and " + Process + ProcessValue;
                }

                string DaySt = "";
                if (clsWebLib.RetValidLen(DayStatus).ToString() != "")
                {
                    DaySt = "and a.DayStatus = '" + DayStatus + "'";
                }

                var str = @"select a.EmpSystemID,e.EmployeeCode,a.DayStatus,format(a.WorkDate ,'dd-MMM-yyyy') as WorkDate,e.PlantId,p.UserName as Plant,
                            a.InTime,a.OutTime,a.ProcessedOT,isnull((a.ProcessedOT*dt.OTMultiplingFactor),'0') as TargetOT,
                            isnull(PreallocatedOTHr*60,'0') as PlanOT,isnull(dt.DayLimit,'0')DayLimit,a.IsOTComfirm,
                            isnull(a.StandardOT,'0')StandardOT,isnull(a.AppliedOTLimit,'0')AppliedOTLimit,
                            isnull(a.AllowedOTLimit,'0')AllowedOTLimit,isnull(a.AdditionalOT,'0')AdditionalOT,dt.ApplicableWM,isnull(dt.MonthlyLimit,'0')MonthlyLimit,
                            --- Week Data
                            WeekLimit= case when a.OTWeek='1' then (select dt.Week1Limit)
                            when a.OTWeek='2' then (select dt.Week2Limit)
                            when a.OTWeek='3' then (select dt.Week3Limit)
                            when a.OTWeek='4' then (select dt.Week4Limit) end,
                            a.OTYear,a.OTMonth,a.OTWeek,a.ManualOutTime,
                            d.UserName as Department,s.UserName as Section,ss.UserName AS SubSection,l.UserName as Designation 
                            from AttdnProcessData a left join employeeinformation e on a.EmpSystemID=e.SystemId
                            left join org.Plant p on p.Id=e.PlantId
                            left join mst.ManpowerBudget mb on mb.id=e.BudgetCode
							left join org.Entity ent on ent.id=mb.EntityId
                            left join DayStatusHeader dh on dh.Id=a.DayStatusHeaderId
                            left join DayTypeWithValues dt on dt.HeaderId=dh.Id
                            left join org.Section s on s.Id=e.SectionId
                            left join ORG.SubSection ss on ss.Id=e.SubSectionId
                            left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId
                            left join org.Department d on d.Id=e.DepartmentId
                            left join PreallocatedOT pot on (pot.PlantID=e.PlantId and pot.WorkDate between '" + FromDate + @"'
                            and '" + ToDate + @"') and ISNULL(ExtendTheDayLimit,'')! =''
                            where  a.IsOTEntitled=1
                            and dt.DayType=a.DayStatus 
                            " + OTConfirm + @" " + isDayStatus + @"
                            " + ProcessFil + @" " + DaySt + @"
                            and OTWeek=" + Week + @"
                            and a.WorkDate between '" + FromDate + @"' and '" + ToDate + @"'
                            and p.Id in (" + Parameters["PlantId"] + ") and ent.Id in (" + Parameters["EntityId"] + ") order by WorkDate asc";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region OTApprove

        public IEnumerable<object> GetWorkOverStayData(string workDate,string plantId)
        {
            try
            {
                var str = @"SELECT '' Id,0 CheckBoxSelect, EI.SystemId EmployeeSystemId
                         ,EI.EmployeeCode Code
						 ,FORMAT(apd.WorkDate,'dd-MMM-yyyy') as APDEmpWorkDate
                         ,EI.EmployeeName
                         , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                         , DG.UserName LegalDesignation
                         ,E.UserName Entity,S.UserName Section, DP.UserName Department,PR.UserName PositionName
						 ,SD.ShiftDefinationName,PMB.Code BudgetCode
                         ,EI.EmployeeStatus,APD.OverStay,APD.DayStatus
						 ,CONVERT(varchar(15),CAST(APD.Intime AS TIME),100) InTime
						 ,CONVERT(varchar(15),CAST(APD.OutTime AS TIME),100) OutTime
						 ,OTTitle = case when EI.ExcludeOT=0 then 'Yes' else 'No' end
						 ,EC.UserName EmployeeCategory,OTHr=CASE WHEN ISNULL(APD.ProcessedOT,0)=0 THEN case when APD.DayTypeOTApplicable='1' then 
                (select distinct ot.AllotedOT from OTPerMinutePolicy ot
                where ot.PlantId=APD.PlantID and ot.OverstayOrEarlyOut=APD.OverStay) 
                when APD.DayTypeOTApplicable='2' then (select distinct ot.OffDayAllotedOT 
				from OTPerMinutePolicy ot
                where ot.PlantId=APD.PlantID and ot.OverstayOrEarlyOut=APD.Duration)
				when APD.DayTypeOTApplicable='3' then (select distinct ot.AllotedOT 
				from OTPerMinutePolicy ot
                where ot.PlantId=APD.PlantID and ot.OverstayOrEarlyOut=APD.OverStay-APD.EarlyIn) 
				end ELSE APD.ProcessedOT END
				,CalculatedOT=
                case when APD.DayTypeOTApplicable='1' then 
                (select distinct ot.AllotedOT from OTPerMinutePolicy ot
                where ot.PlantId=APD.PlantID and ot.OverstayOrEarlyOut=APD.OverStay) 
                when APD.DayTypeOTApplicable='2' then (select distinct ot.OffDayAllotedOT 
				from OTPerMinutePolicy ot
                where ot.PlantId=APD.PlantID and ot.OverstayOrEarlyOut=APD.Duration)
				when APD.DayTypeOTApplicable='3' then (select distinct ot.AllotedOT 
				from OTPerMinutePolicy ot
                where ot.PlantId=APD.PlantID and ot.OverstayOrEarlyOut=APD.OverStay-APD.EarlyIn) 
				end
                         FROM dbo.Employeeinformation EI
                         LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                         LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                         LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                         LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                         LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                         LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                         LEFT JOIN HKP.LegalDesignation  DG on DG.Id=EI.LegalDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId	
                         LEFT JOIN MST.DesignationMaster DM on DM.DesignationId=EI.GivenDesignationId
						 LEFT JOIN HKP.EmployeeCategory EC on EC.Id=DM.EmployeeCategoryId
                         LEFT JOIN ORG.Section S ON S.Id=EI.SectionId                         
						 LEFT JOIN dbo.AttdnProcessData APD on APD.EmpSystemID=EI.SystemId and APD.WorkDate='" + workDate + @"'
                         LEFT JOIN dbo.ShiftDefination SD ON SD.SystemID=APD.ShiftSystemID
                         WHERE EI.PlantId='" + plantId+ @"' AND ISNULL(APD.OverStay,0)<>0 AND EI.ExcludeOT=0 AND EI.SystemId IN (Select EmployeeId from dbo.ExceptionGoodWorkEmployee)
						 and APD.IsOTEntitled='1'and APD.DayTypeOTApplicable != 0 and APD.Duration>0";
                return _sqlRepository.GetDataCollection(str);

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
}