using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Attendances;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Core;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Payrolls;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.HumanResources
{
    public class RestService : Service<AttendanceRest>, IRestService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRestDetailsService _restDetailsService;
        private readonly IPaidHoursEmployeeAssignService _paidHoursEmployeeAssignService;
        private readonly IRepositoryAsync<AttdnProcessData> _attdnProcessDataRepository;
        private readonly ILeaveTransectionService _leaveTransactionService;
        private readonly IEmployeeInformationService _employeeInformationService;

        public RestService(
            IRepositoryAsync<AttendanceRest> restRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork
            , IRestDetailsService restDetailsService
             , IRepositoryAsync<AttdnProcessData> attdnProcessDataRepository
            , IPaidHoursEmployeeAssignService paidHoursEmployeeAssignService
            , ILeaveTransectionService leaveTransactionService
            , IEmployeeInformationService employeeInformationService) : base(restRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _restDetailsService = restDetailsService;
            _attdnProcessDataRepository = attdnProcessDataRepository;
            _paidHoursEmployeeAssignService = paidHoursEmployeeAssignService;
            _leaveTransactionService = leaveTransactionService;
            _employeeInformationService = employeeInformationService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(AttendanceRest), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void Insert(AttendanceRest entity, string plantId, IEnumerable<AttendanceRestDetail> restDetails)
        {
            var empSystemIds = "";
            var flag = false;
            List<AttendanceRestDetail> restDetailsDb_list = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (restDetails == null)
                {
                    throw new CustomException("No employee selected for rest.");
                }

                foreach (var item in restDetails)
                {
                    if (string.IsNullOrEmpty(empSystemIds))
                        empSystemIds = item.EmpSystemId;
                    else
                        empSystemIds += "," + item.EmpSystemId;
                }

                AttendanceProcessAplos ob = new AttendanceProcessAplos();
                ob.LockValidation(plantId, Convert.ToDateTime(entity.AttendanceRestDate).ToString("dd-MMM-yyyy"), Convert.ToDateTime(entity.AttendanceRestDate).ToString("dd-MMM-yyyy"), empSystemIds);

                var getData = base.Query(t => t.AttendanceRestDate == entity.AttendanceRestDate).Select().FirstOrDefault();


                if (getData == null)
                {
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = "R-" + GetPK();
                        entity.ModelState = ModelState.Added;
                        AuditService.AddedLog(entity);
                    }
                    else
                    {
                        entity.ModelState = ModelState.Modified;
                        AuditService.UpdatedLog(entity);
                    }
                    InsertOrUpdateGraph(entity);
                }
                else
                {
                    //InsertOrUpdateGraph(getData);

                }
                if (getData == null)//no old data found
                {
                    _restDetailsService.InsertOrUpdateGraph(restDetails, plantId, entity.Id, out restDetailsDb_list);
                }
                else
                {
                    _restDetailsService.InsertOrUpdateGraph(restDetails, plantId, getData.Id, out restDetailsDb_list);
                }
                foreach (var item in restDetailsDb_list)
                {
                    string empid = restDetails.Where(r => r.EmpSystemId == item.EmpSystemId).Select(r => r.EmpSystemId).FirstOrDefault();
                    
                    if (empid!=null)
                    {
                        var employee = _employeeInformationService.Find(item.EmpSystemId);
                        var getLeaveData = _leaveTransactionService.Query(t => t.EmpSystemID == item.EmpSystemId && t.FromDate == entity.AttendanceRestDate).Select().FirstOrDefault();
                        if (getLeaveData != null)
                        {
                            throw new CustomException("This employee  " + employee.EmployeeCode + " is in leave.");
                        }

                        var getOdData = GetEmpODData(item.EmpSystemId, Convert.ToDateTime(entity.AttendanceRestDate).ToString("dd-MMM-yyyy"));
                        if (getOdData.Tables[0].Rows.Count > 0)
                        {
                            throw new CustomException("This employee  " + employee.EmployeeCode + " is on duty.");
                        }

                        var employeeWeekend = GetEmpWeekendData(item.EmpSystemId, Convert.ToDateTime(entity.AttendanceRestDate).ToString("dd-MMM-yyyy"));

                        if (employeeWeekend.Tables[0].Rows.Count > 0)
                        {
                            throw new CustomException("Rest can not be apply no weekend.");
                        }

                        var employeeHoliday = GetEmpHoliDayData(identity.CompanyGroupId, identity.PlantId, Convert.ToDateTime(entity.AttendanceRestDate).ToString("dd-MMM-yyyy"));

                        if (employeeWeekend.Tables[0].Rows.Count > 0)
                        {
                            throw new CustomException("Rest can not be apply no holiday.");
                        }


                        _restDetailsService.InsertOrUpdateGraph(item);
                        empSystemIds = item.EmpSystemId;
                        var attndata = _attdnProcessDataRepository.Query(t => t.EmpSystemID == item.EmpSystemId && t.WorkDate == entity.AttendanceRestDate).Select().FirstOrDefault();
                        if (attndata != null)
                        {
                            // throw new CustomException("This employee : " + item.EmpSystemId + " is not in the Attendance Process Data.");

                            var ot = _paidHoursEmployeeAssignService.Query(t => t.EmployeeId == item.EmpSystemId).Select(t => t.PaidHours).FirstOrDefault();
                            attndata.AttendanceRestDetailId = item.Id;
                            attndata.OTHr = ot * (-60);
                            attndata.DayStatus = "RST";
                            _attdnProcessDataRepository.Update(attndata);
                        }
                        else
                        {

                            //it will be updated at the time of attendance process.
                        } 
                    }
                }
                flag = true;
                _unitOfWork.BeginTransaction();
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();


                if (Convert.ToDateTime(entity.AttendanceRestDate) < DateTime.Now)
                {

                    DateTime FromDate = Convert.ToDateTime(entity.AttendanceRestDate.ToString());
                    DateTime ToDate = Convert.ToDateTime(entity.AttendanceRestDate.ToString());
                    while (FromDate <= ToDate)
                    {

                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                        ob.SaveTotal(identity.PlantId, Convert.ToDateTime(entity.AttendanceRestDate).ToString("dd-MMM-yyyy"), empSystemIds, false);
                        FromDate = FromDate.AddDays(1);
                    }
                }


            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel GetAllEmployee(GridParameter parameters, string companyGroupId, string companyId, string plantId, string sectionId, string subSectionId, string departmentId, bool isOTEntitle,string AttendanceRestDate)
        {
            try
            {
                string wc = string.Empty;
                if (sectionId == "null" || sectionId == "undefined")
                {
                    wc = "";
                }
                else
                {
                    wc = "AND  EMP.SectionId='" + sectionId + @"' ";
                }
                if (subSectionId == "null" || subSectionId == "undefined")
                {
                    wc += "";
                }
                else
                {
                    wc += "AND  EMP.SubSectionId='" + subSectionId + @"' ";
                }
                if (departmentId == "null" || departmentId == "undefined")
                {
                    wc += "";
                }
                else
                {
                    wc += "AND  EMP.DepartmentId='" + departmentId + @"' ";
                }
                var ot = "";
                if (isOTEntitle)
                {
                    ot = " AND OT.IsOTEntitle = '" + isOTEntitle + @"'";
                }

                parameters.CmdText = @"SELECT * FROM (SELECT 0 Active,EMP.SystemId EmpSystemId,EMP.EmployeeId,EMP.EmployeeName,CONVERT (int, EMP.EmployeeCode) EmployeeCode ,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                     PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,EC.UserName EmployeeCategory,Se.UserName Section,SuS.UserName SubSection,P.UserName Plant
									 FROM EmployeeInformation EMP
									 LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
									 LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
									 LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
									 LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
									 LEFT JOIN ORG.Department DEPT ON EMP.DepartmentId=DEPT.Id
									 LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                     LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EMP.GivenDesignationId
                                     LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                                     LEFT JOIN EmployeeOTEntitle OT ON OT.EmpSystemID=EMP.SystemId
                                     LEFT JOIN [ORG].Section AS Se ON Se.ID = EMP.SectionID
                                     LEFT JOIN [ORG].SubSection AS SuS ON SuS.ID = EMP.SubSectionID
                                     LEFT JOIN [ORG].Plant AS P ON P.Id = EMP.PlantId
									 WHERE (emp.DOJ <= '"+AttendanceRestDate +@"') and (DOS IS NULL OR EMP.DOS>='"+AttendanceRestDate+@"') and 
                        EMP.PlantId='" + plantId + @"' AND EMP.GroupID='" + companyGroupId + @"' AND EMP.CompanyId='" + companyId + @"' " + wc + " " + ot + ") A";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public GridModel GetAllEmployeeForEx(GridParameter parameters, string companyGroupId, string companyId, string plantId, string sectionId, string subSectionId, string departmentId, bool isOTEntitle, string AttendanceRestDate)
        {
            try
            {
                string wc = string.Empty;
                if (sectionId == "null" || sectionId == "undefined")
                {
                    wc = "";
                }
                else
                {
                    wc = "AND  EMP.SectionId='" + sectionId + @"' ";
                }
                if (subSectionId == "null" || subSectionId == "undefined")
                {
                    wc += "";
                }
                else
                {
                    wc += "AND  EMP.SubSectionId='" + subSectionId + @"' ";
                }
                if (departmentId == "null" || departmentId == "undefined")
                {
                    wc += "";
                }
                else
                {
                    wc += "AND  EMP.DepartmentId='" + departmentId + @"' ";
                }
                var ot = "";
                if (isOTEntitle)
                {
                    ot = " AND OT.IsOTEntitle = '" + isOTEntitle + @"'";
                }

                parameters.CmdText = @"SELECT * FROM (SELECT 0 Active,EMP.SystemId EmpSystemId,EMP.EmployeeId,EMP.EmployeeName,EMP.EmployeeCode,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                     PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,EC.UserName EmployeeCategory,Se.UserName Section,SuS.UserName SubSection,P.UserName Plant
,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,'' EffectiveDate
									 FROM EmployeeInformation EMP
									 LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
									 LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
									 LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
									 LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
									 LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
									 LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                     LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EMP.GivenDesignationId
                                     LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                                     LEFT JOIN EmployeeOTEntitle OT ON OT.EmpSystemID=EMP.SystemId
                                     LEFT JOIN [ORG].Section AS Se ON Se.ID = PR.SectionID
                                     LEFT JOIN [ORG].SubSection AS SuS ON SuS.ID = PR.SubSectionID
                                     LEFT JOIN [ORG].Plant AS P ON P.Id = EMP.PlantId
									 WHERE EMP.PlantId='" + plantId + @"' AND  EMP.Employeestatus='Active' AND EMP.SystemId NOT IN (Select EmpSystemId from ExceptionEmployee) AND EMP.GroupID='" + companyGroupId + @"' AND EMP.CompanyId='" + companyId + @"' " + wc + " " + ot + ") A ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public GridModel Query(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.order = "DESC";
                parameters.CmdText = @"SELECT AR.Id
                                    	,REPLACE(CONVERT(VARCHAR(11), AR.AttendanceRestDate, 106), ' ', '-') AttendanceRestDate
                                    	,AR.Remarks
                                    	,AR.AddedBy
                                    	,REPLACE(CONVERT(VARCHAR(11), AR.AddedDate, 106), ' ', '-') AddedDate
                                    	,AR.AddedFromIP
                                    	,AR.UpdatedBy
                                    	,REPLACE(CONVERT(VARCHAR(11), AR.UpdatedDate, 106), ' ', '-') UpdatedDate
                                    	,AR.UpdatedFromIP
                                        ,AR.AttendanceRestDate RestDate
                                    	,Count(ARD.Id) Total
                                    FROM AttendanceRest AR
                                    LEFT JOIN AttendanceRestDetail ARD ON ARD.AttendanceRestId = AR.Id
                                    LEFT JOIN EmployeeInformation E ON E.SystemId=ARD.EmpSystemId
									Where E.PlantId='" + plantId + @"'
                                    GROUP BY AR.Id
                                    	,AR.AttendanceRestDate
                                    	,AR.Remarks
                                    	,AR.AddedBy
                                    	,AR.AddedDate
                                    	,AR.UpdatedBy
                                    	,AR.UpdatedDate
                                    	,AR.AddedFromIP
                                    	,AR.UpdatedFromIP";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetRestDetailsData(string restId, string plantId)
        {
            try
            {
                var sql = @"SELECT  EMP.EmployeeName,CONVERT (int, EMP.EmployeeCode) EmployeeCode,EMP.BudgetCode, DEG.UserName GivenDesignation,EC.UserName EmployeeCategory,D.UserName Designation,
                           Se.UserName Section,SuS.UserName SubSection,P.UserName Plant,DEPT.UserName Department,RD.* 
                            FROM AttendanceRestDetail AS RD
                            LEFT JOIN EmployeeInformation AS EMP ON  EMP.SystemId=RD.EmpSystemId 
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
							LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
							LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
							LEFT JOIN ORG.Department DEPT ON EMP.DepartmentId=DEPT.Id
                            Left JOIN MST.DesignationMaster DM ON DM.DesignationId=EMP.GivenDesignationId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                            LEFT JOIN [ORG].Section AS Se ON Se.ID = EMP.SectionID
                            LEFT JOIN [ORG].SubSection AS SuS ON SuS.ID = EMP.SubSectionID
                            LEFT JOIN [ORG].Plant AS P ON P.Id = EMP.PlantId
                            LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId Where AttendanceRestId='" + restId + @"' AND RD.PlantId='" + plantId + "' ORDER BY EmployeeCode";
                return _sqlRepository.GetDataCollection(sql, null);
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
                throw new CustomException("Rest id is not found...");

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(id);
                if (data != null)
                {

                    _restDetailsService.ExecuteSqlCommand("DELETE FROM AttendanceRestDetail Where AttendanceRestId='" + id + "'");
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

        public DataSet GetEmpLeaveData(string empSystemId, string fromDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT LD.SystemID FROM LeaveTransaction LT
										LEFT JOIN LeaveTransactionDetails LD ON LD.LvTrnsSystemID=LT.SystemID
										WHERE LT.EmpSystemID='" + empSystemId + @"' AND  LD.WorkDate='" + fromDate + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetEmpODData(string empSystemId, string fromDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT ODD.Id FROM EmployeeOnDuty OD
							LEFT JOIN EmployeeOnDutyDetails ODD ON ODD.OnDutyId=OD.Id
							WHERE OD.EmpSystemId='" + empSystemId + @"' AND  ODD.WorkDate='" + fromDate + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetEmpWeekendData(string empSystemId, string fromDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT DayType FROM EmpDateWiseShiftAssign WHERE EmpSystemID='" + empSystemId + @"' and DayType='W' and WorkDate ='" + fromDate + @"' "
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }

        public DataSet GetEmpHoliDayData(string sGroupID, string sPlantID, string fromDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT OFM.CldDescription, OFM.FromDate, OFM.ToDate, OFM.OffDayType, OFM.TotalDay, OFD.DayName, OFM.PlantID  
	                            FROM scs.OffDayMaster OFM
			                            INNER JOIN scs.OffDayDetail OFD ON OFM.Id = OFD.OffDayMasterId 
                                                                    AND OFD.OffDayDate ='" + fromDate + @"'
                                WHERE OFM.CompanyGroupId = '" + sGroupID + @"' AND OFM.PlantID = '" + sPlantID + @"'
									  AND OFM.OffDayType = 'H'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }
    }
}