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
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Data;
using OTSBD;
using Aplos.MaterialManagement.MaterialQuery;

namespace Library.MaterialManagement.Inventory
{
    public class InventoryReceiveDetailService : Service<InventoryReceiveDetail>, IInventoryReceiveDetailService
    {
        #region Constructor

        private readonly IRepositoryAsync<InventoryReceiveDetail> _receiveDetailRepository;
        private readonly IRepositoryAsync<PurchaseOrderDetail> _poDetailRepository;
        private readonly IRepositoryAsync<PurchaseOrder> _poRepository;
        private readonly IRepositoryAsync<InventoryReceiveTax> _receiveTaxRepository;
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IInventoryMaterialService _inventoryMaterialMasterService;
        private readonly IInventoryReceiveService _inventoryReceiveService;
        private readonly IInventoryMaterialService _inventoryMaterialService;


        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepositoryAsync<GRNPORequisitionAllocation> _gRNPOAllocationRepository;
        private readonly IRepositoryAsync<GRNRejectionDetails> _gRNRejectionDetailsRepository;
        private readonly IRepositoryAsync<GRNAcceptanceMap> _GRNAcceptanceMapRepository;
        private readonly IRepositoryAsync<GRNBOQMAP> _GRNBOQMAPRepository;
        private readonly IRepositoryAsync<GRNBinAllocationMap> _GRNBinAllocationMapRepository;
        private readonly IRepositoryAsync<GRNPORequisitionMap> _GRNPORequisitionMapRepository;

        private readonly IRepositoryAsync<PurchaseDocAcceptanceTax> _PurchaseDocAcceptanceTaxRepository;

        private readonly IRepositoryAsync<PurchaseReturn> _PurchaseReturnRepository;
        private readonly IRepositoryAsync<PurchaseReturnDetail> _PurchaseReturnDetailRepository;
        private readonly IRepositoryAsync<PurchaseReturnTax> _PurchaseReturnTaxRepository;
        private readonly IRepositoryAsync<PurchaseReturnService> _PurchaseReturnServiceRepository;
        private readonly IRepositoryAsync<InventoryService> _inventoryServiceRepository;



        public InventoryReceiveDetailService(
            IRepositoryAsync<InventoryReceiveDetail> receiveDetailRepository
            , IRepositoryAsync<PurchaseOrder> poRepository
            , IRepositoryAsync<PurchaseOrderDetail> poDetailRepository
            , IRepositoryAsync<InventoryReceiveTax> receiveTaxRepository
            , IMaterialMasterService materialMasterService
            , IInventoryMaterialService inventoryMaterialMasterService
            , IInventoryReceiveService inventoryReceiveService
            , IInventoryMaterialService inventoryMaterialService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<GRNPORequisitionAllocation> gRNPOAllocationRepository
            , IRepositoryAsync<GRNRejectionDetails> gRNRejectionDetailsRepository
            , IRepositoryAsync<GRNAcceptanceMap> GRNAcceptanceMapRepository
            , IRepositoryAsync<GRNPORequisitionMap> GRNPORequisitionMapRepository
            , IRepositoryAsync<GRNBinAllocationMap> GRNBinAllocationMapRepository
            // , IRepositoryAsync<POGGRNMap> POGGRNMapRepository
            , IRepositoryAsync<PurchaseDocAcceptanceTax> PurchaseDocAcceptanceTaxRepository
            , IRepositoryAsync<PurchaseReturn> PurchaseReturnRepository
            , IRepositoryAsync<PurchaseReturnDetail> PurchaseReturnDetailRepository
            , IRepositoryAsync<PurchaseReturnTax> PurchaseReturnTaxRepository
            , IRepositoryAsync<InventoryService> inventoryServiceRepository
            , IRepositoryAsync<PurchaseReturnService> PurchaseReturnServiceRepository
            , IRepositoryAsync<GRNBOQMAP> GRNBOQMAPRepository

            ) : base(receiveDetailRepository, unitOfWork, pkGeneratorService)
        {
            _receiveDetailRepository = receiveDetailRepository;
            _poDetailRepository = poDetailRepository;
            _poRepository = poRepository;
            _receiveTaxRepository = receiveTaxRepository;
            _materialMasterService = materialMasterService;
            _inventoryMaterialMasterService = inventoryMaterialMasterService;
            _inventoryReceiveService = inventoryReceiveService;
            _inventoryMaterialService = inventoryMaterialService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _gRNPOAllocationRepository = gRNPOAllocationRepository;
            _gRNRejectionDetailsRepository = gRNRejectionDetailsRepository;
            _GRNAcceptanceMapRepository = GRNAcceptanceMapRepository;
            _GRNBOQMAPRepository = GRNBOQMAPRepository;
            _GRNPORequisitionMapRepository = GRNPORequisitionMapRepository;
            _GRNBinAllocationMapRepository = GRNBinAllocationMapRepository;
            // _POGGRNMapRepository = POGGRNMapRepository;
            _PurchaseDocAcceptanceTaxRepository = PurchaseDocAcceptanceTaxRepository;
            _PurchaseReturnRepository = PurchaseReturnRepository;
            _PurchaseReturnDetailRepository = PurchaseReturnDetailRepository;
            _PurchaseReturnTaxRepository = PurchaseReturnTaxRepository;
            _inventoryServiceRepository = inventoryServiceRepository;
            _PurchaseReturnServiceRepository = PurchaseReturnServiceRepository;
        }

