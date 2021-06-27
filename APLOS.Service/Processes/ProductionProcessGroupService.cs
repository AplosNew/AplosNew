#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Processes;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Processes
{
    public class ProductionProcessGroupService : Service<ProductionProcessGroup>, IProductionProcessGroupService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ProductionProcessGroupService(
            IRepositoryAsync<ProductionProcessGroup> ProductionProcessGroupRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(ProductionProcessGroupRepository, unitOfWork)
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



        #region GetDayList

        public GridModel GetCbo(string companyGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT B.Id Value,B.UserName Text FROM [HKP].[ProductionProcessGroup] B
                            WHERE  B.Archive =0 AND B.Active=1 AND B.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
        }

        #endregion GetDayList

        #region InsertUpdate

        /// <summary>
        /// CompanyFYPeriod Insert.
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(ProductionProcessGroup entity)
        {
            var flag = false;
            try
            {
                CheckUnique(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetAutoId();
                entity.CompanyGroupId = identity.CompanyGroupId;
                InsertGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(ProductionProcessGroup), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(ProductionProcessGroup entity)
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
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
        }

        #endregion InsertUpdate

        private void CheckUnique(ProductionProcessGroup entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Id!=entity.Id && t.Code == entity.Code && !t.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.Id != entity.Id && t.UserName == entity.UserName && !t.Archive);
        }

        public void DeleteGraph(string key)
        {
            var flag = false;

            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                ProductionProcessGroup ProductionProcessGroup = Find(key);
                base.DeleteGraph(ProductionProcessGroup);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                    null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, nameof(Setups)));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters.CmdText = @"SELECT B.* FROM [HKP].[ProductionProcessGroup] B
                                        WHERE B.Archive =0 AND B.Active=1 AND B.CompanyGroupId='" + companyGroupId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }
    }
}