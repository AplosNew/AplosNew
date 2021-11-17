using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using bplib;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace Library.HumanResource.NewAttendanceProcess

{
    public class OTConfirmationProcessService
    {

        ISqlRepository _sqlRepository;
        public OTConfirmationProcessService()
        {
            _sqlRepository = new SqlRepository();
        }


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

        public IEnumerable<object> getGridData(string Week, string FromDate, string ToDate, string OTConfirmationValue, string OTLimit, string Process, string ProcessValue, string DayStatus
 , string DSApp, Dictionary<string, string> Parameters)
        {
            try
            {
                string OTConfirm = "";
                if(clsWebLib.RetValidLen(OTConfirmationValue).ToString() != "" && clsWebLib.RetValidLen(OTConfirmationValue).ToString() != "2")
                {
                    OTConfirm = "and IsOTComfirm = " + OTConfirmationValue;
                }

                string isDayStatus = "";
                if (clsWebLib.RetValidLen(DSApp).ToString() != "" && clsWebLib.RetValidLen(DSApp).ToString() != "2") 
                {
                    isDayStatus = "and isLock =" + DSApp;
                }

                string ProcessFil = "";
                if(clsWebLib.RetValidLen(Process).ToString() != "" && clsWebLib.RetValidLen(ProcessValue).ToString() == "")
                {
                    throw new Exception("Please Enter The Process Filter Value!!");
                }

                if (clsWebLib.RetValidLen(Process).ToString() == "" && clsWebLib.RetValidLen(ProcessValue).ToString() != "")
                {
                    throw new Exception("Please Enter The Process Filter Selection!!");
                }

                if(clsWebLib.RetValidLen(Process).ToString() != "" && clsWebLib.RetValidLen(ProcessValue).ToString() != "")
                {
                    ProcessFil = " and "+Process + ProcessValue;
                }

                string DaySt = "";
                if(clsWebLib.RetValidLen(DayStatus).ToString() != "" )
                {
                    DaySt = "and a.DayStatus = '"+DayStatus+"'";
                }

                var str = @"select a.EmpSystemID,e.EmployeeCode,a.DayStatus,format(a.WorkDate ,'dd-MMM-yyyy') as WorkDate,e.PlantId,p.UserName as Plant,
                            a.InTime,a.OutTime,a.ProcessedOT,isnull((a.ProcessedOT*dt.OTMultiplingFactor),'0') as TargetOT,
                            isnull(PreallocatedOTHr*60,'0') as PlanOT,isnull(dt.DayLimit,'0')DayLimit,a.IsOTComfirm,
                            isnull(a.StandardOT,'0')StandardOT,isnull(a.AppliedOTLimit,'0')AppliedOTLimit,
                            isnull(a.AllowedOTLimit,'0')AllowedOTLimit,isnull(a.AdditionalOT,'0')AdditionalOT,dt.ApplicableWM,
                            --- Week Data
                            WeekLimit= case when a.OTWeek='1' then (select dt.Week1Limit)
                            when a.OTWeek='2' then (select dt.Week2Limit)
                            when a.OTWeek='3' then (select dt.Week3Limit)
                            when a.OTWeek='4' then (select dt.Week4Limit) end,
                            a.OTYear,a.OTMonth,a.OTWeek,
                            d.UserName as Department,s.UserName as Section,ss.UserName AS SubSection,l.UserName as Designation 
                            from AttdnProcessData a left join employeeinformation e on a.EmpSystemID=e.SystemId
                            left join org.Plant p on p.Id=e.PlantId
                            left join mst.DesignationMasterLegalDesignation ddm on
                            ddm.LegalDesignationId = e.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
                            left join DayStatusPlantChild dc on dc.EmpTypeId=dm.EmployeeCategoryId
                            and dc.PlantId=e.PlantId
                            left join DayStatusHeader dh on dh.Id=dc.headerId
                            left join DayTypeWithValues dt on dt.HeaderId=dh.Id
                            left join org.Section s on s.Id=e.SectionId
                            left join ORG.SubSection ss on ss.Id=e.SubSectionId
                            left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId
                            left join org.Department d on d.Id=e.DepartmentId
                            left join PreallocatedOT pot on (pot.PlantID=e.PlantId and pot.WorkDate between '" + FromDate+@"'
                            and '"+ToDate+@"') and ISNULL(ExtendTheDayLimit,'')! =''
                            where  IsOTEntitled=1
                            and dt.DayType=a.DayStatus 
                            "+OTConfirm+@" "+isDayStatus+@"
                            "+ProcessFil+@" "+DaySt+@"
                            and OTWeek="+Week+@"
                            and a.WorkDate between '"+FromDate+@"' and '"+ToDate+@"'
                            and p.Id in ("+ Parameters["PlantId"] + ") order by WorkDate asc";

                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public void ProcessData(string Data, string OTWeek,string SelectedOT)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<Dictionary<string, object>> _objects = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(Data);
                var StringDates = new List<DateTime>();
                StringCollection StrDistinctEmployee = new StringCollection();

                #region To Find Max & Min Date

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

                #endregion

                #region List to DataTable

                DataTable Table = ToDataTable(_objects);

                #endregion

                #region Monthly Confirmed OT

                //DataTable MonthData=new DataTable();
                //MonthData.Columns.Add("EmpSystemId");
                //MonthData.Columns.Add("MonthlyConfirmedOT");
                //MonthlyOTData(out MonthData,StringDates.Min(date => date).ToString("dd-MMM-yyyy"));
                
                #endregion

                for (int i = 0; i < Table.Rows.Count; i++)
                {

                    #region Distinct Employees 
                    string EmpId = Table.Rows[i][@"EmpSystemId"].ToString();

                    if (StrDistinctEmployee.Contains(EmpId))
                    {
                        continue;
                    }

                    StrDistinctEmployee.Add(EmpId);
                    #endregion

                    string WeekMaxDate = StringDates.Max(date => date).ToString("dd-MMM-yyyy");
                    string WeekMinDate = StringDates.Min(date => date).ToString("dd-MMM-yyyy");

                    DateTime MaxDate = StringDates.Max(date => date);
                    DateTime MinDate = StringDates.Min(date => date);

                    while (MinDate <= MaxDate)
                    {
                        string ApplicablePattern = clsWebLib.RetValidLen(Table.Rows[i]["ApplicableWM"]).ToString();
                        string FormatDate = MinDate.ToString("dd-MMM-yyyy");
                        decimal WeekStandardOTMaster = 0;

                        if (ApplicablePattern == "W")
                        {
                            Table.DefaultView.RowFilter = @"EmpSystemID='" + EmpId + "' AND IsOTConfirm=1 AND WorkDate <>#" + FormatDate + "# " +
                            "AND WorkDate >= #" + WeekMinDate + "# and WorkDate<= #" + WeekMaxDate + "# ";

                            if (Table.DefaultView.Count > 0)
                            {
                                for (int j = 0; j < Table.DefaultView.Count; j++)
                                {
                                    string ApplicableWM = Table.DefaultView[j][@"ApplicableWM"].ToString();
                                    if (ApplicableWM == "W")
                                    {
                                        // Sum Up the Week StandardOT
                                        decimal StandardOT = Convert.ToDecimal(Table.DefaultView[j][@"StandardOT"].ToString());
                                        WeekStandardOTMaster += StandardOT;
                                    }
                                }
                            }
                        }

                           
                        Table.DefaultView.RowFilter = @"EmpSystemID='" + EmpId + "'AND IsOTConfirm=0 AND WorkDate =#" + FormatDate + "# ";
                        if (Table.DefaultView.Count > 0)
                        {
                               
                            DataRow dr = Table.DefaultView[0].Row;
                            dr.BeginEdit();

                            decimal TargetOT = Convert.ToDecimal(Table.DefaultView[0][@"TargetOT"].ToString());
                            decimal DayLimit = Convert.ToDecimal(Table.DefaultView[0][@"DayLimit"].ToString());
                            decimal WeekLimit = Convert.ToDecimal(Table.DefaultView[0][@"WeekLimit"].ToString());
                            decimal PlanOT = Convert.ToDecimal(Table.DefaultView[0][@"PlanOT"].ToString());

                            #region AllowedOT Limit  

                                if (DayLimit == 0) // Say If DayType is W Or H 
                                {     
                                    // Entire Target OT In Additional OT && Standard OT ->0               
                                    dr["AllowedOTLimit"] = 0;
                                }

                                string WkDateApplicableWM = Table.DefaultView[0][@"ApplicableWM"].ToString();
                                if (WkDateApplicableWM == "W")
                                {
                                    // Min of Balance Limit of Week & DailyLimit
                                    decimal BalanceWeekLimit = WeekLimit - WeekStandardOTMaster;
                                    decimal SmallerValue= Math.Min(BalanceWeekLimit, DayLimit);
                                    
                                    dr["AllowedOTLimit"] = SmallerValue;
                                }

                            #endregion

                            #region AppliedOTLimit Calculation

                                if (PlanOT>0)
                                {
                                    dr["AppliedOTLimit"] = PlanOT;
                                }
                                else
                                {
                                    decimal Allowed = Convert.ToDecimal(Table.DefaultView[0][@"AllowedOTLimit"].ToString());
                                    dr["AppliedOTLimit"] = Allowed;
                                }

                            #endregion

                            #region Standard OT

                            if (SelectedOT == "1")
                            {
                                decimal AppliedChecker = Convert.ToDecimal(Table.DefaultView[0][@"AppliedOTLimit"].ToString());
                                decimal MinValue = Math.Min(AppliedChecker, TargetOT);
                                dr["StandardOT"] = MinValue;
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
                            dr["OTComfirmBy"] = identity.Name;
                            dr["DateOTComfirm"] = DateTime.Now;
                            dr.EndEdit();

                            #endregion

                        }

                        MinDate = MinDate.AddDays(1);
                    }

                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

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

        public void MonthlyOTData(out DataTable ds,string Date)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select EmpSystemID,Isnull(Sum(StandardOT),'0')MonthlyConfirmedOT
                from AttdnProcessData where OTMonth=Month('"+Date+@"') and OTYear=Year('"+Date+@"')
                and ISNULL(daystatus,'')!='' AND IsOTComfirm=1
                group by EmpSystemID";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataTableThroughAdapter(sql,out ds);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

    }
}