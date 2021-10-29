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
using Library.Core;
using Library.Service.Enums;
using System.Reflection;
using Library.Service.Logs;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Systems;

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

        public DataSet GetCalYearInfo(string CalYearId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"select * from YearlyCalendar WHERE ID='" + CalYearId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }


        public IEnumerable<object> GetData(string calYearId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = identity.PlantId;
                
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                }

                var sql = @"select distinct SystemId as EmpId,EI.EmployeeCode
                        	,EI.EmployeeName
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ,ei.SubSectionId,ei.DepartmentId,ei.SectionId
							
                            ,Jan=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='1' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Feb=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='2' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Mar=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='3' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
    
                            Apr=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='4' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            May=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='5' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')

							,June=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='6' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')

							,July=isnull((select SUM(presentvalue)+sum(latevalue)
                            from AttdnProcessData y
                            where MONTH(workdate)='7' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')
							                 
                            ,Aug=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='8' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Sep=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='9' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"' 
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Oct=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='10' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Nov=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='11' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Dec=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='12' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')
                            
                            from attdnprocessdata a JOIN
                            EmployeeInformation eI on eI.SystemId=a.EmpSystemID
                                                    LEFT JOIN ORG.Department DP ON DP.Id = EI.DepartmentId
                                                    LEFT JOIN ORG.Section AS Se ON Se.Id = EI.SectionID
                                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = EI.SubSectionID                        
                            where eI.PlantID='" + plantId+@"' and WorkDate between '"+_FromDate+@"' and '"+_ToDate+@"'
                            AND (ei.EmployeeStatus='Active')";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetReportData(string EmpId,string calYearId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = identity.PlantId;
               
                string _FromDate = string.Empty;
                string _ToDate = string.Empty;
                var dsCalYear = GetCalYearInfo(calYearId);
                if (dsCalYear.Tables[0].Rows.Count > 0)
                {
                    _FromDate = dsCalYear.Tables[0].Rows[0]["FromDate"].ToString();
                    _ToDate = dsCalYear.Tables[0].Rows[0]["ToDate"].ToString();
                }

                var sql = @"select distinct SystemId as EmpId,EI.EmployeeCode
                        	,EI.EmployeeName
                            , DP.UserName Department
                            , se.UserName Section
                            , Sus.UserName SubSection
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ,ei.SubSectionId,ei.DepartmentId,ei.SectionId
							
                            ,Jan=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='1' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Feb=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='2' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Mar=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='3' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
    
                            Apr=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='4' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            May=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='5' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')

							,June=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='6' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')

							,July=isnull((select SUM(presentvalue)+sum(latevalue)
                            from AttdnProcessData y
                            where MONTH(workdate)='7' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')
							                 
                            ,Aug=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='8' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Sep=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='9' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"' 
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Oct=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='10' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Nov=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='11' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0'),
                            
                            Dec=isnull((select SUM(presentvalue)+sum(latevalue) from AttdnProcessData y
                            where MONTH(workdate)='12' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
                            and PlantID='" + plantId + @"'
                            and y.EmpSystemID=a.EmpSystemID
                            group by EmpSystemID),'0')
                            
                            from attdnprocessdata a JOIN
                            EmployeeInformation eI on eI.SystemId=a.EmpSystemID
                                                    LEFT JOIN ORG.Department DP ON DP.Id = EI.DepartmentId
                                                    LEFT JOIN ORG.Section AS Se ON Se.Id = EI.SectionID
                                                    LEFT JOIN ORG.SubSection AS SuS ON SuS.Id = EI.SubSectionID                        
                            where eI.PlantID='" + plantId + @"' and WorkDate between '" + _FromDate + @"' and '" + _ToDate + @"'
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

    public class ActiveInActiveEmpNewProcessService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public ActiveInActiveEmpNewProcessService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }


        public void InActiveToActiveNewAttdnProcess(string SystemId, string reason)
        {

            try
            {
                clsGenID genid = new clsGenID();
                genid.GenID("EmployeeReactivation", out string Id);

                // PoValue = "0";
               // var Id = GetPK();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var AddedFromIp = identity.IPAddress;
                var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var AddedBy = identity.Name;
                var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
                var CompanyGroupId = identity.CompanyGroupId;
                var CompanyId = identity.CompanyId;
                var PlantId = identity.PlantId;
                var EmployeeId = SystemId;
                var Reason = reason;

                //Lock
                DataTable dt = GetEffectiveDateForAttdn(SystemId);
                DateTime FromDate = Convert.ToDateTime(dt.Rows[0]["ApprovedEffectiveDate"].ToString());
                FromDate = FromDate.AddDays(1);

               // AttendanceProcessAplos ob = new AttendanceProcessAplos();
                //ob.LockValidation(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), DateTime.Now.ToString("dd-MMM-yyyy"), SystemId);

                if (reason == null)
                {
                    throw new CustomException("Please Enter Reason", Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, "", "", false, ModuleEnum.Product.ToString()));
                }
                else
                {
                    string _sql = "Update dbo.EmployeeInformation set DOS=null,DOSBy=null,DOSDate=null,EmployeeStatus='Active' where SystemId='" + SystemId + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                    string _sql1 = "Insert into EmployeeReactivation(Id," +
                    "CompanyGroupId," +
                    "CompanyId," +
                    "PlantId," +
                    "EmployeeId," +
                    "Reason," +
                    "AddedBy," +
                    "AddedDate," +
                    "AddedFromIp," +
                    "UpdatedBy," +
                    "UpdatedDate," +
                    "UpdatedFromIp) " +
                    "values ('" + Id + "'," +
                    "'" + CompanyGroupId + "'," +
                    "'" + CompanyId + "'," +
                    "'" + PlantId + "'," +
                    "'" + SystemId + "'," +
                     "'" + Reason + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                    "'" + AddedFromIp + "'," +
                    "'" + AddedBy + "'," +
                    "'" + AddedDate + "'," +
                      "'" + AddedFromIp + "')";
                    _sqlRepository.ExecuteSqlCommand(_sql1);
                    #region Attendance process
                    clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                    //DataTable dt = GetEffectiveDateForAttdn(SystemId);

                    //DateTime FromDate = Convert.ToDateTime(dt.Rows[0]["ApprovedEffectiveDate"].ToString());
                    DateTime ToDate = DateTime.Now;
                    while (FromDate <= ToDate)
                    {
                        AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                        obj.SaveTotal(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), SystemId, false, true);//Main Function for attendace Process
                        FromDate = FromDate.AddDays(1);
                    }


                    #endregion
                }


            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public DataTable GetEffectiveDateForAttdn(string EmpSystemId)
        {
            try
            {

                string sql = @"SELECT top 1 FORMAT(ApprovedEffectiveDate,'dd-MMM-yyyy')  ApprovedEffectiveDate                                   
                                    FROM [TRN].[Resignation]
                                    where EmployeeId='" + EmpSystemId + "' order by AddedDate desc";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }





    }
}

