#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Projects;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Projects
{
    /// <summary>
    ///  Class ProductService.
    /// </summary>
    public partial class ProjectPlanningService : Service<ProjectPlanning>, IProjectPlanningService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IProjectPlanningDetailService _projectPlanningDetailService;
        private readonly IProjectPlanningMachineTypeService _projectPlanningFixedAssetService;
        private readonly IProjectPlanningMaterialMasterService _projectPlanningMaterialService;

        public ProjectPlanningService(
            IRepositoryAsync<ProjectPlanning> projectPlanningRepository
            , IProjectPlanningDetailService projectPlanningDetailService
            , IProjectPlanningMachineTypeService projectPlanningFixedAssetService
            , IProjectPlanningMaterialMasterService projectPlanningMaterialService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _projectPlanningDetailService = projectPlanningDetailService;
            _projectPlanningFixedAssetService = projectPlanningFixedAssetService;
            _projectPlanningMaterialService = projectPlanningMaterialService;
        }

        #endregion Constructor

        public string InsertAndUpdate(ProjectPlanning entity, IEnumerable<ProjectPlanningDetail> projectPlanningDetail, IEnumerable<ProjectPlanningMachineType> projectPlanningFixedAsset, IEnumerable<ProjectPlanningMaterialMaster> projectPlanningMaterial)
        {
            var flag = false;
            string pkId = GetPK();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = "PPC-" + pkId;
                    entity.CompanyId = identity.CompanyId;
                    entity.ModelState = ModelState.Added;
                    AuditService.Log(entity);
                }
                else
                {
                    entity.ModelState = ModelState.Modified;
                    AuditService.Log(entity);
                }
                InsertOrUpdateGraph(entity);
                //******ProjectPlanningDetail******//
                if (projectPlanningDetail != null)
                {
                    _projectPlanningDetailService.InsertOrUpdate(projectPlanningDetail, entity.Id);
                }
                //******ProjectPlanningFixedAsset******//
                if (projectPlanningFixedAsset != null)
                {
                    _projectPlanningFixedAssetService.InsertOrUpdate(projectPlanningFixedAsset, entity.Id);
                }
                //******ProjectPlanningMaterialMaster******//
                if (projectPlanningMaterial != null)
                {
                    _projectPlanningMaterialService.InsertOrUpdate(projectPlanningMaterial, entity.Id);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return entity.Id;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string Id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var data = Find(Id);
                if (data != null)
                {
                    _projectPlanningDetailService.DeleteWithMaster(Id);
                    base.DeleteGraph(data);
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

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProjectPlanning), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUnique(ProjectPlanning entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id);
        }

        public override void Update(ProjectPlanning entity)
        {
            try
            {
                CheckUnique(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PP.*,C.Code AS CurrencyName,E.UserName AS EntityName,  EI.EmployeeName,PM.UserName AS PositionName, PMB.Code AS ManpowerBudgetName FROM MST.ProjectPlanning AS PP
							LEFT OUTER JOIN [ORG].[Entity] AS E ON PP.EntityId = E.Id
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EI ON PP.EmployeeId=EI.SystemId
							LEFT OUTER JOIN [ORG].[Position]  AS PM ON PP.PositionId=PM.Id
							LEFT OUTER JOIN [MST].[ManpowerBudget] AS PMB ON PP.ManpowerBudgetId=PMB.Id
							LEFT OUTER JOIN [ORG].[Position]  AS PMPB ON PMB.PositionId=PMPB.Id
							LEFT OUTER JOIN [SCS].[Currency] C ON PP.CurrencyId = C.Id   ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PPC.Id AS Value, PPC.Title AS Text FROM MST.ProjectPlanning AS PPC
                                 WHERE PPC.CompanyId = '" + identity.CompanyId + @"'
                                 ORDER BY PPC.Title ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel FindById(GridParameter parameters, string id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"	SELECT PP.*,C.Code AS CurrencyName,PPR.Description AS RequisitionTitle,PPC.UserName AS ProjectPlanningCategoryName,ppsc.UserName AS ProjectPlanningSubCategoryName,EI.EmployeeName, PM.NickName AS PositionName, PMB.NickName AS ManpowerBudgetName FROM MST.ProjectPlanning AS PP
                            LEFT OUTER JOIN [MST].[ProjectPlanningDetail] AS PPD ON PP.Id=PPD.ProjectPlanningId
                            LEFT OUTER JOIN [MST].[ProjectPlanningRequisition] AS PPR ON PPR.ProjectPlanningId = PP.Id
	                        LEFT OUTER JOIN [HKP].[ProjectPlanningCategory] AS PPC ON PPD.ProjectPlanningCategoryId = PPC.Id
	                        LEFT OUTER JOIN [HKP].[ProjectPlanningSubCategory] AS PPSC ON PPD.ProjectPlanningSubCategoryId = PPSC.Id
                            LEFT OUTER JOIN [dbo].[EmployeeInformation] AS EI ON PP.EmployeeId=EI.SystemId
							LEFT OUTER JOIN [dbo].[EmployeeInformation] AS PM ON PP.EmployeeId=PM.SystemId
							LEFT OUTER JOIN [dbo].[EmployeeInformation] AS PMB ON PP.EmployeeId=PMB.SystemId
							LEFT OUTER JOIN [SCS].[Currency] C ON PP.CurrencyId = C.Id

                WHERE PP.Id='" + id + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetCompanyCurrencyCountryWise()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string _sql = @"SELECT CU.Code, C.BaseCurrencyId, CO.CurrencyId FROM ORG.Company AS C
                         INNER JOIN MST.AddressMaster AS AM ON AM.Id=C.AddressMasterId
                         INNER JOIN SCS.Country AS CO ON CO.Id=AM.CountryId
                         INNER JOIN SCS.Currency CU ON CU.Id=C.BaseCurrencyId
                        WHERE C.Id='" + identity.CompanyId + "' ";
            return _sqlRepository.GetDataCollection(_sql, null);
        }

        public IEnumerable<object> GetCoaIdByCompany()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string _sql = @"SELECT COM.COAId,C.UserName AS CoaName FROM [ORG].[Company] AS COM
                            LEFT OUTER JOIN [HKP].[COA] AS C ON COM.COAId = C.Id
                            WHERE COM.Id='" + identity.CompanyId + "' ";
            return _sqlRepository.GetDataCollection(_sql, null);
        }
    }
}