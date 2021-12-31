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

        public IEnumerable<object> GetEmployeeInformation(string month, string year)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
               
                var sql = @"select EmpSystemID,e.EmployeeCode,p.UserName as Plant,p.Id as PlantId,
                            format(WorkDate,'dd-MMM-yyyy')WorkDate,DayStatus,dp.UserName
                            as Department,s.UserName as Section,
                            SuS.UserName as SubSection,ld.UserName as Designation,SandwichFlag as TodayFlag,
                (select SandwichFlag from AttdnProcessData where WorkDate=DATEADD(day,-1,a.WorkDate) 
                and EmpSystemID=a.EmpSystemID)PrevDayFlag
                from attdnprocessdata a
                left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                left join org.Plant p on p.Id=e.PlantId
                left join org.Section s on s.Id=e.SectionId
                LEFT JOIN ORG.Department DP ON DP.Id = E.DepartmentId
                LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = E.SubSectionID
                left join hkp.LegalDesignation ld on ld.Id=e.LegalDesignationId
                where a.WorkDate between '" + date+@"' and GETDATE() and
                SandwichReprocess=1 order by EmpSystemID,Workdate,SandwichFlag asc";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public DataTable SandWichDataTable(string PlantId, string month, string year)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
                var str = @"select dd.* from (
                select EmpSystemID,a.RowId,format(WorkDate, 'dd-MMM-yyyy')WorkDate,
                SandwichFlag,SandwichReprocess,
                (select SandwichFlag from AttdnProcessData where WorkDate = 
				DATEADD(day, -1, a.WorkDate)
                and EmpSystemID = a.EmpSystemID
                and a.PlantID = '" + PlantId+ @"')PrevDayFlag,
                (select RowId from AttdnProcessData where WorkDate = 
				DATEADD(day, -1, a.WorkDate)
                and EmpSystemID = a.EmpSystemID
                and a.PlantID = '"+PlantId+@"')PrevRowId
                from attdnprocessdata a
                left join EmployeeInformation e on e.SystemId = a.EmpSystemID
                where a.WorkDate between '" + date+@"' and GETDATE() and
                SandwichReprocess = 1 and e.PlantID = '"+PlantId+@"' )as dd				
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

        public DataTable RecallUpdatedDataTable(string PlantId, string month, string year)
        {
            try
            {
                int dd = 01;
                string jj = month.ToString() + "-" + dd.ToString() + "-" + year.ToString();
                string date = DateTime.Parse(jj).ToString("dd-MMM-yyyy");
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
                where a.WorkDate between '" + date + @"' and GETDATE() and
                SandwichReprocess = 1 and PlantID = '" + PlantId + @"' )as dd				
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


        public void Process(string PlantId, string month, string year)
        {
            try
            {
                #region Calculations
                string MaxDate = "", MinDate = "";
                DataTable SandwichData; // Build DataTable For Sandwich Process
                SandwichData = SandWichDataTable(PlantId, month, year);
                if (SandwichData.Rows.Count > 0)
                {
                    #region DataTable Generation 

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

                    #endregion


                    for (int i = 0; i < SandwichData.Rows.Count; i++)
                    {
                        #region Variables

                        string EmpId = clsWebLib.RetValidLen(SandwichData.Rows[i]["EmpSystemID"]).ToString();
                        string TodayFlag = clsWebLib.RetValidLen(SandwichData.Rows[i]["SandwichFlag"]).ToString();
                        string PrevDayFlag = clsWebLib.RetValidLen(SandwichData.Rows[i]["PrevDayFlag"]).ToString();
                        string RowId = clsWebLib.RetValidLen(SandwichData.Rows[i]["RowId"]).ToString();
                        string PrevDayRowId = clsWebLib.RetValidLen(SandwichData.Rows[i]["PrevRowId"]).ToString();
                        string WkDate = clsWebLib.RetValidLen(SandwichData.Rows[i]["WorkDate"]).ToString();

                        #endregion

                        if (TodayFlag != "" && PrevDayFlag != "")
                        {

                            #region Flag Changing Logic

                            SandwichData.DefaultView.RowFilter = @"EmpSystemID='" + EmpId + "' AND WorkDate =#" + WkDate + "# ";
                            DataRow dr = SandwichData.DefaultView[0].Row;
                            string ActualFlag = SandwichData.DefaultView[0][@"SandwichFlag"].ToString();

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
                            string ChangedFlag = SandwichData.DefaultView[0][@"SandwichFlag"].ToString();

                            #endregion

                            #region To Change Value of Flag in Next Day Row

                            SandwichData.DefaultView.RowFilter = @"PrevRowId='" + RowId + "' ";
                            if (SandwichData.DefaultView.Count > 0)
                            {
                                if (ActualFlag != ChangedFlag)
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
                }
                #endregion

                #region Save Data in APD 
                if (SandwichData.Rows.Count > 0)
                {
                    int counter = 0;
                    ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                    var sqlx = @"select * from AttdnProcessData where PlantId='" + PlantId + "' and SandwichReprocess = 1 and WorkDate between '" + MinDate + "' and '" + MaxDate + "'";

                    objCon.OpenDataSetThroughAdapter(sqlx, out DataSet dsRef, false, false, "", "1");

                    for (int i = 0; i < SandwichData.Rows.Count; i++)
                    {
                        // Manipulated DataSet Variables
                        string RowId = SandwichData.Rows[i][@"RowId"].ToString();
                        string ChangedFlag = SandwichData.Rows[i][@"SandwichFlag"].ToString();

                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            string ActualFlag = clsWebLib.RetValidLen(dsRef.Tables[0].DefaultView[0][@"SandwichFlag"]).ToString();
                            if (ActualFlag != ChangedFlag)
                            {
                                counter++;
                                dr.BeginEdit();
                                dr["SandwichFlag"] = ChangedFlag;
                                dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                dr.EndEdit();
                            }
                        }

                    }
                    if (counter > 0)
                    {
                        clsStaticInfo info = new clsStaticInfo();
                        info.SaveDataSets(dsRef);
                    }
                }
                #endregion

                #region DayStatus Changing Logic
              
                DataTable RefreshedDt; // Build DataTable For DayStatus Changing
                RefreshedDt = SandWichDataTable(PlantId, month, year);
                if (RefreshedDt.Rows.Count > 0)
                {
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
                                    // RowId Fetching for In Range b/w previous sandwichflags 2 _ _ _ _ _ _ _ 2

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

                        #endregion
                    }
                }                 
                
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }       
        
    }
}
 