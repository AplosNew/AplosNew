#region Using

using Library.Core;
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.FixedAssets
{
    public class FixedAssetSubCategoryService : Service<FixedAssetSubCategory>, IFixedAssetSubCategoryService
    {
        #region Constractors

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyGroupFixedAssetSubCategoryService _companyGroupFixedAssetSubCategoryService;
        private readonly IFixedAssetMasterService _fixedAssetMasterService;
        private readonly ISqlRepository _sqlRepository;

        public FixedAssetSubCategoryService(
            IRepositoryAsync<FixedAssetSubCategory> fixedAssetSubCategoryRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork,
            IFixedAssetMasterService fixedAssetMasterService,
            ICompanyGroupFixedAssetSubCategoryService companyGroupFixedAssetSubCategoryService
            , ISqlRepository sqlRepository
            ) : base(fixedAssetSubCategoryRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _companyGroupFixedAssetSubCategoryService = companyGroupFixedAssetSubCategoryService;
            _fixedAssetMasterService = fixedAssetMasterService;
        }

        #endregion Constractors

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = $"SELECT * FROM [HKP].[FixedAssetSubCategory] WHERE Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public override void Insert(FixedAssetSubCategory entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                _companyGroupFixedAssetSubCategoryService.InsertGraph(new CompanyGroupFixedAssetSubCategory { FixedAssetSubCategoryId = entity.Id, Active = entity.Active });
                InsertGraph(entity);
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

        private string GetPK()
        {
            return GetAutoNumber("FixedAssetSubCategory", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private void Check(FixedAssetSubCategory entity)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.StandardName, entity.StandardName, r => r.Id != entity.Id && r.StandardName == entity.StandardName && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && !r.Archive);
        }

        public override void Update(FixedAssetSubCategory entity)
        {
            var flag = false;
            try
            {
                Check(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                // If department row inactive
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

        public void Delete(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "FixedAssetSubCategory Id"));
                var fMaster = _fixedAssetMasterService.Query(r => r.FixedAssetSubCategoryId == id).Select().FirstOrDefault();
                if (fMaster != null)
                {
                    throw new CustomException("This subcategory is used on Asset Master " + fMaster.UserName);
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                _companyGroupFixedAssetSubCategoryService.DeleteGraph(entity.Id);
                base.Delete(entity);
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

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query(r => !r.Archive).Select().Max(r => r.Sequence + 1);
            }
            catch (Exception)
            {
                return 1.00M;
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query(r => !r.Archive && r.Active).Select().OrderBy(r => r.UserName)
                       select new { Text = m.UserName, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }
    }
}