using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.FixedAsset;
using Library.Model.FixedAssets;
using Library.Model.Inventory;
using Library.Model.Materials;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Extension.Accounts;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Systems;
using Library.Service.Vouchers;
using Library.ViewModel.Accounts;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.FixedAssets
{
    public class FixedAssetRegisterService : Service<FixedAssetRegister>, IFixedAssetRegisterService
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IRepositoryAsync<VoucherDetail> _voucherDetailRepository;
        private readonly IRepositoryAsync<InventoryIssueHistory> _inventoryIssueHistoryRepository;
        private readonly IRepositoryAsync<FixedAssetRegisterDetail> _fixedAssetRegisterDetailRepository;
        private readonly IRepositoryAsync<FixedAssetRegister> _fixedAssetRegisterRepository;
        private readonly IRepositoryAsync<FixedAssetRegisterDisposed> _fixedAssetRegisterDisposedRepository;
        private readonly IRepositoryAsync<FixedAssetRegisterDisposedDetail> _fixedAssetRegisterDisposedDetailRepository;
        private readonly IRepositoryAsync<FixedAssetRegisterDisposedTax> _fixedAssetRegisterDisposedTaxRepository;
        private readonly IRepositoryAsync<SubFixedAssetRegister> _subFixedAssetRegisterRepository;
        private readonly IFixedAssetRegisterCharacteristicsValueService _fixedAssetRegisterSkuValueService;
        private readonly IRepositoryAsync<FixedAssetMasterGL> _fixedAssetMasterGLepository;
        private readonly IRepositoryAsync<FixedAssetRegisterCharacteristicsValue> _faRegisterSquRepository;
        private readonly IRepositoryAsync<MaterialMaster> _materialMasterRepository;
        private readonly IRepositoryAsync<FixedAssetMasterBudgetTag> _fixedAssetMasterBudgetTagRepository;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IVoucherService _voucherService;
        private readonly IPlantService _plantService;

        public FixedAssetRegisterService(
            IRepositoryAsync<FixedAssetRegister> fixedAssetRegisterRepository
            , IRepositoryAsync<VoucherDetail> voucherDetailRepository
            , IRepositoryAsync<InventoryIssueHistory> inventoryIssueHistoryRepository
            , IRepositoryAsync<FixedAssetRegisterDetail> fixedAssetRegisterDetailRepository
            , IRepositoryAsync<FixedAssetRegisterDisposed> fixedAssetRegisterDisposedRepository
            , IRepositoryAsync<FixedAssetRegisterDisposedDetail> fixedAssetRegisterDisposedDetailRepository
            , IRepositoryAsync<FixedAssetRegisterDisposedTax> fixedAssetRegisterDisposedTaxRepository
            , IRepositoryAsync<SubFixedAssetRegister> subFixedAssetRegisterRepository
            , IPKGeneratorService pkGeneratorService
            , IFixedAssetRegisterCharacteristicsValueService fixedAssetRegisterSkuValueService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<FixedAssetMasterGL> faMasterRepository
            , IRepositoryAsync<FixedAssetRegisterCharacteristicsValue> faRegisterSquRepository
            , IRepositoryAsync<MaterialMaster> materialMasterRepository
            , IRepositoryAsync<FixedAssetMasterBudgetTag> fixedAssetMasterBudgetTagRepository
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IVoucherService voucherService
            , IPlantService plantService
            ) : base(fixedAssetRegisterRepository, unitOfWork, pkGeneratorService)
        {
            _fixedAssetRegisterRepository = fixedAssetRegisterRepository;
            _fixedAssetRegisterDisposedRepository = fixedAssetRegisterDisposedRepository;
            _voucherDetailRepository = voucherDetailRepository;
            _inventoryIssueHistoryRepository = inventoryIssueHistoryRepository;
            _subFixedAssetRegisterRepository = subFixedAssetRegisterRepository;
            _fixedAssetRegisterDetailRepository = fixedAssetRegisterDetailRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _fixedAssetRegisterSkuValueService = fixedAssetRegisterSkuValueService;
            _sqlRepository = sqlRepository;
            _fixedAssetMasterGLepository = faMasterRepository;
            _faRegisterSquRepository = faRegisterSquRepository;
            _materialMasterRepository = materialMasterRepository;
            _fixedAssetMasterBudgetTagRepository = fixedAssetMasterBudgetTagRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _voucherService = voucherService;
            _plantService = plantService;
            _fixedAssetRegisterDisposedDetailRepository = fixedAssetRegisterDisposedDetailRepository;
            _fixedAssetRegisterDisposedTaxRepository = fixedAssetRegisterDisposedTaxRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(FixedAssetRegister), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        private string GetCaptalizeRegisterPK()
        {
            return _pkGeneratorService.GetAutoNumber("CaptalizeRegister", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        private string GetSubFixedAssetRegisterPK()
        {
            return _pkGeneratorService.GetAutoNumber("SubFixedAssetRegister", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        private string GetFixedAssetRegisterDetailPK()
        {
            return _pkGeneratorService.GetAutoNumber("FixedAssetRegisterDetail", PKGeneratorEnum.Auto, null, DateTime.Now);
        }
        public FixedAssetRegister GetItem(string PK)
        {
            try
            {
                var _sql = "select * from TRN.[FixedAssetRegister] where Id='" + PK + "'  and Archive=0";
                return _fixedAssetRegisterRepository.SelectQuery(_sql, "").FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }



        public IEnumerable<object> GetRegisterInfoWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string companyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT COUNT(FAR.Id) AS TotalRow
                            , SUM(FAR.FABaseAmount) AS FABaseAmountTotal
                            , SUM(FAR.FAGroupAmount) AS FAGroupAmountTotal
                            , SUM(FAR.FAHardAmount) AS FAHardAmountTotal
                            , SUM(FAR.ADBaseAmount) AS ADBaseAmountTotal
                            , SUM(FAR.ADGroupAmount) AS ADGroupAmountTotal
                            , SUM(FAR.ADHardAmount) AS ADHardAmountTotal
                            FROM [TRN].[FixedAssetRegister] FAR
                            LEFT JOIN MST.MaterialMaster MM ON FAR.MaterialMasterId=MM.Id
                            LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                            LEFT JOIN HKP.GLGeneralInfo GL ON BM.GLGeneralInfoId=GL.Id
                            WHERE MM.BudgetMasterId='" + budgetMasterId + "' AND BM.GLGeneralInfoId='" + assetGLId + @"'
                            AND FAR.CompanyId='" + identity.CompanyId + "' AND FAR.IsOpeningBalance=1 AND FAR.IsFinancial=1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetRegisterSavedTotalRowWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string companyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT COUNT(FAR.Id) TotalSavedRow
                    FROM [TRN].[FixedAssetRegister] FAR
                    LEFT JOIN MST.MaterialMaster MM ON FAR.MaterialMasterId=MM.Id
                    LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                    LEFT JOIN HKP.GLGeneralInfo GL ON BM.GLGeneralInfoId=GL.Id
                    WHERE MM.BudgetMasterId='" + budgetMasterId + @"' AND BM.GLGeneralInfoId='" + assetGLId + @"'
                    AND CompanyId='" + identity.CompanyId + "' AND IsOpeningBalance =1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }



        //public IEnumerable<object> GetOpeningBalanceInfoWithAssetItemId(string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        //{
        //    return _fixedAssetQueryService.GetOpeningBalanceInfoWithAssetItemId(assetGLId, assetBudgetId, assetActivityId, companyId);
        //}


        public IEnumerable<object> GetOpeningBalanceInfoWithBudgetMasterId(string assetBudgetId, string assetActivityId, string companyId, string accDepBudgetMasterId, string accDepActivityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                var sql = @"DECLARE @ADFAR DECIMAL(18,8) = (SELECT SUM(FAR.ADBaseAmount) ADBaseAmount
		FROM  TRN.FixedAssetRegister FAR 
		LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=FAR.FixedAssetMasterId
		WHERE FGL.AccumulatedDepreciationBudgetMasterId='" + accDepBudgetMasterId + "' AND FGL.AccumulatedDepreciationActivityId='" + accDepActivityId + @"' 
		  AND FAR.IsOpeningBalance=1)
        SELECT SUM(x.TOTALROW) TotalRow,SUM(x.FABaseAmountTotal)FABaseAmountTotal,SUM(X.ADBaseAmountTotal) ADBaseAmountTotal, ISNULL(@ADFAR,0) ADBaseRegsiterAmount
                        FROM (
					SELECT 0 TotalRow,CC.CompanyCurrencyAmount AS FABaseAmountTotal,FOBD.FixedAssetMasterId,FOBD.FAType,
					FOBD.Id , 0 ADBaseAmountTotal
						   FROM [TRN].[OpeningBalanceDetail] AS FOBD
						   JOIN TRN.OpeningBalance OB ON OB.Id=FOBD.OpeningBalanceId
                        INNER JOIN [TRN].[Voucher] AS FOB ON FOB.Id=OB.VoucherId
                        INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.GLGeneralInfoId
                        INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
				 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
				 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.DrAmount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
				 FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
				) AS CC ON CC.OpeningBalanceDetailId=FOBD.Id

				WHERE  FOBD.FAType='AssetCapatalized'
				AND
                 FOBD.BudgetMasterId='" + assetBudgetId + "' AND FOBD.ActivityId='" + assetActivityId + @"'
			    AND FOB.IsPark=0 
                UNION
					 SELECT  0 TotalRow,0 FABaseAmountTotal,null FixedAssetMasterId,null FAType, 
					 FOBD.Id,CC.CompanyCurrencyAmount AS ADBaseAmountTotal
				FROM [TRN].[OpeningBalanceDetail] AS FOBD
				JOIN TRN.OpeningBalance OB ON OB.Id=FOBD.OpeningBalanceId
				INNER JOIN [TRN].[Voucher] AS FOB ON FOB.Id=OB.VoucherId
				INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.GLGeneralInfoId
				INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
					 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
					 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.CrAmount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
					 FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
					) AS CC ON CC.OpeningBalanceDetailId=FOBD.Id
					WHERE FOBD.FAType='AccDept'
				AND 
                 FOBD.BudgetMasterId='" + accDepBudgetMasterId + @"' 
				AND FOB.IsPark=0 
                ) X";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT m.Id, M.Id As AssetNo, s.UserName FixedAsset, c.UserName FixedAssetCategory, sc.UserName FixedAssetSubcategory
                            , fac.UserName FixedAssetClass
                            , fasc.UserName FixedAssetSubClass
                            , p.UserName Vendor, m.SerialNo
                            , Replace(CONVERT(VARCHAR(11), m.InvoiceDate, 106), ' ', '-') InvoiceDate, m.Brand
	                        , m.InvoiceNo, m.Model, m.YearOfManufacture, m.YearOfInstallation, cn.UserName Country, m.IsForProduction
                            FROM TRN.[FixedAssetRegister] m
                            LEFT outer JOIN MST.[FixedAssetMaster] fam ON fam.Id = m.FixedAssetMasterId
                            LEFT outer JOIN HKP.[FixedAssetClass] fac ON fac.Id = fam.FixedAssetClassId
                            LEFT outer JOIN [HKP].[FixedAssetSubClass] fasc ON fasc.Id = fam.FixedAssetSubClassId
                            LEFT outer JOIN HKP.[FixedAsset] s ON s.Id = fam.FixedAssetId
                            LEFT outer JOIN HKP.[FixedAssetCategory] c ON c.Id = fam.FixedAssetCategoryId
                            LEFT outer JOIN HKP.[FixedAssetSubCategory] sc ON sc.Id = fam.FixedAssetSubCategoryId
                            LEFT outer JOIN [SCS].[Country] cn ON cn.Id = m.CountryOfOriginId
                            left outer join [HKP].[Party] p on p.Id=m.VendorId
                            WHERE m.CompanyId = '" + identity.CompanyId + @"' and IsOpeningBalance=1  and m.Archive=0
                            Order by c.UserName,sc.UserName, m.SerialNo ,m.InvoiceDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel Query(GridParameter parameters, string companyGroupId, string companyId, string[] fixedAssetRegisterIds)
        {
            try
            {
                var fixedAssetRegisterId = "";
                fixedAssetRegisterId = fixedAssetRegisterIds.Length > 0 ? string.Join(",", fixedAssetRegisterIds.Select(item => "'" + item + "'")) : "' '";
                parameters.CmdText = @"SELECT m.Id
	                                ,m.BrandId
	                                ,m.CountryOfOriginId
                                    ,s.UserName FixedAsset
                                    ,p.UserName Vendor
                                    ,m.SerialNo
	                                ,m.InvoiceNo
                                    , m.FixedAssetMasterId
	                                , m.IsForProduction
	                                , m.Model, m.VendorId, M.PlantId, M.CurrencyId, M.CompanyId
									, M.CompanyGroupId, m.YearOfManufacture, m.YearOfInstallation
	                                , c.UserName FixedAssetCategory, sc.UserName FixedAssetSubcategory
                                    , fac.UserName FixedAssetClass, fasc.UserName FixedAssetSubClass, cn.UserName Country, m.[Description]
                                FROM TRN.[FixedAssetRegister]  m
                                LEFT outer JOIN  MST.[FixedAssetMaster]  fam ON fam.Id = m.FixedAssetMasterId
                                LEFT outer JOIN  HKP.[FixedAsset]  s ON s.Id = fam.FixedAssetId
                                LEFT outer JOIN  HKP.[FixedAssetClass]  fac ON fac.Id = fam.FixedAssetClassId
                                LEFT outer JOIN  HKP.[FixedAssetSubClass] fasc ON fasc.Id = fam.FixedAssetSubClassId
                                LEFT outer JOIN  HKP.[FixedAssetCategory] c ON c.Id = fam.FixedAssetCategoryId
                                LEFT outer JOIN  HKP.[FixedAssetSubCategory]  sc ON sc.Id = fam.FixedAssetSubCategoryId
                                LEFT outer JOIN  SCS.[Country]   cn ON cn.Id = m.CountryOfOriginId
                                left outer join  HKP.[Party]  p on p.Id=m.VendorId
								LEFT OUTER JOIN  [ORG].[Plant] PL ON M.PlantId = PL.Id
								LEFT OUTER JOIN  [ORG].Company CO ON M.CompanyId = CO.Id
                                WHERE m.CompanyGroupId='" + companyGroupId + @"' and m.CompanyId = '" + companyId + @"' AND M.IsOpeningBalance=1  and m.Archive=0 ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public GridModel GetSearchData(GridParameter parameters, string[] ids)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FR.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.MaterialMasterId
                                    , FR.SerialNo, FR.AssetNo, FR.InvoiceNo, MM.UserName MaterialMasterName
                                    , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                                    , FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                                    , FAM.FixedAssetSubCategoryId, FAM.AssetType, FR.Price PurchasePrice,FR.Price TotalPrice
									, MMA.StandardName Article, FR.IsFinancial,FR.FABudgetMasterId,FR.FAActivityId,FR.ADBudgetMasterId,FR.ADActivityId
                                    FROM [TRN].[FixedAssetRegister] FR
					                LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId=MM.Id
					                LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                                    LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                                    LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMT ON BM.Id=FAMT.BudgetMasterId AND MM.BudgetMasterId=FAMT.BudgetMasterId
                                    LEFT JOIN [MST].[FixedAssetMaster] FAM ON FAMT.FixedAssetMasterId= FAM.Id
                                    LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                    LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                                    WHERE FR.CompanyId='" + identity.CompanyId + @"' and FR.IsOpeningBalance=1 and FR.Archive=0
                                    AND FR.Id NOT IN(" + ReturnStringArray(ids) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetOBFARegisterData(GridParameter parameters, string[] ids)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FR.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.MaterialMasterId
                                    , FR.SerialNo, FR.AssetNo, FR.InvoiceNo, MM.UserName MaterialMasterName
                                    , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                                    , FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                                    , FAM.FixedAssetSubCategoryId, FAM.AssetType, FR.Price PurchasePrice,FR.Price TotalPrice
									, MMA.StandardName Article, FR.IsFinancial,FR.FABudgetMasterId,FR.FAActivityId,FR.ADBudgetMasterId,FR.ADActivityId
									, GL.UserName GLName,B.UserName BudgetName, A.UserName ActivityName,FR.LCNumber
                                    FROM [TRN].[FixedAssetRegister] FR
					                LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId=MM.Id
					                LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                                    LEFT JOIN MST.BudgetMaster BM ON FR.FABudgetMasterId = BM.Id
                                    LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMT ON BM.Id=FAMT.BudgetMasterId AND FR.FABudgetMasterId=FAMT.BudgetMasterId
                                    LEFT JOIN [MST].[FixedAssetMaster] FAM ON FAMT.FixedAssetMasterId= FAM.Id
                                    LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                    LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
									LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=BM.GLGeneralInfoId
									LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                                    LEFT JOIN HKP.Activity A ON A.Id=FR.FAActivityId
                                    WHERE FR.CompanyId='" + identity.CompanyId + @"' and FR.IsOpeningBalance=1 and FR.Archive=0
                                    AND FR.Id NOT IN(" + ReturnStringArray(ids) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetRegisterByMaterialMaster(GridParameter parameters, string companyId, string materialMasterId)
        {
            try
            {
                parameters.CmdText = @"SELECT FR.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.SerialNo, FR.Id AssetNo, FAM.UserName FixedAssetMasterName, FAM.AssetType, MMA.StandardName AS Article
                                    , MT.[Description] AS MaterialTypeName, MGP.UserName AS MaterialGroupMasterName, FR.MaterialMasterId, MM.Code AS MaterialMasterCode, MM.UserName AS MaterialMasterName, FR.[Description]
                                    FROM [TRN].[FixedAssetRegister] FR
                                    LEFT JOIN [MST].[MaterialMaster] MM ON FR.MaterialMasterId=MM.Id
                                    LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MGP.Id=MM.MaterialGroupMasterId
                                    LEFT JOIN [HKP].[MaterialType] AS MT ON MT.Id=MGP.MaterialTypeId
                                    LEFT JOIN [MST].MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                                    LEFT JOIN [MST].BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                                    LEFT JOIN [HKP].FixedAssetMasterBudgetTag FAMT ON BM.Id=FAMT.BudgetMasterId AND MM.BudgetMasterId=FAMT.BudgetMasterId
                                    LEFT JOIN [MST].[FixedAssetMaster] FAM ON FAMT.FixedAssetMasterId= FAM.Id
                                    LEFT JOIN [HKP].FixedAssetCategory FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                    LEFT JOIN [HKP].FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                                    WHERE FR.CompanyId = '" + companyId + "' and FR.IsOpeningBalance=1 and FR.Archive=0 AND FR.MaterialMasterId='" + materialMasterId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetListForBudgetMaster(GridParameter parameters, string companyId, string[] ids)
        {
            try
            {
                parameters.CmdText = @"SELECT FR.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.MaterialMasterId, FR.SerialNo, FR.Id AssetNo,FR.InvoiceNo,
                            MM.UserName MaterialMaster, FAC.UserName AssetCategory, FASC.UserName AssetSubCategory, FAM.FixedAssetCategoryId, FAM.FixedAssetSubCategoryId,
                            FAM.AssetType, FAM.UserName AS AssetMaster, MMA.StandardName Article
                            FROM [TRN].[FixedAssetRegister] FR
							LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId=MM.Id
							LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                            LEFT JOIN [MST].[FixedAssetMaster] FAM ON FR.FixedAssetMasterId= FAM.Id
							LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId=FAC.Id
							LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                            WHERE FR.CompanyId = '" + companyId + @"' and FR.IsOpeningBalance=1 and FR.Archive=0
                            AND FR.Id NOT IN(" + ReturnStringArray(ids) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public GridModel GetSearch(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters.CmdText = @"
                                SELECT
                                    ,s.UserName FixedAsset
                                    ,c.UserName FixedAssetCategory
	                                ,sc.UserName FixedAssetSubcategory
									,m.Brand
									,m.SerialNo
                                    ,p.UserName Vendor,0 IsSelectedID,m.Id FROM TRN.[FixedAssetRegister] m
                                LEFT outer JOIN HKP.[FixedAsset] s ON s.Id = m.FixedAssetId
                                LEFT outer JOIN HKP.[FixedAssetCategory] c ON c.Id = m.FixedAssetCategoryId
                                LEFT outer JOIN HKP.[FixedAssetSubCategory] sc ON sc.Id = m.FixedAssetSubCategoryId
                                left outer join [HKP].[Party] p on p.Id=m.VendorId
                                WHERE m.CompanyId = '" + identity.CompanyId + @"' AND M.IsOpeningBalance=1  and  m.Archive=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> QueryForAttribute(string fixedAssetRegisterId, string assetItemId)
        {
            try
            {
                var _sql = @"SELECT MAM.FixedAssetAttributeId AS FixedAssetAttributeId
                            , MA.UserName AS FixedAssetAttributeName
                            , MAM.IsFreeField
                            , MAM.IsPreDefinedField
                            , MAM.IsMandatory
                            , MMAV.Id
                            , MMAV.FixedAssetRegisterId
                            , FixedAssetAttributeValueId = CASE WHEN (ISNULL(MMAV.Id, '') = '' AND MAV.IsDefault = 1)
							    THEN MAV.Id ELSE MMAV.FixedAssetAttributeValueId END
                            , ValueFreeText =CASE WHEN (ISNULL(MMAV.Id, '') = '' AND MAV.IsDefault = 1)
									    THEN MAV.[UserName] ELSE (ISNULL(MMAV.ValueFreeText, '')
									    + ISNULL(MMAV.[UserName], '')) END
                            ,'True' AS FlagDisable
                            FROM (SELECT * FROM MST.AssetItemAttribute WHERE AssetItemId = '" + assetItemId + @"') AS MAM
                            LEFT JOIN HKP.FixedAssetAttribute AS MA ON MAM.FixedAssetAttributeId = MA.Id
                            LEFT OUTER JOIN (SELECT a.*,b.UserName FROM [TRN].[AssetItemValue] a left outer join hkp.FixedAssetAttributeValue b on a.FixedAssetAttributeValueId=b.Id WHERE a.Archive = 0
                            AND a.FixedAssetRegisterId = '" + fixedAssetRegisterId + @"') AS MMAV ON MMAV.FixedAssetAttributeId = MA.Id
                            LEFT JOIN (SELECT * FROM HKP.FixedAssetAttributeValue WHERE Active = 1
                            AND IsDefault = 1) AS MAV ON MAM.FixedAssetAttributeId = MAV.FixedAssetAttributeId";
                return _sqlRepository.GetDataCollection(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                return from m in base.Query(r => !r.Archive)
                       select new { Text = m.Model + " " + m.SerialNo, Value = m.Id };
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        #region Insert n Update

        private static void OutItem(string pk, FixedAssetRegister from_ui, ref FixedAssetRegister from_db)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (from_db == null || from_db.Id == null || from_db.Id == "")
                {
                    from_db = new FixedAssetRegister
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    #region Add
                    from_db.Id = pk;//set pk

                    from_db.BrandId = from_ui.BrandId;
                    from_db.CompanyId = identity.CompanyId;
                    from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.MaterialMasterId = from_ui.MaterialMasterId;
                    from_db.CountryOfOriginId = from_ui.CountryOfOriginId;
                    from_db.InvoiceNo = from_ui.InvoiceNo;
                    from_db.InvoiceDate = from_ui.InvoiceDate;
                    from_db.IsForProduction = from_ui.IsForProduction;
                    from_db.Model = from_ui.Model;
                    from_db.Price = from_ui.Price;
                    from_db.CurrencyId = from_ui.CurrencyId;
                    from_db.SerialNo = from_ui.SerialNo;
                    from_db.VendorId = from_ui.VendorId;
                    from_db.PlantId = from_ui.PlantId;
                    from_db.YearOfInstallation = from_ui.YearOfInstallation;
                    from_db.YearOfManufacture = from_ui.YearOfManufacture;
                    from_db.LifeTime = from_ui.LifeTime;
                    from_db.CapitalizationDate = from_ui.CapitalizationDate;
                    from_db.FABaseAmount = from_ui.FABaseAmount;
                    from_db.FAGroupAmount = from_ui.FAGroupAmount;
                    from_db.FAHardAmount = from_ui.FAHardAmount;
                    from_db.ADBaseAmount = from_ui.ADBaseAmount;
                    from_db.ADGroupAmount = from_ui.ADGroupAmount;
                    from_db.ADHardAmount = from_ui.ADHardAmount;
                    from_db.FABaseCurrencyId = from_ui.FABaseCurrencyId;
                    from_db.FAGroupCurrencyId = from_ui.FAGroupCurrencyId;
                    from_db.FAHardCurrencyId = from_ui.FAHardCurrencyId;
                    from_db.ADBaseCurrencyId = from_ui.ADBaseCurrencyId;
                    from_db.ADGroupCurrencyId = from_ui.ADGroupCurrencyId;
                    from_db.ADHardCurrencyId = from_ui.ADHardCurrencyId;
                    from_db.MaterialMasterArticleId = from_ui.MaterialMasterArticleId;
                    from_db.RFId = from_ui.RFId;
                    from_db.IsFinancial = from_ui.IsFinancial;
                    from_db.Year = from_ui.Year;
                    from_db.AssetNo = from_ui.AssetNo;
                    if (!String.IsNullOrEmpty(from_ui.RFId))
                    {
                        from_db.RFIdAddedBy = identity.UserId;
                        from_db.RFIdAddedDate = DateTime.Now;
                    }
                    else
                    {
                        from_db.RFIdAddedBy = null;
                        from_db.RFIdAddedDate = null;
                    }
                    #endregion Add
                }
                else
                {
                    #region Edit
                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);

                    from_db.BrandId = from_ui.BrandId;
                    from_db.CompanyId = identity.CompanyId;
                    from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.MaterialMasterId = from_ui.MaterialMasterId;
                    from_db.CountryOfOriginId = from_ui.CountryOfOriginId;
                    from_db.MaterialMasterId = from_ui.MaterialMasterId;
                    from_db.InvoiceNo = from_ui.InvoiceNo;
                    from_db.InvoiceDate = from_ui.InvoiceDate;
                    from_db.IsForProduction = from_ui.IsForProduction;
                    from_db.Model = from_ui.Model;
                    from_db.Price = from_ui.Price;
                    from_db.Model = from_ui.Model;
                    from_db.SerialNo = from_ui.SerialNo;
                    from_db.VendorId = from_ui.VendorId;
                    from_db.YearOfInstallation = from_ui.YearOfInstallation;
                    from_db.YearOfManufacture = from_ui.YearOfManufacture;
                    from_db.PlantId = from_ui.PlantId;
                    from_db.YearOfInstallation = from_ui.YearOfInstallation;
                    from_db.YearOfManufacture = from_ui.YearOfManufacture;
                    from_db.LifeTime = from_ui.LifeTime;
                    from_db.CapitalizationDate = from_ui.CapitalizationDate;
                    from_db.FABaseAmount = from_ui.FABaseAmount;
                    from_db.FAGroupAmount = from_ui.FAGroupAmount;
                    from_db.FAHardAmount = from_ui.FAHardAmount;
                    from_db.ADBaseAmount = from_ui.ADBaseAmount;
                    from_db.ADGroupAmount = from_ui.ADGroupAmount;
                    from_db.ADHardAmount = from_ui.ADHardAmount;
                    from_db.FABaseCurrencyId = from_ui.FABaseCurrencyId;
                    from_db.FAGroupCurrencyId = from_ui.FAGroupCurrencyId;
                    from_db.FAHardCurrencyId = from_ui.FAHardCurrencyId;
                    from_db.ADBaseCurrencyId = from_ui.ADBaseCurrencyId;
                    from_db.ADGroupCurrencyId = from_ui.ADGroupCurrencyId;
                    from_db.ADHardCurrencyId = from_ui.ADHardCurrencyId;
                    from_db.MaterialMasterArticleId = from_ui.MaterialMasterArticleId;
                    from_db.RFId = from_ui.RFId;
                    from_db.IsFinancial = from_ui.IsFinancial;
                    from_db.Description = from_ui.Description;
                    from_db.Year = from_ui.Year;
                    from_db.AssetNo = from_ui.AssetNo;
                    if (from_ui.RFId != null || from_ui.RFId != "")
                    {
                        from_db.RFIdUpdatedDate = DateTime.Now;
                        from_db.RFIdUpdatedBy = identity.UserId;
                    }
                    else
                    {
                        from_db.RFIdUpdatedDate = null;
                        from_db.RFIdUpdatedBy = null;
                    }
                    #endregion Edit
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void InitInsert(string pk, FixedAssetRegister from_ui, out FixedAssetRegister from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (from_ui == null || from_ui.Id == null || from_ui.Id == "")
                {
                    from_db = new FixedAssetRegister
                    {
                        ModelState = ModelState.Added
                    };
                    AuditService.Log(from_db);

                    #region Add
                    from_db.Id = pk;//set pk

                    from_db.BrandId = from_ui.BrandId;
                    from_db.FixedAssetMasterId = from_ui.FixedAssetMasterId;
                    from_db.CompanyId = identity.CompanyId;
                    from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.CountryOfOriginId = from_ui.CountryOfOriginId;
                    from_db.MaterialMasterId = from_ui.MaterialMasterId;
                    from_db.InvoiceNo = from_ui.InvoiceNo;
                    from_db.InvoiceDate = from_ui.InvoiceDate;
                    from_db.IsForProduction = from_ui.IsForProduction;
                    from_db.Model = from_ui.Model;
                    from_db.Price = from_ui.Price;
                    from_db.CurrencyId = from_ui.CurrencyId;
                    from_db.SerialNo = from_ui.SerialNo;
                    from_db.VendorId = from_ui.VendorId;
                    from_db.PlantId = from_ui.PlantId;
                    from_db.YearOfInstallation = from_ui.YearOfInstallation;
                    from_db.YearOfManufacture = from_ui.YearOfManufacture;
                    from_db.LifeTime = from_ui.LifeTime;
                    from_db.CapitalizationDate = from_ui.CapitalizationDate;
                    from_db.FABaseAmount = from_ui.FABaseAmount;
                    from_db.FAGroupAmount = from_ui.FAGroupAmount;
                    from_db.FAHardAmount = from_ui.FAHardAmount;
                    from_db.ADBaseAmount = from_ui.ADBaseAmount;
                    from_db.ADGroupAmount = from_ui.ADGroupAmount;
                    from_db.ADHardAmount = from_ui.ADHardAmount;
                    from_db.FABaseCurrencyId = from_ui.FABaseCurrencyId;
                    from_db.FAGroupCurrencyId = from_ui.FAGroupCurrencyId;
                    from_db.FAHardCurrencyId = from_ui.FAHardCurrencyId;
                    from_db.ADBaseCurrencyId = from_ui.ADBaseCurrencyId;
                    from_db.ADGroupCurrencyId = from_ui.ADGroupCurrencyId;
                    from_db.ADHardCurrencyId = from_ui.ADHardCurrencyId;
                    from_db.IsOpeningBalance = from_ui.IsOpeningBalance;
                    from_db.FABudgetMasterId = from_ui.FABudgetMasterId;
                    from_db.FAActivityId = from_ui.FAActivityId;
                    from_db.ADBudgetMasterId = from_ui.ADBudgetMasterId;
                    from_db.ADActivityId = from_ui.ADActivityId;
                    from_db.IsAUC = from_ui.IsAUC;
                    from_db.MaterialMasterArticleId = from_ui.MaterialMasterArticleId;
                    from_db.RFId = from_ui.RFId;
                    from_db.Description = from_ui.Description;
                    from_db.IsFinancial = from_ui.IsFinancial;
                    from_db.CapitalizeRegisterNo = from_ui.CapitalizeRegisterNo;
                    from_db.Year = from_ui.Year;
                    from_db.AssetNo = from_ui.AssetNo;
                    from_db.LCNumber = from_ui.LCNumber;
                    from_db.Remarks = from_ui.Remarks;
                    from_db.Quantity = from_ui.Quantity;
                    from_db.DepreciationRuleId = from_ui.DepreciationRuleId;
                    from_db.MultiplicationFactor = from_ui.MultiplicationFactor;
                    if (!String.IsNullOrEmpty(from_ui.RFId))
                    {
                        from_db.RFIdAddedBy = identity.UserId;
                        from_db.RFIdAddedDate = DateTime.Now;
                    }
                    else
                    {
                        from_db.RFIdAddedBy = null;
                        from_db.RFIdAddedDate = null;
                    }

                    #endregion Add
                }
                else
                {
                    #region Edit
                    from_db = new FixedAssetRegister
                    {
                        ModelState = ModelState.Added
                    };

                    from_db.ModelState = ModelState.Modified;
                    AuditService.Log(from_db);

                    from_db.BrandId = from_ui.BrandId;
                    from_db.FixedAssetMasterId = from_ui.FixedAssetMasterId;
                    from_db.CompanyId = identity.CompanyId;
                    from_db.CompanyGroupId = identity.CompanyGroupId;
                    from_db.CountryOfOriginId = from_ui.CountryOfOriginId;
                    from_db.MaterialMasterId = from_ui.MaterialMasterId;
                    from_db.InvoiceNo = from_ui.InvoiceNo;
                    from_db.InvoiceDate = from_ui.InvoiceDate;
                    from_db.IsForProduction = from_ui.IsForProduction;
                    from_db.Model = from_ui.Model;
                    from_db.Price = from_ui.Price;
                    from_db.Model = from_ui.Model;
                    from_db.Id = from_ui.Id;
                    from_db.CurrencyId = from_ui.CurrencyId;
                    from_db.SerialNo = from_ui.SerialNo;
                    from_db.VendorId = from_ui.VendorId;
                    from_db.YearOfInstallation = from_ui.YearOfInstallation;
                    from_db.YearOfManufacture = from_ui.YearOfManufacture;
                    from_db.PlantId = from_ui.PlantId;
                    from_db.YearOfInstallation = from_ui.YearOfInstallation;
                    from_db.YearOfManufacture = from_ui.YearOfManufacture;
                    from_db.LifeTime = from_ui.LifeTime;
                    from_db.CapitalizationDate = from_ui.CapitalizationDate;
                    from_db.FABaseAmount = from_ui.FABaseAmount;
                    from_db.FAGroupAmount = from_ui.FAGroupAmount;
                    from_db.FAHardAmount = from_ui.FAHardAmount;
                    from_db.ADBaseAmount = from_ui.ADBaseAmount;
                    from_db.ADGroupAmount = from_ui.ADGroupAmount;
                    from_db.ADHardAmount = from_ui.ADHardAmount;
                    from_db.FABaseCurrencyId = from_ui.FABaseCurrencyId;
                    from_db.FAGroupCurrencyId = from_ui.FAGroupCurrencyId;
                    from_db.FAHardCurrencyId = from_ui.FAHardCurrencyId;
                    from_db.ADBaseCurrencyId = from_ui.ADBaseCurrencyId;
                    from_db.ADGroupCurrencyId = from_ui.ADGroupCurrencyId;
                    from_db.ADHardCurrencyId = from_ui.ADHardCurrencyId;
                    from_db.IsOpeningBalance = from_ui.IsOpeningBalance;
                    from_db.FABudgetMasterId = from_ui.FABudgetMasterId;
                    from_db.FAActivityId = from_ui.FAActivityId;
                    from_db.ADBudgetMasterId = from_ui.ADBudgetMasterId;
                    from_db.ADActivityId = from_ui.ADActivityId;
                    from_db.IsAUC = from_ui.IsAUC;
                    from_db.RFIdAddedBy = from_ui.RFIdAddedBy;
                    from_db.RFIdAddedDate = from_ui.RFIdAddedDate;
                    from_db.MaterialMasterArticleId = from_ui.MaterialMasterArticleId;
                    from_db.RFId = from_ui.RFId;
                    from_db.Description = from_ui.Description;
                    from_db.IsFinancial = from_ui.IsFinancial;
                    from_db.CapitalizeRegisterNo = from_ui.CapitalizeRegisterNo;
                    from_db.Year = from_ui.Year;
                    from_db.AssetNo = from_ui.AssetNo;
                    from_db.LCNumber = from_ui.LCNumber;
                    from_db.Remarks = from_ui.Remarks;
                    from_db.Quantity = from_ui.Quantity;
                    from_db.DepreciationRuleId = from_ui.DepreciationRuleId;
                    from_db.MultiplicationFactor = from_ui.MultiplicationFactor;
                    if (from_ui.RFId != null || from_ui.RFId != "")
                    {
                        from_db.RFIdUpdatedDate = DateTime.Now;
                        from_db.RFIdUpdatedBy = identity.UserId;
                    }
                    else
                    {
                        from_db.RFIdUpdatedDate = null;
                        from_db.RFIdUpdatedBy = null;
                    }
                    #endregion Edit
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertORUpdateItem(FixedAssetRegister master, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode
            , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
            , string assetGLId, string assetBudgetId, string assetActivityId)
        {
            FixedAssetRegister localItem = null;
            masterid = string.Empty;
            #region-- Assign Amount
            decimal reFABaseAmountTotal = 0;
            decimal reADBaseAmountTotal = 0;
            decimal reTotalRow = 0;
            //-----
            decimal opFABaseAmountTotal = 0;
            decimal opADBaseAmountTotal = 0;
            decimal opTotalRow = 0;
            decimal subassestAmount = 0;
            decimal tempTotalAmount = 0;
            decimal tempFAAmount = 0;
            decimal tempADAmount = 0;
            tempFAAmount = master.FABaseAmount;
            tempADAmount = master.ADBaseAmount;
            var fixedAssetMasterGL = _sqlRepository.GetModelCollection<FixedAssetMasterGL>(@"SELECT FGL.AccumulatedDepreciationBudgetMasterId,FGL.AccumulatedDepreciationActivityId,FGL.FixedAssetMasterId  
		                                FROM HKP.FixedAssetMasterBudgetTag TAG 
		                                LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=TAG.FixedAssetMasterId 
		                                LEFT JOIN HKP.FixedAssetMasterGL FGL ON FGL.FixedAssetMasterId=FAM.Id WHERE TAG.BudgetMasterId='" + assetBudgetId + "'").FirstOrDefault();


            var savedOPInfo = GetOpeningBalanceInfoWithBudgetMasterId(assetBudgetId, assetActivityId, master.CompanyId
                , fixedAssetMasterGL.AccumulatedDepreciationBudgetMasterId, fixedAssetMasterGL.AccumulatedDepreciationActivityId);
            var savedRegInfo = GetRegisterInfoWithFAMId(master.MaterialMasterId, assetBudgetId, assetGLId, master.CompanyId);
            foreach (var item in savedRegInfo)
            {
                var dic = (Dictionary<string, object>)item;
                reFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                reADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                reTotalRow = Convert.ToDecimal(dic["TotalRow"].ToString());
            }
            foreach (var item in savedOPInfo)
            {
                var dic = (Dictionary<string, object>)item;
                opFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                opADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                reADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseRegsiterAmount"].ToString()));
                opTotalRow = Convert.ToDecimal(CheckNumIsExist(dic["TotalRow"].ToString()));
            }
            #endregion Insert n Update
            var _pk = GetPK();
            var flag = false;
            var builder = new System.Text.StringBuilder();
            var value = "''";
            try
            {
                if (opTotalRow > 0 && reTotalRow + NumberOfQuantity > opTotalRow)
                {
                    throw new CustomException("Register quantity  must be below opening balance quantity");
                }
                if (NumberOfQuantity > 0)
                {
                    #region--Save Validation

                    master.ADBudgetMasterId = fixedAssetMasterGL.AccumulatedDepreciationBudgetMasterId;
                    master.ADActivityId = fixedAssetMasterGL.AccumulatedDepreciationActivityId;
                    master.FABudgetMasterId = assetBudgetId;
                    master.FAActivityId = assetActivityId;
                    master.Year = DateTime.Now.Year.ToString();
                    var maxAssetNo = _fixedAssetRegisterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(AssetNo, 2) AS INT)), 0) AssetNo FROM [TRN].[FixedAssetRegister] WHERE PlantId='{master.PlantId}' AND [Year]='{master.Year}'").First();

                    if (master.IsFinancial)
                    {
                        var materialMaster = _materialMasterRepository.Find(master.MaterialMasterId);
                        if (null == materialMaster)
                            throw new CustomException("Material Master is null.");
                        //var faTagData = _fixedAssetMasterBudgetTagRepository.Query(r => r.BudgetMasterId == materialMaster.BudgetMasterId).Select().FirstOrDefault();
                        var faTagData = _fixedAssetMasterBudgetTagRepository.Query().Select().FirstOrDefault();
                        if (null == faTagData)
                            throw new CustomException("Fixed Asset Master Tag data not found.");

                        var checkACUD = _fixedAssetMasterGLepository.Query(r => r.FixedAssetMasterId == faTagData.FixedAssetMasterId).Select().FirstOrDefault();
                        if (checkACUD == null)
                            throw new CustomException("This item is not configured with Accumulative Depreciation GL");
                        else
                        {
                            if (checkACUD.AccumulatedDepreciationGLId == null)
                            {
                                throw new CustomException(checkACUD.FixedAssetMaster.UserName + " is not configured with Accumulative Depreciation GL");
                            }
                        }
                        //TODO: totalPrice + reFABaseAmountTotal > opFABaseAmountTotal
                        //if ((master.FABaseAmount * NumberOfQuantity + reFABaseAmountTotal) > opFABaseAmountTotal)
                        if (master.Id != null)
                        {
                            decimal exsistregisterAmount = _fixedAssetRegisterRepository.SqlQuery<decimal>($"SELECT ISNULL(FABaseAmount, 0) FABaseAmount FROM [TRN].[FixedAssetRegister] WHERE Id='{master.Id}'").First();

                            if ((master.FABaseAmount + reFABaseAmountTotal - exsistregisterAmount) > opFABaseAmountTotal)
                            {
                                throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal.ToString("0.##"));
                            }
                        }
                        else
                        {
                            if ((master.FABaseAmount + reFABaseAmountTotal) > opFABaseAmountTotal)
                            {
                                throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal.ToString("0.##"));
                            }
                        }

                        //TODO: totalADPrice + reADBaseAmountTotal > opADBaseAmountTotal
                        // if ((master.ADBaseAmount * NumberOfQuantity + reADBaseAmountTotal) > opADBaseAmountTotal)
                        if ((master.ADBaseAmount + reADBaseAmountTotal) > opADBaseAmountTotal)
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADBaseAmountTotal.ToString("0.##"));
                        }
                        if (subFixedAssetRegister != null)
                        {
                            subassestAmount = Math.Round(subFixedAssetRegister.Sum(r => r.Amount) / NumberOfQuantity, 2);
                            tempFAAmount -= subFixedAssetRegister.Sum(r => r.Amount);
                        }
                        //master.FABaseAmount = Math.Round(master.FABaseAmount / NumberOfQuantity, 2);
                        master.ADBaseAmount = Math.Round(master.ADBaseAmount / NumberOfQuantity, 2);
                        master.FABaseAmount = master.Price;
                        if (master.Id != null)
                        {
                            tempFAAmount = master.Price;
                        }


                    }
                    // builder.Append(value);
                    #endregion
                    for (int i = 0; i < NumberOfQuantity; i++)
                    {
                        maxAssetNo++;
                        master.AssetNo = master.Year.ToString().Substring(2, 2) + maxAssetNo;
                        var lc = localItem;
                        var asItem = assetItemValue;
                        var id = MakePK(_pk, i, 2);
                        if (i + 1 == NumberOfQuantity)
                        {
                            master.FABaseAmount = tempFAAmount - tempTotalAmount;
                            master.Price = master.FABaseAmount;
                            master.Quantity = 1;
                        }
                        else
                        {
                            tempTotalAmount += master.Price;
                            master.Quantity = 1;
                        }
                        InitInsert(id, master, out lc);
                        InsertOrUpdateGraph(lc);
                        var subAssetTempId = _subFixedAssetRegisterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SubFixedAssetRegister] WHERE FixedAssetRegisterId='{lc.Id}'").First();

                        /*Sub Fixed Asset Resigter If have */
                        if (subFixedAssetRegister != null)
                        {
                            foreach (var item in subFixedAssetRegister)
                            {
                                if (string.IsNullOrEmpty(item.Id))
                                {
                                    subAssetTempId++;
                                    var subFAR = new SubFixedAssetRegister
                                    {
                                        Id = lc.Id + subAssetTempId,
                                        Amount = Math.Round(item.Amount / NumberOfQuantity, 2),
                                        FixedAssetRegisterId = lc.Id,
                                        SubAssetTypeId = item.SubAssetTypeId,
                                        CurrencyId = item.CurrencyId,
                                        CapitalizationDate = item.CapitalizationDate,
                                        CapitalizationRate = Math.Round(lc.FABaseAmount / lc.Price, 4)
                                    };
                                    AuditService.AddedLog(subFAR);
                                    _subFixedAssetRegisterRepository.Insert(subFAR);
                                }
                                else
                                {
                                    var subFAR = new SubFixedAssetRegister
                                    {
                                        Amount = Math.Round(item.Amount / NumberOfQuantity, 2),
                                        Id = item.Id,
                                        FixedAssetRegisterId = lc.Id,
                                        SubAssetTypeId = item.SubAssetTypeId,
                                        CapitalizationDate = item.CapitalizationDate,
                                        CurrencyId = item.CurrencyId,
                                        CapitalizationRate = Math.Round(lc.FABaseAmount / lc.Price, 4)
                                    };
                                    AuditService.UpdatedLog(subFAR);
                                    _subFixedAssetRegisterRepository.Update(subFAR);
                                }
                            }
                        }


                        //_assetItemValueService.InsertOrUpdateGraph(asItem, lc.Id);
                        _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(lc, fixedAssetRegisterSkuValue);
                        if (value == "''")
                        {
                            value = "'" + id + "'";
                            builder.Append(value);
                        }
                        else
                        { builder.Append(",'" + id + "'"); }
                    }
                }
                else
                {
                    localItem = GetItem(master.Id);
                    #region-- Update Validation
                    if (master.IsFinancial)
                    {
                        if (!((reFABaseAmountTotal - (localItem.FABaseAmount) + master.FABaseAmount) <= opFABaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        }

                        if (!((reADBaseAmountTotal - localItem.ADBaseAmount + master.ADBaseAmount) <= opADBaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADBaseAmountTotal);
                        }
                    }
                    #endregion
                    OutItem(_pk, master, ref localItem);
                    InsertOrUpdateGraph(localItem);
                    _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(master, fixedAssetRegisterSkuValue);
                    value = "'" + master.Id + "'";
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                masterid = builder.ToString();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, master.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion

        public static string CheckNumIsExist(string strNumber)
        {
            strNumber = strNumber.Replace(",", "");
            var n = new System.Globalization.NumberFormatInfo();
            return strNumber.Trim() == "" ? "0" : Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out double d) ? strNumber : "0";
        }

        public void DeleteItem(string masterId)
        {
            FixedAssetRegister from_db = null;
            var flag = false;
            try
            {
                DelItem(masterId, out from_db);
                var _sql = @"select * from TRN.FixedAssetRegisterCharacteristicsValue WHERE FixedAssetRegisterId='" + masterId + "'";
                var _subAssetsql = @"select * from TRN.SubFixedAssetRegister WHERE FixedAssetRegisterId='" + masterId + "'";
                var child = _sqlRepository.GetModelCollection<FixedAssetRegisterCharacteristicsValue>(_sql, null);
                var subAssetchild = _sqlRepository.GetModelCollection<SubFixedAssetRegister>(_subAssetsql, null);
                if (child.Count > 0)
                {
                    _faRegisterSquRepository.ExecuteSqlCommand("DELETE FROM trn.FixedAssetRegisterCharacteristicsValue WHERE FixedAssetRegisterId='" + masterId + "'");
                }
                if (subAssetchild.Count > 0)
                {
                    _subFixedAssetRegisterRepository.ExecuteSqlCommand("DELETE FROM trn.SubFixedAssetRegister WHERE FixedAssetRegisterId='" + masterId + "'");
                }
                Delete(from_db);
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void DelItem(string id, out FixedAssetRegister from_db)
        {
            from_db = null;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                from_db = GetItem(id);

                if (from_db.Id == null || from_db.Id == "")
                {
                    throw new Exception("No Asset found against Id: [" + id + "]");
                }
                else
                {
                    from_db.ModelState = ModelState.Deleted;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetPriceAndCurrencyById(string id)
        {
            try
            {
                var sql = $"SELECT FAI.Price,FAI.CurrencyId,CU.Code AS Currency ,FAD.MaterialMasterGLId,FAD.AccDepreciationGLId " +
                        "FROM [TRN].FixedAssetRegister AS FAI Left OUTER  join SCS.Currency AS CU ON CU.Id=FAI.CurrencyId " +
                        "LEFT OUTER JOIN MST.FixedAssetGL AS FAD ON FAD.FixedAssetItemId=FAI.Id " +
                         $"WHERE FAI.Id='{id}'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public IEnumerable<object> GetSavedListById(string assetRegisterIdList)
        {
            try
            {
                //var sql = @"SELECT FAR.Id,FAR.InvoiceNo,FAR.FixedAssetMasterId,FAM.UserName AS FixedAssetMasterName FROM [TRN].[FixedAssetRegister] FAR
                //            LEFT OUTER JOIN [MST].[FixedAssetMaster] FAM ON FAR.FixedAssetMasterId = FAM.Id
                //            WHERE FAR.Id IN (" + AssetRegisterIdList + ") ";

                var sql = @"SELECT FAR.Id,FAR.InvoiceNo, FAR.SerialNo, FAR.AssetNo FROM [TRN].[FixedAssetRegister] FAR
                            WHERE FAR.Id IN (" + assetRegisterIdList + ") ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
        }

        public IEnumerable<object> GetSkuWithRegister(string materialMasterId, string registerId)
        {
            try
            {
                var sql = @"SELECT S.Id, S.AssetItemId, S.FixedAssetRegisterId, D1.Id AssetItemCharacteristicsId, D1.UserName Sku, S.SkuValue FROM MST.AssetItem A
                            LEFT OUTER JOIN HKP.AssetItemCharacteristics D1 ON A.Dimension1=D1.Id
                            LEFT OUTER JOIN (SELECT Id, SkuValue, AssetItemId, FixedAssetRegisterId, AssetItemCharacteristicsId FROM HKP.FixedAssetRegisterSkuValue WHERE FixedAssetRegisterId ='" + registerId + @"') S ON D1.Id= S.AssetItemCharacteristicsId
                            WHERE A.Id='" + materialMasterId + @"'
                            UNION
                            SELECT S.Id, S.AssetItemId, S.FixedAssetRegisterId, D2.Id AssetItemCharacteristicsId, D2.UserName Sku, S.SkuValue  FROM MST.AssetItem A
                            LEFT OUTER JOIN HKP.AssetItemCharacteristics D2 ON A.Dimension2=D2.Id
                            LEFT OUTER JOIN (SELECT Id, SkuValue,AssetItemId, FixedAssetRegisterId, AssetItemCharacteristicsId FROM HKP.FixedAssetRegisterSkuValue WHERE FixedAssetRegisterId ='" + registerId + @"') S ON D2.Id= S.AssetItemCharacteristicsId
                            WHERE A.Id='" + materialMasterId + @"'
                            UNION
                            SELECT S.Id, S.AssetItemId, S.FixedAssetRegisterId, D3.Id AssetItemCharacteristicsId, D3.UserName Sku, S.SkuValue  FROM MST.AssetItem A
                            LEFT OUTER JOIN HKP.AssetItemCharacteristics D3 ON A.Dimension3=D3.Id
                            LEFT OUTER JOIN (SELECT Id, SkuValue, AssetItemId, FixedAssetRegisterId, AssetItemCharacteristicsId FROM HKP.FixedAssetRegisterSkuValue WHERE FixedAssetRegisterId ='" + registerId + @"') S ON D3.Id= S.AssetItemCharacteristicsId
                            WHERE A.Id='" + materialMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> CheckMasterIsRegisterApplyByMaterialMasterId(string assetMasterId)
        {
            try
            {
                var sql = @"SELECT OB.IsPark From TRN.MaterialMasterOpeningBalanceDetail AS OBD
							LEFT JOIN TRN.OpeningBalance OB ON OBD.OpeningBalanceId=OB.Id
                            WHERE OBD.FixedAssetMasterId='" + assetMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region fixed Asset Register JV OB
        public void InsertORUpdateItemJVOB(FixedAssetRegister master, int NumberOfQuantity, string CompanyCurrencyCode
           , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
           , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId)
        {
            FixedAssetRegister localItem = null;
            masterid = string.Empty;
            #region-- Assign Amount
            decimal reFABaseAmountTotal = 0;
            decimal reFAGroupAmountTotal = 0;
            decimal reFAHardAmountTotal = 0;
            decimal reADBaseAmountTotal = 0;
            decimal reADGroupAmountTotal = 0;
            decimal reADHardAmountTotal = 0;
            decimal reTotalRow = 0;
            //-----
            decimal opFABaseAmountTotal = 0;
            decimal opADBaseAmountTotal = 0;
            decimal opTotalRow = 0;
            var savedOPInfo = GetJVOpeningBalanceFixedAssetItem(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, master.CompanyId);
            var savedRegInfo = GetRegisterInfoWithFAMId(master.MaterialMasterId, null, null, master.CompanyId);
            foreach (var item in savedRegInfo)
            {
                var dic = (Dictionary<string, object>)item;
                reFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                reFAGroupAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FAGroupAmountTotal"].ToString()));
                reFAHardAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FAHardAmountTotal"].ToString()));
                reADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                reADGroupAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADGroupAmountTotal"].ToString()));
                reADHardAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADHardAmountTotal"].ToString()));
                reTotalRow = Convert.ToDecimal(dic["TotalRow"].ToString());
            }
            foreach (var item in savedOPInfo)
            {
                var dic = (Dictionary<string, object>)item;
                opFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                opADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                opTotalRow = Convert.ToDecimal(CheckNumIsExist(dic["TotalRow"].ToString()));
            }
            #endregion Insert n Update
            var _pk = GetPK();
            var flag = false;
            var builder = new System.Text.StringBuilder();
            var value = "''";
            try
            {
                if (opTotalRow > 0 && reTotalRow + NumberOfQuantity > opTotalRow)
                {
                    throw new CustomException("Register quantity  must be below opening balance quantity");
                }
                if (NumberOfQuantity > 0)
                {
                    #region--Save Validation
                    if (master.IsFinancial)
                    {
                        var materialMaster = _materialMasterRepository.Find(master.MaterialMasterId);
                        if (null == materialMaster)
                            throw new CustomException("Material Master is null.");
                        var faTagData = _fixedAssetMasterBudgetTagRepository.Query().Select().FirstOrDefault();
                        //var faTagData = _fixedAssetMasterBudgetTagRepository.Query(r => r.BudgetMasterId == materialMaster.BudgetMasterId).Select().FirstOrDefault();
                        if (null == faTagData)
                            throw new CustomException("Fixed Asset Master Tag data not found.");

                        var checkACUD = _fixedAssetMasterGLepository.Query(r => r.FixedAssetMasterId == faTagData.FixedAssetMasterId).Select().FirstOrDefault();
                        if (checkACUD == null)
                            throw new CustomException("This item is not configured with Accumulative Depreciation GL");
                        else
                        {
                            if (checkACUD.AccumulatedDepreciationGLId == null)
                            {
                                throw new CustomException(checkACUD.FixedAssetMaster.UserName + " is not configured with Accumulative Depreciation GL");
                            }
                        }
                        if ((master.FABaseAmount * NumberOfQuantity + reFABaseAmountTotal) > opFABaseAmountTotal)
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        }



                        if ((master.ADBaseAmount * NumberOfQuantity + reADBaseAmountTotal) > opADBaseAmountTotal)
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADBaseAmountTotal);
                        }


                    }
                    // builder.Append(value);
                    #endregion
                    for (int i = 0; i < NumberOfQuantity; i++)
                    {
                        var lc = localItem;
                        var asItem = assetItemValue;
                        var id = MakePK(_pk, i, 2);
                        InitInsert(id, master, out lc);
                        InsertOrUpdateGraph(lc);
                        //_assetItemValueService.InsertOrUpdateGraph(asItem, lc.Id);
                        _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(lc, fixedAssetRegisterSkuValue);
                        if (value == "''")
                        {
                            value = "'" + id + "'";
                            builder.Append(value);
                        }
                        else
                        { builder.Append(",'" + id + "'"); }
                    }
                }
                else
                {
                    localItem = GetItem(master.Id);
                    #region-- Update Validation
                    if (master.IsFinancial)
                    {
                        if (!((reFABaseAmountTotal - (localItem.FABaseAmount) + master.FABaseAmount) <= opFABaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        }
                        if (!((reADBaseAmountTotal - localItem.ADBaseAmount + master.ADBaseAmount) <= opADBaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADBaseAmountTotal);
                        }
                    }
                    #endregion
                    OutItem(_pk, master, ref localItem);
                    InsertOrUpdateGraph(localItem);
                    _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(master, fixedAssetRegisterSkuValue);
                    value = "'" + master.Id + "'";
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                masterid = builder.ToString();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, master.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public IEnumerable<object> CheckFixedMasterIsRegisterApplyByOBJV(string assetMasterId)
        {
            try
            {
                var sql = @"SELECT OB.IsPark From TRN.OpeningBalanceDetail AS OBD
							LEFT JOIN TRN.OpeningBalance OB ON OBD.OpeningBalanceId=OB.Id
                            WHERE OBD.FixedAssetMasterId='" + assetMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetJVOpeningBalanceFixedAssetItem(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        {
            var accDepGL = _fixedAssetMasterGLepository.Query(r => r.FixedAssetMasterId == fixedAssetMasterId).Select().FirstOrDefault();
            return GetJVOpeningBalanceFixedAssetItem(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, companyId, accDepGL.AccumulatedDepreciationGLId, accDepGL.AccumulatedDepreciationBudgetMasterId, accDepGL.AccumulatedDepreciationActivityId);
        }
        public IEnumerable<object> GetJVOpeningBalanceFixedAssetItem(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId, string accDepGLId, string accDepBudgetId, string accDepActivityId)
        {
            try
            {
                var sql = @"SELECT SUM(x.TOTALROW) TotalRow,SUM(x.FABaseAmountTotal)FABaseAmountTotal,SUM(X.ADBaseAmountTotal)ADBaseAmountTotal
                        FROM (
					SELECT 0 AS TotalRow,CC.CompanyCurrencyAmount AS FABaseAmountTotal,FOBD.FixedAssetMasterId,FOBD.FAType,
					FOBD.Id , 0 ADBaseAmountTotal
						   FROM [TRN].[OpeningBalanceDetail] AS FOBD
                        INNER JOIN [TRN].[OpeningBalance] AS FOB ON FOB.Id=FOBD.OpeningBalanceId
                        INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.GLGeneralInfoId
                        INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
				 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
				 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.DrAmount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
				 FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
				) AS CC ON CC.OpeningBalanceDetailId=FOBD.Id

				WHERE  FOBD.FAType='AssetCapatalized'
				AND FOBD.FixedAssetMasterId='" + fixedAssetMasterId + "' AND FOBD.GLGeneralInfoId='" + assetGLId + "' AND FOBD.BudgetMasterId='" + assetBudgetId + "' AND FOBD.ActivityId='" + assetActivityId + @"'
			    AND FOB.IsPark=0
                UNION
					 SELECT  0 AS TotalRow,0 FABaseAmountTotal,null FixedAssetMasterId,null FAType, 
					 FOBD.Id,CC.CompanyCurrencyAmount AS ADBaseAmountTotal
				FROM [TRN].[OpeningBalanceDetail] AS FOBD
				INNER JOIN [TRN].[OpeningBalance] AS FOB ON FOB.Id=FOBD.OpeningBalanceId
				INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.GLGeneralInfoId
				INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
					 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
					 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.CrAmount AS CompanyCurrencyAmount, OBDC.OpeningBalanceDetailId
					 FROM [TRN].[OpeningBalanceDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					 INNER JOIN [TRN].[OpeningBalanceDetail] AS OBD ON OBD.Id=OBDC.OpeningBalanceDetailId
					WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
					) AS CC ON CC.OpeningBalanceDetailId=FOBD.Id
					WHERE FOBD.FAType='AccDept'
				AND FOBD.FixedAssetMasterId='" + fixedAssetMasterId + "' AND FOBD.GLGeneralInfoId='" + accDepGLId + "' AND FOBD.BudgetMasterId='" + accDepBudgetId + "' AND FOBD.ActivityId='" + accDepActivityId + @"'
				AND FOB.IsPark=0
                ) X";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public IEnumerable<object> GetJVOBRegisterInfoWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string activityId, string companyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT SUM(X.FABaseAmountTotal)AS FABaseAmountTotal, SUM(X.ADBaseAmountTotal) AS ADBaseAmountTotal
						FROM 
						(
						SELECT COUNT(FAR.Id) AS TotalRow
                            , SUM(FAR.FABaseAmount) AS FABaseAmountTotal
                            , SUM(FAR.FAGroupAmount) AS FAGroupAmountTotal
                            , SUM(FAR.FAHardAmount) AS FAHardAmountTotal
                            , 0 ADBaseAmountTotal
                            , 0 ADGroupAmountTotal
                            , 0 ADHardAmountTotal
                            FROM [TRN].[FixedAssetRegister] FAR
                            LEFT JOIN MST.MaterialMaster MM ON FAR.MaterialMasterId=MM.Id
                            LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                            LEFT JOIN HKP.GLGeneralInfo GL ON BM.GLGeneralInfoId=GL.Id
                            LEFT JOIN HKP.Activity A ON MM.ActivityId=A.Id
                            WHERE MM.BudgetMasterId='" + budgetMasterId + "' AND BM.GLGeneralInfoId='" + assetGLId + "' AND MM.ActivityId='" + activityId + @"'
                            AND FAR.CompanyId='" + companyId + @"' AND FAR.IsOpeningBalance=1 AND FAR.IsFinancial=1
							UNION
							SELECT COUNT(FAR.Id) AS TotalRow
                            , 0 FABaseAmountTotal
                            , 0 FAGroupAmountTotal
                            , 0 FAHardAmountTotal
                            , SUM(FAR.ADBaseAmount) AS ADBaseAmountTotal
                            , SUM(FAR.ADGroupAmount) AS ADGroupAmountTotal
                            , SUM(FAR.ADHardAmount) AS ADHardAmountTotal
                            FROM [TRN].[FixedAssetRegister] FAR
                            LEFT JOIN MST.MaterialMaster MM ON FAR.MaterialMasterId=MM.Id
                            LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                            LEFT JOIN HKP.GLGeneralInfo GL ON BM.GLGeneralInfoId=GL.Id
							LEFT JOIN hkp.FixedAssetMasterBudgetTag FAMT ON FAMT.BudgetMasterId=BM.Id
							LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMT.FixedAssetMasterId
                            LEFT JOIN HKP.Activity A ON MM.ActivityId=A.Id
                            WHERE FAMT.FixedAssetMasterId='" + assetMasterId + @"'
                            AND FAR.CompanyId='" + companyId + @"' AND FAR.IsOpeningBalance=1 AND FAR.IsFinancial=1
							) X";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetOBFixedAssetList(string companyId, string plantId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT OB.DocRefNo,V.VoucherNo,FAM.UserName FixedAsset,GL.UserName GLName,B.UserName BudgetName,OBD.GLGeneralInfoId,OBD.BudgetMasterId,OBD.ActivityId
                                        ,A.UserName ActivityName,OBD.DrAmount Amount ,R.FABaseAmountTotal RegisterAmount,R.Price, (OBD.DrAmount-(R.FABaseAmountTotal+ISNULL(r.SubAssetAmount,0))) Balance,ISNULL(r.SubAssetAmount,0) SubAssetAmount
                                        FROM TRN.OpeningBalanceDetail OBD
                                        LEFT JOIN  TRN.OpeningBalance OB ON OB.Id=OBD.OpeningBalanceId
                                        LEFT JOIN HKP.GLGeneralInfo GL ON GL.Id=OBD.GLGeneralInfoId
                                        LEFT JOIN MST.BudgetMaster BM ON BM.Id=OBD.BudgetMasterId
                                        LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
                                        LEFT JOIN HKP.Activity A ON A.Id=OBD.ActivityId
                                        LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMB ON FAMB.BudgetMasterId=OBD.BudgetMasterId
                                        LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMB.FixedAssetMasterId
                                        LEFT JOIN TRN.Voucher V ON V.Id=OB.VoucherId
										LEFT JOIN (
										SELECT    SUM(FAR.FABaseAmount) AS FABaseAmountTotal,SUM(FAR.Price) Price,FAR.FABudgetMasterId,SUM(SR.SubAssetAmount) SubAssetAmount
												FROM [TRN].[FixedAssetRegister] FAR
												LEFT JOIN MST.MaterialMaster MM ON FAR.MaterialMasterId=MM.Id
												LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
												LEFT JOIN (SELECT FixedAssetRegisterId,SUM(Amount) SubAssetAmount FROM TRN.SubFixedAssetRegister 
													GROUP BY FixedAssetRegisterId) SR ON SR.FixedAssetRegisterId=FAR.Id
												AND FAR.CompanyId='" + companyId + "' AND FAR.PlantId='" + plantId + @"' AND FAR.IsOpeningBalance=1 AND FAR.IsFinancial=1
					WHERE FAR.IsOpeningBalance=1 AND FAR.IsFinancial=1							
GROUP BY FAR.FABudgetMasterId
										) R ON R.FABudgetMasterId=OBD.BudgetMasterId
                                        WHERE OBD.PartyType='FixedAsset' AND OBD.FAType='AssetCapatalized' AND OB.CompanyId='" + companyId + "' AND OB.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region fixed Asset Register JV 

        public GridModel GetJVFixedAssetRegisterList(GridParameter parameters, string[] ids)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FR.Id,FR.Id AS FixedAssetRegisterId,V.VoucherNo, FR.MaterialMasterArticleId, FR.MaterialMasterId
                                    , FR.SerialNo, FR.Id AssetNo, FR.InvoiceNo, MM.UserName MaterialMasterName
                                    , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                                    , FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                                    , FAM.FixedAssetSubCategoryId, FAM.AssetType, FR.Price PurchasePrice,FR.FABaseAmount
									, MMA.StandardName Article, FR.IsFinancial,IID.InventoryIssueId IssueNo,IRD.InventoryReceiveId GRNNo,FR.CapitalizeRegisterNo
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
                                    WHERE FR.CompanyId='" + identity.CompanyId + @"' and FR.IsOpeningBalance=0 and FR.Archive=0 and FR.IsAUC=0
                                    AND FR.Id NOT IN(" + ReturnStringArray(ids) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> CheckFixedMasterIsRegisterApplyByJV(string assetMasterId)
        {
            try
            {
                var sql = @"SELECT OB.IsPark From TRN.VoucherDetail AS OBD
							LEFT JOIN TRN.Voucher OB ON OBD.VoucherId=OB.Id
                            WHERE OBD.FixedAssetMasterId='" + assetMasterId + "' AND OBD.OpeningBalanceDetailId IS NULL";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public IEnumerable<object> GetJVFixedAssetItem(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        {
            try
            {
                var accDepGL = _fixedAssetMasterGLepository.Query(r => r.FixedAssetMasterId == fixedAssetMasterId).Select().FirstOrDefault();
                if (accDepGL == null)
                    throw new CustomException("Fixed Asset Master Account Determinate is not set!");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"SELECT SUM(x.TOTALROW) TotalRow,SUM(x.FABaseAmountTotal)FABaseAmountTotal,SUM(X.ADBaseAmountTotal)ADBaseAmountTotal
                        FROM (
					SELECT 0 AS TotalRow,CC.CompanyCurrencyAmount AS FABaseAmountTotal,FOBD.FixedAssetMasterId,FOBD.FAType,
					FOBD.Id , 0 ADBaseAmountTotal
						   FROM [TRN].[VoucherDetail] AS FOBD
                        INNER JOIN [TRN].[Voucher] AS FOB ON FOB.Id=FOBD.VoucherId
                        INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.GLGeneralInfoId
                        INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
				 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
				 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.DrAmount AS CompanyCurrencyAmount, OBDC.VoucherDetailId
				 FROM [TRN].[VoucherDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
				) AS CC ON CC.VoucherDetailId=FOBD.Id

				WHERE  FOBD.FAType='AssetCapatalized'
				AND --FOBD.FixedAssetMasterId='" + fixedAssetMasterId + @"' AND 
                FOBD.GLGeneralInfoId='" + assetGLId + "' AND FOBD.BudgetMasterId='" + assetBudgetId + "' AND FOBD.ActivityId='" + assetActivityId + @"'
			    AND FOB.IsPark=0 AND FOBD.OpeningBalanceDetailId IS NULL
                UNION
					 SELECT  0 AS TotalRow,0 FABaseAmountTotal,null FixedAssetMasterId,null FAType, 
					 FOBD.Id,CC.CompanyCurrencyAmount AS ADBaseAmountTotal
				FROM [TRN].[VoucherDetail] AS FOBD
				INNER JOIN [TRN].[Voucher] AS FOB ON FOB.Id=FOBD.VoucherId
				INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.GLGeneralInfoId
				INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
					 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
					 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.CrAmount AS CompanyCurrencyAmount, OBDC.VoucherDetailId
					 FROM [TRN].[VoucherDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					 INNER JOIN [TRN].[VoucherDetail] AS OBD ON OBD.Id=OBDC.VoucherDetailId
					WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
					) AS CC ON CC.VoucherDetailId=FOBD.Id
					WHERE FOBD.FAType='AccDept'
				AND --FOBD.FixedAssetMasterId='" + fixedAssetMasterId + @"' AND 
                FOBD.GLGeneralInfoId='" + accDepGL.AccumulatedDepreciationGLId + "' AND FOBD.BudgetMasterId='" + accDepGL.AccumulatedDepreciationBudgetMasterId + "' AND FOBD.ActivityId='" + accDepGL.AccumulatedDepreciationActivityId + @"'
				AND FOB.IsPark=0 AND FOBD.OpeningBalanceDetailId IS NULL
                ) X";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetJVRegisterInfoWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string activityId, string companyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT SUM(X.FABaseAmountTotal)AS FABaseAmountTotal, SUM(X.ADBaseAmountTotal) AS ADBaseAmountTotal
						FROM 
						(
						SELECT COUNT(FAR.Id) AS TotalRow
                            , SUM(FAR.FABaseAmount) AS FABaseAmountTotal
                            , SUM(FAR.FAGroupAmount) AS FAGroupAmountTotal
                            , SUM(FAR.FAHardAmount) AS FAHardAmountTotal
                            , 0 ADBaseAmountTotal
                            , 0 ADGroupAmountTotal
                            , 0 ADHardAmountTotal
                            FROM [TRN].[FixedAssetRegister] FAR
                            LEFT JOIN MST.MaterialMaster MM ON FAR.MaterialMasterId=MM.Id
                            LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                            LEFT JOIN HKP.GLGeneralInfo GL ON BM.GLGeneralInfoId=GL.Id
                            LEFT JOIN HKP.Activity A ON MM.ActivityId=A.Id
                            WHERE MM.BudgetMasterId='" + budgetMasterId + "' AND BM.GLGeneralInfoId='" + assetGLId + "' AND MM.ActivityId='" + activityId + @"'
                            AND FAR.CompanyId='" + companyId + @"' AND FAR.IsOpeningBalance=0 AND FAR.IsFinancial=1
							UNION
							SELECT COUNT(FAR.Id) AS TotalRow
                            , 0 FABaseAmountTotal
                            , 0 FAGroupAmountTotal
                            , 0 FAHardAmountTotal
                            , SUM(FAR.ADBaseAmount) AS ADBaseAmountTotal
                            , SUM(FAR.ADGroupAmount) AS ADGroupAmountTotal
                            , SUM(FAR.ADHardAmount) AS ADHardAmountTotal
                            FROM [TRN].[FixedAssetRegister] FAR
                            LEFT JOIN MST.MaterialMaster MM ON FAR.MaterialMasterId=MM.Id
                            LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                            LEFT JOIN HKP.GLGeneralInfo GL ON BM.GLGeneralInfoId=GL.Id
							LEFT JOIN hkp.FixedAssetMasterBudgetTag FAMT ON FAMT.BudgetMasterId=BM.Id
							LEFT JOIN MST.FixedAssetMaster FAM ON FAM.Id=FAMT.FixedAssetMasterId
                            LEFT JOIN HKP.Activity A ON MM.ActivityId=A.Id
                            WHERE FAMT.FixedAssetMasterId='" + assetMasterId + @"'
                            AND FAR.CompanyId='" + companyId + @"' AND FAR.IsOpeningBalance=0 AND FAR.IsFinancial=1
							) X";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetJVSubAssetList(string fixedAssetRegisterId)
        {
            try
            {
                var sql = @"SELECT SAT.UserName SubAssetTypeName,SFR.Id,SFR.FixedAssetRegisterId,SFR.SubAssetTypeId,SFR.CurrencyId,C.Code SubAssetCurrencyCode
                        , Replace(CONVERT(VARCHAR(11), SFR.CapitalizationDate, 106), ' ', '-') CapitalizationDate,SFR.CapitalizationRate,SFR.CapitalizeRegisterNo
                        , SFR.BaseCurrencyId, SFR.BaseAmount,SFR.Amount
                        FROM TRN.SubFixedAssetRegister SFR
                            left join hkp.SubAssetType SAT ON SAT.Id=SFR.SubAssetTypeId
							LEFT JOIN SCS.Currency C ON C.Id=SFR.CurrencyId
                            WHERE SFR.FixedAssetRegisterId='" + fixedAssetRegisterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertORUpdateItemJV(FixedAssetRegister master, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode
           , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
           , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId)
        {
            FixedAssetRegister localItem = null;
            masterid = string.Empty;
            #region-- Assign Amount
            decimal reFABaseAmountTotal = 0;
            decimal reFAGroupAmountTotal = 0;
            decimal reFAHardAmountTotal = 0;
            decimal reADBaseAmountTotal = 0;
            decimal reADGroupAmountTotal = 0;
            decimal reADHardAmountTotal = 0;
            decimal reTotalRow = 0;
            //-----
            decimal opFABaseAmountTotal = 0;
            decimal opFAGroupAmountTotal = 0;
            decimal opFAHardAmountTotal = 0;
            decimal opADBaseAmountTotal = 0;
            decimal opADGroupAmountTotal = 0;
            decimal opADHardAmountTotal = 0;
            decimal opTotalRow = 0;

            decimal remainingNoOfQuantity = 0;
            decimal tempTotalBooksAmount = 0;
            decimal tempTotalAmount = 0;
            decimal tempcumulativePrice = 0;
            decimal tempPrice = 0;
            decimal tempFAAmount = 0;
            decimal tempADAmount = 0;
            decimal tempUnitTrnPrice = 0;
            remainingNoOfQuantity = NumberOfQuantity;
            tempPrice = master.PurchasePrice;
            tempFAAmount = master.FABaseAmount;
            tempADAmount = master.ADBaseAmount;
            decimal subassestAmount = 0;

            var savedOPInfo = GetJVFixedAssetItem(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, master.CompanyId);
            var savedRegInfo = GetRegisterInfoWithFAMId(master.MaterialMasterId, null, null, master.CompanyId);
            master.Year = DateTime.Now.Year.ToString();
            var maxAssetNo = _fixedAssetRegisterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(AssetNo, 2) AS INT)), 0) AssetNo FROM [TRN].[FixedAssetRegister] WHERE PlantId='{master.PlantId}' AND [Year]='{master.Year}'").First();

            foreach (var item in savedRegInfo)
            {
                var dic = (Dictionary<string, object>)item;
                reFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                reADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                reTotalRow = Convert.ToDecimal(dic["TotalRow"].ToString());
            }
            foreach (var item in savedOPInfo)
            {
                var dic = (Dictionary<string, object>)item;
                opFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                opADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                opTotalRow = Convert.ToDecimal(CheckNumIsExist(dic["TotalRow"].ToString()));
            }
            #endregion Insert n Update
            var _pk = GetPK();
            var flag = false;
            var builder = new System.Text.StringBuilder();
            var value = "''";
            try
            {
                if (opTotalRow > 0 && reTotalRow + NumberOfQuantity > opTotalRow)
                {
                    throw new CustomException("Register quantity  must be below opening balance quantity");
                }
                if (NumberOfQuantity > 0)
                {
                    #region--Save Validation
                    if (master.IsFinancial)
                    {
                        var materialMaster = _materialMasterRepository.Find(master.MaterialMasterId);
                        if (null == materialMaster)
                            throw new CustomException("Material Master is null.");
                        var faTagData = _fixedAssetMasterBudgetTagRepository.Query().Select().FirstOrDefault();
                        //var faTagData = _fixedAssetMasterBudgetTagRepository.Query(r => r.BudgetMasterId == materialMaster.BudgetMasterId).Select().FirstOrDefault();
                        if (null == faTagData)
                            throw new CustomException("Fixed Asset Master Tag data not found.");

                        var checkACUD = _fixedAssetMasterGLepository.Query(r => r.FixedAssetMasterId == faTagData.FixedAssetMasterId).Select().FirstOrDefault();
                        if (checkACUD == null)
                            throw new CustomException("This item is not configured with Accumulative Depreciation GL");
                        else
                        {
                            if (checkACUD.AccumulatedDepreciationGLId == null)
                            {
                                throw new CustomException(checkACUD.FixedAssetMaster.UserName + " is not configured with Accumulative Depreciation GL");
                            }
                        }
                        if ((master.FABaseAmount * NumberOfQuantity + reFABaseAmountTotal) > opFABaseAmountTotal)
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        }
                        if ((master.FAGroupAmount * NumberOfQuantity + reFAGroupAmountTotal) > opFAGroupAmountTotal)
                        {
                            throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAGroupAmountTotal);
                        }
                        if ((master.FAHardAmount * NumberOfQuantity + reFAHardAmountTotal) > opFAHardAmountTotal)
                        {
                            throw new CustomException(HardCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAHardAmountTotal);
                        }


                    }
                    //else { }
                    //if (subFixedAssetRegister != null && master.FABaseCurrencyId==master.CurrencyId)
                    //{
                    //    subassestAmount = Math.Round(subFixedAssetRegister.Sum(r => r.Amount) / NumberOfQuantity, 2);
                    //    //tempFAAmount -= subFixedAssetRegister.Sum(r => r.BaseAmount);
                    //    //tempPrice -= subFixedAssetRegister.Sum(r => r.Amount);
                    //}
                    //else
                    //{
                    //    subassestAmount = Math.Round(subFixedAssetRegister.Sum(r => r.Amount) / NumberOfQuantity, 2);
                    //    //tempFAAmount -= subFixedAssetRegister.Sum(r => r.BaseAmount);
                    //    //tempPrice -= subFixedAssetRegister.Sum(r => r.Amount);
                    //}

                    //master.FABaseAmount = Math.Round(master.FABaseAmount / NumberOfQuantity, 2);
                    master.ADBaseAmount = Math.Round(master.ADBaseAmount / NumberOfQuantity, 2);
                    tempUnitTrnPrice = Math.Round(master.PurchasePrice / NumberOfQuantity, 2);
                    master.FABaseAmount = master.UnitPrice;
                    if (master.Id != null)
                    {
                        tempFAAmount = master.UnitPrice;
                        tempPrice = master.PurchasePrice;
                    }
                    // builder.Append(value);
                    #endregion
                    for (int i = 0; i < NumberOfQuantity; i++)
                    {
                        master.AssetNo = master.Year.ToString().Substring(2, 2) + maxAssetNo;
                        if (i + 1 == NumberOfQuantity)
                        {
                            master.FABaseAmount = tempFAAmount - tempTotalAmount;
                            master.Price = tempPrice - tempcumulativePrice;
                            master.Quantity = 1;
                        }
                        else
                        {
                            tempTotalAmount += master.FABaseAmount;
                            master.Price = tempUnitTrnPrice;
                            tempcumulativePrice += tempUnitTrnPrice;
                            master.Quantity = 1;
                        }
                        var lc = localItem;
                        var asItem = assetItemValue;
                        var id = MakePK(_pk, i, 2);
                        InitInsert(id, master, out lc);
                        InsertOrUpdateGraph(lc);
                        var currentId = _subFixedAssetRegisterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SubFixedAssetRegister] WHERE FixedAssetRegisterId='{lc.Id}'").First();

                        /*Sub Fixed Asset Resigter If have */
                        if (subFixedAssetRegister != null)
                        {
                            foreach (var item in subFixedAssetRegister)
                            {
                                if (string.IsNullOrEmpty(item.Id))
                                {
                                    currentId++;
                                    var subFAR = new SubFixedAssetRegister
                                    {
                                        Id = lc.Id + currentId,
                                        Amount = Math.Round(item.Amount / NumberOfQuantity, 2),
                                        BaseAmount = Math.Round(item.BaseAmount / NumberOfQuantity, 2),
                                        FixedAssetRegisterId = lc.Id,
                                        SubAssetTypeId = item.SubAssetTypeId,
                                        CurrencyId = item.CurrencyId,
                                        BaseCurrencyId = item.BaseCurrencyId,
                                        CapitalizationDate = item.CapitalizationDate,
                                        CapitalizationRate = Math.Round(lc.FABaseAmount / lc.Price, 4)
                                    };
                                    AuditService.AddedLog(subFAR);
                                    _subFixedAssetRegisterRepository.Insert(subFAR);
                                }
                                else
                                {
                                    var subFAR = new SubFixedAssetRegister
                                    {
                                        Amount = Math.Round(item.Amount, 2),
                                        BaseAmount = Math.Round(item.BaseAmount, 2),
                                        Id = item.Id,
                                        FixedAssetRegisterId = lc.Id,
                                        SubAssetTypeId = item.SubAssetTypeId,
                                        CurrencyId = item.CurrencyId,
                                        BaseCurrencyId = item.BaseCurrencyId,
                                        CapitalizationDate = item.CapitalizationDate,
                                        CapitalizationRate = Math.Round(lc.FABaseAmount / lc.Price, 4)
                                    };
                                    AuditService.UpdatedLog(subFAR);
                                    _subFixedAssetRegisterRepository.Update(subFAR);
                                }
                            }
                        }

                        //_assetItemValueService.InsertOrUpdateGraph(asItem, lc.Id);
                        _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(lc, fixedAssetRegisterSkuValue);
                        if (value == "''")
                        {
                            value = "'" + id + "'";
                            builder.Append(value);
                        }
                        else
                        { builder.Append(",'" + id + "'"); }
                    }
                }
                else
                {
                    localItem = GetItem(master.Id);
                    #region-- Update Validation
                    if (master.IsFinancial)
                    {
                        if (!((reFABaseAmountTotal - (localItem.FABaseAmount) + master.FABaseAmount) <= opFABaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        }
                        if (!((reFAGroupAmountTotal - localItem.FAGroupAmount + master.FAGroupAmount) <= opFAGroupAmountTotal))
                        {
                            throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAGroupAmountTotal);
                        }
                        if (!((reFAHardAmountTotal - localItem.FAHardAmount + master.FAHardAmount) <= opFAHardAmountTotal))
                        {
                            throw new CustomException(HardCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAHardAmountTotal);
                        }
                        if (!((reADBaseAmountTotal - localItem.ADBaseAmount + master.ADBaseAmount) <= opADBaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADBaseAmountTotal);
                        }
                        if (!((reADGroupAmountTotal - localItem.ADGroupAmount + master.ADGroupAmount) <= opADGroupAmountTotal))
                        {
                            throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADGroupAmountTotal);
                        }
                        if (!((reADHardAmountTotal - localItem.ADHardAmount + master.ADHardAmount) <= opADHardAmountTotal))
                        {
                            throw new CustomException(HardCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADHardAmountTotal);
                        }
                    }
                    #endregion
                    OutItem(_pk, master, ref localItem);
                    InsertOrUpdateGraph(localItem);
                    _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(master, fixedAssetRegisterSkuValue);
                    value = "'" + master.Id + "'";
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                masterid = builder.ToString();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, master.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #endregion

        #region fixed Asset Register AUC JV 

        public GridModel GetAUCJVFixedAssetRegisterList(GridParameter parameters, string[] ids)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT FR.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.MaterialMasterId
                                    , FR.SerialNo, FR.Id AssetNo, FR.InvoiceNo, MM.UserName MaterialMasterName
                                    , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                                    , FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                                    , FAM.FixedAssetSubCategoryId, FAM.AssetType, FR.Price PurchasePrice
									, MMA.StandardName Article, FR.IsFinancial
                                    FROM [TRN].[FixedAssetRegister] FR
					                LEFT JOIN MST.MaterialMaster MM ON FR.MaterialMasterId=MM.Id
					                LEFT JOIN MST.MaterialMasterArticle MMA ON FR.MaterialMasterArticleId= MMA.Id
                                    LEFT JOIN MST.BudgetMaster BM ON MM.BudgetMasterId = BM.Id
                                    LEFT JOIN HKP.FixedAssetMasterBudgetTag FAMT ON BM.Id=FAMT.BudgetMasterId AND MM.BudgetMasterId=FAMT.BudgetMasterId
                                    LEFT JOIN [MST].[FixedAssetMaster] FAM ON FAMT.FixedAssetMasterId= FAM.Id
                                    LEFT JOIN HKP.FixedAssetCategory FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                    LEFT JOIN HKP.FixedAssetSubCategory FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                                    WHERE FR.CompanyId='" + identity.CompanyId + @"' and FR.IsOpeningBalance=0 and FR.Archive=0 and FR.IsAUC=1 
                                    AND FR.Id NOT IN(" + ReturnStringArray(ids) + ")";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> CheckFixedMasterIsRegisterApplyByAUCJV(string assetMasterId)
        {
            try
            {
                var sql = @"SELECT OB.IsPark From TRN.VoucherDetail AS OBD
							LEFT JOIN TRN.Voucher OB ON OBD.VoucherId=OB.Id
                            WHERE OBD.FixedAssetMasterId='" + assetMasterId + "' AND OBD.OpeningBalanceDetailId IS NULL AND OBD.FAType='AssetNonCapitalized'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetAUCJVFixedAssetItem(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        {
            try
            {
                var accDepGL = _fixedAssetMasterGLepository.Query(r => r.FixedAssetMasterId == fixedAssetMasterId).Select().FirstOrDefault();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"SELECT SUM(x.TOTALROW) TotalRow,SUM(x.FABaseAmountTotal)FABaseAmountTotal,SUM(X.ADBaseAmountTotal)ADBaseAmountTotal
                        FROM (
					SELECT 0 AS TotalRow,CC.CompanyCurrencyAmount AS FABaseAmountTotal,FOBD.FixedAssetMasterId,FOBD.FAType,
					FOBD.Id , 0 ADBaseAmountTotal
						   FROM [TRN].[VoucherDetail] AS FOBD
                        INNER JOIN [TRN].[Voucher] AS FOB ON FOB.Id=FOBD.VoucherId
                        INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.GLGeneralInfoId
                        INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
				 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
				 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.DrAmount AS CompanyCurrencyAmount, OBDC.VoucherDetailId
				 FROM [TRN].[VoucherDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
				) AS CC ON CC.VoucherDetailId=FOBD.Id

				WHERE  FOBD.FAType='AssetNonCapitalized'
				AND FOBD.FixedAssetMasterId='" + fixedAssetMasterId + "' AND FOBD.GLGeneralInfoId='" + assetGLId + "' AND FOBD.BudgetMasterId='" + assetBudgetId + "' AND FOBD.ActivityId='" + assetActivityId + @"'
			    AND FOB.IsPark=0 AND FOBD.OpeningBalanceDetailId IS NULL
                UNION
					 SELECT  0 AS TotalRow,0 FABaseAmountTotal,null FixedAssetMasterId,null FAType, 
					 FOBD.Id,CC.CompanyCurrencyAmount AS ADBaseAmountTotal
				FROM [TRN].[VoucherDetail] AS FOBD
				INNER JOIN [TRN].[Voucher] AS FOB ON FOB.Id=FOBD.VoucherId
				INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.GLGeneralInfoId
				INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
					 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
					 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.CrAmount AS CompanyCurrencyAmount, OBDC.VoucherDetailId
					 FROM [TRN].[VoucherDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					 INNER JOIN [TRN].[VoucherDetail] AS OBD ON OBD.Id=OBDC.VoucherDetailId
					WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
					) AS CC ON CC.VoucherDetailId=FOBD.Id
					WHERE FOBD.FAType='AccDept'
				AND FOBD.FixedAssetMasterId='" + fixedAssetMasterId + "' AND FOBD.GLGeneralInfoId='" + accDepGL.AccumulatedDepreciationGLId + "' AND FOBD.BudgetMasterId='" + accDepGL.AccumulatedDepreciationBudgetMasterId + "' AND FOBD.ActivityId='" + accDepGL.AccumulatedDepreciationActivityId + @"'
				AND FOB.IsPark=0 AND FOBD.OpeningBalanceDetailId IS NULL
                ) X";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IEnumerable<object> GetAUCJVRegisterInfoWithFAMId(string assetMasterId, string budgetMasterId, string assetGLId, string activityId, string companyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT SUM(X.TotalRow) TotalRow,SUM(X.FABaseAmountTotal)AS FABaseAmountTotal, SUM(X.ADBaseAmountTotal) AS ADBaseAmountTotal
						FROM 
						(
						SELECT COUNT(FAR.Id) AS TotalRow
                            , SUM(FAR.FABaseAmount) AS FABaseAmountTotal
                            , SUM(FAR.FAGroupAmount) AS FAGroupAmountTotal
                            , SUM(FAR.FAHardAmount) AS FAHardAmountTotal
                            , 0 ADBaseAmountTotal
                            , 0 ADGroupAmountTotal
                            , 0 ADHardAmountTotal
                            FROM [TRN].[FixedAssetRegister] FAR
                            LEFT JOIN MST.MaterialMaster MM ON FAR.MaterialMasterId=MM.Id
                            LEFT JOIN HKP.FixedAssetMasterGL famgl ON MM.FixedAssetMasterId=famgl.FixedAssetMasterId
                            WHERE famgl.AssetUnderConstructionBudgetMasterId='" + budgetMasterId + "' AND famgl.AssetUnderConstructionGLId='" + assetGLId + "' AND famgl.AssetUnderConstructionActivityId='" + activityId + @"'
                            AND FAR.CompanyId='" + companyId + @"' AND FAR.IsOpeningBalance=0 AND FAR.IsFinancial=1
							) X";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetAUCJVSubAssetList(string fixedAssetRegisterId)
        {
            try
            {
                var sql = @"SELECT SAT.UserName SubAssetTypeName, SFR.* FROM TRN.SubFixedAssetRegister SFR
                            left join hkp.SubAssetType SAT ON SAT.Id=SFR.SubAssetTypeId
                            WHERE SFR.FixedAssetRegisterId='" + fixedAssetRegisterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertORUpdateItemAUCJV(FixedAssetRegister master, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode
           , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
           , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId)
        {
            FixedAssetRegister localItem = null;
            masterid = string.Empty;
            #region-- Assign Amount
            decimal reFABaseAmountTotal = 0;
            decimal reADBaseAmountTotal = 0;
            decimal reTotalRow = 0;
            //-----
            decimal opFABaseAmountTotal = 0;
            decimal opADBaseAmountTotal = 0;
            decimal opTotalRow = 0;
            var savedOPInfo = GetAUCJVFixedAssetItem(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, master.CompanyId);
            var savedRegInfo = GetAUCJVRegisterInfoWithFAMId(master.MaterialMasterId, assetBudgetId, assetGLId, assetActivityId, master.CompanyId);
            foreach (var item in savedRegInfo)
            {
                var dic = (Dictionary<string, object>)item;
                reFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                reADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                reTotalRow = Convert.ToDecimal(dic["TotalRow"].ToString());
            }
            foreach (var item in savedOPInfo)
            {
                var dic = (Dictionary<string, object>)item;
                opFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                opADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                opTotalRow = Convert.ToDecimal(CheckNumIsExist(dic["TotalRow"].ToString()));
            }
            #endregion Insert n Update
            var _pk = GetPK();
            var flag = false;
            var builder = new System.Text.StringBuilder();
            var value = "''";
            try
            {
                if (opTotalRow > 0 && reTotalRow + NumberOfQuantity > opTotalRow)
                {
                    throw new CustomException("Register quantity  must be below opening balance quantity");
                }
                if (NumberOfQuantity > 0)
                {
                    #region--Save Validation
                    if (master.IsFinancial)
                    {
                        var materialMaster = _materialMasterRepository.Find(master.MaterialMasterId);
                        if (null == materialMaster)
                            throw new CustomException("Material Master is null.");
                        //var faTagData = _fixedAssetMasterBudgetTagRepository.Query(r => r.BudgetMasterId == materialMaster.BudgetMasterId).Select().FirstOrDefault();
                        var faTagData = _fixedAssetMasterBudgetTagRepository.Query().Select().FirstOrDefault();
                        if (null == faTagData)
                            throw new CustomException("Fixed Asset Master Tag data not found.");

                        var checkACUD = _fixedAssetMasterGLepository.Query(r => r.FixedAssetMasterId == faTagData.FixedAssetMasterId).Select().FirstOrDefault();
                        if (checkACUD == null)
                            throw new CustomException("This item is not configured with Accumulative Depreciation GL");
                        else
                        {
                            if (checkACUD.AccumulatedDepreciationGLId == null)
                            {
                                throw new CustomException(checkACUD.FixedAssetMaster.UserName + " is not configured with Accumulative Depreciation GL");
                            }
                        }
                        if ((master.FABaseAmount * NumberOfQuantity + reFABaseAmountTotal) > opFABaseAmountTotal)
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        }

                    }
                    // builder.Append(value);
                    #endregion
                    for (int i = 0; i < NumberOfQuantity; i++)
                    {
                        var lc = localItem;
                        var asItem = assetItemValue;
                        var id = MakePK(_pk, i, 2);
                        InitInsert(id, master, out lc);
                        InsertOrUpdateGraph(lc);
                        var currentId = _subFixedAssetRegisterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[SubFixedAssetRegister] WHERE FixedAssetRegisterId='{lc.Id}'").First();

                        /*Sub Fixed Asset Resigter If have */
                        if (subFixedAssetRegister != null)
                        {
                            foreach (var item in subFixedAssetRegister)
                            {
                                if (string.IsNullOrEmpty(item.Id))
                                {
                                    currentId++;
                                    var subFAR = new SubFixedAssetRegister
                                    {
                                        Id = lc.Id + currentId,
                                        Amount = Math.Round(item.Amount, 2),
                                        FixedAssetRegisterId = lc.Id,
                                        SubAssetTypeId = item.SubAssetTypeId,
                                        CurrencyId = item.CurrencyId,
                                        CapitalizationDate = item.CapitalizationDate,
                                        CapitalizationRate = Math.Round(lc.FABaseAmount / lc.Price, 4)
                                    };
                                    AuditService.AddedLog(subFAR);
                                    _subFixedAssetRegisterRepository.Insert(subFAR);
                                }
                                else
                                {
                                    var subFAR = new SubFixedAssetRegister
                                    {
                                        Amount = Math.Round(item.Amount, 2),
                                        Id = item.Id,
                                        FixedAssetRegisterId = lc.Id,
                                        SubAssetTypeId = item.SubAssetTypeId,
                                        CapitalizationDate = item.CapitalizationDate,
                                        CurrencyId = item.CurrencyId,
                                        CapitalizationRate = Math.Round(lc.FABaseAmount / lc.Price, 4)
                                    };
                                    AuditService.UpdatedLog(subFAR);
                                    _subFixedAssetRegisterRepository.Update(subFAR);
                                }
                            }
                        }

                        //_assetItemValueService.InsertOrUpdateGraph(asItem, lc.Id);
                        _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(lc, fixedAssetRegisterSkuValue);
                        if (value == "''")
                        {
                            value = "'" + id + "'";
                            builder.Append(value);
                        }
                        else
                        { builder.Append(",'" + id + "'"); }
                    }
                }
                else
                {
                    localItem = GetItem(master.Id);
                    #region-- Update Validation
                    if (master.IsFinancial)
                    {
                        if (!((reFABaseAmountTotal - (localItem.FABaseAmount) + master.FABaseAmount) <= opFABaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        }
                    }
                    #endregion
                    OutItem(_pk, master, ref localItem);
                    InsertOrUpdateGraph(localItem);
                    _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(master, fixedAssetRegisterSkuValue);
                    value = "'" + master.Id + "'";
                }

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                masterid = builder.ToString();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, master.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }


        public IEnumerable<object> GetAUCList(string masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT m.Id, m.Id AssetNo, m.BrandId, m.CountryOfOriginId, M.MaterialMasterId, M.MaterialMasterArticleId, A.UserName AS MaterialMasterName
	                        , A.BaseUOMId, UOM.UserName AS BaseUOMName, p.UserName Vendor, m.SerialNo, m.InvoiceNo
                            , m.IsFinancial, Replace(CONVERT(VARCHAR(11), m.InvoiceDate, 106), ' ', '-') InvoiceDate
	                        , m.IsForProduction, m.Model, m.VendorId, M.PlantId, M.CurrencyId, M.CompanyId, M.CompanyGroupId, m.FABaseCurrencyId
	                        , m.FAGroupCurrencyId, m.FAHardCurrencyId, m.ADBaseCurrencyId, m.ADGroupCurrencyId, m.ADHardCurrencyId
	                        , m.FABaseAmount, m.FAGroupAmount, m.FAHardAmount, m.ADBaseAmount, m.ADGroupAmount, m.ADHardAmount
	                        , m.[LifeTime], Replace(CONVERT(VARCHAR(11), m.CapitalizationDate, 106), ' ', '-') CapitalizationDate
                            , m.Price, m.Price PurchasePrice, fam.UserName 'AssetMasterName', m.YearOfManufacture
	                        , m.YearOfInstallation, m.[Description]
	                        , A.FixedAssetMasterId, fam.UserName 'FixedAssetMasterName'
	                        , c.UserName FixedAssetCategory, sc.UserName FixedAssetSubCategory
	                        , famgl.AssetUnderConstructionGLId AS AssetGLId, GL.UserName AssetGLName
	                        , famgl.AssetUnderConstructionBudgetMasterId AS AssetBudgetMasterId, B.UserName AssetBudgetName
	                        , famgl.AssetUnderConstructionActivityId AS AssetActivityId, AC.UserName AssetActivityName
	                        , cn.UserName Country, fam.AssetType, MMA.StandardName Article
                            FROM  TRN.[FixedAssetRegister]  m
                            LEFT JOIN MST.MaterialMaster A ON M.MaterialMasterId=A.Id
                            LEFT JOIN MST.MaterialMasterArticle MMA ON m.MaterialMasterArticleId= MMA.Id
                            LEFT JOIN SCS.UnitOfMeasurement UOM ON A.BaseUOMId = UOM.Id
                            LEFT JOIN SCS.[Country] cn ON cn.Id = m.CountryOfOriginId
                            LEFT join HKP.[Party]  p on p.Id=m.VendorId
                            LEFT JOIN [ORG].[Plant] PL ON M.PlantId = PL.Id
                            LEFT JOIN [ORG].Company CO ON M.CompanyId = CO.Id
                            LEFT JOIN MST.FixedAssetMaster fam ON A.FixedAssetMasterId=fam.Id
                            LEFT JOIN HKP.FixedAssetMasterGL famgl ON FAM.Id=famgl.FixedAssetMasterId
							 LEFT JOIN MST.BudgetMaster BM ON famgl.AssetUnderConstructionBudgetMasterId = BM.Id
                            LEFT JOIN HKP.Budget B ON  BM.BudgetId=B.Id
                            LEFT JOIN HKP.[FixedAssetCategory]  c ON c.Id = fam.FixedAssetCategoryId
                            LEFT JOIN HKP.[FixedAssetSubCategory]  sc ON sc.Id = fam.FixedAssetSubCategoryId
                            LEFT JOIN HKP.GLGeneralInfo GL ON famgl.AssetUnderConstructionGLId=GL.Id
                            LEFT JOIN HKP.Activity AC ON AC.Id=famgl.AssetUnderConstructionActivityId
                            WHERE m.CompanyId = '" + identity.CompanyId + "' and m.Id='" + masterId + @"'  and m.Archive=0 and m.IsAUC=1
                            Order by c.UserName, sc.UserName, m.SerialNo, m.InvoiceDate";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region FixedAssets AUC Capitalize GRN Bass

        public IEnumerable<object> GetGRNFixedAssetList(string plantId)
        {
            try
            {
                var sql = @"SELECT IR.Id GRNNo,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END, IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName,
                                     FORMAT(IR.GRNDate,'dd-MMM-yyyy') GRNDate, P.Code AS PartyCode, P.UserName AS PartyName, UoM.UserName AS TransactionUoM,CU.Code AS CurrencyCode
                                     , IR.IsNonCreditable, FORMAT(IR.MatureDate,'dd-MMM-yyyy') MatureDate, FORMAT(IR.EntryDate,'dd-MMM-yyyy') EntryDate, FORMAT(IR.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate, IR.DeliveryByAddress
                                     , DPP.UserName AS DeliveryBy, CP.UserName AS PartyAccountGroupName, PT.UserName AS PaymentTermName, IR.GateEntryNo, IR.InvoicingByAddress, IPP.UserName AS InvoicingBy,IR.ToCurrencyRate
                                     , MGM.UserName AS MaterialGroupMasterName
									 ,IR.POId, IRD.InventoryReceiveId, IRD.POId, IRD.PODetailsId, IRD.Id AS InventoryReceiveDetailId, IRD.InventoryMaterialId
									  ,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						, IRD.TransactionQty, IRD.BaseQty,IRD.BaseUOMId,IRD.TransactionUoMId
						,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) StockQty
						, ISNULL(IRD.IssueQty,0) IssueQty, ISNULL(IRD.BaseIssueQty,0) BaseIssueQty
						 ,ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0) AS BalanceStock
                        ,ISNULL(IRD.TotalMaterialTranAmount,0) TotalMaterialTranAmount
						 ,ISNULL(IRD.TotalMaterialBooksCurrencyAmount,0) TotalMaterialBooksCurrencyAmount
						 ,ISNULL(IRD.BooksCurrencyBaseRate,0) BooksCurrencyBaseRate
						 ,ISNULL(IRD.TrnCurrencyBaseRate,0) TrnCurrencyBaseRate
						 , IRD.MaterialTranRate, IRD.BooksCurrencyBaseRate, CU.Code AS TCurrency, IRD.MaterialTranAmount
                       , BaseRate=CASE WHEN IRD.TransactionUoMId<>IRD.BaseUOMId THEN IRD.MaterialTranAmount/IRD.BaseQty ELSE IRD.BooksCurrencyBaseRate END
                           , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS ReceiveDate, 0 AS RequisitionQty
							,(IRD.MaterialTranRate * IR.ToCurrencyRate) BaseCurrencyRate
						    ,GL.UserName GLName,GL.Id GLGeneralInfoId,IRD.PostDrBudgetMasterId BudgetMasterId,B.UserName BudgetName,IRD.PostDrActivityId ActivityId,A.UserName ActivityName
							, IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
									 FROM TRN.InventoryReceiveDetail IRD 
                                     LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
                                     LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                                     LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
								LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
							    JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							    LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							    LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							    LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							    LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                     LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IRD.TransactionUoMId=UoM.Id
									 LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
                                     JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                     LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                                      LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
                                     			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                                     JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                                     LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                                     WHERE IRD.IsAsset=1 AND IRD.CapitalizeVoucherDetailId IS NULL AND IR.PlantId='" + plantId + @"' AND (ISNULL(IRD.BaseQty,0) - ISNULL(IRD.BaseIssueQty, 0))>0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }
        public IEnumerable<object> GetGRNCapitalizeFixedAssetGL(string companyId, string inventoryDetailId)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(10)='" + inventoryDetailId + @"'
                SELECT  IR.Id,IRD.Id AS InventoryReceiveDetailId, 'Vendor' AS OtherName, 'Cr' AS TrnType ,NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,  IRD.PostDRGLGeneralInfoId GLGeneralInfoId
							,GLF.AccountCode GLGeneralInfoCode 
							, GLF.UserName GLGeneralInfoName
							, IRD.PostDRBudgetMasterId BudgetMasterId
							, BF.Code BudgetCode
							, BF.UserName BudgetName 
							, IRD.PostDRActivityId ActivityId
							, AF.Code ActivityCode
							, AF.UserName ActivityName
							,IRD.*
                            ,IRD.TotalMaterialBooksCurrencyAmount Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON IRD.PostDRGLGeneralInfoId=GLF.Id
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON IRD.PostDRBudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON IRD.PostDRActivityId= AF.Id
						WHERE IRD.Id=@receiveId 
						UNION
						SELECT  IR.Id,IRD.Id AS InventoryReceiveDetailId, 'Vendor' AS OtherName, 'Dr' AS TrnType ,NULL MaterialGroupMasterId, NULL AS TaxCategoryId
                            ,  GLF.Id GLGeneralInfoId
							,GLF.AccountCode GLGeneralInfoCode 
							, GLF.UserName GLGeneralInfoName
							, MM.BudgetMasterId 
							, BF.Code BudgetCode
							, BF.UserName BudgetName 
							, MM.ActivityId 
							, AF.Code ActivityCode
							, AF.UserName ActivityName
							,IRD.*
                        ,IRD.TotalMaterialBooksCurrencyAmount Amount
						FROM [TRN].[InventoryReceiveDetail] AS IRD 
						LEFT JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
						LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
						LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
						LEFT JOIN[MST].[BudgetMaster] AS BMF ON MM.BudgetMasterId= BMF.Id
						LEFT JOIN [HKP].[Budget] AS BF ON BMF.BudgetId= BF.Id
						LEFT JOIN[HKP].[GLGeneralInfo] AS GLF ON BMF.GLGeneralInfoId=GLF.Id
						LEFT JOIN [HKP].[Activity] AS AF ON MM.ActivityId= AF.Id
						WHERE IRD.Id=@receiveId ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel GetFixedAssetCapitalizeJournalData(GridParameter parameters, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT VDD.CrAmount Amount,ird.InventoryReceiveId GRNNo,V.*
                                      FROM TRN.Voucher V 
                                      LEFT JOIN (SELECT VD.Id,VD.VoucherId,VDC.CrAmount,vd.EmployeeId,vd.PartyId 
                                      FROM TRN.VoucherDetail VD 
                                      JOIN TRN.VoucherDetailCurrency VDC ON VDC.VoucherDetailId=VD.Id WHERE VDC.CrAmount>0) AS VDD ON VDD.VoucherId=V.Id
                                      LEFT JOIN TRN.InventoryReceiveDetail IRD on IRD.CapitalizeVoucherDetailId=VDD.Id
                                      WHERE V.SourceType='FixedAssetCapitalizeJournal' AND V.PlantId='" + plantId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        private Dictionary<string, object> GetFixedAssetCapitalizeJournalHeader(string companyGroupId, string companyId, string plantId, string voucherId, string sourceType)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode
                                    ,GRNNo =  STUFF((select distinct ','+XIRD.InventoryReceiveId  from
														 TRN.InventoryIssueHistory AS XIH  
														  join TRN.InventoryReceiveDetail XIRD ON XIRD.Id=XIH.InventoryReceiveDetailId
														  JOIN TRN.VoucherDetail XVD ON XVD.Id=XIH.CapitalizeVoucherDetailId
														  JOIN TRN.Voucher XV ON XV.Id=XVD.VoucherId
													    where	V.Id=XV.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,IssueNo =  STUFF((select distinct ','+XIID.InventoryIssueId  from
														 TRN.InventoryIssueHistory AS XIH  
														  join TRN.InventoryIssueDetail XIID ON XIID.Id=XIH.InventoryIssueDetailId
														  JOIN TRN.VoucherDetail XVD ON XVD.Id=XIH.CapitalizeVoucherDetailId
														  JOIN TRN.Voucher XV ON XV.Id=XVD.VoucherId
													    where	V.Id=XV.Id  for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' AND V.SourceType='" + sourceType + "'";
            return _sqlRepository.GetData(cmdText);
        }

        public IWorkbook GetFixedAssetCapitalizeJournalReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId, string sourceType)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetFixedAssetCapitalizeJournalHeader(companyGroupId, companyId, plantId, voucherId, sourceType);

            if (header.Count > 0)
            {
                reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];
            }
            else
            {
                reportFileName = "";
            }

            var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Entry Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Issue No.");
            reportUtility.SetText(ref sheet, row, 2, header["IssueNo"].ToString(), ExcelHAlign.HAlignLeft);

            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "GRN No.");
            reportUtility.SetText(ref sheet, row, 5, header["GRNNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;

                }


                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                var lastRow = row - 1;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, header["VoucherTypeName"].ToString(), companyId, plantId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        #endregion

        #region Issue AUC Capitalize
        public IEnumerable<object> GetIssueAssetAUCList(string plantId)
        {
            try
            {
                var sql = @"SELECT 0 Active,IR.Id IssueNo,IRD.Id InventoryIssueDetailId,IIH.Id InventoryIssueHistoryId,IVRD.Id InventoryReceiveDetailId,IVR.Id GRNNo,IVR.GateEntryNo,FORMAT(IVR.GRNDate,'dd-MMM-yyyy') GRNDate, [Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END, IVR.EmployeeId, EI.EmployeeCode, EI.EmployeeName,
                                     FORMAT(IR.IssueDate,'dd-MMM-yyyy') IssueDate,  UoM.UserName AS TransactionUoM
                                     , P.Code AS PartyCode, P.UserName AS PartyName
									 , FORMAT(IR.AddedDate,'dd-MMM-yyyy') EntryDate
                                     ,IR.IssueType,IRD.IsAsset
                                     , MGM.UserName AS MaterialGroupMasterName
									 , IRD.InventoryMaterialId
									  ,IIH.Qty TransactionQty
									  ,IIH.Qty BaseQty
									  ,GC.Code GRNCurrency,CU.Code AS CurrencyCode,IVR.CurrencyId,IVR.ToCurrencyRate
									  ,IIH.Rate BaseCurrencyRate
									  ,ROUND(IIH.TotalAmount,4) GRNAmount
									  ,ROUND(IIH.Qty*IVRD.BooksCurrencyBaseRate,4) Amount
                                      ,IRD.BaseUOMId,IRD.TransactionUoMId
							, IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
							, BM.GLGeneralInfoId PostCrGLGeneralInfoId , GL.AccountCode CrAccountCode, GL.UserName CrGLGeneralInfoName 
	                            , IRD.BudgetMasterId PostCrBudgetMasterId, B.Code CrBudgetCode , B.UserName CrBudgetName 
                                , IRD.ActivityId PostCrActivityId, A.Code CrActivityCode , A.UserName CrActivityName
								

								, BMM.GLGeneralInfoId PostDrGLGeneralInfoId, MGL.AccountCode DrAccountCode, MGL.UserName DrGLGeneralInfoName 
	                            , MM.BudgetMasterId PostDrBudgetMasterId, MB.Code DrBudgetCode , MB.UserName DrBudgetName 
                                , MM.ActivityId PostDrActivityId, MA.Code DrActivityCode , MA.UserName DrActivityName
								,FAB.FixedAssetMasterId,UoM.UserName UnitOfMeasurement,FAM.UserName AssetMasterName
									 FROM TRN.InventoryIssueHistory IIH 
									 JOIN TRN.InventoryIssueDetail IRD  ON IIH.InventoryIssueDetailId=IRD.Id
                                     LEFT JOIN TRN.InventoryIssue IR ON IR.Id=IRD.InventoryIssueId
									 LEFT JOIN TRN.InventoryReceiveDetail IVRD ON IVRD.Id=IIH.InventoryReceiveDetailId
									 LEFT JOIN TRN.InventoryReceive IVR ON IVR.Id=IVRD.InventoryReceiveId
                                     LEFT JOIN [HKP].[Party] AS P ON IVR.PartyId=P.Id

                                     LEFT JOIN [EmployeeInformation] AS EI ON IVR.EmployeeId=EI.SystemId
								LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
							    JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							    LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							    LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							    LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							    LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                     LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IRD.TransactionUoMId=UoM.Id
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON IRD.BudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON BM.GLGeneralInfoId=GL.Id
                        LEFT JOIN [HKP].[Activity] AS A ON IRD.ActivityId= A.Id
                                     JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                                     JOIN [SCS].[Currency] AS GC ON IVR.CurrencyId=GC.Id
									 LEFT JOIN HKP.FixedAssetMasterBudgetTag FAB ON FAB.BudgetMasterId=MM.BudgetMasterId
									 LEFT JOIN [MST].[BudgetMaster] AS BMM ON MM.BudgetMasterId= BMM.Id
                        LEFT JOIN [HKP].[Budget] AS MB ON BMM.BudgetId= MB.Id
						LEFT JOIN [HKP].[GLGeneralInfo] AS MGL ON BMM.GLGeneralInfoId=MGL.Id
                        LEFT JOIN [HKP].[Activity] AS MA ON MM.ActivityId= MA.Id
						left join MST.FixedAssetMaster FAM ON FAM.Id=FAB.FixedAssetMasterId

         WHERE IIH.IsCapitalize=0 AND IIH.CapitalizeVoucherDetailId IS NULL AND IR.IssueType='Capital' AND IRD.IsAsset=1    AND IR.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public IEnumerable<object> GetIssueInventoryAUCList(string plantId)
        {
            try
            {
                var sql = @"SELECT 0 Active,IR.Id IssueNo,IRD.Id InventoryIssueDetailId,IIH.Id InventoryIssueHistoryId,IVRD.Id InventoryReceiveDetailId,IVR.Id GRNNo,IVR.GateEntryNo,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END, IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName,
                                     FORMAT(IR.IssueDate,'dd-MMM-yyyy') IssueDate,  UoM.UserName AS TransactionUoM,CU.Code AS CurrencyCode
                                     , P.Code AS PartyCode, P.UserName AS PartyName
									 , FORMAT(IR.AddedDate,'dd-MMM-yyyy') EntryDate
                                     ,IR.IssueType,IRD.IsAsset
                                     , MGM.UserName AS MaterialGroupMasterName
									 , IRD.InventoryMaterialId
									  
									  ,ROUND(IRD.PolicyRate,4) BaseCurrencyRate
									  ,IIH.Qty TransactionQty
									  ,IIH.Qty BaseQty
									  ,IIH.Rate BaseCurrencyRate
									  ,ROUND(IIH.TotalMaterialBooksCurrencyAmount,4) Amount
						,IRD.BaseUOMId,IRD.TransactionUoMId
							, IM.MaterialMasterId, MM.UserName
                            , IM.ArticleId, ART.StandardName
                            , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                            , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                            , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                            , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                            , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                            , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue

							, IRD.PostDrGLGeneralInfoId PostCrGLGeneralInfoId , GL.AccountCode CrAccountCode, GL.UserName CrGLGeneralInfoName 
	                            , IRD.PostDrBudgetMasterId PostCrBudgetMasterId , B.Code CrBudgetCode , B.UserName CrBudgetName 
                                , IRD.PostDrActivityId PostCrActivityId , A.Code CrActivityCode , A.UserName CrActivityName
								
                                , NULL PostDrGLGeneralInfoId, NULL DrAccountCode, NULL DrGLGeneralInfoName 
	                            , NULL PostDrBudgetMasterId, NULL DrBudgetCode , NULL DrBudgetName 
                                , NULL PostDrActivityId, NULL DrActivityCode , NULL DrActivityName
								,NULL FixedAssetMasterId,NULL AssetMasterName

									 FROM TRN.InventoryIssueHistory IIH 
									 JOIN TRN.InventoryIssueDetail IRD  ON IIH.InventoryIssueDetailId=IRD.Id
                                     LEFT JOIN TRN.InventoryIssue IR ON IR.Id=IRD.InventoryIssueId
									 LEFT JOIN TRN.InventoryReceiveDetail IVRD ON IVRD.Id=IIH.InventoryReceiveDetailId
									 LEFT JOIN TRN.InventoryReceive IVR ON IVR.Id=IVRD.InventoryReceiveId
                                     LEFT JOIN [HKP].[Party] AS P ON IVR.PartyId=P.Id

                                     LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
								LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IRD.InventoryMaterialId
							    JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
							    LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
							    LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
							    LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
							    LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
							    LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
							    LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
							    LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                                     LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON IRD.TransactionUoMId=UoM.Id
									 LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON IRD.PostDrGLGeneralInfoId=GL.Id
                        LEFT JOIN [MST].[BudgetMaster] AS BM ON IRD.PostDrBudgetMasterId= BM.Id
                        LEFT JOIN [HKP].[Budget] AS B ON BM.BudgetId= B.Id
                        LEFT JOIN [HKP].[Activity] AS A ON IRD.PostDrActivityId= A.Id
                                     JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id

         WHERE IIH.IsCapitalize=0 AND IIH.CapitalizeVoucherDetailId IS NULL AND IR.IssueType='Capital' AND IRD.IsAsset=0 AND IR.VoucherId<>''   AND IR.PlantId='" + plantId + "'";
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

        #region Capitalize Asset Register
        public GridModel GetCapitalizeAssetItem(GridParameter parameters, string faType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT Flag=CAST(0 AS bit),VD.VoucherId,VD.Id VoucherDetailNo,v.VoucherNo,IIH.Id InventoryIssueHistoryId,Round((IIH.TotalAmount),4) Amount
                    ,Round((IIH.TotalMaterialBooksCurrencyAmount),4) FABaseAmount,LC.LCANo,LC.LCRef,PO.PurchaseLCId,IR.CurrencyId,II.CurrencyId BaseCurrencyId
                    ,FAM.Id FixedAssetMasterId, BM.GLGeneralInfoId, AGL.UserName AS AssetGLName, BM.GLGeneralInfoId AS AssetGLId, FAMT.BudgetMasterId
                                    ,IIH.Qty,IR.Id GRNNo,IR.GateEntryNo,IR.DocRefNo InvoiceNo
									,REPLACE(Convert(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS InvoiceDate
									,II.Id IssueNo
									,TUoM.UserName AS TransactionUoM,CU.Code CurrencyCode,GC.Code GRNCurrencyCode
									,REPLACE(Convert(VARCHAR(11), II.IssueDate, 106), ' ', '-') AS CapitalizeDate
									, BM.BudgetId, B.UserName AssetBudgetName,MM.UserName MaterialMasterName,MMA.StandardName ArticleStandardName
									, FAM.UserName AS AssetMasterName, FAM.AssetType, AC.UserName AS ActivityName
                                    , AC.Id ActivityId, BM.RefNo, FAC.UserName FixedAssetCategory, FASC.UserName AS FixedAssetSubCategory
									,P.UserName VendorName,EI.EmployeeCode+ ''+EI.EmployeeName EmployeeName,IR.PartyId,IM.MaterialMasterId,IM.ArticleId,IM.FirstCharacteristicsValueId
                                    ,REPLACE(Convert(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate,IRD.CountryId
                                    FROM TRN.VoucherDetail VD 
									JOIN TRN.Voucher V ON V.Id=VD.VoucherId
									LEFT JOIN  TRN.InventoryIssueHistory IIH ON IIH.CapitalizeVoucherDetailId=VD.Id
									LEFT JOIN  TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									LEFT JOIN  TRN.InventoryIssue II ON II.Id=IID.InventoryIssueId
									LEFT JOIN  TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
									LEFT JOIN  TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
                                    LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=IRD.POId
									LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
                                    LEFT JOIN  TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
									LEFT JOIN  TRN.GateEntry GE ON GE.Id=IR.GateEntryNo
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS AGL ON AGL.Id=VD.GLGeneralInfoId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS AC ON AC.Id=VD.ActivityId
                                    LEFT JOIN [HKP].[FixedAssetMasterBudgetTag] AS FAMT ON FAMT.BudgetMasterId=VD.BudgetMasterId
                                    LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=FAMT.FixedAssetMasterId
                                    LEFT JOIN [HKP].[FixedAssetCategory] FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                    LEFT JOIN [HKP].[FixedAssetSubCategory] FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
									LEFT JOIN [SCS].Currency CU ON CU.Id=II.CurrencyId
									LEFT JOIN [SCS].Currency GC ON GC.Id=IR.CurrencyId
									LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
									LEFT JOIN [HKP].[Party] AS P ON P.Id= IR.PartyId
									LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId= IR.EmployeeId
									LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=IM.MaterialMasterId
									LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id=IM.ArticleId
                                    WHERE V.SourceType='FixedAssetCapitalizeJournal' AND VD.FAType='" + faType + @"' AND IIH.IsRegister=0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetAUCCIExpenseData(string column, string value, string faType)
        {
            try
            {
                string CmdText = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                if (faType == "AUC")
                {
                    CmdText = @"SELECT * FROM (SELECT Flag=CAST(0 AS bit),IRD.Id InventoryReceiveDetailId,IR.VoucherId,IRD.VoucherDetailId,v.VoucherNo,Round((IRD.TotalMaterialBooksCurrencyAmount),4) Amount
                    ,Round((0),4) FABaseAmount,LC.LCANo,PO.PurchaseLCId,IR.CurrencyId, BM.GLGeneralInfoId, AGL.UserName AS AssetGLName, BM.GLGeneralInfoId AS AssetGLId
                                    ,TUoM.UserName AS BaseUoM,IRD.BaseQty,IR.Id GRNNo,IR.GateEntryNo,IR.DocRefNo InvoiceNo
									,REPLACE(Convert(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS InvoiceDate
									,GC.Code GRNCurrencyCode
									, BM.BudgetId, B.UserName AssetBudgetName,MM.UserName MaterialMasterName,MMA.StandardName ArticleStandardName, AC.UserName AS ActivityName
                                    , AC.Id ActivityId, BM.RefNo,P.UserName VendorName,EI.EmployeeCode+ ''+EI.EmployeeName EmployeeName,IR.PartyId,IM.MaterialMasterId,IM.ArticleId,IM.FirstCharacteristicsValueId
                                    ,REPLACE(Convert(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate,IRD.CountryId,FY.FiscalYearName
                                    FROM TRN.InventoryReceiveDetail IRD 
									LEFT JOIN  TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
									JOIN TRN.Voucher V ON V.Id=IR.VoucherId
                                    LEFT JOIN [SCS].[FiscalYear] FY ON FY.Id=V.FiscalYearId
                                    LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=IR.POId
									LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
                                    LEFT JOIN  TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
									LEFT JOIN  TRN.GateEntry GE ON GE.Id=IR.GateEntryNo
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=IRD.PostDrBudgetMasterId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS AGL ON AGL.Id=IRD.PostDrGLGeneralInfoId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS AC ON AC.Id=IRD.PostDrActivityId                
									LEFT JOIN [SCS].Currency GC ON GC.Id=IR.CurrencyId
									LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.BaseUOMId = TUoM.Id
									LEFT JOIN [HKP].[Party] AS P ON P.Id= IR.PartyId
									LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId= IR.EmployeeId
									LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=IM.MaterialMasterId
									LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id=IM.ArticleId
                                    Where IRD.IsAsset=1 AND  IR.VoucherId<>'' AND V.IsPark=0  
                                    AND IRD.Id NOT IN (select ISNULL(InventoryReceiveDetailId,'') from TRN.InventoryIssueHistory)
                                    AND IRD.Id NOT IN (Select ISNULL([InventoryReceiveDetailId],'') from [TRN].[CapitalizationMasterDetail])) AS TEMP WHERE " + strkey + "";
                }
                else if (faType == "CI")
                {
                    CmdText = @"SELECT * FROM (SELECT Flag=CAST(0 AS bit),VD.VoucherId,VD.Id VoucherDetailId,v.VoucherNo,IIH.Id InventoryIssueHistoryId,Round((IIH.TotalAmount),4) Amount
                    ,Round((IIH.TotalMaterialBooksCurrencyAmount),4) FABaseAmount,LC.LCANo,PO.PurchaseLCId,IR.CurrencyId,II.CurrencyId BaseCurrencyId
                    , BM.GLGeneralInfoId, AGL.UserName AS AssetGLName, BM.GLGeneralInfoId AS AssetGLId, VD.BudgetMasterId
                                    ,IIH.Qty,IR.Id GRNNo,IR.GateEntryNo,IR.DocRefNo InvoiceNo
									,REPLACE(Convert(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS InvoiceDate,II.Id IssueNo
									,TUoM.UserName AS TransactionUoM,CU.Code CurrencyCode,GC.Code GRNCurrencyCode
									,REPLACE(Convert(VARCHAR(11), II.IssueDate, 106), ' ', '-') AS CapitalizeDate
									, BM.BudgetId, B.UserName AssetBudgetName,MM.UserName MaterialMasterName,MMA.StandardName ArticleStandardName, AC.UserName AS ActivityName
                                    , AC.Id ActivityId, BM.RefNo,P.UserName VendorName,EI.EmployeeCode+ ''+EI.EmployeeName EmployeeName,IR.PartyId,IM.MaterialMasterId,IM.ArticleId,IM.FirstCharacteristicsValueId
                                    ,REPLACE(Convert(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate,IRD.CountryId,en.UserName Entity,CC.UserName CostCenter,IIH.InventoryReceiveDetailId
                                    ,FY.FiscalYearName
                                    FROM  TRN.InventoryIssueHistory IIH
									LEFT JOIN  TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
									LEFT JOIN  TRN.InventoryIssue II ON II.Id=IID.InventoryIssueId
									LEFT JOIN  TRN.VoucherDetail VD ON VD.Id=IID.DrVoucherDetailId
									JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                                    LEFT JOIN [SCS].[FiscalYear] FY ON FY.Id=V.FiscalYearId
									LEFT JOIN  TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
									LEFT JOIN  TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
                                    LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=IR.POId
									LEFT JOIN dbo.PurchaseLC LC ON LC.Id=PO.PurchaseLCId
                                    LEFT JOIN  TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
									LEFT JOIN  TRN.GateEntry GE ON GE.Id=IR.GateEntryNo
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS AGL ON AGL.Id=VD.GLGeneralInfoId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS AC ON AC.Id=VD.ActivityId
                                    
									LEFT JOIN [SCS].Currency CU ON CU.Id=II.CurrencyId
									LEFT JOIN [SCS].Currency GC ON GC.Id=IR.CurrencyId
									LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
									LEFT JOIN [HKP].[Party] AS P ON P.Id= IR.PartyId
									LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId= IR.EmployeeId
									LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=IM.MaterialMasterId
									LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id=IM.ArticleId
									LEFT JOIN ORG.Entity EN  ON EN.Id=II.EntityId
									LEFT JOIN [ORG].[CostCenter] CC ON CC.Id=iid.CostCenterId
                                    WHERE  II.IssueType='Capital' AND II.VoucherId<>'' AND V.IsPark=0 AND IIH.Id NOT IN (Select ISNULL([InventoryIssueHistoryId],'') from [TRN].[CapitalizationMasterDetail])) AS TEMP WHERE " + strkey + "";
                }
                else
                {
                    CmdText = @"SELECT top 1000 * FROM (SELECT Flag=CAST(0 AS bit),VD.VoucherId,VD.Id VoucherDetailId,v.VoucherNo,Round((VD.DrAmount),4) Amount
                    ,Round((0),4) FABaseAmount,V.CurrencyId TransactionCurrencyId, BM.GLGeneralInfoId, AGL.UserName AS AssetGLName, BM.GLGeneralInfoId AS AssetGLId
                                    ,0 Qty,'' GRNNo,V.DocRefNo InvoiceNo
									,REPLACE(Convert(VARCHAR(11), V.DocDate, 106), ' ', '-') AS InvoiceDate
									,GC.Code GRNCurrencyCode
									, BM.BudgetId, B.UserName AssetBudgetName
									, AC.UserName AS ActivityName
                                    , AC.Id ActivityId, BM.RefNo
									,P.UserName VendorName,EI.EmployeeCode+ ''+EI.EmployeeName EmployeeName,VD.PartyId
                                    ,REPLACE(Convert(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate,en.UserName Entity,CC.UserName CostCenter
                                    ,FY.FiscalYearName
                                    FROM TRN.VoucherDetail VD 
									JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                                    LEFT JOIN [SCS].[FiscalYear] FY ON FY.Id=VD.FiscalYearId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS AGL ON AGL.Id=VD.GLGeneralInfoId
									LEFT JOIN HKP.AccountGroup AG ON AG.Id=AGL.AccountGroupId
									LEFT JOIN HKP.AccountType ATY ON ATY.Id=AG.AccountTypeId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS AC ON AC.Id=VD.ActivityId
									LEFT JOIN [SCS].Currency GC ON GC.Id=V.CurrencyId
									LEFT JOIN [HKP].[Party] AS P ON P.Id= VD.PartyId
									LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId= VD.EmployeeId
									LEFT JOIN ORG.Entity EN  ON EN.Id=VD.EntityId
									LEFT JOIN [ORG].[CostCenter] CC ON CC.Id=VD.CostCenterId
                                   WHERE V.IsPark=0 AND VD.DrAmount>0 AND BM.IsCapital=1 
                                   AND V.Id NOT IN (select VoucherId from TRN.InventoryIssue where IssueType='Capital' AND VoucherId IS NOT NULL)
                                   AND VD.Id NOT IN (Select ISNULL([VoucherDetailId],'') from [TRN].[CapitalizationMasterDetail])) AS TEMP WHERE " + strkey + " order by FiscalYearName";
                }

                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetAUCCIExpenseReport(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {


                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Data";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;

                #region columns

                sheet[ROW, COL].Text = "Vendor"; sheet[ROW, COL].ColumnWidth = 16; int colVen = COL; COL++;
                sheet[ROW, COL].Text = "GRN Date"; sheet[ROW, COL].ColumnWidth = 16; int colGD = COL; COL++;
                sheet[ROW, COL].Text = "GRNNo"; sheet[ROW, COL].ColumnWidth = 16; int colGN = COL; COL++;
                sheet[ROW, COL].Text = "GRNRowId"; sheet[ROW, COL].ColumnWidth = 16; int colGRN = COL; COL++;
                sheet[ROW, COL].Text = "GL"; sheet[ROW, COL].ColumnWidth = 16; int colAssetGLName = COL; COL++;
                sheet[ROW, COL].Text = "Budget"; sheet[ROW, COL].ColumnWidth = 16; int colAssetBudgetName = COL; COL++;
                sheet[ROW, COL].Text = "Activity"; sheet[ROW, COL].ColumnWidth = 16; int colActivityName = COL; COL++;
                sheet[ROW, COL].Text = "Material"; sheet[ROW, COL].ColumnWidth = 16; int colM = COL; COL++;
                sheet[ROW, COL].Text = "Article"; sheet[ROW, COL].ColumnWidth = 16; int colA = COL; COL++;
                sheet[ROW, COL].Text = "UoM"; sheet[ROW, COL].ColumnWidth = 16; int colU = COL; COL++;
                sheet[ROW, COL].Text = "Qty"; sheet[ROW, COL].ColumnWidth = 16; int colQ = COL; COL++;
                sheet[ROW, COL].Text = "Amount"; sheet[ROW, COL].ColumnWidth = 16; int colAM = COL; COL++;
                sheet[ROW, COL].Text = "VoucherRowId"; sheet[ROW, COL].ColumnWidth = 16; int colVRI = COL; COL++;
                sheet[ROW, COL].Text = "VoucherNo"; sheet[ROW, COL].ColumnWidth = 16; int colV = COL; COL++;
                sheet[ROW, COL].Text = "Issue No"; sheet[ROW, COL].ColumnWidth = 16; int colIN = COL; COL++;
                sheet[ROW, COL].Text = "Issue Date"; sheet[ROW, COL].ColumnWidth = 16; int colID = COL; COL++;
                sheet[ROW, COL].Text = "Entity"; sheet[ROW, COL].ColumnWidth = 16; int colE = COL; COL++;
                sheet[ROW, COL].Text = "Cost Center"; sheet[ROW, COL].ColumnWidth = 16; int colCC = COL;


                #endregion columns

                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

                int startRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, colVen].Text = data.Rows[i]["VendorName"].ToString();
                    sheet[ROW, colGD].Text = data.Rows[i]["InvoiceDate"].ToString();
                    sheet[ROW, colGN].Text = data.Rows[i]["GRNNo"].ToString();
                    sheet[ROW, colGRN].Text = data.Rows[i]["InventoryReceiveDetailId"].ToString();
                    sheet[ROW, colAssetGLName].Text = data.Rows[i]["AssetGLName"].ToString();
                    sheet[ROW, colAssetBudgetName].Text = data.Rows[i]["AssetBudgetName"].ToString();
                    sheet[ROW, colActivityName].Text = data.Rows[i]["ActivityName"].ToString();
                    sheet[ROW, colM].Text = data.Rows[i]["MaterialMasterName"].ToString();
                    sheet[ROW, colA].Text = data.Rows[i]["ArticleStandardName"].ToString();
                    sheet[ROW, colU].Text = data.Rows[i]["BaseUoM"].ToString();
                    sheet[ROW, colQ].Text = data.Rows[i]["Qty"].ToString();
                    sheet[ROW, colAM].Text = data.Rows[i]["Amount"].ToString();
                    sheet[ROW, colVRI].Text = data.Rows[i]["VoucherDetailId"].ToString();
                    sheet[ROW, colV].Text = data.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, colIN].Text = data.Rows[i]["IssueNo"].ToString();
                    sheet[ROW, colID].Text = data.Rows[i]["CapitalizeDate"].ToString();
                    sheet[ROW, colE].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, colCC].Text = data.Rows[i]["CostCenter"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Asset Capitalization Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************

                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void InsertORUpdateCapitalizeAsset(FixedAssetRegister master, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, decimal NumberOfQuantity, string CompanyCurrencyCode
          , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
          , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, IEnumerable<FixedAssetRegisterDetail> fixedAssetRegisterDetail)
        {
            FixedAssetRegister localItem = null;
            masterid = string.Empty;
            #region-- Assign Amount
            decimal reFABaseAmountTotal = 0;
            decimal reFAGroupAmountTotal = 0;
            decimal reFAHardAmountTotal = 0;
            decimal reADBaseAmountTotal = 0;
            decimal reADGroupAmountTotal = 0;
            decimal reADHardAmountTotal = 0;
            decimal reTotalRow = 0;
            //-----
            decimal opFABaseAmountTotal = 0;
            decimal opFAGroupAmountTotal = 0;
            decimal opFAHardAmountTotal = 0;
            decimal opADBaseAmountTotal = 0;
            decimal opADGroupAmountTotal = 0;
            decimal opADHardAmountTotal = 0;
            decimal opTotalRow = 0;

            decimal remainingNoOfQuantity = 0;
            decimal tempTotalSubAssetAmount = 0;
            decimal tempTotalBooksAmount = 0;
            decimal tempTotalAmount = 0;
            decimal tempFAAmount = 0;
            decimal tempADAmount = 0;
            remainingNoOfQuantity = NumberOfQuantity;
            tempFAAmount = master.FABaseAmount;
            tempADAmount = master.ADBaseAmount;

            var savedOPInfo = GetCapitalizeAssetItemValue(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, master.CompanyId);
            var savedRegInfo = GetRegisterInfoWithFAMId(master.MaterialMasterId, null, null, master.CompanyId);
            foreach (var item in savedRegInfo)
            {
                var dic = (Dictionary<string, object>)item;
                reFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                reADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                reTotalRow = Convert.ToDecimal(dic["TotalRow"].ToString());
            }
            foreach (var item in savedOPInfo)
            {
                var dic = (Dictionary<string, object>)item;
                opFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                opADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                opTotalRow = Convert.ToDecimal(CheckNumIsExist(dic["TotalRow"].ToString()));
            }
            #endregion Insert n Update
            var _pk = GetPK();
            var _capitalizeRegisterId = GetCaptalizeRegisterPK();
            var _fixedAssetRegisterDetailId = GetFixedAssetRegisterDetailPK();
            var _subFixedAssetRegisterId = GetSubFixedAssetRegisterPK();


            var flag = false;
            var builder = new System.Text.StringBuilder();
            var value = "''";
            try
            {
                if (opTotalRow > 0 && reTotalRow + NumberOfQuantity > opTotalRow)
                {
                    throw new CustomException("Register quantity  must be below opening balance quantity");
                }
                if (NumberOfQuantity > 0)
                {
                    #region--Save Validation
                    if (master.IsFinancial)
                    {
                        var materialMaster = _materialMasterRepository.Find(master.MaterialMasterId);
                        if (null == materialMaster)
                            throw new CustomException("Material Master is null.");
                        if (null == materialMaster.BudgetMasterId)
                            throw new CustomException(materialMaster.UserName + " Budget Master not found.");
                        //var faTagData = _fixedAssetMasterBudgetTagRepository.Query().Select().FirstOrDefault();
                        var faTagData = _fixedAssetMasterBudgetTagRepository.Query(r => r.BudgetMasterId == materialMaster.BudgetMasterId).Select().FirstOrDefault();
                        if (null == faTagData)
                            throw new CustomException("Fixed Asset Master Tag data not found.");

                        //var checkACUD = _fixedAssetMasterGLepository.Query(r => r.FixedAssetMasterId == faTagData.FixedAssetMasterId).Select().FirstOrDefault();
                        var checkACUD = GetFixedAssetMasterGLData(faTagData.FixedAssetMasterId);
                        if (checkACUD == null)
                            throw new CustomException("This item is not configured with Accumulative Depreciation GL");
                        else
                        {
                            if (checkACUD["AccumulatedDepreciationGLId"].ToString() == null || checkACUD["AccumulatedDepreciationGLId"].ToString() == "")
                            {
                                //throw new CustomException(checkACUD.FixedAssetMaster.UserName + " is not configured with Accumulative Depreciation GL");
                                throw new CustomException(checkACUD["FixedAssetMasterName"].ToString() + " is not configured with Accumulative Depreciation GL");
                            }
                        }
                        if ((master.FABaseAmount * NumberOfQuantity + reFABaseAmountTotal) > opFABaseAmountTotal)
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        }
                        if ((master.FAGroupAmount * NumberOfQuantity + reFAGroupAmountTotal) > opFAGroupAmountTotal)
                        {
                            throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAGroupAmountTotal);
                        }
                        if ((master.FAHardAmount * NumberOfQuantity + reFAHardAmountTotal) > opFAHardAmountTotal)
                        {
                            throw new CustomException(HardCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAHardAmountTotal);
                        }
                        //---
                        //if ((master.ADBaseAmount * NumberOfQuantity + reADBaseAmountTotal) > opADBaseAmountTotal)
                        //{
                        //    throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADBaseAmountTotal);
                        //}
                        if ((master.ADGroupAmount * NumberOfQuantity + reADGroupAmountTotal) > opADGroupAmountTotal)
                        {
                            throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADGroupAmountTotal);
                        }

                        //master.FABaseAmount = Math.Round(master.FABaseAmount / NumberOfQuantity, 2);
                        master.ADBaseAmount = Math.Round(master.ADBaseAmount / NumberOfQuantity, 2);
                        //master.FABaseAmount = master.Price + subassestAmount;
                        if (subFixedAssetRegister != null)
                        {
                            tempTotalSubAssetAmount = subFixedAssetRegister.Sum(r => r.Amount);
                        }
                    }
                    // builder.Append(value);
                    int currentId = 0;
                    #endregion
                    for (int i = 0; i < NumberOfQuantity; i++)
                    {
                        var lc = localItem;
                        var asItem = assetItemValue;
                        var id = MakePK(_pk, i, 2);
                        if (master.Id == null)
                        {
                            master.CapitalizeRegisterNo = _capitalizeRegisterId;
                        }

                        if (i + 1 == NumberOfQuantity)
                        {
                            master.FABaseAmount = master.TotalPrice - tempTotalBooksAmount;
                            master.Price = master.TotalGRNAmount - (tempTotalAmount);
                            master.Quantity = 1;
                        }
                        else if (1 > remainingNoOfQuantity)
                        {
                            master.FABaseAmount = master.TotalPrice - tempTotalBooksAmount;
                            master.Price = master.TotalGRNAmount - (tempTotalAmount);
                            master.Quantity = remainingNoOfQuantity;
                        }
                        else
                        {
                            tempTotalAmount += master.Price;
                            master.FABaseAmount = Math.Round(master.TotalPrice / NumberOfQuantity, 2);
                            tempTotalBooksAmount += master.FABaseAmount;
                            master.Quantity = 1;
                            remainingNoOfQuantity -= 1;
                        }
                        master.FABudgetMasterId = assetBudgetId;
                        master.FAActivityId = assetActivityId;
                        InitInsert(id, master, out lc);
                        InsertOrUpdateGraph(lc);
                        //if(master.InventoryIssueHistoryId != null)
                        //{
                        //    var inventoryIssueHistory = _inventoryIssueHistoryRepository.Find(master.InventoryIssueHistoryId);
                        //    inventoryIssueHistory.IsRegister = true;
                        //    _inventoryIssueHistoryRepository.Update(inventoryIssueHistory);
                        //}


                        /*Sub Fixed Asset Resigter If have */

                        if (subFixedAssetRegister != null)
                        {

                            foreach (var item in subFixedAssetRegister)
                            {
                                if (string.IsNullOrEmpty(item.Id))
                                {
                                    currentId++;
                                    var subFAR = new SubFixedAssetRegister
                                    {
                                        Id = MakePK(_subFixedAssetRegisterId, currentId, 4),
                                        FixedAssetRegisterId = lc.Id,
                                        SubAssetTypeId = item.SubAssetTypeId,
                                        Amount = Math.Round(item.Amount / NumberOfQuantity, 2),
                                        CapitalizationRate = Math.Round(lc.FABaseAmount / lc.Price, 4)
                                    };
                                    subFAR.SubAssetTypeId = item.SubAssetTypeId;
                                    subFAR.CurrencyId = item.CurrencyId;
                                    subFAR.CapitalizationDate = item.CapitalizationDate;
                                    AuditService.AddedLog(subFAR);
                                    _subFixedAssetRegisterRepository.Insert(subFAR);
                                }
                                else
                                {
                                    var subFAR = new SubFixedAssetRegister
                                    {
                                        Id = item.Id,
                                        FixedAssetRegisterId = lc.Id,
                                        SubAssetTypeId = item.SubAssetTypeId,
                                        Amount = Math.Round(item.Amount / NumberOfQuantity, 2),
                                        CapitalizationRate = Math.Round(lc.FABaseAmount / lc.Price, 4)
                                    };
                                    subFAR.SubAssetTypeId = item.SubAssetTypeId;
                                    subFAR.CurrencyId = item.CurrencyId;
                                    subFAR.CapitalizationDate = item.CapitalizationDate;
                                    AuditService.UpdatedLog(subFAR);
                                    _subFixedAssetRegisterRepository.Update(subFAR);
                                }
                            }
                        }

                        //_assetItemValueService.InsertOrUpdateGraph(asItem, lc.Id);
                        _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(lc, fixedAssetRegisterSkuValue);
                        if (value == "''")
                        {
                            value = "'" + id + "'";
                            builder.Append(value);
                        }
                        else
                        { builder.Append(",'" + id + "'"); }

                    }

                }
                else
                {
                    localItem = GetItem(master.Id);
                    #region-- Update Validation
                    if (master.IsFinancial)
                    {
                        if (!((reFABaseAmountTotal - (localItem.FABaseAmount) + master.FABaseAmount) <= opFABaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        }
                        if (!((reFAGroupAmountTotal - localItem.FAGroupAmount + master.FAGroupAmount) <= opFAGroupAmountTotal))
                        {
                            throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAGroupAmountTotal);
                        }
                        if (!((reFAHardAmountTotal - localItem.FAHardAmount + master.FAHardAmount) <= opFAHardAmountTotal))
                        {
                            throw new CustomException(HardCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAHardAmountTotal);
                        }
                        if (!((reADBaseAmountTotal - localItem.ADBaseAmount + master.ADBaseAmount) <= opADBaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADBaseAmountTotal);
                        }
                        if (!((reADGroupAmountTotal - localItem.ADGroupAmount + master.ADGroupAmount) <= opADGroupAmountTotal))
                        {
                            throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADGroupAmountTotal);
                        }
                        if (!((reADHardAmountTotal - localItem.ADHardAmount + master.ADHardAmount) <= opADHardAmountTotal))
                        {
                            throw new CustomException(HardCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADHardAmountTotal);
                        }
                    }
                    #endregion
                    OutItem(_pk, master, ref localItem);
                    InsertOrUpdateGraph(localItem);
                    _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(master, fixedAssetRegisterSkuValue);
                    value = "'" + master.Id + "'";
                }
                if (fixedAssetRegisterDetail != null)
                {
                    int detailId = 0;

                    foreach (var item in fixedAssetRegisterDetail)
                    {
                        detailId++;
                        item.CapitalizeRegisterNo = _capitalizeRegisterId;
                        item.Id = MakePK(_fixedAssetRegisterDetailId, detailId, 4);
                        AuditService.AddedLog(item);
                        _fixedAssetRegisterDetailRepository.Insert(item);
                        var inventoryIssueHistory = _inventoryIssueHistoryRepository.Find(item.InventoryIssueHistoryId);
                        inventoryIssueHistory.IsRegister = true;
                        _inventoryIssueHistoryRepository.Update(inventoryIssueHistory);
                    }
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                masterid = builder.ToString();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, master.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        private Dictionary<string, object> GetFixedAssetMasterGLData(string FixedAssetMasterId)
        {
            var cmdText = @"select FAGL.*,FAM.UserName FixedAssetMasterName from
                            [HKP].[FixedAssetMasterGL] FAGL
                            LEFT JOIN [MST].[FixedAssetMaster] FAM ON FAM.Id=FAGL.FixedAssetMasterId
                            where FAGL.FixedAssetMasterId = '" + FixedAssetMasterId.ToString() + @"'";
            return _sqlRepository.GetData(cmdText);
        }

        #endregion
        #region Non Asset Register
        public IEnumerable<object> GetNonAssetItem(string faType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT  II.Id IssueNo,VIS.VoucherNo IssueVoucherNo,REPLACE(Convert(VARCHAR(11), II.IssueDate, 106), ' ', '-') AS IssueDate
                                    ,REPLACE(Convert(VARCHAR(11), vis.PostingDate, 106), ' ', '-') AS IssuePostingDate,VIS.Id IssueVoucherId
                                    ,REPLACE(Convert(VARCHAR(11), vis.VoucherDate, 106), ' ', '-') AS IssueVoucherDate,VD.VoucherId
                                    ,v.VoucherNo CapitalizeVoucherNo,REPLACE(Convert(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS CapitalizePostingDate,IIH.Id InventoryIssueHistoryId
                                    ,IR.Id GRNNo,IR.GateEntryNo,IR.DocRefNo InvoiceNo,REPLACE(Convert(VARCHAR(11), IR.DocDate, 106), ' ', '-') AS InvoiceDate
                                    , BM.GLGeneralInfoId, AGL.UserName AS AssetGLName, B.UserName AssetBudgetName, AC.UserName AS ActivityName
                                    , BM.GLGeneralInfoId AS AssetGLId, FAMT.BudgetMasterId, BM.BudgetId,MM.UserName MaterialMasterName,MMA.StandardName ArticleStandardName
                                    , FAM.UserName AS AssetMasterName,FAM.Id FixedAssetMasterId, FAM.AssetType,Round((IIH.TotalMaterialBooksCurrencyAmount),4) Amount
                                    ,IIH.Qty,II.CurrencyId,TUoM.UserName AS TransactionUoM,CU.Code CurrencyCode,V.CurrencyId
                                    , AC.Id ActivityId, BM.RefNo, FAC.UserName FixedAssetCategory, FASC.UserName AS FixedAssetSubCategory
                                    ,P.UserName VendorName,EI.EmployeeCode+ ''+EI.EmployeeName EmployeeName,IR.PartyId,IM.MaterialMasterId,IM.ArticleId,IM.FirstCharacteristicsValueId
                                    ,REPLACE(Convert(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate,IRD.CountryId
                                    FROM TRN.VoucherDetail VD 
                                    JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                                    LEFT JOIN  TRN.InventoryIssueHistory IIH ON IIH.CapitalizeVoucherDetailId=VD.Id
                                    LEFT JOIN  TRN.InventoryIssueDetail IID ON IID.Id=IIH.InventoryIssueDetailId
                                    LEFT JOIN  TRN.InventoryIssue II ON II.CapitalizeVoucherId=v.Id
                                    LEFT JOIN  TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
                                    LEFT JOIN  TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
                                    LEFT JOIN  TRN.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                                    LEFT JOIN  TRN.GateEntry GE ON GE.Id=IR.GateEntryNo
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS AGL ON AGL.Id=VD.GLGeneralInfoId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS AC ON AC.Id=VD.ActivityId
                                    LEFT JOIN [HKP].[FixedAssetMasterBudgetTag] AS FAMT ON FAMT.BudgetMasterId=VD.BudgetMasterId
                                    LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=FAMT.FixedAssetMasterId
                                    LEFT JOIN [HKP].[FixedAssetCategory] FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                    LEFT JOIN [HKP].[FixedAssetSubCategory] FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                                    LEFT JOIN [SCS].Currency CU ON CU.Id=II.CurrencyId
                                    LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId = TUoM.Id
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id= IR.PartyId
                                    LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId= IR.EmployeeId
                                    LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=IM.MaterialMasterId
                                    LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id=IM.ArticleId
                                    LEFT JOIN TRN.Voucher VIS ON VIS.Id=II.VoucherId
                                    WHERE V.SourceType='FixedAssetCapitalizeJournal' AND VD.FAType='" + faType + @"' AND IIH.IsRegister=0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetAssetRegisterItemForSubAsset()
        {
            try
            {
                // have to modified. search option needed.
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT top(5000)  FAR.Id FixedAssetRegisterId,MM.UserName MaterialMasterName,MMA.StandardName ArticleStandardName
                    , FAM.UserName AS AssetMasterName,FAR.Price,FAR.CapitalizeRegisterNo,FAR.InvoiceNo,0 Active,0 Amount,NULL CapitalizationDate,NULL SubAssetTypeId 
                    FROM TRN.FixedAssetRegister FAR 
                    LEFT JOIN [MST].[MaterialMaster] MM ON FAR.MaterialMasterId=MM.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id=FAR.MaterialMasterArticleId
                    LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=FAR.FixedAssetMasterId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertORUpdateCapitalizeNonAsset(FixedAssetRegister master, IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, int NumberOfQuantity, string CompanyCurrencyCode
         , string CompanyGroupCurrencyCode, string HardCurrencyCode, out string masterid, IEnumerable<MaterialMasterMachineProcess> assetItemValue, IEnumerable<FixedAssetRegisterCharacteristicsValue> fixedAssetRegisterSkuValue
         , string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, IEnumerable<FixedAssetRegisterDetail> fixedAssetRegisterDetail)
        {
            FixedAssetRegister localItem = null;
            masterid = string.Empty;
            #region-- Assign Amount
            decimal reFABaseAmountTotal = 0;
            decimal reFAGroupAmountTotal = 0;
            decimal reFAHardAmountTotal = 0;
            decimal reADBaseAmountTotal = 0;
            decimal reADGroupAmountTotal = 0;
            decimal reADHardAmountTotal = 0;
            decimal reTotalRow = 0;
            //-----
            decimal opFABaseAmountTotal = 0;
            decimal opFAGroupAmountTotal = 0;
            decimal opFAHardAmountTotal = 0;
            decimal opADBaseAmountTotal = 0;
            decimal opADGroupAmountTotal = 0;
            decimal opADHardAmountTotal = 0;
            decimal opTotalRow = 0;
            var savedOPInfo = GetCapitalizeAssetItemValue(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, master.CompanyId);

            var savedRegInfo = GetRegisterInfoWithFAMId(master.MaterialMasterId, null, null, master.CompanyId);
            foreach (var item in savedRegInfo)
            {
                var dic = (Dictionary<string, object>)item;
                reFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                reADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                reTotalRow = Convert.ToDecimal(dic["TotalRow"].ToString());
            }
            foreach (var item in savedOPInfo)
            {
                var dic = (Dictionary<string, object>)item;
                opFABaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["FABaseAmountTotal"].ToString()));
                opADBaseAmountTotal = Convert.ToDecimal(CheckNumIsExist(dic["ADBaseAmountTotal"].ToString()));
                opTotalRow = Convert.ToDecimal(CheckNumIsExist(dic["TotalRow"].ToString()));
            }
            #endregion Insert n Update
            var _pk = GetPK();
            var _capitalizeRegisterId = GetCaptalizeRegisterPK();
            var _fixedAssetRegisterDetailId = GetFixedAssetRegisterDetailPK();
            var _subFixedAssetRegisterId = GetSubFixedAssetRegisterPK();
            var flag = false;
            var builder = new System.Text.StringBuilder();
            var value = "''";
            try
            {
                if (opTotalRow > 0 && reTotalRow + NumberOfQuantity > opTotalRow)
                {
                    throw new CustomException("Register quantity  must be below opening balance quantity");
                }
                if (NumberOfQuantity > 0)
                {
                    #region--Save Validation
                    if (master.IsFinancial)
                    {
                        var materialMaster = _materialMasterRepository.Find(master.MaterialMasterId);
                        if (null == materialMaster)
                            throw new CustomException("Material Master is null.");
                        var faTagData = _fixedAssetMasterBudgetTagRepository.Query().Select().FirstOrDefault();
                        //var faTagData = _fixedAssetMasterBudgetTagRepository.Query(r => r.BudgetMasterId == materialMaster.BudgetMasterId).Select().FirstOrDefault();
                        if (null == faTagData)
                            throw new CustomException("Fixed Asset Master Tag data not found.");

                        var checkACUD = _fixedAssetMasterGLepository.Query(r => r.FixedAssetMasterId == faTagData.FixedAssetMasterId).Select().FirstOrDefault();
                        if (checkACUD == null)
                            throw new CustomException("This item is not configured with Accumulative Depreciation GL");
                        else
                        {
                            if (checkACUD.AccumulatedDepreciationGLId == null)
                            {
                                throw new CustomException(checkACUD.FixedAssetMaster.UserName + " is not configured with Accumulative Depreciation GL");
                            }
                        }
                        //if ((master.FABaseAmount * NumberOfQuantity + reFABaseAmountTotal) > opFABaseAmountTotal)
                        //{
                        //    throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        //}
                        //if ((master.FAGroupAmount * NumberOfQuantity + reFAGroupAmountTotal) > opFAGroupAmountTotal)
                        //{
                        //    throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAGroupAmountTotal);
                        //}
                        //if ((master.FAHardAmount * NumberOfQuantity + reFAHardAmountTotal) > opFAHardAmountTotal)
                        //{
                        //    throw new CustomException(HardCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAHardAmountTotal);
                        //}
                        //---
                        //if ((master.ADBaseAmount * NumberOfQuantity + reADBaseAmountTotal) > opADBaseAmountTotal)
                        //{
                        //    throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADBaseAmountTotal);
                        //}
                        //if ((master.ADGroupAmount * NumberOfQuantity + reADGroupAmountTotal) > opADGroupAmountTotal)
                        //{
                        //    throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADGroupAmountTotal);
                        //}
                        //if ((master.ADHardAmount * NumberOfQuantity + reADHardAmountTotal) > opADHardAmountTotal)
                        //{
                        //    throw new CustomException(HardCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADHardAmountTotal);
                        //}
                    }
                    // builder.Append(value);
                    #endregion
                    for (int i = 0; i < NumberOfQuantity; i++)
                    {
                        var lc = localItem;
                        var asItem = assetItemValue;
                        var id = MakePK(_pk, i, 2);
                        master.CapitalizeRegisterNo = _capitalizeRegisterId;
                        InitInsert(id, master, out lc);
                        InsertOrUpdateGraph(lc);

                        /*Sub Fixed Asset Resigter If have */
                        if (subFixedAssetRegister != null)
                        {
                            int currentId = 0;
                            foreach (var item in subFixedAssetRegister)
                            {
                                if (string.IsNullOrEmpty(item.Id))
                                {
                                    currentId++;
                                    var subFAR = new SubFixedAssetRegister
                                    {
                                        Id = MakePK(_subFixedAssetRegisterId, currentId, 4),
                                        Amount = item.Amount,
                                        FixedAssetRegisterId = lc.Id,
                                        SubAssetTypeId = item.SubAssetTypeId
                                    };
                                    AuditService.AddedLog(subFAR);
                                    _subFixedAssetRegisterRepository.Insert(subFAR);
                                }
                                else
                                {
                                    var subFAR = new SubFixedAssetRegister
                                    {
                                        Amount = item.Amount,
                                        Id = item.Id,
                                        FixedAssetRegisterId = lc.Id,
                                        SubAssetTypeId = item.SubAssetTypeId
                                    };
                                    AuditService.UpdatedLog(subFAR);
                                    _subFixedAssetRegisterRepository.Update(subFAR);
                                }
                            }
                        }

                        //_assetItemValueService.InsertOrUpdateGraph(asItem, lc.Id);
                        _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(lc, fixedAssetRegisterSkuValue);
                        if (value == "''")
                        {
                            value = "'" + id + "'";
                            builder.Append(value);
                        }
                        else
                        { builder.Append(",'" + id + "'"); }
                    }

                }
                else
                {
                    localItem = GetItem(master.Id);
                    #region-- Update Validation
                    if (master.IsFinancial)
                    {
                        if (!((reFABaseAmountTotal - (localItem.FABaseAmount) + master.FABaseAmount) <= opFABaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFABaseAmountTotal);
                        }
                        if (!((reFAGroupAmountTotal - localItem.FAGroupAmount + master.FAGroupAmount) <= opFAGroupAmountTotal))
                        {
                            throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAGroupAmountTotal);
                        }
                        if (!((reFAHardAmountTotal - localItem.FAHardAmount + master.FAHardAmount) <= opFAHardAmountTotal))
                        {
                            throw new CustomException(HardCurrencyCode + " register amount  can't be greater then opening balance amount : " + opFAHardAmountTotal);
                        }
                        if (!((reADBaseAmountTotal - localItem.ADBaseAmount + master.ADBaseAmount) <= opADBaseAmountTotal))
                        {
                            throw new CustomException(CompanyCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADBaseAmountTotal);
                        }
                        if (!((reADGroupAmountTotal - localItem.ADGroupAmount + master.ADGroupAmount) <= opADGroupAmountTotal))
                        {
                            throw new CustomException(CompanyGroupCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADGroupAmountTotal);
                        }
                        if (!((reADHardAmountTotal - localItem.ADHardAmount + master.ADHardAmount) <= opADHardAmountTotal))
                        {
                            throw new CustomException(HardCurrencyCode + " register amount  can't be greater then opening balance amount : " + opADHardAmountTotal);
                        }
                    }
                    #endregion
                    OutItem(_pk, master, ref localItem);
                    InsertOrUpdateGraph(localItem);
                    _fixedAssetRegisterSkuValueService.InsertOrUpdateGraph(master, fixedAssetRegisterSkuValue);
                    value = "'" + master.Id + "'";
                }
                if (fixedAssetRegisterDetail != null)
                {
                    int detailId = 0;

                    foreach (var item in fixedAssetRegisterDetail)
                    {
                        detailId++;
                        item.CapitalizeRegisterNo = _capitalizeRegisterId;
                        item.Id = MakePK(_fixedAssetRegisterDetailId, detailId, 4);
                        AuditService.AddedLog(item);
                        _fixedAssetRegisterDetailRepository.Insert(item);
                        if (!string.IsNullOrWhiteSpace(item.InventoryIssueHistoryId))
                        {
                            var inventoryIssueHistory = _inventoryIssueHistoryRepository.Find(item.InventoryIssueHistoryId);
                            inventoryIssueHistory.IsRegister = true;
                            _inventoryIssueHistoryRepository.Update(inventoryIssueHistory);
                        }

                    }
                }
                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

                masterid = builder.ToString();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, master.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.FixedAsset.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }


        public void InsertORUpdateCapitalizeSubAsset(IEnumerable<SubFixedAssetRegister> subFixedAssetRegister, IEnumerable<FixedAssetRegisterDetail> fixedAssetRegisterDetail)
        {
            var flag = false;
            try
            {
                var _capitalizeRegisterId = GetCaptalizeRegisterPK();
                var _subFixedAssetRegisterId = GetSubFixedAssetRegisterPK();
                var _fixedAssetRegisterDetailId = GetFixedAssetRegisterDetailPK();
                int capitalizeId = 0;
                int fixedAssetRegisterDetailId = 0;
                /*Sub Fixed Asset Resigter If have */
                if (subFixedAssetRegister != null)
                {
                    foreach (var item in subFixedAssetRegister)
                    {
                        capitalizeId++;
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            var subFAR = new SubFixedAssetRegister
                            {
                                Id = MakePK(_subFixedAssetRegisterId, capitalizeId, 4),
                                Amount = Math.Round(item.Amount, 2),
                                FixedAssetRegisterId = item.FixedAssetRegisterId,
                                SubAssetTypeId = item.SubAssetTypeId,
                                CapitalizeRegisterNo = _capitalizeRegisterId,
                                CapitalizationDate = item.CapitalizationDate
                            };
                            AuditService.AddedLog(subFAR);
                            _subFixedAssetRegisterRepository.Insert(subFAR);
                        }
                        else
                        {
                            var subFAR = new SubFixedAssetRegister
                            {
                                Amount = Math.Round(item.Amount, 2),
                                Id = item.Id,
                                FixedAssetRegisterId = item.FixedAssetRegisterId,
                                SubAssetTypeId = item.SubAssetTypeId
                            };
                            AuditService.UpdatedLog(subFAR);
                            _subFixedAssetRegisterRepository.Update(subFAR);
                        }
                    }
                }


                if (fixedAssetRegisterDetail != null)
                {
                    foreach (var item in fixedAssetRegisterDetail)
                    {
                        fixedAssetRegisterDetailId++;
                        item.CapitalizeRegisterNo = _capitalizeRegisterId;
                        item.Id = MakePK(_fixedAssetRegisterDetailId, fixedAssetRegisterDetailId, 4);
                        AuditService.AddedLog(item);
                        _fixedAssetRegisterDetailRepository.Insert(item);
                        if (!string.IsNullOrEmpty(item.InventoryIssueHistoryId))
                        {
                            var inventoryIssueHistory = _inventoryIssueHistoryRepository.Find(item.InventoryIssueHistoryId);
                            inventoryIssueHistory.IsRegister = true;
                            _inventoryIssueHistoryRepository.Update(inventoryIssueHistory);
                        }

                    }
                }
                _unitOfWork.BeginTransaction();
                flag = true;
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

        #endregion
        #region Expenses Register
        public IEnumerable<object> GetExpensesRegisterItem(string faType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"SELECT  VD.VoucherId
                                    ,v.VoucherNo CapitalizeVoucherNo,REPLACE(Convert(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS CapitalizePostingDate
                                    , BM.GLGeneralInfoId, AGL.UserName AS AssetGLName, B.UserName AssetBudgetName, AC.UserName AS ActivityName
                                    , BM.GLGeneralInfoId AS AssetGLId, FAMT.BudgetMasterId, BM.BudgetId
                                    , FAM.UserName AS AssetMasterName,FAM.Id FixedAssetMasterId, FAM.AssetType,VD.DrAmount Amount
                                    ,CU.Code CurrencyCode,V.CurrencyId
                                    , AC.Id ActivityId, BM.RefNo, FAC.UserName FixedAssetCategory, FASC.UserName AS FixedAssetSubCategory
                                    ,REPLACE(Convert(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                                    FROM TRN.VoucherDetail VD 
                                    JOIN TRN.Voucher V ON V.Id=VD.VoucherId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=VD.BudgetMasterId
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS AGL ON AGL.Id=VD.GLGeneralInfoId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS AC ON AC.Id=VD.ActivityId
                                    LEFT JOIN [HKP].[FixedAssetMasterBudgetTag] AS FAMT ON FAMT.BudgetMasterId=VD.BudgetMasterId
                                    LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=FAMT.FixedAssetMasterId
                                    LEFT JOIN [HKP].[FixedAssetCategory] FAC ON FAM.FixedAssetCategoryId=FAC.Id
                                    LEFT JOIN [HKP].[FixedAssetSubCategory] FASC ON FAM.FixedAssetSubCategoryId=FASC.Id
                                    LEFT JOIN [SCS].Currency CU ON CU.Id=V.CurrencyId
                                    WHERE V.SourceType='ExpensesCapitalizeJournal' AND VD.FAType='" + faType + @"' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
        #region Fixed Asset Lost

        #endregion
        public IEnumerable<object> GetCapitalizeAssetItemValue(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId)
        {
            var accDepGL = _fixedAssetMasterGLepository.Query(r => r.FixedAssetMasterId == fixedAssetMasterId).Select().FirstOrDefault();

            return GetCapitalizeAssetItemValueData(fixedAssetMasterId, assetGLId, assetBudgetId, assetActivityId, companyId, accDepGL.AccumulatedDepreciationGLId, accDepGL.AccumulatedDepreciationBudgetMasterId, accDepGL.AccumulatedDepreciationActivityId);
        }
        private DataTable GetRegisterReportData(string companyGroupId, string companyId, string plantId, string MaterialMasterId, string MaterialMasterArticleId, string fixedAssetMasterId, string vendorId)
        {
            var sql = "";

            //if (PartyType == "All")
            //{
            sql = @"SELECT FR.SerialNo, FR.Id AssetNo,  e.UserName Entity, D.UserName Department, FR.Model
                , FR.InvoiceNo, MM.UserName MaterialMasterName, MMA.StandardName Article,FR.[Description]
                , FAM.UserName FixedAssetMasterName, FAC.UserName FixedAssetCategory
                --, FASC.UserName FixedAssetSubCategory, FAM.FixedAssetCategoryId
                --, FAM.FixedAssetSubCategoryId, FAM.AssetType
				,PC.Code PurchaseCurrency
				,BC.Code BaseCurrency
				,FR.Quantity
                ,isnull( FR.Price,0 )PurchasePrice
				,isnull( FR.FABaseAmount,0)FABaseAmount
				,ISNULL(SAR.SubAssetAmount,0) SubAssetBaseAmount

				,isnull (FR.FABaseAmount,0) + (ISNULL(SAR.SubAssetAmount,0)) TotalBaseAmount
				,ISNULL(FR.ADBaseAmount,0) + ISNULL(FADP.FixedAssetDepreciationAmount,0)  ADBaseAmount
				,ISNULL(FR.FABaseAmount,0) + isnull(SAR.SubAssetAmount,0) - ISNULL(FR.ADBaseAmount,0) - ISNULL(FADP.FixedAssetDepreciationAmount,0) NetFixedAssetsBaseAmount

                ,OpeningBalance = case when fr.IsOpeningBalance = 1 then 'YES' else 'NO' end
                ,format( fr.CapitalizationDate, 'dd-MMM-yyyy') CapitalizationDate
                --, FR.IsFinanciali
                ,P.UserName VendorName
                ,FR.[LifeTime]
                ,C.UserName OriginName
                ,FR.YearOfInstallation,FR.Id,FR.Id AS FixedAssetRegisterId, FR.MaterialMasterArticleId, FR.MaterialMasterId
                ,IR.Id GRNNo,IR.POId PONo , FADR.Description DepreciationRules
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
				left join scs.Currency PC on PC.Id= FR.CurrencyId
				left join scs.Currency BC on BC.Id= FR.FABaseCurrencyId
				left join mst.FixedAssetDepreciationRule FADR ON FADR.Id = FR.DepreciationRuleId

	
                LEFT JOIN(SELECT FixedAssetRegisterId,sum(isnull( Amount * CapitalizationRate,0)) SubAssetAmount 
				FROM TRN.SubFixedAssetRegister 
				group by FixedAssetRegisterId)SAR ON SAR.FixedAssetRegisterId =FR.Id

               left join ORG.Entity E on E.Id= FR.EntityId
			   left join ORG.Department D on D.Id = FR.DepartmentId
               LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
                --WHERE FR.CompanyId='" + companyId + @"' and FR.Archive=0 and FR.IsAUC=0
               -- AND FR.Id NOT IN(' ')

                     WHERE FR.CompanyGroupId='" + companyGroupId + "'and FR.CompanyId='" + companyId + "' AND FR.PlantId='" + plantId + @"'
                                    and FR.Archive=0 and FR.IsAUC=0 AND FR.Status IS NULL
                                    AND FR.Id NOT IN(' ')
				                     and FR.MaterialMasterId in(" + MaterialMasterId + ") AND FR.MaterialMasterArticleId in (" + MaterialMasterArticleId + ") AND FR.FixedAssetMasterId in (" + fixedAssetMasterId + @")
					                 and FR.VendorId in (" + vendorId + @") 
                                     --AND MM.IsAsset in ()  ";
            return _sqlRepository.GetDataTable(sql);
        }
        private DataTable GetRegisterDisposedReportData(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string nonPosted, string posted, string DisposeStatus)
        {
            var sql = "";
            var Posted = 0;
            if (nonPosted == "true")
            {
                Posted = 1;
            }
            if (posted == "true")
            {
                Posted = 0;
            }
            if (posted == "true" && nonPosted == "true")
            {
                Posted = 2;
            }
            //if (PartyType == "All")
            //{
            sql = @"SELECT FR.SerialNo, FR.Id AssetNo,  e.UserName Entity, D.UserName Department, FR.Model
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
                LEFT JOIN (select SUM(CurrentDepreciationAmount)FixedAssetDepreciationAmount,FixedAssetRegisterId from [TRN].[FixedAssetDepreciationProcess] GROUP BY  FixedAssetRegisterId) FADP ON FADP.FixedAssetRegisterId=FR.Id
               left join ORG.Entity E on E.Id= FR.EntityId
			   left join ORG.Department D on D.Id = FR.DepartmentId
                --WHERE FR.CompanyId='" + companyId + @"' and FR.Archive=0 and FR.IsAUC=0
               -- AND FR.Id NOT IN(' ')

                     WHERE FR.CompanyGroupId='" + companyGroupId + "'and FR.CompanyId='" + companyId + "' AND FR.PlantId='" + plantId + @"'
                                    and FR.Archive=0 and FR.IsAUC=0 AND FR.Status IS NOT NULL
                                    AND FR.Id NOT IN(' ')
                                    AND FR.Status in (" + DisposeStatus + @") 
				                    AND fard.IsPark=case when  " + Posted + @"=2 then fard.IsPark else " + Posted + @" end
				                    AND convert(Date,fard.DocDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 
				                     
                                     --AND MM.IsAsset in ()  ";
            return _sqlRepository.GetDataTable(sql);
        }
        private IEnumerable<object> GetCapitalizeAssetItemValueData(string fixedAssetMasterId, string assetGLId, string assetBudgetId, string assetActivityId, string companyId, string accDepGL, string accDepBudgetId, string accDepActivityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"SELECT SUM(x.TOTALROW) TotalRow,SUM(x.FABaseAmountTotal)FABaseAmountTotal,SUM(X.ADBaseAmountTotal)ADBaseAmountTotal
                        FROM (
					SELECT IIH.Qty AS TotalRow,CC.CompanyCurrencyAmount AS FABaseAmountTotal,FOBD.FixedAssetMasterId,FOBD.FAType,
					FOBD.Id , 0 ADBaseAmountTotal
						   FROM [TRN].[VoucherDetail] AS FOBD
                        INNER JOIN TRN.InventoryIssueHistory IIH ON IIH.CapitalizeVoucherDetailId=FOBD.Id
                        INNER JOIN [TRN].[Voucher] AS FOB ON FOB.Id=FOBD.VoucherId
                        INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.GLGeneralInfoId
                        INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
				 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
				 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.DrAmount AS CompanyCurrencyAmount, OBDC.VoucherDetailId
				 FROM [TRN].[VoucherDetailCurrency] AS OBDC
				 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
				 WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
				) AS CC ON CC.VoucherDetailId=FOBD.Id

				WHERE  FOBD.FAType='AssetCapatalized'
				AND --FOBD.FixedAssetMasterId='" + fixedAssetMasterId + @"' AND 
                FOBD.GLGeneralInfoId='" + assetGLId + "' AND FOBD.BudgetMasterId='" + assetBudgetId + "' AND FOBD.ActivityId='" + assetActivityId + @"'
			    AND FOB.IsPark=0 AND FOBD.OpeningBalanceDetailId IS NULL
                UNION
					 SELECT  0 AS TotalRow,0 FABaseAmountTotal,null FixedAssetMasterId,null FAType, 
					 FOBD.Id,CC.CompanyCurrencyAmount AS ADBaseAmountTotal
				FROM [TRN].[VoucherDetail] AS FOBD
				INNER JOIN [TRN].[Voucher] AS FOB ON FOB.Id=FOBD.VoucherId
				INNER JOIN [HKP].[GLGeneralInfo] AS GGI ON GGI.Id=FOBD.GLGeneralInfoId
				INNER JOIN [SCS].[Currency] AS C ON C.Id=FOBD.CurrencyId
				LEFT OUTER JOIN (
					 SELECT OBDC.ParallelCurrencyId AS CompanyCurrencyId, OBDC.FromCurrencyId AS CompanyFromCurrencyId, OBDC.ToCurrencyId AS CompanyToCurrencyId,
					 OBDC.ToCurrencyRate AS CompanyCurrencyRate, OBDC.CrAmount AS CompanyCurrencyAmount, OBDC.VoucherDetailId
					 FROM [TRN].[VoucherDetailCurrency] AS OBDC
					 INNER JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=OBDC.ParallelCurrencyId
					 INNER JOIN [TRN].[VoucherDetail] AS OBD ON OBD.Id=OBDC.VoucherDetailId
					WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='" + companyId + @"' 
					) AS CC ON CC.VoucherDetailId=FOBD.Id
					WHERE FOBD.FAType='AccDept'
				AND --FOBD.FixedAssetMasterId='" + fixedAssetMasterId + @"' AND 
                FOBD.GLGeneralInfoId='" + accDepGL + "' AND FOBD.BudgetMasterId='" + accDepBudgetId + "' AND FOBD.ActivityId='" + accDepActivityId + @"'
				AND FOB.IsPark=0 AND FOBD.OpeningBalanceDetailId IS NULL
                ) X";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public IWorkbook FixedAssetRegisterList(string companyGroupId, string companyId, string plantId, string MaterialMasterId, string MaterialMasterArticleId, string fixedAssetMasterId, string vendorId)
        {

            //Start EmployeeAdvanceDueList


            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtGatenntryRegisterList = GetRegisterReportData(companyGroupId, companyId, plantId, MaterialMasterId, MaterialMasterArticleId, fixedAssetMasterId, vendorId);


            if (dtGatenntryRegisterList.Rows.Count == 0)
                throw new Exception("No data found");
            // throw new Exception("To date must be above or equal to From Date.");

            worksheet.Name = "FixedAssetsRegisterReport";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            // worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            // worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //  ROW++;

            worksheet[ROW, COL].Text = "SerialNo";
            int colSerialNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "AssetNo";
            int colAssetNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;



            worksheet[ROW, COL].Text = "Entity";
            int colEntity = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Department";
            int colDepartment = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Model";
            int colModel = COL;
            worksheet[ROW, COL].ColumnWidth = 16;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Capitalization Date";
            int colCapitalizationDate = COL;
            worksheet[ROW, COL].ColumnWidth = 14;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Fixed Asset Master";
            int colFixedAssetMasterName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Material Master";
            int colMaterialMasterName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Article";
            int colArticle = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Description";
            int colDescription = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Depreciation Rules";
            int colDepreciationRules = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Pur. Currency";
            int colPurchaseCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Quantity";
            int colQuantity = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Purchase Price";
            int colPurchasePrice = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            worksheet[ROW, COL].Text = "Base Currency";
            int colBaseCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "FA Base Amount";
            int colFABaseAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "SubAsset Base Amount";
            int colSubAssetAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Total Base Amount";
            int colTotalAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "AD Base Amount";
            int colADBaseAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;

            COL++;
            worksheet[ROW, COL].Text = "Net FABase Amount";
            int colNetFixedAssetsBaseAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Invoice No.";
            int colInvoiceNo = COL;
            worksheet[ROW, COL].ColumnWidth = 17;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN No.";
            int colGRNNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "PO No.";
            int colPONo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;



            worksheet[ROW, COL].Text = "Vendor";
            int colVendorName = COL;
            worksheet[ROW, COL].ColumnWidth = 32;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Life Time";
            int colLifeTime = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Origin";
            int colOriginName = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Year Of Installation";
            int colYearOfInstallation = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Opening Balance";
            int colIsOpeningBalance = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;


            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            ROW++;

            for (int i = 0; i < dtGatenntryRegisterList.Rows.Count; i++)
            {
                // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                worksheet[ROW, colSerialNo].Text = dtGatenntryRegisterList.Rows[i]["SerialNo"].ToString();
                worksheet[ROW, colAssetNo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["AssetNo"].ToString());
                worksheet[ROW, colGRNNo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["GRNNo"].ToString());
                worksheet[ROW, colPONo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["PONo"].ToString());
                // worksheet[ROW, colIsOpeningBalance].Number =clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["OpeningBalance"].ToString());
                worksheet[ROW, colIsOpeningBalance].Text = (dtGatenntryRegisterList.Rows[i]["OpeningBalance"].ToString());
                worksheet[ROW, colModel].Text = dtGatenntryRegisterList.Rows[i]["Model"].ToString();
                worksheet[ROW, colCapitalizationDate].Text = dtGatenntryRegisterList.Rows[i]["CapitalizationDate"].ToString();
                worksheet[ROW, colInvoiceNo].Text = dtGatenntryRegisterList.Rows[i]["InvoiceNo"].ToString();
                worksheet[ROW, colFixedAssetMasterName].Text = dtGatenntryRegisterList.Rows[i]["FixedAssetMasterName"].ToString();

                worksheet[ROW, colMaterialMasterName].Text = dtGatenntryRegisterList.Rows[i]["MaterialMasterName"].ToString();
                worksheet[ROW, colArticle].Text = dtGatenntryRegisterList.Rows[i]["Article"].ToString();
                worksheet[ROW, colEntity].Text = dtGatenntryRegisterList.Rows[i]["Entity"].ToString();
                worksheet[ROW, colDepartment].Text = dtGatenntryRegisterList.Rows[i]["Department"].ToString();
                worksheet[ROW, colDescription].Text = dtGatenntryRegisterList.Rows[i]["Description"].ToString();
                worksheet[ROW, colDepreciationRules].Text = dtGatenntryRegisterList.Rows[i]["DepreciationRules"].ToString();
                worksheet[ROW, colPurchaseCurrency].Text = dtGatenntryRegisterList.Rows[i]["PurchaseCurrency"].ToString();
                worksheet[ROW, colBaseCurrency].Text = dtGatenntryRegisterList.Rows[i]["BaseCurrency"].ToString();

                worksheet[ROW, colQuantity].Text = dtGatenntryRegisterList.Rows[i]["Quantity"].ToString();

                worksheet[ROW, colPurchasePrice].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["PurchasePrice"].ToString());
                worksheet[ROW, colPurchasePrice].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colVendorName].Text = dtGatenntryRegisterList.Rows[i]["VendorName"].ToString();
                worksheet[ROW, colLifeTime].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["LifeTime"].ToString());
                worksheet[ROW, colOriginName].Text = dtGatenntryRegisterList.Rows[i]["OriginName"].ToString();
                worksheet[ROW, colYearOfInstallation].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["YearOfInstallation"].ToString());
                worksheet[ROW, colPurchasePrice].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["PurchasePrice"].ToString());
                worksheet[ROW, colFABaseAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["FABaseAmount"].ToString());
                worksheet[ROW, colSubAssetAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["SubAssetBaseAmount"].ToString());
                worksheet[ROW, colTotalAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["TotalBaseAmount"].ToString());

                worksheet[ROW, colADBaseAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["ADBaseAmount"].ToString());
                worksheet[ROW, colNetFixedAssetsBaseAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["NetFixedAssetsBaseAmount"].ToString());

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();

            reportUtility.PlantHeader(ref worksheet, endCol, "Fixed Assets Register", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze penes
            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;
            #endregion

            return workbook;
        }
        public IWorkbook FixedAssetRegisterDisposedList(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string nonPosted, string posted, string DisposeStatus)
        {

            //Start EmployeeAdvanceDueList
            ReportUtility reportUtility = new ReportUtility();

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtGatenntryRegisterList = GetRegisterDisposedReportData(companyGroupId, companyId, plantId, fromDate, toDate, nonPosted, posted, DisposeStatus);


            if (dtGatenntryRegisterList.Rows.Count == 0)
                throw new Exception("No data found");
            // throw new Exception("To date must be above or equal to From Date.");

            worksheet.Name = "FixedAssetsRegisterDisposedReport";

            int COL = 1; int ROW = 5;
            int startCol = COL;

            // worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
            // worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //  ROW++;



            worksheet[ROW, COL].Text = "AssetNo";
            int colAssetNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "SerialNo";
            int colSerialNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Material Master";
            int colMaterialMasterName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Article";
            int colArticle = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "SKU1";
            int colSKU1 = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "SKU2";
            int colSKU2 = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "SKU3";
            int colSKU3 = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Fixed Asset Master";
            int colFixedAssetMasterName = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;



            worksheet[ROW, COL].Text = "Disposal type";
            int colDisposaltype = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Disposal No";
            int colDisposalNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Disposal Date";
            int colDisposalDate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Customer Name";
            int colCustomerName = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Currency";
            int colCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;


            worksheet[ROW, COL].Text = "Disposed Amount";
            int colNegotiationValue = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Exchange Rate";
            int colToCurrencyRate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Base Disposed Amount";
            int colBaseNagotiationValue = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Net Book Val.";
            int colNetFixedAssetsBaseAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Loss Or Gain";
            int colLossOrGain = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Posting Status";
            int colPostingStatus = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Gate Pass No";
            int colGatePassNo = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Gate Pass Date";
            int colGatePassDate = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Description";
            int colDescription = COL;
            worksheet[ROW, COL].ColumnWidth = 40;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Model";
            int colModel = COL;
            worksheet[ROW, COL].ColumnWidth = 16;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Entity";
            int colEntity = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Department";
            int colDepartment = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Depreciation Rules";
            int colDepreciationRules = COL;
            worksheet[ROW, COL].ColumnWidth = 25;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Capitalization Date";
            int colCapitalizationDate = COL;
            worksheet[ROW, COL].ColumnWidth = 14;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Quantity";
            int colQuantity = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Pur. UOM";
            int colPurchaseUOM = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Pur. Currency";
            int colPurchaseCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Pur. Rate";
            int colPurchasePrice = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Pur. Exchange Rate";
            int colPurchaseExchangeRate = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Base Currency";
            int colBaseCurrency = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "FA Base Amount";
            int colFABaseAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "SubAsset Base Amount";
            int colSubAssetAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Total Base Amount";
            int colTotalAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "AD Base Amount";
            int colADBaseAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Process Depreciation Amount";
            int colProcessDepreciationAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 20;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Adjustment Depreciation Amount";
            int colAdjustmentDepreciationAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 22;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            worksheet[ROW, COL].Text = "Invoice No.";
            int colInvoiceNo = COL;
            worksheet[ROW, COL].ColumnWidth = 17;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "GRN No.";
            int colGRNNo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "PO No.";
            int colPONo = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;



            worksheet[ROW, COL].Text = "Vendor";
            int colVendorName = COL;
            worksheet[ROW, COL].ColumnWidth = 32;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Life Time";
            int colLifeTime = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Origin";
            int colOriginName = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            worksheet[ROW, COL].Text = "Year Of Installation";
            int colYearOfInstallation = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;



            worksheet[ROW, COL].Text = "Opening Balance";
            int colIsOpeningBalance = COL;
            worksheet[ROW, COL].ColumnWidth = 12;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;


            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            ROW++;

            for (int i = 0; i < dtGatenntryRegisterList.Rows.Count; i++)
            {
                // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                worksheet[ROW, colSerialNo].Text = dtGatenntryRegisterList.Rows[i]["SerialNo"].ToString();
                worksheet[ROW, colAssetNo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["AssetNo"].ToString());
                worksheet[ROW, colGRNNo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["GRNNo"].ToString());
                worksheet[ROW, colPONo].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["PONo"].ToString());
                // worksheet[ROW, colIsOpeningBalance].Number =clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["OpeningBalance"].ToString());
                worksheet[ROW, colIsOpeningBalance].Text = (dtGatenntryRegisterList.Rows[i]["OpeningBalance"].ToString());
                worksheet[ROW, colModel].Text = dtGatenntryRegisterList.Rows[i]["Model"].ToString();
                worksheet[ROW, colCapitalizationDate].Text = dtGatenntryRegisterList.Rows[i]["CapitalizationDate"].ToString();
                worksheet[ROW, colInvoiceNo].Text = dtGatenntryRegisterList.Rows[i]["InvoiceNo"].ToString();
                worksheet[ROW, colFixedAssetMasterName].Text = dtGatenntryRegisterList.Rows[i]["FixedAssetMasterName"].ToString();

                worksheet[ROW, colMaterialMasterName].Text = dtGatenntryRegisterList.Rows[i]["MaterialMasterName"].ToString();
                worksheet[ROW, colArticle].Text = dtGatenntryRegisterList.Rows[i]["Article"].ToString();
                worksheet[ROW, colEntity].Text = dtGatenntryRegisterList.Rows[i]["Entity"].ToString();
                worksheet[ROW, colDepartment].Text = dtGatenntryRegisterList.Rows[i]["Department"].ToString();
                worksheet[ROW, colDescription].Text = dtGatenntryRegisterList.Rows[i]["Description"].ToString();
                worksheet[ROW, colDepreciationRules].Text = dtGatenntryRegisterList.Rows[i]["DepreciationRules"].ToString();
                worksheet[ROW, colPurchaseCurrency].Text = dtGatenntryRegisterList.Rows[i]["PurchaseCurrency"].ToString();
                worksheet[ROW, colBaseCurrency].Text = dtGatenntryRegisterList.Rows[i]["BaseCurrency"].ToString();

                worksheet[ROW, colQuantity].Text = dtGatenntryRegisterList.Rows[i]["Quantity"].ToString();
                worksheet[ROW, colPurchaseUOM].Text = dtGatenntryRegisterList.Rows[i]["PurchaseUOM"].ToString();
                worksheet[ROW, colPurchaseExchangeRate].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["PurchaseExchangeRate"].ToString());
                worksheet.Range[ROW, colPurchaseExchangeRate].NumberFormat = reportUtility.NumberFormatDecimalFour();

                worksheet[ROW, colVendorName].Text = dtGatenntryRegisterList.Rows[i]["VendorName"].ToString();
                worksheet[ROW, colLifeTime].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["LifeTime"].ToString());
                worksheet[ROW, colOriginName].Text = dtGatenntryRegisterList.Rows[i]["OriginName"].ToString();
                worksheet[ROW, colYearOfInstallation].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["YearOfInstallation"].ToString());

                worksheet[ROW, colDisposaltype].Text = (dtGatenntryRegisterList.Rows[i]["Status"].ToString());
                worksheet[ROW, colDisposalNo].Text = dtGatenntryRegisterList.Rows[i]["DisposalNo"].ToString();
                worksheet[ROW, colDisposalDate].Text = dtGatenntryRegisterList.Rows[i]["DocDate"].ToString();
                worksheet[ROW, colVoucherNo].Text = dtGatenntryRegisterList.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colPostingStatus].Text = dtGatenntryRegisterList.Rows[i]["PostingStatus"].ToString();

                worksheet[ROW, colSKU1].Text = (dtGatenntryRegisterList.Rows[i]["FirstCharacteristicsValue"].ToString());
                worksheet[ROW, colSKU2].Text = dtGatenntryRegisterList.Rows[i]["SecondCharacteristicsValue"].ToString();
                worksheet[ROW, colSKU3].Text = dtGatenntryRegisterList.Rows[i]["ThirdCharacteristicsValue"].ToString();
                worksheet[ROW, colCustomerName].Text = dtGatenntryRegisterList.Rows[i]["CustomerName"].ToString();
                worksheet[ROW, colCurrency].Text = dtGatenntryRegisterList.Rows[i]["Currency"].ToString();
                worksheet[ROW, colToCurrencyRate].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["ToCurrencyRate"].ToString());
                worksheet.Range[ROW, colToCurrencyRate].NumberFormat = reportUtility.NumberFormatDecimalFour();
                worksheet[ROW, colNegotiationValue].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["NegotiationValue"].ToString());
                worksheet.Range[ROW, colNegotiationValue].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                worksheet[ROW, colBaseNagotiationValue].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["BaseNagotiationValue"].ToString());
                worksheet.Range[ROW, colBaseNagotiationValue].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                worksheet[ROW, colLossOrGain].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["LossOrGain"].ToString());
                worksheet.Range[ROW, colLossOrGain].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                worksheet[ROW, colGatePassNo].Text = dtGatenntryRegisterList.Rows[i]["GatePassNo"].ToString();
                worksheet[ROW, colGatePassDate].Text = dtGatenntryRegisterList.Rows[i]["GatePassDate"].ToString();

                worksheet[ROW, colPurchasePrice].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["PurchasePrice"].ToString());
                worksheet.Range[ROW, colPurchasePrice].NumberFormat = reportUtility.NumberFormatDecimalFour();
                worksheet[ROW, colFABaseAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["FABaseAmount"].ToString());
                worksheet.Range[ROW, colFABaseAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                worksheet[ROW, colSubAssetAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["SubAssetBaseAmount"].ToString());
                worksheet.Range[ROW, colSubAssetAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                worksheet[ROW, colTotalAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["TotalBaseAmount"].ToString());
                worksheet.Range[ROW, colTotalAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                worksheet[ROW, colADBaseAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["ADBaseAmount"].ToString());
                worksheet.Range[ROW, colADBaseAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                worksheet[ROW, colProcessDepreciationAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["ProcessDepreciationAmount"].ToString());
                worksheet.Range[ROW, colProcessDepreciationAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                worksheet[ROW, colAdjustmentDepreciationAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["AdjustmentDepreciationAmount"].ToString());
                worksheet.Range[ROW, colAdjustmentDepreciationAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                worksheet[ROW, colNetFixedAssetsBaseAmount].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["NetFixedAssetsBaseAmount"].ToString());
                worksheet.Range[ROW, colNetFixedAssetsBaseAmount].NumberFormat = reportUtility.NumberFormatDecimalTwo();

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //ReportUtility reportUtility = new ReportUtility();

            reportUtility.PlantHeader(ref worksheet, endCol, "Fixed Assets Register Disposed", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze penes
            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;
            #endregion

            return workbook;
        }

        #region FixedAssetLost
        public string InsertFixedAssetLost(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegister> fixedAssetRegister)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                string TableName = "trn.FixedAssetRegisterDisposed";
                bplib.clsGenID genidYearly = new bplib.clsGenID();
                genidYearly.GenerateIDYearly(DateTime.Now.ToString(), TableName, out string _id);
                int detailId = 0;
                var fixedAssetDispose = new FixedAssetRegisterDisposed
                {
                    Status = fixedAssetDisposed.Status,
                    Remarks = fixedAssetDisposed.Remarks,
                    EmployeeId = fixedAssetDisposed.EmployeeId,
                    Id = "RD" + _id,
                    IsPark = true,
                    DocDate = fixedAssetDisposed.DocDate
                };
                AuditService.AddedLog(fixedAssetDispose);
                _fixedAssetRegisterDisposedRepository.Insert(fixedAssetDispose);

                foreach (var item in fixedAssetRegister)
                {
                    detailId++;
                    var fixedAssetReg = _fixedAssetRegisterRepository.Find(item.Id);

                    fixedAssetReg.AdjustmentDepreciationAmount = item.AdjustmentDepreciationAmount;
                    fixedAssetReg.NegotiationValue = item.NegotiationValue;
                    fixedAssetReg.Status = fixedAssetDisposed.Status;
                    fixedAssetReg.Remarks = fixedAssetDisposed.Remarks;
                    _fixedAssetRegisterRepository.Update(fixedAssetReg);


                    var fixedAssetDisposeDetail = new FixedAssetRegisterDisposedDetail
                    {
                        FixedAssetRegisterId = fixedAssetReg.Id,
                        NegotiationValue = item.NegotiationValue,
                        BaseNagotiationValue = item.BaseNagotiationValue,

                        FixedAssetRegisterDisposedId = fixedAssetDispose.Id,
                        Id = "D" + fixedAssetDispose.Id + detailId,
                    };
                    AuditService.AddedLog(fixedAssetDisposeDetail);
                    _fixedAssetRegisterDisposedDetailRepository.Insert(fixedAssetDisposeDetail);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return fixedAssetDisposed.Remarks;
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
        public void InsertCapitalizeAssetLost(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegisterDisposedDetailViewModel> assetRegisterList, IEnumerable<FixedAssetRegisterDisposedTaxViewModel> disposedTaxList)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                string TableName = "trn.FixedAssetRegisterDisposed";
                bplib.clsGenID genidYearly = new bplib.clsGenID();
                genidYearly.GenerateIDYearly(DateTime.Now.ToString(), TableName, out string _id);
                int detailId = 0;
                var fixedAssetDispose = new FixedAssetRegisterDisposed
                {
                    Status = fixedAssetDisposed.Status,
                    Remarks = fixedAssetDisposed.Remarks,
                    LorryNo = fixedAssetDisposed.LorryNo,
                    EmployeeId = fixedAssetDisposed.EmployeeId,
                    PartyId = fixedAssetDisposed.PartyId,
                    PartyPlantId = fixedAssetDisposed.PartyPlantId,
                    DeliveryPartyPlantId = fixedAssetDisposed.DeliveryPartyPlantId,
                    InvoicingByAddress = fixedAssetDisposed.InvoicingByAddress,
                    DeliveryByAddress = fixedAssetDisposed.DeliveryByAddress,
                    CurrencyId = fixedAssetDisposed.CurrencyId,
                    ToCurrencyRate = fixedAssetDisposed.ToCurrencyRate,
                    Id = "RD" + _id,
                    IsPark = true,
                    DocDate = fixedAssetDisposed.DocDate
                };
                AuditService.AddedLog(fixedAssetDispose);
                _fixedAssetRegisterDisposedRepository.Insert(fixedAssetDispose);
                fixedAssetDisposed.Id = fixedAssetDispose.Id;
                
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";
                foreach (var item in assetRegisterList)
                {
                    detailId++;
                    builderSql = @"UPDATE [TRN].[AssetRegister] SET Status = 'Disposed' ,AdjustmentDepreciationAmount = '" + item.AdjustmentDepreciationAmount + "'  WHERE Id='" + item.AssetRegisterId + "'  ";
                    rdBuilder.Append(builderSql);


                    var fixedAssetDisposeDetail = new FixedAssetRegisterDisposedDetail
                    {
                        AssetRegisterId = item.AssetRegisterId,
                        NegotiationValue = Convert.ToDecimal(item.NegotiationValue),
                        BaseNagotiationValue = Convert.ToDecimal(item.NegotiationValue),

                        FixedAssetRegisterDisposedId = fixedAssetDispose.Id,
                        Id = "D" + fixedAssetDispose.Id + detailId,
                    };
                    AuditService.AddedLog(fixedAssetDisposeDetail);
                    _fixedAssetRegisterDisposedDetailRepository.Insert(fixedAssetDisposeDetail);
                    var currentId = 0;
                    if (disposedTaxList != null && disposedTaxList.Where(x=> x.AssetRegisterId==item.AssetRegisterId).Count() > 0)
                    {
                        foreach (var taxVM in disposedTaxList.Where(x => x.AssetRegisterId == item.AssetRegisterId))
                        {
                            if (taxVM.TaxCategoryId == null)
                                throw new CustomException("Please Select Tax Category !");

                            currentId++;
                            var disposedTax = new FixedAssetRegisterDisposedTax
                            {
                                Id = _pkGeneratorService.MakePK(fixedAssetDisposeDetail.Id, currentId, 2),
                                AssetRegisterId = item.AssetRegisterId,
                                FixedAssetRegisterDisposedId = fixedAssetDisposeDetail.FixedAssetRegisterDisposedId,
                                FixedAssetRegisterDisposedDetailId = fixedAssetDisposeDetail.Id,
                                Percentage = taxVM.Percentage,
                                TaxCategoryId = taxVM.TaxCategoryId,
                                Amount = taxVM.Amount,
                                AddedBy = fixedAssetDisposeDetail.AddedBy,
                                AddedDate = fixedAssetDisposeDetail.AddedDate,
                                AddedFromIP = fixedAssetDisposeDetail.AddedFromIP,
                                ModelState = ModelState.Added,
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null
                            };
                            _fixedAssetRegisterDisposedTaxRepository.Insert(disposedTax);
                        }
                    }
                }

                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                //return fixedAssetDisposed;
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
        public void DeleteCapitalizationMaster(string capitalizationMasterId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var inDirect = new System.Text.StringBuilder();
                var inDirectsql = "";

                inDirectsql = @"DELETE FROM [TRN].[AssetRegisterChild] where CapitalizationMasterId in('" + capitalizationMasterId + @"')
                                DELETE FROM [TRN].[AssetRegister] where CapitalizationMasterId='" + capitalizationMasterId + @"'
		                        DELETE FROM [TRN].[CapitalizationMasterDetail] where CapitalizationMasterId in('" + capitalizationMasterId + @"')
		                        DELETE FROM [TRN].[CapitalizationMaster] where Id in('" + capitalizationMasterId + @"') ";
                inDirect.Append(inDirectsql);
                _sqlRepository.ExecuteSqlCommand(inDirect.ToString());
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

        public void DeleteCapitalizationMasterPost(string voucherId, string deletedRemarks)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.CheckingFiscalYearClose(voucher);
                _accountsCommonService.InsertVoucherLogDeleted(voucherId, voucher.VoucherNo, "", "", "", "", "", "", "", "", "", "", "", deletedRemarks);

                var inDirect = new System.Text.StringBuilder();
                var inDirectsql = "";

                inDirectsql = @"DECLARE @VoucherId varchar(50)='" + voucherId + @"'
	                            UPDATE [TRN].[AssetRegisterChild] SET VoucherDetailId=NULL where CapitalizationMasterId IN(SELECT Id FROM [TRN].[CapitalizationMaster] where VoucherId=@VoucherId)
	                            UPDATE [TRN].[CapitalizationMaster] SET IsApproved=0,VoucherId=NULL where VoucherId=@VoucherId
	                            DELETE from trn.VoucherDetailCurrency where VoucherId=@VoucherId
	                            DELETE from trn.VoucherDetail where VoucherId=@VoucherId
	                            DELETE from trn.Voucher where Id=@VoucherId ";
                inDirect.Append(inDirectsql);
                _sqlRepository.ExecuteSqlCommand(inDirect.ToString());
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

        public void DeleteCapitalizeAssetRegisterDisposed(string fixedAssetRegisterDisposedId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var inDirect = new System.Text.StringBuilder();
                var inDirectsql = "";

                inDirectsql = @"DELETE FROM [TRN].[FixedAssetRegisterDisposedTax] WHERE FixedAssetRegisterDisposedId='" + fixedAssetRegisterDisposedId + @"'
                            DELETE FROM [TRN].[FixedAssetRegisterDisposedAdditionalTax] WHERE FixedAssetRegisterDisposedId='" + fixedAssetRegisterDisposedId + @"'
                            DELETE FROM [TRN].[FixedAssetRegisterDisposedDetail] WHERE FixedAssetRegisterDisposedId='" + fixedAssetRegisterDisposedId + @"'
                            DELETE FROM [TRN].[FixedAssetRegisterDisposed] WHERE Id='" + fixedAssetRegisterDisposedId + @"' ";
                inDirect.Append(inDirectsql);
                _sqlRepository.ExecuteSqlCommand(inDirect.ToString());
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

        public void DeleteDepreciationProcess(string assetDepreciationId)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var inDirect = new System.Text.StringBuilder();
                var inDirectsql = "";

                inDirectsql = @"DELETE FROM [TRN].[AssetDepreciationDetail] WHERE AssetDepreciationId in('" + assetDepreciationId + @"') AND AssetRegisterId NOT IN(SELECT AssetRegisterId FROM [TRN].[FixedAssetRegisterDisposedDetail])
	                            DELETE FROM [TRN].[AssetDepreciation] WHERE Id in('" + assetDepreciationId + @"') AND Id NOT IN(SELECT AssetDepreciationId FROM [TRN].[AssetDepreciationDetail] WHERE AssetDepreciationId in('" + assetDepreciationId + @"') AND AssetRegisterId  IN(SELECT AssetRegisterId FROM [TRN].[FixedAssetRegisterDisposedDetail]))
	                            UPDATE [TRN].[AssetDepreciation] SET Status='Disposed Assets Depreciation' WHERE Id in('" + assetDepreciationId + @"') AND Id  IN(SELECT AssetDepreciationId FROM [TRN].[AssetDepreciationDetail] WHERE AssetDepreciationId in('" + assetDepreciationId + @"') AND AssetRegisterId  IN(SELECT AssetRegisterId FROM [TRN].[FixedAssetRegisterDisposedDetail])) ";
                inDirect.Append(inDirectsql);
                _sqlRepository.ExecuteSqlCommand(inDirect.ToString());
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
        public void DeleteDepreciationProcessPost(string voucherId, string deletedRemarks)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.CheckingFiscalYearClose(voucher);
                _accountsCommonService.InsertVoucherLogDeleted(voucherId, voucher.VoucherNo, "", "", "", "", "", "", "", "", "", "", "", deletedRemarks);

                var inDirect = new System.Text.StringBuilder();
                var inDirectsql = "";

                inDirectsql = @"UPDATE [TRN].[AssetDepreciationDetail] SET VoucherDetailId=NULL  where AssetDepreciationId in(SELECT Id FROM  [TRN].[AssetDepreciation] WHERE VoucherId in('" + voucherId + @"'))
	                            UPDATE [TRN].[AssetDepreciation]  SET VoucherId=NULL WHERE VoucherId in('" + voucherId + @"')
	                            DELETE from trn.VoucherDetailCurrency where VoucherId in('" + voucherId + @"')
	                            DELETE from trn.VoucherDetail where VoucherId in('" + voucherId + @"')
	                            DELETE from trn.Voucher where Id in('" + voucherId + @"') ";
                inDirect.Append(inDirectsql);
                _sqlRepository.ExecuteSqlCommand(inDirect.ToString());
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

        public string EditFixedAssetLost(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegisterDisposedDetail> fixedAssetRegister)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                int detailId = 0;
                var fixedAssetDispose = new FixedAssetRegisterDisposed
                {
                    Status = fixedAssetDisposed.Status,
                    Remarks = fixedAssetDisposed.Remarks,
                    EmployeeId = fixedAssetDisposed.EmployeeId,
                    Id = fixedAssetDisposed.Id,
                    IsPark = true,
                    DocDate = fixedAssetDisposed.DocDate
                };
                AuditService.UpdatedLog(fixedAssetDispose);
                _fixedAssetRegisterDisposedRepository.Update(fixedAssetDispose);

                foreach (var item in fixedAssetRegister)
                {
                    detailId++;
                    var fixedAssetReg = _fixedAssetRegisterRepository.Find(item.FixedAssetRegisterId);

                    fixedAssetReg.AdjustmentDepreciationAmount = item.AdjustmentDepreciationAmount;
                    fixedAssetReg.NegotiationValue = item.NegotiationValue;
                    fixedAssetReg.BaseNagotiationValue = item.BaseNagotiationValue;
                    fixedAssetReg.Status = fixedAssetDisposed.Status;
                    fixedAssetReg.Remarks = fixedAssetDisposed.Remarks;
                    _fixedAssetRegisterRepository.Update(fixedAssetReg);


                    var fixedAssetDisposeDetail = new FixedAssetRegisterDisposedDetail
                    {
                        FixedAssetRegisterId = item.FixedAssetRegisterId,
                        NegotiationValue = item.NegotiationValue,
                        BaseNagotiationValue = item.BaseNagotiationValue,
                        FixedAssetRegisterDisposedId = fixedAssetDispose.Id,
                        Id = item.Id,
                    };
                    AuditService.UpdatedLog(fixedAssetDisposeDetail);
                    _fixedAssetRegisterDisposedDetailRepository.Update(fixedAssetDisposeDetail);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return fixedAssetDisposed.Remarks;
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

        #endregion
        #region FixedAsset Sales
        public string InsertFixedAssetSales(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegister> fixedAssetRegister)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                string TableName = "trn.FixedAssetRegisterDisposed";
                bplib.clsGenID genidYearly = new bplib.clsGenID();
                genidYearly.GenerateIDYearly(DateTime.Now.ToString(), TableName, out string _id);
                int detailId = 0;
                var fixedAssetDispose = new FixedAssetRegisterDisposed
                {
                    Status = fixedAssetDisposed.Status,
                    Remarks = fixedAssetDisposed.Remarks,
                    PartyId = fixedAssetDisposed.PartyId,
                    PartyPlantId = fixedAssetDisposed.PartyPlantId,
                    DeliveryPartyPlantId = fixedAssetDisposed.DeliveryPartyPlantId,
                    InvoicingByAddress = fixedAssetDisposed.InvoicingByAddress,
                    DeliveryByAddress = fixedAssetDisposed.DeliveryByAddress,
                    Id = "RD" + _id,
                    IsPark = true,
                    ToCurrencyRate = fixedAssetDisposed.ToCurrencyRate,
                    CurrencyId = fixedAssetDisposed.CurrencyId,
                    DocDate = fixedAssetDisposed.DocDate


                };
                AuditService.AddedLog(fixedAssetDispose);
                _fixedAssetRegisterDisposedRepository.Insert(fixedAssetDispose);

                foreach (var item in fixedAssetRegister)
                {
                    detailId++;
                    var fixedAssetReg = _fixedAssetRegisterRepository.Find(item.Id);

                    fixedAssetReg.AdjustmentDepreciationAmount = item.AdjustmentDepreciationAmount;
                    fixedAssetReg.NegotiationValue = item.NegotiationValue;
                    fixedAssetReg.BaseNagotiationValue = item.BaseNagotiationValue;
                    fixedAssetReg.Status = fixedAssetDisposed.Status;
                    fixedAssetReg.Remarks = fixedAssetDisposed.Remarks;
                    _fixedAssetRegisterRepository.Update(fixedAssetReg);


                    var fixedAssetDisposeDetail = new FixedAssetRegisterDisposedDetail
                    {
                        FixedAssetRegisterId = fixedAssetReg.Id,
                        NegotiationValue = item.NegotiationValue,
                        FixedAssetRegisterDisposedId = fixedAssetDispose.Id,
                        Id = "D" + fixedAssetDispose.Id + detailId,
                        BaseNagotiationValue = item.BaseNagotiationValue
                    };
                    AuditService.AddedLog(fixedAssetDisposeDetail);
                    _fixedAssetRegisterDisposedDetailRepository.Insert(fixedAssetDisposeDetail);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return fixedAssetDisposed.Remarks;
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

        public string EditFixedAssetSales(string status, FixedAssetRegisterDisposed disposeVM, IEnumerable<FixedAssetRegisterDisposedDetail> fixedAssetRegister)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                int detailId = 0;
                var fixedAssetDispose = new FixedAssetRegisterDisposed
                {
                    Status = disposeVM.Status,
                    Remarks = disposeVM.Remarks,
                    PartyId = disposeVM.PartyId,
                    PartyPlantId = disposeVM.PartyPlantId,
                    DeliveryPartyPlantId = disposeVM.DeliveryPartyPlantId,
                    InvoicingByAddress = disposeVM.InvoicingByAddress,
                    DeliveryByAddress = disposeVM.DeliveryByAddress,

                    Id = disposeVM.Id,
                    IsPark = true,
                    ToCurrencyRate = disposeVM.ToCurrencyRate,
                    CurrencyId = disposeVM.CurrencyId,
                    DocDate = disposeVM.DocDate
                };
                AuditService.UpdatedLog(fixedAssetDispose);
                _fixedAssetRegisterDisposedRepository.Update(fixedAssetDispose);

                foreach (var item in fixedAssetRegister)
                {
                    detailId++;
                    var fixedAssetReg = _fixedAssetRegisterRepository.Find(item.FixedAssetRegisterId);

                    fixedAssetReg.AdjustmentDepreciationAmount = item.AdjustmentDepreciationAmount;
                    fixedAssetReg.NegotiationValue = item.NegotiationValue;
                    fixedAssetReg.BaseNagotiationValue = item.BaseNagotiationValue;
                    fixedAssetReg.Status = status;
                    fixedAssetReg.Remarks = disposeVM.Remarks;
                    _fixedAssetRegisterRepository.Update(fixedAssetReg);


                    var fixedAssetDisposeDetail = new FixedAssetRegisterDisposedDetail
                    {
                        FixedAssetRegisterId = item.FixedAssetRegisterId,
                        NegotiationValue = item.NegotiationValue,
                        FixedAssetRegisterDisposedId = fixedAssetDispose.Id,
                        BaseNagotiationValue = item.BaseNagotiationValue,
                        Id = item.Id,

                    };
                    AuditService.UpdatedLog(fixedAssetDisposeDetail);
                    _fixedAssetRegisterDisposedDetailRepository.Update(fixedAssetDisposeDetail);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return disposeVM.Remarks;
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


        #endregion

        #region FixedAsset Scrap and Theft
        public string InsertFixedAssetScrap(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegister> fixedAssetRegister)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                string TableName = "trn.FixedAssetRegisterDisposed";
                bplib.clsGenID genidYearly = new bplib.clsGenID();
                genidYearly.GenerateIDYearly(DateTime.Now.ToString(), TableName, out string _id);
                int detailId = 0;
                var fixedAssetDispose = new FixedAssetRegisterDisposed
                {
                    Status = fixedAssetDisposed.Status,
                    Remarks = fixedAssetDisposed.Remarks,
                    Id = "RD" + _id,
                    IsPark = true,
                    DocDate = fixedAssetDisposed.DocDate
                };
                AuditService.AddedLog(fixedAssetDispose);
                _fixedAssetRegisterDisposedRepository.Insert(fixedAssetDispose);

                foreach (var item in fixedAssetRegister)
                {
                    detailId++;
                    var fixedAssetReg = _fixedAssetRegisterRepository.Find(item.Id);

                    fixedAssetReg.AdjustmentDepreciationAmount = item.AdjustmentDepreciationAmount;
                    fixedAssetReg.NegotiationValue = 0;
                    fixedAssetReg.BaseNagotiationValue = 0;
                    fixedAssetReg.Status = fixedAssetDisposed.Status;
                    fixedAssetReg.Remarks = fixedAssetDisposed.Remarks;
                    _fixedAssetRegisterRepository.Update(fixedAssetReg);


                    var fixedAssetDisposeDetail = new FixedAssetRegisterDisposedDetail
                    {
                        FixedAssetRegisterId = fixedAssetReg.Id,
                        NegotiationValue = 0,
                        FixedAssetRegisterDisposedId = fixedAssetDispose.Id,
                        Id = "D" + fixedAssetDispose.Id + detailId,
                    };
                    AuditService.AddedLog(fixedAssetDisposeDetail);
                    _fixedAssetRegisterDisposedDetailRepository.Insert(fixedAssetDisposeDetail);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return fixedAssetDisposed.Remarks;
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
        public string EditFixedAssetScrap(FixedAssetRegisterDisposed fixedAssetDisposed, IEnumerable<FixedAssetRegisterDisposedDetail> fixedAssetRegister)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                int detailId = 0;
                var fixedAssetDispose = new FixedAssetRegisterDisposed
                {
                    Status = fixedAssetDisposed.Status,
                    Remarks = fixedAssetDisposed.Remarks,
                    Id = fixedAssetDisposed.Id,
                    IsPark = true,
                    DocDate = fixedAssetDisposed.DocDate
                };
                AuditService.UpdatedLog(fixedAssetDispose);
                _fixedAssetRegisterDisposedRepository.Update(fixedAssetDispose);

                foreach (var item in fixedAssetRegister)
                {
                    detailId++;
                    var fixedAssetReg = _fixedAssetRegisterRepository.Find(item.FixedAssetRegisterId);

                    fixedAssetReg.AdjustmentDepreciationAmount = item.AdjustmentDepreciationAmount;
                    fixedAssetReg.NegotiationValue = 0;
                    fixedAssetReg.BaseNagotiationValue = 0;
                    fixedAssetReg.Status = fixedAssetDisposed.Status;
                    fixedAssetReg.Remarks = fixedAssetDisposed.Remarks;
                    _fixedAssetRegisterRepository.Update(fixedAssetReg);


                    var fixedAssetDisposeDetail = new FixedAssetRegisterDisposedDetail
                    {
                        FixedAssetRegisterId = item.FixedAssetRegisterId,
                        NegotiationValue = 0,
                        FixedAssetRegisterDisposedId = fixedAssetDispose.Id,
                        Id = item.Id,
                    };
                    AuditService.UpdatedLog(fixedAssetDisposeDetail);
                    _fixedAssetRegisterDisposedDetailRepository.Update(fixedAssetDisposeDetail);
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                return fixedAssetDisposed.Remarks;
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

        #endregion


        public IEnumerable<object> GetCapitalizeAssetRegisterApproveByCbo()
        {
            try
            {
                var sql = @"SELECT E.SystemId As Value, E.EmployeeCode+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          INNER JOIN dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          WHERE  A.ActionStatus='CapitalizeAssetRegisterApproveBy' AND E.EmployeeStatus='Active'";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

    }
}