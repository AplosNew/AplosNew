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

namespace Library.HumanResource.NewAttendanceProcess

{
    public class WeekOffUpdatesService
    {

        ISqlRepository _sqlRepository;
        public WeekOffUpdatesService()
        {
            _sqlRepository = new SqlRepository();
        }


        public IEnumerable<object> getPlants()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"Select Username as Text , Id as Value from ORG.Plant where CompanyId = '" + identity.CompanyId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getCurrentList()
        {
            try
            {
                var str = @"Select ROW_NUMBER() OVER(ORDER BY ew.EffectiveDate desc) as Rows,ew.EmpSystemId, ew.WOHeaderId,format(ew.EffectiveDate,'dd-MMM-yyyy') as EffectiveDate
                            , ew.Id, ei.SystemId,ei.EmployeeCode,
                            ei.EmployeeName, wo.UserName as WOName
                            from dbo.EmployeeWeeklyOff ew
                            left join dbo.EmployeeInformation ei on ei.SystemId = ew.EmpSystemId
                            left join dbo.WeekOffHeader wo on wo.Id = ew.WOHeaderId
                            order by CAST(ew.EffectiveDate as Date) desc
                                ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        // The Section for Saving And Updating of Data
        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData, string addedname, string addeddate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = addedname;
            dr["AddedDate"] = addeddate;
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }

    

        //The Apis for the 2nd Page

    

        public void SaveFileList(List<Dictionary<string,object>> data )
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.EmployeeWeeklyOff";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1 = 2", out dsMaster, false, "1");

                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    int indexa = 0;
                    for (int i = 0; i < data.Count; i++)
                    {
                        Dictionary<string, object> jj = data[i];
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        indexa++;
                        jj["Id"] = _Id ;

                        AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                    }


                }

                var sqls = @"Delete from " + TableName;

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                objCone.CommitTransaction();

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
       
