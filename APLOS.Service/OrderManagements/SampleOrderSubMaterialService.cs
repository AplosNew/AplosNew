#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Materials;
using Library.Service.Setups;
using Library.Service.Systems;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class SampleOrderSubMaterialService : Service<SampleOrderSubMaterial>, ISampleOrderSubMaterialService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfMeasurementService _uomService;
        private readonly ISampleOrderSubMaterialValueService _sampleOrderSubMaterialValueService;
        private readonly IMaterialAttributeValueService _materialAttributeValueService;
        private readonly IMaterialGroupMasterService _materialGroupMasterService;
        private readonly IRepositoryAsync<SampleOrderSubMaterial> _sampleOrderRepository;

        public SampleOrderSubMaterialService(
            IRepositoryAsync<SampleOrderSubMaterial> sampleOrderRepository
            , ISampleOrderSubMaterialValueService sampleOrderSubMaterialValueService
            , IPKGeneratorService pkGeneratorService
            , IMaterialAttributeValueService materialAttributeValueService
            , IMaterialGroupMasterService materialGroupMasterService
            , IUnitOfMeasurementService uomService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(sampleOrderRepository, unitOfWork, pkGeneratorService)
        {
            _sampleOrderRepository = sampleOrderRepository;
            _uomService = uomService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _sampleOrderSubMaterialValueService = sampleOrderSubMaterialValueService;
            _materialAttributeValueService = materialAttributeValueService;
            _materialGroupMasterService = materialGroupMasterService;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber("SampleOrderValue", PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public IEnumerable<object> Query(string masterId)
        {
            try
            {
                var dataList = base.Query(t => t.SampleOrderId == masterId).Include(t => t.MaterialGroupMaster)
                    .Include(t => t.MaterialMaster)
                    .Include(t => t.Article)
                    .Include(t => t.Currency)
                    .Include(t => t.UoM)
                    .Include(t => t.MaterialAttributeValues.Select(a => a.MaterialAttribute)).Select();
                var listData = new List<object>();
                foreach (var item in dataList)
                {
                    foreach (var child in item.MaterialAttributeValues)
                    {
                        if (!string.IsNullOrEmpty(child.MaterialAttributeId))
                        {
                            if (!string.IsNullOrEmpty(child.MaterialAttributeValueId))
                                child.MaterialAttributeValueFreeText = _materialAttributeValueService
                                    .Query(t => t.MaterialAttributeId == child.MaterialAttributeId && t.Id == child.MaterialAttributeValueId)
                                    .Select(t => t.Description).FirstOrDefault();
                            else
                                child.MaterialAttributeValueFreeText = _sampleOrderSubMaterialValueService.Query(t => t.Id == child.Id).Select(t => t.MaterialAttributeValueFreeText).First();
                        }
                    }
                    var row = new
                    {
                        item.Id,
                        item.SampleOrderId,
                        item.MaterialMasterId,
                        MaterialMasterName = string.IsNullOrEmpty(item.MaterialMasterId) ? null : item.MaterialMaster.UserName,
                        item.ArticleId,
                        ArticleName = string.IsNullOrEmpty(item.ArticleId) ? null : item.Article.StandardName,
                        item.MaterialGroupMasterId,
                        MaterialGroupMasterName = item.MaterialGroupMaster.UserName,
                        item.UoMId,
                        UoMName = item.UoM.UserName,
                        item.CurrencyId,
                        CurrencyName = item.Currency.Code,
                        item.TestingStandardId,
                        item.Name,
                        item.Qty,
                        item.Rate,
                        DeliveryDate = item.DeliveryDate.ToString("dd-MMM-yyyy"),
                        item.Remarks,
                        item.IsConfirmed,
                        item.MaterialAttributeValues,
                    };
                    listData.Add(row);
                }
                return listData;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        /// <summary>
        /// for sample pending
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public GridModel GetPendingSampleOrderList(GridParameter parameters, string entityId)
        {
            try
            {
                parameters.order = "asc";
                parameters.sort = "MaterialGroupMasterName, MaterialMasterName, ArticleName";
                parameters.CmdText = @"SELECT SOSM.Id,P.UserName AS PartyName
	                                         ,SOSM.SampleOrderId
	                                         ,SOSM.MaterialGroupMasterId ,MGM.UserName AS MaterialGroupMasterName
	                                         ,SOSM.MaterialMasterId ,MM.UserName AS MaterialMasterName
	                                         ,SOSM.ArticleId,SM.StandardName AS ArticleName
	                                         ,SOSM.UoMId ,UoM.UserName AS UoM
	                                         ,SOSM.CurrencyId ,CU.Code AS CurrencyName
	                                         ,SOSM.Name ,SOSM.Qty ,SOSM.Rate
	                                         ,DeliveryDate=REPLACE(CONVERT(CHAR(11), SOSM.DeliveryDate, 106),' ','-')
	                                         ,SOSM.IsConfirmed
	                                         ,RequestReferenceDate=REPLACE(CONVERT(CHAR(11), SO.RequestReferenceDate, 106),' ','-')
	                                         ,SO.ReferenceDocNo
                                             ,ISNULL(CV1.Description,'')+' '+ISNULL(CV2.Description,'')+' '+ ISNULL(CV3.Description,'') Detail
	                                         ,CAST(0 as BIT) AS Flag
                                    FROM TRN.SampleOrderSubMaterial AS SOSM
                                    LEFT OUTER JOIN TRN.SampleOrder AS SO ON SOSM.SampleOrderId=SO.Id
                                    LEFT OUTER JOIN HKP.Party AS P ON SO.PartyId=P.Id
                                    LEFT OUTER JOIN MST.MaterialGroupMaster AS MGM ON SOSM.MaterialGroupMasterId=MGM.Id
                                    LEFT OUTER JOIN MST.MaterialMaster AS MM ON SOSM.MaterialMasterId=MM.Id
                                    LEFT OUTER JOIN MST.MaterialMasterArticle AS SM ON SOSM.ArticleId=SM.Id
                                    LEFT OUTER JOIN SCS.UnitOfMeasurement AS UoM ON SOSM.UoMId=UoM.Id
                                    LEFT OUTER JOIN SCS.Currency AS CU ON SOSM.CurrencyId=CU.Id

                                    LEFT OUTER JOIN HKP.Characteristics AS CH1 ON SOSM.FirstCharacteristicsId=CH1.Id
                                    LEFT OUTER JOIN HKP.Characteristics AS CH2 ON SOSM.SecondCharacteristicsId=CH2.Id
                                    LEFT OUTER JOIN HKP.Characteristics AS CH3 ON SOSM.ThirdCharacteristicsId=CH3.Id

                                    LEFT OUTER JOIN HKP.CharacteristicsValue AS CV1 ON SOSM.FirstCharacteristicsValueId=CH1.Id
                                    LEFT OUTER JOIN HKP.CharacteristicsValue AS CV2 ON SOSM.SecondCharacteristicsValueId=CH2.Id
                                    LEFT OUTER JOIN HKP.CharacteristicsValue AS CV3 ON SOSM.ThirdCharacteristicsValueId=CH3.Id
                                    WHERE SO.EntityId='" + entityId + "' AND SOSM.Id NOT IN (SELECT SampleOrderSubMaterialId FROM TRN.SamplePackingListMaterialDetails)";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel GetMaterialList(GridParameter parameters, string materialGroupId)
        {
            try
            {
                //parameters.CmdText = @"SELECT MM.MaterialGroupMasterId,MGM.UserName AS MaterialGroupMaster,MM.Id AS MaterialMasterId,MM.UserName AS MaterialMaster, SM.Id AS SubMaterialId, SM.StandardName AS ArticleName
                //                         ,MM.MaterialGridId,MG.[Description] AS MaterialGridNo
                //                            ,MM.BaseUOMId,UoM.UserName AS UoM, OS.UserName AS OurStyle,Mt.IsOurStyleRequired
                //                         ,FirstCharacteristicsId=(SELECT MGC.CharacteristicsId FROM HKP.MaterialGrid AS MG
                //                                LEFT OUTER JOIN HKP.MaterialGridCharacteristics AS MGC ON MGC.MaterialGridId=MG.Id WHERE MG.Id=MM.MaterialGridId AND MGC.Sort='1')
                //                         ,Characteristics1=(SELECT CH.Alias FROM HKP.MaterialGrid AS MG
                //                                LEFT OUTER JOIN HKP.MaterialGridCharacteristics AS MGC ON MGC.MaterialGridId=MG.Id
                //                          LEFT OUTER JOIN HKP.Characteristics AS CH ON MGC.CharacteristicsId=CH.Id WHERE MG.Id=MM.MaterialGridId AND MGC.Sort='1')
                //                         ,SecondCharacteristicsId=(SELECT MGC.CharacteristicsId FROM HKP.MaterialGrid AS MG
                //                                LEFT OUTER JOIN HKP.MaterialGridCharacteristics AS MGC ON MGC.MaterialGridId=MG.Id WHERE MG.Id=MM.MaterialGridId AND MGC.Sort='2')
                //                         ,Characteristics2=(SELECT CH.Alias FROM HKP.MaterialGrid AS MG
                //                                LEFT OUTER JOIN HKP.MaterialGridCharacteristics AS MGC ON MGC.MaterialGridId=MG.Id
                //                          LEFT OUTER JOIN HKP.Characteristics AS CH ON MGC.CharacteristicsId=CH.Id WHERE MG.Id=MM.MaterialGridId AND MGC.Sort='2')
                //                         ,ThirdCharacteristicsId=(SELECT MGC.CharacteristicsId FROM HKP.MaterialGrid AS MG
                //                                LEFT OUTER JOIN HKP.MaterialGridCharacteristics AS MGC ON MGC.MaterialGridId=MG.Id WHERE MG.Id=MM.MaterialGridId AND MGC.Sort='3')
                //                         ,Characteristics3=(SELECT CH.Alias FROM HKP.MaterialGrid AS MG
                //                                LEFT OUTER JOIN HKP.MaterialGridCharacteristics AS MGC ON MGC.MaterialGridId=MG.Id
                //                          LEFT OUTER JOIN HKP.Characteristics AS CH ON MGC.CharacteristicsId=CH.Id WHERE MG.Id=MM.MaterialGridId AND MGC.Sort='3')
                //                    FROM MST.MaterialMaster AS MM
                //                    LEFT OUTER JOIN HKP.MaterialType AS MT ON MM.MaterialTypeId=MT.Id
                //                    LEFT OUTER JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                //                    LEFT OUTER JOIN MST.MaterialMasterArticle AS SM ON SM.MaterialMasterId=MM.Id
                //                    LEFT OUTER JOIN HKP.MaterialGrid AS MG ON MM.MaterialGridId=MG.Id
                //                    LEFT OUTER JOIN SCS.UnitOfMeasurement AS UoM ON MM.BaseUOMId=UoM.Id
                //                    LEFT OUTER JOIN HKP.OurStyle AS OS ON MM.OurStyleId=OS.Id
                //                        WHERE MM.MaterialGroupMasterId='" + materialGroupId + "'";
                parameters.CmdText = @"SELECT DISTINCT MM.MaterialGroupMasterId, MM.HSNCodeId, MGP.UserName AS MaterialGroupMasterName, MT.Description AS MaterialTypeName, MM.Id ,MM.Code ,MM.UserName, MM.StandardName, MM.ShortName
                                        , MM.BaseUOMId, UOMB.UserName AS BaseUoM, MT.IsProductMstRequired, PM.UserName AS ProductMasterName
                                        , MT.IsOurStyleRequired, OS.UserName AS OurStyleName, MM.WithSKU, ISNULL(ART.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
                                        , hasInventory=CASE WHEN IM.Id<>'' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MM.IsOriginApplicable
                                FROM [MST].[MaterialMaster] AS MM
                                LEFT JOIN [TRN].ProductDefinition AS PD ON PD.MaterialMasterId= MM.Id
                                LEFT JOIN[HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
                                LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
                                LEFT JOIN [MST].[ProductMaster] AS PM ON PD.ProductMasterId = PM.Id
                                LEFT JOIN hkp.ProductCategory pc on pc.Id=pm.ProductCategoryId
                                LEFT JOIN hkp.ProductSubCategory psc on psc.Id=pm.ProductSubCategoryId
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS UOMB ON MM.BaseUOMId = UOMB.Id
                                LEFT JOIN [HKP].OurStyle AS OS ON PD.OurStyleId= OS.Id
                                LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0THEN COUNT(MaterialMasterId) ELSE 0 END
                                                , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
                                            FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS ART ON ART.MaterialMasterId=MM.Id
                                LEFT JOIN TRN.InventoryMaterial AS IM ON IM.MaterialMasterId=MM.Id
                                WHERE MM.MaterialGroupMasterId='"+ materialGroupId + "' AND MM.Archive=0 AND MM.Active=1 ";
                    return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public IEnumerable<object> GetPendingList(string[] ids)
        {
            try
            {
                var _sql = @"SELECT SOSM.Id
	                               ,SOSM.SampleOrderId
	                               ,SOSM.MaterialGroupMasterId ,MGM.UserName AS MaterialGroupMasterName
	                               ,SOSM.MaterialMasterId ,MM.UserName AS MaterialMasterName
	                               ,SOSM.ArticleId,SM.StandardName AS ArticleName
	                               ,SOSM.UoMId ,UoM.UserName AS UoM
	                               ,SOSM.CurrencyId ,CU.Code AS CurrencyName
	                               ,SOSM.Name ,SOSM.Qty ,SOSM.Rate
	                               ,DeliveryDate=REPLACE(CONVERT(CHAR(11), SOSM.DeliveryDate, 106),' ','-')
	                               ,SOSM.IsConfirmed
	                               ,RequestReferenceDate=REPLACE(CONVERT(CHAR(11), SO.RequestReferenceDate, 106),' ','-')
	                               ,SO.ReferenceDocNo
                                   ,ISNULL(CV1.Description,'')+' '+ISNULL(CV2.Description,'')+' '+ ISNULL(CV3.Description,'') Detail
                            FROM TRN.SampleOrderSubMaterial AS SOSM
                            LEFT OUTER JOIN TRN.SampleOrder AS SO ON SOSM.SampleOrderId=SO.Id
                            LEFT OUTER JOIN MST.MaterialGroupMaster AS MGM ON SOSM.MaterialGroupMasterId=MGM.Id
                            LEFT OUTER JOIN MST.MaterialMaster AS MM ON SOSM.MaterialMasterId=MM.Id
                            LEFT OUTER JOIN MST.MaterialMasterArticle AS SM ON SOSM.ArticleId=SM.Id
                            LEFT OUTER JOIN SCS.UnitOfMeasurement AS UoM ON SOSM.UoMId=UoM.Id
                            LEFT OUTER JOIN SCS.Currency AS CU ON SOSM.CurrencyId=CU.Id

                            LEFT OUTER JOIN HKP.Characteristics AS CH1 ON SOSM.FirstCharacteristicsId=CH1.Id
                            LEFT OUTER JOIN HKP.Characteristics AS CH2 ON SOSM.SecondCharacteristicsId=CH2.Id
                            LEFT OUTER JOIN HKP.Characteristics AS CH3 ON SOSM.ThirdCharacteristicsId=CH3.Id

                            LEFT OUTER JOIN HKP.CharacteristicsValue AS CV1 ON SOSM.FirstCharacteristicsValueId=CV1.Id
                            LEFT OUTER JOIN HKP.CharacteristicsValue AS CV2 ON SOSM.SecondCharacteristicsValueId=CV2.Id
                            LEFT OUTER JOIN HKP.CharacteristicsValue AS CV3 ON SOSM.ThirdCharacteristicsValueId=CV3.Id
                            WHERE SOSM.Id IN(" + ReturnStringArray(ids) + ") ORDER BY MaterialGroupMasterName, MaterialMasterName, ArticleName";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void InsertGraph(string masterId, IEnumerable<SampleOrderSubMaterial> entities)
        {
            try
            {
                if (entities != null)
                {
                    //var dbValueList = GetAttributeValueList(entity.Select(t => t.MaterialGroupMasterId).First());
                    var pk = GetMaxNumber(nameof(SampleOrderSubMaterial), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in entities)
                    {
                        var localList = item.MaterialAttributeValues.ToList();
                        foreach (var hala in localList)
                        {
                            if (string.IsNullOrEmpty(hala.MaterialAttributeValueId) &&
                                string.IsNullOrEmpty(hala.MaterialAttributeValueFreeText))
                            {
                                item.MaterialAttributeValues.Remove(hala);
                            }
                        }
                        //IfMaterialAttributeValueExist(item, dbValueList);
                        pk.MaxNumber++;
                        item.Id = pk.MaxNumber.ToString();
                        item.SampleOrderId = masterId;
                        _sampleOrderSubMaterialValueService.InsertOrUpdateGraph(masterId, item);
                        base.InsertGraph(item);
                    }
                }
                //else
                //    //string materialGroupId = entity.First().MaterialGroupMasterId;
                //    //IfMaterialAttributeExist(materialGroupId);
                //    throw new CustomException("Can not save without material group");
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void InsertOrUpdateGraph(string masterId, IEnumerable<SampleOrderSubMaterial> subMaterials)
        {
            try
            {
                var dbList = base.Query(t => t.SampleOrderId == masterId).Include(t => t.MaterialAttributeValues).Select().ToList();
                if (subMaterials != null)
                {
                    //var uomIds = subMaterials.Where(t => t.MaterialMasterId != null).Select(t => t.UoMId).ToArray();
                    var materialMasterIds = subMaterials.Where(t => t.MaterialMasterId != null).Select(t => t.MaterialMasterId).ToArray();
                    var uom_Data = GetMaterialMasterUom(materialMasterIds);
                    //throw new CustomException("Please insert atleast one material attribute.............!");
                    //var dbValueList = GetAttributeValueList(subMaterials.Select(t => t.MaterialGroupMasterId).FirstOrDefault());
                    var pk = GetMaxNumber(nameof(SampleOrderSubMaterial), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var item in subMaterials)
                    {
                        //IfMaterialAttributeValueExist(item, dbValueList);
                        if (item.Id.StartsWith("n-"))
                        {
                            var localList = item.MaterialAttributeValues.ToList();
                            foreach (var hala in localList)
                            {
                                if (string.IsNullOrEmpty(hala.MaterialAttributeValueId) &&
                                    string.IsNullOrEmpty(hala.MaterialAttributeValueFreeText))
                                {
                                    item.MaterialAttributeValues.Remove(hala);
                                }
                            }
                            pk.MaxNumber++;
                            item.Id = pk.MaxNumber.ToString();
                            item.SampleOrderId = masterId;
                            _sampleOrderSubMaterialValueService.InsertOrUpdateGraph(masterId, item);
                            base.InsertGraph(item);
                        }
                        else
                        {
                            _sampleOrderSubMaterialValueService.InsertOrUpdateGraph(masterId, item);
                            if (!string.IsNullOrEmpty(item.MaterialMasterId))
                            {
                                if (!UomIdExistInMaterialMasterForUpdating(uom_Data, item))
                                {
                                    var uomName = _uomService.Query(t => t.Id == item.UoMId).Select(t => t.UserName).FirstOrDefault();
                                    throw new CustomException("This UoM " + uomName + " is not exist in material master!");
                                }
                            }
                            UpdateGraph(item);
                        }
                    }
                }
                if (dbList.Count() > 0)
                {
                    if (subMaterials == null)
                    {
                        foreach (var item in dbList)
                        {
                            _sampleOrderSubMaterialValueService.DeleteGraph(item);
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!subMaterials.Any(t => t.Id == item.Id))
                            {
                                _sampleOrderSubMaterialValueService.DeleteGraph(item);
                                base.DeleteGraph(item);
                            }
                        }
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private static bool UomIdExistInMaterialMasterForUpdating(IEnumerable<SampleOrderViewModel> uom_Data, SampleOrderSubMaterial item)
        {
            var flag = false;
            foreach (var it in uom_Data)
            {
                if (item.MaterialMasterId == it.MaterialMasterId && item.UoMId == it.UoMId)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        public void DeleteGraph(string masterId)
        {
            var dbList = base.Query(t => t.SampleOrderId == masterId).Include(t => t.MaterialAttributeValues).Select().AsEnumerable();
            if (dbList != null)
            {
                foreach (var item in dbList)
                {
                    _sampleOrderSubMaterialValueService.DeleteGraph(item);
                    base.DeleteGraph(item);
                }
            }
        }

        public IEnumerable<SampleOrderSubMaterialValue> GetAttributeValueList(string materialGroupId)
        {
            string _sql = @"SELECT MV.Id
                                  ,SampleOrderId
	                              ,MV.SampleOrderSubMaterialId
	                              ,MV.MaterialAttributeId
	                              ,MV.MaterialAttributeValueId
	                              ,MaterialAttributeValueFreeText= CASE WHEN MV.MaterialAttributeValueId<> '' THEN MAV.[Description]
									                               ELSE MV.MaterialAttributeValueFreeText END
                                  ,MV.AddedBy,MV.AddedDate,MV.AddedFromIP
	                              ,MV.UpdatedBy,MV.UpdatedDate,MV.UpdatedFromIP
                            FROM TRN.SampleOrderSubMaterialValue AS MV
                            LEFT OUTER JOIN HKP.MaterialAttributeValue AS MAV ON MAV.Id=MV.MaterialAttributeValueId
                            WHERE MV.SampleOrderSubMaterialId IN(SELECT Id FROM TRN.SampleOrderSubMaterial WHERE MaterialGroupMasterId='" + materialGroupId + "')";
            return _sampleOrderRepository.SqlQuery<SampleOrderSubMaterialValue>(_sql).AsEnumerable();
        }

        private void IfMaterialAttributeExist(string id)
        {
            string sql = @"IF EXISTS(SELECT 1 FROM(
                                SELECT MaterialGroupMasterId AS CheckingColumn FROM MST.MaterialAttributeMaster WHERE Archive=0
                                ) A WHERE CheckingColumn = '" + id + "') SELECT 1 ELSE SELECT 0 RETURN ";
            var data = Convert.ToBoolean(_sampleOrderRepository.SqlQuery<int>(sql).Single());
            if (data)
                throw new CustomException("Please insert at least one sub-material....!");
        }

        private void IfMaterialAttributeValueExist(SampleOrderSubMaterial entity, IEnumerable<SampleOrderSubMaterialValue> dbMValue)
        {
            try
            {
                if (dbMValue != null && entity.MaterialAttributeValues != null)
                {
                    var attrList = entity.MaterialAttributeValues.Select(t => t.MaterialAttributeId).ToList();
                    var valuesList = dbMValue.Where(t => t.SampleOrderSubMaterialId != entity.Id).Select(t => t.SampleOrderSubMaterialId).ToList().Distinct();
                    foreach (var item in valuesList)//Article List
                    {
                        var mvUI = entity.MaterialAttributeValues.ToList();
                        var mvDB = dbMValue.Where(t => t.SampleOrderSubMaterialId == item).AsEnumerable();
                        var count = 0;
                        for (int i = 0; i < attrList.Count; i++)//Attribute List
                        {
                            count += MaterialValueValidation(mvUI, mvDB, attrList[i]);
                        }//Attribute List
                        if (attrList.Count == count)
                            throw new CustomException("Sub-Material [" + entity.Name + "] exist........!");
                    }//Article List
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private int MaterialValueValidation(IEnumerable<SampleOrderSubMaterialValue> mvUI, IEnumerable<SampleOrderSubMaterialValue> mvDB, string entityId)
        {
            try
            {
                var attrValueUi = mvUI.Where(t => t.MaterialAttributeId == entityId).Select(t => t.MaterialAttributeValueFreeText).FirstOrDefault();
                var attrValueDb = mvDB.Where(t => t.MaterialAttributeId == entityId).Select(t => t.MaterialAttributeValueFreeText).FirstOrDefault();
                if (attrValueUi != null && attrValueDb != null && attrValueUi.ToUpper() == attrValueDb.ToUpper())
                    return 1;
                else
                    return 0;
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public GridModel GetMaterialMasterByCustomer(GridParameter parameters, string partyId, string[] sampleOrderSubMaterialIds)
        {
            try
            {
                parameters.order = "asc";
                parameters.sort = "UserName";
                parameters.CmdText = @"SELECT MM.Id AS MaterialMasterId,'' AS Flag ,SOSM.MaterialGroupMasterId,MGM.UserName AS MaterialGroupMaster
										,MM.Code ,MM.UserName ,MM.StandardName ,MM.ShortName ,SOSM.ArticleId ,SM.StandardName AS ArticleName
										,RequestReferenceDate=REPLACE(CONVERT(CHAR(11), SO.RequestReferenceDate, 106),' ','-')
										,SO.Id AS SampleOrderId, SO.ReferenceDocNo,SOSM.Id AS SampleOrderSubMaterialId,SOSM.Name AS SampleSubMaterial
										,SOSM.UoMId,UoM.UserName AS UoMName ,SOSM.CurrencyId ,CU.Code AS CurrencyName,SOSM.Rate,SOSM.Qty AS OrderQty
										,DeliveryDate=REPLACE(CONVERT(CHAR(11), SOSM.DeliveryDate, 106),' ','-')
										,ISNULL(CV1.Description,'')+' '+ISNULL(CV2.Description,'')+' '+ ISNULL(CV3.Description,'') Detail
										,SOSM.FirstCharacteristicsId,SOSM.SecondCharacteristicsId,SOSM.ThirdCharacteristicsId
										,SOSM.FirstCharacteristicsValueId,SOSM.SecondCharacteristicsValueId,SOSM.ThirdCharacteristicsValueId
										,PF.PackingFormId AS PackingFormId1
										,IsSingleEntry=CASE WHEN ISNULL(PF.IsSingleEntry,'')='' THEN CAST(0 AS BIT) ELSE CAST(PF.IsSingleEntry AS BIT) END
										,PF2.PackingFormId AS PackingFormId2
										,[Count]=(SELECT COALESCE(COUNT(DISTINCT Id),0) FROM MST.MaterialGroupPackingForm WHERE MaterialGroupMasterId=MGM.Id)
										,PendingQty=CASE WHEN (PACK.BaseUOMId<>'' AND PACK.BaseUOMId=SOSM.UoMId) THEN (SOSM.Qty-PACK.BaseQty)
												WHEN (PACK.BaseUOMId<>'' AND PACK.BaseUOMId<>SOSM.UoMId) THEN ((PACK.BaseUOMFactor*SOSM.Qty)-PACK.BaseQty)/PACK.BaseUOMFactor
												ELSE SOSM.Qty END
										--,PACK.SamplePackingListMaterialId
									FROM TRN.SampleOrderSubMaterial AS SOSM
									LEFT JOIN TRN.SampleOrder AS SO ON SOSM.SampleOrderId=SO.Id
									LEFT JOIN MST.MaterialGroupMaster AS MGM ON SOSM.MaterialGroupMasterId=MGM.Id
									LEFT JOIN MST.MaterialMaster AS MM ON SOSM.MaterialMasterId=MM.Id
									LEFT JOIN (SELECT A.BaseUOMFactor,SUM(B.BaseQty) AS BaseQty,B.SampleOrderSubMaterialId,B.BaseUOMId,B.SampleOrderId,B.MaterialMasterId FROM MST.MaterialMasterAlternativeUOM AS A
											INNER JOIN TRN.SamplePackingListMaterialDetails AS B ON A.MaterialMasterId=B.MaterialMasterId AND A.BaseUOMId=B.BaseUOMId
											WHERE A.MaterialMasterId IN(SELECT DISTINCT MaterialMasterId FROM TRN.SampleOrderSubMaterial WHERE MaterialMasterId<>'')
											AND A.AlternativeUOMId IN(SELECT UoMId FROM TRN.SampleOrderSubMaterial WHERE MaterialMasterId<>'') AND A.BaseUOMId=B.BaseUOMId
											GROUP BY A.BaseUOMFactor,B.SampleOrderSubMaterialId,B.BaseUOMId,B.SampleOrderId,B.MaterialMasterId)
										AS PACK ON PACK.SampleOrderSubMaterialId=SOSM.Id AND PACK.SampleOrderId=SO.Id AND PACK.MaterialMasterId=MM.Id
									LEFT JOIN MST.[MaterialMasterArticle] AS SM ON SOSM.ArticleId=SM.Id
									LEFT JOIN SCS.UnitOfMeasurement AS UoM ON SOSM.UoMId=UoM.Id
									LEFT JOIN SCS.Currency AS CU ON SOSM.CurrencyId=CU.Id

									LEFT JOIN HKP.Characteristics AS C1 ON SOSM.FirstCharacteristicsId = C1.Id
									LEFT JOIN HKP.Characteristics AS C2 ON SOSM.SecondCharacteristicsId = C2.Id
									LEFT JOIN HKP.Characteristics AS C3 ON SOSM.ThirdCharacteristicsId = C3.Id
									LEFT JOIN HKP.CharacteristicsValue AS CV1 ON SOSM.FirstCharacteristicsValueId = CV1.Id
									LEFT JOIN HKP.CharacteristicsValue AS CV2 ON SOSM.SecondCharacteristicsValueId = CV2.Id
									LEFT JOIN HKP.CharacteristicsValue AS CV3 ON SOSM.ThirdCharacteristicsValueId = CV3.Id

									LEFT JOIN (SELECT COALESCE(COUNT(DISTINCT Id),0) AS RC ,MaterialGroupMasterId FROM MST.MaterialGroupPackingForm
										GROUP BY MaterialGroupMasterId) AS SF ON SF.MaterialGroupMasterId=MGM.Id

									LEFT JOIN (SELECT PackingFormId,IsSingleEntry,[Sequence],MaterialGroupMasterId
										FROM MST.MaterialGroupPackingForm WHERE [Sequence]=1) PF on PF.MaterialGroupMasterId=MM.MaterialGroupMasterId
									LEFT OUTER JOIN (SELECT PackingFormId,IsSingleEntry,[Sequence],MaterialGroupMasterId
										FROM MST.MaterialGroupPackingForm WHERE [Sequence]=2) PF2 on PF2.MaterialGroupMasterId=MM.MaterialGroupMasterId
                        WHERE SO.PartyId='" + partyId + "' AND SOSM.MaterialMasterId<>'' AND SOSM.IsConfirmed=1 AND SOSM.DeliveryDate<>''";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void Confirmation(string id, bool flag)
        {
            try
            {
                var entity = Find(id);
                entity.IsConfirmed = flag;
                Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void MaterialAttach(SampleOrderSubMaterial sampleOrderMaterial)
        {
            try
            {
                var entity = Find(sampleOrderMaterial.Id);
                entity.MaterialMasterId = sampleOrderMaterial.MaterialMasterId;
                entity.ArticleId = sampleOrderMaterial.ArticleId;
                entity.FirstCharacteristicsId = sampleOrderMaterial.FirstCharacteristicsId;
                entity.FirstCharacteristicsValueId = sampleOrderMaterial.FirstCharacteristicsValueId;
                entity.SecondCharacteristicsId = sampleOrderMaterial.SecondCharacteristicsId;
                entity.SecondCharacteristicsValueId = sampleOrderMaterial.SecondCharacteristicsValueId;
                entity.ThirdCharacteristicsId = sampleOrderMaterial.ThirdCharacteristicsId;
                entity.ThirdCharacteristicsValueId = sampleOrderMaterial.ThirdCharacteristicsValueId;
                Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void MaterialDetached(string id)
        {
            try
            {
                var entity = Find(id);
                entity.MaterialMasterId = null;
                entity.ArticleId = null;
                entity.FirstCharacteristicsId = null;
                entity.FirstCharacteristicsValueId = null;
                entity.SecondCharacteristicsId = null;
                entity.SecondCharacteristicsValueId = null;
                entity.ThirdCharacteristicsId = null;
                entity.ThirdCharacteristicsValueId = null;
                Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void DispatchDate(string id, DateTime date)
        {
            try
            {
                var entity = Find(id);
                entity.DeliveryDate = date;
                Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void IfUoMExistInMaterialMaster(string materialMasterId, string uomId)
        {
            try
            {
                var sql = @"IF EXISTS(SELECT 1 FROM(
                                SELECT CheckingColumn1 FROM
                                (SELECT Id,BaseUOMId AS CheckingColumn1 FROM MST.MaterialMaster WHERE Id='" + materialMasterId + @"'
                                UNION
                                SELECT MaterialMasterId,AlternativeUOMId AS CheckingColumn1 FROM MST.MaterialMasterAlternativeUOM  WHERE MaterialMasterId='" + materialMasterId + @"') AS A
                                ) AA WHERE CheckingColumn1 ='" + uomId + "') SELECT 1 ELSE SELECT 0 RETURN";

                var data = Convert.ToBoolean(_sampleOrderRepository.SqlQuery<int>(sql).First());
                if (!data)
                    throw new CustomException("Order UoM is not exist in material master!");
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private IEnumerable<SampleOrderViewModel> GetMaterialMasterUom(string[] materialMasterId)
        {
            try
            {
                var sql = @"SELECT Id AS MaterialMasterId,BaseUOMId AS UoMId FROM MST.MaterialMaster WHERE Id IN(" + ReturnStringArray(materialMasterId) + @") UNION
                        SELECT MaterialMasterId AS Id,AlternativeUOMId AS UomId FROM MST.MaterialMasterAlternativeUOM  WHERE MaterialMasterId IN(" + ReturnStringArray(materialMasterId) + ")";
                return _sampleOrderRepository.SqlQuery<SampleOrderViewModel>(sql).ToList();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }
    }
}