using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Materials;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.Service.Products
{
    public class ProductDefinitionService : Service<ProductDefinition>, IProductDefinitionService
    {
        #region Constructor

        private readonly IRepositoryAsync<ProductDefinition> _productDefinitionRepository;
        private readonly IRepositoryAsync<ProductDefinitionEfficency> _efficencyRepository;
        private readonly IMaterialMasterArticleService _articleService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public ProductDefinitionService(
            IRepositoryAsync<ProductDefinition> productDefinitionRepository
            , IRepositoryAsync<ProductDefinitionEfficency> efficencyRepository
            , IMaterialMasterArticleService articleService
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IUnitOfWork unitOfWork)
            : base(productDefinitionRepository, unitOfWork, pkGeneratorService)
        {
            _productDefinitionRepository = productDefinitionRepository;
            _efficencyRepository = efficencyRepository;
            _articleService = articleService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(ProductDefinition), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public GridModel GetMaterialMasterList(GridParameter parameters, string companyGroupId, string[] searchParam)
        {
            try
            {
                const string sqlParam = "";
                //sqlParam += CreateWhereCluse(searchParam[0], "MT.Description", sqlParam);
                //sqlParam += CreateWhereCluse(searchParam[1], "MGP.UserName", sqlParam);
                //sqlParam += CreateWhereCluse(searchParam[2], "MC.UserName", sqlParam);
                //sqlParam += CreateWhereCluse(searchParam[3], "MSC.UserName", sqlParam);
                //if (!string.IsNullOrEmpty(sqlParam))
                //    sqlParam = sqlParam.Insert(0, " AND ");

                parameters.CmdText = @"SELECT MT.UserName AS MaterialType
                                      , MGP.UserName AS MaterialGroupMaster
                                      , MM.ProductMasterId, PM.UserName AS ProductMaster
                                      , UOMB.UserName AS BaseUoM
                                      , MC.UserName MaterialCategory
	                                  , MSC.UserName MaterialSubCategory
                                      , MM.Id AS MaterialMasterId
                                      , MM.Sequence,MM.Code,MM.ShortName,MM.StandardName,MM.UserName, SKU=CASE WHEN MM.WithSKU=1 THEN 'Yes' ELSE 'No' END
                                      , Active=CASE WHEN MM.Active=1 THEN 'Yes' ELSE 'No' END
                                      --, FAM.UserName AS AssetMaster, FAM.AssetType
                                      , B.Code AS AssetBudgetCode
                                      , Revenue=CASE WHEN (MM.IsInventory=1 OR MM.IsExpenseOut=1) THEN 'Yes' ELSE 'No' END
                FROM [MST].[MaterialMaster] AS MM
                LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
                LEFT OUTER JOIN [HKP].[MaterialCategory] AS MC ON MM.MaterialCategoryId = MC.Id
                LEFT OUTER JOIN [HKP].[MaterialSubCategory] AS MSC ON MM.MaterialSubCategoryId = MSC.Id
                --LEFT OUTER JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId = FAM.Id
                LEFT OUTER JOIN [MST].[ProductMaster] AS PM ON MM.ProductMasterId = PM.Id
                INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                LEFT JOIN MST.BudgetMaster AS BM ON MM.BudgetMasterId=BM.Id
                LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                WHERE MM.CompanyGroupId = '" + companyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1
                AND MM.Id IN(SELECT MaterialMasterId FROM MST.MaterialMasterBusinessProcess AS A INNER JOIN SCS.BusinessProcess AS B ON A.BusinessProcessId=B.Id
							WHERE MM.Id NOT IN(SELECT MaterialMasterId FROM TRN.ProductDefinition) AND B.BusinessProcessName='" + BusinessProcessEnum.ProductDefinition + "')" + sqlParam;
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                  Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                  ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetSavedData()
        {
            try
            {
                string CmdText = @"SELECT A.Id, MT.UserName AS MaterialType, MGP.UserName AS MaterialGroupMaster, UoM.UserName AS BaseUoM
								, MC.UserName MaterialCategory, MSC.UserName MaterialSubCategory
	                            , A.MaterialMasterId,MM.Code,MM.StandardName, MM.UserName, A.ProductMasterId, PM.UserName ProductMaster
                                FROM TRN.ProductDefinition AS A
                                INNER JOIN MST.MaterialMaster AS MM ON A.MaterialMasterId=MM.Id
				                LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
				                LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
                                LEFT OUTER JOIN [HKP].[MaterialCategory] AS MC ON MM.MaterialCategoryId = MC.Id
                                LEFT OUTER JOIN [HKP].[MaterialSubCategory] AS MSC ON MM.MaterialSubCategoryId = MSC.Id
                                INNER JOIN SCS.UnitOfMeasurement AS UoM ON MM.BaseUOMId=UoM.Id
                                INNER JOIN MST.ProductMaster AS PM ON A.ProductMasterId=PM.Id";
                return _sqlRepository.GetDataCollection(CmdText);
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

        public IEnumerable<object> GetMaterialMasterList(string companyGroupId)
        {
            try
            {
                string CmdText = @"SELECT MT.UserName AS MaterialType
                                      , MGP.UserName AS MaterialGroupMaster
                                      , UOMB.UserName AS BaseUoM
                                      , MC.UserName MaterialCategory
	                                  , MSC.UserName MaterialSubCategory
                                      , MM.Id AS MaterialMasterId
                                      --, MM.Sequence
                                      ,MM.Code
                                      --,MM.ShortName
                                      ,MM.StandardName
                                      ,MM.UserName
                                      --, SKU=CASE WHEN MM.WithSKU=1 THEN 'Yes' ELSE 'No' END
                                      , Active=CASE WHEN MM.Active=1 THEN 'Yes' ELSE 'No' END
                                      --, B.Code AS AssetBudgetCode
                                      --, Revenue=CASE WHEN (MM.IsInventory=1 OR MM.IsExpenseOut=1) THEN 'Yes' ELSE 'No' END
                                      ,0 Flag
                FROM [MST].[MaterialMaster] AS MM
                LEFT OUTER JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                LEFT OUTER JOIN [HKP].[MaterialType] AS MT ON MGP.MaterialTypeId = MT.Id
                LEFT OUTER JOIN [HKP].[MaterialCategory] AS MC ON MM.MaterialCategoryId = MC.Id
                LEFT OUTER JOIN [HKP].[MaterialSubCategory] AS MSC ON MM.MaterialSubCategoryId = MSC.Id
                INNER JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                LEFT JOIN MST.BudgetMaster AS BM ON MM.BudgetMasterId=BM.Id
                LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
                WHERE MM.CompanyGroupId = '" + companyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1
                AND MM.Id IN(SELECT MaterialMasterId FROM MST.MaterialMasterBusinessProcess AS A INNER JOIN SCS.BusinessProcess AS B ON A.BusinessProcessId=B.Id
							WHERE MM.Id NOT IN(SELECT MaterialMasterId FROM TRN.ProductDefinition) AND B.BusinessProcessName='" + BusinessProcessEnum.ProductDefinition + "')";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                  Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                  ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string[] tempParam)
        {
            try
            {
                var str = "";
                str += CreateWhereCluse(tempParam[0], "MM.UserName", str);
                str += CreateWhereCluse(tempParam[1], "UoM.UserName", str);
                str += CreateWhereCluse(tempParam[2], "PM.UserName", str);
                str += CreateWhereCluse(tempParam[3], "S.UserName", str);
                str += CreateWhereCluse(tempParam[4], "OS.UserName", str);
                if (!string.IsNullOrEmpty(str))
                    str = str.Insert(0, " WHERE ");
                parameters.CmdText = @"SELECT A.Id
	                                    , A.MaterialMasterId, MM.UserName, UoM.UserName AS BaseUoM
		                                , A.ProductMasterId, PM.UserName AS ProductMasterName
		                                , A.SeasonId, S.UserName AS SeasonName
		                                , A.OurStyleId, OS.UserName AS OurStyleName
		                                , A.CostAndManufactureCurrencyId, C.Code AS CMCurrency
		                                , A.CostAndManufacture, A.TotalQty
		                                , A.FirstdayOutPut, A.IsFixed, A.IncrementValue, A.DaysToReachTheTarget, A.Active,A.ProcessId
                                FROM TRN.ProductDefinition AS A
                                INNER JOIN MST.MaterialMaster AS MM ON A.MaterialMasterId=MM.Id
                                INNER JOIN SCS.UnitOfMeasurement AS UoM ON MM.BaseUOMId=UoM.Id
                                INNER JOIN MST.ProductMaster AS PM ON A.ProductMasterId=PM.Id
                                INNER JOIN HKP.Season AS S ON A.SeasonId=S.Id
                                LEFT JOIN HKP.OurStyle AS OS ON A.OurStyleId=OS.Id
                                INNER JOIN SCS.Currency AS C ON A.CostAndManufactureCurrencyId=C.Id";
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

        public void InsertOrUpdateGraph(IEnumerable<ProductDefinition> entities)
        {
            var flag = false;
            try
            {
                if (entities == null)
                    throw new CustomException("Please insert Product");
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var item in entities)
                {
                    var pk = GetAutoNumber(nameof(ProductDefinition), PKGeneratorEnum.Auto, null, DateTime.Now);
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        
                        item.Id = pk;
                        item.CostAndManufacture = 0.0m;
                        item.FirstdayOutPut = 0;
                        item.IsFixed = string.Empty;
                        item.IncrementValue = 0;
                        item.DaysToReachTheTarget = 0;
                        item.Active = true;
                        item.TotalQty = 0;
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
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Setup.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertGraph(ProductDefinition entity, IEnumerable<MaterialMasterArticle> articleList, IEnumerable<ProductDefinitionEfficency> efficencyList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.Id = GetPK();
                base.InsertGraph(entity);
                if (articleList != null)
                    _articleService.ProcessInsertGraph(entity.Id, articleList);
                if (efficencyList != null)
                {
                    var count = _productDefinitionRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ProductDefinitionEfficency] WHERE ProductDefinitionId='{entity.Id}'").First();
                    foreach (var item in efficencyList)
                    {
                        count++;
                        item.Id = MakePK(entity.Id, count, 2);
                        item.ProductDefinitionId = entity.Id;
                        AuditService.AddedLog(item);
                        _efficencyRepository.Insert(item);
                    }
                }
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
                  ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void UpdateGraph(ProductDefinition entity, IEnumerable<MaterialMasterArticle> articleList, IEnumerable<ProductDefinitionEfficency> efficencyList)
        {
            var flag = false;
            try
            {
                var data = Find(entity.Id);
                if (data == null) throw new CustomException("This data has no longer.");
                _unitOfWork.BeginTransaction();
                flag = true;
                Update(entity);
                if (articleList != null)
                    _articleService.ProcessInsertGraph(entity.Id, articleList);
                if (efficencyList != null)
                {
                    var count = _productDefinitionRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ProductDefinitionEfficency] WHERE ProductDefinitionId='{entity.Id}'").First();
                    foreach (var item in efficencyList)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            item.Id = MakePK(entity.Id, count, 2);
                            item.ProductDefinitionId = entity.Id;
                            AuditService.AddedLog(item);
                            _efficencyRepository.Insert(item);
                        }
                        else _efficencyRepository.Update(item);
                    }
                }
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
                  ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
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
                var data = Find(id);
                if (data == null) throw new CustomException("This data has no longer.");
                _unitOfWork.BeginTransaction();
                flag = true;
                _articleService.DeleteArticleProcessGraphByProductDefinition(data.Id);
                var efficencyList = _efficencyRepository.Query(t => t.ProductDefinitionId == id).Select().ToList();
                if (efficencyList != null)
                {
                    foreach (var item in efficencyList)
                    {
                        _efficencyRepository.Delete(item);
                    }
                }
                Delete(data);
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
                  ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        #region ProductDefinitionEfficency

        public IEnumerable<ProductDefinitionEfficency> GetEfficencyList(string masterId)
        {
            try
            {
                var data = new List<ProductDefinitionEfficency>();
                if (masterId != "undefined")
                {
                    var sql = @"SELECT A.Id, A.ProductDefinitionId, A.ColumnSequence, A.EfficencyName, CAST(A.SPT AS INT) AS SPT
                            , A.NoOfWorkStation, A.EfficencyPercentage, A.AddedBy, A.AddedDate, A.AddedFromIP, A.UpdatedBy, A.UpdatedDate, A.UpdatedFromIP
                        FROM TRN.ProductDefinitionEfficency AS A WHERE A.ProductDefinitionId='" + masterId + @"' ORDER BY A.ColumnSequence";
                    data = _productDefinitionRepository.SqlQuery<ProductDefinitionEfficency>(sql).ToList();
                }
                else
                    data = CreateTable(masterId);
                return data;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }
        private List<ProductDefinitionEfficency> CreateTable(string masterId)
        {
            try
            {
                var list = new List<ProductDefinitionEfficency>();
                var seq = 1;
                foreach (var item in Enum.GetValues(typeof(ProductEfficency)))
                {
                    var model = new ProductDefinitionEfficency
                    {
                        Id = null,
                        ProductDefinitionId = null,
                        SPT = 0,
                        ColumnSequence = seq,
                        EfficencyName = item.ToString(),
                        NoOfWorkStation = 0,
                        EfficencyPercentage = 0,
                        AddedBy = null,
                        AddedDate = DateTime.Now,
                        AddedFromIP = null,
                        UpdatedBy = null,
                        UpdatedDate = null,
                        UpdatedFromIP = null
                    };
                    list.Add(model);
                    seq++;
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #endregion ProductDefinitionEfficency
    }
}