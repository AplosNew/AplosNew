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
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Projects
{
    /// <summary>
    ///  Class ProductCategoryService.
    /// </summary>
    public partial class ProjectPlanningCategoryService : Service<ProjectPlanningCategory>, IProjectPlanningCategoryService
    {
        #region Constructor

        private readonly ICompanyGroupWiseProjectPlanningCategoryService _companyGroupWiseProjectPlanningCategoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ProjectPlanningCategoryService(
            IRepositoryAsync<ProjectPlanningCategory> projectPlanningCategoryRepository,
            ICompanyGroupWiseProjectPlanningCategoryService companyGroupWiseProjectPlanningCategoryService,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(projectPlanningCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _companyGroupWiseProjectPlanningCategoryService = companyGroupWiseProjectPlanningCategoryService;
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

        public override void Insert(ProjectPlanningCategory entity)
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
                    entity.Id = "PPC-" + pkId;
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
                    CompanyGroupWiseProjectPlanningCategory comgroupProjectPlanningCategory = new CompanyGroupWiseProjectPlanningCategory
                    {
                        Id = "CPPC-" + pkId,
                        ProjectPlanningCategoryId = entity.Id,
                        CompanyGroupId = identity.CompanyGroupId,
                        Active = true,
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(comgroupProjectPlanningCategory);
                    _companyGroupWiseProjectPlanningCategoryService.InsertOrUpdateGraph(comgroupProjectPlanningCategory);
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

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProjectPlanningCategory), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUnique(ProjectPlanningCategory entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id);
        }

        public override void Update(ProjectPlanningCategory entity)
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

                CompanyGroupWiseProjectPlanningCategory comop = _companyGroupWiseProjectPlanningCategoryService.FindbyFKId(key);
                _companyGroupWiseProjectPlanningCategoryService.Delete(comop.Id);
                base.Delete(key);
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
                parameters.CmdText = @"SELECT PPC.* FROM HKP.ProjectPlanningCategory AS PPC
                                         LEFT OUTER JOIN (SELECT * FROM HKP.CompanyGroupWiseProjectPlanningCategory WHERE CompanyGroupId = '" + identity.CompanyGroupId + @"') cgpc
                                         ON PPC.Id = cgpc.ProjectPlanningCategoryId  WHERE ISNULL(cgpc.Id, '')<> '' ";
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
                string _sql = @"SELECT PPC.Id AS Value, PPC.UserName AS Text FROM HKP.ProjectPlanningCategory AS PPC
left outer join(SELECT * FROM HKP.CompanyGroupWiseProjectPlanningCategory WHERE CompanyGroupId = '" + identity.CompanyGroupId + @"') cgpc
 ON PPC.Id = cgpc.ProjectPlanningCategoryId  WHERE ISNULL(cgpc.Id, '')<> '' AND PPC.Active=1  ORDER BY PPC.UserName ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}