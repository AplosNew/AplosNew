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
                var str = @"Select SystemId , EmployeeCode , EmployeeName from dbo.EmployeeInformation where PlantId = '"+identity.PlantId+"'";
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
                            where EmpSystemId = '" + EmpId+"' order by EffectiveDate desc";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
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

}