        #endregion Constructor
        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(GRNRejectionDetails), out sID);
            return sID;
        }

        private string GetPurchaseReturnPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PurchaseReturn), out sID);
            return sID;
        }
        private string GetPKGRNAccept()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(GRNAcceptanceMap), out sID);
            return sID;
        }
        private string GetPurchaseReturnTaxPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PurchaseReturnTax), out sID);
            return sID;
        }
        public void InsertExtraTax(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            var flag = false;
            var rdBuilder = new System.Text.StringBuilder();
            var builderSql = "";
            try
            {
                decimal MaterialTaxDetailSum = 0;
                decimal ChargesTaxDetailSum = 0;
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
                        var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            item.InventoryReceiveId = entity.InventoryReceiveId;
                            if (item.InventoryServiceId == null)
                            {

                                if (item.Id == null)
                                {
                                    currentId++;
                                    item.Id = MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                                    item.InventoryReceiveId = entity.InventoryReceiveId;
                                    item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                                    item.InventoryServiceId = null;
                                    item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                    AuditService.AddedLog(item);
                                    _receiveTaxRepository.Insert(item);
                                    //_receiveTaxRepository.InsertOrUpdateGraph(item);
                                }
                                else
                                {
                                    item.InventoryReceiveId = entity.InventoryReceiveId;
                                    item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                                    item.InventoryServiceId = null;
                                    item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                    AuditService.AddedLog(item);
                                    _receiveTaxRepository.Update(item);
                                  
                                }

                                MaterialTaxDetailSum += item.TaxAmount;
                                builderSql = @"Update trn.InventoryReceiveDetail set TotalTaxAmount='" + MaterialTaxDetailSum + "'  where Id='" + item.InventoryReceiveDetailId + "'";
                                rdBuilder.Append(builderSql);
                                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

                            }
                            else
                            {
                                if (item.Id == null)
                                {
                                    currentId++;
                                    item.Id = MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                                    item.InventoryReceiveId = entity.InventoryReceiveId;
                                    item.InventoryReceiveDetailId = null;
                                    item.InventoryServiceId = item.InventoryServiceId;
                                    item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                    AuditService.AddedLog(item);
                                    _receiveTaxRepository.Insert(item);
                                    //_receiveTaxRepository.InsertOrUpdateGraph(item);
                                }
                                else
                                {
                                    item.InventoryReceiveId = entity.InventoryReceiveId;
                                    item.InventoryReceiveDetailId = null;
                                    item.InventoryServiceId = item.InventoryServiceId;
                                    item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                    AuditService.AddedLog(item);
                                    _receiveTaxRepository.Update(item);
                                   
                                }
                                ChargesTaxDetailSum += item.TaxAmount;
                                builderSql = @"Update trn.InventoryReceiveDetail set ChargesTaxTranAmount='" + ChargesTaxDetailSum + "'  where Id='" + item.Id + "'";
                                rdBuilder.Append(builderSql);
                                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

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

        public void InsertExtraTaxUpdate(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            var flag = false;
            var rdBuilder = new System.Text.StringBuilder();
            var builderSql = "";
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
                        var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId='{entity.InventoryReceiveDetailId}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            item.InventoryReceiveId = entity.InventoryReceiveId;
                            if (item.Id == null)
                            {
                                currentId++;
                                item.Id = MakePK(entity.InventoryReceiveDetailId, currentId, 2);
                                item.InventoryReceiveId = entity.InventoryReceiveId;
                                item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                                item.InventoryServiceId = null;
                                item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Insert(item);
                                //_receiveTaxRepository.InsertOrUpdateGraph(item);
                            }
                            else
                            {
                                item.InventoryReceiveId = entity.InventoryReceiveId;
                                item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                                item.InventoryServiceId = null;
                                item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Update(item);
                                var invMaterialDetail = _receiveDetailRepository.SqlQuery<decimal>(@"SELECT TotalTaxAmount FROM [TRN].[InventoryReceiveDetail] WHERE Id='" + item.InventoryReceiveDetailId + "'").FirstOrDefault();
                                var res = Convert.ToDecimal(invMaterialDetail) + Convert.ToDecimal(item.TaxAmount);
                                builderSql = @"Update trn.InventoryReceiveDetail set TotalTaxAmount='" + res + "'  where Id='" + item.InventoryReceiveDetailId + "'";
                                //builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET BaseIssueQty='" + Convert.ToDecimal(issueQty) + "',IssueQty='" + Convert.ToDecimal(issueQty) + "'  WHERE Id='" + item.Id + "'";
                                rdBuilder.Append(builderSql);
                                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
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
        public void UpdateGRNBOQTax(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            var flag = false;
            var rdBuilder = new System.Text.StringBuilder();
            var builderSql = "";
            try
            {
                ResetCurrencyRate(entity);
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entity.IsNotNull())
                {
                    // insert in PO Item tax
                    if (taxCategoryList.IsNotNull())
                    {
                        foreach (var item in taxCategoryList)
                        {
                                item.InventoryReceiveId = entity.InventoryReceiveId;
                                item.InventoryReceiveDetailId = entity.InventoryReceiveDetailId;
                                item.InventoryServiceId = null;
                                item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Update(item);
                                var res =  Convert.ToDecimal(taxCategoryList.Sum(r=>r.TaxAmount));
                                builderSql = @"Update trn.InventoryReceiveDetail set TotalTaxAmount='" + res + "'  where Id='" + item.InventoryReceiveDetailId + "'";
                                rdBuilder.Append(builderSql);
                                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
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
        private string GetPKGRNPORequisitionAllocation()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(GRNPORequisitionAllocation), out sID);
            return sID;
        }
        //PO GRN
        public void InsertOrUpdateGraphNew(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id
            , string MaterialStorageId, string GRNType, IEnumerable<GRNPORequisitionMap> requisitionDetailList, IEnumerable<GRNBinAllocationMap> grnBinAllocationMap)
        {
            var flag = false;
            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.GRNType = GRNType;
                if (entity.Id == null)
                {
                    _inventoryReceiveService.Insert(entity);
                    if (entity.PurchaseDocumentAcceptanceId != null)
                    {
                        var GRNAcceptance = new GRNAcceptanceMap
                        {
                            Id = base.GetAutoNumber(nameof(GRNAcceptanceMap), PKGeneratorEnum.Yearly, null, DateTime.Now),
                            GRNId = entity.Id,
                            PurchaseDocumentAcceptanceId = entity.PurchaseDocumentAcceptanceId,
                        };
                        AuditService.AddedLog(GRNAcceptance);
                        _GRNAcceptanceMapRepository.Insert(GRNAcceptance);
                    }
                }
                else
                {
                    _inventoryReceiveService.Update(entity);
                    //TODO:
                }
                var grnDetailCheck = _receiveDetailRepository.Query(r => r.InventoryReceiveId == entity.Id).Select(r=>r.Id).FirstOrDefault();
                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var currentId1 = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{entity.Id}'").First();
                var Temppodetailid = "";
                var grndId = "";
                if (grnDetailCheck == null)
                {
                    foreach (var itemDetail in entityMat)
                    {
                        itemDetail.CompanyGroupId = identity.CompanyGroupId;
                        itemDetail.CompanyId = identity.CompanyId;
                        itemDetail.PlantId = identity.PlantId;
                        Temppodetailid = itemDetail.InventoryReceiveDetailId;
                        itemDetail.IsNonCreditable = entity.IsNonCreditable;

                        if (CheckItemExist(itemDetail))
                            throw new CustomException(itemDetail.MaterialMasterName + " already received");

                        ResetCurrencyRate(itemDetail);
                        itemDetail.ToCurrencyRate = entity.ToCurrencyRate;
                        if (itemDetail.IsNotNull())
                        {
                            if (itemDetail.PurchaseDocumentAcceptanceId != null)
                            {
                                itemDetail.ToCurrencyRate = entity.ToCurrencyRate;
                            }
                            var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                            if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                            ///TODO : Get total qyt and amount by country and issue qty
                            itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                            itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());

                            itemDetail.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.PurchaseReturnQty).Sum());
                            itemDetail.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueReturnQty).Sum());
                            itemDetail.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                            itemDetail.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventorySalesQty).Sum());
                            itemDetail.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryScrapQty).Sum());
                            itemDetail.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryTransferQty).Sum());

                            var ShortageQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ShortageQty).Sum();
                            var RejectionQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.RejectionQty).Sum();
                            var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ApprovedQty).Sum();


                            var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                            var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                            var altUomIds = new string[] { itemDetail.TransactionUoMId };
                            var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                            if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                                 && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                            {

                                if (baseUoMFactorList.Count() == 0)
                                {
                                    itemDetail.BaseUoMFactor = 1;
                                }
                                else
                                {
                                    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                }
                                itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;

                                itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                                itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                            }
                            else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                                && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                            {

                                if (baseUoMFactorList.Count() == 0)
                                {
                                    itemDetail.BaseUoMFactor = 1;
                                }
                                else
                                {
                                    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                }
                                itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor;//itemDetail.TransactionQty;
                                itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                if (itemDetail.TotalTaxAmount == null)
                                    itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                  Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                         Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                            }
                            else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                            {

                                if (baseUoMFactorList.Count() == 0)
                                {
                                    itemDetail.BaseUoMFactor = 1;
                                }
                                else
                                {
                                    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);

                                }
                                itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                itemDetail.MaterialTranAmount = itemDetail.TrnAmount;
                                itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                            }

                            else
                            {
                                itemDetail.BaseUoMFactor = 1;
                                itemDetail.BaseQty = itemDetail.TransactionQty * itemDetail.BaseUoMFactor;//itemDetail.TransactionQty;
                                itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                            }
                            if (itemDetail.PurchaseDocumentAcceptanceId == null && itemDetail.PurchaseDocumentAcceptanceDetailId == null)
                            {
                                var poDetail = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                                if (poDetail == null)
                                    throw new CustomException("PO Details Or Inventory Details not found!");

                                poDetail.GRNRcvQty += itemDetail.TransactionQty;
                                if (itemDetail.Tolerance == 0)
                                {
                                    if (poDetail.TransactionQty < (poDetail.GRNRcvQty- itemDetail.PurchaseReturnQty))
                                        throw new CustomException("Received Qty can not cross balance Qty.");
                                }

                                if (itemDetail.POClosStatus == true)
                                {
                                    poDetail.QtyStatus = true;
                                }
                                else
                                {
                                    poDetail.QtyStatus = false;
                                }
                                AuditService.UpdatedLog(poDetail);
                                _poDetailRepository.Update(poDetail);

                            }

                            // Insert in receive detail
                            if (string.IsNullOrEmpty(itemDetail.Id))
                            {
                                var NewId = entity.Id + "-";
                                currentId1++;
                                grndId = NewId + currentId1;
                                var receiveDetail = new InventoryReceiveDetail
                                {
                                    Id = NewId + currentId1, //MakePK(NewId + currentId, 0,0),
                                    MaterialStorageId = itemDetail.MaterialStorageId,//MaterialStorageId
                                    InventoryReceiveId = entity.Id,//itemDetail.InventoryReceiveId,
                                                                   //InventoryMaterialId = entity.InventoryMaterialId,
                                    TransactionQty = itemDetail.TransactionQty,//itemDetail.TransactionQty,
                                    TransactionUoMId = itemDetail.TransactionUoMId,
                                    BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                    BaseUOMId = itemDetail.BaseUOMId,
                                    BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                    MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                    MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                                    TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                                    TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                                    POID = itemDetail.POID,
                                    PODetailsID = itemDetail.PODetailsID,
                                    TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                                    ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                                    ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTaxTranAmount), 2),
                                    IssueQty = 0,
                                    BaseIssueQty = 0,
                                    TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                    BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 2),
                                    PurchaseDocumentAcceptanceId = itemDetail.PurchaseDocumentAcceptanceId,
                                    PurchaseDocumentAcceptanceDetailId = itemDetail.PurchaseDocumentAcceptanceDetailId,
                                    PurchaseReturnQty = 0,
                                    IssueReturnQty = 0,
                                    InventorySalesQty = 0,
                                    InventoryScrapQty = 0,
                                    MaterialMasterOpeningBalanceDetailId = null,
                                    LotNumber = itemDetail.LotNumber,
                                    LotNo = itemDetail.LotNumber,
                                    Diameter = itemDetail.Diameter,
                                    Type = itemDetail.Type,
                                    ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                    RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                    ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),
                                    ShortageRatePercent = 100,
                                    ShortageValue = Math.Round(Convert.ToDecimal(itemDetail.ShortageValue), 2),
                                    RejectRatePercent = 50,
                                    GRNQty = itemDetail.TransactionQty,
                                    GRNTotalAmount = Math.Round(itemDetail.TransactionQty * Convert.ToDecimal(itemDetail.TransactionRate), 2),
                                    IsAsset = itemDetail.IsAsset,
                                    GrossAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                    DiscountAmount = Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                    QualityStatus = itemDetail.QualityStatus


                                };
                                try
                                {

                                    itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                                    receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                    receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                    receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);

                                    AuditService.AddedLog(receiveDetail);

                                    itemDetail.TotalQty = ((Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty + itemDetail.IssueReturnQty)) - (Convert.ToDecimal(itemDetail.IssueQty) + Convert.ToDecimal(itemDetail.PurchaseReturnQty) + Convert.ToDecimal(itemDetail.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetail.InventorySalesQty) + Convert.ToDecimal(itemDetail.InventoryScrapQty) + Convert.ToDecimal(itemDetail.InventoryTransferQty)));
                                    itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetail.TotalQty);
                                    itemDetail.ShortageQty = Convert.ToDecimal(receiveDetail.ShortageQty + ShortageQty);
                                    itemDetail.RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty + RejectionQty);
                                    itemDetail.ApprovedQty = Convert.ToDecimal(receiveDetail.ApprovedQty + ApprovedQty);

                                    _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);

                                    receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                                    InsertGraph(receiveDetail);
                                    updateArticleMinMaxValue(itemDetail.MinimumValue, itemDetail.MaximumValue, Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), itemDetail.ArticleId);
                                    if (grnBinAllocationMap != null)
                                    {
                                        int b = 0;
                                        foreach (var grnbinAllo in grnBinAllocationMap.Where(r => r.InventoryReceiveDetailId == receiveDetail.PODetailsID))
                                        {
                                            b++;
                                            var grnbinAlloObj = new GRNBinAllocationMap
                                            {
                                                Id = MakePK(receiveDetail.Id, b, 2),
                                                InventoryReceiveDetailId = receiveDetail.Id,
                                                StorageBinMasterId = grnbinAllo.StorageBinMasterId,
                                                Qty = grnbinAllo.Qty
                                            };
                                            AuditService.AddedLog(grnbinAlloObj);
                                            _GRNBinAllocationMapRepository.Insert(grnbinAlloObj);
                                        }
                                    }


                                    int rejectDetailId = 1;
                                    var RejectionDetails = new GRNRejectionDetails
                                    {
                                        Id = grndId.ToString() + rejectDetailId,
                                        GRNDeailsId = grndId,
                                        RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty),
                                        RejectionUoMId = itemDetail.TransactionUoMId,
                                        BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                        BaseUOMId = itemDetail.BaseUOMId,
                                        RejectionRate = Convert.ToDecimal(receiveDetail.RejectRatePercent),
                                        RejeactionValue = Convert.ToDecimal(receiveDetail.RejectValue),
                                    };
                                    AuditService.AddedLog(RejectionDetails);
                                    _gRNRejectionDetailsRepository.Insert(RejectionDetails);
                                    if (requisitionDetailList != null)
                                    {
                                        foreach (var item in requisitionDetailList.Where(r => r.PODetailId == receiveDetail.PODetailsID))
                                        {
                                            item.Id = base.GetAutoNumber(nameof(GRNPORequisitionMap), PKGeneratorEnum.Yearly, null, DateTime.Now);
                                            item.InventoryReceiveDetailId = receiveDetail.Id;
                                            AuditService.AddedLog(item);
                                            _GRNPORequisitionMapRepository.Insert(item);
                                        }
                                    }
                                }
                                catch (DivideByZeroException )
                                {

                                }
                               
                            }
                        }

                        // insert in receive tax
                        if (itemDetail.PurchaseDocumentAcceptanceId == null && itemDetail.PurchaseDocumentAcceptanceDetailId == null)
                        {
                            if (taxCategoryList.IsNotNull())
                            {
                                var currentId = 0;
                                foreach (var item in taxCategoryList.Where(r => r.InventoryReceiveDetailId == Temppodetailid))
                                {
                                    currentId++;
                                    item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                                    item.InventoryReceiveId = entity.Id;//itemDetail.InventoryReceiveId;
                                    item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                                    item.InventoryServiceId = null;
                                    item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                    AuditService.AddedLog(item);
                                    _receiveTaxRepository.Insert(item);
                                }
                            }
                        }
                        else
                        {
                            var currentId = 0;
                            var AcceptanceMaterialTaxList = _PurchaseDocAcceptanceTaxRepository.Query(r => r.PurchaseDocAcceptanceId == itemDetail.PurchaseDocumentAcceptanceId && r.PurchaseDocAcceptanceDetailId != null && r.PurchaseDocAcceptanceDetailId == itemDetail.PurchaseDocumentAcceptanceDetailId).Select().ToList();//($"SELECT * FROM TRN.PurchaseDocAcceptanceTax WHERE PurchaseDocAcceptanceDetailId IS NULL AND PurchaseDocAcceptanceId='{AcceptanceId}'").ToList();

                            foreach (var item1 in AcceptanceMaterialTaxList)
                            {
                                currentId++;
                                var inventoryReceiveTax = new InventoryReceiveTax
                                {
                                    Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2),
                                    InventoryReceiveId = entity.Id,//itemDetail.InventoryReceiveId;
                                    InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId,
                                    InventoryServiceId = null,
                                    TaxCategoryId = item1.TaxCategoryId,
                                    Percentage = item1.Percentage,
                                    TaxAmount = Math.Round(item1.TaxAmount, 2)
                                };
                                AuditService.AddedLog(inventoryReceiveTax);
                                _receiveTaxRepository.Insert(inventoryReceiveTax);
                            }
                        }

                        if (Convert.ToDecimal(itemDetail.POQty) > (Convert.ToDecimal(itemDetail.GRNRcvQty + itemDetail.TransactionQty)))
                        {
                            entity.msgForAllocationNeed = "You have to allocate GRN Qty manually for Sales Order ! Please go to edit mode for allocation";
                        }
                        else
                        {
                            var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"select 
											c.PODetailId ,C.BOQDetailId ,C.Id POBOQMAPID ,C.TransactionQty TransactionQtyForPO
											,C.TransactionUoMId,uom.UserName TransactionUoM  ,C.BaseQty ,C.BaseUoMId ,C.POBOQQty
											,C.POUoMId ,d.BOMQty ReqQty ,0 allowQty ,b.TransactionQty POTransactionQty
											,0 TransactionQty ,0 RejectionQty ,null Active,d.SalesOrderId ,b.Id
											,isnull(AllocatedSOQty.AllocatedSOQty,0) AllocatedSOQty
											From trn.PurchaseOrderDetail b --on b.Id=a.PODetailsId
											left join trn.POBOQMAP c on c.PODetailId=b.Id
											left join boq d On d.Id=c.BOQDetailId
											left join scs.UnitOfMeasurement uom ON uom.Id=c.TransactionUoMId 
											left JOIN(select POBOQMapId ,Sum(TransactionQty) AllocatedSOQty from trn.GRNPORequisitionAllocation  GROUP BY POBOQMapId)AllocatedSOQty ON AllocatedSOQty.POBOQMapId=c.Id
											where b.Id='" + itemDetail.PODetailsID + @"'").ToList();
                            if (receiveDetailList.IsNotNull())
                            {
                                bool isQtyAlocated = true;
                                decimal temp = 0;
                                int count = 0;
                                foreach (var issue in receiveDetailList)
                                {
                                    count++;
                                    if (count == 1)
                                    {
                                        if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) > itemDetail.TransactionQty)
                                        {
                                            itemDetail.TransactionQty = itemDetail.TransactionQty;
                                            isQtyAlocated = false;
                                        }
                                        else if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) < itemDetail.TransactionQty)
                                        {
                                            temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = (issue.TransactionQtyForPO - issue.AllocatedSOQty);
                                            isQtyAlocated = true;
                                        }
                                        else
                                        {
                                            itemDetail.TransactionQty = itemDetail.TransactionQty;
                                            isQtyAlocated = true;

                                        }
                                    }
                                    if (count > 1)
                                    {
                                        if (isQtyAlocated == true)
                                        {
                                            if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) > temp)
                                            {
                                                itemDetail.TransactionQty = itemDetail.TransactionQty;
                                                isQtyAlocated = false;
                                            }
                                            if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) < temp)
                                            {
                                                temp = (temp - (issue.TransactionQtyForPO - issue.AllocatedSOQty));
                                                itemDetail.TransactionQty = (issue.TransactionQtyForPO - issue.AllocatedSOQty);
                                                isQtyAlocated = true;
                                            }
                                            else
                                            {
                                                itemDetail.TransactionQty = temp;
                                                isQtyAlocated = true;
                                            }
                                        }
                                        else
                                        {
                                            itemDetail.TransactionQty = 0;
                                        }
                                    }
                                    var baseQqtynew = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty));
                                    var POBOQQtyNew = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty));
                                    var gRNPOAllocation = new GRNPORequisitionAllocation
                                    {
                                        Id = base.GetAutoNumber(nameof(GRNPORequisitionAllocation), PKGeneratorEnum.Yearly, null, DateTime.Now),
                                        InventoryReceiveDetailId = grndId,
                                        POBOQMapId = issue.POBOQMapId,
                                        POReqDetailsID = issue.POReqDetailsID,
                                        BOQDetailId = issue.BOQDetailId,
                                        TransactionQty = Convert.ToDecimal(itemDetail.TransactionQty),
                                        TransactionUoMId = itemDetail.TransactionUoMId,
                                        BaseQty = baseQqtynew,
                                        BaseUoMId = issue.BaseUOMId,
                                        POBOQQty = POBOQQtyNew,
                                        POUoMId = itemDetail.POUoMId,
                                        RejectQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                        RejectBaseQty = Convert.ToDecimal(itemDetail.RejectBaseQty),
                                        SalesOrderId = issue.SalesOrderId
                                    };
                                    AuditService.AddedLog(gRNPOAllocation);
                                    _gRNPOAllocationRepository.Insert(gRNPOAllocation);
                                }
                            }
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
        public void InsertOrUpdateGraphNewGRNBOQ(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, IEnumerable<InventoryMaterialViewModel> BOQAllocationSave)
        {
            var flag = false;
            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {

                _unitOfWork.BeginTransaction();

                flag = true;
                entity.GRNType = GRNType;
                if (entity.Id == null)
                {
                    _inventoryReceiveService.Insert(entity);
                    if (entity.PurchaseDocumentAcceptanceId != null)
                    {
                        var GRNAcceptance = new GRNAcceptanceMap
                        {
                            Id = base.GetAutoNumber(nameof(GRNAcceptanceMap), PKGeneratorEnum.Yearly, null, DateTime.Now),
                            GRNId = entity.Id,
                            PurchaseDocumentAcceptanceId = entity.PurchaseDocumentAcceptanceId,
                            //Qty = receiveDetail.TransactionQty
                        };
                        AuditService.AddedLog(GRNAcceptance);
                        _GRNAcceptanceMapRepository.Insert(GRNAcceptance);
                    }
                }
                else
                {
                    AuditService.UpdatedLog(entity);
                    _inventoryReceiveService.Update(entity);
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var grnDetailCheck = _receiveDetailRepository.Query(r => r.InventoryReceiveId == entity.Id).Select(r => r.Id).FirstOrDefault();
                var currentId1 = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{entity.Id}'").First();
                var Temppodetailid = "";
                var grndId = "";
                if (grnDetailCheck == null) {
                    foreach (var itemDetail in entityMat)
                    {
                        itemDetail.CompanyGroupId = identity.CompanyGroupId;
                        itemDetail.CompanyId = identity.CompanyId;
                        itemDetail.PlantId = identity.PlantId;
                        Temppodetailid = itemDetail.InventoryReceiveDetailId;
                        itemDetail.IsNonCreditable = entity.IsNonCreditable;

                        if (CheckItemExist(itemDetail))
                            throw new CustomException(itemDetail.MaterialMasterName + " already received");

                        ResetCurrencyRate(itemDetail);
                        itemDetail.ToCurrencyRate = entity.ToCurrencyRate;
                        if (itemDetail.IsNotNull())
                        {
                            if (itemDetail.PurchaseDocumentAcceptanceId != null)
                            {
                                itemDetail.ToCurrencyRate = entity.ToCurrencyRate;

                            }


                            var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                            if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                            ///TODO : Get total qyt and amount by country and issue qty
                            itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                            itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());

                            itemDetail.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.PurchaseReturnQty).Sum());
                            itemDetail.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueReturnQty).Sum());
                            itemDetail.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                            itemDetail.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventorySalesQty).Sum());
                            itemDetail.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryScrapQty).Sum());
                            itemDetail.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryTransferQty).Sum());

                            var ShortageQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ShortageQty).Sum();
                            var RejectionQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.RejectionQty).Sum();
                            var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ApprovedQty).Sum();


                            var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                            var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                            var altUomIds = new string[] { itemDetail.TransactionUoMId };
                            var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                            if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                                 && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                            {

                                if (baseUoMFactorList.Count() == 0)
                                {
                                    itemDetail.BaseUoMFactor = 1;
                                }
                                else
                                {
                                    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                }
                                itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;

                                itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                                itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                            }
                            else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                                && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                            {

                                if (baseUoMFactorList.Count() == 0)
                                {
                                    itemDetail.BaseUoMFactor = 1;
                                }
                                else
                                {
                                    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                }
                                itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor;//itemDetail.TransactionQty;
                                itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                if (itemDetail.TotalTaxAmount == null)
                                    itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                  Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                //itemDetail.ChargesTranAmount = itemDetail.MaterialTranAmount * ratio;
                                itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                         Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            }
                            else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                            {

                                if (baseUoMFactorList.Count() == 0)
                                {
                                    itemDetail.BaseUoMFactor = 1;
                                }
                                else
                                {
                                    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);

                                }
                                itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                itemDetail.MaterialTranAmount = itemDetail.TrnAmount;
                                itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                  Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                            }

                            else
                            {

                                itemDetail.BaseUoMFactor = 1;
                                itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor;//itemDetail.TransactionQty;
                                itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                  Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                            }
                            if (itemDetail.PurchaseDocumentAcceptanceId == null && itemDetail.PurchaseDocumentAcceptanceDetailId == null)
                            {
                                var poDetail = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                                if (poDetail == null)
                                    throw new CustomException("PO Details Or Inventory Details not found!");

                                poDetail.GRNRcvQty += itemDetail.TransactionQty;
                                if (itemDetail.Tolerance == 0)
                                {
                                    if (poDetail.TransactionQty < poDetail.GRNRcvQty)
                                        throw new CustomException("Received Qty can not cross balance Qty.");
                                }

                                if (itemDetail.POClosStatus == true)
                                {
                                    poDetail.QtyStatus = true;
                                }
                                else
                                {
                                    poDetail.QtyStatus = false;
                                }
                                AuditService.UpdatedLog(poDetail);
                                _poDetailRepository.Update(poDetail);

                            }

                            // Insert in receive detail
                            if (string.IsNullOrEmpty(itemDetail.Id))
                            {
                                var NewId = entity.Id + "-";
                                //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[MaterialRequsitionDetails] WHERE //MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();
                                //currentId++;

                                currentId1++;
                                grndId = NewId + currentId1;
                                var receiveDetail = new InventoryReceiveDetail
                                {
                                    Id = NewId + currentId1, //MakePK(NewId + currentId, 0,0),
                                    MaterialStorageId = entity.MaterialStorageId,//MaterialStorageId
                                    InventoryReceiveId = entity.Id,//itemDetail.InventoryReceiveId,
                                                                   //InventoryMaterialId = entity.InventoryMaterialId,
                                    TransactionQty = itemDetail.NetQty,//itemDetail.TransactionQty,
                                    TransactionUoMId = itemDetail.TransactionUoMId,
                                    BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                    BaseUOMId = itemDetail.BaseUOMId,
                                    BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                    MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                    MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                                    TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                                    TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                                    POID = itemDetail.POID,
                                    PODetailsID = itemDetail.PODetailsID,
                                    TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                                    ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                                    ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTaxTranAmount), 2),
                                    IssueQty = 0,
                                    BaseIssueQty = 0,
                                    TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                    BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 2),
                                    PurchaseDocumentAcceptanceId = itemDetail.PurchaseDocumentAcceptanceId,
                                    PurchaseDocumentAcceptanceDetailId = itemDetail.PurchaseDocumentAcceptanceDetailId,
                                    PurchaseReturnQty = 0,
                                    IssueReturnQty = 0,
                                    InventorySalesQty = 0,
                                    InventoryScrapQty = 0,
                                    MaterialMasterOpeningBalanceDetailId = null,
                                    LotNumber = itemDetail.LotNumber,
                                    LotNo = itemDetail.LotNumber,
                                    Diameter = itemDetail.Diameter,
                                    Type = itemDetail.Type,
                                    ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                    RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                    ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),
                                    ShortageRatePercent = 110,
                                    ShortageValue = Math.Round(Convert.ToDecimal(itemDetail.ShortageValue), 2),
                                    RejectRatePercent = 50,
                                    GRNQty = itemDetail.TransactionQty,
                                    GRNTotalAmount = Math.Round(itemDetail.TransactionQty * Convert.ToDecimal(itemDetail.TransactionRate), 2),
                                    IsAsset = itemDetail.IsAsset,
                                    GrossAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                    DiscountAmount = Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                    QualityStatus = itemDetail.QualityStatus


                                };
                                try
                                {

                                    itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                                    receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                    receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                    receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);

                                    AuditService.AddedLog(receiveDetail);

                                    itemDetail.TotalQty = ((Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty + itemDetail.IssueReturnQty)) - (Convert.ToDecimal(itemDetail.IssueQty) + Convert.ToDecimal(itemDetail.PurchaseReturnQty) + Convert.ToDecimal(itemDetail.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetail.InventorySalesQty) + Convert.ToDecimal(itemDetail.InventoryScrapQty) + Convert.ToDecimal(itemDetail.InventoryTransferQty)));
                                    itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetail.TotalQty);
                                    itemDetail.ShortageQty = Convert.ToDecimal(receiveDetail.ShortageQty + ShortageQty);
                                    itemDetail.RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty + RejectionQty);
                                    itemDetail.ApprovedQty = Convert.ToDecimal(receiveDetail.ApprovedQty + ApprovedQty);

                                    _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                                    receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                                    InsertGraph(receiveDetail);
                                    updateArticleMinMaxValue(itemDetail.MinimumValue, itemDetail.MaximumValue, Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), itemDetail.ArticleId);
                                    foreach (var boqallocat in BOQAllocationSave.Where(r => r.PODetailsID == receiveDetail.PODetailsID))
                                    {
                                        if (string.IsNullOrEmpty(boqallocat.Id))
                                        {

                                            var grnpoboqreqAll = new GRNPORequisitionAllocation
                                            {

                                                Id = base.GetAutoNumber(nameof(GRNPORequisitionAllocation), PKGeneratorEnum.Yearly, null, DateTime.Now),
                                                InventoryReceiveDetailId = receiveDetail.Id,
                                                POBOQMapId = boqallocat.POBOQMapId,
                                                POReqDetailsID = boqallocat.POReqDetailsID,
                                                BOQDetailId = boqallocat.BOQDetailId,
                                                // TransactionQty = Convert.ToDecimal(boqallocat.TransactionQty),//Receivable detail TransactionQty and Boq Detail TransactionQty same object name. 
                                                TransactionQty = Convert.ToDecimal(boqallocat.Qty),
                                                TransactionUoMId = boqallocat.TransactionUoMId,
                                                BaseQty = (decimal)conversion.Convert(boqallocat.MaterialMasterId, boqallocat.TransactionUoMId, boqallocat.BaseUOMId.ToString(), Convert.ToDouble(boqallocat.Qty)),
                                                BaseUoMId = boqallocat.BaseUOMId,
                                                POBOQQty = (decimal)conversion.Convert(boqallocat.MaterialMasterId, boqallocat.TransactionUoMId, boqallocat.POUoMId.ToString(), Convert.ToDouble(boqallocat.Qty)),
                                                POUoMId = boqallocat.POUoMId,
                                                RejectQty = Convert.ToDecimal(boqallocat.RejectionQty),
                                                RejectBaseQty = Convert.ToDecimal(boqallocat.RejectBaseQty),
                                                SalesOrderId = boqallocat.SalesOrderId

                                            };
                                            AuditService.AddedLog(grnpoboqreqAll);
                                            _gRNPOAllocationRepository.Insert(grnpoboqreqAll);

                                        }

                                    }


                                    int rejectDetailId = 1;
                                    var RejectionDetails = new GRNRejectionDetails
                                    {
                                        Id = grndId.ToString() + rejectDetailId,
                                        GRNDeailsId = grndId,
                                        RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty),
                                        RejectionUoMId = itemDetail.TransactionUoMId,
                                        BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                        BaseUOMId = itemDetail.BaseUOMId,
                                        RejectionRate = Convert.ToDecimal(receiveDetail.RejectRatePercent),
                                        RejeactionValue = Convert.ToDecimal(receiveDetail.RejectValue),
                                    };
                                    AuditService.AddedLog(RejectionDetails);
                                    _gRNRejectionDetailsRepository.Insert(RejectionDetails);
                                }
                                catch (DivideByZeroException )
                                {

                                }
                                finally
                                {

                                }
                            }
                        }

                        // insert in receive tax
                        if (itemDetail.PurchaseDocumentAcceptanceId == null && itemDetail.PurchaseDocumentAcceptanceDetailId == null)
                        {
                            if (taxCategoryList.IsNotNull())
                            {
                                var currentId = 0;
                                //var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                                foreach (var item in taxCategoryList.Where(r => r.InventoryReceiveDetailId == Temppodetailid))
                                {
                                    currentId++;
                                    item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                                    item.InventoryReceiveId = entity.Id;//itemDetail.InventoryReceiveId;
                                    item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                                    item.InventoryServiceId = null;
                                    item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                    AuditService.AddedLog(item);
                                    _receiveTaxRepository.Insert(item);
                                }
                            }

                        }
                        else
                        {
                            var currentId = 0;
                            var AcceptanceMaterialTaxList = _PurchaseDocAcceptanceTaxRepository.Query(r => r.PurchaseDocAcceptanceId == itemDetail.PurchaseDocumentAcceptanceId && r.PurchaseDocAcceptanceDetailId != null && r.PurchaseDocAcceptanceDetailId == itemDetail.PurchaseDocumentAcceptanceDetailId).Select().ToList();//($"SELECT * FROM TRN.PurchaseDocAcceptanceTax WHERE PurchaseDocAcceptanceDetailId IS NULL AND PurchaseDocAcceptanceId='{AcceptanceId}'").ToList();

                            foreach (var item1 in AcceptanceMaterialTaxList)
                            {
                                currentId++;
                                var inventoryReceiveTax = new InventoryReceiveTax
                                {
                                    Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2),
                                    InventoryReceiveId = entity.Id,//itemDetail.InventoryReceiveId;
                                    InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId,
                                    InventoryServiceId = null,
                                    TaxCategoryId = item1.TaxCategoryId,
                                    Percentage = item1.Percentage,
                                    TaxAmount = Math.Round(item1.TaxAmount, 2)
                                };
                                AuditService.AddedLog(inventoryReceiveTax);
                                _receiveTaxRepository.Insert(inventoryReceiveTax);
                            }
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
        public void BOQInsertOrUpdateGraphNew(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, List<InventoryMaterialViewModel> List)
        {
            var flag = false;
            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {

                _unitOfWork.BeginTransaction();

                flag = true;
                entity.GRNType = GRNType;
                if (string.IsNullOrEmpty(entity.Id))
                {
                    _inventoryReceiveService.Insert(entity);
                    if (entity.PurchaseDocumentAcceptanceId != null)
                    {
                        var GRNAcceptance = new GRNAcceptanceMap
                        {
                            Id = base.GetAutoNumber(nameof(GRNAcceptanceMap), PKGeneratorEnum.Yearly, null, DateTime.Now),
                            GRNId = entity.Id,
                            PurchaseDocumentAcceptanceId = entity.PurchaseDocumentAcceptanceId,
                            //Qty = receiveDetail.TransactionQty
                        };
                        AuditService.AddedLog(GRNAcceptance);
                        _GRNAcceptanceMapRepository.Insert(GRNAcceptance);

                    }
                }
                else
                {
                    _inventoryReceiveService.Update(entity);

                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var currentId1 = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{entity.Id}'").First();
                var Temppodetailid = "";
                var grndId = "";
                foreach (var itemDetail in List)
                {
                    itemDetail.CompanyGroupId = identity.CompanyGroupId;
                    itemDetail.CompanyId = identity.CompanyId;
                    itemDetail.PlantId = identity.PlantId;
                    Temppodetailid = itemDetail.InventoryReceiveDetailId;
                    itemDetail.IsNonCreditable = entity.IsNonCreditable;

                    if (CheckItemExist(itemDetail))
                        throw new CustomException(itemDetail.MaterialMasterName + " already received");

                    ResetCurrencyRate(itemDetail);
                    itemDetail.ToCurrencyRate = entity.ToCurrencyRate;
                    if (itemDetail.IsNotNull())
                    {
                        if (itemDetail.PurchaseDocumentAcceptanceId != null)
                        {
                            itemDetail.ToCurrencyRate = entity.ToCurrencyRate;

                        }


                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());

                        itemDetail.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.PurchaseReturnQty).Sum());
                        itemDetail.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueReturnQty).Sum());
                        itemDetail.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                        itemDetail.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventorySalesQty).Sum());
                        itemDetail.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryScrapQty).Sum());
                        itemDetail.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryTransferQty).Sum());

                        var ShortageQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ShortageQty).Sum();
                        var RejectionQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.RejectionQty).Sum();
                        var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ApprovedQty).Sum();


                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {

                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            }
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;

                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                            && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {

                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            }
                            itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor;//itemDetail.TransactionQty;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            if (itemDetail.TotalTaxAmount == null)
                                itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                            //itemDetail.ChargesTranAmount = itemDetail.MaterialTranAmount * ratio;
                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {

                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);

                            }
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.MaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                        }

                        else
                        {

                            itemDetail.BaseUoMFactor = 1;
                            itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor;//itemDetail.TransactionQty;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                        }

                        // Insert in receive detail
                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {
                            var NewId = entity.Id + "-";
                            //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[MaterialRequsitionDetails] WHERE //MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();
                            //currentId++;

                            currentId1++;
                            grndId = NewId + currentId1;
                            var receiveDetail = new InventoryReceiveDetail
                            {
                                Id = NewId + currentId1, //MakePK(NewId + currentId, 0,0),
                                MaterialStorageId = itemDetail.MaterialStorageId,//MaterialStorageId
                                InventoryReceiveId = entity.Id,//itemDetail.InventoryReceiveId,
                                                               //InventoryMaterialId = entity.InventoryMaterialId,
                                TransactionQty = itemDetail.NetQty,//itemDetail.TransactionQty,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                BaseUOMId = itemDetail.BaseUOMId,
                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                                TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                                TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                                //POID = itemDetail.POID,
                                //PODetailsID = itemDetail.PODetailsID,
                                TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                                ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                                ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTaxTranAmount), 2),
                                IssueQty = 0,
                                BaseIssueQty = 0,
                                TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 2),
                                PurchaseDocumentAcceptanceId = itemDetail.PurchaseDocumentAcceptanceId,
                                PurchaseDocumentAcceptanceDetailId = itemDetail.PurchaseDocumentAcceptanceDetailId,
                                PurchaseReturnQty = 0,
                                IssueReturnQty = 0,
                                InventorySalesQty = 0,
                                InventoryScrapQty = 0,
                                MaterialMasterOpeningBalanceDetailId = null,
                                LotNumber = itemDetail.LotNumber,
                                LotNo = itemDetail.LotNumber,
                                Diameter = itemDetail.Diameter,
                                Type = itemDetail.Type,
                                ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),
                                ShortageRatePercent = 110,
                                ShortageValue = Math.Round(Convert.ToDecimal(itemDetail.ShortageValue), 2),
                                RejectRatePercent = 50,
                                GRNQty = itemDetail.TransactionQty,
                                GRNTotalAmount = Math.Round(itemDetail.TransactionQty * Convert.ToDecimal(itemDetail.TransactionRate), 2),
                                IsAsset = itemDetail.IsAsset,
                                GrossAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                DiscountAmount = Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                QualityStatus = itemDetail.QualityStatus


                            };
                            try
                            {

                                itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                                receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);

                                AuditService.AddedLog(receiveDetail);
                                itemDetail.TotalQty = ((Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty + itemDetail.IssueReturnQty)) - (Convert.ToDecimal(itemDetail.IssueQty) + Convert.ToDecimal(itemDetail.PurchaseReturnQty) + Convert.ToDecimal(itemDetail.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetail.InventorySalesQty) + Convert.ToDecimal(itemDetail.InventoryScrapQty) + Convert.ToDecimal(itemDetail.InventoryTransferQty)));
                                itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetail.TotalQty);
                                itemDetail.ShortageQty = Convert.ToDecimal(receiveDetail.ShortageQty + ShortageQty);
                                itemDetail.RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty + RejectionQty);
                                itemDetail.ApprovedQty = Convert.ToDecimal(receiveDetail.ApprovedQty + ApprovedQty);

                                _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                                receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                                InsertGraph(receiveDetail);
                                updateArticleMinMaxValue(itemDetail.MinimumValue, itemDetail.MaximumValue, Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), itemDetail.ArticleId);


                                int rejectDetailId = 1;
                                var RejectionDetails = new GRNRejectionDetails
                                {
                                    Id = grndId.ToString() + rejectDetailId,
                                    GRNDeailsId = grndId,
                                    RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty),
                                    RejectionUoMId = itemDetail.TransactionUoMId,
                                    BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                    BaseUOMId = itemDetail.BaseUOMId,
                                    RejectionRate = Convert.ToDecimal(receiveDetail.RejectRatePercent),
                                    RejeactionValue = Convert.ToDecimal(receiveDetail.RejectValue),
                                };
                                AuditService.AddedLog(RejectionDetails);
                                _gRNRejectionDetailsRepository.Insert(RejectionDetails);
                               
                            }
                            catch (DivideByZeroException ex)
                            {

                            }
                            finally
                            {

                            }
                        }
                    }

                    // insert in receive tax
                    if (itemDetail.PurchaseDocumentAcceptanceId == null && itemDetail.PurchaseDocumentAcceptanceDetailId == null)
                    {
                        if (taxCategoryList.IsNotNull())
                        {
                            var currentId = 0;
                            //var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                            foreach (var item in taxCategoryList.Where(r => r.InventoryReceiveDetailId == Temppodetailid))
                            {
                                currentId++;
                                item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                                item.InventoryReceiveId = entity.Id;//itemDetail.InventoryReceiveId;
                                item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                                item.InventoryServiceId = null;
                                item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Insert(item);
                            }
                        }

                    }
                    else
                    {
                        var currentId = 0;
                        var AcceptanceMaterialTaxList = _PurchaseDocAcceptanceTaxRepository.Query(r => r.PurchaseDocAcceptanceId == itemDetail.PurchaseDocumentAcceptanceId && r.PurchaseDocAcceptanceDetailId != null && r.PurchaseDocAcceptanceDetailId == itemDetail.PurchaseDocumentAcceptanceDetailId).Select().ToList();//($"SELECT * FROM TRN.PurchaseDocAcceptanceTax WHERE PurchaseDocAcceptanceDetailId IS NULL AND PurchaseDocAcceptanceId='{AcceptanceId}'").ToList();

                        foreach (var item1 in AcceptanceMaterialTaxList)
                        {
                            currentId++;
                            var inventoryReceiveTax = new InventoryReceiveTax
                            {
                                Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2),
                                InventoryReceiveId = entity.Id,//itemDetail.InventoryReceiveId;
                                InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId,
                                InventoryServiceId = null,
                                TaxCategoryId = item1.TaxCategoryId,
                                Percentage = item1.Percentage,
                                TaxAmount = Math.Round(item1.TaxAmount, 2)
                            };
                            AuditService.AddedLog(inventoryReceiveTax);
                            _receiveTaxRepository.Insert(inventoryReceiveTax);
                        }
                    }

                    if (Convert.ToDecimal(itemDetail.POQty) > (Convert.ToDecimal(itemDetail.GRNRcvQty + itemDetail.TransactionQty)))
                    {
                        entity.msgForAllocationNeed = "You have to allocate GRN Qty manually for Sales Order ! Please go to edit mode for allocation";
                        //throw new CustomException("You have to allocate GRN Qty manually for Sales Order !");
                    }
                    else
                    {
                        var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"select --a.Id GRNID
										
											c.PODetailId
											,C.BOQDetailId
											,C.Id POBOQMAPID
											,C.TransactionQty TransactionQtyForPO
											,C.TransactionUoMId,uom.UserName TransactionUoM
											,C.BaseQty
											,C.BaseUoMId
											,C.POBOQQty
											,C.POUoMId
											,d.BOMQty ReqQty
											,0 allowQty
											,b.TransactionQty POTransactionQty
											--,a.TransactionQty GRNQty
											--,a.RejectionQty  GRNRejectionQty
											,0 TransactionQty
											,0 RejectionQty
											,null Active				
											,d.SalesOrderId
											,b.Id
											,isnull(AllocatedSOQty.AllocatedSOQty,0) AllocatedSOQty
											--From trn.InventoryReceiveDetail a
											From trn.PurchaseOrderDetail b --on b.Id=a.PODetailsId
											left join trn.POBOQMAP c on c.PODetailId=b.Id
											left join boq d On d.Id=c.BOQDetailId
											left join scs.UnitOfMeasurement uom ON uom.Id=c.TransactionUoMId 
											left JOIN(select POBOQMapId ,Sum(TransactionQty) AllocatedSOQty from trn.GRNPORequisitionAllocation  GROUP BY POBOQMapId)AllocatedSOQty ON AllocatedSOQty.POBOQMapId=c.Id
											where b.Id='" + itemDetail.PODetailsID + @"'").ToList();
                        if (receiveDetailList.IsNotNull())
                        {
                            bool isQtyAlocated = true;
                            decimal temp = 0;
                            int count = 0;
                            foreach (var issue in receiveDetailList)
                            {
                                count++;
                                if (count == 1)
                                {
                                    if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) > itemDetail.TransactionQty)
                                    {

                                        itemDetail.TransactionQty = itemDetail.TransactionQty;
                                        //temp += itemDetail.TransactionQty;
                                        isQtyAlocated = false;

                                    }
                                    else if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) < itemDetail.TransactionQty)
                                    {
                                        temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                        //temp = issue.TransactionQtyForPO - issue.AllocatedSOQty;
                                        itemDetail.TransactionQty = (issue.TransactionQtyForPO - issue.AllocatedSOQty);
                                        isQtyAlocated = true;

                                    }
                                    else
                                    {
                                        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                        itemDetail.TransactionQty = itemDetail.TransactionQty;
                                        isQtyAlocated = true;

                                    }
                                }
                                if (count > 1)
                                {
                                    if (isQtyAlocated == true)
                                    {
                                        if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) > temp)
                                        {
                                            //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = itemDetail.TransactionQty;
                                            isQtyAlocated = false;
                                        }
                                        if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) < temp)
                                        {
                                            //temp = temp - issue.TransactionQtyForPO;
                                            temp = (temp - (issue.TransactionQtyForPO - issue.AllocatedSOQty));
                                            //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = (issue.TransactionQtyForPO - issue.AllocatedSOQty);
                                            isQtyAlocated = true;
                                        }
                                        else
                                        {
                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = temp;
                                            isQtyAlocated = true;

                                        }

                                    }
                                    else
                                    {
                                        itemDetail.TransactionQty = 0;
                                    }
                                }

                                var baseQqtynew = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty));
                                var POBOQQtyNew = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty));
                                var gRNPOAllocation = new GRNPORequisitionAllocation
                                {
                                    Id = base.GetAutoNumber(nameof(GRNPORequisitionAllocation), PKGeneratorEnum.Yearly, null, DateTime.Now),
                                    InventoryReceiveDetailId = grndId,
                                    POBOQMapId = issue.POBOQMapId,
                                    POReqDetailsID = issue.POReqDetailsID,
                                    TransactionQty = Convert.ToDecimal(itemDetail.TransactionQty),
                                    TransactionUoMId = itemDetail.TransactionUoMId,
                                    BaseQty = baseQqtynew,
                                    //BaseQty = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty)),
                                    BaseUoMId = issue.BaseUOMId,
                                    //POBOQQty = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty)),
                                    POBOQQty = POBOQQtyNew,
                                    POUoMId = itemDetail.POUoMId,
                                    RejectQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                    RejectBaseQty = Convert.ToDecimal(itemDetail.RejectBaseQty),
                                    SalesOrderId = issue.SalesOrderId
                                    //AutoAllocate = true

                                };
                                AuditService.AddedLog(gRNPOAllocation);
                                _gRNPOAllocationRepository.Insert(gRNPOAllocation);
                            }
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


        public void InsertOrUpdateGraphNewEdits(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType)
        {
            var flag = false;
            var rdBuilder = new System.Text.StringBuilder();
            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                entity.GRNType = GRNType;
                AuditService.UpdatedLog(entity);
                _inventoryReceiveService.Update(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var currentId1 = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId='{entity.Id}'").First();
                var Temppodetailid = "";


                foreach (var itemDetail in entityMatAndImat)
                {
                    itemDetail.CompanyGroupId = identity.CompanyGroupId;
                    itemDetail.CompanyId = identity.CompanyId;
                    itemDetail.PlantId = identity.PlantId;
                    Temppodetailid = itemDetail.InventoryReceiveDetailId;

                    if (CheckItemExist(itemDetail))
                        throw new CustomException(itemDetail.MaterialMasterName + " already received");

                    ResetCurrencyRate(itemDetail);
                    itemDetail.ToCurrencyRate = entity.ToCurrencyRate;
                    if (itemDetail.IsNotNull())
                    {
                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                       
                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                          
                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);

                            }
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                            && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {
                          
                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            }
                            itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor; //itemDetail.TransactionQty;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                            //itemDetail.ChargesTranAmount = itemDetail.MaterialTranAmount * ratio;
                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {
                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            }
                            //itemDetail.BaseUoMFactor = 1; //Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor); //Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                                                                                                  //itemDetail.TotalMaterialTranAmount = itemDetail.TransactionAmount;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                        }

                        else
                        {
                         
                            itemDetail.BaseUoMFactor = 1;
                            itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor;//itemDetail.TransactionQty;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);

                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);

                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                        }
                        var poDetailData = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                        var GRNRcvQty = itemDetail.PreviousQty;
                        var poDetail = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                        // var IRDDetail = _receiveDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                        if (poDetail == null)
                            throw new CustomException("PO Details Or Inventory Details not found!");
                       
                        var PreviousShortQty = itemDetail.ShortageQty;
                        var PreviousRejectionQty = itemDetail.RejectionQty;
                        var PreviousApprovedQty = itemDetail.ApprovedQty;


                        // if (poDetail.BaseQty < poDetail.GRNRcvQty)

                        //if (poDetail.TransactionQty < poDetail.GRNRcvQty)
                        //    throw new CustomException("Received Qty can not cross balance Qty.");
                        poDetail.GRNRcvQty = itemDetail.TransactionQty;
                        poDetail.QtyStatus = poDetail.TransactionQty == poDetail.GRNRcvQty;

                        AuditService.UpdatedLog(poDetail);
                        _poDetailRepository.Update(poDetail);

                        var MaterialQty = _inventoryMaterialService.Query(r => r.MaterialMasterId == itemDetail.MaterialMasterId
                                            && r.ArticleId == itemDetail.ArticleId
                                            && r.FirstCharacteristicsId == itemDetail.FirstCharacteristicsId
                                            && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId
                                            && r.SecondCharacteristicsId == itemDetail.SecondCharacteristicsId
                                            && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId
                                            && r.ThirdCharacteristicsId == itemDetail.ThirdCharacteristicsId
                                            && r.ThirdCharacteristicsValueId == itemDetail.ThirdCharacteristicsValueId
                                            && r.CountryId == itemDetail.CountryId
                        ).Select().FirstOrDefault();
                        var TotalQty = MaterialQty.TotalQty;
                        var AvgQty = MaterialQty.AvgRate;
                        //var resQty = (MaterialQty.TotalQty - GRNRcvQty) + itemDetail.TransactionQty;
                        var resQty = (MaterialQty.TotalQty - GRNRcvQty) + itemDetail.NetQty;
                        //var resAvg = (itemDetail.TrnAmount / itemDetail.TransactionQty);
                        var resAvg = (itemDetail.TrnAmount / itemDetail.NetQty);

                        var resShortQty = (MaterialQty.ShortageQty - PreviousShortQty) + itemDetail.ShortageQty;
                        var resRejectionQty = (MaterialQty.RejectionQty - PreviousRejectionQty) + itemDetail.RejectionQty;
                        var resApprovedQty = (MaterialQty.ApprovedQty - PreviousApprovedQty) + itemDetail.ApprovedQty;

                        var sqlres = @"Update TRN.InventoryMaterial set TotalQty='" + resQty + "',AvgRate='" + resAvg + "',ShortageQty ='" + resShortQty + "', RejectionQty='" + resRejectionQty + "', ApprovedQty='" + resApprovedQty + "' " +
                            "where MaterialMasterId='" + MaterialQty.MaterialMasterId + "' " +
                            "AND ArticleId='" + MaterialQty.ArticleId + "' " +
                            "AND  isnull(FirstCharacteristicsId,'')='" + MaterialQty.FirstCharacteristicsId + "'" +
                            "AND  isnull(FirstCharacteristicsValueId,'')='" + MaterialQty.FirstCharacteristicsValueId + "'" +
                            "AND  isnull(SecondCharacteristicsId,'') = '" + MaterialQty.SecondCharacteristicsId + "'" +
                            "AND  isnull(SecondCharacteristicsId,'') = '" + MaterialQty.SecondCharacteristicsValueId + "'" +
                            "AND  isnull(ThirdCharacteristicsId,'') = '" + MaterialQty.ThirdCharacteristicsId + "'" +
                            "AND  isnull(ThirdCharacteristicsValueId,'') = '" + MaterialQty.ThirdCharacteristicsValueId + "'" +
                            "AND isnull(CountryId,'') = '" + MaterialQty.CountryId + "'";
                        _sqlRepository.GetDataCollection(sqlres);

                        var pruchaseReqD = _poDetailRepository.Find(itemDetail.PODetailsID);
                        pruchaseReqD.GRNRcvQty = Convert.ToDecimal(((poDetailData.GRNRcvQty - GRNRcvQty) + itemDetail.TransactionQty));
                        _poDetailRepository.Update(pruchaseReqD);

                        if (!string.IsNullOrEmpty(itemDetail.Id))
                        {
                            currentId1++;
                            var receiveDetail = new InventoryReceiveDetail
                            {

                                Id = itemDetail.InventoryReceiveDetailId,
                                MaterialStorageId = itemDetail.MaterialStorageId,//MaterialStorageId,
                                InventoryReceiveId = id,
                                //InventoryMaterialId = entity.InventoryMaterialId,
                                TransactionQty = itemDetail.NetQty,//itemDetail.TransactionQty,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                BaseUOMId = itemDetail.BaseUOMId,
                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                // TransactionRate = Convert.ToDecimal(itemDetail.TransactionRate),
                                MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                //TransactionAmount = Convert.ToDecimal(itemDetail.TransactionAmount),
                                MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                                //TotalMaterialTranAmount = Convert.ToDecimal(itemDetail.TotalMaterialTranAmount),
                                TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                                TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                                POID = itemDetail.POID,
                                PODetailsID = itemDetail.PODetailsID,
                                //TotalTaxAmount = Convert.ToDecimal(itemDetail.TotalTaxAmount),
                                TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                                //ChargesAmount= Convert.ToDecimal(itemDetail.ChargesAmount),
                                ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                                ChargesTaxTranAmount = Convert.ToDecimal(itemDetail.ChargesTaxTranAmount),
                                IssueQty = 0,
                                BaseIssueQty = 0,
                                PurchaseDocumentAcceptanceId = itemDetail.PurchaseDocumentAcceptanceId,
                                PurchaseDocumentAcceptanceDetailId = itemDetail.PurchaseDocumentAcceptanceDetailId,
                                LotNumber = itemDetail.LotNumber,
                                Diameter = itemDetail.Diameter,
                                Type = itemDetail.Type,
                                ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),
                                ShortageRatePercent = 110,
                                RejectRatePercent = 50,
                                GRNQty = itemDetail.TransactionQty,
                                GRNTotalAmount = (itemDetail.TransactionQty * Convert.ToDecimal(itemDetail.TransactionRate)),
                                TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 4),
                                PurchaseReturnQty = 0,
                                IssueReturnQty = 0,
                                ReductionByAdjustmentQty = 0,
                                InventorySalesQty = 0,
                                InventoryScrapQty = 0,
                                InventoryTransferQty = 0,
                                MaterialMasterOpeningBalanceDetailId = null,
                                GrossAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                DiscountAmount = Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                QualityStatus = itemDetail.QualityStatus

                            };
                            try
                            {

                                itemDetail.InventoryReceiveDetailId = receiveDetail.Id;

                                receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);

                                AuditService.UpdatedLog(receiveDetail);
                                receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                                UpdateGraph(receiveDetail);
                                var Val = _gRNRejectionDetailsRepository.Query(r => r.GRNDeailsId == receiveDetail.Id).Select().FirstOrDefault();

                                var RejectionDetails = new GRNRejectionDetails
                                {
                                    Id = Val.Id, //MakePK(NewId + currentId, 0,0),
                                    GRNDeailsId = Val.GRNDeailsId,
                                    RejectionQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                    RejectionUoMId = itemDetail.TransactionUoMId,
                                    BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                    BaseUOMId = itemDetail.BaseUOMId,
                                    RejectionRate = Convert.ToDecimal(itemDetail.RejectionRate),
                                    RejeactionValue = Convert.ToDecimal(itemDetail.RejectionValue),
                                };
                                AuditService.AddedLog(RejectionDetails);
                                _gRNRejectionDetailsRepository.Update(RejectionDetails);
                           
                            }
                            catch (DivideByZeroException ex)
                            {

                            }
                            finally
                            {

                            }
                        }
                    }

                    // insert in receive tax
                    if (taxCategoryList.IsNotNull())
                    {
                        //var currentId = 0;
                        //var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                        foreach (var item in taxCategoryList.Where(r => r.InventoryReceiveDetailId == Temppodetailid))
                        {
                            //currentId++;
                            //item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                            item.Id = item.Id;
                            item.InventoryReceiveId = id;//itemDetail.InventoryReceiveId;
                            item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                            item.InventoryServiceId = null;
                            item.TaxAmount = Math.Round(item.TaxAmount, 2);
                            AuditService.AddedLog(item);
                            _receiveTaxRepository.Update(item);
                        }
                    }
                    if (Convert.ToDecimal(itemDetail.POQty) > (Convert.ToDecimal(itemDetail.GRNRcvQty + itemDetail.TransactionQty)))
                    {
                        var GRNPORequisitionAllocation = _gRNPOAllocationRepository.Query(t => t.InventoryReceiveDetailId == itemDetail.InventoryReceiveDetailId).Select().ToList();
                        if (GRNPORequisitionAllocation.IsNotNull())
                        {
                            foreach (var item in GRNPORequisitionAllocation)
                            {
                                item.ModelState = ModelState.Deleted;
                                _gRNPOAllocationRepository.Delete(item);
                            }
                        }
                        entity.msgForAllocationNeed = "You have to allocate GRN Qty manually for Sales Order ! Please go to edit mode for allocation";
                        //throw new CustomException("You have to allocate GRN Qty manually for Sales Order !");
                    }
                    else
                    {

                        var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"select --a.Id GRNID
											c.PODetailId
											,C.BOQDetailId
											,C.Id POBOQMAPID
											,C.TransactionQty TransactionQtyForPO
											,C.TransactionUoMId,uom.UserName TransactionUoM
											,C.BaseQty
											,C.BaseUoMId
											,C.POBOQQty
											,C.POUoMId
											,d.BOMQty ReqQty
											,0 allowQty
											,b.TransactionQty POTransactionQty
											--,a.TransactionQty GRNQty
											--,a.RejectionQty  GRNRejectionQty
											,0 TransactionQty
											,0 RejectionQty
											,null Active				
											,d.SalesOrderId
											,b.Id
											,Isnull(AllocatedSOQty.AllocatedSOQty,0) AllocatedSOQty
											--From trn.InventoryReceiveDetail a
											From trn.PurchaseOrderDetail b --on b.Id=a.PODetailsId
											left join trn.POBOQMAP c on c.PODetailId=b.Id
											left join boq d On d.Id=c.BOQDetailId
											left join scs.UnitOfMeasurement uom ON uom.Id=c.TransactionUoMId 	
											left JOIN(select POBOQMapId ,Sum(TransactionQty) AllocatedSOQty from trn.GRNPORequisitionAllocation  GROUP BY POBOQMapId)AllocatedSOQty ON AllocatedSOQty.POBOQMapId=c.Id
											where b.Id='" + itemDetail.PODetailsID + @"'").ToList();
                        if (receiveDetailList.IsNotNull())
                        {
                            var GRNPORequisitionAllocation = _gRNPOAllocationRepository.Query(t => t.InventoryReceiveDetailId == itemDetail.InventoryReceiveDetailId).Select().ToList();
                            if (GRNPORequisitionAllocation.IsNotNull())
                            {
                                foreach (var item in GRNPORequisitionAllocation)
                                {
                                    item.ModelState = ModelState.Deleted;
                                    _gRNPOAllocationRepository.Delete(item);
                                }
                            }

                            bool isQtyAlocated = true;
                            decimal temp = 0;
                            int count = 0;
                            foreach (var issue in receiveDetailList)
                            {


                                count++;
                                if (count == 1)
                                {
                                    if (issue.TransactionQtyForPO > itemDetail.TransactionQty)
                                    {

                                        itemDetail.TransactionQty = itemDetail.TransactionQty;
                                        //temp += itemDetail.TransactionQty;
                                        isQtyAlocated = false;

                                    }
                                    else if (issue.TransactionQtyForPO < itemDetail.TransactionQty)
                                    {
                                        temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                        itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                        isQtyAlocated = true;

                                    }
                                    else
                                    {
                                        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                        itemDetail.TransactionQty = itemDetail.TransactionQty;
                                        isQtyAlocated = true;

                                    }
                                }
                                if (count > 1)
                                {
                                    if (isQtyAlocated == true)
                                    {
                                        if (issue.TransactionQtyForPO > temp)
                                        {
                                            //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = itemDetail.TransactionQty;
                                            isQtyAlocated = false;
                                        }
                                        if (issue.TransactionQtyForPO < temp)
                                        {
                                            temp = temp - issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                            isQtyAlocated = true;
                                        }
                                        else
                                        {
                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = temp;
                                            isQtyAlocated = true;

                                        }

                                    }
                                    else
                                    {
                                        itemDetail.TransactionQty = 0;
                                    }
                                }
                                var BaseQtynew = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty));
                                var POBOQQtynew = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.TransactionUoMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty));

                                var gRNPORequisitionAllocation = new GRNPORequisitionAllocation
                                {
                                    Id = base.GetAutoNumber(nameof(GRNPORequisitionAllocation), PKGeneratorEnum.Yearly, null, DateTime.Now),
                                    InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId,// grndId,
                                    POBOQMapId = issue.POBOQMapId,
                                    POReqDetailsID = issue.POReqDetailsID,
                                    TransactionQty = Convert.ToDecimal(itemDetail.TransactionQty),
                                    TransactionUoMId = itemDetail.TransactionUoMId,
                                    BaseQty = BaseQtynew,//(decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty)),
                                    BaseUoMId = issue.BaseUOMId,
                                    POBOQQty = POBOQQtynew, //(decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.TransactionUoMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty)),
                                    POUoMId = itemDetail.TransactionUoMId,
                                    RejectQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                    RejectBaseQty = Convert.ToDecimal(itemDetail.RejectBaseQty),
                                    SalesOrderId = issue.SalesOrderId

                                };
                                AuditService.AddedLog(gRNPORequisitionAllocation);
                                _gRNPOAllocationRepository.Insert(gRNPORequisitionAllocation);
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException ex)
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

        public void UpdateGRNBYPOMaster(InventoryReceive entity, string GRNType)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                entity.GRNType = GRNType;
                AuditService.UpdatedLog(entity);
                _inventoryReceiveService.Update(entity);
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException ex)
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
        public void InsertFOCDetail(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, IEnumerable<InventoryMaterialViewModel> List)
        {
            var flag = false;

            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {

                _unitOfWork.BeginTransaction();

                flag = true;
                entity.GRNType = GRNType;
                _inventoryReceiveService.Update(entity);
                if (entity.PurchaseDocumentAcceptanceId != null)
                {
                    var GRNAcceptance = new GRNAcceptanceMap
                    {
                        Id = base.GetAutoNumber(nameof(GRNAcceptanceMap), PKGeneratorEnum.Yearly, null, DateTime.Now),
                        GRNId = entity.Id,
                        PurchaseDocumentAcceptanceId = entity.PurchaseDocumentAcceptanceId,
                        //Qty = receiveDetail.TransactionQty
                    };
                    AuditService.AddedLog(GRNAcceptance);
                    _GRNAcceptanceMapRepository.Insert(GRNAcceptance);

                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var currentId1 = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{entity.Id}'").First();
                var Temppodetailid = "";
                var grndId = "";
                foreach (var itemDetail in List)
                {
                    itemDetail.CompanyGroupId = identity.CompanyGroupId;
                    itemDetail.CompanyId = identity.CompanyId;
                    itemDetail.PlantId = identity.PlantId;
                    Temppodetailid = itemDetail.InventoryReceiveDetailId;
                    itemDetail.IsNonCreditable = entity.IsNonCreditable;

                    if (CheckItemExist(itemDetail))
                        throw new CustomException(itemDetail.MaterialMasterName + " already received");

                    ResetCurrencyRate(itemDetail);
                    itemDetail.ToCurrencyRate = entity.ToCurrencyRate;
                    if (itemDetail.IsNotNull())
                    {
                        if (itemDetail.PurchaseDocumentAcceptanceId != null)
                        {
                            itemDetail.ToCurrencyRate = entity.ToCurrencyRate;

                        }


                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());

                        itemDetail.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.PurchaseReturnQty).Sum());
                        itemDetail.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueReturnQty).Sum());
                        itemDetail.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                        itemDetail.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventorySalesQty).Sum());
                        itemDetail.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryScrapQty).Sum());
                        itemDetail.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryTransferQty).Sum());

                        var ShortageQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ShortageQty).Sum();
                        var RejectionQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.RejectionQty).Sum();
                        var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ApprovedQty).Sum();


                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {

                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            }
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.TotalMaterialTranAmount = 0;
                            itemDetail.ChargesTranAmount = 0; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = 0;//itemDetail.TrnAmount * ratioServiceTax;

                            itemDetail.TotalMaterialBooksCurrencyAmount = 0;

                            itemDetail.TrnCurrencyBaseRate = 0;
                            itemDetail.BooksCurrencyBaseRate = 0;
                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                            && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {

                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            }
                            itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor;//itemDetail.TransactionQty;
                            itemDetail.TotalMaterialTranAmount = 0;
                            itemDetail.ChargesTranAmount = 0; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = 0;//itemDetail.TrnAmount * ratioServiceTax;
                            if (itemDetail.TotalTaxAmount == null)
                                itemDetail.TotalTaxAmount = 0;
                            itemDetail.TotalMaterialTranAmount += 0;
                            itemDetail.TotalMaterialBooksCurrencyAmount = 0;
                            itemDetail.TotalMaterialBooksCurrencyAmount += 0;
                            itemDetail.TrnCurrencyBaseRate = 0;
                            itemDetail.BooksCurrencyBaseRate = 0;
                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {

                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);

                            }
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.MaterialTranAmount = 0;
                            itemDetail.ChargesTranAmount = 0; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = 0;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalTaxAmount = 0;
                            itemDetail.TotalMaterialBooksCurrencyAmount = 0;
                            itemDetail.TrnCurrencyBaseRate = 0;
                            itemDetail.BooksCurrencyBaseRate = 0;
                        }

                        else
                        {

                            itemDetail.BaseUoMFactor = 1;
                            itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor;//itemDetail.TransactionQty;
                            itemDetail.ChargesTranAmount = 0; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = 0;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalTaxAmount = 0;
                            itemDetail.TotalMaterialBooksCurrencyAmount = 0;
                            itemDetail.TrnCurrencyBaseRate = 0;
                            itemDetail.BooksCurrencyBaseRate = 0;

                        }
                        
                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {
                            var NewId = entity.Id + "-";
                            currentId1++;
                            grndId = NewId + currentId1;
                            var receiveDetail = new InventoryReceiveDetail
                            {
                                Id = NewId + currentId1, //MakePK(NewId + currentId, 0,0),
                                MaterialStorageId = itemDetail.MaterialStorageId,//MaterialStorageId
                                InventoryReceiveId = entity.Id,//itemDetail.InventoryReceiveId,
                                                               //InventoryMaterialId = entity.InventoryMaterialId,
                                TransactionQty = itemDetail.NetQty,//itemDetail.TransactionQty,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                BaseUOMId = itemDetail.BaseUOMId,
                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                                TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                                TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                                POID = itemDetail.POID,
                                PODetailsID = itemDetail.PODetailsID,
                                TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                                ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                                ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTaxTranAmount), 2),
                                IssueQty = 0,
                                BaseIssueQty = 0,
                                TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 2),
                                PurchaseDocumentAcceptanceId = itemDetail.PurchaseDocumentAcceptanceId,
                                PurchaseDocumentAcceptanceDetailId = itemDetail.PurchaseDocumentAcceptanceDetailId,
                                PurchaseReturnQty = 0,
                                IssueReturnQty = 0,
                                InventorySalesQty = 0,
                                InventoryScrapQty = 0,
                                MaterialMasterOpeningBalanceDetailId = null,
                                LotNumber = itemDetail.LotNumber,
                                LotNo = itemDetail.LotNumber,
                                Diameter = itemDetail.Diameter,
                                Type = itemDetail.Type,
                                ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),
                                ShortageRatePercent = 110,
                                ShortageValue = Math.Round(Convert.ToDecimal(itemDetail.ShortageValue), 2),
                                RejectRatePercent = 50,
                                GRNQty = itemDetail.TransactionQty,
                                GRNTotalAmount = Math.Round(itemDetail.TransactionQty * Convert.ToDecimal(itemDetail.TransactionRate), 2),
                                IsAsset = itemDetail.IsAsset,
                                GrossAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                DiscountAmount = Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                QualityStatus = itemDetail.QualityStatus,
                                MasterOrderItemId = itemDetail.MasterOrderItemId

                            };
                            try
                            {

                                itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                                receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);

                                AuditService.AddedLog(receiveDetail);
                                itemDetail.TotalQty = ((Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty + itemDetail.IssueReturnQty)) - (Convert.ToDecimal(itemDetail.IssueQty) + Convert.ToDecimal(itemDetail.PurchaseReturnQty) + Convert.ToDecimal(itemDetail.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetail.InventorySalesQty) + Convert.ToDecimal(itemDetail.InventoryScrapQty) + Convert.ToDecimal(itemDetail.InventoryTransferQty)));
                                itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetail.TotalQty);
                                itemDetail.ShortageQty = Convert.ToDecimal(receiveDetail.ShortageQty + ShortageQty);
                                itemDetail.RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty + RejectionQty);
                                itemDetail.ApprovedQty = Convert.ToDecimal(receiveDetail.ApprovedQty + ApprovedQty);

                                _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                                receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                                InsertGraph(receiveDetail);



                                int rejectDetailId = 1;
                                var RejectionDetails = new GRNRejectionDetails
                                {
                                    Id = grndId.ToString() + rejectDetailId,
                                    GRNDeailsId = grndId,
                                    RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty),
                                    RejectionUoMId = itemDetail.TransactionUoMId,
                                    BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                    BaseUOMId = itemDetail.BaseUOMId,
                                    RejectionRate = Convert.ToDecimal(receiveDetail.RejectRatePercent),
                                    RejeactionValue = Convert.ToDecimal(receiveDetail.RejectValue),
                                };
                                AuditService.AddedLog(RejectionDetails);
                                _gRNRejectionDetailsRepository.Insert(RejectionDetails);
                            }
                            catch (DivideByZeroException ex)
                            {

                            }
                            finally
                            {

                            }
                        }
                    }

                    // insert in receive tax
                    if (itemDetail.PurchaseDocumentAcceptanceId == null && itemDetail.PurchaseDocumentAcceptanceDetailId == null)
                    {
                        if (taxCategoryList.IsNotNull())
                        {
                            var currentId = 0;
                            //var currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                            foreach (var item in taxCategoryList.Where(r => r.InventoryReceiveDetailId == Temppodetailid))
                            {
                                currentId++;
                                item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                                item.InventoryReceiveId = entity.Id;//itemDetail.InventoryReceiveId;
                                item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                                item.InventoryServiceId = null;
                                item.TaxAmount = Math.Round(item.TaxAmount, 2);
                                AuditService.AddedLog(item);
                                _receiveTaxRepository.Insert(item);
                            }
                        }

                    }
                    else
                    {
                        var currentId = 0;
                        var AcceptanceMaterialTaxList = _PurchaseDocAcceptanceTaxRepository.Query(r => r.PurchaseDocAcceptanceId == itemDetail.PurchaseDocumentAcceptanceId && r.PurchaseDocAcceptanceDetailId != null && r.PurchaseDocAcceptanceDetailId == itemDetail.PurchaseDocumentAcceptanceDetailId).Select().ToList();//($"SELECT * FROM TRN.PurchaseDocAcceptanceTax WHERE PurchaseDocAcceptanceDetailId IS NULL AND PurchaseDocAcceptanceId='{AcceptanceId}'").ToList();

                        foreach (var item1 in AcceptanceMaterialTaxList)
                        {
                            currentId++;
                            var inventoryReceiveTax = new InventoryReceiveTax
                            {
                                Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2),
                                InventoryReceiveId = entity.Id,//itemDetail.InventoryReceiveId;
                                InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId,
                                InventoryServiceId = null,
                                TaxCategoryId = item1.TaxCategoryId,
                                Percentage = item1.Percentage,
                                TaxAmount = Math.Round(item1.TaxAmount, 2)
                            };
                            AuditService.AddedLog(inventoryReceiveTax);
                            _receiveTaxRepository.Insert(inventoryReceiveTax);
                        }
                    }

                    if (Convert.ToDecimal(itemDetail.POQty) > (Convert.ToDecimal(itemDetail.GRNRcvQty + itemDetail.TransactionQty)))
                    {
                        entity.msgForAllocationNeed = "You have to allocate GRN Qty manually for Sales Order ! Please go to edit mode for allocation";
                    }
                    else
                    {
                        var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"select --a.Id GRNID
											--,MGM.UserName AS MaterialGroupMasterName
											--,MM.Id AS MaterialMasterId
											--,MM.UserName
											--,IM.ArticleId
											--,ART.StandardName
											--,IM.FirstCharacteristicsId
											--,FC.UserName AS FirstCharacteristics
											--,IM.FirstCharacteristicsValueId
											--,FCV.UserName AS FirstCharacteristicsValue
											--,IM.SecondCharacteristicsId
											--,SC.UserName AS SecondCharacteristics
											--,IM.SecondCharacteristicsValueId
											--,SCV.UserName AS SecondCharacteristicsValue
											--,IM.ThirdCharacteristicsId
											--,TC.UserName AS ThirdCharacteristics
											--,IM.ThirdCharacteristicsValueId
											--,TCV.UserName AS ThirdCharacteristicsValue
											c.PODetailId
											,C.BOQDetailId
											,C.Id POBOQMAPID
											,C.TransactionQty TransactionQtyForPO
											,C.TransactionUoMId,uom.UserName TransactionUoM
											,C.BaseQty
											,C.BaseUoMId
											,C.POBOQQty
											,C.POUoMId
											,d.BOMQty ReqQty
											,0 allowQty
											,b.TransactionQty POTransactionQty
											--,a.TransactionQty GRNQty
											--,a.RejectionQty  GRNRejectionQty
											,0 TransactionQty
											,0 RejectionQty
											,null Active				
											,d.SalesOrderId
											,b.Id
											,isnull(AllocatedSOQty.AllocatedSOQty,0) AllocatedSOQty
											--From trn.InventoryReceiveDetail a
											From trn.PurchaseOrderDetail b --on b.Id=a.PODetailsId
											left join trn.POBOQMAP c on c.PODetailId=b.Id
											left join boq d On d.Id=c.BOQDetailId
											--left JOIN trn.InventoryMaterial IM ON IM.Id=a.InventoryMaterialId
											--left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
											--LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
											--LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
											--LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
											--LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
											--LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
											--LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
											--LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
											--LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
											left join scs.UnitOfMeasurement uom ON uom.Id=c.TransactionUoMId 
											left JOIN(select POBOQMapId ,Sum(TransactionQty) AllocatedSOQty from trn.GRNPORequisitionAllocation  GROUP BY POBOQMapId)AllocatedSOQty ON AllocatedSOQty.POBOQMapId=c.Id
											where b.Id='" + itemDetail.PODetailsID + @"'").ToList();
                        if (receiveDetailList.IsNotNull())
                        {
                            bool isQtyAlocated = true;
                            decimal temp = 0;
                            int count = 0;
                            foreach (var issue in receiveDetailList)
                            {
                                count++;
                                if (count == 1)
                                {
                                    if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) > itemDetail.TransactionQty)
                                    {

                                        itemDetail.TransactionQty = itemDetail.TransactionQty;
                                        //temp += itemDetail.TransactionQty;
                                        isQtyAlocated = false;

                                    }
                                    else if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) < itemDetail.TransactionQty)
                                    {
                                        temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                        //temp = issue.TransactionQtyForPO - issue.AllocatedSOQty;
                                        itemDetail.TransactionQty = (issue.TransactionQtyForPO - issue.AllocatedSOQty);
                                        isQtyAlocated = true;

                                    }
                                    else
                                    {
                                        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                        itemDetail.TransactionQty = itemDetail.TransactionQty;
                                        isQtyAlocated = true;

                                    }
                                }
                                if (count > 1)
                                {
                                    if (isQtyAlocated == true)
                                    {
                                        if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) > temp)
                                        {
                                            //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = itemDetail.TransactionQty;
                                            isQtyAlocated = false;
                                        }
                                        if ((issue.TransactionQtyForPO - issue.AllocatedSOQty) < temp)
                                        {
                                            //temp = temp - issue.TransactionQtyForPO;
                                            temp = (temp - (issue.TransactionQtyForPO - issue.AllocatedSOQty));
                                            //itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = (issue.TransactionQtyForPO - issue.AllocatedSOQty);
                                            isQtyAlocated = true;
                                        }
                                        else
                                        {
                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = temp;
                                            isQtyAlocated = true;

                                        }

                                    }
                                    else
                                    {
                                        itemDetail.TransactionQty = 0;
                                    }
                                }

                                var baseQqtynew = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty));
                                var POBOQQtyNew = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.POUoMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty));
                                var gRNPOAllocation = new GRNPORequisitionAllocation
                                {
                                    Id = base.GetAutoNumber(nameof(GRNPORequisitionAllocation), PKGeneratorEnum.Yearly, null, DateTime.Now),
                                    InventoryReceiveDetailId = grndId,
                                    POBOQMapId = issue.POBOQMapId,
                                    POReqDetailsID = issue.POReqDetailsID,
                                    TransactionQty = Convert.ToDecimal(itemDetail.TransactionQty),
                                    TransactionUoMId = itemDetail.TransactionUoMId,
                                    BaseQty = baseQqtynew,
                                    BaseUoMId = issue.BaseUOMId,
                                    POBOQQty = POBOQQtyNew,
                                    POUoMId = itemDetail.POUoMId,
                                    RejectQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                    RejectBaseQty = Convert.ToDecimal(itemDetail.RejectBaseQty),
                                    SalesOrderId = issue.SalesOrderId
                                    //AutoAllocate = true

                                };
                                AuditService.AddedLog(gRNPOAllocation);
                                _gRNPOAllocationRepository.Insert(gRNPOAllocation);
                            }
                        }
                    }


                    foreach (var item in entityMat.Where(q => q.MaterialMasterId == itemDetail.MaterialMasterId && q.ArticleId == itemDetail.ArticleId && q.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId && q.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId))
                    {
                        var grnboqmap = new GRNBOQMAP
                        {
                            Id = base.GetAutoNumber(nameof(GRNBOQMAP), PKGeneratorEnum.Yearly, null, DateTime.Now),
                            InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId,
                            BOQDetailId = item.BOQId,
                            TransactionQty = item.TransactionQty,
                            BaseQty = item.BaseQty,
                            POBOQQty = item.POBOQQty,
                            BaseUoMId = item.BaseUOMId,
                            POUoMId = item.POUoMId,

                        };
                        AuditService.AddedLog(grnboqmap);
                        _GRNBOQMAPRepository.Insert(grnboqmap);
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

        public void UpdateFOCDetail(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMatAndImat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType)
        {
            var flag = false;
            var rdBuilder = new System.Text.StringBuilder();
            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                entity.GRNType = GRNType;
                AuditService.UpdatedLog(entity);
                _inventoryReceiveService.Update(entity);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var currentId1 = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId='{entity.Id}'").First();
                var Temppodetailid = "";


                foreach (var itemDetail in entityMatAndImat)
                {
                    itemDetail.CompanyGroupId = identity.CompanyGroupId;
                    itemDetail.CompanyId = identity.CompanyId;
                    itemDetail.PlantId = identity.PlantId;
                    Temppodetailid = itemDetail.InventoryReceiveDetailId;

                    if (CheckItemExist(itemDetail))
                        throw new CustomException(itemDetail.MaterialMasterName + " already received");

                    ResetCurrencyRate(itemDetail);
                    itemDetail.ToCurrencyRate = entity.ToCurrencyRate;
                    if (itemDetail.IsNotNull())
                    {
                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);



                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {

                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);

                            }
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                            && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {

                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            }
                            itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor; //itemDetail.TransactionQty;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                            //itemDetail.ChargesTranAmount = itemDetail.MaterialTranAmount * ratio;
                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {

                            if (baseUoMFactorList.Count() == 0)
                            {
                                itemDetail.BaseUoMFactor = 1;
                            }
                            else
                            {
                                Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            }
                            //itemDetail.BaseUoMFactor = 1; //Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor); //Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                                                                                                  //itemDetail.TotalMaterialTranAmount = itemDetail.TransactionAmount;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                        }

                        else
                        {

                            itemDetail.BaseUoMFactor = 1;
                            itemDetail.BaseQty = itemDetail.NetQty * itemDetail.BaseUoMFactor;//itemDetail.TransactionQty;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);

                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);

                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                        }
                        var poDetailData = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                        var GRNRcvQty = itemDetail.PreviousQty;

                        var poDetail = _poDetailRepository.Query(r => r.Id == itemDetail.PODetailsID).Select().FirstOrDefault();
                        if (poDetail == null)
                            throw new CustomException("PO Details Or Inventory Details not found!");
                        var PreviousShortQty = itemDetail.ShortageQty;
                        var PreviousRejectionQty = itemDetail.RejectionQty;
                        var PreviousApprovedQty = itemDetail.ApprovedQty;
                        poDetail.GRNRcvQty = itemDetail.TransactionQty;
                        poDetail.QtyStatus = poDetail.TransactionQty == poDetail.GRNRcvQty;

                        AuditService.UpdatedLog(poDetail);
                        _poDetailRepository.Update(poDetail);

                        var MaterialQty = _inventoryMaterialService.Query(r => r.MaterialMasterId == itemDetail.MaterialMasterId
                                            && r.ArticleId == itemDetail.ArticleId
                                            && r.FirstCharacteristicsId == itemDetail.FirstCharacteristicsId
                                            && r.FirstCharacteristicsValueId == itemDetail.FirstCharacteristicsValueId
                                            && r.SecondCharacteristicsId == itemDetail.SecondCharacteristicsId
                                            && r.SecondCharacteristicsValueId == itemDetail.SecondCharacteristicsValueId
                                            && r.ThirdCharacteristicsId == itemDetail.ThirdCharacteristicsId
                                            && r.ThirdCharacteristicsValueId == itemDetail.ThirdCharacteristicsValueId
                                            && r.CountryId == itemDetail.CountryId
                        ).Select().FirstOrDefault();
                        var TotalQty = MaterialQty.TotalQty;
                        var AvgQty = MaterialQty.AvgRate;
                        var resQty = (MaterialQty.TotalQty - GRNRcvQty) + itemDetail.NetQty;
                        var resAvg = (itemDetail.TrnAmount / itemDetail.NetQty);

                        var resShortQty = (MaterialQty.ShortageQty - PreviousShortQty) + itemDetail.ShortageQty;
                        var resRejectionQty = (MaterialQty.RejectionQty - PreviousRejectionQty) + itemDetail.RejectionQty;
                        var resApprovedQty = (MaterialQty.ApprovedQty - PreviousApprovedQty) + itemDetail.ApprovedQty;

                        var sqlres = @"Update TRN.InventoryMaterial set TotalQty='" + resQty + "',AvgRate='" + resAvg + "',ShortageQty ='" + resShortQty + "', RejectionQty='" + resRejectionQty + "', ApprovedQty='" + resApprovedQty + "' " +
                            "where MaterialMasterId='" + MaterialQty.MaterialMasterId + "' " +
                            "AND ArticleId='" + MaterialQty.ArticleId + "' " +
                            "AND  isnull(FirstCharacteristicsId,'')='" + MaterialQty.FirstCharacteristicsId + "'" +
                            "AND  isnull(FirstCharacteristicsValueId,'')='" + MaterialQty.FirstCharacteristicsValueId + "'" +
                            "AND  isnull(SecondCharacteristicsId,'') = '" + MaterialQty.SecondCharacteristicsId + "'" +
                            "AND  isnull(SecondCharacteristicsId,'') = '" + MaterialQty.SecondCharacteristicsValueId + "'" +
                            "AND  isnull(ThirdCharacteristicsId,'') = '" + MaterialQty.ThirdCharacteristicsId + "'" +
                            "AND  isnull(ThirdCharacteristicsValueId,'') = '" + MaterialQty.ThirdCharacteristicsValueId + "'" +
                            "AND isnull(CountryId,'') = '" + MaterialQty.CountryId + "'";
                        _sqlRepository.GetDataCollection(sqlres);
                        var pruchaseReqD = _poDetailRepository.Find(itemDetail.PODetailsID);
                        pruchaseReqD.GRNRcvQty = Convert.ToDecimal(((poDetailData.GRNRcvQty - GRNRcvQty) + itemDetail.TransactionQty));
                        _poDetailRepository.Update(pruchaseReqD);

                        if (!string.IsNullOrEmpty(itemDetail.Id))
                        {
                            currentId1++;
                            var receiveDetail = new InventoryReceiveDetail
                            {

                                Id = itemDetail.InventoryReceiveDetailId,
                                MaterialStorageId = itemDetail.MaterialStorageId,//MaterialStorageId,
                                InventoryReceiveId = id,
                                TransactionQty = itemDetail.NetQty,//itemDetail.TransactionQty,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                BaseUOMId = itemDetail.BaseUOMId,
                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                                TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                                TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                                POID = itemDetail.POID,
                                PODetailsID = itemDetail.PODetailsID,
                                TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                                ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                                ChargesTaxTranAmount = Convert.ToDecimal(itemDetail.ChargesTaxTranAmount),
                                IssueQty = 0,
                                BaseIssueQty = 0,
                                PurchaseDocumentAcceptanceId = itemDetail.PurchaseDocumentAcceptanceId,
                                PurchaseDocumentAcceptanceDetailId = itemDetail.PurchaseDocumentAcceptanceDetailId,
                                LotNumber = itemDetail.LotNumber,
                                Diameter = itemDetail.Diameter,
                                Type = itemDetail.Type,
                                ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),
                                ShortageRatePercent = 110,
                                RejectRatePercent = 50,
                                GRNQty = itemDetail.TransactionQty,
                                GRNTotalAmount = (itemDetail.TransactionQty * Convert.ToDecimal(itemDetail.TransactionRate)),
                                TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 4),
                                PurchaseReturnQty = 0,
                                IssueReturnQty = 0,
                                ReductionByAdjustmentQty = 0,
                                InventorySalesQty = 0,
                                InventoryScrapQty = 0,
                                InventoryTransferQty = 0,
                                MaterialMasterOpeningBalanceDetailId = null,
                                GrossAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                DiscountAmount = Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                QualityStatus = itemDetail.QualityStatus

                            };
                            try
                            {

                                itemDetail.InventoryReceiveDetailId = receiveDetail.Id;

                                receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);


                                AuditService.UpdatedLog(receiveDetail);
                                receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                                UpdateGraph(receiveDetail);
                                var Val = _gRNRejectionDetailsRepository.Query(r => r.GRNDeailsId == receiveDetail.Id).Select().FirstOrDefault();

                                var RejectionDetails = new GRNRejectionDetails
                                {
                                    Id = Val.Id, //MakePK(NewId + currentId, 0,0),
                                    GRNDeailsId = Val.GRNDeailsId,
                                    RejectionQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                    RejectionUoMId = itemDetail.TransactionUoMId,
                                    BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                    BaseUOMId = itemDetail.BaseUOMId,
                                    RejectionRate = Convert.ToDecimal(itemDetail.RejectionRate),
                                    RejeactionValue = Convert.ToDecimal(itemDetail.RejectionValue),
                                };
                                AuditService.AddedLog(RejectionDetails);
                                _gRNRejectionDetailsRepository.Update(RejectionDetails);

                            }
                            catch (DivideByZeroException  )
                            {

                            }
                            finally
                            {

                            }
                        }
                    }
                    if (taxCategoryList.IsNotNull())
                    {
                        foreach (var item in taxCategoryList.Where(r => r.InventoryReceiveDetailId == Temppodetailid))
                        {
                            item.Id = item.Id;
                            item.InventoryReceiveId = id;//itemDetail.InventoryReceiveId;
                            item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                            item.InventoryServiceId = null;
                            item.TaxAmount = Math.Round(item.TaxAmount, 2);
                            AuditService.AddedLog(item);
                            _receiveTaxRepository.Update(item);
                        }
                    }
                    if (Convert.ToDecimal(itemDetail.POQty) > (Convert.ToDecimal(itemDetail.GRNRcvQty + itemDetail.TransactionQty)))
                    {
                        var GRNPORequisitionAllocation = _gRNPOAllocationRepository.Query(t => t.InventoryReceiveDetailId == itemDetail.InventoryReceiveDetailId).Select().ToList();
                        if (GRNPORequisitionAllocation.IsNotNull())
                        {
                            foreach (var item in GRNPORequisitionAllocation)
                            {
                                item.ModelState = ModelState.Deleted;
                                _gRNPOAllocationRepository.Delete(item);
                            }
                        }
                        entity.msgForAllocationNeed = "You have to allocate GRN Qty manually for Sales Order ! Please go to edit mode for allocation";
                    }
                    else
                    {

                        var receiveDetailList = _sqlRepository.GetModelCollection<InventoryMaterialViewModel>(@"select --a.Id GRNID
											--,MGM.UserName AS MaterialGroupMasterName
											--,MM.Id AS MaterialMasterId
											--,MM.UserName
											--,IM.ArticleId
											--,ART.StandardName
											--,IM.FirstCharacteristicsId
											--,FC.UserName AS FirstCharacteristics
											--,IM.FirstCharacteristicsValueId
											--,FCV.UserName AS FirstCharacteristicsValue
											--,IM.SecondCharacteristicsId
											--,SC.UserName AS SecondCharacteristics
											--,IM.SecondCharacteristicsValueId
											--,SCV.UserName AS SecondCharacteristicsValue
											--,IM.ThirdCharacteristicsId
											--,TC.UserName AS ThirdCharacteristics
											--,IM.ThirdCharacteristicsValueId
											--,TCV.UserName AS ThirdCharacteristicsValue
											c.PODetailId
											,C.BOQDetailId
											,C.Id POBOQMAPID
											,C.TransactionQty TransactionQtyForPO
											,C.TransactionUoMId,uom.UserName TransactionUoM
											,C.BaseQty
											,C.BaseUoMId
											,C.POBOQQty
											,C.POUoMId
											,d.BOMQty ReqQty
											,0 allowQty
											,b.TransactionQty POTransactionQty
											--,a.TransactionQty GRNQty
											--,a.RejectionQty  GRNRejectionQty
											,0 TransactionQty
											,0 RejectionQty
											,null Active				
											,d.SalesOrderId
											,b.Id
											,Isnull(AllocatedSOQty.AllocatedSOQty,0) AllocatedSOQty
											--From trn.InventoryReceiveDetail a
											From trn.PurchaseOrderDetail b --on b.Id=a.PODetailsId
											left join trn.POBOQMAP c on c.PODetailId=b.Id
											left join boq d On d.Id=c.BOQDetailId
											--left JOIN trn.InventoryMaterial IM ON IM.Id=a.InventoryMaterialId
											--left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId = MM.Id
											--LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId = MGM.Id
											--LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId = ART.Id
											--LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId = FC.Id
											--LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId = SC.Id
											--LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId = TC.Id
											--LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId = FCV.Id
											--LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId = SCV.Id
											--LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId = TCV.Id
											left join scs.UnitOfMeasurement uom ON uom.Id=c.TransactionUoMId 	
											left JOIN(select POBOQMapId ,Sum(TransactionQty) AllocatedSOQty from trn.GRNPORequisitionAllocation  GROUP BY POBOQMapId)AllocatedSOQty ON AllocatedSOQty.POBOQMapId=c.Id
											where b.Id='" + itemDetail.PODetailsID + @"'").ToList();
                        if (receiveDetailList.IsNotNull())
                        {
                            var GRNPORequisitionAllocation = _gRNPOAllocationRepository.Query(t => t.InventoryReceiveDetailId == itemDetail.InventoryReceiveDetailId).Select().ToList();
                            if (GRNPORequisitionAllocation.IsNotNull())
                            {
                                foreach (var item in GRNPORequisitionAllocation)
                                {
                                    item.ModelState = ModelState.Deleted;
                                    _gRNPOAllocationRepository.Delete(item);
                                }
                            }

                            bool isQtyAlocated = true;
                            decimal temp = 0;
                            int count = 0;
                            foreach (var issue in receiveDetailList)
                            {


                                count++;
                                if (count == 1)
                                {
                                    if (issue.TransactionQtyForPO > itemDetail.TransactionQty)
                                    {

                                        itemDetail.TransactionQty = itemDetail.TransactionQty;
                                        //temp += itemDetail.TransactionQty;
                                        isQtyAlocated = false;

                                    }
                                    else if (issue.TransactionQtyForPO < itemDetail.TransactionQty)
                                    {
                                        temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                        itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                        isQtyAlocated = true;

                                    }
                                    else
                                    {
                                        //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                        itemDetail.TransactionQty = itemDetail.TransactionQty;
                                        isQtyAlocated = true;

                                    }
                                }
                                if (count > 1)
                                {
                                    if (isQtyAlocated == true)
                                    {
                                        if (issue.TransactionQtyForPO > temp)
                                        {
                                            //temp = itemDetail.TransactionQty- issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = itemDetail.TransactionQty;
                                            isQtyAlocated = false;
                                        }
                                        if (issue.TransactionQtyForPO < temp)
                                        {
                                            temp = temp - issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = issue.TransactionQtyForPO;
                                            isQtyAlocated = true;
                                        }
                                        else
                                        {
                                            //temp = itemDetail.TransactionQty - issue.TransactionQtyForPO;
                                            itemDetail.TransactionQty = temp;
                                            isQtyAlocated = true;

                                        }

                                    }
                                    else
                                    {
                                        itemDetail.TransactionQty = 0;
                                    }
                                }

                                var BaseQtynew = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty));
                                var POBOQQtynew = (decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.TransactionUoMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty));

                                var gRNPORequisitionAllocation = new GRNPORequisitionAllocation
                                {
                                    Id = base.GetAutoNumber(nameof(GRNPORequisitionAllocation), PKGeneratorEnum.Yearly, null, DateTime.Now),
                                    InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId,// grndId,
                                    POBOQMapId = issue.POBOQMapId,
                                    POReqDetailsID = issue.POReqDetailsID,
                                    TransactionQty = Convert.ToDecimal(itemDetail.TransactionQty),
                                    TransactionUoMId = itemDetail.TransactionUoMId,
                                    BaseQty = BaseQtynew,//(decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.BaseUOMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty)),
                                    BaseUoMId = issue.BaseUOMId,
                                    POBOQQty = POBOQQtynew, //(decimal)conversion.Convert(itemDetail.MaterialMasterId, itemDetail.TransactionUoMId, itemDetail.TransactionUoMId.ToString(), Convert.ToDouble(itemDetail.TransactionQty)),
                                    POUoMId = itemDetail.TransactionUoMId,
                                    RejectQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                    RejectBaseQty = Convert.ToDecimal(itemDetail.RejectBaseQty),
                                    SalesOrderId = issue.SalesOrderId

                                };
                                AuditService.AddedLog(gRNPORequisitionAllocation);
                                _gRNPOAllocationRepository.Insert(gRNPORequisitionAllocation);
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException ex)
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

        public void InsertOrUpdateGraphNewEditsOnlyGRN(IEnumerable<InventoryMaterialViewModel> entityMat, string Id)//Sk
        {
            var flag = false;

            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{Id}'").First();
                var Temppodetailid = "";
                var grndId = "";
                foreach (var itemDetail in entityMat)
                {
                    itemDetail.CompanyGroupId = identity.CompanyGroupId;
                    itemDetail.CompanyId = identity.CompanyId;
                    itemDetail.PlantId = identity.PlantId;
                    Temppodetailid = itemDetail.InventoryReceiveDetailId;

                    //if (CheckItemExist(itemDetail))
                    //	throw new CustomException(itemDetail.MaterialMasterName + " already received");

                    //ResetCurrencyRate(itemDetail);

                    if (itemDetail.IsNotNull())
                    {


                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);



                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {

                            ///Added Date 22-10-19
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {

                            //added date 22-10-2019
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            //itemDetail.TotalMaterialTranAmount = itemDetail.TransactionAmount;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                            && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {

                            //AddedDate
                            itemDetail.BaseUoMFactor = 1;
                            itemDetail.BaseQty = itemDetail.TransactionQty;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                            //itemDetail.ChargesTranAmount = itemDetail.MaterialTranAmount * ratio;
                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                            //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                            //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
                        }
                        else
                        {

                            //itemDetail.BaseUoMFactor = itemDetail.TransactionQty;
                            itemDetail.BaseUoMFactor = 1;
                            itemDetail.BaseQty = itemDetail.TransactionQty;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);

                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);

                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                        }



                        // Insert in receive detail
                        if (!string.IsNullOrEmpty(itemDetail.Id))
                        {
                            //currentId1++;
                            var receiveDetail = new InventoryReceiveDetail
                            {

                                Id = itemDetail.InventoryReceiveDetailId,
                                MaterialStorageId = itemDetail.MaterialStorageId,
                                InventoryReceiveId = itemDetail.InventoryReceiveId,
                                InventoryMaterialId = itemDetail.InventoryMaterialId,
                                TransactionQty = itemDetail.TransactionQty,
                                TransactionUoMId = itemDetail.TransactionUoMId,
                                BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                BaseUOMId = itemDetail.BaseUOMId,
                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                MaterialTranRate = Convert.ToDecimal(itemDetail.TransactionRate),
                                MaterialTranAmount = Convert.ToDecimal(itemDetail.TrnAmount),
                                TotalMaterialTranAmount = Convert.ToDecimal(itemDetail.TotalMaterialTranAmount),
                                TotalMaterialBooksCurrencyAmount = Convert.ToDecimal(itemDetail.BaseAmount),
                                TotalTaxAmount = Convert.ToDecimal(itemDetail.TaxAmount),

                                IssueQty = null,
                                BaseIssueQty = Convert.ToDecimal(itemDetail.BaseIssueQty),

                                ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),

                                ShortageRatePercent = Convert.ToDecimal(itemDetail.ShortageRate),
                                ShortageValue = Convert.ToDecimal(itemDetail.ShortageValue),
                                RejectRatePercent = Convert.ToDecimal(itemDetail.RejectionRate),
                                RejectValue = Convert.ToDecimal(itemDetail.RejectionValue),
                                RejectClamPercent = Convert.ToDecimal(itemDetail.RejectionClamRate),
                                Description = itemDetail.Description,
                                ShortRejFlag = true,
                                TrnCurrencyBaseRate = Convert.ToDecimal(itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty),
                                BooksCurrencyBaseRate = Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty),
                                ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2), //itemDetail.TrnAmount * ratio;
                                ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTaxTranAmount), 2),//itemDetail.TrnAmount * ratioServiceTax;

                            };
                            try
                            {

                                itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                                AuditService.UpdatedLog(receiveDetail);
                                var NewId = receiveDetail.InventoryReceiveId + "-";
                                UpdateGraph(receiveDetail);

                                currentId++;
                                grndId = NewId + currentId;


                                var Val = _gRNRejectionDetailsRepository.Query(r => r.GRNDeailsId == receiveDetail.Id).Select().FirstOrDefault();
                                if (Val == null)
                                {
                                    int rejectDetailId = 1;
                                    var RejectionDetails = new GRNRejectionDetails
                                    {
                                        Id = grndId.ToString() + rejectDetailId,
                                        GRNDeailsId = receiveDetail.Id,
                                        RejectionQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                        RejectionUoMId = itemDetail.TransactionUoMId,
                                        BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                        BaseUOMId = itemDetail.BaseUOMId,
                                        RejectionRate = Convert.ToDecimal(itemDetail.RejectionRate),
                                        RejeactionValue = Convert.ToDecimal(itemDetail.RejectionValue),
                                    };
                                    AuditService.AddedLog(RejectionDetails);
                                    _gRNRejectionDetailsRepository.Insert(RejectionDetails);
                                }
                                else
                                {
                                    var RejectionDetails = new GRNRejectionDetails
                                    {
                                        Id = Val.Id,
                                        GRNDeailsId = Val.GRNDeailsId,
                                        RejectionQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                        RejectionUoMId = itemDetail.TransactionUoMId,
                                        BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                        BaseUOMId = itemDetail.BaseUOMId,
                                        RejectionRate = Convert.ToDecimal(itemDetail.RejectionRate),
                                        RejeactionValue = Convert.ToDecimal(itemDetail.RejectionValue),
                                    };
                                    AuditService.AddedLog(RejectionDetails);
                                    _gRNRejectionDetailsRepository.Update(RejectionDetails);
                                }



                                //UpdateInventoryDetail(receiveDetail, ratio, Convert.ToDecimal(itemDetail.ToCurrencyRate), itemDetail.IsNonCreditable);


                            }
                            catch (DivideByZeroException ex)
                            {

                            }
                            finally
                            {

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

        public void InsertOrUpdateGraph(InventoryMaterialViewModel itemDetail, IEnumerable<InventoryReceiveTax> taxCategoryList, IEnumerable<GRNBinAllocationMap> gRNBinAllocationMapList)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster=null;
                itemDetail.CompanyGroupId = identity.CompanyGroupId;
                itemDetail.CompanyId = identity.CompanyId;
                itemDetail.PlantId = identity.PlantId;
                if (CheckItemExist(itemDetail))
                    throw new CustomException(itemDetail.MaterialMasterName + " already received");

                ResetCurrencyRate(itemDetail);
                _unitOfWork.BeginTransaction();
                flag = true;
                var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{itemDetail.InventoryReceiveId}'").First();
                if (itemDetail.IsNotNull())
                {
                    var ratio = _inventoryReceiveService.GetChargesRatio(itemDetail.InventoryReceiveId, itemDetail.Id, Convert.ToDecimal(itemDetail.TransactionAmount), null, 0, itemDetail.IsNonCreditable);
                    var ratioServiceTax = _inventoryReceiveService.GetChargesTaxRatio(itemDetail.InventoryReceiveId, itemDetail.Id, Convert.ToDecimal(itemDetail.TransactionAmount), null, 0, itemDetail.IsNonCreditable);

                    var otherchargesratio = _inventoryReceiveService.GetChargesRatio(itemDetail.InventoryReceiveId, itemDetail.Id, Convert.ToDecimal(itemDetail.TransactionAmount), null, 0, itemDetail.IsNonCreditable);
                    var otherchargesratioServiceTax = _inventoryReceiveService.GetChargesTaxRatio(itemDetail.InventoryReceiveId, itemDetail.Id, Convert.ToDecimal(itemDetail.TransactionAmount), null, 0, itemDetail.IsNonCreditable);

                    var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                    if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                    ///TODO : Get total qyt and amount by country and issue qty
                    itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                    itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());


                    itemDetail.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.PurchaseReturnQty).Sum());
                    itemDetail.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueReturnQty).Sum());
                    itemDetail.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                    itemDetail.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventorySalesQty).Sum());
                    itemDetail.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryScrapQty).Sum());
                    itemDetail.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryTransferQty).Sum());





                    var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialBooksCurrencyAmount).Sum();

                    var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                    var altUomIds = new string[] { itemDetail.TransactionUoMId };
                    var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                    if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                    {
                        itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                        itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                        itemDetail.TotalMaterialTranAmount = (itemDetail.TransactionRate * itemDetail.TransactionQty); //itemDetail.TransactionAmount; 
                        itemDetail.ChargesTranAmount = (itemDetail.TransactionRate * itemDetail.NetQty) * ratio;//itemDetail.TransactionAmount * ratio;
                        itemDetail.ChargesTaxTranAmount = (itemDetail.TransactionRate * itemDetail.NetQty) * ratioServiceTax;//itemDetail.TransactionAmount * ratioServiceTax;
                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                        itemDetail.TotalMaterialBooksCurrencyAmount = (itemDetail.TransactionRate * itemDetail.NetQty) * itemDetail.ToCurrencyRate; //itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;

                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                        //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                        //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
                    }
                    else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                    {
                        itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                        itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                        //itemDetail.TotalMaterialTranAmount = itemDetail.TransactionAmount;
                        itemDetail.TotalMaterialTranAmount = (itemDetail.TransactionRate * itemDetail.TransactionQty); //itemDetail.TransactionAmount;
                        itemDetail.ChargesTranAmount = (itemDetail.TransactionRate * itemDetail.NetQty) * ratio; //itemDetail.TransactionAmount * ratio;
                        itemDetail.ChargesTaxTranAmount = (itemDetail.TransactionRate * itemDetail.NetQty) * ratioServiceTax;//itemDetail.TransactionAmount * ratioServiceTax;
                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                        itemDetail.TotalMaterialBooksCurrencyAmount = (itemDetail.TransactionRate * itemDetail.TransactionQty) * itemDetail.ToCurrencyRate; //itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;

                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                        //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                        //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
                    }
                    else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                    {

                        itemDetail.BaseUoMFactor = 1;
                        itemDetail.BaseQty = itemDetail.TransactionQty;
                        itemDetail.TotalMaterialTranAmount = (itemDetail.TransactionRate * itemDetail.TransactionQty);//itemDetail.TransactionAmount;
                        itemDetail.ChargesTranAmount = (itemDetail.TransactionRate * itemDetail.NetQty) * ratio; //itemDetail.TransactionAmount * ratio;
                        itemDetail.ChargesTaxTranAmount = (itemDetail.TransactionRate * itemDetail.NetQty) * ratioServiceTax; //itemDetail.TransactionAmount * ratioServiceTax;
                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                        itemDetail.TotalMaterialBooksCurrencyAmount = (itemDetail.TransactionRate * itemDetail.TransactionQty) * itemDetail.ToCurrencyRate; //itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;
                                                                                                                                                    //itemDetail.ChargesTranAmount = itemDetail.MaterialTranAmount * ratio;
                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                        //entity.TotalQty = Convert.ToDecimal(entity.TotalQty + entity.BaseQty);
                        //entity.AvgRate = Convert.ToDecimal((totalAmount + entity.TotalMaterialTranAmount) / entity.TotalQty);
                    }

                    else
                    {
                        //itemDetail.BaseUoMFactor = itemDetail.TransactionQty;
                        itemDetail.BaseUoMFactor = 1;
                        itemDetail.BaseQty = itemDetail.TransactionQty;
                        itemDetail.TotalMaterialTranAmount = (itemDetail.TransactionRate * itemDetail.TransactionQty);//itemDetail.TransactionAmount;
                        itemDetail.ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate * itemDetail.NetQty) * ratio, 2); //itemDetail.TransactionAmount * ratio;
                        itemDetail.ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate * itemDetail.NetQty) * ratioServiceTax, 2);//itemDetail.TransactionAmount * ratioServiceTax;
                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                          Convert.ToDecimal(itemDetail.ChargesTranAmount);

                        itemDetail.TotalMaterialBooksCurrencyAmount = (itemDetail.TransactionRate * itemDetail.TransactionQty) * itemDetail.ToCurrencyRate;//itemDetail.TransactionAmount * itemDetail.ToCurrencyRate;

                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);

                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                    }
                    // Insert in receive detail
                    if (string.IsNullOrEmpty(itemDetail.Id))
                    {
                        if (taxCategoryList.IsNotNull())
                        {
                            itemDetail.TotalTaxAmount = taxCategoryList.Sum(r => r.TaxAmount);
                        }
                        //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId='{itemDetail.InventoryReceiveId}'").First();
                        var NewId = itemDetail.InventoryReceiveId + "-";
                        currentId++;

                        //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[MaterialRequsitionDetails] WHERE //MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();
                        //currentId++;						
                        //grndId = NewId + currentId;
                        var receiveDetail = new InventoryReceiveDetail
                        {
                            Id = NewId + currentId,//MakePK(itemDetail.InventoryReceiveId + 1, currentId, 2),
                            MaterialStorageId = itemDetail.MaterialStorageId,
                            InventoryReceiveId = itemDetail.InventoryReceiveId,
                            //InventoryMaterialId = entity.InventoryMaterialId,
                            TransactionQty = itemDetail.TransactionQty,//itemDetail.TransactionQty,
                            TransactionUoMId = itemDetail.TransactionUoMId,
                            BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                            BaseUOMId = itemDetail.BaseUOMId,
                            BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                            MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                            MaterialTranAmount = Math.Round((Convert.ToDecimal(itemDetail.TransactionRate) * itemDetail.TransactionQty), 2),//Convert.ToDecimal(itemDetail.TransactionAmount),

                            TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                            TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                            TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalTaxAmount), 2),

                            IssueQty = null,
                            BaseIssueQty = 0,
                            Description = itemDetail.Description,
                            ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                            ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTaxTranAmount), 2),
                            TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                            BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 4),
                            PurchaseReturnQty = 0,
                            IssueReturnQty = 0,
                            ReductionByAdjustmentQty = 0,
                            InventorySalesQty = 0,
                            InventoryScrapQty = 0,
                            InventoryTransferQty = 0,
                            MaterialMasterOpeningBalanceDetailId = null,
                            LotNumber = itemDetail.LotNumber,
                            Diameter = itemDetail.Diameter,
                            Type = itemDetail.Type,
                            ShortageQty = itemDetail.ShortageQty,
                            RejectionQty = itemDetail.RejectionQty,
                            ApprovedQty = itemDetail.ApprovedQty,
                            ShortageRatePercent = 100,
                            RejectRatePercent = 50,
                            GRNQty = itemDetail.TransactionQty,
                            GRNTotalAmount = itemDetail.TransactionAmount,
                            IsAsset = itemDetail.IsAsset,
                            LotNo = itemDetail.LotNo,
                            QualityStatus = itemDetail.QualityStatus,
                            GrossAmount = itemDetail.GrossAmount,
                            DiscountAmount = itemDetail.DiscountAmount,
                            MasterOrderItemId = itemDetail.MasterOrderItemId
                        };
                        itemDetail.InventoryReceiveDetailId = receiveDetail.Id;

                        receiveDetail.ShortageValue = Math.Round(Convert.ToDecimal(itemDetail.ShortageQty * itemDetail.TrnCurrencyBaseRate), 2);//Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                        receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                        receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);

                        AuditService.AddedLog(receiveDetail);


                        itemDetail.TotalQty = ((Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty + itemDetail.IssueReturnQty)) - (Convert.ToDecimal(itemDetail.IssueQty) + Convert.ToDecimal(itemDetail.PurchaseReturnQty) + Convert.ToDecimal(itemDetail.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetail.InventorySalesQty) + Convert.ToDecimal(itemDetail.InventoryScrapQty) + Convert.ToDecimal(itemDetail.InventoryTransferQty)));
                        itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialBooksCurrencyAmount) / itemDetail.TotalQty);//TotalMaterialTranAmount

                        _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                        receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                        InsertGraph(receiveDetail);
                        UpdateInventoryDetail(receiveDetail, ratio, ratioServiceTax, Convert.ToDecimal(itemDetail.ToCurrencyRate), itemDetail.IsNonCreditable);
                        updateArticleMinMaxValue( itemDetail.MinimumValue,  itemDetail.MaximumValue, Convert.ToDecimal(itemDetail.TotalMaterialTranAmount),  itemDetail.ArticleId);


                    }

                    // insert in receive tax
                    if (taxCategoryList.IsNotNull())
                    {
                        currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            currentId++;
                            item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                            item.InventoryReceiveId = itemDetail.InventoryReceiveId;
                            item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                            item.InventoryServiceId = null;
                            item.TaxAmount = Math.Round(item.TaxAmount, 2);
                            AuditService.AddedLog(item);
                            _receiveTaxRepository.Insert(item);
                        }
                    }
                    
                    

                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
               
                if (gRNBinAllocationMapList.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                    con.getDataSet("Select * from TRN.GRNBinAllocationMap where 1=2", out dsMaster);
                    currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[GRNBinAllocationMap] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                    
                    foreach (var item in gRNBinAllocationMapList)
                    {
                        currentId++;
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item.Id + "'";
                       
                            if (dv.Count == 0)
                            {
                                item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                            item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                            AuditService.AddedLog(item);
                            AddNewRow(dsMaster.Tables[0], item);
                            }
                            
                    }
                    //foreach (var item in gRNBinAllocationMapList)
                    //{

                    //    item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                    //    item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                    //    AuditService.AddedLog(item);
                    //    AddNewRow<GRNBinAllocationMap>(vDetailData.Tables[0], gRNBinAllocationMap);
                    //    InsertGRNBinAllocationMap(item, ref dsMaster);
                    //    
                    //    
                    //}
                    clsStaticInfo objApp = new clsStaticInfo();
                    objApp.SaveDataSets(dsMaster);
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
        public void updateArticleMinMaxValue(string minvalue,string maxvalue,decimal trnAmount,string articleId)
        {
            if (Convert.ToDecimal(maxvalue) < trnAmount)
            {
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = @"UPDATE MST.MaterialMasterArticle SET MaximumValue=" + trnAmount + " WHERE Id=" + articleId + "";
                rdBuilder.Append(builderSql);
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
            }
            else if (Convert.ToDecimal(minvalue) < trnAmount)
            {
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = @"UPDATE MST.MaterialMasterArticle SET MinimumValue=" + trnAmount + " WHERE Id=" + articleId + "";
                rdBuilder.Append(builderSql);
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
            }

        }
        private GRNBinAllocationMap InsertGRNBinAllocationMap( GRNBinAllocationMap gRNBinAllocationMap, ref DataSet vDetailData)
        {

            
            
            return gRNBinAllocationMap;
        }
        public void AddNewRow<T>(DataTable dt, T Data)
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
        public void InsertFOCMaterial(InventoryMaterialViewModel itemDetail, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            var flag = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                itemDetail.CompanyGroupId = identity.CompanyGroupId;
                itemDetail.CompanyId = identity.CompanyId;
                itemDetail.PlantId = identity.PlantId;
                if (CheckItemExist(itemDetail))
                    throw new CustomException(itemDetail.MaterialMasterName + " already received");

                ResetCurrencyRate(itemDetail);
                _unitOfWork.BeginTransaction();
                flag = true;
                var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{itemDetail.InventoryReceiveId}'").First();
                if (itemDetail.IsNotNull())
                {
                    var ratio = _inventoryReceiveService.GetChargesRatio(itemDetail.InventoryReceiveId, itemDetail.Id, Convert.ToDecimal(itemDetail.TransactionAmount), null, 0, itemDetail.IsNonCreditable);
                    var ratioServiceTax = _inventoryReceiveService.GetChargesTaxRatio(itemDetail.InventoryReceiveId, itemDetail.Id, Convert.ToDecimal(itemDetail.TransactionAmount), null, 0, itemDetail.IsNonCreditable);

                    var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                    if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                    ///TODO : Get total qyt and amount by country and issue qty
                    itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                    itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());


                    itemDetail.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.PurchaseReturnQty).Sum());
                    itemDetail.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueReturnQty).Sum());
                    itemDetail.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                    itemDetail.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventorySalesQty).Sum());
                    itemDetail.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryScrapQty).Sum());
                    itemDetail.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryTransferQty).Sum());





                    var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialBooksCurrencyAmount).Sum();

                    var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                    var altUomIds = new string[] { itemDetail.TransactionUoMId };
                    var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                    if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                    {
                        itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                        itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);
                        itemDetail.TotalMaterialTranAmount = 0;//itemDetail.TransactionAmount;
                        itemDetail.ChargesTranAmount = 0; //itemDetail.TransactionAmount * ratio;
                        itemDetail.ChargesTaxTranAmount = 0;//itemDetail.TransactionAmount * ratioServiceTax;
                        itemDetail.TotalMaterialTranAmount += 0;
                        itemDetail.TotalMaterialBooksCurrencyAmount = 0;
                        itemDetail.TotalMaterialBooksCurrencyAmount += 0;
                        itemDetail.TrnCurrencyBaseRate = 0;
                        itemDetail.BooksCurrencyBaseRate = 0;
                    }
                    else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                    {
                        itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                        itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);
                        itemDetail.TotalMaterialTranAmount = 0;//itemDetail.TransactionAmount;
                        itemDetail.ChargesTranAmount = 0; //itemDetail.TransactionAmount * ratio;
                        itemDetail.ChargesTaxTranAmount = 0;//itemDetail.TransactionAmount * ratioServiceTax;
                        itemDetail.TotalMaterialTranAmount += 0;
                        itemDetail.TotalMaterialBooksCurrencyAmount = 0;
                        itemDetail.TotalMaterialBooksCurrencyAmount += 0;
                        itemDetail.TrnCurrencyBaseRate = 0;
                        itemDetail.BooksCurrencyBaseRate = 0;
                    }


                    else
                    {
                        //itemDetail.BaseUoMFactor = itemDetail.TransactionQty;
                        itemDetail.BaseUoMFactor = 1;
                        itemDetail.BaseQty = itemDetail.NetQty;
                        itemDetail.TotalMaterialTranAmount = 0;//itemDetail.TransactionAmount;
                        itemDetail.ChargesTranAmount = 0; //itemDetail.TransactionAmount * ratio;
                        itemDetail.ChargesTaxTranAmount = 0;//itemDetail.TransactionAmount * ratioServiceTax;
                        itemDetail.TotalMaterialTranAmount += 0;
                        itemDetail.TotalMaterialBooksCurrencyAmount = 0;
                        itemDetail.TotalMaterialBooksCurrencyAmount += 0;
                        itemDetail.TrnCurrencyBaseRate = 0;
                        itemDetail.BooksCurrencyBaseRate = 0;

                    }
                    // Insert in receive detail
                    if (string.IsNullOrEmpty(itemDetail.Id))
                    {
                        if (taxCategoryList.IsNotNull())
                        {
                            itemDetail.TotalTaxAmount = taxCategoryList.Sum(r => r.TaxAmount);
                        }
                        //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail] WHERE InventoryReceiveId='{itemDetail.InventoryReceiveId}'").First();
                        var NewId = itemDetail.InventoryReceiveId + "-";
                        currentId++;

                        //var currentId = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 1) AS INT)), 0) Id FROM [TRN].[MaterialRequsitionDetails] WHERE //MaterialReqqusitionMasterId='{entity.MaterialReqqusitionMasterId}'").First();
                        //currentId++;						
                        //grndId = NewId + currentId;
                        var receiveDetail = new InventoryReceiveDetail
                        {
                            Id = NewId + currentId,//MakePK(itemDetail.InventoryReceiveId + 1, currentId, 2),
                            MaterialStorageId = itemDetail.MaterialStorageId,
                            InventoryReceiveId = itemDetail.InventoryReceiveId,
                            //InventoryMaterialId = entity.InventoryMaterialId,
                            TransactionQty = itemDetail.NetQty,//itemDetail.TransactionQty,
                            TransactionUoMId = itemDetail.TransactionUoMId,
                            BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                            BaseUOMId = itemDetail.BaseUOMId,
                            BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                            MaterialTranRate = 0,
                            MaterialTranAmount = 0,//Convert.ToDecimal(itemDetail.TransactionAmount),

                            TotalMaterialTranAmount = 0,
                            TotalMaterialBooksCurrencyAmount = 0,
                            TotalTaxAmount = 0,

                            IssueQty = null,
                            BaseIssueQty = 0,
                            Description = itemDetail.Description,
                            ChargesTranAmount = 0,
                            ChargesTaxTranAmount = 0,
                            TrnCurrencyBaseRate = 0,
                            BooksCurrencyBaseRate = 0,
                            PurchaseReturnQty = 0,
                            IssueReturnQty = 0,
                            ReductionByAdjustmentQty = 0,
                            InventorySalesQty = 0,
                            InventoryScrapQty = 0,
                            InventoryTransferQty = 0,
                            MaterialMasterOpeningBalanceDetailId = null,
                            LotNumber = itemDetail.LotNumber,
                            Diameter = itemDetail.Diameter,
                            Type = itemDetail.Type,
                            ShortageQty = itemDetail.ShortageQty,
                            RejectionQty = itemDetail.RejectionQty,
                            ApprovedQty = itemDetail.ApprovedQty,
                            ShortageRatePercent = 110,
                            RejectRatePercent = 50,
                            GRNQty = itemDetail.TransactionQty,
                            GRNTotalAmount = itemDetail.TransactionAmount,
                            IsAsset = itemDetail.IsAsset,
                            LotNo = itemDetail.LotNo,
                            QualityStatus = itemDetail.QualityStatus,
                            GrossAmount = itemDetail.GrossAmount,
                            DiscountAmount = itemDetail.DiscountAmount,
                            MasterOrderItemId = itemDetail.MasterOrderItemId
                        };
                        itemDetail.InventoryReceiveDetailId = receiveDetail.Id;

                        receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                        receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                        receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);

                        AuditService.AddedLog(receiveDetail);

                        itemDetail.TotalQty = ((Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty + itemDetail.IssueReturnQty)) - (Convert.ToDecimal(itemDetail.IssueQty) + Convert.ToDecimal(itemDetail.PurchaseReturnQty) + Convert.ToDecimal(itemDetail.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetail.InventorySalesQty) + Convert.ToDecimal(itemDetail.InventoryScrapQty) + Convert.ToDecimal(itemDetail.InventoryTransferQty)));
                        itemDetail.AvgRate = 0;//TotalMaterialTranAmount

                        _inventoryMaterialMasterService.InsertOrUpdateFromReceive(itemDetail);
                        receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                        InsertGraph(receiveDetail);
                        UpdateFOCInventoryDetail(receiveDetail, ratio, ratioServiceTax, Convert.ToDecimal(itemDetail.ToCurrencyRate), itemDetail.IsNonCreditable);
                    }

                    // insert in receive tax
                    if (taxCategoryList.IsNotNull())
                    {
                        currentId = _receiveTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId='{itemDetail.InventoryReceiveDetailId}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            currentId++;
                            item.Id = MakePK(itemDetail.InventoryReceiveDetailId, currentId, 2);
                            item.InventoryReceiveId = itemDetail.InventoryReceiveId;
                            item.InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId;
                            item.InventoryServiceId = null;
                            item.TaxAmount = Math.Round(item.TaxAmount, 2);
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
        private void UpdateInventoryDetail(InventoryReceiveDetail detail, decimal ratio, decimal ratioServiceTax, decimal currencyRate, bool isNonCreditable)
        {
            var detailList = Query(t => t.InventoryReceiveId == detail.InventoryReceiveId).Select().ToList();// && t.Id != detail.Id
            if (detailList.IsNotNull())
            {
                int i = 0;
                decimal Tax1 = 0;
                decimal serviceCN1 = 0;
                int detailCount = detailList.Count;
                decimal tempserAmt = 0;
                decimal tempserTax = 0;
                var serviceex = _inventoryServiceRepository.Query(r => r.InventoryReceiveId == detail.InventoryReceiveId).Select().ToList();
                if (serviceex != null)
                {
                    Tax1 = serviceex.Sum(r => r.TotalTaxAmount);
                    serviceCN1 = serviceex.Sum(r => r.Amount);
                }
                foreach (var item in detailList)
                {
                    i++;

                    if (serviceex != null)
                    {
                        if (detailCount > i)
                        {
                            item.ChargesTaxTranAmount = Math.Round(item.MaterialTranAmount * ratioServiceTax, 2);
                            item.ChargesTranAmount = Math.Round(item.MaterialTranAmount * ratio, 2);
                            tempserAmt += item.ChargesTranAmount;
                            tempserTax += item.ChargesTaxTranAmount;
                        }
                        else if (detailCount == i)
                        {
                            item.ChargesTranAmount = Math.Round(serviceCN1 - (tempserAmt + detail.ChargesTranAmount), 2);
                            item.ChargesTaxTranAmount = Math.Round(Tax1 - (tempserTax + detail.ChargesTaxTranAmount), 2);

                        }
                    }

                    item.TotalMaterialTranAmount = isNonCreditable ? Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.TotalTaxAmount + item.ChargesTranAmount + item.ChargesTaxTranAmount), 2) :
                           Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.ChargesTranAmount), 2);
                    item.TotalMaterialBooksCurrencyAmount = Math.Round(item.MaterialTranAmount * currencyRate, 2);
                    item.TotalMaterialBooksCurrencyAmount = isNonCreditable ? Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.TotalTaxAmount + item.ChargesTranAmount + item.ChargesTaxTranAmount) * Convert.ToDecimal(currencyRate), 2) :
                             Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.ChargesTranAmount) * Convert.ToDecimal(currencyRate), 2);
                    item.TrnCurrencyBaseRate = Math.Round((item.TotalMaterialTranAmount / item.BaseQty), 4);
                    item.BooksCurrencyBaseRate = Math.Round((item.TotalMaterialBooksCurrencyAmount / item.BaseQty), 4);

                    item.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(item);
                    UpdateGraph(item);
                }
            }
        }
        private void UpdateInventoryDetailAfterDelete(InventoryReceiveDetail detail, decimal ratioServiceTax, decimal ratio, decimal currencyRate, bool isNonCreditable)
        {
            var detailList = Query(t => t.InventoryReceiveId == detail.InventoryReceiveId && t.Id != detail.Id).Select().ToList();// && t.Id != detail.Id
            if (detailList.IsNotNull())
            {
                int i = 0;
                decimal Tax1 = 0;
                decimal serviceCN1 = 0;
                int detailCount = detailList.Count;
                decimal tempserAmt = 0;
                decimal tempserTax = 0;
                var serviceex = _inventoryServiceRepository.Query(r => r.InventoryReceiveId == detail.InventoryReceiveId).Select().ToList();
                if (serviceex != null)
                {
                    Tax1 = serviceex.Sum(r => r.TotalTaxAmount);
                    serviceCN1 = serviceex.Sum(r => r.Amount);
                }
                foreach (var item in detailList)
                {
                    i++;

                    if (serviceex != null)
                    {
                        if (detailCount > i)
                        {
                            item.ChargesTaxTranAmount = Math.Round(item.MaterialTranAmount * ratioServiceTax, 2);
                            item.ChargesTranAmount = Math.Round(item.MaterialTranAmount * ratio, 2);
                            tempserAmt += item.ChargesTranAmount;
                            tempserTax += item.ChargesTaxTranAmount;
                        }
                        else if (detailCount == i)
                        {
                            item.ChargesTranAmount = Math.Round(serviceCN1 - (tempserAmt), 2);
                            item.ChargesTaxTranAmount = Math.Round(Tax1 - (tempserTax), 2);

                        }
                    }

                    item.TotalMaterialTranAmount = isNonCreditable ? Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.TotalTaxAmount + item.ChargesTranAmount + item.ChargesTaxTranAmount), 2) :
                          Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.ChargesTranAmount), 2);
                    //item.TotalMaterialBooksCurrencyAmount = item.MaterialTranAmount * currencyRate;
                    item.TotalMaterialBooksCurrencyAmount = isNonCreditable ? Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.TotalTaxAmount + item.ChargesTranAmount + item.ChargesTaxTranAmount) * Convert.ToDecimal(currencyRate), 2) :
                             Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.ChargesTranAmount) * Convert.ToDecimal(currencyRate), 2);

                    item.TrnCurrencyBaseRate = Math.Round(item.TotalMaterialTranAmount / item.BaseQty, 4);
                    item.BooksCurrencyBaseRate = Math.Round(item.TotalMaterialBooksCurrencyAmount / item.BaseQty, 4);
                    item.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(item);
                    UpdateGraph(item);
                }
            }
        }
        public void Delete(string receiveDetailId)
        {
            var flag = false;
            try
            {
                var isNonCreditable = _receiveDetailRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[InventoryReceive] AS A JOIN [TRN].[InventoryReceiveDetail] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + receiveDetailId + "'").First();
                var data = Find(receiveDetailId);
                if (data.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _inventoryMaterialMasterService.UpdateFromReceive(data.InventoryMaterialId, receiveDetailId);
                    var taxCategoryList = _receiveTaxRepository.Query(t => t.InventoryReceiveDetailId == receiveDetailId).Select().ToList();
                    if (taxCategoryList.IsNotNull())
                    {
                        foreach (var item in taxCategoryList)
                        {
                            item.ModelState = ModelState.Deleted;
                            _receiveTaxRepository.Delete(item);
                        }
                    }
                    var ratio = _inventoryReceiveService.GetChargesRatio(data.InventoryReceiveId, data.Id, 0, null, 0, isNonCreditable);
                    var ratioServiceTax = _inventoryReceiveService.GetChargesTaxRatio(data.InventoryReceiveId, data.Id, 0, null, 0, isNonCreditable);
                    UpdateInventoryDetailAfterDelete(data, ratioServiceTax, ratio, 1, isNonCreditable);

                    var PODetailData = _poDetailRepository.Find(data.PODetailsID);
                    if (PODetailData.IsNotNull())
                    {
                        PODetailData.GRNRcvQty = Convert.ToDecimal(((PODetailData.GRNRcvQty - data.GRNQty)));
                        PODetailData.QtyStatus = PODetailData.TransactionQty == PODetailData.GRNRcvQty;
                        _poDetailRepository.Update(PODetailData);
                    }

                    var GRNPORequisitionAllocation = _gRNPOAllocationRepository.Query(t => t.InventoryReceiveDetailId == receiveDetailId).Select().ToList();
                    if (GRNPORequisitionAllocation.IsNotNull())
                    {
                        foreach (var item in GRNPORequisitionAllocation)
                        {
                            item.ModelState = ModelState.Deleted;
                            _gRNPOAllocationRepository.Delete(item);
                        }
                    }
                    base.DeleteGraph(data);

                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    DataSet dsMaster2 = null;
                    string setOffsql = @"SELECT * from trn.GRNPORequisitionMap where InventoryReceiveDetailId = '" + receiveDetailId + "'";
                    string grnBinAllocationMapsql = @"SELECT * from trn.GRNBinAllocationMap where InventoryReceiveDetailId = '" + receiveDetailId + "'";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");
                    objCon1.OpenDataSetThroughAdapter(grnBinAllocationMapsql, out dsMaster2, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var grnPOreqSql = @"DELETE trn.GRNPORequisitionMap where InventoryReceiveDetailId ='" + receiveDetailId + "'";
                        rdBuilder.Append(grnPOreqSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }

                    if (dsMaster2.Tables[0].Rows.Count > 0)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var grnBinAllocationSql = @"DELETE trn.GRNBinAllocationMap where InventoryReceiveDetailId ='" + receiveDetailId + "'";
                        rdBuilder.Append(grnBinAllocationSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }

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

        private void UpdateFOCInventoryDetail(InventoryReceiveDetail detail, decimal ratio, decimal ratioServiceTax, decimal currencyRate, bool isNonCreditable)
        {
            var detailList = Query(t => t.InventoryReceiveId == detail.InventoryReceiveId).Select().ToList();// && t.Id != detail.Id
            if (detailList.IsNotNull())
            {
                int i = 0;
                decimal Tax1 = 0;
                decimal serviceCN1 = 0;
                int detailCount = detailList.Count;
                decimal tempserAmt = 0;
                decimal tempserTax = 0;
                var serviceex = _inventoryServiceRepository.Query(r => r.InventoryReceiveId == detail.InventoryReceiveId).Select().ToList();
                if (serviceex != null)
                {
                    Tax1 = serviceex.Sum(r => r.TotalTaxAmount);
                    serviceCN1 = serviceex.Sum(r => r.Amount);
                }
                foreach (var item in detailList)
                {
                    i++;

                    if (serviceex != null)
                    {
                        if (detailCount > i)
                        {
                            item.ChargesTaxTranAmount = Math.Round(item.MaterialTranAmount * ratioServiceTax, 2);
                            item.ChargesTranAmount = Math.Round(item.MaterialTranAmount * ratio, 2);
                            tempserAmt += item.ChargesTranAmount;
                            tempserTax += item.ChargesTaxTranAmount;
                        }
                        else if (detailCount == i)
                        {
                            item.ChargesTranAmount = Math.Round(serviceCN1 - (tempserAmt + detail.ChargesTranAmount), 2);
                            item.ChargesTaxTranAmount = Math.Round(Tax1 - (tempserTax + detail.ChargesTaxTranAmount), 2);

                        }
                    }

                    item.TotalMaterialTranAmount = 0;
                    item.TotalMaterialBooksCurrencyAmount = 0;
                    item.TrnCurrencyBaseRate = 0;
                    item.BooksCurrencyBaseRate = 0;

                    item.ModelState = ModelState.Modified;
                    AuditService.UpdatedLog(item);
                    UpdateGraph(item);
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
                                AND ISNULL(IM.CountryId,'')='" + entity.CountryId + @"'
                                AND ISNULL(IRD.LotNumber,'')='" + entity.LotNumber + @"'
                                AND ISNULL(IRD.Diameter,'')='" + entity.Diameter + @"'
								AND ISNULL(IRD.Type,'')='" + entity.Type + @"'
                            ) AS TBL ) SELECT 1 ELSE SELECT 0 RETURN";
                var d = Convert.ToBoolean(_receiveDetailRepository.SqlQuery<int>(sql).First());
                return d;
            }
            catch
            {
                throw;
            }
        }


        #region Purchase Return
        public void InsertOrUpdateGraphForPurchaseReturn(PurchaseReturn entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<PurchaseReturnTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, IEnumerable<InventoryMaterialViewModel> chargesList, IEnumerable<PurchaseReturnTax> ServicetaxCategoryList)
        {
            var flag = false;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var GRNCalculateList = new List<InventoryIssueHistory>();
            try
            {
                _unitOfWork.BeginTransaction();
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";
                flag = true;
                entity.GRNType = GRNType;
                if (entity.GRNType == "Save")
                {
                    entity.Id = "";
                }
                else
                {

                }

                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPurchaseReturnPK();
                    AuditService.AddedLog(entity);
                    _PurchaseReturnRepository.Insert(entity);
                }
                else
                {
                    AuditService.UpdatedLog(entity);
                    _PurchaseReturnRepository.Update(entity);
                }

                var currentId1 = _PurchaseReturnDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[PurchaseReturnDetail]  WHERE PurchaseReturnId ='{entity.Id}'").First();
                var Temppodetailid = "";

                var grndId = "";
                foreach (var itemDetail in entityMat)
                {
                    itemDetail.Id = null;
                    itemDetail.CompanyGroupId = identity.CompanyGroupId;
                    itemDetail.CompanyId = identity.CompanyId;
                    itemDetail.PlantId = identity.PlantId;
                    Temppodetailid = itemDetail.InventoryReceiveDetailId;
                    itemDetail.IsNonCreditable = entity.IsNonCreditable;

                    if (itemDetail.IsNotNull())
                    {


                        var materialData = _inventoryMaterialMasterService.GetInventoryMaterialByUpToSku(itemDetail);
                        if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                        ///TODO : Get total qyt and amount by country and issue qty
                        itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                        itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());
                        var ShortageQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ShortageQty).Sum();
                        var RejectionQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.RejectionQty).Sum();
                        var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ApprovedQty).Sum();


                        var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                        var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                        var altUomIds = new string[] { itemDetail.TransactionUoMId };
                        var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                        if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                             && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {

                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            if (itemDetail.TotalTaxAmount == null)
                                itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                        }
                        else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {


                            //added date 22-10-2019
                            itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                            itemDetail.BaseQty = Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                            //itemDetail.TotalMaterialTranAmount = itemDetail.TransactionAmount;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            if (itemDetail.TotalTaxAmount == null)
                                itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;


                        }
                        else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                            && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                        {

                            //AddedDate
                            itemDetail.BaseUoMFactor = 1;
                            itemDetail.BaseQty = itemDetail.TransactionQty;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            if (itemDetail.TotalTaxAmount == null)
                                itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);
                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                            //itemDetail.ChargesTranAmount = itemDetail.MaterialTranAmount * ratio;
                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;


                        }
                        else
                        {

                            itemDetail.BaseUoMFactor = 1;
                            itemDetail.BaseQty = itemDetail.TransactionQty;
                            itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                            itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                            itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                            if (itemDetail.TotalTaxAmount == null)
                                itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;

                            itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                              Convert.ToDecimal(itemDetail.ChargesTranAmount);

                            itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                            itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                     Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);

                            itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                            itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                        }

                        var NewId = entity.Id + "-";
                        if (entity.GRNType == "Save")
                        {

                            currentId1++;
                            grndId = NewId + currentId1;
                            itemDetail.Id = "";
                        }
                        else
                        {
                            grndId = itemDetail.InventoryReceiveDetailId;
                            itemDetail.Id = grndId;
                            itemDetail.InventoryReceiveDetailId = itemDetail.InventoryServiceId;

                        }

                        var receiveDetail = new PurchaseReturnDetail
                        {
                            Id = grndId,
                            PurchaseReturnId = entity.Id,
                            MaterialMasterId = itemDetail.MaterialMasterId,
                            ArticleId = itemDetail.ArticleId,
                            FirstCharacteristicsId = itemDetail.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = itemDetail.FirstCharacteristicsValueId,
                            SecondCharacteristicsId = itemDetail.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = itemDetail.SecondCharacteristicsValueId,
                            ThirdCharacteristicsId = itemDetail.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = itemDetail.ThirdCharacteristicsValueId,
                            MaterialStorageId = itemDetail.MaterialStorageId,
                            TransactionQty = itemDetail.TransactionQty,
                            TransactionUoMId = itemDetail.TransactionUoMId,
                            BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                            BaseUOMId = itemDetail.BaseUOMId,
                            BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                            MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4), //Math.Round((Convert.ToDecimal(itemDetail.TotalMaterialTranAmount) / itemDetail.TransactionQty), 4),//Convert.ToDecimal(itemDetail.TransactionRate),
                            MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                            //IssueQty = itemDetail.IssueQty,
                            TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                            TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                            //TotalMaterialTranAmount = detailtrnAmount / totalGRNQty,
                            TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                            ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ServiceCharge), 2),
                            ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ServiceTax), 2),
                            TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                            BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 4),
                            CountryId = itemDetail.CountryId,
                            Description = itemDetail.Description,
                            IsAsset = false,
                            InventoryReceiveId = entity.InventoryReceiveId,
                            InventoryReceiveDetailId = itemDetail.InventoryReceiveDetailId,
                            InventoryMaterialId = itemDetail.InventoryMaterialId
                        };
                        try
                        {
                            if (string.IsNullOrEmpty(itemDetail.Id))
                            {
                                AuditService.AddedLog(receiveDetail);
                                _PurchaseReturnDetailRepository.Insert(receiveDetail);

                                var invMaterial = _PurchaseReturnDetailRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + itemDetail.InventoryMaterialId + "'").FirstOrDefault();
                                var invMaterialRcv = _PurchaseReturnDetailRepository.SqlQuery<InventoryReceiveDetail>(@"SELECT * FROM [TRN].[InventoryReceiveDetail] WHERE Id='" + itemDetail.InventoryReceiveDetailId + "'").FirstOrDefault();
                                builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET PurchaseReturnQty='" + Convert.ToDecimal(invMaterialRcv.PurchaseReturnQty + itemDetail.TransactionQty) + @"' WHERE Id = '" + itemDetail.InventoryReceiveDetailId + "'";

                                rdBuilder.Append(builderSql);

                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal(invMaterial.TotalQty - itemDetail.TransactionQty) + "' WHERE Id='" + itemDetail.InventoryMaterialId + "'";
                                rdBuilder.Append(builderSql);
                            }
                            else
                            {
                                AuditService.UpdatedLog(receiveDetail);
                                _PurchaseReturnDetailRepository.Update(receiveDetail);

                                var invMaterial = _PurchaseReturnDetailRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + itemDetail.InventoryMaterialId + "'").FirstOrDefault();
                                var invMaterialRcv = _PurchaseReturnDetailRepository.SqlQuery<InventoryReceiveDetail>(@"SELECT * FROM [TRN].[InventoryReceiveDetail] WHERE Id='" + itemDetail.InventoryReceiveDetailId + "'").FirstOrDefault();

                                var invMaterialRcv1 = _PurchaseReturnDetailRepository.SqlQuery<PurchaseReturnDetail>(@"SELECT * FROM [TRN].[PurchaseReturnDetail] WHERE Id='" + itemDetail.Id + "'").FirstOrDefault();
                                builderSql = @"UPDATE [TRN].[InventoryReceiveDetail] SET PurchaseReturnQty='" + Convert.ToDecimal((invMaterialRcv.PurchaseReturnQty - itemDetail.oldReturnQty) + itemDetail.TransactionQty) + @"' WHERE Id = '" + itemDetail.InventoryServiceId + "'";

                                rdBuilder.Append(builderSql);
                                var invRcved = _receiveDetailRepository.SqlQuery<InventoryReceiveDetail>(@"SELECT * FROM [TRN].[InventoryReceiveDetail] WHERE Id='" + itemDetail.InventoryServiceId + "'").FirstOrDefault();

                                builderSql = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal((invMaterial.TotalQty + itemDetail.oldReturnQty) - itemDetail.TransactionQty) + "' WHERE Id='" + itemDetail.InventoryMaterialId + "'";
                                rdBuilder.Append(builderSql);
                            }


                        }
                        catch (DivideByZeroException ex)
                        {

                        }
                        finally
                        {

                        }
                    }

                    if (taxCategoryList.IsNotNull())
                    {
                        if (string.IsNullOrEmpty(itemDetail.Id))
                        {
                            foreach (var item in taxCategoryList.Where(r => r.PODetailId == Temppodetailid))//
                            {
                                //currentId++;
                                item.Id = GetPurchaseReturnTaxPK();
                                item.PurchaseReturnId = entity.Id;//itemDetail.InventoryReceiveId;
                                item.PurchaseReturnDetailId = grndId;
                                item.PurchaseReturnServiceId = null;
                                item.TaxAmount = Math.Round(Convert.ToDecimal(item.TaxAmount), 2);
                                AuditService.AddedLog(item);
                                _PurchaseReturnTaxRepository.Insert(item);
                            }
                        }
                        else
                        {
                            foreach (var item in taxCategoryList.Where(r => r.PODetailId == Temppodetailid))//
                            {
                                //currentId++;
                                item.Id = item.Id;
                                item.PurchaseReturnId = entity.Id;//itemDetail.InventoryReceiveId;
                                item.PurchaseReturnDetailId = grndId;
                                item.PurchaseReturnServiceId = null;
                                item.TaxAmount = Math.Round(Convert.ToDecimal(item.TaxAmount), 2);
                                AuditService.UpdatedLog(item);
                                _PurchaseReturnTaxRepository.Update(item);
                            }
                        }

                    }
                }

                #region Service
                var currentId = _PurchaseReturnServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseReturnService] WHERE PurchaseReturnId='{entity.Id}'").First();
                if (chargesList != null)
                {
                    foreach (var itemDetail in chargesList)
                    {
                        if (itemDetail.IsNotNull())
                        {
                            //entity.ToCurrencyRate = entity.ToCurrencyRate == 0 ? 1 : entity.ToCurrencyRate;

                            currentId++;
                            var service = new PurchaseReturnService
                            {
                                Id = MakePK(entity.Id + 2, currentId, 2),
                                PurchaseReturnId = entity.Id,
                                ServiceMasterId = itemDetail.ServiceMasterId,
                                Amount = Math.Round(Convert.ToDecimal(itemDetail.Amount), 2),
                                TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalTaxAmount), 2),
                            };
                            AuditService.AddedLog(service);
                            //InsertGraph(service);
                            _PurchaseReturnServiceRepository.Insert(service);
                            if (ServicetaxCategoryList.IsNotNull())
                            {
                                var crrId = _PurchaseReturnTaxRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[PurchaseReturnTax] WHERE PurchaseReturnServiceId='{service.Id}'").First();
                                foreach (var item in ServicetaxCategoryList.Where(r => r.ServiceMasterId == service.ServiceMasterId))
                                {
                                    crrId++;
                                    item.Id = GetPurchaseReturnTaxPK();// MakePK(service.Id, crrId, 2);
                                    item.PurchaseReturnId = entity.Id;
                                    item.PurchaseReturnDetailId = null;
                                    item.PurchaseReturnServiceId = service.Id;
                                    item.TaxAmount = Math.Round(Convert.ToDecimal(item.TaxAmount), 2);

                                    AuditService.AddedLog(item);
                                    _PurchaseReturnTaxRepository.Insert(item);
                                }
                            }
                        }
                    }
                }

                #endregion
                _unitOfWork.SaveChanges();
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
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

        public void DeletePurchaseReturnRow1(string PurchaseReturnDetailId, string inventoryReceiveDetailId, string InventoryMaterial, decimal Trasantionqty)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var rdBuilder = new System.Text.StringBuilder();
                //flag = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"delete from trn.PurchaseReturnTax where PurchaseReturnDetailId='" + PurchaseReturnDetailId + "'";
                _sqlRepository.GetDataCollection(sql);
                var sql1 = @"delete from trn.PurchaseReturnDetail where id='" + PurchaseReturnDetailId + "'";
                _sqlRepository.GetDataCollection(sql1);
                var invMaterial = _PurchaseReturnDetailRepository.SqlQuery<InventoryMaterial>(@"SELECT * FROM [TRN].[InventoryMaterial] WHERE Id='" + InventoryMaterial + "'").FirstOrDefault();
                var sql2 = @"UPDATE [TRN].[InventoryReceiveDetail] SET PurchaseReturnQty='" + Convert.ToDecimal(0.00) + @"' WHERE Id = '" + inventoryReceiveDetailId + "'";

                _sqlRepository.GetDataCollection(sql2);
                //var invRcved = _receiveDetailRepository.SqlQuery<InventoryReceiveDetail>(@"SELECT * FROM [TRN].[InventoryReceiveDetail] WHERE Id='" + inventoryReceiveDetailId + "'").FirstOrDefault();

                var sql3 = @"UPDATE [TRN].[InventoryMaterial] SET TotalQty='" + Convert.ToDecimal((invMaterial.TotalQty - Trasantionqty)) + "' WHERE Id='" + InventoryMaterial + "'";
                _sqlRepository.GetDataCollection(sql3);
                _unitOfWork.SaveChanges();
                //_sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
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
        public IEnumerable<object> GetCheckedByAndApprovedBY(string CheckedBy, string ApprovedBy)
        {

            var sql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (CheckedBy == "true" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='IssueSlipCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='IssueSlipApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "false")
                {

                }
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        #region notification setting Purchase Return

        public IEnumerable<object> GetCheckedByAndApprovedBYForPurchaserReturn(string CheckedBy, string ApprovedBy)
        {

            var sql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (CheckedBy == "true" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='PurchaseReturnCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                else if (CheckedBy == "false" && ApprovedBy == "true")
                {
                    sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='PurchaseReturnApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion


        public IEnumerable<object> GetProductionRecipeMaterialList(string productionOrderId)
        {
            try
            {
                var _sql = @"SELECT POD.Id,0 AS Checked, POD.ProductionOrderId, POD.SalesOrderId
	                            --, RM.Id AS RecipeMaterialId
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
                                , LSD = REPLACE(CONVERT(CHAR(11), LSD, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , DEST.UserName AS DestinationName, SHP.UserName AS ShipmentModeName
	                            , PO.PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description
	                            , Flag = CAST(0 AS BIT)
                                ,null Active
                            FROM [TRN].[ProductionOrderDetail] AS POD
                            LEFT JOIN [TRN].[SalesOrder] AS SO ON POD.SalesOrderId=SO.Id
                            LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
                            --LEFT JOIN [TRN].[RecipeMaterial] AS RM ON RM.MaterialMasterId = MOI.MaterialMasterId AND RM.ArticleId = MOI.ArticleId
                            JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id --RM.MaterialMasterId = MM.Id AND 
                            JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id --RM.ArticleId = ART.Id AND 
                            LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                            LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                            LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                            LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                            LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                            WHERE POD.ProductionOrderId = '" + productionOrderId + "'";

                _sql = @"SELECT ROW_NUMBER() OVER (ORDER BY MasterOrderItemId) AS RN
	                            ,POD.Id, POD.ProductionOrderId, MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName,MOI.BuyerReferenceNo,MOI.OwnReferenceNo,MO.BuyerReferenceNo AS BuyerOrderNo,MO.OwnReferenceNo AS OwnOrderNo
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
                                , LSD = REPLACE(CONVERT(CHAR(11), SO.LSD, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description
	                            , Flag = CAST(0 AS BIT),null Active
                       FROM 
                       [TRN].[ProductionOrderDetail] AS POD
                       JOIN [TRN].[SalesOrder] AS SO ON pod.SalesOrderId=so.Id
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                            WHERE POD.ProductionOrderId = '" + productionOrderId + "'" +
                            "ORDER BY MOI.MATERIALMASTERID,MOI.ArticleID";

                return _sqlRepository.GetDataCollection(_sql, null);
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

        public IEnumerable<object> GetProcessByProductionOrder(string productionOrderId)
        {
            try
            {
                var _sql = @"Select B.Id Value,B.UserName Text from 
							[TRN].[ProductionOrderProcessSet] A
							Left join hkp.Process B On B.Id=A.ProcessId
							where ProductionOrderId='" + productionOrderId + @"'";

                return _sqlRepository.GetDataCollection(_sql, null);
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

        #region OS GRN Save

        public void OSReceiptGRNInsertOrUpdateGraphNew(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, IEnumerable<InventoryMaterialViewModel> entityMatByProduct)
        {
            var flag = false;
            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {
                _unitOfWork.BeginTransaction();

                flag = true;
                //entity.Id = null;
                entity.GRNType = GRNType;
                if (entity.Id.IsNull())
                {

                    var BaseCurrencyId = _receiveDetailRepository.SqlQuery<string>($"SELECT  BaseCurrencyId FROM [Org].[Company]  WHERE Id ='{entity.CompanyId}'").First();
                    entity.CurrencyId = BaseCurrencyId.ToString();
                    _inventoryReceiveService.Insert(entity);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    var Temppodetailid = "";
                    var grndId = "";
                    var currentId1 = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{entity.Id}'").First();
                    if (entityMat.IsNotNull())
                    {

                        foreach (var itemDetail in entityMat)
                        {
                            itemDetail.CompanyGroupId = identity.CompanyGroupId;
                            itemDetail.CompanyId = identity.CompanyId;
                            itemDetail.PlantId = identity.PlantId;
                            Temppodetailid = itemDetail.InventoryReceiveDetailId;
                            itemDetail.IsNonCreditable = entity.IsNonCreditable;
                            if (CheckItemExist(itemDetail))
                                throw new CustomException(itemDetail.MaterialMasterName + " already received");

                            ResetCurrencyRate(itemDetail);

                            if (itemDetail.IsNotNull())
                            {
                                var materialData = _inventoryMaterialMasterService.JWGetInventoryMaterialByUpToSku(itemDetail);
                                if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                                ///TODO : Get total qyt and amount by country and issue qty
                                itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                                itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());

                                itemDetail.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.PurchaseReturnQty).Sum());
                                itemDetail.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueReturnQty).Sum());
                                itemDetail.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                                itemDetail.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventorySalesQty).Sum());
                                itemDetail.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryScrapQty).Sum());
                                itemDetail.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryTransferQty).Sum());

                                var ShortageQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ShortageQty).Sum();
                                var RejectionQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.RejectionQty).Sum();
                                var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ApprovedQty).Sum();


                                var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                                var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                                var altUomIds = new string[] { itemDetail.TransactionUoMId };
                                var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                                if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                                     && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                {

                                    ///Added Date 22-10-19
                                    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                    itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                    if (itemDetail.TotalTaxAmount == null)
                                        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                      Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                                    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                                }

                                else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                                    && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                {

                                    //AddedDate
                                    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                    itemDetail.BaseQty = itemDetail.NetQty;
                                    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                    if (itemDetail.TotalTaxAmount == null)
                                        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                      Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;


                                }
                                else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                                {

                                    //added date 22-10-2019
                                    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                    itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);
                                    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                    if (itemDetail.TotalTaxAmount == null)
                                        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                      Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                                    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;


                                }
                                else
                                {

                                    //Added Date :22-10-2019

                                    itemDetail.BaseUoMFactor = 1;
                                    itemDetail.BaseQty = itemDetail.NetQty;
                                    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                    if (itemDetail.TotalTaxAmount == null)
                                        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                      Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                                }





                                // Insert in receive detail
                                if (string.IsNullOrEmpty(itemDetail.Id))
                                {
                                    var NewId = entity.Id + "-";


                                    currentId1++;
                                    grndId = NewId + currentId1;
                                    var receiveDetail = new InventoryReceiveDetail
                                    {

                                        Id = NewId + currentId1,
                                        MaterialStorageId = itemDetail.MaterialStorageId,
                                        InventoryReceiveId = entity.Id,

                                        TransactionQty = itemDetail.NetQty,
                                        TransactionUoMId = itemDetail.TransactionUoMId,
                                        BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                        BaseUOMId = itemDetail.BaseUOMId,
                                        BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                        MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                        MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                                        TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                                        POID = itemDetail.POID,
                                        PODetailsID = itemDetail.PODetailsID,
                                        TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                                        ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                                        ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTaxTranAmount), 2),
                                        IssueQty = 0,
                                        BaseIssueQty = 0,
                                        TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 2),
                                        PurchaseDocumentAcceptanceId = itemDetail.PurchaseDocumentAcceptanceId,
                                        PurchaseDocumentAcceptanceDetailId = itemDetail.PurchaseDocumentAcceptanceDetailId,
                                        PurchaseReturnQty = 0,
                                        IssueReturnQty = 0,
                                        InventorySalesQty = 0,
                                        InventoryScrapQty = 0,
                                        MaterialMasterOpeningBalanceDetailId = null,
                                        LotNumber = itemDetail.LotNumber,
                                        LotNo = itemDetail.LotNumber,
                                        Diameter = itemDetail.Diameter,
                                        Type = itemDetail.Type,
                                        ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                        RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                        ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),
                                        ShortageRatePercent = 110,
                                        ShortageValue = Math.Round(Convert.ToDecimal(itemDetail.ShortageValue), 2),
                                        RejectRatePercent = 50,
                                        GRNQty = itemDetail.TransactionQty,
                                        GRNTotalAmount = Math.Round(itemDetail.TransactionQty * Convert.ToDecimal(itemDetail.TransactionRate), 2),
                                        IsAsset = itemDetail.IsAsset,
                                        GrossAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                        DiscountAmount = Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                        QualityStatus = itemDetail.QualityStatus,
                                        OSTransformationPOId = itemDetail.OSTransformationPOId,
                                        OSTransformationPODetailId = itemDetail.OSTransformationPODetailId,
                                        OSTransformationPOInputMaterialId = null,
                                        OSTransformationPOByProductId = null,
                                        MaterialFor = "JWOUTPUTMaterial"
                                    };
                                    try
                                    {
                                        itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                                        receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                        receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                        receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);
                                        AuditService.AddedLog(receiveDetail);
                                        itemDetail.TotalQty = ((Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty + itemDetail.IssueReturnQty)) - (Convert.ToDecimal(itemDetail.IssueQty) + Convert.ToDecimal(itemDetail.PurchaseReturnQty) + Convert.ToDecimal(itemDetail.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetail.InventorySalesQty) + Convert.ToDecimal(itemDetail.InventoryScrapQty) + Convert.ToDecimal(itemDetail.InventoryTransferQty)));
                                        itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetail.TotalQty);
                                        itemDetail.ShortageQty = Convert.ToDecimal(receiveDetail.ShortageQty + ShortageQty);
                                        itemDetail.RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty + RejectionQty);
                                        itemDetail.ApprovedQty = Convert.ToDecimal(receiveDetail.ApprovedQty + ApprovedQty);
                                        _inventoryMaterialMasterService.JWInsertOrUpdateFromReceive(itemDetail);
                                        receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                                        InsertGraph(receiveDetail);
                                        int rejectDetailId = 1;
                                        var RejectionDetails = new GRNRejectionDetails
                                        {
                                            Id = grndId.ToString() + rejectDetailId,
                                            GRNDeailsId = grndId,
                                            RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty),
                                            RejectionUoMId = itemDetail.TransactionUoMId,
                                            BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                            BaseUOMId = itemDetail.BaseUOMId,
                                            RejectionRate = Convert.ToDecimal(receiveDetail.RejectRatePercent),
                                            RejeactionValue = Convert.ToDecimal(receiveDetail.RejectValue),
                                        };
                                        AuditService.AddedLog(RejectionDetails);
                                        _gRNRejectionDetailsRepository.Insert(RejectionDetails);

                                    }
                                    catch (DivideByZeroException ex)
                                    {

                                    }
                                    finally
                                    {

                                    }
                                }
                            }
                        }
                    }

                    if (entityMatByProduct.IsNotNull())
                    {

                        foreach (var itemDetailNew in entityMatByProduct)
                        {
                            if (itemDetailNew.ArticleId.IsNotNull())
                            {
                                itemDetailNew.CompanyGroupId = identity.CompanyGroupId;
                                itemDetailNew.CompanyId = identity.CompanyId;
                                itemDetailNew.PlantId = identity.PlantId;
                                Temppodetailid = itemDetailNew.InventoryReceiveDetailId;
                                itemDetailNew.IsNonCreditable = entity.IsNonCreditable;
                                if (CheckItemExist(itemDetailNew))
                                    throw new CustomException(itemDetailNew.MaterialMasterName + " already received");

                                ResetCurrencyRate(itemDetailNew);

                                if (itemDetailNew.IsNotNull())
                                {
                                    var materialData = _inventoryMaterialMasterService.JWGetInventoryMaterialByUpToSku(itemDetailNew);
                                    if (materialData.IsNotNull()) itemDetailNew.InventoryMaterialId = materialData.Id;
                                    ///TODO : Get total qyt and amount by country and issue qty
                                    itemDetailNew.TotalQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.BaseQty).Sum();
                                    itemDetailNew.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.IssueQty).Sum());

                                    itemDetailNew.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.PurchaseReturnQty).Sum());
                                    itemDetailNew.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.IssueReturnQty).Sum());
                                    itemDetailNew.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                                    itemDetailNew.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventorySalesQty).Sum());
                                    itemDetailNew.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventoryScrapQty).Sum());
                                    itemDetailNew.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventoryTransferQty).Sum());

                                    var ShortageQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ShortageQty).Sum();
                                    var RejectionQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.RejectionQty).Sum();
                                    var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ApprovedQty).Sum();


                                    var totalAmount = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                                    var materialMasterIds = new string[] { itemDetailNew.MaterialMasterId };
                                    var altUomIds = new string[] { itemDetailNew.TransactionUoMId };
                                    var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                                    if (itemDetailNew.BaseUOMId != itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId != itemDetailNew.BaseCurrencyId
                                         && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        ///Added Date 22-10-19
                                        itemDetailNew.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetailNew.BaseUOMId && t.AlternativeUOMId == itemDetailNew.TransactionUoMId).BaseUOMFactor);
                                        itemDetailNew.BaseQty = Convert.ToDecimal(itemDetailNew.NetQty * itemDetailNew.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;

                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;

                                    }
                                    else if (itemDetailNew.BaseUOMId == itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId != itemDetailNew.BaseCurrencyId)
                                    {

                                        //added date 22-10-2019
                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = Convert.ToDecimal(itemDetailNew.NetQty * itemDetailNew.BaseUoMFactor);
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;

                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;


                                    }
                                    else if (itemDetailNew.BaseUOMId != itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId == itemDetailNew.BaseCurrencyId
                                        && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        //AddedDate
                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = itemDetailNew.NetQty;
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;
                                    }
                                    else
                                    {

                                        //Added Date :22-10-2019

                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = itemDetailNew.NetQty;
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;
                                    }

                                    // Insert in receive detail
                                    if (string.IsNullOrEmpty(itemDetailNew.Id))
                                    {
                                        var NewId = entity.Id + "-";
                                        currentId1++;
                                        grndId = NewId + currentId1;
                                        var receiveDetail = new InventoryReceiveDetail
                                        {

                                            Id = NewId + currentId1,
                                            MaterialStorageId = itemDetailNew.MaterialStorageId,
                                            InventoryReceiveId = entity.Id,
                                            TransactionQty = itemDetailNew.NetQty,
                                            TransactionUoMId = itemDetailNew.TransactionUoMId,
                                            BaseQty = Convert.ToDecimal(itemDetailNew.BaseQty),
                                            BaseUOMId = itemDetailNew.BaseUOMId,
                                            BaseUoMFactor = Convert.ToDecimal(itemDetailNew.BaseUoMFactor),
                                            MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetailNew.TransactionRate), 4),
                                            MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TrnAmount), 2),
                                            TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TotalMaterialTranAmount), 2),
                                            TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TotalMaterialBooksCurrencyAmount), 2),
                                            POID = itemDetailNew.POID,
                                            PODetailsID = itemDetailNew.PODetailsID,
                                            TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetailNew.BaseTaxAmount), 2),
                                            ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.ChargesTranAmount), 2),
                                            ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.ChargesTaxTranAmount), 2),
                                            IssueQty = 0,
                                            BaseIssueQty = 0,
                                            TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetailNew.TrnCurrencyBaseRate), 4),
                                            BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetailNew.BooksCurrencyBaseRate), 2),
                                            PurchaseDocumentAcceptanceId = itemDetailNew.PurchaseDocumentAcceptanceId,
                                            PurchaseDocumentAcceptanceDetailId = itemDetailNew.PurchaseDocumentAcceptanceDetailId,
                                            PurchaseReturnQty = 0,
                                            IssueReturnQty = 0,
                                            InventorySalesQty = 0,
                                            InventoryScrapQty = 0,
                                            MaterialMasterOpeningBalanceDetailId = null,
                                            LotNumber = itemDetailNew.LotNumber,
                                            LotNo = itemDetailNew.LotNumber,
                                            Diameter = itemDetailNew.Diameter,
                                            Type = itemDetailNew.Type,
                                            ShortageQty = Convert.ToDecimal(itemDetailNew.ShortageQty),
                                            RejectionQty = Convert.ToDecimal(itemDetailNew.RejectionQty),
                                            ApprovedQty = Convert.ToDecimal(itemDetailNew.ApprovedQty),
                                            ShortageRatePercent = 110,
                                            ShortageValue = Math.Round(Convert.ToDecimal(itemDetailNew.ShortageValue), 2),
                                            RejectRatePercent = 50,
                                            GRNQty = itemDetailNew.TransactionQty,
                                            GRNTotalAmount = Math.Round(itemDetailNew.TransactionQty * Convert.ToDecimal(itemDetailNew.TransactionRate), 2),
                                            IsAsset = itemDetailNew.IsAsset,
                                            GrossAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetailNew.DiscountAmount), 2),
                                            DiscountAmount = Math.Round(Convert.ToDecimal(itemDetailNew.DiscountAmount), 2),
                                            QualityStatus = itemDetailNew.QualityStatus,
                                            OSTransformationPOId = null,
                                            OSTransformationPODetailId = null,
                                            OSTransformationPOInputMaterialId = itemDetailNew.OSTransformationPOInputMaterialId,
                                            OSTransformationPOByProductId = itemDetailNew.OSTransformationPOByProductId,
                                            MaterialFor = "JWBYPRODUCTMaterial"
                                        };
                                        try
                                        {
                                            itemDetailNew.InventoryReceiveDetailId = receiveDetail.Id;
                                            receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetailNew.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetailNew.TransactionRate), 2);
                                            receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetailNew.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetailNew.TransactionRate), 2);
                                            receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);
                                            AuditService.AddedLog(receiveDetail);
                                            itemDetailNew.TotalQty = ((Convert.ToDecimal(itemDetailNew.TotalQty + itemDetailNew.BaseQty + itemDetailNew.IssueReturnQty)) - (Convert.ToDecimal(itemDetailNew.IssueQty) + Convert.ToDecimal(itemDetailNew.PurchaseReturnQty) + Convert.ToDecimal(itemDetailNew.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetailNew.InventorySalesQty) + Convert.ToDecimal(itemDetailNew.InventoryScrapQty) + Convert.ToDecimal(itemDetailNew.InventoryTransferQty)));
                                            itemDetailNew.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetailNew.TotalQty);
                                            itemDetailNew.ShortageQty = Convert.ToDecimal(receiveDetail.ShortageQty + ShortageQty);
                                            itemDetailNew.RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty + RejectionQty);
                                            itemDetailNew.ApprovedQty = Convert.ToDecimal(receiveDetail.ApprovedQty + ApprovedQty);

                                            _inventoryMaterialMasterService.JWInsertOrUpdateFromReceive(itemDetailNew);
                                            receiveDetail.InventoryMaterialId = itemDetailNew.InventoryMaterialId;
                                            InsertGraph(receiveDetail);
                                            int rejectDetailId = 1;
                                            var RejectionDetails = new GRNRejectionDetails
                                            {
                                                Id = grndId.ToString() + rejectDetailId,
                                                GRNDeailsId = grndId,
                                                RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty),
                                                RejectionUoMId = itemDetailNew.TransactionUoMId,
                                                BaseUoMFactor = Convert.ToDecimal(itemDetailNew.BaseUoMFactor),
                                                BaseUOMId = itemDetailNew.BaseUOMId,
                                                RejectionRate = Convert.ToDecimal(receiveDetail.RejectRatePercent),
                                                RejeactionValue = Convert.ToDecimal(receiveDetail.RejectValue),
                                            };
                                            AuditService.AddedLog(RejectionDetails);
                                            _gRNRejectionDetailsRepository.Insert(RejectionDetails);

                                        }
                                        catch (DivideByZeroException ex)
                                        {

                                        }
                                        finally
                                        {

                                        }
                                    }
                                }


                            }
                        }
                    }
                }
                else
                {

                    _inventoryReceiveService.Update(entity);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    var Temppodetailid = "";
                    if (entityMat.IsNotNull())
                    {
                        foreach (var itemDetail in entityMat)
                        {
                            itemDetail.CompanyGroupId = identity.CompanyGroupId;
                            itemDetail.CompanyId = identity.CompanyId;
                            itemDetail.PlantId = identity.PlantId;
                            Temppodetailid = itemDetail.InventoryReceiveDetailId;
                            itemDetail.IsNonCreditable = entity.IsNonCreditable;
                            if (CheckItemExist(itemDetail))
                                throw new CustomException(itemDetail.MaterialMasterName + " already received");

                            ResetCurrencyRate(itemDetail);

                            if (itemDetail.IsNotNull())
                            {
                                var materialData = _inventoryMaterialMasterService.JWGetInventoryMaterialByUpToSku(itemDetail);
                                if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                                ///TODO : Get total qyt and amount by country and issue qty
                                itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                                itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());

                                itemDetail.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.PurchaseReturnQty).Sum());
                                itemDetail.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueReturnQty).Sum());
                                itemDetail.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                                itemDetail.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventorySalesQty).Sum());
                                itemDetail.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryScrapQty).Sum());
                                itemDetail.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryTransferQty).Sum());

                                var ShortageQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ShortageQty).Sum();
                                var RejectionQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.RejectionQty).Sum();
                                var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ApprovedQty).Sum();


                                var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                                var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                                var altUomIds = new string[] { itemDetail.TransactionUoMId };
                                var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                                if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                                     && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                {

                                    ///Added Date 22-10-19
                                    itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                    itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                    if (itemDetail.TotalTaxAmount == null)
                                        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                      Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                                    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                                }
                                else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                                {

                                    //added date 22-10-2019
                                    itemDetail.BaseUoMFactor = 1;
                                    itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);
                                    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                    if (itemDetail.TotalTaxAmount == null)
                                        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                      Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                                    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;


                                }
                                else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                                    && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                {

                                    //AddedDate
                                    itemDetail.BaseUoMFactor = 1;
                                    itemDetail.BaseQty = itemDetail.NetQty;
                                    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                    if (itemDetail.TotalTaxAmount == null)
                                        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                      Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;


                                }
                                else
                                {

                                    //Added Date :22-10-2019

                                    itemDetail.BaseUoMFactor = 1;
                                    itemDetail.BaseQty = itemDetail.NetQty;
                                    itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                    itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                    itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                    if (itemDetail.TotalTaxAmount == null)
                                        itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                    itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                      Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                    itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                    itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                             Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                    itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                    itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                                }





                                // Insert in receive detail

                                var NewId = entity.Id + "-";


                                //currentId1++;
                                //grndId = NewId + currentId1;
                                var receiveDetail = new InventoryReceiveDetail
                                {

                                    Id = itemDetail.InventoryReceiveDetailId,
                                    MaterialStorageId = itemDetail.MaterialStorageId,
                                    InventoryReceiveId = entity.Id,

                                    TransactionQty = itemDetail.NetQty,
                                    TransactionUoMId = itemDetail.TransactionUoMId,
                                    BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                    BaseUOMId = itemDetail.BaseUOMId,
                                    BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                    MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                    MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                                    TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                                    TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                                    POID = itemDetail.POID,
                                    PODetailsID = itemDetail.PODetailsID,
                                    TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                                    ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                                    ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTaxTranAmount), 2),
                                    IssueQty = 0,
                                    BaseIssueQty = 0,
                                    TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                    BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 2),
                                    PurchaseDocumentAcceptanceId = itemDetail.PurchaseDocumentAcceptanceId,
                                    PurchaseDocumentAcceptanceDetailId = itemDetail.PurchaseDocumentAcceptanceDetailId,
                                    PurchaseReturnQty = 0,
                                    IssueReturnQty = 0,
                                    InventorySalesQty = 0,
                                    InventoryScrapQty = 0,
                                    MaterialMasterOpeningBalanceDetailId = null,
                                    LotNumber = itemDetail.LotNumber,
                                    LotNo = itemDetail.LotNumber,
                                    Diameter = itemDetail.Diameter,
                                    Type = itemDetail.Type,
                                    ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                    RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                    ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),
                                    ShortageRatePercent = 110,
                                    ShortageValue = Math.Round(Convert.ToDecimal(itemDetail.ShortageValue), 2),
                                    RejectRatePercent = 50,
                                    GRNQty = itemDetail.TransactionQty,
                                    GRNTotalAmount = Math.Round(itemDetail.TransactionQty * Convert.ToDecimal(itemDetail.TransactionRate), 2),
                                    IsAsset = itemDetail.IsAsset,
                                    GrossAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                    DiscountAmount = Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                    QualityStatus = itemDetail.QualityStatus,
                                    OSTransformationPOId = itemDetail.OSTransformationPOId,
                                    OSTransformationPODetailId = itemDetail.OSTransformationPODetailId,
                                    OSTransformationPOInputMaterialId = null,
                                    OSTransformationPOByProductId = null,
                                    MaterialFor = "JWOUTPUTMaterial"



                                };
                                try
                                {

                                    itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                                    receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                    receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                    receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);

                                    AuditService.UpdatedLog(receiveDetail);

                                    itemDetail.TotalQty = ((Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty + itemDetail.IssueReturnQty)) - (Convert.ToDecimal(itemDetail.IssueQty) + Convert.ToDecimal(itemDetail.PurchaseReturnQty) + Convert.ToDecimal(itemDetail.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetail.InventorySalesQty) + Convert.ToDecimal(itemDetail.InventoryScrapQty) + Convert.ToDecimal(itemDetail.InventoryTransferQty)));
                                    itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetail.TotalQty);
                                    itemDetail.ShortageQty = Convert.ToDecimal(receiveDetail.ShortageQty + ShortageQty);
                                    itemDetail.RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty + RejectionQty);
                                    itemDetail.ApprovedQty = Convert.ToDecimal(receiveDetail.ApprovedQty + ApprovedQty);

                                    _inventoryMaterialMasterService.JWInsertOrUpdateFromReceive(itemDetail);
                                    receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                                    UpdateGraph(receiveDetail);

                                }
                                catch (DivideByZeroException )
                                {

                                }
                                finally
                                {

                                }

                            }
                        }
                    }

                    if (entityMatByProduct.IsNotNull())
                    {

                        foreach (var itemDetailNew in entityMatByProduct)
                        {
                            if (itemDetailNew.ArticleId.IsNotNull())
                            {
                                itemDetailNew.CompanyGroupId = identity.CompanyGroupId;
                                itemDetailNew.CompanyId = identity.CompanyId;
                                itemDetailNew.PlantId = identity.PlantId;
                                Temppodetailid = itemDetailNew.InventoryReceiveDetailId;
                                itemDetailNew.IsNonCreditable = entity.IsNonCreditable;
                                if (CheckItemExist(itemDetailNew))
                                    throw new CustomException(itemDetailNew.MaterialMasterName + " already received");

                                ResetCurrencyRate(itemDetailNew);

                                if (itemDetailNew.IsNotNull())
                                {
                                    var materialData = _inventoryMaterialMasterService.JWGetInventoryMaterialByUpToSku(itemDetailNew);
                                    if (materialData.IsNotNull()) itemDetailNew.InventoryMaterialId = materialData.Id;
                                    ///TODO : Get total qyt and amount by country and issue qty
                                    itemDetailNew.TotalQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.BaseQty).Sum();
                                    itemDetailNew.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.IssueQty).Sum());

                                    itemDetailNew.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.PurchaseReturnQty).Sum());
                                    itemDetailNew.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.IssueReturnQty).Sum());
                                    itemDetailNew.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                                    itemDetailNew.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventorySalesQty).Sum());
                                    itemDetailNew.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventoryScrapQty).Sum());
                                    itemDetailNew.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventoryTransferQty).Sum());

                                    var ShortageQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ShortageQty).Sum();
                                    var RejectionQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.RejectionQty).Sum();
                                    var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ApprovedQty).Sum();


                                    var totalAmount = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                                    var materialMasterIds = new string[] { itemDetailNew.MaterialMasterId };
                                    var altUomIds = new string[] { itemDetailNew.TransactionUoMId };
                                    var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                                    if (itemDetailNew.BaseUOMId != itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId != itemDetailNew.BaseCurrencyId
                                         && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        ///Added Date 22-10-19
                                        itemDetailNew.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetailNew.BaseUOMId && t.AlternativeUOMId == itemDetailNew.TransactionUoMId).BaseUOMFactor);
                                        itemDetailNew.BaseQty = Convert.ToDecimal(itemDetailNew.NetQty * itemDetailNew.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;

                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;

                                    }
                                    else if (itemDetailNew.BaseUOMId == itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId != itemDetailNew.BaseCurrencyId)
                                    {

                                        //added date 22-10-2019
                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = Convert.ToDecimal(itemDetailNew.NetQty * itemDetailNew.BaseUoMFactor);
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;

                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;


                                    }
                                    else if (itemDetailNew.BaseUOMId != itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId == itemDetailNew.BaseCurrencyId
                                        && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        //AddedDate
                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = itemDetailNew.NetQty;
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;


                                    }
                                    else
                                    {

                                        //Added Date :22-10-2019

                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = itemDetailNew.NetQty;
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;

                                    }

                                    var NewId = entity.Id + "-";
                                    var receiveDetail1 = new InventoryReceiveDetail
                                    {

                                        Id = itemDetailNew.InventoryReceiveDetailId,
                                        MaterialStorageId = itemDetailNew.MaterialStorageId,
                                        InventoryReceiveId = entity.Id,
                                        TransactionQty = itemDetailNew.NetQty,
                                        TransactionUoMId = itemDetailNew.TransactionUoMId,
                                        BaseQty = Convert.ToDecimal(itemDetailNew.BaseQty),
                                        BaseUOMId = itemDetailNew.BaseUOMId,
                                        BaseUoMFactor = Convert.ToDecimal(itemDetailNew.BaseUoMFactor),
                                        MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetailNew.TransactionRate), 4),
                                        MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TrnAmount), 2),
                                        TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TotalMaterialTranAmount), 2),
                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TotalMaterialBooksCurrencyAmount), 2),
                                        POID = itemDetailNew.POID,
                                        PODetailsID = itemDetailNew.PODetailsID,
                                        TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetailNew.BaseTaxAmount), 2),
                                        ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.ChargesTranAmount), 2),
                                        ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.ChargesTaxTranAmount), 2),
                                        IssueQty = 0,
                                        BaseIssueQty = 0,
                                        TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetailNew.TrnCurrencyBaseRate), 4),
                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetailNew.BooksCurrencyBaseRate), 2),
                                        PurchaseDocumentAcceptanceId = itemDetailNew.PurchaseDocumentAcceptanceId,
                                        PurchaseDocumentAcceptanceDetailId = itemDetailNew.PurchaseDocumentAcceptanceDetailId,
                                        PurchaseReturnQty = 0,
                                        IssueReturnQty = 0,
                                        InventorySalesQty = 0,
                                        InventoryScrapQty = 0,
                                        MaterialMasterOpeningBalanceDetailId = null,
                                        LotNumber = itemDetailNew.LotNumber,
                                        LotNo = itemDetailNew.LotNumber,
                                        Diameter = itemDetailNew.Diameter,
                                        Type = itemDetailNew.Type,
                                        ShortageQty = Convert.ToDecimal(itemDetailNew.ShortageQty),
                                        RejectionQty = Convert.ToDecimal(itemDetailNew.RejectionQty),
                                        ApprovedQty = Convert.ToDecimal(itemDetailNew.ApprovedQty),
                                        ShortageRatePercent = 110,
                                        ShortageValue = Math.Round(Convert.ToDecimal(itemDetailNew.ShortageValue), 2),
                                        RejectRatePercent = 50,
                                        GRNQty = itemDetailNew.TransactionQty,
                                        GRNTotalAmount = Math.Round(itemDetailNew.TransactionQty * Convert.ToDecimal(itemDetailNew.TransactionRate), 2),
                                        IsAsset = itemDetailNew.IsAsset,
                                        GrossAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetailNew.DiscountAmount), 2),
                                        DiscountAmount = Math.Round(Convert.ToDecimal(itemDetailNew.DiscountAmount), 2),
                                        QualityStatus = itemDetailNew.QualityStatus,
                                        OSTransformationPOId = null,
                                        OSTransformationPODetailId = null,
                                        OSTransformationPOInputMaterialId = itemDetailNew.OSTransformationPOInputMaterialId,
                                        OSTransformationPOByProductId = itemDetailNew.OSTransformationPOByProductId,
                                        MaterialFor = "JWBYPRODUCTMaterial"
                                    };
                                    try
                                    {

                                        itemDetailNew.InventoryReceiveDetailId = receiveDetail1.Id;
                                        receiveDetail1.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetailNew.ShortageQty) * receiveDetail1.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetailNew.TransactionRate), 2);
                                        receiveDetail1.RejectValue = Math.Round(((Convert.ToDecimal(itemDetailNew.RejectionQty) * receiveDetail1.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetailNew.TransactionRate), 2);
                                        receiveDetail1.RejectClamPercent = (100 - receiveDetail1.RejectRatePercent);
                                        AuditService.UpdatedLog(receiveDetail1);
                                        itemDetailNew.TotalQty = ((Convert.ToDecimal(itemDetailNew.TotalQty + itemDetailNew.BaseQty + itemDetailNew.IssueReturnQty)) - (Convert.ToDecimal(itemDetailNew.IssueQty) + Convert.ToDecimal(itemDetailNew.PurchaseReturnQty) + Convert.ToDecimal(itemDetailNew.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetailNew.InventorySalesQty) + Convert.ToDecimal(itemDetailNew.InventoryScrapQty) + Convert.ToDecimal(itemDetailNew.InventoryTransferQty)));
                                        itemDetailNew.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail1.TotalMaterialTranAmount) / itemDetailNew.TotalQty);
                                        itemDetailNew.ShortageQty = Convert.ToDecimal(receiveDetail1.ShortageQty + ShortageQty);
                                        itemDetailNew.RejectionQty = Convert.ToDecimal(receiveDetail1.RejectionQty + RejectionQty);
                                        itemDetailNew.ApprovedQty = Convert.ToDecimal(receiveDetail1.ApprovedQty + ApprovedQty);
                                        _inventoryMaterialMasterService.JWInsertOrUpdateFromReceive(itemDetailNew);
                                        receiveDetail1.InventoryMaterialId = itemDetailNew.InventoryMaterialId;
                                        UpdateGraph(receiveDetail1);


                                    }
                                    catch (DivideByZeroException ex)
                                    {

                                    }
                                    finally
                                    {

                                    }
                                }
                            }
                        }
                    }

                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                SaveReceiptTransformationWOMaterial(entityMat, entity.Id);
                SaveReceiptByProductWOMaterial(entityMatByProduct, entity.Id);
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

        private string GetTransformationChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceiveDetail", out sID);
            return sID;
        }

        public void SaveReceiptTransformationWOMaterial(IEnumerable<InventoryMaterialViewModel> entityMat, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //      var JWItemId = "' '";
                var OtMatId = "' '";

                foreach (var empitem in entityMat)
                {
                    if (empitem.ArticleId.IsNull())
                    {
                        //         JWItemId += ",'" + empitem.JWInputItemId + "' ";
                        OtMatId += ",'" + empitem.OSTransformationPODetailId + "' ";
                    }
                }
                con.OpenDataSetThroughAdapter("select * from TRN.InventoryReceiveDetail where OSTransformationPODetailId IN ( " + OtMatId + ") and InventoryReceiveId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in entityMat)
                {
                    if (item.ArticleId.IsNull())
                    {

                        ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPODetailId='" + item.OSTransformationPODetailId + "' and InventoryReceiveId='" + MasterId + "' ";

                        if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = ExistOrNot.Tables[0].NewRow();
                            //        dr["Id"] = "TC" + GetTransformationChildPK();
                            dr["Id"] = GetTransformationChildPK();

                            dr["InventoryReceiveId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseUOMId"] = item.TransactionUoMId;
                            dr["BaseQty"] = item.TransactionQty;
                            dr["QualityStatus"] = item.QualityStatus;
                            dr["OSTransformationPOId"] = item.OSTransformationPOId;
                            dr["OSTransformationPODetailId"] = item.OSTransformationPODetailId;
                            dr["MaterialFor"] = "JWOUTPUTMaterial";

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            ExistOrNot.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPODetailId='" + item.OSTransformationPODetailId + "' and InventoryReceiveId='" + MasterId + "' ";

                            if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = ExistOrNot.Tables[0].NewRow();
                                dr["Id"] = GetTransformationChildPK();

                                dr["InventoryReceiveId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.TransactionUoMId;
                                dr["BaseQty"] = item.TransactionQty;
                                dr["QualityStatus"] = item.QualityStatus;
                                dr["OSTransformationPOId"] = item.OSTransformationPOId;
                                dr["OSTransformationPODetailId"] = item.OSTransformationPODetailId;
                                dr["MaterialFor"] = "JWOUTPUTMaterial";

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                ExistOrNot.Tables[0].Rows.Add(dr);

                            }
                            else
                            {
                                //edit
                                DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["InventoryReceiveId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.TransactionUoMId;
                                dr["BaseQty"] = item.TransactionQty;
                                dr["QualityStatus"] = item.QualityStatus;
                                dr["OSTransformationPOId"] = item.OSTransformationPOId;
                                dr["OSTransformationPODetailId"] = item.OSTransformationPODetailId;
                                dr["MaterialFor"] = "JWOUTPUTMaterial";

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;


                                dr.EndEdit();
                            }


                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        private string GetTransformationBYProdPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceiveDetail", out sID);
            return sID;
        }

        public void SaveReceiptByProductWOMaterial(IEnumerable<InventoryMaterialViewModel> entityMatByProduct, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (entityMatByProduct.IsNotNull())
                {
                    DataSet ExistOrNot;

                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    var MIId = "' '";
                    var BPId = "' '";

                    foreach (var empitem in entityMatByProduct)
                    {
                        if (empitem.ArticleId.IsNull())
                        {
                            MIId += ",'" + empitem.OSTransformationPOInputMaterialId + "' ";
                            BPId += ",'" + empitem.OSTransformationPOByProductId + "' ";
                        }
                    }
                    con.OpenDataSetThroughAdapter("select * from TRN.InventoryReceiveDetail where OSTransformationPOInputMaterialId IN ( " + MIId + ") and OSTransformationPOByProductId IN (" + BPId + ") and InventoryReceiveId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                    foreach (var item in entityMatByProduct)
                    {
                        if (item.ArticleId.IsNull())
                        {

                            ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOInputMaterialId='" + item.OSTransformationPOInputMaterialId + "' and OSTransformationPOByProductId='" + item.OSTransformationPOByProductId + "' and InventoryReceiveId='" + MasterId + "' ";

                            if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = ExistOrNot.Tables[0].NewRow();
                                //        dr["Id"] = "TC" + GetTransformationChildPK();
                                dr["Id"] = GetTransformationBYProdPK();

                                dr["InventoryReceiveId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.TransactionUoMId;
                                dr["BaseQty"] = item.TransactionQty;
                                dr["QualityStatus"] = item.QualityStatus;
                                dr["OSTransformationPOInputMaterialId"] = item.OSTransformationPOInputMaterialId;
                                dr["OSTransformationPOByProductId"] = item.OSTransformationPOByProductId;
                                dr["MaterialFor"] = "JWBYPRODUCTMaterial";

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;
                                ExistOrNot.Tables[0].Rows.Add(dr);

                            }
                            else
                            {
                                ExistOrNot.Tables[0].DefaultView.RowFilter = "OSTransformationPOInputMaterialId='" + item.OSTransformationPOInputMaterialId + "' and OSTransformationPOByProductId='" + item.OSTransformationPOByProductId + "' and InventoryReceiveId='" + MasterId + "' ";

                                if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                                {
                                    DataRow dr = ExistOrNot.Tables[0].NewRow();
                                    dr["Id"] = GetTransformationBYProdPK();

                                    dr["InventoryReceiveId"] = MasterId;
                                    dr["TransactionQty"] = item.TransactionQty;
                                    dr["TransactionUoMId"] = item.TransactionUoMId;
                                    dr["BaseUOMId"] = item.TransactionUoMId;
                                    dr["BaseQty"] = item.TransactionQty;
                                    dr["QualityStatus"] = item.QualityStatus;
                                    dr["OSTransformationPOInputMaterialId"] = item.OSTransformationPOInputMaterialId;
                                    dr["OSTransformationPOByProductId"] = item.OSTransformationPOByProductId;
                                    dr["MaterialFor"] = "JWBYPRODUCTMaterial";

                                    dr["AddedBy"] = identity.Name;
                                    dr["AddedDate"] = System.DateTime.Now.ToString();
                                    dr["AddedFromIP"] = identity.IPAddress;

                                    ExistOrNot.Tables[0].Rows.Add(dr);

                                }
                                else
                                {
                                  
                                    DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;
                                    dr.BeginEdit();
                                    dr["InventoryReceiveId"] = MasterId;
                                    dr["TransactionQty"] = item.TransactionQty;
                                    dr["TransactionUoMId"] = item.TransactionUoMId;
                                    dr["BaseUOMId"] = item.TransactionUoMId;
                                    dr["BaseQty"] = item.TransactionQty;
                                    dr["QualityStatus"] = item.QualityStatus;
                                    dr["OSTransformationPOInputMaterialId"] = item.OSTransformationPOInputMaterialId;
                                    dr["OSTransformationPOByProductId"] = item.OSTransformationPOByProductId;
                                    dr["MaterialFor"] = "JWBYPRODUCTMaterial";

                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                    dr["UpdatedFromIP"] = identity.IPAddress;


                                    dr.EndEdit();
                                }


                            }
                        }
                    }
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(ExistOrNot);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Job Work Receipt
        #region Job Work
        public void JobWorkInsertOrUpdateNew(InventoryReceive entity, IEnumerable<InventoryMaterialViewModel> entityMat, IEnumerable<InventoryReceiveTax> taxCategoryList, string id, string MaterialStorageId, string GRNType, IEnumerable<InventoryMaterialViewModel> entityMatByProduct)
        {
            var flag = false;
            Library.Service.Extension.Conversions.UOMConversion conversion = new Library.Service.Extension.Conversions.UOMConversion();
            try
            {
                _unitOfWork.BeginTransaction();

                flag = true;
                //entity.Id = null;
                entity.GRNType = GRNType;
                if (entity.Id.IsNull())
                {

                    var BaseCurrencyId = _receiveDetailRepository.SqlQuery<string>($"SELECT  BaseCurrencyId FROM [Org].[Company]  WHERE Id ='{entity.CompanyId}'").First();
                    entity.CurrencyId = BaseCurrencyId.ToString();
                    _inventoryReceiveService.Insert(entity);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    var Temppodetailid = "";
                    var grndId = "";
                    var currentId1 = _receiveDetailRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(substring(id, CHARINDEX('-',id)+1,len(id)) AS INT)), 0) Id FROM [TRN].[InventoryReceiveDetail]  WHERE InventoryReceiveId ='{entity.Id}'").First();
                    if (entityMat.IsNotNull())
                    {

                        foreach (var itemDetail in entityMat)
                        {
                            if (itemDetail.ArticleId.IsNotNull())
                            {
                                itemDetail.CompanyGroupId = identity.CompanyGroupId;
                                itemDetail.CompanyId = identity.CompanyId;
                                itemDetail.PlantId = identity.PlantId;
                                Temppodetailid = itemDetail.InventoryReceiveDetailId;
                                itemDetail.IsNonCreditable = entity.IsNonCreditable;
                                if (CheckItemExist(itemDetail))
                                    throw new CustomException(itemDetail.MaterialMasterName + " already received");

                                ResetCurrencyRate(itemDetail);

                                if (itemDetail.IsNotNull())
                                {
                                    var materialData = _inventoryMaterialMasterService.JWGetInventoryMaterialByUpToSku(itemDetail);
                                    if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                                    ///TODO : Get total qyt and amount by country and issue qty
                                    itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                                    itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());

                                    itemDetail.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.PurchaseReturnQty).Sum());
                                    itemDetail.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueReturnQty).Sum());
                                    itemDetail.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                                    itemDetail.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventorySalesQty).Sum());
                                    itemDetail.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryScrapQty).Sum());
                                    itemDetail.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryTransferQty).Sum());

                                    var ShortageQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ShortageQty).Sum();
                                    var RejectionQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.RejectionQty).Sum();
                                    var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ApprovedQty).Sum();


                                    var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                                    var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                                    var altUomIds = new string[] { itemDetail.TransactionUoMId };
                                    var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                                    if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                                         && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        ///Added Date 22-10-19
                                        itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                        itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                        itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                        itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                        itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                        if (itemDetail.TotalTaxAmount == null)
                                            itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                        itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                                    }

                                    else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                                        && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        //AddedDate
                                        itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                        itemDetail.BaseQty = itemDetail.NetQty;
                                        itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                        itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                        itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                        if (itemDetail.TotalTaxAmount == null)
                                            itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                        itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;


                                    }
                                    else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                                    {

                                        //added date 22-10-2019
                                        itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                        itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);
                                        itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                        itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                        itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                        if (itemDetail.TotalTaxAmount == null)
                                            itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                        itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;


                                    }
                                    else
                                    {

                                        //Added Date :22-10-2019

                                        itemDetail.BaseUoMFactor = 1;
                                        itemDetail.BaseQty = itemDetail.NetQty;
                                        itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                        itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                        itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                        if (itemDetail.TotalTaxAmount == null)
                                            itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                        itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                                    }
                                    // Insert in receive detail
                                    if (string.IsNullOrEmpty(itemDetail.Id))
                                    {
                                        var NewId = entity.Id + "-";
                                        currentId1++;
                                        grndId = NewId + currentId1;
                                        var receiveDetail = new InventoryReceiveDetail
                                        {

                                            Id = NewId + currentId1,
                                            MaterialStorageId = itemDetail.MaterialStorageId,
                                            InventoryReceiveId = entity.Id,
                                            TransactionQty = itemDetail.NetQty,
                                            TransactionUoMId = itemDetail.TransactionUoMId,
                                            BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                            BaseUOMId = itemDetail.BaseUOMId,
                                            BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                            MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                            MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                                            TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                                            TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                                            POID = itemDetail.POID,
                                            PODetailsID = itemDetail.PODetailsID,
                                            TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                                            ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                                            ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTaxTranAmount), 2),
                                            IssueQty = 0,
                                            BaseIssueQty = 0,
                                            TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                            BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 2),
                                            PurchaseDocumentAcceptanceId = itemDetail.PurchaseDocumentAcceptanceId,
                                            PurchaseDocumentAcceptanceDetailId = itemDetail.PurchaseDocumentAcceptanceDetailId,
                                            PurchaseReturnQty = 0,
                                            IssueReturnQty = 0,
                                            InventorySalesQty = 0,
                                            InventoryScrapQty = 0,
                                            MaterialMasterOpeningBalanceDetailId = null,
                                            LotNumber = itemDetail.LotNumber,
                                            LotNo = itemDetail.LotNumber,
                                            Diameter = itemDetail.Diameter,
                                            Type = itemDetail.Type,
                                            ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                            RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                            ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),
                                            ShortageRatePercent = 110,
                                            ShortageValue = Math.Round(Convert.ToDecimal(itemDetail.ShortageValue), 2),
                                            RejectRatePercent = 50,
                                            GRNQty = itemDetail.TransactionQty,
                                            GRNTotalAmount = Math.Round(itemDetail.TransactionQty * Convert.ToDecimal(itemDetail.TransactionRate), 2),
                                            IsAsset = itemDetail.IsAsset,
                                            GrossAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                            DiscountAmount = Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                            QualityStatus = itemDetail.QualityStatus,
                                            JWTransformationPOId = itemDetail.JWTransformationPOId,
                                            JWTransformationPODetailId = itemDetail.JWTransformationPODetailId,
                                            JWTransformationPOInputMaterialId = null,
                                            JWTransformationPOByProductId = null,
                                            MaterialFor = "JobWorkOUTPUTMaterial"

                                        };
                                        try
                                        {
                                            itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                                            receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                            receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                            receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);
                                            AuditService.AddedLog(receiveDetail);
                                            itemDetail.TotalQty = ((Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty + itemDetail.IssueReturnQty)) - (Convert.ToDecimal(itemDetail.IssueQty) + Convert.ToDecimal(itemDetail.PurchaseReturnQty) + Convert.ToDecimal(itemDetail.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetail.InventorySalesQty) + Convert.ToDecimal(itemDetail.InventoryScrapQty) + Convert.ToDecimal(itemDetail.InventoryTransferQty)));
                                            itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetail.TotalQty);
                                            itemDetail.ShortageQty = Convert.ToDecimal(receiveDetail.ShortageQty + ShortageQty);
                                            itemDetail.RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty + RejectionQty);
                                            itemDetail.ApprovedQty = Convert.ToDecimal(receiveDetail.ApprovedQty + ApprovedQty);
                                            _inventoryMaterialMasterService.JWInsertOrUpdateFromReceive(itemDetail);
                                            receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                                            InsertGraph(receiveDetail);

                                            int rejectDetailId = 1;
                                            var RejectionDetails = new GRNRejectionDetails
                                            {
                                                Id = grndId.ToString() + rejectDetailId,
                                                GRNDeailsId = grndId,
                                                RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty),
                                                RejectionUoMId = itemDetail.TransactionUoMId,
                                                BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                                BaseUOMId = itemDetail.BaseUOMId,
                                                RejectionRate = Convert.ToDecimal(receiveDetail.RejectRatePercent),
                                                RejeactionValue = Convert.ToDecimal(receiveDetail.RejectValue),
                                            };
                                            AuditService.AddedLog(RejectionDetails);
                                            _gRNRejectionDetailsRepository.Insert(RejectionDetails);

                                        }
                                        catch (DivideByZeroException ex)
                                        {

                                        }
                                        finally
                                        {

                                        }
                                    }
                                }

                            }

                        }
                    }



                    if (entityMatByProduct.IsNotNull())
                    {

                        foreach (var itemDetailNew in entityMatByProduct)
                        {
                            if (itemDetailNew.ArticleId.IsNotNull())
                            {
                                itemDetailNew.CompanyGroupId = identity.CompanyGroupId;
                                itemDetailNew.CompanyId = identity.CompanyId;
                                itemDetailNew.PlantId = identity.PlantId;
                                Temppodetailid = itemDetailNew.InventoryReceiveDetailId;
                                itemDetailNew.IsNonCreditable = entity.IsNonCreditable;
                                if (CheckItemExist(itemDetailNew))
                                    throw new CustomException(itemDetailNew.MaterialMasterName + " already received");

                                ResetCurrencyRate(itemDetailNew);

                                if (itemDetailNew.IsNotNull())
                                {
                                    var materialData = _inventoryMaterialMasterService.JWGetInventoryMaterialByUpToSku(itemDetailNew);
                                    if (materialData.IsNotNull()) itemDetailNew.InventoryMaterialId = materialData.Id;
                                    ///TODO : Get total qyt and amount by country and issue qty
                                    itemDetailNew.TotalQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.BaseQty).Sum();
                                    itemDetailNew.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.IssueQty).Sum());

                                    itemDetailNew.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.PurchaseReturnQty).Sum());
                                    itemDetailNew.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.IssueReturnQty).Sum());
                                    itemDetailNew.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                                    itemDetailNew.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventorySalesQty).Sum());
                                    itemDetailNew.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventoryScrapQty).Sum());
                                    itemDetailNew.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventoryTransferQty).Sum());

                                    var ShortageQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ShortageQty).Sum();
                                    var RejectionQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.RejectionQty).Sum();
                                    var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ApprovedQty).Sum();


                                    var totalAmount = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                                    var materialMasterIds = new string[] { itemDetailNew.MaterialMasterId };
                                    var altUomIds = new string[] { itemDetailNew.TransactionUoMId };
                                    var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                                    if (itemDetailNew.BaseUOMId != itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId != itemDetailNew.BaseCurrencyId
                                         && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        ///Added Date 22-10-19
                                        itemDetailNew.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetailNew.BaseUOMId && t.AlternativeUOMId == itemDetailNew.TransactionUoMId).BaseUOMFactor);
                                        itemDetailNew.BaseQty = Convert.ToDecimal(itemDetailNew.NetQty * itemDetailNew.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;

                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;

                                    }
                                    else if (itemDetailNew.BaseUOMId == itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId != itemDetailNew.BaseCurrencyId)
                                    {

                                        //added date 22-10-2019
                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = Convert.ToDecimal(itemDetailNew.NetQty * itemDetailNew.BaseUoMFactor);
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;

                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;


                                    }
                                    else if (itemDetailNew.BaseUOMId != itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId == itemDetailNew.BaseCurrencyId
                                        && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        //AddedDate
                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = itemDetailNew.NetQty;
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;
                                    }
                                    else
                                    {
                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = itemDetailNew.NetQty;
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;
                                    }

                                    if (string.IsNullOrEmpty(itemDetailNew.Id))
                                    {
                                        var NewId = entity.Id + "-";
                                        currentId1++;
                                        grndId = NewId + currentId1;
                                        var receiveDetail = new InventoryReceiveDetail
                                        {
                                            Id = NewId + currentId1,
                                            MaterialStorageId = itemDetailNew.MaterialStorageId,
                                            InventoryReceiveId = entity.Id,
                                            TransactionQty = itemDetailNew.NetQty,
                                            TransactionUoMId = itemDetailNew.TransactionUoMId,
                                            BaseQty = Convert.ToDecimal(itemDetailNew.BaseQty),
                                            BaseUOMId = itemDetailNew.BaseUOMId,
                                            BaseUoMFactor = Convert.ToDecimal(itemDetailNew.BaseUoMFactor),
                                            MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetailNew.TransactionRate), 4),
                                            MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TrnAmount), 2),
                                            TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TotalMaterialTranAmount), 2),
                                            TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TotalMaterialBooksCurrencyAmount), 2),
                                            POID = itemDetailNew.POID,
                                            PODetailsID = itemDetailNew.PODetailsID,
                                            TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetailNew.BaseTaxAmount), 2),
                                            ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.ChargesTranAmount), 2),
                                            ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.ChargesTaxTranAmount), 2),
                                            IssueQty = 0,
                                            BaseIssueQty = 0,
                                            TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetailNew.TrnCurrencyBaseRate), 4),
                                            BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetailNew.BooksCurrencyBaseRate), 2),
                                            PurchaseDocumentAcceptanceId = itemDetailNew.PurchaseDocumentAcceptanceId,
                                            PurchaseDocumentAcceptanceDetailId = itemDetailNew.PurchaseDocumentAcceptanceDetailId,
                                            PurchaseReturnQty = 0,
                                            IssueReturnQty = 0,
                                            InventorySalesQty = 0,
                                            InventoryScrapQty = 0,
                                            MaterialMasterOpeningBalanceDetailId = null,
                                            LotNumber = itemDetailNew.LotNumber,
                                            LotNo = itemDetailNew.LotNumber,
                                            Diameter = itemDetailNew.Diameter,
                                            Type = itemDetailNew.Type,
                                            ShortageQty = Convert.ToDecimal(itemDetailNew.ShortageQty),
                                            RejectionQty = Convert.ToDecimal(itemDetailNew.RejectionQty),
                                            ApprovedQty = Convert.ToDecimal(itemDetailNew.ApprovedQty),
                                            ShortageRatePercent = 110,
                                            ShortageValue = Math.Round(Convert.ToDecimal(itemDetailNew.ShortageValue), 2),
                                            RejectRatePercent = 50,
                                            GRNQty = itemDetailNew.TransactionQty,
                                            GRNTotalAmount = Math.Round(itemDetailNew.TransactionQty * Convert.ToDecimal(itemDetailNew.TransactionRate), 2),
                                            IsAsset = itemDetailNew.IsAsset,
                                            GrossAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetailNew.DiscountAmount), 2),
                                            DiscountAmount = Math.Round(Convert.ToDecimal(itemDetailNew.DiscountAmount), 2),
                                            QualityStatus = itemDetailNew.QualityStatus,
                                            JWTransformationPOId = null,
                                            JWTransformationPODetailId = null,
                                            JWTransformationPOInputMaterialId = itemDetailNew.JWTransformationPOInputMaterialId,
                                            JWTransformationPOByProductId = itemDetailNew.JWTransformationPOByProductId,
                                            MaterialFor = "JobWorkBYPRODUCTMaterial"
                                        };
                                        try
                                        {

                                            itemDetailNew.InventoryReceiveDetailId = receiveDetail.Id;
                                            receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetailNew.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetailNew.TransactionRate), 2);
                                            receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetailNew.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetailNew.TransactionRate), 2);
                                            receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);
                                            AuditService.AddedLog(receiveDetail);
                                            itemDetailNew.TotalQty = ((Convert.ToDecimal(itemDetailNew.TotalQty + itemDetailNew.BaseQty + itemDetailNew.IssueReturnQty)) - (Convert.ToDecimal(itemDetailNew.IssueQty) + Convert.ToDecimal(itemDetailNew.PurchaseReturnQty) + Convert.ToDecimal(itemDetailNew.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetailNew.InventorySalesQty) + Convert.ToDecimal(itemDetailNew.InventoryScrapQty) + Convert.ToDecimal(itemDetailNew.InventoryTransferQty)));
                                            itemDetailNew.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetailNew.TotalQty);
                                            itemDetailNew.ShortageQty = Convert.ToDecimal(receiveDetail.ShortageQty + ShortageQty);
                                            itemDetailNew.RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty + RejectionQty);
                                            itemDetailNew.ApprovedQty = Convert.ToDecimal(receiveDetail.ApprovedQty + ApprovedQty);

                                            _inventoryMaterialMasterService.JWInsertOrUpdateFromReceive(itemDetailNew);
                                            receiveDetail.InventoryMaterialId = itemDetailNew.InventoryMaterialId;
                                            InsertGraph(receiveDetail);

                                            int rejectDetailId = 1;
                                            var RejectionDetails = new GRNRejectionDetails
                                            {
                                                Id = grndId.ToString() + rejectDetailId,
                                                GRNDeailsId = grndId,
                                                RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty),
                                                RejectionUoMId = itemDetailNew.TransactionUoMId,
                                                BaseUoMFactor = Convert.ToDecimal(itemDetailNew.BaseUoMFactor),
                                                BaseUOMId = itemDetailNew.BaseUOMId,
                                                RejectionRate = Convert.ToDecimal(receiveDetail.RejectRatePercent),
                                                RejeactionValue = Convert.ToDecimal(receiveDetail.RejectValue),
                                            };
                                            AuditService.AddedLog(RejectionDetails);
                                            _gRNRejectionDetailsRepository.Insert(RejectionDetails);
                                        }
                                        catch (DivideByZeroException ex)
                                        {

                                        }
                                        finally
                                        {

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {

                    _inventoryReceiveService.Update(entity);
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    var Temppodetailid = "";
                    if (entityMat.IsNotNull())
                    {
                        foreach (var itemDetail in entityMat)
                        {
                            if (itemDetail.ArticleId.IsNotNull())
                            {
                                itemDetail.CompanyGroupId = identity.CompanyGroupId;
                                itemDetail.CompanyId = identity.CompanyId;
                                itemDetail.PlantId = identity.PlantId;
                                Temppodetailid = itemDetail.InventoryReceiveDetailId;
                                itemDetail.IsNonCreditable = entity.IsNonCreditable;
                                if (CheckItemExist(itemDetail))
                                    throw new CustomException(itemDetail.MaterialMasterName + " already received");

                                ResetCurrencyRate(itemDetail);

                                if (itemDetail.IsNotNull())
                                {
                                    var materialData = _inventoryMaterialMasterService.JWGetInventoryMaterialByUpToSku(itemDetail);
                                    if (materialData.IsNotNull()) itemDetail.InventoryMaterialId = materialData.Id;
                                    ///TODO : Get total qyt and amount by country and issue qty
                                    itemDetail.TotalQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.BaseQty).Sum();
                                    itemDetail.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueQty).Sum());

                                    itemDetail.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.PurchaseReturnQty).Sum());
                                    itemDetail.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.IssueReturnQty).Sum());
                                    itemDetail.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                                    itemDetail.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventorySalesQty).Sum());
                                    itemDetail.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryScrapQty).Sum());
                                    itemDetail.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.InventoryTransferQty).Sum());

                                    var ShortageQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ShortageQty).Sum();
                                    var RejectionQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.RejectionQty).Sum();
                                    var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.ApprovedQty).Sum();


                                    var totalAmount = Query(t => t.InventoryMaterialId == itemDetail.InventoryMaterialId && t.Id != itemDetail.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                                    var materialMasterIds = new string[] { itemDetail.MaterialMasterId };
                                    var altUomIds = new string[] { itemDetail.TransactionUoMId };
                                    var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                                    if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId
                                         && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        ///Added Date 22-10-19
                                        itemDetail.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetail.BaseUOMId && t.AlternativeUOMId == itemDetail.TransactionUoMId).BaseUOMFactor);
                                        itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                        itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                        itemDetail.ChargesTranAmount = itemDetail.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                        itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                        if (itemDetail.TotalTaxAmount == null)
                                            itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                        itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;

                                    }
                                    else if (itemDetail.BaseUOMId == itemDetail.TransactionUoMId && itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                                    {

                                        //added date 22-10-2019
                                        itemDetail.BaseUoMFactor = 1;
                                        itemDetail.BaseQty = Convert.ToDecimal(itemDetail.NetQty * itemDetail.BaseUoMFactor);
                                        itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                        itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                        itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                        if (itemDetail.TotalTaxAmount == null)
                                            itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                        itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;

                                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;


                                    }
                                    else if (itemDetail.BaseUOMId != itemDetail.TransactionUoMId && itemDetail.CurrencyId == itemDetail.BaseCurrencyId
                                        && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {
                                        //AddedDate
                                        itemDetail.BaseUoMFactor = 1;
                                        itemDetail.BaseQty = itemDetail.NetQty;
                                        itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                        itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                        itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                        if (itemDetail.TotalTaxAmount == null)
                                            itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                        itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                                    }
                                    else
                                    {

                                        //Added Date :22-10-2019
                                        itemDetail.BaseUoMFactor = 1;
                                        itemDetail.BaseQty = itemDetail.NetQty;
                                        itemDetail.TotalMaterialTranAmount = itemDetail.TrnAmount;
                                        itemDetail.ChargesTranAmount = itemDetail.ServiceCharge;
                                        itemDetail.ChargesTaxTranAmount = itemDetail.ServiceTax;
                                        if (itemDetail.TotalTaxAmount == null)
                                            itemDetail.TotalTaxAmount = itemDetail.BaseTaxAmount;
                                        itemDetail.TotalMaterialTranAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetail.ChargesTranAmount);
                                        itemDetail.TotalMaterialBooksCurrencyAmount = itemDetail.TrnAmount * itemDetail.ToCurrencyRate;
                                        itemDetail.TotalMaterialBooksCurrencyAmount += itemDetail.IsNonCreditable ? Convert.ToDecimal(itemDetail.TotalTaxAmount + itemDetail.ChargesTranAmount + itemDetail.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetail.ChargesTranAmount) * Convert.ToDecimal(itemDetail.ToCurrencyRate);
                                        itemDetail.TrnCurrencyBaseRate = itemDetail.TotalMaterialTranAmount / itemDetail.BaseQty;
                                        itemDetail.BooksCurrencyBaseRate = itemDetail.TotalMaterialBooksCurrencyAmount / itemDetail.BaseQty;
                                    }

                                    var NewId = entity.Id + "-";
                                    var receiveDetail = new InventoryReceiveDetail
                                    {
                                        Id = itemDetail.InventoryReceiveDetailId,
                                        MaterialStorageId = itemDetail.MaterialStorageId,
                                        InventoryReceiveId = entity.Id,
                                        TransactionQty = itemDetail.NetQty,
                                        TransactionUoMId = itemDetail.TransactionUoMId,
                                        BaseQty = Convert.ToDecimal(itemDetail.BaseQty),
                                        BaseUOMId = itemDetail.BaseUOMId,
                                        BaseUoMFactor = Convert.ToDecimal(itemDetail.BaseUoMFactor),
                                        MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetail.TransactionRate), 4),
                                        MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2),
                                        TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialTranAmount), 2),
                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetail.TotalMaterialBooksCurrencyAmount), 2),
                                        POID = itemDetail.POID,
                                        PODetailsID = itemDetail.PODetailsID,
                                        TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetail.BaseTaxAmount), 2),
                                        ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTranAmount), 2),
                                        ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetail.ChargesTaxTranAmount), 2),
                                        IssueQty = 0,
                                        BaseIssueQty = 0,
                                        TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.TrnCurrencyBaseRate), 4),
                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetail.BooksCurrencyBaseRate), 2),
                                        PurchaseDocumentAcceptanceId = itemDetail.PurchaseDocumentAcceptanceId,
                                        PurchaseDocumentAcceptanceDetailId = itemDetail.PurchaseDocumentAcceptanceDetailId,
                                        PurchaseReturnQty = 0,
                                        IssueReturnQty = 0,
                                        InventorySalesQty = 0,
                                        InventoryScrapQty = 0,
                                        MaterialMasterOpeningBalanceDetailId = null,
                                        LotNumber = itemDetail.LotNumber,
                                        LotNo = itemDetail.LotNumber,
                                        Diameter = itemDetail.Diameter,
                                        Type = itemDetail.Type,
                                        ShortageQty = Convert.ToDecimal(itemDetail.ShortageQty),
                                        RejectionQty = Convert.ToDecimal(itemDetail.RejectionQty),
                                        ApprovedQty = Convert.ToDecimal(itemDetail.ApprovedQty),
                                        ShortageRatePercent = 110,
                                        ShortageValue = Math.Round(Convert.ToDecimal(itemDetail.ShortageValue), 2),
                                        RejectRatePercent = 50,
                                        GRNQty = itemDetail.TransactionQty,
                                        GRNTotalAmount = Math.Round(itemDetail.TransactionQty * Convert.ToDecimal(itemDetail.TransactionRate), 2),
                                        IsAsset = itemDetail.IsAsset,
                                        GrossAmount = Math.Round(Convert.ToDecimal(itemDetail.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                        DiscountAmount = Math.Round(Convert.ToDecimal(itemDetail.DiscountAmount), 2),
                                        QualityStatus = itemDetail.QualityStatus,
                                        JWTransformationPOId = itemDetail.JWTransformationPOId,
                                        JWTransformationPODetailId = itemDetail.JWTransformationPODetailId,
                                        JWTransformationPOInputMaterialId = null,
                                        JWTransformationPOByProductId = null,
                                        MaterialFor = "JobWorkOUTPUTMaterial"
                                    };
                                    try
                                    {

                                        itemDetail.InventoryReceiveDetailId = receiveDetail.Id;
                                        receiveDetail.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetail.ShortageQty) * receiveDetail.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                        receiveDetail.RejectValue = Math.Round(((Convert.ToDecimal(itemDetail.RejectionQty) * receiveDetail.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetail.TransactionRate), 2);
                                        receiveDetail.RejectClamPercent = (100 - receiveDetail.RejectRatePercent);

                                        AuditService.UpdatedLog(receiveDetail);
                                        itemDetail.TotalQty = ((Convert.ToDecimal(itemDetail.TotalQty + itemDetail.BaseQty + itemDetail.IssueReturnQty)) - (Convert.ToDecimal(itemDetail.IssueQty) + Convert.ToDecimal(itemDetail.PurchaseReturnQty) + Convert.ToDecimal(itemDetail.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetail.InventorySalesQty) + Convert.ToDecimal(itemDetail.InventoryScrapQty) + Convert.ToDecimal(itemDetail.InventoryTransferQty)));
                                        itemDetail.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail.TotalMaterialTranAmount) / itemDetail.TotalQty);
                                        itemDetail.ShortageQty = Convert.ToDecimal(receiveDetail.ShortageQty + ShortageQty);
                                        itemDetail.RejectionQty = Convert.ToDecimal(receiveDetail.RejectionQty + RejectionQty);
                                        itemDetail.ApprovedQty = Convert.ToDecimal(receiveDetail.ApprovedQty + ApprovedQty);
                                        _inventoryMaterialMasterService.JWInsertOrUpdateFromReceive(itemDetail);
                                        receiveDetail.InventoryMaterialId = itemDetail.InventoryMaterialId;
                                        UpdateGraph(receiveDetail);

                                    }
                                    catch (DivideByZeroException ex)
                                    {

                                    }
                                    finally
                                    {

                                    }

                                }


                            }
                        }
                    }

                    if (entityMatByProduct.IsNotNull())
                    {

                        foreach (var itemDetailNew in entityMatByProduct)
                        {
                            if (itemDetailNew.ArticleId.IsNotNull())
                            {
                                itemDetailNew.CompanyGroupId = identity.CompanyGroupId;
                                itemDetailNew.CompanyId = identity.CompanyId;
                                itemDetailNew.PlantId = identity.PlantId;
                                Temppodetailid = itemDetailNew.InventoryReceiveDetailId;
                                itemDetailNew.IsNonCreditable = entity.IsNonCreditable;
                                if (CheckItemExist(itemDetailNew))
                                    throw new CustomException(itemDetailNew.MaterialMasterName + " already received");

                                ResetCurrencyRate(itemDetailNew);

                                if (itemDetailNew.IsNotNull())
                                {
                                    var materialData = _inventoryMaterialMasterService.JWGetInventoryMaterialByUpToSku(itemDetailNew);
                                    if (materialData.IsNotNull()) itemDetailNew.InventoryMaterialId = materialData.Id;
                                    ///TODO : Get total qyt and amount by country and issue qty
                                    itemDetailNew.TotalQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.BaseQty).Sum();
                                    itemDetailNew.IssueQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.IssueQty).Sum());

                                    itemDetailNew.PurchaseReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.PurchaseReturnQty).Sum());
                                    itemDetailNew.IssueReturnQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.IssueReturnQty).Sum());
                                    itemDetailNew.ReductionByAdjustmentQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ReductionByAdjustmentQty).Sum());

                                    itemDetailNew.InventorySalesQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventorySalesQty).Sum());
                                    itemDetailNew.InventoryScrapQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventoryScrapQty).Sum());
                                    itemDetailNew.InventoryTransferQty = Convert.ToDecimal(Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.InventoryTransferQty).Sum());

                                    var ShortageQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ShortageQty).Sum();
                                    var RejectionQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.RejectionQty).Sum();
                                    var ApprovedQty = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.ApprovedQty).Sum();


                                    var totalAmount = Query(t => t.InventoryMaterialId == itemDetailNew.InventoryMaterialId && t.Id != itemDetailNew.Id).Select(t => t.TotalMaterialTranAmount).Sum();

                                    var materialMasterIds = new string[] { itemDetailNew.MaterialMasterId };
                                    var altUomIds = new string[] { itemDetailNew.TransactionUoMId };
                                    var baseUoMFactorList = _materialMasterService.GetBaseUoMConvertionFactorByMaterialMaster(materialMasterIds, altUomIds);

                                    if (itemDetailNew.BaseUOMId != itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId != itemDetailNew.BaseCurrencyId
                                         && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        ///Added Date 22-10-19
                                        itemDetailNew.BaseUoMFactor = Convert.ToDecimal(baseUoMFactorList.FirstOrDefault(t => t.BaseUOMId == itemDetailNew.BaseUOMId && t.AlternativeUOMId == itemDetailNew.TransactionUoMId).BaseUOMFactor);
                                        itemDetailNew.BaseQty = Convert.ToDecimal(itemDetailNew.NetQty * itemDetailNew.BaseUoMFactor);//Convert.ToDecimal(itemDetail.TransactionQty * itemDetail.BaseUoMFactor);
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge; //itemDetail.TrnAmount * ratio;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;//itemDetail.TrnAmount * ratioServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;

                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;

                                    }
                                    else if (itemDetailNew.BaseUOMId == itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId != itemDetailNew.BaseCurrencyId)
                                    {

                                        //added date 22-10-2019
                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = Convert.ToDecimal(itemDetailNew.NetQty * itemDetailNew.BaseUoMFactor);
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;

                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;


                                    }
                                    else if (itemDetailNew.BaseUOMId != itemDetailNew.TransactionUoMId && itemDetailNew.CurrencyId == itemDetailNew.BaseCurrencyId
                                        && (baseUoMFactorList != null && baseUoMFactorList.Count() > 0))
                                    {

                                        //AddedDate
                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = itemDetailNew.NetQty;
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;


                                    }
                                    else
                                    {

                                        //Added Date :22-10-2019

                                        itemDetailNew.BaseUoMFactor = 1;
                                        itemDetailNew.BaseQty = itemDetailNew.NetQty;
                                        itemDetailNew.TotalMaterialTranAmount = itemDetailNew.TrnAmount;
                                        itemDetailNew.ChargesTranAmount = itemDetailNew.ServiceCharge;
                                        itemDetailNew.ChargesTaxTranAmount = itemDetailNew.ServiceTax;
                                        if (itemDetailNew.TotalTaxAmount == null)
                                            itemDetailNew.TotalTaxAmount = itemDetailNew.BaseTaxAmount;
                                        itemDetailNew.TotalMaterialTranAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) :
                                          Convert.ToDecimal(itemDetailNew.ChargesTranAmount);
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount = itemDetailNew.TrnAmount * itemDetailNew.ToCurrencyRate;
                                        itemDetailNew.TotalMaterialBooksCurrencyAmount += itemDetailNew.IsNonCreditable ? Convert.ToDecimal(itemDetailNew.TotalTaxAmount + itemDetailNew.ChargesTranAmount + itemDetailNew.ChargesTaxTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate) :
                                                 Convert.ToDecimal(itemDetailNew.ChargesTranAmount) * Convert.ToDecimal(itemDetailNew.ToCurrencyRate);
                                        itemDetailNew.TrnCurrencyBaseRate = itemDetailNew.TotalMaterialTranAmount / itemDetailNew.BaseQty;
                                        itemDetailNew.BooksCurrencyBaseRate = itemDetailNew.TotalMaterialBooksCurrencyAmount / itemDetailNew.BaseQty;

                                    }
                                    var NewId = entity.Id + "-";

                                    var receiveDetail1 = new InventoryReceiveDetail
                                    {
                                        Id = itemDetailNew.InventoryReceiveDetailId,
                                        MaterialStorageId = itemDetailNew.MaterialStorageId,
                                        InventoryReceiveId = entity.Id,

                                        TransactionQty = itemDetailNew.NetQty,
                                        TransactionUoMId = itemDetailNew.TransactionUoMId,
                                        BaseQty = Convert.ToDecimal(itemDetailNew.BaseQty),
                                        BaseUOMId = itemDetailNew.BaseUOMId,
                                        BaseUoMFactor = Convert.ToDecimal(itemDetailNew.BaseUoMFactor),
                                        MaterialTranRate = Math.Round(Convert.ToDecimal(itemDetailNew.TransactionRate), 4),
                                        MaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TrnAmount), 2),
                                        TotalMaterialTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TotalMaterialTranAmount), 2),
                                        TotalMaterialBooksCurrencyAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TotalMaterialBooksCurrencyAmount), 2),
                                        POID = itemDetailNew.POID,
                                        PODetailsID = itemDetailNew.PODetailsID,
                                        TotalTaxAmount = Math.Round(Convert.ToDecimal(itemDetailNew.BaseTaxAmount), 2),
                                        ChargesTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.ChargesTranAmount), 2),
                                        ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(itemDetailNew.ChargesTaxTranAmount), 2),
                                        IssueQty = 0,
                                        BaseIssueQty = 0,
                                        TrnCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetailNew.TrnCurrencyBaseRate), 4),
                                        BooksCurrencyBaseRate = Math.Round(Convert.ToDecimal(itemDetailNew.BooksCurrencyBaseRate), 2),
                                        PurchaseDocumentAcceptanceId = itemDetailNew.PurchaseDocumentAcceptanceId,
                                        PurchaseDocumentAcceptanceDetailId = itemDetailNew.PurchaseDocumentAcceptanceDetailId,
                                        PurchaseReturnQty = 0,
                                        IssueReturnQty = 0,
                                        InventorySalesQty = 0,
                                        InventoryScrapQty = 0,
                                        MaterialMasterOpeningBalanceDetailId = null,
                                        LotNumber = itemDetailNew.LotNumber,
                                        LotNo = itemDetailNew.LotNumber,
                                        Diameter = itemDetailNew.Diameter,
                                        Type = itemDetailNew.Type,
                                        ShortageQty = Convert.ToDecimal(itemDetailNew.ShortageQty),
                                        RejectionQty = Convert.ToDecimal(itemDetailNew.RejectionQty),
                                        ApprovedQty = Convert.ToDecimal(itemDetailNew.ApprovedQty),
                                        ShortageRatePercent = 110,
                                        ShortageValue = Math.Round(Convert.ToDecimal(itemDetailNew.ShortageValue), 2),
                                        RejectRatePercent = 50,
                                        GRNQty = itemDetailNew.TransactionQty,
                                        GRNTotalAmount = Math.Round(itemDetailNew.TransactionQty * Convert.ToDecimal(itemDetailNew.TransactionRate), 2),
                                        IsAsset = itemDetailNew.IsAsset,
                                        GrossAmount = Math.Round(Convert.ToDecimal(itemDetailNew.TrnAmount), 2) + Math.Round(Convert.ToDecimal(itemDetailNew.DiscountAmount), 2),
                                        DiscountAmount = Math.Round(Convert.ToDecimal(itemDetailNew.DiscountAmount), 2),
                                        QualityStatus = itemDetailNew.QualityStatus,
                                        JWTransformationPOId = null,
                                        JWTransformationPODetailId = null,
                                        JWTransformationPOInputMaterialId = itemDetailNew.JWTransformationPOInputMaterialId,
                                        JWTransformationPOByProductId = itemDetailNew.JWTransformationPOByProductId,
                                        MaterialFor = "JobWorkBYPRODUCTMaterial"
                                    };
                                    try
                                    {

                                        itemDetailNew.InventoryReceiveDetailId = receiveDetail1.Id;
                                        receiveDetail1.ShortageValue = Math.Round(((Convert.ToDecimal(itemDetailNew.ShortageQty) * receiveDetail1.ShortageRatePercent) / 100) * Convert.ToDecimal(itemDetailNew.TransactionRate), 2);
                                        receiveDetail1.RejectValue = Math.Round(((Convert.ToDecimal(itemDetailNew.RejectionQty) * receiveDetail1.RejectRatePercent) / 100) * Convert.ToDecimal(itemDetailNew.TransactionRate), 2);
                                        receiveDetail1.RejectClamPercent = (100 - receiveDetail1.RejectRatePercent);
                                        AuditService.UpdatedLog(receiveDetail1);
                                        itemDetailNew.TotalQty = ((Convert.ToDecimal(itemDetailNew.TotalQty + itemDetailNew.BaseQty + itemDetailNew.IssueReturnQty)) - (Convert.ToDecimal(itemDetailNew.IssueQty) + Convert.ToDecimal(itemDetailNew.PurchaseReturnQty) + Convert.ToDecimal(itemDetailNew.ReductionByAdjustmentQty) + Convert.ToDecimal(itemDetailNew.InventorySalesQty) + Convert.ToDecimal(itemDetailNew.InventoryScrapQty) + Convert.ToDecimal(itemDetailNew.InventoryTransferQty)));
                                        itemDetailNew.AvgRate = Convert.ToDecimal((totalAmount + receiveDetail1.TotalMaterialTranAmount) / itemDetailNew.TotalQty);

                                        itemDetailNew.ShortageQty = Convert.ToDecimal(receiveDetail1.ShortageQty + ShortageQty);
                                        itemDetailNew.RejectionQty = Convert.ToDecimal(receiveDetail1.RejectionQty + RejectionQty);
                                        itemDetailNew.ApprovedQty = Convert.ToDecimal(receiveDetail1.ApprovedQty + ApprovedQty);
                                        _inventoryMaterialMasterService.JWInsertOrUpdateFromReceive(itemDetailNew);
                                        receiveDetail1.InventoryMaterialId = itemDetailNew.InventoryMaterialId;
                                        UpdateGraph(receiveDetail1);

                                    }
                                    catch (DivideByZeroException ex)
                                    {

                                    }
                                    finally
                                    {

                                    }
                                }
                            }
                        }
                    }

                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
                SaveJobWorkReceiptTransformationWOMaterial(entityMat, entity.Id);
                SaveJobWorkReceiptByProductWOMaterial(entityMatByProduct, entity.Id);
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

        private string GetJWWithoutMatPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceiveDetail", out sID);
            return sID;
        }

        public void SaveJobWorkReceiptTransformationWOMaterial(IEnumerable<InventoryMaterialViewModel> entityMat, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //      var JWItemId = "' '";
                var OtMatId = "' '";

                foreach (var empitem in entityMat)
                {
                    if (empitem.ArticleId.IsNull())
                    {
                        //         JWItemId += ",'" + empitem.JWInputItemId + "' ";
                        OtMatId += ",'" + empitem.JWTransformationPODetailId + "' ";
                    }
                }
                con.OpenDataSetThroughAdapter("select * from TRN.InventoryReceiveDetail where JWTransformationPODetailId IN ( " + OtMatId + ") and InventoryReceiveId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in entityMat)
                {
                    if (item.ArticleId.IsNull())
                    {

                        ExistOrNot.Tables[0].DefaultView.RowFilter = "JWTransformationPODetailId='" + item.JWTransformationPODetailId + "' and InventoryReceiveId='" + MasterId + "' ";

                        if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow dr = ExistOrNot.Tables[0].NewRow();
                            dr["Id"] = GetJWWithoutMatPK();

                            dr["InventoryReceiveId"] = MasterId;
                            dr["TransactionQty"] = item.TransactionQty;
                            dr["TransactionUoMId"] = item.TransactionUoMId;
                            dr["BaseUOMId"] = item.TransactionUoMId;
                            dr["BaseQty"] = item.TransactionQty;
                            dr["QualityStatus"] = item.QualityStatus;
                            dr["JWTransformationPOId"] = item.JWTransformationPOId;
                            dr["JWTransformationPODetailId"] = item.JWTransformationPODetailId;
                            dr["MaterialFor"] = "JobWorkOUTPUTMaterial";

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            ExistOrNot.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            ExistOrNot.Tables[0].DefaultView.RowFilter = "JWTransformationPODetailId='" + item.JWTransformationPODetailId + "' and InventoryReceiveId='" + MasterId + "' ";

                            if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = ExistOrNot.Tables[0].NewRow();
                                dr["Id"] = GetJWWithoutMatPK();

                                dr["InventoryReceiveId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.TransactionUoMId;
                                dr["BaseQty"] = item.TransactionQty;
                                dr["QualityStatus"] = item.QualityStatus;
                                dr["JWTransformationPOId"] = item.JWTransformationPOId;
                                dr["JWTransformationPODetailId"] = item.JWTransformationPODetailId;
                                dr["MaterialFor"] = "JobWorkOUTPUTMaterial";

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;

                                ExistOrNot.Tables[0].Rows.Add(dr);

                            }
                            else
                            {
                                //edit
                                DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["InventoryReceiveId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.TransactionUoMId;
                                dr["BaseQty"] = item.TransactionQty;
                                dr["QualityStatus"] = item.QualityStatus;
                                dr["JWTransformationPOId"] = item.JWTransformationPOId;
                                dr["JWTransformationPODetailId"] = item.JWTransformationPODetailId;
                                dr["MaterialFor"] = "JobWorkOUTPUTMaterial";

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;


                                dr.EndEdit();
                            }


                        }
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetJWTransformationBYProdPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceiveDetail", out sID);
            return sID;
        }

        public void SaveJobWorkReceiptByProductWOMaterial(IEnumerable<InventoryMaterialViewModel> entityMatByProduct, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (entityMatByProduct.IsNotNull())
                {
                    DataSet ExistOrNot;

                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    var MIId = "' '";
                    var BPId = "' '";

                    foreach (var empitem in entityMatByProduct)
                    {
                        if (empitem.ArticleId.IsNull())
                        {
                            MIId += ",'" + empitem.JWTransformationPOInputMaterialId + "' ";
                            BPId += ",'" + empitem.JWTransformationPOByProductId + "' ";
                        }
                    }
                    con.OpenDataSetThroughAdapter("select * from TRN.InventoryReceiveDetail where JWTransformationPOInputMaterialId IN ( " + MIId + ") and JWTransformationPOByProductId IN (" + BPId + ") and InventoryReceiveId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                    foreach (var item in entityMatByProduct)
                    {
                        if (item.ArticleId.IsNull())
                        {

                            ExistOrNot.Tables[0].DefaultView.RowFilter = "JWTransformationPOInputMaterialId='" + item.JWTransformationPOInputMaterialId + "' and JWTransformationPOByProductId='" + item.JWTransformationPOByProductId + "' and InventoryReceiveId='" + MasterId + "' ";

                            if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = ExistOrNot.Tables[0].NewRow();
                                dr["Id"] = GetJWTransformationBYProdPK();

                                dr["InventoryReceiveId"] = MasterId;
                                dr["TransactionQty"] = item.TransactionQty;
                                dr["TransactionUoMId"] = item.TransactionUoMId;
                                dr["BaseUOMId"] = item.TransactionUoMId;
                                dr["BaseQty"] = item.TransactionQty;
                                dr["QualityStatus"] = item.QualityStatus;
                                dr["JWTransformationPOInputMaterialId"] = item.JWTransformationPOInputMaterialId;
                                dr["JWTransformationPOByProductId"] = item.JWTransformationPOByProductId;
                                dr["MaterialFor"] = "JobWorkBYPRODUCTMaterial";

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();
                                dr["AddedFromIP"] = identity.IPAddress;
                                ExistOrNot.Tables[0].Rows.Add(dr);
                            }
                            else
                            {
                                ExistOrNot.Tables[0].DefaultView.RowFilter = "JWTransformationPOInputMaterialId='" + item.JWTransformationPOInputMaterialId + "' and JWTransformationPOByProductId='" + item.JWTransformationPOByProductId + "' and InventoryReceiveId='" + MasterId + "' ";

                                if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                                {
                                    DataRow dr = ExistOrNot.Tables[0].NewRow();
                                    dr["Id"] = GetJWTransformationBYProdPK();

                                    dr["InventoryReceiveId"] = MasterId;
                                    dr["TransactionQty"] = item.TransactionQty;
                                    dr["TransactionUoMId"] = item.TransactionUoMId;
                                    dr["BaseUOMId"] = item.TransactionUoMId;
                                    dr["BaseQty"] = item.TransactionQty;
                                    dr["QualityStatus"] = item.QualityStatus;
                                    dr["JWTransformationPOInputMaterialId"] = item.JWTransformationPOInputMaterialId;
                                    dr["JWTransformationPOByProductId"] = item.JWTransformationPOByProductId;
                                    dr["MaterialFor"] = "JobWorkBYPRODUCTMaterial";

                                    dr["AddedBy"] = identity.Name;
                                    dr["AddedDate"] = System.DateTime.Now.ToString();
                                    dr["AddedFromIP"] = identity.IPAddress;

                                    ExistOrNot.Tables[0].Rows.Add(dr);

                                }
                                else
                                {
                                    //edit
                                    DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                                    dr.BeginEdit();

                                    dr["InventoryReceiveId"] = MasterId;
                                    dr["TransactionQty"] = item.TransactionQty;
                                    dr["TransactionUoMId"] = item.TransactionUoMId;
                                    dr["BaseUOMId"] = item.TransactionUoMId;
                                    dr["BaseQty"] = item.TransactionQty;
                                    dr["QualityStatus"] = item.QualityStatus;
                                    dr["JWTransformationPOInputMaterialId"] = item.JWTransformationPOInputMaterialId;
                                    dr["JWTransformationPOByProductId"] = item.JWTransformationPOByProductId;
                                    dr["MaterialFor"] = "JobWorkBYPRODUCTMaterial";

                                    dr["UpdatedBy"] = identity.Name;
                                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                    dr["UpdatedFromIP"] = identity.IPAddress;


                                    dr.EndEdit();
                                }


                            }
                        }
                    }
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(ExistOrNot);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void JWDelete(string receiveDetailId)
        {
            var flag = false;
            try
            {
                var isNonCreditable = _receiveDetailRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[InventoryReceive] AS A JOIN [TRN].[InventoryReceiveDetail] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + receiveDetailId + "'").First();
                var data = Find(receiveDetailId);
                if (data.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;

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

        public void IssueSlipDelete(string IssueslipDEtailId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";
                flag = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                builderSql = @"delete from trn.IssueRequestBOQMap where IssueRequestDetailId='" + IssueslipDEtailId + "'";
                rdBuilder.Append(builderSql);
                builderSql = @"delete from trn.IssueRequest where Id='" + IssueslipDEtailId + "'";
                rdBuilder.Append(builderSql);

                _unitOfWork.SaveChanges();
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

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
        public void IssueSlipDeleteFn(string IssueslipDEtailId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var rdBuilder = new System.Text.StringBuilder();
                var builderSql = "";
                flag = true;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                builderSql = @"Delete from trn.IssueRequestSKUMap where IssueRequestMasterId='" + IssueslipDEtailId + "'";
                rdBuilder.Append(builderSql);
                builderSql = @"Delete from trn.IssueRequestMasterSalesOrderMap where IssueRequestMasterId='" + IssueslipDEtailId + "'";
                rdBuilder.Append(builderSql);

                builderSql = @"Delete from trn.IssueRequestMasterProcessMap where IssueRequestMasterId='" + IssueslipDEtailId + "'";
                rdBuilder.Append(builderSql);

                builderSql = @"Delete from trn.IssueRequestMaster where Id='" + IssueslipDEtailId + "'";
                rdBuilder.Append(builderSql);


                _unitOfWork.SaveChanges();
                _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());

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
        public void GRNBOQDetailDelete(string receiveId,string receiveDetailId)
        {
            var flag = false;
            try
            {
                var isNonCreditable = _receiveDetailRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[InventoryReceive] AS A JOIN [TRN].[InventoryReceiveDetail] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + receiveDetailId + "'").First();
                var masterdata = _inventoryReceiveService.Find(receiveId);
                if(masterdata!=null && masterdata.Status=="Posting" && masterdata.VoucherId != null)
                {
                    throw new CustomException("Posted GRN delete is not allowed");
                }
                var data = Find(receiveDetailId);
                if (data.IsNotNull())
                {
                    _unitOfWork.BeginTransaction();
                    flag = true;
                    _inventoryMaterialMasterService.UpdateFromReceive(data.InventoryMaterialId, receiveDetailId);
                    var taxCategoryList = _receiveTaxRepository.Query(t => t.InventoryReceiveDetailId == receiveDetailId).Select().ToList();
                    if (taxCategoryList.IsNotNull())
                    {
                        foreach (var item in taxCategoryList)
                        {
                            item.ModelState = ModelState.Deleted;
                            _receiveTaxRepository.Delete(item);
                        }
                    }
                    var ratio = _inventoryReceiveService.GetChargesRatio(data.InventoryReceiveId, data.Id, 0, null, 0, isNonCreditable);
                    var ratioServiceTax = _inventoryReceiveService.GetChargesTaxRatio(data.InventoryReceiveId, data.Id, 0, null, 0, isNonCreditable);
                    UpdateInventoryDetailAfterDelete(data, ratioServiceTax, ratio, 1, isNonCreditable);

                    var PODetailData = _poDetailRepository.Find(data.PODetailsID);
                    if (PODetailData.IsNotNull())
                    {
                        PODetailData.GRNRcvQty = Convert.ToDecimal(((PODetailData.GRNRcvQty - data.GRNQty)));
                        PODetailData.QtyStatus = PODetailData.TransactionQty == PODetailData.GRNRcvQty;
                        AuditService.UpdatedLog(PODetailData);
                        _poDetailRepository.Update(PODetailData);
                    }

                    var GRNPORequisitionAllocation = _gRNPOAllocationRepository.Query(t => t.InventoryReceiveDetailId == receiveDetailId).Select().ToList();
                    if (GRNPORequisitionAllocation.IsNotNull())
                    {
                        foreach (var item in GRNPORequisitionAllocation)
                        {
                            item.ModelState = ModelState.Deleted;
                            _gRNPOAllocationRepository.Delete(item);
                        }
                    }
                    base.DeleteGraph(data);

                    ConnectionManager.DAL.ConManager objCon1;
                    DataSet dsMaster1 = null;
                    DataSet dsMaster2 = null;
                    DataSet dsMaster3 = null;
                    string setOffsql = @"SELECT * from trn.GRNPORequisitionMap where InventoryReceiveDetailId = '" + receiveDetailId + "'";
                    string grnBinAllocationMapsql = @"SELECT * from trn.GRNBinAllocationMap where InventoryReceiveDetailId = '" + receiveDetailId + "'";
                    string GRNRejectionDetailsMapsql = @"SELECT * from trn.GRNRejectionDetails where GRNDeailsId = '" + receiveDetailId + "'";
                    objCon1 = new ConnectionManager.DAL.ConManager("1");
                    objCon1.OpenDataSetThroughAdapter(setOffsql, out dsMaster1, false, "1");
                    objCon1.OpenDataSetThroughAdapter(grnBinAllocationMapsql, out dsMaster2, false, "1");
                    objCon1.OpenDataSetThroughAdapter(GRNRejectionDetailsMapsql, out dsMaster3, false, "1");

                    if (dsMaster1.Tables[0].Rows.Count > 0)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var grnPOreqSql = @"DELETE trn.GRNPORequisitionMap where InventoryReceiveDetailId ='" + receiveDetailId + "'";
                        rdBuilder.Append(grnPOreqSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }

                    if (dsMaster2.Tables[0].Rows.Count > 0)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var grnBinAllocationSql = @"DELETE trn.GRNBinAllocationMap where InventoryReceiveDetailId ='" + receiveDetailId + "'";
                        rdBuilder.Append(grnBinAllocationSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }

                    if (dsMaster3.Tables[0].Rows.Count > 0)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var GRNRejectionDetailSql = @"DELETE trn.GRNRejectionDetails where GRNDeailsId ='" + receiveDetailId + "'";
                        rdBuilder.Append(GRNRejectionDetailSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }

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
}
