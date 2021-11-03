#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Inventory
{
    public class SFGMovementService : Service<SFGMovement>, ISFGMovementService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public SFGMovementService(
            IRepositoryAsync<SFGMovement> SFGMovementRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(SFGMovementRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor
        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT S.* FROM [MST].[SFGMovement] S";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetUserSFGMovementList(string userId)
        {
            try
            {
                var sql = @"SELECT UG.Id, UG.UserId, UG.SFGInventoryId, PG.Code, PG.[Sequence], PG.ShortName, PG.StandardName, PG.UserName
                            FROM [SEC].[UserSFGInventory] AS UG
                            JOIN [HKP].[SFGInventory] AS PG ON UG.SFGInventoryId=PG.Id
                            WHERE UG.UserId='" + userId + "' ORDER BY PG.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Securities.ToString()));
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query().Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public decimal GetAutoSequence()
        {
            try
            {
                return Query().Select().Max(r => r.Sequence + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(SFGMovement), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(SFGMovement entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.Active);
        }
       
        public override void Insert(SFGMovement entity)
        {
            try
            {
                if (entity != null)
                {
                    Check(entity);
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = GetPK();
                        base.Insert(entity);
                    }
                }
                else
                    throw new CustomException("Incomplete data.");

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
           
        }

        public override void Update(SFGMovement entity)
        {
            try
            {
                Check(entity);
                var dbdata = Find(entity.Id);
                if (dbdata == null || string.IsNullOrEmpty(dbdata.Id))
                    throw new CustomException("The record no longer exists.");
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }
    }
}