using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Reflection;

namespace Library.Service.Materials
{
    public partial class MaterialStockService : Service<MaterialStock>, IMaterialStockService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public MaterialStockService(
            IRepositoryAsync<MaterialStock> materialStockRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork)
            : base(materialStockRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string[] tempParam)
        {
            try
            {
                string str = "";
                str += CreateWhereCluse(tempParam[0], "MM.Code", str);
                str += CreateWhereCluse(tempParam[1], "MM.ShortName", str);
                str += CreateWhereCluse(tempParam[2], "MM.StandardName", str);
                str += CreateWhereCluse(tempParam[3], "MM.UserName", str);
                str += CreateWhereCluse(tempParam[4], "UoM.UserName", str);
                if (!string.IsNullOrEmpty(str))
                    str = str.Insert(0, " WHERE ");
                parameters.CmdText = @"SELECT A.Id
	                                , A.MaterialMasterId , MM.Code, MM.ShortName, MM.StandardName, MM.UserName, UoM.UserName AS BaseUoM
	                                , A.StandardRateCurrencyId, UoM1.UserName AS StandardRateCurrency
	                                , A.StandardRateUoMId, UoM2.UserName AS StandardRateUoM
	                                , A.InventoryUoMId, UoM3.UserName AS InventoryUoM
	                                , A.UsageType, A.RequirementType
	                                , A.HazardsLevel, A.Flammability, A.PurchaseFrequency, A.LifeTimeinDays, A.StandardRate
	                                , A.MinimumOrderQuantity, A.MinimumInventoryLevel, A.ReorderLevel, A.IsLocal, A.IsImport
                                FROM TRN.MaterialStock AS A
                                INNER JOIN MST.MaterialMaster AS MM ON A.MaterialMasterId=MM.Id
                                INNER JOIN SCS.UnitOfMeasurement AS UoM ON MM.BaseUOMId=UoM.Id
                                INNER JOIN SCS.UnitOfMeasurement AS UoM1 ON A.StandardRateCurrencyId=UoM1.Id
                                INNER JOIN SCS.UnitOfMeasurement AS UoM2 ON A.StandardRateUoMId=UoM2.Id
                                INNER JOIN SCS.UnitOfMeasurement AS UoM3 ON A.InventoryUoMId=UoM3.Id" + str;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private static string CreateWhereCluse(string fieldValue, string sqlField, string conditionVariable)
        {
            if (!string.IsNullOrEmpty(fieldValue))
            {
                var str = "";
                if (!string.IsNullOrEmpty(conditionVariable)) str = " AND ";
                return str + sqlField + @" LIKE ('%" + fieldValue + "%')";
            }
            return string.Empty;
        }

        public GridModel GetMaterialMasterList(GridParameter parameters, string companyGroupId, string[] searchParam)
        {
            try
            {
                string sqlParam = "";
                sqlParam += CreateWhereCluse(searchParam[0], "MT.Description", sqlParam);
                sqlParam += CreateWhereCluse(searchParam[1], "MGP.UserName", sqlParam);
                sqlParam += CreateWhereCluse(searchParam[2], "MC.UserName", sqlParam);
                sqlParam += CreateWhereCluse(searchParam[3], "MSC.UserName", sqlParam);
                if (!string.IsNullOrEmpty(sqlParam))
                    sqlParam = sqlParam.Insert(0, " AND ");

                parameters.CmdText = @"SELECT MT.Description AS MaterialType
                                      , MGP.UserName AS MaterialGroupMaster
                                      , PM.UserName AS ProductMaster
                                      , UOMB.UserName AS BaseUom
                                      , MC.UserName MaterialCategory
	                                  , MSC.UserName MaterialSubCategory
                                      , MM.Id AS MaterialMasterId
                                      , MM.Sequence,MM.Code,MM.ShortName,MM.StandardName,MM.UserName, SKU=CASE WHEN MM.WithSKU=1 THEN 'Yes' ELSE 'No' END
                                      , Active=CASE WHEN MM.Active=1 THEN 'Yes' ELSE 'No' END
                                      , FAM.UserName AS AssetMaster, FAM.AssetType
                                      , B.Code AS AssetBudgetCode
                                      , Revenue=CASE WHEN (MM.IsInventory=1 OR MM.IsExpenseOut=1) THEN 'Yes' ELSE 'No' END
                FROM [MST].[MaterialMaster] AS MM
                LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                LEFT OUTER JOIN [HKP].[MaterialCategory] AS MC ON MM.MaterialCategoryId = MC.Id
                LEFT OUTER JOIN [HKP].[MaterialSubCategory] AS MSC ON MM.MaterialSubCategoryId = MSC.Id
                LEFT OUTER JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId = FAM.Id
                LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                LEFT JOIN MST.BudgetMaster AS BM ON MM.BudgetMasterId=BM.Id
                LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                WHERE MM.CompanyGroupId = '" + companyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1
                AND MM.Id IN(SELECT MaterialMasterId FROM MST.MaterialMasterBusinessProcess AS A INNER JOIN SCS.BusinessProcess AS B ON A.BusinessProcessId=B.Id
							WHERE B.BusinessProcessName='" + BusinessProcessEnum.MaterialStock+ "')" + sqlParam;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                  Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                  ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(MaterialStock), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Insert(MaterialStock entity)
        {
            try
            {
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public override void Update(MaterialStock entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void Delete(string id)
        {
            try
            {
                var entity = Find(id);
                if (entity != null)
                    base.Delete(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}