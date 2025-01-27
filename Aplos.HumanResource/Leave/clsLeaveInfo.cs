using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Leave
{
    public class clsLeaveInfo
    {
        ISqlRepository _sqlRepository;

        public clsLeaveInfo()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> GetGeneral(string id, string EmpSystemid)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select d.SystemID, LT.LeaveDays, L.UserName LeaveTypeName,format( LT.FromDate , 'dd-MMM-yyyy') FromDate,format( LT.ToDate , 'dd-MMM-yyyy') ToDate,FORMAT(d.WorkDate,'dd-MMM-yyyy') WorkDate,a.DayStatus,
                            CONCAT(sd.ShiftType,'(' ,format (sd.InTime, 'HH:mm'),'-',format (sd.OutTime, 'HH:mm'),')') shiftTime, 
                            concat (case when a.InTime is not null then format (a.InTime, 'HH:mm')
                           -- when m.InTime is not null then format (m.InTime, 'HH:mm')
                            else format (a.PunchInTime, 'HH:mm') end,
							'-', 
							case when a.OutTime is not null then format (a.OutTime, 'HH:mm')
                           -- when m.OutTime is not null then format (m.OutTime, 'HH:mm')
                            else format (a.PunchOutTime, 'HH:mm') end) punchTime

							,format (m.InTime, 'HH:mm')+'-'+format (m.OutTime, 'HH:mm') ManualTime
                            from LeaveTransactionDetails d
                            left join AttdnProcessData a on a.WorkDate=d.WorkDate and a.EmpSystemID='" + EmpSystemid + @"'
                            left join AttdnManualData m on m.WorkDate=d.WorkDate and m.EmpSystemID='"+ EmpSystemid + @"'
                            left join ShiftDefination sd on sd.SystemID = a.ShiftSystemID
							left join [dbo].[LeaveTransaction] AS LT on LT.SystemID = d.LvTrnsSystemID
							LEFT JOIN [dbo].[LeaveType] AS L ON L.Id=LT.LTSystemID
                            where LvTrnsSystemID= '" + id + "' order by d.WorkDate";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public void DeleteLeave(string ID, string DetailId, bool _isFromDate, bool _isToDate,string FromDate, string ToDate)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from LeaveTransactionDetails where SystemID='" + ID + "'");

                if(_isFromDate)
                {
                    string q = "update LeaveTransaction set LeaveDays = (select SUM(LeaveDuration)d from LeaveTransactionDetails where LvTrnsSystemID = '" + DetailId + @"'
                             ) , FromDate = '" + FromDate + @"' where SystemID='" + DetailId + @" '";
                    con.executeQuery(q);
                }
                else if (_isToDate)
                {
                    con.executeQuery("update LeaveTransaction set LeaveDays = (select SUM(LeaveDuration)d from LeaveTransactionDetails where LvTrnsSystemID = '" + DetailId + @"'
                             ) , ToDate = '" + ToDate + @"'
                            where SystemID='" + DetailId + @" '");
                }
               else
                {
                    con.executeQuery("update LeaveTransaction set LeaveDays = (select SUM(LeaveDuration)d from LeaveTransactionDetails where LvTrnsSystemID = '" + DetailId + @"'
                             ) 
                            where SystemID='" + DetailId + @" '");
                }
              

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetEmpLeaveListForSingleDelete( string companyGroupId, string companyId, string plantId, string employeeId, string yearNo)
        {
            try
            {
                string strSQL = string.Empty;
                var fromDate = string.Empty;
                var toDate = string.Empty;
                DataSet dsYear = null;
                GetYearlyCalendarDetails(yearNo, out dsYear);
                if (dsYear.Tables[0].Rows.Count > 0)
                {
                    fromDate = dsYear.Tables[0].Rows[0]["FromDate"].ToString();
                    toDate = dsYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {
                    var newyear = DateTime.Now.Year;
                    fromDate = "01-Jan-" + newyear;
                    toDate = "31-Dec-" + newyear;
                }
                strSQL = @"SELECT LT.*, e.SectionId , format(LT.FromDate,'dd-MMM-yyyy') Fdate,format(LT.ToDate,'dd-MMM-yyyy') Tdate, 
                                        L.UserName  AS leaveTypeName,e.EmployeeName,e.EmployeeCode,e.EmpPicPath,
                                        dpt.UserName Depertment,ds.username Designation, de.duration
                                        FROM [dbo].[LeaveTransaction] AS LT
                                        LEFT JOIN [dbo].[LeaveType] AS L ON L.Id=LT.LTSystemID
                                        LEFT JOIN EmployeeInformation E ON E.SystemId=LT.EmpSystemID
                                        LEFT JOIN ORG.Department dpt on dpt.Id = e.DepartmentId
                                        LEFT JOIN HKP.Designation ds on ds.Id = e.DesignationSystemID
                                        left join (select sum(LeaveDuration) duration,LvTrnsSystemID from LeaveTransactionDetails group by LvTrnsSystemID) de on de.LvTrnsSystemID = lt.SystemID
                                    WHERE  LT.EmpSystemID=e.SystemId and lt.PlantID = '" + plantId + @"' and de.duration > 1
                                    and LT.SystemID in(--33
                                    select d.LvTrnsSystemID
                                    from LeaveTransactionDetails d
                                    left join AttdnProcessData a on a.WorkDate=d.WorkDate and a.EmpSystemID=e.SystemId and a.LeaveDuration = 1
                                    left join AttdnManualData m on m.WorkDate=d.WorkDate and m.EmpSystemID=e.SystemId 
                                    inner join DayType dt on dt.DayType = a.DayStatus --and dt.category in ('Present','Late')
                                    
                                    where a.EmpSystemID=e.SystemId and 
                                    (a.InTime is not null or a.PunchInTime is not null or m.InTime is not null
                                    or a.OutTime is not null or a.PunchOutTime is not null or m.OutTime is not null
                                    )
                                        )--33
                                    AND ((LT.FromDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
                                    OR LT.ToDate BETWEEN '" + fromDate + @"' AND '" + toDate + "') OR L.LeaveType ='Maternity') ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }

        }//End Function

        public IEnumerable<object> GetAllLeave(string EmpSystemid, string plantId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT FORMAT(LT.FromDate,'dd-MMM-yyyy') LFromDate, FORMAT(LT.ToDate,'dd-MMM-yyyy') LToDate,Approved = case when LT.IsApproved = 1 then 'Yes' else 'No' end ,e.SectionId , L.UserName  AS leaveTypeName
                            FROM [dbo].[LeaveTransaction] AS LT
                            LEFT JOIN [dbo].[LeaveType] AS L ON L.Id=LT.LTSystemID
                            LEFT JOIN EmployeeInformation E ON E.SystemId=LT.EmpSystemID
                            LEFT JOIN ORG.Department dpt on dpt.Id = e.DepartmentId
                            LEFT JOIN HKP.Designation ds on ds.Id = e.DesignationSystemID
                            WHERE  LT.EmpSystemID=e.SystemId and lt.PlantID = '" + plantId + @"'
                            and e.SystemId = '" + EmpSystemid + @"' order by LT.FromDate,MONTH(ToDate)";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public void GetYearlyCalendarDetails(string YearId, out DataSet dsYear)
        {

            try
            {

                ConnectionManager.DAL.ConManager objCon;
                string sql = @"SELECT YearNo,FORMAT(FromDate,'dd-MMM-yyyy')  FromDate,FORMAT(ToDate,'dd-MMM-yyyy')  ToDate, IsYearEndClosed, PlantId, Id FROM YearlyCalendar WHERE Id='" + YearId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsYear, false, "1");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public IEnumerable<object> GetEmployeeList(string plantId, string companyId, string YearId)
        {
            try
            {
                string CmdText = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                    PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                    ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,
                                    EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                                    FROM EmployeeInformation EMP
                                    LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                    LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                    LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                    LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                    LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                    LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                    LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                    LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                    LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                    WHERE emp.PlantID='" + plantId + @"'  and EMP.CompanyId='" + companyId + @"'
                                    and emp.SystemId in( select t.EmpSystemID
                                    from LeaveTransactionDetails d
                                    inner join LeaveTransaction t on t.SystemID=d.LvTrnsSystemID
                                    left join AttdnProcessData a on a.WorkDate=d.WorkDate and a.EmpSystemID=t.EmpSystemID
                                    inner join DayType dt on dt.DayType = a.DayStatus and dt.category in ('Present','Late')
                                    where YEAR(d.WorkDate)=(select YearNo from YearlyCalendar where Id = '" + YearId + @"'))
                                    ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        // New Attendance Process
        public IEnumerable<object> GetGeneralNew(string id, string EmpSystemid)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select d.SystemID, LT.LeaveDays, L.UserName LeaveTypeName,format( LT.FromDate , 'dd-MMM-yyyy') FromDate,format( LT.ToDate , 'dd-MMM-yyyy') ToDate,FORMAT(d.WorkDate,'dd-MMM-yyyy') WorkDate,a.DayStatus,
                            CONCAT(sd.ShiftType,'(' ,format (sd.InTime, 'HH:mm'),'-',format (sd.OutTime, 'HH:mm'),')') shiftTime, 
                            concat (case when a.InTime is not null then format (a.InTime, 'HH:mm')
                           -- when m.InTime is not null then format (m.InTime, 'HH:mm')
                            else format (a.PunchInTime, 'HH:mm') end,
							'-', 
							case when a.OutTime is not null then format (a.OutTime, 'HH:mm')
                           -- when m.OutTime is not null then format (m.OutTime, 'HH:mm')
                            else format (a.PunchOutTime, 'HH:mm') end) punchTime

							,format (a.ManualInTime, 'HH:mm')+'-'+format (a.ManualOutTime, 'HH:mm') ManualTime
                            from LeaveTransactionDetails d
                            left join AttdnProcessData a on a.WorkDate=d.WorkDate and a.EmpSystemID='" + EmpSystemid + @"'
                            left join ShiftDefination sd on sd.SystemID = a.ShiftSystemID
							left join [dbo].[LeaveTransaction] AS LT on LT.SystemID = d.LvTrnsSystemID
							LEFT JOIN [dbo].[LeaveType] AS L ON L.Id=LT.LTSystemID
                            where LvTrnsSystemID= '" + id + "' order by d.WorkDate";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetEmpLeaveListForSingleDeleteNew(string companyGroupId, string companyId, string plantId, string employeeId, string yearNo)
        {
            try
            {
                string strSQL = string.Empty;
                var fromDate = string.Empty;
                var toDate = string.Empty;
                DataSet dsYear = null;
                GetYearlyCalendarDetails(yearNo, out dsYear);
                if (dsYear.Tables[0].Rows.Count > 0)
                {
                    fromDate = dsYear.Tables[0].Rows[0]["FromDate"].ToString();
                    toDate = dsYear.Tables[0].Rows[0]["ToDate"].ToString();
                }
                else
                {
                    var newyear = DateTime.Now.Year;
                    fromDate = "01-Jan-" + newyear;
                    toDate = "31-Dec-" + newyear;
                }
                strSQL = @"SELECT LT.*, e.SectionId , format(LT.FromDate,'dd-MMM-yyyy') Fdate,format(LT.ToDate,'dd-MMM-yyyy') Tdate, 
                                        L.UserName  AS leaveTypeName,e.EmployeeName,e.EmployeeCode,e.EmpPicPath,
                                        dpt.UserName Depertment,ds.username Designation, de.duration
                                        FROM [dbo].[LeaveTransaction] AS LT
                                        LEFT JOIN [dbo].[LeaveType] AS L ON L.Id=LT.LTSystemID
                                        LEFT JOIN EmployeeInformation E ON E.SystemId=LT.EmpSystemID
                                        LEFT JOIN ORG.Department dpt on dpt.Id = e.DepartmentId
                                        LEFT JOIN HKP.Designation ds on ds.Id = e.DesignationSystemID
                                        left join (select sum(LeaveDuration) duration,LvTrnsSystemID from LeaveTransactionDetails group by LvTrnsSystemID) de on de.LvTrnsSystemID = lt.SystemID
                                    WHERE  LT.EmpSystemID=e.SystemId and lt.PlantID = '" + plantId + @"' and de.duration > 1
                                    and LT.SystemID in(--33
                                    select d.LvTrnsSystemID
                                    from LeaveTransactionDetails d
                                    left join AttdnProcessData a on a.WorkDate=d.WorkDate and a.EmpSystemID=e.SystemId and a.LeaveDuration = 1
                                    inner join DayType dt on dt.DayType = a.DayStatus --and dt.category in ('Present','Late')
                                    
                                    where a.EmpSystemID=e.SystemId and 
                                    (a.InTime is not null or a.PunchInTime is not null or a.ManualInTime is not null
                                    or a.OutTime is not null or a.PunchOutTime is not null or a.ManualOutTime is not null
                                    )
                                        )--33
                                    AND ((LT.FromDate BETWEEN '" + fromDate + @"' AND '" + toDate + @"'
                                    OR LT.ToDate BETWEEN '" + fromDate + @"' AND '" + toDate + "') OR L.LeaveType ='Maternity') ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }

        }//End Function

    }
}
