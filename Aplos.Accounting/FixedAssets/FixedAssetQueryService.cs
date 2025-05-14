using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.FixedAssets;
using Library.Service.Enums;
using Library.Service.FixedAssets;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading;
using OTSBD;
using Library.Accounting.Accounts;
using System.Linq;

namespace Library.Accounting.FixedAssets
{
    public class FixedAssetQueryService
    {
        private readonly ISqlRepository _sqlRepository;
        public FixedAssetQueryService(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;

        }


        public IEnumerable<object> GetFixedAssetList(string masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT m.Id, m.Id AssetNo, m.BrandId, m.CountryOfOriginId, M.MaterialMasterId, M.MaterialMasterArticleId, A.UserName AS MaterialMasterName
	                        , A.BaseUOMId, UOM.UserName AS BaseUOMName, p.UserName Vendor, m.SerialNo, m.InvoiceNo
                            , m.IsFinancial, Replace(CONVERT(VARCHAR(11), m.InvoiceDate, 106), ' ', '-') InvoiceDate
	                        , m.IsForProduction, m.Model, m.VendorId, M.PlantId, M.CurrencyId, M.CompanyId, M.CompanyGroupId, m.FABaseCurrencyId
	                        , m.FAGroupCurrencyId, m.FAHardCurrencyId, m.ADBaseCurrencyId, m.ADGroupCurrencyId, m.ADHardCurrencyId
	                        , m.FABaseAmount +ISNULL(SA.Amount,0) FABaseAmount, m.FAGroupAmount, m.FAHardAmount, m.ADBaseAmount, m.ADGroupAmount, m.ADHardAmount
	                        , m.[LifeTime], Replace(CONVERT(VARCHAR(11), m.CapitalizationDate, 106), ' ', '-') CapitalizationDate
                            , m.Price, m.Price PurchasePrice,m.Price TotalPrice, fam.UserName 'AssetMasterName', m.YearOfManufacture
	                        , m.YearOfInstallation, m.[Description]
	                        , FAMT.FixedAssetMasterId, fam.UserName 'FixedAssetMasterName'
	                        , c.UserName FixedAssetCategory, sc.UserName FixedAssetSubCategory
	                        , BM.GLGeneralInfoId AS AssetGLId, GL.UserName AssetGLName
	                        , A.BudgetMasterId AS AssetBudgetMasterId, B.UserName AssetBudgetName
	                        , A.ActivityId AS AssetActivityId, AC.UserName AssetActivityName
	                        , cn.UserName Country, fam.AssetType, MMA.StandardName Article,m.Remarks,m.LCNumber,m.Quantity
	                        ,m.DepreciationRuleId,cast (m.MultiplicationFactor as varchar)MultiplicationFactor
                            FROM  TRN.[FixedAssetRegister]  m
                            LEFT JOIN MST.MaterialMaster A ON M.MaterialMasterId=A.Id
                            LEFT JOIN MST.MaterialMasterArticle MMA ON m.MaterialMasterArticleId= MMA.Id
                            LEFT JOIN SCS.UnitOfMeasurement UOM ON A.BaseUOMId = UOM.Id
                            LEFT JOIN SCS.[Country] cn ON cn.Id = m.CountryOfOriginId
                            LEFT join HKP.[Party]  p on p.Id=m.VendorId
                            LEFT JOIN [ORG].[Plant] PL ON M.PlantId = PL.Id
                            LEFT JOIN [ORG].Company CO ON M.CompanyId = CO.Id
                            LEFT JOIN MST.BudgetMaster BM ON A.BudgetMasterId = BM.Id
                            LEFT JOIN HKP.Budget B ON  BM.BudgetId=B.Id
                            LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMT ON BM.Id=FAMT.BudgetMasterId AND A.BudgetMasterId=FAMT.BudgetMasterId
                            LEFT JOIN MST.FixedAssetMaster fam ON FAMT.FixedAssetMasterId=fam.Id
                            LEFT JOIN HKP.[FixedAssetCategory]  c ON c.Id = fam.FixedAssetCategoryId
                            LEFT JOIN HKP.[FixedAssetSubCategory]  sc ON sc.Id = fam.FixedAssetSubCategoryId
                            LEFT JOIN HKP.GLGeneralInfo GL ON BM.GLGeneralInfoId=GL.Id
                            LEFT JOIN HKP.Activity AC ON A.ActivityId=AC.Id
                            LEFT JOIN (SELECT FixedAssetRegisterId,SUM(Amount) Amount FROM TRN.SubFixedAssetRegister GROUP BY FixedAssetRegisterId ) SA ON SA.FixedAssetRegisterId=m.Id
                            WHERE m.CompanyId = '" + identity.CompanyId + "' and m.Id='" + masterId + @"'  and m.Archive=0
                            Order by c.UserName, sc.UserName, m.SerialNo, m.InvoiceDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetFixedAssetListJV(string masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT m.Id, m.Id AssetNo, m.BrandId, m.CountryOfOriginId, M.MaterialMasterId, M.MaterialMasterArticleId, A.UserName AS MaterialMasterName
	                        , A.BaseUOMId, UOM.UserName AS BaseUOMName, p.UserName Vendor, m.SerialNo, m.InvoiceNo
                            , m.IsFinancial, Replace(CONVERT(VARCHAR(11), m.InvoiceDate, 106), ' ', '-') InvoiceDate
	                        , m.IsForProduction, m.Model, m.VendorId, M.PlantId, M.CurrencyId, M.CompanyId, M.CompanyGroupId, m.FABaseCurrencyId
	                        , m.FAGroupCurrencyId, m.FAHardCurrencyId, m.ADBaseCurrencyId, m.ADGroupCurrencyId, m.ADHardCurrencyId
	                        , m.FABaseAmount +ISNULL(SA.Amount,0) FABaseAmount, m.FAGroupAmount, m.FAHardAmount, m.ADBaseAmount, m.ADGroupAmount, m.ADHardAmount
	                        , m.[LifeTime], Replace(CONVERT(VARCHAR(11), m.CapitalizationDate, 106), ' ', '-') CapitalizationDate
                            , m.Price, m.Price PurchasePrice,m.FABaseAmount TotalPrice,m.FABaseAmount UnitPrice, fam.UserName 'AssetMasterName', m.YearOfManufacture
	                        , m.YearOfInstallation, m.[Description]
	                        , FAMT.FixedAssetMasterId, fam.UserName 'FixedAssetMasterName'
	                        , c.UserName FixedAssetCategory, sc.UserName FixedAssetSubCategory
	                        , BM.GLGeneralInfoId AS AssetGLId, GL.UserName AssetGLName
	                        , A.BudgetMasterId AS AssetBudgetMasterId, B.UserName AssetBudgetName
	                        , A.ActivityId AS AssetActivityId, AC.UserName AssetActivityName
	                        , cn.UserName Country, fam.AssetType, MMA.StandardName Article,m.Remarks,m.LCNumber,m.Quantity
	                        ,m.DepreciationRuleId ,cast( m.MultiplicationFactor as varchar)MultiplicationFactor
                            FROM  TRN.[FixedAssetRegister]  m
                            LEFT JOIN MST.MaterialMaster A ON M.MaterialMasterId=A.Id
                            LEFT JOIN MST.MaterialMasterArticle MMA ON m.MaterialMasterArticleId= MMA.Id
                            LEFT JOIN SCS.UnitOfMeasurement UOM ON A.BaseUOMId = UOM.Id
                            LEFT JOIN SCS.[Country] cn ON cn.Id = m.CountryOfOriginId
                            LEFT join HKP.[Party]  p on p.Id=m.VendorId
                            LEFT JOIN [ORG].[Plant] PL ON M.PlantId = PL.Id
                            LEFT JOIN [ORG].Company CO ON M.CompanyId = CO.Id
                            LEFT JOIN MST.BudgetMaster BM ON A.BudgetMasterId = BM.Id
                            LEFT JOIN HKP.Budget B ON  BM.BudgetId=B.Id
                            LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMT ON BM.Id=FAMT.BudgetMasterId AND A.BudgetMasterId=FAMT.BudgetMasterId
                            LEFT JOIN MST.FixedAssetMaster fam ON FAMT.FixedAssetMasterId=fam.Id
                            LEFT JOIN HKP.[FixedAssetCategory]  c ON c.Id = fam.FixedAssetCategoryId
                            LEFT JOIN HKP.[FixedAssetSubCategory]  sc ON sc.Id = fam.FixedAssetSubCategoryId
                            LEFT JOIN HKP.GLGeneralInfo GL ON BM.GLGeneralInfoId=GL.Id
                            LEFT JOIN HKP.Activity AC ON A.ActivityId=AC.Id
                            LEFT JOIN (SELECT FixedAssetRegisterId,SUM(Amount) Amount FROM TRN.SubFixedAssetRegister GROUP BY FixedAssetRegisterId ) SA ON SA.FixedAssetRegisterId=m.Id
                            WHERE m.CompanyId = '" + identity.CompanyId + "' and m.Id='" + masterId + @"'  and m.Archive=0
                            Order by c.UserName, sc.UserName, m.SerialNo, m.InvoiceDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetCapitalizedFixedAssetRegister(string masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT FAR.Id, FAR.Id AssetNo, FAR.BrandId, FAR.CountryOfOriginId, FAR.MaterialMasterId, FAR.MaterialMasterArticleId, A.UserName AS MaterialMasterName
	                        , A.BaseUOMId, UOM.UserName AS BaseUOMName, p.UserName Vendor, FAR.SerialNo, FAR.InvoiceNo
                            , FAR.IsFinancial, Replace(CONVERT(VARCHAR(11), FAR.InvoiceDate, 106), ' ', '-') InvoiceDate
	                        , FAR.IsForProduction, FAR.Model, FAR.VendorId, FAR.PlantId, FAR.CurrencyId, FAR.CompanyId, FAR.CompanyGroupId, FAR.FABaseCurrencyId
	                        , FAR.FAGroupCurrencyId, FAR.FAHardCurrencyId, FAR.ADBaseCurrencyId, FAR.ADGroupCurrencyId, FAR.ADHardCurrencyId
	                        , FAR.FABaseAmount,  FAR.FAGroupAmount, FAR.FAHardAmount, FAR.ADBaseAmount, FAR.ADGroupAmount, FAR.ADHardAmount
	                        , FAR.[LifeTime], Replace(CONVERT(VARCHAR(11), FAR.CapitalizationDate, 106), ' ', '-') CapitalizationDate
                            , FAR.Price, FAR.Price TotalGRNAmount,FAR.FABaseAmount TotalPrice
							, fam.UserName 'AssetMasterName', FAR.YearOfManufacture
	                        , FAR.YearOfInstallation, FAR.[Description]
	                        , FAMT.FixedAssetMasterId, fam.UserName 'FixedAssetMasterName'
	                        , c.UserName FixedAssetCategory, sc.UserName FixedAssetSubCategory
	                        , BM.GLGeneralInfoId AS AssetGLId, GL.UserName AssetGLName
	                        , A.BudgetMasterId AS AssetBudgetMasterId, B.UserName AssetBudgetName
	                        , A.ActivityId AS AssetActivityId, AC.UserName AssetActivityName
	                        , cn.UserName Country, fam.AssetType, MMA.StandardName Article,FAR.Remarks
                            , CG.Code GRNCurrencyCode,CB.Code CurrencyCode ,FAR.LCNumber,FAR.Quantity NumberOfQuantity,FAR.CapitalizeRegisterNo,V.VoucherNo
	                        ,FAR.DepreciationRuleId,cast( FAR.MultiplicationFactor as varchar)MultiplicationFactor
                            FROM  TRN.[FixedAssetRegister]  FAR
                            LEFT JOIN MST.MaterialMaster A ON FAR.MaterialMasterId=A.Id
                            LEFT JOIN MST.MaterialMasterArticle MMA ON FAR.MaterialMasterArticleId= MMA.Id
                            LEFT JOIN SCS.UnitOfMeasurement UOM ON A.BaseUOMId = UOM.Id
                            LEFT JOIN SCS.[Country] cn ON cn.Id = FAR.CountryOfOriginId
                            LEFT join HKP.[Party]  p on p.Id=FAR.VendorId
                            LEFT JOIN [ORG].[Plant] PL ON FAR.PlantId = PL.Id
                            LEFT JOIN [ORG].Company CO ON FAR.CompanyId = CO.Id
                            LEFT JOIN MST.BudgetMaster BM ON A.BudgetMasterId = BM.Id
                            LEFT JOIN HKP.Budget B ON  BM.BudgetId=B.Id
                            LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMT ON BM.Id=FAMT.BudgetMasterId AND A.BudgetMasterId=FAMT.BudgetMasterId
                            LEFT JOIN MST.FixedAssetMaster fam ON FAMT.FixedAssetMasterId=fam.Id
                            LEFT JOIN HKP.[FixedAssetCategory]  c ON c.Id = fam.FixedAssetCategoryId
                            LEFT JOIN HKP.[FixedAssetSubCategory]  sc ON sc.Id = fam.FixedAssetSubCategoryId
                            LEFT JOIN HKP.GLGeneralInfo GL ON BM.GLGeneralInfoId=GL.Id
                            LEFT JOIN HKP.Activity AC ON A.ActivityId=AC.Id
                            LEFT JOIN SCS.Currency CG ON CG.Id=FAR.CurrencyId
							LEFT JOIN SCS.Currency CB ON CB.Id=FAR.FABaseCurrencyId
                            LEFT JOIN TRN.FixedAssetRegisterDetail FRD ON FRD.CapitalizeRegisterNo=FAR.CapitalizeRegisterNo
									LEFT JOIN TRN.InventoryIssueHistory IIH ON IIH.Id=FRD.InventoryIssueHistoryId
									LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IIH.CapitalizeVoucherDetailId
									LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId 
                            WHERE FAR.CompanyId = '" + identity.CompanyId + "' and FAR.Id='" + masterId + @"'  and FAR.Archive=0
                            Order by c.UserName, sc.UserName, FAR.SerialNo, FAR.InvoiceDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public GridModel GetAssetItemList(GridParameter parameters, string asset, string consumable)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT MT.Description AS MaterialType, MGP.UserName AS MaterialGroupMaster, MM.Code, MM.UserName, MG.[Description] AS GridName,
                                    PM.UserName AS ProductMaster, UOMB.UserName AS BaseUom, MM.StandardName, MM.ShortName, MM.[Description], MM.Id, MM.MaterialGridId, MM.BaseUOMId
                                    FROM [MST].[MaterialMaster] AS MM
                                    LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId=MT.Id
                                    LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId=MGP.Id
                                    LEFT OUTER JOIN [HKP].[MaterialGrid] AS MG ON MM.MaterialGridId=MG.Id
                                    LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId=PM.Id
                                    INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId=UOMB.Id
                                    WHERE MM.CompanyGroupId='" + identity.CompanyGroupId + "' AND MT.Nature='" + asset + "' OR MT.Nature='" + consumable + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetOpeningBalanceInfoWithAssetItemId(string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT SUM(x.TOTALROW) TotalRow,SUM(x.FABaseAmountTotal)FABaseAmountTotal,SUM(x.FAGroupAmountTotal)FAGroupAmountTotal,SUM(x.FAHardAmountTotal)FAHardAmountTotal,SUM(X.FAHardAmountTotal)FAHardAmountTotal,SUM(X.ADBaseAmountTotal)ADBaseAmountTotal,SUM(X.ADGroupAmountTotal)ADGroupAmountTotal,SUM(X.ADHardAmountTotal)ADHardAmountTotal
                        FROM (
                           SELECT FOBD.Quantity AS TotalRow,CC.CompanyCurrencyAmount AS FABaseAmountTotal,FOBD.FixedAssetMasterId, GC.CompanyGroupCurrencyAmount AS FAGroupAmountTotal, HC.HardCurrencyAmount AS FAHardAmountTotal, AAC.ADBaseAmountTotal,AAC.ADGroupAmountTotal,AAC.ADHardAmountTotal
                           FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                        INNER JOIN [TRN].[OpeningBalance] AS FOB ON FOB.Id=FOBD.OpeningBalanceId
                        INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.AssetGLId
                        INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
				 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
				 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.Amount AS CompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
				 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='FA'
				) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
				LEFT OUTER JOIN (
				SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId, OBDC.ToCurrencyId AS CompanyGroupToCurrencyId,
				 OBDC.ToCurrencyRate AS CompanyGroupCurrencyRate, OBDC.Amount AS CompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
				 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='FA'
				) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
				LEFT OUTER JOIN (
				 SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId, OBDC.ToCurrencyId AS HardToCurrencyId,
				 OBDC.ToCurrencyRate AS HardCurrencyRate, OBDC.Amount AS HardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
				 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='FA'
				) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                LEFT OUTER JOIN (
                SELECT  FOBD.Id,CC.CompanyCurrencyAmount AS ADBaseAmountTotal, GC.CompanyGroupCurrencyAmount AS ADGroupAmountTotal,HC.HardCurrencyAmount AS ADHardAmountTotal
				FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
				INNER JOIN [TRN].[OpeningBalance] AS FOB ON FOB.Id=FOBD.OpeningBalanceId
				INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.AccumulatedDepreciationGLId
				INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
					 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
					 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.Amount AS CompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
					 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					 WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='AD'
					) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
					LEFT OUTER JOIN (
					SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId, OBDC.ToCurrencyId AS CompanyGroupToCurrencyId,
					 OBDC.ToCurrencyRate AS CompanyGroupCurrencyRate, OBDC.Amount AS CompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
					 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					 WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='AD'
					) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
					LEFT OUTER JOIN (
					 SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId, OBDC.ToCurrencyId AS HardToCurrencyId,
					 OBDC.ToCurrencyRate AS HardCurrencyRate, OBDC.Amount AS HardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
					 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					 WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='AD'
                    ) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id) AS AAC ON FOBD.Id = AAC.Id
                    WHERE FOB.IsPark=0 AND FOBD.AssetGLId='" + assetGLId + @"' AND FOBD.AssetBudgetMasterId='" + assetBudgetId + "' AND FOBD.AssetActivityId='" + assetActivityId + "' AND FOB.CompanyId='" + companyId + "' ) X";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Dictionary<string, object>> GetCapitalizeAssetRegisterPopUpList(string column, string value, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 300 * from (SELECT ARC.AssetRegisterId FixedAssetRegisterId,ARC.AssetRegisterId,SUM(ARC.Amount) AssetAmount
                            ,(SUM(ISNULL(ARC.DepreciationAmount,0))+ SUM(ISNULL(ARC.AdjustmentDepreciationAmount,0))) DepreciationAmount
                            ,(SUM(ARC.Amount)-(SUM(ISNULL(ARC.DepreciationAmount,0))+ SUM(ISNULL(ARC.AdjustmentDepreciationAmount,0)))) NetAmount
                            ,FAM.UserName FixedAssetMaster,FAI.UserName FixedAssetItem,AR.AssetSlNo, AR.Status, AR.AssetCondition,AR.UserReference, AR.OldReference, AR.UserGroup, AR.Remarks  
                            FROM TRN.AssetRegisterChild ARC
							LEFT JOIN TRN.AssetRegister AR ON AR.Id=ARC.AssetRegisterId
                            LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
                            LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
                            WHERE ARC.CompanyGroupId='" + identity.CompanyGroupId + "' AND ARC.CompanyId='" + companyId + "' AND ARC.PlantId='" + identity.PlantId + @"'  AND ARC.VoucherDetailId is not null
                            AND ARC.AssetRegisterId NOT IN(select AssetRegisterId from TRN.FixedAssetRegisterDisposedDetail)
                            GROUP BY ARC.AssetRegisterId,FAM.UserName ,FAI.UserName ,AR.AssetSlNo,AR.Status
						    , AR.AssetCondition,AR.UserReference, AR.OldReference, AR.UserGroup, AR.Remarks
                            ) AS TEMP WHERE " + strkey + " order by FixedAssetMaster,FixedAssetItem ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetFixedAssetRegisterPopUpList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 300 * from (SELECT FR.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.MaterialMasterId,FR.FixedAssetMasterId
                                    , FR.SerialNo, FR.Id AssetNo, FR.InvoiceNo, MM.UserName MaterialMasterName
                                    , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                                    , FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                                    , FAM.FixedAssetSubCategoryId, FAM.AssetType
                                    ,P.UserName Vendor
                                    ,c.Code TrnCurrency

                                    , ISNULL(FR.Price,0) Price
									,ISNULL(SAR.subAssetAmount,0) SubAssetAmount
									, ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0) PurchasePrice
									 ,ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)-ISNULL(FR.ADBaseAmount,0)- ISNULL(FADP.FixedAssetDepreciationAmount,0) NetBookValue 
								--	, 0 NegotiationValue

								   , BC.Code BaseCurrency
									,isnull(FR.FABaseAmount,0)FABaseAmount
									,ISNULL(SAR.subAssetBaseAmount,0) SubAssetBaseAmount
									,isnull(FR.FABaseAmount,0) + ISNULL(SAR.subAssetBaseAmount,0) PurchaseBaseAmount
									,isnull( FR.ADBaseAmount,0) + ISNULL(FADP.FixedAssetDepreciationAmount,0) ADBaseAmount
                                    ,isnull(FR.FABaseAmount,0)+ISNULL(SAR.subAssetBaseAmount,0)-ISNULL(FR.ADBaseAmount,0) - ISNULL(FADP.FixedAssetDepreciationAmount,0) NetBaseBookValue 
									, 0 NegotiationValue,0 BaseNagotiationValue

                                    , MMA.StandardName Article, FR.IsFinancial,IsOpeningBalance=case when FR.IsOpeningBalance=0 then 'No' Else 'Yes' End
                                    , GL.AccountCode GLGeneralInfoCode,GL.UserName GLGeneralInfoName,GL.Id GLGeneralInfoId
									, BM.Id BudgetMasterId,B.UserName BudgetName,BM.RefNo BudgetRefNo
									, A.UserName ActivityName, FR.FAActivityId ActivityId
                                   		,format( FR.CapitalizationDate,'dd-MMM-yyyy')CapitalizationDate
									,format(FR.InvoiceDate,'dd-MMM-yyyy') PurchaseDate
									,format( ii.IssueDate,'dd-MMM-yyyy')IssueDate


                                    FROM[TRN].[FixedAssetRegister] FR
                                   LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId= MM.Id
                                   LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                                   LEFT JOIN MST.BudgetMaster BM ON FR.FABudgetMasterId = BM.Id
                                   LEFT JOIN [MST].[FixedAssetMaster] FAM ON FR.FixedAssetMasterId= FAM.Id
                                   LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId= FAC.Id
                                   LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId= FASC.Id

