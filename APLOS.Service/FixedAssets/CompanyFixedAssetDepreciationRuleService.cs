using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.FixedAssets;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.FixedAssets
{
    public class CompanyFixedAssetDepreciationRuleService : Service<CompanyFixedAssetDepreciationRule>, ICompanyFixedAssetDepreciationRuleService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public CompanyFixedAssetDepreciationRuleService(
            IRepositoryAsync<CompanyFixedAssetDepreciationRule> companyFixedAssetDepreciationRuleRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(companyFixedAssetDepreciationRuleRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
        }

        #endregion Constructor

        public GridModel QueryAssetMaster(GridParameter parameters)
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
                                WHERE BM.Archive=0 AND BM.Active=1 ";
            return _sqlRepository.GetGridData(parameters);
        }


        public List<Dictionary<string, object>> GetListAssetMaster(string companyId)
        {
            var sql = @"select FAM.UserName,FAM.Id FixedAssetMasterId, CFADR.Id,CFADR.DepreciationRuleId ,CFADR.CompanyId
                    from mst.FixedAssetMaster FAM
                    left join (select * from  mst.CompanyFixedAssetDepreciationRule where CompanyId='"+companyId+"')CFADR ON CFADR.FixedAssetMasterId = FAM.Id";
                   return _sqlRepository.GetDataCollection(sql);

        }



        public GridModel Query(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT CWDR.*, DR.Description, FM.UserName AS FixedAssetMaster FROM MST.CompanyFixedAssetDepreciationRule CWDR
                                        LEFT OUTER JOIN MST.FixedAssetDepreciationRule DR ON CWDR.DepreciationRuleId = DR.Id
                                        LEFT OUTER JOIN MST.FixedAssetMaster FM ON CWDR.FixedAssetMasterId = FM.Id
                                        WHERE CWDR.CompanyId='" + companyId + "' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombine(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.order = "ASC";
                parameters.sort = "FixedAssetMasterName";
                parameters.CmdText = @"	SELECT  CDR.Id
										, FAM.Id AS FixedAssetMasterId
										,CDR.DepreciationRuleId
										,CDR.CompanyId
										,DR.Description
										,DR.Code
                                        ,FAM.UserName AS FixedAssetMasterName
                                       ,FACT.UserName AS FixedAssetCategoryName
                                       ,FASCT.UserName AS FixedAssetSubCategoryName
										,FAM.FixedAssetCategoryId
										,FAM.FixedAssetSubCategoryId
                                            FROM MST.FixedAssetMaster As FAM
                                            LEFT OUTER JOIN HKP.FixedAssetCategory As FACT ON FACT.Id = FAM.FixedAssetCategoryId
                                            LEFT OUTER JOIN HKP.FixedAssetSubCategory As FASCT ON FASCT.Id = FAM.FixedAssetSubCategoryId
           LEFT OUTER JOIN  MST.CompanyFixedAssetDepreciationRule CDR ON CDR.FixedAssetMasterId = FAM.Id and CDR.CompanyId='" + companyId + @"'
           LEFT OUTER JOIN MST.FixedAssetDepreciationRule DR ON CDR.DepreciationRuleId = DR.Id ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        private static string GetOp(string search)
        {
            return string.IsNullOrEmpty(search) ? "WHERE" : "AND"; ;
        }

        public GridModel GetSearchWithCombineAll(GridParameter parameters, string companyId, string FixedAssetCategoryIds, string FixedAssetSubCategoryIds)
        {
            try
            {
                string search = null;

                if (FixedAssetCategoryIds != "''")
                {
                    search += " " + GetOp(search) + "  FAM.FixedAssetCategoryId IN(" + FixedAssetCategoryIds + ")";
                }

                if (FixedAssetSubCategoryIds != "''")
                {
                    search += "  " + GetOp(search) + " FAM.FixedAssetSubCategoryId IN(" + FixedAssetSubCategoryIds + ")";
                }

                parameters.CmdText = @"SELECT  CDR.Id
										, FAM.Id AS FixedAssetMasterId
										,CDR.DepreciationRuleId
										,CDR.CompanyId
										,DR.Description
										,DR.Code
                                        ,FAM.UserName AS FixedAssetMasterName
                                       ,FACT.UserName AS FixedAssetCategoryName
                                       ,FASCT.UserName AS FixedAssetSubCategoryName
										,FAM.FixedAssetCategoryId
										,FAM.FixedAssetSubCategoryId
                                            FROM MST.FixedAssetMaster As FAM
                                            LEFT OUTER JOIN HKP.FixedAssetCategory As FACT ON FACT.Id = FAM.FixedAssetCategoryId
                                            LEFT OUTER JOIN HKP.FixedAssetSubCategory As FASCT ON FASCT.Id = FAM.FixedAssetSubCategoryId
           LEFT OUTER JOIN  MST.CompanyFixedAssetDepreciationRule CDR ON CDR.FixedAssetMasterId = FAM.Id and CDR.CompanyId='" + companyId + @"'
           LEFT OUTER JOIN MST.FixedAssetDepreciationRule DR ON CDR.DepreciationRuleId = DR.Id " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineWithAssing(GridParameter parameters, string companyId, string FixedAssetCategoryIds, string FixedAssetSubCategoryIds)
        {
            try
            {
                var search = "WHERE CDR.Id <> '' ";
                if (FixedAssetCategoryIds != "''")
                    search += " AND FAM.FixedAssetCategoryId IN(" + FixedAssetCategoryIds + ")";
                if (FixedAssetSubCategoryIds != "''")
                    search += " AND FAM.FixedAssetSubCategoryId IN(" + FixedAssetSubCategoryIds + ")";
                parameters.CmdText = @"SELECT  CDR.Id
										, FAM.Id AS FixedAssetMasterId
										,CDR.DepreciationRuleId
										,CDR.CompanyId
										,DR.Description
										,DR.Code
                                        ,FAM.UserName AS FixedAssetMasterName
                                       ,FACT.UserName AS FixedAssetCategoryName
                                       ,FASCT.UserName AS FixedAssetSubCategoryName
										,FAM.FixedAssetCategoryId
										,FAM.FixedAssetSubCategoryId
                                            FROM MST.FixedAssetMaster As FAM
                                            LEFT OUTER JOIN HKP.FixedAssetCategory As FACT ON FACT.Id = FAM.FixedAssetCategoryId
                                            LEFT OUTER JOIN HKP.FixedAssetSubCategory As FASCT ON FASCT.Id = FAM.FixedAssetSubCategoryId
           LEFT OUTER JOIN  MST.CompanyFixedAssetDepreciationRule CDR ON CDR.FixedAssetMasterId = FAM.Id and CDR.CompanyId='" + companyId + @"'
           LEFT OUTER JOIN MST.FixedAssetDepreciationRule DR ON CDR.DepreciationRuleId = DR.Id
            " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineWithNotAssing(GridParameter parameters, string companyId, string FixedAssetCategoryIds, string FixedAssetSubCategoryIds)
        {
            try
            {
                var search = " ";
                if (FixedAssetCategoryIds != "''")
                    search += " AND FAM.FixedAssetCategoryId IN(" + FixedAssetCategoryIds + ")";
                if (FixedAssetSubCategoryIds != "''")
                    search += " AND FAM.FixedAssetSubCategoryId IN(" + FixedAssetSubCategoryIds + ")";
                parameters.CmdText = @"SELECT CDR.Id, FAM.Id AS FixedAssetMasterId, CDR.DepreciationRuleId, CDR.CompanyId, DR.Description, DR.Code
                                    , FAM.UserName AS FixedAssetMasterName, FACT.UserName AS FixedAssetCategoryName, FASCT.UserName AS FixedAssetSubCategoryName
									, FAM.FixedAssetCategoryId, FAM.FixedAssetSubCategoryId
                                    FROM MST.FixedAssetMaster As FAM
                                    LEFT OUTER JOIN HKP.FixedAssetCategory As FACT ON FACT.Id = FAM.FixedAssetCategoryId
                                    LEFT OUTER JOIN HKP.FixedAssetSubCategory As FASCT ON FASCT.Id = FAM.FixedAssetSubCategoryId
                                    LEFT OUTER JOIN  MST.CompanyFixedAssetDepreciationRule CDR ON CDR.FixedAssetMasterId = FAM.Id and CDR.CompanyId='" + companyId + @"'
                                    LEFT OUTER JOIN MST.FixedAssetDepreciationRule DR ON CDR.DepreciationRuleId = DR.Id
                                    WHERE (ISNULL(CDR.DepreciationRuleId,'')= '') " + search + " ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber("CompanyWiseDepreciationRule", PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public void InsertUpdateCDepreciation(IEnumerable<CompanyFixedAssetDepreciationRule> entities)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var pk = GetMaxNumber();
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        InsertGraph(item);
                    }
                    else
                    {
                        UpdateGraph(item);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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

        public void DeleteGraph(string id)
        {
            var data_Db = base.Query(r => r.Id == id).Select().FirstOrDefault().Id;
            if (data_Db != null)
            {
                Delete(data_Db);
            }
        }
    }
}