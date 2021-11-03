using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.Products
{
    public class ProductSubCategoryService : Service<ProductSubCategory>, IProductSubCategoryService
    {
        #region Constructor

        private readonly ICompanyGroupWiseProductSubCategoryService _companyGroupWiseProductSubCategoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ProductSubCategoryService(
            IRepositoryAsync<ProductSubCategory> ProductSubCategoryRepository,
            ICompanyGroupWiseProductSubCategoryService companyGroupWiseProductSubCategoryService,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(ProductSubCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _companyGroupWiseProductSubCategoryService = companyGroupWiseProductSubCategoryService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query(r => !r.Archive).Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        public override void Insert(ProductSubCategory entity)
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
                    CompanyGroupWiseProductSubCategory comgroupProductSubCategory = new CompanyGroupWiseProductSubCategory
                    {
                        Id = "CPSC-" + pkId,
                        ProductSubCategoryId = entity.Id,
                        CompanyGroupId = identity.CompanyGroupId,
                        Active = true,
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(comgroupProductSubCategory);
                    _companyGroupWiseProductSubCategoryService.InsertOrUpdateGraph(comgroupProductSubCategory);
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

        private void CheckUnique(ProductSubCategory entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id && !r.Archive);
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProductSubCategory), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(ProductSubCategory entity)
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

        public override void Archive(string key)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                ProductSubCategory entity = Find(key);
                base.Archive(entity.Id);

                CompanyGroupWiseProductSubCategory comop = new CompanyGroupWiseProductSubCategory();
                comop = _companyGroupWiseProductSubCategoryService.FindbyFKId(key);
                _companyGroupWiseProductSubCategoryService.Archive(comop.Id);
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
                parameters.CmdText = $"SELECT psc.* FROM {DbSchema.HKP}.[{DbTable.ProductSubCategory}] AS psc " +
                                     $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseProductSubCategory}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') cgpsc " +
                                     $"ON psc.Id = cgpsc.ProductSubCategoryId  WHERE ISNULL(cgpsc.Id, '')<> '' AND  psc.Archive=0 ";
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
                string _sql = $"SELECT pc.Id AS Value, pc.UserName as Text FROM {DbSchema.HKP}.[{DbTable.ProductSubCategory}] AS pc " +
                                     $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseProductSubCategory}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') cgpc " +
                                     $"ON pc.Id = cgpc.ProductSubCategoryId  WHERE ISNULL(cgpc.Id, '')<> '' AND pc.Active=1 AND  pc.Archive=0 ORDER BY pc.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}