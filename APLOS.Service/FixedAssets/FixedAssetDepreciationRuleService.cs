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
    public class FixedAssetDepreciationRuleService : Service<FixedAssetDepreciationRule>, IFixedAssetDepreciationRuleService
    {
        #region Constractors

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyFixedAssetDepreciationRuleService _companyFixedAssetDepreciationRuleService;
        private readonly ISqlRepository _sqlRepository;

        public FixedAssetDepreciationRuleService(
            IRepositoryAsync<FixedAssetDepreciationRule> fixedAssetRepository,
            IPKGeneratorService pkGeneratorService,
             ICompanyFixedAssetDepreciationRuleService companyFixedAssetDepreciationRuleService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(fixedAssetRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _companyFixedAssetDepreciationRuleService = companyFixedAssetDepreciationRuleService;
        }

        #endregion Constractors

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT DR.* FROM MST.FixedAssetDepreciationRule DR ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public override void Insert(FixedAssetDepreciationRule entity)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                InsertGraph(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetPK()
        {
            return GetAutoNumber("DepreciationRule", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(FixedAssetDepreciationRule entity)
        {
            var flag = false;
            try
            {
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public override void Archive(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "DepreciationRule Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                entity.ModelState = ModelState.Modified;
                AuditService.Log(entity);
                // If division row inactive
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
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
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "DepreciationRule Id"));
                var data_Db = _companyFixedAssetDepreciationRuleService.Query(r => r.DepreciationRuleId == id).Select().FirstOrDefault();
                if (data_Db != null)
                {
                    throw new CustomException("This Depreciation Rule is used on company Depreciation Rule");
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                //_companyFixedAssetDepreciationRuleService.DeleteGraph(id);
                base.DeleteGraph(id);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query(r => r.Active).Select().OrderBy(r => r.Code)
                       select new { Text = m.Description, Value = m.Id, m.Code };
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