	                                LEFT JOIN TRN.FixedAssetRegisterDetail FRD ON FRD.CapitalizeRegisterNo=FR.CapitalizeRegisterNo
									LEFT JOIN TRN.InventoryIssueHistory IIH ON IIH.Id=FRD.InventoryIssueHistoryId
									LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IIH.CapitalizeVoucherDetailId
									LEFT JOIN TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									left join trn.InventoryIssue II on ii.Id = iid.InventoryIssueId
									LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
									left join trn.InventoryReceive IR on IR.Id =  IRD.InventoryReceiveId
									LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId 
                                    LEFT JOIN SCS.Currency C ON C.Id =FR.CurrencyId
                                    LEFT JOIN SCS.Currency BC ON BC.Id =FR.FABaseCurrencyId

	                                LEFT JOIN HKP.Party P ON P.Id = FR.VendorId
								   LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=BM.GLGeneralInfoId
								   LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
								   LEFT JOIN HKP.Activity A ON A.Id=FR.FAActivityId
								   LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount,ISNULL(Sum(BaseAmount),0) subAssetBaseAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
								   LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
                                   WHERE FR.CompanyId= '" + companyId + @"'  and FR.Archive= 0 and FR.IsAUC= 0 AND FR.Status IS NULL ) AS TEMP WHERE " + strkey + " order by SerialNo ";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFixedAssetRegisterDisposeEditList(string fixedAssetRegisterDisposeId, string companyId)
        {
            //string strkey = "1=1";
            //if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
            //    strkey = column + " like '%" + value + "%'";

            var sql = @"select top 300 * from (SELECT FARD.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.MaterialMasterId,FR.FixedAssetMasterId
                                    , FR.SerialNo, FR.Id AssetNo, FR.InvoiceNo, MM.UserName MaterialMasterName
                                    , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                                    , FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                                    , FAM.FixedAssetSubCategoryId, FAM.AssetType
                                    ,P.UserName Vendor
                                    ,c.Code TrnCurrency
	                                ,FAD.DocDate
                                    , ISNULL(FR.Price,0) Price
									,ISNULL(SAR.subAssetAmount,0) SubAssetAmount
									, ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0) PurchasePrice
									 ,ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)-ISNULL(FR.ADBaseAmount,0)-ISNULL(FADP.FixedAssetDepreciationAmount,0)  NetBookValue 
								--	, 0 NegotiationValue
                                    , ISNULL(FR.AdjustmentDepreciationAmount,0) AdjustmentDepreciationAmount
								   , BC.Code BaseCurrency
									,isnull(FR.FABaseAmount,0)FABaseAmount
									,ISNULL(SAR.subAssetBaseAmount,0) SubAssetBaseAmount
									,isnull(FR.FABaseAmount,0) + ISNULL(SAR.subAssetBaseAmount,0) PurchaseBaseAmount
									,isnull( FR.ADBaseAmount,0)+ISNULL(FADP.FixedAssetDepreciationAmount,0)ADBaseAmount
                                    , isnull(FR.FABaseAmount,0)+ISNULL(SAR.subAssetBaseAmount,0)-ISNULL(FR.ADBaseAmount,0)-ISNULL(FADP.FixedAssetDepreciationAmount,0) NetBaseBookValue 
									,FARD.BaseNagotiationValue, FARD.NegotiationValue

                                    , MMA.StandardName Article, FR.IsFinancial,IsOpeningBalance=case when FR.IsOpeningBalance=0 then 'No' Else 'Yes' End
                                    , GL.AccountCode GLGeneralInfoCode,GL.UserName GLGeneralInfoName,GL.Id GLGeneralInfoId
									, BM.Id BudgetMasterId,B.UserName BudgetName,BM.RefNo BudgetRefNo
									, A.UserName ActivityName, FR.FAActivityId ActivityId
                                   		,format( FR.CapitalizationDate,'dd-MMM-yyyy')CapitalizationDate
									,format(FR.InvoiceDate,'dd-MMM-yyyy') PurchaseDate
									,format( ii.IssueDate,'dd-MMM-yyyy')IssueDate
		                            ,FAD.Remarks

                                    FROM[TRN].[FixedAssetRegister] FR
                                   LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId= MM.Id
                                   LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                                   LEFT JOIN MST.BudgetMaster BM ON FR.FABudgetMasterId = BM.Id
                                   LEFT JOIN [MST].[FixedAssetMaster] FAM ON FR.FixedAssetMasterId= FAM.Id
                                   LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId= FAC.Id
                                   LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId= FASC.Id

	                                LEFT JOIN TRN.FixedAssetRegisterDetail FRD ON FRD.CapitalizeRegisterNo=FR.CapitalizeRegisterNo
									LEFT JOIN TRN.InventoryIssueHistory IIH ON IIH.Id=FRD.InventoryIssueHistoryId
									LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IIH.CapitalizeVoucherDetailId
									LEFT JOIN TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									left join trn.InventoryIssue II on ii.Id = iid.InventoryIssueId
									LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
									left join trn.InventoryReceive IR on IR.Id =  IRD.InventoryReceiveId
									LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId 
                                    LEFT JOIN SCS.Currency C ON C.Id =FR.CurrencyId
                                    LEFT JOIN SCS.Currency BC ON BC.Id =FR.FABaseCurrencyId

	                                LEFT JOIN HKP.Party P ON P.Id = FR.VendorId
								   LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=BM.GLGeneralInfoId
								   LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
								   LEFT JOIN HKP.Activity A ON A.Id=FR.FAActivityId
								   LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount,ISNULL(Sum(BaseAmount),0) subAssetBaseAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
                                    LEFT JOIN TRN.FixedAssetRegisterDisposedDetail FARD ON FARD.FixedAssetRegisterId=FR.Id
                                    LEFT JOIN TRN.FixedAssetRegisterDisposed FAD ON FAD.Id=FARD.FixedAssetRegisterDisposedId
									LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
                                   WHERE FR.CompanyId= '" + companyId + @"'  and FR.Archive= 0 and FR.IsAUC= 0 
                                    AND FARD.FixedAssetRegisterDisposedId='" + fixedAssetRegisterDisposeId + @"'
                                       ) AS TEMP 
                                    --WHERE FR.Id= order by SerialNo 
                                        ";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetCapitalizedAssetRegisterDisposeEditList(string fixedAssetRegisterDisposeId, string companyId)
        {
            var sql = @"select top 300 * from (SELECT FADD.Id,ARC.AssetRegisterId FixedAssetRegisterId,ARC.AssetRegisterId,SUM(ARC.Amount) AssetAmount
                            ,(SUM(ISNULL(ARC.DepreciationAmount,0))+ SUM(ISNULL(ARC.AdjustmentDepreciationAmount,0))) DepreciationAmount
                            ,(SUM(ARC.Amount)-(SUM(ISNULL(ARC.DepreciationAmount,0))+ SUM(ISNULL(ARC.AdjustmentDepreciationAmount,0)))) NetAmount
                            ,FAM.UserName FixedAssetMaster,FAI.UserName FixedAssetItem,AR.AssetSlNo, AR.Status, AR.AssetCondition,AR.UserReference, AR.OldReference, AR.UserGroup, AR.Remarks
							,sum(isnull(FADD.NegotiationValue,0))NegotiationValue,sum(isnull(FADD.BaseNagotiationValue,0))BaseNagotiationValue
							,(SELECT SUM(ISNULL(Amount,0)) FROM [TRN].[FixedAssetRegisterDisposedTax] WHERE FixedAssetRegisterDisposedDetailId=FADD.Id)TaxAmount
                            ,SUM(ISNULL(AR.AdjustmentDepreciationAmount,0)) AdjustmentDepreciationAmount
                            FROM  [TRN].[FixedAssetRegisterDisposedDetail] FADD 
							LEFT JOIN TRN.AssetRegister AR ON AR.Id=FADD.AssetRegisterId
							LEFT JOIN (SELECT sum(isnull( DepreciationAmount,0))DepreciationAmount,sum(isnull(AdjustmentDepreciationAmount,0))AdjustmentDepreciationAmount
									,sum(isnull(Amount,0))Amount ,AssetRegisterId ,CompanyId
									FROM TRN.AssetRegisterChild GROUP BY AssetRegisterId,CompanyId) ARC ON FADD.AssetRegisterId=ARC.AssetRegisterId
                            LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
                            LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
                            WHERE ARC.CompanyId= '" + companyId + @"' AND FADD.FixedAssetRegisterDisposedId='" + fixedAssetRegisterDisposeId + @"'
                            GROUP BY FADD.Id,ARC.AssetRegisterId,FAM.UserName ,FAI.UserName ,AR.AssetSlNo,AR.Status,FADD.FixedAssetRegisterDisposedId
						    , AR.AssetCondition,AR.UserReference, AR.OldReference, AR.UserGroup, AR.Remarks
                            ) AS TEMP ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetCapitalizedAssetRegisterDisposeTaxList(string fixedAssetRegisterDisposeId, string companyId)
        {
            var sql = @"select top 300 * from (SELECT * 
                        FROM [TRN].[FixedAssetRegisterDisposedTax] 
                        WHERE FixedAssetRegisterDisposedId='" + fixedAssetRegisterDisposeId + @"'
                        ) AS TEMP ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetCapitalizedAssetRegisterDisposeAdditionalTaxList(string fixedAssetRegisterDisposeId, string companyId)
        {
            var sql = @"select top 300 * from (SELECT TC.UserName TaxName,FADAT.Percentage ValueOfFixed,FADAT.BooksCurrencyTaxAmount CompanyCurrencyAmount,FADAT.* 
                        FROM [TRN].[FixedAssetRegisterDisposedAdditionalTax] FADAT
                        INNER JOIN [MST].[TaxCode] AS TC ON TC.Id = FADAT.TaxCodeId
                        WHERE FADAT.FixedAssetRegisterDisposedId='" + fixedAssetRegisterDisposeId + @"'
                        ) AS TEMP ";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFixedAssetLostByDisposeIdList(string id)
        {

            var sql = @"select frd.Id,frd.Id DisposeNo,fr.Remarks,fr.[Status],ei.EmployeeName,frd.IsPark,rdd.FixedAssetRegisterId,rdd.FixedAssetRegisterDisposedId
                , FR.SerialNo, FR.Id AssetNo, FR.InvoiceNo, MM.UserName MaterialMasterName
                , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                , FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                , FAM.FixedAssetSubCategoryId, FAM.AssetType	
                ,P.UserName Vendor
                       		,format( FR.CapitalizationDate,'dd-MMM-yyyy')CapitalizationDate
									,format(FR.InvoiceDate,'dd-MMM-yyyy') PurchaseDate
									,format( ii.IssueDate,'dd-MMM-yyyy')IssueDate
                                    ,format( frd.DocDate,'dd-MMM-yyyy')DocDate
			               ,c.Code TrnCurrency

                                    , ISNULL(FR.Price,0) Price
									,ISNULL(SAR.subAssetAmount,0) SubAssetAmount
									, ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0) PurchasePrice
									 ,ISNULL(FR.Price,0)+ISNULL(SAR.subAssetAmount,0)-ISNULL(FR.ADBaseAmount,0) NetBookValue 
								--	, 0 NegotiationValue
									, ISNULL(FR.AdjustmentDepreciationAmount,0) AdjustmentDepreciationAmount
								   , BC.Code BaseCurrency
									,isnull(FR.FABaseAmount,0)FABaseAmount
									,ISNULL(SAR.subAssetBaseAmount,0) SubAssetBaseAmount
									,isnull(FR.FABaseAmount,0) + ISNULL(SAR.subAssetBaseAmount,0) PurchaseBaseAmount
									,isnull( FR.ADBaseAmount,0)+ ISNULL(FADP.FixedAssetDepreciationAmount,0)ADBaseAmount
                                    ,isnull(FR.FABaseAmount,0)+ISNULL(SAR.subAssetBaseAmount,0)-ISNULL(FR.ADBaseAmount,0)- ISNULL(FADP.FixedAssetDepreciationAmount,0) NetBaseBookValue  
										,isnull( rdd.NegotiationValue,0)NegotiationValue
                               	,isnull( rdd.BaseNagotiationValue,0)BaseNagotiationValue

                , MMA.StandardName Article, FR.IsFinancial,IsOBBalance=case when FR.IsOpeningBalance=0 then 'No' Else 'Yes' End
                , GL.AccountCode GLGeneralInfoCode,GL.UserName GLGeneralInfoName,GL.Id GLGeneralInfoId
                , BM.Id BudgetMasterId,B.UserName BudgetName,BM.RefNo BudgetRefNo
                , A.UserName ActivityName, FR.FAActivityId ActivityId, frd.AddedBy,frd.AddedDate,frd.AddedFromIP,frd.EmployeeId
                from TRN.FixedAssetRegisterDisposedDetail rdd 
				join TRN.FixedAssetRegisterDisposed frd  ON rdd.FixedAssetRegisterDisposedId=frd.Id
                left join TRN.FixedAssetRegister FR on FR.Id=rdd.FixedAssetRegisterId
                left join dbo.EmployeeInformation ei on ei.SystemId=frd.EmployeeId
				LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId= MM.Id
                 LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                 LEFT JOIN MST.BudgetMaster BM ON FR.FABudgetMasterId = BM.Id
                 LEFT JOIN [MST].[FixedAssetMaster] FAM ON FR.FixedAssetMasterId= FAM.Id
                 LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId= FAC.Id
                 LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId= FASC.Id

				  LEFT JOIN TRN.FixedAssetRegisterDetail FARD ON FARD.CapitalizeRegisterNo=FR.CapitalizeRegisterNo
									LEFT JOIN TRN.InventoryIssueHistory IIH ON IIH.Id=FARD.InventoryIssueHistoryId
									LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IIH.CapitalizeVoucherDetailId
									LEFT JOIN TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									left join trn.InventoryIssue II on ii.Id = iid.InventoryIssueId
									LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
									left join trn.InventoryReceive IR on IR.Id =  IRD.InventoryReceiveId
                                     LEFT JOIN HKP.Party P ON P.Id = FR.VendorId
				 LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=BM.GLGeneralInfoId
				 LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
				 LEFT JOIN HKP.Activity A ON A.Id=FR.FAActivityId
                      LEFT JOIN SCS.Currency C ON C.Id =frd.CurrencyId
                     LEFT JOIN SCS.Currency BC ON BC.Id =FR.FABaseCurrencyId
                LEFT JOIN (SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount ,ISNULL(Sum(BaseAmount),0) subAssetBaseAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
				LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
                 where frd.Id='" + id + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetCapitalizeAssetLostByDisposeIdList(string id)
        {

            var sql = @"select frd.Id,frd.Id DisposeNo,AR.[Status],ei.EmployeeName,FRDD.AssetRegisterId,FRDD.FixedAssetRegisterDisposedId
				,FAM.UserName FixedAssetMaster,FAI.UserName FixedAssetItem
                , ISNULL(ADP.AssetAmount,0) AssetAmount,ISNULL(ADP.DepreciationAmount,0)DepreciationAmount,ISNULL(ADP.NetAmount,0)NetAmount
				,ISNULL(AR.AdjustmentDepreciationAmount,0)AdjustmentDepreciationAmount, ISNULL(FRDD.NegotiationValue,0) NegotiationValue
                ,frd.CurrencyId trnCurrencyId,frd.ToCurrencyRate
                ,(SELECT SUM(ISNULL(Amount,0)) FROM [TRN].[FixedAssetRegisterDisposedTax] WHERE FixedAssetRegisterDisposedDetailId=FRDD.Id)TaxAmount
                from TRN.FixedAssetRegisterDisposedDetail FRDD 
				join TRN.FixedAssetRegisterDisposed frd  ON FRDD.FixedAssetRegisterDisposedId=frd.Id
                left join TRN.AssetRegister AR on AR.Id=FRDD.AssetRegisterId
                left join dbo.EmployeeInformation ei on ei.SystemId=frd.EmployeeId
				LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
				LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
				LEFT JOIN (select SUM(ARC.Amount) AssetAmount,SUM(ISNULL(ARC.DepreciationAmount,0))DepreciationAmount,SUM(ARC.NetAmount)NetAmount,ARC.AssetRegisterId from [TRN].[AssetRegisterChild] ARC GROUP BY  ARC.AssetRegisterId) ADP ON ADP.AssetRegisterId=AR.Id
                 where frd.Id='" + id + "'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFixedAssetLostJVList(string fixedAssetDisposeId, string companyId, string plantId)
        {

            var sql = @"DECLARE @receiveId varchar(10)='" + fixedAssetDisposeId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

						SELECT X.* FROM(
						SELECT  'Depreciation' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMG.DepreciationBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FAMG.DepreciationActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							 ,SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN HKP.FixedAssetMasterGL AS FAMG  ON FAMG.FixedAssetMasterId=FR.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FAMG.DepreciationBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FAMG.DepreciationGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FAMG.DepreciationActivityId= A.Id
						LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FR.ADBudgetMasterId, B.Code, B.UserName, FR.ADActivityId, A.Code, A.UserName
						UNION
						SELECT  'Advance' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =GAD.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =GAD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = GAD.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM(FRDD.NegotiationValue) AS Dr
							, NULL Cr
							, SUM(FRDD.NegotiationValue) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN ORG.Company C ON C.Id=FR.CompanyId
						LEFT JOIN HKP.GeneralAccountDeterminate GAD ON GAD.COAId=C.COAId AND GAD.Id='FixedAssetLostRecoveryFromEmployee'

						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  GAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, GAD.BudgetMasterId, B.Code, B.UserName, GAD.ActivityId, A.Code, A.UserName
					    UNION
						SELECT  'LossOnDispose' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =FGL.LossOnDisposalAssetGLId       
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FGL.LossOnDisposalAssetBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FGL.LossOnDisposalAssetBudgetMasterId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM(FR.Price+SAR.subAssetAmount-FR.ADBaseAmount-FRDD.NegotiationValue)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM(FR.Price+SAR.subAssetAmount-FR.ADBaseAmount-FRDD.NegotiationValue)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=FR.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FGL.LossOnDisposalAssetBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FGL.LossOnDisposalAssetGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FGL.LossOnDisposalAssetActivityId= A.Id
						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  FGL.LossOnDisposalAssetGLId, GL.AccountCode, GL.UserName, FGL.LossOnDisposalAssetBudgetMasterId, B.Code, B.UserName, FGL.LossOnDisposalAssetActivityId, A.Code, A.UserName
						
						UNION
						SELECT  'Asset' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FR.FABudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FR.FAActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							
							, NULL Dr
							, SUM(FR.Price+SAR.subAssetAmount) AS Cr
							, SUM(FR.Price+SAR.subAssetAmount) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId

						LEFT JOIN[MST].[BudgetMaster] AS BM ON FR.FABudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FR.FAActivityId= A.Id
						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FR.FABudgetMasterId, B.Code, B.UserName, FR.FAActivityId, A.Code, A.UserName";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetCapitalizeAssetLostJVList(string fixedAssetDisposeId, string companyId, string plantId)
        {

            var sql = @"DECLARE @receiveId varchar(10)='" + fixedAssetDisposeId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

						SELECT X.* FROM(
						SELECT  'Depreciation' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMG.DepreciationBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FAMG.DepreciationActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							 ,SUM( ISNULL(AR.AdjustmentDepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM( ISNULL(AR.AdjustmentDepreciationAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.AssetRegister AR ON AR.Id=FRDD.AssetRegisterId
						LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
						LEFT JOIN HKP.FixedAssetMasterGL AS FAMG  ON FAMG.FixedAssetMasterId=FAI.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FAMG.DepreciationBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FAMG.DepreciationGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FAMG.DepreciationActivityId= A.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,FAMG.DepreciationBudgetMasterId,FAMG.DepreciationActivityId
						UNION All
						SELECT  'Advance' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =GAD.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =GAD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = GAD.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM(FRDD.NegotiationValue) AS Dr
							, NULL Cr
							, SUM(FRDD.NegotiationValue) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.AssetRegister FR ON FR.Id=FRDD.AssetRegisterId
						LEFT JOIN HKP.GeneralAccountDeterminate GAD ON  GAD.Id='FixedAssetLostRecoveryFromEmployee'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  GAD.GLGeneralInfoId, GL.AccountCode, GL.UserName, GAD.BudgetMasterId, B.Code, B.UserName, GAD.ActivityId, A.Code, A.UserName
					    UNION All
						SELECT  'LossOnDispose' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =FAMG.LossOnDisposalAssetGLId       
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMG.LossOnDisposalAssetBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FAMG.LossOnDisposalAssetBudgetMasterId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM(NetAmount-ISNULL(FRDD.NegotiationValue,0)) -SUM( ISNULL(AR.AdjustmentDepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM(NetAmount-ISNULL(FRDD.NegotiationValue,0)) -SUM( ISNULL(AR.AdjustmentDepreciationAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.AssetRegister AR ON AR.Id=FRDD.AssetRegisterId
						LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
						LEFT JOIN HKP.FixedAssetMasterGL AS FAMG  ON FAMG.FixedAssetMasterId=FAI.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FAMG.LossOnDisposalAssetBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FAMG.LossOnDisposalAssetGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FAMG.LossOnDisposalAssetActivityId= A.Id
						LEFT JOIN (select SUM(ARC.Amount) AssetAmount,SUM(ISNULL(ARC.DepreciationAmount,0))DepreciationAmount,SUM(ARC.NetAmount)NetAmount,ARC.AssetRegisterId from [TRN].[AssetRegisterChild] ARC GROUP BY  ARC.AssetRegisterId) ADP ON ADP.AssetRegisterId=AR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  FAMG.LossOnDisposalAssetGLId, GL.AccountCode, GL.UserName, FAMG.LossOnDisposalAssetBudgetMasterId, B.Code, B.UserName, FAMG.LossOnDisposalAssetActivityId, A.Code, A.UserName
						
						UNION All
						SELECT  'Asset' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =ADP.GLGeneralInfoId        
							,GLGeneralInfoCode =ADP.AccountCode 
							,GLGeneralInfoName =ADP.GLGeneralInfoName
							,BudgetMasterId =ADP.BudgetMasterId
							,BudgetCode = ADP.BudgetCode
							,BudgetName =ADP.BudgetName 
							,ActivityId = ADP.ActivityId
							,ActivityCode = ADP.ActivityCode
							,ActivityName =ADP.ActivityName
							, NULL Dr
							, SUM(ADP.NetAmount) AS Cr
							, SUM(ADP.NetAmount) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.AssetRegister AR ON AR.Id=FRDD.AssetRegisterId
						LEFT JOIN (select SUM(ARC.NetAmount) NetAmount,ARC.AssetRegisterId ,BM.GLGeneralInfoId, GL.AccountCode, GL.UserName GLGeneralInfoName
						, VD.BudgetMasterId, B.Code BudgetCode, B.UserName BudgetName, VD.ActivityId, A.Code ActivityCode, A.UserName ActivityName
											from [TRN].[AssetRegisterChild] ARC 
											LEFT JOIN [TRN].[VoucherDetail]  VD ON VD.Id=ARC.VoucherDetailId
											LEFT JOIN [MST].[BudgetMaster] AS BM ON  BM.Id=VD.BudgetMasterId
											LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
											LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
											LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
                                            WHERE ARC.VoucherDetailId IS NOT NULL
											GROUP BY  ARC.AssetRegisterId ,BM.GLGeneralInfoId, GL.AccountCode, GL.UserName
						, VD.BudgetMasterId, B.Code, B.UserName, VD.ActivityId, A.Code, A.UserName) ADP ON ADP.AssetRegisterId=AR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  ADP.GLGeneralInfoId, ADP.AccountCode, ADP.GLGeneralInfoName, ADP.BudgetMasterId, ADP.BudgetCode, ADP.BudgetName, ADP.ActivityId, ADP.ActivityCode, ADP.ActivityName
                        ) X 
                        WHERE X.Amount>0
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetCapitalizeAssetSalesJVList(string fixedAssetDisposeId, string companyId, string plantId)
        {

            var sql = @"DECLARE @receiveId varchar(10)='" + fixedAssetDisposeId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

						SELECT X.* FROM(
						SELECT  'Depreciation' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMG.DepreciationBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FAMG.DepreciationActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							 ,SUM( ISNULL(AR.AdjustmentDepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM( ISNULL(AR.AdjustmentDepreciationAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.AssetRegister AR ON AR.Id=FRDD.AssetRegisterId
						LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
						LEFT JOIN HKP.FixedAssetMasterGL AS FAMG  ON FAMG.FixedAssetMasterId=FAI.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FAMG.DepreciationBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FAMG.DepreciationGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FAMG.DepreciationActivityId= A.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,FAMG.DepreciationBudgetMasterId,FAMG.DepreciationActivityId
						UNION All
						SELECT  'A/R' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =CPGL.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =CPGL.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = CPGL.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							,SUM(FRDD.NegotiationValue)+ISNULL((SELECT SUM(Amount) FROM [TRN].[FixedAssetRegisterDisposedTax] WHERE FixedAssetRegisterDisposedId=@receiveId),0)
                                + ISNULL((SELECT SUM(TaxAmount) FROM [TRN].[FixedAssetRegisterDisposedAdditionalTax] WHERE FixedAssetRegisterDisposedId=@receiveId),0) AS Dr
							, NULL Cr
							,SUM(FRDD.NegotiationValue)+ISNULL((SELECT SUM(Amount) FROM [TRN].[FixedAssetRegisterDisposedTax] WHERE FixedAssetRegisterDisposedId=@receiveId),0) 
                                + ISNULL((SELECT SUM(TaxAmount) FROM [TRN].[FixedAssetRegisterDisposedAdditionalTax] WHERE FixedAssetRegisterDisposedId=@receiveId),0) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.AssetRegister FR ON FR.Id=FRDD.AssetRegisterId
						LEFT JOIN [HKP].[CompanyParty] CP ON FRD.PartyId = CP.PartyId AND CP.PlantId=@plantId AND CP.PartyType='Customer'
						LEFT JOIN HKP.CompanyPartyGL CPGL ON CPGL.CompanyPartyId=CP.Id and CPGL.PartyGLType='ReconciliationGL' 
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON CPGL.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON CPGL.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON CPGL.ActivityId= A.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  CPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, CPGL.BudgetMasterId, B.Code, B.UserName, CPGL.ActivityId, A.Code, A.UserName

					    UNION All
						SELECT  'LossOnDispose' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =GAD.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =GAD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = GAD.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM(NetAmount-ISNULL(FRDD.NegotiationValue,0)) -SUM( ISNULL(AR.AdjustmentDepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM(NetAmount-ISNULL(FRDD.NegotiationValue,0)) -SUM( ISNULL(AR.AdjustmentDepreciationAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.AssetRegister AR ON AR.Id=FRDD.AssetRegisterId
						LEFT JOIN HKP.GeneralAccountDeterminate GAD ON  GAD.Id='LossOnDisposalFixedAsset'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id
						LEFT JOIN (select SUM(ARC.Amount) AssetAmount,SUM(ISNULL(ARC.DepreciationAmount,0))DepreciationAmount,SUM(ARC.NetAmount)NetAmount,ARC.AssetRegisterId from [TRN].[AssetRegisterChild] ARC GROUP BY  ARC.AssetRegisterId) ADP ON ADP.AssetRegisterId=AR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  GAD.GLGeneralInfoId,GAD.BudgetMasterId,GAD.ActivityId,GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName
						
						UNION All
						SELECT  'GainOnDispose' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =GAD.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =GAD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = GAD.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, NULL AS Dr
							, SUM(ISNULL(FRDD.NegotiationValue,0))-SUM( ISNULL(NetAmount,0)) -SUM( ISNULL(AR.AdjustmentDepreciationAmount,0)) AS Cr
							, SUM(ISNULL(FRDD.NegotiationValue,0))-SUM( ISNULL(NetAmount,0)) -SUM( ISNULL(AR.AdjustmentDepreciationAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.AssetRegister AR ON AR.Id=FRDD.AssetRegisterId
						LEFT JOIN HKP.GeneralAccountDeterminate GAD ON  GAD.Id='GainOnDisposalFixedAsset'
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON GAD.GLGeneralInfoId=GL.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON GAD.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON GAD.ActivityId= A.Id
						LEFT JOIN (select SUM(ARC.Amount) AssetAmount,SUM(ISNULL(ARC.DepreciationAmount,0))DepreciationAmount,SUM(ARC.NetAmount)NetAmount,ARC.AssetRegisterId from [TRN].[AssetRegisterChild] ARC GROUP BY  ARC.AssetRegisterId) ADP ON ADP.AssetRegisterId=AR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  GAD.GLGeneralInfoId,GAD.BudgetMasterId,GAD.ActivityId,GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName
						
						UNION All
						SELECT  'Asset' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =ADP.GLGeneralInfoId        
							,GLGeneralInfoCode =ADP.AccountCode 
							,GLGeneralInfoName =ADP.GLGeneralInfoName
							,BudgetMasterId =ADP.BudgetMasterId
							,BudgetCode = ADP.BudgetCode
							,BudgetName =ADP.BudgetName 
							,ActivityId = ADP.ActivityId
							,ActivityCode = ADP.ActivityCode
							,ActivityName =ADP.ActivityName
							, NULL Dr
							, SUM(ADP.NetAmount) AS Cr
							, SUM(ADP.NetAmount) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.AssetRegister AR ON AR.Id=FRDD.AssetRegisterId
						LEFT JOIN (select SUM(ARC.NetAmount) NetAmount,ARC.AssetRegisterId ,BM.GLGeneralInfoId, GL.AccountCode, GL.UserName GLGeneralInfoName
						, VD.BudgetMasterId, B.Code BudgetCode, B.UserName BudgetName, VD.ActivityId, A.Code ActivityCode, A.UserName ActivityName
											from [TRN].[AssetRegisterChild] ARC 
											LEFT JOIN [TRN].[VoucherDetail]  VD ON VD.Id=ARC.VoucherDetailId
											LEFT JOIN [MST].[BudgetMaster] AS BM ON  BM.Id=VD.BudgetMasterId
											LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
											LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
											LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
                                            WHERE ARC.VoucherDetailId IS NOT NULL
											GROUP BY  ARC.AssetRegisterId ,BM.GLGeneralInfoId, GL.AccountCode, GL.UserName
						, VD.BudgetMasterId, B.Code, B.UserName, VD.ActivityId, A.Code, A.UserName) ADP ON ADP.AssetRegisterId=AR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  ADP.GLGeneralInfoId, ADP.AccountCode, ADP.GLGeneralInfoName, ADP.BudgetMasterId, ADP.BudgetCode, ADP.BudgetName, ADP.ActivityId, ADP.ActivityCode, ADP.ActivityName
                        UNION All
						SELECT  'TaxPayable' AS OtherName, 'Cr' AS TrnType
                        ,GLGeneralInfoId =TCGL.LiabilityGLId ,GLGeneralInfoCode =GL.AccountCode ,GLGeneralInfoName =GL.UserName
                        ,BudgetMasterId =TCGL.LiabilityBudgetMasterId,BudgetCode = B.Code,BudgetName =B.UserName 
						,ActivityId = TCGL.LiabilityActivityId,ActivityCode = A.Code,ActivityName =A.UserName
						, NULL Dr
						, SUM(FRDD.Amount) Cr
						, SUM(FRDD.Amount) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedTax FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN [MST].[TaxCategoryGL] AS TCGL ON TCGL.TaxCategoryId=FRDD.TaxCategoryId AND TCGL.InputTaxOutPutTax='Output' AND ISNULL(TCGL.TaxType,'')='Excluded' 
						LEFT JOIN [MST].[TaxCategory] AS TC ON TCGL.TaxCategoryId=TC.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TCGL.LiabilityGLId=GL.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM ON TCGL.LiabilityBudgetMasterId= BM.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON TCGL.LiabilityActivityId= A.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@receiveId
						GROUP BY  TCGL.LiabilityGLId, GL.AccountCode, GL.UserName, TCGL.LiabilityBudgetMasterId, B.Code, B.UserName, TCGL.LiabilityActivityId, A.Code, A.UserName
                        UNION All
						SELECT 'TCSPayable' AS OtherName, 'Cr' AS TrnType
						, TGL.WithholdCreditableGLId AS GLGeneralInfoId, GL.AccountCode AS GLGeneralInfoCode, GL.UserName AS GLGeneralInfoName
						, TGL.WithholdCreditableBudgetMasterId AS BudgetMasterId, B.Code AS BudgetCode, B.UserName AS BudgetName
						, TGL.WithholdCreditableActivityId AS ActivityId, A.Code AS ActivityCode, A.UserName AS ActivityName
						,  NULL Dr, SUM(IRT.TaxAmount) AS Cr
						, SUM(IRT.TaxAmount) AS Amount
					    FROM [TRN].[FixedAssetRegisterDisposedAdditionalTax] AS IRT
                        LEFT JOIN [TRN].[FixedAssetRegisterDisposed] AS IR ON IRT.FixedAssetRegisterDisposedId=IR.Id
					    LEFT JOIN MST.TaxCode TCO ON TCO.Id=IRT.TaxCodeId  
					    LEFT JOIN MST.TaxCodeGL TGL ON TGL.TaxCodeId=TCO.Id 
					    LEFT JOIN [MST].[TaxCategory] AS TC ON IRT.TaxCategoryId=TC.Id
					    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON TGL.WithholdCreditableGLId=GL.Id
					    LEFT JOIN [MST].[BudgetMaster] AS BM ON TGL.WithholdCreditableBudgetMasterId= BM.Id
					    LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
					    LEFT JOIN [HKP].[Activity] AS A ON TGL.WithholdCreditableActivityId= A.Id
					    WHERE IRT.FixedAssetRegisterDisposedId=@receiveId AND TCO.InputOrOutput='Output' 
					    GROUP BY  IRT.TaxCategoryId,IRT.TaxCodeId, TGL.WithholdCreditableGLId, GL.AccountCode, GL.UserName, TGL.WithholdCreditableBudgetMasterId, B.Code
					    , B.UserName, TGL.WithholdCreditableActivityId, A.Code, A.UserName
                        ) X 
                        WHERE X.Amount>0
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFixedAssetSalesSingleJVList(string fixedAssetDisposeId, string companyId, string plantId)
        {

            var sql = @"DECLARE @fixedAssetDisposeId varchar(10)='" + fixedAssetDisposeId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

						SELECT X.* FROM(
						SELECT  'Depreciation' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMG.DepreciationBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FAMG.DepreciationActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							 ,SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN HKP.FixedAssetMasterGL AS FAMG  ON FAMG.FixedAssetMasterId=FR.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FAMG.DepreciationBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FAMG.DepreciationGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FAMG.DepreciationActivityId= A.Id
						LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FAMG.DepreciationBudgetMasterId, B.Code, B.UserName, FAMG.DepreciationActivityId, A.Code, A.UserName
						
					    UNION
						SELECT  OtherName=CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN 'LossOnSale' ELSE 'GainOnSale' End 
						
						,TrnType= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN 'Dr' ELSE 'Cr' End 

							,GLGeneralInfoId= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN FGL.LossOnSaleOfAssetGLId ELSE FGL.GainOnSaleOfAssetGLId END      
							,GLGeneralInfoCode= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN GL.AccountCode ELSE GLG.AccountCode END      
							,GLGeneralInfoName= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN GL.UserName ELSE GLG.UserName END      
							,BudgetMasterId= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN FGL.LossOnSaleOfAssetBudgetMasterId ELSE FGL.GainOnSaleOfAssetBudgetMasterId END      
							,BudgetCode= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN B.Code ELSE BG.Code END      
							,BudgetName= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN B.UserName ELSE BG.UserName END      
							,ActivityId= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN FGL.LossOnSaleOfAssetActivityId ELSE FGL.GainOnSaleOfAssetActivityId END      
							,ActivityCode= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN A.Code ELSE AG.Code END      
							,ActivityName= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))>0 THEN A.UserName ELSE AG.UserName END      
							, Dr= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))> 0 THEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue)) ELSE 0 END 
							, Cr= CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue)) < 0 THEN SUM(FR.ADBaseAmount)+SUM(FR.BaseNagotiationValue)+SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))+SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) - SUM(FR.FABaseAmount)-SUM(ISNULL(SAR.subAssetAmount,0)) ELSE 0 END 
							, Amount=CASE WHEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue))> 0 THEN (SUM(FR.FABaseAmount)+SUM(ISNULL(SAR.subAssetAmount,0))-SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))-SUM(FR.BaseNagotiationValue)) 
									ELSE SUM(FR.ADBaseAmount)+SUM(FR.BaseNagotiationValue)+SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))+SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) - SUM(FR.FABaseAmount)-SUM(ISNULL(SAR.subAssetAmount,0)) END 
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=FR.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FGL.LossOnSaleOfAssetBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FGL.LossOnSaleOfAssetGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FGL.LossOnSaleOfAssetActivityId= A.Id

						LEFT JOIN[MST].[BudgetMaster] AS BMG ON FGL.GainOnSaleOfAssetBudgetMasterId= BMG.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLG ON FGL.GainOnSaleOfAssetGLId=GLG.Id
						LEFT JOIN [HKP].[Budget] AS BG ON BMG.BudgetId= BG.Id
						LEFT JOIN [HKP].[Activity] AS AG ON FGL.GainOnSaleOfAssetActivityId= AG.Id

						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  FGL.LossOnSaleOfAssetGLId,FGL.GainOnSaleOfAssetGLId, GL.AccountCode, GL.UserName, FGL.LossOnSaleOfAssetBudgetMasterId, B.Code, B.UserName, FGL.LossOnSaleOfAssetActivityId, A.Code, A.UserName
						,GLG.AccountCode,GLG.UserName,BG.Code,BG.UserName,FGL.GainOnSaleOfAssetActivityId,AG.Code,AG.UserName,FGL.GainOnSaleOfAssetBudgetMasterId
						UNION
						SELECT  'Asset' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FR.FABudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FR.FAActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							
							, NULL Dr
							, SUM(FR.FABaseAmount+ISNULL(SAR.subAssetAmount,0)) -SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))AS Cr
							, SUM(FR.FABaseAmount+ISNULL(SAR.subAssetAmount,0)) -SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId

						LEFT JOIN[MST].[BudgetMaster] AS BM ON FR.FABudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FR.FAActivityId= A.Id
						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(BaseAmount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FR.FABudgetMasterId, B.Code, B.UserName, FR.FAActivityId, A.Code, A.UserName
						UNION
						SELECT  'A/R' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =MGPGL.GLGeneralInfoId       
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =MGPGL.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = MGPGL.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM(FR.BaseNagotiationValue) AS Dr
							, NULL Cr
							, SUM(FR.BaseNagotiationValue) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN MST.MaterialMaster MM ON MM.Id=FR.MaterialMasterId
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId

						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON FRD.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId 
						AND MGPGL.PartyAccountGroupId= PACG.Id AND MGPGL.GLType='Receivable'
						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGPGL.BudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						) X 
                        WHERE X.Amount>0
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFixedAssetSalesBookAsSalesJV1List(string fixedAssetDisposeId, string companyId, string plantId)
        {

            var sql = @"DECLARE @fixedAssetDisposeId varchar(10)='" + fixedAssetDisposeId + @"', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

					SELECT X.* FROM (
						SELECT  'Depreciation' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FR.ADBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FR.ADActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM(FR.ADBaseAmount) AS Dr
							, NULL Cr
							, SUM(FR.ADBaseAmount) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FR.ADBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FR.ADActivityId= A.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FR.ADBudgetMasterId, B.Code, B.UserName, FR.ADActivityId, A.Code, A.UserName
						
					    UNION
						SELECT  'LossOnDispose' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =FGL.LossOnDisposalAssetGLId       
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FGL.LossOnDisposalAssetBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FGL.LossOnDisposalAssetBudgetMasterId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM(FR.Price+SAR.subAssetAmount-FR.ADBaseAmount) AS Dr
							, NULL Cr
							, SUM(FR.Price+SAR.subAssetAmount-FR.ADBaseAmount) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=FR.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FGL.LossOnDisposalAssetBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FGL.LossOnDisposalAssetGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FGL.LossOnDisposalAssetActivityId= A.Id
						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  FGL.LossOnDisposalAssetGLId, GL.AccountCode, GL.UserName, FGL.LossOnDisposalAssetBudgetMasterId, B.Code, B.UserName, FGL.LossOnDisposalAssetActivityId, A.Code, A.UserName
						
						UNION
						SELECT  'Asset' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FR.FABudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FR.FAActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							
							, NULL Dr
							, SUM(FR.Price+SAR.subAssetAmount) AS Cr
							, SUM(FR.Price+SAR.subAssetAmount) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId

						LEFT JOIN[MST].[BudgetMaster] AS BM ON FR.FABudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FR.FAActivityId= A.Id
						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FR.FABudgetMasterId, B.Code, B.UserName, FR.FAActivityId, A.Code, A.UserName
						) X
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFixedAssetScrapSingleJVList(string fixedAssetDisposeId, string companyId, string plantId)
        {

            var sql = @"DECLARE @fixedAssetDisposeId varchar(10)='" + fixedAssetDisposeId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

						SELECT X.* FROM(
						SELECT  'Depreciation' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMG.DepreciationBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FAMG.DepreciationActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							 ,SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN HKP.FixedAssetMasterGL AS FAMG  ON FAMG.FixedAssetMasterId=FR.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FAMG.DepreciationBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FAMG.DepreciationGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FAMG.DepreciationActivityId= A.Id
						LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FR.ADBudgetMasterId, B.Code, B.UserName, FR.ADActivityId, A.Code, A.UserName
						
					    UNION
						SELECT  OtherName='LossOnDisposal'
						
						,TrnType='Dr'

							,GLGeneralInfoId=FGL.LossOnDisposalAssetGLId   
							,GLGeneralInfoCode=GL.AccountCode     
							,GLGeneralInfoName=  GL.UserName    
							,BudgetMasterId= FGL.LossOnDisposalAssetBudgetMasterId     
							,BudgetCode=  B.Code    
							,BudgetName=  B.UserName    
							,ActivityId=  FGL.LossOnDisposalAssetActivityId     
							,ActivityCode=  A.Code    
							,ActivityName=  A.UserName    
							, Dr=  SUM(FR.FABaseAmount+isnull(SAR.subAssetAmount,0))- SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))
							, Cr=0
							, Amount= SUM(FR.FABaseAmount+isnull(SAR.subAssetAmount,0))- SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=FR.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FGL.LossOnDisposalAssetBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FGL.LossOnDisposalAssetGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FGL.LossOnDisposalAssetActivityId= A.Id

						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  FGL.LossOnSaleOfAssetGLId,FGL.LossOnDisposalAssetGLId, GL.AccountCode, GL.UserName, FGL.LossOnDisposalAssetBudgetMasterId, B.Code, B.UserName, FGL.LossOnDisposalAssetActivityId, A.Code, A.UserName
						,FGL.GainOnSaleOfAssetActivityId,FGL.GainOnSaleOfAssetBudgetMasterId
						UNION
						SELECT  'Asset' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FR.FABudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FR.FAActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							
							, NULL Dr
							, SUM(FR.FABaseAmount+isnull(SAR.subAssetAmount,0)) AS Cr
							, SUM(FR.FABaseAmount+isnull(SAR.subAssetAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId

						LEFT JOIN[MST].[BudgetMaster] AS BM ON FR.FABudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FR.FAActivityId= A.Id
						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FR.FABudgetMasterId, B.Code, B.UserName, FR.FAActivityId, A.Code, A.UserName
						) X 
                        WHERE X.Amount>0
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFixedAssetTheftSingleJVList(string fixedAssetDisposeId, string companyId, string plantId)
        {

            var sql = @"DECLARE @fixedAssetDisposeId varchar(10)='" + fixedAssetDisposeId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

						SELECT X.* FROM(
						SELECT  'Depreciation' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMG.DepreciationBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FAMG.DepreciationActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							 ,SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM( ISNULL(FR.AdjustmentDepreciationAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN HKP.FixedAssetMasterGL AS FAMG  ON FAMG.FixedAssetMasterId=FR.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FAMG.DepreciationBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FAMG.DepreciationGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FAMG.DepreciationActivityId= A.Id
						LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FR.ADBudgetMasterId, B.Code, B.UserName, FR.ADActivityId, A.Code, A.UserName
						
					    UNION
						SELECT  OtherName='LossOnDisposal'
						
						,TrnType='Dr'

							,GLGeneralInfoId=FGL.LossOnDisposalAssetGLId   
							,GLGeneralInfoCode=GL.AccountCode     
							,GLGeneralInfoName=  GL.UserName    
							,BudgetMasterId= FGL.LossOnDisposalAssetBudgetMasterId     
							,BudgetCode=  B.Code    
							,BudgetName=  B.UserName    
							,ActivityId=  FGL.LossOnDisposalAssetActivityId     
							,ActivityCode=  A.Code    
							,ActivityName=  A.UserName    
							, Dr=  SUM(FR.FABaseAmount+isnull(SAR.subAssetAmount,0))- SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))
							, Cr=0
							, Amount= SUM(FR.FABaseAmount+isnull(SAR.subAssetAmount,0))- SUM(FR.ADBaseAmount)-SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0))-SUM( ISNULL(FR.AdjustmentDepreciationAmount,0))
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=FR.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FGL.LossOnDisposalAssetBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FGL.LossOnDisposalAssetGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FGL.LossOnDisposalAssetActivityId= A.Id

						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  FGL.LossOnSaleOfAssetGLId,FGL.LossOnDisposalAssetGLId, GL.AccountCode, GL.UserName, FGL.LossOnDisposalAssetBudgetMasterId, B.Code, B.UserName, FGL.LossOnDisposalAssetActivityId, A.Code, A.UserName
						,FGL.GainOnSaleOfAssetActivityId,FGL.GainOnSaleOfAssetBudgetMasterId
						UNION
						SELECT  'Asset' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FR.FABudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FR.FAActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							
							, NULL Dr
							, SUM(FR.FABaseAmount+isnull(SAR.subAssetAmount,0)) AS Cr
							, SUM(FR.FABaseAmount+isnull(SAR.subAssetAmount,0)) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId

						LEFT JOIN[MST].[BudgetMaster] AS BM ON FR.FABudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FR.FAActivityId= A.Id
						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FR.FABudgetMasterId, B.Code, B.UserName, FR.FAActivityId, A.Code, A.UserName
						) X 
                        WHERE X.Amount>0
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFixedAssetSalesBookAsSalesJV2List(string fixedAssetDisposeId, string companyId, string plantId)
        {

            var sql = @"DECLARE @fixedAssetDisposeId varchar(10)='" + fixedAssetDisposeId + @"', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'


						SELECT X.* FROM (
						SELECT  'A/R' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =MGPGL.GLGeneralInfoId       
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =MGPGL.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = MGPGL.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM(FR.NegotiationValue) AS Dr
							, NULL Cr
							, SUM(FR.NegotiationValue) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN MST.MaterialMaster MM ON MM.Id=FR.MaterialMasterId
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId

						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON FRD.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId 
						AND MGPGL.PartyAccountGroupId= PACG.Id AND MGPGL.GLType='Receivable'




						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGPGL.BudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  MGPGL.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName

						UNION
						SELECT  'Sales' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =MGPGL.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = MGPGL.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							
							, NULL Dr
							, SUM(FR.NegotiationValue) AS Cr
							, SUM(FR.NegotiationValue) AS Amount
						FROM  TRN.FixedAssetRegisterDisposedDetail FRDD
						LEFT JOIN TRN.FixedAssetRegisterDisposed FRD ON FRD.Id=FRDD.FixedAssetRegisterDisposedId
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FRDD.FixedAssetRegisterId
						LEFT JOIN MST.MaterialMaster MM ON MM.Id=FR.MaterialMasterId
						LEFT JOIN (SELECT MGGL.* FROM [ORG].[Company] AS C JOIN [HKP].[MaterialGroupGL] AS MGGL ON C.COAId=MGGL.COAId WHERE C.Id=@companyId)
								AS MGGL ON MM.MaterialGroupMasterId = MGGL.MaterialGroupMasterId
						LEFT JOIN(SELECT * FROM [HKP].[CompanyParty] WHERE PlantId=@plantId AND PartyType='Customer')AS CP ON FRD.PartyId = CP.PartyId
						LEFT JOIN [HKP].[PartyAccountGroup] AS PACG ON CP.PartyAccountGroupId = PACG.Id
						LEFT JOIN [HKP].[MaterialGroupPartyAccountGroupGL] AS MGPGL ON MGGL.MaterialGroupMasterId = MGPGL.MaterialGroupMasterId 
						AND MGPGL.PartyAccountGroupId= PACG.Id AND MGPGL.GLType='Sales'

						LEFT JOIN[MST].[BudgetMaster] AS BM ON MGPGL.BudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON MGPGL.ActivityId= A.Id
						LEFT JOIN ( SELECT FixedAssetRegisterId,ISNULL(Sum(Amount),0) subAssetAmount FROM TRN.SubFixedAssetRegister group by FixedAssetRegisterId) SAR ON SAR.FixedAssetRegisterId=FR.Id
						WHERE FRDD.FixedAssetRegisterDisposedId=@fixedAssetDisposeId
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, MGPGL.BudgetMasterId, B.Code, B.UserName, MGPGL.ActivityId, A.Code, A.UserName
						) X
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFixedAssetDepreciationSingleJVList(string fixedAssetMasterId, DateTime depreciationProcessDate, string companyId, string plantId)
        {

            var sql = @"DECLARE @fixedAssetMasterId varchar(10)='" + fixedAssetMasterId + "',@depreciationProcessDate DATE='" + depreciationProcessDate + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

						SELECT X.* FROM(
						SELECT  'Depreciation' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMG.DepreciationBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FAMG.DepreciationActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM( ISNULL(FDP.CurrentDepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM( ISNULL(FDP.CurrentDepreciationAmount,0)) AS Amount
					    FROM [TRN].[FixedAssetDepreciationProcess] FDP
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FDP.FixedAssetRegisterId
						LEFT JOIN HKP.FixedAssetMasterGL AS FAMG  ON FAMG.FixedAssetMasterId=FR.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FAMG.DepreciationBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FAMG.DepreciationGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FAMG.DepreciationActivityId= A.Id
					   WHERE FDP.FixedAssetMasterId=@fixedAssetMasterId AND CAST(FDP.DepreciationProcessDate AS date)=CAST(@depreciationProcessDate AS date)
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,FAMG.DepreciationBudgetMasterId,FAMG.DepreciationActivityId,FDP.FixedAssetMasterId
						
						UNION
						SELECT  'Asset' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FR.FABudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FR.FAActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, NULL Dr
							,  SUM( ISNULL(FDP.CurrentDepreciationAmount,0)) AS Cr
							,  SUM( ISNULL(FDP.CurrentDepreciationAmount,0)) AS Amount
						FROM [TRN].[FixedAssetDepreciationProcess] FDP
						LEFT JOIN TRN.FixedAssetRegister FR ON FR.Id=FDP.FixedAssetRegisterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FR.FABudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FR.FAActivityId= A.Id
						WHERE FDP.FixedAssetMasterId=@fixedAssetMasterId AND CAST(FDP.DepreciationProcessDate AS date)=CAST(@depreciationProcessDate AS date)
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, FR.FABudgetMasterId, B.Code, B.UserName, FR.FAActivityId, A.Code, A.UserName
						) X 
                        WHERE X.Amount>0
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetCapitalizationSingleJVList(string capitalizationMasterId, string companyId, string plantId)
        {
            var sql = @"DECLARE @capitalizationMasterId varchar(50)='" + capitalizationMasterId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

						SELECT X.* FROM(
						SELECT  'Asset' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMG.AssetUnderConstructionBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FAMG.AssetUnderConstructionActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							,BudgetMasterActivityId =BMA.Id
							, SUM( ISNULL(CM.TotalAmount,0)) AS Dr
							, NULL Cr
							, SUM( ISNULL(CM.TotalAmount,0)) AS Amount
					    FROM [TRN].[CapitalizationMaster] CM
						LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=CM.FixedAssetItemId
						LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAI.FixedAssetMasterId
						LEFT JOIN HKP.FixedAssetMasterGL AS FAMG  ON FAMG.FixedAssetMasterId=FAM.Id
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FAMG.AssetUnderConstructionBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FAMG.AssetUnderConstructionGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FAMG.AssetUnderConstructionActivityId= A.Id
						LEFT JOIN [MST].[BudgetMasterActivity] AS BMA ON FAMG.AssetUnderConstructionActivityId= BMA.ActivityId AND FAMG.AssetUnderConstructionBudgetMasterId= BMA.BudgetMasterId
					    WHERE CM.Id=@capitalizationMasterId 
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,FAMG.AssetUnderConstructionBudgetMasterId,FAMG.AssetUnderConstructionActivityId,BMA.Id
						
						UNION
						SELECT  'Capitalization' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =VD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = VD.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							,BudgetMasterActivityId =BMA.Id
							, NULL Dr
							,  SUM( ISNULL(CD.Amount,0)) AS Cr
							,  SUM( ISNULL(CD.Amount,0)) AS Amount
						FROM [TRN].[CapitalizationMasterDetail] CD
						LEFT JOIN TRN.VoucherDetail VD	 ON VD.Id=CD.VoucherDetailId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
						LEFT JOIN [MST].[BudgetMasterActivity] AS BMA ON VD.ActivityId= BMA.ActivityId AND VD.BudgetMasterId= BMA.BudgetMasterId
						WHERE CD.CapitalizationMasterId=@capitalizationMasterId 
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, VD.BudgetMasterId, B.Code, B.UserName, VD.ActivityId, A.Code, A.UserName,BMA.Id
						) X 
                        WHERE X.Amount>0
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetCapitalizationSingleJVListFromAssetRegister(string capitalizationMasterId, string companyId, string plantId)
        {
            var sql = @"DECLARE @capitalizationMasterId varchar(50)='" + capitalizationMasterId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

						SELECT X.* FROM(
						SELECT  'Asset' AS OtherName, 'Dr' AS TrnType,FAI.FixedAssetMasterId
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMBT.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = BMA.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							,BudgetMasterActivityId =BMA.Id
							, SUM( ISNULL(ARC.Amount,0)) AS Dr
							, NULL Cr
							, SUM( ISNULL(ARC.Amount,0)) AS Amount
					    FROM [TRN].[AssetRegister] AR
						LEFT JOIN [TRN].[AssetRegisterChild] ARC ON AR.Id=ARC.AssetRegisterId
						LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
						LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAI.FixedAssetMasterId
						LEFT JOIN [HKP].[FixedAssetMasterBudgetTag] AS FAMBT  ON FAMBT.FixedAssetMasterId=FAM.Id
						LEFT JOIN [MST].[BudgetMaster] AS BM ON FAMBT.BudgetMasterId= BM.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN (SELECT Id,BudgetMasterId,ActivityId FROM [MST].[BudgetMasterActivity] WHERE Isdefault=1 ) AS BMA ON BMA.BudgetMasterId= FAMBT.BudgetMasterId 
						LEFT JOIN [HKP].[Activity] AS A ON BMA.ActivityId= A.Id
					    WHERE ARC.CapitalizationMasterId=@capitalizationMasterId 
						GROUP BY BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,FAMBT.BudgetMasterId,BMA.ActivityId,BMA.Id,FAI.FixedAssetMasterId
						
						UNION
						SELECT  'Capitalization' AS OtherName, 'Cr' AS TrnType, '' FixedAssetMasterId
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =VD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = VD.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							,BudgetMasterActivityId =BMA.Id
							, NULL Dr
							,  SUM( ISNULL(CD.Amount,0)) AS Cr
							,  SUM( ISNULL(CD.Amount,0)) AS Amount
						FROM [TRN].[CapitalizationMasterDetail] CD
						LEFT JOIN TRN.VoucherDetail VD	 ON VD.Id=CD.VoucherDetailId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON VD.BudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
						LEFT JOIN [MST].[BudgetMasterActivity] AS BMA ON VD.ActivityId= BMA.ActivityId AND VD.BudgetMasterId= BMA.BudgetMasterId
						WHERE CD.CapitalizationMasterId=@capitalizationMasterId 
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, VD.BudgetMasterId, B.Code, B.UserName, VD.ActivityId, A.Code, A.UserName,BMA.Id
						) X  
                        WHERE X.Amount>0
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }
        public GridModel GetFixedAssetAccDepGL(GridParameter parameters, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS FixedAssetMasterId
                                ,FAM.UserName AS FixedAssetMasterName
                                ,FAD.AccumulatedDepreciationGLId
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,FAD.AccumulatedDepreciationBudgetMasterId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                
                                FROM MST.FixedAssetMaster As FAM
                                LEFT JOIN HKP.FixedAssetMasterGL AS FAD  ON FAD.FixedAssetMasterId=FAM.Id
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN MST.BudgetMaster AS ADBudgetM ON FAD.AccumulatedDepreciationBudgetMasterId = ADBudgetM.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON ADBudgetM.BudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetOpeningBalanceInfoWithFAMId(string assetMasterId, string companyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"SELECT FOBD.Quantity AS TotalRow,CC.CompanyCurrencyAmount AS FABaseAmountTotal, GC.CompanyGroupCurrencyAmount AS FAGroupAmountTotal, HC.HardCurrencyAmount AS FAHardAmountTotal, AAC.ADBaseAmountTotal,AAC.ADGroupAmountTotal,AAC.ADHardAmountTotal  FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
                INNER JOIN [TRN].[OpeningBalance] AS FOB ON FOB.Id=FOBD.OpeningBalanceId
                INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.MaterialMasterGLId
                INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
				 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
				 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.Amount AS CompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
				 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='FA'
				) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
				LEFT OUTER JOIN (
				SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId, OBDC.ToCurrencyId AS CompanyGroupToCurrencyId,
				 OBDC.ToCurrencyRate AS CompanyGroupCurrencyRate, OBDC.Amount AS CompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
				 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='FA'
				) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
				LEFT OUTER JOIN (
				 SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId, OBDC.ToCurrencyId AS HardToCurrencyId,
				 OBDC.ToCurrencyRate AS HardCurrencyRate, OBDC.Amount AS HardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
				 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='FA'
				) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
                LEFT OUTER JOIN (
                SELECT  FOBD.Id,CC.CompanyCurrencyAmount AS ADBaseAmountTotal, GC.CompanyGroupCurrencyAmount AS ADGroupAmountTotal,HC.HardCurrencyAmount AS ADHardAmountTotal
				FROM [TRN].[MaterialMasterOpeningBalanceDetail] AS FOBD
				INNER JOIN [TRN].[OpeningBalance] AS FOB ON FOB.Id=FOBD.OpeningBalanceId
				INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.AccumulatedDepreciationGLId
				INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
					 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
					 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.Amount AS CompanyCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
					 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					 WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='AD'
					) AS CC ON CC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
					LEFT OUTER JOIN (
					SELECT OBDC.ParallelCurrencyId AS CompanyGroupCurrencyId, OBDC.FromCurrencyId AS CompanyGroupFromCurrencyId, OBDC.ToCurrencyId AS CompanyGroupToCurrencyId,
					 OBDC.ToCurrencyRate AS CompanyGroupCurrencyRate, OBDC.Amount AS CompanyGroupCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
					 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					 WHERE CPC.ParallelCurrencyType='CompanyGroupCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='AD'
					) AS GC ON GC.MaterialMasterOpeningBalanceDetailId=FOBD.Id
					LEFT OUTER JOIN (
					 SELECT OBDC.ParallelCurrencyId AS HardCurrencyId, OBDC.FromCurrencyId AS HardFromCurrencyId, OBDC.ToCurrencyId AS HardToCurrencyId,
					 OBDC.ToCurrencyRate AS HardCurrencyRate, OBDC.Amount AS HardCurrencyAmount, OBDC.MaterialMasterOpeningBalanceDetailId
					 FROM [TRN].[MaterialMasterOpeningBalanceDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					 WHERE CPC.ParallelCurrencyType='HardCurrency' AND CPC.CompanyId='" + companyId + @"' AND OBDC.GLType='AD'
                    ) AS HC ON HC.MaterialMasterOpeningBalanceDetailId=FOBD.Id) AS AAC ON FOBD.Id = AAC.Id
                    WHERE FOB.IsPark=0 AND FOBD.FixedAssetMasterId='" + assetMasterId + "' AND FOB.CompanyId='" + companyId + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<Dictionary<string, object>> GetPostedAUCList(string plantId)
        {
            try
            {
                var sql = @"SELECT V.Id, V.VoucherDate, V.PostingDate, V.DocRefNo,V.SourceType, V.VoucherTypeId, V.CurrencyId, V.DocDate, V.EntityId
									, C.Code AS CurrencyCode, IIH.TotalAmount , V.VoucherNo, V.IsPark, V.Narration
									,GRNNo =  STUFF((select distinct ','+XIRD.InventoryReceiveId  from
														 TRN.InventoryIssueHistory AS XIH  
														  join TRN.InventoryReceiveDetail XIRD ON XIRD.Id=XIH.InventoryReceiveDetailId
													    where	XIH.CapitalizeVoucherDetailId=IIH.CapitalizeVoucherDetailId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,IssueNo =  STUFF((select distinct ','+XIID.InventoryIssueId  from
														 TRN.InventoryIssueHistory AS XIH  
														  join TRN.InventoryIssueDetail XIID ON XIID.Id=XIH.InventoryIssueDetailId
													    where	XIH.CapitalizeVoucherDetailId=IIH.CapitalizeVoucherDetailId  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                                    FROM TRN.VoucherDetail VD  
									  JOIN(SELECT SUM(TotalAmount) TotalAmount,CapitalizeVoucherDetailId FROM TRN.InventoryIssueHistory GROUP BY CapitalizeVoucherDetailId) IIH ON VD.Id=IIH.CapitalizeVoucherDetailId
									LEFT JOIN  TRN.[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN SCS.Currency AS C ON C.Id = V.CurrencyId
                                    WHERE V.ExchangeType IS NULL AND V.Archive=0  AND V.PlantId='" + plantId + "' AND V.SourceType='" + SourceType.FixedAssetCapitalizeJournal + @"' and vd.FAType in ('AssetCapatalized','AssetNonCapitalized')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetSearchWithCombine(GridParameter parameters, string coaId, string fixedAssetMasterIds)
        {
            try
            {
                string search = null;

                if (fixedAssetMasterIds != "''")
                {
                    search += " Where FAM.Id IN(" + fixedAssetMasterIds + ")";
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS FixedAssetMasterId
                                ,FAM.UserName AS FixedAssetMasterName
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId
                                ,C.UserName
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                ,FAD.AccumulatedDepreciationBudgetMasterId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationBudgetMasterId,FAD.DepreciationActivityId
                                ,DEPBudget.UserName AS   DepreciationBudgetName
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionBudgetMasterId
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentBudgetMasterId
                                ,FAD.DownPaymentActivityId
                                ,DPBudget.UserName AS   DownPaymentBudgetName
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountBudgetMasterId
                                ,FAD.ClearingAccountActivityId
                                ,CABudget.UserName AS   ClearingAccountBudgetName
                                ,CAActivity.UserName AS ClearingAccountActivityName
                                ,FAD.GainOnSaleOfAssetBudgetMasterId
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetBudgetMasterId
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetBudgetMasterId
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetBudgetMasterId
								,FAD.LessValueAssetActivityId
                                ,LEVBudget.UserName AS   LessValueAssetBudgetName
                                ,LEActivity.UserName AS LessValueAssetActivityName
                                FROM MST.FixedAssetMaster As FAM
                                LEFT JOIN HKP.FixedAssetMasterGL AS FAD  ON FAD.FixedAssetMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='" + coaId + @"') C ON FAD.COAId=C.Id
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId
                                LEFT JOIN MST.BudgetMaster AS ADBudgetM ON FAD.AccumulatedDepreciationBudgetMasterId = ADBudgetM.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON ADBudgetM.BudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
								LEFT JOIN MST.BudgetMaster AS DEPBudgetM ON FAD.DepreciationBudgetMasterId = DEPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     DEPBudget.Id =   DEPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   AUCBudgetM ON   FAD.AssetUnderConstructionBudgetMasterId =   AUCBudgetM.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   AUCBudget.Id =   AUCBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   DPBudgetM ON   FAD.DownPaymentBudgetMasterId =   DPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   DPBudget.Id =   DPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   CABudgetM ON   FAD.ClearingAccountBudgetMasterId =   CABudgetM.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   CABudget.Id =   CABudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id

                                LEFT JOIN MST.BudgetMaster AS   GOSBudgetM ON   FAD.GainOnSaleOfAssetBudgetMasterId = GOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   GOSBudget.Id =   GOSBudgetM.BudgetId

                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LOSBudgetM ON   FAD.LossOnSaleOfAssetBudgetMasterId =   LOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   LOSBudget.Id =   LOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LODBudgetM ON   FAD.LossOnDisposalAssetBudgetMasterId =   LODBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   LODBudget.Id =   LODBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LEVBudgetM ON   FAD.LessValueAssetBudgetMasterId =   LEVBudgetM.Id
								LEFT JOIN HKP.Budget AS   LEVBudget ON   LEVBudget.Id =   LEVBudgetM.BudgetId
								LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id  " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineWithAssing(GridParameter parameters, string coaId, string fixedAssetMasterIds)
        {
            try
            {
                string search = null;

                if (fixedAssetMasterIds != "''")
                {
                    search += " And  FAM.Id IN(" + fixedAssetMasterIds + ")";
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS FixedAssetMasterId
                                ,FAM.UserName AS FixedAssetMasterName
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId
                                ,C.UserName
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                ,FAD.AccumulatedDepreciationBudgetMasterId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationBudgetMasterId,FAD.DepreciationActivityId
                                ,DEPBudget.UserName AS   DepreciationBudgetName
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionBudgetMasterId
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentBudgetMasterId
                                ,FAD.DownPaymentActivityId
                                ,DPBudget.UserName AS   DownPaymentBudgetName
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountBudgetMasterId
                                ,FAD.ClearingAccountActivityId
                                ,CABudget.UserName AS   ClearingAccountBudgetName
                                ,CAActivity.UserName AS ClearingAccountActivityName
                                ,FAD.GainOnSaleOfAssetBudgetMasterId
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetBudgetMasterId
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetBudgetMasterId
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetBudgetMasterId
								,FAD.LessValueAssetActivityId
                                ,LEVBudget.UserName AS   LessValueAssetBudgetName
                                ,LEActivity.UserName AS LessValueAssetActivityName
                                FROM MST.FixedAssetMaster As FAM
                                LEFT JOIN HKP.FixedAssetMasterGL AS FAD  ON FAD.FixedAssetMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='" + coaId + @"') C ON FAD.COAId=C.Id
                                                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId
                                LEFT JOIN MST.BudgetMaster AS ADBudgetM ON FAD.AccumulatedDepreciationBudgetMasterId = ADBudgetM.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON ADBudgetM.BudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
								LEFT JOIN MST.BudgetMaster AS DEPBudgetM ON FAD.DepreciationBudgetMasterId = DEPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     DEPBudget.Id =   DEPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   AUCBudgetM ON   FAD.AssetUnderConstructionBudgetMasterId =   AUCBudgetM.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   AUCBudget.Id =   AUCBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   DPBudgetM ON   FAD.DownPaymentBudgetMasterId =   DPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   DPBudget.Id =   DPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   CABudgetM ON   FAD.ClearingAccountBudgetMasterId =   CABudgetM.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   CABudget.Id =   CABudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   GOSBudgetM ON   FAD.GainOnSaleOfAssetBudgetMasterId = GOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   GOSBudget.Id =   GOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LOSBudgetM ON   FAD.LossOnSaleOfAssetBudgetMasterId =   LOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   LOSBudget.Id =   LOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LODBudgetM ON   FAD.LossOnDisposalAssetBudgetMasterId =   LODBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   LODBudget.Id =   LODBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LEVBudgetM ON   FAD.LessValueAssetBudgetMasterId =   LEVBudgetM.Id
								LEFT JOIN HKP.Budget AS   LEVBudget ON   LEVBudget.Id =   LEVBudgetM.BudgetId
								LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id
                                WHERE   FAD.AccumulatedDepreciationGLId <> '' AND FAD.DepreciationGLId <> ''
                                AND FAD.DownPaymentGLId <> '' AND FAD.ClearingAccountGLId <> ''
                                AND FAD.GainOnSaleOfAssetGLId <> '' AND FAD.LossOnSaleOfAssetGLId <> ''
                                AND FAD.LossOnDisposalAssetGLId <> '' " + search + "";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineWithNotAssing(GridParameter parameters, string coaId, string fixedAssetMasterIds)
        {
            try
            {
                string search = null;

                if (fixedAssetMasterIds != "''")
                {
                    search += " And  FAM.Id IN(" + fixedAssetMasterIds + ")";
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS FixedAssetMasterId
                                ,FAM.UserName AS FixedAssetMasterName
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId
                                ,C.UserName
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                ,FAD.AccumulatedDepreciationBudgetMasterId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationBudgetMasterId,FAD.DepreciationActivityId
                                ,DEPBudget.UserName AS   DepreciationBudgetName
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionBudgetMasterId
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentBudgetMasterId
                                ,FAD.DownPaymentActivityId
                                ,DPBudget.UserName AS   DownPaymentBudgetName
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountBudgetMasterId
                                ,FAD.ClearingAccountActivityId
                                ,CABudget.UserName AS   ClearingAccountBudgetName
                                ,CAActivity.UserName AS ClearingAccountActivityName
                                ,FAD.GainOnSaleOfAssetBudgetMasterId
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetBudgetMasterId
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetBudgetMasterId
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetBudgetMasterId
								,FAD.LessValueAssetActivityId
                                ,LEVBudget.UserName AS   LessValueAssetBudgetName
                                ,LEActivity.UserName AS LessValueAssetActivityName
                                FROM MST.FixedAssetMaster As FAM
                                LEFT JOIN HKP.FixedAssetMasterGL AS FAD  ON FAD.FixedAssetMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA where isnull(Id,'') ='" + coaId + @"') C ON FAD.COAId=C.Id
                                                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId
                                LEFT JOIN MST.BudgetMaster AS ADBudgetM ON FAD.AccumulatedDepreciationBudgetMasterId = ADBudgetM.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON ADBudgetM.BudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
								LEFT JOIN MST.BudgetMaster AS DEPBudgetM ON FAD.DepreciationBudgetMasterId = DEPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     DEPBudget.Id =   DEPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   AUCBudgetM ON   FAD.AssetUnderConstructionBudgetMasterId =   AUCBudgetM.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   AUCBudget.Id =   AUCBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   DPBudgetM ON   FAD.DownPaymentBudgetMasterId =   DPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   DPBudget.Id =   DPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   CABudgetM ON   FAD.ClearingAccountBudgetMasterId =   CABudgetM.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   CABudget.Id =   CABudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   GOSBudgetM ON   FAD.GainOnSaleOfAssetBudgetMasterId = GOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   GOSBudget.Id =   GOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LOSBudgetM ON   FAD.LossOnSaleOfAssetBudgetMasterId =   LOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   LOSBudget.Id =   LOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LODBudgetM ON   FAD.LossOnDisposalAssetBudgetMasterId =   LODBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   LODBudget.Id =   LODBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LEVBudgetM ON   FAD.LessValueAssetBudgetMasterId =   LEVBudgetM.Id
								LEFT JOIN HKP.Budget AS   LEVBudget ON   LEVBudget.Id =   LEVBudgetM.BudgetId
								LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id
                               WHERE ISNULL(FAD.AccumulatedDepreciationGLId, '') = ''
                            OR ISNULL(FAD.DepreciationGLId, '') = ''OR ISNULL(FAD.AssetUnderConstructionGLId, '') = ''
                            OR ISNULL(FAD.DownPaymentGLId, '') = '' OR ISNULL(FAD.ClearingAccountGLId, '') = ''
							OR ISNULL(FAD.GainOnSaleOfAssetGLId, '') = '' OR ISNULL(FAD.LossOnSaleOfAssetGLId, '') = ''
							OR ISNULL(FAD.LossOnDisposalAssetGLId, '') = ''" + search + @" ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchWithCombineCoa(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FAD.Id
                                ,FAM.Id AS FixedAssetMasterId
                                ,FAM.UserName AS FixedAssetMasterName
                                ,FAD.AccumulatedDepreciationGLId
                                ,FAD.AssetUnderConstructionGLId
                                ,FAD.DepreciationGLId
                                ,FAD.DownPaymentGLId
                                ,FAD.ClearingAccountGLId
                                ,FAD.GainOnSaleOfAssetGLId
                                ,FAD.LossOnSaleOfAssetGLId
                                ,FAD.LossOnDisposalAssetGLId
                                ,FAD.LessValueAssetGLId
                                ,C.UserName
                                ,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS AccDepreciationGLInfo
                                ,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS DepreciationGLInfo
                                ,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS AUCGLInfo
                                ,GLGI5.AccountCode + ' - ' + GLGI5.UserName AS DownPaymentGLInfo
                                ,GLGI6.AccountCode + ' - ' + GLGI6.UserName AS ClearingAccountGLInfo
                                ,GLGI7.AccountCode + ' - ' + GLGI7.UserName AS GainOnSaleOfAssetGLInfo
                                ,GLGI8.AccountCode + ' - ' + GLGI8.UserName AS LossOnSaleOfAssetGLInfo
                                ,GLGI9.AccountCode + ' - ' + GLGI9.UserName AS LossOnDisposalAssetGLInfo
                                ,GLGI10.AccountCode + ' - ' + GLGI10.UserName AS LessValueAssetGLInfo
                                ,FAD.AccumulatedDepreciationBudgetMasterId
                                ,FAD.AccumulatedDepreciationActivityId
                                ,ADBudget.UserName AS   AccumulatedDepreciationBudgetName
                                ,ADActivity.UserName AS AccumulatedDepreciationActivityName
                                ,FAD.DepreciationBudgetMasterId,FAD.DepreciationActivityId
                                ,DEPBudget.UserName AS   DepreciationBudgetName
                                ,DEPActivity.UserName AS DepreciationActivityName
                                ,FAD.AssetUnderConstructionBudgetMasterId
                                ,FAD.AssetUnderConstructionActivityId
                                ,AUCBudget.UserName AS   AssetUnderConstructionBudgetName
                                ,AUCActivity.UserName AS AssetUnderConstructionActivityName
                                ,FAD.DownPaymentBudgetMasterId
                                ,FAD.DownPaymentActivityId
                                ,DPBudget.UserName AS   DownPaymentBudgetName
                                ,DPActivity.UserName AS DownPaymentActivityName
                                ,FAD.ClearingAccountBudgetMasterId
                                ,FAD.ClearingAccountActivityId
                                ,CABudget.UserName AS   ClearingAccountBudgetName
                                ,CAActivity.UserName AS ClearingAccountActivityName
                                ,FAD.GainOnSaleOfAssetBudgetMasterId
                                ,FAD.GainOnSaleOfAssetActivityId
                                ,GOSBudget.UserName AS   GainOnSaleOfAssetBudgetName
                                ,GOSActivity.UserName AS GainOnSaleOfAssetActivityName
                                ,FAD.LossOnSaleOfAssetBudgetMasterId
                                ,FAD.LossOnSaleOfAssetActivityId
                                ,LOSBudget.UserName AS   LossOnSaleOfAssetBudgetName
                                ,LOSActivity.UserName AS LossOnSaleOfAssetActivityName
                                ,FAD.LossOnDisposalAssetBudgetMasterId
                                ,FAD.LossOnDisposalAssetActivityId
                                ,LODBudget.UserName AS   LossOnDisposalAssetBudgetName
                                ,LODActivity.UserName AS LossOnDisposalAssetActivityName
								,FAD.LessValueAssetBudgetMasterId
								,FAD.LessValueAssetActivityId
                                ,LEVBudget.UserName AS   LessValueAssetBudgetName
                                ,LEActivity.UserName AS LessValueAssetActivityName
                                FROM MST.FixedAssetMaster As FAM
                                LEFT JOIN HKP.FixedAssetMasterGL AS FAD  ON FAD.FixedAssetMasterId=FAM.Id
                                LEFT JOIN(SELECT Id, UserName from HKP.COA) C ON FAD.COAId=C.Id
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=FAD.AccumulatedDepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=FAD.DepreciationGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FAD.AssetUnderConstructionGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI5 ON GLGI5.Id=FAD.DownPaymentGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI6 ON GLGI6.Id=FAD.ClearingAccountGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI7 ON GLGI7.Id=FAD.GainOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI8 ON GLGI8.Id=FAD.LossOnSaleOfAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI9 ON GLGI9.Id=FAD.LossOnDisposalAssetGLId
                                LEFT JOIN HKP.GLGeneralInfo AS GLGI10 ON GLGI10.Id=FAD.LessValueAssetGLId
                                LEFT JOIN MST.BudgetMaster AS ADBudgetM ON FAD.AccumulatedDepreciationBudgetMasterId = ADBudgetM.Id
                                LEFT JOIN HKP.Budget AS ADBudget ON ADBudgetM.BudgetId = ADBudget.Id
                                LEFT JOIN HKP.Activity AS ADActivity ON FAD.AccumulatedDepreciationActivityId = ADActivity.Id
								LEFT JOIN MST.BudgetMaster AS DEPBudgetM ON FAD.DepreciationBudgetMasterId = DEPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DEPBudget ON     DEPBudget.Id =   DEPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DEPActivity ON FAD.DepreciationActivityId = DEPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   AUCBudgetM ON   FAD.AssetUnderConstructionBudgetMasterId =   AUCBudgetM.Id
                                LEFT JOIN HKP.Budget AS   AUCBudget ON   AUCBudget.Id =   AUCBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS AUCActivity ON FAD.AssetUnderConstructionActivityId = AUCActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   DPBudgetM ON   FAD.DownPaymentBudgetMasterId =   DPBudgetM.Id
                                LEFT JOIN HKP.Budget AS   DPBudget ON   DPBudget.Id =   DPBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS DPActivity ON FAD.DownPaymentActivityId = DPActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   CABudgetM ON   FAD.ClearingAccountBudgetMasterId =   CABudgetM.Id
                                LEFT JOIN HKP.Budget AS   CABudget ON   CABudget.Id =   CABudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS CAActivity ON FAD.ClearingAccountActivityId = CAActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   GOSBudgetM ON   FAD.GainOnSaleOfAssetBudgetMasterId = GOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   GOSBudget ON   GOSBudget.Id =   GOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS GOSActivity ON FAD.GainOnSaleOfAssetActivityId = GOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LOSBudgetM ON   FAD.LossOnSaleOfAssetBudgetMasterId =   LOSBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LOSBudget ON   LOSBudget.Id =   LOSBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LOSActivity ON FAD.LossOnSaleOfAssetActivityId = LOSActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LODBudgetM ON   FAD.LossOnDisposalAssetBudgetMasterId =   LODBudgetM.Id
                                LEFT JOIN HKP.Budget AS   LODBudget ON   LODBudget.Id =   LODBudgetM.BudgetId
                                LEFT JOIN HKP.Activity AS LODActivity ON FAD.LossOnDisposalAssetActivityId = LODActivity.Id
                                LEFT JOIN MST.BudgetMaster AS   LEVBudgetM ON   FAD.LessValueAssetBudgetMasterId =   LEVBudgetM.Id
								LEFT JOIN HKP.Budget AS   LEVBudget ON   LEVBudget.Id =   LEVBudgetM.BudgetId
								LEFT JOIN HKP.Activity AS LEActivity ON FAD.LessValueAssetActivityId = LEActivity.Id ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        #region Entity Fixed Assets Register
        public List<Dictionary<string, object>> GetEntityFixedAssetRegisterElasticSearchDataList(string companyGroupId, string companyId, string plantId)
        {
            var sql = @"select distinct MM.UserName MaterialMaster,MMA.StandardName Article,FA.UserName AssetMaster,P.UserName Party
                , FAR.MaterialMasterId,FAR.MaterialMasterArticleId,FAR.VendorId,FAR.FixedAssetMasterId

                 ,IsAsset =case when MM.IsAsset =1 then 'Yes' else  'No'  end
				 , Machine=case when MBP.BusinessProcessName ='MachineDefinition' Then 'Yes' else 'No' end 
				 , count(FAR.FixedAssetMasterId) FACount

				-- ,sum( ISNULL(FAR.FABaseAmount,0))FABaseAmount
				-- ,sum( ISNULL(FAR.ADBaseAmount,0)) ADBaseAmount
				-- ,sum( ISNULL(FAR.FABaseAmount,0)- ISNULL(FAR.ADBaseAmount,0)) NetFixedAssetsAmount
				 -- ,sum( isnull(sar.SubAssetAmount,0))SubAssetAmount
				  --,TotalAssetsBaseAmount= sum( ISNULL(FAR.FABaseAmount,0) + (isnull(sar.SubAssetAmount,0) )) 

				 ,sum( ISNULL(FAR.FABaseAmount,0))FABaseAmount
				  ,sum( isnull(sar.SubAssetAmount,0))SubAssetAmount
				  ,sum(ISNULL(FAR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) ) TotalBaseAmount
				 ,sum( ISNULL(FAR.ADBaseAmount,0)) ADBaseAmount
				 ,sum( ISNULL(FAR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) - ISNULL(FAR.ADBaseAmount,0) ) NetFixedAssetsAmount
				  --,TotalAssetsBaseAmount= sum( ISNULL(FAR.FABaseAmount,0) + (isnull(sar.SubAssetAmount,0) )) 


		        from TRN.FixedAssetRegister FAR 
				JOIN MST.MaterialMaster MM ON MM.Id=FAR.MaterialMasterId
				JOIN MST.MaterialMasterArticle MMA ON MMA.Id=FAR.MaterialMasterArticleId
				JOIN MST.FixedAssetMaster FA ON FA.Id=FAR.FixedAssetMasterId
				LEFT JOIN HKP.Party P ON P.Id=FAR.VendorId

			    LEFT JOIN (SELECT MBP.MaterialMasterId,BP.BusinessProcessName FROM [MST].[MaterialMasterBusinessProcess] AS MBP
                LEFT JOIN [SCS].[BusinessProcess] AS BP ON MBP.BusinessProcessId = BP.Id
                WHERE BP.BusinessProcessName ='MachineDefinition') AS MBP ON MBP.MaterialMasterId=MM.Id


		        left join(select sum(Amount * CapitalizationRate) SubAssetAmount,FixedAssetRegisterId from  trn.SubFixedAssetRegister
				group by FixedAssetRegisterId
				) sar on sar.FixedAssetRegisterId=FAR.Id


		        WHERE FAR.CompanyGroupId='" + companyGroupId + "' AND FAR.CompanyId='" + companyId + "' AND FAR.PlantId='" + plantId + @"'  AND FAR.Status IS NULL
			

               GROUP BY FAR.MaterialMasterId ,MM.UserName ,MMA.StandardName ,FA.UserName,P.UserName 
			   ,MM.IsAsset,MBP.BusinessProcessName,FAR.FixedAssetMasterId
			    ,FAR.MaterialMasterId,FAR.MaterialMasterArticleId,FAR.VendorId,FAR.FixedAssetMasterId";
            return _sqlRepository.GetDataCollection(sql);

        }

        public List<Dictionary<string, object>> GetEntityFixedAssetRegisterDataList(string companyGroupId, string companyId, string plantId, string materialMasterId, string materialMasterArticleId, string fixedAssetMasterId, string vendorId, string isAsset, string machine)
        {
            var sql = @"SELECT  E.UserName Entity, D.UserName Department, FR.Id,FR.Id AS FixedAssetRegisterId,V.VoucherNo, FR.MaterialMasterArticleId, FR.MaterialMasterId
                                    , FR.SerialNo, FR.Id AssetNo, FR.InvoiceNo, MM.UserName MaterialMasterName
                                    , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                                    , FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                                    , FAM.FixedAssetSubCategoryId, FAM.AssetType

									--, ISNULL(FR.FABaseAmount,0)FABaseAmount
									--, ISNULL(sar.SubAssetAmount,0) SubAssetAmount
									--, ISNULL(FR.ADBaseAmount,0) ADBaseAmount
									--,ISNULL(FR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) - ISNULL(FR.ADBaseAmount,0) NetFixedAssetsAmount


				                 ,( ISNULL(FR.FABaseAmount,0))FABaseAmount
				                  ,( isnull(sar.SubAssetAmount,0))SubAssetAmount
				                  ,(ISNULL(FR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) ) TotalBaseAmount
				                 ,( ISNULL(FR.ADBaseAmount,0)) ADBaseAmount
				                 ,( ISNULL(FR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) - ISNULL(FR.ADBaseAmount,0) ) NetFixedAssetsAmount
				                  --,TotalAssetsBaseAmount= sum( ISNULL(FR.FABaseAmount,0) + (isnull(sar.SubAssetAmount,0) )) 


									, MMA.StandardName Article
									, FR.IsFinancial,IID.InventoryIssueId IssueNo,IRD.InventoryReceiveId GRNNo,FR.CapitalizeRegisterNo
                                    FROM [TRN].[FixedAssetRegister] FR
					                LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId=MM.Id
					                LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                                    LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                                    LEFT JOIN [MST].[FixedAssetMaster] FAM ON FR.FixedAssetMasterId= FAM.Id
                                    LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                    LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
									LEFT JOIN TRN.FixedAssetRegisterDetail FRD ON FRD.CapitalizeRegisterNo=FR.CapitalizeRegisterNo
									LEFT JOIN TRN.InventoryIssueHistory IIH ON IIH.Id=FRD.InventoryIssueHistoryId
									LEFT JOIN TRN.VoucherDetail VD ON VD.Id=IIH.CapitalizeVoucherDetailId
									LEFT JOIN TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
									LEFT JOIN TRN.Voucher V ON V.Id=VD.VoucherId 
									left join(select sum(isnull( Amount * CapitalizationRate,0)) SubAssetAmount,FixedAssetRegisterId from  trn.SubFixedAssetRegister
										group by FixedAssetRegisterId
										) sar on sar.FixedAssetRegisterId=FR.Id
		                            left join ORG.Entity E on E.Id= FR.EntityId
									left join ORG.Department D on D.Id = FR.DepartmentId

                                    WHERE FR.CompanyGroupId='" + companyGroupId + "'and FR.CompanyId='" + companyId + "' AND FR.PlantId='" + plantId + @"'
                                    and FR.Archive=0 and FR.IsAUC=0 and	FR.DisposedVoucherId IS NULL AND FR.Status IS NULL
                                    AND FR.Id NOT IN(' ')
				                     and FR.MaterialMasterId in(" + materialMasterId + ") AND FR.MaterialMasterArticleId in (" + materialMasterArticleId + ") AND FR.FixedAssetMasterId in (" + fixedAssetMasterId + @")
					                 and FR.VendorId in (" + vendorId + @") 
                                     --AND MM.IsAsset in () ";
            return _sqlRepository.GetDataCollection(sql);

        }

        #endregion

        #region  Fixed Assets Register Report for Elastis Search
        public List<Dictionary<string, object>> GetFixedAssetRegisterElasticSearchDataList(string companyGroupId, string companyId, string plantId, string materialMasterId, string materialMasterArticleId, string fixedAssetMasterId, string vendorId, string isAsset, string machine, string fromDate, string toDate)
        {
            var sql = @"select distinct MM.UserName MaterialMaster,MMA.StandardName Article,FA.UserName AssetMaster,P.UserName Party
                , FAR.MaterialMasterId,FAR.MaterialMasterArticleId,FAR.VendorId,FAR.FixedAssetMasterId

                 ,IsAsset =case when MM.IsAsset =1 then 'Yes' else  'No'  end
				 , Machine=case when MBP.BusinessProcessName ='MachineDefinition' Then 'Yes' else 'No' end 
				 , count(FAR.FixedAssetMasterId) FACount

				-- ,sum( ISNULL(FAR.FABaseAmount,0))FABaseAmount
				 --,sum( ISNULL(FAR.ADBaseAmount,0)) ADBaseAmount
				-- ,sum( ISNULL(FAR.FABaseAmount,0)- ISNULL(FAR.ADBaseAmount,0)) NetFixedAssetsAmount
				 -- ,sum( isnull(sar.SubAssetAmount,0))SubAssetAmount
				 -- ,TotalAssetsBaseAmount= sum( ISNULL(FAR.FABaseAmount,0) + (isnull(sar.SubAssetAmount,0) )) 
				 ,sum( ISNULL(FAR.FABaseAmount,0))FABaseAmount
				  ,sum( isnull(sar.SubAssetAmount,0))SubAssetAmount
				  ,sum(ISNULL(FAR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) ) TotalBaseAmount
				 ,sum( ISNULL(FAR.ADBaseAmount,0)) + SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0)) + sum( ISNULL(FAR.AdjustmentDepreciationAmount,0)) ADBaseAmount
				 ,sum( ISNULL(FAR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) - ISNULL(FAR.ADBaseAmount,0) ) - SUM(ISNULL(FADP.FixedAssetDepreciationAmount,0)) NetFixedAssetsAmount
				  --,TotalAssetsBaseAmount= sum( ISNULL(FAR.FABaseAmount,0) + (isnull(sar.SubAssetAmount,0) )) 


		        from TRN.FixedAssetRegister FAR 
				JOIN MST.MaterialMaster MM ON MM.Id=FAR.MaterialMasterId
				JOIN MST.MaterialMasterArticle MMA ON MMA.Id=FAR.MaterialMasterArticleId
				JOIN MST.FixedAssetMaster FA ON FA.Id=FAR.FixedAssetMasterId
				LEFT JOIN HKP.Party P ON P.Id=FAR.VendorId

			    LEFT JOIN (SELECT MBP.MaterialMasterId,BP.BusinessProcessName FROM [MST].[MaterialMasterBusinessProcess] AS MBP
                LEFT JOIN [SCS].[BusinessProcess] AS BP ON MBP.BusinessProcessId = BP.Id
                WHERE BP.BusinessProcessName ='MachineDefinition') AS MBP ON MBP.MaterialMasterId=MM.Id


		        left join(select sum(Amount * CapitalizationRate) SubAssetAmount,FixedAssetRegisterId from  trn.SubFixedAssetRegister
				group by FixedAssetRegisterId
				) sar on sar.FixedAssetRegisterId=FAR.Id
				LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FAR.Id

		        WHERE FAR.CompanyGroupId='" + companyGroupId + "' AND FAR.CompanyId='" + companyId + "' AND FAR.PlantId='" + plantId + @"'  AND FAR.Status is null
					  AND convert(Date,FAR.CapitalizationDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
				  --and FAR.MaterialMasterId in (" + materialMasterId + ") AND FAR.MaterialMasterArticleId in (" + materialMasterArticleId + ") AND FAR.FixedAssetMasterId in (" + fixedAssetMasterId + @")
					-- and FAR.VendorId in (" + vendorId + ") AND MM.IsAsset in (" + isAsset + ") AND MBP.BusinessProcessName in (" + machine + @")

               GROUP BY FAR.MaterialMasterId ,MM.UserName ,MMA.StandardName ,FA.UserName,P.UserName 
			   ,MM.IsAsset,MBP.BusinessProcessName,FAR.FixedAssetMasterId
			    ,FAR.MaterialMasterId,FAR.MaterialMasterArticleId,FAR.VendorId,FAR.FixedAssetMasterId";
            return _sqlRepository.GetDataCollection(sql);

        }

        public List<Dictionary<string, object>> GetFixedAssetRegisterDisposedElasticSearchDataList(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string nonPosted, string posted, string DisposeStatus)
        {
            var Posted = 0;
            if (nonPosted == "True")
            {
                Posted = 1;
            }
            if (posted == "True")
            {
                Posted = 0;
            }
            if (posted == "True" && nonPosted == "True")
            {
                Posted = 2;
            }
            var sql = @"SELECT FR.SerialNo, FR.Id AssetNo,  e.UserName Entity, D.UserName Department, FR.Model
                , FR.InvoiceNo, MM.UserName MaterialMasterName, MMA.StandardName Article,FR.[Description]
                , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                --, FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                --, FAM.FixedAssetSubCategoryId, FAM.AssetType
				,PC.Code PurchaseCurrency
				,BC.Code BaseCurrency
				,FR.Quantity
				,TUOM.UserName PurchaseUOM
                ,isnull( FR.Price,0 )PurchasePrice
				,IR.ToCurrencyRate PurchaseExchangeRate
				,isnull( FR.FABaseAmount,0)FABaseAmount
				,ISNULL(SAR.SubAssetAmount,0) SubAssetBaseAmount

				,isnull (FR.FABaseAmount,0) + (ISNULL(SAR.SubAssetAmount,0)) TotalBaseAmount
				,ISNULL(FR.ADBaseAmount,0) ADBaseAmount
				,ISNULL(FADP.FixedAssetDepreciationAmount,0) ProcessDepreciationAmount
				,ISNULL(FR.AdjustmentDepreciationAmount,0) AdjustmentDepreciationAmount
				,ISNULL(FR.FABaseAmount,0) + isnull(SAR.SubAssetAmount,0) - ISNULL(FR.ADBaseAmount,0)-ISNULL(FADP.FixedAssetDepreciationAmount,0)-ISNULL(FR.AdjustmentDepreciationAmount,0) NetFixedAssetsBaseAmount

                ,OpeningBalance = case when fr.IsOpeningBalance = 1 then 'YES' else 'NO' end
                ,format( fr.CapitalizationDate, 'dd-MMM-yyyy') CapitalizationDate
                --, FR.IsFinanciali
                ,P.UserName VendorName
                ,FR.[LifeTime]
                ,C.UserName OriginName
                ,FR.YearOfInstallation,FR.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.MaterialMasterId
                ,IR.Id GRNNo,IR.POId PONo , FADR.Description DepreciationRules
                ,FR.Status,format( fard.DocDate,'dd-MMM-yyyy')DocDate,v.VoucherNo,fard.Id DisposalNo
				,CASE WHEN fard.IsPark=0 THEN 'Posted' ELSE 'Non Posted' END PostingStatus
				,Customer.UserName CustomerName,CU.Code Currency,CAST(fard.ToCurrencyRate AS decimal(18,4))ToCurrencyRate,rdd.NegotiationValue
				 ,rdd.BaseNagotiationValue
				 ,(rdd.BaseNagotiationValue-( ISNULL(FR.FABaseAmount,0) + isnull(sar.SubAssetAmount,0) - ISNULL(FR.ADBaseAmount,0)-ISNULL(FADP.FixedAssetDepreciationAmount,0)-ISNULL(FR.AdjustmentDepreciationAmount,0)) )LossOrGain
				 ,isnull(GP.Id,GPS.Id) GatePassNo,CASE WHEN GP.GatePassEntryDate IS NOT NULL THEN format( GP.GatePassEntryDate,'dd-MMM-yyyy') 
				 ELSE format( GPS.GatePassEntryDate,'dd-MMM-yyyy') END GatePassDate
                , ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
				 ,ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue
                FROM [TRN].[FixedAssetRegister] FR
                LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId=MM.Id
                LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMT ON BM.Id=FAMT.BudgetMasterId AND MM.BudgetMasterId=FAMT.BudgetMasterId
                LEFT JOIN [MST].[FixedAssetMaster] FAM ON FR.FixedAssetMasterId= FAM.Id
                LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId=FAC.Id
                LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                LEFT JOIN HKP.Party P ON P.Id=FR.VendorId
                LEFT JOIN SCS.Country C ON C.Id=FR.CountryOfOriginId
                LEFT JOIN TRN.FixedAssetRegisterDetail FRD ON FRD.CapitalizeRegisterNo=FR.CapitalizeRegisterNo
                LEFT JOIN TRN.InventoryIssueHistory IIH ON IIH.Id=FRD.InventoryIssueHistoryId
                LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
                LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
				LEFT JOIN SCS.UnitOfMeasurement TUOM ON TUOM.Id=IRD.TransactionUoMId
				left join scs.Currency PC on PC.Id= FR.CurrencyId
				left join scs.Currency BC on BC.Id= FR.FABaseCurrencyId
				left join mst.FixedAssetDepreciationRule FADR ON FADR.Id = FR.DepreciationRuleId
                LEFT JOIN TRN.FixedAssetRegisterDisposedDetail rdd ON rdd.FixedAssetRegisterId=FR.Id
				LEFT JOIN TRN.FixedAssetRegisterDisposed fard ON rdd.FixedAssetRegisterDisposedId=fard.Id
				LEFT JOIN TRN.Voucher V ON V.Id =fard.DisposedVoucherId
				LEFT JOIN HKP.Party Customer ON Customer.Id = fard.PartyId
                LEFT JOIN SCS.Currency CU ON CU.Id =fard.CurrencyId
				LEFT JOIN [TRN].[InOutGatePassMaster] GP ON GP.FixedAssetRegisterDisposedId =fard.Id
                LEFT JOIN [TRN].[InOutGatePassMaster] GPS ON GPS.FixedAssetScrapId =fard.Id
				LEFT JOIN HKP.CharacteristicsValue AS FCV ON MM.Id=FCV.MaterialMasterId
				LEFT JOIN HKP.CharacteristicsValue AS SCV ON MM.Id=SCV.MaterialMasterId
				LEFT JOIN HKP.CharacteristicsValue AS TCV ON MM.Id=TCV.MaterialMasterId
	
                LEFT JOIN(SELECT FixedAssetRegisterId,sum(isnull( Amount * CapitalizationRate,0)) SubAssetAmount 
				FROM TRN.SubFixedAssetRegister 
				group by FixedAssetRegisterId)SAR ON SAR.FixedAssetRegisterId =FR.Id

               left join ORG.Entity E on E.Id= FR.EntityId
			   left join ORG.Department D on D.Id = FR.DepartmentId
			   LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id

		        WHERE FR.CompanyGroupId='" + companyGroupId + "' AND FR.CompanyId='" + companyId + "' AND FR.PlantId='" + plantId + @"'  AND FR.Status is not null
				AND FR.Status in (" + DisposeStatus + @") 
				AND  fard.IsPark=case when  " + Posted + @"=2 then fard.IsPark else " + Posted + @" end
				AND convert(Date,fard.DocDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 
				";
            return _sqlRepository.GetDataCollection(sql);

        }


        #endregion Fixed Assets Register Report for Elastis Search

        #region Fixed Asset Depreciation Process
        public IEnumerable<object> GetfixedAssetMastersListForProcess(string companyGroupId, string companyId, string plantId, string fiscalYearId, string toDate, string startDate)
        {
            var sql = @"DECLARE @FromDate NVARCHAR(20) = DATEADD(day,-1,'" + startDate + @"');
						DECLARE @FiscalYearId AS [varchar](20)
						select @FiscalYearId=Id from [SCS].[FiscalYear] where @FromDate BETWEEN StartDate AND EndDate
						SELECT FAM.*,
                        FAC.UserName 'FixedAssetCategory',
                        FASC.UserName 'FixedAssetSubCategory'
						,CASE WHEN ( select TOP 1 FiscalYearId from [TRN].[FixedAssetDepreciationProcess]  where FiscalYearId	='" + fiscalYearId + @"'	AND FixedAssetMasterId=FAM.Id)>0 
					    THEN 'Processed upto '+ CAST(( select TOP 1 DepreciationProcessDate from [TRN].[FixedAssetDepreciationProcess]  where FiscalYearId	='" + fiscalYearId + @"'	AND FixedAssetMasterId=FAM.Id ORDER BY Id DESC) AS varchar)
						ELSE   'Not Process' END ProcessStatus
						,(select COUNT(Id) from [TRN].[FixedAssetRegister] where CapitalizationDate<=@FromDate AND FixedAssetMasterId=FAM.Id)PreviousYearAsset
						,(select COUNT(Id) from [TRN].[FixedAssetDepreciationProcess] where FiscalYearId=@FiscalYearId AND FixedAssetMasterId=FAM.Id)PreviousYearAssetProcess
						,CASE WHEN (select TOP 1 DepreciationProcessDate from [TRN].[FixedAssetDepreciationProcess] where FiscalYearId=@FiscalYearId AND FixedAssetMasterId=FAM.Id ORDER BY Id DESC)=@FromDate THEN 'Yes' ELSE 'No' END PreviousYearAssetFullProcess
                        FROM  MST.[FixedAssetMaster]  FAM
                        LEFT OUTER JOIN  HKP.[FixedAssetCategory]  FAC ON FAM.FixedAssetCategoryId=FAC.Id
                        LEFT OUTER JOIN  HKP.[FixedAssetSubCategory]  FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                     WHERE FAM.CompanyGroupId='" + companyGroupId + @"' 
					 AND FAM.Id IN(select FixedAssetMasterId from [TRN].[FixedAssetRegister] where CapitalizationDate<='" + toDate + @"')";
            return _sqlRepository.GetDataCollection(sql);

        }
        public Dictionary<string, object> GetFiscalYearDataByFiscalYear(string fiscalYearId)
        {
            try
            {
                var sql = @"SELECT Replace(CONVERT(VARCHAR(11), FY.StartDate, 106), ' ', '-') StartDate
							,Replace(CONVERT(VARCHAR(11), FY.EndDate, 106), ' ', '-') EndDate FROM SCS.FiscalYear AS FY
                            WHERE FY.Id='" + fiscalYearId + "'";
                return _sqlRepository.GetData(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name.ToString(), null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Calendars.ToString()));
            }
        }
        public void FixedAssetDepreciationProcess(string selectedAssetMastersLists, string fiscalYearId, string toDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = @"EXEC SP_FixedAssetDepreciationProcess '" + selectedAssetMastersLists + "' ,'" + fiscalYearId + "' ,'" + toDate + "' ,'" + identity.FullName + "' ,'" + identity.IPAddress + "'";
                rdBuilder.Append(builderSql);
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());


            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public List<Dictionary<string, object>> GetDepreciationProcessDataList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select AD.Id AssetDepreciationId,AD.ProcessName,FORMAT(AD.ProcessDate, 'dd-MMM-yyyy') ProcessDate
                                    ,ISNULL((SELECT SUM(DepreciationAmount) FROM [TRN].[AssetDepreciationDetail] WHERE AssetDepreciationId=AD.Id),0) DepreciationAmount
									,BC.Code BaseCurrency,AD.CurrencyId,1 ToCurrencyRate,ISNULL(v.VoucherNo,'')VoucherNo,AD.VoucherId,AD.Status,AD.AddedDate
                FROM  [TRN].[AssetDepreciation] AD
				LEFT JOIN SCS.Currency BC ON BC.Id =AD.CurrencyId
                LEFT JOIN TRN.Voucher V ON V.Id =AD.VoucherId
                ) AS TEMP WHERE " + strkey + " order by AddedDate DESC   ";
            return _sqlRepository.GetDataCollection(sql);
        }

        #endregion

        #region Fixed Asset Depreciation POST
        public List<Dictionary<string, object>> GetFixedAssetDepreciationListForPosting(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select FR.FixedAssetMasterId
									,FAM.UserName 'FixedAssetMaster'
									,FAC.UserName 'FixedAssetCategory'
									,FASC.UserName 'FixedAssetSubCategory'
									,FORMAT(FDP.DepreciationProcessDate, 'dd-MMM-yyyy') DepreciationProcessDate
                                    ,sum( ISNULL(FDP.CurrentDepreciationAmount,0)) FixedAssetDepreciationAmount
								    ,BC.Code BaseCurrency,1 CompanyCurrencyRate,1 ToCurrencyRate
                FROM [TRN].[FixedAssetDepreciationProcess] FDP
                LEFT JOIN TRN.FixedAssetRegister FR on FR.Id=FDP.FixedAssetRegisterId
	            LEFT JOIN SCS.Currency BC ON BC.Id =FR.FABaseCurrencyId
				LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FR.FixedAssetMasterId
                LEFT  JOIN  HKP.[FixedAssetCategory]  FAC ON FAM.FixedAssetCategoryId=FAC.Id
                LEFT  JOIN  HKP.[FixedAssetSubCategory]  FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                WHERE FR.CompanyId='" + companyId + @"' AND FDP.DepreciationVoucherId IS NULL
                   GROUP BY  FR.FixedAssetMasterId,FAM.UserName,FAC.UserName,FDP.DepreciationProcessDate,FASC.UserName,BC.Code	
                ) AS TEMP WHERE " + strkey + " order by DepreciationProcessDate ASC  ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetFixedAssetDepreciationPostedList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select V.Id,FR.FixedAssetMasterId
									,FAM.UserName 'FixedAssetMaster'
									,FAC.UserName 'FixedAssetCategory'
									,FASC.UserName 'FixedAssetSubCategory'
									,FORMAT(FDP.DepreciationProcessDate, 'dd-MMM-yyyy') DepreciationProcessDate
                                    ,sum( ISNULL(FDP.CurrentDepreciationAmount,0)) FixedAssetDepreciationAmount
								    ,BC.Code BaseCurrency,1 CompanyCurrencyRate,1 ToCurrencyRate
									,V.VoucherNo,FORMAT(V.PostingDate, 'dd-MMM-yyyy') PostingDate
                FROM [TRN].[FixedAssetDepreciationProcess] FDP
                LEFT JOIN TRN.FixedAssetRegister FR on FR.Id=FDP.FixedAssetRegisterId
				INNER JOIN TRN.Voucher V ON V.Id=FDP.DepreciationVoucherId
	            LEFT JOIN SCS.Currency BC ON BC.Id =FR.FABaseCurrencyId
				LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FR.FixedAssetMasterId
                LEFT  JOIN  HKP.[FixedAssetCategory]  FAC ON FAM.FixedAssetCategoryId=FAC.Id
                LEFT  JOIN  HKP.[FixedAssetSubCategory]  FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                WHERE FR.CompanyId='" + companyId + @"' AND V.Archive=0 AND FDP.DepreciationVoucherId IS NOT NULL
                GROUP BY  V.Id,FR.FixedAssetMasterId,FAM.UserName,FAC.UserName,FDP.DepreciationProcessDate,FASC.UserName,BC.Code,V.VoucherNo,V.PostingDate ) AS TEMP WHERE " + strkey + " order by PostingDate DESC   ";
            return _sqlRepository.GetDataCollection(sql);
        }
        #endregion

        #region Capitalize Asset Register
        public List<Dictionary<string, object>> GetCapitalizeData()
        {
            string sql = @"SELECT CM.*,FORMAT(CM.CapitalizationDate,'dd-MMM-yyyy')CD,FAI.UserName FixedAssetItem,E.EmployeeName ApprovedByName, E.EmployeeCode ApprovedByEmployeeCode,Approved=CASE WHEN CM.IsApproved=1 THEN 'Approved' ELSE '' END
                            FROM [TRN].[CapitalizationMaster] CM
                            LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=CM.FixedAssetItemId
                            LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=CM.ApprovedById Order by CM.AddedDate DESC";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetCapitalizeDataList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (SELECT CM.*,FORMAT(CM.CapitalizationDate,'dd-MMM-yyyy')CD,FAI.UserName FixedAssetItem,E.EmployeeName ApprovedByName
                        ,E.EmployeeCode ApprovedByEmployeeCode,Approved=CASE WHEN CM.IsApproved=1 THEN 'Approved' ELSE '' END,FAI.FixedAssetMasterId,FAM.UserName FixedAssetMaster
                        ,CMStatus = case when CM.VoucherId is not null then 'Posted' else 'Parked' end
                        FROM [TRN].[CapitalizationMaster] CM
                        LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=CM.FixedAssetItemId
                        LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
                        LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=CM.ApprovedById
                ) AS TEMP WHERE " + strkey + " order by AddedDate DESC   ";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetApprovedCapitalizeData(string type)
        {
            string sql = @"SELECT CM.*,FORMAT(CM.CapitalizationDate,'dd-MMM-yyyy')CD,FAI.UserName FixedAssetItem,E.EmployeeName ApprovedByName, E.EmployeeCode ApprovedByEmployeeCode,Approved=CASE WHEN CM.IsApproved=1 THEN 'Approved' ELSE '' END
                        FROM [TRN].[CapitalizationMaster] CM
                        LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=CM.FixedAssetItemId
                        LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=CM.ApprovedById
                        Where CM.IsApproved=1 AND CM.VoucherId IS NULL ORDER BY CapitalizationDate ";
             return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetFixedAssetMasterItem()
        {
            string sql = @"SELECT FI.*,Uom.Code CapacityUoM,FM.UserName FixedAssetMaster,FC.UserName FixedAssetCategory,FSC.UserName FixedAssetSubCategory  
FROM MST.FixedAssetItem FI
LEFT JOIN MST.FixedAssetMaster FM ON FM.Id=FI.FixedAssetMasterId
LEFT JOIN HKP.FixedAssetCategory FC ON FC.Id=FM.FixedAssetCategoryId
LEFT JOIN HKP.FixedAssetSubCategory FSC ON FSC.Id=FM.FixedAssetSubCategoryId
LEFT JOIN SCS.UnitOfMeasurement UoM ON UoM.Id=FI.CapacityUoMId";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetCapitalizationMasterDetail(string masterId)
        {
            string sql = @"SELECT C.*,MM.UserName MaterialMasterName,MMA.StandardName ArticleStandardName,V.VoucherNo,IRD.InventoryReceiveId GRNNo, Qty=CASE WHEN IRD.BaseQty=0 THEN IH.Qty ELSE IRD.BaseQty END 
FROM [TRN].[CapitalizationMasterDetail] C
LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=C.InventoryReceiveDetailId
LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=IM.ArticleId
left join [TRN].[VoucherDetail] VD ON VD.Id=C.VoucherDetailId
left join [TRN].[Voucher] V ON V.Id=VD.VoucherId
left join TRN.InventoryIssueHistory IH ON IH.Id=InventoryIssueHistoryId
Where  C.CapitalizationMasterId='" + masterId + "'";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetCapitalizationDetailByMaster(string masterId)
        {
            string sql = @"SELECT C.*,MM.UserName MaterialMasterName,MMA.StandardName ArticleStandardName,V.VoucherNo,IRD.InventoryReceiveId GRNNo, Qty=CASE WHEN IRD.BaseQty=0 THEN IH.Qty ELSE IRD.BaseQty END 
FROM [TRN].[CapitalizationMasterDetail] C
LEFT JOIN TRN.InventoryReceiveDetail IRD ON IRD.Id=C.InventoryReceiveDetailId
LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=IM.ArticleId
left join [TRN].[VoucherDetail] VD ON VD.Id=C.VoucherDetailId
left join [TRN].[Voucher] V ON V.Id=VD.VoucherId
left join TRN.InventoryIssueHistory IH ON IH.Id=InventoryIssueHistoryId
Where  C.CapitalizationMasterId " + masterId + "";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetUnApprovedData(string EmployeeId)
        {
            string sql = @"SELECT CM.*,FORMAT(CM.CapitalizationDate,'dd-MMM-yyyy')CD,FAI.UserName FixedAssetItem,E.EmployeeName ApprovedByName, E.EmployeeCode ApprovedByEmployeeCode
FROM [TRN].[CapitalizationMaster] CM
LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=CM.FixedAssetItemId
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=CM.ApprovedById
Where CM.IsApproved=0 AND CM.ApprovedById='" + EmployeeId + "'";

            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetApprovedData(string EmployeeId)
        {
            string sql = @"SELECT CM.*,FORMAT(CM.CapitalizationDate,'dd-MMM-yyyy')CD,FAI.UserName FixedAssetItem,E.EmployeeName ApprovedByName, E.EmployeeCode ApprovedByEmployeeCode
FROM [TRN].[CapitalizationMaster] CM
LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=CM.FixedAssetItemId
LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=CM.ApprovedById
Where CM.IsApproved=1 AND CM.ApprovedById='" + EmployeeId + "'";

            return _sqlRepository.GetDataCollection(sql, null);
        }
        public void DeleteDetailData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [TRN].[CapitalizationMasterDetail] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function
        public void SaveCapitalizeData(Dictionary<string, object> data, List<Dictionary<string, object>> items, List<Dictionary<string, object>> assetRegisterList, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild = null;
            DataSet _assetRegisterData, _assetRegisterChildData = null;
            DataSet _assetRegisterAdditionData, _assetRegisterAdditionChildData = null;
            string _Id = string.Empty;
            string _CId = string.Empty;
            try
            {
                bplib.clsGenID genid = new bplib.clsGenID();

                string sql = "SELECT * FROM [TRN].[CapitalizationMaster] WHERE Id='" + data["Id"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "CapitalizationMaster", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #region items 
                if (items != null)
                {
                    objCon.OpenDataSetThroughAdapter("SELECT * FROM [TRN].[CapitalizationMasterDetail] where  CapitalizationMasterId='" + masterId + "'", out dsChild, false, "1");
                    foreach (var item in items)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        item["CapitalizationMasterId"] = masterId;
                        if (dv.Count == 0)
                        {
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "CapitalizationMaster", out _CId);

                            item["Id"] = _CId;
                            item["CapitalizationMasterId"] = masterId;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                #region AssetRegister_AssetRegisterChild
                string _AssetRegisterId = string.Empty;
                decimal AssetRegisterAmount = 0;
                decimal AssetRegisterTotalAmount = 0;
                genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AssetRegister", out _AssetRegisterId);
                string sqlAssetRegister = "SELECT * FROM [TRN].[AssetRegister] WHERE 1=2 ";
                string sqlAssetRegisterChild = "SELECT * FROM [TRN].[AssetRegisterChild] WHERE 1=2 ";
                string sqlAssetRegisterChildData = "SELECT * FROM [TRN].[AssetRegisterChild] WHERE CapitalizationMasterId='" + data["Id"] + "' ";
                objCon.OpenDataSetThroughAdapter(sqlAssetRegister, out _assetRegisterData, false, "1");
                objCon.OpenDataSetThroughAdapter(sqlAssetRegisterChild, out _assetRegisterChildData, false, "1");
                objCon.OpenDataSetThroughAdapter(sqlAssetRegisterChild, out _assetRegisterAdditionChildData, false, "1");
                AccountsCommonService _accountsCommonService = new AccountsCommonService(_sqlRepository);

                if(data["Type"].ToString()== "New")
                {
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    DataTable dtAR = _sqlRepository.GetDataTable(sqlAssetRegisterChildData);
                    if (dtAR.Rows.Count > 0)
                    {
                        builderSql = @"DELETE FROM [TRN].[AssetRegisterChild] where CapitalizationMasterId='" + data["Id"] + "'  ";
                        rdBuilder.Append(builderSql);

                        for (int i = 0; i < dtAR.Rows.Count; i++)
                        {
                            builderSql = @"DELETE FROM [TRN].[AssetRegister] where Id='" + dtAR.Rows[i]["AssetRegisterId"].ToString() + "'  ";
                            rdBuilder.Append(builderSql);
                        }
                            _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }

                    for (int i = 0; i < Int32.Parse(data["Qty"].ToString()); i++)
                    {
                        var id = _accountsCommonService.MakePK(_AssetRegisterId, i + 1, 4);
                        var assetRegisterData = new
                        {
                            Id = id,
                            FixedAssetItemId = data["FixedAssetItemId"].ToString(),
                            AddedBy = identity.Name,
                            AddedDate = System.DateTime.Now.ToString(),
                            AddedFromIP = identity.IPAddress,
                        };
                        AddNewRowAssetRegister(_assetRegisterData.Tables[0], assetRegisterData);

                        if (Int32.Parse(data["Qty"].ToString()) - 1 == i) {
                                AssetRegisterAmount = decimal.Parse(data["TotalAmount"].ToString()) - AssetRegisterTotalAmount;
                        }
                            else
                        {
                                AssetRegisterAmount = Math.Round(decimal.Parse(data["TotalAmount"].ToString()) / decimal.Parse(data["Qty"].ToString()), 2);
                        }

                    var assetRegisterChildData = new
                        {
                            Id = _accountsCommonService.MakePK(id, 1, 2),
                            FixedAssetItemId = data["FixedAssetItemId"].ToString(),
                            AssetRegisterId = id,
                            CapitalizationMasterId = masterId,
                            CapitalizationChildId = masterId+"-"+ (i + 1),
                            Amount = AssetRegisterAmount,
                            NetAmount = AssetRegisterAmount,
                            CompanyGroupId = identity.CompanyGroupId,
                            CompanyId = identity.CompanyId,
                            PlantId = identity.PlantId,
                            AddedBy = identity.Name,
                            AddedDate = System.DateTime.Now.ToString(),
                            AddedFromIP = identity.IPAddress,
                        };
                        AssetRegisterTotalAmount += AssetRegisterAmount;
                        AddNewRowAssetRegister(_assetRegisterChildData.Tables[0], assetRegisterChildData);

                    }
                }  //Addition
                else
                {
                    var rdBuilder = new System.Text.StringBuilder();
                    var builderSql = "";
                    DataTable dtAR = _sqlRepository.GetDataTable(sqlAssetRegisterChildData);
                    if (dtAR.Rows.Count > 0)
                    {
                        builderSql = @"DELETE FROM [TRN].[AssetRegisterChild] where CapitalizationMasterId='" + data["Id"] + "'  ";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }
                    var i = 0;
                    foreach (var item in assetRegisterList)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT CAST(MAX(Id) AS bigint)+1 Id FROM [TRN].[AssetRegisterChild] where  AssetRegisterId='" + item["AssetRegisterId"].ToString() + "'", out _assetRegisterAdditionData, false, "1");
                        var _assetAdditiondata = new
                        {
                            Id = _assetRegisterAdditionData.Tables[0].Rows[0]["Id"].ToString(),
                            FixedAssetItemId = item["FixedAssetItemId"].ToString(),
                            AssetRegisterId = item["AssetRegisterId"].ToString(),
                            CapitalizationMasterId = masterId,
                            CapitalizationChildId = masterId + "-" + (i + 1),
                            Amount = item["Amount"].ToString(),
                            NetAmount = item["Amount"].ToString(),
                            CompanyGroupId = identity.CompanyGroupId,
                            CompanyId = identity.CompanyId,
                            PlantId = identity.PlantId,
                            AddedBy = identity.Name,
                            AddedDate = System.DateTime.Now.ToString(),
                            AddedFromIP = identity.IPAddress,
                        };
                        i++;
                        AddNewRowAssetRegister(_assetRegisterAdditionChildData.Tables[0], _assetAdditiondata);

                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsChild, _assetRegisterData, _assetRegisterChildData, _assetRegisterAdditionChildData);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void ApproveCapitalizeData(Dictionary<string, object> data, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsChild = null;
            string _Id = string.Empty;
            string _CId = string.Empty;
            try
            {
                bplib.clsGenID genid = new bplib.clsGenID();

                string sql = "SELECT * FROM [TRN].[CapitalizationMaster] WHERE Id='" + data["Id"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "CapitalizationMaster", out _Id);

                    data["Id"] = _Id;
                    //data["Type"] = "New";

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void AddNewRowAssetRegister<T>(DataTable dt, T Data)
        {
            Dictionary<string, object> sourceData = Data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(Data, null));
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dt.Rows.Add(dr);
        }
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }
        public List<Dictionary<string, object>> GetCapitalizeAssetRegisterPostedList(string column, string value, string type, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select V.Id,FAI.FixedAssetMasterId,CM.FixedAssetItemId,CM.Id CapitalizationMasterId
									,FAM.UserName FixedAssetMaster
									,FAI.UserName FixedAssetItem
									,FAC.UserName FixedAssetCategory
									,FASC.UserName FixedAssetSubCategory
									,CM.Qty,CM.Type,V.VoucherNo,FORMAT(V.PostingDate, 'dd-MMM-yyyy') PostingDate
									,CM.TotalAmount Amount
				FROM TRN.Voucher V 
				INNER JOIN [TRN].[CapitalizationMaster] CM ON CM.VoucherId=V.Id
				LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=CM.FixedAssetItemId
				LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
                LEFT  JOIN  HKP.[FixedAssetCategory]  FAC ON FAM.FixedAssetCategoryId=FAC.Id
                LEFT  JOIN  HKP.[FixedAssetSubCategory]  FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                WHERE V.CompanyId='" + companyId + @"' AND V.Archive=0 
                ) AS TEMP WHERE " + strkey + " order by PostingDate DESC   ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetAssetRegisterList(string companyGroupId, string companyId, string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"SELECT TOP 1000 * from ( SELECT 0 Active, ARC.CapitalizationMasterId,ARC.CapitalizationChildId,ARC.Amount AssetAmount,FAI.UserName FixedAssetItem, AR.FixedAssetItemId,ARC.AssetRegisterId
							,AR.AssetSlNo, AR.RFId, AR.BarCode, AR.Status, AR.AssetCondition,AR.UserReference, AR.OldReference, AR.UserGroup, AR.Remarks 
                            FROM TRN.AssetRegisterChild ARC
							LEFT JOIN TRN.AssetRegister AR ON AR.Id=ARC.AssetRegisterId
                            LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
							LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARC.CapitalizationMasterId
                            WHERE ARC.VoucherDetailId is not null AND CM.Type='New'
                            ) AS TEMP WHERE " + strkey + " order by FixedAssetItem ASC ";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public List<Dictionary<string, object>> GetAssetRegisterChildAdditionList(string masterId)
        {
            string sql = @"SELECT 1 Active, ARC.CapitalizationMasterId,ARC.CapitalizationChildId,ARC.Amount,ARC.NetAmount,FAI.UserName FixedAssetItem, AR.FixedAssetItemId,ARC.AssetRegisterId
							,AR.AssetSlNo, AR.RFId, AR.BarCode, AR.Status, AR.AssetCondition,AR.UserReference, AR.OldReference, AR.UserGroup, AR.Remarks
                            ,(SELECT SUM(Amount) FROM TRN.AssetRegisterChild where AssetRegisterId=ARC.AssetRegisterId AND VoucherDetailId IS NOT NULL)AssetAmount
                            FROM TRN.AssetRegisterChild ARC
							LEFT JOIN TRN.AssetRegister AR ON AR.Id=ARC.AssetRegisterId
                            LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
							LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARC.CapitalizationMasterId
                            WHERE ARC.CapitalizationMasterId='" + masterId + "' AND ARC.VoucherDetailId is null AND CM.Type='Addition' ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetAssetRegisterUpdateList(string companyGroupId, string companyId, string column, string value, string capitalizationMasterId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"SELECT * from ( SELECT 0 Active, AR.Id AssetRegisterId, AR.FixedAssetItemId,FAI.UserName FixedAssetItem,FAM.UserName FixedAssetMaster, AR.AssetSlNo, AR.RFId, AR.BarCode
                            ,AR.AdditionalInfoUpdateId, AR.Status, AR.AssetCondition,AR.UserReference, AR.OldReference, AR.UserGroup, AR.Remarks 
                            ,ARC.Id AssetRegisterChildId,ARC.Amount,ARC.DepreciationAmount,CM.TotalAmount,ARC.CapitalizationMasterId,ARC.CapitalizationChildId,ARC.VoucherDetailId
                            FROM TRN.AssetRegisterChild ARC
							LEFT JOIN TRN.AssetRegister AR ON AR.Id=ARC.AssetRegisterId
                            LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
                            LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
                            LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARC.CapitalizationMasterId
                            WHERE ARC.CapitalizationMasterId='" + capitalizationMasterId + @"'
                            ) AS TEMP WHERE " + strkey + " order by AssetRegisterId ASC ";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        #endregion

        #region AdditionalInfoItem

        public void SaveAdditionalInfoItem(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from HKP.AdditionalInfoItem where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from HKP.AdditionalInfoItem where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from HKP.AdditionalInfoItem where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("AdditionalInfoItem", out _Id);

                    data["Id"] = "AI" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteAdditionalInfoItemData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM HKP.AdditionalInfoItem WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function
        public List<Dictionary<string, object>> GetAdditionalInfoItemList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (
                                                SELECT State=CAST(0 AS bit),AII.*,uom.UserName UoM 
                                                ,Man = CASE WHEN aii.IsMandatory=1 THEN 'Yes' ELSE 'No' end
                                                ,Act = CASE WHEN aii.ACTIVE=1 THEN 'Yes' ELSE 'No' end
                                                FROM HKP.AdditionalInfoItem AII
                                                LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=AII.UoMId) AS TEMP WHERE " + strkey + " order by sequence";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public double GetAdditionalInfoItemSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  ISNULL(Max(Sequence),0) AS Sequence FROM HKP.AdditionalInfoItem");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public List<Dictionary<string, object>> GetAdditionalData(string masterId)
        {

            string sql = @"SELECT M.*,A.Sequence,A.Code,A.ShortName,A.StandardName,A.UserName 
                                        FROM [TRN].[AssetItemAdditionalInfoMap] M
                                        LEFT JOIN HKP.[AdditionalInfoItem] A ON A.Id=M.AdditionalInfoItemId 
                                        Where M.FixedAssetItemId='" + masterId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public List<Dictionary<string, object>> GetAdditionalDataByAssetId(string masterId, string headerId)
        {

            string sql = @"SELECT M.AdditionalInfoItemId,A.Sequence,A.Code,A.ShortName,A.StandardName,A.UserName,uom.UserName UoM,AIUD.Id,AIUD.[Value],AIUD.Remarks
                                        FROM [TRN].[AssetItemAdditionalInfoMap] M
                                        LEFT JOIN MST.FixedAssetItem FA ON FA.Id=M.FixedAssetItemId
                                        LEFT JOIN HKP.[AdditionalInfoItem] A ON A.Id=M.AdditionalInfoItemId
                                        LEFT JOIN scs.UnitOfMeasurement AS uom ON uom.Id=FA.CapacityUoMId
                                        LEFT JOIN(SELECT * from trn.AdditionalInfoUpdateDetail WHERE ISNULL(AdditionalInfoUpdateId,'" + headerId + "')='" + headerId + @"') AIUD ON AIUD.AdditionalInfoItemId=M.AdditionalInfoItemId 
                                        Where M.FixedAssetItemId='" + masterId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetAdditionallInfoUpdateData()
        {

            string sql = @"SELECT M.*,A.UserName FixedAssetItem
                        FROM [TRN].[AdditionalInfoUpdate] M
                        LEFT JOIN MST.FixedAssetItem A ON A.Id=M.FixedAssetItemId";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetAssetRegisterData(string fixedAssetItemId)
        {

            string sql = @"SELECT Flag=CAST(0 AS bit),AR.*,FAI.UserName FixedAssetItem FROM TRN.AssetRegister AR
LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
WHERE AR.AdditionalInfoUpdateId IS NULL AND AR.FixedAssetItemId='"+ fixedAssetItemId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public IEnumerable<object> GetTaggedAssetRegisterData(string headerId)
        {

            string sql = @"SELECT AR.*,FAI.UserName FixedAssetItem FROM TRN.AssetRegister AR
LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
WHERE AR.AdditionalInfoUpdateId='"+ headerId + "'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        public List<Dictionary<string, object>> BTBPerformanceData()
        {

            string sql = @"select distinct isnull(c.FileNo,'')FileNo,isnull(b.UserName,'') Bank,isnull(bu.userName,'') Buyer,c.Id ContractId,c.ContractNo,isnull(c.MasterLCId,'')MasterLCId,c.Amount MasterLCValue
								,p.UserName SupplierName,plc.LCRef BTBLCNo,plc.LCDate,plc.Type UsancePeriod,plc.Amount Value,0 Percentage,isnull(pda.AcceptanceDate,'')AcceptanceDate
								,isnull(pda.AcceptanceAmount,0)AcceptanceAmount,isnull(i.PostingDate,'') BankAcceptanceDate,isnull(i.ActualDueDate,'') MaturityDate
								,isnull(iwo.PostingDate,'') PaymentDate,isnull(iwo.Amount,0) PaymentPaidAmount--,fn.Amount PCAmount,fn.PostingDate
								from PurchaseLc plc
								left join [HKP].[Party] p on p.Id=plc.VendorId
								left join Contract c on C.Id=plc.ContractId
								left join trn.salesorder so on so.ContractId=c.Id
								left join trn.MasterOrderItem moi on moi.Id=so.MasterOrderItemId
								left join trn.MasterOrder mo on mo.Id=moi.MasterOrderId
								left join hkp.Buyer bu on bu.Id=mo.BuyerId
								left join HKP.Bank b on b.Id=C.BankId
								left join trn.PurchaseDocAcceptance pda on pda.PurchaseLCId=plc.Id
								left join trn.Invoice i on i.PurchaseDocAcceptanceId=pda.Id
								left join trn.InvoiceWriteOff iwo on iwo.VoucherId=i.VoucherId
								left join trn.financing fn on fn.VoucherId=i.VoucherId
								left join hkp.FinancingType ft on ft.Id=fn.FinancingTypeId
								where plc.OrderSpecific='Yes'";
            return _sqlRepository.GetDataCollection(sql, null);
        }

        #endregion

        #region Asset Depreciation Process
        public IEnumerable<object> GetAssetMastersListForProcess(string companyGroupId, string companyId, string plantId, string fiscalYearId, string toDate, string startDate)
        {
            var sql = @"DECLARE @FromDate NVARCHAR(20) = DATEADD(day,-1,'" + startDate + @"');
						DECLARE @FiscalYearId AS [varchar](20)
						select @FiscalYearId=Id from [SCS].[FiscalYear] where @FromDate BETWEEN StartDate AND EndDate
						SELECT FAM.*,
                        FAC.UserName 'FixedAssetCategory',
                        FASC.UserName 'FixedAssetSubCategory'
                        ,FADR.description DepreciationRules
						,CASE WHEN ( select TOP 1 AD.FiscalYearId from [TRN].[AssetDepreciation] AD 
							INNER JOIN [TRN].[AssetDepreciationDetail] ADDS ON ADDS.AssetDepreciationId=AD.Id   where AD.FiscalYearId='" + fiscalYearId + @"'	AND ADDS.FixedAssetMasterId=FAM.Id)>0 
					    THEN 'Processed upto '+ CAST(( select TOP 1 AD.ProcessDate from [TRN].[AssetDepreciation] AD 
							INNER JOIN [TRN].[AssetDepreciationDetail] ADDS ON ADDS.AssetDepreciationId=AD.Id   where AD.FiscalYearId='" + fiscalYearId + @"'	AND ADDS.FixedAssetMasterId=FAM.Id AND ISNULL(AD.Status,'') not in ('Disposed Assets Depreciation') ORDER BY AD.ProcessDate DESC) AS varchar)
						ELSE   'Not Process' END ProcessStatus
						,(select COUNT(ARC.Id) from [TRN].[AssetRegisterChild] ARC
								LEFT JOIN [TRN].[CapitalizationMaster] CM  ON  CM.Id = ARC.CapitalizationMasterId
								LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=ARC.FixedAssetItemId where CM.CapitalizationDate<=@FromDate AND FAI.FixedAssetMasterId=FAM.Id)PreviousYearAsset
						,(select COUNT(AD.Id) from [TRN].[AssetDepreciation] AD 
							INNER JOIN [TRN].[AssetDepreciationDetail] ADDS ON ADDS.AssetDepreciationId=AD.Id  where AD.FiscalYearId=@FiscalYearId AND ADDS.FixedAssetMasterId=FAM.Id)PreviousYearAssetProcess
						,CASE WHEN (select TOP 1 AD.ProcessDate from [TRN].[AssetDepreciation] AD 
							INNER JOIN [TRN].[AssetDepreciationDetail] ADDS ON ADDS.AssetDepreciationId=AD.Id  where AD.FiscalYearId=@FiscalYearId AND ADDS.FixedAssetMasterId=FAM.Id AND ISNULL(AD.Status,'') not in ('Disposed Assets Depreciation') ORDER BY AD.ProcessDate DESC)=@FromDate THEN 'Yes' ELSE 'No' END PreviousYearAssetFullProcess
                        FROM  MST.[FixedAssetMaster]  FAM
                        LEFT OUTER JOIN  HKP.[FixedAssetCategory]  FAC ON FAM.FixedAssetCategoryId=FAC.Id
                        LEFT OUTER JOIN  HKP.[FixedAssetSubCategory]  FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                        LEFT JOIN MST.CompanyFixedAssetDepreciationRule CFADR  ON  CFADR.FixedAssetMasterId = FAM.Id 
						LEFT JOIN [MST].[FixedAssetDepreciationRule] FADR  ON  FADR.Id = CFADR.DepreciationRuleId
                     WHERE FAM.CompanyGroupId='" + companyGroupId + @"' 
					 AND FAM.Id IN(select FAI.FixedAssetMasterId from [TRN].[AssetRegisterChild] ARC
									LEFT JOIN [TRN].[CapitalizationMaster] CM  ON  CM.Id = ARC.CapitalizationMasterId
									LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=ARC.FixedAssetItemId where CM.CapitalizationDate<='" + toDate + @"' AND ARC.VoucherDetailId IS NOT NULL)";
            return _sqlRepository.GetDataCollection(sql);

        }
        public void AssetDepreciationProcess(string selectedAssetMastersLists, string fiscalYearId, string toDate, string processName)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = @"EXEC SP_AssetDepreciationProcess '" + selectedAssetMastersLists + "' ,'" + fiscalYearId + "' ,'" + toDate + "' ,'" + identity.FullName + "' ,'" + identity.IPAddress + "' ,'" + processName + "' ,'" + identity.CompanyGroupId + "' ,'" + identity.CompanyId + "' ,'" + identity.PlantId + "'";
                rdBuilder.Append(builderSql);
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());


            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        #endregion

        #region Capitalize Asset Depreciation Post
        public List<Dictionary<string, object>> GetAssetDepreciationPostedList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select V.Id,V.VoucherNo,FORMAT(V.PostingDate, 'dd-MMM-yyyy') PostingDate
									,AD.ProcessName,FORMAT(AD.ProcessDate, 'dd-MMM-yyyy') ProcessDate
                                    ,ISNULL((SELECT SUM(DepreciationAmount) FROM [TRN].[AssetDepreciationDetail] WHERE AssetDepreciationId=AD.Id),0) DepreciationAmount
									,BC.Code BaseCurrency,AD.Id AssetDepreciationId,V.PostingDate VPostingDate,V.IsPark,Status= case when V.IsPark=0 then 'Posted' else 'Parked' end
                FROM  [TRN].[AssetDepreciation] AD
				INNER JOIN TRN.Voucher V ON V.Id=AD.VoucherId
				LEFT JOIN SCS.Currency BC ON BC.Id =AD.CurrencyId ) AS TEMP WHERE " + strkey + " order by VPostingDate DESC   ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetAssetDepreciationListForPosting(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select AD.Id AssetDepreciationId,AD.ProcessName,FORMAT(AD.ProcessDate, 'dd-MMM-yyyy') ProcessDate
                                    ,ISNULL((SELECT SUM(DepreciationAmount) FROM [TRN].[AssetDepreciationDetail] WHERE AssetDepreciationId=AD.Id),0) DepreciationAmount
									,BC.Code BaseCurrency,AD.CurrencyId,1 ToCurrencyRate,AD.Status
                FROM  [TRN].[AssetDepreciation] AD
				LEFT JOIN SCS.Currency BC ON BC.Id =AD.CurrencyId 
                WHERE AD.CompanyId='" + companyId + @"' AND AD.VoucherId IS NULL
                ) AS TEMP WHERE " + strkey + " order by ProcessDate ASC  ";
            return _sqlRepository.GetDataCollection(sql);
        }
        public List<Dictionary<string, object>> GetAssetDepreciationSingleJVList(string assetDepreciationId, string companyId, string plantId)
        {

            var sql = @"DECLARE @assetDepreciationId varchar(50)='" + assetDepreciationId + "', @companyId varchar(10)='" + companyId + "', @plantId varchar(30)='" + plantId + @"'

						SELECT X.* FROM(
						SELECT  'Depreciation' AS OtherName, 'Dr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =FAMG.DepreciationBudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = FAMG.DepreciationActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, SUM( ISNULL(ADDS.DepreciationAmount,0)) AS Dr
							, NULL Cr
							, SUM( ISNULL(ADDS.DepreciationAmount,0)) AS Amount
					    FROM  [TRN].[AssetDepreciationDetail] ADDS
						LEFT JOIN HKP.FixedAssetMasterGL AS FAMG  ON FAMG.FixedAssetMasterId=ADDS.FixedAssetMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BM ON FAMG.DepreciationBudgetMasterId= BM.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GL ON FAMG.DepreciationGLId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON FAMG.DepreciationActivityId= A.Id
					   WHERE ADDS.AssetDepreciationId =@assetDepreciationId 
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, B.Code, B.UserName, A.Code, A.UserName,FAMG.DepreciationBudgetMasterId,FAMG.DepreciationActivityId
						
						UNION
						SELECT  'Asset' AS OtherName, 'Cr' AS TrnType
							,GLGeneralInfoId =BM.GLGeneralInfoId        
							,GLGeneralInfoCode =GL.AccountCode 
							,GLGeneralInfoName =GL.UserName
							,BudgetMasterId =VD.BudgetMasterId
							,BudgetCode = B.Code
							,BudgetName =B.UserName 
							,ActivityId = VD.ActivityId
							,ActivityCode = A.Code
							,ActivityName =A.UserName
							, NULL Dr
							,  SUM( ISNULL(ADDS.DepreciationAmount,0)) AS Cr
							,  SUM( ISNULL(ADDS.DepreciationAmount,0)) AS Amount
						FROM [TRN].[AssetDepreciationDetail] ADDS
						LEFT JOIN [TRN].[AssetRegisterChild]  ARC ON ARC.Id=ADDS.AssetRegisterChildId
						LEFT JOIN [TRN].[VoucherDetail]  VD ON VD.Id=ARC.VoucherDetailId
						LEFT JOIN [MST].[BudgetMaster] AS BM ON  BM.Id=VD.BudgetMasterId
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON VD.GLGeneralInfoId=GL.Id
						LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[Activity] AS A ON VD.ActivityId= A.Id
						WHERE ADDS.AssetDepreciationId=@assetDepreciationId 
						GROUP BY  BM.GLGeneralInfoId, GL.AccountCode, GL.UserName, VD.BudgetMasterId, B.Code, B.UserName, VD.ActivityId, A.Code, A.UserName
						) X 
                        WHERE X.Amount>0
						ORDER BY 2 DESC";
            return _sqlRepository.GetDataCollection(sql);
        }
        #endregion

        #region Capitalize Asset Register Report
        public List<Dictionary<string, object>> GetCapitalizeAssetRegisterDynamicDataList(string companyGroupId, string companyId, string plantId, string fromDate, string toDate)
        {
            var sql = @"DECLARE @fromDate varchar(50)='" + fromDate + @"',@toDate varchar(50)='" + toDate + @"', @companyGroupId varchar(10)='" + companyGroupId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"'

SELECT CapitalizationMasterId,	CapitalizationChildId,	DepreciationRules
,FixedAssetMasterId,FixedAssetMaster,FixedAssetItemId,FixedAssetItem,AssetRegisterId,UserReference,CapitalizationDate
,GL,Budget,Activity,Factor,LifeTime
,(( AssetAmount +AdditionAssetAmount)-ISNULL((OpeningDepreciationAmount+AdjustmentDepreciationAmount),0)) OpeningAmount
, (AdditionAssetAmountFTP)CapitalizedAmountFTP
,(( AssetAmount +AdditionAssetAmount+AdditionAssetAmountFTP)-ISNULL((OpeningDepreciationAmount+AdjustmentDepreciationAmount),0)) TotalAmount
,ISNULL((DepreciationAmountFTP),0)DepreciationAmount
,DisposeAmount=case when DisposeAmount>0 then ISNULL((DisposeAmount),0)-ISNULL((OpeningDepreciationAmount+AdjustmentDepreciationAmount+DepreciationAmountFTP),0) else 0 end
,(( AssetAmount +AdditionAssetAmount+AdditionAssetAmountFTP)-ISNULL((OpeningDepreciationAmount+AdjustmentDepreciationAmount+DepreciationAmountFTP),0)-(case when DisposeAmount>0 then ISNULL((DisposeAmount),0)-ISNULL((OpeningDepreciationAmount+AdjustmentDepreciationAmount+DepreciationAmountFTP),0) else 0 end)) NetAmount 	

FROM(SELECT ARC.CapitalizationMasterId,ARC.CapitalizationChildId,FADR.DepreciationRules,ARC.Amount AssetAmount,ISNULL(ARC.AdjustmentDepreciationAmount,0)AdjustmentDepreciationAmount,ARC.NetAmount
							,FAI.FixedAssetMasterId,FAM.UserName FixedAssetMaster,FAI.UserName FixedAssetItem, AR.FixedAssetItemId,ARC.AssetRegisterId,REPLACE(CONVERT(VARCHAR(11), CM.CapitalizationDate, 106), ' ', '-') CapitalizationDate
							,GL.UserName GL,B.UserName Budget,A.UserName Activity
							,AR.AssetSlNo, AR.RFId, AR.BarCode, AR.Status, AR.AssetCondition,AR.UserReference, AR.OldReference, AR.UserGroup, AR.Remarks  
							,FADR.Factor,FADR.LifeTime
								,ISNULL((SELECT (SUM(ARCA.Amount)-ISNULL(SUM(ISNULL(ARCA.AdjustmentDepreciationAmount,0)),0)) AdditionAssetAmount
										FROM TRN.AssetRegisterChild ARCA
										LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARCA.CapitalizationMasterId
										WHERE AssetRegisterId=ARC.AssetRegisterId  AND ARCA.CompanyGroupId=@companyGroupId AND ARCA.CompanyId=@companyId  AND ARCA.PlantId=@plantId  AND ARCA.VoucherDetailId is not null AND CM.Type='Addition'
										AND convert(Date,CM.CapitalizationDate) <  @fromDate),0)AdditionAssetAmount
								,ISNULL((SELECT (SUM(ARCA.Amount)-ISNULL(SUM(ISNULL(ARCA.AdjustmentDepreciationAmount,0)),0)) AdditionAssetAmount
										FROM TRN.AssetRegisterChild ARCA
										LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARCA.CapitalizationMasterId
										WHERE AssetRegisterId=ARC.AssetRegisterId  AND ARCA.CompanyGroupId=@companyGroupId AND ARCA.CompanyId=@companyId  AND ARCA.PlantId=@plantId  AND ARCA.VoucherDetailId is not null AND CM.Type='Addition'
										AND convert(Date,CM.CapitalizationDate) BETWEEN  @fromDate AND @toDate),0)AdditionAssetAmountFTP
								,ISNULL((SELECT SUM(DepreciationAmount)DepreciationAmount FROM  [TRN].[AssetDepreciation] AD
									INNER JOIN [TRN].[AssetDepreciationDetail] ADDS ON ADDS.AssetDepreciationId=AD.Id
									WHERE ADDS.AssetRegisterId=ARC.AssetRegisterId AND  AD.VoucherId IS NOT NULL AND AD.ProcessDate <  @fromDate ),0)OpeningDepreciationAmount
								,ISNULL((SELECT SUM(DepreciationAmount)DepreciationAmount FROM  [TRN].[AssetDepreciation] AD
									INNER JOIN [TRN].[AssetDepreciationDetail] ADDS ON ADDS.AssetDepreciationId=AD.Id
									WHERE ADDS.AssetRegisterId=ARC.AssetRegisterId AND  AD.VoucherId IS NOT NULL AND AD.ProcessDate BETWEEN  @fromDate AND @toDate),0)DepreciationAmountFTP
								,ISNULL((SELECT SUM(AD.Amount) DisposeAmount
								
								FROM  TRN.AssetRegisterChild AD
									INNER JOIN TRN.AssetRegister XAR ON XAR.Id=AD.AssetRegisterId
									INNER JOIN [TRN].[FixedAssetRegisterDisposedDetail] FADD ON FADD.AssetRegisterId=AR.Id
									INNER JOIN [TRN].[FixedAssetRegisterDisposed] FAD ON FAD.Id=FADD.FixedAssetRegisterDisposedId
									INNER JOIN [TRN].[Voucher] V ON V.Id=FAD.DisposedVoucherId
									WHERE XAR.Id=AR.Id  and V.PostingDate BETWEEN  @fromDate AND @toDate),0)DisposeAmount
								
							FROM TRN.AssetRegisterChild  ARC
							LEFT JOIN TRN.AssetRegister AR ON AR.Id=ARC.AssetRegisterId
                            LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
                            LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
							LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARC.CapitalizationMasterId
							LEFT JOIN MST.CompanyFixedAssetDepreciationRule CFADR  ON  CFADR.FixedAssetMasterId = FAI.FixedAssetMasterId 
							LEFT JOIN [MST].[FixedAssetDepreciationRule] FADR  ON  FADR.Id = CFADR.DepreciationRuleId 
							left join trn.VoucherDetail VD on VD.Id = ARC.VoucherdetailId
							left join TRN.Voucher V on V.Id = CM.VoucherId
							left join hkp.GLGeneralInfo GL ON GL.Id=vd.GLGeneralInfoId
							left join MST.BudgetMaster BM ON BM.Id=vd.BudgetMasterId
							left join hkp.Budget B ON B.Id=BM.BudgetId
							left join hkp.Activity A ON A.Id=vd.ActivityId
							left join mst.BudgetMasterActivity bma ON bma.BudgetMasterId=VD.BudgetMasterId and bma.ActivityId=VD.ActivityId
							LEFT JOIN [TRN].[FixedAssetRegisterDisposedDetail] FADD ON FADD.AssetRegisterId=AR.iD
		                WHERE ARC.CompanyGroupId=@companyGroupId AND ARC.CompanyId=@companyId  AND ARC.PlantId=@plantId  AND ARC.VoucherDetailId is not null AND CM.Type='New'
					    AND convert(Date,CM.CapitalizationDate) <  @fromDate ) T

UNION ALL
SELECT CapitalizationMasterId,	CapitalizationChildId,	DepreciationRules
,FixedAssetMasterId,FixedAssetMaster,FixedAssetItemId,FixedAssetItem,AssetRegisterId,UserReference,CapitalizationDate
,GL,Budget,Activity,Factor,LifeTime
,0 OpeningAmount, ( AssetAmount +AdditionAssetAmountFTP)CapitalizedAmountFTP, ( AssetAmount +AdditionAssetAmountFTP)TotalAmount
,ISNULL((DepreciationAmountFTP+AdjustmentDepreciationAmount),0)DepreciationAmount
,ISNULL((DisposeAmount),0)  DisposeAmount
,(( AssetAmount +AdditionAssetAmountFTP)-ISNULL((DepreciationAmountFTP+AdjustmentDepreciationAmount),0))NetAmount 	

FROM(SELECT ARC.CapitalizationMasterId,ARC.CapitalizationChildId,FADR.DepreciationRules,ARC.Amount AssetAmount,ISNULL(ARC.AdjustmentDepreciationAmount,0)AdjustmentDepreciationAmount,ARC.NetAmount
							,FAI.FixedAssetMasterId,FAM.UserName FixedAssetMaster,FAI.UserName FixedAssetItem, AR.FixedAssetItemId,ARC.AssetRegisterId,REPLACE(CONVERT(VARCHAR(11), CM.CapitalizationDate, 106), ' ', '-') CapitalizationDate
							,GL.UserName GL,B.UserName Budget,A.UserName Activity
							,AR.AssetSlNo, AR.RFId, AR.BarCode, AR.Status, AR.AssetCondition,AR.UserReference, AR.OldReference, AR.UserGroup, AR.Remarks 
							,FADR.Factor,FADR.LifeTime
							,ISNULL((SELECT (SUM(ARCA.Amount)-ISNULL(SUM(ISNULL(ARCA.AdjustmentDepreciationAmount,0)),0)) AdditionAssetAmount
										FROM TRN.AssetRegisterChild ARCA
										LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARCA.CapitalizationMasterId
										WHERE AssetRegisterId=ARC.AssetRegisterId  AND ARCA.CompanyGroupId=@companyGroupId AND ARCA.CompanyId=@companyId  AND ARCA.PlantId=@plantId  AND ARCA.VoucherDetailId is not null AND CM.Type='Addition'
										AND convert(Date,CM.CapitalizationDate) BETWEEN  @fromDate AND @toDate),0)AdditionAssetAmountFTP
							,ISNULL((SELECT SUM(DepreciationAmount)DepreciationAmount FROM  [TRN].[AssetDepreciation] AD
									INNER JOIN [TRN].[AssetDepreciationDetail] ADDS ON ADDS.AssetDepreciationId=AD.Id
									WHERE ADDS.AssetRegisterId=ARC.AssetRegisterId AND  AD.VoucherId IS NOT NULL AND AD.ProcessDate BETWEEN  @fromDate AND @toDate),0)DepreciationAmountFTP
							,ISNULL((SELECT SUM(AD.Amount) DisposeAmount
								FROM  TRN.AssetRegisterChild AD
									INNER JOIN TRN.AssetRegister XAR ON XAR.Id=AD.AssetRegisterId
									INNER JOIN [TRN].[FixedAssetRegisterDisposedDetail] FADD ON FADD.AssetRegisterId=AR.Id
									INNER JOIN [TRN].[FixedAssetRegisterDisposed] FAD ON FAD.Id=FADD.FixedAssetRegisterDisposedId
									INNER JOIN [TRN].[Voucher] V ON V.Id=FAD.DisposedVoucherId
									WHERE XAR.Id=AR.Id  and V.PostingDate BETWEEN  @fromDate AND @toDate),0)DisposeAmount
									
                            FROM TRN.AssetRegisterChild ARC
							LEFT JOIN TRN.AssetRegister AR ON AR.Id=ARC.AssetRegisterId
                            LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
                            LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
							LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARC.CapitalizationMasterId
							LEFT JOIN MST.CompanyFixedAssetDepreciationRule CFADR  ON  CFADR.FixedAssetMasterId = FAI.FixedAssetMasterId 
							LEFT JOIN [MST].[FixedAssetDepreciationRule] FADR  ON  FADR.Id = CFADR.DepreciationRuleId 
							left join trn.VoucherDetail VD on VD.Id = ARC.VoucherdetailId
							left join TRN.Voucher V on V.Id = CM.VoucherId
							left join hkp.GLGeneralInfo GL ON GL.Id=vd.GLGeneralInfoId
							left join MST.BudgetMaster BM ON BM.Id=vd.BudgetMasterId
							left join hkp.Budget B ON B.Id=BM.BudgetId
							left join hkp.Activity A ON A.Id=vd.ActivityId
							left join mst.BudgetMasterActivity bma ON bma.BudgetMasterId=VD.BudgetMasterId and bma.ActivityId=VD.ActivityId
							LEFT JOIN [TRN].[FixedAssetRegisterDisposedDetail] FADD ON FADD.AssetRegisterId=AR.iD
		                WHERE ARC.CompanyGroupId=@companyGroupId AND ARC.CompanyId=@companyId  AND ARC.PlantId=@plantId  AND ARC.VoucherDetailId is not null AND CM.Type='New'

					    AND convert(Date,CM.CapitalizationDate) BETWEEN  @fromDate AND @toDate) T
				        ORDER BY FixedAssetMaster,FixedAssetItem  ";
            return _sqlRepository.GetDataCollection(sql);

        }

        public List<Dictionary<string, object>> GetAssetRegisterElasticSearchDataList(string companyGroupId, string companyId, string plantId, string fromDate, string toDate)
        {
            var sql = @"DECLARE @fromDate varchar(50)='" + fromDate + @"',@toDate varchar(50)='" + toDate + @"', @companyGroupId varchar(10)='" + companyGroupId + @"', @companyId varchar(10)='" + companyId + @"', @plantId varchar(30)='" + plantId + @"'

SELECT CapitalizationMasterId,	CapitalizationChildId,	DepreciationRules,Factor,LifeTime,SalvageValue,Description
,FixedAssetMasterId,FixedAssetMaster,FixedAssetItemId,FixedAssetItem,AssetRegisterId,UserReference,CapitalizationDate
,AssetAmount,AdditionAssetAmount, ( AssetAmount +AdditionAssetAmount)TotalAmount
,ISNULL(DepreciationAmount,0)DepreciationAmount,AdditionDepreciationAmount,AdjustmentDepreciationAmount,ISNULL((DepreciationAmount+AdditionDepreciationAmount+AdjustmentDepreciationAmount),0)TotalDepreciation
,(( AssetAmount +AdditionAssetAmount)-ISNULL((DepreciationAmount+AdditionDepreciationAmount+AdjustmentDepreciationAmount),0))NetAmount 	

FROM(SELECT ARC.CapitalizationMasterId,ARC.CapitalizationChildId,FADR.DepreciationRules,ARC.Amount AssetAmount,ARC.DepreciationAmount,ISNULL(ARC.AdjustmentDepreciationAmount,0)AdjustmentDepreciationAmount,ARC.NetAmount
							,FAI.FixedAssetMasterId,FAM.UserName FixedAssetMaster,FAI.UserName FixedAssetItem, AR.FixedAssetItemId,ARC.AssetRegisterId,REPLACE(CONVERT(VARCHAR(11), CM.CapitalizationDate, 106), ' ', '-') CapitalizationDate
							,AR.AssetSlNo, AR.RFId, AR.BarCode, AR.Status, AR.AssetCondition,AR.UserReference, AR.OldReference, AR.UserGroup, AR.Remarks  ,AR.AdditionalInfoUpdateId
							,FADR.Factor,FADR.LifeTime,FADR.SalvageValue,FADR.Description
                           ,STUFF((select distinct ','+AI.UserName+'-'+ CAST(AUD.Value AS varchar)
						        FROM [TRN].[AdditionalInfoUpdateDetail] AUD
						        INNER JOIN [TRN].[AdditionalInfoUpdate] AU ON AU.Id=AUD.AdditionalInfoUpdateId						
						        INNER JOIN [HKP].[AdditionalInfoItem] AI ON AI.Id=AUD.AdditionalInfoItemId
						        WHERE AUD.AdditionalInfoUpdateId=AR.AdditionalInfoUpdateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')AdditionalInfo
								,ISNULL((SELECT SUM(ARCA.Amount) AdditionAssetAmount
										FROM TRN.AssetRegisterChild ARCA
										LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARCA.CapitalizationMasterId
										WHERE AssetRegisterId=ARC.AssetRegisterId  AND ARCA.CompanyGroupId=@companyGroupId AND ARCA.CompanyId=@companyId  AND ARCA.PlantId=@plantId  AND ARCA.VoucherDetailId is not null AND CM.Type='Addition'
										AND convert(Date,CM.CapitalizationDate) BETWEEN  @fromDate AND @toDate),0)AdditionAssetAmount
								,ISNULL((SELECT SUM(ARCA.DepreciationAmount) AdditionDepreciationAmount
									FROM TRN.AssetRegisterChild ARCA
									LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARCA.CapitalizationMasterId
									WHERE AssetRegisterId=ARC.AssetRegisterId  AND ARCA.CompanyGroupId=@companyGroupId AND ARCA.CompanyId=@companyId  AND ARCA.PlantId=@plantId  AND ARCA.VoucherDetailId is not null AND CM.Type='Addition'
									AND convert(Date,CM.CapitalizationDate) BETWEEN  @fromDate AND @toDate),0)AdditionDepreciationAmount
                            FROM TRN.AssetRegisterChild ARC
							LEFT JOIN TRN.AssetRegister AR ON AR.Id=ARC.AssetRegisterId
                            LEFT JOIN MST.FixedAssetItem FAI ON FAI.Id=AR.FixedAssetItemId
                            LEFT JOIN MST.[FixedAssetMaster]  FAM ON FAM.Id=FAI.FixedAssetMasterId
							LEFT JOIN [TRN].[CapitalizationMaster] CM ON CM.Id=ARC.CapitalizationMasterId
							LEFT JOIN MST.CompanyFixedAssetDepreciationRule CFADR  ON  CFADR.FixedAssetMasterId = FAI.FixedAssetMasterId 
							LEFT JOIN [MST].[FixedAssetDepreciationRule] FADR  ON  FADR.Id = CFADR.DepreciationRuleId 
		                WHERE ARC.CompanyGroupId=@companyGroupId AND ARC.CompanyId=@companyId  AND ARC.PlantId=@plantId  AND ARC.VoucherDetailId is not null AND CM.Type='New'
						AND ARC.AssetRegisterId NOT IN (SELECT AssetRegisterId FROM [TRN].[FixedAssetRegisterDisposedDetail])
					    AND convert(Date,CM.CapitalizationDate) BETWEEN  @fromDate AND @toDate) T
				        ORDER BY FixedAssetMaster,FixedAssetItem  ";
            return _sqlRepository.GetDataCollection(sql);

        }
        #endregion

        #region
        public List<Dictionary<string, object>> GetAssetDepreciationProcessList(string column, string value, string companyId)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";
            var sql = @"select top 100 * from (select AD.Id AssetDepreciationId,AD.ProcessName,FORMAT(AD.ProcessDate, 'dd-MMM-yyyy') ProcessDate
                                    ,ISNULL((SELECT SUM(DepreciationAmount) FROM [TRN].[AssetDepreciationDetail] WHERE AssetDepreciationId=AD.Id),0) DepreciationAmount
									,BC.Code BaseCurrency,AD.CurrencyId,1 ToCurrencyRate
                FROM  [TRN].[AssetDepreciation] AD
				LEFT JOIN SCS.Currency BC ON BC.Id =AD.CurrencyId 
                WHERE AD.CompanyId='" + companyId + @"' 
                ) AS TEMP WHERE " + strkey + " order by ProcessDate ASC  ";
            return _sqlRepository.GetDataCollection(sql);
        }
        #endregion
    }
}
