using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.Service.Products
{
    public class ProductMasterService : Service<ProductMaster>, IProductMasterService
    {
        #region Constructor


        private readonly IProductMasterAttributeValueService _productMasterAttributeValueService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<ProductMaster> _charaterValueRepository;
        private readonly IRepositoryAsync<ProductMasterEfficency> _efficencyRepository;
        private readonly IRepositoryAsync<ProductMasterAlternativeUOM> _materialMasterAlternativeUOM;

        public ProductMasterService(
            IRepositoryAsync<ProductMaster> charaterValueRepository,
            IProductMasterAttributeValueService productMasterAttributeValueService,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
               , IRepositoryAsync<ProductMasterEfficency> efficencyRepository
               , IRepositoryAsync<ProductMasterAlternativeUOM> materialMasterAlternativeUOM
            , IRepositoryAsync<ProductDefinition> productMasterRepository
            ) :
            base(charaterValueRepository, unitOfWork, pkGeneratorService)
        {
            _charaterValueRepository = charaterValueRepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _productMasterAttributeValueService = productMasterAttributeValueService;
            _sqlRepository = sqlRepository;
            _efficencyRepository = efficencyRepository;
            _materialMasterAlternativeUOM = materialMasterAlternativeUOM;
        }

        #endregion Constructor

        public decimal GetAutoSequence()
        {
            try
            {
                return base.Query(r => !r.Archive).Select().Max(r => r.Sequence + 1);
            }
            catch (Exception ex)
            {
                return 1.00M;
            }
        }

        public void Insert(ProductMaster entity, IEnumerable<ProductMasterAttributeValue> productMasterAttributeValue, IEnumerable<ProductMasterEfficency> efficencyList, IEnumerable<ProductMasterAlternativeUOM> materialMasterAlternativeUOM)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                CheckUnique(entity, identity.CompanyGroupId);
                _unitOfWork.BeginTransaction();
                flag = true;
                //if (productMasterAttributeValue != null)
                //{
                //var attrIds = productMasterAttributeValue.Select(r => r.MaterialAttributeValueId);
                //var attrFreeTexts = productMasterAttributeValue.Select(r => r.MaterialAttributeValueFreeText);
                //}
                if (!CheckUniqueRows(entity, productMasterAttributeValue))
                {
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        entity.Id = GetPK(identity.CompanyGroupId);
                        entity.CompanyGroupId = identity.CompanyGroupId;
                        InsertGraph(entity);
                    }
                    else
                    {
                        UpdateGraph(entity);
                    }
                }
                else
                    throw new CustomException("This data already exist....!");

                //******ProductMasterAttributeValue******//
                InsertOrUpdateProductMasterAttributeValue(entity.Id, identity.CompanyGroupId, productMasterAttributeValue);
                
                if (efficencyList != null)
                {
                    var count = _charaterValueRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ProductMasterEfficency] WHERE ProductMasterId='{entity.Id}'").First();
                    foreach (var item in efficencyList)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            count++;
                            item.Id = MakePK(entity.Id, count, 2);
                            item.ProductMasterId = entity.Id;
                            AuditService.AddedLog(item);
                            _efficencyRepository.Insert(item);
                        }
                        else
                        {
                            AuditService.UpdatedLog(item);
                            _efficencyRepository.Update(item);
                        }
                    }
                }

                //if (materialMasterAlternativeUOM!=null)
                //{
                    var dbList = _materialMasterAlternativeUOM.Query(t => t.ProductMasterId == entity.Id).Select().ToList();
                    if (materialMasterAlternativeUOM != null)
                    {
                        var pk = _pkGeneratorService.GetMaxNumber(nameof(ProductMasterAlternativeUOM), PKGeneratorEnum.Auto, null, DateTime.Now);
                        foreach (var item in materialMasterAlternativeUOM)
                        {
                            if (string.IsNullOrEmpty(item.Id))
                            {
                                pk.MaxNumber++;
                                item.Id = pk.MaxNumber.ToString();
                                item.ProductMasterId = entity.Id;
                                AuditService.AddedLog(item);
                                _materialMasterAlternativeUOM.Insert(item);
                            }
                            else if (!string.IsNullOrEmpty(item.Id))
                            {
                                AuditService.UpdatedLog(item);
                                _materialMasterAlternativeUOM.Update(item);
                            }
                       
                        }
                    }
                    if (dbList != null)
                    {
                        if (materialMasterAlternativeUOM == null)
                        {
                            foreach (var item in dbList)
                            {
                                _materialMasterAlternativeUOM.Delete(item);
                            }
                        }
                        else
                        {
                            foreach (var item in dbList)
                            {
                                if (!materialMasterAlternativeUOM.Any(t => t.Id == item.Id))
                                    _materialMasterAlternativeUOM.Delete(item);
                            }
                        }
                    }
                //}

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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void InsertOrUpdateProductMasterAttributeValue(string productMasterId, string companyGroupId, IEnumerable<ProductMasterAttributeValue> productMasterAttributeValue)
        {
            try
            {
                if (productMasterAttributeValue != null)
                {
                    string i = GetProductMasterAttributeValuePK(companyGroupId);
                    var count = 0;
                    foreach (var item in productMasterAttributeValue)
                    {
                        if (item.Id == null)//Insert
                        {
                            if (string.IsNullOrEmpty(item.MaterialAttributeValueId) && string.IsNullOrEmpty(item.MaterialAttributeValueFreeText))
                            {
                                //Do Nothing.
                            }
                            else
                            {
                                count++;
                                SetMaterialAttributeValueId(item);
                                item.Id = i + "-" + count;
                                item.ProductMasterId = productMasterId;
                                item.Active = true;
                                _productMasterAttributeValueService.InsertGraph(item);
                            }
                        }
                        else
                        {
                            //Edit
                            if (string.IsNullOrEmpty(item.MaterialAttributeValueId) && string.IsNullOrEmpty(item.MaterialAttributeValueFreeText))
                            {
                                //ProductMasterAttributeValue pMAttributeValue = _productMasterAttributeValueService.Find(item.Id);
                                //pMAttributeValue.Active = false;
                                //AuditService.Log(pMAttributeValue, true);
                                _productMasterAttributeValueService.Archive(item.Id);
                            }
                            else
                            {
                                SetMaterialAttributeValueId(item);
                                item.Active = true;
                                _productMasterAttributeValueService.UpdateGraph(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// This function check MaterialAttributeValueId null or not.
        /// </summary>
        /// <param name="item"></param>
        private static void SetMaterialAttributeValueId(ProductMasterAttributeValue item)
        {
            if (item.MaterialAttributeValueId != null)
            {
                item.MaterialAttributeValueFreeText = null;
            }
            else
            {
                if (item.MaterialAttributeValueFreeText == null)
                {
                    throw new Exception("Free Text can not be null");
                }
            }
        }

        private string GetPK(string companyGroupId)
        {
            return "PM-" + GetAutoNumber(nameof(ProductMaster), PKGeneratorEnum.Auto, companyGroupId, DateTime.Now);
        }

        private string GetProductMasterAttributeValuePK(string companyGroupId)
        {
            return _pkGeneratorService.GetAutoNumber(nameof(ProductMasterAttributeValue), PKGeneratorEnum.Auto, companyGroupId, DateTime.Now);
        }

        private void CheckUnique(ProductMaster entity, string companyGroupId)
        {
            CheckUniqueColumn(UniqueColumnName.Code, entity.Code, r => r.Id != entity.Id && r.Code == entity.Code && r.CompanyGroupId == companyGroupId && !r.Archive);
            CheckUniqueColumn(UniqueColumnName.UserName, entity.UserName, r => r.Id != entity.Id && r.UserName == entity.UserName && r.CompanyGroupId == companyGroupId && !r.Archive);
        }

        private bool CheckUniqueRows(ProductMaster entity, IEnumerable<ProductMasterAttributeValue> productMasterAttributeValue)
        {
            try
            {
                if (productMasterAttributeValue != null)
                {
                    DataTable dtChild = null;
                    DataView dvChild = null;
                    DataSet ds = FromDb(entity);
                    using (var dataView = new DataView(ds.Tables[0]))
                    {
                        DataTable dtDistinctList = dataView.ToTable(true, "Id");
                        for (int i = 0; i < dtDistinctList.Rows.Count; i++)
                        {
                            var pId = dtDistinctList.Rows[i]["Id"];
                            using (dvChild = new DataView(ds.Tables[0])
                            {
                                RowFilter = "Id='" + pId + "'"
                            })
                            {
                                dtChild = dvChild.ToTable();
                                var _isAvialable = IsAvialable(dtChild, productMasterAttributeValue);
                                if (_isAvialable)
                                    return true;
                            }
                        }
                        return false;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataSet FromDb(ProductMaster entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var parameters = new GridParameter
                {
                    ExportType = "DATASET"
                };
                parameters.CmdText = "SELECT P.Id,PM.MaterialAttributeValueId,PM.MaterialAttributeValueFreeText " +
                                $"FROM {DbSchema.Masters}.[{DbTable.ProductMaster}] AS p  " +
                                $"LEFT OUTER JOIN {DbSchema.Masters}.[{DbTable.ProductMasterAttributeValue}] AS PM ON PM.ProductMasterId=P.Id " +
                                $"WHERE P.Id<>'{entity.Id}' AND P.ProductCategoryId='{entity.ProductCategoryId}' AND P.ProductSubCategoryId='{entity.ProductSubCategoryId}' AND P.CompanyGroupId='{identity.CompanyGroupId}'";
                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static bool IsAvialable(DataTable dt, IEnumerable<ProductMasterAttributeValue> pmAttributeValue)
        {
            var totalCount = 0;
            DataTable dtbl = null;
            DataView dv = null;
            try
            {
                if (dt.Rows.Count != pmAttributeValue.Count())
                    return false;
                else
                {
                    foreach (var item in pmAttributeValue)
                    {
                        var mId = item.MaterialAttributeValueId;
                        var mValue = item.MaterialAttributeValueFreeText;
                        using (dv = new DataView(dt))
                        {
                            if (!string.IsNullOrEmpty(mId))
                            {
                                dv.RowFilter = "MaterialAttributeValueId='" + mId + "'";
                                dtbl = dv.ToTable();
                                if (dtbl.Rows.Count > 0)
                                {
                                    totalCount += 1;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                dv.RowFilter = "MaterialAttributeValueFreeText='" + mValue + "'";
                                dtbl = dv.ToTable();
                                if (dtbl.Rows.Count > 0)
                                {
                                    totalCount += 1;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    return totalCount == dt.Rows.Count;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override void Archive(string key)
        {
            var flag = false;
            try
            {
                CheckIdUse(key);
                _unitOfWork.BeginTransaction();
                flag = true;
                _productMasterAttributeValueService.DeleteGraph(key);
                DeleteGraph(key);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel GetCbo(string companyGroupId)
        {
            try
            {
                var sql = @"SELECT PM.Id AS Value, PM.UserName AS Text,PM.BaseUOMId
                            FROM MST.ProductMaster AS PM
                            WHERE PM.CompanyGroupId = '"+ companyGroupId + "' AND PM.Archive = 0 AND PM.Active = 1";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public GridModel GetPMCbo()
        {
            try
            {
                var sql = @"SELECT PM.Id AS Value, PM.UserName AS Text,PM.BaseUOMId
                            FROM MST.ProductMaster AS PM
                            Where PM.Id NOT IN(SELECT ProductMasterId FROM TRN.ProductDefinition) AND Active=1";

                return _sqlRepository.GetGridData(new GridParameter { CmdText = sql });
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT PM.*, PC.UserName AS ProductCategoryName, PSC.UserName AS ProductSubCategoryName, P.UserName AS ProductName, PR.UserName BaseProcess, UOMB.UserName AS BaseUom 
                                        FROM MST.[ProductMaster] AS PM 
                                        LEFT OUTER JOIN HKP.[ProductCategory] AS PC ON PC.Id = PM.ProductCategoryId
                                        LEFT OUTER JOIN HKP.[ProductSubCategory] AS PSC ON PSC.Id = PM.ProductSubCategoryId
                                        LEFT OUTER JOIN HKP.[Product] AS P ON P.Id = PM.ProductId
                                        LEFT OUTER JOIN HKP.[Process] AS PR ON PR.Id = PM.BaseProcessId
                                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UOMB ON PM.BaseUOMId = UOMB.Id
                                        WHERE PM.CompanyGroupId = '" + identity.CompanyGroupId+"' AND PM.Archive = 0";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        //This data only show in Material master
        public IEnumerable<object> ProductMasterWithDetails(string productMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = "SELECT PC.UserName AS ProductCategoryName, " +
                                      "PSC.UserName AS ProductSubCategoryName, " +
                                      "P.UserName AS ProductName, " +
                                      "MA.UserName AS MaterialAttributeName, " +
                                      "ISNULL(MAV.[Description],'') + ISNULL( PMAV.MaterialAttributeValueFreeText,'') AS MaterialAttributeValue " +
                               "FROM MST.ProductMasterAttributeValue AS PMAV " +
                               "INNER JOIN MST.ProductMaster AS PM ON PM.Id=PMAV.ProductMasterId " +
                               "LEFT OUTER JOIN HKP.ProductCategory AS PC ON PM.ProductCategoryId=PC.Id " +
                               "LEFT OUTER JOIN HKP.ProductSubCategory AS PSC ON PM.ProductSubCategoryId=PSC.Id " +
                               "LEFT OUTER JOIN HKP.Product AS P ON PM.ProductId=P.Id " +
                               "LEFT OUTER JOIN HKP.MaterialAttribute AS MA ON PMAV.MaterialAttributeId=MA.Id " +
                               "LEFT OUTER JOIN HKP.MaterialAttributeValue AS MAV ON PMAV.MaterialAttributeValueId=MAV.Id " +
                               $"WHERE PMAV.ProductMasterId='{productMasterId}' AND PMAV.Archive=0 AND PM.CompanyGroupId='{identity.CompanyGroupId}' ";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> ProductMasterComminationData(string productMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = @"SELECT PC.UserName AS ProductCategoryName
                                      ,PSC.UserName AS ProductSubCategoryName
                                      ,P.UserName AS ProductName
                                    FROM MST.ProductMaster AS PM
                                    LEFT OUTER JOIN HKP.ProductCategory AS PC ON PM.ProductCategoryId = PC.Id
                                    LEFT OUTER JOIN HKP.ProductSubCategory AS PSC ON PM.ProductSubCategoryId = PSC.Id
                                    LEFT OUTER JOIN HKP.Product AS P ON PM.ProductId = P.Id
                                    WHERE PM.Id = '" + productMasterId + "' AND PM.CompanyGroupId = '" + identity.CompanyGroupId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        private void CheckIdUse(string id)
        {
            string sql = $"IF EXISTS(SELECT 1 FROM( " +
                            $"SELECT ProductMasterId AS CheckingColumn FROM [{DbSchema.Masters}].[{DbTable.MaterialMaster}] WHERE Archive=0 " +
                            $") A WHERE CheckingColumn = '{id}') SELECT 1 ELSE SELECT 0 RETURN ";
            var data = Convert.ToBoolean(_charaterValueRepository.SqlQuery<int>(sql).Single());
            if (data)
                throw new CustomException("Already attribute exist in material master, you can't delete....!");
        }

        #region ProductMasterEfficency

        public IEnumerable<ProductMasterEfficency> GetEfficencyList(string masterId)
        {
            try
            {
                var data = new List<ProductMasterEfficency>();
                if (masterId != "undefined")
                {
                    var sql = @"SELECT A.Id, A.ProductMasterId, A.ColumnSequence, A.EfficencyName, A.SPT
                            , A.NoOfWorkStation, A.EfficencyPercentage,  A.StandardWorkingHours,A.StandardWorkingHourCost,A.AdditionalWorkingHourCostPerHour,A.ValueLossPercentage,
                            A.AddedBy, A.AddedDate, A.AddedFromIP, A.UpdatedBy, A.UpdatedDate, A.UpdatedFromIP
                            FROM TRN.ProductMasterEfficency AS A WHERE A.ProductMasterId='" + masterId + @"' ORDER BY A.ColumnSequence";
                    data = _efficencyRepository.SqlQuery<ProductMasterEfficency>(sql).ToList();
                    if (data.Count==0)
                    {
                        data = CreateTable(masterId);
                    }
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
        private List<ProductMasterEfficency> CreateTable(string masterId)
        {
            try
            {
                var list = new List<ProductMasterEfficency>();
                var seq = 1;
                foreach (var item in Enum.GetValues(typeof(ProductEfficency)))
                {
                    var model = new ProductMasterEfficency
                    {
                        Id = null,
                        ProductMasterId = null,
                        SPT = 0,
                        ColumnSequence = seq,
                        EfficencyName = item.ToString(),
                        NoOfWorkStation = 0,
                        EfficencyPercentage = 0,
                        StandardWorkingHours = 0,
                        StandardWorkingHourCost = 0,
                        AdditionalWorkingHourCostPerHour = 0,
                        ValueLossPercentage = 0,
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

        #endregion ProductMasterEfficency

        #region AltUom
        public IEnumerable<object> GetProductMasterAltUomList(string productMasterId)
        {
            try
            {
                var _sql = @"SELECT MMAU.*, UOMA.UserName AS AlternativeUOMName,UOMB.UserName AS BaseUOMName
                            FROM MST.ProductMasterAlternativeUOM AS MMAU 
                            LEFT OUTER JOIN SCS.[UnitOfMeasurement] AS UOMA ON MMAU.AlternativeUOMId=UOMA.Id 
                            LEFT OUTER JOIN SCS.[UnitOfMeasurement] AS UOMB ON MMAU.BaseUOMId=UOMB.Id 
                            WHERE MMAU.Archive = 0 AND MMAU.ProductMasterId = '" + productMasterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        } 
        #endregion
    }
}