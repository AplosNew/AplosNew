#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Employees
{
    public class AuthorizationConfigService : Service<AuthorizationConfig>, IAuthorizationConfigService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public AuthorizationConfigService(
            IRepositoryAsync<AuthorizationConfig> AuthorizationConfigRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(AuthorizationConfigRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(AuthorizationConfig), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public bool CheckDuplicateEmployee(string Id, string EmpSystemId, string action)
        {
            try
            {
                var _sql = @"SELECT * FROM AuthorizationConfig WHERE ActionStatus='"+ action + "' and EmployeeId='"+ EmpSystemId + "' AND Id<>'"+Id+"' ";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override void Insert(AuthorizationConfig entity)
        {
            try
            {
                var check = CheckDuplicateEmployee(entity.Id, entity.EmployeeId, entity.ActionStatus);
                if (check)
                {
                    entity.Id = GetPK();
                    base.Insert(entity);
                }
                else
                {
                    throw new CustomException("Selected Employee "+ entity.EmployeeId + " already exists for "+ entity.ActionStatus + "");
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "AuthorizationConfig Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                AuthorizationConfig entity = Find(id);
                // If section row inactive
                base.DeleteGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<object> Query(string CompanyId,string PlantId, string actionStatus)
        {
            try
            {
               
               string CmdText = @"SELECT SSU.Id,SSU.ActionStatus, SSU.EmployeeId,EI.EmployeeName,EI.CompanyId,EI.PlantId,EI.GroupId,EI.EmployeeCode
                                      ,EI.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                      PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                                      ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,EI.EmpPicPath,EI.EmployeeStatus,C.UserName Company
									  FROM dbo.AuthorizationConfig SSU
									  LEFT JOIN dbo.EmployeeInformation EI on ei.SystemId=ssu.EmployeeId
                                      LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                      LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                      LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                      LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                      LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                      LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                      LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                      LEFT JOIN ORG.Plant PL ON PL.Id=EI.PlantId
                                      LEFT JOIN ORG.Company C ON C.Id=EI.CompanyId
                                      LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                      LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id
                                      LEFT JOIN HKP.LegalDesignation LDEG ON EI.LegalDesignationId=LDEG.Id
									  WHERE SSU.ActionStatus='" + actionStatus + "' ORDER BY EI.EmployeeStatus";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetCbo(string status,string plantId)
        {
            try
            {
                var sql = @"SELECT AC.EmployeeId Id,EI.EmployeeName AS Value FROM [dbo].[AuthorizationConfig] AC  
						LEFT JOIN dbo.EmployeeInformation AS EI ON EI.SystemId=AC.EmployeeId
						WHERE AC.ActionStatus='" + status + "' AND EI.EmployeeStatus='Active'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetAllEmployeeData()
        {
            try
            {
               string  CmdText = @"SELECT CAST (0 AS bit) Flag,E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,PMB.Code BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
                                    ,PR.DepartmentId
                                    ,PR.DivisionId
									,PR.SectionId
							    	,E.EmpType
							    	,E.GivenDesignationId
									,E.EmployeeCategorySystemID EmployeeCategoryId
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,GD.UserName GivenDesignation
                                    ,LD.UserName LegalDesignation
							    	,DEPT.UserName AS Department
							    	,DV.UserName AS Division
									,SC.UserName AS Section
                                    ,E.EmployeeCode
									,E.EmpPicPath
                                    ,E.DOJ
                                    ,P.UserName Plant
									,SS.UserName SubSection
                                    ,E.EmployeeCodeNumeric
                                    ,C.UserName Company
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Division DV ON PR.DivisionId = DV.Id
							    LEFT JOIN ORG.Section SC ON PR.SectionId = SC.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    LEFT JOIN HKP.Designation GD ON E.GivenDesignationId = GD.Id
                                LEFT JOIN HKP.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                LEFT JOIN ORG.Plant P ON P.Id=E.PlantId
								LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                LEFT JOIN ORG.Company C ON C.Id=E.CompanyId
                                WHERE E.EmployeeStatus='Active' AND E.EmpType<>'Guest' Order by EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
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