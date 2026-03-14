#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Addresses;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class PreRecruitmentEmployeeService : Service<PreRecruitmentEmployee>, IPreRecruitmentEmployeeService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<PreRecruitmentEmployee> _preRecruitmentEmployeeRepository;
        private readonly IRepositoryAsync<SMTPConfiguration> _smtpConfigurationRepository;
        private readonly IRepositoryAsync<ApprovalConfiguration> _ApprovalConfigurationRepository;
        private readonly IEmployeeInformationService _employeeInformationService;
        private readonly ISignatureService _signatrueService;
        private readonly IPlantService _plantService;

        public PreRecruitmentEmployeeService(
            IRepositoryAsync<PreRecruitmentEmployee> preRecruitmentEmployeeRepository
            , IRepositoryAsync<ApprovalConfiguration> ApprovalConfigurationRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISignatureService signatrueService
            , IEmployeeInformationService employeeInformationService
            , IRepositoryAsync<SMTPConfiguration> smtpConfigurationRepository
            , ISqlRepository sqlRepository
            , IPlantService plantService
            ) : base(preRecruitmentEmployeeRepository, unitOfWork, pkGeneratorService)
        {
            _preRecruitmentEmployeeRepository = preRecruitmentEmployeeRepository;
            _ApprovalConfigurationRepository = ApprovalConfigurationRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _signatrueService = signatrueService;
            _employeeInformationService = employeeInformationService;
            _smtpConfigurationRepository = smtpConfigurationRepository;
            _plantService = plantService;
        }

        #endregion Constructor

        #region Login

        public bool Login(string id, string pin)
        {
            try
            {
                var data = Find(id);
                if (data == null)
                    throw new CustomException("Invalid employee id");
                if (!data.ReadyForCandidateAccess && !data.SelectionDateTime.HasValue) throw new CustomException("You have no permision");
                if (data.Completed) throw new CustomException("You have no permision");
                var expireDate = data.SelectionDateTime.Value.Date.AddDays(data.ExpiredDays + 1);
                if (expireDate < DateTime.Now.Date) throw new CustomException("Account expired.");
                if (data.IsFirstlogin)
                {
                    if (data.NewPIN != pin)
                        throw new CustomException("Invalid pin");
                }
                else if (data.InitialPIN != pin)
                    throw new CustomException("Invalid pin");

                data.LastLoginTime = DateTime.Now;
                Update(data);
                return data.IsFirstlogin;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdatePinAndLoginFlag(string id, string pin)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException("Employee id is required");
                if (string.IsNullOrEmpty(pin))
                    throw new CustomException("Pin is required");

                var data = Find(id);
                if (data == null)
                    throw new CustomException("Invalid employee id");
                if (data.InitialPIN == pin)
                    throw new CustomException("This password is not available");
                data.NewPIN = pin;
                data.IsFirstlogin = true;
                data.LastLoginTime = DateTime.Now;
                Update(data);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion Login

        #region GetData

        public IEnumerable<object> GetData(string empid)
        {
            try
            {
                var data = Find(empid);
                if (!data.IsFirstlogin)
                    throw new CustomException("Invalid employee.");
                var sql = @"SELECT PRE.*
							       	,CG.[Image] CompanyGroupLogo
							       	,CNT.PhoneLength
							       	,COM.IsTINRequiredForSalaryAbove
							       	,CNT.TINCaption
							       	,CNT.NIDCaption
							       	,CNT.NIDLength
							       	,CNT.TINLength
							       	,COM.TINRequiredForSalaryAbove
							       	,PO.UserName PresThanaName
							       	,ParmPO.UserName ParmThanaName
							       	,D.UserName PresDistrictName
							       	,ParmD.UserName ParmDistrictName
							       	,C.UserName PresCountryName
							       	,ParmC.UserName ParmCountryName
							       	,ParmP.UserName ParmPostOfficeName
							       	,PerP.UserName PresPostOfficeName
							       	,PerCT.UserName PresCityName
							       	,ParCT.UserName ParmCityName
							       	,AM.CountryId
							       FROM PreRecruitmentEmployee PRE
							       LEFT JOIN ORG.CompanyGroup AS CG ON PRE.GroupId = CG.Id
							       LEFT JOIN scs.PoliceStation PO ON PRE.PresThanaID = PO.Id
							       LEFT JOIN scs.PoliceStation ParmPO ON PRE.ParmThanaID = ParmPO.Id
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
							       LEFT JOIN ORG.Plant PL ON PRE.PlantId = PL.Id
							       LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId = AM.Id
							       LEFT JOIN SCS.Country CNT ON AM.CountryId = CNT.Id
							       LEFT JOIN ORG.Company COM ON PRE.CompanyId = COM.Id
                              WHERE PRE.Id='" + empid + "' AND PRE.Completed=0 ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetJobData(string Id)
        {
            try
            {
                var sql = @"SELECT JDI.Id, JDI.UserName JobDescription from [MST].[ManpowerBudgetJobDescription] PMBJD
                            LEFT OUTER JOIN [HKP].[JobDescription] JD ON PMBJD.JobDescriptionId=JD.Id
                            LEFT OUTER JOIN [HKP].[JobDescriptionItem] JDI ON JD.JobDescriptionItemId=JDI.Id
                            Where PMBJD.ManpowerBudgetId=(Select PR.BudgetId From [dbo].[PreRecruitmentEmployee] PR Where PR.Id='" + Id + "') AND PMBJD.Archive=0";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetDocumentData(string companyGroupId, string budgetId, string plantId, string empType, string pId)
        {
            try
            {
                var sql = @"SELECT PD.Id,PD.FileName,PD.FileId,PD.PreRecruitmentEmployeeId,CD.Id AS ComplianceDocumentId,
                            CD.UserName DocumentName,CD.DocumentType,
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
                            WHERE CD.EmploymentStage='PreRecruitment' AND CD.DocumentationBy='Self'
							and DC.EmployeeCategoryId =
							 (Select D.EmployeeCategoryId From
                         (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + companyGroupId + @"') AS D
						 Left outer join dbo.PreRecruitmentEmployee PE ON D.DesignationId=PE.GivenDesignationId
						 WHERE PE.BudgetId= '" + budgetId + @"' AND PE.Id='" + pId + @"')
							--=(Select D.EmployeeCategoryId From
       --                  (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + companyGroupId + @"') AS D
       --                  LEFT OUTER JOIN ORG.Position AS P ON P.DesignationId = D.DesignationId
       --                  LEFT OUTER JOIN MST.ManpowerBudget AS M ON M.PositionId=P.Id WHERE M.Id= '" + budgetId + @"')

                         AND DC.PlantId='" + plantId + @"' and CD.IsSkillBased=1 AND PC.PositionId=(select PositionId from MST.ManpowerBudget WHERE Id= '" + budgetId + @"')
                         AND (CD.EmpType='" + empType + @"' or CD.EmpType='Both')
						  UNION
						  SELECT PD.Id,PD.FileName,PD.FileId,PD.PreRecruitmentEmployeeId,CD.Id AS ComplianceDocumentId,
                            CD.UserName DocumentName,CD.DocumentType,
                            CD.IsSkillBased,'' PositionId,CDSD.OptionalOrMandatory,
                            CD.EmpType,E.UserName AS EmployeeCategory
                            FROM HKP.ComplianceDocumentSet AS CDS
                            LEFT OUTER JOIN HKP.DocumentConfigurationDesignationGroup DC ON CDS.Id=DC.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CDS.Id= CDSD.ComplianceDocumentSetId
                            LEFT OUTER JOIN HKP.ComplianceDocument AS CD ON CDSD.ComplianceDocumentId = CD.Id
                            LEFT OUTER JOIN HKP.EmployeeCategory AS E ON DC.EmployeeCategoryId = E.Id
                            LEFT OUTER JOIN  (Select * from  dbo.PreRecruitmentDocument Where PreRecruitmentEmployeeId='" + pId + @"') PD ON CD.Id=PD.ComplianceDocumentId
                            WHERE CD.EmploymentStage='PreRecruitment' AND CD.DocumentationBy='Self'
							and DC.EmployeeCategoryId =
							 (Select D.EmployeeCategoryId From
                         (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + companyGroupId + @"') AS D
						 Left outer join dbo.PreRecruitmentEmployee PE ON D.DesignationId=PE.GivenDesignationId
						 WHERE PE.BudgetId= '" + budgetId + @"' AND PE.Id='" + pId + @"')
							--=(Select D.EmployeeCategoryId From
       --                  (SELECT * FROM MST.DesignationMaster Where CompanyGroupId='" + companyGroupId + @"') AS D
       --                  LEFT OUTER JOIN ORG.Position AS P ON P.DesignationId = D.DesignationId
       --                  LEFT OUTER JOIN MST.ManpowerBudget AS M ON M.PositionId=P.Id WHERE M.Id= '" + budgetId + @"')

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

        public IEnumerable<object> GetDocumentDataList(string companyGroupId, string budgetId, string pId, string plantId)
        {
            try
            {
                var sql = @"SELECT distinct  PD.*
									,CD.UserName DocumentName
									,CD.DocumentType
									,CD.IsSkillBased
									,CDSD.OptionalOrMandatory
									,CD.EmpType
									,CD.ProfileType,CD.DocNumberRequired,CD.DocDateRequired
									,E.UserName AS EmployeeCategory
                                    ,CD.DocumentationBy
								FROM dbo.PreRecruitmentDocument PD
								LEFT JOIN hkp.ComplianceDocument CD ON PD.ComplianceDocumentId = CD.Id
								LEFT JOIN HKP.ComplianceDocumentSetDetail AS CDSD ON CD.Id = CDSD.ComplianceDocumentId
								LEFT JOIN (select  * from hkp.DocumentConfigurationDesignationGroup

								Where PlantId='" + plantId + @"' and EmployeeCategoryId = (
										SELECT D.EmployeeCategoryId
										FROM (SELECT * FROM MST.DesignationMaster WHERE CompanyGroupId = '" + companyGroupId + @"'
											) AS D
										LEFT JOIN dbo.PreRecruitmentEmployee PE ON D.DesignationId = PE.GivenDesignationId
										WHERE PE.BudgetId = '" + budgetId + @"'
											AND PE.Id = '" + pId + @"'
										)
								)DD ON CDSD.ComplianceDocumentSetId = DD.ComplianceDocumentSetId
								LEFT JOIN HKP.EmployeeCategory AS E ON DD.EmployeeCategoryId = E.Id
								WHERE PD.PreRecruitmentEmployeeId = '" + pId + @"'
									AND CD.EmploymentStage = 'PreRecruitment'
									AND CD.DocumentationBy = 'Self'
									AND ISNULL(CD.ProfileType,'') NOT IN ('Qualification','Training','Experience','Photo')
									AND E.UserName IS NOT NULL";
                //AND PD.DueDate IS NOT NULL";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetAllCandidate(GridParameter parameters, string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"Select E.*,D.UserName Designation, DEG.UserName GivenDesignation, DEPT.UserName Department From dbo.PreRecruitmentEmployee E
                                        LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetId = PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                                        LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
                                        LEFT JOIN HKP.Designation DEG ON DEG.Id = E.GivenDesignationId
                                    WHERE E.GroupID='" + identity.CompanyGroupId + @"' AND E.CompanyId='" + identity.CompanyId + "' AND E.PlantId='" + plantId + "' AND E.Completed=0 AND E.IsApproved=0 AND E.ReadyForCandidateAccess=1";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetCandidateData(GridParameter parameters, string companyGroupId, string companyId, string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PRE.*, PR.UserName PositionName,OM.Code as OperationMasterCode, E.UserName EntityName, D.UserName Designation, DG.UserName GivenDesignation
                                ,PR.DesignationId, CNT.PhoneLength ,CNT.TINCaption,CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove, PMB.Code
                            	FROM PreRecruitmentEmployee PRE
                            LEFT JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                            LEFT JOIN HKP.Designation DG ON PRE.GivenDesignationId=DG.Id
							LEFT JOIN ORG.Plant PL ON PRE.PlantId = PL.Id
							LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
							LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
							LEFT JOIN ORG.Company COM ON PRE.CompanyId=COM.Id
                            left join MST.OperationMaster OM ON OM.Id = PRE.OperationMasterID
                            WHERE PRE.GroupID='" + companyGroupId + "' AND PRE.CompanyId='" + companyId + "' AND PRE.PlantId='" + plantId + "' AND PRE.Completed=0 AND PRE.IsApproved=0 AND PRE.ReadyForCandidateAccess=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetCandidateDataWithAssignNonAssignDoc(GridParameter parameters, string assign, string plantId)
        {
            try
            {
                var a = "";
                a = assign.ToUpper() == "ASSIGN" ? " AND ED.TotalDoc>0" : " AND isnull(ED.TotalDoc,0)=0";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"Select PRE.*,0 Active,E.UserName EntityName,D.UserName Designation,PR.UserName PositionName,DEG.UserName GivenDesignations, DEPT.UserName AS Department,ED.TotalDoc,PMB.Code
							FROM PreRecruitmentEmployee PRE
							LEFT JOIN MST.ManpowerBudget PMB ON PRE.BudgetId=PMB.Id
							LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
							LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
							LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
							LEFT JOIN HKP.Designation DEG on DEG.Id=PRE.GivenDesignationId
							LEFT JOIN (SELECT PreRecruitmentEmployeeId, COUNT (Id) TotalDoc FROM  dbo.PreRecruitmentDocument group by PreRecruitmentEmployeeId) AS ED ON PRE.Id=ED.PreRecruitmentEmployeeId
							Where PRE.Completed=0 AND PRE.GroupID='" + identity.CompanyGroupId + @"' AND PRE.CompanyId='" + identity.CompanyId + @"' AND PRE.PlantId='" + plantId + @"' " + a + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion GetData

        #region PreRecruitmentEmployee

        public IEnumerable<object> GetEntityByEmployee(string tableName, string fieldName, string employeeId)
        {
            try
            {
                var _sql = @"SELECT E.Code , E.UserName AS EntityName, C.UserName AS CompanyName FROM ORG.Entity AS E
                            LEFT OUTER JOIN ORG.Company AS C ON E.CompanyId=C.Id WHERE E.Id IN(
                            SELECT EntityId FROM " + tableName + " WHERE " + fieldName + "='" + employeeId + "')";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateMaster(PreRecruitmentEmployee entity)
        {
            try
            {
                var dblist = Find(entity.Id);
                dblist.Image = entity.Image;
                dblist.Gender = entity.Gender;
                dblist.DOB = entity.DOB;
                dblist.Phone = entity.Phone;
                dblist.Email = entity.Email;
                dblist.Salutation = entity.Salutation;
                dblist.FirstName = entity.FirstName;
                dblist.MiddleName = entity.MiddleName;
                dblist.LastName = entity.LastName;
                dblist.NickName = entity.NickName;
                dblist.EmployeeName = entity.FirstName + " " + entity.MiddleName + " " + entity.LastName;
                dblist.BirthdayCelebrationDate = entity.BirthdayCelebrationDate;
                dblist.IsKnownPerson = entity.IsKnownPerson;
                dblist.NumberOfKnownPerson = entity.NumberOfKnownPerson;
                dblist.ApplyingAsFresher = entity.ApplyingAsFresher;
                dblist.NationalID = entity.NationalID;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public PreRecruitmentEmployee GetMaster(string PK)
        {
            try
            {
                var _sql = "SELECT * FROM PreRecruitmentEmployee WHERE Id='" + PK + "'";
                return _preRecruitmentEmployeeRepository.SelectQuery(_sql, null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdatePersonal(PreRecruitmentEmployee entity)
        {
            try
            {
                var dblist = Find(entity.Id);

                dblist.FatherName = entity.FatherName;
                dblist.MotherName = entity.MotherName;
                dblist.CitizenID = entity.CitizenID;
                dblist.ReligionID = entity.ReligionID;
                dblist.BloodGroupID = entity.BloodGroupID;
                dblist.CivilStatusID = entity.CivilStatusID;
                dblist.SpouseName = entity.SpouseName;
                dblist.SpouseNationalID = entity.SpouseNationalID;
                dblist.SpouseOccupation = entity.SpouseOccupation;
                dblist.NoOfChildren = entity.NoOfChildren;
                dblist.TIN = entity.TIN;
                dblist.MarriagedayCelebrationDate = entity.MarriagedayCelebrationDate;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateAddress(PreRecruitmentEmployee entity)
        {
            try
            {
                var dblist = Find(entity.Id);
                dblist.PresentAddress1 = entity.PresentAddress1;
                dblist.PresentAddress2 = entity.PresentAddress2;
                dblist.ParmanentAddress1 = entity.ParmanentAddress1;
                dblist.ParmanentAddress2 = entity.ParmanentAddress2;
                dblist.PresThanaID = entity.PresThanaID;
                dblist.ParmThanaID = entity.ParmThanaID;
                dblist.PresPostOfficeID = entity.PresPostOfficeID;
                dblist.ParmPostOfficeID = entity.ParmPostOfficeID;
                dblist.PresZipCode = entity.PresZipCode;
                dblist.ParmZipCode = entity.ParmZipCode;
                dblist.PresDistrictID = entity.PresDistrictID;
                dblist.ParmDistrictID = entity.ParmDistrictID;
                dblist.PresCountryID = entity.PresCountryID;
                dblist.ParmCountryID = entity.ParmCountryID;
                dblist.PresCityID = entity.PresCityID;
                dblist.ParmCityID = entity.ParmCityID;
                dblist.PresAreaID = entity.PresAreaID;
                dblist.ParmAreaID = entity.ParmAreaID;
                dblist.EmrCntPer1Name = entity.EmrCntPer1Name;
                dblist.EmrCntPer2Name = entity.EmrCntPer2Name;
                dblist.EmrCntPer1CellNo = entity.EmrCntPer1CellNo;
                dblist.EmrCntPer2CellNo = entity.EmrCntPer2CellNo;
                dblist.ParmanentArea = entity.ParmanentArea;
                dblist.PresentArea = entity.PresentArea;
                dblist.ParmStateId = entity.ParmStateId;
                dblist.PresStateId = entity.PresStateId;
                dblist.EmrCntPer1CellNo2 = entity.EmrCntPer1CellNo2;
                dblist.EmrCntPer1CellNo3 = entity.EmrCntPer1CellNo3;
                dblist.EmrCntPer2CellNo2 = entity.EmrCntPer2CellNo2;
                dblist.EmrCntPer2CellNo3 = entity.EmrCntPer2CellNo3;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateFinal(PreRecruitmentEmployee entity)
        {
            try
            {
                var emailSetup = new EmailSetup();
                var dblist = Find(entity.Id);
                dblist.Submitted = true;
                dblist.SubmitDateTime = DateTime.Now;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateSubmitByDepartment(IEnumerable<PreRecruitmentEmployee> entities)
        {
            try
            {
                var pks = entities.Select(t => t.Id);
                var from_db = base.Query(t => pks.Contains(t.Id)).Select().AsEnumerable();

                foreach (var item in entities)
                {
                    if (!from_db.Any(t => t.Id == item.Id))
                        throw new CustomException(ServiceResources.RecordNoLonger.ToString());

                    item.IsDepartmentSubmit = true;
                    Update(item);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateApprove(PreRecruitmentEmployee entity)
        {
            try
            {
                var dblist = Find(entity.Id);
                dblist.IsImage = entity.IsImage;
                dblist.IsApproved = true;
                dblist.ApprovedDateTime = DateTime.Now;
                dblist.ApprovedFromIP = entity.ApprovedFromIP;
                dblist.ApprovedBy = entity.ApprovedBy;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateCandidate(PreRecruitmentEmployee entity)
        {
            try
            {
                var dblist = Find(entity.Id);
                dblist.Submitted = entity.Submitted;
                dblist.ExpiredDays = entity.ExpiredDays;
                dblist.NewPIN = entity.NewPIN;
                Update(dblist);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel SearchPostOfficeName(GridParameter parameters, string sCountry, string sDistrict)
        {
            try
            {
                parameters.sort = "PostOfficeName";
                parameters.searchBy = "PostOfficeName";
                var strSQL = @"SELECT * FROM
                                    (SELECT PO.Id SystemID, PO.Code, PO.UserName PostOfficeName,
                                   D.ID DistrictID, D.UserName DistrictName, C.Id CountryID, C.UserName CountryName
		                           FROM scs.PostOffice PO
		                           LEFT JOIN SCS.District D ON po.DistrictID = D.Id
		                           LEFT JOIN SCS.[State] s	ON D.StateId = s.ID
		                           LEFT JOIN SCS.Country C	ON s.CountryId = C.ID  ";

                if (sDistrict.Trim() != "")
                {
                    strSQL = strSQL + " WHERE T.DistrictID = '" + sDistrict.Trim() + @"'";
                }

                strSQL = strSQL + ") A";

                if (sCountry.Trim() != "")
                {
                    strSQL = strSQL + " WHERE CountryName = '" + sCountry.Trim() + @"'";
                }

                parameters.CmdText = strSQL;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                                   Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                                   ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetPoliceStationName(GridParameter parameters, string districtId)
        {
            try
            {
                parameters.sort = "PoliceStationName";
                parameters.searchBy = "PoliceStationName";
                var strSQL = @"SELECT * FROM (SELECT PO.Id SystemID,PO.Code, PO.UserName PoliceStationName,
                                   D.ID DistrictID, D.UserName DistrictName, C.Id CountryID, C.UserName CountryName
		                           FROM scs.PoliceStation PO
		                           LEFT JOIN SCS.District D ON po.DistrictID = D.Id
		                           LEFT JOIN SCS.[State] s	ON D.StateId = s.ID
		                           LEFT JOIN SCS.Country C	ON s.CountryId = C.ID";
                if (districtId.Trim() != "")
                {
                    strSQL = strSQL + " WHERE PO.DistrictID = '" + districtId.Trim() + @"'";
                }

                strSQL = strSQL + ") A";

                parameters.CmdText = strSQL;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel SearchCountryName(GridParameter parameters)
        {
            try
            {
                parameters.sort = "UserName";
                parameters.searchBy = "UserName";
                parameters.CmdText = @"SELECT C.Id,
                                  C.Code,
                                  C.UserName,
                                  C.ShortName,
                                  C.StandardName,
                                  C.Nationality,
                                  C.Description,
                                  C.Remarks,
                                  C.GMTMinute,
                                  C.GMTHour
                           FROM   scs.Country C ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel SearchCityName(GridParameter parameters, string countryId)
        {
            try
            {
                parameters.sort = "UserName";
                parameters.searchBy = "UserName";
                var strSQL = @"Select C.Id, C.Code, C.ShortName, C.StandardName, C.UserName, C.Description, C.Remarks From SCS.City C ";
                if (countryId.Trim() != "")
                {
                    strSQL = strSQL + " WHERE C.CountryID = '" + countryId.Trim() + @"'";
                }
                parameters.CmdText = strSQL;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel SearchDistrictName(GridParameter parameters, string countryId)
        {
            try
            {
                parameters.sort = "DistrictName";
                parameters.searchBy = "DistrictName";
                var strSQL = @"SELECT * FROM (SELECT D.ID, D.Code, D.UserName DistrictName, C.UserName CountryName
                                        FROM scs.District D
                                        LEFT JOIN scs.[State] s	ON D.StateId = s.ID
                                        LEFT JOIN scs.Country C	ON s.CountryId = C.ID  ";

                if (countryId.Trim() != "")
                {
                    strSQL = strSQL + " WHERE s.CountryID = '" + countryId.Trim() + @"'";
                }

                strSQL = strSQL + ") A";

                parameters.CmdText = strSQL;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #endregion PreRecruitmentEmployee

        #region EmployeeInformation

        public IEnumerable<object> inactiveEmps(string col, string cGroupId, string val)
        {
            try
            {
                string filter = "";
                if (col == "" || col == null || val == "" || val == null)
                {
                    filter = "";
                }
                else
                {
                    filter = filter + " and EI." + col + @" = '" + val + "'";
                }
                var str = @"SELECT EI.*,format(EI.DOJ , 'dd-MMM-yyyy') as EDOJ,format(EI.DOS , 'dd-MMM-yyyy') as EDOS ,format(EI.DOB , 'dd-MMM-yyyy') as EDOB,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
                 ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
                                         ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
                 ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
                 ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
                 ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation,PR.DesignationId
                                         ,EAG.AttendanceGroupId,PGM.PayrollGroupId, OM.Code OperationMasterCode , OV.Code OperationVariationCode,LD.UserName LegalDesignation
                                    FROM dbo.Employeeinformation EI
                                     LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
                LEFT JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
                LEFT JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
                LEFT JOIN SCS.District D ON EI.PresDistrictID = D.Id
                LEFT JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
                               LEFT JOIN SCS.Country C ON EI.PresCountryID = C.ID
                               LEFT JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
                               LEFT JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
                               LEFT JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
                                     LEFT JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
                               LEFT JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
                                     LEFT JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
                LEFT JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
                LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
                LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
                LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
                LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                                     LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                     LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                     LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                     LEFT JOIN dbo.EmployeeAttendanceGroup EAG on EAG.EmployeeId=EI.SystemId
                         LEFT JOIN MST.PayrollGroupMaster PGM on PGM.EmployeeId=EI.SystemId
                LEFT JOIN MST.OperationMaster OM ON OM.Id=EI.OperationMasterID
                LEFT JOIN MST.OperationVariation OV ON OV.Id=EI.OperationVariationId
                                     LEFT JOIN [HKP].[LegalDesignation] LD ON LD.Id=EI.LegalDesignationId
                                     WHERE EI.EmployeeStatus ='Separated'  AND  EI.GroupId='" + cGroupId + @"' " + filter + @"";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static string GetPadding(string iv)
        {
            while (iv.Length < 5)
            {
                iv = "0" + iv;
            }
            return iv;
        }

        public void Insert(EmployeeInformation entity)
        {
            try
            {
                var prefix = _plantService.GetPlantPrefix(entity.PlantID);
                if (string.IsNullOrEmpty(prefix))
                    throw new Exception("No prefix found for this plant.");
                if (entity.SystemId == null)
                {
                    var epk = _signatrueService.GetAutoNumber("EMP_BASIC", DateTime.Now);

                    entity.SystemId = DateTime.Now.ToString("yy") + GetPadding(Convert.ToInt32(epk).ToString());
                    entity.EmployeeName = entity.FirstName + " " + entity.LastName;
                    var pk = _signatrueService.GetMaxNumber(entity.PlantID + "EMP_BASIC", DateTime.Now);
                    pk.LastNumber++;
                    var pad = GetPadding(Convert.ToInt32(pk.LastNumber).ToString());
                    entity.EmployeeId = prefix + DateTime.Now.ToString("yy") + pad; ;
                    entity.EmployeeCode = entity.EmployeeId;
                    entity.EmpPicPath = entity.SystemId + entity.EmpPicPath;
                    entity.DateAdded = DateTime.Now;
                    _employeeInformationService.Insert(entity);
                }
                else
                {
                    entity.DateUpdated = DateTime.Now;
                    _employeeInformationService.Update(entity);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.*,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
                 ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
                                         ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
                 ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
                 ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
                 ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation,PR.DesignationId
                                         ,EAG.AttendanceGroupId,PGM.PayrollGroupId, OM.Code OperationMasterCode , OV.Code OperationVariationCode,LD.UserName LegalDesignation
                                     FROM dbo.Employeeinformation EI
                                     LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
                LEFT JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
                LEFT JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
                LEFT JOIN SCS.District D ON EI.PresDistrictID = D.Id
                LEFT JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
                               LEFT JOIN SCS.Country C ON EI.PresCountryID = C.ID
                               LEFT JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
                               LEFT JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
                               LEFT JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
                                     LEFT JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
                               LEFT JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
                                     LEFT JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
                LEFT JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
                LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
                LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
                LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
                LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                                     LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                     LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                     LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                         LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                                     LEFT JOIN dbo.EmployeeAttendanceGroup EAG on EAG.EmployeeId=EI.SystemId
                         LEFT JOIN MST.PayrollGroupMaster PGM on PGM.EmployeeId=EI.SystemId
                LEFT JOIN MST.OperationMaster OM ON OM.Id=EI.OperationMasterID
                LEFT JOIN MST.OperationVariation OV ON OV.Id=EI.OperationVariationId
                                     LEFT JOIN [HKP].[LegalDesignation] LD ON LD.Id=EI.LegalDesignationId
                                     WHERE EI.EmployeeStatus ='Active' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetActiveAndInActiveEmployeeList(GridParameter parameters, string companyGroupId, string plantId, string EmployeeStatus)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.*,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
                 ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
                 ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
                 ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
                 ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
                 ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation,PR.DesignationId
                 ,EAG.AttendanceGroupId,PGM.PayrollGroupId, OM.Code OperationMasterCode, OV.Code OperationVariationCode,LD.UserName LegalDesignation,S.UserName Section
                FROM dbo.Employeeinformation EI
                LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
                LEFT JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
                LEFT JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
                LEFT JOIN SCS.District D ON EI.PresDistrictID = D.Id
                LEFT JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
                LEFT JOIN SCS.Country C ON EI.PresCountryID = C.ID
                LEFT JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
                LEFT JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
                LEFT JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
                LEFT JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
                LEFT JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
                LEFT JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
                LEFT JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
                LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
                LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
                LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
                LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
                LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
                LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
                LEFT JOIN dbo.EmployeeAttendanceGroup EAG on EAG.EmployeeId=EI.SystemId
                LEFT JOIN MST.PayrollGroupMaster PGM on PGM.EmployeeId=EI.SystemId
                LEFT JOIN MST.OperationMaster OM ON OM.Id=EI.OperationMasterID
                LEFT JOIN MST.OperationVariation OV ON OV.Id=EI.OperationVariationId
                LEFT JOIN [HKP].[LegalDesignation] LD ON LD.Id=EI.LegalDesignationId
                LEFT JOIN ORG.Section S ON S.Id=EI.SectionId
                                     WHERE EI.EmployeeStatus ='" + EmployeeStatus + "' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeWithPlant(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.*,DG.UserName Designation, DP.UserName Department
                              FROM dbo.Employeeinformation EI
							  LEFT OUTER JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT OUTER JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
							  LEFT  JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
				              LEFT  JOIN ORG.Department DP on DP.Id=EI.DepartmentId
                              Where EI.PlantId='" + plantId + "' AND EI.IsApproved=1 AND EI.EmployeeStatus='Active'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> CboList()
        {
            try
            {
                return _sqlRepository.GetDataCollection(@"Select D.Id AS [Value], D.UserName AS [Text] From HKP.Designation AS D Order By UserName");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion EmployeeInformation
    }
}