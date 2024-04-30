using Aplos.Service.Enums;
using Library.Core;
using Library.Data;
using Library.Data.Sql;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Taxations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Library.Accounting.Accounts
{
    public class AccountsGLService
    {
        private readonly ISqlRepository _sqlRepository;
        public AccountsGLService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        public GridModel GetAllGLBudgetActivity(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT  distinct AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , B.BudgetMasterId, B.RefNo, B.BudgetCode, B.BudgetName, A.ActivityId, A.ActivityCode, A.ActivityName, A.BudgetMasterActivityId
                                    FROM [HKP].[GLGeneralInfo] AS GLGI
                                    LEFT JOIN [HKP].[GLCompanyGroup] AS GLCG ON GLCG.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLAccountType] AS GLTY ON GLTY.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                    LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                                    LEFT JOIN (SELECT BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, BM.GLGeneralInfoId, BM.RefNo
	                                    FROM [HKP].[Budget] AS B
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.BudgetId=B.Id
                                    ) AS B ON B.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN (SELECT A.Id AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, BA.BudgetMasterId,BA.Id AS BudgetMasterActivityId
	                                    FROM [HKP].[Activity] AS A
	                                    LEFT JOIN [MST].[BudgetMasterActivity] AS BA ON BA.ActivityId=A.Id
                                    ) AS A ON A.BudgetMasterId=B.BudgetMasterId
                                    WHERE GLGI.Archive=0 AND GLGI.Active=1 AND GLCG.CompanyGroupId='" + companyGroupId + "' AND GLCI.CompanyId='" + companyId + @"' 
                                    AND  GLGI.Id not in ( select GlGeneralInfoId from mst.BankMaster where AccountType='HouseBank')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetNonReconAssetLiabilityGLBudgetActivityList(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, B.BudgetMasterId, B.RefNo, B.BudgetCode, B.BudgetName
                                    , A.ActivityId, A.ActivityCode, A.ActivityName
                                    FROM [HKP].[GLGeneralInfo] AS GLGI
                                    LEFT JOIN [HKP].[GLCompanyGroup] AS GLCG ON GLCG.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                    LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                                    LEFT JOIN (SELECT BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, BM.GLGeneralInfoId, BM.RefNo
	                                    FROM [HKP].[Budget] AS B
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.BudgetId=B.Id
                                    ) AS B ON B.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN (SELECT A.Id AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, BA.BudgetMasterId
	                                    FROM [HKP].[Activity] AS A
	                                    LEFT JOIN [MST].[BudgetMasterActivity] AS BA ON BA.ActivityId=A.Id
                                    ) AS A ON A.BudgetMasterId=B.BudgetMasterId
                                    WHERE GLGI.Archive=0 AND GLGI.Active=1 AND GLCG.CompanyGroupId='" + companyGroupId + "' AND GLCI.CompanyId='" + companyId + @"' 
                                    AND GLGI.Id NOT IN (SELECT GLAT.GLGeneralInfoId FROM [HKP].[GLAccountType] as GLAT WHERE GLAT.GLGeneralInfoId<>'') AND ACT.Id IN ('Asset','Liability')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        private static string GetGLSQL()
        {
            return @"SELECT GLGI.COAId, C.UserName AS COA, GLGI.AccountGroupId, AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                        FROM [HKP].[GLGeneralInfo] AS GLGI
                        JOIN [HKP].[GLCompanyGroup] AS GLCG ON glcg.GLGeneralInfoId=GLGI.Id
                        LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                        LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                        LEFT JOIN [HKP].[GLAccountType] AS GLAT ON GLAT.GLGeneralInfoId=GLGI.Id
                        LEFT JOIN [HKP].[COA] AS C ON C.Id=GLGI.COAId
                        WHERE GLGI.Active=1 AND GLGI.Archive=0 ";
        }
        public GridModel GetAssetLiabilityGLListTaxRecon(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = GetGLSQL() + " AND GLGI.COAId = '" + coaId + "' AND ACT.Id IN ('" + AccountTypeEnum.Asset + "','" + AccountTypeEnum.Liability + "') AND GLGI.IsPostingAutomaticOnly=0 " +
                    "AND GLGI.Id  IN (SELECT GLAT.GLGeneralInfoId FROM [HKP].[GLAccountType] as GLAT WHERE GLAT.GLGeneralInfoId<>'' AND GLAT.AccountType='Tax') ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetAssetCOAWise(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT C.Id AS COAId, AG.UserName AS AccountGroupName, C.UserName AS COAName
		                            , GLGI.UserName AS GLGeneralInfoName, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.Id AS GLGeneralInfoId
		                            FROM HKP.GLGeneralInfo AS GLGI
									JOIN HKP.COA AS C ON C.Id=GLGI.COAId
		                            LEFT JOIN HKP.GLAccountType AS GLAT ON GLAT.GLGeneralInfoId = GLGI.Id
		                            LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GLGI.AccountGroupId
									LEFT JOIN HKP.AccountType AS ACT ON ACT.Id =AG.AccountTypeId
                                    WHERE GLGI.COAId='" + coaId + @"' AND ACT.Id='" + AccountTypeEnum.Asset + @"' 
                                    AND GLGI.Id  IN (SELECT GLAT.GLGeneralInfoId FROM [HKP].[GLAccountType] as GLAT WHERE GLAT.GLGeneralInfoId<>'' AND GLAT.AccountType='Material')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetAssetCOAWiseIncentive(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT C.Id AS COAId, AG.UserName AS AccountGroupName, C.UserName AS COAName
		                            , GLGI.UserName AS GLGeneralInfoName, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.Id AS GLGeneralInfoId
		                            FROM HKP.GLGeneralInfo AS GLGI
									JOIN HKP.COA AS C ON C.Id=GLGI.COAId
		                            LEFT JOIN HKP.GLAccountType AS GLAT ON GLAT.GLGeneralInfoId = GLGI.Id
		                            LEFT JOIN HKP.AccountGroup AS AG ON AG.Id = GLGI.AccountGroupId
									LEFT JOIN HKP.AccountType AS ACT ON ACT.Id =AG.AccountTypeId
                                    WHERE GLGI.COAId='" + coaId + @"' AND ACT.Id='" + AccountTypeEnum.Asset + @"' 
                                    AND GLGI.Id NOT  IN (SELECT GLAT.GLGeneralInfoId FROM [HKP].[GLAccountType] as GLAT WHERE GLAT.GLGeneralInfoId<>'' AND GLAT.AccountType='Asset')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetExpenseGLList(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = GetGLSQL() + " AND GLGI.COAId = '" + coaId + "' AND ACT.Id in ('" + AccountTypeEnum.Expense + "','"+ AccountTypeEnum.Asset + @"')
                    AND GLGI.Id NOT IN (SELECT GLAT.GLGeneralInfoId FROM [HKP].[GLAccountType] as GLAT WHERE GLAT.GLGeneralInfoId<>'')
                    ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public GridModel GetExpenseGLTaxRecon(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = GetGLSQL() + " AND GLGI.COAId = '" + coaId + "' AND ACT.Id='" + AccountTypeEnum.Expense  +"' "+ 
                    "AND GLGI.Id IN(SELECT GLAT.GLGeneralInfoId FROM [HKP].[GLAccountType] as GLAT WHERE GLAT.GLGeneralInfoId <> '' AND GLAT.AccountType = 'Tax') ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetCreditableGLTaxRecon(GridParameter parameters, string coaId, string companyGroupId)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT glgi.COAId, GLGI.Id AS CreditableGL, GLGI.Id AS GLGeneralInfoId, GLGI.UserName AS CreditableGLItem, GLGI.UserName AS GLGeneralInfoName,
                                    GLGI.AccountCode AS CreditableGLCode, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.AccountGroupId, ag.UserName AS AccountGroupName, c.UserName AS COA
                                    FROM HKP.[GLGeneralInfo] AS GLGI
                                    LEFT JOIN HKP.[GLCompanyInfo] AS GLCI ON glci.GLGeneralInfoId = GLGI.Id
                                    LEFT JOIN  HKP.[AccountGroup] AS AG ON AG.Id = GLGI.AccountGroupId
                                    LEFT JOIN HKP.AccountType AS ACT ON ACT.Id = AG.AccountTypeId
                                    LEFT JOIN  HKP.[GLCompanyGroup] AS GLCG  ON GLCG.GLGeneralInfoId = GLGI.Id
                                    LEFT JOIN  HKP.[COA] AS c ON c.Id = GLGI.COAId
                                    WHERE ACT.Id='" + AccountTypeEnum.Asset + @"'
                                    AND GLGI.COAId='" + coaId + "' AND GLGI.Archive = 0 AND GLCG.CompanyGroupId='" + companyGroupId + @"'
                                    AND GLGI.Active=1 AND  GLGI.Id  IN (SELECT GLAT.GLGeneralInfoId FROM [HKP].[GLAccountType] as GLAT WHERE GLAT.GLGeneralInfoId<>'' AND GLAT.AccountType = 'Tax')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetWithHoldGLSetupTaxRecon(GridParameter parameters, string coaId, string comgroupid)
        {
            try
            {
                parameters.CmdText = @"SELECT DISTINCT  glgi.COAId, glgi.Id AS WithholdCreditableGL, glgi.Id AS GLGeneralInfoId, glgi.UserName AS GLGeneralInfoName,
                                      glgi.UserName AS WithHoldGLItem, glgi.AccountCode AS GLGeneralInfoCode, glgi.AccountCode AS WithHoldGLCode, glgi.AccountGroupId,
                                      ag.UserName AS AccountGroupName, c.UserName AS COA
                                    FROM HKP.[GLGeneralInfo] AS glgi
                                    LEFT JOIN HKP.[AccountGroup] AS ag ON AG.Id=glgi.AccountGroupId
                                    LEFT JOIN HKP.AccountType AS ACT ON ACT.Id=AG.AccountTypeId
                                    LEFT JOIN HKP.[GLCompanyGroup] AS glcg  ON glcg.GLGeneralInfoId = glgi.Id
                                    LEFT JOIN HKP.[COA] AS c ON c.Id=glgi.COAId
                                    WHERE ACT.Id='" + AccountTypeEnum.Liability + @"'
                                    AND glgi.COAId='" + coaId + @"' AND glcg.CompanyGroupId='" + comgroupid + @"'  AND glgi.Archive=0 AND glgi.Active=1 AND  GLGI.Id  IN (SELECT GLAT.GLGeneralInfoId FROM [HKP].[GLAccountType] as GLAT WHERE GLAT.GLGeneralInfoId<>'' AND GLAT.AccountType = 'Tax')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetInvoiceGLBudgetList(GridParameter parameters, string companyGroupId, string companyId, string AccountType)
        {
            try
            {
                parameters.CmdText = @"SELECT distinct GLGI.COAId, C.UserName AS COA, GLGI.AccountGroupId, AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , BU.BudgetMasterId, BU.BudgetCode, BU.BudgetName,BU.RefNo, A.ActivityId, A.ActivityCode, A.ActivityName,ACT.Id AccountType,A.IsOrderSpecific,A.ActivityOrderType,A.ValueOfDistribution
                                    FROM [HKP].[GLGeneralInfo] AS GLGI
                                    LEFT JOIN HKP.[GLCompanyInfo]AS GLCI ON GLCI.GLGeneralInfoId = GLGI.Id
                                    LEFT JOIN HKP.[AccountGroup]  AS AG ON AG.Id = GLGI.AccountGroupId
                                    LEFT JOIN HKP.[AccountType]  AS ACT ON ACT.Id = AG.AccountTypeId
                                    LEFT JOIN HKP.[GLCompanyGroup] AS glcg ON glcg.GLGeneralInfoId = GLGI.Id
                                    LEFT JOIN HKP.[COA] AS C ON C.Id = GLGI.COAId
                                    LEFT JOIN (SELECT BM.Id AS BudgetMasterId,BM.RefNo, B.Code AS BudgetCode, B.UserName AS BudgetName, BM.GLGeneralInfoId FROM HKP.Budget AS B
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON B.Id=BM.BudgetId
                                    ) AS BU ON BU.GLGeneralInfoId=GLGI.Id
                                    LEFT OUTER JOIN (SELECT A.Id AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, BM.Id AS BudgetMasterId,A.IsOrderSpecific,A.ActivityOrderType,A.ValueOfDistribution  FROM HKP.Activity AS A
										LEFT OUTER JOIN [MST].[BudgetMasterActivity] AS BA ON A.Id= BA.ActivityId
										LEFT OUTER JOIN [MST].[BudgetMaster] AS BM ON BA.BudgetMasterId=BM.Id
										LEFT OUTER JOIN HKP.Budget AS B ON BM.BudgetId = B.Id
									) AS A ON A.BudgetMasterId=BU.BudgetMasterId
                                    WHERE glcg.CompanyGroupId='" + companyGroupId + @"' AND GLCI.CompanyId='" + companyId + @"' AND GLGI.IsPostingAutomaticOnly=0 AND ACT.Id in ('" + AccountType + @"','Asset','Liability') AND GLGI.Active = 1 AND GLGI.Archive = 0
                                    AND GLGI.Id NOT IN(SELECT BM.GLGeneralInfoId FROM [MST].[BankMaster] AS BM  WHERE BM.GLGeneralInfoId <> '')
                                    AND GLGI.Id NOT IN(SELECT CM.GLGeneralInfoId FROM [MST].[CashMaster] AS CM  WHERE CM.GLGeneralInfoId <> '') 
                                    --AND  GLGI.Id NOT IN (SELECT GLAT.GLGeneralInfoId FROM [HKP].[GLAccountType] as GLAT WHERE GLAT.GLGeneralInfoId<>'')
                                    ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetTaxCategoryMaterialLevelCbo(string companyGroupId, string countryId)
        {
            try
            {
                string sql = "";
                sql = @"SELECT Id Value, UserName Text FROM MST.TaxCategory 
								WHERE  Active = 1 AND CompanyGroupId = '" + companyGroupId + @"' AND CountryId = '" + countryId + @"' AND TaxCategoryLevel='Material' ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetTaxCategoryGSTTypeCbo(string companyGroupId, string countryId)
        {
            try
            {
                string sql = "";
                sql = @"SELECT Id Value, UserName Text FROM MST.TaxCategory 
								WHERE  Active = 1 AND CompanyGroupId = '" + companyGroupId + @"' AND CountryId = '" + countryId + @"' AND TaxCategoryType='GST' ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetAdditionalTaxCbo(DateTime postingDate, string companyId)
        {
            try
            {
                var sql = @"SELECT DISTINCT TC.Id, TC.UserName,TCY.[Type],TCD.ValueOfFixed,TC.TaxCategoryId
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                        LEFT JOIN MST.TaxCategory TCA ON TCA.Id=TC.TaxCategoryId
						LEFT JOIN MST.TaxCodeDetail TCD ON TCD.TaxCodeId=TC.Id AND TCD.TaxCodeYearId=TCY.Id
                        WHERE TC.InputOrOutput='" + TaxCodeInputOutput.Input + @"'
						AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + companyId + @"' 
                         AND TCA.TaxCategoryLevel='"+ TaxCategoryLevelEnum.Invoice.ToString() + @"'
                    --UNION ALL
						-- SELECT DISTINCT TC.Id, TC.UserName AS Text
                        --FROM [MST].[TaxCodeYear] AS TCY
                        --LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        --LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					   -- LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						--LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                       -- LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                       -- LEFT JOIN MST.TaxCategory TCA ON TCA.Id=TC.TaxCategoryId
                       -- WHERE TC.InputOrOutput='Input'
						--AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + companyId + @"' 
                        -- AND TC.IsRCM=0
                    ";
                var data = _sqlRepository.GetDataCollection(sql);
                if (null == data)
                    throw new CustomException(ResourcesCore.FYNotFound);
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetAdditionalTaxOutputCbo(DateTime postingDate, string companyId)
        {
            try
            {
                var sql = @"SELECT DISTINCT TC.Id, TC.UserName,TCY.[Type],TCD.ValueOfFixed,TC.TaxCategoryId
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                        LEFT JOIN MST.TaxCategory TCA ON TCA.Id=TC.TaxCategoryId
						LEFT JOIN MST.TaxCodeDetail TCD ON TCD.TaxCodeId=TC.Id AND TCD.TaxCodeYearId=TCY.Id
                        WHERE TC.InputOrOutput='" + TaxCodeInputOutput.Output + @"'
						AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + companyId + @"' 
                         AND TCA.TaxCategoryType='TCS'
                    ";
                var data = _sqlRepository.GetDataCollection(sql);
                if (null == data)
                    throw new CustomException(ResourcesCore.FYNotFound);
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetTDSCbo(DateTime postingDate, string companyId)
        {
            try
            {
                var sql = @"SELECT DISTINCT TC.Id, TC.UserName,TCY.[Type],TCD.ValueOfFixed,TC.TaxCategoryId
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                        LEFT JOIN MST.TaxCategory TCA ON TCA.Id=TC.TaxCategoryId
						LEFT JOIN MST.TaxCodeDetail TCD ON TCD.TaxCodeId=TC.Id AND TCD.TaxCodeYearId=TCY.Id
                        WHERE TC.InputOrOutput='" + TaxCodeInputOutput.Input + @"'
						AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + companyId + @"' 
                         AND TCA.TaxCategoryType='TDS'
                    ";
                var data = _sqlRepository.GetDataCollection(sql);
                if (null == data)
                    throw new CustomException(ResourcesCore.FYNotFound);
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetTDSOutPutCbo(DateTime postingDate, string companyId)
        {
            try
            {
                var sql = @"SELECT DISTINCT TC.Id, TC.UserName,TCY.[Type],TCD.ValueOfFixed,TC.TaxCategoryId
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                        LEFT JOIN MST.TaxCategory TCA ON TCA.Id=TC.TaxCategoryId
						LEFT JOIN MST.TaxCodeDetail TCD ON TCD.TaxCodeId=TC.Id AND TCD.TaxCodeYearId=TCY.Id
                        WHERE TC.InputOrOutput='" + TaxCodeInputOutput.Output + @"'
						AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + companyId + @"' 
                         AND TCA.TaxCategoryType='TDS'
                    ";
                var data = _sqlRepository.GetDataCollection(sql);
                if (null == data)
                    throw new CustomException(ResourcesCore.FYNotFound);
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        private Dictionary<string, object> GetCompanyCOAId(string companyId)
        {
            var cmdText = @"select COAId from org.Company where Id = '" + companyId + "'";
            return _sqlRepository.GetData(cmdText);
        }
        public Dictionary<string, object> GetTaxCodeById(string companyId, string taxCodeId, string invoiceTaxId, DateTime postingDate)
        {
            try
            {

                var coaId = GetCompanyCOAId(companyId);
                var sql = @"SELECT tc.Id AS TaxCodeId, tc.TaxCategoryId, tc.Code, tc.UserName, tc.Description, tc.ManuallyEditable,
                            tc.IsMerge, tc.IsCreditable, tc.IsWithhold, vit.TaxAmount, vit.TaxAutoAmount, 0 Sequence,tc.IsRCM,
                            tgl.CreditableGLId, tgl.ExpensesGLId, tgl.WithholdCreditableGLId, vit.Id,TCY.[Type],TCY.ValueOfFixed
                            FROM MST.TaxCode AS tc
                            LEFT JOIN(SELECT T.TaxCodeId,T.[Type],TCD.ValueOfFixed FROM  MST.TaxCodeYear T LEFT JOIN SCS.TaxYear TY ON T.TaxYearId=TY.Id
							LEFT JOIN MST.TaxCodeDetail TCD ON TCD.TaxCodeYearId=T.Id
							WHERE TY.StartDate<='" + postingDate.ToDbDate() + "' AND TY.EndDate >='" + postingDate.ToDbDate() + @"'
							  ) TCY ON TCY.TaxCodeId=tc.Id 
                            LEFT OUTER JOIN (SELECT vt.TaxAmount, vt.TaxAutoAmount, vt.TaxCodeId, vt.Id
                            FROM TRN.InvoiceTax AS vt
                            WHERE vt.Id = '" + invoiceTaxId + @"') AS vit
                            ON vit.TaxCodeId = tc.Id
                            LEFT  JOIN (SELECT tcg.CreditableGLId, tcg.WithholdCreditableGLId, tcg.ExpensesGLId, tcg.TaxCodeId
                            FROM MST.TaxCodeGL AS tcg WHERE tcg.COAId = '" + coaId["COAId"].ToString() + @"') AS tgl
                            ON tgl.TaxCodeId = tc.Id WHERE tc.Id = '" + taxCodeId + "'";
                return _sqlRepository.GetData(sql, taxCodeId);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetTaxCodeInputVATGST(DateTime postingDate, string companyId)
        {
            try
            {
                var sql = @"SELECT DISTINCT TC.Id, TC.UserName AS Text, TC.IsWithhold, TC.IsCreditable, TC.IsMerge
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        LEFT JOIN MST.TaxCategory TCT ON TCT.Id=TC.TaxCategoryId
                        LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                        WHERE TC.InputOrOutput='" + TaxCodeInputOutput.Input + @"' AND TCT.TaxCategoryType IN ('GST','VAT')
						AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + companyId + "'";
                var data = _sqlRepository.GetCombo(sql, "Id", "Text");
                if (null == data)
                    throw new CustomException(ResourcesCore.FYNotFound);
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetTaxCodeInvoiceTriggeringInstanceOthers()
        {
            try
            {
                var sql = @"SELECT DISTINCT TC.Id, TC.UserName AS Text
                        FROM [MST].[TaxCode] TC
                        WHERE TC.InvoiceOrPayment='Others' ";
                var data = _sqlRepository.GetCombo(sql, "Id", "Text");
                if (null == data)
                    throw new CustomException(ResourcesCore.FYNotFound);
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetTaxCodeOutputVATGST(DateTime postingDate, string companyId)
        {
            try
            {
                var sql = @"SELECT DISTINCT TC.Id, TC.UserName AS Text, TC.IsWithhold, TC.IsCreditable, TC.IsMerge
                        FROM [MST].[TaxCodeYear] AS TCY
                        LEFT JOIN [MST].[TaxCode] AS TC ON TC.Id = TCY.TaxCodeId
                        LEFT JOIN [MST].[TaxCodeGL] AS TCGL ON TC.Id = TCGL.TaxCodeId
					    LEFT JOIN [SCS].[TaxYear] AS TY ON TY.Id=TCY.TaxYearId
						LEFT JOIN [SCS].[TaxYearPeriod] AS TYP ON TYP.TaxYearId=TY.Id
                        LEFT JOIN MST.TaxCategory TCT ON TCT.Id=TC.TaxCategoryId
                        LEFT JOIN [ORG].Company AS CO ON CO.COAId=TCGL.COAId
                        WHERE TC.InputOrOutput='" + TaxCodeInputOutput.Output + @"' AND TCT.TaxCategoryType IN ('GST','VAT')
						AND TYP.StartDate <='" + postingDate.ToDbDate() + "' AND TYP.EndDate >='" + postingDate.ToDbDate() + "' AND CO.Id='" + companyId + "'";
                var data = _sqlRepository.GetCombo(sql, "Id", "Text");
                if (null == data)
                    throw new CustomException(ResourcesCore.FYNotFound);
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetGLListByCOA(GridParameter parameters, string coaId, AccountTypeEnum accountType, ReconcileAccountEnum glAccountType)
        {
            try
            {
                parameters.CmdText = @"SELECT GLGI.COAId, C.UserName AS COAName, GLGI.AccountGroupId, AG.Code AS AccountGroupCode, AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName--, BM.BudgetMasterId, BM.BudgetCode, BM.BudgetName, BMA.ActivityId, BMA.ActivityCode, BMA.ActivityName
                                        FROM HKP.[GLGeneralInfo] AS GLGI
                                        JOIN HKP.[GLCompanyGroup] AS GLCG ON GLCG.GLGeneralInfoId=GLGI.Id
                                        LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                        LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                                        LEFT JOIN [HKP].[GLAccountType] AS GLAT ON GLAT.GLGeneralInfoId=GLGI.Id
                                        LEFT JOIN [HKP].[COA] AS C ON C.Id=GLGI.COAId
                                        --LEFT JOIN (
	                                        --SELECT BM.GLGeneralInfoId, BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName FROM [MST].[BudgetMaster] AS BM
	                                        --LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        --)AS BM ON BM.GLGeneralInfoId=GLGI.Id
                                        --LEFT JOIN (
	                                        --SELECT BMA.BudgetMasterId, A.Id AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName FROM [MST].[BudgetMasterActivity] AS BMA
	                                        --LEFT JOIN [HKP].[Activity] AS A ON A.Id=BMA.ActivityId
                                        --) AS BMA ON BMA.BudgetMasterId=BM.BudgetMasterId
                                        WHERE GLGI.Archive=0 AND GLGI.Active=1 AND GLGI.COAId='" + coaId + "' AND ACT.Id='" + accountType + "' AND GLAT.AccountType='" + glAccountType + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetReconeGLPartyAccountGroup(GridParameter parameters, string coaId, AccountTypeEnum accountType, ReconcileAccountEnum glAccountType)
        {
            try
            {
                parameters.CmdText = @"SELECT GLGI.COAId, C.UserName AS COAName, GLGI.AccountGroupId, AG.Code AS AccountGroupCode, AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId
                                        , GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName, BM.BudgetMasterId, BM.BudgetCode, BM.BudgetName, BMA.ActivityId, BMA.ActivityCode, BMA.ActivityName
                                        FROM HKP.[GLGeneralInfo] AS GLGI
                                        JOIN HKP.[GLCompanyGroup] AS GLCG ON GLCG.GLGeneralInfoId=GLGI.Id
                                        LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                        LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                                        LEFT JOIN [HKP].[GLAccountType] AS GLAT ON GLAT.GLGeneralInfoId=GLGI.Id
                                        LEFT JOIN [HKP].[COA] AS C ON C.Id=GLGI.COAId
                                        LEFT JOIN (
	                                        SELECT BM.GLGeneralInfoId, BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName FROM [MST].[BudgetMaster] AS BM
	                                        LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                        )AS BM ON BM.GLGeneralInfoId=GLGI.Id
                                        LEFT JOIN (
	                                        SELECT BMA.BudgetMasterId, A.Id AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName FROM [MST].[BudgetMasterActivity] AS BMA
	                                        LEFT JOIN [HKP].[Activity] AS A ON A.Id=BMA.ActivityId
                                        ) AS BMA ON BMA.BudgetMasterId=BM.BudgetMasterId
                                        WHERE GLGI.Archive=0 AND GLGI.Active=1 AND GLGI.COAId='" + coaId + "' AND ACT.Id='" + accountType + "' AND GLAT.AccountType='" + glAccountType + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetExpensesBookingCbo(string companyGroupId, string companyId)
        {
            try
            {
                var sql = @"SELECT ETT.Id AS EmployeeTransactionTypeId, ETT.UserName AS EmployeeTransactionTypeName
                            , EGL.AdvanceGLId, EGL.AdvanceGLCode, EGL.AdvanceGLName
                            , EGL.AdvanceBudgetMasterId, EGL.AdvanceBudgetCode, EGL.AdvanceBudgetName
                            , EGL.AdvanceActivityId, EGL.AdvanceActivityCode, EGL.AdvanceActivityName
                            , EGL.PayableGLId, EGL.PayableGLCode, EGL.PayableGLName
                            , EGL.PayableBudgetMasterId, EGL.PayableBudgetCode, EGL.PayableBudgetName
                            , EGL.PayableActivityId, EGL.PayableActivityCode, EGL.PayableActivityName
                            , ETT.AdvanceType,EGL.PayableBudgetActive,EGL.PayableBudgetMasterActivityActive,EGL.AdvanceBudgetActive,EGL.AdvanceBudgetMasterActivityActive
                            FROM [HKP].[EmployeeTransactionType] ETT
                            LEFT JOIN(
	                        SELECT ETTGL.EmployeeTransactionTypeId, ETTGL.AdvanceGLId, AGGI.AccountCode AS AdvanceGLCode, AGGI.UserName AS AdvanceGLName
	                        , ETTGL.AdvanceBudgetMasterId, AB.Code AS AdvanceBudgetCode, AB.UserName AS AdvanceBudgetName
	                        , ETTGL.AdvanceActivityId, AA.Code AS AdvanceActivityCode, AA.UserName AS AdvanceActivityName
	                        , ETTGL.PayableGLId, PGGI.AccountCode AS PayableGLCode, PGGI.UserName AS PayableGLName
	                        , ETTGL.PayableBudgetMasterId, PB.Code AS PayableBudgetCode, PB.UserName AS PayableBudgetName
	                        , ETTGL.PayableActivityId, PA.Code AS PayableActivityCode, PA.UserName AS PayableActivityName, ETTGL.IsExpensesBooking
                            ,PBM.Active PayableBudgetActive,BMA.Active PayableBudgetMasterActivityActive,ABM.Active AdvanceBudgetActive,ABMA.Active AdvanceBudgetMasterActivityActive
	                        FROM [HKP].[EmployeeTransactionTypeGL] AS ETTGL
	                        LEFT JOIN [HKP].[GLGeneralInfo] AS AGGI ON AGGI.Id=ETTGL.AdvanceGLId
	                        LEFT JOIN [MST].[BudgetMaster] AS ABM ON ABM.Id=ETTGL.AdvanceBudgetMasterId
	                        LEFT JOIN [HKP].[Budget] AS AB ON AB.Id=ABM.BudgetId
	                        LEFT JOIN [HKP].[Activity] AS AA ON AA.Id=ETTGL.AdvanceActivityId
	                        LEFT JOIN [HKP].[GLGeneralInfo] AS PGGI ON PGGI.Id=ETTGL.PayableGLId
	                        LEFT JOIN [MST].[BudgetMaster] AS PBM ON PBM.Id=ETTGL.PayableBudgetMasterId
	                        LEFT JOIN [HKP].[Budget] AS PB ON PB.Id=PBM.BudgetId
	                        LEFT JOIN [HKP].[Activity] AS PA ON PA.Id=ETTGL.PayableActivityId
							LEFT JOIN [MST].[BudgetMasterActivity] BMA ON BMA.BudgetMasterId=PBM.Id AND BMA.ActivityId=PA.Id
                            LEFT JOIN [MST].[BudgetMasterActivity] ABMA ON ABMA.BudgetMasterId=ABM.Id AND ABMA.ActivityId=AA.Id
	                        LEFT JOIN [ORG].[Company] AS C ON C.COAId=ETTGL.COAId
	                        WHERE C.Id='" + companyId + @"'
                        )AS EGL ON EGL.EmployeeTransactionTypeId=ETT.Id
                        WHERE ETT.Active=1 AND ETT.CompanyGroupId='" + companyGroupId + "' AND EGL.IsExpensesBooking=1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        public GridModel GetRevenueExpenseGLBudget(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT  BM.GLGeneralInfoId GLGeneralInfoId,BM.Id BudgetMasterId,
						 AG.UserName AS AccountGroupName, GLGI.AccountCode AS GLGeneralInfoCode
						 , GLGI.UserName AS GLGeneralInfoName, BM.RefNo, B.Code BudgetCode, B.UserName BudgetName
						FROM [MST].[BudgetMaster] AS BM 
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLGI ON BM.GLGeneralInfoId=GLGI.Id
						LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
						 LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
						WHERE ACT.Id IN ('" + AccountTypeEnum.Revenue + @"','" + AccountTypeEnum.Expense + "')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetCurrentAssetRevenueExpenseGLBudget(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT  BM.GLGeneralInfoId GLGeneralInfoId,BM.Id BudgetMasterId,
						 AG.UserName AS AccountGroupName, GLGI.AccountCode AS GLGeneralInfoCode
						 , GLGI.UserName AS GLGeneralInfoName, BM.RefNo, B.Code BudgetCode, B.UserName BudgetName
						FROM [MST].[BudgetMaster] AS BM 
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLGI ON BM.GLGeneralInfoId=GLGI.Id
						LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
						LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                        LEFT JOIN [HKP].[GLAccountType] AS GLAT ON GLAT.GLGeneralInfoId=GLGI.Id
						WHERE ACT.Id IN ('" + AccountTypeEnum.Revenue + @"','" + AccountTypeEnum.Expense + "') OR GLAT.AccountType='"+ ReconcileAccountEnum.Employee.ToString() + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        public GridModel GetExpensesGLBudgetActivityCOAWise(GridParameter parameters, string coaId)
        {
            try
            {
                parameters.CmdText = @"SELECT  C.Id AS COAId, AG.UserName AS AccountGroupName, C.UserName AS COAName
		                            , GLGI.UserName AS GLGeneralInfoName, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.Id AS GLGeneralInfoId
                                    , BM.BudgetId, B.UserName BudgetName, BM.RefNo
                                    , BMA.ActivityId, A.UserName ActivityName, BMA.BudgetMasterId
		                            FROM [MST].[BudgetMasterActivity] BMA
									LEFT JOIN [MST].[BudgetMaster] AS BM ON BMA.BudgetMasterId=BM.Id
									LEFT JOIN  HKP.Budget AS B ON BM.BudgetId=B.Id
									LEFT JOIN  HKP.Activity AS A ON BMA.ActivityId=A.Id
									LEFT JOIN HKP.GLGeneralInfo AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
									JOIN HKP.COA AS C ON C.Id=GLGI.COAId
		                            LEFT OUTER JOIN HKP.GLAccountType AS GLAT ON GLAT.GLGeneralInfoId = GLGI.Id
		                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id = GLGI.AccountGroupId
									LEFT JOIN HKP.AccountType AS ACT ON ACT.Id =AG.AccountTypeId
                                        WHERE GLGI.COAId = '" + coaId + @"' AND ACT.Id IN ('" + AccountTypeEnum.Expense + "')";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetAllLiabilityGLBudgetActivity(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , B.BudgetMasterId, B.RefNo, B.BudgetCode, B.BudgetName, A.ActivityId, A.ActivityCode, A.ActivityName, GLTY.AccountType
                                    FROM [HKP].[GLGeneralInfo] AS GLGI
                                    LEFT JOIN [HKP].[GLCompanyGroup] AS GLCG ON GLCG.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLAccountType] AS GLTY ON GLTY.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                    LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                                    LEFT JOIN (SELECT BM.Id AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName, BM.GLGeneralInfoId, BM.RefNo
	                                    FROM [HKP].[Budget] AS B
                                        LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.BudgetId=B.Id
                                    ) AS B ON B.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN (SELECT A.Id AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName, BA.BudgetMasterId
	                                    FROM [HKP].[Activity] AS A
	                                    LEFT JOIN [MST].[BudgetMasterActivity] AS BA ON BA.ActivityId=A.Id
                                    ) AS A ON A.BudgetMasterId=B.BudgetMasterId
                                    WHERE GLGI.Archive=0 AND GLGI.Active=1 AND GLCG.CompanyGroupId='" + companyGroupId + "' AND GLCI.CompanyId='" + companyId + @"' AND ACT.Id='" + AccountTypeEnum.Liability + @"'
                                    AND GLGI.Id NOT IN(SELECT BM.GLGeneralInfoId FROM [MST].[BankMaster] AS BM WHERE BM.GLGeneralInfoId <> '')
                                    AND GLGI.Id NOT IN(SELECT CM.GLGeneralInfoId FROM [MST].[CashMaster] AS CM WHERE CM.GLGeneralInfoId <> '') AND GLGI.IsPostingAutomaticOnly = 0
                                    AND GLGI.Id NOT IN(SELECT GLGeneralInfoId FROM [HKP].[GLAccountType] ) ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetTrailBalanceRoundOffList(string plantId, string trnType)
        {
            try
            {
                var sql = @"Select * from ( SELECT x.Particulars,x.GL,x.Budget,x.Activity,x.MainHead,x.BalanceType
                            ,TrnType=case when (case when x.CRcumulative<0 then abs(x.CRcumulative) when x.DRcumulative>0 then  x.DRcumulative  else 0 end)>0 then 'Cr' else 'Dr' end
                            ,DrAmount=case  when x.DRcumulative<0 then abs(x.DRcumulative) when x.CRcumulative>0 then  x.CRcumulative else 0 end
                            ,CrAmount=case when x.CRcumulative<0 then abs(x.CRcumulative) when x.DRcumulative>0 then  x.DRcumulative  else 0 end
                            ,x.GLGeneralInfoId,x.BudgetMasterId,x.ActivityId,X.PartyId,0 Active,X.PartyType,X.PartyPlantId
                            FROM
                            ( SELECT distinct	
		                         sum(CASE WHEN ACT.BalanceType = 'Debit' THEN (sum(VDC.DrAmount)-sum(VDC.CrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId, A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as DRcumulative--, VD.PartyPlantId
                                , sum(CASE WHEN ACT.BalanceType = 'Credit' THEN (sum(VDC.CrAmount)-sum(VDC.DrAmount)) ELSE 0 END) over (partition by GL.Id, VD.BudgetMasterId,A.Id,VD.BankMasterId,VD.CashMasterId, VD.PartyId, VD.PartyPlantId, VDC.ParallelCurrencyId order by VDC.ParallelCurrencyId) as CRcumulative ,--, VD.PartyPlantId
                                            ACT.BalanceType,
                                            ACT.Id AS [MainHead],
		                                    VD.GLGeneralInfoId,GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode,
                                            VD.BudgetMasterId,
		                                    BUD.UserName AS Budget,
											A.UserName AS Activity,
											[Particulars]=CASE 
											WHEN BA.AccountTitle<>'' THEN BA.AccountTitle
											WHEN CM.UserName<>'' THEN CM.UserName
											WHEN P.UserName<>'' THEN P.UserName
											ELSE ''	END
                                            ,PartyType=CASE WHEN CPC.PartyType='Customer' AND CPV.PartyType IN ('Vendor') THEN 'Both' WHEN CPC.PartyType<>'' THEN CPC.PartyType ELSE CPV.PartyType END
                                            ,A.Id AS ActivityId , VD.PartyId, VD.PartyPlantId
	                                        FROM TRN.VoucherDetailCurrency AS VDC
		                                    INNER JOIN TRN.VoucherDetail AS VD ON VD.Id =VDC.VoucherDetailId
		                                    INNER JOIN TRN.Voucher AS V ON V.Id=VD.VoucherId
		                                    LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=VD.GLGeneralInfoId
                                            LEFT OUTER JOIN HKP.AccountGroup AS AG ON AG.Id=GL.AccountGroupId
                                            LEFT OUTER JOIN [HKP].[AccountType] act on act.Id =AG.AccountTypeId
                                            LEFT JOIN SCS.Currency AS CU ON CU.Id=VDC.ParallelCurrencyId
											LEFT JOIN MST.BudgetMaster BM ON VD.BudgetMasterId=BM.Id
                                            LEFT JOIN [HKP].[Budget] AS BUD ON BM.BudgetId=BUD.Id
											LEFT JOIN HKP.Activity A ON VD.ActivityId=A.Id
											LEFT JOIN [MST].BankMaster AS BA ON BA.Id=VD.BankMasterId
											LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
											LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
											LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                                            LEFT JOIN [HKP].[CompanyParty] AS CPC ON CPC.PartyId=P.Id AND CPC.PartyType IN ('Customer') AND CPC.PlantId='" + plantId + @"'
											LEFT JOIN [HKP].[CompanyParty] AS CPV ON CPV.PartyId=P.Id AND CPV.PartyType IN ('Vendor')  AND CPV.PlantId='" + plantId + @"'
                                            WHERE  V.PlantId='" + plantId + @"' 
                                            AND  v.IsPark=0 and vd.PartyId<>''
                                            GROUP BY GL.Id, GL.AccountCode, VDC.ParallelCurrencyId, CU.Code, VD.GLGeneralInfoId, GL.UserName, 
											GL.AccountCode, ACT.BalanceType, ACT.Id, VD.BudgetMasterId, A.UserName, BUD.UserName, v.PostingDate, A.Id, BA.AccountTitle, CM.UserName
											,VD.BankMasterId, VD.CashMasterId, P.UserName, VD.PartyId,VD.PartyPlantId,CPC.PartyType,CPV.PartyType ) x 
                                            WHERE
											  (ISNULL(DRcumulative,0.00) between   -2 and 2   and ISNULL(DRcumulative,0.00) <> 0.00) OR 
											  (ISNULL(CRcumulative,0.00) <> 0 and ISNULL(CRcumulative,0.00) between   -2 and 2 )) tb  where tb.TrnType='" + trnType+@"'
											  ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetInvoiceRoundOffList(string companyId,string plantId, string trnType)
        {
            try
            {
                var sql = @"Select * from (
                    SELECT    V.VoucherNo+' '+P.UserName Particulars,IVD.GL,IVD.Budget,IVD.Activity,'Asset' MainHead,'Debit' BalanceType,'Cr' TrnType
					,0 DrAmount,CrAmount=ISNULL(IVD.InvoiceBooksAmount,0)-ISNULL(IVD.SetOffBooksAmount,0),IVD.GLGeneralInfoId,IVD.BudgetMasterId,ivd.ActivityId,IVD.BudgetMasterActivityId
						,IV.PartyType,IV.PartyId,IV.PartyPlantId,IVD.InvoiceId,IVD.InvoiceDetailId				
                                        FROM  [TRN].[Invoice] AS IV 
										 JOIN (select IDE.InvoiceId,IDE.Id InvoiceDetailId,VD.PartyId,SUM(VDC.DrAmount) InvoiceBooksAmount ,SUM(IwV.SetOffBooksAmount) SetOffBooksAmount
										 ,GL.UserName GL ,B.UserName Budget,AC.UserName Activity,VD.GLGeneralInfoId,VD.BudgetMasterId,vd.ActivityId,bma.Id BudgetMasterActivityId
											FROM  [TRN].[InvoiceDetail] IDE
						LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceDetailId=IDE.Id
						LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
						LEFT JOIN [TRN].[Voucher] AS VI ON VI.Id=VD.VoucherId
						LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=VD.GLGeneralInfoId
						LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
						LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
						LEFT JOIN HKP.Activity AC ON AC.Id=VD.ActivityId
						LEFT JOIN MST.BudgetMasterActivity BMA ON BMA.BudgetMasterId=BM.Id AND BMA.ActivityId=AC.Id
						LEFT JOIN (SELECT iwd.InvoiceDetailId,iwd.PartyId
								,SUM(VDC.CrAmount) SetOffBooksAmount
								FROM  [TRN].[InvoiceWriteOffDetail] iwd 
								JOIN TRN.InvoiceWriteOff iw on iw.Id=iwd.InvoiceWriteOffId 
								LEFT JOIN TRN.VoucherDetail VD ON VD.InvoiceWriteOffDetailId=iwd.Id
								LEFT JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id
									JOIN TRN.Voucher WV ON WV.Id=VD.VoucherId
								GROUP BY iwd.InvoiceDetailId,iwd.PartyId
								)AS IwV ON IwV.InvoiceDetailId=IDE.Id AND VD.PartyId=IwV.PartyId
							WHERE VI.IsPark=0 and VD.PartyType='Customer'
							GROUP BY IDE.InvoiceId,VD.PartyId,GL.UserName,IDE.Id
						 ,B.UserName ,AC.UserName ,VD.GLGeneralInfoId,VD.BudgetMasterId,vd.ActivityId,bma.Id 
										) AS IVD ON IVD.InvoiceId=IV.Id AND IVD.PartyId=IV.PartyId
						    LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
                            LEFT JOIN [TRN].[Voucher] AS V ON V.Id=IV.VoucherId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
                            LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
                            WHERE IV.Archive=0 AND V.IsPark=0  AND IV.SourceType in ('CustomerInvoice','CustomerBanksReceipt','CustomerReceipt','SalesInvoice','InventorySales')
							AND ISNULL(IVD.InvoiceBooksAmount,0)-ISNULL(IVD.SetOffBooksAmount,0)>0
                             AND IV.PlantId='" + plantId + @"'  AND IV.IsWrittenOff=1

						UNION ALL
                        SELECT        V.VoucherNo+' '+P.UserName Particulars,GL.UserName GL,B.UserName Budget,AC.UserName Activity,'Asset' MainHead,'Debit' BalanceType,'Cr' TrnType
					    ,0 DrAmount,CrAmount=ISNULL(IVD.Amount * CC.CompanyCurrencyRate,0) - ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0),VD.GLGeneralInfoId,VD.BudgetMasterId,vd.ActivityId,VD.BudgetMasterActivityId
                        ,IV.PartyType,IV.PartyId,IV.PartyPlantId,NULL InvoiceId,NULL InvoiceDetailId	
                            FROM [TRN].[AdjustmentNoteDetail] AS IVD
							LEFT JOIN [TRN].[AdjustmentNote] AS IV ON IVD.AdjustmentNoteId=IV.Id
							LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
							LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=IVD.Id
							LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
							LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=VD.GLGeneralInfoId
								LEFT JOIN MST.BudgetMaster BM ON BM.Id=VD.BudgetMasterId
								LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
								LEFT JOIN HKP.Activity AC ON AC.Id=VD.ActivityId
							LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
							LEFT JOIN (select SUM(ISNULL(VDCW.CrAmount,0))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
									INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
									INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
									INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
									where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
									GROUP BY  IWD.AdjustmentNoteId)W ON W.AdjustmentNoteId=IVD.AdjustmentNoteId
							LEFT JOIN (
							SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
							VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
							FROM [TRN].[VoucherDetailCurrency] AS VDC
							JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
							WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"'
						) AS CC ON CC.VoucherDetailId=VD.Id
						
                            WHERE IV.Archive=0 AND V.IsPark=0  AND IV.PartyType='Customer' AND IV.SourceType in ('DebitNote','CustomerReceipt')
							AND ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0)-ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)>0
                            AND IV.PlantId='"+ plantId + @"'
                           and iv.IsWrittenOff=1
						) x
where x.TrnType='"+ trnType + @"'
";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel GetIssuePostingGLBudgetActivityList(GridParameter parameters, string companyGroupId, string companyId, AccountTypeEnum accountTypeEnum)
        {
            try
            {
                parameters.CmdText = @"SELECT AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , BMA.BudgetMasterId, BM.RefNo, B.Code BudgetCode, B.UserName BudgetName, BMA.ActivityId, A.Code ActivityCode, A.UserName ActivityName, GLTY.AccountType
									,BMA.Active
                                    FROM [MST].[BudgetMasterActivity] BMA
									LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=BMA.BudgetMasterId
									LEFT JOIN [HKP].[Budget] B ON B.Id=BM.BudgetId
									LEFT JOIN [HKP].[Activity] A ON A.Id=BMA.ActivityId
									LEFT JOIN  [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [HKP].[GLCompanyGroup] AS GLCG ON GLCG.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLAccountType] AS GLTY ON GLTY.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                    LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                                    WHERE GLGI.Archive=0 AND GLGI.Active=1 AND GLCG.CompanyGroupId='" + companyGroupId + "' AND GLCI.CompanyId='" + companyId + "' AND ACT.Id='" + accountTypeEnum + @"'
                                    UNION ALL
                                    SELECT AG.UserName AS AccountGroupName, GLGI.Id AS GLGeneralInfoId, GLGI.AccountCode AS GLGeneralInfoCode, GLGI.UserName AS GLGeneralInfoName
                                    , BMA.BudgetMasterId, BM.RefNo, B.Code BudgetCode, B.UserName BudgetName, BMA.ActivityId, A.Code ActivityCode, A.UserName ActivityName, GLTY.AccountType
									,BMA.Active
                                    FROM [MST].[BudgetMasterActivity] BMA
									LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=BMA.BudgetMasterId
									LEFT JOIN [HKP].[Budget] B ON B.Id=BM.BudgetId
									LEFT JOIN [HKP].[Activity] A ON A.Id=BMA.ActivityId
									LEFT JOIN  [HKP].[GLGeneralInfo] AS GLGI ON GLGI.Id=BM.GLGeneralInfoId
                                    LEFT JOIN [HKP].[GLCompanyGroup] AS GLCG ON GLCG.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLCompanyInfo] AS GLCI ON GLCI.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[GLAccountType] AS GLTY ON GLTY.GLGeneralInfoId=GLGI.Id
                                    LEFT JOIN [HKP].[AccountGroup] AS AG ON AG.Id=GLGI.AccountGroupId
                                    LEFT JOIN [HKP].[AccountType] AS ACT ON ACT.Id=AG.AccountTypeId
                                    WHERE GLGI.Archive=0 AND GLGI.Active=1 AND GLCG.CompanyGroupId='" + companyGroupId + "' AND GLCI.CompanyId='" + companyId + @"' AND GLTY.AccountType='Material'
";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
    }
}
