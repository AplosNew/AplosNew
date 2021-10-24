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

namespace Library.HumanResource.NewAttendanceProcess
{
    public class NewAttdnProcessPlantLockService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public NewAttdnProcessPlantLockService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        public string GetUnLockedEmployees(string Date,string PlantId)
        {
            try
            {
                var sql = @"select e.EmployeeCode,e.EmployeeName,a.EmpSystemID,format(a.WorkDate,'yyyy-MMM-dd')WorkDate,
                a.DayStatus,a.IsLock,a.LockedBy,
                ent.UserName as Entity,u.UserName as Unit,format(e.DOJ,'yyyy-MMM-dd')DOJ,
                s.UserName as Section,ss.UserName as SubSection,dept.UserName as Department
                FROM AttdnProcessData A left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = e.BudgetCode
                LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId    
                LEFT JOIN [ORG].[Unit] u ON u.Id = ENT.UnitId
                LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId  
                LEFT JOIN [ORG].[Department] dept ON dept.Id = POS.DepartmentId
                LEFT JOIN [ORG].[Section] s ON s.Id = POS.SectionId
                LEFT JOIN [ORG].[SubSection] ss ON ss.Id = POS.SubSectionId                           
                where WorkDate='" + Date + @"' and e.EmployeeStatus='Active'
                and IsLock=0 AND a.PlantID='" + PlantId + "'";
               
                return sql;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetExpectedLockedDate(string PlantId)
        {
            try
            {
                var sql = @"select top 1 Id,LockedDate,PlantId,
                Format(dateadd(DD, +1, cast(LockedDate as date)),'dd-MMM-yyyy')as ExpectedDate
                from PlantWiseAttendanceLock where PlantId='" + PlantId+@"' and IsActive='1'
                order by LockedDate desc";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public string GetLockedEmployees(string Date,string PlantId)
        {
            try
            {
             
                var sql = @"select e.EmployeeCode,e.EmployeeName,a.EmpSystemID,format(a.WorkDate,'yyyy-MMM-dd')WorkDate,
                a.DayStatus,a.IsLock,a.LockedBy,
                ent.UserName as Entity,u.UserName as Unit,format(e.DOJ,'yyyy-MMM-dd')DOJ,
                s.UserName as Section,ss.UserName as SubSection,dept.UserName as Department
                FROM AttdnProcessData A left join EmployeeInformation e on e.SystemId=a.EmpSystemID
                LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id = e.BudgetCode
                LEFT JOIN ORG.Entity AS ENT ON ENT.Id = MB.EntityId    
                LEFT JOIN [ORG].[Unit] u ON u.Id = ENT.UnitId
                LEFT JOIN ORG.Position AS POS ON POS.Id = MB.PositionId  
                LEFT JOIN [ORG].[Department] dept ON dept.Id = POS.DepartmentId
                LEFT JOIN [ORG].[Section] s ON s.Id = POS.SectionId
                LEFT JOIN [ORG].[SubSection] ss ON ss.Id = POS.SubSectionId                           
                where WorkDate='"+Date+@"' and e.EmployeeStatus='Active'
                and IsLock=1 AND a.PlantID='"+PlantId+"'";
               
                return sql;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void LockAttdn(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter("select * from PlantWiseAttendanceLock where LockedDate='" + Date + "' and PlantId='" + identity.PlantId + "'", out DataSet dsRef, false, false, "", "1");

            dsRef.Tables[0].DefaultView.RowFilter = @"PlantId='" + identity.PlantId + "' ";
            if (dsRef.Tables[0].DefaultView.Count == 0)
            {

                clsGenID genid = new clsGenID();
                genid.GenID("PlantWiseAttendanceLock", out string _Id);

                DataRow dr = dsRef.Tables[0].NewRow();
                dr["Id"] ="AL"+ _Id;
                dr["LockedDate"] = Date;
                dr["IsActive"] = true;
                dr["PlantId"] =identity.PlantId;
                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = Convert.ToDateTime(DateTime.Now);
                dr["AddedFromIP"] = identity.IPAddress;

                dsRef.Tables[0].Rows.Add(dr);

            }
            else
            {
                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                dr.BeginEdit();
                dr["IsActive"] = true;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = Convert.ToDateTime(DateTime.Now);
                dr["UpdatedFromIP"] = identity.IPAddress;
                dr.EndEdit();
            }

            clsStaticInfo info = new clsStaticInfo();
            info.SaveDataSets(dsRef);
        }

        public int UnLockAttdn(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter("select * from PlantWiseAttendanceLock where LockedDate='" + Date + "' and PlantId='" + identity.PlantId + "'", out DataSet dsRef, false, false, "", "1");

            dsRef.Tables[0].DefaultView.RowFilter = @"PlantId='" + identity.PlantId + "' ";
            int i = 0;
            if (dsRef.Tables[0].DefaultView.Count > 0)
            {
                DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                dr.BeginEdit();
                dr["IsActive"] = false;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = Convert.ToDateTime(DateTime.Now);
                dr["UpdatedFromIP"] = identity.IPAddress;
                dr.EndEdit();
                i++;
            }

            clsStaticInfo info = new clsStaticInfo();
            info.SaveDataSets(dsRef);
            return i;
        }

    }

    public class FullYearPresentDaysCount
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public FullYearPresentDaysCount()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
       

        public IEnumerable<object> GetData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = identity.PlantId;
                var date = DateTime.Now.ToString("dd-MMM-yyyy");

                var sql = @"select distinct SystemId as EmpId,EI.EmployeeCode
                        	,EI.EmployeeName
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ,ei.SubSectionId,ei.DepartmentId,ei.SectionId
							
                            ,Jan=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='1'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Feb=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='2'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Mar=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='3'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
    
                            Apr=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='4'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            May=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='5'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')

							,June=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='6'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')

							,July=isnull((select SUM(presentvalue)+sum(latevalue)
                            from AttdnProcessData y
                            where MONTH(workdate)='7'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')
							                 
                            ,Aug=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='8'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Sep=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='9'
                            and PlantID='" + plantId + @"' 
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Oct=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='10'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Nov=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='11'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Dec=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='12'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')
                            
                            from attdnprocessdata a JOIN
                            EmployeeInformation eI on eI.SystemId=a.EmpSystemID
                                                    LEFT JOIN ORG.Department DP ON DP.Id = EI.DepartmentId
                                                    LEFT JOIN ORG.Section AS Se ON Se.Id = EI.SectionID
                                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = EI.SubSectionID                        
                            where eI.PlantID='" + plantId+@"' and year(WorkDate)=year('"+date+@"')
                            AND (ei.EmployeeStatus='Active')";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetReportData(string EmpId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = identity.PlantId;
                var date = DateTime.Now.ToString("dd-MMM-yyyy");

                var sql = @"select distinct SystemId as EmpId,EI.EmployeeCode
                        	,EI.EmployeeName
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ,ei.SubSectionId,ei.DepartmentId,ei.SectionId
							
                            ,Jan=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='1'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Feb=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='2'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Mar=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='3'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
    
                            Apr=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='4'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            May=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='5'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')

							,June=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='6'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')

							,July=isnull((select SUM(presentvalue)+sum(latevalue)
                            from AttdnProcessData y
                            where MONTH(workdate)='7'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')
							                 
                            ,Aug=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='8'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Sep=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='9'
                            and PlantID='" + plantId + @"' 
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Oct=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='10'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Nov=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='11'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Dec=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='12'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')
                            
                            from attdnprocessdata a JOIN
                            EmployeeInformation eI on eI.SystemId=a.EmpSystemID
                                                    LEFT JOIN ORG.Department DP ON DP.Id = EI.DepartmentId
                                                    LEFT JOIN ORG.Section AS Se ON Se.Id = EI.SectionID
                                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = EI.SubSectionID                        
                            where eI.PlantID='" + plantId + @"' and year(WorkDate)=year('" + date + @"')
                            AND (ei.EmployeeStatus='Active')
                            --- Filters
                            and isnull(ei.SystemId, '') IN(" + EmpId + @")";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }


}

