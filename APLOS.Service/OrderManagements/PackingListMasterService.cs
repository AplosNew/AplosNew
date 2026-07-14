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
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.OrderManagements
{
    public class PackingListMasterService : Service<PackingListMaster>, IPackingListMasterService
    {
        #region Constructor

        private readonly IRepositoryAsync<DispatchUnitMaster> _unitMasterRepository;
        private readonly IRepositoryAsync<DispatchUnitArticle> _unitArticleRepository;
        private readonly IRepositoryAsync<DispatchUnitSKU> _unitSkuRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public PackingListMasterService(
            IRepositoryAsync<PackingListMaster> packingListMasterRepository
            , IRepositoryAsync<DispatchUnitMaster> unitMasterRepository
            , IRepositoryAsync<DispatchUnitArticle> unitArticleRepository
            , IRepositoryAsync<DispatchUnitSKU> unitDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(packingListMasterRepository, unitOfWork, pkGeneratorService)
        {
            _unitMasterRepository = unitMasterRepository;
            _unitArticleRepository = unitArticleRepository;
            _unitSkuRepository = unitDetailRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        private string GetPK()
        {
            return GetAutoNumber(nameof(PackingListMaster), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        #region List

        public GridModel GetCompanyPartyList(GridParameter parameters, string plantId, string entityId)
        {
            try
            {
                parameters.CmdText = @"SELECT MO.Id AS MasterOrderId, MO.MasterOrderNo, MO.InvoicingPartyPlantId, MO.InvoicingByAddress, MO.DeliveryPartyPlantId, MO.DeliveryByAddress, MO.TotalQty, MO.TotalQtyUOMId
                                        , P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType
                                        , CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                        , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                        , CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                        , CP.TaxApplicable, CP.IsTaxApplicableChangeable, CP.PlantId
									    , (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                            FROM [TRN].[MasterOrder] AS MO
							LEFT JOIN [HKP].[OrderStatus] AS OS ON MO.OrderStatusId = OS.Id
							LEFT JOIN [HKP].[OrderCategory] AS OC ON MO.OrderCategoryId = OC.Id
                            LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
                            LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id AND CP.PlantId = MO.PlantId
                            LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                            LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
							LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
							LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
							WHERE P.Archive=0 AND P.Active=1 AND MO.PlantId = '" + plantId + "' AND MO.EntityId = '" + entityId + @"'
							AND OS.Id= '" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' AND OC.UserName= 'Confirmed' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string entityId)
        {
            try
            {
                parameters.CmdText = @"SELECT PK.Id, PK.PlantId, PK.EntityId
	                                , PK.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
	                                , PK.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, PK.InvoicingByAddress
	                                , PK.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, PK.DeliveryByAddress
	                                , PK.TotalQty, PK.TotalQtyUOMId, TUoM.UserName AS TotalQtyUOM, PK.TotalQtyBaseUoMId
	                                , PK.Remarks 
                                FROM [TRN].[PackingListMaster] AS PK
                                LEFT JOIN [HKP].[Party] AS P ON PK.PartyId = P.Id
                                LEFT JOIN [HKP].[PartyPlant] AS InvPP ON PK.InvoicingPartyPlantId=InvPP.Id
                                LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON PK.DeliveryPartyPlantId=DeliPP.Id
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON PK.TotalQtyUOMId=TUoM.Id
                                WHERE PK.EntityId = '" + entityId + "'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel GetSalesOrderList(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT  SO.Id, SO.Id SalesOrderNo, SO.MasterOrderItemId, SO.ShipmentModeId, SO.DestinationId
		                                , DeliveryDate = REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-')
		                                , CommitmentDate = REPLACE(CONVERT(CHAR(11), SO.CommitmentDate, 106),' ','-')
		                                , SO.CustomerPOId, po.PONumber, SO.OrderStatusId, SO.OrderCategoryId, SO.SOType, SO.ResponsiblePersonId
		                                , SO.UpCharge, SO.Qty, SO.Rate, SO.IsFirstEntry
		                                , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName, MOI.ArticleId, ARt.StandardName AS ArticleName
                                        , ISNULL(ATTR.HasAttribute,CAST(0 AS BIT)) AS HasAttribute
                                FROM [TRN].[SalesOrder] AS SO
                                JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                                LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                                JOIN [TRN].[ProductionOrderDetail] AS POD ON POD.SalesOrderId = SO.Id
                                JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id
                                JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = Art.Id
                                LEFT JOIN (SELECT AttributeSetLength=CASE WHEN COUNT(MaterialMasterId)>0THEN COUNT(MaterialMasterId) ELSE 0 END
                                                , HasAttribute=CASE WHEN COUNT(MaterialMasterId)>0 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END, MaterialMasterId
                                            FROM MST.MaterialMasterAttribute GROUP BY MaterialMasterId) AS ATTR ON MOI.MaterialMasterId=ATTR.MaterialMasterId
                                LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                                LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                                WHERE So.Id NOT IN (SELECT SalesOrderId FROM [TRN].[DispatchUnitArticle])
                                AND OS.Id= '" + Library.Model.Enums.OrderStatusEnum.Active.ToString() + @"' AND OC.UserName= 'Confirmed'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public IEnumerable<object> GetDispatchMasterArticleList(string packingId)
        {
            try
            {
                var sql = @"SELECT DART.Id, DART.PackingListMasterId, DART.DispatchUnitMasterId, DART.FGInventoryReceiveId, DART.SalesOrderId
		                         , DUM.DispatchUnitCode, SO.Id SalesOrderNo, MO.MasterOrderNo
		                         , DART.MaterialMasterId, MM.UserName AS MaterialMasterName
		                         , DART.ArticleId, ARt.StandardName AS ArticleName
		                         , DART.Qty, DART.QtyUOMId, DART.QtyBaseUoMId
                        FROM [TRN].[DispatchUnitArticle] AS DART 
                        JOIN [TRN].[DispatchUnitMaster] AS DUM ON DART.DispatchUnitMasterId = DUM.Id
                        JOIN [TRN].[SalesOrder] AS SO ON DART.SalesOrderId = SO.Id
                        JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                        JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id
                        LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = Art.Id
                        JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId= MO.Id
                        WHERE DART.PackingListMasterId = '" + packingId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetDispatchAllSKUList(string packingId)
        {
            try
            {
                var sql = @"SELECT DSKU.Id, DART.DispatchUnitMasterId, DUM.DispatchUnitCode, SO.Id SalesOrderNo, MO.MasterOrderNo
		                        , MM.UserName AS MaterialMasterName, ARt.StandardName AS ArticleName
		                        , DSKU.SalesOrderFirstCharacteristicsId, DSKU.FirstCharacteristicsId, CH1.UserName AS CH1Name
		                        , FST.CharacteristicsValueId AS CHVId1, CH1Value=CASE WHEN FST.CharacteristicsValueId IS NOT NULL THEN CHV1.UserName ELSE FST.ValueFreeText END
		                        , DSKU.SalesOrderSecondCharacteristicsId, DSKU.SecondCharacteristicsId, CH2.UserName AS CH2Name
		                        , SCN.CharacteristicsValueId AS CHVId2, CH2Value=CASE WHEN SCN.CharacteristicsValueId IS NOT NULL THEN CHV2.UserName ELSE SCN.ValueFreeText END
		                        , DSKU.SalesOrderThirdCharacteristicsId, DSKU.ThirdCharacteristicsId, CH3.UserName AS CH3Name
		                        , TRD.CharacteristicsValueId AS CHVId3, CH3Value=CASE WHEN TRD.CharacteristicsValueId IS NOT NULL THEN CHV3.UserName ELSE TRD.ValueFreeText END
		                        , DSKU.NoOfPackingUnit, DSKU.QtyPerPackingUnit, DSKU.Qty
                        FROM [TRN].[DispatchUnitSKU] AS DSKU
                        JOIN [TRN].[DispatchUnitArticle] AS DART ON DSKU.DispatchUnitArticleId = DART.Id
                        JOIN [TRN].[DispatchUnitMaster] AS DUM ON DART.DispatchUnitMasterId = DUM.Id
                        JOIN [TRN].[SalesOrder] AS SO ON DART.SalesOrderId = SO.Id
                        JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                        JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id
                        LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = Art.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH1 ON DSKU.FirstCharacteristicsId = CH1.Id
                        LEFT JOIN [TRN].[FirstCharacteristics] AS FST ON DSKU.SalesOrderFirstCharacteristicsId = FST.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON  FST.CharacteristicsValueId = CHV1.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH2 ON DSKU.SecondCharacteristicsId = CH2.Id
                        LEFT JOIN [TRN].[SecondCharacteristics] AS SCN ON DSKU.SalesOrderSecondCharacteristicsId = SCN.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON  SCN.CharacteristicsValueId = CHV2.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH3 ON DSKU.ThirdCharacteristicsId = CH3.Id
                        LEFT JOIN [TRN].[ThirdCharacteristics] AS TRD ON DSKU.SalesOrderThirdCharacteristicsId = TRD.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON  TRD.CharacteristicsValueId = CHV3.Id
                        JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId= MO.Id
                        WHERE DART.PackingListMasterId = '" + packingId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetDispatchData(string dispatchUnitMasterId)
        {
            try
            {
                var sql = @"SELECT DUM.Id, DUM.PackingListMasterId,DUM.DispatchUnitCode, DUM.Qty, DUM.QtyUOMId, DUM.QtyBaseUoMId
                                , DUM.NetWeight, DUM.GrossWeight, DUM.Remarks
                        FROM [TRN].[DispatchUnitMaster] AS DUM
                        WHERE DUM.Id = '" + dispatchUnitMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetDispatchArticleList(string dispatchUnitMasterId)
        {
            try
            {
                var sql = @"SELECT DART.Id, DART.PackingListMasterId, DART.DispatchUnitMasterId, DART.FGInventoryReceiveId, DART.SalesOrderId
                            , DART.MaterialMasterId, MM.UserName AS MaterialMasterName
                            , DART.ArticleId, ART.StandardName AS ArticleName
                            , DART.Qty, DART.QtyUOMId, DART.QtyBaseUoMId
                    FROM [TRN].[DispatchUnitArticle] AS DART
                    JOIN [MST].[MaterialMaster] AS MM ON DART.MaterialMasterId = MM.Id
                    LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON DART.ArticleId = Art.Id
                    WHERE DART.DispatchUnitMasterId = '" + dispatchUnitMasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetDispatchSKUListByArticle(string dispatchArticleId)
        {
            try
            {
                var sql = @"SELECT DSKU.Id, DSKU.DispatchUnitArticleId
		                        , DSKU.SalesOrderFirstCharacteristicsId, DSKU.FirstCharacteristicsId, CH1.UserName AS CH1Name
		                        , FST.CharacteristicsValueId AS CHVId1, CH1Value=CASE WHEN FST.CharacteristicsValueId IS NOT NULL THEN CHV1.UserName ELSE FST.ValueFreeText END
		                        , DSKU.SalesOrderSecondCharacteristicsId, DSKU.SecondCharacteristicsId, CH2.UserName AS CH2Name
		                        , SCN.CharacteristicsValueId AS CHVId2, CH2Value=CASE WHEN SCN.CharacteristicsValueId IS NOT NULL THEN CHV2.UserName ELSE SCN.ValueFreeText END
		                        , DSKU.SalesOrderThirdCharacteristicsId, DSKU.ThirdCharacteristicsId, CH3.UserName AS CH3Name
		                        , TRD.CharacteristicsValueId AS CHVId3, CH3Value=CASE WHEN TRD.CharacteristicsValueId IS NOT NULL THEN CHV3.UserName ELSE TRD.ValueFreeText END
		                        , DSKU.NoOfPackingUnit, DSKU.QtyPerPackingUnit, DSKU.Qty, DSKU.QtyUOMId, DSKU.QtyBaseUoMId
                        FROM [TRN].[DispatchUnitSKU] AS DSKU
                        LEFT JOIN [HKP].[Characteristics] AS CH1 ON DSKU.FirstCharacteristicsId = CH1.Id
                        LEFT JOIN [TRN].[FirstCharacteristics] AS FST ON DSKU.SalesOrderFirstCharacteristicsId = FST.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON  FST.CharacteristicsValueId = CHV1.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH2 ON DSKU.SecondCharacteristicsId = CH2.Id
                        LEFT JOIN [TRN].[SecondCharacteristics] AS SCN ON DSKU.SalesOrderSecondCharacteristicsId = SCN.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON  SCN.CharacteristicsValueId = CHV2.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH3 ON DSKU.ThirdCharacteristicsId = CH3.Id
                        LEFT JOIN [TRN].[ThirdCharacteristics] AS TRD ON DSKU.SalesOrderThirdCharacteristicsId = TRD.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON  TRD.CharacteristicsValueId = CHV3.Id
                        WHERE DSKU.DispatchUnitArticleId  = '" + dispatchArticleId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetSalesOrderSKUList(string salesOrderId)
        {
            try
            {
                var sql = @"SELECT FST.SalesOrderId, FST.Id AS FirstCharacteristicsId, FST.[Sequence], FST.CharacteristicsId AS CHId1, CH1.UserName AS CH1Name
                            , FST.CharacteristicsValueId AS CHVId1, CH1Value=CASE WHEN FST.CharacteristicsValueId IS NOT NULL THEN CHV1.UserName ELSE FST.ValueFreeText END, FST.Qty AS FQty
                            , SCN.Id AS SecondCharacteristicsId, SCN.[Sequence], SCN.CharacteristicsId AS CHId2, CH2.UserName AS CH2Name
                            , SCN.CharacteristicsValueId AS CHVId2, CH2Value=CASE WHEN SCN.CharacteristicsValueId IS NOT NULL THEN CHV2.UserName ELSE SCN.ValueFreeText END, SCN.Qty AS SQty
                            , TRD.Id AS ThirdCharacteristicsId, TRD.[Sequence], TRD.CharacteristicsId AS CHId3, CH3.UserName AS CH3Name
                            , TRD.CharacteristicsValueId AS CHVId3, CH3Value=CASE WHEN TRD.CharacteristicsValueId IS NOT NULL THEN CHV3.UserName ELSE TRD.ValueFreeText END, TRD.Qty AS THQty
                            , Qty=CASE WHEN TRD.Id IS NOT NULL THEN TRD.Qty
                                        WHEN SCN.Id IS NOT NULL THEN SCN.Qty
                                        WHEN FST.Id IS NOT NULL THEN FST.Qty END
                        FROM [TRN].[FirstCharacteristics] AS FST
                        LEFT JOIN [HKP].[Characteristics] AS CH1 ON FST.CharacteristicsId = CH1.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV1 ON  FST.CharacteristicsValueId = CHV1.Id
                        LEFT JOIN [TRN].[SecondCharacteristics] AS SCN ON SCN.FirstCharacteristicsId = FST.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH2 ON SCN.CharacteristicsId = CH2.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV2 ON  SCN.CharacteristicsValueId = CHV2.Id
                        LEFT JOIN [TRN].[ThirdCharacteristics] AS TRD ON TRD.SecondCharacteristicsId = SCN.Id
                        LEFT JOIN [HKP].[Characteristics] AS CH3 ON TRD.CharacteristicsId = CH3.Id
                        LEFT JOIN [HKP].[CharacteristicsValue] AS CHV3 ON  TRD.CharacteristicsValueId = CHV3.Id
                        WHERE FST.SalesOrderId = '" + salesOrderId + "' ORDER BY 2, 9, 16";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        #endregion

        public IEnumerable<object> GetEntityCboByPlant(string plantId)
        {
            try
            {
                var sql = @"SELECT ENTD.Id, ENTP.Id, EN.Id AS [Value], EN.UserName AS [Text]
                                , ENTD.IsDispatchGrpApplicable, ENTD.ProcessNature AS DispatchProcessNature, ENTD.DispatchUoM
                                , ENTP.ProcessNature AS PackingProcessNature, ENTP.PackingUoM
                        FROM [ORG].[Entity] AS EN
                        LEFT JOIN [HKP].[EntityProcessTag] AS ENTD ON ENTD.EntityId = EN.Id AND ENTD.ProcessNature='Dispatch'
                        LEFT JOIN [HKP].[EntityProcessTag] AS ENTP ON ENTP.EntityId = EN.Id AND ENTP.ProcessNature='Packing'
                        WHERE EN.PlantId = '" + plantId + "' ORDER BY 4";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Process.ToString()));
            }
        }

        public override void Insert(PackingListMaster entity)
        {
            try
            {
                entity.Id = GetPK();
                base.Insert(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public override void Update(PackingListMaster entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "PackingListMaster Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = Find(id);
                base.DeleteGraph(entity);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertOrUpdateDispatch(DispatchUnitMaster dispatch, IEnumerable<DispatchUnitArticle> articleList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                dispatch.Qty = articleList.Sum(t => t.Qty);

                if (string.IsNullOrEmpty(dispatch.Id))
                {
                    var count = _unitMasterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[DispatchUnitMaster] WHERE PackingListMasterId='{dispatch.PackingListMasterId}'").First();
                    count++;
                    dispatch.Id = MakePK(dispatch.PackingListMasterId, count, 2);
                    dispatch.QtyBaseUoMId = dispatch.QtyUOMId;
                    AuditService.AddedLog(dispatch);
                    _unitMasterRepository.Insert(dispatch);

                }
                else
                {
                    AuditService.UpdatedLog(dispatch);
                    _unitMasterRepository.Update(dispatch);
                }

                var count2 = _unitMasterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[DispatchUnitArticle] WHERE DispatchUnitMasterId='{dispatch.Id}'").First();

                foreach (var item in articleList)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        count2++;
                        item.Id = MakePK(dispatch.Id, count2, 2);
                        item.DispatchUnitMasterId = dispatch.Id;
                        item.QtyBaseUoMId = item.QtyUOMId;
                        AuditService.AddedLog(item);
                        _unitArticleRepository.Insert(item);
                    }
                    else
                    {
                        AuditService.UpdatedLog(item);
                        _unitArticleRepository.Update(item);
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void InsertOrUpdateDispatchSku(IEnumerable<DispatchUnitSKU> skuList)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var dispatchArticleId = skuList.FirstOrDefault().DispatchUnitArticleId;

                if (_unitArticleRepository.Query(t => t.Id == dispatchArticleId).Select(t => t.Qty).Sum() != skuList.Sum(t => t.Qty))
                    throw new CustomException("Packet qty doesn't match box qty");

                var count = _unitMasterRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[DispatchUnitSKU] WHERE DispatchUnitArticleId='{dispatchArticleId}'").First();

                foreach (var item in skuList)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        count++;
                        item.Id = MakePK(dispatchArticleId, count, 2);
                        item.DispatchUnitArticleId = dispatchArticleId;
                        AuditService.AddedLog(item);
                        _unitSkuRepository.Insert(item);
                    }
                    else
                    {
                        AuditService.UpdatedLog(item);
                        _unitSkuRepository.Update(item);
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
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteDispatchArticleGraph(string id)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;

                var entity = _unitArticleRepository.Find(id);
                var dispatchUnitMasterId = entity.DispatchUnitMasterId;

                var dbArticleList = _unitArticleRepository.Query(t => t.DispatchUnitMasterId == dispatchUnitMasterId).Select().ToList();
                var dis = _unitMasterRepository.Query(t => t.Id == dispatchUnitMasterId).Select().FirstOrDefault();

                dis.Qty -= entity.Qty;

                var dbList = _unitSkuRepository.Query(t => t.DispatchUnitArticleId == id).Select().ToList();

                foreach (var item in dbList)
                {
                    _unitSkuRepository.Delete(item);
                }
                _unitArticleRepository.Delete(entity);

                if (dbArticleList.Count() == 1)
                    _unitMasterRepository.Delete(dis);
                else
                {
                    AuditService.UpdatedLog(dis);
                    _unitMasterRepository.Update(dis);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.OrderManagement.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteDispatchSkuGraph(string id)
        {
            try
            {
                var entity = _unitSkuRepository.Query(t => t.Id == id).Select().FirstOrDefault();
                _unitSkuRepository.Delete(entity);
                _unitOfWork.SaveChanges();
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