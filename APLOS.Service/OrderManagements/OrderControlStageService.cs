#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.OrderManagements
{
    public partial class OrderControlStageService : Service<OrderControlStage>, IOrderControlStageService
    {
        #region Constructor
        private readonly IRepository<OrderControlStage> _repOrderControlStage;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;

        public OrderControlStageService(
            IRepositoryAsync<OrderControlStage> repOrderControlStage,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(repOrderControlStage, unitOfWork)
        {
            _repOrderControlStage = repOrderControlStage;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Insert

        public override void Insert(OrderControlStage entity)
        {
            try
            {
                entity.Id = GetAutoId();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #endregion Insert

        #region GetAutoId

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(OrderControlStage), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        #endregion GetAutoId

        #region Update

        public override void Update(OrderControlStage entity)
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
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #endregion Update

        #region GetAutoSequence

        public decimal GetAutoSequence()
        {
            return _repOrderControlStage.Query().Select().Max(r => r.Sequence + 1);
        }

        #endregion GetAutoSequence

        #region Query

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets all items in this collection. </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------

        public override IQueryFluent<OrderControlStage> Query()
        {
            return base.Query(r => !r.IsArchive);
        }

        #endregion Query

        #region GetOrderControlStageList

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Query all Budget Sub Category list for ddl. </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the order control stage lists in this
        /// collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------

        public IEnumerable<object> GetOrderControlStageList()
        {
            try
            {
                return from m in _repOrderControlStage.Query(m => !m.IsArchive)
                       select new { Text = m.StandardName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #endregion GetOrderControlStageList
    }

    public partial class OrderControlStageService : IOrderControlStageService
    {
        private readonly ISqlRepository _sqlRepository;

        public virtual GridModel GetSearchData(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT Id, Sequence, Code, ShortName, StandardName, UserName, Description, Remarks, Active as IsActive, Archive as IsArchive,ADDEDBY, ADDEDDATE, ADDEDFROMIP, UPDATEBY, UPDATEDDATE, UPDATEDFROMIP " +
                          $"FROM {DbSchema.HKP}.[OrderControlStage] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public virtual Dictionary<string, object> GetModelData(object id)
        {
            try
            {
                var sql = $"SELECT Id, Sequence, Code, ShortName, StandardName, UserName, Description, Remarks, Active as IsActive, Archive as IsArchive,ADDEDBY As AddedBy, CONVERT(VARCHAR(10),ADDEDDATE,6) As AddedDate, ADDEDFROMIP As AddedFromIP, UPDATEBY, UPDATEDDATE, UPDATEDFROMIP " +
                          $"FROM  { DbSchema.HKP}.[OrderControlStage] WHERE Id = @0";
                return _sqlRepository.GetData(sql, id);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                  Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }
    }
}