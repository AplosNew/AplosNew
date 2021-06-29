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

namespace Library.MaterialManagement.Inventory
{
    public class SFGInventoryService : Service<SFGInventory>, ISFGInventoryService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public SFGInventoryService(
            IRepositoryAsync<SFGInventory> SFGInventoryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(SFGInventoryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor
        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT S.*,'' AS Flag, P.UserName OwnerProcess FROM [HKP].[SFGInventory] S
                                        LEFT JOIN HKP.Process P ON P.Id=S.ProcessId";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
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
            return GetAutoNumber(nameof(SFGInventory), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(SFGInventory entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.Active);
        }
       
        public override void Insert(SFGInventory entity)
        {
            try
            {
                if (entity != null)
                {
                    Check(entity);
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = "SI"+GetPK();
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

        public override void Update(SFGInventory entity)
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