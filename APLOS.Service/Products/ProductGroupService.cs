using Library.Core;
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

namespace Library.Service.Products
{
    public partial class ProductGroupService : Service<ProductGroup>, IProductGroupService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public ProductGroupService(
            IRepositoryAsync<ProductGroup> productGroupRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(productGroupRepository, unitOfWork)
        {
            _pkGeneratorService = pkGeneratorService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region GetAutoSequence

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Query auto sequence number. </summary>
        /// <returns>   decimal. </returns>
        ///-------------------------------------------------------------------------------------------------
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

        #endregion GetAutoSequence

        #region Query

        public override IQueryFluent<ProductGroup> Query()
        {
            return base.Query(r => !r.IsArchive);
        }

        #endregion Query

        #region GetProductGroupList

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the budget master lists in this collection. </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the budget master lists in this
        /// collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------

        public IEnumerable<object> GetProductGroupList()
        {
            try
            {
                return from m in base.Query(r => r.IsActive && !r.IsArchive).Select().OrderBy(r => r.Sequence)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #endregion GetProductGroupList

        #region InsertUpdate

        /// <summary>
        /// CompanyFYPeriod Insert.
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(ProductGroup entity)
        {
            try
            {
                entity.Id = GetAutoId();
                base.Insert(entity);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(ProductGroup), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(ProductGroup entity)
        {
            try
            {
                AuditService.Log(entity);
                base.Update(entity);
                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #endregion InsertUpdate
    }

    public partial class ProductGroupService : IProductGroupService
    {
        private readonly ISqlRepository _sqlRepository;

        public virtual GridModel GetSearchData(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT Id, Sequence, Code, ShortName, StandardName, UserName, Description, Remarks, Active as IsActive, Archive as IsArchive,ADDEDBY, ADDEDDATE, ADDEDFROMIP, UPDATEBY, UPDATEDDATE, UPDATEDFROMIP " +
                          $"FROM {DbSchema.HKP}.[{DbTable.ProductGroup}] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public virtual Dictionary<string, object> GetModelData(object id)
        {
            try
            {
                var sql = $"SELECT Id, Sequence, Code, ShortName, StandardName, UserName, Description, Remarks, Active as IsActive, Archive as IsArchive,ADDEDBY As AddedBy, CONVERT(VARCHAR(10),ADDEDDATE,6) As AddedDate, ADDEDFROMIP As AddedFromIP, UPDATEBY, UPDATEDDATE, UPDATEDFROMIP " +
                    $"FROM  { DbSchema.HKP}.[{DbTable.ProductGroup}] WHERE Id = @0";
                return _sqlRepository.GetData(sql, id);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
    }
}