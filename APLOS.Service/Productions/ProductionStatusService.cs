#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions;
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

namespace Library.Service.Productions
{
    public class ProductionStatusService : Service<ProductionStatus>, IProductionStatusService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyGroupProductionStatusService _companyGroupProductionStatusService;

        public ProductionStatusService(
            IRepositoryAsync<ProductionStatus> ProductionStatusRepository,
            IPKGeneratorService pkGeneratorService,
            ICompanyGroupProductionStatusService companyGroupProductionStatusService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(ProductionStatusRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _companyGroupProductionStatusService = companyGroupProductionStatusService;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return Query().Select().Max(r => r.PlanningGroupPriority + 1);
            }
            catch
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProductionStatus), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private void Check(ProductionStatus entity)
        {
            CheckUniqueColumn(UniqueColumnName.PlanningGroupPriority, entity.PlanningGroupPriority.ToString(), r => r.Id != entity.Id && r.PlanningGroupPriority == entity.PlanningGroupPriority && r.Active);
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.Active);
        }

        public override void Insert(ProductionStatus entity)
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
                var companyGroupProductionStatus = new CompanyGroupProductionStatus();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyGroupProductionStatus.Id = entity.Id + "-" + i;
                companyGroupProductionStatus.ProductionStatusId = entity.Id;
                companyGroupProductionStatus.CompanyGroupId = identity.CompanyGroupId;
                companyGroupProductionStatus.Active = true;
                _companyGroupProductionStatusService.Insert(companyGroupProductionStatus);
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

        public override void Update(ProductionStatus entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                // If department row inactive
                _companyGroupProductionStatusService.UpdateGraph(entity.Id, entity.Active);
                UpdateGraph(entity);
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

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "ProductionStatus Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                ProductionStatus entity = Find(id);
                // If section row inactive
                _companyGroupProductionStatusService.DeleteGraph(entity.Id);
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