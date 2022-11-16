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

namespace Library.Accounting.FixedAssets
{
    public class FixedAssetQueryService
    {
        private readonly ISqlRepository _sqlRepository;
        public FixedAssetQueryService(ISqlRepository sqlRepository )
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
        public GridModel GetAssetItemList(GridParameter parameters,string asset,string consumable)
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
        
        
        public List<Dictionary<string, object>> GetFixedAssetRegisterPopUpList(string column, string value,string companyId)
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
                                    AND FARD.FixedAssetRegisterDisposedId='"+ fixedAssetRegisterDisposeId +@"'
                                       ) AS TEMP 
                                    --WHERE FR.Id= order by SerialNo 
                                        ";
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
                 where frd.Id='" + id+"'";
            return _sqlRepository.GetDataCollection(sql);
        }

        public List<Dictionary<string, object>> GetFixedAssetLostJVList(string fixedAssetDisposeId, string companyId,string plantId)
        {

            var sql = @"DECLARE @receiveId varchar(10)='"+ fixedAssetDisposeId + "', @companyId varchar(10)='"+ companyId + "', @plantId varchar(30)='"+ plantId + @"'

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

		public List<Dictionary<string, object>> GetFixedAssetSalesSingleJVList(string fixedAssetDisposeId, string companyId, string plantId)
		{

			var sql = @"DECLARE @fixedAssetDisposeId varchar(10)='" + fixedAssetDisposeId + "', @companyId varchar(10)='"+ companyId + "', @plantId varchar(30)='"+ plantId + @"'

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

            var sql = @"DECLARE @fixedAssetDisposeId varchar(10)='" + fixedAssetDisposeId + @"', @companyId varchar(10)='"+ companyId + "', @plantId varchar(30)='"+ plantId +  @"'

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

        public List<Dictionary<string, object>> GetPostedAUCList( string plantId)
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

                                    WHERE FR.CompanyGroupId='" + companyGroupId+"'and FR.CompanyId='"+companyId+"' AND FR.PlantId='"+plantId+ @"'
                                    and FR.Archive=0 and FR.IsAUC=0 and	FR.DisposedVoucherId IS NULL AND FR.Status IS NULL
                                    AND FR.Id NOT IN(' ')
				                     and FR.MaterialMasterId in(" + materialMasterId+") AND FR.MaterialMasterArticleId in ("+materialMasterArticleId+") AND FR.FixedAssetMasterId in ("+fixedAssetMasterId+@")
					                 and FR.VendorId in ("+vendorId+@") 
                                     --AND MM.IsAsset in () ";
            return _sqlRepository.GetDataCollection(sql);

        }

        #endregion

        #region  Fixed Assets Register Report for Elastis Search
        public List<Dictionary<string, object>> GetFixedAssetRegisterElasticSearchDataList(string companyGroupId, string companyId, string plantId, string materialMasterId, string materialMasterArticleId, string fixedAssetMasterId, string vendorId, string isAsset, string machine)
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
		public IEnumerable<object> GetfixedAssetMastersListForProcess(string companyGroupId, string companyId, string plantId, string fiscalYearId, string toDate,string startDate)
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
		#endregion


	}
}
