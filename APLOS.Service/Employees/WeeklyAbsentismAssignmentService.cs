#region Using
using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.Payrolls;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
#endregion Using

namespace Library.Service.Setups
{
    public class WeeklyAbsentismAssignmentService : Service<WeeklyAbsentismAssignment>, IWeeklyAbsentismAssignmentService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<WeeklyAbsentismAssignment> _weeklyAbsentismAssignmentRepository;

        public WeeklyAbsentismAssignmentService(
            IRepositoryAsync<WeeklyAbsentismAssignment> weeklyAbsentismAssignmentRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork,
            ISqlRepository sqlRepository) : base(weeklyAbsentismAssignmentRepository, unitOfWork, pkGeneratorService)
        {
            _weeklyAbsentismAssignmentRepository = weeklyAbsentismAssignmentRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK(string Id)
        {
            return GetAutoNumber(nameof(WeeklyAbsentismAssignment), PKGeneratorEnum.Yearly, Id, DateTime.Now);
        }

        public void InsertUpdate(IEnumerable<WeeklyAbsentismAssignment> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(WeeklyAbsentismAssignment), PKGeneratorEnum.Auto, null, DateTime.Now);
                if (entities == null)
                {
                    throw new CustomException("No data found to save.");
                }

                foreach (var item in entities)
                {
                    //var data = base.Query(t => t.Id == articleId).Include(t => t.MaterialMasterArticleValues).Select().FirstOrDefault();
                    var data = GetWorkDate(item.EmpSystemID, item.WorkingDate.ToString());
                    if (data.Tables[0].Rows.Count == 0)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.ModelState = ModelState.Added;
                            AuditService.Log(item);
                            InsertGraph(item);
                        }
                        else
                        {
                            item.ModelState = ModelState.Modified;
                            AuditService.Log(item);
                            UpdateGraph(item);
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(RetentionAllowanceMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public GridModel Query(GridParameter parameters, string plantId, string fromDate, string toDate)
        {
            try
            {
                parameters.CmdText = @"Select A.EmpSystemID,convert (int, E.EmployeeCode) EmployeeCode ,B.DayStatus,b.WorkDate from
                                     (
                                     Select EmpSystemID from [dbo].[AttdnProcessData]
                                     Where WorkDate between '" + fromDate + @"' AND '" + toDate + @"' and DayStatus='A'
                                     Group by EmpSystemID
                                     Having COUNT(DayStatus)>=3
                                     ) A
                                     inner JOIN EmployeeInformation E on a.EmpSystemID=e.SystemId
                                     inner JOIN(SELECT * FROM [dbo].[AttdnProcessData]
                                     Where WorkDate between '" + fromDate + @"' AND '" + toDate + @"' ) B ON B.EmpSystemID=A.EmpSystemID WHERE e.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public DataSet GetYearValue(string yearId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"Select YearNo,FromDate from [dbo].[YearlyCalendar] Where Id=" + yearId + ""
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetplantWeekOff(string plantId)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT DefaultWeekOff FROM PlantWiseHRMSSetting WHERE PlantID='" + plantId + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }
       
        public IEnumerable<object> GetOffDayData(string yearId, string month, string plantId)
        {
            try
            {
                var sql = @"SELECT M.OffDayType,D.OffDayDate FROM SCS.OffDayMaster M
                                    LEFT JOIN SCS.OffDayDetail D on D.OffDayMasterId=M.Id
                                    WHERE M.PlantId='" + plantId + @"' AND YearlyCalendarId='" + yearId + @"' AND M.OffDayType='W' AND FORMAT(D.OffDayDate,'MMM')='" + month + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetEmployeesDetailsData(string workDate, string employeeCode)
        {
            try
            {

                var toDate = Convert.ToDateTime(workDate);
                var fromDate = Convert.ToDateTime(toDate).AddDays(-6).ToString("dd-MMM-yyyy");

                var sql = @"Select A.EmpSystemID,REPLACE(CONVERT(VARCHAR(11),A.WorkDate,106),' ','-') WorkDate 
                            ,DayStatus,CONVERT(VARCHAR(15), CAST(A.InTime AS TIME), 100) InTime,CONVERT(VARCHAR(15), CAST(A.OutTime AS TIME), 100) OutTime
                            FROM [dbo].[AttdnProcessData] A
                            LEFT JOIN EmployeeInformation E on E.SystemId=A.EmpSystemID
                            WHERE A.WorkDate BETWEEN '" + fromDate + @"' and '" + workDate + @"' and E.EmployeeCode='" + employeeCode + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }//End Function

        public GridModel GetAssignedList(GridParameter parameters, string plantId, string month, string yearId)
        {
            try
            {
                if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(yearId))
                {
                    var year = GetYearValue(yearId);
                    var calenderYear = Convert.ToDateTime(year.Tables[0].Rows[0]["FromDate"]).ToString("yyyy");
                    var date = "01-" + month + "-" + calenderYear;
                    var mon = Convert.ToDateTime(date).Month.ToString();

                    parameters.searchBy = "EmployeeCode";
                    parameters.sort = "EmployeeCode";
                    parameters.order = "ASC";
                    parameters.CmdText = @"SELECT CONVERT (int, E.EmployeeCode) EmployeeCode,E.EmployeeName,A.EmpSystemID,A.Id,A.CompanyGroupId,A.PlantId,
                                        REPLACE(Convert(varchar(11),A.WorkingDate,106),' ','-') WorkingDate FROM [SCS].[WeeklyAbsentismAssignment] A
                                        LEFT JOIN EmployeeInformation E on E.SystemID=A.EmpSystemID
                                        WHERE A.PlantId='" + plantId + @"' AND  Month(A.WorkingDate)=" + mon + @" AND Year(A.WorkingDate)=" + calenderYear + "";
                    return _sqlRepository.GetGridData(parameters);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetEmployeeData(string yearId, string month, string plantId, string day)
        {
            var fromDate = string.Empty;
            var eDate = string.Empty;
            var dayName = "";
            int count = 0;
            var calenderMonth = "";
            var calenderYear = "";
            var calenderDate = DateTime.Now.ToString("dd-MMM-yyyy");
            var calYear = "";
            List<Weekend> weekendList = new List<Weekend>();
            try
            {
                //by monir 190319
                //DataSet dslocation = null;
                //GetJobLocation("", "", "", out dslocation);
                var currentMonthDate = DateTime.Now.ToString("dd-MMM-yyyy");
                DataSet year = null;
                if (yearId != "null")
                {
                    year = GetYearValue(yearId);
                    calenderYear = Convert.ToDateTime(year.Tables[0].Rows[0]["FromDate"]).ToString("yyyy");

                    var plantWeekOff = GetplantWeekOff(plantId);
                    var pw = plantWeekOff.Tables[0].Rows[0]["DefaultWeekOff"];

                    fromDate = "01-" + month + "-" + calenderYear;
                    if (month=="Dec")
                    {
                        eDate="31-" + month + "-" + calenderYear;
                    }
                    else
                    {
                        eDate = new DateTime(Convert.ToDateTime(fromDate).Year, Convert.ToDateTime(fromDate).Month + 1, 1).AddDays(-1).ToString("dd-MMM-yyyy");
                    }
                    var FirstDate = Convert.ToDateTime(fromDate);

                    dayName = Convert.ToDateTime(fromDate).DayOfWeek.ToString();

                    while (dayName != pw.ToString())
                    {
                        FirstDate = FirstDate.AddDays(1);
                        count++;
                        dayName = Convert.ToDateTime(FirstDate).DayOfWeek.ToString();
                    }

                    int countWeek = 0;
                    var firstWeekend = Convert.ToDateTime(fromDate).AddDays(count);
                    var w1 = firstWeekend.AddDays(-6);
                    countWeek++;
                    var fDate = w1;
                    Weekend weekend = new Weekend();
                    weekend.FromDate = fDate.ToString("dd-MMM-yyyy");
                    weekend.WeekNo = countWeek.ToString();
                    w1 = w1.AddDays(7);
                    weekend.ToDate = w1.AddDays(-1).ToString("dd-MMM-yyyy");
                    weekendList.Add(weekend);

                    while (w1.ToString("MMM") == month)
                    {
                        Weekend newweekend = new Weekend();
                        countWeek++;
                        newweekend.FromDate = w1.ToString("dd-MMM-yyyy");
                        newweekend.WeekNo = countWeek.ToString();
                        w1 = w1.AddDays(7);
                        newweekend.ToDate = w1.AddDays(-1).ToString("dd-MMM-yyyy");

                        if (w1.AddDays(-1).ToString("MMM") == month)
                        {
                            weekendList.Add(newweekend);
                        }
                    }
                    var lastDate = weekendList[weekendList.Count - 1].ToDate;
                    currentMonthDate = Convert.ToDateTime(lastDate).AddMonths(-1).AddDays(1).ToString("dd-MMM-yyyy");
                    calenderMonth = Convert.ToDateTime(lastDate).Month.ToString();
                    calYear = Convert.ToDateTime(lastDate).Year.ToString();

                }

                var week = string.Empty;
                var data = string.Empty;
                count = 0;
                foreach (var item in weekendList)
                {
                    if (string.IsNullOrEmpty(week))
                    {
                        week = @"SELECT A.EmpSystemID,CONVERT (int, E.EmployeeCode) EmployeeCode ,E.EmployeeName,1 WeekNo1,0 WeekNo2,0 WeekNo3,0 WeekNo4,0 WeekNo5,0 W0Status,0 W1Status,0 W2Status,0 W3Status,0 W4Status FROM
                            (
                            SELECT COUNT( EmpSystemID) C,EmpSystemID FROM [dbo].[AttdnProcessData]
                            WHERE WorkDate BETWEEN '" + item.FromDate + @"' AND '" + item.ToDate + @"'
                            AND ( --ee
							   DayStatus='A' OR LTSystemID IN (select Id from dbo.LeaveType Where LeaveType='Leave Without Pay')
							    ) --ee
                            GROUP BY EmpSystemID
                            HAVING COUNT(DayStatus)>" + day + @"
                            ) A
                             INNER JOIN (Select * from EmployeeInformation Where EmployeeStatus='Active'  OR DOS>='" + currentMonthDate + @"') E on a.EmpSystemID=e.SystemId
                             INNER JOIN(SELECT * FROM [dbo].[AttdnProcessData]
                             WHERE WorkDate BETWEEN '" + item.FromDate + @"' AND '" + item.ToDate + @"' ) B
                             ON B.EmpSystemID=A.EmpSystemID WHERE e.PlantId='" + plantId + @"' --AND B.EmpSystemId NOT IN (SELECT EmpSystemId FROM [SCS].[WeeklyAbsentismAssignment] WHERE Month(WorkingDate)=" + calenderMonth + @" AND Year(WorkingDate)=" + calYear + ")" +
                             "";
                        count++;
                    }
                    else if (!string.IsNullOrEmpty(week))
                    {
                        string wn = "";
                        count++;
                        if (count == 2)
                        {
                            wn = ",1 WeekNo2,0 WeekNo3,0 WeekNo4,0 WeekNo5";
                        }
                        else if (count == 3)
                        {
                            wn = ",0 WeekNo2,1 WeekNo3,0 WeekNo4,0 WeekNo5";
                        }
                        else if (count == 4)
                        {
                            wn = ",0 WeekNo2,0 WeekNo3,1 WeekNo4,0 WeekNo5";
                        }
                        else if (count == 5)
                        {
                            wn = ",0 WeekNo2,0 WeekNo3,0 WeekNo4,1 WeekNo5";
                        }
                        week += @" 
                            UNION
                            SELECT A.EmpSystemID,CONVERT (int, E.EmployeeCode) EmployeeCode ,E.EmployeeName,0 WeekNo1 " + wn + @",0 W0Status,0 W1Status,0 W2Status,0 W3Status,0 W4Status FROM
                            (
                            SELECT COUNT( EmpSystemID) C,EmpSystemID FROM [dbo].[AttdnProcessData]
                            WHERE WorkDate BETWEEN '" + item.FromDate + @"' AND '" + item.ToDate + @"' 
                            AND ( --ee
							   DayStatus='A' OR LTSystemID IN (select Id from dbo.LeaveType Where LeaveType='Leave Without Pay')
							    ) --ee
                            GROUP BY EmpSystemID
                            HAVING COUNT(DayStatus)>" + day + @"
                            ) A
                             INNER JOIN (Select * from EmployeeInformation Where EmployeeStatus='Active' OR DOS>='" + currentMonthDate + @"') E on a.EmpSystemID=e.SystemId
                             INNER JOIN(SELECT * FROM [dbo].[AttdnProcessData]
                             WHERE WorkDate BETWEEN '" + item.FromDate + @"' AND '" + item.ToDate + @"' ) B
                              ON B.EmpSystemID=A.EmpSystemID WHERE e.PlantId='" + plantId + @"' --AND B.EmpSystemId NOT IN (SELECT EmpSystemId FROM [SCS].[WeeklyAbsentismAssignment] WHERE Month(WorkingDate)=" + calenderMonth + @" AND Year(WorkingDate)=" + calYear + ")" +
                              "";
                    }
                }

                return _sqlRepository.GetDataCollection(week);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetAssignedEmployeeList(string plantId, string month, string yearId)
        {
            try
            {
                if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(yearId) && month != "null" && yearId != "null")
                {
                    var year = GetYearValue(yearId);
                    var calenderYear = Convert.ToDateTime(year.Tables[0].Rows[0]["FromDate"]).ToString("yyyy");
                    var date = "01-" + month + "-" + calenderYear;
                    var mon = Convert.ToDateTime(date).Month.ToString();

                    var sql = @"SELECT CONVERT (int, E.EmployeeCode) EmployeeCode,E.EmployeeName,A.EmpSystemID,A.Id,A.CompanyGroupId,A.PlantId,
                                        REPLACE(Convert(varchar(11),A.WorkingDate,106),' ','-') WorkingDate FROM [SCS].[WeeklyAbsentismAssignment] A
                                        LEFT JOIN EmployeeInformation E on E.SystemID=A.EmpSystemID
                                        WHERE A.PlantId='" + plantId + @"' AND  Month(A.WorkingDate)=" + mon + @" AND Year(A.WorkingDate)=" + calenderYear + @" ORDER BY EmployeeCode";
                    return _sqlRepository.GetDataCollection(sql);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void DeleteMaster(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new CustomException("Id is not found...");

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(id);
                if (data != null)
                {
                    base.Delete(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public DataSet GetWorkDate(string empId, string workDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter

            {
                ExportType = "DATASET",
                CmdText = @"SELECT Id from [SCS].[WeeklyAbsentismAssignment] Where EmpSystemId='" + @empId + "' AND WorkingDate='" + workDate + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public void GetJobLocation(string sGroupID, string sPlantID, string strSystemID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM JobLocation ORDER BY JobLocation";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
    }

    public class Weekend
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string WeekNo { get; set; }
    }
}