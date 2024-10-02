using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Invoices;
using Library.Model.Materials;
using Library.Model.Parties;
using Library.Model.SalesManagements;
using Library.Model.Taxations;
using Library.Model.Vouchers;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Invoices;
using Library.Service.Logs;
using Library.Service.Systems;
using Library.Service.Vouchers;
using Library.ViewModel.SalesManagements;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Library.Model.OrderManagements;
using Library.Service.Extension.Accounts;
using Library.Service.Extension;
using Library.Model.Productions;
using Library.Crosscutting.Security;
using System.Threading;

namespace Library.Service.SalesManagements
{
    public class SalesService : ISalesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvoiceService _invoiceService;
        private readonly IVoucherService _voucherService;
        private readonly IRepositoryAsync<InvoiceDetail> _invoiceDetailRepository;
        private readonly IRepositoryAsync<InvoiceTax> _invoiceTaxRepository;
        private readonly IRepositoryAsync<InvoiceTaxDetail> _invoiceTaxDetailRepository;
        private readonly IRepositoryAsync<TaxCodeGL> _taxCodeGLRepository;
        private readonly IRepositoryAsync<TaxCategoryGL> _taxCategoryGLRepository;
        private readonly IRepositoryAsync<Sales> _salesRepository;
        private readonly IRepositoryAsync<SalesMaterial> _salesMaterialRepository;
        private readonly IRepositoryAsync<SalesOrderItem> _salesMaterialSORepository;
        private readonly IRepositoryAsync<SalesPacking> _salesPackingRepository;
        private readonly IRepositoryAsync<Model.SalesManagements.SalesService> _salesServiceRepository;
        private readonly IRepositoryAsync<SalesTax> _salesTaxRepository;
        private readonly IRepositoryAsync<MaterialMaster> _materialMasterRepository;
        private readonly IRepositoryAsync<ServiceMaster> _serviceMasterRepository;
        private readonly IRepositoryAsync<MaterialGroupGL> _materialGroupGLRepository;
        private readonly IRepositoryAsync<ServiceGroupGL> _serviceGroupGLRepository;
        private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
        private readonly IRepositoryAsync<CompanyPartyGL> _companyPartyGLRepository;
        private readonly IRepositoryAsync<VoucherTypeMatrix> _voucherTypeMatrixRepository;
        private readonly IRepositoryAsync<MaterialGroupPartyAccountGroupGL> _materialGroupPartyAccountGroupGLRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<FirstCharacteristics> _firstCharacteristicsRepository;
        private readonly IRepositoryAsync<SecondCharacteristics> _secondCharacteristicsRepository;
        private readonly IRepositoryAsync<ThirdCharacteristics> _thirdCharacteristicsRepository;
        private readonly IInvoiceTaxService _invoiceTaxService;
        private readonly IRepositoryAsync<Library.Model.Inventory.InventorySales> _SalesRepository;
        private readonly IRepositoryAsync<Library.Model.Inventory.InventorySalesDetail> _SalesDetailService;
        private readonly IRepositoryAsync<Library.Model.Inventory.InventorySalesHistory> _SalesHistoryService;
        private readonly IRepositoryAsync<ItemScanChild> _ItemScanChildDataService;
        public SalesService(
             IInvoiceService invoiceService
            , IVoucherService voucherService
            , IRepositoryAsync<InvoiceDetail> invoiceDetailRepository
            , IRepositoryAsync<InvoiceTax> invoiceTaxRepository
            , IRepositoryAsync<InvoiceTaxDetail> invoiceTaxDetailRepository
            , IRepositoryAsync<TaxCodeGL> taxCodeGLRepository
            , IRepositoryAsync<TaxCategoryGL> taxCategoryGLRepository
            , IUnitOfWork unitOfWork
            , IRepositoryAsync<Sales> salesRepository
            , IRepositoryAsync<SalesMaterial> salesMaterialRepository
            , IRepositoryAsync<SalesOrderItem> salesMaterialSORepository
            , IRepositoryAsync<SalesPacking> salesPackingRepository
            , IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            , IRepositoryAsync<SalesTax> salesTaxRepository
            , IRepositoryAsync<MaterialMaster> materialMasterRepository
            , IRepositoryAsync<MaterialGroupGL> materialGroupGLRepository
            , IRepositoryAsync<ServiceMaster> serviceMasterRepository
            , IRepositoryAsync<ServiceGroupGL> serviceGroupGLRepository
            , IRepositoryAsync<CompanyParty> companyPartyRepository
            , IRepositoryAsync<CompanyPartyGL> companyPartyGLRepository
            , IRepositoryAsync<VoucherTypeMatrix> voucherTypeMatrixRepository
            , IRepositoryAsync<Model.SalesManagements.SalesService> salesServiceRepository
            , IRepositoryAsync<MaterialGroupPartyAccountGroupGL> materialGroupPartyAccountGroupGLRepository
            , IRepositoryAsync<FirstCharacteristics> firstCharacteristicsRepository
            , IRepositoryAsync<SecondCharacteristics> secondCharacteristicsRepository
            , IRepositoryAsync<ThirdCharacteristics> thirdCharacteristicsRepository
            , IInvoiceTaxService invoiceTaxService
            , IRepositoryAsync<Library.Model.Inventory.InventorySales> SalesRepository
            , IRepositoryAsync<Library.Model.Inventory.InventorySalesDetail> SalesDetailService
            , IRepositoryAsync<Library.Model.Inventory.InventorySalesHistory> SalesHistoryService
            , IRepositoryAsync<ItemScanChild> ItemScanChildDataService
            )
        {
            _unitOfWork = unitOfWork;
            _invoiceService = invoiceService;
            _voucherService = voucherService;
            _invoiceDetailRepository = invoiceDetailRepository;
            _invoiceTaxRepository = invoiceTaxRepository;
            _invoiceTaxDetailRepository = invoiceTaxDetailRepository;
            _taxCodeGLRepository = taxCodeGLRepository;
            _taxCategoryGLRepository = taxCategoryGLRepository;
            _salesRepository = salesRepository;
            _salesMaterialRepository = salesMaterialRepository;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
            _salesTaxRepository = salesTaxRepository;
            _materialMasterRepository = materialMasterRepository;
            _serviceMasterRepository = serviceMasterRepository;
            _materialGroupGLRepository = materialGroupGLRepository;
            _serviceGroupGLRepository = serviceGroupGLRepository;
            _companyPartyRepository = companyPartyRepository;
            _companyPartyGLRepository = companyPartyGLRepository;
            _voucherTypeMatrixRepository = voucherTypeMatrixRepository;
            _salesServiceRepository = salesServiceRepository;
            _materialGroupPartyAccountGroupGLRepository = materialGroupPartyAccountGroupGLRepository;
            _salesMaterialSORepository = salesMaterialSORepository;
            _salesPackingRepository = salesPackingRepository;

            _firstCharacteristicsRepository = firstCharacteristicsRepository;
            _secondCharacteristicsRepository = secondCharacteristicsRepository;
            _thirdCharacteristicsRepository = thirdCharacteristicsRepository;
            _invoiceTaxService = invoiceTaxService;
            _SalesRepository = SalesRepository;
            _SalesDetailService = SalesDetailService;
            _SalesHistoryService = SalesHistoryService;
            _ItemScanChildDataService = ItemScanChildDataService;

        }

        #region Sales
        public void Insert(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
            var flag = false;
            try
            {

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                var sales = new Sales
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    //DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    CurrencyId = voucherVM.CurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    BaseNoOfDays = voucherVM.BaseNoOfDays,
                    BaseOnDueDate = voucherVM.BaseOnDueDate,
                    DeliveryPartyPlantId = voucherVM.DeliveryPartyPlantId,
                    EntryDate = voucherVM.VoucherDate,
                    InvoiceDate = voucherVM.InvoiceDate,
                    InvoicingPartyPlantId = voucherVM.InvoicingPartyPlantId,
                    MatureDate = voucherVM.MatureDate,
                    PartyId = voucherVM.PartyId,
                    PartyType = voucherVM.PartyType,
                    Narration = voucherVM.Narration,
                    ItemDescription = voucherVM.ItemDescription,
                    PaymentTermId = voucherVM.PaymentTermId,
                    RowState = RowState.Parked.ToString(),
                    DeliveryByAddress = voucherVM.DeliveryByAddress,
                    InvoicingByAddress = voucherVM.InvoicingByAddress,
                    IsAdditionalInfoApplicable = voucherVM.IsAdditionalInfoApplicable,
                    InvoiceStatus = voucherVM.InvoiceStatus,
                    PaymentToReceiveBankId = voucherVM.PaymentToReceiveBankId,
                    TrancastionTypeId = voucherVM.TrancastionTypeId,
                    SourceType = "Sales",
                    ModelState = ModelState.Added,
                    Id = "S" + _pkGeneratorService.GetAutoNumber(nameof(Sales), PKGeneratorEnum.Yearly, null, DateTime.Now)
                };
                sales.InvoiceNo = sales.Id;
                voucherVM.Id = sales.Id;
                AuditService.AddedLog(sales);
                _salesRepository.Insert(sales);
                voucherVM.AddedDate = sales.AddedDate;

                var currentSalesMaterialId = 0;
                var currentSalesServiceId = 0;
                var currentSalesTaxId = 0;
                if (salesMaterialVMList != null)
                {
                    foreach (var salesMaterialVM in salesMaterialVMList)
                    {
                        currentSalesMaterialId++;
                        var salesMaterial = new SalesMaterial
                        {
                            Id = _pkGeneratorService.MakePK(sales.Id, currentSalesMaterialId, 3),
                            SalesId = sales.Id,
                            MaterialMasterId = salesMaterialVM.MaterialMasterId,
                            ArticleId = salesMaterialVM.ArticleId,
                            FirstCharacteristicsId = salesMaterialVM.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = salesMaterialVM.FirstCharacteristicsValueId,
                            SecondCharacteristicsId = salesMaterialVM.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = salesMaterialVM.SecondCharacteristicsValueId,
                            ThirdCharacteristicsId = salesMaterialVM.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = salesMaterialVM.ThirdCharacteristicsValueId,
                            //BaseUOMId = salesMaterialVM.BaseUOMId,
                            BaseUOMId = salesMaterialVM.TransactionUoMId,
                            //BaseRate = salesMaterialVM.BaseRate,
                            BaseRate = salesMaterialVM.TransactionRate,
                            //BaseQty = salesMaterialVM.BaseQty,
                            BaseQty = salesMaterialVM.TransactionQty,
                            //BaseAmount = salesMaterialVM.BaseAmount,
                            BaseAmount = salesMaterialVM.TransactionAmount,
                            BaseUoMFactor = salesMaterialVM.BaseUoMFactor,
                            TransactionUoMId = salesMaterialVM.TransactionUoMId,
                            TransactionRate = salesMaterialVM.TransactionRate,
                            TransactionQty = salesMaterialVM.TransactionQty,
                            TransactionAmount = salesMaterialVM.TransactionAmount,
                            //BooksCurrencyTransactionAmount = Math.Round(salesMaterialVM.TransactionAmount * voucherVM.CompanyCurrencyRate),
                            BooksCurrencyTransactionAmount = Math.Round(salesMaterialVM.TransactionAmount * sales.ToCurrencyRate, 2),
                            TaxAmount = salesMaterialVM.TaxAmount,
                            //BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                            BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * sales.ToCurrencyRate, 2),
                            //BooksCurrencyBaseRate = Math.Round(voucherVM.CompanyCurrencyRate * salesMaterialVM.TransactionRate, 4),
                            BooksCurrencyBaseRate = Math.Round(voucherVM.CompanyCurrencyRate * sales.ToCurrencyRate, 4),
                            NetAmount = salesMaterialVM.TaxAmount + salesMaterialVM.TransactionAmount,
                            ModelState = ModelState.Added,
                            AddedBy = sales.AddedBy,
                            AddedDate = sales.AddedDate,
                            AddedFromIP = sales.AddedFromIP,
                            UpdatedBy = null,
                            UpdatedDate = null,
                            CanceledBy = null,
                            IsCanceled = false,
                            Remark = null
                        };
                        if (voucherVM.CurrencyId != companyCurrencyId)
                        {
                            //salesMaterial.BooksCurrencyTransactionAmount = Math.Round(salesMaterialVM.TransactionAmount * voucherVM.CompanyCurrencyRate, 2);
                            salesMaterial.BooksCurrencyTransactionAmount = Math.Round(salesMaterialVM.TransactionAmount * sales.ToCurrencyRate, 2);
                        }
                        _salesMaterialRepository.Insert(salesMaterial);

                        if (salesMaterialVM.TaxList != null && salesMaterialVM.TaxList.Count > 0)
                        {
                            foreach (var taxVM in salesMaterialVM.TaxList)
                            {
                                if (taxVM.TaxCategoryId == null)
                                    throw new CustomException("Please Select Tax Category !");

                                currentSalesTaxId++;
                                var salesTax = new SalesTax
                                {
                                    Id = _pkGeneratorService.MakePK(salesMaterial.Id, currentSalesTaxId, 2),
                                    AddedBy = salesMaterial.AddedBy,
                                    AddedDate = salesMaterial.AddedDate,
                                    AddedFromIP = salesMaterial.AddedFromIP,
                                    Amount = taxVM.TotalAmount,
                                    //BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                    BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * sales.ToCurrencyRate, 2),
                                    HSNCodeId = taxVM.HSNCodeId,
                                    Percentage = taxVM.Percentage,
                                    SalesId = sales.Id,
                                    SalesMaterialId = salesMaterial.Id,
                                    TaxCategoryId = taxVM.TaxCategoryId,
                                    SalesServiceId = null,
                                    ModelState = ModelState.Added,
                                    UpdatedBy = null,
                                    UpdatedDate = null,
                                    UpdatedFromIP = null
                                };
                                _salesTaxRepository.Insert(salesTax);
                            }
                        }
                    }
                }
                if (salesServiceVMList != null)
                {
                    foreach (var salesServiceVM in salesServiceVMList)
                    {

                        currentSalesServiceId++;
                        var salesService = new Model.SalesManagements.SalesService
                        {
                            AddedBy = sales.AddedBy,
                            AddedDate = sales.AddedDate,
                            AddedFromIP = sales.AddedFromIP,
                            Amount = salesServiceVM.Amount,
                            BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * sales.ToCurrencyRate, 2),
                            //BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                            Id = _pkGeneratorService.MakePK(sales.Id, currentSalesServiceId, 2),
                            ModelState = ModelState.Added,
                            NetAmount = salesServiceVM.NetAmount,
                            SalesId = sales.Id,
                            ServiceMasterId = salesServiceVM.ServiceMasterId,
                            TaxAmount = salesServiceVM.TaxAmount,
                            //BooksCurrencyTaxAmount = Math.Round(salesServiceVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                            BooksCurrencyTaxAmount = Math.Round(salesServiceVM.TaxAmount * sales.ToCurrencyRate, 2),
                            UpdatedBy = null,
                            UpdatedDate = null,
                            UpdatedFromIP = null
                        };
                        _salesServiceRepository.Insert(salesService);

                        if (salesServiceVM.ServiceTaxList != null && salesServiceVM.ServiceTaxList.Count > 0)
                        {
                            foreach (var taxVM in salesServiceVM.ServiceTaxList)
                            {
                                if (taxVM.TaxCategoryId == null)
                                    throw new CustomException("Please Select Tax Category !");

                                currentSalesTaxId++;
                                var salesTax = new SalesTax
                                {
                                    Id = _pkGeneratorService.MakePK(salesService.Id, currentSalesTaxId, 2),
                                    AddedBy = salesService.AddedBy,
                                    AddedDate = salesService.AddedDate,
                                    AddedFromIP = salesService.AddedFromIP,
                                    Amount = taxVM.TotalAmount,
                                    //BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                    BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * sales.ToCurrencyRate, 2),
                                    HSNCodeId = taxVM.HSNCodeId,
                                    Percentage = taxVM.Percentage,
                                    SalesId = sales.Id,
                                    SalesMaterialId = null,
                                    SalesServiceId = salesService.Id,
                                    TaxCategoryId = taxVM.TaxCategoryId,
                                    ModelState = ModelState.Added,
                                    UpdatedBy = null,
                                    UpdatedDate = null,
                                    UpdatedFromIP = null
                                };
                                _salesTaxRepository.Insert(salesTax);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void Update(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);


                _unitOfWork.BeginTransaction();
                flag = true;


                var sales = _salesRepository.Find(voucherVM.Id);

                sales.UpdatedBy = voucherVM.UpdatedBy;
                sales.UpdatedDate = voucherVM.UpdatedDate;
                sales.UpdatedFromIP = voucherVM.UpdatedFromIP;
                sales.SourceType = "Sales";
                sales.InvoiceStatus = voucherVM.InvoiceStatus;
                sales.InvoiceNo = sales.Id;
                sales.EntityId = voucherVM.EntityId;
                sales.InvoiceDate = voucherVM.InvoiceDate;
                sales.CurrencyId = voucherVM.CurrencyId;
                sales.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                sales.DocRefNo = voucherVM.DocRefNo;
                sales.ItemDescription = voucherVM.ItemDescription;
                sales.PaymentToReceiveBankId = voucherVM.PaymentToReceiveBankId;
                sales.PaymentTermId = voucherVM.PaymentTermId;
                sales.BaseOnDueDate = voucherVM.BaseOnDueDate;
                sales.BaseNoOfDays = voucherVM.BaseNoOfDays;
                sales.MatureDate = voucherVM.MatureDate;
                sales.Narration = voucherVM.Narration;
                sales.TrancastionTypeId = voucherVM.TrancastionTypeId;
                sales.ModelState = ModelState.Modified;
                AuditService.UpdatedLog(sales);
                _salesRepository.Update(sales);

                var currentSalesMaterialId = _salesMaterialRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.SalesMaterial WHERE SalesId='{sales.Id}'").First();
                var currentSalesTaxId = _salesMaterialRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 4) AS INT)), 0) Id FROM TRN.SalesTax WHERE SalesId='{sales.Id}' AND SalesServiceId IS NULL").First();
                var currentServiceSalesTaxId = _salesMaterialRepository.SqlQuery<int>($"SELECT Count(Id) Id FROM TRN.SalesTax WHERE SalesId='{sales.Id}' AND SalesmaterialId IS NULL").First();

