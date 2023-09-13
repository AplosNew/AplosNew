#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.IE
{
    public class SizeGroupService : Service<SizeGroup>, ISizeGroupService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public SizeGroupService(
            IRepositoryAsync<SizeGroup> SizeGroupRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) :
            base(SizeGroupRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query().Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(SizeGroup), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void Check(SizeGroup entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName);
        }

        public override void Insert(SizeGroup entity)
        {
            try
            {
                Check(entity);
                entity.Id ="SG-"+ GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                //throw new CustomException(ex.Message, ex,
                //Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                throw ex;
            }
        }

        public override void Update(SizeGroup entity)
        {
            try
            {
                Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                //throw new CustomException(ex.Message, ex,
                //Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                //ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
                throw ex;

            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM [HKP].[SizeGroup]";
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
    }
}