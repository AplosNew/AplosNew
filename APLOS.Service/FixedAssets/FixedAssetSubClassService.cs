#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.FixedAssets;
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

namespace Library.Service.FixedAssets
{
    public class FixedAssetSubClassService : Service<FixedAssetSubClass>, IFixedAssetSubClassService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ICompanyGroupFixedAssetSubClassService _companyGroupFixedAssetSubClassService;

        public FixedAssetSubClassService(
            IRepositoryAsync<FixedAssetSubClass> FixedAssetSubClassRepository,
            IPKGeneratorService pkGeneratorService,
            ICompanyGroupFixedAssetSubClassService companyGroupFixedAssetSubClassService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(FixedAssetSubClassRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _companyGroupFixedAssetSubClassService = companyGroupFixedAssetSubClassService;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return Query().Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        private string GetPK()
        {
            return GetAutoNumber("FixedAssetSubClass", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void Check(FixedAssetSubClass entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.Active);
            CheckUniqueColumn(UniqueColumnName.StandardName, entity.StandardName, r => r.Id != entity.Id && r.StandardName == entity.StandardName && r.Active);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.Active);
        }

        public override void Insert(FixedAssetSubClass entity)
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
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var companyGroupFixedAssetSubClass = new CompanyGroupFixedAssetSubClass
                {
                    Id = entity.Id + "-" + 1,
                    FixedAssetSubClassId = entity.Id,
                    CompanyGroupId = identity.CompanyGroupId,
                    Active = true
                };
                _companyGroupFixedAssetSubClassService.Insert(companyGroupFixedAssetSubClass);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public override void Update(FixedAssetSubClass entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                // If department row inacitve
                _companyGroupFixedAssetSubClassService.UpdateGraph(entity.Id, entity.Active);
                UpdateGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
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
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "FixedAssetSubClass Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                // If section row inactive
                _companyGroupFixedAssetSubClassService.DeleteGraph(entity.Id);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }
}