        public DataTable getEmployeeWeekRosterFile( )
        {
            try
            {
                var str = @"Select ei.EmployeeCode , ew.WOHeaderId as RosterId,format(ew.EffectiveDate,'dd-MMM-yyyy') as EffectiveDate
from dbo.EmployeeWeeklyOff ew
left join dbo.EmployeeInformation ei on ei.SystemId = ew.EmpSystemId
                            ";

                return _sqlRepository.GetDataTable(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public DataTable getEmployeesAll()
        {
            try
            {
                var str = @"Select SystemId, EmployeeCode from dbo.EmployeeInformation where EmployeeCode<>''
                           ";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getRostersFile()
        {
            try
            {
                var str = @"Select * from dbo.WeekOffHeader";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public List<string> getRostersList()
        {
            try
            {
                var str1 = @"Select Id from dbo.WeekOffHeader ";
                DataTable dt = _sqlRepository.GetDataTable(str1);

                List<string> roster = new List<string>();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        roster.Add(dt.Rows[i]["Id"].ToString());
                    }
                }

                return roster;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<string> getWeekOffList()
        {
            try
            {
                var str1 = @"Select Id from dbo.EmployeeWeeklyOff ";
                DataTable dt = _sqlRepository.GetDataTable(str1);

                List<string> roster = new List<string>();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        roster.Add(dt.Rows[i]["Id"].ToString());
                    }
                }

                return roster;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// First Tab Functions
        public IEnumerable<object> getWeekOffCbo()
        {
            try
            {
                var str = @"Select Id as value , UserName as text from dbo.WeekOffHeader";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmployees()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"Select SystemId , EmployeeCode , EmployeeName ,BudgetCode from dbo.EmployeeInformation where PlantId = '"+identity.PlantId+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmpWeekOff(string EmpId)
        {
            try
            {
                var str = @"Select eow.EmpSystemId , isnull(eow.WOHeaderId,'') as WOHeaderId, format(eow.EffectiveDate,'dd-MMM-yyyy') as EffectiveDate
                            , isnull(wo.UserName,'') as UserName  from dbo.EmployeeWeeklyOff eow 
                            left join dbo.WeekOffHeader wo on wo.Id = eow.WOHeaderId
                            where EmpSystemId = '" + EmpId+ "' order by Convert(datetime , EffectiveDate) desc";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<string> getBudgets()
        {
            try
            {
                var str = @"Select Id from mst.ManPowerBudget where IsScattedWeekOffApplicable = 1";
                DataTable dtBudgets = _sqlRepository.GetDataTable(str);
                List<string> list = dtBudgets.AsEnumerable()
                            .Select(r => r.Field<string>("Id"))
                            .ToList();
                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getWeekOffsLists(string EmpID)
        {
            try
            {
                var str = @"Select weeks.WOHeaderId , weeks.UserName , Count(SystemId) as Emps
                            from
                            (
                            Select sd.WOHeaderId  , wh.UserName , sh.PlantId
                            from dbo.ScatteredWeekHeader sh
                            left join dbo.ScatteredWeekChild sc on sc.HeaderId = sh.Id
                            left join dbo.ScatteredWeekDefinition sd on sd.ID = sc.ScatteredWeekDefinitionId
                            left join dbo.WeekOffHeader wh on wh.Id = sd.WOHeaderId
                            ) as weeks 
                            left join 
                            (
                            Select ei.SystemId , ei.BudgetCode , ei.PlantId,
                            (Select top 1 WOHeaderId from dbo.EmployeeWeeklyOff
                            where EmpSystemId = ei.SystemId 
                            order by EffectiveDate desc) as WOHeader
                            from dbo.EmployeeInformation ei
                            where ei.BudgetCode = (Select BudgetCode from dbo.EmployeeInformation where SystemId = '" + EmpID + @"')
                            ) as emps on weeks.PlantId = emps.PlantId and weeks.WOHeaderId = emps.WOHeader
                            group by  weeks.WOHeaderId , weeks.UserName
                            order by Emps asc , WOHeaderId
                            ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getDistinctEmployeesToBeProcessed(string EffectiveDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var str = @"select distinct ex.EmpSystemId,e.EmployeeCode,e.EmployeeName,l.UserName as Designation,
                s.UserName AS Section,ss.UserName as SubSection,d.UserName as Department
                from EmployeeWeeklyOff ex 
                join EmployeeInformation e on e.SystemId=ex.EmpSystemId
                left join org.Department d on d.Id=e.DepartmentId
                left join org.Section s on s.Id=e.SectionId
                left join org.SubSection ss on ss.Id=e.SubSectionId
                left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId
                where E.DOJ <= '"+EffectiveDate+@"' -- Effective Date 
                AND (E.DOS >= '"+EffectiveDate+"' OR ISNULL(E.DOS,'') = '' OR E.DOS = '01/01/1901')" +
                "and e.plantId='"+identity.PlantId+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        public void UpdateRosterWeekOffData(string fDate, string tDate, string plant,string data)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"UPDATE apd SET WeeklyStatus=ISNULL(ISNULL(ws.Code,IWO.DayType),NULL) 
 
				FROM AttdnProcessData apd
LEFT JOIN dbo.EmployeeInformation EI 
       ON EI.SystemId = apd.EmpSystemId
LEFT JOIN dbo.RosterBudget RB 
       ON RB.BudgetId = EI.BudgetCode
-- 1️⃣ Get applicable EffectiveDate
OUTER APPLY (
    SELECT TOP (1) *
    FROM dbo.RosterEffectiveDate
    WHERE RPHeaderId = RB.RosterId
      AND EffectiveDate <= apd.WorkDate
    ORDER BY EffectiveDate DESC
) RED
-- 2️⃣ Calculate DayInWeek ONCE
CROSS APPLY (
    SELECT ((DATEDIFF(DAY, RED.EffectiveDate, apd.WorkDate)) % (select count(Days31) from dbo.RosterPatternChild where Days31<>'' AND  RPHeaderId=RB.RosterId )) + 1 AS DayInWeek
) D
-- 3️⃣ Pick correct RosterPatternChild row
LEFT JOIN dbo.RosterPatternChild RPC
       ON RPC.RPHeaderId = RB.RosterId
      AND RPC.Days31 = D.DayInWeek
LEFT JOIN hkp.WeeklyStatus WS
       ON WS.Id = RPC.WeeklyStatusId


LEFT JOIN (Select dd.*,
                (Select wcc.DayType from
                dbo.WeekOffChild wcc where wcc.WOSequence =dd.DayDiff 
                and wcc.WOHeaderId = dd.WeekOffHeaderId) 
                as DayType,
(Select wcc.[Day] from
                dbo.WeekOffChild wcc where wcc.WOSequence =dd.DayDiff 
                and wcc.WOHeaderId = dd.WeekOffHeaderId) 
                as [WeekDay]
,'' RosterDayType
                from
                (Select e.SystemId,
                
                (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                where EmpSystemId = e.SystemId and ex.EffectiveDate<='" + fDate + @"'
                order by ex.EffectiveDate desc) WeekOffHeaderId,

                (DATEDIFF(DAY, (Select top 1 ed.EffectiveDate from
                dbo.WeekOffHeader h 
                left join dbo.WeekOffEffectiveDate ed on ed.WOHeaderId = h.Id               
                where ed.EffectiveDate <= '" + fDate + @"' and ed.WOHeaderId =  
				(Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                where EmpSystemId = e.SystemId and ex.EffectiveDate<='" + fDate + @"'
                order by ex.EffectiveDate desc)
                order by ed.EffectiveDate desc) , '" + fDate + @"') % 
                (Select max(WOSequence) from WeekOffHeader h 
                left join WeekOffChild wc on wc.WOHeaderId=h.Id 
                where h.Id =  
				(Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                where EmpSystemId = e.SystemId and ex.EffectiveDate<='" + fDate + @"'
                order by ex.EffectiveDate desc)
				)
				)+1 as DayDiff
                from 
                EmployeeInformation e
                left join EmployeeWeeklyOff ex on e.SystemId=ex.EmpSystemId
				LEFT JOIN dbo.RosterBudget RB 
       ON RB.BudgetId = E.BudgetCode
-- 1️⃣ Get applicable EffectiveDate
OUTER APPLY (
    SELECT TOP (1) *
    FROM dbo.RosterEffectiveDate
    WHERE RPHeaderId = RB.RosterId
      AND EffectiveDate <= '" + fDate + @"'
    ORDER BY EffectiveDate DESC
) RED
-- 2️⃣ Calculate DayInWeek ONCE
CROSS APPLY (
    SELECT ((DATEDIFF(DAY, RED.EffectiveDate, '" + fDate + @"')) % (select count(Days31) from dbo.RosterPatternChild where Days31<>'' AND  RPHeaderId=RB.RosterId )) + 1 AS DayInWeek
) D
-- 3️⃣ Pick correct RosterPatternChild row
LEFT JOIN dbo.RosterPatternChild RPC
       ON RPC.RPHeaderId = RB.RosterId
      AND RPC.Days31 = D.DayInWeek
LEFT JOIN hkp.WeeklyStatus WS
       ON WS.Id = RPC.WeeklyStatusId
                where e.SystemId in( select empsystemid from EmployeeWeeklyOff)
                and e.PlantId='" + plant + @"' and e.SystemId in ("+ data + @")
                group by e.SystemId
                ) as dd) IWO ON IWO.SystemId=apd.EmpSystemId and DATENAME(WEEKDAY,apd.WorkDate)=IWO.[WeekDay]
				where apd.workdate  between '" + fDate + @"' and '" + tDate + @"' and apd.PlantId='" + plant + @"' and isnull(EmpSystemID,'') IN ( -- and apd.EmpSystemID='2525844'
									SELECT isnull(ei.SystemId,'') 
                                    FROM EmployeeInformation AS ei WHERE  ei.PlantId='" + plant + @"'
                                   AND  ei.DOJ <= '" + fDate + @"' 
                                   AND (ei.DOS >= '" + fDate + @"' OR ISNULL(ei.DOS,'') = '' OR ei.DOS = '01/01/1901')  and apd.EmpSystemID in (" + data + @"))  ";
                ConnectionManager.clsConnection objCone = new ConnectionManager.clsConnection();
                objCone.BeginTransaction();
                objCone.executeQuery(sql);
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void UpdateIndividualData(string fDate, string tDate, string plant, string data)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sqlxNew = @"UPDATE apd SET apd.WeeklyStatus=ISNULL(x.DayType,'') from (
                Select dd.*,
                (Select wcc.DayType from
                dbo.WeekOffChild wcc where wcc.WOSequence =dd.DayDiff 
                and wcc.WOHeaderId = dd.WeekOffHeaderId) 
                as DayType
                ,(Select wcc.[Day] from
                dbo.WeekOffChild wcc where wcc.WOSequence =dd.DayDiff 
                and wcc.WOHeaderId = dd.WeekOffHeaderId) 
                as [WeekDay]
                from
                (Select e.SystemId,
                
                (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                where EmpSystemId = e.SystemId and ex.EffectiveDate<='" + fDate + @"'
                order by ex.EffectiveDate desc) WeekOffHeaderId,

                (DATEDIFF(DAY, (Select top 1 ed.EffectiveDate from
                dbo.WeekOffHeader h 
                left join dbo.WeekOffEffectiveDate ed on ed.WOHeaderId = h.Id               
                where ed.EffectiveDate <= '" + fDate + @"' and ed.WOHeaderId =  
				(Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                where EmpSystemId = e.SystemId and ex.EffectiveDate<='" + fDate + @"'
                order by ex.EffectiveDate desc)
                order by ed.EffectiveDate desc) , '" + fDate + @"') % 
                (Select max(WOSequence) from WeekOffHeader h 
                left join WeekOffChild wc on wc.WOHeaderId=h.Id 
                where h.Id =  
				(Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                where EmpSystemId = e.SystemId and ex.EffectiveDate<='" + fDate + @"'
                order by ex.EffectiveDate desc)
				)
				)+1 as DayDiff
                from 
                EmployeeInformation e
                left join EmployeeWeeklyOff ex on e.SystemId=ex.EmpSystemId
                where e.SystemId in( select empsystemid from EmployeeWeeklyOff)
 
and E.BudgetCode not in(Select SystemId from 
				 dbo.RosterBudget RB 
-- 1️⃣ Get applicable EffectiveDate
OUTER APPLY (
    SELECT TOP (1) *
    FROM dbo.RosterEffectiveDate
    WHERE RPHeaderId = RB.RosterId
      AND EffectiveDate <= '" + fDate + @"'
    ORDER BY EffectiveDate DESC
) RED
-- 2️⃣ Calculate DayInWeek ONCE
CROSS APPLY (
    SELECT ((DATEDIFF(DAY, RED.EffectiveDate, '" + fDate + @"')) % (select count(Days31) from dbo.RosterPatternChild where Days31<>'' AND  RPHeaderId=RB.RosterId )) + 1 AS DayInWeek
) D
-- 3️⃣ Pick correct RosterPatternChild row
LEFT JOIN dbo.RosterPatternChild RPC
       ON RPC.RPHeaderId = RB.RosterId
      AND RPC.Days31 = D.DayInWeek
LEFT JOIN hkp.WeeklyStatus WS
       ON WS.Id = RPC.WeeklyStatusId
				)
                and e.PlantId='" + plant + @"' 
                group by e.SystemId
                ) as dd	
				) x  join (select  PlantID,WorkDate,WeeklyStatus,EmpSystemID from AttdnProcessData 
                                   WHERE WorkDate  between '" + fDate + @"' and '" + tDate + @"' and PlantId='" + plant + @"' 
                                     ) apd on apd.EmpSystemID=x.SystemId AND apd.EmpSystemID in ("+ data + @") AND apd.WeeklyStatus IS NULL  
									 and x.[WeekDay]=DATENAME(WEEKDAY,apd.WorkDate)  ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.BeginTransaction();
                objCon.executeQuery(sqlxNew);
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public string ProcessAttendanceNew(string EffectiveDate, string data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = DateTime.Now.ToString();
                #region Plant Lock Checking

                DataSet PlantLock;
                string FD = EffectiveDate;
                string TD = DateTime.Now.ToString("yyyy-MM-dd");
                PlantLockCheck(FD, TD, out PlantLock, identity.PlantId);
                string pl = "";
                if (PlantLock.Tables[0].Rows.Count > 0)
                {
                    for (var i = 0; i < PlantLock.Tables[0].Rows.Count; i++)
                    {
                        pl = pl + " " + PlantLock.Tables[0].Rows[i]["LockedDate"].ToString() + ", ";
                    }
                    return "The Plant is Locked for - " + pl;

                }
                #endregion

                #region WeekOff Flagging

                ConnectionManager.DAL.ConManager objConR = new ConnectionManager.DAL.ConManager("1");
                string DayType = null;

                UpdateRosterWeekOffData(FD,TD, identity.PlantId, data);

                UpdateIndividualData(FD,TD, identity.PlantId,data);

                #endregion

                return "true";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string ProcessAttendance(string EffectiveDate,string data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = DateTime.Now.ToString();

                #region Plant Lock Checking

                DataSet PlantLock;
                    string FD = EffectiveDate;
                    string TD = DateTime.Now.ToString("yyyy-MM-dd");
                PlantLockCheck(FD, TD, out PlantLock, identity.PlantId);
                string pl = "";
                if (PlantLock.Tables[0].Rows.Count > 0)
                {
                    for (var i = 0; i < PlantLock.Tables[0].Rows.Count; i++)
                    {
                        pl = pl + " " + PlantLock.Tables[0].Rows[i]["LockedDate"].ToString() + ", ";
                    }
                    return "The Plant is Locked for - " + pl;
                   
                }
                #endregion

                string GettingRows = @"Select jj.* ,  (Select wcc.DayType from
                                                dbo.WeekOffChild wcc where wcc.WOSequence =jj.Seq 
                                                and wcc.WOHeaderId = jj.WeekOffHeaderId) 
                                                as DayType , ap.RowId , (Case when ap.RowId = jj.MyRowId then 1 else 0 end) as Checks
                                    from
                                                (Select ap.WorkDate, ap.EmpSystemID, format(ap.WorkDate,'yyyyMMdd')+ap.EmpSystemID as MyRowId,
                                                (Select distinct
                                                (DATEDIFF(DAY, (Select top 1 ed.EffectiveDate from
                                                dbo.WeekOffHeader h 
                                                left join dbo.WeekOffEffectiveDate ed on ed.WOHeaderId = h.Id
                                                where ed.EffectiveDate <= ap.WorkDate and ed.WOHeaderId =  
                                    (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                                                where EmpSystemId = e.SystemId and ex.EffectiveDate<=ap.WorkDate
                                                order by ex.EffectiveDate desc)
                                                order by ed.EffectiveDate desc) , ap.WorkDate) % 
                                                (Select max(WOSequence) from WeekOffHeader h 
                                                left join WeekOffChild wc on wc.WOHeaderId=h.Id 
                                                where h.Id =  
                                    (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                                                where EmpSystemId = e.SystemId and ex.EffectiveDate<=ap.WorkDate
                                                order by ex.EffectiveDate desc)
                                    )
                                    )+1 as DayDiff
                                                from 
                                                EmployeeInformation e
                                                left join EmployeeWeeklyOff ex on e.SystemId=ex.EmpSystemId
                                                where e.PlantId=ap.PlantID and e.SystemId = ap.EmpSystemID) as Seq,

                                                (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                                                where EmpSystemId = ap.EmpSystemID and ex.EffectiveDate<=ap.WorkDate
                                                order by ex.EffectiveDate desc) WeekOffHeaderId 
                                    from AttdnProcessData ap 

                                    where ap.EmpSystemID in("+data+") and WorkDate between '" + FD + @"' and '" + TD + @"'
                                    )as jj
                                    left join AttdnProcessData ap on ap.WorkDate = jj.WorkDate 
                                    and ap.EmpSystemID In("+data+") and ap.WorkDate between '" + FD + @"' and '" + TD + @"'";

                    DataTable dt = _sqlRepository.GetDataTable(GettingRows);
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("select * from dbo.AttdnProcessData where EmpSystemID IN ("+data+") and WorkDate between '" + FD + @"' and '" + TD + @"'", out dsMaster, false, "1");

                    string RowMaster = "''";
                
                if(dt.Rows.Count>0)
                {
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        RowMaster = RowMaster + ",'" + dt.Rows[i]["RowId"].ToString() + "'";
                        dsMaster.Tables[0].DefaultView.RowFilter = @"RowId ='" + dt.Rows[i]["RowId"].ToString() + "'";
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["WeeklyStatus"] = dt.Rows[i]["DayType"].ToString();
                        dr["isLock"] = false;
                        dr["ManualFlag"] = true;
                        dr["ManualEntryTime"] = DateTime.Now;
                        dr["ManualByWhom"] = identity.Name;
                        dr["LockedDate"] = DBNull.Value;
                        dr["LockedBy"] = DBNull.Value;
                        dr["IsOTComfirm"] = false;
                        dr["OTComfirmBy"] = DBNull.Value;
                        dr["DateOTComfirm"] = DBNull.Value;
                        dr.EndEdit();

                    }

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);

                    #region Attnd Process Call

                    NewAttendanceProcessService ap = new NewAttendanceProcessService();
                    ap.ManualScheduler(identity.PlantId, RowMaster);
                   
                    #endregion
                }
                return "true";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void saveSingle(string EmpId , string EffectiveDate , string WeekId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = DateTime.Now.ToString();
                string TableName = "dbo.EmployeeWeeklyOff";


                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where EmpSystemId='" + EmpId + "' and EffectiveDate='" + EffectiveDate + "'", out dsMaster, false, "1");

                    DataTable no = _sqlRepository.GetDataTable("Select top 1 Id as Nos from dbo.EmployeeWeeklyOff order by Cast(Id as numeric) desc");
                    int id = int.Parse(no.Rows[0]["Nos"].ToString()) + 1;

                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        if (dsMaster.Tables[0].Rows[0]["WOHeaderId"].ToString() == WeekId)
                        {
                            throw new Exception("Already the Same Data Exists !!");
                        }
                        else
                        {
                            DataRow ddr = dsMaster.Tables[0].Rows[0];
                            ddr.BeginEdit();
                            ddr["WOHeaderId"] = WeekId;
                            ddr["UpdatedBy"] = identity.Name;
                            ddr["UpdatedDate"] = DateTime.Now.ToString();
                            ddr["UpdatedFromIP"] = identity.IPAddress;
                            ddr.EndEdit();
                        }
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = id.ToString();
                        dr["EmpSystemId"] = EmpId;
                        dr["WOHeaderId"] = WeekId;
                        dr["EffectiveDate"] = Convert.ToDateTime(EffectiveDate).ToString("yyyy-MM-dd");
                        dr["AddedBy"] = addedname;
                        dr["AddedDate"] = addeddate;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }


                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);
               
                
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void PlantLockCheck(string FDate, string TDate, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string From = Convert.ToDateTime(FDate).ToString("dd-MMM-yyyy");
                string To = Convert.ToDateTime(TDate).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId='" + Plant + @"'
                and LockedDate between '" + From + "' and '" + To + "' and IsActive='1'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


    }

    public class WeeklyOffService
    {

        ISqlRepository _sqlRepository;
        public WeeklyOffService()
        {
            _sqlRepository = new SqlRepository();
        }


        public List<Dictionary<string, object>> ShiftDefinationSearch(string PlantId)
        {
            //ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT SystemID ShiftDefinationID, ShiftDefinationName, ShiftDefinationDescription, ShiftType, SequenceNo ShiftSequence, CONVERT(VARCHAR(10), InTime, 108) AS InTime,
                                        InTimeStartMargin, LateMargin, AbsentEndMargin, CONVERT(VARCHAR(10), OutTime, 108) AS OutTime,
                                        OutTimeEndMargin, OTStartTime, CONVERT(VARCHAR(10), BreakStratTime, 108) AS BreakStratTime,
                                        CONVERT(VARCHAR(10), BreakEndTime, 108) AS BreakEndTime, BreakPeriod, WorkingHour, IsActive, DefaultShift, IsGapInclude
                                FROM ShiftDefination WHERE  PlantID = '" + PlantId + @"' Order By ShiftDefinationName";

                return _sqlRepository.GetDataCollection(strSql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }

        public IEnumerable<object> getMaster()
        {
            try
            {
                var str = @"Select wo.*, format(wo.AddedDate,'dd-MMM-yyyy') as CreationDate
                            from dbo.WeekOffHeader wo";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getDateChild(string Id)
        {
            try
            {
                var str = @"Select Id, WOHeaderId, format(EffectiveDate,'dd-MMM-yyyy') as EffectiveDate from dbo.WeekOffEffectiveDate where WOHeaderId = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getDayChild(string Id)
        {
            try
            {
                var str = @"Select Id ,WOHeaderId,WOSequence, Day, DayType   from dbo.WeekOffChild where WOHeaderId = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        // The Section for Saving And Updating of Data
        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData, string addedname, string addeddate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            dr["AddedBy"] = addedname;
            dr["AddedDate"] = addeddate;
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }

        public string saveMasters(Dictionary<string, object> Master, List<Dictionary<string, object>> Effective)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string TableName = "dbo.WeekOffHeader";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Master["Id"] + "'", out dsMaster, false, "1");

                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();

                string DateId = ((DateTime.Now.Year).ToString()).Substring(2);
                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    addedname = identity.Name;
                    addeddate = System.DateTime.Now.ToString();
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Master["Id"] = DateId + _Id.ToString().PadLeft(4, '0');
                    AddNewRow(dsMaster.Tables[0], Master, addedname, addeddate);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["StandardName"] = Master["StandardName"];
                    dr["ShortName"] = Master["ShortName"];
                    dr["Description"] = Master["Description"];
                    dr["Remarks"] = Master["Remarks"];
                    dr["Active"] = Master["Active"];
                    dr["UserName"] = Master["UserName"];
                    dr["AddedBy"] = dsMaster.Tables[0].Rows[0]["AddedBy"];
                    dr["AddedDate"] = dsMaster.Tables[0].Rows[0]["AddedDate"];
                    dr["AddedFromIP"] = dsMaster.Tables[0].Rows[0]["AddedFromIP"];
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }

                #endregion data update

                // The Effective Date Child Table entry


                string TableName1 = "dbo.WeekOffEffectiveDate";
                DataSet dsChild;
                ConnectionManager.DAL.ConManager con1 = new ConnectionManager.DAL.ConManager("1");
                con1.OpenDataSetThroughAdapter("select * from " + TableName1 + " where WOHeaderId='" + Master["Id"] + "'", out dsChild, false, "1");
                if (dsChild.Tables[0].Rows.Count == 0)
                {
                    int indexa = 0;
                    for (int i = 0; i < Effective.Count; i++)
                    {
                        Dictionary<string, object> jj = Effective[i];
                        indexa++;
                        jj["WOHeaderId"] = Master["Id"];
                        jj["Id"] = Master["Id"] + indexa.ToString().PadLeft(2, '0');
                        addedname = identity.Name;
                        addeddate = System.DateTime.Now.ToString();
                        AddNewRow(dsChild.Tables[0], jj, addedname, addeddate);
                    }
                }
                else
                {

                    addedname = dsChild.Tables[0].Rows[0]["AddedBy"].ToString();
                    addeddate = dsChild.Tables[0].Rows[0]["AddedDate"].ToString();
                    for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                    {
                        dsChild.Tables[0].Rows[i].Delete();
                    }
                    dsChild.AcceptChanges();

                    int indexa = 0;
                    for (int i = 0; i < Effective.Count; i++)
                    {
                        Dictionary<string, object> jj = Effective[i];
                        indexa++;
                        jj["WOHeaderId"] = Master["Id"];
                        jj["Id"] = Master["Id"] + indexa.ToString().PadLeft(2, '0');

                        AddNewRow(dsChild.Tables[0], jj, addedname, addeddate);
                    }

                    var sqls = @"Delete from " + TableName1 + " where WOHeaderId = '" + Master["Id"] + "'";

                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                    objCone.CommitTransaction();
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);
                return Master["Id"].ToString();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //public string deleteMaster(string id)
        //{
        //    try
        //    {


        //        string TableName = "dbo.RosterPatternHeader";
        //        if (string.IsNullOrEmpty(id))
        //            throw new Exception("Select entry first");
        //        ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
        //        con.BeginTransaction();
        //        con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
        //        con.CommitTransaction();

        //        return "Success";

        //    }
        //    catch (Exception ex)
        //    {

        //        return ex.Message;

        //    }
        //}

        public void SaveDays(List<Dictionary<string, object>> Week, string HeaderId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.WeekOffChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where WOHeaderId='" + HeaderId + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    if (Week != null)
                    {
                        int indexa = 0;
                        for (int i = 0; i < Week.Count; i++)
                        {
                            Dictionary<string, object> jj = Week[i];
                            indexa++;
                            jj["Id"] = jj["WOHeaderId"] + indexa.ToString().PadLeft(2, '0');

                            AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                        }
                    }
                    else
                    {
                        throw new Exception("Please First Add Days!!");
                    }


                }
                else
                {
                    if (Week != null)
                    {
                        addedname = dsMaster.Tables[0].Rows[0]["AddedBy"].ToString();
                        addeddate = dsMaster.Tables[0].Rows[0]["AddedDate"].ToString();
                        for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                        {
                            dsMaster.Tables[0].Rows[i].Delete();
                        }
                        dsMaster.AcceptChanges();

                        int indexa = 0;
                        for (int i = 0; i < Week.Count; i++)
                        {
                            Dictionary<string, object> jj = Week[i];
                            indexa++;
                            jj["Id"] = jj["WOHeaderId"] + indexa.ToString().PadLeft(2, '0');

                            AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                        }

                    }

                    var sqls = @"Delete from " + TableName + " where WOHeaderId = '" + HeaderId + "'";

                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                    objCone.CommitTransaction();
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception e)
            {
                throw e;
            }

        }


    }

    public class WeekDefinitionService
    {

        ISqlRepository _sqlRepository;
        public WeekDefinitionService()
        {
            _sqlRepository = new SqlRepository();
        }


        public IEnumerable<object> getPlants()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"Select Username as Text , Id as Value from ORG.Plant where CompanyId = '" + identity.CompanyId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getCurrentList()
        {
            try
            {
                var str = @"Select ROW_NUMBER() OVER(ORDER BY ew.EffectiveDate desc) as Rows,ew.EmpSystemId, ew.WOHeaderId,format(ew.EffectiveDate,'dd-MMM-yyyy') as EffectiveDate
                            , ew.Id, ei.SystemId,ei.EmployeeCode,
                            ei.EmployeeName, wo.UserName as WOName
                            from dbo.EmployeeWeeklyOff ew
                            left join dbo.EmployeeInformation ei on ei.SystemId = ew.EmpSystemId
                            left join dbo.WeekOffHeader wo on wo.Id = ew.WOHeaderId
                            order by CAST(ew.EffectiveDate as Date) desc
                                ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        // The Section for Saving And Updating of Data
        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }



        //The Apis for the 2nd Page



        public void SaveFileList(List<Dictionary<string, object>> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.WeekDefination";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName, out dsMaster, false, "1");

                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }



                for (int i = 0; i < data.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "WD" + i.ToString();
                    dr["DayNo"] = data[i]["DayNo"].ToString();
                    dr["Days31"] = data[i]["Days31"].ToString();

                    if (bplib.clsWebLib.RetValidLen(data[i]["Days30"]).ToString() == "")
                        dr["Days30"] = DBNull.Value;
                    else
                        dr["Days30"] = data[i]["Days30"].ToString();

                    if (bplib.clsWebLib.RetValidLen(data[i]["Days29"]).ToString() == "")
                        dr["Days29"] = DBNull.Value;
                    else
                        dr["Days29"] = data[i]["Days29"].ToString();

                    if (bplib.clsWebLib.RetValidLen(data[i]["Days28"]).ToString() == "")
                        dr["Days28"] = DBNull.Value;
                    else
                        dr["Days28"] = data[i]["Days28"].ToString();

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getWeekDef()
        {
            try
            {
                var str = @"Select * from WeekDefination ";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getEmployeesAll()
        {
            try
            {
                var str = @"Select SystemId, EmployeeCode from dbo.EmployeeInformation
                           ";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable getRostersFile()
        {
            try
            {
                var str = @"Select * from dbo.WeekOffHeader";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public List<string> getRostersList()
        {
            try
            {
                var str1 = @"Select Id from dbo.WeekOffHeader ";
                DataTable dt = _sqlRepository.GetDataTable(str1);

                List<string> roster = new List<string>();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        roster.Add(dt.Rows[i]["Id"].ToString());
                    }
                }

                return roster;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// First Tab Functions
        public IEnumerable<object> getWeekOff()
        {
            try
            {
                var str = @"Select Id as value , UserName as text from dbo.WeekOffHeader";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmployees()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"Select SystemId , EmployeeCode , EmployeeName from dbo.EmployeeInformation where PlantId = '" + identity.PlantId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmpWeekOff(string EmpId)
        {
            try
            {
                var str = @"Select eow.EmpSystemId , isnull(eow.WOHeaderId,'') as WOHeaderId, format(eow.EffectiveDate,'dd-MMM-yyyy') as EffectiveDate
                            , isnull(wo.UserName,'') as UserName  from dbo.EmployeeWeeklyOff eow 
                            left join dbo.WeekOffHeader wo on wo.Id = eow.WOHeaderId
                            where EmpSystemId = '" + EmpId + "' order by EffectiveDate desc";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> getDistinctEmployeesToBeProcessed(string EffectiveDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var str = @"select distinct ex.EmpSystemId,e.EmployeeCode,e.EmployeeName,l.UserName as Designation,
                s.UserName AS Section,ss.UserName as SubSection,d.UserName as Department
                from EmployeeWeeklyOff ex 
                join EmployeeInformation e on e.SystemId=ex.EmpSystemId
                left join org.Department d on d.Id=e.DepartmentId
                left join org.Section s on s.Id=e.SectionId
                left join org.SubSection ss on ss.Id=e.SubSectionId
                left join hkp.LegalDesignation l on l.Id=e.LegalDesignationId
                where E.DOJ <= '" + EffectiveDate + @"' -- Effective Date 
                AND (E.DOS >= '" + EffectiveDate + "' OR ISNULL(E.DOS,'') = '' OR E.DOS = '01/01/1901')" +
                "and e.plantId='" + identity.PlantId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string ProcessAttendance(string EffectiveDate, string data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = DateTime.Now.ToString();

                #region Plant Lock Checking

                DataSet PlantLock;
                string FD = EffectiveDate;
                string TD = DateTime.Now.ToString("yyyy-MM-dd");
                PlantLockCheck(FD, TD, out PlantLock, identity.PlantId);
                string pl = "";
                if (PlantLock.Tables[0].Rows.Count > 0)
                {
                    for (var i = 0; i < PlantLock.Tables[0].Rows.Count; i++)
                    {
                        pl = pl + " " + PlantLock.Tables[0].Rows[i]["LockedDate"].ToString() + ", ";
                    }
                    return "The Plant is Locked for - " + pl;

                }
                #endregion

                string GettingRows = @"Select jj.* ,  (Select wcc.DayType from
                                                dbo.WeekOffChild wcc where wcc.WOSequence =jj.Seq 
                                                and wcc.WOHeaderId = jj.WeekOffHeaderId) 
                                                as DayType , ap.RowId , (Case when ap.RowId = jj.MyRowId then 1 else 0 end) as Checks
                                    from
                                                (Select ap.WorkDate, ap.EmpSystemID, format(ap.WorkDate,'yyyyMMdd')+ap.EmpSystemID as MyRowId,
                                                (Select distinct
                                                (DATEDIFF(DAY, (Select top 1 ed.EffectiveDate from
                                                dbo.WeekOffHeader h 
                                                left join dbo.WeekOffEffectiveDate ed on ed.WOHeaderId = h.Id
                                                where ed.EffectiveDate <= ap.WorkDate and ed.WOHeaderId =  
                                    (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                                                where EmpSystemId = e.SystemId and ex.EffectiveDate<=ap.WorkDate
                                                order by ex.EffectiveDate desc)
                                                order by ed.EffectiveDate desc) , ap.WorkDate) % 
                                                (Select max(WOSequence) from WeekOffHeader h 
                                                left join WeekOffChild wc on wc.WOHeaderId=h.Id 
                                                where h.Id =  
                                    (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                                                where EmpSystemId = e.SystemId and ex.EffectiveDate<=ap.WorkDate
                                                order by ex.EffectiveDate desc)
                                    )
                                    )+1 as DayDiff
                                                from 
                                                EmployeeInformation e
                                                left join EmployeeWeeklyOff ex on e.SystemId=ex.EmpSystemId
                                                where e.PlantId=ap.PlantID and e.SystemId = ap.EmpSystemID) as Seq,

                                                (Select top 1 ex.WOHeaderId from dbo.EmployeeWeeklyOff ex
                                                where EmpSystemId = ap.EmpSystemID and ex.EffectiveDate<=ap.WorkDate
                                                order by ex.EffectiveDate desc) WeekOffHeaderId 
                                    from AttdnProcessData ap 

                                    where ap.EmpSystemID in(" + data + ") and WorkDate between '" + FD + @"' and '" + TD + @"'
                                    )as jj
                                    left join AttdnProcessData ap on ap.WorkDate = jj.WorkDate 
                                    and ap.EmpSystemID In(" + data + ") and ap.WorkDate between '" + FD + @"' and '" + TD + @"'";

                DataTable dt = _sqlRepository.GetDataTable(GettingRows);
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.AttdnProcessData where EmpSystemID IN (" + data + ") and WorkDate between '" + FD + @"' and '" + TD + @"'", out dsMaster, false, "1");

                string RowMaster = "''";

                if (dt.Rows.Count > 0)
                {
                    for (var i = 0; i < dt.Rows.Count; i++)
                    {
                        RowMaster = RowMaster + ",'" + dt.Rows[i]["RowId"].ToString() + "'";
                        dsMaster.Tables[0].DefaultView.RowFilter = @"RowId ='" + dt.Rows[i]["RowId"].ToString() + "'";
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["WeeklyStatus"] = dt.Rows[i]["DayType"].ToString();
                        dr["isLock"] = false;
                        dr["ManualFlag"] = true;
                        dr["ManualEntryTime"] = DateTime.Now;
                        dr["ManualByWhom"] = identity.Name;
                        dr["LockedDate"] = DBNull.Value;
                        dr["LockedBy"] = DBNull.Value;
                        dr["IsOTComfirm"] = false;
                        dr["OTComfirmBy"] = DBNull.Value;
                        dr["DateOTComfirm"] = DBNull.Value;
                        dr.EndEdit();

                    }

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);

                    #region Attnd Process Call

                    NewAttendanceProcessService ap = new NewAttendanceProcessService();
                    ap.ManualScheduler(identity.PlantId, RowMaster);

                    #endregion
                }
                return "true";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void saveSingle(string EmpId, string EffectiveDate, string WeekId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = DateTime.Now.ToString();
                string TableName = "dbo.EmployeeWeeklyOff";


                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where EmpSystemId='" + EmpId + "' and EffectiveDate='" + EffectiveDate + "'", out dsMaster, false, "1");

                DataTable no = _sqlRepository.GetDataTable("Select top 1 Id as Nos from dbo.EmployeeWeeklyOff order by Cast(Id as numeric) desc");
                int id = int.Parse(no.Rows[0]["Nos"].ToString()) + 1;

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    if (dsMaster.Tables[0].Rows[0]["WOHeaderId"].ToString() == WeekId)
                    {
                        throw new Exception("Already the Same Data Exists !!");
                    }
                    else
                    {
                        DataRow ddr = dsMaster.Tables[0].Rows[0];
                        ddr.BeginEdit();
                        ddr["WOHeaderId"] = WeekId;
                        ddr["UpdatedBy"] = identity.Name;
                        ddr["UpdatedDate"] = DateTime.Now.ToString();
                        ddr["UpdatedFromIP"] = identity.IPAddress;
                        ddr.EndEdit();
                    }
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = id.ToString();
                    dr["EmpSystemId"] = EmpId;
                    dr["WOHeaderId"] = WeekId;
                    dr["EffectiveDate"] = Convert.ToDateTime(EffectiveDate).ToString("yyyy-MM-dd");
                    dr["AddedBy"] = addedname;
                    dr["AddedDate"] = addeddate;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void PlantLockCheck(string FDate, string TDate, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string From = Convert.ToDateTime(FDate).ToString("dd-MMM-yyyy");
                string To = Convert.ToDateTime(TDate).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId='" + Plant + @"'
                and LockedDate between '" + From + "' and '" + To + "' and IsActive='1'";

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