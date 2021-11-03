#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Machines;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Machines
{
    public partial class MachineCategoryService : Service<MachineCategory>, IMachineCategoryService
    {
        #region Constructor

        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IUnitOfWork _unitOfWork;

        public MachineCategoryService(
            IRepositoryAsync<MachineCategory> machineRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(machineRepository, unitOfWork)
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

        public override IQueryFluent<MachineCategory> Query()
        {
            return base.Query(r => !r.Archive);
        }

        #endregion Query

        #region GetmachineCategoryList

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets the budget master lists in this collection. </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the budget master lists in this
        /// collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public IEnumerable<object> GetmachineCategoryList()
        {
            try
            {
                return from m in base.Query(r => r.Active && !r.Archive).Select().OrderBy(r => r.Sequence)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM  [HKP].[{DbTable.MachineCategory}] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        #endregion GetmachineCategoryList

        #region InsertUpdate

        /// <summary>
        /// CompanyFYPeriod Insert.
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(MachineCategory entity)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetAutoId();
                base.Insert(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        private string GetAutoId()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(MachineCategory), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(MachineCategory entity)
        {
            try
            {
                AuditService.Log(entity);
                base.Update(entity);
                _unitOfWork.SaveChanges();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                entity.AddedBy, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        #endregion InsertUpdate
    }

    public partial class MachineCategoryService : IMachineCategoryService
    {
        private readonly ISqlRepository _sqlRepository;

        public virtual GridModel GetSearchData(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM  [HKP].[{DbTable.MachineCategory}] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public virtual Dictionary<string, object> GetModelData(object id)
        {
            try
            {
                var sql = $"SELECT * FROM  [HKP].[{DbTable.MachineCategory}] WHERE Id=@0";
                return _sqlRepository.GetData(sql, id);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }
    }
}