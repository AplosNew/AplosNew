#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class ApprovedEmployeeService : Service<EmployeeInformation>, IApprovedEmployeeService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IEmpAcademicQualificationInformationService _empAcademicQualificationInformationService;
        private readonly IEmpExperienceInformationService _empExperienceInformationService;
        private readonly IEmpTrainingInformationService _empTrainingInformationService;
        private readonly IEmployeeDocumentService _employeeDocumentService;

        public ApprovedEmployeeService(
            IRepositoryAsync<EmployeeInformation> EmployeeApprovalServiceRepository
            , IUnitOfWork unitOfWork
            , IEmpExperienceInformationService empExperienceInformationService
            , IEmpAcademicQualificationInformationService empAcademicQualificationInformationService
            , IEmpTrainingInformationService empTrainingInformationService
            , ISqlRepository sqlRepository
            , IEmployeeDocumentService employeeDocumentService) :
            base(EmployeeApprovalServiceRepository, unitOfWork)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _empAcademicQualificationInformationService = empAcademicQualificationInformationService;
            _empExperienceInformationService = empExperienceInformationService;
            _empTrainingInformationService = empTrainingInformationService;
            _employeeDocumentService = employeeDocumentService;
        }

        #endregion Constructor

        public void Insert(EmployeeInformation employeeInformation,
                          IEnumerable<EmpAcademicQualificationInformation> empAcademicQualificationInformations,
                          IEnumerable<EmpExperienceInformation> empExperienceInformations,
                          IEnumerable<EmpTrainingInformation> empTrainingInformations,
                          IEnumerable<EmployeeDocument> employeeDocuments)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                employeeInformation.ApprovedFromIP = identity.IPAddress;
                employeeInformation.IsApproved = true;
                employeeInformation.ApprovedBy = identity.FullName;
                employeeInformation.ApprovedDateTime = DateTime.Now;
                UpdateGraph(employeeInformation);

                if (empAcademicQualificationInformations != null)
                {
                    foreach (EmpAcademicQualificationInformation empQualification in empAcademicQualificationInformations)
                    {
                        empQualification.ApprovedFromIP = identity.IPAddress;
                        empQualification.ApprovedDateTime = DateTime.Now;
                        empQualification.ApprovedBy = identity.FullName;
                        _empAcademicQualificationInformationService.UpdateGraph(empQualification);
                    }
                }
                if (empExperienceInformations != null)
                {
                    foreach (EmpExperienceInformation empExperience in empExperienceInformations)
                    {
                        empExperience.ApprovedFromIP = identity.IPAddress;
                        empExperience.ApprovedDateTime = DateTime.Now;
                        empExperience.ApprovedBy = identity.FullName;
                        _empExperienceInformationService.UpdateGraph(empExperience);
                    }
                }
                if (empTrainingInformations != null)
                {
                    foreach (EmpTrainingInformation empTraining in empTrainingInformations)
                    {
                        empTraining.ApprovedFromIP = identity.IPAddress;
                        empTraining.ApprovedDateTime = DateTime.Now;
                        empTraining.ApprovedBy = identity.FullName;
                        _empTrainingInformationService.UpdateGraph(empTraining);
                    }
                }
                if (employeeDocuments != null)
                {
                    foreach (EmployeeDocument employeeDocument in employeeDocuments)
                    {
                        employeeDocument.ApprovedFromIP = identity.IPAddress;
                        employeeDocument.ApprovedDateTime = DateTime.Now;
                        employeeDocument.ApprovedBy = identity.FullName;
                        _employeeDocumentService.UpdateGraph(employeeDocument);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel GetAllEmployee(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT EMP.*,E.UserName EntityName,D.UserName Designation,PR.UserName PositionName
									 ,DEG.UserName GivenDesignation,DEPT.UserName Department
									 FROM EmployeeInformation EMP
									 LEFT OUTER JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
									 LEFT OUTER JOIN ORG.Position PR ON PMB.PositionId=PR.Id
									 LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id
									 LEFT OUTER JOIN HKP.Designation D ON PR.DesignationId=D.Id
									 LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
									 LEFT OUTER JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
									 WHERE EMP.GroupID='" + identity.CompanyGroupId + @"' AND EMP.CompanyId='" + identity.CompanyId + @"' AND EMP.IsApproved=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetEmployeeData(string eId)
        {
            try
            {
                var sql = @"Select EMP.*, COALESCE(PO.UserName,'') PresentPoliceStation, COALESCE( ParmP.UserName, '')ParmanentPoliceStation,
                           COALESCE(D.UserName,'') PresDistrict, COALESCE(ParmD.UserName,'') ParmDistrict,COALESCE(C.UserName,'') PresCountry,COALESCE(ParmC.UserName,'') ParmCountry,
                           COALESCE(ParmP.UserName,'') ParmPostOffice,COALESCE(PerP.UserName,'') PresPostOffice,COALESCE(PerCT.UserName,'') PresCity,COALESCE(ParCT.UserName,'') ParmCity,
                           COALESCE(ParmS.UserName,'') ParmState,COALESCE(PresS.UserName,'') PresState
                            FROM EmployeeInformation EMP
                             LEFT JOIN scs.PoliceStation PO ON EMP.PresThanaID=PO.Id
                             LEFT JOIN scs.PoliceStation ParmPO ON EMP.ParmThanaID=ParmPO.Id
                             LEFT JOIN SCS.District D ON EMP.PresDistrictID = D.Id
                             LEFT JOIN SCS.District ParmD ON EMP.ParmDistrictID = ParmD.Id
                             LEFT JOIN SCS.Country C ON EMP.PresCountryID = C.ID
                             LEFT JOIN SCS.Country ParmC ON EMP.ParmCountryID = ParmC.ID
                             LEFT JOIN SCS.PostOffice ParmP ON EMP.ParmPostOfficeID = ParmP.ID
                             LEFT JOIN SCS.PostOffice PerP ON EMP.PresPostOfficeID = PerP.ID
                             LEFT JOIN SCS.City PerCT ON EMP.PresCityID = PerCT.ID
                             LEFT JOIN SCS.City ParCT ON EMP.ParmCityID = ParCT.ID
                             LEFT JOIN SCS.[State] ParmS ON EMP.ParmStateId = ParmS.Id
                             LEFT JOIN SCS.[State] PresS ON EMP.PresStateId = PresS.Id
                            WHERE EMP.SystemId='" + eId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetQualificationData(string empSystemID)
        {
            try
            {
                var sql = @"SELECT * FROM EmpAcademicQualificationInformation where EmpSystemID = '" + empSystemID + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetExperienceData(string empSystemID)
        {
            try
            {
                var sql = @"SELECT * FROM EmpExperienceInformation where EmpSystemID = '" + empSystemID + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetTrainingData(string empSystemID)
        {
            try
            {
                var sql = @"SELECT * FROM EmpTrainingInformation where EmpSystemID = '" + empSystemID + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetEmployeeDocumentData(string eId)
        {
            try
            {
                var sql = @"SELECT ED.*,CD.UserName ComplianceDocument FROM [dbo].[EmployeeDocument] ED
							LEFT OUTER JOIN HKP.ComplianceDocument CD ON ED.ComplianceDocumentId=CD.Id WHERE ED.EmpSystemID='" + eId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
    }
}