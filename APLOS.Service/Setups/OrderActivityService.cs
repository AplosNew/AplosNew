#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Setups
{
    public class OrderActivityService : Service<OrderActivity>, IOrderActivityService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderActivityService(
            IRepositoryAsync<OrderActivity> buyerActivityRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork) : base(buyerActivityRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Buyer
        public void InsertBuyerActivity(OrderActivity entity)
        {
            try
            {
                CheckUnique(entity, OrderActivityType.Buyer.ToString());
                entity.Id = GetAutoNumber(nameof(OrderActivity), PKGeneratorEnum.Auto, null, DateTime.Now);
                entity.ActivityType = OrderActivityType.Buyer.ToString();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public void UpdateBuyerActivity(OrderActivity entity)
        {
            try
            {
                CheckUnique(entity, OrderActivityType.Buyer.ToString());
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        #endregion Buyer

        #region Inquiry
        public void InsertInquiryActivity(OrderActivity entity)
        {
            try
            {
                CheckUnique(entity, OrderActivityType.Inquiry.ToString());
                entity.Id = GetAutoNumber(nameof(OrderActivity), PKGeneratorEnum.Auto, null, DateTime.Now);
                entity.ActivityType = OrderActivityType.Inquiry.ToString();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public void UpdateInquiryActivity(OrderActivity entity)
        {
            try
            {
                CheckUnique(entity, OrderActivityType.Inquiry.ToString());
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        #endregion Buyer

        public GridModel Query(GridParameter parameters, string companyGroupId, string activityType)
        {
            try
            {
                parameters.CmdText = @"SELECT BA.* FROM [SCS].[OrderActivity] BA WHERE BA.CompanyGroupId='" + companyGroupId + "' AND ActivityType='" + activityType + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        public IEnumerable<object> GetCbo(string companyGroupId, string activityType)
        {
            try
            {
                return from m in base.Query(t => t.CompanyGroupId == companyGroupId && t.ActivityType == activityType).Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
        }

        private void CheckUnique(OrderActivity entity, string activityType)
        {
            if (base.Any(t => t.Id != entity.Id && t.CompanyGroupId == entity.CompanyGroupId && t.ActivityName == entity.ActivityName && t.ActivityType == activityType))
                throw new CustomException("This " + entity.ActivityName + " all ready exist in this company group.");
            if (base.Any(t => t.Id != entity.Id && t.CompanyGroupId == entity.CompanyGroupId && t.UserName == entity.UserName && t.ActivityType == activityType))
                throw new CustomException("This " + entity.UserName + " all ready exist in this company group.");
        }
    }
}