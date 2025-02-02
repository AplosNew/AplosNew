using bplib;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class SandwichProcessService
    {

        ISqlRepository _sqlRepository;
        public SandwichProcessService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region DataSet Functions

        public IEnumerable<object> GetEmployeeInformationPlantWise(string month, string year, string PlantId)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
                string ToDate = Convert.ToDateTime(date).AddDays(32).ToString("dd-MMM-yyyy");


                var sql = @"select EmpSystemID,e.EmployeeCode,p.UserName as Plant,p.Id as PlantId,
                            format(WorkDate,'dd-MMM-yyyy')WorkDate,DayStatus,dp.UserName
                            as Department,s.UserName as Section,
                            SuS.UserName as SubSection,ld.UserName as Designation,SandwichFlag as TodayFlag,
                (select SandwichFlag from AttdnProcessData where WorkDate=DATEADD(day,-1,a.WorkDate) 
                and EmpSystemID=a.EmpSystemID)PrevDayFlag
                from attdnprocessdata a
                left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                left join org.Plant p on p.Id=e.PlantId
                LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                left join org.Section s on s.Id=PR.SectionId
                LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                left join hkp.LegalDesignation ld on ld.Id=e.LegalDesignationId
                where a.PlantId='" + PlantId + "' and a.WorkDate between '" + date + @"' and '" + ToDate + @"' and
                SandwichReprocess=1 order by EmpSystemID,Workdate,SandwichFlag asc";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public IEnumerable<object> GetEmployeeInformation(string month, string year)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
                string ToDate = Convert.ToDateTime(date).AddDays(32).ToString("dd-MMM-yyyy");


                var sql = @"select EmpSystemID,e.EmployeeCode,p.UserName as Plant,p.Id as PlantId,
                            format(WorkDate,'dd-MMM-yyyy')WorkDate,DayStatus,dp.UserName
                            as Department,s.UserName as Section,
                            SuS.UserName as SubSection,ld.UserName as Designation,SandwichFlag as TodayFlag,
                (select SandwichFlag from AttdnProcessData where WorkDate=DATEADD(day,-1,a.WorkDate) 
                and EmpSystemID=a.EmpSystemID)PrevDayFlag
                from attdnprocessdata a
                left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                left join org.Plant p on p.Id=e.PlantId
                LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                left join org.Section s on s.Id=PR.SectionId
                LEFT JOIN ORG.Department DP ON DP.Id = PR.DepartmentId
                LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = PR.SubSectionID
                left join hkp.LegalDesignation ld on ld.Id=e.LegalDesignationId
                where a.WorkDate between '" + date + @"' and '" + ToDate + @"' and
                SandwichReprocess=1 order by EmpSystemID,Workdate,SandwichFlag asc";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public DataTable SandWichDataTable(string PlantId, string month, string year, string EmpMaster)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
                string ToDate = Convert.ToDateTime(date).AddDays(32).ToString("dd-MMM-yyyy");

                var str = @"select dd.* from (
                select EmpSystemID,a.RowId,format(WorkDate, 'dd-MMM-yyyy')WorkDate,
                SandwichFlag,SandwichReprocess,
                (select SandwichFlag from AttdnProcessData where WorkDate = 
				DATEADD(day, -1, a.WorkDate)
                and EmpSystemID = a.EmpSystemID
                and a.PlantID = '" + PlantId + @"')PrevDayFlag,
                (select RowId from AttdnProcessData where WorkDate = 
				DATEADD(day, -1, a.WorkDate)
                and EmpSystemID = a.EmpSystemID
                and a.PlantID = '" + PlantId + @"')PrevRowId
                from attdnprocessdata a
                left join EmployeeInformation e on e.SystemId = a.EmpSystemID
                where a.WorkDate between '" + date + @"' and '" + ToDate + @"' and
                SandwichReprocess = 1 and EmpSystemID in (" + EmpMaster + ") and e.PlantID = '" + PlantId + @"' )as dd				
				where dd.PrevDayFlag is not null
				order by dd.WorkDate,dd.EmpSystemID asc";

                DataTable dtTemp = _sqlRepository.GetDataTable(str);
                return dtTemp;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public DataTable RecallUpdatedDataTable(string PlantId, string month, string year, string EmpMaster)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
                string ToDate = Convert.ToDateTime(date).AddDays(32).ToString("dd-MMM-yyyy");

                var str = @"select dd.* from (
                select EmpSystemID,a.RowId,format(WorkDate, 'dd-MMM-yyyy')WorkDate,
                DayStatus,SandwichFlag,a.ProcessFinalDayStatus,
                (select SandwichFlag from AttdnProcessData where WorkDate = 
				DATEADD(day, -1, a.WorkDate)
                and EmpSystemID = a.EmpSystemID
                and a.PlantID = '" + PlantId + @"')PrevDayFlag,
                (select WorkDate from AttdnProcessData where WorkDate = 
				DATEADD(day, -1, a.WorkDate)
                and EmpSystemID = a.EmpSystemID
                and a.PlantID = '" + PlantId + @"')PrevWkDate
                from attdnprocessdata a
                where a.WorkDate between '" + date + @"' and '" + ToDate + @"' and
                SandwichReprocess = 1 and EmpSystemID in (" + EmpMaster + ") and PlantID = '" + PlantId + @"' )as dd				
				where dd.PrevDayFlag is not null
				order by dd.WorkDate,dd.EmpSystemID asc";

                DataTable dtTemp = _sqlRepository.GetDataTable(str);
                return dtTemp;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void UpdatePayDayValues(string MinDate, string MaxDate, string Plant, string EmpMaster)
        {

            try
            {
                var sql = @"update AttdnProcessData set PresentValue=x.PresentValue,LateValue=x.LateValue,
                AbsentValue=x.AbsentValue,
                LvValue=x.LvValue,CompAssignLvValue=x.CompAssignLvValue,WeekOffValue=x.WeekOffValue,
                HoliDayValue=x.HoliDayValue,
                WeekOffHoliDayValue=x.WeekOffHoliDayValue,
                WorkingDayValue=x.WorkingDay,
                ActualWorkingDayValue=x.ActualWorkingDay,PayDayValue=x.TotalPayDay,NonPayDayValue=x.TotalNonPayDay
                from 
                (select distinct p.EmpSystemID,p.rowid as rowidx,Result=dt.DayType,format(p.WorkDate,'yyyy-MMM-dd')WorkDate,                 		
				isnull(dt.PresentValuePD,'0')PresentValue,isnull(dt.LateValueLV,'0')LateValue,isnull(dt.AbsentValueAB,'0')AbsentValue,
				isnull(dt.LeaveValueLP,'0')LvValue,isnull(dt.CompAssignLv,'0')CompAssignLvValue,
                isnull(dt.WeeklyOffWO,'0')WeekOffValue,isnull(dt.HolidayH,'0')HoliDayValue,isnull(dt.WeekOffHoliDayWOH,'0')WeekOffHoliDayValue,
			    isnull(dt.TotalWorkingDay,'0')WorkingDay,
				isnull(dt.ActualWorkingDay,'0')ActualWorkingDay,isnull(dt.PayDay,'0')TotalPayDay,isnull(dt.NonPayDay,'0')TotalNonPayDay                 
				from AttdnProcessData p
                        join EmployeeInformation  ei on ei.SystemId=p.EmpSystemID
                     	left join DayStatusHeader dh on dh.Id=p.DayStatusHeaderId
						left join DayStatus ds on ds.headerId=dh.Id
						left join DayTypeWithValues dt on dt.Id=ds.DayTypeWithValuesId									       
						where WorkDate between '" + MinDate + @"' and '" + MaxDate + @"'
						and p.EmpSystemID in (" + EmpMaster + ") and SandwichStatus is not null and ei.PlantId='" + Plant + @"'
						and dt.DayType=p.DayStatus)	as x where
						x.rowidx=RowId";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
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
        public DataTable EmpListCount(string PlantId, string month, string year)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
                string ToDate = Convert.ToDateTime(date).AddDays(32).ToString("dd-MMM-yyyy");

                var str = @"select distinct EmpSystemID               
                from attdnprocessdata a
                left join EmployeeInformation e on e.SystemId = a.EmpSystemID
                where a.WorkDate between '" + date + @"' and '" + ToDate + @"' and
                e.PlantID = '" + PlantId + "' and SandwichReprocess = 1";

                DataTable dtTemp = _sqlRepository.GetDataTable(str);
                return dtTemp;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        #region Saving Functions
        public void Process(string PlantId, string month, string year)
        {
            try
            {
                #region 
                string EmpParameter = "''";
                DataTable EmpListMaster; // Build Employee DataTable For Sandwich Process

                EmpListMaster = EmpListCount(PlantId, month, year); // Employee FindOut
                int empCounter = 0;
                for (int x = 0; x < EmpListMaster.Rows.Count; x++)
                {
                    empCounter++;
                    if (empCounter == 100)
                    {
                        // Calling Max 100 Employees Every Time
                        SaveLog("Sandwich Process for 100 Employees...", PlantId, false);
                        EmpWiseProcess(PlantId, month, year, EmpParameter);
                        EmpParameter = "''";
                        empCounter = 0;
                    }
                    else
                    {
                        string EmpId = clsWebLib.RetValidLen(EmpListMaster.Rows[x][@"EmpSystemID"]).ToString();
                        EmpParameter += ",'" + EmpId + "'";
                    }

                }
                if (EmpParameter != "''")
                {
                    EmpWiseProcess(PlantId, month, year, EmpParameter);
                    EmpParameter = "''";
                }
                #endregion


            }
            catch (Exception ex)
            {
                SaveLog(ex.Message, PlantId, true);
                throw ex;
            }
        }
        public void EmpWiseProcess(string PlantId, string month, string year, string EmpMaster)
        {
            try
            {
                #region Calculations                
                string MaxDate = "", MinDate = "";
                DataTable SandwichData; // Build DataTable For Sandwich Process

                SandwichData = SandWichDataTable(PlantId, month, year, EmpMaster);
                if (SandwichData.Rows.Count > 0)
                {
                    SaveLog("Sandwich Process Start ...", PlantId, false);

                    #region Min Max Date Finding Generation 

                    StringCollection StrDistinctWorkDate = new StringCollection();
                    StringCollection StrDistinctEmployee = new StringCollection();
                    var StringDates = new List<DateTime>();

                    for (int i = 0; i < SandwichData.Rows.Count; i++)
                    {
                        string WkDate = clsWebLib.RetValidLen(SandwichData.Rows[i][@"WorkDate"]).ToString();
                        if (StrDistinctWorkDate.Contains(WkDate))
                        {
                            continue;
                        }

                        StrDistinctWorkDate.Add(WkDate);
                        StringDates.Add(Convert.ToDateTime(WkDate));
                    }
                    MaxDate = StringDates.Max(date => date).ToString("dd-MMM-yyyy");
                    MinDate = StringDates.Min(date => date).ToString("dd-MMM-yyyy");

                    SaveLog("Min-Max Date Found ...", PlantId, false);

                    #endregion


                    for (int i = 0; i < SandwichData.Rows.Count; i++)
                    {
                        #region Variables

                        string EmpId = clsWebLib.RetValidLen(SandwichData.Rows[i]["EmpSystemID"]).ToString();
                        string TodayFlag = clsWebLib.RetValidLen(SandwichData.Rows[i]["SandwichFlag"]).ToString();
                        string PrevDayFlag = clsWebLib.RetValidLen(SandwichData.Rows[i]["PrevDayFlag"]).ToString();
                        string RowId = clsWebLib.RetValidLen(SandwichData.Rows[i]["RowId"]).ToString();
                        string WkDate = clsWebLib.RetValidLen(SandwichData.Rows[i]["WorkDate"]).ToString();

                        #endregion

                        if (TodayFlag != "" && PrevDayFlag != "")
                        {

                            #region Flag Changing Logic                           
                            string ActualFlag = "", ChangedFlag = "";

                            DataRow dr = SandwichData.Rows[i];
                            ActualFlag = SandwichData.Rows[i][@"SandwichFlag"].ToString();

                            dr.BeginEdit();
                            if (PrevDayFlag == "0" && TodayFlag == "2")
                            {
                                dr["SandwichFlag"] = "0"; //Today Change                                    
                                dr["SandwichReprocess"] = false;
                            }

                            else if (PrevDayFlag == "1" && TodayFlag == "2")
                            {
                                dr["SandwichFlag"] = "2"; //Today Change
                            }

                            else if (PrevDayFlag == "0" && TodayFlag == "3")
                            {
                                dr["SandwichFlag"] = "0"; //Today Change
                                dr["SandwichReprocess"] = false;
                            }

                            else if (PrevDayFlag == "0" && TodayFlag == "4")
                            {
                                dr["SandwichFlag"] = "0"; //Today Change
                                dr["SandwichReprocess"] = false;
                            }

                            else if (PrevDayFlag == "1" && TodayFlag == "3")
                            {
                                dr["SandwichFlag"] = "3"; //Today Change
                            }
                            dr.EndEdit();
                            ChangedFlag = SandwichData.Rows[i][@"SandwichFlag"].ToString();

                            #endregion

                            #region To Change Value of Flag in Next Day Row

                            if (ActualFlag != ChangedFlag)
                            {
                                SandwichData.DefaultView.RowFilter = @"PrevRowId='" + RowId + "' ";
                                if (SandwichData.DefaultView.Count > 0)
                                {
                                    DataRow drx = SandwichData.DefaultView[0].Row;
                                    drx.BeginEdit();
                                    drx["PrevDayFlag"] = ChangedFlag;
                                    drx.EndEdit();
                                }
                            }

                            #endregion

                        }
                    }
                    SaveLog("Sandwich Calculations Done ...", PlantId, false);

                }
                #endregion

                #region Flag Changing Logic 

                SaveLog("SandwichData Saving in APD Start ...", PlantId, false);

                if (SandwichData.Rows.Count > 0)
                {
                    int counter = 0;
                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where EmpSystemId IN(" + EmpMaster + ") and PlantId='" + PlantId + "' and SandwichReprocess = 1 and WorkDate between '" + MinDate + "' and '" + MaxDate + "'";

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    for (int i = 0; i < SandwichData.Rows.Count; i++)
                    {
                        // Manipulated DataSet Variables
                        string RowId = SandwichData.Rows[i][@"RowId"].ToString();
                        string ChangedFlag = SandwichData.Rows[i][@"SandwichFlag"].ToString();
                        string ChangedReprocessFlag = clsWebLib.RetValidLen(SandwichData.Rows[i][@"SandwichReprocess"]).ToString();


                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            string ActualFlag = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"SandwichFlag"]).ToString();
                            string ReProcessFlag = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"SandwichReprocess"]).ToString();

                            if (ActualFlag != ChangedFlag)
                            {
                                counter++;
                                dr.BeginEdit();
                                dr["SandwichFlag"] = ChangedFlag;
                                if (ChangedReprocessFlag != ReProcessFlag)
                                {
                                    dr["SandwichReprocess"] = ChangedReprocessFlag;
                                }
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }

                    }
                    if (counter > 0)
                    {
                        SaveLog("Sandwich Process Flags Saved ...", PlantId, false);
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsRef);
                    }
                }
                #endregion

                #region DayStatus Changing Logic

                DataTable RefreshedDt; // Build DataTable For DayStatus Changing
                RefreshedDt = RecallUpdatedDataTable(PlantId, month, year, EmpMaster);
                if (RefreshedDt.Rows.Count > 0)
                {
                    int ddx = 01;
                    string jj = month.ToString() + "-" + ddx.ToString() + "-" + year.ToString();
                    string TempDate = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
                    SaveLog("Updated DataSet Called ...", PlantId, false);

                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter("select * from AttdnProcessData where 1=2", out DataSet MasterDataSet, false, false, "", "1");

                    for (int i = 0; i < RefreshedDt.Rows.Count; i++)
                    {
                        #region Variables                       

                        string EmpId = clsWebLib.RetValidLen(RefreshedDt.Rows[i]["EmpSystemID"]).ToString();
                        string TodayFlag = clsWebLib.RetValidLen(RefreshedDt.Rows[i]["SandwichFlag"]).ToString();
                        string PrevDayFlag = clsWebLib.RetValidLen(RefreshedDt.Rows[i]["PrevDayFlag"]).ToString();
                        string PrevWkDate = clsWebLib.RetValidLen(RefreshedDt.Rows[i]["PrevWkDate"]).ToString();
                        string FinalStatus = clsWebLib.RetValidLen(RefreshedDt.Rows[i]["ProcessFinalDayStatus"]).ToString();

                        #endregion

                        #region Generating DataSet

                        if (PrevDayFlag == "2" || PrevDayFlag == "4" || PrevDayFlag == "3")
                        {
                            if (TodayFlag == "1")
                            {
                                if (FinalStatus != "")
                                {
                                    var sqly = @"SELECT * FROM (
                                                            select RowId,EmpSystemID,sandwichflag as SandwichMaster,
                                                            CASE WHEN SandwichFlag IN (2,3) THEN 2 ELSE 
                                                            SandwichFlag END SandwichFlag,WorkDate,
                                                            DENSE_RANK() OVER (PARTITION BY EmpSystemID,CASE WHEN 
                                                            SandwichFlag IN (2,3) THEN 2 ELSE 
                                                            SandwichFlag END ORDER BY WorkDate DESC,CASE WHEN SandwichFlag IN 
                                                            (2,3) THEN 2 ELSE SandwichFlag END) AS RNKFlag,
                                                            DENSE_RANK() OVER (PARTITION BY EmpSystemID ORDER BY WorkDate DESC) 
                                                            AS RNKEmp
                                                            from AttdnProcessData where WorkDate <= '" + PrevWkDate + @"'
                                                            and EmpSystemID='" + EmpId + @"' and SandwichFlag !='4'
                                                            ) AS K WHERE RNKFlag=RNKEmp AND K.SandwichFlag NOT IN (0,1,3)";

                                    var RowData = _sqlRepository.GetDataTable(sqly);
                                    if (RowData.Rows.Count > 0)
                                    {
                                        for (int x = 0; x < RowData.Rows.Count; x++)
                                        {
                                            var RowxId = RowData.Rows[x]["RowId"].ToString();
                                            var SandwichMaster = RowData.Rows[x]["SandwichMaster"].ToString();
                                            var Wk = RowData.Rows[x]["WorkDate"].ToString();
                                            if (Convert.ToDateTime(Wk) >= Convert.ToDateTime(TempDate))
                                            {
                                                if (SandwichMaster == "3")
                                                {
                                                    DataRow drx = MasterDataSet.Tables[0].NewRow();
                                                    drx["DayStatus"] = "W";
                                                    drx["RowId"] = RowxId;
                                                    MasterDataSet.Tables[0].Rows.Add(drx);
                                                }
                                                else if (SandwichMaster == "2")
                                                {
                                                    if (FinalStatus != "")
                                                    {
                                                        DataRow drx = MasterDataSet.Tables[0].NewRow();
                                                        drx["DayStatus"] = FinalStatus;
                                                        drx["RowId"] = RowxId;
                                                        MasterDataSet.Tables[0].Rows.Add(drx);

                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }

                        #endregion
                    }

                    #region Save Sandwich Status
                    if (MasterDataSet.Tables[0].Rows.Count > 0)
                    {
                        SaveLog("BackDate DayStatus Saving Logic Start ...", PlantId, false);

                        int x = 0;
                        ConnectionManager.DAL.ConManager newcon = new ConnectionManager.DAL.ConManager("1");
                        var sqlx = @"select * from AttdnProcessData where EmpSystemId In(" + EmpMaster + ") and PlantId='" + PlantId + "' and SandwichReprocess = 1 and WorkDate between '" + MinDate + "' and '" + MaxDate + "'";

                        newcon.OpenDataSetThroughAdapter(sqlx, out DataSet dsMaster, false, false, "", "1");

                        for (int j = 0; j < MasterDataSet.Tables[0].Rows.Count; j++)
                        {
                            string IndvRow = clsWebLib.RetValidLen(MasterDataSet.Tables[0].Rows[j][@"RowId"]).ToString();
                            string DayType = clsWebLib.RetValidLen(MasterDataSet.Tables[0].Rows[j][@"DayStatus"]).ToString();

                            dsMaster.Tables[0].DefaultView.RowFilter = @"RowId='" + IndvRow + "'";
                            if (dsMaster.Tables[0].DefaultView.Count > 0)
                            {
                                DataRow dry = dsMaster.Tables[0].DefaultView[0].Row;
                                string ActualSandwich = clsWebLib.RetValidLen(dsMaster.Tables[0].DefaultView[0][@"Sandwichstatus"]).ToString();
                                if (ActualSandwich != DayType)
                                {
                                    x++;
                                    dry.BeginEdit();
                                    dry["Sandwichstatus"] = DayType;
                                    dry["DayStatus"] = DayType;
                                    dry["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dry["UpdatedBy"] = "Sandwich";
                                    dry.EndEdit();
                                }
                            }

                        }
                        if (x > 0)
                        {
                            SaveLog("Sandwich DayStatus Updated ...", PlantId, false);
                            clsStaticInfo info = new clsStaticInfo();
                            info.SaveDataSets(dsMaster);
                        }
                    }
                    #endregion

                }

                #endregion

                #region PayDay Values Change

                UpdatePayDayValues(MinDate, MaxDate, PlantId, EmpMaster);
                SaveLog("PayDay Values Updated ...", PlantId, false);

                #endregion
            }
            catch (Exception ex)
            {
                SaveLog(ex.Message, PlantId, true);
                throw ex;
            }
        }

        #endregion
    }
}
