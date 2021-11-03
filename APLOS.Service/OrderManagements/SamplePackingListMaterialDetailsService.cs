#region Using

using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.OrderManagements;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class SamplePackingListMaterialDetailsService : Service<SamplePackingListMaterialDetails>, ISamplePackingListMaterialDetailsService
    {
        #region Constructor

        private readonly IRepositoryAsync<SamplePackingListMaterialDetails> _samplePackingListMaterialRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public SamplePackingListMaterialDetailsService(
            IRepositoryAsync<SamplePackingListMaterialDetails> samplePackingListMaterialRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(samplePackingListMaterialRepository, unitOfWork, pkGeneratorService)
        {
            _samplePackingListMaterialRepository = samplePackingListMaterialRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(SamplePackingListMaterialDetails), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        public void InsertPackingMaterial(IEnumerable<SamplePackingListMaterialDetails> entities, string id)
        {
            try
            {
                if (entities != null)
                {
                    var materialMasterIds = entities.Select(t => t.MaterialMasterId).ToArray();
                    var baseUomList = GetBaseUoMListByMaterialMaster(materialMasterIds);
                    var uomIds = entities.Select(t => t.UoMId).ToArray();
                    var uomConvertionList = GetBaseUoMConvertionByMaterialMaster(materialMasterIds, uomIds);

                    var ids = entities.Select(t => t.Id).ToArray();
                    var sampleOrderId = entities.Select(t => t.SampleOrderId).FirstOrDefault();
                    var orderOoMIds = entities.Select(t => t.OrderUoMId).ToArray();
                    var pendingQtyList = GetPendingQtyByMaterialMaster(materialMasterIds, orderOoMIds, ids, sampleOrderId);

                    var pk = GetMaxNumber(nameof(SamplePackingListMaterialDetails), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var entity in entities)
                    {
                        pk.MaxNumber++;
                        entity.Id = pk.MaxNumber.ToString();
                        entity.SamplePackingListMaterialId = id;
                        entity.BaseUOMId = baseUomList.Where(t => t.MaterialMasterId == entity.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault();
                        entity.BaseQty = SumConversion(uomConvertionList, entity);
                        IsAvailableQuantity(pendingQtyList, entity);
                        InsertGraph(entity);
                    }
                }
                else
                    throw new CustomException("Can not save without material.");
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

        private static void IsAvailableQuantity(IEnumerable<MaterialViewModel> pendingQtyList, SamplePackingListMaterialDetails entity)
        {
            var pendigQtyData = pendingQtyList.FirstOrDefault(t => t.MaterialMasterId == entity.MaterialMasterId);
            if (entity.OrderUoMId == entity.BaseUOMId && entity.PendingQty <= entity.BaseQty)
                throw new CustomException("Quantity can not greater than order quantity.");
            else if (pendigQtyData != null && entity.OrderUoMId != entity.BaseUOMId)
            {
                var oderQty = (pendigQtyData.BaseUOMFactor * entity.PendingQty);
                if (oderQty < entity.BaseQty)
                    throw new CustomException("Quantity can not greater than order quantity.");
            }
        }

        private decimal SumConversion(IEnumerable<MaterialViewModel> uomConvertionList, SamplePackingListMaterialDetails entity)
        {
            decimal conversion = 0;
            var dt = uomConvertionList.FirstOrDefault(t => t.MaterialMasterId == entity.MaterialMasterId && t.BaseUOMId == entity.BaseUOMId && t.AlternativeUOMId == entity.UoMId);
            conversion = dt != null && entity.UoMId != entity.BaseUOMId ? Convert.ToDecimal(entity.Qty) * (dt.BaseUOMFactor == null ? 0 : dt.BaseUOMFactor.Value) : Convert.ToDecimal(entity.Qty);
            return conversion;
        }

        public void UpdatePackingMaterial(IEnumerable<SamplePackingListMaterialDetails> entities)
        {
            try
            {
                if (entities != null)
                {
                    var materialMasterIds = entities.Select(t => t.MaterialMasterId).ToArray();
                    var baseUomList = GetBaseUoMListByMaterialMaster(materialMasterIds);
                    var uomIds = entities.Select(t => t.UoMId).ToArray();
                    var uomConvertionList = GetBaseUoMConvertionByMaterialMaster(materialMasterIds, uomIds);

                    var ids = entities.Select(t => t.Id).ToArray();
                    var sampleOrderId = entities.Select(t => t.SampleOrderId).FirstOrDefault();
                    var orderOoMIds = entities.Select(t => t.OrderUoMId).ToArray();
                    var pendingQtyList = GetPendingQtyByMaterialMaster(materialMasterIds, orderOoMIds, ids, sampleOrderId);

                    var pk = GetMaxNumber(nameof(SamplePackingListMaterialDetails), PKGeneratorEnum.Auto, null, DateTime.Now);
                    foreach (var entity in entities)
                    {
                        if (string.IsNullOrEmpty(entity.Id))
                        {
                            pk.MaxNumber++;
                            entity.Id = pk.MaxNumber.ToString();
                            entity.BaseUOMId = baseUomList.Where(t => t.MaterialMasterId == entity.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault();
                            entity.BaseQty = SumConversion(uomConvertionList, entity);
                            IsAvailableQuantity(pendingQtyList, entity);
                            InsertGraph(entity);
                        }
                        else
                        {
                            entity.BaseUOMId = baseUomList.Where(t => t.MaterialMasterId == entity.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault();
                            entity.BaseQty = SumConversion(uomConvertionList, entity);
                            IsAvailableQuantity(pendingQtyList, entity);
                            UpdateGraph(entity);
                        }
                    }
                }
                var id = entities.First().SamplePackingListMaterialId;
                var dbList = Query(t => t.SamplePackingListMaterialId == id).Select().AsEnumerable();
                if (dbList != null)
                {
                    if (entities == null)
                    {
                        foreach (var item in dbList)
                        {
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entities.Any(t => t.Id == item.Id))
                            {
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

        public void UpdatePackLessMaterial(IEnumerable<SamplePackingListMaterialDetails> entities)
        {
            try
            {
                if (entities != null)
                {
                    var materialMasterIds = entities.Select(t => t.MaterialMasterId).ToArray();
                    var baseUomList = GetBaseUoMListByMaterialMaster(materialMasterIds);
                    var uomIds = entities.Select(t => t.UoMId).ToArray();
                    var uomConvertionList = GetBaseUoMConvertionByMaterialMaster(materialMasterIds, uomIds);

                    var ids = entities.Select(t => t.Id).ToArray();
                    var sampleOrderId = entities.Select(t => t.SampleOrderId).FirstOrDefault();
                    var orderOoMIds = entities.Select(t => t.OrderUoMId).ToArray();
                    var pendingQtyList = GetPendingQtyByMaterialMaster(materialMasterIds, orderOoMIds, ids, sampleOrderId);

                    foreach (var entity in entities)
                    {
                        entity.BaseUOMId = baseUomList.Where(t => t.MaterialMasterId == entity.MaterialMasterId).Select(t => t.BaseUOMId).FirstOrDefault();
                        entity.BaseQty = SumConversion(uomConvertionList, entity);
                        IsAvailableQuantity(pendingQtyList, entity);
                        Update(entity);
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

        public void DeletePackingMaterial(string firstPackId)
        {
            try
            {
                var dbList = Query(t => t.SamplePackingListMaterialId == firstPackId).Select().AsEnumerable();
                foreach (var item in dbList)
                {
                    base.DeleteGraph(item);
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

        public void DeleteGraph(string masterId)
        {
            try
            {
                var dbList = Query(t => t.SamplePackingListId == masterId).Select().AsEnumerable();
                if (dbList != null || dbList.Count() > 0)
                {
                    foreach (var second in dbList)
                    {
                        base.DeleteGraph(second);
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

        public IEnumerable<object> GetPackingMaterial(string firstFormId)
        {
            try
            {
                var sql = @"SELECT SPM.Id,SPM.SamplePackingListId
							,SPM.MaterialMasterId,SPM.MaterialGroupMasterId
							,SPM.SamplePackingListMaterialId,MGM.UserName AS MaterialGroupMasterName
							,MM.UserName AS MaterialMasterName
							,SPM.ArticleId,SM.StandardName AS SubMaterialName
							,SO.ReferenceDocNo,RequestReferenceDate=REPLACE(CONVERT(CHAR(11), SO.RequestReferenceDate, 106),' ','-')
							,SPM.SampleOrderId,SPM.SampleOrderSubMaterialId
							,SSM.Name AS SampleSubMaterial
							,DeliveryDate=REPLACE(CONVERT(CHAR(11), SSM.DeliveryDate, 106),' ','-')
							,ISNULL(CV1.Description,'')+' '+ISNULL(CV2.Description,'')+' '+ ISNULL(CV3.Description,'') Detail
							,SPM.FirstCharacteristicsId,SPM.SecondCharacteristicsId,SPM.ThirdCharacteristicsId
							,SPM.FirstCharacteristicsValueId,SPM.SecondCharacteristicsValueId,SPM.ThirdCharacteristicsValueId
							,PF.PackingFormId AS PackingFormId1
							,PF2.PackingFormId AS PackingFormId2
							,SSM.Qty AS OrderQty, SSM.UoMId AS OrderUoMId, UoM.UserName AS OrderUoM, SSM.Rate, CU.Code AS CurrencyName
							,SPM.Qty,SPM.UoMId,PF.PackingFormId, PF.IsSingleEntry , PF2.PackingFormId AS PackingFormId2
							,'' AS materialUoMList
							,PendingQty=CASE WHEN (PACK.BaseUOMId=SSM.UoMId) THEN (SSM.Qty-PACK.BaseQty)
								WHEN (PACK.BaseUOMId<>SSM.UoMId) THEN ((PACK.BaseUOMFactor*SSM.Qty)-PACK.BaseQty)/PACK.BaseUOMFactor
								ELSE SSM.Qty END
							,[Count]=(SELECT COALESCE(COUNT(DISTINCT Id),0) FROM MST.MaterialGroupPackingForm WHERE MaterialGroupMasterId=MGM.Id)
							,IsSingleEntry=CASE WHEN ISNULL(PF.IsSingleEntry,'')='' THEN 0 ELSE PF.IsSingleEntry END
					FROM TRN.SamplePackingListMaterialDetails AS SPM
					JOIN TRN.SampleOrder AS SO ON SPM.SampleOrderId=SO.Id
					JOIN TRN.SampleOrderSubMaterial AS SSM ON SPM.SampleOrderSubMaterialId=SSM.Id
					JOIN [MST].[MaterialMaster] AS MM ON SPM.MaterialMasterId=MM.Id
					LEFT OUTER JOIN (SELECT A.BaseUOMFactor,SUM(B.BaseQty) AS BaseQty,B.SampleOrderSubMaterialId,B.BaseUOMId,B.SampleOrderId,B.SamplePackingListId FROM MST.MaterialMasterAlternativeUOM AS A
							INNER JOIN TRN.SamplePackingListMaterialDetails AS B ON A.MaterialMasterId=B.MaterialMasterId AND A.BaseUOMId=B.BaseUOMId
							WHERE A.MaterialMasterId IN(SELECT DISTINCT MaterialMasterId FROM TRN.SampleOrderSubMaterial WHERE MaterialMasterId<>'') AND SamplePackingListMaterialId<>'2'
							AND A.AlternativeUOMId IN(SELECT UoMId FROM TRN.SampleOrderSubMaterial WHERE MaterialMasterId<>'') AND A.BaseUOMId=B.BaseUOMId
							GROUP BY A.BaseUOMFactor,B.SampleOrderSubMaterialId,B.BaseUOMId,B.SampleOrderId,B.SamplePackingListId)
					AS PACK ON PACK.SampleOrderSubMaterialId=SSM.Id AND PACK.SampleOrderId=SO.Id AND PACK.SamplePackingListId=SPM.SamplePackingListId
					INNER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
					INNER JOIN [MST].[MaterialGroupMaster] AS MGM ON SSM.MaterialGroupMasterId = MGM.Id
					LEFT OUTER JOIN SCS.UnitOfMeasurement AS UoM ON SSM.UoMId = UoM.Id
					LEFT JOIN [MST].[MaterialMasterArticle] AS SM ON SSM.ArticleId=SM.Id
					LEFT OUTER JOIN SCS.Currency AS CU ON SSM.CurrencyId=CU.Id
					LEFT JOIN HKP.Characteristics AS C1 ON SSM.FirstCharacteristicsId = C1.Id
					LEFT JOIN HKP.Characteristics AS C2 ON SSM.SecondCharacteristicsId = C2.Id
					LEFT JOIN HKP.Characteristics AS C3 ON SSM.ThirdCharacteristicsId = C3.Id
					LEFT JOIN HKP.CharacteristicsValue AS CV1 ON SSM.FirstCharacteristicsValueId = CV1.Id
					LEFT JOIN HKP.CharacteristicsValue AS CV2 ON SSM.SecondCharacteristicsValueId = CV2.Id
					LEFT JOIN HKP.CharacteristicsValue AS CV3 ON SSM.ThirdCharacteristicsValueId = CV3.Id
					LEFT OUTER JOIN (SELECT PackingFormId,IsSingleEntry,[Sequence],MaterialGroupMasterId
					FROM MST.MaterialGroupPackingForm WHERE [Sequence]=1) PF on PF.MaterialGroupMasterId=MM.MaterialGroupMasterId
					LEFT OUTER JOIN (SELECT PackingFormId,IsSingleEntry,[Sequence],MaterialGroupMasterId
					FROM MST.MaterialGroupPackingForm WHERE [Sequence]=2) PF2 on PF2.MaterialGroupMasterId=MM.MaterialGroupMasterId
                        WHERE SPM.SamplePackingListMaterialId='" + firstFormId + "' ORDER BY SPM.MaterialMasterId,SPM.ArticleId";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetPackLessMaterialList(string masterId)
         {
            try
            {
                var sql = @"SELECT SPM.Id,SPM.SamplePackingListId
									,SPM.MaterialMasterId,SPM.MaterialGroupMasterId
									,SPM.SamplePackingListMaterialId,MGM.UserName AS MaterialGroupMasterName
									,MM.UserName AS MaterialMasterName
									,SPM.ArticleId,SM.StandardName AS SubMaterialName
									,SO.ReferenceDocNo,RequestReferenceDate=REPLACE(CONVERT(CHAR(11), SO.RequestReferenceDate, 106),' ','-')
									,SPM.SampleOrderId,SPM.SampleOrderSubMaterialId
									,SSM.Name AS SampleSubMaterial
									,DeliveryDate=REPLACE(CONVERT(CHAR(11), SSM.DeliveryDate, 106),' ','-')
									,ISNULL(CV1.Description,'')+' '+ISNULL(CV2.Description,'')+' '+ ISNULL(CV3.Description,'') Detail
									,SPM.FirstCharacteristicsId,SPM.SecondCharacteristicsId,SPM.ThirdCharacteristicsId
									,SPM.FirstCharacteristicsValueId,SPM.SecondCharacteristicsValueId,SPM.ThirdCharacteristicsValueId
									, SSM.Rate,CU.Code AS CurrencyName,SPM.Qty,SPM.UoMId,UoM.UserName AS UoMName
									,'' AS materialUoMList
						FROM TRN.SamplePackingListMaterialDetails AS SPM
						INNER JOIN TRN.SampleOrder AS SO ON SPM.SampleOrderId=SO.Id
						INNER JOIN TRN.SampleOrderSubMaterial AS SSM ON SPM.SampleOrderSubMaterialId=SSM.Id
						INNER JOIN [MST].[MaterialMaster] AS MM ON SPM.MaterialMasterId=MM.Id
						INNER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
						INNER JOIN [MST].[MaterialGroupMaster] AS MGM ON SSM.MaterialGroupMasterId = MGM.Id
						LEFT JOIN SCS.UnitOfMeasurement AS UoM ON SSM.UoMId = UoM.Id
						LEFT JOIN [MST].[MaterialMasterArticle] AS SM ON SSM.ArticleId = SM.Id
						LEFT JOIN SCS.Currency AS CU ON SSM.CurrencyId=CU.Id
						LEFT JOIN HKP.Characteristics AS C1 ON SSM.FirstCharacteristicsId = C1.Id
						LEFT JOIN HKP.Characteristics AS C2 ON SSM.SecondCharacteristicsId = C2.Id
						LEFT JOIN HKP.Characteristics AS C3 ON SSM.ThirdCharacteristicsId = C3.Id
						LEFT JOIN HKP.CharacteristicsValue AS CV1 ON SSM.FirstCharacteristicsValueId = CV1.Id
						LEFT JOIN HKP.CharacteristicsValue AS CV2 ON SSM.SecondCharacteristicsValueId = CV2.Id
						LEFT JOIN HKP.CharacteristicsValue AS CV3 ON SSM.ThirdCharacteristicsValueId = CV3.Id
                        WHERE SPM.SamplePackingListId='" + masterId + "' AND SPM.SamplePackingListMaterialId NOT IN(SELECT DISTINCT SamplePackingListMaterialId FROM TRN.SamplePackingListForm)";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetAllMaterialList(string masterId)
        {
            try
            {
                var sql = @"SELECT SPM.Id,SPM.SamplePackingListId
								,SPM.MaterialMasterId,SPM.MaterialGroupMasterId
								,SPM.SamplePackingListMaterialId,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName AS MaterialMasterName
								,SPM.ArticleId,SM.StandardName AS SubMaterialName
								,SO.ReferenceDocNo,RequestReferenceDate=REPLACE(CONVERT(CHAR(11), SO.RequestReferenceDate, 106),' ','-')
								,SPM.SampleOrderId,SPM.SampleOrderSubMaterialId
								,SSM.Name AS SampleSubMaterial
								,DeliveryDate=REPLACE(CONVERT(CHAR(11), SSM.DeliveryDate, 106),' ','-')
								,ISNULL(CV1.Description,'')+' '+ISNULL(CV2.Description,'')+' '+ ISNULL(CV3.Description,'') Detail
								,SPM.FirstCharacteristicsId,SPM.SecondCharacteristicsId,SPM.ThirdCharacteristicsId
								,SPM.FirstCharacteristicsValueId,SPM.SecondCharacteristicsValueId,SPM.ThirdCharacteristicsValueId
								,SSM.Rate,CU.Code AS CurrencyName,SPM.Qty,SPM.UoMId,UoM.UserName AS UoMName
					FROM TRN.SamplePackingListMaterialDetails AS SPM
					INNER JOIN TRN.SampleOrder AS SO ON SPM.SampleOrderId=SO.Id
					INNER JOIN TRN.SampleOrderSubMaterial AS SSM ON SPM.SampleOrderSubMaterialId=SSM.Id
					INNER JOIN [MST].[MaterialMaster] AS MM ON SPM.MaterialMasterId=MM.Id
					INNER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
					INNER JOIN [MST].[MaterialGroupMaster] AS MGM ON SSM.MaterialGroupMasterId = MGM.Id
					LEFT JOIN SCS.UnitOfMeasurement AS UoM ON SPM.UoMId = UoM.Id
					LEFT JOIN [MST].[MaterialMasterArticle] AS SM ON SSM.ArticleId = SM.Id
					LEFT JOIN SCS.Currency AS CU ON SSM.CurrencyId=CU.Id
					LEFT JOIN HKP.Characteristics AS C1 ON SSM.FirstCharacteristicsId = C1.Id
					LEFT JOIN HKP.Characteristics AS C2 ON SSM.SecondCharacteristicsId = C2.Id
					LEFT JOIN HKP.Characteristics AS C3 ON SSM.ThirdCharacteristicsId = C3.Id
					LEFT JOIN HKP.CharacteristicsValue AS CV1 ON SSM.FirstCharacteristicsValueId = CV1.Id
					LEFT JOIN HKP.CharacteristicsValue AS CV2 ON SSM.SecondCharacteristicsValueId = CV2.Id
					LEFT JOIN HKP.CharacteristicsValue AS CV3 ON SSM.ThirdCharacteristicsValueId = CV3.Id
                        WHERE SPM.SamplePackingListId='" + masterId + "' ORDER BY MGM.UserName";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetViewMaterialList(string firstFormId, string smpMaterialId)
        {
            try
            {
                var sql = @"SELECT SPM.Id,SPM.SamplePackingListId
								,SPM.MaterialMasterId,SPM.MaterialGroupMasterId
								,SPM.SamplePackingListMaterialId,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName AS MaterialMasterName
								,SPM.ArticleId,SM.StandardName AS SubMaterialName
								,SO.ReferenceDocNo,RequestReferenceDate=REPLACE(CONVERT(CHAR(11), SO.RequestReferenceDate, 106),' ','-')
								,SPM.SampleOrderId,SPM.SampleOrderSubMaterialId
								,SSM.Name AS SampleSubMaterial
								,DeliveryDate=REPLACE(CONVERT(CHAR(11), SSM.DeliveryDate, 106),' ','-')
								,ISNULL(CV1.Description,'')+' '+ISNULL(CV2.Description,'')+' '+ ISNULL(CV3.Description,'') Detail
								,SSM.Qty AS OrderQty,UoM1.UserName AS OrderUoM,SSM.Rate,CU.Code AS CurrencyName
								,Qty=(SELECT ContentQty FROM TRN.SamplePackingListForm WHERE Id='1'),SPM.UoMId,UoM.UserName AS UoMName
								--,PendingQty=CASE WHEN (PACK.BaseUOMId=SSM.UoMId) THEN (SSM.Qty-PACK.BaseQty)
								--WHEN (PACK.BaseUOMId<>SSM.UoMId) THEN ((PACK.BaseUOMFactor*SSM.Qty)-PACK.BaseQty)/PACK.BaseUOMFactor ELSE SSM.Qty END
					FROM TRN.SamplePackingListMaterialDetails AS SPM
					INNER JOIN TRN.SampleOrder AS SO ON SPM.SampleOrderId=SO.Id
					INNER JOIN TRN.SampleOrderSubMaterial AS SSM ON SPM.SampleOrderSubMaterialId=SSM.Id
					LEFT OUTER JOIN (SELECT A.BaseUOMFactor,SUM(B.BaseQty) AS BaseQty,B.SampleOrderSubMaterialId,B.BaseUOMId,B.SampleOrderId FROM MST.MaterialMasterAlternativeUOM AS A
							INNER JOIN TRN.SamplePackingListMaterialDetails AS B ON A.MaterialMasterId=B.MaterialMasterId AND A.BaseUOMId=B.BaseUOMId
							WHERE A.MaterialMasterId IN(SELECT DISTINCT MaterialMasterId FROM TRN.SampleOrderSubMaterial WHERE MaterialMasterId<>'') AND SamplePackingListMaterialId<>'1'
							AND A.AlternativeUOMId IN(SELECT UoMId FROM TRN.SampleOrderSubMaterial WHERE MaterialMasterId<>'') AND A.BaseUOMId=B.BaseUOMId
							GROUP BY A.BaseUOMFactor,B.SampleOrderSubMaterialId,B.BaseUOMId,B.SampleOrderId)
					AS PACK ON PACK.SampleOrderSubMaterialId=SSM.Id AND PACK.SampleOrderId=SO.Id
					INNER JOIN [MST].[MaterialMaster] AS MM ON SPM.MaterialMasterId=MM.Id
					INNER JOIN [HKP].[MaterialType] AS MT ON MM.MaterialTypeId = MT.Id
					INNER JOIN [MST].[MaterialGroupMaster] AS MGM ON SSM.MaterialGroupMasterId = MGM.Id
					LEFT JOIN SCS.UnitOfMeasurement AS UoM ON SPM.UoMId = UoM.Id
					LEFT JOIN SCS.UnitOfMeasurement AS UoM1 ON SSM.UoMId = UoM1.Id
					LEFT JOIN [MST].[MaterialMasterArticle] AS SM ON SSM.ArticleId=SM.Id
					LEFT JOIN SCS.Currency AS CU ON SSM.CurrencyId=CU.Id
					LEFT JOIN HKP.Characteristics AS C1 ON SSM.FirstCharacteristicsId = C1.Id
					LEFT JOIN HKP.Characteristics AS C2 ON SSM.SecondCharacteristicsId = C2.Id
					LEFT JOIN HKP.Characteristics AS C3 ON SSM.ThirdCharacteristicsId = C3.Id
					LEFT JOIN HKP.CharacteristicsValue AS CV1 ON SSM.FirstCharacteristicsValueId = CV1.Id
					LEFT JOIN HKP.CharacteristicsValue AS CV2 ON SSM.SecondCharacteristicsValueId = CV2.Id
					LEFT JOIN HKP.CharacteristicsValue AS CV3 ON SSM.ThirdCharacteristicsValueId = CV3.Id
					WHERE SPM.SamplePackingListMaterialId='" + smpMaterialId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #region Material Master UoM And Qty

        private IEnumerable<MaterialViewModel> GetBaseUoMListByMaterialMaster(string[] materialMasterId)
        {
            try
            {
                var _sql = @"SELECT MM.Id AS MaterialMasterId,0.00 AS AlternativeUOMFactor,'' AS AlternativeUOMId,0.00 AS BaseUOMFactor, MM.BaseUOMId FROM MST.MaterialMaster MM WHERE MM.Id IN(" + ReturnStringArray(materialMasterId) + ")";
                return _samplePackingListMaterialRepository.SqlQuery<MaterialViewModel>(_sql).ToList();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private IEnumerable<MaterialViewModel> GetBaseUoMConvertionByMaterialMaster(string[] materialMasterIds, string[] alternativeUOMIds)
        {
            try
            {
                var _sql = @"SELECT MaterialMasterId,AlternativeUOMFactor,AlternativeUOMId,BaseUOMFactor,BaseUOMId FROM MST.MaterialMasterAlternativeUOM MUoM WHERE MUoM.MaterialMasterId IN(" + ReturnStringArray(materialMasterIds) + ") AND AlternativeUOMId IN(" + ReturnStringArray(alternativeUOMIds) + ")";
                return _samplePackingListMaterialRepository.SqlQuery<MaterialViewModel>(_sql).ToList();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        private IEnumerable<MaterialViewModel> GetPendingQtyByMaterialMaster(string[] materialMasterIds, string[] orderUoMIds, string[] ids, string sampleOrderId)
        {
            try
            {
                var _sql = @"SELECT B.MaterialMasterId,A.AlternativeUOMId,A.BaseUOMFactor,B.BaseUOMId, SUM(B.BaseQty) AS BaseQty
                                    FROM MST.MaterialMasterAlternativeUOM AS A
                                    INNER JOIN TRN.SamplePackingListMaterialDetails AS B ON A.MaterialMasterId=B.MaterialMasterId AND A.BaseUOMId=B.BaseUOMId
                                    WHERE A.MaterialMasterId IN(" + ReturnStringArray(materialMasterIds) + @") AND A.AlternativeUOMId IN(" + ReturnStringArray(orderUoMIds) + @")-- AND B.Id NOT IN(" + ReturnStringArray(ids) + @") AND B.SampleOrderId='" + sampleOrderId + @"'
                                    GROUP BY A.BaseUOMFactor,B.SampleOrderSubMaterialId,B.BaseUOMId,B.SampleOrderId,B.MaterialMasterId,A.AlternativeUOMId";
                return _samplePackingListMaterialRepository.SqlQuery<MaterialViewModel>(_sql).ToList();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        #endregion Material Master UoM And Qty
    }
}