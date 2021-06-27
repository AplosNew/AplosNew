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
    public class PreRecruitmentDocumentApprovalService : Service<PreRecruitmentEmployee>, IPreRecruitmentDocumentApprovalService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;
        private readonly IPreRecruitmentEmpQualificationService _preRecruitmentEmpQualificationService;
        private readonly IPreRecruitmentEmpExperienceService _preRecruitmentEmpExperienceService;
        private readonly IPreRecruitmentEmpTrainingService _preRecruitmentEmpTrainingService;
        private readonly IPreRecruitmentDocumentService _preRecruitmentDocumentService;

        public PreRecruitmentDocumentApprovalService(
            IRepositoryAsync<PreRecruitmentEmployee> PreRecruitmentDocumentApprovalRepository
            , IUnitOfWork unitOfWork
            , IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            , IPreRecruitmentEmpQualificationService preRecruitmentEmpQualificationService
            , IPreRecruitmentEmpExperienceService preRecruitmentEmpExperienceService
            , IPreRecruitmentEmpTrainingService preRecruitmentEmpTrainingService
            , IPreRecruitmentDocumentService preRecruitmentDocumentService
            , ISqlRepository sqlRepository
            ) :
            base(PreRecruitmentDocumentApprovalRepository, unitOfWork)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
            _preRecruitmentEmpQualificationService = preRecruitmentEmpQualificationService;
            _preRecruitmentEmpExperienceService = preRecruitmentEmpExperienceService;
            _preRecruitmentEmpTrainingService = preRecruitmentEmpTrainingService;
            _preRecruitmentDocumentService = preRecruitmentDocumentService;
        }

        #endregion Constructor

        public void Insert(PreRecruitmentEmployee preRecruitmentEmployee,
            IEnumerable<PreRecruitmentEmpQualification> preRecruitmentEmpQualificationList,
            IEnumerable<PreRecruitmentEmpExperience> preRecruitmentEmpExperienceList,
            IEnumerable<PreRecruitmentEmpTraining> preRecruitmentEmpTrainingList,
            IEnumerable<PreRecruitmentDocument> preRecruitmentDocumentList)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                preRecruitmentEmployee.ApprovedFromIP = identity.IPAddress;
                preRecruitmentEmployee.IsApproved = true;
                preRecruitmentEmployee.ApprovedBy = identity.FullName;
                _preRecruitmentEmployeeService.UpdateApprove(preRecruitmentEmployee);

                if (preRecruitmentEmpQualificationList != null)
                {
                    foreach (PreRecruitmentEmpQualification preRecruitmentEmpQualification in preRecruitmentEmpQualificationList)
                    {
                        preRecruitmentEmpQualification.ApprovedFromIP = identity.IPAddress;
                        preRecruitmentEmpQualification.ApprovedDateTime = DateTime.Now;
                        preRecruitmentEmpQualification.ApprovedBy = identity.FullName;
                        _preRecruitmentEmpQualificationService.UpdateGraph(preRecruitmentEmpQualification);
                    }
                }
                if (preRecruitmentEmpExperienceList != null)
                {
                    foreach (PreRecruitmentEmpExperience preRecruitmentEmpExperience in preRecruitmentEmpExperienceList)
                    {
                        preRecruitmentEmpExperience.ApprovedFromIP = identity.IPAddress;
                        preRecruitmentEmpExperience.ApprovedDateTime = DateTime.Now;
                        preRecruitmentEmpExperience.ApprovedBy = identity.FullName;
                        _preRecruitmentEmpExperienceService.UpdateGraph(preRecruitmentEmpExperience);
                    }
                }
                if (preRecruitmentEmpTrainingList != null)
                {
                    foreach (PreRecruitmentEmpTraining preRecruitmentEmpTraining in preRecruitmentEmpTrainingList)
                    {
                        preRecruitmentEmpTraining.ApprovedFromIP = identity.IPAddress;
                        preRecruitmentEmpTraining.ApprovedDateTime = DateTime.Now;
                        preRecruitmentEmpTraining.ApprovedBy = identity.FullName;
                        _preRecruitmentEmpTrainingService.UpdateGraph(preRecruitmentEmpTraining);
                    }
                }
                if (preRecruitmentDocumentList != null)
                {
                    foreach (PreRecruitmentDocument preRecruitmentDocument in preRecruitmentDocumentList)
                    {
                        preRecruitmentDocument.ApprovedFromIP = identity.IPAddress;
                        preRecruitmentDocument.ApprovedDateTime = DateTime.Now;
                        preRecruitmentDocument.ApprovedBy = identity.FullName;
                        _preRecruitmentDocumentService.UpdateGraph(preRecruitmentDocument);
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

        public IEnumerable<object> GetEmployeeData(string eId)
        {
            try
            {
                var sql = @"Select PRE.*, COALESCE(PO.UserName,'') PresentPoliceStation, COALESCE( ParmP.UserName, '')ParmanentPoliceStation,
                           COALESCE(D.UserName,'') PresDistrict, COALESCE(ParmD.UserName,'') ParmDistrict,COALESCE(C.UserName,'') PresCountry,COALESCE(ParmC.UserName,'') ParmCountry,
                           COALESCE(ParmP.UserName,'') ParmPostOffice,COALESCE(PerP.UserName,'') PresPostOffice,COALESCE(PerCT.UserName,'') PresCity,COALESCE(ParCT.UserName,'') ParmCity,
                           COALESCE(ParmS.UserName,'') ParmState,COALESCE(PresS.UserName,'') PresState
                            FROM PreRecruitmentEmployee PRE
                             LEFT JOIN scs.PoliceStation PO ON PRE.PresThanaID=PO.Id
                             LEFT JOIN scs.PoliceStation ParmPO ON PRE.ParmThanaID=ParmPO.Id
                             LEFT JOIN SCS.District D ON PRE.PresDistrictID = D.Id
                             LEFT JOIN SCS.District ParmD ON PRE.ParmDistrictID = ParmD.Id
                             LEFT JOIN SCS.Country C ON PRE.PresCountryID = C.ID
                             LEFT JOIN SCS.Country ParmC ON PRE.ParmCountryID = ParmC.ID
                             LEFT JOIN SCS.PostOffice ParmP ON PRE.ParmPostOfficeID = ParmP.ID
                             LEFT JOIN SCS.PostOffice PerP ON PRE.PresPostOfficeID = PerP.ID
                             LEFT JOIN SCS.City PerCT ON PRE.PresCityID = PerCT.ID
                             LEFT JOIN SCS.City ParCT ON PRE.ParmCityID = ParCT.ID
                             LEFT JOIN SCS.[State] ParmS ON PRE.ParmStateId = ParmS.Id
                             LEFT JOIN SCS.[State] PresS ON PRE.PresStateId = PresS.Id
                            WHERE PRE.Id='" + eId + "'";
                return _sqlRepository.GetDataCollection(sql);
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
                var sql = @"Select PD.*,CD.UserName ComplianceDocument,CDS.OptionalOrMandatory from [dbo].[PreRecruitmentDocument] PD
                            Left outer join HKP.ComplianceDocument CD ON PD.ComplianceDocumentId=CD.Id
                            Left outer join HKP.ComplianceDocumentSetDetail CDS ON PD.ComplianceDocumentSetId=CDS.Id
							WHERE ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')  AND CD.EmploymentStage = 'PreRecruitment' AND PD.PreRecruitmentEmployeeId='" + eId + "'Order By ComplianceDocument";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetAllSubmittedEmployee(GridParameter parameters, bool isControlAdmin, bool isSysAdmin, string companyGroupId, string companyId, string employeeId)
        {
            try
            {
                var str = "";
                if (!isControlAdmin && !isSysAdmin)
                    str = @" AND PRE.BudgetId IN (SELECT Id from mst.ManpowerBudget WHERE EntityId IN (SELECT entityid FROM [HKP].[ApprovalConfiguration] WHERE PreRecruitmentDocRP='" + employeeId + "'))";
                parameters.CmdText = @"Select PRE.*,E.UserName EntityName,D.UserName Designation,PR.UserName PositionName
									 ,DEG.UserName GivenDesignation, DEPT.UserName AS Department
									 FROM PreRecruitmentEmployee PRE
									 LEFT OUTER JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id
									 LEFT OUTER JOIN ORG.Position PR ON PMB.PositionId=PR.Id
									 LEFT OUTER JOIN HKP.Designation DEG on DEG.Id=PRE.GivenDesignationId
								     LEFT OUTER JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
									 LEFT OUTER JOIN ORG.Entity E ON PMB.EntityId=E.Id
									 LEFT OUTER JOIN HKP.Designation D ON PR.DesignationId=D.Id
									 Where PRE.GroupID='" + companyGroupId + @"' AND PRE.CompanyId='" + companyId + @"' AND PRE.IsApproved=0 " + str;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetAllDocumentData(string companyGroupId, string budgetId, string plantId, string empType, string pId)
        {
            try
            {
                var sql = @"SELECT PD.Id,PD.FileName,PD.FileId,PD.PreRecruitmentEmployeeId,CD.Id AS ComplianceDocumentId,
                            CD.UserName DocumentName,CD.DocumentType,PD.IsDocumentApproved,
                            CD.IsSkillBased,PC.PositionId,CDSD.OptionalOrMandatory,
                            CD.EmpType,E.UserName AS EmployeeCategory
                            FROM HKP.ComplianceDocumentSet AS CDS
                            LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON CDS.Id=DC.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id= CDSD.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
                            LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
                            LEFT OUTER JOIN HKP.ComplianceDocumentPositonCode PC ON CD.Id=PC.ComplianceDocumentId
                            LEFT OUTER JOIN ORG.Position PO ON PC.PositionId=PO.Id
                            LEFT OUTER JOIN  (Select * from  dbo.PreRecruitmentDocument Where PreRecruitmentEmployeeId='" + pId + @"') PD ON CD.Id=PD.ComplianceDocumentId
                            WHERE CD.EmploymentStage='PreRecruitment'
							and DC.EmployeeCategoryId=(Select D.EmployeeCategoryId From
                         (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + companyGroupId + @"') AS D
                         LEFT OUTER JOIN ORG.Position AS P ON P.DesignationId = D.DesignationId
                         LEFT OUTER JOIN MST.ManpowerBudget AS M ON M.PositionId=P.Id WHERE M.Id= '" + budgetId + @"')
                         AND DC.PlantId='" + plantId + @"' and CD.IsSkillBased=1 AND PC.PositionId=(select PositionId from MST.ManpowerBudget WHERE Id= '" + budgetId + @"')
                         AND (CD.EmpType='" + empType + @"' or CD.EmpType='Both')
						  union
						  SELECT PD.Id,PD.FileName,PD.FileId,PD.PreRecruitmentEmployeeId,CD.Id AS ComplianceDocumentId,
                            CD.UserName DocumentName,CD.DocumentType,PD.IsDocumentApproved,
                            CD.IsSkillBased,'' PositionId,CDSD.OptionalOrMandatory,
                            CD.EmpType,E.UserName AS EmployeeCategory
                            FROM HKP.ComplianceDocumentSet AS CDS
                            LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON CDS.Id=DC.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id= CDSD.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
                            LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
                            LEFT OUTER JOIN  (Select * from  dbo.PreRecruitmentDocument Where PreRecruitmentEmployeeId='" + pId + @"') PD ON CD.Id=PD.ComplianceDocumentId
                            WHERE CD.EmploymentStage='PreRecruitment'
							and DC.EmployeeCategoryId=(Select D.EmployeeCategoryId From
                         (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + companyGroupId + @"') AS D
                         LEFT OUTER JOIN ORG.Position AS P ON P.DesignationId = D.DesignationId
                         LEFT OUTER JOIN MST.ManpowerBudget AS M ON M.PositionId=P.Id WHERE M.Id= '" + budgetId + @"')
                         AND DC.PlantId='" + plantId + @"' and CD.IsSkillBased=0
                         AND (CD.EmpType='" + empType + @"' or CD.EmpType='Both') Order By CD.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
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