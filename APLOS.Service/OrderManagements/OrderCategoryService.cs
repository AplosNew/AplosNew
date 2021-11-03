#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.OrderManagements
{
    public partial class OrderCategoryService : Service<OrderCategory>, IOrderCategoryService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyGroupOrderCategoryService _companyGroupOrderCategoryService;

        public OrderCategoryService(
            IRepositoryAsync<OrderCategory> OrderCategoryRepository,
            IPKGeneratorService pkGeneratorService,
            ICompanyGroupOrderCategoryService companyGroupOrderCategoryService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(OrderCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _companyGroupOrderCategoryService = companyGroupOrderCategoryService;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return Query().Select().Max(r => r.PlanningPriority + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(OrderCategory), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(OrderCategory entity)
        {
            CheckUniqueColumn(UniqueColumnName.PlanningPriority, entity.PlanningPriority.ToString(), r => r.Id != entity.Id && r.PlanningPriority == entity.PlanningPriority && r.Active);
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.Active);
        }

        public override void Insert(OrderCategory entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                entity.Active = true;
                base.Insert(entity);
                var i = 1;
                var companyGroupOrderCategory = new CompanyGroupOrderCategory();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyGroupOrderCategory.Id = entity.Id + "-" + i;
                companyGroupOrderCategory.OrderCategoryId = entity.Id;
                companyGroupOrderCategory.CompanyGroupId = identity.CompanyGroupId;
                companyGroupOrderCategory.Active = true;
                _companyGroupOrderCategoryService.Insert(companyGroupOrderCategory);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public override void Update(OrderCategory entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                // If department row inactive
                _companyGroupOrderCategoryService.UpdateGraph(entity.Id, entity.Active);
                UpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "OrderCategory Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                OrderCategory entity = Find(id);
                // If section row inactive
                _companyGroupOrderCategoryService.DeleteGraph(entity.Id);
                base.DeleteGraph(entity);
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}