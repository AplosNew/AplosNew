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