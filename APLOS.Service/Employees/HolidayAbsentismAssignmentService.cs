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
    public class HolidayAbsentismAssignmentService : Service<HolidayAbsentismAssignment>, IHolidayAbsentismAssignmentService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<HolidayAbsentismAssignment> _holidayAbsentismAssignmentRepository;

        public HolidayAbsentismAssignmentService(
            IRepositoryAsync<HolidayAbsentismAssignment> holidayAbsentismAssignmentRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork,
            ISqlRepository sqlRepository) : base(holidayAbsentismAssignmentRepository, unitOfWork, pkGeneratorService)
        {
            _holidayAbsentismAssignmentRepository = holidayAbsentismAssignmentRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK(string Id)
        {
            return GetAutoNumber(nameof(HolidayAbsentismAssignment), PKGeneratorEnum.Yearly, Id, DateTime.Now);
        }

        public void InsertUpdate(IEnumerable<HolidayAbsentismAssignment> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber(nameof(HolidayAbsentismAssignment), PKGeneratorEnum.Auto, null, DateTime.Now);
                if (entities == null)
                {
                    throw new CustomException("No data found to save.");
                }

                foreach (var item in entities)
                {
                    //var data = base.Query(t => t.Id == articleId).Include(t => t.MaterialMasterArticleValues).Select().FirstOrDefault();
                    var data = GetWorkDate(item.EmpSystemID, item.WorkDate.ToString());
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

        public IEnumerable<ComboModel> GetHolidayCbo(string yearId, string month, string plantId)
        {
            var sql = @"SELECT M.Id,REPLACE(CONVERT(VARCHAR(11),D.OffDayDate,106),' ','-') OffDayDate FROM SCS.OffDayMaster M
                       LEFT JOIN SCS.OffDayDetail D on D.OffDayMasterId = M.Id
                       WHERE M.PlantId = '" + plantId + @"' 
                       AND YearlyCalendarId = '" + yearId + @"' AND M.OffDayType = 'H' AND FORMAT(D.OffDayDate,'MMM')= '" + month + @"' AND M.IsMandatory=0";
            return _sqlRepository.GetCombo(sql, "Id", "OffDayDate");
        }

        public IEnumerable<object> GetHolidayData(string yearId, string month, string plantId)
        {
            try
            {
                var sql = @"SELECT M.OffDayType,D.OffDayDate FROM SCS.OffDayMaster M
                          LEFT JOIN SCS.OffDayDetail D on D.OffDayMasterId = M.Id
                          WHERE M.PlantId = '" + plantId + @"' AND YearlyCalendarId = '" + yearId + @"' AND M.OffDayType = 'H' AND FORMAT(D.OffDayDate,'MMM')= '" + month + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public DataSet GetDSHolidayData(string yearId, string month, string plantId)
        {
            var fromDate = string.Empty;
            var eDate = string.Empty;
            var year = GetYearValue(yearId);
            var calenderYear = Convert.ToDateTime(year.Tables[0].Rows[0]["FromDate"]).ToString("yyyy");
            fromDate = "01-" + month + "-" + calenderYear;
            eDate = new DateTime(Convert.ToDateTime(fromDate).Year, Convert.ToDateTime(fromDate).Month + 1, 1).AddDays(-1).ToString("dd-MMM-yyyy");
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT DISTINCT REPLACE(CONVERT(VARCHAR(11),WorkDate,106),' ','-') WorkDate 
                                       FROM AttdnProcessData where DayStatus='H' 
                                       AND WorkDate between '" + fromDate + @"' and '" + eDate + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public IEnumerable<object> GetEmployeesDetailsData(string workDate, string employeeCode)
        {
            try
            {

                var date = Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                var tDate = Convert.ToDateTime(date).AddDays(-1).ToString("dd-MMM-yyyy");
                var fDate = Convert.ToDateTime(date).AddDays(-3).ToString("dd-MMM-yyyy");
                var sql = @"SELECT A.EmpSystemID
                            	,REPLACE(CONVERT(VARCHAR(11), A.WorkDate, 106), ' ', '-') WorkDate
                            	,DayStatus DayStatusOrigianl
                            	,CONVERT(VARCHAR(15), CAST(A.InTime AS TIME), 100) InTime
                            	,CONVERT(VARCHAR(15), CAST(A.OutTime AS TIME), 100) OutTime
                            	,W.EmpSystemID
                            	,DayStatus = CASE WHEN w.EmpSystemID IS NULL THEN DayStatus ELSE DayStatus + ' (A)' END
                            FROM [dbo].[AttdnProcessData] A
                            LEFT JOIN EmployeeInformation E ON E.SystemId = A.EmpSystemID
                            LEFT JOIN (
                            	SELECT EmpSystemID ,WorkingDate
                            	FROM [SCS].[WeeklyAbsentismAssignment] WA
                            	WHERE WorkingDate BETWEEN '" + fDate + @"' and '" + tDate + @"'
                            	) W ON W.EmpSystemID = A.EmpSystemID
                            	AND a.WorkDate = w.WorkingDate
                            WHERE A.WorkDate BETWEEN '" + fDate + @"' and '" + tDate + @"' AND E.EmployeeCode = '" + employeeCode + "'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }//End Function

        public GridModel GetAssignedList(GridParameter parameters, string plantId,string workDate)
        {
            try
            {
                if (!string.IsNullOrEmpty(workDate))
                {
                    parameters.searchBy = "EmployeeCode";
                    parameters.sort = "EmployeeCode";
                    parameters.order = "ASC";
                    parameters.CmdText = @"SELECT CONVERT (int, E.EmployeeCode) EmployeeCode,E.EmployeeName,A.EmpSystemID,A.Id,A.CompanyGroupId,A.PlantId,
                                        REPLACE(Convert(varchar(11),A.WorkDate,106),' ','-') WorkDate FROM [TRN].[HolidayAbsentismAssignment] A
                                        LEFT JOIN EmployeeInformation E on E.SystemID=A.EmpSystemID
                                        WHERE A.PlantId='" + plantId + @"' AND  A.WorkDate='" + workDate + @"'";
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

        public IEnumerable<object> GetEmployeeData(string workDate, string day, string plantId)
        {
            var fromDate = string.Empty;
            var eDate = string.Empty;
            var tDate = string.Empty;
            var fDate = string.Empty;
            var calenderDate = DateTime.Now.ToString("dd-MMM-yyyy");
            //List<Holiday> holidayList = new List<Holiday>();
            try
            {

                var date = Convert.ToDateTime(workDate).ToString("dd-MMM-yyyy");
                tDate = Convert.ToDateTime(date).AddDays(-1).ToString("dd-MMM-yyyy");
                fDate = Convert.ToDateTime(date).AddDays(-3).ToString("dd-MMM-yyyy");
                var mon = Convert.ToDateTime(date).Month.ToString();
                var year = Convert.ToDateTime(date).Year.ToString();


                //var sql = @"SELECT A.EmpSystemID,A.EmployeeCode ,A.EmployeeName, COUNT(A.WorkDate) WorkDate FROM  
                //      (SELECT A.EmpSystemID,CONVERT (int, E.EmployeeCode) EmployeeCode ,E.EmployeeName,A.WorkDate FROM
                //       (
                //       SELECT COUNT( EmpSystemID) C,EmpSystemID,WorkDate FROM [dbo].[AttdnProcessData]
                //       WHERE WorkDate BETWEEN '" + fDate + @"' AND '" + tDate + @"' 
                //       AND   DayStatus='A' AND PlantID='" + plantId + @"'
                //       GROUP BY EmpSystemID,WorkDate
                //       ) A
                //        INNER JOIN (SELECT * FROM EmployeeInformation WHERE EmployeeStatus='Active' OR DOS>='" + fDate + @"'  AND PlantID='" + plantId + @"') E on a.EmpSystemID=e.SystemId
                //         UNION 
                //      SELECT  A.EmpSystemID,E.EmployeeCode,e.EmployeeName,A.WorkDate
                //      FROM AttdnProcessData A
                //      INNER JOIN (Select * from [SCS].[WeeklyAbsentismAssignment] Where PlantId='" + plantId + @"' AND  Month(WorkingDate)=" + mon + @" AND Year(WorkingDate)=" + year + @") W ON W.EmpSystemID=A.EmpSystemID AND W.WorkingDate=A.WorkDate
                //      INNER JOIN EmployeeInformation E ON E.SystemID=W.EmpSystemID
                //      WHERE A.WorkDate between '" + fDate + @"' AND '" + tDate + @"'  AND A.DayStatus='W') A  
                //      GROUP BY A.EmpSystemID,A.EmployeeCode ,A.EmployeeName
                //      HAVING COUNT(A.WorkDate)>" + day + @"
                //      ";
                var sql = @"SELECT A.EmpSystemID,A.EmployeeCode ,A.EmployeeName, COUNT(A.WorkDate) WorkDate FROM
                        (SELECT A.EmpSystemID,CONVERT (int, E.EmployeeCode) EmployeeCode ,E.EmployeeName,A.WorkDate FROM
                        (
                        SELECT COUNT( EmpSystemID) C,EmpSystemID,WorkDate FROM [dbo].[AttdnProcessData]
                        WHERE WorkDate BETWEEN '" + fDate + @"' AND '" + tDate + @"' 
                        AND DayStatus='A' AND PlantID='"+plantId+@"'
                        GROUP BY EmpSystemID,WorkDate
                        ) A
                        INNER JOIN (SELECT * FROM EmployeeInformation WHERE EmployeeStatus='Active' OR DOS>='"+fDate+@"' AND PlantID='"+plantId+@"') E on a.EmpSystemID=e.SystemId
                        UNION
                        SELECT A.EmpSystemID,E.EmployeeCode,e.EmployeeName,A.WorkDate
                        FROM AttdnProcessData A
                        INNER JOIN (Select * from [SCS].[WeeklyAbsentismAssignment] Where PlantId='"+plantId+@"' AND Month(WorkingDate)="+mon+@"
                        AND Year(WorkingDate)="+year+@") W ON W.EmpSystemID=A.EmpSystemID AND W.WorkingDate=A.WorkDate
                        INNER JOIN EmployeeInformation E ON E.SystemID=W.EmpSystemID
                        WHERE A.WorkDate BETWEEN '" + fDate + @"' AND '" + tDate + @"'  AND A.DayStatus='W'
                        UNION
                        SELECT A.EmpSystemID,E.EmployeeCode,e.EmployeeName,A.WorkDate
                        FROM AttdnProcessData A
                        INNER JOIN (Select * from trn.[HolidayAbsentismAssignment] Where PlantId='"+plantId+@"' AND Month(WorkDate)="+mon+@"
                        AND Year(WorkDate)="+year+@") W ON W.EmpSystemID=A.EmpSystemID AND W.WorkDate=A.WorkDate
                        INNER JOIN EmployeeInformation E ON E.SystemID=W.EmpSystemID
                        WHERE A.WorkDate BETWEEN '" + fDate + @"' AND '" + tDate + @"'  AND A.DayStatus='H'
                        ) A
                        GROUP BY A.EmpSystemID,A.EmployeeCode ,A.EmployeeName
                        HAVING COUNT(A.WorkDate)>"+day+"";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetAssignedEmployeeList(string plantId, string workDate)
        {
            try
            {
                if (!string.IsNullOrEmpty(workDate))
                {
                    var sql = @"SELECT CONVERT(INT, E.EmployeeCode) EmployeeCode
                            	,E.EmployeeName
                            	,A.EmpSystemID
                            	,A.Id
                            	,A.CompanyGroupId
                            	,A.PlantId
                            	,REPLACE(Convert(VARCHAR(11), A.WorkDate, 106), ' ', '-') WorkDate
                            	,Selected = CASE WHEN A.Id IS NULL THEN 0 ELSE 1 END
                            FROM [TRN].[HolidayAbsentismAssignment] A
                            LEFT JOIN EmployeeInformation E ON E.SystemID = A.EmpSystemID
                            WHERE A.PlantId='" + plantId + @"' AND  A.WorkDate='" + workDate + @"' ORDER BY EmployeeCode";
                    
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
                CmdText = @"SELECT Id from [TRN].[HolidayAbsentismAssignment] Where EmpSystemId='" + @empId + "' AND WorkDate='" + workDate + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public void GetJobLocation(string sGroupID, string sPlantID, string strSystemID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT *  FROM JobLocation ORDER BY JobLocation";

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

    public class Holiday
    {
        public string Date { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
}