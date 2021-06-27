#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Costings;
using Library.Model.Machines;
using Library.Model.Processes;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#endregion Using

namespace Library.Service.Costings
{
    public class CostingComponentService : Service<CostingComponent>, ICostingComponentService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public CostingComponentService(
            IRepositoryAsync<CostingComponent> CostingSubCategoryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(CostingSubCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                //return Query().Select().Max(t => t.Sequence + 1);
                DataTable dt = _sqlRepository.GetDataTable("Select Max(Sequence) AS Sequence from HKP.CostingItem");
                if (dt.Rows.Count == 0)
                    return 1.00M;
                else
                    return (decimal)OTSBD.clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            }
            catch
            {
                return 1.00M;
            }
        }

        private void Check(CostingComponent entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, t => t.Id != entity.Id && t.Code == entity.Code && t.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, t => t.Id != entity.Id && t.UserName == entity.UserName && t.Active);
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return (from m in base.Query(t => t.Active).Select().OrderBy(t => t.UserName)
                        select new { Text = m.UserName, Value = m.Id }).Distinct();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Menu.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT * FROM [HKP].[CostingComponent]";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public override void Insert(CostingComponent entity)
        {
            try
            {
                Check(entity);
                entity.Id = GetAutoNumber(nameof(CostingComponent), PKGeneratorEnum.Auto, DateTime.Now);
                entity.Active = true;
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public override void Update(CostingComponent entity)
        {
            try
            {
                Check(entity);
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }
        
    }
}