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
using Aplos.HumanResource;
using Library.Core;
using Library.Service.Enums;
using System.Reflection;
using Library.Service.Logs;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Systems;
using clsAttendance;

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
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ,ei.SubSectionId,ei.DepartmentId,ei.SectionId,
                            ld.UserName as LegalDesignation
							
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
                                                    left join hkp.LegalDesignation ld on ld.Id=ei.LegalDesignationId
                            where eI.PlantID='" + plantId+@"' and WorkDate between '"+_FromDate+@"' and '"+_ToDate+@"'
                            ";

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
                            , FORMAT(EI.DOJ, 'dd-MMM-yyyy') DOJ,ei.SubSectionId,ei.DepartmentId,ei.SectionId,
                            ld.UserName as LegalDesignation
							
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
                                                    left join hkp.LegalDesignation ld on ld.Id=ei.LegalDesignationId
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


        public IEnumerable<object> GetBalanceData()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = identity.PlantId;

                string Today = DateTime.Now.ToString("dd-MMM-yyyy");

                int Month = DateTime.Now.Month;
                int Year = DateTime.Now.Year;
                int Day = DateTime.Now.Day;

                string week = "";
                int Fd = 0;
                int Td = 0;

                if(Day >= 1 && Day<=8)
                {
                    week = "OT Time Setting (W-1)";
                    Fd = 1;
                    Td = 8;
                }
                else if(Day >=9 && Day<=16)
                {
                    week = "OT Time Setting (W-2)";
                    Fd = 9;
                    Td = 16;
                }
                else if (Day >= 17 && Day <= 24)
                {
                    week = "OT Time Setting (W-3)";
                    Fd = 17;
                    Td = 24;
                }
                else
                {
                    week = "OT Time Setting (W-4)";
                    Fd = 24;
                    Td = DateTime.DaysInMonth(Year, Month); ;
                }

                string FDt = (new DateTime(Year, Month, Fd)).ToString("dd-MMM-yyyy");
                string TDt = (new DateTime(Year, Month, Td)).ToString("dd-MMM-yyyy");

                var str = @"Select Pos.Code as PositionCode, mb.Code as BudgetCode, l.UserName as LegalDesg, U.UserName as Unit , s.UserName as Section , ss.UserName as SubSection,
                            ei.EmployeeCode ,ei.EmployeeName , ei.CellPhnNo , ei.PresentAddress1,
                            sum(apd.processedot) as ProcessedOT,
                            ((select isnull(MaxOTLimitParWeek,'0') as NormalDayOT from OTLimitSetting ol
                            where ol.PlantID='"+plantId+@"' AND ol.UserName='"+week+ @"') - sum(apd.ProcessedOT) ) as BalanceOT
                            from AttdnProcessData apd
                            left join EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join mst.ManpowerBudget mb on mb.Id = ei.BudgetCode
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Section s on s.Id=ei.SectionId
                            left join ORG.SubSection ss on ss.Id=ei.SubSectionId
                            left join org.Unit u on u.Id=ei.UnitId
                            left join hkp.LegalDesignation l on l.Id=ei.LegalDesignationId
                            where apd.WorkDate between '"+FDt+@"' and '"+TDt+@"' and ei.PlantId = '" + plantId + @"'
                            and apd.IsOTEntitled=1
                            and apd.EmpSystemID IN(SELECT EmpSystemID FROM AttdnProcessData WHERE PlantId='" + plantId + @"'
                            AND WorkDate='"+Today+@"' AND InStatus='IM' )
                            group by pos.Code , mb.Code , l.UserName , u.UserName , s.UserName , ss.UserName,ei.EmployeeCode ,ei.EmployeeName , ei.CellPhnNo ,ei.PresentAddress1
                            order by pos.Code desc, BalanceOT desc";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable GetBalanceDataReport(string EmpSystemId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var plantId = identity.PlantId;

                string Today = DateTime.Now.ToString("dd-MMM-yyyy");

                int Month = DateTime.Now.Month;
                int Year = DateTime.Now.Year;
                int Day = DateTime.Now.Day;

                string week = "";
                int Fd = 0;
                int Td = 0;

                if (Day >= 1 && Day <= 8)
                {
                    week = "OT Time Setting (W-1)";
                    Fd = 1;
                    Td = 8;
                }
                else if (Day >= 9 && Day <= 16)
                {
                    week = "OT Time Setting (W-2)";
                    Fd = 9;
                    Td = 16;
                }
                else if (Day >= 17 && Day <= 24)
                {
                    week = "OT Time Setting (W-3)";
                    Fd = 17;
                    Td = 24;
                }
                else
                {
                    week = "OT Time Setting (W-4)";
                    Fd = 24;
                    Td = DateTime.DaysInMonth(Year, Month); ;
                }

                string FDt = (new DateTime(Year, Month, Fd)).ToString("dd-MMM-yyyy");
                string TDt = (new DateTime(Year, Month, Td)).ToString("dd-MMM-yyyy");

                var str = @"Select Pos.Code as PositionCode, mb.Code as BudgetCode, l.UserName as LegalDesg, U.UserName as Unit , s.UserName as Section , ss.UserName as SubSection,
                            ei.EmployeeCode ,ei.EmployeeName , ei.CellPhnNo , ei.PresentAddress1,
                            sum(apd.processedot) as ProcessedOT,
                            ((select isnull(MaxOTLimitParWeek,'0') as NormalDayOT from OTLimitSetting ol
                            where ol.PlantID='" + plantId + @"' AND ol.UserName='" + week + @"') - sum(apd.ProcessedOT) ) as BalanceOT
                            from AttdnProcessData apd
                            left join EmployeeInformation ei on ei.SystemId = apd.EmpSystemID
                            left join mst.ManpowerBudget mb on mb.Id = ei.BudgetCode
                            left join org.Position pos on pos.Id = mb.PositionId
                            left join org.Section s on s.Id=ei.SectionId
                            left join ORG.SubSection ss on ss.Id=ei.SubSectionId
                            left join org.Unit u on u.Id=ei.UnitId
                            left join hkp.LegalDesignation l on l.Id=ei.LegalDesignationId
                            where apd.WorkDate between '" + FDt + @"' and '" + TDt + @"' and ei.PlantId = '" + plantId + @"'
                            and apd.IsOTEntitled=1
                            and apd.EmpSystemID IN(SELECT EmpSystemID FROM AttdnProcessData WHERE PlantId='" + plantId + @"'
                            AND WorkDate='" + Today + @"' AND InStatus='IM' )
                            group by pos.Code , mb.Code , l.UserName , u.UserName , s.UserName , ss.UserName,ei.EmployeeCode ,ei.EmployeeName , ei.CellPhnNo ,ei.PresentAddress1
                            order by pos.Code desc, BalanceOT desc";

                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception e)
            {
                throw e;
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

                #region Current Month Finding

                DataTable dtFNF = GetFNFEmployee(SystemId);
                if (dtFNF.Rows.Count>0)
                {
                    throw new Exception("Full and Final Settlement Employee can't be reactive.");
                }

                DataTable dt = GetEffectiveDateForAttdn(SystemId);
                DateTime FromDate = Convert.ToDateTime(dt.Rows[0]["ApprovedEffectiveDate"].ToString());
              
                string Today = DateTime.Now.ToString("dd-MMM-yyyy");
                int Month = DateTime.Now.Month;
                string StartDate = "";

                // For Finding Current Month for Row Creation and Plant Lock Checking
                while(FromDate<=Convert.ToDateTime(Today))
                {
                    int MonthCounter = FromDate.Month;
                    if(Month==MonthCounter)
                    {
                        StartDate = FromDate.ToString("dd-MMM-yyyy");
                        break;
                    }
                    
                    FromDate = FromDate.AddDays(1);
                }

                #endregion

                #region Plant Lock Checking

                DataSet PlantLock;
                PlantLockCheck(StartDate, Today, out PlantLock, identity.PlantId);
                string pl = "";
                if (PlantLock.Tables[0].Rows.Count > 0)
                {
                    for (var i = 0; i < PlantLock.Tables[0].Rows.Count; i++)
                    {
                        pl = pl + " " + PlantLock.Tables[0].Rows[i]["LockedDate"].ToString() + ", ";
                    }

                    throw new Exception("The Plant is Locked for - " + pl);
                }

                #endregion

                if (reason == null)
                {
                    throw new CustomException("Please Enter Reason", Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, "", "", false, ModuleEnum.Product.ToString()));
                }
                else
                {
                    #region SqlCommands Region

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
                    "values ('" + "ER"+Id + "'," +
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

                    #endregion

                    #region Attendance process

                    if(StartDate!="")
                    {
                        string CreatedEmpIds = "''";
                      
                        #region RowCreation Logic

                        ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter("select * from AttdnProcessData where WorkDate between '" + StartDate + "' and '" + Today + "' and PlantID = '" + identity.PlantId + "' and EmpSystemID='" + SystemId + "'", out DataSet dsRef, false, false, "", "1");

                        DateTime frmdate = Convert.ToDateTime(StartDate);
                        DateTime Todate = Convert.ToDateTime(Today);
                        int days = 0;

                        while (frmdate.AddDays(days) <= Todate)
                        {
                            string CurrentDate = Convert.ToString(Convert.ToDateTime(frmdate).AddDays(days));
                            DataSet RowCreationData; // Iterate b/w DOJ and Today's Date
                            RowCreation(out RowCreationData, identity.PlantId, CurrentDate, SystemId);
                            if (RowCreationData.Tables[0].Rows.Count > 0)
                            {
                                string EmpWkDate = RowCreationData.Tables[0].Rows[0][@"WorkDate"].ToString();

                                for (int i = 0; i < RowCreationData.Tables[0].Rows.Count; i++)
                                {
                                    string EmpId = RowCreationData.Tables[0].Rows[i][@"SystemId"].ToString();
                                    var GpId = RowCreationData.Tables[0].Rows[0][@"GroupID"].ToString();
                                    string RowId = RowCreationData.Tables[0].Rows[i][@"RowId"].ToString();
                                    string HoliDay = RowCreationData.Tables[0].Rows[i][@"HolidayStatus"].ToString();
                                    string WeekOfftype = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"WeekOfftype"]).ToString();
                                    string WeeklyStatus = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"WeeklyStatus"]).ToString();

                                    // Set Budgeted Shift as Default Shift  
                                    string BudgetShift = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"BudgetedShift"]).ToString();
                                    string BudgetShiftDurn = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"ShiftDuration"]).ToString();
                                    string BudgetShiftIn = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"BudgetShiftIn"]).ToString();
                                    string BudgetShiftOut = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"BudgetShiftOut"]).ToString();
                                    ShiftTime(ref BudgetShiftIn, ref BudgetShiftOut, EmpWkDate);

                                    var BudgetId = RowCreationData.Tables[0].Rows[i][@"BudgetId"].ToString();
                                    var FullDayDuration = RowCreationData.Tables[0].Rows[i][@"FullDayDuration"].ToString();
                                    var HalfDayDuration = RowCreationData.Tables[0].Rows[i][@"HalfDayDuration"].ToString();
                                    var ShortDuration = RowCreationData.Tables[0].Rows[i][@"ShortDuration"].ToString();
                                    var HoursWithoutOT = RowCreationData.Tables[0].Rows[i][@"HoursWithoutOT"].ToString();

                                    string HeaderId = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"HeaderId"]).ToString();
                                    string LeavePolicyId = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"LeavePolicyMasterId"]).ToString();
                                    var MonthData = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"Month"]).ToString();
                                    var Year = clsWebLib.RetValidLen(RowCreationData.Tables[0].Rows[i][@"Year"]).ToString();

                                    var PlantInPunchStartTime = RowCreationData.Tables[0].Rows[i][@"PlantInPunchStartTime"].ToString();
                                    PlantInTime(ref PlantInPunchStartTime, EmpWkDate);

                                    dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                                    
                                    if (dsRef.Tables[0].DefaultView.Count == 0 && Convert.ToBoolean(RowCreationData.Tables[0].Rows[i]["TobeAdded"].ToString()) == true)
                                    {
                                        DataRow dr = dsRef.Tables[0].NewRow();
                                        dr["EmpSystemID"] = EmpId;
                                        dr["RowId"] = RowId;
                                        dr["WorkDate"] = EmpWkDate; // Localizing Default Values
                                        dr["GroupID"] = GpId;
                                        dr["PlantID"] = PlantId;
                                        dr["OTMonth"] = MonthData;
                                        dr["OTYear"] = Year;

                                        dr["BudgetId"] = clsWebLib.RetValidLen(BudgetId);
                                        dr["PlantInPunchStartTime"] = clsWebLib.RetValidLen(PlantInPunchStartTime);
                                        dr["ManualFlag"] = true;

                                        if (BudgetShift.ToString() != "")
                                        {
                                            // Assigned Shift
                                            dr["ShiftSystemID"] = BudgetShift;
                                            dr["ShiftInTime"] = BudgetShiftIn;
                                            dr["ShiftOutTime"] = BudgetShiftOut;
                                            dr["BudgetedShiftID"] = BudgetShift;

                                            // Duration Columns
                                            dr["ShiftDuration"] = BudgetShiftDurn;
                                            dr["ShiftHalfDayDuration"] = clsWebLib.RetValidLen(HalfDayDuration);
                                            dr["ShiftShortDuration"] = clsWebLib.RetValidLen(ShortDuration);
                                            dr["ShiftFullDayDuration"] = clsWebLib.RetValidLen(FullDayDuration);
                                            dr["ShiftHoursWithoutOT"] = clsWebLib.RetValidLen(HoursWithoutOT);
                                        }

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
                                        dr["AddedBy"] = "ReActivationProcess";
                                        dr["DateAdded"] = Convert.ToDateTime(DateTime.Now);

                                        #endregion

                                        #region HeaderId Localized
                                        dr["DayStatusHeaderId"] = HeaderId;
                                        if (LeavePolicyId != "")
                                        {
                                            dr["LeavePolicyMasterId"] = LeavePolicyId;
                                        }
                                        #endregion

                                        if (HoliDay != "false")
                                        {
                                            dr["HolidayStatus"] = "H";
                                        }
                                        if (WeekOfftype == "CompanyWeekOff")
                                        {
                                            // Setting WeekOff Using Company WeekOff Setting
                                            dr["WeeklyStatus"] = WeeklyStatus;
                                        }

                                        dsRef.Tables[0].Rows.Add(dr);

                                        CheckerFunction(ref CreatedEmpIds, RowId); // loop in and Adding distinct RowIds

                                    }

                                }
                            }

                            days += 1; // Increment Day Counter
                        }
                        clsStaticInfo csl = new clsStaticInfo();
                        csl.SaveDataSets(dsRef); // Rows Saved

                        #endregion

                        #region Individual WeekOff Setting
                        // New Entry of Employees and Fetching from Range
                        DataSet IndividualWeekOfEmps;

                        IndividualWeekOffDataSet(out IndividualWeekOfEmps, StartDate, Today, SystemId);
                        if (IndividualWeekOfEmps.Tables[0].Rows.Count > 0)
                        {
                            ConnectionManager.DAL.ConManager conx = new ConnectionManager.DAL.ConManager("1");
                            conx.OpenDataSetThroughAdapter("select * from AttdnProcessData where EmpSystemID = '" + SystemId + "'and month(WorkDate) = '" + Month + "'", out DataSet dsMaster, false, false, "", "1");

                            for (int i = 0; i < IndividualWeekOfEmps.Tables[0].Rows.Count; i++)
                            {
                                string RowId = IndividualWeekOfEmps.Tables[0].Rows[i][@"RowId"].ToString();
                                string DayType = clsWebLib.RetValidLen(IndividualWeekOfEmps.Tables[0].Rows[i][@"DayType"]).ToString();

                                dsMaster.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";

                                if (dsMaster.Tables[0].DefaultView.Count > 0)
                                {
                                    // Calculated DayType and Setting Their Weekoffs in APD
                                    DataRow drx = dsMaster.Tables[0].DefaultView[0].Row;
                                    drx.BeginEdit();
                                    drx["WeeklyStatus"] = DayType;
                                    drx["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    drx["UpdatedBy"] = "ReActivationProcess";
                                    drx.EndEdit();
                                }
                            }
                            csl.SaveDataSets(dsMaster); // Rows Saved

                        }

                        #endregion

                        #region OTEligibleData Flagging
                        DataSet OTElgbEmp;
                        OTEligibleEmpSet(StartDate, Today, out OTElgbEmp, identity.PlantId, SystemId); // OT Eligible DataSet Generation
                        if (OTElgbEmp.Tables[0].Rows.Count > 0)
                        {
                            // Start Date of Month & Today to get RowIds 
                            ConnectionManager.DAL.ConManager newcon = new ConnectionManager.DAL.ConManager("1");
                            newcon.OpenDataSetThroughAdapter("select * from AttdnProcessData where EmpSystemID = '"+SystemId+"'and month(WorkDate) = '"+Month+"'", out DataSet dsOt, false, false, "", "1");

                            for (int i = 0; i < OTElgbEmp.Tables[0].Rows.Count; i++)
                            {
                                string RowId = OTElgbEmp.Tables[0].Rows[i][@"RowId"].ToString();
                                string IsOTEntitled = OTElgbEmp.Tables[0].Rows[i][@"IsOTEntitled"].ToString();

                                // Only RowIds that exist will come
                                dsOt.Tables[0].DefaultView.RowFilter = @"RowId='" + RowId + "' ";
                                if (dsOt.Tables[0].DefaultView.Count > 0)
                                {
                                    // Updation in APD Table for OT Entitled Employees
                                    DataRow dr = dsOt.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();

                                    dr["IsOTEntitled"] = clsWebLib.GetBoolData(IsOTEntitled);
                                    dr["UpdatedBy"] = "ReActivationProcess";
                                    dr["DateUpdated"] = Convert.ToDateTime(DateTime.Now);
                                    dr.EndEdit();
                                }
                            }
                            csl.SaveDataSets(dsOt);
                        }
                        #endregion

                        #region OTWeek Localization
                        if (StartDate != "")
                        {
                            string strSql = string.Empty;
                            DataSet dsWeekData;
                            OTWeekData(StartDate, Today, out dsWeekData, identity.PlantId);
                            if (dsWeekData.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i < dsWeekData.Tables[0].Rows.Count; i++)
                                {
                                    string Datex = clsWebLib.RetValidLen(dsWeekData.Tables[0].Rows[i]["WorkDate"]).ToString();
                                    string Week = clsWebLib.RetValidLen(dsWeekData.Tables[0].Rows[i]["OTWeek"]).ToString();

                                    if (Datex != "" && Week != "")
                                    {
                                        if (strSql.Length == 0)
                                        {
                                            strSql = @" update AttdnProcessData set OTWeek='" + Week + "' where WorkDate='" + Datex + "' and " +
                                                "PlantId='" + identity.PlantId + "' and RowId in (" + CreatedEmpIds + ") ;";
                                        }
                                        else
                                        {
                                            strSql += Environment.NewLine + @" update AttdnProcessData set OTWeek='" + Week + "' where WorkDate='" + Datex + "' and " +
                                                "PlantId='" + identity.PlantId + "' and RowId in (" + CreatedEmpIds + ") ;";
                                        }
                                    }
                                }
                                if (strSql.Length > 0)
                                {
                                    UpdateStatus(strSql); // OTWeek Updation
                                }
                            }
                        }
                        #endregion

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

        #region DataSet Region

        public void RowCreation(out DataSet ds, string Plant, string WkDate,string SystemId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                // DataSet For Row Creation of Reactivated Employees
                // It Will compare ShiftTime Change Master & Shift Defination

                string newformat = Convert.ToDateTime(WkDate).ToString("yyyyMMdd");

                var sql = @"select TobeAdded=case When isnull(p.EmpSystemID,'') ='' then 'true' 
			    else 'false' end , e.SystemId,'" + WkDate + @"' as WorkDate,Month('" + WkDate + @"') as Month,
				Year('" + WkDate + @"') as Year,
                convert(varchar(30),'" + newformat + @"' )+convert(varchar(30), e.SystemId)RowId,e.PlantId,
				e.GroupID,
                mb.ShiftDefinationId as BudgetedShift,isnull(stcm.InTime,sdy.InTime) as BudgetShiftIn,
				ISNULL(stcm.OutTime,sdy.OutTime) as BudgetShiftOut,
                ISNULL(stcm.ShiftDuration,sdy.ShiftDuration) as ShiftDuration,
				mb.Id as BudgetId,Op.InPunchStartTime as PlantInPunchStartTime, 
                FullDayDuration=ISNULL(stcm.FullDayDuration,sdy.FullDayDuration),HalfDayDuration=
				isnull(stcm.HalfDayDuration,sdy.HalfDayDuration),
				ShortDuration=ISNULL(stcm.ShortDuration,sdy.ShortDuration),
				HoursWithoutOT=ISNULL(stcm.HoursWithoutOT,sdy.HoursWithoutOT),
                HolidayStatus=isnull((select om.OffDayType
                from SCS.OffDayMaster om left join scs.OffDayDetail od
                on om.Id=od.OffDayMasterId where od.OffDayDate='" + WkDate + @"'
                and om.PlantId='" + Plant + @"' and om.OffDayType='H'),'false'),
                WeekOfftype=isnull((SELECT TOP(1) WOHeaderId FROM EmployeeWeeklyOff ex
				left join EmployeeInformation emp on emp.SystemId=ex.EmpSystemId
				where  
				emp.DOJ <= '" + WkDate + @"' AND (emp.DOS >= '" + WkDate + @"' OR 
				ISNULL(emp.DOS,'') = '' 
				OR emp.DOS = '01/01/1901') and emp.SystemId=e.SystemId ORDER BY ex.EffectiveDate DESC),'CompanyWeekOff'),
				WeeklyStatus=isnull((select od.OffDayType
				from scs.OffDayMaster od 
				left join scs.OffDayDetail odd on odd.OffDayMasterId=od.Id
				where od.OffDayType='W' 
				and od.PlantId='" + Plant + "' and odd.OffDayDate='" + WkDate + @"'),'NW'),
                dh.Id as HeaderId,dxc.LeavePolicyMasterId               
                from EmployeeInformation e 
                left join mst.DesignationMasterLegalDesignation ddm on ddm.LegalDesignationId = 
		        e.LegalDesignationId
				left join mst.DesignationMaster 
				dm on dm.Id = ddm.DesignationMasterId
				left join scs.DesignationMasterConfiguration dxc on dxc.DesignationMasterId=dm.Id
				and dxc.PlantId=e.PlantId
				left join DayStatusPlantChild 
				dc on dc.EmpTypeId=dm.EmployeeCategoryId
				and dc.PlantId=e.PlantId
				left join DayStatusHeader dh on dh.Id=dc.headerId        
                left join mst.ManpowerBudget mb on mb.Id=e.BudgetCode
                left join ShiftDefination sdy on sdy.SystemID=mb.ShiftDefinationId				  
				LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '" + WkDate + @"' 
							BETWEEN stcm.FromDate AND stcm.ToDate AND 
							sdy.SystemID=stcm.ShiftDefinationID                            
                left join org.Plant pl on pl.Id=e.PlantId
                left join OutPunchConfigurationHeader Op on OP.PlantId=pl.Id
				left join AttdnProcessData p on p.EmpSystemID=e.SystemId 
				and p.WorkDate='" + WkDate + @"'              
                where e.EmpType!='Guest' and e.PlantId='" + Plant + @"' and e.SystemID='"+SystemId+@"'
                and DOJ <= '" + WkDate + @"' AND (E.DOS >= '" + WkDate + @"' OR ISNULL(E.DOS,'') = '' 
				OR E.DOS = '01/01/1901') ";

                // Finds HolidayStatus,BudgetCode as well as Weekly Status of Company WeekOff
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void OTWeekData(string FromDate, string ToDate, out DataSet ds, string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var sql = @"select distinct Format(WorkDate,'dd-MMM-yyyy')WorkDate,
				OTWeek from AttdnProcessData where PlantID='" + PlantId + @"'
				and WorkDate between '" + FromDate + "' and '" + ToDate + @"'
				and OTWeek is not null";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
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
  
        void PlantInTime(ref string PlantInPunchStartTime, string WorkDate)
        {

            if (string.IsNullOrEmpty(PlantInPunchStartTime))
            {
                return;
            }
            PlantInPunchStartTime = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(PlantInPunchStartTime).ToString("hh:mm:ss tt");

        }

        public void CheckerFunction(ref string ManualFlagRowId, string Value)
        {
            if (ManualFlagRowId.Contains(Value))
            {
                return;
            }
            else
            {
                ManualFlagRowId += ",'" + Value + "'";
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

        public DataTable GetFNFEmployee(string EmpSystemId)
        {
            try
            {

                string sql = @"select * from EmployeeFullAndFinalSettlement Where EmpSystemId='"+ EmpSystemId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
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

        public void IndividualWeekOffDataSet(out DataSet ds, string FromDate, string ToDate, string EmpMaster)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                // It finds all the Weekoff Values of Range of Dates from Start To Today's Date 
                // That have Week Off other than Company Week Off ....
                var sql = @"select dd.* from (Select jj.* ,  (Select wcc.DayType from
                                                 
												    dbo.WeekOffChild wcc where wcc.WOSequence =jj.Seq 
                                                    and wcc.WOHeaderId = jj.WeekOffHeaderId) 
                                                    as DayType , ap.RowId , (Case when 
													ap.RowId = jj.MyRowId then 1 else 0 end) as Checks
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

                                        where ap.EmpSystemID='"+ EmpMaster +@"' and WorkDate 
										between '" + FromDate + @"' and '" + ToDate + @"'
                                        )as jj
                                        left join AttdnProcessData ap on
										ap.WorkDate = jj.WorkDate and 
										ap.EmpSystemID='"+ EmpMaster + @"' and ap.WorkDate 
										between '" + FromDate + @"' and '" + ToDate + @"'
										)as dd where dd.Checks=1 and isnull(dd.DayType,'')!=''";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void OTEligibleEmpSet(string FromDate, string ToDate, out DataSet ds, string PlantId, string SystemId)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                var sql = @"select distinct e.SystemId as EmpId,dc.IsOTEntitled,
				Format(p.WorkDate,'yyyy-MMM-dd')WorkDate,(Format(p.WorkDate,'yyyyMMdd')+e.SystemId)
				as RowId
                from AttdnProcessData p join
                EmployeeInformation e on e.SystemId=p.EmpSystemID    
				left join mst.DesignationMasterLegalDesignation ddm on 
                ddm.LegalDesignationId = e.LegalDesignationId
                left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
				left join scs.DesignationMasterConfiguration dc on dc.DesignationMasterId=dm.Id
                and dc.PlantId=e.PlantId
                where p.WorkDate between '" + FromDate + @"' and '" + ToDate + @"' and
				e.PlantId='" + PlantId + @"' 
                and E.DOJ <= '" + ToDate + @"' 
				AND (E.DOS >= '" + ToDate + @"' OR ISNULL(E.DOS,'') = '' 
				OR E.DOS = '01/01/1901')and dc.IsOTEntitled=1 and  e.SystemId='"+SystemId+@"' 
				and e.SystemId not in (select final.EmpSystemId from 
				(select distinct o.empsystemId,
				(select top 1 Exclude from NonEligibleOT m where 
				m.EmpSystemId=o.EmpSystemId order by EffectiveDate desc)as x 
				from NonEligibleOT o) final where final.x=1)";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void UpdateStatus(string sql)
        {
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(sql, true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }

        #endregion

    }
}

