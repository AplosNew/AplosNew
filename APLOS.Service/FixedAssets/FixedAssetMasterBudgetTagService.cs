using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.FixedAssets;
using Library.Service.ChartOfAccounts;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Service.FixedAssets
{
    public class FixedAssetMasterBudgetTagService : Service<FixedAssetMasterBudgetTag>, IFixedAssetMasterBudgetTagService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<FixedAssetMasterBudgetTag> _fixedAssetMasterBudgetTagRepository;

        public FixedAssetMasterBudgetTagService(
            IRepositoryAsync<FixedAssetMasterBudgetTag> fixedAssetMasterBudgetTagRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository) : base(fixedAssetMasterBudgetTagRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _fixedAssetMasterBudgetTagRepository = fixedAssetMasterBudgetTagRepository;
        }

        #endregion Constructor

        public void InsertOrUpdateGraph(IEnumerable<FixedAssetMasterBudgetTag> entities)
        {
            var flag = false;
            try
            {
                var pk = base.GetMaxNumber(nameof(FixedAssetMasterBudgetTag), PKGeneratorEnum.Auto, null, DateTime.Now);
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entities != null)
                {
                    foreach (var item in entities)
                    {
                        Check(item);
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            item.ModelState = ModelState.Added;
                            pk.MaxNumber++;
                            AuditService.Log(item);
                            item.Id = pk.MaxNumber.ToString();
                        }
                        else
                        {
                            item.ModelState = ModelState.Modified;
                            AuditService.Log(item);
                        }
                        InsertOrUpdateGraph(item);
                    }
                }
                else
                {
                    throw new CustomException("No changes found to save");
                }
                flag = true;
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
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel Query(GridParameter parameters, string coaId)
        {
            parameters.CmdText = @"SELECT FAMT.Id, BM.COAId, FAMT.FixedAssetMasterId, BM.Id AS BudgetMasterId, BM.RefNo, BM.GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                , B.Code AS BudgetCode, B.UserName AS BudgetName, BC.UserName AS BudgetCategoryName, BSC.UserName AS BudgetSubCategoryName
                                FROM [MST].[BudgetMaster] AS BM
                                LEFT JOIN [HKP].[BudgetGroup] AS BG ON BG.Id=bm.BudgetGroupId
                                LEFT JOIN [HKP].[BudgetCategory] AS BC ON BC.Id=BM.BudgetCategoryId
                                LEFT JOIN [HKP].[BudgetSubCategory] AS BSC ON BSC.Id=BM.BudgetSubCategoryId
                                LEFT JOIN [HKP].[Budget] B ON B.Id=BM.BudgetId
                                LEFT JOIN [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                                LEFT JOIN [HKP].[GLAccountType] AS GLAT ON GLAT.GLGeneralInfoId=GLGI.Id
                                LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                                LEFT JOIN [HKP].[FixedAssetMasterBudgetTag] AS FAMT ON FAMT.BudgetMasterId=BM.Id
                                WHERE BM.Archive=0 AND BM.Active=1 AND GLAT.AccountType='" + AccountTypeEnum.Asset + "' AND ACT.Id='" + AccountTypeEnum.Asset + "' AND BM.COAId='" + coaId + "'";
            return _sqlRepository.GetGridData(parameters);
        }
        private void Check(FixedAssetMasterBudgetTag entity)
        {
            CheckUniqueColumn(UniqueColumnName.AssetUserName, entity.FixedAssetMasterId, r => r.Id != entity.Id  && r.FixedAssetMasterId == entity.FixedAssetMasterId);
        }
    }
}