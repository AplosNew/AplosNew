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
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.Projects
{
    public partial class ProjectPlanningSubCategoryService : Service<ProjectPlanningSubCategory>, IProjectPlanningSubCategoryService
    {
        #region Constructor

        private readonly ICompanyGroupWiseProjectPlanningSubCategoryService _companyGroupWiseProjectPlanningSubCategoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ProjectPlanningSubCategoryService(
            IRepositoryAsync<ProjectPlanningSubCategory> ProjectPlanningSubCategoryRepository,
            ICompanyGroupWiseProjectPlanningSubCategoryService companyGroupWiseProjectPlanningSubCategoryService,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(ProjectPlanningSubCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _companyGroupWiseProjectPlanningSubCategoryService = companyGroupWiseProjectPlanningSubCategoryService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        public override void Insert(ProjectPlanningSubCategory entity)
        {
            var flag = false;
            var isInsert = false;
            string pkId = GetPK();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                CheckUnique(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    isInsert = true;
                    entity.Id = "PSC-" + pkId;
                    entity.ModelState = ModelState.Added;
                    AuditService.Log(entity);
                }
                else
                {
                    entity.ModelState = ModelState.Modified;
                    AuditService.Log(entity);
                }
                InsertOrUpdateGraph(entity);
                if (isInsert)
                {
                    CompanyGroupWiseProjectPlanningSubCategory comgroupProjectPlanningSubCategory = new CompanyGroupWiseProjectPlanningSubCategory
                    {
                        Id = "CPSC-" + pkId,
                        ProjectPlanningSubCategoryId = entity.Id,
                        CompanyGroupId = identity.CompanyGroupId,
                        Active = true,
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(comgroupProjectPlanningSubCategory);
                    _companyGroupWiseProjectPlanningSubCategoryService.InsertOrUpdateGraph(comgroupProjectPlanningSubCategory);
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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

        private void CheckUnique(ProjectPlanningSubCategory entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id);
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProjectPlanningSubCategory), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(ProjectPlanningSubCategory entity)
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

        public void Delete(string key)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                CompanyGroupWiseProjectPlanningSubCategory comop = _companyGroupWiseProjectPlanningSubCategoryService.FindbyFKId(key);
                _companyGroupWiseProjectPlanningSubCategoryService.Delete(comop.Id);
                ProjectPlanningSubCategory entity = Find(key);
                base.Delete(entity.Id);
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

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PPSC.* FROM HKP.ProjectPlanningSubCategory AS PPSC
                                        left outer join(SELECT * FROM HKP.CompanyGroupWiseProjectPlanningSubCategory WHERE CompanyGroupId = '" + identity.CompanyGroupId + @"') cgpsc
                                        ON PPSC.Id = cgpsc.ProjectPlanningSubCategoryId  WHERE ISNULL(cgpsc.Id, '')<> '' ";
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
                string _sql = @"SELECT PPSC.Id AS Value, PPSC.UserName as Text FROM HKP.ProjectPlanningSubCategory AS PPSC
                                left outer join(SELECT * FROM HKP.CompanyGroupWiseProjectPlanningSubCategory WHERE CompanyGroupId = '" + identity.CompanyGroupId + @"') cgpc
                                ON PPSC.Id = cgpc.ProjectPlanningSubCategoryId  WHERE ISNULL(cgpc.Id, '')<> '' AND PPSC.Active=1  ORDER BY PPSC.UserName ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}