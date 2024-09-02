using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.ViewModel.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Library.MaterialManagement.Inventory
{
    public class InventoryServiceService : Service<InventoryService>, IInventoryServiceService
    {
        #region Constructor

        private readonly IRepositoryAsync<InventoryReceiveTax> _receiveTaxRepository;
        private readonly IRepositoryAsync<InventoryService> _inventoryServiceRepository;
        private readonly IRepositoryAsync<POService> _POServiceRepository;
        private readonly IRepositoryAsync<InventoryService> _inventoryServicesRepository;
        private readonly IRepositoryAsync<InventoryReceiveDetail> _invRecDetailRepository;
        private readonly IInventoryReceiveService _inventoryReceiveService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly IRepositoryAsync<PurchaseDocAcceptanceTax> _PurchaseDocAcceptanceTaxRepository; 

        public InventoryServiceService(
            IRepositoryAsync<InventoryService> inventoryServiceRepository
            ,IRepositoryAsync<POService> POServiceRepository
            , IRepositoryAsync<InventoryService> inventoryServicesRepository
            , IRepositoryAsync<InventoryReceiveTax> receiveTaxRepository
            , IRepositoryAsync<InventoryReceiveDetail> invRecDetailRepository
            , IInventoryReceiveService inventoryReceiveService
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<PurchaseDocAcceptanceTax> PurchaseDocAcceptanceTaxRepository
            ) : base(inventoryServiceRepository, unitOfWork, pkGeneratorService)
        {
            _inventoryServiceRepository = inventoryServiceRepository;
            _POServiceRepository = POServiceRepository; 
            _inventoryServicesRepository = inventoryServicesRepository; 
            _invRecDetailRepository = invRecDetailRepository;
            _receiveTaxRepository = receiveTaxRepository;
            _inventoryReceiveService = inventoryReceiveService;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _PurchaseDocAcceptanceTaxRepository = PurchaseDocAcceptanceTaxRepository;
        }

        #endregion Constructor

        #region InventoryService

        public void InsertGraph(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            if (Convert.ToBoolean(_inventoryServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.InventoryService WHERE InventoryReceiveId='" + entity.InventoryReceiveId + "' AND ServiceMasterId='" + entity.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                throw new CustomException("This service already taken."); ;

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entity.IsNotNull())
                {
                    entity.ToCurrencyRate = entity.ToCurrencyRate == 0 ? 1 : entity.ToCurrencyRate;
                    var currentId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryService] WHERE InventoryReceiveId='{entity.InventoryReceiveId}'").First();
                    currentId++;
                    if(taxCategoryList.IsNotNull())
					{
                        entity.TotalTaxAmount = taxCategoryList.Sum(r => r.TaxAmount);
                    }
                   
                    var service = new InventoryService
                    {
                        Id = MakePK(entity.InventoryReceiveId + 2, currentId, 2),
                        InventoryReceiveId = entity.InventoryReceiveId,
                        ServiceMasterId = entity.ServiceMasterId,
                        Amount = Math.Round(Convert.ToDecimal(entity.TransactionAmount),2),
                        TotalTaxAmount =  Math.Round(Convert.ToDecimal(entity.TotalTaxAmount),2),
                    };
                    AuditService.AddedLog(service);
                    InsertGraph(service);
                    if (taxCategoryList.IsNotNull())
                    {
                        var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryServiceId='{service.Id}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            crrId++;
                            item.Id = MakePK(service.Id, crrId, 2);
                            item.InventoryReceiveId = entity.InventoryReceiveId;
                            item.InventoryReceiveDetailId = null;
                            item.InventoryServiceId = service.Id;
                            item.TaxAmount = Math.Round(item.TaxAmount, 2);
                            AuditService.AddedLog(item);
                            _receiveTaxRepository.Insert(item);
                        }
                    }
                    var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == service.InventoryReceiveId).Select(t => t.IsNonCreditable).FirstOrDefault();//+ service.TotalTaxAmount
                    var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount) : service.Amount, isNonCreditable);
					var ratioServiceTax = _inventoryReceiveService.GetChargesTaxRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? ( service.TotalTaxAmount) : service.TotalTaxAmount, isNonCreditable);
					if (entity.CurrencyId != entity.BaseCurrencyId)
                        UpdateInventoryDetail(service, ratioServiceTax,ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                    else if (entity.CurrencyId == entity.BaseCurrencyId)
                        UpdateInventoryDetail(service, ratioServiceTax,ratio, 1, entity.IsNonCreditable);
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



        public void InsertGraphUpdate(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            if (Convert.ToBoolean(_inventoryServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.InventoryService WHERE InventoryReceiveId='" + entity.InventoryReceiveId + "' AND ServiceMasterId='" + entity.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                throw new CustomException("This service already taken."); ;

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entity.IsNotNull())
                {
                    entity.ToCurrencyRate = entity.ToCurrencyRate == 0 ? 1 : entity.ToCurrencyRate;
                    var currentId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryService] WHERE InventoryReceiveId='{entity.InventoryReceiveId}'").First();
                    currentId++;
                    var service = new InventoryService
                    {
                        Id = MakePK(entity.InventoryReceiveId + 2, currentId, 2),
                        InventoryReceiveId = entity.InventoryReceiveId,
                        ServiceMasterId = entity.ServiceMasterId,
                        Amount = Convert.ToDecimal(entity.TransactionAmount),
                        TotalTaxAmount = Convert.ToDecimal(entity.TotalTaxAmount),
                    };
                    AuditService.AddedLog(service);
                    InsertGraph(service);
                    if (taxCategoryList.IsNotNull())
                    {
                        var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryServiceId='{service.Id}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            crrId++;
                            item.Id = MakePK(service.Id, crrId, 2);
                            item.InventoryReceiveId = entity.InventoryReceiveId;
                            item.InventoryReceiveDetailId = null;
                            item.InventoryServiceId = service.Id;
                            AuditService.AddedLog(item);
                            _receiveTaxRepository.Insert(item);
                        }
                    }
                    var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == service.InventoryReceiveId).Select(t => t.IsNonCreditable).FirstOrDefault();
                    var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount + service.TotalTaxAmount) : service.Amount, isNonCreditable);
                    var ratioServiceTax = _inventoryReceiveService.GetChargesTaxRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.TotalTaxAmount) : service.TotalTaxAmount, isNonCreditable);
                    if (entity.CurrencyId != entity.BaseCurrencyId)
                        UpdateInventoryDetail(service, ratioServiceTax, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable);
                    else if (entity.CurrencyId == entity.BaseCurrencyId)
                        UpdateInventoryDetail(service, ratioServiceTax, ratio, 1, entity.IsNonCreditable);
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

        public void OtherVendorInsertGraph(InventoryMaterialViewModel entity, IEnumerable<InventoryReceiveTax> taxCategoryList)
        {
            if (Convert.ToBoolean(_inventoryServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.InventoryService WHERE InventoryReceiveId='" + entity.InventoryReceiveId + "' AND ServiceMasterId='" + entity.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
                throw new CustomException("This service already taken.");

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                if (entity.IsNotNull())
                {
                    entity.ToCurrencyRate = entity.ToCurrencyRate == 0 ? 1 : entity.ToCurrencyRate;
                    var currentId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryService] WHERE InventoryReceiveId='{entity.InventoryReceiveId}'").First();
                    currentId++;
                    if (taxCategoryList.IsNotNull())
                    {
                        entity.TotalTaxAmount = taxCategoryList.Sum(r => r.TaxAmount);
                    }

                    var service = new InventoryService
                    {
                        Id = MakePK(entity.InventoryReceiveId + 2, currentId, 2),
                        InventoryReceiveId = entity.InventoryReceiveId,
                        ServiceMasterId = entity.ServiceMasterId,
                        Amount = Math.Round(Convert.ToDecimal(entity.TransactionAmount), 2),
                        TotalTaxAmount = Math.Round(Convert.ToDecimal(entity.TotalTaxAmount), 2),
                        IsOtherVendor = true
                    };
                    AuditService.AddedLog(service);
                    InsertGraph(service);
                    if (taxCategoryList.IsNotNull())
                    {
                        var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryServiceId='{service.Id}'").First();
                        foreach (var item in taxCategoryList)
                        {
                            crrId++;
                            item.Id = MakePK(service.Id, crrId, 2);
                            item.InventoryReceiveId = entity.InventoryReceiveId;
                            item.InventoryReceiveDetailId = null;
                            item.InventoryServiceId = service.Id;
                            item.TaxAmount = Math.Round(item.TaxAmount, 2);
                            AuditService.AddedLog(item);
                            _receiveTaxRepository.Insert(item);
                        }
                    }
                    var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == service.InventoryReceiveId).Select(t => t.IsNonCreditable).FirstOrDefault();//+ service.TotalTaxAmount
                    var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount) : service.Amount, isNonCreditable);
                    var ratioServiceTax = _inventoryReceiveService.GetChargesTaxRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.TotalTaxAmount) : service.TotalTaxAmount, isNonCreditable);
                    var inventoryReceivedata = _inventoryReceiveService.Query(r => r.Id == service.InventoryReceiveId).Select().FirstOrDefault();
                    inventoryReceivedata.OtherPartyId = entity.OtherPartyId;
                    inventoryReceivedata.OtherPartyPlantId = entity.OtherPartyPlantId;
                    inventoryReceivedata.OtherPartyDocRefNo = entity.OtherPartyDocRefNo;
                    inventoryReceivedata.OtherPartyRCMApplicable = entity.OtherPartyRCMApplicable;
                    AuditService.AddedLog(inventoryReceivedata); 
                    _inventoryReceiveService.Update(inventoryReceivedata);
                    
                        UpdateOtherVendorChargesInventoryDetail(service, ratioServiceTax, ratio, Convert.ToDecimal(entity.ToCurrencyRate), entity.IsNonCreditable, entity.CurrencyId, entity.BaseCurrencyId);
                    
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



        public void InsertGraphNew(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string id, string AcceptanceId)
        {
          
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var currentId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryService] WHERE InventoryReceiveId='{id}'").First();
                if (chargesListPO != null)
                {
                    foreach (var itemDetail in chargesListPO)
                    {
                        
                        itemDetail.ToCurrencyRate = itemDetail.ToCurrencyRate == 0 ? 1 : itemDetail.ToCurrencyRate;

                        currentId++;
                        var service = new InventoryService
                        {
                            Id = MakePK(id, currentId, 3),//itemDetail.InventoryReceiveId + 2
                            InventoryReceiveId = id,//itemDetail.InventoryReceiveId,
                            ServiceMasterId = itemDetail.ServiceMasterId,
                            //Amount = Convert.ToDecimal(itemDetail.TransactionAmount),
                            Amount = Convert.ToDecimal(itemDetail.Amount),
                            TotalTaxAmount = Convert.ToDecimal(itemDetail.TotalTaxAmount),
                            POID = itemDetail.InventoryReceiveId,
                            POServiceId = itemDetail.Id
                        };

                        AuditService.AddedLog(service);

                        InsertGraph(service);
                        if (string.IsNullOrWhiteSpace(AcceptanceId))
                        {
                            var poDetail = _POServiceRepository.Query(r => r.Id == itemDetail.Id).Select().FirstOrDefault();
                            if (null == poDetail)
                                throw new CustomException("PO Service not found!");

                            poDetail.GRNServiceAmount += itemDetail.Amount;

                            if (poDetail.Amount < poDetail.GRNServiceAmount)
                                throw new CustomException("Received Amount can not cross balance Balance.");

                            poDetail.AmountStatus = poDetail.Amount == poDetail.GRNServiceAmount;
                            AuditService.UpdatedLog(poDetail);
                            _POServiceRepository.Update(poDetail);
                        }
                        if (string.IsNullOrWhiteSpace(AcceptanceId))
                        {
                            if (POServiceTaxList.IsNotNull())
                            {
                                var crrId = 0;
                                //var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryServiceId='{service.Id}'").First();
                                foreach (var item in POServiceTaxList.Where(r => r.InventoryServiceId == itemDetail.Id))
                                {
                                    crrId++;
                                    var inventoryReceiveTax = new InventoryReceiveTax
                                    {
                                        Id = MakePK(service.Id, crrId, 3),
                                        InventoryReceiveId = id,//itemDetail.InventoryReceiveId;
                                        InventoryReceiveDetailId = null,
                                        InventoryServiceId = service.Id,
                                        TaxCategoryId = item.TaxCategoryId,
                                        Percentage = item.Percentage,
                                        TaxAmount = item.TaxAmount

                                    };
                                    AuditService.AddedLog(inventoryReceiveTax);
                                    //item.ModelState = ModelState.Added;
                                    _receiveTaxRepository.Insert(inventoryReceiveTax);
                                }
                            }
                            var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == id).Select(t => t.IsNonCreditable).FirstOrDefault();//service.InventoryReceiveId
                            var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount + service.TotalTaxAmount) : service.Amount, isNonCreditable);
                            
                            if (itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                                UpdatePOGRNBYChargesDetail(service, ratio, ratio, Convert.ToDecimal(itemDetail.ToCurrencyRate), itemDetail.IsNonCreditable);
                            else if (itemDetail.CurrencyId == itemDetail.BaseCurrencyId)
                                UpdatePOGRNBYChargesDetail(service, ratio, ratio, 1, itemDetail.IsNonCreditable);

                        }
                        else
                        {
                            //if (POServiceTaxList.IsNotNull())
                            //{
                                var crrId = 0;
                            //PurchaseDocAcceptanceTaxRepository
                            //var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryServiceId='{service.Id}'").First();
                            var AcceptanceServiceTaxList = _PurchaseDocAcceptanceTaxRepository.Query(r => r.PurchaseDocAcceptanceId == AcceptanceId && r.PurchaseDocAcceptanceDetailId == null && r.PurchaseDocAcceptanceServiceId== itemDetail.Id).Select().ToList();//($"SELECT * FROM TRN.PurchaseDocAcceptanceTax WHERE PurchaseDocAcceptanceDetailId IS NULL AND PurchaseDocAcceptanceId='{AcceptanceId}'").ToList();

                            foreach (var item in AcceptanceServiceTaxList)
                                {
                                    crrId++;
                                    var inventoryReceiveTax = new InventoryReceiveTax
                                    {
                                        Id = MakePK(service.Id, crrId, 3),
                                        InventoryReceiveId = id,//itemDetail.InventoryReceiveId;
                                        InventoryReceiveDetailId = null,
                                        InventoryServiceId = service.Id,
                                        TaxCategoryId = item.TaxCategoryId,
                                        Percentage = item.Percentage,
                                        TaxAmount = item.TaxAmount

                                    };
                                    AuditService.AddedLog(inventoryReceiveTax);
                                    //item.ModelState = ModelState.Added;
                                    _receiveTaxRepository.Insert(inventoryReceiveTax);
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
        public void InsertGraphNewBOQ(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string id, string AcceptanceId)
        {
          
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                var currentId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryService] WHERE InventoryReceiveId='{id}'").First();
                if (chargesListPO != null)
                {
                    foreach (var itemDetail in chargesListPO)
                    {
                        
                        itemDetail.ToCurrencyRate = itemDetail.ToCurrencyRate == 0 ? 1 : itemDetail.ToCurrencyRate;

                        currentId++;
                        var service = new InventoryService
                        {
                            Id = MakePK(id, currentId, 3),//itemDetail.InventoryReceiveId + 2
                            InventoryReceiveId = id,//itemDetail.InventoryReceiveId,
                            ServiceMasterId = itemDetail.ServiceMasterId,
                            //Amount = Convert.ToDecimal(itemDetail.TransactionAmount),
                            Amount = Convert.ToDecimal(itemDetail.Amount),
                            TotalTaxAmount = Convert.ToDecimal(itemDetail.TotalTaxAmount),
                            POID = itemDetail.InventoryReceiveId,
                            POServiceId = itemDetail.Id
                        };

                        AuditService.AddedLog(service);

                        InsertGraph(service);
                        if (string.IsNullOrWhiteSpace(AcceptanceId))
                        {
                            var poDetail = _POServiceRepository.Query(r => r.Id == itemDetail.Id).Select().FirstOrDefault();
                            if (null == poDetail)
                                throw new CustomException("PO Service not found!");

                            poDetail.GRNServiceAmount += itemDetail.Amount;

                            if (poDetail.Amount < poDetail.GRNServiceAmount)
                                throw new CustomException("Received Amount can not cross balance Balance.");

                            poDetail.AmountStatus = poDetail.Amount == poDetail.GRNServiceAmount;
                            AuditService.UpdatedLog(poDetail);
                            _POServiceRepository.Update(poDetail);
                        }
                        if (string.IsNullOrWhiteSpace(AcceptanceId))
                        {
                            if (POServiceTaxList.IsNotNull())
                            {
                                var crrId = 0;
                                //var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryServiceId='{service.Id}'").First();
                                foreach (var item in POServiceTaxList.Where(r => r.InventoryServiceId == itemDetail.Id))
                                {
                                    crrId++;
                                    var inventoryReceiveTax = new InventoryReceiveTax
                                    {
                                        Id = MakePK(service.Id, crrId, 3),
                                        InventoryReceiveId = id,//itemDetail.InventoryReceiveId;
                                        InventoryReceiveDetailId = null,
                                        InventoryServiceId = service.Id,
                                        TaxCategoryId = item.TaxCategoryId,
                                        Percentage = item.Percentage,
                                        TaxAmount = item.TaxAmount

                                    };
                                    AuditService.AddedLog(inventoryReceiveTax);
                                    //item.ModelState = ModelState.Added;
                                    _receiveTaxRepository.Insert(inventoryReceiveTax);
                                }
                            }
                            var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == id).Select(t => t.IsNonCreditable).FirstOrDefault();//service.InventoryReceiveId
                            var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount + service.TotalTaxAmount) : service.Amount, isNonCreditable);
                            if (itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                            {
                                service.InventoryReceiveId = id;
                                //UpdateInventoryDetail(service, ratio, Convert.ToDecimal(itemDetail.ToCurrencyRate), itemDetail.IsNonCreditable);
                            }
                            else if (itemDetail.CurrencyId == itemDetail.BaseCurrencyId)
                            {
                                service.InventoryReceiveId = id;
                                //UpdateInventoryDetail(service, ratio, 1, itemDetail.IsNonCreditable);
                            }
                        }
                        else
                        {
                            //if (POServiceTaxList.IsNotNull())
                            //{
                                var crrId = 0;
                            //PurchaseDocAcceptanceTaxRepository
                            //var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryServiceId='{service.Id}'").First();
                            var AcceptanceServiceTaxList = _PurchaseDocAcceptanceTaxRepository.Query(r => r.PurchaseDocAcceptanceId == AcceptanceId && r.PurchaseDocAcceptanceDetailId == null && r.PurchaseDocAcceptanceServiceId== itemDetail.Id).Select().ToList();//($"SELECT * FROM TRN.PurchaseDocAcceptanceTax WHERE PurchaseDocAcceptanceDetailId IS NULL AND PurchaseDocAcceptanceId='{AcceptanceId}'").ToList();

                            foreach (var item in AcceptanceServiceTaxList)
                                {
                                    crrId++;
                                    var inventoryReceiveTax = new InventoryReceiveTax
                                    {
                                        Id = MakePK(service.Id, crrId, 3),
                                        InventoryReceiveId = id,//itemDetail.InventoryReceiveId;
                                        InventoryReceiveDetailId = null,
                                        InventoryServiceId = service.Id,
                                        TaxCategoryId = item.TaxCategoryId,
                                        Percentage = item.Percentage,
                                        TaxAmount = item.TaxAmount

                                    };
                                    AuditService.AddedLog(inventoryReceiveTax);
                                    //item.ModelState = ModelState.Added;
                                    _receiveTaxRepository.Insert(inventoryReceiveTax);
                                }
                            //}
                            //var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == id).Select(t => t.IsNonCreditable).FirstOrDefault();//service.InventoryReceiveId
                            //var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount + service.TotalTaxAmount) : service.Amount, isNonCreditable);
                            //if (itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                            //{
                            //    service.InventoryReceiveId = id;
                            //    //UpdateInventoryDetail(service, ratio, Convert.ToDecimal(itemDetail.ToCurrencyRate), itemDetail.IsNonCreditable);
                            //}
                            //else if (itemDetail.CurrencyId == itemDetail.BaseCurrencyId)
                            //{
                            //    service.InventoryReceiveId = id;
                            //    //UpdateInventoryDetail(service, ratio, 1, itemDetail.IsNonCreditable);
                            //}

                        }


                        
                        //}//end

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
        public void InsertGraphNewEdit(IEnumerable<InventoryMaterialViewModel> chargesListPO, IEnumerable<InventoryReceiveTax> POServiceTaxList, string id)
        {
            //if (Convert.ToBoolean(_inventoryServiceRepository.SqlQuery<int>(@"IF EXISTS(SELECT 1 FROM(SELECT * FROM TRN.InventoryService WHERE InventoryReceiveId='" + id + "' AND ServiceMasterId='" + entity.ServiceMasterId + "') AS A) SELECT 1 ELSE SELECT 0 RETURN").First()))
            //    throw new CustomException("This service already taken."); ;

            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                //var currentId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryService] WHERE InventoryReceiveId='{id}'").First();
                if (chargesListPO != null)
                {
                    foreach (var itemDetail in chargesListPO)
                    {
                        //if (itemDetail.IsNotNull())
                        //{
                        itemDetail.ToCurrencyRate = itemDetail.ToCurrencyRate == 0 ? 1 : itemDetail.ToCurrencyRate;

                        //currentId++;
                        var service = new InventoryService
                        {
                            Id = itemDetail.Id,//MakePK(id, currentId, 3),//itemDetail.InventoryReceiveId + 2
                            InventoryReceiveId = id,//itemDetail.InventoryReceiveId,
                            ServiceMasterId = itemDetail.ServiceMasterId,
                            //Amount = Convert.ToDecimal(itemDetail.TransactionAmount),
                            Amount = Convert.ToDecimal(itemDetail.Amount),
                            TotalTaxAmount = Convert.ToDecimal(itemDetail.TotalTaxAmount),
                            POID = itemDetail.POID,
                            POServiceId = itemDetail.POServiceId
                        };

                        AuditService.AddedLog(service);

                        UpdateGraph(service);

                        var poDetail = _POServiceRepository.Query(r => r.Id == itemDetail.POServiceId).Select().FirstOrDefault();
                        if (null == poDetail)
                            throw new CustomException("PO Service not found!");

                        poDetail.GRNServiceAmount += itemDetail.Amount;

                        //if (poDetail.Amount < poDetail.GRNServiceAmount)
                        //    throw new CustomException("Received Amount can not cross balance Balance.");

                        poDetail.AmountStatus = poDetail.Amount == poDetail.GRNServiceAmount;
                        AuditService.UpdatedLog(poDetail);
                        _POServiceRepository.Update(poDetail);


                        if (POServiceTaxList.IsNotNull())
                        {
                            //var crrId = 0;
                            //var crrId = _inventoryServiceRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[InventoryReceiveTax] WHERE InventoryServiceId='{service.Id}'").First();
                            foreach (var item in POServiceTaxList.Where(r => r.InventoryServiceId == itemDetail.Id))
                            {
                                //crrId++;
                                var inventoryReceiveTax = new InventoryReceiveTax
                                {
                                    Id = item.Id,//MakePK(service.Id, crrId, 3),
                                    InventoryReceiveId = id,//itemDetail.InventoryReceiveId;
                                    InventoryReceiveDetailId = null,
                                    InventoryServiceId = service.Id,
                                    TaxCategoryId = item.TaxCategoryId,
                                    Percentage = item.Percentage,
                                    TaxAmount = item.TaxAmount

                                };
                                AuditService.AddedLog(inventoryReceiveTax);
                                //item.ModelState = ModelState.Added;
                                _receiveTaxRepository.Update(inventoryReceiveTax);
                            }
                        }
                        var isNonCreditable = _inventoryReceiveService.Query(t => t.Id == id).Select(t => t.IsNonCreditable).FirstOrDefault();//service.InventoryReceiveId
                        var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, isNonCreditable ? (service.Amount + service.TotalTaxAmount) : service.Amount, isNonCreditable);
                        if (itemDetail.CurrencyId != itemDetail.BaseCurrencyId)
                        {
                            service.InventoryReceiveId = id;
                            //UpdateInventoryDetail(service, ratio, Convert.ToDecimal(itemDetail.ToCurrencyRate), itemDetail.IsNonCreditable);
                        }
                        else if (itemDetail.CurrencyId == itemDetail.BaseCurrencyId)
                        {
                            service.InventoryReceiveId = id;
                            //UpdateInventoryDetail(service, ratio, 1, itemDetail.IsNonCreditable);
                        }
                        //}//end

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
        private void UpdateInventoryDetail(InventoryService service, decimal ratioServiceTax, decimal ratio, decimal currencyRate, bool isNonCreditable)
        {
            try
            {
                var detailList = _invRecDetailRepository.Query(t => t.InventoryReceiveId == service.InventoryReceiveId).Select().ToList();
                if (detailList.IsNotNull())
                {
                    decimal Tax=0;
                    decimal serviceCN=0;
                    decimal Tax1 = 0;
                    decimal serviceCN1 = 0;
                    int i = 0;
                    foreach (var item in detailList)
                    {
                        i++;
                        //var chamnt = item.ChargesTranAmount;

                        //if (i <= detailList.Count-1)
                        //{
                        //    item.ChargesTaxTranAmount += Math.Round(item.MaterialTranAmount * ratioServiceTax, 2);
                        //    item.ChargesTranAmount += Math.Round(item.MaterialTranAmount * ratio, 2);
                        //    //item.ChargesTaxTranAmount = Math.Round(item.MaterialTranAmount * ratio, 2);
                        //    //item.ChargesTranAmount = Math.Round(item.MaterialTranAmount * ratioServiceTax, 2);
                        //    Tax += Math.Round(item.MaterialTranAmount * ratioServiceTax, 2);
                        //    serviceCN += Math.Round(item.MaterialTranAmount * ratio, 2);
                        //    //item.ChargesTaxTranAmount = Math.Round(Tax, 2);
                        //    //item.ChargesTranAmount = Math.Round(serviceCN, 2);
                        //}
                        //else
                        //{
                        //    item.ChargesTaxTranAmount += Math.Round(Convert.ToDecimal(service.TotalTaxAmount)-Tax,2);
                        //    item.ChargesTranAmount += Math.Round(Convert.ToDecimal(service.Amount) - serviceCN,2);
                        //    //item.ChargesTaxTranAmount = Math.Round((Convert.ToDecimal(service.TotalTaxAmount) +item.ChargesTaxTranAmount)- Tax, 2);
                        //    //item.ChargesTranAmount = Math.Round((Convert.ToDecimal(service.Amount) + item.ChargesTranAmount)- serviceCN, 2);
                        //}
                        if (i <= detailList.Count - 1)
                        {
                            item.ChargesTaxTranAmount = Math.Round(item.MaterialTranAmount * ratioServiceTax, 2);
                            item.ChargesTranAmount = Math.Round(item.MaterialTranAmount * ratio, 2);
                            Tax += Convert.ToDecimal(item.ChargesTaxTranAmount);
                            serviceCN += Convert.ToDecimal(item.ChargesTranAmount);
                        }
                        else
                        {
                            var serviceex = _inventoryServiceRepository.Query(r => r.InventoryReceiveId == service.InventoryReceiveId).Select().ToList();
							if (serviceex != null)
							{
                                Tax1 = serviceex.Sum(r => r.TotalTaxAmount);
                                serviceCN1 = serviceex.Sum(r => r.Amount);
                            }
                            
                            item.ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(service.TotalTaxAmount + Tax1) - Tax, 2);
                            item.ChargesTranAmount = Math.Round(Convert.ToDecimal(service.Amount + serviceCN1) - serviceCN, 2);
                        }
                        //item.ChargesTranAmount -= item.ChargesTaxTranAmount;//+ item.ChargesTaxTranAmount + item.ChargesTaxTranAmount
                        item.TotalMaterialTranAmount = isNonCreditable ? Math.Round(Convert.ToDecimal(item.MaterialTranAmount+  item.TotalTaxAmount + item.ChargesTranAmount + item.ChargesTaxTranAmount),2) :
                          Math.Round(Convert.ToDecimal(item.MaterialTranAmount+item.ChargesTranAmount),2);
						//item.TotalMaterialBooksCurrencyAmount = item.MaterialTranAmount * currencyRate; 
											
						item.TotalMaterialBooksCurrencyAmount = isNonCreditable ? Math.Round(Convert.ToDecimal(item.MaterialTranAmount+item.TotalTaxAmount + item.ChargesTranAmount + item.ChargesTaxTranAmount) * Convert.ToDecimal(currencyRate),2) :
                                 Math.Round(Convert.ToDecimal(item.MaterialTranAmount+item.ChargesTranAmount) * Convert.ToDecimal(currencyRate),2);





                        //item.TotalMaterialTranAmount += isNonCreditable ? Math.Round(Convert.ToDecimal(item.TotalTaxAmount + item.ChargesTranAmount + item.ChargesTaxTranAmount), 2) :
                        //                          Math.Round(Convert.ToDecimal(item.ChargesTranAmount), 2);
                        //item.TotalMaterialBooksCurrencyAmount = item.MaterialTranAmount * currencyRate;

                        //item.TotalMaterialBooksCurrencyAmount += isNonCreditable ? Math.Round(Convert.ToDecimal(item.TotalTaxAmount + item.ChargesTranAmount + item.ChargesTaxTranAmount) * Convert.ToDecimal(currencyRate), 2) :
                        //         Math.Round(Convert.ToDecimal(item.ChargesTranAmount) * Convert.ToDecimal(currencyRate), 2);
                        //item.TrnCurrencyBaseRate = Math.Round(item.TotalMaterialTranAmount / item.BaseQty, 4);
                        //item.BooksCurrencyBaseRate = Math.Round(item.TotalMaterialBooksCurrencyAmount / item.BaseQty, 4);
                    


                    item.TrnCurrencyBaseRate = Math.Round(item.TotalMaterialTranAmount / item.BaseQty,4);
						item.BooksCurrencyBaseRate = Math.Round(item.TotalMaterialBooksCurrencyAmount / item.BaseQty,4);
						item.ModelState = ModelState.Modified;
                        AuditService.UpdatedLog(item);
                        _invRecDetailRepository.Update(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void UpdatePOGRNBYChargesDetail(InventoryService service, decimal ratioServiceTax, decimal ratio, decimal currencyRate, bool isNonCreditable)
        {
            try
            {
                var detailList = _invRecDetailRepository.Query(t => t.InventoryReceiveId == service.InventoryReceiveId).Select().ToList();
                if (detailList.IsNotNull())
                {
                    decimal Tax = 0;
                    decimal serviceCN = 0;
                    decimal Tax1 = 0;
                    decimal serviceCN1 = 0;
                    int i = 0;
                    foreach (var item in detailList)
                    {
                        i++;
                       
                        if (i <= detailList.Count - 1)
                        {
                            item.ChargesTaxTranAmount = Math.Round(item.MaterialTranAmount * ratioServiceTax, 2);
                            item.ChargesTranAmount = Math.Round(item.MaterialTranAmount * ratio, 2);
                            Tax += Convert.ToDecimal(item.ChargesTaxTranAmount);
                            serviceCN += Convert.ToDecimal(item.ChargesTranAmount);
                        }
                        else
                        {
                            var serviceex = _inventoryServiceRepository.Query(r => r.InventoryReceiveId == service.InventoryReceiveId).Select().ToList();
                            if (serviceex != null)
                            {
                                Tax1 = serviceex.Sum(r => r.TotalTaxAmount);
                                serviceCN1 = serviceex.Sum(r => r.Amount);
                            }

                            item.ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(service.TotalTaxAmount + Tax1) - Tax, 2);
                            item.ChargesTranAmount = Math.Round(Convert.ToDecimal(service.Amount + serviceCN1) - serviceCN, 2);
                        }
                        //item.ChargesTranAmount -= item.ChargesTaxTranAmount;//+ item.ChargesTaxTranAmount + item.ChargesTaxTranAmount
                        item.TotalMaterialTranAmount = isNonCreditable ? Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.TotalTaxAmount + item.ChargesTranAmount + item.ChargesTaxTranAmount), 2) :
                          Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.ChargesTranAmount), 2);
                        //item.TotalMaterialBooksCurrencyAmount = item.MaterialTranAmount * currencyRate; 

                        item.TotalMaterialBooksCurrencyAmount = isNonCreditable ? Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.TotalTaxAmount + item.ChargesTranAmount + item.ChargesTaxTranAmount) * Convert.ToDecimal(currencyRate), 2) :
                                 Math.Round(Convert.ToDecimal(item.MaterialTranAmount + item.ChargesTranAmount) * Convert.ToDecimal(currencyRate), 2);


                        item.TrnCurrencyBaseRate = Math.Round(item.TotalMaterialTranAmount / item.BaseQty, 4);
                        item.BooksCurrencyBaseRate = Math.Round(item.TotalMaterialBooksCurrencyAmount / item.BaseQty, 4);
                        item.ModelState = ModelState.Modified;
                        AuditService.UpdatedLog(item);
                        _invRecDetailRepository.Update(item);
                    }
                }
                _unitOfWork.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void UpdateOtherVendorChargesInventoryDetail(InventoryService service, decimal ratioServiceTax, decimal ratio, decimal currencyRate, bool isNonCreditable,string trnCurrencyId,string BaseCurrencyId)
        {
            try
            {
                var detailList = _invRecDetailRepository.Query(t => t.InventoryReceiveId == service.InventoryReceiveId).Select().ToList();
                if (detailList.IsNotNull())
                {
                    decimal Tax = 0;
                    decimal serviceCN = 0;
                    decimal Tax1 = 0;
                    decimal serviceCN1 = 0;
                    int i = 0;
                    foreach (var item in detailList)
                    {
                        i++;
                        
                        if (i <= detailList.Count - 1)
                        {
                            item.AdditionalChargesTax = Math.Round(item.MaterialTranAmount * ratioServiceTax, 2);
                            item.AdditionalChargesAmount = Math.Round(item.MaterialTranAmount * ratio, 2);
                            Tax += Convert.ToDecimal(item.ChargesTaxTranAmount);
                            serviceCN += Convert.ToDecimal(item.ChargesTranAmount);
                        }
                        else
                        {
                            var serviceex = _inventoryServiceRepository.Query(r => r.InventoryReceiveId == service.InventoryReceiveId && r.IsOtherVendor==true).Select().ToList();
                            if (serviceex != null)
                            {
                                Tax1 = serviceex.Sum(r => r.TotalTaxAmount);
                                serviceCN1 = serviceex.Sum(r => r.Amount);
                            }

                            item.AdditionalChargesTax = Math.Round(Convert.ToDecimal(service.TotalTaxAmount), 2);
                            item.AdditionalChargesAmount = Math.Round(Convert.ToDecimal(service.Amount), 2);
                        }
                      
                        item.ModelState = ModelState.Modified;
                        AuditService.UpdatedLog(item);
                        _invRecDetailRepository.Update(item);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void UpdateInventoryDetailDelete(InventoryService service, decimal ratioServiceTax, decimal ratio, decimal currencyRate, bool isNonCreditable)
        {
            try
            {
                var detailList = _invRecDetailRepository.Query(t => t.InventoryReceiveId == service.InventoryReceiveId).Select().ToList();
                if (detailList.IsNotNull())
                {
                    decimal Tax = 0;
                    decimal serviceCN = 0;
                    int i = 0;
                    foreach (var item in detailList)
                    {
						i++;

						if (i <= detailList.Count)
						{
							item.ChargesTaxTranAmount = Math.Round(item.MaterialTranAmount * ratioServiceTax, 2);
							item.ChargesTranAmount = Math.Round(item.MaterialTranAmount * ratio, 2);
							Tax += Convert.ToDecimal(item.ChargesTaxTranAmount);
							serviceCN += Convert.ToDecimal(item.ChargesTranAmount);
						}
						else
						{
							item.ChargesTaxTranAmount = Math.Round(Convert.ToDecimal(service.TotalTaxAmount) - Tax,2);
							item.ChargesTranAmount = Math.Round(Convert.ToDecimal(service.Amount) - serviceCN,2);

						}

						//item.ChargesTranAmount -= item.ChargesTaxTranAmount;//+ item.ChargesTaxTranAmount + item.ChargesTaxTranAmount
						//item.TotalMaterialTranAmount += isNonCreditable ? Convert.ToDecimal(item.TotalTaxAmount + item.ChargesTranAmount) :
						//  Convert.ToDecimal(item.ChargesTranAmount);
						//item.TotalMaterialBooksCurrencyAmount = item.MaterialTranAmount * currencyRate;

						//item.TotalMaterialBooksCurrencyAmount += isNonCreditable ? Convert.ToDecimal(item.TotalTaxAmount + item.ChargesTranAmount) * Convert.ToDecimal(currencyRate) :
						//         Convert.ToDecimal(item.ChargesTranAmount) * Convert.ToDecimal(currencyRate);




						item.TotalMaterialTranAmount = isNonCreditable ? (Convert.ToDecimal(item.MaterialTranAmount + item.TotalTaxAmount + item.ChargesTranAmount) + item.ChargesTaxTranAmount) :
						  Convert.ToDecimal(item.MaterialTranAmount + item.ChargesTranAmount);
						//item.TotalMaterialBooksCurrencyAmount = item.MaterialTranAmount * currencyRate;

						item.TotalMaterialBooksCurrencyAmount = isNonCreditable ? (Convert.ToDecimal(item.MaterialTranAmount + item.TotalTaxAmount + item.ChargesTranAmount) + item.ChargesTaxTranAmount) :
                                 Convert.ToDecimal(item.TotalMaterialTranAmount - item.ChargesTranAmount);
                        item.TrnCurrencyBaseRate = item.TotalMaterialTranAmount / item.BaseQty;
                        item.BooksCurrencyBaseRate = item.TotalMaterialBooksCurrencyAmount / item.BaseQty;
                        item.ModelState = ModelState.Modified;
                        AuditService.UpdatedLog(item);
                        _invRecDetailRepository.Update(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Delete(string serviceId)
        {
            var flag = false;
            try
            {
                var isNonCreditable = _inventoryServiceRepository.SqlQuery<bool>(@"SELECT A.IsNonCreditable FROM [TRN].[InventoryReceive] AS A JOIN [TRN].[InventoryService] AS B ON B.InventoryReceiveId=A.Id WHERE B.Id='" + serviceId + "'").First();
                var service = Find(serviceId);
                if (!service.IsNotNull()) throw new CustomException("Data not found");
                _unitOfWork.BeginTransaction();
                flag = true;

                var taxCategoryList = _receiveTaxRepository.Query(t => t.InventoryServiceId == serviceId).Select().ToList();
                if (taxCategoryList.IsNotNull())
                {
                    foreach (var item in taxCategoryList)
                    {
                        item.ModelState = ModelState.Deleted;
                        _receiveTaxRepository.Delete(item);
                    }
                }
                var ratio = _inventoryReceiveService.GetChargesRatio(service.InventoryReceiveId, null, 0, service.Id, 0, isNonCreditable);
				var ratioServiceTax = _inventoryReceiveService.GetChargesTaxRatio(service.InventoryReceiveId, null, 0, service.Id, 0, isNonCreditable);
                //UpdateInventoryDetail(service, ratioServiceTax, ratio, 1, isNonCreditable);
                UpdateInventoryDetailDelete(service, ratioServiceTax, ratio, 1, isNonCreditable);
                //Service Update
                var POServiceData = _POServiceRepository.Find(service.POServiceId);
                if(POServiceData.IsNotNull())
				{
                    POServiceData.GRNServiceAmount = Convert.ToDecimal(((POServiceData.GRNServiceAmount - service.Amount)));
                    POServiceData.AmountStatus = POServiceData.Amount == POServiceData.GRNServiceAmount;
                    _POServiceRepository.Update(POServiceData);
                }
                

                base.DeleteGraph(service);
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

        #endregion InventoryService

        public IEnumerable<object> Query(string receiveId)
        {
            try
            {
                //var sql = @"SELECT A.Id, A.InventoryReceiveId, A.ServiceMasterId, B.UserName AS ServiceMasterName, A.Amount, A.TotalTaxAmount
                //            FROM [TRN].[InventoryService] AS A JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id WHERE A.InventoryReceiveId='" + receiveId + "'";
                var sql = @"SELECT A.Id
                        , A.InventoryReceiveId
                        , A.ServiceMasterId
                        , B.UserName AS ServiceMasterName
                         ,A.Amount Amount,A.Amount GRNServiceAmount
                        , POT.Amount-A.Amount AS  Bal
                        , POT.Amount As POAmount
                        --, A.TotalTaxAmount
                        ,A.POID
						,A.POServiceId,IRT.TaxAmount TotalTaxAmount
                        FROM [TRN].[InventoryService] AS A 
                        JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                        left JOIN (select Id, Amount from TRN.POService) AS POT on A.POServiceId=POT.Id
                        left join ( Select InventoryServiceId, sum(TaxAmount) TaxAmount from  trn.InventoryReceiveTax group by InventoryServiceId) IRT On IRT.InventoryServiceId=A.Id
                        
                        WHERE A.InventoryReceiveId='" + receiveId + "' And A.IsOtherVendor=0";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> OtherVendorChargesQuery(string receiveId)
        {
            try
            {
                      var sql = @"SELECT A.Id
                        , A.InventoryReceiveId
                        , A.ServiceMasterId
                        , B.UserName AS ServiceMasterName
                         ,A.Amount Amount,A.Amount GRNServiceAmount
                        , POT.Amount-A.Amount AS  Bal
                        , POT.Amount As POAmount
                        --, A.TotalTaxAmount
                        ,A.POID
						,A.POServiceId,IRT.TaxAmount TotalTaxAmount
                        FROM [TRN].[InventoryService] AS A 
                        JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                        left JOIN (select Id, Amount from TRN.POService) AS POT on A.POServiceId=POT.Id
                        left join ( Select InventoryServiceId, sum(TaxAmount) TaxAmount from  trn.InventoryReceiveTax group by InventoryServiceId) IRT On IRT.InventoryServiceId=A.Id
                        
                        WHERE A.InventoryReceiveId='" + receiveId + "' And A.IsOtherVendor=1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> QueryBOQ(string receiveId)
        {
            try
            {
                //var sql = @"SELECT A.Id, A.InventoryReceiveId, A.ServiceMasterId, B.UserName AS ServiceMasterName, A.Amount, A.TotalTaxAmount
                //            FROM [TRN].[InventoryService] AS A JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id WHERE A.InventoryReceiveId='" + receiveId + "'";
                var sql = @"SELECT A.Id
                        , A.InventoryReceiveId
                        , A.ServiceMasterId
                        , B.UserName AS ServiceMasterName
                         ,A.Amount Amount,A.Amount GRNServiceAmount
                        , POT.Amount-A.Amount AS  Bal
                        , POT.Amount As POAmount
                        --, A.TotalTaxAmount
                        ,A.POID
						,A.POServiceId,IRT.TaxAmount TotalTaxAmount
                        FROM [TRN].[InventoryService] AS A 
                        JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                        left JOIN (select Id, Amount from TRN.POService) AS POT on A.POServiceId=POT.Id
                        left join ( Select InventoryServiceId, sum(TaxAmount) TaxAmount from  trn.InventoryReceiveTax group by InventoryServiceId) IRT On IRT.InventoryServiceId=A.Id
                        
                        WHERE A.InventoryReceiveId='" + receiveId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> QueryPurchaseReturnCharges(string receiveId) 
        {
            try
            {
                //var sql = @"SELECT A.Id, A.InventoryReceiveId, A.ServiceMasterId, B.UserName AS ServiceMasterName, A.Amount, A.TotalTaxAmount
                //            FROM [TRN].[InventoryService] AS A JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id WHERE A.InventoryReceiveId='" + receiveId + "'";
                var sql = @"SELECT A.Id
                        , A.PurchaseReturnId
                        , A.ServiceMasterId
                        , B.UserName AS ServiceMasterName
                         ,A.Amount Amount,A.Amount GRNServiceAmount
                        --, POT.Amount-A.Amount AS  Bal
                        --, POT.Amount As POAmount
                        --, A.TotalTaxAmount
                        ,A.POID
						,A.POServiceId,IRT.TaxAmount TotalTaxAmount
                        FROM [TRN].[PurchaseReturnService] AS A 
                        JOIN [HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id 
                       -- left JOIN (select Id, Amount from TRN.POService) AS POT on A.POServiceId=POT.Id
                        left join ( Select InventoryServiceId, sum(TaxAmount) TaxAmount from  trn.PurchaseReturnTax group by InventoryServiceId) IRT On IRT.InventoryServiceId=A.Id
                        WHERE A.PurchaseReturnId='" + receiveId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> Query1(string receiveId,string AcceptanceId)
        {
            try
            {
                var sql = "";
                if (receiveId != "null")
                {
                    string paramter = "";
                    if (receiveId != "")
                    {
                        if (paramter == "")
                            paramter += "A.InventoryReceiveId in(" + receiveId + ")";
                        else
                            paramter += " AND A.InventoryReceiveId in(" + receiveId + ")";
                    }

                     sql = @"SELECT A.Id, A.InventoryReceiveId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.Amount As POAmount
                            --,A.Amount
                            --, A.TotalTaxAmount
                             ,POT.TaxAmount As POTaxAmount
                             ,0 TotalTaxAmount
                            --,TaxAmount
                            ,null ChargeTaxList
                            ,'True' enableid1
                            ,GRNServiceAmount
                            ,0 AS Amount
                            ,AmountStatus
                            FROM
                            [TRN].[POService]
                            AS A
                           INner JOIN[HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id
                           left JOIN (select InventoryServiceId, Sum(TaxAmount) as TaxAmount from TRN.PurchaseOrderTax group by InventoryServiceId) AS POT on A.id=POT.InventoryServiceId
                           WHERE A.AmountStatus=0 AND  " + paramter + "";
                }
                else 
                {
                     sql = @"SELECT A.Id, A.PurchaseDocAcceptanceId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.Amount As POAmount                           
                            ,POT.TaxAmount As TotalTaxAmount
                            ,null ChargeTaxList
                            ,'True' enableid1
                            ,0 GRNServiceAmount
                            ,(A.Amount-0) AS Amount
                            ,0 AmountStatus
                            FROM TRN.PurchaseDocAcceptanceService AS A
                           INner JOIN[HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id
                           left JOIN (select PurchaseDocAcceptanceId, Sum(TaxAmount) as TaxAmount from TRN.PurchaseDocAcceptanceTax group by PurchaseDocAcceptanceId) AS POT on A.PurchaseDocAcceptanceId=POT.PurchaseDocAcceptanceId
                           WHERE A.PurchaseDocAcceptanceId='" + AcceptanceId + "'";
                }
                
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
        public IEnumerable<object> Query1BOQ(string receiveId,string AcceptanceId)
        {
            try
            {
                var sql = "";
                if (receiveId != "null")
                {
                    string paramter = "";
                    if (receiveId != "")
                    {
                        if (paramter == "")
                            paramter += "A.InventoryReceiveId in(" + receiveId + ")";
                        else
                            paramter += " AND A.InventoryReceiveId in(" + receiveId + ")";
                    }

                     sql = @"SELECT A.Id, A.InventoryReceiveId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.Amount As POAmount
                            --,A.Amount
                            --, A.TotalTaxAmount
                             ,POT.TaxAmount As POTaxAmount
                             ,0 TotalTaxAmount
                            --,TaxAmount
                            ,null ChargeTaxList
                            ,'True' enableid1
                            ,GRNServiceAmount
                            ,0 AS Amount
                            ,AmountStatus
                            FROM
                            [TRN].[POService]
                            AS A
                           INner JOIN[HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id
                           left JOIN (select InventoryServiceId, Sum(TaxAmount) as TaxAmount from TRN.PurchaseOrderTax group by InventoryServiceId) AS POT on A.id=POT.InventoryServiceId
                           WHERE A.AmountStatus=0 AND  " + paramter + "";
                }
                else 
                {
                     sql = @"SELECT A.Id, A.PurchaseDocAcceptanceId
                            , A.ServiceMasterId
                            , B.UserName AS ServiceMasterName
                            , A.Amount As POAmount                           
                            ,POT.TaxAmount As TotalTaxAmount
                            ,null ChargeTaxList
                            ,'True' enableid1
                            ,0 GRNServiceAmount
                            ,(A.Amount-0) AS Amount
                            ,0 AmountStatus
                            FROM TRN.PurchaseDocAcceptanceService AS A
                           INner JOIN[HKP].[ServiceMaster] AS B ON A.ServiceMasterId=B.Id
                           left JOIN (select PurchaseDocAcceptanceId, Sum(TaxAmount) as TaxAmount from TRN.PurchaseDocAcceptanceTax group by PurchaseDocAcceptanceId) AS POT on A.PurchaseDocAcceptanceId=POT.PurchaseDocAcceptanceId
                           WHERE A.PurchaseDocAcceptanceId='" + AcceptanceId + "'";
                }
                
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> getTCSData(string receiveId) 
        {
            try
            {
                var sql = "";
			   sql = @"select PRTCS.Id,PRTCS.TaxCategoryId,Tc.UserName TaxCategoryName ,PRTCS.Percentage,PRTCS.TaxAmount,PRTCS.TaxCodeId,PRTCS.InventoryReceiveId
                        FROM trn.InventoryReceive IR 
                        left join  [TRN].[InventoryReceiveAdditionalTax] PRTCS on PRTCS.InventoryReceiveId=IR.id
                        left JOIN mst.TaxCategory TC ON Tc.Id=PRTCS.TaxCategoryId
                        where IR.Id ='" + receiveId + "'";
				
				return _sqlRepository.GetDataCollection(sql);

			}
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }
    }
}