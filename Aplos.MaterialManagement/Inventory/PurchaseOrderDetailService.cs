using Aplos.MaterialManagement.MaterialQuery;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Materials;
using Library.Service.Systems;
using Library.ViewModel.Inventory;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Library.MaterialManagement.Inventory
{
    public class PurchaseOrderDetailService : Service<PurchaseOrderDetail>, IPurchaseOrderDetailService
    {


        #region Constructor
        private readonly IPKGeneratorService _pkGeneratorService;

        private readonly IRepositoryAsync<PurchaseOrderDetail> _receiveDetailRepository;
        private readonly IRepositoryAsync<PoRequisitionDetail> _poRequisitionDetailRepository;

        private readonly IRepositoryAsync<POBOQMap> _POBOQMapRepository;

        private readonly IRepositoryAsync<ServicePODetail> _ServicePODetail;
        private readonly IRepositoryAsync<InventoryReceive> _inventoryReceiveRepository;
        private readonly IRepositoryAsync<PurchaseOrderTax> _receiveTaxRepository;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IPOMaterialService _inventoryMaterialMasterService;
        private readonly IPurchaseOrderService _inventoryReceiveService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPurchaseOrderService _inventoryReveiveService;
        private readonly IMaterialRequsitionDetailsServiceService _materialRequsitionDetailsServiceService;
        private readonly IRepositoryAsync<MaterialRequsitionDetails> _reqDetailRepository;
        private readonly IRepositoryAsync<ServicePOTax> _ServicePOTax;
        private readonly IRepositoryAsync<TermsAndConditionsPOChild> _termsAndConditionsPOChildRepository;
        private readonly IRepositoryAsync<TermsAndConditionsPODetails> _termsAndConditionsPODetailRepository;
        public PurchaseOrderDetailService(
             IRepositoryAsync<PurchaseOrderDetail> receiveDetailRepository
            , IRepositoryAsync<PoRequisitionDetail> poRequisitionDetailRepository
              , IRepositoryAsync<ServicePODetail> ServicePODetail
            , IRepositoryAsync<InventoryReceive> inventoryReceiveRepository
            , IRepositoryAsync<PurchaseOrderTax> receiveTaxRepository
            , IMaterialMasterService materialMasterService
            , IPOMaterialService inventoryMaterialMasterService
            , IPurchaseOrderService inventoryReceiveService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IPurchaseOrderService inventoryReveiveService
            , IMaterialRequsitionDetailsServiceService materialRequsitionDetailsServiceService
            , IRepositoryAsync<MaterialRequsitionDetails> reqDetailRepository
            , IRepositoryAsync<ServicePOTax> ServicePOTax
            , IRepositoryAsync<POBOQMap> POBOQMapRepository
            , IRepositoryAsync<TermsAndConditionsPOChild> termsAndConditionsPOChildRepository
            , IRepositoryAsync<TermsAndConditionsPODetails> termsAndConditionsPODetailRepository
            ) : base(receiveDetailRepository, unitOfWork, pkGeneratorService)
        {
            _receiveDetailRepository = receiveDetailRepository;
            _receiveTaxRepository = receiveTaxRepository;
            _materialMasterService = materialMasterService;
            _inventoryMaterialMasterService = inventoryMaterialMasterService;
            _inventoryReceiveService = inventoryReceiveService;
            _inventoryReceiveRepository = inventoryReceiveRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _pkGeneratorService = pkGeneratorService;
            _inventoryReveiveService = inventoryReveiveService;
            _materialRequsitionDetailsServiceService = materialRequsitionDetailsServiceService;
            _reqDetailRepository = reqDetailRepository;
            _poRequisitionDetailRepository = poRequisitionDetailRepository;
            _ServicePODetail = ServicePODetail;
            _ServicePOTax = ServicePOTax;
            _POBOQMapRepository = POBOQMapRepository;
            _termsAndConditionsPOChildRepository = termsAndConditionsPOChildRepository;
            _termsAndConditionsPODetailRepository = termsAndConditionsPODetailRepository;
        }

        #endregion Constructor

       
        public void InsertOrUpdateGraph(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {


            var flag = false;
            try
            {
                //if (CheckItemExist(entity))
                //    throw new CustomException(entity.MaterialMasterName + " already received");
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                ResetCurrencyRate(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entity.IsNotNull())
                {
                    var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(entity);
                    //var materialData = materialCommonService.GetInventoryMaterialByUpToSku(entity).FirstOrDefault();
                    if (materialData.IsNotNull()) entity.InventoryMaterialId = materialData.Id;
                    ///TODO : Get total qyt and amount by country and issue qty
                    entity.TotalQty = Query(t => t.InventoryMaterialId == entity.InventoryMaterialId && t.Id != entity.Id).Select(t => t.BaseQty).Sum();
                    var totalAmount = Query(t => t.InventoryMaterialId == entity.InventoryMaterialId && t.Id != entity.Id).Select(t => t.BaseAmount).Sum();

                    var materialMasterIds = new string[] { entity.MaterialMasterId };
                    var altUomIds = new string[] { entity.TransactionUoMId };
                    var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                    if (entity.BaseUOMId != entity.TransactionUoMId && entity.CurrencyId != entity.BaseCurrencyId
                         && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                    {
                        entity.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == entity.BaseUOMId && t.AlternativeUOMId == entity.TransactionUoMId).BaseUOMFactor);
                        entity.BaseQty = Convert.ToDecimal(entity.TransactionQty * entity.BaseUoMFactor);
                        entity.BaseAmount = entity.TransactionAmount * entity.ToCurrencyRate;

                        //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                        //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                    }
                    else if (entity.BaseUOMId == entity.TransactionUoMId && entity.CurrencyId != entity.BaseCurrencyId)
                    {
                        entity.BaseQty = entity.TransactionQty;
                        entity.BaseUoMFactor = entity.TransactionQty;
                        //entity.BaseAmount = entity.TransactionAmount * entity.ToCurrencyRate;
                        entity.BaseAmount = entity.TransactionAmount;

                        //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                        //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                    }
                    else if (entity.BaseUOMId != entity.TransactionUoMId && entity.CurrencyId == entity.BaseCurrencyId
                        && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                    {
                        entity.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == entity.BaseUOMId && t.AlternativeUOMId == entity.TransactionUoMId).BaseUOMFactor);
                        entity.BaseQty = Convert.ToDecimal(entity.TransactionQty * entity.BaseUoMFactor);
                        entity.BaseAmount = entity.TransactionAmount;

                        //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                        //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                    }
                    else
                    {
                        entity.BaseUoMFactor = entity.TransactionQty;
                        entity.BaseQty = entity.TransactionQty;
                        entity.BaseAmount = entity.TransactionAmount;
                    }
                    // Insert in receive detail
                    if (string.IsNullOrEmpty(entity.Id))
                    {
                        var grndId = "";
                        var NewId = entity.InventoryReceiveId + "-";
                        var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId='{entity.InventoryReceiveId}'").First();
                        currentId++;
                        var receiveDetail = new PurchaseOrderDetail
                        {
                            //Id = MakePK(entity.InventoryReceiveId + 1, currentId, 2),
                            Id = NewId + currentId,
                            MaterialStorageId = entity.MaterialStorageId,
                            InventoryReceiveId = entity.InventoryReceiveId,
                            InventoryMaterialId = entity.MaterialMasterId,//InventoryMaterial is MaterialMasterId
                            TransactionQty = entity.TransactionQty,
                            TransactionUoMId = entity.TransactionUoMId,
                            BaseQty = Convert.ToDecimal(entity.BaseQty),
                            BaseUOMId = entity.BaseUOMId,
                            BaseUoMFactor = Convert.ToDecimal(entity.BaseUoMFactor),
                            TransactionRate = Math.Round(Convert.ToDecimal(entity.TransactionRate), 4),
                            TransactionAmount = Math.Round(Convert.ToDecimal(entity.TransactionAmount), 2),
                            BaseAmount = Math.Round(Convert.ToDecimal(entity.BaseAmount), 2),
                            TotalTaxAmount = Math.Round(Convert.ToDecimal(entity.TotalTaxAmount), 2),
                            IssueQty = null,
                            GRNRcvQty = 0,
                            QtyStatus = false,
                            CountryId = entity.CountryId,
                            MasterOrderId = null,
                            MasterOrderDetailId = null,
                            Description = entity.Description,
                            DeliveryDate = entity.DeliveryDate,
                            ArticleId = entity.ArticleId,
                            FirstCharacteristicsId = entity.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = entity.FirstCharacteristicsValueId,
                            SecondCharacteristicsId = entity.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = entity.SecondCharacteristicsValueId,
                            ThirdCharacteristicsId = entity.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = entity.ThirdCharacteristicsValueId,
                            AcceptanceRcvQty = 0,
                            AcceptanceRcvStatusQty = false,
                            RefferenceNo = entity.RefferenceNo,
                            Tolerance = entity.Tolerance



                        };
                        entity.InventoryReceiveDetailId = receiveDetail.Id;
                        AuditService.AddedLog(receiveDetail);
                        var ratio = _inventoryReceiveService.GetChargesRatio(receiveDetail.InventoryReceiveId, receiveDetail.Id, receiveDetail.TransactionAmount, null, 0, entity.IsNonCreditable);

                        receiveDetail.ChargesAmount = receiveDetail.TransactionAmount * ratio;
                        receiveDetail.WithInvoiceRate = entity.IsNonCreditable ? (receiveDetail.TransactionAmount + receiveDetail.TotalTaxAmount + receiveDetail.ChargesAmount) / receiveDetail.TransactionQty
                                                 : (receiveDetail.TransactionAmount + receiveDetail.ChargesAmount) / receiveDetail.TransactionQty;
                        receiveDetail.AfterInvoiceRate = receiveDetail.WithInvoiceRate;

                        //receiveDetail.BaseAmount += entity.IsNonCreditable ? Convert.ToDecimal(entity.TotalTaxAmount + receiveDetail.ChargesAmount) * Convert.ToDecimal(entity.ToCurrencyRate) :
                        //     Convert.ToDecimal(receiveDetail.ChargesAmount) * Convert.ToDecimal(entity.ToCurrencyRate);
                        receiveDetail.BaseAmount = entity.IsNonCreditable ? Convert.ToDecimal(entity.TotalTaxAmount + receiveDetail.BaseAmount) * Convert.ToDecimal(entity.ToCurrencyRate) :
                             Convert.ToDecimal(receiveDetail.BaseAmount);//* Convert.ToDecimal(entity.ToCurrencyRate);

                        entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                        entity.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.BaseAmount) / entity.TotalQty);

                        _inventoryMaterialMasterService.InsertOrUpdateFromReceive(entity);
                        receiveDetail.InventoryMaterialId = entity.MaterialMasterId;
                        InsertGraph(receiveDetail);
                        UpdateInventoryDetail(receiveDetail, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                    }
                    // insert in receive tax
                    if (taxCategoryList.IsNotNull())
                    {
                        var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            currentId++;
                            item.Id = MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                            item.InventoryReceiveId = entity.InventoryReceiveId;
                            item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                            item.InventoryServiceId = null;
                            AuditService.AddedLog(item);
                            _receiveTaxRepository.Insert(item);
                        }
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
                {
                    _unitOfWork.Rollback();
                }
            }
        }

        public void InsertOrUpdateGraphFGForMasterOrder(PurchaseOrder entity, IEnumerable<InventoryMaterialViewModel> Materialentity, IEnumerable<PurchaseOrderTax> taxCategoryList, IEnumerable<InventoryMaterialViewModel> ServiceEntity, IEnumerable<PurchaseOrderTax> ServicetaxCategoryList)
        {


            var flag = false;
            try
            {
                //if (CheckItemExist(entity))
                //    throw new CustomException(entity.MaterialMasterName + " already received");


                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                entity.POType = "FGPO";
                entity.MasterOrderId = Materialentity.Select(r => r.MasterOrderId).FirstOrDefault();

                _inventoryReveiveService.Insert(entity);
                foreach (var itemDetail in Materialentity)
                {
                    ResetCurrencyRate(itemDetail);
                    if (entity.IsNotNull())
                    {
                        itemDetail.CompanyGroupId = identity.CompanyGroupId;
                        itemDetail.CompanyId = identity.CompanyId;
                        itemDetail.PlantId = identity.PlantId;
                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != entity.Id).Select(t => t.BaseQty).Sum();
                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != entity.Id).Select(t => t.BaseAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && entity.CurrencyId != entity.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;
                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && entity.CurrencyId != entity.BaseCurrencyId)
                        {
                            itemDetail.BaseQty = itemDetail.TransactionQty;
                            itemDetail.BaseUoMFactor = itemDetail.TransactionQty;
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == entity.BaseCurrencyId
                            && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }
                        else
                        {
                            itemDetail.BaseUoMFactor = itemDetail.TransactionQty;
                            itemDetail.BaseQty = itemDetail.TransactionQty;
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }
                        // Insert in receive detail
                        if (string.IsNullOrEmpty(entity.Id))
                        {
                            var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderDetail] WHERE InventoryReceiveId='{itemDetail.InventoryReceiveId}'").First();
                            currentId++;
                            var receiveDetail = new PurchaseOrderDetail
                            {
                                Id = MakePK(itemDetail.InventoryReceiveId + 1, currentId, 2),
                                MaterialStorageId = itemDetail.MaterialStorageId,
                                InventoryReceiveId = itemDetail.InventoryReceiveId,
                                //InventoryMaterialId = entity.InventoryMaterialId,
                                TransactionQty = itemDetail.TransactionQty,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                BaseUOMId = itemDetail.BaseUOMId,
                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                TransactionRate = Convert.ToDecimal(itemDetail.TransactionRate),
                                TransactionAmount = Convert.ToDecimal(itemDetail.TransactionAmount),
                                BaseAmount = Convert.ToDecimal(itemDetail.BaseAmount),
                                TotalTaxAmount = Convert.ToDecimal(itemDetail.TotalTaxAmount),
                                IssueQty = null,
                                GRNRcvQty = 0,
                                QtyStatus = false,
                                CountryId = itemDetail.CountryId,
                                MasterOrderId = itemDetail.MasterOrderId,
                                MasterOrderDetailId = itemDetail.MasterOrderDetailId,
                                AcceptanceRcvQty = 0,
                                AcceptanceRcvStatusQty = false,


                            };
                            itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                            AuditService.AddedLog(receiveDetail);
                            var ratio = _inventoryReceiveService.GetChargesRatio(receiveDetail.InventoryReceiveId, receiveDetail.Id, receiveDetail.TransactionAmount, null, 0, entity.IsNonCreditable);

                            receiveDetail.ChargesAmount = receiveDetail.TransactionAmount * ratio;
                            receiveDetail.WithInvoiceRate = entity.IsNonCreditable ? (receiveDetail.TransactionAmount + receiveDetail.TotalTaxAmount + receiveDetail.ChargesAmount) / receiveDetail.TransactionQty
                                                     : (receiveDetail.TransactionAmount + receiveDetail.ChargesAmount) / receiveDetail.TransactionQty;
                            receiveDetail.AfterInvoiceRate = receiveDetail.WithInvoiceRate;

                            //receiveDetail.BaseAmount += entity.IsNonCreditable ? Convert.ToDecimal(entity.TotalTaxAmount + receiveDetail.ChargesAmount) * Convert.ToDecimal(entity.ToCurrencyRate) :
                            //     Convert.ToDecimal(receiveDetail.ChargesAmount) * Convert.ToDecimal(entity.ToCurrencyRate);
                            receiveDetail.BaseAmount = entity.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + receiveDetail.BaseAmount) * Convert.ToDecimal(entity.ToCurrencyRate) :
                                 Convert.ToDecimal(receiveDetail.BaseAmount);//* Convert.ToDecimal(entity.ToCurrencyRate);

                            itemDetail.TotalQty = Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty);
                            itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.BaseAmount) / itemDetail.TotalQty);

                            _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                            receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                            InsertGraph(receiveDetail);
                            UpdateInventoryDetail(receiveDetail, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                        }
                        // insert in receive tax
                        if (taxCategoryList.IsNotNull())
                        {
                            var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                            foreach (var item in taxCategoryList)
                            {
                                currentId++;
                                item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                                item.InventoryReceiveId = itemDetail.InventoryReceiveId;
                                item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                                item.InventoryServiceId = null;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Insert(item);
                            }
                        }
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
                {
                    _unitOfWork.Rollback();
                }
            }

        }



        private string GetPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(PoRequisitionDetail), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        private string GetPOBOQPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(POBOQMap), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public void InsertExtraTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList)
        {
            var flag = false;
            try
            {
                if (CheckItemExist(entity))
                    throw new CustomException(entity.MaterialMasterName + " already received");

                ResetCurrencyRate(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                var dbList = _receiveTaxRepository.Query(t => t.InventoryReceiveDetailId == entity.InventoryReceiveDetailId).Select().AsEnumerable();
                if (entity.IsNotNull())
                {
                    // insert in PO Item tax
                    if (taxCategoryList.IsNotNull())
                    {
                        var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            if (item.Id == null)
                            {
                                currentId++;
                                item.Id = MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                                item.InventoryReceiveId = entity.InventoryReceiveId;
                                item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                                item.InventoryServiceId = null;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Insert(item);
                                //_receiveTaxRepository.InsertOrUpdateGraph(item);
                            }
                            else
                            {
                                item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                                item.InventoryServiceId = null;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Update(item);
                            }
                        }
                        if (dbList != null)
                        {
                            var deleteList = dbList.Where(t => t.InventoryReceiveDetailId == entity.InventoryReceiveDetailId).ToList();
                            foreach (var item in deleteList)
                            {
                                if (!taxCategoryList.Any(t => t.Id == item.Id))
                                    _receiveTaxRepository.Delete(item);
                            }
                        }
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
                {
                    _unitOfWork.Rollback();
                }
            }

        }
        public void InsertserviceTax(InventoryMaterialViewModel entity, IEnumerable<PurchaseOrderTax> taxCategoryList, string ServiceId)
        {
            var flag = false;
            try
            {
                if (CheckItemExist(entity))
                    throw new CustomException(entity.MaterialMasterName + " already received");
                var dbList = _receiveTaxRepository.Query(t => t.InventoryServiceId == ServiceId && t.InventoryReceiveId == entity.InventoryReceiveId).Select().AsEnumerable();
                ResetCurrencyRate(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entity.IsNotNull())
                {

                    // insert in receive tax
                    if (taxCategoryList.IsNotNull())
                    {
                        var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            if (item.Id == null)
                            {
                                currentId++;
                                item.Id = GetPK();//MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                                item.InventoryReceiveId = entity.InventoryReceiveId;
                                item.InventoryReceiveDetailId = null; //entity.InventoryReceiveDetailId;
                                item.InventoryServiceId = ServiceId;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Insert(item);
                                //_receiveTaxRepository.InsertOrUpdateGraph(item);
                            }
                            else
                            {
                                item.InventoryReceiveId = entity.InventoryReceiveId;
                                item.InventoryServiceId = ServiceId;
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Update(item);
                            }
                        }
                        if (dbList != null)
                        {
                            var deleteList = dbList.Where(t => t.InventoryServiceId == ServiceId && t.InventoryReceiveId == entity.InventoryReceiveId).ToList();
                            foreach (var item in deleteList)
                            {
                                if (!taxCategoryList.Any(t => t.Id == item.Id))
                                    _receiveTaxRepository.Delete(item);

                            }
                        }
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
                {
                    _unitOfWork.Rollback();
                }
            }
        }


        private static void ResetCurrencyRate(InventoryMaterialViewModel entity)
        {
            if (string.IsNullOrEmpty(entity.ToCurrencyRate.ToString()))
            {
                if (entity.BaseCurrencyId != entity.CurrencyId)
                    throw new CustomException("Please input currency rate.");
                else
                    entity.ToCurrencyRate = 1;
            }
            else if (entity.ToCurrencyRate == 0)
            {
                if (entity.BaseCurrencyId != entity.CurrencyId)
                    throw new CustomException("Please input currency rate.");
                else
                    entity.ToCurrencyRate = 1;
            }
            else
            {
                if (entity.BaseCurrencyId == entity.CurrencyId)
                    entity.ToCurrencyRate = 1;
            }
        }

        private void UpdateInventoryDetail(PurchaseOrderDetail detail, decimal ratio, decimal currencyRate, bool isNonCreditable)
        {
            var detailList = base.Query(t => t.InventoryReceiveId == detail.InventoryReceiveId && t.Id != detail.Id).Select().ToList();
            if (detailList.IsNotNull())
            {
                foreach (var item in detailList)
                {
                    //var chamnt = item.ChargesAmount;
                    var chamnt = item.BaseAmount;
                    item.ChargesAmount = item.TransactionAmount * ratio;
                    //item.WithInvoiceRate = isNonCreditable ? (item.TransactionAmount + item.TotalTaxAmount + item.ChargesAmount) / item.TransactionQty
                    //                             : (item.TransactionAmount + item.ChargesAmount) / item.TransactionQty;
                    item.WithInvoiceRate = isNonCreditable ? (item.TransactionAmount + item.TotalTaxAmount + item.BaseAmount) / item.TransactionQty
                                                 : (item.TransactionAmount + item.BaseAmount) / item.TransactionQty;
                    item.AfterInvoiceRate = item.WithInvoiceRate;
                    item.BaseAmount = (item.BaseAmount - (chamnt * currencyRate)) + item.BaseAmount * currencyRate;

                    item.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(item);
                    UpdateGraph(item);
                }
            }
        }

        public void Delete(string receiveDetailId, string OrderSpecific)
        {
            var flag = false;
            if (OrderSpecific == "No")
            {
                try
                {
                    var isNonCreditable = _receiveDetailRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[PurchaseOrder] AS A JOIN [TRN].[PurchaseOrderDetail] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + receiveDetailId + "'").First();
                    var data = Find(receiveDetailId);
                    if (data.IsNotNull())
                    {
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        _inventoryMaterialMasterService.UpdateFromReceive(data.InventoryMaterialId, receiveDetailId);
                        var taxCategoryList = _receiveTaxRepository.Query(t => t.InventoryReceiveDetailId == receiveDetailId).Select().ToList();
                        if (taxCategoryList.Count > 0)
                        {
                            foreach (var item in taxCategoryList)
                            {
                                item.ModelState = ModelState.Deleted;
                                _receiveTaxRepository.Delete(item);
                                _unitOfWork.SaveChanges();
                            }
                        }
                        var ratio = _inventoryReceiveService.GetChargesRatio(data.InventoryReceiveId, data.Id, 0, null, 0, isNonCreditable);
                        UpdateInventoryDetail(data, ratio, 1, isNonCreditable);
                        var res = _inventoryReceiveRepository.SqlQuery<int>(@"Select POId=Case when IR.POId IS NULL then 0 else 1 end from [TRN].PurchaseOrder PO Left JOIN [TRN].[InventoryReceive]  IR On IR.POId=PO.Id where PO.Id= '" + data.InventoryReceiveId + "'").FirstOrDefault();
                        if (res == 1)
                        {
                            throw new CustomException("Already Received In PO");

                        }
                        base.DeleteGraph(data);
                        _unitOfWork.SaveChanges();
                        flag = false;
                        _unitOfWork.Commit();
                    }
                    else
                        throw new CustomException("Data not found");
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
                    {
                        _unitOfWork.Rollback();
                    }
                }
            }
            else
            {
                try
                {
                    var isNonCreditable = _receiveDetailRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[PurchaseOrder] AS A JOIN [TRN].[PurchaseOrderDetail] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + receiveDetailId + "'").First();
                    var data = Find(receiveDetailId);
                    if (data.IsNotNull())
                    {
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        _inventoryMaterialMasterService.UpdateFromReceive(data.InventoryMaterialId, receiveDetailId);
                        var taxCategoryList = _receiveTaxRepository.Query(t => t.InventoryReceiveDetailId == receiveDetailId).Select().ToList();
                        if (taxCategoryList.Count > 0)
                        {
                            foreach (var item in taxCategoryList)
                            {
                                item.ModelState = ModelState.Deleted;
                                _receiveTaxRepository.Delete(item);
                                _unitOfWork.SaveChanges();
                            }
                        }
                        var ratio = _inventoryReceiveService.GetChargesRatio(data.InventoryReceiveId, data.Id, 0, null, 0, isNonCreditable);
                        UpdateInventoryDetail(data, ratio, 1, isNonCreditable);
                        var POBOQMAPList = _POBOQMapRepository.Query(t => t.PODetailId == receiveDetailId).Select().ToList();
                        if (POBOQMAPList.Count > 0)
                        {
                            foreach (var itemPOBOQMap in POBOQMAPList)
                            {
                                itemPOBOQMap.ModelState = ModelState.Deleted;
                                _POBOQMapRepository.Delete(itemPOBOQMap);
                                _unitOfWork.SaveChanges();
                            }
                        }
                        var res = _inventoryReceiveRepository.SqlQuery<int>(@"Select POId=Case when IR.POId IS NULL then 0 else 1 end from [TRN].PurchaseOrder PO Left JOIN [TRN].[InventoryReceive]  IR On IR.POId=PO.Id where PO.Id= '" + data.InventoryReceiveId + "'").FirstOrDefault();
                        if (res == 1)
                        {
                            throw new CustomException("Already Received In PO");

                        }
                        base.DeleteGraph(data);
                        _unitOfWork.SaveChanges();
                        flag = false;
                        _unitOfWork.Commit();
                    }
                    else
                        throw new CustomException("Data not found");
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
                    {
                        _unitOfWork.Rollback();
                    }
                }
            }

        }

        private bool CheckItemExist(InventoryMaterialViewModel entity)
        {
            try
            {
                var sql = @"IF EXISTS(SELECT 1 FROM(
                                SELECT IRD.InventoryMaterialId FROM [TRN].[InventoryReceiveDetail] AS IRD
                                JOIN [TRN].[InventoryMaterial] AS IM ON IRD.InventoryMaterialId=IM.Id
                                WHERE IRD.InventoryReceiveId='" + entity.InventoryReceiveId + @"'
                                AND IM.MaterialMasterId='" + entity.MaterialMasterId + @"'
                                AND ISNULL(IM.ArticleId,'')='" + entity.ArticleId + @"'
                                AND ISNULL(IM.FirstCharacteristicsValueId,'')='" + entity.FirstCharacteristicsValueId + @"'
                                AND ISNULL(IM.SecondCharacteristicsValueId,'')='" + entity.SecondCharacteristicsValueId + @"'
                                AND ISNULL(IM.ThirdCharacteristicsValueId,'')='" + entity.ThirdCharacteristicsValueId + @"'
                            ) AS TBL ) SELECT 1 ELSE SELECT 0 RETURN";
                var d = Convert.ToBoolean(_receiveDetailRepository.SqlQuery<int>(sql).First());
                return d;
            }
            catch
            {
                throw;
            }
        }
        public void UpdateMaterial(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            try
            {
                
                if (entity.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item1 in entity)
                    {
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        var ip = identity.IPAddress;
                        var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                        var UpdatedBy = identity.Name;
                        var TaxAutoId = item1.InventoryReceiveDetailId;
                        if (item1.TransactionRate == null)
                        {
                            item1.TransactionRate = 0;

                        }
                        if (item1.Tolerance == null)
                        {
                            item1.Tolerance = 0;//Convert.ToInt32(0).ToString();

                        }

                        if (item1.BaseTaxAmount == null)
                        {
                            item1.BaseTaxAmount = "0.00";
                        }
                        //if(string.IsNullOrEmpty(item1.Tolerance))
                        //{
                        //	item1.Tolerance = 0;

                        //}
                        string _sql = "Update TRN.purchaseOrderDetail set TransactionQty='" + item1.TransactionQty + "', TransactionRate='" + item1.TransactionRate + "', TransactionAmount='" + item1.TrnAmount + "',TotalTaxAmount='" + item1.BaseTaxAmount + "' ,BaseAmount='" + item1.TrnAmount + "',Description ='" + item1.Description + "',RefferenceNo ='" + item1.RefferenceNo + "',DeliveryDate='" + item1.DeliveryDate + "',UpdatedBy='" + UpdatedBy + "',UpdatedDate='" + updatedDate + "',UpdatedFromIP='" + ip + "',Tolerance='" + item1.Tolerance + "'  where id='" + TaxAutoId + "'";
                        _sqlRepository.ExecuteSqlCommand(_sql);
                    }
                }
                if (receiveTaxList.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item in receiveTaxList)
                    {
                        
                        var TaxAutoId = item.Id;
                        string _sql1 = "Update TRN.PurchaseOrderTax set TaxAmount='" + item.TaxAmount + "' where id='" + TaxAutoId + "' ";
                        _sqlRepository.ExecuteSqlCommand(_sql1);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public void UpdateServiceAndTax(IEnumerable<POMaterialViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
        {
            try
            {
                if (entity.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item1 in entity)
                    {
                        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        var ip = identity.IPAddress;
                        var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
                        var UpdatedBy = identity.Name;
                        var TaxAutoId = item1.Id;
                        if (item1.TotalTaxAmount == null)
                        {
                            item1.TotalTaxAmount = 0;
                        }
                        // string _sql = "Update TRN.purchaseOrderDetail set TransactionQty='" + item1.TransactionQty + "', BaseQty='" + item1.TransactionQty + "',BaseUOMFactor='" + item1.TransactionQty + "', TransactionRate='" + item1.TransactionRate + "', TransactionAmount='" + item1.TrnAmount + "',TotalTaxAmount='" + item1.BaseTaxAmount + "' ,BaseAmount='" + item1.BaseAmount + "',UpdatedBy='" + UpdatedBy + "',UpdatedDate='" + updatedDate + "',UpdatedFromIP='" + ip + "'  where id='" + TaxAutoId + "'";
                        string _sql = "Update TRN.POService set Amount='" + item1.Amount + "', TotalTaxAmount='" + item1.TotalTaxAmount + "', Description='" + item1.Description + "',UpdatedBy='" + UpdatedBy + "',UpdatedDate='" + updatedDate + "',UpdatedFromIP='" + ip + "'  where id='" + TaxAutoId + "'";
                        _sqlRepository.ExecuteSqlCommand(_sql);
                    }
                }

                //AuditService.AddedLog(voucher);
                // 
                // _sqlRepository.ExecuteSqlCommand(_sql);
                if (receiveTaxList.IsNotNull())
                {
                    // var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                    foreach (var item in receiveTaxList)
                    {
                        var TaxAutoId = item.Id;
                        string _sql1 = "Update TRN.PurchaseOrderTax set TaxAmount='" + item.TaxAmount + "' where id='" + TaxAutoId + "' ";
                        _sqlRepository.ExecuteSqlCommand(_sql1);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }



        #region     POByRequisition
        public void InsertOrUpdateGraphPoByReq(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {


            var flag = false;

            try
            {
                //if (CheckItemExist(entity))
                //    throw new CustomException(entity.MaterialMasterName + " already received");

                //ResetCurrencyRate(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                var NewId = "";
                var NewId1 = "";
                //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[PurchaseOrderDetail] WHERE  InventoryReceiveId = '{PoId}'").First();
                var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(Id, CHARINDEX('-',id)+1,len(Id))    AS INT)), 0) Id FROM[TRN].[PurchaseOrderDetail] WHERE  InventoryReceiveId = '{PoId}'").First();

                decimal TransactionQtyGroupSum = 0;

                var groupListentity = groupList;
                foreach (var itemDetail in groupListentity)
                {
                    if (entity.IsNotNull())
                    {
                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);
                        if (!string.IsNullOrEmpty(itemDetail.MaterialMasterId))
                        {
                            TransactionQtyGroupSum = entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId).Sum(r => r.TransactionQty);

                        }
                        else
                        {
                            TransactionQtyGroupSum = itemDetail.TransactionQty;

                        }

                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);

                            itemDetail.BaseQty = Convert.ToDecimal(TransactionQtyGroupSum * itemDetail.BaseUoMFactor);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;

                            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {
                            //var TransactionQty = entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId).Sum(r => r.TransactionQty);
                            itemDetail.BaseQty = TransactionQtyGroupSum;
                            //itemDetail.BaseQty = itemDetail.TransactionQty;

                            itemDetail.BaseUoMFactor = TransactionQtyGroupSum;
                            //entity.BaseAmount = entity.TransactionAmount * entity.ToCurrencyRate;
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;

                            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            //var TransactionQty = entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId).Sum(r => r.TransactionQty);
                            //Command Date 26/12/2019
                            //itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            //itemDetail.BaseQty = Convert.ToDecimal(TransactionQtyGroupSum * itemDetail.BaseUoMFactor);
                            //End Command Date 26/12/2019
                            itemDetail.BaseQty = Convert.ToDecimal(TransactionQtyGroupSum);

                            //itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;

                            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                        }
                        else
                        {
                            //var TransactionQty = entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId).Sum(r => r.TransactionQty);
                            //itemDetail.BaseUoMFactor = itemDetail.TransactionQty;
                            //itemDetail.BaseQty = itemDetail.TransactionQty;
                            itemDetail.BaseUoMFactor = TransactionQtyGroupSum;
                            itemDetail.BaseQty = TransactionQtyGroupSum;
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }

                        //End Update Req Table

                        itemDetail.Id = "";
                        if (string.IsNullOrEmpty(itemDetail.MaterialStorageId))
                        {
                            //TransactionQty = TransactionQty;
                        }
                        // Insert in receive detail
                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {

                            
                            NewId = PoId + "-";
                            currentId++;
                            var receiveDetail = new PurchaseOrderDetail
                            {
                                //Id = MakePK(itemDetail.InventoryReceiveId + 1, currentId, 2),
                                Id = NewId + currentId, //MakePK(NewId + currentId, 0,0),
                                MaterialStorageId = itemDetail.MaterialStorageId,
                                InventoryReceiveId = PoId, //itemDetail.InventoryReceiveId,
                                InventoryMaterialId = itemDetail.MaterialMasterId,
                                TransactionQty = TransactionQtyGroupSum,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                //BaseUOMId = itemDetail.BaseUOMId,
                                BaseUOMId = itemDetail.TransactionUoMId,

                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                TransactionRate = Convert.ToDecimal(itemDetail.TransactionRate),
                                TransactionAmount = Convert.ToDecimal(itemDetail.TransactionAmount),
                                BaseAmount = Convert.ToDecimal(itemDetail.BaseAmount),
                                TotalTaxAmount = Convert.ToDecimal(itemDetail.TotalTaxAmount),
                                IssueQty = null,
                                GRNRcvQty = 0,
                                QtyStatus = false,
                                CountryId = itemDetail.CountryId,
                                MasterOrderId = null,
                                MasterOrderDetailId = null,
                                Description = itemDetail.Description,
                                DeliveryDate = itemDetail.DeliveryDate,
                                RequisitionId = itemDetail.RequisitionId,
                                RequisitionDetailId = itemDetail.RequisitionDetailId,
                                ArticleId = itemDetail.ArticleId,
                                FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                AcceptanceRcvQty = 0,
                                AcceptanceRcvStatusQty = false,
                                RefferenceNo = itemDetail.RefferenceNo


                            };
                            NewId1 = receiveDetail.Id;

                            itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                            AuditService.AddedLog(receiveDetail);
                            var ratio = _inventoryReceiveService.GetChargesRatio(receiveDetail.InventoryReceiveId, receiveDetail.Id, receiveDetail.TransactionAmount, null, 0, itemDetail.IsNonCreditable);

                            receiveDetail.ChargesAmount = receiveDetail.TransactionAmount * ratio;
                            
                            receiveDetail.BaseAmount = itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + receiveDetail.BaseAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                 Convert.ToDecimal(receiveDetail.BaseAmount);//* Convert.ToDecimal(entity.ToCurrencyRate);

                            itemDetail.TotalQty = Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty);
                            itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.BaseAmount) / itemDetail.TotalQty);

                            _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                            receiveDetail.InventoryMaterialId = itemDetail.MaterialMasterId;
                            InsertGraph(receiveDetail);
                            //UpdateInventoryDetail(receiveDetail, ratio, Convert.ToDecimal(itemDetail.ToCurrencyRate), itemDetail.IsNonCreditable);
                            // insert in receive tax
                            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                            //var list =

                            var list = _inventoryReceiveService.GetTaxCategoryList1(identity.CompanyGroupId, PoId, identity.PlantId, itemDetail.HSNCodeId);


                            if (list.IsNotNull())
                            {
                                var currentIdTax = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                                foreach (var item in list)
                                {

                                    //new Di
                                    //var v = new System.Collections.Generic.Dictionary<string, object>(item).Items[""].
                                    var potax = new PurchaseOrderTax();
                                    currentIdTax++;
                                    potax.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentIdTax, 2);
                                    potax.InventoryReceiveId = PoId;//itemDetail.InventoryReceiveId;
                                    potax.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                                    potax.InventoryServiceId = null;
                                    potax.HSNCodeId = itemDetail.HSNCodeId;
                                    potax.TaxCategoryId = item.TaxCategoryId;
                                    potax.Percentage = item.Percentage;
                                    potax.TaxAmount = item.TaxAmount;
                                    potax.ModelState = ModelState.Added;
                                    AuditService.AddedLog(potax);
                                    _receiveTaxRepository.Insert(potax);
                                    //InsertGraph(item);
                                }
                            }

                        }

                    }

                    foreach (var POReqDetail in entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId))
                    {
                        try
                        {


                            if (entity.IsNotNull() && !string.IsNullOrEmpty(POReqDetail.MaterialMasterId))
                            {
                                var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(POReqDetail);
                                if (materialData.IsNotNull()) POReqDetail.InventoryMaterialId = materialData.Id;
                                ///TODO : Get total qyt and amount by country and issue qty
                                POReqDetail.TotalQty = Query(t => t.InventoryMaterialId == POReqDetail.InventoryMaterialId && t.Id != POReqDetail.Id).Select(t => t.BaseQty).Sum();
                                //var totalAmount = Query(t => t.InventoryMaterialId == POReqDetail.InventoryMaterialId && t.Id != POReqDetail.Id).Select(t => t.BaseAmount).Sum();

                                var materialMasterIds = new string[] { POReqDetail.MaterialMasterId };
                                var altUomIds = new string[] { POReqDetail.TransactionUoMId };
                                var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                                if (POReqDetail.BaseUOMId != POReqDetail.TransactionUoMId && POReqDetail.CurrencyId != POReqDetail.BaseCurrencyId
                                     && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                {
                                    POReqDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == POReqDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                    POReqDetail.BaseQty = Convert.ToDecimal(POReqDetail.TransactionQty * POReqDetail.BaseUoMFactor);
                                    POReqDetail.BaseAmount = POReqDetail.TransactionAmount * POReqDetail.ToCurrencyRate;

                                    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                                    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                                }
                                else if (POReqDetail.BaseUOMId == POReqDetail.TransactionUoMId && POReqDetail.CurrencyId != POReqDetail.BaseCurrencyId)
                                {
                                    POReqDetail.BaseQty = POReqDetail.TransactionQty;
                                    POReqDetail.BaseUoMFactor = POReqDetail.TransactionQty;
                                    //entity.BaseAmount = entity.TransactionAmount * entity.ToCurrencyRate;
                                    POReqDetail.BaseAmount = POReqDetail.TransactionAmount;

                                    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                                    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                                }
                                else if (POReqDetail.BaseUOMId != POReqDetail.TransactionUoMId && POReqDetail.CurrencyId == POReqDetail.BaseCurrencyId
                                    && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                {
                                    POReqDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == POReqDetail.BaseUOMId && t.AlternativeUOMId == POReqDetail.TransactionUoMId).BaseUOMFactor);
                                    POReqDetail.BaseQty = Convert.ToDecimal(POReqDetail.TransactionQty * POReqDetail.BaseUoMFactor);
                                    POReqDetail.BaseAmount = POReqDetail.TransactionAmount;

                                    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                                    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                                }
                                else
                                {
                                    POReqDetail.BaseUoMFactor = POReqDetail.TransactionQty;
                                    POReqDetail.BaseQty = POReqDetail.TransactionQty;
                                    POReqDetail.BaseAmount = POReqDetail.TransactionAmount;
                                }

                                var reqDetail = _materialRequsitionDetailsServiceService.Query(r => r.Id == POReqDetail.RequisitionDetailId).Select().FirstOrDefault();

                                if (reqDetail == null)
                                    throw new CustomException("Requisition Details Or PO Details not found!");

                                reqDetail.PORcvQty += POReqDetail.TransactionQty;
                                //if (reqDetail.TransactionQty < reqDetail.PORcvQty)
                                //	throw new CustomException("Received Qty can not cross balance Qty.");
                                if (reqDetail.TransactionQty <= reqDetail.PORcvQty)
                                {
                                    reqDetail.POQtyStatus = true;
                                }
                                else if (itemDetail.WantToClose == true)
                                {
                                    reqDetail.POQtyStatus = true;
                                }
                                else
                                {
                                    reqDetail.POQtyStatus = false;
                                }
                                reqDetail.AccessQtyReason = itemDetail.AccessQtyReason;
                                //reqDetail.POQtyStatus = reqDetail.TransactionQty == reqDetail.PORcvQty;
                                AuditService.UpdatedLog(reqDetail);
                                _reqDetailRepository.Update(reqDetail);
                                //End Update Req Table

                                
                                var PoReqDetail = new PoRequisitionDetail
                                {
                                    Id = GetPK(),
                                    PoDetailId = NewId1,
                                    RequisitionDetailId = POReqDetail.RequisitionDetailId,
                                    TransactionQty = POReqDetail.TransactionQty,
                                    BaseQty = Convert.ToDecimal(POReqDetail.BaseQty),

                                };
                                AuditService.AddedLog(PoReqDetail);
                                _poRequisitionDetailRepository.Insert(PoReqDetail);

                            }


                        }
                        catch (Exception e)
                        {

                        }
                    }
                    //where there is no materialMasterId
                    foreach (var POReqDetail in entity.Where(r => r.RequisitionDetailId == itemDetail.RequisitionDetailId))
                    {
                        try
                        {


                            if (entity.IsNotNull() && string.IsNullOrEmpty(POReqDetail.MaterialMasterId))
                            {
                                var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(POReqDetail);
                                if (materialData.IsNotNull()) POReqDetail.InventoryMaterialId = materialData.Id;
                                ///TODO : Get total qyt and amount by country and issue qty
                                POReqDetail.TotalQty = Query(t => t.InventoryMaterialId == POReqDetail.InventoryMaterialId && t.Id != POReqDetail.Id).Select(t => t.BaseQty).Sum();
                                //var totalAmount = Query(t => t.InventoryMaterialId == POReqDetail.InventoryMaterialId && t.Id != POReqDetail.Id).Select(t => t.BaseAmount).Sum();

                                var materialMasterIds = new string[] { POReqDetail.MaterialMasterId };
                                var altUomIds = new string[] { POReqDetail.TransactionUoMId };
                                var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                                if (POReqDetail.BaseUOMId != POReqDetail.TransactionUoMId && POReqDetail.CurrencyId != POReqDetail.BaseCurrencyId
                                     && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                {
                                    POReqDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == POReqDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                    POReqDetail.BaseQty = Convert.ToDecimal(POReqDetail.TransactionQty * POReqDetail.BaseUoMFactor);
                                    POReqDetail.BaseAmount = POReqDetail.TransactionAmount * POReqDetail.ToCurrencyRate;

                                    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                                    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                                }
                                else if (POReqDetail.BaseUOMId == POReqDetail.TransactionUoMId && POReqDetail.CurrencyId != POReqDetail.BaseCurrencyId)
                                {
                                    POReqDetail.BaseQty = POReqDetail.TransactionQty;
                                    POReqDetail.BaseUoMFactor = POReqDetail.TransactionQty;
                                    //entity.BaseAmount = entity.TransactionAmount * entity.ToCurrencyRate;
                                    POReqDetail.BaseAmount = POReqDetail.TransactionAmount;

                                    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                                    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                                }
                                else if (POReqDetail.BaseUOMId != POReqDetail.TransactionUoMId && POReqDetail.CurrencyId == POReqDetail.BaseCurrencyId
                                    && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                {
                                    POReqDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == POReqDetail.BaseUOMId && t.AlternativeUOMId == POReqDetail.TransactionUoMId).BaseUOMFactor);
                                    POReqDetail.BaseQty = Convert.ToDecimal(POReqDetail.TransactionQty * POReqDetail.BaseUoMFactor);
                                    POReqDetail.BaseAmount = POReqDetail.TransactionAmount;

                                    //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                                    //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                                }
                                else
                                {
                                    POReqDetail.BaseUoMFactor = POReqDetail.TransactionQty;
                                    POReqDetail.BaseQty = POReqDetail.TransactionQty;
                                    POReqDetail.BaseAmount = POReqDetail.TransactionAmount;
                                }

                                var reqDetail = _materialRequsitionDetailsServiceService.Query(r => r.Id == POReqDetail.RequisitionDetailId).Select().FirstOrDefault();
                                if (reqDetail == null)
                                    throw new CustomException("Requisition Details Or PO Details not found!");

                                reqDetail.PORcvQty += POReqDetail.TransactionQty;
                                //if (reqDetail.TransactionQty < reqDetail.PORcvQty)
                                //	throw new CustomException("Received Qty can not cross balance Qty.");
                                if (reqDetail.TransactionQty >= reqDetail.PORcvQty)
                                {
                                    reqDetail.POQtyStatus = true;
                                }
                                else if (itemDetail.WantToClose == true)
                                {
                                    reqDetail.POQtyStatus = true;
                                }
                                else
                                {
                                    reqDetail.POQtyStatus = false;
                                }
                                //reqDetail.POQtyStatus = reqDetail.TransactionQty == reqDetail.PORcvQty;
                                reqDetail.AccessQtyReason = itemDetail.AccessQtyReason;
                                AuditService.UpdatedLog(reqDetail);
                                _reqDetailRepository.Update(reqDetail);
                                //End Update Req Table

                                //POReqDetail.Id = "";
                                // Insert in receive detail
                                //if (string.IsNullOrEmpty(POReqDetail.Id))
                                //{


                                var PoReqDetail = new PoRequisitionDetail
                                {
                                    Id = GetPK(),
                                    PoDetailId = NewId1,
                                    RequisitionDetailId = POReqDetail.RequisitionDetailId,
                                    TransactionQty = POReqDetail.TransactionQty,
                                    BaseQty = Convert.ToDecimal(itemDetail.BaseQty),

                                };
                                AuditService.AddedLog(PoReqDetail);
                                _poRequisitionDetailRepository.Insert(PoReqDetail);

                            }


                        }
                        catch (Exception e)
                        {

                        }
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
                {
                    _unitOfWork.Rollback();
                }
            }

        }

        public void InsertOrUpdateGraphPoUpdateByReq(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {

            var flag = false;

            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var NewId = "";
                var NewId1 = "";
                //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[PurchaseOrderDetail] WHERE  InventoryReceiveId = '{PoId}'").First();
                var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(Id, CHARINDEX('-',id)+1,len(Id))    AS INT)), 0) Id FROM[TRN].[PurchaseOrderDetail] WHERE  InventoryReceiveId = '{PoId}'").First();

                var groupListentity = groupList;
                foreach (var itemDetail in groupListentity)
                {
                    if (entity.IsNotNull())
                    {
                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);
                        var TransactionQty = entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId).Sum(r => r.TransactionQty);
                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);

                            itemDetail.BaseQty = Convert.ToDecimal(TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;
                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {
                            itemDetail.BaseQty = TransactionQty;
                            itemDetail.BaseUoMFactor = TransactionQty;
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                            && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseQty = Convert.ToDecimal(TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }
                        else
                        {
                            itemDetail.BaseUoMFactor = 1;
                            itemDetail.BaseQty = TransactionQty;
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }

                        itemDetail.Id = "";
                        // Insert in receive detail
                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {

                            NewId = PoId + "-";
                            currentId++;
                            var receiveDetail = new PurchaseOrderDetail
                            {
                                //Id = MakePK(itemDetail.InventoryReceiveId + 1, currentId, 2),
                                //Id = NewId + currentId, //MakePK(NewId + currentId, 0,0),
                                Id = itemDetail.InventoryReceiveDetailId,
                                MaterialStorageId = itemDetail.MaterialStorageId,
                                InventoryReceiveId = PoId, //itemDetail.InventoryReceiveId,
                                InventoryMaterialId = itemDetail.MaterialMasterId,
                                TransactionQty = TransactionQty,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                //BaseUOMId = itemDetail.BaseUOMId,
                                BaseUOMId = itemDetail.TransactionUoMId,

                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                TransactionRate = Convert.ToDecimal(itemDetail.TransactionRate),
                                TransactionAmount = Convert.ToDecimal(itemDetail.TransactionAmount),
                                BaseAmount = Convert.ToDecimal(itemDetail.BaseAmount),
                                TotalTaxAmount = Convert.ToDecimal(itemDetail.TotalTaxAmount),
                                IssueQty = null,
                                GRNRcvQty = 0,
                                QtyStatus = false,
                                CountryId = itemDetail.CountryId,
                                MasterOrderId = null,
                                MasterOrderDetailId = null,
                                Description = itemDetail.Description,
                                DeliveryDate = itemDetail.DeliveryDate,
                                RequisitionId = itemDetail.RequisitionId,
                                RequisitionDetailId = itemDetail.RequisitionDetailId,
                                ArticleId = itemDetail.ArticleId,
                                FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                AcceptanceRcvQty = 0,
                                AcceptanceRcvStatusQty = false

                            };
                            NewId1 = receiveDetail.Id;

                            itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                            AuditService.AddedLog(receiveDetail);
                            var ratio = _inventoryReceiveService.GetChargesRatio(receiveDetail.InventoryReceiveId, receiveDetail.Id, receiveDetail.TransactionAmount, null, 0, itemDetail.IsNonCreditable);

                            receiveDetail.ChargesAmount = receiveDetail.TransactionAmount * ratio;
                            receiveDetail.AfterInvoiceRate = receiveDetail.WithInvoiceRate;
                            receiveDetail.BaseAmount = itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + receiveDetail.BaseAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                 Convert.ToDecimal(receiveDetail.BaseAmount);//* Convert.ToDecimal(entity.ToCurrencyRate);

                            itemDetail.TotalQty = Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty);
                            itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.BaseAmount) / itemDetail.TotalQty);

                            _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                            receiveDetail.InventoryMaterialId = itemDetail.MaterialMasterId;
                            UpdateGraph(receiveDetail);
                            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                            

                        }

                    }

                    foreach (var POReqDetail in entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId))
                    {
                        if (entity.IsNotNull())
                        {
                            var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(POReqDetail);
                            if (materialData.IsNotNull()) POReqDetail.InventoryMaterialId = materialData.Id;
                            ///TODO : Get total qyt and amount by country and issue qty
                            POReqDetail.TotalQty = Query(t => t.InventoryMaterialId == POReqDetail.InventoryMaterialId && t.Id != POReqDetail.Id).Select(t => t.BaseQty).Sum();
                            //var totalAmount = Query(t => t.InventoryMaterialId == POReqDetail.InventoryMaterialId && t.Id != POReqDetail.Id).Select(t => t.BaseAmount).Sum();

                            var materialMasterIds = new string[] { POReqDetail.MaterialMasterId };
                            var altUomIds = new string[] { POReqDetail.TransactionUoMId };
                            var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                            if (POReqDetail.BaseUOMId != POReqDetail.TransactionUoMId && POReqDetail.CurrencyId != POReqDetail.BaseCurrencyId
                                 && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                            {
                                POReqDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == POReqDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                POReqDetail.BaseQty = Convert.ToDecimal(POReqDetail.TransactionQty * POReqDetail.BaseUoMFactor);
                                POReqDetail.BaseAmount = POReqDetail.TransactionAmount * POReqDetail.ToCurrencyRate;

                                //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                                //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                            }
                            else if (POReqDetail.BaseUOMId == POReqDetail.TransactionUoMId && POReqDetail.CurrencyId != POReqDetail.BaseCurrencyId)
                            {
                                POReqDetail.BaseQty = POReqDetail.TransactionQty;
                                POReqDetail.BaseUoMFactor = POReqDetail.TransactionQty;
                                //entity.BaseAmount = entity.TransactionAmount * entity.ToCurrencyRate;
                                POReqDetail.BaseAmount = POReqDetail.TransactionAmount;

                                //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                                //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                            }
                            else if (POReqDetail.BaseUOMId != POReqDetail.TransactionUoMId && POReqDetail.CurrencyId == POReqDetail.BaseCurrencyId
                                && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                            {
                                POReqDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == POReqDetail.BaseUOMId && t.AlternativeUOMId == POReqDetail.TransactionUoMId).BaseUOMFactor);
                                POReqDetail.BaseQty = Convert.ToDecimal(POReqDetail.TransactionQty * POReqDetail.BaseUoMFactor);
                                POReqDetail.BaseAmount = POReqDetail.TransactionAmount;

                                //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                                //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.BaseAmount) / entity.TotalQty);
                            }
                            else
                            {
                                POReqDetail.BaseUoMFactor = POReqDetail.TransactionQty;
                                POReqDetail.BaseQty = POReqDetail.TransactionQty;
                                POReqDetail.BaseAmount = POReqDetail.TransactionAmount;
                            }
                           
                            var reqDetail = _materialRequsitionDetailsServiceService.Query(r => r.Id == POReqDetail.RequisitionDetailId).Select().FirstOrDefault();
                            if (reqDetail == null)
                                throw new CustomException("Requisition Details Or PO Details not found!");
                            reqDetail.PORcvQty = (reqDetail.PORcvQty - POReqDetail.PreviousQty) + POReqDetail.TransactionQty;
                            //reqDetail.PORcvQty += POReqDetail.TransactionQty;
                            if (reqDetail.TransactionQty < reqDetail.PORcvQty)
                                throw new CustomException("Received Qty can not cross balance Qty.");
                            if (reqDetail.TransactionQty == reqDetail.PORcvQty)
                            {
                                reqDetail.POQtyStatus = reqDetail.TransactionQty == reqDetail.PORcvQty;
                            }
                            else
                            {
                                reqDetail.POQtyStatus = false;
                            }
                            AuditService.UpdatedLog(reqDetail);
                            _reqDetailRepository.Update(reqDetail);

                            var Val = _poRequisitionDetailRepository.Query(r => r.RequisitionDetailId == reqDetail.Id).Select().FirstOrDefault();

                            if (Val == null)
                            {
                                var PoReqDetail = new PoRequisitionDetail
                                {
                                    Id = GetPK(),
                                    PoDetailId = NewId1,
                                    RequisitionDetailId = POReqDetail.RequisitionDetailId,
                                    TransactionQty = POReqDetail.TransactionQty,
                                    BaseQty = Convert.ToDecimal(POReqDetail.BaseQty),

                                };
                                AuditService.AddedLog(PoReqDetail);
                                _poRequisitionDetailRepository.Insert(PoReqDetail);
                            }
                            else
                            {
                                var PoReqDetail = new PoRequisitionDetail
                                {
                                    Id = Val.Id,
                                    PoDetailId = Val.PoDetailId,
                                    RequisitionDetailId = Val.RequisitionDetailId,
                                    TransactionQty = POReqDetail.TransactionQty,
                                    BaseQty = Convert.ToDecimal(POReqDetail.BaseQty),

                                };
                                AuditService.AddedLog(PoReqDetail);
                                _poRequisitionDetailRepository.Update(PoReqDetail);
                            }

                        }
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
                {
                    _unitOfWork.Rollback();
                }
            }
        }



        public void DeletePOByReq(string receiveDetailId)
        {
            var flag = false;
            try
            {
                var isNonCreditable = _receiveDetailRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[PurchaseOrder] AS A JOIN [TRN].[PurchaseOrderDetail] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + receiveDetailId + "'").FirstOrDefault();
                var data = Find(receiveDetailId);
                if (data.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _inventoryMaterialMasterService.UpdateFromReceive(data.InventoryMaterialId, receiveDetailId);
                    var taxCategoryList = _receiveTaxRepository.Query(t => t.InventoryReceiveDetailId == receiveDetailId).Select().ToList();
                    if (taxCategoryList.Count > 0)
                    {
                        foreach (var item in taxCategoryList)
                        {
                            item.ModelState = ModelState.Deleted;
                            _receiveTaxRepository.Delete(item);
                            _unitOfWork.SaveChanges();
                        }
                    }
                    var ratio = _inventoryReceiveService.GetChargesRatio(data.InventoryReceiveId, data.Id, 0, null, 0, isNonCreditable);
                    UpdateInventoryDetail(data, ratio, 1, isNonCreditable);
                    var res = _inventoryReceiveRepository.SqlQuery<int>(@"Select POId=Case when IR.POId IS NULL then 0 else 1 end from [TRN].PurchaseOrder PO Left JOIN [TRN].[InventoryReceive]  IR On IR.POId=PO.Id where PO.Id= '" + data.InventoryReceiveId + "'").FirstOrDefault();
                    if (res == 1)
                    {
                        throw new CustomException("Already Received In PO");

                    }
                  
                   var poRequisitionData= _poRequisitionDetailRepository.Query(r=>r.PoDetailId==data.Id).Select().ToList();
                    if (poRequisitionData.Count > 0) {
                        foreach (var item in poRequisitionData)
                        {
                            var invMaterial = _reqDetailRepository.SqlQuery<MaterialRequsitionDetails>(@"select * from trn.MaterialRequsitionDetails where id='" + item.RequisitionDetailId + "'").FirstOrDefault();
                            string _sql = "Update trn.MaterialRequsitionDetails set PORcvQty='" + Convert.ToDecimal(invMaterial.PORcvQty - item.TransactionQty) + "',POQtyStatus=0 where id='" + item.RequisitionDetailId + "'";
                            
                            _sqlRepository.ExecuteSqlCommand(_sql);
                            _poRequisitionDetailRepository.Delete(item);
                        }
                    }
                    
                    base.DeleteGraph(data);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();
                }
                else
                    throw new CustomException("Data not found");
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
                {
                    _unitOfWork.Rollback();
                }
            }
        }


        
        public void InsertServicePODetailByReq(IEnumerable<ServicePODetailsViewModel> entity, string ServicePoMasterId, IEnumerable<ServicePOTax> taxCategoryList)
        {


            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                //var currentId = _ServicePODetail.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(Id, CHARINDEX('-',id)+1,len(Id))    AS INT)), 0) Id FROM[TRN].[ServicePODetail] WHERE  ServiceMasterId = '{ServicePoMasterId}'").First();
                var currentId = _ServicePODetail.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(Id, CHARINDEX('-',id)+1,len(Id))    AS INT)), 0) Id FROM[TRN].[ServicePODetail] WHERE  ServicePOMasterId = '{ServicePoMasterId}'").First();


                if (entity != null)
                {
                    var ServicePOId = ServicePoMasterId + "-";
                    foreach (var ServiceitemDetail in entity)

                    {
                        currentId++;
                        if (ServiceitemDetail.Id == null)
                        {
                            var PurchaseDoService = new ServicePODetail
                            {
                                Id = ServicePOId + currentId,
                                ServicePOMasterId = ServicePoMasterId,
                                ServiceMasterId = ServiceitemDetail.ServiceMasterId,

                                Description = ServiceitemDetail.Description,
                                ServiceRequsitionDetailId = ServiceitemDetail.ServiceRequsitionDetailId,
                                ServiceReqMasterId = ServiceitemDetail.ServiceReqMasterId,
                                Qty = ServiceitemDetail.Qty,
                                Rate = ServiceitemDetail.TransactionRate,
                                TransactionUoMId = ServiceitemDetail.TransactionUoMId,
                                Amount = Math.Round(ServiceitemDetail.TransactionRate * ServiceitemDetail.Qty, 2),
                                TotalTaxAmount = Math.Round(ServiceitemDetail.TotalTaxAmount, 2),
                                GRNServiceAmount = Math.Round(ServiceitemDetail.GRNServiceAmount, 2),
                                AmountStatus = ServiceitemDetail.AmountStatus,
                                ModelState = ModelState.Added//r => r.ServiceMasterId == PurchaseDoService.ServiceMasterId
                            };
                            PurchaseDoService.TotalTaxAmount = taxCategoryList.Where(r => r.ServiceMasterId == PurchaseDoService.ServiceMasterId).Sum(t => t.TaxAmount);
                            //PurchaseDoService.TotalTaxAmount = Convert.ToDecimal(Query(t => t.ServiceMasterId == PurchaseDoService.ServiceMasterId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());
                            AuditService.AddedLog(PurchaseDoService);
                            _ServicePODetail.Insert(PurchaseDoService);
                            if (taxCategoryList.IsNotNull())
                            {
                                var currentId1 = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ServicePOTax] WHERE ServicePODetailid='{PurchaseDoService.Id}'").First();
                                foreach (var item in taxCategoryList.Where(r => r.ServiceMasterId == PurchaseDoService.ServiceMasterId && r.ServiceRequsitionDetailId == PurchaseDoService.ServiceRequsitionDetailId))
                                {
                                    currentId1++;
                                    item.Id = MakePK(PurchaseDoService.Id, currentId1, 2);
                                    item.ServicePOMasterId = PurchaseDoService.ServicePOMasterId;
                                    item.ServicePODetailId = PurchaseDoService.Id;
                                    item.TaxCategoryId = item.TaxCategoryId;
                                    item.HSNCodeId = item.HSNCodeId;
                                    item.Percentage = item.Percentage;
                                    item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                    AuditService.AddedLog(item);
                                    _ServicePOTax.Insert(item);
                                }
                            }
                        }
                    }
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();

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
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }

        }
        public void InsertServicePODetail(ServicePODetail entity, string ServicePoMasterId, IEnumerable<ServicePOTax> taxCategoryList)
        {


            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var currentId = _ServicePODetail.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(Id, CHARINDEX('-',id)+1,len(Id))    AS INT)), 0) Id FROM[TRN].[ServicePODetail] WHERE  ServicePOMasterId = '{ServicePoMasterId}'").First();

                if (entity != null)
                {
                    var ServicePOId = ServicePoMasterId + "-";
                    currentId++;
                    if (entity.Id == null)
                    {
                        var PurchaseDoService = new ServicePODetail
                        {
                            Id = ServicePOId + currentId,
                            ServicePOMasterId = ServicePoMasterId,
                            ServiceMasterId = entity.ServiceMasterId,
                            BudgetMasterId = entity.BudgetMasterId,
                            ActivityId = entity.ActivityId,
                            Qty = entity.Qty,
                            Rate = Math.Round(entity.Rate, 4),
                            Amount = Math.Round(entity.Amount, 2),
                            TotalTaxAmount = Math.Round(entity.TotalTaxAmount, 2),
                            GRNServiceAmount = Math.Round(entity.GRNServiceAmount, 2),
                            AmountStatus = entity.AmountStatus,
                            Description = entity.Description,
                            ServiceRequsitionDetailId = entity.ServiceRequsitionDetailId,
                            ServiceReqMasterId = entity.ServiceReqMasterId,
                            TransactionUoMId = entity.TransactionUoMId,
                            ModelState = ModelState.Added//r => r.ServiceMasterId == PurchaseDoService.ServiceMasterId
                        };
                        PurchaseDoService.TotalTaxAmount = taxCategoryList.Where(r => r.ServiceMasterId == PurchaseDoService.ServiceMasterId).Sum(t => t.TaxAmount);
                        //PurchaseDoService.TotalTaxAmount = Convert.ToDecimal(Query(t => t.ServiceMasterId == PurchaseDoService.ServiceMasterId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());
                        AuditService.AddedLog(PurchaseDoService);
                        _ServicePODetail.Insert(PurchaseDoService);
                        if (taxCategoryList.IsNotNull())
                        {

                            try
                            {
                                var currentId1 = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ServicePOTax] WHERE ServicePODetailid='{PurchaseDoService.Id}'").First();
                                foreach (var item in taxCategoryList.Where(r => r.ServiceMasterId == PurchaseDoService.ServiceMasterId))
                                {
                                    currentId1++;
                                    item.Id = MakePK(PurchaseDoService.Id, currentId1, 2);
                                    item.ServicePOMasterId = PurchaseDoService.ServicePOMasterId;
                                    item.ServicePODetailId = PurchaseDoService.Id;
                                    item.TaxCategoryId = item.TaxCategoryId;
                                    item.HSNCodeId = item.HSNCodeId;
                                    item.Percentage = item.Percentage;
                                    item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                    AuditService.AddedLog(item);
                                    _ServicePOTax.Insert(item);
                                }
                            }
                            catch (Exception ex)
                            {

                                throw ex;
                            }
                        }

                    }
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();

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
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }

        }
        public void GetUpdateServicePOTax(IEnumerable<ServicePOTaxViewModel> receiveTaxList, string ServicePODetailId, string servicePOid)
        {
            var sql = "";
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                //var currentId = _ServicePODetail.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(Id, CHARINDEX('-',id)+1,len(Id))    AS INT)), 0) Id FROM[TRN].[ServicePODetail] WHERE  ServiceMasterId = '{ServicePoMasterId}'").First();
                var currentId = _ServicePODetail.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(Id, CHARINDEX('-',id)+1,len(Id))    AS INT)), 0) Id FROM[TRN].[ServicePOTax] WHERE  ServicePOMasterId = '{servicePOid}'").First();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (receiveTaxList != null)
                {
                    var servicePODetail = _ServicePODetail.Find(ServicePODetailId);
                    var ServicePOId = servicePOid + "-";
                    foreach (var ServiceitemDetail in receiveTaxList)
                    {
                        if (string.IsNullOrEmpty(ServiceitemDetail.Id))
                        {
                            currentId++;
                            if (ServiceitemDetail.Id == null)
                            {
                                var PurchaseDoService = new ServicePOTax
                                {
                                    Id = ServicePOId + currentId,
                                    ServicePOMasterId = servicePOid,
                                    ServicePODetailId = ServicePODetailId,
                                    TaxCategoryId = ServiceitemDetail.TaxCategoryId,
                                    HSNCodeId = ServiceitemDetail.HSNCodeId,
                                    Percentage = ServiceitemDetail.Percentage,
                                    TaxAmount = ServiceitemDetail.TaxAmount,
                                    ModelState = ModelState.Added
                                };

                                AuditService.AddedLog(PurchaseDoService);
                                _ServicePOTax.Insert(PurchaseDoService);

                            }
                        }
                        else
                        {
                            var PurchaseDoService = new ServicePOTax
                            {
                                Id = ServiceitemDetail.Id,
                                ServicePOMasterId = servicePOid,
                                ServicePODetailId = ServicePODetailId,
                                TaxCategoryId = ServiceitemDetail.TaxCategoryId,
                                HSNCodeId = ServiceitemDetail.HSNCodeId,
                                Percentage = ServiceitemDetail.Percentage,
                                TaxAmount = ServiceitemDetail.TaxAmount,
                                ModelState = ModelState.Added
                            };
                            AuditService.UpdatedLog(PurchaseDoService);
                            _ServicePOTax.Update(PurchaseDoService);
                        }
                    }

                    servicePODetail.TotalTaxAmount = receiveTaxList.Sum(r => r.TaxAmount);
                    servicePODetail.UpdatedDate = DateTime.Now;
                    servicePODetail.UpdatedFromIP = identity.IPAddress;
                    servicePODetail.UpdatedBy = identity.Name;
                    _ServicePODetail.Update(servicePODetail);
                    _unitOfWork.SaveChanges();
                    flag = false;
                    _unitOfWork.Commit();

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
                 ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
            finally
            {
                if (flag)
                {
                    _unitOfWork.Rollback();
                }
            }
        }
        #endregion


        #region PO bOQ Item SAve

        public void InsertOrUpdateGraphPoForBOQItem(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {
            //Sk
            List<POBOQMap> abc = new List<POBOQMap>();
            var flag = false;
            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var NewId = "";
                var NewId1 = "";
                var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(Id, CHARINDEX('-',id)+1,len(Id))    AS INT)), 0) Id FROM[TRN].[PurchaseOrderDetail] WHERE  InventoryReceiveId = '{PoId}'").First();
                decimal TransactionQtyGroupSum = 0;

                var groupListentity = groupList;
                foreach (var itemDetail in groupListentity)
                {
                    var refferenceNo = "";
                    if (entity.IsNotNull())
                    {

                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);
                        if (!string.IsNullOrEmpty(itemDetail.MaterialMasterId))
                        {
                            TransactionQtyGroupSum = entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId).Sum(r => r.TransactionQty);
                            var lst = entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId).ToList();
                            
                            var ViewModelData = new List<InventoryMaterialViewModel>();                         //var newList = new string[]{  }; 
                            foreach (var lstt1 in lst)
                            {
                                if (ViewModelData.Count == 0)
                                {
                                    ViewModelData.Add(lstt1);
                                }
                                else
                                {
                                    try
                                    {
                                        var tempViewModelData = ViewModelData.Where(r => r.RefferenceNo == lstt1.RefferenceNo).Select(r => r.RefferenceNo).FirstOrDefault();
                                        if (tempViewModelData == null)
                                        {
                                            ViewModelData.Add(lstt1);
                                        }
                                    }
                                    catch (Exception)
                                    { }
                                }
                            }


                            foreach (var lstt in ViewModelData)
                            {
                                if (refferenceNo == "")
                                    refferenceNo += "" + lstt.RefferenceNo;
                                else

                                    refferenceNo += "," + lstt.RefferenceNo;
                            }
                           
                        }
                        else
                        {
                            TransactionQtyGroupSum = itemDetail.TransactionQty;

                        }

                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            //itemDetail.BaseQty = Convert.ToDecimal(TransactionQtyGroupSum * itemDetail.BaseUoMFactor);

                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            //double conversiongroupListData1 = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            //itemDetail.POBOQQty = Convert.ToDecimal(conversiongroupListData1);

                            itemDetail.BaseAmount = itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;

                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {
                            //itemDetail.BaseQty = TransactionQtyGroupSum;
                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            //double conversiongroupListData1 = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            //itemDetail.POBOQQty = Convert.ToDecimal(conversiongroupListData1);
                            itemDetail.BaseUoMFactor = TransactionQtyGroupSum;
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;

                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {


                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }
                        else
                        {

                            itemDetail.BaseUoMFactor = TransactionQtyGroupSum;
                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }

                        //End Update Req Table
                        itemDetail.Id = "";
                        if (string.IsNullOrEmpty(itemDetail.MaterialStorageId))
                        {
                            //TransactionQty = TransactionQty;
                        }
                        // Insert in receive detail
                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {


                            NewId = PoId + "-";
                            currentId++;
                            var receiveDetail = new PurchaseOrderDetail
                            {

                                Id = NewId + currentId,
                                MaterialStorageId = itemDetail.MaterialStorageId,
                                InventoryReceiveId = PoId,
                                InventoryMaterialId = itemDetail.MaterialMasterId,
                                TransactionQty = TransactionQtyGroupSum,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),//Convert.ToDecimal(itemDetail.BaseQtyNew),//
                                BaseUOMId = itemDetail.BaseUOMId,
                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                TransactionRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                TransactionAmount = Math.Round(TransactionQtyGroupSum * Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4), 2),//Convert.ToDecimal(itemDetail.TransactionAmount),
                                BaseAmount = Math.Round(TransactionQtyGroupSum * Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4), 2),//Convert.ToDecimal(itemDetail.BaseAmount),
                                TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalTaxAmount), 2),
                                IssueQty = null,
                                GRNRcvQty = 0,
                                QtyStatus = false,
                                CountryId = itemDetail.CountryId,
                                MasterOrderId = null,
                                MasterOrderDetailId = null,
                                Description = itemDetail.Description,
                                DeliveryDate = itemDetail.DeliveryDate,
                                RequisitionId = itemDetail.RequisitionId,
                                RequisitionDetailId = itemDetail.RequisitionDetailId,
                                ArticleId = itemDetail.ArticleId,
                                FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                AcceptanceRcvQty = 0,
                                AcceptanceRcvStatusQty = false,
                                RefferenceNo = refferenceNo,//itemDetail.RefferenceNo
                                Tolerance = itemDetail.Tolerance



                            };
                            NewId1 = receiveDetail.Id;

                            itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                            AuditService.AddedLog(receiveDetail);
                            var ratio = _inventoryReceiveService.GetChargesRatio(receiveDetail.InventoryReceiveId, receiveDetail.Id, receiveDetail.TransactionAmount, null, 0, itemDetail.IsNonCreditable);

                            receiveDetail.ChargesAmount = receiveDetail.TransactionAmount * ratio;

                            receiveDetail.BaseAmount = itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + receiveDetail.BaseAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                 Convert.ToDecimal(receiveDetail.BaseAmount);//* Convert.ToDecimal(entity.ToCurrencyRate);

                            itemDetail.TotalQty = Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty);
                            itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.BaseAmount) / itemDetail.TotalQty);

                            _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                            receiveDetail.InventoryMaterialId = itemDetail.MaterialMasterId;
                            InsertGraph(receiveDetail);

                            // insert in receive tax
                            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                            //var list =

                            var list = _inventoryReceiveService.GetTaxCategoryList1(identity.CompanyGroupId, PoId, identity.PlantId, itemDetail.HSNCodeId);

                            if (list.IsNotNull())
                            {
                                var currentIdTax = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                                foreach (var item in list)
                                {

                                    //new Di
                                    //var v = new System.Collections.Generic.Dictionary<string, object>(item).Items[""].
                                    var potax = new PurchaseOrderTax();
                                    currentIdTax++;
                                    potax.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentIdTax, 2);
                                    potax.InventoryReceiveId = PoId;
                                    potax.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                                    potax.InventoryServiceId = null;
                                    potax.HSNCodeId = itemDetail.HSNCodeId;
                                    potax.TaxCategoryId = item.TaxCategoryId;
                                    potax.Percentage = item.Percentage;
                                    potax.TaxAmount = item.TaxAmount;
                                    potax.ModelState = ModelState.Added;
                                    AuditService.AddedLog(potax);
                                    _receiveTaxRepository.Insert(potax);
                                }
                            }

                        }

                    }

                    //POBOQMap PoBOQDetail = new POBOQMap();
                    foreach (var POReqDetail in entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId))

                    {
                        try
                        {

                            var PoBOQDetail = new POBOQMap
                            {
                                Id = GetPOBOQPK(),
                                PODetailId = NewId1,
                                BOQDetailId = POReqDetail.BOQId,
                                TransactionQty = POReqDetail.TransactionQty,
                                TransactionUoMId = POReqDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                BaseUoMId = itemDetail.BaseUOMId,
                                POUoMId = itemDetail.POUoMId,
                                POBOQQty = Convert.ToDecimal(conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(POReqDetail.TransactionQty)).ToString("F2")),
                            };
                            AuditService.AddedLog(PoBOQDetail);
                            _POBOQMapRepository.Insert(PoBOQDetail);
                            //abc.Add(PoBOQDetail);
                        }
                        catch (Exception e)
                        {

                        }
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
                {
                    _unitOfWork.Rollback();
                }
            }
        }


        public void InsertOrUpdateGraphPoForBOQItemUpdate(IEnumerable<InventoryMaterialViewModel> entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {

            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var NewId = "";
                var NewId1 = "";
                var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[PurchaseOrderDetail] WHERE  InventoryReceiveId = '{PoId}'").First();
                var groupListentity = groupList;
                var PODetailsId = "";
                foreach (var itemDetail in groupListentity)
                {
                    var refferenceNo = "";
                    if (entity.IsNotNull())
                    {
                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseAmount).Sum();
                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);
                        var TransactionQty = entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId).Sum(r => r.TransactionQty);
                        var lst = entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId).ToList();

                        var ViewModelData = new List<InventoryMaterialViewModel>();                         //var newList = new string[]{  }; 
                        foreach (var lstt1 in lst)
                        {
                            if (ViewModelData.Count == 0)
                            {
                                ViewModelData.Add(lstt1);
                            }
                            else
                            {
                                var tempViewModelData = ViewModelData.Where(r => r.RefferenceNo == lstt1.RefferenceNo).Select(r => r.RefferenceNo).FirstOrDefault();
                                if (tempViewModelData == null)
                                {
                                    ViewModelData.Add(lstt1);

                                }

                            }
                        }


                        foreach (var lstt in ViewModelData)
                        {
                            if (refferenceNo == "")
                                refferenceNo += "" + lstt.RefferenceNo;
                            else

                                refferenceNo += "," + lstt.RefferenceNo;
                        }
                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);

                            itemDetail.BaseQty = Convert.ToDecimal(TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;

                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {
                            itemDetail.BaseQty = TransactionQty;
                            itemDetail.BaseUoMFactor = TransactionQty;
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;

                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                            && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseQty = Convert.ToDecimal(TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;

                        }
                        else
                        {
                            itemDetail.BaseUoMFactor = TransactionQty;
                            itemDetail.BaseQty = TransactionQty;
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }

                        //End Update Req Table
                        itemDetail.Id = "";
                        var PODetailId = entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId && r.InventoryReceiveDetailId != "").FirstOrDefault();
                        PODetailsId = PODetailId.InventoryReceiveDetailId;
                        // Insert in receive detail
                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {

                            NewId = PoId + "-";
                            currentId++;
                            var receiveDetail = new PurchaseOrderDetail
                            {
                                Id = PODetailId.InventoryReceiveDetailId.ToString(),//itemDetail.InventoryReceiveDetailId, 
                                MaterialStorageId = itemDetail.MaterialStorageId,
                                InventoryReceiveId = PoId, //itemDetail.InventoryReceiveId,
                                InventoryMaterialId = itemDetail.MaterialMasterId,
                                TransactionQty = TransactionQty,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                BaseUOMId = itemDetail.TransactionUoMId,
                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                TransactionRate = Convert.ToDecimal(itemDetail.TransactionRate),

                                //TransactionAmount = Convert.ToDecimal(itemDetail.TransactionAmount),
                                //BaseAmount = Convert.ToDecimal(itemDetail.BaseAmount),
                                TransactionAmount = Math.Round(TransactionQty * Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4), 2),//Convert.ToDecimal(itemDetail.TransactionAmount),
                                BaseAmount = Math.Round(TransactionQty * Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4), 2),//Convert.ToDecimal(itemDetail.BaseAmount),


                                TotalTaxAmount = Convert.ToDecimal(itemDetail.TotalTaxAmount),
                                IssueQty = null,
                                GRNRcvQty = 0,
                                QtyStatus = false,
                                CountryId = itemDetail.CountryId,
                                MasterOrderId = null,
                                MasterOrderDetailId = null,
                                Description = itemDetail.Description,
                                DeliveryDate = itemDetail.DeliveryDate,
                                RequisitionId = itemDetail.RequisitionId,
                                RequisitionDetailId = itemDetail.RequisitionDetailId,
                                ArticleId = itemDetail.ArticleId,
                                FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                AcceptanceRcvQty = 0,
                                AcceptanceRcvStatusQty = false,
                                RefferenceNo = refferenceNo,//itemDetail.RefferenceNo
                                Tolerance = itemDetail.Tolerance
                            };
                            NewId1 = receiveDetail.Id;

                            itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                            AuditService.AddedLog(receiveDetail);
                            var ratio = _inventoryReceiveService.GetChargesRatio(receiveDetail.InventoryReceiveId, receiveDetail.Id, receiveDetail.TransactionAmount, null, 0, itemDetail.IsNonCreditable);

                            receiveDetail.ChargesAmount = receiveDetail.TransactionAmount * ratio;
                            receiveDetail.AfterInvoiceRate = receiveDetail.WithInvoiceRate;

                            receiveDetail.BaseAmount = itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + receiveDetail.BaseAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                 Convert.ToDecimal(receiveDetail.BaseAmount);//* Convert.ToDecimal(entity.ToCurrencyRate);

                            itemDetail.TotalQty = Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty);
                            itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.BaseAmount) / itemDetail.TotalQty);

                            _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                            receiveDetail.InventoryMaterialId = itemDetail.MaterialMasterId;
                            UpdateGraph(receiveDetail);
                            // insert in receive tax
                            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                            
                        }

                    }

                    Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
                    //double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                    foreach (var POReqDetail in entity.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId && r.ThirdCharacteristicsValueId == r.ThirdCharacteristicsValueId))
                    {
                        if (entity.IsNotNull())
                        {
                            var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(POReqDetail);
                            if (materialData.IsNotNull()) POReqDetail.InventoryMaterialId = materialData.Id;
                            ///TODO : Get total qyt and amount by country and issue qty
                            POReqDetail.TotalQty = Query(t => t.InventoryMaterialId == POReqDetail.InventoryMaterialId && t.Id != POReqDetail.Id).Select(t => t.BaseQty).Sum();
                            var materialMasterIds = new string[] { POReqDetail.MaterialMasterId };
                            var altUomIds = new string[] { POReqDetail.TransactionUoMId };
                            var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);
                            if (POReqDetail.BaseUOMId != POReqDetail.TransactionUoMId && POReqDetail.CurrencyId != POReqDetail.BaseCurrencyId
                                 && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                            {
                                POReqDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == POReqDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                POReqDetail.BaseQty = Convert.ToDecimal(POReqDetail.TransactionQty * POReqDetail.BaseUoMFactor);
                                POReqDetail.BaseAmount = POReqDetail.TransactionAmount * POReqDetail.ToCurrencyRate;
                            }
                            else if (POReqDetail.BaseUOMId == POReqDetail.TransactionUoMId && POReqDetail.CurrencyId != POReqDetail.BaseCurrencyId)
                            {
                                POReqDetail.BaseQty = POReqDetail.TransactionQty;
                                POReqDetail.BaseUoMFactor = POReqDetail.TransactionQty;
                                POReqDetail.BaseAmount = POReqDetail.TransactionAmount;
                            }
                            else if (POReqDetail.BaseUOMId != POReqDetail.TransactionUoMId && POReqDetail.CurrencyId == POReqDetail.BaseCurrencyId
                                && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                            {
                                POReqDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == POReqDetail.BaseUOMId && t.AlternativeUOMId == POReqDetail.TransactionUoMId).BaseUOMFactor);
                                POReqDetail.BaseQty = Convert.ToDecimal(POReqDetail.TransactionQty * POReqDetail.BaseUoMFactor);
                                POReqDetail.BaseAmount = POReqDetail.TransactionAmount;
                            }
                            else
                            {
                                POReqDetail.BaseUoMFactor = POReqDetail.TransactionQty;
                                POReqDetail.BaseQty = POReqDetail.TransactionQty;
                                POReqDetail.BaseAmount = POReqDetail.TransactionAmount;
                            }

                            if (string.IsNullOrEmpty(POReqDetail.SavedPOBOQId))
                            {
                                var PoReqDetail = new POBOQMap
                                {

                                    Id = GetPOBOQPK(),
                                    PODetailId = NewId1,
                                    BOQDetailId = POReqDetail.BOQId,
                                    TransactionQty = POReqDetail.TransactionQty,
                                    TransactionUoMId = POReqDetail.TransactionUoMId,
                                    BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                    BaseUoMId = itemDetail.BaseUOMId,
                                    POUoMId = itemDetail.POUoMId,
                                    POBOQQty = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(POReqDetail.TransactionQty)),
                                };
                                AuditService.AddedLog(PoReqDetail);
                                _POBOQMapRepository.Insert(PoReqDetail);
                            }
                            else
                            {
                                var POBOQMapRepository = _POBOQMapRepository.Query(t => t.PODetailId == PODetailsId.ToString()).Select().ToList();
                                if (POBOQMapRepository.IsNotNull())
                                {
                                    foreach (var item in POBOQMapRepository)
                                    {
                                        item.ModelState = ModelState.Deleted;
                                        _POBOQMapRepository.Delete(item);
                                    }
                                }

                                var PoReqDetail = new POBOQMap
                                {

                                    Id = GetPOBOQPK(),
                                    PODetailId = NewId1,
                                    BOQDetailId = POReqDetail.BOQId,
                                    TransactionQty = POReqDetail.TransactionQty,
                                    TransactionUoMId = POReqDetail.TransactionUoMId,
                                    BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                    BaseUoMId = itemDetail.BaseUOMId,
                                    POUoMId = itemDetail.POUoMId,
                                    POBOQQty = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(POReqDetail.TransactionQty)),
                                };
                                AuditService.AddedLog(PoReqDetail);
                                _POBOQMapRepository.Insert(PoReqDetail);
                            }
                        }
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
                {
                    _unitOfWork.Rollback();
                }
            }
        }


        #endregion

        #region POBoqInsertUpdate

        private string GetTermsAndConditionsPOChildPK()
        {
            return _pkGeneratorService.GetAutoNumber(nameof(TermsAndConditionsPOChild), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }
        public void POBoqInsertUpdate(PurchaseOrder entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<InventoryMaterialViewModel> boqmapList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {
            //Sk
            List<POBOQMap> abc = new List<POBOQMap>();
            var flag = false;
            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var NewId = "";
                var NewId1 = "";
                var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(Id, CHARINDEX('-',id)+1,len(Id))    AS INT)), 0) Id FROM[TRN].[PurchaseOrderDetail] WHERE  InventoryReceiveId = '{PoId}'").First();
                decimal TransactionQtyGroupSum = 0;
                _inventoryReceiveService.InsertPOBOQMaster(entity);

                var groupListentity = groupList;
                foreach (var itemDetail in groupListentity)
                {
                    var refferenceNo = "";
                    if (entity.IsNotNull())
                    {

                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                        TransactionQtyGroupSum = itemDetail.TransactionQty;

                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && entity.CurrencyId != entity.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);

                            itemDetail.BaseAmount = itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;

                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && entity.CurrencyId != entity.BaseCurrencyId)
                        {
                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            itemDetail.BaseUoMFactor = 1;
                            itemDetail.BaseAmount = itemDetail.BaseQty * itemDetail.ToCurrencyRate;

                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && entity.CurrencyId == entity.BaseCurrencyId && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseAmount = (itemDetail.BaseQty * itemDetail.TransactionRate) / itemDetail.BaseUoMFactor;
                        }
                        else
                        {

                            itemDetail.BaseUoMFactor = 1;
                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }



                        //End Update Req Table
                        itemDetail.Id = "";
                        if (string.IsNullOrEmpty(itemDetail.MaterialStorageId))
                        {
                            //TransactionQty = TransactionQty;
                        }
                        // Insert in receive detail
                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {


                            NewId = entity.Id + "-";
                            currentId++;
                            var receiveDetail = new PurchaseOrderDetail
                            {

                                Id = NewId + currentId,
                                MaterialStorageId = itemDetail.MaterialStorageId,
                                InventoryReceiveId = entity.Id,
                                InventoryMaterialId = itemDetail.MaterialMasterId,
                                TransactionQty = TransactionQtyGroupSum,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),//Convert.ToDecimal(itemDetail.BaseQtyNew),//
                                BaseUOMId = itemDetail.BaseUOMId,
                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                TransactionRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                TransactionAmount = Math.Round(TransactionQtyGroupSum * Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4), 2),//Convert.ToDecimal(itemDetail.TransactionAmount),
                                BaseAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseQty), 2) * Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 2),//Convert.ToDecimal(itemDetail.BaseAmount),
                                TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalTaxAmount), 2),
                                IssueQty = null,
                                GRNRcvQty = 0,
                                QtyStatus = false,
                                CountryId = itemDetail.CountryId,
                                MasterOrderId = null,
                                MasterOrderDetailId = null,
                                Description = itemDetail.Description,
                                DeliveryDate = itemDetail.DeliveryDate,
                                RequisitionId = itemDetail.RequisitionId,
                                RequisitionDetailId = itemDetail.RequisitionDetailId,
                                ArticleId = itemDetail.ArticleId,
                                FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                AcceptanceRcvQty = 0,
                                AcceptanceRcvStatusQty = false,
                                RefferenceNo = itemDetail.RefferenceNo,
                                Tolerance = itemDetail.Tolerance



                            };
                            NewId1 = receiveDetail.Id;

                            itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                            AuditService.AddedLog(receiveDetail);
                            var ratio = _inventoryReceiveService.GetChargesRatio(receiveDetail.InventoryReceiveId, receiveDetail.Id, receiveDetail.TransactionAmount, null, 0, itemDetail.IsNonCreditable);

                            receiveDetail.ChargesAmount = receiveDetail.TransactionAmount * ratio;

                            receiveDetail.BaseAmount = itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + receiveDetail.BaseAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                 Convert.ToDecimal(receiveDetail.BaseAmount);//* Convert.ToDecimal(entity.ToCurrencyRate);

                            itemDetail.TotalQty = Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty);
                            itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.BaseAmount) / itemDetail.TotalQty);

                            _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                            receiveDetail.InventoryMaterialId = itemDetail.MaterialMasterId;
                            InsertGraph(receiveDetail);

                            // insert in receive tax
                            //var list =
                            BOQQueryService purchaseOrderBOQQueryService = new BOQQueryService(_sqlRepository);
                            var list = purchaseOrderBOQQueryService.GetPOBOQTaxCategoryList(entity.CompanyGroupId, entity.InvoicingPartyPlantId, entity.PlantId, itemDetail.HSNCodeId);

                            if (list.IsNotNull())
                            {
                                var currentIdTax = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                                foreach (var item in list)
                                {

                                    //new Di
                                    //var v = new System.Collections.Generic.Dictionary<string, object>(item).Items[""].
                                    var potax = new PurchaseOrderTax();
                                    currentIdTax++;
                                    potax.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentIdTax, 2);
                                    potax.InventoryReceiveId = entity.Id;
                                    potax.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                                    potax.InventoryServiceId = null;
                                    potax.HSNCodeId = itemDetail.HSNCodeId;
                                    potax.TaxCategoryId = item.TaxCategoryId;
                                    potax.Percentage = item.Percentage;
                                    potax.TaxAmount = receiveDetail.TransactionAmount* (item.Percentage/100);
                                    potax.ModelState = ModelState.Added;
                                    AuditService.AddedLog(potax);
                                    _receiveTaxRepository.Insert(potax);
                                }
                            }

                        }

                    }

                    //POBOQMap PoBOQDetail = new POBOQMap();
                    foreach (var POReqDetail in boqmapList.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId
                                                        && r.ThirdCharacteristicsValueId == itemDetail.ThirdCharacteristicsValueId && r.POCriteria == itemDetail.POCriteria && itemDetail.GroupId==r.GroupId))

                    {
                        try
                        {

                            var PoBOQDetail = new POBOQMap
                            {
                                Id = GetPOBOQPK(),
                                PODetailId = NewId1,
                                BOQDetailId = POReqDetail.BOQId,
                                TransactionQty = POReqDetail.TransactionQty,
                                TransactionUoMId = POReqDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseUoMFactor * POReqDetail.TransactionQty),
                                BaseUoMId = itemDetail.BaseUOMId,
                                POUoMId = itemDetail.POUoMId,
                                POBOQQty = Convert.ToDecimal(conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(POReqDetail.TransactionQty)).ToString("F2")),
                            };
                            AuditService.AddedLog(PoBOQDetail);
                            _POBOQMapRepository.Insert(PoBOQDetail);
                            //abc.Add(PoBOQDetail);



                        }
                        catch (Exception e)
                        {

                        }
                    }


                }

                if (entity.TermsAndConditionsId != null)
                {
                    string NewSoId = string.Empty;
                    DataTable dtFromMaster = _sqlRepository.GetDataTable("SELECT * FROM  TermsAndConditionsChild WHERE TermsAndConditionsMasterId='" + entity.TermsAndConditionsId + "'");
                    DataTable dtFromFirstCharacteristics = _sqlRepository.GetDataTable("SELECT * FROM TermsAndConditionsDetails Where TermsAndConditionsChildId IN(Select Id from TermsAndConditionsChild Where TermsAndConditionsMasterId='" + entity.TermsAndConditionsId + "')");
                    int SCount = 0;
                    for (int m = 0; m < dtFromMaster.Rows.Count; m++)
                    {
                        TermsAndConditionsPOChild termsAndConditionsPOChild = new TermsAndConditionsPOChild();
                        TermsAndConditionsPODetails termsAndConditionsPODetails = new TermsAndConditionsPODetails();
                        SCount++;
                        termsAndConditionsPOChild.Id = entity.TermsAndConditionsId + GetTermsAndConditionsPOChildPK() + SCount;
                        NewSoId = termsAndConditionsPOChild.Id;
                        termsAndConditionsPOChild.POId = entity.Id;
                        termsAndConditionsPOChild.Title = dtFromMaster.Rows[m]["Title"].ToString();
                        termsAndConditionsPOChild.AddedBy = entity.AddedBy;
                        termsAndConditionsPOChild.AddedDate = entity.AddedDate;
                        termsAndConditionsPOChild.AddedFromIP = entity.AddedFromIP;
                        _termsAndConditionsPOChildRepository.Insert(termsAndConditionsPOChild);

                        for (int i = 0; i < dtFromFirstCharacteristics.DefaultView.Count; i++)
                        {

                            termsAndConditionsPODetails.Id = NewSoId + (i + 1);
                            termsAndConditionsPODetails.TermsAndConditionsPOChildId = NewSoId;
                            termsAndConditionsPODetails.HeaderCaption = dtFromFirstCharacteristics.DefaultView[i].Row["HeaderCaption"].ToString();
                            termsAndConditionsPODetails.Description = dtFromFirstCharacteristics.DefaultView[i].Row["Description"].ToString();
                            termsAndConditionsPODetails.AddedBy = entity.AddedBy;
                            termsAndConditionsPODetails.AddedDate = entity.AddedDate;
                            termsAndConditionsPODetails.AddedFromIP = entity.AddedFromIP;
                            _termsAndConditionsPODetailRepository.Insert(termsAndConditionsPODetails);
                        }
                    }

                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
                {
                    _unitOfWork.Rollback();
                }
            }
        }
        public void POBoqUpdate(PurchaseOrder entity, IEnumerable<InventoryMaterialViewModel> groupList, IEnumerable<InventoryMaterialViewModel> boqmapList, IEnumerable<PurchaseOrderTax> taxCategoryList, string PoId)
        {
            //Sk
            List<POBOQMap> abc = new List<POBOQMap>();
            var flag = false;
            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var NewId = "";
                var NewId1 = "";
                var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(Id, CHARINDEX('-',id)+1,len(Id))    AS INT)), 0) Id FROM[TRN].[PurchaseOrderDetail] WHERE  InventoryReceiveId = '{PoId}'").First();
                decimal TransactionQtyGroupSum = 0;
                _inventoryReceiveService.UpdateGraph(entity);

                var groupListentity = groupList;
                foreach (var itemDetail in groupListentity)
                {
                    var refferenceNo = "";
                    if (entity.IsNotNull())
                    {

                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                        TransactionQtyGroupSum = itemDetail.TransactionQty;

                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && entity.CurrencyId != entity.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;

                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && entity.CurrencyId != entity.BaseCurrencyId)
                        {
                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            itemDetail.BaseUoMFactor = TransactionQtyGroupSum;
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;

                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && entity.CurrencyId == entity.BaseCurrencyId && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }
                        else
                        {

                            itemDetail.BaseUoMFactor = 1;
                            double conversiongroupListData = conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.TransactionUoMId.ToString(), Convert.ToDouble(TransactionQtyGroupSum));//TODO: Should Pass Base UOM
                            itemDetail.BaseQty = Convert.ToDecimal(conversiongroupListData);
                            itemDetail.BaseAmount = itemDetail.TransactionAmount;
                        }



                        //End Update Req Table

                        if (string.IsNullOrEmpty(itemDetail.MaterialStorageId))
                        {
                            //TransactionQty = TransactionQty;
                        }
                        // Insert in receive detail
                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {


                            NewId = entity.Id + "-";
                            currentId++;
                            var receiveDetail = new PurchaseOrderDetail
                            {

                                Id = NewId + currentId,
                                MaterialStorageId = itemDetail.MaterialStorageId,
                                InventoryReceiveId = entity.Id,
                                InventoryMaterialId = itemDetail.MaterialMasterId,
                                TransactionQty = TransactionQtyGroupSum,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),//Convert.ToDecimal(itemDetail.BaseQtyNew),//
                                BaseUOMId = itemDetail.BaseUOMId,
                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                TransactionRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                TransactionAmount = Math.Round(TransactionQtyGroupSum * Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4), 2),//Convert.ToDecimal(itemDetail.TransactionAmount),
                                BaseAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseQty), 2) * Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 2),//Convert.ToDecimal(itemDetail.BaseAmount),
                                TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalTaxAmount), 2),
                                IssueQty = null,
                                GRNRcvQty = 0,
                                QtyStatus = false,
                                CountryId = itemDetail.CountryId,
                                MasterOrderId = null,
                                MasterOrderDetailId = null,
                                Description = itemDetail.Description,
                                DeliveryDate = itemDetail.DeliveryDate,
                                RequisitionId = itemDetail.RequisitionId,
                                RequisitionDetailId = itemDetail.RequisitionDetailId,
                                ArticleId = itemDetail.ArticleId,
                                FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                                AcceptanceRcvQty = 0,
                                AcceptanceRcvStatusQty = false,
                                RefferenceNo = itemDetail.RefferenceNo,
                                Tolerance = itemDetail.Tolerance
                            };
                            NewId1 = receiveDetail.Id;

                            itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                            AuditService.AddedLog(receiveDetail);
                            var ratio = _inventoryReceiveService.GetChargesRatio(receiveDetail.InventoryReceiveId, receiveDetail.Id, receiveDetail.TransactionAmount, null, 0, itemDetail.IsNonCreditable);

                            receiveDetail.ChargesAmount = receiveDetail.TransactionAmount * ratio;

                            receiveDetail.BaseAmount = itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + receiveDetail.BaseAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                 Convert.ToDecimal(receiveDetail.BaseAmount);//* Convert.ToDecimal(entity.ToCurrencyRate);

                            itemDetail.TotalQty = Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty);
                            itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.BaseAmount) / itemDetail.TotalQty);

                            _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                            receiveDetail.InventoryMaterialId = itemDetail.MaterialMasterId;
                            InsertGraph(receiveDetail);
                            // insert in receive tax
                            //var list =
                            BOQQueryService purchaseOrderBOQQueryService = new BOQQueryService(_sqlRepository);
                            var list = purchaseOrderBOQQueryService.GetPOBOQTaxCategoryList(entity.CompanyGroupId, entity.InvoicingPartyPlantId, entity.PlantId, itemDetail.HSNCodeId) ;

                            if (receiveDetail.TotalTaxAmount == 0 && list.Count()>0)
                            {
                                receiveDetail.TotalTaxAmount = receiveDetail.TransactionAmount * (list.Sum(r=>r.Percentage)) / 100; 
                            }
                            if (list.IsNotNull())
                            {
                                var currentIdTax = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseOrderTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                                foreach (var item in list)
                                {

                                    //new Di
                                    //var v = new System.Collections.Generic.Dictionary<string, object>(item).Items[""].
                                    var potax = new PurchaseOrderTax();
                                    currentIdTax++;
                                    potax.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentIdTax, 2);
                                    potax.InventoryReceiveId = entity.Id;
                                    potax.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                                    potax.InventoryServiceId = null;
                                    potax.HSNCodeId = itemDetail.HSNCodeId;
                                    potax.TaxCategoryId = item.TaxCategoryId;
                                    potax.Percentage = item.Percentage;
                                    potax.TaxAmount = receiveDetail.TransactionAmount * (item.Percentage / 100);
                                    potax.ModelState = ModelState.Added;
                                    AuditService.AddedLog(potax);
                                    _receiveTaxRepository.Insert(potax);
                                }
                            }

                        }

                    }

                    //POBOQMap PoBOQDetail = new POBOQMap();
                    //foreach (var POReqDetail in boqmapList.Where(r => r.PODetailsID == itemDetail.Id ))
                    foreach (var POReqDetail in boqmapList.Where(r => r.MaterialMasterId == itemDetail.MaterialMasterId && r.ArticleId == itemDetail.ArticleId && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId
                                                                            && r.ThirdCharacteristicsValueId == itemDetail.ThirdCharacteristicsValueId 
                                                                            && r.POCriteria == itemDetail.POCriteria && r.BOQDetailId==itemDetail.BOQDetailId))

                    {
                        try
                        {
                            if (string.IsNullOrEmpty(POReqDetail.Id))
                            {
                                var PoBOQDetail = new POBOQMap
                                {
                                    Id = GetPOBOQPK(),
                                    PODetailId = NewId1,
                                    BOQDetailId = POReqDetail.BOQId,
                                    TransactionQty = POReqDetail.TransactionQty,
                                    TransactionUoMId = POReqDetail.TransactionUoMId,
                                    BaseQty = Convert.ToDecimal(itemDetail.BaseUoMFactor * POReqDetail.TransactionQty),
                                    BaseUoMId = itemDetail.BaseUOMId,
                                    POUoMId = itemDetail.POUoMId,
                                    POBOQQty = Convert.ToDecimal(conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(POReqDetail.TransactionQty)).ToString("F2")),
                                };
                                AuditService.AddedLog(PoBOQDetail);
                                _POBOQMapRepository.Insert(PoBOQDetail);
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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
                {
                    _unitOfWork.Rollback();
                }
            }
        }
        #endregion
        #region PO Parameter
        public void DeletePOMaterial(string receiveDetailId, string OrderSpecific)
        {
            var flag = false;
            if (OrderSpecific == "No")
            {
                try
                {
                    var isNonCreditable = _receiveDetailRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[PurchaseOrder] AS A JOIN [TRN].[PurchaseOrderDetail] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + receiveDetailId + "'").First();
                    var data = Find(receiveDetailId);
                    if (data.IsNotNull())
                    {
                        _unitOfWork.BeginTransaction();
                        flag = true;

                        _inventoryReceiveService.ExecuteSqlCommand(@"INSERT INTO [TRN].[PurchaseOrderDetailBackUp] SELECT * FROM [TRN].[PurchaseOrderDetail] WHERE Id='" + receiveDetailId + "';");
                        _inventoryMaterialMasterService.UpdateFromReceive(data.InventoryMaterialId, receiveDetailId);
                        var taxCategoryList = _receiveTaxRepository.Query(t => t.InventoryReceiveDetailId == receiveDetailId).Select().ToList();
                        if (taxCategoryList.Count > 0)
                        {
                            foreach (var item in taxCategoryList)
                            {
                                item.ModelState = ModelState.Deleted;
                                _receiveTaxRepository.Delete(item);
                                _unitOfWork.SaveChanges();
                            }
                        }
                        var ratio = _inventoryReceiveService.GetChargesRatio(data.InventoryReceiveId, data.Id, 0, null, 0, isNonCreditable);
                        UpdateInventoryDetail(data, ratio, 1, isNonCreditable);
                        var res = _inventoryReceiveRepository.SqlQuery<int>(@"Select POId=Case when IR.POId IS NULL then 0 else 1 end from [TRN].PurchaseOrder PO Left JOIN [TRN].[InventoryReceive]  IR On IR.POId=PO.Id where PO.Id= '" + data.InventoryReceiveId + "'").FirstOrDefault();
                        if (res == 1)
                        {
                            throw new CustomException("Already Received In PO");

                        }
                        base.DeleteGraph(data);
                        _unitOfWork.SaveChanges();
                        flag = false;
                        _unitOfWork.Commit();
                    }
                    else
                        throw new CustomException("Data not found");
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
                    {
                        _unitOfWork.Rollback();
                    }
                }
            }
            else
            {
                try
                {
                    var isNonCreditable = _receiveDetailRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[PurchaseOrder] AS A JOIN [TRN].[PurchaseOrderDetail] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + receiveDetailId + "'").First();
                    var data = Find(receiveDetailId);
                    if (data.IsNotNull())
                    {
                        _unitOfWork.BeginTransaction();
                        flag = true;
                        _inventoryReceiveService.ExecuteSqlCommand(@"INSERT INTO [TRN].[PurchaseOrderDetailBackUp] SELECT * FROM [TRN].[PurchaseOrderDetail] WHERE Id='" + receiveDetailId + "';");
                        _inventoryMaterialMasterService.UpdateFromReceive(data.InventoryMaterialId, receiveDetailId);
                        var taxCategoryList = _receiveTaxRepository.Query(t => t.InventoryReceiveDetailId == receiveDetailId).Select().ToList();
                        if (taxCategoryList.Count > 0)
                        {
                            foreach (var item in taxCategoryList)
                            {
                                item.ModelState = ModelState.Deleted;
                                _receiveTaxRepository.Delete(item);
                                _unitOfWork.SaveChanges();
                            }
                        }
                        var ratio = _inventoryReceiveService.GetChargesRatio(data.InventoryReceiveId, data.Id, 0, null, 0, isNonCreditable);
                        UpdateInventoryDetail(data, ratio, 1, isNonCreditable);
                        var POBOQMAPList = _POBOQMapRepository.Query(t => t.PODetailId == receiveDetailId).Select().ToList();
                        if (POBOQMAPList.Count > 0)
                        {
                            foreach (var itemPOBOQMap in POBOQMAPList)
                            {
                                itemPOBOQMap.ModelState = ModelState.Deleted;
                                _POBOQMapRepository.Delete(itemPOBOQMap);
                                _unitOfWork.SaveChanges();
                            }
                        }
                        var res = _inventoryReceiveRepository.SqlQuery<int>(@"Select POId=Case when IR.POId IS NULL then 0 else 1 end from [TRN].PurchaseOrder PO Left JOIN [TRN].[InventoryReceive]  IR On IR.POId=PO.Id where PO.Id= '" + data.InventoryReceiveId + "'").FirstOrDefault();
                        if (res == 1)
                        {
                            throw new CustomException("Already Received In PO");

                        }
                        base.DeleteGraph(data);
                        _unitOfWork.SaveChanges();
                        flag = false;
                        _unitOfWork.Commit();
                    }
                    else
                        throw new CustomException("Data not found");
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
                    {
                        _unitOfWork.Rollback();
                    }
                }
            }

        }
        #endregion
    }
}