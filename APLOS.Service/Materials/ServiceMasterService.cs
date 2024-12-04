#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.ManagementChartOfAccounts;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Materials
{
    public class ServiceMasterService : Service<ServiceMaster>, IServiceMasterService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<EmployeeInformation> _EmployeeInformationRepository;

        public ServiceMasterService(
            IRepositoryAsync<ServiceMaster> projectPlanningCategoryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<EmployeeInformation> EmployeeInformationRepository
            )
            : base(projectPlanningCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _EmployeeInformationRepository = EmployeeInformationRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select(t => t.Sequence).Max() + 1;
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ServiceMaster), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(ServiceMaster entity)
        {
            try
            {
                CheckUnique(entity);
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public override void Update(ServiceMaster entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public void Delete(string id)
        {
            try
            {
                var data = base.Find(id);
                base.Delete(id);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        private void CheckUnique(ServiceMaster entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Code == entity.Code && t.Id != entity.Id && t.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.UserName == entity.UserName && t.Id != entity.Id && t.Active);
        }

        public GridModel Query(GridParameter parameters, string[] ids)
        {
            try
            {
                parameters.CmdText = @"SELECT A.Id, A.ServiceGroupId, B.UserName AS ServiceGroupName,A.TransactionUoMId, A.[Sequence], A.Code, A.UserName, A.StandardName, A.[Description], A.Remarks, A.Active,A.HSNCodeId,A.CompanyId
                            FROM [HKP].[ServiceMaster] AS A JOIN [HKP].[ServiceGroup] AS B ON A.ServiceGroupId=B.Id WHERE A.Id NOT IN (" + ReturnStringArray(ids) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public GridModel QueryServiceMaster(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT A.Id, A.ServiceGroupId, B.UserName AS ServiceGroupName, A.[Sequence], A.Code, A.UserName, A.StandardName, A.[Description], A.Remarks, A.Active,A.HSNCodeId
                            FROM [HKP].[ServiceMaster] AS A JOIN [HKP].[ServiceGroup] AS B ON A.ServiceGroupId=B.Id WHERE A.Id NOT IN (Select ServiceMasterId from HKP.CompanyServiceMaster)";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }
        public GridModel GetCboEmployeeBudgetWithServiceMasterPopUpList(GridParameter parameters, string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId)) return null;
            var employee = _EmployeeInformationRepository.Find(employeeId);
            parameters.CmdText = @"SELECT distinct BM.*, RP.MappingLevel, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, B.UserName AS BudgetName, BG.UserName AS BudgetGroup, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,SM.ServiceMasterId,SMU.UserName ServiceMaster
                                        FROM [MST].[EmployeeResponsiblePerson] AS RP
                                        JOIN [MST].[BudgetMaster] AS BM ON BM.Id=RP.BudgetMasterId
                                        JOIN (SELECT SMGL.ServiceMasterId,BMA.BudgetMasterId
													FROM HKP.ServiceMasterGL SMGL 
													LEFT JOIN MST.BudgetMasterActivity BMA ON BMA.Id=SMGL.DrControlId) AS SM ON SM.BudgetMasterId=RP.BudgetMasterId
                                        JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
							            LEFT JOIN [HKP].[BudgetCategory] AS BC ON BC.Id=BM.BudgetCategoryId
							            LEFT JOIN [HKP].[BudgetSubCategory] AS BSC ON BSC.Id=BM.BudgetSubCategoryId
                                        LEFT JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        LEFT JOIN [HKP].[ServiceMaster] AS SMU on SMU.Id = SM.ServiceMasterId
                                        WHERE RP.EmployeeId='" + employee.SystemId + @"' AND RP.SourceType='" + ResponsiblePersonSourceType.BudgetMaster + @"'
                                        UNION
                                        SELECT distinct BM.*, NULL MappingLevel, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, B.UserName AS BudgetName, BG.UserName AS BudgetGroup,GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,SM.ServiceMasterId,SMU.UserName ServiceMaster
                                        FROM [MST].[ManpowerBudgetResponsiblePerson] AS RP
                                        JOIN [MST].[BudgetMaster] AS BM ON BM.Id=RP.BudgetMasterId
                                        JOIN (SELECT SMGL.ServiceMasterId,BMA.BudgetMasterId
													FROM HKP.ServiceMasterGL SMGL 
													LEFT JOIN MST.BudgetMasterActivity BMA ON BMA.Id=SMGL.DrControlId) AS SM ON SM.BudgetMasterId=RP.BudgetMasterId
                                        JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
							            LEFT JOIN [HKP].[BudgetCategory] AS BC ON BC.Id=BM.BudgetCategoryId
							            LEFT JOIN [HKP].[BudgetSubCategory] AS BSC ON BSC.Id=BM.BudgetSubCategoryId
										LEFT JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        LEFT JOIN [HKP].[ServiceMaster] AS SMU on SMU.Id = SM.ServiceMasterId
                                        WHERE RP.ManpowerBudgetId='" + employee.BudgetCode + @"' AND RP.SourceType='" + ResponsiblePersonSourceType.BudgetMaster + @"'
                                        UNION
                                       SELECT distinct BM.*, NULL MappingLevel, BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory, B.UserName AS BudgetName, BG.UserName AS BudgetGroup,GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName,SM.ServiceMasterId,SMU.UserName ServiceMaster
                                        FROM [ORG].[PositionResponsiblePerson] AS RP
                                        JOIN [MST].[BudgetMaster] AS BM ON BM.Id=RP.BudgetMasterId
                                        JOIN (SELECT SMGL.ServiceMasterId,BMA.BudgetMasterId
													FROM HKP.ServiceMasterGL SMGL 
													LEFT JOIN MST.BudgetMasterActivity BMA ON BMA.Id=SMGL.DrControlId) AS SM ON SM.BudgetMasterId=RP.BudgetMasterId
                                        JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
							            LEFT JOIN [HKP].[BudgetCategory] AS BC ON BC.Id=BM.BudgetCategoryId
							            LEFT JOIN [HKP].[BudgetSubCategory] AS BSC ON BSC.Id=BM.BudgetSubCategoryId
										LEFT JOIN [HKP].[BudgetGroup] AS bg on BM.BudgetGroupId = bg.Id
                                        LEFT JOIN [HKP].[ServiceMaster] AS SMU on SMU.Id = SM.ServiceMasterId
                                        WHERE RP.PositionId='" + employee.BudgetCode + @"' AND RP.SourceType='" + ResponsiblePersonSourceType.BudgetMaster + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        public IEnumerable<object> GetBudgetMasterActivityWithServiceMasterCbo(string budgetMasterId, string level, string employeeId)
        {
            if (level == "Activity")
            {
                var sql = @"SELECT DISTINCT BMA.BudgetMasterId, BMA.ActivityId, A.UserName AS ActivityName, A.FALinked, A.ActivityType,A.IsOrderSpecific,BMA.IsServiceApplicable,BMA.ActivityOrderType,SMU.UserName ServiceMaster
                        FROM [MST].[BudgetMasterActivity] AS BMA
                        JOIN [HKP].[ServiceMasterGL] AS SMGL ON BMA.Id=SMGL.DrControlId
                        JOIN [HKP].[Activity] AS A ON A.Id=BMA.ActivityId
                        JOIN [MST].[EmployeeResponsiblePerson] AS ERP ON ERP.BudgetMasterActivityId=BMA.Id
                        LEFT JOIN [HKP].[ServiceMaster] AS SMU on SMU.Id = SMGL.ServiceMasterId
                        WHERE BMA.Active=1 AND BMA.BudgetMasterId='" + budgetMasterId + "' AND ERP.EmployeeId='" + employeeId + @"'"; 
                return _sqlRepository.GetDataCollection(sql);
            }
            else
            {
                var sql = @"SELECT BMA.BudgetMasterId, BMA.ActivityId, A.UserName AS ActivityName, A.FALinked, A.ActivityType,A.IsOrderSpecific,BMA.ActivityOrderType,BMA.IsServiceApplicable,SMU.UserName ServiceMaster
                        FROM [MST].[BudgetMasterActivity] AS BMA
                        JOIN [HKP].[ServiceMasterGL] AS SMGL ON BMA.Id=SMGL.DrControlId
                        JOIN [HKP].[Activity] AS A ON A.Id=BMA.ActivityId
                        LEFT JOIN [HKP].[ServiceMaster] AS SMU on SMU.Id = SMGL.ServiceMasterId
                        WHERE BMA.Active=1 AND BMA.BudgetMasterId='" + budgetMasterId + "' ORDER BY A.Code, A.UserName";
                return _sqlRepository.GetDataCollection(sql);
            }
        }
    }
}