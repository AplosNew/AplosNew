#region Using

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

#endregion Using

namespace Library.Service.Products
{
    /// <summary>
    ///  Class ProductCategoryService.
    /// </summary>
    public partial class ProductCategoryService : Service<ProductCategory>, IProductCategoryService
    {
        #region Constructor

        private readonly ICompanyGroupWiseProductCategoryService _companyGroupWiseProductCategoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ProductCategoryService(
            IRepositoryAsync<ProductCategory> productCategoryRepository,
            ICompanyGroupWiseProductCategoryService companyGroupWiseProductCategoryService,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(productCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _companyGroupWiseProductCategoryService = companyGroupWiseProductCategoryService;
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
            catch
            {
                return 1.00M;
            }
        }

        public override void Insert(ProductCategory entity)
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
                    entity.Id = "PC-" + pkId;
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
                    CompanyGroupWiseProductCategory comgroupProductCategory = new CompanyGroupWiseProductCategory
                    {
                        Id = "CPC-" + pkId,
                        ProductCategoryId = entity.Id,
                        CompanyGroupId = identity.CompanyGroupId,
                        Active = true,
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(comgroupProductCategory);
                    _companyGroupWiseProductCategoryService.InsertOrUpdateGraph(comgroupProductCategory);
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
            return GetAutoNumber(nameof(ProductCategory), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUnique(ProductCategory entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id && !r.Archive);
        }

        public override void Update(ProductCategory entity)
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
                base.Archive(key);

                CompanyGroupWiseProductCategory comop = new CompanyGroupWiseProductCategory();
                comop = _companyGroupWiseProductCategoryService.FindbyFKId(key);
                _companyGroupWiseProductCategoryService.Archive(comop.Id);
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
                parameters.CmdText = $"SELECT pc.* FROM {DbSchema.HKP}.[{DbTable.ProductCategory}] AS pc " +
                                     $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseProductCategory}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') cgpc " +
                                     $"ON pc.Id = cgpc.ProductCategoryId  WHERE ISNULL(cgpc.Id, '')<> '' AND  pc.Archive=0 ";
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
                string _sql = $"SELECT pc.Id AS Value, pc.UserName as Text FROM {DbSchema.HKP}.[{DbTable.ProductCategory}] AS pc " +
                                     $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseProductCategory}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') cgpc " +
                                     $"ON pc.Id = cgpc.ProductCategoryId  WHERE ISNULL(cgpc.Id, '')<> '' AND pc.Active=1 AND  pc.Archive=0 ORDER BY pc.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}