                var currentSalesServiceId = 0;
                if (salesMaterialVMList != null)
                {
                    foreach (var salesMaterialVM in salesMaterialVMList)
                    {
                        if (string.IsNullOrEmpty(salesMaterialVM.Id))
                        {

                            currentSalesMaterialId++;
                            var salesMaterial = new SalesMaterial
                            {
                                Id = _pkGeneratorService.MakePK(sales.Id, currentSalesMaterialId, 3),
                                SalesId = sales.Id,
                                MaterialMasterId = salesMaterialVM.MaterialMasterId,
                                ArticleId = salesMaterialVM.ArticleId,
                                FirstCharacteristicsId = salesMaterialVM.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = salesMaterialVM.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = salesMaterialVM.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = salesMaterialVM.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = salesMaterialVM.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = salesMaterialVM.ThirdCharacteristicsValueId,
                                //BaseUOMId = salesMaterialVM.BaseUOMId,
                                BaseUOMId = salesMaterialVM.TransactionUoMId,
                                //BaseRate = salesMaterialVM.BaseRate,
                                BaseRate = salesMaterialVM.TransactionRate,
                                //BaseQty = salesMaterialVM.BaseQty,
                                BaseQty = salesMaterialVM.TransactionQty,
                                //BaseAmount = salesMaterialVM.BaseAmount,
                                BaseAmount = salesMaterialVM.TransactionAmount,
                                BaseUoMFactor = salesMaterialVM.BaseUoMFactor,
                                TransactionUoMId = salesMaterialVM.TransactionUoMId,
                                TransactionRate = salesMaterialVM.TransactionRate,
                                TransactionQty = salesMaterialVM.TransactionQty,
                                TransactionAmount = salesMaterialVM.TransactionAmount,
                                TaxAmount = salesMaterialVM.TaxAmount,
                                NetAmount = salesMaterialVM.TaxAmount + salesMaterialVM.TransactionAmount,
                                //BooksCurrencyTransactionAmount = Math.Round(salesMaterialVM.TransactionAmount * voucherVM.CompanyCurrencyRate),
                                BooksCurrencyTransactionAmount = Math.Round(salesMaterialVM.TransactionAmount * sales.ToCurrencyRate, 2),
                                BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * sales.ToCurrencyRate, 2),
                                //BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                                BooksCurrencyBaseRate = Math.Round(voucherVM.CompanyCurrencyRate * salesMaterialVM.TransactionRate, 4),
                                ModelState = ModelState.Added,
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null
                            };
                            _salesMaterialRepository.Insert(salesMaterial);
                            if (salesMaterialVM.TaxList != null && salesMaterialVM.TaxList.Count > 0)
                            {
                                foreach (var taxVM in salesMaterialVM.TaxList)
                                {
                                    if (string.IsNullOrEmpty(taxVM.Id))
                                    {
                                        if (taxVM.TaxCategoryId == null)
                                            throw new CustomException("Please Selete Tax Category !");

                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {
                                            Id = _pkGeneratorService.MakePK(salesMaterial.Id, currentSalesTaxId, 2),
                                            AddedBy = sales.AddedBy,
                                            AddedDate = sales.AddedDate,
                                            AddedFromIP = sales.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            //BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * sales.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = salesMaterial.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            SalesServiceId = null,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _salesTaxRepository.Insert(salesTax);
                                    }
                                    else
                                    {
                                        var salesTax = new SalesTax
                                        {
                                            Id = taxVM.Id,
                                            AddedBy = sales.AddedBy,
                                            AddedDate = sales.AddedDate,
                                            AddedFromIP = sales.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * sales.ToCurrencyRate, 2),
                                            //BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = salesMaterial.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            SalesServiceId = null,
                                            ModelState = ModelState.Modified,
                                            UpdatedBy = sales.UpdatedBy,
                                            UpdatedDate = sales.UpdatedDate,
                                            UpdatedFromIP = sales.UpdatedFromIP
                                        };
                                        _salesTaxRepository.Update(salesTax);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var salesMaterial = new SalesMaterial
                            {
                                Id = salesMaterialVM.Id,
                                SalesId = sales.Id,
                                MaterialMasterId = salesMaterialVM.MaterialMasterId,
                                ArticleId = salesMaterialVM.ArticleId,
                                //BaseUOMId = salesMaterialVM.BaseUOMId,
                                FirstCharacteristicsId = salesMaterialVM.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = salesMaterialVM.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = salesMaterialVM.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = salesMaterialVM.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = salesMaterialVM.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = salesMaterialVM.ThirdCharacteristicsValueId,
                                //BaseUOMId = salesMaterialVM.BaseUOMId,
                                BaseUOMId = salesMaterialVM.TransactionUoMId,
                                //BaseRate = salesMaterialVM.BaseRate,
                                BaseRate = salesMaterialVM.TransactionRate,
                                //BaseQty = salesMaterialVM.BaseQty,
                                BaseQty = salesMaterialVM.TransactionQty,
                                //BaseAmount = salesMaterialVM.BaseAmount,
                                BaseAmount = salesMaterialVM.TransactionAmount,
                                BaseUoMFactor = salesMaterialVM.BaseUoMFactor,
                                TransactionUoMId = salesMaterialVM.TransactionUoMId,
                                TransactionRate = salesMaterialVM.TransactionRate,
                                TransactionQty = salesMaterialVM.TransactionQty,
                                TransactionAmount = salesMaterialVM.TransactionAmount,
                                TaxAmount = salesMaterialVM.TaxAmount,
                                BooksCurrencyTransactionAmount = Math.Round(salesMaterialVM.TransactionAmount * sales.ToCurrencyRate, 2),
                                //BooksCurrencyTransactionAmount = Math.Round(salesMaterialVM.TransactionAmount * voucherVM.CompanyCurrencyRate),
                                //BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                                BooksCurrencyBaseRate = Math.Round(voucherVM.CompanyCurrencyRate * salesMaterialVM.TransactionRate, 4),
                                BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * sales.ToCurrencyRate, 2),
                                NetAmount = salesMaterialVM.TaxAmount + salesMaterialVM.TransactionAmount,
                                ModelState = ModelState.Modified,
                                UpdatedBy = sales.UpdatedBy,
                                UpdatedDate = sales.UpdatedDate,
                                UpdatedFromIP = sales.UpdatedFromIP
                            };
                            _salesMaterialRepository.Update(salesMaterial);
                            if (salesMaterialVM.TaxList != null && salesMaterialVM.TaxList.Count > 0)
                            {
                                foreach (var taxVM in salesMaterialVM.TaxList)
                                {
                                    if (string.IsNullOrEmpty(taxVM.Id))
                                    {
                                        if (taxVM.TaxCategoryId == null)
                                            throw new CustomException("Please Selete Tax Category !");

                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {
                                            Id = _pkGeneratorService.MakePK(salesMaterialVM.Id, currentSalesTaxId, 2),
                                            AddedBy = sales.AddedBy,
                                            AddedDate = sales.AddedDate,
                                            AddedFromIP = sales.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            //BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * sales.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = salesMaterial.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            SalesServiceId = null,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _salesTaxRepository.Insert(salesTax);
                                    }
                                    else
                                    {
                                        var salesTax = new SalesTax
                                        {
                                            Id = taxVM.Id,
                                            AddedBy = sales.AddedBy,
                                            AddedDate = sales.AddedDate,
                                            AddedFromIP = sales.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            //BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * sales.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = salesMaterial.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            SalesServiceId = null,
                                            ModelState = ModelState.Modified,
                                            UpdatedBy = sales.UpdatedBy,
                                            UpdatedDate = sales.UpdatedDate,
                                            UpdatedFromIP = sales.UpdatedFromIP
                                        };
                                        _salesTaxRepository.Update(salesTax);
                                    }
                                }
                            }
                        }

                    }
                }

                if (salesServiceVMList != null)
                {
                    foreach (var salesServiceVM in salesServiceVMList)
                    {
                        if (string.IsNullOrEmpty(salesServiceVM.Id))
                        {
                            currentSalesServiceId++;
                            var salesService = new Model.SalesManagements.SalesService
                            {
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                Amount = salesServiceVM.Amount,
                                //BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * sales.ToCurrencyRate, 2),
                                Id = _pkGeneratorService.MakePK(sales.Id, currentSalesServiceId, 2),
                                ModelState = ModelState.Added,
                                NetAmount = salesServiceVM.NetAmount,
                                SalesId = sales.Id,
                                ServiceMasterId = salesServiceVM.ServiceMasterId,
                                TaxAmount = salesServiceVM.TaxAmount,
                                //BooksCurrencyTaxAmount = Math.Round(salesServiceVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                                BooksCurrencyTaxAmount = Math.Round(salesServiceVM.TaxAmount * sales.ToCurrencyRate, 2),
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null
                            };
                            _salesServiceRepository.Insert(salesService);
                            if (salesServiceVM.ServiceTaxList != null && salesServiceVM.ServiceTaxList.Count > 0)
                            {
                                foreach (var taxVM in salesServiceVM.ServiceTaxList)
                                {
                                    if (string.IsNullOrEmpty(taxVM.Id))
                                    {
                                        if (taxVM.TaxCategoryId == null)
                                            throw new CustomException("Please Selete Tax Category !");

                                        currentServiceSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {
                                            Id = _pkGeneratorService.MakePK("S" + salesService.Id, currentServiceSalesTaxId, 2),
                                            AddedBy = sales.AddedBy,
                                            AddedDate = sales.AddedDate,
                                            AddedFromIP = sales.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            //BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * sales.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = null,
                                            SalesServiceId = salesService.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _salesTaxRepository.Insert(salesTax);
                                    }
                                    else
                                    {
                                        var salesTax = new SalesTax
                                        {
                                            Id = taxVM.Id,
                                            AddedBy = sales.AddedBy,
                                            AddedDate = sales.AddedDate,
                                            AddedFromIP = sales.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            //BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * sales.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = null,
                                            SalesServiceId = salesService.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = sales.UpdatedBy,
                                            UpdatedDate = sales.UpdatedDate,
                                            UpdatedFromIP = sales.UpdatedFromIP
                                        };
                                        _salesTaxRepository.Update(salesTax);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var salesService = new Model.SalesManagements.SalesService
                            {
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                Amount = salesServiceVM.Amount,
                                Id = salesServiceVM.Id,
                                ModelState = ModelState.Added,
                                NetAmount = salesServiceVM.NetAmount,
                                SalesId = sales.Id,
                                ServiceMasterId = salesServiceVM.ServiceMasterId,
                                TaxAmount = salesServiceVM.TaxAmount,
                                //BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * sales.ToCurrencyRate, 2),
                                BooksCurrencyTaxAmount = Math.Round(salesServiceVM.TaxAmount * sales.ToCurrencyRate, 2),
                                //BooksCurrencyTaxAmount = Math.Round(salesServiceVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null
                            };
                            _salesServiceRepository.Update(salesService);
                            if (salesServiceVM.ServiceTaxList != null && salesServiceVM.ServiceTaxList.Count > 0)
                            {
                                foreach (var taxVM in salesServiceVM.ServiceTaxList)
                                {
                                    if (string.IsNullOrEmpty(taxVM.Id))
                                    {
                                        if (taxVM.TaxCategoryId == null)
                                            throw new CustomException("Please Selete Tax Category !");

                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {
                                            Id = _pkGeneratorService.MakePK(salesServiceVM.Id, currentSalesTaxId, 2),
                                            AddedBy = sales.AddedBy,
                                            AddedDate = sales.AddedDate,
                                            AddedFromIP = sales.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            //BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * sales.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = null,
                                            SalesServiceId = salesService.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _salesTaxRepository.Insert(salesTax);
                                    }
                                    else
                                    {
                                        var salesTax = new SalesTax
                                        {
                                            Id = taxVM.Id,
                                            AddedBy = sales.AddedBy,
                                            AddedDate = sales.AddedDate,
                                            AddedFromIP = sales.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            //BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * sales.ToCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = null,
                                            SalesServiceId = salesService.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = sales.UpdatedBy,
                                            UpdatedDate = sales.UpdatedDate,
                                            UpdatedFromIP = sales.UpdatedFromIP
                                        };
                                        _salesTaxRepository.Update(salesTax);
                                    }
                                }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteTaxRow(string Id)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                _salesTaxRepository.Delete(Id);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteServiceTaxRow(string Id)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                _salesTaxRepository.Delete(Id);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void Delete(string id)
        {
            string strSQL, strPSQL, strBSQL, strOSQL, strSSQL, strASQL, strPSSQL, strUSC;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                //if (CheckUsing(id))
                //    throw new CustomException("First delete Operation!");

                strOSQL = "DELETE FROM TRN.SalesTax WHERE SalesId='" + id + "'";
                strASQL = "DELETE FROM TRN.SalesAdditionalTax WHERE SalesId='" + id + "'";
                strSSQL = "DELETE FROM TRN.SalesService WHERE SalesId='" + id + "'";
                strPSQL = "DELETE FROM TRN.SalesOrderItem WHERE SalesId='" + id + "'";
                strBSQL = "DELETE FROM TRN.SalesMaterial WHERE SalesId='" + id + "'";
                strPSSQL = "DELETE FROM dbo.PostSalesInvoice WHERE SalesId='" + id + "'";
                //strUSC = "update dbo.ItemScanChild set SalesId=NULL Where SalesId='" + id + "'";
                strSQL = "DELETE FROM TRN.Sales WHERE Id = '" + id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strASQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strPSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strBSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strPSSQL, true, "1");
                //objCon.ExecuteNonQueryWrapper(strUSC, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        public void DeleteSalesMaterial(string Id)
        {
            string strPSQL, strISCSQL, strBSQL, strOSQL, updatasc = null;
            DataSet dsMaster, dsSC;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                var smdata = _salesMaterialRepository.Find(Id);
                var secondCharacteristicsData = _secondCharacteristicsRepository.Query(t => t.SalesOrderId == smdata.SalesOrderId && t.Id == smdata.SecondCharacteristicsId).Select(t => t.SalesQty).FirstOrDefault();
                if (secondCharacteristicsData != 0)
                {
                    updatasc = "Update TRN.SecondCharacteristics set SalesQty=" + secondCharacteristicsData + "-" + smdata.BaseQty + " WHERE SalesOrderId='" + smdata.SalesOrderId + "' AND Id='" + smdata.SecondCharacteristicsId + "'";
                }

                strISCSQL = "Update ItemScanChild set SalesMaterialId=NULL,SalesId=NULL,IsDespatch=0 WHERE SalesMaterialId='" + Id + "'";
                strOSQL = "DELETE FROM TRN.SalesTax WHERE SalesMaterialId='" + Id + "'";
                strBSQL = "DELETE FROM TRN.SalesMaterial WHERE Id='" + Id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strISCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strBSQL, true, "1");
                if (secondCharacteristicsData != 0)
                    objCon.ExecuteNonQueryWrapper(updatasc, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }

        }

        public void CancelSalesMaterial(string Id, string remark)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strISCSQL, strBSQL, strOSQL = null;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {

                strOSQL = "Update  TRN.SalesTax set BooksCurrencyTransactionAmount=0,Amount=0 WHERE SalesMaterialId='" + Id + "'";
                strBSQL = "Update  TRN.SalesMaterial set TaxAmount=0,NetAmount=0,BaseQty=0,BaseAmount=0,TransactionQty=0,TransactionAmount=0,IsCanceled=1,Remark='" + remark + "',CanceledBy='" + identity.UserId + "' WHERE Id='" + Id + "'";
                strISCSQL = "Update  ItemScanChild set SalesMaterialId=NULL,SalesId=NULL,IsDespatch=0 WHERE SalesMaterialId='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strBSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strISCSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }

        }

        public void DeleteSalesService(string Id)
        {
            string strPSQL, strBSQL, strOSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strOSQL = "DELETE FROM TRN.SalesTax WHERE SalesServiceId='" + Id + "'";
                strBSQL = "DELETE FROM TRN.SalesService WHERE Id='" + Id + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strOSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strBSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon = null;
            }

        }

        public void SalesInvoicePost(VoucherViewModel voucherVM, IEnumerable<VoucherDetailViewModel> salesJVDetail, IEnumerable<SalesMaterialViewModel> salesMaterialDetailGLList, IEnumerable<SalesServiceViewModel> salesServiceDetailGLList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);

                var taxYear = CheckingFiscalYearPeriod(voucherVM.CompanyGroupId, voucherVM.PostingDate);
                voucherVM.TaxYearId = taxYear["TaxYearId"].ToString();
                voucherVM.TaxYearPeriodId = taxYear["TaxYearPeriodId"].ToString();
                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // Get Sales
                var sales = _salesRepository.Find(voucherVM.Id);
                sales.PaymentTermId = voucherVM.PaymentTermId;
                sales.BaseNoOfDays = voucherVM.BaseNoOfDays;
                sales.BaseOnDueDate = voucherVM.BaseOnDueDate;
                sales.MatureDate = voucherVM.MatureDate;
                sales.RowState = "Posted";

                var invoice = new Invoice
                {
                    Amount = salesJVDetail.Where(r => r.OtherName == "Customer" && r.TrnType == "Dr").Sum(r => r.Amount),
                    BaseNoOfDays = voucherVM.BaseNoOfDays,
                    BaseOnDueDate = voucherVM.BaseOnDueDate,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.PostingDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    EntityId = voucherVM.EntityId,
                    PlantId = voucherVM.PlantId,
                    IsExcludingTax = voucherVM.IsExcludingTax,
                    IsSplit = voucherVM.IsSplit,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    DeliveryPartyPlantId = voucherVM.DeliveryPartyPlantId,
                    PartyType = PartyType.Customer.ToString(),
                    EmployeeId = voucherVM.EmployeeId,
                    PaymentTermId = voucherVM.PaymentTermId,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    RevisedDueDate = voucherVM.MatureDate,
                    ActualDueDate = voucherVM.MatureDate,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate
                };
                _invoiceService.InsertInvoice(invoice);

                var voucherType = _voucherTypeMatrixRepository.Query(r => r.SourceType == "SalesInvoice").Select().FirstOrDefault();
                if (null == voucherType)
                    throw new CustomException("Voucher Type not found!");
                voucherVM.VoucherTypeId = voucherType.VoucherTypeId;
                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = invoice.CompanyGroupId,
                    CompanyId = invoice.CompanyId,
                    PlantId = invoice.PlantId,
                    EntityId = invoice.EntityId,
                    CurrencyId = invoice.CurrencyId,
                    FiscalYearId = invoice.FiscalYearId,
                    FiscalYearPeriodId = invoice.FiscalYearPeriodId,
                    TaxYearId = invoice.TaxYearId,
                    TaxYearPeriodId = invoice.TaxYearPeriodId,
                    AddedBy = invoice.AddedBy,
                    AddedDate = invoice.AddedDate,
                    AddedFromIP = invoice.AddedFromIP,
                    VoucherDate = invoice.VoucherDate,
                    DocDate = invoice.PostingDate,
                    DocRefNo = invoice.DocRefNo,
                    Archive = invoice.Archive,
                    IsPark = invoice.IsPark,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                AuditService.PostedLog(voucher);
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                invoice.VoucherId = voucher.Id;
                sales.VoucherId = voucher.Id;
                _salesRepository.Update(sales);

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                var currentTaxRecord = 0;
                var totalAmountCr = 0.0M;
                var totalTaxCr = 0.0M;
                decimal totalAmountDr = 0;
                //sales Insert into Invoice and voucher
                //var salesTaxVMList = GetSalesTaxGroupTaxCategory(voucherVM.Id).Select().ToList();
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                if (salesJVDetail != null)
                {
                    var invoiceDetail = new InvoiceDetail
                    {
                        AddedBy = invoice.AddedBy,
                        AddedDate = invoice.AddedDate,
                        AddedFromIP = invoice.AddedFromIP,
                        Archive = invoice.Archive
                    };
                    foreach (var voucherDetailVM in salesJVDetail)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                            throw new CustomException("Without Budget can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                            throw new CustomException("Without Activity can not post.");
                        if (voucherDetailVM.TrnType == "Dr")
                        {
                            if (voucherDetailVM.OtherName == "Customer")
                            {
                                invoiceDetail.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                                invoiceDetail.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                                invoiceDetail.ActivityId = voucherDetailVM.ActivityId;
                                invoiceDetail.Amount = voucherDetailVM.Amount;
                                invoiceDetail.NetAmount = voucherDetailVM.Amount;
                                invoiceDetail.InvoiceId = invoice.Id;
                                InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                            }

                            var voucherDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                DrAmount = voucherDetailVM.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = sales.Narration,
                                InvoiceDetailId = invoiceDetail.Id,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            if (voucherDetailVM.OtherName == "Customer")
                            {
                                voucherDr.PartyId = sales.PartyId;
                                voucherDr.PartyType = voucherDetailVM.OtherName;
                                voucherDr.PartyPlantId = sales.InvoicingPartyPlantId;
                            }
                            totalAmountDr += voucherDr.DrAmount;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);
                            voucherDetailVM.VoucherDetailId = voucherDr.Id;




                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = sales.ToCurrencyRate,
                                ToCurrencyConversion = 1 / sales.ToCurrencyRate,
                                DrAmount = voucherDr.DrAmount * sales.ToCurrencyRate
                            });

                            if (voucherDetailVM.OtherName == "TaxReceivable" || voucherDetailVM.OtherName == "SVTaxReceivable" || voucherDetailVM.OtherName == "TCSReceivable")
                            {
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherDr.Id,
                                    VoucherId = voucher.Id,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucher.TaxYearId,
                                    TaxYearPeriodId = voucher.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxCodeId = voucherDetailVM.TaxCodeId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.SalesInvoice.ToString(),
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP
                                };
                                _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    Id = invoiceTax.Id + 1,
                                    InvoiceTaxId = invoiceTax.Id,
                                    Amount = invoiceTax.TaxAmount,
                                    GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Dr",
                                    AddedBy = invoiceTax.AddedBy,
                                    AddedDate = invoiceTax.AddedDate,
                                    AddedFromIP = invoiceTax.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            }
                        }
                        if (voucherDetailVM.TrnType == "Cr")
                        {
                            var voucherCr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CrAmount = voucherDetailVM.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = sales.Narration,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            totalAmountCr += voucherCr.CrAmount;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                            voucherDetailVM.VoucherDetailId = voucherCr.Id;
                            //_salesMaterialRepository.Update(voucherDetailVM);

                            if (voucherDetailVM.OtherName == "Sales")
                            {
                                foreach (var item in salesMaterialDetailGLList.Where(r => r.GLGeneralInfoId == voucherCr.GLGeneralInfoId
                           && r.BudgetMasterId == voucherCr.BudgetMasterId && r.ActivityId == voucherCr.ActivityId))
                                {
                                    if (item.SalesMaterialId != null)
                                    {
                                        var salesMaterial = _salesMaterialRepository.Find(item.SalesMaterialId);
                                        salesMaterial.VoucherDetailId = voucherCr.Id;
                                        salesMaterial.ModelState = ModelState.Modified;
                                        salesMaterial.PostCrGLGeneralInfoId = voucherCr.GLGeneralInfoId;
                                        salesMaterial.PostCrBudgetMasterId = voucherCr.BudgetMasterId;
                                        salesMaterial.PostCrActivityId = voucherCr.ActivityId;
                                        salesMaterial.PostDrGLGeneralInfoId = invoiceDetail.GLGeneralInfoId;
                                        salesMaterial.PostDrBudgetMasterId = invoiceDetail.BudgetMasterId;
                                        salesMaterial.PostDrActivityId = invoiceDetail.ActivityId;
                                        AuditService.UpdatedLog(salesMaterial);
                                        _salesMaterialRepository.Update(salesMaterial);
                                    }

                                }
                            }

                            if (voucherDetailVM.OtherName == "Service")
                            {
                                foreach (var item in salesServiceDetailGLList.Where(r => r.GLGeneralInfoId == voucherCr.GLGeneralInfoId
                           && r.BudgetMasterId == voucherCr.BudgetMasterId && r.ActivityId == voucherCr.ActivityId))
                                {
                                    if (item.SalesServiceId != null)
                                    {
                                        var salesService = _salesServiceRepository.Find(item.SalesServiceId);
                                        salesService.VoucherDetailId = voucherCr.Id;
                                        salesService.ModelState = ModelState.Modified;
                                        AuditService.UpdatedLog(salesService);
                                        _salesServiceRepository.Update(salesService);
                                    }

                                }
                            }

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherCr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = sales.ToCurrencyRate,
                                ToCurrencyConversion = 1 / sales.ToCurrencyRate,
                                CrAmount = voucherCr.CrAmount * sales.ToCurrencyRate
                            });

                            if (voucherDetailVM.OtherName == "TaxPayable" || voucherDetailVM.OtherName == "SVTaxPayable" || voucherDetailVM.OtherName == "TCSPayable")
                            {
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherCr.Id,
                                    VoucherId = voucher.Id,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucher.TaxYearId,
                                    TaxYearPeriodId = voucher.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxCodeId = voucherDetailVM.TaxCodeId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.SalesInvoice.ToString(),
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP
                                };
                                _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    Id = invoiceTax.Id + 1,
                                    InvoiceTaxId = invoiceTax.Id,
                                    Amount = invoiceTax.TaxAmount,
                                    GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Cr",
                                    AddedBy = invoiceTax.AddedBy,
                                    AddedDate = invoiceTax.AddedDate,
                                    AddedFromIP = invoiceTax.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
        private static string MakePK(string masterId, int currentId, int padLeft)
        {
            return masterId + currentId.ToString().PadLeft(padLeft, '0');
        }

        private Dictionary<string, object> CheckingFiscalYearPeriod(string groupId, DateTime postingDate)
        {
            var sql = @"SELECT A.TaxYearId, A.Id AS TaxYearPeriodId, B.TaxYearCode, B.TaxYearName, A.PeriodNo, A.PeriodName FROM [SCS].[TaxYearPeriod] A
                        JOIN [SCS].[TaxYear] AS B ON A.TaxYearId=B.Id WHERE B.CompanyGroupId='" + groupId + @"' AND CAST('" + postingDate + @"' AS DATE) BETWEEN  CAST(A.StartDate AS DATE) AND  CAST(A.EndDate AS DATE)";
            var data = _sqlRepository.GetData(sql);
            if (null == data || data.Count == 0)
                throw new CustomException("Tax year not found");
            return data;
        }


        public GridModel GetMaterialSalesList(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                //parameters.sort = "CAST(AddedDate AS datetime)";
                //parameters.sort = "TAB.AddedDate,TAB.InvoiceNo";
                parameters.CmdText = @"SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId,CO.BaseCurrencyId, C.Code AS CurrencyCode, S.DocRefNo, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									 Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId, S.BaseNoOfDays
                                    , Replace(CONVERT(VARCHAR(11), S.BaseOnDueDate, 106), ' ', '-') BaseOnDueDate
									, S.InvoiceNo, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, Replace(CONVERT(VARCHAR(11), S.MatureDate, 106), ' ', '-')  MatureDate, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId,S.BLNumber,S.ItemDescription,S.ComercialInvoiceNo,S.EXPFromNo,S.EXPDate,S.BLDate
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark,S.AddedDate,s.AddedBy,S.AddedFromIP,FORMAT(S.UpdatedDate,'dd-MMM-yyyy') UpdatedDate,s.UpdatedBy,S.UpdatedFromIP
                                    , CP.TaxApplicable,CP.PartyAccountGroupId,CP.IsPaymentTermChangeable
                                    , V.VoucherNo,S.PaymentToReceiveBankId
									FROM [TRN].[Sales] AS S
                                    LEFT JOIN [ORG].[Company] AS CO ON CO.Id=S.CompanyId
                                    JOIN [HKP].[Party] AS P ON P.Id=S.PartyId
									LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND S.PlantId=CP.PlantId AND CP.PartyType='Customer'
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=S.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=S.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=S.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=S.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
                                    LEFT JOIN [TRN].[Voucher] V ON V.Id=S.VoucherId
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "' AND S.SourceType='Sales'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        public GridModel GetMasterOrderSalesList(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                //parameters.sort = "CAST(AddedDate AS datetime)";
                //parameters.sort = "TAB.AddedDate,TAB.InvoiceNo";
                parameters.CmdText = @"SELECT S.Id,S.Id AS SalesId, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId,CO.BaseCurrencyId, C.Code AS CurrencyCode, S.DocRefNo, ISNULL(SM.Amount,0) + ISNULL(SS.Amount,0) AS Amount,
									 Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') InvoiceDate,
									Replace(CONVERT(VARCHAR(11), S.EntryDate, 106), ' ', '-') VoucherDate, Replace(CONVERT(VARCHAR(11), S.InvoiceDate, 106), ' ', '-') PostingDate
                                    , S.RowState, S.DeliveryPartyPlantId, S.InvoicingPartyPlantId AS PartyPlantId, S.InvoicingPartyPlantId, S.EntityId, S.PaymentTermId, S.BaseNoOfDays, S.BaseOnDueDate
									, S.InvoiceNo, PPI.UserName AS BillTo, AM.StateId AS InvoicingStateId, ST.UserName AS InvoicingState, PPI.GSTIN AS InvoicingGSTIN
									, PPD.UserName AS ShipTo, STD.UserName AS DeliveryState, PPD.GSTIN AS DeliveryGSTIN, S.InvoicingByAddress, S.DeliveryByAddress, S.MatureDate, S.ToCurrencyRate
									, S.ToCurrencyRate AS CompanyCurrencyRate, S.Narration, S.PartyType, S.VoucherId, AMP.StateId AS PlantStateId,S.BLNumber,S.ItemDescription,S.ComercialInvoiceNo,S.EXPFromNo,S.EXPDate,S.BLDate
                                    , CASE  WHEN S.RowState='Parked' THEN 1 ELSE 0 END AS IsPark,S.AddedDate,s.AddedBy,S.AddedFromIP,FORMAT(S.UpdatedDate,'dd-MMM-yyyy') UpdatedDate,s.UpdatedBy,S.UpdatedFromIP,S.IsAdditionalInfoApplicable,S.PaymentToReceiveBankId
									FROM [TRN].[Sales] AS S
                                    LEFT JOIN [ORG].[Company] AS CO ON CO.Id=S.CompanyId
                                    JOIN [HKP].[Party] AS P ON P.Id=S.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=S.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=S.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=S.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=S.PlantId
									LEFT JOIN [MST].[AddressMaster] AS AMP ON AMP.Id=PT.AddressMasterId
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesMaterial] M GROUP BY M.SalesId) AS SM ON SM.SalesId=S.Id
									LEFT JOIN (SELECT M.SalesId,SUM(M.NetAmount) AS Amount FROM [TRN].[SalesService] M GROUP BY M.SalesId) AS SS ON SS.SalesId=S.Id
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "'  AND SourceType='MasterOrderSales'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }


        public List<Dictionary<string, object>> GetSalesServiceData(string companyGroupId, string companyId, string plantId, string salesId)
        {
            var cmdText = @"SELECT SS.Id, SS.SalesId, SS.ServiceMasterId, SS.Amount, SS.TaxAmount, NetAmount=SS.Amount+ SS.TaxAmount, SM.UserName AS ChargeName, NULL ServiceTaxList 
								FROM TRN.SalesService AS SS 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SS.SalesId
                                LEFT JOIN HKP.ServiceMaster SM ON SM.Id=SS.ServiceMasterId
								WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' AND SA.Id='" + salesId + @"'";
            return _sqlRepository.GetDataCollection(cmdText);
        }

        public List<Dictionary<string, object>> GetSalesTaxData(string companyGroupId, string companyId, string plantId, string salesId)
        {
            var cmdText = @"SELECT ST.Id, ST.SalesId, ST.SalesMaterialId, ST.TaxCategoryId, TC.UserName AS TaxCategory,ST.HSNCodeId, HC.Code, ST.[Percentage], ST.Amount AS TotalAmount
								FROM TRN.SalesTax AS ST 
								LEFT JOIN TRN.SalesMaterial AS SM ON SM.Id=ST.SalesMaterialId
								LEFT JOIN TRN.Sales AS SA ON SA.Id=ST.SalesId
								LEFT JOIN MST.TaxCategory AS TC ON TC.Id=ST.TaxCategoryId
								LEFT JOIN HKP.HSNCode AS HC ON HC.Id=ST.HSNCodeId
								WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' AND SA.Id='" + salesId + @"' AND ST.SalesServiceId IS NULL";
            return _sqlRepository.GetDataCollection(cmdText);
        }

        public List<Dictionary<string, object>> GetSalesServiceTaxData(string companyGroupId, string companyId, string plantId, string salesId)
        {
            var cmdText = @"SELECT ST.Id, ST.SalesId, ST.SalesServiceId, ST.TaxCategoryId, TC.UserName AS TaxCategory,ST.HSNCodeId, HC.Code, ST.[Percentage], ST.Amount
								FROM TRN.SalesTax AS ST 
								LEFT JOIN TRN.SalesService AS SM ON SM.Id=ST.SalesServiceId
								LEFT JOIN TRN.Sales AS SA ON SA.Id=ST.SalesId
								LEFT JOIN MST.TaxCategory AS TC ON TC.Id=ST.TaxCategoryId
								LEFT JOIN HKP.HSNCode AS HC ON HC.Id=ST.HSNCodeId
								WHERE SA.CompanyGroupId='" + companyGroupId + "' AND SA.CompanyId='" + companyId + "' AND SA.PlantId='" + plantId + "' AND SA.Id='" + salesId + @"' AND ST.SalesMaterialId IS NULL";
            return _sqlRepository.GetDataCollection(cmdText);
        }

        public GridModel GetSalesPendingList(GridParameter parameters, string companyGroupId, string companyId)
        {
            try
            {
                parameters.CmdText = @"SELECT S.Id, S.PartyId, P.Code AS PartyCode, P.UserName AS PartyName, S.CurrencyId, C.Code AS CurrencyCode, S.DocRefNo, S.InvoiceDate, S.InvoiceNo, PPD.UserName AS BillTo, S.RowState
									FROM [TRN].[Sales] AS S
                                    JOIN [HKP].[Party] AS P ON P.Id=S.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=S.DeliveryPartyPlantId
                                    JOIN [SCS].[Currency] AS C ON C.Id=S.CurrencyId
                                    WHERE S.CompanyGroupId='" + companyGroupId + "' AND S.CompanyId='" + companyId + "' AND S.RowState='Submit'";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
        }

        private DataTable GetSalesTaxGroupTaxCategory(string salesId)
        {
            var cmdText = @"SELECT [Percentage], SUM(Amount) AS Amount, TaxCategoryId FROM TRN.SalesTax WHERE SalesId='" + salesId + @"'
						 GROUP BY TaxCategoryId,[Percentage]";
            return _sqlRepository.GetDataTable(cmdText);
        }

        private DataTable GetSalesAdditionalTaxGroupTaxCategory(string salesId)
        {
            var cmdText = @"SELECT [Percentage], SUM(TaxAmount) AS TaxAmount, TaxCategoryId,TaxCodeId FROM TRN.SalesAdditionalTax WHERE SalesId='" + salesId + @"'
						 GROUP BY TaxCategoryId,[Percentage],TaxCodeId";
            return _sqlRepository.GetDataTable(cmdText);
        }



        #endregion

        #region Master Order Sales Invoice

        public IEnumerable<object> GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId, string PODate)
        {
            try
            {
                var sql = @"DECLARE @receiveId varchar(100)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + plantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.Id=@receiveId)-- AND AD.Active=1 AND AD.Archive=0)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, HP.[Percentage] AS [Percentage], NULL TotalAmount
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId AND convert(DATE, EffectiveDate)<='" + PODate + @"') AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + companyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticSales + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasSales + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }


        public void MasterOrderSalesInsert(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesOrderItem> selectedMasterOrderList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);

                _unitOfWork.BeginTransaction();
                flag = true;

                var sales = new Sales
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    //DocDate = voucherVM.DocDate,

                    CurrencyId = voucherVM.CurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    BaseNoOfDays = voucherVM.BaseNoOfDays,
                    BaseOnDueDate = voucherVM.BaseOnDueDate,
                    DeliveryPartyPlantId = voucherVM.DeliveryPartyPlantId,
                    EntryDate = voucherVM.VoucherDate,
                    InvoiceDate = voucherVM.InvoiceDate,
                    InvoicingPartyPlantId = voucherVM.InvoicingPartyPlantId,
                    MatureDate = voucherVM.MatureDate,
                    PartyId = voucherVM.PartyId,
                    PartyType = voucherVM.PartyType,
                    Narration = voucherVM.Narration,
                    PaymentTermId = voucherVM.PaymentTermId,
                    RowState = RowState.Parked.ToString(),
                    DeliveryByAddress = voucherVM.DeliveryByAddress,
                    InvoicingByAddress = voucherVM.InvoicingByAddress,
                    ModelState = ModelState.Added,
                    BLNumber = voucherVM.BLNumber,
                    ItemDescription = voucherVM.ItemDescription,
                    InvoiceStatus = voucherVM.InvoiceStatus,
                    BLDate = voucherVM.BLDate,
                    EXPDate = voucherVM.EXPDate,
                    EXPFromNo = voucherVM.EXPFromNo,
                    ComercialInvoiceNo = voucherVM.ComercialInvoiceNo,
                    IsAdditionalInfoApplicable = voucherVM.IsAdditionalInfoApplicable,
                    PaymentToReceiveBankId = voucherVM.PaymentToReceiveBankId,
                    TrancastionTypeId = voucherVM.TrancastionTypeId,
                    SourceType = "MasterOrderSales",
                    Id = "MS" + _pkGeneratorService.GetAutoNumber(nameof(Sales), PKGeneratorEnum.Yearly, null, DateTime.Now),
                };
                sales.DocRefNo = sales.Id;
                sales.InvoiceNo = sales.Id;
                voucherVM.Id = sales.Id;
                AuditService.AddedLog(sales);
                _salesRepository.Insert(sales);

                var currentSalesMaterialId = 0;
                var currentSalesOrderItemId = 0;
                var currentSalesServiceId = 0;
                var currentSalesTaxId = 0;
                if (salesMaterialVMList != null)
                {
                    foreach (var salesMaterialVM in salesMaterialVMList)
                    {
                        currentSalesMaterialId++;
                        var salesMaterial = new SalesMaterial
                        {
                            Id = _pkGeneratorService.MakePK(sales.Id, currentSalesMaterialId, 3),
                            SalesId = sales.Id,
                            MaterialMasterId = salesMaterialVM.MaterialMasterId,
                            SalesOrderId = salesMaterialVM.SalesOrderId,
                            ArticleId = salesMaterialVM.ArticleId,
                            FirstCharacteristicsId = salesMaterialVM.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = salesMaterialVM.FirstCharacteristicsValueId,
                            SecondCharacteristicsId = salesMaterialVM.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = salesMaterialVM.SecondCharacteristicsValueId,
                            ThirdCharacteristicsId = salesMaterialVM.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = salesMaterialVM.ThirdCharacteristicsValueId,
                            BaseUOMId = salesMaterialVM.BaseUOMId,
                            BaseRate = salesMaterialVM.BaseRate,
                            BaseQty = salesMaterialVM.SalesQty,
                            BaseAmount = Math.Round(salesMaterialVM.SalesQty * salesMaterialVM.BaseRate, 2),
                            BaseUoMFactor = salesMaterialVM.BaseUoMFactor,
                            TransactionUoMId = salesMaterialVM.BaseUOMId,
                            TransactionRate = salesMaterialVM.TransactionRate,
                            TransactionQty = salesMaterialVM.SalesQty,
                            TransactionAmount = Math.Round(salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty, 2),
                            BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2),
                            BooksCurrencyBaseRate = Math.Round(voucherVM.CompanyCurrencyRate * salesMaterialVM.TransactionRate, 4),

                            TaxAmount = salesMaterialVM.TaxAmount,
                            BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                            NetAmount = salesMaterialVM.NetAmount,
                            ModelState = ModelState.Added,
                            AddedBy = sales.AddedBy,
                            AddedDate = sales.AddedDate,
                            AddedFromIP = sales.AddedFromIP,
                            UpdatedBy = null,
                            UpdatedDate = null,
                            UpdatedFromIP = null,
                            IsCanceled = false,
                            CanceledBy = null,
                            Remark = null
                        };

                        if (voucherVM.CurrencyId != companyCurrencyId)
                        {
                            salesMaterial.BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2);
                        }
                        _salesMaterialRepository.Insert(salesMaterial);

                        var firstCharacteristics = _firstCharacteristicsRepository.Find(salesMaterialVM.FirstCharacteristicsId);

                        if (firstCharacteristics != null)
                        {
                            var secondCharacteristics = _secondCharacteristicsRepository.Find(salesMaterialVM.SecondCharacteristicsId);
                            var thirdCharacteristics = _thirdCharacteristicsRepository.Find(salesMaterialVM.ThirdCharacteristicsId);

                            firstCharacteristics.SalesQty += salesMaterialVM.SalesQty;
                            _firstCharacteristicsRepository.Update(firstCharacteristics);


                            if (secondCharacteristics != null)
                            {
                                secondCharacteristics.SalesQty += salesMaterialVM.SalesQty;
                                _secondCharacteristicsRepository.Update(secondCharacteristics);
                            }

                            if (thirdCharacteristics != null)
                            {
                                thirdCharacteristics.SalesQty += salesMaterialVM.SalesQty;
                                _thirdCharacteristicsRepository.Update(thirdCharacteristics);
                            }
                        }

                        if (salesMaterialVM.TaxList != null && salesMaterialVM.TaxList.Count > 0)
                        {
                            foreach (var taxVM in salesMaterialVM.TaxList)
                            {
                                if (taxVM.TaxCategoryId == null)
                                    throw new CustomException("Please Select Tax Category !");

                                currentSalesTaxId++;
                                var salesTax = new SalesTax
                                {
                                    Id = _pkGeneratorService.MakePK(salesMaterial.Id, currentSalesTaxId, 2),
                                    AddedBy = salesMaterial.AddedBy,
                                    AddedDate = salesMaterial.AddedDate,
                                    AddedFromIP = salesMaterial.AddedFromIP,
                                    Amount = taxVM.TotalAmount,
                                    BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                    HSNCodeId = taxVM.HSNCodeId,
                                    Percentage = taxVM.Percentage,
                                    SalesId = sales.Id,
                                    SalesMaterialId = salesMaterial.Id,
                                    TaxCategoryId = taxVM.TaxCategoryId,
                                    SalesServiceId = null,
                                    ModelState = ModelState.Added,
                                    UpdatedBy = null,
                                    UpdatedDate = null,
                                    UpdatedFromIP = null
                                };
                                _salesTaxRepository.Insert(salesTax);
                            }
                        }
                    }
                }

                if (selectedMasterOrderList != null)
                {
                    foreach (var item in selectedMasterOrderList)
                    {
                        currentSalesOrderItemId++;
                        var salesMaterialSo = new SalesOrderItem
                        {
                            Id = _pkGeneratorService.MakePK(sales.Id, currentSalesOrderItemId, 2),

                            MasterOrderId = item.MasterOrderId,
                            SalesId = sales.Id,
                            MasterOrderItemId = item.MasterOrderItemId,
                            ModelState = ModelState.Added,
                            AddedBy = sales.AddedBy,
                            AddedDate = sales.AddedDate,
                            AddedFromIP = sales.AddedFromIP,
                            UpdatedBy = null,
                            UpdatedDate = null,
                            UpdatedFromIP = null
                        };
                        _salesMaterialSORepository.Insert(salesMaterialSo);
                    }
                }

                if (salesServiceVMList != null)
                {
                    foreach (var salesServiceVM in salesServiceVMList)
                    {

                        currentSalesServiceId++;
                        var salesService = new Model.SalesManagements.SalesService
                        {
                            AddedBy = sales.AddedBy,
                            AddedDate = sales.AddedDate,
                            AddedFromIP = sales.AddedFromIP,
                            Amount = salesServiceVM.Amount,
                            BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                            Id = _pkGeneratorService.MakePK(sales.Id, currentSalesServiceId, 2),
                            ModelState = ModelState.Added,
                            NetAmount = salesServiceVM.NetAmount,
                            SalesId = sales.Id,
                            ServiceMasterId = salesServiceVM.ServiceMasterId,
                            TaxAmount = salesServiceVM.TaxAmount,
                            UpdatedBy = null,
                            UpdatedDate = null,
                            UpdatedFromIP = null
                        };
                        _salesServiceRepository.Insert(salesService);

                        if (salesServiceVM.ServiceTaxList != null && salesServiceVM.ServiceTaxList.Count > 0)
                        {
                            foreach (var taxVM in salesServiceVM.ServiceTaxList)
                            {
                                if (taxVM.TaxCategoryId == null)
                                    throw new CustomException("Please Select Tax Category !");

                                currentSalesTaxId++;
                                var salesTax = new SalesTax
                                {
                                    Id = _pkGeneratorService.MakePK(salesService.Id, currentSalesTaxId, 2),
                                    AddedBy = salesService.AddedBy,
                                    AddedDate = salesService.AddedDate,
                                    AddedFromIP = salesService.AddedFromIP,
                                    Amount = taxVM.Amount,
                                    BooksCurrencyTransactionAmount = Math.Round(taxVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                    HSNCodeId = taxVM.HSNCodeId,
                                    Percentage = taxVM.Percentage,
                                    SalesId = sales.Id,
                                    SalesMaterialId = null,
                                    SalesServiceId = salesService.Id,
                                    TaxCategoryId = taxVM.TaxCategoryId,
                                    ModelState = ModelState.Added,
                                    UpdatedBy = null,
                                    UpdatedDate = null,
                                    UpdatedFromIP = null
                                };
                                _salesTaxRepository.Insert(salesTax);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void MasterOrderSalesUpdate(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesOrderItem> selectedMasterOrderList, IEnumerable<SalesServiceViewModel> salesServiceVMList)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _unitOfWork.BeginTransaction();
                flag = true;
                var sales = _salesRepository.Find(voucherVM.Id);

                sales.CompanyGroupId = voucherVM.CompanyGroupId;
                sales.CompanyId = voucherVM.CompanyId;
                sales.PlantId = voucherVM.PlantId;
                sales.EntityId = voucherVM.EntityId;

                sales.CurrencyId = voucherVM.CurrencyId;
                sales.ToCurrencyRate = voucherVM.CompanyCurrencyRate;
                sales.BaseNoOfDays = voucherVM.BaseNoOfDays;
                sales.BaseOnDueDate = voucherVM.BaseOnDueDate;
                sales.DeliveryPartyPlantId = voucherVM.DeliveryPartyPlantId;
                sales.EntryDate = voucherVM.VoucherDate;
                sales.InvoiceDate = voucherVM.InvoiceDate;
                sales.InvoicingPartyPlantId = voucherVM.InvoicingPartyPlantId;
                sales.MatureDate = voucherVM.MatureDate;
                sales.PartyId = voucherVM.PartyId;
                sales.PartyType = voucherVM.PartyType;
                sales.Narration = voucherVM.Narration;
                sales.PaymentTermId = voucherVM.PaymentTermId;
                sales.RowState = RowState.Parked.ToString();
                sales.DeliveryByAddress = voucherVM.DeliveryByAddress;
                sales.InvoicingByAddress = voucherVM.InvoicingByAddress;
                sales.ComercialInvoiceNo = voucherVM.ComercialInvoiceNo;
                sales.BLNumber = voucherVM.BLNumber;
                sales.ItemDescription = voucherVM.ItemDescription;
                sales.BLDate = voucherVM.BLDate;
                sales.EXPDate = voucherVM.EXPDate;
                sales.EXPFromNo = voucherVM.EXPFromNo;
                sales.InvoiceStatus = voucherVM.InvoiceStatus;
                sales.IsAdditionalInfoApplicable = voucherVM.IsAdditionalInfoApplicable;
                sales.PaymentToReceiveBankId = voucherVM.PaymentToReceiveBankId;
                sales.UpdatedBy = voucherVM.UpdatedBy;
                sales.UpdatedDate = voucherVM.UpdatedDate;
                sales.UpdatedFromIP = voucherVM.UpdatedFromIP;
                sales.SourceType = "MasterOrderSales";
                sales.TrancastionTypeId = voucherVM.TrancastionTypeId;
                sales.ModelState = ModelState.Modified;

                sales.DocRefNo = sales.Id;
                sales.InvoiceNo = sales.Id;
                AuditService.UpdatedLog(sales);
                _salesRepository.Update(sales);

                var currentSalesMaterialId = 0;
                var currentSalesServiceId = 0;
                var currentSalesOrderItemId = 0;
                var currentSalesTaxId = 0;
                if (salesMaterialVMList != null)
                {
                    var historyId = _salesMaterialRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM TRN.SalesMaterial WHERE SalesId='{sales.Id}'").First();
                    //var historyId = _salesMaterialRepository.SqlQuery<int>($"SELECT ISNULL(COUNT(Id), 0) Id FROM TRN.SalesMaterial WHERE SalesId='{sales.Id}'").First();
                    foreach (var salesMaterialVM in salesMaterialVMList)
                    {
                        currentSalesMaterialId++;


                        if (string.IsNullOrEmpty(salesMaterialVM.Id))
                        {
                            historyId++;
                            var salesMaterial = new SalesMaterial
                            {
                                Id = _pkGeneratorService.MakePK(sales.Id, historyId, 3),
                                SalesId = sales.Id,
                                SalesOrderId = salesMaterialVM.SalesOrderId,
                                MaterialMasterId = salesMaterialVM.MaterialMasterId,
                                ArticleId = salesMaterialVM.ArticleId,
                                FirstCharacteristicsId = salesMaterialVM.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = salesMaterialVM.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = salesMaterialVM.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = salesMaterialVM.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = salesMaterialVM.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = salesMaterialVM.ThirdCharacteristicsValueId,
                                BaseUOMId = salesMaterialVM.BaseUOMId,
                                BaseRate = salesMaterialVM.BaseRate,
                                //BaseQty = salesMaterialVM.BaseQty,
                                BaseQty = salesMaterialVM.SalesQty,
                                BaseAmount = Math.Round(salesMaterialVM.SalesQty * salesMaterialVM.BaseRate, 2),

                                BaseUoMFactor = salesMaterialVM.BaseUoMFactor,
                                TransactionUoMId = salesMaterialVM.BaseUOMId,

                                TransactionRate = salesMaterialVM.TransactionRate,
                                TransactionQty = salesMaterialVM.SalesQty,
                                TransactionAmount = Math.Round(salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty, 2),
                                TaxAmount = salesMaterialVM.TaxAmount,
                                NetAmount = salesMaterialVM.NetAmount,
                                BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2),
                                BooksCurrencyBaseRate = Math.Round(voucherVM.CompanyCurrencyRate * salesMaterialVM.TransactionRate, 4),
                                BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),

                                ModelState = ModelState.Added,
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                UpdatedBy = sales.UpdatedBy,
                                UpdatedDate = sales.UpdatedDate,
                                UpdatedFromIP = sales.UpdatedFromIP,
                                IsCanceled = false,
                            };
                            if (voucherVM.CurrencyId != companyCurrencyId)
                            {
                                salesMaterial.BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2);
                            }

                            _salesMaterialRepository.Insert(salesMaterial);

                            if (salesMaterialVM.TaxList != null && salesMaterialVM.TaxList.Count > 0)
                            {
                                foreach (var taxVM in salesMaterialVM.TaxList)
                                {
                                    if (taxVM.TaxCategoryId == null)
                                        throw new CustomException("Please Select Tax Category !");

                                    if (string.IsNullOrEmpty(taxVM.Id))
                                    {
                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {
                                            Id = _pkGeneratorService.MakePK(salesMaterial.Id, currentSalesTaxId, 2),
                                            AddedBy = salesMaterial.AddedBy,
                                            AddedDate = salesMaterial.AddedDate,
                                            AddedFromIP = salesMaterial.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = salesMaterial.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            SalesServiceId = null,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _salesTaxRepository.Insert(salesTax);
                                    }
                                    else
                                    {
                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {

                                            Id = taxVM.Id,
                                            Amount = taxVM.TotalAmount,
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = salesMaterial.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            SalesServiceId = null,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            AddedBy = salesMaterial.AddedBy,
                                            AddedDate = salesMaterial.AddedDate,
                                            AddedFromIP = salesMaterial.AddedFromIP,
                                            UpdatedBy = salesMaterial.UpdatedBy,
                                            UpdatedDate = salesMaterial.UpdatedDate,
                                            UpdatedFromIP = salesMaterial.UpdatedFromIP,
                                            ModelState = ModelState.Modified
                                        };
                                        _salesTaxRepository.Update(salesTax);
                                    }



                                }
                            }

                        }
                        else
                        {
                            var salesMaterial = new SalesMaterial
                            {
                                Id = salesMaterialVM.Id,
                                SalesId = sales.Id,
                                SalesOrderId = salesMaterialVM.SalesOrderId,
                                MaterialMasterId = salesMaterialVM.MaterialMasterId,
                                ArticleId = salesMaterialVM.ArticleId,
                                FirstCharacteristicsId = salesMaterialVM.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = salesMaterialVM.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = salesMaterialVM.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = salesMaterialVM.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = salesMaterialVM.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = salesMaterialVM.ThirdCharacteristicsValueId,
                                BaseUOMId = salesMaterialVM.BaseUOMId,
                                BaseRate = salesMaterialVM.BaseRate,
                                //BaseQty = salesMaterialVM.BaseQty,
                                BaseQty = salesMaterialVM.SalesQty,
                                BaseAmount = Math.Round(salesMaterialVM.SalesQty * salesMaterialVM.BaseRate, 2),

                                BaseUoMFactor = salesMaterialVM.BaseUoMFactor,
                                TransactionUoMId = salesMaterialVM.BaseUOMId,

                                TransactionRate = salesMaterialVM.TransactionRate,
                                TransactionQty = salesMaterialVM.SalesQty,
                                TransactionAmount = Math.Round(salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty, 2),

                                BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2),
                                BooksCurrencyBaseRate = Math.Round(voucherVM.CompanyCurrencyRate * salesMaterialVM.TransactionRate, 4),
                                BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                                TaxAmount = salesMaterialVM.TaxAmount,
                                NetAmount = salesMaterialVM.NetAmount,
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                UpdatedBy = sales.UpdatedBy,
                                UpdatedDate = sales.UpdatedDate,
                                UpdatedFromIP = sales.UpdatedFromIP,
                                ModelState = ModelState.Modified
                                ,
                                IsCanceled = false
                            };

                            if (voucherVM.CurrencyId != companyCurrencyId)
                            {
                                salesMaterial.BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2);
                            }
                            _salesMaterialRepository.Update(salesMaterial);


                            if (salesMaterialVM.TaxList != null && salesMaterialVM.TaxList.Count > 0)
                            {
                                foreach (var taxVM in salesMaterialVM.TaxList)
                                {
                                    if (taxVM.TaxCategoryId == null)
                                        throw new CustomException("Please Select Tax Category !");

                                    if (string.IsNullOrEmpty(taxVM.Id))
                                    {
                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {
                                            Id = _pkGeneratorService.MakePK(salesMaterial.Id, currentSalesTaxId, 2),
                                            AddedBy = salesMaterial.AddedBy,
                                            AddedDate = salesMaterial.AddedDate,
                                            AddedFromIP = salesMaterial.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = salesMaterial.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            SalesServiceId = null,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _salesTaxRepository.Insert(salesTax);
                                    }
                                    else
                                    {
                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {

                                            Id = taxVM.Id,
                                            Amount = taxVM.TotalAmount,
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = salesMaterial.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            SalesServiceId = null,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            AddedBy = salesMaterial.AddedBy,
                                            AddedDate = salesMaterial.AddedDate,
                                            AddedFromIP = salesMaterial.AddedFromIP,
                                            UpdatedBy = salesMaterial.UpdatedBy,
                                            UpdatedDate = salesMaterial.UpdatedDate,
                                            UpdatedFromIP = salesMaterial.UpdatedFromIP,
                                            ModelState = ModelState.Modified
                                        };
                                        _salesTaxRepository.Update(salesTax);
                                    }
                                }
                            }

                        }

                        var firstCharacteristics = _firstCharacteristicsRepository.Find(salesMaterialVM.FirstCharacteristicsId);

                        if (firstCharacteristics != null)
                        {
                            var secondCharacteristics = _secondCharacteristicsRepository.Find(salesMaterialVM.SecondCharacteristicsId);
                            var thirdCharacteristics = _thirdCharacteristicsRepository.Find(salesMaterialVM.ThirdCharacteristicsId);


                            firstCharacteristics.SalesQty = firstCharacteristics.SalesQty - salesMaterialVM.TempSalesQty + salesMaterialVM.SalesQty;
                            _firstCharacteristicsRepository.Update(firstCharacteristics);


                            if (secondCharacteristics != null)
                            {
                                secondCharacteristics.SalesQty = secondCharacteristics.SalesQty - salesMaterialVM.TempSalesQty + salesMaterialVM.SalesQty;
                                _secondCharacteristicsRepository.Update(secondCharacteristics);
                            }

                            if (thirdCharacteristics != null)
                            {
                                thirdCharacteristics.SalesQty = thirdCharacteristics.SalesQty - salesMaterialVM.TempSalesQty + salesMaterialVM.SalesQty;
                                _thirdCharacteristicsRepository.Update(thirdCharacteristics);
                            }
                        }


                    }
                }

                if (selectedMasterOrderList != null)
                {
                    currentSalesOrderItemId = _salesMaterialRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM TRN.SalesOrderItem WHERE SalesId='{sales.Id}'").First();
                    foreach (var item in selectedMasterOrderList)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            currentSalesOrderItemId++;
                            var salesMaterialSo = new SalesOrderItem
                            {
                                Id = _pkGeneratorService.MakePK(sales.Id, currentSalesOrderItemId, 2),

                                MasterOrderId = item.MasterOrderId,
                                SalesId = sales.Id,
                                MasterOrderItemId = item.MasterOrderItemId,
                                ModelState = ModelState.Added,
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null
                            };
                            _salesMaterialSORepository.Insert(salesMaterialSo);
                        }
                        else
                        {
                            var salesMaterialSo = new SalesOrderItem
                            {
                                Id = item.Id,
                                MasterOrderId = item.MasterOrderId,
                                SalesId = sales.Id,
                                MasterOrderItemId = item.MasterOrderItemId,
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                UpdatedBy = sales.UpdatedBy,
                                UpdatedDate = sales.UpdatedDate,
                                UpdatedFromIP = sales.UpdatedFromIP,
                                ModelState = ModelState.Modified
                            };
                            _salesMaterialSORepository.Update(salesMaterialSo);
                        }
                    }
                }

                if (salesServiceVMList != null)
                {
                    foreach (var salesServiceVM in salesServiceVMList)
                    {

                        currentSalesServiceId++;
                        if (string.IsNullOrEmpty(salesServiceVM.Id))
                        {
                            var salesService = new Model.SalesManagements.SalesService
                            {
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                Amount = salesServiceVM.Amount,
                                BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                Id = _pkGeneratorService.MakePK(sales.Id, currentSalesServiceId, 2),
                                ModelState = ModelState.Added,
                                NetAmount = salesServiceVM.NetAmount,
                                SalesId = sales.Id,
                                ServiceMasterId = salesServiceVM.ServiceMasterId,
                                TaxAmount = salesServiceVM.TaxAmount,
                                BooksCurrencyTaxAmount = Math.Round(salesServiceVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null
                            };
                            _salesServiceRepository.Insert(salesService);

                            if (salesServiceVM.ServiceTaxList != null && salesServiceVM.ServiceTaxList.Count > 0)
                            {
                                foreach (var taxVM in salesServiceVM.ServiceTaxList)
                                {
                                    if (taxVM.TaxCategoryId == null)
                                        throw new CustomException("Please Select Tax Category !");

                                    currentSalesTaxId++;
                                    var salesTax = new SalesTax
                                    {
                                        Id = _pkGeneratorService.MakePK(salesService.Id, currentSalesTaxId, 2),
                                        AddedBy = salesService.AddedBy,
                                        AddedDate = salesService.AddedDate,
                                        AddedFromIP = salesService.AddedFromIP,
                                        Amount = taxVM.Amount,
                                        BooksCurrencyTransactionAmount = Math.Round(taxVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                        HSNCodeId = taxVM.HSNCodeId,
                                        Percentage = taxVM.Percentage,
                                        SalesId = sales.Id,
                                        SalesMaterialId = null,
                                        SalesServiceId = salesService.Id,
                                        TaxCategoryId = taxVM.TaxCategoryId,
                                        ModelState = ModelState.Added,
                                        UpdatedBy = null,
                                        UpdatedDate = null,
                                        UpdatedFromIP = null
                                    };
                                    _salesTaxRepository.Insert(salesTax);
                                }
                            }
                        }
                        else
                        {
                            var salesService = new Model.SalesManagements.SalesService
                            {
                                Id = salesServiceVM.Id,
                                Amount = salesServiceVM.Amount,
                                BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                ModelState = ModelState.Modified,
                                NetAmount = salesServiceVM.NetAmount,
                                SalesId = sales.Id,
                                ServiceMasterId = salesServiceVM.ServiceMasterId,
                                TaxAmount = salesServiceVM.TaxAmount,
                                BooksCurrencyTaxAmount = Math.Round(salesServiceVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                                UpdatedBy = sales.UpdatedBy,
                                UpdatedDate = sales.UpdatedDate,
                                UpdatedFromIP = sales.UpdatedFromIP
                            };
                            _salesServiceRepository.Update(salesService);

                            if (salesServiceVM.ServiceTaxList != null && salesServiceVM.ServiceTaxList.Count > 0)
                            {
                                foreach (var taxVM in salesServiceVM.ServiceTaxList)
                                {

                                    if (string.IsNullOrEmpty(taxVM.Id))
                                    {
                                        if (taxVM.TaxCategoryId == null)
                                            throw new CustomException("Please Selete Tax Category !");

                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {
                                            Id = _pkGeneratorService.MakePK(salesServiceVM.Id, currentSalesTaxId, 2),
                                            AddedBy = sales.AddedBy,
                                            AddedDate = sales.AddedDate,
                                            AddedFromIP = sales.AddedFromIP,
                                            Amount = taxVM.TaxAmount,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = null,
                                            SalesServiceId = salesService.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _salesTaxRepository.Insert(salesTax);
                                    }
                                    else
                                    {
                                        var salesTax = new SalesTax
                                        {
                                            Id = taxVM.Id,
                                            Amount = taxVM.Amount,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = null,
                                            SalesServiceId = salesService.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            ModelState = ModelState.Modified,
                                            UpdatedBy = salesService.UpdatedBy,
                                            UpdatedDate = salesService.UpdatedDate,
                                            UpdatedFromIP = salesService.UpdatedFromIP
                                        };
                                        _salesTaxRepository.Update(salesTax);
                                    }
                                }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void InsertInvoiceDetail(Invoice invoice, InvoiceDetail invoiceDetail, int currentId)
        {
            invoiceDetail.Id = "IND" + MakePK(invoice.Id, currentId, 1);
            invoiceDetail.InvoiceId = invoice.Id;
            invoiceDetail.Archive = invoice.Archive;
            invoiceDetail.AddedBy = invoice.AddedBy;
            invoiceDetail.AddedDate = invoice.AddedDate;
            invoiceDetail.AddedFromIP = invoice.AddedFromIP;
            _invoiceDetailRepository.Insert(invoiceDetail);
        }
        public void MasterOrderSalesPost(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList
            , IEnumerable<SalesMaterialViewModel> salesMaterialDetailGLList, IEnumerable<SalesServiceViewModel> salesServiceDetailGLList)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                var taxYear = CheckingFiscalYearPeriod(voucherVM.CompanyGroupId, voucherVM.PostingDate);
                voucherVM.TaxYearId = taxYear["TaxYearId"].ToString();
                voucherVM.TaxYearPeriodId = taxYear["TaxYearPeriodId"].ToString();
                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                // Get Sales
                var sales = _salesRepository.Find(voucherVM.Id);
                sales.RowState = "Posted";
                //var companyParty = _companyPartyRepository.Query(r => r.CompanyId == sales.CompanyId && r.PlantId == sales.PlantId && r.PartyId == sales.PartyId && r.PartyType == sales.PartyType).Select().FirstOrDefault();
                //if (null == companyParty)
                //    throw new CustomException("Plant party mapping not found!");
                voucherVM.IsPark = false;

                var invoice = new Invoice
                {
                    Amount = salesMaterialVMList.Where(r => r.OtherName == "Customer" && r.TrnType == "Dr").Sum(r => r.Amount),
                    BaseNoOfDays = voucherVM.BaseNoOfDays,
                    BaseOnDueDate = voucherVM.BaseOnDueDate,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    EntityId = voucherVM.EntityId,
                    PlantId = voucherVM.PlantId,
                    IsExcludingTax = voucherVM.IsExcludingTax,
                    IsSplit = voucherVM.IsSplit,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = PartyType.Customer.ToString(),
                    EmployeeId = voucherVM.EmployeeId,
                    PaymentTermId = voucherVM.PaymentTermId,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    RevisedDueDate = voucherVM.MatureDate,
                    ActualDueDate = voucherVM.MatureDate,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate,
                    IsPark = voucherVM.IsPark
                };
                _invoiceService.InsertInvoice(invoice);


                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = invoice.CompanyGroupId,
                    CompanyId = invoice.CompanyId,
                    PlantId = invoice.PlantId,
                    CurrencyId = invoice.CurrencyId,
                    FiscalYearId = invoice.FiscalYearId,
                    FiscalYearPeriodId = invoice.FiscalYearPeriodId,
                    TaxYearId = invoice.TaxYearId,
                    TaxYearPeriodId = invoice.TaxYearPeriodId,
                    AddedBy = invoice.AddedBy,
                    AddedDate = invoice.AddedDate,
                    AddedFromIP = invoice.AddedFromIP,
                    PostedBy = invoice.AddedBy,
                    PostedDate = invoice.AddedDate,
                    VoucherDate = invoice.VoucherDate,
                    DocDate = invoice.DocDate,
                    DocRefNo = invoice.DocRefNo,
                    Archive = invoice.Archive,
                    IsPark = invoice.IsPark,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                invoice.VoucherId = voucher.Id;
                sales.VoucherId = voucher.Id;
                ////sales.BaseNoOfDays = voucherVM.BaseNoOfDays,
                ////    sales.BaseOnDueDate = voucherVM.BaseOnDueDate,
                ////    sales.RevisedDueDate = voucherVM.MatureDate,
                ////    sales.ActualDueDate = voucherVM.MatureDate,
                _salesRepository.Update(sales);

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                var currentTaxRecord = 0;
                var totalAmountCr = 0.0M;
                var totalTaxCr = 0.0M;
                decimal totalAmountDr = 0;
                //sales Insert into Invoice and voucher
                //var salesTaxVMList = GetSalesTaxGroupTaxCategory(voucherVM.Id).Select().ToList();
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                if (salesMaterialVMList != null)
                {
                    var invoiceDetail = new InvoiceDetail
                    {
                        AddedBy = invoice.AddedBy,
                        AddedDate = invoice.AddedDate,
                        AddedFromIP = invoice.AddedFromIP,
                        Archive = invoice.Archive
                    };
                    foreach (var voucherDetailVM in salesMaterialVMList)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                            throw new CustomException("Without Budget can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                            throw new CustomException("Without Activity can not post.");
                        if (voucherDetailVM.TrnType == "Dr")
                        {
                            if (voucherDetailVM.OtherName == "Customer")
                            {
                                invoiceDetail.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                                invoiceDetail.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                                invoiceDetail.ActivityId = voucherDetailVM.ActivityId;
                                invoiceDetail.Amount = voucherDetailVM.Amount;
                                invoiceDetail.NetAmount = voucherDetailVM.Amount;
                                invoiceDetail.InvoiceId = invoice.Id;
                                InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                            }

                            var voucherDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                DrAmount = voucherDetailVM.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = sales.Narration,

                                PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            if (voucherDetailVM.OtherName == "Customer")
                            {
                                voucherDr.InvoiceDetailId = invoiceDetail.Id;
                                voucherDr.PartyId = sales.PartyId;
                                voucherDr.PartyPlantId = sales.InvoicingPartyPlantId;
                                voucherDr.PartyType = voucherDetailVM.OtherName;
                            }
                            totalAmountDr += voucherDr.DrAmount;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);
                            voucherDetailVM.VoucherDetailId = voucherDr.Id;




                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = sales.ToCurrencyRate,
                                ToCurrencyConversion = 1 / sales.ToCurrencyRate,
                                DrAmount = voucherDr.DrAmount * sales.ToCurrencyRate
                            });

                            if (voucherDetailVM.OtherName == "TaxReceivable" || voucherDetailVM.OtherName == "SVTaxReceivable" || voucherDetailVM.OtherName == "TCSReceivable")
                            {
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherDr.Id,
                                    VoucherId = voucher.Id,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucher.TaxYearId,
                                    TaxYearPeriodId = voucher.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxCodeId = voucherDetailVM.TaxCodeId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.SalesInvoice.ToString(),
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP
                                };
                                _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    Id = invoiceTax.Id + 1,
                                    InvoiceTaxId = invoiceTax.Id,
                                    Amount = invoiceTax.TaxAmount,
                                    GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Dr",
                                    AddedBy = invoiceTax.AddedBy,
                                    AddedDate = invoiceTax.AddedDate,
                                    AddedFromIP = invoiceTax.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            }
                        }
                        if (voucherDetailVM.TrnType == "Cr")
                        {
                            var voucherCr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CrAmount = voucherDetailVM.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = sales.Narration,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            totalAmountCr += voucherCr.CrAmount;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                            voucherDetailVM.VoucherDetailId = voucherCr.Id;
                            //_salesMaterialRepository.Update(voucherDetailVM);

                            if (voucherDetailVM.OtherName == "Sales")
                            {
                                foreach (var item in salesMaterialDetailGLList.Where(r => r.GLGeneralInfoId == voucherCr.GLGeneralInfoId
                           && r.BudgetMasterId == voucherCr.BudgetMasterId && r.ActivityId == voucherCr.ActivityId))
                                {
                                    if (item.SalesMaterialId != null)
                                    {
                                        var salesMaterial = _salesMaterialRepository.Find(item.SalesMaterialId);
                                        salesMaterial.VoucherDetailId = voucherCr.Id;
                                        salesMaterial.ModelState = ModelState.Modified;
                                        salesMaterial.PostCrGLGeneralInfoId = voucherCr.GLGeneralInfoId;
                                        salesMaterial.PostCrBudgetMasterId = voucherCr.BudgetMasterId;
                                        salesMaterial.PostCrActivityId = voucherCr.ActivityId;
                                        salesMaterial.PostDrGLGeneralInfoId = invoiceDetail.GLGeneralInfoId;
                                        salesMaterial.PostDrBudgetMasterId = invoiceDetail.BudgetMasterId;
                                        salesMaterial.PostDrActivityId = invoiceDetail.ActivityId;
                                        AuditService.UpdatedLog(salesMaterial);
                                        _salesMaterialRepository.Update(salesMaterial);
                                    }

                                }
                            }

                            if (voucherDetailVM.OtherName == "Service")
                            {
                                foreach (var item in salesServiceDetailGLList.Where(r => r.GLGeneralInfoId == voucherCr.GLGeneralInfoId
                           && r.BudgetMasterId == voucherCr.BudgetMasterId && r.ActivityId == voucherCr.ActivityId))
                                {
                                    if (item.SalesServiceId != null)
                                    {
                                        var salesService = _salesServiceRepository.Find(item.SalesServiceId);
                                        salesService.VoucherDetailId = voucherCr.Id;
                                        salesService.ModelState = ModelState.Modified;
                                        AuditService.UpdatedLog(salesService);
                                        _salesServiceRepository.Update(salesService);
                                    }

                                }
                            }

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherCr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = sales.ToCurrencyRate,
                                ToCurrencyConversion = 1 / sales.ToCurrencyRate,
                                CrAmount = voucherCr.CrAmount * sales.ToCurrencyRate
                            });

                            if (voucherDetailVM.OtherName == "TaxPayable" || voucherDetailVM.OtherName == "SVTaxPayable" || voucherDetailVM.OtherName == "TCSPayable")
                            {
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherCr.Id,
                                    VoucherId = voucher.Id,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucher.TaxYearId,
                                    TaxYearPeriodId = voucher.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxCodeId = voucherDetailVM.TaxCodeId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.SalesInvoice.ToString(),
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP
                                };
                                _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    Id = invoiceTax.Id + 1,
                                    InvoiceTaxId = invoiceTax.Id,
                                    Amount = invoiceTax.TaxAmount,
                                    GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Cr",
                                    AddedBy = invoiceTax.AddedBy,
                                    AddedDate = invoiceTax.AddedDate,
                                    AddedFromIP = invoiceTax.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            }
                        }
                    }
                }

                // var salesServiceList = _salesServiceRepository.Query(r => r.SalesId == voucherVM.Id).Select().ToList();

                //if (salesServiceList != null)
                //{
                //    foreach (var voucherDetailVM in salesServiceList)
                //    {

                //        var serviceGroupId = _serviceMasterRepository.Query(r => r.Id == voucherDetailVM.ServiceMasterId).Select(r => r.ServiceGroupId).FirstOrDefault();
                //        if (null == serviceGroupId)
                //            throw new CustomException("Service GL mapping not found!");
                //        var serviceGroupGLList = _serviceGroupGLRepository.Query(r => r.ServiceGroupId == serviceGroupId).Select().FirstOrDefault();
                //        if (null == serviceGroupGLList)
                //            throw new CustomException("Service Master GL not found!");

                //        if (string.IsNullOrEmpty(serviceGroupGLList.ServiceGLId))
                //            throw new CustomException("Without GL can not post.");
                //        // in libility side Dr.
                //        var voucherCr = new VoucherDetail
                //        {
                //            GLGeneralInfoId = serviceGroupGLList.ServiceGLId,
                //            BudgetMasterId = serviceGroupGLList.ServiceBudgetMasterId,
                //            ActivityId = serviceGroupGLList.ServiceActivityId,
                //            CrAmount = voucherDetailVM.Amount,
                //            CurrencyId = voucherVM.CurrencyId,
                //            DocDate = voucherVM.DocDate,
                //            DocRefNo = voucherVM.DocRefNo,
                //            Narration = sales.Narration,
                //            PostingWithoutTaxAllow = invoice.IsExcludingTax,
                //            AddedBy = voucher.AddedBy,
                //            AddedDate = voucher.AddedDate,
                //            AddedFromIP = voucher.AddedFromIP
                //        };
                //        totalAmountCr += voucherCr.CrAmount;
                //        currentVoucherDetaiRecord++;
                //        _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                //        voucherDetailVM.VoucherDetailId = voucherCr.Id;
                //        _salesServiceRepository.Update(voucherDetailVM);
                //        _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                //        {
                //            ParallelCurrencyId = companyCurrencyId,
                //            FromCurrencyId = voucherCr.CurrencyId,
                //            ToCurrencyId = companyCurrencyId,
                //            ToCurrencyRate = sales.ToCurrencyRate,
                //            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherCr.CurrencyId, companyCurrencyId, sales.ToCurrencyRate),
                //            CrAmount = voucherCr.CrAmount * sales.ToCurrencyRate
                //        });
                //    }
                //}
                //if (salesTaxVMList != null)
                //{
                //    // var salesTax = _salesTaxRepository.Query(r => r.SalesMaterialId == voucherDetailVM.Id).Select().ToList();
                //    foreach (var stax in salesTaxVMList)
                //    {
                //        var taxcat = stax["TaxCategoryId"].ToString();
                //        var taxgl = _taxCategoryGLRepository.Query(r => r.TaxCategoryId == taxcat.ToString() && r.InputTaxOutPutTax == "Output").Select().FirstOrDefault();
                //        if (null == taxgl)
                //            throw new CustomException("Tax GL  not found!");
                //        currentTaxRecord++;
                //        var invoiceTax = new InvoiceTax
                //        {
                //            Archive = false,
                //            Id = MakePK(invoice.Id, currentTaxRecord, 2),
                //            VoucherId = voucher.Id,
                //            InvoiceId = invoice.Id,
                //            TaxYearId = voucher.TaxYearId,
                //            TaxYearPeriodId = voucher.TaxYearPeriodId,
                //            TaxCategoryId = stax["TaxCategoryId"].ToString(),
                //            TaxAmount = Convert.ToDecimal(stax["Amount"]),
                //            TaxAutoAmount = 0,
                //            PartyId = voucherVM.PartyId,
                //            PartyPlantId = voucherVM.PartyPlantId,
                //            SourceType = SourceType.SalesInvoice.ToString(),
                //            AddedBy = voucher.AddedBy,
                //            AddedDate = voucher.AddedDate,
                //            AddedFromIP = voucher.AddedFromIP
                //        };
                //        _invoiceTaxRepository.Insert(invoiceTax);
                //        var invoiceTaxDetail = new InvoiceTaxDetail
                //        {
                //            Id = invoiceTax.Id + 1,
                //            InvoiceTaxId = invoiceTax.Id,
                //            Amount = invoiceTax.TaxAmount,
                //            GLGeneralInfoId = taxgl.GLGeneralInfoId,
                //            BudgetMasterId = taxgl.BudgetMasterId,
                //            ActivityId = taxgl.ActivityId,
                //            AType = "Cr",
                //            AddedBy = invoiceTax.AddedBy,
                //            AddedDate = invoiceTax.AddedDate,
                //            AddedFromIP = invoiceTax.AddedFromIP
                //        };
                //        _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                //        totalTaxCr += invoiceTaxDetail.Amount;
                //        var voucherTaxCr = new VoucherDetail
                //        {
                //            GLGeneralInfoId = taxgl.GLGeneralInfoId,
                //            BudgetMasterId = taxgl.BudgetMasterId,
                //            ActivityId = taxgl.ActivityId,
                //            CrAmount = invoiceTaxDetail.Amount,
                //            CurrencyId = voucherVM.CurrencyId,
                //            DocDate = voucherVM.DocDate,
                //            DocRefNo = voucherVM.DocRefNo,
                //            Narration = sales.Narration,
                //            PostingWithoutTaxAllow = invoice.IsExcludingTax,
                //            AddedBy = voucher.AddedBy,
                //            AddedDate = voucher.AddedDate,
                //            AddedFromIP = voucher.AddedFromIP
                //        };
                //        totalAmountCr += voucherTaxCr.CrAmount;

                //        currentVoucherDetaiRecord++;
                //        _voucherService.InsertVoucherDetail(voucher, voucherTaxCr, currentVoucherDetaiRecord);
                //        invoiceTax.VoucherDetailId = voucherTaxCr.Id;

                //        _voucherService.InsertVoucherDetailCompanyCurrency(voucherTaxCr, new VoucherDetailCurrency
                //        {
                //            ParallelCurrencyId = companyCurrencyId,
                //            FromCurrencyId = voucherTaxCr.CurrencyId,
                //            ToCurrencyId = companyCurrencyId,
                //            ToCurrencyRate = sales.ToCurrencyRate,
                //            ToCurrencyConversion = _voucherService.GetCompanyCurrencyExchange(voucherTaxCr.CurrencyId, companyCurrencyId, sales.ToCurrencyRate),
                //            CrAmount = voucherTaxCr.CrAmount * sales.ToCurrencyRate
                //        });
                //        if (companyParty.TaxApplicable == "Mandatory")
                //        {
                //            var rcmgl = _taxCategoryGLRepository.Query(r => r.TaxCategoryId == taxcat.ToString() && r.InputTaxOutPutTax == "Input" && r.TaxType=="RCM").Select().FirstOrDefault();
                //            if(null == rcmgl)
                //            throw new CustomException("Tax Category Creditable GL  not found!");


                //                //var invoiceTaxDetailDdr = new InvoiceTaxDetail
                //                //{
                //                //    GLGeneralInfoId = rcmgl.GLGeneralInfoId,
                //                //    BudgetMasterId = rcmgl.BudgetMasterId,
                //                //    ActivityId = rcmgl.ActivityId,
                //                //    Amount = invoiceTax.TaxAmount,
                //                //    AType = "Dr"
                //                //};
                //                //totalcreditableDrAmount += invoiceTaxDetail.Amount;
                //                //_invoiceTaxService.InsertInvoiceTaxDetail(invoiceTax, invoiceTaxDetail, 2);

                //                //var voucherDetailTaxDr = new VoucherDetail
                //                //{
                //                //    GLGeneralInfoId = invoiceTaxDetail.GLGeneralInfoId,
                //                //    BudgetMasterId = invoiceTaxDetail.BudgetMasterId,
                //                //    ActivityId = invoiceTaxDetail.ActivityId,
                //                //    InvoiceTaxDetailId = invoiceTaxDetail.Id,
                //                //    DrAmount = invoiceTaxDetail.Amount,
                //                //    PostingWithoutTaxAllow = voucherDetailTaxDr.PostingWithoutTaxAllow
                //                //};
                //                //currentVoucherDetailId++;
                //                //_voucherService.InsertVoucherDetail(voucher, voucherDetailTaxDr, currentVoucherDetailId);
                //                //totalAmountDr += voucherDetailTax.DrAmount;
                //                //var voucherDetailCurrencybase = new VoucherDetailCurrency
                //                //{
                //                //    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                //                //    ToCurrencyId = companyCurrencyId,
                //                //    ParallelCurrencyId = companyCurrencyId,
                //                //    FromCurrencyId = voucherVM.CurrencyId,
                //                //    DrAmount = voucherVM.CompanyCurrencyRate * voucherDetailTaxDr.DrAmount,
                //                //    ToCurrencyConversion = 1 / voucherVM.CompanyCurrencyRate
                //                //};
                //                //totalBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                //                //totalAPBaseCurrencyDrAmount += voucherDetailCurrencybase.DrAmount;
                //                //_voucherService.InsertVoucherDetailCompanyCurrency(voucherDetailTaxDr, voucherDetailCurrencybase);
                //        }
                //        taxcat = null;
                //    }
                //}


                // Update Inventory Received

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
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

        public void DeleteMasterOrderSalePost(string companyId, string plantId, string salesId, string voucherId, string deletedRemarks)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;

                var voucher = _voucherService.FindVoucher(voucherId);
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.InsertVoucherLogDeleted(voucherId, voucher.VoucherNo, "", "", "", "", "", "", "", "", "", "", salesId, deletedRemarks);

                var writtenOff = _invoiceService.Query(r => r.VoucherId == voucherId && r.WrittenOffAmount > 0).Select().ToList();

                if (writtenOff.Count() > 0)
                    throw new CustomException("Delete is not allow,Please delete Customer Payment Receipt First ! ");

                var vendorAdWr = new System.Text.StringBuilder();
                var vendorAdWrsql = "";
                vendorAdWrsql = @"update trn.SalesMaterial set VoucherDetailId=null where SalesId='" + salesId + "' ";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete trn.VoucherDetailCurrency where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalesInvoice.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"delete from TRN.InvoiceTaxDetail where InvoiceTaxId in(select Id from trn.InvoiceTax where InvoiceId in(select Id from TRN.Invoice where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalesInvoice.ToString() + "' AND Id = '" + voucherId + "')))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.InvoiceTax  where InvoiceId in(select Id from TRN.Invoice where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalesInvoice.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.VoucherDetail where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalesInvoice.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.InvoiceDetailCharges where invoiceid in (select Id from TRN.Invoice  where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalesInvoice.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete from TRN.InvoiceDetail  where InvoiceId in(select Id from TRN.Invoice where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalesInvoice.ToString() + "' AND Id = '" + voucherId + "'))";
                vendorAdWr.Append(vendorAdWrsql);

                

                vendorAdWrsql = @"delete from TRN.Invoice  where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalesInvoice.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);

                vendorAdWrsql = @"update trn.Sales set VoucherId=null where VoucherId in (select Id from trn.voucher where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalesInvoice.ToString() + "' AND Id = '" + voucherId + "')";
                vendorAdWr.Append(vendorAdWrsql);
                vendorAdWrsql = @"delete trn.voucher  where CompanyId='" + companyId + "' AND PlantId='" + plantId + "' AND SourceType='" + SourceType.SalesInvoice.ToString() + "' AND Id = '" + voucherId + "'";
                vendorAdWr.Append(vendorAdWrsql);
                _sqlRepository.ExecuteSqlCommand(vendorAdWr.ToString());
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();

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
        public object GetMasterOrderIdBySalesId(string salesId)
        {
            try
            {
                string sql = @"SELECT distinct A.MasterOrderItemId,A.MasterOrderId, SO.ContractId,C.ContractNo,MLC.LCRef, MLC.BenificiaryBankId
								FROM [TRN].[SalesOrderItem]  A
                                LEFT JOIN TRN.MasterOrder B ON B.Id=A.MasterOrderId
								LEFT JOIN TRN.MasterOrderItem MOI ON MOI.MasterOrderId=A.MasterOrderId
								LEFT JOIN [TRN].[SalesOrder] SO ON SO.MasterOrderItemId=MOI.Id
								LEFT JOIN dbo.[Contract] C ON C.Id=SO.ContractId
								LEFT JOIN dbo.[MasterLC] MLC ON MLC.Id=C.MasterLCId
                                WHERE SalesId='" + salesId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMasterOrderDataByMasterOrderId(string companyId, string masterOrderId, string masterOrderItemId, string salesId)
        {
            try
            {
                var sql = @"SELECT SOI.Id,SOI.SalesId,A.Id AS  MasterOrderId, MOI.Id MasterOrderItemId, A.CompanyId, A.CommitmentId, A.PlantId, A.EntityId
                                    , A.OrderType, A.PartyId, P.UserName AS CustomerName, A.BuyerId	
                                    , A.BuyerBrandId, A.BuyerDivisionId, A.TestingStandardId, A.MasterOrderNo, A.OrderStatusId	
                                    , A.OrderCategoryId, A.SeasonId, A.OrderYear, A.CurrencyId, A.TotalQty	
                                    , A.NoOfLineItem, A.ResponsiblePersonId, EI.EmployeeName AS ResponsiblePersonName
                                    , A.InvoicingPartyPlantId, InvPP.UserName AS InvoicingPartyPlant, A.InvoicingByAddress
		                            , A.DeliveryPartyPlantId, DeliPP.UserName AS DeliveryPartyPlant, A.DeliveryByAddress
		                            , PartyAccountGroupId=(SELECT DISTINCT PartyAccountGroupId FROM [HKP].[CompanyParty] WHERE CompanyId=A.CompanyId
								                            AND PartyId=A.PartyId AND PartyType='Customer' )
								    ,A.OrderWastagePercentage
								    ,A.ExtraOrderPercentage
								    ,A.TotalQtyUOMId,PL.UserName,A.IsReplacement,A.Type,C.Code Currency,A.SpecialTaxId,A.IsExtraOrderPercentage, 0 Active
                                    ,MM.UserName MaterialMaster, MMA.StandardName Article,MOI.TotalQty ItemQty
                            FROM [TRN].[MasterOrder] AS A
                            JOIN [TRN].[MasterOrderItem] AS MOI ON MOI.MasterOrderId=A.Id
                            JOIN [TRN].[SalesOrderItem] AS SOI ON SOI.MasterOrderId=A.Id AND SOI.MasterOrderItemId=MOI.Id
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [HKP].[PartyPlant] AS InvPP ON A.InvoicingPartyPlantId=InvPP.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DeliPP ON A.DeliveryPartyPlantId=DeliPP.Id
                            LEFT JOIN EmployeeInformation AS EI ON A.ResponsiblePersonId=EI.SystemId
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN MST.MaterialMaster MM ON MM.Id=MOI.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=MOI.ArticleId
                            WHERE A.CompanyId='" + companyId + "' AND A.Id " + masterOrderId + " AND MOI.Id " + masterOrderItemId + "  AND SOI.SalesId='" + salesId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        #endregion

        #region Packing Integration

        public void PackingInvoiceInsert(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesPacking> selectedPackingList, IEnumerable<SalesServiceViewModel> salesServiceVMList, DataSet dsDetail, DataSet dsHistory, DataSet dsItemScanData)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _unitOfWork.BeginTransaction();
                flag = true;

                var sales = new Sales
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    //DocDate = voucherVM.DocDate,

                    CurrencyId = voucherVM.CurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    BaseNoOfDays = voucherVM.BaseNoOfDays,
                    BaseOnDueDate = voucherVM.BaseOnDueDate,
                    DeliveryPartyPlantId = voucherVM.DeliveryPartyPlantId,
                    EntryDate = voucherVM.VoucherDate,
                    InvoiceDate = voucherVM.InvoiceDate,
                    InvoicingPartyPlantId = voucherVM.InvoicingPartyPlantId,
                    MatureDate = voucherVM.MatureDate,
                    PartyId = voucherVM.PartyId,
                    PartyType = voucherVM.PartyType,
                    Narration = voucherVM.Narration,
                    PaymentTermId = voucherVM.PaymentTermId,
                    RowState = RowState.Parked.ToString(),
                    DeliveryByAddress = voucherVM.DeliveryByAddress,
                    InvoicingByAddress = voucherVM.InvoicingByAddress,
                    ModelState = ModelState.Added,
                    BLNumber = voucherVM.BLNumber,
                    ItemDescription = voucherVM.ItemDescription,
                    BLDate = voucherVM.BLDate,
                    EXPDate = voucherVM.EXPDate,
                    EXPFromNo = voucherVM.EXPFromNo,
                    InvoiceStatus = voucherVM.InvoiceStatus,
                    ComercialInvoiceNo = voucherVM.ComercialInvoiceNo,
                    IsAdditionalInfoApplicable = voucherVM.IsAdditionalInfoApplicable,
                    PaymentToReceiveBankId = voucherVM.PaymentToReceiveBankId,
                    AdditionalFrieght = voucherVM.AdditionalFrieght,
                    AdditionalFrieghtValue = voucherVM.AdditionalFrieghtValue,
                    Incoterms = voucherVM.Incoterms,
                    IncotermsValue = voucherVM.IncotermsValue,
                    TrancastionTypeId = voucherVM.TrancastionTypeId,
                    SourceType = "Packing",
                    Id = "MS" + _pkGeneratorService.GetAutoNumber(nameof(Sales), PKGeneratorEnum.Yearly, null, DateTime.Now),
                };


                sales.DocRefNo = sales.Id;
                sales.InvoiceNo = sales.Id;
                voucherVM.Id = sales.Id;
                AuditService.AddedLog(sales);
                _salesRepository.Insert(sales);
                var currentSalesMaterialId = 0;
                var currentSalesOrderItemId = 0;
                var currentSalesServiceId = 0;
                var currentSalesTaxId = 0;
                int count = 0;
                if (dsDetail.Tables[0].Rows.Count > 0)
                {
                    var InventorySales = new Library.Model.Inventory.InventorySales
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        CurrencyId = voucherVM.CurrencyId,
                        SalesDate = (DateTime)voucherVM.InvoiceDate,
                        DocDate = (DateTime)voucherVM.InvoiceDate,


                        Id = _pkGeneratorService.GetAutoNumber(nameof(Library.Model.Inventory.InventorySales), PKGeneratorEnum.Yearly, null, DateTime.Now),
                    };
                    AuditService.AddedLog(InventorySales);
                    _SalesRepository.Insert(InventorySales);

                    for (int i = 0; i < dsDetail.Tables[0].Rows.Count; i++)
                    {
                        currentSalesMaterialId++;
                        var SalesDetail = new Library.Model.Inventory.InventorySalesDetail
                        {
                            Id = _pkGeneratorService.MakePK(InventorySales.Id, currentSalesMaterialId, 2),
                            InventorySalesId = InventorySales.Id,
                            InventoryMaterialId = dsDetail.Tables[0].Rows[i]["InventoryMaterialId"].ToString(),
                            TransactionQty = Convert.ToDecimal(dsDetail.Tables[0].Rows[i]["TransactionQty"].ToString()),
                            BaseQty = Convert.ToDecimal(dsDetail.Tables[0].Rows[i]["TransactionQty"].ToString()),
                            PolicyRate = Convert.ToDecimal(dsDetail.Tables[0].Rows[i]["PolicyRate"]),
                            PolicyAmount = Convert.ToDecimal(dsDetail.Tables[0].Rows[i]["PolicyAmount"].ToString()),
                            BaseUOMId = dsDetail.Tables[0].Rows[i]["BaseUOMId"].ToString(),
                            TransactionUoMId = dsDetail.Tables[0].Rows[i]["TransactionUoMId"].ToString(),
                            Policy = "FIFO",
                            AddedBy = sales.AddedBy,
                            AddedDate = sales.AddedDate,
                            AddedFromIP = sales.AddedFromIP,
                            UpdatedBy = null,
                            UpdatedDate = null,
                            UpdatedFromIP = null
                        };
                        _SalesDetailService.Insert(SalesDetail);
                        for (int j = 0; j < dsHistory.Tables[0].Rows.Count; j++)
                        {
                            if (dsHistory.Tables[0].Rows[j]["PackingId"].ToString() == dsDetail.Tables[0].Rows[i]["PackingId"].ToString())
                            {
                                count++;
                                var InventorySalesHistory = new Library.Model.Inventory.InventorySalesHistory
                                {
                                    Id = _pkGeneratorService.MakePK(SalesDetail.Id, count, 2),
                                    InventorySalesDetailId = SalesDetail.Id,
                                    InventoryReceiveDetailId = dsHistory.Tables[0].Rows[j]["InventoryReceiveDetailId"].ToString(),
                                    Qty = Convert.ToDecimal(dsHistory.Tables[0].Rows[j]["Qty"].ToString()),
                                    TotalBaseAmount = Convert.ToDecimal(dsHistory.Tables[0].Rows[j]["TotalAmount"].ToString()),
                                    BaseRate = Convert.ToDecimal(dsHistory.Tables[0].Rows[j]["BooksCurrencyBaseRate"].ToString()),
                                    BooksCurrencyBaseAmount = Convert.ToDecimal(dsHistory.Tables[0].Rows[j]["TotalMaterialBooksCurrencyAmount"].ToString()),

                                    AddedBy = sales.AddedBy,
                                    AddedDate = sales.AddedDate,
                                    AddedFromIP = sales.AddedFromIP,
                                    UpdatedBy = null,
                                    UpdatedDate = null,
                                    UpdatedFromIP = null
                                };
                                _SalesHistoryService.Insert(InventorySalesHistory);
                            }
                        }

                    }
                }


                if (salesMaterialVMList != null)
                {
                    foreach (var salesMaterialVM in salesMaterialVMList)
                    {
                        currentSalesMaterialId++;
                        var salesMaterial = new SalesMaterial
                        {
                            Id = _pkGeneratorService.MakePK(sales.Id, currentSalesMaterialId, 3),
                            SalesId = sales.Id,
                            MaterialMasterId = salesMaterialVM.MaterialMasterId,
                            SalesOrderId = salesMaterialVM.SalesOrderId,
                            ArticleId = salesMaterialVM.ArticleId,
                            FirstCharacteristicsId = salesMaterialVM.FirstCharacteristicsId,
                            FirstCharacteristicsValueId = salesMaterialVM.FirstCharacteristicsValueId,
                            SecondCharacteristicsId = salesMaterialVM.SecondCharacteristicsId,
                            SecondCharacteristicsValueId = salesMaterialVM.SecondCharacteristicsValueId,
                            ThirdCharacteristicsId = salesMaterialVM.ThirdCharacteristicsId,
                            ThirdCharacteristicsValueId = salesMaterialVM.ThirdCharacteristicsValueId,
                            BaseUOMId = salesMaterialVM.BaseUOMId,

                            BaseRate = salesMaterialVM.BaseRate,
                            //BaseQty = salesMaterialVM.BaseQty,
                            BaseQty = salesMaterialVM.SalesQty,
                            BaseAmount = Math.Round(salesMaterialVM.SalesQty * salesMaterialVM.BaseRate, 2),

                            BaseUoMFactor = salesMaterialVM.BaseUoMFactor,
                            TransactionUoMId = salesMaterialVM.BaseUOMId,

                            TransactionRate = salesMaterialVM.TransactionRate,
                            TransactionQty = salesMaterialVM.SalesQty,
                            TransactionAmount = Math.Round(salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty, 2),

                            BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2),

                            BooksCurrencyBaseRate = Math.Round(voucherVM.CompanyCurrencyRate * salesMaterialVM.TransactionRate, 4),

                            TaxAmount = salesMaterialVM.TaxAmount,
                            BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                            NetAmount = salesMaterialVM.NetAmount,
                            GoodsDescription = salesMaterialVM.GoodsDescription,
                            ModelState = ModelState.Added,
                            AddedBy = sales.AddedBy,
                            AddedDate = sales.AddedDate,
                            AddedFromIP = sales.AddedFromIP,
                            UpdatedBy = null,
                            UpdatedDate = null,
                            UpdatedFromIP = null,
                            IsCanceled = false,
                            Remark = null,
                            CanceledBy = null
                        };

                        if (voucherVM.CurrencyId != companyCurrencyId)
                        {
                            salesMaterial.BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2);
                        }
                        _salesMaterialRepository.Insert(salesMaterial);

                        var firstCharacteristics = _firstCharacteristicsRepository.Find(salesMaterialVM.FirstCharacteristicsId);

                        if (firstCharacteristics != null)
                        {
                            var secondCharacteristics = _secondCharacteristicsRepository.Find(salesMaterialVM.SecondCharacteristicsId);
                            var thirdCharacteristics = _thirdCharacteristicsRepository.Find(salesMaterialVM.ThirdCharacteristicsId);

                            firstCharacteristics.SalesQty += salesMaterialVM.SalesQty;
                            _firstCharacteristicsRepository.Update(firstCharacteristics);


                            if (secondCharacteristics != null)
                            {
                                secondCharacteristics.SalesQty += salesMaterialVM.SalesQty;
                                _secondCharacteristicsRepository.Update(secondCharacteristics);
                            }

                            if (thirdCharacteristics != null)
                            {
                                thirdCharacteristics.SalesQty += salesMaterialVM.SalesQty;
                                _thirdCharacteristicsRepository.Update(thirdCharacteristics);
                            }
                        }

                        if (salesMaterialVM.TaxList != null && salesMaterialVM.TaxList.Count > 0)
                        {
                            foreach (var taxVM in salesMaterialVM.TaxList)
                            {
                                if (taxVM.TaxCategoryId == null)
                                    throw new CustomException("Please Select Tax Category !");

                                currentSalesTaxId++;
                                var salesTax = new SalesTax
                                {
                                    Id = _pkGeneratorService.MakePK(salesMaterial.Id, currentSalesTaxId, 2),
                                    AddedBy = salesMaterial.AddedBy,
                                    AddedDate = salesMaterial.AddedDate,
                                    AddedFromIP = salesMaterial.AddedFromIP,
                                    Amount = taxVM.TotalAmount,
                                    BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                    HSNCodeId = taxVM.HSNCodeId,
                                    Percentage = taxVM.Percentage,
                                    SalesId = sales.Id,
                                    SalesMaterialId = salesMaterial.Id,
                                    TaxCategoryId = taxVM.TaxCategoryId,
                                    SalesServiceId = null,
                                    ModelState = ModelState.Added,
                                    UpdatedBy = null,
                                    UpdatedDate = null,
                                    UpdatedFromIP = null
                                };
                                _salesTaxRepository.Insert(salesTax);
                            }
                        }

                        if (dsItemScanData.Tables[0].Rows.Count > 0)
                        {
                            dsItemScanData.Tables[0].DefaultView.RowFilter = "SOId='" + salesMaterialVM.SalesOrderId + "' AND PackingId='" + salesMaterialVM.PackingId + "'";

                            for (int i = 0; i < dsItemScanData.Tables[0].DefaultView.Count; i++)
                            {
                                var childData = _ItemScanChildDataService.Find(dsItemScanData.Tables[0].DefaultView[i]["Id"].ToString());
                                childData.SalesMaterialId = salesMaterial.Id;
                                childData.SalesId = sales.Id;
                                childData.IsDespatch = true;
                                childData.ReturnNetWeight = 0;
                                childData.UpdatedBy = sales.AddedBy;
                                childData.UpdatedDate = sales.AddedDate;
                                childData.ModelState = ModelState.Modified;
                                _ItemScanChildDataService.Update(childData);
                            }

                        }

                    }
                }

                if (selectedPackingList != null)
                {
                    foreach (var item in selectedPackingList)
                    {
                        currentSalesOrderItemId++;
                        var salesMaterialSo = new SalesPacking
                        {
                            Id = _pkGeneratorService.MakePK(sales.Id, currentSalesOrderItemId, 2),

                            PackingId = item.PackingId,
                            Qty = item.Qty,
                            Amount = item.Amount,
                            ProductLibraryId = item.ProductLibraryId,
                            SalesId = sales.Id,
                            ModelState = ModelState.Added,
                            AddedBy = sales.AddedBy,
                            AddedDate = sales.AddedDate,
                            AddedFromIP = sales.AddedFromIP,
                            UpdatedBy = null,
                            UpdatedDate = null,
                            UpdatedFromIP = null
                        };
                        _salesPackingRepository.Insert(salesMaterialSo);
                    }
                }

                if (salesServiceVMList != null)
                {
                    foreach (var salesServiceVM in salesServiceVMList)
                    {

                        currentSalesServiceId++;
                        var salesService = new Model.SalesManagements.SalesService
                        {
                            AddedBy = sales.AddedBy,
                            AddedDate = sales.AddedDate,
                            AddedFromIP = sales.AddedFromIP,
                            Amount = salesServiceVM.Amount,
                            BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                            Id = _pkGeneratorService.MakePK(sales.Id, currentSalesServiceId, 2),
                            ModelState = ModelState.Added,
                            NetAmount = salesServiceVM.NetAmount,
                            SalesId = sales.Id,
                            ServiceMasterId = salesServiceVM.ServiceMasterId,
                            TaxAmount = salesServiceVM.TaxAmount,
                            UpdatedBy = null,
                            UpdatedDate = null,
                            UpdatedFromIP = null
                        };
                        _salesServiceRepository.Insert(salesService);

                        if (salesServiceVM.ServiceTaxList != null && salesServiceVM.ServiceTaxList.Count > 0)
                        {
                            foreach (var taxVM in salesServiceVM.ServiceTaxList)
                            {
                                if (taxVM.TaxCategoryId == null)
                                    throw new CustomException("Please Select Tax Category !");

                                currentSalesTaxId++;
                                var salesTax = new SalesTax
                                {
                                    Id = _pkGeneratorService.MakePK(salesService.Id, currentSalesTaxId, 2),
                                    AddedBy = salesService.AddedBy,
                                    AddedDate = salesService.AddedDate,
                                    AddedFromIP = salesService.AddedFromIP,
                                    Amount = taxVM.Amount,
                                    BooksCurrencyTransactionAmount = Math.Round(taxVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                    HSNCodeId = taxVM.HSNCodeId,
                                    Percentage = taxVM.Percentage,
                                    SalesId = sales.Id,
                                    SalesMaterialId = null,
                                    SalesServiceId = salesService.Id,
                                    TaxCategoryId = taxVM.TaxCategoryId,
                                    ModelState = ModelState.Added,
                                    UpdatedBy = null,
                                    UpdatedDate = null,
                                    UpdatedFromIP = null
                                };
                                _salesTaxRepository.Insert(salesTax);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void PackingInvoiceUpdate(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList, IEnumerable<SalesPacking> selectedPackingList, IEnumerable<SalesServiceViewModel> salesServiceVMList, DataSet dsItemScanData)
        {
            var flag = false;
            try
            {
                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);

                _unitOfWork.BeginTransaction();
                flag = true;

                var sales = new Sales
                {
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    PlantId = voucherVM.PlantId,
                    EntityId = voucherVM.EntityId,
                    //DocDate = voucherVM.DocDate,

                    CurrencyId = voucherVM.CurrencyId,
                    ToCurrencyRate = voucherVM.CompanyCurrencyRate,
                    BaseNoOfDays = voucherVM.BaseNoOfDays,
                    BaseOnDueDate = voucherVM.BaseOnDueDate,
                    DeliveryPartyPlantId = voucherVM.DeliveryPartyPlantId,
                    EntryDate = voucherVM.VoucherDate,
                    InvoiceDate = voucherVM.InvoiceDate,
                    InvoicingPartyPlantId = voucherVM.InvoicingPartyPlantId,
                    MatureDate = voucherVM.MatureDate,
                    PartyId = voucherVM.PartyId,
                    PartyType = voucherVM.PartyType,
                    Narration = voucherVM.Narration,
                    PaymentTermId = voucherVM.PaymentTermId,
                    RowState = RowState.Parked.ToString(),
                    DeliveryByAddress = voucherVM.DeliveryByAddress,
                    InvoicingByAddress = voucherVM.InvoicingByAddress,
                    ComercialInvoiceNo = voucherVM.ComercialInvoiceNo,
                    BLNumber = voucherVM.BLNumber,
                    ItemDescription = voucherVM.ItemDescription,
                    BLDate = voucherVM.BLDate,
                    InvoiceStatus = voucherVM.InvoiceStatus,
                    EXPDate = voucherVM.EXPDate,
                    EXPFromNo = voucherVM.EXPFromNo,
                    PaymentToReceiveBankId = voucherVM.PaymentToReceiveBankId,
                    AdditionalFrieght = voucherVM.AdditionalFrieght,
                    AdditionalFrieghtValue = voucherVM.AdditionalFrieghtValue,
                    IsAdditionalInfoApplicable = voucherVM.IsAdditionalInfoApplicable,
                    Incoterms = voucherVM.Incoterms,
                    IncotermsValue = voucherVM.IncotermsValue,
                    AddedBy = voucherVM.AddedBy,
                    AddedDate = voucherVM.AddedDate,
                    AddedFromIP = voucherVM.AddedFromIP,
                    UpdatedBy = voucherVM.UpdatedBy,
                    UpdatedDate = voucherVM.UpdatedDate,
                    UpdatedFromIP = voucherVM.UpdatedFromIP,
                    SourceType = "Packing",
                    TrancastionTypeId = voucherVM.TrancastionTypeId,
                    ModelState = ModelState.Modified,
                    Id = voucherVM.Id
                };
                sales.DocRefNo = sales.Id;
                sales.InvoiceNo = sales.Id;
                AuditService.UpdatedLog(sales);
                _salesRepository.Update(sales);

                var currentSalesMaterialId = 0;
                var currentSalesServiceId = 0;
                var currentSalesOrderItemId = 0;
                var currentSalesTaxId = 0;
                if (salesMaterialVMList != null)
                {
                    var historyId = _salesMaterialRepository.SqlQuery<int>($"SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM TRN.SalesMaterial WHERE SalesId='{sales.Id}'").First();
                    foreach (var salesMaterialVM in salesMaterialVMList)
                    {
                        currentSalesMaterialId++;
                        if (string.IsNullOrEmpty(salesMaterialVM.Id))
                        {
                            historyId++;
                            var salesMaterial = new SalesMaterial
                            {
                                Id = _pkGeneratorService.MakePK(sales.Id, historyId, 3),
                                SalesId = sales.Id,
                                SalesOrderId = salesMaterialVM.SalesOrderId,
                                MaterialMasterId = salesMaterialVM.MaterialMasterId,
                                ArticleId = salesMaterialVM.ArticleId,
                                FirstCharacteristicsId = salesMaterialVM.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = salesMaterialVM.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = salesMaterialVM.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = salesMaterialVM.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = salesMaterialVM.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = salesMaterialVM.ThirdCharacteristicsValueId,
                                BaseUOMId = salesMaterialVM.BaseUOMId,
                                BaseRate = salesMaterialVM.BaseRate,
                                //BaseQty = salesMaterialVM.BaseQty,
                                BaseQty = salesMaterialVM.SalesQty,
                                BaseAmount = Math.Round(salesMaterialVM.SalesQty * salesMaterialVM.BaseRate, 2),

                                BaseUoMFactor = salesMaterialVM.BaseUoMFactor,
                                TransactionUoMId = salesMaterialVM.BaseUOMId,

                                TransactionRate = salesMaterialVM.TransactionRate,
                                TransactionQty = salesMaterialVM.SalesQty,
                                TransactionAmount = Math.Round(salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty, 2),
                                TaxAmount = salesMaterialVM.TaxAmount,
                                NetAmount = salesMaterialVM.NetAmount,
                                BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2),
                                BooksCurrencyBaseRate = Math.Round(voucherVM.CompanyCurrencyRate * salesMaterialVM.TransactionRate, 4),
                                BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),

                                GoodsDescription = salesMaterialVM.GoodsDescription,
                                ModelState = ModelState.Added,
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                UpdatedBy = sales.UpdatedBy,
                                UpdatedDate = sales.UpdatedDate,
                                UpdatedFromIP = sales.UpdatedFromIP
                            };
                            if (voucherVM.CurrencyId != companyCurrencyId)
                            {
                                salesMaterial.BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2);
                            }

                            _salesMaterialRepository.Insert(salesMaterial);

                            if (salesMaterialVM.TaxList != null && salesMaterialVM.TaxList.Count > 0)
                            {
                                foreach (var taxVM in salesMaterialVM.TaxList)
                                {
                                    if (taxVM.TaxCategoryId == null)
                                        throw new CustomException("Please Select Tax Category !");

                                    currentSalesTaxId++;
                                    var salesTax = new SalesTax
                                    {
                                        Id = _pkGeneratorService.MakePK(salesMaterial.Id, currentSalesTaxId, 2),
                                        AddedBy = salesMaterial.AddedBy,
                                        AddedDate = salesMaterial.AddedDate,
                                        AddedFromIP = salesMaterial.AddedFromIP,
                                        Amount = taxVM.TotalAmount,
                                        BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                        HSNCodeId = taxVM.HSNCodeId,
                                        Percentage = taxVM.Percentage,
                                        SalesId = sales.Id,
                                        SalesMaterialId = salesMaterial.Id,
                                        TaxCategoryId = taxVM.TaxCategoryId,
                                        SalesServiceId = null,
                                        ModelState = ModelState.Added,
                                        UpdatedBy = null,
                                        UpdatedDate = null,
                                        UpdatedFromIP = null
                                    };
                                    _salesTaxRepository.Insert(salesTax);
                                }
                            }

                        }
                        else
                        {
                            var salesMaterial = new SalesMaterial
                            {
                                Id = salesMaterialVM.Id,
                                SalesId = sales.Id,
                                SalesOrderId = salesMaterialVM.SalesOrderId,
                                MaterialMasterId = salesMaterialVM.MaterialMasterId,
                                ArticleId = salesMaterialVM.ArticleId,
                                FirstCharacteristicsId = salesMaterialVM.FirstCharacteristicsId,
                                FirstCharacteristicsValueId = salesMaterialVM.FirstCharacteristicsValueId,
                                SecondCharacteristicsId = salesMaterialVM.SecondCharacteristicsId,
                                SecondCharacteristicsValueId = salesMaterialVM.SecondCharacteristicsValueId,
                                ThirdCharacteristicsId = salesMaterialVM.ThirdCharacteristicsId,
                                ThirdCharacteristicsValueId = salesMaterialVM.ThirdCharacteristicsValueId,
                                BaseUOMId = salesMaterialVM.BaseUOMId,
                                BaseRate = salesMaterialVM.BaseRate,
                                //BaseQty = salesMaterialVM.BaseQty,
                                BaseQty = salesMaterialVM.SalesQty,
                                BaseAmount = Math.Round(salesMaterialVM.SalesQty * salesMaterialVM.BaseRate, 2),

                                BaseUoMFactor = salesMaterialVM.BaseUoMFactor,
                                TransactionUoMId = salesMaterialVM.BaseUOMId,

                                TransactionRate = salesMaterialVM.TransactionRate,
                                TransactionQty = salesMaterialVM.SalesQty,
                                TransactionAmount = Math.Round(salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty, 2),

                                BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2),
                                BooksCurrencyBaseRate = Math.Round(voucherVM.CompanyCurrencyRate * salesMaterialVM.TransactionRate, 4),
                                BooksCurrencyTaxAmount = Math.Round(salesMaterialVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                                TaxAmount = salesMaterialVM.TaxAmount,
                                NetAmount = salesMaterialVM.NetAmount,
                                GoodsDescription = salesMaterialVM.GoodsDescription,
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                UpdatedBy = sales.UpdatedBy,
                                UpdatedDate = sales.UpdatedDate,
                                UpdatedFromIP = sales.UpdatedFromIP,
                                ModelState = ModelState.Modified
                            };

                            if (voucherVM.CurrencyId != companyCurrencyId)
                            {
                                salesMaterial.BooksCurrencyTransactionAmount = Math.Round((salesMaterialVM.TransactionRate * salesMaterialVM.SalesQty) * voucherVM.CompanyCurrencyRate, 2);
                            }
                            _salesMaterialRepository.Update(salesMaterial);


                            if (salesMaterialVM.TaxList != null && salesMaterialVM.TaxList.Count > 0)
                            {
                                foreach (var taxVM in salesMaterialVM.TaxList)
                                {
                                    if (taxVM.TaxCategoryId == null)
                                        throw new CustomException("Please Select Tax Category !");

                                    if (string.IsNullOrEmpty(taxVM.Id))
                                    {
                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {
                                            Id = _pkGeneratorService.MakePK(salesMaterial.Id, currentSalesTaxId, 2),
                                            AddedBy = salesMaterial.AddedBy,
                                            AddedDate = salesMaterial.AddedDate,
                                            AddedFromIP = salesMaterial.AddedFromIP,
                                            Amount = taxVM.TotalAmount,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = salesMaterial.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            SalesServiceId = null,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _salesTaxRepository.Insert(salesTax);
                                    }
                                    else
                                    {
                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {

                                            Id = taxVM.Id,
                                            Amount = taxVM.TotalAmount,
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = salesMaterial.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            SalesServiceId = null,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            AddedBy = salesMaterial.AddedBy,
                                            AddedDate = salesMaterial.AddedDate,
                                            AddedFromIP = salesMaterial.AddedFromIP,
                                            UpdatedBy = salesMaterial.UpdatedBy,
                                            UpdatedDate = salesMaterial.UpdatedDate,
                                            UpdatedFromIP = salesMaterial.UpdatedFromIP,
                                            ModelState = ModelState.Modified
                                        };
                                        _salesTaxRepository.Update(salesTax);
                                    }
                                }
                            }

                        }

                        var firstCharacteristics = _firstCharacteristicsRepository.Find(salesMaterialVM.FirstCharacteristicsId);

                        if (firstCharacteristics != null)
                        {
                            var secondCharacteristics = _secondCharacteristicsRepository.Find(salesMaterialVM.SecondCharacteristicsId);
                            var thirdCharacteristics = _thirdCharacteristicsRepository.Find(salesMaterialVM.ThirdCharacteristicsId);


                            firstCharacteristics.SalesQty = firstCharacteristics.SalesQty - salesMaterialVM.TempSalesQty + salesMaterialVM.SalesQty;
                            _firstCharacteristicsRepository.Update(firstCharacteristics);


                            if (secondCharacteristics != null)
                            {
                                secondCharacteristics.SalesQty = secondCharacteristics.SalesQty - salesMaterialVM.TempSalesQty + salesMaterialVM.SalesQty;
                                _secondCharacteristicsRepository.Update(secondCharacteristics);
                            }

                            if (thirdCharacteristics != null)
                            {
                                thirdCharacteristics.SalesQty = thirdCharacteristics.SalesQty - salesMaterialVM.TempSalesQty + salesMaterialVM.SalesQty;
                                _thirdCharacteristicsRepository.Update(thirdCharacteristics);
                            }
                        }

                        if (dsItemScanData.Tables[0].Rows.Count > 0)
                        {
                            dsItemScanData.Tables[0].DefaultView.RowFilter = "SOId='" + salesMaterialVM.SalesOrderId + "' AND PackingId='" + salesMaterialVM.PackingId + "'";

                            for (int i = 0; i < dsItemScanData.Tables[0].DefaultView.Count; i++)
                            {
                                var childData = _ItemScanChildDataService.Find(dsItemScanData.Tables[0].DefaultView[i]["Id"].ToString());
                                childData.SalesMaterialId = salesMaterialVM.Id;
                                childData.SalesId = sales.Id;
                                childData.IsDespatch = true;
                                childData.ReturnNetWeight = 0;
                                childData.ModelState = ModelState.Modified;
                                _ItemScanChildDataService.Update(childData);
                            }

                        }
                    }
                }

                if (selectedPackingList != null)
                {
                    foreach (var item in selectedPackingList)
                    {
                        if (string.IsNullOrEmpty(item.Id))
                        {
                            currentSalesOrderItemId++;
                            var salesMaterialSo = new SalesPacking
                            {
                                Id = _pkGeneratorService.MakePK(sales.Id, currentSalesOrderItemId, 2),

                                PackingId = item.PackingId,
                                Qty = item.Qty,
                                Amount = item.Amount,
                                ProductLibraryId = item.ProductLibraryId,
                                SalesId = sales.Id,
                                ModelState = ModelState.Added,
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null
                            };
                            _salesPackingRepository.Insert(salesMaterialSo);
                        }
                        else
                        {
                            var salesMaterialSo = new SalesPacking
                            {
                                Id = item.Id,
                                PackingId = item.PackingId,
                                Qty = item.Qty,
                                Amount = item.Amount,
                                SalesId = sales.Id,
                                ProductLibraryId = item.ProductLibraryId,
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                UpdatedBy = sales.UpdatedBy,
                                UpdatedDate = sales.UpdatedDate,
                                UpdatedFromIP = sales.UpdatedFromIP,
                                ModelState = ModelState.Modified
                            };
                            _salesPackingRepository.Update(salesMaterialSo);
                        }
                    }
                }

                if (salesServiceVMList != null)
                {
                    foreach (var salesServiceVM in salesServiceVMList)
                    {

                        currentSalesServiceId++;
                        if (string.IsNullOrEmpty(salesServiceVM.Id))
                        {
                            var salesService = new Model.SalesManagements.SalesService
                            {
                                AddedBy = sales.AddedBy,
                                AddedDate = sales.AddedDate,
                                AddedFromIP = sales.AddedFromIP,
                                Amount = salesServiceVM.Amount,
                                BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                Id = _pkGeneratorService.MakePK(sales.Id, currentSalesServiceId, 2),
                                ModelState = ModelState.Added,
                                NetAmount = salesServiceVM.NetAmount,
                                SalesId = sales.Id,
                                ServiceMasterId = salesServiceVM.ServiceMasterId,
                                TaxAmount = salesServiceVM.TaxAmount,
                                BooksCurrencyTaxAmount = Math.Round(salesServiceVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                                UpdatedBy = null,
                                UpdatedDate = null,
                                UpdatedFromIP = null
                            };
                            _salesServiceRepository.Insert(salesService);

                            if (salesServiceVM.ServiceTaxList != null && salesServiceVM.ServiceTaxList.Count > 0)
                            {
                                foreach (var taxVM in salesServiceVM.ServiceTaxList)
                                {
                                    if (taxVM.TaxCategoryId == null)
                                        throw new CustomException("Please Select Tax Category !");

                                    currentSalesTaxId++;
                                    var salesTax = new SalesTax
                                    {
                                        Id = _pkGeneratorService.MakePK(salesService.Id, currentSalesTaxId, 2),
                                        AddedBy = salesService.AddedBy,
                                        AddedDate = salesService.AddedDate,
                                        AddedFromIP = salesService.AddedFromIP,
                                        Amount = taxVM.Amount,
                                        BooksCurrencyTransactionAmount = Math.Round(taxVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                        HSNCodeId = taxVM.HSNCodeId,
                                        Percentage = taxVM.Percentage,
                                        SalesId = sales.Id,
                                        SalesMaterialId = null,
                                        SalesServiceId = salesService.Id,
                                        TaxCategoryId = taxVM.TaxCategoryId,
                                        ModelState = ModelState.Added,
                                        UpdatedBy = null,
                                        UpdatedDate = null,
                                        UpdatedFromIP = null
                                    };
                                    _salesTaxRepository.Insert(salesTax);
                                }
                            }
                        }
                        else
                        {
                            var salesService = new Model.SalesManagements.SalesService
                            {
                                Id = salesServiceVM.Id,
                                Amount = salesServiceVM.Amount,
                                BooksCurrencyTransactionAmount = Math.Round(salesServiceVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                ModelState = ModelState.Modified,
                                NetAmount = salesServiceVM.NetAmount,
                                SalesId = sales.Id,
                                ServiceMasterId = salesServiceVM.ServiceMasterId,
                                TaxAmount = salesServiceVM.TaxAmount,
                                BooksCurrencyTaxAmount = Math.Round(salesServiceVM.TaxAmount * voucherVM.CompanyCurrencyRate, 2),
                                UpdatedBy = sales.UpdatedBy,
                                UpdatedDate = sales.UpdatedDate,
                                UpdatedFromIP = sales.UpdatedFromIP
                            };
                            _salesServiceRepository.Update(salesService);

                            if (salesServiceVM.ServiceTaxList != null && salesServiceVM.ServiceTaxList.Count > 0)
                            {
                                foreach (var taxVM in salesServiceVM.ServiceTaxList)
                                {

                                    if (string.IsNullOrEmpty(taxVM.Id))
                                    {
                                        if (taxVM.TaxCategoryId == null)
                                            throw new CustomException("Please Selete Tax Category !");

                                        currentSalesTaxId++;
                                        var salesTax = new SalesTax
                                        {
                                            Id = _pkGeneratorService.MakePK(salesServiceVM.Id, currentSalesTaxId, 2),
                                            AddedBy = sales.AddedBy,
                                            AddedDate = sales.AddedDate,
                                            AddedFromIP = sales.AddedFromIP,
                                            Amount = taxVM.TaxAmount,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.TotalAmount * voucherVM.CompanyCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = null,
                                            SalesServiceId = salesService.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            ModelState = ModelState.Added,
                                            UpdatedBy = null,
                                            UpdatedDate = null,
                                            UpdatedFromIP = null
                                        };
                                        _salesTaxRepository.Insert(salesTax);
                                    }
                                    else
                                    {
                                        var salesTax = new SalesTax
                                        {
                                            Id = taxVM.Id,
                                            Amount = taxVM.Amount,
                                            BooksCurrencyTransactionAmount = Math.Round(taxVM.Amount * voucherVM.CompanyCurrencyRate, 2),
                                            HSNCodeId = taxVM.HSNCodeId,
                                            Percentage = taxVM.Percentage,
                                            SalesId = sales.Id,
                                            SalesMaterialId = null,
                                            SalesServiceId = salesService.Id,
                                            TaxCategoryId = taxVM.TaxCategoryId,
                                            ModelState = ModelState.Modified,
                                            UpdatedBy = salesService.UpdatedBy,
                                            UpdatedDate = salesService.UpdatedDate,
                                            UpdatedFromIP = salesService.UpdatedFromIP
                                        };
                                        _salesTaxRepository.Update(salesTax);
                                    }
                                }
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void PackingSalesPost(VoucherViewModel voucherVM, IEnumerable<SalesMaterialViewModel> salesMaterialVMList
           , IEnumerable<SalesMaterialViewModel> salesMaterialDetailGLList
            , IEnumerable<SalesServiceViewModel> salesServiceDetailGLList
            , SalesPacking packing, IEnumerable<SalesMaterialViewModel> PackingDetailVMList, string packingVoucherTypeId)
        {
            var flag = false;
            try
            {
                #region Get Company Parallerl Currency Id

                AccountCommonExtensionService _accountsCommonService = new AccountCommonExtensionService();
                _accountsCommonService.GetParallelCurrency(voucherVM.CompanyId, out string companyCurrencyId, out string companyCurrencyCode);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);
                _accountsCommonService.CheckingTaxYearPeriod(voucherVM);
                _accountsCommonService.CheckingFiscalYearPeriod(voucherVM);

                #endregion Get Company Parallerl Currency Id

                _unitOfWork.BeginTransaction();
                flag = true;
                var sales = _salesRepository.Find(voucherVM.Id);
                sales.RowState = "Posted";
                voucherVM.IsPark = false;

                var invoice = new Invoice
                {
                    Amount = salesMaterialVMList.Where(r => r.OtherName == "Customer" && r.TrnType == "Dr").Sum(r => r.Amount),
                    BaseNoOfDays = voucherVM.BaseNoOfDays,
                    BaseOnDueDate = voucherVM.BaseOnDueDate,
                    CompanyGroupId = voucherVM.CompanyGroupId,
                    CompanyId = voucherVM.CompanyId,
                    CurrencyId = voucherVM.CurrencyId,
                    DocDate = voucherVM.DocDate,
                    DocRefNo = voucherVM.DocRefNo,
                    Narration = voucherVM.Narration,
                    EntityId = voucherVM.EntityId,
                    PlantId = voucherVM.PlantId,
                    IsExcludingTax = voucherVM.IsExcludingTax,
                    IsSplit = voucherVM.IsSplit,
                    PartyId = voucherVM.PartyId,
                    PartyPlantId = voucherVM.PartyPlantId,
                    PartyType = PartyType.Customer.ToString(),
                    EmployeeId = voucherVM.EmployeeId,
                    PaymentTermId = voucherVM.PaymentTermId,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    RevisedDueDate = voucherVM.MatureDate,
                    ActualDueDate = voucherVM.MatureDate,
                    VoucherTypeId = voucherVM.VoucherTypeId,
                    FiscalYearId = voucherVM.FiscalYearId,
                    FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                    TaxYearId = voucherVM.TaxYearId,
                    VoucherDate = DateTime.Now,
                    TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                    CompanyCurrencyRate = voucherVM.ToCurrencyRate,
                    IsPark = voucherVM.IsPark
                };
                _invoiceService.InsertInvoice(invoice);


                // INSERT INTO Voucher TABLE
                var voucher = new Voucher
                {
                    CompanyGroupId = invoice.CompanyGroupId,
                    CompanyId = invoice.CompanyId,
                    PlantId = invoice.PlantId,
                    EntityId = invoice.EntityId,
                    CurrencyId = invoice.CurrencyId,
                    FiscalYearId = invoice.FiscalYearId,
                    FiscalYearPeriodId = invoice.FiscalYearPeriodId,
                    TaxYearId = invoice.TaxYearId,
                    TaxYearPeriodId = invoice.TaxYearPeriodId,
                    AddedBy = invoice.AddedBy,
                    AddedDate = invoice.AddedDate,
                    AddedFromIP = invoice.AddedFromIP,
                    VoucherDate = invoice.VoucherDate,
                    DocDate = invoice.DocDate,
                    DocRefNo = invoice.DocRefNo,
                    Archive = invoice.Archive,
                    IsPark = invoice.IsPark,
                    Narration = voucherVM.Narration,
                    PostingDate = voucherVM.PostingDate,
                    SourceType = SourceType.SalesInvoice.ToString(),
                    VoucherTypeId = voucherVM.VoucherTypeId,
                };
                voucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + voucher.Id;
                _voucherService.InsertVoucher(voucher, voucherVM.FiscalYearPrefix);

                invoice.VoucherId = voucher.Id;
                sales.VoucherId = voucher.Id;

                _salesRepository.Update(sales);

                var currentInvoiceDetail = 0;
                var currentVoucherDetaiRecord = 0;
                var currentTaxRecord = 0;
                var totalAmountCr = 0.0M;
                var totalTaxCr = 0.0M;
                decimal totalAmountDr = 0;
                //sales Insert into Invoice and voucher
                //var salesTaxVMList = GetSalesTaxGroupTaxCategory(voucherVM.Id).Select().ToList();
                var invoiceTaxPk = _invoiceTaxService.GetMaxNumber();
                if (salesMaterialVMList != null)
                {
                    var invoiceDetail = new InvoiceDetail
                    {
                        AddedBy = invoice.AddedBy,
                        AddedDate = invoice.AddedDate,
                        AddedFromIP = invoice.AddedFromIP,
                        Archive = invoice.Archive
                    };
                    foreach (var voucherDetailVM in salesMaterialVMList)
                    {
                        if (string.IsNullOrEmpty(voucherDetailVM.GLGeneralInfoId))
                            throw new CustomException("Without GL can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.BudgetMasterId))
                            throw new CustomException("Without Budget can not post.");
                        if (string.IsNullOrEmpty(voucherDetailVM.ActivityId))
                            throw new CustomException("Without Activity can not post.");
                        if (voucherDetailVM.TrnType == "Dr")
                        {
                            if (voucherDetailVM.OtherName == "Customer")
                            {
                                invoiceDetail.GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId;
                                invoiceDetail.BudgetMasterId = voucherDetailVM.BudgetMasterId;
                                invoiceDetail.ActivityId = voucherDetailVM.ActivityId;
                                invoiceDetail.Amount = voucherDetailVM.Amount;
                                invoiceDetail.NetAmount = voucherDetailVM.Amount;
                                invoiceDetail.InvoiceId = invoice.Id;
                                InsertInvoiceDetail(invoice, invoiceDetail, currentInvoiceDetail);
                            }

                            var voucherDr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                DrAmount = voucherDetailVM.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = sales.Narration,

                                PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            if (voucherDetailVM.OtherName == "Customer")
                            {
                                voucherDr.InvoiceDetailId = invoiceDetail.Id;
                                voucherDr.PartyId = sales.PartyId;
                                voucherDr.PartyPlantId = sales.InvoicingPartyPlantId;
                                voucherDr.PartyType = voucherDetailVM.OtherName;
                            }
                            totalAmountDr += voucherDr.DrAmount;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherDr, currentVoucherDetaiRecord);
                            voucherDetailVM.VoucherDetailId = voucherDr.Id;




                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherDr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherDr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = sales.ToCurrencyRate,
                                ToCurrencyConversion = 1 / sales.ToCurrencyRate,
                                DrAmount = voucherDr.DrAmount * sales.ToCurrencyRate
                            });

                            if (voucherDetailVM.OtherName == "TaxReceivable" || voucherDetailVM.OtherName == "SVTaxReceivable" || voucherDetailVM.OtherName == "TCSReceivable")
                            {
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherDr.Id,
                                    VoucherId = voucher.Id,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucher.TaxYearId,
                                    TaxYearPeriodId = voucher.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxCodeId = voucherDetailVM.TaxCodeId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.SalesInvoice.ToString(),
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP
                                };
                                _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    Id = invoiceTax.Id + 1,
                                    InvoiceTaxId = invoiceTax.Id,
                                    Amount = invoiceTax.TaxAmount,
                                    GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Dr",
                                    AddedBy = invoiceTax.AddedBy,
                                    AddedDate = invoiceTax.AddedDate,
                                    AddedFromIP = invoiceTax.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            }
                        }
                        if (voucherDetailVM.TrnType == "Cr")
                        {
                            var voucherCr = new VoucherDetail
                            {
                                GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                ActivityId = voucherDetailVM.ActivityId,
                                CrAmount = voucherDetailVM.Amount,
                                CurrencyId = voucherVM.CurrencyId,
                                DocDate = voucherVM.DocDate,
                                DocRefNo = voucherVM.DocRefNo,
                                Narration = sales.Narration,
                                PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                AddedBy = voucher.AddedBy,
                                AddedDate = voucher.AddedDate,
                                AddedFromIP = voucher.AddedFromIP
                            };
                            totalAmountCr += voucherCr.CrAmount;
                            currentVoucherDetaiRecord++;
                            _voucherService.InsertVoucherDetail(voucher, voucherCr, currentVoucherDetaiRecord);
                            voucherDetailVM.VoucherDetailId = voucherCr.Id;
                            //_salesMaterialRepository.Update(voucherDetailVM);

                            if (voucherDetailVM.OtherName == "Sales")
                            {
                                foreach (var item in salesMaterialDetailGLList.Where(r => r.GLGeneralInfoId == voucherCr.GLGeneralInfoId
                           && r.BudgetMasterId == voucherCr.BudgetMasterId && r.ActivityId == voucherCr.ActivityId))
                                {
                                    if (item.SalesMaterialId != null)
                                    {
                                        var salesMaterial = _salesMaterialRepository.Find(item.SalesMaterialId);
                                        salesMaterial.VoucherDetailId = voucherCr.Id;
                                        salesMaterial.ModelState = ModelState.Modified;
                                        salesMaterial.PostCrGLGeneralInfoId = voucherCr.GLGeneralInfoId;
                                        salesMaterial.PostCrBudgetMasterId = voucherCr.BudgetMasterId;
                                        salesMaterial.PostCrActivityId = voucherCr.ActivityId;
                                        salesMaterial.PostDrGLGeneralInfoId = invoiceDetail.GLGeneralInfoId;
                                        salesMaterial.PostDrBudgetMasterId = invoiceDetail.BudgetMasterId;
                                        salesMaterial.PostDrActivityId = invoiceDetail.ActivityId;
                                        AuditService.UpdatedLog(salesMaterial);
                                        _salesMaterialRepository.Update(salesMaterial);
                                    }

                                }
                            }

                            if (voucherDetailVM.OtherName == "Service")
                            {
                                foreach (var item in salesServiceDetailGLList.Where(r => r.GLGeneralInfoId == voucherCr.GLGeneralInfoId
                           && r.BudgetMasterId == voucherCr.BudgetMasterId && r.ActivityId == voucherCr.ActivityId))
                                {
                                    if (item.SalesServiceId != null)
                                    {
                                        var salesService = _salesServiceRepository.Find(item.SalesServiceId);
                                        salesService.VoucherDetailId = voucherCr.Id;
                                        salesService.ModelState = ModelState.Modified;
                                        AuditService.UpdatedLog(salesService);
                                        _salesServiceRepository.Update(salesService);
                                    }

                                }
                            }

                            _voucherService.InsertVoucherDetailCompanyCurrency(voucherCr, new VoucherDetailCurrency
                            {
                                ParallelCurrencyId = companyCurrencyId,
                                FromCurrencyId = voucherCr.CurrencyId,
                                ToCurrencyId = companyCurrencyId,
                                ToCurrencyRate = sales.ToCurrencyRate,
                                ToCurrencyConversion = 1 / sales.ToCurrencyRate,
                                CrAmount = voucherCr.CrAmount * sales.ToCurrencyRate
                            });

                            if (voucherDetailVM.OtherName == "TaxPayable" || voucherDetailVM.OtherName == "SVTaxPayable" || voucherDetailVM.OtherName == "TCSPayable")
                            {
                                currentTaxRecord++;
                                var invoiceTax = new InvoiceTax
                                {
                                    Archive = false,
                                    VoucherDetailId = voucherCr.Id,
                                    VoucherId = voucher.Id,
                                    InvoiceId = invoice.Id,
                                    TaxYearId = voucher.TaxYearId,
                                    TaxYearPeriodId = voucher.TaxYearPeriodId,
                                    TaxCategoryId = voucherDetailVM.TaxCategoryId,
                                    TaxCodeId = voucherDetailVM.TaxCodeId,
                                    TaxAmount = voucherDetailVM.Amount,
                                    TaxAutoAmount = 0,
                                    PartyId = voucherVM.PartyId,
                                    SourceType = SourceType.SalesInvoice.ToString(),
                                    AddedBy = voucher.AddedBy,
                                    AddedDate = voucher.AddedDate,
                                    AddedFromIP = voucher.AddedFromIP
                                };
                                _invoiceTaxService.InsertInvoiceTax(invoice, invoiceTax, invoiceTaxPk);
                                var invoiceTaxDetail = new InvoiceTaxDetail
                                {
                                    Id = invoiceTax.Id + 1,
                                    InvoiceTaxId = invoiceTax.Id,
                                    Amount = invoiceTax.TaxAmount,
                                    GLGeneralInfoId = voucherDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = voucherDetailVM.BudgetMasterId,
                                    ActivityId = voucherDetailVM.ActivityId,
                                    AType = "Cr",
                                    AddedBy = invoiceTax.AddedBy,
                                    AddedDate = invoiceTax.AddedDate,
                                    AddedFromIP = invoiceTax.AddedFromIP
                                };
                                _invoiceTaxDetailRepository.Insert(invoiceTaxDetail);
                            }
                        }
                    }
                }

                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();


                if (PackingDetailVMList != null && PackingDetailVMList.Where(a => a.TrnType == "Dr").Sum(r => r.Amount) > 0)
                {
                    DataSet _drvDetailData = null;
                    DataSet _drvDetailCurrencyData = null;
                    DataSet _crvDetailData = null;
                    DataSet _crvDetailCurrencyData = null;

                    var totalPackingAmountDr = 0.0M;
                    var totalPackingAmountCr = 0.0M;
                    // INSERT INTO Voucher TABLE
                    var packingvoucher = new Voucher
                    {
                        CompanyGroupId = voucherVM.CompanyGroupId,
                        CompanyId = voucherVM.CompanyId,
                        PlantId = voucherVM.PlantId,
                        EntityId = voucherVM.EntityId,
                        CurrencyId = companyCurrencyId,//voucherVM.CurrencyId,
                        FiscalYearId = voucherVM.FiscalYearId,
                        FiscalYearPeriodId = voucherVM.FiscalYearPeriodId,
                        TaxYearId = voucherVM.TaxYearId,
                        TaxYearPeriodId = voucherVM.TaxYearPeriodId,
                        AddedBy = packing.AddedBy,
                        AddedDate = packing.AddedDate,
                        AddedFromIP = packing.AddedFromIP,
                        VoucherDate = voucherVM.VoucherDate,
                        DocDate = voucherVM.DocDate,
                        DocRefNo = voucherVM.DocRefNo,
                        IsPark = voucherVM.IsPark,
                        Narration = voucherVM.Narration,
                        PostingDate = voucherVM.PostingDate,
                        SourceType = SourceType.PackingJournal.ToString(),
                        VoucherTypeId = packingVoucherTypeId,
                    };
                    packingvoucher.TransactionRefNo = DateTime.Now.Year.ToString().Substring(2) + packingvoucher.Id;
                    //_voucherService.InsertVoucher(packingvoucher, voucherVM.FiscalYearPrefix);
                    _accountsCommonService.InsertVoucher(packingvoucher, voucherVM.FiscalYearPrefix, out DataSet _vdataset);


                    if (PackingDetailVMList != null)
                    {
                        foreach (var packingDetailVM in PackingDetailVMList)
                        {
                            if (string.IsNullOrEmpty(packingDetailVM.GLGeneralInfoId))
                                throw new CustomException("Without GL can not post.");
                            if (string.IsNullOrEmpty(packingDetailVM.BudgetMasterId))
                                throw new CustomException("Without Budget can not post.");
                            if (string.IsNullOrEmpty(packingDetailVM.ActivityId))
                                throw new CustomException("Without Activity can not post.");
                            if (packingDetailVM.TrnType == "Dr")
                            {
                                var voucherPackingDr = new VoucherDetail
                                {
                                    GLGeneralInfoId = packingDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = packingDetailVM.BudgetMasterId,
                                    ActivityId = packingDetailVM.ActivityId,
                                    DrAmount = packingDetailVM.Amount,
                                    CurrencyId = companyCurrencyId,// voucherVM.CurrencyId,
                                    DocDate = voucherVM.DocDate,
                                    DocRefNo = voucherVM.DocRefNo,
                                    Narration = voucherVM.Narration,

                                    AddedBy = packingvoucher.AddedBy,
                                    AddedDate = packingvoucher.AddedDate,
                                    AddedFromIP = packingvoucher.AddedFromIP
                                };
                                currentVoucherDetaiRecord++;
                                totalPackingAmountDr += voucherPackingDr.DrAmount;
                                //_voucherService.InsertVoucherDetail(packingvoucher, voucherPackingDr, currentVoucherDetaiRecord);
                                packingDetailVM.VoucherDetailId = voucherPackingDr.Id;
                                _accountsCommonService.InsertVoucherDetail(packingvoucher, voucherPackingDr, currentVoucherDetaiRecord, ref _drvDetailData);

                                //_voucherService.InsertVoucherDetailCompanyCurrency(voucherPackingDr, new VoucherDetailCurrency
                                //{
                                _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherPackingDr, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherPackingDr.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = 1,//sales.ToCurrencyRate,
                                    ToCurrencyConversion = 1,// / sales.ToCurrencyRate,
                                    DrAmount = voucherPackingDr.DrAmount //* sales.ToCurrencyRate
                                }, ref _drvDetailCurrencyData);
                            }
                            if (packingDetailVM.TrnType == "Cr")
                            {
                                var voucherPackingCr = new VoucherDetail
                                {
                                    GLGeneralInfoId = packingDetailVM.GLGeneralInfoId,
                                    BudgetMasterId = packingDetailVM.BudgetMasterId,
                                    ActivityId = packingDetailVM.ActivityId,
                                    CrAmount = packingDetailVM.Amount,
                                    CurrencyId = companyCurrencyId,//voucherVM.CurrencyId,
                                    DocDate = voucherVM.DocDate,
                                    DocRefNo = voucherVM.DocRefNo,
                                    Narration = sales.Narration,
                                    PostingWithoutTaxAllow = invoice.IsExcludingTax,
                                    AddedBy = packingvoucher.AddedBy,
                                    AddedDate = packingvoucher.AddedDate,
                                    AddedFromIP = packingvoucher.AddedFromIP
                                };
                                totalPackingAmountCr += voucherPackingCr.CrAmount;
                                currentVoucherDetaiRecord++;
                                //_voucherService.InsertVoucherDetail(packingvoucher, voucherPackingCr, currentVoucherDetaiRecord);
                                packingDetailVM.VoucherDetailId = voucherPackingCr.Id;
                                _accountsCommonService.InsertVoucherDetail(packingvoucher, voucherPackingCr, currentVoucherDetaiRecord, ref _crvDetailData);


                                //_voucherService.InsertVoucherDetailCompanyCurrency(voucherPackingCr, new VoucherDetailCurrency
                                //{
                                _accountsCommonService.InsertVoucherDetailCompanyCurrency(voucherPackingCr, new VoucherDetailCurrency
                                {
                                    ParallelCurrencyId = companyCurrencyId,
                                    FromCurrencyId = voucherPackingCr.CurrencyId,
                                    ToCurrencyId = companyCurrencyId,
                                    ToCurrencyRate = 1,// sales.ToCurrencyRate,
                                    ToCurrencyConversion = 1, /// sales.ToCurrencyRate,
                                    CrAmount = voucherPackingCr.CrAmount// * sales.ToCurrencyRate
                                }, ref _crvDetailCurrencyData);

                            }
                        }
                    }

                    ConnectionManager.DAL.ConManager objCon;
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    DataSet dsBillMaster;
                    objCon.OpenDataSetThroughAdapter("select * from dbo.SalesPacking Where SalesId='" + packing.SalesId + "'", out dsBillMaster, false, "1");

                    for (int i = 0; i < dsBillMaster.Tables.Count; i++)
                    {
                        DataView dv = new DataView(dsBillMaster.Tables[i]);
                        dv.RowFilter = "Id='" + dsBillMaster.Tables[i].Rows[i]["Id"] + "'";

                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();

                            drmo["VoucherId"] = packingvoucher.Id;
                            drmo["UpdatedBy"] = packingvoucher.AddedBy;
                            drmo["UpdatedDate"] = DateTime.Now.ToString();
                            drmo["UpdatedFromIP"] = packingvoucher.AddedFromIP;
                            drmo.EndEdit();
                        }
                    }

                    if (totalPackingAmountDr != totalPackingAmountCr)
                        throw new CustomException("Dr and Cr amount is not equal.");
                    clsStaticInfo objApp = new clsStaticInfo();
                    objApp.SaveDataSets(_vdataset, _drvDetailData, _drvDetailCurrencyData, _crvDetailData, _crvDetailCurrencyData, dsBillMaster
                        );
                }

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



        public void DeleteSale(string invoiceId, string voucherId)
        {
            var flag = false;
            try
            {

                _unitOfWork.BeginTransaction();
                flag = true;
                var voucher = _voucherService.FindVoucher(voucherId);
                if (voucher.IsPark == false)
                    throw new CustomException("Delete is not allow after post ! ");

                var voucherdetail = _voucherService.QueryVoucherDetail(voucherId).Select().ToList();
                var voucherdetailcurrnecy = _voucherService.QueryVoucherDetailCurrency(voucherId).Select().ToList();
                // var invoice = base.Find(invoiceId);
                var invoiceDetail = _invoiceDetailRepository.Query(r => r.InvoiceId == invoiceId).Select().ToList();
                var invoiceTax = _invoiceTaxRepository.Query(r => r.InvoiceId == invoiceId).Select().ToList();
                // var invoiceTDS = _additionalTaxRepository.Query(r => r.InvoiceId == invoiceId).Select().ToList();
                foreach (var item in voucherdetailcurrnecy)
                {
                    _voucherService.DeleteVoucherDetailCurrency(item.Id);
                }
                if (invoiceTax != null)
                {
                    foreach (var item in invoiceTax)
                    {
                        var rdBuilder = new System.Text.StringBuilder();
                        var builderSql = @"UPDATE [TRN].InvoiceTax SET VoucherDetailId=NULL WHERE Id='" + item.Id + "'";
                        rdBuilder.Append(builderSql);
                        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                    }
                }

                //if (invoiceTDS.Count > 0)
                //{
                //    foreach (var item in invoiceTDS)
                //    {
                //        var rdBuilder = new System.Text.StringBuilder();
                //        var builderSql = @"DELETE [TRN].AdditionalTaxDetail  WHERE AdditionalTaxId='" + item.Id + "'";
                //        rdBuilder.Append(builderSql);
                //        _sqlRepository.ExecuteSqlCommand(rdBuilder.ToString());
                //        _additionalTaxRepository.Delete(item.Id);
                //    }
                //}

                foreach (var item in voucherdetail)
                {
                    var gltransaction = _voucherService.QueryGLTransactionDetail(item.Id).Select().ToList();
                    if (gltransaction.Count > 0)
                    {
                        foreach (var item1 in gltransaction)
                        {
                            _voucherService.DeleteGLTransactionDetail(item1.Id);

                        }

                    }
                    _voucherService.DeleteVoucherDetail(item.Id);
                }
                if (invoiceTax != null)
                {
                    foreach (var item in invoiceTax)
                    {
                        var invoicetaxDdetail = _invoiceTaxDetailRepository.Query(r => r.InvoiceTaxId == item.Id).Select().ToList();
                        foreach (var item1 in invoicetaxDdetail)
                        {
                            _invoiceTaxDetailRepository.Delete(item1.Id);
                        }
                        _invoiceTaxRepository.Delete(item.Id);
                    }
                }
                foreach (var item in invoiceDetail)
                {
                    _invoiceDetailRepository.Delete(item.Id);
                }
                //base.Delete(invoiceId);
                _voucherService.DeleteVoucher(voucher.Id);
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
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Accounts.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }
    }


}