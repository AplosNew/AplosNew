#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public partial class SeparationTypeService : Service<SeparationType>, ISeparationTypeService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public SeparationTypeService(
            IRepositoryAsync<SeparationType> itemRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(itemRepository, unitOfWork)
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

        public override IQueryFluent<SeparationType> Query()
        {
            return base.Query(r => !r.IsArchive);
        }

        #endregion Query

        #region GetSeparationTypeList

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the budget master lists in this collection. </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the budget master lists in this
        /// collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------

        public IEnumerable<object> GetSeparationTypeList()
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

        #endregion GetSeparationTypeList

        #region InsertUpdate

        /// <summary>
        /// CompanyFYPeriod Insert.
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(SeparationType entity)
        {
            try
            {
                entity.Id = GetAutoId();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(SeparationType), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(SeparationType entity)
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

    public partial class SeparationTypeService : ISeparationTypeService
    {
        private readonly ISqlRepository _sqlRepository;

        

       
    }
}