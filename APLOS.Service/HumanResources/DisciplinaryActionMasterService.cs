#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.HumanResources;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Reflection;

#endregion Using

namespace Library.Service.HumanResources
{
    public class DisciplinaryActionMasterService : Service<DisciplinaryActionMaster>, IDisciplinaryActionMasterService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public DisciplinaryActionMasterService(
            IRepositoryAsync<DisciplinaryActionMaster> DisciplinaryActionMasterRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(DisciplinaryActionMasterRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
        }

        #endregion Constructor

        public string GetPK()
        {
            return GetAutoNumber(nameof(DisciplinaryActionMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(DisciplinaryActionMaster entity)
        {
            try
            {
                entity.Id = "DAM-" + GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public override void Update(DisciplinaryActionMaster entity)
        {
            try
            {
                //Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string empSystemId)
        {
            try
            {
                parameters.CmdText = @"SELECT DAM.*
                                       	,EI.EmployeeName ResponsiblePersonName
                                       	,E.EmployeeName InvestigatorName
                                        ,EID.EmployeeName
                                       	,DA.UserName Action
                                       	,DAC.UserName Criticality
                                        ,E.EmpPicPath
                                        ,E.DOJ
										,E.DOC
                                       FROM [TRN].[DisciplinaryActionMaster] DAM
                                       LEFT JOIN HKP.DisciplinaryAction DA ON DAM.ActionId = DA.Id
                                       LEFT JOIN HKP.DisciplinaryActionCriticality DAC ON DAM.ActionCriticalityId = DAC.Id
                                       LEFT JOIN dbo.EmployeeInformation E ON DAM.InvestigatorId = E.SystemId
                                       LEFT JOIN dbo.EmployeeInformation EI ON DAM.ResponsiblePersonId = EI.SystemId
                                       LEFT JOIN dbo.EmployeeInformation EID ON DAM.EmpSystemId= EID.SystemId
                                       WHERE DAM.EmpSystemId='" + empSystemId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel GetEmployeeData(GridParameter parameters, string plantId, string empId)
        {
            try
            {
                parameters.CmdText = @"SELECT E.SystemId
							    	,E.PlantId
							    	,E.GroupID
							    	,E.CompanyId
							    	,E.EmployeeName
							    	,E.BudgetCode
							    	,PR.UserName PositionName
							    	,E.TelePhnNo
							    	,E.EmailId
							    	,E.EmpType
							    	,E.GivenDesignationId
							    	,EN.UserName EntityName
							    	,D.UserName Designation
							    	,DEPT.UserName AS Department
									,E.EmployeeCode
									,E.EmpPicPath
							    FROM EmployeeInformation E
							    LEFT JOIN MST.ManpowerBudget PMB ON E.BudgetCode = PMB.Id
							    LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
							    LEFT JOIN ORG.Department DEPT ON E.DepartmentId = DEPT.Id
							    LEFT JOIN ORG.Entity EN ON PMB.EntityId = EN.Id
							    LEFT JOIN HKP.Designation D ON PR.DesignationId = D.Id
							    WHERE E.EmployeeStatus = 'Active' AND E.IsApproved=1  AND E.PlantId = '" + plantId + @"' AND SystemId<> '" + empId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void Delete(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                base.Delete(Id);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }
    }
}