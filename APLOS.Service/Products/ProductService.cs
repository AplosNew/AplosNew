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
    public partial class ProductService : Service<Product>, IProductService
    {
        #region Constructor

        private readonly ICompanyGroupWiseProductService _companyGroupWiseProductService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ProductService(
            IRepositoryAsync<Product> ProductRepository,
            ICompanyGroupWiseProductService companyGroupWiseProductService,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            )
            : base(ProductRepository, unitOfWork, pkGeneratorService)
        {
            _companyGroupWiseProductService = companyGroupWiseProductService;
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

        public override void Insert(Product entity)
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
                    entity.Id = "P-" + pkId;
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
                    CompanyGroupWiseProduct comgroupProduct = new CompanyGroupWiseProduct
                    {
                        Id = "CP-" + pkId,
                        ProductId = entity.Id,
                        CompanyGroupId = identity.CompanyGroupId,
                        Active = true,
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(comgroupProduct);
                    _companyGroupWiseProductService.InsertOrUpdateGraph(comgroupProduct);
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
            return GetAutoNumber(nameof(Product), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void CheckUnique(Product entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Code == entity.Code && r.Id != entity.Id && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.UserName == entity.UserName && r.Id != entity.Id && !r.Archive);
        }

        public override void Update(Product entity)
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
                Product entity = Find(key);
                base.Archive(entity.Id);

                CompanyGroupWiseProduct comop = new CompanyGroupWiseProduct();
                comop = _companyGroupWiseProductService.FindbyFKId(key);
                _companyGroupWiseProductService.Archive(comop.Id);
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
                parameters.CmdText = $"SELECT p.* FROM {DbSchema.HKP}.[{DbTable.Product}] AS p " +
                                     $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseProduct}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') cgp " +
                                     $"ON p.Id = cgp.ProductId  WHERE ISNULL(cgp.Id, '')<> '' AND  p.Archive=0 ";
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
                string _sql = $"SELECT P.Id AS Value, P.UserName as Text FROM {DbSchema.HKP}.[{DbTable.Product}] AS P " +
                                     $"left outer join(SELECT * FROM {DbSchema.HKP}.[{DbTable.CompanyGroupWiseProduct}] WHERE CompanyGroupId = '{identity.CompanyGroupId}') CGP " +
                                     $"ON P.Id = CGP.ProductId  WHERE ISNULL(CGP.Id, '')<> '' AND P.Active=1 AND P.Archive=0 ORDER BY P.UserName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}